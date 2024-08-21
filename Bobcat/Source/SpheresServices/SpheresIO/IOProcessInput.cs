#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.Log;
using EFS.Import;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.PosRequestInformation.Import;
using EFS.Process;
using EFS.SpheresIO.MarketData;
using EFS.SpheresIO.MarketData.T7RDF;
using EFS.SpheresIO.PosRequest;
using EFS.SpheresIO.Trade;
using EFS.TradeInformation.Import;
//
using EfsML.Business;
using EfsML.DynamicData;
//
using FpML.Interface;
using static EFS.ACommon.Cst;
#endregion

namespace EFS.SpheresIO
{

    /// <summary>
    /// 
    /// </summary>
    public class ProcessInput : ProcessInOut
    {
        /// <summary>
        ///  Type d'importation 
        /// </summary>
        public enum ImportTypeEnum
        {
            /// <summary>
            /// Importation dans des tables SQL
            /// </summary>
            Table,
            /// <summary>
            /// Importation de trades (Trade de marché, debt security,  tarde risk...)
            /// </summary>
            Trade,
            /// <summary>
            /// Importation ds POSREQUEST
            /// </summary>
            PosRequest,
            /// <summary>
            /// 
            /// </summary>
            NA
        }

        #region Members
        private Cst.InputSourceDataStyle inputSourceDataStyle;
        private bool _isIOXmlOverridesExtended;
        //
        private int m_NbSourceTotalLines;
        private int m_NbParsingIgnoredLines;
        //
        private SqlOrder m_SqlOrder;
        //
        private int m_NbRejectedRows;
        private List<IOTrackDetail> m_IOTrackDetails;

        #endregion

        #region accessors
        /// <summary>
        /// 
        /// </summary>
        protected override bool IsIOXmlOverridesExtended
        {
            get { return (_isIOXmlOverridesExtended); }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override string ElementIdentifiersForMessage
        {
            get
            {
                // A compléter par style en cas de besoins
                return m_DataStyle;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pIdIoTaskDet"></param>
        /// <param name="pIdIOInput"></param>
        /// <param name="pRetCodeOnNoData"></param>
        /// <param name="pRetCodeOnNoDataModif"></param>
        public ProcessInput(Task pTask, int pIdIoTaskDet, string pIdIOInput,
            Cst.IOReturnCodeEnum pRetCodeOnNoData, Cst.IOReturnCodeEnum pRetCodeOnNoDataModif)
            : base(pTask, pIdIoTaskDet, pIdIOInput, true, pRetCodeOnNoData, pRetCodeOnNoDataModif) { }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Parsing des données 
        /// </summary>
        /// FI 20140617 [19911] Add EUREXPRISMA_STLPRICESFILE
        /// PM 20150707 [21104] Add SPANXMLRISKPARAMETERFILE
        /// FI 20150917 [19081] Modify
        // EG 20180423 Analyse du code Correction [CA2200]
        /// PM 20171212 [23646] Add T7RDFFIXMLFILE
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void ReadingFromSource()
        {
            try
            {
                inputSourceDataStyle = (Cst.InputSourceDataStyle)Enum.Parse(typeof(Cst.InputSourceDataStyle), m_DataStyle, true);

                //Charger les parsings pour m_IdIOInOut
                InputParsing inputParsing = new InputParsing(m_IdIOInOut, m_Task);
                if (inputParsing.ParsingCount == 0)
                {
                    throw new Exception(
                        "<b>No enabled parsing specified for input</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
                }
                //    
                #region Parsing ( Résultat : postParsingFlow )

                // RD 20100111 [16818] MS Excel® file import
                bool isData = ((inputSourceDataStyle == Cst.InputSourceDataStyle.UNICODEDATA) ||
                    (inputSourceDataStyle == Cst.InputSourceDataStyle.ANSIDATA) ||
                    (inputSourceDataStyle == Cst.InputSourceDataStyle.XMLDATA));

                bool isExcelFile = (inputSourceDataStyle == Cst.InputSourceDataStyle.EXCELFILE);
                bool isXMLFile = (inputSourceDataStyle == Cst.InputSourceDataStyle.XMLFILE);
                bool isXMLData = (inputSourceDataStyle == Cst.InputSourceDataStyle.XMLDATA);

                string dataSourceFlow = string.Empty;
                if (isData)
                {
                    if (m_Task.m_IOMQueue.dataSpecified)
                        dataSourceFlow = m_Task.m_IOMQueue.data.Value;
                    else
                        throw new InvalidDataException("Message queue does not contains data element");
                }

                IOTaskDetInOutFile postParsingFlow = null;

                switch (inputSourceDataStyle)
                {
                    #region UNICODEDATA, ANSIDATA, UNICODEFILE, ANSIFILE, EUREXMARKETDATAFILE, EUREXTHEORETICALPRICEFILE_QUOTE, EUREXTHEORETICALPRICEFILE_RISKARRAY, LONDONSPANTHEORETICALPRICEFILE, SPANTHEORETICALPRICEFILE, CBOETHEORETICALPRICEFILE, OMXNORDICFMSFILE, OMXNORDICVCTFILE
                    case Cst.InputSourceDataStyle.UNICODEDATA:
                    case Cst.InputSourceDataStyle.ANSIDATA:
                    case Cst.InputSourceDataStyle.UNICODEFILE:
                    case Cst.InputSourceDataStyle.ANSIFILE:
                    case Cst.InputSourceDataStyle.CLEARNETMARKETDATAFILE:
                    case Cst.InputSourceDataStyle.ICEFUTURESEUROPEMARKETDATAFILE:
                    case Cst.InputSourceDataStyle.EUREXMARKETDATAFILE:
                    case Cst.InputSourceDataStyle.EUREXTHEORETICALPRICEFILE_QUOTE:
                    case Cst.InputSourceDataStyle.EUREXTHEORETICALPRICEFILE_RISKARRAY:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_LIQUIDITYFACTORSFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_MARKETCAPACITIESFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_FXRATESFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE:
                    case Cst.InputSourceDataStyle.SPANLIFFEMARKETDATAFILE:
                    case Cst.InputSourceDataStyle.LONDONSPANTHEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.SPANTHEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.SPANRISKPARAMETERMATDATEFILE:
                    case Cst.InputSourceDataStyle.SPANXMLRISKPARAMETERFILE:
                    case Cst.InputSourceDataStyle.CBOETHEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.NASDAQOMXFMSFILE:
                    case Cst.InputSourceDataStyle.NASDAQOMXVCTFILE:
                    case Cst.InputSourceDataStyle.MEFFASSETRISKDATAFILE:
                    case Cst.InputSourceDataStyle.MEFFCONTRACTASSETFILE:
                    case Cst.InputSourceDataStyle.T7RDFFIXMLFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYVARREFERENTIALDATAFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMVARREFERENTIALDATAFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYEXPIRYPRICESFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMEXPIRYPRICESFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYOPTIONSDELTASFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMOPTIONSDELTASFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYSTOCKINDICESVALUESFILE:
                        // PM 20240122 [WI822] Add EURONEXTLEGACYEQYVARREFERENTIALDATAFILE, EURONEXTLEGACYCOMVARREFERENTIALDATAFILE, EURONEXTLEGACYEQYEXPIRYPRICESFILE, EURONEXTLEGACYCOMEXPIRYPRICESFILE, EURONEXTLEGACYEQYOPTIONSDELTASFILE, EURONEXTLEGACYCOMOPTIONSDELTASFILE, EURONEXTLEGACYSTOCKINDICESVALUESFILE

                        // RD 20100111 [16818] MS Excel® file import
                        #region Charger les lignes une par une dans postParsingFlow
                        bool isFileOnError = false;
                        postParsingFlow = new IOTaskDetInOutFile();

                        try
                        {
                            if (isData && m_Task.m_IOMQueue.dataSpecified)
                            {
                                #region IOMQueue Data
                                IOTools.InitPostParsingFlow(false, dataSourceFlow, m_DataStyle, ref postParsingFlow);
                                // PM 20180219 [23824] IOTools => IOCommonTools
                                //IOTools.OpenStringReader(dataSourceFlow);
                                IOCommonTools.OpenStringReader(dataSourceFlow);

                                // RD 20150601 [21054][20816] La Parsing du flux Data n'est pas effectué
                                LoadLinesFromSource(true, inputParsing, ref postParsingFlow, ref isFileOnError);
                                #endregion
                            }
                            else
                            {
                                #region File
                                // PM 20180219 [23824] VerifyFilePathForImport déplacée dans IOTaskTools
                                //string dataName = this.DataName;
                                //VerifyFilePathForImport(ref dataName);
                                //this.DataName = dataName;
                                this.DataName = IOTaskTools.VerifyFilePathForImport(m_Task, this.DataName, m_MsgLogInOut);

                                // FI 20131021 [17861] appel à DirectImport ou PostParsingFlowFromFile
                                if (IsFileDirectImport(inputSourceDataStyle))
                                {
                                    if (IsFileDirectImportOk(inputSourceDataStyle))
                                    {
                                        DirectImport(inputSourceDataStyle, inputParsing);
                                    }
                                }
                                else
                                {
                                    PostParsingFlowFromFile(inputParsing, ref postParsingFlow, ref isFileOnError);
                                }
                                #endregion File
                            }
                        }
                        catch (Exception) { throw; }
                        finally
                        {
                            // PM 20180219 [23824] IOTools => IOCommonTools
                            if (isData)
                            {
                                //IOTools.CloseStream();
                                IOCommonTools.CloseStream();
                            }
                            else
                            {
                                //IOTools.CloseFile(m_DataName);
                                IOCommonTools.CloseFile(m_DataName);
                            }
                        }
                        #endregion Charger les lignes

                        UpdatePostParsingFlowStatus(isFileOnError, ref postParsingFlow);

                        break;
                    #endregion

                    case Cst.InputSourceDataStyle.EXCELFILE:
                        PostParsingFlowFromExcel(inputParsing, ref postParsingFlow);
                        break;

                    case Cst.InputSourceDataStyle.XMLDATA:
                    case Cst.InputSourceDataStyle.XMLFILE:
                        PostParsingFlowFromXMLData(isData, dataSourceFlow, inputParsing.GetXSLParsingFile(), ref postParsingFlow);
                        break;

                    #region default
                    default:
                        throw new Exception(
                            "<b>Input data style (" + m_DataStyle + ") is not supported</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
                        #endregion
                }

                TaskPostParsing.taskDet.input.file = postParsingFlow;

                #region AddPostParsingFileInLog
                // FI 20131021 [17861] use methode IsFileDirectImport
                //if (inputSourceDataStyle != Cst.InputSourceDataStyle.EUREXTHEORETICALPRICEFILE_RISKARRAY)
                if (false == IsFileDirectImport(inputSourceDataStyle))
                {
                    bool isStatusError = (ProcessStateTools.StatusError.ToLower() == TaskPostParsing.taskDet.input.file.status);
                    // FI 20200706 [XXXXX] SetErrorWarning si error
                    if (isStatusError)
                        ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    AddPostParsingFileInLog(isStatusError);
                }
                #endregion


                if ((postParsingFlow.row != null) && ArrFunc.IsAllElementEmpty(postParsingFlow.row))
                    PostParsingNoData();

                #endregion Parsing
            }
            catch (Exception ex)
            {
                /* FI 20200706 [XXXXX] Mise en commentaire non nécessaire du fait du throw
                // FI 20200623 [XXXXX] SetErrorWarning
                m_Task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);   // FI 20190829 [XXXXX] Ecriture dans le log Spheres du message d'erreur
                */
                string msg = ExceptionTools.GetMessageExtended(ex);

                /* FI 20200706 [XXXXX] Mise en commentaire non nécessaire du fait du throw
                
                m_ElementStatus = ProcessStateTools.StatusEnum.ERROR;
                */
                Logger.Log(new LoggerData(LogLevelEnum.Error, msg, 3));

                // FI 20190829 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6003), 3, new LogParam("Reading")));

                // FI 20190829 [XXXXX] Generation de l'exception Error on Reading pour faire comme dans writing (voir "Error on writing")
                throw new Exception("Error on Reading");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void PrepareWriting()
        {
            #region IOTrack
            m_IOTrackDataSourceIdent = m_DataStyle;
            m_IOTrackDataTargetIdent = "N/A";
            m_IOTrackDescription = "Import of data";

            m_NbRejectedRows = 0;
            m_IOTrackDetails = new List<IOTrackDetail>();
            #endregion

            m_SqlOrder = null;
            m_IsWritingError = false;

            // ouverte de la connexion pour la (les) transactions (futures)
            if (m_CommitMode != Cst.CommitMode.AUTOCOMMIT)
                m_DbConnection = DataHelper.OpenConnection(m_Task.Cs);

            // ouverte de la transaction lorsque si FULLCOMMIT
            if (m_CommitMode == Cst.CommitMode.FULLCOMMIT)
                BeginInputTran();

            base.PrepareWriting();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsOk"></param>
        /// <param name="pIsPostMappingRowFilled"></param>
        /// RD 20110516/ Correction sur le "/" entre le folder et fileName 
        /// RD 20120830 [18102] Gestion des compteurs IOTRACK            
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel FinalizeWriting(bool pIsOk, bool pIsPostMappingRowFilled)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                // RD 20120621 [17974]
                // Vérifier si des données de la BD sont modifiées
                bool noDataModification = pIsPostMappingRowFilled;

                if (noDataModification)
                {
                    if ((true == pIsOk) || m_CommitMode != Cst.CommitMode.FULLCOMMIT)
                    {
                        int totalDataModification =
                            (from detail in m_IOTrackDetails
                             select (detail.InsertedRows + detail.UpdatedRows + detail.DeletedRows)
                            ).Sum();
                        noDataModification = (totalDataModification == 0);
                    }
                }

                if (noDataModification)
                {

                    ProcessStateTools.StatusEnum status = ProcessStateTools.StatusNoneEnum;

                    //FI 20120129 lorsque les messages sont issus de la gateway, la tâche IO ne doit pas générer un warning si aucune action a été entreprise par la tâche
                    //if (m_Task.m_IOMQueue.IsGatewayMqueue)
                    //    status = ProcessStateTools.StatusNoneEnum;

                    // RD 20130620 [18702] 
                    switch (m_RetCodeOnNoDataModif)
                    {
                        case Cst.IOReturnCodeEnum.ERROR:
                            // FI 20200706 [XXXXX] use ProcessState
                            //m_ElementStatus = ProcessStateTools.StatusErrorEnum;
                            ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                            status = ProcessStateTools.StatusErrorEnum;
                            ret = Cst.ErrLevel.DATANOTFOUND;
                            pIsOk = false;
                            break;
                        case Cst.IOReturnCodeEnum.WARNING:
                            // FI 20200706 [XXXXX] use ProcessState
                            //m_ElementStatus = ProcessStateTools.StatusWarningEnum;
                            ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            status = ProcessStateTools.StatusWarningEnum;
                            break;
                    }

                    /* FI 20200706 [XXXXX] Mise en commenataire
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_Task.process.ProcessState.SetErrorWarning(status);
                    */

                    
                    Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(status), new SysMsgCode(SysCodeEnum.LOG, 6032), 2));
                }

                FinalizeIOTrack(pIsOk);

                base.FinalizeWriting(pIsOk, pIsPostMappingRowFilled);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (m_CommitMode == Cst.CommitMode.FULLCOMMIT)
                {
                    if (pIsOk)
                        CommitInputTran();
                    else
                        RollbackInputTran();
                }

                if (null != m_DbConnection)
                    DataHelper.CloseConnection(m_DbConnection);
            }

            return ret;

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPassNumber"></param>
        /// <param name="pPassMessage"></param>
        /// <returns></returns>
        /// RD 20120830 [18102] Gestion des compteurs IOTRACK
        /// FI 20180319 [XXXXX] Modify
        /// EG 20180423 Analyse du code Correction [CA2200]
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// FI 20201119 [XXXX] Refactoring (Plusieurs tentatives si deadlock ou timeout)
        /// EG 20220221 [XXXXX] Gestion IRQ
        protected override bool Writing(int pPassNumber, string pPassMessage)
        {
            bool ret = true;
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            try
            {
                IOProcessImportCache.Clear();

                if (ArrFunc.IsFilled(m_TaskPostMapping.taskDet.input.file.row))
                {
                    foreach (IOTaskDetInOutFileRow row in m_TaskPostMapping.taskDet.input.file.row)
                    {
                        if (IRQTools.IsIRQRequested(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref codeReturn))
                            return false;

                        // FI 20131122 [19233] appel à BuildLogMsgDetail
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, BuildLogMsgDetail(row, "<b>Start row import...</b>"), 3));

                        try
                        {
                            SetIOXmlOverridesExtended(row);

                            if (m_CommitMode == Cst.CommitMode.RECORDCOMMIT)
                            {
                                // FI 20210128 [XXXXX] call TryMultiple2
                                TryMultiple2(row, pPassMessage);
                            }
                            else
                            {
                                RowProcess(row, pPassMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            m_IsWritingError = true;

                            switch (m_CommitMode)
                            {
                                case Cst.CommitMode.FULLCOMMIT:
                                    throw;
                                case Cst.CommitMode.AUTOCOMMIT:
                                case Cst.CommitMode.RECORDCOMMIT:
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    // FI 20200706 [XXXXX] Mise en commentaire => non nécessaire du fait de  m_IsWritingError = true;
                                    //m_Task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                    string msg = ExceptionTools.GetMessageExtended(ex);
                                    
                                    // FI 20200706 [XXXXX] Mise en commentaire => non nécessaire du fait de  m_IsWritingError = true;
                                    //m_ElementStatus = ProcessStateTools.StatusEnum.ERROR;
                                    Logger.Log(new LoggerData(LogLevelEnum.Error, BuildLogMsgDetail(row, pPassMessage + Cst.Space + msg), 3));

                                    // FI 20190724 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                                    AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));
                                    break;
                            }
                        }
                        finally
                        {
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Debug, row.LogRowDesc + Cst.CrLf + "<b>End row import</b>", 3));
                        }
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                m_IsWritingError = true;

                // FI 20190724 [XXXXX] Pas de SpheresException dans le log 
                //=> sans intérêt puisque tjs tronqué généralement
                
                // FI 20200706 [XXXXX] Mise en commentaire puisque m_IsWritingError = true;
                //m_Task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // FI 20190724 [XXXXX] Ecriture dans le log Spheres du message d'erreur
                string msg = ExceptionTools.GetMessageExtended(ex);
                
                // FI 20200706 [XXXXX] Mise en commentaire puisque m_IsWritingError = true;
                //m_ElementStatus = ProcessStateTools.StatusEnum.ERROR;
                Logger.Log(new LoggerData(LogLevelEnum.Error, msg, 3));

                // FI 20190724 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));

                ret = false;
            }
            finally
            {
                // FI 20180319 [XXXXX] 
                AppInstance.TraceManager.TraceTimeSummaryAll(string.Empty);
            }

            return ret;
        }

        /// <summary>
        /// statut final d'un row post mapping après son traitement
        /// </summary>
        /// FI 20201221 [XXXXX] Add 
        ProcessStateTools.StatusEnum currentRowStatus;
        /// <summary>
        /// Mis à jour de currentRowStatus si Error ou Warning
        /// </summary>
        /// <param name="pStatus"></param>
        /// FI 20201221 [XXXXX] Add Method
        private void SetErrorWarningCurrentRow(ProcessStateTools.StatusEnum pStatus)
        {
            if ((false == ProcessStateTools.IsStatusError(currentRowStatus)) && ProcessStateTools.IsStatusErrorWarning(pStatus))
                currentRowStatus = pStatus;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// RD 20110401 [17379] Serialize mode : Add column SERIALIZEMODE
        // EG 20180426 Analyse du code Correction [CA2202]
        protected override IDataReader GetInOutData()
        {
            // PM 20180219 [23824] Partage de la requête
            //// RD 20100111 [16818] MS Excel® file import
            //string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf;
            //sqlQuery += SQLCst.SELECT + "DISPLAYNAME, DESCRIPTION, LOGLEVEL, COMMITMODE, DATASTYLE, DATACONNECTION, DATANAME, ISVERTICAL, ISHETEROGENOUS, ";
            //sqlQuery += "DATASECTIONSTART, DATASECTIONEND, NBOMITROWSTART, NBOMITROWEND, XSLMAPPING, NBCOLUMNBYROWMAP, SERIALIZEMODE, DATATARGETDESC" + Cst.CrLf;
            //sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOINPUT + Cst.CrLf;
            //sqlQuery += SQLCst.WHERE + " IDIOINPUT = " + DataHelper.SQLString(m_IdIOInOut) + Cst.CrLf;
            //sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(m_Task.Cs, Cst.OTCml_TBL.IOINPUT);
            //IDataReader drOutput = DataHelper.ExecuteReader(m_Task.Cs, CommandType.Text, sqlQuery);
            //return drOutput;
            //return DataHelper.ExecuteReader(m_Task.Cs, CommandType.Text, GetInOutQuery());
            return IOTaskTools.TaskInputDataReader(m_Task.Cs, m_IdIOInOut);
        }

        // EG 20180426 Analyse du code Correction [CA2202]
        protected override QueryParameters GetInOutQuery()
        {
            //// RD 20100111 [16818] MS Excel® file import
            //string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf;
            //sqlQuery += SQLCst.SELECT + "DISPLAYNAME, DESCRIPTION, LOGLEVEL, COMMITMODE, DATASTYLE, DATACONNECTION, DATANAME, ISVERTICAL, ISHETEROGENOUS, ";
            //sqlQuery += "DATASECTIONSTART, DATASECTIONEND, NBOMITROWSTART, NBOMITROWEND, XSLMAPPING, NBCOLUMNBYROWMAP, SERIALIZEMODE, DATATARGETDESC" + Cst.CrLf;
            //sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOINPUT + Cst.CrLf;
            //sqlQuery += SQLCst.WHERE + " IDIOINPUT = " + DataHelper.SQLString(m_IdIOInOut) + Cst.CrLf;
            //sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(m_Task.Cs, Cst.OTCml_TBL.IOINPUT);
            //return sqlQuery;
            QueryParameters queryParameters = IOTaskTools.TaskInputQuery(m_Task.Cs, m_IdIOInOut);
            return queryParameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAction"></param>
        /// <returns></returns>
        private static string GetLibAction(IOCommonTools.SqlAction pAction)
        {
            string ret = string.Empty;
            //
            switch (pAction)
            {
                case IOCommonTools.SqlAction.I:
                    ret = "Insert";
                    break;
                case IOCommonTools.SqlAction.U:
                    ret = "Update";
                    break;
                case IOCommonTools.SqlAction.D:
                    ret = "Delete";
                    break;
            }
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20210128 Add
        private void BeginInputTran()
        {
            m_DbTransaction = DataHelper.BeginTran(m_DbConnection);
        }


        /// <summary>
        /// 
        /// </summary>
        private void RollbackInputTran()
        {
            DataHelper.RollbackTran(m_DbTransaction, false);

            if (null != m_SqlOrder)
                m_SqlOrder.DbTransaction = null;
        }

        /// <summary>
        /// Commit de la transaction courante
        /// </summary>
        private void CommitInputTran()
        {
            DataHelper.CommitTran(m_DbTransaction, false);

            if (null != m_SqlOrder)
                m_SqlOrder.DbTransaction = null;
        }



        /// <summary>
        /// Load lines of flow Source in PostParsingFlow structur : IOTaskDetInOutFile
        /// </summary>
        /// <param name="pIsData"></param>
        /// <param name="pInputParsing"></param>
        /// <param name="pPostParsingFlow"></param>
        /// <param name="pErrorFile"></param>
        /// RD 20100111 [16818] MS Excel® file import
        /// RD 20110401 [17379] Serialize mode
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20220221 [XXXXX] Gestion IRQ
        private void LoadLinesFromSource(bool pIsData, InputParsing pInputParsing, ref IOTaskDetInOutFile pPostParsingFlow, ref bool pErrorFile)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            int rowId = 1;
            int lineNumber = 1;
            IOTaskDetInOutFileRow rootRow = null;

            // RD 20110323
            ArrayList arrRowsAll = new ArrayList();
            ArrayList arrRowsWithoutLevel0 = new ArrayList();
            ArrayList arrRowsLevel0 = new ArrayList();
            IOTaskDetInOutFileRow row = null;

            pPostParsingFlow.statusSpecified = true;
            pPostParsingFlow.status = ProcessStateTools.StatusSuccess.ToLower();
            //            
            // PM 20180219 [23824] IOTools => IOCommonTools
            m_NbSourceTotalLines = IOCommonTools.CurrentFileLineCount;
            //
            // PM 20180219 [23824] IOTools => IOCommonTools
            //if (pIsData)
            //{
            //    currentLine = IOTools.StringReader.ReadLine();
            //}
            //else
            //{
            //    currentLine = IOTools.StreamReader.ReadLine();
            //}
            string currentLine = IOCommonTools.StringOrStreamReadLine(pIsData);
            //
            while (currentLine != null)
            {
                try
                {
                    #region Process Line per Line
                    bool isOk = true;

                    if (IRQTools.IsIRQRequested(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                        break;

                    if (isOk)
                    {
                        #region Row to omit (Start or End)
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //isOk = ((lineNumber > m_NbOmitRowStart)
                        //    && ((m_NbOmitRowEnd == 0) || (lineNumber <= IOTools.CurrentFileLineCount - m_NbOmitRowEnd)));
                        isOk = ((lineNumber > m_NbOmitRowStart)
                            && ((m_NbOmitRowEnd == 0) || (lineNumber <= IOCommonTools.CurrentFileLineCount - m_NbOmitRowEnd)));
                        // RD 201000927 [] Considérer comme ignorées, les lignes du fichier d'origine,ignorées par parametrage du parsing
                        if (false == isOk)
                        {
                            
                            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6016), 0, new LogParam(lineNumber)));

                            m_NbParsingIgnoredLines++;
                        }
                        #endregion
                    }

                    if (isOk)
                    {
                        #region Check is currentLine is filled
                        isOk = StrFunc.IsFilled(currentLine);
                        // RD 201000927 [] Considérer comme ignorées, les lignes blanches du fichier d'origine
                        if (false == isOk)
                        {
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 6017), 0, new LogParam(lineNumber)));

                            m_NbParsingIgnoredLines++;
                        }
                        #endregion
                    }

                    if (isOk)
                    {
                        //FL/PL 20130225 Manage return boolean value
                        // PM 20180219 [23824] LoadLine déplacée de IOTools dans IOCommonTools
                        //if (IOTools.LoadLine(m_Task, currentLine, lineNumber, rowId, pInputParsing, ref row, ref m_NbParsingIgnoredLines))
                        if (IOCommonTools.LoadLine(m_Task, currentLine, lineNumber, rowId, pInputParsing, ref row, ref m_NbParsingIgnoredLines))
                        {

                            // RD 20110401 [17379] Hierachical Row
                            if (false == pInputParsing.IsHierarchical)
                            {
                                arrRowsAll.Add(row);
                            }
                            else
                            {
                                #region Gestion de la structure hiérarchique des lignes
                                int rowLevel = pInputParsing.GetRowLevelParsing();
                                //
                                row.level = rowLevel;
                                // 
                                // Pour IOINPUT_PARSING.ROWLEVEL = Null, level est considérée comme non spécifié 
                                row.levelSpecified = (rowLevel >= 0);
                                //
                                // SI IOINPUT_PARSING.ROWLEVEL = Null, <=0 ou 1, donc la ligne est considérée comme étant une ligne à la racine                                    
                                if (rowLevel <= 1)
                                {
                                    if (rowLevel == 0)
                                        arrRowsLevel0.Add(row);
                                    else
                                        arrRowsWithoutLevel0.Add(row);
                                    //
                                    arrRowsAll.Add(row);
                                    //
                                    if (rowLevel == 1)
                                        // Pour IOINPUT_PARSING.ROWLEVEL = 1, c'est considérée comme étant une ligne racine, susceptible d'avoir des row Fils 
                                        rootRow = row;
                                    // RD 20140114 [19472] Ajout de la condition sur le nombre de Parsings correspondant à la ligne, pour distinguer le cas:
                                    // - d'une ligne avec un Niveau de Parsing non renseigné ou à zéro (ParsingCount>0)
                                    // - et d'une ligne pour laquelle aucun Parsing n'est prévue (ParsingCount=0)
                                    // Ainsi pour le deuxième cas, on continu normalement avec le reste des lignes, comme si les lignes sans Parsings n'ont pas existées.
                                    else if (pInputParsing.ParsingCount > 0)
                                        // Pour IOINPUT_PARSING.ROWLEVEL = Null ou 0, c'est considérée comme étant une ligne racine, NON susceptible d'avoir des row Fils 
                                        rootRow = null;
                                }
                                else
                                    // Pour IOINPUT_PARSING.ROWLEVEL > 1, donc la ligne est considérée comme étant une ligne Fille d'une éventuelle ligne Parente, sinon ERROR
                                    AddChildRowToAncestryRow(rootRow, row, rowLevel);
                                #endregion
                            }
                        }

                        rowId += 1;
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    pErrorFile = true;
                    // FI 20200623 [XXXXX] AddCriticalException
                    m_Task.Process.ProcessState.AddCriticalException(ex);
                    

                    
                    // FI 20200706 [XXXXX] call SetErrorWarning
                    //m_ElementStatus = ProcessStateTools.StatusEnum.ERROR;
                    ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(string.Empty, ex)));
                }
                // PM 20180219 [23824] IOTools => IOCommonTools
                //if (pIsData)
                //{
                //    currentLine = IOTools.StringReader.ReadLine();
                //}
                //else
                //{
                //    currentLine = IOTools.StreamReader.ReadLine();
                //}
                currentLine = IOCommonTools.StringOrStreamReadLine(pIsData);
                //
                lineNumber += 1;
            }

            // RD 20110323
            if (arrRowsAll.Count > 0)
            {
                pPostParsingFlow.row = (IOTaskDetInOutFileRow[])arrRowsAll.ToArray(typeof(IOTaskDetInOutFileRow));

                if (arrRowsLevel0.Count > 0)
                {
                    pPostParsingFlow.rowsLevel0 = (IOTaskDetInOutFileRow[])arrRowsLevel0.ToArray(typeof(IOTaskDetInOutFileRow));
                    pPostParsingFlow.rowsWithoutLevel0 = (IOTaskDetInOutFileRow[])arrRowsWithoutLevel0.ToArray(typeof(IOTaskDetInOutFileRow));
                }
                else
                    pPostParsingFlow.rowsWithoutLevel0 = (IOTaskDetInOutFileRow[])arrRowsAll.ToArray(typeof(IOTaskDetInOutFileRow));
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pErrorFile"></param>
        /// <param name="pPostParsingFlow"></param>
        private static void UpdatePostParsingFlowStatus(bool pErrorFile, ref IOTaskDetInOutFile pPostParsingFlow)
        {
            //20081226 FI [16446] add isFileOnError = true s'il existe 1 ligne en erreur 
            for (int i = 0; i < ArrFunc.Count(pPostParsingFlow.row); i++)
            {
                pErrorFile = GetPostParsingRowStatus(pPostParsingFlow.row[i]);
                if (pErrorFile)
                    break;
            }
            //
            if (pErrorFile)
                pPostParsingFlow.status = ProcessStateTools.StatusError.ToLower();
            else
                pPostParsingFlow.status = ProcessStateTools.StatusSuccess.ToLower();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPostParsingRow"></param>
        /// <returns></returns>
        private static bool GetPostParsingRowStatus(IOTaskDetInOutFileRow pPostParsingRow)
        {
            bool ret = false;
            //
            if (null != pPostParsingRow) // => (null == postParsingFlow.row[i]) when line skipedd by setting or blanck
            {
                // si row.status non spécifié alors considérer SUCCESS
                if ((pPostParsingRow.statusSpecified) && (pPostParsingRow.status != (ProcessStateTools.StatusSuccess.ToLower())))
                    return true;
                //
                if (ArrFunc.IsFilled(pPostParsingRow.childRows))
                {
                    for (int i = 0; i < ArrFunc.Count(pPostParsingRow.childRows); i++)
                    {
                        ret = GetPostParsingRowStatus(pPostParsingRow.childRows[i]);
                        if (ret)
                            break;
                    }
                }
            }
            //
            return ret;

        }

        /// <summary>
        /// Ajouter un row fils à son ascendant, en spécifiant la racine (ascendant), et le niveau (génération) du fils par rapport à la racine
        /// </summary>
        /// <param name="pAncestryRow">la racine (ascendant)</param>
        /// <param name="pChildRow">Le fils</param>
        /// <param name="pChildGeneration">Niveau (génération) du fils par rapport à la racine (ascendant)</param>
        private void AddChildRowToAncestryRow(IOTaskDetInOutFileRow pAncestryRow, IOTaskDetInOutFileRow pChildRow, int pChildGeneration)
        {
            if (null == pAncestryRow)
                throw new Exception(
                    "Error in Hierarchical Level. No ancestor for row with level: " + pChildGeneration.ToString());
            //
            if (pChildGeneration == 2)
            {
                // pRootRow est l'ascendant directe de pChildRow ( 2 éme génération après le pére)
                // Donc rajouter pChildRow à la collections des fils de pRootRow
                ArrayList aAncestryChilds = new ArrayList();
                //
                if (ArrFunc.IsFilled(pAncestryRow.childRows))
                {
                    foreach (IOTaskDetInOutFileRow childRow in pAncestryRow.childRows)
                        aAncestryChilds.Add(childRow);
                }
                //
                aAncestryChilds.Add(pChildRow);
                //
                pAncestryRow.childRows = (IOTaskDetInOutFileRow[])aAncestryChilds.ToArray(typeof(IOTaskDetInOutFileRow));
            }
            else
            {
                // pRootRow n'est l'ascendant directe de pChildRow
                // Donc il faudrait remonter d'une génération pour trouver le pére
                if (ArrFunc.IsFilled(pAncestryRow.childRows))
                {
                    // Prendre la dernier fils de pRootRow
                    IOTaskDetInOutFileRow newAncestryRow = pAncestryRow.childRows[ArrFunc.Count(pAncestryRow.childRows) - 1];
                    // Chercher le pére dans la génération d'après pRootRow
                    AddChildRowToAncestryRow(newAncestryRow, pChildRow, pChildGeneration - 1);
                }
                else
                    throw new Exception(
                        "Error in Hierarchical Level. No ancestor for row with level: " + pChildGeneration.ToString());
            }
        }

        /// <summary>
        ///  Vérifie que le paramètre instrument est présent dans les paramètres de l'importation 
        ///  <para>Retourne les caratéristiques de l'instrument</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pSettings">Représente la configuration de l'import (contient les paramètres de l'import)</param>
        /// <param name="pScheme">Représente le nom du paramètre instrument</param>
        /// <param name="pColumn">Liste des caratéristiques à récupérer</param>
        /// <returns></returns>
        /// <exception cref="Exception si instrument non spécifié ou inconnu"></exception>
        private static SQL_Instrument GetImportSettingInstrument(string pCS, IDbTransaction pDbTransaction, ImportSettings pSettings, string pScheme, string[] pColumn)
        {
            string instrIdentifier = string.Empty;
            IScheme scheme = Tools.GetScheme(pSettings.parameter, pScheme);
            if (null != scheme)
            {
                instrIdentifier = ((ImportParameter)scheme).GetDataValue(pCS, pDbTransaction);
            }
            //
            if (StrFunc.IsEmpty(instrIdentifier))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<b>Instrument not specified</b>, parameter[scheme:{0}] is mandatory", pScheme);
                throw new Exception(sb.ToString());
            }
            //
            SQL_Instrument ret = new SQL_Instrument(CSTools.SetCacheOn(pCS), instrIdentifier);
            if (false == ret.LoadTable(pColumn))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<b>Instrument {0} unknown</b>", instrIdentifier);
                throw new Exception(sb.ToString());
            }

            return ret;
        }

        /// <summary>
        /// <para>Vérifie la présence du pavé configuration (settings)</para>
        /// <para>Vérifie la présence de importMode</para>
        /// </summary>
        /// <param name="pTradeImport"></param>
        /// <exception cref="Exception en cas d'absence"></exception>
        private static void CheckTradeImportSettings(TradeImport pTradeImport)
        {
            try
            {
                //
                if (false == pTradeImport.settingsSpecified)
                    throw new Exception("<b>Settings not specified</b>");
                //
                if (false == pTradeImport.settings.importModeSpecified)
                    throw new Exception(
                        "<b>Import mode not specified</b>, expected value is one of the following : New,Consult,Update,RemoveOnly,RemoveReplace");
            }
            catch (Exception ex)
            {
                throw new Exception("Check settings error", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessInOut"></param>
        /// <param name="pDataDelimiter"></param>
        /// <param name="pXMLElementToImport"></param>
        /// <param name="pXMLFilter"></param>
        private static void GetXMLElementToImportAndFilter(ProcessInOut pProcessInOut, string pDataDelimiter, out string pXMLElementToImport, out string pXMLFilter)
        {
            // 
            pXMLElementToImport = string.Empty;
            pXMLFilter = string.Empty;

            string temp1 = StrFunc.ExtractString(pProcessInOut.DataName, 1, ";");
            string temp2 = StrFunc.ExtractString(pProcessInOut.DataName, 2, ";");
            //
            string separator = (StrFunc.IsFilled(pDataDelimiter) ? pDataDelimiter : "=");
            //
            if (StrFunc.IsFilled(temp1))
            {
                if (StrFunc.ExtractString(temp1, 0, "=") == IOTools.Element)
                    pXMLElementToImport = StrFunc.ExtractString(temp1, 1, separator);
                else if (StrFunc.ExtractString(temp1, 0, "=") == IOTools.Filter)
                    pXMLFilter = StrFunc.ExtractString(temp1, 1, separator);
            }
            //
            if (StrFunc.IsFilled(temp2))
            {
                if (StrFunc.ExtractString(temp2, 0, "=") == IOTools.Element)
                    pXMLElementToImport = StrFunc.ExtractString(temp2, 1, separator);
                else if (StrFunc.ExtractString(temp2, 0, "=") == IOTools.Filter)
                    pXMLFilter = StrFunc.ExtractString(temp2, 1, separator);
            }
            //
            pXMLElementToImport = pXMLElementToImport.Trim();
            pXMLFilter = pXMLFilter.Trim();
        }

        /// <summary>
        ///  Interprète la donnée en XSLPARSING présente dans IOPARSING.XSLPARSING
        /// </summary>
        /// <param name="pXslParsingIn"></param>
        /// <returns></returns>
        private string GetXSLParsing(string pXslParsingIn)
        {
            string ret = pXslParsingIn;

            if (StrFunc.IsEmpty(pXslParsingIn) || pXslParsingIn.ToLower().Contains("please insert"))
            {
                throw new Exception(
                        @"<b>You forgot to specify the XSL Parsing File for input [" + m_IdIOInOut + @"] (" + m_DataName + " [" + m_DataStyle + @"]).</b>
                                    Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                    (eg: \\server\share\directory\parsing_sample.xsl or S:\directory\parsing_sample.xsl)");
            }

            if (false == File.Exists(pXslParsingIn))
            {
                bool isFound;
                try
                {
                    isFound = m_Task.Process.AppInstance.SearchFile2(m_Task.Cs, pXslParsingIn, ref ret);
                }
                catch { isFound = false; }

                if (!isFound)
                {
                    //On tente au cas où GetFile() soit bugguées...                        
                    ret = m_Task.Process.AppInstance.GetFilepath(pXslParsingIn);
                    isFound = File.Exists(ret);
                }

                if (!isFound)
                {
                    throw new Exception(
                            @"<b>XSL Parsing File (" + pXslParsingIn + ") specified for input [" + m_IdIOInOut + @"] (" + m_DataName + " [" + m_DataStyle + @"]) does not exist.</b>
                                        Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                        (eg: \\server\share\directory\parsing_sample.xsl or S:\directory\parsing_sample.xsl)");
                }
            }

            return ret;
        }

        /// <summary>
        ///  Génère potentiellement un fichier XML de travail à partir du XML en entrée de manière à optimiser la transformation XSL
        ///  <para>Le fichier XML de travail est généré dès lors qu'un element (+un filter) sont déclarés (exemple orgmast.xml;Element="ExchangeMaster";Filter="//r[ExchAcro='CME']")</para>
        ///  <para>Le fichier XML de travail est de structure identique au fichier en entrée. Il est moins volumineux</para>
        /// </summary>
        /// <param name="pXmlFileNameIn">Fichier XML en entrée</param>
        /// <param name="pXmlFileNameOut">Fichier ML en sortie</param>
        private void BuilXMLFile(string pXmlFileNameIn, out string pXmlFileNameOut)
        {
            pXmlFileNameOut = pXmlFileNameIn;
            GetXMLElementToImportAndFilter(this, "\"", out string xmlElementToImport, out string xmlFilter);

            if (StrFunc.IsFilled(xmlElementToImport))
            {
                try
                {
                    string tempXMLFileName = pXmlFileNameIn.Substring(pXmlFileNameIn.LastIndexOf("\\") + 1);
                    tempXMLFileName = m_Task.Session.MapTemporaryPath(tempXMLFileName, AppSession.AddFolderSessionId.True);
                    pXmlFileNameOut = tempXMLFileName;
                    //
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Creation of the temporary working file: " + tempXMLFileName + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), 3));
                    //
                    //
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //IOTools.OpenFile(pXmlFileNameIn);
                    //IOTools.OpenFile(tempXMLFileName, Cst.WriteMode.WRITE);
                    IOCommonTools.OpenFile(pXmlFileNameIn);
                    IOCommonTools.OpenFile(tempXMLFileName, Cst.WriteMode.WRITE);
                    //
                    bool isAlreadyFilterVerifiedByElement = false;
                    bool isElement = false;
                    bool isElementEnd = false;
                    //
                    string elementStart = "<" + xmlElementToImport;
                    string elementEnd = "</" + xmlElementToImport + ">";
                    //
                    //StringBuilder xmlDataFlow = new StringBuilder();
                    StringBuilder elementFlow = new StringBuilder();
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //string line = IOTools.StreamReader.ReadLine();
                    string line = IOCommonTools.StreamReader.ReadLine();
                    string lineTrim = string.Empty;
                    //
                    while (line != null)
                    {
                        #region Lire ligne par ligne
                        lineTrim = line.Trim();
                        //
                        // Chercher le début de l'élément concerné par le Filtre
                        if ((false == isElement) && lineTrim.StartsWith(elementStart))
                            isElement = true;
                        //
                        // Chercher la fin de l'élément concerné par le Filtre
                        if (isElement && (lineTrim.EndsWith(elementEnd)))
                            isElementEnd = true;
                        //
                        if (isElement)
                        {
                            if (false == isAlreadyFilterVerifiedByElement)
                            {
                                elementFlow.AppendLine(line); // Stocker temporairement le contenu de l'élément concerné par le Filtre
                            }
                        }
                        else
                        {
                            // PM 20180219 [23824] IOTools => IOCommonTools
                            //IOTools.StreamWriter.WriteLine(line); // Ecrire tous les autres éléments non concernés par le Filtre
                            IOCommonTools.StreamWriter.WriteLine(line); // Ecrire tous les autres éléments non concernés par le Filtre
                        }
                        //
                        if (isElementEnd)
                        {
                            if (false == isAlreadyFilterVerifiedByElement)
                            {
                                bool isToWrite = false;
                                //
                                if (StrFunc.IsFilled(xmlFilter))
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(elementFlow.ToString());
                                    //
                                    XmlNodeList element = doc.SelectNodes(xmlFilter);
                                    //
                                    if (null != element && element.Count > 0)
                                        isToWrite = true;
                                    //
                                    doc.RemoveAll();
                                    doc = null;
                                    element = null;
                                }
                                else
                                {
                                    isToWrite = true;
                                }
                                //
                                if (isToWrite)
                                {
                                    //xmlDataFlow.AppendLine(elementFlow.ToString());
                                    // PM 20180219 [23824] IOTools => IOCommonTools
                                    //IOTools.StreamWriter.WriteLine(elementFlow);
                                    IOCommonTools.StreamWriter.WriteLine(elementFlow);
                                    isAlreadyFilterVerifiedByElement = true;
                                    elementFlow = null;
                                }
                            }
                            //
                            isElement = false;
                            isElementEnd = false;
                            if ((null == elementFlow) || elementFlow.Length > 0)
                                elementFlow = new StringBuilder();
                        }
                        //
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //line = IOTools.StreamReader.ReadLine();
                        line = IOCommonTools.StreamReader.ReadLine();
                        #endregion
                    }
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Ended", 3));
                }
                catch (Exception ex)
                {
                    throw new Exception(
                     @"<b>Error to filter data source</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                }
                finally
                {
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //IOTools.CloseFile(pXmlFileNameIn);
                    IOCommonTools.CloseFile(pXmlFileNameIn);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsData"></param>
        /// <param name="pDataSourceFlow"></param>
        /// <param name="pXmlFile"></param>
        /// <param name="pXslFile"></param>
        /// <param name="pParameters"></param>
        /// <param name="pMsgLogInOut"></param>
        /// <returns></returns>
        private static string ParsingXMLData(Boolean pIsData, string pDataSourceFlow, string pXmlFile, string pXslFile,
                IOTaskParams pParameters, ArrayList pMsgLogInOut)
        {
            try
            {
                string ret;
                if (pIsData)
                {
                    StringBuilder result = IOTools.XmlTransformInSb(false, pDataSourceFlow, pXslFile, pParameters);
                    ret = result.ToString();
                }
                else
                {
                    StringBuilder result = IOTools.XmlTransformInSb(pXmlFile, pXslFile, pParameters);
                    ret = result.ToString();
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "<b>Error to Transform the source stream</b>" + Cst.CrLf +
                    "[Xsl file: " + pXslFile + "]" + Cst.CrLf + ArrFunc.GetStringList(pMsgLogInOut, Cst.CrLf), ex);
            }
        }

        /// <summary>
        ///  Recupère le flux post-Parsing lorsque les données sont aux format XML
        /// </summary>
        /// <param name="pIsData">true si le flux XML est présent dans le message Queue</param>
        /// <param name="pDataSourceFlow">Flux XML lorsque ce dernier est présent dans le message Queue</param>
        /// <param name="pXslParsingIn">Représente le XSL de parsing</param>
        /// <param name="postParsingFlow">Représente le flux Post-Parsing</param>
        private void PostParsingFlowFromXMLData(bool pIsData, string pDataSourceFlow, string pXslParsingIn, ref IOTaskDetInOutFile postParsingFlow)
        {
            string xmlFileName = StrFunc.ExtractString(m_DataName, 0, ";");
            if (false == pIsData)
            {
                // PM 20180219 [23824] VerifyFilePathForImport déplacée dans IOTaskTools
                xmlFileName = IOTaskTools.VerifyFilePathForImport(m_Task, xmlFileName, m_MsgLogInOut);
            }

            BuilXMLFile(xmlFileName, out string tempXMLFileName);

            string xslParsing = GetXSLParsing(pXslParsingIn);

            IOTaskParams parameters = null;
            if (TaskPostParsing.IsParametersSpecified)
                parameters = TaskPostParsing.parameters;

            string postParsing = ParsingXMLData(pIsData, pDataSourceFlow, tempXMLFileName, xslParsing, parameters, m_MsgLogInOut);

            #region Désérialiser le résultat de la transformation dans postParsingFlow
            try
            {
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(IOTaskDetInOutFile), postParsing);
                postParsingFlow = (IOTaskDetInOutFile)CacheSerializer.Deserialize(serializeInfo);
                postParsingFlow.status = ProcessStateTools.StatusSuccess.ToLower();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "xml Parsing is not valid, please validate against the schema SpheresIO.xsd, complextype:IOTaskDetInOutFile", ex);
            }
            #endregion Désérialiser le résultat

            IOTools.SetFileInfoToPostParsingFlow(!pIsData, xmlFileName, ref postParsingFlow);
        }

        /// <summary>
        ///  Recupère le flux post-Parsing lorsque les données sont dans un fichier Excel
        /// </summary>
        /// RD 20100111 [16818] MS Excel® file import
        // EG 20180423 Analyse du code Correction [CA2200]
        private void PostParsingFlowFromExcel(InputParsing inputParsing, ref IOTaskDetInOutFile postParsingFlow)
        {
            string dataToImport = string.Empty;
            
            string excelFileName = StrFunc.ExtractString(m_DataName, 0, ";");

            // PM 20180219 [23824] VerifyFilePathForImport déplacée dans IOTaskTools
            excelFileName = IOTaskTools.VerifyFilePathForImport(m_Task, excelFileName, m_MsgLogInOut);

            
            string localExcelFileName = excelFileName;
            // PM 20180219 [23824] IOTools => IOCommonTools
            if (IOCommonTools.IsHttp(excelFileName))
            {
                #region Load Http file
                localExcelFileName = localExcelFileName.Substring(localExcelFileName.LastIndexOf("/") + 1);
                localExcelFileName = m_Task.Session.MapTemporaryPath(localExcelFileName, AppSession.AddFolderSessionId.True);
                //
                try
                {
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //IOTools.DownloadWebFile(excelFileName, localExcelFileName);
                    IOCommonTools.DownloadWebFile(excelFileName, localExcelFileName);

                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, "<b>File to import is downloaded in:</b> " + localExcelFileName + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf)));
                }
                catch (Exception ex)
                {
                    throw new Exception(
                     @"<b>Error to download file to import</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                }
                #endregion
            }

            string excelOleDbCs = string.Empty;
            IOTools.VerifyDataConnectionForImport(this, localExcelFileName, ref excelOleDbCs);

            try
            {
                
                DataTable dtExcelFile;
                DataRow[] drExcelFile;
                try
                {
                    #region 1- read data from OleDb source
                    string excelSheetName = string.Empty;
                    string excelRange = string.Empty; // du genre B1:HV16
                    IOTools.GetExcelSheetNameAndDataRange(this, ref excelSheetName, ref excelRange);
                    //
                    string selectSQL = "select * from [" + excelSheetName + "$" + excelRange + "]";
                    DataSet dsExcelFile = DataHelper.ExecuteDataset(excelOleDbCs, m_Task.Cs, CommandType.Text, selectSQL);
                    dtExcelFile = dsExcelFile.Tables[0];
                    drExcelFile = dtExcelFile.Select();
                    #endregion
                }
                catch (Exception ex)
                {
                    bool isSqlError = DataHelper.AnalyseSQLException(excelOleDbCs, ex, out string message, out string errorCode);
                    //
                    // RD 20100115 [16792] Utiliser SQLErrorEnum
                    if (isSqlError)
                    {
                        throw new Exception(
                                    @"<b>Error to read data from OleDb source</b>" + Cst.CrLf + message +
                                    Cst.CrLf + errorCode + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                //
                int columnCount = dtExcelFile.Columns.Count;
                string fmtIsoValue = string.Empty;
                string line = string.Empty;
                // PM 20180219 [23824] IOTools => IOCommonTools
                //IOTools.OpenStringWriter();
                IOCommonTools.OpenStringWriter();
                //
                #region 2- Transform Excel flow to Text Flow
                if (m_IsVertical)
                {
                    for (int i = 0; i < columnCount; i++)
                    {
                        line = string.Empty;
                        //
                        foreach (DataRow row in drExcelFile)
                        {
                            fmtIsoValue = string.Empty;
                            //
                            if (false == ArrFunc.IsAllElementEmpty2(row.ItemArray) && (false == Convert.IsDBNull(row[i])))
                                fmtIsoValue = row[i].ToString();
                            //
                            line += fmtIsoValue + IOTools.ExcelDataSeparatorCaracter;
                        }
                        //
                        line = line.TrimEnd(IOTools.ExcelDataSeparator.ToCharArray());
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //IOTools.StringWriter.WriteLine(line);
                        IOCommonTools.StringWriter.WriteLine(line);
                    }
                }
                else
                {
                    foreach (DataRow row in drExcelFile)
                    {
                        line = string.Empty;
                        //  
                        if (false == ArrFunc.IsAllElementEmpty2(row.ItemArray))
                        {
                            for (int i = 0; i < columnCount; i++)
                            {
                                fmtIsoValue = string.Empty;
                                //
                                if (false == Convert.IsDBNull(row[i]))
                                {
                                    // PM 20190904 [24878] Gestion des données decimal et double (problème to ToString qui converti en notation scientifique: exemple 0.00001 en 1E-05)
                                    Type rowType = row.Table.Columns[i].DataType;
                                    if ((rowType == typeof(System.Decimal)) || (rowType == typeof(System.Double)))
                                    {
                                        fmtIsoValue = StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(row[i]));
                                    }
                                    else
                                    {
                                        fmtIsoValue = row[i].ToString();
                                    }
                                }
                                //
                                line += fmtIsoValue + IOTools.ExcelDataSeparatorCaracter;
                            }
                            //
                            line = line.TrimEnd(IOTools.ExcelDataSeparator.ToCharArray());
                        }
                        //
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //IOTools.StringWriter.WriteLine(line);
                        IOCommonTools.StringWriter.WriteLine(line);
                    }
                }
                #endregion
                //
                // PM 20180219 [23824] IOTools => IOCommonTools
                //dataToImport = IOTools.StringWriter.ToString();
                dataToImport = IOCommonTools.StringWriter.ToString();
                inputParsing.DefaultSeparator = IOTools.ExcelDataSeparator;
            }
            catch (Exception) { throw; }
            finally
            {
                // PM 20180219 [23824] IOTools => IOCommonTools
                //IOTools.CloseStream();
                IOCommonTools.CloseStream();
            }
            //
            bool isFlowOnError = false;
            postParsingFlow = new IOTaskDetInOutFile();
            //
            try
            {
                #region 3- Charger les lignes une par une dans postParsingFlow
                IOTools.InitPostParsingFlow(false, dataToImport, m_DataStyle, ref postParsingFlow);
                IOTools.SetFileInfoToPostParsingFlow(true, excelFileName, ref postParsingFlow);
                // PM 20180219 [23824] IOTools => IOCommonTools
                //IOTools.OpenStringReader(dataToImport);
                IOCommonTools.OpenStringReader(dataToImport);
                //
                LoadLinesFromSource(true, inputParsing, ref postParsingFlow, ref isFlowOnError);
                #endregion Charger les lignes
            }
            catch (Exception) { throw; }
            finally
            {
                // PM 20180219 [23824] IOTools => IOCommonTools
                //IOTools.CloseStream();
                IOCommonTools.CloseStream();
            }
            //
            UpdatePostParsingFlowStatus(isFlowOnError, ref postParsingFlow);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="row"></param>
        /// FI 20130320 [] add Method
        /// FI 20131122 [19233] suppression du paramètre  rowMessage
        //private void SetIOXmlOverridesExtended(IOTaskDetInOutFileRow row, ArrayList rowMessage)
        private void SetIOXmlOverridesExtended(IOTaskDetInOutFileRow row)
        {
            _isIOXmlOverridesExtended = false;

            if (ArrFunc.IsFilled(row.tradeImport))
                _isIOXmlOverridesExtended = true;
            else if (ArrFunc.IsFilled(row.posRequestImport))
                _isIOXmlOverridesExtended = true;
        }

        /// <summary>
        /// Importation des élements de type table présents dans le row post Mapping
        /// </summary>
        /// <param name="pRow">Représente le row post Mapping</param>
        /// <param name="pTable">Représente les importations dans n tables</param>
        /// <param name="pPassMessage"></param>
        /// <param name="recordIOTrackDetails"></param>
        /// <param name="nbRecordRejected"></param>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// FI 20130320 [] add Method
        /// FI 20131122 [19233] Modification de la signature de la méthode
        /// EG 20180423 Analyse du code Correction [CA2200]
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// FI 20201221 [XXXXX] add pSetErrorWarning, pRecordIOTrackDetails and pNbRecordRejected are out paramters
        private void RowTableImport(IOTaskDetInOutFileRow pRow, IOTaskDetInOutFileRowTable[] pTable, string pPassMessage, out List<IOTrackDetail> recordIOTrackDetails, out int nbRecordRejected, SetErrorWarning pSetErrorWarning)
        {
            nbRecordRejected = 0;
            recordIOTrackDetails = new List<IOTrackDetail>();

            foreach (IOTaskDetInOutFileRowTable table in pTable)
            {
                try
                {
                    ArrayList tableMessage = IOTools.RowTableDesc(pRow, table);

                    IOTrackDetail tableIOTrackDetail = GetIOTrackDetail(recordIOTrackDetails, table.name);

                    if (ArrFunc.IsEmpty(table.column))
                        throw new Exception("[No column generated]");

                    IOCommonTools.SqlAction actionId = ParseAction(table.action);

                    #region Traitement de la table
                    int response = -1;
                    string quote_source = null;
                    string quote_source_customvalue = null;
                    m_SqlOrder = new SqlOrder(m_DbTransaction, m_Task, pRow, table);
                    int rowAffected = 0;
                    if (actionId == IOCommonTools.SqlAction.IU)
                    {
                        #region Test d'existence des données
                        if (m_SqlOrder.GetQuery(IOCommonTools.SqlAction.IU, pSetErrorWarning))
                        {
                            //PL 20140718 Newness output param
                            response = ExistData(m_SqlOrder, ref quote_source, ref quote_source_customvalue);
                        }
                        else
                        {
                            nbRecordRejected++;
                            continue;
                        }
                        #endregion

                        string systemMsg = null;
                        switch (response)
                        {
                            case -1:
                                // Aucun enregistrement corrrespondant aux critères n'existe 
                                actionId = IOCommonTools.SqlAction.I;
                                break;
                            case 1:
                                // Un enregistrement corrrespondant existe, mais avec certaines données différentes
                                actionId = IOCommonTools.SqlAction.U;

                                //PL 20140918 Newness
                                if (m_SqlOrder.IsQuoteTable)
                                {
                                    //Cas particulier des historiques de cotations (ex. QUOTE_ETD_H)
                                    if ((!String.IsNullOrEmpty(quote_source_customvalue)) && (quote_source_customvalue.IndexOf("NOTOVERRIDABLE") >= 0))
                                    {
                                        systemMsg = "LOG-06073";
                                    }
                                }
                                break;
                            case 0:
                                // Un enregistrement corrrespondant existe, et ce avec les données identiques
                                systemMsg = "LOG-06013";
                                break;
                        }

                        if (!String.IsNullOrEmpty(systemMsg))
                        {
                            #region Log Completed Action message
                            string firstMsgDet = GetLibAction(IOCommonTools.SqlAction.U);
                            ArrayList useMessage = new ArrayList(tableMessage);
                            if (systemMsg == "LOG-06073")
                            {
                                firstMsgDet += " (source:<b>" + quote_source + "</b>, rule:NOTOVERRIDABLE)";
                            }
                            useMessage.Insert(0, firstMsgDet);
                            useMessage.Insert(0, systemMsg);
                            ProcessMapping.LogLogInfo(pSetErrorWarning, pRow.logInfo, useMessage);
                            #endregion

                            #region Poster un Message au MOM
                            // RD 20110629
                            // Gestion des Paramètres dans mQueue
                            IOTools.SendMQueue(m_Task, pRow, table, m_SqlOrder, IOCommonTools.SqlAction.NA, m_DbTransaction, useMessage);
                            #endregion

                            continue; //Spheres® stoppe et traite la prochaine table
                        }

                        if (m_SqlOrder.IsInsertUpdateControl)
                        {
                            if (false == m_SqlOrder.GetQuery(actionId, true, pSetErrorWarning))
                            {
                                nbRecordRejected++;
                                continue;
                            }
                        }

                        rowAffected = m_SqlOrder.ExecuteNonQuery(actionId);
                    }
                    else
                    {
                        if (m_SqlOrder.GetQuery(actionId, pSetErrorWarning))
                        {
                            rowAffected = m_SqlOrder.ExecuteNonQuery(actionId);
                        }
                        else
                        {
                            nbRecordRejected++;
                            continue;
                        }
                    }

                    if (rowAffected != 0)
                    {
                        if (ArrFunc.IsFilled(table.spheresLib))
                        {
                            for (int i = 0; i < table.spheresLib.Length; i++)
                                table.spheresLib[i].Exec(m_Task.Cs, m_DbTransaction);
                        }

                        // Poste éventuellement un Message à un autre service
                        IOTools.SendMQueue(m_Task, pRow, table, m_SqlOrder, actionId, m_DbTransaction, tableMessage);
                        //
                        #region Log Completed Action message
                        ArrayList useMessage = new ArrayList(tableMessage);
                        useMessage.Insert(0, GetLibAction(actionId));
                        useMessage.Insert(0, "LOG-06014");
                        ProcessMapping.LogLogInfo(pSetErrorWarning, pRow.logInfo, useMessage);
                        #endregion Log action
                        //
                        #region IOTrack
                        switch (actionId)
                        {
                            case IOCommonTools.SqlAction.I:
                                tableIOTrackDetail.InsertedRows += rowAffected;
                                break;
                            case IOCommonTools.SqlAction.U:
                                tableIOTrackDetail.UpdatedRows += rowAffected;
                                break;
                            case IOCommonTools.SqlAction.D:
                                tableIOTrackDetail.DeletedRows += rowAffected;
                                break;
                        }
                        #endregion
                    }
                    else if (actionId == IOCommonTools.SqlAction.U)
                    {
                        #region Log Completed Action message
                        ArrayList useMessage = new ArrayList(tableMessage);
                        useMessage.Insert(0, GetLibAction(actionId));
                        useMessage.Insert(0, "LOG-06013");
                        ProcessMapping.LogLogInfo(pSetErrorWarning, pRow.logInfo, useMessage);
                        #endregion Log action
                        // Poste éventuellement un Message à un autre service
                        IOTools.SendMQueue(m_Task, pRow, table, m_SqlOrder, IOCommonTools.SqlAction.NA, m_DbTransaction, useMessage);
                    }
                    // MF 20120404 No delete ops leveled down to Unknown (request origin: Consulting)
                    else if (actionId == IOCommonTools.SqlAction.D)
                    {
                        // FI 20190610 [24696] 
                        ArrayList useMessage = new ArrayList(tableMessage);
                        useMessage.Insert(0, GetLibAction(actionId));
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6015), 0, new LogParam(GetLibAction(actionId))));
                    }
                    else
                    {
                        #region Log Failed Action message
                        ArrayList useMessage = new ArrayList(tableMessage);
                        useMessage.Insert(0, GetLibAction(actionId));
                        useMessage.Insert(0, "LOG-06015");
                        ProcessMapping.LogLogInfo(pSetErrorWarning, pRow.logInfo, ProcessStateTools.StatusError, useMessage);
                        #endregion Log action
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    if (m_CommitMode == Cst.CommitMode.AUTOCOMMIT)
                    {
                        // 20090518 RD / On considère une ligne en erreur comme étant rejetée dans IOTRACK
                        nbRecordRejected++;
                        pSetErrorWarning(ProcessStateTools.StatusEnum.ERROR);

                        string msg = ExceptionTools.GetMessageExtended(ex);
                        Logger.Log(new LoggerData(LogLevelEnum.Error, BuildLogMsgDetail(pRow, pPassMessage + Cst.Space + msg), 3));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        ///  Importation des posRequest
        /// <para>Retourne l'action résultante (Nouvelle insertion, Mise à jour, etc..)</para>
        /// <para>Retourne true s'il existe au moins une condition non respectée dont le statut est StatusEnum.ERROR</para>
        /// </summary>
        /// <param name="pRow">Représente un enregistrement postMapping</param>
        /// <param name="pPosRequestImport">Représente l'importation dans POSREQUEST</param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pSetErrorWarning"></param>
        /// FI 20131122 [19233] add parameter pRow
        /// FI 20180207 [XXXXX] Modification de signature (suppression paramètre out pour faciliter le multithreading)
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// FI 20200706 [XXXXX] Add SetErrorWarning delegate
        private IOCommonTools.SqlAction PosRequestImport(IOTaskDetInOutFileRow pRow, PosRequestImport pPosRequestImport, IDbTransaction pDbTransaction, SetErrorWarning pSetErrorWarning)
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "Initialize…", 4));

            CheckPosRequestImportSettings(pPosRequestImport);

            // Recherche du paramètre instrumentIdentifier
            // Ce paramètre est essentiel, l'instrument permet de piloter l'alimentation de IFixInstrument présent dans le document POSREQUEST 
            SQL_Instrument sqlInstr = GetImportSettingInstrument(m_Task.Cs, pDbTransaction, pPosRequestImport.settings,
                PosRequestImportCst.instrumentIdentifier, new string[] { "IDENTIFIER", "PRODUCT_IDENTIFIER" });

            ProcessPosRequestImport import = new ProcessPosRequestImport(pPosRequestImport, pDbTransaction, m_Task);
            import.SetParameter(PosRequestImportCst.instrumentIdentifier, sqlInstr.Identifier);
            // FI 20200706 [XXXXX] Call InitDelegate
            import.InitDelegate(pSetErrorWarning);

            //FI 20131122 [19233] alimentation de LogHeader
            import.LogHeader = pRow.LogRowDesc;

            IOCommonTools.SqlAction ret = import.Process();

            return ret;
        }

        /// <summary>
        /// <para>Vérifie que l'importation contient le pavé configuration</para>
        /// <para>Vérifie la présence de importMode</para>
        /// </summary>
        /// <param name="pPosRequestImport"></param>
        /// <exception cref="Exception en cas d'absence"></exception>
        private static void CheckPosRequestImportSettings(PosRequestImport pPosRequestImport)
        {
            try
            {
                //
                if (false == pPosRequestImport.settingsSpecified)
                    throw new Exception("<b>Settings not specified</b>");
                //
                if (false == pPosRequestImport.settings.importModeSpecified)
                    throw new Exception(
                        "<b>Import mode not specified</b>, expected value is one of the following : New,Update,RemoveOnly");
            }
            catch (Exception ex)
            {
                throw new Exception("Check settings error", ex);
            }
        }

        /// <summary>
        ///  Retourne le IOTrackDetail concernant {pTable}. Si absent est ajouté dans la liste {lstIOTrackDetail}
        /// </summary>
        /// <param name="pLstIOTrackDetail"></param>
        /// <param name="pTableName"></param>
        /// <returns></returns>
        /// FI 20130321 add method
        private static IOTrackDetail GetIOTrackDetail(List<IOTrackDetail> pLstIOTrackDetail, string pTableName)
        {
            IOTrackDetail ret = pLstIOTrackDetail.Find(detail => detail.TableName == pTableName.ToUpper());
            if (null == ret)
            {
                ret = new IOTrackDetail(pTableName.ToUpper());
                pLstIOTrackDetail.Add(ret);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        private static void AddIOTrackDetail(List<IOTrackDetail> destination, IOTrackDetail source)
        {
            AddIOTrackDetail(destination, new List<IOTrackDetail> { source });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        private static void AddIOTrackDetail(List<IOTrackDetail> destination, List<IOTrackDetail> source)
        {
            IEnumerable<string> tables = from item in source
                                         select item.TableName;

            foreach (string item in tables)
            {
                IOTrackDetail destinationTable = GetIOTrackDetail(destination, item);
                IOTrackDetail sourceTable = GetIOTrackDetail(source, item);

                destinationTable.InsertedRows += sourceTable.InsertedRows;
                destinationTable.DeletedRows += sourceTable.DeletedRows;
                destinationTable.UpdatedRows += sourceTable.UpdatedRows;
            }
        }

        /// <summary>
        /// Importation des élements de type TradeImport ou de type PosRequestImport présents dans le row post Mapping
        /// </summary>
        /// <param name="pRow">Représente l'enregistrement postParsing</param>
        /// <param name="pImport">Représente les importations des trades ou les importations dans posRequest</param>
        /// <param name="pPassMessage"></param>
        /// <param name="pRecordIOTrackDetails"></param>
        /// <param name="pNbRecordRejected"></param>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// FI 20131122 [19233] add parameter pRow 
        /// FI 20180207 [XXXXX]  Modifications mineures
        /// EG 20180423 Analyse du code Correction [CA2200]
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// FI 20201221 [XXXXX] add pSetErrorWarning, pRecordIOTrackDetails and pNbRecordRejected are out paramters
        private void RowImport(IOTaskDetInOutFileRow pRow, object[] pImport, string pPassMessage, out IOTrackDetail pRecordIOTrackDetails, out int pNbRecordRejected, SetErrorWarning pSetErrorWarning)
        {
            pNbRecordRejected = 0;

            string Table;
            if (pImport.GetType().Equals(typeof(TradeImport[])))
                Table = "TRADE";
            else if (pImport.GetType().Equals(typeof(PosRequestImport[])))
                Table = "POSREQUEST";
            else
                throw new NotImplementedException(StrFunc.AppendFormat("pImport type {0} is not implemented", pImport.GetType().ToString()));

            pRecordIOTrackDetails = new IOTrackDetail(Table);

            foreach (object importItem in pImport)
            {
                IOCommonTools.SqlAction actionId = IOCommonTools.SqlAction.NA;

                try
                {
                    switch (Table)
                    {
                        case "TRADE":
                            // FI 20200706 [XXXXX] ProcessState.SetErrorWarning)
                            actionId = TradeImport2(m_Task, pRow, (TradeImport)importItem, m_DbTransaction, pSetErrorWarning);
                            break;
                        case "POSREQUEST":
                            // FI 20200706 [XXXXX] ProcessState.SetErrorWarning)
                            actionId = PosRequestImport(pRow, (PosRequestImport)importItem, m_DbTransaction, pSetErrorWarning);
                            break;
                        default:
                            throw new NotImplementedException($"{Table} is not implemented");
                    }

                    switch (actionId)
                    {
                        case IOCommonTools.SqlAction.I:
                            pRecordIOTrackDetails.InsertedRows++;
                            break;
                        case IOCommonTools.SqlAction.U:
                            pRecordIOTrackDetails.UpdatedRows++;
                            break;
                        case IOCommonTools.SqlAction.D:
                            pRecordIOTrackDetails.DeletedRows++;
                            break;
                        default:
                            pNbRecordRejected++;
                            break;
                    }

                }
                catch (Exception ex)
                {
                    if (m_CommitMode == Cst.CommitMode.AUTOCOMMIT)
                    {
                        pSetErrorWarning(ProcessStateTools.StatusEnum.ERROR);
                        //On considère une ligne en erreur comme étant rejetée dans IOTRACK
                        pNbRecordRejected++;

                        // FI 20200623 [XXXXX] SetErrorWarning
                        // FI 20200607 [XXXXX] Mise en commentaire puisque m_IsWritingError = true
                        //m_Task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        string msg = ExceptionTools.GetMessageExtended(ex);

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, BuildLogMsgDetail(pRow, pPassMessage + Cst.Space + msg), 3));

                        AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        ///  Ouvre le fichier Source et génère un fichier réduit au strict nécessaire
        ///  <para>Retourne le fichier modifié</para>
        /// </summary>
        /// <param name="inputParsing"></param>
        /// FI 20130412 add method FilePreLoading, appel aux classes IOTools_MARKETDATA2
        /// FI 20131021 [17861] l'importation des riskArray Eurex ne passe plus dans cette méthode
        private string FilterFile()
        {
            string tempFileName = null;

            switch (inputSourceDataStyle)
            {
                case Cst.InputSourceDataStyle.CLEARNETMARKETDATAFILE:
                    tempFileName = FilterCLEARNETSAFile();
                    break;
                case Cst.InputSourceDataStyle.ICEFUTURESEUROPEMARKETDATAFILE:
                    // RD 20171130 [23504] Filtrage du fichier ICEFuturesEurope
                    tempFileName = FilterICEFuturesEuropeFile();
                    break;
                case Cst.InputSourceDataStyle.EUREXMARKETDATAFILE:
                case Cst.InputSourceDataStyle.EUREXTHEORETICALPRICEFILE_QUOTE:
                    tempFileName = FilterEurexFile();
                    break;

                case Cst.InputSourceDataStyle.SPANLIFFEMARKETDATAFILE:
                case Cst.InputSourceDataStyle.LONDONSPANTHEORETICALPRICEFILE:
                    tempFileName = FilterLondonSpan2File();
                    break;

                case Cst.InputSourceDataStyle.SPANTHEORETICALPRICEFILE:
                    // Fichier SPAN contenant des noms d'échéance
                    tempFileName = FilterSpanFile(false);
                    break;

                case Cst.InputSourceDataStyle.SPANRISKPARAMETERMATDATEFILE:
                    // Fichier SPAN contenant des dates d'échéance
                    tempFileName = FilterSpanFile(true);
                    break;

                case Cst.InputSourceDataStyle.CBOETHEORETICALPRICEFILE:
                    tempFileName = FilterCBOEFile();
                    break;

                case Cst.InputSourceDataStyle.NASDAQOMXVCTFILE:
                case Cst.InputSourceDataStyle.NASDAQOMXFMSFILE:
                    //FI 20130412 : appel à FilterOMXNordicFile
                    tempFileName = FilterOMXNordicSeriesFile();
                    break;
                case Cst.InputSourceDataStyle.MEFFASSETRISKDATAFILE:
                case Cst.InputSourceDataStyle.MEFFCONTRACTASSETFILE:
                    tempFileName = FilterMeffFile();
                    break;
            }

            return tempFileName;
        }

        /// <summary>
        ///  Retourne un fichier OMXNordic allégé à partir du fichier source basé sur la structure OMXSeries 
        ///  <para>Le fichier résultat est alimenté avec l'IDASSET identifié par Spheres à partir des données OXM Series</para>
        /// </summary>
        /// <returns></returns>
        //FI 20130412 add method
        private string FilterOMXNordicSeriesFile()
        {
            string ret = GetFilePreLoading();
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Creation of the temporary working file: " + ret + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), 3));

            //IOTools_OMX io = new IOTools_OMX(m_Task, m_DataName, m_DataStyle);
            MarketDataImportOMX io = new MarketDataImportOMX(m_Task, m_DataName, m_DataStyle);
            switch (inputSourceDataStyle)
            {
                case Cst.InputSourceDataStyle.NASDAQOMXVCTFILE:
                case Cst.InputSourceDataStyle.NASDAQOMXFMSFILE:
                    io.CreateSpheresSeriesFile(ret);
                    break;
                default:
                    throw new Exception(StrFunc.AppendFormat("{0} is not implemented", inputSourceDataStyle.ToString()));
            }

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Ended", 3));

            return ret;
        }

        /// <summary>
        /// Retourne le nom du fichier utilisé pour stocker le résultat du preloading
        /// </summary>
        /// FI 20130412 add method
        private string GetFilePreLoading()
        {
            string ret = Path.GetFileName(m_DataName); //Nom du fichier sans le path
            ret = m_Task.Session.MapTemporaryPath(ret, AppSession.AddFolderSessionId.True); //Ajout d'un path sur le folder "Temporary"
            return ret;
        }

        /// <summary>
        ///  Retourne true si l'importation de type File {inputSourceDataStyle} applique un filtre particulier avant l'importation 
        /// </summary>
        /// <param name="inputSourceDataStyle"></param>
        /// <returns></returns>
        /// FI 20130412 add method
        private static bool IsFilterFile(Cst.InputSourceDataStyle inputSourceDataStyle)
        {
            bool ret = false;
            // 20220325 [XXXXX] usage de InputSourceDataStyleAttribute
            InputSourceDataStyleAttribute inputSourceAttribut = ReflectionTools.GetAttribute<InputSourceDataStyleAttribute>(inputSourceDataStyle);
            if (null != inputSourceAttribut)
                ret = inputSourceAttribut.IsFilterFile;
            return ret;


            //bool ret = (inputSourceDataStyle == Cst.InputSourceDataStyle.CLEARNETMARKETDATAFILE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.ICEFUTURESEUROPEMARKETDATAFILE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.EUREXMARKETDATAFILE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.EUREXTHEORETICALPRICEFILE_QUOTE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.SPANLIFFEMARKETDATAFILE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.LONDONSPANTHEORETICALPRICEFILE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.SPANTHEORETICALPRICEFILE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.SPANRISKPARAMETERMATDATEFILE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.NASDAQOMXFMSFILE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.NASDAQOMXVCTFILE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.CBOETHEORETICALPRICEFILE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.MEFFASSETRISKDATAFILE)
            //        || (inputSourceDataStyle == Cst.InputSourceDataStyle.MEFFCONTRACTASSETFILE);

        }

        /// <summary>
        ///  Filtre des fichiers issus du Liffe
        /// </summary>
        private string FilterLondonSpan2File()
        {
            string ret = GetFilePreLoading();
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Creation of the temporary working file: " + ret + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), 3));

            //IOTools_London_SPAN2 io_London_SPAN = new IOTools_London_SPAN2(m_Task, m_DataName, m_DataStyle);
            MarketDataImportLondonSPAN io_London_SPAN = new MarketDataImportLondonSPAN(m_Task, m_DataName, m_DataStyle);
            if (inputSourceDataStyle == Cst.InputSourceDataStyle.SPANLIFFEMARKETDATAFILE)
            {
                io_London_SPAN.Create_LightMarketDataFile(ret);
            }
            else if (inputSourceDataStyle == Cst.InputSourceDataStyle.LONDONSPANTHEORETICALPRICEFILE)
            {
                io_London_SPAN.Create_LightTheoreticalPriceFile(ret);
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", inputSourceDataStyle));
            }

            
            Logger.Log(new LoggerData(LogLevelEnum.Info, "<b>Filtering data source</b>: Ended", 3));

            return ret;
        }

        /// <summary>
        ///  Filtre des fichiers issus de Eurex
        /// </summary>
        private string FilterEurexFile()
        {
            string ret = GetFilePreLoading();
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Creation of the temporary working file: " + ret + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), 3));

            //IOTools_EUREX2 io_EUREX = new IOTools_EUREX2(m_Task, m_DataName, m_DataStyle);
            MarketDataImportEurex io_EUREX = new MarketDataImportEurex(m_Task, m_DataName, m_DataStyle);
            if (inputSourceDataStyle == Cst.InputSourceDataStyle.EUREXMARKETDATAFILE)
            {
                io_EUREX.Create_LightMarketDataFile(ret);
            }
            else if (inputSourceDataStyle == Cst.InputSourceDataStyle.EUREXTHEORETICALPRICEFILE_QUOTE)
            {
                io_EUREX.Create_LightTheoreticalPriceFile(ret);
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", inputSourceDataStyle));
            }

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Ended", 3));

            return ret;
        }

        /// <summary>
        ///  Filtre des fichiers issus de CBOE
        /// </summary>
        /// <returns></returns>
        private string FilterCBOEFile()
        {
            string ret = GetFilePreLoading();

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Creation of the temporary working file: " + ret + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), 3));

            //IOTools_CBOE2 io_CBOE = new IOTools_CBOE2(m_Task, m_DataName, m_DataStyle);
            MarketDataImportCBOE io_CBOE = new MarketDataImportCBOE(m_Task, m_DataName, m_DataStyle);
            io_CBOE.Create_LightTheoreticalPriceFile(ret);
            
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Ended", 3));

            return ret;
        }

        /// <summary>
        ///  Filtre des fichiers issus de CLEARNET-SA
        /// </summary>
        /// <returns></returns>
        private string FilterCLEARNETSAFile()
        {
            string ret = GetFilePreLoading();
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Creation of the temporary working file: " + ret + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), 3));

            //IOTools_CLEARNET2 io_CLEARNET = new IOTools_CLEARNET2(m_Task, m_DataName, m_DataStyle);
            MarketDataImportLCHClearnetSA marketImport = new MarketDataImportLCHClearnetSA(m_Task, m_DataName, m_DataStyle);
            marketImport.Create_LightMarketDataFile(ret);

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Ended", 3));

            return ret;
        }

        /// <summary>
        ///  Filtre des fichiers issus de ICE Futures Europe
        /// </summary>
        /// <returns></returns>
        private string FilterICEFuturesEuropeFile()
        {
            string ret = GetFilePreLoading();
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Creation of the temporary working file: " + ret + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), 3));

            //IOTools_CLEARNET2 io_CLEARNET = new IOTools_CLEARNET2(m_Task, m_DataName, m_DataStyle);
            MarketDataImportICEFuturesEurope marketImport = new MarketDataImportICEFuturesEurope(m_Task, m_DataName, m_DataStyle);
            marketImport.Create_LightMarketDataFile(ret);
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Ended", 3));

            return ret;
        }

        /// <summary>
        /// Filtrage des fichiers SPAN (Standard, Expanded et Expanded Paris)
        /// </summary>
        /// <param name="pIsWithMaturityDate">Indique s'il faut utiliser les dates d'échéances au lieu des noms d'échéances</param>
        /// <returns></returns>
        private string FilterSpanFile(bool pIsWithMaturityDate)
        {
            string ret = GetFilePreLoading();
            MarketDataImportSPAN io_SPAN;

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Creation of the temporary working file: " + ret + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), 3));

            if (pIsWithMaturityDate)
            {
                io_SPAN = new MarketDataImportSPAN(m_Task, m_DataName, m_DataStyle, MarketDataImportSPAN.AssetFindMaturityEnum.MATURITYDATE);
            }
            else
            {
                io_SPAN = new MarketDataImportSPAN(m_Task, m_DataName, m_DataStyle, MarketDataImportSPAN.AssetFindMaturityEnum.MATURITYMONTHYEAR);
            }
            io_SPAN.Create_LightTheoreticalPriceFile(ret);

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Ended", 3));

            return ret;
        }

        /// <summary>
        /// Filtrage des fichiers MEFF
        /// </summary>
        /// <returns></returns>
        private string FilterMeffFile()
        {
            string ret = GetFilePreLoading();
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Creation of the temporary working file: " + ret + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), 3));

            //IOTools_MEFF io_MEFF = new IOTools_MEFF(m_Task, m_DataName, m_DataStyle);
            MarketDataImportMEFF io_MEFF = new MarketDataImportMEFF(m_Task, m_DataName, m_DataStyle);
            if (inputSourceDataStyle == Cst.InputSourceDataStyle.MEFFCONTRACTASSETFILE)
            {
                io_MEFF.Create_LightContractFile(ret);
            }
            else if (inputSourceDataStyle == Cst.InputSourceDataStyle.MEFFASSETRISKDATAFILE)
            {
                io_MEFF.Create_LightRiskDataFile(ret);
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", inputSourceDataStyle));
            }

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "<b>Filtering data source</b>: Ended", 3));

            return ret;
        }

        /// <summary>
        /// <para>
        /// Retourne -1 si un l'enregistrement n'existe pas
        /// </para>
        /// <para>
        /// Retourne 1 s'il existe un enregistrement avec des données différentes
        /// </para>
        /// <para>
        /// Retourne 0 s'il existe un enregistrement avec des données identiques
        /// </para>
        /// </summary>
        /// <param name="sqlOrder"></param>
        /// <param name="opQuote_source"></param>
        /// <param name="opQuote_source_customvalue"></param>
        /// <returns></returns>
        //FI 20130503 [] add Method
        //PL 20140718 add output param
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180426 Analyse du code Correction [CA2202]
        private static int ExistData(SqlOrder pSqlOrder, ref string opQuote_source, ref string opQuote_source_customvalue)
        {
            int ret = -1;
            // PM 20180219 [23824] IOTools => IOCommonTools
            //using (IDataReader dr = pSqlOrder.ExecuteReader(IOTools.SqlAction.S))
            using (IDataReader dr = pSqlOrder.ExecuteReader(IOCommonTools.SqlAction.S))
            {
                //if (dr.Read() && (false == Convert.IsDBNull(dr.GetValue(0))))
                if (dr.Read() && (!Convert.IsDBNull(dr["Result"])))
                {
                    ret = Convert.ToInt32(dr["Result"]);
                    opQuote_source = null;
                    opQuote_source_customvalue = null;

                    if (pSqlOrder.IsQuoteTable && (!Convert.IsDBNull(dr["SOURCE"])))
                    {
                        opQuote_source = dr["SOURCE"].ToString();
                        opQuote_source_customvalue = dr["CUSTOMVALUE"].ToString();
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Importation directe des données sans passage par les étapes de parsing/mapping classique
        /// </summary>
        /// <param name="inputSourceDataStyle"></param>
        /// <param name="inputParsing"></param>
        /// FI 20131021 [17861] add Method
        ///  FI 20140617 [19911] add case EUREXPRISMA_STLPRICESFILE
        /// PM 20150707 [21104] Add SPANXMLRISKPARAMETERFILE
        /// PM 20171212 [23646] Add T7RDFFIXMLFILE
        /// EG 20220221 [XXXXX] Gestion IRQ
        private void DirectImport(Cst.InputSourceDataStyle inputSourceDataStyle, InputParsing inputParsing)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //PM 20150707 [21104] Deplacé dans le switch
            //MarketDataImportEurex rbmImport = new MarketDataImportEurex(m_Task, m_DataName, m_DataStyle);

            MarketDataImportEurexPrisma prismaImport = null;
            InputSourceDataStyleAttribute inputSourceAttribut = ReflectionTools.GetAttribute<InputSourceDataStyleAttribute>(inputSourceDataStyle);
            if ((null != inputSourceAttribut) && inputSourceAttribut.IsPRISMA)
                prismaImport = GetMarketDataImportEurexPrisma(inputSourceDataStyle);

            // PM 20160115 [] Gestion PriceOnly
            bool isPriceOnly = false;
            if (m_Task.IoTask.IsParametersSpecified && m_Task.IoTask.ExistTaskParam("PRICEONLY"))
            {
                if (false == bool.TryParse(m_Task.IoTask.parameters["PRICEONLY"], out isPriceOnly))
                {
                    isPriceOnly = false;
                }
            }

            if (false == IRQTools.IsIRQRequestedWithLog(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
            {
                switch (inputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXTHEORETICALPRICEFILE_RISKARRAY:
                        MarketDataImportEurex rbmImport = new MarketDataImportEurex(m_Task, m_DataName, m_DataStyle);
                        m_NbSourceTotalLines = rbmImport.Input_TheoreticalPriceFile_RiskArray(inputParsing, ref m_NbParsingIgnoredLines);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                        m_NbSourceTotalLines = prismaImport.InputTheoreticalPriceFile(inputParsing, ref m_NbParsingIgnoredLines);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        m_NbSourceTotalLines = prismaImport.InputRiskMeasure(inputParsing, ref m_NbParsingIgnoredLines);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_MARKETCAPACITIESFILE:
                        m_NbSourceTotalLines = prismaImport.InputMarketCapacities(inputParsing, ref m_NbParsingIgnoredLines);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_LIQUIDITYFACTORSFILE:
                        m_NbSourceTotalLines = prismaImport.InputLiquidityFactors(inputParsing, ref m_NbParsingIgnoredLines);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_FXRATESFILE:
                        m_NbSourceTotalLines = prismaImport.InputFxExchangeRates(inputParsing, ref m_NbParsingIgnoredLines);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE:
                        // FI 20220325 [XXXXX] Add codage en dur. Les tâches EUREX - RISKDATA - PRISMA, Eurosys - EUREX - RISKDATA - PRISMA, ECC - RISKDATA - PRISMA sont onsolete
                        // Il n'est plus nécessiare d'importer les données de risque dans les tables PRISMA_H
                        if (Task.IoTask.Name.EndsWith("RISKDATA - PRISMA"))
                        {
                            /*EUREX - RISKDATA - PRISMA, Eurosys - EUREX - RISKDATA - PRISMA, ECC - RISKDATA - PRISMA */
                            prismaImport.InputSettlementPriceFile(inputParsing, ref m_NbParsingIgnoredLines);
                        }
                        else
                        {
                            /*EUREX - PRICE - PRISMA */
                            MarketDataImportEurexPrismaSTLPrices prismaPriceImport = new MarketDataImportEurexPrismaSTLPrices(m_Task, m_DataName, m_DataStyle);
                            prismaPriceImport.InputSettlementPriceFile(inputParsing, ref m_NbParsingIgnoredLines);
                        }
                        break;
                    case Cst.InputSourceDataStyle.SPANXMLRISKPARAMETERFILE:
                        MarketDataImportSPANXml spanxmlImport = new MarketDataImportSPANXml(m_Task, m_DataName, m_DataStyle, isPriceOnly);
                        m_NbSourceTotalLines = spanxmlImport.ImportRiskParameterFile();
                        break;
                    case Cst.InputSourceDataStyle.T7RDFFIXMLFILE:
                        //MarketDataImportT7RDF t7RDFImport = new MarketDataImportT7RDF(m_Task, m_DataName, m_DataStyle, m_RetCodeOnNoData, m_RetCodeOnNoDataModif);
                        MarketDataImportT7RDF2 t7RDFImport = new MarketDataImportT7RDF2(m_Task, m_DataName, m_DataStyle, m_RetCodeOnNoData, m_RetCodeOnNoDataModif);
                        m_NbSourceTotalLines = t7RDFImport.ImportT7RDFFile();
                        break;
                    case InputSourceDataStyle.EURONEXTLEGACYEQYVARREFERENTIALDATAFILE:
                    case InputSourceDataStyle.EURONEXTLEGACYCOMVARREFERENTIALDATAFILE:
                    case InputSourceDataStyle.EURONEXTLEGACYEQYEXPIRYPRICESFILE:
                    case InputSourceDataStyle.EURONEXTLEGACYCOMEXPIRYPRICESFILE:
                    case InputSourceDataStyle.EURONEXTLEGACYEQYOPTIONSDELTASFILE:
                    case InputSourceDataStyle.EURONEXTLEGACYCOMOPTIONSDELTASFILE:
                    case InputSourceDataStyle.EURONEXTLEGACYSTOCKINDICESVALUESFILE:
                        // PM 20240122 [WI822] Add EURONEXTLEGACYEQYVARREFERENTIALDATAFILE, EURONEXTLEGACYCOMVARREFERENTIALDATAFILE, EURONEXTLEGACYEQYEXPIRYPRICESFILE, EURONEXTLEGACYCOMVARREFERENTIALDATAFILE, EURONEXTLEGACYEQYOPTIONSDELTASFILE, EURONEXTLEGACYCOMOPTIONSDELTASFILE, EURONEXTLEGACYSTOCKINDICESVALUESFILE
                        EuronextLegacyDataImport euronextLegacyDataImport = new EuronextLegacyDataImport(m_Task, m_DataName, m_DataStyle);
                        switch (inputSourceDataStyle)
                        {
                            case InputSourceDataStyle.EURONEXTLEGACYEQYVARREFERENTIALDATAFILE:
                            case InputSourceDataStyle.EURONEXTLEGACYCOMVARREFERENTIALDATAFILE:
                                m_NbSourceTotalLines = euronextLegacyDataImport.InputReferentialAndClosingPricesFile(inputSourceDataStyle, inputParsing, m_NbOmitRowStart);
                                break;
                            case InputSourceDataStyle.EURONEXTLEGACYEQYEXPIRYPRICESFILE:
                            case InputSourceDataStyle.EURONEXTLEGACYCOMEXPIRYPRICESFILE:
                                m_NbSourceTotalLines = euronextLegacyDataImport.InputExpiryPricesFile(inputSourceDataStyle, inputParsing, m_NbOmitRowStart);
                                break;
                            case InputSourceDataStyle.EURONEXTLEGACYEQYOPTIONSDELTASFILE:
                            case InputSourceDataStyle.EURONEXTLEGACYCOMOPTIONSDELTASFILE:
                                m_NbSourceTotalLines = euronextLegacyDataImport.InputOptionsDeltasFile(inputSourceDataStyle, inputParsing, m_NbOmitRowStart);
                                break;
                            case InputSourceDataStyle.EURONEXTLEGACYSTOCKINDICESVALUESFILE:
                                m_NbSourceTotalLines = euronextLegacyDataImport.InputStockIndicesValuesFile(inputParsing, m_NbOmitRowStart);
                                break;
                        }
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("InputSourceDataStyle: {0} is not implemented", inputSourceDataStyle.ToString()));
                }
            }
        }


        /// <summary>
        ///  Retourne la classe charger d'importer les données PRISMA
        /// </summary>
        /// <param name="inputSourceDataStyle"></param>
        /// <returns></returns>
        /// FI 20220325 [XXXXX] Add
        private MarketDataImportEurexPrisma GetMarketDataImportEurexPrisma(Cst.InputSourceDataStyle inputSourceDataStyle)
        {

            // PM 20141022 [9700] Eurex Prisma for Eurosys Futures
            bool isEurosysSoftware = false; // Indique si l'import est à destination d'Eurosys Futures ou non
            if (m_Task.IoTask.IsParametersSpecified && m_Task.IoTask.ExistTaskParam("SOFTWARE"))
            {
                string software = m_Task.IoTask.parameters["SOFTWARE"];
                isEurosysSoftware = (software == Software.SOFTWARE_EurosysFutures);
            }
            MarketDataImportEurexPrisma prismaImport;
            if (isEurosysSoftware)
            {
                prismaImport = new EurosysMarketDataImportEurexPrisma(m_Task, m_DataName, m_DataStyle);
            }
            else
            {
                prismaImport = new MarketDataImportEurexPrisma(m_Task, m_DataName, m_DataStyle);
            }
            return prismaImport;
        }

        /// <summary>
        /// Retourne true si l'importation doit être exécutée
        /// <para>Pour les importations de type RISKDATA, l'importation est fonction de la valeur du paramètre PRICEONLY</para>
        /// </summary>
        /// <param name="inputSourceDataStyle"></param>
        /// <returns></returns>
        /// FI 20131113 [17861] Add Method
        /// FI 20140617 [19911] gestion du type EUREXPRISMA_STLPRICESFILE
        /// PM 20150707 [21104] Add SPANXMLRISKPARAMETERFILE
        /// PM 20171212 [23646] Add T7RDFFIXMLFILE
        private Boolean IsFileDirectImportOk(Cst.InputSourceDataStyle inputSourceDataStyle)
        {
            Boolean isOk = true;
            switch (inputSourceDataStyle)
            {
                case Cst.InputSourceDataStyle.EUREXTHEORETICALPRICEFILE_RISKARRAY:
                case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                case Cst.InputSourceDataStyle.EUREXPRISMA_LIQUIDITYFACTORSFILE:
                case Cst.InputSourceDataStyle.EUREXPRISMA_MARKETCAPACITIESFILE:
                case Cst.InputSourceDataStyle.EUREXPRISMA_FXRATESFILE:
                    if (m_Task.IoTask.IsParametersSpecified && m_Task.IoTask.ExistTaskParam("PRICEONLY"))
                        isOk = BoolFunc.IsFalse(m_Task.IoTask.parameters["PRICEONLY"]);
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE:
                    // PM 20151124 [20124] Importation des cours via le fichier Prisma "Settlement_Prices" : isOk est laissé à true
                    //if (m_Task.IoTask.IsParametersSpecified && m_Task.IoTask.ExistTaskParam("PRICEONLY"))
                    //    isOk = BoolFunc.IsFalse(m_Task.IoTask.parameters["PRICEONLY"]);
                    break;
                case Cst.InputSourceDataStyle.SPANXMLRISKPARAMETERFILE:
                case Cst.InputSourceDataStyle.T7RDFFIXMLFILE:
                case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYVARREFERENTIALDATAFILE:
                case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMVARREFERENTIALDATAFILE:
                case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYEXPIRYPRICESFILE:
                case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMEXPIRYPRICESFILE:
                case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYOPTIONSDELTASFILE:
                case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMOPTIONSDELTASFILE:
                case Cst.InputSourceDataStyle.EURONEXTLEGACYSTOCKINDICESVALUESFILE:
                    // PM 20240122 [WI822] Add EURONEXTLEGACYEQYVARREFERENTIALDATAFILE, EURONEXTLEGACYCOMVARREFERENTIALDATAFILE, EURONEXTLEGACYEQYEXPIRYPRICESFILE, EURONEXTLEGACYCOMEXPIRYPRICESFILE, EURONEXTLEGACYEQYOPTIONSDELTASFILE, EURONEXTLEGACYCOMOPTIONSDELTASFILE, EURONEXTLEGACYSTOCKINDICESVALUESFILE
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("InputSourceDataStyle: {0} is not implemented", inputSourceDataStyle.ToString()));
            }
            return isOk;
        }

        /// <summary>
        ///  Recupère le flux post-Parsing lorsque les données sont dans un fichier 
        /// </summary>
        /// <param name="inputParsing"></param>
        /// <param name="postParsingFlow"></param>
        /// <param name="isFileOnError"></param>
        /// FI 20131021 [17861] add Method
        private void PostParsingFlowFromFile(InputParsing pInputParsing, ref IOTaskDetInOutFile pPostParsingFlow, ref Boolean pIsFileOnError)
        {
            //FI 20130412 appel à la méthode FilePreLoading
            string tempFileName = string.Empty;
            if (IsFilterFile(inputSourceDataStyle))
                tempFileName = FilterFile();

            string fileToImport = m_DataName;
            if (StrFunc.IsFilled(tempFileName))
                fileToImport = tempFileName;

            IOTools.InitPostParsingFlow(true, fileToImport, m_DataStyle, ref pPostParsingFlow);
            // PM 20180219 [23824] IOTools => IOCommonTools
            //IOTools.OpenFile(fileToImport, m_DataStyle);
            IOCommonTools.OpenFile(fileToImport, m_DataStyle);

            LoadLinesFromSource(false, pInputParsing, ref pPostParsingFlow, ref pIsFileOnError);

        }

       

        /// <summary>
        /// valorise m_DataName 
        /// </summary>
        /// <param name="inputSourceDataStyle"></param>
        /// FI 20131021 [17861] add method
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void SetDataName(Cst.InputSourceDataStyle inputSourceDataStyle)
        {
            base.SetDataName(inputSourceDataStyle);

            //PL 20130527 Code suivant anciennement présent dans: ReadingFromSource(). Remonté ici pour pouvoir loguer le nom réel du fichier de données importées.
            if (StrFunc.ContainsIn(m_DataName, "<Data "))
                m_DataName = StringDynamicData.GetDynamicstring(m_Task.Cs, m_DataName);

            // PM 20180219 [23824] IOTools => IOCommonTools
            m_DataName = IOCommonTools.CheckDynamicString(m_DataName, m_Task, false);

            bool isData = ((inputSourceDataStyle == Cst.InputSourceDataStyle.UNICODEDATA) ||
                (inputSourceDataStyle == Cst.InputSourceDataStyle.ANSIDATA) ||
                (inputSourceDataStyle == Cst.InputSourceDataStyle.XMLDATA));

            if (!(isData && m_Task.m_IOMQueue.dataSpecified))
            {

                
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6034), 2, new LogParam(m_DataName)));
            }

        }

        /// <summary>
        /// Parse {paction} en IOCommonTools.SqlAction
        /// </summary>
        /// <param name="pAction"></param>
        /// <exception cref="Exception lorsque pAction n'est pas une valeur de l'enum"></exception>
        /// <returns></returns>
        /// FI 20131122 [19233] Add method
        private static IOCommonTools.SqlAction ParseAction(string pAction)
        {
            IOCommonTools.SqlAction ret;
            if (Enum.IsDefined(typeof(IOCommonTools.SqlAction), pAction))
                ret = (IOCommonTools.SqlAction)Enum.Parse(typeof(IOCommonTools.SqlAction), pAction);
            else
                throw new Exception("[Specified Action is not managed: " + pAction + "]");

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="pMsg"></param>
        /// <returns></returns>
        // FI 20131122 [19233] Add Method
        // EG 20190114 Add detail to ProcessLog Refactoring
        private static string BuildLogMsgDetail(IOTaskDetInOutFileRow pRow, string pMsg)
        {
            StrBuilder msg = new StrBuilder();
            msg += pRow.LogRowDesc + Cst.CrLf;
            msg += pMsg;
            if (StrFunc.IsFilled(pRow.LogRowInfoMsg))
                msg += Cst.CrLf + pRow.LogRowInfoMsg;

            return msg.ToString();
        }

        /// <summary>
        ///  Génère une nouvelle instance d'une processus d'importation de trade
        /// </summary>
        /// <returns></returns>
        /// FI 20131213 [19337] add method
        /// FI 20161214 [21916] Modify
        /// FI 20180207 [XXXXX] Methode Static
        private static ProcessTradeImportBase NewProcessTradeImport(Task pTask, IOTaskDetInOutFileRow pRow, TradeImport pTradeImport, IDbTransaction pDbTransaction)
        {

            SQL_Instrument sqlInstr = GetImportSettingInstrument(pTask.Cs, pDbTransaction, pTradeImport.settings,
                        TradeImportCst.instrumentIdentifier, new string[] { "IDENTIFIER", "PRODUCT_IDENTIFIER" });

            bool isDebtSecurity = (sqlInstr.Product_Identifier.ToUpper() == Cst.ProductDebtSecurity.ToUpper());

            // FI 20161214 [21916] add  ProductMarginRequirement (Permettre l'importation des trades déposit)
            bool isTradeRisk = (sqlInstr.Product_Identifier.ToUpper() == Cst.ProductCashPayment.ToUpper() ||
                                sqlInstr.Product_Identifier.ToUpper() == Cst.ProductMarginRequirement.ToUpper());

            ProcessTradeImportBase ret;
            if (isDebtSecurity)
                ret = new ProcessDebtSecImport(pTradeImport, pDbTransaction, pTask);
            else if (isTradeRisk)
                ret = new ProcessTradeRiskImport(pTradeImport, pDbTransaction, pTask);
            else
                ret = new ProcessTradeImport(pTradeImport, pDbTransaction, pTask);

            // FI 20131122 [19233] alimentation de LogHeader
            ret.LogHeader = pRow.LogRowDesc;

            // RD 20100608 [17038] Refactoring dans le cadre de LSD_TradeImport
            ret.SetParameter(TradeImportCst.instrumentIdentifier, sqlInstr.Identifier);

            return ret;
        }


        /// <summary>
        /// <para>Retourne l'action résultante (Nouvelle insertion, Mise à jour, etc..)</para>
        /// <para>Retourne true s'il existe au moins une condition non respectée dont le statut est StatusEnum.ERROR</para>
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pRow"></param>
        /// <param name="pTradeImport"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pSetErrorWarning"></param>
        /// <returns></returns>
        /// FI 20211206 [XXXXX] Add Method (Usage de TryMultiple)
        private static IOCommonTools.SqlAction TradeImport2(Task pTask, IOTaskDetInOutFileRow pRow, TradeImport pTradeImport, IDbTransaction pDbTransaction, SetErrorWarning pSetErrorWarning)
        {

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "Initialize…", 4));

            CheckTradeImportSettings(pTradeImport);


            
            Logger.Write();


            IOCommonTools.SqlAction ret = IOCommonTools.SqlAction.NA;
            try
            {
                TryMultiple tryMultiple = new TryMultiple(pTask.Cs, "ImportTrade", "Import Trade")
                {
                    LogHeader = pRow.LogRowDesc,
                    LogRankOrder = 3,
                };
                ret = tryMultiple.Exec(() =>
                {
                    ProcessTradeImportBase processTradeImport = NewProcessTradeImport(pTask, pRow, pTradeImport, pDbTransaction);
                    processTradeImport.InitDelegate(pSetErrorWarning);
                    return processTradeImport.Process();
                });
            }
            finally
            {

                
                Logger.Write();
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        private void FinalizeIOTrack(bool pIsOk)
        {
            if (m_CommitMode == Cst.CommitMode.FULLCOMMIT)
            {
                if (false == pIsOk)
                    m_IOTrackDetails.Clear();
            }

            string filePath = string.Empty;
            string fileDate = string.Empty;
            string fileSize = string.Empty;
            //
            if ((null != m_TaskPostMapping) &&
                (null != m_TaskPostMapping.taskDet) &&
                (null != m_TaskPostMapping.taskDet.input) &&
                (null != m_TaskPostMapping.taskDet.input.file))
            {
                filePath = m_TaskPostMapping.taskDet.input.file.folder;
                //
                if (StrFunc.IsFilled(filePath) && filePath.EndsWith(@"/") == false)
                    filePath += @"/";
                //
                filePath += m_TaskPostMapping.taskDet.input.file.name;
                // 
                fileDate = m_TaskPostMapping.taskDet.input.file.date;
                fileSize = m_TaskPostMapping.taskDet.input.file.size;
            }
            else
            {
                filePath = TaskPostParsing.taskDet.input.file.folder;
                //
                if (StrFunc.IsFilled(filePath) && filePath.EndsWith(@"/") == false)
                    filePath += @"/";
                //
                filePath += TaskPostParsing.taskDet.input.file.name;
                //
                fileDate = TaskPostParsing.taskDet.input.file.date;
                fileSize = TaskPostParsing.taskDet.input.file.size;
            }
            //
            m_IOTrackFileMsg = "[File: " + filePath + "]" + Cst.CrLf;
            //
            if (StrFunc.IsFilled(fileDate))
                m_IOTrackFileMsg += "[Date: " + fileDate + "]" + Cst.CrLf;
            //
            if (StrFunc.IsFilled(fileSize))
                m_IOTrackFileMsg += "[Size: " + fileSize + "]" + Cst.CrLf;

            bool isXmlInput = ((m_DataStyle == Cst.InputSourceDataStyle.XMLDATA.ToString()) ||
                (m_DataStyle == Cst.InputSourceDataStyle.XMLFILE.ToString()));

            // Pour un fichier XML le nombre de lignes n'est pas significatif
            if (false == isXmlInput)
                m_IOTrackFileMsg += "[Row(s): " + m_NbSourceTotalLines.ToString() + "]" + Cst.CrLf;

            int totalInsertedRows = (from detail in m_IOTrackDetails select detail.InsertedRows).Sum();
            int totalUpdatedRows = (from detail in m_IOTrackDetails select detail.UpdatedRows).Sum();
            int totalDeleteddRows = (from detail in m_IOTrackDetails select detail.DeletedRows).Sum();

            if (false == isXmlInput)
            {
                m_IOTrackDatasIdent = new string[5] {
                    "Ignored rows by setting",
                    "Rejected rows by controls",
                    "Inserted data",
                    "Modified data",
                    "Removed data" };
                m_IOTrackDatas = new string[5] {
                    m_NbParsingIgnoredLines.ToString(),
                    m_NbRejectedRows.ToString(),
                    totalInsertedRows.ToString(),
                    totalUpdatedRows.ToString(),
                    totalDeleteddRows.ToString() };
            }
            else
            {
                m_IOTrackDatasIdent = new string[4] {
                    "Rejected rows by controls",
                    "Inserted data",
                    "Modified data",
                    "Removed data" };
                m_IOTrackDatas = new string[4] {
                    m_NbRejectedRows.ToString(),
                    totalInsertedRows.ToString(),
                    totalUpdatedRows.ToString(),
                    totalDeleteddRows.ToString() };
            }

            if (m_IOTrackDetails.Count > 0)
            {
                // RD 20120830 [18102] Gestion des compteurs IOTRACK
                // 1- Rechercher la description dans le paramétrage IOINPUT.DATATARGETDESC
                if (StrFunc.IsFilled(m_DataTargetDescription))
                {
                    List<string> targetDescs = new List<string>(m_DataTargetDescription.Split(';'));
                    foreach (string targetDesc in targetDescs)
                    {
                        string[] aTargetDesc = targetDesc.Split(':');

                        if (aTargetDesc.Length > 1)
                        {
                            (from data in m_IOTrackDetails
                             where data.TableName == aTargetDesc[0].ToUpper()
                             select data).ToList()
                            .ForEach(data => data.TableDescription = aTargetDesc[1]);
                        }
                    }
                }

                // 2- Sinon, Rechercher la description dans l'Enum Cst.OTCml_TBL
                (from data in m_IOTrackDetails
                 where StrFunc.IsEmpty(data.TableDescription) && Enum.IsDefined(typeof(Cst.OTCml_TBL), data.TableName)
                 select data).ToList()
                .ForEach(data => data.TableDescription =
                    Cst.GetDescription((Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), data.TableName)));

                foreach (IOTrackDetail detail in m_IOTrackDetails
                    .OrderBy(desc => StrFunc.IsFilled(desc.TableDescription))
                    .ThenBy(desc => desc.TableDescription)
                    .ThenBy(desc => desc.TableName).ToList())
                {
                    if (StrFunc.IsFilled(detail.TableDescription))
                        m_IOTrackLotxt += Cst.HTMLBold + detail.TableDescription + Cst.HTMLEndBold +
                            " [" + detail.TableName + "]" + Cst.CrLf;
                    else
                        m_IOTrackLotxt += detail.TableName + Cst.CrLf;

                    m_IOTrackLotxt += (detail.InsertedRows > 0 ? Cst.HTMLBold : string.Empty) +
                            "- Inserted " + Cst.Tab + detail.InsertedRows.ToString() +
                            (detail.InsertedRows > 0 ? Cst.HTMLEndBold : string.Empty) + Cst.CrLf;
                    m_IOTrackLotxt += (detail.UpdatedRows > 0 ? Cst.HTMLBold : string.Empty) +
                            "- Modified " + Cst.Tab + detail.UpdatedRows.ToString() +
                            (detail.UpdatedRows > 0 ? Cst.HTMLEndBold : string.Empty) + Cst.CrLf;
                    m_IOTrackLotxt += (detail.DeletedRows > 0 ? Cst.HTMLBold : string.Empty) +
                            "- Removed" + Cst.Tab + detail.DeletedRows.ToString() +
                            (detail.DeletedRows > 0 ? Cst.HTMLEndBold : string.Empty) + Cst.CrLf;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nbRecordRejected"></param>
        /// <param name="lstIOTrackDetail"></param>
        /// FI 20201119 [XXXXX] Add Method 
        private void RowTrack(int nbRecordRejected, List<IOTrackDetail> lstIOTrackDetail)
        {
            m_NbRejectedRows += nbRecordRejected;
            AddIOTrackDetail(m_IOTrackDetails, lstIOTrackDetail);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pRow"></param>
        /// <param name="pTradeImport"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pInputElement"></param>
        /// <returns></returns>
        /// FI 20180207 [XXXXX] New method en vue d'un usage multithreading
        private static async System.Threading.Tasks.Task<IOCommonTools.SqlAction> TradeImportAsync(Task pTask, IOTaskDetInOutFileRow pRow, TradeImport pTradeImport, IDbTransaction pDbTransaction,
            SetErrorWarning pInputElement)
        {
            IOCommonTools.SqlAction ret = await System.Threading.Tasks.Task.Run(() => TradeImport2(pTask, pRow, pTradeImport, pDbTransaction, pInputElement));
            return ret;
        }

        /// <summary>
        ///  Traitement d'un row de Mapping avec plusieurs tentatives en cas de deadLock ou Timeout.
        ///  <para>Ce traitement s'applique uniquement si RECORDCOMMIT </para>
        /// </summary>
        /// <param name="row"></param>
        /// <param name="pPassMessage"></param>
        /// FI 20210128 [XXXXX] Add
        private void TryMultiple2(IOTaskDetInOutFileRow row, string pPassMessage)
        {
            TryMultiple @try = new TryMultiple(Task.Cs, "RowImport", BuildLogMsgDetail(row, pPassMessage).Replace(Cst.CrLf, Cst.Space))
            {
                SetErrorWarning = ProcessState.SetErrorWarning,
                IsModeTransactional = true,
                TransactionType = TransactionTypeEnum.External,
            };
            @try.InitTransactionDelegate(delegate { BeginInputTran(); }, delegate { CommitInputTran(); }, delegate { RollbackInputTran(); });
            @try.Exec<IOTaskDetInOutFileRow, string>(delegate (IOTaskDetInOutFileRow arg1, string arg2) { RowProcess(arg1, arg2); }, row, pPassMessage);
        }

        /// <summary>
        ///  Traitement d'un row de Mapping 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="pPassMessage"></param>
        private void RowProcess(IOTaskDetInOutFileRow row, string pPassMessage)
        {
            int rowNbRecordRejected = 0;
            List<IOTrackDetail> rowIOTrackDetail = new List<IOTrackDetail>();

            // FI 20201221 [XXXX] alimentation de currentRowStatus
            currentRowStatus = ProcessStateTools.StatusEnum.SUCCESS;

            // Importation des trades
            if (ArrFunc.IsFilled(row.tradeImport))
            { 
                // FI 20201221 [XXXX] Appel à SetErrorWarningCurrentRow 
                RowImport(row, row.tradeImport, pPassMessage, out IOTrackDetail tradeIOTrackDetail, out int tradeImportNbRecordRejected, SetErrorWarningCurrentRow);

                AddIOTrackDetail(rowIOTrackDetail, tradeIOTrackDetail);
                rowNbRecordRejected += tradeImportNbRecordRejected;
            }

            // Importation des posRequest
            if (ArrFunc.IsFilled(row.posRequestImport))
            {
                // FI 20201221 [XXXX] Appel à SetErrorWarningCurrentRow 
                RowImport(row, row.posRequestImport, pPassMessage, out IOTrackDetail posrequestIOTrackDetail, out int posRequestImportNbRecordRejected, SetErrorWarningCurrentRow);

                AddIOTrackDetail(rowIOTrackDetail, posrequestIOTrackDetail);
                rowNbRecordRejected += posRequestImportNbRecordRejected;
            }

            // Importation des tables
            if (ArrFunc.IsFilled(row.table))
            {
                // FI 20201221 [XXXX] Appel à SetErrorWarningCurrentRow 
                RowTableImport(row, row.table, pPassMessage, out List<IOTrackDetail> tableIOTrackDetail, out int tableImportNbRecordRejected, SetErrorWarningCurrentRow);

                AddIOTrackDetail(rowIOTrackDetail, tableIOTrackDetail);
                rowNbRecordRejected += tableImportNbRecordRejected;
            }

            RowTrack(rowNbRecordRejected, rowIOTrackDetail);

            /* FI 20211202 [XXXX] Le statut du row est utilisé pour alimenter le statut de l'élément
            // FI 20201221 [XXXX] Le statut du row est utilisé pour alimenter le statut de la tâche
            Task.SetErrorWarning(currentRowStatus);
            */
            ProcessState.SetErrorWarning(currentRowStatus);

            if (currentRowStatus == ProcessStateTools.StatusEnum.ERROR)
                m_IsWritingError = true;
        }

        #endregion Methods
    }
}

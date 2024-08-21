#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.EfsSend;
using EFS.Common.IO;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresIO.Compare;
using EFS.SpheresIO.Trade;
//
using EFS.TradeInformation;
using EFS.TradeInformation.Import;
//
using EfsML.Business;
using EfsML.DynamicData;
//
using FpML.Interface;
#endregion

namespace EFS.SpheresIO
{
    // RD 20100305 
    // 1- Affichage des Messages de log concernant le Début et la fin de chaque étape (Parsing, Mapping, Writing) y compris en niveau de Log NONE.
    // 2- Gestion de la check « IsHeterogenous » sur les parsing.
    // 3- Possibilité de rajouter un Filtre sur le chemin d’un fichier source XML : 
    //      Exemple :  \\SVR-MSS-2\SVR-MSS-2-partage\FOml_MarketDataFiles\Data\span.xml;Element="exchange";Filter="//../exch[text()='CBT']"
    // 4- Utilisation du Cache Spheres en mode AUTOCOMMIT et si c’est spécifié sur le « row » PostMapping.

    // RD 20120626 [17950] Task: Execution of the element under condition
    // RD 20120626 [17939] Task: Send an email after execution

    /// <summary>
    /// 
    /// </summary>
    public class SpheresIOProcess : ProcessBase
    {
        #region Members
        /// <summary>
        /// Message queue déclencheur du traitement
        /// </summary>
        public IOMQueue m_IOMQueue;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected override string DataIdent
        {
            get
            {
                return "IOTASK";
            }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pAppInstance"></param>
        public SpheresIOProcess(MQueueBase pMQueue, AppInstanceService pAppInstance) :
            base(pMQueue, pAppInstance)
        {
            m_IOMQueue = (IOMQueue)pMQueue;
            // FI 20240118 [WI814] L'appel pourra être supprimé lorsque les versions des gateways en prod seront >= v14
            m_IOMQueue.SetIsGateway();
        }
        #endregion Constructor

        #region Methods
        //------------------------------------------------------
        // RD 200912108 Merci de ne pas supprimer ces méthodes
        //------------------------------------------------------
        private Cst.ErrLevel ImportExcel()
        {
            //string fileName = @"C:\Temp\Test.xls";
            //string sheetName = "Feuil1";
            //string range = string.Empty;
            //int nbData = 6; 

            string fileName = @"C:\Temp\euribor_2009.xls";
            string sheetName = "Euribor";
            string range = string.Empty; // du genre B1:HV16
            string oleDbCs = "Provider=Microsoft.ACE.OLEDB.12.0; Extended Properties=\"Excel 12.0;HDR=No;IMEX=1\"";
            oleDbCs = "Data Source=" + fileName + ";" + oleDbCs.Trim();
            //string oleDbConnection = "Provider=Microsoft.Jet.OLEDB.4.0; Extended Properties=\"Excel 8.0;HDR=No;IMEX=1\"";

            bool isLinearParsing = false;
            //
            string selectSQL = "select * from [" + sheetName + "$" + range + "]";
            DataSet dsResult = DataHelper.ExecuteDataset(oleDbCs, Cs, CommandType.Text, selectSQL);
            DataTable dtResult = dsResult.Tables[0];
            DataRow[] drResult = dtResult.Select();
            //
            IOTaskDetInOutFileRow[] AllRows = null;
            int rowNumber = 0;
            //
            if (false == isLinearParsing)
            {
                //while (drResult.Read() && rowNumber < nbData)
                foreach (DataRow row in drResult)
                {
                    if (false == ArrFunc.IsAllElementEmpty(row.ItemArray))
                    {
                        //int columnCount = drResult.FieldCount;
                        int columnCount = dtResult.Columns.Count;
                        string dataName = string.Empty;
                        //
                        if (null == AllRows)
                            AllRows = new IOTaskDetInOutFileRow[columnCount];
                        //
                        for (int j = 0; j < columnCount; j++)
                        {
                            TypeData.TypeDataEnum typeData = TypeData.GetTypeDataEnum(dtResult.Columns[j].DataType.Name);

                            if (0 == rowNumber)
                                typeData = TypeData.TypeDataEnum.date;
                            //
                            string fmtIsoValue = null;
                            if (false == Convert.IsDBNull(row[j]))
                            {
                                fmtIsoValue = row[j].ToString();

                                if (TypeData.IsTypeDate(typeData) && (false == StrFunc.IsDate(fmtIsoValue, DtFunc.FmtShortDate)))
                                {
                                    double d = double.Parse(fmtIsoValue);
                                    DateTime dtValue = DateTime.FromOADate(d);

                                    fmtIsoValue = ObjFunc.FmtToISo(dtValue, typeData);
                                }
                            }
                            if (0 == j)
                                dataName = fmtIsoValue;

                            if (null == AllRows[j])
                                AllRows[j] = new IOTaskDetInOutFileRow();

                            if (null == AllRows[j].data)
                                AllRows[j].data = new IOTaskDetInOutFileRowData[drResult.Length];

                            AllRows[j].data[rowNumber] = new IOTaskDetInOutFileRowData
                            {
                                datatype = typeData.ToString(), // Type de donnée de parsing
                                name = dataName,// drResult.GetName(j);// Nom de donnée dans le parsing
                                value = fmtIsoValue
                            };

                            //AllRows[j].src = drResult.GetName(j);
                            AllRows[j].src = dtResult.Columns[j].ColumnName;
                        }
                        //
                        rowNumber++;
                    }
                }
            }
            else
            {
                // ToDo ( voir ProcessOutput.ReadingFromSource())
            }
            //
            //drResult.Close();
            //connection.Close();
            //
            IOTask task = new IOTask
            {
                taskDet = new IOTaskDet
                {
                    input = new IOTaskDetInOut
                    {
                        file = new IOTaskDetInOutFile
                        {
                            row = AllRows
                        }
                    }
                }
            };

            IOTools.XmlSerialize(typeof(IOTask), task, @"C:\Temp\Test.xml");
            return Cst.ErrLevel.SUCCESS;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            Task ioTask = null;
            LoggerData startTaskLog = default;
            try
            {
                
                startTaskLog = new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6001), 0, new LogParam(m_IOMQueue.identifier));
                Logger.Log(startTaskLog);

                ioTask = new Task(this);

                ret = ioTask.ProcessTask();

            }
            catch (Exception ex)
            {
                // FI 20220803 [XXXXX] Trace déjà alimentée par le logger
                if (false == LoggerManager.IsEnabled)
                {
                    // FI 20200918 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                    Common.AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));
                }

                SpheresException2 spheresException = SpheresExceptionParser.GetSpheresException(null, ex);

                // FI 20200623 [XXXXX] AddException
                ProcessState.AddException(spheresException); // PL 20130702 [WindowsEvent Project] Newness
                
                
                Logger.Log(new LoggerData(spheresException));

                ret = Cst.ErrLevel.FAILURE;
            }
            finally
            {
                // RD 20130329 [18542] Gestion du statut des message de Log Début et Fin de la tache
                if (ioTask != null)
                {
                    // EG 20140115 
                    //ioTask.TaskStatus = ProcessStateTools.StatusEnum.ERROR;  

                    
                    // Log: Fin du traitement de la tâche
                    //LoggerData endTaskLog = new LoggerData(LoggerTools.StatusToLogLevelEnum(ioTask.TaskStatus), new SysMsgCode(SysCodeEnum.LOG, 6002), 0, new LogParam(m_IOMQueue.identifier));
                    //Logger.Log(endTaskLog);
                    // La mise à jour du message de début de traitement de l'élément n'est plus nécessaire
                    if ((ProcessStateTools.StatusErrorEnum == ioTask.TaskStatus) || (ProcessStateTools.StatusWarningEnum == ioTask.TaskStatus))
                    {
                        // Gestion GRPNO : La mise à jour du message de début de traitement de l'élément n'est plus nécessaire
                        //// Mise à jour du statut du message de début de traitement de la tâche ("LOG-06001")
                        //Logger.UpdateLogLevel(startTaskLog, LoggerTools.StatusToLogLevelEnum(ioTask.TaskStatus));
                        LoggerData endTaskLog = new LoggerData(LoggerTools.StatusToLogLevelEnum(ioTask.TaskStatus), new SysMsgCode(SysCodeEnum.LOG, 6002), 0, new LogParam(m_IOMQueue.identifier));
                        Logger.Log(endTaskLog);
                    }
                    else
                    {
                        // Gestion GRPNO : La mise à jour du message de début de traitement de l'élément n'est plus nécessaire
                        //// Mise à jour du statut du message de début et de fin de traitement de la tâche en vérifiant chaque message (car certaines tâches crées des Warning bien qu'étant en success)
                        //Logger.UpdateWithWorstLogLevel(startTaskLog, endTaskLog);
                        LogLevelEnum worstLogLevelEnum = Logger.CurrentScope.GetWorstLogLevel(startTaskLog);
                        // PM 20200909 [XXXXX] Passage du Level en None si Info pour affichage systèmatique
                        worstLogLevelEnum = (worstLogLevelEnum == LogLevelEnum.Info ? LogLevelEnum.None : worstLogLevelEnum);
                        LoggerData endTaskLog = new LoggerData(worstLogLevelEnum, new SysMsgCode(SysCodeEnum.LOG, 6002), 0, new LogParam(m_IOMQueue.identifier));
                        Logger.Log(endTaskLog);
                    }
                    //Logger.Write();

                    // Mise à jour du statut du process
                    ProcessState.SetErrorWarning(ioTask.TaskStatus);
                }
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SelectDatas()
        {
            if (MQueue.idSpecified || MQueue.identifierSpecified)
            {
                DataParameters parameters = new DataParameters();
                if (MQueue.idSpecified)
                    parameters.Add(new DataParameter(Cs, "IDIOTASK", DbType.Int32), MQueue.id);
                else if (MQueue.identifierSpecified)
                    parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDENTIFIER), MQueue.identifier);

                StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT + @"t.IDIOTASK As IDDATA" + Cst.CrLf);
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOTASK + " t" + Cst.CrLf;

                SQLWhere sqlWhere = new SQLWhere();
                if (MQueue.idSpecified)
                    sqlWhere.Append(@"(t.IDIOTASK=@IDIOTASK)");
                else if (MQueue.identifierSpecified)
                    sqlWhere.Append(@"(t.IDENTIFIER=@IDENTIFIER)");

                sqlSelect += sqlWhere.ToString();
                string sqlSelectTrade = sqlSelect.ToString();
                DsDatas = DataHelper.ExecuteDataset(Cs, CommandType.Text, sqlSelectTrade, parameters.GetArrayDbParameter());
            }
            /* FI 20200916 [XXXXX] Mis en commentaire, IsProcessEnded n'est plus utilisé
             else
            {
                // RD 20140123 [19526] Don't process message queue without Id            
                IsProcessEnded = true;
            }
            */
        }

        /// <summary>
        /// Alimente, Enrichie le message Queue lorsque ce dernier est pauvre
        /// <para>Les messsages issus des gateways sont pauvres puisqu'ils ne contiennent que identifier de tâche</para>
        /// </summary>
        /// FI 20130129 mQueue.idInfo doit être renseigné puisqu'il est utilisé pour alimenter le tracker lorsque le message est issu d'une gateway
        protected override void ProcessInitializeMqueue()
        {
            base.ProcessInitializeMqueue();

            if (false == IsProcessObserver)
            {
                if (false == MQueue.idInfoSpecified)
                {
                    IdInfo mqueueInfo = SQL_TableTools.GetIOMQueueIdInfo(CSTools.SetCacheOn(Cs), MQueue.id);
                    MQueue.idInfoSpecified = (null != mqueueInfo);
                    if (MQueue.idInfoSpecified)
                        MQueue.idInfo = mqueueInfo;
                }
            }
        }


        #endregion Methods
    }

}


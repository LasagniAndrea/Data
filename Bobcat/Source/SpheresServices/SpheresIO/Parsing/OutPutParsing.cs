// PM 20180219 [23824] Déplacé dans CommonIO

//#region Using Directives
//using System;
//using System.Collections;
//using System.Reflection;
//using System.Data;
//using System.Globalization;
//using System.Text.RegularExpressions;

//using EFS.ACommon;
//using EFS.Common;
//using EFS.Common.IO;
//using EFS.Common.Log;
//using EFS.ApplicationBlocks.Data;

//using EfsML;
//using EfsML.Business;
//using EfsML.Enum;
//#endregion Using Directives

//namespace EFS.SpheresIO
//{

//    /// <summary>
//    /// 
//    /// </summary>
//    public class OutputParsing : IOParsing
//    {

//        #region Accessors
//        /// <summary>
//        /// 
//        /// </summary>
//        protected override string IOParsingTableName
//        {
//            get { return Cst.OTCml_TBL.IOOUTPUT_PARSING.ToString(); }
//        }
//        #endregion

//        #region Constructor
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pIdOutput"></param>
//        /// <param name="pTask"></param>
//        public OutputParsing(string pIdOutput, Task pTask)
//            : base(pIdOutput, pTask)
//        {
//            m_IsIn = false;
//        }
//        #endregion Constructor

//        #region Methods
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pIdOutput"></param>
//        /// <returns></returns>
//        protected override DataSet LoadIOParsing(string pIdOutput)
//        {
//            string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf;
//            //
//            sqlQuery += SQLCst.SELECT + IOParsingTableName + ".IDIOPARSING, " + IOParsingTableName + ".SEQUENCENO, " + Cst.CrLf;
//            sqlQuery += IOParsingTableName + ".DATASECTION, " + IOParsingTableName + ".DATANAME, " + Cst.CrLf;
//            sqlQuery += IOParsingTableName + ".DATAVALUE ";
//            //
//            sqlQuery += GetJoinIOParsingSQLQuery();
//            //
//            sqlQuery += SQLCst.WHERE + IOParsingTableName + ".IDIOOUTPUT = " + DataHelper.SQLString(pIdOutput) + Cst.CrLf;
//            sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(m_Task.Cs, IOParsingTableName) + Cst.CrLf;
//            sqlQuery += SQLCst.ORDERBY + IOParsingTableName + ".SEQUENCENO" + Cst.CrLf;

//            return DataHelper.ExecuteDataset(m_Task.Cs, CommandType.Text, sqlQuery);

//        }

//        /// <summary>
//        /// Fabriquer la ligne à Ecrire dans le fichier Destination
//        /// </summary>
//        /// <param name="pRow"></param>
//        /// <param name="pLine"></param>
//        /// <returns></returns>
//        public Cst.ErrLevel MakeLine(IOTaskDetInOutFileRow pRow, ref string pLine)
//        {
//            return MakeLine(pRow, ref pLine, false);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pRow"></param>
//        /// <param name="pLine"></param>
//        /// <param name="pIsTitle"></param>
//        /// <returns></returns>
//        /// RD 20100608 [] Refactoring
//        public Cst.ErrLevel MakeLine(IOTaskDetInOutFileRow pRow, ref string pLine, bool pIsTitle)
//        {

//            string rowMsg = "Row [Id= " + pRow.id + "]";
//            //

//            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
//            bool isLastData = false;
//            //
//            #region Charger les parsings pour ce Row
//            if (RowIsToWrite(pRow))
//            {
//                // RD 20100608 [] 
//                if (ArrFunc.IsEmpty(m_DrIOParsing))
//                    throw new Exception("no parsing specified");
//            }
//            else
//            {
//                ProcessLogInfo logInfo = new ProcessLogInfo(ProcessStateTools.StatusUnknownEnum, 0, string.Empty, new string[] { rowMsg + " not writted" });
//                m_Task.process.ProcessLogAddDetail(logInfo);
//                ret = Cst.ErrLevel.DATAIGNORE;
//            }
//            #endregion
//            #region Ecrire dans la ligne selon ses parsings
//            if (ret == Cst.ErrLevel.SUCCESS)
//            {
//                pLine = string.Empty;
//                //
//                for (int parsingNumber = 0; parsingNumber < ParsingCount; parsingNumber++)
//                {
//                    #region Charger ParsingDet pour un parsing
//                    m_IdParsing = GetParsingDet(parsingNumber);
//                    #endregion
//                    //
//                    if (ParsingDetCount == 0)
//                    {
//                        ProcessLogInfo processLogInfo = new ProcessLogInfo(ProcessStateTools.StatusErrorEnum, 0, string.Empty,
//                            new string[] { "(" + rowMsg + ") : no details specified", "Parsing " + m_IdParsing });
//                        m_Task.process.ProcessLogAddDetail(processLogInfo);
//                        ret = Cst.ErrLevel.DATANOTFOUND;
//                    }
//                    //
//                    if (ret == Cst.ErrLevel.SUCCESS)
//                    {
//                        for (int parsingDetNumber = 0; parsingDetNumber < ParsingDetCount; parsingDetNumber++)
//                        {
//                            bool isRowRejected = false;
//                            string errMsg = string.Empty;
//                            GetParsingDetData(parsingDetNumber);
//                            //
//                            string msgDet = rowMsg + ", Name:" + m_DataName + ", IdParsing: " + m_IdParsing;
//                            //
//                            #region Valoriser le champs
//                            try
//                            {
//                                ParseData(pRow, pIsTitle, ref isRowRejected);
//                            }
//                            catch (Exception ex)
//                            {
//                                errMsg = ex.Message;
//                            }
//                            //
//                            if (StrFunc.IsFilled(errMsg))
//                            {
//                                errMsg = "Error on formatting data" + Cst.CrLf + msgDet + Cst.CrLf + " [" + errMsg + "]";
//                                m_Task.process.ProcessLogAddDetail(new ProcessLogInfo(ProcessStateTools.StatusErrorEnum, 0, string.Empty, new string[] { errMsg }));
//                                ret = Cst.ErrLevel.ABORTED;
//                            }
//                            #endregion
//                            //
//                            if (isRowRejected)
//                                return Cst.ErrLevel.DATAREJECTED;
//                            //
//                            if (StrFunc.IsEmpty(errMsg))
//                            {
//                                #region Ecrire le champs sur la ligne
//                                //
//                                try
//                                {
//                                    if ((parsingNumber == ParsingCount - 1) && (parsingDetNumber == ParsingDetCount - 1))
//                                        isLastData = true;
//                                    else
//                                        isLastData = false;
//                                    //
//                                    WriteData(ref pLine, isLastData);
//                                }
//                                catch (Exception ex)
//                                {
//                                    errMsg = ex.Message;
//                                }
//                                //
//                                if (StrFunc.IsFilled(errMsg))
//                                {
//                                    errMsg = "Error on writing data" + Cst.CrLf + msgDet + Cst.CrLf + " [" + errMsg + "]";
//                                    m_Task.process.ProcessLogAddDetail(new ProcessLogInfo(ProcessStateTools.StatusErrorEnum, 0, string.Empty, new string[] { errMsg }));
//                                    ret = Cst.ErrLevel.ABORTED;
//                                }
//                                #endregion
//                            }
//                        }
//                    }
//                }
//            }
//            #endregion
//            //
//            return ret;

//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pRow"></param>
//        /// <param name="pIsTitle"></param>
//        /// <param name="pIsRejected"></param>
//        private void ParseData(IOTaskDetInOutFileRow pRow, bool pIsTitle, ref bool pIsRejected)
//        {

//            if (!pIsTitle)
//                ParseData(pRow, ref pIsRejected);
//            else
//            {
//                if (m_DataLength <= 0)
//                {
//                    ParseData(pRow, ref pIsRejected);
//                    //
//                    if (StrFunc.IsFilled(m_DataValue))
//                        m_DataLength = m_DataValue.Length;
//                }
//                //
//                m_DataValue = m_DataName.ToUpper();

//                // RD 20131125 [19237] En cas de séparateurs, ne pas aligner les en-têtes de colonnes
//                if (m_DataLength > 0 && StrFunc.IsEmpty(m_DataSeparator))
//                    m_DataValue = m_DataValue.PadRight(m_DataLength);

//                //
//                //                m_DataType          = ;
//                //                m_DataStart         = ;
//                //                m_DataSeparator     = ;
//                m_DataCharDelimiter = string.Empty;
//                //                m_DataName          = ;
//                m_DataAlignment = string.Empty;
//                m_DefaultValue = string.Empty;
//                m_DefaultRule = string.Empty;
//                m_DataFormat = string.Empty;
//                m_DataFillChar = string.Empty;
//                m_DataGrpSeparator = string.Empty;
//                m_DataDecSeparator = string.Empty;
//                m_DataRoundDir = string.Empty;
//                m_DataRoundPrec = 0;
//            }

//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pRow"></param>
//        /// <param name="pIsRejected"></param>
//        private void ParseData(IOTaskDetInOutFileRow pRow, ref bool pIsRejected)
//        {
//            try
//            {
//                bool dataFound = false;
//                GetDataValueFromRow(pRow, ref dataFound, ref pIsRejected);
//                //
//                if (false == dataFound)
//                    throw new Exception("Incorrect data name, check parsing parameters");

//                //Appliquer la valeur par défaut 
//                SetDataValueDefaultValue();

//                // Appliquer le format	
//                if (StrFunc.IsFilled(m_DataValue))
//                    SetDataValueFormat();

//                // Appliquer le delimiteur et le remplissage		
//                SetDataValueDelimiter();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error to parse data:" + m_DataName.ToUpper() + " (IdParsing: " + m_IdParsing.ToUpper() + ")", ex);
//            }
//        }

//        /// <summary>
//        /// Vérifier si le Row en cours est à Ecrire ou pas, et charger tous les Parsings correspondant.
//        /// </summary>
//        /// <param name="pRow"></param>
//        /// <returns></returns>
//        /// RD 20100608 [] Bug: Dans le cas du même parsing définit plusieurs fois sur un export avec des valeurs de controls
//        private bool RowIsToWrite(IOTaskDetInOutFileRow pRow)
//        {
//            string idIOParsing = null;
//            string dataValue;
//            string dataName;
//            //
//            DataRow[] drAllParsings = m_DtIOParsing.Select();
//            DataTable dtRowParsings = null;
//            //
//            if (ArrFunc.IsFilled(drAllParsings))
//            {
//                dtRowParsings = m_DtIOParsing.Clone();
//                //
//                foreach (DataRow rowParsing in drAllParsings)
//                {
//                    dataName = (Convert.IsDBNull(rowParsing["DATANAME"]) ? string.Empty : rowParsing["DATANAME"].ToString());
//                    dataValue = (Convert.IsDBNull(rowParsing["DATAVALUE"]) ? string.Empty : rowParsing["DATAVALUE"].ToString());
//                    idIOParsing = (Convert.IsDBNull(rowParsing["IDIOPARSING"]) ? string.Empty : rowParsing["IDIOPARSING"].ToString());
//                    //
//                    if (StrFunc.IsFilled(dataName) && StrFunc.IsFilled(dataValue))
//                    {
//                        GetDataValueFromRow(pRow, dataName);
//                        //
//                        if (m_DataValue == dataValue)
//                        {
//                            if (StrFunc.IsFilled(idIOParsing))
//                                dtRowParsings.ImportRow(rowParsing);
//                            else
//                                return false;
//                        }
//                    }
//                    else
//                    {
//                        if (StrFunc.IsFilled(idIOParsing))
//                            dtRowParsings.ImportRow(rowParsing);
//                        else
//                            return false;
//                    }
//                }
//            }
//            //
//            if (null != dtRowParsings)
//                m_DrIOParsing = dtRowParsings.Select();
//            else
//                m_DrIOParsing = null;
//            //
//            return true;
//        }

//        /// <revision>
//        ///     <version>2.3.0.17</version><author>PL</author><date>20091209</date> 
//        ///     <comment>
//        ///     Gestion du separator sur la dernière donnée
//        ///     </comment>
//        /// </revision>	
//        private void WriteData(ref string pLine, bool pIsLastData)
//        {
//            string messageStart = "DataName: " + m_DataName;
//            string messageValue = string.Empty;
//            int fillLength = 0;
//            string oldSub = string.Empty;
//            //
//            int dataLength = (m_DataLength != 0 ? m_DataLength : (StrFunc.IsFilled(m_DataValue) ? m_DataValue.Length : 0));
//            string dataSeparator = (StrFunc.IsFilled(m_DataSeparator) ? m_DataSeparator : string.Empty);

//            //PL 20091209 Add if()
//            if (pIsLastData)
//            {
//                //WARNING: Ne pas tester StrFunc.IsEmpty() car cette méthode retourne TRUE si la donnée est par exemple "\r\n"
//                if (dataSeparator.Length == 0)
//                {
//                    dataSeparator = @"\x0d\x0a";
//                }
//            }
//            //
//            #region Ecrire la donnée
//            if (m_DataStart > 0)
//            {
//                #region Avec position
//                if (m_DataStart - 1 <= pLine.Length)
//                {

//                    messageValue = "Check your parsing parameters, " + messageStart + " is overlying other data";
//                    //
//                    if (m_DataStart - 1 + dataLength <= pLine.Length)
//                    {
//                        oldSub = pLine.Substring(m_DataStart - 1, dataLength);
//                        pLine = pLine.Substring(0, m_DataStart - 1) + m_DataValue + Regex.Unescape(dataSeparator) + pLine.Substring(m_DataStart - 1 + dataLength);
//                    }
//                    else
//                    {
//                        oldSub = pLine.Substring(m_DataStart - 1);
//                        pLine = pLine.Substring(0, m_DataStart - 1) + m_DataValue;
//                        //PL 20091209 set comment on if()
//                        //if (!pIsLastData)
//                        pLine += Regex.Unescape(dataSeparator);
//                    }
//                    //Error   
//                    if (StrFunc.IsFilled(Regex.Escape(oldSub)))
//                        throw new Exception(messageValue);
//                }
//                else
//                {
//                    fillLength = m_DataStart - pLine.Length - 1;
//                    //
//                    pLine += string.Empty.PadRight(fillLength) + m_DataValue;
//                    //PL 20091209 set comment on if()
//                    //if (!pIsLastData)
//                    pLine += Regex.Unescape(dataSeparator);
//                }
//                #endregion
//            }
//            else if (pIsLastData)
//            {
//                //PL 20091209 set comment on else if (pIsLastData)
//                //pLine += m_DataValue;
//                pLine += m_DataValue + Regex.Unescape(dataSeparator);
//            }
//            else if (StrFunc.IsFilled(dataSeparator))
//            {
//                pLine += m_DataValue + Regex.Unescape(dataSeparator);
//            }
//            else
//            {
//                throw new Exception("No Start position or Separator are specified in parsing parameters");
//            }
//            #endregion Ecrire la donnée

//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pRow"></param>
//        /// <param name="pDataFound"></param>
//        /// <param name="pIsRejected"></param>
//        private void GetDataValueFromRow(IOTaskDetInOutFileRow pRow, ref bool pDataFound, ref bool pIsRejected)
//        {
//            m_DataValue = GetDataValueFromRow(m_Task, pRow, m_DataName, ref pDataFound, ref pIsRejected);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pRow"></param>
//        /// <param name="pDataName"></param>
//        private void GetDataValueFromRow(IOTaskDetInOutFileRow pRow, string pDataName)
//        {
//            bool dataFound = false;
//            m_DataValue = GetDataValueFromRow(m_Task, pRow, pDataName, ref dataFound);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pTask"></param>
//        /// <param name="pRow"></param>
//        /// <param name="pDataName"></param>
//        /// <param name="pDataFound"></param>
//        /// <returns></returns>
//        public static string GetDataValueFromRow(Task pTask, IOTaskDetInOutFileRow pRow, string pDataName, ref bool pDataFound)
//        {
//            bool isRejected = false;
//            return GetDataValueFromRow(pTask, pRow, pDataName, ref pDataFound, ref isRejected);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pTask"></param>
//        /// <param name="pRow"></param>
//        /// <param name="pDataName"></param>
//        /// <param name="pDataFound"></param>
//        /// <param name="pIsRejected"></param>
//        /// <returns></returns>
//        public static string GetDataValueFromRow(Task pTask, IOTaskDetInOutFileRow pRow, string pDataName, ref bool pDataFound, ref bool pIsRejected)
//        {
//            ArrayList messageValue = new ArrayList();
//            messageValue.Add("Row (" + pRow.id + ")");
//            messageValue.Add("Data (" + pDataName + ")");


//            string dataValue = string.Empty;
//            pDataFound = false;
//            //
//            for (int i = 0; i < pRow.data.Length; i++)
//            {
//                if (pRow.data[i].name == pDataName)
//                {
//                    pDataFound = true;
//                    string resultAction = string.Empty;
//                    // RD 20100305 / Utilisation du l'Enum IOCommonTools.SqlAction
//                    ProcessMapping.GetMappedDataValue(pTask, pRow.data[i], null, IOCommonTools.SqlAction.NA, ref dataValue, ref resultAction, messageValue);
//                    dataValue = IOTools.SetDataRowToDynamicString(dataValue, pTask, pRow, false);
//                    #region SpheresLib in XML
//                    //
//                    // 20090116 a décommenter si on veut interpréter des SpheresLib dans un éventuel flux XML
//                    //
//                    //if (TypeData.IsTypeXml(pRow.data[i].datatype) && (null != pRow.data[i].valueXML))
//                    //{
//                    //    dataValue = pRow.data[i].valueXML.Value;
//                    //    dataValue = IOTools.SetDataRowToDynamicString(dataValue, m_Task, pRow, true);
//                    //}
//                    //else
//                    //{
//                    //    ProcessMapping.GetMappedDataValue(m_Task, pRow.data[i], null, ref dataValue, m_IOTrackMessageValue);
//                    //    dataValue = IOTools.SetDataRowToDynamicString(dataValue, m_Task, pRow, true);
//                    //}
//                    ////
//                    //if (dataValue == null)
//                    //    dataValue = string.Empty;
//                    ////
//                    //if (TypeData.IsTypeXml(pRow.data[i].datatype) && (null != pRow.data[i].valueXML))
//                    //    pRow.data[i].valueXML.Value = dataValue;
//                    //else
//                    //    pRow.data[i].value = dataValue;                                
//                    //
//                    #endregion
//                    //
//                    pIsRejected = (resultAction == "REJECTROW");
//                    break;
//                }
//            }
//            //
//            return dataValue;

//        }
//        #endregion
//    }
//}

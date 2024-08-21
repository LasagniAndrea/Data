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
//    /// Représente les parsings d'une importation 
//    /// </summary>
//    public class InputParsing : IOParsing
//    {
//        #region Members
//        // RD 20100111 [16818] MS Excel® file import
//        private string m_DefaultSeparator;
//        private bool m_IsHierarchical;
//        #endregion Members

//        #region Accessors
//        /// <summary>
//        /// 
//        /// </summary>
//        /// RD 20100111 [16818] MS Excel® file import
//        protected override string IOParsingTableName
//        {
//            get { return Cst.OTCml_TBL.IOINPUT_PARSING.ToString(); }
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        public string DefaultSeparator
//        {
//            get { return m_DefaultSeparator; }
//            set { m_DefaultSeparator = value; }
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        public bool IsHierarchical
//        {
//            get { return m_IsHierarchical; }
//        }
//        #endregion

//        #region Constructor
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pIdInput"></param>
//        /// <param name="pTask"></param>
//        public InputParsing(string pIdInput, Task pTask)
//            : base(pIdInput, pTask)
//        {
//            m_IsIn = true;
//            m_IsHierarchical = (m_DtIOParsing.Select("ROWLEVEL is not null").Length > 0);
//        }
//        #endregion Constructor

//        #region methods
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pIdInput"></param>
//        /// <returns></returns>
//        /// RD 20110401 [17379] hierachical row
//        protected override DataSet LoadIOParsing(string pIdInput)
//        {
//            string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf;

//            sqlQuery += SQLCst.SELECT + IOParsingTableName + ".IDIOPARSING, " + IOParsingTableName + ".SEQUENCENO, " + Cst.CrLf;
//            sqlQuery += IOParsingTableName + ".RECORDTYPESTART, " + IOParsingTableName + ".RECORDTYPELENGTH, " + Cst.CrLf;
//            sqlQuery += IOParsingTableName + ".DATASECTION, " + IOParsingTableName + ".DATAKEY, " + Cst.CrLf;
//            sqlQuery += IOParsingTableName + ".DATAVALUE, " + IOParsingTableName + ".ROWLEVEL ";
//            //
//            sqlQuery += GetJoinIOParsingSQLQuery();
//            //
//            sqlQuery += SQLCst.WHERE + IOParsingTableName + ".IDIOINPUT = " + DataHelper.SQLString(pIdInput) + Cst.CrLf;
//            sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(m_Task.Cs, IOParsingTableName) + Cst.CrLf;
//            sqlQuery += SQLCst.ORDERBY + IOParsingTableName + ".SEQUENCENO" + Cst.CrLf;
//            //
//            return DataHelper.ExecuteDataset(m_Task.Cs, CommandType.Text, sqlQuery);
//        }

//        /// <summary>
//        /// Parser la ligne en cours et retourner le résultat dans le tableau IOTaskDetInOutFileRowData[]
//        /// </summary>
//        /// <param name="pLine"></param>
//        /// <param name="pLineNumber"></param>
//        /// <param name="pRowData"></param>
//        /// <param name="pIsLightSerializeMode"></param>
//        /// <returns>false If at least one Data status is Error</returns>
//        /// 20081226 FI [16446] Methode void
//        /// RD 20100408 Optimisation
//        /// RD 20100608 [] Bug: Dans le cas du même parsing définit plusieurs fois sur un import avec des valeurs de controls                
//        /// RD 20110401 [17379] Serialize mode : Add param pIsLightSerializeMode
//        public Boolean ParseLine(string pLine, int pLineNumber, bool pIsLightSerializeMode, out IOTaskDetInOutFileRowData[] pRowData)
//        {
//            Boolean isOk = true;
//            string lineRem = pLine;

//            // RD 20100608
//            if (ArrFunc.IsEmpty(m_DrIOParsing))
//                throw new Exception("no parsing specified");

//            // RD 20100608
//            pRowData = null;
//            ArrayList alRowData = new ArrayList();
//            IOTaskDetInOutFileRowData rowData = null;
//            //
//            for (int parsingNumber = 0; parsingNumber < ParsingCount; parsingNumber++)
//            {
//                //Charger ParsingDet pour un parsing 
//                m_IdParsing = GetParsingDet(parsingNumber);
//                //
//                if (0 == ParsingDetCount)
//                    throw new Exception("Line:" + pLineNumber.ToString() + " no details specified, Parsing " + m_IdParsing);
//                //
//                for (int parsingDetNumber = 0; parsingDetNumber < ParsingDetCount; parsingDetNumber++)
//                {
//                    string errMsg = string.Empty;
//                    ProcessStateTools.StatusEnum parseStatusEnum = ProcessStateTools.StatusSuccessEnum;
//                    //
//                    //Charger ParsingDet d'un champs
//                    GetParsingDetData(parsingDetNumber);
//                    //
//                    #region Parse
//                    try
//                    {
//                        ParseData(pLine, pLineNumber, ref lineRem);
//                    }
//                    catch (Exception ex)
//                    {
//                        string msg = "<b>Error on parsing" + Cst.CrLf;
//                        msg += "Data name: '" + m_DataName + "', Line number: " + pLineNumber.ToString() + ", IdParsing: '" + m_IdParsing + "'</b>";
//                        msg = StrFunc.AppendFormat(msg, m_DataName, pLineNumber.ToString(), m_IdParsing);

//                        SpheresException exLog = SpheresExceptionParser.GetSpheresException(msg, ex);
//                        m_Task.process.ProcessLogAddDetail(exLog);

//                        parseStatusEnum = ProcessStateTools.StatusErrorEnum;
//                    }
//                    #endregion Parse


//                    #region Créer la ligne Data dans XML post parsing
//                    // RD 20100608 [] 
//                    if ((false == m_IsOptional) || StrFunc.IsFilled(m_DataValue))
//                    {
//                        rowData = new IOTaskDetInOutFileRowData();
//                        rowData.name = m_DataName;
//                        rowData.datatype = m_DataType;
//                        // RD 20110401 [17379] Serialize mode : Pour le mode Light ne pas sérialiser le DataType s'il est = string
//                        rowData.datatypeSpecified = IOTools.XMLIsDataTypeSpecified(rowData.datatype, pIsLightSerializeMode);
//                        //
//                        // RD 20100723 [17097] Input: XML Column 
//                        // Mettre le flux de Type XML dans un "valueXML" comme étant CDATA, à l'image de l'export d'un flux XML
//                        if (StrFunc.IsFilled(m_DataValue) && TypeData.IsTypeXml(rowData.datatype))
//                            rowData.valueXML = IOTools.SetInCDATA(m_DataValue);
//                        else
//                            rowData.value = m_DataValue;
//                        //
//                        // RD 20110401 [17379] Serialize mode : Pour le mode Light ne pas sérialiser le Status s'il est = success
//                        IOTools.XMLSetStatus(ref rowData.status, ref rowData.statusSpecified, ProcessStateTools.IsStatusError(parseStatusEnum), pIsLightSerializeMode);
//                        //
//                        alRowData.Add(rowData);
//                    }
//                    #endregion
//                    //
//                    // RD 20100408 [] Optimisation
//                    if (false == ProcessStateTools.IsStatusSuccess(parseStatusEnum))
//                        isOk = false;
//                }
//            }
//            //
//            // RD 20100608 []
//            if (alRowData.Count > 0)
//                pRowData = (IOTaskDetInOutFileRowData[])alRowData.ToArray(typeof(IOTaskDetInOutFileRowData));

//            return isOk;
//        }

//        /// <summary>
//        /// Renvoi le niveau du Row définit sur l'association du Parsing, dans le cas d'une structure hiérarchique du flux à importer. 
//        /// </summary>
//        /// <returns></returns>
//        public int GetRowLevelParsing()
//        {
//            int ret = -1;
//            //
//            if (ParsingCount > 0)
//                ret = (Convert.IsDBNull(m_DrIOParsing[0]["ROWLEVEL"]) ? -1 : Convert.ToInt32(m_DrIOParsing[0]["ROWLEVEL"]));
//            //
//            return ret;
//        }

//        /// <summary>
//        /// Vérifier si la ligne en cours est à Parser ou pas, et charger tous les Parsings correspondant à la ligne en cours.
//        /// </summary>
//        /// <param name="pLine"></param>
//        /// <returns></returns>
//        /// 20081226 FI [16446] Methode public
//        /// RD 20100608 [] Bug: Dans le cas du même parsing définit plusieurs fois sur un import avec des valeurs de controls
//        public bool IsLineToParse(string pLine)
//        {

//            //
//            DataRow[] drAllParsings = m_DtIOParsing.Select();
//            DataTable dtLineParsings = null;
//            //
//            if (ArrFunc.IsFilled(drAllParsings))
//            {
//                dtLineParsings = m_DtIOParsing.Clone();
//                //
//                foreach (DataRow rowParsing in drAllParsings)
//                {
//                    int recordTypeStart = (Convert.IsDBNull(rowParsing["RECORDTYPESTART"]) ? 0 : Convert.ToInt32(rowParsing["RECORDTYPESTART"]));
//                    int recordTypeLength = (Convert.IsDBNull(rowParsing["RECORDTYPELENGTH"]) ? 0 : Convert.ToInt32(rowParsing["RECORDTYPELENGTH"]));
//                    string dataValue = (Convert.IsDBNull(rowParsing["DATAVALUE"]) ? null : rowParsing["DATAVALUE"].ToString());
//                    string idIOParsing = (Convert.IsDBNull(rowParsing["IDIOPARSING"]) ? null : rowParsing["IDIOPARSING"].ToString());
//                    //
//                    if (recordTypeStart > 0 && recordTypeLength > 0 && StrFunc.IsFilled(dataValue))
//                    {
//                        // RD 20100202 / Bug signalé par MF / Le Début de la valeur de control dépasse la longueur de la ligne
//                        recordTypeStart--;
//                        //
//                        if (StrFunc.IsFilled(pLine) && (pLine.Length >= (recordTypeStart + recordTypeLength)))
//                        {
//                            // RD 20100421 [16955]
//                            string dataToControl = pLine.Substring(recordTypeStart, recordTypeLength);
//                            string returnData = string.Empty;
//                            //
//                            dataValue = IOTools.CheckDynamicString(dataValue, m_Task, true, false, dataToControl, out returnData);
//                            //
//                            if ((StrFunc.IsFilled(returnData) && (0 == string.Compare(returnData, dataValue, true))) ||
//                                (0 == string.Compare(dataToControl, dataValue, true)))
//                            {
//                                if (StrFunc.IsFilled(idIOParsing))
//                                    dtLineParsings.ImportRow(rowParsing);
//                                else
//                                    return false;
//                            }
//                        }
//                    }
//                    else if (StrFunc.IsFilled(idIOParsing))
//                        dtLineParsings.ImportRow(rowParsing);
//                    else
//                        return false;
//                }
//            }
//            //
//            if (null != dtLineParsings)
//                m_DrIOParsing = dtLineParsings.Select();
//            else
//                m_DrIOParsing = null;
//            //
//            return true;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pLine"></param>
//        /// <param name="pLineNumber"></param>
//        /// <param name="pLineRem"></param>
//        private void ParseData(string pLine, int pLineNumber, ref string pLineRem)
//        {
//            GetDataValueFromLine(pLine, pLineNumber, ref pLineRem);

//            // PM 20130424 : Déplacement de SetDataValueDelimiter() dans GetDataValueFromLine()
//            // car dans GetDataValueFromLine() il y a une vérification du format de la données qui ne peut être réalisé correctement tant qu'il y a les delimiters
//            ////Enlever le délimiteur 
//            //SetDataValueDelimiter();

//            //Appliquer la valeur par défaut 
//            SetDataValueDefaultValue(pLineNumber);

//            //Appliquer le Format 
//            // 20090601 RD Si la velaur est vide, pas la peine d'appliquer un format
//            if (StrFunc.IsFilled(m_DataValue))
//                SetDataValueFormat();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pLine"></param>
//        /// <param name="pLineNumber"></param>
//        /// <param name="pLineRem"></param>
//        private void GetDataValueFromLine(string pLine, int pLineNumber, ref string pLineRem)
//        {
//            m_DataValue = string.Empty;
//            //
//            string separator;
//            int separatorPosition;
//            int separatorLength;
//            //
//            // RD 20100111 [16818] MS Excel® file import
//            separator = m_DataSeparator;
//            if (StrFunc.IsFilled(m_DefaultSeparator))
//                separator = m_DefaultSeparator;
//            //
//            if (StrFunc.IsFilled(separator))
//            {
//                #region With Separator
//                if ((0 < m_DataStart) && (m_DataStart - 1 <= pLine.Length))
//                    pLineRem = pLine.Substring(m_DataStart - 1);

//                // RD 20100608 [] Bug: dernière donnée avec séparateur
//                if (null != pLineRem && pLineRem.Length > 0)
//                {
//                    separator = Regex.Unescape(separator);
//                    separatorLength = separator.Length;
//                    //
//                    separatorPosition = pLineRem.IndexOf(separator);
//                    bool isLastData = (separatorPosition == -1);
//                    //
//                    if (isLastData)
//                    {
//                        m_DataValue = pLineRem.Substring(0);
//                        pLineRem = string.Empty;
//                    }
//                    else
//                    {
//                        m_DataValue = pLineRem.Substring(0, separatorPosition);
//                        pLineRem = pLineRem.Substring(separatorPosition + separatorLength);
//                        //----------------------------------------------------------
//                        //PL 20110309 / TRIM 17346
//                        //----------------------------------------------------------
//                        #region Parsing avec Delimiteur ET Donnée contenant le Separateur
//                        //Exemple:. Delimiteur: "   Separateur: ,   Donnée: "10,000.00"
//                        if (StrFunc.IsFilled(m_DataCharDelimiter))
//                        {
//                            string delimiter = Regex.Unescape(m_DataCharDelimiter);
//                            //
//                            if (StrFunc.IsFilled(delimiter) && StrFunc.IsFilled(m_DataValue)
//                                && m_DataValue.StartsWith(delimiter) && !m_DataValue.EndsWith(delimiter))
//                            {
//                                int guard = 0;
//                                while (pLineRem.Length > 0)
//                                {
//                                    guard++;
//                                    if (guard == 999)
//                                        throw new Exception("Infinite loop detected");
//                                    //
//                                    m_DataValue += separator;

//                                    separatorPosition = pLineRem.IndexOf(separator);
//                                    isLastData = (separatorPosition == -1);

//                                    if (isLastData)
//                                    {
//                                        m_DataValue += pLineRem.Substring(0);
//                                        pLineRem = string.Empty;
//                                        break;
//                                    }
//                                    else
//                                    {
//                                        m_DataValue += pLineRem.Substring(0, separatorPosition);
//                                        pLineRem = pLineRem.Substring(separatorPosition + separatorLength);

//                                        if (m_DataValue.EndsWith(delimiter))
//                                            break;
//                                    }
//                                }//end while
//                            }
//                        }
//                        #endregion
//                        //----------------------------------------------------------
//                    }
//                }
//                else if (false == m_IsOptional)
//                {
//                    throw new Exception("Mandatory data not Found");
//                }
//                #endregion With Separator
//            }
//            else
//            {
//                #region Without Separator
//                // FI 20131024 [17861] pour gérer un cas particulier
//                // le séparateur de la 1er donnée est ";" 
//                // La 2nd donnée contient le reste de la ligne et contient des ";"
//                // Voir parsing EUREX - RISKDATA - TheoInst OI-serie - SCENARIOS PRICES
//                if ((m_DataStart == 0) &&  StrFunc.IsFilled(pLineRem)) 
//                {
//                    m_DataValue = pLineRem; 
//                }
//                // PM 20131022 [19085] Correction du bug de donnée en fin de ligne mais dont la longueur est inférieur à la longueur maximum de la donnée
//                //if (m_DataStart - 1 + m_DataLength <= pLine.Length)
//                //m_DataValue = pLine.Substring(m_DataStart - 1, m_DataLength).Trim();
//                else if (m_DataStart <= pLine.Length)
//                {
//                    int len = System.Math.Min(m_DataLength, pLine.Length - m_DataStart + 1);
//                    m_DataValue = pLine.Substring(m_DataStart - 1, len).Trim();
//                }
//                else if (false == m_IsOptional)
//                {
//                    throw new Exception("Mandatory data not Found");
//                }
//                #endregion Without Separator
//            }
//            // PM 20130424 Déplacé à partir de ParseData()
//            //Enlever le délimiteur 
//            SetDataValueDelimiter();
//            //
//            if ((false == m_IsOptional) || StrFunc.IsFilled(m_DataValue))
//            {
//                // RD 20100111 [16818] MS Excel® file import
//                TypeData.TypeDataEnum dataType = TypeData.TypeDataEnum.@string;

//                if (StrFunc.IsFilled(m_DataType))
//                {
//                    dataType = TypeData.GetTypeDataEnum(m_DataType);
//                }
//                //PL 20150610 Add test on IsNullOrEmpty() and refactoring double try/catch
//                //if (TypeData.IsTypeDate(dataType) && (false == StrFunc.IsDate(m_DataValue, m_DataFormat)))
//                if (TypeData.IsTypeDate(dataType)
//                    && (!String.IsNullOrEmpty(m_DataValue)) 
//                    && (!StrFunc.IsDate(m_DataValue, m_DataFormat)))
//                {
//                    try
//                    {
//                        m_DataValue = DtFunc.DateTimeToString(DateTime.FromOADate(double.Parse(m_DataValue)), m_DataFormat);
//                    }
//                    catch { }
//                }
//            }
//        }
//        #endregion
//    }
//}

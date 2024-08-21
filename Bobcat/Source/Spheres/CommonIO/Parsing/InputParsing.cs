using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.IO.Interface;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace EFS.Common.IO
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20230526 [XXXXX] Add
    public class IOParsingException : Exception
    {
        public IOParsingException(string message) : base(message)
        {

        }
        public IOParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    
    /// <summary>
    /// Représente les parsings d'une importation 
    /// </summary>
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO (InputParsing.cs)
    public class InputParsing : IOParsing
    {
        #region Members
        // RD 20100111 [16818] MS Excel® file import
        private string m_DefaultSeparator;
        private readonly bool m_IsHierarchical;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        /// RD 20100111 [16818] MS Excel® file import
        protected override string IOParsingTableName
        {
            get { return Cst.OTCml_TBL.IOINPUT_PARSING.ToString(); }
        }
        /// <summary>
        /// 
        /// </summary>
        public string DefaultSeparator
        {
            get { return m_DefaultSeparator; }
            set { m_DefaultSeparator = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsHierarchical
        {
            get { return m_IsHierarchical; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdInput"></param>
        /// <param name="pTask"></param>
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        //public InputParsing(string pIdInput, Task pTask) : base(pIdInput, pTask)
        public InputParsing(string pIdInput, IIOTaskLaunching pTask) : base(pIdInput, pTask)
        {
            m_IsIn = true;
            m_IsHierarchical = (m_DtIOParsing.Select("ROWLEVEL is not null").Length > 0);
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdInput"></param>
        /// <returns></returns>
        /// RD 20110401 [17379] hierachical row
        /// FI 20210105 [25634] Add RECORDTYPEPOSITION, RECORDTYPEDATASEPARATOR, RECORDTYPECHARDELIMITER
        protected override DataSet LoadIOParsing(string pIdInput)
        {
            string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf;
            //
            sqlQuery += SQLCst.SELECT + IOParsingTableName + ".IDIOPARSING, " + IOParsingTableName + ".SEQUENCENO, " + Cst.CrLf;
            sqlQuery += IOParsingTableName + ".RECORDTYPESTART, " + IOParsingTableName + ".RECORDTYPELENGTH, " + Cst.CrLf;
            sqlQuery += IOParsingTableName + ".RECORDTYPEPOSITION, " + IOParsingTableName + ".RECORDTYPEDATASEPARATOR, " + IOParsingTableName + ".RECORDTYPECHARDELIMITER, " + Cst.CrLf;
            sqlQuery += IOParsingTableName + ".DATASECTION, " + IOParsingTableName + ".DATAKEY, " + Cst.CrLf;
            sqlQuery += IOParsingTableName + ".DATAVALUE, " + IOParsingTableName + ".ROWLEVEL ";
            //
            sqlQuery += GetJoinIOParsingSQLQuery();
            //
            sqlQuery += SQLCst.WHERE + IOParsingTableName + ".IDIOINPUT = " + DataHelper.SQLString(pIdInput) + Cst.CrLf;
            sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(m_Task.Cs, IOParsingTableName) + Cst.CrLf;
            sqlQuery += SQLCst.ORDERBY + IOParsingTableName + ".SEQUENCENO" + Cst.CrLf;
            //
            return DataHelper.ExecuteDataset(m_Task.Cs, CommandType.Text, sqlQuery);
        }

        /// <summary>
        /// Parser la ligne en cours et retourner le résultat dans le tableau IOTaskDetInOutFileRowData[]
        /// </summary>
        /// <param name="pLine"></param>
        /// <param name="pLineNumber"></param>
        /// <param name="pRowData"></param>
        /// <param name="pIsLightSerializeMode"></param>
        /// <returns>false If at least one Data status is Error</returns>
        /// 20081226 FI [16446] Methode void
        /// RD 20100408 Optimisation
        /// RD 20100608 [] Bug: Dans le cas du même parsing définit plusieurs fois sur un import avec des valeurs de controls                
        /// RD 20110401 [17379] Serialize mode : Add param pIsLightSerializeMode
        /// FI 20210105 [25634] light refactoring
        public Boolean ParseLine(string pLine, int pLineNumber, bool pIsLightSerializeMode, out IOTaskDetInOutFileRowData[] pRowData)
        {
            // RD 20100608
            if (ArrFunc.IsEmpty(m_DrIOParsing))
                throw new Exception("no parsing specified");

            Boolean isOk = true;
            string lineRem = pLine;

            List<IOTaskDetInOutFileRowData> alRowData = new List<IOTaskDetInOutFileRowData>();
            for (int parsingNumber = 0; parsingNumber < ParsingCount; parsingNumber++)
            {
                //Charger ParsingDet pour un parsing 
                m_IdParsing = GetParsingDet(parsingNumber);

                if (0 == ParsingDetCount)
                    throw new Exception("Line:" + pLineNumber.ToString() + " no details specified, Parsing " + m_IdParsing);

                for (int parsingDetNumber = 0; parsingDetNumber < ParsingDetCount; parsingDetNumber++)
                {
                    ProcessStateTools.StatusEnum parseStatusEnum = ProcessStateTools.StatusSuccessEnum;

                    //Charger ParsingDet d'un champs
                    GetParsingDetItem(parsingDetNumber);

                    #region Parse
                    try
                    {
                        ParseData(pLine, pLineNumber, ref lineRem);
                    }
                    catch (Exception ex)
                    {
                        string msg = "<b>Error on parsing" + Cst.CrLf;
                        msg += "Data name: '" + m_DataName + "', Line number: " + pLineNumber.ToString() + ", IdParsing: '" + m_IdParsing + "'</b>";
                        msg = StrFunc.AppendFormat(msg, m_DataName, pLineNumber.ToString(), m_IdParsing);
                        // FI 20230526 [XXXXX] si IOParsingException sans innerEception => affichage d'un message plus simple (sans stack etc..)
                        if (ex.GetType().Equals(typeof(IOParsingException)) && (null == ex.InnerException))
                        {
                            Logger.Log(new LoggerData(LogLevelEnum.Error, msg + Cst.CrLf + ex.Message, pRankOrder: 3));
                        }
                        else
                        {
                            SpheresException2 exLog = SpheresExceptionParser.GetSpheresException(msg, ex);
                            
                            Logger.Log(new LoggerData(exLog));
                        }

                        parseStatusEnum = ProcessStateTools.StatusErrorEnum;
                    }
                    #endregion Parse


                    #region Créer la ligne Data dans XML post parsing
                    // RD 20100608 [] 
                    if ((false == m_IsOptional) || StrFunc.IsFilled(m_DataValue))
                    {
                        IOTaskDetInOutFileRowData rowData = new IOTaskDetInOutFileRowData
                        {
                            name = m_DataName,
                            datatype = m_DataType
                        };
                        // RD 20110401 [17379] Serialize mode : Pour le mode Light ne pas sérialiser le DataType s'il est = string
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        rowData.datatypeSpecified = IOCommonTools.XMLIsDataTypeSpecified(rowData.datatype, pIsLightSerializeMode);

                        // RD 20100723 [17097] Input: XML Column 
                        // Mettre le flux de Type XML dans un "valueXML" comme étant CDATA, à l'image de l'export d'un flux XML
                        if (StrFunc.IsFilled(m_DataValue) && TypeData.IsTypeXml(rowData.datatype))
                        {
                            // PM 20180219 [23824] IOTools => IOCommonTools
                            rowData.valueXML = IOCommonTools.SetInCDATA(m_DataValue);
                        }
                        else
                        {
                            rowData.value = m_DataValue;
                        }

                        // RD 20110401 [17379] Serialize mode : Pour le mode Light ne pas sérialiser le Status s'il est = success
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        IOCommonTools.XMLSetStatus(ref rowData.status, ref rowData.statusSpecified, ProcessStateTools.IsStatusError(parseStatusEnum), pIsLightSerializeMode);

                        alRowData.Add(rowData);
                    }
                    #endregion
                    //
                    // RD 20100408 [] Optimisation
                    if (false == ProcessStateTools.IsStatusSuccess(parseStatusEnum))
                        isOk = false;
                }
            }

            // RD 20100608
            pRowData = null;
            if (alRowData.Count > 0)
                pRowData = alRowData.ToArray();

            return isOk;
        }

        /// <summary>
        /// Renvoi le niveau du Row définit sur l'association du Parsing, dans le cas d'une structure hiérarchique du flux à importer. 
        /// </summary>
        /// <returns></returns>
        public int GetRowLevelParsing()
        {
            int ret = -1;
            //
            if (ParsingCount > 0)
            {
                ret = (Convert.IsDBNull(m_DrIOParsing[0]["ROWLEVEL"]) ? -1 : Convert.ToInt32(m_DrIOParsing[0]["ROWLEVEL"]));
            }
            //
            return ret;
        }

        /// <summary>
        /// Vérifier si la ligne en cours est à Parser ou pas, et charger tous les Parsings correspondant à la ligne en cours.
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        /// 20081226 FI [16446] Methode public
        /// RD 20100608 [] Bug: Dans le cas du même parsing définit plusieurs fois sur un import avec des valeurs de controls
        /// FI 20210105 [25634] Refactoring for Delimited Text File (Fichier avec sépateur (like csv))
        public bool IsLineToParse(string pLine)
        {
            DataRow[] drAllParsings = m_DtIOParsing.Select();

            DataTable dtLineParsings = null;
            if (ArrFunc.IsFilled(drAllParsings))
            {
                dtLineParsings = m_DtIOParsing.Clone();

                foreach (DataRow rowParsing in drAllParsings)
                {
                    // Fichier plats sans séparateur
                    int recordTypeStart = (Convert.IsDBNull(rowParsing["RECORDTYPESTART"]) ? 0 : Convert.ToInt32(rowParsing["RECORDTYPESTART"]));
                    int recordTypeLength = (Convert.IsDBNull(rowParsing["RECORDTYPELENGTH"]) ? 0 : Convert.ToInt32(rowParsing["RECORDTYPELENGTH"]));

                    // FI 20210105[25634] new 
                    // Fichier plats avec séparateur 
                    int recordTypePosition = (Convert.IsDBNull(rowParsing["RECORDTYPEPOSITION"]) ? 0 : Convert.ToInt32(rowParsing["RECORDTYPEPOSITION"]));
                    string recordTypeSeparator = (Convert.IsDBNull(rowParsing["RECORDTYPEDATASEPARATOR"]) ? null : rowParsing["RECORDTYPEDATASEPARATOR"].ToString().Trim());
                    string recordTypeCharDelimiter = (Convert.IsDBNull(rowParsing["RECORDTYPECHARDELIMITER"]) ? null : rowParsing["RECORDTYPECHARDELIMITER"].ToString().Trim());

                    //valeur de contrôle
                    string dataValue = (Convert.IsDBNull(rowParsing["DATAVALUE"]) ? null : rowParsing["DATAVALUE"].ToString());

                    //Parsing courant
                    string idIOParsing = (Convert.IsDBNull(rowParsing["IDIOPARSING"]) ? null : rowParsing["IDIOPARSING"].ToString());

                    if (StrFunc.IsFilled(dataValue))
                    {
                        // Il existe une donnée de contrôle
                        // si la donnée de contrôle est vérifiée
                        //      si le parsing courant est renseigné 
                        //          => il sera utilisé 
                        //      sinon (parsing non renseigné)
                        //           => l'enregistrement est ignoré
                        //
                        // si la donnée de contrôle est non vérifiée
                        //      si le parsing courant est renseigné 
                        //          => il ne sera pas utilisé 
                        //      sinon (parsing non renseigné)
                        //           => sans effet

                        // L'enregistrement est ignorié
                        string dataToControl = string.Empty;
                        Boolean isReadDataToControl = false;

                        if ((recordTypeStart > 0) && (recordTypeLength > 0))
                        {
                            //cas des fichiers sans separateur
                            // RD 20100202 / Bug signalé par MF / Le Début de la valeur de control dépasse la longueur de la ligne
                            recordTypeStart--;
                            isReadDataToControl = StrFunc.IsFilled(pLine) && (pLine.Length >= (recordTypeStart + recordTypeLength));
                            if (isReadDataToControl)
                            {
                                // RD 20100421 [16955]
                                dataToControl = pLine.Substring(recordTypeStart, recordTypeLength);
                            }
                        }
                        else if ((recordTypePosition > 0) && (StrFunc.IsFilled(recordTypeSeparator)))
                        {
                            isReadDataToControl = StrFunc.IsFilled(pLine);
                            if (isReadDataToControl)
                            {
                                string lineRem = pLine;
                                for (int i = 0; i < recordTypePosition; i++)
                                {
                                    //cas des fichiers avec separateur
                                    dataToControl = ExtractFirstData(ref lineRem, recordTypeSeparator, recordTypeCharDelimiter);
                                    dataToControl = RemoveDelimiter(dataToControl, recordTypeCharDelimiter);
                                }
                            }
                        }

                        if (isReadDataToControl)
                        {
                            // PM 20180219 [23824] IOTools => IOCommonTools
                            dataValue = IOCommonTools.CheckDynamicString(dataValue, m_Task, false, dataToControl, out string returnData);

                            if ((StrFunc.IsFilled(returnData) && (0 == string.Compare(returnData, dataValue, true))) ||
                                (0 == string.Compare(dataToControl, dataValue, true)))
                            {
                                if (StrFunc.IsFilled(idIOParsing))
                                {
                                    dtLineParsings.ImportRow(rowParsing);
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else if (StrFunc.IsFilled(idIOParsing))
                    {
                        dtLineParsings.ImportRow(rowParsing);
                    }
                    else 
                    {
                        // Il existe un paramétrage sans parsing spécifié ni donnée de contrôle
                        // => l'enregistrement est ignoré
                        return false;
                    }
                }
            }

            if (null != dtLineParsings)
            {
                m_DrIOParsing = dtLineParsings.Select();
            }
            else
            {
                m_DrIOParsing = null;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLine"></param>
        /// <param name="pLineNumber"></param>
        /// <param name="pLineRem"></param>
        private void ParseData(string pLine, int pLineNumber, ref string pLineRem)
        {
            GetDataValueFromLine(pLine, ref pLineRem);

            // PM 20130424 : Déplacement de SetDataValueDelimiter() dans GetDataValueFromLine()
            // car dans GetDataValueFromLine() il y a une vérification du format de la données qui ne peut être réalisé correctement tant qu'il y a les delimiters
            ////Enlever le délimiteur 
            //SetDataValueDelimiter();

            //Appliquer la valeur par défaut 
            SetDataValueDefaultValue(pLineNumber);

            //Appliquer le Format 
            // 20090601 RD Si la velaur est vide, pas la peine d'appliquer un format
            if (StrFunc.IsFilled(m_DataValue))
            {
                SetDataValueFormat();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLine"></param>
        /// <param name="pLineRem"></param>
        private void GetDataValueFromLine(string pLine, ref string pLineRem)
        {
            m_DataValue = string.Empty;

            // RD 20100111 [16818] MS Excel® file import
            string separator = m_DataSeparator;
            if (StrFunc.IsFilled(m_DefaultSeparator))
                separator = m_DefaultSeparator;

            if (StrFunc.IsFilled(separator))
            {
                #region With Separator
                if ((0 < m_DataStart) && (m_DataStart - 1 <= pLine.Length))
                {
                    pLineRem = pLine.Substring(m_DataStart - 1);
                }
                if (null != pLineRem && pLineRem.Length > 0)
                {
                    // FI 20210105 [25634] call ExtractFirstData
                    m_DataValue = ExtractFirstData(ref pLineRem, separator, m_DataCharDelimiter);
                }
                else if (false == m_IsOptional)
                {
                    // FI 20230526 [XXXXX] throw a IOParsingException
                    throw new IOParsingException("Mandatory data not Found");
                }
                #endregion With Separator
            }
            else
            {
                #region Without Separator
                // FI 20131024 [17861] pour gérer un cas particulier
                // le séparateur de la 1er donnée est ";" 
                // La 2nd donnée contient le reste de la ligne et contient des ";"
                // Voir parsing EUREX - RISKDATA - TheoInst OI-serie - SCENARIOS PRICES
                if ((m_DataStart == 0) && StrFunc.IsFilled(pLineRem))
                {
                    m_DataValue = pLineRem;
                }
                // PM 20131022 [19085] Correction du bug de donnée en fin de ligne mais dont la longueur est inférieur à la longueur maximum de la donnée
                //if (m_DataStart - 1 + m_DataLength <= pLine.Length)
                //m_DataValue = pLine.Substring(m_DataStart - 1, m_DataLength).Trim();
                else if (m_DataStart <= pLine.Length)
                {
                    int len = System.Math.Min(m_DataLength, pLine.Length - m_DataStart + 1);
                    m_DataValue = pLine.Substring(m_DataStart - 1, len).Trim();
                }
                else if (false == m_IsOptional)
                {
                    throw new IOParsingException("Mandatory data not Found");
                }
                #endregion Without Separator
            }
            // PM 20130424 Déplacé à partir de ParseData()
            //Enlever le délimiteur 
            SetDataValueDelimiter();
            //
            if ((false == m_IsOptional) || StrFunc.IsFilled(m_DataValue))
            {
                // RD 20100111 [16818] MS Excel® file import
                TypeData.TypeDataEnum dataType = TypeData.TypeDataEnum.@string;

                if (StrFunc.IsFilled(m_DataType))
                {
                    dataType = TypeData.GetTypeDataEnum(m_DataType);
                }
                //PL 20150610 Add test on IsNullOrEmpty() and refactoring double try/catch
                //if (TypeData.IsTypeDate(dataType) && (false == StrFunc.IsDate(m_DataValue, m_DataFormat)))
                if (TypeData.IsTypeDate(dataType)
                    && (!String.IsNullOrEmpty(m_DataValue))
                    && (!StrFunc.IsDate(m_DataValue, m_DataFormat)))
                {
                    try
                    {
                        m_DataValue = DtFunc.DateTimeToString(DateTime.FromOADate(double.Parse(m_DataValue)), m_DataFormat);
                    }
                    catch { }
                }
            }
        }
        #endregion

        /// <summary>
        /// Retourne la donnée présente dans {pLineRem} avant le 1er {pSeparator}
        /// <para>La donnée obtenue est supprimée de {pLineRem} (permet des appels récursifs)</para>
        /// <para>Exemple : "toto;titi";"toto2";titi2 => Retourne  "toto;titi"</para>
        /// <para>Exemple : "toto2";titi2 => Retourne  "toto2"</para>
        /// <para>Exemple : "titi2" => Retourne  "titi2"</para>
        /// </summary>
        /// <param name="pLineRem"></param>
        /// <param name="pSeparator"></param>
        /// <param name="pCharDelimiter"></param>
        /// <returns></returns>
        /// FI 20210105 [25634] Add Method
        private static string ExtractFirstData(ref string pLineRem, string pSeparator, string pCharDelimiter)
        {
            pSeparator = Regex.Unescape(pSeparator);
            int separatorLength = pSeparator.Length;

            int separatorPosition = pLineRem.IndexOf(pSeparator);
            bool isLastData = (separatorPosition == -1);

            string ret;
            if (isLastData)
            {
                ret = pLineRem.Substring(0);
                pLineRem = string.Empty;
            }
            else
            {
                ret = pLineRem.Substring(0, separatorPosition);
                pLineRem = pLineRem.Substring(separatorPosition + separatorLength);
                //----------------------------------------------------------
                //PL 20110309 / TRIM 17346
                //----------------------------------------------------------
                #region Parsing avec Delimiteur ET Donnée contenant le Separateur
                //Exemple:. Delimiteur: "   Separateur: ,   Donnée: "10,000.00"
                if (StrFunc.IsFilled(pCharDelimiter))
                {
                    string delimiter = Regex.Unescape(pCharDelimiter);
                    //
                    if (StrFunc.IsFilled(delimiter) && StrFunc.IsFilled(ret)
                        && ret.StartsWith(delimiter) && !ret.EndsWith(delimiter))
                    {
                        int guard = 0;
                        while (pLineRem.Length > 0)
                        {
                            guard++;
                            if (guard == 999)
                            {
                                throw new Exception("Infinite loop detected");
                            }

                            ret += pSeparator;

                            separatorPosition = pLineRem.IndexOf(pSeparator);
                            isLastData = (separatorPosition == -1);

                            if (isLastData)
                            {
                                ret += pLineRem.Substring(0);
                                pLineRem = string.Empty;
                                break;
                            }
                            else
                            {
                                ret += pLineRem.Substring(0, separatorPosition);
                                pLineRem = pLineRem.Substring(separatorPosition + separatorLength);

                                if (ret.EndsWith(delimiter))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            return ret;
        }
    
    }
}

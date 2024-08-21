using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.IO.Interface;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;

namespace EFS.Common.IO
{
    public static class IOCommonTools
    {
        #region Enum
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public enum SqlAction
        {
            S,
            /// <summary>
            /// Insert
            /// </summary>
            I,
            /// <summary>
            /// Update
            /// </summary>
            U,
            /// <summary>
            /// Delete
            /// </summary>
            D,
            /// <summary>
            /// Insert, Update
            /// </summary>
            IU,
            /// <summary>
            /// Aucune action
            /// </summary>
            NA,
        }
        #endregion Enum

        #region Class
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static class ParametersTools
        {
            #region Members
            //
            // PM 20180219 [23824] Déplacé à partir de IOTools
            public static string Parameters = "parameters.";
            //
            #endregion Members

            #region Methods
            #region GetParameterName
            public static string GetParameterName(string pValue)
            {
                return (pValue.Substring(pValue.IndexOf(Parameters) + Parameters.Length));
            }
            #endregion
            #region GetColumnParamValue
            public static string GetValueFromParameters(IOTaskDetInOutFileRow pRow, IOTaskDetInOutFileRowTableColumn pColumn, string pValueWithParam)
            {
                string ret = pValueWithParam;
                //
                if (StrFunc.IsFilled(pValueWithParam) && pValueWithParam.Trim().StartsWith(Parameters))
                {
                    string paramName = GetParameterName(pValueWithParam.Trim());
                    //
                    // le Param sur la colonne est plus prioritaire
                    if ((null != pColumn) && pColumn.ExistColumnParam(paramName))
                        ret = pColumn.GetColumnParamValue(paramName);
                    else if ((null != pRow) && pRow.ExistRowParam(paramName))
                        ret = pRow.GetRowParamValue(paramName);
                    else
                        throw new Exception("Used parameter is not exist: <b>" + pValueWithParam.Trim().TrimStart(Parameters.ToCharArray()) + "</b>");
                }
                //
                return ret;
            }
            #endregion

            #region ValuateDataWithParameters
            public static void ValuateDataWithParameters(IOTaskDetInOutFileRow pRow, IOTaskDetInOutFileRowTableColumn pColumn, XmlDocument pXmlDocumentData)
            {
                ValuateDataWithParameters(pRow, pColumn, pXmlDocumentData.ChildNodes);
            }
            public static void ValuateDataWithParameters(IOTaskDetInOutFileRow pRow, IOTaskDetInOutFileRowTableColumn pColumn, XmlNodeList pNodeList)
            {
                foreach (XmlNode node in pNodeList)
                {
                    // Valoriser le contenu de l'élément
                    if ((node.NodeType == XmlNodeType.Text))
                        node.Value = GetValueFromParameters(pRow, pColumn, node.Value);
                    else
                    {
                        if (node.NodeType != XmlNodeType.ProcessingInstruction && node.NodeType != XmlNodeType.XmlDeclaration)
                        {
                            // Valoriser les attributs de l'élément
                            foreach (XmlAttribute attribute in node.Attributes)
                                attribute.Value = GetValueFromParameters(pRow, pColumn, attribute.Value);
                            //
                            // Valoriser les éléments enfants            
                            ValuateDataWithParameters(pRow, pColumn, node.ChildNodes);
                        }
                    }
                }
            }
            #endregion
            #endregion Methods
        }
        #endregion Class

        #region Members
        #region Constantes
        // PM 20180219 [23824] Déplacée à partir de la class ProcessInOut (IOProcessInOut.cs)
        public const string PREFIX_XML_ROW_ID = "r";
        #endregion Constantes

        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static string TableName = "[Table]";
        private static WebClient m_WebClient;
        private static Stream m_Stream;
        private static StreamReader m_StreamReader;
        private static StreamWriter m_StreamWriter;
        private static StringReader m_StringReader;
        private static StringWriter m_StringWriter;
        private static int m_CurrentFileLineCount;
        #endregion Members

        #region Accessors
        public static Stream Stream
        {
            get { return m_Stream; }
        }
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static StreamWriter StreamWriter
        {
            get { return m_StreamWriter; }
        }
        public static StreamReader StreamReader
        {
            get { return m_StreamReader; }
        }
        public static StringReader StringReader
        {
            get { return m_StringReader; }
        }
        public static StringWriter StringWriter
        {
            get { return m_StringWriter; }
        }
        /// <summary>
        /// Retourne le nombre de lignes du fichier en cours
        /// </summary>
        public static int CurrentFileLineCount
        {
            get { return m_CurrentFileLineCount; }
        }
        #endregion Accessors

        #region Methods
        #region public CheckDefaultValue
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        /// <param name="pDefaultValue"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static string CheckDefaultValue(string pValue, string pDefaultValue)
        {
            if (pValue.ToUpper() == "INHERITED")
                return pDefaultValue;

            return pValue;
        }
        #endregion CheckDefaultValue
        #region public GetJoinSQLQuery
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTableName"></param>
        /// <param name="pJoinQuery"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static string GetJoinSQLQuery(string pTableName, string pJoinQuery)
        {
            pJoinQuery = pJoinQuery.Replace(TableName, pTableName);

            if (StrFunc.IsFilled(pJoinQuery))
            {
                if (pJoinQuery.StartsWith(SQLCst.X_FROM.ToString()))
                    return pJoinQuery;
                else
                    return ", " + pJoinQuery;
            }
            else
            {
                return SQLCst.FROM_DBO + pTableName + Cst.CrLf;
            }
        }
        #endregion

        #region File Functions
        #region DynamicFilePath
        #region public CheckDynamicString
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pXMLTask"></param>
        /// <param name="pIsData"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        public static string CheckDynamicString(string pString, IIOTaskLaunching pXMLTask, bool pIsData)
        {
            ArrayList elementRowDataList = null;
            return CheckDynamicString(pString, pXMLTask, pIsData, ref elementRowDataList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pXMLTask"></param>
        /// <param name="pIsInOut"></param>
        /// <param name="pIsData"></param>
        /// <param name="pDataValue"></param>
        /// <param name="pReturnData"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        public static string CheckDynamicString(string pString, IIOTaskLaunching pXMLTask, bool pIsData, string pDataValue, out string pReturnData)
        {
            ArrayList elementRowDataList = null;
            return CheckDynamicString(pString, pXMLTask, pIsData, pDataValue, out pReturnData, ref elementRowDataList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pXMLTask"></param>
        /// <param name="pIsInOut"></param>
        /// <param name="pIsData"></param>
        /// <param name="pElementRowDataList"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        public static string CheckDynamicString(string pString, IIOTaskLaunching pXMLTask, bool pIsData, ref ArrayList pElementRowDataList)
        {
            return CheckDynamicString(pString, pXMLTask, pIsData, string.Empty, out _, ref pElementRowDataList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pXMLTask"></param>
        /// <param name="pIsInOut"></param>
        /// <param name="pIsData"></param>
        /// <param name="pDataValue"></param>
        /// <param name="pReturnData"></param>
        /// <param name="pElementRowDataList"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        public static string CheckDynamicString(string pString, IIOTaskLaunching pXMLTask, bool pIsData, string pDataValue, out string pReturnData, ref ArrayList pElementRowDataList)
        {
            const string ElementStart = "<";
            const string ElementEnd = "/>";
            //
            const string SQLStart = ElementStart + "SQL ";
            const string SQLEnd = "</SQL>";
            //
            const string SpheresLibStart = ElementStart + "SpheresLib ";
            const string SpheresLibEnd = "</SpheresLib>";
            //
            const string RowStart = ElementStart + "Row ";
            const string RowDataStart = ElementStart + "RowData ";
            //
            // RD 20100421 [16955]
            pReturnData = string.Empty;
            //
            string retString = pString;
            //
            if (StrFunc.IsEmpty(retString))
            {
                return retString;
            }
            //
            int elementStartPos = retString.IndexOf(ElementStart);
            int elementEndPos = -1;
            int elementStartPos2 = -1;
            //
            if (elementStartPos == -1)
            {
                return retString;
            }
            //
            if (false == (StrFunc.ContainsIn(retString, SpheresLibStart) || StrFunc.ContainsIn(retString, SQLStart) ||
                StrFunc.ContainsIn(retString, RowDataStart) || StrFunc.ContainsIn(retString, RowStart)))
            {
                return retString;
            }
            //
            string elementEnd;
            string elementString;
            Type elementType;
            string dataResult = string.Empty;
            //
            try
            {
                while (elementStartPos > -1)
                {
                    bool isSQL = false;
                    bool isSpheresLib = false;
                    bool isRow = false;
                    bool isRowData = false;
                    //				
                    if ((retString.Length > elementStartPos + SQLStart.Length) && (retString.Substring(elementStartPos, SQLStart.Length) == SQLStart))
                    {
                        isSQL = true;
                        //
                        elementEnd = SQLEnd;
                        elementEndPos = retString.IndexOf(elementEnd, elementStartPos);
                        //
                        // Si je trouve pas la balise fermante </SQL>
                        // ou bien celle trouvée appartient à une autre balise ouvrante
                        // ERROR
                        elementStartPos2 = retString.IndexOf(SQLStart, elementStartPos + SQLStart.Length);
                        if ((elementEndPos == -1) || ((elementStartPos2 > -1) && (elementEndPos > elementStartPos2)))
                        {
                            throw new Exception("Element '" + SQLStart + "' is not closed - '" + elementEnd + "' is expected ");
                        }
                    }
                    else if ((retString.Length > elementStartPos + SpheresLibStart.Length) && (retString.Substring(elementStartPos, SpheresLibStart.Length) == SpheresLibStart))
                    {
                        isSpheresLib = true;
                        //
                        elementEnd = SpheresLibEnd;
                        elementEndPos = retString.IndexOf(elementEnd, elementStartPos);
                        //
                        // Si je trouve pas la balise fermante </SpheresLib> 
                        // ou bien celle trouvée appartient à une autre balise ouvrante
                        // je cherche la balise fermante />
                        elementStartPos2 = retString.IndexOf(SpheresLibStart, elementStartPos + SpheresLibStart.Length);
                        if ((elementEndPos == -1) || ((elementStartPos2 > -1) && (elementEndPos > elementStartPos2)))
                        {
                            elementEnd = ElementEnd;
                            elementEndPos = retString.IndexOf(elementEnd, elementStartPos);
                            //
                            // Si je trouve pas la balise fermante /> 
                            // ou bien celle trouvée appartient à une autre balise ouvrante
                            // ERROR
                            elementStartPos2 = retString.IndexOf(ElementStart, elementStartPos + SpheresLibStart.Length);
                            if ((elementEndPos == -1) || ((elementStartPos2 > -1) && (elementEndPos > elementStartPos2)))
                            {
                                throw new Exception("Element '" + SpheresLibStart + "' is not closed - '" + ElementEnd + "' or '" + SpheresLibEnd + "' are expected ");
                            }
                        }
                    }
                    else if ((retString.Length > elementStartPos + RowStart.Length) && (retString.Substring(elementStartPos, RowStart.Length) == RowStart))
                    {
                        isRow = true;
                        //
                        elementEnd = ElementEnd;
                        elementEndPos = retString.IndexOf(elementEnd, elementStartPos);
                    }
                    else if ((retString.Length > elementStartPos + RowDataStart.Length) && (retString.Substring(elementStartPos, RowDataStart.Length) == RowDataStart))
                    {
                        isRowData = true;
                        //
                        elementEnd = ElementEnd;
                        elementEndPos = retString.IndexOf(elementEnd, elementStartPos);
                    }
                    else
                    {
                        elementEnd = ElementEnd;
                        elementEndPos = retString.IndexOf(elementEnd, elementStartPos);
                    }
                    //
                    if (elementEndPos > -1)
                    {
                        elementString = retString.Substring(elementStartPos, elementEndPos + elementEnd.Length - elementStartPos);
                    }
                    else
                    {
                        elementString = retString.Substring(elementStartPos);
                    }
                    //
                    if (isSQL || isSpheresLib)
                    {
                        #region Sql or SpheresLib
                        IOMappedData ioData = new IOMappedData();
                        //
                        // RD 20100421 [16955]
                        if (StrFunc.IsFilled(pDataValue))
                        {
                            ioData.value = pDataValue;
                        }
                        //
                        EFS_SerializeInfoBase serializeInfo = null;
                        if (isSQL)
                        {
                            elementType = typeof(DynamicPathSQL);
                            serializeInfo = new EFS_SerializeInfoBase(elementType, elementString);
                            DynamicPathSQL dataSQL = (DynamicPathSQL)CacheSerializer.Deserialize(serializeInfo);
                            //
                            ioData.sql = dataSQL;
                            ioData.dataformat = dataSQL.format;
                        }
                        else if (isSpheresLib)
                        {
                            elementType = typeof(DynamicPathSpheresLib);
                            serializeInfo = new EFS_SerializeInfoBase(elementType, elementString);
                            DynamicPathSpheresLib dataSpheresLib = (DynamicPathSpheresLib)CacheSerializer.Deserialize(serializeInfo);
                            //
                            ioData.spheresLib = dataSpheresLib;
                            ioData.dataformat = dataSpheresLib.format;
                        }
                        //
                        // RD 20100421 [16955]
                        pReturnData = ioData.GetParamDataValue("RETURN", pXMLTask.Cs, null);
                        //
                        ProcessMapping.GetMappedDataValue(pXMLTask, pXMLTask.SetErrorWarning, ioData, null, ref dataResult);
                        //
                        if (StrFunc.IsFilled(dataResult))
                        {
                            retString = retString.Replace(elementString, dataResult);
                        }
                        else
                        {
                            throw new Exception("No data found for: '" + elementString + "'");
                        }
                        #endregion
                    }
                    else if (isRow || isRowData)
                    {
                        #region isRow or isRowData
                        string dataName = string.Empty;
                        string dataProperty = string.Empty;
                        string dataFormat = string.Empty;
                        //
                        GetRowDataInfo(elementString, isRow, ref dataName, ref dataProperty, ref dataFormat);
                        //
                        string listItem = (isRow ? "ROW" : "DATA") + ";" + (isRow ? dataProperty : dataName);
                        if ((null != pElementRowDataList) && (false == pElementRowDataList.Contains(listItem)))
                        {
                            pElementRowDataList.Add(listItem);
                        }
                        //
                        dataResult = "{" + listItem + "}";
                        retString = retString.Replace(elementString, dataResult);
                        #endregion
                    }
                    else
                    {
                        if (pIsData)
                        {
                            elementStartPos += ElementStart.Length;
                        }
                        else
                        {
                            throw new Exception("Element not supported: " + elementString);
                        }
                    }
                    //
                    elementStartPos = retString.IndexOf(ElementStart, elementStartPos + dataResult.Length);
                    dataResult = string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Dynamic string : '" + retString + "' is not correct", ex);
            }
            //
            return retString;
        }
        #endregion public CheckDynamicString
        #region public GetRowDataInfo
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pElementString"></param>
        /// <param name="pIsRow"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataProperty"></param>
        /// <param name="pDataFormat"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void GetRowDataInfo(string pElementString, bool pIsRow, ref string pDataName, ref string pDataProperty, ref string pDataFormat)
        {
            pDataName = string.Empty;
            Type elementType;
            EFS_SerializeInfoBase serializeInfo;
            if (pIsRow)
            {
                elementType = typeof(DynamicPathRow);
                serializeInfo = new EFS_SerializeInfoBase(elementType, pElementString);
                DynamicPathRow dynamicPathElement = (DynamicPathRow)CacheSerializer.Deserialize(serializeInfo);

                pDataProperty = dynamicPathElement.attribut;
                pDataFormat = dynamicPathElement.format;
            }
            else
            {
                elementType = typeof(DynamicPathData);
                serializeInfo = new EFS_SerializeInfoBase(elementType, pElementString);
                DynamicPathData dynamicPathElement = (DynamicPathData)CacheSerializer.Deserialize(serializeInfo);

                pDataName = dynamicPathElement.name;

                pDataProperty = dynamicPathElement.attribut;
                if (StrFunc.IsEmpty(pDataProperty))
                    pDataProperty = "VALUE";

                pDataFormat = dynamicPathElement.format;
            }
        }
        #endregion public GetRowDataInfo
        #region public SetDataRowToDynamicString
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataValue"></param>
        /// <param name="pXMLTask"></param>
        /// <param name="pRow"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        //public static string SetDataRowToDynamicString(string pDataValue, Task pXMLTask, IOTaskDetInOutFileRow pRow)
        public static string SetDataRowToDynamicString(string pDataValue, IIOTaskLaunching pXMLTask, IOTaskDetInOutFileRow pRow)
        {
            return SetDataRowToDynamicString(pDataValue, pXMLTask, pRow, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataValue"></param>
        /// <param name="pXMLTask"></param>
        /// <param name="pRow"></param>
        /// <param name="pIsData"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        //public static string SetDataRowToDynamicString(string pDataValue, Task pXMLTask, IOTaskDetInOutFileRow pRow, bool pIsData)
        public static string SetDataRowToDynamicString(string pDataValue, IIOTaskLaunching pXMLTask, IOTaskDetInOutFileRow pRow, bool pIsData)
        {
            const string ElementStart = "{";
            const string ElementEnd = "}";
            const string RowStart = ElementStart + "ROW";
            const string DataStart = ElementStart + "DATA";

            string retDataValue = pDataValue;
            if (StrFunc.IsEmpty(retDataValue))
            {
                return retDataValue;
            }

            retDataValue = CheckDynamicString(retDataValue, pXMLTask, pIsData);
            if (false == (StrFunc.ContainsIn(retDataValue, DataStart) || StrFunc.ContainsIn(retDataValue, RowStart)))
            {
                return retDataValue;
            }

            int elementStartPos = retDataValue.IndexOf(ElementStart);
            if (elementStartPos == -1)
            {
                return retDataValue;
            }

            string elementEnd;
            string elementString;
            string dataResult = string.Empty;
            try
            {
                while (elementStartPos > -1)
                {
                    bool isRow = false;
                    bool isData = false;
                    string dataProperty = string.Empty;
                    string dataName = string.Empty;

                    if ((retDataValue.Length > elementStartPos + RowStart.Length) && (retDataValue.Substring(elementStartPos, RowStart.Length).ToUpper() == RowStart))
                    {
                        isRow = true;
                    }
                    else if ((retDataValue.Length > elementStartPos + DataStart.Length) && (retDataValue.Substring(elementStartPos, DataStart.Length).ToUpper() == DataStart))
                    {
                        isData = true;
                    }

                    elementEnd = ElementEnd;
                    int elementEndPos = retDataValue.IndexOf(elementEnd, elementStartPos);

                    elementString = retDataValue.Substring(elementStartPos, elementEndPos + ElementEnd.Length - elementStartPos);

                    if (isRow || isData)
                    {
                        string tmpElementString = elementString.Substring(ElementStart.Length, elementString.Length - ElementStart.Length - ElementEnd.Length);
                        string[] rowData = tmpElementString.Split(';');

                        if (isRow)
                        {
                            dataProperty = rowData[1];
                            switch (dataProperty.Trim().ToUpper())
                            {
                                case "ID":
                                    dataResult = pRow.id;
                                    break;
                                case "SRC":
                                    dataResult = pRow.src;
                                    break;
                                case "STATUS":
                                    dataResult = pRow.status;
                                    break;
                                default:
                                    throw new Exception("Property: '" + dataProperty + "' is not supported for Row Element: " + elementString);
                            }
                            retDataValue = retDataValue.Replace(elementString, dataResult);
                        }
                        else
                        {
                            dataName = rowData[1];
                            dataProperty = "VALUE";
                            switch (dataProperty.Trim().ToUpper())
                            {
                                case "VALUE":
                                    bool dataFound = false;
                                    dataResult = OutputParsing.GetDataValueFromRow(pXMLTask, pRow, dataName, ref dataFound);
                                    if (false == dataFound)
                                    {
                                        throw new Exception("Incorrect data name for Data Element: " + elementString);
                                    }
                                    break;
                                default:
                                    throw new Exception("Property: '" + dataProperty + "' is not supported for Data Element: " + elementString);
                            }
                            retDataValue = retDataValue.Replace(elementString, dataResult);
                        }
                    }
                    else
                    {
                        if (pIsData)
                        {
                            elementStartPos += ElementStart.Length;
                        }
                        else
                        {
                            throw new Exception("Element not supported: " + elementString);
                        }
                    }
                    elementStartPos = retDataValue.IndexOf(ElementStart, elementStartPos + dataResult.Length);
                    dataResult = string.Empty;
                }
                return retDataValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Dynamic file path: " + pDataValue + " is not correct", ex);
            }
        }
        #endregion public SetDataRowToDynamicString
        #endregion DynamicFilePath

        #region OpenFile
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUrl"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        private static void OpenHttpFile(string pUrl)
        {
            m_WebClient = new WebClient
            {
                //PL 20120911 Use of DefaultCredentials
                Credentials = CredentialCache.DefaultCredentials
            };
            m_Stream = m_WebClient.OpenRead(pUrl);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pStreamReader"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        private static void OpenHttpFile(string pUrl, ref StreamReader pStreamReader)
        {
            OpenHttpFile(pUrl);
            pStreamReader = new StreamReader(m_Stream);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pLocalFilePath"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void DownloadWebFile(string pUrl, string pLocalFilePath)
        {
            WebClient webClient = new WebClient
            {
                //PL 20120911 Use of DefaultCredentials
                Credentials = CredentialCache.DefaultCredentials
            };
            webClient.DownloadFile(pUrl, pLocalFilePath);
        }

        /// <summary>
        /// Ouvre un fichier en lecture
        /// <para>Affecte m_StreamReader</para>
        /// </summary>
        /// <param name="pFilePath"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void OpenFile(string pFilePath)
        {
            OpenFile(pFilePath, Cst.InputSourceDataStyle.ANSIFILE.ToString());
        }
        /// <summary>
        /// Ouvre un fichier en lecture
        /// <para>Affecte m_StreamReader</para>
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <param name="pFileEncoding"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void OpenFile(string pFilePath, string pFileEncoding)
        {
            // PM 20180219 [23824] Remplacement par appel à nouvelle méthode
            //if (IsHttp(pFilePath))
            //{
            //    OpenHttpFile(pFilePath, ref m_StreamReader);
            //}
            //else
            //{
            //    if (File.Exists(pFilePath))
            //    {
            //        Encoding fileEncoding = (pFileEncoding == Cst.InputSourceDataStyle.UNICODEFILE.ToString()) ? Encoding.Unicode : Encoding.Default;

            //        //PL 20130710 Add File.Open and usage of m_Stream 
            //        //m_StreamReader = new StreamReader(pFilePath, fileEncoding);
            //        m_Stream = File.Open(pFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //        m_StreamReader = new StreamReader(m_Stream, fileEncoding);
            //    }
            //    else
            //    {
            //        throw new Exception("Error: File does not exist! [Filename: " + pFilePath + "]");
            //    }
            //}
            OpenFile(pFilePath, pFileEncoding, ref m_Stream, ref m_StreamReader);
        }

        /// <summary>
        /// Ouvre un fichier en lecture
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <param name="pFileEncoding"></param>
        /// <param name="pStream"></param>
        /// <param name="pStreamReader"></param>
        // PM 20180219 [23824] Ajout pour stream non static
        public static void OpenFile(string pFilePath, string pFileEncoding, ref Stream pStream, ref StreamReader pStreamReader)
        {
            if (IsHttp(pFilePath))
            {
                OpenHttpFile(pFilePath, ref pStreamReader);
            }
            else
            {
                if (File.Exists(pFilePath))
                {
                    Encoding fileEncoding = (pFileEncoding == Cst.InputSourceDataStyle.UNICODEFILE.ToString()) ? Encoding.Unicode : Encoding.Default;
                    //
                    pStream = File.Open(pFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    pStreamReader = new StreamReader(pStream, fileEncoding);
                }
                else
                {
                    throw new Exception("Error: File does not exist! [Filename: " + pFilePath + "]");
                }
            }
        }
        /// <summary>
        /// Ouvre un fichier en écriture
        /// <para>Affecte m_StreamWriter</para>
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <param name="pWriteMode"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void OpenFile(string pFilePath, Cst.WriteMode pWriteMode)
        {
            OpenFile(pFilePath, pWriteMode, Cst.InputSourceDataStyle.ANSIFILE.ToString());
        }
        /// <summary>
        /// Ouvre un fichier en écriture
        /// <para>Affecte m_StreamWriter</para>
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <param name="pWriteMode"></param>
        /// <param name="pFileEncoding"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void OpenFile(string pFilePath, Cst.WriteMode pWriteMode, string pFileEncoding)
        {
            OpenFile(pFilePath, pWriteMode, pFileEncoding, string.Empty);
        }
        /// <summary>
        /// Ouvre un fichier en écriture
        /// <para>Affecte m_StreamWriter</para>
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <param name="pWriteMode"></param>
        /// <param name="pFileEncoding"></param>
        /// <param name="pTempDirectory"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void OpenFile(string pFilePath, Cst.WriteMode pWriteMode, string pFileEncoding, string pTempDirectory)
        {
            try
            {
                string correctFilePath = pFilePath;
                FileInfo fInfo = new FileInfo(correctFilePath);
                if (!fInfo.Directory.Exists)
                {
                    throw new Exception("File path: " + fInfo.Directory.FullName + " does not exist ");
                }
                else
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileInfo assemblyFileInfo = new FileInfo(assembly.Location);
                    //
                    if (assemblyFileInfo.Directory.FullName == fInfo.Directory.FullName)
                        fInfo = new FileInfo(pTempDirectory + @"/" + correctFilePath);
                    //
                    correctFilePath = fInfo.Directory.FullName + @"/" + fInfo.Name;
                    //
                    Encoding fileEncoding = Encoding.Default;
                    if (pFileEncoding == Cst.InputSourceDataStyle.UNICODEFILE.ToString())
                        fileEncoding = Encoding.Unicode;
                    //
                    switch (pWriteMode)
                    {
                        case Cst.WriteMode.APPEND:
                            m_StreamWriter = new StreamWriter(correctFilePath, true, fileEncoding);
                            break;
                        case Cst.WriteMode.WRITE:
                            m_StreamWriter = new StreamWriter(correctFilePath, false, fileEncoding);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error to open file : " + pFilePath + " - " + ex.Message, ex);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        // RD 20100111 [16818] MS Excel® file import
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void OpenStringReader(string pData)
        {
            OpenStringReader(pData, Cst.InputSourceDataStyle.ANSIFILE.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pDataEncoding"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void OpenStringReader(string pData, string pDataEncoding)
        {
            Encoding dataEncoding = Encoding.Default;
            //
            if (pDataEncoding == Cst.InputSourceDataStyle.UNICODEFILE.ToString())
                dataEncoding = Encoding.Unicode;
            //
            string encodedData = dataEncoding.GetString(dataEncoding.GetBytes(pData));
            //
            m_StringReader = new StringReader(encodedData);
        }

        /// <summary>
        /// 
        /// </summary>
        // RD 20100111 [16818] MS Excel® file import
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void OpenStringWriter()
        {
            m_StringWriter = new StringWriter();
        }
        #endregion

        #region CloseFile
        /// <summary>
        /// 
        /// </summary>
        // PM 20180219 [23824] Déplacé à partir de IOTools et rendue public
        //private static void CloseHttpFile()
        public static void CloseHttpFile()
        {
            if (null != m_WebClient)
            {
                m_WebClient.Dispose();
            }
        }
        //
        /// <summary>
        ///
        /// </summary>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void CloseFile()
        {
            CloseFile(string.Empty);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="pFilePath"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void CloseFile(string pFilePath)
        {
            CloseStream();
            //
            if (IsHttp(pFilePath))
            {
                CloseHttpFile();
            }
        }
        public static void CloseStream()
        {
            if (m_StringReader != null)
            {
                m_StringReader.Close();
            }
            // RD 20100111 [16818] MS Excel® file import
            if (m_StringWriter != null)
            {
                m_StringWriter.Close();
            }
            if (m_Stream != null)
            {
                m_Stream.Close();
            }
            if (m_StreamReader != null)
            {
                m_StreamReader.Close();
            }
            if (m_StreamWriter != null)
            {
                m_StreamWriter.Close();
            }
        }
        #endregion

        #region Others
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static bool IsHttp(string pFilePath)
        {
            if ((StrFunc.IsFilled(pFilePath)) && (pFilePath.StartsWith("http://")))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Valorise le nombre de lignes du fichier
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <param name="pFileEncoding"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools et rendue public
        //private static void GetFileLineCount(string pFilePath, string pFileEncoding)
        public static void GetFileLineCount(string pFilePath, string pFileEncoding)
        {
            m_CurrentFileLineCount = 0;

            if (StrFunc.IsFilled(pFilePath))
            {
                OpenFile(pFilePath, pFileEncoding);
                while ((_ = m_StreamReader.ReadLine()) != null)
                {
                    m_CurrentFileLineCount += 1;
                }
                CloseFile(pFilePath);
            }
        }
        // PM 20180219 [23824] Déplacé à partir de IOTools
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        //public static string GetFileMsg(string pFilePath, Task pTask)
        public static string GetFileMsg(string pFilePath, IIOTaskLaunching pTask)
        {
            return GetFileMsg(pFilePath, pTask, 0);
        }
        // PM 20180219 [23824] Déplacé à partir de IOTools
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        //public static string GetFileMsg(string pFilePath, Task pTask, int pNum)
        public static string GetFileMsg(string pFilePath, IIOTaskLaunching pTask, int pNum)
        {
            FileInfo fInfo = new FileInfo(pFilePath);
            if (false == fInfo.Directory.Exists)
            {
                // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
                //fInfo = new FileInfo(pTask.process.appInstance.GetFilepath(pFilePath));
                fInfo = new FileInfo(pTask.GetFilepath(pFilePath));
            }
            //
            return GetFileMsg(fInfo, pNum);
        }
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static string GetFileMsg(FileInfo pFileInfo)
        {
            return GetFileMsg(pFileInfo, 0);
        }
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static string GetFileMsg(FileInfo pFileInfo, int pNum)
        {
            string fileMsg;
            if (pNum > 0)
            {
                fileMsg = "[File " + pNum.ToString() + ": " + pFileInfo.FullName;
                if (pFileInfo.Exists)
                {
                    fileMsg += ", Date: " + DtFunc.DateTimeToString(pFileInfo.CreationTime, DtFunc.FmtDateLongTime);
                    fileMsg += ", " + GetFileSizeMsg(pFileInfo) + "]";
                }
                else
                {
                    fileMsg += "]";
                }
            }
            else
            {
                fileMsg = "[File: " + pFileInfo.FullName + "]" + Cst.CrLf;
                if (pFileInfo.Exists)
                {
                    fileMsg += "[Date: " + DtFunc.DateTimeToString(pFileInfo.CreationTime, DtFunc.FmtDateLongTime) + "]" + Cst.CrLf;
                    fileMsg += "[" + GetFileSizeMsg(pFileInfo) + "]";
                }
            }
            //
            return fileMsg;
        }
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static string GetFileSizeMsg(string pFilePath)
        {
            FileInfo fileInfo = new FileInfo(pFilePath);
            return GetFileSizeMsg(fileInfo);
        }
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static string GetFileSizeMsg(FileInfo pFileInfo)
        {
            decimal fileSize = decimal.Divide(pFileInfo.Length, 1024);
            fileSize = decimal.Round(fileSize, 2);
            string fileMsg = "Size: " + fileSize.ToString() + " KB";
            return fileMsg;
        }
        //
        /// <summary>
        /// Retourne le nombre de lignes du flux
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools et rendue public
        //private static void GetFlowLineCount(string pData)
        public static void GetFlowLineCount(string pData)
        {
            m_CurrentFileLineCount = 0;
            // RD 20100111 [16818] MS Excel® file import
            OpenStringReader(pData);

            while ((_ = m_StringReader.ReadLine()) != null)
            {
                m_CurrentFileLineCount += 1;
            }
            CloseStream();
        }

        /// <summary>
        ///  Déplace le fichier dans un nouveau répertoire 
        /// </summary>
        /// <param name="pNewFolder">Représente le folder de destination</param>
        /// <param name="pFileInfo">représente le fichier à déplacer</param>
        /// <param name="pFilePath">nom complet du fichier une fois déplacé</param>
        /// FI 20131113 [19081] Add Method MoveFileToFolder
        /// FI 20131223 [XXXXX] add FileTools.ErrLevel code en cas d'erreur 
        /// FI 20160503 [XXXXX] Modify 
        // PM 20180219 [23824] Déplacé à partir de IOTools et suppression du paramètre pProcessInOut
        //public static FileTools.ErrLevel MoveFileToFolder(ProcessInOut pProcessInOut, string pNewFolder, FileInfo pFileInfo, ref string opFilePath)
        public static FileTools.ErrLevel MoveFileToFolder(string pNewFolder, FileInfo pFileInfo, ref string opFilePath)
        {
            string finalFilePath = Path.Combine(pNewFolder, pFileInfo.Name);

            // FI 20160503 [XXXXX] Appel à FileMove3
            FileTools.ErrLevel ret = FileTools.FileMove3(pFileInfo.FullName, ref finalFilePath);

            switch (ret)
            {
                case FileTools.ErrLevel.SUCCESS:
                    opFilePath = finalFilePath;
                    break;
                case FileTools.ErrLevel.FILENOTFOUND:
                case FileTools.ErrLevel.IOEXCEPTION: // FI 20160503 [XXXXX] add (valeur de retour possible avec la méthode FileTools.FileMove3)
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("MoveFileToFolder Function.Value :{0} is not implemented", ret.ToString()));
            }
            return ret;
        }

        /// <summary>
        /// Génère un folder de backUp enfant de folderPath
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        /// FI 20131113 [19081] Add Method CreateBkpFolder
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static string CreateBkpFolder(string folderPath)
        {
            string ret = Path.Combine(folderPath, "Bkp");
            DirectoryInfo directoryInfo = new DirectoryInfo(ret);
            if (!directoryInfo.Exists)
            {
                try
                {
                    SystemIOTools.CreateDirectory(ret);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                            @"<b>Backup folder not created</b>" + Cst.CrLf +
                            "[Folder: " + folderPath + "][Bkp Folder: " + ret + "]" + Cst.CrLf + Cst.SYSTEM_EXCEPTION, ex);
                }
            }

            _ = new DirectoryInfo(ret);
            FileTools.CheckFolder(ret, 30, out _);
            return ret;
        }
        #endregion

        /// <summary>
        /// Retourne une ligne de StringReader ou StreamReader suivant la valeur de pIsFromString
        /// </summary>
        /// <param name="pIsFromString"></param>
        /// <returns></returns>
        public static string StringOrStreamReadLine(bool pIsFromString)
        {
            string currentLine;
            if (pIsFromString)
            {
                currentLine = m_StringReader.ReadLine();
            }
            else
            {
                currentLine = m_StreamReader.ReadLine();
            }
            return currentLine;
        }
        #endregion File Function

        #region public RowTableDesc
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pRowTable"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static ArrayList RowTableDesc(IOTaskDetInOutFileRow pRow, IOTaskDetInOutFileRowTable pRowTable)
        {
            ArrayList ret = new ArrayList
            {
                RowDesc(pRow),
                RowTableDesc(pRowTable)
            };
            return ret;
        }
        public static string RowDesc(IOTaskDetInOutFileRow pRow)
        {
            return "Row (id:<b>" + pRow.id + "</b>, src:" + pRow.src + ")";
        }
        public static string RowTableDesc(IOTaskDetInOutFileRowTable pRowTable)
        {
            return "Table (name:<b>" + pRowTable.name + "</b>, sequenceno:" + pRowTable.sequenceno + ")";
        }
        #endregion RowTableDesc
        #region public SetInCDATA
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static XmlCDataSection SetInCDATA(string pData)
        {
            XmlDocument doc = new XmlDocument();
            return SetInCDATA(pData, doc);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pMainDocument"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static XmlCDataSection SetInCDATA(string pData, XmlDocument pMainDocument)
        {
            XmlCDataSection ret = null;
            //
            try
            {
                // Load XML
                XmlDocument xmlData = new XmlDocument();
                xmlData.LoadXml(pData);

                // Remove XmlDeclaration Node
                XMLTools.RemoveXmlDeclaration(xmlData);

                // Set XML to CDataSection
                ret = pMainDocument.CreateCDataSection(xmlData.InnerXml);
            }
            catch (XmlException)
            {
                // On arrive ici si le LoadXml plante => ds ce cas la donnée n'est pas XML => pas de CDATA
            }
            //
            return ret;
        }
        #endregion public SetInCDATA
        #region public XMLIsDataTypeSpecified
        /// <summary>
        /// Alimente DataTypeSpecified
        /// </summary>
        /// <param name="pDataType"></param>
        /// <param name="pDataTypeSpecified"></param>
        /// <param name="pOverrides"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static bool XMLIsDataTypeSpecified(string pDataType, bool pIsLightSerializeMode)
        {
            if (pIsLightSerializeMode)
            {
                // Pour le mode LIGHT, si string, ne pas sérialiser            
                return (StrFunc.IsFilled(pDataType) && (false == TypeData.IsTypeString(pDataType)));
            }
            else
            {
                return true;
            }
        }
        #endregion public XMLIsDataTypeSpecified
        #region public XMLSetStatus
        /// <summary>
        /// Alimente status pour la sérialisation
        /// </summary>
        /// <param name="pStatus"></param>
        /// <param name="pStatusSpecified"></param>
        /// <param name="pIsError"></param>
        /// <param name="pIsLightSerializeMode"></param>
        // PM 20180219 [23824] Déplacé à partir de IOTools
        public static void XMLSetStatus(ref string pStatus, ref bool pStatusSpecified, bool pIsError, bool pIsLightSerializeMode)
        {
            if (false == pIsError)
            {
                // Pour le mode LIGHT ne pas mettre success
                if (pIsLightSerializeMode)
                {
                    pStatus = string.Empty;
                }
                else
                {
                    pStatus = ProcessStateTools.StatusSuccess.ToLower();
                }
            }
            else
            {
                pStatus = ProcessStateTools.StatusError.ToLower();
            }
            //
            // Pour le mode LIGHT, si success, ne pas sérialiser
            if (pIsLightSerializeMode)
            {
                pStatusSpecified = StrFunc.IsFilled(pStatus);
            }
            else
            {
                pStatusSpecified = true;
            }
        }
        #endregion public XMLSetStatus

        #region public LoadLine
        /// <summary>
        /// Parse une ligne et insère le contenu dans un IOTaskDetInOutFileRow
        /// </summary>
        /// <param name="pTask">tâcheIO</param>
        /// <param name="pLine">ligne en entrée</param>
        /// <param name="pLineNumber"></param>
        /// <param name="pRowId"></param>
        /// <param name="pInputParsing">Liste des parsings</param>
        /// <param name="opRow">Résultat du parsing</param>
        /// <param name="opNbParsingIgnoredLines">est incrémenté de 1 si la ligne est ignorée par le parsing</param>
        /// <returns></returns>
        // FL/PL 20130225 Add return boolean value 
        // PM 20180219 [23824] Déplacé à partir de IOTools
        // PM 20180219 [23824] Task => IIOTaskLaunching
        //public static bool LoadLine(Task pTask, string pLine, int pLineNumber, int pRowId, InputParsing pInputParsing,
        //    ref IOTaskDetInOutFileRow opRow, ref int opNbParsingIgnoredLines)
        // EG 20190114 Add detail to ProcessLog Refactoring
        public static bool LoadLine(IIOTaskLaunching pTask, string pLine, int pLineNumber, int pRowId, InputParsing pInputParsing,
            ref IOTaskDetInOutFileRow opRow, ref int opNbParsingIgnoredLines)
        {
            IOTaskDetInOutFileRowData[] rowData = null;
            bool isLightSerializeMode = false;

            // 20081226 FI [16446] add test sur IsLineToParse pour afficher un message correct
            // RD 20100608 [] Bug: Dans le cas du même parsing définit plusieurs fois sur un import avec des valeurs de controls
            bool isToParse = pInputParsing.IsLineToParse(pLine);
            if (!isToParse)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 6018), 2, new LogParam(pLineNumber)));

                opNbParsingIgnoredLines++;
            }

            if (isToParse)
            {
                //FI 20120912 usage de SpheresExceptionParser
                //au passage usage de isParsingOk variable plus optimiste que isError
                bool isParsingOk = true;
                try
                {
                    // RD 20110401 [17379] Serialize mode : Add param IsLightSerializeMode
                    isParsingOk = pInputParsing.ParseLine(pLine, pLineNumber, isLightSerializeMode, out rowData);
                }
                catch (Exception ex)
                {
                    isParsingOk = false;

                    SpheresException2 LogEx = SpheresExceptionParser.GetSpheresException("Error on Parsing Line :" + pLineNumber.ToString(), ex);

                    // FI 20200629 [XXXXX] Add AddException
                    pTask.AddCriticalException(LogEx);

                    // PM 20180219 [23824] Task => IIOTaskLaunching
                    
                    Logger.Log(new LoggerData(LogEx));
                }
                finally
                {
                    // RD 20110323
                    opRow = new IOTaskDetInOutFileRow
                    {
                        id = PREFIX_XML_ROW_ID + pRowId.ToString(),
                        src = pLineNumber.ToString(),
                        data = rowData
                    };
                    // RD 20110401 [17379] Serialize mode : Pour le mode Light ne pas sérialiser le Status s'il est = success
                    XMLSetStatus(ref opRow.status, ref opRow.statusSpecified, (false == isParsingOk), isLightSerializeMode);
                }
            }

            return isToParse;
        }
        #endregion public LoadLine
        #endregion Methods
    }
}

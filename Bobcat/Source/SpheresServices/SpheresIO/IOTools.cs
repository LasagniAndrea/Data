using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.MQueue;
using EfsML.DynamicData;
//
namespace EFS.SpheresIO
{
    #region class IOTools
    public class IOTools
    {
        #region Members
        // RD 20100111 [16818] MS Excel® file import
        public static string Sheet = "Sheet";
        public static string Range = "Range";
        public static string Element = "Element";
        public static string Filter = "Filter";
        public static string DataSource = "Data Source";
        public static string ExcelDataSeparator = @"\x09";
        public static string ExcelDataSeparatorCaracter = Regex.Unescape(ExcelDataSeparator);
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region public RowTableDesc
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
            return $"Row (id:<b>{pRow.id}</b>, src:{pRow.src})";
        }
        public static string RowTableDesc(IOTaskDetInOutFileRowTable pRowTable)
        {
            return $"Table (name:<b>{pRowTable.name}</b>, sequenceno:{pRowTable.sequenceno})";
        }
        #endregion RowTableDesc

        #region File Functions
        #region Others
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessInOut"></param>
        /// <param name="pFilePath"></param>
        /// <param name="pDataConnection"></param>
        public static void VerifyDataConnectionForImport(ProcessInOut pProcessInOut, string pFilePath, ref string pDataConnection)
        {
            if (StrFunc.IsEmpty(pProcessInOut.DataConnection) || pProcessInOut.DataConnection.ToLower().Contains("please insert"))
            {
                throw new Exception(
                    @"<b>You forgot to specify the connection to the data source of import</b>
                         (eg: 'Provider=Microsoft.Jet.OLEDB.4.0;Extended Properties=''Excel 8.0''' for Excel 97 onwards Excel file format 
                               or 'Provider=Microsoft.ACE.OLEDB.12.0;Extended Properties=''Excel 12.0''' for Excel 2007 or higher Excel file format)" + Cst.CrLf + ArrFunc.GetStringList(pProcessInOut.MsgLog, Cst.CrLf));
            }
            //
            pDataConnection = DataSource + "=" + pFilePath + ";" + pProcessInOut.DataConnection.Trim();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessInOut"></param>
        /// <param name="pExcelSheetName"></param>
        /// <param name="pExcelRange"></param>
        public static void GetExcelSheetNameAndDataRange(ProcessInOut pProcessInOut, ref string pExcelSheetName, ref string pExcelRange)
        {
            string temp1 = StrFunc.ExtractString(pProcessInOut.DataName, 1, ";");
            string temp2 = StrFunc.ExtractString(pProcessInOut.DataName, 2, ";");
            //
            if (StrFunc.IsFilled(temp1))
            {
                if (StrFunc.ExtractString(temp1, 0, "=") == Sheet)
                    pExcelSheetName = StrFunc.ExtractString(temp1, 1, "=");
                else if (StrFunc.ExtractString(temp1, 0, "=") == Range)
                    pExcelRange = StrFunc.ExtractString(temp1, 1, "=");
            }
            //
            if (StrFunc.IsFilled(temp2))
            {
                if (StrFunc.ExtractString(temp2, 0, "=") == Sheet)
                    pExcelSheetName = StrFunc.ExtractString(temp2, 1, "=");
                else if (StrFunc.ExtractString(temp2, 0, "=") == Range)
                    pExcelRange = StrFunc.ExtractString(temp2, 1, "=");
            }
            //
            pExcelSheetName = pExcelSheetName.Trim();
            pExcelRange = pExcelRange.Trim();
            //
            if (StrFunc.IsEmpty(pExcelSheetName))
            {
                throw new Exception(
                            @"<b>You forgot to specify the worksheet name of the Excel file, which contains the data to be imported</b>
                                        - Verify it, and if necessary add it into the end of the file path preceding with ';" + Sheet + @"='. 
                                        - You can also specify the range of data to be imported preceding with ';" + Range + @"='. 
                                        (eg: \\server\share\directory\sample.xls;" + Sheet + "=Sheet1;" + Range + "=B1:HV16)" + Cst.CrLf + ArrFunc.GetStringList(pProcessInOut.MsgLog, Cst.CrLf));
            }
        }
        #endregion
        #endregion File Function

        #region PostParsing Flow Functions
        // RD 20100111 [16818] MS Excel® file import
        //public static void SetFileInfoToPostParsingFlow(string pFilePath, ref IOTaskDetInOutFile pXmlParsing)
        //{
        //    SetFileInfoToPostParsingFlow(true, pFilePath, ref pXmlParsing);
        //}
        public static void SetFileInfoToPostParsingFlow(bool pIsFile, string pFilePath, ref IOTaskDetInOutFile pXmlParsing)
        {

            pXmlParsing.name = null;
            pXmlParsing.folder = null;
            pXmlParsing.date = null;
            pXmlParsing.size = null;
            //
            if (pIsFile)
            {
                // PM 20180219 [23824] IOTools => IOCommonTools
                //if (IsHttp(pFilePath))
                if (IOCommonTools.IsHttp(pFilePath))
                {
                    pXmlParsing.nameSpecified = StrFunc.IsFilled(pFilePath.Substring(pFilePath.LastIndexOf("/") + 1));
                    if (pXmlParsing.nameSpecified)
                        pXmlParsing.name = pFilePath.Substring(pFilePath.LastIndexOf("/") + 1);
                    //
                    pXmlParsing.folderSpecified = StrFunc.IsFilled(pFilePath.Substring(0, pFilePath.LastIndexOf("/")));
                    if (pXmlParsing.folderSpecified)
                        pXmlParsing.folder = pFilePath.Substring(0, pFilePath.LastIndexOf("/"));
                }
                else
                {
                    FileInfo fileInfo = new FileInfo(pFilePath);
                    //
                    pXmlParsing.nameSpecified = StrFunc.IsFilled(fileInfo.Name);
                    if (pXmlParsing.nameSpecified)
                        pXmlParsing.name = fileInfo.Name;
                    //
                    pXmlParsing.folderSpecified = StrFunc.IsFilled(fileInfo.DirectoryName);
                    if (pXmlParsing.folderSpecified)
                        pXmlParsing.folder = fileInfo.DirectoryName;
                    //
                    pXmlParsing.dateSpecified = DtFunc.IsDateTimeFilled(fileInfo.CreationTime.Date);
                    // RD 20100826 / Mettre la date au format ISO
                    if (pXmlParsing.dateSpecified)
                        pXmlParsing.date = DtFunc.DateTimeToStringISO(fileInfo.CreationTime);
                    //
                    decimal fileSize = decimal.Divide(fileInfo.Length, 1024);
                    pXmlParsing.sizeSpecified = (fileSize > 0);
                    if (pXmlParsing.sizeSpecified)
                    {
                        fileSize = decimal.Round(fileSize, 2);
                        pXmlParsing.size = fileSize.ToString() + " KB";
                    }
                    // fileInfo = null; 20210105 [25634] positionnement à null non nécessaire. Mémoire gérée par GC
                }
            }
        }
        //public static void InitPostParsingFlow(string pFlow, string pDataStyle, ref  IOTaskDetInOutFile pXmlParsing)
        //{
        //    InitPostParsingFlow(true, pFlow, pDataStyle, ref pXmlParsing);
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsFile"></param>
        /// <param name="pFlow"></param>
        /// <param name="pDataStyle"></param>
        /// <param name="pXmlParsing"></param>
        /// FI 20131021 [17861] Modify method
        /// Cette méthode n'est plus appelée en mode EUREXTHEORETICALPRICEFILE_RISKARRAY
        public static void InitPostParsingFlow(bool pIsFile, string pFlow, string pDataStyle, ref IOTaskDetInOutFile pXmlParsing)
        {

            //Lire les infos du fichier
            SetFileInfoToPostParsingFlow(pIsFile, pFlow, ref pXmlParsing);
            //
            if (pIsFile)
            {
                // Nombre total de lignes du fichier, 
                // FI 20131021 [17861] Mise en commentaire de l'instruction if
                //if (pDataStyle != Cst.InputSourceDataStyle.EUREXTHEORETICALPRICEFILE_RISKARRAY.ToString())
                // PM 20180219 [23824] IOTools => IOCommonTools
                //GetFileLineCount(pFlow, pDataStyle);
                IOCommonTools.GetFileLineCount(pFlow, pDataStyle);
            }
            else
            {
                // Nombre total de lignes du flux 
                // PM 20180219 [23824] IOTools => IOCommonTools
                //GetFlowLineCount(pFlow);
                IOCommonTools.GetFlowLineCount(pFlow);
            }
            //
            // Ici prendre le Nombre total de lignes non vides, 
            // car les lignes vides sont ignorées automatqiuement par le Parsing 
            // RD 20110323 / Ici on risque de prendre en considération dans m_CurrentFileFilledLineCount les lignes ignorées par le Parsing
            //pXmlParsing.row = new IOTaskDetInOutFileRow[m_CurrentFileFilledLineCount];
        }
        #endregion PostParsing Flow Functions

        /// <summary>
        /// Load XSL file
        /// </summary>
        /// <param name="pXslTransform"></param>
        /// <param name="pXslFilePath"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void XslTransformLoad(XslCompiledTransform pXslTransform, string pXslFilePath,
            XsltSettings pXslSettings, XmlResolver pXmlResolver)
        {
            int countLoop = 1;
            bool isToLoop = true;

            // La première fois on rentre dans la boucle
            while (isToLoop)
            {
                try
                {
                    // Load the XSL data.
                    if (pXmlResolver == null)
                        pXslTransform.Load(pXslFilePath);
                    else
                        pXslTransform.Load(pXslFilePath, pXslSettings, pXmlResolver);

                    // Fichier XSL n'est pas bloqué par un autre process, donc pas besoin de boucler
                    isToLoop = false;
                }
                catch (Exception ex)
                {
                    if (FileTools.IsFileUsedException(ex))
                    {
                        //Pause de 5 sec.
                        Thread.Sleep(FileTools.ThreadSleepBeforLoop);

                        // On tente l'utilisation du fichier XSL au maximum 10 fois
                        if (countLoop < FileTools.MaxLoopPass)
                            // Nombre de boucles incrémenté
                            countLoop++;
                        else
                            throw;
                    }
                    else
                        throw;
                }
            }
        }

        #region Xml Functions
        #region XmlTransformInStream
        public static MemoryStream XmlTransformInStream(XmlDocument pXmlDoc, string pXslFilePath)
        {

            // Load the XSL data.
            XslCompiledTransform xslTransform = new XslCompiledTransform();
            // RD 20140127 [19530] Use IOTools.XslTransformLoad method
            IOTools.XslTransformLoad(xslTransform, pXslFilePath, null, null);
            // Transform XML and then load the results into XmlDocument.
            XPathNavigator xmlFileNav = pXmlDoc.CreateNavigator();
            MemoryStream msOutput = new MemoryStream();
            xslTransform.Transform(xmlFileNav, null, msOutput);
            return msOutput;
        }
        #endregion

        #region XmlTransformInDoc
        public static XmlDocument XmlTransformInDoc(StringBuilder pXmlStrBuilder, string pXslFilePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(pXmlStrBuilder.ToString().Trim());
            //
            return XmlTransformInDoc(xmlDoc, pXslFilePath);
        }
        public static XmlDocument XmlTransformInDoc(XmlDocument pXmlDoc, string pXslFilePath)
        {
            MemoryStream msOutput = XmlTransformInStream(pXmlDoc, pXslFilePath);
            //
            // Set the position to the beginning of the stream.
            msOutput.Seek(0, SeekOrigin.Begin);
            XmlTextReader xtReader = new XmlTextReader(msOutput);
            //
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xtReader);
            //
            msOutput.Close();
            //
            return xmlDoc;

        }
        #endregion

        #region XmlTransformInSb
        public static StringBuilder XmlTransformInSb(XmlDocument pXmlDoc, string pXslFilePath)
        {
            return XmlTransformInSb(pXmlDoc, pXslFilePath, null);
        }
        public static StringBuilder XmlTransformInSb(XmlDocument pXmlDoc, string pXslFilePath, IOTaskParams pParams)
        {
            // Load the XSL data.
            XslCompiledTransform xslTransform = new XslCompiledTransform();
            // RD 20140127 [19530] Use IOTools.XslTransformLoad method
            IOTools.XslTransformLoad(xslTransform, pXslFilePath, null, null);

            // Transform XML and then load the results into XmlDocument.
            XPathNavigator xmlFileNav = pXmlDoc.CreateNavigator();
            StringBuilder sbXMLResult = new StringBuilder();
            //
            XmlTextWriter xmlFileWriter = new XmlTextWriter(new StringWriter(sbXMLResult));
            //
            // RD 20110303 Pour passer au fichier XSL de Parsin les prameters du Task
            XsltArgumentList args = null;
            //
            if (null != pParams)
            {
                args = new XsltArgumentList();
                //
                foreach (IOTaskParamsParam ioParameter in pParams.param)
                    args.AddParam(ioParameter.id, "", ioParameter.Value);
            }
            //
            xslTransform.Transform(xmlFileNav, args, xmlFileWriter);
            xmlFileWriter.Close();
            //
            return sbXMLResult;

        }
        public static StringBuilder XmlTransformInSb(string pXmlFlow, string pXslFilePath, IOTaskParams pParams)
        {
            return XmlTransformInSb(true, pXmlFlow, pXslFilePath, pParams);
        }
        public static StringBuilder XmlTransformInSb(bool pIsFile, string pXmlFlow, string pXslFilePath, IOTaskParams pParams)
        {

            XmlDocument docXmlFlow = new XmlDocument();
            //	
            if (pIsFile)
            {
                // Load XML source 
                // PM 20180219 [23824] IOTools => IOCommonTools
                //OpenFile(pXmlFlow);
                IOCommonTools.OpenFile(pXmlFlow);
                //
                //if (IsHttp(pXmlFlow))
                if (IOCommonTools.IsHttp(pXmlFlow))
                {
                    try
                    {
                        //docXmlFlow.Load(m_Stream);
                        docXmlFlow.Load(IOCommonTools.Stream);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error on loading xml sream", ex);
                    }
                    finally
                    {
                        //CloseHttpFile();
                        IOCommonTools.CloseHttpFile();
                    }
                }
                else
                {
                    try
                    {
                        docXmlFlow.Load(pXmlFlow);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(StrFunc.AppendFormat("Error on loading xml File {0}", pXmlFlow), ex);
                    }
                    finally
                    {
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //CloseFile(pXmlFlow);
                        IOCommonTools.CloseFile(pXmlFlow);
                    }
                }

            }
            else
            {
                docXmlFlow.LoadXml(pXmlFlow);
            }
            //
            return XmlTransformInSb(docXmlFlow, pXslFilePath, pParams);
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXmlStrBuilder"></param>
        /// <param name="pXslFilePath"></param>
        /// <returns></returns>
        public static string XmlTransformInString(StringBuilder pXmlStrBuilder, string pXslFilePath)
        {
            XmlDocument xmlFile = new XmlDocument();
            xmlFile.LoadXml(pXmlStrBuilder.ToString().Trim());
            //
            return XmlTransformInString(xmlFile, pXslFilePath);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXmlFile"></param>
        /// <param name="pXslFilePath"></param>
        /// <returns></returns>
        public static string XmlTransformInString(XmlDocument pXmlFile, string pXslFilePath)
        {
            return XmlTransformInSb(pXmlFile, pXslFilePath).ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pType"></param>
        /// <param name="pFilePath"></param>
        /// <returns></returns>
        public static object XmlDeserialize(Type pType, string pFilePath)
        {
            XmlDocument xmlFlow = new XmlDocument();
            xmlFlow.Load(pFilePath);
            //
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(pType, xmlFlow.OuterXml);
            return CacheSerializer.Deserialize(serializeInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pType"></param>
        /// <param name="pDocument"></param>
        /// <param name="pFile"></param>
        public static void XmlSerialize(Type pType, object pDocument, string pFile)
        {
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(pType, pDocument)
            {
                IsWithoutNamespaces = true
            };
            CacheSerializer.Serialize(serializeInfo, pFile);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXmlSerializer"></param>
        /// <param name="pDocument"></param>
        /// <param name="pFile"></param>
        // EG 20180426 Analyse du code Correction [CA2202]
        public static void XmlSerialize(XmlSerializer pXmlSerializer, object pDocument, string pFile)
        {
            StreamWriter stream = new StreamWriter(pFile, false);
            using (XmlWriter writer = new XmlTextWriterFormattedNoDeclaration(stream))
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                pXmlSerializer.Serialize(writer, pDocument, ns);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXmlStrBuilder"></param>
        /// <param name="pFilePath"></param>
        public static void XmlSaveFile(StringBuilder pXmlStrBuilder, string pFilePath)
        {
            XmlDocument xmlFile = new XmlDocument();
            xmlFile.LoadXml(pXmlStrBuilder.ToString());
            xmlFile.Save(pFilePath);

        }

        #region XmlCDATASection
        // PM 20180219 [23824] Déplacé dans IOCommonTools
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="pData"></param>
        ///// <returns></returns>
        //public static XmlCDataSection SetInCDATA(string pData)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    return SetInCDATA(pData, doc);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="pData"></param>
        ///// <param name="pMainDocument"></param>
        ///// <returns></returns>
        //public static XmlCDataSection SetInCDATA(string pData, XmlDocument pMainDocument)
        //{
        //    XmlCDataSection ret = null;
        //    //
        //    try
        //    {
        //        // Load XML
        //        XmlDocument xmlData = new XmlDocument();
        //        xmlData.LoadXml(pData);


        //        // Remove XmlDeclaration Node
        //        XMLTools.RemoveXmlDeclaration(xmlData);

        //        // Set XML to CDataSection
        //        ret = pMainDocument.CreateCDataSection(xmlData.InnerXml);
        //    }
        //    catch (XmlException)
        //    {
        //        // On arrive ici si le LoadXml plante => ds ce cas la donnée n'est pas XML => pas de CDATA
        //    }
        //    //
        //    return ret;
        //}

        /// <summary>
        /// Enlever le CDATA des noeuds ValueXml
        /// </summary>
        /// <param name="pXmlNodeList"></param>
        public static void RemoveCDATAFromValueXml(XmlDocument pDoc, string pMasterElement, IOXmlOverrides pOverrides)
        {
            RemoveCDATAFromValueXml(pDoc, pMasterElement, pOverrides, pDoc.ChildNodes);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDoc"></param>
        /// <param name="pMasterElement"></param>
        /// <param name="pOverrides"></param>
        /// <param name="pXmlNodeList"></param>
        public static void RemoveCDATAFromValueXml(XmlDocument pDoc, string pMasterElement, IOXmlOverrides pOverrides, XmlNodeList pXmlNodeList)
        {
            if ((null != pXmlNodeList) && (pXmlNodeList.Count > 0))
            {
                // RD 20100727 / Correction de l'algorithme qui ne marche pas dans tous les cas                
                foreach (XmlNode xmlNode in pXmlNodeList)
                {
                    if (xmlNode.NodeType != XmlNodeType.ProcessingInstruction)
                    {
                        string filter;
                        // 1- Enlever le CDATA de tous les éléments ValueXML enfants 
                        // RD 20110307 [17339] Output: XML to PDF 
                        // Traiter tous les types qui commencent par xml ( exemple: xmlfo, ...)  
                        if (null != pOverrides)
                            filter = pOverrides.GetMemberNewAttribute(pMasterElement) + "[substring(@" + pOverrides.GetMemberNewAttribute("datatype") + ",1,1)='x']/" + pOverrides.GetMemberNewAttribute("valueXML");
                        else
                            filter = pMasterElement + "[substring(@datatype,1,3)='xml']/ValueXML";
                        //                        
                        XmlNodeList nodeValueXmlList = xmlNode.SelectNodes(filter);
                        //
                        if ((null != nodeValueXmlList) && (nodeValueXmlList.Count > 0))
                        {
                            foreach (XmlNode nodeValueXml in nodeValueXmlList)
                            {
                                XmlCDataSection nodeValueXml_CDATA = null;
                                foreach (XmlNode nodeValueXml_Node in nodeValueXml.ChildNodes)
                                {
                                    if (nodeValueXml_Node.NodeType == XmlNodeType.CDATA)
                                    {
                                        nodeValueXml_CDATA = (XmlCDataSection)nodeValueXml_Node;
                                        break;
                                    }
                                }
                                //
                                if (null != nodeValueXml_CDATA)
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(nodeValueXml_CDATA.Data);
                                    //
                                    foreach (XmlNode newNode in doc.ChildNodes)
                                        nodeValueXml.InsertBefore(pDoc.ImportNode(newNode, true), nodeValueXml_CDATA);
                                    //
                                    nodeValueXml.RemoveChild(nodeValueXml_CDATA);
                                }
                            }
                        }
                        //
                        // 2- Enlever le CDATA de tous les élément ValueXML enfants des autres éléments enfants
                        RemoveCDATAFromValueXml(pDoc, pMasterElement, pOverrides, xmlNode.ChildNodes);
                    }
                }
            }
        }

        /// <summary>
        /// Mettre les noeuds ValueXml dans un CDATA
        /// </summary>
        /// <param name="pDoc"></param>
        /// <param name="pMasterElement"></param>
        /// <param name="pOverrides"></param>
        /// RD 20110401 / [17379] Serialize mode : En mode LIGHT, utiliser les attributs de substitution
        public static void SetValueXmlInCDATA(XmlDocument pDoc, string pMasterElement, IOXmlOverrides pOverrides)
        {
            SetValueXmlInCDATA(pDoc, pMasterElement, pOverrides, pDoc.ChildNodes);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDoc"></param>
        /// <param name="pMasterElement"></param>
        /// <param name="pOverrides"></param>
        /// <param name="pXmlNodeList"></param>
        public static void SetValueXmlInCDATA(XmlDocument pDoc, string pMasterElement, IOXmlOverrides pOverrides, XmlNodeList pXmlNodeList)
        {
            if ((null != pXmlNodeList) && (pXmlNodeList.Count > 0))
            {
                // RD 20100727 / Correction de l'algorithme qui ne marche pas dans tous les cas
                foreach (XmlNode xmlNode in pXmlNodeList)
                {
                    // 1- Mettre dans un CDATA tous les élément ValueXML enfants 
                    if (xmlNode.NodeType != XmlNodeType.ProcessingInstruction)
                    {
                        string filter;
                        // RD 20110307 [17339] Output: XML to PDF 
                        // Traiter tous les types qui commencent par xml ( exemple: xmlfo, ...)
                        if (null != pOverrides)
                            filter = pOverrides.GetMemberNewAttribute(pMasterElement) + "[substring(@" + pOverrides.GetMemberNewAttribute("datatype") + ",1,1)='x']/" + pOverrides.GetMemberNewAttribute("valueXML");
                        else
                            filter = pMasterElement + "[substring(@datatype,1,3)='xml']/ValueXML";
                        //
                        XmlNodeList nodeValueXmlList = xmlNode.SelectNodes(filter);
                        // 
                        if ((null != nodeValueXmlList) && (nodeValueXmlList.Count > 0))
                        {
                            foreach (XmlNode nodeValueXml in nodeValueXmlList)
                            {
                                XmlNode nodeValueXml_Value = null;
                                foreach (XmlNode nodeValueXml_Node in nodeValueXml.ChildNodes)
                                {
                                    if (nodeValueXml_Node.NodeType == XmlNodeType.Element)
                                    {
                                        nodeValueXml_Value = nodeValueXml_Node;
                                        break;
                                    }
                                }
                                //
                                if (null != nodeValueXml_Value)
                                {
                                    XmlCDataSection xmlCdata = pDoc.CreateCDataSection(nodeValueXml_Value.OuterXml);
                                    nodeValueXml.InsertBefore(xmlCdata, nodeValueXml_Value);
                                    nodeValueXml.RemoveChild(nodeValueXml_Value);
                                }
                            }
                        }
                        //
                        // 2- Mettre dans un CDATA tous les élément ValueXML enfants des autres éléments enfants
                        SetValueXmlInCDATA(pDoc, pMasterElement, pOverrides, xmlNode.ChildNodes);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Valoriser tous les éléments SQL et tous les éléments SpheresLib dans un flux XML
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <param name="pDoc"></param>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// FI 20201221 [XXXXX] Add pSetErrorWarning
        public static void ValuateDynamicDataInXml(Task pXMLTask, XmlDocument pDoc, SetErrorWarning pSetErrorWarning)
        {
            try
            {
                string dynamicDataResult = string.Empty;
                //
                #region Valoriser tous les éléments SQL
                XmlNode nodeDynamicData = null;
                XmlNodeList nodeSQLList = pDoc.SelectNodes("//SQL");
                //
                while ((null != nodeSQLList) && (nodeSQLList.Count > 0))
                {
                    nodeDynamicData = nodeSQLList[0];

                    EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(DataSQL), nodeDynamicData.OuterXml);
                    DataSQL dataSQL = (DataSQL)CacheSerializer.Deserialize(serializeInfo);

                    IOMappedData ioData = new IOMappedData
                    {
                        sql = dataSQL
                    };

                    dynamicDataResult = string.Empty;
                    ProcessMapping.GetMappedDataValue(pXMLTask, pSetErrorWarning, ioData, null, ref dynamicDataResult);

                    XmlNode parentNode = nodeDynamicData.ParentNode;
                    parentNode.RemoveChild(nodeDynamicData);
                    parentNode.InnerText = dynamicDataResult;
                    //
                    // Pour ne pas revaloriser des éléments SQL enfants d'un élément "Param", lui même enfants d'un élément SQL déjà valorisé
                    nodeSQLList = pDoc.SelectNodes("//SQL");
                }
                #endregion

                #region Valoriser tous les éléments SpheresLib
                XmlNodeList nodeSpheresLibList = pDoc.SelectNodes("//SpheresLib");
                //
                while ((null != nodeSpheresLibList) && (nodeSpheresLibList.Count > 0))
                {
                    nodeDynamicData = nodeSpheresLibList[0];

                    EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(DataSpheresLib), nodeDynamicData.OuterXml);
                    DataSpheresLib dataSpheresLib = (DataSpheresLib)CacheSerializer.Deserialize(serializeInfo);

                    IOMappedData ioData = new IOMappedData
                    {
                        spheresLib = dataSpheresLib
                    };

                    if (dataSpheresLib.GetParam("FORMAT", out ParamData paramFormat))
                        ioData.dataformat = paramFormat.value;

                    dynamicDataResult = string.Empty;
                    ProcessMapping.GetMappedDataValue(pXMLTask, pSetErrorWarning, ioData, null, ref dynamicDataResult);

                    XmlNode parentNode = nodeDynamicData.ParentNode;
                    parentNode.RemoveChild(nodeDynamicData);
                    parentNode.InnerText = dynamicDataResult;
                    //
                    // Pour ne pas revaloriser des éléments SpheresLib enfants d'un élément "Param", lui même enfants d'un élément SpheresLib déjà valorisé
                    nodeSpheresLibList = pDoc.SelectNodes("//SpheresLib");
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(@"<b>Error to valuate dynamic data</b>", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pParentNode"></param>
        public static void XMLRemoveChildNodes(XmlNode pParentNode)
        {
            int countChildNodes = pParentNode.ChildNodes.Count;
            for (int indexChildNode = countChildNodes - 1; indexChildNode >= 0; indexChildNode--)
                pParentNode.RemoveChild(pParentNode.ChildNodes[indexChildNode]);
        }

        ///// <summary>
        ///// Alimente status pour la sérialisation
        ///// </summary>
        ///// <param name="pStatus"></param>
        ///// <param name="pStatusSpecified"></param>
        ///// <param name="pIsError"></param>
        ///// <param name="pIsLightSerializeMode"></param>
        //// PM 20180219 [23824] Déplacé dans IOCommonTools
        //public static void XMLSetStatus(ref string pStatus, ref bool pStatusSpecified, bool pIsError, bool pIsLightSerializeMode)
        //{
        //    if (false == pIsError)
        //    {
        //        // Pour le mode LIGHT ne pas mettre success
        //        if (pIsLightSerializeMode)
        //            pStatus = string.Empty;
        //        else
        //            pStatus = ProcessStateTools.StatusSuccess.ToLower();
        //    }
        //    else
        //        pStatus = ProcessStateTools.StatusError.ToLower();
        //    //
        //    // Pour le mode LIGHT, si success, ne pas sérialiser
        //    if (pIsLightSerializeMode)
        //        pStatusSpecified = StrFunc.IsFilled(pStatus);
        //    else
        //        pStatusSpecified = true;
        //}

        ///// <summary>
        ///// Alimente DataTypeSpecified
        ///// </summary>
        ///// <param name="pDataType"></param>
        ///// <param name="pDataTypeSpecified"></param>
        ///// <param name="pOverrides"></param>
        //// PM 20180219 [23824] Déplacé dans IOCommonTools
        //public static bool XMLIsDataTypeSpecified(string pDataType, bool pIsLightSerializeMode)
        //{
        //    if (pIsLightSerializeMode)
        //        // Pour le mode LIGHT, si string, ne pas sérialiser            
        //        return (StrFunc.IsFilled(pDataType) && (false == TypeData.IsTypeString(pDataType)));
        //    else
        //        return true;
        //}

        /// <summary>
        /// Alimente quelques données du document (DataType, status, ...) soit avec les valeurs de substitution qui seront utilisée dans le XSL de Transformation
        /// soit avec les valeurs Spheres pour la dé-sérialisation, après transformations XSL
        /// </summary>
        /// <param name="pDoc">Le Document XML</param>
        /// <param name="pOverrides">Les substitution</param>
        /// <param name="pIsToOverride">true: avant transformation XSL, false: après transformations XSL</param>        
        public static void XMLSetData(XmlDocument pDoc, IOXmlOverrides pOverrides, bool pIsToOverride)
        {
            if (null != pOverrides)
            {
                string dataTypeNewAttribute = pOverrides.GetMemberNewAttribute("datatype");
                string dataTypeFilter = "//" + pOverrides.GetMemberNewAttribute("row") + "//@" + dataTypeNewAttribute;
                //
                XmlNodeList dataTypeNodeList = pDoc.SelectNodes(dataTypeFilter);
                //
                if ((null != dataTypeNodeList) && (dataTypeNodeList.Count > 0))
                {
                    foreach (XmlNode dataTypeNode in dataTypeNodeList)
                    {
                        if (pIsToOverride)
                            dataTypeNode.Value = pOverrides.GetDataTypeOverride(dataTypeNode.Value).ToString();
                        else
                            dataTypeNode.Value = pOverrides.GetDataTypeFromOverride(dataTypeNode.Value).ToString();
                    }
                }
                //
                string statusNewAttribute = pOverrides.GetMemberNewAttribute("status");
                string statusFilter = "//" + pOverrides.GetMemberNewAttribute("row") + "//@" + statusNewAttribute;
                //
                XmlNodeList statusNodeList = pDoc.SelectNodes(statusFilter);
                //
                if ((null != statusNodeList) && (statusNodeList.Count > 0))
                {
                    foreach (XmlNode statusNode in statusNodeList)
                    {
                        if (pIsToOverride)
                            statusNode.Value = pOverrides.GetStatusOverride(statusNode.Value).ToString();
                        else
                            statusNode.Value = pOverrides.GetStatusFromOverride(statusNode.Value).ToString();
                    }
                }
            }
        }

        #endregion Xml Function

        #region Queue Functions


        /// <summary>
        /// Génère des messages
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pRow"></param>
        /// <param name="pTable"></param>
        /// <param name="pSqlOrder"></param>
        /// <param name="pActionId"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pLogMsg"></param>
        /// FI 20130426 [18344] Modification de la signature de la fonction  
        /// FI 20130426 [18344] Gestion de NormMsgFactoryMQueue
        public static void SendMQueue(Task pTask, IOTaskDetInOutFileRow pRow, IOTaskDetInOutFileRowTable pTable, SqlOrder pSqlOrder, IOCommonTools.SqlAction pActionId, IDbTransaction pDbTransaction, ArrayList pLogMsg)
        {
            IOTaskDetInOutFileRowTableMQueue tableMQueue = pTable.mQueue;

            if (tableMQueue != null)
            {
                ArrayList logMsg = new ArrayList(pLogMsg)
                {
                    $"Queue (name:{tableMQueue.name})"
                };
                //
                if (tableMQueue.name.ToUpper() == "QuotationHandlingMQueue".ToUpper())
                {
                    string queueAction = GetMQueueAction(tableMQueue.action, pActionId);
                    //
                    if (StrFunc.IsFilled(queueAction))
                    {
                        SendQuotationMQueue(pTask, pRow, pTable.name, queueAction, pTable.mQueue, pSqlOrder, pActionId,
                            pDbTransaction, logMsg);
                    }
                }
                else if (tableMQueue.name.ToUpper() == "NormMsgFactoryMQueue".ToUpper())
                {
                    SendNormMsgFactoryMQueue(pTask, pTable);
                }
                else
                {
                    throw new Exception(ArrFunc.GetStringList(logMsg, Cst.Space) + " not supported - No MOM message sended");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pRow"></param>
        /// <param name="pTableName"></param>
        /// <param name="pQueueAction"></param>
        /// <param name="pMQParamDataList"></param>
        /// <param name="pSqlOrder"></param>
        /// <param name="pAction"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pLogMsg"></param>
        /// FI 20170306 [22225] Modify
        private static void SendQuotationMQueue(Task pTask, IOTaskDetInOutFileRow pRow, string pTableName, string pQueueAction,
            IOParamDataList pMQParamDataList, SqlOrder pSqlOrder, IOCommonTools.SqlAction pAction, IDbTransaction pDbTransaction, ArrayList pLogMsg)
        {

            // RD 20110629
            // Gestion des Paramétres dans mQueue
            //
            // Valoriser la liste des parametres 
            pMQParamDataList.ValuateParamWithParameters(pTask, pRow, null, pDbTransaction, pLogMsg);

            Quote quote;
            string mqParamValue;
            object paramValue;
            if (pTableName.ToUpper() == Cst.OTCml_TBL.QUOTE_FXRATE_H.ToString())
                quote = new Quote_FxRate();
            else if (pTableName.ToUpper() == Cst.OTCml_TBL.QUOTE_RATEINDEX_H.ToString())
                quote = new Quote_RateIndex();
            else if (pTableName.ToUpper() == Cst.OTCml_TBL.QUOTE_SIMPLEIRS_H.ToString())
                quote = new Quote_SimpleIRSwap();
            // PM 20151027 [20964] Ajout gestion DERIVATIVEATTRIB
            else if ((pTableName.ToUpper() == Cst.OTCml_TBL.QUOTE_ETD_H.ToString()) ||
                (pTableName.ToUpper() == Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString()) ||
                (pTableName.ToUpper() == Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString()) ||
                (pTableName.ToUpper() == Cst.OTCml_TBL.ASSET_ETD.ToString()))
            {
                quote = new Quote_ETDAsset();
                // FI 20170306 [22225] use quote_ETDAsset
                Quote_ETDAsset quote_ETDAsset = (Quote_ETDAsset)quote;

                quote_ETDAsset.QuoteTable = pTableName;
                quote_ETDAsset.contractMultiplierSpecified = pMQParamDataList.GetParameterValue("CONTRACTMULTIPLIER", out mqParamValue);
                //
                if (quote_ETDAsset.contractMultiplierSpecified)
                    quote_ETDAsset.contractMultiplier = Convert.ToDecimal(mqParamValue);
                //
                if (pMQParamDataList.GetParameterValue("IDDC", out mqParamValue))
                    paramValue = mqParamValue;
                else
                    paramValue = pSqlOrder.GetDataParameterValue(pAction, "IDDC");
                //
                quote_ETDAsset.idDC = (ObjFunc.IsNull(paramValue) ? 0 : Convert.ToInt32(paramValue.ToString()));
                quote_ETDAsset.idDCSpecified = (((Quote_ETDAsset)quote).idDC > 0);
                //
                if (pMQParamDataList.GetParameterValue("IDDERIVATIVEATTRIB", out mqParamValue))
                {
                    paramValue = mqParamValue;
                }
                else
                {
                    paramValue = pSqlOrder.GetDataParameterValue(pAction, "IDDERIVATIVEATTRIB");
                }
                //
                quote_ETDAsset.idDerivativeAttrib = (ObjFunc.IsNull(paramValue) ? 0 : Convert.ToInt32(paramValue.ToString()));
                quote_ETDAsset.idDerivativeAttribSpecified = (((Quote_ETDAsset)quote).idDerivativeAttrib > 0);
                //
                quote_ETDAsset.isCashFlowsValSpecified = pMQParamDataList.GetParameterValue("IsCashFlowsVal", out mqParamValue);
                //
                if (quote_ETDAsset.isCashFlowsValSpecified)
                    quote_ETDAsset.isCashFlowsVal = Convert.ToBoolean(mqParamValue);

                // Si le contractMultiplier a changé au niveau du DC ou de L'ASSET
                // Et si le besoin de régénérer les événements de cashFlow ne se pose pas 
                // Alors pas la peine de poster le message au service SpheresQuotationHandling
                if (quote_ETDAsset.IsQuoteTable_ETD &&
                    ((false == quote_ETDAsset.isCashFlowsValSpecified) || (false == quote_ETDAsset.isCashFlowsVal)))
                {
                    return;
                }
            }
            else
                quote = new Quote_ToDefine();
            //
            quote.action = pQueueAction;
            //
            if (pMQParamDataList.GetParameterValue("IDASSET", out mqParamValue))
                paramValue = mqParamValue;
            else
                paramValue = pSqlOrder.GetDataParameterValue(pAction, "IDASSET");
            //
            quote.idAsset = (ObjFunc.IsNull(paramValue) ? 0 : Convert.ToInt32(paramValue.ToString()));
            //
            if (pMQParamDataList.GetParameterValue("TIME", out mqParamValue))
                paramValue = mqParamValue;
            else
                paramValue = pSqlOrder.GetDataParameterValue(pAction, "TIME");
            //
            quote.time = (ObjFunc.IsNull(paramValue) ? DateTime.MinValue : Convert.ToDateTime(paramValue.ToString()));
            //
            if ((null != quote as Quote_ETDAsset) && ((Quote_ETDAsset)quote).IsQuoteTable_ETD)
            {
                // Ici y'a pas de Quotation, le but est juste de régénérer les événements de cashFlow, 
                // suite à la modification dans les référentiels DERIVATIVECONTRACT et ASSET_ETD
                quote.valueSpecified = false;
                //
                ((Quote_ETDAsset)quote).timeSpecified = DtFunc.IsDateTimeFilled(quote.time);
            }
            else
            {
                if (pMQParamDataList.GetParameterValue("IDMARKETENV", out mqParamValue))
                    paramValue = mqParamValue;
                else
                    paramValue = pSqlOrder.GetDataParameterValue(pAction, "IDMARKETENV");
                //
                quote.idMarketEnv = (ObjFunc.IsNull(paramValue) ? string.Empty : paramValue.ToString());
                //
                if (pMQParamDataList.GetParameterValue("IDVALSCENARIO", out mqParamValue))
                    paramValue = mqParamValue;
                else
                    paramValue = pSqlOrder.GetDataParameterValue(pAction, "IDVALSCENARIO");
                //
                quote.idValScenario = (ObjFunc.IsNull(paramValue) ? string.Empty : paramValue.ToString());
                //
                if (pMQParamDataList.GetParameterValue("IDBC", out mqParamValue))
                    paramValue = mqParamValue;
                else
                    paramValue = pSqlOrder.GetDataParameterValue(pAction, "IDBC");
                //
                quote.idBC = (ObjFunc.IsNull(paramValue) ? string.Empty : paramValue.ToString());
                //
                if (pMQParamDataList.GetParameterValue("QUOTESIDE", out mqParamValue))
                    paramValue = mqParamValue;
                else
                    paramValue = pSqlOrder.GetDataParameterValue(pAction, "QUOTESIDE");
                //
                quote.quoteSide = (ObjFunc.IsNull(paramValue) ? string.Empty : paramValue.ToString());
                //
                if (pMQParamDataList.GetParameterValue("CASHFLOWTYPE", out mqParamValue))
                    paramValue = mqParamValue;
                else
                    paramValue = pSqlOrder.GetDataParameterValue(pAction, "CASHFLOWTYPE");
                //
                quote.cashFlowType = (ObjFunc.IsNull(paramValue) ? string.Empty : paramValue.ToString());
                //20101021 PL Add quoteTiming 
                if (pMQParamDataList.GetParameterValue("QUOTETIMING", out mqParamValue))
                    paramValue = mqParamValue;
                else
                    paramValue = pSqlOrder.GetDataParameterValue(pAction, "QUOTETIMING");
                //
                quote.quoteTiming = (ObjFunc.IsNull(paramValue) ? string.Empty : paramValue.ToString());
                //
                if (pMQParamDataList.GetParameterValue("VALUE", out mqParamValue))
                    paramValue = mqParamValue;
                else
                    paramValue = pSqlOrder.GetDataParameterValue(pAction, "VALUE");
                //
                quote.valueSpecified = true;
                quote.value = (ObjFunc.IsNull(paramValue) ? 0 : Convert.ToDecimal(paramValue.ToString()));
                //
                quote.SetIdQuote(pTask.Cs, pDbTransaction);
            }
            //
            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = pTask.Cs
            };

            QuotationHandlingMQueue qhMQueue = new QuotationHandlingMQueue(quote, mQueueAttributes);

            SendMQueue(pTask, qhMQueue, Cst.ProcessTypeEnum.QUOTHANDLING);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTableMQueueAction"></param>
        /// <param name="pActionId"></param>
        /// <returns></returns>
        public static string GetMQueueAction(string pTableMQueueAction, IOCommonTools.SqlAction pActionId)
        {

            if (StrFunc.IsEmpty(pTableMQueueAction))
            {
                switch (pActionId)
                {
                    case IOCommonTools.SqlAction.I:
                        return "Added";
                    case IOCommonTools.SqlAction.U:
                        return "Modified";
                    case IOCommonTools.SqlAction.D:
                        return "Deleted";
                    default:
                        return pActionId.ToString();
                }
            }
            else
            {
                switch (pTableMQueueAction.ToUpper().Trim())
                {
                    case "IUD":
                        if ((pActionId == IOCommonTools.SqlAction.I))
                            return "Added";
                        else if ((pActionId == IOCommonTools.SqlAction.U))
                            return "Modified";
                        else if ((pActionId == IOCommonTools.SqlAction.D))
                            return "Deleted";
                        //
                        break;
                    case "IU":
                        if ((pActionId == IOCommonTools.SqlAction.I))
                            return "Added";
                        else if ((pActionId == IOCommonTools.SqlAction.U))
                            return "Modified";
                        //
                        break;
                    case "I":
                        if ((pActionId == IOCommonTools.SqlAction.I))
                            return "Added";
                        //
                        break;
                    case "U":
                        if ((pActionId == IOCommonTools.SqlAction.U))
                            return "Modified";
                        //
                        break;
                    case "D":
                        if ((pActionId == IOCommonTools.SqlAction.D))
                            return "Deleted";
                        //
                        break;
                }
            }
            //
            return string.Empty;

        }



        /// <summary>
        /// Génère un message vers NormMsgFactory
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pTable"></param>
        /// FI 20130426 [18344] Add Method 
        private static void SendNormMsgFactoryMQueue(Task pTask, IOTaskDetInOutFileRowTable pTable)
        {
            if (false == ArrFunc.IsFilled(pTable.column))
                throw new Exception(StrFunc.AppendFormat("Table (name:{0}) doesn't contains columns"));

            IOTaskDetInOutFileRowTableColumn column = pTable.GetMqueueColumn();
            if (null == column)
                throw new Exception(StrFunc.AppendFormat("Table (name:{0}) doesn't contains a Column MQueue", pTable.name));

            if (false == TypeData.IsTypeXml(column.datatype))
                throw new Exception(StrFunc.AppendFormat("Column (name:{0}) is not an xml column", column.name));

            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(NormMsgFactoryMQueue), column.value);
            NormMsgFactoryMQueue queue = CacheSerializer.Deserialize(serializeInfo) as NormMsgFactoryMQueue;

            SendMQueue(pTask, queue, Cst.ProcessTypeEnum.NORMMSGFACTORY);
        }

        /// <summary>
        /// Envoie le message vers le service pour traitement
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pMqueue"></param>
        /// <param name="pService"></param>
        /// <param name="pProcessTypeEnum"></param>
        /// FI 20130426 [18344] add Method
        /// FI 20130917 [18953] La méthode est désormais public
        public static void SendMQueue(Task pTask, MQueueBase pMqueue, Cst.ProcessTypeEnum pProcessTypeEnum)
        {

            // PL 20130206 [18323] Pour utiliser le même ticket du tracker dans «Position Keeping »
            pMqueue.header.requesterSpecified = (null != pTask.Requester);
            pMqueue.header.requester = pTask.Requester;

            //PL 20101124 Refactoring 
            MQueueSendInfo mqSendInfo = EFS.SpheresService.ServiceTools.GetMqueueSendInfo(pProcessTypeEnum, pTask.Process.AppInstance);
            if ((null == mqSendInfo) || (!mqSendInfo.IsInfoValid))
                throw new Exception("Probably " + Cst.Process.GetService(pProcessTypeEnum) + " service is not yet installed");

            // RD 20121214 Increment 1 To column POSTEDSUBMSG (TRACKER)
            pTask.Process.Tracker.AddPostedSubMsg(1, pTask.Session);
            pTask.Process.Send_RetrieveOnError(pMqueue, mqSendInfo);//Default timeout: 60 sec.


        }



        #endregion Queue Functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDoc"></param>
        /// <returns></returns>
        public static byte[] ReadDocToBytes(XmlDocument pDoc)
        {
            Encoding fileEncoding = Encoding.Default;
            return fileEncoding.GetBytes(pDoc.OuterXml);
        }
    }
    #endregion class IOTools


}
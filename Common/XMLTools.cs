using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions; 
using System.Reflection;
using System.IO;

using System.Xml;
using System.Xml.Schema; 
using System.Xml.Xsl;  
using System.Xml.XPath;
using System.Xml.Serialization;


using System.Data;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

namespace EFS.Common
{

    

    /// <summary>
    /// Description résumée de XMLTools.
    /// </summary>
    public sealed class XMLTools
    {
        #region Constructor
        public XMLTools()
        {
        }
        #endregion Constructor

        /// <summary>
        /// Generic function to get an attribute from an XML object.
        /// </summary>
        /// <param name="pNode">Node of XML that describes object.</param>
        /// <param name="pAttribute">Name of attribute to find.</param>
        /// <returns>Returns string of value corresponding to attribute looked up.</returns>
        public static string GetNodeAttribute(XmlNode pNode, string pAttribute)
        {
            return GetNodeAttribute(pNode, pAttribute, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNode"></param>
        /// <param name="pAttribute"></param>
        /// <param name="pbTrim"></param>
        /// <returns></returns>
        public static string GetNodeAttribute(XmlNode pNode, string pAttribute, bool pbTrim)
        {
            string ret = string.Empty;
            //
            if ((null != pNode) && StrFunc.IsFilled(pAttribute) && (null != pNode.Attributes) && (null != pNode.Attributes.GetNamedItem(pAttribute)))
            {
                if (pbTrim)
                    ret = pNode.Attributes.GetNamedItem(pAttribute).InnerText.Trim();
                else
                    ret = pNode.Attributes.GetNamedItem(pAttribute).InnerText;
            }
            return ret;
        }

        /// <summary>
        /// Retourne true si l'attribut {pAttribute} existe sur le noeud {pNode}
        /// </summary>
        /// <param name="pNode"></param>
        /// <param name="pAttribute"></param>
        /// <returns></returns>
        public static bool ExistNodeAttribute(XmlNode pNode, string pAttribute)
        {
            return (null != pNode.Attributes.GetNamedItem(pAttribute));
        }

        /// <summary>
        /// Generic function to set an attribute from an XML object.
        /// </summary>
        /// <param name="pNode">Node of XML that describes object.</param>
        /// <param name="pAttribute">Name of attribute to find.</param>
        /// <param name="pValue">Value to set</param>
        /// <returns>Returns string of value corresponding to attribute looked up.</returns>
        public static bool SetNodeAttribute(XmlNode pNode, string pAttribute, string pValue)
        {
            bool ret = ExistNodeAttribute(pNode, pAttribute);
            if (ret)
                pNode.Attributes[pAttribute].InnerText = pValue;
            return ret;
        }

        /// <summary>
        ///  Supprime le noeud XmlDeclaration du document xml
        /// </summary>
        /// <param name="pXmlDoc"></param>
        public static void RemoveXmlDeclaration(XmlDocument pXmlDoc)
        {

            XmlNode xmlNode = pXmlDoc.FirstChild;
            //
            if (xmlNode.NodeType == XmlNodeType.XmlDeclaration)
                pXmlDoc.RemoveChild(xmlNode);
        }

        /// <summary>
        /// Mettre à jour la colonne {pTableName}.{pColumnName} avec le contenu du fichier {pXmlFilePath}
        /// <para>La ligne de la table à mettre à jour est identifiée par la clé:</para>
        /// <para>- composée des colonnes {pKeyColumns}</para>
        /// <para>- avec comme valeurs {pKeyValues}</para>
        /// <para>- de types {pKeyDatatypes}</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pXmlFilePath"></param>
        /// <param name="pTableName"></param>
        /// <param name="pColumnName"></param>
        /// <param name="pKeyColumns"></param>
        /// <param name="pKeyValues"></param>
        /// <param name="pKeyDatatypes"></param>
        public static void SaveXmlFileToXmlColumn(string pCS, string pXmlFilePath, string pTableName, string pColumnName,
            string[] pKeyColumns, string[] pKeyValues, string[] pKeyDatatypes, int pIdaUPD)
        {
            if (pTableName != string.Empty && pColumnName != string.Empty)
            {
                LOFile loFile = new LOFile();
                loFile.SetFileContent(FileTools.ReadFileToBytes(pXmlFilePath));
                LOFileColumn dbfc = new LOFileColumn(pCS, pTableName, pColumnName, pKeyColumns, pKeyValues, pKeyDatatypes);

                Cst.ErrLevel errLevel = dbfc.SaveFile(loFile, pIdaUPD, out Exception opException);
                if (errLevel != Cst.ErrLevel.SUCCESS)
                    throw (opException);
            }
        }

        /// <summary>
        /// Replace tag img on xslt File
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pXmlDocument"></param>
        /// <param name="pPhysicalPath"></param>
        /// <param name="pRelativePath"></param>
        /// <returns></returns>
        public static string ReplaceHtmlTagImage(string pCS, string pXmlDocument, string pPhysicalPath, string pRelativePath)
        {
            return ReplaceTagImage(pCS, pXmlDocument, "img", pPhysicalPath, pRelativePath);
        }

        /// <summary>
        /// Replace tag fo:external-graphic on xslt File
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pXmlDocument"></param>
        /// <param name="pPhysicalPath"></param>
        /// <returns></returns>
        public static string ReplaceXslFoTagImage(string pCS, string pXmlDocument, string pPhysicalPath)
        {
            return ReplaceTagImage(pCS, pXmlDocument, "fo:external-graphic", pPhysicalPath, string.Empty);
        }

        /// <summary>
        ///  <para>Replace tag fo:external-graphic on xslt File</para>
        ///  <para>Replace tag img on html File (xhtml File only, check xml conformity before)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pXmlDocument"></param>
        /// <param name="pTagType"></param>
        /// <param name="pPhysicalPath"></param>
        /// <param name="pRelativePath"></param>
        /// <returns></returns>
        // EG 20160404 Migration vs2013
        public static string ReplaceTagImage(string pCS, string pXmlDocument, string pTagType, string pPhysicalPath, string pRelativePath)
        {
            bool isLoadOk = true;
            string ret = pXmlDocument;
            //
            XmlDocument xmlDoc = null;
            #region Load XMLDocument
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(pXmlDocument);
            }
            catch (Exception) { isLoadOk = false; }
            #endregion
            //
            if (isLoadOk)
            {

                #region Replace tag fo:external-graphic or Replace tag img

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");
                nsmgr.AddNamespace("fo", "http://www.w3.org/1999/XSL/Format");
                string extensionFile;
                XmlNodeList nodeList;
                string url;
                switch (pTagType.ToLower())
                {
                    case "fo:external-graphic":
                        nodeList = xmlDoc.SelectNodes("//fo:external-graphic", nsmgr);
                        url = "url('<filePath>')";
                        extensionFile = "gif";
                        break;
                    case "fo:block":
                        nodeList = xmlDoc.SelectNodes("//fo:block", nsmgr);
                        url = "url('<filePath>')";
                        extensionFile = "gif";
                        break;

                    case "img":
                    default:
                        nodeList = xmlDoc.SelectNodes("//img");
                        url = "<filePath>";
                        extensionFile = "jpg";
                        break;
                }

                if (nodeList.Count > 0)
                {
                    for (int i = nodeList.Count - 1; i >= 0; i--)
                    {
                        XmlNode node = nodeList[i];
                        XmlAttribute xmlAttrib = node.Attributes["src"];

                        if (null == xmlAttrib)
                            xmlAttrib = node.Attributes["background-image"];

                        if (null != xmlAttrib)
                        {
                            if (StrFunc.ContainsIn(xmlAttrib.Value, "sql(select") && StrFunc.ContainsIn(xmlAttrib.Value, ")"))
                            {
                                IDataReader dr = null;
                                try
                                {
                                    string identifier = string.Empty;
                                    byte[] img = null;

                                    string query = xmlAttrib.Value.Replace("sql(", string.Empty);
                                    query = query.Substring(0, query.Length - 1);  // 1 = taille de ")"
                                    //
                                    //20070717 FI utilisation de ExecuteDataTable pour le cache
                                    // FI 20200520 [XXXXX] Add SQL cache
                                    dr = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(pCS), query).CreateDataReader();
                                    if (dr.Read())
                                    {
                                        identifier = Convert.ToString(dr.GetValue(0));
                                        if (Convert.DBNull != dr.GetValue(1))
                                            img = (byte[])(dr.GetValue(1));
                                    }
                                    //
                                    if (StrFunc.IsFilled(identifier) && ArrFunc.IsFilled(img))
                                    {
                                        string physicalPath = pPhysicalPath;
                                        if (false == physicalPath.EndsWith(@"\"))
                                            physicalPath += @"\";
                                        //
                                        string filePath = physicalPath + identifier + "." + extensionFile;
                                        try
                                        {
                                            FileTools.WriteBytesToFile(img, filePath, FileTools.WriteFileOverrideMode.Override);
                                        }
                                        catch (Exception)
                                        {
                                            // Il y peut y avoir erreur si le fichier est déjà utilisé par un autre process, dans ce cas on continue 
                                        }
                                        //
                                        if ("img" == pTagType.ToLower())
                                        {
                                            string relativePath = pRelativePath;
                                            if (false == pRelativePath.EndsWith(@"/"))
                                                relativePath += @"/";
                                            filePath = relativePath + identifier + "." + extensionFile;
                                        }
                                        //
                                        xmlAttrib.Value = url.Replace("<filePath>", filePath);
                                    }
                                    else
                                    {
                                        // EG 20160404 Migration vs2013
                                        //node.RemoveAll();
                                        node.ParentNode.RemoveChild(node);
                                    }
                                }
                                catch (Exception) { }
                                finally
                                {
                                    if (null != dr)
                                        dr.Close();
                                }
                            }
                        }
                    }
                    //
                }
                #endregion
                //
                ret = xmlDoc.InnerXml;
            }
            //
            return ret;
        }

        /// <summary>
        /// Retourne une valeur valide d'ID par le schéma du W3C pour l'id {pId}
        /// <para>Exemple ajoute id comme prefix si {pId} représente un numérique</para>
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public static string GetXmlId(string pId)
        {

            // RD 20131209 [19315][19304] Ajout du test sur pId
            // FI 20131220 [19378] usage de StrFunc.IsFilled
            if (StrFunc.IsFilled(pId))
            {
                //Ajout du prefix "id" si commence par un numérique.
                Regex regex = new Regex(@"^\d+[\W\w]*$");
                if (regex.IsMatch(pId))
                    pId = "id" + pId;

                //ticket 17786 gestion +
                pId = pId.Replace(@"+", "..PLUS..");

                //
                //20080819 PL Suppression des "/", caractère interdit dans un ID par le schéma du W3C 
                pId = pId.Replace(@"/", string.Empty);
            }

            return pId;
        }

        /// <summary>
        /// Returns a valid XPath statement to use for searching attribute values regardless of 's or "s
        /// </summary>
        /// <param name="attributeValue">Attribute value to parse</param>
        /// <returns>Parsed attribute value in concat() if needed</returns>
        // RD 20150526 [20614] Add
        public static string EncaseXpathString(string pInput)
        {
            // If we don't have any " then encase string in "
            if (!pInput.Contains("\""))
                return String.Format("\"{0}\"", pInput);

            // If we have some " but no ' then encase in '
            if (!pInput.Contains("'"))
                return String.Format("'{0}'", pInput);

            // If we get here we have both " and ' in the string so must use Concat
            StringBuilder sb = new StringBuilder("concat(");

            // Going to look for " as they are LESS likely than ' in our data so will minimise
            // number of arguments to concat.
            int lastPos = 0;
            int nextPos = pInput.IndexOf("\"");
            while (nextPos != -1)
            {
                // If this is not the first time through the loop then seperate arguments with ,
                if (lastPos != 0)
                    sb.Append(",");

                sb.AppendFormat("\"{0}\",'\"'", pInput.Substring(lastPos, nextPos - lastPos));
                lastPos = ++nextPos;

                // Find next occurance
                nextPos = pInput.IndexOf("\"", lastPos);
            }

            sb.Append(")");
            return sb.ToString();
        }

    }

    /// <summary>
    /// A generic XSL Transformation Class 
    /// </summary>
    public sealed class XSLTTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// FI 20191007 [XXXXX] Add
        private static readonly object m_xslLock = new object();
        /// <summary>
        /// 
        /// </summary>
        /// FI 20191007 [XXXXX] Add
        private static Dictionary<string, XslCompiledTransform> dicXslCompiledTransform;

        /// <summary>
        /// Retourne le résultat de la transformation XSLT d'un flux XML
        /// </summary>
        /// <param name="xmlInput">Flux xml</param>
        /// <param name="xsltPath">URI de la feuille de style</param>
        /// <returns></returns>
        public static string TransformXml(StringBuilder xmlInput, string xsltPath)
        {
            return TransformXml(xmlInput, xsltPath, null, null);
        }
        /// <summary>
        /// Retourne le résultat de la transformation XSLT d'un flux XML
        /// </summary>
        /// <param name="xmlInput">Flux xml</param>
        /// <param name="xsltPath">URI de la feuille de style</param>
        /// <param name="xsltParams">Liste des arguments</param>
        /// <param name="xsltObjects">Valeurs des arguments</param>
        /// <returns></returns>
        public static string TransformXml(StringBuilder xmlInput, string xsltPath, Hashtable xsltParams, Hashtable xsltObjects)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlInput.ToString());
            return TransformXml(xmldoc, xsltPath, xsltParams, xsltObjects);
        }

        /// <summary>
        /// Retourne le résultat de la transformation XSLT d'un fichier XML
        /// </summary>
        /// <param name="xmlPath">URI de fichier xml</param>
        /// <param name="xsltPath">URI de la feuille de style</param>
        /// <param name="xsltParams">Liste des arguments</param>
        /// <param name="xsltObjects">Valeurs des arguments</param>
        /// <returns></returns>
        public static string TransformXml(string xmlPath, string xsltPath)
        {
            return TransformXml(xmlPath, xsltPath, null, null);
        }
        /// <summary>
        /// Retourne le résultat de la transformation XSLT d'un fichier XML
        /// </summary>
        /// <param name="xmlPath">URI de fichier xml</param>
        /// <param name="xsltPath">URI de la feuille de style</param>
        /// <param name="xsltParams">Liste des arguments</param>
        /// <param name="xsltObjects">Valeurs des arguments</param>
        /// <returns></returns>
        public static string TransformXml(string xmlPath, string xsltPath, Hashtable xsltParams, Hashtable xsltObjects)
        {
            XPathDocument doc = new XPathDocument(xmlPath);
            //20071212 FI Ticket 16012 => Migration Asp2.0
            return TransformXml(doc, xsltPath, xsltParams, xsltObjects);
        }
        /// <summary>
        /// Retourne le résultat de la transformation XSLT d'un flux XML
        /// </summary>
        /// <param name="doc">Flux xml en entrée</param>
        /// <param name="xsltPath">URI de al feuille de style</param>
        /// <param name="xsltParams">Liste des arguments</param>
        /// <param name="xsltObjects">Valeurs des arguments</param>
        /// <returns></returns>
        /// FI 20200608 [XXXXX] using syntax
        public static string TransformXml(IXPathNavigable doc, string xsltPath, Hashtable xsltParams, Hashtable xsltObjects)
        {
            //20071212 FI Ticket 16012 => Migration Asp2.0  : utilisation de la class XslCompiledTransform 
            
            if (StrFunc.IsEmpty(xsltPath))
                throw new ArgumentException("xslt path is empty");

            // FI 20191007 Utilisation d'un dictionnaire pour augmenter les performances  dicXslCompiledTransform
            // FI 20200525 [XXXXX] Call GetXslCompiledTransform
            XslCompiledTransform xslDoc = GetXslCompiledTransform<string>(xsltPath, xsltPath);

            //Fill XsltArgumentList if necessary
            XsltArgumentList args = new XsltArgumentList();
            if (ArrFunc.IsFilled(xsltParams))
            {
                IDictionaryEnumerator pEnumerator = xsltParams.GetEnumerator();
                while (pEnumerator.MoveNext())
                    args.AddParam(pEnumerator.Key.ToString(), "", pEnumerator.Value);
            }
            //
            if (ArrFunc.IsFilled(xsltObjects))
            {
                IDictionaryEnumerator pEnumerator = xsltObjects.GetEnumerator();
                while (pEnumerator.MoveNext())
                    args.AddExtensionObject(pEnumerator.Key.ToString(), pEnumerator.Value);
            }
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                xslDoc.Transform(doc, args, sw);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlInput"></param>
        /// <returns></returns>
        public static string RemoveXmlnsAlias(StringBuilder xmlInput)
        {
            return RemoveXmlnsAlias(xmlInput, new UnicodeEncoding());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlInput"></param>
        /// <param name="pEncoding"></param>
        /// <returns></returns>
        public static string RemoveXmlnsAlias(StringBuilder xmlInput, Encoding pEncoding)
        {

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlInput.ToString());
            return RemoveXmlnsAlias(xmldoc, pEncoding);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public static string RemoveXmlnsAlias(string xmlPath)
        {
            return RemoveXmlnsAlias(xmlPath, new UnicodeEncoding());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <param name="pEncoding"></param>
        /// <returns></returns>
        public static string RemoveXmlnsAlias(string xmlPath, Encoding pEncoding)
        {
            XPathDocument doc = new XPathDocument(xmlPath);
            return RemoveXmlnsAlias(doc, pEncoding);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="pEncoding"></param>
        /// <returns></returns>
        /// FI 20200608 [XXXXX] using syntax
        private static string RemoveXmlnsAlias(IXPathNavigable doc, Encoding pEncoding)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            string name = assemblyName + "." + "Library.RemXmlnsAlias.xslt";

            XmlDocument xsltdoc = new XmlDocument();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                string xslt = string.Empty;
                if (null == stream)
                    throw new InvalidProgramException($"Resource {name} not found");

                using (StreamReader reader = new StreamReader(stream))
                {
                    xslt = reader.ReadToEnd();
                    reader.Close();
                }
                xsltdoc.LoadXml(xslt);
            }
            // FI 20200525 [XXXXX] Call GetXslCompiledTransform
            //XsltSettings settings = new XsltSettings(true, true);
            //XslCompiledTransform xslDoc = new XslCompiledTransform();
            //xslDoc.Load(xsltdoc, settings, new XmlUrlResolver());
            StringBuilder sb = new StringBuilder();
            using (StringWriterWithEncoding sw = new StringWriterWithEncoding(sb, pEncoding))
            {
                XslCompiledTransform xslDoc = GetXslCompiledTransform<XmlDocument>(name, xsltdoc);
                xslDoc.Transform(doc, null, sw);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Retourne le type de flux résultat de la transformation xsl (pdf, txt, xml, html)
        /// <para>Chaque type de flux est identifié par l'extension de fichier communément utilisé</para>
        /// <para>Retourne String.empty lorsque le xslt ne donne aucune indication</para>
        /// </summary>
        /// <param name="xsltFile">Url du fichier xslt</param>
        /// <param name="pDefaultValue">Valeur par défaut</param>
        /// <returns></returns>
        public static string GetOutputStreamType(string xsltFile)
        {
            return GetOutputStreamType(xsltFile, string.Empty);
        }
        /// <summary>
        /// Retourne le type de flux résultat de la transformation xsl (pdf, txt, xml, html)
        /// <para>Chaque type de flux est identifié par l'extension de fichier communément utilisé</para>
        /// <para>Retourne la valeur par défaut si le xsl ne donne pas d'indication</para>
        /// </summary>
        /// <param name="xsltFile">Url du fichier xslt</param>
        /// <param name="pDefaultValue">Valeur par défaut</param>
        /// <returns></returns>
        public static string GetOutputStreamType(string xsltFile, string pDefaultValue)
        {
            XmlTextReader xsltReader = null;
            try
            {
                string ret = pDefaultValue;
                xsltReader = new XmlTextReader(xsltFile);
                //
                if (xsltReader.ReadToFollowing("xsl:stylesheet"))
                {
                    if (StrFunc.IsFilled(xsltReader.GetAttribute("xmlns:fo")))
                        ret = "pdf";
                    else if (xsltReader.ReadToFollowing("xsl:output"))
                        ret = GetXslOutputStreamType(xsltFile, pDefaultValue);
                }
                return ret;
            }
            finally
            {
                if (null != xsltReader)
                    xsltReader.Close();
            }
        }

        /// <summary>
        /// Charge, compile la feuille de style xslt et détecte la méthode spécifiée dans le tag xsl:output
        /// <para>Si la méthode ne donne pas d'indication,(par ex le tag n'est pas présent), alors Spheres® retourne la valeur par défaut</para>
        /// </summary>
        /// <param name="xsltFile">URI de la feuille de style xslt</param>
        /// <param name="pDefaultValue">Valeur par défaut</param>
        /// <returns></returns>
        public static string GetXslOutputStreamType(string xsltFile, string pDefaultValue)
        {
            XmlOutputMethod xslOutputMethod = GetXslOutputMethod(xsltFile);
            return XmlOutputMethodToStreamType(xslOutputMethod, pDefaultValue);
        }

        /// <summary>
        /// Charge, compile la feuille de style xslt et retourne la méthode spécifiée dans le tag xsl:output
        /// </summary>
        /// <param name="xsltFile">URI de la feuille de style</param>
        /// <returns></returns>
        public static XmlOutputMethod GetXslOutputMethod(string xsltFile)
        {
            // FI 20200525 [XXXXX] Call GetXslCompiledTransform
            //XslCompiledTransform xslt = new XslCompiledTransform();
            //xslt.Load(xsltFile);
            XslCompiledTransform xslt = GetXslCompiledTransform<string>(xsltFile, xsltFile);
            return xslt.OutputSettings.OutputMethod;
        }

        /// <summary>
        /// Traduit l'enum XmlOutputMethod en une valeur qui représente le type de flux de sortie
        /// <para>Chaque type de flux est identifié par l'extension de fichier communément utilisé (retourne donc xml,txt,html)</para>
        /// <para>Retourne la valeur par défaut si XmlOutputMethod</para>
        /// </summary>
        /// <param name="pMethod"></param>
        /// <param name="pDefaultValue"></param>
        /// <returns></returns>
        private static string XmlOutputMethodToStreamType(XmlOutputMethod pMethod, string pDefaultValue)
        {

            string ret;

            switch (pMethod)
            {
                case XmlOutputMethod.Html:
                case XmlOutputMethod.Xml:
                    ret = pMethod.ToString().ToLower();
                    break;
                case XmlOutputMethod.Text:
                    ret = "txt";
                    break;
                case XmlOutputMethod.AutoDetect:
                    ret = pDefaultValue;
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pMethod.ToString()));
            }
            return ret;
        }

        /// <summary>
        /// Recherche la transormation XSL compilée dans un cache. Si non présent la transormation XSL est chargée et ajoutée dans le cache
        /// <para>S</para>
        /// </summary>
        /// <param name="pKey">clé d'accès au dictionnaire</param>
        /// <param name="pStylesheet">T de type String (URI) ou IXPathNavigable (XMLDocument) </param>
        /// <returns></returns>
        // FI 20200525 [XXXXX] Add Method
        private static XslCompiledTransform GetXslCompiledTransform<T>(string pKey, T pStylesheet)
        {

            XslCompiledTransform xslDoc;

            if (null == dicXslCompiledTransform)
                dicXslCompiledTransform = new Dictionary<string, XslCompiledTransform>();

            if (dicXslCompiledTransform.ContainsKey(pKey))
            {
                xslDoc = dicXslCompiledTransform[pKey];
            }
            else
            {
                XsltSettings settings = new XsltSettings(true, true);

                xslDoc = new XslCompiledTransform();
                if (typeof(T).GetInterface(typeof(IXPathNavigable).ToString()) != null)
                    xslDoc.Load((IXPathNavigable)pStylesheet, settings, new XmlUrlResolver());
                else if (typeof(T).Equals(typeof(String)))
                    xslDoc.Load(pStylesheet.ToString(), settings, new XmlUrlResolver());
                else
                    throw new NotSupportedException(StrFunc.AppendFormat("Type :{0} is not supported", typeof(T).ToString()));

                lock (m_xslLock)
                {
                    dicXslCompiledTransform.Add(pKey, xslDoc);
                }
            }
            return xslDoc;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class XSDValidation
    {
        #region members
        private ArrayList _validationResults;
        private XmlReaderSettings _settings;
        #endregion

        #region accessor
        /// <summary>
        /// Représente les paramètres de lecture du fichier XML
        /// </summary>
        public XmlReaderSettings Settings
        {
            get { return _settings; }
        }
        #endregion

        #region constructor
        public XSDValidation(string[] pSchema)
        {
            SetDefaultSettings(pSchema);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Ecriture du flux XML dans un fichier
        /// Les tags incohérents vis à vis de l'XSD sont commentés avec le message d'erreur
        /// </summary>
        /// <remarks>Attention les tags du flux en entrée doivent être séparés par des \r\n</remarks> 
        /// <param name="pXmlFlow">Flux XML en entrée</param>
        /// <param name="pFile">Fichier destination</param>
        public void WriteFile(string pXmlFlow, string pFile)
        {
            //
            _validationResults = new ArrayList();
            //

            using (XmlReader reader = XmlReader.Create(new StringReader(pXmlFlow), Settings))
            {
                while (reader.Read())
                {
                }
                
                Regex regex = new Regex(@"\r\n");
                string[] xmlLines = regex.Split(pXmlFlow);
                if (0 < _validationResults.Count)
                {
                    string mainMessage = "<!-- ********************************************* -->";
                    mainMessage += Environment.NewLine + "<!-- THIS DOCUMENT IS NOT A XML FILE IN CONFORMITY -->";
                    mainMessage += Environment.NewLine + "<!-- WITH THE SPECIFIED SCHEMAS DEFINITIONS        -->";
                    mainMessage += Environment.NewLine + "<!-- ********************************************* -->";
                    xmlLines[0] = xmlLines[0] + mainMessage + Environment.NewLine;
                    for (int i = _validationResults.Count - 1; i >= 0; i--)
                    {
                        ValidationEventArgs args = (ValidationEventArgs)_validationResults[i];
                        xmlLines[args.Exception.LineNumber - 1] = "<!--" + args.Message + "-->" + xmlLines[args.Exception.LineNumber - 1];
                        xmlLines[args.Exception.LineNumber - 1] = "<!--" + args.Severity.ToString() + "-->" + xmlLines[args.Exception.LineNumber - 1];
                    }
                }
                
                using (StreamWriter writer = new StreamWriter(pFile, false, Encoding.UTF8))
                {
                    for (int i = 0; i < xmlLines.Length; i++)
                        writer.WriteLine(xmlLines[i]);
                }
            }
        }

        /// <summary>
        /// Validation XSD d'un flux XML
        /// </summary>
        /// <param name="pXmlFlow">Flux xml en entrée</param>
        /// <exception cref="XSDValidationException en cas de non conformité"></exception>
        public void CheckConformity(string pXmlFlow)
        {
            XmlReader reader = null;
            //
            _validationResults = new ArrayList();
            //
            try
            {
                reader = XmlReader.Create(new StringReader(pXmlFlow), Settings);
                while (reader.Read())
                {
                }
                //
                if (0 < _validationResults.Count)
                {
                    string msg = string.Empty;
                    for (int i = 0; i < _validationResults.Count; i++)
                    {
                        ValidationEventArgs validationArg = (ValidationEventArgs)_validationResults[i];
                        string validationArgMsg = StrFunc.AppendFormat("{0} [line number {1},line position {2}]", validationArg.Message, validationArg.Exception.LineNumber, validationArg.Exception.LinePosition);
                        msg += validationArgMsg + Cst.CrLf;
                    }
                    throw new XSDValidationException(msg);
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != reader)
                    reader.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ValidationHandler(object sender, ValidationEventArgs args)
        {
            _validationResults.Add(args);
        }

        /// <summary>
        /// Inititialise les paramètres de lecture du fichier XML (class XmlReaderSettings)
        /// </summary>
        /// <param name="pSchema">Repésente les schema pour la validation</param>
        private void SetDefaultSettings(string[] pSchema)
        {

            if (ArrFunc.IsEmpty(pSchema))
                throw new ArgumentException("Argument pSheme is Empty");
            //
            _settings = new XmlReaderSettings();
            Settings.IgnoreComments = true;
            Settings.IgnoreProcessingInstructions = true;
            Settings.IgnoreWhitespace = false;
            Settings.ValidationType = ValidationType.Schema;
            Settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            // Add schema
            foreach (string schema in pSchema)
            {
                if (StrFunc.IsFilled(schema))
                    Settings.Schemas.Add(null, schema);
            }
            //Add Event
            Settings.ValidationEventHandler += new ValidationEventHandler(ValidationHandler);

        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    // EG 20180425 Analyse du code Correction [CA2237]
    [Serializable]
    public class XSDValidationException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public XSDValidationException()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMessage"></param>
        public XSDValidationException(string pMessage)
            : base(pMessage)
        {
        }

    }

}

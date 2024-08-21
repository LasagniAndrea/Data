using System;
using System.Collections.Specialized;
using System.Xml;
using System.IO;

namespace EFS.Common
{
    /// <summary>
    /// Description résumée de Config.
    /// </summary>
    public class Config
    {
        #region Members
        private XmlDocument xmlDocument;
        private bool isFileOpened;
        private string fileName;
        private readonly string root;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        public Config(string pFileName)
        {
            isFileOpened = Open(pFileName);
            root = "appSettings";
        }
        #endregion Constructors
        #region Methods
        #region Close
        public void Close()
        {
            isFileOpened = false;
            xmlDocument = null;
            fileName = string.Empty;
        }
        #endregion Close
        #region GetConfig
        public StringDictionary GetConfig()
        {
            return GetConfig(root);
        }
        // EG 20180425 Correction path SelectNodes + No catch
        public StringDictionary GetConfig(string pSection)
        {
            StringDictionary settings = new StringDictionary();
            try
            {
                if (isFileOpened)
                {
                    XmlNode xmlRoot = OpenSection(pSection);
                    if (null != xmlRoot)
                    {
                        // Lecture de la valeur
                        XmlNodeList xmlNodeList = xmlRoot.SelectNodes("descendant::add");
                        foreach (XmlNode item in xmlNodeList)
                            settings.Add(item.Attributes.GetNamedItem("key").Value, item.Attributes.GetNamedItem("value").Value);
                    }
                }
            }
            catch (Exception) {}
            return settings;
        }
        #endregion GetConfig
        #region Open
        // EG 20180423 Analyse du code Correction [CA2202]
        public bool Open(string pFileName)
        {
            StreamWriter streamWriter = null;
            try
            {
                if (false == File.Exists(pFileName))
                {
                    FileStream fileStream = new FileStream(pFileName, FileMode.OpenOrCreate);
                    streamWriter = new StreamWriter(fileStream);
                    streamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                    streamWriter.WriteLine("<configuration>");
                    streamWriter.WriteLine("  <appSettings>");
                    streamWriter.WriteLine("  </appSettings>");
                    streamWriter.WriteLine("</configuration>");
                }
                xmlDocument = new XmlDocument();
                xmlDocument.Load(pFileName);
                fileName = pFileName;
            }
            catch (Exception) { return false; }
            finally
            {
                //if (null != fileStream)
                //    fileStream.Close();
                if (null != streamWriter)
                    streamWriter.Close();
            }
            return true;
        }
        #endregion Open
        #region OpenSection
        private XmlNode OpenSection(string pSection)
        {
            XmlNode xmlNode = null;
            XmlNode xmlRoot;
            try
            {
                xmlNode = xmlDocument.DocumentElement.SelectSingleNode("/configuration/" + pSection);
                if (null == xmlNode)
                {
                    xmlRoot = xmlDocument.DocumentElement.SelectSingleNode("/configuration");
                    xmlNode = xmlDocument.CreateNode(XmlNodeType.Element, pSection, String.Empty);
                    xmlRoot.AppendChild(xmlNode);
                    xmlDocument.Save(fileName);
                }
            }
            catch { }
            return xmlNode;
        }
        #endregion OpenSection
        #region ReadSetting
        public string ReadSetting(string pKey)
        {
            return ReadSetting(root, pKey);
        }
        public string ReadSetting(string pSection, string pKey)
        {
            try
            {
                if (isFileOpened)
                {
                    XmlNode xmlRoot = OpenSection(pSection);
                    if (null != xmlRoot)
                    {
                        XmlNode xmlNode = xmlRoot.SelectSingleNode("add[@key=\"" + pKey + "\"]");
                        if (null != xmlNode)
                            return xmlNode.Attributes.GetNamedItem("value").Value;
                    }
                }
            }
            catch (Exception) { return null; }
            return null;
        }
        #endregion ReadSetting
        #region SaveSetting
        public bool SaveSetting(string pKey, string pValue)
        {
            return SaveSetting(root, pKey, pValue);
        }
        public bool SaveSetting(string pSection, string pKey, string pValue)
        {
            try
            {
                if (isFileOpened)
                {
                    XmlNode xmlRoot = OpenSection(pSection);
                    if (null != xmlRoot)
                    {
                        XmlNode xmlNode = xmlRoot.SelectSingleNode("add[@key=\"" + pKey + "\"]");
                        if (null == xmlNode)
                        {
                            xmlNode = xmlDocument.CreateNode(XmlNodeType.Element, "add", string.Empty);
                            XmlNode xmlKey = xmlDocument.CreateNode(XmlNodeType.Attribute, "key", string.Empty);
                            xmlKey.Value = pKey;
                            XmlNode xmlValue = xmlDocument.CreateNode(XmlNodeType.Attribute, "value", string.Empty);
                            xmlValue.Value = pValue;
                            xmlNode.Attributes.SetNamedItem(xmlKey);
                            xmlNode.Attributes.SetNamedItem(xmlValue);
                            xmlRoot.AppendChild(xmlNode);
                        }
                        xmlNode.Attributes.GetNamedItem("value").Value = pValue;
                        xmlDocument.Save(fileName);
                    }
                }
            }
            catch (Exception) { return false; }
            return true;
        }
        #endregion SaveSetting
        #endregion Methods
    }
}

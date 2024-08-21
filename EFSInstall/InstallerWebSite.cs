using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;
//
using System.Security.Permissions;


namespace EFSInstall
{
    /// <summary>
    /// Description résumée de Installer.
    /// </summary>
    [RunInstaller(true)]
    
    public class InstallerWebSite : Installer
    {
        #region Members
        private System.ComponentModel.Container components = null;
        //
        private const string SoftwareOTCmL = "OTCml";
        private const string SoftwareVision = "Vision";
        private const string SoftwarePortal = "EFS";
        //
        private const string KeySoftware = "Software";
        private const string KeyUserName = "_UserName";
        private const string KeyPwd = "_Pwd";
        private const string KeyIsPwdCrypted = "_IsPwdCrypted";
        private const string KeyRdbmsName = "_RdbmsName";
        private const string KeyServerName = "_ServerName";
        private const string KeyDatabaseName = "_DatabaseName";
        private const string KeySourceDisplay = "_SourceDisplay";
        private const string KeyHash = "_Hash";
        private const string KeyPDFGenerator = "_PDFGenerator";
        private const string KeyPDFGeneratorTimeout = "_PDFGeneratorTimeout";
        //
        private const string DirectoryDeployTemp = "DeployTemp";
        private const string DirectoryTemporary = "Temporary";
        #endregion

        #region Constructor
        public InstallerWebSite()
        {
            // Cet appel est requis par le concepteur.
            InitializeComponent();

            // TODO : ajoutez les initialisations après l'appel à InitializeComponent
        }
        #endregion

        #region Methods
        #region Gestion des Fichiers et des Répertoires

        private void CreateDirectory(DirectoryInfo pPathDirectory)
        {
            if (pPathDirectory.Exists)
            {
                // Supprimer le répertoire et le recréer
                pPathDirectory.Delete(true);
                pPathDirectory.Refresh();
            }

            // Créer le répertoire destination
            pPathDirectory.Create();
            pPathDirectory.Refresh();

            if (!pPathDirectory.Exists)
            {
                throw new InstallException("Error to create Directory: " + pPathDirectory.FullName);
            }
        }

        private void CopyDirectory(string pPathSource, string pPathDest)
        {
            CopyDirectory(pPathSource, pPathDest, false);
        }
        private void CopyDirectory(string pPathSource, string pPathDest, bool pDelSource)
        {
            // Copier tous les fichiers
            foreach (string file in Directory.GetFiles(pPathSource))
            {
                FileInfo fileInfo = new FileInfo(file);

                // Copier le fichier 
                File.Copy(file, pPathDest + "\\" + fileInfo.Name, true);

                // Supprimer le fichier source
                if (pDelSource)
                    fileInfo.Delete();
            };

            // Copier tous les sous répertoires
            foreach (string subDirectory in Directory.GetDirectories(pPathSource))
            {
                DirectoryInfo subDirectoryInfo = new DirectoryInfo(subDirectory);

                // Créer le répertoire destination
                DirectoryInfo destDirectoryInfo = new DirectoryInfo(pPathDest + "\\" + subDirectoryInfo.Name);
                CreateDirectory(destDirectoryInfo);

                // Copier le sous répertoire
                CopyDirectory(subDirectory, destDirectoryInfo.FullName, pDelSource);
            };

            // Supprimer le répertoire source
            if (pDelSource)
                DeleteDirectory(pPathSource);
        }

        private void DeleteDirectory(string pPathDirectory)
        {
            // Supprimer le répertoire 
            DirectoryInfo tempPathInfo = new DirectoryInfo(pPathDirectory);
            if (tempPathInfo.Exists)
                tempPathInfo.Delete(true);
        }

        private void DeleteFile(string pPathFile)
        {
            FileInfo fileInfo = new FileInfo(pPathFile);
            if (fileInfo.Exists)
                fileInfo.Delete();
        }

        private void SetWriteRigtDirectory(string pPathDirectory)
        {
            DirectoryInfo tempPathInfo = new DirectoryInfo(pPathDirectory);

            if (tempPathInfo.Exists)
            {
                _ = new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, tempPathInfo.FullName);
            }
        }
        #endregion

        

        #region public override Install
        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);
            //
            try
            {
                string tempPath = string.Empty;
                string sSoftware = Context.Parameters["Software"];
                //
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileInfo assemblyFileInfo = new FileInfo(assembly.Location);
                FileInfo webConfigFileInfo = new FileInfo(assemblyFileInfo.Directory + @"\Web.config");
                //
                #region Upgrade Web.config file
                // There is No WebConfig File with PdfGenerator
                if (webConfigFileInfo.Exists)
                {
                    try
                    {
                        #region Get the parameter passed across in the CustomActionData.
                        string sUserName = Context.Parameters["UserName"];
                        string sPwd = Context.Parameters["Pwd"];
                        string sRdbmsName = Context.Parameters["RdbmsName"];
                        string sServerName = Context.Parameters["ServerName"];
                        string sDatabaseName = Context.Parameters["DatabaseName"];                        
                        //
                        string sTimeOut = Context.Parameters["TimeOut"];
                        string sSMTPServer = Context.Parameters["SMTPServer"];
                        string sSMTPSender = Context.Parameters["SMTPSender"];
                        string sMOMType = Context.Parameters["MOMType"];
                        string sMOMPath = Context.Parameters["MOMPath"];
                        string sPDFGeneratorPathDest = this.Context.Parameters["PDFGeneratorPath"];
                        // 20070703 RD 
                        //-------------------
                        // Ici le chemin \\ServerName\Directory\FileName passé en parametre est transformé 
                        // automatiquement en \ServerName\Directory\FileName
                        // Ajouter un backslash pour : \ServerName\Directory\FileName
                        //-------------------
                        if (sMOMPath.StartsWith(@"\"))
                            sMOMPath = @"\" + sMOMPath;
                        //
                        if (sPDFGeneratorPathDest.StartsWith(@"\"))
                            sPDFGeneratorPathDest = @"\" + sPDFGeneratorPathDest;
                        //-------------------
                        string sPDFGeneratorTimeoutValue = "300";
                        if (sPDFGeneratorPathDest != null && sPDFGeneratorPathDest.Trim().Length > 0 && !sPDFGeneratorPathDest.Trim().EndsWith("\\"))
                            sPDFGeneratorPathDest = sPDFGeneratorPathDest.Trim() + @"\";
                        #endregion
                        //
                        bool isExistPDFGeneratorPathNode = false;
                        bool isExistPDFGeneratorTimeoutNode = false;
                        string sKeyValue = string.Empty;

                        // Load the config file into the XML DOM.
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.Load(webConfigFileInfo.FullName);

                        #region configuration/appSettings
                        XmlNode rootNode = xmlDocument.SelectSingleNode("configuration/appSettings");

                        // Finds the right node and change it to the new value.            
                        foreach (XmlNode node in rootNode.SelectNodes("add"))
                        {
                            sKeyValue = node.Attributes.GetNamedItem("key").Value;
                            //
                            if (sKeyValue == KeySoftware)
                            {
                                string webConfigSoftware = node.Attributes.GetNamedItem("value").Value;
                                if (false == (webConfigSoftware == sSoftware))
                                    throw new InstallException("Error to upgrade: Database" + sDatabaseName + "is not a " + webConfigSoftware + " Database");
                            }
                            //
                            if (sKeyValue.EndsWith(KeyUserName))
                            {
                                node.Attributes.GetNamedItem("key").Value = sSoftware + KeyUserName;
                                node.Attributes.GetNamedItem("value").Value = sUserName;
                            }
                            //
                            if (sKeyValue.EndsWith(KeyPwd))
                            {
                                node.Attributes.GetNamedItem("key").Value = sSoftware + KeyPwd;
                                node.Attributes.GetNamedItem("value").Value = sPwd;
                            }

                            if (sKeyValue.EndsWith(KeyIsPwdCrypted))
                            {
                                node.Attributes.GetNamedItem("key").Value = sSoftware + KeyIsPwdCrypted;
                                node.Attributes.GetNamedItem("value").Value = "0";
                            }

                            if (sKeyValue.EndsWith(KeyRdbmsName))
                            {
                                node.Attributes.GetNamedItem("key").Value = sSoftware + KeyRdbmsName;
                                node.Attributes.GetNamedItem("value").Value = sRdbmsName;
                            }

                            if (sKeyValue.EndsWith(KeyServerName))
                            {
                                node.Attributes.GetNamedItem("key").Value = sSoftware + KeyServerName;
                                node.Attributes.GetNamedItem("value").Value = sServerName;
                            }

                            if (sKeyValue.EndsWith(KeyDatabaseName))
                            {
                                node.Attributes.GetNamedItem("key").Value = sSoftware + KeyDatabaseName;
                                node.Attributes.GetNamedItem("value").Value = sDatabaseName;
                            }

                            if (sKeyValue.EndsWith(KeySourceDisplay))
                            {
                                node.Attributes.GetNamedItem("key").Value = sSoftware + KeySourceDisplay;
                                if (sSoftware.ToUpper() == SoftwarePortal.ToUpper())
                                    node.Attributes.GetNamedItem("value").Value = "HIDE";
                                else
                                    node.Attributes.GetNamedItem("value").Value = "DISABLED";
                            }

                            if (sKeyValue.EndsWith(KeyHash))
                            {
                                node.Attributes.GetNamedItem("key").Value = sSoftware + KeyHash;
                                if (sSoftware.ToUpper() == SoftwarePortal.ToUpper())
                                    node.Attributes.GetNamedItem("value").Value = "None";
                                else
                                    node.Attributes.GetNamedItem("value").Value = "MD5";
                            }
                            //
                            if (sSoftware.ToUpper() == SoftwareVision.ToUpper())
                            {
                                if (sKeyValue.EndsWith(KeyPDFGenerator))
                                {
                                    node.Attributes.GetNamedItem("key").Value = sSoftware + KeyPDFGenerator;
                                    node.Attributes.GetNamedItem("value").Value = sPDFGeneratorPathDest;
                                    //
                                    isExistPDFGeneratorPathNode = true;
                                }
                                //
                                if (sKeyValue.EndsWith(KeyPDFGeneratorTimeout))
                                {
                                    node.Attributes.GetNamedItem("key").Value = sSoftware + KeyPDFGeneratorTimeout;
                                    node.Attributes.GetNamedItem("value").Value = sPDFGeneratorTimeoutValue;
                                    //
                                    isExistPDFGeneratorTimeoutNode = true;
                                }
                            }

                            if (node.Attributes.GetNamedItem("key").Value == "Spheres_ErrorEmailSmtpServer")
                                node.Attributes.GetNamedItem("value").Value = sSMTPServer;

                            if (node.Attributes.GetNamedItem("key").Value == "Spheres_ErrorEmailSender")
                                node.Attributes.GetNamedItem("value").Value = sSMTPSender;

                            if (node.Attributes.GetNamedItem("key").Value == "MOMType")
                            {
                                if (sMOMType == "MQSeries")
                                    node.Attributes.GetNamedItem("value").Value = "MSMQ";
                                else
                                    node.Attributes.GetNamedItem("value").Value = sMOMType;
                            }

                            if (node.Attributes.GetNamedItem("key").Value == "MOMPath")
                                node.Attributes.GetNamedItem("value").Value = sMOMPath;
                        }

                        // Insérer le noeud s'il n'existe pas dans Web.config
                        if (sSoftware.ToUpper() == SoftwareVision.ToUpper())
                        {
                            XmlNode newNode = null;
                            XmlAttribute Key = null;
                            XmlAttribute Value = null;

                            if (isExistPDFGeneratorPathNode == false)
                            {
                                newNode = xmlDocument.CreateNode(XmlNodeType.Element, "add", String.Empty);
                                Key = xmlDocument.CreateAttribute("key", String.Empty);
                                Value = xmlDocument.CreateAttribute("value", String.Empty);
                                //
                                Key.Value = sSoftware + KeyPDFGenerator;
                                Value.Value = sPDFGeneratorPathDest;
                                //
                                newNode.Attributes.Append(Key);
                                newNode.Attributes.Append(Value);
                                //
                                rootNode.AppendChild(newNode);
                            }
                            //
                            if (isExistPDFGeneratorTimeoutNode == false)
                            {
                                newNode = xmlDocument.CreateNode(XmlNodeType.Element, "add", String.Empty);
                                Key = xmlDocument.CreateAttribute("key", String.Empty);
                                Value = xmlDocument.CreateAttribute("value", String.Empty);
                                //
                                Key.Value = sSoftware + KeyPDFGeneratorTimeout;
                                Value.Value = sPDFGeneratorTimeoutValue;
                                //
                                newNode.Attributes.Append(Key);
                                newNode.Attributes.Append(Value);
                                //
                                rootNode.AppendChild(newNode);
                            }
                        }
                        #endregion
                        #region configuration/system.web/authentication/forms
                        rootNode = xmlDocument.SelectSingleNode("configuration/system.web/authentication/forms");
                        rootNode.Attributes.GetNamedItem("timeout").Value = sTimeOut;
                        #endregion
                        #region configuration/system.web/sessionState
                        rootNode = xmlDocument.SelectSingleNode("configuration/system.web/sessionState");
                        rootNode.Attributes.GetNamedItem("timeout").Value = sTimeOut;
                        #endregion

                        // Write out the new config file.
                        xmlDocument.Save(webConfigFileInfo.FullName);
                    }
                    catch (Exception ex)
                    {
                        throw new InstallException("Error to upgrade: " + webConfigFileInfo.FullName + " - " + ex.Message);
                    }
                }
                    #endregion
                //
                #region Set permission to ASP user
                try
                {
                    tempPath = assemblyFileInfo.Directory + "\\" + DirectoryTemporary + "\\";
                    SetWriteRigtDirectory(tempPath);
                }
                catch (Exception ex) { throw new InstallException("Error to set permission to ASP user - " + ex.Message); }
                #endregion
                //
                #region Deleting DeployTemp directory
                try
                {
                    tempPath = assemblyFileInfo.Directory + "\\" + DirectoryDeployTemp + "\\";
                    DeleteDirectory(tempPath);
                }
                catch (Exception ex) { throw new InstallException("Error to delete DeployTemp directory - " + ex.Message); }
                #endregion
            }
            catch (InstallException ex) { throw new InstallException("Error to install Spheres application - " + ex.Message); }
            catch (Exception ex) { throw new InstallException("Error to install Spheres application - " + ex.Message); }
        }
        #endregion

        #region  protected override Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Code généré par le Concepteur de composants
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion
        #endregion
    }
}

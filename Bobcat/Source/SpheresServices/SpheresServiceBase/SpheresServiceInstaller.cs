using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using System.Linq; 

using EFS.ACommon;
using EFS.Common;
using EFS.SpheresService;   
using EFS.SpheresServiceParameters;

namespace EFS.ServiceInstall
{
    /// <summary>
    /// Description résumée de EventInstaller.
    /// </summary>
    // EG 20100106 Add Assembly Attribute reader
    // EG 20180423 Analyse du code Correction [CA1405]
    [RunInstaller(true)]
    [ComVisible(false)]
    public abstract class SpheresServiceInstaller : Installer
    {
        #region Members
        /// <summary>
        ///This call is required by the Designer. 
        /// </summary>
        protected ServiceInstaller serviceInstaller;
        /// <summary>
        /// 
        /// </summary>
        protected ServiceProcessInstaller processInstaller;
        /// <summary>
        /// 
        /// </summary>
        protected Container components = null;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Représente le type de service 
        /// </summary>
        protected Cst.ServiceEnum ServiceEnum
        {
            set;
            get;
        }

        /// <summary>
        /// <para>Représente le nom par défaut du service lors d'une installation classique (sans instance associée) </para>
        /// <para>Représente le prefix commun à l'ensemble des instances d'un service lors d'une installation (avec instance associée)</para>
        /// </summary>
        protected string ServicePrefix
        {
            private set;
            get;
        }

        /// <summary>
        /// Obtient le nom de l'instance associée à l'installation
        /// <para>L'instance est spécifié dans le parameter INSTANCE</para>
        /// </summary>
        /// <returns></returns>
        protected string InstanceName
        {
            get
            {
                String ret = string.Empty;

                if (StrFunc.IsFilled(GetContextParameter("INSTANCE")))
                    ret = GetContextParameter("INSTANCE");

                return ret;
            }
        }

        /// <summary>
        /// Obtient le nom de l'instance installée 
        /// <para>(i.e. Obtient {instanceName} lorqu'une instance est spécifiée lors de l'installation), cas des gateways</para>
        /// <para>(i.e. Obtient "unnamed" lorsqu'aucune instance est spécifiée lors de l'installation, cas le plus courant)</para>
        /// </summary>
        protected string InstanceNameInSetupConfig
        {
            get
            {
                string ret = this.InstanceName;
                if (StrFunc.IsEmpty(ret))
                    ret = "unnamed";
                return ret;
            }
        }

        /// <summary>
        /// Représente les paramétrages présents dans le fichier de configuration SpheresInstallConfig
        /// </summary>
        protected Dictionary<string, object> InstanceParameters
        {
            get;
            private set;
        }

        /// <summary>
        ///  Active/désactive le mode silencieux lors de l'installation 
        /// </summary>
        protected Boolean IsSilentMode
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le fichier SpheresSetupConfig
        /// </summary>
        /// FI 20170824 [XXXXX] Add property
        protected string FileConfig
        {
            get;
            private set;
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceEnum"></param>
        /// <param name="pTypeInstaller"></param>
        /// FI 20131016 [] instanciation de la classe Software
        /// FI 20131106 [19139] Usage du compte LocalSystem par défaut
        public SpheresServiceInstaller(Cst.ServiceEnum pServiceEnum, Type pTypeInstaller)
        {
            //FI 20131016, Software est une classe utilisée pour l'interprétation des mots clés de MOMPATH 
            // voir method TranslateMOMPath
            new Software(Software.SOFTWARE_Spheres);

            ServiceEnum = pServiceEnum;
            ServicePrefix = SpheresServiceBase.ConstructServiceName(ServiceEnum, null);
            // 
            // processInstaller
            // 
            processInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem,
                Password = null,
                Username = null
            };
            // 
            // serviceInstaller
            // 
            serviceInstaller = new ServiceInstaller
            {
                StartType = ServiceStartMode.Automatic,
                ServiceName = ServicePrefix
            };
            serviceInstaller.DisplayName = SpheresServiceBase.ConstructServiceDisplayName(serviceInstaller.ServiceName);
            serviceInstaller.Description = BuildDescription(serviceInstaller.ServiceName, pTypeInstaller);

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string GetContextParameter(string key)
        {
            string ret = String.Empty;
            if (Context.Parameters.ContainsKey(key))
                ret = Context.Parameters[key].ToString();
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != processInstaller)
                    processInstaller.Dispose();

                if (null != serviceInstaller)
                    serviceInstaller.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedState"></param>
        protected override void OnBeforeInstall(IDictionary savedState)
        {
            base.OnBeforeInstall(savedState);

            //# if DEBUG
            //            System.Diagnostics.Debugger.Launch();
            //# endif

            // FI Appel des méthodes GetServiceName et GetDisplayName 
            // Car ServiceName et DisplayName sont fonction instanceName
            if (StrFunc.IsFilled(InstanceName))
            {
                serviceInstaller.ServiceName = SpheresServiceBase.ConstructServiceName(ServiceEnum, InstanceName);
                serviceInstaller.DisplayName = SpheresServiceBase.ConstructServiceDisplayName(serviceInstaller.ServiceName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateServer"></param>
        ///FI 20131106 [19139] refactoring base.Install est appelée après SetServiceUserFromContext
        ///FI 20161020 [XXXXX] Modify
        public override void Install(IDictionary stateServer)
        {
# if DEBUG
            System.Diagnostics.Debugger.Launch();
# endif
            try
            {
                // 20100622 MF - Adding additional installation arguments for the "multiple instances" functionality
                this.Context.Parameters.Add(ServiceKeyEnum.Prefix.ToString(), this.ServicePrefix);
                this.Context.Parameters.Add(ServiceKeyEnum.ServiceAccount.ToString(), processInstaller.Account.ToString());
                this.Context.Parameters.Add(ServiceKeyEnum.ServiceUserName.ToString(), processInstaller.Username);
                this.Context.Parameters.Add(ServiceKeyEnum.ServicePassword.ToString(), processInstaller.Password);

                //FI 20131010 [19041] 
                LoadInstanceParameters();

                //FI 20131011 [19041] call ReplaceContext
                ReplaceContext();

                //FI 20131106 [19139] Call of SetServiceUserFromContext
                SetServiceUserFromContext();

                //FI 20210517 [XXXXX] Add Try catch
                Boolean isServiceAlreadyExists = false;
                try
                {
                    //FI 20131106 [19139] base.Install est appelé ici
                    base.Install(stateServer);
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    if (e.NativeErrorCode == 1073) // ERROR_SERVICE_EXISTS (1073) The specified service already exists.
                    {
                        // FI 20210517 aucune action si le service existe déjà
                        // Cette exception peut se produire lorsque le msi est exécuté en mode "repair" (c-à-d lorsque le product est déjà installé) 
                        isServiceAlreadyExists = true;
                    }
                    else
                        throw;
                }

                if (false == isServiceAlreadyExists)
                {
                    //FI 20121220 [] add méthode MFFunction que je ne comprends pas
                    MFFunction();

                    SaveInRegistry();

                    // PM 20200102 [XXXXX] New Log: Ajoute une dépendance du service au service SpherresLogger
                    AddLoggerReference();

                    //FI 20131010 [19041] call SaveConfig
                    SaveConfig();

                    //FI 20131008 [19041]
                    CreateInstance();

                    //FI 20161020 [XXXXX] Mise à jour des fichiers script START_SPHERESSERVICES et STOP_SPHERESSERVICES (fichiers bat et ps1)
                    ServiceInfo serviceInfo = new ServiceInfo(serviceInstaller.ServiceName);
                    string version = serviceInfo.GetVersion();
                    string path = serviceInfo.GetInformationsKey(ServiceKeyEnum.PathInstall);
                    // PM 20201119 [XXXXX] Démarrage de SpheresLogger en premier et arret en dernier
                    //SpheresServiceParameters.SpheresServiceParametersHelper.StartStopToolsAdd(path, version, this.servicePrefix);
                    SpheresServiceParameters.SpheresServiceParametersHelper.StartStopToolsAdd(path, version);
                }
            }
            catch (Exception ex)
            {
                string errMsgTitle = "Error to install service";
                string errMsgService = "Service : {SERVICE} \x0A\x0A" +
                        "Installation error is occured. Services will not run correctly." + "\x0A" +
                        "Read carefully message below and re-install the services." + "\x0A\x0A";

                MessageBox.Show(errMsgService.Replace("{SERVICE}", this.serviceInstaller.ServiceName) +
                    ex.Message, errMsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);

                throw;
            }
        }

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif

            base.OnBeforeUninstall(savedState);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateServer"></param>
        /// FI 20161020 [XXXXX] Modify
        public override void Uninstall(IDictionary stateServer)
        {
# if DEBUG
            System.Diagnostics.Debugger.Launch();
# endif
            try
            {
                base.Uninstall(stateServer);

                /* FI 20210518 [XXXXX] Mise en commentaire usage de Context.Parameters["assemblypath"]
                // FI 20161020 [XXXXX]  suppression des fichiers START_SPHERESSERVICES et STOP_SPHERESSERVICES
                ServiceInfo serviceInfo = new ServiceInfo(serviceInstaller.ServiceName);
                string path = serviceInfo.GetInformationsKey(ServiceKeyEnum.PathInstall);
                SpheresServiceParameters.SpheresServiceParametersHelper.StartStopToolsDel(path);
                */

                //FI 20210518 [XXXXX] Utilisation de assemblypath pour récupérer le path
                //plus fialble que serviceInfo.GetInformationsKey(ServiceKeyEnum.PathInstall) si installation avec instance (possible avec les gateways)
                // 
                if (StrFunc.IsFilled(Context.Parameters["assemblypath"]))
                {
                    string path = Path.GetDirectoryName(this.Context.Parameters["assemblypath"]);
                    SpheresServiceParameters.SpheresServiceParametersHelper.StartStopToolsDel(path);
                }

                // Ligne suivante pour debugger la désinstallation (EG 20080130)
                //System.Diagnostics.Debugger.Launch();
                // 20100622 MF - New uninstallation procedure
                SpheresServiceParameters.SpheresServiceParametersHelper.DeleteAllInstances(this.ServicePrefix);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Service : " + this.serviceInstaller.ServiceName + "\x0A\x0A" +
                    "Uninstallation error is occured. Services is not correctly uninstalled." + "\x0A\x0A" +
                    ex.Message, "Error to uninstall service", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUserName"></param>
        /// <param name="pPwd"></param>
        /// <param name="pRdbmsName"></param>
        /// <param name="pServerName"></param>
        /// <param name="pDatabaseName"></param>
        /// <returns></returns>
        ///  MF 20121105 - moved from the GateBCSInstaller class in order to re-use in SpheresServiceInstaller subclasses
        protected static string GetConnectionString(string pUserName, string pPwd, string pRdbmsName, string pServerName, string pDatabaseName)
        {
            string ret = string.Empty;

            if (null != pRdbmsName)
            {
                if (pRdbmsName.ToUpper() == "ORACL10G")
                    ret = "Data Source=SID();User ID=SCHEMA();Password=PWD();";
                else if (pRdbmsName.ToUpper() == "SQLSRV2K8")
                    ret = "Data Source=SERVERNAME();Initial Catalog=DBNAME();Persist Security Info=False;User Id=USERNAME();Password=PWD();Workstation Id=127.0.0.1;Packet Size=4096";

                if (StrFunc.IsFilled(ret))
                {
                    if (null != pServerName)
                    {
                        if (pServerName.Trim().Length > 0)
                        {
                            ret = ret.Replace("SID()", pServerName);
                            ret = ret.Replace("SERVERNAME()", pServerName);
                        }
                    }
                    if (null != pDatabaseName)
                    {
                        if (pDatabaseName.Trim().Length > 0)
                        {
                            ret = ret.Replace("DBNAME()", pDatabaseName);
                            ret = ret.Replace("SCHEMA()", pDatabaseName);
                        }
                    }
                    ret = ret.Replace("USERNAME()", pUserName);
                    ret = ret.Replace("PWD()", pPwd);
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <returns></returns>
        protected static string BuildDescription(string pServiceName, Type pTypeInstaller)
        {
            string ret = pServiceName;
            if (null != pTypeInstaller)
            {
                Assembly assy = pTypeInstaller.Assembly;
                foreach (Attribute attr in Attribute.GetCustomAttributes(assy))
                {
                    if (attr.GetType() == typeof(AssemblyDescriptionAttribute))
                    {
                        ret = ((AssemblyDescriptionAttribute)attr).Description;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Enregistrement dans la registry du service 
        /// <para>Ajout des informations liées à l'installation (MOMTYPE,MONPATH,etc..)</para>
        /// </summary>
        protected void SaveInRegistry()
        {
            ServiceTools.SetRegistryServiceInformations(ServiceEnum, this.serviceInstaller, this.Context.Parameters);
            RegistryTools.AddServiceNameToImagePath(this.serviceInstaller.ServiceName);
            RegistryTools.SetEventLogService(this.serviceInstaller.ServiceName);
        }

        /// <summary>
        /// Function incompréhensible
        /// </summary>
        /// FI 20121220 []
        private void MFFunction()
        {
            // Existence check for spheres context parameters 
            if (!this.Context.Parameters.ContainsKey(ServiceKeyEnum.MOMPath.ToString()))
            {

                ServiceInfo serviceInfo = new ServiceInfo(serviceInstaller.ServiceName);

                string defaultsourcepath = @"REGKEY_HKLM;SOFTWARE\EFS\CommonServiceParameters";

                // FI GLOP GATE => ClassType est nécessaire
                // Call the modal configuration of the service to request the user 
                // to input the necessary lacking parameters.
                Dictionary<string, object> parameters =
                    SpheresServiceParametersHelper.CallModalConfiguration(
                    serviceInfo,
                    //this.Context.Parameters[ServiceKeyEnum.ClassType.ToString()],
                    SpheresServiceParametersHelper.ConfigurationMode.Update,
                    // the registry key where we stock the default spheres parameters; 
                    // (feel free to modify this value if you have to search in another location (registry, file, or database))
                    defaultsourcepath,
                    out bool foundDefaultValues);

                if (ArrFunc.IsFilled(parameters))
                {
                    // adding the user inputed parameters to the context collection
                    foreach (System.Collections.Generic.KeyValuePair<string, object> pair in parameters)
                    {
                        if (this.Context.Parameters.ContainsKey(pair.Key))
                            this.Context.Parameters.Remove(pair.Key);
                        this.Context.Parameters.Add(pair.Key, Convert.ToString(pair.Value));
                    }
                }
                // when no default configuraton parameter is found we set the context elmnts, 
                // which the user has just inputed, as the default values
                if (!foundDefaultValues)

                    SpheresServiceParametersHelper.WriteDefaulInstallationValues(
                    ServiceEnum,
                    serviceInstaller,
                    this.Context.Parameters,
                    defaultsourcepath
                    );

            }
        }

        /// <summary>
        /// Création des instances du service sur la base du fichier de configuration SpheresInstallConfig
        /// <para>En cas de plantage un message indique l'erreur et propose à l'utilisateur d'installer les instances manuellement</para>
        /// </summary>
        /// FI 20131008 [19041] installation des instances paramétrées dans le fichier de configuration de l'installation 
        /// FI 20131106 [19139] Test de la valeur retour de SpheresServiceParametersHelper.CreateServiceFromService
        /// FI 20170824 [XXXXX] Modify (si isSilentMode =>  affichage du message d'erreur généré par sc.exe si erreur il y a (voir varaiable newInstanceNOk)) 
        private void CreateInstance()
        {
            try
            {
                // FI 20170824 [XXXXX] Usage List<DictionaryEntry>
                List<DictionaryEntry> newInstance = new List<DictionaryEntry>();

                if (ArrFunc.IsFilled(InstanceParameters))
                {
                    foreach (string key in InstanceParameters.Keys)
                    {
                        if (key != InstanceNameInSetupConfig)
                        {
                            Dictionary<string, object> serviceParameters = (Dictionary<string, object>)InstanceParameters[key];

                            int retCreateServiceFromService = SpheresServiceParametersHelper.CreateServiceFromService(this.serviceInstaller.ServiceName, serviceParameters, 
                                out string standardOutput);
                            newInstance.Add(new DictionaryEntry(key, retCreateServiceFromService));
                        }
                    }
                }

                if (false == IsSilentMode)
                {
                    string msgTitle = "Install service";
                    // FI 20170824 [XXXXX] Linq for newInstanceOk 
                    List<string> newInstanceOk = (from item in newInstance.Where(x => (int)x.Value == 0)
                                                  select (string)item.Key).ToList();

                    if (ArrFunc.IsFilled(newInstanceOk))
                    {
                        string msgService = StrFunc.AppendFormat("Service : {0} \x0A\x0AThe following instances have been installed.\x0A", this.serviceInstaller.ServiceName);
                        foreach (string instance in newInstanceOk)
                            msgService += StrFunc.AppendFormat(" - {0} \x0A", instance);

                        MessageBox.Show(msgService, msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // FI 20170824 [XXXXX] Linq for newInstanceNOk 
                    List<DictionaryEntry> newInstanceNOk = (from item in newInstance.Where(x => (int)x.Value != 0)
                                                            select item).ToList();
                    if (ArrFunc.IsFilled(newInstanceNOk))
                    {
                        string msgService = StrFunc.AppendFormat("Service : {0} \x0A\x0AThe following instances have not been installed.\x0A", this.serviceInstaller.ServiceName);
                        foreach (DictionaryEntry instance in newInstanceNOk)
                        {
                            if ((int)instance.Value == -1)
                                msgService += StrFunc.AppendFormat(" - {0}. Instance already exists\x0A", instance.Value);
                            else
                                msgService += StrFunc.AppendFormat(" - {0}. sc.exe create failed with exit code : {1}\x0A", instance.Key, instance.Value);
                        }

                        msgService += StrFunc.AppendFormat("\x0A\x0APlease check your entries in {0} and try again.", FileConfig);

                        MessageBox.Show(msgService, msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
            catch (Exception ex)
            {
                string errMsgTitle = "Error to install service instance";
                string errMsgService = "Service : {SERVICE} \x0A\x0A" +
                        "Installation error is occured." + "\x0A" +
                        "Reinstall the instances manually." + "\x0A\x0A";

                MessageBox.Show(errMsgService.Replace("{SERVICE}", this.serviceInstaller.ServiceName) +
                    ex.Message, errMsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Retourne les instances paramétrées et leurs paramétrages respectifs présents dans le fichier de configuration de setup
        /// </summary>
        /// <param name="pFileName">Fichier de configuration</param>
        /// <param name="pService">Type de service</param>
        /// <param name="pInstanceNameInSetupConfig">Nom de l'instance installée dans le fichier de configuration du setup</param>
        /// <returns></returns>
        /// FI 20131008 [19041] add method
        /// FI 20131018 []  call MOMSettings.TranslateMOMPath
        /// FI 20131107 [19142] Prise en considération du tag defaultSettings
        private static Dictionary<string, object> GetInstanceParameters(string pFileName, string pService, string pInstanceNameInSetupConfig)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            // EG 20160404 Migration vs2013
            //XmlDataDocument doc = new XmlDataDocument();
            XmlDocument doc = new XmlDocument();
            doc.Load(pFileName);

            XmlNode nodeService = doc.SelectSingleNode(StrFunc.AppendFormat("setup/{0}", pService));
            XmlNode nodeDefault = doc.SelectSingleNode("setup/defaultSettings");

            if (null != nodeService)
            {
                XmlNodeList instanceNodeList = nodeService.SelectNodes("instance");

                foreach (XmlNode nodeItem in instanceNodeList)
                {
                    Dictionary<string, object> parameters = new Dictionary<string, object>();

                    //Chargement des paramètres présents dans un noeud settings
                    XmlNodeList lstReference = nodeItem.SelectNodes("settingsReference");
                    foreach (XmlNode reference in lstReference)
                    {
                        XmlNode node = doc.SelectSingleNode(StrFunc.AppendFormat(@"setup/settings[@id=""{0}""]", reference.Attributes["href"].Value));
                        if (null != node)
                            AddParameters(parameters, node);
                    }

                    //Chargement des paramètres présents dans les noeud add enfants du noeud instance
                    AddParameters(parameters, nodeItem);

                    //Chargement des paramètres présents dans les noeuds add enfants du noeud defaultSettings
                    if (null != nodeDefault)
                        AddParameters(parameters, nodeDefault);

                    if (parameters.Count > 0)
                    {
                        string instance = nodeItem.Attributes["name"].Value;
                        parameters.Add(ServiceKeyEnum.Instance.ToString(), instance);
                        ret.Add(instance, parameters);
                    }
                }
            }

            // Lorsqu'il aucun paramétrage pour l'instance en cours d'installation et qu'il existe des defaultSettings
            // Spheres génère ici une entrée pour l'instance en cours d'installation avec les paramètres présents dans defaultSettings
            if ((null != nodeDefault) && (false == ret.ContainsKey(pInstanceNameInSetupConfig)))
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                AddParameters(parameters, nodeDefault);
                if (parameters.Count > 0)
                {
                    string instance = pInstanceNameInSetupConfig;
                    parameters.Add(ServiceKeyEnum.Instance.ToString(), instance);
                    ret.Add(instance, parameters);
                }
            }

            return ret;
        }

        /// <summary>
        /// Mise à jour du fichier de configuration du service
        /// </summary>
        //FI 20131010 [19041] add Method 
        protected virtual void SaveConfig()
        {


        }

        /// <summary>
        /// Retourne true s'il existe un fihier de configuration pour le setup
        /// </summary>
        /// <param name="pFileConfig">Retourne l'URL du fichier</param>
        /// <returns></returns>
        /// FI 20131010 [19041] Add Method
        /// FI 20131106 [19139] Utilisation de la Parameters["assemblyPath"]
        private Boolean ExistFileSpheresSetupConfig(out string pFileConfig)
        {
            pFileConfig = null;

            //FI 20131106 [19139]
            //ServiceInfo serviceInfo = new ServiceInfo(this.serviceInstaller.ServiceName);
            //string fullName = serviceInfo.GetInformationsKey(ServiceKeyEnum.FullName);

            string assemblyPath = this.Context.Parameters["assemblyPath"];
            string pathRoot = Directory.GetParent(assemblyPath).Parent.FullName;

            string fileConfig = StrFunc.AppendFormat(@"{0}\SpheresSetupConfig.xml", pathRoot);

            Boolean ret = true;
            while (false == File.Exists(fileConfig) && ret)
            {
                DirectoryInfo directoryInfo = Directory.GetParent(pathRoot);
                ret = (null != directoryInfo);
                if (ret)
                {
                    pathRoot = directoryInfo.FullName;
                    fileConfig = StrFunc.AppendFormat(@"{0}\SpheresSetupConfig.xml", pathRoot);
                }
            }

            if (ret)
            {
                pFileConfig = fileConfig;
            }

            return ret;
        }

        /// <summary>
        /// Charge les instances paramétrées et leurs paramétrages respectifs présents dans le fichier de configuration de setup
        /// </summary>
        /// <returns></returns>
        /// FI 20131011 [19041] Add Method
        /// FI 20170824 [XXXXX] Modify
        private void LoadInstanceParameters()
        {
            // FI 20170824 [XXXXX] Alimentation de fileConfig
            FileConfig = string.Empty;
            if (ExistFileSpheresSetupConfig(out string tmpfileConfig))
            {
                FileConfig = tmpfileConfig;
                InstanceParameters = GetInstanceParameters(FileConfig, ServiceEnum.ToString(), this.InstanceNameInSetupConfig);
                IsSilentMode = IsSetupSilentMode(FileConfig);
            }
        }

        /// <summary>
        /// Remplace les informations sur l'installation en cours par celles présentes dans le fichier de configuration
        /// </summary>
        /// FI 20131011 [19041] Add Method
        private void ReplaceContext()
        {
            Dictionary<string, string> replaceContext = new Dictionary<string, string>();

            if (ArrFunc.IsFilled(InstanceParameters))
            {
                if (InstanceParameters.ContainsKey(InstanceNameInSetupConfig))
                {
                    Dictionary<string, object> serviceParameters = (Dictionary<string, object>)InstanceParameters[InstanceNameInSetupConfig];
                    foreach (string key in serviceParameters.Keys)
                    {
                        if (this.Context.Parameters.ContainsKey(key) && key != ServiceKeyEnum.Instance.ToString())
                        {
                            this.Context.Parameters[key] = (String)serviceParameters[key];
                            replaceContext.Add(key, (String)serviceParameters[key]);
                        }
                    }
                }

                if (false == IsSilentMode)
                {
                    if (ArrFunc.IsFilled(replaceContext))
                    {
                        string msgTitle = "Install service";

                        string msgService = StrFunc.AppendFormat("Service : {0} \x0A\x0AThe following keys have been overwritten.\x0A", this.serviceInstaller.ServiceName);
                        foreach (string key in replaceContext.Keys)
                        {
                            string replaceValue = replaceContext[key];
                            if (key == ServiceKeyEnum.ServicePassword.ToString())
                                replaceValue = "*".PadLeft(replaceValue.Length, '*');
                            msgService += StrFunc.AppendFormat(" - {0} : {1} \x0A", key, replaceValue);
                        }

                        MessageBox.Show(msgService, msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        /// <summary>
        /// Initialise le compte windows® sur l'installation 
        /// </summary>
        /// FI 20131106 [19139] Add method
        private void SetServiceUserFromContext()
        {
            string serviceAccount = this.Context.Parameters[ServiceKeyEnum.ServiceAccount.ToString()];
            try
            {
                ServiceAccount serviceAccountEnum = (ServiceAccount)Enum.Parse(typeof(ServiceAccount), serviceAccount);
                processInstaller.Account = serviceAccountEnum;
            }

            catch (Exception ex)
            {
                throw new Exception(StrFunc.AppendFormat("value:{0} is not valid", serviceAccount), ex);
            }

            if (processInstaller.Account == ServiceAccount.User)
            {
                processInstaller.Username = this.Context.Parameters[ServiceKeyEnum.ServiceUserName.ToString()];
                processInstaller.Password = this.Context.Parameters[ServiceKeyEnum.ServicePassword.ToString()];
            }
        }

        /// <summary>
        /// Ajoute à la liste des paramètres {parameters} les valeurs présentes dans les noeuds "add" enfant du noeud {node}
        /// <para>L'ajout n'est effectué que si le paramètre est non présent</para>
        /// </summary>
        /// <param name="parameters">liste des paramètres</param>
        /// <param name="node">noeud qui contient des enfants de type add</param>
        /// FI 20131107 [19142] Add Method
        private static void AddParameters(Dictionary<string, object> parameters, XmlNode node)
        {
            XmlNodeList lstAdd = node.SelectNodes("add");
            foreach (XmlNode nodeAdd in lstAdd)
            {
                XmlAttribute _attrkey = nodeAdd.Attributes["key"];
                if ((null != _attrkey) && (false == (parameters.ContainsKey(_attrkey.Value))))
                {
                    //FI 20131018 [] call MOMSettings.TranslateMOMPath
                    string value = nodeAdd.Attributes["value"].Value;
                    if (_attrkey.Value == ServiceKeyEnum.MOMPath.ToString())
                        value = MOMSettings.TranslateMOMPath(value, Software.VersionBuild);
                    parameters.Add(_attrkey.Value, value);
                }
            }
        }

        /// <summary>
        /// Lecture de l'attribut setup/@silent 
        /// </summary>
        /// <param name="pFileName">fichier de configfuration</param>
        /// <returns></returns>
        private static Boolean IsSetupSilentMode(string pFileName)
        {
            Boolean ret = false;
            // EG 20160404 Migration vs2013
            //XmlDataDocument doc = new XmlDataDocument();
            XmlDocument doc = new XmlDocument();
            doc.Load(pFileName);

            XmlNode nodeMute = doc.SelectSingleNode(StrFunc.AppendFormat("setup/@silent"));
            if (null != nodeMute)
                ret = BoolFunc.IsTrue(nodeMute.Value);

            return ret;
        }

        /// <summary>
        /// Ajoute une dépendance du service au service SpherresLogger
        /// </summary>
        /// <returns></returns>
        // PM 20200102 [XXXXX] New Log: Add method
        protected int AddLoggerReference()
        {
            int exitCode = 0;
            string msgTitle = "Adding reference to SpheresLogger service";
            string serviceName = serviceInstaller.ServiceName;
            try
            {
                exitCode = SpheresServiceParametersHelper.AddLoggerReference(serviceName, out string standardOutput);

                if ((false == IsSilentMode) && (exitCode != 0))
                {
                    string msgService = string.Format("Service {0} : sc.exe config failed with exit code : {1}\x0AOutput : {2}", serviceName, exitCode, standardOutput);
                    MessageBox.Show(msgService, msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                string errMsgService = "Service : " + serviceName + "\x0A\x0A" +
                                       "Unable to add dependance to service SpheresLogger.\x0A\x0A" +
                                       ex.Message;

                MessageBox.Show(errMsgService, msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return exitCode;
        }
        #endregion Methods
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Install;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Windows.Forms;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
//
using Microsoft.Win32;

namespace EFS.SpheresService
{
    // PM 20200601 [XXXXX] Déplacé dans ACommon/ServiceTools.cs
    ///// <summary>
    ///// 
    ///// </summary>
    ///// FI 20131106 [19139] Add ServiceAccount, ServiceUserName, ServicePassword
    //public enum ServiceKeyEnum
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    ServiceEnum,
    //    /// <summary>
    //    /// Obtient ou définit le nom court utilisé pour identifier le service sur le sercice
    //    /// <para>Exemple :SpheresConfirmationMsgGenv5.1.6102-Inst:ConfirmationGen1 </para>
    //    /// </summary>
    //    ServiceName,
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    DisplayName,
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    Description,
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    Instance,
    //    /// <summary>
    //    /// Exemple : C:\Program Files\EFS\Spheres GateFIXMLEurex 3.0 RC\SpheresGateFIXMLEurexServicev3.0.exe -sSpheresGateFIXMLEurexv3.0.4737-Inst:Instance
    //    /// </summary>
    //    ImagePath,
    //    /// <summary>
    //    /// Exemple : SpheresGateFIXMLEurexServicev3.0
    //    /// </summary>
    //    ExeName,
    //    /// <summary>
    //    /// Exemple C:\Program Files\EFS\Spheres GateFIXMLEurex 3.0 RC\
    //    /// </summary>
    //    PathInstall,
    //    /* FI 20160804 [Migration TFS] Supression de Path 
    //    /// <summary>
    //    /// Exemple C:\Program Files\EFS\Spheres GateFIXMLEurex 3.0 RC\SpheresGateFIXMLEurex
    //    /// </summary>
    //    Path,
    //    */
    //    /* FI 20160804 [Migration TFS] Supression de PathXml (=> Non utilisé)
    //    ///// <summary>
    //    ///// Exemple C:\Program Files\EFS\Spheres GateFIXMLEurex 3.0 RC\SpheresGateFIXMLEurex\XML_Files
    //    ///// </summary>
    //    //PathXml,
    //    */
    //    /// <summary>
    //    /// Exemple C:\Program Files\EFS\Spheres GateFIXMLEurex 3.0 RC\SpheresGateFIXMLEurexServicev3.0.exe
    //    /// </summary>
    //    FullName,
    //    /// <summary>
    //    /// {FileWatcher | MSMQ | MQSerie}
    //    /// </summary>
    //    MOMType,
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    MOMPath,
    //    /// <summary>
    //    /// Obtient ou Définit la caractéristique "recoverable" d'un message
    //    /// <para>
    //    /// Un message « non recouvrable » est plus rapide à envoyer, car stocké uniquement en RAM. 
    //    /// <para>Attention: si le service MSMQ est redémarré tous les messages « non recouvrable » de la queue sont perdus !</para>
    //    /// <para>Default value: true</para>
    //    /// </summary>
    //    MOMRecoverable,
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    MOMEncrypt,
    //    /// <summary>
    //    /// Niveau de log au niveau des services (Pilote l'écriture dans le journal des évènements de Windows®)
    //    /// <para>Ne pas confondre avec le niveau de log des traitements</para>
    //    /// </summary>
    //    LogLevel,
    //    /// <summary>
    //    /// Timeout d'inaccessibilité à une queue de type MSMQ
    //    /// <para>Default value: 60 sec.</para>
    //    /// </summary>
    //    MSMQUnreachableTimeout,
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    Application,
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    AppDirectory,
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    ActivateObserver,
    //    /// <summary>
    //    /// Classe du service
    //    /// <para>Exemple EFS.SpheresService.SpheresIOService</para>
    //    /// </summary>
    //    ClassType,
    //    /// <summary>
    //    /// <para>
    //    /// Exemple SpheresGateFIXMLEurexv3.0.4737
    //    /// </para>
    //    /// </summary>
    //    Prefix,
    //    /// <summary>
    //    /// Activation oui/non du mode cache de connexions sur la gestion des queues.
    //    /// <para></para>
    //    /// </summary>
    //    MSMQEnableConnectionCache,
    //    /// <summary>
    //    /// Type d'utilisateur 
    //    /// <para>Valeurs possibles définies par l'enum System.ServiceProcess.ServiceAccount</para>
    //    /// </summary>
    //    ServiceAccount,
    //    /// <summary>
    //    /// Compte Windows® qui exécute le service 
    //    /// <para>Doit être renseigné lorsque ServiceUser = User</para>
    //    /// </summary>
    //    ServiceUserName,
    //    /// <summary>
    //    /// Mot de passe du compte Windows® qui exécute le service 
    //    /// <para>Doit être renseigné lorsque ServiceUser = User</para>
    //    /// </summary>
    //    ServicePassword
    //}

    #region class ServiceTools
    public sealed class ServiceTools
    {
        // PM 20200601 [XXXXX] Déplacé dans la classe RegistryConst dans ACommon/Registry.cs
        //#region public const
        ///// <summary>
        ///// System\\CurrentControlSet\\Services\\
        ///// </summary>
        //public const string RegistryKey = "System\\CurrentControlSet\\Services\\";
        ///// <summary>
        ///// Parameters
        ///// </summary>
        //public const string RegistrySubKeyParameters = "Parameters";
        ///// <summary>
        ///// Eventlog
        ///// </summary>
        //public const string RegistrySubKeyLog = "Eventlog";
        ///// EG 20130619 Replace "_" by "-Inst:"
        //public const string DelimiterInstance = "-Inst:";
        //#endregion public const

        #region Constructor
        public ServiceTools() { }
        #endregion Constructor

        #region Methods
        // PM 20200601 [XXXXX] Déplacé dans la classe de RegistryTools (ACommon/Registry.cs)
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="pServiceName"></param>
        ///// <returns></returns>
        //public static void AddServiceNameToImagePath(string pServiceName)
        //{
        //    RegistryKey service = null;
        //    try
        //    {
        //        //PL 20120319 Add pIsWritable = true
        //        service = RegistryTools.GetRegistryKeyService(true, pServiceName);
        //        if (null != service)
        //        {
        //            string key = ServiceKeyEnum.ImagePath.ToString();
        //            string realImagePath = (string)service.GetValue(key);
        //            if (StrFunc.IsFilled(realImagePath))
        //                realImagePath = realImagePath.Replace("\"", string.Empty);
        //            int i = realImagePath.IndexOf("-s");
        //            if (-1 < i)
        //                realImagePath = realImagePath.Substring(0, i);
        //            service.SetValue(ServiceKeyEnum.ImagePath.ToString(), realImagePath + " -s" + pServiceName);
        //        }
        //    }
        //    finally
        //    {
        //        if (null != service)
        //            service.Close();
        //    }
        //}

        /// <summary>
        /// Create the registry key needed by the service in input for DEBUG 
        /// </summary>
        /// <param name="pServiceEnum">Main service type</param>
        /// <param name="pVersion">current assembly version</param>
        /// <param name="opServiceName">output, the full service name used tu build the key</param>
        /// <returns>true when the create operation is correctly executed</returns>
        /// MF 20120606 Ticket 17860
        /// EG 20121221
        public static bool CreateRegistryServiceInformation(Cst.ServiceEnum pServiceEnum, Version pVersion, out string opServiceName)
        {
            bool created = false;

            // 1. Define the name of the service, please note the NORMAL mode
            opServiceName = String.Format("{0}v{1}.{2}.{3}", pServiceEnum, pVersion.Major, pVersion.Minor, pVersion.Build);
            // 2. Load the registry key related to the service
            RegistryKey serviceRegKey = RegistryTools.GetRegistryKeyService(true, opServiceName);
#if DEBUG
            if (serviceRegKey == null)
            {
                // 2.1. Define the name of the service, please note the DEBUG mode
                opServiceName = String.Format("{0}DEBUGv{1}.{2}.{3}", pServiceEnum, pVersion.Major, pVersion.Minor, pVersion.Build);
                serviceRegKey = RegistryTools.GetRegistryKeyService(true, opServiceName);
            }
#endif

            // 2.2 When the registry key does not exist then we start the creation process
            if (serviceRegKey == null)
            {

                // 3. compute the assembly location, used to fill the ImagePath parameter
                string fullPathAssembly = System.Reflection.Assembly.GetEntryAssembly().Location;
                // 3.1  20120711 MF replace the assembly service name with the current service type name (Ticket 17860, Item 4)
                fullPathAssembly = fullPathAssembly.Replace(pServiceEnum.ToString(), pServiceEnum.ToString());

                // 4. Define a regex to find all the key related to the specific REFERENCE service inside of the currentcontrolset registry key
                //Regex findKeyServices = new Regex(String.Format(@"^{0}v(\d+).(\d+).(\d+)$", pReferenceParamsServiceType));
                // EG 20131024 La regex ne concerne que la révision
                Regex findKeyServices = new Regex(String.Format(@"^{0}v{1}.{2}.(\d+)$", pServiceEnum, pVersion.Major, pVersion.Minor));

                // 5. the default directory name of the queue for the current service assembly version, used only when no previous version
                //    of the reference service has found
                string defaultNameQueue = String.Format(@"Queue_v{0}.{1}.{2}", pVersion.Major, pVersion.Minor, pVersion.Build);

                // 5.1 default mompath ... following the same rules at point 5 
                string momPath = String.Format(@"C:\Spheres\Process\{0}", defaultNameQueue);

                RegistryKey currentControlSet = null;
                RegistryKey serviceRegKeyParams = null;
                RegistryKey logRegKey = null;
                RegistryKey serviceLogRegKey = null;
                RegistryKey newerServiceRegKey = null;
                RegistryKey newerServiceRegKeyParams = null;

                try
                {
                    Cst.MOM.MOMEnum momType = Cst.MOM.MOMEnum.FileWatcher;

                    if (pServiceEnum != Cst.ServiceEnum.SpheresLogger)
                    {
                        // 6. Open the current control set key
                        currentControlSet = Registry.LocalMachine.OpenSubKey(RegistryConst.RegistryKey, true);

                        // 6.1 Get all the rgistry keys inside of the current control set
                        IEnumerable<string> keysControlSet = currentControlSet.GetSubKeyNames();

                        // 6.2 filter the registry key, and get only the keys related to the current reference service
                        keysControlSet = keysControlSet.Where(elem => findKeyServices.IsMatch(elem));

                        if (keysControlSet.Count() > 0)
                        {

                            // 6.3 Compute the version number for each installed service instance
                            // EG 20131024 La regex ne concerne que la révision
                            //var parsedKeys = (from key in keysControlSet
                            //                  let match = findKeyServices.Match(key)
                            //                  select new
                            //                  {
                            //                      KeyName = key,
                            //                      Version =
                            //                          int.Parse(match.Groups[1].Value) * 100 +
                            //                          int.Parse(match.Groups[2].Value) * 10 +
                            //                          int.Parse(match.Groups[3].Value),
                            //                  });

                            var parsedKeys = (from key in keysControlSet
                                              let match = findKeyServices.Match(key)
                                              select new
                                              {
                                                  KeyName = key,
                                                  Version = int.Parse(match.Groups[1].Value)
                                              });

                            // 6.4 get the newer installed service
                            var maxKey =
                                (from parsedKey in parsedKeys
                                 where parsedKey.Version == parsedKeys.Max(elem => elem.Version)
                                 select parsedKey).First();

                            // 6.4.1 ... and retrieve its own parameters
                            newerServiceRegKey = currentControlSet.OpenSubKey(maxKey.KeyName, false);
                            newerServiceRegKeyParams = newerServiceRegKey.OpenSubKey(RegistryConst.RegistrySubKeyParameters, false);

                            string valueMomType =
                                (string)newerServiceRegKeyParams.GetValue(Enum.GetName(typeof(ServiceKeyEnum), ServiceKeyEnum.MOMType), momType);

                            momType = Enum.IsDefined(typeof(Cst.MOM.MOMEnum), valueMomType) ?
                                (Cst.MOM.MOMEnum)Enum.Parse(typeof(Cst.MOM.MOMEnum), valueMomType) : momType;

                            momPath =
                                (string)newerServiceRegKeyParams.GetValue(Enum.GetName(typeof(ServiceKeyEnum), ServiceKeyEnum.MOMPath), momPath);

                            // 6.4.1.1 ... define a regex in order to modify the retrieven mompath for the newer installed service
                            // 20120710 MF introducing a group name <prefix> to get rid of the _ during the replacement
                            Regex findPath = new Regex(@"^(?<prefix>.*)(\d+).(\d+).(\d+)$");

                            // 6.4.1.2 ... replace and create a up to date mompath related to the current assembly version
                            if (findPath.IsMatch(momPath))
                            {
                                // 20120710 MF introducing a group name <prefix> in order to do not use the _ separator 
                                //             between the name and version number
                                momPath = findPath.Replace(momPath, String.Format("${{prefix}}{0}.{1}.{2}", pVersion.Major, pVersion.Minor, pVersion.Build));
                            }
                            // 6.4.1.3 ... if the retireven mompath owned by the newer installed service does not have a version number, we create
                            //         a new directory using the default queue name
                            else
                            {
                                // EG 20120627 Combine FullName and MOMPath for FileWatcher only
                                if ((momType == Cst.MOM.MOMEnum.FileWatcher) && StrFunc.IsEmpty(momPath))
                                {
                                    DirectoryInfo parentDir = Directory.GetParent(momPath);
                                    momPath = Path.Combine(parentDir.FullName, defaultNameQueue);
                                }
                            }
                        }
                    }

                    // 7. create the new service key
                    serviceRegKey = currentControlSet.CreateSubKey(opServiceName);

                    serviceRegKey.SetValue(Enum.GetName(typeof(ServiceKeyEnum), ServiceKeyEnum.ServiceEnum), pServiceEnum);
                    serviceRegKey.SetValue(Enum.GetName(typeof(ServiceKeyEnum), ServiceKeyEnum.DisplayName), opServiceName);
                    serviceRegKey.SetValue(Enum.GetName(typeof(ServiceKeyEnum), ServiceKeyEnum.Description), opServiceName);
                    serviceRegKey.SetValue(Enum.GetName(typeof(ServiceKeyEnum), ServiceKeyEnum.ImagePath), fullPathAssembly);

                    // 7.1 create the parameters sub-key
                    serviceRegKeyParams = serviceRegKey.CreateSubKey(RegistryConst.RegistrySubKeyParameters);

                    if (pServiceEnum != Cst.ServiceEnum.SpheresLogger)
                    {
                        serviceRegKeyParams.SetValue(Enum.GetName(typeof(ServiceKeyEnum), ServiceKeyEnum.MOMType), momType);
                        serviceRegKeyParams.SetValue(Enum.GetName(typeof(ServiceKeyEnum), ServiceKeyEnum.MOMPath), momPath);

                        // 8. build the missing directories
                        if (momType == Cst.MOM.MOMEnum.FileWatcher)
                        {
                            ServiceTools.CreateWatcherFolder(pServiceEnum, momPath);
                            ServiceTools.CreateWatcherFolder(Cst.ServiceEnum.SpheresResponse, momPath);
                            // FI 20140519 [19923] add folder for SpheresWebSession
                            ServiceTools.CreateWatcherFolder(Cst.ServiceEnum.SpheresWebSession, momPath);
                        }
                    }

                    serviceRegKeyParams.SetValue(Enum.GetName(typeof(ServiceKeyEnum), ServiceKeyEnum.ActivateObserver), false);
                    serviceRegKeyParams.SetValue(Enum.GetName(typeof(ServiceKeyEnum), ServiceKeyEnum.ClassType), "");

                    // 9. build the log key, for the the event log queue linked to the service
                    // 10. save the reference to the SpheresServicesMessage module
                    RegistryTools.SetEventLogService(opServiceName);

                    created = true;
                }
                finally
                {
                    // 11. free all the registry resources
                    if (newerServiceRegKey != null)
                    {
                        newerServiceRegKey.Close();
                    }
                    if (newerServiceRegKeyParams != null)
                    {
                        newerServiceRegKeyParams.Close();
                    }
                    if (serviceRegKeyParams != null)
                    {
                        serviceRegKeyParams.Close();
                    }
                    if (serviceRegKey != null)
                    {
                        serviceRegKey.Close();
                    }
                    if (serviceLogRegKey != null)
                    {
                        serviceLogRegKey.Close();
                    }
                    if (logRegKey != null)
                    {
                        logRegKey.Close();
                    }
                    if (currentControlSet != null)
                    {
                        currentControlSet.Close();
                    }
                    if (created)
                    {
                        // not useful in debug mode
                        RegistryTools.AddServiceNameToImagePath(opServiceName);
                    }
                }
            }
            return created;
        }

        // TODO MF 20110328 DeleteRegistryServiceInformation pas encore testé!
        /// <summary>
        /// Delete the Windows registry keys related to the service in input
        /// </summary>
        /// <param name="pServiceName">the name of the service we want to clean the registry keys</param>
        /// <param name="pLogName">the log queue linked to the service, needed to remove the xxx_L registry keys</param>
        /// <returns>true when the delete operation is executed</returns>
        public static bool DeleteRegistryServiceInformation(string pServiceName, string pLogName)
        {
            bool deleted = false;

            //PL 20120319 Add pIsWritable = true
            RegistryKey serviceRegKey = RegistryTools.GetRegistryKeyService(true, pServiceName);

            if (serviceRegKey != null)
            {
                RegistryKey currentControlSet = null;

                RegistryKey logRegKey = null;

                try
                {

                    currentControlSet = Registry.LocalMachine.OpenSubKey(RegistryConst.RegistryKey);

                    currentControlSet.DeleteSubKeyTree(pServiceName);

                    logRegKey = currentControlSet.OpenSubKey(Path.Combine(RegistryConst.RegistrySubKeyLog, pLogName), true);

                    currentControlSet.DeleteSubKeyTree(RegistryTools.GetEventLogSource(pServiceName));

                }
                finally
                {
                    if (currentControlSet != null)
                        currentControlSet.Close();

                    if (logRegKey != null)
                        logRegKey.Close();
                }

                deleted = true;
            }

            return deleted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <returns></returns>
        public static void DeleteParametersService(string pServiceName)
        {
            RegistryKey service = null;
            try
            {
                //PL 20120319 Add pIsWritable = true
                service = RegistryTools.GetRegistryKeyService(true, pServiceName);
                if (null != service)
                {
                    if (service.OpenSubKey(RegistryConst.RegistrySubKeyParameters) != null)
                        service.DeleteSubKeyTree(RegistryConst.RegistrySubKeyParameters);
                }
            }
            finally
            {
                if (null != service)
                    service.Close();
            }
        }

        // PM 20200601 [XXXXX] Déplacé dans la classe de RegistryTools (ACommon/Registry.cs)
        ///// <summary>
        /////  Suprresion des entrées utilisées pour écriture dans le journal  des événements de windows®
        ///// </summary>
        ///// FI 20161026 [XXXXX] Modify
        //public static void DeleteEventLogService(string pServiceName)
        //{
        //    RegistryKey serviceLog = null;
        //    RegistryKey service = null;
        //    try
        //    {
        //        string logName = GetEventLog(pServiceName);
        //        string logSource = GetEventLogSource(pServiceName);
        //        //PL 20120319 Add pIsWritable = true
        //        service = RegistryTools.GetRegistryKeyService(true, RegistryConst.RegistrySubKeyLog);
        //        if (null != service)
        //        {
        //            serviceLog = service.OpenSubKey(logName, true);
        //            if (serviceLog.OpenSubKey(logSource) != null)
        //                serviceLog.DeleteSubKeyTree(logSource);
        //        }
        //    }
        //    finally
        //    {
        //        if (null != serviceLog)
        //            serviceLog.Close();
        //        if (null != service)
        //            service.Close();
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <param name="pDataSource"></param>
        /// <returns></returns>
        public static void GetDataSourceService(string pServiceName, ref string pDataSource)
        {
            GetDataSourceService(pServiceName, ref pDataSource, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <param name="pDataSource"></param>
        /// <param name="pIsPasswordEncrypted"></param>
        /// <returns></returns>
        public static void GetDataSourceService(string pServiceName, ref string pDataSource, bool pIsPasswordEncrypted)
        {
            string dataSource = string.Empty;
            RegistryKey parameters = null;
            try
            {
                //PL 20120319 Use pIsWritable = false
                parameters = RegistryTools.GetRegistryKeyServiceParameters(false, pServiceName);
                if (null != parameters)
                {
                    string rdbms = (string)parameters.GetValue("Rdbms");
                    string server = (string)parameters.GetValue("Server");
                    string dbName = (string)parameters.GetValue("DbName");
                    string user = (string)parameters.GetValue("User");
                    string pwd = (string)parameters.GetValue("Pwd");

                    if (DataHelper.IsRdmbsOracle(rdbms))
                        dataSource = SystemSettings.GetAppSettings("OracleTemplateConnectionString");
                    else if (DataHelper.IsRdmbsSQLServer(rdbms))
                        dataSource = SystemSettings.GetAppSettings("SqlServerTemplateConnectionString");

                    if (null != dataSource)
                    {
                        if (StrFunc.IsFilled(server))
                        {
                            dataSource = dataSource.Replace("SID()", server);
                            dataSource = dataSource.Replace("SERVERNAME()", server);
                        }
                        if (StrFunc.IsFilled(dbName))
                        {
                            dataSource = dataSource.Replace("DBNAME()", dbName);
                            dataSource = dataSource.Replace("SCHEMA()", dbName);
                        }
                        dataSource = dataSource.Replace("USERNAME()", user);
                        dataSource = dataSource.Replace("PWD()", Cryptography.Decrypt(pwd));
                    }

                }
            }
            finally
            {
                if (null != parameters)
                    parameters.Close();
                pDataSource = dataSource;
            }

        }

        // PM 20200601 [XXXXX] Déplacé dans la classe de RegistryTools (ACommon/Registry.cs)
        ///// <summary>
        ///// /
        ///// </summary>
        ///// <param name="pIsWritable"></param>
        ///// <param name="pServiceName"></param>
        ///// <param name="pAdditionalParameters"></param>
        ///// <returns></returns>
        /////PL 20120319 Add pIsWritable
        ///// EG 20130619 Passage Public -> Private
        //private static RegistryKey GetRegistryKeyService(bool pIsWritable, string pServiceName, params string[] pAdditionalParameters)
        //{
        //    if (pAdditionalParameters == null || pAdditionalParameters.Length == 0 || pAdditionalParameters.Length == 3)
        //        return Registry.LocalMachine.OpenSubKey(RegistryConst.RegistryKey + pServiceName, pIsWritable);
        //    else if (pAdditionalParameters.Length == 4)
        //        return Registry.LocalMachine.OpenSubKey(pAdditionalParameters[3], pIsWritable);
        //    else
        //        throw new ArgumentOutOfRangeException("pAdditionalParameters", "pAdditionalParameters length not equals 3 or 4");
        //}

        // PM 20200601 [XXXXX] Déplacé dans la classe de RegistryTools (ACommon/Registry.cs)
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="pIsWritable"></param>
        ///// <param name="pServiceName"></param>
        ///// <param name="pAdditionalParameters"></param>
        ///// <returns></returns>
        ///// PL 20120319 Add pIsWritable
        ///// EG 20130619 Passage Public -> Private
        //private static RegistryKey GetRegistryKeyServiceParameters(bool pIsWritable, string pServiceName, params string[] pAdditionalParameters)
        //{
        //    RegistryKey service = GetRegistryKeyService(pIsWritable, pServiceName, pAdditionalParameters);
        //    if (null != service)
        //        return service.OpenSubKey(RegistryConst.RegistrySubKeyParameters, pIsWritable);
        //    return null;
        //}

        /// <summary>
        /// Get service registry values (root and parameters sub-dir)
        /// </summary>
        /// <param name="pServiceName">Service Type</param>
        /// <param name="pAdditionalParameters">Optional parameters collection used to read an alternative key;  
        /// array structure: array structure: [serviceName, serviceDisplayName, serviceDescription, local machine key path]</param>
        /// <returns></returns>
        /// FI 20131106 [19139] Lecture des paramètres ServiceAccount,ServiceUserName,ServicePassword
        public static StringDictionary GetRegistryServiceInformations(string pServiceName, params string[] pAdditionalParameters)
        {
            StringDictionary dictionary = null;

            string serviceName = pServiceName;

            using (RegistryKey regKey_Service = RegistryTools.GetRegistryKeyService(false, serviceName, pAdditionalParameters))
            {
                if (regKey_Service == null)
                {
                    //PL 20121228 New features
                    #region Recherche d'une clé d'un service de même version (Maj.Min) mais d'une autre Revision
                    // NB: Depuis la v3 de Spheres®, le nom du service est suffixé par le n° de version (Maj.Min.Rev)
                    using (RegistryKey regKey_ServiceSameVersion = RegistryTools.GetRegistryKeyService(false, string.Empty, null))
                    {
                        if (AppInstance.TraceManager != null)
                            AppInstance.TraceManager.TraceVerbose(null, string.Format("pServiceName={0}", serviceName));

                        // PL 20160113 Idéalement il faudrait mettre en place l'algo suivant:
                        //Step 1: Recherche d'une clé d'un service de même version (Maj.Min.Rev), mais sur une autre instance, sans contrôle du chiffre d'instance
                        //        ex. Source: SpheresNormMsgFactoryv5.0.5820-Inst:ORA  ou  SpheresNormMsgFactoryv5.0.5820-Inst:ORA1
                        //            Target: SpheresIOv5.0.5820-Inst:ORA  ou  SpheresIOv5.0.5820-Inst:ORA5  
                        //Step 2: Recherche d'une clé d'un service de même version (Maj.Min.Rev), mais sans instance
                        //        ex. Source: SpheresNormMsgFactoryv5.0.5820-Inst:ORA  ou  SpheresNormMsgFactoryv5.0.5820-Inst:ORA1
                        //            Target: SpheresIOv5.0.5820
                        //Step 3: Recherche d'une clé d'un service de même version (Maj.Min), toutes Revisions, mais sur la même instance
                        //        ex. Source: SpheresNormMsgFactoryv5.0.5820-Inst:ORA1
                        //            Target: SpheresIOv5.0.5819-Inst:ORA1  ou  SpheresIOv5.0.5821-Inst:ORA1  
                        //Step 4: Recherche d'une clé d'un service de même version (Maj.Min), toutes Revisions, mais sur une autre instance, sans contrôle du chiffre d'instance
                        //        ex. Source: SpheresNormMsgFactoryv5.0.5820-Inst:ORA  ou  SpheresNormMsgFactoryv5.0.5820-Inst:ORA1
                        //            Target: SpheresIOv5.0.5819-Inst:ORA  ou  SpheresIOv5.0.5821-Inst:ORA5  
                        //Step 5: Recherche d'une clé d'un service de même version (Maj.Min), toutes Revisions, mais sans instance
                        //        ex. Source: SpheresNormMsgFactoryv5.0.5820-Inst:ORA  ou  SpheresNormMsgFactoryv5.0.5820-Inst:ORA1
                        //            Target: SpheresIOv5.0.5819  ou  SpheresIOv5.0.5821

                        // PL 20160113 Add test Contains()
                        if ((regKey_ServiceSameVersion != null) && serviceName.Contains("."))
                        {
                            string serviceName_WithoutRevision = serviceName.Substring(0, serviceName.LastIndexOf("."));
                            List<string> lstService = new List<string>();

                            foreach (string subRegKey in regKey_ServiceSameVersion.GetSubKeyNames())
                            {
                                //Clé d'un service de même nom et même version (Maj.Min)
                                if (subRegKey.StartsWith(serviceName_WithoutRevision))
                                    lstService.Add(subRegKey);
                            }
                            lstService.Sort(); //Tri pour retenir la Revision la plus récente

                            serviceName = lstService.LastOrDefault();
                        }
                    }
                    #endregion
                }
            }

            using (RegistryKey regKey_Service = RegistryTools.GetRegistryKeyService(false, serviceName, pAdditionalParameters))
            {
                if (regKey_Service != null)
                {
                    dictionary = new StringDictionary();
                    #region Main information
                    dictionary.Add(ServiceKeyEnum.ServiceName.ToString(), pServiceName);
                    dictionary.Add(ServiceKeyEnum.ServiceEnum.ToString(), (string)regKey_Service.GetValue(ServiceKeyEnum.ServiceEnum.ToString()));
                    dictionary.Add(ServiceKeyEnum.DisplayName.ToString(), (string)regKey_Service.GetValue(ServiceKeyEnum.DisplayName.ToString()));
                    dictionary.Add(ServiceKeyEnum.Description.ToString(), (string)regKey_Service.GetValue(ServiceKeyEnum.Description.ToString()));

                    // In case the path contains some spaces the ImagePath string will be automatically incapsulated within ", 
                    // cleaning special characters
                    string strNormalised = ((string)regKey_Service.GetValue(ServiceKeyEnum.ImagePath.ToString()));
                    if (StrFunc.IsFilled(strNormalised))
                        strNormalised = strNormalised.Trim(new char[] { '"' });
                    dictionary.Add(ServiceKeyEnum.ImagePath.ToString(), strNormalised);
                    #endregion Main information

                    #region SubKey Parameters information
                    using (RegistryKey regKey_Parameters = RegistryTools.GetRegistryKeyServiceParameters(false, pServiceName, pAdditionalParameters))
                    {
                        if (null != regKey_Parameters)
                        {
                            dictionary.Add(ServiceKeyEnum.Instance.ToString(), (string)regKey_Parameters.GetValue(ServiceKeyEnum.Instance.ToString()));
                            dictionary.Add(ServiceKeyEnum.MOMType.ToString(), (string)regKey_Parameters.GetValue(ServiceKeyEnum.MOMType.ToString()));
                            dictionary.Add(ServiceKeyEnum.MOMPath.ToString(), (string)regKey_Parameters.GetValue(ServiceKeyEnum.MOMPath.ToString()));

                            dictionary.Add(ServiceKeyEnum.ActivateObserver.ToString(), (string)regKey_Parameters.GetValue(ServiceKeyEnum.ActivateObserver.ToString()));

                            dictionary.Add(ServiceKeyEnum.ClassType.ToString(), (string)regKey_Parameters.GetValue(ServiceKeyEnum.ClassType.ToString()));
                            dictionary.Add(ServiceKeyEnum.Prefix.ToString(), (string)regKey_Parameters.GetValue(ServiceKeyEnum.Prefix.ToString()));

                            dictionary.Add(ServiceKeyEnum.MSMQEnableConnectionCache.ToString(), (string)regKey_Parameters.GetValue(ServiceKeyEnum.MSMQEnableConnectionCache.ToString()));

                            dictionary.Add(ServiceKeyEnum.LogLevel.ToString(), (string)regKey_Parameters.GetValue(ServiceKeyEnum.LogLevel.ToString()));
                            dictionary.Add(ServiceKeyEnum.MSMQUnreachableTimeout.ToString(), (string)regKey_Parameters.GetValue(ServiceKeyEnum.MSMQUnreachableTimeout.ToString()));

                            dictionary.Add(ServiceKeyEnum.ServiceAccount.ToString(), (string)regKey_Parameters.GetValue(ServiceKeyEnum.ServiceAccount.ToString()));

                            string serviceUserName = (string)regKey_Parameters.GetValue(ServiceKeyEnum.ServiceUserName.ToString());
                            string servicePwd = (string)regKey_Parameters.GetValue(ServiceKeyEnum.ServicePassword.ToString());
                            if (StrFunc.IsFilled(serviceUserName))
                                dictionary.Add(ServiceKeyEnum.ServiceUserName.ToString(), serviceUserName);
                            if (StrFunc.IsFilled(servicePwd))
                                dictionary.Add(ServiceKeyEnum.ServicePassword.ToString(), servicePwd);
                        }
                    }
                    #endregion SubKey Parameters information

                    #region Others information
                    string imagePath = dictionary[ServiceKeyEnum.ImagePath.ToString()];
                    string serviceEnum = dictionary[ServiceKeyEnum.ServiceEnum.ToString()];
                    if (StrFunc.IsFilled(imagePath))
                    {
                        int posPath = imagePath.LastIndexOf(@"\");
                        int posExe = imagePath.LastIndexOf(".exe");
                        //
                        if ((-1 < posPath) && (-1 < posExe))
                        {
                            string pathInstall = imagePath.Substring(0, posPath);
                            string exeName = imagePath.Substring(posPath + 1, posExe - (posPath + 1));

                            dictionary.Add(ServiceKeyEnum.ExeName.ToString(), exeName);
                            dictionary.Add(ServiceKeyEnum.PathInstall.ToString(), pathInstall);
                            dictionary.Add(ServiceKeyEnum.FullName.ToString(), Path.Combine(pathInstall, exeName) + "." + "exe");
                        }
                    }
                    #endregion Others information
                }
            }
            return dictionary;
        }

        /// <summary>
        /// Recherche du 1er nom de service qui matche avec le nom de service (StartWith) et le MOMPath passé en paramètre (Equal)
        /// Cas 1: Nom du service passé en paramètre avec numéro de version complet
        /// Cas 2: Nom du service passé en paramètre avec numéro de version sans révision
        /// </summary>
        /// <param name="pServiceName">Nom du service</param>
        /// <param name="pPriorityMOMPath">MOM path (MSMQ ou FileWatcher)</param>
        /// <returns>Nom du service sélectionné</returns>
        /// EG 20130619 Service/MOMPath New
        public static string GetFirstServiceNameWithSameMOMPath(string pServiceName, string pPriorityMOMPath)
        {
            RegistryKey currentControlSet = null;
            try
            {
                string _serviceName = pServiceName;
                // Ouverture de la clé de registre des services dans CurrentControlSet 
                currentControlSet = Registry.LocalMachine.OpenSubKey(RegistryConst.RegistryKey, true);
                string _serviceNameSelected = GetFirstServiceNameWithSameMOMPath(currentControlSet, _serviceName, pPriorityMOMPath);
                if (StrFunc.IsEmpty(_serviceNameSelected))
                {
                    // Recherche d'un service de même version (Maj.Min) mais d'une autre Revision
                    _serviceName = pServiceName.Substring(0, _serviceName.LastIndexOf("."));
                    _serviceNameSelected = GetFirstServiceNameWithSameMOMPath(currentControlSet, _serviceName, pPriorityMOMPath);
                }
                return _serviceNameSelected;
            }
            finally
            {
                if (null != currentControlSet)
                    currentControlSet.Close();
            }
        }
        /// <summary>
        /// Recherche du 1er nom de service qui matche avec le nom de service (StartWith) et le MOMPath passé en paramètre (Equal)
        /// Cas 1: Nom du service passé en paramètre avec numéro de version complet
        /// Cas 2: Nom du service passé en paramètre avec numéro de version sans révision
        /// </summary>
        /// <param name="pCurrentControlSet">Clé de registre des services (CurrentControlSet)</param>
        /// <param name="pServiceName"></param>
        /// <param name="pPriorityMOMPath">MOM path (MSMQ ou FileWatcher)</param>
        /// <returns>Nom du service sélectionné</returns>
        /// EG 20130619 Service/MOMPath New
        private static string GetFirstServiceNameWithSameMOMPath(RegistryKey pCurrentControlSet, string pServiceName, string pPriorityMOMPath)
        {
            RegistryKey regKey_Parameters = null;
            RegistryKey regKey_Service = null;
            string _serviceNameSelected = string.Empty;
            try
            {
                // Chargement de toutes les clés de services dont le nom commence par : pServiceName
                List<string> keysControlSet = pCurrentControlSet.GetSubKeyNames().Where(elem => elem.StartsWith(pServiceName)).ToList();
                keysControlSet.Sort();
                keysControlSet.Reverse();
                foreach (string key in keysControlSet)
                {
                    regKey_Service = Registry.LocalMachine.OpenSubKey(RegistryConst.RegistryKey + key, false);
                    if (null != regKey_Service)
                    {
                        regKey_Parameters = regKey_Service.OpenSubKey(RegistryConst.RegistrySubKeyParameters, false);
                        string _MOMPath = (string)regKey_Parameters.GetValue(ServiceKeyEnum.MOMPath.ToString());
                        // Contrôle equivalence MOMPath
                        if (pPriorityMOMPath == _MOMPath)
                        {
                            _serviceNameSelected = key;
                            break;
                        }
                    }
                }
                return _serviceNameSelected;
            }
            finally
            {
                if (null != regKey_Parameters)
                    regKey_Parameters.Close();
                if (null != regKey_Service)
                    regKey_Service.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <param name="pkey"></param>
        /// <returns></returns>
        public static string GetRegistryServiceInformationsKey(string pServiceName, ServiceKeyEnum pkey)
        {
            string key = pkey.ToString();
            string ret = null;
            StringDictionary dicService = GetRegistryServiceInformations(pServiceName);
            if (null != dicService || (dicService.ContainsKey(key)))
                ret = (string)dicService[key];
            return ret;
        }

        

        /// <summary>
        /// Retourne les informations MOM type, MOM patch etc pour le paramètre pProcess
        /// </summary>
        /// <param name="pProcess"></param>
        /// <returns></returns>
        /// EG 20130619 Service/MOMPath 
        public static MQueueSendInfo GetMqueueSendInfo(Cst.ProcessTypeEnum pProcess, AppInstanceService pAppInstance)
        {
            Cst.ServiceEnum serviceTarget = Cst.Process.GetService(pProcess);

            //PL 20121227 Use serviceName_WithoutInstance instead of serviceName
            string serviceTargetName = pAppInstance.ServiceName_WithoutInstance.Replace(pAppInstance.ServiceEnum.ToString(), serviceTarget.ToString());
            serviceTargetName = serviceTargetName.Replace("DEBUG", string.Empty);

            // EG 20130619 Service/MOMPath Add appInstance.MOMPath parameter
            MQueueSendInfo sendInfo = GetQueueInfo(serviceTargetName, pAppInstance.MOMPath);

            return sendInfo;
        }

        
        /// <summary>
        /// Retourne les caractéristiques du MOM d'écoute d'un service
        /// </summary>
        /// <param name="pServiceName">Nom du service</param>
        /// <param name="pMOMPath"></param>
        /// <returns></returns>
        private static MQueueSendInfo GetQueueInfo(string pServiceName, string pMOMPath)
        {
            string serviceName = pServiceName;

            MQueueSendInfo sendInfo = new MQueueSendInfo()
            {
                MOMSetting = new MOMSettings()
            };

            ServiceInfo serviceInfo = new ServiceInfo(pServiceName, pMOMPath);
            if (serviceInfo.IsLoaded)
            {
                // EG 20130619 new ServiceInfo() peut avoir modifié le nom du service d'origine on le récupère
                serviceName = serviceInfo.Service;
                sendInfo.MOMSetting.MOMType = Cst.MOM.GetMOMEnum(serviceInfo.GetInformationsKey(ServiceKeyEnum.MOMType));
                sendInfo.MOMSetting.MOMPath = serviceInfo.GetInformationsKey(ServiceKeyEnum.MOMPath);

                #if DEBUG
                //PL for debug on SVR-DB01 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                if (Environment.MachineName == "DWS-136")
                {
                    if (sendInfo.MOMSetting.MOMType == Cst.MOM.MOMEnum.Unknown)
                    {
                        sendInfo.MOMSetting.MOMType = Cst.MOM.MOMEnum.FileWatcher;
                        sendInfo.MOMSetting.MOMPath = @"C:\SpheresServices\Queue";
                    }
                }
                //PL for debug on SVR-DB01 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                #endif

                string tmp;
                tmp = serviceInfo.GetInformationsKey(ServiceKeyEnum.MOMRecoverable);
                sendInfo.MOMSetting.MOMRecoverable = StrFunc.IsEmpty(tmp) || BoolFunc.IsTrue(tmp);
                tmp = serviceInfo.GetInformationsKey(ServiceKeyEnum.MOMEncrypt);
                sendInfo.MOMSetting.MOMEncrypt = StrFunc.IsFilled(tmp) && BoolFunc.IsTrue(tmp);
            }

            // EG 20130619 Service/MOMPath Attention ServiceTools.DelimiterInstance a changé de valeur
            if ((!sendInfo.IsInfoValid) && StrFunc.ContainsIn(serviceName, RegistryConst.DelimiterInstance))
            {
                // PL 20160113 Replace ToCharArray()
                //string[] split = serviceName.Split(ServiceTools.DelimiterInstance.ToCharArray()); 
                string serviceName_WithoutInstance = serviceName.Substring(0, serviceName.LastIndexOf(RegistryConst.DelimiterInstance));
                // EG 20130619 Service/MOMPath Add pMOMPath parameter
                sendInfo = GetQueueInfo(serviceName_WithoutInstance, pMOMPath);
            }
            else if ((!sendInfo.IsInfoValid) && StrFunc.ContainsIn(serviceName, "DEBUG"))
            {
                // PL 20130205 Add this "else if" with test on "DEBUG"
                // EG 20130619 Service/MOMPath Add pMOMPath parameter
                sendInfo = GetQueueInfo(serviceName.Replace("DEBUG", string.Empty), pMOMPath);
            }
            return sendInfo;
        }

        /// <summary>
        /// Suffix use by service for FileWatcher Folder or MSMQ Queue
        /// <para>NB: currently only used by the BCS Gateway to add the MemberCode</para>
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pService"></param>
        /// <param name="pIdA_Entity"></param>
        /// <returns></returns>
        public static string GetQueueSuffix(string pConnectionString, Cst.ServiceEnum pService, int pIdA_Entity)
        {
            string ret = null;
            switch (pService)
            {
                //case Cst.ServiceEnum.SpheresIO:
                //    //PL 20101124 TEST
                //    ret = ".MC";
                //    break;
                case Cst.ServiceEnum.SpheresGateBCS:
                    #region Specific to Gateway BCS
                    //Select du MemberCode pour l'entité du user connecté
                    if (StrFunc.IsFilled(pConnectionString) && (pIdA_Entity > 0))
                    {
                        string memberCode = null;
                        IDataReader dr = null;
                        try
                        {
                            string cs = pConnectionString;
                            const string CSSMEMBERIDENT_Italy_XCCG = @"XCCG";
                            const string BIC_CCG = @"CCEGITRR";
                            #region sqlSelect
                            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
                            sqlSelect += "csm.CSSMEMBERCODE" + Cst.CrLf;
                            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.CSMID.ToString() + " csm" + Cst.CrLf;
                            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a_css on a_css.IDA=csm.IDA_CSS";
                            sqlSelect += SQLCst.AND + "a_css.BIC=@BIC" + Cst.CrLf;
                            sqlSelect += SQLCst.WHERE + "csm.IDA=@IDA";
                            sqlSelect += SQLCst.AND + "csm.CSSMEMBERIDENT=" + DataHelper.SQLString(CSSMEMBERIDENT_Italy_XCCG);
                            sqlSelect += SQLCst.AND + "csm.CSSMEMBERCODE" + SQLCst.IS_NOT_NULL;

                            DataParameters parameters = new DataParameters(new DataParameter[] { });
                            //IDA of Entity
                            parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDA), pIdA_Entity);
                            //BIC of Clearing system
                            parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.BIC), BIC_CCG);
                            QueryParameters qry = new QueryParameters(cs, sqlSelect.ToString(), parameters);
                            #endregion
                            //
                            dr = DataHelper.ExecuteDataTable(qry.Cs, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader();
                            if (dr.Read())
                                memberCode = Convert.ToString(dr["CSSMEMBERCODE"]);
                        }
                        catch (Exception) { throw; }
                        finally
                        {
                            if (null != dr)
                                dr.Close();
                        }
                        //
                        if (StrFunc.IsFilled(memberCode))
                            ret = ret + @"." + memberCode;
                    }
                    #endregion Specific to Gateway BCS
                    break;
            }
            return ServiceTools.GetQueueSuffix(pService, ret);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pService"></param>
        /// <returns></returns>
        public static string GetQueueSuffix(Cst.ServiceEnum pService)
        {
            return GetQueueSuffix(pService, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pService"></param>
        /// <param name="pSuffix"></param>
        /// <returns></returns>
        public static string GetQueueSuffix(Cst.ServiceEnum pService, string pSuffix)
        {
            string ret;
            switch (pService)
            {
                case Cst.ServiceEnum.SpheresAccountGen:
                    ret = "ACCOUNTGEN";
                    break;
                case Cst.ServiceEnum.SpheresClosingGen:
                    ret = "CLOSINGGEN";
                    break;
                case Cst.ServiceEnum.SpheresConfirmationMsgGen:
                    ret = "CONFMSGGEN";
                    break;
                case Cst.ServiceEnum.SpheresEarGen:
                    ret = "EARGEN";
                    break;
                case Cst.ServiceEnum.SpheresEventsGen:
                    ret = "EVENTSGEN";
                    break;
                case Cst.ServiceEnum.SpheresEventsVal:
                    ret = "EVENTSVAL";
                    break;
                case Cst.ServiceEnum.SpheresIO:
                    ret = "IO";
                    break;
                case Cst.ServiceEnum.SpheresInvoicingGen:
                    ret = "INVOICINGGEN";
                    break;
                case Cst.ServiceEnum.SpheresMarkToMarketGen:
                    ret = "MTMGEN";
                    break;
                case Cst.ServiceEnum.SpheresQuotationHandling:
                    ret = "QUOTHANDLING";
                    break;
                case Cst.ServiceEnum.SpheresSettlementInstrGen:
                    ret = "SIGEN";
                    break;
                case Cst.ServiceEnum.SpheresTradeActionGen:
                    ret = "TRADEACTGEN";
                    break;
                case Cst.ServiceEnum.SpheresScheduler:
                    ret = "SCHEDULER";
                    break;
                case Cst.ServiceEnum.SpheresSettlementMsgGen:
                    ret = "SETTLMSGGEN";
                    break;
                case Cst.ServiceEnum.SpheresShell:
                    ret = "SHELL";
                    break;
                case Cst.ServiceEnum.SpheresGateBCS:
                    ret = "GATEBCS";
                    break;
                case Cst.ServiceEnum.SpheresGateFIXMLEurex:
                    ret = "GATEFIXMLEUREX";
                    break;
                case Cst.ServiceEnum.SpheresResponse:
                    ret = "RESPONSE";
                    break;
                case Cst.ServiceEnum.SpheresRiskPerformance:
                    ret = "RISKPERFORMANCE";
                    break;
                case Cst.ServiceEnum.SpheresNormMsgFactory:
                    ret = "NORMMSGFACTORY";
                    break;
                case Cst.ServiceEnum.TestService:
                    ret = "TESTSERVICE";
                    break;
                case Cst.ServiceEnum.SpheresWebSession:
                    ret = "WEBSESSION";
                    break;
                case Cst.ServiceEnum.SpheresLogger:
                    ret = "LOGGER";
                    break;
                default:
                    ret = "NA";
                    break;
            }
            if (StrFunc.IsFilled(pSuffix))
                ret += pSuffix;
            return ret.ToLower();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInstaller"></param>
        /// <returns></returns>
        public static void SetDisplayNameService(ServiceInstaller pInstaller)
        {
            RegistryKey service = null;
            try
            {
                //PL 20120319 Add pIsWritable = true
                service = RegistryTools.GetRegistryKeyService(true, pInstaller.ServiceName);
                if (null != service)
                {
                    service.SetValue(ServiceKeyEnum.Description.ToString(), pInstaller.Description);
                    service.SetValue(ServiceKeyEnum.DisplayName.ToString(), pInstaller.DisplayName);
                }
            }

            finally
            {
                if (null != service)
                    service.Close();
            }
        }

        /// <summary>
        /// Set service registry values (root and parameters sub-dir)
        /// </summary>
        /// <param name="pServiceType">Service Type</param>
        /// <param name="pInstaller">
        /// Install informations (service name, description, etc) ; can be null
        /// </param>
        /// <param name="pParameters">Registry values collection</param>
        /// <param name="pAdditionalParameters">
        /// Optional parameters collection used : 
        /// 1. when pInstaller is null -> array structure: [serviceName, serviceDisplayName, serviceDescription]
        /// 2. when you want to write a different registry key -> array structure: [serviceName, serviceDisplayName, serviceDescription, local machine key path]
        /// </param>
        /// FI 20130908 [19041] alimentation des paramètres MOMRecoverable,MSMQEnableConnectionCache,Timeout
        /// Ces derniers peuvent être renseignés dans le fichier xml d'intallation des instances
        /// FI 20131016 []
        /// FI 20131106 [19139] Enregistrement des clés, ServiceAccount, ServiceUserName, ServicePassword
        public static void SetRegistryServiceInformations(
            Cst.ServiceEnum pServiceEnum, ServiceInstaller pInstaller, StringDictionary pParameters,
            params string[] pAdditionalParameters)
        {

            RegistryKey service = null;
            RegistryKey config = null;
            try
            {
                string key = string.Empty;
                string serviceName = (pInstaller != null) ? pInstaller.ServiceName : pAdditionalParameters[0];
                string serviceDisplayName = (pInstaller != null) ? pInstaller.DisplayName : pAdditionalParameters[1];
                string serviceDescription = (pInstaller != null) ? pInstaller.Description : pAdditionalParameters[2];
                string momPath = string.Empty;

                //PL 20120319 Add pIsWritable
                service = RegistryTools.GetRegistryKeyService(true, serviceName, pAdditionalParameters);

                if (null != service)
                {
                    //ServiceEnum
                    key = ServiceKeyEnum.ServiceEnum.ToString();
                    service.SetValue(key, pServiceEnum.ToString());

                    //DisplayName
                    key = ServiceKeyEnum.DisplayName.ToString();
                    service.SetValue(key, serviceDisplayName);

                    //Description
                    key = ServiceKeyEnum.Description.ToString();
                    service.SetValue(key, serviceDescription);

                    // ImagePath (Bidouille EG 20080130)
                    key = ServiceKeyEnum.ImagePath.ToString();
                    string imagePath = service.GetValue(key).ToString();
                    if (StrFunc.IsFilled(imagePath))
                    {
                        service.SetValue(key, imagePath.Replace("\"", string.Empty));
                        imagePath = service.GetValue(key).ToString();
                    }
                    //
                    config = service.CreateSubKey(RegistryConst.RegistrySubKeyParameters);
                    if (null != config)
                    {
                        //Instance
                        key = ServiceKeyEnum.Instance.ToString();
                        if (pParameters.ContainsKey(key))
                            config.SetValue(key, pParameters[key] ?? "0");

                        // PM 20200102 [XXXXX] New Log : Pas de MOM pour SpheresLogger
                        if (pServiceEnum != Cst.ServiceEnum.SpheresLogger)
                        {
                            //MOMType
                            string momType = string.Empty;
                            key = ServiceKeyEnum.MOMType.ToString();
                            if (pParameters.ContainsKey(Cst.MOM.MOMType))
                            {
                                momType = pParameters[key];
                                config.SetValue(key, momType ?? string.Empty);
                            }

                            //MOMPath
                            key = ServiceKeyEnum.MOMPath.ToString();
                            if (pParameters.ContainsKey(key))
                            {
                                momPath = MOMSettings.TranslateMOMPath(pParameters[key], Software.VersionBuild);

                                if (StrFunc.IsFilled(momPath))
                                {
                                    if (StrFunc.IsFilled(momType) &&
                                        (momType.ToUpper() == Cst.MOM.MOMEnum.FileWatcher.ToString().ToUpper()))
                                    {
                                        if (false == Directory.Exists(momPath))
                                        {
                                            string msgTitle = "{SERVICE} service installation";
                                            string msg = "Folder is not exist, it will be created." + Cst.CrLf + "[" + momPath + "]";
                                            //
                                            MessageBox.Show(msg, msgTitle.Replace("{SERVICE}",
                                                pInstaller != null ?
                                                pInstaller.ServiceName : pAdditionalParameters[0]),
                                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                    }

                                    // FI 20180416 [XXXXX] Suppression du subterfuge qui consistait à mettre un "\" lorsque momPath représent un partage réseau
                                    // Cette évolution avait été mise pour corriger un bug dans du framework puisqu'un seul "\" était présent dans momPath => Il n'est plus nécessaire
                                    //config.SetValue(key, momPath == null ? string.Empty :
                                    //        (momPath.StartsWith(@"\") ? @"\" : string.Empty) + momPath);
                                    config.SetValue(key, momPath ?? string.Empty);

                                }
                                else
                                    throw (new InstallException("Folder or Queue is mandatory"));
                            }

                            // 20091110 EG Add ActivateObserver
                            key = ServiceKeyEnum.ActivateObserver.ToString();
                            if (pParameters.ContainsKey(key))
                                config.SetValue(key, pParameters[key] != null && BoolFunc.IsTrue(pParameters[key]));

                            // FI 20131008 [19041] add MOMRecoverable
                            key = ServiceKeyEnum.MOMRecoverable.ToString();
                            if (pParameters.ContainsKey(key))
                                config.SetValue(key, pParameters[key] != null && BoolFunc.IsTrue(pParameters[key]));

                            // FI 20131008 [19041] add MSMQEnableConnectionCache
                            key = ServiceKeyEnum.MSMQEnableConnectionCache.ToString();
                            if (pParameters.ContainsKey(key))
                                config.SetValue(key, pParameters[key] != null && BoolFunc.IsTrue(pParameters[key]));

                            // FI 20131008 [19041] add MSMQUnreachableTimeout
                            key = ServiceKeyEnum.MSMQUnreachableTimeout.ToString();
                            if (pParameters.ContainsKey(key))
                                config.SetValue(key, pParameters[key] == null ? 0 : IntFunc.IntValue2(pParameters[key]));
                        }

                        // 20100506 MF
                        key = ServiceKeyEnum.ClassType.ToString();
                        if (pParameters.ContainsKey(key))
                            config.SetValue(key, pParameters[key] ?? string.Empty);

                        key = ServiceKeyEnum.Prefix.ToString();
                        if (pParameters.ContainsKey(key))
                            config.SetValue(key, pParameters[key] ?? string.Empty);

                        //FI 20131106 [19139] add
                        key = ServiceKeyEnum.ServiceAccount.ToString();
                        if (pParameters.ContainsKey(key))
                        {
                            ServiceAccount serviceAccount = (ServiceAccount)Enum.Parse(typeof(ServiceAccount), pParameters[key]);
                            config.SetValue(key, serviceAccount.ToString());

                            if (ServiceAccount.User == serviceAccount)
                            {
                                //FI 20131106 [19139] add, le user est ajouté s'il est renseigné
                                key = ServiceKeyEnum.ServiceUserName.ToString();
                                if (pParameters.ContainsKey(key))
                                {
                                    string value = pParameters[key] ?? string.Empty;
                                    config.SetValue(key, value);
                                }

                                //FI 20131106 [19139] add, le mote de passe est ajouté s'il est renseigné
                                //Le mot de passe est crypté dans la base de registre de windows®
                                key = ServiceKeyEnum.ServicePassword.ToString();
                                if (pParameters.ContainsKey(key))
                                {
                                    string value = pParameters[key] ?? string.Empty;
                                    if (StrFunc.IsFilled(value))
                                        value = Cryptography.Encrypt(value);
                                    config.SetValue(key, value);
                                }
                            }
                        }
                    }

                    // PM 20200102 [XXXXX] New Log : Pas de MOM pour SpheresLogger
                    if ((pServiceEnum != Cst.ServiceEnum.SpheresLogger) &&
                        pParameters.ContainsKey(ServiceKeyEnum.MOMType.ToString()) &&
                        pParameters.ContainsKey(ServiceKeyEnum.MOMPath.ToString()))
                    {
                        if (Cst.MOM.IsFileWatcher(pParameters[ServiceKeyEnum.MOMType.ToString()]))
                        {
                            // FI 20180416 [XXXXX] Suppression du subterfuge qui consistait à mettre un "\" lorsque momPath représent un partage réseau
                            // Cette évolution avait été mise pour corriger un bug dans du framework => Il n'est plus nécessaire
                            
                            //ServiceTools.CreateWatcherFolder(pServiceEnum,
                            //    (momPath.StartsWith(@"\") ? @"\" : string.Empty) + momPath);
                            //ServiceTools.CreateWatcherFolder(Cst.ServiceEnum.SpheresResponse,
                            //    (momPath.StartsWith(@"\") ? @"\" : string.Empty) + momPath);
                            //// FI 20140519 [19923] add folder for SpheresWebSession
                            //ServiceTools.CreateWatcherFolder(Cst.ServiceEnum.SpheresWebSession,
                            //    (momPath.StartsWith(@"\") ? @"\" : string.Empty) + momPath);

                            ServiceTools.CreateWatcherFolder(pServiceEnum, momPath);
                            ServiceTools.CreateWatcherFolder(Cst.ServiceEnum.SpheresResponse, momPath);
                            // FI 20140519 [19923] add folder for SpheresWebSession
                            ServiceTools.CreateWatcherFolder(Cst.ServiceEnum.SpheresWebSession, momPath);
                        }
                    }
                }
            }
            finally
            {
                if (null != service)
                    service.Close();
                if (null != config)
                    config.Close();
            }
        }

        // PM 20200601 [XXXXX] Déplacé dans la class RegistryTools (ACommon/Registry.cs)
        ///// <summary>
        ///// Retourne le nom du journal des évènements de windows utilisé par un Service Spheres 
        ///// <para>- soit SpheresGateways</para>
        ///// <para>- soit Spheres</para>
        ///// </summary>
        ///// <param name="pServiceName"></param>
        ///// <returns></returns>
        ///// FI 20161026 [XXXXX] Add
        //public static string GetEventLog(string pServiceName)
        //{
        //    string ret = string.Empty;

        //    if (SpheresServiceTools.IsSpheresGateService(pServiceName))
        //    {
        //        ret = Cst.SpheresGatewayEventLog;
        //    }
        //    else if (SpheresServiceTools.IsSpheresService(pServiceName))
        //    {
        //        ret = Cst.SpheresEventLog;
        //    }
        //    else
        //    {
        //        throw new NotImplementedException(StrFunc.AppendFormat("Service (name:{0}) is not valid", pServiceName));
        //    }
        //    return ret;
        //}

        ///// <summary>
        ///// Retourne le nom de la source pour écriture dans le journal des évènements de windows 
        ///// </summary>
        ///// <param name="pServiceName"></param>
        ///// <returns></returns>
        //public static string GetEventLogSource(string pServiceName)
        //{
        //    return StrFunc.AppendFormat("{0}{1}", pServiceName, Cst.EventLogSourceExtension);
        //}

        ///// <summary>
        /////  Alimentation de System\\CurrentControlSet\\Services\\eventLog
        ///// </summary>
        ///// <param name="pServiceName"></param>
        ///// FI 20161026 [XXXXX] Modify
        //public static void SetEventLogService(string pServiceName)
        //{
        //    RegistryKey sourceLog = null;
        //    RegistryKey serviceLog = null;
        //    RegistryKey service = null;

        //    try
        //    {
        //        //PL 20120319 Use pIsWritable = false
        //        service = RegistryTools.GetRegistryKeyService(false, pServiceName);
        //        string eventMessageFile = (string)service.GetValue("ImagePath");
        //        int i = eventMessageFile.LastIndexOf("\\");
        //        if (-1 < i)
        //            eventMessageFile = eventMessageFile.Substring(0, i + 1) + "SpheresServicesMessage.dll";

        //        //PL 20120319 Add pIsWritable = true
        //        service = RegistryTools.GetRegistryKeyService(true, RegistryConst.RegistrySubKeyLog);

        //        // FI 20161026 [XXXXX]  call GetEventLog(pServiceName)
        //        // EventLog 
        //        serviceLog = service.CreateSubKey(GetEventLog(pServiceName));

        //        string sourceLogName = GetEventLogSource(pServiceName);  
        //        sourceLog = serviceLog.CreateSubKey(sourceLogName);
        //        sourceLog.SetValue("EventMessageFile", eventMessageFile);
        //        sourceLog.SetValue("CategoryMessageFile", eventMessageFile);
        //        sourceLog.SetValue("CategoryCount", 4);
        //        sourceLog.SetValue("TypesSupported", 7);
        //    }
        //    finally
        //    {
        //        if (null != sourceLog)
        //            sourceLog.Close();
        //        if (null != serviceLog)
        //            serviceLog.Close();
        //        if (null != service)
        //            service.Close();
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceType"></param>
        /// <param name="pFolderWatcher"></param>
        private static void CreateWatcherFolder(Cst.ServiceEnum pServiceType, string pFolderWatcher)
        {
            DirectoryInfo dirSubInfo;
            DirectoryInfo dirInfo = new DirectoryInfo(pFolderWatcher);
            if (!dirInfo.Exists)
                dirInfo.Create();
            //
            string serviceFolderName = GetQueueSuffix(pServiceType);
            //
            dirSubInfo = new DirectoryInfo(dirInfo.FullName + @"\" + serviceFolderName);
            if (!dirSubInfo.Exists)
                dirSubInfo.Create();
            //
            dirSubInfo = new DirectoryInfo(dirInfo.FullName + @"\Progress");
            if (!dirSubInfo.Exists)
                dirSubInfo.Create();
            //
            dirSubInfo = new DirectoryInfo(dirInfo.FullName + @"\Garbage");
            if (!dirSubInfo.Exists)
                dirSubInfo.Create();
            //
            dirSubInfo = new DirectoryInfo(dirInfo.FullName + @"\Success");
            if (dirSubInfo.Exists)
            {
                //PL 20101124 Replace by File.Delete()
                //dirSubInfo.Delete(true);
                FileInfo[] files = dirSubInfo.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
                foreach (FileInfo file in files)
                    File.Delete(file.FullName);
            }
            //
            dirSubInfo = new DirectoryInfo(dirInfo.FullName + @"\Error");
            if (dirSubInfo.Exists)
            {
                //dirSubInfo.Delete(true);
                FileInfo[] files = dirSubInfo.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
                foreach (FileInfo file in files)
                    File.Delete(file.FullName);
            }
            //
            dirSubInfo = new DirectoryInfo(dirInfo.FullName + @"\Replica");
            if (dirSubInfo.Exists)
            {
                //dirSubInfo.Delete(true);
                FileInfo[] files = dirSubInfo.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
                foreach (FileInfo file in files)
                    File.Delete(file.FullName);
            }
        }
        #endregion Methods
    }
    #endregion class ServiceTools

    /// <summary>
    /// Classe chargée des lectures des informations présentes dans la registry pour un service Spheres®
    /// </summary>
    public class ServiceInfo
    {
        #region Members
        private string _service;
        // EG 20130619 Service/MOMPath Add _PriorityMOMPath 
        private readonly string _priorityMOMPath;
        private StringDictionary _dicService;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient ou définie le nom du service
        /// <para>Si définition, chargement dans la foulée des informations présentes dans la registry</para>
        /// </summary>
        public string Service
        {
            get { return _service; }
            set
            {
                _service = value;
                LoadInformationsKey();
            }
        }
        /// <summary>
        ///  Retourne true si la lecture de la registry a été opérée
        /// </summary>
        public bool IsLoaded
        {
            get { return ((null != _dicService) && (_dicService.Count > 0)); }
        }
        #endregion Members

        #region constructor
        /// <summary>
        /// Nouvelle instance
        /// </summary>
        public ServiceInfo() { }
        /// <summary>
        /// Nouvelle instance et Chargement des informations présentes dans la registry pour le service {pService} 
        /// </summary>
        /// <param name="pService"></param>
        public ServiceInfo(string pService) :
            this(pService, null) { }
        /// <summary>
        /// Nouvelle instance et Chargement des informations présentes dans la registry pour le service {pService} 
        /// </summary>
        /// <param name="pService"></param>
        /// <param name="pPriorityMOMPath"></param>
        /// EG 20130619 Service/MOMPath Add PriorityMOMPath 
        public ServiceInfo(string pService, string pPriorityMOMPath)
        {
            _service = pService;
            _priorityMOMPath = pPriorityMOMPath;
            LoadInformationsKey();
        }
        #endregion constructor

        #region Methods
        /// <summary>
        ///  Charge les informations présentes dans la registry
        /// </summary>
        /// EG 20130619 Service/MOMPath Add GetFirstServiceNameWithSameMOMPath
        public void LoadInformationsKey()
        {
            if (StrFunc.IsFilled(_priorityMOMPath))
            {
                string _newService = ServiceTools.GetFirstServiceNameWithSameMOMPath(_service, _priorityMOMPath);
                if (StrFunc.IsFilled(_newService))
                    _service = _newService;
            }
            _dicService = ServiceTools.GetRegistryServiceInformations(_service);
        }

        /// <summary>
        /// Retourne la valeur associée à la clé  {pkey}
        /// <para>Retourne null si la clé n'existe pas</para>
        /// </summary>
        /// <param name="pkey"></param>
        /// <returns></returns>
        public string GetInformationsKey(ServiceKeyEnum pkey)
        {
            string key = pkey.ToString();
            string ret = null;

            if (IsLoaded && (_dicService.ContainsKey(key)))
                ret = (string)_dicService[key];
            return ret;
        }

        /// <summary>
        ///  Retourne la version {Major.Minor.Revision}
        ///  <para>Evalution de la version via la clé Prefix</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20161020 [XXXXX] Add Method
        /// FI 20161214 [21916] Modify
        public string GetVersion()
        {
            string prefix = GetInformationsKey(ServiceKeyEnum.Prefix);

            if (StrFunc.IsEmpty(prefix))
            {
                throw new NotSupportedException(
                    StrFunc.AppendFormat("Registry key (Id:{0}) doesn't exist on service {1}", ServiceKeyEnum.Prefix.ToString(), this._service));
            }

            Regex regex = new Regex(@"^[a-z]*v(\d+.\d+.\d+)$", RegexOptions.IgnoreCase);

            MatchCollection matchCol = regex.Matches(prefix);
            if (matchCol.Count == 0)
                throw new NotSupportedException(StrFunc.AppendFormat("prefix format :{0} is not valid", prefix));

            string ret = matchCol[0].Groups[1].Value;

            return ret;
        }
        #endregion Methods
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq; 
using System.Reflection;
using System.Runtime.Remoting;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
 //
using EFS.ACommon;
using EFS.Common;
using EFS.SpheresService;

namespace EFS.SpheresServiceParameters
{
    public static class SpheresServiceParametersHelper
    {
        #region Enums
        /// <summary>
        /// Available modality to interact with the service configuration parameters set
        /// </summary>
        public enum ConfigurationMode
        {
            /// <summary>
            /// used to write the service configuration for a brand new service
            /// </summary>
            Write,
            /// <summary>
            /// used to read the service configuration for an existence service
            /// </summary>
            Read,
            /// <summary>
            /// used to change the service configuration for an existence service or 
            /// to write the first service instance configuration during the installation process
            /// </summary>
            Update
        }
        public enum ServiceStatePreRemove
        {
            Stop,
            Pending,
            Remove,
        }

        /// <summary>
        /// source type where you can find the default service parameters
        /// </summary><
        /// <remarks>actually the registry (local machine section) is the only supported source type</remarks>
        enum SourceParametersType
        {
            /// <summary>
            /// windows registry, local machine key
            /// </summary>
            REGKEY_HKLM,
            //FILE,
            //DATABASE
        }
        #endregion Enums

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rdbmsName"></param>
        /// <param name="serverName"></param>
        /// <param name="databaseName"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <returns></returns>
        /// FI 20131010 add SQLSRV2K12 et ORACL11G
        /// FI 20170731 [XXXXX] Modify
        public static string GetConnectionString(string rdbmsName, string serverName, string databaseName, string userName, string passWord)
        {
            string ret;
            switch (rdbmsName)
            {
                case "SQLSRV2K8":
                case "SQLSRV2K12":
                case "SQLSRV2K16": //add FI 20170731 [XXXXX] 
                    ret =
                        String.Format("Data Source={0};Initial Catalog={1};Persist Security Info=False;User Id={2};Password={3};Workstation Id=127.0.0.1;Packet Size=4096"
                        , serverName, databaseName, userName, passWord);
                    break;
                case "ORACL10G":
                case "ORACL11G":
                case "ORACL12C": //add FI 20170731 [XXXXX] 
                    ret = String.Format("Data Source={0};User ID={1};Password={2};",
                        serverName, userName, passWord);
                    break;

                default:
                    throw new NotImplementedException(String.Format("RDBMS {0} is not implemented", rdbmsName));
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        public static Assembly GetServiceAssembly(ServiceInfo serviceInfo)
        {
            string fullPath = serviceInfo.GetInformationsKey(ServiceKeyEnum.FullName);
            Assembly ret = Assembly.ReflectionOnlyLoadFrom(fullPath);
            return ret;
        }

        public static Dictionary<string, object> GetServiceProperties(string pServiceName, string strServiceClassType)
        {
            Dictionary<string, object> retProperties = null;

            ServiceInfo serviceInfo = new ServiceInfo(pServiceName);//service.ServiceName

            ISpheresServiceParameters serviceParametersInterface = OpenServiceInstance(serviceInfo, out AppDomain domainInstall);

            if (serviceParametersInterface != null)
                retProperties = new Dictionary<string, object>(serviceParametersInterface.ServiceProperties);

            CloseServiceInstance(domainInstall);

            return retProperties;
        }

        public static ServiceStatePreRemove CanRemove(ServiceControllerStatus currentState)
        {
            ServiceStatePreRemove res = ServiceStatePreRemove.Pending;

            switch (currentState)
            {
                case ServiceControllerStatus.Paused:
                case ServiceControllerStatus.Running:
                    res = ServiceStatePreRemove.Stop;
                    break;
                case ServiceControllerStatus.StartPending:
                case ServiceControllerStatus.PausePending:
                case ServiceControllerStatus.ContinuePending:
                case ServiceControllerStatus.StopPending:
                    break;
                case ServiceControllerStatus.Stopped:
                default:
                    res = ServiceStatePreRemove.Remove;
                    break;
            }

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static bool ImplementsISpheresServiceParameters(string pServiceName)
        {
            ServiceInfo serviceInfo = new ServiceInfo(pServiceName);

            Assembly serviceAssembly = null;
            try
            {
                serviceAssembly = SpheresServiceParametersHelper.GetServiceAssembly(serviceInfo);
            }
            catch { }

            ISpheresServiceParameters serviceParametersInterface = null;

            if (serviceAssembly != null)
            {
                try
                {
                    serviceParametersInterface = OpenServiceInstance(serviceInfo, out AppDomain domainInstall);

                    SpheresServiceParametersHelper.CloseServiceInstance(domainInstall);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }

            return (serviceParametersInterface != null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formParameters"></param>
        /// <returns></returns>
        public static Dictionary<string, object> StartModalConfiguration(List<BaseFormParameters> formParameters)
        {
            List<Dictionary<string, object>> groupParameters = new List<Dictionary<string, object>>();

            for (int indexForm = 0; indexForm < formParameters.Count; indexForm++)
            {
                BaseFormParameters form = formParameters[indexForm];

                DialogResult res = form.ShowDialog();

                switch (res)
                {

                    case DialogResult.Cancel:
                        if (form.GoBack)
                        {
                            indexForm -= 2;
                            groupParameters.RemoveAt(groupParameters.Count - 1);
                        }
                        else
                        {
                            indexForm = formParameters.Count;
                            groupParameters = new List<Dictionary<string, object>>();
                        }
                        break;

                    case (DialogResult.OK):
                    default:
                        groupParameters.Add(form.Parameters);
                        break;
                }
            }

            Dictionary<string, object> retParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);


            foreach (Dictionary<string, object> group in groupParameters)
            {
                foreach (KeyValuePair<string, object> pair in group)
                {
                    retParameters.Add(pair.Key, pair.Value);
                }
            }

            return retParameters;
        }

        /// <summary>
        /// Fill the forms list values restoring a previous configuration
        /// </summary>
        /// <param name="service">primary source to obtain the default values</param>
        /// <param name="serviceInfo">secondary source to obtain the default values 
        /// <remarks>if the primary source is specified the secondary source will not be considered</remarks>
        /// </param>
        /// <param name="formParameters">form collection filled with the collected values</param>
        /// <param name="paramSourceElements">support parameters used in case the secondary source is specified.
        /// array structure: [type string;full path string]</param>
        /// <returns>true if the given source contains any value, false if the source is empty</returns>
        public static bool SetDefaultvalues(ServiceController service, ServiceInfo serviceInfo, List<BaseFormParameters> formParameters, params string[] paramSourceElements)
        {
            bool foundDefaultInstallationData = true;

            StringDictionary installationData = null;

            // case 1:  set the default form values using the the actual registry key values of the service
            if (service != null)

                installationData = ServiceTools.GetRegistryServiceInformations(service.ServiceName);

            // case 2: set the default form values using the specified source (paramSourceElements)
            else if (serviceInfo != null && paramSourceElements != null && paramSourceElements.Length == 2)
            {
                //  do not catch the exception
                SourceParametersType actualType = (SourceParametersType)Enum.Parse(typeof(SourceParametersType), paramSourceElements[0]);

                switch (actualType)
                {
                    case SourceParametersType.REGKEY_HKLM:

                        installationData = ServiceTools.GetRegistryServiceInformations(
                            serviceInfo.GetInformationsKey(ServiceKeyEnum.ServiceName),
                            new string[] { 
                                serviceInfo.GetInformationsKey(ServiceKeyEnum.ServiceName), 
                                serviceInfo.GetInformationsKey(ServiceKeyEnum.DisplayName), 
                                serviceInfo.GetInformationsKey(ServiceKeyEnum.Description),
                                paramSourceElements[1]
                            });

                        break;

                    //default:
                    //    throw new ArgumentOutOfRangeException("paramSourceElements",
                    //        "paramSourceElements[0] does not contain a known source type");
                    //    break;
                }

                if (installationData == null)
                    installationData = new StringDictionary();

                if (installationData.Count == 0)
                    foundDefaultInstallationData = false;

            }
            else
            {
                if (serviceInfo == null)
                    throw new ArgumentNullException("serviceInfo");

                if (paramSourceElements == null || paramSourceElements.Length != 2)
                    throw new ArgumentOutOfRangeException("paramSourceElements",
                           "paramSourceElements length not equals 2");
            }

            // Do not call the InitCollection procedure if you failed to load the data 
            // using the specified source (paramSourceElements)
            if (foundDefaultInstallationData)
                foreach (BaseFormParameters form in formParameters)
                    form.InternalCollection.InitCollection(installationData);

            return foundDefaultInstallationData;
        }

        /// <summary>
        /// Call the modal configuration for a spheres service 
        /// </summary>
        /// <remarks>do not use it INSIDE any procedure defined in the installation scope</remarks>
        /// <param name="service">The service instance wrapper you want to red/write/modify the spheres configuration</param>
        /// <param name="mode">values: Write/Read/Update</param>
        /// <returns></returns>
        public static Dictionary<string, object> CallModalConfiguration(ServiceController service, ConfigurationMode mode)
        {
            Dictionary<string, object> retParameters = null;

            ServiceInfo serviceInfo = new ServiceInfo(service.ServiceName);

            ISpheresServiceParameters serviceParametersInterface = OpenServiceInstance(serviceInfo, out AppDomain domainInstall);

            if (serviceParametersInterface != null)
            {
                List<BaseFormParameters> formParameters = serviceParametersInterface.InitModalConfiguration();

                if (mode != ConfigurationMode.Write)
                    SpheresServiceParametersHelper.SetDefaultvalues(service, null, formParameters);

                foreach (BaseFormParameters form in formParameters)
                    form.InternalCollection.Enabled = mode != ConfigurationMode.Read;

                if (formParameters.Count > 0)
                    formParameters[0].EnableBackButton = false;

                retParameters = serviceParametersInterface.StartModalConfiguration(formParameters);

            }

            CloseServiceInstance(domainInstall);

            return retParameters;
        }

        /// <summary>
        /// Call the modal configuration for a spheres service instance during its own installation process
        /// </summary>
        /// <remarks>do not use it OUTSIDE any procedure defined in the installation scope</remarks>
        /// <param name="serviceInfo">The service instance wrapper you want to red/write/modify the spheres configuration</param>
        /// <param name="mode">values: Update. No other value is permitted</param>
        /// <param name="defaultParametersSource">Support parameters used 
        /// to specify the source for the default configuration values. array structure: [type string;full path string]</param>
        /// <param name="foundDefaultValues">True if we find some default values for the service modal configuration</param>
        public static Dictionary<string, object> CallModalConfiguration(
            ServiceInfo serviceInfo,
            ConfigurationMode mode, string defaultParametersSource,
            out bool foundDefaultValues)
        {
            foundDefaultValues = true;

            Dictionary<string, object> retParameters = null;

            // get a new service object 
            ISpheresServiceParameters serviceParametersInterface = OpenServiceInstance(serviceInfo, out AppDomain domainInstall);

            if (serviceParametersInterface != null)
            {
                // Instance the necessary forms to configure the service
                List<BaseFormParameters> formParameters = serviceParametersInterface.InitModalConfiguration();

                string[] paramSourceElements = defaultParametersSource.Split(new char[] { ';' });

                // Get the default form values (IFF they have been previously saved).
                // The research of the default values is active just when we pass the paramSourceElements array
                // that includes the informations to open the data source containing the default values.
                // We are using the Update ConfigurationMode during the install procedure.
                if (mode != ConfigurationMode.Write && paramSourceElements.Length == 2)
                    foundDefaultValues = SpheresServiceParametersHelper.SetDefaultvalues(
                        null, serviceInfo, formParameters, paramSourceElements);

                foreach (BaseFormParameters form in formParameters)
                    form.InternalCollection.Enabled = mode != ConfigurationMode.Read;

                if (formParameters.Count > 0)
                    formParameters[0].EnableBackButton = false;

                // in case we did not find any default values then we start the modal configuration, otherwise we just take
                // the form parameters that we set in the SetDefaultvalues procedure.
                if (!foundDefaultValues)
                {
                    // 1 starting the modal configuration
                    retParameters = serviceParametersInterface.StartModalConfiguration(formParameters);
                }
                else
                {
                    retParameters = new Dictionary<string, object>();

                    // 2 taking the form parameters
                    foreach (BaseFormParameters form in formParameters)
                        foreach (KeyValuePair<string, object> parameter in form.Parameters)
                            retParameters.Add(parameter.Key, parameter.Value);
                }

            }
            if (null != domainInstall)
                CloseServiceInstance(domainInstall);

            return retParameters;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <param name="serviceParameters"></param>
        /// <param name="serviceProperties"></param>
        public static void WriteInstallInformation(
            string pServiceName, Dictionary<string, object> serviceParameters, Dictionary<string, object> serviceProperties)
        {

            StringDictionary parameters = new StringDictionary();

            foreach (KeyValuePair<string, object> pair in serviceParameters)
                parameters.Add(pair.Key, pair.Value.ToString());

            string displayName = SpheresServiceBase.ConstructServiceDisplayName(pServiceName);
            string description = parameters[ServiceKeyEnum.Description.ToString()];


            ServiceTools.SetRegistryServiceInformations(
                (Cst.ServiceEnum)serviceProperties["SERVICEENUM"],
                null,
                parameters,
                new string[] { pServiceName, displayName, description });

            RegistryTools.SetEventLogService(pServiceName);

            RegistryTools.AddServiceNameToImagePath(pServiceName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <param name="pServiceInfoSource"></param>
        /// <param name="serviceParameters"></param>
        public static void CallWriteInstallInformation(
            string pServiceName, ServiceInfo pServiceInfoSource, Dictionary<string, object> serviceParameters)
        {

            ServiceInfo serviceInfo = pServiceInfoSource;

            ISpheresServiceParameters serviceParametersInterface = OpenServiceInstance(serviceInfo, out AppDomain domainInstall);

            if (serviceParametersInterface != null)
                serviceParametersInterface.WriteInstallInformation(pServiceName, serviceParameters);

            if (null != domainInstall)
                CloseServiceInstance(domainInstall);
        }

        /// <summary>
        /// Write the default installation values on the given source
        /// </summary>
        /// <param name="serviceEnum">service type</param>
        /// <param name="serviceInstaller">service wrapper</param>
        /// <param name="stringDictionary">values collection to write in the source 
        /// defined in the defaultsourcepath parameter</param>
        /// <param name="defaultsourcepath">Source type and connection string. 
        /// array structure: [type string;full path string]</param>
        public static void WriteDefaulInstallationValues(
            Cst.ServiceEnum serviceEnum,
            ServiceInstaller serviceInstaller,
            StringDictionary contextParameters, string defaultsourcepath)
        {
            string[] paramSourceElements = defaultsourcepath.Split(new char[] { ';' });

            SourceParametersType actualType = (SourceParametersType)Enum.Parse(typeof(SourceParametersType), paramSourceElements[0]);

            switch (actualType)
            {
                case SourceParametersType.REGKEY_HKLM:

                    // Try to open the key.
                    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                        paramSourceElements[1], true);

                    // if the key does not exist then we create it...
                    if (key == null)
                    {
                        key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(
                                paramSourceElements[1],
                                Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);

                        // ...and we put in all the the mandatory parameters.
                        key.SetValue(ServiceKeyEnum.ImagePath.ToString(),
                            // specifying a fake exe just to satisfy the minimal data set requisites for the 
                            // GetRegistryServiceInformations procedure. 
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msiexec.exe"));
                    }

                    // closing the ky otherwise the SetRegistryServiceInformations will stuck
                    key.Close();

                    // write the defaul installation values
                    ServiceTools.SetRegistryServiceInformations(
                        serviceEnum,
                        serviceInstaller,
                        contextParameters,
                        new string[] {
                                serviceInstaller.ServiceName,
                                serviceInstaller.DisplayName,
                                serviceInstaller.Description,
                                defaultsourcepath.Split(new char[] { ';' })[1]
                                });

                    break;

                //default:
                //    throw new ArgumentOutOfRangeException("paramSourceElements",
                //        "paramSourceElements[0] does not contain a known source type");
                //    break;
            }
        }

        /// <summary>
        /// Base procedure to clear the common installation data (shared by any Spheres services) 
        /// </summary>
        /// <remarks>It is usually called by any service extended unistallation procedure</remarks>
        /// <param name="pServiceName">Name of the service instance</param>
        public static void DeleteInstallInformation(string pServiceName)
        {
            ServiceTools.DeleteParametersService(pServiceName);
            RegistryTools.DeleteEventLogService(pServiceName);
        }

        /// <summary>
        /// Call the service instance procedure that clears the installation data
        /// </summary>
        /// <param name="strServiceName">Name of the service instance</param>
        public static void CallDeleteInstallInformation(string pServiceName)
        {
            ServiceInfo serviceInfo = new ServiceInfo(pServiceName);

            ISpheresServiceParameters serviceParametersInterface = OpenServiceInstance(serviceInfo, out AppDomain domainInstall);

            if (serviceParametersInterface != null)
            {
                serviceParametersInterface.DeleteInstallInformation(pServiceName);
            }
            else
            {
                string message = String.Format("The service {0} is unloadable, the whole set of installation data could not be deleted",
                    pServiceName);
                Trace.WriteLine(message, "Warning");
                MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                SpheresServiceParametersHelper.DeleteInstallInformation(pServiceName);
            }

            if (null != domainInstall)
                CloseServiceInstance(domainInstall);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="parameters"></param>
        public static void CallUpdateSectionConfig(string serviceName, Dictionary<string, object> parameters)
        {
            ServiceInfo serviceInfo = new ServiceInfo(serviceName);
            string serviceFullPath = serviceInfo.GetInformationsKey(ServiceKeyEnum.FullName);

            ISpheresServiceParameters serviceParametersInterface = OpenServiceInstance(serviceInfo, out AppDomain domainInstall);

            if (serviceParametersInterface != null)
            {
                StringDictionary tempDictionary = new StringDictionary();
                foreach (KeyValuePair<string, object> pair in parameters)
                    tempDictionary.Add(pair.Key, pair.Value.ToString());

                serviceParametersInterface.UpdateSectionConfig(serviceFullPath, tempDictionary);
            }

            if (null != domainInstall)
                CloseServiceInstance(domainInstall);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceNamePrefix"></param>
        public static void DeleteAllInstances(string pServiceNamePrefix)
        {
            ServiceController[] services = ServiceController.GetServices(Environment.MachineName);

            foreach (ServiceController service in services.Where(x => SpheresServiceTools.IsSpheresService(x.ServiceName)))
            {
                ServiceInfo serviceInfo = new ServiceInfo(service.ServiceName);
                string prefix = serviceInfo.GetInformationsKey(ServiceKeyEnum.Prefix);

                if (prefix == pServiceNamePrefix)
                {
                    SpheresServiceParametersHelper.CallDeleteInstallInformation(service.ServiceName);
                    DeleteService(service.ServiceName);
                }
            }
        }

        /// <summary>
        ///  Création des fichiers START_SPHERESSERVICES et STOP_SPHERESSERVICES (fichiers bat et ps1).
        ///  <para>Si le fichier existe déjà, son contenu est remplacé</para>
        /// </summary>
        /// <param name="pPath">Répertoire où sont installés les services, les fichiers bat et ps1 seront placés dans ce répertoire</param>
        /// <param name="pVersion">Major.Minor.Revision</param>
        /// FI 20161020 [XXXXX] Add Method
        /// PL 20181031 Remove Batch START_SPHERESSERVICES (Pb Performance...)
        /// EG 20181128 Remove Batch STOP_SPHERESSERVICES (Pb Installation via VS...)
        /// PM 20201119 [XXXXX] Génération systématique du fichier complet afin de pouvoir gérer le démarrage du Logger en premier et arret en dernier
        ///public static void StartStopToolsAdd(string pPath, string pVersion, string pServiceNamePrefix)
        public static void StartStopToolsAdd(string pPath, string pVersion)
        {
            //StreamWriter writerStart_bat = null;
            //StreamWriter writerStop_bat = null;
            StreamWriter writerStart_ps1 = null;
            StreamWriter writerStop_ps1 = null;

            try
            {
                String[] file = StartStopToolsGetFiles(pPath);

                ServiceController[] services = ServiceController.GetServices(Environment.MachineName);

                //foreach (ServiceController service in services.Where(x => SpheresServiceTools.IsSpheresService(x.ServiceName)))
                //{
                //    ServiceInfo serviceInfo = new ServiceInfo(service.ServiceName);
                //    string prefix = serviceInfo.GetInformationsKey(ServiceKeyEnum.Prefix);
                //    string version = serviceInfo.GetVersion();

                //    Boolean isAdd = (version == pVersion);
                //    if (isAdd && StrFunc.IsFilled(pServiceNamePrefix))
                //        isAdd = (prefix == pServiceNamePrefix);

                //    if (isAdd)
                //    {
                //        //bat (command dos)
                //        //if (null == writerStart_bat)
                //        //    writerStart_bat = File.AppendText(file[0]);
                //        //writerStart_bat.WriteLine(StrFunc.AppendFormat("sc start {0}", service.ServiceName));

                //        //if (null == writerStop_bat)
                //        //    writerStop_bat = File.AppendText(file[1]);
                //        //writerStop_bat.WriteLine(StrFunc.AppendFormat("sc stop {0}", service.ServiceName));

                //        //PS (Powershell)
                //        if (null == writerStart_ps1)
                //        {
                //            writerStart_ps1 = File.AppendText(file[0]);
                //        }
                //        writerStart_ps1.WriteLine(StrFunc.AppendFormat("Start-Service {0}", service.ServiceName));

                //        if (null == writerStop_ps1)
                //        {
                //            writerStop_ps1 = File.AppendText(file[1]);
                //        }
                //        // PM 20200102 [XXXXX] New Log: Ajout del'option -Force pour forcer l'arrêt même si le service à des dépendnaces (c'est le cas mainenant: dépendance à SpheresLogger)
                //        //writerStop_ps1.WriteLine(StrFunc.AppendFormat("Stop-Service {0}", service.ServiceName));
                //        writerStop_ps1.WriteLine(StrFunc.AppendFormat("Stop-Service -Force {0}", service.ServiceName));
                //    }
                //}

                // PM 20201119 [XXXXX] Démarrage de SpheresLogger en premier et arret en dernier
                // Prendre tous les services Spheres
                IEnumerable<ServiceController> spheresServices = services.Where(s => SpheresServiceTools.IsSpheresService(s.ServiceName));
                // Garder les services de la bonne version
                spheresServices = spheresServices.Where(s =>
                {
                    bool isOK;

                    ServiceInfo serviceInfo = new ServiceInfo(s.ServiceName);
                    // FI 20210719 [XXXXX] Add Test existance de Prefix avant l'appel à GetVersion
                    // Prefix peut être absent (cas très particulier où un service fonctionne tjs alors qu'il a été désinstallé, il est présent dans la varables services[]).
                    // Spheres ne considère que les services Spheres installés dont la version est égale pVersion
                    string version = Cst.NotAvailable;
                    if (StrFunc.IsFilled(serviceInfo.GetInformationsKey(ServiceKeyEnum.Prefix)))
                        version = serviceInfo.GetVersion();
                    isOK = (version == pVersion);
                    
                    if (isOK)
                    {
                        // FI 20210719 [XXXXX] Add test sur PathInstall
                        // Spheres ne considère que les services Spheres installés sur pPath
                        string path = Cst.NotAvailable;
                        if (StrFunc.IsFilled(serviceInfo.GetInformationsKey(ServiceKeyEnum.PathInstall)))
                            path = serviceInfo.GetInformationsKey(ServiceKeyEnum.PathInstall);
                        isOK = (path == pPath);
                    }

                    return isOK;
                }
                );
                // Prendre le service Logger uniquement
                IEnumerable<ServiceController> spheresServicesLogger = spheresServices.Where(s => SpheresServiceTools.IsSpheresLoggerService(s.ServiceName));
                // Garder tous les services spheres à part le service Logger
                spheresServices = spheresServices.Where(s => (false == SpheresServiceTools.IsSpheresLoggerService(s.ServiceName)));

                // Ecraser le contenu des fichiers
                writerStart_ps1 = File.CreateText(file[0]);
                writerStop_ps1 = File.CreateText(file[1]);

                // FI 20210719 [XXXXX] Lecture du copyrightAttribute sur l'assembly
                AssemblyCopyrightAttribute copyrightAttribute = AssemblyTools.GetAssemblyAttribute<AssemblyCopyrightAttribute>(Assembly.GetExecutingAssembly());
                if (null != copyrightAttribute)
                {
                    writerStart_ps1.WriteLine($"#Copyright{copyrightAttribute.Copyright}");
                    writerStop_ps1.WriteLine($"#Copyright{copyrightAttribute.Copyright}");
                }

                foreach (ServiceController service in spheresServicesLogger)
                {
                    writerStart_ps1.WriteLine(StrFunc.AppendFormat("Start-Service {0}", service.ServiceName));
                }
                foreach (ServiceController service in spheresServices.OrderBy(x => x.ServiceName))
                {
                    writerStart_ps1.WriteLine(StrFunc.AppendFormat("Start-Service {0}", service.ServiceName));
                    writerStop_ps1.WriteLine(StrFunc.AppendFormat("Stop-Service -Force {0}", service.ServiceName));
                }
                foreach (ServiceController service in spheresServicesLogger)
                {
                    writerStop_ps1.WriteLine(StrFunc.AppendFormat("Stop-Service -Force {0}", service.ServiceName));
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                // PL 20181031 Remove Batch START_SPHERESSERVICES (Pb Performance...)
                //if (null != writerStart_bat)
                //{
                //    writerStart_bat.Flush();
                //    writerStart_bat.Close();
                //    writerStart_bat.Dispose();
                //}
                //if (null != writerStop_bat)
                //{
                //    writerStop_bat.Flush();
                //    writerStop_bat.Close();
                //    writerStop_bat.Dispose();
                //}

                if (null != writerStart_ps1)
                {
                    writerStart_ps1.Flush();
                    writerStart_ps1.Close();
                    writerStart_ps1.Dispose();
                }
                if (null != writerStop_ps1)
                {
                    writerStop_ps1.Flush();
                    writerStop_ps1.Close();
                    writerStop_ps1.Dispose();
                }
            }
        }

        /// <summary>
        ///  Suppression des fichiers START_SPHERESSERVICES et STOP_SPHERESSERVICES
        /// </summary>
        /// <param name="pPath">Chemin d'accès</param>
        /// <param name="version"></param>
        /// FI 20161020 [XXXXX] Add Method 
        public static void StartStopToolsDel(string pPath)
        {
            String[] file = StartStopToolsGetFiles(pPath);
            foreach (string fileItem in file)
            {
                if (System.IO.File.Exists(fileItem))
                {
                    System.IO.File.Delete(fileItem);
                }
            }
        }

        /// <summary>
        /// Verify if a service already exists with the same name
        /// </summary>
        /// <param name="strServiceName">String name for existence check</param>
        /// <returns>true if exists</returns>
        public static bool ServiceExists(string strServiceName)
        {
            bool ret = false;

            ServiceController[] services = ServiceController.GetServices(Environment.MachineName);

            foreach (ServiceController service in services)
            {
                if (strServiceName == service.ServiceName)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// suppression du service à l'aide de l'outil SC (Sc.exe) inclus dans le Kit de ressources. (http://support.microsoft.com/kb/251192/fr)
        /// </summary>
        /// <param name="pServiceName"></param>
        public static void DeleteService(string pServiceName)
        {
            DeleteService(pServiceName, out _, out _);
        }

        /// <summary>
        /// suppression du service à l'aide de l'outil SC (Sc.exe) inclus dans le Kit de ressources. (http://support.microsoft.com/kb/251192/fr)
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <param name="pStandardOutput"></param>
        public static void DeleteService(string pServiceName, out int pExitCode, out string pStandardOutput)
        {
            pExitCode = 0;
            pStandardOutput = null;
            System.Diagnostics.Process scProcess = null;
            try
            {
                scProcess = new System.Diagnostics.Process();
                scProcess.StartInfo.CreateNoWindow = true;
                scProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                scProcess.StartInfo.UseShellExecute = false;
#if !DEBUG
                        scProcess.StartInfo.RedirectStandardOutput = true;
                        scProcess.StartInfo.RedirectStandardError = true;
#endif
                scProcess.StartInfo.FileName = "sc ";
                scProcess.StartInfo.Arguments = String.Format("delete {0}", pServiceName);
                scProcess.Start();
                scProcess.WaitForExit(30000);

                pExitCode = scProcess.ExitCode;
                if (scProcess.StartInfo.RedirectStandardOutput && false == scProcess.StartInfo.UseShellExecute)
                {
                    if (null != scProcess.StandardOutput)
                        pStandardOutput = scProcess.StandardOutput.ReadToEnd();
                }
            }
            catch { throw; }
            finally
            {
                if (null != scProcess)
                    scProcess.Close();
            }
        }

        /// <summary>
        /// Création du service à l'aide de l'outil SC (Sc.exe) inclus dans le Kit de ressources. (http://support.microsoft.com/kb/251192/fr)
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <param name="pDisplayName"></param>
        /// <param name="pServiceFullPath"></param>
        /// <param name="pServiceAccount">Type de compte windows</param>
        /// <param name="pServiceUserName">compte windows. Ce paramètre est nécessaire lorsque le type de compte est User</param>
        /// <param name="pServicePwd">mot de passe du compte windows. Ce paramètre est nécessaire lorsque le type de compte est User</param>
        /// <param name="pDependService">Nom du service dont le service dépendra</param>
        /// <param name="pExitCode"></param>
        /// <param name="pStandardOutput"></param>
        /// FI 20131106 [19139] Add parameters pServiceAccount,pServiceUserName,pServicePwd
        /// PM 20200102 [XXXXX] New Log: ajout du paramètre pDependService
        //public static void CreateService(string pServiceName, string pDisplayName, string pServiceFullPath, ServiceAccount pServiceAccount, string pServiceUserName, string pServicePwd, 
        //                                 out int pExitCode, out string pStandardOutput)
        public static void CreateService(string pServiceName, string pDisplayName, string pServiceFullPath,
            ServiceAccount pServiceAccount, string pServiceUserName, string pServicePwd, string pDependService,
            out int pExitCode, out string pStandardOutput)

        {
            pExitCode = 0;
            pStandardOutput = null;
            System.Diagnostics.Process scProcess = new System.Diagnostics.Process();
            try
            {
                scProcess.StartInfo.CreateNoWindow = true;
                scProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                scProcess.StartInfo.UseShellExecute = false;
#if !DEBUG
                        scProcess.StartInfo.RedirectStandardOutput = true;
                        scProcess.StartInfo.RedirectStandardError = true;
#endif
                //Warning: La syntaxe impose l'usage d'un espace après le signe équal de chaque paramètre.
                scProcess.StartInfo.FileName = "sc.exe ";

                // PM 20200102 [XXXXX] New Log: ajout gestion dépendance du service
                string dependOption = string.Empty;
                if (StrFunc.IsFilled(pDependService))
                {
                    dependOption = string.Format(" depend= \"{0}\"", pDependService);
                }

                // FI 20131106 [19139] les arguments sont fonction du type de compte
                switch (pServiceAccount)
                {
                    case ServiceAccount.LocalSystem:
                        scProcess.StartInfo.Arguments = String.Format("create {0} binpath= \"{1}\" start= {2} displayname= \"{3}\" {4}",
                        pServiceName,
                        pServiceFullPath,
                        "auto",
                        pDisplayName,
                        dependOption);
                        break;
                    case ServiceAccount.LocalService:
                    case ServiceAccount.NetworkService:
                        string obj = @"NT AUTHORITY\" + pServiceAccount.ToString() ;
                        scProcess.StartInfo.Arguments = String.Format("create {0} binpath= \"{1}\" start= {2} displayname= \"{3}\" obj= \"{4}\" {5}",
                        pServiceName,
                        pServiceFullPath,
                        "auto",
                        pDisplayName,
                        obj,
                        dependOption);
                        break;
                    case ServiceAccount.User:
                        scProcess.StartInfo.Arguments = String.Format("create {0} binpath= \"{1}\" start= {2} displayname= \"{3}\" obj= \"{4}\" password= \"{5}\" {6}",
                        pServiceName,
                        pServiceFullPath,
                        "auto",
                        pDisplayName,
                        pServiceUserName,
                        pServicePwd,
                        dependOption);
                        break;
                }

                scProcess.Start();
                scProcess.WaitForExit(30000);

                pExitCode = scProcess.ExitCode;
                if (scProcess.StartInfo.RedirectStandardOutput && false == scProcess.StartInfo.UseShellExecute)
                {
                    if (null != scProcess.StandardOutput)
                        pStandardOutput = scProcess.StandardOutput.ReadToEnd();
                }
            }
            catch { throw; }
            finally
            {
                if (null != scProcess)
                    scProcess.Close();
            }
        }

        /// <summary>
        /// Recherche le nom du service SpherresLogger dont la version est en paramètre
        /// </summary>
        /// <param name="pServiceVersion">Version du service recherché</param>
        /// <returns>default si le service n'a pas été trouvé</returns>
        // PM 20200102 [XXXXX] New Log: Add method
        public static string GetLoggerServiceName(string pServiceVersion)
        {
            string loggerServiceName = default;
            ServiceController[] services = ServiceController.GetServices(Environment.MachineName);

            foreach (ServiceController service in services.Where(s => SpheresServiceTools.IsSpheresLoggerService(s.ServiceName)))
            {
                ServiceInfo serviceInfo = new ServiceInfo(service.ServiceName);
                string version = serviceInfo.GetVersion();
                if (version == pServiceVersion)
                {
                    loggerServiceName = service.ServiceName;
                    break;
                }
            }
            return loggerServiceName;
        }

        /// <summary>
        /// Ajoute une dépendance du service au service SpherresLogger
        /// </summary>
        /// <returns></returns>
        // PM 20200102 [XXXXX] New Log: Add method
        public static int AddLoggerReference(string pServiceName, out string pStandardOutput)
        {
            int exitCode = 0;
            pStandardOutput = default;
            // PM 20210531 [XXXXX] Pas de Logger pour les Gateways
            //if (false == SpheresServiceTools.IsSpheresLoggerService(pServiceName))
            if ((false == SpheresServiceTools.IsSpheresLoggerService(pServiceName)) && (false == SpheresServiceTools.IsSpheresGateService(pServiceName)))
            {
                ServiceInfo serviceInfo = new ServiceInfo(pServiceName);
                string version = serviceInfo.GetVersion();
                string serviceLoggerName = SpheresServiceParametersHelper.GetLoggerServiceName(version);

                if (serviceLoggerName != default)
                {
                    System.Diagnostics.Process scProcess = new System.Diagnostics.Process();

                    try
                    {
                        scProcess.StartInfo.CreateNoWindow = true;
                        scProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                        scProcess.StartInfo.UseShellExecute = false;
#if !DEBUG
                        scProcess.StartInfo.RedirectStandardOutput = true;
                        scProcess.StartInfo.RedirectStandardError = true;
#endif

                        scProcess.StartInfo.FileName = "sc.exe ";
                        scProcess.StartInfo.Arguments = String.Format("config {0} depend={1}", pServiceName, serviceLoggerName);

                        scProcess.Start();
                        scProcess.WaitForExit(30000);

                        exitCode = scProcess.ExitCode;
                        if (scProcess.StartInfo.RedirectStandardOutput && false == scProcess.StartInfo.UseShellExecute)
                        {
                            if (null != scProcess.StandardOutput)
                            {
                                pStandardOutput = scProcess.StandardOutput.ReadToEnd();
                            }
                        }
                    }
                    catch { throw; }
                    finally
                    {
                        if (null != scProcess)
                        {
                            scProcess.Close();
                        }
                    }
                }
            }
            return exitCode;
        }

        /// <summary>
        /// création d'un service à partir d'un service existant 
        /// <para>Retourne 0 si le service est créé correctement</para>
        /// <para>Retourne -1 si le service existe déjà</para>
        /// <para>Retourne une autre valeur si erreur</para>
        /// <para></para>
        /// 
        /// </summary>
        /// <param name="pServiceSource">nom du service source</param>
        /// <param name="parameters">Liste des paramètres du nouveau service (MOMTYPE,MOMPATH,etc..)</param>
        /// <param name="pStandardOutput"></param>
        /// <exception cref="Exception si le paramètre Instance est non renseigné"></exception>
        /// FI 20131008 [19041] add methode 
        /// FI 20131106 [19139]
        // PM 20210115 [XXXXX] Ajout ActivateObserver
        public static Int32 CreateServiceFromService(string pServiceSource, Dictionary<string, object> parameters,
                                                                    out string pStandardOutput)
        {
            int ret = -99;
            pStandardOutput = null;

            string instanceName = null;
            if (ArrFunc.IsFilled(parameters) && parameters.ContainsKey(ServiceKeyEnum.Instance.ToString()))
                instanceName = (string)parameters[ServiceKeyEnum.Instance.ToString()];

            if (StrFunc.IsEmpty(instanceName))
                throw new Exception(StrFunc.AppendFormat("parameter :{0} is empty", ServiceKeyEnum.Instance.ToString()));

            ServiceInfo serviceInfo = new ServiceInfo(pServiceSource);
            string serviceClassType = serviceInfo.GetInformationsKey(ServiceKeyEnum.ClassType);
            string instanceNamePrefix = serviceInfo.GetInformationsKey(ServiceKeyEnum.Prefix);
            string description = serviceInfo.GetInformationsKey(ServiceKeyEnum.Description);
            parameters.Add(ServiceKeyEnum.ClassType.ToString(), serviceClassType);
            parameters.Add(ServiceKeyEnum.Prefix.ToString(), instanceNamePrefix);
            parameters.Add(ServiceKeyEnum.Description.ToString(), description);

            // FI 20210804 [XXXXX] si MOMPath est non renseigné => récupération du path du service source
            // FI 20210804 [XXXXX] si MOMPath est renseigné => Interprétation des mots clés MAJOR() MINOR() REVISON() et BUILD() 
            if (StrFunc.IsEmpty(parameters[ServiceKeyEnum.MOMPath.ToString()].ToString()) &&
                parameters[ServiceKeyEnum.MOMType.ToString()].ToString() == serviceInfo.GetInformationsKey(ServiceKeyEnum.MOMType))
            {
                parameters[ServiceKeyEnum.MOMPath.ToString()] = serviceInfo.GetInformationsKey(ServiceKeyEnum.MOMPath);
            }
            else if (StrFunc.IsFilled(parameters[ServiceKeyEnum.MOMPath.ToString()].ToString()))
            {
                parameters[ServiceKeyEnum.MOMPath.ToString()] = MOMSettings.TranslateMOMPath(parameters[ServiceKeyEnum.MOMPath.ToString()].ToString(),
                    serviceInfo.GetVersion());
            }


            // PM 20210115 [XXXXX] Ajout ActivateObserver
            // FI 20210311 [XXXXX] Add test sur existence de ActivateObserver
            if (false == parameters.ContainsKey(ServiceKeyEnum.ActivateObserver.ToString()))
                parameters.Add(ServiceKeyEnum.ActivateObserver.ToString(), BoolFunc.IsTrue(serviceInfo.GetInformationsKey(ServiceKeyEnum.ActivateObserver)));

            // PM 20200102 [XXXXX] New Log: gestion du service de dépendance
            string dependServiceName = default;
            if (false == SpheresServiceTools.IsSpheresLoggerService(pServiceSource))
            {
                string version = serviceInfo.GetVersion();
                dependServiceName = SpheresServiceParametersHelper.GetLoggerServiceName(version);
            }

            ServiceAccount serviceAccountEnum = ServiceAccount.LocalSystem;
            if (false == parameters.ContainsKey(ServiceKeyEnum.ServiceAccount.ToString()))
            {
                // FI 20131106 [19139] Récupération des informations sur le compte windows depuis le service source si elles ne sont pas présentes ds parameters
                string serviceAccount = serviceInfo.GetInformationsKey(ServiceKeyEnum.ServiceAccount);
                // FI 20131106 [19245] add test sur StrFunc.IsFilled(serviceAccount)
                if (StrFunc.IsFilled(serviceAccount))
                {
                    serviceAccountEnum = (ServiceAccount)Enum.Parse(typeof(ServiceAccount), serviceAccount);
                    parameters.Add(ServiceKeyEnum.ServiceAccount.ToString(), serviceAccountEnum.ToString());

                    if (serviceAccountEnum == ServiceAccount.User)
                    {
                        string serviceUserName = serviceInfo.GetInformationsKey(ServiceKeyEnum.ServiceUserName);
                        string servicePassword = serviceInfo.GetInformationsKey(ServiceKeyEnum.ServicePassword);
                        if (StrFunc.IsFilled(servicePassword))
                            servicePassword = Cryptography.Decrypt(servicePassword);
                        parameters.Add(ServiceKeyEnum.ServiceUserName.ToString(), serviceUserName);
                        parameters.Add(ServiceKeyEnum.ServicePassword.ToString(), servicePassword);
                    }
                }
            }
            else
            {
                string serviceAccount = parameters[ServiceKeyEnum.ServiceAccount.ToString()].ToString();
                serviceAccountEnum = (ServiceAccount)Enum.Parse(typeof(ServiceAccount), serviceAccount);
            }

            // FI 20131106 [19139] Valorisation de userName, pwd 
            string userName = null;
            string pwd = null;
            if (serviceAccountEnum == ServiceAccount.User)
            {
                if (parameters.ContainsKey(ServiceKeyEnum.ServiceUserName.ToString()))
                    userName = parameters[ServiceKeyEnum.ServiceUserName.ToString()].ToString();
                if (parameters.ContainsKey(ServiceKeyEnum.ServicePassword.ToString()))
                    pwd = parameters[ServiceKeyEnum.ServicePassword.ToString()].ToString();
            }

            string serviceFullName = serviceInfo.GetInformationsKey(ServiceKeyEnum.FullName);

            ArrayList instanceNameList = GetListInstance(instanceName);
            foreach (string instName in instanceNameList)
            {
                parameters[ServiceKeyEnum.Instance.ToString()] = instName;

                // RD 20130613 [18747] Utiliser le clé {ServiceKeyEnum.Prefix} pour composer le nom de l'instance
                string newServiceName = SpheresServiceBase.ConstructServiceName(instanceNamePrefix, instName);
                string newDisplayName = SpheresServiceBase.ConstructServiceDisplayName(newServiceName);

                if (!SpheresServiceParametersHelper.ServiceExists(newServiceName))
                {

                    // PM 20200102 [XXXXX] New Log: ajout du paramètre dependServiceName
                    SpheresServiceParametersHelper.CreateService(newServiceName, newDisplayName, serviceFullName,
                                                                    serviceAccountEnum, userName, pwd, dependServiceName,
                                                                    out int exitCode, out string standardOutput);

                    ret = exitCode;
                    pStandardOutput = standardOutput;

                    if (0 == ret)
                    {
                        SpheresServiceParametersHelper.CallWriteInstallInformation(newServiceName, serviceInfo, parameters);
                        SpheresServiceParametersHelper.CallUpdateSectionConfig(newServiceName, parameters);
                    }
                }
                else
                {
                    ret = -1;
                }
            }
            return ret;
        }

        #region Private Methods
        /// <summary>
        ///  Retourne le nom des fichiers START_SPHERESSERVICES et STOP_SPHERESSERVICES
        /// </summary>
        /// <param name="pPath"></param>
        /// <returns></returns>
        /// FI 20161020 [XXXXX] Add Method 
        // EG 20181128 Remove Batch STOP_SPHERESSERVICES (Pb Installation via VS...)
        private static string[] StartStopToolsGetFiles(string pPath)
        {
            string startfileName = "START_SPHERESSERVICES";
            string stopfileName = "STOP_SPHERESSERVICES";

            String[] file = new String[2]
            {
                //StrFunc.AppendFormat(@"{0}\{1}.bat", pPath, startfileName),
                //StrFunc.AppendFormat(@"{0}\{1}.bat", pPath, stopfileName),
                StrFunc.AppendFormat(@"{0}\{1}.ps1", pPath, startfileName),
                StrFunc.AppendFormat(@"{0}\{1}.ps1", pPath, stopfileName),
            };

            return file;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                Assembly assembly = System.Reflection.Assembly.Load(args.Name);
                if (assembly != null)
                    return assembly;
            }
            catch { }

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            string[] Parts = args.Name.Split(',');
            string File = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + Parts[0].Trim() + ".dll";

            return Assembly.LoadFrom(File);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <param name="domainInstall"></param>
        /// <returns></returns>
        private static ISpheresServiceParameters OpenServiceInstance(ServiceInfo serviceInfo, out AppDomain domainInstall)
        {
            ISpheresServiceParameters serviceParameters = null;

            AppDomainSetup domaininfo = new AppDomainSetup
            {
                ApplicationBase = serviceInfo.GetInformationsKey(ServiceKeyEnum.PathInstall)
            };

            domainInstall = AppDomain.CreateDomain("PathInstall", null, domaininfo);

            string serviceClassType = serviceInfo.GetInformationsKey(ServiceKeyEnum.ClassType);
            string assemblyName = serviceInfo.GetInformationsKey(ServiceKeyEnum.ExeName);

            if (StrFunc.IsFilled(assemblyName) && StrFunc.IsFilled(serviceClassType))
            {

                // Need a custom resolver so we can load assembly from non current path, if the current assembly does not know
                // where to find the assemblies of the object created by the Activator.CreateInstance procedure (like the installer)
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);


                ObjectHandle handleService = null;
                try
                {
                    handleService = Activator.CreateInstance(
                        domainInstall,
                        assemblyName,
                        serviceClassType,
                        true,
                        BindingFlags.CreateInstance,
                        null,
                        null,
                        System.Globalization.CultureInfo.CurrentCulture,
                        null);

                    //new Object[] { strServiceName },
                    Object objectService = handleService.Unwrap();
                    serviceParameters = (ISpheresServiceParameters)objectService;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }

                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }

            return serviceParameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domainInstall"></param>
        private static void CloseServiceInstance(AppDomain domainInstall)
        {
            AppDomain.Unload(domainInstall);
        }

        /// <summary>
        /// Retourne une liste d'instance à partir de la donnes string {pInstance}
        /// <para>(i.e. IO 2-4 retourne une liste avec IO2 et IO4</para>
        /// </summary>
        /// <param name="pInstance"></param>
        /// <returns></returns>
        /// FI 20131011 [19041] add method 
        private static ArrayList GetListInstance(string pInstance)
        {
            ArrayList ret = new ArrayList
            {
                pInstance
            };

            string regularExpression = @"^([a-z]*)\s*(\d+)\s*-\s*(\d+)$";
            Regex regexNumber = new Regex(regularExpression, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match machcode = regexNumber.Match(pInstance);
            if (machcode.Success)
            {
                string prefix = machcode.Groups[1].Value;
                Int32 firstInst = IntFunc.IntValue2(machcode.Groups[2].Value);
                Int64 lastInst = IntFunc.IntValue2(machcode.Groups[3].Value);
                if (firstInst < lastInst)
                {
                    ret.Clear();
                    for (Int64 i = firstInst; i <= lastInst; i++)
                    {
                        ret.Add(prefix + i.ToString());
                    }
                }
            }
            return ret;
        }
        #endregion Private Methods
        #endregion Methods
    }
}

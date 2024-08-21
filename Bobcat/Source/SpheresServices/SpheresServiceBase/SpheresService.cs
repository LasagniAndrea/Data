using System;
using System.ComponentModel;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ServiceProcess;
using System.Messaging;
using System.Threading;
using System.Linq; //PL 20171220 Add
using System.Runtime.InteropServices;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.ApplicationBlocks.Data;
using EFS.Restriction;
using EFS.LoggerClient;
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;
using System.Threading.Tasks;

namespace EFS.SpheresService
{
    #region class SpheresService
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public abstract class SpheresServiceBase : ServiceBase, ISpheresServiceParameters
    {
        #region Members

        private MessageQueue _messageQueue;
        private IAsyncResult _messageQueue_AsyncResult;
        private int _messageQueue_BeginPeek_Count;
        private FileSystemWatcher _fileSystemWatcher;
        private bool _listenerStarted;

        private string _progressFileFullName;

        private MQueueBase _mQueue;
        /// <summary>
        /// ie C:\Spheres\Process\Queue or "POSTE-059\private$\spheresqueue"
        /// </summary>
        private string _MOMPathRoot;
        /// <summary>
        /// ie C:\Spheres\Process\Queue\EventsGen or "POSTE-059\private$\spheresqueueeventsgen"
        /// </summary>
        private string _MOMpath;
        private readonly bool _isRecoverable;
        private readonly bool _isEncrypt;
        private readonly int _unreachableTimeout;
        private int _countFolderNotFound;
        private int _countMOMUnreachable;
        private int _countMessage;
        private int _countSuccessfully;
        /// <summary>
        /// Drapeau qui indique qu'un message est en cours de traitement
        /// </summary>
        private bool _isProcessing;

        private readonly ServiceInfo _serviceInfo;

        /// <summary>
        ///  Identificateur du message en cours de traitement
        ///  <para>Lorsque le message provient de FileWatcher _currentMessageId = contient le nom du fichier</para>
        /// </summary>
        private string _currentMessageId;

        /// <summary>
        /// 
        /// </summary>
        protected ServiceTrace m_ServiceTrace;
        
        /// <summary>
        /// 
        /// </summary>
        private readonly Hashtable _lastFileReadTime;


        #endregion

        #region Accessors
        /// <summary>
        /// Obtient true si l'observateur de service est actif 
        /// </summary>
        protected bool ActivateObserver
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual Cst.ServiceEnum ServiceEnum
        {
            get { return Cst.ServiceEnum.NA; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected bool IsFileWatcher
        {
            get { return (Cst.MOM.MOMEnum.FileWatcher == MomTypeEnum); }
        }

        #region MsMQueue
        /// <summary>
        /// 
        /// </summary>
        protected bool IsMsMQueue
        {
            get { return (Cst.MOM.MOMEnum.MSMQ == MomTypeEnum); }
        }

        /// <summary>
        /// MSMQueue avec une adresse de type "FormatName"
        /// </summary>
        /// 20100517 PL Newness: Gestion des adresses de queue de type "FormatName"
        protected bool IsMsMQueue_FormatNameAddress
        {
            get { return (IsMsMQueue && _MOMPathRoot.ToUpper().StartsWith("FORMATNAME:")); }
        }

        /// <summary>
        /// MSMQueue de type "private$"
        /// </summary>
        protected bool IsMsMQueue_Private
        {
            get { return (IsMsMQueue && _MOMPathRoot.ToUpper().IndexOf("PRIVATE$") > 0); }
        }

        /// <summary>
        /// MSMQueue de type "public"
        /// </summary>
        protected bool IsMsMQueue_Public
        {
            get { return (!IsMsMQueue_Private); }
        }
        #endregion MsMQueue

        /// <summary>
        /// 
        /// </summary>
        private static string FilterFile
        {
            get
            {
                //StrBuilder filterFile = new StrBuilder(ProcessType.ToString());

                StrBuilder filterFile = new StrBuilder();
                filterFile += "???????????????";            //15 ? -> ProcessType (avec X à Droite)   
                filterFile += "_" + "???????????????????";	//19 ? -> System date (yyyyMMddHHmmssfffff)
                for (int i = 1; i <= 3; i++)
                {
                    filterFile += "_" + "????????";			//n*3 ? -> Trade status 
                }
                filterFile += "_*";							//IDT
                filterFile += "_*";							//IDP
                filterFile += "_*";							//IDI
                filterFile += ".xml";
                return filterFile.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected Cst.MOM.MOMEnum MomTypeEnum
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient true si lorsqu'un message (fileWatcher, MSQM) est en cous de traitement
        /// </summary>
        protected bool IsProcessing
        {
            get { return _isProcessing; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected MQueueBase MQueue
        {
            get { return _mQueue; }
        }

        #region Path
        protected static string PathProgress { get { return "Progress"; } }
        protected static string PathSuccess { get { return "Success"; } }
        protected static string PathError { get { return "Error"; } }
        protected static string PathReplica { get { return "Replica"; } }
        protected static string PathGarbage { get { return "Garbage"; } }
        #endregion Path

        protected AppInstanceService AppInstance
        {
            get;
            private set;
        }
        /// <summary>
        ///  Retourne le nom de la machine locale
        /// </summary>
        protected static string HostName
        {
            get { return System.Environment.MachineName; }
        }
        /// <summary>
        /// Obtient le nom d'utilisateur connecté à Windows
        /// </summary>
        protected static string UserName
        {
            get { return System.Environment.UserName; }
        }
        /// <summary>
        /// {ServiceName} (v{version})
        /// </summary>
        protected string ServiceLongName
        {
            get
            {

                return StrFunc.AppendFormat("{0} (v{1})", ServiceName, ServiceVersion);
            }
        }

        /// <summary>
        /// <para>Retourne la version de l'assembly courante 
        /// <para>Ne retourne pas la version de l'executable</para>
        /// <para>Major.Minor.Build (Exemple 6.0.6138)</para>
        /// </summary>
        protected static string ServiceVersion
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return StrFunc.AppendFormat("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected string Environment
        {
            get { return ServiceLongName + @" [MOM: " + MomTypeEnum.ToString() + @"] [Path: " + _MOMpath + @"]"; }
        }

        /// <summary>
        /// Retourne true si le service transfère les fichiers "similaires" au fichier courant vers le répertoire REPLICA (si ce dernier existe)
        /// <para>Un fichier est dit "similaire" s'il produit la même action</para>
        /// <para>Par exemple, cela permet d'éviter de la multiplication des envois de confirmation d'un même trade même lorsque plusieurs demandes ont été postées (plusieurs demandes ont peut-être été postées car le service n'était pas en ligne)</para>
        /// </summary>
        /// FI 20120920 [18137] la valeur par défaut est mise à false, cette fonctionnalité est désactivée
        protected virtual bool IsReplicaMsgWithFileWatcherAvailable
        {
            get
            {
                //FI 20120920 [18137] Cette fonctionnalité est à revoir 
                //Aujourd'hui cette fonctionnalité ne supprime que les fichiers qui commence par le même nom.
                //En théorie cela n'est pas possible puisque le nom généré est unique dans le temps puisqu'il contient 
                //la time de génération du message 
                //
                //L'expression de besoin identifiée dans le summary semble pourtant intéressante, il faudrait réactiver cette fonctionnalité plus tard
                //return true;
                return false;
            }
        }

        /// <summary>
        /// Permet de définir un suffixe spécifique pour le nom de la queue sur laquelle le service va se mettre en écoute
        /// </summary>
        protected virtual string SpecificSuffix
        {
            get;
            set;
        }

        /// <summary>
        /// Retourne le niveau de log pour le gestionnaire d'événement windows
        /// </summary>
        protected LogLevelDetail LogLevel
        {
            get;
            private set;
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public SpheresServiceBase() :
            this(null) { }

        /// <summary>
        /// Constructor
        ///<para>Aucune exception n'est levée, toute exception est trappée et donne lieu à un message dans le journal des évènements Windows</para>
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <param name="pLogName"></param>
        /// FI 20160804 [Migration TFS] Modify
        /// FI 20170215 [XXXXX] Modify
        // EG 20180423 Analyse du code Correction [CA2200]
        public SpheresServiceBase(string pServiceName)
        {
            try
            {

                if (StrFunc.IsFilled(pServiceName))
                    base.ServiceName = pServiceName;

                CanPauseAndContinue = true;
                
                ThreadTools.SetCurrentCulture(Cst.EnglishCulture);

                _serviceInfo = new ServiceInfo(ServiceName);

                string tmp = _serviceInfo.GetInformationsKey(ServiceKeyEnum.MSMQUnreachableTimeout);
                if (StrFunc.IsEmpty(tmp))
                    _unreachableTimeout = 60;
                else
                    _unreachableTimeout = Convert.ToInt32(tmp);

                MomTypeEnum = Cst.MOM.GetMOMEnum(_serviceInfo.GetInformationsKey(ServiceKeyEnum.MOMType));
                _MOMPathRoot = _serviceInfo.GetInformationsKey(ServiceKeyEnum.MOMPath);

                //PL 20101020 FDA 20101020 WARNING: Management of Recoverable property
                string MOMRecoverable = _serviceInfo.GetInformationsKey(ServiceKeyEnum.MOMRecoverable);
                _isRecoverable = StrFunc.IsEmpty(MOMRecoverable) || BoolFunc.IsTrue(MOMRecoverable);

                string MOMEncrypt = _serviceInfo.GetInformationsKey(ServiceKeyEnum.MOMEncrypt);
                _isEncrypt = StrFunc.IsFilled(MOMEncrypt) && BoolFunc.IsTrue(MOMEncrypt);

                // FI 20160804 [Migration TFS] pathInstall 
                string pathInstall = _serviceInfo.GetInformationsKey(ServiceKeyEnum.PathInstall);

                
                AppInstance = new AppInstanceService(HostName, ServiceEnum, ServiceName, ServiceVersion, pathInstall, _MOMPathRoot);
                AppInstance.InitilizeTraceManager();

                SpheresTraceSource spheresTrace = EFS.Common.AppInstance.TraceManager.SpheresTrace;
                // FI 20190705 [XXXXX] Ecriture dans la trace en cas de d'erreur de requête SQL
                if (null != spheresTrace)
                {
                    DataHelper.traceQueryError = EFS.Common.AppInstance.TraceManager.TraceError;
                    DataHelper.traceQueryWarning = EFS.Common.AppInstance.TraceManager.TraceWarning;
                    DataHelper.sqlDurationLimit = spheresTrace.SqlDurationLimit;
                }

                InitLogger();

                InitLogLevel();

                _lastFileReadTime = new Hashtable();

                // EG 20091110
                ActivateObserver = BoolFunc.IsTrue(_serviceInfo.GetInformationsKey(ServiceKeyEnum.ActivateObserver));

            }
            catch (Exception ex)
            {
                WriteEventLog_SystemError(null, SpheresExceptionParser.GetSpheresException("SpheresServiceBase.ctor", ex));
                throw;
            }
        }

        #endregion Constructors

        #region Methods

        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        /// FI 20221202 [XXXXX] Refactoring => Le code existant a été déplacé dans OnStop
        protected override void Dispose(bool disposing)
        {
            EFS.Common.AppInstance.TraceManager.TraceInformation(this, "Dispose service");

            // FI 20230310 [XXXXX] dispose m_ServiceTrace
            if (null != m_ServiceTrace)
                m_ServiceTrace.Dispose();

            if (null != _fileSystemWatcher)
            {
                _fileSystemWatcher.Changed -= new FileSystemEventHandler(this.OnFileSystemWatcher_Changed);
                _fileSystemWatcher.Dispose();
            }
            
            base.Dispose(disposing);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            _countMOMUnreachable = 0;
            _countFolderNotFound = 0;
            WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.START, null);
            ActivateService();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnContinue()
        {
            EFS.Common.AppInstance.TraceManager.TraceInformation(this, "Continue service");
            WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.CONTINUE, null);
            ActivateService();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnStop()
        {
            EFS.Common.AppInstance.TraceManager.TraceInformation(this, "Stop service");

            CloseServiceTrace();

            if (LogLevel >= LogLevelDetail.LEVEL4)
            {
                WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.STOP,
                            StrFunc.AppendFormat("StopService [Message: {0}][Success: {1}][MOM Unreachable: {2}][Folder Not Found: {3}]",
                                                 _countMessage, _countSuccessfully, _countMOMUnreachable, _countFolderNotFound));
            }
            else
            {
                WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.STOP, null);
            }

            BreakService();

            // FI 20221202 [XXXXX] l'équivalent de ce code était présent sous OnDispose
            try
            {
                if (null != MQueue) //Equivalent à il y a un traitement en cours
                {
                    // FI 20180314 [XXXXX] TraceTimeReset
                    EFS.Common.AppInstance.TraceManager.TraceTimeReset();
                    AppInstance.LstIRQSemaphore.Clear();

                    StopProcess();
                    
                    // Message reposté dans la queue afin d'être traité de nouveau 
                    EndProcess(_currentMessageId, ProcessStateTools.StatusEnum.PENDING);
                }
            }
            finally
            {
                TerminateProcess();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPause()
        {
            EFS.Common.AppInstance.TraceManager.TraceInformation(this, "Pause service");
            WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.BREAK, null);
            BreakService();
        }

        /// <summary>
        /// Retourne  "{pServiceEnum}"+"v"+ Assembly.Major.Major + Assembly.Major.Minor + Assembly.Build
        /// <para>Ajoute -Inst:{pInstanceName} lorsque pInstanceName est not null</para>
        /// </summary>
        /// <param name="pServiceEnum"></param>
        /// <param name="pInstanceName"></param>
        /// <returns></returns>
        public static string ConstructServiceName(Cst.ServiceEnum pServiceEnum, string pInstanceName)
        {
            Version version = GetServiceVersion();

            string ret =
                string.Format("{0}v{1}.{2}.{3}", pServiceEnum.ToString(),
                                    version.Major, version.Minor, version.Build);
            //
            if (false == string.IsNullOrEmpty(pInstanceName))
            {
                ret += string.Format("-Inst:{0}", pInstanceName);
            }

            return ret;
        }

        /// <summary>
        /// Retourne  "{pInstanceNamePrefix}"+ [Inst:{pInstanceName}]
        /// </summary>
        /// <param name="pInstanceNamePrefix"></param>
        /// <param name="pInstanceName"></param>
        /// <returns></returns>
        public static string ConstructServiceName(string pInstanceNamePrefix, string pInstanceName)
        {
            string ret = pInstanceNamePrefix;

            if (false == string.IsNullOrEmpty(pInstanceName))
                ret += string.Format("-Inst:{0}", pInstanceName);

            return ret;
        }

        /// <summary>
        /// Construit le DisplayName d'un service à partir du Name
        /// <para>ex. "SpheresIOv3.0.4567" produit "Spheres IO (v3.0.4567)"</para>
        /// <para>ex. "SpheresIOv3.0.4567-Inst:Instance" produit "Spheres IO (v3.0.4567) - Inst:Instance"</para>
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <returns></returns>
        /// FI 20200513 [XXXXX] Refactoring du fait d'un pb si l'instance contient un v
        /// Exemple SpheresEventsGenv10.0.7436-Inst:EventsGen1, on obtenait Spheres EventsGenv10.0.7436 - Inst:E (ventsGen1) 
        public static string ConstructServiceDisplayName(string pServiceName)
        {
            string ret = pServiceName;

            string serviceName = pServiceName;

            string instName = string.Empty;
            int postInstance = pServiceName.LastIndexOf("-Inst:");
            if (postInstance > 0)
            {
                serviceName = pServiceName.Substring(0, postInstance);
                instName = pServiceName.Substring(postInstance).Replace("-Inst:", string.Empty);
            }

            int pos_last_v = serviceName.LastIndexOf("v");
            if (pos_last_v > 0)
            {
                ret = ret.Substring(0, pos_last_v).Replace("Spheres", "Spheres ").Replace("Gateway", "Gateway ")
                    + " (" + serviceName.Substring(pos_last_v) + ")";
            }

            if (postInstance > 0)
            {
                ret = StrFunc.AppendFormat("{0} - Inst:{1}", ret, instName);
            }

            return ret;
        }


        /// <summary>
        /// Retourne la Version de l'assemblie.
        /// <para>NB: Cette méthode est masquée depuis la classe enfant.</para>
        /// </summary>
        /// <returns></returns>
        public static Version GetServiceVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        /// <summary>
        /// Activation du service
        /// </summary>
        protected void ActivateService()
        {
            try
            {
                // PM 20160104 [POC MUREX] Add
                if ((null == Thread.CurrentThread.Name) && StrFunc.IsFilled(AppInstance.ServiceName))
                {
                    Thread.CurrentThread.Name = AppInstance.ServiceName;
                }

                // FI 20190719 [XXXXX] 
                AppInstance.AppTraceManager.TraceInformation(this,
                     StrFunc.AppendFormat("Start service (Instance: {0}, version: {1}, Windows® Account:{2}, Machine Name:{3})", AppInstance.AppNameInstance, AppInstance.AppVersion, System.Environment.UserName, System.Environment.MachineName));

                if (EFS.Common.AppInstance.TraceManager.IsSpheresTraceAvailable)
                    Common.AppInstance.TraceManager.TraceInformation(this, $"SqlDurationLimit: {EFS.Common.AppInstance.TraceManager.SpheresTrace.SqlDurationLimit}");


                /// FI 20190822 [24861] En mode verbose, Spheres® affiche les DLL présentes dans le réperoire Root des services
                /// Ceci afin de pouvoir vérifier les LDR installés
                string Assemblies = AssemblyTools.GetAppInstanceAssemblies<string>(this.AppInstance);
                EFS.Common.AppInstance.TraceManager.TraceInformation(this, StrFunc.AppendFormat("Assemblies info:{0}{1}", Cst.CrLf, Assemblies));

                // PM 20200102 [XXXXX] New Log : pas de LoggerClient pour SpheresLogger
                // PM 20210531 [XXXXX] Pas de Logger pour les Gateways
                //if (serviceEnum != Cst.ServiceEnum.SpheresLogger)
                if ((ServiceEnum != Cst.ServiceEnum.SpheresLogger) && (false == AppInstance.IsGateway))
                    LoggerManager.Initialize((AppInstance.AppNameInstance, AppInstance.AppVersion, AppInstance.ServiceName, 1), EFS.Common.AppInstance.TraceManager);

                //CleanTemporaryDirectory
                AppInstance.CleanTemporaryDirectory();

                if (IsLoggerAvailable)
                {
                    LoggerManager.Initialize((AppInstance.AppNameInstance, AppInstance.AppVersion, AppInstance.ServiceName, 1), EFS.Common.AppInstance.TraceManager);
                }


                // PM 20200102 [XXXXX] New Log : pas de queue pour SpheresLogger
                if (ServiceEnum != Cst.ServiceEnum.SpheresLogger)
                {
                    //20101026 PL Refactoring: 
                    switch (MomTypeEnum)
                    {
                        case Cst.MOM.MOMEnum.FileWatcher:
                            ListenerFileWatcher();
                            if (ActivateObserver)
                                OpenServiceTrace();
                            
                            RecoveryFileWatcher(_fileSystemWatcher);
                            break;
                        case Cst.MOM.MOMEnum.MSMQ:
                            ListenerMsMQueue();
                            if (ActivateObserver)
                                OpenServiceTrace();

                            RecoveryMsMQueue(_messageQueue, _messageQueue_AsyncResult);
                            break;
                        default:
                            throw new NotImplementedException(MomTypeEnum.ToString() + " is not implemented");
                    }
                }
            }
            catch (Exception ex)
            {
                SpheresException2 sEx = SpheresExceptionParser.GetSpheresException(string.Empty, ex);
                WriteEventLog_SystemError(null, sEx);
                if (sEx.ProcessState.CodeReturn == Cst.ErrLevel.MOM_PATH_ERROR)
                    WriteEventLog_ListenerBreak("ActivateService");
            }
        }

        protected void BreakService()
        {
            // EG 20101213 Plus d'appel à ListenerStartStop en mode MSMQ 
            // car sinon l'arrêt du service ne fonctionne pas (EndPeek)
            if (IsFileWatcher)
            {
                ListenerStartStop(false, _fileSystemWatcher, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSpheresException"></param>
        /// FI 20130802[] Add this method => utilisée par les gateways
        protected void WriteEventLog_SystemError(SpheresException2 pSpheresException)
        {
            WriteEventLog_SystemError(null, pSpheresException);
        }
        /* FI 20200623 [XXXXX] Mise en commentaire
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMessageId"></param>
        /// <param name="pSpheresException"></param>
        protected void WriteEventLog_SystemError(string pMessageId, SpheresException pSpheresException)
        {
            Cst.ErrLevel codeReturn = pSpheresException.ProcessState.CodeReturn;
            string message = pSpheresException.Message;
            if (StrFunc.IsFilled(pSpheresException.StackTrace))
            {
                message += Cst.CrLf2 + "Stack Trace:" + Cst.CrLf + pSpheresException.StackTrace;
            }
            if (pSpheresException.IsInnerException && StrFunc.IsFilled(pSpheresException.InnerException.StackTrace))
            {
                message += Cst.CrLf2 + "Stack Trace:" + Cst.CrLf + pSpheresException.InnerException.StackTrace;
            }

            if ((null != ExceptionTools.GetFirstRDBMSException(pSpheresException)) || (codeReturn == Cst.ErrLevel.NOTCONNECTED)) // FI 20200623 [XXXXX] Utilisation de  GetFirstRDBMSException
            {
                if (codeReturn != Cst.ErrLevel.NOTCONNECTED)
                {
                    codeReturn = Cst.ErrLevel.SQL_ERROR;
                }
                WriteEventLog_SystemError(Cst.MOM.MOMEnum.Unknown, codeReturn, null, pSpheresException.Method, message);
            }
            else if (null != ExceptionTools.GetFirtsCSharpException(pSpheresException)) // FI 20200623 [XXXXX] Utilisation de  GetFirtsCSharpException
            {
                codeReturn = Cst.ErrLevel.CS_OBJECTREFNOTSET;
                WriteEventLog_SystemError(Cst.MOM.MOMEnum.Unknown, codeReturn, null, pSpheresException.Method, message);
            }
            else
            {
                WriteEventLog(EventLog_EventId.SpheresServices_Error_System, pMessageId,
                    pSpheresException.ProcessState.Status, codeReturn, pSpheresException.Method, message);
            }
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMessageId"></param>
        /// <param name="pSpheresException"></param>
        /// FI 20200623 [XXXXX] Refactoring
        protected void WriteEventLog_SystemError(string pMessageId, SpheresException2 pSpheresException)
        {
            Cst.ErrLevel codeReturn = pSpheresException.ProcessState.CodeReturn;
            // FI 20200623 [XXXXX] Use property MessageExtended
            string message = pSpheresException.MessageExtended;
            if (codeReturn == Cst.ErrLevel.NOTCONNECTED)
            {
                WriteEventLog_SystemError(Cst.MOM.MOMEnum.Unknown, codeReturn, null, pSpheresException.Method, new Tuple<string, Nullable<SQLErrorEnum>>(message, null));
            }
            else if (null != ExceptionTools.GetFirstRDBMSException(pSpheresException))
            {
                Exception sqlException = ExceptionTools.GetFirstRDBMSException(pSpheresException);
                Nullable<SQLErrorEnum> sqlError = null;
                if (null != MQueue)
                    sqlError = DataHelper.AnalyseSQLException(MQueue.ConnectionString, sqlException);

                codeReturn = Cst.ErrLevel.SQL_ERROR;
                WriteEventLog_SystemError(Cst.MOM.MOMEnum.Unknown, codeReturn, null, pSpheresException.Method, new Tuple<string, Nullable<SQLErrorEnum>>(message, sqlError));
            }
            else if (null != ExceptionTools.GetFirtsCSharpException(pSpheresException))
            {
                codeReturn = Cst.ErrLevel.CS_OBJECTREFNOTSET;
                WriteEventLog_SystemError(Cst.MOM.MOMEnum.Unknown, codeReturn, null, pSpheresException.Method, new Tuple<string, Nullable<SQLErrorEnum>>(message, null));
            }
            else
            {
                WriteEventLog(EventLog_EventId.SpheresServices_Error_System, pMessageId,
                    pSpheresException.ProcessState.Status, codeReturn, pSpheresException.Method, message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMOMType"></param>
        /// <param name="pErrLevel"></param>
        /// <param name="pMessageId"></param>
        /// <param name="pMethod"></param>
        /// <param name="pAddionalInfo"></param>
        protected void WriteEventLog_SystemError(Cst.MOM.MOMEnum pMOMType, Cst.ErrLevel pErrLevel, string pMessageId, string pMethod, Tuple<string, Nullable<SQLErrorEnum>> pAddionalInfo)
        {
            EventLog_EventId eventId = EventLog_EventId.SpheresServices_Error_System;
            string message = string.Empty;

            switch (pErrLevel)
            {
                case Cst.ErrLevel.MOM_PATH_ERROR:
                    eventId = EventLog_EventId.SpheresServices_Error_MOMPath;
                    message = string.Format("{0} is unreachable", pMOMType.ToString()) + Cst.CrLf;
                    message += @"- Contact your system administrator to make sure there are no problem with the network or its configurations.";
                    break;
                case Cst.ErrLevel.INITIALIZE_ERROR:
                    eventId = EventLog_EventId.SpheresServices_Error_MOMInitialize;
                    if (pMOMType == Cst.MOM.MOMEnum.MSMQ)
                    {
                        message = "The queue either does not exist, it is unreachable or the message queuing service (MSMQ) is stopped." + Cst.CrLf;
                    }
                    else if (IsMsMQueue)
                    {
                        message = "The folder either does not exist, it is unavailable or the name resolution service (WINS/DNS) is stopped." + Cst.CrLf;
                    }
                    message += "- WARNING: Spheres® software will not run again until this Spheres® service is not successfully restarted." + Cst.CrLf;
                    message += "- Contact your system administrator to make sure there are no problem with the network or its configurations, and to restart this Spheres® service.";
                    break;
                case Cst.ErrLevel.MESSAGE_MOVE_ERROR:
                    eventId = EventLog_EventId.SpheresServices_Error_MOMUndefined;
                    break;
                case Cst.ErrLevel.CS_OBJECTREFNOTSET:
                    eventId = EventLog_EventId.SpheresServices_Error_ObjRefNotSet;
                    message = "A SYSTEM error occurred." + Cst.CrLf;
                    message += @"- Contact the software publisher for more information.";
                    break;
                case Cst.ErrLevel.NOTCONNECTED:
                    eventId = EventLog_EventId.SpheresServices_Error_SQLConnection;
                    message = "A SQL error occurred." + Cst.CrLf;
                    message += "- Contact your system administrator to make sure there are no problem with the network or data server.";
                    break;
                case Cst.ErrLevel.SQL_ERROR:
                    // FI 20201013 [XXXXX] Meilleure gestion des Erreurs SQL. Lecture de Item2 si nécessaire
                    bool isDefaultEntry = true;
                    if (pAddionalInfo.Item2 != null)
                    {
                        switch (pAddionalInfo.Item2)
                        {
                            case SQLErrorEnum.DeadLock:
                                eventId = EventLog_EventId.SpheresServices_Error_SQLDeadlock;
                                message = "A DEADLOCK error occurred." + Cst.CrLf;
                                isDefaultEntry = false;
                                break;
                            case SQLErrorEnum.Timeout:
                                eventId = EventLog_EventId.SpheresServices_Error_SQLTimeout;
                                message = "A TIMEOUT error occurred." + Cst.CrLf;
                                isDefaultEntry = false;
                                break;
                            default:
                                isDefaultEntry = true;
                                break;
                        }
                    }
                    if (isDefaultEntry)
                    {
                        eventId = EventLog_EventId.SpheresServices_Error_SQL;
                        message = "A SQL error occurred." + Cst.CrLf;
                    }

                    message += @"- Contact your system administrator to make sure that the database used is compatible with the version of the service above.";
                    break;
                // PM 20221020 [25617] Add CMECONNECTIONFAILED
                case Cst.ErrLevel.CMECONNECTIONFAILED:
                    eventId = EventLog_EventId.SpheresServices_Error_CMECore;
                    break;
            }
            if (StrFunc.IsFilled(pAddionalInfo.Item1))
            {
                if (StrFunc.IsFilled(message))
                {
                    message += Cst.CrLf;
                }
                // PM 20221020 [25617] Correction de l'absence de .Item1
                //message += pAddionalInfo;
                message += pAddionalInfo.Item1;
            }

            WriteEventLog(eventId, pMessageId,
                ProcessStateTools.StatusErrorEnum, pErrLevel, pMethod, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMOMType"></param>
        /// <param name="pErrLevel"></param>
        /// <param name="pAddInfo"></param>
        protected void WriteEventLog_SystemInformation(Cst.MOM.MOMEnum pMOMType, Cst.ErrLevel pErrLevel, string pAddInfo)
        {
            EventLog_EventId eventId = EventLog_EventId.SpheresServices_Info_System;
            string method = string.Empty;
            string message = string.Empty;

            switch (pErrLevel)
            {
                case Cst.ErrLevel.START:
                    method = "OnStart";
                    eventId = EventLog_EventId.SpheresServices_Info_Start;
                    message = @"StartService";
                    break;
                case Cst.ErrLevel.CONTINUE:
                    method = "OnContinue";
                    eventId = EventLog_EventId.SpheresServices_Info_Continue;
                    message = @"StartService";
                    break;
                case Cst.ErrLevel.STOP:
                    method = "OnStop";
                    eventId = EventLog_EventId.SpheresServices_Info_Stop;
                    message = @"StopService";
                    break;
                case Cst.ErrLevel.BREAK:
                    method = "OnPause";
                    eventId = EventLog_EventId.SpheresServices_Info_Pause;
                    message = @"StopService";
                    break;
                case Cst.ErrLevel.INITIALIZE:
                    method = "ActivateService";
                    eventId = EventLog_EventId.SpheresServices_Info_Initialize;
                    message = string.Format(@"Service initialize listening on {0}...", pMOMType.ToString());
                    break;
                case Cst.ErrLevel.CONNECTED:
                    method = "ActivateService";
                    eventId = EventLog_EventId.SpheresServices_Info_Connected;
                    message = string.Format(@"Service has successfully established listening on  {0}.", pMOMType.ToString());
                    break;
                case Cst.ErrLevel.EXECUTED:
                    method = "ActivateProcess";
                    eventId = EventLog_EventId.SpheresServices_Info_System;
                    message = Ressource.GetString(ServiceName) + Cst.CrLf + "Process information.";
                    break;
            }
            if (StrFunc.IsFilled(pAddInfo))
            {
                if (StrFunc.IsFilled(message))
                {
                    message += Cst.CrLf;
                }
                message += pAddInfo;
            }

            WriteEventLog(eventId,
                null,
                ProcessStateTools.StatusSuccessEnum, pErrLevel, method, message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatus"></param>
        /// <param name="pErrLevel"></param>
        /// <param name="pMessageId"></param>
        /// <param name="pMethod"></param>
        /// <param name="pAddInfo"></param>
        // EG 20190318 Test IRQ
        protected void WriteEventLog_BusinessError(ProcessStateTools.StatusEnum pStatus, Cst.ErrLevel pErrLevel, string pMessageId, string pMethod, string pAddInfo)
        {
            EventLog_EventId eventId = EventLog_EventId.SpheresServices_Error_Business;
            string message;
            switch (pErrLevel)
            {
                case Cst.ErrLevel.NOTHINGTODO:
                case Cst.ErrLevel.TUNING_IGNORE:
                    eventId = EventLog_EventId.SpheresServices_Warning_Business;
                    message = GetInfo() + " - " + "Warning, no process has occurred.";
                    break;
                case Cst.ErrLevel.IRQ_EXECUTED:
                    eventId = EventLog_EventId.SpheresServices_Warning_Business;
                    message = GetInfo() + " - " + "Warning, process has volontary interrupted.";
                    break;
                default:
                    message = GetInfo() + " - " + "Warning, process not completed!";
                    break;
            }
            if (StrFunc.IsFilled(pAddInfo))
            {
                if (StrFunc.IsFilled(message))
                {
                    message += Cst.CrLf;
                }
                message += pAddInfo;
            }

            WriteEventLog(eventId, pMessageId,
                pStatus, pErrLevel, pMethod, message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pErrLevel"></param>
        /// <param name="pMessageId"></param>
        /// <param name="pAddInfo"></param>
        protected void WriteEventLog_BusinessInformation(Cst.ErrLevel pErrLevel, string pMessageId, string pAddInfo)
        {
            EventLog_EventId eventId = EventLog_EventId.SpheresServices_Info_Business;
            string method;
            string message;
            switch (pErrLevel)
            {
                default:
                    method = "ActiveProcess";
                    message = GetInfo() + " - " + "Process completed successfully.";
                    break;
            }
            if (StrFunc.IsFilled(pAddInfo))
            {
                if (StrFunc.IsFilled(message))
                {
                    message += Cst.CrLf;
                }
                message += pAddInfo;
            }

            WriteEventLog(eventId, pMessageId,
                ProcessStateTools.StatusSuccessEnum, pErrLevel, method, message);
        }


        /// <summary>
        /// WriteEventLog
        /// </summary>
        /// <param name="pStatus"></param>
        /// <param name="pCodeReturn"></param>
        /// <param name="pMessage"></param>
        /// FI 20130802 [] pb de compilation dans gateway => La méthode est maintenant protected
        protected void WriteEventLog(ProcessStateTools.StatusEnum pStatus, Cst.ErrLevel pCodeReturn, string pMessage)
        {
            WriteEventLog(EventLog_EventId.SpheresServices_Error_Undefined,
                null,
                pStatus, pCodeReturn, string.Empty, pMessage);
        }
        /// <summary>
        /// WriteEventLog
        /// </summary>
        /// <param name="pEventId"></param>
        /// <param name="pMessageId"></param>
        /// <param name="pStatus"></param>
        /// <param name="pCodeReturn"></param>
        /// <param name="pMethod"></param>
        /// <param name="pMessage"></param>
        /// FI 20130802 [XXXXX] pb de compilation dans gateway => La méthode est maintenant protected
        /// FI 20160804 [Migration TFS] Modify
        /// FI 20161021 [XXXXX] Modify
        protected void WriteEventLog(EventLog_EventId pEventId, string pMessageId,
            ProcessStateTools.StatusEnum pStatus, Cst.ErrLevel pCodeReturn, string pMethod, string pMessage)
        {
            try
            {
                // source  => journal des évènements de windows® utilisé par le service 
                string eventLog = RegistryTools.GetEventLog(this.ServiceName);
                // source  => source de l'évènement qui sera inscrit dans le journal
                string source = this.ServiceName + Cst.EventLogSourceExtension;

                string CSWithoutPassword = "N/A";
                if (null != _mQueue)
                {
                    try { CSWithoutPassword = _mQueue.ConnectionStringWithoutPassword; }
                    catch { CSWithoutPassword = "N/A"; };
                }

                EventLogCharateristics elc = new EventLogCharateristics(HostName, eventLog, source, pStatus, pCodeReturn, pEventId);

                if (IsAddToEventLog(elc.Level))
                {
                    //Niveau (Level) de log nécessitant une écriture dans le Journal Windows.
                    string[] replacementStrings = new string[10];
                    replacementStrings[1 - 1] = ServiceLongName;
                    replacementStrings[2 - 1] = StrFunc.IsFilled(pMessage) ? pMessage = pMessage.TrimEnd(Cst.CrLf.ToCharArray()) : string.Empty; ;
                    replacementStrings[3 - 1] = pCodeReturn.ToString();
                    replacementStrings[4 - 1] = StrFunc.IsFilled(pMethod) ? pMethod : string.Empty;

                    replacementStrings[5 - 1] = MomTypeEnum.ToString();
                    replacementStrings[6 - 1] = StrFunc.IsFilled(_MOMpath) ? _MOMpath : "N/A";
                    replacementStrings[7 - 1] = CSWithoutPassword;

                    replacementStrings[8 - 1] = StrFunc.IsFilled(pMessageId) ? pMessageId : "N/A";

                    replacementStrings[9 - 1] = HostName;
                    replacementStrings[10 - 1] = UserName;

                    elc.Data = replacementStrings;
                    // FI 20210629 [XXXXX] Using syntaxe
                    using (EventLogEx eventLogEx = new EventLogEx(elc))
                    {
                        eventLogEx.ReportEvent();
                    }
                }
            }
            catch (Exception ex)
            {
                //Si Erreur Tentative d'ecriture dans un fichier SpheresService.log
                try
                {
                    /* Generation du repertoire s'il n'existe pas */
                    // FI 20160804 [Migration TFS] Utilisation de MapPath
                    //string directory = AppInstance.GetLogDirectory();
                    string directory = AppInstance.MapPath("Log");
                    SystemIOTools.CreateDirectory(directory);

                   

                    SpheresException2 oEx = new SpheresException2("WriteEventLog", ex);
                    ErrorBlock errBlock = new ErrorBlock(oEx, new AppSession(this.AppInstance), string.Empty);
                    ErrorFormatter ef = new ErrorFormatter(errBlock);

                    string fileName = ErrorManager.BuildLogFileName(StrFunc.AppendFormat(@"{0}\{1}", directory, "SpheresService.log"));
                    using (StreamWriter sw = File.AppendText(fileName))
                    {
                        sw.WriteLine(ef.LogFileData);
                        sw.Flush();
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected MQueueCount GetNumberMessageInQueue()
        {
            return GetNumberMessageInQueue(Cst.ProcessTypeEnum.NA);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessType"></param>
        protected MQueueCount GetNumberMessageInQueue(Cst.ProcessTypeEnum pProcessType)
        {

            MQueueCount ret = new MQueueCount();
            
            //20100517 PL Newness: Gestion des adresses de queue de type "FormatName"
            //if (IsMsMQueue && MessageQueue.Exists(_Path))
            if (IsMsMQueue)
            {
                //NB: Méthode Exists() indisponible avec une queue de type "FormatName"
                if (IsMsMQueue_FormatNameAddress || MessageQueue.Exists(_MOMpath))
                {
                    #region MSMQueue message's counter
                    Message[] messages = _messageQueue.GetAllMessages();
                    foreach (Message message in messages)
                    {
                        GetNumberMessageInQueue(pProcessType, message.Label, ret);
                    }
                    #endregion MSMQueue message's counter
                }
            }
            else if (IsFileWatcher)
            {
                #region FileWatcher message's counter
                DirectoryInfo directoryInfo = new DirectoryInfo(_MOMpath);
                if (directoryInfo.Exists)
                {
                    FileInfo[] fileInfos = directoryInfo.GetFiles(FilterFile);
                    if ((null != fileInfos) && (0 < fileInfos.Length))
                    {
                        foreach (FileInfo fileInfo in fileInfos)
                        {
                            string fileName = Path.ChangeExtension(fileInfo.Name, null);
                            GetNumberMessageInQueue(pProcessType, fileName, ret);
                        }
                    }

                }
                #endregion FileWatcher message's counter
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessType"></param>
        /// <param name="pMessage"></param>
        /// <param name="pCountQueue"></param>
        // PM 20210115 [XXXXX] Changement de paramètres
        //protected void GetNumberMessageInQueue(Cst.ProcessTypeEnum pProcessType, string pMessage, ref int pCountQueueHigh, ref int pCountQueueNormal, ref int pCountQueueLow)
        private void GetNumberMessageInQueue(Cst.ProcessTypeEnum pProcessType, string pMessage, MQueueCount pCountQueue)
        {
            Regex regex = new Regex(MQueueBase.GetRegularExpressionPattern());
            if (regex.IsMatch(pMessage))
            {
                string[] item = pMessage.Split('_');
                if (8 == item.Length)
                {
                    if (item[0].StartsWith(pProcessType.ToString()))
                    {
                        if (item[4].StartsWith(Cst.StatusPriority.HIGH.ToString()))
                        {
                            pCountQueue.CountQueueHigh += 1;
                        }
                        else if (item[4].StartsWith(Cst.StatusPriority.REGULAR.ToString()))
                        {
                            pCountQueue.CountQueueNormal += 1;
                        }
                        else if (item[4].StartsWith(Cst.StatusPriority.LOW.ToString()))
                        {
                            pCountQueue.CountQueueLow += 1;
                        }
                    }
                }
            }
        }

        #region Listener Start/Stop
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMOMObject"></param>
        private void StartListener(object pMOMObject)
        {
            ListenerStartStop(true, pMOMObject, null);
        }
        /// <summary>
        /// 
        /// </summary>
        private void StopListener()
        {
            if (IsFileWatcher)
                ListenerStartStop(false, _fileSystemWatcher, null);
            else if (IsMsMQueue)
                ListenerStartStop(false, _messageQueue, _messageQueue_AsyncResult);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMOMObject"></param>
        private void StopListener(object pMOMObject)
        {
            ListenerStartStop(false, pMOMObject, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMOMObject"></param>
        /// <param name="pAsyncResult"></param>
        private void StopListener(object pMOMObject, IAsyncResult pAsyncResult)
        {
            ListenerStartStop(false, pMOMObject, pAsyncResult);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsStart"></param>
        /// <param name="pMOMObject"></param>
        /// <param name="pAsyncResult"></param>
        private void ListenerStartStop(bool pIsStart, object pMOMObject, IAsyncResult pAsyncResult)
        {
            //20101026 PL Refactoring: Voir plus tard pour écrire une INFORMATION sur l'état Start/Stop dans EventViewer (si LogLevel=Full)
            #region FileWatcher
            if (IsFileWatcher)
            {
                //PL 20120319 Add test on null object
                if (pMOMObject != null)
                    ((FileSystemWatcher)pMOMObject).EnableRaisingEvents = pIsStart;
            }
            #endregion
            #region MsMQueue
            else if (IsMsMQueue)
            {
                //START ---------------------------------------------------------------
                if (pIsStart)
                {
                    Exception lastException = null;
                    if (_listenerStarted || (_messageQueue_BeginPeek_Count > 0))
                    {
                        try
                        {
                            StopListener();//NB: Lister already started --> stop
                        }
                        catch (Exception ex)
                        {
                            lastException = ex;
                        }
                    }
                    if (_listenerStarted || (_messageQueue_BeginPeek_Count > 0))
                    {
                        string errorMsg = @"Message queuing service (MSMQ) stop failure" + Cst.CrLf;
                        errorMsg += @"[Path: " + _MOMpath + "]" + Cst.CrLf;
                        errorMsg += @"[BeginPeek: " + _messageQueue_BeginPeek_Count.ToString() + "]" + Cst.CrLf;
                        ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.MOM_PATH_ERROR);
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, errorMsg, processState, lastException);
                    }
                    else
                    {
                        _messageQueue_AsyncResult = ((MessageQueue)pMOMObject).BeginPeek();
                        _messageQueue_BeginPeek_Count++;
                    }
                }
                //STOP ----------------------------------------------------------------
                else
                {
                    if (_listenerStarted)
                    {
                        if ((pAsyncResult != null) && (pMOMObject != null))
                        {
                            _ = ((MessageQueue)pMOMObject).EndPeek(pAsyncResult);
                            _messageQueue_BeginPeek_Count--;
                        }
                    }
                }
            }
            #endregion
            //
            _listenerStarted = pIsStart;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool ListenerFileWatcher()
        {
            bool isListenerCreated = false;

            //20101026 PL Refactoring: 
            Exception lastException = null;
            bool isMOMExist = false;

            //PL 20101125
            //            Il faut ici passer en 2ème paramètre:
            //            - Si GATEBCS: le MemberCode lu dans la registry ou le fichier de config, et ce pour l'instance concernée 
            //            - Sinon     : null 
            // PM 20101130
            // Utilisation d'un membre virtuel pour le suffixe spécifique alimenté par le MemberCode pour GATEBCS
            _MOMpath = _MOMPathRoot + @"\" + ServiceTools.GetQueueSuffix(ServiceEnum, SpecificSuffix);
            #region TEST
            if (StrFunc.IsFilled(SpecificSuffix) && !Directory.Exists(_MOMpath))
            {
                //Tentative d'utilisation d'un folder ignorant le specificSuffix 
                _MOMpath = _MOMPathRoot + @"\" + ServiceTools.GetQueueSuffix(ServiceEnum);
            }
            #endregion
            //
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(_MOMpath);
                if (directoryInfo.Exists)
                {
                    isMOMExist = true;

                    isListenerCreated = true;
                    _fileSystemWatcher = new FileSystemWatcher();
                    ((ISupportInitialize)(this._fileSystemWatcher)).BeginInit();
                    this._fileSystemWatcher.NotifyFilter = ((NotifyFilters)(((((NotifyFilters.FileName | NotifyFilters.DirectoryName)
                        | NotifyFilters.Size)
                        | NotifyFilters.LastWrite)
                        | NotifyFilters.LastAccess)));

                    WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.FileWatcher, Cst.ErrLevel.INITIALIZE, null);

                    this._fileSystemWatcher.Changed += new FileSystemEventHandler(this.OnFileSystemWatcher_Changed);
                    //this._fileSystemWatcher.EnableRaisingEvents = true;  //20101026 PL Refactoring: Mis en commentaire
                    ((ISupportInitialize)(this._fileSystemWatcher)).EndInit();

                    _fileSystemWatcher.Filter = FilterFile;
                    _fileSystemWatcher.Path = _MOMpath;
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
            }

            if ((!isMOMExist) || (lastException != null))
            {
                if ((!isMOMExist) && (lastException == null))
                {
                    //NB: Get filesystem error 
                    string currentDir = Directory.GetCurrentDirectory();
                    try
                    {
                        Directory.SetCurrentDirectory(_MOMpath);
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                    }
                    finally
                    {
                        Directory.SetCurrentDirectory(currentDir);
                    }
                }

                string errorMsg = @"FileWatcher path does not exist or is unavailable!";
                errorMsg += @" [Path: " + _MOMpath + "]";
                ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.MOM_PATH_ERROR);
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, errorMsg, processState, lastException);
            }
            else
            {
                //Tentative fructeuse, écriture d'un message Information dans le journal Windows
                WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.FileWatcher, Cst.ErrLevel.CONNECTED, null);
            }

            return isListenerCreated;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool ListenerMsMQueue()
        {
            bool isListenerCreated = false;

            Exception lastException = null;
            bool isMOMExist = false;

            //WARNING: Sur une queue "Remote" le type d'adresse "FormatName" ne fonctionne pas en mode DEBUG, le compte windows ne dispose pas des droits nécessaires sur le "Remote serveur".
            SetMSMQEnableConnectionCache();

            //PL 20101125
            //            Il faut ici passer en 2ème paramètre:
            //            - Si GATEBCS: le MemberCode lu dans la registry ou le fichier de config, et ce pour l'instance concernée 
            //            - Sinon     : null 
            // PM 20101130
            // Utilisation d'un membre virtuel pour le suffixe spécifique alimenté par le MemberCode pour GATEBCS
            _MOMpath = _MOMPathRoot + ServiceTools.GetQueueSuffix(ServiceEnum, SpecificSuffix);
            
            if (StrFunc.IsFilled(SpecificSuffix))
            {
                MessageQueue mq = MQueueTools.GetMsMQueue_NoError(_MOMpath, _unreachableTimeout);
                if (mq == null)
                {
                    //Tentative d'utilisation d'un folder ignorant le specificSuffix 
                    _MOMpath = _MOMPathRoot + ServiceTools.GetQueueSuffix(ServiceEnum);
                }
            }
            

            double timeOut = _unreachableTimeout;
            //#if DEBUG
            //                    timeOut = 10;
            //                    System.Diagnostics.Debugger.Break();
            //#endif
            bool isCreate = true;
            bool isWarning = false;
            bool isWarningRecorded = false;
            int count = 0;
            DatetimeProfiler dtProfiler = new DatetimeProfiler(DateTime.Now);
            #region while isMOMExist / timeout
            while (((timeOut <= 0) || (dtProfiler.GetTimeSpan().TotalSeconds.CompareTo(timeOut) == -1))
                && (!isMOMExist)
                && (count < Int32.MaxValue))
            {
                try
                {
                    //NB: Avec une adresse de type "FormatName", le seul moyen de renseigner le path, 
                    //    c'est au moment de l'instanciation de l'objet (la propriété "path" ne le permet pas).
                    count++;

                    //Lors du tout 1er appel depuis ActivateService(), l'objet _messageQueue n'existe pas encore, on le crée donc immédiatement.
                    if (count == 1)
                    {
                        if (_messageQueue == null)
                        {
                            //----------------------------------------------------------------------------------------------
                            //1ère tentative de création 
                            //----------------------------------------------------------------------------------------------
                            isCreate = true;
                        }
                        else
                        {
                            //----------------------------------------------------------------------------------------------
                            //1ère tentative de réaccession, on espère que l'objet _messageQueue est encore valide... 
                            //On supprime son délégué qui sera recréé plus bas, et on patiente 0.5sec au cas où l'objet deviendrait réaccessible
                            //----------------------------------------------------------------------------------------------
                            isCreate = false;
                            isWarning = true;
                            _messageQueue.PeekCompleted -= new PeekCompletedEventHandler(this.OnPeekCompleted);
                            Thread.Sleep(500); //Pause de 0.5 sec.
                        }
                    }
                    else
                    {
                        //----------------------------------------------------------------------------------------------
                        //Nème tentatives, l'objet initial _messageQueue n'est a priori plus valide... 
                        //----------------------------------------------------------------------------------------------
                        isCreate = true;
                        Thread.Sleep(500); //Pause de 0.5 sec.
                    }
                    if (isCreate)
                    {
                        //Reinitialisation complète (Objet et variables)
                        _listenerStarted = false;
                        _messageQueue_BeginPeek_Count = 0;
                        _messageQueue_AsyncResult = null;
                        //
                        if (IsMsMQueue_FormatNameAddress || MessageQueue.Exists(_MOMpath))
                        {
                            //NB: Avec une adresse de type "FormatName", la méthode MessageQueue.Exists() est indisponible
                            isListenerCreated = true;
                            _messageQueue = new MessageQueue(_MOMpath);
                        }
                    }
                    if (isListenerCreated || !isCreate)
                    {
                        //Vérification du bon fonctionnement de la queue, par généreration d'une erreur lors du MoveNext() 
                        MessageEnumerator messageEnum = _messageQueue.GetMessageEnumerator2();
                        messageEnum.MoveNext();
                        messageEnum = null;
                        //
                        isMOMExist = true;
                        //
                        if (isWarningRecorded)
                            isWarning = false;
                    }
                    else
                    {
                        isWarning = (count == 1) || ((LogLevel >= LogLevelDetail.LEVEL4) && ((count % 10) == 0));
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    isWarning = (count == 1) || ((LogLevel >= LogLevelDetail.LEVEL4) && ((count % 10) == 0));
                }
                finally
                {
                    if (isWarning)
                    {
                        //Tentative infructeuse, écriture d'un message Warning dans le journal Windows
                        isWarningRecorded = true;
                        isWarning = false;

                        WriteEventLog_SystemError(Cst.MOM.MOMEnum.MSMQ, Cst.ErrLevel.MOM_PATH_ERROR, null, "ListenerMsMQueue",
                            new Tuple<string, SQLErrorEnum?>(@"[Path: " + _MOMpath + "] [Failed attempts to access: " + count.ToString() + "]", null));
                    }
                }
            }
            #endregion
            //
            if (isMOMExist)
            {
                #region isMOMExist --> WriteEventLog()
                //Tentative fructeuse, écriture d'un message Information dans le journal Windows
                WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.MSMQ, Cst.ErrLevel.INITIALIZE, null);
                //
                _messageQueue.PeekCompleted += new PeekCompletedEventHandler(this.OnPeekCompleted);
                //
                if (isWarningRecorded)
                {
                    //N tentative(s) infructeuse(s), écriture d'un message Information dans le journal Windows
                    WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.MSMQ, Cst.ErrLevel.CONNECTED, @" [Attempts to access: " + count.ToString() + "]");
                }
                else
                {
                    //1ère tentative fructeuse, écriture d'un message Information dans le journal Windows
                    WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.MSMQ, Cst.ErrLevel.CONNECTED, null);
                }
                #endregion
            }
            else
            {
                #region !isMOMExist --> throw new SpheresException()
                string errorMsg = @"Message queuing service (MSMQ) does not exist or is unreachable!";
                errorMsg += @" [Path: " + _MOMpath + "]";
                errorMsg += @" [Failed attempts to access: " + count.ToString() + "]";
                ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.MOM_PATH_ERROR);
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, errorMsg, processState, lastException);
                #endregion
            }
            return isListenerCreated;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileSystemWatcher"></param>
        /// FI 20240318 [WI873] Refactoring (suppression de isRefreshFile)
        private void RecoveryFileWatcher(object pFileSystemWatcher)
        {
            try
            {
                StopListener(pFileSystemWatcher);
#if DEBUG
                #region PL for debug on SVR-DB01 
                //PL for debug on SVR-DB01 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                if (System.Environment.MachineName == "DWS-136")
                {
                    string overridePath = @"C:\SpheresServices\Queue";
                    _MOMpath = _MOMpath.Replace(_MOMPathRoot, overridePath);
                    _MOMPathRoot = overridePath;
                }
                #endregion
#endif

                IEnumerable<FileInfo> fileInfos = GetFileWatcherFiles();

                while (fileInfos.Count() > 0)
                {
                    foreach (FileInfo item in fileInfos)
                    {
                        _currentMessageId = item.Name;
                        try
                        {
                            if (false == _isProcessing)
                            {
                                ActiveProcess(_currentMessageId);
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteEventLog_SystemError(_currentMessageId, new SpheresException2("SpheresService.RecoveryFileWatcher", ex));
                        }
                        finally
                        {
                            _currentMessageId = null;
                        }
                    }
                }
            }
            catch (Exception) { throw; }
            finally
            {
                StartListener(_fileSystemWatcher);
            }
        }

        /// <summary>
        /// Retourne une collection énumérable de fichiers xml dont le nom respecte <see cref="MQueueBase.GetRegularExpressionPattern()"/>
        /// </summary>
        /// <returns></returns>
        /// FI 20240318 [WI873] add Method
        private IEnumerable<FileInfo> GetFileWatcherFiles()
        {

            DirectoryInfo directoryInfo = new DirectoryInfo(_MOMpath);

            Regex regex = new Regex(MQueueBase.GetRegularExpressionPattern());

            IEnumerable<FileInfo> ret = Directory.EnumerateFiles(_MOMpath, FilterFile)
                    .Select(f => new FileInfo(f)).Where(x => regex.IsMatch(Path.ChangeExtension(x.Name, null)))
                    .OrderBy(f => f.Name.Substring(16, 19));

            return ret;

        }


        /// <summary>
        /// Traite les messages présents dans la queue, se remet ensuite à l'écoute de la queue afin de déclencher l'évènement OnPeekCompleted avec l'arrivée d'un nouveau message (BeginPeek)
        /// </summary>
        /// <exception cref="SpheresException2 [MOM_PATH_ERROR] si la queue n'est pas accessible (Cela se produit lorsque Spheres® tente une nouvelle connexion à la queue et qu'elle n'aboutie pas)"/>
        private void RecoveryMsMQueue(object pMessageQueue, IAsyncResult pAsyncResult)
        {
            RecoveryMsMQueue(pMessageQueue, pAsyncResult, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMessageQueue"></param>
        /// <param name="pAsyncResult"></param>
        /// <param name="pCountRecursiveCall"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        private void RecoveryMsMQueue(object pMessageQueue, IAsyncResult pAsyncResult, int pCountRecursiveCall)
        {
            if (pCountRecursiveCall < 2)
            {
                pCountRecursiveCall++;
                //
                SpheresException2 spheresException_Main;
                bool isQueueUnReachable = false;
                Regex regex = new Regex(MQueueBase.GetRegularExpressionPattern());
                MessageEnumerator messageEnumerator = null;
                string step = null;
                //
                try
                {
                    #region StopListener / GetMessage
                    spheresException_Main = null;
                    try
                    {
                        #region StopListener
                        step = "StopListener";
                        StopListener(pMessageQueue, pAsyncResult);
                        #endregion

                        #region GetMessage
                        // Get a cursor into the messages in the queue.
                        step = "GetMessageEnumerator2";
                        messageEnumerator = ((MessageQueue)pMessageQueue).GetMessageEnumerator2();
                        //#if DEBUG
                        //                        System.Diagnostics.Debugger.Break();
                        //#endif
                        step = "MoveNext";
                        int guardMessageAlreadyReceived = 0;
                        while (messageEnumerator.MoveNext())
                        {
                            #region ActiveProcess
                            step = "Current";

                            //PL 20131219 Add Try/Catch(MessageAlreadyReceived)
                            try
                            {
                                Message msgCurrent = messageEnumerator.Current;

                                if (regex.IsMatch(msgCurrent.Label))
                                {
                                    _currentMessageId = msgCurrent.Id;

                                    SpheresException2 spheresException = null;
                                    try
                                    {
                                        if (false == _isProcessing)
                                        {
                                            ActiveProcess(_currentMessageId);
                                        }
                                    }
                                    catch (SpheresException2 ex) { spheresException = ex; }
                                    catch (Exception ex) { spheresException = new SpheresException2("SpheresService.RecoveryMsMQueue", ex); }
                                    finally
                                    {
                                        if (null != spheresException)
                                        {
                                            WriteEventLog_SystemError(_currentMessageId, spheresException);
                                        }
                                    }
                                }
                            }
                            catch (MessageQueueException ex)
                            {
                                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.MessageAlreadyReceived)
                                {
                                    guardMessageAlreadyReceived++;
                                    if (guardMessageAlreadyReceived >= 100)
                                    {
                                        throw;
                                    }
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                    catch (MessageQueueException ex)
                    {
                        spheresException_Main = new SpheresException2("SpheresServiceBase.RecoveryMsMQueue",
                            StrFunc.AppendFormat("Error on {0}: [ErrMsg: {1}][ErrCode: {2}]",
                            step,
                            ex.MessageQueueErrorCode.ToString(),
                            ex.ErrorCode.ToString()),
                            ex);
                    }
                    catch (SpheresException2 ex) { spheresException_Main = ex; }
                    catch (Exception ex) { spheresException_Main = new SpheresException2("SpheresServiceBase.RecoveryMsMQueue", StrFunc.AppendFormat("Error on {0}", step), ex); }
                    finally
                    {
                        if (null != messageEnumerator)
                        {
                            messageEnumerator.Close();
                        }
                        if (null != spheresException_Main)
                        {
                            //NB: Une exception s'est produite lors de l'arrêt du listener ou dans la boucle de traitement des messages présents dans la queue.
                            //    L'erreur rencontrée s'est a priori produite lors de la lecture de la queue...
                            //    On tente une reconnection à la queue et effectue un nouveau RecoveryMsMQueue() pour tenter de traiter les messages existants
                            WriteEventLog_SystemError(null, spheresException_Main);//20101026 PL Refactoring: Voir pour ajouter ici les valeurs de : _messageQueue_BeginPeek_Count et _listenerStarted

                            if (_countMOMUnreachable < Int32.MaxValue)
                            {
                                _countMOMUnreachable++;
                            }
                            if (ListenerMsMQueue())//Cette méthode génère une exception si l'initialisation échoue
                            {
                                //Ici l'objet MessageQueue a été recréé, l'ancien étant a priori HS
                                RecoveryMsMQueue(_messageQueue, _messageQueue_AsyncResult, pCountRecursiveCall);
                            }
                            else
                            {
                                //Ici l'objet MessageQueue n'a pas été recréé, l'ancien ayant été a priori récupéré
                                RecoveryMsMQueue(pMessageQueue, pAsyncResult, pCountRecursiveCall);
                            }
                        }
                    }
                    #endregion
                }
                catch (SpheresException2 ex)
                {
                    isQueueUnReachable = (ex.ProcessState.CodeReturn == Cst.ErrLevel.MOM_PATH_ERROR);
                    throw;
                }
                catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
                finally
                {
                    if ((!isQueueUnReachable) && (pCountRecursiveCall == 1))
                    {
                        //NB: On se remet à l'écoute de la queue, si toutes les conditions suivantes sont vérifiées
                        //    - Exception autre que Cst.ErrLevel.MOM_PATH_ERROR 
                        //    - 1er appel à cette méthode RecoveryMsMqueue (pour ne pas démarrer plusieurs fois !!) 
                        #region StartListener
                        StartListener(_messageQueue);
                        #endregion
                    }
                }
            }
        }

        private void OnFileSystemWatcher_Changed(object source, FileSystemEventArgs e)
        {
            try
            {
                RecoveryFileWatcher(source);
            }
            catch (Exception ex)
            {
                WriteEventLog_SystemError(null, new SpheresException2("SpheresServiceBase.OnFileSystemWatcher_Changed", ex));
            }
        }

        private void OnPeekCompleted(object source, PeekCompletedEventArgs asyncResult)
        {
            try
            {
                RecoveryMsMQueue(source, asyncResult.AsyncResult);
            }
            catch (Exception ex)
            {
                SpheresException2 spheresException = new SpheresException2("SpheresServiceBase.OnPeekCompleted", ex);
                WriteEventLog_SystemError(null, spheresException);
                if (spheresException.ProcessState.CodeReturn == Cst.ErrLevel.MOM_PATH_ERROR)
                    WriteEventLog_ListenerBreak("OnPeekCompleted");
            }
        }

        private string GetInfo()
        {
            //PL 20130624 Le nom du service est déjà présent en début du log généré
            //StrBuilder message = new StrBuilder(Ressource.GetString(ServiceName, true));
            StrBuilder message = new StrBuilder();

            if ((null != _mQueue) && (_mQueue.idSpecified || _mQueue.identifierSpecified))
            {
                //message += @": " + _mQueue.LibProcessType + @" - ";
                message += @"Service: " + _mQueue.LibProcessType + @" - ";

                string label = "Identifier";
                if (_mQueue.IsIdT)
                    label = "Trade";
                else if (_mQueue.LibProcessType == Cst.ProcessTypeEnum.IO.ToString())
                    label = "Task";

                if (_mQueue.idSpecified && _mQueue.identifierSpecified)
                    message += StrFunc.AppendFormat("{0}: {1} [id:{2}]", label, _mQueue.identifier, _mQueue.id.ToString());
                else if (_mQueue.identifierSpecified)
                    message += StrFunc.AppendFormat("{0}: {1}", label, _mQueue.identifier);
                else
                    message += StrFunc.AppendFormat("{0} [id:{1}]", (label != "Identifier" ? label : string.Empty), _mQueue.id.ToString());
            }

            return message.ToString();
        }


        /// <summary>
        /// Traitement d'un message
        /// </summary>
        /// <param name="pMessageId"></param>
        /// ThreadTools.SetCurrentCulture
        /// FI 20170215 [XXXXX] Modify
        // EG 20180423 Analyse du code Correction [CA2200]
        private void ActiveProcess(string pMessageId)
        {
            // **********************************************************************
            // WARNING: 
            // Lorsqu'une anamolie se produit avec l'exécutable, mais pas en mode DEBUG, mettre en commentaire 
            // le changement de culture ci-dessous, afin d'essayer de reproduire l'anomalie en mode DEBUG.
            // **********************************************************************

            //English is default culture
            ThreadTools.SetCurrentCulture(Cst.EnglishCulture);

            // When  File pMessageId = "xxxxx.xml" Where "xxxxx"=FileName
            // When  MSMQ pMessageId = "xxxxx" Where "xxxxx"=Message.Id
            try
            {
                // Timer Stop
                if (null != m_ServiceTrace)
                    m_ServiceTrace.StopTimer();


                _isProcessing = true;

                // Initialisation des statuts et code retour
                ProcessState processState = new ProcessState(ProcessStateTools.StatusUnknownEnum, ProcessStateTools.CodeReturnSuccessEnum)
                {
                    // STEP 1: Lecture du message -------------------------------------------------------
                    CodeReturn = PrepareProcess(ref pMessageId)
                };

                if (ProcessStateTools.IsCodeReturnSuccess(processState.CodeReturn))
                {
                    int idLog = 0;
                    try
                    {
                        if (_countMessage < Int32.MaxValue)
                            _countMessage++;


                        // STEP 2: Exécution du message ---------------------------------------------------
                        processState = ExecuteProcess(out idLog);

                        // Alimentation du journal des événements de Windows® lors d'une erreur "Système"
                        if (ArrFunc.Count(processState.SpheresExceptions) > 0)
                        {
                            foreach (SpheresException2 item in processState.SpheresExceptions)
                                WriteEventLog_SystemError(null, item);
                        }

                        if (idLog > 0)
                        {
                            // PM 20221020 [25617] Replace WriteEventLog_MOMorFOLDERError by WriteEventLog_CheckFromProcessDetL
                            //WriteEventLog_MOMorFOLDERError(pMessageId, idLog);
                            WriteEventLog_CheckFromProcessDetL(pMessageId, idLog);
                        }

                        WriteEventLog_ProcessState(processState, pMessageId, idLog);

                    }
                    catch (Exception ex)
                    {
                        #region Alimentation, en cas d'anomalie, du journal des événements de Windows®
                        //FI 20091209 [16790] Si le traitement génère une exception Spheres® écrit dans le journal des évènements de windows
                        //En écrivant ici dans le log la ConnectionString est présente dans le message généré puisque Mqueue est != null 
                        //Voir Méthode WriteEventLog

                        // FI 20180527 Alimentation du CurrentStatus comme partout ailleurs
                        //processState.Status = ProcessStateTools.StatusErrorEnum;
                        processState.CurrentStatus = ProcessStateTools.StatusErrorEnum;
                        WriteEventLog_SystemError(pMessageId, new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex));

                        Exception sqlException = ExceptionTools.GetFirstRDBMSException(ex);
                        if (null != sqlException)
                        {
                            //FI 20110411 [17391] Si timeout lors de l'insert de la 1ere ligne dans le log (PROCESS_L)
                            //Spheres® recommmence le traitement si un timeout s'est produit lors de l'initialisation du log
                            //PL 20130708 Si DeadLock on recommmence le traitement (Test in progress...)
                            SQLErrorEnum error = DataHelper.AnalyseSQLException(MQueue.ConnectionString, sqlException);
                            if (
                                (idLog == 0) && (error == SQLErrorEnum.Timeout)
                                ||
                                (error == SQLErrorEnum.DeadLock)
                                )
                            {
                                //PL 20130708 Use CurrentStatus instead of Status (see also EndProcess below)
                                //processState.Status = ProcessStateTools.StatusPendingEnum;
                                processState.CurrentStatus = ProcessStateTools.StatusPendingEnum;
                            }
                        }
                        #endregion
                    }
                }


                // STEP 3: Archivage ou purge du message --------------------------------------------
                if ((processState.CodeReturn != Cst.ErrLevel.MESSAGE_NOTFOUND) && (processState.CodeReturn != Cst.ErrLevel.MESSAGE_MOVE_ERROR))
                    EndProcess(pMessageId, processState.CurrentStatus);
                // ----------------------------------------------------------------------------------

                #region Trace (Alimentation de SERVICE_L)
                // FI 20230309 [XXXXX] déplacé après le STEP 3 (Avant était effectué avant STEP 3)
                // Aini si exception durant la trace, le message queue ne reste pas dans PROGRESS
                if (ActivateObserver)
                {
                    if ((null != MQueue) && (processState.CodeReturn != Cst.ErrLevel.NOTCONNECTED))
                    {
                        TraceProcess();
                        if (MQueue.IsMessageObserver)
                            TraceService();
                    }
                }
                #endregion
            }
            catch (Exception) { throw; }
            finally
            {

                TerminateProcess();
                // FI 20230309 [XXXXX] TIMER START déplacé ici
                if (null != m_ServiceTrace)
                    m_ServiceTrace.StartTimer();

            }
        }


        /// <summary>
        /// Méthode en charge des étapes préalables avant l'rexécution procesuss.
        /// <para>Lecture du message, interprétation (cast), contrôle (CS)...</para>
        /// <para>Retourne les valeurs suivantes : SUCCESS, MESSAGE_NOTFOUND, MESSAGE_MOVE_ERROR, MESSAGE_CAST_ERROR, NOTCONNECTED</para>
        /// </summary>
        /// <param name="pMessageId"></param>
        /// <returns></returns>
        //PL 20131219 Param by Ref
        // EG 20180423 Analyse du code Correction [CA2200]
        private Cst.ErrLevel PrepareProcess(ref string opMessageId)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            switch (MomTypeEnum)
            {
                case Cst.MOM.MOMEnum.FileWatcher:
                    #region Déplacement du fichier dans le répertoire de travail (Progress)
                    // 20081024 RD 7551 
                    string fileFullName = string.Empty;
                    SpheresException2 spheresException = null;
                    try
                    {
                        //20071031 FI 15898 add Try Catch
                        FileTools.ErrLevel err = FileTools.ErrLevel.SUCCESS;

                        #region Progress folder
                        string progressFullName = _MOMPathRoot + @"\" + PathProgress;
                        if (!Directory.Exists(progressFullName))
                            Directory.CreateDirectory(progressFullName);
                        #endregion

                        fileFullName = _MOMpath + @"\" + opMessageId.ToUpper();
                        _progressFileFullName = progressFullName + @"\" + opMessageId.ToUpper();
                        err = FileTools.FileMove3(fileFullName, ref _progressFileFullName); //PL 20121226 Use FileMove3() instead of FileMove2()
                        switch (err)
                        {
                            case FileTools.ErrLevel.SUCCESS:
                                errLevel = Cst.ErrLevel.SUCCESS;
                                break;
                            case FileTools.ErrLevel.FILENOTFOUND:
                            case FileTools.ErrLevel.IOEXCEPTION:
                                // File used by another process => No Problem (Cas où il existe plusieurs process qui scannent le même repertoire)
                                errLevel = Cst.ErrLevel.MESSAGE_NOTFOUND;
                                //PL 20121224 Mise en commentaire de l'appel à WriteEventLog(), afin d'éviter la saturation du Log.
                                //string nfMsg = StrFunc.AppendFormat("File[name:{0}, Folder:{1}] is not found, this is not critical on multiple instances mode", pMessageId, fileFullName);
                                //WriteEventLog(pMessageId, ProcessStateTools.StatusWarningEnum, errLevel, "PrepareProcess", nfMsg);
                                break;
                            default:
                                // Move error (Cas qui ne doit jamais se produire)
                                errLevel = Cst.ErrLevel.MESSAGE_MOVE_ERROR;
                                WriteEventLog_SystemError(Cst.MOM.MOMEnum.FileWatcher, errLevel, opMessageId, "PrepareProcess", new Tuple<string, SQLErrorEnum?>(fileFullName + " --> " + _progressFileFullName,null));
                                break;
                        }

                        // 20081024 RD 7551 
                        if (_lastFileReadTime.Contains(fileFullName))
                            _lastFileReadTime.Remove(fileFullName);
                    }
                    catch (SpheresException2 ex) { spheresException = ex; }
                    catch (Exception ex) { spheresException = new SpheresException2("SpheresServiceBase.PrepareProcess", ex); }
                    finally
                    {
                        bool isLogError = (null != spheresException);
                        if (isLogError)
                        {
                            errLevel = Cst.ErrLevel.MESSAGE_MOVE_ERROR;
                            // 20081024 RD 7551                             
                            if (spheresException.Message.Contains("used by another process") ||
                                spheresException.Message.Contains("utilisé par un autre process"))
                            {
                                DateTime lastFileReadTime = DateTime.MinValue;
                                if (_lastFileReadTime.Contains(fileFullName))
                                {
                                    lastFileReadTime = (DateTime)_lastFileReadTime[fileFullName];
                                }

                                if (DtFunc.IsDateTimeFilled(lastFileReadTime))
                                {
                                    if (0 <= DateTime.Compare(lastFileReadTime, DateTime.Now.AddMinutes(-5)))
                                        isLogError = false;
                                    else
                                        _lastFileReadTime[fileFullName] = DateTime.Now;
                                }
                                else
                                {
                                    _lastFileReadTime.Add(fileFullName, DateTime.Now);
                                    isLogError = false;
                                }
                            }
                            else if (_lastFileReadTime.Contains(fileFullName))
                            {
                                _lastFileReadTime.Remove(fileFullName);
                            }

                            if (isLogError)
                            {
                                WriteEventLog_SystemError(opMessageId, spheresException);
                            }
                        }
                    }
                    #endregion Déplacement du fichier dans le répertoire de travail (Progress)

                    #region Deserialisation\\Cast
                    if (Cst.ErrLevel.SUCCESS == errLevel)
                    {
                        try
                        {
                            _mQueue = MQueueTools.ReadFromFile(_progressFileFullName);
                        }
                        catch (Exception ex)
                        {
                            SpheresException2 msgCastEx = new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
                            errLevel = Cst.ErrLevel.MESSAGE_CAST_ERROR;
                            WriteEventLog_SystemError(opMessageId, msgCastEx);
                        }
                    }
                    #endregion Deserialisation
                    break;
                case Cst.MOM.MOMEnum.MSMQ:
                    //PL 20131219 
                    int vMajor, vMinor, vRevision;
                    vMajor = vMinor = vRevision = 0;
                    string m_BizCmptLevel = (string)SystemSettings.GetAppSettings("BizCmptLevel", typeof(string), string.Empty);
                    if (!string.IsNullOrEmpty(m_BizCmptLevel))
                    {
                        string[] version = m_BizCmptLevel.Split('.');
                        vMajor = Convert.ToInt32(version[0]);
                        vMinor = Convert.ToInt32(version[1]);
                        if (version.Length > 2)
                            vRevision = Convert.ToInt32(version[2]);
                    }

                    #region Receive du message
                    Message msg = null;
                    SpheresException2 msgEx = null;
                    try
                    {
                        //PL 20131219 
                        int timeOutValueInSecond = (int)SystemSettings.GetAppSettings("MSMQReceiveTimeout", typeof(int), 0);
                        if ((vMajor == 3) && (vMinor == 7) && (vRevision < 5101))
                        {
                            //Fonctionnement tel qu'il était avant la v3.7.5101
                            timeOutValueInSecond = 10;
                        }
                        if (timeOutValueInSecond > 0)
                        {
                            msg = _messageQueue.ReceiveById(opMessageId, (new TimeSpan(0, 0, timeOutValueInSecond)));
                        }
                        else
                        {
                            msg = _messageQueue.ReceiveById(opMessageId);
                        }
                    }
                    catch (Exception ex)
                    {
                        bool isTryReceive = false;
                        if (ex.GetType() == typeof(MessageQueueException))
                        {
                            MessageQueueException mqex = (MessageQueueException)ex;
                            if (mqex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout
                                || mqex.MessageQueueErrorCode == MessageQueueErrorCode.MessageAlreadyReceived)
                            {
                                isTryReceive = true;
                            }
                        }
                        else if (ex.GetType() == typeof(InvalidOperationException))
                        {
                            isTryReceive = true;
                        }
                        else
                        {
                            msgEx = new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
                        }

                        if (isTryReceive)
                        {
                            //PL 20131219 
                            try
                            {
                                if ((vMajor == 3) && (vMinor == 7) && (vRevision < 5101))
                                {
                                    //Fonctionnement tel qu'il était avant la v3.7.5101
                                    throw;
                                }
                                else
                                {
                                    //Le message concerné n'est plus disponible, on tente alors de lire le 1er message disponible pendant 3 sec.
                                    msg = _messageQueue.Receive((new TimeSpan(0, 0, 3)));
                                    opMessageId = msg.Id;
                                }
                            }
                            catch (MessageQueueException ex2)
                            {
                                if (ex2.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout
                                    || ex2.MessageQueueErrorCode == MessageQueueErrorCode.MessageAlreadyReceived)
                                {
                                    // NB: Cas où il existe plusieurs instances qui scannent la même queue
                                    string nfMsg = StrFunc.AppendFormat("Message[id:{0}] is not found, this is not critical on multiple instances mode", opMessageId);
                                    //PL 20121224 Mise en commentaire de l'appel à WriteEventLog(), afin d'éviter la saturation du Log.
                                    //WriteEventLog(opMessageId, ProcessStateTools.StatusWarningEnum, errLevel, "PrepareProcess", nfMsg);
                                    //msgEx = new SpheresException("SpheresServiceBase.PrepareProcess", nfMsg,
                                    //    new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.MESSAGE_NOTFOUND));
                                    errLevel = Cst.ErrLevel.MESSAGE_NOTFOUND;
                                }
                            }
                            catch (Exception ex2) { msgEx = new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex2); }
                        }
                    }
                    finally
                    {
                        if (null != msgEx)
                        {
                            errLevel = Cst.ErrLevel.MESSAGE_NOTFOUND;
                            WriteEventLog_SystemError(opMessageId, msgEx);
                        }
                    }
                    #endregion Receive du message

                    #region Deserialisation\\Cast
                    if (Cst.ErrLevel.SUCCESS == errLevel)
                    {
                        try
                        {
                            _mQueue = MQueueTools.ReadFromMessage(msg);
                        }
                        catch (Exception ex)
                        {
                            SpheresException2 msgCastEx = new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
                            errLevel = Cst.ErrLevel.MESSAGE_CAST_ERROR;
                            WriteEventLog_SystemError(opMessageId, msgCastEx);
                        }
                    }
                    #endregion Deserialisation\\Cast
                    break;
                default:
                    throw new NotImplementedException(MomTypeEnum.ToString() + " is not implemented");
            }

            //Check Connection
            if (Cst.ErrLevel.SUCCESS == errLevel)
            {
                try
                {
                    _mQueue.serviceName = AppInstance.AppNameInstance;
                    //PL 20190107 Use CSManager 
                    //DataHelper.CheckConnection(_mQueue.ConnectionString);
                    CSManager csManager = new CSManager(_mQueue.ConnectionString);
                    DataHelper.CheckConnection(csManager.Cs);
                }
                catch (DataHelperException ex)
                {
                    errLevel = Cst.ErrLevel.NOTCONNECTED;
                    SpheresException2 SpheresEx = new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex.Message,
                        new ProcessState(ProcessStateTools.StatusErrorEnum, errLevel), ex);
                    WriteEventLog_SystemError(opMessageId, SpheresEx);
                }
            }
            // FI 20200929 [XXXXX] Test sur Cst.ErrLevel.SUCCESS == errLevel 
            // si impossibilité de connexion, on constate errLevel = Cst.ErrLevel.NOTCONNECTED
            if (Cst.ErrLevel.SUCCESS == errLevel)
            {
                // EG 20200327 [XXXXX] Test _mQueue (peut être non valorisé car Message non trouvé car pris par autre service)
                if (null != _mQueue)
                {
                    // FI 20191211 [XXXXX] Appel à ResetDatesysCol afin de réinitialisée la date système courante
                    /* FI 20200811 [XXXXX] Mise en commentaire
                    OTCmlHelper.ResetDatesysCol(_mQueue.ConnectionString);
                    // FI 20200810 [XXXXX] Alimentation de la date système de suite post connexion réussie
                    // (pour éviter que l'appel à cette méthode soit effectuée alors qu'il existe une transaction => provoquant l'ouverture de connexions supplémentaires)
                    OTCmlHelper.GetDateSys(_mQueue.ConnectionString);
                    */
                    // FI 20200811 [XXXXX]
                    // => Appel à SynchroDatesysCol pour que le 1er appel à OTCmlHelper.GetDateSys(cs) ne provoque pas l'ouverture d'une nouvelle connexion s'il existe une transaction courante 
                    OTCmlHelper.SynchroDatesysCol(_mQueue.ConnectionString);
                }
            }

            return errLevel;

        }

        /// <summary>
        /// Entry method to execute a specific service process
        /// <para>
        /// </summary>
        /// <remarks>
        /// Any Spheres service must override this method (<seealso cref="ActiveProcess(string pMessageId)"/>)
        /// </remarks>
        /// <param name="pProcessLog"></param>
        /// <returns></returns>
        protected abstract ProcessState ExecuteProcess(out int pProcessLog);

        /// <summary>
        /// Arrêt du service alors qu'un traitement est en cours
        /// <para>
        /// Cette méthode doit être overrider  pour implémenter un traitement particulier lorsque le service est arrêté et qu'un traitement est en cours
        /// </para>
        /// </summary>
        protected virtual void StopProcess()
        {
        }

        #region Methods de gestion de ServiceTrace
        /// <summary>
        /// Methode appelée par le Timer de ServiceTrace
        /// </summary>
        /// <param name="state"></param>
        private void OnTraceServiceTimer(object state)
        {
            try
            {
                TraceService();
            }
            catch (Exception ex)
            {
                WriteEventLog_SystemError(null, new SpheresException2(nameof(OnTraceServiceTimer), ex));
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected void TraceService()
        {
            if (null != m_ServiceTrace)
            {
                List<Cst.ProcessTypeEnum> lstProcess = ReflectionTools.GetEnumValues<Cst.ProcessTypeEnum, Cst.ProcessRequestedAttribute>()
                    .Where(x => ReflectionTools.GetAttribute<Cst.ProcessRequestedAttribute>(x).ServiceName == this.ServiceEnum).ToList();


                List<Task> tsk = new List<Task>();
                lstProcess.ForEach(item =>
                {
                    tsk.Add(m_ServiceTrace.TraceServiceAsync(item, MQueue, GetNumberMessageInQueue(item)));
                });
                
                Task.WaitAll(tsk.ToArray());
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        protected virtual void TraceProcess()
        {
        }

        /// <summary>
        /// Création de ServiceTrace
        /// </summary>
        protected void OpenServiceTrace()
        {
            // PM 20210115 [XXXXX] Refactoring ServiceTrace
            if (default(ServiceTrace) == m_ServiceTrace)
            {
                m_ServiceTrace = new ServiceTrace(AppInstance, MomTypeEnum.ToString(), _MOMpath, new TimerCallback(OnTraceServiceTimer));
            }
        }

        /// <summary>
        /// Arrêt de ServiceTrace
        /// </summary>
        private void CloseServiceTrace()
        {
            try
            {
                if (null != m_ServiceTrace)
                {
                    // PM 20210115 [XXXXX] Refactoring ServiceTrace
                    m_ServiceTrace.Close(GetNumberMessageInQueue());
                    m_ServiceTrace = default;
                }
            }
            catch (Exception ex)
            {
                WriteEventLog_SystemError(null, new SpheresException2("SpheresService.CloseServiceTrace", ex));
            }
        }
        #endregion Methods de gestion de ServiceTrace

        /// <summary>
        /// Gestion du message (issu du MOM, FileWatcher ou MSMQ) en fin de process en fonction du status retour du traitement
        /// </summary>
        /// <param name="pMessageId"></param>
        /// <param name="pStatus"></param>
        private void EndProcess(string pMessageId, ProcessStateTools.StatusEnum pStatus)
        {
            if (IsFileWatcher)
                EndFileWatcherProcess(pMessageId, pStatus);
            else if (IsMsMQueue)
                EndMSMQProcess(pStatus);
        }
        /// <summary>
        /// Déplace le fichier message:
        /// <para>vers le répertoire SUCCESS si succès</para>
        /// <para>vers le répertoire ERROR si erreur</para>
        /// <vers>vers le répertoire d'origine si "PENDING"(pour une tentative de traitement ultérieure)</vers>
        /// </summary>
        /// <param name="pFileName"></param>
        /// <param name="pStatus"></param>
        //PL 20180306 Nouveau principe: l'horodatage est tjs positionné en 16, et l'horodatage initial est sauvegardé à la fin, et le cas échéant le dernier horodatage est sauvegardé tout à la fin 
        //ex. 1er  postage: POSKEEPREQUESTX_2018030604005909111_XXXXXXXX_XXXXXXXX_XXXXXXXX_0_0_0.xml
        //    2nd  postage: POSKEEPREQUESTX_2018030604055837222_XXXXXXXX_XXXXXXXX_XXXXXXXX_0_0_0_2018030604005909111.xml
        //    3eme postage: POSKEEPREQUESTX_2018030604066986333_XXXXXXXX_XXXXXXXX_XXXXXXXX_0_0_0_2018030604005909111_2018030604055837222.xml
        //    4eme postage: POSKEEPREQUESTX_2018030604077569444_XXXXXXXX_XXXXXXXX_XXXXXXXX_0_0_0_2018030604005909111_2018030604066986333.xml
        private void EndFileWatcherProcess(string pFileName, ProcessStateTools.StatusEnum pStatus)
        {
            string resultFileName = Path.ChangeExtension(pFileName, null);
            //Identification des 19 caractères finaux.
            //Ces derniers sont présents lorsque le message est issu d'un précédent traitement qui a terminé en pending. 
            //Ils représentent alors l'horodage de la demande initiale. 
            Regex regex = new Regex(@"_\d{19}$");
            bool isExistONEEndedTimestamp = regex.IsMatch(resultFileName);
            regex = new Regex(@"_\d{19}_\d{19}$");
            bool isExistTWOEndedTimestamp = regex.IsMatch(resultFileName);

            #region Déplacement des fichiers similaires dans le répertoire REPLICA
            if (IsReplicaMsgWithFileWatcherAvailable)
            {
                try
                {
                    bool isExistPathReplica = Directory.Exists(_MOMPathRoot + @"\" + PathReplica);

                    //Replace timestamp by joker characters (?)
                    string similarFileName = resultFileName.Replace(resultFileName.Substring(16, 19), "???????????????????");
                    //Remove end timestamp
                    if (isExistONEEndedTimestamp)
                        similarFileName = similarFileName.Substring(0, similarFileName.Length - (1 + 19));
                    if (isExistTWOEndedTimestamp)
                        similarFileName = similarFileName.Substring(0, similarFileName.Length - (1 + 19));

                    DirectoryInfo directoryInfo = new DirectoryInfo(_MOMpath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles(similarFileName.ToUpper() + "*.xml");
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        if (isExistPathReplica)
                        {
                            string resultFullPathReplica = _MOMPathRoot + "/" + PathReplica + "/" + fileInfo.Name;
                            FileTools.FileMove2(fileInfo.FullName, ref resultFullPathReplica);
                        }
                        else
                        {
                            FileTools.FileDelete2(fileInfo.FullName);
                        }
                    }
                }
                catch { }
            }

            #endregion
            #region Déplacement du fichier dans le répertoire final (Success/Error ou Queue d'origine)

            string timestamp = resultFileName.Substring(16, 19);
            //Reset timestamp 
            resultFileName = resultFileName.Replace(timestamp, DateTime.Now.ToString("yyyyMMddHHmmssfffff"));
            if ((!isExistONEEndedTimestamp) || (!isExistTWOEndedTimestamp))
            {
                //Add initial or last timestamp at the end
                resultFileName += "_" + timestamp;
            }
            else
            {
                //Overrride last timestamp at the end
                resultFileName = resultFileName.Substring(0, resultFileName.Length - (1 + 19)) + "_" + timestamp;
            }
            resultFileName += ".xml";

            if (ProcessStateTools.IsStatusPending(pStatus))
            {
                // Repostage du message courant dans la queue d'origine pour retraitement 
                FileTools.FileDelete2(_progressFileFullName);
                MQueueTools.Send(_mQueue, ServiceTools.GetMqueueSendInfo(_mQueue.ProcessType, AppInstance));

            }
            else
            {
                // Déplacement du fichier dans le répertoire final (Success/Error)
                string resultFolder = _MOMPathRoot + @"\" + PathSuccess;
                if (pStatus != ProcessStateTools.StatusEnum.SUCCESS && pStatus != ProcessStateTools.StatusEnum.NONE)
                {
                    resultFolder = _MOMPathRoot + @"\" + PathError;
                }

                if (Directory.Exists(resultFolder))
                {
                    string resultFileFullPath = resultFolder + @"\" + resultFileName.ToUpper();
                    FileTools.FileMove2(_progressFileFullName, ref resultFileFullPath);
                }
                else
                {
                    FileTools.FileDelete2(_progressFileFullName);
                }
            }
            #endregion
        }

        /// <summary>
        /// Envoi un nouveau message:
        /// <para>vers la queue succes si succes</para>
        /// <para>vers la queue error si error</para>
        /// <vers>vers la queue de travail si pending (Pour une tentative de traitement ultérieure)</vers>
        /// </summary>
        /// <param name="pStatus"></param>
        private void EndMSMQProcess(ProcessStateTools.StatusEnum pStatus)
        {
            //try
            //{
            // RD 20130402 [18549] Gestion des queues ERROR et SUCCESS
            //string suffix = string.Empty;
            //bool isToSend = true;

            //switch (pStatus)
            //{
            //    case ProcessStateTools.StatusEnum.SUCCESS:
            //    case ProcessStateTools.StatusEnum.NONE:
            //        suffix = PathSuccess;
            //        break;
            //    case ProcessStateTools.StatusEnum.PENDING:
            //        suffix = string.Empty;
            //        break;
            //    default:
            //        suffix = PathError;
            //        break;
            //}

            //if (StrFunc.IsFilled(suffix))
            //{
            //    // Vérification de l'existence de la queue
            //    MessageQueue mq = MQueueTools.GetMsMQueue_NoError(_pathRoot + suffix, _unreachableTimeout);
            //    if (mq == null)
            //        isToSend = false;
            //}

            //if (isToSend)
            //{
            //    MQueueTools.SendQueue(_mQueue, _pathRoot, suffix, _isRecoverable, _isEncrypt,
            //        _mQueue.header.messageQueueName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfffff"));
            //}

            // RD 20130410 [18549] Mise en commentaire du code ci-dessus, pour les raisons suivantes:
            // A la recherche de l'existence de la queue, on est obligé de faire plusieurs tentatives, pendant un laps de temps (timeout), 
            // car la queue pourrait être inaccessible pendant un moment, et devenir accessible quelques temps après.
            // Ce timeout cause des problèmes de performance.                
            if (ProcessStateTools.IsStatusPending(pStatus))
            {
                MQueueTools.SendQueue(_mQueue, _MOMPathRoot, string.Empty, _isRecoverable, _isEncrypt,
                    _mQueue.header.messageQueueName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfffff"));
            }
            //}
            //catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        /// <summary>
        /// 
        /// </summary>
        private void TerminateProcess()
        {
            _mQueue = null;
            _isProcessing = false;
        }

        /// <summary>
        /// Retourne true si le type d'entrée possède le niveau suffisant pour être injecté dans le journal des évèments de Windows.
        /// <para>Ceci est fonction de niveau de log du service</para>
        /// </summary>
        /// <param name="pEventLogEntry"></param>
        /// <returns></returns>
        private bool IsAddToEventLog(EventLogEntryType pEventLogEntry)
        {
            // Les Erreurs sont toujours ajoutées
            bool ret = (pEventLogEntry == EventLogEntryType.Error);
            if (false == ret)
            {
                // Les autres type d'infos sont ajoutées uniquement si LogLevel est supérieur à LEVEL1 (ie Aucun LOG)
                //if (LogLevel != LogLevelDetail.NONE)
                if (LogLevel != LogLevelDetail.LEVEL1)
                    ret = true;
            }
            return ret;
        }
        
        /// <summary>
        ///  Obtient true lorsque le service fait appel à des processus qui utilisent le service de Log (PROCESS_L, PROCESSDET_L) 
        /// </summary>
        /// 
        private Boolean IsLoggerAvailable { get => (ServiceEnum != Cst.ServiceEnum.SpheresLogger) && (false == AppInstance.IsGateway); }


        /// <summary>
        /// Activation du logger en fonction de la clé de configuration "RemoteLogEnabled"
        /// </summary>
        protected void InitLogger()
        {
            if (IsLoggerAvailable && BoolFunc.IsTrue(SystemSettings.GetAppSettings("RemoteLogEnabled", "true")))
            {
                LoggerManager.Enable();
            }
            else
            {
                LoggerManager.Disable();
            }
        }



        /// <summary>
        /// Charge le niveau de détail du log associé au journal des évènements de Windows®
        /// </summary>
        protected void InitLogLevel()
        {
            
            LogLevel = LogLevelDetail.LEVEL3;

            string keyLogLevel = _serviceInfo.GetInformationsKey(ServiceKeyEnum.LogLevel);
            if (StrFunc.IsFilled(keyLogLevel))
            {
                try
                {
                    if (Enum.IsDefined(typeof(LogLevelDetail), keyLogLevel))
                    {
                        LogLevel = (LogLevelDetail)Enum.Parse(typeof(LogLevelDetail), keyLogLevel, true);
                    }
                    else
                    {
                        //Pour compatibilité ascendante, si besoin... 
                        switch (keyLogLevel)
                        {
                            case "FULL":
                                LogLevel = LogLevelDetail.LEVEL4;
                                break;
                            case "NONE":
                                LogLevel = LogLevelDetail.LEVEL2;
                                break;
                            default:
                                LogLevel = LogLevelDetail.LEVEL3;
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteEventLog_SystemError(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.INCORRECTPARAMETER, null, "InitLogLevel",
                                               new Tuple<string, SQLErrorEnum?>(StrFunc.AppendFormat("Invalid LogLevel parameter, LogLevel is set to DEFAULT. Error: {0}", ex.Message), null));
                }
            }
        }

        /// <summary>
        /// Définit le mode cache sur la gestion des queues MSMQ
        /// <para>Si rien n'est précisé dans la base de registre EnableConnectionCache=false</para>
        /// </summary>
        private void SetMSMQEnableConnectionCache()
        {
            MessageQueue.EnableConnectionCache = false;

            string infoMSMQEnableConnectionCache = _serviceInfo.GetInformationsKey(ServiceKeyEnum.MSMQEnableConnectionCache);
            if (StrFunc.IsFilled(infoMSMQEnableConnectionCache))
            {
                try
                {
                    MessageQueue.EnableConnectionCache = BoolFunc.IsTrue(infoMSMQEnableConnectionCache);
                }
                catch (Exception ex)
                {
                    //Si l'affectation de EnableConnectionCache est en exception
                    //On génère un message de warning dans le log, le traitement continue avec la valeur par défaut
                    WriteEventLog_SystemError(Cst.MOM.MOMEnum.MSMQ, Cst.ErrLevel.DATAREJECTED, null, "SetMSMQEnableConnectionCache",
                                               new Tuple<string, SQLErrorEnum?>(StrFunc.AppendFormat("MSMQ EnableConnectionCache is set to FALSE. Error: {0}", ex.Message), null));
                }
            }
        }

        /// <summary>
        /// Ajoute dans le journal des évènement un message indiquant que le service ne traitera plus de message, car la queue est détectée comme inaccessible.
        /// </summary>
        /// <returns></returns>
        private void WriteEventLog_ListenerBreak(string pMethod)
        {
            try
            {
                WriteEventLog_SystemError(IsMsMQueue ? Cst.MOM.MOMEnum.MSMQ : Cst.MOM.MOMEnum.FileWatcher, Cst.ErrLevel.INITIALIZE_ERROR, null, pMethod, new Tuple<string, SQLErrorEnum?>(StrFunc.AppendFormat("[Path: {0}]", _MOMpath), null));
            }
            catch (Exception ex)
            {
                throw SpheresExceptionParser.GetSpheresException(string.Empty, ex);
            }
        }

        /// <summary>
        /// Recherche d'éventuels messages d'erreur dans PROCESSDET_L afin d'alimentation du journal des évènements
        /// </summary>
        /// <param name="pMessageId">Nom du message de demande d'exécution du process</param>
        /// <param name="idLog">Id du process</param>
        // PM 20221020 [25617] New
        private void WriteEventLog_CheckFromProcessDetL(string pMessageId, int idLog)
        {
            WriteEventLog_MOMorFOLDERError(pMessageId, idLog);
            WriteEventLog_CMEErrorWarning(pMessageId, idLog);
        }

        /// <summary>
        /// Recherche d'éventuels messages d'erreur provenant de la conexion au CME
        /// </summary>
        /// <param name="pMessageId"></param>
        /// <param name="idLog"></param>
        // PM 20221020 [25617] New
        private void WriteEventLog_CMEErrorWarning(string pMessageId, int idLog)
        {
            string cs = MQueue.ConnectionString;
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.ID), idLog);

            string sql = @"select distinct IDSTPROCESS, DATA1, SYSNUMBER
            from dbo.PROCESSDET_L
            where (IDPROCESS_L = @ID) and (SYSCODE = 'CME')";

            QueryParameters qry = new QueryParameters(cs, sql, dp);

            using (IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    // PL 20221024 [XXXXX] Passage de CME-02570 à CME-00001 
                    if (Convert.ToInt32(dr["SYSNUMBER"]) == 1)
                    {
                        Cst.ErrLevel errLevel = Cst.ErrLevel.CMECONNECTIONFAILED;
                        string msg = "Unable to connect to the CME Core margin service";
                        WriteEventLog_SystemError(MomTypeEnum, errLevel, pMessageId, "ActiveProcess", new Tuple<string, SQLErrorEnum?>(msg, null));
                    }

                }
            }
        }

        /// <summary>
        /// Recherche d'éventuels messages d'erreur de type FOLDERNOTFOUND ou MOM_PATH_ERROR pour alimentation du journal des évènements
        /// </summary>
        /// <param name="pMessageId"></param>
        /// <param name="idLog"></param>
        private void WriteEventLog_MOMorFOLDERError(string pMessageId, int idLog)
        {
            //Recherche d'éventuels messages du LOG Spheres®, à dupliquer dans le journal des événements de Windows®
            //Note: Que le traitement final soit en succès ou en erreur, s'il existe certaines erreurs, Spheres les injecte 
            //      dans le journal des évènements du serveur aplicatif sous forme de warning. Cela afin d'informer l'administrateur 
            //      réseau qu'il existe éventuellement des pbs sur le réseau (FOLDERNOTFOUND, ...)

            string cs = MQueue.ConnectionString;
            Cst.ErrLevel errLevel = Cst.ErrLevel.UNDEFINED;
            string msg = string.Empty;
            bool isExistStatusError = false;

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.ID), idLog);

            string[] idStprocess = new string[] { ProcessStateTools.StatusError, ProcessStateTools.StatusUnknown };
            StrBuilder sql = new StrBuilder(SQLCst.SELECT_DISTINCT);
            sql += "IDSTPROCESS,DATA2,DATA3,";
            sql += "case when MESSAGE like '%FOLDERNOTFOUND%' then 'Folder' else 'MOM' end as ERRORTYPE" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.PROCESSDET_L.ToString() + Cst.CrLf;
            sql += SQLCst.WHERE + "(IDPROCESS_L=@ID)" + Cst.CrLf;
            sql += SQLCst.AND + "(" + DataHelper.SQLColumnIn(cs, "IDSTPROCESS", idStprocess, TypeData.TypeDataEnum.@string) + ")" + Cst.CrLf;
            sql += SQLCst.AND + "(" + Cst.CrLf;
            sql += "(MESSAGE like '%" + Cst.ErrLevel.FOLDERNOTFOUND.ToString() + @"%')" + Cst.CrLf;
            sql += SQLCst.OR + "(MESSAGE like '%" + Cst.ErrLevel.MOM_PATH_ERROR.ToString() + @"%')" + Cst.CrLf;
            sql += ")";

            QueryParameters qry = new QueryParameters(cs, sql.ToString(), dp);

            using (IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    if (ProcessStateTools.IsStatusError(dr["IDSTPROCESS"].ToString()))
                    {
                        isExistStatusError = true;
                    }

                    if (dr["ERRORTYPE"].ToString() == "MOM")
                    {
                        msg += "MOM is unreachable" + Cst.CrLf;
                        errLevel = Cst.ErrLevel.MOM_PATH_ERROR;
                    }
                    else if (dr["ERRORTYPE"].ToString() == "Folder")
                    {
                        msg += "Folder does not exist or is not accessible" + Cst.CrLf;
                        errLevel = Cst.ErrLevel.FOLDERNOTFOUND;
                    }

                    msg += dr["DATA2"].ToString() + Cst.CrLf; //DATA2 contient le path
                    msg += dr["DATA3"].ToString() + Cst.CrLf; //DATA3 contient le nbre de tentative d'accès 
                }
            }

            if (errLevel != Cst.ErrLevel.UNDEFINED)
            {
                if (errLevel == Cst.ErrLevel.MOM_PATH_ERROR)
                {
                    if (_countMOMUnreachable < Int32.MaxValue)
                        _countMOMUnreachable++;
                }
                else if (errLevel == Cst.ErrLevel.FOLDERNOTFOUND)
                {
                    if (_countFolderNotFound < Int32.MaxValue)
                        _countFolderNotFound++;
                }

                if (isExistStatusError)
                {
                    if (errLevel == Cst.ErrLevel.MOM_PATH_ERROR)
                        msg += "- If the queue exist, ";
                    else
                        msg += "- If the folder exist, ";
                    msg += "contact your system administrator to verify that the acccount has rights and to make sure there are no problems with the network or its configurations.";
                }
                WriteEventLog_SystemError(Cst.MOM.MOMEnum.Unknown, errLevel, pMessageId, "ActiveProcess", new Tuple<string, SQLErrorEnum?>(msg, null));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processState"></param>
        /// <param name="pMessageId"></param>
        /// <param name="idLog"></param>
        private void WriteEventLog_ProcessState(ProcessState processState, string pMessageId, int idLog)
        {
            //PL 20121227 Use processState.CurrentStatus instead of processState.Status
            if (ProcessStateTools.IsNOTStatusPending(processState.CurrentStatus))
            {
                //Ecriture dans le journal des événements de Windows®: 
                //- si ERROR   (et quelque soit LogLevel)
                //- si SUCCESS et LogLevel FULL 

                if (ProcessStateTools.IsStatusSuccess(processState.CurrentStatus))
                {

                    if (LogLevel >= LogLevelDetail.LEVEL4)
                    {
                        string msg = string.Empty;
                        if (idLog > 0)
                        {
                            msg += Cst.CrLf + StrFunc.AppendFormat("For more information, see the Spheres® process log [id:{0}]", idLog);
                        }
                        WriteEventLog_BusinessInformation(Cst.ErrLevel.EXECUTED, pMessageId, msg);
                    }

                    #region _countSuccessfully
                    bool isWriteLog = false;
                    if (_countSuccessfully < Int32.MaxValue)
                    {
                        _countSuccessfully++;

                        if (LogLevel == LogLevelDetail.LEVEL5)
                            isWriteLog = ((_countSuccessfully % 10) == 0);
                        else if (LogLevel == LogLevelDetail.LEVEL4)
                            isWriteLog = ((_countSuccessfully % 100) == 0);
                        else if (LogLevel == LogLevelDetail.LEVEL3)
                            isWriteLog = ((_countSuccessfully % 1000) == 0);
                    }

                    if (isWriteLog)
                    {
                        string msg = StrFunc.AppendFormat("Result [Message: {0}][Success: {1}]", _countMessage, _countSuccessfully);
                        WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.EXECUTED, msg);
                    }

                    if (_countSuccessfully == Int32.MaxValue)
                        _countSuccessfully = 0;
                    #endregion

                }
                else //UNSUCCESS
                {
                    string msg = string.Empty;
                    //PL 20130624 Test when processState.CurrentStatus equal ZERO (not initialize)
                    if (processState.CurrentStatus != 0)
                    {
                        msg += "- Current status: " + processState.CurrentStatus.ToString() + Cst.CrLf;
                    }
                    msg += "- Process status: " + processState.Status.ToString();
                    if (idLog > 0)
                    {
                        msg += Cst.CrLf + StrFunc.AppendFormat("For more information, see the Spheres® process log [id:{0}]", idLog);
                    }
                    //PL 20121227 Use processState.CurrentStatus instead of processState.Status
                    //WriteEventLog(pMessageId, processState.CurrentStatus, processState.CodeReturn, "ExecuteProcess", msg);
                    //PL 20130624 Use processState.Status when processState.CurrentStatus is NA (not initialize) ex. Cas d'une IO Task inexistante
                    if (processState.CurrentStatus != ProcessStateTools.StatusEnum.NA)
                    {
                        WriteEventLog_BusinessError(processState.CurrentStatus, processState.CodeReturn, pMessageId, "ActiveProcess", msg);
                    }
                    else
                    {
                        WriteEventLog_BusinessError(processState.Status, processState.CodeReturn, pMessageId, "ActiveProcess", msg);
                    }
                }
            }
        }

        #endregion

        #region ISpheresServiceParameters Membres

        /// <summary>
        /// 
        /// </summary>
        public virtual Dictionary<string, object> ServiceProperties
        {
            get
            {
                Dictionary<string, object> serviceProperties = new Dictionary<string, object>(1)
                {
                    { "SERVICEENUM", this.ServiceEnum }
                };

                return serviceProperties;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual List<BaseFormParameters> InitModalConfiguration()
        {
            List<BaseFormParameters> formParameters = new List<BaseFormParameters>();


            MOMFormParameters mOMForm = new MOMFormParameters
            {
                Text = ConstructServiceName(ServiceEnum, null)
            };
            formParameters.Add(mOMForm);

            MOMInfoFormParameters mOMInfoForm = new MOMInfoFormParameters
            {
                Text = ConstructServiceName(ServiceEnum, null)
            };
            formParameters.Add(mOMInfoForm);

            ObserverFormParameters obsInfoForm = new ObserverFormParameters
            {
                Text = ConstructServiceName(ServiceEnum, null)
            };
            formParameters.Add(obsInfoForm);

            return formParameters;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="formParameters"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object> StartModalConfiguration(List<BaseFormParameters> formParameters)
        {
            return SpheresServiceParametersHelper.StartModalConfiguration(formParameters);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceParameters"></param>
        public virtual void WriteInstallInformation(string serviceName, Dictionary<string, object> serviceParameters)
        {
            SpheresServiceParametersHelper.WriteInstallInformation(serviceName, serviceParameters, this.ServiceProperties);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strServiceName"></param>
        public virtual void DeleteInstallInformation(string strServiceName)
        {
            SpheresServiceParametersHelper.DeleteInstallInformation(strServiceName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceFullPath"></param>
        /// <param name="parameters"></param>
        public virtual void UpdateSectionConfig(string pServiceFullPath, StringDictionary parameters)
        {
            // not needed...
        }

        

        #endregion
    }
    #endregion SpheresService
}

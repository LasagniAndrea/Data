using System;
using System.Linq;
using System.ServiceModel;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.SpheresServiceParameters;
//
namespace EFS.SpheresService
{
    /// <summary>
    /// Service d'hébergement du service WCF LoggerService
    /// </summary>
    public partial class LoggerService : SpheresServiceBase, ISpheresServiceParameters
    //public partial class LoggerService : ServiceBase //, ISpheresServiceParameters
    {
        #region Members
        /// <summary>
        /// Hôte du service
        /// </summary>
        private ServiceHost m_serviceHost = null;
        #endregion Members

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        public LoggerService()
            : base()
        {
                InitializeComponent();
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pServiceName">Nom du service</param>
        public LoggerService(string pServiceName)
            : base(pServiceName)
        {
            if (StrFunc.IsFilled(pServiceName))
            {
                base.ServiceName = pServiceName;
            }
            base.CanPauseAndContinue = true;
        }
        #endregion Constructors

        #region Override Accessors
        /// <summary>
        /// 
        /// </summary>
        protected override Cst.ServiceEnum ServiceEnum
        {
            get
            {
                return Cst.ServiceEnum.SpheresLogger;
            }
        }
        #endregion Override Accessors

        #region Override Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            ActivateServiceLogger();
            //
            base.OnStart(args);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnContinue()
        {
            ActivateServiceLogger();
            //
            base.OnContinue();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            //
            BreakServiceLogger();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPause()
        {
            base.OnPause();
            //
            BreakServiceLogger();
        }

        /// <summary>
        /// Arret du process
        /// </summary>
        protected override void StopProcess()
        {
            // Aucun process
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            // Aucun process
            ProcessState processState = new ProcessState(ProcessStateTools.StatusEnum.NA, Cst.ErrLevel.NOTHINGTODO);
            pIdLog = 0;
            return processState;
        }
        #endregion Override Methods

        #region Static Methods
        /// <summary>
        /// Point d'entrée principal du service.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
#if DEBUG
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresLogger, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                LoggerService debugService = new LoggerService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.OnStart(args);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
#endif
#if !DEBUG
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new LoggerService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new LoggerService(SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresRiskPerformance,null))
                };
            }
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
#endif
        }
        #endregion Static Methods

        #region Methods
        /// <summary>
        /// Activation du service de log
        /// </summary>
        protected void ActivateServiceLogger()
        {
            try
            {
                if (m_serviceHost != null)
                {
                    m_serviceHost.Close();
                }

                // Création de l'hôte du service
                m_serviceHost = new ServiceHost(typeof(EFS.LoggerService.SpheresLoggerService));
                
                // Ouverture de ServiceHostBase pour créer les listeners et démarrer l'écoute des messages.
                m_serviceHost.Open();

                // PM 20240612 [WI542] Ajout log de l'adresse de connexion
                Uri baseAddress = m_serviceHost.BaseAddresses.FirstOrDefault();
                if (baseAddress != default(Uri))
                {
                    EFS.Common.AppInstance.TraceManager.TraceInformation(ErrorLogTools.GetMethodName(), $"SpheresLoggerService base address: {baseAddress.OriginalString}");
                }
            }
            catch (Exception ex)
            {
                SpheresException2 extEx = SpheresExceptionParser.GetSpheresException("Start ServiceLogger Error", ex);
                WriteEventLog_SystemError(null, extEx);
            }
        }

        /// <summary>
        /// Arrêt du service de log
        /// </summary>
        protected void BreakServiceLogger()
        {
            try
            {
                if (m_serviceHost != null)
                {
                    m_serviceHost.Close();
                    m_serviceHost = null;
                }
            }
            catch (Exception ex)
            {
                SpheresException2 extEx = SpheresExceptionParser.GetSpheresException("Stop ServiceLogger Error", ex);
                WriteEventLog_SystemError(null, extEx);
            }
        }
        #endregion Methods
    }
}

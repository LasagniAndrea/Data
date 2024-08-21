using System;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;


using EFS.ACommon;
using EFS.Process;
using EFS.Process.EarGen;
using EFS.SpheresServiceParameters;

namespace EFS.SpheresService
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresEarGenService : SpheresServiceBase, ISpheresServiceParameters
    {
        #region Members
        private ProcessTradeBase earGenProcess;
        #endregion Members
        #region Accessors
        #region protected serviceEnum
        protected override Cst.ServiceEnum ServiceEnum
        {
            get { return Cst.ServiceEnum.SpheresEarGen; }
        }
        #endregion
        #endregion Accessors
        #region Constructor
        public SpheresEarGenService(string pServiceName) : base(pServiceName) { }
        public SpheresEarGenService() : base() { }
        #endregion Constructor
        #region Methods
        #region Main [principal process entry]
        static void Main(string[] args)
        {
#if DEBUG
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresEarGen, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresEarGenService debugService = new SpheresEarGenService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] 
                { 
                     new SpheresEarGenService(args[0].Substring(2))
                };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                   new SpheresEarGenService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresEarGen,null))
                };
            }
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
#endif
        }
        #endregion Main [principal process entry]
        #region CleanProcess
        protected override void StopProcess()
        {
            if (null != earGenProcess)
                earGenProcess.StopProcess();
        }
        #endregion CleanProcess
        #region ExecuteProcess
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            earGenProcess = new EarGenProcess(MQueue, AppInstance);
            earGenProcess.ProcessStart();
            pIdLog = earGenProcess.IdProcess;
            return earGenProcess.ProcessState;
        }
        #endregion ExecuteProcess

        /// <summary>
        /// 
        /// </summary>
        protected override void TraceProcess()
        {

            if (null == earGenProcess)
                throw new InvalidOperationException($"({nameof(earGenProcess)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");

            m_ServiceTrace.TraceProcessAsync(earGenProcess, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                earGenProcess.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);

        }
        
        
        /// <summary>
        /// Retourne la Version du service.
        /// <para>NB: Cette méthode masque la méthode de même nom de la classe de base.</para>
        /// </summary>
        /// <returns></returns>
        public static new Version GetServiceVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
        #endregion Methods

        
    }
}
using System;
using System.Threading;
using System.Collections.Specialized;
using System.Collections.Generic;


using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Process;
using EFS.Process.SettlementInstrGen; 


// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;
using System.Reflection;
using System.Runtime.InteropServices;


namespace EFS.SpheresService
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresSettlementInstrGenService : SpheresServiceBase, ISpheresServiceParameters
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private ProcessTradeBase settlementInstrGenProcess;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected override Cst.ServiceEnum ServiceEnum
        {
            get { return Cst.ServiceEnum.SpheresSettlementInstrGen; }
        }

        #endregion Accessors

        #region Constructor
        public SpheresSettlementInstrGenService(string pServiceName) : base(pServiceName) { }
        public SpheresSettlementInstrGenService() : base() { }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
#if (DEBUG)
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresSettlementInstrGen, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresSettlementInstrGenService debugService = new SpheresSettlementInstrGenService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
				
			System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresSettlementInstrGenService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new SpheresSettlementInstrGenService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresSettlementInstrGen,null))
                };
            }
				
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void StopProcess()
        {
            if (null != settlementInstrGenProcess)
                settlementInstrGenProcess.StopProcess();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            settlementInstrGenProcess = new SettlementInstrGenProcess(MQueue, AppInstance);
            settlementInstrGenProcess.ProcessStart();
            pIdLog = settlementInstrGenProcess.IdProcess;
            return settlementInstrGenProcess.ProcessState;
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

        protected override void TraceProcess()
        {
            if (null == settlementInstrGenProcess)
                throw new InvalidOperationException($"({nameof(settlementInstrGenProcess)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");


            m_ServiceTrace.TraceProcessAsync(settlementInstrGenProcess, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                settlementInstrGenProcess.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);

        }
    }
}
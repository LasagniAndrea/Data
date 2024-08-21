using System;
using System.Threading;

using EFS.ACommon;
using EFS.Process;
using EFS.Process.MarkToMarket;

// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EFS.SpheresService
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresMarkToMarketGenService : SpheresServiceBase, ISpheresServiceParameters
	{
		#region Members
		private ProcessTradeBase mtmGenProcess;
		#endregion Members

		#region Accessors
		/// <summary>
		/// 
		/// </summary>
		protected override Cst.ServiceEnum ServiceEnum
		{
			get{return  Cst.ServiceEnum.SpheresMarkToMarketGen;} 
		}
		
		#endregion Accessors

		#region Constructor
		public SpheresMarkToMarketGenService(string pServiceName) : base(pServiceName){}
		public SpheresMarkToMarketGenService() : base(){}
		#endregion Constructor

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
        static void Main(string[] args)
        {
#if DEBUG
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresMarkToMarketGen, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresMarkToMarketGenService debugService = new SpheresMarkToMarketGenService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresMarkToMarketGenService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new SpheresMarkToMarketGenService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresMarkToMarketGen,null))
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
			if (null != mtmGenProcess)
                mtmGenProcess.StopProcess();
		}
		
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            mtmGenProcess = new MarkToMarketGenProcess(MQueue, AppInstance);

            mtmGenProcess.ProcessStart();

            
            //if (null != mtmGenProcess.processLog)
            //    pIdLog = mtmGenProcess.processLog.header.IdProcess;
            pIdLog = mtmGenProcess.IdProcess;

            return mtmGenProcess.ProcessState;
        }



        /// <summary>
        /// 
        /// </summary>
        protected override void TraceProcess()
        {
            if (null == mtmGenProcess)
                throw new InvalidOperationException($"({nameof(mtmGenProcess)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");

            m_ServiceTrace.TraceProcessAsync(mtmGenProcess, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                mtmGenProcess.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);

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
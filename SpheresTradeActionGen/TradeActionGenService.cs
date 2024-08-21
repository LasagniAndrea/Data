using System;
using System.Threading;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using EFS.Process;
using EFS.ACommon;
using EFS.Common;  

// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;
using System.Reflection;


namespace EFS.SpheresService
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresTradeActionGenService : SpheresServiceBase, ISpheresServiceParameters
	{
		#region Members
        private ProcessTradeBase tradeActionGenProcess;
		#endregion Members

		#region Accessors
		#region protected serviceEnum
		protected override Cst.ServiceEnum ServiceEnum
		{
			get{return  Cst.ServiceEnum.SpheresTradeActionGen;} 
		}
		#endregion
		#endregion Accessors

		#region Constructor
    	public SpheresTradeActionGenService () : base(){}
		public SpheresTradeActionGenService(string pServiceName) : base(pServiceName){}
		#endregion Constructor

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
        static void Main(string[] args)
        {
#if DEBUG

            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresTradeActionGen, GetServiceVersion(), out string serviceName);

            if (StrFunc.IsFilled(serviceName))
            {
                SpheresTradeActionGenService debugService = new SpheresTradeActionGenService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { 
                        new SpheresTradeActionGenService(args[0].Substring(2)) 
                    };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new SpheresTradeActionGenService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresTradeActionGen,null))
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
			if (null != tradeActionGenProcess)
                tradeActionGenProcess.StopProcess();
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            tradeActionGenProcess = new TradeActionGenProcess(MQueue, AppInstance);
            tradeActionGenProcess.ProcessStart();

            
            //if (null != tradeActionGenProcess.processLog)
            //    pIdLog = tradeActionGenProcess.processLog.header.IdProcess;
            pIdLog = tradeActionGenProcess.IdProcess;

            return tradeActionGenProcess.ProcessState;
        }

        /// <summary>
        /// 
        /// </summary>

        protected override void TraceProcess()
        {

            if (null == tradeActionGenProcess)
                throw new InvalidOperationException($"({nameof(tradeActionGenProcess)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");

            m_ServiceTrace.TraceProcessAsync(tradeActionGenProcess, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                tradeActionGenProcess.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);

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

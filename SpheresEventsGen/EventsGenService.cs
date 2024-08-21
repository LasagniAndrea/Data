#region Using Directives
using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;


using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;

using EFS.Process;
using EFS.Process.EventsGen; 




// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
#endregion Using Directives

namespace EFS.SpheresService
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresEventsGenService : SpheresServiceBase, ISpheresServiceParameters
	{
		#region Members
        private EventsGenProcess eventsGenProcess;
		#endregion Members

		#region Accessors
		#region protected serviceEnum
		protected override Cst.ServiceEnum ServiceEnum
		{
			get{return  Cst.ServiceEnum.SpheresEventsGen;} 
		}
		#endregion
		#endregion Accessors

		#region Constructor
		public SpheresEventsGenService(string pServiceName) : base(pServiceName){}
		public SpheresEventsGenService(): base(){}
		#endregion Constructor

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
        static void Main(string[] args)
        {
#if DEBUG
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresEventsGen, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresEventsGenService debugService = new SpheresEventsGenService(serviceName);
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
                    new SpheresEventsGenService(args[0].Substring(2)) 
                };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new SpheresEventsGenService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresEventsGen,null))
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
			if (null != eventsGenProcess)
                eventsGenProcess.StopProcess();
        }
        
        
        /// <summary>
        /// Lance un process
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            eventsGenProcess = new EventsGenProcess(MQueue, AppInstance);

            eventsGenProcess.ProcessStart();
            
            pIdLog = eventsGenProcess.IdProcess;

            return eventsGenProcess.ProcessState;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void TraceProcess()
        {

            if (null == eventsGenProcess)
                throw new InvalidOperationException($"({nameof(eventsGenProcess)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");

            m_ServiceTrace.TraceProcessAsync(eventsGenProcess, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                eventsGenProcess.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);
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
#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;

using EFS.ACommon;
using EFS.Common.MQueue;

using EFS.Process;
using EFS.Process.Accruals;
using EFS.Process.LinearDepreciation;
using EFS.Process.PosKeeping;
// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;

using SpheresClosingGen.Properties;

#endregion Using Directives



namespace EFS.SpheresService
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresClosingGenService : SpheresServiceBase, ISpheresServiceParameters
	{
		#region Members
		private ProcessTradeBase process;
		#endregion Members
		#region Accessors
		#region protected serviceEnum
		protected override Cst.ServiceEnum ServiceEnum
		{
			get{return  Cst.ServiceEnum.SpheresClosingGen;} 
		}
		#endregion
		#endregion Accessors
		#region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI attention ce constructor est appelé en création d'instance, lors de la desinstallation d'un service
		public SpheresClosingGenService() : base(){}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceName"></param>
		public SpheresClosingGenService(string pServiceName) : base(pServiceName){}
		#endregion Constructor
		#region Methods
		#region Main [principal process entry] 
        static void Main(string[] args)
        {
#if (DEBUG)
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresClosingGen, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresClosingGenService debugService = new SpheresClosingGenService(serviceName);
                //debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresClosingGenService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new SpheresClosingGenService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresClosingGen,null))
                };
            }
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
#endif
        }
		#endregion Main [principal process entry] 
		#region CleanProcess
        protected override void StopProcess() 
		{
			if (null != process)
                process.StopProcess();
		}
		#endregion CleanProcess
        #region ExecuteProcess
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            pIdLog = 0;
            process = null;
            if (MQueue.GetType().Equals(typeof(AccrualsGenMQueue)))
            {
                process = new AccrualsGenProcess(MQueue, AppInstance);
            }
            else if (MQueue.GetType().Equals(typeof(LinearDepGenMQueue)))
            {
                process = new LinearDepGenProcess(MQueue, AppInstance);
            }
            else if (MQueue.GetType().BaseType.Equals(typeof(PosKeepingMQueue)))
            {
                process = new PosKeepingGenProcess(MQueue, AppInstance);
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("type:{0} is not implemented", MQueue.GetType().ToString()));
            
            process.ProcessStart();

            
            pIdLog = process.IdProcess;

            return process.ProcessState;
        }
        #endregion ExecuteProcess

        /// <summary>
        /// 
        /// </summary>
        protected override void TraceProcess()
        {

            if (null == process)
                throw new InvalidOperationException($"({nameof(process)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");


            m_ServiceTrace.TraceProcessAsync(process, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                process.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);

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

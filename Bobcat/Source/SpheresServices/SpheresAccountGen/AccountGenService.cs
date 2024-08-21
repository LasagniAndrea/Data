using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Runtime.InteropServices;

using EFS.ACommon;
using EFS.Process;
using EFS.SpheresAccounting;
// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;
using System.Reflection;

namespace EFS.SpheresService
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class AccountGenService : SpheresServiceBase, ISpheresServiceParameters
	{
		#region Members
		private ProcessTradeBase accProcess;
		#endregion Members
		#region Accessors
		#region protected serviceEnum
		protected override Cst.ServiceEnum ServiceEnum
		{
			get{return  Cst.ServiceEnum.SpheresAccountGen;} 
		}
		#endregion protected serviceEnum
		#endregion Accessors
		#region Constructor
		public AccountGenService() : base(){}
		public AccountGenService(string pServiceName) : base(pServiceName){}
		#endregion Constructor
		#region Methods
		#region Main [principal process entry] 
        static void Main(string[] args)
        {
#if (DEBUG)
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresAccountGen, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                AccountGenService debugService = new AccountGenService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new AccountGenService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new AccountGenService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresAccountGen,null))
                };
            }
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
#endif
        }
		#endregion Main [principal process entry] 
		#region CleanProcess
        protected override void StopProcess() 
		{
			if (null != accProcess)
                accProcess.StopProcess();
		}
		#endregion CleanProcess
        #region ExecuteProcess
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            accProcess = new AccountGenProcess(MQueue, AppInstance);
            accProcess.ProcessStart();
            pIdLog = accProcess.IdProcess;
            return accProcess.ProcessState;
        }
        #endregion ExecuteProcess

        /// <summary>
        /// 
        /// </summary>
        protected override void TraceProcess()
        {

            if (null == accProcess)
                throw new InvalidOperationException($"({nameof(accProcess)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");


            m_ServiceTrace.TraceProcessAsync(accProcess, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                accProcess.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);

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
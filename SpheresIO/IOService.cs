using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection; 
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
//
using EFS.ACommon ;
using EFS.ApplicationBlocks.Data;
using EFS.Common;  
using EFS.LoggerClient;
using EFS.Process;
using EFS.SpheresIO;
// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;

namespace EFS.SpheresService
{
    /// <summary>
    /// Classe du service IO
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresIOService : SpheresServiceBase, ISpheresServiceParameters
	{
		#region Members
		private ProcessBase iOProcess;
		#endregion Members

		#region Accessors
		/// <summary>
        /// 
		/// </summary>
		protected override Cst.ServiceEnum ServiceEnum
		{
			get{return  Cst.ServiceEnum.SpheresIO;} 
		}
        /// <summary>
        /// 
        /// </summary>
        protected override bool IsReplicaMsgWithFileWatcherAvailable
        {
            get
            {
                //On active autant de fois la task I/O qu'il y a des demandes
                return false;
            }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public SpheresIOService() : base(){}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceName"></param>
		public SpheresIOService(string pServiceName) : base(pServiceName){}
		#endregion Constructor

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
        static void Main(string[] args)
        {
#if DEBUG
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresIO, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresIOService debugService = new SpheresIOService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresIOService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new SpheresIOService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresIO,null))
                };
            }

            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
#endif

        }
		
        protected override void StopProcess()
        {
            if (null != iOProcess)
            {
                iOProcess.StopProcess();
            }
        }
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            iOProcess = new SpheresIOProcess(MQueue, AppInstance);
            iOProcess.ProcessStart();
            pIdLog = iOProcess.IdProcess;
            return iOProcess.ProcessState;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void TraceProcess()
        {
            if (null == iOProcess)
                throw new InvalidOperationException($"({nameof(iOProcess)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");

            m_ServiceTrace.TraceProcessAsync(iOProcess, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                iOProcess.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);

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
#region Using Directives
using System;
using System.Threading;
using System.Collections.Specialized;
using System.Collections.Generic;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;

using EFS.Process;

// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;
using System.Reflection;
using System.Runtime.InteropServices;



#endregion Using Directives

namespace EFS.SpheresService
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresQuotationHandlingService : SpheresServiceBase, ISpheresServiceParameters
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private ProcessBase quotationHandlingProcess;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected override Cst.ServiceEnum ServiceEnum
        {
            get { return Cst.ServiceEnum.SpheresQuotationHandling; }
        }

        #endregion Accessors

        #region Constructor
        public SpheresQuotationHandlingService() : base() { }
        public SpheresQuotationHandlingService(string pServiceName) : base(pServiceName) { }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
#if (DEBUG)
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresQuotationHandling, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresQuotationHandlingService debugService = new SpheresQuotationHandlingService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresQuotationHandlingService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new SpheresQuotationHandlingService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresQuotationHandling,null))
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
            if (null != quotationHandlingProcess)
                quotationHandlingProcess.StopProcess();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            quotationHandlingProcess = new QuotationHandlingProcess(MQueue, AppInstance);
            quotationHandlingProcess.ProcessStart();
            pIdLog = quotationHandlingProcess.IdProcess;
            return quotationHandlingProcess.ProcessState;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void TraceProcess()
        {
            if (null == quotationHandlingProcess)
                throw new InvalidOperationException($"({nameof(quotationHandlingProcess)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");

            m_ServiceTrace.TraceProcessAsync(quotationHandlingProcess, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                quotationHandlingProcess.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);

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

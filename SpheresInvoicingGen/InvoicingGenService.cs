#region Using Directives
using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

using EFS.ACommon;
using EFS.Common.MQueue;
using EFS.Process;



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
    public class SpheresInvoicingGenService : SpheresServiceBase, ISpheresServiceParameters
    {
        #region Members
        private ProcessTradeBase invoicingGenProcess;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected override Cst.ServiceEnum ServiceEnum
        {
            get { return Cst.ServiceEnum.SpheresInvoicingGen; }
        }
        
        #endregion Accessors

        #region Constructor
        public SpheresInvoicingGenService(string pServiceName) : base(pServiceName) { }
        public SpheresInvoicingGenService() : base() { }
        #endregion Constructor

        #region Methods

        static void Main(string[] args)
        {
#if DEBUG
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresInvoicingGen, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresInvoicingGenService debugService = new SpheresInvoicingGenService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresInvoicingGenService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new SpheresInvoicingGenService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresInvoicingGen,null))
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
            if (null != invoicingGenProcess)
                invoicingGenProcess.StopProcess();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            invoicingGenProcess = new InvoicingGenProcess(MQueue, AppInstance);
            invoicingGenProcess.ProcessStart();
            pIdLog = invoicingGenProcess.IdProcess;
            return invoicingGenProcess.ProcessState;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void TraceProcess()
        {

            if (null == invoicingGenProcess)
                throw new InvalidOperationException($"({nameof(invoicingGenProcess)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");

            m_ServiceTrace.TraceProcessAsync(invoicingGenProcess, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                invoicingGenProcess.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);

        }
        
        
        #endregion Methods

        
    }
}
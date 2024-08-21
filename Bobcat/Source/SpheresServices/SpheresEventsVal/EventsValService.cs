#region Using Directives
using System;
using System.Threading;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;



using EFS.Process;

// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;
using System.Reflection;


#endregion Using Directives

namespace EFS.SpheresService
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresEventsValService : SpheresServiceBase
    {
        #region Members
        private ProcessBase valProcess;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected override Cst.ServiceEnum ServiceEnum
        {
            get { return Cst.ServiceEnum.SpheresEventsVal; }
        }
        
        #endregion Accessors

        #region Constructor
        public SpheresEventsValService() : base() { }
        public SpheresEventsValService(string pServiceName) : base(pServiceName) { }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
#if (DEBUG)
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresEventsVal, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresEventsValService debugService = new SpheresEventsValService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresEventsValService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] 
                { 
                    new SpheresEventsValService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresEventsVal, null))
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
            if (null != valProcess)
                valProcess.StopProcess();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            pIdLog = 0;
            if (MQueue.GetType().Equals(typeof(EventsValMQueue)))
            {
                valProcess = new EventsValProcess(MQueue, AppInstance);
            }
            else if (MQueue.GetType().Equals(typeof(CollateralValMQueue)))
            {
                valProcess = new CollateralValProcess(MQueue, AppInstance);
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("type:{0} is not implemented", MQueue.GetType().ToString()));

            valProcess.ProcessStart();

            
            //if (null != valProcess.processLog)
            //    pIdLog = valProcess.processLog.header.IdProcess;
            pIdLog = valProcess.IdProcess;

            return valProcess.ProcessState;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void TraceProcess()
        {

            if (null == valProcess)
                throw new InvalidOperationException($"({nameof(valProcess)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");

            m_ServiceTrace.TraceProcessAsync(valProcess, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                valProcess.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);

        }
        
        
        #endregion Methods

        
    }
}

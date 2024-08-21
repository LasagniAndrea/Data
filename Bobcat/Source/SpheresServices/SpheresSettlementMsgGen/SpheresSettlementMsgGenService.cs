using System;
using System.Threading;

using EFS.SpheresService; 
using EFS.ACommon;
using EFS.Common.MQueue;
using EFS.Process;
using EFS.Process.SettlementMessage;


// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EFS.SpheresService
{
    #region SpheresSettlementMsGenService
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresSettlementMsGenService : SpheresServiceBase, ISpheresServiceParameters
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private ProcessBase process;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected override Cst.ServiceEnum ServiceEnum
        {
            get { return Cst.ServiceEnum.SpheresSettlementMsgGen; }
        }

        #endregion Accessors

        #region Constructor
        public SpheresSettlementMsGenService(string pServiceName) : base(pServiceName) { }
        public SpheresSettlementMsGenService() : base() { }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
#if DEBUG
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresSettlementMsgGen, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresSettlementMsGenService debugService = new SpheresSettlementMsGenService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresSettlementMsGenService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new SpheresSettlementMsGenService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresSettlementMsgGen,null))
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
            if (null != process)
                process.StopProcess();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            pIdLog = 0;
            process = null;
            if (MQueue.GetType().Equals(typeof(ESRStandardGenMQueue)))
            {
                process = new ESRStandardGenProcess(MQueue, AppInstance);
            }
            else if (MQueue.GetType().Equals(typeof(ESRNetGenMQueue)))
            {
                process = new ESRNetGenProcess(MQueue, AppInstance);
            }
            else if (MQueue.GetType().Equals(typeof(MSOGenMQueue)))
            {
                process = new MSOGenProcess(MQueue, AppInstance);
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("type:{0} is not implemented", MQueue.GetType().ToString()));

            process.ProcessStart();

            
            //if (null != process.processLog)
            //    pIdLog = process.processLog.header.IdProcess;
            pIdLog = process.IdProcess;

            return process.ProcessState;
        }

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
        
        #endregion Methods

    }
    #endregion
}
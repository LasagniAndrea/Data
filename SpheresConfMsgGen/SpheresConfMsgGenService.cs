using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;

using EFS.ACommon;
using EFS.Common.MQueue;

using EFS.Process;
using EFS.Process.Notification;

// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;

namespace EFS.SpheresService
{
    /// <summary>
    /// Service didié à la messagerie  
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]    
    public class SpheresConfirmationMsgGenService : SpheresServiceBase
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
            get { return Cst.ServiceEnum.SpheresConfirmationMsgGen; }
        }
        #endregion

        #region Constructor
        public SpheresConfirmationMsgGenService(string pServiceName) : base(pServiceName) { }
        public SpheresConfirmationMsgGenService() : base() { }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

#if DEBUG
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresConfirmationMsgGen, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresConfirmationMsgGenService debugService = new SpheresConfirmationMsgGenService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresConfirmationMsgGenService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new SpheresConfirmationMsgGenService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresConfirmationMsgGen,null))
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
        /// FI 20190529 [XXXXX] Refactoring => Usage de MQueue.ProcessType
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            pIdLog = 0;

            switch (MQueue.ProcessType)
            {
                case Cst.ProcessTypeEnum.CIGEN:
                    //Génération des instructions de confirmation
                    process = new ConfInstrGenProcess((ConfirmationInstrGenMQueue)MQueue, AppInstance);
                    break;
                case Cst.ProcessTypeEnum.CMGEN:
                    //Génération des messages de confirmation
                    process = new ConfMsgGenProcess((ConfirmationMsgGenMQueue)MQueue, AppInstance);
                    break;
                case Cst.ProcessTypeEnum.RIMGEN:
                    process = new ReportInstrMsgGenProcess((ReportInstrMsgGenMQueue)MQueue, AppInstance);
                    break;
                case Cst.ProcessTypeEnum.RMGEN:
                    process = new ReportMsgGenProcess((ReportMsgGenMQueue)MQueue, AppInstance);
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("type:{0} is not implemented", MQueue.GetType().ToString()));
            }

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
    
}
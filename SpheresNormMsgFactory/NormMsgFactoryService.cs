#region Using Directives
using System;
using System.Diagnostics;
using System.Threading;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Process;




// Importing base symbols for the service configuration
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
#endregion Using Directives

namespace EFS.SpheresService
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class SpheresNormMsgFactoryService : SpheresServiceBase, ISpheresServiceParameters
    {
        #region Members
        private NormMsgFactoryProcess normMsgFactoryProcess;
        #endregion Members
        #region Accessors
        #region protected serviceEnum
        protected override Cst.ServiceEnum ServiceEnum
        {
            get { return Cst.ServiceEnum.SpheresNormMsgFactory; }
        }
        #endregion
        #endregion Accessors
        #region Constructor
        public SpheresNormMsgFactoryService(string pServiceName) : base(pServiceName) { }
        public SpheresNormMsgFactoryService() : base() { }
        #endregion Constructor
        #region Methods
        #region Main [principal process entry]
        static void Main(string[] args)
        {
#if DEBUG
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresNormMsgFactory, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                SpheresNormMsgFactoryService debugService = new SpheresNormMsgFactoryService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#else
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresNormMsgFactoryService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new SpheresNormMsgFactoryService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresNormMsgFactory,null))
                };
            }

            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
#endif
        }
        #endregion Main [principal process entry]
        #region StopProcess
        protected override void StopProcess()
        {
            if (null != normMsgFactoryProcess)
                normMsgFactoryProcess.StopProcess();
        }
        #endregion StopProcess
        #region ExecuteProcess
        /// <summary>
        /// Lance un process
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        // EG 20180525 [23979] IRQ Processing
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            // Suppression des sémaphores d'interruption en fin de vie (+1jour)
            RemoveEndOfLifeIRQSemaphore();
            normMsgFactoryProcess = new NormMsgFactoryProcess(MQueue, AppInstance);
            normMsgFactoryProcess.ProcessStart();

            
            //if (null != normMsgFactoryProcess.processLog)
            //    pIdLog = normMsgFactoryProcess.processLog.header.IdProcess;
            pIdLog = normMsgFactoryProcess.IdProcess;

            return normMsgFactoryProcess.ProcessState;
        }
        #endregion ExecuteProcess
        /// <summary>
        /// 
        /// </summary>
        protected override void TraceProcess()
        {

            if (null == normMsgFactoryProcess)
                throw new InvalidOperationException($"({nameof(normMsgFactoryProcess)} is null");

            if (null == m_ServiceTrace)
                throw new InvalidOperationException($"({nameof(m_ServiceTrace)} is null");

            if (null == MQueue)
                throw new InvalidOperationException($"({nameof(MQueue)} is null");

            m_ServiceTrace.TraceProcessAsync(normMsgFactoryProcess, GetNumberMessageInQueue(MQueue.ProcessType)).Wait();
            if (MQueue.IsMessageObserver)
                normMsgFactoryProcess.Tracker.UpdateIdData(Cst.OTCml_TBL.SERVICE_L.ToString(), m_ServiceTrace.GetService(MQueue.ConnectionString, MQueue.ProcessType).IdService);
        }
        
        

        #region RemoveEndOfLifeIRQSemaphore
        /// <summary>
        /// Suppression des sémaphores en fin de vie = créées il y a plus d'1 journée 
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190318 Dispose des Semaphore vieille de plus de 10H lors de l'activation de NormMsgFactory
        private void RemoveEndOfLifeIRQSemaphore()
        {
            if (0 < AppInstance.LstIRQSemaphore.Count)
            {
                List<IRQSemaphore> lstToDeactivate = AppInstance.LstIRQSemaphore.FindAll(irqSemaphore => irqSemaphore.userRequester.Item3.AddHours(10).CompareTo(DateTime.Now) <= 0);
                if (null != lstToDeactivate)
                {
                    lstToDeactivate.ForEach(item =>
                    {
                        if (null != item.semaphore)
                        {
                            item.semaphore.Close();
                            item.semaphore.Dispose();
                            item.semaphore = null;
                        }
                    });
                    AppInstance.LstIRQSemaphore.RemoveAll(item => (null == item.semaphore));
                }
            }
        }
        #endregion RemoveEndOfLifeIRQSemaphore
        #endregion Methods
    }
}
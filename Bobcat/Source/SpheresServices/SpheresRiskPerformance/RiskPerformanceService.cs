using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Threading;

using EFS.SpheresService;
using EFS.ACommon;
using EFS.Common.MQueue;
using EFS.Process;
using EFS.SpheresServiceParameters;
using EFS.SpheresServiceParameters.SampleForms;


using EFS.SpheresRiskPerformance.Properties;
using EFS.SpheresRiskPerformance.CashBalance;
using EFS.SpheresRiskPerformance.CashBalanceInterest;

namespace EFS.SpheresRiskPerformance
{
    class RiskPerformanceService : SpheresServiceBase, ISpheresServiceParameters
    {

        #region Members
        /// <summary>
        /// 
        /// </summary>
        private ProcessBase process;
        #endregion Members

        protected override Cst.ServiceEnum ServiceEnum
        {
            get
            {
                return Cst.ServiceEnum.SpheresRiskPerformance;
            }
        }


        private static string ServiceEventsGenKeyName
        {
            get
            {
                string serviceKeyName = String.Empty;

                serviceKeyName = SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresEventsGen, null);

#if DEBUG
                serviceKeyName = String.Concat(serviceKeyName, "DEBUG");
#endif

                return serviceKeyName;
            }
        }

        public RiskPerformanceService()
            : base()
        { }

        public RiskPerformanceService(string pServiceName)
            : base(pServiceName)
        { }


        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">null</param>
        public static void Main(string[] args)
        {

#if DEBUG
            ServiceTools.CreateRegistryServiceInformation(Cst.ServiceEnum.SpheresRiskPerformance, GetServiceVersion(), out string serviceName);
            if (StrFunc.IsFilled(serviceName))
            {
                RiskPerformanceService debugService = new RiskPerformanceService(serviceName);
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service created");
                debugService.ActivateService();
                debugService.WriteEventLog_SystemInformation(Cst.MOM.MOMEnum.Unknown, Cst.ErrLevel.SUCCESS, @"#Debug mode: Service activated");
                Thread.Sleep(-1);
            }
#endif

#if !DEBUG

            System.ServiceProcess.ServiceBase[] ServicesToRun;

            if ((null != args) && (0 < args.Length))
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new RiskPerformanceService(args[0].Substring(2)) };
            }
            else
            {
                ServicesToRun = new System.ServiceProcess.ServiceBase[]
                { 
                    new RiskPerformanceService(
                        SpheresServiceBase.ConstructServiceName(Cst.ServiceEnum.SpheresRiskPerformance,null))
                };
            }

            System.ServiceProcess.ServiceBase.Run(ServicesToRun);

#endif

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        protected override ProcessState ExecuteProcess(out int pIdLog)
        {
            pIdLog = 0;
            ProcessState processState = new ProcessState(ProcessStateTools.StatusEnum.PROGRESS);

            RiskPerformanceMQueue riskPerfMessage = MQueue as RiskPerformanceMQueue;
            CashBalanceInterestMQueue cashBalanceInterestMessage = MQueue as CashBalanceInterestMQueue;

            string errorMessage = String.Empty;
            if (this.AppInstance == null)
            {
                // when the service appinstance is null, then something bad is happen while the service registry key has been reading
                processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.INITIALIZE_ERROR);
                errorMessage = String.Format(Ressource.GetString("RiskPerformance_ERRInitRegistryKey"),
                    String.Concat(RegistryConst.RegistryKey, "RiskPerformanceService"));
            }

            if (ProcessStateTools.IsStatusProgress(processState.Status))
            {
                //PM 20120809 [18058] Add cashBalanceInterestMessage
                if ((null == riskPerfMessage) && (null == cashBalanceInterestMessage))
                {
                    // when the deserialised message type is wrong, we return a message cast error
                    processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.MESSAGE_CAST_ERROR);
                    errorMessage = String.Format(Ressource.GetString("RiskPerformance_ERRMessageType"),
                        (null != MQueue) ? MQueue.GetType().ToString() : Cst.NotAvailable, this.Environment);
                }
            }


#if DEBUGDEV

            WriteEventLog( ProcessStateTools.StatusNoneEnum, Cst.ErrLevel.SUCCESS, "Test process executed in debug mode");
#endif

#if RELEASEDEV

            WriteEventLog(ProcessStateTools.StatusNoneEnum, Cst.ErrLevel.SUCCESS,"Test process executed in release mode");
#endif

            // if the Status is OK, the service can execute the process request received from the mqueue
            if (ProcessStateTools.IsStatusProgress(processState.Status))
            {
                // RD 20110816 [App.Rest.]
                // Utiliser le même service pour:
                // 1- Calcul de Déposit
                // 2- Calcul des Appels/Restitutions et Soldes 

                if (null != cashBalanceInterestMessage)
                {
                    process = new CashBalanceInterestProcess(cashBalanceInterestMessage, AppInstance);
                }
                else if (null != riskPerfMessage)
                {
                    if (riskPerfMessage.IsCashBalanceProcess)
                        process = new CashBalanceProcess(riskPerfMessage, AppInstance);
                    else if (riskPerfMessage.IsDepositProcess)
                        process = new RiskPerformanceProcess(riskPerfMessage, AppInstance);
                    else
                        throw new NotImplementedException(StrFunc.AppendFormat("process :{0} is not implemented", riskPerfMessage.ProcessType.ToString()));
                }

                //
                // PROCESS START...
                process.ProcessStart();

                // PROCESS END
                processState = process.ProcessState;

                 
                pIdLog = process.IdProcess;
            }
            else
            {
                //PL 20130701 [WindowsEvent Project]
                //            Message qui diffère des autres services!!! On ne peut docn faire appel classiquement à WriteEventLog_SystemError()
                //            Situation à élucider...
                WriteEventLog_BusinessError(processState.Status, processState.CodeReturn, null, MethodInfo.GetCurrentMethod().Name, errorMessage);
            }

            return processState;
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void StopProcess()
        {
            //FI 20190821 [XXXXX]  Appel à StopProcess
            //base.StopProcess();
            if (null != process)
                process.StopProcess();
        }


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
    }
}

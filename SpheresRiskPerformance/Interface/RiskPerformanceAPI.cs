using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Process;
using EFS.SpheresRiskPerformance.CashBalance;
using System;


namespace EFS.SpheresRiskPerformance.Interface
{
    /// <summary>
    /// Static helper class allowing to call a risk margin evaluation 
    /// </summary>
    public static class RiskPerformanceAPI
    {

        /// <summary>
        /// Instance a new process instance
        /// </summary>
        /// <param name="pQueue">Execution parameters</param>
        /// <param name="pService">Reference at the current service, creating 
        /// the RiskPerformance process instance</param>
        /// <returns>a new RiskPerformanceProcess, ready to be executed</returns>
        public static RiskPerformanceProcess InstanceProcess(RiskPerformanceMQueue pQueue, AppInstanceService pService)
        {
            RiskPerformanceProcess process = new RiskPerformanceProcess(pQueue, pService);
            return process;
        }
        /// <summary>
        /// One time execution of a risk margin evaluation
        /// </summary>
        /// <param name="pQueue">Execution parameters</param>
        /// <param name="pCallProcess">Reference at the current process, calling the risk margin evaluation</param>
        /// <returns></returns>
        // EG 20180205 [23769]
        // EG 20180307 [23769] Gestion Asynchrone (plus de parametres Async = Gérer dans .Config)
        public static ProcessState ExecuteSlaveCall(RiskPerformanceMQueue pQueue, ProcessBase pCallProcess)
        {
            return ExecuteSlaveCall(pQueue, pCallProcess, true);
        }
        /// <summary>
        /// One time execution of a risk margin evaluation
        /// </summary>
        /// <param name="pQueue">Execution parameters</param>
        /// <param name="pCallProcess">Reference at the current process, calling the risk margin evaluation</param>
        /// <param name="pIsSendMessage_PostProcess"></param>
        /// <returns></returns>
        /// FI 20161021 [22152] Modify
        // EG 20180205 [23769]
        // EG 20180307 [23769] Gestion Asynchrone (plus de parametres Async = Gérer dans .Config)
        // EG 20190613 [24683] Upd Set Tracker To Slave
        public static ProcessState ExecuteSlaveCall(RiskPerformanceMQueue pQueue, ProcessBase pCallProcess, bool pIsSendMessage_PostProcess)
        {
            ProcessBase process;
            if (pQueue.IsCashBalanceProcess)
                process = new CashBalanceProcess(pQueue, pCallProcess.AppInstance);
            else if (pQueue.IsDepositProcess) // FI 20161021 [22152] add else if
                process = new RiskPerformanceProcess(pQueue, pCallProcess.AppInstance);
            else // FI 20161021 [22152] add throw new NotImplementedException
                throw new NotImplementedException(StrFunc.AppendFormat("ProcessType : {0} is not implemented", pQueue.ProcessType.ToString()));

            
            process.ProcessCall = ProcessBase.ProcessCallEnum.Slave;
            process.IsSendMessage_PostProcess = pIsSendMessage_PostProcess;
            
            process.IdProcess = pCallProcess.IdProcess;
            //
            process.LogDetailEnum = pCallProcess.LogDetailEnum;
            // FI 20190605 [XXXXX] valorisation du tracker
            process.Tracker = pCallProcess.Tracker;
            process.ProcessStart();

            return process.ProcessState;
        }
    }
}
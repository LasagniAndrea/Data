using EFS.ACommon;
using EFS.Common;
using EFS.Process;
using System;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Extension de Class factory pour la gestion ASYNCHRONE
    /// </summary>
    public sealed partial class RiskMethodFactory
    {
        /// <summary>
        /// Evaluation des déposit pour une chambre
        /// </summary>
        /// <param name="pProcessBase">Process appelant</param>
        /// <param name="pIdCss">Identifiant CSS</param>
        // EG 20180205 [23769]
        // EG 20180525 [23979] IRQ Processing
        public void EvaluateDeposits(RiskCommonProcessBase pProcessBase, CalculationSheet.CalculationSheetRepository pLogsRepository, int pIdCss)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (m_MethodSet.TryGetValue(pIdCss, out RiskMethodSet methodSet))
            {
                if (false == IRQTools.IsIRQRequested(pProcessBase, pProcessBase.IRQNamedSystemSemaphore, ref codeReturn))
                {
                    AppInstance.TraceManager.TraceVerbose(this, String.Format("MethodSet.EvaluateNetMargining (CSS: {0})", pIdCss.ToString()));
                    methodSet.EvaluateNetMarginingThreading(pProcessBase, pLogsRepository);

                    if ((Cst.ErrLevel.IRQ_EXECUTED != pProcessBase.ProcessState.CodeReturn) &&
                        (false == IRQTools.IsIRQRequested(pProcessBase, pProcessBase.IRQNamedSystemSemaphore, ref codeReturn)))
                    {
                        AppInstance.TraceManager.TraceVerbose(this, String.Format("MethodSet.EvaluateGrossMargining (CSS: {0})", pIdCss.ToString()));
                        methodSet.EvaluateGrossMargining(pLogsRepository);

                        if (Cst.ErrLevel.IRQ_EXECUTED != pProcessBase.ProcessState.CodeReturn)
                        {
                            if (false == IRQTools.IsIRQRequested(pProcessBase, pProcessBase.IRQNamedSystemSemaphore, ref codeReturn))
                            {
                                AppInstance.TraceManager.TraceVerbose(this, String.Format("MethodSet.WeightingRatio (CSS: {0})", pIdCss.ToString()));
                                methodSet.WeightingRatio();
                                AppInstance.TraceManager.TraceVerbose(this, String.Format("MethodSet.ResetParameters (CSS: {0})", pIdCss.ToString()));
                                methodSet.ResetParameters();
                            }
                        }
                    }
                }
            }

            if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                pProcessBase.ProcessState.CodeReturn = codeReturn;
        }
    }
}
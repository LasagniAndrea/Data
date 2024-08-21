#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Status;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.EventsGen
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EventsGenProductCalculationBase 
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        protected string m_ConnectionString;
        /// <summary>
        /// 
        /// </summary>
        protected TradeInfo  m_tradeInfo;
        /// <summary>
        /// 
        /// </summary>
        protected string CS
        {
            get { return m_ConnectionString; }
        }
        #endregion Members
        #region Constructors
        public EventsGenProductCalculationBase(string pCS, TradeInfo pTradeInfo)
        {
            m_ConnectionString = pCS;
            m_tradeInfo = pTradeInfo;
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Génération des éléments de calcul nécessaires à la génération des évènements 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public virtual Cst.ErrLevel Calculation(ProcessBase pProcess, IProduct pProduct)
        {
            // FI 20200623 [XXXXX] SetErrorWarning
            pProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5), 0, new LogParam(MethodInfo.GetCurrentMethod().Name)));

            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected Cst.ErrLevel CalcProvisions(IProduct pProduct)
        {
            IDeclarativeProvision declarativeProvision = (IDeclarativeProvision)pProduct;
            #region CancelableProvision
            if (declarativeProvision.CancelableProvisionSpecified)
            {
                ICancelableProvision cancelableProvision = declarativeProvision.CancelableProvision;
                cancelableProvision.Efs_ExerciseDates = new EFS_ExerciseDates(m_ConnectionString, cancelableProvision, m_tradeInfo.tradeLibrary.DataDocument);
            }
            #endregion CancelableProvision
            #region ExtendibleProvision
            if (declarativeProvision.ExtendibleProvisionSpecified)
            {
                IExtendibleProvision exp = declarativeProvision.ExtendibleProvision;
                exp.Efs_ExerciseDates = new EFS_ExerciseDates(m_ConnectionString, exp, m_tradeInfo.tradeLibrary.DataDocument);
            }
            #endregion ExtendibleProvision
            #region MandatoryEralyTermination
            if (declarativeProvision.EarlyTerminationProvisionSpecified &&
                declarativeProvision.EarlyTerminationProvision.MandatorySpecified)
            {
                IMandatoryEarlyTermination met = declarativeProvision.EarlyTerminationProvision.Mandatory;
                met.Efs_MandatoryEarlyTerminationDates = new EFS_MandatoryEarlyTerminationDates(m_ConnectionString, met, m_tradeInfo.tradeLibrary.DataDocument);
            }
            #endregion MandatoryEralyTermination
            #region OptionalEarlyTermination
            if (declarativeProvision.EarlyTerminationProvisionSpecified &&
                declarativeProvision.EarlyTerminationProvision.OptionalSpecified)
            {
                IOptionalEarlyTermination oet = declarativeProvision.EarlyTerminationProvision.Optional;
                oet.Efs_ExerciseDates = new EFS_ExerciseDates(m_ConnectionString, oet, m_tradeInfo.tradeLibrary.DataDocument);
            }
            #endregion OptionalEarlyTermination
            #region StepUpProvision
            if (declarativeProvision.StepUpProvisionSpecified)
            {
                IStepUpProvision sup = declarativeProvision.StepUpProvision;
                sup.Efs_ExerciseDates = new EFS_ExerciseDates(m_ConnectionString, sup, m_tradeInfo.tradeLibrary.DataDocument);
            }
            #endregion StepUpProvision
            return Cst.ErrLevel.SUCCESS;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <param name="pInterestRateStreams"></param>
        /// <returns></returns>
        protected Cst.ErrLevel CalcInterestRateStreams(ProcessBase pProcess, IInterestRateStream[] pInterestRateStreams)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            ICalculationPeriodDates calculationPeriodDates = null;
            foreach (IInterestRateStream stream in pInterestRateStreams)
            {
                ret = CalcInterestRateStream(pProcess, stream, ref calculationPeriodDates);
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    foreach (IInterestRateStream streamRef in pInterestRateStreams)
                    {
                        if (streamRef.CalculationPeriodDates.Id == calculationPeriodDates.Id)
                        {
                            streamRef.CalculationPeriodDates.Efs_CalculationPeriodDates = calculationPeriodDates.Efs_CalculationPeriodDates;
                            break;
                        }
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <param name="pStream"></param>
        /// <param name="pCalcPeriod"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Call ReflectionTools.GetObjectById (substitution to the static class EFS_CURRENT)  
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel CalcInterestRateStream(ProcessBase pProcess, IInterestRateStream pStream, ref ICalculationPeriodDates pCalcPeriod)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            string hRef = string.Empty;
            IPaymentDates paymentDates = pStream.PaymentDates;
            #region Get CalculationPeriodDate [CalculationPeriodDateReference and/or ResetDateReference]
            if (paymentDates.CalculationPeriodDatesReferenceSpecified)
            {
                hRef = paymentDates.CalculationPeriodDatesReference.HRef;
                pCalcPeriod = (ICalculationPeriodDates)ReflectionTools.GetObjectById(m_tradeInfo.tradeLibrary.DataDocument.DataDocument.Item, hRef);
            }
            else if (paymentDates.ResetDatesReferenceSpecified)
            {
                hRef = paymentDates.ResetDatesReference.HRef;
                IResetDates resetDates = (IResetDates)ReflectionTools.GetObjectById(m_tradeInfo.tradeLibrary.DataDocument.DataDocument.Item, hRef);
                if (resetDates != null)
                    hRef = resetDates.CalculationPeriodDatesReference.HRef;
                pCalcPeriod = (ICalculationPeriodDates)ReflectionTools.GetObjectById(m_tradeInfo.tradeLibrary.DataDocument.DataDocument.Item, hRef);
            }
            #endregion Get CalculationPeriodDate [CalculationPeriodDateReference and/or ResetDateReference]
            if (null != pCalcPeriod)
            {
                #region CalculationPeriodDates
                pCalcPeriod.Efs_CalculationPeriodDates = new EFS_CalculationPeriodDates(m_ConnectionString, pCalcPeriod, pStream, m_tradeInfo.tradeLibrary.DataDocument);
                #endregion CalculationPeriodDates
                #region ResetDates
                if (pStream.ResetDatesSpecified)
                {
                    int[] indexId = GetIndexes(pStream);
                    pStream.ResetDates.Efs_ResetDates = new EFS_ResetDates(m_ConnectionString, pStream.ResetDates, pCalcPeriod, indexId, m_tradeInfo.tradeLibrary.DataDocument);
                }
                #endregion ResetDates
                #region CapFloored
                if (pStream.IsCapFloored)
                {
                    ICalculation calculation = pStream.CalculationPeriodAmount.Calculation;
                    _ = new EFS_CapFlooreds(m_ConnectionString, pStream.PayerPartyReference, pStream.ReceiverPartyReference,
                        calculation.RateFloatingRate, pCalcPeriod, m_tradeInfo.tradeLibrary.DataDocument);
                }
                #endregion CapFloored
                #region PaymentDates
                paymentDates.Efs_PaymentDates = new EFS_PaymentDates(m_ConnectionString, paymentDates, pCalcPeriod, pStream.ResetDates, pStream, m_tradeInfo.tradeLibrary.DataDocument);
                #endregion PaymentDates
                #region FxLinkedNotionalDates
                if ((pStream.CalculationPeriodAmount.CalculationSpecified) &&
                    (pStream.CalculationPeriodAmount.Calculation.FxLinkedNotionalSpecified))
                {
                    ICalculation calculation = pStream.CalculationPeriodAmount.Calculation;
                    IFxLinkedNotionalSchedule fxLinkedNotional = calculation.FxLinkedNotional;
                    fxLinkedNotional.Efs_FxLinkedNotionalDates = new EFS_FxLinkedNotionalDates(m_ConnectionString, fxLinkedNotional, m_tradeInfo.tradeLibrary.DataDocument);
                    pCalcPeriod.Efs_CalculationPeriodDates.CreateFxLinkedNotionalPeriod(fxLinkedNotional, pStream);
                }
                #endregion FxLinkedNotionalDates
                #region SetCurrencyPayment
                paymentDates.Efs_PaymentDates.SetCurrencyPayment(pCalcPeriod);
                #endregion SetCurrencyPayment
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                pProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 546), 0,
                    new LogParam(LogTools.IdentifierAndId(pProcess.MQueue.Identifier, pProcess.CurrentId)),
                    new LogParam((hRef ?? Cst.NotAvailable))));

                throw new Exception();
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStream"></param>
        /// <returns></returns>
        /// EG 20110406 Ticket:17374 (Stub sans stubCalculationPeriodAmountSpecified)
        private static int[] GetIndexes(IInterestRateStream pStream)
        {

            int[] indexId = new int[5] { 0, 0, 0, 0, 0 };
            ICalculationPeriodAmount calculationPeriodAmount = pStream.CalculationPeriodAmount;
            #region Regular Indexes
            int i = 0;
            if (calculationPeriodAmount.CalculationSpecified)
            {
                if (calculationPeriodAmount.Calculation.RateFloatingRateSpecified)
                    indexId[i] = calculationPeriodAmount.Calculation.RateFloatingRate.FloatingRateIndex.OTCmlId;
                else if (calculationPeriodAmount.Calculation.RateInflationRateSpecified)
                    indexId[i] = calculationPeriodAmount.Calculation.RateInflationRate.FloatingRateIndex.OTCmlId;
            }
            #endregion Regular Indexes
            #region Initial Stub Indexes
            if (pStream.StubCalculationPeriodAmountSpecified)
            {
                IStubCalculationPeriodAmount stubCalculationPeriodAmount = pStream.StubCalculationPeriodAmount;
                if (stubCalculationPeriodAmount.InitialStubSpecified)
                {
                    IStub stub = stubCalculationPeriodAmount.InitialStub;
                    if (stub.StubTypeFloatingRateSpecified)
                    {
                        for (int j = 0; j < stub.StubTypeFloatingRate.Length; j++)
                        {
                            if (1 < j) break;
                            i++;
                            indexId[i] = stub.StubTypeFloatingRate[j].FloatingRateIndex.OTCmlId;
                        }
                    }
                }
                else
                {
                    i++;
                    indexId[i] = indexId[0];
                }
            }
            else if (pStream.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
            {
                indexId[1] = indexId[0];
                i += 2;
            }
            else
                i += 2;
            #endregion Initial Stub Indexes
            #region Final Stub Indexes
            if (pStream.StubCalculationPeriodAmountSpecified)
            {
                IStubCalculationPeriodAmount stubCalculationPeriodAmount = pStream.StubCalculationPeriodAmount;
                if (stubCalculationPeriodAmount.FinalStubSpecified)
                {
                    IStub stub = stubCalculationPeriodAmount.FinalStub;
                    if (stub.StubTypeFloatingRateSpecified)
                    {
                        for (int j = 0; j < stub.StubTypeFloatingRate.Length; j++)
                        {
                            if (1 < j) break;
                            i++;
                            indexId[i] = stub.StubTypeFloatingRate[j].FloatingRateIndex.OTCmlId;
                        }
                    }
                }
                else
                {
                    i++;
                    indexId[i] = indexId[0];
                }
            }
            else if (pStream.CalculationPeriodDates.LastRegularPeriodEndDateSpecified)
            {
                indexId[3] = indexId[0];
            }
            #endregion Final Stub Indexes
            return indexId;
        }
        #endregion Methods
    }

    /// <summary>
    /// 
    /// </summary>
    public class EventGenCalculationTools
    {
        /// <summary>
        /// Mise en place des éléments de calcul nécessaire à la génération des évènements rattachés à l'array pPayment (ex. alimentation des EFS_Payment)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// <param name="pPayments"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190613 [24683] Use DbTransaction
        [Obsolete("Use the same function on PaymentTools", true)]
        public static Cst.ErrLevel CalcPayments(string pCS, IProduct pProduct, IPayment[] pPayments, DataDocumentContainer pDataDocument)
        {
            return CalcPayments(pCS, null, pProduct, pPayments, pDataDocument);
        }
        /// <summary>
        /// Mise en place des éléments de calcul nécessaire à la génération des évènemnts rattachés à l'array pPayment (ex. alimentation des EFS_Payment)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pProduct"></param>
        /// <param name="pPayments"></param>
        /// <param name="pDataDocument"></param>
        /// <returns></returns>
        // EG 20190603 [24683] Use DbTransaction
        [Obsolete("Use the same function on PaymentTools", true)]
        public static Cst.ErrLevel CalcPayments(string pCS, IDbTransaction pDbTransaction, IProduct pProduct, IPayment[] pPayments, DataDocumentContainer pDataDocument)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            foreach (IPayment payment in pPayments)
            {
                payment.Efs_Payment = new EFS_Payment(pCS, pDbTransaction, payment, pProduct, pDataDocument);
                ret = payment.Efs_Payment.ErrLevel;
            }
            return ret;
        }
    }
}
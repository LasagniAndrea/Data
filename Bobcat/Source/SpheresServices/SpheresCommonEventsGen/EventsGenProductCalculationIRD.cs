#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.EventsGen
{
    /// <summary>
    /// 
    /// </summary>
    public class EventsGenProductCalculationIRD : EventsGenProductCalculationBase
    {
        #region Constructors
        public EventsGenProductCalculationIRD(string pCS, TradeInfo pTradeInfo)
            : base(pCS, pTradeInfo) { }
        #endregion Constructors
        //
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Calculation(ProcessBase pProcess, IProduct pProduct)
        {
            Cst.ErrLevel ret;
            if (pProduct.ProductBase.IsBulletPayment)
            {
                #region BulletPayment
                IBulletPayment bulletPayment = (IBulletPayment)pProduct;
                bulletPayment.Payment.Efs_Payment = new EFS_Payment(m_ConnectionString, null, bulletPayment.Payment, m_tradeInfo.tradeLibrary.DataDocument);
                ret = bulletPayment.Payment.Efs_Payment.ErrLevel;
                #endregion BulletPayment
            }
            else if (pProduct.ProductBase.IsCapFloor)
            {
                #region CapFloor
                ICapFloor capFloor = (ICapFloor)pProduct;
                #region InterestRateStream
                ICalculationPeriodDates calculationPeriodDates = null;
                ret = CalcInterestRateStream(pProcess, capFloor.Stream, ref calculationPeriodDates);
                #endregion InterestRateStream
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    capFloor.Stream.CalculationPeriodDates = calculationPeriodDates;
                    #region Provisions
                    ret = CalcProvisions(pProduct);
                    #endregion Provisions
                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        #region AdditionalPayment
                        if (capFloor.AdditionalPaymentSpecified)
                            ret = PaymentTools.CalcPayments(CS, pProduct, capFloor.AdditionalPayment, m_tradeInfo.tradeLibrary.DataDocument);
                        #endregion AdditionalPayment
                    }
                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        #region Premium
                        if (capFloor.PremiumSpecified)
                            ret = PaymentTools.CalcPayments(CS, pProduct, capFloor.Premium, m_tradeInfo.tradeLibrary.DataDocument);
                        #endregion Premium
                    }
                }
                #endregion CapFloor
            }
            else if (pProduct.ProductBase.IsFra)
            {
                #region Fra
                IFra fra = (IFra)pProduct;
                fra.Efs_FraDates = new EFS_FraDates(m_ConnectionString, fra.PaymentDate, fra.FixingDateOffset, fra.AdjustedTerminationDate.DateValue, 
                    fra, m_tradeInfo.tradeLibrary.DataDocument);
                ret = fra.Efs_FraDates.ErrLevel;
                #endregion Fra
            }
            else if (pProduct.ProductBase.IsLoanDeposit)
            {
                #region LoanDeposit
                ILoanDeposit loanDeposit = (ILoanDeposit)pProduct;
                #region InterestRateStreams
                ret = CalcInterestRateStreams(pProcess, loanDeposit.Stream);
                #endregion InterestRateStreams
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    #region Provisions
                    ret = CalcProvisions(pProduct);
                    #endregion Provisions
                }
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    #region AdditionalPayment
                    if (loanDeposit.AdditionalPaymentSpecified)
                        ret = PaymentTools.CalcPayments(CS, pProduct, loanDeposit.AdditionalPayment, m_tradeInfo.tradeLibrary.DataDocument);
                    #endregion AdditionalPayment
                }
                #endregion LoanDeposit
            }
            else if (pProduct.ProductBase.IsSwap)
            {
                #region Swap
                ISwap swap = (ISwap)pProduct;
                #region InterestRateStreams
                ret = CalcInterestRateStreams(pProcess, swap.Stream);
                #endregion InterestRateStreams
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    #region Provisions
                    ret = CalcProvisions(pProduct);
                    #endregion Provisions
                }
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    #region AdditionalPayment
                    if (swap.AdditionalPaymentSpecified)
                        ret = PaymentTools.CalcPayments(CS, pProduct, swap.AdditionalPayment, m_tradeInfo.tradeLibrary.DataDocument);
                    #endregion AdditionalPayment
                }
                #endregion Swap
            }
            else if (pProduct.ProductBase.IsSwaption)
            {
                #region Swaption
                ISwaption swaption = (ISwaption)pProduct;
                #region ExerciseDates
                ((IExercise)swaption.EFS_Exercise).Efs_ExerciseDates = new EFS_ExerciseDates(m_ConnectionString, swaption, m_tradeInfo.tradeLibrary.DataDocument);
                #endregion ExerciseDates
                #region SwaptionDates
                swaption.Efs_SwaptionDates = new EFS_SwaptionDates(m_ConnectionString, swaption, m_tradeInfo.tradeLibrary.DataDocument);
                #endregion SwaptionDates
                #region Premium
                if (swaption.PremiumSpecified)
                    ret = PaymentTools.CalcPayments(CS, pProduct, swaption.Premium, m_tradeInfo.tradeLibrary.DataDocument);
                else
                    ret = Cst.ErrLevel.SUCCESS;
                #endregion Premium
                //if (Cst.ErrLevel.SUCCESS == ret)
                //{
                //    #region Swap Underlyer
                //    ISwap swap = (ISwap)swaption.swap;
                //    #region InterestRateStreams
                //    ret = CalcInterestRateStreams(pProduct, swap.stream);
                //    #endregion InterestRateStreams
                //    if (Cst.ErrLevel.SUCCESS == ret)
                //    {
                //        #region Provisions
                //        ret = CalcProvisions(pProduct);
                //        #endregion Provisions
                //    }
                //    if (Cst.ErrLevel.SUCCESS == ret)
                //    {
                //        #region AdditionalPayment
                //        if (swaption.swap.additionalPaymentSpecified)
                //            ret = CalcPayments(pProduct, swap.additionalPayment);
                //        #endregion AdditionalPayment
                //    }
                //    #endregion Swap Underlyer
                //}
                #endregion Swaption
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                pProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 20), 0, new LogParam(pProduct.ProductBase.ProductName)));

                throw new NotImplementedException();
            }
            return ret;
        }
        #endregion Methods
    }
    
}

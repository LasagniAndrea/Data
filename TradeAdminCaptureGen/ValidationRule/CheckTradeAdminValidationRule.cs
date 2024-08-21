#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EfsML.Business;
using EfsML.Interface;
using System;
using System.Collections;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CheckTradeAdminValidationRule
    /// <summary>
    /// Validation de la saisie des factures
    /// </summary>
    ///<remarks>
    /// 
    ///</remarks> 
    public class CheckTradeAdminValidationRule : CheckTradeValidationRuleBase
    {
        #region Members
        private readonly TradeAdminInput m_Input;
        #endregion Members
        #region Accessors

        #endregion Accessors
        #region Constructors
        // EG 20171115 Upd Add CaptureSessionInfo
        public CheckTradeAdminValidationRule(TradeAdminInput pTradeAdminInput, Cst.Capture.ModeEnum pCaptureModeEnum)
            : base(pTradeAdminInput.SQLInstrument, pCaptureModeEnum)
        {
            m_Input = pTradeAdminInput;
        }
        #endregion constructor
        #region Methods
        // EG 20091126 New rules (AllocatedAmount overflow on Settlement & UnallocatedAmount on selected invoice 
        // EG 20110209 Ticket 17345
        #region CheckInvoiceSettlement
        private void CheckInvoiceSettlement(string pCS)
        {
            IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)m_Input.DataDocument.CurrentProduct.Product;
            decimal unallocatedAmount = invoiceSettlement.UnallocatedAmount.Amount.DecValue;
            if (IsCheckError)
            {

                if (unallocatedAmount < 0)
                {
                    string[] paramError = new string[2]{
                        StrFunc.FmtDecimalToInvariantCulture(Math.Abs(unallocatedAmount)),
                        invoiceSettlement.UnallocatedAmount.Currency
                    };
                    SetValidationRuleError("Msg_ValidationRule_UnallocatedAmount", paramError);
                }

                if (invoiceSettlement.AllocatedInvoiceSpecified)
                {
                    bool isExistSelectedInvoiceUnallocated = false;
                    foreach (IAllocatedInvoice allocated in invoiceSettlement.AllocatedInvoice)
                    {
                        if ((null == allocated.AllocatedAmounts) || (0 == allocated.AllocatedAmounts.AccountingAmount.Amount.DecValue))
                        {
                            isExistSelectedInvoiceUnallocated = true;
                            break;
                        }
                    }
                    if (isExistSelectedInvoiceUnallocated)
                        SetValidationRuleError("Msg_ValidationRule_UnAllocatedSelectedInvoice");
                }

            }

            if (IsCheckWarning)
            {
                decimal bankCharges = 0;
                decimal vatBankCharges = 0;
                decimal fxGainOrLossAmount = 0;
                if (invoiceSettlement.BankChargesAmountSpecified)
                    bankCharges = invoiceSettlement.BankChargesAmount.Amount.DecValue;
                if (invoiceSettlement.VatBankChargesAmountSpecified)
                    vatBankCharges = invoiceSettlement.VatBankChargesAmount.Amount.DecValue;
                if (invoiceSettlement.FxGainOrLossAmountSpecified)
                    fxGainOrLossAmount = invoiceSettlement.FxGainOrLossAmount.Amount.DecValue;

                decimal netCashAmount = invoiceSettlement.NetCashAmount.Amount.DecValue;
                decimal grossCashAmount = netCashAmount + bankCharges + vatBankCharges;
                decimal settlementAmount = invoiceSettlement.SettlementAmount.Amount.DecValue;

                // EG 20110209 Ticket 17345
                EFS_Cash unallocatedNetAmount = new EFS_Cash(CSTools.SetCacheOn(pCS), grossCashAmount * (unallocatedAmount / settlementAmount), invoiceSettlement.NetCashAmount.Currency);
                decimal totalAvailableamount = grossCashAmount - (fxGainOrLossAmount + unallocatedNetAmount.AmountRounded);

                decimal allocatedAmounts = 0;
                if (invoiceSettlement.AllocatedInvoiceSpecified)
                {
                    foreach (IAllocatedInvoice allocated in invoiceSettlement.AllocatedInvoice)
                    {
                        if (null != allocated.AllocatedAmounts)
                        {
                            if (invoiceSettlement.NetCashAmount.Currency == allocated.AllocatedAmounts.IssueAmount.Currency)
                                allocatedAmounts += allocated.AllocatedAmounts.IssueAmount.Amount.DecValue;
                            else
                                allocatedAmounts += allocated.AllocatedAmounts.AccountingAmount.Amount.DecValue;
                        }
                    }
                }
                if (allocatedAmounts != totalAvailableamount)
                {
                    string[] paramError = new string[3]{
                        invoiceSettlement.NetCashAmount.Currency,
                        StrFunc.FmtDecimalToInvariantCulture(totalAvailableamount),
                        StrFunc.FmtDecimalToInvariantCulture(allocatedAmounts)};
                    SetValidationRuleError("Msg_ValidationRule_AllocationUnbalanced", paramError);
                }
            }
        }
        #endregion CheckInvoiceSettlement

        #region ValidationRules
        // EG 20100208 Add ProductContainer
        public override bool ValidationRules(string pCS, IDbTransaction pDbTransaction, CheckModeEnum pCheckMode)
        {
            m_CheckMode = pCheckMode;
            m_CheckConformity = new Hashtable();
            if (m_Input.Product.IsInvoiceSettlement)
                CheckInvoiceSettlement(pCS);
            return ArrFunc.IsEmpty(m_CheckConformity);
        }
        #endregion ValidationRules
        #endregion Methods
    }

    #endregion CheckTradeAdminValidationRule
}

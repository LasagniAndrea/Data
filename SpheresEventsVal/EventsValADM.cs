#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.GUI.Interface;
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
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process
{
    /// <summary>
    /// Description résumée de EventsValProcessADM.
    /// </summary>
    #region EventsValProcessADM
    public class EventsValProcessADM : EventsValProcessBase
    {
        #region Members
        private bool m_CounterValueAmountIsModified;
        private readonly CommonValParameters m_Parameters;
        #endregion Members
        #region Accessors
        #region Parameters
        public override CommonValParameters Parameters
        {
            get { return m_Parameters; }
        }
        #endregion Parameters
        #endregion Accessors
        #region Constructors
        public EventsValProcessADM(EventsValProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary,IProduct pProduct)
            : base(pProcess, pDsTrade, pTradeLibrary,pProduct)
        {
            m_Parameters = new CommonValParametersADM();
        }
        #endregion Constructors
        #region Methods
        #region GetRowCounterValue
        public DataRow[] GetRowCounterValue()
        {
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTTYPE in ('{1}','{2}','{3}')",
            m_ParamIdT.Value, EventTypeFunc.NetTurnOverAccountingAmount, EventTypeFunc.NetTurnOverIssueAmount, EventTypeFunc.FeeAccountingAmount), "IDE");
        }
        #endregion GetRowCounterValue
        #region GetRowFeeCounterValue
        public DataRow[] GetRowFeeCounterValue()
        {
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTTYPE = '{1}'",
            m_ParamIdT.Value, EventTypeFunc.FeeAccountingAmount), "IDE");
        }
        #endregion GetRowFeeCounterValue
        #region GetRowNTICounterValue
        public DataRow[] GetRowNTICounterValue()
        {
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTTYPE = '{1}'",
            m_ParamIdT.Value, EventTypeFunc.NetTurnOverIssueAmount), "IDE");
        }
        #endregion GetRowNTICounterValue
        #region GetRowTXICounterValue
        public DataRow GetRowTXICounterValue(int pIdE)
        {
            DataRow[] rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDE_EVENT = {0} and EVENTTYPE = '{1}'", pIdE, EventTypeFunc.TaxIssueAmount), "IDE");
            if (ArrFunc.IsFilled(rows))
                return rows[0];
            else
                return null;
        }
        #endregion GetRowTXICounterValue
        #region GetRowNTACounterValue
        public DataRow[] GetRowNTACounterValue()
        {
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDE_EVENT = {0} and EVENTTYPE = '{1}'", m_ParamIdEParent.Value, EventTypeFunc.NetTurnOverAccountingAmount), "IDE");
        }
        #endregion GetRowNTACounterValue
        #region GetRowTXACounterValue
        public DataRow GetRowTXACounterValue(int pIdE)
        {
            DataRow[] rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDE_EVENT = {0} and EVENTTYPE = '{1}'", pIdE, EventTypeFunc.TaxAccountingAmount), "IDE");
            if (ArrFunc.IsFilled(rows))
                return rows[0];
            else
                return null;
        }
        #endregion GetRowTXACounterValue
        #region GetRowInvoicingDatesPeriod
        public DataRow GetRowInvoicingDatesPeriod()
        {
            DataRow[] rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}' and EVENTTYPE = '{2}'",
                m_ParamIdT.Value, EventCodeFunc.InvoicingDates, EventTypeFunc.Period), "IDE");
            if (ArrFunc.IsFilled(rows))
                return rows[0];
            else
                return null;
        }
        #endregion GetRowInvoicingDatesPeriod
        #region GetRowInvoiceMasterPeriod
        public DataRow GetRowInvoiceMasterPeriod()
        {
            DataRow[] rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}' and EVENTTYPE = '{2}'",
                m_ParamIdT.Value, EventCodeFunc.InvoiceMaster, EventTypeFunc.Period), "IDE");
            if (ArrFunc.IsFilled(rows))
                return rows[0];
            else
                return null;
        }
        #endregion GetRowInvoiceMasterPeriod
        #region GetRowInvoiceMasterBasePeriod
        public DataRow GetRowInvoiceMasterBasePeriod()
        {
            DataRow[] rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}' and EVENTTYPE = '{2}'",
                m_ParamIdT.Value, EventCodeFunc.InvoiceMasterBase, EventTypeFunc.Period), "IDE");
            if (ArrFunc.IsFilled(rows))
                return rows[0];
            else
                return null;
        }
        #endregion GetRowInvoiceMasterBasePeriod
        #region GetRowAmendedInvoicePeriod
        public DataRow GetRowAmendedInvoicePeriod()
        {
            DataRow[] rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}' and EVENTTYPE = '{2}'",
                m_ParamIdT.Value, EventCodeFunc.InvoiceAmended, EventTypeFunc.Period), "IDE");
            if (ArrFunc.IsFilled(rows))
                return rows[0];
            else
                return null;
        }
        #endregion GetRowAmendedInvoicePeriod
        #region IsRowMustBeCalculate
        public override bool IsRowMustBeCalculate(DataRow pRow)
        {
            string eventCode = pRow["EVENTCODE"].ToString();
            string eventType = pRow["EVENTTYPE"].ToString();
            if (null != m_EventsValMQueue.quote)
            {
                if (IsQuote_FxRate)
                {
                    if (EventCodeAndEventTypeFunc.IsFxRateReset(eventCode, eventType))
                        return (IsRowHasFixingEvent(pRow));
                    else
                    {
                        DataRow[] drChild = pRow.GetChildRows(this.DsEvents.ChildEvent);
                        if (ArrFunc.IsFilled(drChild))
                        {
                            foreach (DataRow row in drChild)
                            {
                                eventCode = row["EVENTCODE"].ToString();
                                eventType = row["EVENTTYPE"].ToString();
                                if (EventCodeAndEventTypeFunc.IsFxRateReset(eventCode, eventType) && IsRowHasFixingEvent(row))
                                    return true;
                            }
                        }
                    }
                }
            }
            else if (EventTypeFunc.IsFeeAccountingAmount(eventType))
            {
                string currency = pRow["UNIT"].ToString();
                DataRow drParent = pRow.GetParentRow(this.DsEvents.ChildEvent);
                if (null != drParent)
                    return (currency != drParent["UNIT"].ToString());
            }
            else if (EventTypeFunc.IsTaxIssueAmount(eventType) || EventTypeFunc.IsTaxAccountingAmount(eventType))
                return false;
            else
                return true;
            return false;
        }
        #endregion IsRowMustBeCalculate
        #region UpdateTradeXML_CounterValueAmount
        private void UpdateTradeXML_CounterValueAmount(int pIdE, DataRow pRowCounterValue, EFS_ExchangeCurrencyFixingEvent pExchangeCurrencyFixingEvent)
        {
            decimal counterValueAmount = pExchangeCurrencyFixingEvent.exchangeCurrencyCounterValueAmount;
            bool isReverse = pExchangeCurrencyFixingEvent.IsReverse;
            DataRow rowDetail = GetRowDetail(pIdE);
            string eventType = pRowCounterValue["EVENTTYPE"].ToString();
            int idEParent = Convert.ToInt32(pRowCounterValue["IDE_EVENT"]);
            DataRow rowParent = GetRowEvent(idEParent);
            string eventCode = rowParent["EVENTCODE"].ToString();
            
            IInvoice invoice = (IInvoice)m_tradeLibrary.Product;
            decimal rate = 0;
            bool rateSpecified = ((null != rowDetail) && (false == Convert.IsDBNull(rowDetail["RATE"])));
            if (rateSpecified)
                rate = Convert.ToDecimal(rowDetail["RATE"]);
            if (EventTypeFunc.IsFeeAccountingAmount(eventType))
            {
                int idESource = Convert.ToInt32(rowParent["IDE_SOURCE"]);
                IInvoiceFee invoiceFee = (IInvoiceFee)invoice.InvoiceDetails.InvoiceFee(idESource);
                if (null != invoiceFee)
                {
                    m_CounterValueAmountIsModified = (null == invoice.NetTurnOverAccountingRate) || (invoice.NetTurnOverAccountingRate.DecValue != rate);
                    invoiceFee.FeeAccountingAmountSpecified = rateSpecified;
                    invoiceFee.FeeAccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(counterValueAmount, pRowCounterValue["UNIT"].ToString());
                    if (invoiceFee.FeeAmount.Amount.DecValue == invoiceFee.FeeInitialAmount.Amount.DecValue)
                    {
                        invoiceFee.FeeInitialAccountingAmountSpecified = rateSpecified;
                        invoiceFee.FeeInitialAccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(counterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    if (invoiceFee.FeeBaseAmount.Amount.DecValue == invoiceFee.FeeInitialAmount.Amount.DecValue)
                    {
                        // EG 20230222 [WI853][26600] Facturation : Corrections diverses
                        invoiceFee.FeeBaseAccountingAmountSpecified = rateSpecified;
                        if (invoiceFee.FeeInitialAccountingAmountSpecified)
                            invoiceFee.FeeBaseAccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(invoiceFee.FeeInitialAccountingAmount.Amount.DecValue, pRowCounterValue["UNIT"].ToString());
                        else
                            invoiceFee.FeeBaseAccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(0, pRowCounterValue["UNIT"].ToString());
                    }
                    else if ((false == invoiceFee.FeeBaseAccountingAmountSpecified) && rateSpecified)
                    {
                        invoiceFee.FeeBaseAccountingAmountSpecified = rateSpecified;
                        EFS_Cash cash = new EFS_Cash(Process.Cs, invoiceFee.FeeBaseAmount.Amount.DecValue * rate, pRowCounterValue["UNIT"].ToString());
                        invoiceFee.FeeBaseAccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(cash.AmountRounded, pRowCounterValue["UNIT"].ToString());
                    }
                }
            }
            else if (EventTypeFunc.IsNetTurnOverIssueAmount(eventType))
            {
                    invoice.NetTurnOverIssueRateSpecified = rateSpecified;
                    if (rateSpecified)
                    {
                        m_CounterValueAmountIsModified = (null == invoice.NetTurnOverIssueRate) || (invoice.NetTurnOverIssueRate.DecValue != rate);
                        if (EventCodeFunc.IsInvoicingDates(eventCode))
                        {
                            invoice.NetTurnOverIssueRate = new EFS_Decimal(rate);
                            invoice.NetTurnOverIssueAmount = ((IProduct)invoice).ProductBase.CreateMoney(counterValueAmount, pRowCounterValue["UNIT"].ToString());
                            if (isReverse)
                            {
                                invoice.IssueRateIsReverseSpecified = isReverse;
                                invoice.IssueRateIsReverse = new EFS_Boolean(isReverse);
                                invoice.IssueRateReadSpecified = true;
                                invoice.IssueRateRead = new EFS_Decimal(pExchangeCurrencyFixingEvent.RateRead);
                            }
                        }
                        else if (EventCodeFunc.IsInvoiceMaster(eventCode))
                        {
                            IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).InitialInvoiceAmount;
                            invoiceAmounts.NetTurnOverIssueAmount = ((IProduct)invoice).ProductBase.CreateMoney(counterValueAmount, pRowCounterValue["UNIT"].ToString());
                        }
                        else if (EventCodeFunc.IsInvoiceMasterBase(eventCode))
                        {
                            INetInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).BaseNetInvoiceAmount;
                            invoiceAmounts.IssueAmountSpecified = true;
                            invoiceAmounts.IssueAmount = ((IProduct)invoice).ProductBase.CreateMoney(counterValueAmount, pRowCounterValue["UNIT"].ToString());
                        }
                        else if (EventCodeFunc.IsInvoiceAmended(eventCode))
                        {
                            IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).TheoricInvoiceAmount;
                            invoiceAmounts.NetTurnOverIssueAmount = ((IProduct)invoice).ProductBase.CreateMoney(counterValueAmount, pRowCounterValue["UNIT"].ToString());
                        }
                    }
            }
            else if (EventTypeFunc.IsNetTurnOverAccountingAmount(eventType))
            {
                invoice.NetTurnOverAccountingRateSpecified = rateSpecified;
                if (rateSpecified)
                {
                    m_CounterValueAmountIsModified = (null == invoice.NetTurnOverAccountingRate) || (invoice.NetTurnOverAccountingRate.DecValue != rate);
                    if (EventCodeFunc.IsInvoicingDates(eventCode))
                    {
                        invoice.NetTurnOverAccountingRate = new EFS_Decimal(rate);
                        invoice.NetTurnOverAccountingAmountSpecified = true;
                        invoice.NetTurnOverAccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(counterValueAmount, pRowCounterValue["UNIT"].ToString());
                        if (isReverse)
                        {
                            invoice.AccountingRateIsReverseSpecified = isReverse;
                            invoice.AccountingRateIsReverse = new EFS_Boolean(isReverse);
                            invoice.AccountingRateReadSpecified = true;
                            invoice.AccountingRateRead = new EFS_Decimal(pExchangeCurrencyFixingEvent.RateRead);
                        }

                    }
                    else if (EventCodeFunc.IsInvoiceMaster(eventCode))
                    {
                        IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).InitialInvoiceAmount;
                        invoiceAmounts.NetTurnOverAccountingAmountSpecified = true;
                        invoiceAmounts.NetTurnOverAccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(counterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceMasterBase(eventCode))
                    {
                        INetInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).BaseNetInvoiceAmount;
                        invoiceAmounts.AccountingAmountSpecified = true; ;
                        invoiceAmounts.AccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(counterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceAmended(eventCode))
                    {
                        IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).TheoricInvoiceAmount;
                        invoiceAmounts.NetTurnOverAccountingAmountSpecified = true;
                        invoiceAmounts.NetTurnOverAccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(counterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                }
            }
        }
        #endregion UpdateTradeXML_CounterValueAmount
        #region UpdateTradeXML_CounterValueTaxAmount
        private void UpdateTradeXML_CounterValueTaxAmount(DataRow pRowNetTurnOver, DataRow pRowCounterValue, decimal pCounterValueAmount)
        {
            int idE = Convert.ToInt32(pRowNetTurnOver["IDE"]);
            DataRow rowDetail = GetRowDetail(idE);
            bool rateSpecified = (false == Convert.IsDBNull(rowDetail["RATE"]));
            int idEParent = Convert.ToInt32(pRowNetTurnOver["IDE_EVENT"]);
            DataRow rowParent = GetRowEvent(idEParent);
            string eventCode = rowParent["EVENTCODE"].ToString();
            string eventType = pRowCounterValue["EVENTTYPE"].ToString();
            IInvoice invoice = (IInvoice)m_tradeLibrary.Product;

            if (EventTypeFunc.IsTaxIssueAmount(eventType))
            {
                invoice.Tax.IssueAmountSpecified = true;
                if (rateSpecified)
                {
                    if (EventCodeFunc.IsInvoicingDates(eventCode))
                    {
                        invoice.Tax.IssueAmount.Amount = new EFS_Decimal(pCounterValueAmount);
                    }
                    else if (EventCodeFunc.IsInvoiceMaster(eventCode))
                    {
                        IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).InitialInvoiceAmount;
                        invoiceAmounts.Tax.IssueAmountSpecified = true;
                        invoiceAmounts.Tax.IssueAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceMasterBase(eventCode))
                    {
                        INetInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).BaseNetInvoiceAmount;
                        invoiceAmounts.Tax.IssueAmountSpecified = true;
                        invoiceAmounts.Tax.IssueAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceAmended(eventCode))
                    {
                        IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).TheoricInvoiceAmount;
                        invoiceAmounts.Tax.IssueAmountSpecified = true;
                        invoiceAmounts.Tax.IssueAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                }
            }
            else if (EventTypeFunc.IsTaxAccountingAmount(eventType))
            {
                invoice.Tax.AccountingAmountSpecified = rateSpecified;
                if (rateSpecified)
                {
                    if (EventCodeFunc.IsInvoicingDates(eventCode))
                    {
                        invoice.Tax.AccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceMaster(eventCode))
                    {
                        IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).InitialInvoiceAmount;
                        invoiceAmounts.Tax.AccountingAmountSpecified = true;
                        invoiceAmounts.Tax.AccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceMasterBase(eventCode))
                    {
                        INetInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).BaseNetInvoiceAmount;
                        invoiceAmounts.Tax.AccountingAmountSpecified = true;
                        invoiceAmounts.Tax.AccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceAmended(eventCode))
                    {
                        IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).TheoricInvoiceAmount;
                        invoiceAmounts.Tax.AccountingAmountSpecified = true;
                        invoiceAmounts.Tax.AccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                }
            }
        }
        #endregion UpdateTradeXML_CounterValueTaxAmount
        #region UpdateTradeXML_CounterValueTaxDetAmount
        private void UpdateTradeXML_CounterValueTaxDetAmount(int pPosDetTax,DataRow pRowNetTurnOver, DataRow pRowCounterValue, decimal pCounterValueAmount)
        {
            int idE = Convert.ToInt32(pRowNetTurnOver["IDE"]);
            DataRow rowDetail = GetRowDetail(idE);
            bool rateSpecified = (false == Convert.IsDBNull(rowDetail["RATE"]));
            int idEGrandParent = Convert.ToInt32(pRowNetTurnOver["IDE_EVENT"]);
            DataRow rowGrandParent = GetRowEvent(idEGrandParent);
            string eventCode = rowGrandParent["EVENTCODE"].ToString();
            int idEParent = Convert.ToInt32(pRowCounterValue["IDE_EVENT"]);
            DataRow rowParent = GetRowEvent(idEParent);
            string eventType = rowParent["EVENTTYPE"].ToString();
            IInvoice invoice = (IInvoice)m_tradeLibrary.Product;


            if (EventTypeFunc.IsTaxIssueAmount(eventType))
            {
                invoice.Tax.Details[pPosDetTax].TaxAmount.IssueAmountSpecified = true;
                if (rateSpecified)
                {
                    if (EventCodeFunc.IsInvoicingDates(eventCode))
                    {
                        invoice.Tax.Details[pPosDetTax].TaxAmount.IssueAmount.Amount = new EFS_Decimal(pCounterValueAmount);
                    }
                    else if (EventCodeFunc.IsInvoiceMaster(eventCode))
                    {
                        IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).InitialInvoiceAmount;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.IssueAmountSpecified = true;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.IssueAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceMasterBase(eventCode))
                    {
                        INetInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).BaseNetInvoiceAmount;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.IssueAmountSpecified = true;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.IssueAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceAmended(eventCode))
                    {
                        IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).TheoricInvoiceAmount;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.AccountingAmountSpecified = true;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.IssueAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                }
            }
            else if (EventTypeFunc.IsTaxAccountingAmount(eventType))
            {
                invoice.Tax.Details[pPosDetTax].TaxAmount.AccountingAmountSpecified = rateSpecified;
                if (rateSpecified)
                {
                    if (EventCodeFunc.IsInvoicingDates(eventCode))
                    {
                        invoice.Tax.Details[pPosDetTax].TaxAmount.AccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceMaster(eventCode))
                    {
                        IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).InitialInvoiceAmount;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.AccountingAmountSpecified = true;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.AccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceMasterBase(eventCode))
                    {
                        INetInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).BaseNetInvoiceAmount;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.AccountingAmountSpecified = true;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.AccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                    else if (EventCodeFunc.IsInvoiceAmended(eventCode))
                    {
                        IInvoiceAmounts invoiceAmounts = ((IInvoiceSupplement)invoice).TheoricInvoiceAmount;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.AccountingAmountSpecified = true;
                        invoiceAmounts.Tax.Details[pPosDetTax].TaxAmount.AccountingAmount = ((IProduct)invoice).ProductBase.CreateMoney(pCounterValueAmount, pRowCounterValue["UNIT"].ToString());
                    }
                }
            }
        }
        #endregion UpdateTradeXML_CounterValueTaxDetAmount

        #region SetCounterValue
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20220519 [WI637] Les datarows sont mise à jour (sans update, il sera exécuté ensuite par lot)
        private Cst.ErrLevel SetCounterValue(DataRow[] pRowsCounterValue)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            ArrayList alSpheresException = new ArrayList();
            bool isLogToDisplay = true;

            #region Amount with fixing Process
            foreach (DataRow rowCounterValue in pRowsCounterValue)
            {
                m_ParamInstrumentNo.Value = Convert.ToInt32(rowCounterValue["INSTRUMENTNO"]);
                m_ParamStreamNo.Value = Convert.ToInt32(rowCounterValue["STREAMNO"]);
                //
                int idE = Convert.ToInt32(rowCounterValue["IDE"]);
                bool isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_EventsValProcess.ScanCompatibility_Event(idE));
                isRowMustBeCalculate = isRowMustBeCalculate && IsRowMustBeCalculate(rowCounterValue);
                //
                if (isRowMustBeCalculate)
                {
                    // Calcul des contrevaleurs
                    if (isLogToDisplay)
                    {

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 602), 4,
                            new LogParam(LogTools.IdentifierAndId(m_EventsValProcess.MQueue.Identifier, m_EventsValProcess.MQueue.id)),
                            new LogParam(rowCounterValue["EVENTTYPE"])));

                        isLogToDisplay = false;
                    }

                    EFS_ExchangeCurrencyFixingEvent exchangeCurrencyFixingEvent = null;
                    try
                    {
                        Parameters.Add(m_CS, m_tradeLibrary, rowCounterValue);
                        exchangeCurrencyFixingEvent = new EFS_ExchangeCurrencyFixingEvent(this, rowCounterValue);
                        SetTAXCounterValue(rowCounterValue, exchangeCurrencyFixingEvent);
                        UpdateTradeXML_CounterValueAmount(idE, rowCounterValue, exchangeCurrencyFixingEvent);
                    }
                    catch (SpheresException2 ex)
                    {
                        if (ex.IsStatusError)
                            alSpheresException.Add(ex);
                    }
                    catch (Exception ex)
                    {
                        alSpheresException.Add(new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex));
                    }
                }
            }
            #endregion Amount with fixing Process
            //
            if (ArrFunc.IsFilled(alSpheresException))
            {
                ret = Cst.ErrLevel.ABORTED;
                
                foreach (SpheresException2 ex in alSpheresException)
                {
                    Logger.Log(new LoggerData(ex));
                }
            }
            return ret;
        }
        #endregion SetCounterValue
        #region SetTAXCounterValue
        private Cst.ErrLevel SetTAXCounterValue(DataRow pRowCounterValue,EFS_ExchangeCurrencyFixingEvent pFixingEvent)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (pFixingEvent.IsExistReset)
            {
                string eventType = pRowCounterValue["EVENTTYPE"].ToString();
                int idE = Convert.ToInt32(pRowCounterValue["IDE"]);
                DataRow rowTAX = null;
                if (EventTypeFunc.IsNetTurnOverIssueAmount(eventType))
                    rowTAX = GetRowTXICounterValue(idE);
                else if (EventTypeFunc.IsNetTurnOverAccountingAmount(eventType))
                    rowTAX = GetRowTXACounterValue(idE);

                if (null != rowTAX)
                {
                    #region Total Tax Countervalue
                    int idETax = Convert.ToInt32(rowTAX["IDE"]);
                    DataRow rowDetailTAX = GetRowDetail(idETax);
                    if (null != rowDetailTAX)
                    {
                        #region Detail Tax Countervalue
                        DataRow[] rowTAXDET = rowTAX.GetChildRows(DsEvents.ChildEvent);
                        if (null != rowTAXDET)
                        {
                            int i = 0;
                            decimal totalTaxAmount = 0;
                            decimal taxAmount = RoundingCurrencyAmount(pFixingEvent.ReferenceCurrency, Convert.ToDecimal(rowDetailTAX["NOTIONALAMOUNT"]) * pFixingEvent.Rate);
                            decimal grossTurnovertotalTaxAmount = Convert.ToDecimal(pRowCounterValue["VALORISATION"]) - taxAmount;
                            foreach (DataRow row in rowTAXDET)
                            {
                                int idETaxDet = Convert.ToInt32(row["IDE"]);
                                DataRow rowFee = GetRowFee(idETaxDet);
                                decimal taxRate = Convert.ToDecimal(rowFee["TAXRATE"]);
                                decimal taxDetAmount = RoundingCurrencyAmount(pFixingEvent.ReferenceCurrency, grossTurnovertotalTaxAmount * taxRate);
                                row["VALORISATION"] = taxDetAmount;
                                row["UNIT"] = pFixingEvent.ReferenceCurrency;
                                row["VALORISATIONSYS"] = taxDetAmount;
                                row["UNIT"] = pFixingEvent.ReferenceCurrency;
                                Update(idETaxDet, false);
                                UpdateTradeXML_CounterValueTaxDetAmount(i, pRowCounterValue, row, taxDetAmount);
                                i++;
                                totalTaxAmount += taxDetAmount;
                            }
                            rowTAX["VALORISATION"] = totalTaxAmount;
                            rowTAX["UNIT"] = pFixingEvent.ReferenceCurrency;
                            rowTAX["VALORISATIONSYS"] = totalTaxAmount;
                            rowTAX["UNITSYS"] = pFixingEvent.ReferenceCurrency;
                            if (null != rowDetailTAX)
                                rowDetailTAX["RATE"] = pFixingEvent.Rate;
                            CommonValFunc.SetRowCalculated(rowTAX);
                            Update(idETax, false);
                            UpdateTradeXML_CounterValueTaxAmount(pRowCounterValue, rowTAX, totalTaxAmount);
                        }
                        #endregion Detail  Tax Countervalue
                    }
                    #endregion Total Tax Countervalue
                }
            }
            return ret;
        }
        #endregion SetTAXCounterValue
        #region Update
        // EG 20180502 Analyse du code Correction [CA2200]
        // EG 20200914 [XXXXX] Correction DtTrade => DtTradeXML
        protected override void Update(int pIdE, bool pIsError)
        {
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                dbTransaction = DataHelper.BeginTran(m_CS);
                //
                if (m_CounterValueAmountIsModified)
                {
                    EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(m_tradeLibrary.DataDocument.DataDocument);
                    StringBuilder sb = CacheSerializer.Serialize(serializerInfo);
                    DsTrade.DtTradeXML.Rows[0]["TRADEXML"] = sb.ToString();
                    DsTrade.UpdateTradeXML(dbTransaction);
                }
                DsEvents.Update(dbTransaction);
                //UpdateEventAsset(dbTransaction);
                //UpdateEventDet(dbTransaction);
                //UpdateEventFee(dbTransaction);

                ProcessStateTools.StatusEnum statusEnum = pIsError ? ProcessStateTools.StatusErrorEnum : ProcessStateTools.StatusSuccessEnum;
                
                EventProcess eventProcess = new EventProcess(this.m_CS); 
                eventProcess.Write(dbTransaction, pIdE, m_Process.MQueue.ProcessType, statusEnum, OTCmlHelper.GetDateSysUTC(m_CS), Process.Tracker.IdTRK_L);
                
                DataHelper.CommitTran(dbTransaction);
            }
            catch (SpheresException2)
            {
                isException = true;
                throw;
            }
            catch (Exception ex)
            {
                isException = true;
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
            }
            finally
            {
                if ((isException) && (null != dbTransaction))
                {
                    try
                    {
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    catch { }
                }
            }
        }
        /// <summary>
        /// Mise àjour des datarows et du TRADEXML
        /// pour les événements de facturation avec devise comptable <> devise de frais
        /// Les datarows sont mise à jour par lot
        /// </summary>
        /// <param name="pUpdateTradeXML"></param>
        // EG 20220519 [WI637] New 
        protected void UpdateEventsAndTradeXML(bool pUpdateTradeXML)
        {
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                dbTransaction = DataHelper.BeginTran(m_CS);
                if (m_CounterValueAmountIsModified && pUpdateTradeXML) 
                {
                    EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(m_tradeLibrary.DataDocument.DataDocument);
                    StringBuilder sb = CacheSerializer.Serialize(serializerInfo);
                    DsTrade.DtTradeXML.Rows[0]["TRADEXML"] = sb.ToString();
                    DsTrade.UpdateTradeXML(dbTransaction);
                }
                DsEvents.Update(dbTransaction);
                DataHelper.CommitTran(dbTransaction);
            }
            catch (SpheresException2)
            {
                isException = true;
                throw;
            }
            catch (Exception ex)
            {
                isException = true;
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
            }
            finally
            {
                if ((isException) && (null != dbTransaction))
                {
                    try {DataHelper.RollbackTran(dbTransaction);}
                    catch { }
                }
            }
        }
        #endregion Update
        #region Valorize
        // EG 20180502 Analyse du code Correction [CA2214]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20220519 [WI637] Refactoring Mise à jour des événements par lot
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel retMaster = Cst.ErrLevel.SUCCESS;
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            // Calcul des contrevaleurs
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 601), 3,
                new LogParam(LogTools.IdentifierAndId(m_EventsValProcess.MQueue.Identifier, m_EventsValProcess.MQueue.id))));

            // All FAA
            DataRow[] rowsCounterValue = GetRowFeeCounterValue();

            List<List<DataRow>> subRows = ListExtensionsTools.ChunkBy(rowsCounterValue.ToList(), 200);
            subRows.ForEach(lstRows =>
            {
                ret = SetCounterValue(lstRows.ToArray());
                if (Cst.ErrLevel.SUCCESS != ret)
                    retMaster = ret;
                else
                    UpdateEventsAndTradeXML(false);
            });

            // All NTI
            rowsCounterValue = GetRowNTICounterValue();
            // NTA Master Invoice
            DataRow rowPeriod = GetRowInvoiceMasterPeriod();
            if (null != rowPeriod)
            {
                m_ParamIdEParent.Value = Convert.ToInt32(rowPeriod["IDE"]);
                rowsCounterValue = rowsCounterValue.Union(GetRowNTACounterValue()).ToArray();
            }
            // NTA Master base Invoice
            rowPeriod = GetRowInvoiceMasterBasePeriod();
            if (null != rowPeriod)
            {
                m_ParamIdEParent.Value = Convert.ToInt32(rowPeriod["IDE"]);
                rowsCounterValue = rowsCounterValue.Union(GetRowNTACounterValue()).ToArray();
            }
            // NTA Amended Invoice
            rowPeriod = GetRowAmendedInvoicePeriod();
            if (null != rowPeriod)
            {
                m_ParamIdEParent.Value = Convert.ToInt32(rowPeriod["IDE"]);
                rowsCounterValue = rowsCounterValue.Union(GetRowNTACounterValue()).ToArray();
            }
            // NTA Invoice
            rowPeriod = GetRowInvoicingDatesPeriod();
            if (null != rowPeriod)
            {
                m_ParamIdEParent.Value = Convert.ToInt32(rowPeriod["IDE"]);
                DataRow[] rowsNTA= GetRowNTACounterValue();

                if (m_tradeLibrary.Product.ProductBase.IsAdditionalInvoice || m_tradeLibrary.Product.ProductBase.IsCreditNote)
                {
                    IInvoice invoice = (IInvoice)m_tradeLibrary.Product;
                    INetInvoiceAmounts baseAmounts = ((IInvoiceSupplement)invoice).BaseNetInvoiceAmount;
                    IInvoiceAmounts theoricAmounts = ((IInvoiceSupplement)invoice).TheoricInvoiceAmount;
                    if (invoice.NetTurnOverAccountingAmountSpecified)
                    {
                        decimal NTA_Amount = invoice.NetTurnOverAccountingAmount.Amount.DecValue;
                        decimal NTA_MasterBaseAmount = baseAmounts.AccountingAmount.Amount.DecValue;
                        decimal NTA_AmendedAmount = theoricAmounts.NetTurnOverAccountingAmount.Amount.DecValue;
                        decimal differential = Math.Abs(NTA_MasterBaseAmount - NTA_AmendedAmount);
                        if (NTA_Amount != differential)
                        {
                            invoice.NetTurnOverAccountingAmount.Amount.DecValue = differential;
                            rowsNTA[0].BeginEdit();
                            rowsNTA[0]["VALORISATION"] = differential;
                            rowsNTA[0].EndEdit();
                        }
                    }
                }
                rowsCounterValue = rowsCounterValue.Union(rowsNTA).ToArray();
            }

            ret = SetCounterValue(rowsCounterValue);
            if (Cst.ErrLevel.SUCCESS != ret)
                retMaster = ret;
            else
                UpdateEventsAndTradeXML(true);

            return retMaster;
        }
        #endregion Valorize
        #endregion Methods
    }
    #endregion EventsValProcessADM
}

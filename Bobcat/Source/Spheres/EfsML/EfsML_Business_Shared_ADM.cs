#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Reflection;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_AllocatedInvoice
    public class EFS_AllocatedInvoice
    {
        #region Members
        protected string m_Cs;
        protected Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public int invoiceIdT;
        public string invoiceIdentifier;
        public int invoiceIdE;
        public int invoiceSettlementIdT;
        public string invoiceSettlementIdentifier;
        public int invoiceSettlementIdE;
        public IAdjustedDate invoiceDate;
        public IAdjustedDate receptionDate;
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public IMoney fxGainOrLossAmount;
        public bool fxGainOrLossAmountSpecified;
        public IMoney amount;
        public IMoney issueAmount;
        public IMoney accountingAmount;
        #endregion Members
        #region Accessors
        #region AccountingAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal AccountingAmount
        {
            get { return this.accountingAmount.Amount;}
        }
        #endregion AccountingAmount
        #region AccountingCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string AccountingCurrency
        {
            get { return this.accountingAmount.Currency;}
        }
        #endregion AccountingCurrency
        #region AdjustedInvoiceDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedInvoiceDate
        {
            get { return new EFS_Date(invoiceDate.Value); }
        }
        #endregion AdjustedInvoiceDate
        #region AdjustedReceptionDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedReceptionDate
        {
            get { return new EFS_Date(receptionDate.Value); }
        }
        #endregion AdjustedReceptionDate
        #region InvoiceDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate InvoiceDate
        {
            get {return new EFS_EventDate(invoiceDate.DateValue, invoiceDate.DateValue);}
        }
        #endregion ReceptionDate
        #region InvoiceIdE
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public int InvoiceIdE
        {
            get {return invoiceIdE;}
        }
        #endregion InvoiceIdE
        #region InvoiceIdentifier
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string InvoiceIdentifier
        {
            get {return invoiceIdentifier;}
        }
        #endregion InvoiceIdentifier
        #region InvoiceIdT
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public int InvoiceIdT
        {
            get {return invoiceIdT;}
        }
        #endregion InvoiceIdT
        #region InvoiceSettlementIdE
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public int InvoiceSettlementIdE
        {
            get {return invoiceSettlementIdE; }
        }
        #endregion InvoiceSettlementIdE
        #region InvoiceSettlementIdentifier
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string InvoiceSettlementIdentifier
        {
            get {return invoiceSettlementIdentifier; }
        }
        #endregion InvoiceSettlementIdentifier
        #region InvoiceSettlementIdT
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public int InvoiceSettlementIdT
        {
            get {return invoiceSettlementIdT; }
        }
        #endregion InvoiceSettlementIdT
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PayerPartyReference
        {
            get {return payerPartyReference.HRef; }
        }
        #endregion PayerPartyReference
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region FxGainOrLossType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string FxGainOrLossType
        {
            get
            {
                string eventType = string.Empty;
                if (fxGainOrLossAmountSpecified)
                {
                    if (0 < fxGainOrLossAmount.Amount.DecValue)
                        eventType = EventTypeFunc.ForeignExchangeProfit;
                    else
                        eventType = EventTypeFunc.ForeignExchangeLoss;
                }
                return eventType;
            }
        }
        #endregion FxGainOrLossType
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string ReceiverPartyReference
        {
            get {return receiverPartyReference.HRef; }
        }
        #endregion ReceiverPartyReference
        #region ReceptionDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate ReceptionDate
        {
            get {return new EFS_EventDate(receptionDate.DateValue, receptionDate.DateValue); }
        }
        #endregion ReceptionDate
        #endregion Accessors
        #region Constructors
        public EFS_AllocatedInvoice(string pConnectionString, IAdjustedDate pReceptionDate,
            IReference pPayerPartyReference, IReference pReceiverPartyReference, IAllocatedInvoice pAllocatedInvoice)
        {
            m_Cs = pConnectionString;
            #region Reception Date
            receptionDate = pReceptionDate;
            #endregion Reception Date
            #region Payer/Receiver
            payerPartyReference = pPayerPartyReference;
            receiverPartyReference = pReceiverPartyReference;
            #endregion Payer/Receiver
            m_ErrLevel = Calc(pAllocatedInvoice);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        protected Cst.ErrLevel Calc(IAllocatedInvoice pAllocatedInvoice)
        {
            #region Invoice Date
            invoiceDate = pAllocatedInvoice.InvoiceDate;
            #endregion Invoice Date
            #region invoice_IdT
            invoiceIdT = pAllocatedInvoice.OTCmlId;
            #endregion invoice_IdT
            #region invoice_Identifier
            invoiceIdentifier = pAllocatedInvoice.Identifier.Value;
            #endregion invoice_Identifier
            #region FxGainOrLossAmount
            fxGainOrLossAmountSpecified = pAllocatedInvoice.FxGainOrLossAmountSpecified;
            if (fxGainOrLossAmountSpecified)
                fxGainOrLossAmount = pAllocatedInvoice.FxGainOrLossAmount;
            #endregion FxGainOrLossAmount
            #region AllocatedAmount
            amount = pAllocatedInvoice.AllocatedAmounts.Amount;
            issueAmount = pAllocatedInvoice.AllocatedAmounts.IssueAmount;
            accountingAmount = pAllocatedInvoice.AllocatedAmounts.AccountingAmount;
            #endregion AllocatedAmount
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_AllocatedInvoice

    #region EFS_BaseInvoice
    public class EFS_BaseInvoice : EFS_CorrectedInvoice
    {
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BaseInvoice(string pConnectionString, INetInvoiceAmounts pNetInvoiceAmounts, EFS_Invoice pEFS_Invoice, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pNetInvoiceAmounts, pEFS_Invoice, pDataDocument)
        {
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
        }
        #endregion Constructors
    }
    #endregion EFS_BaseInvoice

    #region EFS_CorrectedInvoice
    // EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Add partyReference pour GrossTurnOver
    public class EFS_CorrectedInvoice : EFS_InvoiceBase
    {
        #region Members
        public bool grossTurnOverAmountSpecified;
        public IMoney grossTurnOverAmount;
        public bool rebateAmountSpecified;
        public IMoney rebateAmount;
        public IReference grossTurnOverAmountPayerPartyReference;
        public IReference grossTurnOverAmountReceiverPartyReference;

        public bool taxSpecified; 
        public EFS_InvoiceTax tax;
        
        public IMoney netTurnOverAmount;
        public EFS_InvoiceTurnOver netTurnOverIssueAmount;
        public EFS_InvoiceTurnOver netTurnOverAccountingAmount;
        #endregion Members
        #region Accessors
        // EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Add partyReference pour GrossTurnOver
        // EG 20240210 [WI640] Correctifs
        public string GrossTurnOverPayer
        {
            get { return payerPartyReference.HRef; }
        }
        // EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Add partyReference pour GrossTurnOver
        // EG 20240210 [WI640] Correctifs
        public string GrossTurnOverReceiver
        {
            get { return receiverPartyReference.HRef; }
        }
        #region GrossTurnOverAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal GrossTurnOverAmount
        {
            get {return new EFS_Decimal(grossTurnOverAmount.Amount.DecValue); }
        }
        #endregion GrossTurnOverAmount
        #region GrossTurnOverCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string GrossTurnOverCurrency
        {
            get {return grossTurnOverAmount.Currency; }
        }
        #endregion GrossTurnOverCurrency
        #region NetTurnOverAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal NetTurnOverAmount
        {
            get {return new EFS_Decimal(netTurnOverAmount.Amount.DecValue); }
        }
        #endregion NetTurnOverAmount
        #region NetTurnOverCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string NetTurnOverCurrency
        {
            get {return netTurnOverAmount.Currency; }
        }
        #endregion NetTurnOverCurrency
        #region RebateAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal RebateAmount
        {
            get
            {
                EFS_Decimal amount = new EFS_Decimal(0);
                if (rebateAmountSpecified)
                    amount.DecValue = rebateAmount.Amount.DecValue;
                return amount;
            }
        }
        #endregion RebateAmount
        #region RebateCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string RebateCurrency
        {
            get
            {
                string currency = netTurnOverAmount.Currency;
                if (rebateAmountSpecified)
                    currency = rebateAmount.Currency;
                return currency;
            }
        }
        #endregion RebateCurrency


        #region TaxAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_Decimal TaxAmount
        {
            get
            {
                if (taxSpecified)
                    return tax.amount.Amount;
                else
                    return null;
            }
        }
        #endregion TaxAmount
        #region TaxCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual string TaxCurrency
        {
            get
            {
                if (taxSpecified)
                    return tax.amount.Currency;
                else
                    return null;
            }
        }
        #endregion TaxCurrency
        #region TaxAccountingBase
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_InvoicingAmountBase TaxAccountingBase
        {
            get
            {
                if (taxSpecified)
                    return netTurnOverAccountingAmount.taxAmount.BaseAmount;
                else
                    return null;
            }
        }
        #endregion TaxAccountingBase
        #region TaxAccountingAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_Decimal TaxAccountingAmount
        {
            get
            {
                if (taxSpecified)
                    return netTurnOverAccountingAmount.taxAmount.amount.Amount;
                else
                    return null;
            }
        }
        #endregion TaxAccountingAmount
        #region TaxAccountingCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual string TaxAccountingCurrency
        {
            get
            {
                if (taxSpecified)
                    return netTurnOverAccountingAmount.taxAmount.amount.Currency;
                else
                    return null;
            }
        }
        #endregion TaxAccountingCurrency
        #region TaxIssueBase
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_InvoicingAmountBase TaxIssueBase
        {
            get
            {
                if (taxSpecified)
                    return netTurnOverIssueAmount.taxAmount.BaseAmount;
                else
                    return null;
            }
        }
        #endregion TaxIssueBase
        #region TaxIssueAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_Decimal TaxIssueAmount
        {
            get
            {
                if (taxSpecified)
                    return netTurnOverIssueAmount.taxAmount.amount.Amount;
                else
                    return null;
            }
        }
        #endregion TaxIssueAmount
        #region TaxIssueCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual string TaxIssueCurrency
        {
            get
            {
                if (taxSpecified)
                    return netTurnOverIssueAmount.taxAmount.amount.Currency;
                else
                    return null;
            }
        }
        #endregion TaxIssueCurrency

        #region TurnOverAccountingAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_Decimal TurnOverAccountingAmount
        {
            get {return netTurnOverAccountingAmount.TurnOverAmount; }
        }
        #endregion TurnOverAccountingAmount
        #region TurnOverAccountingCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual string TurnOverAccountingCurrency
        {
            get {return netTurnOverAccountingAmount.TurnOverCurrency; }
        }
        #endregion TurnOverAccountingCurrency
        #region TurnOverAccountingBase
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_InvoicingAmountBase TurnOverAccountingBase
        {
            get {return netTurnOverAccountingAmount.TurnOverBase; }
        }
        #endregion TurnOverAccountingBase
        #region TurnOverAccountingRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_Decimal TurnOverAccountingRate
        {
            get
            {
                if (netTurnOverAccountingAmount.turnOverFixingRateSpecified)
                    return netTurnOverAccountingAmount.turnOverFixingRate;
                else
                    return new EFS_Decimal(0);
            }
        }
        #endregion TurnOverAccountingRate
        #region TurnOverIssueAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_Decimal TurnOverIssueAmount
        {
            get {return netTurnOverIssueAmount.TurnOverAmount; }
        }
        #endregion TurnOverIssueAmount
        #region TurnOverIssueCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual string TurnOverIssueCurrency
        {
            get {return netTurnOverIssueAmount.TurnOverCurrency; }
        }
        #endregion TurnOverIssueCurrency
        #region TurnOverIssueBase
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_InvoicingAmountBase TurnOverIssueBase
        {
            get {return netTurnOverIssueAmount.TurnOverBase; }
        }
        #endregion TurnOverIssueBase
        #region TurnOverIssueRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_Decimal TurnOverIssueRate
        {
            get
            {
                if (netTurnOverIssueAmount.turnOverFixingRateSpecified)
                    return netTurnOverIssueAmount.turnOverFixingRate;
                else
                    return new EFS_Decimal(0);
            }
        }
        #endregion TurnOverIssueRate

        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CorrectedInvoice(string pConnectionString, DataDocumentContainer pDataDocument) : base(pConnectionString, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CorrectedInvoice(string pConnectionString, INetInvoiceAmounts pNetInvoiceAmounts, EFS_Invoice pEFS_Invoice, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pEFS_Invoice, pDataDocument)
        {
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
            #region NetTurnOverAmount
            netTurnOverAmount = pNetInvoiceAmounts.Amount;
            #endregion NetTurnOverAmount
            #region NetTurnOverIssueAmount
            netTurnOverIssueAmount = (EFS_InvoiceTurnOver)pEFS_Invoice.netTurnOverIssueAmount.Clone();
            netTurnOverIssueAmount.turnOverBaseAmount.Amount.DecValue = netTurnOverAmount.Amount.DecValue;
            netTurnOverIssueAmount.turnOverAmount.Amount.DecValue = pNetInvoiceAmounts.IssueAmount.Amount.DecValue;
            #endregion NetTurnOverIssueAmount
            #region NetTurnOverAccountingAmount
            netTurnOverAccountingAmount = (EFS_InvoiceTurnOver)pEFS_Invoice.netTurnOverAccountingAmount.Clone();
            netTurnOverAccountingAmount.turnOverBaseAmount.Amount.DecValue = netTurnOverAmount.Amount.DecValue;
            if (netTurnOverAccountingAmount.TurnOverCurrency == netTurnOverAmount.Currency)
                netTurnOverAccountingAmount.turnOverAmount.Amount.DecValue = netTurnOverAmount.Amount.DecValue;
            else
                netTurnOverAccountingAmount.turnOverAmount.Amount.DecValue = 0;
            #endregion NetTurnOverAccountingAmount
            #region Tax
            taxSpecified = pNetInvoiceAmounts.TaxSpecified;
            if (taxSpecified)
            {
                #region TaxAmount
                tax = new EFS_InvoiceTax(pConnectionString, pNetInvoiceAmounts, EventTypeFunc.NetTurnOverAmount, 1);
                #endregion TaxAmount
                #region TaxIssueAmount
                netTurnOverIssueAmount.taxAmountSpecified = taxSpecified;
                netTurnOverIssueAmount.taxAmount.baseAmount.Amount.DecValue = pNetInvoiceAmounts.Tax.Amount.Amount.DecValue;
                netTurnOverIssueAmount.taxAmount.amount.Amount.DecValue = pNetInvoiceAmounts.Tax.IssueAmount.Amount.DecValue;
                SetTaxDetail(netTurnOverIssueAmount, pNetInvoiceAmounts.Tax);
                #endregion TaxIssueAmount
                #region TaxAccountingAmount
                netTurnOverAccountingAmount.taxAmountSpecified = taxSpecified;
                netTurnOverAccountingAmount.taxAmount.baseAmount.Amount.DecValue = pNetInvoiceAmounts.Tax.Amount.Amount.DecValue;
                netTurnOverAccountingAmount.taxAmount.amount.Amount.DecValue = pNetInvoiceAmounts.Tax.AccountingAmount.Amount.DecValue;
                SetTaxDetail(netTurnOverAccountingAmount, pNetInvoiceAmounts.Tax);
                #endregion TaxAccountingAmount
            }
            #endregion Tax
        }

        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CorrectedInvoice(string pConnectionString, IInvoiceAmounts pInvoiceAmounts, EFS_Invoice pEFS_Invoice, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pEFS_Invoice, pDataDocument)
        {
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
            #region GrossTurnOverAmount
            grossTurnOverAmountSpecified = true;
            grossTurnOverAmount = pInvoiceAmounts.GrossTurnOverAmount;
            #endregion GrossTurnOverAmount
            #region RebateAmount
            rebateAmountSpecified = pInvoiceAmounts.RebateAmountSpecified;
            if (rebateAmountSpecified)
                rebateAmount = pInvoiceAmounts.RebateAmount;
            #endregion RebateAmount
            #region NetTurnOverAmount
            netTurnOverAmount = pInvoiceAmounts.NetTurnOverAmount;
            #endregion NetTurnOverAmount
            #region NetTurnOverIssueAmount
            netTurnOverIssueAmount = (EFS_InvoiceTurnOver)pEFS_Invoice.netTurnOverIssueAmount.Clone();
            netTurnOverIssueAmount.turnOverBaseAmount.Amount.DecValue = netTurnOverAmount.Amount.DecValue;
            netTurnOverIssueAmount.turnOverAmount.Amount.DecValue = pInvoiceAmounts.NetTurnOverIssueAmount.Amount.DecValue;
            #endregion NetTurnOverIssueAmount
            #region NetTurnOverAccountingAmount
            netTurnOverAccountingAmount = (EFS_InvoiceTurnOver)pEFS_Invoice.netTurnOverAccountingAmount.Clone();
            netTurnOverAccountingAmount.turnOverBaseAmount.Amount.DecValue = netTurnOverAmount.Amount.DecValue;
            if (netTurnOverAccountingAmount.TurnOverCurrency == netTurnOverAmount.Currency)
                netTurnOverAccountingAmount.turnOverAmount.Amount.DecValue = netTurnOverAmount.Amount.DecValue;
            else
                netTurnOverAccountingAmount.turnOverAmount.Amount.DecValue = 0;
            #endregion NetTurnOverAccountingAmount
            #region Tax
            taxSpecified = pInvoiceAmounts.TaxSpecified;
            if (taxSpecified)
            {
                #region TaxAmount
                tax = new EFS_InvoiceTax(pConnectionString, pInvoiceAmounts, EventTypeFunc.NetTurnOverAmount, 1);
                #endregion TaxAmount
                #region TaxIssueAmount
                netTurnOverIssueAmount.taxAmountSpecified = taxSpecified;
                netTurnOverIssueAmount.taxAmount.baseAmount.Amount.DecValue = pInvoiceAmounts.Tax.Amount.Amount.DecValue;
                netTurnOverIssueAmount.taxAmount.amount.Amount.DecValue = pInvoiceAmounts.Tax.IssueAmount.Amount.DecValue;
                SetTaxDetail(netTurnOverIssueAmount, pInvoiceAmounts.Tax);
                #endregion TaxIssueAmount
                #region TaxAccountingAmount
                netTurnOverAccountingAmount.taxAmountSpecified = taxSpecified;
                netTurnOverAccountingAmount.taxAmount.baseAmount.Amount.DecValue = pInvoiceAmounts.Tax.Amount.Amount.DecValue;
                netTurnOverAccountingAmount.taxAmount.amount.Amount.DecValue = pInvoiceAmounts.Tax.AccountingAmount.Amount.DecValue;
                SetTaxDetail(netTurnOverAccountingAmount, pInvoiceAmounts.Tax);
                #endregion TaxAccountingAmount
            }
            #endregion Tax
        }
        #endregion Constructors
        #region Methods
        private void SetTaxDetail(EFS_InvoiceTurnOver pNetTurnOver, IInvoiceTax pInvoiceTax)
        {
            int nbTaxDet = tax.details.Length;
            for (int i = 0; i < nbTaxDet; i++)
            {
                EFS_InvoiceTaxDetails taxDet = pNetTurnOver.taxAmount.details[i];
                if (pNetTurnOver.turnOverType == EventTypeFunc.NetTurnOverIssueAmount)
                    taxDet.amount.Amount.DecValue = pInvoiceTax.Details[i].TaxAmount.IssueAmount.Amount.DecValue;
                else if (pNetTurnOver.turnOverType == EventTypeFunc.NetTurnOverAccountingAmount)
                    taxDet.amount.Amount.DecValue = pInvoiceTax.Details[i].TaxAmount.AccountingAmount.Amount.DecValue;
            }
        }
        #endregion Methods
    }
    #endregion EFS_CorrectedInvoice

    #region EFS_InitialInvoice
    public class EFS_InitialInvoice : EFS_CorrectedInvoice
    {
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_InitialInvoice(string pConnectionString, IInvoiceAmounts pInvoiceAmounts, EFS_Invoice pEFS_Invoice, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pInvoiceAmounts, pEFS_Invoice, pDataDocument)
        {
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
        }
        #endregion Constructors
    }
    #endregion EFS_InitialInvoice
    #region EFS_Invoice
    public class EFS_Invoice : EFS_TheoricInvoice
    {
        #region Members
        public bool efs_InitialInvoiceSpecified;
        public EFS_InitialInvoice efs_InitialInvoice;
        public bool efs_BaseInvoiceSpecified;
        public EFS_BaseInvoice efs_BaseInvoice;
        public bool efs_TheoricInvoiceSpecified;
        public EFS_TheoricInvoice efs_TheoricInvoice;
        #endregion Members
        #region Accessors
        #region TaxAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override EFS_Decimal TaxAmount
        {
            get
            {
                if (taxSpecified)
                    return tax.amount.Amount;
                else
                    return null;
            }
        }
        #endregion TaxAmount
        #region TaxCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override string TaxCurrency
        {
            get
            {
                if (taxSpecified)
                    return tax.amount.Currency;
                else
                    return null;
            }
        }
        #endregion TaxCurrency
        #region TaxAccountingAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override EFS_Decimal TaxAccountingAmount
        {
            get
            {
                if (taxSpecified)
                    return netTurnOverAccountingAmount.taxAmount.amount.Amount;
                else
                    return null;
            }
        }
        #endregion TaxAccountingAmount
        #region TaxAccountingBase
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override EFS_InvoicingAmountBase TaxAccountingBase
        {
            get
            {
                if (taxSpecified)
                    return netTurnOverAccountingAmount.taxAmount.BaseAmount;
                else
                    return null;
            }
        }
        #endregion TaxAccountingBase
        #region TaxAccountingCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override string TaxAccountingCurrency
        {
            get
            {
                string currency = null;
                if (taxSpecified)
                {
                    currency = netTurnOverAccountingAmount.taxAmount.amount.Currency;
                    if (StrFunc.IsEmpty(currency))
                        currency = netTurnOverAccountingAmount.turnOverAmount.Currency;
                }
                return currency;
            }
        }
        #endregion TaxAccountingCurrency
        #region TaxIssueAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override EFS_Decimal TaxIssueAmount
        {
            get
            {
                if (taxSpecified)
                    return netTurnOverIssueAmount.taxAmount.amount.Amount;
                else
                    return null;
            }
        }
        #endregion TaxIssueAmount
        #region TaxIssueBase
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override EFS_InvoicingAmountBase TaxIssueBase
        {
            get
            {
                if (taxSpecified)
                    return netTurnOverIssueAmount.taxAmount.BaseAmount;
                else
                    return null;
            }
        }
        #endregion TaxIssueBase
        #region TaxIssueCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override string TaxIssueCurrency
        {
            get
            {
                string currency = null;
                if (taxSpecified)
                {
                    currency = netTurnOverIssueAmount.taxAmount.amount.Currency;
                    if (StrFunc.IsEmpty(currency))
                        currency = netTurnOverIssueAmount.turnOverAmount.Currency;
                }
                return currency;
            }
        }
        #endregion TaxIssueCurrency

        #region TurnOverAccountingAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override EFS_Decimal TurnOverAccountingAmount
        {
            get {return netTurnOverAccountingAmount.TurnOverAmount; }
        }
        #endregion TurnOverAccountingAmount
        #region TurnOverAccountingCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override string TurnOverAccountingCurrency
        {
            get {return netTurnOverAccountingAmount.TurnOverCurrency; }
        }
        #endregion TurnOverAccountingCurrency
        #region TurnOverAccountingBase
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override EFS_InvoicingAmountBase TurnOverAccountingBase
        {
            get {return netTurnOverAccountingAmount.TurnOverBase; }
        }
        #endregion TurnOverAccountingBase
        #region TurnOverAccountingRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override EFS_Decimal TurnOverAccountingRate
        {
            get
            {
                if (netTurnOverAccountingAmount.turnOverFixingRateSpecified)
                    return netTurnOverAccountingAmount.turnOverFixingRate;
                else
                    return null;
            }
        }
        #endregion TurnOverAccountingRate
        #region TurnOverIssueAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override EFS_Decimal TurnOverIssueAmount
        {
            get {return this.netTurnOverIssueAmount.TurnOverAmount; }
        }
        #endregion TurnOverIssueAmount
        #region TurnOverIssueCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override string TurnOverIssueCurrency
        {
            get {return this.netTurnOverIssueAmount.TurnOverCurrency; }
        }
        #endregion TurnOverIssueCurrency
        #region TurnOverIssueBase
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override EFS_InvoicingAmountBase TurnOverIssueBase
        {
            get {return netTurnOverIssueAmount.TurnOverBase; }
        }
        #endregion TurnOverIssueBase
        #region TurnOverIssueRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public override EFS_Decimal TurnOverIssueRate
        {
            get
            {
                if (netTurnOverIssueAmount.turnOverFixingRateSpecified)
                    return netTurnOverIssueAmount.turnOverFixingRate;
                else
                    return null;
            }
        }
        #endregion TurnOverIssueRate
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_Invoice(string pConnectionString, IInvoiceSupplement pInvoice, DataDocumentContainer pDataDocument)
            : this(pConnectionString, (IInvoice)pInvoice, pDataDocument)
        {
            efs_InitialInvoice = new EFS_InitialInvoice(m_Cs, pInvoice.InitialInvoiceAmount, this, m_DataDocument);
            efs_InitialInvoiceSpecified = (Cst.ErrLevel.SUCCESS == efs_InitialInvoice.ErrLevel);
            efs_BaseInvoice = new EFS_BaseInvoice(m_Cs, pInvoice.BaseNetInvoiceAmount, this, m_DataDocument);
            efs_BaseInvoiceSpecified = (Cst.ErrLevel.SUCCESS == efs_BaseInvoice.ErrLevel);
            efs_TheoricInvoice = new EFS_TheoricInvoice(m_Cs, pInvoice.TheoricInvoiceAmount, this, m_DataDocument);
            efs_TheoricInvoiceSpecified = (Cst.ErrLevel.SUCCESS == efs_TheoricInvoice.ErrLevel);
        }

        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_Invoice(string pConnectionString, IInvoice pInvoice, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pDataDocument)
        {
            m_Cs = pConnectionString;
            m_ErrLevel = Calc(pInvoice);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Add partyReference pour GrossTurnOver
        protected Cst.ErrLevel Calc(IInvoice pInvoice)
        {
            #region Payer/Receiver
            payerPartyReference = pInvoice.PayerPartyReference;
            receiverPartyReference = pInvoice.ReceiverPartyReference;
            #endregion Payer/Receiver
            #region Invoice Date
            invoiceDate = pInvoice.InvoiceDate;
            #endregion Invoice Date
            #region PaymentDate
            IAdjustableOrRelativeAndAdjustedDate paymentDate = pInvoice.PaymentDate;
            Cst.ErrLevel ret;
            if (paymentDate.AdjustedDateSpecified)
            {
                IProduct product = (IProduct)pInvoice;
                IAdjustedDate adjustedDate = product.ProductBase.CreateAdjustedDate(paymentDate.AdjustedDate.DateValue);
                settlementDate = new EFS_AdjustableDate(m_Cs, m_DataDocument)
                {
                    adjustableDateSpecified = false,
                    adjustedDate = adjustedDate
                };
            }
            else if (paymentDate.AdjustableDateSpecified)
            {
                settlementDate = new EFS_AdjustableDate(m_Cs, paymentDate.AdjustableDate, m_DataDocument);
            }
            else if (paymentDate.RelativeDateSpecified)
            {
                settlementDate = new EFS_AdjustableDate(m_Cs, m_DataDocument);
                ret = Tools.OffSetDateRelativeTo(m_Cs, paymentDate.RelativeDate, out DateTime[] offsetDate, m_DataDocument);
                if (ret == Cst.ErrLevel.SUCCESS)
                    settlementDate = new EFS_AdjustableDate(m_Cs, offsetDate[0], paymentDate.RelativeDate.GetAdjustments, m_DataDocument);
            }
            #endregion PaymentDate
            #region TurnOverAmount / Rebate / Tax
            grossTurnOverAmountSpecified = pInvoice.GrossTurnOverAmountSpecified;
            grossTurnOverAmount = pInvoice.GrossTurnOverAmount;

            rebateAmountSpecified = pInvoice.RebateAmountSpecified;
            if (rebateAmountSpecified)
                rebateAmount = pInvoice.RebateAmount;

            netTurnOverAmount = pInvoice.NetTurnOverAmount;

            // EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Add partyReference pour GrossTurnOver
            grossTurnOverAmountPayerPartyReference = pInvoice.CreatePartyReference(payerPartyReference.HRef);
            grossTurnOverAmountReceiverPartyReference = pInvoice.CreatePartyReference(receiverPartyReference.HRef);

            if (grossTurnOverAmountSpecified && rebateAmountSpecified)
            {
                if (grossTurnOverAmount.Amount.DecValue < rebateAmount.Amount.DecValue)
                {
                    grossTurnOverAmountPayerPartyReference.HRef = receiverPartyReference.HRef;
                    grossTurnOverAmountReceiverPartyReference.HRef = payerPartyReference.HRef;
                }
            }
            #region TaxAmount
            taxSpecified = pInvoice.TaxSpecified;
            if (taxSpecified)
                tax = new EFS_InvoiceTax(m_Cs, pInvoice, EventTypeFunc.NetTurnOverAmount, 1);
            #endregion TaxAmount

            netTurnOverIssueAmount = new EFS_InvoiceTurnOver(m_Cs, pInvoice, EventTypeFunc.NetTurnOverIssueAmount, m_DataDocument);
            netTurnOverAccountingAmount = new EFS_InvoiceTurnOver(m_Cs, pInvoice, EventTypeFunc.NetTurnOverAccountingAmount, m_DataDocument);
            #endregion TurnOverAmount / Rebate / Tax
            #region DetailGrossTurnOverAmount
            ArrayList aDetailGrossTurnOver = new ArrayList();
            foreach (IInvoiceTrade trade in pInvoice.InvoiceDetails.InvoiceTrade)
            {
                foreach (IInvoiceFee fee in trade.InvoiceFees.InvoiceFee)
                {
                    aDetailGrossTurnOver.Add(new EFS_InvoiceDetailGrossTurnOver(fee, netTurnOverAccountingAmount));
                }
            }
            if (0 < aDetailGrossTurnOver.Count)
                detailGrossTurnOver = (EFS_InvoiceDetailGrossTurnOver[])aDetailGrossTurnOver.ToArray(typeof(EFS_InvoiceDetailGrossTurnOver));
            #endregion DetailGrossTurnOverAmount
            #region Rebate
            // EG 20151016 [21272] Add test pInvoice.rebateAmountSpecified
            invoiceRebateSpecified = pInvoice.RebateConditionsSpecified && pInvoice.RebateAmountSpecified;
            if (invoiceRebateSpecified)
                invoiceRebate = new EFS_InvoiceRebate(pInvoice.RebateConditions);
            #endregion Rebate
            ret = Cst.ErrLevel.SUCCESS;
            return ret;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_Invoice
    #region EFS_InvoiceBase
    public class EFS_InvoiceBase
    {
        #region Members
        protected string m_Cs;
        protected DataDocumentContainer m_DataDocument;
        protected Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public IAdjustedDate invoiceDate;
        public EFS_AdjustableDate settlementDate;
        #endregion Members
        #region Accessors
        #region AdjustedInvoiceDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedInvoiceDate
        {
            get{return new EFS_Date(invoiceDate.Value); }
        }
        #endregion AdjustedInvoiceDate
        #region AdjustedSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedSettlementDate
        {
            get {return settlementDate.AdjustedEventDate; }
        }
        #endregion AdjustedSettlementDate
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region InvoiceDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate InvoiceDate
        {
            get {return new EFS_EventDate(invoiceDate.DateValue, invoiceDate.DateValue); }
        }
        #endregion InvoiceDate
        #region SettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate SettlementDate
        {
            get {return settlementDate.EventDate; }
        }
        #endregion SettlementDate
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_InvoiceBase(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_InvoiceBase(string pConnectionString, EFS_Invoice pEFS_Invoice, DataDocumentContainer pDataDocument)
            : this(pConnectionString, pDataDocument)
        {
            payerPartyReference = pEFS_Invoice.payerPartyReference;
            receiverPartyReference = pEFS_Invoice.receiverPartyReference;
            invoiceDate = pEFS_Invoice.invoiceDate;
            settlementDate = pEFS_Invoice.settlementDate;
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
        }
        #endregion Constructors
    }
    #endregion EFS_Invoice
    #region EFS_InvoiceDetailGrossTurnOver
    public class EFS_InvoiceDetailGrossTurnOver
    {
        #region Members
        private readonly Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public IMoney feeAmount;
        public EFS_InvoiceTurnOver feeAccountingAmount;
        public EFS_Date feeDate;
        public int idE_Source;
        public int idA_Pay;
        public bool idB_PaySpecified;
        public int idB_Pay;
        public string feeType;
        #endregion Members
        #region Constructors
        public EFS_InvoiceDetailGrossTurnOver(IInvoiceFee pInvoiceFee, EFS_InvoiceTurnOver pTurnOverAccountingAmount)
        {
            m_ErrLevel = Calc(pInvoiceFee, pTurnOverAccountingAmount);
        }
        #endregion Constructors
        #region Accessors
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region FeeAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal FeeAmount
        {
            get {return new EFS_Decimal(feeAmount.Amount.DecValue); }
        }
        #endregion FeeAmount
        #region FeeCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string FeeCurrency
        {
            get {return feeAmount.Currency; }
        }
        #endregion FeeCurrency
        #region FeeDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate FeeDate
        {
            get {return new EFS_EventDate(feeDate.DateValue, feeDate.DateValue); }
        }
        #endregion FeeDate
        #region FeeType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string FeeType
        {
            get { return feeType; }
        }
        #endregion FeeDate
        #region IdE_Source
        public int IdE_Source
        {
            get { return idE_Source; }
        }
        #endregion IdE_Source
        #region InvoicePayer
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string InvoicePayer
        {
            get
            {
                string payer = idA_Pay.ToString();
                if (idB_PaySpecified)
                    payer += ";" + idB_Pay.ToString();
                return payer;
            }
        }
        #endregion InvoicePayer
        #endregion Accessors


        #region Methods
        #region Calc
        protected Cst.ErrLevel Calc(IInvoiceFee pInvoiceFee, EFS_InvoiceTurnOver pTurnOverAccountingAmount)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            feeAmount = pInvoiceFee.FeeAmount;
            feeDate = pInvoiceFee.FeeDate;
            feeType = pInvoiceFee.FeeType.Value;
            idE_Source = pInvoiceFee.OTCmlId;
            idA_Pay = pInvoiceFee.IdA_Pay.IntValue;
            idB_PaySpecified = pInvoiceFee.IdB_PaySpecified;
            if (idB_PaySpecified)
                idB_Pay = pInvoiceFee.IdB_Pay.IntValue;

            #region FeeAccountingAmount
            feeAccountingAmount = (EFS_InvoiceTurnOver)pTurnOverAccountingAmount.Clone();
            feeAccountingAmount.turnOverBaseAmount.Amount.DecValue = feeAmount.Amount.DecValue;
            if (feeAccountingAmount.TurnOverCurrency == feeAmount.Currency)
                feeAccountingAmount.turnOverAmount.Amount.DecValue = feeAmount.Amount.DecValue;
            else
                feeAccountingAmount.turnOverAmount.Amount.DecValue = 0;
            #endregion FeeAccountingAmount
            return ret;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_InvoiceDetailGrossTurnOver
    #region EFS_InvoiceRebate
    public class EFS_InvoiceRebate
    {
        #region Members
        private readonly Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public bool totalRebateAmountSpecified;
        public IMoney totalRebateAmount;
        public bool netTurnOverInExcessAmountSpecified;
        public IMoney netTurnOverInExcessAmount;
        public bool rebateBracketSpecified;
        public EFS_InvoiceRebateBracket rebateBracket;
        #endregion Members
        #region Constructors
        public EFS_InvoiceRebate(IRebateConditions pRebateconditions)
        {
            m_ErrLevel = Calc(pRebateconditions);
        }
        #endregion Constructors
        #region Accessors
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region TotalRebateAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal TotalRebateAmount
        {
            get
            {
                EFS_Decimal amount = null;
                if (totalRebateAmountSpecified)
                    amount = new EFS_Decimal(totalRebateAmount.Amount.DecValue);
                return amount;
            }
        }
        #endregion TotalRebateAmount
        #region TotalRebateCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string TotalRebateCurrency
        {
            get
            {
                string currency = string.Empty;
                if (totalRebateAmountSpecified)
                    currency = totalRebateAmount.Currency;
                return currency;
            }
        }
        #endregion TotalRebateCurrency
        #endregion Accessors
        #region Methods
        #region Calc
        protected Cst.ErrLevel Calc(IRebateConditions pRebateconditions)
        {
            #region Rebate
            totalRebateAmountSpecified = pRebateconditions.TotalRebateAmountSpecified;
            if (totalRebateAmountSpecified)
            {
                totalRebateAmount = pRebateconditions.TotalRebateAmount;
                #region Cap
                netTurnOverInExcessAmountSpecified = pRebateconditions.CapConditionsSpecified &&
                                                     pRebateconditions.CapConditions.Result.NetTurnOverInExcessAmountSpecified;
                if (netTurnOverInExcessAmountSpecified)
                    netTurnOverInExcessAmount = pRebateconditions.CapConditions.Result.NetTurnOverInExcessAmount;
                #endregion Cap
                #region Bracket
                rebateBracketSpecified = pRebateconditions.BracketConditionsSpecified;
                if (rebateBracketSpecified)
                    rebateBracket = new EFS_InvoiceRebateBracket(pRebateconditions.BracketConditions);
                #endregion Bracket
            }
            #endregion Rebate
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_InvoiceRebate
    #region EFS_InvoiceRebateBracket
    public class EFS_InvoiceRebateBracket
    {
        #region Members
        private readonly Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public IMoney totalAmount;
        public BracketApplicationEnum managementType;
        public IRebateBracketCalculation[] details;
        #endregion Members
        #region Constructors
        public EFS_InvoiceRebateBracket(IRebateBracketConditions pRebateBracketCondition)
        {
            m_ErrLevel = Calc(pRebateBracketCondition);
        }
        #endregion Constructors
        #region Accessors
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region RebateBracketType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string RebateBracketType
        {
            get
            {
                string rebateBracketType;
                if (BracketApplicationEnum.Cumulative == managementType)
                    rebateBracketType = EventTypeFunc.DetailRebateCumulative;
                else
                    rebateBracketType = EventTypeFunc.DetailRebateUnit;
                return rebateBracketType;
            }
        }
        #endregion RebateBracketType
        #region TotalRebateBracketAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal TotalRebateBracketAmount
        {
            get {return new EFS_Decimal(totalAmount.Amount.DecValue); }
        }
        #endregion TotalRebateBracketAmount
        #region TotalRebateBracketCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string TotalRebateBracketCurrency
        {
            get {return totalAmount.Currency; }
        }
        #endregion TotalRebateBracketCurrency
        #endregion Accessors
        #region Methods
        #region Calc
        protected Cst.ErrLevel Calc(IRebateBracketConditions pRebateBracketCondition)
        {
            #region Rebate
            totalAmount = pRebateBracketCondition.Result.TotalRebateBracketAmount;
            managementType = pRebateBracketCondition.Parameters.ManagementType;
            IRebateBracketCalculation[] brackets = pRebateBracketCondition.Result.Calculations.RebateBracketCalculation;
            ArrayList aBrackets = new ArrayList();
            foreach (IRebateBracketCalculation bracket in brackets)
            {
                if (bracket.BracketAmountSpecified)
                    aBrackets.Add(bracket);
            }
            if (0 < aBrackets.Count)
                details = (IRebateBracketCalculation[])aBrackets.ToArray(typeof(IRebateBracketCalculation));
            #endregion Rebate
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_InvoiceRebateBracket
    #region EFS_InvoiceSettlement
    public class EFS_InvoiceSettlement
    {
        #region Members
        protected string m_Cs;
        protected Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public IReference bankPartyReference;
        public SQL_Actor bankActor;
        public IAdjustedDate receptionDate;
        public IMoney settlementAmount;
        public IMoney cashAmount;
        public IMoney netCashAmount;
        public bool bankChargesAmountSpecified;
        public IMoney bankChargesAmount;
        public IMoney vatBankChargesAmount;
        public bool vatBankChargesAmountSpecified;
        public IMoney fxGainOrLossAmount;
        public bool fxGainOrLossAmountSpecified;
        public IMoney unallocatedAmount;
        public bool unallocatedAmountSpecified;
        public bool allocatedInvoiceSpecified;
        public EFS_AllocatedInvoice[] allocatedInvoice;
        #endregion Members
        #region Accessors
        #region BankPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string BankPartyReference
        {
            get {return bankPartyReference.HRef; }
        }
        #endregion BankPartyReference
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region FxGainOrLossType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string FxGainOrLossType
        {
            get
            {
                string eventType = string.Empty;
                if (fxGainOrLossAmountSpecified)
                {
                    if (0 < fxGainOrLossAmount.Amount.DecValue)
                        eventType = EventTypeFunc.ForeignExchangeProfit;
                    else
                        eventType = EventTypeFunc.ForeignExchangeLoss;
                }
                return eventType;
            }
        }
        #endregion FxGainOrLossType
        #region FxGainOrLossPayerReference
        // EG 20110510
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string FxGainOrLossPayerReference
        {
            get
            {
                if (FxGainOrLossType == EventTypeFunc.ForeignExchangeProfit)
                    return payerPartyReference.HRef;
                else
                    return receiverPartyReference.HRef;
            }
        }
        #endregion FxGainOrLossPayerReference
        #region FxGainOrLossReceiverReference
        // EG 20110510
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string FxGainOrLossReceiverReference
        {
            get
            {
                if (FxGainOrLossType == EventTypeFunc.ForeignExchangeProfit)
                    return receiverPartyReference.HRef;
                else
                    return payerPartyReference.HRef;
            }
        }
        #endregion FxGainOrLossReceiverReference
        #region FxGainOrLossAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal FxGainOrLossAmount
        {
            get
            {
                EFS_Decimal amount = new EFS_Decimal();
                if (fxGainOrLossAmountSpecified)
                    amount.DecValue = Math.Abs(fxGainOrLossAmount.Amount.DecValue);
                return amount; 
            }
        }
        #endregion FxGainOrLossAmount
        #region SettlementAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal SettlementAmount
        {
            get {return this.settlementAmount.Amount; }
        }
        #endregion SettlementAmount
        #region SettlementCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string SettlementCurrency
        {
            get {return this.settlementAmount.Currency; }
        }
        #endregion SettlementCurrency
        #endregion Accessors
        #region Constructors
        public EFS_InvoiceSettlement(string pConnectionString, IInvoiceSettlement pInvoiceSettlement)
        {
            m_Cs = pConnectionString;
            m_ErrLevel = Calc(pInvoiceSettlement);
        }
        #endregion Constructors
        #region Indexors
        public EFS_AllocatedInvoice this[int pIdT]
        {
            get
            {
                EFS_AllocatedInvoice ret = null;
                if (allocatedInvoiceSpecified)
                {
                    foreach (EFS_AllocatedInvoice allocated in allocatedInvoice)
                    {
                        if (pIdT == allocated.invoiceIdT)
                        {
                            ret = allocated;
                            break;
                        }
                    }
                }
                return ret;
            }
        }
        #endregion Indexors

        #region Methods
        #region Calc
        // 20090729 EG Correction BUG génération des evts de règlement si pas de frais bancaires (AddParty EventGenService)
        protected Cst.ErrLevel Calc(IInvoiceSettlement pInvoiceSettlement)
        {
            IProduct product = (IProduct)pInvoiceSettlement;
            #region Reception Date
            receptionDate = pInvoiceSettlement.ReceptionDate;
            #endregion Reception Date
            #region Payer/Receiver
            payerPartyReference = pInvoiceSettlement.PayerPartyReference;
            receiverPartyReference = pInvoiceSettlement.ReceiverPartyReference;
            #endregion Payer/Receiver
            #region Bank charges
            bankActor = new SQL_Actor(m_Cs, pInvoiceSettlement.AccountNumber.Correspondant.OTCmlId);
            if (bankActor.IsLoaded)
                bankPartyReference = product.ProductBase.CreatePartyOrAccountReference(bankActor.XmlId);
            bankChargesAmountSpecified = pInvoiceSettlement.BankChargesAmountSpecified;
            if (bankChargesAmountSpecified)
                bankChargesAmount = pInvoiceSettlement.BankChargesAmount;
            vatBankChargesAmountSpecified = pInvoiceSettlement.VatBankChargesAmountSpecified;
            if (vatBankChargesAmountSpecified)
                vatBankChargesAmount = pInvoiceSettlement.VatBankChargesAmount;
            #endregion Bank charges
            #region SettlementAmount / CashAmount / NetCashAmount
            settlementAmount = pInvoiceSettlement.SettlementAmount;
            cashAmount = pInvoiceSettlement.CashAmount;
            netCashAmount = pInvoiceSettlement.NetCashAmount;
            #endregion SettlementAmount / CashAmount / NetCashAmount
            #region FxGainOrLossAmount
            fxGainOrLossAmountSpecified = pInvoiceSettlement.FxGainOrLossAmountSpecified;
            if (fxGainOrLossAmountSpecified)
                fxGainOrLossAmount = pInvoiceSettlement.FxGainOrLossAmount;
            #endregion FxGainOrLossAmount
            #region UnallocatedAmount
            unallocatedAmountSpecified = (0 < pInvoiceSettlement.UnallocatedAmount.Amount.DecValue);
            if (unallocatedAmountSpecified)
                unallocatedAmount = pInvoiceSettlement.UnallocatedAmount;
            #endregion UnallocatedAmount
            #region AllocatedInvoice
            allocatedInvoiceSpecified = (pInvoiceSettlement.AllocatedInvoiceSpecified && (0 < pInvoiceSettlement.AllocatedInvoice.Length));
            if (allocatedInvoiceSpecified)
            {
                ArrayList aAllocated = new ArrayList();
                foreach (IAllocatedInvoice allocated in pInvoiceSettlement.AllocatedInvoice)
                {
                    aAllocated.Add(new EFS_AllocatedInvoice(m_Cs, receptionDate, payerPartyReference, receiverPartyReference, allocated));
                }
                if (0 < aAllocated.Count)
                    allocatedInvoice = (EFS_AllocatedInvoice[])aAllocated.ToArray(typeof(EFS_AllocatedInvoice));
            }
            #endregion AllocatedInvoice
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_InvoiceSettlement
    #region EFS_InvoiceTax
    public class EFS_InvoiceTax : ICloneable
    {
        #region Members
        private readonly Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public string turnOverType;
        public IMoney baseAmount;
        public IMoney amount;
        public EFS_InvoiceTaxDetails[] details;
        #endregion Members
        #region Accessors
        #region Amount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal Amount
        {
            get {return amount.Amount; }
        }
        #endregion Amount
        #region BaseAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_InvoicingAmountBase BaseAmount
        {
            get {return new EFS_InvoicingAmountBase(baseAmount.Amount, amount.Currency);}
        }
        #endregion BaseAmount
        #region Currency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string Currency
        {
            get {return amount.Currency; }
        }
        #endregion Currency
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #endregion Accessors
        #region Constructors
        public EFS_InvoiceTax()
        {
        }
        public EFS_InvoiceTax(string pCs, IInvoice pInvoice, string pTurnOverType, decimal pQuoteValue)
        {
            m_ErrLevel = Cst.ErrLevel.UNDEFINED;
            turnOverType = pTurnOverType;
            baseAmount = ((IProduct)pInvoice).ProductBase.CreateMoney(pInvoice.Tax.Amount.Amount.DecValue, pInvoice.Tax.Amount.Currency);
            if (turnOverType == EventTypeFunc.NetTurnOverAmount)
                amount = pInvoice.Tax.Amount;
            else if (turnOverType == EventTypeFunc.NetTurnOverIssueAmount)
                amount = pInvoice.Tax.IssueAmount;
            else if (turnOverType == EventTypeFunc.NetTurnOverAccountingAmount)
                amount = pInvoice.Tax.AccountingAmount;

            SetTaxDetails(pCs,pInvoice.Tax,pQuoteValue);
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
        }

        public EFS_InvoiceTax(string pCs,INetInvoiceAmounts pInvoice, string pTurnOverType,decimal pQuoteValue)
        {
            m_ErrLevel = Cst.ErrLevel.UNDEFINED;
            turnOverType = pTurnOverType;
            baseAmount = pInvoice.CreateMoney(pInvoice.Tax.Amount.Amount.DecValue, pInvoice.Tax.Amount.Currency);
            if (turnOverType == EventTypeFunc.NetTurnOverAmount)
                amount = pInvoice.Tax.Amount;
            else if (turnOverType == EventTypeFunc.NetTurnOverIssueAmount)
                amount = pInvoice.Tax.IssueAmount;
            else if (turnOverType == EventTypeFunc.NetTurnOverAccountingAmount)
                amount = pInvoice.Tax.AccountingAmount;

            SetTaxDetails(pCs,pInvoice.Tax, pQuoteValue);
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
        }
        public EFS_InvoiceTax(string pCs,IInvoiceAmounts pInvoice, string pTurnOverType,decimal pQuoteValue)
        {
            turnOverType = pTurnOverType;
            baseAmount = pInvoice.CreateMoney(pInvoice.Tax.Amount.Amount.DecValue, pInvoice.Tax.Amount.Currency);
            if (turnOverType == EventTypeFunc.NetTurnOverAmount)
                amount = pInvoice.Tax.Amount;
            else if (turnOverType == EventTypeFunc.NetTurnOverIssueAmount)
                amount = pInvoice.Tax.IssueAmount;
            else if (turnOverType == EventTypeFunc.NetTurnOverAccountingAmount)
                amount = pInvoice.Tax.AccountingAmount;

            SetTaxDetails(pCs,pInvoice.Tax, pQuoteValue);
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
        }
        #endregion Constructors
        #region Methods
        #region SetTaxDetails
        private void SetTaxDetails(string pCs,IInvoiceTax pInvoiceTax,decimal pQuoteValue)
        {
            int nbTax = pInvoiceTax.Details.Length;
            details = new EFS_InvoiceTaxDetails[nbTax];
            for (int i = 0; i < nbTax; i++)
                details[i] = new EFS_InvoiceTaxDetails(pCs, turnOverType, pInvoiceTax.Details[i], pQuoteValue);
        }
        #endregion SetTaxDetails
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            EFS_InvoiceTax clone = new EFS_InvoiceTax
            {
                baseAmount = (IMoney)baseAmount.Clone(),
                amount = (IMoney)amount.Clone(),
                details = new EFS_InvoiceTaxDetails[details.Length]
            };
            for (int i = 0; i < details.Length; i++)
                clone.details[i] = (EFS_InvoiceTaxDetails)details[i].Clone();
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #endregion Methods
    }
    #endregion EFS_InvoiceTax
    #region EFS_InvoiceTaxDetails
    public class EFS_InvoiceTaxDetails : ICloneable
    {
        #region Members
        private readonly Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public IMoney amount;
        public string eventType;
        public EFS_TaxSource taxSource;
        #endregion Members
        #region Accessors
        #region Amount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal Amount
        {
            get {return amount.Amount;}
        }
        #endregion Amount
        #region Currency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string Currency
        {
            get {return amount.Currency;}
        }
        #endregion Currency
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string EventType
        {
            get {return eventType; }
        }
        #endregion EventType
        #region TaxSource
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_TaxSource TaxSource
        {
            get {return taxSource; }
        }
        #endregion TaxSource
        #endregion Accessors
        #region Constructors
        public EFS_InvoiceTaxDetails()
        {
        }
        public EFS_InvoiceTaxDetails(string pCs, string pTurnOverType, ITaxSchedule pTaxSchedule) : this(pCs,pTurnOverType, pTaxSchedule, 1) { }
        public EFS_InvoiceTaxDetails(string pCs, string pTurnOverType, ITaxSchedule pTaxSchedule, decimal pRate)
        {
            if (pTaxSchedule.TaxAmountSpecified)
            {
                IMoney _amount = null;
                decimal baseAmount = pTaxSchedule.TaxAmount.Amount.Amount.DecValue;
                if (pTurnOverType == EventTypeFunc.NetTurnOverAmount)
                    _amount = pTaxSchedule.TaxAmount.Amount;
                else if (pTurnOverType == EventTypeFunc.NetTurnOverIssueAmount)
                    _amount = pTaxSchedule.TaxAmount.IssueAmount;
                else if (pTurnOverType == EventTypeFunc.NetTurnOverAccountingAmount)
                    _amount = pTaxSchedule.TaxAmount.AccountingAmount;

                EFS_Cash cash = new EFS_Cash(pCs, baseAmount * pRate, _amount.Currency);
                amount = pTaxSchedule.CreateMoney(cash.AmountRounded, _amount.Currency);
                SetSource(pTaxSchedule);
            }
        }
        #endregion Constructors
        #region Methods
        #region SetSource
        private void SetSource(ITaxSchedule pTaxSchedule)
        {
            ISpheresIdSchemeId spheresIdScheme = pTaxSchedule.TaxSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailEventTypeScheme);
            if (null != spheresIdScheme)
                eventType = pTaxSchedule.TaxSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailEventTypeScheme).Value;
            taxSource = new EFS_TaxSource(pTaxSchedule.TaxSource, pTaxSchedule.TaxSource);
        }
        #endregion SetEventType
        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            EFS_InvoiceTaxDetails clone = new EFS_InvoiceTaxDetails
            {
                amount = (IMoney)amount.Clone(),
                eventType = eventType,
                taxSource = (EFS_TaxSource)taxSource.Clone()
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #endregion Methods
    }
    #endregion EFS_InvoiceTaxDetails
    #region EFS_InvoiceTurnOver
    public class EFS_InvoiceTurnOver : ICloneable
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument; 
        private readonly Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;

        public bool taxAmountSpecified;
        public EFS_InvoiceTax taxAmount;

        public IMoney turnOverBaseAmount;
        public IMoney turnOverAmount;
        public string turnOverType;
        public bool turnOverFixingSpecified;
        public EFS_FxFixing turnOverFixing;
        public bool turnOverFixingRateSpecified;
        public EFS_Decimal turnOverFixingRate;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_InvoiceTurnOver(string pConnectionString, string pTurnOverType, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            turnOverType = pTurnOverType;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_InvoiceTurnOver(string pConnectionString, IInvoice pInvoice, string pTurnOverType, DataDocumentContainer pDataDocument)
            :this(pConnectionString, pTurnOverType, pDataDocument)
        {
            m_ErrLevel = Calc(pInvoice, pTurnOverType);
        }
        #endregion Constructors
        #region Accessors
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region TurnOverAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal TurnOverAmount
        {
            get{return turnOverAmount.Amount; }
        }
        #endregion TurnOverAmount
        #region TurnOverCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string TurnOverCurrency
        {
            get {return turnOverAmount.Currency; }
        }
        #endregion TurnOverCurrency
        #region TurnOverBase
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_InvoicingAmountBase TurnOverBase
        {
            get {return new EFS_InvoicingAmountBase(turnOverBaseAmount.Amount, turnOverAmount.Currency);}
        }
        #endregion TurnOverBase
        #region TurnOverRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal TurnOverRate
        {
            get
            {
                if (turnOverFixingRateSpecified)
                    return turnOverFixingRate;
                else
                    return null;
            }
        }
        #endregion TurnOverRate
        #endregion Accessors
        #region Methods
        #region Calc
        protected Cst.ErrLevel Calc(IInvoice pInvoice, string pTurnOverType)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;
            #region NetTurnOver
            turnOverBaseAmount = ((IProduct)pInvoice).ProductBase.CreateMoney(pInvoice.NetTurnOverAmount.Amount.DecValue, pInvoice.NetTurnOverAmount.Currency);
            turnOverType = pTurnOverType;
            if (EventTypeFunc.IsNetTurnOverIssueAmount(turnOverType))
            {
                turnOverAmount = pInvoice.NetTurnOverIssueAmount;
                codeReturn = SetTurnOverIssueFixing(pInvoice);
            }
            else if (EventTypeFunc.IsNetTurnOverAccountingAmount(turnOverType))
            {
                codeReturn = SetTurnOverAccountingAmount(pInvoice);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = SetTurnOverAccountingFixing(pInvoice);
            }
            #endregion NetTurnOver
            #region Tax
            taxAmountSpecified = pInvoice.TaxSpecified;
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && taxAmountSpecified)
            {
                decimal quoteValue = 0;
                if (turnOverFixingRateSpecified)
                    quoteValue = turnOverFixingRate.DecValue;
                else if (turnOverBaseAmount.Currency == turnOverAmount.Currency)
                    quoteValue = 1;
                taxAmount = new EFS_InvoiceTax(m_Cs,pInvoice, turnOverType, quoteValue);
                codeReturn = taxAmount.ErrLevel;
            }
            #endregion Tax
            return codeReturn;
        }
        #endregion Calc
        #region SetTurnOverAccountingFixing
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel SetTurnOverAccountingFixing(IInvoice pInvoice)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            turnOverFixingSpecified = (turnOverAmount.Currency != pInvoice.NetTurnOverAmount.Currency);
            if (turnOverFixingSpecified)
            {
                IProductBase productBase = (IProductBase)pInvoice;
                IFxFixing fixing = productBase.CreateFxFixing();
                KeyAssetFxRate keyAssetFxRate = new KeyAssetFxRate
                {
                    IdC1 = pInvoice.NetTurnOverAmount.Currency,
                    IdC2 = turnOverAmount.Currency
                };
                int idAsset = keyAssetFxRate.GetIdAsset(m_Cs, null);
                if (0 < idAsset)
                {
                    SQL_AssetFxRate sql_AssetFxRate = new SQL_AssetFxRate(m_Cs, idAsset, SQL_Table.ScanDataDtEnabledEnum.Yes);
                    if (sql_AssetFxRate.IsLoaded)
                    {
                        fixing.PrimaryRateSource = fixing.CreateInformationSource();
                        fixing.PrimaryRateSource.OTCmlId = sql_AssetFxRate.Id;
                        fixing.PrimaryRateSource.RateSource.Value = sql_AssetFxRate.PrimaryRateSrc;
                        fixing.PrimaryRateSource.RateSourcePageSpecified = StrFunc.IsFilled(sql_AssetFxRate.PrimaryRateSrcPage);
                        if (fixing.PrimaryRateSource.RateSourcePageSpecified)
                            fixing.PrimaryRateSource.CreateRateSourcePage(sql_AssetFxRate.PrimaryRateSrcPage);
                        fixing.PrimaryRateSource.RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_AssetFxRate.PrimaryRateSrcHead);
                        fixing.PrimaryRateSource.RateSourcePageHeading = sql_AssetFxRate.PrimaryRateSrcHead;
                        fixing.CreateQuotedCurrencyPair(sql_AssetFxRate.QCP_Cur1, sql_AssetFxRate.QCP_Cur2, sql_AssetFxRate.QCP_QuoteBasisEnum);
                        //fixing.CreateBusinessCenterTime
                        fixing.FixingTime = fixing.CreateBusinessCenterTime();
                        fixing.FixingTime.HourMinuteTime.TimeValue = sql_AssetFxRate.TimeRateSrc;
                        fixing.FixingTime.BusinessCenter.Value = sql_AssetFxRate.IdBC_RateSrc;
                        fixing.FixingDate = new EFS_Date();

                        IBusinessDayAdjustments bda = productBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.PRECEDING, sql_AssetFxRate.IdBC_RateSrc);
                        EFS_AdjustableDate fixingDate = new EFS_AdjustableDate(m_Cs, pInvoice.InvoiceDate.DateValue, bda, m_DataDocument);
                        fixing.FixingDate.DateValue = fixingDate.AdjustedEventDate.DateValue;
                        fixing.QuotedCurrencyPair = fixing.CreateQuotedCurrencyPair(sql_AssetFxRate.QCP_Cur1, sql_AssetFxRate.QCP_Cur2, sql_AssetFxRate.QCP_QuoteBasisEnum);
                        turnOverFixing = new EFS_FxFixing(EventTypeFunc.FxRate, fixing);
                        turnOverFixingSpecified = true;
                    }
                }
                else
                {
                    //EG 20091207 Affichage erreur si Asset non trouvé
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Asset not found : " + keyAssetFxRate.IdC1 + "-" + keyAssetFxRate.IdC2);
                }
            }
            return codeReturn;
        }
        #endregion SetTurnOverAccountingFixing
        #region SetTurnOverAccountingAmount
        // EG 20150706 [21021] Nullable<int> idB
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Reverse du Payeur/Receiver si GTO < 0 (Cas d'un Rebate > GTO de base)
        private Cst.ErrLevel SetTurnOverAccountingAmount(IInvoice pInvoice)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IProductBase productBase = (IProductBase)pInvoice;
            // EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Reverse du Payeur/Receiver si GTO < 0 (Cas d'un Rebate > GTO de base)
            Nullable<int> idB;
            if (productBase.IsCreditNote)
                idB = m_DataDocument.GetOTCmlId_Book(pInvoice.PayerPartyReference.HRef)?? m_DataDocument.GetOTCmlId_Book(pInvoice.ReceiverPartyReference.HRef);
            else
                idB = m_DataDocument.GetOTCmlId_Book(pInvoice.ReceiverPartyReference.HRef) ?? m_DataDocument.GetOTCmlId_Book(pInvoice.PayerPartyReference.HRef);
            if (idB.HasValue)
            {
                SQL_Book book = new SQL_Book(m_Cs, idB.Value);
                if (book.IsLoaded)
                {
                    string logMessage;
                    if (0 < book.IdA_Entity)
                    {
                        SQL_Actor actor = new SQL_Actor(m_Cs, book.IdA_Entity)
                        {
                            WithInfoEntity = true
                        };
                        if (actor.IsLoaded)
                        {
                            if (actor.IsEntityExist)
                            {

                                if (StrFunc.IsFilled(actor.IdCAccount))
                                {
                                    decimal amount = 0;
                                    if (actor.IdCAccount == pInvoice.NetTurnOverAmount.Currency)
                                        amount = pInvoice.NetTurnOverAmount.Amount.DecValue;
                                    turnOverAmount = productBase.CreateMoney(amount, actor.IdCAccount);
                                }
                                else
                                {
                                    logMessage = "Accounting currency is not specified for Accounting entity: " + actor.Identifier + "[id=" + book.IdA_Entity.ToString() + "]";
                                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, logMessage);
                                }
                            }
                            else
                            {
                                logMessage = "Accounting entity: " + actor.Identifier + "[id=" + book.IdA_Entity.ToString() + "] specified for this book: " +
                                    book.Identifier + "[id=" + idB + "] does not exist";
                                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, logMessage);
                            }
                        }
                        else
                        {
                            logMessage = "Accounting entity not loaded: " + "[id=" + book.IdA_Entity.ToString() + "] specified for this book: " + book.Identifier + "[id=" + idB + "]";
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, logMessage);
                        }
                    }
                    else
                    {
                        logMessage = "Accounting entity is missing for this book: " + book.Identifier + "[id=" + idB + "]";
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, logMessage);
                    }

                }
            }
            return codeReturn;
        }
        #endregion SetTurnOverAccountingAmount
        #region SetTurnOverIssueFixing
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel SetTurnOverIssueFixing(IInvoice pInvoice)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            turnOverFixingSpecified = pInvoice.InvoiceRateSourceSpecified;
            if (turnOverFixingSpecified)
            {
                IProductBase productBase = (IProductBase)pInvoice;
                IFxFixing fixing = productBase.CreateFxFixing();
                fixing.PrimaryRateSource = pInvoice.InvoiceRateSource.RateSource;
                if (false == pInvoice.InvoiceRateSource.AdjustedFixingDateSpecified)
                {
                    codeReturn = Tools.OffSetDateRelativeTo(m_Cs, pInvoice.InvoiceRateSource.FixingDate, out DateTime offsetDate, m_DataDocument);
                    if (codeReturn == Cst.ErrLevel.SUCCESS)
                    {
                        pInvoice.InvoiceRateSource.AdjustedFixingDateSpecified = DtFunc.IsDateTimeFilled(offsetDate);
                        pInvoice.InvoiceRateSource.AdjustedFixingDate = productBase.CreateAdjustedDate(offsetDate);
                    }
                    else
                    {
                        string logMessage = "NetTurnOver Issue rate fixing calculation error";
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, logMessage);
                    }
                }
                fixing.FixingDate = new EFS_Date(pInvoice.InvoiceRateSource.AdjustedFixingDate.Value);
                fixing.FixingTime = pInvoice.InvoiceRateSource.FixingTime;
                SQL_AssetFxRate sql_AssetFxRate = new SQL_AssetFxRate(m_Cs, pInvoice.InvoiceRateSource.RateSource.OTCmlId, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (sql_AssetFxRate.IsLoaded)
                    fixing.QuotedCurrencyPair = fixing.CreateQuotedCurrencyPair(sql_AssetFxRate.QCP_Cur1, sql_AssetFxRate.QCP_Cur2, sql_AssetFxRate.QCP_QuoteBasisEnum);
                turnOverFixing = new EFS_FxFixing(EventTypeFunc.FxRate, fixing);
                turnOverFixingSpecified = true;
            }
            turnOverFixingRateSpecified = pInvoice.NetTurnOverIssueRateSpecified;
            if (turnOverFixingRateSpecified)
                turnOverFixingRate = pInvoice.NetTurnOverIssueRate;
            return codeReturn;
        }
        #endregion SetTurnOverIssueFixing
        #endregion Methods
        #region ICloneable Members
        #region Clone
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public object Clone()
        {
            EFS_InvoiceTurnOver clone = new EFS_InvoiceTurnOver(m_Cs, turnOverType, m_DataDocument)
            {
                turnOverAmount = (IMoney)turnOverAmount.Clone(),
                turnOverBaseAmount = (IMoney)turnOverBaseAmount.Clone(),
                turnOverFixingSpecified = turnOverFixingSpecified
            };
            if (turnOverFixingSpecified)
                clone.turnOverFixing = (EFS_FxFixing)turnOverFixing.Clone();
            clone.turnOverFixingRateSpecified = turnOverFixingRateSpecified;
            if (turnOverFixingRateSpecified)
                clone.turnOverFixingRate = new EFS_Decimal(turnOverFixingRate.DecValue);
            clone.taxAmountSpecified = taxAmountSpecified;
            if (taxAmountSpecified)
                clone.taxAmount = (EFS_InvoiceTax)taxAmount.Clone();
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
    }
    #endregion EFS_InvoiceTurnOver
    #region EFS_InvoicingAmountBase
    public class EFS_InvoicingAmountBase
    {
        #region Members
        public EFS_Decimal notionalAmount;
        public string referenceCurrency;
        #endregion Members
        #region Constructors
        public EFS_InvoicingAmountBase(EFS_Decimal pNotionalAmount, string pReferenceCurrency)
        {
            notionalAmount = pNotionalAmount;
            referenceCurrency = pReferenceCurrency;
        }
        #endregion Constructors
    }
    #endregion EFS_InvoicingAmountBase

    #region EFS_TheoricInvoice
    public class EFS_TheoricInvoice : EFS_CorrectedInvoice
    {
        #region Members
        public bool invoiceRebateSpecified;
        public EFS_InvoiceRebate invoiceRebate;
        public EFS_InvoiceDetailGrossTurnOver[] detailGrossTurnOver;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_TheoricInvoice(string pConnectionString, DataDocumentContainer pDataDocument) : base(pConnectionString, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_TheoricInvoice(string pConnectionString, IInvoiceAmounts pInvoiceAmounts, EFS_Invoice pEFS_Invoice, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pInvoiceAmounts, pEFS_Invoice, pDataDocument)
        {
            invoiceRebateSpecified = pEFS_Invoice.invoiceRebateSpecified;
            invoiceRebate = pEFS_Invoice.invoiceRebate;
            detailGrossTurnOver = pEFS_Invoice.detailGrossTurnOver;
            m_ErrLevel = Cst.ErrLevel.SUCCESS;

        }
        #endregion Constructors
    }
    #endregion EFS_TheoricInvoice

}

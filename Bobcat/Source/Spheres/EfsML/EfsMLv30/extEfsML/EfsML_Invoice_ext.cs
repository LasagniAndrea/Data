#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.AssetDef;
using EfsML.v30.Repository;
using EfsML.v30.Shared;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Doc;
using FpML.v44.Shared;
using System;
using System.Reflection;

#endregion using directives


namespace EfsML.v30.Invoice
{
    #region AdditionalInvoice
    public partial class AdditionalInvoice : IProduct, IAdditionalInvoice
    {
        #region Accessors
        #endregion Accessors
        #region Constructors
        public AdditionalInvoice():base()
        {
            initialInvoiceAmount = new InitialInvoiceAmounts();
            baseNetInvoiceAmount = new NetInvoiceAmounts();
            theoricInvoiceAmount = new InvoiceAmounts();
        }
        #endregion Constructors
        #region Interfaces
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members  
        #region IAdditionalInvoice Members
        IInitialInvoiceAmounts IInvoiceSupplement.InitialInvoiceAmount
        {
            set { this.initialInvoiceAmount = (InitialInvoiceAmounts)value; }
            get {return this.initialInvoiceAmount; }
        }
        INetInvoiceAmounts IInvoiceSupplement.BaseNetInvoiceAmount
        {
            set { this.baseNetInvoiceAmount = (NetInvoiceAmounts)value; }
            get { return this.baseNetInvoiceAmount; }
        }
        IInvoiceAmounts IInvoiceSupplement.TheoricInvoiceAmount
        {
            set { this.theoricInvoiceAmount = (InvoiceAmounts)value; }
            get { return this.theoricInvoiceAmount; }
        }
        #endregion IAdditionalInvoice Members
        #region IInvoice Members
        IReference IInvoice.PayerPartyReference
        {
            set { this.payerPartyReference = (PartyOrAccountReference)value; }
            get { return this.payerPartyReference; }
        }
        IReference IInvoice.ReceiverPartyReference
        {
            set { this.receiverPartyReference = (PartyOrAccountReference)value; }
            get { return this.receiverPartyReference; }
        }
        IAdjustedDate IInvoice.InvoiceDate
        {
            set { this.invoiceDate = (IdentifiedDate)value; }
            get { return this.invoiceDate; }
        }
        IAdjustableOrRelativeAndAdjustedDate IInvoice.PaymentDate
        {
            set { this.paymentDate = (AdjustableOrRelativeAndAdjustedDate)value; }
            get { return this.paymentDate; }
        }
        bool IInvoice.GrossTurnOverAmountSpecified
        {
            set { this.grossTurnOverAmountSpecified = value; }
            get { return this.grossTurnOverAmountSpecified; }
        }
        bool IInvoice.RebateIsInExcessSpecified
        {
            set { this.rebateIsInExcessSpecified = value; }
            get { return this.rebateIsInExcessSpecified; }
        }
        EFS_Boolean IInvoice.RebateIsInExcess
        {
            set { this.rebateIsInExcess = value; }
            get { return this.rebateIsInExcess; }
        }
        bool IInvoice.RebateAmountSpecified
        {
            set { this.rebateAmountSpecified = value; }
            get { return this.rebateAmountSpecified; }
        }
        IMoney IInvoice.RebateAmount
        {
            set { this.rebateAmount = (Money)value; }
            get { return this.rebateAmount; }
        }
        bool IInvoice.NetTurnOverAccountingAmountSpecified
        {
            set { this.netTurnOverAccountingAmountSpecified = value; }
            get { return this.netTurnOverAccountingAmountSpecified; }
        }
        IMoney IInvoice.NetTurnOverAccountingAmount
        {
            set { this.netTurnOverAccountingAmount = (Money)value; }
            get { return this.netTurnOverAccountingAmount; }
        }
        bool IInvoice.NetTurnOverAccountingRateSpecified
        {
            set { this.netTurnOverAccountingRateSpecified = value; }
            get { return this.netTurnOverAccountingRateSpecified; }
        }
        EFS_Decimal IInvoice.NetTurnOverAccountingRate
        {
            set { this.netTurnOverAccountingRate = value; }
            get { return this.netTurnOverAccountingRate; }
        }
        IMoney IInvoice.GrossTurnOverAmount
        {
            set { this.grossTurnOverAmount = (Money)value; }
            get { return this.grossTurnOverAmount; }
        }
        bool IInvoice.RebateConditionsSpecified
        {
            set { this.rebateConditionsSpecified = value; }
            get { return this.rebateConditionsSpecified; }
        }
        IRebateConditions IInvoice.RebateConditions
        {
            set { this.rebateConditions = (RebateConditions)value; }
            get { return this.rebateConditions; }
        }
        bool IInvoice.TaxSpecified
        {
            set { this.taxSpecified = value; }
            get { return this.taxSpecified; }
        }
        IInvoiceTax IInvoice.Tax
        {
            set { this.tax = (InvoiceTax)value; }
            get { return this.tax; }
        }
        IMoney IInvoice.NetTurnOverAmount
        {
            set { this.netTurnOverAmount = (Money)value; }
            get { return this.netTurnOverAmount; }
        }
        IMoney IInvoice.NetTurnOverIssueAmount
        {
            set { this.netTurnOverIssueAmount = (Money)value; }
            get { return this.netTurnOverIssueAmount; }
        }
        bool IInvoice.NetTurnOverIssueRateSpecified
        {
            set { this.netTurnOverIssueRateSpecified = value; }
            get { return this.netTurnOverIssueRateSpecified; }
        }
        EFS_Decimal IInvoice.NetTurnOverIssueRate
        {
            set { this.netTurnOverIssueRate = value; }
            get { return this.netTurnOverIssueRate; }
        }
        bool IInvoice.InvoiceRateSourceSpecified
        {
            set { this.invoiceRateSourceSpecified = value; }
            get { return this.invoiceRateSourceSpecified; }
        }
        IInvoiceRateSource IInvoice.InvoiceRateSource
        {
            set { this.invoiceRateSource = (InvoiceRateSource)value; }
            get { return this.invoiceRateSource; }
        }
        IInvoiceDetails IInvoice.InvoiceDetails
        {
            set { this.invoiceDetails = (InvoiceDetails)value; }
            get { return this.invoiceDetails; }
        }
        IInvoicingScope IInvoice.Scope
        {
            set { this.scope = (InvoicingScope)value; }
            get { return this.scope; }
        }
        EFS_Invoice IInvoice.Efs_Invoice
        {
            set { this.efs_Invoice = value; }
            get { return this.efs_Invoice; }
        }
        EFS_BaseInvoice IInvoice.Efs_BaseInvoice
        {
            set { this.efs_BaseInvoice = value; }
            get { return this.efs_BaseInvoice; }
        }
        EFS_TheoricInvoice IInvoice.Efs_TheoricInvoice
        {
            set { this.efs_TheoricInvoice = value; }
            get { return this.efs_TheoricInvoice; }
        }
        EFS_InitialInvoice IInvoice.Efs_InitialInvoice
        {
            set { this.efs_InitialInvoice = value; }
            get { return this.efs_InitialInvoice; }
        }
        IInvoicingScope IInvoice.CreateInvoicingScope(int pOTCmlId, string pIdentifier)
        {
            InvoicingScope invoicingScope = new InvoicingScope
            {
                invoicingScopeScheme = "http://www.euro-finance-systems.fr/otcml/invoicingScope",
                OTCmlId = pOTCmlId,
                Value = pIdentifier
            };
            return invoicingScope;
        }
        IInvoiceDetails IInvoice.CreateInvoiceDetails(int pTradeLength)
        {
            InvoiceDetails invoiceDetails = new InvoiceDetails
            {
                invoiceTrade = new InvoiceTrade[pTradeLength]
            };
            return invoiceDetails;
        }
        // EG 20240205 [WI640] New
        IReference IInvoice.CreatePartyReference(string pHref)
        {
            return new PartyReference(pHref);
        }
        IRebateConditions IInvoice.CreateRebateConditions()
        {
            RebateConditions rebateConditions = new RebateConditions
            {
                capConditions = new RebateCapConditions(),
                bracketConditions = new RebateBracketConditions()
            };
            return rebateConditions;
        }
        IInvoiceRateSource IInvoice.CreateInvoiceRateSource(IInformationSource pRateSource, IBusinessCenterTime pBusinessCenterTime, IRelativeDateOffset pRelativeDateOffset)
        {
            InvoiceRateSource invoiceRateSource = new InvoiceRateSource
            {
                fixingDate = (RelativeDateOffset)pRelativeDateOffset,
                fixingTime = new BusinessCenterTime
                {
                    businessCenter = new BusinessCenter(pBusinessCenterTime.BusinessCenter.Value),
                    hourMinuteTime = new HourMinuteTime(pBusinessCenterTime.HourMinuteTime.Value)
                },
                rateSource = (InformationSource)pRateSource
            };
            return invoiceRateSource;
        }
        IInformationSource IInvoice.CreateInformationSource(int pOTCmlId, string pIdentifier)
        {
            InformationSource rateSource = new InformationSource
            {
                OTCmlId = pOTCmlId
            };
            rateSource.rateSource.Value = pIdentifier;
            rateSource.rateSource.informationProviderScheme = "http://www.fpml.org/coding-scheme/information-provider-2-0";
            rateSource.rateSourcePage = new RateSourcePage();
            return rateSource;
        }

        IInvoiceTradeSort IInvoice.CreateInvoiceTradeSort()
        {
            return new InvoiceTradeSort();
        }
        IInvoiceTradeSortKeys IInvoice.CreateInvoiceTradeSortKeys(int pDim)
        {
            InvoiceTradeSortKeys ret = new InvoiceTradeSortKeys
            {
                key = new InvoiceTradeSortKey[pDim]
            };
            for (int i = 0; i < ArrFunc.Count(ret.key); i++)
                ret.key[i] = new InvoiceTradeSortKey();
            return ret;
        }
        IInvoiceTradeSortGroups IInvoice.CreateInvoiceTradeSortGroups(int pDim)
        {
            InvoiceTradeSortGroups ret = new InvoiceTradeSortGroups
            {
                group = new InvoiceTradeSortGroup[pDim]
            };
            for (int i = 0; i < ArrFunc.Count(ret.group); i++)
                ret.group[i] = new InvoiceTradeSortGroup();
            return ret;
        }
        ISchemeId IInvoice.CreateInvoiceTradeSortKey()
        {
            return new InvoiceTradeSortKey();
        }
        IInvoiceTradeSortGroup IInvoice.CreateInvoiceTradeSortGroup()
        {
            return new InvoiceTradeSortGroup();
        }
        IInvoiceTradeSortKeyValue[] IInvoice.CreateInvoiceInvoiceTradeSortKeyValue(int pDim)
        {
            InvoiceTradeSortKeyValue[] ret = new InvoiceTradeSortKeyValue[pDim];
            //
            for (int i = 0; i < ArrFunc.Count(ret); i++)
                ret[i] = new InvoiceTradeSortKeyValue();
            //
            return ret;
        }
        IInvoiceTradeSortSum IInvoice.CreateInvoiceTradeSortSum()
        {
            return new InvoiceTradeSortSum();
        }
        IReference IInvoice.CreateInvoiceTradeReference()
        {
            InvoiceTradeReference ret = new InvoiceTradeReference();
            return (IReference)ret;
        }
        IReference[] IInvoice.CreateInvoiceTradeReference(int pDim)
        {
            InvoiceTradeReference[] ret = new InvoiceTradeReference[pDim];
            //
            for (int i = 0; i < ArrFunc.Count(ret); i++)
                ret[i] = new InvoiceTradeReference();
            //
            return (IReference[])ret;
        }
        IInvoiceTax IInvoice.CreateInvoiceTax(int pNbTax)
        {
            InvoiceTax invoiceTax = new InvoiceTax { details = new TaxSchedule[pNbTax] };
            for (int i = 0; i < pNbTax; i++)
            {
                invoiceTax.details[i] = new TaxSchedule
                {
                    taxSource = new SpheresSource()
                };
            }
            return invoiceTax;
        }
        ISpheresSource IInvoice.CreateSpheresSource { get { return new SpheresSource(); } }
        ITripleInvoiceAmounts IInvoice.CreateTripleInvoiceAmounts()
        {
            return new TripleInvoiceAmounts();
        }
        #endregion IInvoice Members
        #endregion Interfaces
    }
    #endregion AdditionalInvoice
    #region AllocatedInvoice
    // EG 20091125 Add allocatedEnterAmount
    public partial class AllocatedInvoice : IAllocatedInvoice
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Money allocatedEnterAmount;
        #endregion Members
		#region Constructors
        public AllocatedInvoice() : base() {}
		#endregion Constructors
        #region Interfaces
        #region IAllocatedInvoice Members
        IAdjustedDate IAllocatedInvoice.InvoiceDate
        {
            set { this.invoiceDate = (IdentifiedDate)value; }
            get {return this.invoiceDate;}
        }
        INetInvoiceAmounts IAllocatedInvoice.AllocatedAmounts
        {
            set { this.allocatedAmounts = (NetInvoiceAmounts)value; }
            get { return this.allocatedAmounts; }
        }
        INetInvoiceAmounts IAllocatedInvoice.UnAllocatedAmounts
        {
            set { this.unAllocatedAmounts = (NetInvoiceAmounts)value; }
            get { return this.unAllocatedAmounts; }
        }
        bool IAllocatedInvoice.FxGainOrLossAmountSpecified
        {
            set { this.fxGainOrLossAmountSpecified = value; }
            get { return this.fxGainOrLossAmountSpecified; }
        }
        IMoney IAllocatedInvoice.FxGainOrLossAmount
        {
            set { this.fxGainOrLossAmount = (Money)value; }
            get { return this.fxGainOrLossAmount; }
        }
        IMoney IAllocatedInvoice.AllocatedEnterAmount
        {
            set { this.allocatedEnterAmount = (Money)value; }
            get { return this.allocatedEnterAmount; }
        }

        string IAllocatedInvoice.Id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        #endregion IAllocatedInvoice Members
        #region IInitialNetInvoiceAmounts Members
        EFS_String IInitialNetInvoiceAmounts.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string IInitialNetInvoiceAmounts.OtcmlId
        {
            set { this.otcmlId = value; }
            get { return this.otcmlId; }
        }
        int IInitialNetInvoiceAmounts.OTCmlId
        {
            get { return this.OTCmlId; }
            set { this.OTCmlId = value; }
        }
        INetInvoiceAmounts IInitialNetInvoiceAmounts.CreateNetInvoiceAmounts()
        {
            NetInvoiceAmounts netInvoiceAmounts = new NetInvoiceAmounts();
            return netInvoiceAmounts;
        }
        #endregion IInitialNetInvoiceAmounts Members
        #region NetInvoiceAmounts Members
        bool INetInvoiceAmounts.TaxSpecified
        {
            set { this.taxSpecified = value; }
            get { return this.taxSpecified; }
        }
        IInvoiceTax INetInvoiceAmounts.Tax
        {
            set { this.tax = (InvoiceTax)value; }
            get { return this.tax; }
        }
        #endregion NetInvoiceAmounts Members
        #region ITripleInvoiceAmounts Members
        IMoney ITripleInvoiceAmounts.Amount
        {
            set { this.amount = (Money)value; }
            get { return this.amount; }
        }
        bool ITripleInvoiceAmounts.IssueAmountSpecified
        {
            set { this.issueAmountSpecified = value; }
            get { return this.issueAmountSpecified; }
        }
        IMoney ITripleInvoiceAmounts.IssueAmount
        {
            set { this.issueAmount = (Money)value; }
            get { return this.issueAmount; }
        }
        bool ITripleInvoiceAmounts.AccountingAmountSpecified
        {
            set { this.accountingAmountSpecified = value; }
            get { return this.accountingAmountSpecified; }
        }
        IMoney ITripleInvoiceAmounts.AccountingAmount
        {
            set { this.accountingAmount = (Money)value; }
            get { return this.accountingAmount; }
        }
        #endregion ITripleInvoiceAmounts Members
        #endregion Interfaces
    }
    #endregion AllocatedInvoice
    #region AvailableInvoice
    public partial class AvailableInvoice : IAvailableInvoice
    {
        #region Constructors
        public AvailableInvoice(){}
        #endregion Constructors
        #region Interfaces
        #region IAvailableInvoice Members
        IAdjustedDate IAvailableInvoice.InvoiceDate
        {
            set { this.invoiceDate = (IdentifiedDate)value; }
            get { return this.invoiceDate; }

        }
        INetInvoiceAmounts IAvailableInvoice.AvailableAmounts
        {
            set { this.availableAmounts = (NetInvoiceAmounts)value; }
            get { return this.availableAmounts; }
        }
        bool IAvailableInvoice.AllocatedAccountingAmountSpecified
        {
            set { this.allocatedAccountingAmountSpecified = value; }
            get { return this.allocatedAccountingAmountSpecified; }
        }
        IMoney IAvailableInvoice.AllocatedAccountingAmount
        {
            set { this.allocatedAccountingAmount = (Money)value; }
            get { return this.allocatedAccountingAmount; }
        }
        string IAvailableInvoice.Id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        #endregion IAllocatedInvoice Members
        #region IInitialNetInvoiceAmounts Members
        EFS_String IInitialNetInvoiceAmounts.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string IInitialNetInvoiceAmounts.OtcmlId
        {
            set { this.otcmlId = value; }
            get { return this.otcmlId; }
        }
        int IInitialNetInvoiceAmounts.OTCmlId
        {
            get { return this.OTCmlId; }
            set { this.OTCmlId = value; }
        }
        #endregion IInitialNetInvoiceAmounts Members
        #region INetInvoiceAmounts Members
        bool INetInvoiceAmounts.TaxSpecified
        {
            set { this.taxSpecified = value; }
            get { return this.taxSpecified; }
        }
        IInvoiceTax INetInvoiceAmounts.Tax
        {
            set { this.tax = (InvoiceTax)value; }
            get { return this.tax; }
        }
        #endregion INetInvoiceAmounts Members
        #region ITripleInvoiceAmounts Members
        IMoney ITripleInvoiceAmounts.Amount
        {
            set { this.amount = (Money)value; }
            get { return this.amount; }
        }
        bool ITripleInvoiceAmounts.IssueAmountSpecified
        {
            set { this.issueAmountSpecified = value; }
            get { return this.issueAmountSpecified; }
        }
        IMoney ITripleInvoiceAmounts.IssueAmount
        {
            set { this.issueAmount = (Money)value; }
            get { return this.issueAmount; }
        }
        bool ITripleInvoiceAmounts.AccountingAmountSpecified
        {
            set { this.accountingAmountSpecified = value; }
            get { return this.accountingAmountSpecified; }
        }

        IMoney ITripleInvoiceAmounts.AccountingAmount
        {
            set { this.accountingAmount = (Money)value; }
            get { return this.accountingAmount; }
        }
        #endregion ITripleInvoiceAmounts Members
        #endregion Interfaces
    }
    #endregion AvailableInvoice

    #region Bracket
    public partial class Bracket : IBracket
    {
        #region Constructors
        public Bracket(){}
        public Bracket(decimal pLowValue, decimal pHighValue)
        {
            lowValue = new EFS_Decimal(pLowValue);
            lowValueSpecified = true;
            highValue = new EFS_Decimal(pHighValue);
            highValueSpecified = true;
        }
        #endregion Constructors
        #region IBracket Members
        bool IBracket.LowValueSpecified
        {
            set { this.lowValueSpecified = value; }
            get { return this.lowValueSpecified; }
        }
        EFS_Decimal IBracket.LowValue
        {
            set { this.lowValue = value; }
            get { return this.lowValue; }
        }
        bool IBracket.HighValueSpecified
        {
            set { this.highValueSpecified = value; }
            get { return this.highValueSpecified; }
        }
        EFS_Decimal IBracket.HighValue
        {
            set { this.highValue = value; }
            get { return this.highValue; }
        }
        bool IBracket.IsBracketMatch(decimal pAmount)
        {
            return (0 < Decimal.Compare(pAmount, lowValue.DecValue)) && (0 <= Decimal.Compare(highValue.DecValue,pAmount));
        }
        #endregion IBracket Members
    }
    #endregion Bracket

    #region CreditNote
    public partial class CreditNote : IProduct, ICreditNote
    {
        #region Accessors
        #endregion Accessors
        #region Constructors
        public CreditNote(): base()
        {
            initialInvoiceAmount = new InitialInvoiceAmounts();
            baseNetInvoiceAmount = new NetInvoiceAmounts();
            theoricInvoiceAmount = new InvoiceAmounts();
        }
        #endregion Constructors
        #region Interfaces
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members
        #region ICreditNote Members
        IInitialInvoiceAmounts IInvoiceSupplement.InitialInvoiceAmount
        {
            set { this.initialInvoiceAmount = (InitialInvoiceAmounts)value; }
            get { return this.initialInvoiceAmount; }
        }
        INetInvoiceAmounts IInvoiceSupplement.BaseNetInvoiceAmount
        {
            set { this.baseNetInvoiceAmount = (NetInvoiceAmounts)value; }
            get { return this.baseNetInvoiceAmount; }
        }
        IInvoiceAmounts IInvoiceSupplement.TheoricInvoiceAmount
        {
            set { this.theoricInvoiceAmount = (InvoiceAmounts)value; }
            get { return this.theoricInvoiceAmount; }
        }
        #endregion ICreditNote Members
        #region IInvoice Members
        IReference IInvoice.PayerPartyReference
        {
            set { this.payerPartyReference = (PartyOrAccountReference)value; }
            get { return this.payerPartyReference; }
        }
        IReference IInvoice.ReceiverPartyReference
        {
            set { this.receiverPartyReference = (PartyOrAccountReference)value; }
            get { return this.receiverPartyReference; }
        }
        IAdjustedDate IInvoice.InvoiceDate
        {
            set { this.invoiceDate = (IdentifiedDate)value; }
            get { return this.invoiceDate; }
        }
        IAdjustableOrRelativeAndAdjustedDate IInvoice.PaymentDate
        {
            set { this.paymentDate = (AdjustableOrRelativeAndAdjustedDate)value; }
            get { return this.paymentDate; }
        }
        bool IInvoice.GrossTurnOverAmountSpecified
        {
            set { this.grossTurnOverAmountSpecified = value; }
            get { return this.grossTurnOverAmountSpecified; }
        }
        bool IInvoice.RebateIsInExcessSpecified
        {
            set { this.rebateIsInExcessSpecified = value; }
            get { return this.rebateIsInExcessSpecified; }
        }
        EFS_Boolean IInvoice.RebateIsInExcess
        {
            set { this.rebateIsInExcess = value; }
            get { return this.rebateIsInExcess; }
        }
        bool IInvoice.RebateAmountSpecified
        {
            set { this.rebateAmountSpecified = value; }
            get { return this.rebateAmountSpecified; }
        }
        IMoney IInvoice.RebateAmount
        {
            set { this.rebateAmount = (Money)value; }
            get { return this.rebateAmount; }
        }
        bool IInvoice.NetTurnOverAccountingAmountSpecified
        {
            set { this.netTurnOverAccountingAmountSpecified = value; }
            get { return this.netTurnOverAccountingAmountSpecified; }
        }
        IMoney IInvoice.NetTurnOverAccountingAmount
        {
            set { this.netTurnOverAccountingAmount = (Money)value; }
            get { return this.netTurnOverAccountingAmount; }
        }
        bool IInvoice.NetTurnOverAccountingRateSpecified
        {
            set { this.netTurnOverAccountingRateSpecified = value; }
            get { return this.netTurnOverAccountingRateSpecified; }
        }
        EFS_Decimal IInvoice.NetTurnOverAccountingRate
        {
            set { this.netTurnOverAccountingRate = value; }
            get { return this.netTurnOverAccountingRate; }
        }
        IMoney IInvoice.GrossTurnOverAmount
        {
            set { this.grossTurnOverAmount = (Money)value; }
            get { return this.grossTurnOverAmount; }
        }
        bool IInvoice.RebateConditionsSpecified
        {
            set { this.rebateConditionsSpecified = value; }
            get { return this.rebateConditionsSpecified; }
        }
        IRebateConditions IInvoice.RebateConditions
        {
            set { this.rebateConditions = (RebateConditions)value; }
            get { return this.rebateConditions; }
        }
        IMoney IInvoice.NetTurnOverAmount
        {
            set { this.netTurnOverAmount = (Money)value; }
            get { return this.netTurnOverAmount; }
        }
        IMoney IInvoice.NetTurnOverIssueAmount
        {
            set { this.netTurnOverIssueAmount = (Money)value; }
            get { return this.netTurnOverIssueAmount; }
        }
        bool IInvoice.NetTurnOverIssueRateSpecified
        {
            set { this.netTurnOverIssueRateSpecified = value; }
            get { return this.netTurnOverIssueRateSpecified; }
        }
        EFS_Decimal IInvoice.NetTurnOverIssueRate
        {
            set { this.netTurnOverIssueRate = value; }
            get { return this.netTurnOverIssueRate; }
        }
        bool IInvoice.InvoiceRateSourceSpecified
        {
            set { this.invoiceRateSourceSpecified = value; }
            get { return this.invoiceRateSourceSpecified; }
        }
        IInvoiceRateSource IInvoice.InvoiceRateSource
        {
            set { this.invoiceRateSource = (InvoiceRateSource)value; }
            get { return this.invoiceRateSource; }
        }
        IInvoiceDetails IInvoice.InvoiceDetails
        {
            set { this.invoiceDetails = (InvoiceDetails)value; }
            get { return this.invoiceDetails; }
        }
        IInvoicingScope IInvoice.Scope
        {
            set { this.scope = (InvoicingScope)value; }
            get { return this.scope; }
        }
        EFS_Invoice IInvoice.Efs_Invoice
        {
            set { this.efs_Invoice = value; }
            get { return this.efs_Invoice; }
        }
        EFS_TheoricInvoice IInvoice.Efs_TheoricInvoice
        {
            set { this.efs_TheoricInvoice = value; }
            get { return this.efs_TheoricInvoice; }
        }
        EFS_InitialInvoice IInvoice.Efs_InitialInvoice
        {
            set { this.efs_InitialInvoice = value; }
            get { return this.efs_InitialInvoice; }
        }
        IInvoicingScope IInvoice.CreateInvoicingScope(int pOTCmlId, string pIdentifier)
        {
            InvoicingScope invoicingScope = new InvoicingScope
            {
                invoicingScopeScheme = "http://www.euro-finance-systems.fr/otcml/invoicingScope",
                OTCmlId = pOTCmlId,
                Value = pIdentifier
            };
            return invoicingScope;
        }
        IInvoiceDetails IInvoice.CreateInvoiceDetails(int pTradeLength)
        {
            InvoiceDetails invoiceDetails = new InvoiceDetails
            {
                invoiceTrade = new InvoiceTrade[pTradeLength]
            };
            return invoiceDetails;
        }
        // EG 20240205 [WI640] New
        IReference IInvoice.CreatePartyReference(string pHref)
        {
            return new PartyReference(pHref);
        }
        IRebateConditions IInvoice.CreateRebateConditions()
        {
            RebateConditions rebateConditions = new RebateConditions
            {
                capConditions = new RebateCapConditions(),
                bracketConditions = new RebateBracketConditions()
            };
            return rebateConditions;
        }
        IInvoiceRateSource IInvoice.CreateInvoiceRateSource(IInformationSource pRateSource, IBusinessCenterTime pBusinessCenterTime, IRelativeDateOffset pRelativeDateOffset)
        {
            InvoiceRateSource invoiceRateSource = new InvoiceRateSource
            {
                fixingDate = (RelativeDateOffset)pRelativeDateOffset,
                rateSource = (InformationSource)pRateSource,
                fixingTime = new BusinessCenterTime
                {
                    businessCenter = new BusinessCenter(pBusinessCenterTime.BusinessCenter.Value),
                    hourMinuteTime = new HourMinuteTime(pBusinessCenterTime.HourMinuteTime.Value)
                }
            };
            return invoiceRateSource;
        }
        IInformationSource IInvoice.CreateInformationSource(int pOTCmlId, string pIdentifier)
        {
            InformationSource rateSource = new InformationSource
            {
                OTCmlId = pOTCmlId,
                rateSourcePage = new RateSourcePage()
            };
            rateSource.rateSource.Value = pIdentifier;
            rateSource.rateSource.informationProviderScheme = "http://www.fpml.org/coding-scheme/information-provider-2-0";
            return rateSource;
        }
        IInvoiceTax IInvoice.CreateInvoiceTax(int pNbTax)
        {
            InvoiceTax invoiceTax = new InvoiceTax
            {
                details = new TaxSchedule[pNbTax]
            };
            for (int i = 0; i < pNbTax; i++)
            {
                invoiceTax.details[i] = new TaxSchedule
                {
                    taxSource = new SpheresSource()
                };
            }
            return invoiceTax;
        }
        ISpheresSource IInvoice.CreateSpheresSource { get { return new SpheresSource(); } }
        ITripleInvoiceAmounts IInvoice.CreateTripleInvoiceAmounts()
        {
            return new TripleInvoiceAmounts();
        }
        #endregion IInvoice Members
        #endregion Interfaces
    }
    #endregion AdditionalInvoice

    #region InitialInvoiceAmounts
    public partial class InitialInvoiceAmounts : IInitialInvoiceAmounts
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
		#region Constructors
        public InitialInvoiceAmounts(): base()
        {
            identifier = new EFS_String();
        }
        #endregion Constructors
        #region Interfaces
        #region IInvoiceAmounts Members
        IMoney IInvoiceAmounts.GrossTurnOverAmount
        {
            set { this.grossTurnOverAmount = (Money)value; }
            get { return this.grossTurnOverAmount; }
        }
        bool IInvoiceAmounts.RebateAmountSpecified
        {
            set { this.rebateAmountSpecified = value; }
            get { return this.rebateAmountSpecified; }
        }
        IMoney IInvoiceAmounts.RebateAmount
        {
            set { this.rebateAmount = (Money)value; }
            get { return this.rebateAmount; }
        }
        bool IInvoiceAmounts.TaxSpecified
        {
            set { this.taxSpecified = value; }
            get { return this.taxSpecified; }
        }
        IInvoiceTax IInvoiceAmounts.Tax
        {
            set { this.tax = (InvoiceTax)value; }
            get { return this.tax; }
        }
        IMoney IInvoiceAmounts.NetTurnOverAmount
        {
            set { this.netTurnOverAmount = (Money)value; }
            get { return this.netTurnOverAmount; }
        }
        IMoney IInvoiceAmounts.NetTurnOverIssueAmount
        {
            set { this.netTurnOverIssueAmount = (Money)value; }
            get { return this.netTurnOverIssueAmount; }
        }
        bool IInvoiceAmounts.NetTurnOverAccountingAmountSpecified
        {
            set { this.netTurnOverAccountingAmountSpecified = value; }
            get { return this.netTurnOverAccountingAmountSpecified; }
        }
        IMoney IInvoiceAmounts.NetTurnOverAccountingAmount
        {
            set { this.netTurnOverAccountingAmount = (Money)value; }
            get { return this.netTurnOverAccountingAmount; }
        }
        #endregion IInvoiceAmounts Members
        #region IInitialInvoiceAmounts Members
        EFS_String IInitialInvoiceAmounts.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string IInitialInvoiceAmounts.OtcmlId
        {
            set { this.otcmlId = value; }
            get { return this.otcmlId; }
        }
        int IInitialInvoiceAmounts.OTCmlId
        {
            get { return this.OTCmlId; }
            set { this.OTCmlId = value; }
        }
        #endregion IInitialInvoiceAmounts Members
        #endregion Interfaces
    }
    #endregion InitialInvoiceAmounts
    #region InitialNetInvoiceAmounts
    public partial class InitialNetInvoiceAmounts : IInitialNetInvoiceAmounts
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public InitialNetInvoiceAmounts(): base(){}
        #endregion Constructors
        #region Interfaces
        #region IInitialNetInvoiceAmounts Members
        EFS_String IInitialNetInvoiceAmounts.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        string IInitialNetInvoiceAmounts.OtcmlId
        {
            set { this.otcmlId = value; }
            get { return this.otcmlId; }
        }
        int IInitialNetInvoiceAmounts.OTCmlId
        {
            get { return this.OTCmlId; }
            set { this.OTCmlId = value; }
        }
        INetInvoiceAmounts IInitialNetInvoiceAmounts.CreateNetInvoiceAmounts()
        {
            NetInvoiceAmounts netInvoiceAmounts = new NetInvoiceAmounts();
            return netInvoiceAmounts;
        }
        #endregion IInitialNetInvoiceAmounts Members
        #region INetInvoiceAmounts Members
        bool INetInvoiceAmounts.TaxSpecified
        {
            set { this.taxSpecified = value; }
            get { return this.taxSpecified; }
        }
        IInvoiceTax INetInvoiceAmounts.Tax
        {
            set { this.tax = (InvoiceTax)value; }
            get { return this.tax; }
        }
        #endregion INetInvoiceAmounts Members

        #region ITripleInvoiceAmounts Members
        IMoney ITripleInvoiceAmounts.Amount
        {
            set { this.amount = (Money)value; }
            get { return this.amount; }
        }
        bool ITripleInvoiceAmounts.IssueAmountSpecified
        {
            set { this.issueAmountSpecified = value; }
            get { return this.issueAmountSpecified; }
        }
        IMoney ITripleInvoiceAmounts.IssueAmount
        {
            set { this.issueAmount = (Money)value; }
            get { return this.issueAmount; }
        }
        bool ITripleInvoiceAmounts.AccountingAmountSpecified
        {
            set { this.accountingAmountSpecified = value; }
            get { return this.accountingAmountSpecified; }
        }
        IMoney ITripleInvoiceAmounts.AccountingAmount
        {
            set { this.accountingAmount = (Money)value; }
            get { return this.accountingAmount; }
        }
        #endregion ITripleInvoiceAmounts Members
        #endregion Interfaces
    }
    #endregion InitialNetInvoiceAmounts

    #region Invoice
    public partial class Invoice : IProduct,IInvoice
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Invoice efs_Invoice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_InitialInvoice efs_InitialInvoice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_BaseInvoice efs_BaseInvoice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_TheoricInvoice efs_TheoricInvoice;
        #endregion Members
        #region Accessors
        #region AdjustedInvoiceDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_Date AdjustedInvoiceDate
        {
            get {return new EFS_Date(efs_Invoice.invoiceDate.Value); }
        }
        #endregion AdjustedInvoiceDate
        #region AdjustedTaxInvoiceDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_Date AdjustedTaxInvoiceDate
        {
            get
            {
                if (efs_Invoice.taxSpecified)
                    return new EFS_Date(efs_Invoice.invoiceDate.Value);
                else
                    return null;
            }
        }
        #endregion AdjustedTaxInvoiceDate
        #region InvoiceDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_EventDate InvoiceDate
        {
            get {return new EFS_EventDate(efs_Invoice.invoiceDate.DateValue, efs_Invoice.invoiceDate.DateValue);}
        }
        #endregion InvoiceDate
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual string PayerPartyReference
        {
            get {return efs_Invoice.payerPartyReference.HRef; }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual string ReceiverPartyReference
        {
            get {return efs_Invoice.receiverPartyReference.HRef; }
        }
        #endregion ReceiverPartyReference
        #endregion Accessors
        #region Constructors
        public Invoice()
		{
			payerPartyReference = new PartyOrAccountReference();
			receiverPartyReference = new PartyOrAccountReference();
            invoiceDetails = new InvoiceDetails();
            invoiceDate = new IdentifiedDate();
            paymentDate = new AdjustableOrRelativeAndAdjustedDate();
            scope = new InvoicingScope();
            rebateConditions = new RebateConditions();
            netTurnOverAmount = new Money();
            netTurnOverIssueAmount = new Money();
            netTurnOverAccountingAmount = new Money();
		}
		#endregion Constructors
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        //IProduct[] IProduct.ProductsStrategy { get { return null; } }
        #endregion IProduct Members
        #region IInvoice Members
        IReference IInvoice.PayerPartyReference
        {
            set { this.payerPartyReference = (PartyOrAccountReference)value; }
            get { return this.payerPartyReference; }
        }
        IReference IInvoice.ReceiverPartyReference
        {
            set { this.receiverPartyReference = (PartyOrAccountReference)value; }
            get { return this.receiverPartyReference; }
        }
        IAdjustedDate IInvoice.InvoiceDate
        {
            set { this.invoiceDate = (IdentifiedDate)value; }
            get { return this.invoiceDate; }
        }
        IAdjustableOrRelativeAndAdjustedDate IInvoice.PaymentDate
        {
            set { this.paymentDate = (AdjustableOrRelativeAndAdjustedDate)value; }
            get { return this.paymentDate; }
        }
        bool IInvoice.GrossTurnOverAmountSpecified
        {
            set { this.grossTurnOverAmountSpecified = value; }
            get { return this.grossTurnOverAmountSpecified; }
        }
        IMoney IInvoice.GrossTurnOverAmount
        {
            set { this.grossTurnOverAmount = (Money)value; }
            get { return this.grossTurnOverAmount; }
        }
        bool IInvoice.RebateIsInExcessSpecified
        {
            set { this.rebateIsInExcessSpecified = value; }
            get { return this.rebateIsInExcessSpecified; }
        }
        EFS_Boolean IInvoice.RebateIsInExcess
        {
            set { this.rebateIsInExcess = value; }
            get { return this.rebateIsInExcess; }
        }
        bool IInvoice.RebateAmountSpecified
        {
            set { this.rebateAmountSpecified = value; }
            get { return this.rebateAmountSpecified; }
        }
        IMoney IInvoice.RebateAmount
        {
            set { this.rebateAmount = (Money)value; }
            get { return this.rebateAmount; }
        }
        bool IInvoice.RebateConditionsSpecified
        {
            set { this.rebateConditionsSpecified = value; }
            get { return this.rebateConditionsSpecified; }
        }
        IRebateConditions IInvoice.RebateConditions
        {
            set { this.rebateConditions = (RebateConditions)value; }
            get { return this.rebateConditions; }
        }
        bool IInvoice.TaxSpecified
        {
            set { taxSpecified = value; }
            get { return taxSpecified; }
        }
        IInvoiceTax IInvoice.Tax
        {
            set { tax = (InvoiceTax)value; }
            get { return this.tax; }
        }
        IMoney IInvoice.NetTurnOverAmount
        {
            set { this.netTurnOverAmount = (Money)value; }
            get { return this.netTurnOverAmount; }
        }
        IMoney IInvoice.NetTurnOverIssueAmount
        {
            set { this.netTurnOverIssueAmount = (Money)value; }
            get { return this.netTurnOverIssueAmount; }
        }
        bool IInvoice.NetTurnOverIssueRateSpecified
        {
            set { this.netTurnOverIssueRateSpecified = value; }
            get { return this.netTurnOverIssueRateSpecified; }
        }
        EFS_Decimal IInvoice.NetTurnOverIssueRate
        {
            set { this.netTurnOverIssueRate = value; }
            get { return this.netTurnOverIssueRate; }
        }
        bool IInvoice.IssueRateIsReverseSpecified
        {
            set { this.issueRateIsReverseSpecified = value; }
            get { return this.issueRateIsReverseSpecified; }
        }
        EFS_Boolean IInvoice.IssueRateIsReverse
        {
            set { this.issueRateIsReverse = value; }
            get { return this.issueRateIsReverse; }
        }
        bool IInvoice.IssueRateReadSpecified
        {
            set { this.issueRateReadSpecified = value; }
            get { return this.issueRateReadSpecified; }
        }
        EFS_Decimal IInvoice.IssueRateRead
        {
            set { this.issueRateRead = value; }
            get { return this.issueRateRead; }
        }
        bool IInvoice.InvoiceRateSourceSpecified
        {
            set { this.invoiceRateSourceSpecified = value; }
            get { return this.invoiceRateSourceSpecified; }
        }
        IInvoiceRateSource IInvoice.InvoiceRateSource
        {
            set { this.invoiceRateSource = (InvoiceRateSource)value; }
            get { return this.invoiceRateSource; }
        }
        bool IInvoice.NetTurnOverAccountingAmountSpecified
        {
            set { this.netTurnOverAccountingAmountSpecified = value; }
            get { return this.netTurnOverAccountingAmountSpecified; }
        }
        IMoney IInvoice.NetTurnOverAccountingAmount
        {
            set { this.netTurnOverAccountingAmount = (Money)value; }
            get { return this.netTurnOverAccountingAmount; }
        }
        bool IInvoice.NetTurnOverAccountingRateSpecified
        {
            set { this.netTurnOverAccountingRateSpecified = value; }
            get { return this.netTurnOverAccountingRateSpecified; }
        }
        EFS_Decimal IInvoice.NetTurnOverAccountingRate
        {
            set { this.netTurnOverAccountingRate = value; }
            get { return this.netTurnOverAccountingRate; }
        }
        bool IInvoice.AccountingRateIsReverseSpecified 
        {
            set { this.accountingRateIsReverseSpecified = value; }
            get { return this.accountingRateIsReverseSpecified; }
        }
        EFS_Boolean IInvoice.AccountingRateIsReverse
        {
            set { this.accountingRateIsReverse = value; }
            get { return this.accountingRateIsReverse; }
        }
        bool IInvoice.AccountingRateReadSpecified
        {
            set { this.accountingRateReadSpecified = value; }
            get { return this.accountingRateReadSpecified; }
        }
        EFS_Decimal IInvoice.AccountingRateRead
        {
            set { this.accountingRateRead = value; }
            get { return this.accountingRateRead; }
        }

        IInvoiceDetails IInvoice.InvoiceDetails
        {
            set { this.invoiceDetails = (InvoiceDetails)value; }
            get { return this.invoiceDetails; }
        }
        IInvoicingScope IInvoice.Scope
        {
            set { this.scope = (InvoicingScope)value; }
            get { return this.scope; }
        }
        EFS_Invoice IInvoice.Efs_Invoice
        {
            set { this.efs_Invoice = value; }
            get { return this.efs_Invoice; }
        }
        EFS_InitialInvoice IInvoice.Efs_InitialInvoice
        {
            set { this.efs_InitialInvoice = value; }
            get { return this.efs_InitialInvoice; }
        }
        EFS_BaseInvoice IInvoice.Efs_BaseInvoice
        {
            set { this.efs_BaseInvoice = value; }
            get { return this.efs_BaseInvoice; }
        }
        EFS_TheoricInvoice IInvoice.Efs_TheoricInvoice
        {
            set { this.efs_TheoricInvoice = value; }
            get { return this.efs_TheoricInvoice; }
        }
        IInvoicingScope IInvoice.CreateInvoicingScope(int pOTCmlId, string pIdentifier) 
        {
            InvoicingScope invoicingScope = new InvoicingScope
            {
                invoicingScopeScheme = "http://www.euro-finance-systems.fr/otcml/invoicingScope",
                OTCmlId = pOTCmlId,
                Value = pIdentifier
            };
            return invoicingScope;
        }
        IInvoiceDetails IInvoice.CreateInvoiceDetails(int pTradeLength)
        {
            InvoiceDetails invoiceDetails = new InvoiceDetails
            {
                invoiceTrade = new InvoiceTrade[pTradeLength]
            };
            return invoiceDetails;
        }
        // EG 20240205 [WI640] New
        IReference IInvoice.CreatePartyReference(string pHref)
        {
            return new PartyReference(pHref);
        }
        IRebateConditions IInvoice.CreateRebateConditions()
        {
            RebateConditions rebateConditions = new RebateConditions
            {
                capConditions = new RebateCapConditions(),
                bracketConditions = new RebateBracketConditions()
            };
            return rebateConditions;
        }
        IInvoiceRateSource IInvoice.CreateInvoiceRateSource(IInformationSource pRateSource,IBusinessCenterTime pBusinessCenterTime, IRelativeDateOffset pRelativeDateOffset)
        {
            InvoiceRateSource invoiceRateSource = new InvoiceRateSource
            {
                fixingDate = (RelativeDateOffset)pRelativeDateOffset,
                fixingTime = new BusinessCenterTime
                {
                    businessCenter = new BusinessCenter(pBusinessCenterTime.BusinessCenter.Value),
                    hourMinuteTime = new HourMinuteTime(pBusinessCenterTime.HourMinuteTime.Value)
                },
                rateSource = (InformationSource)pRateSource
            };
            return invoiceRateSource;
        }
        IInformationSource IInvoice.CreateInformationSource(int pOTCmlId, string pIdentifier)
        {
            InformationSource rateSource = new InformationSource
            {
                OTCmlId = pOTCmlId
            };
            rateSource.rateSource.Value = pIdentifier;
            rateSource.rateSource.informationProviderScheme = "http://www.fpml.org/coding-scheme/information-provider-2-0";
            rateSource.rateSourcePage = new RateSourcePage();
            return rateSource;
        }
        IInvoiceTax IInvoice.CreateInvoiceTax(int pNbTax)
        {
            InvoiceTax invoiceTax = new InvoiceTax
            {
                details = new TaxSchedule[pNbTax]
            };
            for (int i = 0; i < pNbTax; i++)
            {
                invoiceTax.details[i] = new TaxSchedule
                {
                    taxSource = new SpheresSource()
                };
            }
            return invoiceTax;
        }
        ISpheresSource IInvoice.CreateSpheresSource { get { return new SpheresSource(); } }
        IInvoiceTradeSort IInvoice.CreateInvoiceTradeSort()
        {
            return new InvoiceTradeSort();
        }
        IInvoiceTradeSortKeys IInvoice.CreateInvoiceTradeSortKeys(int pDim)
        {
            InvoiceTradeSortKeys ret = new InvoiceTradeSortKeys
            {
                key = new InvoiceTradeSortKey[pDim]
            };
            for (int i = 0; i < ArrFunc.Count(ret.key); i++)
                ret.key[i] = new InvoiceTradeSortKey();
            return ret;
        }
        IInvoiceTradeSortGroups IInvoice.CreateInvoiceTradeSortGroups(int pDim)
        {
            InvoiceTradeSortGroups ret = new InvoiceTradeSortGroups
            {
                group = new InvoiceTradeSortGroup[pDim]
            };
            for (int i = 0; i < ArrFunc.Count(ret.group); i++)
                ret.group[i] = new InvoiceTradeSortGroup();
            return ret;
        }

        ISchemeId IInvoice.CreateInvoiceTradeSortKey()
        {
            return new InvoiceTradeSortKey();
        }
        IInvoiceTradeSortGroup IInvoice.CreateInvoiceTradeSortGroup()
        {
            return new InvoiceTradeSortGroup();
        }
        IInvoiceTradeSortKeyValue[] IInvoice.CreateInvoiceInvoiceTradeSortKeyValue(int pDim)
        {
            InvoiceTradeSortKeyValue[] ret = new InvoiceTradeSortKeyValue[pDim];
            //
            for (int i = 0; i < ArrFunc.Count(ret); i++)
                ret[i] = new InvoiceTradeSortKeyValue();
            //
            return ret;
        }
        IInvoiceTradeSortSum IInvoice.CreateInvoiceTradeSortSum()
        {
            return new InvoiceTradeSortSum();
        }
        IReference IInvoice.CreateInvoiceTradeReference()
        {
            InvoiceTradeReference ret = new InvoiceTradeReference();
            return (IReference)ret;
        }
        IReference[] IInvoice.CreateInvoiceTradeReference(int pDim)
        {
            InvoiceTradeReference[] ret = new InvoiceTradeReference[pDim];
            //
            for (int i = 0; i < ArrFunc.Count(ret); i++)
                ret[i] = new InvoiceTradeReference();
            //
            return (IReference[])ret;
        }
        ITripleInvoiceAmounts IInvoice.CreateTripleInvoiceAmounts() 
        { 
            return new TripleInvoiceAmounts();
        }
        #endregion IInvoice Members
    }
    #endregion Invoice
    #region InvoiceAmounts
    public partial class InvoiceAmounts : IInvoiceAmounts
    {
		#region Constructors
        public InvoiceAmounts()
		{
            grossTurnOverAmount = new Money();
            rebateAmount = new Money();
            netTurnOverAmount = new Money();
            netTurnOverAccountingAmount = new Money();
            netTurnOverIssueAmount = new Money();
            tax = new InvoiceTax();
        }
		#endregion Constructors
        #region Interfaces
        #region IInvoiceAmounts Members
        IMoney IInvoiceAmounts.GrossTurnOverAmount
        {
            set { this.grossTurnOverAmount = (Money)value; }
            get { return this.grossTurnOverAmount; }
        }
        bool IInvoiceAmounts.RebateAmountSpecified
        {
            set { this.rebateAmountSpecified = value; }
            get { return this.rebateAmountSpecified; }
        }
        IMoney IInvoiceAmounts.RebateAmount
        {
            set { this.rebateAmount = (Money)value; }
            get { return this.rebateAmount; }
        }
        bool IInvoiceAmounts.TaxSpecified
        {
            set { this.taxSpecified = value; }
            get { return this.taxSpecified; }
        }
        IInvoiceTax IInvoiceAmounts.Tax
        {
            set { this.tax = (InvoiceTax)value; }
            get { return this.tax; }
        }
        IMoney IInvoiceAmounts.NetTurnOverAmount
        {
            set { this.netTurnOverAmount = (Money)value; }
            get { return this.netTurnOverAmount; }
        }
        IMoney IInvoiceAmounts.NetTurnOverIssueAmount
        {
            set { this.netTurnOverIssueAmount = (Money)value; }
            get { return this.netTurnOverIssueAmount; }
        }
        bool IInvoiceAmounts.NetTurnOverAccountingAmountSpecified
        {
            set { this.netTurnOverAccountingAmountSpecified = value; }
            get { return this.netTurnOverAccountingAmountSpecified; }
        }
        IMoney IInvoiceAmounts.NetTurnOverAccountingAmount
        {
            set { this.netTurnOverAccountingAmount = (Money)value; }
            get { return this.netTurnOverAccountingAmount; }
        }
        IMoney IInvoiceAmounts.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
        #endregion IInvoiceAmounts Members
        #endregion Interfaces
    }
    #endregion InvoiceAmounts
    #region InvoiceDetails
    // EG 20140702 Upd 
    public partial class InvoiceDetails : IInvoiceDetails
    {
        #region Indexors
        public IInvoiceTrade this[int pIndex]
        {
            get { return invoiceTrade[pIndex]; }
            set { invoiceTrade[pIndex] = (InvoiceTrade)value; }
        }
        #endregion Indexors
        #region IInvoiceDetails Members
        IInvoiceTrade[] IInvoiceDetails.InvoiceTrade
        {
            set { this.invoiceTrade = (InvoiceTrade[])value; }
            get { return this.invoiceTrade; }
        }
        IInvoiceTradeSort IInvoiceDetails.InvoiceTradeSort
        {
            set { this.invoiceTradeSort = (InvoiceTradeSort)value; }
            get { return this.invoiceTradeSort; }
        }
        bool IInvoiceDetails.InvoiceTradeSortSpecified
        {
            set { this.invoiceTradeSortSpecified = value; }
            get { return this.invoiceTradeSortSpecified; }
        }
        #endregion IInvoiceDetails Members
        #region Methode

        // 20090909 FI [Add Asset on InvoiceTrade] ajout du paramètre pDataDocument
        //20091021 FI [add sales in invoice] add parameter pSalesLength
        //EG 20100203 Gestion des stratégies (intégration du produit de référence)
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20220324 [XXXXX] Alimentation des données du contrat sur CommoditySpot
        IInvoiceTrade IInvoiceDetails.CreateInvoiceTrade(string pCS, string pIdentifier, int pTradeOTCmlId,
            DateTime pTradeDate, DateTime pInDate, DateTime pOutDate, string idC, TradeSideEnum pSide, string pCounterpartyReference,
            string pInstrIdentifier, int pInstrOTCmlId, int pEventLength, int pTraderLength, int pSalesLength, DataDocumentContainer pDataDocument)
        {
            // productContainer = produit de référence s'il existe ou produit de base (CurrentTrade.product)
            ProductContainerBase productContainer = pDataDocument.CurrentProduct.MainProduct ;

            InvoiceTrade invoiceTrade = new InvoiceTrade
            {
                identifier = new EFS_String(pIdentifier),
                tradeDate = new TradeDate
                {
                    DateValue = pTradeDate
                },
                OTCmlId = pTradeOTCmlId,
                inDate = new EFS_Date
                {
                    DateValue = pInDate
                },
                outDate = new EFS_Date
                {
                    DateValue = pOutDate
                },

                currency = new Currency(idC),
                side = pSide,
                counterpartyPartyReference = new PartyOrAccountReference(pCounterpartyReference)
            };

            //FI 20091223 [16471] Alimentation de periodNumberOfDays
            pDataDocument.GetStartAndEndDates(pCS, false, out DateTime dtStart, out DateTime dtEnd);
            invoiceTrade.periodNumberOfDaysSpecified = DtFunc.IsDateTimeFilled(dtStart) && DtFunc.IsDateTimeFilled(dtEnd);
            if (invoiceTrade.periodNumberOfDaysSpecified)
            {
                IInterval interval = pDataDocument.CurrentProduct.ProductBase.CreateInterval(PeriodEnum.T, 1);
                EFS_DayCountFraction dcf = new EFS_DayCountFraction(dtStart, dtEnd, DayCountFractionEnum.ACT360, interval);
                invoiceTrade.periodNumberOfDays = new EFS_Integer(dcf.TotalNumberOfCalculatedDays);
            }
            //
            //FI 20091223 [16471] Alimentation de notionalAmount
            // EG 20180307 [23769] Gestion dbTransaction
            IMoney notionalAmount = productContainer.GetMainCurrencyAmount(pCS, null);
            invoiceTrade.notionalAmountSpecified = (null != notionalAmount);
            if (invoiceTrade.notionalAmountSpecified)
                invoiceTrade.notionalAmount = new Money(notionalAmount.Amount.DecValue, notionalAmount.Currency); ;
            //
            invoiceTrade.instrument = new ProductType(pInstrIdentifier, pInstrOTCmlId);
            //
            invoiceTrade.invoiceFees = new InvoiceFees
            {
                invoiceFee = new InvoiceFee[pEventLength]
            };
            //
            #region traders
            invoiceTrade.tradersSpecified = (0 < pTraderLength);
            if (invoiceTrade.tradersSpecified)
                invoiceTrade.traders = new Trader[pTraderLength];
            #endregion
            //
            //20091021 FI [add sales in invoice] add Sales
            #region sales
            invoiceTrade.salesSpecified = (0 < pSalesLength);
            if (invoiceTrade.salesSpecified)
                invoiceTrade.sales = new Trader[pSalesLength];
            #endregion
            //
            //FI 20091223 [16471] Alimentation de shortAsset pour les titres et les swaptions
            if (productContainer.IsSecurityTransaction)
            {
                #region asset si IsSecurityTransaction
                ISecurityAsset[] securityAsset = productContainer.GetSecurityAsset();
                invoiceTrade.assetSpecified = ArrFunc.IsFilled(securityAsset);
                if (invoiceTrade.assetSpecified)
                {
                    invoiceTrade.asset = new ShortAsset[ArrFunc.Count(securityAsset)];
                    for (int i = 0; i < ArrFunc.Count(securityAsset); i++)
                    {
                        //Rechargement du ISecurityAsset avec les données en base pour le titre
                        ISecurityAsset securityAsset2 = Tools.LoadSecurityAsset(pCS, null, securityAsset[i].OTCmlId);
                        if (null == securityAsset2)
                        {
                            string identifier = securityAsset[i].OTCmlId.ToString();
                            if (securityAsset[i].SecurityNameSpecified)
                                identifier = securityAsset[i].SecurityName.Value;
                            throw new Exception(StrFunc.AppendFormat("security Asset {0} not found", identifier));
                        }
                        //
                        IDebtSecurity debtSecurity = securityAsset2.DebtSecurity;
                        IProductBase productBase = (IProductBase)debtSecurity;
                        //
                        invoiceTrade.asset[i] = new ShortAsset();
                        //
                        //Alimentation de shortAsset.instrument avec l'instrument du titre
                        ISpheresIdScheme instrSheme = (ISpheresIdScheme)productBase.ProductType;
                        invoiceTrade.asset[i].instrumentSpecified = true;
                        invoiceTrade.asset[i].instrument = new ProductType(instrSheme.Value, instrSheme.OTCmlId);
                        //
                        //Alimentation de shortAsset.Description avec l'identifier du titre
                        invoiceTrade.asset[i].descriptionSpecified = true;
                        if (invoiceTrade.asset[i].descriptionSpecified)
                            invoiceTrade.asset[i].description = new EFS_String(securityAsset2.SecurityId.Value);
                        //
                        //Alimentation de shortAsset.instrumentId avec les codes (cedel , isin , etc,... existants sur le titre)
                        invoiceTrade.asset[i].instrumentId = new InstrumentId[ArrFunc.Count(debtSecurity.Security.InstrumentId)];
                        for (int j = 0; j < ArrFunc.Count(debtSecurity.Security.InstrumentId); j++)
                        {
                            invoiceTrade.asset[i].instrumentId[j] = new InstrumentId
                            {
                                instrumentIdScheme = debtSecurity.Security.InstrumentId[j].Scheme,
                                Value = debtSecurity.Security.InstrumentId[j].Value
                            };
                        }
                        //
                        //Alimentation de shortAsset.notionalAmount si Repo
                        if (productContainer.IsRepo)
                        {
                            IRepo repo = (IRepo)productContainer.Product;
                            IMoney assetNotionalAmount = null;
                            for (int j = 0; j < ArrFunc.Count(repo.SpotLeg); j++)
                            {
                                if (securityAsset[i].OTCmlId == repo.SpotLeg[j].DebtSecurityTransaction.SecurityAssetOTCmlId)
                                    assetNotionalAmount = repo.SpotLeg[j].DebtSecurityTransaction.Quantity.NotionalAmount;
                            }
                            if (null == assetNotionalAmount)
                            {
                                for (int j = 0; j < ArrFunc.Count(repo.ForwardLeg); j++)
                                {
                                    if (securityAsset[i].OTCmlId == repo.ForwardLeg[j].DebtSecurityTransaction.SecurityAssetOTCmlId)
                                        assetNotionalAmount = repo.ForwardLeg[j].DebtSecurityTransaction.Quantity.NotionalAmount;
                                }
                            }
                            invoiceTrade.asset[i].notionalAmountSpecified = (null != assetNotionalAmount);
                            if (invoiceTrade.asset[i].notionalAmountSpecified)
                                invoiceTrade.asset[i].notionalAmount = new Money(assetNotionalAmount.Amount.DecValue, assetNotionalAmount.Currency);
                        }
                        //Alimentation de shortAsset.periodNumberOfDaysSpecified
                        invoiceTrade.asset[i].periodNumberOfDaysSpecified = false;
                    }
                }
                #endregion
            }
            else if (productContainer.IsSwaption)
            {
                #region asset si Swaption
                invoiceTrade.assetSpecified = true;
                invoiceTrade.asset = new ShortAsset[1] { new ShortAsset() };

                //Alimentation de shortAsset.instrument avec l'instrument du swap
                ISwaption swaption = (ISwaption)productContainer.Product;
                ISpheresIdScheme instrSheme = (ISpheresIdScheme)((IProductBase)swaption.Swap).ProductType;
                invoiceTrade.asset[0].instrumentSpecified = true;
                invoiceTrade.asset[0].instrument = new ProductType(instrSheme.Value, instrSheme.OTCmlId);

                //Alimentation de shortAsset.instrumentId avec le product swap
                invoiceTrade.asset[0].instrumentId = new InstrumentId[1] { new InstrumentId() };
                string schemeProduct = EnumTools.GenerateScheme(pCS, Cst.OTCmL_SecurityIdSourceScheme, "securityIDSourceEnum", "ISDAFpMLProductSpecification", true);
                invoiceTrade.asset[0].instrumentId[0].instrumentIdScheme = schemeProduct;
                invoiceTrade.asset[0].instrumentId[0].Value = Cst.ProductSwap;

                //Alimentation de shortAsset.periodNumberOfDaysSpecified
                pDataDocument.GetStartAndEndDates(pCS, true, out DateTime dtStartSwap, out DateTime dtEndSwap);
                invoiceTrade.asset[0].periodNumberOfDaysSpecified = DtFunc.IsDateTimeFilled(dtStartSwap) && DtFunc.IsDateTimeFilled(dtEndSwap);
                if (invoiceTrade.periodNumberOfDaysSpecified)
                {
                    IInterval interval = pDataDocument.CurrentProduct.ProductBase.CreateInterval(PeriodEnum.T, 1);
                    EFS_DayCountFraction dcfSwap = new EFS_DayCountFraction(dtStartSwap, dtEndSwap, DayCountFractionEnum.ACT360, interval);
                    invoiceTrade.asset[0].periodNumberOfDays = new EFS_Integer(dcfSwap.TotalNumberOfCalculatedDays);
                }
                #endregion
            }
            else if (productContainer.IsExchangeTradedDerivative)
            {
                // 20100628 Alimentation du marché et derivativecontract 
                IExchangeTradedDerivative etd = (IExchangeTradedDerivative)productContainer.Product;
                // FI 20121004 [18172] paramètre pDbTransaction à null
                ExchangeTradedDerivativeContainer etdContainer = new ExchangeTradedDerivativeContainer(CSTools.SetCacheOn(pCS), etd, productContainer.DataDocument);
                _ = etdContainer.GetDerivativeContract(pCS, null, out SQL_DerivativeContract sqlDerivativeContract);
                if (null != sqlDerivativeContract)
                {
                    invoiceTrade.contractSpecified = true;
                    invoiceTrade.contract = new ContractRepository
                    {
                        contractCategorySpecified = true,
                        contractCategory = Cst.ContractCategory.DerivativeContract.ToString(),
                        OTCmlId = sqlDerivativeContract.Id,
                        identifier = sqlDerivativeContract.Identifier,
                        displayname = sqlDerivativeContract.DisplayName,
                        descriptionSpecified = StrFunc.IsFilled(sqlDerivativeContract.Description),
                        extllinkSpecified = StrFunc.IsFilled(sqlDerivativeContract.ExtlLink)
                    };
                    if (invoiceTrade.contract.descriptionSpecified)
                        invoiceTrade.contract.description = sqlDerivativeContract.Description;
                    if (invoiceTrade.contract.extllinkSpecified)
                        invoiceTrade.contract.extllink = sqlDerivativeContract.ExtlLink;
                }

                _ = etdContainer.GetMarket(pCS, null, out SQL_Market sqlMarket);
                if (null != sqlMarket)
                {
                    invoiceTrade.marketSpecified = true;
                    invoiceTrade.market = new CommonRepository
                    {
                        OTCmlId = sqlMarket.Id,
                        identifier = sqlMarket.Identifier,
                        displayname = sqlMarket.DisplayName,
                        descriptionSpecified = StrFunc.IsFilled(sqlMarket.Description),
                        extllinkSpecified = StrFunc.IsFilled(sqlMarket.ExtlLink)
                    };
                    if (invoiceTrade.market.descriptionSpecified)
                        invoiceTrade.market.description = sqlMarket.Description;
                    if (invoiceTrade.market.extllinkSpecified)
                        invoiceTrade.market.extllink = sqlMarket.ExtlLink;
                }
            }
            else if (productContainer.IsCommoditySpot)
            {
                CommoditySpotContainer commoditySpotContainer = new CommoditySpotContainer(CSTools.SetCacheOn(pCS), (ICommoditySpot)productContainer.Product, productContainer.DataDocument);
                if (null != commoditySpotContainer.AssetCommodity)
                {
                    // 20100628 Alimentation du marché et derivativecontract 
                    SQL_CommodityContract sqlCommodityContract = new SQL_CommodityContract(pCS, SQL_TableWithID.IDType.Id, commoditySpotContainer.AssetCommodity.IdCC.ToString(), commoditySpotContainer.AssetCommodity.IdM, SQL_Table.ScanDataDtEnabledEnum.No);
                    if (sqlCommodityContract.LoadTable())
                    {
                        invoiceTrade.contractSpecified = true;
                        invoiceTrade.contract = new ContractRepository
                        {
                            contractCategorySpecified = true,
                            contractCategory = Cst.ContractCategory.CommodityContract.ToString(),
                            OTCmlId = sqlCommodityContract.Id,
                            identifier = sqlCommodityContract.Identifier,
                            displayname = sqlCommodityContract.DisplayName,
                            descriptionSpecified = StrFunc.IsFilled(sqlCommodityContract.Description),
                            extllinkSpecified = StrFunc.IsFilled(sqlCommodityContract.ExtlLink)
                        };
                        if (invoiceTrade.contract.descriptionSpecified)
                            invoiceTrade.contract.description = sqlCommodityContract.Description;
                        if (invoiceTrade.contract.extllinkSpecified)
                            invoiceTrade.contract.extllink = sqlCommodityContract.ExtlLink;
                    }
                }
                _ = commoditySpotContainer.GetMarket(pCS, null, out SQL_Market sqlMarket);
                if (null != sqlMarket)
                {
                    invoiceTrade.marketSpecified = true;
                    invoiceTrade.market = new CommonRepository
                    {
                        OTCmlId = sqlMarket.Id,
                        identifier = sqlMarket.Identifier,
                        displayname = sqlMarket.DisplayName,
                        descriptionSpecified = StrFunc.IsFilled(sqlMarket.Description),
                        extllinkSpecified = StrFunc.IsFilled(sqlMarket.ExtlLink)
                    };
                    if (invoiceTrade.market.descriptionSpecified)
                        invoiceTrade.market.description = sqlMarket.Description;
                    if (invoiceTrade.market.extllinkSpecified)
                        invoiceTrade.market.extllink = sqlMarket.ExtlLink;
                }
            }
            //
            #region Price
            if (productContainer.IsSwap || productContainer.IsSwaption || productContainer.IsLoanDeposit || productContainer.IsSaleAndRepurchaseAgreement)
            {
                IInterestRateStream[] stream;
                if (productContainer.IsSwap)
                {
                    stream = ((ISwap)productContainer.Product).Stream;
                }
                else if (productContainer.IsSwaption)
                {
                    stream = ((ISwaption)productContainer.Product).Swap.Stream;
                }
                else if (productContainer.IsLoanDeposit)
                {
                    stream = ((ILoanDeposit)productContainer.Product).Stream;
                }
                else if (productContainer.IsSaleAndRepurchaseAgreement)
                {
                    stream = ((ISaleAndRepurchaseAgreement)productContainer.Product).CashStream;
                }
                else
                {
                    throw new NotImplementedException("Product not implemented");
                }
                //
                if (null != stream)
                {
                    InterestRateStreamsContainer streams = new InterestRateStreamsContainer(stream);
                    //
                    if (productContainer.IsSwap || productContainer.IsSwaption)
                    {
                        // Si swap ou swaption recherche du 1er taux fixe
                        ISchedule fixedRate = streams.GetFirstFixedRate(productContainer.DataDocument.DataDocument);
                        invoiceTrade.priceSpecified = (null != fixedRate);
                        if (invoiceTrade.priceSpecified)
                            invoiceTrade.price = new EFS_String(fixedRate.InitialValue.Value);
                    }
                    else if (productContainer.IsLoanDeposit || productContainer.IsSaleAndRepurchaseAgreement)
                    {
                        IInterestRateStream firstStream = streams.GetFirstStream(productContainer.DataDocument.DataDocument);
                        InterestRateStreamContainer irs = new InterestRateStreamContainer(firstStream);
                        invoiceTrade.priceSpecified = (null != irs.FloatingRateCalculation) || (null != irs.FixedRate);
                        if (invoiceTrade.priceSpecified)
                        {
                            if (null != irs.FloatingRateCalculation)
                            {
                                invoiceTrade.price = new EFS_String(irs.FloatingRateCalculation.FloatingRateIndex.Value);
                            }
                            else if (null != irs.FixedRate)
                            {
                                invoiceTrade.price = new EFS_String(irs.FixedRate.InitialValue.Value);
                            }
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("Product not implemented");
                    }
                }
            }
            else if (productContainer.IsCapFloor)
            {
                CapFloorContainer capFloor = new CapFloorContainer((ICapFloor)productContainer.Product);
                invoiceTrade.priceSpecified = capFloor.IsCap() || capFloor.IsFloor() || capFloor.IsStraddle();
                if (invoiceTrade.priceSpecified)
                {
                    InterestRateStreamContainer irs = new InterestRateStreamContainer(capFloor.CapFloor.Stream);
                    if (capFloor.IsCap() || capFloor.IsStraddle())
                        invoiceTrade.price = new EFS_String(irs.FloatingRateCalculation.CapRateSchedule[0].InitialValue.Value);
                    else if (capFloor.IsFloor())
                        invoiceTrade.price = new EFS_String(irs.FloatingRateCalculation.FloorRateSchedule[0].InitialValue.Value);
                }
            }
            else if (productContainer.IsFxSwap || productContainer.IsFxLeg)
            {
                IFxLeg fxLeg;
                if (productContainer.IsFxSwap)
                {
                    FxSwapContainer fxSwap = new FxSwapContainer((IFxSwap)productContainer.Product);
                    fxLeg = fxSwap.GetFirstLeg();
                }
                else if (productContainer.IsFxLeg)
                {
                    fxLeg = ((IFxLeg)productContainer.Product);
                }
                else
                {
                    throw new NotImplementedException("Product not implemented");
                }
                //
                invoiceTrade.priceSpecified = (null != fxLeg);
                if (invoiceTrade.priceSpecified)
                    invoiceTrade.price = new EFS_String(fxLeg.ExchangeRate.Rate.Value);
            }
            else if (productContainer.IsDebtSecurityTransaction)
            {
                //FI 20151202 [21609] voir le comportement lorsque le prix est en ParValueDecimal      
                //DebtSecurityTransactionContainer debtSecurityTransaction = new DebtSecurityTransactionContainer((IDebtSecurityTransaction)productContainer.product, productContainer.dataDocument.dataDocument);
                DebtSecurityTransactionContainer debtSecurityTransaction = new DebtSecurityTransactionContainer((IDebtSecurityTransaction)productContainer.Product, 
                    productContainer.DataDocument);

                ISecurityAsset asset = debtSecurityTransaction.GetSecurityAssetInDataDocument();
                DataDocumentContainer assetLoad = Tools.LoadDebtSecurity(pCS,null, asset.OTCmlId);
                if (null == assetLoad)
                {
                    string identifier = asset.OTCmlId.ToString();
                    if (asset.SecurityNameSpecified)
                        identifier = asset.SecurityName.Value;
                    throw new Exception(StrFunc.AppendFormat("security Asset {0} not found", identifier));
                }
                // EG 20100414 assetLoad.product.product replace assetLoad.product !!!
                DebtSecurityContainer debtSecurity = new DebtSecurityContainer((IDebtSecurity) assetLoad.CurrentProduct.Product);
                //
                if (debtSecurity.IsDiscount)
                {
                    //precompté
                    invoiceTrade.priceSpecified = (debtSecurityTransaction.DebtSecurityTransaction.Price.CleanPriceSpecified);
                    if (invoiceTrade.priceSpecified)
                        invoiceTrade.price = new EFS_String(debtSecurityTransaction.DebtSecurityTransaction.Price.CleanPrice.Value);
                }
                else
                {
                    //Titre postcompté
                    if (debtSecurity.ExistFixedRate(productContainer.DataDocument.DataDocument))
                    {
                        //Le sous jacent est émis à taux fixe => on retient le cours (le taux) de l'opération
                        invoiceTrade.priceSpecified = true;
                        if (invoiceTrade.priceSpecified)
                            invoiceTrade.price = new EFS_String(debtSecurityTransaction.GetPrice().Value);
                    }
                    if (debtSecurity.ExistFloatingRate(productContainer.DataDocument.DataDocument))
                    {
                        //Le sous jacent est émis à taux flottant et le prix est exprimé en % du nominal => on retient le cours (le taux) de l'opération
                        if (PriceQuoteUnitsTools.IsPriceInParValueDecimal(debtSecurityTransaction.DebtSecurityTransaction.Price.PriceUnits.Value))
                        {
                            invoiceTrade.priceSpecified = true;
                            if (invoiceTrade.priceSpecified)
                                invoiceTrade.price = new EFS_String(debtSecurityTransaction.GetPrice().Value);
                        }
                        //Le sous jacent est émis à taux flottant et le prix est exprimé en taux de rendement
                        else if (PriceQuoteUnitsTools.IsPriceInRate(debtSecurityTransaction.DebtSecurityTransaction.Price.PriceUnits.Value))
                        {
                            InterestRateStreamsContainer streams = new InterestRateStreamsContainer(debtSecurity.DebtSecurity.Stream);
                            IFloatingRateCalculation floatingRateCalculation = streams.GetFirstFloatingRateCalculation(assetLoad.DataDocument);
                            invoiceTrade.priceSpecified = (null != floatingRateCalculation);
                            if (invoiceTrade.priceSpecified)
                                invoiceTrade.price = new EFS_String(floatingRateCalculation.FloatingRateIndex.Value);
                        }
                    }
                }
            }
            #endregion
            //
            return invoiceTrade;
        }
        //
        void IInvoiceDetails.SetInvoiceTrade(int pIndex, IInvoiceTrade pInvoiceTrade)
        {
            string id = ((int)(pIndex + 1)).ToString();
            this[pIndex] = pInvoiceTrade;
            this[pIndex].Id = "invoice_invoiceTrade" + id;
        }
        IInvoiceTrade IInvoiceDetails.GetInvoiceTrade(int pIdT)
        {
            IInvoiceTrade invoiceTrade = null;
            foreach (IInvoiceTrade item in this.invoiceTrade)
            {
                if (pIdT == item.OTCmlId)
                {
                    invoiceTrade = item;
                    break;
                }
            }
            return invoiceTrade;
        }
        IInvoiceFee IInvoiceDetails.InvoiceFee(int pIdE)
        {
            IInvoiceFee invoiceFee = null;
            foreach (IInvoiceTrade item in invoiceTrade)
            {
                invoiceFee = item.InvoiceFee(pIdE);
                if (null != invoiceFee)
                    break;
            }
            return invoiceFee;
        }
        #endregion IInvoiceDetails Members
    }
    #endregion InvoiceDetails
    #region InvoiceFee
    public partial class InvoiceFee : IInvoiceFee, IEFS_Array
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public InvoiceFee()
		{
			feeSchedule = new InvoiceFeeSchedule();
		}
		#endregion Constructors
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #region IInvoiceFee Members
        EFS_String IInvoiceFee.FeeType
        {
            set { this.feeType = value; }
            get { return this.feeType; }
        }
        IMoney IInvoiceFee.FeeAmount
        {
            set { this.feeAmount = (Money)value; }
            get { return this.feeAmount; }
        }
        // EG 20091110
        IMoney IInvoiceFee.FeeBaseAmount
        {
            set { this.feeBaseAmount = (Money)value; }
            get { return this.feeBaseAmount; }
        }
        IMoney IInvoiceFee.FeeInitialAmount
        {
            set { this.feeInitialAmount = (Money)value; }
            get { return this.feeInitialAmount; }
        }
        bool IInvoiceFee.FeeAccountingAmountSpecified
        {
            set { this.feeAccountingAmountSpecified = value; }
            get { return this.feeAccountingAmountSpecified; }
        }
        IMoney IInvoiceFee.FeeAccountingAmount
        {
            set { this.feeAccountingAmount = (Money)value; }
            get { return this.feeAccountingAmount; }
        }
        // EG 20091110
        bool IInvoiceFee.FeeBaseAccountingAmountSpecified
        {
            set { this.feeBaseAccountingAmountSpecified = value; }
            get { return this.feeBaseAccountingAmountSpecified; }
        }
        // EG 20091110
        IMoney IInvoiceFee.FeeBaseAccountingAmount
        {
            set { this.feeBaseAccountingAmount = (Money)value; }
            get { return this.feeBaseAccountingAmount; }
        }

        bool IInvoiceFee.FeeInitialAccountingAmountSpecified
        {
            set { this.feeInitialAccountingAmountSpecified = value; }
            get { return this.feeInitialAccountingAmountSpecified; }
        }
        IMoney IInvoiceFee.FeeInitialAccountingAmount
        {
            set { this.feeInitialAccountingAmount = (Money)value; }
            get { return this.feeInitialAccountingAmount; }
        }
        EFS_Date IInvoiceFee.FeeDate
        {
            set { this.feeDate = value; }
            get { return this.feeDate; }
        }
        EFS_Integer IInvoiceFee.IdA_Pay
        {
            set { this.idA_Pay = value; }
            get { return this.idA_Pay; }
        }
        bool IInvoiceFee.IdB_PaySpecified
        {
            set { this.idB_PaySpecified = value; }
            get { return this.idB_PaySpecified; }
        }
        EFS_Integer IInvoiceFee.IdB_Pay
        {
            set { this.idB_Pay = value; }
            get { return this.idB_Pay; }
        }
        bool IInvoiceFee.FeeScheduleSpecified
        {
            set { this.feeScheduleSpecified = value; }
            get { return this.feeScheduleSpecified; }
        }
        IInvoiceFeeSchedule IInvoiceFee.FeeSchedule
        {
            set { this.feeSchedule = (InvoiceFeeSchedule)value; }
            get { return this.feeSchedule; }
        }
        /*
        bool IInvoiceFee.idFeeScheduleSpecified
        {
            set { this.idFeeScheduleSpecified = value; }
            get { return this.idFeeScheduleSpecified; }
        }

        EFS_Integer IInvoiceFee.idFeeSchedule
        {
            set { this.idFeeSchedule = value; }
            get { return this.idFeeSchedule; }
        }

        bool IInvoiceFee.feeScheduleIdentifierSpecified
        {
            set { this.feeScheduleIdentifierSpecified = value; }
            get { return this.feeScheduleIdentifierSpecified; }
        }
        EFS_String IInvoiceFee.feeScheduleIdentifier
        {
            set { this.feeScheduleIdentifier = value; }
            get { return this.feeScheduleIdentifier; }
        }
        bool IInvoiceFee.formulaDCFSpecified
        {
            set { this.formulaDCFSpecified = value; }
            get { return this.formulaDCFSpecified; }
        }
        EFS_String IInvoiceFee.formulaDCF
        {
            set { this.formulaDCF = value; }
            get { return this.formulaDCF; }
        }
        */
        string IInvoiceFee.OtcmlId
        {
            set { this.otcmlId = value; }
            get { return this.otcmlId; }
        }
        int IInvoiceFee.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        #endregion IInvoiceFee Members
    }
    #endregion InvoiceFees
    #region InvoiceFeeSchedule
    public partial class InvoiceFeeSchedule : IInvoiceFeeSchedule
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors

        #region IInvoiceFeeSchedule Members
        bool IInvoiceFeeSchedule.IdentifierSpecified
        {
            set { this.identifierSpecified = value; }
            get { return this.identifierSpecified; }
        }
        EFS_String IInvoiceFeeSchedule.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        bool IInvoiceFeeSchedule.FormulaDCFSpecified
        {
            set { this.formulaDCFSpecified = value; }
            get { return this.formulaDCFSpecified; }
        }
        EFS_String IInvoiceFeeSchedule.FormulaDCF
        {
            set { this.formulaDCF = value; }
            get { return this.formulaDCF; }
        }
        bool IInvoiceFeeSchedule.DurationSpecified
        {
            set { this.durationSpecified = value; }
            get { return this.durationSpecified; }
        }
        // EG 20091110
        EFS_String IInvoiceFeeSchedule.Duration
        {
            set { this.duration = value; }
            get { return this.duration; }
        }
        //PL 20141023
        bool IInvoiceFeeSchedule.AssessmentBasisValue1Specified
        {
            set { this.assessmentBasisValue1Specified = value; }
            get { return this.assessmentBasisValue1Specified; }
        }
        EFS_Decimal IInvoiceFeeSchedule.AssessmentBasisValue1
        {
            set { this.assessmentBasisValue1 = value; }
            get { return this.assessmentBasisValue1; }
        }
        bool IInvoiceFeeSchedule.AssessmentBasisValue2Specified
        {
            set { this.assessmentBasisValue2Specified = value; }
            get { return this.assessmentBasisValue2Specified; }
        }
        EFS_Decimal IInvoiceFeeSchedule.AssessmentBasisValue2
        {
            set { this.assessmentBasisValue2 = value; }
            get { return this.assessmentBasisValue2; }
        }
        string IInvoiceFeeSchedule.OtcmlId
        {
            set { this.otcmlId = value; }
            get { return this.otcmlId; }
        }
        int IInvoiceFeeSchedule.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }

        #endregion IInvoiceFeeSchedule Members

    }
    #endregion InvoiceFeeSchedule
    #region InvoiceFees
    public partial class InvoiceFees : IInvoiceFees
    {
        #region Indexors
        public IInvoiceFee this[int pIndex]
        {
            get { return invoiceFee[pIndex]; }
            set { invoiceFee[pIndex] = (InvoiceFee)value; }
        }
        public IInvoiceFee this[int pOTCmlId,decimal pFeeAmount]
        {
            get 
            {
                foreach (IInvoiceFee fee in invoiceFee)
                {
                    if ((pOTCmlId == fee.OTCmlId) && (pFeeAmount == fee.FeeAmount.Amount.DecValue))
                        return fee;
                }
                return null; 
            }
        }
        public IInvoiceFee this[int pOTCmlId, bool pIsOTCmlId]
        {
            get
            {
                foreach (IInvoiceFee fee in invoiceFee)
                {
                    if (pOTCmlId == fee.OTCmlId)
                        return fee;
                }
                return null;
            }
        }
        #endregion Indexors

        #region IInvoiceFees Members
        IInvoiceFee[] IInvoiceFees.InvoiceFee
        {
            set { this.invoiceFee = (InvoiceFee[])value; }
            get { return this.invoiceFee; }
        }
        // EG 20091110
        IInvoiceFee IInvoiceFees.CreateInvoiceFee(int pOTCmlId, string pFeeType, string pFeeCurrency, decimal pFeeAmount, decimal pFeeBaseAmount, decimal pFeeInitialAmount,
            DateTime pFeeDate, int pIdA_Pay, int pIdB_Pay, string pIdCAccount)
        {
            IInvoiceFee invoiceFee = new InvoiceFee();
            invoiceFee.OTCmlId = pOTCmlId;
            invoiceFee.FeeType = new EFS_String(pFeeType);
            invoiceFee.FeeDate = new EFS_Date
            {
                DateValue = pFeeDate
            };
            invoiceFee.FeeAmount = new Money(pFeeAmount, pFeeCurrency);
            invoiceFee.FeeBaseAmount = new Money(pFeeBaseAmount, pFeeCurrency);
            invoiceFee.FeeInitialAmount = new Money(pFeeInitialAmount, pFeeCurrency);

            if (pIdCAccount == pFeeCurrency)
            {
                invoiceFee.FeeAccountingAmountSpecified = true;
                invoiceFee.FeeAccountingAmount = new Money(pFeeAmount, pFeeCurrency);
                invoiceFee.FeeBaseAccountingAmountSpecified = true;
                invoiceFee.FeeBaseAccountingAmount = new Money(pFeeBaseAmount, pFeeCurrency);
                invoiceFee.FeeInitialAccountingAmountSpecified = true;
                invoiceFee.FeeInitialAccountingAmount = new Money(pFeeInitialAmount, pFeeCurrency);
            }

            invoiceFee.IdA_Pay = new EFS_Integer(pIdA_Pay);
            invoiceFee.IdB_PaySpecified = (0 < pIdB_Pay);
            if (invoiceFee.IdB_PaySpecified)
                invoiceFee.IdB_Pay = new EFS_Integer(pIdB_Pay);
            return invoiceFee;
        }
        void IInvoiceFees.SetInvoiceFee(int pIndex, IInvoiceFee pInvoiceFee)
        {
            this[pIndex] = pInvoiceFee;
        }
        IMoney IInvoiceFees.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
        IInvoiceFeeSchedule IInvoiceFees.CreateInvoiceFeeSchedule()
        {
            return new InvoiceFeeSchedule();
        }
#endregion IInvoiceFees Members
    }
    #endregion InvoiceFees
    #region InvoiceRateSource
    public partial class InvoiceRateSource : IInvoiceRateSource
    {
		#region Constructors
        public InvoiceRateSource()
		{
			rateSource = new InformationSource();
			fixingTime = new BusinessCenterTime();
		}
		#endregion Constructors

        #region IInvoiceRateSource Members
        IInformationSource IInvoiceRateSource.RateSource
        {
            set { this.rateSource = (InformationSource)value; }
            get { return this.rateSource; }
        }
        IBusinessCenterTime IInvoiceRateSource.FixingTime
        {
            set { this.fixingTime = (BusinessCenterTime)value; }
            get { return this.fixingTime; }
        }
        IRelativeDateOffset IInvoiceRateSource.FixingDate
        {
            set { this.fixingDate = (RelativeDateOffset)value; }
            get { return this.fixingDate; }
        }
        IAdjustedDate IInvoiceRateSource.AdjustedFixingDate
        {
            set { this.adjustedFixingDate = (IdentifiedDate)value; }
            get { return this.adjustedFixingDate; }
        }
        bool IInvoiceRateSource.AdjustedFixingDateSpecified
        {
            set { this.adjustedFixingDateSpecified = value; }
            get { return this.adjustedFixingDateSpecified; }
        }
        object IInvoiceRateSource.Clone()
        {
            InvoiceRateSource clone = new InvoiceRateSource
            {
                adjustedFixingDate = (IdentifiedDate)adjustedFixingDate.Clone(),
                adjustedFixingDateSpecified = adjustedFixingDateSpecified,
                fixingDate = (RelativeDateOffset)fixingDate.Clone(),
                fixingTime = (BusinessCenterTime)fixingTime.Clone(),
                rateSource = (InformationSource)rateSource.Clone()
            };
            return clone;
        }
        #endregion IInvoiceRateSource Members

    }
    #endregion InvoiceRateSource
    #region InvoiceSettlement
    // EG 20091125 Add allocatedEnterAmount 
    // EG 20150128 settlementAmount constructor
    public partial class InvoiceSettlement : IProduct, IInvoiceSettlement
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_InvoiceSettlement efs_InvoiceSettlement;
        #endregion Members
        #region Accessors
        #region AdjustedReceptionDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_Date AdjustedReceptionDate
        {
            get {return new EFS_Date(efs_InvoiceSettlement.receptionDate.Value); }
        }
        #endregion AdjustedReceptionDate
        #region ReceptionDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual EFS_EventDate ReceptionDate
        {
            get {return new EFS_EventDate(efs_InvoiceSettlement.receptionDate.DateValue, efs_InvoiceSettlement.receptionDate.DateValue); }
        }
        #endregion ReceptionDate
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual string PayerPartyReference
        {
            get {return efs_InvoiceSettlement.payerPartyReference.HRef; }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public virtual string ReceiverPartyReference
        {
            get {return efs_InvoiceSettlement.receiverPartyReference.HRef; }
        }
        #endregion ReceiverPartyReference
        #endregion Accessors
        #region Constructors
        // EG 20150128 settlementAmount 
        public InvoiceSettlement()
        {
            payerPartyReference = new PartyOrAccountReference();
            receiverPartyReference = new PartyOrAccountReference();
            accountNumber = new AccountNumber();
            bankChargesAmount = new Money();
            vatBankChargesAmount = new Money();
            unallocatedAmount = new Money();
            settlementAmount = new Money();
            fxGainOrLossAmount = new Money();
            availableInvoice = new AvailableInvoice[1] { new AvailableInvoice() };
            allocatedInvoice = new AllocatedInvoice[1] { new AllocatedInvoice() };
        }
        #endregion Constructors
        #region Indexors
        public IAllocatedInvoice this[int pIdT]
        {
            get
            {
                if (this.allocatedInvoiceSpecified)
                {
                    foreach (AllocatedInvoice item in allocatedInvoice)
                    {
                        if (item.OTCmlId == pIdT)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion Indexors
        #region Methods
        #endregion Methods
        #region Interfaces
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members
        #region IInvoiceSettlement Members
        IReference IInvoiceSettlement.PayerPartyReference
        {
            set { this.payerPartyReference = (PartyOrAccountReference)value; }
            get { return this.payerPartyReference; }
        }
        IReference IInvoiceSettlement.ReceiverPartyReference
        {
            set { this.receiverPartyReference = (PartyOrAccountReference)value; }
            get { return this.receiverPartyReference; }
        }
        IAdjustedDate IInvoiceSettlement.ReceptionDate
        {
            set { this.receptionDate = (IdentifiedDate)value; }
            get { return this.receptionDate; }
        }
        IAccountNumber IInvoiceSettlement.AccountNumber
        {
            set { this.accountNumber = (AccountNumber)value; }
            get { return this.accountNumber; }
        }
        IMoney IInvoiceSettlement.SettlementAmount
        {
            set { this.settlementAmount = (Money)value; }
            get { return this.settlementAmount; }
        }
        IMoney IInvoiceSettlement.CashAmount
        {
            set { this.cashAmount = (Money)value; }
            get { return this.cashAmount; }
        }
        bool IInvoiceSettlement.BankChargesAmountSpecified
        {
            set { this.bankChargesAmountSpecified = value; }
            get { return this.bankChargesAmountSpecified; }
        }
        IMoney IInvoiceSettlement.BankChargesAmount
        {
            set { this.bankChargesAmount = (Money)value; }
            get { return this.bankChargesAmount; }
        }
        bool IInvoiceSettlement.VatBankChargesAmountSpecified
        {
            set { this.vatBankChargesAmountSpecified = value; }
            get { return this.vatBankChargesAmountSpecified; }
        }
        IMoney IInvoiceSettlement.VatBankChargesAmount
        {
            set { this.vatBankChargesAmount = (Money)value; }
            get { return this.vatBankChargesAmount; }
        }
        IMoney IInvoiceSettlement.NetCashAmount
        {
            set { this.netCashAmount = (Money)value; }
            get { return this.netCashAmount; }
        }
        IMoney IInvoiceSettlement.UnallocatedAmount
        {
            set { this.unallocatedAmount = (Money)value; }
            get { return this.unallocatedAmount; }
        }
        bool IInvoiceSettlement.FxGainOrLossAmountSpecified
        {
            set { this.fxGainOrLossAmountSpecified = value; }
            get { return this.fxGainOrLossAmountSpecified; }
        }
        IMoney IInvoiceSettlement.FxGainOrLossAmount
        {
            set { this.fxGainOrLossAmount = (Money)value; }
            get { return this.fxGainOrLossAmount; }
        }
        bool IInvoiceSettlement.AllocatedInvoiceSpecified
        {
            set { this.allocatedInvoiceSpecified = value; }
            get { return this.allocatedInvoiceSpecified; }
        }
        IAllocatedInvoice[] IInvoiceSettlement.AllocatedInvoice
        {
            set { this.allocatedInvoice = (AllocatedInvoice[])value; }
            get { return this.allocatedInvoice; }
        }
        bool IInvoiceSettlement.AvailableInvoiceSpecified
        {
            set { this.availableInvoiceSpecified = value; }
            get { return this.availableInvoiceSpecified; }
        }
        IAvailableInvoice[] IInvoiceSettlement.AvailableInvoice
        {
            set { this.availableInvoice = (AvailableInvoice[])value; }
            get { return this.availableInvoice; }
        }
        IAvailableInvoice IInvoiceSettlement.CreateAvailableInvoice()
        {
            AvailableInvoice availableInvoice = new AvailableInvoice
            {
                identifier = new EFS_String(),
                invoiceDate = new IdentifiedDate(),
                availableAmounts = new NetInvoiceAmounts
                {
                    amount = new Money(),
                    issueAmountSpecified = true,
                    issueAmount = new Money(),
                    accountingAmountSpecified = true,
                    accountingAmount = new Money()
                },
                allocatedAccountingAmountSpecified = true,
                allocatedAccountingAmount = new Money(),
                amount = new Money(),
                issueAmountSpecified = true,
                issueAmount = new Money(),
                accountingAmountSpecified = true,
                accountingAmount = new Money()
            };
            return availableInvoice;
        }
        IAllocatedInvoice IInvoiceSettlement.CreateAllocatedInvoice()
        {
            AllocatedInvoice allocatedInvoice = new AllocatedInvoice
            {
                identifier = new EFS_String(),
                invoiceDate = new IdentifiedDate(),
                allocatedAmounts = new NetInvoiceAmounts
                {
                    amount = new Money(),
                    issueAmountSpecified = true,
                    issueAmount = new Money(),
                    accountingAmountSpecified = true,
                    accountingAmount = new Money()
                },
                amount = new Money(),
                issueAmount = new Money(),
                accountingAmount = new Money(),
                unAllocatedAmounts = new NetInvoiceAmounts
                {
                    amount = new Money(),
                    issueAmountSpecified = true,
                    issueAmount = new Money(),
                    accountingAmountSpecified = true,
                    accountingAmount = new Money()
                },
                fxGainOrLossAmount = new Money()
            };
            allocatedInvoice.fxGainOrLossAmount.amount.DecValue = 0;
            // EG 20091125 Add allocatedEnterAmount 
            allocatedInvoice.allocatedEnterAmount = new Money();
            return allocatedInvoice;
        }

        bool IInvoiceSettlement.IsInvoiceNotSelected(int pIdT)
        {
            return (false == this.allocatedInvoiceSpecified) || (null == this[pIdT]);
        }
        Type IInvoiceSettlement.TypeofAvailableInvoice { get { return new AvailableInvoice().GetType(); } }
        Type IInvoiceSettlement.TypeofAllocatedInvoice { get { return new AllocatedInvoice().GetType(); } }
        EFS_InvoiceSettlement IInvoiceSettlement.Efs_InvoiceSettlement
        {
            set { this.efs_InvoiceSettlement = value; }
            get { return this.efs_InvoiceSettlement; }
        }
        #endregion IInvoiceSettlement Members
        #endregion Interfaces
    }
    #endregion InvoiceSettlement
    #region InvoiceTax
    public partial class InvoiceTax : IInvoiceTax
    {
        #region Constructors
        public InvoiceTax()
        {
        }
        #endregion Constructors

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region IInvoiceTax Members
        ITaxSchedule[] IInvoiceTax.Details
        {
            set { this.details = (TaxSchedule[])value; }
            get { return this.details; }
        }
        decimal IInvoiceTax.GetBaseAmountForTax(string pCs, decimal pNetTurnoverAmount, string pCurrency)
        {
            int nbTaxDet = details.Length;
            decimal taxRate = 0;
            for (int i = 0; i < nbTaxDet; i++)
            {
                ITaxSchedule detail = (ITaxSchedule)details[i];
                taxRate += detail.GetRate();
            }
            EFS_Cash cash = new EFS_Cash(pCs, pNetTurnoverAmount / (1 + taxRate), pCurrency);
            return cash.AmountRounded;
        }
        decimal IInvoiceTax.GetTotalTaxAmount(string pCs, decimal pBaseAmount, string pCurrency)
        {
            int nbTaxDet = details.Length;
            decimal totalTaxAmount = 0;
            for (int i = 0; i < nbTaxDet; i++)
            {
                ITaxSchedule detail = (ITaxSchedule)details[i];
                EFS_Cash cash = new EFS_Cash(pCs, detail.GetRate() * pBaseAmount, pCurrency);
                totalTaxAmount += cash.AmountRounded;
            }
            return totalTaxAmount;
        }

        #endregion IInvoiceTax Members
        #region ITripleInvoiceAmounts Members
        IMoney ITripleInvoiceAmounts.Amount
        {
            set { amount = (Money)value; }
            get { return amount; }
        }
        bool ITripleInvoiceAmounts.IssueAmountSpecified
        {
            set { issueAmountSpecified = value; }
            get { return issueAmountSpecified; }
        }
        IMoney ITripleInvoiceAmounts.IssueAmount
        {
            set { issueAmount = (Money)value; }
            get { return issueAmount; }
        }
        bool ITripleInvoiceAmounts.AccountingAmountSpecified
        {
            set { accountingAmountSpecified = value; }
            get { return accountingAmountSpecified; }
        }
        IMoney ITripleInvoiceAmounts.AccountingAmount
        {
            set { accountingAmount = (Money)value; }
            get { return accountingAmount; }
        }
        #endregion ITripleInvoiceAmounts Members
    }
    #endregion InvoiceTax

    #region InvoiceTrade
    // EG 20171004 [23452] tradeDateTime
    public partial class InvoiceTrade : IInvoiceTrade, IEFS_Array
    {
        #region Members
        //private Product _product;
        #endregion

        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public InvoiceTrade()
        {
            invoiceFees = new InvoiceFees();
        }
        #endregion Constructors
        #region Indexors
        public IInvoiceFee this[int pIdE]
        {
            get { return invoiceFees[pIdE, true]; }
        }
        #endregion Indexors

        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #region IInvoiceTrade Members
        EFS_String IInvoiceTrade.Identifier
        {
            set { this.identifier = value; }
            get { return this.identifier; }
        }
        ISpheresIdScheme IInvoiceTrade.Instrument
        {
            set { this.instrument = (ProductType)value; }
            get { return this.instrument; }
        }
        ITradeDate IInvoiceTrade.TradeDate
        {
            set { this.tradeDate = (TradeDate)value; }
            get { return this.tradeDate; }
        }
        EFS_Date IInvoiceTrade.InDate
        {
            set { this.inDate = value; }
            get { return this.inDate; }
        }
        EFS_Date IInvoiceTrade.OutDate
        {
            set { this.outDate = value; }
            get { return this.outDate; }
        }
        ICurrency IInvoiceTrade.Currency
        {
            set { this.currency = (Currency)value; }
            get { return this.currency; }
        }
        TradeSideEnum IInvoiceTrade.Side
        {
            set { this.side = value; }
            get { return this.side; }
        }
        IReference IInvoiceTrade.CounterpartyPartyReference
        {
            set { this.counterpartyPartyReference = (PartyOrAccountReference)value; }
            get { return this.counterpartyPartyReference; }
        }
        bool IInvoiceTrade.NotionalAmountSpecified
        {
            set { this.notionalAmountSpecified = value; }
            get { return this.notionalAmountSpecified; }
        }
        IMoney IInvoiceTrade.NotionalAmount
        {
            set { this.notionalAmount = (Money)value; }
            get { return this.notionalAmount; }
        }
        IInvoiceFees IInvoiceTrade.InvoiceFees
        {
            set { this.invoiceFees = (InvoiceFees)value; }
            get { return this.invoiceFees; }
        }
        bool IInvoiceTrade.TradersSpecified
        {
            set { this.tradersSpecified = value; }
            get { return this.tradersSpecified; }
        }
        ITrader[] IInvoiceTrade.Traders
        {
            set { this.traders = (Trader[])value; }
            get { return this.traders; }
        }
        //20091021 FI [add sales in invoice] add sales
        bool IInvoiceTrade.SalesSpecified
        {
            set { this.salesSpecified = value; }
            get { return this.salesSpecified; }
        }
        ITrader[] IInvoiceTrade.Sales
        {
            set { this.sales = (Trader[])value; }
            get { return this.sales; }
        }
        string IInvoiceTrade.OtcmlId
        {
            set { this.otcmlId = value; }
            get { return this.otcmlId; }
        }
        int IInvoiceTrade.OTCmlId
        {
            get { return this.OTCmlId; }
        }
        string IInvoiceTrade.Id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        IInvoiceFee IInvoiceTrade.InvoiceFee(int pIdE)
        {
            return this[pIdE];
        }
        ITrader IInvoiceTrade.CreateTrader()
        {
            return new Trader();
        }
        bool IInvoiceTrade.AssetSpecified
        {
            set { this.assetSpecified = value; }
            get { return this.assetSpecified; }
        }
        IShortAsset[] IInvoiceTrade.Asset
        {
            set { this.asset = (ShortAsset[])value; }
            get { return this.asset; }
        }
        EFS_Integer IInvoiceTrade.PeriodNumberOfDays
        {
            set { this.periodNumberOfDays = value; }
            get { return this.periodNumberOfDays; }
        }
        bool IInvoiceTrade.PeriodNumberOfDaysSpecified
        {
            set { this.periodNumberOfDaysSpecified = value; }
            get { return this.periodNumberOfDaysSpecified; }
        }
        bool IInvoiceTrade.MarketSpecified
        {
            set { this.marketSpecified = value; }
            get { return this.marketSpecified; }
        }
        ICommonRepository IInvoiceTrade.Market
        {
            set { this.market = (CommonRepository)value; }
            get { return this.market; }
        }
        bool IInvoiceTrade.ContractSpecified
        {
            set { this.contractSpecified = value; }
            get { return this.contractSpecified; }
        }
        IContractRepository IInvoiceTrade.Contract
        {
            set { this.contract = (ContractRepository)value; }
            get { return this.contract; }
        }
        #endregion IInvoiceTrade Members
    }
    #endregion InvoiceTrade

    #region InvoiceTradeReference
    public partial class InvoiceTradeReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
    }
    #endregion

    #region InvoiceTradeSort
    public partial class InvoiceTradeSort : IInvoiceTradeSort
    {
        #region constructor
        public InvoiceTradeSort()
        {
            this.groups = new InvoiceTradeSortGroups();
            this.keys = new InvoiceTradeSortKeys();
        }
        #endregion

        #region IInvoiceTradeSort Membres
        IInvoiceTradeSortKeys IInvoiceTradeSort.Keys
        {
            get { return (IInvoiceTradeSortKeys)this.keys; }
            set { this.keys = (InvoiceTradeSortKeys)value; }
        }
        IInvoiceTradeSortGroups IInvoiceTradeSort.Groups
        {
            get { return (IInvoiceTradeSortGroups)this.groups; }
            set { this.groups = (InvoiceTradeSortGroups)value; }
        }
        #endregion
    }
    #endregion
    //


    #region InvoiceTradeSortGroup
    public partial class InvoiceTradeSortGroup : IInvoiceTradeSortGroup
    {
        #region constructor
        public InvoiceTradeSortGroup()
        {
        }
        #endregion

        #region IInvoiceTradeSortGroup Membres
        IInvoiceTradeSortKeyValue[] IInvoiceTradeSortGroup.KeyValue
        {
            get { return (IInvoiceTradeSortKeyValue[])this.keyValue; }
            set { this.keyValue = (InvoiceTradeSortKeyValue[])value; }
        }
        IReference[] IInvoiceTradeSortGroup.InvoiceTradeReference
        {
            get { return (IReference[])this.invoiceTradeReference; }
            set { this.invoiceTradeReference = (InvoiceTradeReference[])value; }
        }
        IInvoiceTradeSortSum IInvoiceTradeSortGroup.Sum
        {
            get { return (IInvoiceTradeSortSum)this.sum; }
            set { this.sum = (InvoiceTradeSortSum)value; }
        }
        #endregion
    }
    #endregion InvoiceTradeSortGroup
    //
    #region InvoiceTradeSortGroups
    public partial class InvoiceTradeSortGroups : IInvoiceTradeSortGroups
    {
        #region constructor
        public InvoiceTradeSortGroups()
        {
        }
        #endregion
        #region IInvoiceTradeSortGroups Membres
        IInvoiceTradeSortGroup[] IInvoiceTradeSortGroups.Group
        {
            get { return (IInvoiceTradeSortGroup[])this.group; }
            set { this.group = (InvoiceTradeSortGroup[])value; }
        }
        #endregion
    }
    #endregion

    #region InvoiceTradeSortSum
    public partial class InvoiceTradeSortSum : IInvoiceTradeSortSum
    {
        #region IInvoiceTradeSortSum Membres
        IMoney IInvoiceTradeSortSum.FeeAmount
        {
            get { return this.feeAmount; }
            set { this.feeAmount = (Money)value; }
        }
        //
        IMoney IInvoiceTradeSortSum.FeeAccountingAmount
        {
            get { return this.feeAccountingAmount; }
            set { this.feeAccountingAmount = (Money)value; }
        }
        //
        IMoney IInvoiceTradeSortSum.FeeInitialAmount
        {
            get { return this.feeInitialAmount; }
            set { this.feeInitialAmount = (Money)value; }
        }
        //
        IMoney IInvoiceTradeSortSum.FeeInitialAccountingAmount
        {
            get { return this.feeInitialAccountingAmount; }
            set { this.feeInitialAccountingAmount = (Money)value; }
        }
        #endregion
    }
    #endregion

    //
    #region InvoiceTradeSortKey
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
    public partial class InvoiceTradeSortKey : ISchemeId
    {
        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set { efs_id = new EFS_Id(value); }
            get
            {
                if (efs_id == null)
                    return null;
                else
                    return efs_id.Value;
            }
        }
        #endregion Accessors
        #region Constructors
        public InvoiceTradeSortKey()
        {
            sortScheme = "http://www.efs.org/2007/InvoicingSortEnum";
        }
        #endregion Constructors
        #region ISchemeId Members
        string ISchemeId.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
        string ISchemeId.Source
        {
            set;
            get;
        }
        #endregion ISchemeId Members
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.sortScheme = value; }
            get { return this.sortScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion InvoiceTradeSortKey
    //
    #region InvoiceTradeSortKeys
    public partial class InvoiceTradeSortKeys : IInvoiceTradeSortKeys
    {
        #region constructor
        public InvoiceTradeSortKeys()
        {
        }
        #endregion
        #region IInvoiceTradeSortKeys Membres
        ISchemeId[] IInvoiceTradeSortKeys.Key
        {
            get { return (ISchemeId[])this.key; }
            set { key = (InvoiceTradeSortKey[])value; }
        }
        #endregion
    }
    #endregion InvoiceTradeSortKeys
    //
    #region InvoiceTradeSortKeyValue
    public partial class InvoiceTradeSortKeyValue : IInvoiceTradeSortKeyValue
    {
        #region InvoiceTradeSortKeyValue
        public InvoiceTradeSortKeyValue()
        {
        }
        #endregion

        #region IInvoiceTradeSortKeyValue Membres
        String IInvoiceTradeSortKeyValue.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
        }
        #endregion

        #region IReference Membres
        String IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion
    }
    #endregion InvoiceTradeSortKeyValue
    //
    #region InvoicingScope
    public partial class InvoicingScope : IInvoicingScope
    {
		#region Accessors
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public int OTCmlId
		{
			get 
			{ 
				if (StrFunc.IsFilled(otcmlId))
					return Convert.ToInt32(otcmlId);
				return 0; 
			}
			set { otcmlId = value.ToString(); }
		}
		#endregion Accessors
		#region Constructors
        public InvoicingScope()
		{
			invoicingScopeScheme = "http://www.euro-finance-systems.fr/otcml/invoicingscope";
		}
		#endregion Constructors

        #region IInvoicingScope Members
        string IInvoicingScope.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        int IInvoicingScope.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        #endregion IInvoicingScope Members
        #region IScheme Members
        string IScheme.Scheme
        {
            get { return this.invoicingScopeScheme; }
            set { this.invoicingScopeScheme = value; }
        }
        string IScheme.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
        }
        #endregion IScheme Members

    }
    #endregion InvoicingScope

    #region NetInvoiceAmounts
    public partial class NetInvoiceAmounts : INetInvoiceAmounts
    {
		#region Constructors
        public NetInvoiceAmounts()
        {
            amount = new Money();
            issueAmount = new Money();
            accountingAmount = new Money();
            tax = new InvoiceTax();
        }
        #endregion Constructors

        #region INetInvoiceAmounts Members
        bool INetInvoiceAmounts.TaxSpecified
        {
            set { this.taxSpecified = value; }
            get { return this.taxSpecified; }
        }
        IInvoiceTax INetInvoiceAmounts.Tax
        {
            set { this.tax = (InvoiceTax)value; }
            get { return this.tax; }
        }
        #endregion INetInvoiceAmounts Members
        #region ITripleInvoiceAmounts Members
        IMoney ITripleInvoiceAmounts.Amount
        {
            set { this.amount = (Money)value; }
            get { return this.amount; }
        }
        bool ITripleInvoiceAmounts.IssueAmountSpecified
        {
            set { this.issueAmountSpecified = value; }
            get { return this.issueAmountSpecified; }
        }
        IMoney ITripleInvoiceAmounts.IssueAmount
        {
            set { this.issueAmount = (Money)value; }
            get { return this.issueAmount; }
        }
        bool ITripleInvoiceAmounts.AccountingAmountSpecified
        {
            set { this.accountingAmountSpecified = value; }
            get { return this.accountingAmountSpecified; }
        }

        IMoney ITripleInvoiceAmounts.AccountingAmount
        {
            set { this.accountingAmount = (Money)value; }
            get { return this.accountingAmount; }
        }
        #endregion ITripleInvoiceAmounts Members
    }
    #endregion NetInvoiceAmounts

    #region RebateBracketCalculation
    public partial class RebateBracketCalculation : IRebateBracketCalculation
    {
        #region Accessors
        public EFS_Decimal Amount
        {
            get { return this.amount.Amount; }
        }
        public string Currency
        {
            get { return this.amount.Currency; }
        }
        #endregion Accessors
        #region IRebateBracketCalculation Members
        IBracket IRebateBracketCalculation.Bracket
        {
            set { this.bracket = (Bracket)value; }
            get { return this.bracket; }
        }
        EFS_Decimal IRebateBracketCalculation.BracketRate
        {
            set { this.rate = value; }
            get { return this.rate; }
        }
        IMoney IRebateBracketCalculation.BracketAmount
        {
            set { this.amount = (Money)value; }
            get { return this.amount; }
        }
        bool IRebateBracketCalculation.BracketAmountSpecified
        {
            set { this.amountSpecified = value; }
            get { return this.amountSpecified; }
        }
        #endregion IRebateBracketCalculation Members
    }
    #endregion RebateBracketCalculation
    #region RebateBracketCalculations
    public partial class RebateBracketCalculations : IRebateBracketCalculations
    {
        #region Constructors
        public RebateBracketCalculations(){}
        public RebateBracketCalculations(IRebateBracketParameter[] pParameters)
        {
            rebateBracketCalculation = new RebateBracketCalculation[pParameters.Length];
            for (int i = 0; i < pParameters.Length; i++)
            {

                IRebateBracketParameter parameter = pParameters[i];
                rebateBracketCalculation[i] = new RebateBracketCalculation
                {
                    bracket = new Bracket(parameter.Bracket.LowValue.DecValue, parameter.Bracket.HighValue.DecValue),
                    rate = new EFS_Decimal(parameter.Rate.DecValue)
                };
            }
        }
        #endregion Constructors
        #region Indexors
        public RebateBracketCalculation this[decimal pAmount]
        {
            get
            {
                foreach (RebateBracketCalculation calculation in rebateBracketCalculation)
                {
                    IBracket bracket = calculation.bracket;
                    if (bracket.IsBracketMatch(pAmount))
                        return calculation;
                }
                return null;
            }
        }
        #endregion Indexors

        #region IRebateBracketCalculations Members
        IRebateBracketCalculation[] IRebateBracketCalculations.RebateBracketCalculation
        {
            set { this.rebateBracketCalculation = (RebateBracketCalculation[])value; }
            get { return this.rebateBracketCalculation; }
        }
        decimal IRebateBracketCalculations.CalculRebateBracketAmount(string pConnectionString,BracketApplicationEnum pBracketApplication, decimal pAmount, CurrencyCashInfo pCurrencyCashInfo)
        {
            decimal rebateAmount = 0;
            decimal? currentAmount;
            EFS_Cash cash;
            if (BracketApplicationEnum.Unit == pBracketApplication)
            {
                RebateBracketCalculation calculation = this[pAmount];
                if (null != calculation)
                {
                    currentAmount = calculation.rate.DecValue * pAmount;
                    calculation.amountSpecified = (currentAmount.HasValue && (0 < currentAmount.Value));
                    if (calculation.amountSpecified)
                    {
                        cash = new EFS_Cash(pConnectionString, currentAmount.Value, pCurrencyCashInfo);
                        rebateAmount = cash.AmountRounded;
                        calculation.amount = new Money(rebateAmount, pCurrencyCashInfo.Currency);
                    }
                }
            }
            else if (BracketApplicationEnum.Cumulative == pBracketApplication)
            {
                decimal remainderAmount = pAmount;
                foreach (RebateBracketCalculation calculation in rebateBracketCalculation)
                {
                    Bracket bracket = calculation.bracket;
                    if ((pAmount <= bracket.lowValue.DecValue) || (remainderAmount <= 0))
                        break;

                    Nullable<decimal> bracketAmount = null;
                    currentAmount = System.Math.Min(remainderAmount, bracket.highValue.DecValue - bracket.lowValue.DecValue);
                    bracketAmount = currentAmount * calculation.rate.DecValue;
                    calculation.amountSpecified = (bracketAmount.HasValue && (0 < bracketAmount.Value));
                    if (calculation.amountSpecified)
                    {
                        cash = new EFS_Cash(pConnectionString, bracketAmount.Value, pCurrencyCashInfo);
                        bracketAmount = cash.AmountRounded;
                        calculation.amount = new Money(bracketAmount.Value, pCurrencyCashInfo.Currency);
                        rebateAmount += bracketAmount.Value;
                    }
                    remainderAmount -= currentAmount.Value;
                }
            }
            return rebateAmount;
        }
        #endregion IRebateBracketCalculations Members
    }
    #endregion RebateBracketCalculations
    #region RebateBracketConditions
    public partial class RebateBracketConditions : IRebateBracketConditions
    {
        #region IRebateBracketConditions Members
        IRebateBracketParameters IRebateBracketConditions.Parameters
        {
            set { this.parameters = (RebateBracketParameters)value; }
            get { return this.parameters; }
        }
        IRebateBracketResult IRebateBracketConditions.Result
        {
            set { this.result = (RebateBracketResult)value; }
            get { return this.result; }
        }
        IRebateBracketParameters IRebateBracketConditions.CreateParameters(BracketApplicationEnum pBracketApplication, PeriodEnum pPeriod, int pPeriodMultiplier,RollConventionEnum pRollConvention, int pBracketLength)
        {
            return (new RebateBracketParameters(pBracketApplication, pPeriod, pPeriodMultiplier,pRollConvention, pBracketLength));
        }
        IRebateBracketResult IRebateBracketConditions.CreateResult(IRebateBracketParameter[] pParameters)
        {
            return (new RebateBracketResult(pParameters));
        }
        #endregion IRebateBracketConditions Members
    }
    #endregion RebateBracketConditions
    #region RebateBracketParameter
    public partial class RebateBracketParameter : IRebateBracketParameter,IEFS_Array
    {
        #region Constructors
        public RebateBracketParameter(){}
        public RebateBracketParameter(decimal pLowValue, decimal pHighValue, decimal pDiscountRate)
        {
            bracket = new Bracket(pLowValue, pHighValue);
            rate = new EFS_Decimal(pDiscountRate);
        }
        #endregion Constructors
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members

        #region IRebateBracketParameter Members
        IBracket IRebateBracketParameter.Bracket
        {
            set { this.bracket = (Bracket)value; }
            get { return this.bracket; }
        }
        EFS_Decimal IRebateBracketParameter.Rate
        {
            set { this.rate = value; }
            get { return this.rate; }
        }
        #endregion IRebateBracketParameter Members
    }
    #endregion RebateBracketParameter
    #region RebateBracketParameters
    public partial class RebateBracketParameters : IRebateBracketParameters
    {
        #region Constructors
        public RebateBracketParameters(){}
        public RebateBracketParameters(BracketApplicationEnum pBracketApplication, PeriodEnum pPeriod, int pPeriodMultiplier,RollConventionEnum pRollConvention, int pBracketLength)
        {
            managementType = pBracketApplication;
            applicationPeriod = new CalculationPeriodFrequency(pPeriod, pPeriodMultiplier,pRollConvention);
            parameter = new RebateBracketParameter[pBracketLength];
        }
        #endregion Constructors
        #region Indexors
        public IRebateBracketParameter this[int pIndex]
        {
            get { return parameter[pIndex]; }
            set { parameter[pIndex] = (RebateBracketParameter)value; }
        }
        #endregion Indexors

        #region IRebateBracketParameters Members
        ICalculationPeriodFrequency IRebateBracketParameters.ApplicationPeriod
        {
            set { this.applicationPeriod = (CalculationPeriodFrequency)value; }
            get { return this.applicationPeriod; }
        }
        BracketApplicationEnum IRebateBracketParameters.ManagementType
        {
            set { this.managementType = value; }
            get { return this.managementType; }
        }
        IRebateBracketParameter[] IRebateBracketParameters.Parameter
        {
            set { this.parameter = (RebateBracketParameter[])value; }
            get { return this.parameter; }
        }
        IRebateBracketParameter IRebateBracketParameters.CreateParameter(decimal pLowValue, decimal pHighValue, decimal pDiscountRate)
        {
            return (new RebateBracketParameter(pLowValue, pHighValue, pDiscountRate));
        }
        void IRebateBracketParameters.SetParameter(int pIndex, IRebateBracketParameter parameter)
        {
            this[pIndex] = parameter;
        }
        #endregion IRebateBracketParameters Members
    }
    #endregion RebateBracketParameters
    #region RebateBracketResult
    public partial class RebateBracketResult : IRebateBracketResult
    {
        #region Constructors
        public RebateBracketResult()
        {
            calculations = new RebateBracketCalculations();
        }
        public RebateBracketResult(IRebateBracketParameter[] pParameters)
        {
            calculations = new RebateBracketCalculations(pParameters);
        }
        #endregion Constructors
        #region IRebateBracketResult Members
        bool IRebateBracketResult.SumOfGrossTurnOverPreviousPeriodAmountSpecified
        {
            set { this.sumOfGrossTurnOverPreviousPeriodAmountSpecified = value; }
            get { return this.sumOfGrossTurnOverPreviousPeriodAmountSpecified; }
        }
        IMoney IRebateBracketResult.SumOfGrossTurnOverPreviousPeriodAmount
        {
            set { this.sumOfGrossTurnOverPreviousPeriodAmount = (Money)value; }
            get { return this.sumOfGrossTurnOverPreviousPeriodAmount; }
        }
        bool IRebateBracketResult.SumOfNetTurnOverPreviousPeriodAmountSpecified
        {
            set { this.sumOfNetTurnOverPreviousPeriodAmountSpecified = value; }
            get { return this.sumOfNetTurnOverPreviousPeriodAmountSpecified; }
        }
        IMoney IRebateBracketResult.SumOfNetTurnOverPreviousPeriodAmount
        {
            set { this.sumOfNetTurnOverPreviousPeriodAmount = (Money)value; }
            get { return this.sumOfNetTurnOverPreviousPeriodAmount; }
        }
        IRebateBracketCalculations IRebateBracketResult.Calculations
        {
            set { this.calculations = (RebateBracketCalculations)value; }
            get { return this.calculations; }
        }
        bool IRebateBracketResult.TotalRebateBracketAmountSpecified
        {
            set { this.totalRebateBracketAmountSpecified = value; }
            get { return this.totalRebateBracketAmountSpecified; }
        }
        IMoney IRebateBracketResult.TotalRebateBracketAmount
        {
            set { this.totalRebateBracketAmount = (Money)value; }
            get { return this.totalRebateBracketAmount; }
        }
        #endregion IRebateBracketResult Members
    }
    #endregion RebateBracketResult
    #region RebateCapConditions
    public partial class RebateCapConditions : IRebateCapConditions
    {
        #region IRebateCapConditions Members
        IRebateCapParameters IRebateCapConditions.Parameters
        {
            set { this.parameters = (RebateCapParameters)value; }
            get { return this.parameters; }
        }
        IRebateCapResult IRebateCapConditions.Result
        {
            set { this.result = (RebateCapResult)value; }
            get { return this.result; }
        }
        IRebateCapParameters IRebateCapConditions.CreateParameters(PeriodEnum pPeriod, int pPeriodMultiplier, RollConventionEnum pRollConvention, decimal pAmount, string pCurrency)
        {
            return (new RebateCapParameters(pPeriod, pPeriodMultiplier, pRollConvention, pAmount, pCurrency));
        }
        IRebateCapResult IRebateCapConditions.CreateResult()
        {
            return (new RebateCapResult());
        }
        #endregion IRebateCapConditions Members
    }
    #endregion RebateCapConditions
    #region RebateCapParameters
    public partial class RebateCapParameters : IRebateCapParameters
    {
        #region Constructors
        public RebateCapParameters() {}
        public RebateCapParameters(PeriodEnum pPeriod, int pPeriodMultiplier, RollConventionEnum pRollConvention,decimal pAmount, string pCurrency)
        {
            applicationPeriod = new CalculationPeriodFrequency(pPeriod, pPeriodMultiplier, pRollConvention);
            maximumNetTurnOverAmountSpecified = true;
            maximumNetTurnOverAmount = new Money(pAmount, pCurrency);
        }
        #endregion Constructors
        #region IRebateCapParameters Members
        bool IRebateCapParameters.MaximumNetTurnOverAmountSpecified
        {
            set { this.maximumNetTurnOverAmountSpecified = value; }
            get { return this.maximumNetTurnOverAmountSpecified; }
        }
        IMoney IRebateCapParameters.MaximumNetTurnOverAmount
        {
            set { this.maximumNetTurnOverAmount = (Money)value; }
            get { return this.maximumNetTurnOverAmount; }
        }
        ICalculationPeriodFrequency IRebateCapParameters.ApplicationPeriod
        {
            set { this.applicationPeriod = (CalculationPeriodFrequency)value; }
            get { return this.applicationPeriod; }
        }
        #endregion IRebateCapParameters Members
    }
    #endregion RebateCapParameters
    #region RebateCapResult
    public partial class RebateCapResult : IRebateCapResult
    {
        #region IRebateCapResult Members
        bool IRebateCapResult.SumOfNetTurnOverPreviousPeriodAmountSpecified
        {
            set { this.sumOfNetTurnOverPreviousPeriodAmountSpecified = value; }
            get { return this.sumOfNetTurnOverPreviousPeriodAmountSpecified; }
        }
        IMoney IRebateCapResult.SumOfNetTurnOverPreviousPeriodAmount
        {
            set { this.sumOfNetTurnOverPreviousPeriodAmount = (Money)value; }
            get { return this.sumOfNetTurnOverPreviousPeriodAmount; }
        }
        bool IRebateCapResult.NetTurnOverInExcessAmountSpecified
        {
            set { this.netTurnOverInExcessAmountSpecified = value; }
            get { return this.netTurnOverInExcessAmountSpecified; }
        }
        IMoney IRebateCapResult.NetTurnOverInExcessAmount
        {
            set { this.netTurnOverInExcessAmount = (Money)value; }
            get { return this.netTurnOverInExcessAmount; }
        }
        #endregion IRebateCapResult Members
    }
    #endregion RebateCapResult
    #region RebateConditions
    public partial class RebateConditions : IRebateConditions
    {
        #region IRebateConditions Members
        bool IRebateConditions.BracketConditionsSpecified
        {
            set { this.bracketConditionsSpecified = value; }
            get { return this.bracketConditionsSpecified; }
        }
        IRebateBracketConditions IRebateConditions.BracketConditions
        {
            set { this.bracketConditions = (RebateBracketConditions)value; }
            get { return this.bracketConditions; }
        }
        bool IRebateConditions.CapConditionsSpecified
        {
            set { this.capConditionsSpecified = value; }
            get { return this.capConditionsSpecified; }
        }
        IRebateCapConditions IRebateConditions.CapConditions
        {
            set { this.capConditions = (RebateCapConditions)value; }
            get { return this.capConditions; }
        }
        bool IRebateConditions.TotalRebateAmountSpecified
        {
            set { this.totalRebateAmountSpecified = value; }
            get { return this.totalRebateAmountSpecified; }
        }
        IMoney IRebateConditions.TotalRebateAmount
        {
            set { this.totalRebateAmount = (Money)value; }
            get { return this.totalRebateAmount; }
        }
        void IRebateConditions.CreateBracketConditions()
        {
            this.bracketConditions = new RebateBracketConditions
            {
                parameters = new RebateBracketParameters
                {
                    applicationPeriod = new CalculationPeriodFrequency(PeriodEnum.M, 1, RollConventionEnum.EOM),
                    managementType = BracketApplicationEnum.Unit
                },
                result = new RebateBracketResult()
            };
        }
        void IRebateConditions.CreateCapConditions()
        {
            this.capConditions = new RebateCapConditions
            {
                parameters = new RebateCapParameters
                {
                    applicationPeriod = new CalculationPeriodFrequency(PeriodEnum.M, 1, RollConventionEnum.EOM)
                },
                result = new RebateCapResult()
            };
        }
        #endregion IRebateConditions Members
    }
    #endregion RebateConditions
}
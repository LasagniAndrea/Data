#region using directives
using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;



using EfsML.Enum;
using EfsML.DynamicData;
using EfsML.Interface;
using EfsML.Settlement;

using EfsML.v30.Shared;
using EfsML.v30.Ird;
using EfsML.v30.AssetDef;
using EfsML.v30.Doc;
using EfsML.v30.Repository; 


using FpML.Enum;
using FpML.Interface;
using FpML.v44.Doc;
using FpML.v44.Enum;
using FpML.v44.Ird;
using FpML.v44.Shared;


#endregion using directives


namespace EfsML.v30.Invoice
{
    #region AdditionalInvoice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("additionalInvoice", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class AdditionalInvoice : Invoice 
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("initialInvoiceAmount", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Initial invoice amounts")]
        public InitialInvoiceAmounts initialInvoiceAmount;
        [System.Xml.Serialization.XmlElementAttribute("baseNetInvoiceAmount", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Initial invoice amounts")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Base net invoice amounts")]
        public NetInvoiceAmounts baseNetInvoiceAmount;
        [System.Xml.Serialization.XmlElementAttribute("theoricInvoiceAmount", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Base net invoice amounts")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Theoric invoice amounts")]
        public InvoiceAmounts theoricInvoiceAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Theoric invoice amounts")]
        public bool FillBalise2;
        #endregion Members
    }
    #endregion AdditionalInvoice
    #region AllocatedInvoice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    public partial class AllocatedInvoice : InitialNetInvoiceAmounts
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("invoiceDate", Order = 1)]
        public IdentifiedDate invoiceDate;
        [System.Xml.Serialization.XmlElementAttribute("allocatedAmounts", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Allocated amounts")]
        public NetInvoiceAmounts allocatedAmounts;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxGainOrLossAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxGainOrLossAmount", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Gain or Loss amount")]
        public Money fxGainOrLossAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public NetInvoiceAmounts unAllocatedAmounts;
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;

        #endregion Members
    }
    #endregion AllocatedInvoice
    #region AvailableInvoice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    public partial class AvailableInvoice : InitialNetInvoiceAmounts
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("invoiceDate", Order = 1)]
        public IdentifiedDate invoiceDate;
        [System.Xml.Serialization.XmlElementAttribute("availableAmounts", Order = 2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Available amounts", IsMaster = true)]
        public NetInvoiceAmounts availableAmounts;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool allocatedAccountingAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("allocatedAccountingAmount", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Allocated amounts")]
        public Money allocatedAccountingAmount;
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
        #endregion Members
    }
    #endregion AvailableInvoice

    #region Bracket
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Bracket : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lowValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("lowValue", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Low value")]
        public EFS_Decimal lowValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool highValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("highValue", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "High value")]
        public EFS_Decimal highValue;
        #endregion Members
    }
    #endregion Bracket

    #region CreditNote
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("creditNote", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class CreditNote : Invoice
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("initialInvoiceAmount", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Initial invoice amounts")]
        public InitialInvoiceAmounts initialInvoiceAmount;
        [System.Xml.Serialization.XmlElementAttribute("baseNetInvoiceAmount", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Initial invoice amounts")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Base net invoice amounts")]
        public NetInvoiceAmounts baseNetInvoiceAmount;
        [System.Xml.Serialization.XmlElementAttribute("theoricInvoiceAmount", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Base net invoice amounts")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Theoric invoice amounts")]
        public InvoiceAmounts theoricInvoiceAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Theoric invoice amounts")]
        public bool FillBalise2;
        #endregion Members
    }
    #endregion CreditNote

    #region InitialInvoiceAmounts
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InitialInvoiceAmounts : InvoiceAmounts
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("identifier", Order = 1)]
        [ControlGUI(Name = "Identifier")]
        public EFS_String identifier;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        #endregion Members
    }
    #endregion InitialInvoiceAmounts
    #region InitialNetInvoiceAmounts
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InitialNetInvoiceAmounts : NetInvoiceAmounts
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("identifier", Order = 1)]
        [ControlGUI(Name = "Identifier")]
        public EFS_String identifier;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        #endregion Members
    }
    #endregion InitialNetInvoiceAmounts
    #region Invoice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("invoice", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AdditionalInvoice))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditNote))]
    public partial class Invoice : Product
    {
        #region Members
        [ControlGUI(Name = "Payer",LineFeed=MethodsGUI.LineFeedEnum.Before)]
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference",Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference payerPartyReference;
        [ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference",Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference receiverPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("scope", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Invoicing scope")]
        public InvoicingScope scope;
        [System.Xml.Serialization.XmlElementAttribute("invoiceDate", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "scope")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Invoice date")]
        public IdentifiedDate invoiceDate;
        [System.Xml.Serialization.XmlElementAttribute("paymentDate", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Invoice date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment date")]
        public AdjustableOrRelativeAndAdjustedDate paymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool grossTurnOverAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("grossTurnOverAmount", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment date")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Gross turn over amount")]
        public Money grossTurnOverAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rebateIsInExcessSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rebateIsInExcess", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rebate In Excess")]
        public EFS_Boolean rebateIsInExcess;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rebateAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rebateAmount", Order = 8)]
        [ControlGUI(Name = "Rebate amount", LblWidth = 200)]
        public Money rebateAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rebateConditionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rebateConditions", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rebate conditions")]
        public RebateConditions rebateConditions;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool taxSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tax", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Tax")]
        public InvoiceTax tax;
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Net turn over amount")]
        [System.Xml.Serialization.XmlElementAttribute("netTurnOverAmount", Order = 11)]
        [ControlGUI(Name = "Net turn over amount",LblWidth = 200)]
        public Money netTurnOverAmount;
        [System.Xml.Serialization.XmlElementAttribute("netTurnOverIssueAmount", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Net turn over issue amount")]
        public Money netTurnOverIssueAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool netTurnOverIssueRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("netTurnOverIssueRate", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Net turn over issue rate (used by NetTurnOverIssueAmount)")]
        public EFS_Decimal netTurnOverIssueRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issueRateIsReverseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("issueRateIsReverse", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reverse")]
        public EFS_Boolean issueRateIsReverse;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issueRateReadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("issueRateRead", Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "issue Rate read")]
        public EFS_Decimal issueRateRead;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool invoiceRateSourceSpecified;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Net turn over amount")]
        [System.Xml.Serialization.XmlElementAttribute("invoiceRateSource", Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Invoice rate source")]
        public InvoiceRateSource invoiceRateSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool netTurnOverAccountingAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("netTurnOverAccountingAmount", Order = 17)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Net turn over accounting amount")]
        public Money netTurnOverAccountingAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool netTurnOverAccountingRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("netTurnOverAccountingRate", Order = 18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Net turn over accounting rate (used by all AccountingAmount)")]
        public EFS_Decimal netTurnOverAccountingRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accountingRateIsReverseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accountingRateIsReverse", Order = 19)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reverse")]
        public EFS_Boolean accountingRateIsReverse;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accountingRateReadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accountingRateRead", Order = 20)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "issue Rate read")]
        public EFS_Decimal accountingRateRead;

        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Invoice details", IsVisible = true)]
        [System.Xml.Serialization.XmlElementAttribute("invoiceDetails", Order = 21)]
        public InvoiceDetails invoiceDetails;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Invoice details")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion Invoice
    #region InvoiceAmounts
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceAmounts
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("grossTurnOverAmount", Order = 1)]
        [ControlGUI(Name = "Gross turn over amount", LblWidth = 200)]
        public Money grossTurnOverAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rebateAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rebateAmount", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Rebate amount", LblWidth = 200)]
        public Money rebateAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool taxSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tax", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Tax")]
        public InvoiceTax tax;
        [System.Xml.Serialization.XmlElementAttribute("netTurnOverAmount", Order = 4)]
        [ControlGUI(Name = "Net turn over amount", LblWidth = 200)]
        public Money netTurnOverAmount;
        [System.Xml.Serialization.XmlElementAttribute("netTurnOverIssueAmount", Order = 5)]
        [ControlGUI(Name = "Net turn over amount", LblWidth = 200)]
        public Money netTurnOverIssueAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool netTurnOverAccountingAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("netTurnOverAccountingAmount", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Net turn over accounting amount")]
        public Money netTurnOverAccountingAmount;
        #endregion Members
    }
    #endregion InvoiceAmounts
    #region InvoiceDetails
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceDetails : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("invoiceTrade", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trades", IsMaster = true,IsChild=false)]
        public InvoiceTrade[] invoiceTrade;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool invoiceTradeSortSpecified;
        [System.Xml.Serialization.XmlElementAttribute("invoiceTradeSorting", Order = 2)]
        public InvoiceTradeSort invoiceTradeSort;
        #endregion Members
    }
    #endregion InvoiceDetails
    #region InvoiceFee
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceFee : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("feeType", Order = 1)]
        [ControlGUI(Name = "Type")]
        public EFS_String feeType;
        [System.Xml.Serialization.XmlElementAttribute("feeAmount", Order = 2)]
        [ControlGUI(Name = "Amount")]
        public Money feeAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool feeAccountingAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeAccountingAmount", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Accounting Amount")]
        public Money feeAccountingAmount;
        // EG 20091110
        [System.Xml.Serialization.XmlElementAttribute("feeBaseAmount", Order = 4)]
        [ControlGUI(Name = "Base amount")]
        public Money feeBaseAmount;
        // EG 20091110
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool feeBaseAccountingAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeBaseAccountingAmount", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Accounting Base Amount")]
        public Money feeBaseAccountingAmount;
        [System.Xml.Serialization.XmlElementAttribute("feeInitialAmount", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Money feeInitialAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool feeInitialAccountingAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeInitialAccountingAmount", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Accounting Initial Amount")]
        public Money feeInitialAccountingAmount;
        [System.Xml.Serialization.XmlElementAttribute("feeDate", Order = 9)]
        [ControlGUI(Name = "Date", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Date feeDate;
        [System.Xml.Serialization.XmlElementAttribute("idA_Pay", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Integer idA_Pay;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idB_PaySpecified;
        [System.Xml.Serialization.XmlElementAttribute("idB_Pay", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Integer idB_Pay;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool feeScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeSchedule", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public InvoiceFeeSchedule feeSchedule;
        /*
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool idFeeScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idFeeSchedule", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Integer idFeeSchedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool feeScheduleIdentifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeScheduleIdentifier", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_String feeScheduleIdentifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaDCFSpecified;
        [System.Xml.Serialization.XmlElementAttribute("formulaDCF", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_String formulaDCF;
        */
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string otcmlId;
        #endregion Members
    }
    #endregion InvoiceFees
    #region InvoiceFeeSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceFeeSchedule : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool identifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("identifier", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_String identifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaDCFSpecified;
        [System.Xml.Serialization.XmlElementAttribute("formulaDCF", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_String formulaDCF;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool durationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("duration", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        // EG 20101110
        public EFS_String duration;
        //PL 20141023
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool assessmentBasisValue1Specified;
        [System.Xml.Serialization.XmlElementAttribute("assessmentBasisValue1", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal assessmentBasisValue1;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool assessmentBasisValue2Specified;
        [System.Xml.Serialization.XmlElementAttribute("assessmentBasisValue2", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal assessmentBasisValue2;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string otcmlId;
        #endregion Members
    }
    #endregion InvoiceFeeSchedule
    #region InvoiceFees
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceFees : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("invoiceFee", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fees", IsMaster = true)]
        public InvoiceFee[] invoiceFee;
        #endregion Members
    }
    #endregion InvoiceFees
    #region InvoiceRateSource
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceRateSource : ItemGUI
    {
        #region Members
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Source", IsVisible = true)]
        [System.Xml.Serialization.XmlElementAttribute("rateSource", Order = 1)]
        public InformationSource rateSource;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Source")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fixing Time", IsVisible = true)]
        [System.Xml.Serialization.XmlElementAttribute("fixingTime", Order = 2)]
        public BusinessCenterTime fixingTime;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fixing Time")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Relative to", IsVisible = true)]
        [System.Xml.Serialization.XmlElementAttribute("fixingDate", Order = 3)]
        public RelativeDateOffset fixingDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedFixingDateSpecified;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Relative to")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Adjusted fixing date")]
        [System.Xml.Serialization.XmlElementAttribute("adjustedFixingDate", Order = 4)]
        public IdentifiedDate adjustedFixingDate;
        #endregion Members
    }
    #endregion InvoiceRateSource
    #region InvoiceSettlement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("invoice", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class InvoiceSettlement : Product
    {
        #region Members
        [ControlGUI(Name = "Payer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference payerPartyReference;
        [ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference receiverPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("accountNumber", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Account number")]
        public AccountNumber accountNumber;
        [System.Xml.Serialization.XmlElementAttribute("receptionDate", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Account number")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Reception date")]
        public IdentifiedDate receptionDate;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "receive date")]
        [System.Xml.Serialization.XmlElementAttribute("settlementAmount", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement amount")]
        public Money settlementAmount;
        [System.Xml.Serialization.XmlElementAttribute("cashAmount", Order = 6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash amount")]
        public Money cashAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool bankChargesAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bankChargesAmount", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Bank charges Amount")]
        public Money bankChargesAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool vatBankChargesAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("vatBankChargesAmount", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "VAT Bank charges Amount")]
        public Money vatBankChargesAmount;
        [System.Xml.Serialization.XmlElementAttribute("netCashAmount", Order = 9)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Net Cash amount")]
        public Money netCashAmount;
        [System.Xml.Serialization.XmlElementAttribute("unallocatedAmount", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Unallocated Amount")]
        public Money unallocatedAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool allocatedInvoiceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("allocatedInvoice", Order = 11)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Allocated Invoices", IsMaster = true)]
        public AllocatedInvoice[] allocatedInvoice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxGainOrLossAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxGainOrLossAmount", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "FxGainOrLoss Amount")]
        public Money fxGainOrLossAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool availableInvoiceSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public AvailableInvoice[] availableInvoice;
        #endregion Members
    }
    #endregion InvoiceSettlement
    #region InvoiceTax
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceTax : TripleInvoiceAmounts
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("details", Order = 1)]
        public TaxSchedule[] details;
        #endregion Members
    }
    #endregion InvoiceTax

    #region InvoiceTrade
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    // 20090427 EG Add side (TradeSideEnum)
    // 20090909 FI [Add Asset and price on InvoiceTrade]
    // 20100628 EG [Add Market and DerivativeContract on InvoiceTrade]
    // EG 20171004 [23452] tradeDateTime
    // EG 20220324 [XXXXX] Change type of element contract (ContractRepository) 
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    public partial class InvoiceTrade : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("identifier", Order = 1)]
        [ControlGUI(Name = "Identifier")]
        public EFS_String identifier;
        [System.Xml.Serialization.XmlElementAttribute("instrument", Order = 2)]
        [ControlGUI(Name = "Instrument", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public ProductType instrument;
        [System.Xml.Serialization.XmlElementAttribute("tradeDate", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Date", IsVisible = true)]
        public TradeDate tradeDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tradeDateTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeDateTime", Order = 4)]
        public ZonedDateTime tradeDateTime;
        [System.Xml.Serialization.XmlElementAttribute("inDate", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade Date")]
        [ControlGUI(Name = "In date")]
        public EFS_Date inDate;
        [System.Xml.Serialization.XmlElementAttribute("outDate", Order = 6)]
        [ControlGUI(Name = "Out date")]
        public EFS_Date outDate;
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 7)]
        [ControlGUI(Name = "Currency")]
        public Currency currency;
        [System.Xml.Serialization.XmlElementAttribute("side", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Side")]
        public TradeSideEnum side;
        [System.Xml.Serialization.XmlElementAttribute("counterpartyPartyReference", Order = 9)]
        [ControlGUI(Name = "Counterparty reference")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference counterpartyPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalAmount", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Money notionalAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool periodNumberOfDaysSpecified;
        #region periodNumberOfDays
        /// <summary>
        /// Durée du trade exprimée en nombre de jours (base Exact/360)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("periodNumberOfDays", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Integer periodNumberOfDays;
        #endregion

        #region invoiceFees
        /// <summary>
        /// Liste des frais
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("invoiceFees", Order = 12)]
        public InvoiceFees invoiceFees;
        #endregion
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tradersSpecified;
        #region traders
        /// <summary>
        /// Liste des traders de l'acteur facturé
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("trader", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Trader[] traders;
        #endregion
        //
        //20091021 FI add sales
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool salesSpecified;
        #region sales
        /// <summary>
        /// Liste des sales de l'acteur facturé
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("sales", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Trader[] sales;
        #endregion
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool assetSpecified;
        #region asset
        /// <summary>
        /// liste des assets existants sur le trade
        /// <para>alimenté uniquement pour les opération sur titres</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("asset", Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public ShortAsset[] asset;
        #endregion asset
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceSpecified;
        #region price
        /// <summary>
        /// Prix caractéristique du trade
        /// <para>swap : taux fixe</para>
        /// <para>capFloor : strike</para>
        /// <para>loadDeposit : taux fixe</para>
        /// </summary>
        /// 
        [System.Xml.Serialization.XmlElementAttribute("price", Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_String price;
        #endregion
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool marketSpecified;
        #region Market
        [System.Xml.Serialization.XmlElementAttribute("market", Order = 17)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public CommonRepository market;
        #endregion Market
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contractSpecified;
        #region DerivativeContract / CommodityContract
        [System.Xml.Serialization.XmlElementAttribute("contract", Order = 18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public ContractRepository contract;
        #endregion DerivativeContract / CommodityContract
        //
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
        #endregion Members
    }
    #endregion InvoiceTrade

    #region InvoiceTradeReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceTradeReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion
    //
    #region InvoiceTradeSort
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceTradeSort
    {
        [System.Xml.Serialization.XmlElementAttribute("keys", Order = 1)]
        public InvoiceTradeSortKeys keys;
        [System.Xml.Serialization.XmlElementAttribute("groups", Order = 2)]
        public InvoiceTradeSortGroups groups;
    }
    #endregion InvoiceTradeSort
    //
    #region InvoiceTradeSortKey
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceTradeSortKey
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string sortScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion InvoiceTradeSortKey
    //
    #region InvoiceTradeSortKeys
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceTradeSortKeys
    {
        [System.Xml.Serialization.XmlElementAttribute("key", Order = 1)]
        public InvoiceTradeSortKey[] key;
    }
    #endregion InvoiceTradeSortKeys
    //
    #region InvoiceTradeSortKeyValue
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceTradeSortKeyValue
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members
    }
    #endregion
    //
    #region InvoiceTradeSortGroup
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoiceTradeSortGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("keyValue", Order = 1)]
        public InvoiceTradeSortKeyValue[] keyValue;
        //
        [System.Xml.Serialization.XmlElementAttribute("invoiceTradeReference", Order = 2)]
        public InvoiceTradeReference[] invoiceTradeReference;
        //
        [System.Xml.Serialization.XmlElementAttribute("sum", Order = 3)]
        public InvoiceTradeSortSum sum;
        //
    }
    #endregion
    //    
    #region InvoiceTradeSortGroups
    public partial class InvoiceTradeSortGroups
    {
        [System.Xml.Serialization.XmlElementAttribute("group", Order = 1)]
        public InvoiceTradeSortGroup[] group;
    }
    #endregion
    //
    #region InvoiceTradeSortSum
    public partial class InvoiceTradeSortSum
    {
        [System.Xml.Serialization.XmlElementAttribute("feeAmount", Order = 1)]
        public Money feeAmount;
        //
        [System.Xml.Serialization.XmlElementAttribute("feeAccountingAmount", Order = 2)]
        public Money feeAccountingAmount;
        //
        [System.Xml.Serialization.XmlElementAttribute("feeInitialAmount", Order = 3)]
        public Money feeInitialAmount;
        //
        [System.Xml.Serialization.XmlElementAttribute("feeInitialAccountingAmount", Order = 4)]
        public Money feeInitialAccountingAmount;
    }
    #endregion



    #region InvoicingScope
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoicingScope : SchemeBoxGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string invoicingScopeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        #endregion Members
    }
    #endregion InvoicingScope

    #region NetInvoiceAmounts
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class NetInvoiceAmounts : TripleInvoiceAmounts
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool taxSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tax", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "tax")]
        public InvoiceTax tax;
        #endregion Members
    }
    #endregion NetInvoiceAmounts

    #region RebateBracketCalculation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RebateBracketCalculation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("bracket", Order = 1)]
        public Bracket bracket;
        [System.Xml.Serialization.XmlElementAttribute("rebateBracketRate", Order = 2)]
        [ControlGUI(Name = "rate")]
        public EFS_Decimal rate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool amountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rebateBracketAmount", Order = 3)]
        [ControlGUI(Name = "amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Money amount;
        #endregion Members
    }
    #endregion RebateBracketCalculation
    #region RebateBracketCalculations
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RebateBracketCalculations
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("calculation", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Brackets", IsMaster = true)]
        public RebateBracketCalculation[] rebateBracketCalculation;
        #endregion Members
    }
    #endregion RebateBracketCalculations
    #region RebateBracketConditions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RebateBracketConditions : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("parameters", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Parameters", IsVisible = true)]
        public RebateBracketParameters parameters;
        [System.Xml.Serialization.XmlElementAttribute("result", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Parameters")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Result", IsVisible = true)]
        public RebateBracketResult result;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Result")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion RebateBracketConditions
    #region RebateBracketParameter
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RebateBracketParameter : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("bracket", Order = 1)]
        public Bracket bracket;
        [System.Xml.Serialization.XmlElementAttribute("rate", Order = 2)]
        [ControlGUI(Name = "Rate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal rate;
        #endregion Members
    }
    #endregion RebateBracketParameter
    #region RebateBracketParameters
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RebateBracketParameters : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("applicationPeriod", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Application period")]
        public CalculationPeriodFrequency applicationPeriod;
        [System.Xml.Serialization.XmlElementAttribute("rebateBracketManagementType", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Application period")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Management type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public BracketApplicationEnum managementType;
        [System.Xml.Serialization.XmlElementAttribute("parameter", Order = 3)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Brackets", IsMaster = true)]
        public RebateBracketParameter[] parameter;
        #endregion Members
    }
    #endregion RebateBracketParameters
    #region RebateBracketResult
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RebateBracketResult : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sumOfGrossTurnOverPreviousPeriodAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sumOfGrossTurnOverPreviousPeriodAmount", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sum of Gross turn over previous period amount")]
        public Money sumOfGrossTurnOverPreviousPeriodAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sumOfNetTurnOverPreviousPeriodAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sumOfNetTurnOverPreviousPeriodAmount", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sum of Net turn over previous period amount")]
        public Money sumOfNetTurnOverPreviousPeriodAmount;
        [System.Xml.Serialization.XmlElementAttribute("calculations", Order = 3)]
        public RebateBracketCalculations calculations;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totalRebateBracketAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("totalRebateBracketAmount", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Final amount", IsVisible = true)]
        public Money totalRebateBracketAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Final amount")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion RebateBracketResult

    #region RebateCapConditions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RebateCapConditions : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("parameters", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Parameters", IsVisible = true)]
        public RebateCapParameters parameters;
        [System.Xml.Serialization.XmlElementAttribute("result", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Parameters")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Result", IsVisible = true)]
        public RebateCapResult result;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Result")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion RebateCapConditions
    #region RebateCapParameters
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RebateCapParameters : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("applicationPeriod", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Application period")]
        public CalculationPeriodFrequency applicationPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maximumNetTurnOverAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("maximumNetTurnOverAmount", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Application period")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maximum net turn over amount")]
        public Money maximumNetTurnOverAmount;
        #endregion Members
    }
    #endregion RebateCapParameters
    #region RebateCapResult
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RebateCapResult : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sumOfNetTurnOverPreviousPeriodAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sumOfNetTurnOverPreviousPeriodAmount", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sum of Net turn over previous period amount")]
        public Money sumOfNetTurnOverPreviousPeriodAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool netTurnOverInExcessAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("netTurnOverInExcessAmount", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Net turn over amount in excess")]
        public Money netTurnOverInExcessAmount;
        #endregion Members
    }
    #endregion RebateCapResult
    #region RebateConditions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class RebateConditions : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool bracketConditionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bracketConditions", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Bracket conditions")]
        public RebateBracketConditions bracketConditions;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool capConditionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("capConditions", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cap conditions")]
        public RebateCapConditions capConditions;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool totalRebateAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("totalRebateAmount", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Total rebate amount", LblWidth = 200)]
        public Money totalRebateAmount;
        #endregion Members
    }
    #endregion RebateConditions
}

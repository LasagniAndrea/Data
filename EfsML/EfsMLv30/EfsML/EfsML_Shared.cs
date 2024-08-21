#region using directives
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.v30.AssetDef;
using EfsML.v30.Ird;
using EfsML.v30.LoanDeposit;
using EfsML.v30.Security.Shared;
using FpML.Enum;
using FpML.v44.Assetdef;
using FpML.v44.Fx;
using FpML.v44.Ird;
using FpML.v44.Shared;
#endregion using directives


namespace EfsML.v30.Shared
{
    #region AbstractTransaction
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public abstract partial class AbstractTransaction : Product
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference",Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
		[ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference",Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
		[ControlGUI(Name = "Seller", LineFeed = MethodsGUI.LineFeedEnum.After)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;
		#endregion Members
	}
	#endregion AbstractTransaction
	#region AbstractUnitTransaction
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public abstract partial class AbstractUnitTransaction : AbstractTransaction
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("numberOfUnits", Order = 1)]
		[ControlGUI(Name = "Number of units")]
		public EFS_Decimal numberOfUnits;
		[System.Xml.Serialization.XmlElementAttribute("unitPrice", Order = 2)]
		[ControlGUI(Name = "Price (per unit)", LineFeed = MethodsGUI.LineFeedEnum.BeforeAndAfter)]
		public Money unitPrice;
		#endregion Members
	}
	#endregion AbstractUnitTransaction
    #region AccountNumber
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AccountNumber
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("correspondant", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Correspondant")]
        public SpheresId correspondant;
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 2)]
        [ControlGUI(Name = "Currency")]
        public Currency currency;
        [System.Xml.Serialization.XmlElementAttribute("accountNumber", Order = 3)]
        [ControlGUI(Name = "account Number")]
        public EFS_String accountNumber;
        [System.Xml.Serialization.XmlElementAttribute("accountName", Order = 4)]
        [ControlGUI(Name = "account Name")]
        public EFS_String accountName;
        [System.Xml.Serialization.XmlElementAttribute("nostroAccountNumber", Order = 5)]
        [ControlGUI(Name = "Nostro account number")]
        public EFS_String nostroAccountNumber;
        [System.Xml.Serialization.XmlElementAttribute("journalCode", Order = 6)]
        [ControlGUI(Name = "Journal Code")]
        public EFS_String journalCode;
        #endregion Members
    }
    #endregion AccountNumber

    #region Application
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SoftApplication
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("name", Order = 1)]
        public EFS_String  name;
        [System.Xml.Serialization.XmlElementAttribute("version", Order = 2)]
        public EFS_String version;
        #endregion Members
    }
    #endregion AccountNumber

    #region AssetFxRateId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/EFSmL-3-0")]
    public partial class AssetFxRateId : SchemeBoxGUI
    {
        #region Members
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate ref.", Width = 300)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        public string Value;
        public string otcmlId;
        #endregion Members
    }
    #endregion AssetFxRateId

    #region CalculationBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("calculation", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class CalculationBase : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("notionalSchedule", typeof(Notional), Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Notional", IsVisible = true)]
        public Notional calculationNotional;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Notional")]
        public bool FillBalise;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate", IsCopyPaste = true)]
        public EFS_RadioChoice rate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateFixedRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedRateSchedule", typeof(Schedule), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Fixed Rate", IsVisible = true)]
        public Schedule rateFixedRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateFloatingRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingRateCalculation", typeof(FloatingRateCalculation), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Floating Rate", IsVisible = true)]
        public FloatingRateCalculation rateFloatingRate;

        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "dayCountFraction")]
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction",Order = 4)]
        public DayCountFractionEnum dayCountFraction;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool discountingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("discounting", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "discounting")]
        public Discounting discounting;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool compoundingMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("compoundingMethod",Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "compoundingMethod")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CompoundingMethodEnum compoundingMethod;

        #endregion Members
    }
    #endregion CalculationBase
    #region CalculationPeriodAmountBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CalculationPeriodAmountBase : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Period Amount")]
        public EFS_RadioChoice calculationPeriodAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodAmountCalculationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculation", typeof(CalculationBase), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Calculation", IsVisible = true)]
        public CalculationBase calculationPeriodAmountCalculation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodAmountKnownAmountScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("knownAmountSchedule", typeof(KnownAmountSchedule), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Known Amount Schedule", IsVisible = true, IsCopyPaste = true)]
        public KnownAmountSchedule calculationPeriodAmountKnownAmountSchedule;

        #endregion Members
    }
    #endregion CalculationPeriodAmountBase

    #region CashPosition
    /// <summary>
    /// Représente un solde (un solde de trésorie), un montant à une date donnée
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CashPosition : ItemGUI
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [ControlGUI(Name = "Payer")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        public PartyOrAccountReference payerPartyReference;

        /// <summary>
        /// 
        /// </summary>
        [ControlGUI(Name = "Receiver")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        public PartyOrAccountReference receiverPartyReference;
        
        /// <summary>
        /// 
        /// </summary>
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        [System.Xml.Serialization.XmlElementAttribute("amount", Order = 3)]
        public Money amount;
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Amount")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Date")]
        public EFS_RadioChoice date;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dateDefineSpecified;
        
        /// <summary>
        /// Représente la date de solde (elle est nécessairement ajustée)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("dateDefine", typeof(IdentifiedDate), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public IdentifiedDate dateDefine;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dateReferenceSpecified;

        /// <summary>
        /// Représente une référence qui permet d'obtenir la date de solde (elle est nécessairement ajustée))
        /// </summary>
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        [System.Xml.Serialization.XmlElementAttribute("dateReference", typeof(DateReference), Order = 5)]
        public DateReference dateReference;
        #endregion Members
    }
    #endregion CashPosition

    #region CustomerSettlementPayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CustomerSettlementPayment : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
        [ControlGUI(Name = "currency")]
        public Currency currency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool amountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amount", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "amount")]
        public EFS_Decimal amount;
        [System.Xml.Serialization.XmlElementAttribute("rate", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate", IsVisible = true)]
        public ExchangeRate rate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion CustomerSettlementPayment

    #region EventType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EventType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string eventTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members
    }
    #endregion EventType

    #region ExchangeCashPosition
    /// <summary>
    /// Représente un solde (un solde de trésorerie)
    /// <para>Ce solde peut être exprimé en devise de contrevaleur</para>
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ExchangeCashPosition : CashPosition
    {
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeAmountSpecified;

        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Exchange amount")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [System.Xml.Serialization.XmlElementAttribute("exchangeAmount", Order = 1)]
        public Money exchangeAmount;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeFxRateReferenceSpecified;

        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Exchange fx rate")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exchange fx rate")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.FxRate)]
        [System.Xml.Serialization.XmlElementAttribute("exchangeFxRateReference", Order = 2)]
        public FxRateReference[] exchangeFxRateReference;

        ///// <summary>
        ///// Do not used
        ///// </summary>
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "constituent")]
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool FillBalise2;
    }
    #endregion ExchangeCashPosition

    #region ImplicitProvision
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class ImplicitProvision : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cancelableProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cancelableProvision", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Cancelable Provision")]
		public Empty cancelableProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool mandatoryEarlyTerminationProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationProvision", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Mandatory Early Termination Provision")]
		public Empty mandatoryEarlyTerminationProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionalEarlyTerminationProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionalEarlyTerminationProvision", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Optional Early Termination Provision")]
		public Empty optionalEarlyTerminationProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool extendibleProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("extendibleProvision", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Extendible Provision")]
		public Empty extendibleProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stepUpProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("stepUpProvision", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Step Up Provision")]
		public Empty stepUpProvision;
		#endregion Members
	}
	#endregion ImplicitProvision
    #region Identification
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Identification : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string identificationScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members
    }
    #endregion Identification

    #region InterestRateStreamBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DebtSecurityStream))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(LoanDepositStream))]
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    public abstract partial class InterestRateStreamBase : ItemGUI
    {
        #region Members
        [ControlGUI(Name = "Payer")]
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference payerPartyReference;
        [ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyOrAccountReference receiverPartyReference;
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Period Dates", IsVisible = false, IsCopyPaste = true)]
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodDates",  Order = 3)]
        public CalculationPeriodDates calculationPeriodDates;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Period Dates")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Dates", IsVisible = false, IsCopyPaste = true)]
        [System.Xml.Serialization.XmlElementAttribute("paymentDates", Order = 4)]
        public PaymentDates paymentDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool resetDatesSpecified;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Dates")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Reset Dates", IsCopyPaste = true)]
        [System.Xml.Serialization.XmlElementAttribute("resetDates", Order = 5)]
        public ResetDates resetDates;
        [ControlGUI(IsLabel = false, Name = "Calculation Period Amount", IsCopyPaste = true)]
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodAmount", Order = 6)]
        public CalculationPeriodAmountBase calculationPeriodAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stubCalculationPeriodAmountSpecified;
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Stub Calculation Period amounts", IsCopyPaste = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [System.Xml.Serialization.XmlElementAttribute("stubCalculationPeriodAmount", Order = 7)]
        public StubCalculationPeriodAmount stubCalculationPeriodAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool principalExchangesSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Principal Exchanges")]
        [System.Xml.Serialization.XmlElementAttribute("principalExchanges", Order = 8)]
        public PrincipalExchanges principalExchanges;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementProvisionSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary,  Name = "Settlement Provision")]
        [System.Xml.Serialization.XmlElementAttribute("settlementProvision", Order = 9)]
        public SettlementProvision settlementProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Formula")]
        [System.Xml.Serialization.XmlElementAttribute("formula", Order = 10)]
        public Formula formula;
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion InterestRateStreamBase

    #region MarginRatio
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class MarginRatio : ActualPrice
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spreadSchedule", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread Schedule", IsCopyPaste = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread Schedule", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        public SpreadSchedule spreadSchedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool crossMarginRatioSpecified;
        [System.Xml.Serialization.XmlElementAttribute("crossMarginRatio", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public ActualPrice crossMarginRatio;
        #endregion Members
    }
    #endregion MarginRatio
    #region PartyPayerReceiverReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PartyPayerReceiverReference : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party")]
        public EFS_RadioChoice partyReference;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partyReferencePayerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", typeof(PartyReference), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Payer", IsVisible = true)]
        public PartyReference partyReferencePayer;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partyReferenceReceiverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", typeof(PartyReference), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Receiver", IsVisible = true)]
        public PartyReference partyReferenceReceiver;
        #endregion Members
    }
    #endregion PartyPayerReceiverReference
    #region PaymentQuote
    public partial class PaymentQuote : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool percentageRateFractionSpecified;

        [System.Xml.Serialization.XmlElementAttribute("percentageRateFraction", typeof(EFS_String), Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "rate Fraction")]
        public EFS_String percentageRateFraction;

        [System.Xml.Serialization.XmlElementAttribute("percentageRate", typeof(EFS_Decimal), Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "rate")]
        public EFS_Decimal percentageRate;

        [System.Xml.Serialization.XmlElementAttribute("paymentRelativeTo", typeof(AmountReference), Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Relative to")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentQuote)]
        public AmountReference paymentRelativeTo;
        #endregion Members
    }
    #endregion PaymentQuote
	#region PriceReference
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class PriceReference : HrefGUI
	{
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
		public string href;
	}
	#endregion PriceReference

    #region RoutingCreateElement
    public partial class RoutingCreateElement
    {
    }
    #endregion 
    #region RoutingPartyReference
    public partial class RoutingPartyReference : Routing
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hrefSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion

    #region SpheresId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    public partial class SpheresId
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_SchemeValue spheresId;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "Scheme", Width = 400)]
        public string scheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 150)]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        /*
        [System.Xml.Serialization.XmlAttributeAttribute("id",Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "Id", Width = 50)]
        public string id;
        */
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string otcmlId;
        #endregion Members
    }
    #endregion SpheresId
    #region TradeExtend
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TradeExtend
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_SchemeValue spheresId;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "Scheme", Width = 400)]
        public string scheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 150)]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string otcmlId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool hRefSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute("href", DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public string hRef;
        #endregion Members
    }
    #endregion TradeExtend
    #region SpheresSource
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SpheresSource : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool statusSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "status")]
        [System.Xml.Serialization.XmlElementAttribute("status", Order = 1)]
        public SpheresSourceStatusEnum status;

        [System.Xml.Serialization.XmlElementAttribute("spheresId", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spheres Id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spheres Id")]
        public SpheresId[] spheresId;
        #endregion Members
    }
    #endregion SpheresSource
    #region Tax
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Tax : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("taxSource", Order = 1)]
        //[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source", IsVisible = true)]
        public SpheresSource taxSource;

        [System.Xml.Serialization.XmlElementAttribute("taxDetail", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Detail")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Detail", IsChild = true, IsVariableArray = true, IsMaster = true, IsMasterVisible = true, MinItem = 0)]
        public TaxSchedule[] taxDetail;
        #endregion Members
    }
    #endregion Tax

    #region TaxSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TaxSchedule : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool taxAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("taxAmount", Order = 1)]
        [ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public TripleInvoiceAmounts taxAmount;
        [System.Xml.Serialization.XmlElementAttribute("taxSource", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        public SpheresSource taxSource;
        #endregion Members
    }
    #endregion TaxSchedule
    #region TradeExtends
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TradeExtends : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("tradeExtend", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade Extends")]
        public TradeExtend[] tradeExtend;
        #endregion Members
    }
    #endregion TradeExtends
    #region TripleInvoiceAmounts
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    // EG 20210623 [XXXXX] Add CreateControlGUI optionnel sur issueAmount (Pb Validation sur CB)
    public partial class TripleInvoiceAmounts
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("amount", Order = 1)]
        [ControlGUI(Name = "amount", LblWidth = 200)]
        public Money amount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issueAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("issueAmount", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Issue amount", LblWidth = 200)]
        public Money issueAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accountingAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accountingAmount", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Accounting amount")]
        public Money accountingAmount;
        #endregion Members
    }
    #endregion TripleInvoiceAmounts

    #region ZonedDateTime
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    /// EG 20171003 [23452] New
    public partial class ZonedDateTime : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string zonedDateTimeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            set { _date = new EFS_DateTimeOffset(value); }
            get { return ValueSpecified ? _date.ISODateTimeValue : string.Empty; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ValueSpecified
        {
            get { return (null != _date) && _date.DateTimeValue.HasValue; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool _dateSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Date", LblWidth = 130)]
        public EFS_DateTimeOffset _date;
        [System.Xml.Serialization.XmlAttributeAttribute("tzdbId", DataType = "normalizedString")]
        public string tzdbid;

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion ZonedDateTime
}

#region using directives
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using FpML.Enum;
using FpML.v44.Assetdef;
using FpML.v44.Enum;
using FpML.v44.Option.Shared;
using FpML.v44.Shared;
using System.Reflection;

#endregion using directives

namespace FpML.v44.Cd
{
    #region AdditionalFixedPayments
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AdditionalFixedPayments : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interestShortfallReimbursementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("interestShortfallReimbursement", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Interest shortfall reimbursement")]
        public Empty interestShortfallReimbursement;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool principalShortfallReimbursementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("principalShortfallReimbursement", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Principal shortfall reimbursement")]
        public Empty principalShortfallReimbursement;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool writedownReimbursementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("writedownReimbursement", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Writedown reimbursement")]
        public Empty writedownReimbursement;
        #endregion Members
    }
    #endregion AdditionalFixedPayments
    #region AdditionalTerm
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AdditionalTerm : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue additionalTerm;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 500)]
        public string additionalTermScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
        #endregion Members
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion AdditionalTerm
    #region AdjustedPaymentDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AdjustedPaymentDates : ItemGUI, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", Order = 1)]
        [ControlGUI(Name = "Date")]
        public EFS_Date adjustedPaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("paymentAmount", Order = 2)]
        [ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Money paymentAmount;
        #endregion Members
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion AdjustedPaymentDates

    #region BasketReferenceInformation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BasketReferenceInformation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool basketNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basketName", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basket name")]
        public BasketName basketName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool basketIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basketId",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basket Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basket Id")]
        public BasketId[] basketId;

		[System.Xml.Serialization.XmlElementAttribute("referencePool", IsNullable = false,Order=3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference pool")]
        public ReferencePool referencePool;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nthToDefaultSpecified;
        [System.Xml.Serialization.XmlElementAttribute("nthToDefault",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Nth reference obligation to default triggers payout")]
        public EFS_PosInteger nthToDefault;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mthToDefaultSpecified;
        [System.Xml.Serialization.XmlElementAttribute("mthToDefault",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Mth reference obligation to default to allow representation of Mth defaults")]
        public EFS_PosInteger mthToDefault;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trancheSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tranche",Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CDS tranche terms")]
        public Tranche tranche;
        #endregion Members
    }
    #endregion BasketReferenceInformation

    #region CalculationAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CalculationAmount : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
        [ControlGUI(Name = "Currency", Width = 75)]
        public Currency currency;
        [System.Xml.Serialization.XmlElementAttribute("amount", Order = 2)]
        [ControlGUI(Name = "Amount")]
        public EFS_Decimal amount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepSpecified;
        [System.Xml.Serialization.XmlElementAttribute("step",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Steps")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Step")]
        public Step[] step;
        #endregion Members
    }
    #endregion CalculationAmount
    #region CashSettlementTerms
    // EG 20140702 New build FpML4.4 Choice FixedRecovery between cashSettlementAmount and recoryFactor
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CashSettlementTerms : SettlementTerms
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationDate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation date")]
        public ValuationDate valuationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationTime", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation time")]
        public BusinessCenterTime valuationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quotationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quotationMethod", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quotation method")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public QuotationRateTypeEnum quotationMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quotationAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quotationAmount", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quotation amount")]
        public Money quotationAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minimumquotationAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("minimumQuotationAmount", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Minimum quotation amount")]
        public Money minimumQuotationAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dealerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dealer",Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dealer")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dealer", IsClonable = true, IsChild = false)]
        public EFS_StringArray[] dealer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementBusinessDaysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementBusinessDays", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash settlement business days", Width = 100)]
        public EFS_NonNegativeInteger cashSettlementBusinessDays;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, IsDisplay = false, Name = "Recovery")]
        public EFS_RadioChoice fixedRecovery;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixedRecovery_cashSettlementAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementAmount", typeof(Money), Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Money fixedRecovery_cashSettlementAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixedRecovery_factorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("recoveryFactor", typeof(EFS_Decimal), Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal fixedRecovery_factor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterest", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued interest")]
        public EFS_Boolean accruedInterest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationMethod", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation method", Width = 200)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public ValuationMethodEnum valuationMethod;
        #endregion Members
    }
    #endregion CashSettlementTerms
    #region CreditDefaultSwap
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("creditDefaultSwap", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Credit Default Swap")]
    public partial class CreditDefaultSwap : Product
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("generalTerms", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "General Terms", IsVisible = false)]
        public GeneralTerms generalTerms;
		[System.Xml.Serialization.XmlElementAttribute("feeLeg", Order = 2)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "General Terms")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fee", IsVisible = false)]
        public FeeLeg feeLeg;
		[System.Xml.Serialization.XmlElementAttribute("protectionTerms", Order = 3)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fee")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Protection Terms", IsClonable = true, IsMaster = true, IsChild = true)]
        public ProtectionTerms[] protectionTerms;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, IsDisplay = false, Name = "Settlement Terms")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemCashSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlementTerms", typeof(CashSettlementTerms), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public CashSettlementTerms itemCash;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemPhysicalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("physicalSettlementTerms", typeof(PhysicalSettlementTerms), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public PhysicalSettlementTerms itemPhysical;
        #endregion Members
    }
    #endregion CreditDefaultSwap
    #region CreditDefaultSwapOption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("creditDefaultSwapOption", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Credit Default Swap Option")]
    public partial class CreditDefaultSwapOption : OptionBaseExtended
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("strike", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Strike", IsVisible = true)]
        public CreditOptionStrike strike;
		[System.Xml.Serialization.XmlElementAttribute("creditDefaultSwap", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Credit default swap", IsVisible = true)]
        public CreditDefaultSwap creditDefaultSwap;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Credit default swap")]
        public bool FillBalise2;
        #endregion Members
    }
    #endregion CreditDefaultSwapOption
    #region CreditOptionStrike
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CreditOptionStrike : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expressed as")]
        public EFS_RadioChoice expressedAs;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool expressedAsSpreadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spread", typeof(EFS_Decimal),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Spread", IsVisible = true)]
        public EFS_Decimal expressedAsSpread;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool expressedAsPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("price", typeof(EFS_Decimal),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Price", IsVisible = true)]
        public EFS_Decimal expressedAsPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool expressedAsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikeReference", typeof(FixedRateReference),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Reference", IsVisible = true)]
        public FixedRateReference expressedAsReference;
        #endregion Members
        #region Constructors
        public CreditOptionStrike()
        {
            expressedAsSpread = new EFS_Decimal();
            expressedAsPrice = new EFS_Decimal();
            expressedAsReference = new FixedRateReference();
        }
        #endregion Constructors
    }
    #endregion CreditOptionStrike

    #region DeliverableObligations
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DeliverableObligations : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterest", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued interest")]
        public EFS_Boolean accruedInterest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool categorySpecified;
        [System.Xml.Serialization.XmlElementAttribute("category", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Category", Width = 250)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public ObligationCategoryEnum category;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notSubordinatedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notSubordinated", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Characteristics", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not subordinated")]
        public Empty notSubordinated;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool specifiedCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("specifiedCurrency", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Specified currency")]
        public SpecifiedCurrency specifiedCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notSovereignLenderSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notSovereignLender", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not sovereign lender")]
        public Empty notSovereignLender;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("notDomesticCurrency", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Not domestic currency")]
        public NotDomesticCurrency notDomesticCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticLawSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notDomesticLaw", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not domestic law")]
        public Empty notDomesticLaw;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool listedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("listed", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Listed")]
        public Empty listed;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notContingentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notContingent", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not contingent")]
        public Empty notContingent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticIssuanceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notDomesticIssuance", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not domestic issuance")]
        public Empty notDomesticIssuance;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool assignableLoanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("assignableLoan", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Assignable loan")]
        public PCDeliverableObligationCharac assignableLoan;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool consentRequiredLoanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("consentRequiredLoan", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Consent required loan")]
        public PCDeliverableObligationCharac consentRequiredLoan;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool directLoanParticipationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("directLoanParticipation", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Direct loan participation")]
        public LoanParticipation directLoanParticipation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool transferableSpecified;
        [System.Xml.Serialization.XmlElementAttribute("transferable", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Transferable")]
        public Empty transferable;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maximumMaturitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("maximumMaturity", Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maximum maturity")]
        public Interval maximumMaturity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool acceleratedOrMaturedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("acceleratedOrMatured", Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Accelerated or matured")]
        public Empty acceleratedOrMatured;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notBearerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notBearer", Order = 17)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not bearer")]
        public Empty notBearer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = false, Name = "Obligation liability")]
        public EFS_RadioChoice liability;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityNone;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityGeneralFundSpecified;
        [System.Xml.Serialization.XmlElementAttribute("generalFundObligationLiability", typeof(Empty),Order=18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityGeneralFund;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityFullFaithAndCreditSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fullFaithAndCreditObLiability", typeof(Empty),Order=19)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityFullFaithAndCredit;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityRevenueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("revenueObligationLiability", typeof(Empty),Order=20)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityRevenue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indirectLoanParticipationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indirectLoanParticipation", Order = 21)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Characteristics")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Indirect loan participation")]
        public LoanParticipation indirectLoanParticipation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool excludedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("excluded", Order = 22)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Excluded")]
        public EFS_MultiLineString excluded;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool othReferenceEntityObligationsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("othReferenceEntityObligations", Order = 23)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Other reference entity Obligations")]
        public EFS_MultiLineString othReferenceEntityObligations;
        #endregion Members
        #region Constructors
        public DeliverableObligations()
        {
            liabilityNone = new Empty();
            liabilityFullFaithAndCredit = new Empty();
            liabilityGeneralFund = new Empty();
            liabilityRevenue = new Empty();
        }
        #endregion Constructors
    }
    #endregion DeliverableObligations
    #region DeprecatedScheduledTerminationDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DeprecatedScheduledTerminationDate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", Order = 1)]
        public AdjustableDate2 adjustableDate;
        #endregion Members
    }
    #endregion DeprecatedScheduledTerminationDate

    #region EntityType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class EntityType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string entityTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members
        #region Constructors
        public EntityType()
        {
            entityTypeScheme = "http://www.fpml.org/coding-scheme/entity-type-1-0";
        }
        #endregion Constructors
    }
    #endregion EntityName

    #region FixedRate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FixedRate
    {
        #region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Name = "value")]
		public EFS_Decimal decValue;
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

		#region Accessors
		#region Value
		[System.Xml.Serialization.XmlTextAttribute()]
		public decimal Value
		{
			set { decValue = new EFS_Decimal(value); }
			get {return decValue.DecValue;}
		}
		#endregion Value
		#endregion Accessors
	}
    #endregion FixedRate
    #region FeeLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FeeLeg : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initialPayment", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial payment")]
        public InitialPayment initialPayment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool singlePaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("singlePayment",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Single Payments")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Single Payment", IsClonable = true, IsChild = true)]
        public SinglePayment[] singlePayment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool periodicPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("periodicPayment", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Periodic Payment")]
        public PeriodicPayment periodicPayment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool marketFixedRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("marketFixedRate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Market fixed rate")]
        public EFS_Decimal marketFixedRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDelaySpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentDelay", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "paymentDelay")]
        public EFS_Boolean paymentDelay;
        #endregion Members
    }
    #endregion FeeLeg
    #region FixedAmountCalculation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FixedAmountCalculation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("calculationAmount", Order = 1)]
        [ControlGUI(Name = "Amount")]
        public Money calculationAmount;
        [System.Xml.Serialization.XmlElementAttribute("fixedRate", Order = 2)]
        [ControlGUI(Name = "Market fixed rate")]
        public FixedRate fixedRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public DayCountFractionEnum dayCountFraction;
        #endregion Members
    }
    #endregion FixedAmountCalculation
    #region FixedRateReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FixedRateReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion FixedRateReference
    #region FloatingAmountEvents
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FloatingAmountEvents : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool failureToPayPrincipalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("failureToPayPrincipal", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Failure to pay principal")]
        public Empty failureToPayPrincipal;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interestShortfallSpecified;
        [System.Xml.Serialization.XmlElementAttribute("interestShortfall", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Interest shortfall")]
        public InterestShortFall interestShortfall;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool writedownSpecified;
        [System.Xml.Serialization.XmlElementAttribute("writedown", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Writedown")]
        public Empty writedown;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool floatingAmountProvisionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingAmountProvisions", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Floating amount provisions")]
        public FloatingAmountProvisions floatingAmountProvisions;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalFixedPaymentsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalFixedPayments", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Additional fixed payments")]
        public AdditionalFixedPayments additionalFixedPayments;
        #endregion Members
    }
    #endregion FloatingAmountEvents
    #region FloatingAmountProvisions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FloatingAmountProvisions
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool WACCapInterestProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("WACCapInterestProvision", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "WAC Cap interest provision")]
        public Empty WACCapInterestProvision;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepUpProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("stepUpProvision", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Step up provision")]
        public Empty stepUpProvision;
        #endregion Members
    }
    #endregion FloatingAmountProvisions

    #region GeneralTerms
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class GeneralTerms : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool effectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
        public AdjustableDate2 effectiveDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool scheduledTerminationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("scheduledTerminationDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Scheduled Termination Date")]
        public DeprecatedScheduledTerminationDate scheduledTerminationDate;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 3)]
        [ControlGUI(Name = "Floating rate payer (the 'Seller')")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 4)]
        [ControlGUI(Name = "Fixed rate payer (the 'Buyer')")]
        [ControlGUI(Name = "Buyer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dateAdjustmentsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dateAdjustments", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Day Adjustments")]
        public BusinessDayAdjustments dateAdjustments;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information Terms", IsVisible = false)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Type")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemReferenceInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("referenceInformation", typeof(ReferenceInformation),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public ReferenceInformation itemReferenceInformation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemIndexReferenceInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexReferenceInformation", typeof(IndexReferenceInformation),Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public IndexReferenceInformation itemIndexReferenceInformation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemBasketReferenceInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basketReferenceInformation", typeof(BasketReferenceInformation),Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BasketReferenceInformation itemBasketReferenceInformation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalTermSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalTerm",Order=9)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Information Terms")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Additional Terms")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Additional Terms", IsClonable = true, IsChild = false)]
        public AdditionalTerm[] additionalTerm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool substitutionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("substitution", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "substitution")]
        public Empty substitution;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool modifiedEquityDeliverySpecified;
        [System.Xml.Serialization.XmlElementAttribute("modifiedEquityDelivery", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "modifiedEquityDelivery")]
        public Empty modifiedEquityDelivery;
        #endregion Members
        #region Constructors
        public GeneralTerms()
        {
            itemIndexReferenceInformation = new IndexReferenceInformation();
            itemBasketReferenceInformation = new BasketReferenceInformation();
            itemReferenceInformation = new ReferenceInformation();
        }
        #endregion Constructors
    }
    #endregion GeneralTerms

    #region IndexAnnexSource
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class IndexAnnexSource : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string indexAnnexSourceScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
        #region Constructors
        public IndexAnnexSource()
        {
            indexAnnexSourceScheme = "http://www.fpml.org/coding-scheme/cdx-index-annex-source-1-0";
        }
        #endregion Constructors
    }
    #endregion IndexAnnexSource
    #region IndexId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class IndexId : ItemGUI, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue indexId;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 300)]
        public string indexIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
        #endregion Members
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion IndexId
    #region IndexName
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class IndexName : ItemGUI
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue indexName;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "scheme", Width = 300)]
        public string indexNameScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
    }
    #endregion IndexName
    #region IndexReferenceInformation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class IndexReferenceInformation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexName", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index name")]
        public IndexName indexName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexId",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index Id", IsClonable = true)]
        public IndexId[] indexId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexSeriesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexSeries", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CDS index series identifier")]
        public EFS_PosInteger indexSeries;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexAnnexVersionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexAnnexVersion", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CDS index series version identifier")]
        public EFS_PosInteger indexAnnexVersion;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexAnnexDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexAnnexDate", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CDS index series annex date")]
        public EFS_Date indexAnnexDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexAnnexSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexAnnexSource", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CDS index series annex source", Width = 200)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public IndexAnnexSource indexAnnexSource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool excludedReferenceEntitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("excludedReferenceEntity",Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Excluded reference entity")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Excluded reference entity", IsClonable = true, IsChild = true)]
        public LegalEntity[] excludedReferenceEntity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trancheSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tranche", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Tranche")]
        public Tranche tranche;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settledEntityMatrixSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settledEntityMatrix", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settled entity matrix")]
        public SettledEntityMatrix settledEntityMatrix;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
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
        public IndexReferenceInformation()
        {
            indexAnnexDate = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion IndexReferenceInformation
    #region InitialPayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class InitialPayment : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
        [ControlGUI(Name = "Payer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference payerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        [ControlGUI(Name = "Receiver")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference receiverPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustablePaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustablePaymentDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjustable payment date")]
        public EFS_Date adjustablePaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted payment date")]
        public EFS_Date adjustedPaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("paymentAmount", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment amount", IsVisible = false)]
        public Money paymentAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment amount")]
        public bool FillBalise;
        #endregion Members
        #region Constructors
        public InitialPayment()
        {
            adjustablePaymentDate = new EFS_Date();
            adjustedPaymentDate = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion InitialPayment
    #region InterestShortFall
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class InterestShortFall : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("interestShortfallCap", Order = 1)]
        [ControlGUI(Name = "Interest shortfall cap", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public InterestShortfallCapEnum interestShortfallCap;
        [System.Xml.Serialization.XmlElementAttribute("payerParcompoundingtyReference", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Compounding")]
        public EFS_Boolean compounding;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateSource", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate source of a variable cap")]
        public FloatingRateIndex rateSource;
        #endregion Members
    }
    #endregion InterestShortFall

    #region LoanParticipation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class LoanParticipation : PCDeliverableObligationCharac
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool qualifyingParticipationSellerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("qualifyingParticipationSeller", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Qualifying participation seller")]
        public EFS_MultiLineString qualifyingParticipationSeller;
        #endregion Members
    }
    #endregion LoanParticipation

    #region MatrixSource
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MatrixSource : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string settledEntityMatrixSourceScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
        #region Constructors
        public MatrixSource()
        {
            settledEntityMatrixSourceScheme = "http://www.fpml.org/coding-scheme/settled-entity-matrix-source-1-0";
        }
        #endregion Constructors
    }
    #endregion MatrixSource
    #region MultipleValuationDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MultipleValuationDates : SingleValuationDate
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessDaysThereafterSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessDaysThereafter", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business days there after")]
        public EFS_PosInteger businessDaysThereafter;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool numberValuationDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("numberValuationDates", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Number valuation dates")]
        public EFS_PosInteger numberValuationDates;
        #endregion Members
    }
    #endregion MultipleValuationDates

    #region NotDomesticCurrency
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class NotDomesticCurrency : ItemGUI
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
    }
    #endregion NotDomesticCurrency

    #region Obligations
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    // EG 20140702 New build FpML4.4
    public class Obligations : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("category", Order = 1)]
        [ControlGUI(Name = "Category", Width = 200)]
        public ObligationCategoryEnum category;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notSubordinatedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notSubordinated", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not subordinated")]
        public Empty notSubordinated;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool specifiedCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("specifiedCurrency", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Specified currency")]
        public SpecifiedCurrency specifiedCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notSovereignLenderSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notSovereignLender", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not sovereign lender")]
        public Empty notSovereignLender;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("notDomesticCurrency", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Not domestic currency")]
        public NotDomesticCurrency notDomesticCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticLawSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notDomesticLaw", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not domestic law")]
        public Empty notDomesticLaw;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool listedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("listed", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Listed")]
        public Empty listed;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticIssuanceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notDomesticIssuance", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not domestic issuance")]
        public Empty notDomesticIssuance;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = false, Name = "Obligation liability")]
        public EFS_RadioChoice liability;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityGeneralFundSpecified;
        [System.Xml.Serialization.XmlElementAttribute("generalFundObligationLiability", typeof(Empty),Order=9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityGeneralFund;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityFullFaithAndCreditSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fullFaithAndCreditObLiability", typeof(Empty),Order=10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityFullFaithAndCredit;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityRevenueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("revenueObligationLiability", typeof(Empty),Order=11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityRevenue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notContingentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notContingent", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not contingent")]
        public Empty notContingent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool excludedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("excluded", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Excluded")]
        public EFS_MultiLineString excluded;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool othReferenceEntityObligationsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("othReferenceEntityObligations", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Other reference entity Obligations")]
        public EFS_MultiLineString othReferenceEntityObligations;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool designatedPrioritySpecified;
        [System.Xml.Serialization.XmlElementAttribute("designatedPriority", Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Designated priority")]
        public Lien designatedPriority;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementOnlySpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementOnly", Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "CashSettlement only")]
        public Empty cashSettlementOnly;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool deliveryOfCommitmentsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryOfCommitments", Order = 17)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Delivery of commitments")]
        public Empty deliveryOfCommitments;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool continuitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("continuity", Order = 18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Continuity")]
        public Empty continuity;

        #endregion Members
        #region Constructors
        public Obligations()
        {
            liabilityNone = new Empty();
            liabilityFullFaithAndCredit = new Empty();
            liabilityGeneralFund = new Empty();
            liabilityRevenue = new Empty();
        }
        #endregion Constructors
    }
    #endregion Obligations

    #region PCDeliverableObligationCharac
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(LoanParticipation))]
    public class PCDeliverableObligationCharac : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partialCashSettlementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("partialCashSettlement", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Partial cash settlement")]
        public Empty partialCashSettlement;
        #endregion Members
    }
    #endregion PCDeliverableObligationCharac
    #region PeriodicPayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PeriodicPayment : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment frequency")]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firstPeriodStartDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstPeriodStartDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First period start date")]
        public EFS_Date firstPeriodStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firstPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstPaymentDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First payment date")]
        public EFS_Date firstPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastRegularPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("lastRegularPaymentDate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last regular payment date")]
        public EFS_Date lastRegularPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rollConventionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rollConvention", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Roll convention")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public RollConventionEnum rollConvention;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Fixed amount")]
        public EFS_RadioChoice fixedAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixedAmountDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedAmount", typeof(Money),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Money fixedAmountDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixedAmountCalculationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedAmountCalculation", typeof(FixedAmountCalculation),Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public FixedAmountCalculation fixedAmountCalculation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedPaymentDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDates",Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted payment dates")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted payment dates", IsClonable = true, IsChild = false, MinItem = 0)]
        public AdjustedPaymentDates[] adjustedPaymentDates;
        #endregion Members
        #region Constructors
        public PeriodicPayment()
        {
            firstPeriodStartDate = new EFS_Date();
            firstPaymentDate = new EFS_Date();
            lastRegularPaymentDate = new EFS_Date();

            fixedAmountDefine = new Money();
            fixedAmountCalculation = new FixedAmountCalculation();
        }
        #endregion Constructors
    }
    #endregion PeriodicPayment
    #region PhysicalSettlementPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PhysicalSettlementPeriod : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, IsDisplay = false, Name = "Type")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemNotBusinessDaysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessDaysNotSpecified", typeof(Empty),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty itemNotBusinessDays;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemBusinessDaysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessDays", typeof(EFS_NonNegativeInteger),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", Width = 70)]
        public EFS_NonNegativeInteger itemBusinessDays;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemMaximumBusinessDaysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("maximumBusinessDays", typeof(EFS_NonNegativeInteger),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", Width = 70)]
        public EFS_NonNegativeInteger itemMaximumBusinessDays;
        #endregion Members
        #region Constructors
        public PhysicalSettlementPeriod()
        {
            itemNotBusinessDays = new Empty();
            itemBusinessDays = new EFS_NonNegativeInteger();
            itemMaximumBusinessDays = new EFS_NonNegativeInteger();
        }
        #endregion Constructors
    }
    #endregion PhysicalSettlementPeriod
    #region PhysicalSettlementTerms
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PhysicalSettlementTerms : SettlementTerms
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool physicalSettlementPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("physicalSettlementPeriod", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Physical settlement period")]
        public PhysicalSettlementPeriod physicalSettlementPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool deliverableObligationsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliverableObligations", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Deliverable obligations")]
        public DeliverableObligations deliverableObligations;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool escrowSpecified;
        [System.Xml.Serialization.XmlElementAttribute("escrow", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Escrow")]
        public EFS_Boolean escrow;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sixtyBusinessDaySettlementCapSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sixtyBusinessDaySettlementCap", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sixty business day settlement cap")]
        public EFS_Boolean sixtyBusinessDaySettlementCap;
        #endregion Members
    }
    #endregion PhysicalSettlementTerms
    #region ProtectionTerms
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ProtectionTerms : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("calculationAmount", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation amount", IsVisible = false)]
        public Money calculationAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool creditEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("creditEvents", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation amount")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Credit events")]
        public CreditEvents creditEvents;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool obligationsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("obligations", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Obligations")]
        public Obligations obligations;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool floatingAmountEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingAmountEvents", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Floating amount Events")]
        public FloatingAmountEvents floatingAmountEvents;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Id efs_id;
        #endregion Members
        #region Accessors
        #region Id
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
        #endregion id
        #endregion Accessors
    }
    #endregion ProtectionTerms
    #region ProtectionTermsReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ProtectionTermsReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion ProtectionTermsReference

    #region ReferenceInformation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ReferenceInformation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("referenceEntity", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference entity", IsVisible = false)]
        public LegalEntity referenceEntity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference entity")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Obligations")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemNoReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("noReferenceObligation", typeof(Empty),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty itemNoReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemUnknownReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unknownReferenceObligation", typeof(Empty),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty itemUnknownReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("referenceObligation", typeof(ReferenceObligation),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Obligation", IsClonable = true, IsChild = true)]
        public ReferenceObligation[] itemReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool allGuaranteesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("allGuarantees", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "All guarantees")]
        public EFS_Boolean allGuarantees;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool referencePriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("referencePrice", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference price")]
        public EFS_Decimal referencePrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool referencePolicySpecified;
        [System.Xml.Serialization.XmlElementAttribute("referencePolicy", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Reference policy")]
        public Empty referencePolicy;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool securedListSpecified;
        [System.Xml.Serialization.XmlElementAttribute("securedList", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secured list")]
        public EFS_Boolean securedList;
        #endregion Members
        #region Constructors
        public ReferenceInformation()
        {
            itemNoReference = new Empty();
            itemUnknownReference = new Empty();
            itemReference = new ReferenceObligation[1] { new ReferenceObligation() };
        }
        #endregion Constructors
    }
    #endregion ReferenceInformation
    #region ReferenceObligation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ReferenceObligation : ItemGUI, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Underlying asset")]
        public EFS_RadioChoice underlyingAsset;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyingAssetBondSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bond", typeof(Bond),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Bond underlyingAssetBond;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyingAssetConvertibleBondSpecified;
        [System.Xml.Serialization.XmlElementAttribute("convertibleBond", typeof(ConvertibleBond),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public ConvertibleBond underlyingAssetConvertibleBond;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyingAssetLoanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("loan", typeof(Loan),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Loan underlyingAssetLoan;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyingAssetMortgageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("mortgage", typeof(Mortgage),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Mortgage underlyingAssetMortgage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Primary obligor")]
        public EFS_RadioChoice primaryObligor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool primaryObligorNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty primaryObligorNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool primaryObligorLegalEntitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("primaryObligor", typeof(LegalEntity),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public LegalEntity primaryObligorLegalEntity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool primaryObligorLegalEntityReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("primaryObligorReference", typeof(LegalEntityReference),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public LegalEntityReference primaryObligorLegalEntityReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool guarantorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("guarantor", typeof(LegalEntity),Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Guarantors")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Guarantor", IsClonable = true, IsChild = true)]
        public LegalEntity[] guarantor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool guarantorReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("guarantorReference", typeof(LegalEntityReference),Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Guarantors Reference")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Guarantor", IsClonable = true, IsChild = false)]
        public LegalEntityReference[] guarantorReference;
        #endregion Members
        #region Constructors
        public ReferenceObligation()
        {
            underlyingAssetBond = new Bond();
            underlyingAssetConvertibleBond = new ConvertibleBond();
            underlyingAssetLoan = new Loan();
            underlyingAssetMortgage = new Mortgage();

            primaryObligorNone = new Empty();
            primaryObligorLegalEntity = new LegalEntity();
            primaryObligorLegalEntityReference = new LegalEntityReference();
        }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion ReferenceObligation
    #region ReferencePair
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ReferencePair : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("referenceEntity", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference entity", IsVisible = false)]
        public LegalEntity referenceEntity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference entity")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Obligation")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("referenceObligation", typeof(ReferenceObligation),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public ReferenceObligation itemReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemNoReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("noReferenceObligation", typeof(Empty),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty itemNoReference;

        [System.Xml.Serialization.XmlElementAttribute("entityType", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity type", IsVisible = false)]
        public EntityType entityType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity type")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion ReferencePair
    #region ReferencePool
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ReferencePool : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("referencePoolItem", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference pool item", IsClonable = true)]
        public ReferencePoolItem[] referencePoolItem;
        #endregion Members
    }
    #endregion ReferencePool

    #region ReferencePoolItem
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ReferencePoolItem
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("constituentWeight", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Constituent weight", IsVisible = false)]
        public ConstituentWeight constituentWeight;
        [System.Xml.Serialization.XmlElementAttribute("referencePair", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Constituent weight")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference pair", IsVisible = false)]
        public ReferencePair referencePair;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool protectionTermsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("protectionTermsReference", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Protection terms reference")]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference pair")]
        public ProtectionTermsReference protectionTermsReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementTermsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementTermsReference", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement terms reference")]
        public SettlementTermsReference settlementTermsReference;
        #endregion Members
    }
    #endregion ReferencePoolItem

    #region ScheduledTerminationDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ScheduledTerminationDate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice adjustableOrRelativeDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDateAdjustableDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", typeof(AdjustableDate2),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        public AdjustableDate2 adjustableOrRelativeDateAdjustableDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDateRelativeDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDate", typeof(Interval),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date", IsVisible = true, IsCopyPaste = true)]
        public Interval adjustableOrRelativeDateRelativeDate;
        #endregion Members
        #region Constructors
        public ScheduledTerminationDate()
        {
            adjustableOrRelativeDateAdjustableDate = new AdjustableDate2();
            adjustableOrRelativeDateRelativeDate = new Interval();

        }
        #endregion Constructors
    }
    #endregion ScheduledTerminationDate
    #region SettledEntityMatrix
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SettledEntityMatrix : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("matrixSource", Order = 1)]
        public MatrixSource matrixSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool publicationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("publicationDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Publication date")]
        public EFS_Date publicationDate;
        #endregion Members
    }
    #endregion SettledEntityMatrix
    #region SettlementTerms
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CashSettlementTerms))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PhysicalSettlementTerms))]
    public class SettlementTerms : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementCurrency", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency settlementCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Id efs_id;
        #endregion Members
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
    }
    #endregion SettlementTerms
    #region SettlementTermsReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SettlementTermsReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion SettlementTermsReference
    #region SinglePayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SinglePayment : ItemGUI, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("adjustablePaymentDate", Order = 1)]
        [ControlGUI(Name = "Adjustable payment date")]
        public EFS_Date adjustablePaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted payment date")]
        public EFS_Date adjustedPaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("fixedAmount", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed amount", IsVisible = false)]
        public Money fixedAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed amount")]
        public bool FillBalise;
        #endregion Members

        #region Constructors
        public SinglePayment()
        {
            adjustablePaymentDate = new EFS_Date();
            adjustedPaymentDate = new EFS_Date();

        }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods

    }
    #endregion SinglePayment
    #region SingleValuationDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MultipleValuationDates))]
    public class SingleValuationDate : ItemGUI
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessDaysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessDays", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business days")]
        public EFS_NonNegativeInteger businessDays;
    }
    #endregion SingleValuationDate
    #region SpecifiedCurrency
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SpecifiedCurrency : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("currency", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        public Currency[] currency;
        #endregion Members
    }
    #endregion SpecifiedCurrency

    #region Tranche
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Tranche : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("attachmentPoint", Order = 1)]
        [ControlGUI(Name = "Attachment point")]
        public EFS_Decimal attachmentPoint;
        [System.Xml.Serialization.XmlElementAttribute("exhaustionPoint", Order = 2)]
        [ControlGUI(Name = "Exhaustion point")]
        public EFS_Decimal exhaustionPoint;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool incurredRecoveryApplicableSpecified;
        [System.Xml.Serialization.XmlElementAttribute("incurredRecoveryApplicable", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Incurred recovery")]
        public EFS_Boolean incurredRecoveryApplicable;
        #endregion Members
    }
    #endregion Tranche

    #region ValuationDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ValuationDate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Valuation date")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemSingleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("singleValuationDate", typeof(SingleValuationDate),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public SingleValuationDate itemSingle;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemMultipleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multipleValuationDates", typeof(MultipleValuationDates),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public MultipleValuationDates itemMultiple;
        #endregion Members
        #region Constructors
        public ValuationDate()
        {
            itemSingle = new SingleValuationDate();
            itemMultiple = new MultipleValuationDates();
        }
        #endregion Constructors
    }
    #endregion ValuationDate

}

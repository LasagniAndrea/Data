#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;

using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI;

using FpML.Enum;

using FpML.v42.Enum;
using FpML.v42.Shared;
using FpML.v42.Eqs;
#endregion using directives
/// <revision>
///     <version>1.2.0</version><date>20071003</date><author>EG</author>
///     <comment>
///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent for all method DisplayArray (used to determine REGEX type for derived classes
///     </comment>
/// </revision>

namespace FpML.v42.Asset
{
    #region ActualAmount  Glop Fictif use in Price
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type:</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ActualAmount : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool grossPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("grossPrice", typeof(ActualPrice),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "grossPrice")]
        public ActualPrice grossPrice;

        [System.Xml.Serialization.XmlElementAttribute("netPrice", typeof(ActualPrice),Order=2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Net Price", IsVisible = true)]
        public ActualPrice netPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterestPrice", typeof(EFS_Decimal),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Net Price")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued Interest Price")]
        public EFS_Decimal accruedInterestPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxConversionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxConversion", typeof(FxConversion),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Conversion")]
        public FxConversion fxConversion;
		#endregion Members
	}
    #endregion ActualAmount
    #region ActualPrice
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>currency (zero or one occurrence; of the type Currency)</newpara>
    /// <newpara>amount (exactly one occurrence; of the type xsd:decimal)</newpara>
    /// <newpara>priceExpression (exactly one occurrence; of the type PriceExpressionEnum)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: Price</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ActualPrice : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("currency",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
		[System.Xml.Serialization.XmlElementAttribute("amount", Order = 2)]
        [ControlGUI(Name = "Amount", Width = 200)]
        public EFS_Decimal amount;
		[System.Xml.Serialization.XmlElementAttribute("priceExpression", Order = 3)]
        [ControlGUI(Name = "Expression", Width = 200)]
        public PriceExpressionEnum priceExpression;
		#endregion Members
	}
    #endregion ActualPrice

    #region Basket
    /// <summary>
    /// <newpara><b>Description :</b> A type describing the underlyer features of a basket swap. Each of the basket constituents 
    /// are described through an embedded component, the basketConstituentsType.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>openUnits (exactly one occurrence; of the type xsd:decimal) The number of units (index or securities) that
    /// constitute the underlyer of the swap. In the case of a basket swap, this element is used to reference both the
    /// number of basket units, and the number of each asset components of the basket when these are expressed in
    /// absolute terms.</newpara>
    /// <newpara>basketConstituent (one or more occurrences; of the type BasketConstituent) Describes each of the
    /// components of the basket.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: Underlyer</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Basket
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool openUnitsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("openUnits", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Open Units", Width = 100)]
        public EFS_Decimal openUnits;
        [System.Xml.Serialization.XmlElementAttribute("basketConstituent",Order=2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basket Constituent", IsMaster = true, IsChild = true)]
        public BasketConstituent[] basketConstituent;
		#endregion Members
	}
    #endregion Basket
    #region BasketConstituent
    /// <summary>
    /// <newpara><b>Description :</b> A type describing each of the constituents of a basket swap.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>underlyingAsset (exactly one occurrence; of the type UnderlyingAsset) Define the underlying asset when it is
    /// a listed security.</newpara>
    /// <newpara>constituentWeight (zero or one occurrence; of the type ConstituentWeight) Specifies the weight of each of
    /// the underlyer constituent within the basket, either in absolute or relative terms. This is an optional component,
    /// as certain swaps do not specify a specific weight for each of their basket constituents.</newpara>
    /// <newpara>dividendPayout (zero or one occurrence; of the type DividendPayout) Specifies the dividend payout ratio
    /// associated with an equity underlyer. A basket swap can have different payout ratios across the various
    /// underlying constituents. In certain cases the actual ratio is not known on trade inception, and only general
    /// conditions are then specified.</newpara>
    /// <newpara>underlyerPrice (zero or one occurrence; of the type Price)</newpara>
    /// <newpara>underlyerNotional (zero or one occurrence; of the type Money)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: </newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class BasketConstituent
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("bond", typeof(Bond),Order=1)]
		[System.Xml.Serialization.XmlElementAttribute("cash", typeof(Cash), Order = 1)]
		[System.Xml.Serialization.XmlElementAttribute("convertibleBond", typeof(ConvertibleBond), Order = 1)]
		[System.Xml.Serialization.XmlElementAttribute("deposit", typeof(Deposit), Order = 1)]
		[System.Xml.Serialization.XmlElementAttribute("equity", typeof(EquityAsset), Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedFund", typeof(ExchangeTradedFund),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("future", typeof(Future),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("fxRate", typeof(FxRateAsset),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("index", typeof(Index),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("mutualFund", typeof(MutualFund),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("rateIndex", typeof(RateIndex),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleCreditDefaultSwap", typeof(SimpleCreditDefaultSwap),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleFra", typeof(SimpleFra),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleIrSwap", typeof(SimpleIRSwap),Order=1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Asset", IsVisible = false)]
        public UnderlyingAsset underlyingAsset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool constituentWeightSpecified;
        [System.Xml.Serialization.XmlElementAttribute("constituentWeight", Order=2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Asset")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Constituent Weight")]
        public ConstituentWeight constituentWeight;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPayoutSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendPayout", Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend Payout")]
        public DividendPayout dividendPayout;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerPriceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("underlyerPrice", Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer Price")]
        public Price underlyerPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerNotionalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("underlyerNotional", Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer Notional")]
        public Money underlyerNotional;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerSpreadSpecified;
		[System.Xml.Serialization.XmlElementAttribute("underlyerSpread", Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread schedule reference")]
        public SpreadScheduleReference underlyerSpread;
		#endregion Members
    }
    #endregion BasketConstituent
    #region Bond
    /// <summary>
    /// <newpara><b>Description :</b> Defines the underlying asset when it is a bond.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type UnderlyingAsset)</newpara>
    /// <newpara>• A type describing the basic components of a security of index underlyer.</newpara>
    /// <newpara>relatedExchangeId (zero or one occurrence; of the type ExchangeId) A short form unique identifier for a
    /// related exchange. If the element is not present then the exchange shall be the primary exchange on which
    /// listed futures and options on the underlying are listed. The term "Exchange" is assumed to have the meaning
    /// as defined in the ISDA 2002 Equity Derivatives Definitions.</newpara>
    /// <newpara>issuerName (zero or one occurrence; of the type xsd:string) Specifies the issuer name of a fixed income
    /// security or convertible bond.</newpara>
    /// <newpara>couponRate (zero or one occurrence; of the type xsd:decimal) Specifies the coupon rate (expressed in
    /// percentage) of a fixed income security or convertible bond.</newpara>
    /// <newpara>maturity (zero or one occurrence; of the type xsd:date) The date when the principal amount of a security
    /// becomes due and payable.</newpara>
    /// <newpara>parValue (zero or one occurrence; of the type xsd:decimal) Specifies the nominal amount of a fixed income
    /// security or convertible bond.</newpara>
    /// <newpara>faceAmount (zero or one occurrence; of the type xsd:decimal) Specifies the total amount of the issue.
    /// Corresponds to the par value multiplied by the number of issued security.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: ReferenceObligation</newpara>
    ///<newpara><b>Substituted by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("bond", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public class Bond : UnderlyingAsset
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relatedExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relatedExchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id")]
        public ExchangeId relatedExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuerNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("issuerName", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer Name")]
        public EFS_String issuerName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool senioritySpecified;
        [System.Xml.Serialization.XmlElementAttribute("seniority", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repayment precedence of a debt instrument")]
        public CreditSeniority seniority;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool couponTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("couponType", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Type")]
        public CouponType couponType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool couponRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("couponRate", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Rate")]
        public EFS_Decimal couponRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maturitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturity", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity")]
        public EFS_Date maturity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool parValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("parValue", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Nominal amount")]
        public EFS_Decimal parValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool faceAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("faceAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Total amount of issue")]
        public EFS_Decimal faceAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Frequency")]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Bond";
		#endregion Members
		#region Constructors
        public Bond()
        {
            maturity = new EFS_Date();
        }
		#endregion Constructors
    }
    #endregion Bond

    #region Cash
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("cash", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class Cash : UnderlyingAsset
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Cash";

    }
    #endregion Cash
    #region Commission
    /// <summary>
    /// <newpara><b>Description :</b> A type describing the commission that will be charged for each of the hedge transactions.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>commissionDenomination (exactly one occurrence; of the type CommissionDenominationEnum) The type of
    /// units used to express a commission.</newpara>
    /// <newpara>commissionAmount (exactly one occurrence; of the type xsd:decimal) The commission amount, expressed
    /// in the way indicated by the commissionType element.</newpara>
    /// <newpara>currency (zero or one occurrence; of the type Currency) The currency in which an amount is denominated.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: Price</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Commission : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("commissionDenomination", Order=1)]
		[ControlGUI(Name = "denomination")]
        public CommissionDenominationEnum commissionDenomination;
		[System.Xml.Serialization.XmlElementAttribute("commissionAmount", Order=2)]
        [ControlGUI(Name = "amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal commissionAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("currency", Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
		#endregion Members
	}
    #endregion Commission
    #region ConstituentWeight
    /// <summary>
    /// <newpara><b>Description :</b> A type describing the weight of each of the underlyer constituent within the basket, 
    /// either in absolute or relative terms.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>openUnits (exactly one occurrence; of the type xsd:decimal) The number of units (index or securities) that
    /// constitute the underlyer of the swap. In the case of a basket swap, this element is used to reference both the
    /// number of basket units, and the number of each asset components of the basket when these are expressed in
    /// absolute terms.</newpara>
    /// <newpara>basketPercentage (zero or one occurrence; of the type xsd:decimal) The relative weight of each respective
    /// basket constituent, expressed in percentage.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: BasketConstituent</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ConstituentWeight : ItemGUI
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice constituentWeight;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool constituentWeightBasketPercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basketPercentage", typeof(EFS_Decimal),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Basket Percentage", IsVisible = true)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal constituentWeightBasketPercentage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool constituentWeightBasketAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basketAmount", typeof(Money),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Basket Amount", IsVisible = true)]
        public Money constituentWeightBasketAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool constituentWeightOpenUnitsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("openUnits", typeof(EFS_Decimal),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Open Units", IsVisible = true)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal constituentWeightOpenUnits;
		#endregion Members
		#region Constructors
        public ConstituentWeight()
        {
            constituentWeightBasketPercentage = new EFS_Decimal();
            constituentWeightBasketAmount = new Money();
            constituentWeightOpenUnits = new EFS_Decimal();
        }
		#endregion Constructors
    }
    #endregion ConstituentWeight
    #region ConvertibleBond
    /// <summary>
    /// <newpara><b>Description :</b> Defines the underlying asset when it is a convertible bond.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type UnderlyingAsset)</newpara>
    /// <newpara>• A type describing the basic components of a security of index underlyer.</newpara>
    /// <newpara>relatedExchangeId (zero or more occurrences; of the type ExchangeId) A short form unique identifier for a
    /// related exchange. If the element is not present then the exchange shall be the primary exchange on which
    /// listed futures and options on the underlying are listed. The term "Exchange" is assumed to have the meaning
    /// as defined in the ISDA 2002 Equity Derivatives Definitions.</newpara>
    /// <newpara>issuerName (zero or one occurrence; of the type xsd:string) Specifies the issuer name of a fixed income
    /// security or convertible bond.</newpara>
    /// <newpara>couponRate (zero or one occurrence; of the type xsd:decimal) Specifies the coupon rate (expressed in
    /// percentage) of a fixed income security or convertible bond.</newpara>
    /// <newpara>maturity (zero or one occurrence; of the type xsd:date) The date when the principal amount of a security
    /// becomes due and payable.</newpara>
    /// <newpara>parValue (zero or one occurrence; of the type xsd:decimal) Specifies the nominal amount of a fixed income
    /// security or convertible bond.</newpara>
    /// <newpara>faceAmount (zero or one occurrence; of the type xsd:decimal) Specifies the total amount of the issue.
    /// Corresponds to the par value multiplied by the number of issued security.</newpara>
    /// <newpara>underlyingEquity (exactly one occurrence; of the type UnderlyingAsset) Specifies the equity in which the
    /// comnvertible bond can be converted.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: ReferenceObligation</newpara>
    ///<newpara><b>Substituted by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("convertibleBond", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public class ConvertibleBond : UnderlyingAsset
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relatedExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relatedExchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id", IsClonable = true)]
        public ExchangeId[] relatedExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuerNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("issuerName", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer Name")]
        public EFS_String issuerName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool couponRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("couponRate", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Rate")]
        public EFS_Decimal couponRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maturitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturity", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity")]
        public EFS_Date maturity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool parValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("parValue", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Nominal amount")]
        public EFS_Decimal parValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool faceAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("faceAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Total amount of issue")]
        public EFS_Decimal faceAmount;
        [System.Xml.Serialization.XmlElementAttribute("underlyingEquity", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=7)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Equity", IsVisible = true)]
        public UnderlyingAsset underlyingEquity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Equity")]
        public bool FillBalise;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:ConvertibleBond";
		#endregion Members
		#region Constructors
        public ConvertibleBond()
        {
            maturity = new EFS_Date();
        }
		#endregion Constructors
    }
    #endregion ConvertibleBond
    #region CouponType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class CouponType : SchemeGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string couponTypeScheme;

        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;

        #region Constructors
        public CouponType()
        {
            couponTypeScheme = "http://www.fpml.org/coding-scheme/coupon-type-1-0";
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            CouponType clone = new CouponType();
            clone.couponTypeScheme = this.couponTypeScheme;
            clone.Value = this.Value;
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }

    #endregion CouponType

    #region Deposit
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("deposit", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class Deposit : UnderlyingAsset
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("term", Order=1)]
        [ControlGUI(Name = "Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval term;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Frequency")]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;
		#endregion Members
    }
    #endregion Deposit

    #region DividendPayout
    /// <summary>
    /// <newpara><b>Description :</b> A type describing the dividend payout ratio associated with an equity underlyer. 
    /// In certain cases the actual ratio is not known on trade inception, and only general conditions are then specified.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Either</newpara>
    /// <newpara>dividendPayoutRatio (exactly one occurrence; of the type xsd:decimal) Specifies the actual dividend payout
    /// ratio associated with the equity underlyer.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>dividendPayoutConditions (exactly one occurrence; of the type xsd:string) Specifies the dividend payout
    /// conditions that will be applied in the case where the actual ratio is not known, typically because of regulatory
    /// or legal uncertainties.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: BasketConstituent</newpara>
    ///<newpara>• Complex type: SingleUnderlyer</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class DividendPayout : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Dividend Payout")]
        public EFS_DropDownChoice dividendPayout;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPayoutRatioSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendPayoutRatio", typeof(EFS_Decimal),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dividend Payout Ratio", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public EFS_Decimal dividendPayoutRatio;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPayoutConditionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendPayoutConditions", typeof(EFS_MultiLineString),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dividend Payout Conditions", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 400)]
        public EFS_MultiLineString dividendPayoutConditions;
		#endregion Members
	}
    #endregion DividendPayout

    #region EquityAsset
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("equity", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public partial class EquityAsset : UnderlyingAsset
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relatedExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relatedExchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id", IsClonable = true)]
        public ExchangeId[] relatedExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionsExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("optionsExchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Options Exchange Id")]
        public ExchangeId optionsExchangeId;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:EquityAsset";
		#endregion Members
	}
    #endregion EquityAsset
    #region ExchangeTradedContract
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("exchangeTradedContractNearest", Namespace = "http://www.efs.org/2005/EFSmL-2-0",IsNullable = false)]
    public class ExchangeTradedContract : UnderlyingAsset
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relatedExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relatedExchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id")]
        public ExchangeId relatedExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionsExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("optionsExchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Options Exchange Id")]
        public ExchangeId optionsExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multiplierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multiplier", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer multiplier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contractReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contractReference", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Reference")]
        public EFS_String contractReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool expirationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("expirationDate", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration Date")]
        public AdjustableOrRelativeDate expirationDate;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:ExchangeTradedContract";
		#endregion Members
    }
    #endregion ExchangeTradedContract
    #region ExchangeTradedFund
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("exchangeTradedFund", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public class ExchangeTradedFund : UnderlyingAsset
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relatedExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relatedExchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id")]
        public ExchangeId relatedExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionsExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("optionsExchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Options Exchange Id")]
        public ExchangeId optionsExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fundManagerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fundManager", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [ControlGUI(Name = "Fund Manager")]
        public EFS_String fundManager;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:ExchangeTradedFund";
		#endregion Members
    }
    #endregion ExchangeTradedFund

    #region Future
    /// <summary>
    /// <newpara><b>Description :</b> Defines the underlying asset when it is a listed future contract.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type UnderlyingAsset)</newpara>
    /// <newpara>• A type describing the basic components of a security of index underlyer.</newpara>
    /// <newpara>relatedExchangeId (zero or one occurrence; of the type ExchangeId) A short form unique identifier for a
    /// related exchange. If the element is not present then the exchange shall be the primary exchange on which
    /// listed futures and options on the underlying are listed. The term "Exchange" is assumed to have the meaning
    /// as defined in the ISDA 2002 Equity Derivatives Definitions.</newpara>
    /// <newpara>optionsExchangeId (zero or one occurrence; of the type ExchangeId) A short form unique identifier for an
    /// exchange on which the reference option contract is listed. This is to address the case where the reference
    /// exchange for the future is different than the one for the option. The options Exchange is referenced on share
    /// options when Merger Elections are selected as Options Exchange Adjustment.</newpara>
    /// <newpara>multiplier (zero or one occurrence; of the type xsd:integer) Specifies the contract multiplier that can be
    /// associated with the number of units.</newpara>
    /// <newpara>futureContractReference (zero or one occurrence; of the type xsd:string) Specifies the future contract that
    /// can be referenced, besides the equity or index reference defined as part of the UnderlyerAsset type.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Substituted by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("future", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public class Future : UnderlyingAsset
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relatedExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relatedExchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id")]
        public ExchangeId relatedExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionsExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("optionsExchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Options Exchange Id")]
        public ExchangeId optionsExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multiplierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multiplier", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [ControlGUI(Name = "Multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer multiplier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool futureContractReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("futureContractReference", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=4)]
        [ControlGUI(Name = "Contract Reference")]
        public EFS_String futureContractReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maturitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturity", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration Date")]
        public EFS_Date maturity;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Future";
		#endregion Members
    }
    #endregion Future
    #region FutureId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class FutureId : SchemeGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string futureIdScheme;

        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;

        #region Methods
        #region Clone
        public object Clone()
        {
            FutureId clone = new FutureId();
            clone.futureIdScheme = this.futureIdScheme;
            clone.Value = this.Value;
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion FutureId
    #region FxConversion
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class FxConversion : ItemGUI
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice fxConversion;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxConversionAmountRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amountRelativeTo", typeof(Reference),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount Relative To", IsVisible = true)]
        [ControlGUI(Name = "value")]
        public Reference fxConversionAmountRelativeTo;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxConversionFxRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxRate", typeof(FxRate),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Rate", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public FxRate[] fxConversionFxRate;
		#endregion Members
        #region Constructors
        public FxConversion()
        {
            fxConversionAmountRelativeTo = new Reference();
            fxConversionFxRate = new FxRate[1] { new FxRate() };
        }
        #endregion Constructors
    }
    #endregion FxConversion
    #region FxRateAsset
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("fxRate", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class FxRateAsset : UnderlyingAsset
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Order=1)]
        public QuotedCurrencyPair quotedCurrencyPair;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateSourceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("rateSource", Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate source")]
        public FxSpotRateSource rateSource;
		#endregion Members
    }
    #endregion FxRateAsset

    #region Index
    /// <summary>
    /// <newpara><b>Description :</b> Defines the underlying asset when it is a financial index.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type UnderlyingAsset)</newpara>
    /// <newpara>• A type describing the basic components of a security of index underlyer.</newpara>
    /// <newpara>relatedExchangeId (zero or more occurrences; of the type ExchangeId) A short form unique identifier for a
    /// related exchange. If the element is not present then the exchange shall be the primary exchange on which
    /// listed futures and options on the underlying are listed. The term "Exchange" is assumed to have the meaning
    /// as defined in the ISDA 2002 Equity Derivatives Definitions.</newpara>
    /// <newpara>futureId (zero or one occurrence; with locally defined content) A short form unique identifier for the reference
    /// future contract in the case of an index underlyer.</newpara>
    /// <newpara>Element termDeposit is defined by the complex type TermDeposit</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Substituted by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("index", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public class Index : UnderlyingAsset
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relatedExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relatedExchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id", IsClonable = true)]
        public ExchangeId[] relatedExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool futureIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("futureId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Future Id")]
        public FutureId futureId;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Index";
		#endregion Members
    }
    #endregion Index

    #region MutualFund
    /// <summary>
    /// <newpara><b>Description :</b> Defines the underlying asset when it is a mutual fund.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type UnderlyingAsset)</newpara>
    /// <newpara>• A type describing the basic components of a security of index underlyer.</newpara>
    /// <newpara>openEndedFund (zero or one occurrence; of the type xsd:boolean) Boolean indicator to specify whether the
    /// mutual fund is an open-ended mutual fund.</newpara>
    /// <newpara>fundManager (zero or one occurrence; of the type xsd:string) Specifies the fund manager that is in charge of
    /// the fund.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Substituted by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("mutualFund", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class MutualFund : UnderlyingAsset
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool openEndedFundSpecified;
        [System.Xml.Serialization.XmlElementAttribute("openEndedFund", Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Open Ended")]
        public EFS_Boolean openEndedFund;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fundManagerSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fundManager", Order=2)]
        [ControlGUI(Name = "Fund Manager")]
        public EFS_String fundManager;
		#endregion Members
    }
    #endregion MutualFund

    #region Price
    /// <summary>
    /// <newpara><b>Description :</b> A type describing the strike price of the equity swap.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>commission (zero or one occurrence; of the type Commission) This optional component specifies the
    /// commission to be charged for executing the hedge transactions.</newpara>
    /// <newpara>Either</newpara>
    /// <newpara>determinationMethod (exactly one occurrence; of the type xsd:string) Specifies the method according to
    /// which an amount or a date is determined.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>amountRelativeTo (exactly one occurrence; of the type AmountRelativeTo)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EquitySwapValuation</newpara>
    ///<newpara>• Complex type: BasketConstituent</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///<newpara>• Complex type: EquitySwapValuation</newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquitySwapValuationPrice))]
    public partial class Price : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool commissionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("commission", Order=1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Commission")]
        public Commission commission;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool grossPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("grossPrice", typeof(ActualPrice),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "grossPrice")]
        public ActualPrice grossPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice price;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceDeterminationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("determinationMethod", typeof(EFS_MultiLineString),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Determination Method", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 500)]
        public EFS_MultiLineString priceDeterminationMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceAmountRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amountRelativeTo", typeof(Reference),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount Relative To", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public Reference priceAmountRelativeTo;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceNetPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("netPrice", typeof(ActualPrice),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Net Price", IsVisible = true)]
        public ActualPrice priceNetPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterestPrice", typeof(EFS_Decimal),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued Interest Price")]
        public EFS_Decimal accruedInterestPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxConversionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxConversion", typeof(FxConversion),Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Conversion")]
        public FxConversion fxConversion;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Net Price")]
        public bool FillBalise;
		#endregion Members
	}
    #endregion Price

    #region RateIndex
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("rateIndex", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class RateIndex : UnderlyingAsset
    {
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("floatingRateIndex",Order=1)]
        [ControlGUI(Name = "Index", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public FloatingRateIndex floatingRateIndex;
		[System.Xml.Serialization.XmlElementAttribute("term",Order=2)]
        [ControlGUI(Name = "Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval term;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentFrequency",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Frequency")]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;
		#endregion Members
    }
    #endregion RateIndex

    #region SimpleCreditDefaultSwap
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("simpleCreditDefaultSwap", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class SimpleCreditDefaultSwap : UnderlyingAsset
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity")]
        public EFS_RadioChoice entity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityReferenceEntitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("referenceEntity", typeof(LegalEntity),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public LegalEntity entityReferenceEntity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityCreditEntityReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("creditEntityReference", typeof(Reference),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Reference entityCreditEntityReference;

		[System.Xml.Serialization.XmlElementAttribute("term",Order=3)]
        [ControlGUI(Name = "Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval term;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentFrequency",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Payment Frequency", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval paymentFrequency;
		#endregion Members
        #region Constructors
        public SimpleCreditDefaultSwap()
        {
            entityReferenceEntity = new LegalEntity();
            entityCreditEntityReference = new Reference();
        }
        #endregion Constructors
    }
    #endregion SimpleCreditDefaultSwap
    #region SimpleFra
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("simpleFra", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class SimpleFra : UnderlyingAsset
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("startTerm",Order=1)]
        [ControlGUI(Name = "Start Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval startTerm;
		[System.Xml.Serialization.XmlElementAttribute("endTerm",Order=2)]
        [ControlGUI(Name = "End Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval endTerm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;
		#endregion Members
    }
    #endregion SimpleFra
    #region SimpleIRSwap
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("simpleIrSwap", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class SimpleIRSwap : UnderlyingAsset
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("term",Order=1)]
        [ControlGUI(Name = "Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval term;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentFrequency",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Frequency")]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;
		#endregion Members
    }
    #endregion SimpleIRSwap
    #region SingleUnderlyer
    /// <summary>
    /// <newpara><b>Description :</b> A type describing the single underlyer of a swap.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>underlyingAsset (exactly one occurrence; of the type UnderlyingAsset) Define the underlying asset when it is
    /// a listed security.</newpara>
    /// <newpara>openUnits (exactly one occurrence; of the type xsd:decimal) The number of units (index or securities) that
    /// constitute the underlyer of the swap. In the case of a basket swap, this element is used to reference both the
    /// number of basket units, and the number of each asset components of the basket when these are expressed in
    /// absolute terms.</newpara>
    /// <newpara>dividendPayout (zero or one occurrence; of the type DividendPayout) Specifies the dividend payout ratio
    /// associated with an equity underlyer. A basket swap can have different payout ratios across the various
    /// underlying constituents. In certain cases the actual ratio is not known on trade inception, and only general
    /// conditions are then specified.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: Underlyer</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class SingleUnderlyer : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("bond", typeof(Bond),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("convertibleBond", typeof(ConvertibleBond),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("deposit", typeof(Deposit),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("equity", typeof(EquityAsset),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedFund", typeof(ExchangeTradedFund),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("fxRate", typeof(FxRateAsset),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("future", typeof(Future),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("index", typeof(Index),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("mutualFund", typeof(MutualFund),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("rateIndex", typeof(RateIndex),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleCreditDefaultSwap", typeof(SimpleCreditDefaultSwap),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleFra", typeof(SimpleFra),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleIrSwap", typeof(SimpleIRSwap),Order=1)]
        public UnderlyingAsset underlyingAsset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool openUnitsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("openUnits",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Open Units", Width = 150)]
        public EFS_Decimal openUnits;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPayoutSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendPayout",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend Payout")]
        public DividendPayout dividendPayout;
		#endregion Members
	}
    #endregion SingleUnderlyer

    #region SpreadScheduleReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class SpreadScheduleReference : HrefGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion SpreadScheduleReference

    #region Underlyer
    /// <summary>
    /// <newpara><b>Description :</b> A type describing the whole set of possible underlyers: single underlyers or 
    /// multiple underlyers, each of these having either security or index components.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Either</newpara>
    /// <newpara>singleUnderlyer (exactly one occurrence; of the type SingleUnderlyer) Describes the swap's underlyer when
    /// it has only one asset component.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>basket (exactly one occurrence; of the type Basket) Describes the swap's underlyer when it has multiple asset
    /// components.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EquityLeg</newpara>
    ///<newpara>• Complex type: EquityOption</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Underlyer : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        public EFS_RadioChoice underlyer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerSingleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("singleUnderlyer", typeof(SingleUnderlyer),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Single Underlyer", IsVisible = true)]
        public SingleUnderlyer underlyerSingle;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerBasketSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basket", typeof(Basket),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Underlyer Basket", IsVisible = true)]
        public Basket underlyerBasket;
		#endregion Members
	}
    #endregion Underlyer
    #region UnderlyingAsset
    /// <summary>
    /// <newpara><b>Description :</b> A type describing the basic components of a security of index underlyer.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>instrumentId (one or more occurrences; of the type InstrumentId)</newpara>
    /// <newpara>description (zero or one occurrence; of the type xsd:string) The long name of a security.</newpara>
    /// <newpara>currency (zero or one occurrence; of the type Currency) The currency in which an amount is denominated.</newpara>
    /// <newpara>exchangeId (zero or one occurrence; of the type ExchangeId)</newpara>
    /// <newpara>clearanceSystem (zero or one occurrence; of the type ClearanceSystem)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Element: bond</newpara>
    ///<newpara>• Element: convertibleBond</newpara>
    ///<newpara>• Element: equity</newpara>
    ///<newpara>• Element: exchangeTradedFund</newpara>
    ///<newpara>• Element: future</newpara>
    ///<newpara>• Element: index</newpara>
    ///<newpara>• Element: mutualFund</newpara>
    ///<newpara>• Element: underlyingAsset</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    // EG 20140702 Upd Interface (Add OTCmlId)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Bond))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Cash))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConvertibleBond))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Deposit))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTradedContract))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTradedFund))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Future))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxRateAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Index))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MutualFund))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RateIndex))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SimpleCreditDefaultSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SimpleFra))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SimpleIRSwap))]
    public partial class UnderlyingAsset : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("instrumentId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instrument Id", IsClonable = true, IsMaster = true)]
        public InstrumentId[] instrumentId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("description", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Description", Width = 600)]
        public EFS_String description;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exchangeId", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exchange Id")]
        public ExchangeId exchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool clearanceSystemSpecified;
        [System.Xml.Serialization.XmlElementAttribute("clearanceSystem", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Clearance System")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public ClearanceSystem clearanceSystem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool definitionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("definition", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "definition System")]
        public ProductReference definition;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id
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

        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }

		#endregion Members
	}
    #endregion UnderlyingAsset
}

#region Using Directives
using System;
using System.Reflection;

using EFS.GUI.Interface;
using EFS.GUI.Attributes;

using EfsML.Enum;

using FpML.Enum;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
using FpML.v42.Asset;
using FpML.v42.Eqs;
using FpML.v42.EqShared;
using FpML.v42.Shared;
#endregion Using Directives

namespace FpML.v42.Eqd
{
    #region BrokerEquityOption
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining the broker equity options.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type EquityDerivativeShortFormBase)</newpara>
    /// <newpara>A type for defining short form equity option basic features.</newpara>
    /// <newpara>deltaCrossed (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>brokerageFee (exactly one occurrence; of the type Money)</newpara>
    /// <newpara>brokerNotes (exactly one occurrence; of the type xsd:string)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("brokerEquityOption", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class BrokerEquityOption : EquityDerivativeShortFormBase
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("deltaCrossed", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "delta Crossed")]
        public EFS_Boolean deltaCrossed;
		[System.Xml.Serialization.XmlElementAttribute("brokerageFee", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Brokerage Fee", IsVisible = false)]
        public Money brokerageFee;
		[System.Xml.Serialization.XmlElementAttribute("brokerNotes", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Brokerage Fee")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Broker Notes", Width = 500)]
        public EFS_String brokerNotes;
		#endregion Members
	}
    #endregion BrokerEquityOption

    #region CalendarSpread
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining a calendar spread feature.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>expirationDateTwo (exactly one occurrence; of the type AdjustableOrRelativeDate)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• StrategyFeature</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class CalendarSpread : ItemGUI
    {
		[System.Xml.Serialization.XmlElementAttribute("expirationDateTwo", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration Date Two", IsVisible = true)]
        public AdjustableOrRelativeDate expirationDateTwo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration Date Two")]
        public bool FillBalise;
    }
    #endregion CalendarSpread

    #region EquityAmericanExercise
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining exercise procedures associated with an American style exercise of 
    /// an equity option. This entity inherits from the type SharedAmericanExercise.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type SharedAmericanExercise)</newpara>
    /// <newpara>• TBA</newpara>
    /// <newpara>latestExerciseTimeType (exactly one occurrence; of the type TimeTypeEnum) The latest time of day at
    /// which the equity option can be exercised, for example the official closing time of the exchange.</newpara>
    /// <newpara>equityExpirationTimeType (exactly one occurrence; of the type TimeTypeEnum) The time of day at which
    /// the equity option expires, for example the official closing time of the exchange.</newpara>
    /// <newpara>equityExpirationTime (zero or one occurrence; of the type BusinessCenterTime) The specific time of day at
    /// which the equity option expires.</newpara>
    /// <newpara>equityMultipleExercise (zero or one occurrence; of the type EquityMultipleExercise) The presence of this
    /// element indicates that the option may be exercised on different days. It is not applicable to European options.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EquityExercise</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquityAmericanExercise : SharedAmericanExercise
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool latestExerciseTimeTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("latestExerciseTimeType", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Lastest exercise time Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public TimeTypeEnum latestExerciseTimeType;
		[System.Xml.Serialization.XmlElementAttribute("equityExpirationTimeType", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration time", IsVisible = true)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "time of day")]
        public TimeTypeEnum equityExpirationTimeType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityExpirationTimeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("equityExpirationTime", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Specific time of day")]
        public BusinessCenterTime equityExpirationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityMultipleExerciseSpecified;
		[System.Xml.Serialization.XmlElementAttribute("equityMultipleExercise", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration time")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Multiple exercise")]
        public EquityMultipleExercise equityMultipleExercise;
		#endregion Members
	}
    #endregion EquityAmericanExercise
    #region EquityBermudanExercise
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining exercise procedures associated with a Bermudan style exercise 
    /// of an equity option.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type SharedAmericanExercise)</newpara>
    /// <newpara>bermudaExerciseDates (exactly one occurrence; of the type DateList) 
    /// List of Exercise Dates for a Bermuda option</newpara>
    /// <newpara>latestExerciseTimeType (zero or one occurrence; of the type TimeTypeEnum) 
    /// The latest time of day at which the equity option can be exercised, for example the official closing time of the exchange.</newpara>
    /// <newpara>equityExpirationTimeType (exactly one occurrence; of the type TimeTypeEnum)
    /// The time of day at which the equity option expires, for example the official closing time of the exchange.</newpara>
    /// <newpara>equityExpirationTime (zero or one occurrence; of the type BusinessCenterTime)
    /// The specific time of day at which the equity option expires.</newpara>
    /// <newpara>equityMultipleExercise (zero or one occurrence; of the type EquityMultipleExercise)
    /// The presence of this element indicates that the option may be exercised on different days. 
    /// It is not applicable to European options</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EquityExercise</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquityBermudaExercise : SharedAmericanExercise
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("bermudaExerciseDates", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Dates", IsVisible = true)]
        public DateList bermudaExerciseDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool latestExerciseTimeTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("latestExerciseTimeType", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Dates")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last exercise time")]
        public TimeTypeEnum latestExerciseTimeType;
		[System.Xml.Serialization.XmlElementAttribute("equityExpirationTimeType", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration time", IsVisible = true)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "time of day")]
        public TimeTypeEnum equityExpirationTimeType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityExpirationTimeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("equityExpirationTime", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Specific time of day")]
        public BusinessCenterTime equityExpirationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityMultipleExerciseSpecified;
		[System.Xml.Serialization.XmlElementAttribute("equityMultipleExercise", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration time")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Multiple exercise")]
        public EquityMultipleExercise equityMultipleExercise;
		#endregion Members
	}
    #endregion EquityBermudanExercise
    #region EquityDerivativeBase
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining the common features of equity derivatives.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type Product)
    /// The base type which all FpML products extend</newpara>
    /// <newpara>buyerPartyReference (exactly one occurrence; of the type PartyReference)
    /// A reference to the party that buys this instrument, ie. pays for this instrument and receives the rights defined by it. 
    /// See 2000 ISDA definitions Article 11.1 (b). In the case of FRAs this the fixed rate payer</newpara>
    /// <newpara>sellerPartyReference (exactly one occurrence; of the type PartyReference)
    /// A reference to the party that sells ("writes") this instrument, i.e. that grants the rights defined by this instrument 
    /// and in return receives a payment for it. See 2000 ISDA definitions Article 11.1 (a). 
    /// In the case of FRAs this is the floating rate payer</newpara>
    /// <newpara>optionType (exactly one occurrence; of the type OptionTypeEnum)
    /// The type of option transaction</newpara>
    /// <newpara>equityEffectiveDate (zero or one occurrence; of the type xsd:date)
    /// Effective date for a forward starting option</newpara>
    /// <newpara>underlyer (exactly one occurrence; of the type Underlyer)
    /// Specifies the underlying component, which can be either one or many and consists in either equity, 
    /// index or convertible bond component, or a combination of these</newpara>
    /// <newpara>notional (zero or one occurrence; of the type Money) The notional amount</newpara>
    /// <newpara>equityExercise (exactly one occurrence; of the type EquityExercise)
    /// The parameters for defining how the equity option can be exercised, how it is valued and how it is settled.</newpara>
    /// <newpara>fxFeature (zero or one occurrence; of the type FxFeature) A quanto or composite FX feature</newpara>
    /// <newpara>strategyFeature (zero or one occurrence; of the type StrategyFeature) A equity option simple strategy feature</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityDerivativeShortFormBase))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOptionTransactionSupplement))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BrokerEquityOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityDerivativeLongFormBase))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityForward))]
    public class EquityDerivativeBase : Product
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
        [ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference buyerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
		[ControlGUI(Name = "Seller")]
        public PartyReference sellerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("optionType", Order = 3)]
		[ControlGUI(Name = "Option type")]
        public OptionTypeEnum optionType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityEffectiveDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("equityEffectiveDate", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Effective Date")]
        public EFS_Date equityEffectiveDate;
		[System.Xml.Serialization.XmlElementAttribute("underlyer", Order = 5)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying component", IsVisible = false)]
        public Underlyer underlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notional", Order = 6)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Underlying component")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Notional amount")]
        public Money notional;
		[System.Xml.Serialization.XmlElementAttribute("equityExercise", Order = 7)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise", IsVisible = false)]
        public EquityExercise equityExercise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFeatureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxFeature", Order = 8)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fx feature")]
        public FxFeature fxFeature;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strategyFeatureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("strategyFeature", Order = 9)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strategy feature")]
        public StrategyFeature strategyFeature;
		#endregion Members

        #region Constructors
        public EquityDerivativeBase()
        {
            equityEffectiveDate = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion EquityDerivativeBase
    #region EquityDerivativeLongFormBase
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining the common features of equity derivatives.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type EquityDerivativeBase)
    ///  A type for defining the common features of equity derivatives</newpara>
    /// <newpara>dividendConditions (zero or one occurrence; of the type DividendConditions)</newpara>
    /// <newpara>methodOfAdjustment (exactly one occurrence; of the type MethodOfAdjustmentEnum)
    /// Defines how adjustments will be made to the contract should one or more of the extraordinary events occur.</newpara>
    /// <newpara>extraordinaryEvents (exactly one occurrence; of the type ExtraordinaryEvents)
    /// Where the underlying is shares, specifies events affecting the issuer of those shares that may require the terms 
    /// of the transaction to be adjusted.</newpara>
    /// <newpara>equityFeatures (zero or one occurrence; of the type EquityFeatures) A quanto or composite FX feature</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityForward))]
    public class EquityDerivativeLongFormBase : EquityDerivativeBase
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendConditionsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendConditions", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, IsLabel = false, Name = "Dividend Conditions")]
        public DividendConditions dividendConditions;
		[System.Xml.Serialization.XmlElementAttribute("methodOfAdjustment", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Adjustment method")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public MethodOfAdjustmentEnum methodOfAdjustment;
		[System.Xml.Serialization.XmlElementAttribute("extraordinaryEvents", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extraordinary Events", IsVisible = false)]
        public ExtraordinaryEvents extraordinaryEvents;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityFeaturesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("equityFeatures", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extraordinary Events")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, IsLabel = false, Name = "Equity features")]
        public EquityFeatures equityFeatures;
    }
    #endregion EquityDerivativeLongFormBase
    #region EquityDerivativeShortFormBase
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining short form equity option basic features.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type EquityDerivativeBase)
    ///  A type for defining the common features of equity derivatives</newpara>
    /// <newpara>strike (exactly one occurrence; of the type EquityStrike)</newpara>
    /// <newpara>spotPrice (zero or one occurrence; of the type xsd:decimal)</newpara>
    /// <newpara>numberOfOptions (exactly one occurrence; of the type xsd:decimal)</newpara>
    /// <newpara>equityPremium (exactly one occurrence; of the type EquityPremium)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///<newpara>• BrokerEquityOption</newpara>
    ///<newpara>• EquityOptionTransactionSupplement</newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOptionTransactionSupplement))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BrokerEquityOption))]
    public class EquityDerivativeShortFormBase : EquityDerivativeBase
    {
		[System.Xml.Serialization.XmlElementAttribute("strike", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike", IsVisible = false)]
        public EquityStrike strike;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotPriceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("spotPrice", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Spot price")]
        public EFS_Decimal spotPrice;
		[System.Xml.Serialization.XmlElementAttribute("numberOfOptions", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Number of options", Width = 100)]
        public EFS_Decimal numberOfOptions;
		[System.Xml.Serialization.XmlElementAttribute("equityPremium", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium", IsVisible = false)]
        public EquityPremium equityPremium;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        public bool FillBalise;

        public EquityDerivativeShortFormBase()
        {
            spotPrice = new EFS_Decimal();
        }
    }
    #endregion EquityDerivativeShortFormBase
    #region EquityEuropeanExercise
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining exercise procedures associated with a European style exercise 
    /// of an equity option.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type Exercise)</newpara>
    /// <newpara>• The abstract base class for all types which define way in which options may be exercised.</newpara>
    /// <newpara>expirationDate (exactly one occurrence; of the type AdjustableOrRelativeDate) The last day within an
    /// exercise period for an American style option. For a European style option it is the only day within the exercise
    /// period.</newpara>
    /// <newpara>equityExpirationTimeType (exactly one occurrence; of the type TimeTypeEnum) The time of day at which
    /// the equity option expires, for example the official closing time of the exchange.</newpara>
    /// <newpara>equityExpirationTime (zero or one occurrence; of the type BusinessCenterTime) The specific time of day at
    /// which the equity option expires.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityExercise</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquityEuropeanExercise : Exercise
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("expirationDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration date", IsVisible = false)]
        public AdjustableOrRelativeDate expirationDate;
		[System.Xml.Serialization.XmlElementAttribute("equityExpirationTimeType", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration time", IsVisible = false)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "time of day")]
        public TimeTypeEnum equityExpirationTimeType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityExpirationTimeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("equityExpirationTime", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Specific time of day")]
        public BusinessCenterTime equityExpirationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration time")]
        public bool FillBalise;
		#endregion Members
	}
    #endregion EquityEuropeanExercise
    #region EquityExercise
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining exercise procedures for equity options.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either</newpara>
    /// <newpara>equityEuropeanExercise (exactly one occurrence; of the type EquityEuropeanExercise) 
    /// The parameters for defining the expiration date and time for a European style equity option</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>equityAmericanExercise (exactly one occurrence; of the type EquityAmericanExercise) 
    /// The parameters for defining the exercise period for an American style equity option together 
    /// with the rules governing the quantity of the underlying that can be exercised on any given exercise date.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>equityBermudanExercise (exactly one occurrence; of the type EquityBermudanExercise) 
    /// The parameters for defining the exercise period for an Bermudan style equity option together 
    /// with the rules governing the quantity of the underlying that can be exercised on any given exercise date.</newpara>
    /// <newpara></newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either</newpara>
    /// <newpara>automaticExercise (exactly one occurrence; of the type xsd:boolean) 
    /// If true then each option not previously exercised will be deemed to be exercised at the expiration time 
    /// on the expiration date without service of notice unless the buyer notifies the seller that 
    /// it no longer wishes this to occur.</newpara>
    /// <newpara>Or</newpara> 
    /// <newpara>prePayment (exactly one occurrence; of the type PrePayment) Prepayment features for Forward.</newpara>
    /// <newpara></newpara> 
    /// <newpara>equityValuation (exactly one occurrence; of the type EquityValuation) 
    /// The parameters for defining when valuation of the underlying takes place.</newpara>
    /// <newpara>settlementDate (zero or one occurrence; of the type AdjustableOrRelativeDate)
    /// Date on which settlement of option premiums will occur</newpara>
    /// <newpara>settlementCurrency (exactly one occurrence; of the type Currency)
    /// The currency in which a cash settlement for non-deliverable forward and non-deliverable options</newpara>
    /// <newpara>settlementPriceSource (zero or one occurrence; of the type SettlementPriceSource)</newpara>
    /// <newpara>settlementType (exactly one occurrence; of the type SettlementTypeEnum) How the option will be settled.</newpara>
    /// <newpara>settlementMethodElectionDate (zero or one occurrence; of the type AdjustableOrRelativeDate)</newpara>
    /// <newpara>settlementMethodElectingPartyReference (zero or one occurrence; of the type PartyReference)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityDerivativeBase</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquityExercise : ItemGUI
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice equityExercise;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityExerciseAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("equityAmericanExercise", typeof(EquityAmericanExercise),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EquityAmericanExercise equityExerciseAmerican;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityExerciseBermudaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("equityBermudaExercise", typeof(EquityBermudaExercise),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EquityBermudaExercise equityExerciseBermuda;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityExerciseEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("equityEuropeanExercise", typeof(EquityEuropeanExercise),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EquityEuropeanExercise equityExerciseEuropean;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Method")]
        public EFS_RadioChoice exerciseMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseMethodAutomaticExerciseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("automaticExercise", typeof(EFS_Boolean),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "yes")]
        public EFS_Boolean exerciseMethodAutomaticExercise;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseMethodPrePaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("prePayment", typeof(PrePayment),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public PrePayment exerciseMethodPrePayment;

		[System.Xml.Serialization.XmlElementAttribute("equityValuation", Order = 6)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Valuation", IsVisible = false)]
        public EquityValuation equityValuation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementDate", Order = 7)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Valuation")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Date")]
        public AdjustableOrRelativeDate settlementDate;
		[System.Xml.Serialization.XmlElementAttribute("settlementCurrency", Order = 8)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "settlement Currency", Width = 75)]
        public Currency settlementCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementPriceSourceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementPriceSource", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Price Source")]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value")]
        public SettlementPriceSource settlementPriceSource;
		[System.Xml.Serialization.XmlElementAttribute("settlementType", Order = 10)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Settlement Type")]
        public SettlementTypeEnum settlementType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementMethodElectionDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementMethodElectionDate", Order = 11)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement method", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Election Date")]
        public AdjustableOrRelativeDate settlementMethodElectionDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementMethodElectingPartyReferenceSpecified;
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[System.Xml.Serialization.XmlElementAttribute("settlementMethodElectingPartyReference", Order = 12)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party reference")]
        public PartyReference settlementMethodElectingPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement method")]
        public bool FillBalise;
		#endregion Members
		#region Constructors
		public EquityExercise()
        {
            equityExerciseAmerican = new EquityAmericanExercise();
            equityExerciseBermuda = new EquityBermudaExercise();
            equityExerciseEuropean = new EquityEuropeanExercise();
            exerciseMethodAutomaticExercise = new EFS_Boolean();
            exerciseMethodPrePayment = new PrePayment();
		}
		#endregion Constructors
	}
    #endregion EquityExercise
    #region EquityForward
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining equity forwards.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type EquityDerivativeLongFormBase)
    /// A type for defining the common features of equity derivatives</newpara>
    /// <newpara>forwardPrice (zero or one occurrence; of the type Money) 
    /// The forward price per share, index or basket</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("equityForward", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class EquityForward : EquityDerivativeLongFormBase
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forwardPriceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("forwardPrice", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Forward price")]
        public Money forwardPrice;
		#endregion Members
	}
    #endregion EquityForward
    #region EquityMultipleExercise
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining the multiple exercise provisions of an American or Bermudan 
    /// style equity option.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>integralMultipleExercise (zero or one occurrence; of the type xsd:decimal) When multiple exercise is
    /// applicable and this element is present it specifies that the number of options that can be exercised on a given
    /// exercise date must either be equal to the value of this element or be an integral multiple of it.</newpara>
    /// <newpara>minimumNumberOfOptions (exactly one occurrence; of the type xsd:decimal) When multiple exercise is
    /// applicable this element specifies the minimum number of options that can be exercised on a given exercise
    /// date. If this element is not present then the minimum number is deemed to be 1.</newpara>
    /// <newpara>maximumNumberOfOptions (exactly one occurrence; of the type xsd:decimal) When multiple exercise is
    /// applicable this element specifies the maximum number of options that can be exercised on a given exercise
    /// date. If this element is not present then the maximum number is deemed to be the same as the number of
    /// options.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityAmericanExercise</newpara>
    ///<newpara>• EquityBermudanExercise</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquityMultipleExercise : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool integralMultipleExerciseSpecified;
		[System.Xml.Serialization.XmlElementAttribute("integralMultipleExercise", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "integral multiple exercise", Width = 100)]
        public EFS_Decimal integralMultipleExercise;
		[System.Xml.Serialization.XmlElementAttribute("minimumNumberOfOptions", Order = 2)]
        [ControlGUI(Name = "minimum number of options", Width = 100)]
        public EFS_Decimal minimumNumberOfOptions;
		[System.Xml.Serialization.XmlElementAttribute("maximumNumberOfOptions", Order = 3)]
        [ControlGUI(Name = "maximum number of options", Width = 100)]
        public EFS_Decimal maximumNumberOfOptions;
		#endregion Members
		#region Constructors
		public EquityMultipleExercise()
        {
            integralMultipleExercise = new EFS_Decimal();
		}
		#endregion Constructors
	}
    #endregion EquityMultipleExercise
    #region EquityOption
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining equity options.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type EquityDerivativeLongFormBase)</newpara>
    /// <newpara>• A Type for defining the common features of equity derivatives.</newpara>
    /// <newpara>strike (zero or one occurrence; of the type EquityStrike) 
    /// Defines whether it is a price or level at which the option has been, or will be, struck.</newpara>
    /// <newpara>spotPrice (zero or one occurrence; of the type xsd:decimal) 
    /// The price per share, index or basket observed on the trade or effective date.</newpara>
    /// <newpara>numberOfOptions (zero or one occurrence; of the type xsd:decimal) 
    /// The number of options comprised in the option transaction.</newpara>
    /// <newpara>optionEntitlement (exactly one occurrence; of the type xsd:decimal) 
    /// The number of shares per option comprised in the option transaction.</newpara>
    /// <newpara>equityPremium (exactly one occurrence; of the type EquityPremium) 
    /// The equity option premium payable by the buyer to the seller.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("equityOption", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class EquityOption : EquityDerivativeLongFormBase
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("strike", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike", IsVisible = false)]
        public EquityStrike strike;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotPriceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("spotPrice", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Spot price")]
        public EFS_Decimal spotPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool numberOfOptionsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("numberOfOptions", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Number of options", Width = 100)]
        public EFS_Decimal numberOfOptions;
		[System.Xml.Serialization.XmlElementAttribute("optionEntitlement", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "option Entitlement", Width = 100)]
        public EFS_Decimal optionEntitlement;
		[System.Xml.Serialization.XmlElementAttribute("equityPremium", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium", IsVisible = false)]
        public EquityPremium equityPremium;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        public bool FillBalise;
		#endregion Members
	}
    #endregion EquityOption
    #region EquityOptionTermination
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining Equity Option Termination.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>settlementAmountPaymentDate (exactly one occurrence; of the type AdjustableDate)</newpara>
    /// <newpara>settlementAmount (exactly one occurrence; of the type Money)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("equityOptionTermination", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class EquityOptionTermination : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("settlementAmountPaymentDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment date", IsVisible = false)]
        public AdjustableDate settlementAmountPaymentDate;
		[System.Xml.Serialization.XmlElementAttribute("settlementAmount", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money settlementAmount;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
		#endregion Members
	}
    #endregion EquityOptionTermination
    #region EquityOptionTransactionSupplement
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining equity option transaction supplements.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type EquityDerivativeLongFormBase)</newpara>
    /// <newpara>• A type for defining short form equity option basic features.</newpara>
    /// <newpara>exchangeLookAlike (zero or one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>exchangeTradedContractNearest (zero or one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>multipleExchangeIndexAnnexFallback (zero or one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>methodOfAdjustment (zero or one occurrence; of the type MethodOfAdjustmentEnum)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("equityOptionTransactionSupplement", Namespace = "http://www.fpml.org/2005/FpML-4-2",IsNullable = false)]
    public partial class EquityOptionTransactionSupplement : EquityDerivativeShortFormBase
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeLookAlikeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exchangeLookAlike", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "exchangeLookAlike")]
        public EFS_Boolean exchangeLookAlike;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeTradedContractNearestSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exchangeTradedContractNearest", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "exchangeTradedContractNearest")]
        public EFS_Boolean exchangeTradedContractNearest;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multipleExchangeIndexAnnexFallbackSpecified;
		[System.Xml.Serialization.XmlElementAttribute("multipleExchangeIndexAnnexFallback", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "multipleExchangeIndexAnnexFallback")]
        public EFS_Boolean multipleExchangeIndexAnnexFallback;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool methodOfAdjustmentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("methodOfAdjustment", Order =4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Method of adjustment")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public MethodOfAdjustmentEnum methodOfAdjustment;
		#endregion Members
	}
    #endregion EquityOptionTransactionSupplement
    #region EquityPremium
    /// <summary>
    /// <newpara><b>Description :</b> A type used to describe the amount paid for an equity option.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>payerPartyReference (exactly one occurrence; of the type PartyReference) 
    /// A reference to the party responsible for making the payments defined by this structure.</newpara>
    /// <newpara>receiverPartyReference (exactly one occurrence; of the type PartyReference) 
    /// A reference to the party that receives the payments corresponding to this structure.</newpara>
    /// <newpara>premiumType (zero or one occurrence; of the type PremiumTypeEnum) Forward start Premium type.</newpara> 
    /// <newpara>paymentAmount (zero or one occurrence; of the type Money) The currency amount of the payment.</newpara>
    /// <newpara>paymentDate (zero or one occurrence; of the type AdjustableDate) 
    /// The payment date. This date is subject to adjustment in accordance with any applicable business day convention.</newpara>
    /// <newpara>swapPremium (zero or one occurrence; of the type xsd:boolean) 
    /// Specifies whether or not the premium is to be paid in the style of payments under an interest rate swap contract.</newpara>
    /// <newpara>pricePerOption (zero or one occurrence; of the type xsd:decimal) 
    /// The amount of premium to be paid expressed as a function of the number of options.</newpara>
    /// <newpara>percentageOfNotional (zero or one occurrence; of the type xsd:decimal) 
    /// The amount of premium to be paid expressed as a percentage of the notional value of the transaction. 
    /// A percentage of 5% would be expressed as 0.05.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquityPremium : ItemGUI
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
        [ControlGUI(Name = "Payer")]
        public PartyReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        [ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public PartyReference receiverPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool premiumTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("premiumType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public PremiumTypeEnum premiumType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentAmount", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment amount")]
        public Money paymentAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentDate", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment date")]
        public AdjustableDate paymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool swapPremiumSpecified;
		[System.Xml.Serialization.XmlElementAttribute("swapPremium", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Swap premium")]
        public EFS_Boolean swapPremium;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pricePerOptionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("pricePerOption", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price per option")]
        public Money pricePerOption;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool percentageOfNotionalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("percentageOfNotional", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Percentage of notional")]
        public EFS_Decimal percentageOfNotional;
		#endregion Members
		#region Constructors
		public EquityPremium()
        {
            percentageOfNotional = new EFS_Decimal();
		}
		#endregion Constructors
	}
    #endregion EquityPremium
    #region EquityStrike
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining the strike price for an equity option. 
    /// The strike price is either: (i) in respect of an index option transaction, the level of the relevant index specified 
    /// or otherwise determined in the transaction; or (ii) in respect of a share option transaction, the price per share 
    /// specified or otherwise determined in the transaction.
    /// This can be expressed either as a percentage of notional amount or as an absolute value.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either</newpara>
    /// <newpara>strikePrice (exactly one occurrence; of the type xsd:decimal) 
    /// The price or level at which the option has been struck.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>strikePercentage (exactly one occurrence; of the type xsd:decimal) 
    /// The price or level expressed as a percentage of the forward starting spot price.</newpara>
    /// <newpara></newpara>
    /// <newpara>currency (zero or one occurrence; of the type Currency) The currency in which an amount is denominated.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityDerivativeShortFromBase</newpara>
    ///<newpara>• EquityOption</newpara>
    ///<newpara>• StrikeSpread</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquityStrike : ItemGUI
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike")]
        public EFS_RadioChoice typeStrike;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeStrikePriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikePrice", typeof(EFS_Decimal),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal typeStrikePrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeStrikePercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikePercentage", typeof(EFS_Decimal),Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal typeStrikePercentage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("currency", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
		#endregion Members
		#region Constructors
        public EquityStrike()
        {
            typeStrikePrice = new EFS_Decimal();
            typeStrikePercentage = new EFS_Decimal();
        }
		#endregion Constructors
    }
    #endregion EquityStrike

    #region PrePayment
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining PrePayment.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>payerPartyReference (exactly one occurrence; of the type PartyReference)</newpara>
    /// <newpara>receiverPartyReference (exactly one occurrence; of the type PartyReference)</newpara>
    /// <newpara>prePayment (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>prePaymentAmount (exactly one occurrence; of the type Money)</newpara>
    /// <newpara>prePaymentDate (exactly one occurrence; of the type AdjustableDate)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityExercise</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class PrePayment : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
        [ControlGUI(Name = "Payer")]
        public PartyReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        [ControlGUI(Name = "Receiver")]
        public PartyReference receiverPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("prePayment", Order = 3)]
        [ControlGUI(Name = "Pre-Payment", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean prePayment;
		[System.Xml.Serialization.XmlElementAttribute("prePaymentAmount", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money prePaymentAmount;
		[System.Xml.Serialization.XmlElementAttribute("prePaymentDate", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date", IsVisible = false)]
        public AdjustableDate prePaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
        public bool FillBalise;
		#endregion Members
	}
    #endregion PrePayment

    #region StrategyFeature
    /// <summary>
    /// <newpara><b>Description :</b> A type for definining equity option simple strategy features.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either </newpara>
    /// <newpara>strikeSpread (exactly one occurrence; of the type StrikeSpread)</newpara>
    /// <newpara>or</newpara>
    /// <newpara>calendarSpread (exactly one occurrence; of the type CalendarSpread)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class StrategyFeature : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike")]
        public EFS_RadioChoice strategyFeature;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strategyFeatureCalendarSpreadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calendarSpread", typeof(CalendarSpread),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public CalendarSpread strategyFeatureCalendarSpread;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strategyFeatureStrikeSpreadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikeSpread", typeof(StrikeSpread),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public StrikeSpread strategyFeatureStrikeSpread;
		#endregion Members
        #region Constructors
        public StrategyFeature()
        {
            strategyFeatureCalendarSpread = new CalendarSpread();
            strategyFeatureStrikeSpread = new StrikeSpread();
        }
        #endregion Constructors
    }
    #endregion StrategyFeature
    #region StrikeSpread
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining a strike spread feature.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>upperStrike (exactly one occurrence; of the type EquityStrike)</newpara>
    /// <newpara>upperStrikeNumberOfOptions (exactly one occurrence; of the type xsd:decimal)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• StrategyFeature</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class StrikeSpread : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("upperStrike", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Upper strike", IsVisible = false)]
        public EquityStrike upperStrike;
		[System.Xml.Serialization.XmlElementAttribute("upperStrikeNumberOfOptions", Order = 2)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Upper strike")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Number of options")]
        public EFS_Decimal upperStrikeNumberOfOptions;
		#endregion Members
	}
    #endregion StrikeSpread
}

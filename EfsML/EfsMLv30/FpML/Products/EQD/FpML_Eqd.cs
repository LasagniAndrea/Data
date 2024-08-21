#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.Enum;

using FpML.v44.Assetdef;
using FpML.v44.Enum;
using FpML.v44.Eq.Shared;
using FpML.v44.Shared;
using FpML.v44.Option.Shared;
using EfsML.v30.Security.Shared;
#endregion using directives

namespace FpML.v44.Eqd
{
    #region BrokerEquityOption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("brokerEquityOption", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Broker Equity Option")]
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

    #region EquityAmericanExercise
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class EquityAmericanExercise : SharedAmericanExercise
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class EquityBermudaExercise : SharedAmericanExercise
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BrokerEquityOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityDerivativeShortFormBase))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityDerivativeLongFormBase))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityForward))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOptionTransactionSupplement))]
    public partial  class EquityDerivativeBase : Product
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
        [ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
        [ControlGUI(Name = "Seller")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;
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
        public EquityExerciseValuationSettlement equityExercise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("feature",Order = 8)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Option feature")]
        public OptionFeatures feature;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFeatureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxFeature", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fx feature")]
        public FxFeature fxFeature;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strategyFeatureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("strategyFeature", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strategy feature")]
        public StrategyFeature strategyFeature;
        #endregion Members
    }
    #endregion EquityDerivativeBase
    #region EquityDerivativeLongFormBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityForward))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOption))]
    public partial class EquityDerivativeLongFormBase : EquityDerivativeBase
    {
        #region Members
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
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extraordinary Events")]
		public bool FillBalise;
        #endregion Members
    }
    #endregion EquityDerivativeLongFormBase
    #region EquityDerivativeShortFormBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BrokerEquityOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOptionTransactionSupplement))]
	public partial class EquityDerivativeShortFormBase : EquityDerivativeBase
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
		[System.Xml.Serialization.XmlElementAttribute("numberOfOptions", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Number of options", Width = 100)]
        public EFS_Decimal numberOfOptions;
		[System.Xml.Serialization.XmlElementAttribute("equityPremium", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium", IsVisible = false)]
        public EquityPremium equityPremium;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion EquityDerivativeShortFormBase
    #region EquityEuropeanExercise
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class EquityEuropeanExercise : Exercise
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
    #region EquityExerciseValuationSettlement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class EquityExerciseValuationSettlement : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice equityExercise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityExerciseAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("equityAmericanExercise", typeof(EquityAmericanExercise), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EquityAmericanExercise equityExerciseAmerican;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityExerciseBermudaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("equityBermudaExercise", typeof(EquityBermudaExercise), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EquityBermudaExercise equityExerciseBermuda;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityExerciseEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("equityEuropeanExercise", typeof(EquityEuropeanExercise), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EquityEuropeanExercise equityExerciseEuropean;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Method")]
        public EFS_RadioChoice exerciseMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseMethodAutomaticExerciseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("automaticExercise", typeof(EFS_Boolean), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "yes")]
        public EFS_Boolean exerciseMethodAutomaticExercise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseMethodPrePaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("prePayment", typeof(PrePayment), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public PrePayment exerciseMethodPrePayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseMethodMakeWholeProvisionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("makeWholeProvisions", typeof(MakeWholeProvisions), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public MakeWholeProvisions exerciseMethodMakeWholeProvisions;

        [System.Xml.Serialization.XmlElementAttribute("equityValuation", Order = 7)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Valuation", IsVisible = false)]
        public EquityValuation equityValuation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementDate", Order = 8)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity Valuation")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Date")]
        public AdjustableOrRelativeDate settlementDate;
        [System.Xml.Serialization.XmlElementAttribute("settlementCurrency", Order = 9)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "settlement Currency", Width = 75)]
        public Currency settlementCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementPriceSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementPriceSource", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Price Source")]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value")]
        public SettlementPriceSource settlementPriceSource;
        [System.Xml.Serialization.XmlElementAttribute("settlementType", Order = 11)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Settlement Type")]
        public SettlementTypeEnum settlementType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementMethodElectionDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementMethodElectionDate", Order = 12)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement method", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Election Date")]
        public AdjustableOrRelativeDate settlementMethodElectionDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementMethodElectingPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementMethodElectingPartyReference", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party reference")]
        public PartyReference settlementMethodElectingPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement method")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion EquityExerciseValuationSettlement
    #region EquityForward
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("equityForward", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Equity Forward")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class EquityMultipleExercise : ItemGUI
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
    }
    #endregion EquityMultipleExercise
    #region EquityOption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("equityOption", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Equity Option")]
    public partial class EquityOption : EquityDerivativeLongFormBase
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("strike",  Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike", IsVisible = false)]
        public EquityStrike strike;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotPrice", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Spot price")]
        public EFS_Decimal spotPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool numberOfOptionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("numberOfOptions", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Number of options", Width = 100)]
        public EFS_Decimal numberOfOptions;
        [System.Xml.Serialization.XmlElementAttribute("optionEntitlement", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "option Entitlement", Width = 100)]
        public EFS_Decimal optionEntitlement;
        [System.Xml.Serialization.XmlElementAttribute("equityPremium", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium", IsVisible = false)]
        public EquityPremium equityPremium;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        public bool FillBalise2;
        #endregion Members
    }
    #endregion EquityOption
    #region EquityOptionTermination
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class EquityOptionTermination : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementAmountPaymentDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement amount payment date", IsVisible = false)]
        public AdjustableDate settlementAmountPaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("settlementAmount", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement amount payment date")]
        public Money settlementAmount;
        #endregion Members
    }
    #endregion EquityOptionTermination
    #region EquityOptionTransactionSupplement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("equityOptionTransactionSupplement", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Equity Transaction Supplement")]
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
		[System.Xml.Serialization.XmlElementAttribute("methodOfAdjustment", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Method of adjustment")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public MethodOfAdjustmentEnum methodOfAdjustment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool localJurisdictionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("localJurisdiction", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Local Jurisdiction")]
        public Country localJurisdiction;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike")]
        public EFS_RadioChoice unit;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool unitNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty unitNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool unitMultiplierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multiplier", typeof(EFS_Decimal), Order =6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Contract multiplier")]
        public EFS_Decimal unitMultiplier;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool unitOptionEntitlementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unitOptionEntitlement", typeof(EFS_Decimal), Order =7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Number of shares per option")]
        public EFS_Decimal unitOptionEntitlement;
        #endregion Members
    }
    #endregion EquityOptionTransactionSupplement

    #region PrePayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PrePayment : ItemGUI
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
}

#region using directives
using System;
using System.Collections;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;

using EfsML.v30.Fx;
using EfsML.v30.Shared;

using FpML.Enum;

using FpML.v44.Enum;
using FpML.v44.Shared;
#endregion using directives

namespace FpML.v44.Fx
{
    #region CutName
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CutName : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string cutNameScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members
        #region Constructors
        public CutName()
        {
            cutNameScheme = "http://www.fpml.org/coding-scheme/cut-name-1-0";
        }
        #endregion Constructors
    }
    #endregion CutName

    #region ExchangeRate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ExchangeRate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        public QuotedCurrencyPair quotedCurrencyPair;
        [System.Xml.Serialization.XmlElementAttribute("rate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ControlGUI(Name = "rate", LblWidth = 75, Width = 75)]
        public EFS_Decimal rate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotRate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "spotRate", LblWidth = 75, Width = 75)]
        public EFS_Decimal spotRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forwardPointsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("forwardPoints", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "forwardPoints", LblWidth = 75, Width = 75)]
        public EFS_Decimal forwardPoints;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sideRatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sideRates", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side Rates")]
        public SideRates sideRates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFixingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxFixing", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing")]
        public FxFixing fxFixing;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:ExchangeRate";
        #endregion Members
    }
    #endregion ExchangeRate
    #region ExpiryDateTime
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ExpiryDateTime : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("expiryDate", Order = 1)]
        [ControlGUI(IsLabel = true, Name = "Date")]
        public EFS_Date expiryDate;
        [System.Xml.Serialization.XmlElementAttribute("expiryTime", Order = 2)]
        [ControlGUI(Name = " ")]
        public BusinessCenterTime expiryTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cutNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cutName", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cut name")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CutName cutName;
        #endregion Members
    }
    #endregion ExpiryDateTime

    #region FxAmericanTrigger
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FxAmericanTrigger
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("touchCondition", Order = 1)]
        [ControlGUI(Name = "Condition", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public TouchConditionEnum touchCondition;
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted currency pair", IsVisible = false)]
        public QuotedCurrencyPair quotedCurrencyPair;
        [System.Xml.Serialization.XmlElementAttribute("triggerRate", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted currency pair")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Rate", Width = 75)]
        public EFS_Decimal triggerRate;
        [System.Xml.Serialization.XmlElementAttribute("informationSource", Order = 4)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information source", IsClonable = true, IsMaster = true, IsChild = true)]
        public InformationSource[] informationSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationStartDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observationStartDate", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation start date")]
        public EFS_Date observationStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationEndDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observationEndDate", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation end date")]
        public EFS_Date observationEndDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal spotRate; // use to determine trigger type (UP/DOWN)
        #endregion Members
    }
    #endregion FxAmericanTrigger
    #region FxAverageRateObservationDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FxAverageRateObservationDate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("observationDate", Order = 1)]
        [ControlGUI(Name = "value")]
        public EFS_Date observationDate;
        [System.Xml.Serialization.XmlElementAttribute("averageRateWeightingFactor", Order = 2)]
        [ControlGUI(IsLabel = true, Name = "Weighting Factor", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 50)]
        public EFS_Decimal averageRateWeightingFactor;
        #endregion Members
    }
    #endregion FxAverageRateObservationDate
    #region FxAverageRateObservationSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FxAverageRateObservationSchedule : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("observationStartDate", Order = 1)]
        [ControlGUI(Name = "Start Date")]
        public EFS_Date observationStartDate;
        [System.Xml.Serialization.XmlElementAttribute("observationEndDate", Order = 2)]
        [ControlGUI(Name = "End Date", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Date observationEndDate;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodFrequency", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Frequency", IsVisible = false)]
        public CalculationPeriodFrequency calculationPeriodFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Frequency")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion FxAverageRateObservationSchedule
    #region FxAverageRateOption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("fxAverageRateOption", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20180514 [23812] Report
    public partial class FxAverageRateOption : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
        [ControlGUI(Name = "Seller")]
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=2)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("expiryDateTime", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date", IsVisible = false)]
        public ExpiryDateTime expiryDateTime;
        [System.Xml.Serialization.XmlElementAttribute("exerciseStyle", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Exercise")]
        public ExerciseStyleEnum exerciseStyle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxOptionPremiumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxOptionPremium", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium", IsClonable = true, IsChild = true)]
        public FxOptionPremium[] fxOptionPremium;
        [System.Xml.Serialization.XmlElementAttribute("valueDate", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Value Date")]
        public EFS_Date valueDate;
        [System.Xml.Serialization.XmlElementAttribute("putCurrencyAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=7)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money putCurrencyAmount;
        [System.Xml.Serialization.XmlElementAttribute("callCurrencyAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=8)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money callCurrencyAmount;
        [System.Xml.Serialization.XmlElementAttribute("fxStrikePrice", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=9)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price", IsVisible = false)]
        public FxStrikePrice fxStrikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotRate", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "spot Rate", Width = 75)]
        public EFS_Decimal spotRate;
        [System.Xml.Serialization.XmlElementAttribute("payoutCurrency", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=11)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payout Currency", IsVisible = false)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 75)]
        public Currency payoutCurrency;
        [System.Xml.Serialization.XmlElementAttribute("averageRateQuoteBasis", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=12)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payout Currency")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "averageRateQuoteBasis", Width = 220)]
        public StrikeQuoteBasisEnum averageRateQuoteBasis;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool precisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("precision", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Precision")]
        public EFS_PosInteger precision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool payoutFormulaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payoutFormula", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payout Formula")]
        public EFS_MultiLineString payoutFormula;
        [System.Xml.Serialization.XmlElementAttribute("primaryRateSource", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=15)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate Source", IsVisible = false)]
        public InformationSource primaryRateSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secondaryRateSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("secondaryRateSource", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Rate Source")]
        public InformationSource secondaryRateSource;
        [System.Xml.Serialization.XmlElementAttribute("fixingTime", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=17)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate Source")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fixing Time", IsVisible = false)]
        public BusinessCenterTime fixingTime;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fixing Time")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate Observation")]
        public EFS_RadioChoice rateObservation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateObservationScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averageRateObservationSchedule", typeof(FxAverageRateObservationSchedule)
             , Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Rate Observation Schedule", IsVisible = true)]
        public FxAverageRateObservationSchedule rateObservationSchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateObservationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averageRateObservationDate", typeof(FxAverageRateObservationDate)
             , Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=19)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate Observation Dates")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate Observation Date", IsClonable = true, IsMaster = true)]
        public FxAverageRateObservationDate[] rateObservationDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observedRatesSpecified;

        [System.Xml.Serialization.XmlElementAttribute("observedRates", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=20)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Observed Rates")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Observed Rates", IsClonable = true, IsChild = true)]
        public ObservedRates[] observedRates;
		#endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionType", Order = 21)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions", IsGroup = true)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call/Put")]
		public OptionTypeEnum optionType;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool bermudanExerciseDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bermudanExerciseDates",Order=22)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Bermudan exercise dates")]
		public DateList bermudanExerciseDates;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool procedureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseProcedure",Order=23)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Procedure")]
		public ExerciseProcedure procedure;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool averageStrikeOptionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averageStrikeOption", typeof(AverageStrikeOption),Order=24)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Average strike option", Width = 75)]
		public AverageStrikeOption averageStrikeOption;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool geometricAverageSpecified;
		[System.Xml.Serialization.XmlElementAttribute("geometricAverage", Order = 25)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Geometric average")]
		public Empty geometricAverage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool earlyTerminationProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", Order = 26)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Early Termination Provision")]
        public EarlyTerminationProvision earlyTerminationProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool implicitProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("implicitProvision", Order = 27)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Implicit Provisions")]
        public ImplicitProvision implicitProvision;
        #endregion Members
	}
    #endregion FxAverageRateOption
    #region FxBarrier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FxBarrier : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxBarrierTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxBarrierType", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public FxBarrierTypeEnum fxBarrierType;
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "QuotedCurrencyPair", IsVisible = false)]
        public QuotedCurrencyPair quotedCurrencyPair;
        [System.Xml.Serialization.XmlElementAttribute("triggerRate", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "QuotedCurrencyPair")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Trigger rate", Width = 75)]
        public EFS_Decimal triggerRate;
        [System.Xml.Serialization.XmlElementAttribute("informationSource",Order=4)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information Source", IsClonable = true, IsMaster = true, IsChild = true)]
        public InformationSource[] informationSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationStartDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observationStartDate", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation Start Date")]
        public EFS_Date observationStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationEndDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observationEndDate", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation End Date")]
        public EFS_Date observationEndDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal spotRate; // use to determine barrier type (UP/DOWN)
        #endregion Members
    }
    #endregion FxBarrier
    #region FxBarrierOption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("fxBarrierOption", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20180514 [23812] Report
    public partial class FxBarrierOption : FxOptionLegBarrier
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotRate", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Spot rate", Width = 75)]
        public EFS_Decimal spotRate;
        [System.Xml.Serialization.XmlElementAttribute("fxBarrier", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Barrier")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Barrier", IsClonable = true, IsMaster = true, IsChild = true)]
        public FxBarrier[] fxBarrier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerPayoutSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerPayout", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trigger Payout")]
        public FxOptionPayout triggerPayout;
        #endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionType", Order = 4)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions", IsGroup = true)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call/Put")]
		public OptionTypeEnum optionType;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool bermudanExerciseDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bermudanExerciseDates",Order=5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Bermudan exercise dates")]
		public DateList bermudanExerciseDates;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool procedureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseProcedure",Order=6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Procedure")]
		public ExerciseProcedure procedure;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cappedCallOrFlooredPutSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cappedCallOrFlooredPut",Order=7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Capped call or floored put")]
		public CappedCallOrFlooredPut cappedCallOrFlooredPut;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool fxBarrierParisianNumberOfDaysSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxBarrierParisianNumberOfDays",Order=8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Parisian barrier number of days")]
		public EFS_Integer fxBarrierParisianNumberOfDays;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool fxRebateBarrierSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxRebateBarrier",Order=9)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rebate barrier")]
		public FxBarrier fxRebateBarrier;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool earlyTerminationProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Early Termination Provision")]
        public EarlyTerminationProvision earlyTerminationProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool implicitProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("implicitProvision", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Implicit Provisions")]
        public ImplicitProvision implicitProvision;
        #endregion Members
    }
    #endregion FxBarrierOption
    #region FxDigitalOption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("fxDigitalOption", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20180514 [23812] Report
    public partial class FxDigitalOption : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=2)]
        [ControlGUI(Name = "Seller", LineFeed = MethodsGUI.LineFeedEnum.After)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("expiryDateTime", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry date", IsVisible = false)]
        public ExpiryDateTime expiryDateTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxOptionPremiumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxOptionPremium", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium", IsClonable = true, IsChild = true)]
        public FxOptionPremium[] fxOptionPremium;
        [System.Xml.Serialization.XmlElementAttribute("valueDate", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Value Date")]
        public EFS_Date valueDate;
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=6)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quoted currency pair", IsVisible = true)]
        public QuotedCurrencyPair quotedCurrencyPair;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotRate", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=7)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quoted Currency Pair")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, IsDisplay = true, Name = "Spot rate", Width = 75)]
        public EFS_Decimal spotRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trigger")]
        public EFS_RadioChoice typeTrigger;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeTriggerEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxEuropeanTrigger", Namespace = "http://www.fpml.org/2007/FpML-4-4", Type = typeof(FxEuropeanTrigger),Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European trigger", IsVisible = false)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "European trigger", IsClonable = true, IsChild = true)]
        public FxEuropeanTrigger[] typeTriggerEuropean;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeTriggerAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxAmericanTrigger", Namespace = "http://www.fpml.org/2007/FpML-4-4", Type = typeof(FxAmericanTrigger),Order=9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American trigger", IsVisible = false)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "American trigger", IsClonable = true, IsChild = true)]
        public FxAmericanTrigger[] typeTriggerAmerican;
        [System.Xml.Serialization.XmlElementAttribute("triggerPayout", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=10)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trigger payout", IsVisible = false)]
        public FxOptionPayout triggerPayout;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool limitSpecified;
        [System.Xml.Serialization.XmlElementAttribute("limit", typeof(Empty),Order=11)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trigger payout")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions", IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Limit")]
        public Empty limit;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool boundarySpecified;
        [System.Xml.Serialization.XmlElementAttribute("boundary", typeof(Empty),Order=12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Boundary")]
        public Empty boundary;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool assetOrNothingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("assetOrNothing",Order=13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Asset or nothing")]
        public AssetOrNothing assetOrNothing;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool resurrectingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("resurrecting",Order=14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Resurrecting")]
        public PayoutPeriod resurrecting;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extinguishingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("extinguishing", typeof(PayoutPeriod),Order=15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Extinguishing")]
        public PayoutPeriod extinguishing;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxBarrierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxBarrier",Order=16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Barrier")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Barrier", IsClonable = true, IsChild = true)]
        public FxBarrier[] fxBarrier;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool earlyTerminationProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", Order = 17)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Early Termination Provision")]
        public EarlyTerminationProvision earlyTerminationProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool implicitProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("implicitProvision", Order = 18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Implicit Provisions")]
        public ImplicitProvision implicitProvision;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FxDigitalOption efs_FxDigitalOption;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:FxDigitalOption";
        #endregion Members
    }
    #endregion FxDigitalOption
    #region FxEuropeanTrigger
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FxEuropeanTrigger
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("triggerCondition", Order = 1)]
        [ControlGUI(Name = "Condition", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public TriggerConditionEnum triggerCondition;
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted currency pair", IsVisible = false)]
        public QuotedCurrencyPair quotedCurrencyPair;
        [System.Xml.Serialization.XmlElementAttribute("triggerRate", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted currency pair")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Rate", Width = 75)]
        public EFS_Decimal triggerRate;
        [System.Xml.Serialization.XmlElementAttribute("informationSource",Order=4)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information source", IsClonable = true, IsMaster = true, IsChild = true)]
        public InformationSource[] informationSource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal spotRate;
        #endregion Members
    }
    #endregion FxEuropeanTrigger
    #region FxLeg
    /// <summary>
    /// 
    /// </summary>
    /// FI 20150331 [XXPOC] Modify
    /// EG 20150402 [POC] FpML to EfsML
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    //[System.Xml.Serialization.XmlRootAttribute("fxSingleLeg", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("fxSingleLeg", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class FxLeg : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("exchangedCurrency1", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Currency 1", IsVisible = false, IsCopyPaste = true)]
        public Payment exchangedCurrency1;
        [System.Xml.Serialization.XmlElementAttribute("exchangedCurrency2", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Currency 1")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Currency 2", IsVisible = false, IsCopyPaste = true)]
        public Payment exchangedCurrency2;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxDateValueDateSpecified;

        [System.Xml.Serialization.XmlElementAttribute("valueDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Currency 2")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Date Settlement", IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Value date")]
        public EFS_Date fxDateValueDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxDateCurrency1ValueDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency1ValueDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency 1 value date")]
        public EFS_Date fxDateCurrency1ValueDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxDateCurrency2ValueDateSpecified;

        [System.Xml.Serialization.XmlElementAttribute("currency2ValueDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency 2 value date")]
        public EFS_Date fxDateCurrency2ValueDate;

        [System.Xml.Serialization.XmlElementAttribute("exchangeRate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Date Settlement")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate", IsVisible = false)]
        public ExchangeRate exchangeRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nonDeliverableForwardSpecified;

        [System.Xml.Serialization.XmlElementAttribute("nonDeliverableForward", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "non Deliverable Forward", IsCopyPaste = true)]
        public FxCashSettlement nonDeliverableForward;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool confirmationSenderPartyReferenceSpecified;

        [System.Xml.Serialization.XmlElementAttribute("confirmationSenderPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "confirmation Sender")]
        public PartyReference confirmationSenderPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool marginRatioSpecified;

        [System.Xml.Serialization.XmlElementAttribute("marginRatio", Order = 9)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin ratio", IsVisible = false)]
        public MarginRatio marginRatio;
         
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin ratio")]
        public EFS_FxLeg efs_FxLeg;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150331 [XXPOC] Add
        /// FI 20170116 [21916] RptSide (R majuscule)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public FixML.v50SP1.TrdCapRptSideGrp_Block[] RptSide;

        #endregion Members
    }
    #endregion FxLeg
    #region FxOptionLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("fxSimpleOption", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20180514 [23812] Report
    public partial class FxOptionLeg : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=2)]
        [ControlGUI(Name = "Seller", LineFeed = MethodsGUI.LineFeedEnum.After)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("expiryDateTime", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date", IsVisible = false, IsCopyPaste = true)]
        public ExpiryDateTime expiryDateTime;
        [System.Xml.Serialization.XmlElementAttribute("exerciseStyle", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Exercise")]
        public ExerciseStyleEnum exerciseStyle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxOptionPremiumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxOptionPremium", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        public FxOptionPremium[] fxOptionPremium;
        [System.Xml.Serialization.XmlElementAttribute("valueDate", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Date")]
        public EFS_Date valueDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementTermsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementTerms", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash Settlement Terms", IsCopyPaste = true)]
        public FxCashSettlement cashSettlementTerms;
        [System.Xml.Serialization.XmlElementAttribute("putCurrencyAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=8)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount", IsVisible = false, IsCopyPaste = true)]
        [ControlGUI(Name = "value")]
        public Money putCurrencyAmount;
        [System.Xml.Serialization.XmlElementAttribute("callCurrencyAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=9)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount", IsVisible = false, IsCopyPaste = true)]
        [ControlGUI(Name = "value")]
        public Money callCurrencyAmount;
        [System.Xml.Serialization.XmlElementAttribute("fxStrikePrice", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=10)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price", IsVisible = false, IsCopyPaste = true)]
        public FxStrikePrice fxStrikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quotedAsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quotedAs", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quoted As", IsCopyPaste = true)]
        public QuotedAs quotedAs;
        #endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionType", Order = 12)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions", IsGroup = true)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call/Put")]
		public OptionTypeEnum optionType;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool bermudanExerciseDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bermudanExerciseDates",Order=13)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Bermudan exercise dates", IsCopyPaste = true)]
		public DateList bermudanExerciseDates;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool procedureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseProcedure",Order=14)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Procedure")]
		public ExerciseProcedure procedure;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool marginRatioSpecified;

        [System.Xml.Serialization.XmlElementAttribute("marginRatio", Order = 15)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin ratio", IsVisible = false)]
        public MarginRatio marginRatio;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin ratio")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool earlyTerminationProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Early Termination Provision")]
        public EarlyTerminationProvision earlyTerminationProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool implicitProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("implicitProvision", Order = 17)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Implicit Provisions")]
        public ImplicitProvision implicitProvision;

        /// FI 20150331 [POC] add
        /// FI 20170116 [21916] RptSide (R majuscule)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public FixML.v50SP1.TrdCapRptSideGrp_Block[] RptSide;
		#endregion Members
    }
    #endregion FxOptionLeg
    #region FxOptionPayout
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class FxOptionPayout : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("currency", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Name = "Currency", Width = 75)]
        public Currency currency;
        [System.Xml.Serialization.XmlElementAttribute("amount", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ControlGUI(Name = "Amount")]
        public EFS_Decimal amount;
        [System.Xml.Serialization.XmlElementAttribute("payoutStyle", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [ControlGUI(Name = "Payout style", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public PayoutEnum payoutStyle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementInformation", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement Information")]
        public SettlementInformation settlementInformation;
        #endregion Members
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool customerSettlementPayoutSpecified;
        [System.Xml.Serialization.XmlElementAttribute("customerSettlementPayout", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extension", IsVisible = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Customer settlement payout")]
        public CustomerSettlementPayment customerSettlementPayout;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extension")]
        public bool FillBalise;
        #endregion Members

    }
    #endregion FxOptionPayout
    #region FxOptionPremium
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class FxOptionPremium
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ControlGUI(Name = "Payer", LblWidth = 107)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference payerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=2)]
        [ControlGUI(Name = "Receiver")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference receiverPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("premiumAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=3)]
        [ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Money premiumAmount;
        [System.Xml.Serialization.XmlElementAttribute("premiumSettlementDate", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=4)]
        [ControlGUI(Name = "Settlement Date", LblWidth = 107)]
        public EFS_Date premiumSettlementDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementInformation", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Information")]
        public SettlementInformation settlementInformation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool premiumQuoteSpecified;
        [System.Xml.Serialization.XmlElementAttribute("premiumQuote", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium Quote")]
        public PremiumQuote premiumQuote;
        #endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool customerSettlementPremiumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("customerSettlementPremium", Order = 7)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extension", IsVisible = true)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Customer settlement premium")]
		public CustomerSettlementPayment customerSettlementPremium;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extension")]
		public bool FillBalise;
		#endregion Members
    }
    #endregion FxOptionPremium
    #region FxStrikePrice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FxStrikePrice : ItemGUI
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("rate", Order = 1)]
        [ControlGUI(Name = "Rate", Width = 75)]
        public EFS_Decimal rate;
		[System.Xml.Serialization.XmlElementAttribute("strikeQuoteBasis", Order = 2)]
        [ControlGUI(Name = "Basis", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 220)]
        public StrikeQuoteBasisEnum strikeQuoteBasis;
        #endregion Members
    }
    #endregion FXStrikePrice
    #region FxSwap
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("fxSwap", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class FxSwap : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("fxSingleLeg",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Leg", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 2)]
        public FxLeg[] fxSingleLeg;
        #endregion Members
    }
    #endregion FxSwap

    #region ObservedRates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ObservedRates
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("observationDate", Order = 1)]
        [ControlGUI(Name = "observation Date")]
        public EFS_Date observationDate;
        [System.Xml.Serialization.XmlElementAttribute("observedRate", Order = 2)]
        [ControlGUI(Name = "rate", LblWidth = 45, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75)]
        public EFS_Decimal observedRate;
        #endregion Members
    }
    #endregion ObservedRates

    #region PremiumQuote
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PremiumQuote : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("premiumValue", Order = 1)]
        [ControlGUI(Name = "Value", Width = 75)]
        public EFS_Decimal premiumValue;
        [System.Xml.Serialization.XmlElementAttribute("premiumQuoteBasis", Order = 2)]
        [ControlGUI(Name = "Basis", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 250)]
        public PremiumQuoteBasisEnum premiumQuoteBasis;
        #endregion Members
    }
    #endregion PremiumQuote

    #region QuotedAs
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class QuotedAs : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("optionOnCurrency", Order = 1)]
        [ControlGUI(Name = "OptionOnCurrency", Width = 75)]
        public Currency optionOnCurrency;
        [System.Xml.Serialization.XmlElementAttribute("faceOnCurrency", Order = 2)]
        [ControlGUI(Name = "FaceOnCurrency", Width = 75)]
        public Currency faceOnCurrency;
        [System.Xml.Serialization.XmlElementAttribute("quotedTenor", Order = 3)]
        [ControlGUI(Name = "QuotedTenor", LblWidth = 80)]
        public Interval quotedTenor;
        #endregion Members
    }
    #endregion QuotedAs

    #region SideRate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class SideRate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
        [ControlGUI(Name = "currency", LblWidth = 190, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75)]
        public Currency currency;
        [System.Xml.Serialization.XmlElementAttribute("sideRateBasis", Order = 2)]
        [ControlGUI(Name = "rateBasis", LblWidth = 60, Width = 205)]
        public SideRateBasisEnum sideRateBasis;
        [System.Xml.Serialization.XmlElementAttribute("rate", Order = 3)]
        [ControlGUI(Name = "value", LblWidth = 77, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75)]
        public EFS_Decimal rate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotRate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "spotRate", LblWidth = 180, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75)]
        public EFS_Decimal spotRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forwardPointsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("forwardPoints", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "forwardPoints", LblWidth = 180, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75)]
        public EFS_Decimal forwardPoints;
        #endregion Members
    }
    #endregion SideRate
    #region SideRates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class SideRates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("baseCurrency", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "base currency", Width = 75)]
        public Currency baseCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currency1SideRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency1SideRate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "currency 1")]
        public SideRate currency1SideRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currency2SideRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency2SideRate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "currency 2")]
        public SideRate currency2SideRate;
        #endregion Members
    }
    #endregion SideRates

    #region TermDeposit
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("termDeposit", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class TermDeposit : Product
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("initialPayerReference", Order = 1)]
        [ControlGUI(Name = "Payer", LblWidth = 115, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference initialPayerReference;
		[System.Xml.Serialization.XmlElementAttribute("initialReceiverReference", Order = 2)]
		[ControlGUI(Name = "Receiver")]
        public PartyReference initialReceiverReference;
		[System.Xml.Serialization.XmlElementAttribute("startDate", Order = 3)]
		[ControlGUI(Name = "Start date", LblWidth = 115, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_Date startDate;
		[System.Xml.Serialization.XmlElementAttribute("maturityDate", Order = 4)]
		[ControlGUI(Name = "Maturity date", LblWidth = 115, LineFeed = MethodsGUI.LineFeedEnum.BeforeAndAfter)]
        public EFS_Date maturityDate;
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Order = 5)]
		[ControlGUI(Name = "Day count fraction", LblWidth = 115, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public DayCountFractionEnum dayCountFraction;
		[System.Xml.Serialization.XmlElementAttribute("principal", Order = 6)]
		[ControlGUI(Name = "Amount", LblWidth = 115)]
        public Money principal;
        // FI 20140909 [20340] fixedRate est une donnée obligatoire (voir FpML) 
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        //public bool fixedRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fixedRate", Order = 7)]
		[ControlGUI(Name = "Fixed rate", LblWidth = 77, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal fixedRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interestSpecified;
		[System.Xml.Serialization.XmlElementAttribute("interest", Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Interest")]
        public Money interest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payment",Order=9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public Payment[] payment;
        #endregion Members
    }
    #endregion TermDeposit

}

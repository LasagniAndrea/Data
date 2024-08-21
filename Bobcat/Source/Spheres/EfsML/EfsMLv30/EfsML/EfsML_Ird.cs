#region using directives
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Enum;
using FpML.Enum;
using FpML.v44.Option.Shared;
using FpML.v44.Shared;
using System.Reflection;
#endregion using directives

namespace EfsML.v30.Ird
{
    #region AmericanTrigger
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class AmericanTrigger : ItemGUI
    {
        #region Members
        [ControlGUI(Name = "Condition", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public TouchConditionEnum touchCondition;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerUnderlyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerUnderlyer", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer", IsClonable = true, IsChild = true)]
        public KnockUnderlyer[] triggerUnderlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerUnderlyerBasketTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerUnderlyerBasketType", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "UnderlyerBasketType")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public BasketTypeEnum triggerUnderlyerBasketType;
        [System.Xml.Serialization.XmlElementAttribute("trigger", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger", IsVisible = false)]
        public Trigger trigger;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger")]
        public bool FillBalise;
        #endregion Members
        #region Constructors
        public AmericanTrigger() { }
        #endregion Constructors
    }
    #endregion AmericanTrigger
    #region AverageFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class AverageFeature : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strikeFactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikeFactor", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike factor")]
        public EFS_Decimal strikeFactor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingPeriodOutSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averagingPeriodOut", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging period out")]
        public FeatureAveragingPeriod averagingPeriodOut;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averagingType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging Type", Width = 200)]
        public AveragingTypeEnum averagingType;
        #endregion Members
        #region Constructors
        public AverageFeature() { }
        #endregion Constructors
    }
    #endregion AverageFeature

    #region CorridorBarriers
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CorridorBarriers : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("barrierUpSchedule", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Barrier Up", IsVisible = true)]
        public Schedule barrierUpSchedule;
        [System.Xml.Serialization.XmlElementAttribute("barrierDownSchedule", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Barrier Down")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Barrier Down", IsVisible = true)]
        public Schedule barrierDownSchedule;
        [System.Xml.Serialization.XmlElementAttribute("corridorType", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Barrier Down")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Type")]
        public CorridorTypeEnum corridorType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nbMaxValuesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("nbMaxValues", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Nb Max Values")]
        public NbMaxValues nbMaxValues;
        #endregion Members
        #region Constructors
        public CorridorBarriers() { }
        #endregion Constructors
    }
    #endregion CorridorBarriers

    #region EuropeanTrigger
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class EuropeanTrigger : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("triggerCondition", Order = 1)]
        [ControlGUI(Name = "Condition", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public TriggerConditionEnum triggerCondition;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerUnderlyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerUnderlyer", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer", IsClonable = true, IsChild = true)]
        public KnockUnderlyer[] triggerUnderlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerUnderlyerBasketTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerUnderlyerBasketType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "UnderlyerBasketType")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public BasketTypeEnum triggerUnderlyerBasketType;

        [System.Xml.Serialization.XmlElementAttribute("trigger", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger", IsVisible = false)]
        public Trigger trigger;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger")]
        public bool FillBalise;
        #endregion Members
        #region Constructors
        public EuropeanTrigger() { }
        #endregion Constructors
    }
    #endregion EuropeanTrigger

    #region FeatureAveragingPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class FeatureAveragingPeriod : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool scheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("schedule", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Schedules")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Schedule", IsClonable = true, IsChild = true)]
        public AveragingSchedule[] schedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingDateTimesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averagingDateTimes", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging Dates")]
        public DateTimeList averagingDateTimes;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool weekNumberSpecified;
        [System.Xml.Serialization.XmlElementAttribute("weekNumber", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Week number")]
        public EFS_Decimal weekNumber;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingOnPreviousPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averagingOnPreviousPeriod", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging on previous period")]
        public EFS_Boolean averagingOnPreviousPeriod;
        [System.Xml.Serialization.XmlElementAttribute("marketDisruption", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Market Disruption", Width = 200)]
        public MarketDisruptionEnum marketDisruption;
        #endregion Members
        #region Constructors
        public FeatureAveragingPeriod() { }
        #endregion Constructors
    }
    #endregion FeatureAveragingPeriod
    #region FxFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class FxFeature : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("description", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "description", Width = 500)]
        public EFS_String description;
        [System.Xml.Serialization.XmlElementAttribute("referenceCurrency", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Currency", Width = 75)]
        public IdentifiedCurrency referenceCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotFxRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotFxRate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Composite")]
        public Composite spotFxRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool prefixedFxRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("prefixedFxRate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Quanto")]
        public Quanto prefixedFxRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotFxRateSpreadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotFxRateSpread", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spot FxRate spread")]
        public EFS_Decimal spotFxRateSpread;
        #endregion Members
        #region Constructors
        public FxFeature() { }
        #endregion Constructors
    }
    #endregion FxFeature

    #region IndexedRateFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class IndexedRateFeature : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("description", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "description", Width = 500)]
        public EFS_String description;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice featureType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureTypeIndexSpecified;
        [System.Xml.Serialization.XmlElementAttribute("index", typeof(UnderlyerIndex), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public UnderlyerIndex featureTypeIndex;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureTypeQuotedCurrencyPairSpecified;
        [System.Xml.Serialization.XmlElementAttribute("featureTypeQuotedCurrencyPair", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public QuotedCurrencyPair featureTypeQuotedCurrencyPair;
        [System.Xml.Serialization.XmlElementAttribute("indexationPercentage", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Indexation percentage")]
        public EFS_Decimal indexationPercentage;
        [System.Xml.Serialization.XmlElementAttribute("indexationFormula", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Indexation formula", Width = 200)]
        public IndexationFormulaEnum indexationFormula;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingPeriodOutSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averagingPeriodOut", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging period out")]
        public FeatureAveragingPeriod averagingPeriodOut;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averagingType", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging Type", Width = 200)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public AveragingTypeEnum averagingType;
        #endregion Members
        #region Constructors
        public IndexedRateFeature()
        {
            featureTypeIndex = new UnderlyerIndex();
            featureTypeQuotedCurrencyPair = new QuotedCurrencyPair();
        }
        #endregion Constructors
    }
    #endregion IndexedRateFeature

    #region KnockFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class KnockFeature : ItemGUI, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("description", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "description", Width = 500)]
        public EFS_String description;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("knockUnderlyer", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyers")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer", IsClonable = true, IsChild = true)]
        public KnockUnderlyer[] knockUnderlyer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerBasketTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("knockUnderlyerBasketType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "UnderlyerBasketType")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public BasketTypeEnum knockUnderlyerBasketType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice knockType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockTypeTriggerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("trigger", typeof(Trigger),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Trigger", IsVisible = true)]
        public Trigger knockTypeTrigger;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockTypeCorridorBarriersSpecified;
        [System.Xml.Serialization.XmlElementAttribute("corridorBarriers", typeof(CorridorBarriers),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Corridor Barriers", IsVisible = true)]
        public CorridorBarriers knockTypeCorridorBarriers;
        [System.Xml.Serialization.XmlElementAttribute("knockCondition", Order = 6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "Condition")]
        public KnockConditionEnum knockCondition;
        [System.Xml.Serialization.XmlElementAttribute("knockEffectType", Order = 7)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "Effect Type")]
        public KnockEffectTypeEnum knockEffectType;

        [System.Xml.Serialization.XmlElementAttribute("knockObservation", Order = 8)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observations")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation", IsClonable = true, IsMaster = true, IsChild = true)]
        public KnockObservation[] knockObservation;
        #endregion Members
        #region Constructors
        public KnockFeature()
        {
            knockTypeTrigger = new Trigger();
            knockTypeCorridorBarriers = new CorridorBarriers();
        }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion KnockFeature
    #region KnockObservation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class KnockObservation : ItemGUI, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("observationType", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "Type")]
        public KnockObservationTypeEnum observationType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observationPeriod", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation Periods")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Period", IsClonable = true, IsChild = true)]
        public BusinessDateRange[] observationPeriod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observationDates", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation Dates")]
        public DateList observationDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observationSchedule",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation Schedules")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation Schedule", IsClonable = true, IsChild = true)]
        public AveragingSchedule[] observationSchedule;
        #endregion Members
        #region Constructors
        public KnockObservation() { }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion KnockObservation
    #region KnockUnderlyer
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class KnockUnderlyer : ItemGUI, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        public EFS_RadioChoice knockUnderlyer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerQuotedCurrencyPairSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", typeof(QuotedCurrencyPair),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Quoted Currency Pair", IsVisible = true)]
        public QuotedCurrencyPair knockUnderlyerQuotedCurrencyPair;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerFloatingRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingRate", typeof(UnderlyerFloatingRate), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Floating Rate", IsVisible = true)]
        public UnderlyerFloatingRate knockUnderlyerFloatingRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerIndexSpecified;
        [System.Xml.Serialization.XmlElementAttribute("index", typeof(UnderlyerIndex), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Index", IsVisible = true)]
        [ControlGUI(Name = "Index", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public UnderlyerIndex knockUnderlyerIndex;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerSecurityCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("securityCode", typeof(UnderlyerSecurityCode), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Security Code", IsVisible = true)]
        [ControlGUI(Name = "Code", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public UnderlyerSecurityCode knockUnderlyerSecurityCode;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockUnderlyerNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty knockUnderlyerNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockInformationSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("knockInformationSource", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information Source")]
        public InformationSource knockInformationSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixingTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixingTime", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Time")]
        public BusinessCenterTime fixingTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool associatedStrikeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("associatedStrike", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Associated Strike")]
        public EFS_Decimal associatedStrike;
        #endregion Members
        #region Constructors
        public KnockUnderlyer()
        {
            knockUnderlyerQuotedCurrencyPair = new QuotedCurrencyPair();
            knockUnderlyerFloatingRate = new UnderlyerFloatingRate();
            knockUnderlyerSecurityCode = new UnderlyerSecurityCode();
            knockUnderlyerIndex = new UnderlyerIndex();
            knockUnderlyerNone = new Empty();
        }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion KnockUnderlyer
    #region KnownAmountSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class KnownAmountSchedule : AmountSchedule
    {
        #region Members
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional Initial Value")]
        [System.Xml.Serialization.XmlElementAttribute("notionalValue", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        public Money notionalValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalValueSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        public DayCountFractionEnum dayCountFraction;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:KnownAmountSchedule";
        #endregion Members
    }
    #endregion KnownAmountSchedule

    #region NbMaxValues
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class NbMaxValues : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nbMaxValuesInSpecified;
        [System.Xml.Serialization.XmlElementAttribute("nbMaxValuesIn", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "In")]
        public EFS_Integer nbMaxValuesIn;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nbMaxValuesOutSpecified;
        [System.Xml.Serialization.XmlElementAttribute("nbMaxValuesOut", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Out")]
        public EFS_Integer nbMaxValuesOut;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nbMaxValuesTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("nbMaxValuesType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public NbMaxValuesTypeEnum nbMaxValuesType;
        #endregion Members
        #region Constructors
        public NbMaxValues() { }
        #endregion Constructors
    }
    #endregion NbMaxValues

    #region StreamExtension
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class StreamExtension : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockFeaturesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("knockFeatures", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Knock Features")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Knock Feature", IsClonable = true, IsChild = true)]
        public KnockFeature[] knockFeatures;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerFeaturesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerFeatures", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger Features")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger Feature", IsClonable = true, IsChild = true)]
        public TriggerFeature[] triggerFeatures;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averageFeaturesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averageFeatures", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Average Features")]
        public AverageFeature averageFeatures;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexedRateFeaturesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexedRateFeatures", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index Rate Features")]
        public IndexedRateFeature indexedRateFeatures;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFeaturesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxFeatures", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Features")]
        public FxFeature fxFeatures;
        #endregion Members
        #region Constructors
        public StreamExtension() { }
        #endregion Constructors
    }
    #endregion StreamExtension
    #region StepUpProvision
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class StepUpProvision : Exercise
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
        public EFS_RadioChoice stepupExercise;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepupExerciseAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American Exercise", IsVisible = true)]
        public AmericanExercise stepupExerciseAmerican;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepupExerciseBermudaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bermuda Exercise", IsVisible = true)]
        public BermudaExercise stepupExerciseBermuda;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepupExerciseEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European Exercise", IsVisible = true)]
        public EuropeanExercise stepupExerciseEuropean;
        #endregion Members
    }
    #endregion StepUpProvision

    #region TriggerEffect
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class TriggerEffect : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("triggerEffectType", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, IsDisplay = true, Name = "Effect Type")]
        public KnockEffectTypeEnum triggerEffectType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerPayoutSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerPayout", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Payout")]
        public TriggerPayout triggerPayout;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerPrincipalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerPrincipal", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Principal")]
        public TriggerPrincipal triggerPrincipal;
        #endregion Members
        #region Constructors
        public TriggerEffect() { }
        #endregion Constructors
    }
    #endregion TriggerEffect
    #region TriggerFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class TriggerFeature : ItemGUI, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("description", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "description", Width = 500)]
        public EFS_String description;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice trigger;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("americanTrigger", typeof(AmericanTrigger), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American", IsVisible = true)]
        public AmericanTrigger triggerAmerican;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("europeanTrigger", typeof(EuropeanTrigger), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European", IsVisible = true)]
        public EuropeanTrigger triggerEuropean;

        [System.Xml.Serialization.XmlElementAttribute("triggerObservation", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation", IsClonable = true, IsMaster = true, IsChild = true)]
        public KnockObservation[] observation;

        [System.Xml.Serialization.XmlElementAttribute("triggerEffect", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effect", IsVisible = false)]
        public TriggerEffect effect;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effect")]
        public bool FillBalise;
        #endregion Members
        #region Constructors
        public TriggerFeature()
        {
            triggerAmerican = new AmericanTrigger();
            triggerEuropean = new EuropeanTrigger();
        }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion TriggerFeature
    #region TriggerPayout
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class TriggerPayout : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featurePaymentValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("featurePaymentValue", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Feature")]
        public FeaturePayment featurePaymentValue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureRateValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("featureRateValue", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate")]
        public EFS_Decimal featureRateValue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureSpreadValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("featureSpreadValue", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread")]
        public EFS_Decimal featureSpreadValue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureMultiplierValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("featureMultiplierValue", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Multiplier")]
        public EFS_Decimal featureMultiplierValue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featurePayoutFormulaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("featurePayoutFormula", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Formula")]
        public Formula featurePayoutFormula;
        #endregion Members
        #region Constructors
        public TriggerPayout() { }
        #endregion Constructors
    }
    #endregion TriggerPayout
    #region TriggerPrincipal
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class TriggerPrincipal : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureNotionalPercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("featureNotionalPercentage", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Notional percentage")]
        public EFS_Decimal featureNotionalPercentage;
        [System.Xml.Serialization.XmlElementAttribute("featurePrincipalExchange", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Principal Exchange")]
        public PrincipalExchangeEnum featurePrincipalExchange;
        [System.Xml.Serialization.XmlElementAttribute("principalExchangeType", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Principal Exchange Type")]
        public PrincipalExchangeTypeEnum principalExchangeType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool principalPaymentDualCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("principalPaymentDualCurrency", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Principal Payment Dual Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency principalPaymentDualCurrency;
        #endregion Members
        #region Constructors
        public TriggerPrincipal() { }
        #endregion Constructors
    }
    #endregion TriggerPrincipal

    #region UnderlyerFloatingRate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class UnderlyerFloatingRate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("floatingRateIndex", Order = 1)]
        [ControlGUI(Name = "Rate", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public FloatingRateIndex floatingRateIndex;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexTenorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexTenor", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "indexTenor", LblWidth = 180)]
        public Interval indexTenor;
        #endregion Members
        #region Constructors
        public UnderlyerFloatingRate() { }
        #endregion Constructors
    }
    #endregion UnderlyerFloatingRate
    #region UnderlyerIndex
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class UnderlyerIndex : SchemeBoxGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string indexScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Index)]
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
        public UnderlyerIndex()
        {
            indexScheme = "http://www.efs.org/spec/2005/index-2-0";
        }
        #endregion Constructors
    }
    #endregion UnderlyerIndex
    #region UnderlyerSecurityCode
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class UnderlyerSecurityCode : SchemeBoxGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string securityCodeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.SecurityCode)]
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
        public UnderlyerSecurityCode()
        {
            securityCodeScheme = "http://www.efs.org/spec/2005/securityCode-2-0";
        }
        #endregion Constructors
    }
    #endregion UnderlyerSecurityCode
}

#region using directives
using EFS.ACommon;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.v30.Ird;
using EfsML.v30.Shared;
using FpML.Enum;
using FpML.v44.Assetdef;
using FpML.v44.Enum;
using FpML.v44.Shared;
using System.Reflection;
#endregion using directives

namespace FpML.v44.Ird
{
    #region BondReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BondReference : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("bond", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bond")]
        public Bond bond;
        [System.Xml.Serialization.XmlElementAttribute("conditionPrecedentBond", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bond")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "conditionPrecedentBond")]
        public EFS_Boolean conditionPrecedentBond;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool discrepancyClauseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("discrepancyClause", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "discrepancyClause")]
        public EFS_Boolean discrepancyClause;
        #endregion Members
    }
    #endregion BondReference
    #region BulletPayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("bulletPayment", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [MainTitleGUI(Title = "Bullet Payment")]
    // EG 20180608 Add entityPartyReference
    public partial class BulletPayment : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payment", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        public Payment payment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("entityPartyReference", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 2)]
        [ControlGUI(Name = "Entity", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference entityPartyReference;
        #endregion Members
    }
    #endregion BulletPayment

    #region Calculation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("calculation", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Calculation")]
    public partial class Calculation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional amount", IsCopyPaste = true)]
        public EFS_RadioChoice calculation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationNotionalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalSchedule", typeof(Notional),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Notional", IsVisible = true)]
        public Notional calculationNotional;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationFxLinkedNotionalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxLinkedNotionalSchedule", typeof(FxLinkedNotionalSchedule), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "FxLinked Notional Schedule", IsVisible = true)]
        public FxLinkedNotionalSchedule calculationFxLinkedNotional;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate", IsCopyPaste = true)]
        public EFS_RadioChoice rate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateFixedRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedRateSchedule", typeof(Schedule), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Fixed Rate", IsVisible = true)]
        public Schedule rateFixedRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateFloatingRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingRateCalculation", typeof(FloatingRateCalculation), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Floating Rate", IsVisible = true)]
        public FloatingRateCalculation rateFloatingRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateInflationRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("inflationRateCalculation", typeof(InflationRateCalculation), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Inflation Rate", IsVisible = true)]
        public InflationRateCalculation rateInflationRate;


        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Order = 6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool discountingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("discounting", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "discounting")]
        public Discounting discounting;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool compoundingMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("compoundingMethod", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "compoundingMethod")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CompoundingMethodEnum compoundingMethod;
        #endregion Members
    }
    #endregion Calculation
    #region CalculationPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CalculationPeriod
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool unadjustedStartDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unadjustedStartDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Start date")]
        public EFS_Date unadjustedStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool unadjustedEndDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unadjustedEndDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "End date")]
        public EFS_Date unadjustedEndDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedStartDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedStartDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted start date")]
        public EFS_Date adjustedStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedEndDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedEndDate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted end date")]
        public EFS_Date adjustedEndDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodNumberOfDaysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodNumberOfDays", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Number of days", Width = 70)]
        public EFS_PosInteger calculationPeriodNumberOfDays;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional amount", IsCopyPaste = true)]
        public EFS_RadioChoice amount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool amountNotionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalAmount", typeof(EFS_Decimal),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount", IsVisible = true)]
        public EFS_Decimal amountNotionalAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool amountFxLinkedNotionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxLinkedNotionalAmount", typeof(FxLinkedNotionalAmount),Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "FxLinked Notional Amount", IsVisible = true)]
        public FxLinkedNotionalAmount amountFxLinkedNotionalAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate", IsCopyPaste = true)]
        public EFS_RadioChoice rate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateFloatingRateDefinitionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingRateDefinition", typeof(FloatingRateDefinition),Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Floating rate", IsVisible = true)]
        public FloatingRateDefinition rateFloatingRateDefinition;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateFixedRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedRate", typeof(EFS_Decimal),Order=9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Fixed Rate", IsVisible = true)]
        public EFS_Decimal rateFixedRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountYearFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayCountYearFraction", Order = 10)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Day count year fraction")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public EFS_Decimal dayCountYearFraction;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forecastAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("forecastAmount", Order = 11)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Forecast amount")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public Money forecastAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forecastRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("forecastRate", Order = 12)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Forecast rate")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public EFS_Decimal forecastRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period")]
        public EFS_Id efs_id;
        #endregion Members
        #region Constructors
        public CalculationPeriod()
        {
            amountNotionalAmount         = new EFS_Decimal();
            amountFxLinkedNotionalAmount = new FxLinkedNotionalAmount();
            rateFixedRate                = new EFS_Decimal();
            rateFloatingRateDefinition   = new FloatingRateDefinition();
        }
        #endregion Constructors
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
    #endregion CalculationPeriod
    #region CalculationPeriodAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [MainTitleGUI(Title = "Calculation Period Amount")]
    public partial class CalculationPeriodAmount : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Period Amount")]
        public EFS_RadioChoice calculationPeriodAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodAmountCalculationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculation", typeof(Calculation),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Calculation", IsVisible = true)]
        public Calculation calculationPeriodAmountCalculation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationPeriodAmountKnownAmountScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("knownAmountSchedule", typeof(KnownAmountSchedule),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Known Amount Schedule", IsVisible = true, IsCopyPaste = true)]
        public KnownAmountSchedule calculationPeriodAmountKnownAmountSchedule;
        #endregion Members
    }
    #endregion CalculationPeriodAmount
    #region CalculationPeriodDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("calculationPeriodDates", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class CalculationPeriodDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date", IsVisible = false)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice effectiveDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool effectiveDateAdjustableSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", typeof(AdjustableDate),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        public AdjustableDate effectiveDateAdjustable;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool effectiveDateRelativeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeEffectiveDate", typeof(AdjustedRelativeDateOffset),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date", IsVisible = true)]
        public AdjustedRelativeDateOffset effectiveDateRelative;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type Date")]
        public EFS_RadioChoice terminationDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool terminationDateAdjustableSpecified;
        [System.Xml.Serialization.XmlElementAttribute("terminationDate", typeof(AdjustableDate),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        public AdjustableDate terminationDateAdjustable;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool terminationDateRelativeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeTerminationDate", typeof(RelativeDateOffset),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date", IsVisible = true)]
        public RelativeDateOffset terminationDateRelative;

        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesAdjustments", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Dates Adjustments", IsVisible = false, IsCopyPaste = true)]
        public BusinessDayAdjustments calculationPeriodDatesAdjustments;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firstPeriodStartDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstPeriodStartDate", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Dates Adjustments")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Dates", IsVisible = false, IsGroup = true)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First period start date")]
        public AdjustableDate firstPeriodStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firstRegularPeriodStartDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstRegularPeriodStartDate", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First regular period start date")]
        public EFS_Date firstRegularPeriodStartDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firstCompoundingPeriodEndDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstCompoundingPeriodEndDate", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First compounding period end date")]
        public EFS_Date firstCompoundingPeriodEndDate;

        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lastRegularPeriodEndDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("lastRegularPeriodEndDate", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last regular period End Date")]
        public EFS_Date lastRegularPeriodEndDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stubPeriodTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("stubPeriodType", Order = 10)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Period Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public StubPeriodTypeEnum stubPeriodType;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodFrequency", Order = 11)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Dates")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Frequency", IsVisible = false)]
        public CalculationPeriodFrequency calculationPeriodFrequency;
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
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Frequency")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion CalculationPeriodDates
    #region CalculationPeriodDatesReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CalculationPeriodDatesReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion CalculationPeriodDatesReference
    #region CancelableProvision
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CancelableProvision : Exercise
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
        [ControlGUI(Name = "Buyer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
        [ControlGUI(Name = "Seller")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
        public EFS_RadioChoice cancelableExercise;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cancelableExerciseAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American Exercise", IsVisible = true)]
        public AmericanExercise cancelableExerciseAmerican;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cancelableExerciseBermudaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bermuda Exercise", IsVisible = true)]
        public BermudaExercise cancelableExerciseBermuda;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cancelableExerciseEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European Exercise", IsVisible = true)]
        public EuropeanExercise cancelableExerciseEuropean;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseNoticeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseNotice", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Notice")]
        public ExerciseNotice exerciseNotice;

        [System.Xml.Serialization.XmlElementAttribute("followUpConfirmation", Order = 7)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "followUpConfirmation")]
        public EFS_Boolean followUpConfirmation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cancelableProvisionAdjustedDatesSpecified;

        [System.Xml.Serialization.XmlElementAttribute("cancelableProvisionAdjustedDates",Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cancelable Provision Adjusted Dates")]
        public CancelableProvisionAdjustedDates cancelableProvisionAdjustedDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool finalCalculationPeriodDateAdjustmentSpecified;

        [System.Xml.Serialization.XmlElementAttribute("finalCalculationPeriodDateAdjustment",Order=9)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Final calculation period date adjustment")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "adjustment", IsClonable = true, IsChild = true)]
        public FinalCalculationPeriodDateAdjustment[] finalCalculationPeriodDateAdjustment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialFeeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initialFee", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Initial Fee")]
        public FpML.v44.Shared.SimplePayment initialFee;
        #endregion Members
    }
    #endregion CancelableProvision
    #region CancelableProvisionAdjustedDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CancelableProvisionAdjustedDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("cancellationEvent", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event", IsClonable = true)]
        public CancellationEvent[] cancellationEvent;
        #endregion Members
    }

    #endregion CancelableProvisionAdjustedDates
    #region CancellationEvent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CancellationEvent : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("adjustedExerciseDate", Order = 1)]
        [ControlGUI(Name = "adjusted Exercise Date", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_Date adjustedExerciseDate;
        [System.Xml.Serialization.XmlElementAttribute("adjustedEarlyTerminationDate", Order = 2)]
        [ControlGUI(Name = "adjusted Early Termination Date")]
        public EFS_Date adjustedEarlyTerminationDate;

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
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion CancellationEvent
    #region CapFloor
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    //[System.Xml.Serialization.XmlRootAttribute("capFloor", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("capFloor", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [MainTitleGUI(Title = "Cap Floor")]
    public partial class CapFloor : Product
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("capFloorStream",Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "CapFloor Stream", IsVisible = false, IsCopyPaste = true)]
        public InterestRateStream capFloorStream;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool premiumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("premium",Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cap Floor Stream")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Premium")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium", IsClonable = true, MinItem = 0)]
        [BookMarkGUI(IsVisible = false)]
        public Payment[] premium;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalPayment", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Additional Payment")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Additional Payment", IsClonable = true, IsChild = true, MinItem = 0, IsCopyPasteItem = true)]
        [BookMarkGUI(IsVisible = false)]
        public Payment[] additionalPayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool earlyTerminationProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Early Termination Provision")]
        public EarlyTerminationProvision earlyTerminationProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool implicitProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("implicitProvision", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Implicit Provisions")]
		public ImplicitProvision implicitProvision;
		#endregion Members
    }
    #endregion CapFloor
    #region Cashflows
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Cashflows
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("cashflowsMatchParameters", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "cashflowsMatchParameters")]
        public EFS_Boolean cashflowsMatchParameters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool principalExchangeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("principalExchange",Order=2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Principal exchanges")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Principal exchange", IsClonable = true, IsChild = true)]
        public PrincipalExchange[] principalExchange;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentCalculationPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentCalculationPeriod",Order=3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment calculation periods")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment calculation period", IsClonable = true, IsChild = true)]
        public PaymentCalculationPeriod[] paymentCalculationPeriod;
        #endregion Members
    }
    #endregion Cashflows

    #region DateRelativeToCalculationPeriodDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    // EG 20140702 New 
    public class DateRelativeToCalculationPeriodDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation period dates reference", IsClonable = true)]
        public PaymentDatesReference[] calculationPeriodDatesReference;
        #endregion Members
    }
    #endregion DateRelativeToCalculationPeriodDates
    #region DateRelativeToPaymentDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DateRelativeToPaymentDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("paymentDatesReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment dates reference", IsClonable = true)]
        public PaymentDatesReference[] paymentDatesReference;
        #endregion Members
    }
    #endregion DateRelativeToPaymentDates
    #region Discounting
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Discounting : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("discountingType", Order = 1)]
        [ControlGUI(Name = "Type")]
        public DiscountingTypeEnum discountingType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool discountRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("discountRate", Order = 2)]
        [ControlGUI(Name = "rate")]
        public EFS_Decimal discountRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool discountRateDayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("discountRateDayCountFraction", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "discountRateDayCountFraction", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public DayCountFractionEnum discountRateDayCountFraction;
        #endregion Members
    }
    #endregion Discounting

    #region ExerciseEvent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ExerciseEvent : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("adjustedExerciseDate", Order = 1)]
        [ControlGUI(Name = "Exercise Date")]
        public EFS_Date adjustedExerciseDate;
        [System.Xml.Serialization.XmlElementAttribute("adjustedRelevantSwapEffectiveDate", Order = 2)]
        [ControlGUI(Name = "RelevantSwap Effective Date", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Date adjustedRelevantSwapEffectiveDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedCashSettlementValuationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedCashSettlementValuationDate", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Settlement", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Date")]
        public EFS_Date adjustedCashSettlementValuationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedCashSettlementPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedCashSettlementPaymentDate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Date")]
        public EFS_Date adjustedCashSettlementPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedExerciseFeePaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedExerciseFeePaymentDate", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Settlement")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Fee Payment Date")]
        public EFS_Date adjustedExerciseFeePaymentDate;
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
        public ExerciseEvent()
        {
            adjustedCashSettlementValuationDate = new EFS_Date();
            adjustedCashSettlementPaymentDate = new EFS_Date();
            adjustedExerciseFeePaymentDate = new EFS_Date();
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
    #endregion ExerciseEvent
    #region ExtendibleProvision
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ExtendibleProvision : Exercise
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
        [ControlGUI(Name = "Buyer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
        [ControlGUI(Name = "Seller")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
        public EFS_RadioChoice extendibleExercise;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extendibleExerciseAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American Exercise", IsVisible = true)]
        public AmericanExercise extendibleExerciseAmerican;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extendibleExerciseBermudaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bermuda Exercise", IsVisible = true)]
        public BermudaExercise extendibleExerciseBermuda;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extendibleExerciseEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European Exercise", IsVisible = true)]
        public EuropeanExercise extendibleExerciseEuropean;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseNoticeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseNotice", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Notice")]
        public ExerciseNotice exerciseNotice;
        [System.Xml.Serialization.XmlElementAttribute("followUpConfirmation", Order = 7)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "followUpConfirmation")]
        public EFS_Boolean followUpConfirmation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extendibleProvisionAdjustedDatesSpecified;

        [System.Xml.Serialization.XmlElementAttribute("extendibleProvisionAdjustedDates",Order=8)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Extendible Provision Adjusted Dates")]
        public ExtendibleProvisionAdjustedDates extendibleProvisionAdjustedDates;
        #endregion Members
    }
    #endregion ExtendibleProvision
    #region ExtendibleProvisionAdjustedDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ExtendibleProvisionAdjustedDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("extensionEvent", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event", IsClonable = true)]
        public ExtensionEvent[] extensionEvent;
        #endregion Members
    }
    #endregion ExtendibleProvisionAdjustedDates
    #region ExtensionEvent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ExtensionEvent : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("adjustedExerciseDate", Order = 1)]
        [ControlGUI(Name = "adjusted Exercise Date")]
        public EFS_Date adjustedExerciseDate;
        [System.Xml.Serialization.XmlElementAttribute("adjustedExtendedTerminationDate", Order = 2)]
        [ControlGUI(Name = "adjusted Extended Termination Date", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Date adjustedExtendedTerminationDate;
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
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion ExtensionEvent

    #region FallbackReferencePrice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FallbackReferencePrice : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationPostponementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationPostponement", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Postponement")]
        public ValuationPostponement valuationPostponement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fallbackSettlementRateOptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fallbackSettlementRateOption",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Fallback settlement rate option")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fallback settlement rate option", IsClonable = true, IsChild = true)]
        public SettlementRateOption[] fallbackSettlementRateOption;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fallbackSurveyValuationPostponenmentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fallbackSurveyValuationPostponenment", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Fallback survey valuation postponenment")]
        public Empty fallbackSurveyValuationPostponenment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentDeterminationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgentDetermination", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Calculation agent")]
        public CalculationAgent calculationAgentDetermination;
        #endregion Members
    }
    #endregion FallbackReferencePrice
    #region FinalCalculationPeriodDateAdjustment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FinalCalculationPeriodDateAdjustment : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("relevantUnderlyingDateReference", Order = 1)]
        public RelevantUnderlyingDateReference relevantUnderlyingDateReference;
        [System.Xml.Serialization.XmlElementAttribute("swapStreamReference", Order = 2)]
        public InterestRateStreamReference swapStreamReference;
        [System.Xml.Serialization.XmlElementAttribute("businessDayConvention", Order = 3)]
        [ControlGUI(Name = "convention")]
        public BusinessDayConventionEnum businessDayConvention;
        #endregion Members
    }
    #endregion FinalCalculationPeriodDateAdjustment
    #region FloatingRateDefinition
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FloatingRateDefinition
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculatedRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculatedRate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculated rate")]
        public EFS_Decimal calculatedRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateObservationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateObservation",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate observations")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate observation", IsMaster = true)]
        public RateObservation[] rateObservation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool floatingRateMultiplierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingRateMultiplier", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Multiplier rate")]
        public EFS_Decimal floatingRateMultiplier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spread", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread rate")]
        public EFS_Decimal spread;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool capRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("capRate",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cap rates")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cap rate", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        public Strike[] capRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool floorRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floorRate",Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Floor rates")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Floor rate", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        public Strike[] floorRate;
        #endregion Members
    }
    #endregion FloatingRateDefinition
    #region Fra
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("fra", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Fra")]
    public partial class Fra : Product
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
        [ControlGUI(Name = "Buyer", LblWidth = 120, LineFeed = MethodsGUI.LineFeedEnum.Before)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
        [ControlGUI(Name = "Seller")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("adjustedEffectiveDate", Order = 3)]
        [ControlGUI(Name = "Effective date", LblWidth = 120, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public RequiredIdentifierDate adjustedEffectiveDate;
		[System.Xml.Serialization.XmlElementAttribute("adjustedTerminationDate", Order = 4)]
        [ControlGUI(Name = "Termination date", LblWidth = 120, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_Date adjustedTerminationDate;
		[System.Xml.Serialization.XmlElementAttribute("paymentDate", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment date", IsVisible = false)]
        public AdjustableDate paymentDate;
		[System.Xml.Serialization.XmlElementAttribute("fixingDateOffset", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fixing dates", IsVisible = false, IsCopyPaste = true)]
        public RelativeDateOffset fixingDateOffset;
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Order = 7)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fixing dates")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Day count fraction")]
        public DayCountFractionEnum dayCountFraction;
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodNumberOfDays", Order = 8)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Calculation period number of days", Width = 75)]
        public EFS_PosInteger calculationPeriodNumberOfDays;
		[System.Xml.Serialization.XmlElementAttribute("notional", Order = 9)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Notional", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money notional;
		[System.Xml.Serialization.XmlElementAttribute("fixedRate", Order = 10)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Notional")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate", IsVisible = false)]
        [ControlGUI(Name = "Fixed rate")]
        public EFS_Decimal fixedRate;
		[System.Xml.Serialization.XmlElementAttribute("floatingRateIndex", Order = 11)]
        [ControlGUI(Name = "Floating rate index", Width = 295)]
        public FloatingRateIndex floatingRateIndex;
        [System.Xml.Serialization.XmlElementAttribute("indexTenor",Order=12)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index tenor", IsMaster = true)]
        public Interval[] indexTenor;
		[System.Xml.Serialization.XmlElementAttribute("fraDiscounting", Order = 13)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Discounting")]
        public FraDiscountingEnum fraDiscounting;
        #endregion Members
    }
    #endregion Fra
    #region FxFixingDate
    // EG 20140702 New build FpML4.4 (dateRelativeTo choice)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FxFixingDate : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("periodMultiplier", Order = 1)]
        [ControlGUI(Name = "multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer periodMultiplier;
        [System.Xml.Serialization.XmlElementAttribute("period", Order = 2)]
        [ControlGUI(Name = "period", Width = 70)]
        public PeriodEnum period;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayType", Order = 3)]
        [ControlGUI(Name = "dayType")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public DayTypeEnum dayType;
        [System.Xml.Serialization.XmlElementAttribute("businessDayConvention", Order = 4)]
        [ControlGUI(Name = "convention")]
        public BusinessDayConventionEnum businessDayConvention;

        // Business Centers Choice
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Centers")]
        public EFS_RadioChoice businessCenters;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessCenters businessCentersDefine;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 200)]
        public BusinessCentersReference businessCentersReference;

        // dateRelativeTo Choice
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date relative to")]
        public EFS_RadioChoice dateRelativeTo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dateRelativeToPaymentDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dateRelativeToPaymentDates", typeof(DateRelativeToPaymentDates), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public DateRelativeToPaymentDates dateRelativeToPaymentDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dateRelativeToCalculationPeriodDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dateRelativeToCalculationPeriodDates", typeof(DateRelativeToCalculationPeriodDates), Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 200)]
        public DateRelativeToCalculationPeriodDates dateRelativeToCalculationPeriodDates;
        #endregion Members
        #region Constructors
        public FxFixingDate()
        {
            periodMultiplier = new EFS_Integer();
            period = new PeriodEnum();
            dayType = new DayTypeEnum();
            businessDayConvention = new BusinessDayConventionEnum();
            businessCentersReference = new BusinessCentersReference();
            businessCentersDefine = new BusinessCenters();

            dateRelativeToCalculationPeriodDates = new DateRelativeToCalculationPeriodDates();
            dateRelativeToPaymentDates = new DateRelativeToPaymentDates();
        }
        #endregion Constructors
    }
    #endregion FxFixingDate
    #region FxLinkedNotionalAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FxLinkedNotionalAmount : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool resetDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("resetDate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset date")]
        public EFS_Date resetDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedFxSpotFixingDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedFxSpotFixingDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observed spot rate date")]
        public EFS_Date adjustedFxSpotFixingDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observedFxSpotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observedFxSpotRate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Actual observed spot rate")]
        public EFS_Decimal observedFxSpotRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalAmount", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional amount")]
        public EFS_Decimal notionalAmount;
        #endregion Members
    }
    #endregion FxLinkedNotionalAmount
    #region FxLinkedNotionalSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FxLinkedNotionalSchedule : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("constantNotionalScheduleReference", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "constantNotionalScheduleReference", Width = 170)]
        public ScheduleReference constantNotionalScheduleReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initialValue", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial Value", Width = 170)]
        public EFS_Decimal initialValue;
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "currency", Width = 75)]
        [System.Xml.Serialization.XmlElementAttribute("varyingNotionalCurrency",Order=3)]
        public EFS_Scheme currency;
        [System.Xml.Serialization.XmlElementAttribute("varyingNotionalFixingDates", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Dates", IsVisible = false, IsCopyPaste = true)]
        public RelativeDateOffset varyingNotionalFixingDates;
        [System.Xml.Serialization.XmlElementAttribute("fxSpotRateSource", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Dates")]
        public FxSpotRateSource fxSpotRateSource;
        [System.Xml.Serialization.XmlElementAttribute("varyingNotionalInterimExchangePaymentDates", Order = 6)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interim Exchange Payment Dates", IsVisible = false, IsCopyPaste = true)]
        public RelativeDateOffset varyingNotionalInterimExchangePaymentDates;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interim Exchange Payment Dates")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
        #endregion Members
    }
    #endregion FxLinkedNotionalSchedule

    #region InflationRateCalculation
    /// <summary>
    /// A type defining the components specifiying an Inflation Rate Calculation
    /// </summary>
    // EG 20140702 New build FpML4.4
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("inflationRateCalculation", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class InflationRateCalculation : FloatingRateCalculation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("inflationLag", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Inflation lag", IsVisible = false)]
        public Offset inflationLag;
        [System.Xml.Serialization.XmlElementAttribute("indexSource", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Inflation lag")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Index source")]
        public RateSourcePage indexSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mainPublicationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("mainPublication", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Main publication")]
        public MainPublication mainPublication;
        [System.Xml.Serialization.XmlElementAttribute("interpolationMethod", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Interpolation method")]
        public InterpolationMethod interpolationMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialIndexLevelSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initialIndexLevel", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Initial index level")]
        public EFS_Decimal initialIndexLevel;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fallbackBondApplicableSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fallbackBondApplicable", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "fallback Bond applicable")]
        public EFS_Boolean fallbackBondApplicable;
        #endregion Members
    }
    #endregion InflationRateCalculation
    #region InterestRateStream
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InterestRateStream : ItemGUI
    {
        #region Members
        [ControlGUI(Name = "Payer")]
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference payerPartyReference;
        [ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=2)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference receiverPartyReference;
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Period Dates", IsVisible = false, IsCopyPaste = true)]
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodDates", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=3)]
        public CalculationPeriodDates calculationPeriodDates;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Period Dates")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Dates", IsVisible = false, IsCopyPaste = true)]
        [System.Xml.Serialization.XmlElementAttribute("paymentDates", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=4)]
        public PaymentDates paymentDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool resetDatesSpecified;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Dates")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Reset Dates", IsCopyPaste = true)]
        [System.Xml.Serialization.XmlElementAttribute("resetDates", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=5)]
        public ResetDates resetDates;
        [ControlGUI(IsLabel = false, Name = "Calculation Period Amount", IsCopyPaste = true)]
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=6)]
        public CalculationPeriodAmount calculationPeriodAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stubCalculationPeriodAmountSpecified;
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Stub Calculation Period amounts", IsCopyPaste = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [System.Xml.Serialization.XmlElementAttribute("stubCalculationPeriodAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=7)]
        public StubCalculationPeriodAmount stubCalculationPeriodAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool principalExchangesSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Principal Exchanges")]
        [System.Xml.Serialization.XmlElementAttribute("principalExchanges", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=8)]
        public PrincipalExchanges principalExchanges;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashflowsSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cashflows")]
        [System.Xml.Serialization.XmlElementAttribute("cashflows", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=9)]
        public Cashflows cashflows;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementProvisionSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement Provision")]
        [System.Xml.Serialization.XmlElementAttribute("settlementProvision", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=10)]
        public SettlementProvision settlementProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Formula")]
        [System.Xml.Serialization.XmlElementAttribute("formula", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=11)]
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
    #endregion InterestRateStream
    #region InterestRateStreamReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class InterestRateStreamReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion InterestRateStreamReference

    #region NonDeliverableSettlement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class NonDeliverableSettlement : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("referenceCurrency", Order = 1)]
        [ControlGUI(Name = "Currency", Width = 75)]
        public Currency referenceCurrency;
        [System.Xml.Serialization.XmlElementAttribute("fxFixingDate", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "FxFixing Date", IsVisible = false)]
        public FxFixingDate fxFixingDate;
        [System.Xml.Serialization.XmlElementAttribute("settlementRateOption", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "FxFixing Date")]
        public SettlementRateOption settlementRateOption;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceSourceDisruptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("priceSourceDisruption",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Price source disruption")]
        public PriceSourceDisruption priceSourceDisruption;
        #endregion Members
    }
    #endregion NonDeliverableSettlement
    #region Notional
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [MainTitleGUI(Title = "Notional")]
    public partial class Notional : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("notionalStepSchedule", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional Step", IsVisible = true,IsAutoClose=true)]
        [ControlGUI(IsLabel = false, Name = "Notional Step", IsCopyPaste = true)]
        public AmountSchedule notionalStepSchedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalStepParametersSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalStepParameters", Order = 2)]
        //[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "notionalStepSchedule")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "NotionalStepParameters", IsCopyPaste = true, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public NotionalStepRule notionalStepParameters;
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
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentQuote)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion Notional
    #region NotionalStepRule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("notionalStepParameters", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class NotionalStepRule : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesReference", Order = 1)]
        [ControlGUI(Name = "calculationPeriodDatesReference", LblWidth = 200, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 185)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public DateReference calculationPeriodDatesReference;
        [System.Xml.Serialization.XmlElementAttribute("stepFrequency", Order = 2)]
        [ControlGUI(Name = "Frequency", LblWidth = 170, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval stepFrequency;
        [System.Xml.Serialization.XmlElementAttribute("firstNotionalStepDate", Order = 3)]
        [ControlGUI(Name = "1st Step Date", LblWidth = 279, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Date firstNotionalStepDate;
        [System.Xml.Serialization.XmlElementAttribute("lastNotionalStepDate", Order = 4)]
        [ControlGUI(Name = "Last Step Date", LblWidth = 279, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Date lastNotionalStepDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional Step")]
        public EFS_RadioChoice notionalStep;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalStepAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalStepAmount", typeof(EFS_Decimal),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Regex = EFSRegex.TypeRegex.RegexAmountExtend)]
        public EFS_Decimal notionalStepAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalStepRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalStepRate", typeof(EFS_Decimal),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Rate", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Regex = EFSRegex.TypeRegex.RegexFixedRateExtend)]
        public EFS_Decimal notionalStepRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("stepRelativeTo", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Relative To")]
        public StepRelativeToEnum stepRelativeTo;
        #endregion Members
    }
    #endregion NotionalStepRule

    #region PaymentCalculationPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PaymentCalculationPeriod
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool unadjustedPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unadjustedPaymentDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment dates", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unadjusted payment date")]
        public EFS_Date unadjustedPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted payment date")]
        public EFS_Date adjustedPaymentDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment dates")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Parameters")]
        public EFS_RadioChoice parameters;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool parametersCalculationPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriod", typeof(CalculationPeriod),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Calculation period", IsVisible = true)]
        public CalculationPeriod parametersCalculationPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool parametersFixedPaymentAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedPaymentAmount", typeof(EFS_Decimal),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Fixed payment amount", IsVisible = true)]
        public EFS_Decimal parametersFixedPaymentAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool discountFactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("discountFactor", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Discount factor")]
        public EFS_Decimal discountFactor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forecastPaymentAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("forecastPaymentAmount", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Forecast payment amount")]
        public Money forecastPaymentAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool presentValueAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("presentValueAmount", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Present value amount")]
        public Money presentValueAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public EFS_Href efs_href;
        #endregion Members
        #region Constructors
        public PaymentCalculationPeriod()
        {
            parametersCalculationPeriod = new CalculationPeriod();
            parametersFixedPaymentAmount = new EFS_Decimal();
        }
        #endregion Constructors
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
        [System.Xml.Serialization.XmlAttributeAttribute("href", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string Href
        {
            set { efs_href = new EFS_Href(value); }
            get
            {
                if (efs_href == null)
                    return null;
                else
                    return efs_href.Value;
            }
        }
        #endregion Accessors
    }
    #endregion PaymentCalculationPeriod
    #region PaymentDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("paymentDates", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [MainTitleGUI(Title = "Payment dates")]
    // EG 20140702 Upd 
    public partial class PaymentDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference to")]
        public EFS_RadioChoice paymentDatesDateReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDatesDateReferenceCalculationPeriodDatesReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesReference", typeof(DateReference),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", Width = 190)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public DateReference paymentDatesDateReferenceCalculationPeriodDatesReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDatesDateReferenceResetDatesReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("resetDatesReference", typeof(DateReference),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", Width = 190)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.ResetDates)]
        public DateReference paymentDatesDateReferenceResetDatesReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDatesDateReferenceValuationDatesReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationDatesReference", typeof(DateReference), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", Width = 190)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.ValuationDates)]
        public DateReference paymentDatesDateReferenceValuationDatesReference;
        [System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Frequency", IsVisible = false)]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firstPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstPaymentDate", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Frequency")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Payment Dates", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First payment date")]
        public EFS_Date firstPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastRegularPaymentDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("lastRegularPaymentDate", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Regular payment date")]
        public EFS_Date lastRegularPaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("payRelativeTo", Order = 7)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Payment Dates")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates Calculation", IsVisible = false, IsGroup = true)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Payment Relative to", LblWidth = 228, Width = 210)]
        public PayRelativeToEnum payRelativeTo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDaysOffsetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentDaysOffset", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Offset")]
        public Offset paymentDaysOffset;
        [System.Xml.Serialization.XmlElementAttribute("paymentDatesAdjustments", Order = 9)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates Adjustments", IsVisible = false, IsCopyPaste = true)]
        public BusinessDayAdjustments paymentDatesAdjustments;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates Adjustments")]
        public bool FillBalise;

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
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates Calculation")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentDates)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
		#region Members
		/*
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
		public bool streamExtensionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("streamExtension",Namespace="http://www.efs.org/2007/EFSmL-3-0")]
		[CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level=MethodsGUI.LevelEnum.Intermediary,Name="Extensions...")]
		public StreamExtension streamExtension;
		*/
		#endregion Members

    }
    #endregion PaymentDates
    #region PaymentDatesReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PaymentDatesReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion PaymentDatesReference
    #region PriceSourceDisruption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PriceSourceDisruption
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("fallbackReferencePrice", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fallback reference price", IsVisible = false)]
        public FallbackReferencePrice fallbackReferencePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fallback reference price")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion PriceSourceDisruption
    #region PrincipalExchange
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PrincipalExchange : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool unadjustedPrincipalExchangeDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unadjustedPrincipalExchangeDate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unadjusted principal exchange date")]
        public EFS_Date unadjustedPrincipalExchangeDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedPrincipalExchangeDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedPrincipalExchangeDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted principal exchange date")]
        public EFS_Date adjustedPrincipalExchangeDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool principalExchangeAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("principalExchangeAmount", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Principal exchange amount")]
        public EFS_Decimal principalExchangeAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool discountFactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("discountFactor", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Discount Factor")]
        public EFS_Decimal discountFactor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool presentValuePrincipalExchangeAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("presentValuePrincipalExchangeAmount", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Present Value")]
        public Money presentValuePrincipalExchangeAmount;
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
    }
    #endregion PrincipalExchange

    #region RelevantUnderlyingDateReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RelevantUnderlyingDateReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion RelevantUnderlyingDateReference
    #region ResetDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("resetDates", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class ResetDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesReference", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "calculationPeriodDatesReference", LblWidth = 225, Width = 220)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public DateReference calculationPeriodDatesReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool resetRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("resetRelativeTo", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Relative to", Width = 220)]
        public ResetRelativeToEnum resetRelativeTo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialFixingDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initialFixingDate", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial Fixing Dates", IsCopyPaste = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public RelativeDateOffset initialFixingDate;
        [System.Xml.Serialization.XmlElementAttribute("fixingDates", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Dates", IsVisible = false, IsGroup = true, IsCopyPaste = true)]
        public RelativeDateOffset fixingDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateCutOffDaysOffsetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateCutOffDaysOffset", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Dates")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate CutOff Days Offset")]
        public Offset rateCutOffDaysOffset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Dates Calculation", IsVisible = false, IsGroup = true)]
        public bool FillBalise;
        [System.Xml.Serialization.XmlElementAttribute("resetFrequency", Order = 6)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Frequency", IsVisible = false)]
        public ResetFrequency resetFrequency;
        [System.Xml.Serialization.XmlElementAttribute("resetDatesAdjustments", Order = 7)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Frequency")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Dates Adjustments", IsVisible = false, IsCopyPaste = true)]
        public BusinessDayAdjustments resetDatesAdjustments;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Dates Adjustments")]
        public bool FillBalise2;
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
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Dates Calculation")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.ResetDates)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion ResetDates
    #region ResetDatesReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ResetDatesReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion ResetDatesReference

    #region SettlementProvision
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SettlementProvision : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementCurrency", Order = 1)]
        [ControlGUI(Name = "currency", LblWidth = 115, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Currency settlementCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nonDeliverableSettlementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("nonDeliverableSettlement", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Non-deliverable settlement")]
        public NonDeliverableSettlement nonDeliverableSettlement;
        #endregion Members
    }
    #endregion SettlementProvision
    #region SettlementRateOption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SettlementRateOption : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string settlementRateOptionScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = "value", LblWidth = 50, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public string Value;
        #endregion Members
        #region Constructors
        public SettlementRateOption()
        {
            settlementRateOptionScheme = "http://www.fpml.org/coding-scheme/settlement-rate-option-2-1";
            Value = string.Empty;
        }
        #endregion Constructors
    }
    #endregion RoutingId
    #region StubCalculationPeriodAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class StubCalculationPeriodAmount : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesReference", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "calculationPeriodDatesReference", LblWidth = 221, Width = 190)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public DateReference calculationPeriodDatesReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialStubSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initialStub", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Initial Stub")]
        public Stub initialStub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool finalStubSpecified;
        [System.Xml.Serialization.XmlElementAttribute("finalStub", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Final Stub")]
        public Stub finalStub;
        #endregion Members
    }
    #endregion StubCalculationPeriodAmount
    #region Swap
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("swap", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [MainTitleGUI(Title="Swap")]
    public partial class Swap : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("swapStream", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Swap Stream", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 2, IsCopyPasteItem = true, Color = MethodsGUI.ColorEnum.Green)]
        [BookMarkGUI(Name = "S", IsVisible = true)]
        public InterestRateStream[] swapStream;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool earlyTerminationProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Early Termination Provision")]
        public EarlyTerminationProvision earlyTerminationProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cancelableProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cancelableProvision", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cancelable Provision")]
        public CancelableProvision cancelableProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extendibleProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("extendibleProvision", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Extendible Provision")]
        public ExtendibleProvision extendibleProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalPayment", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Additional Payment")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Additional Payment", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        [BookMarkGUI(IsVisible = false)]
        public Payment[] additionalPayment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalTermsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalTerms", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Additional Terms")]
        public SwapAdditionalTerms additionalTerms;
        #endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stepUpProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("stepUpProvision", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Step-Up Provision")]
		public StepUpProvision stepUpProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool implicitProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("implicitProvision", Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Implicit Provisions")]
		public ImplicitProvision implicitProvision;
		#endregion Members
    }
    #endregion Swap
    #region SwapAdditionalTerms
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SwapAdditionalTerms : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool bondReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bondReference", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Bond reference")]
        public BondReference bondReference;
        #endregion Members
    }
    #endregion SwapAdditionalTerms
    #region Swaption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("swaption", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class Swaption : Product
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool premiumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("premium",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Premium")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public Payment[] premium;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Exercise")]
        public EFS_RadioChoice exercise;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American Exercise", IsVisible = true)]
        public AmericanExercise exerciseAmerican;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseBermudaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bermuda Exercise", IsVisible = true)]
        public BermudaExercise exerciseBermuda;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European Exercise", IsVisible = true)]
        public EuropeanExercise exerciseEuropean;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool procedureSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseProcedure",Order=7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Exercise Procedure")]
        public ExerciseProcedure procedure;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgent",Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Calculation Agent")]
        public CalculationAgent calculationAgent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlement", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cash Settlement")]
        public CashSettlement cashSettlement;
		[System.Xml.Serialization.XmlElementAttribute("swaptionStraddle", Order = 10)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Swaption Straddle")]
        public EFS_Boolean swaptionStraddle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool swaptionAdjustedDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("swaptionAdjustedDates",Order=11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Swaption Adjusted Dates")]
        public SwaptionAdjustedDates swaptionAdjustedDates;
		[System.Xml.Serialization.XmlElementAttribute("swap", Order = 12)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Swap", IsVisible = false)]
        public Swap swap;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Swap")]
		public bool FillBalise;
        #endregion Members
    }
    #endregion Swaption
    #region SwaptionAdjustedDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SwaptionAdjustedDates : ItemGUI
    {
        [System.Xml.Serialization.XmlElementAttribute("exerciseEvent",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Swaption Adjusted Dates", IsClonable = true, IsChild = true)]
        public ExerciseEvent[] exerciseEvent;
    }
    #endregion SwaptionAdjustedDates

    #region ValuationDatesReference
    /// <summary>
    /// Reference to a Valuation dates node.
    ///  A pointer style reference to the associated valuation dates component defined elsewhere in the document. 
    ///  Implemented for Brazilian-CDI Swaps where it will refer to the 
    ///  settlemementProvision/nonDeliverableSettlement/fxFixingDate structure 
    /// </summary>
    // EG 20140702 New build FpML4.4
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ValuationDatesReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion ValuationDatesReference

    #region ValuationPostponement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ValuationPostponement : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("maximumDaysOfPostponement", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Maximum days of postponement", Width = 75)]
        public EFS_PosInteger maximumDaysOfPostponement;
        #endregion Members
    }
    #endregion ValuationPostponement
}

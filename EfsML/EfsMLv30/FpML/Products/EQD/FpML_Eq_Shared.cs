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

using EfsML;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;

using EfsML.v30;
using EfsML.v30.Shared;
using EfsML.v30.Option.Shared;

using FpML.Enum;

using FpML.v44.Assetdef;
using FpML.v44.CorrelationSwaps;
using FpML.v44.Enum;
using FpML.v44.Option.Shared;
using FpML.v44.Shared;
using FpML.v44.DividendSwaps;
using FpML.v44.ReturnSwaps;
using FpML.v44.VarianceSwaps;
#endregion using directives

namespace FpML.v44.Eq.Shared
{
    #region AdditionalDisruptionEvents
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AdditionalDisruptionEvents : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("changeInLaw", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "changeInLaw")]
        public EFS_Boolean changeInLaw;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool failureToDeliverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("failureToDeliver", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "failureToDeliver")]
        public EFS_Boolean failureToDeliver;
        [System.Xml.Serialization.XmlElementAttribute("insolvencyFiling", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Others flags", IsVisible = false, IsGroup = true)]
        [ControlGUI(Name = "insolvencyFiling", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean insolvencyFiling;
        [System.Xml.Serialization.XmlElementAttribute("hedgingDisruption", Order = 4)]
        [ControlGUI(Name = "hedgingDisruption", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean hedgingDisruption;
        [System.Xml.Serialization.XmlElementAttribute("lossOfStockBorrow", Order = 5)]
        [ControlGUI(Name = "lossOfStockBorrow", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean lossOfStockBorrow;
        [System.Xml.Serialization.XmlElementAttribute("increasedCostOfStockBorrow", Order = 6)]
        [ControlGUI(Name = "increasedCostOfStockBorrow", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean increasedCostOfStockBorrow;
        [System.Xml.Serialization.XmlElementAttribute("increasedCostOfHedging", Order = 7)]
        [ControlGUI(Name = "increasedCostOfHedging", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean increasedCostOfHedging;
        [System.Xml.Serialization.XmlElementAttribute("determiningPartyReference", Order = 8)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Others flags")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Determining Party")]
        public PartyReference determiningPartyReference;
        #endregion Members
    }
    #endregion AdditionalDisruptionEvents
    #region AdditionalPaymentAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AdditionalPaymentAmount : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentAmount", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount")]
        public Money paymentAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool formulaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("formula", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Formula")]
        public Formula formula;
		#endregion Members
	}
    #endregion AdditionalPaymentAmount
    #region AdjustableDateOrRelativeDateSequence
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AdjustableDateOrRelativeDateSequence : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice adjustableOrRelativeDateSequence;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDateSequenceAdjustableDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", typeof(AdjustableDate),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        public AdjustableDate adjustableOrRelativeDateSequenceAdjustableDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDateSequenceRelativeDateSequenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDateSequence", typeof(RelativeDateSequence),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date Sequence", IsVisible = true, IsCopyPaste = true)]
        public RelativeDateSequence adjustableOrRelativeDateSequenceRelativeDateSequence;

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
    #endregion AdjustableDateOrRelativeDateSequence

    #region BoundedCorrelation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BoundedCorrelation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minimumBoundaryPercentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("minimumBoundaryPercent", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Minimum boundary percent")]
        public EFS_Decimal minimumBoundaryPercent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maximumBoundaryPercentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("maximumBoundaryPercent", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maximum boundary percent")]
        public EFS_Decimal maximumBoundaryPercent;
        #endregion Members
    }
    #endregion BoundedCorrelation
    #region BoundedVariance
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BoundedVariance : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("realisedVarianceMethod", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Realised variance method")]
        public RealisedVarianceMethodEnum realisedVarianceMethod;
        [System.Xml.Serialization.XmlElementAttribute("daysInRangeAdjustment", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Number of days in range")]
        public EFS_Boolean daysInRangeAdjustment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool upperBarrierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("upperBarrier", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Up barrier")]
        public EFS_Decimal upperBarrier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lowerBarrierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("lowerBarrier", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Low barrier")]
        public EFS_Decimal lowerBarrier;


        #endregion Members
    }
    #endregion BoundedVariance

    #region CalculatedAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(VarianceAmount))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CorrelationAmount))]
    public abstract class CalculatedAmount : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationDates", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation dates")]
        public AdjustableRelativeOrPeriodicDates calculationDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationStartDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("observationStartDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation start date")]
        public AdjustableOrRelativeDate observationStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionsExchangeDividendsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("optionsExchangeDividends", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Options exchange dividends")]
        public EFS_Boolean optionsExchangeDividends;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalDividendsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalDividends", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Additional dividends")]
        public bool additionalDividends;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool allDividendsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("allDividends", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "All dividends")]
        public EFS_Boolean allDividends;
        #endregion Members
    }
    #endregion CalculatedAmount
    #region CalculationFromObservation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Correlation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Variance))]
    public abstract class CalculationFromObservation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Level")]
        public EFS_RadioChoice level;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool levelInitialSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initialLevel", typeof(EFS_Decimal),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal levelInitial;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool levelExpiringSpecified;
        [System.Xml.Serialization.XmlElementAttribute("expiringLevel", typeof(EFS_Boolean),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Boolean levelExpiring;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool levelClosingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("closingLevel", typeof(EFS_Boolean),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Boolean levelClosing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool expectedNSpecified;
        [System.Xml.Serialization.XmlElementAttribute("expectedN", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Expected number of trading days", Width = 70)]
        public EFS_PosInteger expectedN;
        #endregion Members
        #region Constructors
        public CalculationFromObservation()
        {
            levelInitial = new EFS_Decimal();
            levelExpiring = new EFS_Boolean(true);
            levelClosing = new EFS_Boolean(true);
        }
        #endregion Constructors
    }
    #endregion CalculationFromObservation
    #region Compounding
    // EG 20140702 New build FpML4.4 Add compoundingMethodSpecified
    // EG 20140702 New build FpML4.4 Add compoundingSpread
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Compounding
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool compoundingMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("compoundingMethod", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Method")]
        public CompoundingMethodEnum compoundingMethod;
        [System.Xml.Serialization.XmlElementAttribute("compoundingRate", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate")]
        public CompoundingRate compoundingRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool compoundingSpreadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("compoundingRate", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread")]
        public EFS_Decimal compoundingSpread;
        #endregion Members
    }
    #endregion Compounding
    #region CompoundingRate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CompoundingRate
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice type;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeInterestLegRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("interestLegRate", typeof(InterestCalculationReference),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public InterestCalculationReference typeInterestLegRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeSpecificRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("specificRate", typeof(InterestAccrualsMethod),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public InterestAccrualsMethod typeSpecificRate;
        #endregion Members
    }
    #endregion CompoundingRate
    #region Correlation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Correlation : CalculationFromObservation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("notionalAmount", Order = 1)]
        [ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Money notionalAmount;
        [System.Xml.Serialization.XmlElementAttribute("correlationStrikePrice", Order = 2)]
        [ControlGUI(Name = "Correlation strike price", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal correlationStrikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool boundedCorrelationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("boundedCorrelation", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Bounded correlation")]
        public BoundedCorrelation boundedCorrelation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool numberOfDataSeriesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("numberOfDataSeries", DataType = "positiveInteger", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Number of data series")]
        public string numberOfDataSeries;
        #endregion Members
    }
    #endregion Correlation

    #region DirectionalLeg
    // EG 20140702 New build FpML4.4 Add legIdentifier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FixedPaymentLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DirectionalLegUnderlyer))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DirectionalLegUnderlyerValuation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(VarianceLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CorrelationLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DividendLeg))]
    public abstract class DirectionalLeg : Leg
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legIdentifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("legIdentifier", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "legIdentifier")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "legIdentifier", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        [BookMarkGUI(IsVisible = false)]
        public LegIdentifier[] legIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 2)]
        [ControlGUI(Name = "Payer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference payerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 3)]
        [ControlGUI(Name = "Receiver")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference receiverPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date", IsVisible = false)]
        public AdjustableOrRelativeDate effectiveDate;
        [System.Xml.Serialization.XmlElementAttribute("terminationDate", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
        public AdjustableOrRelativeDate terminationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date")]
        public bool FillBalise;
        [System.Xml.Serialization.XmlAttributeAttribute("id", DataType = "ID")]
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
    #endregion DirectionalLeg
    #region DirectionalLegUnderlyer
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DirectionalLegUnderlyerValuation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(VarianceLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CorrelationLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DividendLeg))]
    public abstract class DirectionalLegUnderlyer : DirectionalLeg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("underlyer", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        public Underlyer underlyer;
        [System.Xml.Serialization.XmlElementAttribute("settlementType", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement")]
        [ControlGUI(Name = "Type", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public SettlementTypeEnum settlementType;
        [System.Xml.Serialization.XmlElementAttribute("settlementDate", Order = 3)]
        [ControlGUI(Name = "Date", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public AdjustableOrRelativeDate settlementDate;
        [System.Xml.Serialization.XmlElementAttribute("settlementAmount", Order = 4)]
        [ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Money settlementAmount;
        [System.Xml.Serialization.XmlElementAttribute("settlementCurrency", Order = 5)]
        [ControlGUI(Name = "Currency")]
        public Currency settlementCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFeatureSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxFeature", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Features")]
        public FxFeature fxFeature;
        #endregion Members
    }
    #endregion DirectionalLegUnderlyer
    #region DirectionalLegUnderlyerValuation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(VarianceLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CorrelationLeg))]
    public abstract class DirectionalLegUnderlyerValuation : DirectionalLegUnderlyer
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("valuation", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation")]
        public EquityValuation valuation;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise2;
        #endregion Members
    }
    #endregion DirectionalLegUnderlyerValuation
    #region DividendAdjustment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class DividendAdjustment : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("dividendPeriod", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend Periods", IsClonable = true)]
        public DividendPeriodDividend[] dividendPeriod;
        #endregion Members
    }
    #endregion DividendAdjustment
    #region DividendPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DividendPeriodPayment))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DividendPeriodDividend))]
    public abstract partial class DividendPeriod : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("unadjustedStartDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Divident periods dates", IsVisible = false)]
        [ControlGUI(Name = "Start date")]
        public IdentifiedDate unadjustedStartDate;
        [System.Xml.Serialization.XmlElementAttribute("unadjustedEndDate", Order = 2)]
        [ControlGUI(Name = "Start end date", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public IdentifiedDate unadjustedEndDate;
        [System.Xml.Serialization.XmlElementAttribute("dateAdjustments", Order = 3)]
        public BusinessDayAdjustments dateAdjustments;
        [System.Xml.Serialization.XmlElementAttribute("underlyerReference", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Divident periods dates")]
        [ControlGUI(Name = "Underlyer reference", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public AssetReference underlyerReference;
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
    #endregion DividendPeriod
    #region DividendPeriodDividend
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class DividendPeriodDividend : DividendPeriod
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("dividend", Order = 1)]
        [ControlGUI(Name = "Dividend")]
        public Money dividend;
        [System.Xml.Serialization.XmlElementAttribute("multiplier", Order = 2)]
        [ControlGUI(Name = "Multiplier")]
        public EFS_Decimal multiplier;
        #endregion Members
    }
    #endregion DividendPeriodDividend

    #region EquityCorporateEvents
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class EquityCorporateEvents : ItemGUI
    {
        [System.Xml.Serialization.XmlElementAttribute("shareForShare", Order = 1)]
        [ControlGUI(Name = "Share for share", LblWidth = 150, Width = 200, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public ShareExtraordinaryEventEnum shareForShare;
        [System.Xml.Serialization.XmlElementAttribute("shareForOther", Order = 2)]
        [ControlGUI(Name = "Share for other", LblWidth = 150, Width = 200, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public ShareExtraordinaryEventEnum shareForOther;
        [System.Xml.Serialization.XmlElementAttribute("shareForCombined", Order = 3)]
        [ControlGUI(Name = "Share for combined", LblWidth = 150, Width = 200, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public ShareExtraordinaryEventEnum shareForCombined;
    }
    #endregion EquityCorporateEvents
    #region EquityPremium
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class EquityPremium : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
        [ControlGUI(Name = "Payer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference payerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        [ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference receiverPartyReference;
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
    }
    #endregion EquityPremium
    #region EquityStrike
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class EquityStrike : ItemGUI
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
        [System.Xml.Serialization.XmlElementAttribute("strikePercentage", typeof(EFS_Decimal), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal typeStrikePercentage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strikeDeterminationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikeDeterminationDate", typeof(AdjustableOrRelativeDate), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public AdjustableOrRelativeDate strikeDeterminationDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
        #endregion Members
    }
    #endregion EquityStrike
    #region EquityValuation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class EquityValuation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationDate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Date")]
        public AdjustableDateOrRelativeDateSequence valuationDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationDates", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Dates")]
        public AdjustableRelativeOrPeriodicDates valuationDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationTimeTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationTimeType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Others", IsVisible = false, IsGroup = true)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public TimeTypeEnum valuationTimeType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationTime", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time")]
        public BusinessCenterTime valuationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool futuresPriceValuationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("futuresPriceValuation", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "futuresPriceValuation")]
        public EFS_Boolean futuresPriceValuation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionsPriceValuationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("optionsPriceValuation", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "optionsPriceValuation")]
        public EFS_Boolean optionsPriceValuation;
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
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Others")]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion EquityValuation
    #region ExtraordinaryEvents
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ExtraordinaryEvents : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mergerEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("mergerEvents",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Merger Events")]
        public EquityCorporateEvents mergerEvents;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tenderOfferSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tenderOffer", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "tenderOffer")]
        public EFS_Boolean tenderOffer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tenderOfferEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tenderOfferEvents", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Tender Offer Events")]
        public EquityCorporateEvents tenderOfferEvents;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool compositionOfCombinedConsiderationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("compositionOfCombinedConsideration", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "compositionOfCombinedConsideration")]
        public EFS_Boolean compositionOfCombinedConsideration;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexAdjustmentEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexAdjustmentEvents", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index Adjustment Events")]
        public IndexAdjustmentEvents indexAdjustmentEvents;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Type")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemFailureToDeliverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("failureToDeliver", typeof(EFS_Boolean),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "checked if yes")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "failureToDeliver", IsVisible = true)]
        public EFS_Boolean itemFailureToDeliver;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemAdditionalDisruptionEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalDisruptionEvents", typeof(AdditionalDisruptionEvents),Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Additional Disruption Events", IsVisible = true)]
        public AdditionalDisruptionEvents itemAdditionalDisruptionEvents;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool representationsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("representations",Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Representations")]
        public Representations representations;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nationalisationOrInsolvencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("nationalisationOrInsolvency",Order=9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Nationalisation Or Insolvency", Width = 200)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public NationalisationOrInsolvencyOrDelistingEventEnum nationalisationOrInsolvency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool delistingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("delisting", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Delisting", Width = 200)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public NationalisationOrInsolvencyOrDelistingEventEnum delisting;
        #endregion Members
    }
    #endregion ExtraordinaryEvents

    #region IndexAdjustmentEvents
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class IndexAdjustmentEvents : ItemGUI
    {
        [System.Xml.Serialization.XmlElementAttribute("indexModification", Order = 1)]
        [ControlGUI(Name = "Modification", LblWidth = 100, Width = 220, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public IndexEventConsequenceEnum indexModification;
        [System.Xml.Serialization.XmlElementAttribute("indexCancellation", Order = 2)]
        [ControlGUI(Name = "Cancellation", LblWidth = 100, Width = 220, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public IndexEventConsequenceEnum indexCancellation;
        [System.Xml.Serialization.XmlElementAttribute("indexDisruption", Order = 3)]
        [ControlGUI(Name = "Disruption", LblWidth = 100, Width = 220, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public IndexEventConsequenceEnum indexDisruption;
    }
    #endregion IndexAdjustmentEvents
    #region InterestCalculation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class InterestCalculation : InterestAccrualsMethod
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Day Count Fraction")]
        public DayCountFractionEnum dayCountFraction;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool compoundingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("compounding", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Compounding")]
        public Compounding compounding;
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
    #endregion InterestCalculation
    #region InterestCalculationReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class InterestCalculationReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion InterestCalculationReference
    #region InterestLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("interestLeg", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class InterestLeg : ReturnSwapLeg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("interestLegCalculationPeriodDates", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Dates", IsVisible = false)]
        public InterestLegCalculationPeriodDates interestLegCalculationPeriodDates;
        [System.Xml.Serialization.XmlElementAttribute("notional", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Dates")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional", IsVisible = false)]
        public ReturnSwapNotional notional;
        [System.Xml.Serialization.XmlElementAttribute("interestAmount", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest Amount", IsVisible = false)]
        public LegAmount interestAmount;
        [System.Xml.Serialization.XmlElementAttribute("interestCalculation", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest Calculation", IsVisible = false)]
        public InterestCalculation interestCalculation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stubCalculationPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("stubCalculationPeriod", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest Calculation")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Calculation", IsVisible = false)]
        public StubCalculationPeriod stubCalculationPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Calculation")]
        public bool FillBalise2;
        #endregion Members
    }
    #endregion InterestLeg
    #region InterestLegCalculationPeriodDates
    // EG 20140702 New build FpML4.4 Change Type of interestLegPaymentDates (AdjustableRelativeOrPeriodicDates2)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("interestLegCalculationPeriodDates", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class InterestLegCalculationPeriodDates
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date", IsVisible = false)]
        public AdjustableOrRelativeDate effectiveDate;
        [System.Xml.Serialization.XmlElementAttribute("terminationDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
        public AdjustableOrRelativeDate terminationDate;
        [System.Xml.Serialization.XmlElementAttribute("interestLegResetDates", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Dates", IsVisible = false)]
        public InterestLegResetDates interestLegResetDates;
        [System.Xml.Serialization.XmlElementAttribute("interestLegPaymentDates", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Dates")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates", IsVisible = false)]
        public AdjustableRelativeOrPeriodicDates2 interestLegPaymentDates;
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
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion InterestLegCalculationPeriodDates
    #region InterestLegCalculationPeriodDatesReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class InterestLegCalculationPeriodDatesReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion InterestLegCalculationPeriodDatesReference
    #region InterestLegResetDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    // EG 20140702 Add GUI
    public partial class InterestLegResetDates : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesReference", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "CalculationPeriodDates Reference", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
        public DateReference calculationPeriodDatesReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice resetDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool resetDatesResetRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("resetRelativeTo", typeof(ResetRelativeToEnum), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Reset Relative To", IsVisible = true)]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public ResetRelativeToEnum resetDatesResetRelativeTo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool resetDatesResetFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("resetFrequency", typeof(ResetFrequency), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Reset Frequency", IsVisible = true)]
        public ResetFrequency resetDatesResetFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialFixingDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initialFixingDate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial fixing date")]
        public RelativeDateOffset initialFixingDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixingDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixingDates", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing dates")]
        public AdjustableDatesOrRelativeDateOffset fixingDates;
        #endregion Members
    }
    #endregion InterestLegResetDates

    #region LegAmount
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnSwapAmount))]
    // EG 20231127 [WI755] Implementation Return Swap : Add legCurrencyNone
    public partial class LegAmount : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", IsVisible = false)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice legCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legCurrencyNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty legCurrencyNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legCurrencyCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", typeof(Currency),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Currency", IsVisible = true)]
        public Currency legCurrencyCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legCurrencyDeterminationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("determinationMethod", typeof(DeterminationMethod),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Formula", IsVisible = true)]
        public DeterminationMethod legCurrencyDeterminationMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legCurrencyCurrencyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("currencyReference", typeof(IdentifiedCurrencyReference),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Currency Reference", IsVisible = true)]
        public IdentifiedCurrencyReference legCurrencyCurrencyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentCurrency",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment currency")]
        public PaymentCurrency paymentCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice legAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legAmountReferenceAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("referenceAmount", typeof(ReferenceAmount),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Reference", IsVisible = true)]
        public ReferenceAmount legAmountReferenceAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legAmountFormulaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("formula", typeof(Formula),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Formula", IsVisible = true)]
        public Formula legAmountFormula;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool legAmountEncodedDescriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("encodedDescription", typeof(System.Byte[]), DataType = "base64Binary",Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Encoded Description", IsVisible = true)]
        public System.Byte[] legAmountEncodedDescription;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationDates", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Dates")]
        public AdjustableRelativeOrPeriodicDates calculationDates;
        #endregion Members
    }
    #endregion LegAmount

    #region MakeWholeProvisions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class MakeWholeProvisions : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("makeWholeDate", Order = 1)]
        [ControlGUI(Name = "Make whole date", LblWidth = 75)]
        public EFS_Date makeWholeDate;
        [System.Xml.Serialization.XmlElementAttribute("recallSpread", Order = 2)]
        [ControlGUI(Name = "Recall spread", LblWidth = 45)]
        public EFS_Decimal recallSpread;
        #endregion Members
    }
    #endregion MakeWholeProvisions

    #region NettedSwapBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(VarianceSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CorrelationSwap))]
    public abstract partial class NettedSwapBase : Product
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalPayment",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Additional Payments")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Additional Payment", IsClonable = true, IsChild = true)]
        public ClassifiedPayment[] additionalPayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extraordinaryEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("extraordinaryEvents",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Extraordinary events")]
        public ExtraordinaryEvents extraordinaryEvents;
        #endregion Members
    }
    #endregion NettedSwapBase

    #region OptionFeatures
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class OptionFeatures : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool asianSpecified;
		[System.Xml.Serialization.XmlElementAttribute("asian", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Asian")]
        public Asian asian;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool barrierSpecified;
		[System.Xml.Serialization.XmlElementAttribute("barrier", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Barrier")]
        public Barrier barrier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockSpecified;
		[System.Xml.Serialization.XmlElementAttribute("knock", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Knock")]
        public Knock knock;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool passThroughSpecified;
		[System.Xml.Serialization.XmlElementAttribute("passThrough", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Pass through payments")]
        public PassThrough  passThrough;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendAdjustmentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendAdjustment", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend adjustments")]
		public DividendAdjustment dividendAdjustment;
        #endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool multipleBarrierSpecified;
		[System.Xml.Serialization.XmlElementAttribute("multipleBarrier",Order = 6)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Multiple barriers")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Barrier", IsClonable = true, IsChild = true)]
		public ExtendedBarrier[] multipleBarrier;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool participationRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("participationRate",Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Participation rate")]
		public EFS_Decimal participationRate;
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:OptionFeatures";
		#endregion Members

    }
    #endregion OptionFeatures

    #region PrincipalExchangeAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PrincipalExchangeAmount : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Principal Exchange Amount")]
        public EFS_RadioChoice exchange;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeAmountRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amountRelativeTo", typeof(AmountReference),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount Relative To", IsVisible = true)]
        public AmountReference exchangeAmountRelativeTo;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeDeterminationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("determinationMethod", typeof(EFS_MultiLineString),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Determination Method", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 500)]
        public EFS_MultiLineString exchangeDeterminationMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangePrincipalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("principalAmount", typeof(Money),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Principal Amount", IsVisible = true)]
        public Money exchangePrincipalAmount;
        #endregion Members
    }
    #endregion PrincipalExchangeAmount
    #region PrincipalExchangeDescriptions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PrincipalExchangeDescriptions
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
        [System.Xml.Serialization.XmlElementAttribute("principalExchangeAmount", Order = 3)]
        public PrincipalExchangeAmount principalExchangeAmount;
        [System.Xml.Serialization.XmlElementAttribute("principalExchangeDate", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Principal Exchange Date", IsVisible = false)]
        public AdjustableOrRelativeDate principalExchangeDate;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Principal Exchange Date")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
        #endregion Members
    }
    #endregion PrincipalExchangeDescriptions
    #region PrincipalExchangeFeatures
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PrincipalExchangeFeatures : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("principalExchanges", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Principal Exchange", IsVisible = false)]
        public PrincipalExchanges principalExchanges;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool principalExchangeDescriptionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("principalExchangeDescriptions",Order=2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Principal Exchange")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Descriptions")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Description", IsChild = true)]
        public PrincipalExchangeDescriptions[] principalExchangeDescriptions;
		#endregion Members
	}
    #endregion PrincipalExchangeFeatures

    #region Representations
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Representations : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("nonReliance", Order = 1)]
        [ControlGUI(Name = "non Reliance", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean nonReliance;
        [System.Xml.Serialization.XmlElementAttribute("agreementsRegardingHedging", Order = 2)]
        [ControlGUI(Name = "agreements Regarding Hedging", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean agreementsRegardingHedging;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexDisclaimerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexDisclaimer", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "index Disclaimer")]
        public EFS_Boolean indexDisclaimer;
        [System.Xml.Serialization.XmlElementAttribute("additionalAcknowledgements", Order = 4)]
        [ControlGUI(Name = "additional Acknowledgements", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean additionalAcknowledgements;
        #endregion Members
    }
    #endregion Representations
    #region Return
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Return : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("returnType", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Type")]
        public ReturnTypeEnum returnType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendConditionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendConditions", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Dividend Conditions")]
        public DividendConditions dividendConditions;
		#endregion Members
	}
    #endregion Return
    #region ReturnLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("returnLeg", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class ReturnLeg : ReturnSwapLegUnderlyer
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("rateOfReturn", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate of return", IsVisible = false)]
        public ReturnLegValuation rateOfReturn;
        [System.Xml.Serialization.XmlElementAttribute("notional", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate of return")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional", IsVisible = false)]
        public ReturnSwapNotional notional;
        [System.Xml.Serialization.XmlElementAttribute("amount", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "ReturnSwap Amount", IsVisible = false)]
        public ReturnSwapAmount amount;
        [System.Xml.Serialization.XmlElementAttribute("return", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "ReturnSwap Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Return conditions", IsVisible = false)]
        public Return @return;
        [System.Xml.Serialization.XmlElementAttribute("notionalAdjustments", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Return conditions")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Notional Adjustments")]
        public NotionalAdjustmentEnum notionalAdjustments;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFeatureSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxFeature", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "FX feature")]
        public FxFeature fxFeature;
        #endregion Members
    }
    #endregion ReturnLeg
    #region ReturnLegValuation
    // EG 20140702 New build FpML4.4 notionalReset is optional
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ReturnLegValuation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("initialPrice", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial price", IsVisible = false)]
        public ReturnLegValuationPrice initialPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalResetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalReset", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial price")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Equity notional reset")]
        public EFS_Boolean notionalReset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationPriceInterimSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationPriceInterim", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interim price")]
        public ReturnLegValuationPrice valuationPriceInterim;
        [System.Xml.Serialization.XmlElementAttribute("valuationPriceFinal", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Final price", IsVisible = false)]
        public ReturnLegValuationPrice valuationPriceFinal;
        [System.Xml.Serialization.XmlElementAttribute("paymentDates", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Final price")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment dates of the swap", IsVisible = false)]
        public ReturnSwapPaymentDates paymentDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeTradedContractNearestSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedContractNearest", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment dates of the swap")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exchange traded contract nearest")]
        public ExchangeTradedContract exchangeTradedContractNearest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool marginRatioSpecified;
        [System.Xml.Serialization.XmlElementAttribute("marginRatio", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Margin ratio")]
        public MarginRatio marginRatio;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:ReturnLegValuation";

        #endregion Members
    }
    #endregion ReturnLegValuation
    #region ReturnLegValuationPrice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ReturnLegValuationPrice : Price
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationRulesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationRules", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation rules")]
        public EquityValuation valuationRules;
		#endregion Members
	}
    #endregion ReturnLegValuationPrice
    #region ReturnSwap
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("returnSwap", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public partial class ReturnSwap : ReturnSwapBase
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalPayment",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Additional Payment")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Additional Payment", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public ReturnSwapAdditionalPayment[] additionalPayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool earlyTerminationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("earlyTermination",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Early Termination")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Early Termination", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public ReturnSwapEarlyTermination[] earlyTermination;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extraordinaryEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("extraordinaryEvents",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Extraordinary Events")]
        public ExtraordinaryEvents extraordinaryEvents;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_PrincipalExchangeFeatures efs_InitialPrincipalExchangeFeatures;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_PrincipalExchangeFeatures efs_PrincipalExchangeFeatures;
        #endregion Members
    }
    #endregion ReturnSwap
    #region ReturnSwapAdditionalPayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ReturnSwapAdditionalPayment
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
        [System.Xml.Serialization.XmlElementAttribute("additionalPaymentAmount", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Amount", IsVisible = false)]
        public AdditionalPaymentAmount additionalPaymentAmount;
        [System.Xml.Serialization.XmlElementAttribute("additionalPaymentDate", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Date", IsVisible = false)]
        public AdjustableOrRelativeDate additionalPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentType", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public PaymentType paymentType;
        #endregion Members
    }
    #endregion ReturnSwapAdditionalPayment
    #region ReturnSwapAmount
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ReturnSwapAmount : LegAmount
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("cashSettlement", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Cash Settlement")]
        public EFS_Boolean cashSettlement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionsExchangeDividendsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("optionsExchangeDividends", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exchange Dividends")]
        public EFS_Boolean optionsExchangeDividends;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalDividendsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalDividends", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "additional Dividends")]
        public EFS_Boolean additionalDividends;
        #endregion Members
    }
    #endregion ReturnSwapAmount
    #region ReturnSwapBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquitySwapTransactionSupplement))]
    // EG 20140702 New rptSide
	public partial class ReturnSwapBase : Product
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool buyerPartyReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Buyer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sellerPartyReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Seller")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool returnLegSpecified;
        [System.Xml.Serialization.XmlElementAttribute("returnLeg",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Return Leg")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Return Leg", IsClonable = true, IsChild = true)]
        public ReturnLeg[] returnLeg;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interestLegSpecified;
        [System.Xml.Serialization.XmlElementAttribute("interestLeg",Order=4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Interest Leg")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Interest Leg", IsClonable = true, IsChild = true)]
        public InterestLeg[] interestLeg;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool principalExchangeFeaturesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("principalExchangeFeatures", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Principal Exchanges Features")]
        public PrincipalExchangeFeatures principalExchangeFeatures;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] RptSide (R majuscule)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public FixML.v50SP1.TrdCapRptSideGrp_Block[] RptSide;
        #endregion Members
    }
    #endregion ReturnSwapBase
    #region ReturnSwapEarlyTermination
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ReturnSwapEarlyTermination : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("partyReference", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Party Reference")]
        public PartyReference partyReference;
        [System.Xml.Serialization.XmlElementAttribute("startingDate", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Starting Date", IsVisible = false)]
        public StartingDate startingDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Starting Date")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion ReturnSwapEarlyTermination

    #region ReturnSwapLeg
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnSwapLegUnderlyer))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnLeg))]
    public abstract partial class ReturnSwapLeg : Leg
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
        public bool paymentFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlAttributeAttribute("legIdentifier", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string LegIdentifier
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
    #endregion ReturnSwapLeg
    #region ReturnSwapLegUnderlyer
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnLeg))]
    public abstract class ReturnSwapLegUnderlyer : ReturnSwapLeg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date", IsVisible = false)]
        public AdjustableOrRelativeDate effectiveDate;
        [System.Xml.Serialization.XmlElementAttribute("terminationDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
        public AdjustableOrRelativeDate terminationDate;
        [System.Xml.Serialization.XmlElementAttribute("underlyer", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date")]
        public Underlyer underlyer;
        #endregion Members
    }
    #endregion ReturnSwapLegUnderlyer
    #region ReturnSwapNotional
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ReturnSwapNotional : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice returnSwapNotional;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool returnSwapNotionalDeterminationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("determinationMethod", typeof(EFS_MultiLineString),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Determination Method", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 500)]
        public EFS_MultiLineString returnSwapNotionalDeterminationMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool returnSwapNotionalNotionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalAmount", typeof(Money),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Money returnSwapNotionalNotionalAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool returnSwapNotionalAmountRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amountRelativeTo", typeof(AmountReference),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative To", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 300)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public AmountReference returnSwapNotionalAmountRelativeTo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion ReturnSwapNotional
    #region ReturnSwapPaymentDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ReturnSwapPaymentDates
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDatesInterimSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentDatesInterim", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates Interim")]
        public AdjustableOrRelativeDates paymentDatesInterim;
        [System.Xml.Serialization.XmlElementAttribute("paymentDateFinal", Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Final Date")]
        public AdjustableOrRelativeDate paymentDateFinal;
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
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentDates)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion ReturnSwapPaymentDates

    #region StartingDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class StartingDate
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice startingDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool startingDateRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dateRelativeTo", typeof(DateReference),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative to", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public DateReference startingDateRelativeTo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool startingDateAdjustableDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", typeof(AdjustableDate),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        public AdjustableDate startingDateAdjustableDate;
        #endregion Members
    }
    #endregion StartingDate
    #region StubCalculationPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class StubCalculationPeriod : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialStubSpecified;
        [System.Xml.Serialization.XmlElementAttribute("initialStub", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial Stub")]
        public Stub initialStub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool finalStubSpecified;
        [System.Xml.Serialization.XmlElementAttribute("finalStub", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Final Stub")]
        public Stub finalStub;
        #endregion Members
    }
    #endregion StubCalculationPeriod

    #region Variance
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Variance : CalculationFromObservation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("varianceAmount", Order = 1)]
        public Money varianceAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike expressing as")]
        public EFS_RadioChoice strikeAs;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strikeAsVarianceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("varianceStrikePrice", typeof(EFS_Decimal), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Variance", IsVisible = true)]
        public EFS_Decimal strikeAsVariance;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strikeAsVolatilitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("volatilityStrikePrice", typeof(EFS_Decimal), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Manual Exercise", IsVisible = true)]
        public EFS_Decimal strikeAsVolatility;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool varianceCapSpecified;
        [System.Xml.Serialization.XmlElementAttribute("varianceCap", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Variance cap")]
        public EFS_Boolean varianceCap;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool unadjustedVarianceCapSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unadjustedVarianceCap", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Scaling factor of the variance cap")]
        public EFS_Decimal unadjustedVarianceCap;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool boundedVarianceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("boundedVariance", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Conditions which bound variance")]
        public BoundedVariance boundedVariance;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeTradedContractNearestSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedContractNearest", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exchange traded contract nearest")]
        public ExchangeTradedContract exchangeTradedContractNearest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool vegaNotionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("vegaNotionalAmount", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Vega notional amount")]
        public EFS_Decimal vegaNotionalAmount;
        #endregion Members
        #region Constructors
        public Variance()
        {
            strikeAsVariance = new EFS_Decimal();
            strikeAsVolatility = new EFS_Decimal();
        }
        #endregion Constructors
    }
    #endregion Variance
}

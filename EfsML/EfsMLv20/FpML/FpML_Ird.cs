#region Using Directives
using System;
using System.Reflection;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;

using EFS.ACommon;


using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;

using EfsML.v20;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
using FpML.v42.Shared;
#endregion Using Directives
#region Revision
/// <revision>
///     <version>1.2.0</version><date>20071003</date><author>EG</author>
///     <comment>
///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent for all method DisplayArray (used to determine REGEX type for derived classes
///     </comment>
/// </revision>
#endregion Revision

namespace FpML.v42.Ird
{
	#region BulletPayment
	/// <summary>
	/// <newpara><b>Description :</b> A product to represent a single cashflow.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type Product)</newpara>
	/// <newpara>• The base type which all FpML products extend.</newpara>
	/// <newpara>payment (exactly one occurrence; of the type Payment) A known payment between two parties.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Element: bulletPayment</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	[System.Xml.Serialization.XmlRootAttribute("bulletPayment", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
	public partial class BulletPayment : Product
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payment", Order = 1)]
		public Payment payment;
		#endregion Members
	}
	#endregion BulletPayment

	#region Calculation
	/// <summary>
	/// <newpara><b>Description :</b> A type definining the parameters used in the calculation of fixed or floating 
	/// calculation period amounts.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Either</newpara>
	/// <newpara>notionalSchedule (exactly one occurrence; of the type Notional) The notional amount or notional amount
	/// schedule.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>fxLinkedNotionalSchedule (exactly one occurrence; of the type FxLinkedNotionalSchedule) A notional
	/// amount schedule where each notional that applied to a calculation period is calculated with reference to a
	/// notional amount or notional amount schedule in a different currency by means of a spot currency exchange
	/// rate which is normally observed at the beginning of each period.</newpara>
	/// <newpara>Either</newpara>
	/// <newpara>fixedRateSchedule (exactly one occurrence; of the type Schedule) The fixed rate or fixed rate schedule
	/// expressed as explicit fixed rates and dates. In the case of a schedule, the step dates may be subject to
	/// adjustment in accordance with any adjustments specified in calculationPeriodDatesAdjustments.</newpara>
	/// <newpara>Or</newpara> 
	/// <newpara>floatingRateCalculation (exactly one occurrence; of the type FloatingRateCalculation) The floating rate
	/// calculation definitions</newpara>
	/// <newpara>dayCountFraction (exactly one occurrence; of the type DayCountFractionEnum) The day count fraction.</newpara>
	/// <newpara>discounting (zero or one occurrence; of the type Discounting) The parameters specifying any discounting
	/// conventions that may apply. This element must only be included if discounting applies.</newpara>
	/// <newpara>compoundingMethod (zero or one occurrence; of the type CompoundingMethodEnum) If more that one
	/// calculation period contributes to a single payment amount this element specifies whether compounding is
	/// applicable, and if so, what compounding method is to be used. This element must only be included when more
	/// that one calculation period contributes to a single payment amount.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: CalculationPeriodAmount</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	[System.Xml.Serialization.XmlRootAttribute("calculation", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
	public partial class Calculation : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional amount", IsCopyPaste = true)]
		public EFS_RadioChoice calculation;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool calculationNotionalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notionalSchedule", typeof(Notional), Order = 1)]
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
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Order = 5)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "dayCountFraction")]
		public DayCountFractionEnum dayCountFraction;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool discountingSpecified;
		[System.Xml.Serialization.XmlElementAttribute("discounting", Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "discounting")]
		public Discounting discounting;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool compoundingMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("compoundingMethod", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "compoundingMethod")]
		[ControlGUI(IsPrimary = false, Name = "value")]
		public CompoundingMethodEnum compoundingMethod;
		#endregion Members
	}
	#endregion Calculation
	#region CalculationPeriod
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the parameters used in the calculation of a 
	/// fixed or floating rate calculation period amount. This
	/// type forms part of cashflows representation of a swap stream.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>unadjustedStartDate (zero or one occurrence; of the type xsd:date)</newpara>
	/// <newpara>unadjustedEndDate (zero or one occurrence; of the type xsd:date)</newpara>
	/// <newpara>adjustedStartDate (zero or one occurrence; of the type xsd:date) The calculation period start date, adjusted
	/// according to any relevant business day convention.</newpara>
	/// <newpara>adjustedEndDate (zero or one occurrence; of the type xsd:date) The calculation period end date, adjusted
	/// according to any relevant business day convention.</newpara>
	/// <newpara>calculationPeriodNumberOfDays (zero or one occurrence; of the type xsd:positiveInteger) The number of
	/// days from the adjusted effective / start date to the adjusted termination / end date calculated in accordance
	/// with the applicable day count fraction.</newpara>
	/// <newpara>Either</newpara>
	/// <newpara>notionalAmount (exactly one occurrence; of the type xsd:decimal) The amount that a cashflow will accrue
	/// interest on.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>fxLinkedNotionalAmount (exactly one occurrence; of the type FxLinkedNotionalAmount) The amount that a
	/// cashflow will accrue interest on. This is the calculated amount of the fx linked - ie the other currency notional
	/// amount multiplied by the appropriate fx spot rate.</newpara>
	/// <newpara>Either</newpara>
	/// <newpara>floatingRateDefinition (exactly one occurrence; of the type FloatingRateDefinition) The floating rate reset
	/// information for the calculation period.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>fixedRate (exactly one occurrence; of the type xsd:decimal) The calculation period fixed rate. A per annum
	/// rate, expressed as a decimal. A fixed rate of 5% would be represented as 0.05.</newpara>
	/// </summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: PaymentCalculationPeriod</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class CalculationPeriod
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool unadjustedStartDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("unadjustedStartDate", Order = 1)]
		public EFS_Date unadjustedStartDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool unadjustedEndDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("unadjustedEndDate", Order = 2)]
		public EFS_Date unadjustedEndDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool adjustedStartDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustedStartDate", Order = 3)]
		public EFS_Date adjustedStartDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool adjustedEndDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustedEndDate", Order = 4)]
		public EFS_Date adjustedEndDate;
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodNumberOfDays", Order = 5)]
		public EFS_PosInteger calculationPeriodNumberOfDays;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional amount", IsCopyPaste = true)]
		public EFS_RadioChoice amount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool amountNotionalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notionalAmount", typeof(EFS_Decimal), Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Notional", IsVisible = true)]
		public EFS_Decimal amountNotional;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool amountfxLinkedNotionalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxLinkedNotionalAmount", typeof(FxLinkedNotionalAmount), Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Notional", IsVisible = true)]
		public FxLinkedNotionalAmount amountfxLinkedNotional;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate", IsCopyPaste = true)]
		public EFS_RadioChoice rate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool rateFloatingRateDefinitionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("floatingRateDefinition", typeof(FloatingRateDefinition), Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "FloatingRate", IsVisible = true)]
		public FloatingRateDefinition rateFloatingRateDefinition;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool rateFixedRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxLinkedNotionalAmount", typeof(EFS_Decimal), Order = 9)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "FixedRate", IsVisible = true)]
		public EFS_Decimal rateFixedRate;

		[System.Xml.Serialization.XmlElementAttribute("dayCountYearFraction", Order = 10)]
		public EFS_Decimal dayCountYearFraction;
		[System.Xml.Serialization.XmlElementAttribute("forecastAmount", Order = 11)]
		public Money forecastAmount;
		[System.Xml.Serialization.XmlElementAttribute("forecastRate", Order = 12)]
		public EFS_Decimal forecastRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
		#endregion Members
		#region Accessors
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
		#endregion Accessors
		#region Constructors
		public CalculationPeriod()
		{
			amountNotional = new EFS_Decimal();
			amountfxLinkedNotional = new FxLinkedNotionalAmount();
			rateFloatingRateDefinition = new FloatingRateDefinition();
			rateFixedRate = new EFS_Decimal();
		}
		#endregion Constructors
	}
	#endregion CalculationPeriod
	#region CalculationPeriodAmount
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the parameters used in the calculation of fixed or floating rate 
	/// calculation period amounts or for
	/// specifying a known calculation period amount or known amount schedule.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Either</newpara>
	/// <newpara>calculation (exactly one occurrence; of the type Calculation) The parameters used in the calculation of fixed
	/// or floaring rate calculation period amounts.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>knownAmountSchedule (exactly one occurrence; of the type AmountSchedule) The known calculation
	/// period amount or a known amount schedule expressed as explicit known amounts and dates. In the case of a
	/// schedule, the step dates may be subject to adjustment in accordance with any adjustments specified in
	/// calculationPeriodDatesAdjustments.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: InterestRateStream</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class CalculationPeriodAmount : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Period Amount")]
		public EFS_RadioChoice calculationPeriodAmount;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool calculationPeriodAmountCalculationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("calculation", typeof(Calculation), Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Calculation", IsVisible = true)]
		public Calculation calculationPeriodAmountCalculation;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool calculationPeriodAmountKnownAmountScheduleSpecified;
		[System.Xml.Serialization.XmlElementAttribute("knownAmountSchedule", typeof(KnownAmountSchedule), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Known Amount Schedule", IsVisible = true, IsCopyPaste = true)]
		public KnownAmountSchedule calculationPeriodAmountKnownAmountSchedule;
		#endregion Members
	}
	#endregion CalculationPeriodAmount
	#region CalculationPeriodDates
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	[System.Xml.Serialization.XmlRootAttribute("calculationPeriodDates", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
	public partial class CalculationPeriodDates : ItemGUI
	{
		#region Members
        // EG 20160404 Migration vs2013
        // #warning FpML4.2 en attente...
		/*
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[OpenDivGUI(Level=MethodsGUI.LevelEnum.End,Name="Effective Date",IsVisible=false)]
		[ControlGUI(Level=MethodsGUI.LevelEnum.End,IsDisplay=true,Name="Type")]
		public EFS_RadioChoice effectiveDate;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
		public bool effectiveDateAdjustableSpecified;
		[System.Xml.Serialization.XmlElementAttribute("effectiveDate", typeof(AdjustableDate))]
		[CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level=MethodsGUI.LevelEnum.HiddenKey,Name="Adjustable Date",IsVisible=true)]
		public AdjustableDate effectiveDateAdjustable;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
		public bool effectiveDateRelativeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("relativeEffectiveDate", typeof(RelativeDateOffset))]
		[CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level=MethodsGUI.LevelEnum.HiddenKey,Name="Relative Date",IsVisible=true)]
		public RelativeDateOffset effectiveDateRelative;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level=MethodsGUI.LevelEnum.End,Name="Effective Date")]
		[OpenDivGUI(Level=MethodsGUI.LevelEnum.End,Name="Termination Date",IsVisible=false)]
		[ControlGUI(Level=MethodsGUI.LevelEnum.End,IsDisplay=true,Name="Type Date")]
		public EFS_RadioChoice terminationDate;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
		public bool terminationDateAdjustableSpecified;
		[System.Xml.Serialization.XmlElementAttribute("terminationDate", typeof(AdjustableDate))]
		[CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level=MethodsGUI.LevelEnum.HiddenKey,Name="Adjustable Date",IsVisible=true)]
		public AdjustableDate terminationDateAdjustable;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
		public bool terminationDateRelativeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("relativeTerminationDate", typeof(RelativeDateOffset))]
		[CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level=MethodsGUI.LevelEnum.HiddenKey,Name="Relative Date",IsVisible=true)]
		public RelativeDateOffset terminationDateRelative;
		*/
		[System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date", IsVisible = false)]
		public AdjustableDate effectiveDate;
		[System.Xml.Serialization.XmlElementAttribute("terminationDate", Order = 2)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
		public AdjustableDate terminationDate;
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesAdjustments", Order = 3)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date")]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Dates Adjustments", IsVisible = false, IsCopyPaste = true)]
		public BusinessDayAdjustments calculationPeriodDatesAdjustments;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool firstPeriodStartDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("firstPeriodStartDate", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Dates Adjustments")]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Dates", IsVisible = false, IsGroup = true)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First period start date")]
		public AdjustableDate firstPeriodStartDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool firstRegularPeriodStartDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("firstRegularPeriodStartDate", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First regular period start date")]
		public EFS_Date firstRegularPeriodStartDate;
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool lastRegularPeriodEndDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("lastRegularPeriodEndDate", Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last regular period End Date")]
		public EFS_Date lastRegularPeriodEndDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stubPeriodTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("stubPeriodType", Order = 7)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Period Type")]
		[ControlGUI(IsPrimary = false, Name = "value")]
		public StubPeriodTypeEnum stubPeriodType;
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodFrequency", Order = 8)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Dates")]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Frequency", IsVisible = false)]
		public CalculationPeriodFrequency calculationPeriodFrequency;
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
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Frequency")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
		public EFS_Id efs_id;
		#endregion Members
	}
	#endregion CalculationPeriodDates
	#region CancelableProvision
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the right of a party to cancel a swap transaction on the specified exercise dates. 
	/// The provision is for 'walkaway' cancellation (i.e. the fair value of the swap is not paid). A fee payable on exercise can be
	/// specified.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>buyerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that buys
	/// this instrument, ie. pays for this instrument and receives the rights defined by it. See 2000 ISDA definitions
	/// Article 11.1 (b). In the case of FRAs this the fixed rate payer.</newpara>
	/// <newpara>sellerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that sells
	/// ("writes") this instrument, i.e. that grants the rights defined by this instrument and in return receives a payment
	/// for it. See 2000 ISDA definitions Article 11.1 (a). In the case of FRAs this is the floating rate payer.</newpara>
	/// <newpara>exercise (exactly one occurrence; of the type Exercise) An placeholder for the actual option exercise
	/// definitions.</newpara>
	/// <newpara>exerciseNotice (zero or one occurrence; of the type ExerciseNotice) Definition of the party to whom notice of
	/// exercise should be given.</newpara>
	/// <newpara>followUpConfirmation (exactly one occurrence; of the type xsd:boolean) A flag to indicate whether follow-up
	/// confirmation of exercise (written or electronic) is required following telephonic notice by the buyer to the seller
	/// or seller's agent.</newpara>
	/// <newpara>cancelableProvisionAdjustedDates (zero or one occurrence; of the type
	/// CancelableProvisionAdjustedDates) The adjusted dates associated with a cancelable provision. These dates
	/// have been adjusted for any applicable business day convention.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: Swap</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class CancelableProvision : Exercise
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
		[ControlGUI(Name = "Buyer")]
		public PartyReference buyerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
		[ControlGUI(Name = "Seller")]
		public PartyReference sellerPartyReference;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
		public EFS_RadioChoice cancelableExercise;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cancelableExerciseAmericanSpecified;
		[System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American Exercise", IsVisible = true)]
		public AmericanExercise cancelableExerciseAmerican;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cancelableExerciseBermudaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise), Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bermuda Exercise", IsVisible = true)]
		public BermudaExercise cancelableExerciseBermuda;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cancelableExerciseEuropeanSpecified;
		[System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise), Order = 5)]
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

		[System.Xml.Serialization.XmlElementAttribute("cancellationEvent", Order = 8)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cancelable Provision Adjusted Dates")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cancellation Event", IsClonable = true, IsChild = true)]
		public CancellationEvent[] cancelableProvisionAdjustedDates;
		#endregion Members
	}
	#endregion CancelableProvision
	#region CancellationEvent
	/// <summary>
	/// <newpara><b>Description :</b> The adjusted dates for a specific cancellation date, 
	/// including the adjusted exercise date and adjusted
	/// termination date.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>adjustedExerciseDate (exactly one occurrence; of the type xsd:date) The date on which option exercise
	/// takes place. This date should already be adjusted for any applicable business day convention.</newpara>
	/// <newpara>adjustedEarlyTerminationDate (exactly one occurrence; of the type xsd:date) The early termination date that
	/// is applicable if an early termination provision is exercised. This date should already be adjusted for any
	/// applicable business day convention.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: CancelableProvisionAdjustedDates</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class CancellationEvent : IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("adjustedExerciseDate", Order = 1)]
		[ControlGUI(Name = "adjusted Exercise Date")]
		public EFS_Date adjustedExerciseDate;
		[System.Xml.Serialization.XmlElementAttribute("adjustedEarlyTerminationDate", Order = 2)]
		[ControlGUI(Name = "adjusted Early Termination Date", LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_Date adjustedEarlyTerminationDate;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
		#endregion Members
		#region Accessors
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
	#endregion CancellationEvent
	#region CapFloor
	/// <summary>
	/// <newpara><b>Description :</b> A type defining an interest rate cap, floor, or cap/floor strategy (e.g. collar) product.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type Product)</newpara>
	/// <newpara>• The base type which all FpML products extend.</newpara>
	/// <newpara>capFloorStream (exactly one occurrence; of the type InterestRateStream)</newpara>
	/// <newpara>premium (zero or more occurrences; of the type Payment) The option premium amount payable by buyer to
	/// seller on the specified payment date.</newpara>
	/// <newpara>additionalPayment (zero or more occurrences; of the type Payment) Additional payments between the
	/// principal parties.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Element: capFloor</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	//[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	//[System.Xml.Serialization.XmlRootAttribute("capFloor", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
	[System.Xml.Serialization.XmlRootAttribute("swap", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
	public partial class CapFloor : Product
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("capFloorStream",Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "CapFloor Stream", IsVisible = false, IsCopyPaste = true)]
		public InterestRateStream capFloorStream;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool premiumSpecified;
		[System.Xml.Serialization.XmlElementAttribute("premium",Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cap Floor Stream")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Premium")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium", IsClonable = true, MinItem = 0)]
		[BookMarkGUI(IsVisible = false)]
		public Payment[] premium;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool additionalPaymentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("additionalPayment",Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Additional Payment")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Additional Payment", IsClonable = true, IsChild = true, MinItem = 0, IsCopyPasteItem = true)]
		[BookMarkGUI(IsVisible = false)]
		public Payment[] additionalPayment;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool earlyTerminationProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision",Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Early Termination Provision")]
		public EarlyTerminationProvision earlyTerminationProvision;
		/*
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool implicitEarlyTerminationProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("implicitEarlyTerminationProvision", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Implicit Early Termination Provision")]
		public Empty implicitEarlyTerminationProvision;
		*/
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool implicitProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("implicitProvision", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Implicit Provision")]
		public ImplicitProvision implicitProvision;
		#endregion Members
	}
	#endregion CapFloor
	#region Cashflows
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the cashflow representation of a swap trade.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>cashflowsMatchParameters (exactly one occurrence; of the type xsd:boolean) A true/false flag to indicate
	/// whether the cashflows match the parametric definition of the stream, i.e. whether the cashflows could be
	/// regenerated from the parameters without loss of information.</newpara>
	/// <newpara>principalExchange (zero or more occurrences; of the type PrincipalExchange) The initial, intermediate and
	/// final principal exchange amounts. Typically required on cross currency interest rate swaps where actual
	/// exchanges of principal occur. A list of principal exchange elements may be ordered in the document by
	/// ascending adjusted principal exchange date. An FpML document containing an unordered principal exchange
	/// list is still regarded as a conformant document.</newpara>
	/// <newpara>paymentCalculationPeriod (zero or more occurrences; of the type PaymentCalculationPeriod) The adjusted
	/// payment date and associated calculation period parameters required to calculate the actual or projected
	/// payment amount. A list of payment calculation period elements may be ordered in the document by ascending
	/// adjusted payment date. An FpML document containing an unordered list of payment calculation periods is still
	/// regarded as a conformant document.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: InterestRateStream</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class Cashflows
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("cashflowsMatchParameters", Order = 1)]
		public EFS_Boolean cashflowsMatchParameters;
		[System.Xml.Serialization.XmlElementAttribute("principalExchange", Order = 2)]
		public PrincipalExchange[] principalExchange;
		[System.Xml.Serialization.XmlElementAttribute("paymentCalculationPeriod", Order = 3)]
		public PaymentCalculationPeriod[] paymentCalculationPeriod;
		#endregion Members
	}
	#endregion Cashflows
	#region CashPriceMethod
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the parameters necessary for each of the ISDA cash price methods 
	/// for cash settlement.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>cashSettlementReferenceBanks (zero or one occurrence; of the type CashSettlementReferenceBanks) A
	/// container for a set of reference institutions. These reference institutions may be called upon to provide rate
	/// quotations as part of the method to determine the applicable cash settlement amount. If institutions are not
	/// specified, it is assumed that reference institutions will be agreed between the parties on the exercise date, or
	/// in the case of swap transaction to which mandatory early termination is applicable, the cash settlement
	/// valuation date.</newpara>
	/// <newpara>cashSettlementCurrency (exactly one occurrence; of the type Currency) The currency in which the cash
	/// settlement amount will be calculated and settled.</newpara>
	/// <newpara>quotationRateType (exactly one occurrence; of the type QuotationRateTypeEnum) Which rate quote is to be
	/// observed, either Bid, Mid, Offer or Exercising Party Pays. The meaning of Exercising Party Pays is defined in
	/// the 2000 ISDA Definitions, Section 17.2. Certain Definitions Relating to Cash Settlement, paragraph (j)</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: CashSettlement</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class CashPriceMethod : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cashSettlementReferenceBanksSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlementReferenceBanks", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Banks")]
		public CashSettlementReferenceBanks cashSettlementReferenceBanks;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlementCurrency", Order = 2)]
		[ControlGUI(Name = "currency", LblWidth = 115, LineFeed = MethodsGUI.LineFeedEnum.After)]
		public Currency cashSettlementCurrency;
		[System.Xml.Serialization.XmlElementAttribute("quotationRateType", Order = 3)]
		[ControlGUI(Name = "quotationRateType", LblWidth = 115)]
		public QuotationRateTypeEnum quotationRateType;
		#endregion Members
	}
	#endregion CashPriceMethod
	#region CashSettlement
	/// <summary>
	/// <newpara><b>Description :</b> A type to define the cash settlement terms for a product where cash settlement is applicable.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>cashSettlementValuationTime (exactly one occurrence; of the type BusinessCenterTime) The time of the
	/// cash settlement valuation date when the cash settlement amount will be determined according to the cash
	/// settlement method if the parties have not otherwise been able to agree the cash settlement amount.</newpara>
	/// <newpara>cashSettlementValuationDate (exactly one occurrence; of the type RelativeDateOffset) The date on which
	/// the cash settlement amount will be determined according to the cash settlement method if the parties have not
	/// otherwise been able to agree the cash settlement amount.</newpara>
	/// <newpara>cashSettlementPaymentDate (zero or one occurrence; of the type CashSettlementPaymentDate) The date
	/// on which the cash settlement amount will be paid, subject to adjustment in accordance with any applicable
	/// business day convention. This component would not be present for a mandatory early termination provision
	/// where the cash settlement payment date is the mandatory early termination date.</newpara>
	/// <newpara>Either</newpara>
	/// <newpara>cashPriceMethod (exactly one occurrence; of the type CashPriceMethod) An ISDA defined cash settlement
	/// method used for the determination of the applicable cash settlement amount. The method is defined in the
	/// 2000 ISDA Definitions, Section 17.3. Cash Settlement Methods, paragraph (a).</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>cashPriceAlternateMethod (exactly one occurrence; of the type CashPriceMethod) An ISDA defined cash
	/// settlement method used for the determination of the applicable cash settlement amount. The method is
	/// defined in the 2000 ISDA Definitions, Section 17.3. Cash Settlement Methods, paragraph (b).</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>parYieldCurveAdjustedMethod (exactly one occurrence; of the type YieldCurveMethod) An ISDA defined
	/// cash settlement method used for the determination of the applicable cash settlement amount. The method is
	/// defined in the 2000 ISDA Definitions, Section 17.3. Cash Settlement Methods, paragraph (c).</newpara>
	/// <newpara>Or</newpara> 
	/// <newpara>zeroCouponYieldAdjustedMethod (exactly one occurrence; of the type YieldCurveMethod) An ISDA defined
	/// cash settlement method used for the determination of the applicable cash settlement amount. The method is
	/// defined in the 2000 ISDA Definitions, Section 17.3. Cash Settlement Methods, paragraph (d).</newpara>
	/// <newpara>Or</newpara> 
	/// <newpara>parYieldCurveUnadjustedMethod (exactly one occurrence; of the type YieldCurveMethod) An ISDA defined
	/// cash settlement method used for the determination of the applicable cash settlement amount. The method is
	/// defined in the 2000 ISDA Definitions, Section 17.3. Cash Settlement Methods, paragraph (e).</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: MandatoryEarlyTermination</newpara>
	///<newpara>• Complex type: OptionalEarlyTermination</newpara>
	///<newpara>• Complex type: Swaption</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	[System.Xml.Serialization.XmlRootAttribute("cashSettlement", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
	public partial class CashSettlement : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("cashSettlementValuationTime", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Time", IsVisible = false)]
		public BusinessCenterTime cashSettlementValuationTime;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlementValuationDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Time")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Date", IsVisible = false, IsCopyPaste = true)]
		public RelativeDateOffset cashSettlementValuationDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cashSettlementPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlementPaymentDate", Order = 3)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Valuation Date")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Date")]
		public CashSettlementPaymentDate cashSettlementPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Method")]
		public EFS_DropDownChoice cashSettlementMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementMethodNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty cashSettlementMethodNone;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cashSettlementMethodcashPriceMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashPriceMethod", typeof(CashPriceMethod), Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "cashPriceMethod", IsVisible = true)]
		public CashPriceMethod cashSettlementMethodcashPriceMethod;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cashSettlementMethodcashPriceAlternateMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashPriceAlternateMethod", typeof(CashPriceMethod), Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "cashPriceAlternateMethod", IsVisible = true)]
		public CashPriceMethod cashSettlementMethodcashPriceAlternateMethod;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cashSettlementMethodparYieldCurveAdjustedMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("parYieldCurveAdjustedMethod", typeof(YieldCurveMethod), Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "parYieldCurveAdjustedMethod", IsVisible = true)]
		public YieldCurveMethod cashSettlementMethodparYieldCurveAdjustedMethod;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cashSettlementMethodzeroCouponYieldAdjustedMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("zeroCouponYieldAdjustedMethod", typeof(YieldCurveMethod), Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "zeroCouponYieldAdjustedMethod", IsVisible = true)]
		public YieldCurveMethod cashSettlementMethodzeroCouponYieldAdjustedMethod;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cashSettlementMethodparYieldCurveUnadjustedMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("parYieldCurveUnadjustedMethod", typeof(YieldCurveMethod), Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "parYieldCurveUnadjustedMethod", IsVisible = true)]
		public YieldCurveMethod cashSettlementMethodparYieldCurveUnadjustedMethod;

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
		#endregion Members
	}
	#endregion CashSettlement
	#region CashSettlementPaymentDate
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the cash settlement payment date(s) as either a set of explicit dates, 
	/// together with applicable adjustments, or as a date relative to some other (anchor) date, or as any date in a range 
	/// of contiguous business days.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Either</newpara>
	/// <newpara>adjustableDates (exactly one occurrence; of the type AdjustableDates) A series of dates that shall be subject
	/// to adjustment if they would otherwise fall on a day that is not a business day in the specified business centers,
	/// together with the convention for adjusting the date.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>relativeDate (exactly one occurrence; of the type RelativeDateOffset) A date specified as some offset to
	/// another date (the anchor date).</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>businessDateRange (exactly one occurrence; of the type BusinessDateRange) A range of contiguous
	/// business days.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: CashSettlement</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	[System.Xml.Serialization.XmlRootAttribute("cashSettlementPaymentDate", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
	public partial class CashSettlementPaymentDate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Payment Date")]
		public EFS_RadioChoice paymentDate;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentDateAdjustablesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustableDates", typeof(AdjustableDates), Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Dates", IsVisible = true)]
		public AdjustableDates paymentDateAdjustables;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentDateRelativeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("relativeDate", typeof(RelativeDateOffset), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date", IsVisible = true, IsCopyPaste = true)]
		public RelativeDateOffset paymentDateRelative;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentDateBusinessDateRangeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("businessDateRange", typeof(BusinessDateRange), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Business Date Range", IsVisible = true)]
		public BusinessDateRange paymentDateBusinessDateRange;

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
		#endregion Members
	}
	#endregion CashSettlementPaymentDate
	#region CashSettlementReferenceBanks
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the list of reference institutions polled for relevant rates or prices 
	/// when determining the cash settlement amount for a product where cash settlement is applicable.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>referenceBank (one or more occurrences; of the type ReferenceBank) An institution (party) identified by
	/// means of a coding scheme and an optional name.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: CashPriceMethod</newpara>
	///<newpara>• Complex type: SettlementRateSource</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class CashSettlementReferenceBanks
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("referenceBank", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Bank")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Bank", IsClonable = true)]
		public ReferenceBank[] referenceBank;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
		#endregion Members
		#region Accessors
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
		#endregion Accessors
	}
	#endregion CashSettlementReferenceBanks

	#region Discounting
	/// <summary>
	/// <newpara><b>Description :</b> A type defining discounting information. The 2000 ISDA definitions, 
	/// section 8.4. discounting (related to the calculation of a discounted fixed amount or floating amount) apply. 
	/// This type must only be included if discounting applies.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>discountingType (exactly one occurrence; of the type DiscountingTypeEnum) The discounting method that is
	/// applicable.</newpara>
	/// <newpara>discountRate (zero or one occurrence; of the type xsd:decimal) A discount rate, expressed as a decimal, to
	/// be used in the calculation of a discounted amount. A discount amount of 5% would be represented as 0.05.</newpara>
	/// <newpara>discountRateDayCountFraction (zero or one occurrence; of the type DayCountFractionEnum) A discount
	/// day count fraction to be used in the calculation of a discounted amount.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: Calculation</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
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

	#region EarlyTerminationEvent
	/// <summary>
	/// <newpara><b>Description :</b> A type to define the adjusted dates associated with an early termination provision.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>adjustedExerciseDate (exactly one occurrence; of the type xsd:date) The date on which option exercise
	/// takes place. This date should already be adjusted for any applicable business day convention.</newpara>
	/// <newpara>adjustedEarlyTerminationDate (exactly one occurrence; of the type xsd:date) The early termination date that
	/// is applicable if an early termination provision is exercised. This date should already be adjusted for any
	/// applicable business day convention.</newpara>
	/// <newpara>adjustedCashSettlementValuationDate (exactly one occurrence; of the type xsd:date) The date by which the
	/// cash settlement amount must be agreed. This date should already be adjusted for any applicable business
	/// day convention.</newpara>
	/// <newpara>adjustedCashSettlementPaymentDate (exactly one occurrence; of the type xsd:date) The date on which the
	/// cash settlement amount is paid. This date should already be adjusted for any applicable business dat
	/// convention.</newpara>
	/// <newpara>adjustedExerciseFeePaymentDate (zero or one occurrence; of the type xsd:date) The date on which the
	/// exercise fee amount is paid. This date should already be adjusted for any applicable business day convention.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: OptionalEarlyTerminationAdjustedDates</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class EarlyTerminationEvent : IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("adjustedExerciseDate", Order = 1)]
		[ControlGUI(Name = "Exercise Date")]
		public EFS_Date adjustedExerciseDate;
		[System.Xml.Serialization.XmlElementAttribute("adjustedEarlyTerminationDate", Order = 2)]
		[ControlGUI(Name = "Early Termination Date")]
		public EFS_Date adjustedEarlyTerminationDate;
		[System.Xml.Serialization.XmlElementAttribute("adjustedCashSettlementValuationDate", Order = 3)]
		[ControlGUI(Name = "Cash Settlement Valuation Date")]
		public EFS_Date adjustedCashSettlementValuationDate;
		[System.Xml.Serialization.XmlElementAttribute("adjustedCashSettlementPaymentDate", Order = 4)]
		[ControlGUI(Name = "Cash Settlement Payment Date")]
		public EFS_Date adjustedCashSettlementPaymentDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool adjustedExerciseFeePaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustedExerciseFeePaymentDate", Order = 5)]
		[ControlGUI(Name = "Exercise Fee Payment Date", LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_Date adjustedExerciseFeePaymentDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
		#endregion Members
		#region Accessors
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
	#endregion EarlyTerminationEvent
	#region EarlyTerminationProvision
	/// <summary>
	/// <newpara><b>Description :</b> A type defining an early termination provision for a swap. This early termination is at fair value, 
	/// i.e. on termination the fair value of the product must be settled between the parties.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Either</newpara>
	/// <newpara>mandatoryEarlyTermination (exactly one occurrence; of the type MandatoryEarlyTermination) A mandatory
	/// early termination provision to terminate the swap at fair value.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>optionalEarlyTermination (exactly one occurrence; of the type OptionalEarlyTermination) An option for either
	/// or both parties to terminate the swap at fair value.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: Swap</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class EarlyTerminationProvision : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool mandatoryEarlyTerminationDateTenorSpecified;
		[System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationDateTenor", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Mandatory Early Termination Date Tenor")]
		public Interval mandatoryEarlyTerminationDateTenor;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionalEarlyTerminationParametersSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionalEarlyTerminationParameters", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Optional Early Termination Parameters")]
		public ExercisePeriod optionalEarlyTerminationParameters;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Type")]
		public EFS_RadioChoice earlyTermination;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool earlyTerminationMandatorySpecified;
		[System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTermination", typeof(MandatoryEarlyTermination), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Mandatory Early Termination", IsVisible = true)]
		public MandatoryEarlyTermination earlyTerminationMandatory;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool earlyTerminationOptionalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionalEarlyTermination", typeof(OptionalEarlyTermination), Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Optional Early Termination", IsVisible = true)]
		public OptionalEarlyTermination earlyTerminationOptional;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
		#endregion Members
	}
	#endregion EarlyTerminationProvision
	#region ExerciseEvent
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the adjusted dates associated with a particular exercise event.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>adjustedExerciseDate (exactly one occurrence; of the type xsd:date) The date on which option exercise
	/// takes place. This date should already be adjusted for any applicable business day convention.</newpara>
	/// <newpara>adjustedRelevantSwapEffectiveDate (exactly one occurrence; of the type xsd:date) The effective date of the
	/// underlying swap associated with a given exercise date. This date should already be adjusted for any
	/// applicable business day convention.</newpara>
	/// <newpara>adjustedCashSettlementValuationDate (zero or one occurrence; of the type xsd:date) The date by which the
	/// cash settlement amount must be agreed. This date should already be adjusted for any applicable business
	/// day convention.</newpara>
	/// <newpara>adjustedCashSettlementPaymentDate (zero or one occurrence; of the type xsd:date) The date on which the
	/// cash settlement amount is paid. This date should already be adjusted for any applicable business dat
	/// convention.</newpara>
	/// <newpara>adjustedExerciseFeePaymentDate (zero or one occurrence; of the type xsd:date) The date on which the
	/// exercise fee amount is paid. This date should already be adjusted for any applicable business day convention.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: SwaptionAdjustedDates</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
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
	#region ExercisePeriod
	/// <summary>
	/// <newpara><b>Description :</b> This defines the time interval to the start of the exercise period, 
	/// i.e. the earliest exercise date, and the frequency of subsequent exercise dates (if any)..</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>earliestExerciseDateTenor (exactly one occurrence; of the type Interval. The time interval to the first 
	/// (and possibly only) exercise date in the exercise period.</newpara>
	/// <newpara>exerciseFrequency (zero or one occurrence; of the type Interval) The frequency of subsequent exercise 
	/// dates in the exercise period following the earliest exercise date. An interval of 1 day should be used 
	/// to indicate an American style exercise period.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EarlyTerminationProvision</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class ExercisePeriod
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("earliestExerciseDateTenor", Order = 1)]
		[ControlGUI(Name = "Earliest Exercise Date Tenor", LineFeed = MethodsGUI.LineFeedEnum.After)]
		public Interval earliestExerciseDateTenor;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool exerciseFrequencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseFrequency", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Name = "Exercise Frequency", LineFeed = MethodsGUI.LineFeedEnum.After)]
		public Interval exerciseFrequency;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
        #endregion Members
		#region Accessors
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
		#endregion Accessors
	}
	#endregion ExercisePeriod
	#region ExtendibleProvision
	/// <summary>
	/// <newpara><b>Description :</b> A type defining an option to extend an existing swap transaction on the specified exercise 
	/// dates for a term ending on the specified new termination date.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>buyerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that buys
	/// this instrument, ie. pays for this instrument and receives the rights defined by it. See 2000 ISDA definitions
	/// Article 11.1 (b). In the case of FRAs this the fixed rate payer.</newpara>
	/// <newpara>sellerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that sells
	/// ("writes") this instrument, i.e. that grants the rights defined by this instrument and in return receives a payment
	/// for it. See 2000 ISDA definitions Article 11.1 (a). In the case of FRAs this is the floating rate payer.</newpara>
	/// <newpara>exercise (exactly one occurrence; of the type Exercise) An placeholder for the actual option exercise
	/// definitions.</newpara>
	/// <newpara>exerciseNotice (zero or one occurrence; of the type ExerciseNotice) Definition of the party to whom notice of
	/// exercise should be given.</newpara>
	/// <newpara>followUpConfirmation (exactly one occurrence; of the type xsd:boolean) A flag to indicate whether follow-up
	/// confirmation of exercise (written or electronic) is required following telephonic notice by the buyer to the seller
	/// or seller's agent.</newpara>
	/// <newpara>extendibleProvisionAdjustedDates (zero or one occurrence; of the type ExtendibleProvisionAdjustedDates)
	/// The adjusted dates associated with an extendible provision. These dates have been adjusted for any
	/// applicable business day convention.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: Swap</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class ExtendibleProvision : Exercise
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
		[ControlGUI(Name = "Buyer")]
		public PartyReference buyerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
		[ControlGUI(Name = "Seller")]
		public PartyReference sellerPartyReference;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
		public EFS_RadioChoice extendibleExercise;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool extendibleExerciseAmericanSpecified;
		[System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American Exercise", IsVisible = true)]
		public AmericanExercise extendibleExerciseAmerican;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool extendibleExerciseBermudaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise), Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bermuda Exercise", IsVisible = true)]
		public BermudaExercise extendibleExerciseBermuda;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool extendibleExerciseEuropeanSpecified;
		[System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise), Order = 5)]
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

		[System.Xml.Serialization.XmlElementAttribute("extensionEvent", Order = 8)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Extendible Provision Adjusted Dates")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extension Event", IsClonable = true, IsChild = true)]
		public ExtensionEvent[] extendibleProvisionAdjustedDates;
		#endregion Members
	}
	#endregion ExtendibleProvision
	#region ExtensionEvent
	/// <summary>
	/// <newpara><b>Description :</b> A type to define the adjusted dates associated with an individual extension event.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>adjustedExerciseDate (exactly one occurrence; of the type xsd:date) The date on which option exercise
	/// takes place. This date should already be adjusted for any applicable business day convention.</newpara>
	/// <newpara>adjustedExtendedTerminationDate (exactly one occurrence; of the type xsd:date) The termination date if an
	/// extendible provision is exercised. This date should already be adjusted for any applicable business day
	/// convention.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: ExtendibleProvisionAdjustedDates</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
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

	#region FloatingRateDefinition
	/// <summary>
	/// <newpara><b>Description :</b> A type defining parameters associated with a floating rate reset. This type forms part of the cashflows
	/// representation of a stream.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>calculatedRate (zero or one occurrence; of the type xsd:decimal) The final calculated rate for a calculation
	/// period after any required averaging of rates A calculated rate of 5% would be represented as 0.05.</newpara>
	/// <newpara>rateObservation (zero or more occurrences; of the type RateObservation) The details of a particular rate
	/// observation, including the fixing date and observed rate. A list of rate observation elements may be ordered in
	/// the document by ascending adjusted fixing date. An FpML document containing an unordered list of rate
	/// observations is still regarded as a conformant document.</newpara>
	/// <newpara>floatingRateMultiplier (zero or one occurrence; of the type xsd:decimal) A rate multiplier to apply to the
	/// floating rate. The multiplier can be a positive or negative decimal. This element should only be included if the
	/// multiplier is not equal to 1 (one).</newpara>
	/// <newpara>spread (zero or one occurrence; of the type xsd:decimal) The ISDA Spread, if any, which applies for the
	/// calculation period. The spread is a per annum rate, expressed as a decimal. For purposes of determining a
	/// calculation period amount, if positive the spread will be added to the floating rate and if negative the spread
	/// will be subtracted from the floating rate. A positive 10 basis point (0.1%) spread would be represented as
	/// 0.001.</newpara>
	/// <newpara>capRate (zero or more occurrences; of the type Strike) The cap rate, if any, which applies to the floating rate
	/// for the calculation period. The cap rate (strike) is only required where the floating rate on a swap stream is
	/// capped at a certain strike level. The cap rate is assumed to be exclusive of any spread and is a per annum
	/// rate, expressed as a decimal. A cap rate of 5% would be represented as 0.05.</newpara>
	/// <newpara>floorRate (zero or more occurrences; of the type Strike) The floor rate, if any, which applies to the floating rate
	/// for the calculation period. The floor rate (strike) is only required where the floating rate on a swap stream is
	/// floored at a certain strike level. The floor rate is assumed to be exclusive of any spread and is a per annum
	/// rate, expressed as a decimal. The floor rate of 5% would be represented as 0.05.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: CalculationPeriod</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class FloatingRateDefinition
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool calculatedRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("calculatedRate", Order = 1)]
		public EFS_Decimal calculatedRate;
		[System.Xml.Serialization.XmlElementAttribute("rateObservation", Order = 2)]
		public RateObservation[] rateObservation;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool floatingRateMultiplierSpecified;
		[System.Xml.Serialization.XmlElementAttribute("floatingRateMultiplier", Order = 3)]
		public EFS_Decimal floatingRateMultiplier;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool spreadSpecified;
		[System.Xml.Serialization.XmlElementAttribute("spread", Order = 4)]
		public EFS_Decimal spread;
		[System.Xml.Serialization.XmlElementAttribute("capRate", Order = 5)]
		public Strike[] capRate;
		[System.Xml.Serialization.XmlElementAttribute("floorRate", Order = 6)]
		public Strike[] floorRate;
		#endregion Members
	}
	#endregion FloatingRateDefinition
	#region Fra
	/// <summary>
	/// <newpara><b>Description :</b> A type defining a Forward Rate Agreement (FRA) product.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type Product)</newpara>
	/// <newpara>• The base type which all FpML products extend.</newpara>
	/// <newpara>buyerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that buys
	/// this instrument, ie. pays for this instrument and receives the rights defined by it. See 2000 ISDA definitions
	/// Article 11.1 (b). In the case of FRAs this the fixed rate payer.</newpara>
	/// <newpara>sellerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that sells
	/// ("writes") this instrument, i.e. that grants the rights defined by this instrument and in return receives a payment
	/// for it. See 2000 ISDA definitions Article 11.1 (a). In the case of FRAs this is the floating rate payer.</newpara>
	/// <newpara>adjustedEffectiveDate (exactly one occurrence; with locally defined content) The start date of the calculation
	/// period. This date should already be adjusted for any applicable business day convention. This is also the date
	/// when the observed rate is applied, the reset date.</newpara>
	/// <newpara>adjustedTerminationDate (exactly one occurrence; of the type xsd:date) The end date of the calculation
	/// period. This date should already be adjusted for any applicable business day convention.</newpara>
	/// <newpara>paymentDate (exactly one occurrence; of the type AdjustableDate) The payment date. This date is subject to
	/// adjustment in accordance with any applicable business day convention.</newpara>
	/// <newpara>fixingDateOffset (exactly one occurrence; of the type RelativeDateOffset) Specifies the fixing date relative to
	/// the reset date in terms of a business days offset and an associated set of financial business centers. Normally
	/// these offset calculation rules will be those specified in the ISDA definition for the relevant floating rate index
	/// (ISDA's Floating Rate Option). However, non-standard offset calculation rules may apply for a trade if mutually
	/// agreed by the principal parties to the transaction. The href attribute on the dateRelativeTo element should
	/// reference the id attribute on the adjustedEffectiveDate element.</newpara>
	/// <newpara>dayCountFraction (exactly one occurrence; of the type DayCountFractionEnum) The day count fraction.</newpara>
	/// <newpara>calculationPeriodNumberOfDays (exactly one occurrence; of the type xsd:positiveInteger) The number of
	/// days from the adjusted effective date to the adjusted termination date calculated in accordance with the
	/// applicable day count fraction.</newpara>
	/// <newpara>notional (exactly one occurrence; of the type Money) The notional amount.</newpara>
	/// <newpara>fixedRate (exactly one occurrence; of the type xsd:decimal) The calculation period fixed rate. A per annum
	/// rate, expressed as a decimal. A fixed rate of 5% would be represented as 0.05.</newpara>
	/// <newpara>floatingRateIndex (exactly one occurrence; of the type FloatingRateIndex)</newpara>
	/// <newpara>indexTenor (zero or one occurrence; of the type Interval) The ISDA Designated Maturity, i.e. the tenor of the
	/// floating rate.</newpara>
	/// <newpara>fraDiscounting (exactly one occurrence; of the type FraDiscountingEnum) Specifies whether discounting
	/// applies and, if so, what type.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Element: fra</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	[System.Xml.Serialization.XmlRootAttribute("fra", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
	public partial class Fra : Product
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
		[ControlGUI(Name = "Buyer", LblWidth = 120, LineFeed = MethodsGUI.LineFeedEnum.Before)]
		public PartyReference buyerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
		[ControlGUI(Name = "Seller")]
		public PartyReference sellerPartyReference;
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
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool indexTenorSpecified;
		[System.Xml.Serialization.XmlElementAttribute("indexTenor", Order = 12)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index tenor")]
        public Interval[] indexTenor;
		[System.Xml.Serialization.XmlElementAttribute("fraDiscounting", Order = 13)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate")]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Discounting")]
		public FraDiscountingEnum fraDiscounting;
		#endregion Members
	}
	#endregion Fra
	#region FxLinkedNotionalAmount
	/// <summary>
	/// <newpara><b>Description :</b> A type to describe the cashflow representation for fx linked notionals.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>resetDate (zero or one occurrence; of the type xsd:date)</newpara>
	/// <newpara>adjustedFxSpotFixingDate (zero or one occurrence; of the type xsd:date) The date on which the fx spot rate
	/// is observed. This date should already be adjusted for any applicable business day convention.</newpara>
	/// <newpara>observedFxSpotRate (zero or one occurrence; of the type xsd:decimal) The actual observed fx spot rate.</newpara>
	/// <newpara>notionalAmount (zero or one occurrence; of the type xsd:decimal) The calculation period notional amount.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: CalculationPeriod</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class FxLinkedNotionalAmount
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool resetDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("resetDate", Order = 1)]
		public EFS_Date resetDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool adjustedFxSpotFixingDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustedFxSpotFixingDate", Order = 2)]
		public EFS_Date adjustedFxSpotFixingDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool observedFxSpotRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("observedFxSpotRate", Order = 3)]
		public EFS_Decimal observedFxSpotRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool notionalAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notionalAmount", Order = 4)]
		public EFS_Decimal notionalAmount;
		#endregion Members
	}
	#endregion FxLinkedNotionalAmount
	#region FxLinkedNotionalSchedule
	/// <summary>
	/// <newpara><b>Description :</b> A type to describe a notional schedule where each notional that applies to a calculation 
	/// period is calculated with reference to a notional amount or notional amount schedule in a different currency by means 
	/// of a spot currency exchange rate which is normally observed at the beginning of each period.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>constantNotionalScheduleReference (exactly one occurrence; of the type NotionalReference) A pointer
	/// style reference to the associated constant notional schedule defined elsewhere in the document which
	/// contains the currency amounts which will be converted into the varying notional currency amounts using the
	/// spot currency exchange rate.</newpara>
	/// <newpara>initialValue (zero or one occurrence; of the type xsd:decimal)</newpara>
	/// <newpara>varyingNotionalCurrency (exactly one occurrence; of the type xsd:string) The currency of the varying
	/// notional amount, i.e. the notional amount being determined periodically based on observation of a spot
	/// currency exchange rate.</newpara>
	/// <newpara>varyingNotionalFixingDates (exactly one occurrence; of the type RelativeDateOffset) The dates on which
	/// spot currency exchange rates are observed for purposes of determining the varying notional currency amount
	/// that will apply to a calculation period.</newpara>
	/// <newpara>fxSpotRateSource (exactly one occurrence; of the type FxSpotRateSource) The information source and time
	/// at which the spot currency exchange rate will be observed.</newpara>
	/// <newpara>varyingNotionalInterimExchangePaymentDates (exactly one occurrence; of the type RelativeDateOffset)
	/// The dates on which interim exchanges of notional are paid. Interim exchanges will arise as a result of changes
	/// in the spot currency exchange amount or changes in the constant notional schedule (e.g. amortization).</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: Calculation</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class FxLinkedNotionalSchedule : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("constantNotionalScheduleReference", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "constantNotionalScheduleReference", Width = 170)]
		public NotionalReference constantNotionalScheduleReference;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool initialValueSpecified;
		[System.Xml.Serialization.XmlElementAttribute("initialValue", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial Value", Width = 170)]
		public EFS_Decimal initialValue;
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "currency", Width = 75)]
		[System.Xml.Serialization.XmlElementAttribute("varyingNotionalCurrency", Order = 3)]
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

	#region InterestRateStream
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the components specifiying an interest rate stream, 
	/// including both a parametric and cashflow representation for the stream of payments.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>payerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party
	/// responsible for making the payments defined by this structure.</newpara>
	/// <newpara>receiverPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that
	/// receives the payments corresponding to this structure.</newpara>
	/// <newpara>calculationPeriodDates (exactly one occurrence; of the type CalculationPeriodDates) The calculation periods
	/// dates schedule.</newpara>
	/// <newpara>paymentDates (exactly one occurrence; of the type PaymentDates) The payment dates schedule.</newpara>
	/// <newpara>resetDates (zero or one occurrence; of the type ResetDates) The reset dates schedule. The reset dates
	/// schedule only applies for a floating rate stream.</newpara>
	/// <newpara>calculationPeriodAmount (exactly one occurrence; of the type CalculationPeriodAmount) The calculation
	/// period amount parameters.</newpara>
	/// <newpara>stubCalculationPeriodAmount (zero or one occurrence; of the type StubCalculationPeriodAmount) The stub
	/// calculation period amount parameters. This element must only be included if there is an initial or final stub
	/// calculation period. Even then, it must only be included if either the stub references a different floating rate
	/// tenor to the regular calculation periods, or if the stub is calculated as a linear interpolation of two different
	/// floating rate tenors, or if a specific stub rate or stub amount has been negotiated.</newpara>
	/// <newpara>principalExchanges (zero or one occurrence; of the type PrincipalExchanges) The true/false flags indicating
	/// whether initial, intermediate or final exchanges of principal should occur.</newpara>
	/// <newpara>cashflows (zero or one occurrence; of the type Cashflows) The cashflows representation of the swap stream.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: CapFloor</newpara>
	///<newpara>• Complex type: Swap</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	//[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.efs.org/2005/EFSmL-2-0")]
	public partial class InterestRateStream : ItemGUI
	{
		#region Members
		[ControlGUI(Name = "Payer")]
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 1)]//
		public PartyReference payerPartyReference;
		[ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]//
		public PartyReference receiverPartyReference;
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Period Dates", IsVisible = false, IsCopyPaste = true)]
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodDates", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 3)]//
		public CalculationPeriodDates calculationPeriodDates;
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Period Dates")]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Dates", IsVisible = false, IsCopyPaste = true)]
		[System.Xml.Serialization.XmlElementAttribute("paymentDates", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 4)]//
		public PaymentDates paymentDates;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool resetDatesSpecified;
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment Dates")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Reset Dates", IsCopyPaste = true)]
		[System.Xml.Serialization.XmlElementAttribute("resetDates", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 5)]//
		public ResetDates resetDates;
		[ControlGUI(IsLabel = false, Name = "Calculation Period Amount", IsCopyPaste = true)]
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 6)]//
		public CalculationPeriodAmount calculationPeriodAmount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stubCalculationPeriodAmountSpecified;
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Stub Calculation Period amounts", IsCopyPaste = true)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[System.Xml.Serialization.XmlElementAttribute("stubCalculationPeriodAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 7)]//
		public StubCalculationPeriodAmount stubCalculationPeriodAmount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool principalExchangesSpecified;
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Principal Exchanges")]
		[System.Xml.Serialization.XmlElementAttribute("principalExchanges", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 8)]//			
		public PrincipalExchanges principalExchanges;
		[System.Xml.Serialization.XmlElementAttribute("cashflows", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 9)]//			
		public Cashflows cashflows;
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
		public EFS_Id efs_id;
		#endregion Members
		#region Members
		/*
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
        public bool streamExtensionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("streamExtension",Namespace="http://www.efs.org/2005/EFSmL-2-0",Order=10)]
        [CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level=MethodsGUI.LevelEnum.Intermediary,Name="Extensions...")]
        public StreamExtension streamExtension;
		*/
		#endregion Members
	}
	#endregion InterestRateStream

	#region MandatoryEarlyTermination
	/// <summary>
	/// <newpara><b>Description :</b> A type to define an early termination provision for which exercise is mandatory.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>mandatoryEarlyTerminationDate (exactly one occurrence; of the type AdjustableDate) The early termination
	/// date associated with a mandatory early termination of a swap.</newpara>
	/// <newpara>calculationAgentPartyReference (one or more occurrences; of the type PartyReference) A pointer style
	/// reference to a party identifier defined elsewhere in the document. The party referenced is the ISDA Calculation
	/// Agent for the trade. If more than one party is referenced then the parties are assumed to be co-calculation
	/// agents, i.e. they have joint reponsibility.</newpara>
	/// <newpara>cashSettlement (exactly one occurrence; of the type CashSettlement) If specified, this means that cash
	/// settlement is applicable to the transaction and defines the parameters associated with the cash settlement
	/// procedure. If not specified, then physical settlement is applicable.</newpara>
	/// <newpara>mandatoryEarlyTerminationAdjustedDates (zero or one occurrence; of the type
	/// MandatoryEarlyTerminationAdjustedDates) The adjusted dates associated with a mandatory early termination
	/// provision. These dates have been adjusted for any applicable business day convention.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EarlyTerminationProvision</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class MandatoryEarlyTermination
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationDate", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date", IsVisible = false)]
		public AdjustableDate mandatoryEarlyTerminationDate;
		[System.Xml.Serialization.XmlElementAttribute("calculationAgent", Order = 2)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Termination Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Agent", IsVisible = false)]
		public CalculationAgent calculationAgent;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlement", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Agent")]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Settlement", IsVisible = false)]
		public CashSettlement cashSettlement;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool mandatoryEarlyTerminationAdjustedDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationAdjustedDates", Order = 4)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash Settlement")]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted Dates")]
		public MandatoryEarlyTerminationAdjustedDates mandatoryEarlyTerminationAdjustedDates;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
		#endregion Members
	}
	#endregion MandatoryEarlyTermination
	#region MandatoryEarlyTerminationAdjustedDates
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the adjusted dates associated with a mandatory early termination provision.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>adjustedEarlyTerminationDate (exactly one occurrence; of the type xsd:date) The early termination date that
	/// is applicable if an early termination provision is exercised. This date should already be adjusted for any
	/// applicable business day convention.</newpara>
	/// <newpara>adjustedCashSettlementValuationDate (exactly one occurrence; of the type xsd:date) The date by which the
	/// cash settlement amount must be agreed. This date should already be adjusted for any applicable business
	/// day convention.</newpara>
	/// <newpara>adjustedCashSettlementPaymentDate (exactly one occurrence; of the type xsd:date) The date on which the
	/// cash settlement amount is paid. This date should already be adjusted for any applicable business dat
	/// convention.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: MandatoryEarlyTermination</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class MandatoryEarlyTerminationAdjustedDates
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("adjustedEarlyTerminationDate", Order = 1)]
		[ControlGUI(Name = "EarlyTerminationDate")]
		public EFS_Date adjustedEarlyTerminationDate;
		[System.Xml.Serialization.XmlElementAttribute("adjustedCashSettlementValuationDate", Order = 2)]
		[ControlGUI(Name = "CashSettlementValuationDate")]
		public EFS_Date adjustedCashSettlementValuationDate;
		[System.Xml.Serialization.XmlElementAttribute("adjustedCashSettlementPaymentDate", Order = 3)]
		[ControlGUI(Name = "CashSettlementPaymentDate")]
		public EFS_Date adjustedCashSettlementPaymentDate;
		#endregion Members
	}
	#endregion MandatoryEarlyTerminationAdjustedDates

	#region Notional
	/// <summary>
	/// <newpara><b>Description :</b> An type defining the notional amount or notional amount schedule associated with a swap stream. The
	/// notional schedule will be captured explicitly, specifying the dates that the notional changes and the
	/// outstanding notional amount that applies from that date. A parametric representation of the rules defining the
	/// notional step schedule can optionally be included.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>notionalStepSchedule (exactly one occurrence; of the type AmountSchedule) The notional amount or
	/// notional amount schedule expressed as explicit outstanding notional amounts and dates. In the case of a
	/// schedule, the step dates may be subject to adjustment in accordance with any adjustments specified in
	/// calculationPeriodDatesAdjustments.</newpara>
	/// <newpara>notionalStepParameters (zero or one occurrence; of the type NotionalStepRule) A parametric representation
	/// of the notional step schedule, i.e. parameters used to generate the notional schedule.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: Calculation</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class Notional : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("notionalStepSchedule", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional Step", IsVisible = true)]
		[ControlGUI(IsLabel = false, Name = "Notional Step", IsCopyPaste = true)]
		public AmountSchedule notionalStepSchedule;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool notionalStepParametersSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notionalStepParameters", Order = 2)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "notionalStepSchedule")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "NotionalStepParameters", IsCopyPaste = true, LineFeed = MethodsGUI.LineFeedEnum.After)]
		public NotionalStepRule notionalStepParameters;
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
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentQuote)]
		public EFS_Id efs_id;
		#endregion Members
	}
	#endregion Notional
	#region NotionalStepRule
	/// <summary>
	/// <newpara><b>Description :</b> A type defining a parametric representation of the notional step schedule, i.e. parameters 
	/// used to generate the notional balance on each step date. The step change in notional can be expressed in terms of 
	/// either a fixed amount or as a percentage of either the initial notional or previous notional amount. 
	/// This parametric representation is intended to cover the more common amortizing/accreting.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>calculationPeriodDatesReference (exactly one occurrence; of the type DateReference) A pointer style
	/// reference to the associated calculation period dates component defined elsewhere in the document.</newpara>
	/// <newpara>stepFrequency (exactly one occurrence; of the type Interval) The frequency at which the step changes occur.
	/// This frequency must be a multiple of the stream calculation period frequency.</newpara>
	/// <newpara>firstNotionalStepDate (exactly one occurrence; of the type xsd:date) The unadjusted calculation period start
	/// date of the first change in notional. This day may be subject to adjustment in accordance with any adjustments
	/// specified in calculationPeriodDatesAdjustments.</newpara>
	/// <newpara>lastNotionalStepDate (exactly one occurrence; of the type xsd:date) The unadjusted calculation period end
	/// date of the last change in notional. This day may be subject to adjustment in accordance with any adjustments
	/// specified in calculationPeriodDatesAdjustments.</newpara>
	/// <newpara>Either</newpara>
	/// <newpara>notionalStepAmount (exactly one occurrence; of the type xsd:decimal) The explicit amount that the notional
	/// changes on each step date. This can be a positive or negative amount.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: Notional</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	[System.Xml.Serialization.XmlRootAttribute("notionalStepParameters", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
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
		[System.Xml.Serialization.XmlElementAttribute("notionalStepAmount", typeof(EFS_Decimal), Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount", IsVisible = true)]
		[ControlGUI(IsPrimary = false, Name = "value", Regex = EFSRegex.TypeRegex.RegexAmountExtend)]
		public EFS_Decimal notionalStepAmount;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool notionalStepRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notionalStepRate", typeof(EFS_Decimal), Order = 6)]
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

	#region OptionalEarlyTermination
	/// <summary>
	/// <newpara><b>Description :</b> </newpara>
	/// <newpara><b>Contents :</b> A type defining an early termination provision where either or both parties have 
	/// the right to exercise.</newpara>
	/// <newpara>singlePartyOption (zero or one occurrence; of the type SinglePartyOption) If optional early termination is not
	/// available to both parties then this component specifies the buyer and seller of the option.</newpara>
	/// <newpara>exercise (exactly one occurrence; of the type Exercise) An placeholder for the actual option exercise
	/// definitions.</newpara>
	/// <newpara>exerciseNotice (zero or more occurrences; of the type ExerciseNotice) Definition of the party to whom notice
	/// of exercise should be given.</newpara>
	/// <newpara>followUpConfirmation (exactly one occurrence; of the type xsd:boolean) A flag to indicate whether follow-up
	/// confirmation of exercise (written or electronic) is required following telephonic notice by the buyer to the seller
	/// or seller's agent.</newpara>
	/// <newpara>calculationAgent (exactly one occurrence; of the type CalculationAgent) The ISDA Calculation Agent
	/// responsible for performing duties associated with an optional early termination.</newpara>
	/// <newpara>cashSettlement (exactly one occurrence; of the type CashSettlement) If specified, this means that cash
	/// settlement is applicable to the transaction and defines the parameters associated with the cash settlement
	/// procedure. If not specified, then physical settlement is applicable.</newpara>
	/// <newpara>optionalEarlyTerminationAdjustedDates (zero or one occurrence; of the type
	/// OptionalEarlyTerminationAdjustedDates) An early termination provision to terminate the trade at fair value
	/// where one or both parties have the right to decide on termination.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EarlyTerminationProvision</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class OptionalEarlyTermination : Exercise
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool singlePartyOptionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("singlePartyOption",Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Party Option")]
		public SinglePartyOption singlePartyOption;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
		public EFS_RadioChoice optionalEarlyTerminationExercise;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionalEarlyTerminationExerciseAmericanSpecified;
		[System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise),Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American Exercise", IsVisible = true)]
		public AmericanExercise optionalEarlyTerminationExerciseAmerican;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionalEarlyTerminationExerciseBermudaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise),Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bermuda Exercise", IsVisible = true)]
		public BermudaExercise optionalEarlyTerminationExerciseBermuda;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionalEarlyTerminationExerciseEuropeanSpecified;
		[System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise),Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European Exercise", IsVisible = true)]
		public EuropeanExercise optionalEarlyTerminationExerciseEuropean;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool exerciseNoticeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseNotice",Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Notice")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Notice", IsClonable = true, IsChild = true)]
		public ExerciseNotice[] exerciseNotice;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool followUpConfirmationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("followUpConfirmation",Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "followUpConfirmation")]
		public EFS_Boolean followUpConfirmation;
		[System.Xml.Serialization.XmlElementAttribute("calculationAgent",Order = 7)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Agent", IsVisible = false)]
		public CalculationAgent calculationAgent;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlement",Order = 8)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Calculation Agent")]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash Settlement", IsVisible = false)]
		public CashSettlement cashSettlement;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionalEarlyTerminationAdjustedDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionalEarlyTerminationAdjustedDates",Order = 9)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash Settlement")]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Optional Early Termination Adjusted Dates")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Early Termination Event", IsClonable = true, IsChild = true)]
		public EarlyTerminationEvent[] optionalEarlyTerminationAdjustedDates;
		#endregion Members
	}
	#endregion OptionalEarlyTermination

	#region PaymentCalculationPeriod
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the adjusted payment date and associated calculation period parameters 
	/// required to calculate the actual or projected payment amount. 
	/// This type forms part of the cashflow representation of a swap stream.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>unadjustedPaymentDate (zero or one occurrence; of the type xsd:date)</newpara>
	/// <newpara>adjustedPaymentDate (zero or one occurrence; of the type xsd:date) The adjusted payment date. This date
	/// should already be adjusted for any applicable business day convention. This component is not intended for
	/// use in trade confirmation but my be specified to allow the fee structure to also serve as a cashflow type
	/// component (all dates the the Cashflows type are adjusted payment dates).</newpara>
	/// <newpara>Either</newpara>
	/// <newpara>calculationPeriod (one or more occurrences; of the type CalculationPeriod) The parameters used in the
	/// calculation of a fixed or floating rate calculation period amount. A list of calculation period elements may be
	/// ordered in the document by ascending start date. An FpML document which contains an unordered list of
	/// calcularion periods is still regarded as a conformant document.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>fixedPaymentAmount (exactly one occurrence; of the type xsd:decimal) A known fixed payment amount.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: Cashflows</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class PaymentCalculationPeriod : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool unadjustedPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("unadjustedPaymentDate", Order = 1)]
		public EFS_Date unadjustedPaymentDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool adjustedPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", Order = 2)]
		public EFS_Date adjustedPaymentDate;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Period parameters")]
		public EFS_RadioChoice period;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool periodCalculationPeriodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriod", typeof(CalculationPeriod), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public CalculationPeriod periodCalculationPeriod;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool periodFixedPaymentAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fixedPaymentAmount", typeof(EFS_Decimal), Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public EFS_Decimal periodFixedPaymentAmount;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool discountFactorSpecified;
		[System.Xml.Serialization.XmlElementAttribute("discountFactor", Order = 5)]
		public EFS_Decimal discountFactor;
		[System.Xml.Serialization.XmlElementAttribute("forecastPaymentAmount", Order = 6)]
		public Money forecastPaymentAmount;
		[System.Xml.Serialization.XmlElementAttribute("presentValueAmount", Order = 7)]
		public Money presentValueAmount;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(IsPrimary = false, Name = "value", Width = 200)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
		public EFS_Href efs_href;
		#endregion Members
		#region Accessors
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
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
		public string href
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
		#region Constructors
		public PaymentCalculationPeriod()
		{
			periodCalculationPeriod = new CalculationPeriod();
			periodFixedPaymentAmount = new EFS_Decimal();
		}
		#endregion Constructors
	}
	#endregion PaymentCalculationPeriod
	#region PaymentDates
	/// <summary>
	/// <newpara><b>Description :</b> A type defining parameters used to generate the payment dates schedule, including the 
	/// specification of early
	/// or delayed payments. Payment dates are determined relative to the calculation period dates or the reset dates.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Either</newpara>
	/// <newpara>calculationPeriodDatesReference (exactly one occurrence; of the type DateReference) A pointer style
	/// reference to the associated calculation period dates component defined elsewhere in the document.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>resetDatesReference (exactly one occurrence; of the type DateReference) A pointer style reference to the
	/// associated reset dates component defined elsewhere in the document.</newpara>
	/// <newpara>paymentFrequency (exactly one occurrence; of the type Interval) The frequency at which regular payment
	/// dates occur. If the payment frequency is equal to the frequency defined in the calculation period dates
	/// component then one calculation period contributes to each payment amount. If the payment frequency is less
	/// frequent than the frequency defined in the calculation period dates component then more than one calculation
	/// period will contribute to e payment amount. A payment frequency more frequent than the calculation period
	/// frequency or one that is not a multiple of the calculation period frequency is invalid.</newpara>
	/// <newpara>firstPaymentDate (zero or one occurrence; of the type xsd:date) The first unadjusted payment date. This day
	/// may be subject to adjustment in accordance with any business day convention specified in
	/// paymentDatesAdjustments. This element must only be included if there is an initial stub. This date will
	/// normally correspond to an unadjusted calculation period start or end date. This is true even if early or delayed
	/// payment is specified to be applicable since the actual first payment date will be the specified number of days
	/// before or after the applicable adjusted calculation period start or end date with the resulting payment date then
	/// being adjusted in accordance with any business day convention specified in paymentDatesAdjustments.</newpara>
	/// <newpara>lastRegularPaymentDate (zero or one occurrence; of the type xsd:date) The last regular unadjusted payment
	/// date. This day may be subject to adjustment in accordance with any business day convention specified in
	/// paymentDatesAdjustments. This element must only be included if there is a final stub. All calculation periods
	/// after this date contribute to the final payment. The final payment is made relative to the final set of calculation
	/// periods or the final reset date as the case may be. This date will normally correspond to an unadjusted
	/// calculation period start or end date. This is true even if early or delayed payment is specified to be applicable
	/// since the actual last regular payment date will be the specified number of days before or after the applicable
	/// adjusted calculation period start or end date with the resulting payment date then being adjusted in
	/// accordance with any business day convention specified in paymentDatesAdjustments.</newpara>
	/// <newpara>payRelativeTo (exactly one occurrence; of the type PayRelativeToEnum) Specifies whether the payments
	/// occur relative to each adjusted calculation period start date, adjusted calculation period end date or each reset
	/// date. The reset date is applicable in the case of certain euro (former French Franc) floating rate indices.
	/// Calculation period start date means relative to the start of the first calculation period contributing to a given
	/// payment. Similarly, calculation period end date means the end of the last calculation period contributing to a
	/// given payment.</newpara>
	/// <newpara>paymentDaysOffset (zero or one occurrence; of the type Offset) If early payment or delayed payment is
	/// required, specifies the number of days offset that the payment occurs relative to what would otherwise be the
	/// unadjusted payment date. The offset can be specified in terms of either calendar or business days. Even in the
	/// case of a calendar days offset, the resulting payment date, adjusted for the specified calendar days offset, will
	/// still be adjusted in accordance with the specified payment dates adjustments. This element should only be
	/// included if early or delayed payment is applicable, i.e. if the periodMultiplier element value is not equal to zero.
	/// An early payment would be indicated by a negative periodMultiplier element value and a delayed payment (or
	/// payment lag) would be indicated by a positive periodMultiplier element value.</newpara>
	/// <newpara>paymentDatesAdjustments (exactly one occurrence; of the type BusinessDayAdjustments) The business
	/// day convention to apply to each payment date if it would otherwise fall on a day that is not a business day in
	/// the specified financial business centers.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: InterestRateStream</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	[System.Xml.Serialization.XmlRootAttribute("paymentDates", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
	public partial class PaymentDates : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference to")]
		public EFS_RadioChoice paymentDatesDateReference;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentDatesDateReferenceCalculationPeriodDatesReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesReference", typeof(DateReference), Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[ControlGUI(Name = "value", Width = 190)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.CalculationPeriodDates)]
		public DateReference paymentDatesDateReferenceCalculationPeriodDatesReference;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentDatesDateReferenceResetDatesReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("resetDatesReference", typeof(DateReference), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[ControlGUI(Name = "value", Width = 190)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.ResetDates)]
		public DateReference paymentDatesDateReferenceResetDatesReference;

		[System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Order = 3)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Frequency", IsVisible = false)]
		public Interval paymentFrequency;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool firstPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("firstPaymentDate", Order = 4)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Frequency")]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Payment Dates", IsVisible = false, IsGroup = true)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First payment date")]
		public EFS_Date firstPaymentDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool lastRegularPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("lastRegularPaymentDate", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last Regular payment date")]
		public EFS_Date lastRegularPaymentDate;
		[System.Xml.Serialization.XmlElementAttribute("payRelativeTo", Order = 6)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Stub Payment Dates")]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates Calculation", IsVisible = false, IsGroup = true)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Payment Relative to", LblWidth = 228, Width = 210)]
		public PayRelativeToEnum payRelativeTo;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentDaysOffsetSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentDaysOffset", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Offset")]
		public Offset paymentDaysOffset;
		[System.Xml.Serialization.XmlElementAttribute("paymentDatesAdjustments", Order = 8)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates Adjustments", IsVisible = false, IsCopyPaste = true)]
		public BusinessDayAdjustments paymentDatesAdjustments;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates Adjustments")]
		public bool FillBalise;
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
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Dates Calculation")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentDates)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
		public EFS_Id efs_id;
		#endregion Members
	}
	#endregion PaymentDates
	#region PrincipalExchange
	/// <summary>
	/// <newpara><b>Description :</b> A type defining a principal exchange amount and adjusted exchange date. 
	/// The type forms part of the cashflow representation of a swap stream.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>unadjustedPrincipalExchangeDate (zero or one occurrence; of the type xsd:date)</newpara>
	/// <newpara>adjustedPrincipalExchangeDate (zero or one occurrence; of the type xsd:date) The principal exchange date.
	/// This date should already be adjusted for any applicable business day convention.</newpara>
	/// <newpara>principalExchangeAmount (zero or one occurrence; of the type xsd:decimal) The principal exchange
	/// amount. This amount should be positive if the stream payer is paying the exchange amount and signed
	/// negative if they are receiving it.</newpara>
	/// <newpara></newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: Cashflows</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class PrincipalExchange : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool unadjustedPrincipalExchangeDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("unadjustedPrincipalExchangeDate", Order = 1)]
		public EFS_Date unadjustedPrincipalExchangeDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool adjustedPrincipalExchangeDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustedPrincipalExchangeDate", Order = 2)]
		public EFS_Date adjustedPrincipalExchangeDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool principalExchangeAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("principalExchangeAmount", Order = 3)]
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
		#endregion Members

	}
	#endregion PrincipalExchange

	#region ResetDates
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the parameters used to generate the reset dates schedule and associated 
	/// fixing dates. The reset dates are determined relative to the calculation periods schedules dates.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>calculationPeriodDatesReference (exactly one occurrence; of the type DateReference) A pointer style
	/// reference to the associated calculation period dates component defined elsewhere in the document.</newpara>
	/// <newpara>resetRelativeTo (zero or one occurrence; of the type ResetRelativeToEnum) Specifies whether the reset
	/// dates are determined with respect to each adjusted calculation period start date or adjusted calculation period
	/// end date. If the reset frequency is specified as daily this element must not be included.</newpara>
	/// <newpara>initialFixingDate (zero or one occurrence; of the type RelativeDateOffset)</newpara>
	/// <newpara>fixingDates (exactly one occurrence; of the type RelativeDateOffset) Specifies the fixing date relative to the
	/// reset date in terms of a business days offset and an associated set of financial business centers. Normally
	/// these offset calculation rules will be those specified in the ISDA definition for the relevant floating rate index
	/// (ISDA's Floating Rate Option). However, non-standard offset calculation rules may apply for a trade if mutually
	/// agreed by the principal parties to the transaction. The href attribute on the dateRelativeTo element should
	/// reference the id attribute on the resetDates element.</newpara>
	/// <newpara>rateCutOffDaysOffset (zero or one occurrence; of the type Offset) Specifies the number of business days
	/// before the period end date when the rate cut-off date is assumed to apply. The financial business centers
	/// associated with determining the rate cut-off date are those specified in the reset dates adjustments. The rate
	/// cut-off number of days must be a negative integer (a value of zero would imply no rate cut off applies in which
	/// case the rateCutOffDaysOffset element should not be included). The relevant rate for each reset date in the
	/// period from, and including, a rate cut-off date to, but excluding, the next applicable period end date (or, in the
	/// case of the last calculation period, the termination date) will (solely for purposes of calculating the floating
	/// amount payable on the next applicable payment date) be deemed to be the relevant rate in effect on that rate
	/// cut-off date. For example, if rate cut-off days for a daily averaging deal is -2 business days, then the refix rate
	/// applied on (period end date - 2 days) will also be applied as the reset on (period end date - 1 day), i.e. the
	/// actual number of reset dates remains the same but from the rate cut-off date until the period end date, the
	/// same refix rate is applied. Note that in the case of several calculation periods contributing to a single payment,
	/// the rate cut-off is assumed only to apply to the final calculation period contributing to that payment. The day
	/// type associated with the offset must imply a business days offset.</newpara>
	/// <newpara>resetFrequency (exactly one occurrence; of the type ResetFrequency) The frequency at which reset dates
	/// occur. In the case of a weekly reset frequency, also specifies the day of the week that the reset occurs. If the
	/// reset frequency is greater than the calculation period frequency then this implies that more than one reset date
	/// is established for each calculation period and some form of rate averaging is applicable.</newpara>
	/// <newpara>resetDatesAdjustments (exactly one occurrence; of the type BusinessDayAdjustments) The business day
	/// convention to apply to each reset date if it would otherwise fall on a day that is not a business day in the
	/// specified financial business centers.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: InterestRateStream</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	[System.Xml.Serialization.XmlRootAttribute("resetDates", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
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
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reset Dates Calculation")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.ResetDates)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
		public EFS_Id efs_id;
		#endregion Members
	}
	#endregion ResetDates

	#region SettlementRateSource
	/// <summary>
	/// <newpara><b>Description :</b> A type describing the method for obtaining a settlement rate.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Either</newpara>
	/// <newpara>informationSource (exactly one occurrence; of the type InformationSource) The information source where a
	/// published or displayed market rate will be obtained, e.g. Telerate Page 3750.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>cashSettlementReferenceBanks (exactly one occurrence; of the type CashSettlementReferenceBanks) A
	/// container for a set of reference institutions. These reference institutions may be called upon to provide rate
	/// quotations as part of the method to determine the applicable cash settlement amount. If institutions are not
	/// specified, it is assumed that reference institutions will be agreed between the parties on the exercise date, or
	/// in the case of swap transaction to which mandatory early termination is applicable, the cash settlement
	/// valuation date.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: YieldCurveMethod</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class SettlementRateSource : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Settlement Rate Source")]
		public EFS_RadioChoice source;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool sourceInformationSourceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("informationSource", typeof(InformationSource), Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Information Source", IsVisible = true)]
		public InformationSource sourceInformationSource;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool sourceCashSettlementReferenceBanksSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlementReferenceBanks", typeof(CashSettlementReferenceBanks), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Cash Settlement ReferenceBanks", IsVisible = true)]
		public CashSettlementReferenceBanks sourceCashSettlementReferenceBanks;
		#endregion Members
		#region Constructors
		public SettlementRateSource()
		{
			sourceInformationSource = new InformationSource();
			sourceCashSettlementReferenceBanks = new CashSettlementReferenceBanks();
		}
		#endregion Constructors
	}
	#endregion SettlementRateSource
	#region SinglePartyOption
	/// <summary>
	/// <newpara><b>Description :</b> A type describing the buyer and seller of an option.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>buyerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that buys
	/// this instrument, ie. pays for this instrument and receives the rights defined by it. See 2000 ISDA definitions
	/// Article 11.1 (b). In the case of FRAs this the fixed rate payer.</newpara>
	/// <newpara>sellerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that sells
	/// ("writes") this instrument, i.e. that grants the rights defined by this instrument and in return receives a payment
	/// for it. See 2000 ISDA definitions Article 11.1 (a). In the case of FRAs this is the floating rate payer.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: OptionalEarlyTermination</newpara>
	///<newpara>• Complex type: </newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlRootAttribute("singlePartyOption", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
	public class SinglePartyOption
	{
		#region Members
		[ControlGUI(Name = "Buyer")]
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
		public PartyReference buyerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
		[ControlGUI(Name = "Seller")]
		public PartyReference sellerPartyReference;
		#endregion Members
	}
	#endregion SinglePartyOption
	#region Stub
	/// <summary>
	/// <newpara><b>Description :</b> A type defining how a stub calculation period amount is calculated. 
	/// A single floating rate tenor different to that used for the regular part of the calculation periods schedule may be specified, 
	/// or two floating rate tenors many be specified. If two floating rate tenors are specified then Linear Interpolation 
	/// (in accordance with the 2000 ISDA Definitions, Section 8.3 Interpolation) is assumed to apply. 
	/// Alternatively, an actual known stub rate or stub amount may be specified.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Either</newpara>
	/// <newpara>floatingRate (one or more occurrences; of the type FloatingRate) The rates to be applied to the initial or final
	/// stub may be the linear interpolation of two different rates. While the majority of the time, the rate indices will be
	/// the same as that specified in the stream and only the tenor itself will be different, it is possible to specift two
	/// different rates. For example, a 2 month stub period may use the linear interpolation of a 1 month and 3 month
	/// rate. The different rates would be specified in this component. Note that a maximum of two rates can be
	/// specified. If a stub period uses the same floating rate index, including tenor, as the regular calculation periods
	/// then this should not be specified again within this component, i.e. the stub calculation period amount
	/// component may not need to be specified even if there is an initial or final stub period. If a stub period uses a
	/// different floating rate index compared to the regular calculation periods then this should be specified within this
	/// component. If specified here, they are likely to have id attributes, allowing them to be referenced from within
	/// the cashflows component.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>stubRate (exactly one occurrence; of the type xsd:decimal) An actual rate to apply for the initial or final stub
	/// period may have been agreed between the principal parties (in a similar way to how an initial rate may have
	/// been agreed for the first regular period). If an actual stub rate has been agreed then it would be included in
	/// this component. It will be a per annum rate, expressed as a decimal. A stub rate of 5% would be represented
	/// as 0.05.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>stubAmount (exactly one occurrence; of the type Money) An actual amount to apply for the initial or final stub
	/// period may have been agreed between th two parties. If an actual stub amount has been agreed then it would
	/// be included in this component.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: StubCalculationPeriodAmount</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class Stub : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Stub Type")]
		public EFS_RadioChoice stubType;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stubTypeFloatingRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("floatingRate", typeof(FloatingRate), Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[ControlGUI(Name = "Floating Rate")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Floating Rate", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
		public FloatingRate[] stubTypeFloatingRate;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stubTypeFixedRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("stubRate", typeof(EFS_Decimal), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[ControlGUI(Name = "rate", Width = 100, Regex = EFSRegex.TypeRegex.RegexFixedRate)]
		public EFS_Decimal stubTypeFixedRate;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stubTypeAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("stubAmount", typeof(Money), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[ControlGUI(Name = "amount")]
		public Money stubTypeAmount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stubStartDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("stubStartDate", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Start Date")]
		public AdjustableOrRelativeDate stubStartDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stubEndDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("stubEndDate", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "End Date")]
		public AdjustableOrRelativeDate stubEndDate;
		#endregion Members
	}
	#endregion Stub
	#region StubCalculationPeriodAmount
	/// <summary>
	/// <newpara><b>Description :</b> A type defining how the initial or final stub calculation period amounts is calculated. 
	/// For example, the rate to be applied to the initial or final stub calculation period may be the linear interpolation 
	/// of two different tenors for the floating rate index specified in the calculation period amount component, 
	/// e.g. A two month stub period may used the linear interpolation of a one month and three month floating rate. 
	/// The different rate tenors would be specified in this component. Note that a maximum of two rate tenors can be specified. 
	/// If a stub period uses a single index tenor and this is the same as that specified in the calculation period amount 
	/// component then the initial stub or final stub component, as the case may be, must not be included.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>calculationPeriodDatesReference (exactly one occurrence; of the type DateReference) A pointer style
	/// reference to the associated calculation period dates component defined elsewhere in the document.</newpara>
	/// <newpara>initialStub (zero or one occurrence; of the type Stub) Specifies how the initial stub amount is calculated. A
	/// single floating rate tenor different to that used for the regular part of the calculation periods schedule may be
	/// specified, or two floating tenors may be specified. If two floating rate tenors are specified then Linear
	/// Interpolation (in accordance with the 2000 ISDA Definitions, Section 8.3. Interpolation) is assumed to apply.
	/// Alternatively, an actual known stub rate or stub amount may be specified.</newpara>
	/// <newpara>finalStub (zero or one occurrence; of the type Stub) Specifies how the final stub amount is calculated. A
	/// single floating rate tenor different to that used for the regular part of the calculation periods schedule may be
	/// specified, or two floating tenors may be specified. If two floating rate tenors are specified then Linear
	/// Interpolation (in accordance with the 2000 ISDA Definitions, Section 8.3. Interpolation) is assumed to apply.
	/// Alternatively, an actual known stub rate or stub amount may be specified.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: InterestRateStream</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
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
	/// <summary>
	/// <newpara><b>Description :</b> A type defining swap streams and additional payments between the principal parties involved 
	/// in the swap.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type Product)</newpara>
	/// <newpara>• The base type which all FpML products extend.</newpara>
	/// <newpara>swapStream (one or more occurrences; of the type InterestRateStream) The swap streams.</newpara>
	/// <newpara>earlyTerminationProvision (zero or one occurrence; of the type EarlyTerminationProvision) Parameters
	/// specifying provisions relating to the optional and mandatory early terminarion of a swap transaction.</newpara>
	/// <newpara>cancelableProvision (zero or one occurrence; of the type CancelableProvision) A provision that allows the
	/// specification of an embedded option within a swap giving the buyer of the option the right to terminate the
	/// swap, in whole or in part, on the early termination date.</newpara>
	/// <newpara>extendibleProvision (zero or one occurrence; of the type ExtendibleProvision) A provision that allows the
	/// specification of an embedded option with a swap giving the buyer of the option the right to extend the swap, in
	/// whole or in part, to the extended termination date.</newpara>
	/// <newpara>additionalPayment (zero or more occurrences; of the type Payment) Additional payments between the
	/// principal parties.</newpara>
	/// <newpara></newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Element: swap</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
	[System.Xml.Serialization.XmlRootAttribute("swap", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
	public partial class Swap : Product
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("swapStream", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 1)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Swap Stream", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 2, IsCopyPasteItem = true)]
		[BookMarkGUI(Name = "S", IsVisible = true)]
		public InterestRateStream[] swapStream;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool earlyTerminationProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Early Termination Provision")]
		public EarlyTerminationProvision earlyTerminationProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cancelableProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cancelableProvision", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cancelable Provision")]
		public CancelableProvision cancelableProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool extendibleProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("extendibleProvision", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Extendible Provision")]
		public ExtendibleProvision extendibleProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool additionalPaymentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("additionalPayment", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Additional Payment")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Additional Payment", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
		[BookMarkGUI(IsVisible = false)]
		public Payment[] additionalPayment;
		#endregion Members
		#region Members
		/*
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool implicitEarlyTerminationProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("implicitEarlyTerminationProvision", Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Implicit Early Termination Provision")]
		public Empty implicitEarlyTerminationProvision;
		*/
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool stepUpProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("stepUpProvision", Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Step-Up Provision")]
		public StepUpProvision stepUpProvision;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool implicitProvisionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("implicitProvision", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Implicit Provision")]
		public ImplicitProvision implicitProvision;
		#endregion Members
	}
	#endregion Swap
	#region Swaption
	/// <summary>
	/// <newpara><b>Description :</b> A type to define an option on a swap.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type Product)</newpara>
	/// <newpara>• The base type which all FpML products extend.</newpara>
	/// <newpara>buyerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that buys
	/// this instrument, ie. pays for this instrument and receives the rights defined by it. See 2000 ISDA definitions
	/// Article 11.1 (b). In the case of FRAs this the fixed rate payer.</newpara>
	/// <newpara>sellerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that sells
	/// ("writes") this instrument, i.e. that grants the rights defined by this instrument and in return receives a payment
	/// for it. See 2000 ISDA definitions Article 11.1 (a). In the case of FRAs this is the floating rate payer.</newpara>
	/// <newpara>premium (zero or more occurrences; of the type Payment) The option premium amount payable by buyer to
	/// seller on the specified payment date.</newpara>
	/// <newpara>exercise (exactly one occurrence; of the type Exercise) An placeholder for the actual option exercise
	/// definitions.</newpara>
	/// <newpara>exerciseProcedure (exactly one occurrence; of the type ExerciseProcedure) A set of parameters defining
	/// procedures associated with the exercise.</newpara>
	/// <newpara>calculationAgentPartyReference (one or more occurrences; of the type PartyReference) A pointer style
	/// reference to a party identifier defined elsewhere in the document. The party referenced is the ISDA Calculation
	/// Agent for the trade. If more than one party is referenced then the parties are assumed to be co-calculation
	/// agents, i.e. they have joint reponsibility.</newpara>
	/// <newpara>cashSettlement (zero or one occurrence; of the type CashSettlement) If specified, this means that cash
	/// settlement is applicable to the transaction and defines the parameters associated with the cash settlement
	/// procedure. If not specified, then physical settlement is applicable.</newpara>
	/// <newpara>swaptionStraddle (exactly one occurrence; of the type xsd:boolean) Whether the option is a swaption or a
	/// swaption straddle.</newpara>
	/// <newpara>swaptionAdjustedDates (zero or one occurrence; of the type SwaptionAdjustedDates) The adjusted dates
	/// associated with swaption exercise. These dates have been adjusted for any applicable business day
	/// convention.</newpara>
	/// <newpara>swap (exactly one occurrence; of the type Swap) A swap product definition.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Element: swaption</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	[System.Xml.Serialization.XmlRootAttribute("swaption", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
	public partial class Swaption : Product
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
		[ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
		public PartyReference buyerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
		[ControlGUI(Name = "Seller")]
		public PartyReference sellerPartyReference;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool premiumSpecified;
		[System.Xml.Serialization.XmlElementAttribute("premium", Order = 3)]
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
		[System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise), Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American Exercise", IsVisible = true)]
		public AmericanExercise exerciseAmerican;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool exerciseBermudaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise), Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bermuda Exercise", IsVisible = true)]
		public BermudaExercise exerciseBermuda;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool exerciseEuropeanSpecified;
		[System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise), Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European Exercise", IsVisible = true)]
		public EuropeanExercise exerciseEuropean;
		[System.Xml.Serialization.XmlElementAttribute("exerciseProcedure", Order = 7)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Exercise Procedure", IsVisible = true)]
        public ExerciseProcedure procedure;
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Exercise Procedure")]
		[System.Xml.Serialization.XmlElementAttribute("calculationAgent", Order = 8)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Calculation Agent", IsVisible = true)]
		public CalculationAgent calculationAgent;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cashSettlementSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlement", Order = 9)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Calculation Agent")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Cash Settlement")]
		public CashSettlement cashSettlement;
		[System.Xml.Serialization.XmlElementAttribute("swaptionStraddle", Order = 10)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Swaption Straddle")]
		public EFS_Boolean swaptionStraddle;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool swaptionAdjustedDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("swaptionAdjustedDates", Order = 11)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Swaption Adjusted Dates")]
		public SwaptionAdjustedDates swaptionAdjustedDates;
		[System.Xml.Serialization.XmlElementAttribute("swap", Order = 12)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Swap", IsVisible = false)]
		public Swap swap;
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Swap")]
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool FillBalise;
		#endregion Members
	}
	#endregion Swaption
	#region SwaptionAdjustedDates
	/// <summary>
	/// <newpara><b>Description :</b> A type describing the adjusted dates associated with swaption exercise and settlement.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>exerciseEvent (one or more occurrences; of the type ExerciseEvent) The adjusted dates associated with an
	/// individual swaption exercise date.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: Swaption</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public class SwaptionAdjustedDates : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("exerciseEvent", Order = 1)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Swaption Adjusted Dates", IsClonable = true, IsChild = true)]
		public ExerciseEvent[] exerciseEvent;
		#endregion Members
	}
	#endregion SwaptionAdjustedDates

	#region YieldCurveMethod
	/// <summary>
	/// <newpara><b>Description :</b> A type defining the parameters required for each of the ISDA defined yield curve methods 
	/// for cash settlement.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: CashSettlement</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class YieldCurveMethod
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool settlementRateSourceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementRateSource", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "settlementRateSource")]
		public SettlementRateSource settlementRateSource;
		[System.Xml.Serialization.XmlElementAttribute("quotationRateType", Order = 2)]
		[ControlGUI(Name = "quotationRateType", LblWidth = 115)]
		public QuotationRateTypeEnum quotationRateType;
		#endregion Members
	}
	#endregion YieldCurveMethod
}

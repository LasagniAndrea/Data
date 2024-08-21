#region Using Directives
using System;
using System.Collections;
using System.Reflection;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
using EfsML.Interface;

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

namespace FpML.v42.Fx
{
	#region ExchangeRate
	public partial class ExchangeRate : IExchangeRate
	{
		#region Constructors
		public ExchangeRate()
		{
			quotedCurrencyPair = new QuotedCurrencyPair();
			rate = new EFS_Decimal();
		}
		#endregion Constructors

		#region IExchangeRate Members
		IQuotedCurrencyPair IExchangeRate.quotedCurrencyPair{get {return this.quotedCurrencyPair ; }}
		EFS_Decimal IExchangeRate.rate
		{
			set { this.rate = value;}
			get { return this.rate;}
		}
		bool IExchangeRate.spotRateSpecified
		{
			set { this.spotRateSpecified = value; }
			get { return this.spotRateSpecified;}
		}
		EFS_Decimal IExchangeRate.spotRate
		{
			set { this.spotRate = value; }
			get {return this.spotRate;}
		}
		bool IExchangeRate.forwardPointsSpecified
		{
			set { this.forwardPointsSpecified = value;}
			get { return this.forwardPointsSpecified;}
		}
		EFS_Decimal IExchangeRate.forwardPoints 
        { 
			set { this.forwardPoints = value; }
			get { return this.forwardPoints; } 
		}
		bool IExchangeRate.fxFixingSpecified 
		{
			set { this.fxFixingSpecified = value; } 
			get { return this.fxFixingSpecified; } 
		}
		IFxFixing IExchangeRate.fxFixing
		{
			set { this.fxFixing = (FxFixing)value;}
			get { return this.fxFixing;}
		}
		bool IExchangeRate.sideRatesSpecified 
		{
			set { this.sideRatesSpecified = value; } 
			get { return this.sideRatesSpecified; }
		}
		ISideRates IExchangeRate.sideRates { get { return this.sideRates; } }
		IFxFixing IExchangeRate.CreateFxFixing { get { return new FxFixing(); } }
		IMoney IExchangeRate.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
		#endregion IExchangeRate Members
	}
	#endregion ExchangeRate
	#region ExpiryDateTime
	public partial class ExpiryDateTime : IExpiryDateTime
	{
		#region Accessors
		#region ExpiryDateTimeValue
		public DateTime ExpiryDateTimeValue
		{
			get
			{
				return DtFunc.AddTimeToDate(expiryDate.DateValue, expiryTime.hourMinuteTime.TimeValue);
			}
		}
		#endregion ExpiryDateTimeValue
		#endregion Accessors

        #region constructors
        public ExpiryDateTime()
        {
            this.expiryDate = new EFS_Date();
            this.expiryTime = new BusinessCenterTime();
            this.cutName = new CutName();
        }
        #endregion constructors

		#region IExpiryDateTime Members
		EFS_Date IExpiryDateTime.expiryDate 
		{
			set { this.expiryDate = value;}
			get { return this.expiryDate;}
		}
		DateTime IExpiryDateTime.ExpiryDateTimeValue
		{
			get { return this.ExpiryDateTimeValue; }
		}
		IHourMinuteTime IExpiryDateTime.expiryTime
		{
			set { this.expiryTime.hourMinuteTime =(HourMinuteTime) value; }
			get { return this.expiryTime.hourMinuteTime; }
		}
		string IExpiryDateTime.businessCenter
		{
			set { this.expiryTime.businessCenter.Value = value; }
			get { return this.expiryTime.businessCenter.Value; }
		}
		bool IExpiryDateTime.cutNameSpecified
		{
			set { this.cutNameSpecified = value; }
			get { return this.cutNameSpecified; }
		}
		string IExpiryDateTime.cutName
		{
			set { this.cutName.Value = value; }
			get { return this.cutName.Value; }
		}
		IBusinessCenterTime IExpiryDateTime.businessCenterTime
		{
			get	{return this.expiryTime;}
		}
		#endregion IExpiryDateTime Members
	}
	#endregion ExpiryDateTime

	#region FxAmericanTrigger
	public partial class FxAmericanTrigger : IEFS_Array,IFxAmericanTrigger
	{
		#region Accessors
		#region TriggerRate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public object TriggerRate
		{
			get
			{
				return new EFS_TriggerRate(triggerRate.DecValue, spotRate.DecValue, (IQuotedCurrencyPair)quotedCurrencyPair);
			}
		}
		#endregion TriggerRate
		#endregion Accessors
		#region Constructors
		public FxAmericanTrigger()
		{
			quotedCurrencyPair   = new QuotedCurrencyPair();
			triggerRate          = new EFS_Decimal();
			observationStartDate = new EFS_Date();
			observationEndDate   = new EFS_Date();
		}
		#endregion Constructors
		#region Methods
		#region AdjustedObservationEndDate
		public EFS_Date AdjustedObservationEndDate(EFS_Date pExpiryDate)
		{
			if (observationEndDateSpecified)
				return new EFS_Date(observationEndDate.Value);
			else
				return pExpiryDate;
		}
		#endregion AdjustedObservationEndDate
		#region ObservationStartDate
		public EFS_EventDate ObservationStartDate(EFS_EventDate pTradeDate)
		{
			if (observationStartDateSpecified)
				return new EFS_EventDate(observationStartDate.DateValue, observationStartDate.DateValue);
			else
				return pTradeDate;
		}
		#endregion ObservationStartDate
		#region ObservationEndDate
		public EFS_EventDate ObservationEndDate(EFS_EventDate pExpiryDate)
		{
			if (observationEndDateSpecified)
				return new EFS_EventDate(observationEndDate.DateValue, observationEndDate.DateValue);
			else
				return pExpiryDate;
		}
		#endregion ObservationEndDate
		#region TriggerType
		public string TriggerType(EFS_Decimal pSpotRate, EFS_Date pTradeDate)
		{
			string triggerType = string.Empty;
			if (null == spotRate)
				spotRate = pSpotRate;
			if (null == spotRate) // Cela ne me plait pas à cet endroit EG le 30/07/2005
			{
				// Recherche du spot à tradeDate
				spotRate = new EFS_Decimal(0);
			}
			if (TouchConditionEnum.Touch == touchCondition)
			{
				if (spotRate.DecValue < triggerRate.DecValue)
					triggerType = EventTypeFunc.UpTouch;
				else
					triggerType = EventTypeFunc.DownTouch;
			}
			else if (TouchConditionEnum.Notouch == touchCondition)
			{
				if (spotRate.DecValue < triggerRate.DecValue)
					triggerType = EventTypeFunc.UpNoTouch;
				else
					triggerType = EventTypeFunc.DownNoTouch;
			}
			return triggerType;
		}
		#endregion TriggerType
		#endregion Methods

		#region IEFS_Array Members
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion IEFS_Array Members
		#region IFxAmericanTrigger Members
		TouchConditionEnum IFxAmericanTrigger.touchCondition
		{
			set { this.touchCondition = value; }
			get { return this.touchCondition; }
		}
		bool IFxAmericanTrigger.observationStartDateSpecified
		{
			set { this.observationStartDateSpecified = value; }
			get { return this.observationStartDateSpecified; }
		}
		EFS_Date IFxAmericanTrigger.observationStartDate
		{
			set { this.observationStartDate = value; }
			get { return this.observationStartDate; }
		}
		bool IFxAmericanTrigger.observationEndDateSpecified
		{
			set { this.observationEndDateSpecified = value; }
			get { return this.observationEndDateSpecified; }
		}
		EFS_Date IFxAmericanTrigger.observationEndDate
		{
			set { this.observationEndDate = value; }
			get { return this.observationEndDate; }
		}
		#endregion IFxAmericanTrigger Members
		#region IFxTrigger Members
		EFS_Decimal IFxTrigger.triggerRate
		{
			set { this.triggerRate = value; }
			get { return this.triggerRate; }
		}
		IQuotedCurrencyPair IFxTrigger.quotedCurrencyPair
		{
			set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
			get { return this.quotedCurrencyPair; }
		}
		IInformationSource[] IFxTrigger.informationSource
		{
			set { this.informationSource = (InformationSource[])value; }
			get { return this.informationSource; }
		}
		IInformationSource[] IFxTrigger.CreateInformationSources { get { return new InformationSource[1] { new InformationSource() }; } }
		#endregion IFxTrigger Members
	}
	#endregion FxAmericanTrigger
	#region FxAverageRateObservationDate
	public partial class FxAverageRateObservationDate : IEFS_Array,IFxAverageRateObservationDate
	{
		#region Constructors
		public FxAverageRateObservationDate(){}
		#endregion Constructors

		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
				return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
			else
				return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IFxAverageRateObservationDate Members
		DateTime IFxAverageRateObservationDate.observationDate {get { return this.observationDate.DateValue; }}
		decimal IFxAverageRateObservationDate.averageRateWeightingFactor { get { return this.averageRateWeightingFactor.DecValue; } }
		#endregion IFxAverageRateObservationDate Members
	}
	#endregion FxAverageRateObservationDate
	#region FxAverageRateObservationSchedule
	public partial class FxAverageRateObservationSchedule : IFxAverageRateObservationSchedule
	{
		#region IFxAverageRateObservationSchedule Members
		EFS_Date IFxAverageRateObservationSchedule.observationStartDate 
		{
			get { return this.observationStartDate; } 
		}
		EFS_Date IFxAverageRateObservationSchedule.observationEndDate 
		{
			get { return this.observationEndDate; } 
		}
		ICalculationPeriodFrequency IFxAverageRateObservationSchedule.calculationPeriodFrequency
		{
			set { this.calculationPeriodFrequency =(CalculationPeriodFrequency) value; }
			get { return this.calculationPeriodFrequency; }
		}
		#endregion IFxAverageRateObservationSchedule Members
	}
	#endregion FxAverageRateObservationSchedule
	#region FxAverageRateOption
	public partial class FxAverageRateOption : IProduct, IFxAverageRateOption, IDeclarativeProvision
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_FxAverageRateOption efs_FxAverageRateOption;
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:FxAverageRateOption";
		#endregion Members
		#region Accessors
		#region BuyerPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string BuyerPartyReference
		{
			get {return buyerPartyReference.href;}
		}
		#endregion BuyerPartyReference
		#region SellerPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string SellerPartyReference
		{
			get{return sellerPartyReference.href; }
        }
		#endregion SellerPartyReference
		#endregion Accessors
        #region Constructors
		public FxAverageRateOption()
		{
			rateObservationSchedule = new FxAverageRateObservationSchedule();
			rateObservationDate = new FxAverageRateObservationDate[1] { new FxAverageRateObservationDate() };
			fxStrikePrice = new FxStrikePrice();
			bermudanExerciseDates = new DateList();
		}
		#endregion Constructors

		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IFxAverageRateOption Members
		bool IFxAverageRateOption.spotRateSpecified
		{
			set { this.spotRateSpecified = value; }
			get { return this.spotRateSpecified; }
		}
		EFS_Decimal IFxAverageRateOption.spotRate
		{
			set { this.spotRate = value; }
			get { return this.spotRate; }
		}
		ICurrency IFxAverageRateOption.payoutCurrency
		{
			set { this.payoutCurrency = (Currency)value; }
			get { return this.payoutCurrency; }
		}
		bool IFxAverageRateOption.payoutFormulaSpecified
		{
			set { this.payoutFormulaSpecified = value; }
			get { return this.payoutFormulaSpecified; }
		}
		string IFxAverageRateOption.payoutFormula
		{
			set { this.payoutFormula = new EFS_MultiLineString(value); }
			get { return this.payoutFormula.Value; }
		}
		IInformationSource IFxAverageRateOption.primaryRateSource
		{
			set { this.primaryRateSource = (InformationSource)value; }
			get { return this.primaryRateSource; }
		}
		StrikeQuoteBasisEnum IFxAverageRateOption.averageRateQuoteBasis
		{
			set { this.averageRateQuoteBasis = value; }
			get { return this.averageRateQuoteBasis; }
		}
		bool IFxAverageRateOption.secondaryRateSourceSpecified
		{
			set { this.secondaryRateSourceSpecified = value; }
			get { return this.secondaryRateSourceSpecified; }
		}
		IInformationSource IFxAverageRateOption.secondaryRateSource
		{
			set { this.secondaryRateSource = (InformationSource)value; }
			get { return this.secondaryRateSource; }
		}
		IBusinessCenterTime IFxAverageRateOption.fixingTime
		{
			set { this.fixingTime = (BusinessCenterTime)value; }
			get { return this.fixingTime; }
		}
		bool IFxAverageRateOption.bermudanExerciseDatesSpecified { get { return this.bermudanExerciseDatesSpecified; } }
		IDateList IFxAverageRateOption.bermudanExerciseDates { get { return this.bermudanExerciseDates; } }
		bool IFxAverageRateOption.averageStrikeOptionSpecified 
		{ 
			set { this.averageStrikeOptionSpecified = value; } 
			get { return this.averageStrikeOptionSpecified; } 
		}
		IAverageStrikeOption IFxAverageRateOption.averageStrikeOption
		{
			set { this.averageStrikeOption = (AverageStrikeOption)value; }
			get { return this.averageStrikeOption; }
		}
		bool IFxAverageRateOption.observedRatesSpecified { get { return this.observedRatesSpecified; } }
		IObservedRates[] IFxAverageRateOption.observedRates { get { return this.observedRates; } }
		bool IFxAverageRateOption.rateObservationDateSpecified
		{
			set { this.rateObservationDateSpecified = value; }
			get { return this.rateObservationDateSpecified; }
		}
		IFxAverageRateObservationDate[] IFxAverageRateOption.rateObservationDate
		{
			set { this.rateObservationDate = (FxAverageRateObservationDate[])value; }
			get { return this.rateObservationDate; }
		}
		bool IFxAverageRateOption.rateObservationScheduleSpecified
		{
			set { this.rateObservationScheduleSpecified = value; }
			get { return this.rateObservationScheduleSpecified; }
		}
		IFxAverageRateObservationSchedule IFxAverageRateOption.rateObservationSchedule
		{
			set { this.rateObservationSchedule = (FxAverageRateObservationSchedule)value; }
			get { return this.rateObservationSchedule; }
		}
		bool IFxAverageRateOption.geometricAverageSpecified
		{
			set { this.geometricAverageSpecified = value; }
			get { return this.geometricAverageSpecified; }
		}
		bool IFxAverageRateOption.precisionSpecified
		{
			set { this.precisionSpecified = value; }
			get { return this.precisionSpecified; }
		}
		EFS_PosInteger IFxAverageRateOption.precision
		{
			set { this.precision = value; }
			get { return this.precision; }
		}
		IInformationSource IFxAverageRateOption.CreateInformationSource { get { return new InformationSource(); } }
		IFxAverageRateObservationSchedule IFxAverageRateOption.CreateFxAverageRateObservationSchedule { get { return new FxAverageRateObservationSchedule(); } }
		IFxAverageRateObservationDate[] IFxAverageRateOption.CreateFxAverageRateObservationDates { get { return new FxAverageRateObservationDate[1]; } }
		EFS_FxAverageRateOption IFxAverageRateOption.efs_FxAverageRateOption 
		{ 
			get { return this.efs_FxAverageRateOption; } 
			set { this.efs_FxAverageRateOption = value; } 
		}
		#endregion IFxAverageRateOption Members
		#region IFxOptionBase Membres
		EFS_Date IFxOptionBase.valueDate { get { return this.valueDate; } }
		IExpiryDateTime IFxOptionBase.expiryDateTime { get { return this.expiryDateTime; } }

        IReference IFxOptionBase.buyerPartyReference
        {
            get { return this.buyerPartyReference; }
            set { buyerPartyReference = (PartyReference)value; } 
        }
        IReference IFxOptionBase.sellerPartyReference
        {
            get { return this.sellerPartyReference; }
            set { sellerPartyReference = (PartyReference)value; }
        }
		bool IFxOptionBase.optionTypeSpecified
		{
			set { this.optionTypeSpecified = value; }
			get { return this.optionTypeSpecified; }
		}
		OptionTypeEnum IFxOptionBase.optionType
		{
			set { this.optionType = value; }
			get { return this.optionType; }
		}
		bool IFxOptionBase.callCurrencyAmountSpecified { get { return true; } }
		IMoney IFxOptionBase.callCurrencyAmount { get { return this.callCurrencyAmount; } }
		bool IFxOptionBase.putCurrencyAmountSpecified { get { return true;}}
		IMoney IFxOptionBase.putCurrencyAmount { get { return this.putCurrencyAmount; } }
		bool IFxOptionBase.fxOptionPremiumSpecified 
		{ 
			set { this.fxOptionPremiumSpecified = value; } 
			get { return this.fxOptionPremiumSpecified; } 
		}
		IFxOptionPremium[] IFxOptionBase.fxOptionPremium { get { return this.fxOptionPremium; } }
		#endregion IFxOptionBase Membres
		#region IFxOptionBaseNotDigital Membres
		ExerciseStyleEnum IFxOptionBaseNotDigital.exerciseStyle { get { return this.exerciseStyle; } }
		bool IFxOptionBaseNotDigital.procedureSpecified { get { return this.procedureSpecified; } }
		IExerciseProcedure IFxOptionBaseNotDigital.procedure { get { return this.procedure; } }
		IFxStrikePrice IFxOptionBaseNotDigital.fxStrikePrice { get { return this.fxStrikePrice; } }
		#endregion IFxOptionBaseNotDigital Membres
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return null; } }
		bool IDeclarativeProvision.extendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return null; } }
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return null; } }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion FxAverageRateOption
	#region FxBarrier
	public partial class FxBarrier : IEFS_Array,IFxBarrier
	{
		#region Accessors
		#region TriggerRate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public object TriggerRate
		{
			get
			{
				return new EFS_TriggerRate(triggerRate.DecValue, spotRate.DecValue, quotedCurrencyPair);
			}
		}
		#endregion TriggerRate
		#endregion Accessors
		#region Constructors
		public FxBarrier()
		{
			quotedCurrencyPair = new QuotedCurrencyPair();
			observationStartDate = new EFS_Date();
			observationEndDate = new EFS_Date();
		}
		#endregion Constructors
		#region Methods
		#region AdjustedObservationEndDate
		public EFS_Date AdjustedObservationEndDate(EFS_Date pExpiryDate)
		{
			if (observationEndDateSpecified)
				return new EFS_Date(observationEndDate.Value);
			else
				return pExpiryDate;
		}
		#endregion AdjustedObservationEndDate
		#region BarrierType
		public string BarrierType(EFS_Decimal pSpotRate, EFS_Date pTradeDate, bool pIsFxCapBarrier, bool pIsFxFloorBarrier)
		{
			string barrierType = string.Empty;
			if (null == spotRate)
				spotRate = pSpotRate;
			if (null == spotRate) // Cela ne me plait pas à cet endroit EG le 30/07/2005
			{
				// Recherche du spot à tradeDate
				spotRate = new EFS_Decimal(0);
			}
			if (pIsFxCapBarrier)
				barrierType = EventTypeFunc.CappedCall;
			else if (pIsFxFloorBarrier)
				barrierType = EventTypeFunc.FlooredPut;
			else if ((FxBarrierTypeEnum.Knockin == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockin == fxBarrierType))
			{
				if (spotRate.DecValue < triggerRate.DecValue)
					barrierType = EventTypeFunc.UpIn;
				else
					barrierType = EventTypeFunc.DownIn;
			}
			else if ((FxBarrierTypeEnum.Knockout == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockout == fxBarrierType))
			{
				if (spotRate.DecValue < triggerRate.DecValue)
					barrierType = EventTypeFunc.UpOut;
				else
					barrierType = EventTypeFunc.DownOut;
			}
			return barrierType;
		}
		#endregion BarrierType
		#region ObservationStartDate
		public EFS_EventDate ObservationStartDate(EFS_EventDate pTradeDate)
		{
			if (observationStartDateSpecified)
				return new EFS_EventDate(observationStartDate.DateValue, observationStartDate.DateValue);
			else
				return pTradeDate;
		}
		#endregion ObservationStartDate
		#region ObservationEndDate
		public EFS_EventDate ObservationEndDate(EFS_EventDate pExpiryDate)
		{
			if (observationEndDateSpecified)
				return new EFS_EventDate(observationEndDate.DateValue, observationEndDate.DateValue);
			else
				return pExpiryDate;
		}
		#endregion ObservationEndDate
		#region RebateBarrierType
		public string RebateBarrierType(EFS_Decimal pSpotRate, EFS_Date pTradeDate)
		{
			string barrierType = string.Empty;
			if (null == spotRate)
				spotRate = pSpotRate;
			if (null == spotRate) // Cela ne me plait pas à cet endroit EG le 30/07/2005
			{
				// Recherche du spot à tradeDate
				spotRate = new EFS_Decimal(0);
			}
			if ((FxBarrierTypeEnum.Knockin == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockin == fxBarrierType))
			{
				if (spotRate.DecValue < triggerRate.DecValue)
					barrierType = EventTypeFunc.RebateUpIn;
				else
					barrierType = EventTypeFunc.RebateDownIn;
			}
			else if ((FxBarrierTypeEnum.Knockout == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockout == fxBarrierType))
			{
				if (spotRate.DecValue < triggerRate.DecValue)
					barrierType = EventTypeFunc.RebateUpOut;
				else
					barrierType = EventTypeFunc.RebateDownOut;
			}
			return barrierType;
		}
		#endregion RebateBarrierType
		#endregion Methods

		#region IEFS_Array members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
				return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
			else
				return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array members
		#region IFxBarrier Members
		FxBarrierTypeEnum IFxBarrier.fxBarrierType
		{
			set { this.fxBarrierType = value; }
			get { return this.fxBarrierType; }
		}
		bool IFxBarrier.fxBarrierTypeSpecified
		{
			set { this.fxBarrierTypeSpecified = value; }
			get { return this.fxBarrierTypeSpecified; }
		}
		IInformationSource[] IFxBarrier.informationSource
		{
			set { this.informationSource = (InformationSource[])value; }
			get { return this.informationSource; }
		}
		EFS_Decimal IFxBarrier.triggerRate
		{
			set { this.triggerRate = value; }
			get { return this.triggerRate; }
		}
		bool IFxBarrier.observationStartDateSpecified
		{
			set { this.observationStartDateSpecified = value; }
			get { return this.observationStartDateSpecified; }
		}
		EFS_Date IFxBarrier.observationStartDate
		{
			set { this.observationStartDate = value; }
			get { return this.observationStartDate; }
		}
		bool IFxBarrier.observationEndDateSpecified
		{
			set { this.observationEndDateSpecified = value; }
			get { return this.observationEndDateSpecified; }
		}
		EFS_Date IFxBarrier.observationEndDate
		{
			set { this.observationEndDate = value; }
			get { return this.observationEndDate; }
		}
		IQuotedCurrencyPair IFxBarrier.quotedCurrencyPair
		{
			set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
			get { return this.quotedCurrencyPair; }
		}
		IInformationSource[] IFxBarrier.CreateInformationSources { get { return new InformationSource[1] { new InformationSource() }; } }
		#endregion IFxBarrier Members
	}
	#endregion FxBarrier
	#region FxBarrierOption
	public partial class FxBarrierOption : IProduct, IFxBarrierOption, IDeclarativeProvision
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_FxBarrierOption efs_FxBarrierOption;
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:FxBarrierOption";
		#endregion Members
		#region Constructors
		public FxBarrierOption()
		{
			procedure = new ExerciseProcedure();
			bermudanExerciseDates = new DateList();
			fxRebateBarrier = new FxBarrier();
		}
		#endregion Constructors

		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IFxBarrierOption Members
		bool IFxBarrierOption.bermudanExerciseDatesSpecified { get { return this.bermudanExerciseDatesSpecified; } }
		IDateList IFxBarrierOption.bermudanExerciseDates { get { return this.bermudanExerciseDates; } }
		bool IFxBarrierOption.cashSettlementTermsSpecified { get { return this.cashSettlementTermsSpecified; } }
        IFxCashSettlement IFxBarrierOption.cashSettlementTerms
        {
            set { this.cashSettlementTerms = (FxCashSettlement)value; }
            get { return this.cashSettlementTerms; }
        }
		bool IFxBarrierOption.spotRateSpecified 
		{ 
			set { this.spotRateSpecified = value; } 
			get { return this.spotRateSpecified; } 
		}
		EFS_Decimal IFxBarrierOption.spotRate 
		{ 
			set { this.spotRate = value; } 
			get { return this.spotRate; } 
		}
		IFxBarrier[] IFxBarrierOption.fxBarrier { get { return this.fxBarrier; } }
		bool IFxBarrierOption.fxRebateBarrierSpecified 
		{ 
			set { this.fxRebateBarrierSpecified = value; } 
			get { return this.fxRebateBarrierSpecified; } 
		}
		IFxBarrier IFxBarrierOption.fxRebateBarrier { get { return this.fxRebateBarrier; } }
		bool IFxBarrierOption.cappedCallOrFlooredPutSpecified { get { return this.cappedCallOrFlooredPutSpecified; } }
		ICappedCallOrFlooredPut IFxBarrierOption.cappedCallOrFlooredPut { get { return this.cappedCallOrFlooredPut; } }
		bool IFxBarrierOption.triggerPayoutSpecified 
		{ 
			set { this.triggerPayoutSpecified = value; } 
			get { return this.triggerPayoutSpecified; } 
		}
		IFxOptionPayout IFxBarrierOption.triggerPayout { get { return this.triggerPayout; } }
		EFS_FxBarrierOption IFxBarrierOption.efs_FxBarrierOption 
		{ 
			get { return this.efs_FxBarrierOption; } 
			set { this.efs_FxBarrierOption = value; } 
		}
		#endregion IFxBarrierOption Members
		#region IFxOptionBase Membres
		EFS_Date IFxOptionBase.valueDate { get { return this.valueDate; } }
		IExpiryDateTime IFxOptionBase.expiryDateTime { get { return this.expiryDateTime; } }

        IReference IFxOptionBase.buyerPartyReference
        {
            get { return this.buyerPartyReference; }
            set { buyerPartyReference = (PartyReference) value; }
        }
        IReference IFxOptionBase.sellerPartyReference
        {
            get { return this.sellerPartyReference; }
            set { sellerPartyReference = (PartyReference)value; }
        }
		bool IFxOptionBase.optionTypeSpecified
		{
			set { this.optionTypeSpecified = value; }
			get { return this.optionTypeSpecified; }
		}
		OptionTypeEnum IFxOptionBase.optionType
		{
			set { this.optionType = value; }
			get { return this.optionType; }
		}
		bool IFxOptionBase.callCurrencyAmountSpecified { get { return true; } }
		IMoney IFxOptionBase.callCurrencyAmount { get { return this.callCurrencyAmount; } }
		bool IFxOptionBase.putCurrencyAmountSpecified { get { return true;}}
		IMoney IFxOptionBase.putCurrencyAmount { get { return this.putCurrencyAmount; } }
		bool IFxOptionBase.fxOptionPremiumSpecified
		{
			set { this.fxOptionPremiumSpecified = value; }
			get { return this.fxOptionPremiumSpecified; }
		}
		IFxOptionPremium[] IFxOptionBase.fxOptionPremium { get { return this.fxOptionPremium; } }
		#endregion IFxOptionBase Membres
		#region IFxOptionBaseNotDigital Membres
		ExerciseStyleEnum IFxOptionBaseNotDigital.exerciseStyle { get { return this.exerciseStyle; } }
		bool IFxOptionBaseNotDigital.procedureSpecified { get { return this.procedureSpecified; } }
		IExerciseProcedure IFxOptionBaseNotDigital.procedure { get { return this.procedure; } }
		IFxStrikePrice IFxOptionBaseNotDigital.fxStrikePrice { get { return this.fxStrikePrice; } }
		#endregion IFxOptionBaseNotDigital Membres
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return null; } }
		bool IDeclarativeProvision.extendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return null; } }
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return null; } }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion FxBarrierOption
	#region FxDigitalOption
	public partial class FxDigitalOption : IProduct, IFxDigitalOption, IDeclarativeProvision
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_FxDigitalOption efs_FxDigitalOption;

		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:FxDigitalOption";
		#endregion Members
		#region Accessors
		#region AdjustedExpiryDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Date AdjustedExpiryDate
		{
			get { return new EFS_Date(expiryDateTime.expiryDate.Value); }
		}
		#endregion AdjustedExpiryDate
		#region BuyerPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string BuyerPartyReference
		{
			get {return buyerPartyReference.href; }
		}
		#endregion BuyerPartyReference
		#region ExpiryDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_EventDate ExpiryDate
		{
			get { return new EFS_EventDate(expiryDateTime.expiryDate.DateValue, expiryDateTime.expiryDate.DateValue); }
		}
		#endregion ExpiryDate
		#region SellerPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string SellerPartyReference
		{
			get {return sellerPartyReference.href; }
		}
		#endregion SellerPartyReference
		#endregion Accessors
		#region Constructors
		public FxDigitalOption()
		{
			typeTriggerAmerican = new FxAmericanTrigger[1] { new FxAmericanTrigger() };
			typeTriggerEuropean = new FxEuropeanTrigger[1] { new FxEuropeanTrigger() };
		}
		#endregion Constructors
		#region Methods
		#region CalcPayout
		public decimal CalcPayout(bool pIsInTheMoney, DateTime pTouchDate, decimal pSpotRate)
		{
			decimal amount = 0;
			if (pIsInTheMoney)
			{
				DateTime touchDate = ExpiryDate.adjustedDate.DateValue;
				if (PayoutEnum.Immediate == triggerPayout.payoutStyle)
					touchDate = pTouchDate;
				amount = triggerPayout.amount.DecValue;
				if (extinguishingSpecified)
					amount = CalcPayoutPeriod(extinguishing, amount, touchDate, pSpotRate);
				else if (resurrectingSpecified)
					amount = CalcPayoutPeriod(resurrecting, amount, touchDate, pSpotRate);
			}
			return amount;
		}
		#endregion CalcPayout
		#region CalcPayoutPeriod
		public static decimal CalcPayoutPeriod(PayoutPeriod pPayoutPeriod, decimal pAmount, DateTime pTouchDate, decimal pSpotRate)
		{
			decimal amount = pAmount;
			return amount;
		}
		#endregion CalcPayoutPeriod
		#endregion Methods

		#region IFxDigitalOption Members
		IQuotedCurrencyPair IFxDigitalOption.quotedCurrencyPair { get { return this.quotedCurrencyPair; } }
		bool IFxDigitalOption.typeTriggerEuropeanSpecified 
		{ 
			set { this.typeTriggerEuropeanSpecified = value; } 
			get { return this.typeTriggerEuropeanSpecified; } 
		}
		IFxEuropeanTrigger[] IFxDigitalOption.typeTriggerEuropean { get { return this.typeTriggerEuropean; } }
		bool IFxDigitalOption.typeTriggerAmericanSpecified 
		{ 
			set { this.typeTriggerAmericanSpecified = value; } 
			get { return this.typeTriggerAmericanSpecified; } 
		}
		IFxAmericanTrigger[] IFxDigitalOption.typeTriggerAmerican { get { return this.typeTriggerAmerican; } }
		IFxOptionPayout IFxDigitalOption.triggerPayout { get { return this.triggerPayout; } }
		bool IFxDigitalOption.spotRateSpecified 
		{
			set { this.spotRateSpecified = value;}
			get { return this.spotRateSpecified;}
		}
		EFS_Decimal IFxDigitalOption.spotRate 
		{
			set { this.spotRate = value;}
			get { return this.spotRate;}
		}
		bool IFxDigitalOption.resurrectingSpecified{get {return this.resurrectingSpecified;}}
		IPayoutPeriod IFxDigitalOption.resurrecting{get { return this.resurrecting;}}
		bool IFxDigitalOption.extinguishingSpecified{get { return this.extinguishingSpecified;}}
		IPayoutPeriod IFxDigitalOption.extinguishing{get { return this.extinguishing;}}
		bool IFxDigitalOption.fxBarrierSpecified 
		{ 
			set { this.fxBarrierSpecified = value; } 
			get { return this.fxBarrierSpecified; } 
		}
		IFxBarrier[] IFxDigitalOption.fxBarrier { get { return this.fxBarrier; } }
		bool IFxDigitalOption.boundarySpecified { get { return this.boundarySpecified; } }
		bool IFxDigitalOption.limitSpecified { get { return this.limitSpecified; } }
		bool IFxDigitalOption.assetOrNothingSpecified { get { return this.assetOrNothingSpecified; } }
		IAssetOrNothing IFxDigitalOption.assetOrNothing { get { return this.assetOrNothing; } }
		EFS_EventDate IFxDigitalOption.ExpiryDate { get { return this.ExpiryDate; } }
		EFS_FxDigitalOption IFxDigitalOption.efs_FxDigitalOption
		{
			get { return this.efs_FxDigitalOption; }
			set { this.efs_FxDigitalOption = value; }
		}
		#endregion IFxDigitalOption Members
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IFxOptionBase Membres
		IExpiryDateTime IFxOptionBase.expiryDateTime { get { return this.expiryDateTime; } }
		EFS_Date IFxOptionBase.valueDate { get { return this.valueDate; } }

        IReference IFxOptionBase.buyerPartyReference
        {
            get { return this.buyerPartyReference; }
            set { buyerPartyReference = (PartyReference) value; } 
        }
        IReference IFxOptionBase.sellerPartyReference
        {
            get { return this.sellerPartyReference; }
            set { sellerPartyReference = (PartyReference) value; }
        }
		bool IFxOptionBase.optionTypeSpecified
		{
			set { ; }
			get { return false; }
		}
		OptionTypeEnum IFxOptionBase.optionType
		{
			set { ; }
			get { return OptionTypeEnum.Call; }
		}
		bool IFxOptionBase.callCurrencyAmountSpecified { get { return false; } }
		IMoney IFxOptionBase.callCurrencyAmount { get { return null; } }
		bool IFxOptionBase.putCurrencyAmountSpecified { get { return false; } }
		IMoney IFxOptionBase.putCurrencyAmount { get { return null; } }
		bool IFxOptionBase.fxOptionPremiumSpecified
		{
			set { this.fxOptionPremiumSpecified = value; }
			get { return this.fxOptionPremiumSpecified; }
		}
		IFxOptionPremium[] IFxOptionBase.fxOptionPremium { get { return this.fxOptionPremium; } }
		#endregion IFxOptionBase Membres
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return null; } }
		bool IDeclarativeProvision.extendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return null; } }
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return null; } }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion FxDigitalOption
	#region FxEuropeanTrigger
	public partial class FxEuropeanTrigger : IEFS_Array,IFxEuropeanTrigger
	{
		#region Accessors
		#region TriggerRate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public object TriggerRate
		{
			get
			{
                return new EFS_TriggerRate(triggerRate.DecValue, spotRate.DecValue, quotedCurrencyPair);
			}
		}
		#endregion TriggerRate
		#endregion Accessors
		#region Constructors
		public FxEuropeanTrigger()
		{
			quotedCurrencyPair = new QuotedCurrencyPair();
			triggerRate = new EFS_Decimal();
		}
		#endregion Constructors
		#region Methods
		#region TriggerType
		public string TriggerType(EFS_Decimal pSpotRate, EFS_Date pTradeDate)
		{
			string triggerType = string.Empty;
			if (null == spotRate)
				spotRate = pSpotRate;
			if (null == spotRate) // Cela ne me plait pas à cet endroit EG le 30/07/2005
			{
				// Recherche du spot à tradeDate
				spotRate = new EFS_Decimal(0);
			}
			if (TriggerConditionEnum.Above == triggerCondition)
				triggerType = EventTypeFunc.Above;
			else if (TriggerConditionEnum.Below == triggerCondition)
				triggerType = EventTypeFunc.Below;
			return triggerType;
		}
		#endregion TriggerType
		#endregion Methods

		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IFxEuropeanTrigger Members
		TriggerConditionEnum IFxEuropeanTrigger.triggerCondition
		{ 
			set { this.triggerCondition = value; } 
			get { return this.triggerCondition; } 
		}
		#endregion IFxEuropeanTrigger Members
		#region IFxTrigger Members
		EFS_Decimal IFxTrigger.triggerRate
		{
			set { this.triggerRate = value; }
			get { return this.triggerRate; }
		}
		IQuotedCurrencyPair IFxTrigger.quotedCurrencyPair
		{
			set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
			get { return this.quotedCurrencyPair; }
		}
		IInformationSource[] IFxTrigger.informationSource
		{
			set { this.informationSource = (InformationSource[])value; }
			get { return this.informationSource; }
		}
		IInformationSource[] IFxTrigger.CreateInformationSources { get { return new InformationSource[1] { new InformationSource() }; } }
		#endregion IFxTrigger Members
	}
	#endregion FxEuropeanTrigger
	#region FxLeg
    /// <summary>
    /// 
    /// </summary>
    /// FI 20150331 [XXPOC] Modify
	public partial class FxLeg : IEFS_Array, IProduct, IFxLeg, IDeclarativeProvision
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_FxLeg efs_FxLeg;
		#endregion Members
		#region Constructors
		public FxLeg()
		{
			fxDateValueDate = new EFS_Date();
			fxDateCurrency1ValueDate = new EFS_Date();
			fxDateCurrency2ValueDate = new EFS_Date();
		}
		#endregion Constructors

		#region IEFS_Array Members
		#region DisplayArray
		public new object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IFxLeg Members
		IPayment IFxLeg.exchangedCurrency1
		{
			set { this.exchangedCurrency1 = (Payment)value; }
			get { return this.exchangedCurrency1; }
		}
		IPayment IFxLeg.exchangedCurrency2
		{
			set { this.exchangedCurrency2 = (Payment)value; }
			get { return this.exchangedCurrency2; }
		}
		IExchangeRate IFxLeg.exchangeRate
		{
			set { this.exchangeRate = (ExchangeRate)value; }
			get { return this.exchangeRate; }
		}
		bool IFxLeg.fxDateValueDateSpecified 
		{
			set { this.fxDateValueDateSpecified = value;}
			get { return this.fxDateValueDateSpecified;}
		}
		EFS_Date IFxLeg.fxDateValueDate { get { return this.fxDateValueDate; } }
		bool IFxLeg.fxDateCurrency1ValueDateSpecified 
		{
			set {this.fxDateCurrency1ValueDateSpecified = value; }
			get {return this.fxDateCurrency1ValueDateSpecified; }
		}
		EFS_Date IFxLeg.fxDateCurrency1ValueDate { get { return this.fxDateCurrency1ValueDate; } }
		bool IFxLeg.fxDateCurrency2ValueDateSpecified
		{
			set { this.fxDateCurrency2ValueDateSpecified = value;}
			get { return this.fxDateCurrency2ValueDateSpecified;}
		}
		EFS_Date IFxLeg.fxDateCurrency2ValueDate { get { return this.fxDateCurrency2ValueDate; } }
		bool IFxLeg.nonDeliverableForwardSpecified 
		{
			set { this.nonDeliverableForwardSpecified = value; }
			get { return this.nonDeliverableForwardSpecified; }
		}
		IFxCashSettlement IFxLeg.nonDeliverableForward {get { return this.nonDeliverableForward;}}
		EFS_FxLeg IFxLeg.efs_FxLeg
		{
			get { return this.efs_FxLeg; }
			set { this.efs_FxLeg = value; }
		}
		IPayment IFxLeg.CreatePayment { get { return new Payment(); } }
		IExchangeRate IFxLeg.CreateExchangeRate { get { return new ExchangeRate(); } }
        /// EG 20150402 [POC] add
        bool IFxLeg.marginRatioSpecified
        {
            set { ; }
            get { return false; }
        }
        /// EG 20150402 [POC] add
        IMarginRatio IFxLeg.marginRatio
        {
            get { return null; }
            set { ; }
        }
        /// EG 20150402 [POC] add
        IMarginRatio IFxLeg.CreateMarginRatio
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150731 [XXXXX] add  (Ajouter permet à BA de consulter des trades en FpML.v42)
        /// FI 20170116 [21916] RptSide (R majuscule)
        FixML.Interface.IFixTrdCapRptSideGrp[] IFxLeg.RptSide
        {
            set { RptSide = (FixML.v50SP1.TrdCapRptSideGrp_Block[])value; }
            get { return RptSide; }
        }

		#endregion IFxLeg Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return null; } }
		bool IDeclarativeProvision.extendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return null; } }
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return null; } }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion FxLeg
	#region FXOptionLeg
	public partial class FxOptionLeg : IProduct, IFxOptionLeg, IDeclarativeProvision
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_FxSimpleOption efs_FxSimpleOption;
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:FxOptionLeg";
		#endregion Members
		#region Accessors
		#region BuyerPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string BuyerPartyReference
		{
			get{return buyerPartyReference.href; }
		}
		#endregion BuyerPartyReference
		#region SellerPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string SellerPartyReference
		{
			get{return sellerPartyReference.href; }
		}
		#endregion SellerPartyReference
		#endregion Accessors
		#region Constructors
		public FxOptionLeg()
		{
			fxStrikePrice = new FxStrikePrice();
			bermudanExerciseDates = new DateList();
			procedure = new ExerciseProcedure();
		}
		#endregion Constructors

		#region IFxOptionLeg Membres
        bool IFxOptionLeg.bermudanExerciseDatesSpecified
        {
            get { return this.bermudanExerciseDatesSpecified; }
            set { this.bermudanExerciseDatesSpecified = value; } 
        }
        IDateList IFxOptionLeg.bermudanExerciseDates
        {
            get { return this.bermudanExerciseDates; }
            set { this.bermudanExerciseDates = (DateList)value; }   
        }
        bool IFxOptionLeg.cashSettlementTermsSpecified
        {
            get { return this.cashSettlementTermsSpecified; }
            set { this.cashSettlementTermsSpecified = true; } 
        }
        IFxCashSettlement IFxOptionLeg.cashSettlementTerms
        {
            get { return this.cashSettlementTerms; }
            set { this.cashSettlementTerms = (FxCashSettlement)value; }
        }
        
        EFS_FxSimpleOption IFxOptionLeg.efs_FxSimpleOption
		{
			get { return this.efs_FxSimpleOption; }
			set { this.efs_FxSimpleOption = value; }
		}
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150731 [XXXXX] add  (Ajouter permet à BA de consulter des trades en FpML.v42)
        FixML.Interface.IFixTrdCapRptSideGrp[] IFxOptionLeg.RptSide
        {
            set { RptSide = (FixML.v50SP1.TrdCapRptSideGrp_Block[])value; }
            get { return RptSide; }
        }
        /// EG 20150402 [POC] add
        bool IFxOptionLeg.marginRatioSpecified
        {
            set { ; }
            get { return false; }
        }
        /// EG 20150402 [POC] add
        IMarginRatio IFxOptionLeg.marginRatio
        {
            get { return null; }
            set { ; }
        }
        /// EG 20150402 [POC] add
        IMarginRatio IFxOptionLeg.CreateMarginRatio
        {
            get
            {
                return null;
            }
        }
		#endregion IFxOptionLeg Membres
		#region IFxOptionBase Members
		IExpiryDateTime IFxOptionBase.expiryDateTime { get { return this.expiryDateTime; } }
		EFS_Date IFxOptionBase.valueDate { get { return this.valueDate; } }

        IReference IFxOptionBase.buyerPartyReference
        {
            get { return this.buyerPartyReference; }
            set { this.buyerPartyReference = (PartyReference)  value; } 
        }
        IReference IFxOptionBase.sellerPartyReference
        {
            get { return this.sellerPartyReference; }
            set { this.sellerPartyReference = (PartyReference) value; } 
        }
		bool IFxOptionBase.optionTypeSpecified
		{
			set { this.optionTypeSpecified = value; }
			get { return this.optionTypeSpecified; }
		}
		OptionTypeEnum IFxOptionBase.optionType
		{
			set { this.optionType = value; }
			get { return this.optionType; }
		}
		bool IFxOptionBase.callCurrencyAmountSpecified { get { return true; } }
		IMoney IFxOptionBase.callCurrencyAmount { get { return this.callCurrencyAmount; } }
		bool IFxOptionBase.putCurrencyAmountSpecified { get { return true;}}
		IMoney IFxOptionBase.putCurrencyAmount { get { return this.putCurrencyAmount; } }
		bool IFxOptionBase.fxOptionPremiumSpecified
		{
			set { this.fxOptionPremiumSpecified = value; }
			get { return this.fxOptionPremiumSpecified; }
		}
		IFxOptionPremium[] IFxOptionBase.fxOptionPremium { get { return this.fxOptionPremium; } }
		#endregion IFxOptionBase Members
		#region IFxOptionBaseNotDigital Membres
		ExerciseStyleEnum IFxOptionBaseNotDigital.exerciseStyle { get { return this.exerciseStyle; } }
		bool IFxOptionBaseNotDigital.procedureSpecified { get { return this.procedureSpecified; } }
		IExerciseProcedure IFxOptionBaseNotDigital.procedure { get { return this.procedure; } }
		IFxStrikePrice IFxOptionBaseNotDigital.fxStrikePrice { get { return this.fxStrikePrice; } }
		#endregion IFxOptionBaseNotDigital Membres
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return null; } }
		bool IDeclarativeProvision.extendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return null; } }
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return null; } }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion FxOptionLeg
	#region FxOptionPayout
	public partial class FxOptionPayout : IFxOptionPayout
	{
		#region Accessors
		#region PayoutAmount
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal PayoutAmount
		{
			get{return amount; }
		}
		#endregion PayoutAmount
		#region PayoutCurrency
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PayoutCurrency
		{
			get{return currency.Value; }
		}

		#endregion PayoutCurrency
		#endregion Accessors

		#region IFxOptionPayout Members
		PayoutEnum IFxOptionPayout.payoutStyle 
		{ 
			set { this.payoutStyle = value; } 
			get { return this.payoutStyle; } 
		}
		EFS_Decimal IFxOptionPayout.amount
		{
			get { return this.amount; }
		}
		string IFxOptionPayout.currency	
		{
			set { this.currency.Value = value;}
			get { return this.currency.Value;}
		}
		bool IFxOptionPayout.settlementInformationSpecified
		{
			set { this.settlementInformationSpecified = value; }
			get { return this.settlementInformationSpecified; }
		}
		ISettlementInformation IFxOptionPayout.settlementInformation
		{
			get { return this.settlementInformation; }
		}
        bool IFxOptionPayout.customerSettlementPayoutSpecified
        {
            set { ; }
            get { return false; }
        }
        ICustomerSettlementPayment IFxOptionPayout.customerSettlementPayout
        {
            set { ; }
            get { return null; }
        }
        ICustomerSettlementPayment IFxOptionPayout.CreateCustomerSettlementPayment { get { return new CustomerSettlementPayment(); } }
        IMoney IFxOptionPayout.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
		#endregion IFxOptionPayout Members
	}
	#endregion FxOptionPayout
	#region FxOptionPremium
	public partial class FxOptionPremium : IEFS_Array, IFxOptionPremium
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_FxOptionPremium efs_FxOptionPremium;
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:FxOptionPremium";

		#endregion Members
		#region Accessors
		#region AdjustedPreSettlementDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedPreSettlementDate
		{
			get {return efs_FxOptionPremium.AdjustedPreSettlementDate; }
		}
		#endregion AdjustedPreSettlementDate
		#region AdjustedSettlementDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedSettlementDate
		{
			get {return efs_FxOptionPremium.AdjustedSettlementDate; }
		}
		#endregion AdjustedSettlementDate
		#region ExchangeRate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_ExchangeRate ExchangeRate
		{
			get {return efs_FxOptionPremium.ExchangeRate; }
		}
		#endregion ExchangeRate
		#region ExpirationDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate ExpirationDate
		{
			get {return efs_FxOptionPremium.ExpirationDate; }
		}
		#endregion ExpirationDate
		#region PayerPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PayerPartyReference
		{
			get {return efs_FxOptionPremium.PayerPartyReference; }
		}
		#endregion PayerPartyReference
		#region PremiumAmount
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal PremiumAmount
		{
			get {return efs_FxOptionPremium.PremiumAmount; }
		}

		#endregion PremiumAmount
		#region PremiumCurrency
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PremiumCurrency
		{
			get {return efs_FxOptionPremium.PremiumCurrency; }
		}
		#endregion PremiumCurrency
		#region PremiumQuote
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_FxPremiumQuote PremiumQuote
		{
			get {return efs_FxOptionPremium.premiumQuote; }
		}
		#endregion PremiumQuote
		#region SettlementDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate SettlementDate
		{
			get {return efs_FxOptionPremium.SettlementDate; }
		}
		#endregion SettlementDate
		#region ReceiverPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string ReceiverPartyReference
		{
			get{return efs_FxOptionPremium.ReceiverPartyReference; }
		}
		#endregion ReceiverPartyReference

		#endregion Accessors
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members

		#region IFxOptionPremium Members
        IReference IFxOptionPremium.payerPartyReference
        {
            get { return this.payerPartyReference; }
            set { this.payerPartyReference = (PartyReference)value; }
        }
        IReference IFxOptionPremium.receiverPartyReference
        {
            get { return this.receiverPartyReference; }
            set { this.receiverPartyReference = (PartyReference)value; }
        }
		EFS_Date IFxOptionPremium.premiumSettlementDate
		{
			set { this.premiumSettlementDate = value; }
			get { return this.premiumSettlementDate; }
		}
		bool IFxOptionPremium.customerSettlementPremiumSpecified
		{
			set { this.customerSettlementPremiumSpecified = value; }
			get { return this.customerSettlementPremiumSpecified; }
		}
		ICustomerSettlementPayment IFxOptionPremium.customerSettlementPremium 
		{ 
			set { this.customerSettlementPremium = (CustomerSettlementPayment)value; } 
			get { return this.customerSettlementPremium; } 
		}
        IMoney IFxOptionPremium.premiumAmount
        {
            get { return this.premiumAmount; }
            set { this.premiumAmount = (Money)value; }
        }
		bool IFxOptionPremium.premiumQuoteSpecified
		{
			set {this.premiumQuoteSpecified = value; }
			get {return this.premiumQuoteSpecified; }
		}
		IPremiumQuote IFxOptionPremium.premiumQuote { get { return this.premiumQuote; } }
		EFS_FxOptionPremium IFxOptionPremium.efs_FxOptionPremium
		{
			get { return this.efs_FxOptionPremium; }
			set { this.efs_FxOptionPremium = value; }
		}
		IMoney IFxOptionPremium.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
		bool IFxOptionPremium.settlementInformationSpecified
		{
			set { this.settlementInformationSpecified = value; }
			get { return this.settlementInformationSpecified; }
		}
		ISettlementInformation IFxOptionPremium.settlementInformation
		{
			get { return this.settlementInformation; }
		}
		ICustomerSettlementPayment IFxOptionPremium.CreateCustomerSettlementPayment { get { return new CustomerSettlementPayment(); } }
		#endregion IFxOptionPremium Members
	}
	#endregion FxOptionPremium
	#region FxStrikePrice
	public partial class FxStrikePrice : IFxStrikePrice
	{
		#region Constructors
		public FxStrikePrice(){}
		#endregion Constructors

		#region IFxStrikePrice Members
		EFS_Decimal IFxStrikePrice.rate 
		{ 
			set {this.rate = value;} 
			get {return this.rate;} 
		}
		StrikeQuoteBasisEnum IFxStrikePrice.strikeQuoteBasis 
		{
			set { this.strikeQuoteBasis = value; } 
			get { return this.strikeQuoteBasis; } 
		}
		#endregion IFxStrikePrice Members

	}
	#endregion FXStrikePrice
	#region FxSwap
	public partial class FxSwap : IProduct,IFxSwap,IDeclarativeProvision
	{
		#region Accessors
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_FxLeg[] efs_FxLeg
		{
			get
			{
				ArrayList aFxLeg = new ArrayList();
				foreach (FxLeg item in fxSingleLeg)
				{
					aFxLeg.Add(item.efs_FxLeg);
				}
				return (EFS_FxLeg[])aFxLeg.ToArray(typeof(EFS_FxLeg));
			}
		}
		#endregion Accessors

		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IFxSwap Members
		IFxLeg[] IFxSwap.fxSingleLeg {get { return this.fxSingleLeg;} }
		#endregion IFxSwap Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return null; } }
		bool IDeclarativeProvision.extendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return null; } }
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return null; } }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion FxSwap

	#region ObservedRates
	public partial class ObservedRates : IEFS_Array, IObservedRates
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members

		#region IObservedRates Members
		DateTime IObservedRates.observationDate{get { return this.observationDate.DateValue;}}
		decimal IObservedRates.observedRate{get { return this.observedRate.DecValue;} }
		#endregion IObservedRates Members
	}
	#endregion ObservedRates

	#region PremiumQuote
	public partial class PremiumQuote : IPremiumQuote
	{
		#region IPremiumQuote Members
		EFS_Decimal IPremiumQuote.premiumValue 
		{ 
			set { this.premiumValue = value; } 
			get { return this.premiumValue; } 
		}
		PremiumQuoteBasisEnum IPremiumQuote.premiumQuoteBasis 
		{ 
			set { this.premiumQuoteBasis = value; } 
			get { return this.premiumQuoteBasis; } 
		}
		#endregion IPremiumQuote Members
	}
	#endregion PremiumQuote

	#region SideRate
	public partial class SideRate : ICloneable,ISideRate
	{
		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			SideRate clone = new SideRate();
			clone.currency = (Currency)this.currency.Clone();
			clone.sideRateBasis = this.sideRateBasis;
			clone.rate = (EFS_Decimal)this.rate.Clone();
			clone.spotRateSpecified = this.spotRateSpecified;
			if (clone.spotRateSpecified)
				clone.spotRate = (EFS_Decimal)this.spotRate.Clone();
			clone.forwardPointsSpecified = this.forwardPointsSpecified;
			if (clone.forwardPointsSpecified)
				clone.forwardPoints = (EFS_Decimal)this.forwardPoints.Clone();
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region ISideRate Members
		string ISideRate.currency { get { return this.currency.Value; } }
		SideRateBasisEnum ISideRate.sideRateBasis
		{
			set { this.sideRateBasis = value; }
			get { return this.sideRateBasis; }
		}
		EFS_Decimal ISideRate.rate { get { return this.rate; } }
		bool ISideRate.spotRateSpecified { get { return this.spotRateSpecified; } }
		EFS_Decimal ISideRate.spotRate { get { return this.spotRate; } }
		bool ISideRate.forwardPointsSpecified { get { return this.forwardPointsSpecified; } }
		EFS_Decimal ISideRate.forwardPoints { get { return this.forwardPoints; } }
		IMoney ISideRate.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
		#endregion ISideRate Members

	}
	#endregion SideRate
	#region SideRates
	public partial class SideRates : ISideRates
	{
		#region Constructors
		public SideRates()
		{
			currency1SideRate = new SideRate();
			currency2SideRate = new SideRate();
		}
		#endregion Constructors

		#region ISideRates Members
		ICurrency ISideRates.baseCurrency { get { return this.baseCurrency; } }
		bool ISideRates.currency1SideRateSpecified { get { return this.currency1SideRateSpecified; } }
		ISideRate ISideRates.currency1SideRate { get { return this.currency1SideRate; } }
		bool ISideRates.currency2SideRateSpecified { get { return this.currency2SideRateSpecified; } }
		ISideRate ISideRates.currency2SideRate { get { return this.currency2SideRate; } }
		#endregion ISideRates Members
	}
	#endregion SideRates


	#region TermDeposit
	public partial class TermDeposit : IProduct,ITermDeposit,IDeclarativeProvision
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_TermDeposit efs_TermDeposit;
		#endregion Members
		#region Accessors
		#region Currency
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string Currency
		{
            get { return this.principal.currency.Value; }
		}

		#endregion Currency
		#region InitialPayerReference
		public string InitialPayerReference
		{
			get { return initialPayerReference.href; }
		}
		#endregion InitialPayerReference
		#region InitialReceiverReference
		public string InitialReceiverReference
		{
			get { return initialReceiverReference.href; }
		}
		#endregion InitialReceiverReference
		
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal Notional
		{
			get
			{
				if (null != this.principal)
					return this.principal.amount;
				else
					return null;
            }
		}
		

		#endregion Accessors

		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region ITermDeposit Members
		EFS_Date ITermDeposit.startDate
		{
			set { this.startDate = value; }
			get { return this.startDate; }
		}
		EFS_Date ITermDeposit.maturityDate
		{
			set { this.maturityDate = value; }
			get { return this.maturityDate; }
		}
        // FI 20140909 [20340] fixedRate est une donnée obligatoire (voir FpML) 
        //bool ITermDeposit.fixedRateSpecified
        //{
        //    get { return this.fixedRateSpecified; }
        //}
		EFS_Decimal ITermDeposit.fixedRate
		{
			set { this.fixedRate = value; }
			get { return this.fixedRate; }
		}
		bool ITermDeposit.interestSpecified
		{
			set { this.interestSpecified = value; }
			get { return this.interestSpecified; }
		}
		IMoney ITermDeposit.interest { get { return this.interest; } }
		IMoney ITermDeposit.principal{get { return this.principal;}}
		IReference ITermDeposit.initialPayerReference
		{
			set { this.initialPayerReference = (PartyReference)value; }
			get { return this.initialPayerReference; }
		}
		IReference ITermDeposit.initialReceiverReference
		{
			set { this.initialReceiverReference = (PartyReference)value; }
			get { return this.initialReceiverReference; }
		}
		DayCountFractionEnum ITermDeposit.dayCountFraction
		{
			set { this.dayCountFraction = value; }
			get { return this.dayCountFraction; }
		}
		EFS_TermDeposit ITermDeposit.efs_TermDeposit
		{
			get { return this.efs_TermDeposit; }
			set { this.efs_TermDeposit = value; }
		}
		bool ITermDeposit.paymentSpecified 
		{ 
			set { this.paymentSpecified = value; } 
			get { return this.paymentSpecified; } 
		}
		IPayment[] ITermDeposit.payment { get { return this.payment; } }
		#endregion ITermDeposit Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return null; } }
		bool IDeclarativeProvision.extendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return null; } }
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return null; } }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion TermDeposit
}

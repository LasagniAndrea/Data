#region using directives
using EFS.ACommon;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.Fx;
using EfsML.v30.Shared;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Collections;
using System.Reflection;
#endregion using directives

namespace FpML.v44.Fx
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
		IQuotedCurrencyPair IExchangeRate.QuotedCurrencyPair { get { return this.quotedCurrencyPair; } }
		EFS_Decimal IExchangeRate.Rate 
		{ 
			set { this.rate = value; } 
			get { return this.rate; } 
		}
		bool IExchangeRate.SpotRateSpecified 
		{
			set { this.spotRateSpecified = value; } 
			get { return this.spotRateSpecified; } 
		}
		EFS_Decimal IExchangeRate.SpotRate
		{
			set { this.spotRate = value; }
			get {return this.spotRate;}
		}
		bool IExchangeRate.ForwardPointsSpecified 
		{
			set { this.forwardPointsSpecified = value; }
			get { return this.forwardPointsSpecified; }
		}
		EFS_Decimal IExchangeRate.ForwardPoints 
		{
			set { this.forwardPoints = value; }
			get { return this.forwardPoints; } 
		}
		bool IExchangeRate.FxFixingSpecified 
		{
			set { this.fxFixingSpecified = value; }
			get { return this.fxFixingSpecified; }
		}
		IFxFixing IExchangeRate.FxFixing 
		{ 
			set { this.fxFixing = (FxFixing)value; } 
			get { return this.fxFixing; } 
		}
		bool IExchangeRate.SideRatesSpecified 
        {
			set { this.sideRatesSpecified = value; } 
			get { return this.sideRatesSpecified; } 
		}
		ISideRates IExchangeRate.SideRates { get { return this.sideRates; } }
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
        EFS_Date IExpiryDateTime.ExpiryDate
		{
			set { this.expiryDate = value; }
			get { return this.expiryDate; }
		}
		DateTime IExpiryDateTime.ExpiryDateTimeValue
		{
			get { return this.ExpiryDateTimeValue; }
		}
		IHourMinuteTime IExpiryDateTime.ExpiryTime
		{
			set { this.expiryTime.hourMinuteTime = (HourMinuteTime)value; }
			get { return this.expiryTime.hourMinuteTime; }
		}
		string IExpiryDateTime.BusinessCenter 
		{ 
			set { this.expiryTime.businessCenter.Value = value; } 
			get { return this.expiryTime.businessCenter.Value; } 
		}
		bool IExpiryDateTime.CutNameSpecified 
		{
			set { this.cutNameSpecified = value; }
			get { return this.cutNameSpecified; }
		}
		string IExpiryDateTime.CutName 
		{
			set { this.cutName.Value = value;} 
			get { return this.cutName.Value;} 
		}
		IBusinessCenterTime IExpiryDateTime.BusinessCenterTime
		{
			get { return this.expiryTime; }
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
                decimal spotValue = 0;
                if (null != spotRate)
                    spotValue = spotRate.DecValue;
                EFS_TriggerRate trgRate = new EFS_TriggerRate(triggerRate.DecValue, spotValue, quotedCurrencyPair);
                return trgRate;
			}
		}
		#endregion TriggerRate
		#endregion Accessors
		#region Constructors
		public FxAmericanTrigger()
		{
			quotedCurrencyPair = new QuotedCurrencyPair();
			triggerRate = new EFS_Decimal();
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
		TouchConditionEnum IFxAmericanTrigger.TouchCondition 
		{ 
			set { this.touchCondition = value; } 
			get { return this.touchCondition; } 
		}
		bool IFxAmericanTrigger.ObservationStartDateSpecified
		{
			set { this.observationStartDateSpecified = value; }
			get { return this.observationStartDateSpecified; }
		}
		EFS_Date IFxAmericanTrigger.ObservationStartDate
		{
			set { this.observationStartDate = value; }
			get { return this.observationStartDate; }
		}
		bool IFxAmericanTrigger.ObservationEndDateSpecified
		{
			set { this.observationEndDateSpecified = value; }
			get { return this.observationEndDateSpecified; }
		}
		EFS_Date IFxAmericanTrigger.ObservationEndDate
		{
			set { this.observationEndDate = value; }
			get { return this.observationEndDate; }
		}
		#endregion IFxAmericanTrigger Members
		#region IFxTrigger Members
		EFS_Decimal IFxTrigger.TriggerRate
		{
			set { this.triggerRate = value; }
			get { return this.triggerRate; }
		}
		IQuotedCurrencyPair IFxTrigger.QuotedCurrencyPair
		{
			set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
			get { return this.quotedCurrencyPair; }
		}
		IInformationSource[] IFxTrigger.InformationSource
		{
			set { this.informationSource = (InformationSource[])value; }
			get { return this.informationSource; }
		}
		IInformationSource[] IFxTrigger.CreateInformationSources { get { return new InformationSource[1] { new InformationSource() }; } }
		#endregion IFxTrigger Members
	}
	#endregion FxAmericanTrigger
	#region FxAverageRateObservationDate
	public partial class FxAverageRateObservationDate : IEFS_Array, IFxAverageRateObservationDate
	{
		#region Constructors
		public FxAverageRateObservationDate() { }
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
		DateTime IFxAverageRateObservationDate.ObservationDate { get { return this.observationDate.DateValue; } }
		decimal IFxAverageRateObservationDate.AverageRateWeightingFactor { get { return this.averageRateWeightingFactor.DecValue; } }
		#endregion IFxAverageRateObservationDate Members
	}
	#endregion FxAverageRateObservationDate
	#region FxAverageRateObservationSchedule
	public partial class FxAverageRateObservationSchedule : IFxAverageRateObservationSchedule
	{
		#region IFxAverageRateObservationSchedule Members
		EFS_Date IFxAverageRateObservationSchedule.ObservationStartDate 
		{
			get { return this.observationStartDate; } 
		}
		EFS_Date IFxAverageRateObservationSchedule.ObservationEndDate 
		{
			get { return this.observationEndDate; } 
		}
		ICalculationPeriodFrequency IFxAverageRateObservationSchedule.CalculationPeriodFrequency
		{
			set { this.calculationPeriodFrequency = (CalculationPeriodFrequency)value; }
			get { return this.calculationPeriodFrequency; }
		}
		#endregion IFxAverageRateObservationSchedule Members

	}
	#endregion FxAverageRateObservationSchedule
	#region FxAverageRateOption
	public partial class FxAverageRateOption : IProduct,IFxAverageRateOption,IDeclarativeProvision
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
			get {return buyerPartyReference.href; }
		}
		#endregion BuyerPartyReference
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
		public FxAverageRateOption()
		{
			rateObservationSchedule = new FxAverageRateObservationSchedule();
			rateObservationDate = new FxAverageRateObservationDate[1] { new FxAverageRateObservationDate() };
			fxStrikePrice = new FxStrikePrice();
			bermudanExerciseDates = new DateList();
		}
		#endregion Constructors

		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members

		#region IFxAverageRateOption Members
		bool IFxAverageRateOption.SpotRateSpecified
		{
			set { this.spotRateSpecified = value; }
			get { return this.spotRateSpecified; }
		}
		EFS_Decimal IFxAverageRateOption.SpotRate
		{
			set { this.spotRate = value; }
			get { return this.spotRate; }
		}
		ICurrency IFxAverageRateOption.PayoutCurrency
		{
			set { this.payoutCurrency = (Currency)value; }
			get { return this.payoutCurrency; }
		}
		bool IFxAverageRateOption.PayoutFormulaSpecified
		{
			set { this.payoutFormulaSpecified = value; }
			get { return this.payoutFormulaSpecified; }
		}
		string IFxAverageRateOption.PayoutFormula
		{
			set { this.payoutFormula = new EFS_MultiLineString(value); }
			get { return this.payoutFormula.Value; }
		}
		IInformationSource IFxAverageRateOption.PrimaryRateSource
		{
			set { this.primaryRateSource = (InformationSource)value; }
			get { return this.primaryRateSource; }
		}
		StrikeQuoteBasisEnum IFxAverageRateOption.AverageRateQuoteBasis
		{
			set { this.averageRateQuoteBasis = value; }
			get { return this.averageRateQuoteBasis; }
		}
		bool IFxAverageRateOption.SecondaryRateSourceSpecified
		{
			set { this.secondaryRateSourceSpecified = value; }
			get { return this.secondaryRateSourceSpecified; }
		}
		IInformationSource IFxAverageRateOption.SecondaryRateSource
		{
			set { this.secondaryRateSource = (InformationSource)value; }
			get { return this.secondaryRateSource; }
		}
		IBusinessCenterTime IFxAverageRateOption.FixingTime
		{
			set { this.fixingTime = (BusinessCenterTime)value; }
			get { return this.fixingTime; }
		}
		bool IFxAverageRateOption.BermudanExerciseDatesSpecified { get { return this.bermudanExerciseDatesSpecified; } }
		IDateList IFxAverageRateOption.BermudanExerciseDates { get { return this.bermudanExerciseDates; } }
		bool IFxAverageRateOption.AverageStrikeOptionSpecified
		{
			set { this.averageStrikeOptionSpecified = value; }
			get { return this.averageStrikeOptionSpecified; }
		}
		IAverageStrikeOption IFxAverageRateOption.AverageStrikeOption 
		{
			set { this.averageStrikeOption = (AverageStrikeOption)value; } 
			get { return this.averageStrikeOption; } 
		}
		bool IFxAverageRateOption.ObservedRatesSpecified { get { return this.observedRatesSpecified; } }
		IObservedRates[] IFxAverageRateOption.ObservedRates { get { return this.observedRates; } }
		bool IFxAverageRateOption.RateObservationDateSpecified 
		{
			set { this.rateObservationDateSpecified = value; } 
			get { return this.rateObservationDateSpecified; } 
		}
		IFxAverageRateObservationDate[] IFxAverageRateOption.RateObservationDate 
		{ 
			set { this.rateObservationDate = (FxAverageRateObservationDate[])value; } 
			get { return this.rateObservationDate; } 
		}
		bool IFxAverageRateOption.RateObservationScheduleSpecified 
		{ 
			set { this.rateObservationScheduleSpecified = value; } 
			get { return this.rateObservationScheduleSpecified; } 
		}
		IFxAverageRateObservationSchedule IFxAverageRateOption.RateObservationSchedule 
		{
			set { this.rateObservationSchedule = (FxAverageRateObservationSchedule)value; } 
			get { return this.rateObservationSchedule; } 
		}
		bool IFxAverageRateOption.GeometricAverageSpecified
		{
			set { this.geometricAverageSpecified = value; }
			get { return this.geometricAverageSpecified; }
		}
		bool IFxAverageRateOption.PrecisionSpecified
		{
			set { this.precisionSpecified = value; }
			get { return this.precisionSpecified; }
		}
		EFS_PosInteger IFxAverageRateOption.Precision
		{
			set { this.precision = value; }
			get { return this.precision; }
		}
		IInformationSource IFxAverageRateOption.CreateInformationSource { get { return new InformationSource(); } }
		IFxAverageRateObservationSchedule IFxAverageRateOption.CreateFxAverageRateObservationSchedule { get { return new FxAverageRateObservationSchedule(); } }
		IFxAverageRateObservationDate[] IFxAverageRateOption.CreateFxAverageRateObservationDates { get { return new FxAverageRateObservationDate[1]; } }
		EFS_FxAverageRateOption IFxAverageRateOption.Efs_FxAverageRateOption
		{
			get { return this.efs_FxAverageRateOption; }
			set { this.efs_FxAverageRateOption = value; }
		}

		#endregion IFxAverageRateOption Members
		#region IFxOptionBase Membres
		IExpiryDateTime IFxOptionBase.ExpiryDateTime { get { return this.expiryDateTime; } }
		EFS_Date IFxOptionBase.ValueDate { get { return this.valueDate; } }

        IReference IFxOptionBase.BuyerPartyReference
        {
            get { return this.buyerPartyReference; }
            set { buyerPartyReference = (PartyOrTradeSideReference) value; } 
        }
        IReference IFxOptionBase.SellerPartyReference
        {
            get { return this.sellerPartyReference; }
            set { sellerPartyReference = (PartyOrTradeSideReference)value; } 
        }
		bool IFxOptionBase.OptionTypeSpecified 
		{ 
			set { this.optionTypeSpecified = value; } 
			get { return this.optionTypeSpecified; } 
		}
		OptionTypeEnum IFxOptionBase.OptionType 
		{
			set { this.optionType = value; } 
			get { return this.optionType; } 
		}
		bool IFxOptionBase.CallCurrencyAmountSpecified { get { return true; } }
		IMoney IFxOptionBase.CallCurrencyAmount { get { return this.callCurrencyAmount; } }
		bool IFxOptionBase.PutCurrencyAmountSpecified { get { return true; } }
		IMoney IFxOptionBase.PutCurrencyAmount { get { return this.putCurrencyAmount; } }
		bool IFxOptionBase.FxOptionPremiumSpecified
		{
			set { this.fxOptionPremiumSpecified = value; }
			get { return this.fxOptionPremiumSpecified; }
		}
		IFxOptionPremium[] IFxOptionBase.FxOptionPremium { get { return this.fxOptionPremium; } }
		#endregion IFxOptionBase Membres
		#region IFxOptionBaseNotDigital Membres
		ExerciseStyleEnum IFxOptionBaseNotDigital.ExerciseStyle { get { return this.exerciseStyle; } }
		bool IFxOptionBaseNotDigital.ProcedureSpecified { get { return this.procedureSpecified; } }
		IExerciseProcedure IFxOptionBaseNotDigital.Procedure { get { return this.procedure; } }
		IFxStrikePrice IFxOptionBaseNotDigital.FxStrikePrice { get { return this.fxStrikePrice; } }
		#endregion IFxOptionBaseNotDigital Membres
		#region IDeclarativeProvision Members
        // EG 20180514 [23812] Report
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
        bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return this.earlyTerminationProvisionSpecified; } }
        IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return this.earlyTerminationProvision; } }
        bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
        IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
        bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return implicitProvisionSpecified; } }
        IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return implicitProvision; } }
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
                decimal spotValue = 0;
                if (null != spotRate)
                    spotValue = spotRate.DecValue;
                EFS_TriggerRate trgRate = new EFS_TriggerRate(triggerRate.DecValue, spotValue, quotedCurrencyPair);
                return trgRate;
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
		#region IFxBarrier Members
		FxBarrierTypeEnum IFxBarrier.FxBarrierType 
		{
			set { this.fxBarrierType = value;} 
			get { return this.fxBarrierType;} 
		}
		bool IFxBarrier.FxBarrierTypeSpecified 
		{ 
			set { this.fxBarrierTypeSpecified = value; } 
			get { return this.fxBarrierTypeSpecified; } 
		}
		IInformationSource[] IFxBarrier.InformationSource
		{
			set { this.informationSource = (InformationSource[])value; }
			get { return this.informationSource; }
		}
		EFS_Decimal IFxBarrier.TriggerRate
		{
			set { this.triggerRate = value; }
			get { return this.triggerRate; }
		}
		bool IFxBarrier.ObservationStartDateSpecified
		{
			set { this.observationStartDateSpecified = value; }
			get { return this.observationStartDateSpecified; }
		}
		EFS_Date IFxBarrier.ObservationStartDate
		{
			set { this.observationStartDate = value; }
			get { return this.observationStartDate; }
		}
		bool IFxBarrier.ObservationEndDateSpecified
		{
			set { this.observationEndDateSpecified = value; }
			get { return this.observationEndDateSpecified; }
		}
		EFS_Date IFxBarrier.ObservationEndDate
		{
			set { this.observationEndDate = value; }
			get { return this.observationEndDate; }
		}
		IQuotedCurrencyPair IFxBarrier.QuotedCurrencyPair
		{
			set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
			get { return this.quotedCurrencyPair; }
		}
		IInformationSource[] IFxBarrier.CreateInformationSources { get { return new InformationSource[1] { new InformationSource() }; } }
		#endregion IFxBarrier Members

	}
	#endregion FxBarrier
	#region FxBarrierOption
	public partial class FxBarrierOption : IProduct,IFxBarrierOption,IDeclarativeProvision
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
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IFxBarrierOption Members
		bool IFxBarrierOption.BermudanExerciseDatesSpecified { get { return this.bermudanExerciseDatesSpecified; } }
		IDateList IFxBarrierOption.BermudanExerciseDates { get { return this.bermudanExerciseDates; } }
		bool IFxBarrierOption.CashSettlementTermsSpecified { get { return this.cashSettlementTermsSpecified; } }
        IFxCashSettlement IFxBarrierOption.CashSettlementTerms 
        {
            set { this.cashSettlementTerms = (FxCashSettlement)value; }
            get { return this.cashSettlementTerms; } 
        }
		bool IFxBarrierOption.SpotRateSpecified 
		{ 
			set { this.spotRateSpecified = value; } 
			get { return this.spotRateSpecified; } 
		}
		EFS_Decimal IFxBarrierOption.SpotRate 
		{ 
			set { this.spotRate = value; } 
			get { return this.spotRate; } 
		}
		IFxBarrier[] IFxBarrierOption.FxBarrier { get { return this.fxBarrier; } }
		bool IFxBarrierOption.FxRebateBarrierSpecified 
		{ 
			set { this.fxRebateBarrierSpecified = value; } 
			get { return this.fxRebateBarrierSpecified; } 
		}
		IFxBarrier IFxBarrierOption.FxRebateBarrier { get { return this.fxRebateBarrier; } }
		bool IFxBarrierOption.CappedCallOrFlooredPutSpecified { get { return this.cappedCallOrFlooredPutSpecified; } }
		ICappedCallOrFlooredPut IFxBarrierOption.CappedCallOrFlooredPut { get { return this.cappedCallOrFlooredPut; } }
		bool IFxBarrierOption.TriggerPayoutSpecified 
		{ 
			set { this.triggerPayoutSpecified = value; } 
			get { return this.triggerPayoutSpecified; } 
		}
		IFxOptionPayout IFxBarrierOption.TriggerPayout { get { return this.triggerPayout; } }
		EFS_FxBarrierOption IFxBarrierOption.Efs_FxBarrierOption
		{
			get { return this.efs_FxBarrierOption; }
			set { this.efs_FxBarrierOption = value; }
		}
		#endregion IFxBarrierOption Members
		#region IFxOptionBase Membres
		IExpiryDateTime IFxOptionBase.ExpiryDateTime { get { return this.expiryDateTime; } }
		EFS_Date IFxOptionBase.ValueDate { get { return this.valueDate; } }

        IReference IFxOptionBase.BuyerPartyReference
        {
            get { return this.buyerPartyReference; }
            set { buyerPartyReference = (PartyOrTradeSideReference)value; }
        }
        IReference IFxOptionBase.SellerPartyReference
        {
            get { return this.sellerPartyReference; }
            set { sellerPartyReference = (PartyOrTradeSideReference)value; }
        }
		bool IFxOptionBase.OptionTypeSpecified 
		{ 
			set { this.optionTypeSpecified = value; } 
			get { return this.optionTypeSpecified; } 
		}
		OptionTypeEnum IFxOptionBase.OptionType 
		{
			set { this.optionType = value; } 
			get { return this.optionType; } 
		}
		bool IFxOptionBase.CallCurrencyAmountSpecified { get { return true; } }
		IMoney IFxOptionBase.CallCurrencyAmount { get { return this.callCurrencyAmount; } }
		bool IFxOptionBase.PutCurrencyAmountSpecified { get { return true; } }
		IMoney IFxOptionBase.PutCurrencyAmount { get { return this.putCurrencyAmount; } }
		bool IFxOptionBase.FxOptionPremiumSpecified
		{
			set { this.fxOptionPremiumSpecified = value; }
			get { return this.fxOptionPremiumSpecified; }
		}
		IFxOptionPremium[] IFxOptionBase.FxOptionPremium { get { return this.fxOptionPremium; } }
		#endregion IFxOptionBase Membres
		#region IFxOptionBaseNotDigital Membres
		ExerciseStyleEnum IFxOptionBaseNotDigital.ExerciseStyle { get { return this.exerciseStyle; } }
		bool IFxOptionBaseNotDigital.ProcedureSpecified { get { return this.procedureSpecified; } }
		IExerciseProcedure IFxOptionBaseNotDigital.Procedure { get { return this.procedure; } }
		IFxStrikePrice IFxOptionBaseNotDigital.FxStrikePrice { get { return this.fxStrikePrice; } }
		#endregion IFxOptionBaseNotDigital Membres
		#region IDeclarativeProvision Members
        // EG 20180514 [23812] Report
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
        bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return this.earlyTerminationProvisionSpecified; } }
        IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return this.earlyTerminationProvision; } }
        bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
        IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
        bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return this.implicitProvisionSpecified; } }
        IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return this.implicitProvision; } }
        #endregion IDeclarativeProvision Members
	}
	#endregion FxBarrierOption
	#region FxDigitalOption
	public partial class FxDigitalOption : IProduct,IFxDigitalOption,IDeclarativeProvision
	{
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
		IQuotedCurrencyPair IFxDigitalOption.QuotedCurrencyPair { get { return this.quotedCurrencyPair; } }
		bool IFxDigitalOption.TypeTriggerEuropeanSpecified 
		{
			set { this.typeTriggerEuropeanSpecified = value; } 
			get { return this.typeTriggerEuropeanSpecified; } 
		}
		IFxEuropeanTrigger[] IFxDigitalOption.TypeTriggerEuropean { get { return this.typeTriggerEuropean; } }
		bool IFxDigitalOption.TypeTriggerAmericanSpecified 
		{
			set { this.typeTriggerAmericanSpecified = value; } 
			get { return this.typeTriggerAmericanSpecified; } 
		}
		IFxAmericanTrigger[] IFxDigitalOption.TypeTriggerAmerican { get { return this.typeTriggerAmerican; } }
		IFxOptionPayout IFxDigitalOption.TriggerPayout { get { return this.triggerPayout; } }
		bool IFxDigitalOption.SpotRateSpecified 
		{
			set { this.spotRateSpecified = value; } 
			get { return this.spotRateSpecified; } 
		}
		EFS_Decimal IFxDigitalOption.SpotRate 
		{ 
			set { this.spotRate = value; } 
			get { return this.spotRate; } 
		}
		bool IFxDigitalOption.ResurrectingSpecified { get { return this.resurrectingSpecified; } }
		IPayoutPeriod IFxDigitalOption.Resurrecting { get { return this.resurrecting; } }
		bool IFxDigitalOption.ExtinguishingSpecified { get { return this.extinguishingSpecified; } }
		IPayoutPeriod IFxDigitalOption.Extinguishing { get { return this.extinguishing; } }
		bool IFxDigitalOption.FxBarrierSpecified 
		{
			set { this.fxBarrierSpecified = value;}
			get { return this.fxBarrierSpecified;}
		}
		IFxBarrier[] IFxDigitalOption.FxBarrier { get { return this.fxBarrier; } }
		bool IFxDigitalOption.BoundarySpecified { get { return this.boundarySpecified; } }
		bool IFxDigitalOption.LimitSpecified { get { return this.limitSpecified; } }
		bool IFxDigitalOption.AssetOrNothingSpecified { get { return this.assetOrNothingSpecified; } }
		IAssetOrNothing IFxDigitalOption.AssetOrNothing { get { return this.assetOrNothing; } }
		EFS_EventDate IFxDigitalOption.ExpiryDate { get { return this.ExpiryDate; } }
		EFS_FxDigitalOption IFxDigitalOption.Efs_FxDigitalOption
		{
			get { return this.efs_FxDigitalOption; }
			set { this.efs_FxDigitalOption = value; }
		}

		#endregion IFxDigitalOption Members
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IFxOptionBase Membres
		IExpiryDateTime IFxOptionBase.ExpiryDateTime { get { return this.expiryDateTime; } }
		EFS_Date IFxOptionBase.ValueDate { get { return this.valueDate; } }

        IReference IFxOptionBase.BuyerPartyReference
        {
            get { return this.buyerPartyReference; }
            set { buyerPartyReference = (PartyOrTradeSideReference)value; }
        }
        IReference IFxOptionBase.SellerPartyReference
        {
            get { return this.sellerPartyReference; }
            set { sellerPartyReference = (PartyOrTradeSideReference)value; } 
        }
		bool IFxOptionBase.OptionTypeSpecified
		{
			set { ; }
			get { return false; }
		}
		OptionTypeEnum IFxOptionBase.OptionType
		{
			set { ; }
			get { return OptionTypeEnum.Call; }
		}
		bool IFxOptionBase.CallCurrencyAmountSpecified { get { return false; } }
		IMoney IFxOptionBase.CallCurrencyAmount { get { return null; } }
		bool IFxOptionBase.PutCurrencyAmountSpecified { get { return false; } }
		IMoney IFxOptionBase.PutCurrencyAmount { get { return null; } }
		bool IFxOptionBase.FxOptionPremiumSpecified
		{
			set { this.fxOptionPremiumSpecified = value; }
			get { return this.fxOptionPremiumSpecified; }
		}
		IFxOptionPremium[] IFxOptionBase.FxOptionPremium { get { return this.fxOptionPremium; } }
		#endregion IFxOptionBase Membres
		#region IDeclarativeProvision Members
        // EG 20180514 [23812] Report
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
        bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return this.earlyTerminationProvisionSpecified; } }
        IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return this.earlyTerminationProvision; } }
        bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
        IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
        bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return this.implicitProvisionSpecified; } }
        IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return this.implicitProvision; } }
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
                decimal spotValue = 0;
                if (null != spotRate)
                    spotValue = spotRate.DecValue;
                EFS_TriggerRate trgRate = new EFS_TriggerRate(triggerRate.DecValue, spotValue, quotedCurrencyPair);
				return trgRate;
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
		TriggerConditionEnum IFxEuropeanTrigger.TriggerCondition
		{
			set { this.triggerCondition = value; }
			get { return this.triggerCondition; }
		}
		#endregion IFxEuropeanTrigger Members
		#region IFxTrigger Members
		EFS_Decimal IFxTrigger.TriggerRate
		{
			set { this.triggerRate = value; }
			get { return this.triggerRate; }
		}
		IQuotedCurrencyPair IFxTrigger.QuotedCurrencyPair
		{
			set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
			get { return this.quotedCurrencyPair; }
		}
		IInformationSource[] IFxTrigger.InformationSource
		{
			set	{this.informationSource = (InformationSource[])value;}
			get	{return this.informationSource;}
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
    /// FI 20161114 [RATP] Modify
	public partial class FxLeg : IEFS_Array,IProduct,IFxLeg,IDeclarativeProvision
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:FxLeg";
        #endregion Members

        #region accessors
        #region ValueDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ValueDate
        {
            get
            {
                if (null != efs_FxLeg)
                    return efs_FxLeg.ValueDate;
                else
                    return null;
            }
        }
        #endregion ValueDate

        #region InitialMarginPayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginPayerPartyReference
        {
            get
            {
                string hRef = string.Empty;
                if (null != efs_FxLeg)
                    hRef = efs_FxLeg.InitialMarginPayerPartyReference;
                return hRef;
            }
        }
        #endregion InitialMarginPayerPartyReference
        #region InitialMarginReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginReceiverPartyReference
        {
            get
            {
                string hRef = string.Empty;
                if (null != efs_FxLeg)
                    hRef = efs_FxLeg.InitialMarginReceiverPartyReference;
                return hRef;
            }
        }
        #endregion InitialMarginReceiverPartyReference

        #endregion Accessors
        #region Constructors
        public FxLeg()
        {
            fxDateValueDate = new EFS_Date();
            fxDateCurrency1ValueDate = new EFS_Date();
            fxDateCurrency2ValueDate = new EFS_Date();
            marginRatio = new MarginRatio();
            // FI 20161114 [RATP] new instance for exchangedCurrency1 et exchangedCurrency2
            exchangedCurrency1 = new Payment();
            exchangedCurrency2 = new Payment();
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
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IFxLeg Members
		IPayment IFxLeg.ExchangedCurrency1 
		{ 
			set { this.exchangedCurrency1 = (Payment)value; } 
			get { return this.exchangedCurrency1; } 
		}
		IPayment IFxLeg.ExchangedCurrency2 
		{ 
			set { this.exchangedCurrency2 = (Payment)value; } 
			get { return this.exchangedCurrency2; } 
		}
		IExchangeRate IFxLeg.ExchangeRate 
		{ 
			set { this.exchangeRate = (ExchangeRate)value; } 
			get { return this.exchangeRate; } 
		}
		bool IFxLeg.FxDateValueDateSpecified 
		{ 
			set { this.fxDateValueDateSpecified = value; } 
			get { return this.fxDateValueDateSpecified; } 
		}
		EFS_Date IFxLeg.FxDateValueDate { get { return this.fxDateValueDate; } }
		bool IFxLeg.FxDateCurrency1ValueDateSpecified 
		{ 
			set { this.fxDateCurrency1ValueDateSpecified = value; } 
			get { return this.fxDateCurrency1ValueDateSpecified; } 
		}
		EFS_Date IFxLeg.FxDateCurrency1ValueDate { get { return this.fxDateCurrency1ValueDate; } }
		bool IFxLeg.FxDateCurrency2ValueDateSpecified 
		{ 
			set { this.fxDateCurrency2ValueDateSpecified = value; } 
			get { return this.fxDateCurrency2ValueDateSpecified; } 
		}
		EFS_Date IFxLeg.FxDateCurrency2ValueDate { get { return this.fxDateCurrency2ValueDate; } }
		bool IFxLeg.NonDeliverableForwardSpecified 
		{ 
			set { this.nonDeliverableForwardSpecified = value; } 
			get { return this.nonDeliverableForwardSpecified; } 
		}
		IFxCashSettlement IFxLeg.NonDeliverableForward { get { return this.nonDeliverableForward; } }
		EFS_FxLeg IFxLeg.Efs_FxLeg 
		{ 
			get { return this.efs_FxLeg; } 
			set { this.efs_FxLeg = value; } 
		}
		IPayment IFxLeg.CreatePayment { get { return new Payment(); } }
		IExchangeRate IFxLeg.CreateExchangeRate { get { return new ExchangeRate(); } }

        /// EG 20150402 [POC] Add
        bool IFxLeg.MarginRatioSpecified
        {
            set { this.marginRatioSpecified = value; }
            get { return this.marginRatioSpecified; }
        }
        /// EG 20150402 [POC] add
        IMarginRatio IFxLeg.MarginRatio
        {
            set { this.marginRatio = (MarginRatio)value; }
            get { return this.marginRatio; }
        }
        /// EG 20150402 [POC] add
        IMarginRatio IFxLeg.CreateMarginRatio
        {
            get {return new MarginRatio();}
        }


        /// <summary>
        /// 
        /// </summary>
        /// FI 20150331 [XXPOC] Add
        /// FI 20170116 [21916] RptSide (R majuscule)
        FixML.Interface.IFixTrdCapRptSideGrp[] IFxLeg.RptSide
        {
            set { RptSide = (FixML.v50SP1.TrdCapRptSideGrp_Block[])value; }
            get { return RptSide; }
        }

		#endregion IFxLeg Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion FxLeg
	#region FxOptionLeg
    /// FI 20150331 [POC] Modify
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
		public string BuyerPartyReference
		{
			get
			{
				 return buyerPartyReference.href; 
			}
		}
		#endregion BuyerPartyReference
		#region SellerPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public string SellerPartyReference
		{
			get
			{
				 return sellerPartyReference.href; 
			}
		}
		#endregion SellerPartyReference
        #region InitialMarginPayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginPayerPartyReference
        {
            get
            {
                string hRef = string.Empty;
                if (null != efs_FxSimpleOption)
                    hRef = efs_FxSimpleOption.InitialMarginPayerPartyReference;
                return hRef;
            }
        }
        #endregion InitialMarginPayerPartyReference
        #region InitialMarginReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginReceiverPartyReference
        {
            get
            {
                string hRef = string.Empty;
                if (null != efs_FxSimpleOption)
                    hRef = efs_FxSimpleOption.InitialMarginReceiverPartyReference;
                return hRef;
            }
        }
        #endregion InitialMarginReceiverPartyReference
		#endregion Accessors
		#region Constructors
		public FxOptionLeg()
		{
			fxStrikePrice			= new FxStrikePrice();
			bermudanExerciseDates	= new DateList();
			procedure				= new ExerciseProcedure();
            marginRatio = new MarginRatio();
		}
		#endregion Constructors

		#region IFxOptionLeg Membres
        bool IFxOptionLeg.BermudanExerciseDatesSpecified
        {
            get { return this.bermudanExerciseDatesSpecified; }
            set { this.bermudanExerciseDatesSpecified = value; } 
        }
        IDateList IFxOptionLeg.BermudanExerciseDates
        {
            get { return this.bermudanExerciseDates; }
            set { this.bermudanExerciseDates = (DateList)value; }  
        }
        bool IFxOptionLeg.CashSettlementTermsSpecified
        {
            get { return this.cashSettlementTermsSpecified; }
            set { this.cashSettlementTermsSpecified = value; } 
        }

        IFxCashSettlement IFxOptionLeg.CashSettlementTerms
        {
            get { return this.cashSettlementTerms; }
            set { this.cashSettlementTerms = (FxCashSettlement)value; }
        }

		EFS_FxSimpleOption IFxOptionLeg.Efs_FxSimpleOption
		{
			get { return this.efs_FxSimpleOption; }
			set { this.efs_FxSimpleOption = value; }
		}
        /// FI 20150331 [POC] add
        FixML.Interface.IFixTrdCapRptSideGrp[] IFxOptionLeg.RptSide
        {
            set { RptSide = (FixML.v50SP1.TrdCapRptSideGrp_Block[])value; }
            get { return RptSide; }
        }
        /// EG 20150402 [POC] Add
        bool IFxOptionLeg.MarginRatioSpecified
        {
            set { this.marginRatioSpecified = value; }
            get { return this.marginRatioSpecified; }
        }
        /// EG 20150402 [POC] add
        IMarginRatio IFxOptionLeg.MarginRatio
        {
            set { this.marginRatio = (MarginRatio)value; }
            get { return this.marginRatio; }
        }
        /// EG 20150402 [POC] add
        IMarginRatio IFxOptionLeg.CreateMarginRatio
        {
            get { return new MarginRatio(); }
        }
		#endregion IFxOptionLeg Membres
		#region IFxOptionBase Membres
		IExpiryDateTime IFxOptionBase.ExpiryDateTime { get { return this.expiryDateTime; } }
		EFS_Date IFxOptionBase.ValueDate { get { return this.valueDate; } }

        IReference IFxOptionBase.BuyerPartyReference
        {
            get { return this.buyerPartyReference; }
            set { buyerPartyReference = (PartyOrTradeSideReference) value; }
        }
        IReference IFxOptionBase.SellerPartyReference
        {
            get { return this.sellerPartyReference; }
            set { sellerPartyReference = (PartyOrTradeSideReference)value; }
        }
		bool IFxOptionBase.OptionTypeSpecified
		{
			set { this.optionTypeSpecified = value; }
			get { return this.optionTypeSpecified; }
		}
		OptionTypeEnum IFxOptionBase.OptionType
		{
			set { this.optionType = value; }
			get { return this.optionType; }
		}
		bool IFxOptionBase.CallCurrencyAmountSpecified { get { return true; } }
		IMoney IFxOptionBase.CallCurrencyAmount { get { return this.callCurrencyAmount; } }
		bool IFxOptionBase.PutCurrencyAmountSpecified { get { return true; } }
		IMoney IFxOptionBase.PutCurrencyAmount { get { return this.putCurrencyAmount; } }
		bool IFxOptionBase.FxOptionPremiumSpecified
		{
			set { this.fxOptionPremiumSpecified = value; }
			get { return this.fxOptionPremiumSpecified; }
		}
		IFxOptionPremium[] IFxOptionBase.FxOptionPremium { get { return this.fxOptionPremium; } }
		#endregion IFxOptionBase Membres
		#region IFxOptionBaseNotDigital Membres
		ExerciseStyleEnum IFxOptionBaseNotDigital.ExerciseStyle { get { return this.exerciseStyle; } }
		bool IFxOptionBaseNotDigital.ProcedureSpecified { get { return this.procedureSpecified; } }
		IExerciseProcedure IFxOptionBaseNotDigital.Procedure { get { return this.procedure; } }
		IFxStrikePrice IFxOptionBaseNotDigital.FxStrikePrice { get { return this.fxStrikePrice; } }
		#endregion IFxOptionBaseNotDigital Membres
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IDeclarativeProvision Members
        // EG 20180514 [23812] Report
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
        bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return this.earlyTerminationProvisionSpecified; } }
        IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return this.earlyTerminationProvision; } }
        bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
        IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
        bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return this.implicitProvisionSpecified; } }
        IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return this.implicitProvision; } }
        #endregion IDeclarativeProvision Members
	}
	#endregion FxOptionLeg
	#region FxOptionPayout
	public partial class FxOptionPayout : IFxOptionPayout
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:FxOptionPayout";
        #endregion Members
        #region Accessors
        #region PayoutAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal PayoutAmount
		{
			get {return amount; }
		}
		#endregion PayoutAmount
		#region PayoutCurrency
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PayoutCurrency
		{
			get {return currency.Value; }
		}
		#endregion PayoutCurrency
		#endregion Accessors

		#region IFxOptionPayout Members
		PayoutEnum IFxOptionPayout.PayoutStyle 
		{ 
			set { this.payoutStyle = value; } 
			get { return this.payoutStyle; } 
		}
		EFS_Decimal IFxOptionPayout.Amount
		{
			get {return this.amount;}
		}
		string IFxOptionPayout.Currency 
		{ 
			set { this.currency.Value = value; } 
			get { return this.currency.Value; } 
		}
		bool IFxOptionPayout.SettlementInformationSpecified
		{
			set { this.settlementInformationSpecified = value; }
			get { return this.settlementInformationSpecified; }
		}
		ISettlementInformation IFxOptionPayout.SettlementInformation
		{
			get { return this.settlementInformation;}
		}
        bool IFxOptionPayout.CustomerSettlementPayoutSpecified
        {
            set { this.customerSettlementPayoutSpecified = value; }
            get { return this.customerSettlementPayoutSpecified; }
        }
        ICustomerSettlementPayment IFxOptionPayout.CustomerSettlementPayout
        {
            set { this.customerSettlementPayout = (CustomerSettlementPayment)value; }
            get { return this.customerSettlementPayout; }
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
			get{return efs_FxOptionPremium.SettlementDate; }
		}
		#endregion SettlementDate
		#region ReceiverPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string ReceiverPartyReference
		{
			get {return efs_FxOptionPremium.ReceiverPartyReference; }
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
        IReference IFxOptionPremium.PayerPartyReference
        {
            get { return this.payerPartyReference; }
            set { this.payerPartyReference = (PartyOrAccountReference)value; }
        }
        IReference IFxOptionPremium.ReceiverPartyReference
        {
            get { return this.receiverPartyReference; }
            set { this.receiverPartyReference = (PartyOrAccountReference)value; }
        }
		EFS_Date IFxOptionPremium.PremiumSettlementDate 
		{ 
			set { this.premiumSettlementDate = value; } 
			get { return this.premiumSettlementDate; } 
		}
		bool IFxOptionPremium.CustomerSettlementPremiumSpecified 
		{
			set { this.customerSettlementPremiumSpecified = value; } 
			get { return this.customerSettlementPremiumSpecified; } 
		}
		ICustomerSettlementPayment IFxOptionPremium.CustomerSettlementPremium 
		{ 
			set { this.customerSettlementPremium = (CustomerSettlementPayment)value; } 
			get { return this.customerSettlementPremium; } 
		}
        IMoney IFxOptionPremium.PremiumAmount
        {
            get { return this.premiumAmount; }
            set { this.premiumAmount = (Money)value; }
        }

		bool IFxOptionPremium.PremiumQuoteSpecified 
		{ 
			set { this.premiumQuoteSpecified = value; } 
			get { return this.premiumQuoteSpecified; } 
		}
		IPremiumQuote IFxOptionPremium.PremiumQuote { get { return this.premiumQuote; } }
		EFS_FxOptionPremium IFxOptionPremium.Efs_FxOptionPremium
		{
			get { return this.efs_FxOptionPremium; }
			set { this.efs_FxOptionPremium = value; }
		}
		IMoney IFxOptionPremium.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
		bool IFxOptionPremium.SettlementInformationSpecified
		{
			set { this.settlementInformationSpecified = value; }
			get { return this.settlementInformationSpecified; }
		}
		ISettlementInformation IFxOptionPremium.SettlementInformation
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
		public FxStrikePrice() { }
		#endregion Constructors

		#region IFxStrikePrice Members
		EFS_Decimal IFxStrikePrice.Rate
		{
			set { this.rate = value; }
			get { return this.rate; }
		}
		StrikeQuoteBasisEnum IFxStrikePrice.StrikeQuoteBasis 
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
		public EFS_FxLeg[] Efs_FxLeg
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
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IFxSwap Members
		IFxLeg[] IFxSwap.FxSingleLeg { get { return this.fxSingleLeg; } }
		#endregion IFxSwap Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion FxSwap

	#region ObservedRates
	public partial class ObservedRates : IEFS_Array,IObservedRates
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
		DateTime IObservedRates.ObservationDate { get { return this.observationDate.DateValue;}}
		decimal IObservedRates.ObservedRate { get { return this.observedRate.DecValue; } }
		#endregion IObservedRates Members
	}
	#endregion ObservedRates

	#region PremiumQuote
	public partial class PremiumQuote : IPremiumQuote
	{
		#region IPremiumQuote Members
		EFS_Decimal IPremiumQuote.PremiumValue
		{
			set { this.premiumValue = value; }
			get { return this.premiumValue; }
		}
		PremiumQuoteBasisEnum IPremiumQuote.PremiumQuoteBasis
		{
			set { this.premiumQuoteBasis = value;} 
			get { return this.premiumQuoteBasis;} 
		}
		#endregion IPremiumQuote Members
	}
	#endregion PremiumQuote

	#region SideRate
	public partial class SideRate : ICloneable, ISideRate
	{
		#region ICloneable Members
		#region Clone
		public object Clone()
		{
            SideRate clone = new SideRate
            {
                currency = (Currency)this.currency.Clone(),
                sideRateBasis = this.sideRateBasis,
                rate = (EFS_Decimal)this.rate.Clone(),
                spotRateSpecified = this.spotRateSpecified,
                forwardPointsSpecified = this.forwardPointsSpecified
            };
            if (clone.spotRateSpecified)
				clone.spotRate = (EFS_Decimal)this.spotRate.Clone();
			if (clone.forwardPointsSpecified)
				clone.forwardPoints = (EFS_Decimal)this.forwardPoints.Clone();
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region ISideRate Members
		string ISideRate.Currency{get { return this.currency.Value; }}
		SideRateBasisEnum ISideRate.SideRateBasis
		{
			set { this.sideRateBasis = value; }
			get { return this.sideRateBasis; }
		}
		EFS_Decimal ISideRate.Rate{get { return this.rate; }}
		bool ISideRate.SpotRateSpecified{get { return this.spotRateSpecified; }}
		EFS_Decimal ISideRate.SpotRate { get { return this.spotRate; } }
		bool ISideRate.ForwardPointsSpecified{get { return this.forwardPointsSpecified; }}
		EFS_Decimal ISideRate.ForwardPoints { get { return this.forwardPoints; } }
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
		ICurrency ISideRates.BaseCurrency { get { return this.baseCurrency; } }
		bool ISideRates.Currency1SideRateSpecified { get { return this.currency1SideRateSpecified; } }
		ISideRate ISideRates.Currency1SideRate { get { return this.currency1SideRate; } }
		bool ISideRates.Currency2SideRateSpecified { get { return this.currency2SideRateSpecified; } }
		ISideRate ISideRates.Currency2SideRate { get { return this.currency2SideRate; } }
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
		/// <summary>
		/// 
		/// </summary>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public string Currency
		{
            get
            {
                return this.principal.currency.Value;
            }
		}
		
		/// <summary>
		/// 
		/// </summary>
		public string InitialPayerReference
		{
			get { return initialPayerReference.href; }
		}
		
		/// <summary>
		/// 
		/// </summary>
		public string InitialReceiverReference
		{
			get { return initialReceiverReference.href; }
		}

		/// <summary>
		/// 
		/// </summary>
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
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region ITermDeposit Members
		EFS_Date ITermDeposit.StartDate 
		{ 
			set { this.startDate = value; } 
			get { return this.startDate; } 
		}
		EFS_Date ITermDeposit.MaturityDate 
		{ 
			set { this.maturityDate = value; } 
			get { return this.maturityDate; } 
		}
        // FI 20140909 [20340] fixedRate est une donnée obligatoire (voir FpML) 
        //bool ITermDeposit.fixedRateSpecified 
        //{ 
        //    get { return this.fixedRateSpecified; } 
        //}
		EFS_Decimal ITermDeposit.FixedRate 
		{ 
			set { this.fixedRate = value; } 
			get { return this.fixedRate; } 
		}
		bool ITermDeposit.InterestSpecified 
		{ 
			set { this.interestSpecified = value; } 
			get { return this.interestSpecified; } 
		}
		IMoney ITermDeposit.Interest 
		{
			get { return this.interest; } 
		}
		IMoney ITermDeposit.Principal 
		{ 
			get { return this.principal; } 
		}
		IReference ITermDeposit.InitialPayerReference 
		{ 
			set { this.initialPayerReference =(PartyReference) value; } 
			get { return this.initialPayerReference; } 
		}
		IReference ITermDeposit.InitialReceiverReference 
		{
			set { this.initialReceiverReference = (PartyReference)value; }
			get { return this.initialReceiverReference; } 
		}
		DayCountFractionEnum ITermDeposit.DayCountFraction 
		{ 
			set { this.dayCountFraction = value; } 
			get { return this.dayCountFraction; } 
		}
		EFS_TermDeposit ITermDeposit.Efs_TermDeposit
		{
			get { return this.efs_TermDeposit; }
			set { this.efs_TermDeposit = value; }
		}
		bool ITermDeposit.PaymentSpecified 
		{ 
			set { this.paymentSpecified = value; } 
			get { return this.paymentSpecified; } 
		}
        IPayment[] ITermDeposit.Payment
        {
            get { return this.payment; }
        }
		#endregion ITermDeposit Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion TermDeposit

}

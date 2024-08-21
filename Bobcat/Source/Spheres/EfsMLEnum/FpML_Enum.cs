#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;
#endregion using directives

namespace FpML.Enum
{
	#region Common Enums 4.2 & 4.4
	#region AveragingInOutEnum
	public enum AveragingInOutEnum
	{
		In,
		Out,
		Both,
	}
	#endregion AveragingInOutEnum
	#region AveragingMethodEnum
	public enum AveragingMethodEnum
	{
		Unweighted,
		Weighted,
	}
    #endregion AveragingMethodEnum

    #region BusinessDayConventionEnum
    /// <summary>
    /// The convention for adjusting any relevant date if it would otherwise fall on a day that is not a valid business day. Note that FRN is included here as a type of business day convention although it does not strictly fall within ISDA's definition of a Business Day Convention and does not conform to the simple definition given above.
    /// </summary>
    public enum BusinessDayConventionEnum
	{
        /// <summary>
        /// The non-business date will be adjusted to the first following day that is a business day
        /// </summary>
        FOLLOWING,
        /// <summary>
        /// Per 2000 ISDA Definitions, Section 4.11. FRN Convention; Eurodollar Convention.
        /// </summary>
		FRN,
        /// <summary>
        /// The non-business date will be adjusted to the first following day that is a business day unless that day falls in the next calendar month, in which case that date will be the first preceding day that is a business day
        /// </summary>
        MODFOLLOWING,
        /// <summary>
        /// The non-business day will be adjusted to the first preceding day that is a business day.
        /// </summary>
		PRECEDING,
        /// <summary>
        /// The non-business date will be adjusted to the first preceding day that is a business day unless that day falls in the previous calendar month, in which case that date will be the first following day that us a business day.
        /// </summary>
        MODPRECEDING,
        /// <summary>
        /// The date will not be adjusted if it falls on a day that is not a business day.
        /// </summary>
		NONE,
        /// <summary>
        /// The date adjustments conventions are defined elsewhere, so it is not required to specify them here.
        /// </summary>
		NotApplicable,
	}
	#endregion BusinessDayConventionEnum

	#region CalculationAgentPartyEnum
	public enum CalculationAgentPartyEnum
	{
		ExercisingParty,
		NonExercisingParty,
		AsSpecifiedInMasterAgreement,
	}
	#endregion CalculationAgentPartyEnum
	#region CommissionDenominationEnum
	public enum CommissionDenominationEnum
	{
		BPS,
		Percentage,
		CentsPerShare,
		FixedAmount,
	}
	#endregion CommissionDenominationEnum
	#region CompoundingMethodEnum
	public enum CompoundingMethodEnum
	{
		Flat,
		None,
		Straight,
	}
	#endregion CompoundingMethodEnum
    #region CouponTypeEnum
    public enum CouponTypeEnum
    {
        Fixed,
        Float,
        Struct,
    }
    #endregion CouponTypeEnum


    #region DayCountFractionEnum
    public enum DayCountFractionEnum
	{
		//FpML DCF
		[System.Xml.Serialization.XmlEnumAttribute("1/1")]
		DCF11,
		[System.Xml.Serialization.XmlEnumAttribute("ACT/ACT.ISDA")]
		ACTACTISDA,
		[System.Xml.Serialization.XmlEnumAttribute("ACT/ACT.ISMA")]
		ACTACTISMA,
		[System.Xml.Serialization.XmlEnumAttribute("ACT/ACT.AFB")]
		ACTACTAFB,
		[System.Xml.Serialization.XmlEnumAttribute("ACT/365.FIXED")]
		ACT365FIXED,
		[System.Xml.Serialization.XmlEnumAttribute("ACT/360")]
		ACT360,
		[System.Xml.Serialization.XmlEnumAttribute("30/360")]
		DCF30360,
		[System.Xml.Serialization.XmlEnumAttribute("30E/360")]
		DCF30E360,
        // Ticket 16097 Add new DCF CC 20080227
        [System.Xml.Serialization.XmlEnumAttribute("30E/360.ISDA")]
        DCF30E360ISDA,
        [System.Xml.Serialization.XmlEnumAttribute("ACT/ACT.ICMA")]
        ACTACTICMA,
        [System.Xml.Serialization.XmlEnumAttribute("BUS/252")]
        BUS252,
		//Not FpML DCF
		[System.Xml.Serialization.XmlEnumAttribute("30E+/360")]
		DCF30EPLUS360,
		[System.Xml.Serialization.XmlEnumAttribute("ACT/365L")]
		ACT365L,
	}
	#endregion DayCountFractionEnum
	#region DayTypeEnum
    public enum DayTypeEnum
    {
        /// <summary>
        /// Banque
        /// </summary>
        Business,
        /// <summary>
        /// Calendrier
        /// </summary>
        Calendar,
        /// <summary>
        /// Matière première
        /// </summary>
        /// PL 20131121
        CommodityBusiness,
        /// <summary>
        /// Forex
        /// </summary>
        CurrencyBusiness,
        /// <summary>
        /// Bourse/clearing
        /// </summary>
        ExchangeBusiness,
        /// <summary>
        /// Bourse/trading
        /// </summary>
        ScheduledTradingDay
    }
	#endregion DayTypeEnum

    #region DeterminationMethodEnum
    // CC/PL 20180531 Add value OSPPrice
    // EG 20140526 New
	public enum DeterminationMethodEnum
	{
		AgreedInitialPrice,
		AsSpecifiedInMasterConfirmation,
        CalculationAgent,
		ClosingPrice,
		DividendCurrency,
		ExpiringContractLevel,
        HedgeExecution,
        IssuerPaymentCurrency,
        NAV,
        OpenPrice,
        OSPPrice,
        SettlementCurrency,
        StrikeDateDetermination,
        TWAPPrice,
        ValuationTime,
        VWAPPrice,
    }
    #endregion DeterminationMethodEnum

    #region DiscountingTypeEnum
    public enum DiscountingTypeEnum
	{
		Standard,
		FRA,
	}
	#endregion DiscountingTypeEnum
	#region DividendAmountTypeEnum
	public enum DividendAmountTypeEnum
	{
		RecordAmount,
		ExAmount,
		PaidAmount,
		AsSpecifiedInMasterConfirmation,
	}
	#endregion DividendAmountTypeEnum
	#region DividendDateReferenceEnum
	public enum DividendDateReferenceEnum
	{
		ExDate,
		DividendPaymentDate,
		RecordDate,
		TerminationDate,
		EquityPaymentDate,
		FollowingPaymentDate,
		AdHocDate,
		CumulativeEquityPaid,
		CumulativeLiborPaid,
		CumulativeEquityExDiv,
		CumulativeLiborExDiv,
		SharePayment,
		CashSettlementPaymentDate,
		FloatingAmountPaymentDate,
	}
	#endregion DividendDateReferenceEnum
	#region DividendEntitlementEnum
	public enum DividendEntitlementEnum
	{
		ExDate,
		RecordDate,
	}
	#endregion DividendEntitlementEnum
	#region DividendPeriodEnum
	public enum DividendPeriodEnum
	{
		FirstPeriod,
		SecondPeriod,
	}
	#endregion DividendPeriodEnum
	#region DocumentVersionEnum
	public enum DocumentVersionEnum
	{
		[System.Xml.Serialization.XmlEnumAttribute("4-0")]
		Version40,
		[System.Xml.Serialization.XmlEnumAttribute("4-1")]
		Version41,
		[System.Xml.Serialization.XmlEnumAttribute("4-2")]
		Version42,
		[System.Xml.Serialization.XmlEnumAttribute("4-3")]
		Version43,
		[System.Xml.Serialization.XmlEnumAttribute("4-4")]
		Version44,
	}
	#endregion DocumentVersionEnum

	#region ExerciseStyleEnum
	public enum ExerciseStyleEnum
	{
		American,
		Bermuda,
		European,
	}
	#endregion ExerciseStyleEnum

	#region FraDiscountingEnum
	public enum FraDiscountingEnum
	{
		ISDA,
		AFMA,
		NONE,
	}
	#endregion FraDiscountingEnum
	#region FrequencyTypeEnum
	public enum FrequencyTypeEnum
	{
		Day,
		Business,
	}
	#endregion FrequencyTypeEnum
	#region FxBarrierTypeEnum
	public enum FxBarrierTypeEnum
	{
		Knockin,
		Knockout,
		ReverseKnockin,
		ReverseKnockout,
	}
	#endregion FxBarrierTypeEnum

	#region MethodOfAdjustmentEnum
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
	public enum MethodOfAdjustmentEnum
	{
		CalculationAgent,
		OptionsExchange,
	}
	#endregion MethodOfAdjustmentEnum

	#region NegativeInterestRateTreatmentEnum
	public enum NegativeInterestRateTreatmentEnum
	{
		NegativeInterestRateMethod,
		ZeroInterestRateMethod,
	}
	#endregion NegativeInterestRateTreatmentEnum

	#region OptionTypeEnum
	public enum OptionTypeEnum
	{
		Call,
		Put,
		Payer,
		Receiver,
		Straddle,
	}
	#endregion OptionTypeEnum
	
	#region PayerReceiverEnum
	public enum PayerReceiverEnum
	{
		Payer,
		Receiver,
	}
	#endregion PayerReceiverEnum
	#region PayoutEnum
	public enum PayoutEnum
	{
		Deferred,
		Immediate,
	}
	#endregion PayoutEnum
	#region PayRelativeToEnum
    // EG 20140526 New build FpML4.4
	public enum PayRelativeToEnum
	{
		CalculationPeriodStartDate,
		CalculationPeriodEndDate,
		ResetDate,
        ValuationDate,
	}
	#endregion PayRelativeToEnum
	#region PeriodEnum
	public enum PeriodEnum
	{
		D,
		W,
		M,
		Y,
		T,
	}
    #endregion PeriodEnum
	#region PremiumQuoteBasisEnum
	public enum PremiumQuoteBasisEnum
	{
		PercentageOfCallCurrencyAmount,
		PercentageOfPutCurrencyAmount,
		CallCurrencyPerPutCurrency,
		PutCurrencyPerCallCurrency,
		Explicit,
	}
	#endregion PremiumQuoteBasisEnum
	#region PremiumTypeEnum
	public enum PremiumTypeEnum
	{
		PrePaid,
		PostPaid,
		Variable,
		Fixed,
	}
	#endregion PremiumTypeEnum
	#region PriceExpressionEnum
	public enum PriceExpressionEnum
	{
		AbsoluteTerms,
		PercentageOfNotional,
	}
	#endregion PriceExpressionEnum

	#region QuoteBasisEnum
	public enum QuoteBasisEnum
	{
		Currency1PerCurrency2,
		Currency2PerCurrency1,
	}
	#endregion QuoteBasisEnum
    #region QuotationRateTypeEnum
    public enum QuotationRateTypeEnum
    {
        Bid,
        Ask,
        Mid,
        ExercisingPartyPays,
    }
    #endregion QuotationRateTypeEnum
    #region QuotationSideEnum
    /// <summary>
    /// Source du prix
    /// <para>
    /// Pour l'instant QuotationSideEnum est utilisé en lieu et place de l'enum SettlementPriceSource
    /// </para>
    /// <seealso cref="(http://svr-rd01/fpml44_help/fpml-4-4-schemes.html#s4.46)"/>
    /// </summary>
    // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory (Nouveau type de cotation)
    public enum QuotationSideEnum
	{
		/// <summary>
		/// Ask
		/// </summary>
        Ask,
        /// <summary>
        /// Bid
        /// </summary>
		Bid,
        /// <summary>
        /// Mid
        /// </summary>
		Mid,
        /// <summary>
        /// Close
        /// </summary>
        OfficialClose,
        /// <summary>
        /// Reference
        /// </summary>
        OfficialSettlement,
        /// <summary>
        /// FairValue
        /// </summary>
        FairValue,
    }
    #endregion QuotationSideEnum

    #region RateTreatmentEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
	public enum RateTreatmentEnum
	{
		BondEquivalentYield,
		MoneyMarketYield,
	}
	#endregion RateTreatment
	#region ResetRelativeToEnum
	public enum ResetRelativeToEnum
	{
		CalculationPeriodStartDate,
		CalculationPeriodEndDate,
	}
	#endregion ResetRelativeToEnum
	#region ReturnTypeEnum
	public enum ReturnTypeEnum
	{
		Dividend,
		Price,
		Total,
	}
	#endregion ReturnTypeEnum
	#region RollConventionEnum
	public enum RollConventionEnum
	{
        [System.Xml.Serialization.XmlEnumAttribute("EOM")]
        EOM,
        [System.Xml.Serialization.XmlEnumAttribute("FRN")]
		FRN,
        [System.Xml.Serialization.XmlEnumAttribute("IMM")]
		IMM,
        [System.Xml.Serialization.XmlEnumAttribute("IMMCAD")]
		IMMCAD,
        [System.Xml.Serialization.XmlEnumAttribute("IMMAUD")]
		IMMAUD,
        [System.Xml.Serialization.XmlEnumAttribute("IMMNZD")]
		IMMNZD,
        [System.Xml.Serialization.XmlEnumAttribute("SFE")]
		SFE,
        [System.Xml.Serialization.XmlEnumAttribute("NONE")]
		NONE,
        [System.Xml.Serialization.XmlEnumAttribute("TBILL")]
		TBILL,
        [System.Xml.Serialization.XmlEnumAttribute("1STMON")]
        FIRSTMON,
        [System.Xml.Serialization.XmlEnumAttribute("2NDMON")]
        SECONDMON,
        [System.Xml.Serialization.XmlEnumAttribute("3RDMON")]
        THIRDMON,
        [System.Xml.Serialization.XmlEnumAttribute("4THMON")]
        FOURTHMON,
        [System.Xml.Serialization.XmlEnumAttribute("5THMON")]
        FIFTHMON,
		//LP 20240522 [WI933] Add Last Monday
		[System.Xml.Serialization.XmlEnumAttribute("LASTMON")]
		LASTMON,
		// FI 20240527 [WI944] Add SECONDLASTMON
		[System.Xml.Serialization.XmlEnumAttribute("2NDLASTMON")]
		SECONDLASTMON,
		[System.Xml.Serialization.XmlEnumAttribute("1STTUE")]
        FIRSTTUE,
        [System.Xml.Serialization.XmlEnumAttribute("2NDTUE")]
        SECONDTUE,
        [System.Xml.Serialization.XmlEnumAttribute("3RDTUE")]
        THIRDTUE,
        [System.Xml.Serialization.XmlEnumAttribute("4THTUE")]
        FOURTHTUE,
        [System.Xml.Serialization.XmlEnumAttribute("5THTUE")]
        FIFTHTUE,
		//LP 20240522 [WI933] Add Last Tuesday
		[System.Xml.Serialization.XmlEnumAttribute("LASTTUE")]
		LASTTUE,
		// FI 20240527 [WI944] Add LASTTUE
		[System.Xml.Serialization.XmlEnumAttribute("2NDLASTTUE")]
		SECONDLASTTUE,
		[System.Xml.Serialization.XmlEnumAttribute("1STWED")]
        FIRSTWED,
        [System.Xml.Serialization.XmlEnumAttribute("2NDWED")]
        SECONDWED,
        [System.Xml.Serialization.XmlEnumAttribute("3RDWED")]
        THIRDWED,
        [System.Xml.Serialization.XmlEnumAttribute("4THWED")]
        FOURTHWED,
        [System.Xml.Serialization.XmlEnumAttribute("5THWED")]
        FIFTHWED,
		//LP 20240522 [WI933] Add Last Wednesday
		[System.Xml.Serialization.XmlEnumAttribute("LASTWED")]
		LASTWED,
		// FI 20240527 [WI944] Add 2NDLASTWED
		[System.Xml.Serialization.XmlEnumAttribute("2NDLASTWED")]
		SECONDLASTWED,
		[System.Xml.Serialization.XmlEnumAttribute("1STTHU")]
        FIRSTTHU,
        [System.Xml.Serialization.XmlEnumAttribute("2NDTHU")]
        SECONDTHU,
        [System.Xml.Serialization.XmlEnumAttribute("3RDTHU")]
        THIRDTHU,
        [System.Xml.Serialization.XmlEnumAttribute("4THTHU")]
        FOURTHTHU,
        [System.Xml.Serialization.XmlEnumAttribute("5THTHU")]
        FIFTHTHU,
		//LP 20240522 [WI933] Add Last Thursday
		[System.Xml.Serialization.XmlEnumAttribute("LASTTHU")]
		LASTTHU,
		// FI 20240527 [WI944] Add SECONDLASTFRI
		[System.Xml.Serialization.XmlEnumAttribute("2NDLASTTHU")]
		SECONDLASTTHU,
		[System.Xml.Serialization.XmlEnumAttribute("1STFRI")]
        FIRSTFRI,
        [System.Xml.Serialization.XmlEnumAttribute("2NDFRI")]
        SECONDFRI,
        [System.Xml.Serialization.XmlEnumAttribute("3RDFRI")]
        THIRDFRI,
        [System.Xml.Serialization.XmlEnumAttribute("4THFRI")]
        FOURTHFRI,
        [System.Xml.Serialization.XmlEnumAttribute("5THFRI")]
        FIFTHFRI,
		// LP 20240522 [WI933] Add Last Friday
		[System.Xml.Serialization.XmlEnumAttribute("LASTFRI")]
		LASTFRI,
		// FI 20240523 [26625][WI797] Add SECONDLASTFRI
		[System.Xml.Serialization.XmlEnumAttribute("2NDLASTFRI")]
		SECONDLASTFRI,
		[System.Xml.Serialization.XmlEnumAttribute("FOY")]
        FOY,
        [System.Xml.Serialization.XmlEnumAttribute("FOQ")]
        FOQ,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
		DAY1,
		[System.Xml.Serialization.XmlEnumAttribute("2")]
		DAY2,
		[System.Xml.Serialization.XmlEnumAttribute("3")]
		DAY3,
		[System.Xml.Serialization.XmlEnumAttribute("4")]
		DAY4,
		[System.Xml.Serialization.XmlEnumAttribute("5")]
		DAY5,
		[System.Xml.Serialization.XmlEnumAttribute("6")]
		DAY6,
		[System.Xml.Serialization.XmlEnumAttribute("7")]
		DAY7,
		[System.Xml.Serialization.XmlEnumAttribute("8")]
		DAY8,
		[System.Xml.Serialization.XmlEnumAttribute("9")]
		DAY9,
		[System.Xml.Serialization.XmlEnumAttribute("10")]
		DAY10,
		[System.Xml.Serialization.XmlEnumAttribute("11")]
		DAY11,
		[System.Xml.Serialization.XmlEnumAttribute("12")]
		DAY12,
		[System.Xml.Serialization.XmlEnumAttribute("13")]
		DAY13,
		[System.Xml.Serialization.XmlEnumAttribute("14")]
		DAY14,
		[System.Xml.Serialization.XmlEnumAttribute("15")]
		DAY15,
		[System.Xml.Serialization.XmlEnumAttribute("16")]
		DAY16,
		[System.Xml.Serialization.XmlEnumAttribute("17")]
		DAY17,
		[System.Xml.Serialization.XmlEnumAttribute("18")]
		DAY18,
		[System.Xml.Serialization.XmlEnumAttribute("19")]
		DAY19,
		[System.Xml.Serialization.XmlEnumAttribute("20")]
		DAY20,
		[System.Xml.Serialization.XmlEnumAttribute("21")]
		DAY21,
		[System.Xml.Serialization.XmlEnumAttribute("22")]
		DAY22,
		[System.Xml.Serialization.XmlEnumAttribute("23")]
		DAY23,
		[System.Xml.Serialization.XmlEnumAttribute("24")]
		DAY24,
		[System.Xml.Serialization.XmlEnumAttribute("25")]
		DAY25,
		[System.Xml.Serialization.XmlEnumAttribute("26")]
		DAY26,
		[System.Xml.Serialization.XmlEnumAttribute("27")]
		DAY27,
		[System.Xml.Serialization.XmlEnumAttribute("28")]
		DAY28,
		[System.Xml.Serialization.XmlEnumAttribute("29")]
		DAY29,
		[System.Xml.Serialization.XmlEnumAttribute("30")]
		DAY30,
        [System.Xml.Serialization.XmlEnumAttribute("MON")]
		MON,
        [System.Xml.Serialization.XmlEnumAttribute("TUE")]
		TUE,
        [System.Xml.Serialization.XmlEnumAttribute("WED")]
		WED,
        [System.Xml.Serialization.XmlEnumAttribute("THU")]
		THU,
        [System.Xml.Serialization.XmlEnumAttribute("FRI")]
		FRI,
        [System.Xml.Serialization.XmlEnumAttribute("SAT")]
		SAT,
        [System.Xml.Serialization.XmlEnumAttribute("SUN")]
		SUN,
        /// <summary>
        /// The last Friday which precedes by at least two business days the last business day of the month preceding the option month
        /// </summary>
        /// <remarks>
        /// Ticket 17776 : 
        /// new maturity rule for DC Options type Agricultural Options on the CBOT market.  
        /// Activation: from the 2012/05/11
        /// </remarks>
        [System.Xml.Serialization.XmlEnumAttribute("CBOTAGRIOPT")]
        CBOTAGRIOPT,
        /// <summary>
        ///Last Trading Day is the last Friday prior to the first calendar day of the option expiration month, 
        ///followed by at least two exchange days prior to the first calendar day of the option expiration month.
        ///<remarks>
        ///Exception: If this Friday is not an exchange day, or if this Friday is not an exchange day and followed by only one exchange day 
        ///prior to the first calendar day of the option expiration month, then the exchange day immediately preceding that Friday is the Last Trading Day. 
        ///An exchange day within the meaning of this exception is a day, which is both an exchange day at the Eurex Exchanges and a federal workday in the U.S.
        ///</remarks>
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("EUREXFIXEDINCOMEOPT")]
        EUREXFIXEDINCOMEOPT,
        /// <summary>
        /// Last Monday of the contract month. However, if the last Monday is a Non-Business Day or there is a Non-Business Day in the 4 days following the last Monday, 
        /// the last day of trading will be the penultimate Monday of the delivery month
        /// </summary>
        /// <remarks>
        /// Ticket 19979: new maturity rule for DC Futures type CO2 Emissions Futures on the ICE - LONDON (ICEU) market.  
        /// Activation: from the 2014/05/20
        /// PL 20170217: DEPRECATED - Replaced by CO2EMISSIONSFUT 
        /// </remarks>
        //[System.Xml.Serialization.XmlEnumAttribute("ICEUCO2EMISSIONSFUT")]
        //ICEUCO2EMISSIONSFUT,
        /// <summary>
        /// Last Monday of the contract month. However, if the last Monday is a Non-Business Day or there is a Non-Business Day in the 4 days following the last Monday, 
        /// the last day of trading will be the penultimate Monday of the delivery month
        /// </summary>
        /// <remarks>
        /// Ticket 19979: new maturity rule for DC Futures type CO2 Emissions Futures on the ICE - LONDON (ICEU) market.  
        /// Activation: from the 2014/05/20
        /// PL 20170217: ICEUCO2EMISSIONSFUT renamed to CO2EMISSIONSFUT for use on EEX (ECC) market
        /// </remarks>
        [System.Xml.Serialization.XmlEnumAttribute("CO2EMISSIONSFUT")]
        CO2EMISSIONSFUT,
        /// <summary>
        /// EOM except in December where that will be January 1st
        /// </summary>
        /// <remarks>
        /// New maturity rule for IDEX Delivery (D01FB and D02FB)
        /// </remarks>
        [System.Xml.Serialization.XmlEnumAttribute("IDEXDLV")]
        IDEXDLV,
        /// <summary>
        /// Day prior EOM except Monday
        /// </summary>
        /// <remarks>
        /// Rolls day prior the month end dates irrespective of the length of the month and the previous roll day 
        /// except when end of month is a monday in which case rolls on month end dates.
        /// </remarks>
        /// <remarks>
        /// Ticket 21303: 
        /// New Roll convention for new maturity rule for DC Futures Power and Gas / Cash-Settlement (Financial) on the ECC - EUROPEAN ENERGY EXCHANGE AG (XEEE) market.  
        /// Activation: from the 20160226
        /// </remarks>
        [System.Xml.Serialization.XmlEnumAttribute("EOM-1EXCEPTMON")]
        EOMMINUS1EXCEPTMON,
        /// <summary>
        /// Sao Paulo Time on the Wednesday closest to the 15th calendar day of the contract month
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("WEDNEAR15")]
        WEDNEAR15
    }
	#endregion RollConventionEnum
	#region RoundingDirectionEnum
	//Warning, garder cet enum en phase avec l'enum RoundingDirectionSQL
	public enum RoundingDirectionEnum
	{
		Up,
		Down,
		Nearest,
        /// <summary>
        /// Rounding mode to round towards "nearest neighbor" unless both neighbors are equidistant, in which case round down. 
        /// Behaves as ceiling if the discarded fraction is > 0.5; otherwise, behaves as floor. 
        /// </summary>
        /// <remarks> 
        /// Note that this is NOT the rounding mode commonly taught at school.
        /// http://download.oracle.com/javase/6/docs/api/java/math/RoundingMode.html#HALF_DOWN
        /// </remarks>
        HalfDown,
	}
	#endregion RoundingDirectionEnum

	#region SettlementTypeEnum
	public enum SettlementTypeEnum
	{
		Cash,
		Election,
		Physical,
	}
	#endregion SettlementTypeEnum
	#region SideRateBasisEnum
	public enum SideRateBasisEnum
	{
		Currency1PerBaseCurrency,
		BaseCurrencyPerCurrency1,
		Currency2PerBaseCurrency,
		BaseCurrencyPerCurrency2,
	}
	#endregion SideRateBasisEnum
	#region StandardSettlementStyleEnum
	public enum StandardSettlementStyleEnum
	{
		Standard,
		Net,
		StandardAndNet,
	}
	#endregion StandardSettlementStyleEnum
	#region StepRelativeToEnum
	public enum StepRelativeToEnum
	{
		Initial,
		Previous,
	}
	#endregion StepRelativeToEnum
	#region StrikeQuoteBasisEnum
	public enum StrikeQuoteBasisEnum
	{
		PutCurrencyPerCallCurrency,
		CallCurrencyPerPutCurrency,
	}
	#endregion StrikeQuoteBasisEnum

	#region TimeTypeEnum
	public enum TimeTypeEnum
	{
		Close,
		Open,
		OSP,
		SpecificTime,
		XETRA,
		DerivativesClose,
		AsSpecifiedInMasterConfirmation,
	}
	#endregion TimeTypeEnum
	#region TouchConditionEnum
	public enum TouchConditionEnum
	{
		Touch,
		Notouch,
	}
	#endregion TouchConditionEnum
	#region TriggerConditionEnum
	public enum TriggerConditionEnum
	{
		Above,
		Below,
	}
	#endregion TriggerConditionEnum

	#region WeeklyRollConventionEnum
    // EG 20140526 New build FpML4.4
	public enum WeeklyRollConventionEnum
	{
		SUN,
		MON,
		TUE,
		WED,
		THU,
		FRI,
		SAT,
        TBILL,
	}
	#endregion WeeklyRollConventionEnum
	#endregion Common Enums 4.2 & 4.4
}
namespace FpML.v44.Enum
{
    #region Enums Version 4.4
    #region DifferenceSeverityEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public enum DifferenceSeverityEnum
    {
        Warning,
        Error,
    }
    #endregion DifferenceSeverityEnum
    #region DifferenceTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public enum DifferenceTypeEnum
    {
        Value,
        Reference,
        Structure,
        Scheme,
    }
    #endregion DifferenceTypeEnum

    #region IndexEventConsequenceEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    // EG 20140526 New build FpML4.4 Add RelatedExchange
    public enum IndexEventConsequenceEnum
    {
        CalculationAgentAdjustment,
        NegotiatedCloseOut,
        CancellationAndPayment,
        RelatedExchange,
    }
    #endregion IndexEventConsequenceEnum
    #region InterestShortfallCapEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public enum InterestShortfallCapEnum
    {
        Fixed,
        Variable,
    }
    #endregion InterestShortfallCapEnum

    #region LengthUnitEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public enum LengthUnitEnum
    {
        Pages,
        TimeUnit,
    }
    #endregion LengthUnitEnum

    #region NationalisationOrInsolvencyOrDelistingEventEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public enum NationalisationOrInsolvencyOrDelistingEventEnum
    {
        NegotiatedCloseout,
        CancellationAndPayment,
    }
    #endregion NationalisationOrInsolvencyOrDelistingEventEnum
    #region NotionalAdjustmentEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public enum NotionalAdjustmentEnum
    {
        Execution,
        PortfolioRebalancing,
        Standard,
    }
    #endregion NotionalAdjustmentEnum

    #region ObligationCategoryEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public enum ObligationCategoryEnum
    {
        Payment,
        BorrowedMoney,
        ReferenceObligationsOnly,
        Bond,
        Loan,
        BondOrLoan,
    }
    #endregion ObligationCategoryEnum

    #region RealisedVarianceMethodEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public enum RealisedVarianceMethodEnum
    {
        Previous,
        Last,
        Both,
    }
    #endregion RealisedVarianceMethodEnum

    #region ShareExtraordinaryEventEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public enum ShareExtraordinaryEventEnum
    {
        AlternativeObligation,
        CancellationAndPayment,
        OptionsExchange,
        CalculationAgent,
        ModifiedCalculationAgent,
        PartialCancellationAndPayment,
        Component,
    }
    #endregion ShareExtraordinaryEventEnum
    #region StubPeriodTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public enum StubPeriodTypeEnum
    {
        ShortInitial,
        ShortFinal,
        LongInitial,
        LongFinal,
    }
    #endregion StubPeriodTypeEnum

    #region ValuationMethodEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public enum ValuationMethodEnum
    {
        Market,
        Highest,
        AverageMarket,
        AverageHighest,
        BlendedMarket,
        BlendedHighest,
        AverageBlendedMarket,
        AverageBlendedHighest,
    }
    #endregion ValuationMethodEnum
    #endregion Enums Version 4.4
}

#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Settlement;
using EfsML.v30.PosRequest;
using FixML.Enum;
using FixML.Interface;
using FpML.Enum;
using System;
using System.Collections;
using System.Data;
#endregion Using Directives

namespace FpML.Interface
{
    #region IActualPrice
    public interface IActualPrice
    {
        #region Accessors
        PriceExpressionEnum PriceExpression { set;get;}
        EFS_Decimal Amount { set; get;}
        bool CurrencySpecified { set;get;}
        ICurrency Currency { set; get;}
        #endregion Accessors
    }
    #endregion IActualPrice

    
    #region IAlgorithm
    // FI 20170928 [23452] Add
    public interface IAlgorithm : ISpheresId
    {
        string Name { set; get; }
        IScheme Role { set; get; }
    }
    #endregion

    #region IAdditionalPaymentAmount
    public interface IAdditionalPaymentAmount
    {
        #region Accessors
        bool PaymentAmountSpecified { set;get;}
        IMoney PaymentAmount { get;}
        bool FormulaSpecified { get;}
        //IFormula formula {get;}
        #endregion Accessors
    }
    #endregion IAdditionalPaymentAmount
    #region IAddress
    public interface IAddress
    {
        IStreetAddress StreetAddress { set;get;}
        EFS_String City { set;get;}
        EFS_String State { set;get;}
        IScheme Country { set;get;}
        EFS_String PostalCode { set;get;}
    }
    #endregion
    #region IAdjustableDate
    public interface IAdjustableDate
    {
        #region Accessors
        IBusinessDayAdjustments DateAdjustments { set; get;}
        IAdjustedDate UnadjustedDate { set; get;}
        string Efs_id { set; get; }
        #endregion Accessors
        #region Methods
        object Clone();
        IAdjustedDate GetAdjustedDate();
        #endregion Methods
    }
    #endregion IAdjustableDate
    #region IAdjustableDates
    public interface IAdjustableDates
    {
        #region Accessors
        IAdjustedDate[] UnadjustedDate { get; set; }
        IBusinessDayAdjustments DateAdjustments { get;}
        #endregion Accessors
        #region Indexors
        DateTime this[int pIndex] { get;}
        #endregion Indexors
    }
    #endregion IAdjustableDates
    #region IAdjustableOffset
    public interface IAdjustableOffset : IOffset
    {
        #region Accessors
        bool BusinessCentersNoneSpecified { set; get;}
        object BusinessCentersNone { set; get;}
        bool BusinessCentersDefineSpecified { set; get;}
        IBusinessCenters BusinessCentersDefine { set; get;}
        bool BusinessCentersReferenceSpecified { set; get;}
        IReference BusinessCentersReference { set; get;}
        string BusinessCentersReferenceValue { get;}
        #endregion Accessors

    }
    #endregion IAdjustableOffset
    #region IAdjustableOrRelativeAndAdjustedDate
    public interface IAdjustableOrRelativeAndAdjustedDate : IAdjustableOrRelativeDate
    {
        #region Accessors
        bool AdjustedDateSpecified { set;get;}
        IAdjustedDate AdjustedDate { set;get;}
        #endregion Accessors
    }
    #endregion IAdjustableOrRelativeDate
    #region IAdjustableOrRelativeDate
    public interface IAdjustableOrRelativeDate
    {
        #region Accessors
        bool AdjustableDateSpecified { set; get;}
        IAdjustableDate AdjustableDate { set; get;}
        bool RelativeDateSpecified { set; get;}
        IRelativeDateOffset RelativeDate { set; get;}
        IAdjustableDate CreateAdjustableDate { get;}
        IRelativeDateOffset CreateRelativeDate { get;}
        #endregion Accessors
    }
    #endregion IAdjustableOrRelativeDate
    #region IAdjustableOrRelativeDates
    public interface IAdjustableOrRelativeDates
    {
        #region Accessors
        bool AdjustableDatesSpecified { set; get;}
        IAdjustableDates AdjustableDates { set; get;}
        bool RelativeDatesSpecified { set; get;}
        IRelativeDates RelativeDates { set; get;}
        #endregion Accessors
    }
    #endregion IAdjustableOrRelativeDate
    #region IAdjustableDatesOrRelativeDateOffset
    /// EG 20140702 New
    public interface IAdjustableDatesOrRelativeDateOffset
    {
        #region Accessors
        bool AdjustableDatesSpecified { set; get; }
        IAdjustableDates AdjustableDates { set; get; }
        bool RelativeDateOffsetSpecified { set; get; }
        IRelativeDateOffset RelativeDateOffset { set; get; }
        #endregion Accessors
    }
    #endregion IAdjustableDatesOrRelativeDateOffset

    #region IAdjustableDateOrRelativeDateSequence
    /// EG 20140702 Add relativeDateSequenceSpecified|relativeDateSequence
    public interface IAdjustableDateOrRelativeDateSequence
    {
        #region Accessors
        bool AdjustableDateSpecified { set; get;}
        IAdjustableDate AdjustableDate { get;}
        bool RelativeDateSequenceSpecified { set; get; }
        IRelativeDateSequence RelativeDateSequence { get; }
        #endregion Accessors
    }
    #endregion IAdjustableDateOrRelativeDateSequence
    #region IAdjustableRelativeOrPeriodicDates
    /// EG 20140702 Add relativeDateSequenceSpecified
    public interface IAdjustableRelativeOrPeriodicDates
    {
        #region Accessors
        bool AdjustableDatesSpecified { get;}
        IAdjustableDates AdjustableDates { get;}
        bool PeriodicDatesSpecified { set; get;}
        IPeriodicDates PeriodicDates { set; get;}
        bool RelativeDateSequenceSpecified { set; get; }
        IRelativeDateSequence RelativeDateSequence { get;}
        #endregion Accessors
    }
    #endregion IAdjustableRelativeOrPeriodicDates
    #region IAdjustableRelativeOrPeriodicDates2
    /// EG 20140702 New
    public interface IAdjustableRelativeOrPeriodicDates2
    {
        #region Accessors
        bool AdjustableDatesSpecified { set; get; }
        IAdjustableDates AdjustableDates { set; get; }
        bool PeriodicDatesSpecified { set; get; }
        IPeriodicDates PeriodicDates { set; get; }
        bool RelativeDatesSpecified { set; get; }
        IRelativeDates RelativeDates { set; get; }
        #endregion Accessors
    }
    #endregion IAdjustableRelativeOrPeriodicDates2
    #region IAdjustedDate
    public interface IAdjustedDate
    {
        #region Accessors
        string Value { set;get;}
        DateTime DateValue { set; get;}
        string Id { set;get;}
        #endregion Accessors
    }
    #endregion IAdjustedDate
    #region IAdjustedRelativeDateOffset
    public interface IAdjustedRelativeDateOffset : IRelativeDateOffset
    {
    }
    #endregion IAdjustedRelativeDateOffset
    #region IAmericanExercise
    public interface IAmericanExercise : IExerciseBase
    {
        #region Accessors
        IAdjustableOrRelativeDate CommencementDate { get;}
        IAdjustableOrRelativeDate ExpirationDate { get;}
        bool LatestExerciseTimeSpecified { set;get;}
        IBusinessCenterTime LatestExerciseTime { set;get;}
        bool RelevantUnderlyingDateSpecified { set; get;}
        IAdjustableOrRelativeDates RelevantUnderlyingDate { set; get;}
        bool ExerciseFeeScheduleSpecified { set; get;}
        IExerciseFeeSchedule ExerciseFeeSchedule { get;}
        bool MultipleExerciseSpecified { set; get;}
        IMultipleExercise MultipleExercise { set; get;}
        IMultipleExercise CreateMultipleExercise();
        #endregion Accessors
    }
    #endregion IAmericanExercise
    #region IAmountSchedule
    public interface IAmountSchedule : ISchedule
    {
        #region Accessors
        ICurrency Currency { get;}
        #endregion Accessors
    }
    #endregion IAmountSchedule
    #region IAsian
    public interface IAsian
    {
        #region Accessors
        AveragingInOutEnum AveragingInOut { set;get;}
        bool StrikeFactorSpecified { set;get;}
        EFS_Decimal StrikeFactor { set;get;}
        bool AveragingPeriodInSpecified { set;get;}
        IEquityAveragingPeriod AveragingPeriodIn { set;get;}
        bool AveragingPeriodOutSpecified { set;get;}
        IEquityAveragingPeriod AveragingPeriodOut { set;get;}
        bool LookBackMethodSpecified { set;get;}
        IEmpty LookBackMethod { set;get;}
        #endregion Accessors
    }
    #endregion IAsian

    #region IAsset
    public interface IAsset
    {
        #region Accessors
        IScheme[] InstrumentId { set; get;}
        bool DescriptionSpecified { set; get;}
        EFS_String Description { set; get;}
        #endregion Accessors
    }
    #endregion IAsset

    #region IAssetClass
    public interface IAssetClass
    {
        #region Accessors
        #endregion Accessors
    }
    #endregion IAssetClass

    #region IShortAsset
    public interface IShortAsset : IAsset
    {
        #region Accessors
        ISpheresIdScheme Instrument { set; get;}
        //FI 20091223 [16471] ajout de instrumentSpecified
        bool InstrumentSpecified { set; get;}
        //
        //FI 20091223 [16471] ajout de notionalAmountSpecified
        bool NotionalAmountSpecified { set; get;}
        IMoney NotionalAmount { set; get;}
        //
        //FI 20091223 [16471] ajout de periodNumberOfDays
        bool PeriodNumberOfDaysSpecified { set; get;}
        EFS_Integer PeriodNumberOfDays { set; get;}
        #endregion Accessors
    }
    #endregion IShortAsset

    #region IAssetPool
    public interface IAssetPool
    {
        #region Accessors
        bool VersionSpecified { set;get;}
        EFS_NonNegativeInteger Version { set;get;}
        bool EffectiveDateSpecified { set;get;}
        IAdjustedDate EffectiveDate { set;get;}
        EFS_Decimal InitialFactor { set;get;}
        bool CurrentFactorSpecified { set;get;}
        EFS_Decimal CurrentFactor { set;get;}
        #endregion Accessors
    }
    #endregion IAssetPool
    #region IAtomicSettlementTransfer
    public interface IAtomicSettlementTransfer
    {
        #region Accessors
        bool SuppressSpecified { set; get;}
        bool Suppress { set; get;}
        #endregion Accessors
    }
    #endregion IAtomicSettlementTransfer
    #region IAutomaticExercise
    public interface IAutomaticExercise
    {
        #region Accessors
        decimal ThresholdRate { get;}
        #endregion Accessors
    }
    #endregion IAutomaticExercise

    #region IAveragingSchedule
    public interface IAveragingSchedule
    {
        #region Accessors
        EFS_Date StartDate { set;get;}
        EFS_Date EndDate { set;get;}
        EFS_Integer Frequency { set;get;}
        FrequencyTypeEnum FrequencyType { set;get;}
        bool WeekNumberSpecified { set;get;}
        EFS_Integer WeekNumber { set;get;}
        bool DayOfWeekSpecified { set;get;}
        WeeklyRollConventionEnum DayOfWeek { set;get;}
        IResetFrequency ResetFrequency { get;}
        #endregion Accessors

    }
    #endregion IAveragingSchedule

    #region IBarrier
    public interface IBarrier
    {
        #region Accessors
        bool BarrierCapSpecified { set;get;}
        ITriggerEvent BarrierCap { set;get;}
        bool BarrierFloorSpecified { set;get;}
        ITriggerEvent BarrierFloor { set;get;}
        #endregion Accessors
    }
    #endregion IBarrier
    #region IBasket
    /// EG 20140702 Add basketCurrencySpecified|basketCurrency
    public interface IBasket : IOpenUnits
    {
        #region Accessors
        bool BasketCurrencySpecified { set; get; }
        ICurrency BasketCurrency { set; get; }
        IBasketConstituent[] BasketConstituent { get;}
        #endregion Accessors
    }
    #endregion IBasket
    #region IBasketConstituent
    public interface IBasketConstituent
    {
        #region Accessors
        bool ConstituentWeightSpecified { get;}
        bool ConstituentWeightBasketAmountSpecified { get;}
        decimal ConstituentWeightBasketAmountValue { get;}
        string ConstituentWeightBasketAmountCurrency { get;}
        bool ConstituentWeightBasketPercentageSpecified { get;}
        decimal ConstituentWeightBasketPercentage { get;}
        bool ConstituentWeightOpenUnitsSpecified { get;}
        decimal ConstituentWeightOpenUnits { get;}
        IUnderlyingAsset UnderlyingAsset { get;}
        #endregion Accessors
    }
    #endregion IBasketConstituent
    #region IBermudaExercise
    public interface IBermudaExercise : IExerciseBase
    {
        #region Accessors
        IAdjustableOrRelativeDates BermudaExerciseDates { get;}
        bool RelevantUnderlyingDateSpecified { get;}
        IAdjustableOrRelativeDates RelevantUnderlyingDate { get;}
        bool LatestExerciseTimeSpecified { set;get;}
        IBusinessCenterTime LatestExerciseTime { set;get;}
        bool ExerciseFeeScheduleSpecified { get;}
        IExerciseFeeSchedule ExerciseFeeSchedule { get;}
        bool MultipleExerciseSpecified { set; get;}
        IMultipleExercise MultipleExercise { set; get;}
        IMultipleExercise CreateMultipleExercise();
        #endregion Accessors
    }
    #endregion IBermudaExercise
    #region IBond
    public interface IBond : IExchangeTraded
    {
        #region Accessors
        bool IssuerNameSpecified { set;get;}
        EFS_String IssuerName { set;get;}
        bool IssuerPartyReferenceSpecified { set;get;}
        IReference IssuerPartyReference { set;get;}
        bool SenioritySpecified { set;get;}
        IScheme Seniority { set;get;}
        bool CouponTypeSpecified { set;get;}
        IScheme CouponType { set;get;}
        bool CouponRateSpecified { set;get;}
        EFS_Decimal CouponRate { set;get;}
        bool MaturitySpecified { set;get;}
        EFS_Date Maturity { set;get;}
        bool ParValueSpecified { set;get;}
        EFS_Decimal ParValue { set;get;}
        bool FaceAmountSpecified { set;get;}
        EFS_Decimal FaceAmount { set;get;}
        bool PaymentFrequencySpecified { set;get;}
        IInterval PaymentFrequency { set;get;}
        bool DayCountFractionSpecified { set;get;}
        DayCountFractionEnum DayCountFraction { set;get;}
        string OtcmlId { set;get;}
        // EG 20160404 Migration vs2013
        //int OTCmlId { set; get; }
        #endregion Accessors
    }
    #endregion IBond

    #region IBondOption
    // EG 20150608 [21091] New ParValue
    public interface IBondOption : IOptionBaseExtended
    {
        #region Accessors
        IBondOptionStrike Strike { set; get; }
        bool BondSpecified { set; get; }
        IBond Bond{ set; get; }
        bool ConvertibleBondSpecified { set; get; }
        IConvertibleBond ConvertibleBond { set; get; }
        bool ParValueSpecified { get; }
        EFS_Decimal ParValue { get; }

        #endregion Accessors
    }
    #endregion IBondOption

    #region IBondOptionStrike
    public interface IBondOptionStrike
    {
        #region Accessors
        bool ReferenceSwapCurveSpecified { set; get; }
        IReferenceSwapCurve ReferenceSwapCurve { set; get; }
        bool PriceSpecified { set; get; }
        IOptionStrike Price { set; get; }
        #endregion Accessors
    }
    #endregion IBondOptionStrike

    #region IBulletPayment
    public interface IBulletPayment
    {
        #region Accessors
        IPayment Payment { get;}
        #endregion Accessors
    }
    #endregion IBulletPayment
    #region IBusinessCenter
    // EG 20150422 [20513] BANCAPERTA New id|efs_id
    public interface IBusinessCenter
    {
        #region Accessors
        string BusinessCenterScheme { set; get; }
        string Value { set; get;}
        string Id { set; get; }
        EFS_Id Efs_id { get; }
        #endregion Accessors
    }
    #endregion IBusinessCenter
    #region IBusinessCenters
    public interface IBusinessCenters
    {
        #region Accessors
        IBusinessCenter[] BusinessCenter { set; get;}
        string Id { set; get;}
        EFS_Id Efs_id { get;}
        #endregion Accessors
        #region Methods
        object Clone();
        IInterval GetInterval(string pPeriod, int pPeriodMultiplier);
        IBusinessDayAdjustments GetBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention);
        /// <summary>
        /// Retourne les business Centers associées aux acteurs, devises, marchés
        ///  <para>Retourne null, s'il n'existe aucun business center actif</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdC">devises au format ISO4217_ALPHA3</param>
        /// <param name="pIdM"></param>
        /// <returns></returns>
        /// <param name="pConnectionString"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIdM"></param>
        /// <returns></returns>
        /// FI 20131118 [19118] Add commentaires
        // EG 20180205 [23769] Add dbTransaction  
        IBusinessCenters LoadBusinessCenters(string pConnectionString, IDbTransaction pDbTransaction, string[] pIdA, string[] pIdC, string[] pIdM);
        #endregion Methods
    }
    #endregion IBusinessCenters
    #region IBusinessCenterTime
    /// <summary>
    /// A type for defining a time with respect to a business center location. For example, 11:00am London time.
    /// </summary>
    public interface IBusinessCenterTime
    {
        #region Accessors
        IHourMinuteTime HourMinuteTime { get;}
        IBusinessCenter BusinessCenter { get;}
        #endregion Accessors
    }
    #endregion IBusinessCenterTime
    #region IBusinessDateRange
    public interface IBusinessDateRange : IBusinessDayAdjustments
    {
        #region Accessors
        IBusinessDayAdjustments GetAdjustments { get;}
        EFS_Date UnadjustedFirstDate { set; get;}
        EFS_Date UnadjustedLastDate { set; get;}
        #endregion Accessors
    }
    #endregion IBusinessDateRange
    #region IBusinessDayAdjustments
    // EG 20190115 [24361] Add isSettlementOfHolidayDeliveryConvention (Migration financial settlement for BoM Products)
    public interface IBusinessDayAdjustments
    {
        #region Accessors
        bool IsSettlementOfHolidayDeliveryConvention { set; get; }
        BusinessDayConventionEnum BusinessDayConvention { set; get;}
        bool BusinessCentersNoneSpecified { set; get;}
        object BusinessCentersNone { set; get;}
        bool BusinessCentersDefineSpecified { set; get;}
        IBusinessCenters BusinessCentersDefine { set; get;}
        bool BusinessCentersReferenceSpecified { set; get;}
        IReference BusinessCentersReference { set; get;}
        string BusinessCentersReferenceValue { get;}
        /// <summary>
        /// Retourne un IBusinessDayAdjustments identique  
        /// </summary>
        /// <returns></returns>
        object Clone();
        IOffset DefaultOffsetPreSettlement { get;}
        #endregion Accessors
        #region Methods AdjustableDate 
        /// <summary>
        /// Create a AdjustableDate 
        /// </summary>
        /// <param name="pUnadjustedDate"></param>
        /// <returns></returns>
        IAdjustableDate CreateAdjustableDate(DateTime pUnadjustedDate);
        IOffset CreateOffset(PeriodEnum pPeriod, int pMultiplier, DayTypeEnum pDayType);
        #endregion Methods
    }
    #endregion IBusinessDayAdjustments

    #region ICalculation
    public interface ICalculation
    {
        #region Accessors
        bool NotionalSpecified { set; get;}
        INotional Notional { get;}
        bool FxLinkedNotionalSpecified { set; get;}
        IFxLinkedNotionalSchedule FxLinkedNotional { set; get;}
        bool RateFixedRateSpecified { set; get;}
        ISchedule RateFixedRate { get;}
        bool RateFloatingRateSpecified { set; get;}
        IFloatingRateCalculation RateFloatingRate { get;}
        bool RateInflationRateSpecified { set; get;}
        IInflationRateCalculation RateInflationRate { get;}
        bool DiscountingSpecified { get;}
        IDiscounting Discounting { get;}
        bool CompoundingMethodSpecified { get;}
        CompoundingMethodEnum CompoundingMethod { get;}
        DayCountFractionEnum DayCountFraction { set; get;}
        #endregion accessors
    }
    #endregion ICalculation
    #region ICalculationAgent
    public interface ICalculationAgent
    {
        #region Accessors
        bool PartyReferenceSpecified { set; get;}
        IReference[] PartyReference { set; get;}
        bool PartySpecified { set; get;}
        CalculationAgentPartyEnum Party { set; get;}
        #endregion Accessors
    }
    #endregion ICalculation
    #region ICalculationPeriodAmount
    public interface ICalculationPeriodAmount
    {
        #region Accessors
        bool CalculationSpecified { set; get;}
        ICalculation Calculation { get;}
        bool KnownAmountScheduleSpecified { set; get;}
        IKnownAmountSchedule KnownAmountSchedule { get;}
        #endregion Accessors
    }
    #endregion ICalculationPeriodAmount
    #region ICalculationPeriodDates
    public interface ICalculationPeriodDates
    {
        #region Accessors
        EFS_CalculationPeriodDates Efs_CalculationPeriodDates { set; get;}
        ICalculationPeriodFrequency CalculationPeriodFrequency { get;}
        IBusinessDayAdjustments CalculationPeriodDatesAdjustments { set;get;}
        bool FirstPeriodStartDateSpecified { set; get;}
        IAdjustableDate FirstPeriodStartDate { get;}
        bool FirstRegularPeriodStartDateSpecified { set; get;}
        EFS_Date FirstRegularPeriodStartDate { set; get; }
        bool LastRegularPeriodEndDateSpecified { set; get;}
        EFS_Date LastRegularPeriodEndDate { get; set; }
        bool EffectiveDateAdjustableSpecified { get; set;}
        IAdjustableDate EffectiveDateAdjustable { get; set; }
        bool EffectiveDateRelativeSpecified { get;}
        IAdjustedRelativeDateOffset EffectiveDateRelative { get;}
        bool TerminationDateAdjustableSpecified { get; set;}
        IAdjustableDate TerminationDateAdjustable { get; set;}
        bool TerminationDateRelativeSpecified { get;}
        IRelativeDateOffset TerminationDateRelative { get; }
        string Id { set; get;}
        #endregion Accessors
        #region Methods
        IRequiredIdentifierDate CreateRequiredIdentifierDate(DateTime pDate);
        #endregion Methods
    }
    #endregion ICalculationPeriodDates
    #region ICalculationPeriodFrequency
    public interface ICalculationPeriodFrequency
    {
        #region Accessors
        PeriodEnum Period { set; get;}
        EFS_Integer PeriodMultiplier { set; get;}
        IInterval Interval { get;}
        RollConventionEnum RollConvention { set; get;}
        #endregion Accessors
    }
    #endregion ICalculationPeriodFrequency
    #region ICalendarSpread
    public interface ICalendarSpread
    {
        #region Accessors
        IAdjustableOrRelativeDate ExpirationDateTwo { set;get;}
        #endregion Accessors
    }
    #endregion ICalendarSpread
    #region ICancelableProvision
    public interface ICancelableProvision : IExerciseProvision, IProvision
    {
        #region Accessors
        object EFS_Exercise { get;}
        IReference BuyerPartyReference { set; get;}
        IReference SellerPartyReference { set; get;}
        EFS_ExerciseDates Efs_ExerciseDates { set; get;}
        #endregion Accessors
    }
    #endregion ICancelableProvision
    #region ICapFloor
    public interface ICapFloor
    {
        #region Accessors
        IInterestRateStream Stream { get;}
        IInterestRateStream[] StreamInArray { get;}
        bool PremiumSpecified { set; get;}
        IPayment[] Premium { get;}
        bool AdditionalPaymentSpecified { set; get;}
        IPayment[] AdditionalPayment { get;}
        bool EarlyTerminationProvisionSpecified { get;}
        IEarlyTerminationProvision EarlyTerminationProvision { get;}
        bool ImplicitProvisionSpecified { get;}
        IImplicitProvision ImplicitProvision { get;}
        bool ImplicitCancelableProvisionSpecified { get;}
        bool ImplicitOptionalEarlyTerminationProvisionSpecified { get;}
        bool ImplicitMandatoryEarlyTerminationProvisionSpecified { get;}
        bool ImplicitExtendibleProvisionSpecified { get;}
        EFS_EventDate MaxTerminationDate { get;}
        #endregion Methods
    }
    #endregion ICapFloor
    #region ICashPriceMethod
    public interface ICashPriceMethod
    {
        #region Accessors
        bool CashSettlementReferenceBanksSpecified { set;get;}
        //ICashSettlementReferenceBanks cashSettlementReferenceBanks {set;get;}
        ICurrency CashSettlementCurrency { set;get;}
        QuotationRateTypeEnum QuotationRateType { set;get;}
        #endregion Accessors
    }
    #endregion ICashPriceMethod
    #region ICashSettlement
    public interface ICashSettlement
    {
        #region Accessors
        IRelativeDateOffset ValuationDate { set; get;}
        IBusinessCenterTime ValuationTime { set;get;}
        bool PaymentDateSpecified { set; get;}
        ICashSettlementPaymentDate PaymentDate { set; get;}
        bool CashPriceMethodSpecified { set;get;}
        ICashPriceMethod CashPriceMethod { set;get;}
        bool CashPriceAlternateMethodSpecified { set;get;}
        ICashPriceMethod CashPriceAlternateMethod { set;get;}
        bool ParYieldCurveAdjustedMethodSpecified { set;get;}
        IYieldCurveMethod ParYieldCurveAdjustedMethod { set;get;}
        bool ZeroCouponYieldAdjustedMethodSpecified { set;get;}
        IYieldCurveMethod ZeroCouponYieldAdjustedMethod { set;get;}
        bool ParYieldCurveUnadjustedMethodSpecified { set;get;}
        IYieldCurveMethod ParYieldCurveUnadjustedMethod { set;get;}
        string Id { set;get;}

        #endregion Accessors
        #region Methods
        ICashSettlementPaymentDate CreatePaymentDate { get;}
        ICashPriceMethod CreateCashPriceMethod(string pCurrency, QuotationRateTypeEnum pQuotationRateType);
        #endregion Methods
    }
    #endregion ICashSettlement
    #region ICashSettlementPaymentDate
    public interface ICashSettlementPaymentDate
    {
        #region Accessors
        bool AdjustableDatesSpecified { set; get;}
        IAdjustableDates AdjustableDates { set; get;}
        bool BusinessDateRangeSpecified { set; get;}
        IBusinessDateRange BusinessDateRange { set; get;}
        bool RelativeDateSpecified { set; get;}
        IRelativeDateOffset RelativeDate { set; get;}
        string Id { set;get;}
        #endregion Accessors
        #region Methods
        #endregion Methods
    }
    #endregion ICashSettlementPaymentDate

    #region ICommission
    public interface ICommission
    {
        #region Accessors
        EFS_Decimal Amount { get;}
        CommissionDenominationEnum CommissionDenomination { set;get;}
        bool CurrencySpecified { get;}
        string Currency { set;get;}
        #endregion Accessors
        #region Methods
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        #endregion Methods
    }
    #endregion ICommission
    #region IComposite
    public interface IComposite
    {
        #region Accessors
        EFS_MultiLineString DeterminationMethod { set;get;}
        bool DeterminationMethodSpecified {set; get;}
        IRelativeDateOffset RelativeDate { set;get;}
        bool RelativeDateSpecified {set; get;}
        IFxSpotRateSource FxSpotRateSource { set;get;}
        bool FxSpotRateSourceSpecified {set; get;}
        #endregion Accessors
    }
    #endregion IComposite
    #region ICompounding
    public interface ICompounding
    {
        #region Accessors
        bool CompoundingMethodSpecified {set;get;}
        CompoundingMethodEnum CompoundingMethod  {set;get;}
        ICompoundingRate CompoundingRate  {set;get;} 
        bool CompoundingSpreadSpecified {set;get;}
        decimal CompoundingSpread { set; get; }
        #endregion Accessors
    }
    #endregion ICompounding
    #region ICompoundingRate
    public interface ICompoundingRate
    {
        #region Accessors
        bool TypeInterestLegRateSpecified { set; get; }
        IReference TypeInterestLegRate { set; get; }
        bool TypeSpecificRateSpecified { set; get; }
        IInterestAccrualsMethod TypeSpecificRate { set; get; }
        #endregion Accessors
    }
    #endregion ICompoundingRate
    #region IConvertibleBond
    public interface IConvertibleBond : IBond
    {
        #region Accessors
        IUnderlyingAsset UnderlyingEquity { set;get;}
        bool RedemptionDateSpecified { set;get;}
        EFS_Date RedemptionDate { set;get;}
        #endregion Accessors
    }
    #endregion IConvertibleBond

    #region ICreditEvents
    public interface ICreditEvents
    {
        #region Accessors
        bool BankruptcySpecified { set;get;}
        bool FailureToPaySpecified { set;get;}
        IFailureToPay FailureToPay { set;get;}
        bool FailureToPayPrincipalSpecified { set;get;}
        bool FailureToPayInterestSpecified { set;get;}
        bool ObligationDefaultSpecified { set;get;}
        bool ObligationAccelerationSpecified { set;get;}
        bool RepudiationMoratoriumSpecified { set;get;}
        bool RestructuringSpecified { set;get;}
        IRestructuring Restructuring { set;get;}
        bool DistressedRatingsDowngradeSpecified { set;get;}
        bool MaturityExtensionSpecified { set;get;}
        bool WritedownSpecified { set;get;}
        bool DefaultRequirementSpecified { set;get;}
        IMoney DefaultRequirement { set;get;}
        bool CreditEventNoticeSpecified { set;get;}
        ICreditEventNotice CreditEventNotice { set;get;}
        #endregion Accessors
    }
    #endregion ICreditEvents
    #region ICreditEventNotice
    public interface ICreditEventNotice
    {
        #region Accessors
        INotifyingParty NotifyingParty { set;get;}
        bool BusinessCenterSpecified { set;get;}
        IBusinessCenter BusinessCenter { set;get;}
        bool PubliclyAvailableInformationSpecified { set;get;}
        IPubliclyAvailableInformation PubliclyAvailableInformation { set;get;}
        #endregion Accessors
    }
    #endregion ICreditEventNotice
    #region ICurrency
    /// <summary>
    /// Représente une devise
    /// </summary>
    /// FI 2041010 [20275] Modify
    public interface ICurrency
    {
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        /// FI 2041010 [20275] add
        string CurrencyScheme { set; get; }
        /// <summary>
        /// code de la devise selon currencyScheme
        /// </summary>
        string Value { set; get; }
        #endregion Accessors
    }
    #endregion ICurrency

    #region IDataDocument
    public interface IDataDocument : IDocument
    {
        #region Accessors
        IParty[] Party { set; get; }
        ITrade[] Trade { set; get; }
        ITrade FirstTrade { set; get; }
        object Item { get; }
        #endregion Accessors
        //
        #region Methods
        Type GetTypeParty();
        #endregion Methods
    }
    #endregion IDataDocument
    #region IDateList
    public interface IDateList
    {
        #region Accessors
        object[] Date { get;}
        #endregion Accessors
        #region Indexors
        DateTime this[int pIndex] { get;}
        #endregion Indexors
    }
    #endregion IDateList
    #region IDateOffset
    /// EG 20140702 Add period|periodMultiplier|dayTypeSpecified|dayType|businessDayConvention
    public interface IDateOffset
    {
        #region Accessors
        PeriodEnum Period { set; get; }
        EFS_Integer PeriodMultiplier { set; get; }
        bool DayTypeSpecified { set; get; }
        DayTypeEnum DayType { set; get; }
        BusinessDayConventionEnum BusinessDayConvention { set; get; }
        IOffset Offset { get;}
        #endregion Accessors
        #region Methods
        IBusinessDayAdjustments BusinessDayAdjustments(IBusinessCenters pBusinessCenters);
        #endregion Methods
    }
    #endregion IDateOffset
    #region IDateTimeList
    public interface IDateTimeList
    {
        #region Accessors
        EFS_DateTimeArray[] DateTime { set;get;}
        #endregion Accessors
    }
    #endregion IDateTimeList
    #region IDeclarativeProvision
    public interface IDeclarativeProvision
    {
        #region Methods
        bool CancelableProvisionSpecified { get;}
        ICancelableProvision CancelableProvision { get;}
        bool ExtendibleProvisionSpecified { get;}
        IExtendibleProvision ExtendibleProvision { get;}
        bool EarlyTerminationProvisionSpecified { get;}
        IEarlyTerminationProvision EarlyTerminationProvision { get;}
        bool StepUpProvisionSpecified { get;}
        IStepUpProvision StepUpProvision { get;}
        bool ImplicitProvisionSpecified { get;}
        IImplicitProvision ImplicitProvision { get;}
        #endregion Methods
    }
    #endregion IDeclarativeProvision
    #region IDiscounting
    public interface IDiscounting
    {
        #region Accessors
        DiscountingTypeEnum DiscountingType { get;}
        bool RateSpecified { get;}
        decimal Rate { get;}
        bool DayCountFractionSpecified { get;}
        DayCountFractionEnum DayCountFraction { get;}
        #endregion Accessors
    }
    #endregion IDiscounting
    #region IDividendAdjustment
    public interface IDividendAdjustment
    {
        #region Accessors
        IDividendPeriodDividend[] DividendPeriodDividend { set;get;}
        #endregion Accessors
    }
    #endregion IDividendAdjustment

    #region IDividendConditions
    /// EG 20140702 Add dividendPeriodSpecified|dividendPeriod|dividendPaymentDateSpecified
    public interface IDividendConditions
    {
        #region Accessors
        bool DividendEntitlementSpecified { set; get;}
        DividendEntitlementEnum DividendEntitlement { set;get;}
        bool DividendAmountSpecified { set; get;}
        DividendAmountTypeEnum DividendAmount { set; get;}
        bool DividendPeriodSpecified { set; get; }
        DividendPeriodEnum DividendPeriod { set; get; }
        bool DividendPaymentDateSpecified { set; get; }
        IDividendPaymentDate DividendPaymentDate { set;get;}
        #endregion Accessors
    }
    #endregion IDividendConditions
    #region IDividendPaymentDate
    public interface IDividendPaymentDate
    {
        #region Accessors
        bool DividendDateReferenceSpecified { set; get;}
        DividendDateReferenceEnum DividendDateReference { set; get;}
        bool AdjustableDateSpecified { get;}
        IAdjustableDate AdjustableDate { get;}
        bool OffsetSpecified { get;}
        IOffset Offset { get;}
        #endregion Accessors
    }
    #endregion IDividendPaymentDate
    #region IDividendPayout
    public interface IDividendPayout
    {
        #region Accessors
        bool DividendPayoutRatioSpecified { set;get;}
        EFS_Decimal DividendPayoutRatio { get;}
        bool DividendPayoutConditionsSpecified { get;}
        #endregion Accessors
    }
    #endregion IDividendPayout
    #region IDividendPeriod
    public interface IDividendPeriod
    {
        #region Accessors
        IAdjustedDate UnadjustedStartDate { set;get;}
        IAdjustedDate UnadjustedEndDate { set;get;}
        IBusinessDayAdjustments DateAdjustments { set;get;}
        IReference UnderlyerReference { set;get;}
        #endregion Accessors
    }
    #endregion IDividendPeriod
    #region IDividendPeriodDividend
    public interface IDividendPeriodDividend : IDividendPeriod
    {
        #region Accessors
        IMoney Dividend { set;get;}
        EFS_Decimal Multiplier { set;get;}
        #endregion Accessors
    }
    #endregion IDividendPeriodDividend
    #region IDocument
    /// EG 20140702 Add 
    public interface IDocument
    {
        #region Accessors
        DocumentVersionEnum Version { get;}
        #endregion Accessors
    }
    #endregion IDocument

    #region public IDocumentation
    public interface IDocumentation
    {
        #region Accessors
        bool MasterAgreementSpecified { get; set;}
        IMasterAgreement MasterAgreement { get; set;}
        #endregion Accessors
        #region Methode
        IMasterAgreement CreateMasterAgreement();
        #endregion Methode
    }
    #endregion public IDocumentation


    #region IEarlyTerminationProvision
    public interface IEarlyTerminationProvision
    {
        #region Accessors
        bool MandatorySpecified { get;}
        IMandatoryEarlyTermination Mandatory { get;}
        bool OptionalSpecified { get;}
        IOptionalEarlyTermination Optional { get;}
        #endregion Accessors
    }
    #endregion IEarlyTerminationProvision

    #region IEFS_DayCountFraction
    public interface IEFS_DayCountFraction
    {
        #region Accessors
        string DayCountFractionFpML { get;}
        /// <summary>
        /// numérateur (nbr de jours résiduels lorsque le nbr d'années pleines dépasse 0)
        /// </summary>
        int Numerator { get;}
        /// <summary>
        /// Dénominateur
        /// </summary>
        decimal Denominator { get;}
        /// <summary>
        /// Obtient le nbr d'années pleines
        /// </summary>
        int NumberOfCalendarYears { get;}
        /// <summary>
        /// Obtient le nbr de jour calendaires indépendamment du DCF   
        /// <para>DateEnd - DateStart</para>
        /// </summary>
        int TotalNumberOfCalculatedDays { get;}
        #endregion Accessors
    }
    #endregion IEFS_DayCountFraction
    #region IEFS_Step
    public interface IEFS_Step
    {
        #region Accessors
        DateTime AdjustedDate { get;}
        decimal StepValue { get;}
        #endregion Accessors
    }
    #endregion IEFS_Step

    #region IEmpty
    public interface IEmpty
    {
        #region Accessors
        object Empty { get;}
        #endregion Accessors
    }
    #endregion IEmpty
    #region IEquityAmericanExercise
    public interface IEquityAmericanExercise : IEquitySharedAmericanExercise, IEquityExercise
    {
        #region Accessors
        bool EquityMultipleExerciseSpecified { set;get;}
        IEquityMultipleExercise EquityMultipleExercise { set;get;}
        #endregion Accessors
    }
    #endregion IEquityAmericanExercise
    #region IEquityAsset
    public interface IEquityAsset : IExchangeTraded
    {
    }
    #endregion IEquityAsset
    #region IEquityAveragingPeriod
    public interface IEquityAveragingPeriod
    {
        #region Accessors
        bool ScheduleSpecified { set;get;}
        IAveragingSchedule[] Schedule { set;get;}
        bool AveragingDateTimesSpecified { set;get;}
        IDateTimeList AveragingDateTimes { set;get;}
        IScheme MarketDisruption { set;get;}
        #endregion Accessors
    }
    #endregion IEquityAveragingPeriod
    #region IEquityBermudaExercise
    public interface IEquityBermudaExercise : IEquitySharedAmericanExercise, IEquityExercise
    {
        #region Accessors
        IDateList BermudaExerciseDates { set;get;}
        bool EquityMultipleExerciseSpecified { set;get;}
        IEquityMultipleExercise EquityMultipleExercise { set;get;}
        #endregion Accessors

    }
    #endregion IEquityBermudaExercise
    #region IEquityDerivativeBase
    public interface IEquityDerivativeBase
    {
        #region Accessors
        IReference BuyerPartyReference { set;get;}
        IReference SellerPartyReference { set;get;}
        OptionTypeEnum OptionType { set;get;}
        bool EquityEffectiveDateSpecified { set;get;}
        EFS_Date EquityEffectiveDate { set;get;}
        IUnderlyer Underlyer { set;get;}
        bool NotionalSpecified { set;get;}
        IMoney Notional { set;get;}
        IEquityExerciseValuationSettlement EquityExercise { set;get;}
        bool FeatureSpecified { set;get;}
        IOptionFeatures Feature { set;get;}
        bool FxFeatureSpecified { set;get;}
        IFxFeature FxFeature { set;get;}
        bool StrategyFeatureSpecified { set;get;}
        IStrategyFeature StrategyFeature { set;get;}
        IOptionFeatures CreateOptionFeatures { get; }
        IFxFeature CreateFxFeature { get; }
        #endregion Accessors
    }
    #endregion IEquityDerivativeBase
    #region IEquityDerivativeLongFormBase
    public interface IEquityDerivativeLongFormBase : IEquityDerivativeBase
    {
        #region Accessors
        bool DividendConditionsSpecified { set;get;}
        IDividendConditions DividendConditions { set;get;}
        MethodOfAdjustmentEnum MethodOfAdjustment { set;get;}
        IExtraordinaryEvents ExtraordinaryEvents { set;get;}
        #endregion Accessors
    }
    #endregion IEquityDerivativeLongFormBase
    #region IEquityDerivativeShortFormBase
    public interface IEquityDerivativeShortFormBase : IEquityDerivativeBase
    {
        #region Accessors
        MethodOfAdjustmentEnum MethodOfAdjustment { set; get; }
        #endregion Accessors
    }
    #endregion IEquityDerivativeShortFormBase
    #region IEquityEuropeanExercise
    public interface IEquityEuropeanExercise : IEquityExercise
    {
    }
    #endregion IEquityEuropeanExercise
    #region IEquityExercise
    public interface IEquityExercise : IExerciseId
    {
        #region Accessors
        IAdjustableOrRelativeDate ExpirationDate { set; get;}
        TimeTypeEnum EquityExpirationTimeType { set; get;}
        bool EquityExpirationTimeSpecified { set; get;}
        IBusinessCenterTime EquityExpirationTime { set; get;}
        #endregion
    }
    #endregion
    #region IEquityExerciseValuationSettlement
    public interface IEquityExerciseValuationSettlement
    {
        #region Accessors
        bool EquityExerciseAmericanSpecified { set;get;}
        IEquityAmericanExercise EquityExerciseAmerican { set;get;}
        bool EquityExerciseBermudaSpecified { set;get;}
        IEquityBermudaExercise EquityExerciseBermuda { set;get;}
        bool EquityExerciseEuropeanSpecified { set;get;}
        IEquityEuropeanExercise EquityExerciseEuropean { set;get;}
        bool AutomaticExerciseSpecified { set;get;}
        bool AutomaticExercise { set;get;}
        bool PrePaymentSpecified { set;get;}
        IPrePayment PrePayment { set;get;}
        bool MakeWholeProvisionsSpecified { set;get;}
        IMakeWholeProvisions MakeWholeProvisions { set;get;}
        IEquityValuation EquityValuation { set;get;}
        bool SettlementDateSpecified { set;get;}
        IAdjustableOrRelativeDate SettlementDate { set;get;}
        ICurrency SettlementCurrency { set;get;}
        bool SettlementPriceSourceSpecified { set;get;}
        IScheme SettlementPriceSource { set;get;}
        SettlementTypeEnum SettlementType { set;get;}
        bool ElectionDateSpecified { set;get;}
        IAdjustableOrRelativeDate ElectionDate { set;get;}
        bool ElectingPartyReferenceSpecified { set;get;}
        IReference ElectingPartyReference { set;get;}
        ExerciseStyleEnum GetStyle { get;}
        #endregion Accessors
        #region Methods
        #endregion Methods
    }
    #endregion IEquityExerciseValuationSettlement
    #region IEquityOption
    public interface IEquityOption : IEquityDerivativeLongFormBase
    {
        #region Accessors
        IEquityStrike Strike { set;get;}
        bool SpotPriceSpecified { set;get;}
        EFS_Decimal SpotPrice { set;get;}
        bool NumberOfOptionsSpecified { set;get;}
        EFS_Decimal NumberOfOptions { set;get;}
        EFS_Decimal OptionEntitlement { set;get;}
        IEquityPremium EquityPremium { set;get;}
        EFS_EquityOption Efs_EquityOption { set; get;}
        EFS_EquityOptionPremium Efs_EquityOptionPremium { set; get;}
        #endregion Accessors
    }
    #endregion IEquityOption
    #region IEquityOptionTransactionSupplement 
    public interface IEquityOptionTransactionSupplement : IEquityDerivativeShortFormBase
    {
        #region Accessors
        #endregion Accessors
    }
    #endregion IEquityOptionTransactionSupplement
    #region IEquityMultipleExercise
    public interface IEquityMultipleExercise
    {
        #region Accessors
        bool IntegralMultipleExerciseSpecified { set;get;}
        EFS_Decimal IntegralMultipleExercise { set;get;}
        EFS_Decimal MinimumNumberOfOptions { set;get;}
        EFS_Decimal MaximumNumberOfOptions { set;get;}
        #endregion Accessors
    }
    #endregion IEquityMultipleExercise
    #region IEquityPremium
    // EG 20150422 [20513] BANCAPERTA
    public interface IEquityPremium 
    {
        #region Accessors
        IReference PayerPartyReference { set; get; }
        IReference ReceiverPartyReference { set; get; }
        bool PremiumTypeSpecified { set; get; }
        PremiumTypeEnum PremiumType { set; get; }
        bool PaymentAmountSpecified { set; get; }
        IMoney PaymentAmount { set; get; }
        bool PaymentDateSpecified { set; get; }
        IAdjustableDate PaymentDate { set; get; }
        bool SwapPremiumSpecified { set; get; }
        EFS_Boolean SwapPremium { set; get; }
        bool PricePerOptionSpecified { set; get; }
        IMoney PricePerOption { set; get; }
        bool PercentageOfNotionalSpecified { set; get; }
        EFS_Decimal PercentageOfNotional { set; get; }
        #endregion Accessors
    }
    #endregion IEquityPremium
    #region IEquitySharedAmericanExercise
    public interface IEquitySharedAmericanExercise : ISharedAmericanExercise
    {
        bool LatestExerciseTimeTypeSpecified { set;get;}
        TimeTypeEnum LatestExerciseTimeType { set;get;}
    }
    #endregion
    #region IEquityStrike
    public interface IEquityStrike
    {
        #region Accessors
        bool PriceSpecified { set;get;}
        EFS_Decimal Price { set;get;}
        bool PercentageSpecified { set;get;}
        EFS_Decimal Percentage { set;get;}
        bool StrikeDeterminationDateSpecified { set;get;}
        IAdjustableOrRelativeDate StrikeDeterminationDate { set;get;}
        bool CurrencySpecified { set;get;}
        ICurrency Currency { set;get;}
        #endregion Accessors
    }
    #endregion IEquityStrike
    #region IEquitySwapTransactionSupplement
    /// EG 20140702 New
    public interface IEquitySwapTransactionSupplement : IReturnSwapBase
    {
        #region Accessors
        bool MutualEarlyTerminationSpecified { set; get; }
        EFS_Boolean MutualEarlyTermination { set; get; }
        bool MultipleExchangeIndexAnnexFallbackSpecified { set; get; }
        EFS_Boolean MultipleExchangeIndexAnnexFallback { set; get; }
        bool LocalJurisdictionSpecified { set; get; }
        IScheme LocalJurisdiction { set; get; }
        bool RelevantJurisdictionSpecified { set; get; }
        IScheme RelevantJurisdiction { set; get; }
        #endregion Accessors
    }
    #endregion IEquitySwapTransactionSupplement

    #region IEquityValuation
    /// EG 20140702 Add valuationDateSpecified
    public interface IEquityValuation
    {
        #region Accessors
        bool ValuationDateSpecified { set; get; }
        IAdjustableDateOrRelativeDateSequence ValuationDate { get;}
        bool ValuationDatesSpecified { set; get;}
        IAdjustableRelativeOrPeriodicDates ValuationDates { set; get;}
        bool ValuationTimeTypeSpecified { get;}
        TimeTypeEnum ValuationTimeType { get;}
        bool ValuationTimeSpecified { get;}
        IBusinessCenterTime ValuationTime { get;}
        bool FuturesPriceValuationSpecified { get;}
        EFS_Boolean FuturesPriceValuation { get;}
        bool OptionsPriceValuationSpecified { get;}
        EFS_Boolean OptionsPriceValuation { get;}
        #endregion Accessors
    }
    #endregion IEquityValuation
    #region IEuropeanExercise
    public interface IEuropeanExercise : IExerciseBase
    {
        #region Accessors
        IAdjustableOrRelativeDate ExpirationDate { get;}
        bool RelevantUnderlyingDateSpecified { get;}
        IAdjustableOrRelativeDates RelevantUnderlyingDate { get;}
        bool ExerciseFeeSpecified { get;}
        IExerciseFee ExerciseFee { get;}
        bool PartialExerciseSpecified { set; get; }
        IPartialExercise PartialExercise { get;}
        IPartialExercise CreatePartialExercise();
        #endregion Accessors
    }
    #endregion IEuropeanExercise
    #region IEvent
    public interface IEvent
    {
        #region Accessors
        ISchemeId[] EventId { set; get;}
        #endregion Accessors
    }
    #endregion IEvent
    #region IExchangeRate
    public interface IExchangeRate
    {
        #region Accessors
        IQuotedCurrencyPair QuotedCurrencyPair { get;}
        EFS_Decimal Rate { set; get;}
        bool SpotRateSpecified { set; get;}
        EFS_Decimal SpotRate { set; get;}
        bool ForwardPointsSpecified { set; get;}
        EFS_Decimal ForwardPoints { set; get;}
        bool FxFixingSpecified { set;get;}
        IFxFixing FxFixing { set; get;}
        bool SideRatesSpecified { set; get;}
        ISideRates SideRates { get;}
        IFxFixing CreateFxFixing { get;}
        #endregion Accessors
        #region Methods
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        #endregion Methods
    }
    #endregion IExchangeRate
    #region IExchangeTraded
    public interface IExchangeTraded : IUnderlyingAsset
    {
        #region Accessors
        bool RelatedExchangeIdSpecified { set; get;}
        IScheme[] RelatedExchangeId { set; get;}
        bool OptionsExchangeIdSpecified { set; get;}
        IScheme[] OptionsExchangeId { set; get;}
        IScheme FirstOptionsExchangeId { set; get;}
        #endregion Accessors
    }
    #endregion IExchangeTraded
    #region IExchangeTradedCalculatedPrice
    /// EG 20140702 New 
    public interface IExchangeTradedCalculatedPrice : IExchangeTraded
    {
        #region Accessors
        bool ConstituentExchangeIdSpecified { set; get; }
        IScheme[] ConstituentExchangeId { set; get; }
        IScheme FirstConstituentExchangeId { set; get; }
        #endregion Accessors
    }
    #endregion IExchangeTradedCalculatedPrice

    #region IExerciseId
    public interface IExerciseId
    {
        #region Accessors
        string Id { set; get;}
        #endregion Accessors
    }
    #endregion IExerciseId
    #region IExercise
    public interface IExercise : IExerciseId
    {
        #region Accessors
        EFS_ExerciseDates Efs_ExerciseDates { set; get;}
        #endregion Accessors
    }
    #endregion IExercise
    #region IExerciseBase
    public interface IExerciseBase
    {
        #region Accessors
        IBusinessCenterTime EarliestExerciseTime { set;get;}
        IBusinessCenterTime ExpirationTime { set;get;}
        IBusinessCenterTime CreateBusinessCenterTime { get;}
        IReference[] CreateNotionalReference(string[] pNotionalReference);
        #endregion Accessors
    }
    #endregion IExerciseBase
    #region IExerciseFeeBase
    public interface IExerciseFeeBase
    {
        #region Members
        IReference PayerPartyReference { get;}
        IReference ReceiverPartyReference { get;}
        IReference NotionalReference { get;}
        IRelativeDateOffset FeePaymentDate { set; get;}
        #endregion Accessors
    }
    #endregion IExerciseFee
    #region IExerciseFee
    public interface IExerciseFee : IExerciseFeeBase
    {
        #region Members
        bool FeeAmountSpecified { set;get;}
        EFS_Decimal FeeAmount { set;get;}
        bool FeeRateSpecified { set;get;}
        EFS_Decimal FeeRate { set;get;}
        #endregion Accessors
    }
    #endregion IExerciseFee
    #region IExerciseFeeSchedule
    public interface IExerciseFeeSchedule : IExerciseFeeBase
    {
        #region Accessors
        bool FeeAmountSpecified { get;}
        IAmountSchedule FeeAmount { get;}
        bool FeeRateSpecified { get;}
        ISchedule FeeRate { get;}
        #endregion Accessors
    }
    #endregion IExerciseFeeSchedule
    #region IExerciseProcedure
    // EG 20150422 [20513] BANCAPERTA New followUpConfirmationSpecified
    public interface IExerciseProcedure
    {
        #region Accessors
        bool ExerciseProcedureAutomaticSpecified { get;}
        IAutomaticExercise ExerciseProcedureAutomatic { get;}
        bool ExerciseProcedureManualSpecified { get;}
        IManualExercise ExerciseProcedureManual { get;}
        bool FollowUpConfirmationSpecified { get; }
        bool FollowUpConfirmation { get;}
        #endregion Accessors
    }
    #endregion IExerciseProcedure
    #region IExerciseProvision
    public interface IExerciseProvision
    {
        #region Accessors
        bool AmericanSpecified { set;get;}
        IAmericanExercise American { set;get;}
        bool BermudaSpecified { set;get;}
        IBermudaExercise Bermuda { set;get;}
        bool EuropeanSpecified { set;get;}
        IEuropeanExercise European { set;get;}
        bool ExerciseNoticeSpecified { set;get;}
        IExerciseNotice[] ExerciseNotice { set;get;}
        bool FollowUpConfirmation { set;get;}
        #endregion Accessors
        #region Methods
        IAmericanExercise CreateAmerican { get;}
        #endregion Methods
    }
    #endregion IExerciseProvision
    #region IExerciseNotice
    public interface IExerciseNotice
    {
        #region Accessors
        IReference PartyReference { set;get;}
        bool ExerciseNoticePartyReferenceSpecified { set;get;}
        IReference ExerciseNoticePartyReference { set;get;}
        IBusinessCenter BusinessCenter { set;get;}
        #endregion Accessors
    }
    #endregion IExerciseNotice
    #region IExpiryDateTime
    public interface IExpiryDateTime
    {
        #region Accessors
        EFS_Date ExpiryDate { set; get;}
        DateTime ExpiryDateTimeValue { get;}
        IHourMinuteTime ExpiryTime { set; get;}
        string BusinessCenter { set; get;}
        bool CutNameSpecified { set; get;}
        string CutName { set; get;}
        IBusinessCenterTime BusinessCenterTime { get;}
        #endregion Accessors
    }
    #endregion IExpiryDateTime
    #region IExtendibleProvision
    public interface IExtendibleProvision : IExerciseProvision, IProvision
    {
        #region Accessors
        object EFS_Exercise { get;}
        IReference BuyerPartyReference { set; get;}
        IReference SellerPartyReference { set; get;}
        EFS_ExerciseDates Efs_ExerciseDates { set; get;}
        #endregion Accessors
    }
    #endregion IExtendibleProvision
    #region IExtraordinaryEvents
    public interface IExtraordinaryEvents
    {
        #region Accessors
        bool MergerEventsSpecified { get;}
        #endregion Accessors
    }
    #endregion IExtraordinaryEvents

    #region IFailureToPay
    public interface IFailureToPay
    {
        #region Accessors
        bool GracePeriodExtensionSpecified { set;get;}
        IGracePeriodExtension GracePeriodExtension { set;get;}
        bool PaymentRequirementSpecified { set;get;}
        IMoney PaymentRequirement { set;get;}
        #endregion Accessors
    }
    #endregion IFailureToPay
    #region IFeaturePayment
    public interface IFeaturePayment
    {
        #region Accessors
        IReference PayerPartyReference { set;get;}
        IReference ReceiverPartyReference { set;get;}
        bool LevelPercentageSpecified { set;get;}
        EFS_Decimal LevelPercentage { set;get;}
        bool AmountSpecified { set;get;}
        EFS_Decimal Amount { set;get;}
        bool TimeSpecified { set;get;}
        TimeTypeEnum Time { set;get;}
        bool CurrencySpecified { set;get;}
        ICurrency Currency { set;get;}
        bool FeaturePaymentDateSpecified { set;get;}
        IAdjustableOrRelativeDate FeaturePaymentDate { set;get;}
        #endregion Accessors
    }
    #endregion IFeaturePayment
    #region IFloatingRate
    public interface IFloatingRate
    {
        #region Accessors
        bool CapRateScheduleSpecified { set; get;}
        IStrikeSchedule[] CapRateSchedule { set; get;}
        bool FloorRateScheduleSpecified { set; get;}
        IStrikeSchedule[] FloorRateSchedule { set; get;}
        IFloatingRateIndex FloatingRateIndex { get;}
        bool SpreadScheduleSpecified { set; get;}
        ISchedule SpreadSchedule { set; get;}
        bool FloatingRateMultiplierScheduleSpecified { set; get;}
        ISchedule FloatingRateMultiplierSchedule { get;}
        bool RateTreatmentSpecified { get;}
        RateTreatmentEnum RateTreatment { get;}
        ISchedule CreateSchedule();
        ISpreadSchedule CreateSpreadSchedule();
        IStrikeSchedule[] CreateStrikeSchedule(int pDim);
        bool IndexTenorSpecified { set; get;}
        IInterval IndexTenor { set; get;}
        // EG 20150309 POC - BERKELEY
        ISpreadSchedule[] LstSpreadSchedule { set; get; }
        #endregion Accessors
        #region Methods
        SQL_AssetRateIndex GetSqlAssetRateIndex(string pCs);
        #endregion Methods
    }
    #endregion IFloatingRate
    #region IFloatingRateCalculation
    public interface IFloatingRateCalculation : IFloatingRate
    {
        #region Accessors
        // EG 20161116 Gestion InitialRate (RATP)
        bool InitialRateSpecified { set; get; }
        EFS_Decimal InitialRate { set; get; }
        bool FinalRateRoundingSpecified { get; }
        IRounding FinalRateRounding { get;}
        bool AveragingMethodSpecified { get;}
        AveragingMethodEnum AveragingMethod { get;}
        bool NegativeInterestRateTreatmentSpecified { set;  get; }
        NegativeInterestRateTreatmentEnum NegativeInterestRateTreatment { set; get; }
        ISpreadSchedule[] CreateSpreadSchedules(ISpreadSchedule[] pSpreadSchedule);
        #endregion Accessors
    }
    #endregion IFloatingRateCalculation
    #region IFloatingRateIndex
    public interface IFloatingRateIndex
    {
        #region Accessors
        int OTCmlId { set; get;}
        bool HrefSpecified { set; get;}
        string Href { set; get;}
        string Value { set;get;}
        #endregion Accessors
    }
    #endregion IFloatingRateIndex
    #region IFormula
    public interface IFormula
    {
        #region Accessors
        bool FormulaDescriptionSpecified { set;get;}
        EFS_MultiLineString FormulaDescription { set;get;}
        bool MathSpecified { set;get;}
        IMath Math { set;get;}
        bool FormulaComponentSpecified { set;get;}
        IFormulaComponent[] FormulaComponent { set;get;}
        #endregion Accessors
    }
    #endregion IFormula
    #region IFormulaComponent
    public interface IFormulaComponent
    {
        #region Accessors
        EFS_MultiLineString ComponentDescription { set;get;}
        bool FormulaSpecified { set;get;}
        IFormula Formula { set;get;}
        string Name { set;get;}
        string Href { set;get;}
        #endregion Accessors
    }
    #endregion IFormulaComponent
    #region IFra
    public interface IFra
    {
        #region Accessors
        IReference BuyerPartyReference { get;}
        IReference SellerPartyReference { get;}
        IRequiredIdentifierDate AdjustedEffectiveDate { set; get;}
        EFS_Date AdjustedTerminationDate { set; get;}
        IAdjustableDate PaymentDate { get;}
        IRelativeDateOffset FixingDateOffset { get;}
        DayCountFractionEnum DayCountFraction { set; get;}
        EFS_PosInteger CalculationPeriodNumberOfDays { set;get;}
        IMoney Notional { get;}
        EFS_Decimal FixedRate { set; get;}
        IFloatingRateIndex FloatingRateIndex { set; get;}
        bool IndexTenorSpecified { get;}
        IInterval[] IndexTenor { set; get;}
        IInterval FirstIndexTenor { set; get;}
        FraDiscountingEnum FraDiscounting { get;}
        EFS_FraDates Efs_FraDates { set; get;}
        #endregion Accessors
    }
    #endregion IFra
    #region IFuture
    public interface IFuture : IExchangeTraded
    {
        #region Accessors
        bool MultiplierSpecified { set;get;}
        EFS_Integer Multiplier { set;get;}
        bool FutureContractReferenceSpecified { set;get;}
        EFS_String FutureContractReference { set;get;}
        bool MaturitySpecified { set;get;}
        EFS_Date Maturity { set;get;}
        #endregion Accessors
    }
    #endregion IFuture
    #region IFxAmericanTrigger
    public interface IFxAmericanTrigger : IFxTrigger
    {
        #region Accessors
        bool ObservationStartDateSpecified { set; get;}
        EFS_Date ObservationStartDate { set; get;}
        bool ObservationEndDateSpecified { set; get;}
        EFS_Date ObservationEndDate { set; get;}
        TouchConditionEnum TouchCondition { set; get;}
        #endregion Accessors
    }
    #endregion IFxAmericanTrigger
    #region IFxAverageRateObservationDate
    public interface IFxAverageRateObservationDate
    {
        #region Accessors
        DateTime ObservationDate { get;}
        decimal AverageRateWeightingFactor { get;}
        #endregion Accessors
    }
    #endregion IFxAverageRateObservationDate
    #region IFxAverageRateObservationSchedule
    public interface IFxAverageRateObservationSchedule
    {
        #region Accessors
        EFS_Date ObservationStartDate { get;}
        EFS_Date ObservationEndDate { get;}
        ICalculationPeriodFrequency CalculationPeriodFrequency { set; get;}
        #endregion Accessors
    }
    #endregion IFxAverageRateObservationSchedule
    #region IFxAverageRateOption
    public interface IFxAverageRateOption : IFxOptionBaseNotDigital, IFxOptionBase
    {
        #region Accessors
        bool SpotRateSpecified { set; get;}
        EFS_Decimal SpotRate { set; get;}
        ICurrency PayoutCurrency { set; get;}
        bool PayoutFormulaSpecified { set; get;}
        string PayoutFormula { set; get;}
        IInformationSource PrimaryRateSource { set; get;}
        StrikeQuoteBasisEnum AverageRateQuoteBasis { set;get;}
        bool SecondaryRateSourceSpecified { set;get;}
        IInformationSource SecondaryRateSource { set; get;}
        IBusinessCenterTime FixingTime { set;get;}
        bool AverageStrikeOptionSpecified { set; get;}
        IAverageStrikeOption AverageStrikeOption { set; get;}
        bool BermudanExerciseDatesSpecified { get;}
        IDateList BermudanExerciseDates { get;}
        bool ObservedRatesSpecified { get;}
        IObservedRates[] ObservedRates { get;}
        bool RateObservationDateSpecified { set;get;}
        IFxAverageRateObservationDate[] RateObservationDate { set;get;}
        bool RateObservationScheduleSpecified { set;get;}
        IFxAverageRateObservationSchedule RateObservationSchedule { set;get;}
        bool GeometricAverageSpecified { set;get;}
        bool PrecisionSpecified { set;get;}
        EFS_PosInteger Precision { set;get;}
        EFS_FxAverageRateOption Efs_FxAverageRateOption { set; get;}
        IInformationSource CreateInformationSource { get;}
        IFxAverageRateObservationSchedule CreateFxAverageRateObservationSchedule { get;}
        IFxAverageRateObservationDate[] CreateFxAverageRateObservationDates { get;}
        #endregion Accessors
    }
    #endregion IFxAverageRateOption
    #region IFxBarrier
    public interface IFxBarrier
    {
        #region Accessors
        FxBarrierTypeEnum FxBarrierType { set; get;}
        bool FxBarrierTypeSpecified { set;get;}
        IInformationSource[] InformationSource { set; get;}
        EFS_Decimal TriggerRate { set; get;}
        bool ObservationStartDateSpecified { set;get;}
        EFS_Date ObservationStartDate { set;get;}
        bool ObservationEndDateSpecified { set;get;}
        EFS_Date ObservationEndDate { set;get;}
        IQuotedCurrencyPair QuotedCurrencyPair { set;get;}
        IInformationSource[] CreateInformationSources { get;}
        #endregion Accessors
    }
    #endregion IFxBarrier
    #region IFxBarrierOption
    public interface IFxBarrierOption : IFxOptionBaseNotDigital, IFxOptionBase
    {
        #region Accessors
        bool BermudanExerciseDatesSpecified { get;}
        IDateList BermudanExerciseDates { get;}
        IFxCashSettlement CashSettlementTerms { set; get; }
        bool CashSettlementTermsSpecified { get;}
        bool SpotRateSpecified { set; get;}
        EFS_Decimal SpotRate { set; get;}
        IFxBarrier[] FxBarrier { get;}
        bool FxRebateBarrierSpecified { set; get;}
        IFxBarrier FxRebateBarrier { get;}
        bool CappedCallOrFlooredPutSpecified { get;}
        ICappedCallOrFlooredPut CappedCallOrFlooredPut { get;}
        bool TriggerPayoutSpecified { set; get;}
        IFxOptionPayout TriggerPayout { get;}
        EFS_FxBarrierOption Efs_FxBarrierOption { set; get;}
        #endregion Accessors
    }
    #endregion IFxBarrierOption
    #region IFxCashSettlement
    /// <summary>
    /// A type that is used for describing cash settlement of an option / non deliverable forward. It includes the currency to settle into together with the fixings required to calculate the currency amount
    /// </summary>
    public interface IFxCashSettlement
    {
        #region Accessors
        /// <summary>
        /// The currency in which a cash settlement for non-deliverable forward and non-deliverable options.
        /// </summary>
        ICurrency SettlementCurrency { get;}
        /// <summary>
        /// Specifies the source for and timing of a fixing of an exchange rate. This is used in the agreement of non-deliverable forward trades as well as various types of FX OTC options that require observations against a particular rate.
        /// </summary>
        IFxFixing[] Fixing { get;}
        
        //PL 20100628 customerSettlementRateSpecified à supprimer plus tard...
        bool CustomerSettlementRateSpecified { set;get;}
        bool CalculationAgentSettlementRateSpecified { set;get;}
        #endregion Accessors

        void Initialize();
    }
    #endregion IFxCashSettlement
    #region IFxDigitalOption
    public interface IFxDigitalOption : IFxOptionBase
    {
        #region Accessors
        IQuotedCurrencyPair QuotedCurrencyPair { get;}
        bool TypeTriggerEuropeanSpecified { set; get;}
        IFxEuropeanTrigger[] TypeTriggerEuropean { get;}
        bool TypeTriggerAmericanSpecified { set; get;}
        IFxAmericanTrigger[] TypeTriggerAmerican { get;}
        IFxOptionPayout TriggerPayout { get;}
        bool SpotRateSpecified { set; get;}
        EFS_Decimal SpotRate { set; get;}
        bool ResurrectingSpecified { get;}
        IPayoutPeriod Resurrecting { get;}
        bool ExtinguishingSpecified { get;}
        IPayoutPeriod Extinguishing { get;}
        bool FxBarrierSpecified { set; get;}
        IFxBarrier[] FxBarrier { get;}
        bool BoundarySpecified { get;}
        bool LimitSpecified { get;}
        bool AssetOrNothingSpecified { get;}
        IAssetOrNothing AssetOrNothing { get;}
        EFS_EventDate ExpiryDate { get;}
        EFS_FxDigitalOption Efs_FxDigitalOption { set;get;}
        #endregion Accessors
    }
    #endregion IFxDigitalOption
    #region IFxEuropeanTrigger
    public interface IFxEuropeanTrigger : IFxTrigger
    {
        #region Accessors
        TriggerConditionEnum TriggerCondition { set; get;}
        #endregion Accessors
    }
    #endregion IFxEuropeanTrigger
    #region IFxFeature
    public interface IFxFeature
    {
        #region Accessors
        ISchemeId ReferenceCurrency { set; get;}
        bool FxFeatureCompositeSpecified { set; get;}
        IComposite FxFeatureComposite { set;  get; }
        bool FxFeatureQuantoSpecified { set; get; }
        IQuanto FxFeatureQuanto { set; get; }
        bool FxFeatureCrossCurrencySpecified { set; get; }
        IComposite FxFeatureCrossCurrency { set; get; }
        #endregion Accessors
    }
    #endregion IFxFeature
    #region IFxFixing
    /// <summary>
    /// A type that specifies the source for and timing of a fixing of an exchange rate. 
    /// </summary>
    public interface IFxFixing
    {
        #region Accessors
        /// <summary>
        /// Describes the specific date when a non-deliverable forward or non-deliverable option will \"fix\" against a particular rate, which will be used to compute the ultimate cash settlement.
        /// </summary>
        EFS_Date FixingDate { set; get;}
        /// <summary>
        /// 'The time at which the spot currency exchange rate will be observed. 
        /// <para>It is specified as a time in a specific business center, e.g. 11:00am London time.'</para>
        /// </summary>
        IBusinessCenterTime FixingTime { set; get;}
        /// <summary>
        /// Defines the two currencies for an FX trade and the quotation relationship between the two currencies.
        /// </summary>
        IQuotedCurrencyPair QuotedCurrencyPair { set; get;}
        /// <summary>
        /// 'The primary source for where the rate observation will occur. Will typically be either a page or a reference bank published rate.'
        /// </summary>
        IInformationSource PrimaryRateSource { set; get;}
        bool SecondaryRateSourceSpecified { set; get;}
        /// <summary>
        /// 'An alternative, or secondary, source for where the rate observation will occur. Will typically be either a page or a reference bank published rate.'
        /// </summary>
        IInformationSource SecondaryRateSource { set; get;}

        IInformationSource CreateInformationSource();
        IBusinessCenterTime CreateBusinessCenterTime();
        IBusinessCenterTime CreateBusinessCenterTime(IBusinessCenterTime pBusinessCenterTime);
        IQuotedCurrencyPair CreateQuotedCurrencyPair(string pCurrency1, string pCurrency2, QuoteBasisEnum pQuoteBasis);
        IFxFixing Clone();
        #endregion Accessors
    }
    #endregion IFxFixing
    #region IFxLeg
    /// <summary>
    /// A single-legged FX transaction definition (e.g., spot or forward).
    /// </summary>
    /// FI 20150331 [XXPOC] Modify
    public interface IFxLeg
    {
        #region Accessors
        /// <summary>
        /// This is the first of the two currency flows that define a single leg of a standard foreign exchange transaction.
        /// </summary>
        IPayment ExchangedCurrency1 { set;get;}
        /// <summary>
        /// This is the second of the two currency flows that define a single leg of a standard foreign exchange transaction.
        /// </summary>
        IPayment ExchangedCurrency2 { set;get;}
        /// <summary>
        /// The rate of exchange between the two currencies.
        /// </summary>
        IExchangeRate ExchangeRate { set;get;}
        
        bool FxDateValueDateSpecified { set; get;}
        /// <summary>
        /// The date on which both currencies traded will settle.
        /// </summary>
        EFS_Date FxDateValueDate { get;}
        
        bool FxDateCurrency1ValueDateSpecified { set; get;}
        /// <summary>
        /// The date on which the currency1 amount will be settled. To be used in a split value date scenario.
        /// </summary>
        EFS_Date FxDateCurrency1ValueDate { get;}
        
        bool FxDateCurrency2ValueDateSpecified { set; get;}
        /// <summary>
        /// The date on which the currency2 amount will be settled. To be used in a split value date scenario
        /// </summary>
        EFS_Date FxDateCurrency2ValueDate { get;}
        
        bool NonDeliverableForwardSpecified { set; get;}
        /// <summary>
        /// Used to describe a particular type of FX forward transaction that is settled in a single currency.
        /// </summary>
        IFxCashSettlement NonDeliverableForward { get;}
        /// EG 20150402 [POC] Add
        bool MarginRatioSpecified { set; get; }
        IMarginRatio MarginRatio { set; get; }
        IMarginRatio CreateMarginRatio { get; }

        EFS_FxLeg Efs_FxLeg { set; get;}
        IPayment CreatePayment { get;}
        IExchangeRate CreateExchangeRate { get;}
        // FI 20150331 [POCXX] Add
        // FI 20170116 [21916] RptSide (R majuscule)
        IFixTrdCapRptSideGrp[] RptSide { set; get; }

        #endregion Accessors
    }
    #endregion IFxLeg
    #region IFxLinkedNotionalSchedule
    public interface IFxLinkedNotionalSchedule
    {
        #region Accessors
        EFS_FxLinkedNotionalDates Efs_FxLinkedNotionalDates { set;get;}
        string Currency { set; get;}
        IRelativeDateOffset VaryingNotionalInterimExchangePaymentDates { set; get;}
        IRelativeDateOffset VaryingNotionalFixingDates { set; get;}
        IReference ConstantNotionalScheduleReference { set; get;}
        bool InitialValueSpecified { set; get;}
        EFS_Decimal InitialValue { set; get;}
        IFxSpotRateSource FxSpotRateSource { set; get;}
        #endregion Accessors
        #region Methods
        SQL_AssetFxRate GetSqlAssetFxRate(string pConnectionString);
        #endregion Methods
    }
    #endregion IFxLinkedNotionalSchedule
    #region IFxOptionBase
    public interface IFxOptionBase
    {
        #region Accessors
        /// <summary>
        /// 'The date and time in a location of the option expiry. 
        /// <para>In the case of american options this is the latest possible expiry date and time.'</para>
        /// </summary>
        IExpiryDateTime ExpiryDateTime { get;}
        /// <summary>
        /// 'The date on which both currencies traded will settle.'
        /// </summary>
        EFS_Date ValueDate { get;}

        IReference BuyerPartyReference { get; set; }
        IReference SellerPartyReference { get; set; }
        
        bool OptionTypeSpecified { set; get;}
        OptionTypeEnum OptionType { set; get;}
        
        bool CallCurrencyAmountSpecified { get;}
        /// <summary>
        /// 'The currency amount that the option gives the right to buy.'
        /// </summary>
        IMoney CallCurrencyAmount { get;}
        
        bool PutCurrencyAmountSpecified { get;}
        /// <summary>
        /// 'The currency amount that the option gives the right to sell.'
        /// </summary>
        IMoney PutCurrencyAmount { get;}
        
        bool FxOptionPremiumSpecified { set; get;}
        IFxOptionPremium[] FxOptionPremium { get;}
        #endregion Accessors
    }
    #endregion IFxOptionBase
    #region IFxOptionBaseNotDigital
    public interface IFxOptionBaseNotDigital
    {
        #region Accessors
        ExerciseStyleEnum ExerciseStyle { get;}
        bool ProcedureSpecified { get;}
        IExerciseProcedure Procedure { get;}
        IFxStrikePrice FxStrikePrice { get;}
        #endregion Accessors
    }
    #endregion IFxOptionBaseNotDigital
    #region IFxOptionLeg
    public interface IFxOptionLeg : IFxOptionBaseNotDigital, IFxOptionBase
    {
        #region Accessors
        bool BermudanExerciseDatesSpecified { get; set; }
        IDateList BermudanExerciseDates { get; set; }
        
        bool CashSettlementTermsSpecified { get; set; }
        /// <summary>
        /// This optional element is only used if an option has been specified at execution time to be settled into a single cash payment. This would be used for a non-deliverable option.'
        /// </summary>
        IFxCashSettlement CashSettlementTerms { get; set; }
        EFS_FxSimpleOption Efs_FxSimpleOption { set; get; }
        // FI 20150331 [POC] Add (Peut-être faudra-t-il mettre cet élément dans IFxOptionBase
        // FI 20170116 [21916] RptSide (R majuscule)
        IFixTrdCapRptSideGrp[] RptSide { set; get; }
        /// EG 20150402 [POC] Add
        bool MarginRatioSpecified { set; get; }
        IMarginRatio MarginRatio { set; get; }
        IMarginRatio CreateMarginRatio { get; }

        #endregion Accessors
    }
    #endregion IFxOptionLeg
    #region IFxOptionPayout
    public interface IFxOptionPayout
    {
        #region Accessors
        PayoutEnum PayoutStyle { set; get;}
        EFS_Decimal Amount { get;}
        string Currency { set; get;}
        bool SettlementInformationSpecified { set; get;}
        ISettlementInformation SettlementInformation { get;}
        bool CustomerSettlementPayoutSpecified { set; get;}
        ICustomerSettlementPayment CustomerSettlementPayout { set;get;}
        ICustomerSettlementPayment CreateCustomerSettlementPayment { get;}
        #endregion Accessors
        #region Methods
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        #endregion Methods
    }
    #endregion IFxOptionPayout
    #region IFxOptionPremium
    public interface IFxOptionPremium
    {
        #region Accessors
        IReference PayerPartyReference { get; set;}
        IReference ReceiverPartyReference { get; set;}
        EFS_Date PremiumSettlementDate { set; get;}
        bool CustomerSettlementPremiumSpecified { set; get;}
        ICustomerSettlementPayment CustomerSettlementPremium { set;get;}
        IMoney PremiumAmount { set; get;}
        bool PremiumQuoteSpecified { set; get;}
        IPremiumQuote PremiumQuote { get;}
        EFS_FxOptionPremium Efs_FxOptionPremium { set;get;}
        bool SettlementInformationSpecified { set; get;}
        ISettlementInformation SettlementInformation { get;}
        ICustomerSettlementPayment CreateCustomerSettlementPayment { get;}
        #endregion Accessors
        #region Methods
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        #endregion Methods
    }
    #endregion IFxOptionPremium
    #region IFxRate
    public interface IFxRate
    {
        #region Accessors
        IQuotedCurrencyPair QuotedCurrencyPair { set;get;}
        EFS_Decimal Rate { set;get;}
        #endregion Accessors
    }
    #endregion IFxRate
    #region IFxRateAsset
    public interface IFxRateAsset : IUnderlyingAsset
    {
        #region Accessors
        IQuotedCurrencyPair QuotedCurrencyPair { set; get; }
        bool RateSourceSpecified { set; get; }
        IFxSpotRateSource RateSource { set; get; }
        #endregion Accessors
        #region Methods
        void CreateQuotedCurrencyPair(string pCurrency1, string pCurrency2, QuoteBasisEnum pQuoteBasis);
        #endregion Methods
    }
    #endregion IFxRateAsset

    #region IFxSpotRateSource
    public interface IFxSpotRateSource
    {
        #region Accessors
        IInformationSource PrimaryRateSource { set; get;}
        bool SecondaryRateSourceSpecified { set; get;}
        IInformationSource SecondaryRateSource { set; get;}
        IBusinessCenterTime FixingTime { set; get;}
        #endregion Accessors
        #region Methods
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        IFxFixing CreateFxFixing(string pCurrency1, string pCurrency2, DateTime pFixingDate);
        IInformationSource CreateInformationSource { get;}
        #endregion Methods
    }
    #endregion IFxSpotRateSource
    #region IFxStrikePrice
    public interface IFxStrikePrice
    {
        #region Accessors
        EFS_Decimal Rate { set; get;}
        StrikeQuoteBasisEnum StrikeQuoteBasis { set;get;}
        #endregion Accessors
    }
    #endregion IFxStrikePrice
    #region IFxSwap
    public interface IFxSwap
    {
        #region Accessors
        IFxLeg[] FxSingleLeg { get;}
        #endregion Accessors
    }
    #endregion IFxSwap
    #region IFxTrigger
    public interface IFxTrigger
    {
        #region Accessors
        EFS_Decimal TriggerRate { set; get;}
        IQuotedCurrencyPair QuotedCurrencyPair { set;get;}
        IInformationSource[] InformationSource { set;get;}
        IInformationSource[] CreateInformationSources { get;}
        #endregion Accessors
    }
    #endregion IFxTrigger

    #region IGracePeriodExtension
    public interface IGracePeriodExtension
    {
        #region Accessors
        IOffset GracePeriod { set;get;}
        #endregion Accessors
    }
    #endregion IGracePeriodExtension

    #region IHourMinuteTime
    public interface IHourMinuteTime
    {
        #region Accessors
        string Value { set; get;}
        DateTime TimeValue { set;get;}
        #endregion Accessors
    }
    #endregion IHourMinuteTime

    #region IIndex
    /// EG 20140702 New 
    public interface IIndex : IExchangeTraded
    {
    }
    #endregion IIndex

    #region IInflationRateCalculation
    public interface IInflationRateCalculation : IFloatingRateCalculation
    {
        #region Accessors
        #endregion Accessors
    }
    #endregion IInflationRateCalculation
    #region IInformationSource
    /// <summary>
    /// A type defining the source for a piece of information (e.g. a rate refix or an fx fixing).
    /// </summary>
    public interface IInformationSource
    {
        #region Accessors
        IAssetFxRateId AssetFxRateId { get;}
        int OTCmlId { set;get;}
        /// <summary>
        /// 'An information source for obtaining a market rate. For example Bloomberg, Reuters, Telerate etc.'
        /// </summary>
        IScheme RateSource { set; get;}
        
        bool RateSourcePageSpecified { set; get;}
        /// <summary>
        /// A specific page for the rate source for obtaining a market rate.
        /// </summary>
        IScheme RateSourcePage { set; get;}
        
        bool RateSourcePageHeadingSpecified { set; get;}
        /// <summary>
        /// The heading for the rate source on a given rate source page.
        /// </summary>
        string RateSourcePageHeading { set; get;}
        
        #endregion Accessors
        #region Methods
        void CreateRateSourcePage(string pRateSourcePage);
        void SetAssetFxRateId(int pIdAsset, string pIdentifier);
        #endregion Methods
    }
    #endregion IInformationSource
    #region IInterestAccrualsMethod
    /// EG 20140702 Add fixedRateSpecified|fixedRate|sqlAssetSpecified|sqlAsset
    // EG 20170510 [23153] Add SetAsset 
    public interface IInterestAccrualsMethod
    {
        #region Accessors
        bool FloatingRateSpecified { set; get;}
        IFloatingRateCalculation FloatingRate { set; get;}
        bool FixedRateSpecified { set;  get; }
        EFS_Decimal FixedRate { set; get; }
        bool SqlAssetSpecified { get; }
        SQL_AssetBase SqlAsset { set; get; }
        // EG 20170510 [23153] new
        void SetAsset(string pCS, IDbTransaction pDbTransaction);
        #endregion Accessors
    }
    #endregion IInterestCalculation
    #region IInterestCalculation
    public interface IInterestCalculation : IInterestAccrualsMethod
    {
        #region Accessors
        bool CompoundingSpecified { get;}
        ICompounding Compounding { get; }
        DayCountFractionEnum DayCountFraction { set; get;}
        #endregion Accessors
    }
    #endregion IInterestCalculation
    #region IInterestLeg
    /// EG 20140702 Upd Type paymentDates
    /// EG 20140702 Add CreateResetDates|CreateResetFrequency|CreateRelativeDateOffset|CreatePaymentDates|efs_InterestLeg
    public interface IInterestLeg : IReturnSwapLeg
    {
        #region Accessors
        IReturnSwapLeg ReturnSwapLeg { get;}
        IInterestCalculation InterestCalculation { set; get;}
        IInterestLegCalculationPeriodDates CalculationPeriodDates { set; get;}
        ILegAmount InterestAmount { set; get;}
        bool StubCalculationPeriodSpecified { get;}
        IStubCalculationPeriod StubCalculationPeriod { get;}
        IAdjustableRelativeOrPeriodicDates2 PaymentDates { get; }
        IReturnSwapNotional Notional { get;}
        IInterestLegCalculationPeriodDates CreateCalculationPeriodDates { get;}
        IInterestCalculation CreateInterestCalculation { get;}
        ILegAmount CreateLegAmount { get;}
        IInterestLegResetDates CreateResetDates { get; }
        IResetFrequency CreateResetFrequency { get; }
        IRelativeDateOffset CreateRelativeDateOffset { get; }
        IAdjustableRelativeOrPeriodicDates2 CreatePaymentDates { get; }
        EFS_InterestLeg Efs_InterestLeg { set; get; }
        bool IsPeriodRelativeToReturnLeg {get;}
        #endregion Accessors
    }
    #endregion IInterestLeg

    #region IInterestLegCalculationPeriodDates
    /// EG 20140702 Add paymentDates|resetDates
    public interface IInterestLegCalculationPeriodDates
    {
        #region Accessors
        IAdjustableOrRelativeDate EffectiveDate { get;}
        IAdjustableOrRelativeDate TerminationDate { get;}
        IAdjustableRelativeOrPeriodicDates2 PaymentDates { set; get; }
        IInterestLegResetDates ResetDates { set; get; }
        string Id { set; get; }
        #endregion Accessors
    }
    #endregion IInterestLegCalculationPeriodDates
    #region IInterestLegResetDates
    /// EG 20140702 Add initialFixingDate|initialFixingDateSpecified|fixingDates|fixingDatesSpecified
    public interface IInterestLegResetDates
    {
        #region Accessors
        IReference CalculationPeriodDatesReference { set; get; }
        bool ResetRelativeToSpecified { set; get; }
        ResetRelativeToEnum ResetRelativeTo { set; get; }
        bool ResetFrequencySpecified { set; get; }
        IResetFrequency ResetFrequency { set; get; }
        IRelativeDateOffset InitialFixingDate { set; get; }
        bool InitialFixingDateSpecified { set; get; }
        IAdjustableDatesOrRelativeDateOffset FixingDates { set; get; }
        bool FixingDatesSpecified { set; get; }

        #endregion Accessors
    }
    #endregion IInterestLegResetDates
    #region IInterestRateStream
    public interface IInterestRateStream
    {
        #region Accessors
        IReference PayerPartyReference { get; set;}
        IReference ReceiverPartyReference { get; set; }
        ICalculationPeriodAmount CalculationPeriodAmount { get;}
        bool StubCalculationPeriodAmountSpecified { set; get;}
        IStubCalculationPeriodAmount StubCalculationPeriodAmount { set; get;}
        bool ResetDatesSpecified { set; get;}
        ICalculationPeriodDates CalculationPeriodDates { set; get;}
        IPaymentDates PaymentDates { set;get;}
        IResetDates ResetDates { set;get;}
        string Id { set;get;}
        bool PrincipalExchangesSpecified { set;get;}
        IPrincipalExchanges PrincipalExchanges { get;}

        EFS_Date AdjustedEffectiveDate {get;}
        EFS_Date AdjustedTerminationDate {get;}
        /// <summary>
        /// Obtient calculationPeriodAmountCalculation ou calculationPeriodAmountKnownAmountSchedule
        /// </summary>
        object GetCalculationPeriodAmount  {get;}
        string DayCountFraction {get;}
        EFS_EventDate EffectiveDate { get;}
        EFS_AdjustableDate EffectiveDateAdjustment { get;}
        string EventGroupName {get;}
        
        /// <summary>
        /// Obtient true si au minimum un capRateSchedule est spécifé
        /// </summary>
        bool IsCapped  {get;}
        
        /// <summary>
        /// Obtient true si au minimum un floorRateSchedule est spécifé
        /// </summary>
        bool IsFloored  {get;}
        
        /// <summary>
        /// Obtient true si au minimum un capRateSchedule ou un floorRateSchedule est spécifé
        /// </summary>
        bool IsCapFloored  {get;}
        

        bool IsInitialExchange {get;}
        bool IsIntermediateExchange  {get;}
        bool IsFinalExchange  {get;}
        string GetPayerPartyReference { get;}
        string GetReceiverPartyReference  {get;}
        string StreamCurrency  {get;}
        EFS_EventDate TerminationDate  {get;}
        EFS_AdjustableDate TerminationDateAdjustment  {get;}
        EFS_Date UnadjustedEffectiveDate  {get;}
        EFS_Date UnadjustedTerminationDate  {get;}
        string GetCurrency { get;}
        #endregion Accessors
        #region Methods
        IRelativeDateOffset CreateRelativeDateOffset();
        IResetFrequency CreateResetFrequency();
        IInterval CreateInterval();
        IOffset CreateOffset();
        IStubCalculationPeriodAmount CreateStubCalculationPeriodAmount();
        IResetDates CreateResetDates();
        IBusinessDayAdjustments CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, string pIdBC);
        IBusinessDayAdjustments CreateBusinessDayAdjustments();
        string EventType(string pProduct);
        string EventType2(string pProduct, bool pIsDiscount);
        object Rate(string pStub);
        #endregion Methods
    }
    #endregion IInterestRateStream
    #region IIntermediaryInformation
    public interface IIntermediaryInformation : IRouting
    {
        #region Accessors
        EFS_PosInteger IntermediarySequenceNumber { set; get;}
        #endregion Accessors
    }
    #endregion IIntermediaryInformation
    #region IInterval
    public interface IInterval
    {
        #region Accessors
        PeriodEnum Period { set; get;}
        EFS_Integer PeriodMultiplier { set; get;}
        #endregion Accessors
        #region Members
        IInterval GetInterval(int pMultiplier, PeriodEnum pPeriod);
        IRounding GetRounding(RoundingDirectionEnum pRoundingDirection, int pPrecision);
        int CompareTo(object obj);
        #endregion Members
    }
    #endregion IInterval

    #region IKnock
    public interface IKnock
    {
        #region Accessors
        bool KnockInSpecified { set;get;}
        ITriggerEvent KnockIn { set;get;}
        bool KnockOutSpecified { set;get;}
        ITriggerEvent KnockOut { set;get;}
        #endregion Accessors
    }
    #endregion IKnock

    #region ILegAmount
    // EG 20231127 [WI755] Implementation Return Swap
    public interface ILegAmount
    {
        #region Accessors
        bool PaymentCurrencySpecified { get;}
        IPaymentCurrency PaymentCurrency { get;}
        bool ReferenceAmountSpecified { set;get;}
        IScheme ReferenceAmount { get;}
        bool CalculationDatesSpecified { get;}
        IAdjustableRelativeOrPeriodicDates CalculationDates { get;}
        bool FormulaSpecified { get; }
        IFormula Formula { get; }
        bool CurrencyDeterminationMethodSpecified { get; }
        IScheme CurrencyDeterminationMethod { get; }
        bool CurrencySpecified { get; }
        ICurrency Currency { get; }
        bool CurrencyReferenceSpecified { get; }
        IReference CurrencyReference { get; }
        string MainLegAmountCurrency { get; }
        #endregion Accessors
    }
    #endregion ILegAmount
    #region ILegCurrency
    public interface ILegCurrency
    {
        #region Accessors
        bool CurrencySpecified { get;}
        ICurrency Currency { get;}
        bool DeterminationMethodSpecified { get;}
        IScheme DeterminationMethod { get;}
        bool CurrencyReferenceSpecified { get;}
        IReference CurrencyReference { get;}
        #endregion Accessors
    }
    #endregion ILegCurrency
    #region ILinkId
    public interface ILinkId
    {
        #region Accessors
        string LinkIdScheme { set;get;}
        string Value { set; get;}
        decimal Factor { set; get;}
        string StrFactor { set; get;}
        string Id { set; get;}
        #endregion Accessors
    }
    #endregion ILinkId
    #region IMakeWholeAmount
    // EG 20150410 [20513] BANCAPERTA
    public interface IMakeWholeAmount : ISwapCurveValuation
    {
        #region Accessors
        bool InterpolationMethodSpecified { set; get; }
        IScheme InterpolationMethod { set; get; }
        IAdjustedDate EarlyCallDate { set; get; }
        #endregion Accessors
    }
    #endregion IMakeWholeAmount

    #region IMakeWholeProvisions
    public interface IMakeWholeProvisions
    {
        #region Accessors
        EFS_Date MakeWholeDate { set;get;}
        EFS_Decimal RecallSpread { set;get;}
        #endregion Accessors
    }
    #endregion IMakeWholeProvisions
    #region IManualExercise
    public interface IManualExercise
    {
        #region Accessors
        bool ExerciseNoticeSpecified { get;}
        bool FallbackExerciseSpecified { get;}
        bool FallbackExercise { get;}
        #endregion Accessors
    }
    #endregion IManualExercise
    #region IMandatoryEarlyTermination
    public interface IMandatoryEarlyTermination
    {
        #region Accessors
        IAdjustableDate MandatoryEarlyTerminationDate { get;}
        ICashSettlement CashSettlement { get;}
        bool AdjustedDatesSpecified { get;}
        IMandatoryEarlyTerminationAdjustedDates AdjustedDates { get;}
        EFS_MandatoryEarlyTerminationDates Efs_MandatoryEarlyTerminationDates { set;get;}
        #endregion Accessors
    }
    #endregion IMandatoryEarlyTermination
    #region IMandatoryEarlyTerminationAdjustedDates
    public interface IMandatoryEarlyTerminationAdjustedDates
    {
        #region Accessors
        DateTime AdjustedEarlyTerminationDate { get;}
        DateTime AdjustedCashSettlementPaymentDate { get;}
        DateTime AdjustedCashSettlementValuationDate { get;}
        #endregion Accessors
    }
    #endregion IMandatoryEarlyTerminationAdjustedDates
    #region IMasterAgreement
    public interface IMasterAgreement
    {
        #region
        IScheme MasterAgreementType { get;set;}
        bool MasterAgreementDateSpecified { get;set;}
        EFS_Date MasterAgreementDate { get;set;}
        #endregion
        //
        #region  Method
        IScheme CreateMasterAgreementType();
        #endregion  Method
    }
    #endregion
    #region IMath
    public interface IMath
    {
        #region Accessors
        System.Xml.XmlNode[] Any { set;get;}
        #endregion Accessors
    }
    #endregion IMath
    #region IMoney
    public interface IMoney
    {
        #region Accessors
        EFS_Decimal Amount { set; get;}
        string Currency { set; get;}
        IOffset DefaultOffsetPreSettlement { get;}
        IOffset DefaultOffsetUsanceDelaySettlement { get;}
        ICurrency GetCurrency { get; }
        string Id { set;get;}
        #endregion Accessors
        #region Methods
        IOffset CreateOffset(PeriodEnum pPeriod, int pMultiplier, DayTypeEnum pDayType);
        IMoney Clone();
        #endregion Methods
    }
    #endregion IMoney
    #region IMultipleExercise
    public interface IMultipleExercise : IPartialExercise
    {
        #region Accessors
        bool MaximumNumberOfOptionsSpecified { get;}
        EFS_PosInteger MaximumNumberOfOptions { get;}
        bool MaximumNotionalAmountSpecified { get;}
        EFS_Decimal MaximumNotionalAmount { get;}
        #endregion Accessors
    }
    #endregion IMultipleExercise
    #region INbOptionsAndNotionalBase
    // EG 20150422 [20513] BANCAPERTA New 
    public interface INbOptionsAndNotionalBase 
    {
        #region Accessors
        bool NumberOfOptionsSpecified { get; }
        EFS_Decimal NumberOfOptions { get; }
        EFS_Decimal OptionEntitlement { get; }
        bool NotionalSpecified { get; }
        IMoney Notional { get; }
        #endregion Accessors
    }
    #endregion INbOptionsAndNotionalBase

    #region INettingDesignation
    public interface INettingDesignation
    {
        #region Accessors
        string Value { set; get;}
        int OTCmlId { set;get;}
        #endregion Accessors
    }
    #endregion INettingDesignation
    #region INettingInformationInput
    public interface INettingInformationInput
    {
        #region Accessors
        NettingMethodEnum NettingMethod { set; get;}
        bool NettingDesignationSpecified { set; get;}
        INettingDesignation NettingDesignation { set;get;}
        #endregion Accessors
    }
    #endregion INettingInformationInput
    #region INotifyingParty
    public interface INotifyingParty
    {
        #region Accessors
        IReference BuyerPartyReference { set;get;}
        bool SellerPartyReferenceSpecified { set;get;}
        IReference SellerPartyReference { set;get;}
        #endregion Accessors
    }
    #endregion INotifyingParty
    #region INotional
    public interface INotional
    {
        #region Accessors
        IAmountSchedule StepSchedule { get;}
        bool StepParametersSpecified { get;}
        INotionalStepRule StepParameters { get;}
        string Id { set;get;}
        #endregion Accessors
    }
    #endregion INotional
    #region INotionalStepRule
    public interface INotionalStepRule
    {
        #region Accessors
        bool IsStepDateCalculated { get;}
        EFS_Step Efs_FirstStepDate { get;}
        EFS_Step Efs_LastStepDate { get;}
        EFS_Step Efs_VirtualLastStepDate { get;}
        IInterval StepFrequency { get;}
        bool NotionalStepAmountSpecified { get;}
        decimal NotionalStepAmount { get;}
        bool NotionalStepRateSpecified { get;}
        bool StepRelativeToSpecified { get;}
        StepRelativeToEnum StepRelativeTo { get;}
        decimal NotionalStepRate { get;}
        #endregion Accessors
        #region Methods
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        Cst.ErrLevel CalcAdjustableStepDate(string pCs, DateTime pTerminationDate, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument);
        #endregion Methods
    }
    #endregion INotionalStepRule

    #region IObservedRates
    public interface IObservedRates
    {
        #region Accessors
        DateTime ObservationDate { get;}
        decimal ObservedRate { get;}
        #endregion Accessors
    }
    #endregion IObservedRates
    #region IOffset
    public interface IOffset : IInterval
    {
        #region Accessors
        bool DayTypeSpecified { set; get;}
        DayTypeEnum DayType { set; get;}
        #endregion Accessors
        #region Methods
        /// <summary>
        /// Retourne les business Centers associées au devises {pCurrencies}
        /// <para>Retourne null, s'il n'existe aucun business center actif</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCurrencies">Devise au format ISO4217_ALPHA3</param>
        /// <returns></returns>
        /// FI 20131118 [19118] Add Commentaire 
        // EG 20180307 [23769] Gestion dbTransaction
        IBusinessCenters GetBusinessCentersCurrency(string pCS, IDbTransaction pDbTransaction, params string[] pCurrencies);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pBusinessDayConvention"></param>
        /// <param name="pIdBC"></param>
        /// <returns></returns>
        IBusinessDayAdjustments CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, params string[] pIdBC);
        #endregion Methods
    }
    #endregion IOffset
    #region IOpenUnits
    public interface IOpenUnits
    {
        #region Accessors
        bool OpenUnitsSpecified { set;get;}
        EFS_Decimal OpenUnits { set;get;}
        #endregion Accessors
    }
    #endregion IOpenUnits

    #region IOptionBase
    // EG 20150410 [20513] BANCAPERTA New
    public interface IOptionBase
    {
        #region Accessors
        IReference BuyerPartyReference { set; get; }
        IReference SellerPartyReference { set; get; }
        OptionTypeEnum OptionType { set; get; }
        #endregion Accessors


    }
    #endregion IOptionBase
    #region IOptionBaseExtended
    // EG 20150410 [20513] BANCAPERTA New
    public interface IOptionBaseExtended : IOptionBase
    {
        #region Accessors
        bool PremiumSpecified { set; get; }
        IPremium Premium { set; get; }
        object EFS_Exercise { set; get; }
        ExerciseStyleEnum GetStyle { get; }
        bool AmericanExerciseSpecified { set; get; }
        IAmericanExercise AmericanExercise { set; get; }
        bool BermudaExerciseSpecified { set; get; }
        IBermudaExercise BermudaExercise { set; get; }
        bool EuropeanExerciseSpecified { set; get; }
        IEuropeanExercise EuropeanExercise { set; get; }
        IExerciseProcedure ExerciseProcedure { set; get; }
        bool FeatureSpecified { set; get; }
        IOptionFeature Feature { set; get; }
        bool NotionalAmountSpecified { set; get; }
        IMoney NotionalAmount { set; get; }
        bool NotionalAmountReferenceSpecified { set; get; }
        IReference NotionalAmountReference { set; get; }
        EFS_Decimal OptionEntitlement { set; get; }
        bool EntitlementCurrencySpecified { set; get; }
        ICurrency EntitlementCurrency { set; get; }
        bool NumberOfOptionsSpecified { set; get; }
        EFS_Decimal NumberOfOptions { set; get; }
        bool SettlementTypeSpecified { set; get; }
        SettlementTypeEnum SettlementType { set; get; }
        bool SettlementDateSpecified { set; get; }
        IAdjustableOrRelativeDate SettlementDate { set; get; }
        bool SettlementAmountSpecified { set; get; }
        IMoney SettlementAmount { set; get; }
        bool SettlementCurrencySpecified { set; get; }
        ICurrency SettlementCurrency { set; get; }
        IOptionFeature CreateOptionFeature { get; }
        #endregion Accessors
    }
    #endregion IOptionBaseExtended
    #region IOptionalEarlyTermination
    public interface IOptionalEarlyTermination : IExerciseProvision, IProvision
    {
        #region Accessors
        EFS_ExerciseDates Efs_ExerciseDates { set; get;}
        ICalculationAgent CalculationAgent { set;get;}
        #endregion Accessors
    }
    #endregion IOptionalEarlyTermination
    #region IOptionFeature
    public interface IOptionFeature
    {
        #region Accessors
        bool FxFeatureSpecified { set; get; }
        IFxFeature FxFeature { set; get; }
        bool StrategyFeatureSpecified { set; get; }
        IStrategyFeature StrategyFeature { set; get; }
        bool AsianSpecified { set; get; }
        IAsian Asian { set; get; }
        bool BarrierSpecified { set; get; }
        IBarrier Barrier { set; get; }
        bool KnockSpecified { set; get; }
        IKnock Knock { set; get; }
        bool PassThroughSpecified { set; get; }
        IPassThrough PassThrough { set; get; }
        #endregion Accessors
    }
    #endregion IOptionFeatures
    #region IOptionFeatures
    public interface IOptionFeatures
    {
        #region Accessors
        bool AsianSpecified { set;get;}
        IAsian Asian { set;get;}
        bool BarrierSpecified { set;get;}
        IBarrier Barrier { set;get;}
        bool KnockSpecified { set;get;}
        IKnock Knock { set;get;}
        bool PassThroughSpecified { set;get;}
        IPassThrough PassThrough { set;get;}
        bool DividendAdjustmentSpecified { set;get;}
        IDividendAdjustment DividendAdjustment { set;get;}

        bool MultipleBarrierSpecified { set; get; }
        IExtendedBarrier[] MultipleBarrier { set; get; }

        #endregion Accessors
    }
    #endregion IOptionFeatures
    #region IOptionStrike
    public interface IOptionStrike
    {
        #region Accessors
        bool PriceSpecified { set;get;}
        EFS_Decimal Price { set;get;}
        bool PercentageSpecified { set;get;}
        EFS_Decimal Percentage { set;get;}
        bool CurrencySpecified { set;get;}
        ICurrency Currency { set;get;}
        #endregion Accessors
    }
    #endregion IOptionStrike


    #region IPartialExercise
    public interface IPartialExercise
    {
        #region Accessors
        bool NotionalReferenceSpecified { set; get; }
        IReference[] NotionalReference { set; get;}
        bool IntegralMultipleAmountSpecified { set; get;}
        EFS_Decimal IntegralMultipleAmount { set; get;}
        bool MinimumNumberOfOptionsSpecified { set; get;}
        EFS_PosInteger MinimumNumberOfOptions { set; get;}
        bool MinimumNotionalAmountSpecified { set; get;}
        EFS_Decimal MinimumNotionalAmount { set; get;}
        #endregion Accessors
    }
    #endregion IPartialExercise
    #region IParty
    /// <summary>
    /// 
    /// </summary>
    /// FI 20170928 [23452] Modify
    /// EG 20170926 [22374] Add tzdbid
    public interface IParty
    {
        #region Accessors
        /// <summary>
        /// Obtient ou définit le 1er item de partyId
        /// </summary>
        string PartyId { set; get; }
        /// <summary>
        /// Obtient ou définit partyId
        /// </summary>
        IScheme[] PartyIds { set; get; }
        string PartyName { set; get; }
        string Id { set; get; }
        string OtcmlId { get; }
        int OTCmlId { set; get; }

        string Tzdbid { set; get; }

        // FI 20170928 [23452] add Person
        bool PersonSpecified { set; get; }
        IPerson[] Person { set; get; }
        #endregion Accessors
    }
    #endregion IParty
    #region IPartyTradeIdentifier
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Remplace coquille sur IVersionSchemeId)
    /// EG 20240227 [WI855][WI858] Trade input : Upd by ITradeId interface (TradeId)
    public interface IPartyTradeIdentifier : ITradeIdentifier
    {
        #region Accessors
        IReference PartyReference { set; get;}
        bool BookIdSpecified { set; get;}
        IBookId BookId { get;}
        bool LinkIdSpecified { set; get;}
        ILinkId[] LinkId { set; get; }
        bool LocalClassDervSpecified { set; get;}
        IScheme LocalClassDerv { set; get;}
        bool LocalClassNDrvSpecified { set; get;}
        IScheme LocalClassNDrv { set; get;}
        bool IasClassDervSpecified { set; get;}
        IScheme IasClassDerv { set; get;}
        bool IasClassNDrvSpecified { set; get;}
        IScheme IasClassNDrv { set; get;}
        bool HedgeClassDervSpecified { set; get;}
        IScheme HedgeClassDerv { set; get;}
        bool HedgeClassNDrvSpecified { set; get;}
        IScheme HedgeClassNDrv { set; get;}
        bool FxClassSpecified { set; get;}
        IScheme FxClass { set; get;}
        bool TradeIdSpecified { set; get;}
        ITradeId[] TradeId { get;}
        bool VersionedTradeIdSpecified { set; get;}
        IVersionedSchemeId[] VersionedTradeId { get;}
        #endregion Accessors
        #region Methods
        ILinkId GetLinkIdFromScheme(string pScheme);
        ILinkId GetLinkIdWithNoScheme();
        #endregion Methods
    }
    #endregion IPartyTradeIdentifier
    #region IPartyTradeInformation
    
    /// <summary>
    /// 
    /// </summary>
    // EG 20170918 [23342]
    // FI 20170928 [23452] Modify
    // EG 20171025 [23509] Add executionDateTimeOffset, orderEntered, timestamps
    // EG 20171031 [23509] Upd
    public interface IPartyTradeInformation
    {
        #region Accessors
        string PartyReference { set; get; }

        bool TraderSpecified { get; set; }
        ITrader[] Trader { get; set; }

        bool SalesSpecified { get; set; }
        ITrader[] Sales { get; set; }

        bool ExecutionDateTimeSpecified { get; set; }
        IScheme ExecutionDateTime { get; set; }
        Nullable<DateTimeOffset> ExecutionDateTimeOffset { get; }

        bool OrderEnteredSpecified { get; }
        bool TimestampsSpecified { get; set; }
        ITradeProcessingTimestamps Timestamps { get; set; }

        bool BrokerPartyReferenceSpecified { get; set; }
        IReference[] BrokerPartyReference { get; set; }

        // FI 20170928 [23452] add
        bool RelatedPartySpecified { get; set; }
        IRelatedParty[] RelatedParty { get; set; }

        // FI 20170928 [23452] add
        bool RelatedPersonSpecified { get; set; }
        IRelatedPerson[] RelatedPerson { get; set; }

        // FI 20170928 [23452] add
        bool AlgorithmSpecified { get; set; }
        IAlgorithm[] Algorithm { get; set; }

        // FI 20170928 [23452] add
        bool CategorySpecified { get; set; }
        IScheme[] Category { get; set; }

        // FI 20170928 [23452] add
        bool TradingWaiverSpecified { get; set; }
        IScheme[] TradingWaiver { get; set; }

        // FI 20170928 [23452] add
        bool ShortSaleSpecified { get; set; }
        IScheme ShortSale { get; set; }

        // FI 20170928 [23452] add
        bool OtcClassificationSpecified { get; set; }
        IScheme[] OtcClassification { get; set; }

        // FI 20170928 [23452] add
        bool IsCommodityHedgeSpecified { get; set; }
        bool IsCommodityHedge { get; set; }
        
        // FI 20170928 [23452] add
        bool IsSecuritiesFinancingSpecified { get; set; }
        bool IsSecuritiesFinancing { get; set; }
        #endregion Accessors
    }
    #endregion IPartyTradeInformation

    #region IPartyRole
    public interface IPartyRole
    {
        #region Members
        bool AccountSpecified { set; get;}
        IReference Account { set; get;}
        bool PartySpecified { set; get;}
        IReference Party { set; get;}
        #endregion Members
    }
    #endregion

    #region IPassThrough
    public interface IPassThrough
    {
        IPassThroughItem[] PassThroughItem { set;get;}
    }
    #endregion
    #region IPassThroughItem
    public interface IPassThroughItem
    {
        #region Accessors
        IReference PayerPartyReference { set;get;}
        IReference ReceiverPartyReference { set;get;}
        IReference UnderlyerReference { set;get;}
        EFS_Decimal PassThroughPercentage { set;get;}
        #endregion Accessors
    }
    #endregion IPassThroughItem

    

    #region IPayment
    /// <summary>
    /// 
    /// </summary>
    /// FI 20180328 [23871] Modify
    public interface IPayment
    {
        #region Accessors
        string Id { get; set; } // FI 20180328 [23871] Add
        IReference PayerPartyReference { get; set; }
        IReference ReceiverPartyReference { get; set; }
        IMoney PaymentAmount { get;}
        bool PaymentDateSpecified { set; get;}
        IAdjustableDate PaymentDate { set; get; }
        bool AdjustedPaymentDateSpecified { set; get;}
        DateTime AdjustedPaymentDate { set;  get; }
        bool PaymentQuoteSpecified { set; get;}
        IPaymentQuote PaymentQuote { set;get;}
        bool CustomerSettlementPaymentSpecified { set; get;}
        ICustomerSettlementPayment CustomerSettlementPayment { set; get;}
        bool PaymentSourceSpecified { set; get;}
        ISpheresSource PaymentSource { set; get;}
        bool TaxSpecified { set; get;}
        ITax[] Tax { set; get;}
        ISettlementInformation SettlementInformation { get;}
        string PaymentCurrency { get;}
        string PaymentSettlementCurrency { get;}
        EFS_Payment Efs_Payment { set;get;}
        bool SettlementInformationSpecified { set; get;}
        ICustomerSettlementPayment CreateCustomerSettlementPayment { get;}
        bool PaymentTypeSpecified { set; get;}
        IScheme PaymentType { set; get;}
        IPaymentQuote CreatePaymentQuote { get;}
        IReference CreateReference { get;}
        #endregion Accessors
        #region Methods
        IAdjustedDate CreateAdjustedDate(DateTime pAdjustedDate);
        IAdjustedDate CreateAdjustedDate(string pAdjustedDate);
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        IMoney GetNotionalAmountReference(object pNotionalReference);
        string GetPaymentType(string pEventCode);
        ITax CreateTax { get;}
        ITaxSchedule CreateTaxSchedule { get;}
        ISpheresSource CreateSpheresSource {get;}
        IPayment DeleteMatchPayment(ArrayList pPayment);
        #endregion Methods

    }
    #endregion IPayment
    #region IPaymentCurrency
    // EG 20231127 [WI755] Implementation Return Swap : DEL referenceSpecified
    public interface IPaymentCurrency
    {
        #region Accessors
        string Reference { get;}
        bool CurrencySpecified { get;}
        ICurrency Currency { get;}
        bool CurrencydeterminationMethodSpecified { get;}
        IScheme CurrencyDeterminationMethod { get;}
        string CurrencyDeterminationMethodValue { get;}
        #endregion Accessors
    }
    #endregion IPaymentCurrency
    #region IPaymentDates
    /// EG 20140702 Add valuationDatesReferenceSpecified|valuationDatesReference
    public interface IPaymentDates
    {
        #region Accessors
        IInterval PaymentFrequency { set; get; }
        PayRelativeToEnum PayRelativeTo { set; get; }
        bool FirstPaymentDateSpecified { set; get; }
        EFS_Date FirstPaymentDate { set; get; }
        bool PaymentDaysOffsetSpecified { set; get; }
        IOffset PaymentDaysOffset { set; get; }
        IBusinessDayAdjustments PaymentDatesAdjustments { set; get; }
        bool LastRegularPaymentDateSpecified { set; get; }
        EFS_Date LastRegularPaymentDate { set; get; }
        EFS_PaymentDates Efs_PaymentDates { set; get; }
        bool CalculationPeriodDatesReferenceSpecified { set; get; }
        IReference CalculationPeriodDatesReference { set; get; }
        bool ResetDatesReferenceSpecified { set; get; }
        IReference ResetDatesReference { set; get; }
        bool ValuationDatesReferenceSpecified { set; get; }
        IReference ValuationDatesReference { set; get; }
        string Id { set; get; }
        #endregion Accessors
        #region Methods
        #endregion Methods
    }
    #endregion IPaymentDates
    #region IPaymentQuote
    public interface IPaymentQuote
    {
        #region Accessors
        bool PercentageRateFractionSpecified { set; get;}
        string PercentageRateFraction { set; get;}
        EFS_Decimal PercentageRate { set;get;}
        IReference PaymentRelativeTo { set;get;}
        void InitializePercentageRateFromPercentageRateFraction();
        #endregion Accessors
    }
    #endregion IPaymentQuote

    #region IPerson
    /// <summary>
    /// 
    /// </summary>
    // FI 20170928 [23452] add
    public interface IPerson : ISpheresId
    {
        #region Accessors
        Boolean FirstNameSpecified { set; get; }
        string FirstName { set; get; }

        Boolean SurnameSpecified { set; get; }
        string Surname { set; get; }

        Boolean PersonIdSpecified { set; get; }
        IScheme[] PersonId { set; get; }

        string Id { set; get; }
        #endregion Accessors
    }
    #endregion IPerson



    #region IPeriodicDates
    public interface IPeriodicDates
    {
        #region Accessors
        IAdjustableOrRelativeDate CalculationStartDate { get;}
        bool CalculationEndDateSpecified { get;}
        IAdjustableOrRelativeDate CalculationEndDate { get;}
        ICalculationPeriodFrequency CalculationPeriodFrequency { get;}
        IBusinessDayAdjustments CalculationPeriodDatesAdjustments { get;}
        #endregion Accessors
    }
    #endregion IPeriodicDates
    #region IPrePayment
    public interface IPrePayment
    {
        #region Accessors
        IReference PayerPartyReference { set;get;}
        IReference ReceiverPartyReference { set;get;}
        EFS_Boolean PrePayment { set;get;}
        IMoney PrePaymentAmount { set;get;}
        IAdjustableDate PrePaymentDate { set;get;}
        #endregion Accessors
    }
    #endregion IPrePayment

    #region IPremium
    // EG 20150410 [20513] BANCAPERTA New valuationType
    public interface IPremium : ISimplePayment
    {
        #region Accessors
        PremiumAmountValuationTypeEnum ValuationType { set; get; }
        bool PremiumTypeSpecified { set; get; }
        PremiumTypeEnum PremiumType { set; get; }
        bool PricePerOptionSpecified { set; get; }
        IMoney PricePerOption { set; get; }
        bool PercentageOfNotionalSpecified { set; get; }
        EFS_Decimal PercentageOfNotional { set; get; }
        bool DiscountFactorSpecified { set; get; }
        EFS_Decimal DiscountFactor { set; get; }
        bool PresentValueAmountSpecified { set; get; }
        IMoney PresentValueAmount { set; get; }
        #endregion Accessors
    }
    #endregion IPremium
    #region IPremiumBase
    // EG 20150410 [20513] BANCAPERTA New 
    public interface IPremiumBase
    {
        #region Accessors
        IReference PayerPartyReference { get; }
        IReference ReceiverPartyReference { get;  }
        bool PremiumTypeSpecified {  get; }
        PremiumTypeEnum PremiumType {  get; }
        bool PricePerOptionSpecified {  get; }
        IMoney PricePerOption {  get; }
        bool PercentageOfNotionalSpecified {  get; }
        EFS_Decimal PercentageOfNotional {  get; }
        bool PaymentAmountSpecified {  get; }
        IMoney PaymentAmount {  get; }
        #endregion Accessors
    }
    #endregion IPremiumBase

    #region IPremiumQuote
    public interface IPremiumQuote
    {
        #region Accessors
        EFS_Decimal PremiumValue { set; get;}
        PremiumQuoteBasisEnum PremiumQuoteBasis { set;get;}
        #endregion Accessors
    }
    #endregion IPremiumQuote
    #region IPrice
    public interface IPrice
    {
        #region Accessors
        bool CommissionSpecified { set; get;}
        ICommission Commission { get;}
        bool GrossPriceSpecified { get;}
        IActualPrice GrossPrice { get;}
        bool DeterminationMethodSpecified { set; get;}
        string DeterminationMethod { set; get;}
        bool AmountRelativeToSpecified { set; get;}
        IReference AmountRelativeTo { get;}
        bool NetPriceSpecified { set;get;}
        IActualPrice NetPrice { set; get;}
        bool AccruedInterestPriceSpecified { get;}
        EFS_Decimal AccruedInterestPrice { get;}
        #endregion Accessors
    }
    #endregion IPrice
    #region IPrincipalExchanges
    public interface IPrincipalExchanges
    {
        #region Accessors
        EFS_Boolean InitialExchange { set;get;}
        EFS_Boolean FinalExchange { set;get;}
        EFS_Boolean IntermediateExchange { set;get;}
        #endregion Accessors
    }
    #endregion IPrincipalExchanges
    #region IPrincipalExchangeAmount
    public interface IPrincipalExchangeAmount
    {
        #region Accessors
        bool RelativeToSpecified { get;}
        IReference RelativeTo { get;}
        bool DeterminationMethodSpecified { get;}
        EFS_MultiLineString DeterminationMethod { get;}
        bool AmountSpecified { get;}
        IMoney Amount { get;}
        #endregion Accessors
    }
    #endregion IPrincipalExchangeAmount
    #region IPrincipalExchangeDescriptions
    public interface IPrincipalExchangeDescriptions
    {
        #region Accessors
        IReference PayerPartyReference { get;}
        IReference ReceiverPartyReference { get;}
        IPrincipalExchangeAmount PrincipalExchangeAmount { get;}
        IAdjustableOrRelativeDate PrincipalExchangeDate { get;}
        #endregion Accessors
    }
    #endregion IPrincipalExchangeDescriptions
    #region IPrincipalExchangeFeatures
    public interface IPrincipalExchangeFeatures
    {
        #region Accessors
        bool DescriptionsSpecified { get;}
        IPrincipalExchanges PrincipalExchanges { get;}
        IPrincipalExchangeDescriptions[] Descriptions { get;}
        #endregion Accessors
    }
    #endregion IPrincipalExchangeFeatures
    #region IProduct
    public interface IProduct
    {
        #region Accessors
        object Product { get;}
        IProductBase ProductBase { get;}
        #endregion Accessors
    }
    #endregion IProduct
    #region IProductBase
    /// EG 20130607 [18740] Add RemoveCAExecuted
    /// EG 20140702 Add IsEquitySwapTransactionSupplement|IsVarianceSwapTransactionSupplement
    /// EG 20150317 [POC] Add IsMarginingAndNotFungible|IsMargining|IsFunding
    /// EG 20150317 [POC] Add Nullable pIdEM parameter for CreatePosRequestEOD|CreatePosRequestClosingDAY
    /// EG 20150422 [20513] BANCAPERTA New CreateBusinessCentersReference
    /// EG 20171016 [23509] CreateZonedDateTime
    /// EG 20171025 [23509] CreateTradeProcessingTimestamps
    public interface IProductBase
    {
        #region Accessors
        /// <summary>
        /// Obtient le nom du System.Type de l'instance actuelle
        /// </summary>
        string ProductName { get;}
        bool IsStrategy { get;}
        bool IsBondTransaction { get;}
        bool IsBulletPayment { get;}
        bool IsBrokerEquityOption { get;}
        bool IsCapFloor { get;}
        bool IsDebtSecurity { get;}
        bool IsDebtSecurityTransaction { get;}
        bool IsFra { get;}
        bool IsEquityForward { get;}
        bool IsEquityOption { get;}
        bool IsEquityOptionTransactionSupplement { get;}
        /// <summary>
        /// Retourne true si le produit est une transaction sur Equity (produit EquitySecurityTransaction) 
        /// </summary>
        bool IsEquitySecurityTransaction { get; }
        bool IsExchangeTradedDerivative { get;}
        bool IsFutureTransaction { get;}
        bool IsFxDigitalOption { get;}
        bool IsFxAverageRateOption { get;}
        bool IsFxBarrierOption { get;}
        bool IsFxLeg { get;}
        bool IsFxSwap { get;}
        bool IsFxTermDeposit { get;}
        bool IsInvoice { get;}
        bool IsAdditionalInvoice { get;}
        bool IsCreditNote { get;}
        bool IsInvoiceSettlement { get;}
        bool IsLoanDeposit { get;}
        bool IsReturnSwap { get;}
        bool IsEquitySwapTransactionSupplement { get; }
        bool IsSwap { get;}
        bool IsSwaption { get;}
        bool IsFxOptionLeg { get;}
        bool IsSaleAndRepurchaseAgreement { get;}
        bool IsVarianceSwapTransactionSupplement { get; }

        
        /// <summary>
        /// Retourne true si le produit est une transaction sur titre de créance (Repo,BuyAndSellBack,SecurityLending,DebtSecurityTransaction) 
        /// </summary>
        bool IsSecurityTransaction { get;}
        bool IsRepo { get;}
        bool IsBuyAndSellBack { get;}
        bool IsSecurityLending { get;}
        /// <summary>
        /// 
        /// </summary>
        bool IsMarginRequirement { get; }
        /// <summary>
        /// 
        /// </summary>
        bool IsCashBalance { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsCashBalanceInterest { get; }
        // EG 20131118 New
        bool IsCashPayment { get; }
        //
        bool IsASSET { get;}
        bool IsADM { get;}
        bool IsEQD { get;}
        bool IsLSD { get;}
        bool IsRISK { get; }
        /// <summary>
        /// Obtient true si IsFxOption ou IsFxSwap ou IsFxLeg
        /// </summary>
        bool IsFx { get;}
        /// <summary>
        /// Obtient true si IsFxAverageRateOption ou IsFxBarrierOption ou IsFxDigitalOption ou IsFxOptionLeg
        /// </summary>
        bool IsFxOption { get;}
        bool IsIRD { get;}
        bool IsESE { get; }
        bool IsDSE { get; }
        
        /// <summary>
        /// Retourne true si le produit est un titre ou une transaction sur titre
        /// </summary>
        bool IsSEC { get;}

        /// <summary>
        /// Retourne true si le produit est une option sur titre 
        /// </summary>
        // EG 20150410 [20513] BANCAPERTA
        bool IsBondOption { get; }

        /// EG 20150317 [POC] Add IsMarginingAndNotFungible|IsMargining
        bool IsFungible(string pCS);
        bool IsMarginingAndNotFungible(string pCS);
        bool IsMargining(string pCS);
      
        //
        IProductType ProductType { set; get;}
        string Id { set; get;}
        Type TypeofStream { get;}
        Type TypeofLoadDepositStream { get;}
        Type TypeofFxOptionPremium { get;}
        Type TypeofPayment { get;}
        Type TypeofPaymentDates { get;}
        Type TypeofSchedule { get;}
        Type TypeofEquityPremium { get;}
        Type TypeofCurrency { get; }
        Type TypeofBusinessCenter { get; }
        Type TypeofReturnLeg { get; }
        Type TypeofInterestLeg { get; }
        Type TypeofPhysicalLeg { get; }
        Type TypeofGasPhysicalLeg { get;  }
        Type TypeofElectricityPhysicalLeg { get; }
        Type TypeofFinancialLeg { get; }
        Type TypeofFixedPriceSpotLeg { get  ; }
        Type TypeofFixedPriceLeg { get; }
        Type TypeofEnvironmentalPhysicalLeg { get ; }

        IImplicitProvision ImplicitProvision { set; get;}
        bool ImplicitProvisionSpecified { set; get;}
        bool ImplicitEarlyTerminationProvisionSpecified { get;}
        bool ImplicitCancelableProvisionSpecified { set; get;}
        bool ImplicitExtendibleProvisionSpecified { set; get;}
        bool ImplicitOptionalEarlyTerminationProvisionSpecified { set; get;}
        bool ImplicitMandatoryEarlyTerminationProvisionSpecified { set; get;}

        bool EarlyTerminationProvisionSpecified { set; get;}
        IEarlyTerminationProvision EarlyTerminationProvision { set; get;}
        bool CancelableProvisionSpecified { set; get;}
        ICancelableProvision CancelableProvision { set;get;}
        bool ExtendibleProvisionSpecified { set; get;}
        IExtendibleProvision ExtendibleProvision { set;get;}
        bool MandatoryEarlyTerminationProvisionSpecified { set; get;}
        IMandatoryEarlyTermination MandatoryEarlyTerminationProvision { set;get;}
        bool OptionalEarlyTerminationProvisionSpecified { set; get;}
        IOptionalEarlyTermination OptionalEarlyTerminationProvision { set;get;}
        //bool mainProductReferenceSpecified { set; get;}
        //IReference mainProductReference { set; get;}
        #endregion Accessors

        #region Accessors
        /// EG 20161122 New Commodity Derivative
        bool IsCommoditySwap { get;}
        bool IsCommoditySpot { get; }
        bool IsCommodityDerivative { get; }
        bool PrimaryAssetClassSpecified { set; get; }
        IScheme PrimaryAssetClass { set; get; }
        bool SecondaryAssetClassSpecified { set; get; }
        IScheme[] SecondaryAssetClass { set; get; }
        #endregion Accessors


        #region Methods
        
        IFxLeg[] CreateFxLegs(int pDim);
        IPayment CreatePayment();
        IPriceUnits CreatePriceUnits();
        IMoney CreateMoney();
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        IPartyRole CreatePartyRole();
        ICurrency CreateCurrency(string pCurrency);
        IDebtSecurity  CreateDebtSecurity();
        IDebtSecurityStream[] CreateDebtSecurityStreams(int pDim);
        IDebtSecurityTransaction CreateDebtSecurityTransaction();
        IFxOptionLeg CreateFxOptionLeg();
        IExchangeTradedDerivative CreateExchangeTradedDerivative();
        IReturnSwap CreateReturnSwap();
        IEquitySecurityTransaction CreateEquitySecurityTransaction();
        IBusinessDayAdjustments CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, params string[] pIdBC);
        IAdjustableDate CreateAdjustableDate(DateTime pDate, BusinessDayConventionEnum pBusinessDayConvention, IBusinessCenters pBusinessCenters);
        IRelativeDateOffset CreateRelativeDateOffset();
        IRelativeDates CreateRelativeDates();
        IBusinessDateRange CreateBusinessDateRange();
        IOffset CreateOffset(PeriodEnum pPeriod, int pMultiplier, DayTypeEnum pDayType);
        IAdjustableOffset CreateAdjustableOffset();
        IInterval CreateInterval(PeriodEnum pPeriod, int pMultiplier);
        ICalculationPeriodFrequency CreateFrequency(PeriodEnum pPeriod, int pPeriodMultiplier, RollConventionEnum pRollConvention);
        IInterval[] CreateIntervals();
        IFxFixing CreateFxFixing();
        IExpiryDateTime CreateExpiryDateTime();
        IFxRate CreateFxRate();
        IFxStrikePrice CreateStrikePrice();
        IFxAverageRateOption CreateFxAverageRateOption();
        IInformationSource[] CreateInformationSources();
        IExerciseFee CreateExerciseFee();
        IExerciseProcedure CreateExerciseProcedure();
        ICashSettlement CreateCashSettlement();
        ICalculationAgent CreateCalculationAgent();
        IReference CreatePartyReference(string pReference);
        IReference CreatePartyOrAccountReference(string pReference);
        IReference CreatePartyOrTradeSideReference(string pReference);
        IReference[] CreateArrayPartyReference(string pReference);
        IReference CreateBusinessCentersReference(string pReference);
        ISwap CreateSwap();
        ISecurity CreateSecurity();
        ISecurityAsset CreateSecurityAsset();
        ISecurityLeg CreateSecurityLeg();
        ISecurityLeg[] CreateSecurityLegs(int pDim);
        IInterestRateStream[] CreateInterestRateStream(int pDim);
        ISettlementMessagePartyPayment CreateSettlementMessagePartyPayment();
        // EG 20180205 [23769] Add dbTransaction  
        IBusinessCenters LoadBusinessCenters(string pConnectionString, IDbTransaction pDbTransaction, string[] pIdA, string[] pIdC, string[] pIdM);
        void SetProductType(string pId, string pIdentifier);
        void SetId(int pInstrumentNo);
        IBusinessCenters CreateBusinessCenters(params string[] pIdBCs);
        IAdjustedDate CreateAdjustedDate(DateTime pAdjustedDate);
        IAdjustedDate CreateAdjustedDate(string pAdjustedDate);
        IRoutingCreateElement CreateRoutingCreateElement();
        ISettlementMessageDocument CreateSettlementMessageDocument();
        INotificationDocument CreateConfirmationMessageDocument();
        ISpheresIdSchemeId[] CreateSpheresId(int pDim);
        ITrader CreateTrader();
        IImplicitProvision CreateImplicitProvision();
        IEmpty CreateImplicitProvisionItem();
        ICancelableProvision CreateCancelableProvision();
        IExtendibleProvision CreateExtendibleProvision();
        IEarlyTerminationProvision CreateOptionalEarlyTermination();
        IEarlyTerminationProvision CreateMandatoryEarlyTermination();
        IBusinessCenterTime CreateBusinessCenterTime(DateTime pTime, string pBusinessCenter);
        IQuotedCurrencyPair CreateQuotedCurrencyPair(string pIdC1, string pIdC2, QuoteBasisEnum pQuoteBasis);
        INetInvoiceAmounts CreateNetInvoiceAmounts();
        INetInvoiceAmounts CreateNetInvoiceAmounts(decimal pAmount, string pAmountCurrency,
            decimal pIssueAmount, string pIssueAmountCurrency, decimal pAccountingAmount, string pAccountingAmountCurrency);
        ITradeIntention CreateTradeIntention();
        IPosRequest CreatePosRequestGroupLevel(Cst.PosRequestTypeEnum pRequestType, int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness);
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestOption CreatePosRequestOption(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty);
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestClearingEOD CreatePosRequestClearingEOD(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty);
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestClearingBLK CreatePosRequestClearingBLK(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty);
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestEntry CreatePosRequestEntry(DateTime pDtBusiness, decimal pQty);
        IPosRequestUpdateEntry CreatePosRequestUpdateEntry(SettlSessIDEnum pRequestMode, DateTime pDtBusiness);
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestUnclearing CreatePosRequestUnclearing(DateTime pDtMarket, int pIdPR, int pIdPADET, Cst.PosRequestTypeEnum pRequestType, 
            decimal pQty, DateTime pDtBusiness, int pIdT_Closing, string pClosing_Identifier, decimal pClosingQty);
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestClearingSPEC CreatePosRequestClearingSPEC(SettlSessIDEnum pRequestMode, DateTime pDtBusiness,
            int pIdT, decimal pQty, IPosKeepingClearingTrade[] pTradesTarget);

        IPosRequest CreatePosRequest();
        IPosRequestOption CreatePosRequestOption();
        IPosRequestPositionOption CreatePosRequestPositionOption();
        
        IPosRequestClearingEOD CreatePosRequestClearingEOD();
        IPosRequestClearingBLK CreatePosRequestClearingBLK();
        IPosRequestCorporateAction CreatePosRequestCorporateAction(Cst.PosRequestTypeEnum pRequestType, int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness);
        IPosRequestCorporateAction CreatePosRequestCorporateAction();
        IPosRequestClearingSPEC CreatePosRequestClearingSPEC();
        IPosRequestUpdateEntry CreatePosRequestUpdateEntry();
        IPosRequestUnclearing CreatePosRequestUnclearing();

        IPosRequestCascadingShifting CreatePosRequestCascadingShifting(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness);
        IPosRequestCascadingShifting CreatePosRequestCascadingShifting();
        IPosRequestMaturityOffsetting CreatePosRequestMaturityOffsetting(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness);
        IPosRequestMaturityOffsetting CreatePosRequestMaturityOffsetting();
        // EG 20170206 [22787] New
        IPosRequestPhysicalPeriodicDelivery CreatePosRequestPhysicalPeriodicDelivery(DateTime pDtBusiness);
        // EG 20170206 [22787] New
        IPosRequestPhysicalPeriodicDelivery CreatePosRequestPhysicalPeriodicDelivery();
        IPosRequestEOD CreatePosRequestEOD();
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Add Cst.PosRequestTypeEnum paramter)
        IPosRequestEOD CreatePosRequestEOD(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int>pIdEM, bool pIsCustodian, Cst.PosRequestTypeEnum pPosRequestType);
        IPosRequestREMOVEEOD CreatePosRequestREMOVEEOD();
        IPosRequestREMOVEEOD CreatePosRequestREMOVEEOD(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, bool pIsCustodian);
        IPosRequestREMOVEEOD CreatePosRequestREMOVEEOD(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int> pIdEM, bool pIsCustodian);
        IPosRequestClosingDAY CreatePosRequestClosingDAY();
        /// EG 20150317 [POC] Add Nullable pIdEM parameter
        IPosRequestClosingDAY CreatePosRequestClosingDAY(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int> pIdEM, bool pIsCustodian);
        IPosRequestClosingDayControl CreatePosRequestClosingDayControl(Cst.PosRequestTypeEnum pRequestType,
            int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, DateTime pDtBusiness);
        IPosRequestClosingDayControl CreatePosRequestClosingDayControl(Cst.PosRequestTypeEnum pRequestType,
            int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness);
        IPosKeepingData CreatePosKeepingData();
        IPosKeepingKey CreatePosKeepingKey();
        IPosKeepingClearingTrade CreatePosKeepingClearingTrade();
        IPosKeepingMarket CreatePosKeepingMarket();
        PosKeepingAsset CreatePosKeepingAsset(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset);
        IPosRequestKeyIdentifier CreatePosRequestKeyIdentifier();
        IPosRequestCorrection CreatePosRequestCorrection();
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestCorrection CreatePosRequestCorrection(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty);
        IPosRequestTransfer CreatePosRequestTransfer();
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestTransfer CreatePosRequestTransfer(DateTime pDtBusiness, decimal pQty);
        IPosRequestRemoveAlloc CreatePosRequestRemoveAlloc();
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestRemoveAlloc CreatePosRequestRemoveAlloc(DateTime pDtBusiness, decimal pQty);
        IPosRequestSplit CreatePosRequestSplit();
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        IPosRequestSplit CreatePosRequestSplit(DateTime pDtBusiness, decimal pQty);
        /// EG 20130607 [18740] Add RemoveCAExecuted
        IPosRequestRemoveCAExecuted CreatePosRequestRemoveCAExecuted();
        IPosRequestRemoveCAExecuted CreatePosRequestRemoveCAExecuted(int pIdA_CssCustodian, int pIdM, int pIdCE, DateTime pDtBusiness, bool pIsCustodian);
        IPosRequestDetCorrection CreatePosRequestDetCorrection();
        IPosRequestDetUnderlyer CreatePosRequestDetUnderlyer();
        ISimplePayment CreateSimplePayment();
        IFixInstrument CreateFixInstrument();
        IPosRequestPositionDocument CreatePosRequestPositionDocument();
        IPosRequestDetPositionOption CreatePosRequestDetPositionOption();
        INettingInformationInput CreateNettingInformationInput();
        IParty CreateParty();
        IBookId CreateBookId();
        IUnitQuantity CreateUnitQuantity();
        // EG 20170206 [22787] New
        IPrevailingTime CreatePrevailingTime();
        ICalculationPeriodFrequency CreateCalculationPeriodFrequency();
        // FI 20170928 [23452] add
        IPerson CreatePerson();
        // FI 20170928 [23452] add
        IRelatedPerson CreateRelatedPerson();
        // FI 20170928 [23452] add
        IAlgorithm CreateAlgorithm();
        // FI 20170928 [23452] add
        IRelatedParty CreateRelatedParty();
        // FI 20170928 [23452] add
        IScheme CreateTradeCategory();
        IScheme CreateTradingWaiver();
        IScheme CreateOtcClassification();
        IZonedDateTime CreateZonedDateTime();
        ITradeProcessingTimestamps CreateTradeProcessingTimestamps();
        #endregion Methods
    }
    #endregion IProductBase
    #region IProductType
    /// <summary>
    /// 
    /// </summary>
    /// FI 20150218 [20275] Cette interface hérite de ISpheresIdScheme
    public interface IProductType : ISpheresIdScheme
    {
    }
    #endregion IProductType
    #region IProvision
    public interface IProvision
    {
        #region Methods
        ExerciseStyleEnum GetStyle { get;}
        ICashSettlement CashSettlement { set; get;}
        #endregion Methods
    }
    #endregion IProvision
    #region IPubliclyAvailableInformation
    public interface IPubliclyAvailableInformation
    {
        #region Methods
        bool StandardPublicSourcesSpecified { set;get;}
        bool PublicSourceSpecified { set;get;}
        EFS_StringArray[] PublicSource { set;get;}
        bool SpecifiedNumberSpecified { set;get;}
        EFS_PosInteger SpecifiedNumber { set;get;}
        #endregion Methods
    }
    #endregion IPubliclyAvailableInformation

    #region IRelatedParty
    /// <summary>
    /// 
    /// </summary>
    /// FI 20170928 [23452] add
    public interface IRelatedParty
    {
        IReference PartyReference { set; get; }
        
        bool AccountReferenceSpecified { set; get; }
        IReference AccountReference { set; get; }

        IScheme Role { set; get; }
        IScheme Type { set; get; }
    }
    #endregion IRelatedParty

    #region IRelatedPerson
    /// <summary>
    /// 
    /// </summary>
    /// FI 20170928 [23452] add
    public interface IRelatedPerson
    {
        IReference PersonReference { set; get; }
        IScheme Role { set; get; }
    }
    #endregion IRelatedPerson


    #region IQuanto
    public interface IQuanto
    {
        #region Accessors
        IFxRate[] FxRate { set;get;}
        IFxRate[] CreateFxRate { get;}
        #endregion Accessors
    }
    #endregion IQuanto
    #region IQuotedCurrencyPair
    /// <summary>
    /// Describes the composition of a rate that has been quoted or is to be quoted. 
    /// <para>This includes the two currencies and the quotation relationship between the two currencies and is used as a building block throughout the FX specification.</para>
    /// </summary>
    public interface IQuotedCurrencyPair
    {
        #region Accessors
        string Currency1Scheme { set; get; }
        string Currency1 { set; get;}
        
        string Currency2 { set; get;}
        string Currency2Scheme { set; get; }
        QuoteBasisEnum QuoteBasis { set; get;}
        #endregion Accessors
    }
    #endregion IRouting

    #region IReference
    public interface IReference
    {
        #region Accessors
        string HRef { set; get;}
        #endregion Accessors
    }
    #endregion IReference
    #region IReferenceSwapCurve
    // EG 20150410 [20513] BANCAPERTA
    public interface IReferenceSwapCurve
    {
        #region Accessors
        ISwapCurveValuation SwapUnwindValue { set; get; }
        bool MakeWholeAmountSpecified { set; get; }
        IMakeWholeAmount MakeWholeAmount { set; get; }
        #endregion Accessors
    }
    #endregion IReferenceSwapCurve

    #region IRelativeDateOffset
    public interface IRelativeDateOffset : IOffset
    {
        #region Accessors
        string DateRelativeToValue { set; get;}
        IOffset GetOffset { get;}
        IBusinessDayAdjustments GetAdjustments { get;}
        bool BusinessCentersReferenceSpecified { set;get;}
        bool BusinessCentersNoneSpecified { set;get;}
        bool BusinessCentersDefineSpecified { set;get;}
        IBusinessCenters BusinessCentersDefine { set;get;}
        IReference BusinessCentersReference { set;get;}
        BusinessDayConventionEnum BusinessDayConvention { set;get;}
        #endregion Accessors
    }
    #endregion IRelativeDateOffset
    #region IRelativeDates
    public interface IRelativeDates
    {
        #region Accessors
        IBusinessDayAdjustments GetAdjustments { get;}
        IOffset GetOffset { get; }
        bool ScheduleBoundsSpecified { get;}
        DateTime ScheduleBoundsUnadjustedFirstDate { get;}
        DateTime ScheduleBoundsUnadjustedLastDate { get;}
        bool PeriodSkipSpecified { get;}
        int PeriodSkip { get;}
        #endregion Accessors
    }
    #endregion IRelativeDates
    #region IRelativeDateSequence
    /// EG 20140702 Add DateRelativeToValue|GetAdjustments
    public interface IRelativeDateSequence
    {
        #region Accessors
        IDateOffset[] DateOffset { set; get; }
        string DateRelativeToValue { set; get; }
        IBusinessDayAdjustments GetAdjustments { get; }
        bool BusinessCentersDefineSpecified { set; get; }
        IBusinessCenters BusinessCentersDefine { set; get; }
        bool BusinessCentersReferenceSpecified { set; get; }
        IReference BusinessCentersReference { set; get; }
        #endregion Methods
    }
    #endregion IRelativeDateSequence
    #region IRequiredIdentifierDate
    public interface IRequiredIdentifierDate
    {
        #region Accessors
        DateTime DateValue { set; get;}
        string Value { set; get;}
        string Id { set;get;}
        #endregion Accessors
    }
    #endregion IRequiredIdentifierDate
    #region IResetDates
    public interface IResetDates
    {
        #region Accessors
        IResetFrequency ResetFrequency { set; get;}
        bool ResetRelativeToSpecified { set; get;}
        ResetRelativeToEnum ResetRelativeTo { set; get;}
        EFS_ResetDates Efs_ResetDates { set; get;}
        IBusinessDayAdjustments ResetDatesAdjustments { set; get;}
        bool InitialFixingDateSpecified { set; get;}
        IRelativeDateOffset InitialFixingDate { get;}
        IRelativeDateOffset FixingDates { set; get;}
        bool RateCutOffDaysOffsetSpecified { set; get;}
        IOffset RateCutOffDaysOffset { set; get;}
        IReference CalculationPeriodDatesReference { get;}
        string Id { set;get;}
        #endregion Accessors
    }
    #endregion IResetDates
    #region IResetFrequency
    public interface IResetFrequency
    {
        #region Accessors
        IInterval Interval { get;}
        bool WeeklyRollConventionSpecified { set; get;}
        WeeklyRollConventionEnum WeeklyRollConvention { set; get;}
        #endregion Accessors
    }
    #endregion IResetFrequency
    #region IRestructuring
    public interface IRestructuring
    {
        #region Accessors
        bool RestructuringTypeSpecified { set;get;}
        IScheme RestructuringType { set;get;}
        bool MultipleHolderObligationSpecified { set;get;}
        bool MultipleCreditEventNoticesSpecified { set;get;}
        #endregion Accessors
    }
    #endregion IRestructuring
    #region IReturn
    /// EG 20140702 Add CreateDividendConditions
    public interface IReturn
    {
        #region Accessors
        ReturnTypeEnum ReturnType { set;get;}
        bool DividendConditionsSpecified { set;get;}
        IDividendConditions DividendConditions { set; get; }
        IDividendConditions CreateDividendConditions { get; }
        #endregion Accessors
    }
    #endregion IReturn
    #region IReturnLeg
    /// EG 20140702 Add valuation|CreateValuation|CreateMarginRatio
    /// EG 20140702 Upd rateOfReturn (replace valuation)|CreateRateOfReturn (replace CreateValuation)
    public interface IReturnLeg : IReturnSwapLeg
    {
        #region Accessors
        IAdjustableOrRelativeDate EffectiveDate { get; set; }
        IAdjustableOrRelativeDate TerminationDate { get; set; }
        IReturnSwapAmount ReturnSwapAmount { set; get; }
        IReturnSwapAmount CreateReturnSwapAmount { get; }
        IReturnLegValuation RateOfReturn { set; get; }
        IReturnLegValuation CreateRateOfReturn { get; }
        IReturnSwapNotional Notional { get; }
        IUnderlyer Underlyer { get; }
        EFS_ReturnLeg Efs_ReturnLeg { set; get; }
        bool FxFeatureSpecified { set; get; }
        IFxFeature FxFeature { set; get; }
        IFxFeature CreateFxFeature { get; }
        IReturn Return { set; get; }
        IReturn CreateReturn { get; }
        IMarginRatio CreateMarginRatio { get; }
        bool IsOpenDailyPeriod { get; }
        bool IsDailyPeriod { get; }
        #endregion Accessors
    }
    #endregion IReturnLeg
    #region IReturnLegValuation
    /// EG 20140702 Add notionalResetSpecified|notionalReset|paymentDates|marginRatioSpecified|marginRatio|efs_RateOfReturn
    public interface IReturnLegValuation
    {
        #region Accessors
        bool NotionalResetSpecified { set;  get; }
        EFS_Boolean NotionalReset { set;get;}
        IReturnLegValuationPrice InitialPrice { set;get;}
        bool ValuationPriceInterimSpecified { set;get;}
        IReturnLegValuationPrice ValuationPriceInterim { set;get;}
        IReturnLegValuationPrice ValuationPriceFinal { set;get;}
        IReturnLegValuationPrice CreateReturnLegValuationPrice { get;}
        IReturnSwapPaymentDates PaymentDates { set; get; }
        bool MarginRatioSpecified {set;get;}
        IMarginRatio MarginRatio {set;get;}
        EFS_RateOfReturn Efs_RateOfReturn { set; get; }
        #endregion Accessors
    }
    #endregion IReturnLegValuation
    #region IReturnLegValuationPrice
    /// <summary>
    /// 
    /// </summary>
    /// FI 20150129 [20748] Trnasformation en méthode de de tout ce qui est en Create
    public interface IReturnLegValuationPrice : IPrice
    {
        #region Accessors
        bool ValuationRulesSpecified { set; get; }
        IEquityValuation ValuationRules { set; get; }

        IEquityValuation CreateValuationRules();
        IAdjustableRelativeOrPeriodicDates CreateAdjustableRelativeOrPeriodicDates();
        IPeriodicDates CreatePeriodicDates();
        IActualPrice CreatePrice();
        ICurrency CreateCurrency();
        #endregion Accessors
    }
    #endregion IReturnLegValuationPrice
    #region IReturnSwap
    /// EG 20140702 Add efs_ReturnSwap
    public interface IReturnSwap : IReturnSwapBase 
    {
        #region Accessors
        bool PrincipalExchangeFeaturesSpecified { get; set; }
        IPrincipalExchangeFeatures PrincipalExchangeFeatures { get; set; }
        bool EarlyTerminationSpecified { set;get;}
        IReturnSwapEarlyTermination[] EarlyTermination { set;get;}
        bool ExtraordinaryEventsSpecified { set;get;}
        IExtraordinaryEvents ExtraordinaryEvents { set; get;}
        IExtraordinaryEvents CreateExtraordinaryEvents { get;}
        IReturnSwapEarlyTermination[] CreateEarlyTermination { get;}
        bool AdditionalPaymentSpecified { set;get;}
        IReturnSwapAdditionalPayment[] AdditionalPayment { set; get;}
        EFS_ReturnSwap Efs_ReturnSwap { set; get; }
        #endregion Accessors
    }
    #endregion IReturnSwap

    #region IReturnSwapAdditionalPayment
    public interface IReturnSwapAdditionalPayment
    {
        #region Accessors
        IReference PayerPartyReference { get;}
        IReference ReceiverPartyReference { get;}
        IAdjustableOrRelativeDate AdditionalPaymentDate { get;}
        bool PaymentTypeSpecified { set;get;}
        IScheme PaymentType { set;get;}
        IScheme CreatePaymentType { get;}
        IAdditionalPaymentAmount AdditionalPaymentAmount { get;}
        #endregion Accessors
    }
    #endregion IReturnSwapAdditionalPayment
    #region IReturnSwapAmount
    public interface IReturnSwapAmount : ILegAmount
    {
        #region Accessors
        EFS_Boolean CashSettlement { get;}
        bool OptionsExchangeDividendsSpecified { get;}
        EFS_Boolean OptionsExchangeDividends { get;}
        bool AdditionalDividendsSpecified { get;}
        EFS_Boolean AdditionalDividends { get;}
        #endregion Accessors
    }
    #endregion IReturnSwapAmount
    #region IReturnSwapBase
    /// EG 20140702 Add CreatePartyOrTradeSideReference|returnSwapLegSpecified|returnSwapLeg|rptSide
    public interface IReturnSwapBase 
    {
        #region Accessors
        bool BuyerPartyReferenceSpecified { set; get;}
        IReference BuyerPartyReference { set; get;}
        bool SellerPartyReferenceSpecified { set; get;}
        IReference SellerPartyReference { set; get;}
        bool ReturnLegSpecified { set;get;}
        IReturnLeg[] ReturnLeg { get;}
        bool InterestLegSpecified { set;get;}
        IInterestLeg[] InterestLeg { set;get;}

        /// <summary>
        /// Obtient true si returnLegSpecified ou interestLegSpecified
        /// </summary>
        bool ReturnSwapLegSpecified { get; }
        /// <summary>
        /// Obtient les returnLeg et les interestLeg dans un même array
        /// </summary>
        IReturnSwapLeg[] ReturnSwapLeg { get; }
        // FI 20170116 [21916] RptSide (R majuscule)
        IFixTrdCapRptSideGrp[] RptSide { set; get; }
        #endregion Accessors

        #region Method 
        //FI 20140821 [20275] TODO Il faut créer des méthodes pour les Create
        IReference CreatePartyReference { get; }
        IReference CreatePartyOrTradeSideReference { get; }
        IReturnLeg CreateReturnLeg { get; }
        #endregion Method
    }
    #endregion IReturnSwapBase
    #region IReturnSwapEarlyTermination
    public interface IReturnSwapEarlyTermination
    {
        #region Accessors
        IReference PartyReference { set; get;}
        IStartingDate StartingDate { set; get;}
        IStartingDate CreateStartingDate { get;}
        #endregion Accessors
    }
    #endregion IReturnSwapEarlyTermination
    #region IReturnSwapLeg
    /// EG 20140702 Add LegEventCode|LegEventType
    public interface IReturnSwapLeg
    {
        #region Accessors
        IReference PayerPartyReference { set; get;}
        IReference ReceiverPartyReference { set; get;}
        IReference CreateReference { get;}
        IMoney CreateMoney { get; }

        string LegEventCode { get; }
        string LegEventType { get; }
        #endregion Accessors
    }
    #endregion IReturnSwapLeg
    #region IReturnSwapNotional
    public interface IReturnSwapNotional
    {
        #region Accessors
        bool DeterminationMethodSpecified { get;}
        EFS_MultiLineString DeterminationMethod { get;}
        bool NotionalAmountSpecified { get;}
        IMoney NotionalAmount { get;}
        bool RelativeToSpecified { get;}
        IReference RelativeTo { get;}
        string Id { get;}
        #endregion Accessors
    }
    #endregion IReturnSwapNotional
    #region IReturnSwapPaymentDates
    public interface IReturnSwapPaymentDates
    {
        #region Accessors
        IAdjustableOrRelativeDate PaymentDateFinal { get;}
        bool PaymentDatesInterimSpecified { get;}
        IAdjustableOrRelativeDates PaymentDatesInterim { get;}
        #endregion Accessors
    }
    #endregion IReturnSwapPaymentDates
    #region IRounding
    public interface IRounding
    {
        #region Accessors
        RoundingDirectionEnum RoundingDirection { get;}
        int Precision { get;}
        #endregion Accessors
    }
    #endregion IRounding
    #region IRouting
    public interface IRouting
    {
        #region Accessors
        bool RoutingIdsSpecified { get; set;}
        IRoutingIds RoutingIds { get; set; }
        bool RoutingExplicitDetailsSpecified { get; set;}
        IRoutingExplicitDetails RoutingExplicitDetails { get; set;}
        bool RoutingIdsAndExplicitDetailsSpecified { get;set;}
        IRoutingIdsAndExplicitDetails RoutingIdsAndExplicitDetails { get;set;}
        #endregion Accessors
        #region Methods
        IRouting Clone();
        #endregion Methods
    }
    #endregion IRouting
    #region IRoutingExplicitDetails
    public interface IRoutingExplicitDetails
    {
        #region Accessors
        string RoutingName { get;}
        bool RoutingAccountNumberSpecified { get;}
        string RoutingAccountNumber { get;}
        #endregion Accessors
    }
    #endregion IRoutingExplicitDetails
    #region IRoutingId
    public interface IRoutingId
    {
        #region Accessors
        string RoutingIdCodeScheme { set; get;}
        string Value { set; get;}
        #endregion Accessors

    }
    #endregion IRoutingId
    #region IRoutingIds
    public interface IRoutingIds
    {
        #region Accessors
        IRoutingId[] RoutingId { get; set;}
        void SetRoutingId(ArrayList pRoutingId);
        #endregion Accessors
    }
    #endregion IRoutingIds
    #region IRoutingIdsAndExplicitDetails
    public interface IRoutingIdsAndExplicitDetails
    {
        #region Accessors
        IRoutingIds[] RoutingIds { get; set;}
        EFS_String RoutingName { get; set;}
        //
        bool RoutingAddressSpecified { get; set;}
        IAddress RoutingAddress { get; set;}
        //
        bool RoutingAccountNumberSpecified { get; set;}
        EFS_String RoutingAccountNumber { get; set;}
        //
        bool RoutingReferenceTextSpecified { get; set;}
        EFS_StringArray[] RoutingReferenceText { get; set;}
        #endregion Accessors
    }
    #endregion IRoutingIdsAndExplicitDetails

    #region ISchedule
    public interface ISchedule
    {
        #region Accessors
        EFS_Decimal InitialValue { set; get;}
        bool StepSpecified { get;}
        IStep[] Step { get;}
        EFS_Step[] Efs_Steps { get;}
        bool IsStepCalculated { get;}
        DateTime[] GetStepDatesValue { get;}
        string Id { set;get;}
        #endregion Accessors
        #region Methods
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        Cst.ErrLevel CalcAdjustableSteps(string pCs, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument);
        #endregion Methods
    }
    #endregion ISchedule
    #region IScheme
    public interface IScheme
    {
        #region Accessors
        string Scheme { set;get;}
        string Value { set;get;}
        #endregion Accessors
    }
    #endregion IScheme
    #region ISchemeId
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Add source)
    public interface ISchemeId : IScheme
    {
        #region Accessors
        string Id { set; get; }
        string Source { set; get; }
        #endregion Accessors
    }
    #endregion ISchemeId
    /// EG 20240227 [WI855][WI858] Trade input : New
    public interface ITradeId : ISchemeId
    {
        #region Accessors
        /// <summary>
        /// Les TradeId sont considérés comme non relatifs à un actor pour
        /// les schemes : 
        /// - Spheres_TradeIdMarketTransactionIdScheme
        /// - Cst.Spheres_TradeIdTvticScheme
        /// </summary>
        bool IsRelativeToActor { get; }
        #endregion Accessors
    }

    #region ISecurityExchanges
    public interface ISecurityExchanges
    {
        #region Accessors
        EFS_Boolean InitialExchange { set;get;}
        EFS_Boolean FinalExchange { set;get;}
        EFS_Boolean IntermediateExchange { set;get;}
        #endregion Accessors
    }
    #endregion ISecurityExchanges
    #region ISettlementInformation
    public interface ISettlementInformation
    {
        #region Accessors
        bool StandardSpecified { set; get;}
        StandardSettlementStyleEnum Standard { set; get;}
        bool InstructionSpecified { set; get;}
        ISettlementInstruction Instruction { set; get;}
        #endregion Accessors
    }
    #endregion ISettlementInformation
    #region ISettlementInstruction
    public interface ISettlementInstruction
    {
        #region Accessors
        bool SettlementMethodSpecified { get;}
        IScheme SettlementMethod { get;}
        bool CorrespondentInformationSpecified { get;}
        IRouting CorrespondentInformation { get;}
        bool IntermediaryInformationSpecified { get;}
        IRouting[] IntermediaryInformation { get;}
        IRouting Beneficiary { get;}
        /*
        bool beneficiaryBankSpecified { get;}
        IRouting beneficiaryBank { get;}
        bool depositoryPartyReferenceSpecified { get;}
        IReference depositoryPartyReference { get;}
        bool splitSettlementSpecified { get;}
        ISplitSettlement[] splitSettlement { get;}
        */

        #endregion Accessors
        #region Methods
        IEfsSettlementInstruction CreateEfsSettlementInstruction();
        IEfsSettlementInstruction[] CreateEfsSettlementInstructions();
        IEfsSettlementInstruction[] CreateEfsSettlementInstructions(IEfsSettlementInstruction pEfsSettlementInstruction);
        #endregion Methods

    }
    #endregion ISettlementInstruction
    #region ISharedAmericanExercise
    public interface ISharedAmericanExercise : IExerciseId
    {
        #region Accessors
        IAdjustableOrRelativeDate CommencementDate { set;get;}
        IAdjustableOrRelativeDate ExpirationDate { set;get;}
        bool LatestExerciseTimeSpecified { set;get;}
        IBusinessCenterTime LatestExerciseTime { set;get;}
        #endregion Accessors
    }
    #endregion ISharedAmericanExercise

    #region ISimplePayment
    public interface ISimplePayment
    {
        IReference PayerPartyReference { get; set; }
        IReference ReceiverPartyReference { get; set; }
        IAdjustableOrRelativeAndAdjustedDate PaymentDate { get; set; }
        IMoney PaymentAmount { get; set; }
    }
    #endregion

    #region ISideRate
    public interface ISideRate
    {
        #region Accessors
        string Currency { get;}
        SideRateBasisEnum SideRateBasis { set; get;}
        EFS_Decimal Rate { get;}
        bool SpotRateSpecified { get;}
        EFS_Decimal SpotRate { get;}
        bool ForwardPointsSpecified { get;}
        EFS_Decimal ForwardPoints { get;}
        #endregion Accessors
        #region Methods
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        #endregion Methods
    }
    #endregion ISideRate
    #region ISideRates
    public interface ISideRates
    {
        #region Accessors
        ICurrency BaseCurrency { get;}
        bool Currency1SideRateSpecified { get;}
        ISideRate Currency1SideRate { get;}
        bool Currency2SideRateSpecified { get;}
        ISideRate Currency2SideRate { get;}
        #endregion Methods
    }
    #endregion ISideRates

    #region IReturnLegMainUnderlyer
    /// EG 20140702 New
    /// EG 20150302 CFD Forex Add notionalBase|notionalBaseSpecified
    /// EG 20170510 [23153] Add SetAsset 
    public interface IReturnLegMainUnderlyer
    {
        bool InstrumentIdSpecified { get; }
        IScheme[] InstrumentId { set; get; }
        bool CurrencySpecified { set; get; }
        ICurrency Currency { set; get; }
        bool ExchangeIdSpecified { set; get; }
        ISpheresIdScheme ExchangeId { set; get; }
        bool OpenUnitsSpecified { set; get; }
        EFS_Decimal OpenUnits { set; get; }
        bool SqlAssetSpecified {get; }
        SQL_AssetBase SqlAsset { set; get; }
        Nullable<int> OTCmlId { set; get; }
        Nullable<Cst.UnderlyingAsset> UnderlyerAsset {get;}
        bool NotionalBaseSpecified { get; }
        IMoney NotionalBase { get; }
        // EG 20170510 [23153]
        void SetAsset(string pCS, IDbTransaction pDbTransaction);
    }
    #endregion IReturnLegMainUnderlyer
    #region ISingleUnderlyer
    /// EG 20150302 CFD Forex Add notionalBase|notionalBaseSpecified
    public interface ISingleUnderlyer : IOpenUnits
    {
        #region Accessors
        IUnderlyingAsset UnderlyingAsset { set;get;}
        bool DividendPayoutSpecified { set; get;}
        IDividendPayout DividendPayout { set; get;}
        bool NotionalBaseSpecified { set; get; }
        IMoney NotionalBase { set; get; }
        #endregion Accessors
    }
    #endregion ISingleUnderlyer
    #region ISpreadSchedule
    public interface ISpreadSchedule : ISchedule
    {
        #region Accessors
        ISpreadScheduleType Type { get;}
        void CreateSpreadScheduleType(string pValue);
        #endregion Members
    }
    #endregion ISpreadSchedule
    #region ISpreadSchedule
    // EG 20150309 POC - BERKELEY New
    public interface ISpreadScheduleType : IScheme
    {
        #region Accessors
        //string Value { get;}
        #endregion Members
    }
    #endregion ISpreadScheduleType
    #region IStartingDate
    public interface IStartingDate
    {
        #region Accessors
        bool RelativeToSpecified { set; get;}
        string DateRelativeTo { set;get;}
        bool AdjustableDateSpecified { get;}
        IAdjustableDate AdjustableDate { get;}
        #endregion Accessors
    }
    #endregion IStartingDate
    #region IStep
    public interface IStep
    {
        #region Accessors
        EFS_Date StepDate { get;}
        EFS_Decimal StepValue { get;}
        #endregion Accessors
    }
    #endregion IStep
    #region IStrategy
    public interface IStrategy : IProduct
    {
        #region Accessors
        object[] SubProduct { get; set; }

        bool PremiumProductReferenceSpecified { get; set; }
        IReference PremiumProductReference { get; set; }

        bool MainProductReferenceSpecified { get; set; }
        IReference MainProductReference { get; set; }
        
        #endregion Accessors
        
        #region Indexors
        IProduct this[int pIndex] { get;}
        #endregion Indexors
    }
    #endregion IStrategy
    #region IStrategyFeature
    public interface IStrategyFeature
    {
        #region Accessors
        bool CalendarSpreadSpecified { set;get;}
        ICalendarSpread CalendarSpread { set;get;}
        bool StrikeSpreadSpecified { set;get;}
        IStrikeSpread StrikeSpread { set;get;}
        #endregion Accessors
    }
    #endregion IStrategyFeature
    #region IStreetAddress
    public interface IStreetAddress
    {
        #region Accessors
        EFS_StringArray[] StreetLine { get; set;}
        #endregion Accessors
    }
    #endregion IStreetAddress
    #region IStrikeSchedule
    public interface IStrikeSchedule : ISchedule
    {
        #region Accessors
        bool BuyerSpecified { set; get;}
        PayerReceiverEnum Buyer { set; get;}
        bool SellerSpecified { set; get;}
        PayerReceiverEnum Seller { set; get;}
        #endregion Accessors
    }
    #endregion IStrikeSchedule
    #region IStrikeSpread
    public interface IStrikeSpread
    {
        #region Accessors
        IOptionStrike UpperStrike { set;get;}
        EFS_Decimal UpperStrikeNumberOfOptions { set;get;}
        #endregion Accessors
    }
    #endregion IStrikeSpread
    #region IStub
    public interface IStub
    {
        #region Accessors
        bool StubTypeFloatingRateSpecified { set; get;}
        IFloatingRate[] StubTypeFloatingRate { set; get;}
        bool StubTypeFixedRateSpecified { set; get;}
        EFS_Decimal StubTypeFixedRate { set; get;}
        bool StubTypeAmountSpecified { set; get;}
        IMoney StubTypeAmount { set; get;}
        bool StubStartDateSpecified { get;}
        IAdjustableOrRelativeDate StubStartDate { get;}
        bool StubEndDateSpecified { get;}
        IAdjustableOrRelativeDate StubEndDate { get;}
        IFloatingRate[] CreateFloatingRate { get;}
        IMoney CreateMoney { get;}
        #endregion Accessors
        #region Methods
        #endregion Methods
    }
    #endregion IStub
    #region IStubCalculationPeriod
    public interface IStubCalculationPeriod
    {
        #region Accessors
        bool InitialStubSpecified { get;}
        IStub InitialStub { get;}
        bool FinalStubSpecified { get;}
        IStub FinalStub { get;}
        #endregion Accessors
    }
    #endregion IStubCalculationPeriod
    #region IStubCalculationPeriodAmount
    public interface IStubCalculationPeriodAmount
    {
        #region Accessors
        IReference CalculationPeriodDatesReference { get;}
        bool InitialStubSpecified { set; get;}
        IStub InitialStub { set; get;}
        bool FinalStubSpecified { set; get;}
        IStub FinalStub { set; get;}
        IStub CreateStub { get;}
        #endregion Accessors
    }
    #endregion IStubCalculationPeriodAmount
    #region ISwap
    public interface ISwap
    {
        #region Accessors
        IInterestRateStream[] Stream { get; }
        bool CancelableProvisionSpecified { get; }
        ICancelableProvision CancelableProvision { get; }
        bool ExtendibleProvisionSpecified { get; }
        IExtendibleProvision ExtendibleProvision { get; }
        bool EarlyTerminationProvisionSpecified { get; }
        IEarlyTerminationProvision EarlyTerminationProvision { set; get; }
        bool ImplicitProvisionSpecified { set; get; }
        IImplicitProvision ImplicitProvision { set; get; }
        bool ImplicitCancelableProvisionSpecified { get; }
        bool ImplicitOptionalEarlyTerminationProvisionSpecified { get; }
        bool ImplicitMandatoryEarlyTerminationProvisionSpecified { get; }
        bool ImplicitExtendibleProvisionSpecified { get; }
        bool StepUpProvisionSpecified { get; }
        IStepUpProvision StepUpProvision { get; }
        bool AdditionalPaymentSpecified { set; get; }
        IPayment[] AdditionalPayment { get; }
        EFS_EventDate MaxTerminationDate { get; }
        bool IsPaymentDatesSynchronous { get; set; }
        #endregion Accessors
    }
    #endregion ISwap
    #region ISwapCurveValuation
    // EG 20150410 [20513] BANCAPERTA
    public interface ISwapCurveValuation
    {
        #region Accessors
        IFloatingRateIndex FloatingRateIndex { set; get; }
        bool IndexTenorSpecified { set; get; }
        IInterval IndexTenor { set; get; }
        EFS_Decimal Spread { set; get; }
        bool SideSpecified { set; get; }
        QuotationSideEnum Side { set; get; }
        #endregion Accessors
    }
    #endregion ISwapCurveValuation

    #region ISwaption
    public interface ISwaption
    {
        #region Accessors
        IReference BuyerPartyReference { set; get;}
        IReference SellerPartyReference { set; get;}
        object EFS_Exercise { set; get;}
        ExerciseStyleEnum GetStyle { get;}
        bool ExerciseAmericanSpecified { set; get;}
        IAmericanExercise ExerciseAmerican { set; get;}
        bool ExerciseBermudaSpecified { set; get;}
        IBermudaExercise ExerciseBermuda { set; get;}
        bool ExerciseEuropeanSpecified { set; get;}
        IEuropeanExercise ExerciseEuropean { set; get;}
        bool PremiumSpecified { set; get;}
        IPayment[] Premium { set; get;}
        bool ExerciseProcedureSpecified { set; get;}
        IExerciseProcedure ExerciseProcedure { set; get;}
        bool CalculationAgentSpecified { set; get;}
        ICalculationAgent CalculationAgent { set; get;}
        bool CashSettlementSpecified { get;}
        ICashSettlement CashSettlement { set; get;}
        EFS_Boolean SwaptionStraddle { set; get;}
        bool SwaptionAdjustedDatesSpecified { get;}
        ISwap Swap { set; get;}
        EFS_SwaptionDates Efs_SwaptionDates { set; get;}
        #endregion Accessors
    }
    #endregion ISwaption

    #region ITermDeposit
    public interface ITermDeposit
    {
        #region Accessors
        EFS_Date StartDate { set; get;}
        EFS_Date MaturityDate { set; get;}
        // FI 20140909 [20340] fixedRate est une donnée obligatoire (voir FpML) 
        //bool fixedRateSpecified { get;}
        EFS_Decimal FixedRate { set; get;}
        bool InterestSpecified { set; get;}
        IMoney Interest { get;}
        IMoney Principal { get;}
        IReference InitialPayerReference { set; get;}
        IReference InitialReceiverReference { set; get;}
        DayCountFractionEnum DayCountFraction { set; get;}
        EFS_TermDeposit Efs_TermDeposit { set;get;}
        bool PaymentSpecified { set;get;}
        IPayment[] Payment { get;}
        #endregion Accessors
    }
    #endregion ITermDeposit
    #region ITrade
    public interface ITrade
    {
        #region Accessors
        ITradeHeader TradeHeader { get; set; }
        IProduct Product { get; set; }
        
        bool ExtendsSpecified { get; set;}
        ITradeExtends Extends { get;}

        bool OtherPartyPaymentSpecified { set; get;}
        IPayment[] OtherPartyPayment { set; get; }
        
        EFS_Events ProductEvents { set; get;}
        
        bool BrokerPartyReferenceSpecified { set; get;}
        IReference[] BrokerPartyReference { set; get;}
        
        bool SettlementInputSpecified { set; get;}
        ISettlementInput[] SettlementInput { get;}
        
        bool NettingInformationInputSpecified { set; get;}
        INettingInformationInput NettingInformationInput { set; get;}
        
        bool CalculationAgentSpecified { set; get;}
        ICalculationAgent CalculationAgent { set; get;}
        
        DateTime AdjustedTradeDate { get;}
        
        bool GoverningLawSpecified { set; get;}
        IScheme GoverningLaw { set; get;}
        
        bool TradeSideSpecified { set; get;}
        ITradeSide[] TradeSide { set; get;}
        
        bool DocumentationSpecified { set; get;}
        IDocumentation Documentation { set; get;}
        
        bool TradeIntentionSpecified { set; get;}
        ITradeIntention TradeIntention { set; get;}


        bool TradeIdSpecified { set; get; }
        /// <summary>
        /// Représente l'identifier du trade sous Spheres®
        /// </summary>
        /// FI 20130621 [18745] add property
        /// Dans le cadre de la messagerie, le flux trade est réduit à son minimum. L'élément tradeHeader n'existe pas 
        /// L'identifiant du trade est renseigné via cette property
        string TradeId { set; get; }


        IParty CreateParty();
        IDocumentation CreateDocumentation();
        ISettlementChain CreateSettlementChain();
        IIssiItemsRoutingActorsInfo CreateIssiItemsRoutingActorsInfo(string pConnectionString, int pIdIssi, IssiItem[] pIssiItem);
        
        /// <summary>
        /// Charge OtherPartyPayment
        /// </summary>
        /// <param name="pOtherPartyPayment"></param>
        /// EG 20101020 [17185]
        void SetOtherPartyPayment(ArrayList pOtherPartyPayment);
        #endregion Accessors
    }
    #endregion ITrade
    #region ITradeDate
    /// EG 20140702 Add businessDate|BusinessDate
    public interface ITradeDate
    {
        #region Accessors
        string Efs_id { set;get;}
        /// <summary>
        /// Date valeur au format ISO yyyy-MM-dd
        /// </summary>
        string Value { set; get;}
        /// <summary>
        /// Obtient date valeur 
        /// </summary>
        DateTime DateValue { get;}
        /// <summary>
        /// Obtient TimeStamp
        /// </summary>
        DateTime DateTimeValue { get;}
        
        /// <summary>
        /// Obtient ou définie timestamp au format HH:MM:SS
        /// </summary>
        string TimeStampHHMMSS { set; get;}
        /// <summary>
        /// <para>Obtient DateValue + timeStamp</para>
        /// <para>Définie timeStamp </para>
        /// </summary>
        DateTime TimeStamp { set; get;}

        /// <summary>
        /// Date business au format ISO YYYY-MM-DD
        /// </summary>
        string BusinessDate { set; get; }
        /// <summary>
        /// Date business 
        /// </summary>
        /// EG 20171016 [23509] Unused
        //DateTime BusinessDate { set; get; }
        #endregion Accessors
    }
    #endregion ITradeDate
    #region ITradeHeader
    // EG 20171016 [23509] clearedDate
    public interface ITradeHeader
    {
        #region Accessors
        IPartyTradeIdentifier[] PartyTradeIdentifier { set; get; }
        bool PartyTradeInformationSpecified { set; get;}
        IPartyTradeInformation[] PartyTradeInformation { set; get;}
        ITradeDate TradeDate { get;}
        DateTime AdjustedTradeDate { get;}
        bool ClearedDateSpecified { get; set; }
        /// <summary>
        /// ClearedDate au format ISO YYYY-MM-DD
        /// </summary>
        IAdjustedDate ClearedDate { get; set; }
        #endregion Accessors
    }
    #endregion ITradeHeader
    #region ITradeIdentifier
    public interface ITradeIdentifier
    {
        #region Methods
        string GetTradeIdMemberName();
        ISchemeId GetTradeIdFromScheme(string pScheme);
        ISchemeId GetTradeIdWithNoScheme();
        void RemoveTradeIdFromScheme(string pScheme);
        // EG 20240227 [WI855] New
        void RemoveTradeId(string pScheme);
        // EG 20240227 [WI855] New
        void SetTradeId(string pScheme, string pValue);
        #endregion Accessors
    }
    #endregion ITradeIdentifier
    #region ITradeSide
    public interface ITradeSide
    {
        #region Accessors
        bool ConfirmerSpecified { set; get;}
        IPartyRole Confirmer { set; get;}
        bool SettlerSpecified { set; get;}
        IPartyRole Settler { set; get;}
        IPartyRole Creditor { set; get;}
        string Id { set; get;}
        #endregion Accessors
    }
    #endregion ITradeSide

    #region ITrader
    public interface ITrader
    {
        #region Accessors
        string Identifier { set; get;}
        decimal Factor { set; get;}
        string StrFactor { set; get;}
        string Name { set; get;}
        string OtcmlId { set;get;}
        int OTCmlId { set;get;}
        string Scheme { set;get;}
        #endregion Accessors
    }
    #endregion ITrader
    #region ITrigger
    public interface ITrigger
    {
        #region Accessors
        bool LevelSpecified { set;get;}
        EFS_Decimal Level { set;get;}
        bool LevelPercentageSpecified { set;get;}
        EFS_Decimal LevelPercentage { set;get;}
        bool CreditEventsSpecified { set;get;}
        ICreditEvents CreditEvents { set;get;}
        bool CreditEventsReferenceSpecified { set;get;}
        IReference CreditEventsReference { set;get;}
        #endregion Accessors
    }
    #endregion ITrigger
    #region ITriggerEvent
    public interface ITriggerEvent
    {
        #region Accessors
        bool ScheduleSpecified { set;get;}
        IAveragingSchedule[] Schedule { set;get;}
        bool TriggerDatesSpecified { set;get;}
        IDateList TriggerDates { set;get;}
        ITrigger Trigger { set;get;}
        bool FeaturePaymentSpecified { set;get;}
        IFeaturePayment FeaturePayment { set;get;}
        #endregion Accessors
    }
    #endregion ITriggerEvent

    #region IUnderlyer
    public interface IUnderlyer
    {
        #region Accessors
        bool UnderlyerSingleSpecified { set;get;}
        ISingleUnderlyer UnderlyerSingle { set; get;}
        bool UnderlyerBasketSpecified { get; set; }
        IBasket UnderlyerBasket { get; set; }
        #endregion Accessors
    }
    #endregion IUnderlyer
    #region IUnderlyingAsset
    /// <summary>
    /// 
    /// </summary>
    /// FI 20140812 [XXXXX] Modify 
    public interface IUnderlyingAsset
    {
        #region Accessors
        IScheme[] InstrumentId { set; get; }
        bool DescriptionSpecified { set; get; }
        EFS_String Description { set; get; }
        bool ExchangeIdSpecified { set; get; }
        ISpheresIdScheme ExchangeId { set; get; }
        bool CurrencySpecified { set; get; }
        ICurrency Currency { set; get; }
        bool ClearanceSystemSpecified { set; get; }
        IScheme ClearanceSystem { set; get; }
        bool DefinitionSpecified { set; get; }
        IReference Definition { set; get; }
        int OTCmlId { set; get; }
        string UnderlyerEventType { get; }
        /// <summary>
        /// Obtient la catégorie de l'asset
        /// </summary>
        /// FI 20140812 [XXXXX] add property 
        Cst.UnderlyingAsset UnderlyerAssetCategory { get; }
        ICurrency CreateCurrency(string pIdC);
        ISpheresIdScheme CreateExchangeId(string pIdM);
        IScheme[] CreateInstrumentId(string pId);
        #endregion Accessors
    }
    #endregion IUnderlyingAsset

    #region IVarianceLeg
    public interface IVarianceLeg : IReturnSwapLeg
    {
        #region Accessors
        #endregion Accessors
    }
    #endregion IVarianceLeg
    #region IVersionedSchemeId
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Remplace coquille sur IVersionSchemeId)
    public interface IVersionedSchemeId : ISchemeId
    {
        #region Accessors
        bool EffectiveDateSpecified { set;get;}
        IAdjustedDate EffectiveDate { set;get;}
        #endregion
    }
    #endregion IVersionedSchemeId

    #region IYieldCurveMethod
    public interface IYieldCurveMethod
    {
        #region Accessors
        bool SettlementRateSourceSpecified { set;get;}
        //ISettlementRateSource settlementRateSource;
        QuotationRateTypeEnum QuotationRateType { set;get;}
        #endregion Accessors
    }
    #endregion IYieldCurveMethod

}

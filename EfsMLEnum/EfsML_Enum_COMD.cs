#region using directives
using System;
using System.Xml.Serialization;
using EfsML.Enum.Tools;
#endregion using directives

//Enumerateurs exclusivement utilisé partial les Commodities Derivatives
namespace EfsML.Enum
{
    #region CalendarSourceEnum
    /// EG 20161122 New Commodity Derivative
    public enum CalendarSourceEnum
    {
        ListedOption,
        Future,
    }
    #endregion CalendarSourceEnum
    #region CommodityBullionSettlementDisruptionEnum
    /// EG 20161122 New Commodity Derivative
    public enum CommodityBullionSettlementDisruptionEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("Negotiation")]
        Negotiation,
        [System.Xml.Serialization.XmlEnumAttribute("Cancellation and Payment")]
        CancellationAndPayment,
    }
    #endregion CommodityBullionSettlementDisruptionEnum
    #region CommodityContractClassEnum
    public enum CommodityContractClassEnum
    {
        Coal,
        Electricity,
        Environmental,
        Gas,
        Oil,
        Undefined,
    }
    #endregion
    #region CommodityDayTypeEnum
    public enum CommodityDayTypeEnum
    {
        Business,
        Calendar,
        CommodityBusiness,
        CurrencyBusiness,
        ExchangeBusiness,
        ScheduledTradingDay,
        GasFlow,
        NearbyContractDay,
    }
    #endregion CommodityDayTypeEnum
    #region CommodityPayRelativeToEnum
    public enum CommodityPayRelativeToEnum
    {
		CalculationPeriodStartDate,
		CalculationPeriodEndDate,
        LastPricingDate,
		ResetDate,
        ValuationDate,
        CalculationDate,
        CalculationPeriodMonthEnd,
        CalculationPeriodMonthStart,
        EffectiveDate,
        PricingPeriodMonthEnd,
        TerminationOrExpirationDate,
        TradeDate,
        PricingPeriodEndOfWeek,
        FirstPricingDate,
    }
    #endregion CommodityPayRelativeToEnum
    #region CommodityPriceUnitEnum
    public enum CommodityPriceUnitEnum
    {
        CalculationPeriodStartDate,
        CalculationPeriodEndDate,
        LastPricingDate,
        ResetDate,
        ValuationDate,
        CalculationDate,
        CalculationPeriodMonthEnd,
        CalculationPeriodMonthStart,
        EffectiveDate,
        PricingPeriodMonthEnd,
        TerminationOrExpirationDate,
        TradeDate,
        PricingPeriodEndOfWeek,
        FirstPricingDate,
    }
    #endregion CommodityPriceUnitEnum
    #region CommodityQuantityFrequencyEnum
    public enum CommodityQuantityFrequencyEnum
    {
        PerBusinessDay,
        PerCalculationPeriod,
        PerCalendarDay,
        PerHour,
        PerMonth,
        PerSettlementPeriod,
        InFine,
        Term,
    }
    #endregion CommodityQuantityFrequencyEnum

    #region DayOfWeekEnum
    public enum DayOfWeekEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("MON")]
		Monday,
        [System.Xml.Serialization.XmlEnumAttribute("TUE")]
		Tuesday,
        [System.Xml.Serialization.XmlEnumAttribute("WED")]
		Wednesday,
        [System.Xml.Serialization.XmlEnumAttribute("THU")]
        Thursday,
        [System.Xml.Serialization.XmlEnumAttribute("FRI")]
        Friday,
        [System.Xml.Serialization.XmlEnumAttribute("SAT")]
        Saturday,
        [System.Xml.Serialization.XmlEnumAttribute("SUN")]
        Sunday,
    }
    #endregion DayOfWeekEnum
    #region DayOfWeekExtEnum
    public enum DayOfWeekExtEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("MON")]
		Monday,
        [System.Xml.Serialization.XmlEnumAttribute("TUE")]
		Tuesday,
        [System.Xml.Serialization.XmlEnumAttribute("WED")]
		Wednesday,
        [System.Xml.Serialization.XmlEnumAttribute("THU")]
        Thursday,
        [System.Xml.Serialization.XmlEnumAttribute("FRI")]
        Friday,
        [System.Xml.Serialization.XmlEnumAttribute("SAT")]
        Saturday,
        [System.Xml.Serialization.XmlEnumAttribute("SUN")]
        Sunday,
        [System.Xml.Serialization.XmlEnumAttribute("WD")]
        Weekdays,
        [System.Xml.Serialization.XmlEnumAttribute("WN")]
        Weekends,
    }
    #endregion DayOfWeekExtEnum
    #region DeliveryDatesEnum
    public enum DeliveryDatesEnum
    {
      CalculationPeriod,
      FirstNearby,
      SecondNearby,
      ThirdNearby,
      FourthNearby,
      FifthNearby,
      SixthNearby,
      SeventhNearby,
      EighthNearby,
      NinthNearby,
      TenthNearby,
      EleventhNearby,
      TwelfthNearby,
      ThirteenthNearby,
      FourteenthNearby,
      FifteenthNearby,
      SixteenthNearby,
      SeventeenthNearby,
      EighteenthNearby,
      NineteenthNearby,
      TwentiethNearby,
      TwentyFirstNearby,
      TwentySecondNearby,
      TwentyThirdNearby,
      TwentyFourthNearby,
      TwentyFifthNearby,
      TwentySixthNearby,
      TwentySeventhNearby,
      TwentyEighthNearby,
      TwentyNinthNearby,
      ThirtiethNearby,
      ThirtyFirstNearby,
      ThirtySecondNearby,
      ThirtyThirdNearby,
      ThirtyFourthNearby,
      ThirtyFifthNearby,
      ThirtySixthNearby,
      ThirtySeventhNearby,
      ThirtyEighthNearby,
      ThirtyNinthNearby,
      FortiethNearby,
      FortyFirstNearby,
      FortySecondNearby,
      FortyThirdNearby,
      FortyFourthNearby,
      FortyFifthNearby,
      FortySixthNearby,
      FortySeventhNearby,
      FortyEighthNearby,
      FortyNinthNearby,
      FiftiethNearby,
      FiftyFirstNearby,
      FiftySecondNearby,
      FiftyThirdNearby,
      FiftyFourthNearby,
      FiftyFifthNearby,
      FiftySixthNearby,
      FiftySeventhNearby,
      FiftyEighthNearby,
      FiftyNinthNearby,
      Spot,
      FirstNearbyWeek,
      SecondNearbyWeek,
      ThirdNearbyWeek,
      FourthNearbyWeek,
      FifthNearbyWeek,
      SixthNearbyWeek,
      SeventhNearbyWeek,
      EighthNearbyWeek,
      NinthNearbyWeek,
      TenthNearbyWeek,
      EleventhNearbyWeek,
      TwelfthNearbyWeek,
      ThirteenthNearbyWeek,
      FourteenthNearbyWeek,
      FifteenthNearbyWeek,
      SixteenthNearbyWeek,
      SeventeenthNearbyWeek,
      EighteenthNearbyWeek,
      NineteenthNearbyWeek,
      TwentiethNearbyWeek,
      TwentyFirstNearbyWeek,
      TwentySecondNearbyWeek,
      TwentyThirdNearbyWeek,
      TwentyFourthearbyWeek,
      TwentyFifthNearbyWeek,
      TwentySixthNearbyWeek,
      TwentySeventhNearbyWeek,
      TwentyEighthNearbyWeek,
      TwentyNinthNearbyWeek,
      ThirtiethNearbyWeek,
      ThirtyFirstNearbyWeek,
      ThirtySecondNearbyWeek,
      ThirtyThirdNearbyWeek,
      ThirtyFourthNearbyWeek,
      ThirtyFifthNearbyWeek,
      ThirtySixthNearbyWeek,
      ThirtySeventhNearbyWeek,
      ThirtyEighthNearbyWeek,
      ThirtyNinthNearbyWeek,
      FortiethNearbyWeek,
      FortyFirstNearbyWeek,
      FortySecondNearbyWeek,
      FortyThirdNearbyWeek,
      FortyFourthNearbyWeek,
      FortyFifthNearbyWeek,
      FortySixthNearbyWeek,
      FortySeventhNearbyWeek,
      FortyEighthNearbyWeek,
      FortyNinthNearbyWeek,
      FiftiethNearbyWeek,
      FiftyFirstNearbyWeek,
      FiftySecondNearbyWeek,
    }
    #endregion DeliveryDatesEnum
    #region DeliveryNearbyTypeEnum
    /// EG 20161122 New Commodity Derivative
    public enum DeliveryNearbyTypeEnum
    {
        CalculationPeriod,
        NearbyMonth,
        NearbyWeek,
    }
    #endregion DeliveryNearbyTypeEnum
    #region CommodityDeliveryTypeEnum
    /// EG 20161122 New Commodity Derivative
    public enum CommodityDeliveryTypeEnum
    {
        Firm,
        Interruptible,
    }
    #endregion CommodityDeliveryTypeEnum
    #region DisruptionFallbacksEnum
    /// EG 20161122 New Commodity Derivative
    public enum DisruptionFallbacksEnum
    {
        AsSpecifiedInMasterAgreement,
        AsSpecifiedInConfirmation,
    }
    #endregion DisruptionFallbacksEnum
    #region ElectricityProductTypeEnum
    /// EG 20161122 New Commodity Derivative
    /// EG 20221201 [25639] [WI482] Add Undefined
    public enum ElectricityProductTypeEnum
    {
        Electricity,
        Undefined,
    }
    #endregion ElectricityProductTypeEnum

    /// EG 20221201 [25639] [WI482] New
    public enum CommodityTechnologyTypeEnum
    {
        Hydro,
        OffShoreWind,
        OnShoreWind,
        Solar,
        Thermal,
    }
    #region EnvironmentalAbandonmentOfSchemeEnum
    /// EG 20161122 New Commodity Derivative
    public enum EnvironmentalAbandonmentOfSchemeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("OptionA(1)")]
        OptionA_1,
        [System.Xml.Serialization.XmlEnumAttribute("OptionA(2)")]
        OptionA_2,
        [System.Xml.Serialization.XmlEnumAttribute("OptionB")]
        OptionB,
        [System.Xml.Serialization.XmlEnumAttribute("OptionC")]
        OptionC,
    }
    #endregion EnvironmentalAbandonmentOfSchemeEnum
    #region EnvironmentalProductTypeEnum
    /// EG 20221201 [25639] [WI482] Add GoO et Undefined
    public enum EnvironmentalProductTypeEnum
    {
        EUAllowance,
        EUCredit,
        AlternativeAllowance,
        NOXEmissionsProduct,
        RegionalEmissionsProduct,
        RGGIEmissionsProduct,
        SO2EmissionsProduct,
        StateEmissionProduct,
        VoluntaryEmissionProduct,
        RenewableEnergyCertificate,
        AUSCarbonCreditUnit,
        AUSCarbonUnit,
        AUSEnergySavingCertificate,
        AUSLargeScaleGenerationCertificate,
        AUSSmallScaleTechnologyCertificate,
        AUSVictorianEnergyEfficiencyCertificate,
        MXCCFECRTINTLODS,
        NZEmissionsUnits,
        UKRenewableObligationCertificate,
        EUGuaranteeOfOrigin,
        FRGuaranteeOfOrigin,
        Undefined,
    }
    #endregion EnvironmentalAbandonmentOfSchemeEnum
    #region FlatRateEnum
    /// EG 20161122 New Commodity Derivative
    public enum FlatRateEnum
    {
        Fixed,
        Floating,
    }
    #endregion FlatRateEnum
    #region GasProductTypeEnum
    /// EG 20161122 New Commodity Derivative
    /// EG 20221201 [25639] [WI482] Add undefined
    public enum GasProductTypeEnum
    {
        Butane,
        CarbonDioxide,
        EPMix,
        Ethane,
        Gasoline,
        Helium,
        HydrogenSulfide,
        Isobutane,
        Methane,
        Naphtha,
        NaturalGas,
        Nitrogen,
        Pentane,
        Propane,
        Propylene,
        Water,
        Undefined,
    }
    #endregion GasProductTypeEnum

    #region LoadTypeEnum
    /// EG 20161122 New Commodity Derivative
    public enum LoadTypeEnum
    {
        Base,
        Peak,
        OffPeak,
        BlockHours,
        Custom,
    }
    #endregion LoadTypeEnum
    #region MarketDisruptionEventsEnum
    /// EG 20161122 New Commodity Derivative
    public enum MarketDisruptionEventsEnum
    {
        Applicable,
        NotApplicable,
        AsSpecifiedInMasterAgreement,
        AsSpecifiedInConfirmation,
    }
    #endregion MarketDisruptionEventsEnum
    #region PayRelativeToExtendedEnum
    /// EG 20161122 New Commodity Derivative (Extension à PayRelativeTo Fpml.4.4
    public enum PayRelativeToExtendedEnum
    {
        CalculationPeriodStartDate,
        CalculationPeriodEndDate,
        LastPricingDate,
        ResetDate,
        ValuationDate,
    }
    #endregion PayRelativeToEnum

    #region SettlementPeriodDurationEnum
    /// <summary>
    /// Specifies a set of Settlement Periods associated with an Electricity Transaction for delivery on an Applicable Day or for a series of Applicable Days.
    /// </summary>
    public enum SettlementPeriodDurationEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("2Hours")]
        TwoHours,
        [System.Xml.Serialization.XmlEnumAttribute("1Hour")]
        OneHour,
        [System.Xml.Serialization.XmlEnumAttribute("30Minutes")]
        ThirtyMinutes,
        [System.Xml.Serialization.XmlEnumAttribute("15Minutes")]
        FifteenMinutes,
    }
    #endregion SettlementPeriodDurationEnum
    #region SpecifiedPriceEnum
    /// EG 20161122 New Commodity Derivative
    public enum SpecifiedPriceEnum
    {
        Afternoon,
        Ask,
        Bid,
        Closing,
        High,
        Index,
        MeanOfBidAndAsk,
        LocationalMarginal,
        Low,
        MarginalHourly,
        MarketClearing,
        MeanOfHighAndLow,
        Morning,
        Official,
        Opening,
        OSP,
        Settlement,
        Spot,
        Midpoint,
        NationalSingle,
        WeightedAverage,
        UnWeightedAverage,
    }
    #endregion SpecifiedPriceEnum
}

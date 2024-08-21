#region using directives
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using FixML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
#endregion using directives


namespace EfsML.Interface
{
    #region IPayerReceiverPartyAccountReference
    public interface IPayerReceiverPartyAccountReference
    {
        IReference PayerPartyReference { get; set; }
        IReference ReceiverPartyReference { get; set; }
        IReference PayerAccountReference { get; set; }
        IReference ReceiverAccountReference { get; set; }
    }
    #endregion IPayerReceiverPartyAccountReference

    #region ICommodityBase
    public interface ICommodityBase
    {
        bool BuyerPartyReferenceSpecified { set; get; }
        IReference BuyerPartyReference { set; get; }
        bool SellerPartyReferenceSpecified { set; get; }
        IReference SellerPartyReference { set; get; }

        IAdjustableOrRelativeDate EffectiveDate { set; get; }
        IAdjustableOrRelativeDate TerminationDate { set; get; }
        ICurrency SettlementCurrency { set; get; }
    }
    #endregion ICommodityBase
    #region ICommodityAsset
    public interface ICommodityAsset : IExchangeTraded
    {

    }
    #endregion ICommodityAsset
    #region ICommodityCalculationPeriodsSchedule
    public interface ICommodityCalculationPeriodsSchedule
    {
        EFS_Integer PeriodMultiplier { get; set; }
        PeriodEnum Period { get; set; }
        EFS_Boolean BalanceOfFirstPeriod { get; set; }
    }
    #endregion ICommodityCalculationPeriodsSchedule

    #region ICommodityDeliveryPeriods
    public interface ICommodityDeliveryPeriods
    {
        bool PeriodsSpecified { get; set; }
        IAdjustableDates Periods { get; set; }
        bool PeriodsScheduleSpecified { get; set; }
        ICommodityCalculationPeriodsSchedule PeriodsSchedule { get; set; }

        bool CalculationPeriodsReferenceSpecified { get; set; }
        IReference CalculationPeriodsReference { get; set; }

        bool CalculationPeriodsScheduleReferenceSpecified { get; set; }
        IReference CalculationPeriodsScheduleReference { get; set; }

        bool CalculationPeriodsDatesReferenceSpecified { get; set; }
        IReference CalculationPeriodsDatesReference { get; set; }
    }
    #endregion ICommodityDeliveryPeriods

    #region ICommodityHub
    public interface ICommodityHub
    {
        IReference PartyReference { get; set; }
        bool AccountReferenceSpecified { get; set; }
        IReference AccountReference { get; set; }
        IScheme HubCode { get; set; }
    }
    #endregion ICommodityHub

    #region ICommodityNotionalQuantity
    public interface ICommodityNotionalQuantity
    {
        IScheme QuantityUnit { get; set; }
        EFS_Decimal Quantity { get; set; }
        IScheme QuantityFrequency { get; set; }
    }
    #endregion ICommodityNotionalQuantity
    #region ICommodityNotionalQuantitySchedule
    public interface ICommodityNotionalQuantitySchedule
    {
    }
    #endregion ICommodityNotionalQuantitySchedule
    #region ICommodityPhysicalQuantitySchedule
    public interface ICommodityPhysicalQuantitySchedule
    {
    }
    #endregion ICommodityPhysicalQuantitySchedule
    #region ICommodityRelativePaymentDates
    public interface ICommodityRelativePaymentDates
    {
    }
    #endregion ICommodityRelativePaymentDates
    #region ICommoditySpot
    /// <summary>
    ///  Représente un commoditySpot
    /// </summary>
    /// FI 20170116 [21916] Modify
    /// EG 20221201 [25639] [WI482] Add
    public interface ICommoditySpot : ICommodityBase
    {
        int IdM { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] RptSide (R majuscule)
        IFixTrdCapRptSideGrp[] RptSide { set; get; }
        IFixedPriceSpotLeg FixedLeg { get; set; }
        IPhysicalLeg PhysicalLeg { get; set; }
        int CommodityAssetOTCmlId { get; }
        bool IsGas { get; }
        bool IsElectricity { get; }
        bool IsEnvironmental { get; }

        EFS_Asset Efs_Asset(string pCS);
        EFS_CommoditySpot Efs_CommoditySpot { set; get; }
    }
    #endregion ICommoditySpot
    #region ICommoditySwap
    public interface ICommoditySwap : ICommodityBase
    {
        IFixedPriceLeg FixedLeg { get; set; }
        IPhysicalLeg PhysicalLeg { get; set; }
    }
    #endregion ICommoditySwap

    #region IElectricityDelivery
    public interface IElectricityDelivery
    {
        bool DeliveryPointSpecified { get; set; }
        IScheme DeliveryPoint { get; set; }
        bool DeliveryZoneSpecified { get; set; }
        IScheme DeliveryZone { get; set; }
        bool DeliveryTypeSpecified { get; set; }
        IElectricityDeliveryType DeliveryType { get; set; }

        bool TransmissionContingencySpecified { get; set; }
        IElectricityTransmissionContingency TransmissionContingency { get; set; }

        bool InterconnectionPointSpecified { get; set; }
        IScheme InterconnectionPoint { get; set; }

        bool ElectingPartyReferenceSpecified { get; set; }
        IReference ElectingPartyReference { get; set; }
    }
    #endregion IElectricityDelivery
    #region IElectricityDeliveryFirm
    public interface IElectricityDeliveryFirm
    {
        EFS_Boolean ForceMajeure { get; set; }
    }
    #endregion IElectricityDeliveryFirm

    #region IElectricityDeliverySystemFirm
    public interface IElectricityDeliverySystemFirm
    {
        EFS_Boolean Applicable { get; set; }
        bool SystemSpecified { get; set; }
        IScheme System { get; set; }
    }
    #endregion IElectricityDeliverySystemFirm
    #region IElectricityDeliveryType
    public interface IElectricityDeliveryType
    {
        bool FirmSpecified { get; set; }
        IElectricityDeliveryFirm Firm { get; set; }
        bool NonFirmSpecified { get; set; }
        EFS_Boolean NonFirm { get; set; }
        bool SystemFirmSpecified { get; set; }
        IElectricityDeliverySystemFirm SystemFirm { get; set; }
        bool UnitFirmSpecified { get; set; }
        IElectricityDeliveryUnitFirm UnitFirm { get; set; }
    }
    #endregion IElectricityDeliveryType
    #region IElectricityDeliveryUnitFirm
    public interface IElectricityDeliveryUnitFirm
    {
        EFS_Boolean Applicable { get; set; }
        bool GenerationAssetSpecified { get; set; }
        IScheme GenerationAsset { get; set; }
    }
    #endregion IElectricityDeliveryUnitFirm
    #region IElectricityPhysicalLeg
    public interface IElectricityPhysicalLeg : IPhysicalLeg
    {
        ICommodityDeliveryPeriods DeliveryPeriods { get; set; }
        ISettlementPeriods[] SettlementPeriods { get; set; }
        bool SettlementPeriodsScheduleSpecified { get; set; }
        ISettlementPeriodsSchedule[] SettlementPeriodsSchedule { get; set; }
        bool LoadTypeSpecified { get; set; }
        LoadTypeEnum LoadType { get; set; }
        IElectricityProduct Electricity { get; set; }
        IElectricityDelivery DeliveryConditions { get; set; }
        IElectricityPhysicalQuantity DeliveryQuantity { get; set; }
    }
    #endregion IElectricityPhysicalLeg

    /// EG 20221201 [25639] [WI482] New
    public interface IEnvironmentalPhysicalLeg : IPhysicalLeg
    {

        IUnitQuantity NumberOfAllowances { get; set; }
        IEnvironmentalProduct Environmental { get; set; }
        bool AbandonmentOfSchemeSpecified { get; set; }
        EnvironmentalAbandonmentOfSchemeEnum AbandonmentOfScheme { get; set; }
        IAdjustableOrRelativeDate DeliveryDate { get; set; }
        IDateOffset PaymentDate { get; set; }
        bool BusinessCentersNoneSpecified { set; get; }
        object BusinessCentersNone { set; get; }
        bool BusinessCentersDefineSpecified { set; get; }
        IBusinessCenters BusinessCentersDefine { set; get; }
        bool BusinessCentersReferenceSpecified { set; get; }
        IReference BusinessCentersReference { set; get; }
        string BusinessCentersReferenceValue { get; }
        bool FailureToDeliverApplicableSpecified { get; set; }
        EFS_Boolean FailureToDeliverApplicable { set; get; }
        bool EEPParametersSpecified { set; get; }
        bool ProductionFeaturesSpecified { get; set; }
        IEnvironmentalProductionFeatures ProductionFeatures { get; set; }

    }

    public interface IEEPParameters
    {
        EFS_Boolean EEPApplicable { set; get; }
        IEERiskPeriod RiskPeriod { set; get; }
        EFS_Boolean EquivalentApplicable { set; get; }
        EFS_Boolean PenaltyApplicable { set; get; }
    }

    public interface IEERiskPeriod
    {
        EFS_Date StartDate { set; get; }
        EFS_Date EndDate { set; get; }
    }

    public interface IEnvironmentalProductionFeatures
    {
        bool TechnologySpecified { get; set; }
        CommodityTechnologyTypeEnum Technology { get; set; }
        bool RegionSpecified { get; set; }
        IScheme Region { get; set; }
        bool DeviceSpecified { get; set; }
        IScheme[] Device { get; set; }
    }

    #region IElectricityPhysicalQuantity
    public interface IElectricityPhysicalQuantity : IPhysicalQuantity
    {
        bool NoneSpecified { get; set; }
    }
    #endregion IElectricityPhysicalQuantity
    #region IElectricityProduct
    public interface IElectricityProduct
    {
        ElectricityProductTypeEnum TypeElectricity { get; set; }
        bool VoltageSpecified { get; set; }
        EFS_PositiveDecimal Voltage { get; set; }
    }
    #endregion IElectricityProduct
    #region IElectricityTransmissionContingency
    public interface IElectricityTransmissionContingency
    {
        IScheme Contingency { get; set; }
        IReference[] ContingentParty { get; set; }
    }
    #endregion IElectricityTransmissionContingency

    /// EG 20221201 [25639] [WI482] New
    public interface IEnvironmentalProduct
    {
        EnvironmentalProductTypeEnum ProductType{ get; set; }
        bool CompliancePeriodSpecified { get; set; }
        IEnvironmentalProductComplaincePeriod CompliancePeriod { get; set; }
        bool VintageSpecified { get; set; }
        EFS_Integer[] Vintage { get; set; }
        bool ApplicableLawSpecified { get; set; }
        IScheme ApplicableLaw { get; set; }
        bool TrackingSystemSpecified { get; set; }
        IScheme TrackingSystem { get; set; }
    }
    /// EG 20221201 [25639] [WI482] New
    public interface IEnvironmentalProductComplaincePeriod
    {
        EFS_String StartYear { get; set; }
        EFS_String EndYear { get; set; }
    }
    #region IFixedPrice
    public interface IFixedPrice
    {
        EFS_Decimal Price { get; set; }
        ICurrency Currency{ get; set; }
        IScheme  PriceUnit { get; set; } 
    }
    #endregion IFixedPrice
    #region IFixedPriceLegBase
    public interface IFixedPriceLegBase : IFinancialLeg
    {
        IFixedPrice FixedPrice { get; set; }
        bool NotionalQuantityScheduleSpecified { get; set; }
        ICommodityNotionalQuantitySchedule NotionalQuantitySchedule { get; set; }
        bool NotionalQuantitySpecified { get; set; }
        ICommodityNotionalQuantity NotionalQuantity { get; set; }
        bool QuantityReferenceSpecified { get; set; }
        IReference QuantityReference { get; set; }
        bool TotalNotionalQuantitySpecified { get; set; }
        EFS_Decimal TotalNotionalQuantity { get; set; }
    }
    #endregion IFixedPriceLeg
    #region IFixedPriceLeg
    public interface IFixedPriceLeg : IFixedPriceLegBase
    {
        bool RelativePaymentDatesSpecified { get; set; }
        bool PaymentDatesSpecified { get; set; }
        bool MasterAgreementPaymentDatesSpecified { get; set; }
        ICommodityRelativePaymentDates RelativePaymentDates { get; set; }
        IAdjustableDatesOrRelativeDateOffset PaymentDates { get; set; }
    }
    #endregion IFixedPriceLeg
    #region IFixedSpotPriceLeg
    /// <summary>
    /// 
    /// </summary>
    /// FI 20170116 [21916] Modify
    public interface IFixedPriceSpotLeg : IFixedPriceLegBase
    {
        IPayment GrossAmount { set; get; }
    }
    #endregion IFixedSpotPriceLeg
    #region IFinancialLeg
    // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
    public interface IFinancialLeg : IPayerReceiverPartyAccountReference
    {
        //IUnitQuantity totalQuantityCalculated { get; }
        IUnitQuantity GetTotalQuantityCalculated(DataDocumentContainer pDataDocument);
    }
    #endregion IFinancialLeg


    #region IGasDelivery
    public interface IGasDelivery
    {
        bool DeliveryPointSpecified { get; set; }
        IScheme DeliveryPoint { get; set; }
        bool EntryPointSpecified { get; set; }
        IScheme EntryPoint { get; set; }
        bool WithdrawalPointSpecified { get; set; }
        IScheme WithdrawalPoint { get; set; }
        CommodityDeliveryTypeEnum DeliveryType { get; set; }

        bool InterconnectionPointSpecified { get; set; }
        IScheme InterconnectionPoint { get; set; }

        bool BuyerHubSpecified { get; set; }
        ICommodityHub BuyerHub { get; set; }
        bool SellerHubSpecified { get; set; }
        ICommodityHub SellerHub { get; set; }
    }
    #endregion IGasDelivery
    #region IGasDeliveryPeriods
    public interface IGasDeliveryPeriods : ICommodityDeliveryPeriods
    {
        bool SupplyStartTimeSpecified { get; set; }
        IPrevailingTime SupplyStartTime { get; set; }
        bool SupplyEndTimeSpecified { get; set; }
        IPrevailingTime SupplyEndTime { get; set; }
    }
    #endregion IGasDeliveryPeriods

    #region IGasPhysicalLeg
    public interface IGasPhysicalLeg : IPhysicalLeg
    {
        IGasDeliveryPeriods DeliveryPeriods { get; set; }
        IGasDelivery DeliveryConditions { get; set; }
        IGasProduct Gas { get; set; }
        IGasPhysicalQuantity DeliveryQuantity { get; set; }
    }
    #endregion IGasPhysicalLeg
    #region IGasPhysicalQuantity
    public interface IGasPhysicalQuantity : IPhysicalQuantity
    {

        bool MinPhysicalQuantitySpecified { get; set; }
        ICommodityNotionalQuantity[] MinPhysicalQuantity { get; set; }
        bool MaxPhysicalQuantitySpecified { get; set; }
        ICommodityNotionalQuantity[] MaxPhysicalQuantity { get; set; }
        bool ElectingPartySpecified { get; set; }
        IReference ElectingParty { get; set; }
    }
    #endregion IGasPhysicalQuantity
    #region IGasProduct
    public interface IGasProduct
    {
        GasProductTypeEnum TypeGas { get; set; }
        bool NoneSpecified { get; set; }
        IEmpty None { get; set; }
        bool CalorificValueSpecified { get; set; }
        EFS_NonNegativeDecimal CalorificValue { get; set; }
        bool QualitySpecified { get; set; }
        IScheme Quality { get; set; }
    }
    #endregion IGasProduct

    #region IOffsetPrevailingTime
    public interface IOffsetPrevailingTime
    {
        IPrevailingTime Time { get; set; }
        bool OffsetSpecified { get; set; }
        IOffset Offset { get; set; }
    }
    #endregion IOffsetPrevailingTime


    #region IPhysicalLeg
    public interface IPhysicalLeg : IPayerReceiverPartyAccountReference
    {
        bool CommodityAssetSpecified { get; set; }
        ICommodityAsset CommodityAsset { get; set; }
        void SetAssetCommodityContract(SQL_AssetCommodityContract pSql_Asset);
    }
    #endregion IPhysicalLeg
    #region IPhysicalQuantity
    public interface IPhysicalQuantity
    {
        bool PhysicalQuantitySpecified { get; set; }
        ICommodityNotionalQuantity[] PhysicalQuantity { get; set; }
        bool PhysicalQuantityScheduleSpecified { get; set; }
        ICommodityPhysicalQuantitySchedule[] PhysicalQuantitySchedule { get; set; }
        bool TotalPhysicalQuantitySpecified { get; set; }
        IUnitQuantity TotalPhysicalQuantity { get; set; }
    }
    #endregion IPhysicalQuantity
    #region IPrevailingTime
    // EG 20171025 [23509] Upd Offset
    public interface IPrevailingTime
    {
        IHourMinuteTime HourMinuteTime { get; set; }
        ITimezoneLocation Location { get; set; }
        DateTimeOffset Offset(DateTime pDate);
    }
    #endregion IPrevailingTime

    #region ISettlementPeriods
    /// EG 20221201 [25639] [WI482] Upd Gestion des arrays de type Enums mal géré dans l'automate de saisie Full
    public interface ISettlementPeriods
    {
        SettlementPeriodDurationEnum Duration { get; set; }
        bool ApplicableDaySpecified { get; set; }
        DayOfWeekExtEnum[]  ApplicableDay { get; set; }
        EFS_StringArray[] Efs_applicableDay { get; set; }
        IOffsetPrevailingTime StartTime { get; set; }
        IOffsetPrevailingTime EndTime { get; set; }

        bool TimeDurationSpecified { get; set; }
        EFS_Time TimeDuration { get; set; }

        bool CalendarExcludeHolidaysSpecified { get; set; }
        IScheme CalendarExcludeHolidays { get; set; }
        bool CalendarIncludeHolidaysSpecified { get; set; }
        IScheme CalendarIncludeHolidays { get; set; }
    }
    #endregion ISettlementPeriods
    #region ISettlementPeriodsSchedule
    public interface ISettlementPeriodsSchedule
    {
        ISettlementPeriodsStep[] SettlementPeriodsStep { get; set; }
        bool PeriodsReferenceSpecified { get; set; }
        IReference PeriodsReference { get; set; }
        bool PeriodsScheduleReferenceSpecified { get; set; }
        IReference PeriodsScheduleReference { get; set; }
    }
    #endregion ISettlementPeriodsSchedule
    #region ISettlementPeriodsStep
    public interface ISettlementPeriodsStep
    {
        IReference[] SettlementPeriodsReference { get; set; }
    }
    #endregion ISettlementPeriodsStep

    #region ITimezoneLocation
    public interface ITimezoneLocation : IScheme
    {
        TimeZoneInfo TzInfo { get; }
    }
    #endregion ITimezoneLocation

    #region IUnitQuantity
    public interface IUnitQuantity
    {
        IScheme QuantityUnit { get; set; }
        EFS_NonNegativeDecimal Quantity { get; set; }
    }
    #endregion IUnitQuantity
}

#region using directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Interface;
using FixML.v50SP1;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Linq;
using System.Reflection;
using Tz = EFS.TimeZone;
#endregion using directives

namespace EfsML.v30.CommodityDerivative
{
    #region CommodityAsset
    public partial class CommodityAsset : ICommodityAsset
    {
    }
    #endregion CommodityAsset
    #region CommodityCalculationPeriodsSchedule
    /// EG 20161122 New Commodity Derivative
    public partial class CommodityCalculationPeriodsSchedule : ICommodityCalculationPeriodsSchedule
    {
        EFS_Integer ICommodityCalculationPeriodsSchedule.PeriodMultiplier
        {
            get { return this.periodMultiplier; }
            set { this.periodMultiplier = (EFS_Integer)value; }
        }

        PeriodEnum ICommodityCalculationPeriodsSchedule.Period
        {
            get { return this.period; }
            set { this.period = value; }
        }

        EFS_Boolean ICommodityCalculationPeriodsSchedule.BalanceOfFirstPeriod
        {
            get { return this.balanceOfFirstPeriod; }
            set { this.balanceOfFirstPeriod = (EFS_Boolean)value; }
        }
    }
    #endregion CommodityCalculationPeriodsSchedule

    #region CommodityDeliveryPeriods
    public partial class CommodityDeliveryPeriods : ICommodityDeliveryPeriods
    {
        #region Constructors
        public CommodityDeliveryPeriods()
        {
            deliveryPeriodsScheduleSpecified = true;
            deliveryPeriods = new AdjustableDates();
            deliveryPeriodsSchedule = new CommodityCalculationPeriodsSchedule();
            deliveryCalculationPeriodsReference = new CalculationPeriodsReference();
            deliveryCalculationPeriodsScheduleReference = new CalculationPeriodsScheduleReference();
            deliveryCalculationPeriodsDatesReference = new CalculationPeriodsDatesReference();
        }
        #endregion Constructors

        bool ICommodityDeliveryPeriods.PeriodsSpecified
        {
            get { return this.deliveryPeriodsSpecified; }
            set { this.deliveryPeriodsSpecified = value; }
        }

        IAdjustableDates ICommodityDeliveryPeriods.Periods
        {
            get { return this.deliveryPeriods; }
            set { this.deliveryPeriods = (AdjustableDates)value; }
        }

        bool ICommodityDeliveryPeriods.PeriodsScheduleSpecified
        {
            get { return this.deliveryPeriodsScheduleSpecified; }
            set { this.deliveryPeriodsScheduleSpecified = value; }
        }

        ICommodityCalculationPeriodsSchedule ICommodityDeliveryPeriods.PeriodsSchedule
        {
            get { return this.deliveryPeriodsSchedule; }
            set { this.deliveryPeriodsSchedule = (CommodityCalculationPeriodsSchedule)value; }
        }

        bool ICommodityDeliveryPeriods.CalculationPeriodsReferenceSpecified
        {
            get { return this.deliveryPeriodsSpecified; }
            set { this.deliveryPeriodsSpecified = value; }
        }

        // EG 20180423 Analyse du code Correction [CA1065]
        IReference ICommodityDeliveryPeriods.CalculationPeriodsReference
        {
            get
            {
                throw new InvalidOperationException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        bool ICommodityDeliveryPeriods.CalculationPeriodsScheduleReferenceSpecified
        {
            get { return this.deliveryPeriodsSpecified; }
            set { this.deliveryPeriodsSpecified = value; }
        }

        // EG 20180423 Analyse du code Correction [CA1065]
        IReference ICommodityDeliveryPeriods.CalculationPeriodsScheduleReference
        {
            get
            {
                throw new InvalidOperationException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        bool ICommodityDeliveryPeriods.CalculationPeriodsDatesReferenceSpecified
        {
            get { return this.deliveryPeriodsSpecified; }
            set { this.deliveryPeriodsSpecified = value; }
        }

        // EG 20180423 Analyse du code Correction [CA1065]
        IReference ICommodityDeliveryPeriods.CalculationPeriodsDatesReference
        {
            get
            {
                throw new InvalidOperationException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
    #endregion CommodityDeliveryPeriods
    #region CommodityHub
    /// EG 20161122 New Commodity Derivative
    public partial class CommodityHub : ICommodityHub
    {
        IReference ICommodityHub.PartyReference
        {
            get { return this.partyReference; }
            set { this.partyReference = (PartyReference)value; }
        }

        bool ICommodityHub.AccountReferenceSpecified
        {
            get { return this.accountReferenceSpecified; }
            set { this.accountReferenceSpecified = value; }
        }

        IReference ICommodityHub.AccountReference
        {
            get { return this.accountReference; }
            set { this.accountReference = (AccountReference)value; }
        }

        IScheme ICommodityHub.HubCode
        {
            get { return this.hubCode; }
            set { this.hubCode = (CommodityHubCode)value; }
        }
    }
    #endregion CommodityHub
    #region CommodityNotionalQuantity
    /// EG 20161122 New Commodity Derivative
    public partial class CommodityNotionalQuantity : ICommodityNotionalQuantity
    {
        IScheme ICommodityNotionalQuantity.QuantityUnit
        {
            get { return this.quantityUnit; }
            set { this.quantityUnit = (QuantityUnit)value; }
        }

        EFS_Decimal ICommodityNotionalQuantity.Quantity
        {
            get { return this.quantity; }
            set { this.quantity = (EFS_Decimal)value; }
        }

        IScheme ICommodityNotionalQuantity.QuantityFrequency
        {
            get { return this.quantityFrequency; }
            set { this.quantityFrequency = (CommodityQuantityFrequency)value; }
        }
    }
    #endregion CommodityNotionalQuantity
    #region CommodityNotionalQuantitySchedule
    public partial class CommodityNotionalQuantitySchedule : ICommodityNotionalQuantitySchedule
    {
        #region Constructors
        public CommodityNotionalQuantitySchedule()
        {
            nqNotionalStep = new CommodityNotionalQuantity();
            nqSettlementPeriodsNotionalQuantitySchedule = new CommoditySettlementPeriodsNotionalQuantitySchedule[1] { new CommoditySettlementPeriodsNotionalQuantitySchedule() };
        }
        #endregion Constructors
    }
    #endregion CommodityNotionalQuantitySchedule
    #region CommodityPhysicalQuantitySchedule
    /// EG 20161122 New Commodity Derivative
    public partial class CommodityPhysicalQuantitySchedule : ICommodityPhysicalQuantitySchedule
    {
    }
    #endregion CommodityPhysicalQuantitySchedule
    #region CommodityRelativePaymentDates
    /// EG 20161122 New Commodity Derivative
    public partial class CommodityRelativePaymentDates : ICommodityRelativePaymentDates
    {
        #region Constructors
        public CommodityRelativePaymentDates()
        {
            rtRelativeTo = new CommodityPayRelativeToEnum();
            rtRelativeToEvent = new CommodityPayRelativeToEvent();

            calculationPeriodsReference = new CalculationPeriodsReference();
            calculationPeriodsScheduleReference = new CalculationPeriodsScheduleReference();
            calculationPeriodsDatesReference = new CalculationPeriodsDatesReference();

            businessCentersNone = new Empty();
            businessCentersReference = new BusinessCentersReference();
            businessCentersDefine = new BusinessCenters();
        }
        #endregion Constructors
    }
    #endregion CommodityRelativePaymentDates
    #region CommoditySpot
    /// EG 20161122 New Commodity Derivative
    public partial class CommoditySpot : IProduct, ICommoditySpot
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_CommoditySpot efs_CommoditySpot;
        #endregion Members

        #region Accessors
        #region AdjustedClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedClearingBusinessDate
        {
            get
            {
                if (null != efs_CommoditySpot)
                    return efs_CommoditySpot.AdjustedClearingBusinessDate;
                else
                    return null;
            }
        }
        #endregion AdjustedClearingBusinessDate
        #region AdjustedEffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEffectiveDate
        {
            get
            {

                if (null != efs_CommoditySpot)
                    return efs_CommoditySpot.AdjustedEffectiveDate;
                else
                    return null;
            }
        }
        #endregion AdjustedEffectiveDate
        #region AdjustedTerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedTerminationDate
        {
            get
            {
                if (null != efs_CommoditySpot)
                    return efs_CommoditySpot.AdjustedTerminationDate;
                else
                    return null;
            }
        }
        #endregion AdjustedTerminationDate
        #region ClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ClearingBusinessDate
        {
            get
            {
                if (null != efs_CommoditySpot)
                    return efs_CommoditySpot.ClearingBusinessDate;
                else
                    return null;
            }
        }
        #endregion ClearingBusinessDate

        #region EffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EffectiveDate
        {
            get
            {
                if (null != efs_CommoditySpot)
                    return efs_CommoditySpot.EffectiveDate;
                else
                    return null;
            }
        }
        #endregion EffectiveDate
        #region TerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate TerminationDate
        {
            get
            {
                if (null != efs_CommoditySpot)
                    return efs_CommoditySpot.TerminationDate;
                else
                    return null;
            }
        }
        #endregion TerminationDate
        #region AssetCommodity
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Asset AssetCommodity
        {
            get { return efs_CommoditySpot.AssetCommodity; }
        }
        #endregion AssetCommodity
        #endregion Accessors
        #region Constructors
        public CommoditySpot()
        {
        }
        #endregion Constructors

        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members


        IFixedPriceSpotLeg ICommoditySpot.FixedLeg
        {
            get { return this.fixedLeg; }
            set { this.fixedLeg = (FixedPriceSpotLeg)value; }
        }

        IPhysicalLeg ICommoditySpot.PhysicalLeg
        {
            get { return this.commodityPhysicalLeg; }
            set { this.commodityPhysicalLeg = (PhysicalSwapLeg)value; }
        }
        // FI 20170116 [21916] RptSide (R majuscule)
        IFixTrdCapRptSideGrp[] ICommoditySpot.RptSide
        {
            set { this.RptSide = (TrdCapRptSideGrp_Block[])value; }
            get { return this.RptSide; }
        }

        bool ICommodityBase.BuyerPartyReferenceSpecified
        {
            set {; }
            get { return (null != this.fixedLeg.payerPartyReference) && StrFunc.IsFilled(this.fixedLeg.payerPartyReference.href); }
        }

        IReference ICommodityBase.BuyerPartyReference
        {
            set
            {
                this.fixedLeg.payerPartyReference = (PartyOrAccountReference)value;
                this.commodityPhysicalLeg.receiverPartyReference = (PartyOrAccountReference)value;
            }
            get { return this.fixedLeg.payerPartyReference; }
        }

        bool ICommodityBase.SellerPartyReferenceSpecified
        {
            set {; }
            get { return (null != this.fixedLeg.receiverPartyReference) && StrFunc.IsFilled(this.fixedLeg.receiverPartyReference.href); }
        }

        IReference ICommodityBase.SellerPartyReference
        {
            set
            {
                this.fixedLeg.receiverPartyReference = (PartyOrAccountReference)value;
                this.commodityPhysicalLeg.payerPartyReference = (PartyOrAccountReference)value;
            }
            get { return this.fixedLeg.receiverPartyReference; }
        }

        IAdjustableOrRelativeDate ICommodityBase.EffectiveDate
        {
            set { this.effectiveDate = (AdjustableOrRelativeDate)value; }
            get { return this.effectiveDate; }
        }
        IAdjustableOrRelativeDate ICommodityBase.TerminationDate
        {
            set { this.terminationDate = (AdjustableOrRelativeDate)value; }
            get { return this.terminationDate; }
        }

        ICurrency ICommodityBase.SettlementCurrency
        {
            set { this.settlementCurrency = (Currency)value; }
            get { return this.settlementCurrency; }
        }
        bool ICommoditySpot.IsGas
        {
            get { return this.commodityPhysicalLeg is GasPhysicalLeg; }
        }
        bool ICommoditySpot.IsElectricity
        {
            get { return this.commodityPhysicalLeg is ElectricityPhysicalLeg; }
        }
        /// EG 20221201 [25639] [WI482] Add
        bool ICommoditySpot.IsEnvironmental
        {
            get { return this.commodityPhysicalLeg is EnvironmentalPhysicalLeg; }
        }
        int ICommoditySpot.CommodityAssetOTCmlId
        {
            get
            {
                int OTCmlId = 0;
                if (commodityPhysicalLeg.commodityAssetSpecified)
                    OTCmlId = commodityPhysicalLeg.commodityAsset.OTCmlId;
                return OTCmlId;
            }

        }

        int ICommoditySpot.IdM
        {
            set { this.idM = value; }
            get { return this.idM; }
        }


        EFS_CommoditySpot ICommoditySpot.Efs_CommoditySpot
        {
            set { this.efs_CommoditySpot = value; }
            get { return this.efs_CommoditySpot; }
        }


        EFS_Asset ICommoditySpot.Efs_Asset(string pCS)
        {
            EFS_Asset _efs_Asset = null;
            int _id = ((ICommoditySpot)this).CommodityAssetOTCmlId;
            if (0 < _id)
            {
                //SQL_AssetCommodity sql_AssetCommodity = new SQL_AssetCommodity(CSTools.SetCacheOn(pCS), _id);
                //if (sql_AssetCommodity.IsLoaded)
                //{
                //    _efs_Asset = new EFS_Asset();
                //    _efs_Asset.idAsset = sql_AssetCommodity.Id;
                //    _efs_Asset.description = sql_AssetCommodity.Description;
                //    _efs_Asset.IdMarket = sql_AssetCommodity.IdM;
                //    _efs_Asset.IdMarketFIXML_SecurityExchange = sql_AssetCommodity.Market_FIXML_SecurityExchange;
                //    _efs_Asset.IdMarketIdentifier = sql_AssetCommodity.Market_Identifier;
                //    _efs_Asset.IdMarketISO10383_ALPHA4 = sql_AssetCommodity.Market_ISO10383_ALPHA4;
                //    _efs_Asset.assetCategory = sql_AssetCommodity.AssetCategory.Value;
                //}
            }
            return _efs_Asset;
        }
    }
    #endregion CommoditySpot
    #region CommoditySwap
    /// EG 20161122 New Commodity Derivative
    public partial class CommoditySwap : IProduct, ICommoditySwap
    {
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members

        // EG 20180423 Analyse du code Correction [CA1065]
        bool ICommodityBase.BuyerPartyReferenceSpecified
        {
            get
            {
                throw new InvalidOperationException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        // EG 20180423 Analyse du code Correction [CA1065]
        IReference ICommodityBase.BuyerPartyReference
        {
            get
            {
                throw new InvalidOperationException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        // EG 20180423 Analyse du code Correction [CA1065]
        bool ICommodityBase.SellerPartyReferenceSpecified
        {
            get
            {
                throw new InvalidOperationException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        // EG 20180423 Analyse du code Correction [CA1065]
        IReference ICommodityBase.SellerPartyReference
        {
            get
            {
                throw new InvalidOperationException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        IAdjustableOrRelativeDate ICommodityBase.EffectiveDate
        {
            set { this.effectiveDate = (AdjustableOrRelativeDate)value; }
            get { return this.effectiveDate; }
        }
        IAdjustableOrRelativeDate ICommodityBase.TerminationDate
        {
            set { this.terminationDate = (AdjustableOrRelativeDate)value; }
            get { return this.terminationDate; }
        }

        ICurrency ICommodityBase.SettlementCurrency
        {
            set { this.settlementCurrency = (Currency)value; }
            get { return this.settlementCurrency; }
        }

        // EG 20180423 Analyse du code Correction [CA1065]
        IFixedPriceLeg ICommoditySwap.FixedLeg
        {
            get
            {
                throw new InvalidOperationException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        // EG 20180423 Analyse du code Correction [CA1065]
        IPhysicalLeg ICommoditySwap.PhysicalLeg
        {
            get
            {
                throw new InvalidOperationException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
    #endregion CommoditySwap

    #region ElectricityDelivery
    /// EG 20161122 New Commodity Derivative
    public partial class ElectricityDelivery : IElectricityDelivery
    {
        #region Constructors
        public ElectricityDelivery()
        {
            edDeliveryPoint = new ElectricityDeliveryPoint();
            edDeliveryZone = new CommodityDeliveryPoint();
        }
        #endregion Constructors

        bool IElectricityDelivery.DeliveryPointSpecified
        {
            get { return this.edDeliveryPointSpecified; }
            set { this.edDeliveryPointSpecified = value; }
        }

        IScheme IElectricityDelivery.DeliveryPoint
        {
            get { return this.edDeliveryPoint; }
            set { this.edDeliveryPoint = (ElectricityDeliveryPoint)value; }
        }

        bool IElectricityDelivery.DeliveryZoneSpecified
        {
            get { return this.edDeliveryZoneSpecified; }
            set { this.edDeliveryZoneSpecified = value; }
        }

        IScheme IElectricityDelivery.DeliveryZone
        {
            get { return this.edDeliveryZone; }
            set { this.edDeliveryZone = (CommodityDeliveryPoint)value; }
        }

        bool IElectricityDelivery.DeliveryTypeSpecified
        {
            get { return this.deliveryTypeSpecified; }
            set { this.deliveryTypeSpecified = value; }
        }

        IElectricityDeliveryType IElectricityDelivery.DeliveryType
        {
            get { return this.deliveryType; }
            set { this.deliveryType = (ElectricityDeliveryType)value; }
        }

        bool IElectricityDelivery.TransmissionContingencySpecified
        {
            get { return this.transmissionContingencySpecified; }
            set { this.transmissionContingencySpecified = value; }
        }

        IElectricityTransmissionContingency IElectricityDelivery.TransmissionContingency
        {
            get { return this.transmissionContingency; }
            set { this.transmissionContingency = (ElectricityTransmissionContingency)value; }
        }

        bool IElectricityDelivery.InterconnectionPointSpecified
        {
            get { return this.interconnectionPointSpecified; }
            set { this.interconnectionPointSpecified = value; }
        }

        IScheme IElectricityDelivery.InterconnectionPoint
        {
            get { return this.interconnectionPoint; }
            set { this.interconnectionPoint = (InterconnectionPoint)value; }
        }

        bool IElectricityDelivery.ElectingPartyReferenceSpecified
        {
            get { return this.electingPartyReferenceSpecified; }
            set { this.electingPartyReferenceSpecified = value; }
        }

        IReference IElectricityDelivery.ElectingPartyReference
        {
            get { return this.electingPartyReference; }
            set { this.electingPartyReference = (PartyReference)value; }
        }
    }
    #endregion ElectricityDelivery
    #region ElectricityDeliveryFirm
    public partial class ElectricityDeliveryFirm : IElectricityDeliveryFirm
    {
        EFS_Boolean IElectricityDeliveryFirm.ForceMajeure
        {
            get { return this.forceMajeure; }
            set { this.forceMajeure = (EFS_Boolean)value; }
        }
    }
    #endregion ElectricityDeliveryFirm
    #region ElectricityDeliverySystemFirm
    /// EG 20161122 New Commodity Derivative
    public partial class ElectricityDeliverySystemFirm : IElectricityDeliverySystemFirm
    {
        EFS_Boolean IElectricityDeliverySystemFirm.Applicable
        {
            get { return this.applicable; }
            set { this.applicable = (EFS_Boolean)value; }
        }

        bool IElectricityDeliverySystemFirm.SystemSpecified
        {
            get { return this.systemSpecified; }
            set { this.systemSpecified = value; }
        }

        IScheme IElectricityDeliverySystemFirm.System
        {
            get { return this.system; }
            set { this.system = (CommodityDeliveryPoint)value; }
        }
    }
    #endregion ElectricityDeliverySystemFirm
    #region ElectricityDeliveryUnitFirm
    /// EG 20161122 New Commodity Derivative
    public partial class ElectricityDeliveryUnitFirm : IElectricityDeliveryUnitFirm
    {
        EFS_Boolean IElectricityDeliveryUnitFirm.Applicable
        {
            get { return this.applicable; }
            set { this.applicable = (EFS_Boolean)value; }
        }

        bool IElectricityDeliveryUnitFirm.GenerationAssetSpecified
        {
            get { return this.generationAssetSpecified; }
            set { this.generationAssetSpecified = value; }
        }

        IScheme IElectricityDeliveryUnitFirm.GenerationAsset
        {
            get { return this.generationAsset; }
            set { this.generationAsset = (CommodityDeliveryPoint)value; }
        }
    }
    #endregion ElectricityDeliveryUnitFirm
    #region ElectricityDeliveryType
    /// EG 20161122 New Commodity 
    public partial class ElectricityDeliveryType : ItemGUI, IElectricityDeliveryType
    {
        #region Constructors
        public ElectricityDeliveryType()
        {
            typeFirm = new ElectricityDeliveryFirm();
            typeNonFirm = new EFS_Boolean();
            typeSystemFirm = new ElectricityDeliverySystemFirm();
            typeUnitFirm = new ElectricityDeliveryUnitFirm();
        }
        #endregion Constructors

        bool IElectricityDeliveryType.FirmSpecified
        {
            get { return this.typeFirmSpecified; }
            set { this.typeFirmSpecified = value; }
        }

        IElectricityDeliveryFirm IElectricityDeliveryType.Firm
        {
            get { return this.typeFirm; }
            set { this.typeFirm = (ElectricityDeliveryFirm)value; }
        }

        bool IElectricityDeliveryType.NonFirmSpecified
        {
            get { return this.typeNonFirmSpecified; }
            set { this.typeNonFirmSpecified = value; }
        }

        EFS_Boolean IElectricityDeliveryType.NonFirm
        {
            get { return this.typeNonFirm; }
            set { this.typeNonFirm = (EFS_Boolean)value; }
        }

        bool IElectricityDeliveryType.SystemFirmSpecified
        {
            get { return this.typeSystemFirmSpecified; }
            set { this.typeSystemFirmSpecified = value; }
        }

        IElectricityDeliverySystemFirm IElectricityDeliveryType.SystemFirm
        {
            get { return this.typeSystemFirm; }
            set { this.typeSystemFirm = (ElectricityDeliverySystemFirm)value; }
        }

        bool IElectricityDeliveryType.UnitFirmSpecified
        {
            get { return this.typeUnitFirmSpecified; }
            set { this.typeUnitFirmSpecified = value; }
        }

        IElectricityDeliveryUnitFirm IElectricityDeliveryType.UnitFirm
        {
            get { return this.typeUnitFirm; }
            set { this.typeUnitFirm = (ElectricityDeliveryUnitFirm)value; }
        }
    }
    #endregion ElectricityDeliveryType
    #region ElectricityPhysicalDeliveryQuantitySchedule
    /// EG 20161122 New Commodity Derivative
    public partial class ElectricityPhysicalDeliveryQuantitySchedule : ICommodityPhysicalQuantitySchedule
    {
    }
    #endregion ElectricityPhysicalDeliveryQuantitySchedule
    #region ElectricityPhysicalLeg
    /// EG 20161122 New Commodity Derivative
    public partial class ElectricityPhysicalLeg : IElectricityPhysicalLeg
    {
        ICommodityDeliveryPeriods IElectricityPhysicalLeg.DeliveryPeriods
        {
            get { return this.deliveryPeriods; }
            set { this.deliveryPeriods = (CommodityDeliveryPeriods)value; }
        }

        ISettlementPeriods[] IElectricityPhysicalLeg.SettlementPeriods
        {
            get { return this.settlementPeriods; }
            set { this.settlementPeriods = (SettlementPeriods[])value; }
        }

        bool IElectricityPhysicalLeg.SettlementPeriodsScheduleSpecified
        {
            get { return this.settlementPeriodsScheduleSpecified; }
            set { this.settlementPeriodsScheduleSpecified = value; }
        }

        ISettlementPeriodsSchedule[] IElectricityPhysicalLeg.SettlementPeriodsSchedule
        {
            get { return this.settlementPeriodsSchedule; }
            set { this.settlementPeriodsSchedule = (SettlementPeriodsSchedule[])value; }
        }

        bool IElectricityPhysicalLeg.LoadTypeSpecified
        {
            get { return this.loadTypeSpecified; }
            set { this.loadTypeSpecified = value; }
        }

        LoadTypeEnum IElectricityPhysicalLeg.LoadType
        {
            get { return this.loadType; }
            set { this.loadType = value; }
        }

        IElectricityProduct IElectricityPhysicalLeg.Electricity
        {
            get { return this.electricity; }
            set { this.electricity = (ElectricityProduct)value; }
        }

        IElectricityDelivery IElectricityPhysicalLeg.DeliveryConditions
        {
            get { return this.deliveryConditions; }
            set { this.deliveryConditions = (ElectricityDelivery)value; }
        }

        IElectricityPhysicalQuantity IElectricityPhysicalLeg.DeliveryQuantity
        {
            get { return this.deliveryQuantity; }
            set { this.deliveryQuantity = (ElectricityPhysicalQuantity)value; }
        }

    }
    #endregion ElectricityPhysicalLeg
    #region ElectricityPhysicalQuantity
    /// EG 20161122 New Commodity Derivative
    public partial class ElectricityPhysicalQuantity : IElectricityPhysicalQuantity
    {
        #region Constructors
        public ElectricityPhysicalQuantity()
        {
            itemNone = new Empty();
            itemPhysicalQuantity = new ElectricityPhysicalDeliveryQuantity[1] { new ElectricityPhysicalDeliveryQuantity() };
            itemPhysicalQuantitySchedule = new ElectricityPhysicalDeliveryQuantitySchedule[1] { new ElectricityPhysicalDeliveryQuantitySchedule() };
        }
        #endregion Constructors

        bool IElectricityPhysicalQuantity.NoneSpecified
        {
            get { return this.itemNoneSpecified; }
            set { this.itemNoneSpecified = value; }
        }

        bool IPhysicalQuantity.PhysicalQuantitySpecified
        {
            get { return this.itemPhysicalQuantitySpecified; }
            set { this.itemPhysicalQuantitySpecified = value; }
        }

        ICommodityNotionalQuantity[] IPhysicalQuantity.PhysicalQuantity
        {
            get { return this.itemPhysicalQuantity; }
            set { this.itemPhysicalQuantity = (ElectricityPhysicalDeliveryQuantity[])value; }
        }

        bool IPhysicalQuantity.PhysicalQuantityScheduleSpecified
        {
            get { return this.itemPhysicalQuantityScheduleSpecified; }
            set { this.itemPhysicalQuantityScheduleSpecified = value; }
        }

        ICommodityPhysicalQuantitySchedule[] IPhysicalQuantity.PhysicalQuantitySchedule
        {
            get { return this.itemPhysicalQuantitySchedule; }
            set { this.itemPhysicalQuantitySchedule = (ElectricityPhysicalDeliveryQuantitySchedule[])value; }
        }

        bool IPhysicalQuantity.TotalPhysicalQuantitySpecified
        {
            get { return this.totalPhysicalQuantitySpecified; }
            set { this.totalPhysicalQuantitySpecified = value; }
        }

        IUnitQuantity IPhysicalQuantity.TotalPhysicalQuantity
        {
            get { return this.totalPhysicalQuantity; }
            set { this.totalPhysicalQuantity = (UnitQuantity)value; }
        }
    }
    #endregion ElectricityPhysicalQuantity
    #region ElectricityProduct
    /// EG 20161122 New Commodity Derivative
    public partial class ElectricityProduct : IElectricityProduct
    {
        ElectricityProductTypeEnum IElectricityProduct.TypeElectricity
        {
            get { return this.typeElectricity; }
            set { this.typeElectricity = value; }
        }

        bool IElectricityProduct.VoltageSpecified
        {
            get { return this.voltageSpecified; }
            set { this.voltageSpecified = value; }
        }

        EFS_PositiveDecimal IElectricityProduct.Voltage
        {
            get { return this.voltage; }
            set { this.voltage = (EFS_PositiveDecimal)value; }
        }
    }
    #endregion ElectricityProduct
    #region ElectricityTransmissionContingency
    /// EG 20161122 New Commodity Derivative
    public partial class ElectricityTransmissionContingency : IElectricityTransmissionContingency
    {
        IScheme IElectricityTransmissionContingency.Contingency
        {
            get { return this.contingency; }
            set { this.contingency = (ElectricityTransmissionContingencyType)value; }
        }

        IReference[] IElectricityTransmissionContingency.ContingentParty
        {
            get { return this.contingentParty; }
            set { this.contingentParty = (PartyReference[])value; }
        }
    }
    #endregion ElectricityTransmissionContingency


    /// EG 20221201 [25639] [WI482] New
    public partial class EnvironmentalProductComplaincePeriod : IEnvironmentalProductComplaincePeriod
    {
        EFS_String IEnvironmentalProductComplaincePeriod.StartYear 
        {
            get { return this.startYear; }
            set { this.startYear = value; }
        }
        EFS_String IEnvironmentalProductComplaincePeriod.EndYear 
        {
            get { return this.endYear; }
            set { this.endYear = value; }
        }
    }
    /// EG 20221201 [25639] [WI482] New
    public partial class EnvironmentalPhysicalLeg : IEnvironmentalPhysicalLeg
    {
        public EnvironmentalPhysicalLeg()
        {
            deliveryDate = new AdjustableOrRelativeDate();
            paymentDate = new DateOffset();
            businessCentersNoneSpecified = true;
            businessCentersNone = new Empty();
            businessCentersReference = new BusinessCentersReference();
            businessCentersDefine = new BusinessCenters();
            environmental = new EnvironmentalProduct();
            productionFeatures = new EnvironmentalProductionFeatures();
            productionFeaturesSpecified = true;
        }
        IUnitQuantity IEnvironmentalPhysicalLeg.NumberOfAllowances
        {
            get { return this.numberOfAllowances; }
            set { this.numberOfAllowances = (UnitQuantity)value; }
        }
        IEnvironmentalProduct IEnvironmentalPhysicalLeg.Environmental
        {
            get { return this.environmental; }
            set { this.environmental = (EnvironmentalProduct)value; }
        }
        bool IEnvironmentalPhysicalLeg.AbandonmentOfSchemeSpecified
        {
            get { return this.abandonmentOfSchemeSpecified; }
            set { this.abandonmentOfSchemeSpecified = value; }
        }
        EnvironmentalAbandonmentOfSchemeEnum IEnvironmentalPhysicalLeg.AbandonmentOfScheme
        {
            get { return this.abandonmentOfScheme; }
            set { this.abandonmentOfScheme = value; }
        }
        IAdjustableOrRelativeDate IEnvironmentalPhysicalLeg.DeliveryDate
        {
            set { this.deliveryDate = (AdjustableOrRelativeDate)value; }
            get { return this.deliveryDate; }
        }
        IDateOffset IEnvironmentalPhysicalLeg.PaymentDate
        {
            set { this.paymentDate = (DateOffset)value; }
            get { return this.paymentDate; }
        }
        bool IEnvironmentalPhysicalLeg.BusinessCentersNoneSpecified
        {
            set { this.businessCentersNoneSpecified = value; }
            get { return this.businessCentersNoneSpecified; }
        }
        object IEnvironmentalPhysicalLeg.BusinessCentersNone
        {
            set { this.businessCentersNone = (Empty)value; }
            get { return this.businessCentersNone; }
        }
        bool IEnvironmentalPhysicalLeg.BusinessCentersDefineSpecified
        {
            set { this.businessCentersDefineSpecified = value; }
            get { return this.businessCentersDefineSpecified; }
        }
        IBusinessCenters IEnvironmentalPhysicalLeg.BusinessCentersDefine
        {
            set { this.businessCentersDefine = (BusinessCenters)value; }
            get { return this.businessCentersDefine; }
        }
        bool IEnvironmentalPhysicalLeg.BusinessCentersReferenceSpecified
        {
            set { this.businessCentersReferenceSpecified = value; }
            get { return this.businessCentersReferenceSpecified; }
        }
        IReference IEnvironmentalPhysicalLeg.BusinessCentersReference
        {
            set { this.businessCentersReference = (BusinessCentersReference)value; }
            get { return this.businessCentersReference; }
        }
        string IEnvironmentalPhysicalLeg.BusinessCentersReferenceValue
        {
            get
            {
                if (this.businessCentersReferenceSpecified)
                    return this.businessCentersReference.href;
                return string.Empty;
            }
        }
        bool IEnvironmentalPhysicalLeg.FailureToDeliverApplicableSpecified
        {
            set { this.failureToDeliverApplicableSpecified = value; }
            get { return this.failureToDeliverApplicableSpecified; }
        }
        EFS_Boolean IEnvironmentalPhysicalLeg.FailureToDeliverApplicable
        {
            get { return this.failureToDeliverApplicable; }
            set { this.failureToDeliverApplicable = value; }
        }
        bool IEnvironmentalPhysicalLeg.EEPParametersSpecified
        {
            set { this.eEPParametersSpecified = value; }
            get { return this.eEPParametersSpecified; }
        }


        bool IEnvironmentalPhysicalLeg.ProductionFeaturesSpecified
        {
            get { return this.productionFeaturesSpecified; }
            set { this.productionFeaturesSpecified = value; }
        }

        IEnvironmentalProductionFeatures IEnvironmentalPhysicalLeg.ProductionFeatures
        {
            get { return this.productionFeatures; }
            set { this.productionFeatures = (EnvironmentalProductionFeatures)value; }
        }


    }

    /// EG 20221201 [25639] [WI482] Add Environmental
    public partial class EnvironmentalProduct : IEnvironmentalProduct
    {
        public EnvironmentalProduct()
        {
            compliancePeriod = new EnvironmentalProductComplaincePeriod();
        }
        EnvironmentalProductTypeEnum IEnvironmentalProduct.ProductType
        {
            get { return this.productType; }
            set { this.productType = value; }
        }
        bool IEnvironmentalProduct.CompliancePeriodSpecified
        {
            get { return this.compliancePeriodSpecified; }
            set { this.compliancePeriodSpecified = value; }
        }
        IEnvironmentalProductComplaincePeriod IEnvironmentalProduct.CompliancePeriod
        {
            get { return this.compliancePeriod; }
            set { this.compliancePeriod = (EnvironmentalProductComplaincePeriod)value; }
        }
        bool IEnvironmentalProduct.VintageSpecified
        {
            get { return this.vintageSpecified; }
            set { this.vintageSpecified = value; }
        }
        EFS_Integer[] IEnvironmentalProduct.Vintage
        {
            set { this.vintage = value; }
            get { return this.vintage; }
        }
        bool IEnvironmentalProduct.ApplicableLawSpecified
        {
            get { return this.applicableLawSpecified; }
            set { this.applicableLawSpecified = value; }
        }
        IScheme IEnvironmentalProduct.ApplicableLaw
        {
            get { return this.applicableLaw; }
            set { this.applicableLaw = (EnvironmentalProductApplicableLaw)value; }
        }
        bool IEnvironmentalProduct.TrackingSystemSpecified
        {
            get { return this.trackingSystemSpecified; }
            set { this.trackingSystemSpecified = value; }
        }
        IScheme IEnvironmentalProduct.TrackingSystem
        {
            get { return this.trackingSystem; }
            set { this.trackingSystem = (EnvironmentalTrackingSystem)value; }
        }

    }

    /// EG 20221201 [25639] [WI482] Add Environmental
    public partial class EEPParameters : IEEPParameters
    {
        EFS_Boolean IEEPParameters.EEPApplicable
        {
            get { return this.eEPApplicable; }
            set { this.eEPApplicable = value; }
        }
        IEERiskPeriod IEEPParameters.RiskPeriod
        {
            get { return this.riskPeriod; }
            set { this.riskPeriod = (EEPRiskPeriod)value; }
        }
        EFS_Boolean IEEPParameters.EquivalentApplicable
        {
            get { return this.equivalentApplicable; }
            set { this.equivalentApplicable = value; }
        }
        EFS_Boolean IEEPParameters.PenaltyApplicable
        {
            get { return this.penaltyApplicable; }
            set { this.penaltyApplicable = value; }
        }

    }

    /// EG 20221201 [25639] [WI484] New
    public partial class EEPRiskPeriod : IEERiskPeriod
    {
        EFS_Date IEERiskPeriod.StartDate
        {
            get { return this.startDate; }
            set { this.startDate = value; }
        }
        EFS_Date IEERiskPeriod.EndDate
        {
            get { return this.endDate; }
            set { this.endDate = value; }
        }

    }


    /// EG 20221201 [25639] [WI482] New
    public partial class EnvironmentalProductionRegion : IScheme
    {
        #region Constructors
        public EnvironmentalProductionRegion()
        {
            regionScheme = "http://www.efsml.org/coding-scheme/environmental-production-region";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.regionScheme = value; }
            get { return this.regionScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members

    }

    /// EG 20221201 [25639] [WI482] New
    public partial class EnvironmentalProductionDevice : IEFS_Array, ICloneable, IScheme
    {
        #region Constructors
        public EnvironmentalProductionDevice() : this(string.Empty) { }
        public EnvironmentalProductionDevice(string pValue)
        {
            deviceScheme = "http://www.efsml.org/coding-scheme/environmental-production-device";
            Value = pValue;
        }
        #endregion Constructors
        #region Methods
        #region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new Scheme(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _Value
        #endregion Methods

        string IScheme.Scheme
        {
            set { this.deviceScheme = value; }
            get { return this.deviceScheme; }
        }

        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }

        public object Clone()
        {
            EnvironmentalProductionDevice clone = new EnvironmentalProductionDevice
            {
                deviceScheme = this.deviceScheme,
                Value = this.Value
            };
            return clone;
        }
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }

    #region FinancialSwapLeg
    public abstract partial class FinancialSwapLeg : IFinancialLeg
    {
        IReference IPayerReceiverPartyAccountReference.PayerPartyReference
        {
            get { return this.payerPartyReference; }
            set { this.payerPartyReference = (PartyOrAccountReference)value; }
        }

        IReference IPayerReceiverPartyAccountReference.ReceiverPartyReference
        {
            get { return this.receiverPartyReference; }
            set { this.receiverPartyReference = (PartyOrAccountReference)value; }
        }

        IReference IPayerReceiverPartyAccountReference.PayerAccountReference
        {
            get { return this.payerAccountReference; }
            set { this.payerAccountReference = (AccountReference)value; }
        }

        IReference IPayerReceiverPartyAccountReference.ReceiverAccountReference
        {
            get { return this.receiverAccountReference; }
            set { this.receiverAccountReference = (AccountReference)value; }
        }

        //IUnitQuantity IFinancialLeg.totalQuantityCalculated
        //{
        //    get 
        //    {
        //        IUnitQuantity totalQuantity = null;
        //        if (this is FixedPriceSpotLeg)
        //        {
        //            IFixedPriceLegBase leg = this as IFixedPriceLegBase;
        //            if (leg.quantityReferenceSpecified)
        //            {
        //                object quantityReference = EFS_Current.GetObjectById(leg.quantityReference.hRef);
        //                if (null != quantityReference)
        //                {
        //                    if (Tools.IsTypeOrInterfaceOf(quantityReference, InterfaceEnum.IPhysicalQuantity))
        //                    {
        //                        IPhysicalQuantity physicalQuantity = quantityReference as IPhysicalQuantity;
        //                        if (physicalQuantity.totalPhysicalQuantitySpecified)
        //                        {
        //                            totalQuantity = physicalQuantity.totalPhysicalQuantity;
        //                        }
        //                    }
        //                }
        //            }
        //            else if (leg.totalNotionalQuantitySpecified)
        //            {
        //                // qty = leg.totalNotionalQuantity.DecValue;
        //                // unit !!!
        //            }
        //        }
        //        return totalQuantity;
        //    }
        //}

        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        /// EG 20221201 [25639] [WI482] Upd test for environmental quantity
        IUnitQuantity IFinancialLeg.GetTotalQuantityCalculated(DataDocumentContainer pDataDocument)
        {
            IUnitQuantity totalQuantity = null;
            if (this is FixedPriceSpotLeg)
            {
                IFixedPriceLegBase leg = this as IFixedPriceLegBase;
                if (leg.QuantityReferenceSpecified)
                {
                    object quantityReference = ReflectionTools.GetObjectById(pDataDocument.DataDocument.Item, leg.QuantityReference.HRef);
                    if (null != quantityReference)
                    {
                        if (quantityReference is IPhysicalQuantity) // Tools.IsTypeOrInterfaceOf(quantityReference, InterfaceEnum.IPhysicalQuantity))
                        {
                            IPhysicalQuantity physicalQuantity = quantityReference as IPhysicalQuantity;
                            if (physicalQuantity.TotalPhysicalQuantitySpecified)
                            {
                                totalQuantity = physicalQuantity.TotalPhysicalQuantity;
                            }
                        }
                        /// EG 20221201 [25639] [WI482] New
                        else if (quantityReference is IUnitQuantity)
                        {
                            totalQuantity = quantityReference as IUnitQuantity;
                        }
                    }
                }
                else if (leg.TotalNotionalQuantitySpecified)
                {
                    // qty = leg.totalNotionalQuantity.DecValue;
                    // unit !!!
                }
            }
            return totalQuantity;
        }
    }
    #endregion FinancialSwapLeg

    #region GasDelivery
    /// EG 20161122 New Commodity Derivative
    public partial class GasDelivery : IGasDelivery
    {
        #region Constructors
        public GasDelivery()
        {
            deliveryType = CommodityDeliveryTypeEnum.Firm;
            gdDeliveryPoint = new GasDeliveryPoint();
            gdEntryPoint = new CommodityDeliveryPoint();
            gdWithdrawalPoint = new CommodityDeliveryPoint();
        }
        #endregion Constructors

        bool IGasDelivery.DeliveryPointSpecified
        {
            get { return this.gdDeliveryPointSpecified; }
            set { this.gdDeliveryPointSpecified = value; }
        }

        IScheme IGasDelivery.DeliveryPoint
        {
            get { return this.gdDeliveryPoint; }
            set { this.gdDeliveryPoint = (GasDeliveryPoint)value; }
        }

        bool IGasDelivery.EntryPointSpecified
        {
            get { return this.gdEntryPointSpecified; }
            set { this.gdEntryPointSpecified = value; }
        }

        IScheme IGasDelivery.EntryPoint
        {
            get { return this.gdEntryPoint; }
            set { this.gdEntryPoint = (CommodityDeliveryPoint)value; }
        }

        bool IGasDelivery.WithdrawalPointSpecified
        {
            get { return this.gdWithdrawalPointSpecified; }
            set { this.gdWithdrawalPointSpecified = value; }
        }

        IScheme IGasDelivery.WithdrawalPoint
        {
            get { return this.gdWithdrawalPoint; }
            set { this.gdWithdrawalPoint = (CommodityDeliveryPoint)value; }
        }

        CommodityDeliveryTypeEnum IGasDelivery.DeliveryType
        {
            get { return this.deliveryType; }
            set { this.deliveryType = value; }
        }

        bool IGasDelivery.InterconnectionPointSpecified
        {
            get { return this.interconnectionPointSpecified; }
            set { this.interconnectionPointSpecified = value; }
        }

        IScheme IGasDelivery.InterconnectionPoint
        {
            get { return this.interconnectionPoint; }
            set { this.interconnectionPoint = (InterconnectionPoint)value; }
        }

        bool IGasDelivery.BuyerHubSpecified
        {
            get { return this.buyerHubSpecified; }
            set { this.buyerHubSpecified = value; }
        }

        ICommodityHub IGasDelivery.BuyerHub
        {
            get { return this.buyerHub; }
            set { this.buyerHub = (CommodityHub)value; }
        }

        bool IGasDelivery.SellerHubSpecified
        {
            get { return this.sellerHubSpecified; }
            set { this.sellerHubSpecified = value; }
        }

        ICommodityHub IGasDelivery.SellerHub
        {
            get { return this.sellerHub; }
            set { this.sellerHub = (CommodityHub)value; }
        }
    }
    #endregion GasDelivery
    #region GasDeliveryPeriods
    /// EG 20161122 New Commodity Derivative
    public partial class GasDeliveryPeriods : IGasDeliveryPeriods
    {
        bool IGasDeliveryPeriods.SupplyStartTimeSpecified
        {
            get { return this.supplyStartTimeSpecified; }
            set { this.supplyStartTimeSpecified = value; }
        }

        IPrevailingTime IGasDeliveryPeriods.SupplyStartTime
        {
            get { return this.supplyStartTime; }
            set { this.supplyStartTime = (PrevailingTime)value; }
        }

        bool IGasDeliveryPeriods.SupplyEndTimeSpecified
        {
            get { return this.supplyEndTimeSpecified; }
            set { this.supplyEndTimeSpecified = value; }
        }

        IPrevailingTime IGasDeliveryPeriods.SupplyEndTime
        {
            get { return this.supplyEndTime; }
            set { this.supplyEndTime = (PrevailingTime)value; }
        }
    }
    #endregion GasDeliveryPeriods
    #region GasDeliveryPoint
    /// EG 20161122 New Commodity Derivative
    public partial class GasDeliveryPoint : IScheme
    {
        #region Constructors
        public GasDeliveryPoint()
        {
            deliveryPointECCScheme = "http://www.ecc.de";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.deliveryPointECCScheme = value; }
            get { return this.deliveryPointECCScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion GasDeliveryPoint

    #region GasProduct
    /// EG 20161122 New Commodity Derivative
    public partial class GasProduct : IGasProduct
    {
        #region Constructors
        public GasProduct()
        {
            typeGas = GasProductTypeEnum.NaturalGas;
            complementNoneSpecified = true;
            complementNone = new Empty();
            complementQuality = new GasQuality();
            complementCalorificValue = new EFS_NonNegativeDecimal();
        }
        #endregion Constructors

        GasProductTypeEnum IGasProduct.TypeGas
        {
            get { return this.typeGas; }
            set { this.typeGas = value; }
        }
        bool IGasProduct.NoneSpecified
        {
            get { return this.complementNoneSpecified; }
            set { this.complementNoneSpecified = value; }
        }
        IEmpty IGasProduct.None
        {
            get { return this.complementNone; }
            set { this.complementNone = (Empty)value; }
        }

        bool IGasProduct.CalorificValueSpecified
        {
            get { return this.complementCalorificValueSpecified; }
            set { this.complementCalorificValueSpecified = value; }
        }

        EFS_NonNegativeDecimal IGasProduct.CalorificValue
        {
            get { return this.complementCalorificValue; }
            set { this.complementCalorificValue = (EFS_NonNegativeDecimal)value; }
        }

        bool IGasProduct.QualitySpecified
        {
            get { return this.complementQualitySpecified; }
            set { this.complementQualitySpecified = value; }
        }

        IScheme IGasProduct.Quality
        {
            get { return this.complementQuality; }
            set { this.complementQuality = (GasQuality)value; }
        }
    }
    #endregion GasProduct
    #region GasPhysicalLeg
    /// EG 20161122 New Commodity Derivative
    public partial class GasPhysicalLeg : IGasPhysicalLeg
    {
        IGasDeliveryPeriods IGasPhysicalLeg.DeliveryPeriods
        {
            get { return this.deliveryPeriods; }
            set { this.deliveryPeriods = (GasDeliveryPeriods)value; }
        }

        IGasDelivery IGasPhysicalLeg.DeliveryConditions
        {
            get { return this.deliveryConditions; }
            set { this.deliveryConditions = (GasDelivery)value; }
        }

        IGasProduct IGasPhysicalLeg.Gas
        {
            get { return this.gas; }
            set { this.gas = (GasProduct)value; }
        }

        IGasPhysicalQuantity IGasPhysicalLeg.DeliveryQuantity
        {
            get { return this.deliveryQuantity; }
            set { this.deliveryQuantity = (GasPhysicalQuantity)value; }
        }
    }
    #endregion GasPhysicalLeg
    #region GasPhysicalQuantity
    /// EG 20161122 New Commodity Derivative
    public partial class GasPhysicalQuantity : IGasPhysicalQuantity
    {
        #region Constructors
        public GasPhysicalQuantity()
        {
            itemPhysicalQuantitySpecified = true;
            itemPhysicalQuantity = new CommodityNotionalQuantity[1]{new CommodityNotionalQuantity() };
            itemPhysicalQuantitySchedule = new CommodityPhysicalQuantitySchedule[1]{new CommodityPhysicalQuantitySchedule()};
        }
        #endregion Constructors
        bool IPhysicalQuantity.PhysicalQuantitySpecified
        {
            get { return this.itemPhysicalQuantitySpecified; }
            set { this.itemPhysicalQuantitySpecified = value; }
        }

        ICommodityNotionalQuantity[] IPhysicalQuantity.PhysicalQuantity
        {
            get { return this.itemPhysicalQuantity; }
            set { this.itemPhysicalQuantity = (CommodityNotionalQuantity[])value; }
        }

        bool IPhysicalQuantity.PhysicalQuantityScheduleSpecified
        {
            get { return this.itemPhysicalQuantityScheduleSpecified; }
            set { this.itemPhysicalQuantityScheduleSpecified = value; }
        }

        ICommodityPhysicalQuantitySchedule[] IPhysicalQuantity.PhysicalQuantitySchedule
        {
            get { return this.itemPhysicalQuantitySchedule; }
            set { this.itemPhysicalQuantitySchedule = (CommodityPhysicalQuantitySchedule[])value; }
        }

        bool IPhysicalQuantity.TotalPhysicalQuantitySpecified
        {
            get { return this.totalPhysicalQuantitySpecified; }
            set { this.totalPhysicalQuantitySpecified = value; }
        }

        IUnitQuantity IPhysicalQuantity.TotalPhysicalQuantity
        {
            get { return this.totalPhysicalQuantity; }
            set { this.totalPhysicalQuantity = (UnitQuantity)value; }
        }

        bool IGasPhysicalQuantity.MinPhysicalQuantitySpecified
        {
            get { return this.minPhysicalQuantitySpecified; }
            set { this.minPhysicalQuantitySpecified = value; }
        }

        ICommodityNotionalQuantity[] IGasPhysicalQuantity.MinPhysicalQuantity
        {
            get { return this.minPhysicalQuantity; }
            set { this.minPhysicalQuantity = (CommodityNotionalQuantity[])value; }
        }

        bool IGasPhysicalQuantity.MaxPhysicalQuantitySpecified
        {
            get { return this.maxPhysicalQuantitySpecified; }
            set { this.maxPhysicalQuantitySpecified = value; }
        }

        ICommodityNotionalQuantity[] IGasPhysicalQuantity.MaxPhysicalQuantity
        {
            get { return this.maxPhysicalQuantity; }
            set { this.maxPhysicalQuantity = (CommodityNotionalQuantity[])value; }
        }

        bool IGasPhysicalQuantity.ElectingPartySpecified
        {
            get { return this.electingPartySpecified; }
            set { this.electingPartySpecified = value; }
        }

        IReference IGasPhysicalQuantity.ElectingParty
        {
            get { return this.electingParty; }
            set { this.electingParty = (PartyReference)value; }
        }
    }
    #endregion GasPhysicalQuantity

    /// EG 20221201 [25639] [WI484] New
    public partial class EnvironmentalProductionFeatures : IEnvironmentalProductionFeatures
    {
        #region Constructors
        public EnvironmentalProductionFeatures()
        {
            region = new EnvironmentalProductionRegion();
            device = new EnvironmentalProductionDevice[] { new EnvironmentalProductionDevice() };
        }
        #endregion Constructors

        bool IEnvironmentalProductionFeatures.TechnologySpecified
        {
            get { return this.technologySpecified; }
            set { this.technologySpecified = value; }
        }
        CommodityTechnologyTypeEnum IEnvironmentalProductionFeatures.Technology
        {
            get { return this.technology; }
            set { this.technology = value; }
        }
        bool IEnvironmentalProductionFeatures.RegionSpecified
        {
            get { return this.regionSpecified; }
            set { this.regionSpecified = value; }
        }
        IScheme IEnvironmentalProductionFeatures.Region
        {
            get { return this.region; }
            set { this.region = (EnvironmentalProductionRegion) value; }
        }

        bool IEnvironmentalProductionFeatures.DeviceSpecified
        {
            get { return this.deviceSpecified; }
            set { this.deviceSpecified = value; }
        }
        IScheme[] IEnvironmentalProductionFeatures.Device
        {
            get { return this.device; }
            set
            {
                this.device = new EnvironmentalProductionDevice[] { };
                if (ArrFunc.IsFilled(value))
                {
                    if (0 < value.OfType<SchemeData>().Count())
                        this.device = (from item in value select new EnvironmentalProductionDevice() { deviceScheme = item.Scheme, Value = item.Value }).ToArray();
                    else
                        this.device = value.Cast<EnvironmentalProductionDevice>().ToArray();
                }
                this.deviceSpecified = ArrFunc.IsFilled(this.device);
            }
        }
    }

    #region PhysicalSwapLeg
    public abstract partial class PhysicalSwapLeg : IPhysicalLeg
    {
        IReference IPayerReceiverPartyAccountReference.PayerPartyReference
        {
            get { return this.payerPartyReference; }
            set { this.payerPartyReference = (PartyOrAccountReference)value; }
        }

        IReference IPayerReceiverPartyAccountReference.ReceiverPartyReference
        {
            get { return this.receiverPartyReference; }
            set { this.receiverPartyReference = (PartyOrAccountReference)value; }
        }

        IReference IPayerReceiverPartyAccountReference.PayerAccountReference
        {
            get { return this.payerAccountReference; }
            set { this.payerAccountReference = (AccountReference)value; }
        }

        IReference IPayerReceiverPartyAccountReference.ReceiverAccountReference
        {
            get { return this.receiverAccountReference; }
            set { this.receiverAccountReference = (AccountReference)value; }
        }

        bool IPhysicalLeg.CommodityAssetSpecified
        {
            get { return this.commodityAssetSpecified; }
            set { this.commodityAssetSpecified = value; }
        }

        ICommodityAsset IPhysicalLeg.CommodityAsset
        {
            get { return this.commodityAsset; }
            set { this.commodityAsset = (CommodityAsset)value; }
        }
        void IPhysicalLeg.SetAssetCommodityContract(SQL_AssetCommodityContract pSql_Asset)
        {
            commodityAssetSpecified = true;
            commodityAsset = new CommodityAsset
            {
                OTCmlId = pSql_Asset.Id,

                instrumentId = new InstrumentId[] { new InstrumentId(), new InstrumentId() }
            };
            //Asset Identifier
            commodityAsset.instrumentId[0].Value = pSql_Asset.Identifier;
            commodityAsset.instrumentId[0].instrumentIdScheme = "Identifier";
            //Contract Identifier
            commodityAsset.instrumentId[1].Value = pSql_Asset.CommodityContract_Identifier;
            commodityAsset.instrumentId[1].instrumentIdScheme = "ContractIdentifier";
            //Asset Description
            commodityAsset.descriptionSpecified = StrFunc.IsFilled(pSql_Asset.Description);
            if (commodityAsset.descriptionSpecified)
                commodityAsset.description = new EFS_String(pSql_Asset.Description);
            //Asset currency
            commodityAsset.currencySpecified = StrFunc.IsFilled(pSql_Asset.IdC);
            if (commodityAsset.currencySpecified)
            {
                commodityAsset.currency = new Currency
                {
                    Value = pSql_Asset.IdC
                };
            }
            //Asset exchangeId
            commodityAsset.exchangeIdSpecified = (pSql_Asset.IdM > 0);
            if (commodityAsset.exchangeIdSpecified)
            {
                commodityAsset.exchangeId = new ExchangeId
                {
                    Value = pSql_Asset.Market_FIXML_SecurityExchange,
                    OTCmlId = pSql_Asset.IdM
                };
            }
            //Asset clearanceSystem
            commodityAsset.clearanceSystemSpecified = StrFunc.IsFilled(pSql_Asset.ClearanceSystem);
            if (commodityAsset.clearanceSystemSpecified)
            {
                commodityAsset.clearanceSystem = new ClearanceSystem
                {
                    Value = pSql_Asset.ClearanceSystem
                };
            }
        }
    }
    #endregion PhysicalSwapLeg

    #region FixedPrice
    /// EG 20161122 New Commodity Derivative
    public partial class FixedPrice : IFixedPrice
    {
        EFS_Decimal IFixedPrice.Price
        {
            get { return this.price; }
            set { this.price = value; }
        }
        ICurrency IFixedPrice.Currency
        {
            get { return this.priceCurrency; }
            set { this.priceCurrency = (Currency) value; }
        }

        IScheme IFixedPrice.PriceUnit
        {
            get { return this.priceUnit; }
            set { this.priceUnit = (QuantityUnit) value; }
        }
    }
    #endregion FixedPrice
    #region FixedPriceLeg
    /// EG 20161122 New Commodity Derivative
    public partial class FixedPriceLeg : IFixedPriceLeg
    {
        #region Constructors
        public FixedPriceLeg()
        {
            calculationDates = new AdjustableDates();
            calculationPeriods = new AdjustableDates();
            calculationPeriodsSchedule = new CommodityCalculationPeriodsSchedule();
            calculationPeriodsReference = new CalculationPeriodsReference();
            calculationPeriodsScheduleReference = new CalculationPeriodsScheduleReference();
            calculationPeriodsDatesReference = new CalculationPeriodsDatesReference();
            calculationPeriodsScheduleReferenceSpecified = true;

            nqNotionalQuantitySchedule = new CommodityNotionalQuantitySchedule();
            nqNotionalQuantity = new CommodityNotionalQuantity();
            nqSettlementPeriodsNotionalQuantity = new CommoditySettlementPeriodsNotionalQuantity[1] { new CommoditySettlementPeriodsNotionalQuantity() };
            nqQuantityReference = new QuantityReference();
            nqQuantityReferenceSpecified = true;

            paymentRelativePaymentDates = new CommodityRelativePaymentDates();
            paymentPaymentDates = new AdjustableDatesOrRelativeDateOffset();
            paymentMasterAgreementPaymentDates = new EFS_Boolean(false);
            paymentPaymentDatesSpecified = true;
        }
        #endregion Constructors
        IReference IPayerReceiverPartyAccountReference.PayerPartyReference
        {
            get {return this.payerPartyReference;}
            set { this.payerPartyReference = (PartyOrAccountReference)value; }
        }
        IReference IPayerReceiverPartyAccountReference.ReceiverPartyReference
        {
            get { return this.receiverPartyReference; }
            set { this.receiverPartyReference = (PartyOrAccountReference)value; }
        }
        IReference IPayerReceiverPartyAccountReference.PayerAccountReference
        {
            get { return this.payerAccountReference; }
            set { this.payerAccountReference = (AccountReference)value; }
        }
        IReference IPayerReceiverPartyAccountReference.ReceiverAccountReference
        {
            get { return this.receiverAccountReference; }
            set { this.receiverAccountReference = (AccountReference)value; }
        }

        IFixedPrice IFixedPriceLegBase.FixedPrice
        {
            get { return (this.priceFixedPriceSpecified? this.priceFixedPrice : null); }
            set { this.priceFixedPrice = (FixedPrice)value; }
        }


        bool IFixedPriceLegBase.TotalNotionalQuantitySpecified
        {
            get { return this.totalNotionalQuantitySpecified; }
            set { this.totalNotionalQuantitySpecified = value; } 
    }
        EFS_Decimal IFixedPriceLegBase.TotalNotionalQuantity
        {
            get { return (this.totalNotionalQuantity); }
            set { this.totalNotionalQuantity = (EFS_Decimal)value; }
        }


        bool IFixedPriceLegBase.NotionalQuantityScheduleSpecified
        {
            get { return (this.nqNotionalQuantityScheduleSpecified); }
            set { this.nqNotionalQuantityScheduleSpecified = value; }
        }
        ICommodityNotionalQuantitySchedule IFixedPriceLegBase.NotionalQuantitySchedule
        {
            get { return this.nqNotionalQuantitySchedule; }
            set { this.nqNotionalQuantitySchedule = (CommodityNotionalQuantitySchedule)value; }
        }

        bool IFixedPriceLegBase.NotionalQuantitySpecified
        {
            get { return (this.nqNotionalQuantitySpecified); }
            set { this.nqNotionalQuantitySpecified = value; }
        }
        ICommodityNotionalQuantity IFixedPriceLegBase.NotionalQuantity
        {
            get { return (this.nqNotionalQuantity); }
            set { this.nqNotionalQuantity= (CommodityNotionalQuantity)value; }
        }

        bool IFixedPriceLegBase.QuantityReferenceSpecified
        {
            get { return this.nqQuantityReferenceSpecified; }
            set { this.nqQuantityReferenceSpecified = value; }
        }
        IReference IFixedPriceLegBase.QuantityReference
        {
            get { return (this.nqQuantityReference); }
            set { this.nqQuantityReference = (QuantityReference)value; }
        }

        bool IFixedPriceLeg.RelativePaymentDatesSpecified
        {
            get { return this.paymentRelativePaymentDatesSpecified; }
            set { this.paymentRelativePaymentDatesSpecified = value; }
        }
        ICommodityRelativePaymentDates IFixedPriceLeg.RelativePaymentDates
        {
            get { return this.paymentRelativePaymentDates; }
            set { this.paymentRelativePaymentDates = (CommodityRelativePaymentDates)value; }
        }
        bool IFixedPriceLeg.PaymentDatesSpecified
        {
            get { return this.paymentPaymentDatesSpecified; }
            set { this.paymentPaymentDatesSpecified = value; }
        }
        IAdjustableDatesOrRelativeDateOffset IFixedPriceLeg.PaymentDates
        {
            get { return this.paymentPaymentDates; }
            set { this.paymentPaymentDates = (AdjustableDatesOrRelativeDateOffset)value; }
        }
        bool IFixedPriceLeg.MasterAgreementPaymentDatesSpecified
        {
            get { return this.paymentMasterAgreementPaymentDatesSpecified; }
            set { this.paymentMasterAgreementPaymentDatesSpecified = value; }
        }
    }
    #endregion FixedPriceLeg
    #region FixedPriceSpotLeg
    /// EG 20161122 New Commodity Derivative
    public partial class FixedPriceSpotLeg : IFixedPriceSpotLeg
    {
        #region Constructors
        public FixedPriceSpotLeg()
        {
            nqNotionalQuantitySchedule = new CommodityNotionalQuantitySchedule();
            nqNotionalQuantity = new CommodityNotionalQuantity();
            nqSettlementPeriodsNotionalQuantity = new CommoditySettlementPeriodsNotionalQuantity[1]{ new CommoditySettlementPeriodsNotionalQuantity()};
            nqQuantityReference = new QuantityReference();
            nqQuantityReferenceSpecified = true;

            grossAmount = new Payment();
        }
        #endregion Constructors

        //IReference IPayerReceiverPartyAccountReference.payerPartyReference
        //{
        //    get { return this.payerPartyReference; }
        //    set { this.payerPartyReference = (PartyOrAccountReference)value; }
        //}
        //IReference IPayerReceiverPartyAccountReference.receiverPartyReference
        //{
        //    get { return this.receiverPartyReference; }
        //    set { this.receiverPartyReference = (PartyOrAccountReference)value; }
        //}
        //IReference IPayerReceiverPartyAccountReference.payerAccountReference
        //{
        //    get { return this.payerAccountReference; }
        //    set { this.payerAccountReference = (AccountReference)value; }
        //}
        //IReference IPayerReceiverPartyAccountReference.receiverAccountReference
        //{
        //    get { return this.receiverAccountReference; }
        //    set { this.receiverAccountReference = (AccountReference)value; }
        //}

        IFixedPrice IFixedPriceLegBase.FixedPrice
        {
            get { return this.fixedPrice; }
            set { this.fixedPrice = (FixedPrice)value; }
        }

        IPayment IFixedPriceSpotLeg.GrossAmount
        {
            get { return this.grossAmount; }
            set { this.grossAmount = (Payment)value; }
        }


        bool IFixedPriceLegBase.NotionalQuantityScheduleSpecified
        {
            get { return (this.nqNotionalQuantityScheduleSpecified); }
            set { this.nqNotionalQuantityScheduleSpecified = value; }
        }
        ICommodityNotionalQuantitySchedule IFixedPriceLegBase.NotionalQuantitySchedule
        {
            get { return this.nqNotionalQuantitySchedule; }
            set { this.nqNotionalQuantitySchedule = (CommodityNotionalQuantitySchedule)value; }
        }

        bool IFixedPriceLegBase.NotionalQuantitySpecified
        {
            get { return (this.nqNotionalQuantitySpecified); }
            set { this.nqNotionalQuantitySpecified = value; }
        }
        ICommodityNotionalQuantity IFixedPriceLegBase.NotionalQuantity
        {
            get { return (this.nqNotionalQuantity); }
            set { this.nqNotionalQuantity = (CommodityNotionalQuantity)value; }
        }

        bool IFixedPriceLegBase.QuantityReferenceSpecified
        {
            get { return this.nqQuantityReferenceSpecified; }
            set { this.nqQuantityReferenceSpecified = value; }
        }
        IReference IFixedPriceLegBase.QuantityReference
        {
            get { return (this.nqQuantityReference); }
            set { this.nqQuantityReference = (QuantityReference)value; }
        }

        bool IFixedPriceLegBase.TotalNotionalQuantitySpecified
        {
            get { return this.totalNotionalQuantitySpecified; }
            set { this.totalNotionalQuantitySpecified = value; }
        }
        EFS_Decimal IFixedPriceLegBase.TotalNotionalQuantity
        {
            get { return this.totalNotionalQuantity; }
            set { this.totalNotionalQuantity = (EFS_Decimal)value; }
        }
    }
    #endregion FixedPriceSpotLeg

    #region OffsetPrevailingTime
    /// EG 20161122 New Commodity Derivative
    public partial class OffsetPrevailingTime : IOffsetPrevailingTime
    {
        IPrevailingTime IOffsetPrevailingTime.Time
        {
            get { return this.time; }
            set { this.time = (PrevailingTime)value; }
        }

        bool IOffsetPrevailingTime.OffsetSpecified
        {
            get { return this.offsetSpecified; }
            set { this.offsetSpecified = value; }
        }

        IOffset IOffsetPrevailingTime.Offset
        {
            get { return this.offset; }
            set { this.offset = (Offset)value; }
        }
    }
    #endregion OffsetPrevailingTime

    #region PrevailingTime
    /// EG 20161122 New Commodity Derivative
    public partial class PrevailingTime : IPrevailingTime
    {
        #region Constructors
        // EG 20170206 [22787] New
        public PrevailingTime()
        {
            hourMinuteTime = new HourMinuteTime();
            location = new TimezoneLocation();
        }
        #endregion Constructors
        IHourMinuteTime IPrevailingTime.HourMinuteTime
        {
            get { return this.hourMinuteTime; }
            set { this.hourMinuteTime = (HourMinuteTime)value; }
        }
        ITimezoneLocation IPrevailingTime.Location
        {
            get { return this.location; }
            set { this.location = (TimezoneLocation)value; }
        }
        // EG 20171025 [23509] Upd
        DateTimeOffset IPrevailingTime.Offset(DateTime pDate)
        {
            TimeZoneInfo timeZoneInfo = Tz.Tools.GetTimeZoneInfoFromTzdbId(location.Value);
            DateTime dt = pDate.Date.AddTicks(hourMinuteTime.TimeValue.TimeOfDay.Ticks);
            return new DateTimeOffset(dt, timeZoneInfo.GetUtcOffset(dt));
        }
    }
    #endregion PrevailingTime

    #region SettlementPeriods
    /// EG 20161122 New Commodity Derivative
    public partial class SettlementPeriods : IEFS_Array, ISettlementPeriods
    {
        #region Constructors
        public SettlementPeriods()
        {
            calendarNone = new Empty();
            calendarExcludeHolidays = new CommodityBusinessCalendar();
            calendarIncludeHolidays = new CommodityBusinessCalendar();
        }
        #endregion Constructors
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray

        public object INIT_applicableDay(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members

        SettlementPeriodDurationEnum ISettlementPeriods.Duration
        {
            get { return this.duration; }
            set { this.duration = value; }
        }

        bool ISettlementPeriods.ApplicableDaySpecified
        {
            get { return this.applicableDaySpecified; }
            set { this.applicableDaySpecified = value; }
        }

        /// EG 20221201 [25639] [WI484] New (gestion Array d'Enums)
        DayOfWeekExtEnum[] ISettlementPeriods.ApplicableDay
        {
            get { return this.ApplicableDay; }
            //set { this.applicableDay = value; }
            set
            {
                this.ApplicableDay = null;
                if (ArrFunc.IsFilled(value))
                {
                    this.ApplicableDay = value.Cast<DayOfWeekExtEnum>().ToArray();
                }
                this.applicableDaySpecified = ArrFunc.IsFilled(this.ApplicableDay);
            }

        }

        /// EG 20221201 [25639] [WI484] New (gestion Array d'Enums)
        EFS_StringArray[] ISettlementPeriods.Efs_applicableDay
        {
            get { return this.efs_applicableDay; }
            //set { this.applicableDay = value; }
            set
            {
                this.efs_applicableDay = value;
                this.ApplicableDay = null;
                if (ArrFunc.IsFilled(value))
                {
                    this.ApplicableDay = value.Cast<DayOfWeekExtEnum>().ToArray();
                }
                this.applicableDaySpecified = ArrFunc.IsFilled(this.ApplicableDay);
            }
        }
        IOffsetPrevailingTime ISettlementPeriods.StartTime
        {
            get { return this.startTime; }
            set { this.startTime = (OffsetPrevailingTime)value; }
        }

        IOffsetPrevailingTime ISettlementPeriods.EndTime
        {
            get { return this.endTime; }
            set { this.endTime = (OffsetPrevailingTime)value; }
        }

        bool ISettlementPeriods.TimeDurationSpecified
        {
            get { return this.timeDurationSpecified; }
            set { this.timeDurationSpecified = value; }
        }

        EFS_Time ISettlementPeriods.TimeDuration
        {
            get { return this.timeDuration; }
            set { this.timeDuration = (EFS_Time)value; }
        }

        bool ISettlementPeriods.CalendarExcludeHolidaysSpecified
        {
            get { return this.calendarExcludeHolidaysSpecified; }
            set { this.calendarExcludeHolidaysSpecified = value; }
        }

        IScheme ISettlementPeriods.CalendarExcludeHolidays
        {
            get { return this.calendarExcludeHolidays; }
            set { this.calendarExcludeHolidays = (CommodityBusinessCalendar)value; }
        }

        bool ISettlementPeriods.CalendarIncludeHolidaysSpecified
        {
            get { return this.calendarIncludeHolidaysSpecified; }
            set { this.calendarIncludeHolidaysSpecified = value; }
        }

        IScheme ISettlementPeriods.CalendarIncludeHolidays
        {
            get { return this.calendarIncludeHolidays; }
            set { this.calendarIncludeHolidays = (CommodityBusinessCalendar)value; }
        }
    }
    #endregion SettlementPeriods

    #region SettlementPeriodsReference
    /// EG 20161122 New Commodity Derivative
    public partial class SettlementPeriodsReference : IEFS_Array, ICloneable, IReference
    {
        #region Constructor
        public SettlementPeriodsReference() { }
        public SettlementPeriodsReference(string pReference)
        {
            this.href = pReference;
        }
        #endregion Constructor
        #region IClonable Members
        #region Clone
        public object Clone()
        {
            SettlementPeriodsReference clone = new SettlementPeriodsReference
            {
                href = this.href
            };
            return clone;
        }
        #endregion Clone
        #endregion IClonable Members
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
        #endregion IReference Members
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }
    #endregion SettlementPeriodsReference
    #region ArraySettlementPeriodsReference
    public partial class ArraySettlementPeriodsReference : IEFS_Array, IReference
    {
        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "href", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
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
        #region Constructor
        public ArraySettlementPeriodsReference() { }
        public ArraySettlementPeriodsReference(string pReference)
        {
            this.Href = pReference;
        }
        #endregion Constructor
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
        #region IReference Members
        string IReference.HRef
        {
            set { this.Href = value; }
            get { return this.Href; }
        }
        #endregion IReference Members
    }
    #endregion ArraySettlementPeriodsReference
    #region SettlementPeriodsSchedule
    /// EG 20161122 New Commodity Derivative
    public partial class SettlementPeriodsSchedule : IEFS_Array, ISettlementPeriodsSchedule
    {
        #region Constructors
        public SettlementPeriodsSchedule()
        {
            dcPeriodsReference = new CalculationPeriodsReference();
            dcPeriodsScheduleReference = new CalculationPeriodsScheduleReference();
        }
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

        ISettlementPeriodsStep[] ISettlementPeriodsSchedule.SettlementPeriodsStep
        {
            get { return this.settlementPeriodsStep; }
            set { this.settlementPeriodsStep = (SettlementPeriodsStep[])value; }
        }

        bool ISettlementPeriodsSchedule.PeriodsReferenceSpecified
        {
            get { return this.dcPeriodsReferenceSpecified; }
            set { this.dcPeriodsReferenceSpecified = value; }
        }

        IReference ISettlementPeriodsSchedule.PeriodsReference
        {
            get { return this.dcPeriodsReference; }
            set { this.dcPeriodsReference = (CalculationPeriodsReference)value; }
        }

        bool ISettlementPeriodsSchedule.PeriodsScheduleReferenceSpecified
        {
            get { return this.dcPeriodsScheduleReferenceSpecified; }
            set { this.dcPeriodsScheduleReferenceSpecified = value; }
        }

        IReference ISettlementPeriodsSchedule.PeriodsScheduleReference
        {
            get { return this.dcPeriodsScheduleReference; }
            set { this.dcPeriodsScheduleReference = (CalculationPeriodsScheduleReference)value; }
        }
    }
    #endregion SettlementPeriodsSchedule
    #region SettlementPeriodsStep
    /// EG 20161122 New Commodity Derivative
    public partial class SettlementPeriodsStep : IEFS_Array, ISettlementPeriodsStep
    {
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

        IReference[] ISettlementPeriodsStep.SettlementPeriodsReference
        {
            get { return this.settlementPeriodsReference; }
            set { this.settlementPeriodsReference = (ArraySettlementPeriodsReference[])value; }
        }
    }
    #endregion SettlementPeriodsStep

    #region TimezoneLocation
    /// EG 20161122 New Commodity Derivative
    public partial class TimezoneLocation : ITimezoneLocation
    {
        #region Members
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public Dictionary<string, string> tz;
        #endregion Members

        #region Constructors
        public TimezoneLocation()
        {
            timezoneLocationScheme = "http://www.iana.org/time-zones";
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.timezoneLocationScheme = value; }
            get { return this.timezoneLocationScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members

        #region Members
        // EG 20170918 [23342] Upd
        TimeZoneInfo ITimezoneLocation.TzInfo
        {
            get
            {
                return Tz.Tools.GetTimeZoneInfoFromTzdbId(this.Value);
            }
        }
        #endregion Members
    }
    #endregion TimeZoneLocation
    #region UnitQuantity
    /// EG 20161122 New Commodity Derivative
    public partial class UnitQuantity : IUnitQuantity
    {
        #region Constructors
        public UnitQuantity()
        {}
        public UnitQuantity(decimal pQuantity, string pUnit)
        {
            this.quantity = new EFS_NonNegativeDecimal(pQuantity);
            this.quantityUnit = new QuantityUnit
            {
                Value = pUnit
            };
        }
        #endregion Constructors
        IScheme IUnitQuantity.QuantityUnit
        {
            get { return this.quantityUnit; }
            set { this.quantityUnit = (QuantityUnit)value; }
        }

        EFS_NonNegativeDecimal IUnitQuantity.Quantity
        {
            get { return this.quantity; }
            set { this.quantity = (EFS_NonNegativeDecimal)value; }
        }
    }
    #endregion UnitQuantity

}
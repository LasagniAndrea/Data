#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_CommodityBase
    public abstract class EFS_CommodityBase
    {
        #region Members
        protected string m_Cs;
        protected DataDocumentContainer m_DataDocument;
        protected ITradeDate tradeDate;
        public DateTime clearingBusinessDate;
        protected ICommodityBase commodityBase;

        public IReference buyerPartyReference;
        public IReference sellerPartyReference;
        public EFS_AdjustableDate effectiveDate;
        public EFS_AdjustableDate terminationDate;
        #endregion Members

        #region Accessors
        #region AdjustedClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedClearingBusinessDate
        {
            get
            {
                EFS_Date adjustedDate = new EFS_Date
                {
                    DateValue = clearingBusinessDate.Date
                };
                return adjustedDate;
            }
        }
        #endregion AdjustedClearingBusinessDate
        #region AdjustedEffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEffectiveDate
        {
            get
            {

                if (null != effectiveDate)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = effectiveDate.adjustedDate.DateValue
                    };
                    return dt;
                }
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

                if (null != terminationDate)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = terminationDate.adjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;

            }
        }
        #endregion AdjustedTerminationDate

        #region ClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ClearingBusinessDate
        {
            get { return new EFS_EventDate(clearingBusinessDate.Date, clearingBusinessDate.Date); }
        }
        #endregion ClearingBusinessDate

        #region EffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EffectiveDate
        {
            get
            {
                return new EFS_EventDate(effectiveDate);

            }
        }
        #endregion EffectiveDate
        #region TerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate TerminationDate
        {
            get
            {

                return new EFS_EventDate(terminationDate);

            }
        }
        #endregion TerminationDate
        #region BuyerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string BuyerPartyReference
        {
            get
            {
                return buyerPartyReference.HRef;

            }
        }
        #endregion BuyerPartyReference
        #region SellerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SellerPartyReference
        {
            get
            {
                return sellerPartyReference.HRef;

            }
        }
        #endregion SellerPartyReference
        #endregion Accessors

        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CommodityBase(string pConnectionString, ITradeDate pTradeDate, ICommodityBase pCommodityBase, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            commodityBase = pCommodityBase;

            tradeDate = pTradeDate;
            buyerPartyReference = pCommodityBase.BuyerPartyReference;
            sellerPartyReference = pCommodityBase.SellerPartyReference;

            #region EffectiveDate and Termination calculation
            effectiveDate = Tools.GetEFS_AdjustableDate(m_Cs, commodityBase.EffectiveDate, m_DataDocument);
            terminationDate = Tools.GetEFS_AdjustableDate(m_Cs, commodityBase.TerminationDate, m_DataDocument);
            #endregion EffectiveDate and Termination calculation
        }
        #endregion Constructors
    }
    #endregion EFS_CommodityBase

    #region EFS_CommodityDeliveryDate
    // EG 20171025 [23509] Upd DateTime replace DateTimeOffset , Add timezone
    public class EFS_CommodityDeliveryDate
    {
        #region Members
        protected string cs;
        public DateTime startDate;
        public DateTime endDate;
        public string timezone;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        public EFS_CommodityDeliveryDate()
        {
        }
        #endregion Constructors
    }
    #endregion EFS_CommodityDelivery
    #region EFS_CommodityPayment
    public class EFS_CommodityPayment
    {
        #region Members
        
        private readonly EFS_Payment grossAmount;
        private readonly EFS_FixedPrice fixedPrice;
        private readonly DateTime valuedPaymentDate;
        #endregion Members

        #region Accessors
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return grossAmount.payerPartyReference.HRef;

            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return grossAmount.receiverPartyReference.HRef;

            }
        }
        #endregion ReceiverPartyReference
        #region Amount
        public EFS_Decimal Amount
        {
            get
            {
                return grossAmount.paymentAmount.Amount;
            }
        }
        #endregion Amount
        #region Currency
        /// <summary>
        /// Obtient la devise
        /// </summary>
        public string Currency
        {
            get
            {
                return grossAmount.paymentAmount.Currency;
            }
        }
        #endregion Currency
        #region PaymentDate
        /// <summary>
        /// Obtient les dates (ajustée et non ajustée)
        /// </summary>
        public EFS_EventDate PaymentDate
        {
            get
            {

                return new EFS_EventDate(grossAmount.paymentDateAdjustment);
            }
        }
        #endregion PaymentDate
        #region UnadjustedPaymentDate
        /// <summary>
        /// Obtient la date non ajustée
        /// </summary>
        public EFS_Date UnadjustedPaymentDate
        {
            get
            {

                EFS_Date ret = new EFS_Date
                {
                    DateValue = grossAmount.paymentDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return ret;
            }
        }
        #endregion UnadjustedPaymentDate
        #region AdjustedPaymentDate
        /// <summary>
        /// Obtient la date ajustée
        /// </summary>
        public EFS_Date AdjustedPaymentDate
        {
            get
            {

                EFS_Date ret = new EFS_Date
                {
                    DateValue = grossAmount.paymentDateAdjustment.adjustedDate.DateValue
                };
                return ret;

            }
        }
        #endregion AdjustedPaymentDate
        #region ValuedPaymentDate
        public EFS_Date ValuedPaymentDate
        {
            get
            {

                EFS_Date ret = new EFS_Date
                {
                    DateValue = valuedPaymentDate
                };
                return ret;
            }
        }
        #endregion ValuedPaymentDate

        #region FixedPrice
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FixedPrice FixedPrice
        {
            get { return fixedPrice; }
        }
        #endregion FixedPrice

        #endregion Accessors

        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CommodityPayment(string pCs, ICommoditySpot pCommoditySpot, EFS_CommodityQuantity pQuantity, DataDocumentContainer pDataDocument)
        {
            IFixedPriceSpotLeg fixedLeg = pCommoditySpot.FixedLeg;
            grossAmount = new EFS_Payment(pCs,null, fixedLeg.GrossAmount, pDataDocument);
            fixedPrice = new EFS_FixedPrice(pCs, (IProductBase)pCommoditySpot,  pCommoditySpot.FixedLeg, pCommoditySpot.SettlementCurrency, 
                grossAmount.paymentDateAdjustment, pQuantity.totalQuantity, pDataDocument);

            IOffset offset = pDataDocument.CurrentProduct.ProductBase.CreateOffset(PeriodEnum.D, -1, DayTypeEnum.Business);
            EFS_Offset efsOffset = new EFS_Offset(pCs, offset, grossAmount.paymentDateAdjustment.adjustedDate.DateValue, grossAmount.paymentDateAdjustment.AdjustableDate.DateAdjustments, pDataDocument);

            if (DtFunc.IsDateTimeFilled(efsOffset.offsetDate[0]))
                valuedPaymentDate = efsOffset.offsetDate[0];
        }
        #endregion Constructors
    }
    #endregion EFS_CommodityPayment
    #region EFS_CommoditySwapPayment
    public class EFS_CommoditySwapPayment
    {
        #region Members
        private readonly EFS_AdjustableDate paymentDateAdjustment;
        private readonly ISimplePayment payment;
        private readonly EFS_FixedPrice fixedPrice;
        //private EFS_Decimal totalQuantity;
        #endregion Members

        #region Accessors
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return payment.PayerPartyReference.HRef;

            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return payment.ReceiverPartyReference.HRef;

            }
        }
        #endregion ReceiverPartyReference
        #region Amount
        public EFS_Decimal Amount
        {
            get
            {
                return payment.PaymentAmount.Amount;
            }
        }
        #endregion Amount
        #region Currency
        /// <summary>
        /// Obtient la devise
        /// </summary>
        public string Currency
        {
            get
            {
                return payment.PaymentAmount.Currency;
            }
        }
        #endregion Currency
        #region PaymentDate
        /// <summary>
        /// Obtient les dates (ajustée et non ajustée)
        /// </summary>
        public EFS_EventDate PaymentDate
        {
            get
            {

                return new EFS_EventDate(paymentDateAdjustment);
            }
        }
        #endregion PaymentDate
        #region UnadjustedPaymentDate
        /// <summary>
        /// Obtient la date non ajustée
        /// </summary>
        public EFS_Date UnadjustedPaymentDate
        {
            get
            {

                EFS_Date ret = new EFS_Date
                {
                    DateValue = paymentDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return ret;
            }
        }
        #endregion UnadjustedPaymentDate
        #region AdjustedPaymentDate
        /// <summary>
        /// Obtient la date ajustée
        /// </summary>
        public EFS_Date AdjustedPaymentDate
        {
            get
            {

                EFS_Date ret = new EFS_Date
                {
                    DateValue = paymentDateAdjustment.adjustedDate.DateValue
                };
                return ret;

            }
        }
        #endregion AdjustedPaymentDate

        #region FixedPrice
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FixedPrice FixedPrice
        {
            get { return fixedPrice; }
        }
        #endregion FixedPrice

        #endregion Accessors

        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CommoditySwapPayment(string pCs, ICommoditySwap pCommoditySwap, EFS_AdjustableDate pPaymentDate, EFS_CommodityQuantity pQuantity, DataDocumentContainer pDataDocument)
        {
            paymentDateAdjustment = pPaymentDate;
            IFixedPriceLeg fixedLeg = pCommoditySwap.FixedLeg;

            IProductBase productBase = (IProductBase)pCommoditySwap;
            payment = productBase.CreateSimplePayment();
            payment.PayerPartyReference.HRef = fixedLeg.PayerPartyReference.HRef;
            payment.ReceiverPartyReference.HRef = fixedLeg.ReceiverPartyReference.HRef;
            payment.PaymentAmount.Currency = pCommoditySwap.SettlementCurrency.Value;
            payment.PaymentDate.AdjustableDateSpecified = true;
            payment.PaymentDate.AdjustableDate = pPaymentDate.AdjustableDate;

            fixedPrice = new EFS_FixedPrice(pCs, (IProductBase) pCommoditySwap, pCommoditySwap.FixedLeg, pCommoditySwap.SettlementCurrency, pPaymentDate, 
                pQuantity.totalQuantity, pDataDocument);
            if (null != fixedPrice.settlementPrice)
                payment.PaymentAmount.Amount.DecValue = fixedPrice.settlementPrice.DecValue * pQuantity.totalQuantity.Quantity.DecValue;
        }
        #endregion Constructors
    }
    #endregion EFS_CommoditySwapPayment

    #region EFS_CommodityQuantity
    public class EFS_CommodityQuantity
    {
        #region Members
        public IUnitQuantity totalQuantity;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CommodityQuantity(IProductBase pProductBase, IFixedPriceLegBase pFixedLeg, IPhysicalLeg pPhysicalLeg, DataDocumentContainer pDataDocument)
        {
            Calc(pProductBase, pFixedLeg, pPhysicalLeg, pDataDocument);
        }
        #endregion Constructors
        #region Methods
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20221201 [25639] [WI484] Add Environmental Test
        public void Calc<T1, T2>(IProductBase pProductBase, T1 pLeg1, T2 pLeg2, DataDocumentContainer pDataDocument)
        {
            if (pLeg1 is IFixedPriceLegBase)
            {
                IFixedPriceLegBase fixedLeg = pLeg1 as IFixedPriceLegBase;
                totalQuantity = fixedLeg.GetTotalQuantityCalculated(pDataDocument);
            }
            else if (pLeg1 is IGasPhysicalLeg)
            {
                IGasPhysicalLeg physicalLeg = pLeg1 as IGasPhysicalLeg;
                if (physicalLeg.DeliveryQuantity.TotalPhysicalQuantitySpecified)
                {
                    totalQuantity = physicalLeg.DeliveryQuantity.TotalPhysicalQuantity;
                }
                else if (physicalLeg.DeliveryQuantity.PhysicalQuantitySpecified)
                {
                    totalQuantity = TotalQuantityCalculation(pProductBase, physicalLeg.DeliveryQuantity.PhysicalQuantity);
                }
                else if (physicalLeg.DeliveryQuantity.PhysicalQuantityScheduleSpecified)
                {
                }

            }
            else if (pLeg1 is IElectricityPhysicalLeg)
            {
                IElectricityPhysicalLeg physicalLeg = pLeg1 as IElectricityPhysicalLeg;
                if (physicalLeg.DeliveryQuantity.TotalPhysicalQuantitySpecified)
                {
                    totalQuantity = physicalLeg.DeliveryQuantity.TotalPhysicalQuantity;
                }
                else if (physicalLeg.DeliveryQuantity.PhysicalQuantitySpecified)
                {
                    totalQuantity = TotalQuantityCalculation(pProductBase, physicalLeg.DeliveryQuantity.PhysicalQuantity);
                }
                else if (physicalLeg.DeliveryQuantity.PhysicalQuantityScheduleSpecified)
                {
                }

            }
            else if (pLeg1 is IEnvironmentalPhysicalLeg)
            {
                IEnvironmentalPhysicalLeg physicalLeg = pLeg1 as IEnvironmentalPhysicalLeg;
                totalQuantity = physicalLeg.NumberOfAllowances;
            }
        }


        private IUnitQuantity TotalQuantityCalculation(IProductBase pProductBase, ICommodityNotionalQuantity[] pCommodityNotionalQuantity)
        {
            decimal _totalQuantity = 0;
            string _unitQuantity = string.Empty;
            // Calcul de la quantité totale sur la base de la quantité par fréquence
            List<ICommodityNotionalQuantity> physicalQuantity = pCommodityNotionalQuantity.ToList();
            physicalQuantity.ForEach(item =>
            {
                if (System.Enum.IsDefined(typeof(CommodityQuantityFrequencyEnum), item.QuantityFrequency.Value))
                {
                    _unitQuantity = item.QuantityUnit.Value;

                    CommodityQuantityFrequencyEnum frequency = (CommodityQuantityFrequencyEnum)ReflectionTools.EnumParse(new CommodityQuantityFrequencyEnum(), item.QuantityFrequency.Value);
                    switch (frequency)
                    {
                        case CommodityQuantityFrequencyEnum.InFine:
                        case CommodityQuantityFrequencyEnum.Term:
                            _totalQuantity += item.Quantity.DecValue;
                            break;
                        case CommodityQuantityFrequencyEnum.PerBusinessDay:
                            break;
                        case CommodityQuantityFrequencyEnum.PerCalculationPeriod:
                            break;
                        case CommodityQuantityFrequencyEnum.PerCalendarDay:

                            break;
                        case CommodityQuantityFrequencyEnum.PerHour:
                            break;
                        case CommodityQuantityFrequencyEnum.PerMonth:
                            break;
                        case CommodityQuantityFrequencyEnum.PerSettlementPeriod:
                            break;
                    }
                }
            });
            IUnitQuantity unitQuantity = pProductBase.CreateUnitQuantity();
            unitQuantity.Quantity.DecValue = _totalQuantity;
            unitQuantity.QuantityUnit.Value = _unitQuantity;
            return unitQuantity;
        }
        #endregion Methods
    }
    #endregion EFS_CommodityQuantity

    #region EFS_CommoditySpot
    /// <summary>
    /// Point d'entrée pour la génération des événements
    /// </summary>
    public class EFS_CommoditySpot : EFS_CommodityBase
    {
        #region Members
        public EFS_FixedPriceSpotLeg fixedLeg;
        public EFS_PhysicalLeg physicalLeg;
        private readonly EFS_Asset m_AssetCommodity;
        #endregion Members

        #region Accessors
        #region AssetCommodity
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Asset AssetCommodity
        {
            get { return m_AssetCommodity; }
        }
        #endregion AssetCommodity

        #endregion Accessors
        #region Constructors
        public EFS_CommoditySpot(string pConnectionString, DataDocumentContainer pDataDocument, ICommoditySpot pCommoditySpot)
            : this(pConnectionString, null, pDataDocument, pCommoditySpot)
        { }

        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20221201 [25639] [WI484] Add Environmental Test
        public EFS_CommoditySpot(string pConnectionString, IDbTransaction pDbTransaction, DataDocumentContainer pDataDocument, ICommoditySpot pCommoditySpot)
            : base(pConnectionString, pDataDocument.TradeHeader.TradeDate, pCommoditySpot, pDataDocument)
        {
            // EG 2015124 [POC-MUREX] Add pDbTransaction
            CommoditySpotContainer commoditySpotContainer = new CommoditySpotContainer(m_Cs, pDbTransaction, pCommoditySpot, m_DataDocument);
            clearingBusinessDate = commoditySpotContainer.ClearingBusinessDate;

            #region Asset
            if (pCommoditySpot.PhysicalLeg.CommodityAssetSpecified)
            {
                SQL_AssetCommodityContract sql_Asset = commoditySpotContainer.AssetCommodity;
                m_AssetCommodity = new EFS_Asset
                {
                    idC = sql_Asset.IdC,
                    IdMarket = sql_Asset.IdM,
                    idAsset = sql_Asset.Id,
                    assetCategory = sql_Asset.AssetCategory,

                    contractIdentifier = sql_Asset.Identifier,
                    contractDisplayName = sql_Asset.DisplayName,
                    description = sql_Asset.Description,
                    contractSymbol = sql_Asset.CommodityContract_ContractSymbol,
                    commodityClass = sql_Asset.CommodityContract_Class,
                    commodityQuality = sql_Asset.CommodityContract_Quality,
                    commodityType = sql_Asset.CommodityContract_Type,
                    deliveryPoint = sql_Asset.CommodityContract_DeliveryPoint,
                    tradableType = sql_Asset.CommodityContract_TradableType,
                    deliveryTimezone = sql_Asset.CommodityContract_TimeZone,
                    duration = sql_Asset.CommodityContract_Duration
                };
            }
            #endregion Asset

            fixedLeg = new EFS_FixedPriceSpotLeg(m_Cs, pDataDocument, pCommoditySpot);

            if (pCommoditySpot.IsGas)
                physicalLeg = new EFS_GasPhysicalLeg(m_Cs, pDataDocument, pCommoditySpot, effectiveDate, terminationDate);
            else if (pCommoditySpot.IsElectricity)
                physicalLeg = new EFS_ElectricityPhysicalLeg(m_Cs, pDataDocument, pCommoditySpot, effectiveDate, terminationDate);
            else if (pCommoditySpot.IsEnvironmental)
                physicalLeg = new EFS_EnvironmentalPhysicalLeg(m_Cs, pDataDocument, pCommoditySpot, effectiveDate, terminationDate);
        }
        #endregion Constructors
    }
    #endregion EFS_CommoditySpot


    #region EFS_ElectricityPhysicalLeg
    public class EFS_ElectricityPhysicalLeg : EFS_PhysicalLeg
    {
        #region Members
        #endregion Members
        #region Accessors
        #region EventType
        public override string EventType
        {
            get { return EventTypeFunc.CommodityElectricity; }
        }
        #endregion EventType
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ElectricityPhysicalLeg(string pConnectionString, DataDocumentContainer pDataDocument, ICommoditySpot pCommoditySpot, EFS_AdjustableDate pEffectiveDate, EFS_AdjustableDate pTerminationDate)
            : base(pConnectionString, pCommoditySpot, pEffectiveDate, pTerminationDate, pDataDocument)
        {
            IElectricityPhysicalLeg electricityPhysicalLeg = pCommoditySpot.PhysicalLeg as IElectricityPhysicalLeg;
            deliveryStartTime = electricityPhysicalLeg.SettlementPeriods[0].StartTime.Time;
            deliveryEndTime = electricityPhysicalLeg.SettlementPeriods[0].EndTime.Time;
            Calc();
        }
        #endregion Constructors
    }
    #endregion EFS_ElectricityPhysicalLeg

    // EG 20221201 [25639] [WI484] New
    public class EFS_EnvironmentalPhysicalLeg : EFS_PhysicalLeg
    {
        #region Accessors
        #region EventType
        public override string EventType
        {
            get { return EventTypeFunc.CommodityEnvironmental; }
        }
        #endregion EventType
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_EnvironmentalPhysicalLeg(string pConnectionString, DataDocumentContainer pDataDocument, ICommoditySpot pCommoditySpot, EFS_AdjustableDate pEffectiveDate, EFS_AdjustableDate pTerminationDate)
            : base(pConnectionString, pCommoditySpot, pEffectiveDate, pTerminationDate, pDataDocument)
        {
        }
        #endregion Constructors
    }

    #region EFS_FixedPrice
    public class EFS_FixedPrice
    {
        #region Members
        public EFS_Decimal price;
        public EFS_Decimal settlementPrice;
        public string priceCurrency;
        public string settlementCurrency;
        public EFS_FxFixing efs_Fixing;
        public EFS_Decimal totalQuantity;
        public string unitQuantity;
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FixedPrice(string pCs, IProductBase pProductBase, IFixedPriceLegBase pFixedLeg, ICurrency pSettlementCurrency,
            EFS_AdjustableDate pPaymentDate, IUnitQuantity pTotalQuantity, DataDocumentContainer pDataDocument)
        {

            price = pFixedLeg.FixedPrice.Price;
            priceCurrency = pFixedLeg.FixedPrice.Currency.Value;
            totalQuantity = new EFS_Decimal(pTotalQuantity.Quantity.DecValue);
            unitQuantity = pTotalQuantity.QuantityUnit.Value;
            settlementCurrency = pSettlementCurrency.Value;

            // Conversion du prix si priceCurrency <> settlementCurrency
            if (priceCurrency != settlementCurrency)
            {
                #region FxRate
                KeyAssetFxRate keyAssetFxRate = new KeyAssetFxRate
                {
                    IdC1 = priceCurrency,
                    IdC2 = settlementCurrency
                };
                int idAsset = keyAssetFxRate.GetIdAsset(pCs, null);
                if (0 < idAsset)
                {
                    SQL_AssetFxRate sql_AssetFxRate = new SQL_AssetFxRate(pCs, idAsset, SQL_Table.ScanDataDtEnabledEnum.Yes);
                    if (sql_AssetFxRate.IsLoaded)
                    {
                        IFxFixing fixing = pProductBase.CreateFxFixing();
                        fixing.PrimaryRateSource = fixing.CreateInformationSource();
                        fixing.PrimaryRateSource.OTCmlId = sql_AssetFxRate.Id;
                        fixing.PrimaryRateSource.RateSource.Value = sql_AssetFxRate.PrimaryRateSrc;
                        fixing.PrimaryRateSource.RateSourcePageSpecified = StrFunc.IsFilled(sql_AssetFxRate.PrimaryRateSrcPage);
                        if (fixing.PrimaryRateSource.RateSourcePageSpecified)
                            fixing.PrimaryRateSource.CreateRateSourcePage(sql_AssetFxRate.PrimaryRateSrcPage);
                        fixing.PrimaryRateSource.RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_AssetFxRate.PrimaryRateSrcHead);
                        fixing.PrimaryRateSource.RateSourcePageHeading = sql_AssetFxRate.PrimaryRateSrcHead;
                        fixing.CreateQuotedCurrencyPair(sql_AssetFxRate.QCP_Cur1, sql_AssetFxRate.QCP_Cur2, sql_AssetFxRate.QCP_QuoteBasisEnum);
                        //fixing.CreateBusinessCenterTime
                        fixing.FixingTime = fixing.CreateBusinessCenterTime();
                        fixing.FixingTime.HourMinuteTime.TimeValue = sql_AssetFxRate.TimeRateSrc;
                        fixing.FixingTime.BusinessCenter.Value = sql_AssetFxRate.IdBC_RateSrc;
                        fixing.FixingDate = new EFS_Date();

                        IBusinessDayAdjustments bda = pProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.PRECEDING, sql_AssetFxRate.IdBC_RateSrc);
                        EFS_AdjustableDate fixingDate = new EFS_AdjustableDate(pCs, pPaymentDate.AdjustableDate.GetAdjustedDate().DateValue, bda, pDataDocument);
                        fixing.FixingDate.DateValue = fixingDate.AdjustedEventDate.DateValue;
                        fixing.QuotedCurrencyPair = fixing.CreateQuotedCurrencyPair(sql_AssetFxRate.QCP_Cur1, sql_AssetFxRate.QCP_Cur2, sql_AssetFxRate.QCP_QuoteBasisEnum);
                        efs_Fixing = new EFS_FxFixing(EventTypeFunc.FxRate, fixing);
                    }
                }
                else
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Asset not found : " + keyAssetFxRate.IdC1 + "-" + keyAssetFxRate.IdC2);
                }
                #endregion FxRate
            }
            else
            {
                settlementPrice = new EFS_Decimal(price.DecValue);
            }
        }
        #endregion Constructors
    }
    #endregion EFS_FixedPrice
    #region EFS_FixedSpotPrice
    public class EFS_FixedSpotPrice : EFS_FixedPrice
    {
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FixedSpotPrice(string pCs, ICommoditySpot pCommoditySpot, EFS_AdjustableDate pPaymentDate, IUnitQuantity pTotalQuantity, DataDocumentContainer pDataDocument)
            :base(pCs,(IProductBase) pCommoditySpot, pCommoditySpot.FixedLeg, pCommoditySpot.SettlementCurrency, pPaymentDate, pTotalQuantity, pDataDocument)
        {
        }
        #endregion Constructors
    }
    #endregion EFS_FixedSpotPrice
    #region EFS_FixedPriceLeg
    public class EFS_FixedPriceLeg
    {
        #region Members
        protected string cs;
        protected IFixedPriceLeg fixedLeg;
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public EFS_CommodityQuantity commodityQuantity;
        public EFS_CommoditySwapPayment[] commodityPayment;
        #endregion Members
        #region Accessors
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return payerPartyReference.HRef;

            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return receiverPartyReference.HRef;

            }
        }
        #endregion ReceiverPartyReference
        #region EventType
        public string EventType
        {
            get { return EventTypeFunc.FixedLeg; }
        }
        #endregion EventType
        #endregion Accessors
        #region Constructors
        public EFS_FixedPriceLeg(string pConnectionString, DataDocumentContainer pDataDocument, ICommoditySwap pCommoditySwap)
        {
            cs = pConnectionString;
            fixedLeg = pCommoditySwap.FixedLeg;

            payerPartyReference = fixedLeg.PayerPartyReference;
            receiverPartyReference = fixedLeg.ReceiverPartyReference;

            #region Quantity Calculation
            commodityQuantity = new EFS_CommodityQuantity((IProductBase)pCommoditySwap, fixedLeg, pCommoditySwap.PhysicalLeg, pDataDocument);
            #endregion Quantity Calculation


            #region Payment calculation
            if (fixedLeg.RelativePaymentDatesSpecified)
            {
                // TO DO            
            }
            else if (fixedLeg.PaymentDatesSpecified)
            {
                EFS_AdjustableDates paymentDatesAdjustment = new EFS_AdjustableDates();
                bool paymentDatesAdjustmentSpecified = false;
                if (fixedLeg.PaymentDates.AdjustableDatesSpecified)
                {
                    #region AdjustableDates
                    IAdjustableDates adjustableDates = fixedLeg.PaymentDates.AdjustableDates;
                    object[] unadjustedDates = adjustableDates.UnadjustedDate;
                    paymentDatesAdjustment.adjustableDates = new EFS_AdjustableDate[unadjustedDates.Length];
                    for (int i = 0; i < unadjustedDates.Length; i++)
                    {
                        paymentDatesAdjustment.adjustableDates[i] = new EFS_AdjustableDate(cs, adjustableDates[i], 
                            adjustableDates.DateAdjustments, pDataDocument);
                    }
                    paymentDatesAdjustmentSpecified = true;
                    #endregion AdjustableDates
                }
                else if (fixedLeg.PaymentDates.RelativeDateOffsetSpecified)
                {
                    #region RelativeDateOffset
                    if (Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(cs, fixedLeg.PaymentDates.RelativeDateOffset, out DateTime[] offsetDates, pDataDocument))
                    {
                        paymentDatesAdjustment.adjustableDates = new EFS_AdjustableDate[offsetDates.Length];
                        for (int i = 0; i < offsetDates.Length; i++)
                        {
                            paymentDatesAdjustment.adjustableDates[i] = new EFS_AdjustableDate(cs, offsetDates[i],
                                fixedLeg.PaymentDates.RelativeDateOffset.GetAdjustments, pDataDocument);
                        }
                        paymentDatesAdjustmentSpecified = true;
                    }
                    #endregion RelativeDateOffset
                }
                if (paymentDatesAdjustmentSpecified)
                {
                    commodityPayment = new EFS_CommoditySwapPayment[paymentDatesAdjustment.adjustableDates.Length];
                    for (int i = 0; i < paymentDatesAdjustment.adjustableDates.Length; i++)
                    {
                        commodityPayment[i] = new EFS_CommoditySwapPayment(cs, pCommoditySwap, paymentDatesAdjustment.adjustableDates[i], 
                            commodityQuantity, pDataDocument);
                    }
                }
            }
            #endregion Payment calculation
        }
        #endregion Constructors
    }
    #endregion EFS_FixedPriceLeg
    #region EFS_FixedPriceSpotLeg
    public class EFS_FixedPriceSpotLeg
    {
        #region Members
        protected string m_Cs;
        protected DataDocumentContainer m_DataDocument; 
        protected IFixedPriceSpotLeg fixedLeg;
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public EFS_CommodityQuantity commodityQuantity;
        public EFS_CommodityPayment commodityPayment;
        #endregion Members
        #region Accessors
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return payerPartyReference.HRef;

            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return receiverPartyReference.HRef;

            }
        }
        #endregion ReceiverPartyReference
        #region EventType
        public string EventType
        {
            get { return EventTypeFunc.FixedLeg; }
        }
        #endregion EventType
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FixedPriceSpotLeg(string pConnectionString, DataDocumentContainer pDataDocument, ICommoditySpot pCommoditySpot)
        {   
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            fixedLeg = pCommoditySpot.FixedLeg;

            payerPartyReference = fixedLeg.PayerPartyReference;
            receiverPartyReference = fixedLeg.ReceiverPartyReference;

            
            IProductBase productBase = (IProductBase)pCommoditySpot;
            commodityQuantity = new EFS_CommodityQuantity(productBase, fixedLeg, pCommoditySpot.PhysicalLeg, pDataDocument);
            
            commodityPayment = new EFS_CommodityPayment(m_Cs, pCommoditySpot, commodityQuantity, pDataDocument);
            
        }
        #endregion Constructors
    }
    #endregion EFS_FixedPriceSpotLeg

    #region EFS_GasPhysicalLeg
    public class EFS_GasPhysicalLeg : EFS_PhysicalLeg
    {
        #region Members
        #endregion Members
        #region Accessors
        #region EventType
        public override string EventType
        {
            get { return EventTypeFunc.CommodityGas; }
        }
        #endregion EventType
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_GasPhysicalLeg(string pConnectionString, DataDocumentContainer pDataDocument, ICommoditySpot pCommoditySpot, EFS_AdjustableDate pEffectiveDate, EFS_AdjustableDate pTerminationDate)
            : base(pConnectionString, pCommoditySpot, pEffectiveDate, pTerminationDate, pDataDocument)
        {
            IGasPhysicalLeg gasPhysicalLeg = pCommoditySpot.PhysicalLeg as IGasPhysicalLeg;
            deliveryStartTime = gasPhysicalLeg.DeliveryPeriods.SupplyStartTime;
            deliveryEndTime = gasPhysicalLeg.DeliveryPeriods.SupplyEndTime;
            Calc();
        }
        #endregion Constructors
    }
    #endregion EFS_GasPhysicalLeg

    #region EFS_PhysicalLeg
    public abstract class EFS_PhysicalLeg
    {
        #region Members
        protected string cs;
        protected DataDocumentContainer m_DataDocument;
        protected IPhysicalLeg physicalLeg;
        protected IReference payerPartyReference;
        protected IReference receiverPartyReference;

        public EFS_AdjustableDate effectiveDate;
        public EFS_AdjustableDate terminationDate;
        public IPrevailingTime deliveryStartTime;
        public IPrevailingTime deliveryEndTime;
        public EFS_CommodityQuantity commodityQuantity;
        public EFS_CommodityDeliveryDate deliveryDate;
        #endregion Members
        #region Accessors
        #region EventType
        public virtual string EventType
        {
            get { return string.Empty; }
        }
        #endregion EventType

        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return payerPartyReference.HRef;

            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return receiverPartyReference.HRef;

            }
        }
        #endregion ReceiverPartyReference
        #region DeliveryDate
        public EFS_CommodityDeliveryDate DeliveryDate
        {
            get
            {
                return deliveryDate;
            }
        }
        #endregion DeliveryDate

        #region DeliveryStartDate
        // EG 20171025 [23509] Upd DateTime replace DateTimeOffset
        private DateTime DeliveryStartDate
        {
            get
            {
                return DtFuncML.CalcDeliveryDateTimeUTC(effectiveDate.adjustedDate.DateValue, deliveryStartTime);
            }
        }
        #endregion DeliveryStartDate
        #region DeliveryEndDate
        // EG 20171025 [23509] Upd DateTime replace DateTimeOffset
        private DateTime DeliveryEndDate
        {
            get
            {
                return DtFuncML.CalcDeliveryDateTimeUTC(terminationDate.adjustedDate.DateValue, deliveryEndTime);
            }
        }
        #endregion DeliveryEndDate

        #region Quantity
        public EFS_Decimal Quantity
        {
            get
            {
                return new EFS_Decimal(commodityQuantity.totalQuantity.Quantity.DecValue);
            }
        }
        #endregion Quantity
        #region UnitQuantity
        public string UnitQuantity
        {
            get
            {
                return commodityQuantity.totalQuantity.QuantityUnit.Value;
            }
        }
        #endregion UnitQuantity
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_PhysicalLeg(string pConnectionString, ICommoditySpot pCommoditySpot, EFS_AdjustableDate pEffectiveDate, EFS_AdjustableDate pTerminationDate, 
            DataDocumentContainer pDataDocument)
        {
            cs = pConnectionString;
            m_DataDocument = pDataDocument;
            physicalLeg = pCommoditySpot.PhysicalLeg;
            payerPartyReference = physicalLeg.PayerPartyReference;
            receiverPartyReference = physicalLeg.ReceiverPartyReference;
            effectiveDate = pEffectiveDate;
            terminationDate = pTerminationDate;

            commodityQuantity = new EFS_CommodityQuantity((IProductBase)pCommoditySpot, pCommoditySpot.FixedLeg, physicalLeg, m_DataDocument);
        }
        #endregion Constructors
        #region Methods
        // EG 20171025 [23509] Set timeZone
        protected void Calc()
        {
            deliveryDate = new EFS_CommodityDeliveryDate
            {
                startDate = DeliveryStartDate,
                endDate = DeliveryEndDate,
                timezone = deliveryEndTime.Location.Value
            };
        }
        #endregion Methods
    }
    #endregion EFS_PhysicalLeg

}

#region using directives

using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.Doc;
using FixML.Enum;
using FixML.v50SP1;
using FixML.v50SP1.Enum;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Collections.Generic;

#endregion using directives

/// EG 20150302 CFD Forex Add PosKeepingAsset_FXRATE
namespace EfsML.v30.PosRequest
{
    //──────────────────────────────────────────────────────────────────────────────────────────────────
    // Classes de stockage et travail des données utiles aux différents traitements de tenue de position 
    //──────────────────────────────────────────────────────────────────────────────────────────────────
    #region PosKeepingAsset
    // EG 20190926 [Maturity Redemption] Add IssuerPricePercentage and RedemptionpricePercentage
    public abstract partial class PosKeepingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("idAsset")]
        public int idAsset;
        [System.Xml.Serialization.XmlElementAttribute("identifier")]
        public string identifier;
        [System.Xml.Serialization.XmlElementAttribute("contractMultiplier")]
        public decimal contractMultiplier;
        [System.Xml.Serialization.XmlElementAttribute("nominal")]
        public decimal nominal;
        [System.Xml.Serialization.XmlElementAttribute("nominalCurrency")]
        public string nominalCurrency;
        [System.Xml.Serialization.XmlElementAttribute("priceCurrency")]
        public string priceCurrency;
        [System.Xml.Serialization.XmlElementAttribute("currency")]
        public string currency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idMSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idM")]
        public int idM;
        [System.Xml.Serialization.XmlElementAttribute("idBC")]
        public string idBC;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quote")]
        public PosKeepingQuote quote;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quoteReference")]
        public PosKeepingQuote quoteReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isOfficialCloseMandatory;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isOfficialSettlementMandatory;

        #endregion Members
        #region Accessors
        public virtual int IdA_Issuer
        {
            get;
            set;
        }
        public virtual int IdB_Issuer
        {
            get;
            set;
        }
        // EG 20190918 Add
        public virtual decimal IssuePricePercentage
        {
            get;
            set;
        }
        // EG 20190918
        public virtual decimal RedemptionPricePercentage
        {
            get;
            set;
        }
        #endregion Accessors
    }
    #endregion PosKeepingAsset
    #region PosKeepingAsset_BOND
    // EG 20190926 [Maturity Redemption] Add IssuerPricePercentage and RedemptionpricePercentage
    public partial class PosKeepingAsset_BOND : PosKeepingAsset
    {
        #region Members
        public int idA_Issuer;
        public int idB_Issuer;
        public decimal issuePricePercentage;
        public decimal redemptionPricePercentage;
        #endregion Members
        #region Accessors
        public override int IdA_Issuer
        {
            get { return idA_Issuer; }
            set { idA_Issuer = value; }
        }
        public override int IdB_Issuer
        {
            get { return idB_Issuer; }
            set { idB_Issuer = value; }
        }
        public override decimal IssuePricePercentage
        {
            get { return issuePricePercentage; }
            set { issuePricePercentage = value; }
        }
        public override decimal RedemptionPricePercentage
        {
            get { return redemptionPricePercentage; }
            set { redemptionPricePercentage = value; }
        }
        #endregion Accessors

    }
    #endregion PosKeepingAsset_BOND
    #region PosKeepingAsset_COMS
    public partial class PosKeepingAsset_COMS : PosKeepingAsset
    {
        #region Members
        #endregion Members
        #region Accessors
        #endregion Accessors
    }
    #endregion PosKeepingAsset_COMS
    #region PosKeepingAsset_ETD
    /// <summary>
    /// <para>Classe de travail contients diverses caractéristiques liées à l'ASSET</para>
    /// <para>► Elements la clé de position utilisés dans la tenue de position (pour les calculs par exemple).</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>  ● Maturity date</para>
    /// <para>  ● Nominal, Prix, Strike, Factor,</para>
    /// <para>  ● Devises</para>
    /// <para>  ● Sous-jacent etc...</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    /// 20110816 [FI] Ajout de nominalCurrency et priceCurrency pour satisfaire la nouvelle signature de IPosKeepingAsset
    /// EG 20130603 Ticket: 18721 Détermination de la date de lecture du Prix de référence pour les dénouements automatiques à l'échéance
    /// PM 20130807 [18876] Ajout priceQuoteMethod
    /// EG 20140120 Report 3.7
    /// EG 20170206 [22787] Add firstDeliveryDate, lastDeliveryDate, etc.
    /// FI 20170303 [22916] Modify
    /// PL 20170411 [23064] Modify
    public partial class PosKeepingAsset_ETD : PosKeepingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("maturityDate")]
        public DateTime maturityDate;
        // EG 20140326 [19771][19785] 
        [System.Xml.Serialization.XmlElementAttribute("maturityDateSys")]
        public DateTime maturityDateSys;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool precedingMaturityDateSysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("precedingMaturityDateSys")]
        public DateTime precedingMaturityDateSys;
        /// EG 20130603 Ticket: 18721 
        [System.Xml.Serialization.XmlElementAttribute("lastTradingDay")]
        public DateTime lastTradingDay;
        /// EG 20130603 Ticket: 18721
        [System.Xml.Serialization.XmlElementAttribute("finalSettlementPrice")]
        public FinalSettlementPriceEnum finalSettlementPrice;

        [System.Xml.Serialization.XmlElementAttribute("category")]
        public CfiCodeCategoryEnum category;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exerciseStyleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseStyle")]
        public DerivativeExerciseStyleEnum exerciseStyle;
        [System.Xml.Serialization.XmlElementAttribute("strikePrice")]
        public decimal strikePrice;
        /// <summary>
        ///  In the money condition of "At-the-money" option
        /// </summary>
        /// FI 20170303 [22916] Add
        [System.Xml.Serialization.XmlElementAttribute("inTheMoneyCondition")]
        public ITMConditionEnum inTheMoneyCondition;

        [System.Xml.Serialization.XmlElementAttribute("instrumentNum")]
        public int instrumentNum;
        [System.Xml.Serialization.XmlElementAttribute("instrumentDen")]
        public int instrumentDen;
        [System.Xml.Serialization.XmlElementAttribute("settlMethod")]
        public SettlMethodEnum settlMethod;
        // PL 20170411 [23064] Add column PHYSETTLTAMOUNT
        [System.Xml.Serialization.XmlElementAttribute("physicalSettlementAmount")]
        public PhysicalSettlementAmountEnum physicalSettlementAmount;

        [System.Xml.Serialization.XmlElementAttribute("putCall")]
        public PutOrCallEnum putCall;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool putCallSpecified;
        [System.Xml.Serialization.XmlElementAttribute("assetCategory")]
        public Cst.UnderlyingAsset assetCategory;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetCategorySpecified;
        [System.Xml.Serialization.XmlElementAttribute("factor")]
        public decimal factor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryDate")]
        public DateTime deliveryDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryDelayOffsetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryDelayOffset")]
        public IOffset deliveryDelayOffset;
        [System.Xml.Serialization.XmlElementAttribute("idAsset_Underlyer")]
        public int idAsset_Underlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAsset_UnderlyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("identifier_Underlyer")]
        public string identifier_Underlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool identifier_UnderlyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idDC_Underlyer")]
        public int idDC_Underlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idDC_UnderlyerSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetCategory_UnderlyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("assetCategory_Underlyer")]
        public Cst.UnderlyingAsset assetCategory_Underlyer;


        [System.Xml.Serialization.XmlElementAttribute("instrumentNum_Underlyer")]
        public int instrumentNum_Underlyer;
        [System.Xml.Serialization.XmlElementAttribute("instrumentDen_Underlyer")]
        public int instrumentDen_Underlyer;
        [System.Xml.Serialization.XmlElementAttribute("priceQuoteMethod")]
        public PriceQuoteMethodEnum priceQuoteMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool finalSettlementSideSpecified;
        [System.Xml.Serialization.XmlElementAttribute("finalSettlementSide")]
        public QuotationSideEnum finalSettlementSide;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool finalSettlementTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("finalSettlementTime")]
        public string finalSettlementTime;
        // EG 20120605 [19807] New
        [System.Xml.Serialization.XmlElementAttribute("futValuationMethod")]
        public FuturesValuationMethodEnum valuationMethod;
        //PM 20140807 [20273][20106] Ajout de roundDir et roundPrec
        [System.Xml.Serialization.XmlElementAttribute("curRoundDir")]
        public string roundDir;
        [System.Xml.Serialization.XmlElementAttribute("curRoundPrec")]
        public int roundPrec;
        //PM 20141120 [20508] Ajout de cashFlowCalculationMethod
        [System.Xml.Serialization.XmlElementAttribute("cashFlowCalculationMethod")]
        public CashFlowCalculationMethodEnum cashFlowCalculationMethod;

        // EG 20170206 [22787] New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unitOfMeasureSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unitOfMeasure")]
        public string unitOfMeasure;
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool unitOfMeasureQtySpecified;
        [System.Xml.Serialization.XmlElementAttribute("unitOfMeasureQty")]
        public decimal unitOfMeasureQty;

        // EG 20170206 [22787] New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool firstDeliveryDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstDeliveryDate")]
        public DateTime firstDeliveryDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lastDeliveryDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("lastDeliveryDate")]
        public DateTime lastDeliveryDate;

        // EG 20170206 [22787] New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryTimeStartSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryTimeStart")]
        public IPrevailingTime deliveryTimeStart;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryTimeEndSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryTimeEnd")]
        public IPrevailingTime deliveryTimeEnd;

        // EG 20170206 [22787] New
        [System.Xml.Serialization.XmlElementAttribute("isApplySummertime")]
        public bool isApplySummertime;

        // EG 20170206 [22787] New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryPeriodFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliveryPeriodFrequency")]
        public ICalculationPeriodFrequency deliveryPeriodFrequency;

        // EG 20170206 [22787] New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dayTypeDeliverySpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayTypeDelivery")]
        public DayTypeEnum dayTypeDelivery;

        // EG 20170206 [22787] New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliverySettlementOffsetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("deliverySettlementOffset")]
        public IOffset deliverySettlementOffset;

        // EG 20170206 [22787] New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settlementOfHolidayDeliveryConventionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementOfHolidayDeliveryConvention")]
        public SettlementOfHolidayDeliveryConventionEnum settlementOfHolidayDeliveryConvention;

        // EG 20170206 [22787] New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool firstDeliverySettlementDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstDeliverySettlementDate")]
        public DateTime firstDeliverySettlementDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lastDeliverySettlementDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("lastDeliverySettlementDate")]
        public DateTime lastDeliverySettlementDate;
        #endregion Members
    }
    #endregion PosKeepingAsset_ETD
    #region PosKeepingAsset_EQUITY
    public partial class PosKeepingAsset_EQUITY : PosKeepingAsset
    {
    }
    #endregion PosKeepingAsset_EQUITY
    #region PosKeepingAsset_INDEX
    public partial class PosKeepingAsset_INDEX : PosKeepingAsset
    {
    }
    #endregion PosKeepingAsset_INDEX
    #region PosKeepingAsset_RATEINDEX
    public partial class PosKeepingAsset_RATEINDEX : PosKeepingAsset
    {
    }
    #endregion PosKeepingAsset_RATEINDEX
    #region PosKeepingAsset_FXRATE
    /// EG 20150302 New CFD Forex 
    public partial class PosKeepingAsset_FXRATE : PosKeepingAsset
    {
    }
    #endregion PosKeepingAsset_FXRATE

    #region PosKeepingTrade
    /// <summary>
    /// <para>Classe contenant la liste des trades liés à une clôture spécifique</para>
    /// </summary>
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
    public partial class PosKeepingClearingTrade
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("idT")]
        public int idT;
        [System.Xml.Serialization.XmlElementAttribute("identifier")]
        public string identifier;
        [System.Xml.Serialization.XmlElementAttribute("availableQty")]
        public decimal availableQty;
        [System.Xml.Serialization.XmlElementAttribute("closableQty")]
        public decimal closableQty;
        [System.Xml.Serialization.XmlElementAttribute("dtBusiness")]
        public DateTime dtBusiness;
        [System.Xml.Serialization.XmlElementAttribute("dtExecution")]
        public DateTime dtExecution;
        #endregion Members
    }
    #endregion PosKeepingClearingTrade
    #region PosKeepingCommon
    /// <summary>
    /// <para>Ensemble des informations communes à tout traitement de tenue de position</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Clé de position</para>
    /// <para>► Identifiant des entités</para>
    /// <para>► Asset</para> 
    /// <para>► Trade</para>
    /// <para>► Marché</para> 
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    public abstract partial class PosKeepingCommon : PosKeepingKey
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("asset")]
        public PosKeepingAsset asset;
        [System.Xml.Serialization.XmlElementAttribute("trade")]
        public PosKeepingTrade trade;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tradeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("market")]
        public PosKeepingMarket market;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marketSpecified;
        [System.Xml.Serialization.XmlElementAttribute("instrument")]
        public SQL_Instrument instrument;
        [System.Xml.Serialization.XmlElementAttribute("product")]
        public SQL_Product product;
        #endregion Members
    }
    #endregion PosKeepingCommon
    #region PosKeepingData
    /// <summary>
    /// <para>Classe de TRAVAIL utilisée principalement dans le service de Tenue de position</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de la classe abstrait PosKeepingCommon</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    public partial class PosKeepingData : PosKeepingCommon
    {
    }
    #endregion PosKeepingData
    #region PosKeepingKey
    /// <summary>
    /// <para>Elements déterminants la clé de position.</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Id instrument, asset</para>
    /// <para>► Id de l'acteur et du book du donneur d'ordre</para>
    /// <para>► Id de l'acteur et du book du compensateur</para>
    /// <para>► Id des entités (hors clé de position)</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    public partial class PosKeepingKey
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("idI")]
        public int idI;
        [System.Xml.Serialization.XmlElementAttribute("underlyingAsset")]
        public Nullable<Cst.UnderlyingAsset> underlyingAsset;
        [System.Xml.Serialization.XmlElementAttribute("idAsset")]
        public int idAsset;
        [System.Xml.Serialization.XmlElementAttribute("idA_Dealer")]
        public int idA_Dealer;
        [System.Xml.Serialization.XmlElementAttribute("idB_Dealer")]
        public int idB_Dealer;
        [System.Xml.Serialization.XmlElementAttribute("idA_Clearer")]
        public int idA_Clearer;
        [System.Xml.Serialization.XmlElementAttribute("idB_Clearer")]
        public int idB_Clearer;

        #region Additional Info
        [System.Xml.Serialization.XmlElementAttribute("idA_EntityDealer")]
        public int idA_EntityDealer;
        [System.Xml.Serialization.XmlElementAttribute("idA_EntityClearer")]
        public int idA_EntityClearer;
        #endregion Additional Info
        #endregion Members
    }
    #endregion PosKeepingKey
    #region PosKeepingKeyComparer
    /// <summary>
    /// <para>IEqualityComparer implementation for the PosKeepingKey class</para>
    /// </summary>
    public class PosKeepingKeyComparer : IEqualityComparer<PosKeepingKey>
    {
        /// <summary>
        /// Check the equality of two keys
        /// </summary>
        /// <param name="x">first key to to be compared</param>
        /// <param name="y">second key to be compared</param>
        /// <returns>true when the provided keys are equal</returns>
        public bool Equals(PosKeepingKey x, PosKeepingKey y)
        {

            if (Object.ReferenceEquals(x, y)) return true;

            if (x is null || y is null)
                return false;

            return
                x.idI == y.idI &&
                x.idAsset == y.idAsset &&
                x.idA_Dealer == y.idA_Dealer &&
                x.idB_Dealer == y.idB_Dealer &&
                x.idA_Clearer == y.idA_Clearer &&
                x.idB_Clearer == y.idB_Clearer;

            // EntityClearer and EntityDealer do not make part of the key

        }

        /// <summary>
        /// Get the hashing code of the input key
        /// </summary>
        /// <param name="obj">input key we want ot compute the hashing code</param>
        /// <returns>the hashing code of the provided key</returns>
        public int GetHashCode(PosKeepingKey obj)
        {
            if (obj is null) return 0;

            int hashIdi = obj.idI.GetHashCode();
            int hashIdAsset = obj.idAsset.GetHashCode();
            int hashIdADealer = obj.idA_Dealer.GetHashCode();
            int hashIdBDealer = obj.idB_Dealer.GetHashCode();
            int hashIdAClearer = obj.idA_Clearer.GetHashCode();
            int hashIdBClearer = obj.idB_Clearer.GetHashCode();

            return hashIdi ^ hashIdAsset ^ hashIdADealer ^ hashIdBDealer ^ hashIdAClearer ^ hashIdBClearer;

            // EntityClearer and EntityDealer do not make part of hash code

        }
    }
    #endregion PosKeepingKeyComparer
    #region PosKeepingMarket
    /// <summary>
    /// <para>Classe de travail contients diverses caractéristiques liées à la table ENTITYMARKET</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► élément implicite de la clé de position</para>
    /// <para>► dates des dates de journées de bourse (PREVIOUS, CURRENT et NEXT)</para>
    /// <para>► business center</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    /// PM 20150422 [20575] Add dtEntityPrev, dtEntity, dtEntityNext
    // EG 20240520 [WI930] New IdA_CSS_Identifier, IdM_Identifier, IdBCEntity
    public partial class PosKeepingMarket
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("idA_Entity")]
        public int idA_Entity;
        [System.Xml.Serialization.XmlElementAttribute("idA_CSS")]
        public int idA_CSS;
        [System.Xml.Serialization.XmlElementAttribute("idA_Css_Identifier")]
        public string idA_Css_Identifier;
        [System.Xml.Serialization.XmlElementAttribute("idA_Custodian")]
        public int idA_Custodian;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_CustodianSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idEM")]
        public int idEM;
        [System.Xml.Serialization.XmlElementAttribute("idM")]
        public int idM;
        [System.Xml.Serialization.XmlElementAttribute("idM_Identifier")]
        public string idM_Identifier;
        [System.Xml.Serialization.XmlElementAttribute("idBC")]
        public string idBC;
        [System.Xml.Serialization.XmlElementAttribute("idBCEntity")]
        public string idBCEntity;
        [System.Xml.Serialization.XmlElementAttribute("dtMarketPrev")]
        public DateTime dtMarketPrev;
        [System.Xml.Serialization.XmlElementAttribute("dtMarket")]
        public DateTime dtMarket;
        [System.Xml.Serialization.XmlElementAttribute("dtMarketNext")]
        public DateTime dtMarketNext;
        [System.Xml.Serialization.XmlElementAttribute("dtEntityPrev")]
        public DateTime dtEntityPrev;
        [System.Xml.Serialization.XmlElementAttribute("dtEntity")]
        public DateTime dtEntity;
        [System.Xml.Serialization.XmlElementAttribute("dtEntityNext")]
        public DateTime dtEntityNext;
        #endregion Members
    }
    #endregion PosKeepingMarket
    #region PosKeepingQuote
    /// <summary>
    /// <para>Classe de travail contients les caractéristiques d'une cotation d'un asset</para>
    /// </summary>
    /// EG 20140120 Report 3.7
    public partial class PosKeepingQuote
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("category")]
        public Cst.UnderlyingAsset category;
        [System.Xml.Serialization.XmlElementAttribute("idAsset")]
        public int idAsset;
        [System.Xml.Serialization.XmlElementAttribute("identifier")]
        public string identifier;
        [System.Xml.Serialization.XmlElementAttribute("time")]
        public DateTime quoteTime;
        [System.Xml.Serialization.XmlElementAttribute("quoteSide")]
        public QuotationSideEnum quoteSide;
        [System.Xml.Serialization.XmlElementAttribute("quoteTiming")]
        public QuoteTimingEnum quoteTiming;
        [System.Xml.Serialization.XmlElementAttribute("value")]
        public decimal quotePrice;
        [System.Xml.Serialization.XmlElementAttribute("source")]
        public string source;
        #endregion Members
    }
    #endregion PosKeepingQuote
    #region PosKeepingTrade
    /// <summary>
    /// <para>Classe de travail contients diverses caractéristiques liées à un trade dans un traitement de tenue de position</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques essentielles (sens,PositionEffect, quantité, dtBusiness, dtExecution, ...</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
    public partial class PosKeepingTrade
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("idT")]
        public int idT;
        [System.Xml.Serialization.XmlElementAttribute("identifier")]
        public string identifier;
        [System.Xml.Serialization.XmlElementAttribute("side")]
        public int side;
        [System.Xml.Serialization.XmlElementAttribute("qty")]
        public decimal qty;
        [System.Xml.Serialization.XmlElementAttribute("dtBusiness")]
        public DateTime dtBusiness;
        [System.Xml.Serialization.XmlElementAttribute("dtExecution")]
        public DateTime dtExecution;
        [System.Xml.Serialization.XmlElementAttribute("dtSettlt")]
        public DateTime dtSettlt;
        [System.Xml.Serialization.XmlElementAttribute("positionEffect")]
        public string positionEffect;
        [System.Xml.Serialization.XmlElementAttribute("positionEffect_BookDealer")]
        public string positionEffect_BookDealer;
        [System.Xml.Serialization.XmlElementAttribute("isIgnorepositionEffect_BookDealer")]
        public bool isIgnorePositionEffect_BookDealer;

        [System.Xml.Serialization.XmlIgnore()]
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        public bool UltimatelyPositionEffectSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ultimatelyPositionEffect")]
        public string UltimatelyPositionEffect
        {
            get
            {
                string ret = positionEffect;
                if (isIgnorePositionEffect_BookDealer || !ExchangeTradedDerivativeTools.IsPositionEffect_Open(positionEffect))
                {
                    if (!String.IsNullOrEmpty(positionEffect_BookDealer))
                    {
                        ret = positionEffect_BookDealer;
                    }
                }
                return ret;
            }
        }
        #endregion Members
    }
    #endregion PosKeepingTrade
    #region SplitNewTrade
    /// <summary>
    /// Classe contenant les caractéristiques d'un trade résultant du Splitting d'un trade
    /// </summary>
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public partial class SplitNewTrade
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IdStActivation")]
        public string IdStActivation;
        [System.Xml.Serialization.XmlElementAttribute("Actor")]
        public string ActorIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("IdA")]
        public int IdA;
        [System.Xml.Serialization.XmlElementAttribute("Book")]
        public string BookIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("IdB")]
        public int IdB;
        [System.Xml.Serialization.XmlElementAttribute("Qty")]
        public decimal Qty;
        [System.Xml.Serialization.XmlElementAttribute("PosEfct")]
        public string PosEfct;
        #endregion Members
    }
    #endregion SplitNewTrade

    // Classes principale de stockage et de travail utilisées par les différents traitements
    // de tenue de position. 
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // ► Utilisation côté CLIENT  : pour ECRIRE la demande de traitement (IDPR) et 
    //                              POSTER un message PosKeepingRequestMQueue
    // ► Utilisation côté SERVICE : après LECTURE d'un message PosKeepingRequestMQueue (IDPR) et 
    //                              ALIMENTER la classe POSREQUEST
    //────────────────────────────────────────────────────────────────────────────────────────────────
    #region PosRequest
    /// <summary>
    /// <para>────────────────────────────────────────────────────────────────────</para>
    /// <para>Classe de BASE matérialisant une demande de traitement</para> 
    /// <para>► tenue de position</para>
    /// <para>► traitement de fin de journée</para>
    /// <para>► clôture de journée</para>
    /// <para>etc... </para>
    /// <para>────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestClearingSPEC))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestClearingEOD))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestClearingBLK))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestCorrection))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestGroupLevel))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestEntry))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestEOD))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestREMOVEEOD))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestMaturityOffsetting))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestPhysicalPeriodicDelivery))]    
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestPositionOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestUpdateEntry))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestUnclearing))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestTransfer))]
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    public partial class PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("idPR")]
        public int idPR;
        [System.Xml.Serialization.XmlElementAttribute("idPR_PosRequest")]
        public int idPR_PosRequest;
        [System.Xml.Serialization.XmlElementAttribute("idPR_PosRequestSpecified")]
        public bool idPR_PosRequestSpecified;
        [System.Xml.Serialization.XmlElementAttribute("requestType")]
        public Cst.PosRequestTypeEnum requestType;
        [System.Xml.Serialization.XmlElementAttribute("requestMode")]
        public SettlSessIDEnum requestMode;

        [System.Xml.Serialization.XmlElementAttribute("idA_Entity")]
        public int idA_Entity;
        [System.Xml.Serialization.XmlElementAttribute("idA_CssCustodian")]
        public int idA_CssCustodian;
        [System.Xml.Serialization.XmlElementAttribute("idA_Css")]
        public int idA_Css;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_CssSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idA_Custodian")]
        public int idA_Custodian;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_CustodianSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idEM")]
        public int idEM;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idEMSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idCE")]
        public int idCE;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idCESpecified;
        [System.Xml.Serialization.XmlElementAttribute("dtBusiness")]
        public DateTime dtBusiness;
        [System.Xml.Serialization.XmlElementAttribute("posKeepingKey")]
        public PosKeepingKey posKeepingKey;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool posKeepingKeySpecified;
        [System.Xml.Serialization.XmlElementAttribute("idT")]
        public int idT;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idTSpecified;
        // EG 20170127 Qty Long To Decimal
        [System.Xml.Serialization.XmlElementAttribute("qty")]
        public decimal qty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool qtySpecified;
        [System.Xml.Serialization.XmlElementAttribute("notes")]
        public string notes;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool notesSpecified;
        #region PosRequestKeyIdentifier
        [System.Xml.Serialization.XmlElementAttribute("identifiers")]
        public PosRequestKeyIdentifier identifiers;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool identifiersSpecified;
        #endregion PosRequestKeyIdentifier
        [System.Xml.Serialization.XmlElementAttribute("idAIns")]
        public int idAIns;
        [System.Xml.Serialization.XmlElementAttribute("dtIns")]
        public DateTime dtIns;
        [System.Xml.Serialization.XmlElementAttribute("idAUpd")]
        public int idAUpd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAUpdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dtUpd")]
        public DateTime dtUpd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtUpdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("status")]
        public ProcessStateTools.StatusEnum status;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool statusSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idM;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idMSpecified;

        [System.Xml.Serialization.XmlElementAttribute("idProcessL")]
        public int idProcessL;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idProcessLSpecified;

        [System.Xml.Serialization.XmlElementAttribute("source")]
        public string source;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sourceSpecified;

        [System.Xml.Serialization.XmlElementAttribute("sourceIdProcessL")]
        public int sourceIdProcessL;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sourceIdProcessLSpecified;

        [System.Xml.Serialization.XmlElementAttribute("extlLink")]
        public string extlLink;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extlLinkSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IPosRequestTradeAdditionalInfo additionalInfo;

        [System.Xml.Serialization.XmlElementAttribute("groupProduct")]
        public ProductTools.GroupProductEnum groupProduct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool groupProductSpecified;

        #endregion Members
    }
    #endregion PosRequest
    #region PosRequestGroupLevel
    /// <summary>
    /// <para>────────────────────────────────────────────────────────────────────</para>
    /// <para>Utilisée par les traitements EOD et CLOSINGDAY pour regroupement</para>
    /// <para>────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Identifiant Entité/Marché</para>
    /// <para>  ● date business</para>
    /// <para></para>
    /// <para>► Exemple</para>
    /// <para>  ● EOD</para>
    /// <para>  ● EOD_UPDENTRY</para>
    /// <para>  ● EOD_MANUALOPTION</para>
    /// <para></para>
    /// <para>  ● CLOSINGDAY</para>
    /// <para>  ● CLOSINGDAY_CTRL</para>
    /// <para>  ● CLOSINGDAY_EO</para>D
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("groupLevel")]
    public partial class PosRequestGroupLevel : PosRequest
    {
    }
    #endregion PosRequestGroupLevel

    #region PosRequestKeyIdentifier
    /// <summary>
    /// <para>Stocke l'ensemble des IDENTIFIANTS de chaque élément pour construction de messages informatifs </para>
    /// <para>ou d'erreurs clairs </para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Messages d'envoi et de réponse de traitement</para>
    /// <para>► Messages d'alimentation des tables LOGS</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    /// EG 20130607 [18740] Add corporateAction (RemoveCAExecuted)
    public partial class PosRequestKeyIdentifier
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("entity")]
        public string entity;
        [System.Xml.Serialization.XmlElementAttribute("cssCustodian")]
        public string cssCustodian;
        [System.Xml.Serialization.XmlElementAttribute("market")]
        public string market;
        [System.Xml.Serialization.XmlElementAttribute("instrument")]
        public string instrument;
        [System.Xml.Serialization.XmlElementAttribute("asset")]
        public string asset;
        [System.Xml.Serialization.XmlElementAttribute("trade")]
        public string trade;
        [System.Xml.Serialization.XmlElementAttribute("dealer")]
        public string dealer;
        [System.Xml.Serialization.XmlElementAttribute("bookDealer")]
        public string bookDealer;
        [System.Xml.Serialization.XmlElementAttribute("clearer")]
        public string clearer;
        [System.Xml.Serialization.XmlElementAttribute("bookClearer")]
        public string bookClearer;
        [System.Xml.Serialization.XmlElementAttribute("corporateAction")]
        public string corporateAction;
        #endregion Members
    }
    #endregion PosRequestKeyIdentifier

    #region PosRequestClearingBLK
    /// <summary>
    /// <para>Utilisée par la COMPENSATION GLOBALE (REQUESTTYPE = CLEARBULK)</para>
    /// <para>────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Clé de position</para>
    /// <para>  ● Date business</para>
    /// <para>  ● Quantité à compenser</para>
    /// <para>► DETAIL (PosRequestDetClearingBLK)</para>
    /// <para>  ● Quantité en position à l'ACHAT</para>
    /// <para>  ● Quantité en position à la VENTE</para>
    /// <para>────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("clearing")]
    public partial class PosRequestClearingBLK : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetClearingBLK detail;
        #endregion Members
    }
    #endregion PosRequestClearingBLK
    #region PosRequestClearingEOD
    /// <summary>
    /// <para>Utilisée par la COMPENSATION AUTOMATIQUE DE FIN DE JOURNEE (REQUESTTYPE = CLEAREOD)</para>
    /// <para>──────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Clé de position</para>
    /// <para>  ● Date business</para>
    /// <para>● Quantité à compenser</para>
    /// <para>──────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("clearingEOD")]
    public partial class PosRequestClearingEOD : PosRequestClearingBLK
    {
    }
    #endregion PosRequestClearingEOD
    #region PosRequestClearingSPEC
    /// <summary>
    /// <para>Utilisée par la COMPENSATION/CLOTURE SPECIFIQUE (REQUESTTYPE = CLEARSPEC)</para>
    /// <para>─────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Clé de position</para>
    /// <para>  ● Date business</para>
    /// <para>  ● Quantité à compenser</para>
    /// <para>► DETAIL (PosRequestDetClearingSPEC)</para>
    /// <para>  ● Id de la clôturante/compensante</para>
    /// <para>  ● Id de la clôturée/compensée</para>
    /// <para>─────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("clearing")]
    public partial class PosRequestClearingSPEC : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetClearingSPEC detail;
        #endregion Members
    }
    #endregion PosRequestClearingSPEC
    #region PosRequestClosingDAY
    /// <summary>
    /// <para>Utilisée par la CLOTURE DE JOURNEE (REQUESTTYPE = CLOSINGDAY)</para>
    /// <para>──────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Date business</para>
    /// <para>──────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("closingDay")]
    public partial class PosRequestClosingDAY : PosRequest
    {
    }
    #endregion PosRequestClosingDAY
    #region PosRequestClosingDayControl
    /// <summary>
    /// <para>Utilisée par la CLOTURE DE JOURNEE (REQUESTTYPE = CLOSINGDAY_CTRL)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>Ligne de POSREQUEST toujours ENFANT (IDPR_POSREQUEST) d'une ligne POSREQUEST avec REQUESTTYPE = CLOSINGDAY</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Date business</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("closingDayControl")]
    public partial class PosRequestClosingDayControl : PosRequest
    {
    }
    #endregion PosRequestClosingDayControl
    #region PosRequestCorporateAction
    [System.Xml.Serialization.XmlRootAttribute("corporateAction")]
    public partial class PosRequestCorporateAction : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetCorporateAction detail;
        #endregion Members
    }
    #endregion PosRequestCorporateAction

    #region PosRequestCorrection
    /// <summary>
    /// <para>Utilisée par la CORRECTION DE POSITION (REQUESTTYPE = POC)</para>
    /// <para>──────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Date business</para>
    /// <para>  ● Quantité corrigée (supprimée)</para>
    /// <para>► DETAIL (PosRequestDetCorrection)</para>
    /// <para>  ● Quantité disponible</para>
    /// <para>● Restitution des frais sur la base de la quantité supprimée</para>
    /// <para>──────────────────────────────────────────────────────────────</para>
    /// </summary>
    public partial class PosRequestCorrection : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetCorrection detail;
        #endregion Members
    }
    #endregion PosRequestCorrection

    #region PosRequestEntry
    /// <summary>
    /// <para>Utilisée pour le traitement d'entrée d'allocation en portefeuille (REQUESTTYPE = ENTRY)</para>
    /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Date business</para>
    /// <para>  ● Id du trade entrant</para>
    /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("entry")]
    public partial class PosRequestEntry : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetEntry detail;
        #endregion Members
    }
    #endregion PosRequestEntry
    #region PosRequestEOD
    /// <summary>
    /// <para>Utilisé pour le TRAITEMENT DE FIN DE JOURNEE (REQUESTTYPE = EOD)</para>
    /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entity</para>
    /// <para>  ● Id Clearing House</para>
    /// <para>  ● Date business</para>
    /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("endOfDay")]
    public partial class PosRequestEOD : PosRequest
    {
    }
    #endregion PosRequestEOD
    #region PosRequestREMOVEEOD
    /// <summary>
    /// <para>Utilisé pour l'annulation d'un TRAITEMENT DE FIN DE JOURNEE (REQUESTTYPE = REMOVEEOD)</para>
    /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entity</para>
    /// <para>  ● Id Clearing House</para>
    /// <para>  ● Date business</para>
    /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("removeEndOfDay")]
    public partial class PosRequestREMOVEEOD : PosRequest
    {
    }
    #endregion PosRequestREMOVEEOD
    #region PosRequestCascadingShifting
    /// <summary>
    /// <para>Type de POSREQUEST utilisé pour le traitement de cascading shifting à l'échéance
    /// <para>──────────────────────────────────────────────────────────────────────────────────────
    /// <para>► Héritage de POSREQUEST
    /// <para>  ● Id Entité/Marché
    /// <para>  ● Clé de position
    /// <para>  ● Date business
    /// <para>► DETAIL (PosRequestDetCascadingShifting)</para>
    /// <para>  Caractéristiques nécessaire à la génération de la nouvelle position</para>
    /// <para>──────────────────────────────────────────────────────────────────────────────────────
    /// </summary>
    // PM 20130213 [18414]
    [System.Xml.Serialization.XmlRootAttribute("cascadeShift")]
    public partial class PosRequestCascadingShifting : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetCascadingShifting detail;
        #endregion Members
    }
    #endregion PosRequestCascadingShifting
    #region PosRequestMaturityOffsetting
    /// <summary>
    /// <para>Type de POSREQUEST utilisé pour le traitement clôture à l'échéance (Future et option)</para>
    /// <para>──────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Clé de position</para>
    /// <para>  ● Date business</para>
    /// <para>──────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("maturityOfs")]
    public partial class PosRequestMaturityOffsetting : PosRequest
    {
    }
    #endregion PosRequestMaturityOffsetting
    #region PosRequestOption
    /// <summary>
    /// <para>Utilisée par le DENOUEMENT D'OPTIONS (REQUESTTYPE = ABN, NEX, NAS, AAB, AAS, ASS, AEX, EXE)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Date business</para>
    /// <para>  ● Id du trade</para>
    /// <para>  ● Quantité à dénouer</para>
    /// <para>► DETAIL (PosRequestDetOption)</para>
    /// <para>  Caractéristiques nécessaire au dénouement</para>
    /// <para>  ● Quantité disponible</para>
    /// <para>  ● Strike</para>
    /// <para>  ● Sous-jacent, etc...</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("option")]
    // EG 20160121 [21805] POC-MUREX Add IDTOPTION 
    public partial class PosRequestOption : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetOption detail;
        [System.Xml.Serialization.XmlElementAttribute("idTOption")]
        public int idTOption;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idTOptionSpecified;
        #endregion Members
    }
    #endregion PosRequestOption

    #region PosRequestPhysicalDeliveryPayment
    /// <summary>
    /// <para>Type de POSREQUEST utilisé pour le traitement des payments à la livraison des sous-jacents commodities (Future)
    /// <para>──────────────────────────────────────────────────────────────────────────────────────
    /// <para>► Héritage de POSREQUEST
    /// <para>  ● Id Entité/Marché
    /// <para>  ● Clé de position
    /// <para>  ● Date business
    /// <para>──────────────────────────────────────────────────────────────────────────────────────
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("physicalPeriodicDelivery")]
    public partial class PosRequestPhysicalPeriodicDelivery : PosRequest
    {
    }
    #endregion PosRequestPhysicalPeriodicDelivery

    #region PosRequestPositionOption
    /// <summary>
    /// <para>Utilisée par le DENOUEMENT D'OPTIONS par position </para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Date business</para>
    /// <para>  ● Id du trade</para>
    /// <para>  ● Quantité à dénouer</para>
    /// <para>► DETAIL (PosRequestDetPositionOption)</para>
    /// <para>  Caractéristiques nécessaire au dénouement</para>
    /// <para>  ● Avec calcul de frais</para>
    /// <para>  ● Denouement partiel autorisé</para>
    /// <para>  ● Abandon de la quantité restante</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    /// FI 20130315 [18467] add PosRequestPositionOption
    [System.Xml.Serialization.XmlRootAttribute("option")]
    public partial class PosRequestPositionOption : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetPositionOption detail;
        #endregion Members
    }
    #endregion PosRequestPositionOption

    #region PosRequestRemoveCAExecuted
    /// <summary>
    /// <para>Utilisé pour l'annulation d'un traitement de CA (REQUESTTYPE = RMVCAEXECUTED)</para>
    /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Clearing House</para>
    /// <para>  ● Id Market</para>
    /// <para>  ● Id Coporate Event</para>
    /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    /// EG 20130607 [18740] Add RemoveCAExecuted
    [System.Xml.Serialization.XmlRootAttribute("removeCAExecuted")]
    public partial class PosRequestRemoveCAExecuted : PosRequest
    {
    }
    #endregion PosRequestRemoveCAExecuted

    #region PosRequestRemoveAlloc
    /// <summary>
    /// <para>Utilisée par l'annulation d'ALLOCATION (REQUESTTYPE = RMVALLOC)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Date business</para>
    /// <para>  ● Id du trade source</para>
    /// <para>► DETAIL (PosRequestDetRemoveAlloc)</para>
    /// <para>  Caractéristiques du trade annulé</para>
    /// <para>  ● Quantité initiale</para>
    /// <para>  ● Quantité en position</para>
    /// <para>  Caractéristiques du trade matérialisant la remplaçante</para>
    /// <para>  ● Id et identifiant du trade remplacant</para>
    ///───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("removeAlloc")]
    public partial class PosRequestRemoveAlloc : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetRemoveAlloc detail;
        #endregion Members
    }
    #endregion PosRequestRemoveAlloc

    #region PosRequestSplit
    /// <summary>
    /// <para>Utilisée par le TRADESPLITTING (REQUESTTYPE = TRADESPLITTING)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Date business</para>
    /// <para>  ● Id du trade source</para>
    /// <para>► DETAIL (PosRequestDetSplit)</para>
    /// <para>  Caractéristiques des trades nés du Splitting de trade </para>
    /// <para>  ● Dealer, Quantité</para>
    ///───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("split")]
    public partial class PosRequestSplit : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetSplit detail;
        #endregion Members
    }
    #endregion PosRequestSplit

    #region PosRequestTransfer
    /// <summary>
    /// <para>Utilisée par le TRANSFERT DE POSITION (REQUESTTYPE = POT)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Date business</para>
    /// <para>  ● Id du trade source</para>
    /// <para>  ● Quantité à transferer</para>
    /// <para>► DETAIL (PosRequestDetTransfer)</para>
    /// <para>  Caractéristiques du trade à transférer nécessaires au transfert et à la décompensation éventuelle</para>
    /// <para>  ● Quantité initiale</para>
    /// <para>  ● Quantité en position</para>
    /// <para>  ● Caractéristiques des frais restitués</para>
    /// <para>  Caractéristiques du trade matérialisant le transfert</para>
    /// <para>  ● Id et identifiant du trade cible (nouveau trade matérialisant le transfert)</para>
    /// <para>  ● Id du nouvel acteur donneur d'ordre et de son book</para>
    /// <para>  ● Id du nouvel acteur compensateur et de son book</para>
    ///───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("transfer")]
    public partial class PosRequestTransfer : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetTransfer detail;
        #endregion Members
    }
    #endregion PosRequestTransfer

    #region PosRequestUnclearing
    /// <summary>
    /// <para>Utilisée par la DECOMPENSATION (REQUESTTYPE = UNCLEARING)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Date business</para>
    /// <para>  ● Id du trade à décompenser</para>
    /// <para>  ● Quantité à décompenser</para>
    /// <para>► DETAIL (PosRequestDetUnclearing)</para>
    /// <para>  Caractéristiques nécessaires à la décompensation</para>
    /// <para>  ● Id du POSREQUEST à l'origine de la compensation</para>
    /// <para>  ● Id du POSACTIONDET à décompenser</para>
    /// <para>  ● Id de la négociation clôturante</para>
    /// <para>  ● Date business de la compensation</para>
    /// <para>  ● Quantité compensée</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("unclearing")]
    public partial class PosRequestUnclearing : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetUnclearing detail;
        #endregion Members
    }
    #endregion PosRequestUnclearing
    #region PosRequestUpdateEntry
    /// <summary>
    /// <para>Utilisée par la MISE A JOUR DES CLOTURES (REQUESTTYPE = UPDENTRY)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Clé de position</para>
    /// <para>  ● Date business</para>
    /// <para>► DETAIL (PosRequestDetUnclearing)</para>
    /// <para>  Caractéristiques nécessaires à la clôture</para>
    /// <para>  ● DtBusiness du trade</para>
    /// <para>  ● Sens du trade</para>
    /// <para>  ● Quantité initiale du trade</para>
    /// <para>  ● Horodatage du trade</para>
    /// <para>  ● Position effect du trade</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("updateEntry")]
    public partial class PosRequestUpdateEntry : PosRequest
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public PosRequestDetUpdateEntry detail;
        #endregion Members
    }
    #endregion PosRequestUpdateEntry

    #region PosRequestDetail
    /// <summary>
    /// <para>Matérialise le détail d'une demande de traitement de traitement de tenue de position</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Côté CLIENT  pour sérialiser  les caractéristiques d'une demande POSREQUEST</para>
    /// <para>► Côté SERVICE pour désérialiser et traiter les caractéristiques d'une demande POSREQUEST</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestDetClearingSPEC))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestDetClearingBLK))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestDetEntry))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestDetOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestDetPositionOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestDetCorrection))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestDetUnclearing))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestDetUpdateEntry))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestDetTransfer))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PosRequestDetRemoveAlloc))]

    [System.Xml.Serialization.XmlRootAttribute("detail")]
    public abstract partial class PosRequestDetail
    {
        #region Members
        //Version du document
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EfsMLDocumentVersionEnum EfsMLversion;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int nbAdditionalEvents;
        #endregion Members
    }
    #endregion PosRequestDetail
    #region PosRequestDetCorporateAction
    /// <summary>
    /// <para>Détail pour le trade ajusté suite à CA
    /// </summary>
    // EG 20130417
    /// EG 20140516 [19816] Add contractSymbolDCEx
    public partial class PosRequestDetCorporateAction : PosRequestDetail
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idDCEx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string identifierDCEx;
        /// EG 20140516 [19816] Add contractSymbolDCEx
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string contractSymbolDCEx;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DataDocumentContainer dataDocument;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime businessDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IPosRequestTradeAdditionalInfo additionalInfo;
        #endregion Members
    }
    #endregion PosRequestDetCorporateAction

    #region PosRequestDetCascadingShifting
    /// <summary>
    /// <para>Détail pour un CASCADING ou SHIFTING (REQUESTTYPE = CAS, SHI)</para>
    /// </summary>
    // PM 20130219 [18414] & PM 20130307 [18434]
    // EG 20171107 [23509] dtExecution
    public partial class PosRequestDetCascadingShifting : PosRequestDetail
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idDCDest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string identifierDCDest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string maturityMonthYearDest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DataDocumentContainer dataDocument;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime dtExecution;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IPosRequestTradeAdditionalInfo additionalInfo;
        #endregion Members
    }
    #endregion PosRequestDetCascadingShifting
    #region PosRequestDetClearingBLK
    /// <summary>
    /// <para>Détzail d'une demande de COMPENSATION GLOBALE (REQUESTTYPE = CLEARBULK)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques</para>
    /// <para>  ● Quantité en position à l'ACHAT</para>
    /// <para>  ● Quantité en position à la VENTE</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("clearing", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public partial class PosRequestDetClearingBLK : PosRequestDetail
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("availableQtyBuy")]
        public decimal availableQtyBuy;
        [System.Xml.Serialization.XmlElementAttribute("availableQtySell")]
        public decimal availableQtySell;
        #endregion Members
    }
    #endregion PosRequestDetClearingBLK
    #region PosRequestDetClearingSPEC
    /// <summary>
    /// <para>Détail d'une demande de COMPENSATION SPECIFIQUE (REQUESTTYPE = CLEARSPEC)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques</para>
    /// <para>  ● Id du trade à compenser</para>
    /// <para>  ● Quantité disponible du trade à compenser</para>
    /// <para>  ● Id du trade compensant</para>
    /// <para>  ● Quantité disponible du trade compensant</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("clearing", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class PosRequestDetClearingSPEC : PosRequestDetail
    {
        #region Members
        public PosKeepingClearingTrade[] tradesTarget;
        #endregion Members
    }
    #endregion PosRequestDetClearingSPEC
    #region PosRequestDetCorrection
    /// <summary>
    /// <para>Détail d'une demande de CORRECTION DE POSITION (REQUESTTYPE = POC)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques</para>
    /// <para>  ● Quantité disponible</para>
    /// <para>  ● Restitution des frais</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("correction", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public partial class PosRequestDetCorrection : PosRequestDetail
    {
        #region Members
        // EG 20150907 [21317] Add InitialQty
        [System.Xml.Serialization.XmlElementAttribute("initialQty")]
        public decimal initialQty;
        [System.Xml.Serialization.XmlElementAttribute("availableQty")]
        public decimal availableQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentFeesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentFees")]
        public Payment[] paymentFees;
        /// EG 20150716 [21103]
        [System.Xml.Serialization.XmlElementAttribute("isReversalSafekeeping")]
        public bool isReversalSafekeeping;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isReversalSafekeepingSpecified;
        #endregion Members
    }
    #endregion PosRequestDetCorrection
    #region PosRequestDetEntry
    /// <summary>
    /// <para>Détail pour une entrée en Portefeuille (REQUESTTYPE = ENTRY)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques</para>
    /// <para>  ● DtBusiness du trade</para>
    /// <para>  ● Sens du trade</para>
    /// <para>  ● Quantité initiale du trade</para>
    /// <para>  ● Horodatage du trade</para>
    /// <para>  ● Position effect du trade</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("entry", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
    public partial class PosRequestDetEntry : PosRequestDetail
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("dtBusiness")]
        public DateTime dtBusiness;
        [System.Xml.Serialization.XmlElementAttribute("side")]
        public int side;
        [System.Xml.Serialization.XmlElementAttribute("qty")]
        public decimal qty;
        [System.Xml.Serialization.XmlElementAttribute("dtExecution")]
        public DateTime dtExecution;
        [System.Xml.Serialization.XmlElementAttribute("positionEffect")]
        public string positionEffect;
        [System.Xml.Serialization.XmlElementAttribute("message")]
        public PosKeepingEntryMQueue message;
        #endregion Members
    }
    #endregion PosRequestDetEntry
    #region PosRequestDetOption
    /// <summary>
    /// <para>Détail pour un DENOUEMENT D'OPTIONS (REQUESTTYPE = ABN, NEX, NAS, EXE, AAB, AAS, ASS)</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques</para>
    /// <para>  ● Quantité disponible</para>
    /// <para>  ● Sens du trade</para>
    /// <para>  ● Quantité initiale du trade</para>
    /// <para>  ● Horodatage du trade</para>
    /// <para>  ● Position effect du trade</para>
    /// <para>► Détermination ITL / ATM / OTM</para>/// 
    /// <para>  CALL</para>/// 
    /// <para>  ● InTheMoney = (STRIKE less than    UNDERLYERPRICE)</para>
    /// <para>  ● AtTheMoney = (STRIKE equal to     UNDERLYERPRICE)</para>
    /// <para>  ● OnTheMoney = (STRIKE greater than UNDERLYERPRICE)</para>
    /// <para></para>/// 
    /// <para>  PUT</para>/// 
    /// <para>  ● InTheMoney = (STRIKE greater than UNDERLYERPRICE)</para>
    /// <para>  ● AtTheMoney = (STRIKE equal to     UNDERLYERPRICE)</para>
    /// <para>  ● OnTheMoney = (STRIKE less than    UNDERLYERPRICE)</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("option", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20151019 [21465] Add feeCalculationSpecified|feeCalculation
    // EG 20170127 Qty Long To Decimal
    public partial class PosRequestDetOption : PosRequestDetail
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("availableQty")]
        public decimal availableQty;
        [System.Xml.Serialization.XmlElementAttribute("strikePrice")]
        public decimal strikePrice;
        [System.Xml.Serialization.XmlElementAttribute("underlyer")]
        public PosRequestDetUnderlyer underlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool underlyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentFees")]
        public Payment[] paymentFees;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentFeesSpecified;

        // EG 20151019 [21465] New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feeCalculationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeCalculation")]
        public bool feeCalculation;
        #endregion Members
    }
    #endregion PosRequestDetOption

    #region PosRequestDetPositionOption
    /// <summary>
    /// 
    /// </summary>
    /// FI 20130315 [18467] add PosRequestDetPositionOption
    [System.Xml.Serialization.XmlRootAttribute("option", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class PosRequestDetPositionOption : PosRequestDetail
    {
        /// <summary>
        /// Obtient définit le mode (New, Update, RemoveOnly) du denouement d'option
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("captureMode")]
        public Cst.Capture.ModeEnum captureMode;
        
        /// <summary>
        /// Obtient définit un flag qui autorise l'exécution partielle du denouement d'option
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("partialExecutionAllowed")]
        public Boolean partialExecutionAllowed;

        // RD 20200120 [25114] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feeCalculationSpecified;
        /// <summary>
        /// Obtient définit un flag qui définie si les denouements d'option par trade générés appliquent les barêmes paramétrés ds Spheres®
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("feeCalculation")]
        public Boolean feeCalculation;

        /// <summary>
        /// Obtient définit un flag qui définie si Spheres® doit abandonner les quantités restantes dans le cadre d'un exercice
        /// <para>L'abandon s'applique uniquement sur le dernier trade exercé</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("abandonRemainingQty")]
        public Boolean abandonRemainingQty;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentFeesSpecified;
        /// <summary>
        /// Représente les frais applicables sur la position
        /// <para>FI 20130315 Non géré pour l'instant</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("paymentFees")]
        public Payment[] paymentFees;

    }
    #endregion PosRequestDetPositionOption

    #region PosRequestDetRemoveAlloc
    /// <summary>
    /// <para>Détail pour une annulation d'allocation (REQUESTTYPE = RMVALLOC)</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques du trade annulé</para>
    /// <para>  ● Quantité initiale</para>
    /// <para>  ● Quantité en position au moment de la demande d'annulation</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("remove", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public partial class PosRequestDetRemoveAlloc : PosRequestDetail
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("initialQty")]
        public decimal initialQty;
        [System.Xml.Serialization.XmlElementAttribute("positionQty")]
        public decimal positionQty;
        #endregion Members
    }
    #endregion PosRequestDetRemoveAlloc
    #region PosRequestDetSplit
    /// <summary>
    /// <para>Détail pour un TRADESPLITTING (REQUESTTYPE = TRADESPLITTING)</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques des trades nés du split</para>
    /// <para>  ● splitTrade</para> 
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("split", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class PosRequestDetSplit : PosRequestDetail
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("splitTrade")]
        public SplitNewTrade[] splitNewTrades;
        #endregion Members
    }
    #endregion PosRequestDetSplit
    #region PosRequestDetTransfer
    /// <summary>
    /// <para>Détail pour un TRANSFERT DE POSITION (REQUESTTYPE = POT)</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques du trade à transférer nécessaires au transfert et à la décompensation éventuelle</para>
    /// <para>  ● Quantité initiale</para>
    /// <para>  ● Quantité en position</para>
    /// <para>  ● Caractéristiques des frais restitués</para>
    /// <para>► Caractéristiques du trade matérialisant le transfert</para>
    /// <para>  ● Id et identifiant du trade cible (nouveau trade matérialisant le transfert)</para>
    /// <para>  ● Id du nouvel acteur donneur d'ordre et de son book</para>
    /// <para>  ● Id du nouvel acteur compensateur et de son book</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    /// EG 20141210 [20554] Add idTReplaceSpecified and trade_IdentifierReplaceSpecified
    /// EG 20150716 (21103] Add isReversalSafekeeping
    [System.Xml.Serialization.XmlRootAttribute("transfer", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public partial class PosRequestDetTransfer : PosRequestDetail
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("initialQty")]
        public decimal initialQty;
        [System.Xml.Serialization.XmlElementAttribute("positionQty")]
        public decimal positionQty;
        [System.Xml.Serialization.XmlElementAttribute("dealerTarget")]
        public Party dealerTarget;
        [System.Xml.Serialization.XmlElementAttribute("dealerBookIdTarget")]
        public BookId dealerBookIdTarget;
        [System.Xml.Serialization.XmlElementAttribute("clearerTarget")]
        public Party clearerTarget;
        [System.Xml.Serialization.XmlElementAttribute("clearerBookIdTarget")]
        public BookId clearerBookIdTarget;
        [System.Xml.Serialization.XmlElementAttribute("paymentFees")]
        public Payment[] paymentFees;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentFeesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idTReplace")]
        public int idTReplace;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idTReplaceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("trade_IdentifierReplace")]
        public string trade_IdentifierReplace;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool trade_IdentifierReplaceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("isReversalFees")]
        public bool isReversalFees;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isReversalFeesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("isCalcNewFees")]
        public bool isCalcNewFees;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isCalcNewFeesSpecified;
        /// EG 20150716 [21103]
        [System.Xml.Serialization.XmlElementAttribute("isReversalSafekeeping")]
        public bool isReversalSafekeeping;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isReversalSafekeepingSpecified;

        #endregion Members
    }
    #endregion PosRequestDetTransfer
    #region PosRequestDetUnclearing
    /// <summary>
    /// <para>Détail pour une DECOMPENSATION UNITAIRE (REQUESTTYPE = UNCLEARING)</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques de la clôture/compensation à décompensée</para>
    /// <para>  ● Origine (REQUESTTYPE)</para>
    /// <para>  ● Id du traitement (IDPR)</para>
    /// <para>  ● Id de la clôture (IDPADET)</para>
    /// <para>  ● Id du trade clôturant</para> 
    /// <para>  ● Date business</para> 
    /// <para>  ● Quantité clôturée</para> 
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("unclearing", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public partial class PosRequestDetUnclearing : PosRequestDetail
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("requestType")]
        public Cst.PosRequestTypeEnum requestType;
        [System.Xml.Serialization.XmlElementAttribute("idPR")]
        public int idPR;
        [System.Xml.Serialization.XmlElementAttribute("idPADET")]
        public int idPADET;
        [System.Xml.Serialization.XmlElementAttribute("idT_Closing")]
        public int idT_Closing;
        [System.Xml.Serialization.XmlElementAttribute("closing_Identifier")]
        public string closing_Identifier;
        [System.Xml.Serialization.XmlElementAttribute("dtBusiness")]
        public DateTime dtBusiness;
        [System.Xml.Serialization.XmlElementAttribute("closingQty")]
        public decimal closingQty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idT_DeliverySpecified;
        [System.Xml.Serialization.XmlElementAttribute("idT_Delivery")]
        public int idT_Delivery;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool delivery_IdentifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("delivery_Identifier")]
        public string delivery_Identifier;
        #endregion Members
    }
    #endregion PosRequestDetUnclearing
    #region PosRequestSource
    // EG 20151102 [21465] New
    // EG 20170127 Qty Long To Decimal
    public partial class PosRequestSource
    {
        [System.Xml.Serialization.XmlAttributeAttribute("idPR")]
        public int idPR;
        [System.Xml.Serialization.XmlAttributeAttribute("qty")]
        public decimal qty;
    }
    #endregion PosRequestSource
    #region PosRequestDetUnderlyer
    /// <summary>
    /// <para>Détail d'un actif sous-jacent d'une option</para>
    /// <para>Utilisé sdans le cas des dénouements d'options</para>
    /// </summary>
    // EG 20151019 [21465] Add new quoteTimingSpecified|priceSpecified|dtPriceSpecified
    // EG 20151102 [21465] 
    public partial class PosRequestDetUnderlyer
    {
        #region Members
        // EG 20151102 [21465] New
        [System.Xml.Serialization.XmlElementAttribute("posRequestSource")]
        public PosRequestSource[] posRequestSource;
        //[System.Xml.Serialization.XmlAttributeAttribute("idPRSource")]
        //public int idPRSource;
        [System.Xml.Serialization.XmlAttributeAttribute("requestTypeSource")]
        public Cst.PosRequestTypeEnum requestTypeSource;
        [System.Xml.Serialization.XmlAttributeAttribute("idTsource")]
        public int idTSource;
        [System.Xml.Serialization.XmlAttributeAttribute("assetCategory")]
        public Cst.UnderlyingAsset assetCategory;
        [System.Xml.Serialization.XmlAttributeAttribute("idAsset")]
        public int idAsset;
        [System.Xml.Serialization.XmlElementAttribute("identifier")]
        public string identifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteTimingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quoteTiming")]
        public QuoteTimingEnum quoteTiming;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool priceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("value")]
        public decimal price;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("time")]
        public DateTime dtPrice;
        [System.Xml.Serialization.XmlElementAttribute("source")]
        public string source;
        [System.Xml.Serialization.XmlAttributeAttribute("idI")]
        public int idI;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DataDocumentContainer dataDocument;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IPosRequestTradeAdditionalInfo additionalInfo;
        #endregion Members
    }
    #endregion PosRequestDetUnderlyer
    #region PosRequestDetUpdateEntry
    /// <summary>
    /// <para>Détail pour une MISE A JOUR DE CLOTURES (REQUESTTYPE = UPDENTRY)</para>
    /// <para>──────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques nécessaires à la clôture</para>
    /// <para>  ● DtBusiness du trade</para>
    /// <para>  ● Sens du trade</para>
    /// <para>  ● Quantité initiale du trade</para>
    /// <para>  ● Horodatage du trade</para>
    /// <para>  ● Position effect du trade</para>
    /// <para>──────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("entry", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    // EG 20171024 [23509] Upd dtExecution replace dtTimestamp
    public partial class PosRequestDetUpdateEntry : PosRequestDetail
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("dtBusiness")]
        public DateTime dtBusiness;
        [System.Xml.Serialization.XmlElementAttribute("side")]
        public int side;
        [System.Xml.Serialization.XmlElementAttribute("qty")]
        public decimal qty;
        [System.Xml.Serialization.XmlElementAttribute("dtExecution")]
        public DateTime dtExecution;
        [System.Xml.Serialization.XmlElementAttribute("positionEffect")]
        public string positionEffect;
        #endregion Members
    }
    #endregion PosRequestDetUpdateEntry

    #region PosRequestAdditionalInfo
    // EG 20141230 [20587]
    public partial class PosRequestTradeAdditionalInfo
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DataDocumentContainer templateDataDocument;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_TradeCommon sqlTemplateTrade;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_Product sqlProduct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_Instrument sqlInstrument;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string screenName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Cst.StatusActivation stActivation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string displayName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string description;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string extlLink;
        #endregion Members
    }
    #endregion PosRequestTradeAdditionalInfo

    #region PosRequestTradeExAdditionalInfo
    /// <summary>
    /// <para>Complément d'infos pour l'enregistrement du trade ajusté suite à CA </para>
    /// </summary>
    public partial class PosRequestTradeExAdditionalInfo : PosRequestTradeAdditionalInfo
    {
    }
    #endregion PosRequestTradeExAdditionalInfo

    #region PosRequestUnderlyerAdditionalInfo
    /// <summary>
    /// <para>Complément d'infos pour l'enregistrement du trade</para>
    /// <para>Utilisé dans le cas des dénouements d'options (LIVRAISON PHYSIQUE)</para>
    /// </summary>
    public partial class PosRequestUnderlyerAdditionalInfo : PosRequestTradeAdditionalInfo
    {
    }
    #endregion PosRequestUnderlyerAdditionalInfo

    #region PosRequestPositionDocument
    /// <summary>
    ///  Représente un document qui contient les caractéristiques d'une demande d'action sur position
    ///  <para>la demande s'applique uniquement sur une position </para>
    ///  <para>la demande ne s'applique pas à un trade</para>
    /// </summary>
    /// FI 20130322[18467] L'idéal aurait été qu'il soit de type PositionMaintenanceReport(tag PosMntRpt sous FIXML)   
    /// http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/body_55506577.html
    /// Faute de temps la structure un EFS propriétaire
    /// Les frais ne sont pas gérés dans cette version (de toute façon non prévu dans PosMntRpt sous FixML)
    /// Le jour où les frais devront être gérés il faudra prévoir la parésence  d'un membre party de type Party[]
    [System.Xml.Serialization.XmlRootAttribute("PosRequestPosition", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class PosRequestPositionDocument
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EfsMLDocumentVersionEnum EfsMLversion;

        [System.Xml.Serialization.XmlElementAttribute("requestType")]
        public Cst.PosRequestTypeEnum requestType;

        [System.Xml.Serialization.XmlElementAttribute("requestMode")]
        public SettlSessIDEnum requestMode;

        [System.Xml.Serialization.XmlElementAttribute("clearingBusinessDate")]
        public EFS_Date clearingBusinessDate;

        [System.Xml.Serialization.XmlElementAttribute("actorEntity")]
        public ActorId actorEntity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorDealerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("actorDealer")]
        public ActorId actorDealer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bookDealerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bookDealer")]
        public BookId bookDealer;

        [System.Xml.Serialization.XmlElementAttribute("actorClearer")]
        public ActorId actorClearer;

        [System.Xml.Serialization.XmlElementAttribute("bookClearer")]
        public BookId bookClearer;

        [System.Xml.Serialization.XmlElementAttribute("qty")]
        // EG 20170127 Qty Long To Decimal
        public EFS_Decimal qty;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool notesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notes")]
        public EFS_String notes;

        /// <summary>
        /// Représente l'asset
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("Instrmt", Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
        public InstrumentBlock Instrmt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentFeesSpecified;
        /// <summary>
        /// Représente les frais associés à l'action
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("paymentFees")]
        public Payment[] paymentFees;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("isPartialExecutionAllowed")]
        public EFS_Boolean isPartialExecutionAllowed;
        /// <summary>
        /// Flag qui permet le calcul des frais sur les trades impliqués par l'action
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("isFeeCalculation")]
        public EFS_Boolean isFeeCalculation;
        /// <summary>
        /// Flag utilisée uniquement pour les exercices sur poitions options
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("isAbandonRemainingQty")]
        public EFS_Boolean isAbandonRemainingQty;

    }
    #endregion PosRequestPositionDocument
}

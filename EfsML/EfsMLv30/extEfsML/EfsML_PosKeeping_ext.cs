#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.Doc;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1;
using FixML.v50SP1.Enum;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Tz = EFS.TimeZone;
#endregion using directives

/// EG 20150302 CFD Forex Add PosKeepingAsset_FXRATE
namespace EfsML.v30.PosRequest
{
    #region PosKeepingAsset
    public abstract partial class PosKeepingAsset : IPosKeepingAsset
    {
        #region Constructors
        public PosKeepingAsset() { }
        #endregion Constructors
        #region Methods
        #region CashFlowValorization
        /// <summary>
        /// Calcul du montant d'un Cash Flow ((pPrice1 - pPrice2) * pContractMultiplier * pQuantity)
        /// <para>Pas d'application d'arrondi avant d'appliquer la quantité de lot</para>
        /// </summary>
        /// <param name="pPrice1"></param>
        /// <param name="pPrice2"></param>
        /// <param name="pContractMultiplier"></param>
        /// <param name="pQuantity"></param>
        /// <returns></returns>
        ///PM 20150129 [20737][20754] Ajout du paramètre pPrice2
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public virtual decimal CashFlowValorization(decimal pPrice1, decimal pPrice2, decimal pContractMultiplier, decimal pQuantity)
        {
            //PM 20141120 [20508] Appel de la méthode CashFlowValorization sans effectuer de calcul unitaire
            //return ExchangeTradedDerivativeTools.CashFlowValorization(pPrice, pContractMultiplier, pQuantity, default, 0);
            //PM 20150129 [20737][20754] Modification du calcul unitaire
            //return ExchangeTradedDerivativeTools.CashFlowValorization(pPrice, pContractMultiplier, pQuantity);
            return ExchangeTradedDerivativeTools.CashFlowValorization(pPrice1, pPrice2, pContractMultiplier, pQuantity);
        }
        #endregion CashFlowValorization

        #region CashSettlement
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20151001 [21414] Add Nullable to ClosingPrice and return 
        // EG 20170127 Qty Long To Decimal
        public virtual Nullable<decimal> CashSettlement(decimal pStrikePrice, Nullable<decimal> pClosingPrice, decimal pQuantity)
        {
            return null;
        }
        #endregion CashSettlement
        #region NominalValue
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public virtual decimal NominalValue(decimal pQuantity)
        {
            return 0;
        }
        #endregion NominalValue
        #region RealizedMargin
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public virtual Nullable<decimal> RealizedMargin(Nullable<decimal> pClosedPrice, Nullable<decimal> pClosingPrice, decimal pQuantity)
        {
            Nullable<decimal> realizedMargin = null;
            if (pClosedPrice.HasValue && pClosingPrice.HasValue)
                realizedMargin = (pClosedPrice - pClosingPrice) * pQuantity * contractMultiplier;
            return realizedMargin;
        }
        #endregion RealizedMargin
        #region ToBase100
        public Nullable<decimal> ToBase100(Nullable<decimal> pValue)
        {
            if (pValue.HasValue)
                return ToBase100(pValue.Value);
            else
                return pValue;
        }
        public virtual decimal ToBase100(decimal pValue)
        {
            return 0;
        }
        public Nullable<decimal> ToBase100_UNL(Nullable<decimal> pValue)
        {
            if (pValue.HasValue)
                return ToBase100_UNL(pValue.Value);
            else
                return pValue;
        }
        public virtual decimal ToBase100_UNL(decimal pValue)
        {
            return 0;
        }
        #endregion ToBase100
        #region VariableContractValue
        public virtual decimal VariableContractValue(decimal pPrice)
        {
            return 0;
        }
        public decimal? VariableContractValue(decimal? pPrice)
        {
            return pPrice.HasValue ? VariableContractValue(pPrice.Value) : pPrice;
        }
        public virtual decimal VariableContractValue_UNL(decimal pPrice)
        {
            return 0;
        }
        public decimal? VariableContractValue_UNL(decimal? pPrice)
        {
            return pPrice.HasValue ? VariableContractValue_UNL(pPrice.Value) : pPrice;
        }
        #endregion VariableContractValue

        #region Set
        //PL 20170411 [23064] Modify
        public virtual void Set(DateTime pMaturityDate, string pCategory, decimal pNominal, string pCurrency,
                        decimal pStrikePrice, decimal pContractMultiplier,
                        int pInstrumentNum, int pInstrumentDen, string pIdBC, string pPutCall,
                        string pAssetCategory, decimal pFactor,
                        DateTime pDeliveryDate, SettlMethodEnum pSettltMethod,PhysicalSettlementAmountEnum pPhysicalSettlementAmount,
                        IOffset pDeliveryDelayOffset, FuturesValuationMethodEnum pValuationMethod)
        {
        }
        #endregion Set
        #region SetQuote
        /// <summary>
        /// Alimentation du prix de clôture (OfficialClose) de l'asset ou du sous-jacent
        /// </summary>
        /// <param name="pCategory">Category</param>
        /// <param name="pIdAsset">Id</param>
        /// <param name="pIdentifier">Identifiant</param>
        /// <param name="pTime">Date</param>
        /// <param name="pQuotePrice">Prix</param>
        /// <param name="pSource">Source</param>
        /// EG 20140120 Report 3.7
        public void SetQuote(Cst.UnderlyingAsset pCategory, Quote pQuote, string pSource)
        {
            quoteSpecified = pQuote.valueSpecified;
            if (quoteSpecified)
                quote = new PosKeepingQuote(pCategory, pQuote.idAsset, pQuote.idAsset_Identifier,
                    pQuote.QuoteSide.Value, pQuote.QuoteTiming.Value, pQuote.time, pQuote.value, pSource);
        }
        #endregion SetQuote
        #region SetQuoteReference
        /// <summary>
        /// Alimentation du prix de référence (OfficialSettlement) de l'asset ou du sous-jacent
        /// </summary>
        /// <param name="pCategory">Category</param>
        /// <param name="pQuote">Quotation</param>
        /// <param name="pSource">Source</param>
        public void SetQuoteReference(Cst.UnderlyingAsset pCategory, Quote pQuote, string pSource)
        {
            quoteReferenceSpecified = pQuote.valueSpecified;
            if (quoteReferenceSpecified)
                quoteReference = new PosKeepingQuote(pCategory, pQuote.idAsset, pQuote.idAsset_Identifier,
                    pQuote.QuoteSide.Value, pQuote.QuoteTiming.Value, pQuote.time, pQuote.value, pSource);
        }
        #endregion SetQuoteReference

        #region GetDeliveryPeriods
        public virtual Dictionary<DateTime, List<EFS_DeliveryPeriod>> GetDeliveryPeriods(string pCS, IProductBase pProduct, bool pIsApplySummertime)
        {
            return null;
        }
        #endregion GetDeliveryPeriods
        #region GetLogQuotationSideReference
        public virtual string GetLogQuotationSideReference(DateTime pDateReference)
        {
            return null;
        }
        #endregion GetLogQuotationSideReference
        #region GetSettlementDate
        // EG 20170424 [23064] Add pPosRequestReference parameter
        public virtual DateTime GetSettlementDate(DateTime pDateReference, IPosRequest pPosRequestReference)
        {
            return pDateReference;
        }
        #endregion GetSettlementDate
        #region GetOfficialCloseQuoteTime
        public virtual  DateTime GetOfficialCloseQuoteTime(DateTime pDateReference)
        {
            return pDateReference;
        }
        #endregion GetOfficialCloseQuoteTime
        #region VariationMargin
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public virtual Nullable<decimal> VariationMargin(Nullable<decimal> pPrice1, Nullable<decimal> pPrice2, decimal pQuantity)
        {
            return null;
        }
        #endregion VariationMargin
        #region UnderlyingAsset
        public virtual Nullable<Cst.UnderlyingAsset> UnderlyingAsset
        {
            get {return null;}
        }
        #endregion UnderlyingAsset

        #endregion Methods

        #region IPosKeepingAsset Members
        int IPosKeepingAsset.IdAsset
        {
            set { idAsset = value; }
            get { return idAsset; }
        }
        string IPosKeepingAsset.Identifier
        {
            set { identifier = value; }
            get { return identifier; }
        }
        string IPosKeepingAsset.NominalCurrency
        {
            set { nominalCurrency = value; }
            get { return nominalCurrency; }
        }

        string IPosKeepingAsset.PriceCurrency
        {
            set { priceCurrency = value; }
            get { return priceCurrency; }
        }

        string IPosKeepingAsset.Currency
        {
            set { currency = value; }
            get { return currency; }
        }
        decimal IPosKeepingAsset.ContractMultiplier
        {
            set { contractMultiplier = value; }
            get { return contractMultiplier; }
        }
        string IPosKeepingAsset.IdBC
        {
            set { idBC = value; }
            get { return idBC; }
        }
        IPosKeepingQuote IPosKeepingAsset.Quote
        {
            set { quote = (PosKeepingQuote)value; }
            get { return quote; }
        }
        bool IPosKeepingAsset.QuoteSpecified
        {
            set { quoteSpecified = value; }
            get { return quoteSpecified; }
        }
        IPosKeepingQuote IPosKeepingAsset.QuoteReference
        {
            set { quoteReference = (PosKeepingQuote)value; }
            get { return quoteReference; }
        }
        bool IPosKeepingAsset.QuoteReferenceSpecified
        {
            set { quoteReferenceSpecified = value; }
            get { return quoteReferenceSpecified; }
        }
        int IPosKeepingAsset.IdM
        {
            set { idM = value; }
            get { return idM; }
        }
        bool IPosKeepingAsset.IdMSpecified
        {
            set { idMSpecified = value; }
            get { return idMSpecified; }
        }
        #endregion IPosKeepingAsset Members

    }
    #endregion PosKeepingAsset
    #region PosKeepingAsset_BOND
    /// <summary>
    /// Utilisation pour les DEBTSECURITY (TRADE)
    /// </summary>
    public partial class PosKeepingAsset_BOND : IPosKeepingAsset
    {
        #region Constructors
        public PosKeepingAsset_BOND() : base() { }
        #endregion Constructors
        #region NominalValue
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public override decimal NominalValue(decimal pQuantity)
        {
            return nominal * pQuantity;
        }
        #endregion NominalValue
        #region UnderlyingAsset
        public override Nullable<Cst.UnderlyingAsset> UnderlyingAsset
        {
            get { return Cst.UnderlyingAsset.Bond; }
        }
        #endregion UnderlyingAsset

        #region RealizedMargin
        // EG 20190730 New Calcul des RMG pour DST
        public override Nullable<decimal> RealizedMargin(Nullable<decimal> pClosedPrice, Nullable<decimal> pClosingPrice, decimal pQuantity)
        {
            Nullable<decimal> realizedMargin = null;
            if (pClosedPrice.HasValue && pClosingPrice.HasValue)
                realizedMargin = (pClosedPrice - pClosingPrice) * NominalValue(pQuantity) * contractMultiplier;
            return realizedMargin;
        }
        #endregion RealizedMargin
    }
    #endregion PosKeepingAsset_BOND
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
    /// EG 20130603 Ticket: 18721
    /// PM 20130807 [18876] Variable Tick Value
    /// EG 20140120 Report 3.7
    /// EG 20120605 [19807] New  
    /// PL 20170411 [23064] Modify
    public partial class PosKeepingAsset_ETD : IPosKeepingAsset_ETD
    {
        #region Constructors
        public PosKeepingAsset_ETD():base() { }
        
        public PosKeepingAsset_ETD(DateTime pMaturityDate, string pCategory, decimal pNominal, string pCurrency,
                               decimal pStrikePrice, decimal pContractMultiplier, int pInstrumentNum, int pInstrumentDen,
                               string pIdBC, string pPutCall, string pAssetCategory, decimal pFactor,
                               DateTime pDeliveryDate, SettlMethodEnum pSettltMethod, PhysicalSettlementAmountEnum pPhysicalSettlementAmount, 
                               IOffset pDeliveryDelayOffset, FuturesValuationMethodEnum pValuationMethod)
            : base()
        {
            Set(pMaturityDate, pCategory, pNominal, pCurrency, pStrikePrice, pContractMultiplier,
                pInstrumentNum, pInstrumentDen, pIdBC, pPutCall, pAssetCategory, pFactor,
                pDeliveryDate, pSettltMethod, pPhysicalSettlementAmount,
                pDeliveryDelayOffset, pValuationMethod);
        }
        #endregion Constructors
        #region Methods
        #region CashSettlement
        /// <summary>
        /// Calcul du montant de CashSettlement lors d'un dénouement d'option
        /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
        /// <para>CASHSETTLEMENT : (Cours de clôture (en base 100) - Strike) * Qty * ContractMultiplier</para>
        /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pStrikePrice">Strike</param>
        /// <param name="pClosingPrice">Cours de clôture (exprimé dans la base du contrat)</param>
        /// <param name="pQuantity">Quantité</param>
        /// <returns>Montant du CashSettlement</returns>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20151001 [21414] Nullable<decimal> to pClosingPrice and return
        // EG 20170127 Qty Long To Decimal
        public override Nullable<decimal> CashSettlement(decimal pStrikePrice, Nullable<decimal> pClosingPrice, decimal pQuantity)
        {
            Nullable<decimal> cashSettlement = null;
            if (pClosingPrice.HasValue)
            {
                Nullable<decimal> closingPrice100 = ToBase100_UNL(pClosingPrice);
                decimal varStrikePrice = VariableContractValue_UNL(pStrikePrice);
                closingPrice100 = VariableContractValue_UNL(closingPrice100);
                cashSettlement = CashFlowValorization(closingPrice100.Value, varStrikePrice, contractMultiplier, pQuantity);
            }
            return cashSettlement;
        }
        #endregion CashSettlement
        #region NominalValue
        /// <summary>
        /// Calcul du nominal sur futures et options 
        /// <para>──────────────────────────────────────────────────────────────────────────────────</para>
        /// <para>OPTION : ContractMultiplier * Strike(en base 100) * Qty</para>
        /// <para>FUTURE : Nominal * Qty</para>
        /// <para>──────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pQuantity">Quantité</param>
        /// <returns>Nominal</returns>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public override decimal NominalValue(decimal pQuantity)
        {
            decimal ret = 0;
            if (CfiCodeCategoryEnum.Option == category)
            {
                decimal varStrikePrice = VariableContractValue_UNL(strikePrice);
                ret = contractMultiplier * ToBase100(varStrikePrice) * pQuantity;
            }
            else if (CfiCodeCategoryEnum.Future == category)
                ret = nominal * pQuantity;
            return ret;
        }
        #endregion NominalValue
        #region RealizedMargin
        /// <summary>
        /// Calcul du Realized margin sur les clôtures
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// <para>RMG : (Cours clôturée (en base 100) - Cours clôturante (en base 100)) * Qty * ContractMultiplier</para>
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pClosedPrice">Cours de clôture de la clôturée</param>
        /// <param name="pClosingPrice">Cours de clôture de la clôturante</param>
        /// <param name="pQuantity">Quantité</param>
        /// <returns>Montant du Realized margin</returns>
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public override Nullable<decimal> RealizedMargin(Nullable<decimal> pClosedPrice, Nullable<decimal> pClosingPrice, decimal pQuantity)
        {
            Nullable<decimal> realizedMargin = null;
            if (pClosedPrice.HasValue && pClosingPrice.HasValue)
            {
                decimal? closedPrice100 = ToBase100(pClosedPrice);
                closedPrice100 = VariableContractValue(closedPrice100);
                decimal? closingPrice100 = ToBase100(pClosingPrice);
                closingPrice100 = VariableContractValue(closingPrice100);
                //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                //realizedMargin = (closedPrice100 - closingPrice100) * pQuantity * contractMultiplier;
                //PM 20150129 [20737][20754] Modification du calcul unitaire
                //realizedMargin = CashFlowValorization(closedPrice100.Value - closingPrice100.Value, contractMultiplier, pQuantity);
                realizedMargin = CashFlowValorization(closedPrice100.Value, closingPrice100.Value, contractMultiplier, pQuantity);
            }
            return realizedMargin;
        }
        #endregion RealizedMargin
        #region ToBase100
        /// <summary>
        /// Retourne la conversion en base 100 (fonction du dénominateur du derivativeContract)
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// <para>B100 : E(cours) + ({cours} * 100 * NUM / DEN)</para>
        /// <para>avec</para>
        /// <para>E       = Partie entière</para>
        /// <para>{cours} = Partie fractionnée du cours</para>
        /// <para>NUM     = Numérateur du derivative contract</para>
        /// <para>DEN     = Dénominateur du derivative contract</para>
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pValue">Value to convert</param>
        /// <returns>Value in base 100</returns>
        public override decimal ToBase100(decimal pValue)
        {
            //double value = Convert.ToDouble(pValue);
            //double floorValue = System.Math.Floor(value);
            //double value100 = floorValue + ((value - floorValue) * 100 * instrumentNum / instrumentDen);
            //return Convert.ToDecimal(value100);

            //RD 20130522 [18666] Use ExchangeTradedDerivativeTools.ToBase100
            return ExchangeTradedDerivativeTools.ToBase100(pValue, instrumentNum, instrumentDen);
        }
        public override decimal ToBase100_UNL(decimal pValue)
        {
            if ((0 < instrumentNum_Underlyer) && (0 < instrumentDen_Underlyer))
            {
                return ExchangeTradedDerivativeTools.ToBase100(pValue, instrumentNum_Underlyer, instrumentDen_Underlyer);
            }
            else
                return ToBase100(pValue);
        }
        #endregion ToBase100
        #region VariableContractValue
        /// <summary>
        /// Calcul la valeur du contrat si celui-ci et un contract à valeur variable
        /// </summary>
        /// <param name="pPrice">Prix du contrat</param>
        /// <returns>La valeur du contrat s'il s'agit d'un contract à valeur variable, sinon <paramref name="pPrice"/></returns></returns>
        public override decimal VariableContractValue(decimal pPrice)
        {
            // PM 20181016 [24261] Add parameter NominalAmount
            //return ExchangeTradedDerivativeTools.VariableContractValue(category, priceQuoteMethod, strikePrice, pPrice);
            return ExchangeTradedDerivativeTools.VariableContractValue(category, priceQuoteMethod, strikePrice, pPrice);
        }
        /// <summary>
        /// Calcul la valeur du contrat sous-jacent d'un option si celui-ci et un contract à valeur variable
        /// </summary>
        /// <param name="pPrice">Prix du contrat sous-jacent</param>
        /// <returns>La valeur du contrat s'il s'agit d'un contract à valeur variable, sinon <paramref name="pPrice"/></returns></returns>
        public override decimal VariableContractValue_UNL(decimal pPrice)
        {
            // PM 20181016 [24261] Add parameter NominalAmount
            //return ExchangeTradedDerivativeTools.VariableContractValue(CfiCodeCategoryEnum.Future, priceQuoteMethod, 0, pPrice);
            return ExchangeTradedDerivativeTools.VariableContractValue(CfiCodeCategoryEnum.Future, priceQuoteMethod, 0, pPrice);
        }
        #endregion VariableContractValue
        #region CashFlowValorization
        /// <summary>
        /// Calcul du montant d'un Cash Flow ((pPrice1 - pPrice2) * pContractMultiplier * pQuantity) (*)
        /// <para>(*) Application eventuelle d'arrondi avant d'appliquer la quantité de lot</para>
        /// </summary>
        /// <param name="pPrice1">Prix 1</param>
        /// <param name="pPrice2">Prix 2</param>
        /// <param name="pContractMultiplier"></param>
        /// <param name="pQuantity"></param>
        /// <returns></returns>
        //PM 20140807 [20273][20106] Add method CashFlowValorization
        //PM 20141120 [20508] Ajout de la gestion du paramètre indiquant s'il faut réaliser un calcul unitaire ou non : cashFlowCalculationMethod
        //PM 20150129 [20737][20754] Ajout du paramètre pPrice2
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public override decimal CashFlowValorization(decimal pPrice1, decimal pPrice2, decimal pContractMultiplier, decimal pQuantity)
        {
            return ExchangeTradedDerivativeTools.CashFlowValorization(cashFlowCalculationMethod, pPrice1, pPrice2, pContractMultiplier, pQuantity, roundDir, roundPrec);
        }
        #endregion CashFlowValorization

        #region IsAtMaturity
        // EG 20140326 [19771][19785] 
        private bool IsAtMaturity(DateTime pDateReference)
        {
            bool isAtMaturity = (pDateReference == maturityDate);
            if (false == isAtMaturity)
                isAtMaturity = precedingMaturityDateSysSpecified && (precedingMaturityDateSys == pDateReference);
            return isAtMaturity;
        }
        #endregion IsAtMaturity
        #region IsPhysicalNoPeriodicDelivery
        // PL 20170413 [23064] New
        // EG 20170424 [23064] Add pPosRequestReference parameter
        private bool IsPhysicalNoPeriodicDelivery(DateTime pDateReference, IPosRequest pPosRequestReference)
        {
            return (settlMethod == SettlMethodEnum.PhysicalSettlement) && 
                   (physicalSettlementAmount != PhysicalSettlementAmountEnum.NA) && 
                   IsAtMaturity(pDateReference) && 
                   (false == (pPosRequestReference is IPosRequestPhysicalPeriodicDelivery)); 
        }
        #endregion IsPhysicalNoPeriodicDelivery

        #region Set
        /// <summary>
        /// Alimentation de la classe par les caractéristiques du derivative contract
        /// </summary>
        /// <param name="pMaturityDate">Date de maturité</param>
        /// <param name="pCategory">Catégorie (option ou future)</param>
        /// <param name="pNominal">Nominal</param>
        /// <param name="pCurrency">Devise</param>
        /// <param name="pStrikePrice">Strike</param>
        /// <param name="pContractMultiplier">Contract multiplier</param>
        /// <param name="pInstrumentNum">Numérateur</param>
        /// <param name="pInstrumentDen">Base d'expression des prix</param>
        /// <param name="pIdBC">Business center du marché</param>
        /// <param name="pPutCall">Put ou Call ou null</param>
        /// <param name="pAssetCategory">Catégorie de l'actif sous-jacent</param>
        /// <param name="pFactor">Contract Size</param>
        /// <param name="pDeliveryDelayOffset">Délai (offset) de livraison du sous-jacent</param>
        /// <param name="pValuationMethod">Méthode de valorisation</param>
        // EG 20120605 [19807] New 
        // EG 20160404 Migration vs2013
        public override void Set(DateTime pMaturityDate, string pCategory, decimal pNominal, string pCurrency,
                        decimal pStrikePrice, decimal pContractMultiplier,
                        int pInstrumentNum, int pInstrumentDen, string pIdBC, string pPutCall,
                        string pAssetCategory, decimal pFactor,
                        DateTime pDeliveryDate, SettlMethodEnum pSettltMethod, PhysicalSettlementAmountEnum pPhysicalSettlementAmount,
                        IOffset pDeliveryDelayOffset, FuturesValuationMethodEnum pValuationMethod)
        {
            maturityDate = pMaturityDate;
            category = (CfiCodeCategoryEnum)ReflectionTools.EnumParse(category, pCategory);
            nominal = pNominal;
            currency = pCurrency;
            strikePrice = pStrikePrice;
            contractMultiplier = pContractMultiplier;
            instrumentNum = pInstrumentNum;
            instrumentDen = pInstrumentDen;
            idBC = pIdBC;
            putCallSpecified = StrFunc.IsFilled(pPutCall);
            if (putCallSpecified)
                putCall = (PutOrCallEnum)ReflectionTools.EnumParse(putCall, pPutCall);
            factor = pFactor;
            assetCategorySpecified = StrFunc.IsFilled(pAssetCategory);
            if (assetCategorySpecified)
                assetCategory = (Cst.UnderlyingAsset)ReflectionTools.EnumParse(assetCategory, pAssetCategory);

            settlMethod = pSettltMethod;
            physicalSettlementAmount = pPhysicalSettlementAmount;
            deliveryDateSpecified = DtFunc.IsDateTimeFilled(pDeliveryDate);
            if (deliveryDateSpecified)
                deliveryDate = pDeliveryDate;

            deliveryDelayOffsetSpecified = (null != pDeliveryDelayOffset);
            if (deliveryDelayOffsetSpecified)
                deliveryDelayOffset = (Offset)pDeliveryDelayOffset;
            // EG 20120605 [19807] New
            valuationMethod = pValuationMethod;
        }
        #endregion Set

        #region GetDeliveryPeriods
        // EG 20171025 [23509] Set timeZone
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190115 [24361] Use isSettlementOfHolidayDeliveryConvention (Migration financial settlement for BoM Products)
        public override Dictionary<DateTime, List<EFS_DeliveryPeriod>> GetDeliveryPeriods(string pCS, IProductBase pProduct, bool pIsApplySummertime)
        {
            Dictionary<DateTime, List<EFS_DeliveryPeriod>> dicDeliveryPeriods = new Dictionary<DateTime, List<EFS_DeliveryPeriod>>();

            if (deliveryPeriodFrequencySpecified && firstDeliveryDateSpecified && lastDeliveryDateSpecified)
            {
                bool isOneDayPeriod = (deliveryPeriodFrequency.Interval.Period == PeriodEnum.D) && (deliveryPeriodFrequency.Interval.PeriodMultiplier.IntValue == 1);
                DateTime terminationDate = lastDeliveryDate;
                if (isOneDayPeriod)
                    terminationDate = terminationDate.AddDays(1);
                EFS_Period[] periods = Tools.ApplyInterval(firstDeliveryDate, terminationDate,
                    deliveryPeriodFrequency.Interval, deliveryPeriodFrequency.RollConvention);
                if (ArrFunc.IsFilled(periods))
                {
                    IBusinessDayAdjustments bda = null;
                    if (deliverySettlementOffsetSpecified)
                    {
                        // Recherche de tous les Business Centers concernés (Devise inclus)
                        List<string> idBCs = new List<string>
                        {
                            idBC
                        };

                        IBusinessCenters bcs = deliverySettlementOffset.GetBusinessCentersCurrency(pCS, null, currency);
                        if (null != bcs)
                        {
                            bcs.BusinessCenter.ToList().ForEach(bc =>
                            {
                                if (false == idBCs.Contains(bc.Value))
                                    idBCs.Add(bc.Value);
                            });
                        }
                        BusinessDayConventionEnum bdc = BusinessDayConventionEnum.NotApplicable;
                        if (settlementOfHolidayDeliveryConventionSpecified)
                            bdc = StringToEnum.BusinessDayConvention(settlementOfHolidayDeliveryConvention.ToString());
                        bda = pProduct.CreateBusinessDayAdjustments(bdc, idBCs.ToArray());
                        // EG 20190115 [24361] Set isSettlementOfHolidayDeliveryConvention pour utilisation de la Convention sur le Delivery Settlement
                        bda.IsSettlementOfHolidayDeliveryConvention = settlementOfHolidayDeliveryConventionSpecified;
                    }

                    periods.ToList().ForEach(period =>
                    {
                        // Calcul de la date de paiement
                        DateTime settlementDate = period.date1;
                        if (deliverySettlementOffsetSpecified)
                            settlementDate = Tools.ApplyOffset(pCS, settlementDate, deliverySettlementOffset, bda, null as DataDocumentContainer);

                        // Calcul des heures de livraison sur la base de la date de début de livraison de la période
                        DateTimeOffset deliveryDateStart = period.date1;
                        DateTimeOffset deliveryDateEnd = period.date1;
                        string timeZone = Tz.Tools.UniversalTimeZone;
                        if (deliveryTimeStartSpecified && deliveryTimeEndSpecified)
                        {
                            TimeSpan timeStart = deliveryTimeStart.HourMinuteTime.TimeValue.TimeOfDay;
                            TimeSpan timeEnd = deliveryTimeEnd.HourMinuteTime.TimeValue.TimeOfDay;
                            timeZone = deliveryTimeEnd.Location.Value;

                            // L'heure de début est plus grande que l'heure fin 
                            // alors on travaille sur la date de fin (n'a de sens que sur des périodes journalières)
                            if (timeStart >= timeEnd)
                                deliveryDateEnd = period.date2;

                            deliveryDateStart = deliveryTimeStart.Offset(deliveryDateStart.Date);
                            deliveryDateEnd = deliveryTimeEnd.Offset(deliveryDateEnd.Date);
                        }
                        EFS_DeliveryPeriod deliveryPeriod = new EFS_DeliveryPeriod(period, deliveryDateStart, deliveryDateEnd, timeZone, settlementDate, pIsApplySummertime);

                        // Ajout de la date de règlement dans le dictionnaire
                        if (false == dicDeliveryPeriods.ContainsKey(settlementDate))
                            dicDeliveryPeriods.Add(settlementDate, new List<EFS_DeliveryPeriod>());

                        dicDeliveryPeriods[settlementDate].Add(deliveryPeriod);
                    });
                }
            }
            return dicDeliveryPeriods;
        }
        #endregion GetDeliveryPeriods

        #region GetLogQuotationSideReference
        public override string GetLogQuotationSideReference(DateTime pDateReference)
        {
            string _quotationSide = QuotationSideEnum.OfficialClose.ToString();
            if (exerciseStyleSpecified)
            {
                // EG 20140326 [19771][19785] 
                if (IsAtMaturity(pDateReference))
                {
                    #region Final Settlement Price d’une option à l’échéance (dénouement automatique ou manuel)
                    if (false == finalSettlementSideSpecified)
                    {
                        if (false == quoteReferenceSpecified)
                            _quotationSide = QuotationSideEnum.OfficialSettlement.ToString();
                        if (false == quoteSpecified)
                            _quotationSide += "|" + QuotationSideEnum.OfficialSettlement.ToString();
                    }
                    else if (finalSettlementSide == QuotationSideEnum.OfficialSettlement)
                    {
                        if (false == quoteReferenceSpecified)
                            _quotationSide = QuotationSideEnum.OfficialSettlement.ToString();
                    }
                    else if (finalSettlementSide == QuotationSideEnum.OfficialClose)
                    {
                        if (false == quoteSpecified)
                            _quotationSide = QuotationSideEnum.OfficialSettlement.ToString();
                    }
                    #endregion Final Settlement Price d’une option à l’échéance (dénouement automatique ou manuel)
                }
            }
            return _quotationSide;
        }
        #endregion GetLogQuotationSideReference
        #region GetLogQuoteInformationRelativeTo
        /// EG 20140120 Report 3.7 [19456] New
        // EG 20190926 Upd : Refactoring Cst.PosRequestTypeEnum
        public string GetLogQuoteInformationRelativeTo(IPosRequest pPosRequestReference)
        {
            string _log = string.Empty;
            if ((pPosRequestReference is IPosRequestMaturityOffsetting) ||
                ((Cst.PosRequestTypeEnum.RequestTypeOption & pPosRequestReference.RequestType) == pPosRequestReference.RequestType))
            {
                _log += LogTools.IdentifierAndId(identifier_Underlyer, idAsset_Underlyer);
            }
            else
            {
                _log += LogTools.IdentifierAndId(identifier, idAsset);
            }

            DateTime dtQuote = GetSettlementQuoteTime(pPosRequestReference.DtBusiness, pPosRequestReference);
            _log += " - " + GetLogQuotationSideReference(pPosRequestReference.DtBusiness);
            _log += " - " + DtFunc.DateTimeToStringISO(dtQuote);
            return _log;
        }
        #endregion GetLogQuoteInformationRelativeTo

        #region GetOfficialCloseQuoteTime
        // EG 20140204 Retourne la date de cotation d'un OfficialClose en fonction de la date d'échéance (réelle / Forçée)
        public override DateTime GetOfficialCloseQuoteTime(DateTime pDateReference)
        {
            DateTime dtQuote = pDateReference;
            DateTime _maturityDate = maturityDateSys;
            if (DtFunc.IsDateTimeEmpty(maturityDateSys))
                _maturityDate = maturityDate;
            if (DtFunc.IsDateTimeFilled(_maturityDate) && (pDateReference >= _maturityDate))
                dtQuote = _maturityDate;
            return dtQuote;
        }
        #endregion GetOfficialCloseQuoteTime
        #region GetSettlementDate
        /// EG 20140120 Report 3.7 [19456] New
        /// EG 20140121 Homologuation
        // EG 20140204 Test sur MaturityDateSys (cas Echéance forcée à J+n (n>1) et pDateReference entre maturityDateSys et maturityDate
        // EG 20140326 [19771][19785] 
        // EG 20170424 [23064] Add pPosRequestReference parameter
        public override DateTime GetSettlementDate(DateTime pDateReference, IPosRequest pPosRequestReference)
        {
            DateTime settlementDate = pDateReference;
            // EG 20140326 [19771][19785] 
            // EG 20170424 [23064]
            if (IsAtMaturity(pDateReference) ||
                (pPosRequestReference is IPosRequestPhysicalPeriodicDelivery) ||
                (DtFunc.IsDateTimeFilled(maturityDateSys) && (pDateReference >= maturityDateSys)))
            {
                settlementDate = maturityDateSys;
                if (FinalSettlementPriceEnum.LastTradingDay == finalSettlementPrice)
                    settlementDate = lastTradingDay;

                if (finalSettlementTimeSpecified)
                {
                    DateTime _result = new DtFunc().StringToDateTime(finalSettlementTime, DtFunc.FmtISOShortTime, false);
                    TimeSpan _time = new TimeSpan(_result.Hour, _result.Minute, _result.Second);
                    settlementDate = settlementDate.Add(_time);
                }
            }
            return settlementDate;

        }
        #endregion GetSettlementDate
        #region GetSettlementPrice
        /// <summary>
        /// Lecture du cours de référence pour Dénouement d'option
        /// 
        /// 1. Settlement Price d’une option américaine/bermuda exercée par anticipation (avant son échéance)
        ///    Recherche d’une cotation OfficialClose/Close (DSP) en date d’exercice
        ///    => Si cette cotation existe elle est retournée
        ///    
        /// 2. Final Settlement Price d’une option exercée à l'échéance (exercice manuel ou automatique)
        ///    2.1 FINALSETTLTSIDE est non renseignée
        ///        => Si la cotation EDSP (OfficialSettlement) existe elle est retournée
        ///        => Sinon si la cotation DSP (OfficialClose) existe elle est retournée
        ///    2.2 FINALSETTLTSIDE = EDSP (OfficialSettlement)
        ///        => Si la cotation EDSP existe elle est retournée
        ///    2.3 FINALSETTLTSIDE = DSP (OfficialClose) 
        ///        => Si la cotation DSP existe elle est retournée
        /// </summary>
        /// <returns></returns>
        /// EG 20140120 Report 3.7 [19456] New
        /// EG 20140326 [19771][19785] 
        /// EG 20170206 [22787] Add GetSettlementPriceBySide
        public IPosKeepingQuote GetSettlementPrice(DateTime pDateReference, ref Cst.ErrLevel pErrLevel)
        {
            return GetSettlementPrice(pDateReference, ref pErrLevel, null);
        }
        // EG 20170424 [23064] Add pPosRequestReference parameter
        public IPosKeepingQuote GetSettlementPrice(DateTime pDateReference, ref Cst.ErrLevel pErrLevel, IPosRequest pPosRequestReference)
        {
            IPosKeepingQuote _quote = null;
            if (exerciseStyleSpecified)
            {
                // EG 20140326 [19771][19785] 
                if (IsAtMaturity(pDateReference))
                {
                    #region Final Settlement Price d’une option à l’échéance (dénouement automatique ou manuel)
                    /// EG 20170206 [22787]
                    _quote = GetSettlementPriceBySide();
                    #endregion Final Settlement Price d’une option à l’échéance (dénouement automatique ou manuel)
                }
                else
                {
                    #region Settlement Price d’une option (dénouement anticipé)
                    switch (exerciseStyle)
                    {
                        case DerivativeExerciseStyleEnum.American:
                        case DerivativeExerciseStyleEnum.Bermuda:
                            if (quoteSpecified)
                                _quote = quote;
                            break;
                        case DerivativeExerciseStyleEnum.European:
                            // EG 20140326 [19771][19785] 
                            pErrLevel = Cst.ErrLevel.DATAREJECTED;
                            break;
                    }
                    #endregion Settlement Price d’une option (dénouement anticipé)
                }
            }
            // EG 20170424 [23064]
            else if (pPosRequestReference is IPosRequestPhysicalPeriodicDelivery) 
            {
                _quote = GetSettlementPriceBySide();
            }
            // PL 20170413 [23064]
            // EG 20170424 [23064] 
            else if (IsPhysicalNoPeriodicDelivery(pDateReference, pPosRequestReference))
            {
                _quote = GetSettlementPriceBySide();
            }
            return _quote;
        }
        #endregion GetSettlementPrice

        #region GetSettlementPriceBySide
        // EG 20170206 [22787] New
        private IPosKeepingQuote GetSettlementPriceBySide()
        {
            IPosKeepingQuote _quote = null;
            if (false == finalSettlementSideSpecified)
            {
                // OfficialSettlement ou OfficialClose
                if (quoteReferenceSpecified)
                    _quote = quoteReference;
                else if (quoteSpecified)
                    _quote = quote;
            }
            else if ((finalSettlementSide == QuotationSideEnum.OfficialSettlement) && quoteReferenceSpecified)
            {
                //OfficialSettlement
                _quote = quoteReference;
            }
            else if ((finalSettlementSide == QuotationSideEnum.OfficialClose) && quoteSpecified)
            {
                //OfficialSettlement
                _quote = quote;
            }
            return _quote;
        }
        #endregion GetSettlementPriceBySide

        #region GetSettlementQuoteTime
        /// <summary>
        /// Lecture du la date de lecture de la cotation en fonction du type de traitement demandé
        /// </summary>
        /// <returns></returns>
        // EG 20140115 [19456] New
        // EG 20170424 [23064] Add pPosRequestReference parameter
        // EG 20190926 Upd : Refactoring Cst.PosRequestTypeEnum
        public DateTime GetSettlementQuoteTime(DateTime pDateReference, IPosRequest pPosRequestReference)
        {
            DateTime _quoteTime = pDateReference;
            if ((pPosRequestReference is IPosRequestMaturityOffsetting) ||
                (pPosRequestReference is IPosRequestPhysicalPeriodicDelivery) ||
                ((Cst.PosRequestTypeEnum.RequestTypeOption & pPosRequestReference.RequestType) == pPosRequestReference.RequestType))
            {
                _quoteTime = GetSettlementDate(pDateReference, pPosRequestReference);
            }
            return _quoteTime;
        }
        #endregion GetSettlementQuoteTime
        #region SetFlagQuoteMandatory
        /// EG 20140120 Report 3.7 [19456] New
        // EG 20140212 [19602/19603] toujours lire OfficialClose si dénouement anticipé
        // EG 20140326 [19771][19785] 
        // EG 20170206 [22787]
        // EG 20170424 [23064] Add pPosRequestReference parameter
        public void SetFlagQuoteMandatory(DateTime pDateReference, IPosRequest pPosRequest)
        {
            if (exerciseStyleSpecified)
            {
                // EG 20140326 [19771][19785] 
                if (IsAtMaturity(pDateReference))
                {
                    if (finalSettlementSideSpecified)
                    {
                        isOfficialSettlementMandatory = (finalSettlementSide == QuotationSideEnum.OfficialSettlement);
                        isOfficialCloseMandatory = (finalSettlementSide == QuotationSideEnum.OfficialClose);
                    }
                }
                else
                {
                    switch (exerciseStyle)
                    {
                        case DerivativeExerciseStyleEnum.American:
                        case DerivativeExerciseStyleEnum.Bermuda:
                            // EG 20140212 [19602/19603] toujours lire OfficialClose
                            isOfficialSettlementMandatory = false;
                            isOfficialCloseMandatory = true;
                            break;
                        case DerivativeExerciseStyleEnum.European:
                            // IMPOSSIBLE
                            break;
                    }
                }
            }
            // EG 20170424 [23064]
            else if (pPosRequest is IPosRequestPhysicalPeriodicDelivery)
            {
                if (finalSettlementSideSpecified)
                {
                    isOfficialSettlementMandatory = (finalSettlementSide == QuotationSideEnum.OfficialSettlement);
                    isOfficialCloseMandatory = (finalSettlementSide == QuotationSideEnum.OfficialClose);
                }
            }
            else if (pPosRequest is IPosRequestMaturityOffsetting)
            {
                // FI 20190717 [24785][24752] le prix de clôture de l'asset Future est obligatoire
                // Si Traitement EOD 
                // - Spheres Affiche ERROR dans le LOG
                // - Le statut du POSREQUEST MOF est en ERROR 
                isOfficialSettlementMandatory = false;
                isOfficialCloseMandatory = true;
            }
        }
        #endregion SetFlagQuoteMandatory


        #region VariationMargin
        /// <summary>
        /// Calcul du montant de variation margin utilisée dans :
        /// <para>───────────────────────────────────────────────────</para>
        /// ► le dénouement d'options
        /// ► la correction de postion 
        /// ► le transfert de position
        /// <para>───────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pPrice1">Cours de négociation ou de clôture jour</param>
        /// <param name="pPrice2">Cours de clôture jour ou de négociation</param>
        /// <param name="pQuantity">Quantité</param>
        /// <returns>Montant de la variation de marge</returns>
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public override Nullable<decimal> VariationMargin(Nullable<decimal> pPrice1, Nullable<decimal> pPrice2, decimal pQuantity)
        {
            Nullable<decimal> variationMargin = null;
            if (pPrice1.HasValue && pPrice2.HasValue)
            {
                decimal price1_100 = ToBase100(pPrice1.Value);
                decimal pPrice2_100 = ToBase100(pPrice2.Value);
                // PM 20130807 [18876] For Variable Tick Value
                price1_100 = VariableContractValue(price1_100);
                pPrice2_100 = VariableContractValue(pPrice2_100);
                //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                //variationMargin = (pPrice2_100 - price1_100) * pQuantity * contractMultiplier;
                //PM 20150129 [20737][20754] Modification du calcul unitaire
                //variationMargin = CashFlowValorization(pPrice2_100 - price1_100, contractMultiplier, pQuantity);
                variationMargin = CashFlowValorization(pPrice2_100, price1_100, contractMultiplier, pQuantity);
            }
            return variationMargin;
        }
        #endregion VariationMargin
        #region UnderlyingAsset
        public override Nullable<Cst.UnderlyingAsset> UnderlyingAsset
        {
            get { return Cst.UnderlyingAsset.ExchangeTradedContract; }
        }
        #endregion UnderlyingAsset

        #endregion Methods
        #region IPosKeepingAsset_ETD Members
        DateTime IPosKeepingAsset_ETD.MaturityDate
        {
            set { maturityDate = value; }
            get { return maturityDate; }
        }
        DateTime IPosKeepingAsset_ETD.MaturityDateSys
        {
            set { maturityDateSys = value; }
            get { return maturityDateSys; }
        }
        bool IPosKeepingAsset_ETD.PrecedingMaturityDateSysSpecified
        {
            set { precedingMaturityDateSysSpecified = value; }
            get { return precedingMaturityDateSysSpecified; }
        }
        DateTime IPosKeepingAsset_ETD.PrecedingMaturityDateSys
        {
            set { precedingMaturityDateSys = value; }
            get { return precedingMaturityDateSys; }
        }
        DateTime IPosKeepingAsset_ETD.LastTradingDay
        {
            set { lastTradingDay = value; }
            get { return lastTradingDay; }
        }
        FinalSettlementPriceEnum IPosKeepingAsset_ETD.FinalSettlementPrice
        {
            set { finalSettlementPrice = value; }
            get { return finalSettlementPrice; }
        }
        CfiCodeCategoryEnum IPosKeepingAsset_ETD.Category
        {
            set { category = value; }
            get { return category; }
        }
        bool IPosKeepingAsset_ETD.ExerciseStyleSpecified
        {
            set { exerciseStyleSpecified = value; }
            get { return exerciseStyleSpecified; }
        }

        DerivativeExerciseStyleEnum IPosKeepingAsset_ETD.ExerciseStyle
        {
            set { exerciseStyle = value; }
            get { return exerciseStyle; }
        }

        decimal IPosKeepingAsset_ETD.Nominal
        {
            set { nominal = value; }
            get { return nominal; }
        }
        decimal IPosKeepingAsset_ETD.StrikePrice
        {
            set { strikePrice = value; }
            get { return strikePrice; }
        }
        int IPosKeepingAsset_ETD.InstrumentNum
        {
            set { instrumentNum = value; }
            get { return instrumentNum; }
        }
        int IPosKeepingAsset_ETD.InstrumentDen
        {
            set { instrumentDen = value; }
            get { return instrumentDen; }
        }
        SettlMethodEnum IPosKeepingAsset_ETD.SettlMethod
        {
            set { settlMethod = value; }
            get { return settlMethod; }
        }
        PutOrCallEnum IPosKeepingAsset_ETD.PutCall
        {
            set { putCall = value; }
            get { return putCall; }
        }
        bool IPosKeepingAsset_ETD.PutCallSpecified
        {
            set { putCallSpecified = value; }
            get { return putCallSpecified; }
        }
        Cst.UnderlyingAsset IPosKeepingAsset_ETD.AssetCategory
        {
            set { assetCategory = value; }
            get { return assetCategory; }
        }
        bool IPosKeepingAsset_ETD.AssetCategorySpecified
        {
            set { assetCategorySpecified = value; }
            get { return assetCategorySpecified; }
        }
        decimal IPosKeepingAsset_ETD.Factor
        {
            set { factor = value; }
            get { return factor; }
        }
        bool IPosKeepingAsset_ETD.DeliveryDateSpecified
        {
            set { deliveryDateSpecified = value; }
            get { return deliveryDateSpecified; }
        }
        DateTime IPosKeepingAsset_ETD.DeliveryDate
        {
            set { deliveryDate = value; }
            get { return deliveryDate; }
        }
        bool IPosKeepingAsset_ETD.DeliveryDelayOffsetSpecified
        {
            set { deliveryDelayOffsetSpecified = value; }
            get { return deliveryDelayOffsetSpecified; }
        }

        IOffset IPosKeepingAsset_ETD.DeliveryDelayOffset
        {
            set { deliveryDelayOffset = (Offset)value; }
            get { return deliveryDelayOffset; }
        }
        int IPosKeepingAsset_ETD.IdAsset_Underlyer
        {
            set { idAsset_Underlyer = value; }
            get { return idAsset_Underlyer; }
        }
        bool IPosKeepingAsset_ETD.IdAsset_UnderlyerSpecified
        {
            set { idAsset_UnderlyerSpecified = value; }
            get { return idAsset_UnderlyerSpecified; }
        }
        string IPosKeepingAsset_ETD.Identifier_Underlyer
        {
            set { identifier_Underlyer = value; }
            get { return identifier_Underlyer; }
        }
        bool IPosKeepingAsset_ETD.Identifier_UnderlyerSpecified
        {
            set { identifier_UnderlyerSpecified = value; }
            get { return identifier_UnderlyerSpecified; }
        }
        int IPosKeepingAsset_ETD.IdDC_Underlyer
        {
            set { idDC_Underlyer = value; }
            get { return idDC_Underlyer; }
        }
        bool IPosKeepingAsset_ETD.IdDC_UnderlyerSpecified
        {
            set { idDC_UnderlyerSpecified = value; }
            get { return idDC_UnderlyerSpecified; }
        }
        int IPosKeepingAsset_ETD.InstrumentNum_Underlyer
        {
            set { instrumentNum_Underlyer = value; }
            get { return instrumentNum_Underlyer; }
        }
        int IPosKeepingAsset_ETD.InstrumentDen_Underlyer
        {
            set { instrumentDen_Underlyer = value; }
            get { return instrumentDen_Underlyer; }
        }
        PriceQuoteMethodEnum IPosKeepingAsset_ETD.PriceQuoteMethod
        {
            set { priceQuoteMethod = value; }
            get { return priceQuoteMethod; }
        }
        bool IPosKeepingAsset_ETD.FinalSettlementSideSpecified
        {
            set { finalSettlementSideSpecified = value; }
            get { return finalSettlementSideSpecified; }
        }
        QuotationSideEnum IPosKeepingAsset_ETD.FinalSettlementSide
        {
            set { finalSettlementSide = value; }
            get { return finalSettlementSide; }
        }
        bool IPosKeepingAsset_ETD.FinalSettlementTimeSpecified
        {
            set { finalSettlementTimeSpecified = value; }
            get { return finalSettlementTimeSpecified; }
        }
        string IPosKeepingAsset_ETD.FinalSettlementTime
        {
            set { finalSettlementTime = value; }
            get { return finalSettlementTime; }
        }
        FuturesValuationMethodEnum IPosKeepingAsset_ETD.ValuationMethod
        {
            set { valuationMethod = value; }
            get { return valuationMethod; }
        }
        //PM 20140807 [20273][20106] Ajout de roundDir et roundPrec
        string IPosKeepingAsset_ETD.RoundDir
        {
            set { roundDir = value; }
            get { return roundDir; }
        }
        int IPosKeepingAsset_ETD.RoundPrec
        {
            set { roundPrec = value; }
            get { return roundPrec; }
        }
        //PM 20141120 [20508] Ajout de cashFlowCalculationMethod
        CashFlowCalculationMethodEnum IPosKeepingAsset_ETD.CashFlowCalculationMethod
        {
            set { cashFlowCalculationMethod = value; }
            get { return cashFlowCalculationMethod; }
        }
        // EG 20170206 [22787] New
        bool IPosKeepingAsset_ETD.UnitOfMeasureSpecified
        {
            set { unitOfMeasureSpecified = value; }
            get { return unitOfMeasureSpecified; }
        }
        string IPosKeepingAsset_ETD.UnitOfMeasure
        {
            set { unitOfMeasure = value; }
            get { return unitOfMeasure; }
        }
        //bool IPosKeepingAsset_ETD.unitOfMeasureQtySpecified
        //{
        //    set { unitOfMeasureQtySpecified = value; }
        //    get { return unitOfMeasureQtySpecified; }
        //}
        decimal IPosKeepingAsset_ETD.UnitOfMeasureQty
        {
            set { unitOfMeasureQty = value; }
            get { return unitOfMeasureQty; }
        }

        // EG 20170206 [22787] New
        bool IPosKeepingAsset_ETD.FirstDeliveryDateSpecified
        {
            set { firstDeliveryDateSpecified = value; }
            get { return firstDeliveryDateSpecified; }
        }
        DateTime IPosKeepingAsset_ETD.FirstDeliveryDate 
        {
            set { firstDeliveryDate = value; }
            get { return firstDeliveryDate; }
        }
        bool IPosKeepingAsset_ETD.LastDeliveryDateSpecified 
        {
            set { lastDeliveryDateSpecified = value; }
            get { return lastDeliveryDateSpecified; }
        }
        DateTime IPosKeepingAsset_ETD.LastDeliveryDate
        {
            set { lastDeliveryDate = value; }
            get { return lastDeliveryDate; }
        }
        bool IPosKeepingAsset_ETD.DeliveryTimeStartSpecified
        {
            set { deliveryTimeStartSpecified = value; }
            get { return deliveryTimeStartSpecified; }
        }
        IPrevailingTime IPosKeepingAsset_ETD.DeliveryTimeStart
        {
            set { deliveryTimeStart = value; }
            get { return deliveryTimeStart; }
        }
        bool IPosKeepingAsset_ETD.DeliveryTimeEndSpecified
        {
            set { deliveryTimeEndSpecified = value; }
            get { return deliveryTimeEndSpecified; }
        }
        IPrevailingTime IPosKeepingAsset_ETD.DeliveryTimeEnd
        {
            set { deliveryTimeEnd = value; }
            get { return deliveryTimeEnd; }
        }
        //bool IPosKeepingAsset_ETD.winterToSummerDateSpecified
        //{
        //    set { winterToSummerDateSpecified = value; }
        //    get { return winterToSummerDateSpecified; }
        //}
        //DateTime IPosKeepingAsset_ETD.winterToSummerDate
        //{
        //    set { winterToSummerDate = value; }
        //    get { return winterToSummerDate; }
        //}
        //bool IPosKeepingAsset_ETD.summerToWinterDateSpecified
        //{
        //    set { summerToWinterDateSpecified = value; }
        //    get { return summerToWinterDateSpecified; }
        //}
        //DateTime IPosKeepingAsset_ETD.summerToWinterDate
        //{
        //    set { summerToWinterDate = value; }
        //    get { return summerToWinterDate; }
        //}
        bool IPosKeepingAsset_ETD.IsApplySummertime
        {
            set { isApplySummertime = value; }
            get { return isApplySummertime; }
        }
        bool IPosKeepingAsset_ETD.DeliveryPeriodFrequencySpecified
        {
            set { deliveryPeriodFrequencySpecified = value; }
            get { return deliveryPeriodFrequencySpecified; }
        }
        ICalculationPeriodFrequency IPosKeepingAsset_ETD.DeliveryPeriodFrequency
        {
            set { deliveryPeriodFrequency = value; }
            get { return deliveryPeriodFrequency; }
        }
        bool IPosKeepingAsset_ETD.DayTypeDeliverySpecified
        {
            set { dayTypeDeliverySpecified = value; }
            get { return dayTypeDeliverySpecified; }
        }
        DayTypeEnum IPosKeepingAsset_ETD.DayTypeDelivery
        {
            set { dayTypeDelivery = value; }
            get { return dayTypeDelivery; }
        }

        bool IPosKeepingAsset_ETD.DeliverySettlementOffsetSpecified
        {
            set { deliverySettlementOffsetSpecified = value; }
            get { return deliverySettlementOffsetSpecified; }
        }
        IOffset IPosKeepingAsset_ETD.DeliverySettlementOffset
        {
            set { deliverySettlementOffset = value; }
            get { return deliverySettlementOffset; }
        }
        bool IPosKeepingAsset_ETD.SettlementOfHolidayDeliveryConventionSpecified
        {
            set { settlementOfHolidayDeliveryConventionSpecified = value; }
            get { return settlementOfHolidayDeliveryConventionSpecified; }
        }
        SettlementOfHolidayDeliveryConventionEnum IPosKeepingAsset_ETD.SettlementOfHolidayDeliveryConvention
        {
            set { settlementOfHolidayDeliveryConvention = value; }
            get { return settlementOfHolidayDeliveryConvention; }
        }
        bool IPosKeepingAsset_ETD.FirstDeliverySettlementDateSpecified
        {
            set { firstDeliverySettlementDateSpecified = value; }
            get { return firstDeliverySettlementDateSpecified; }
        }
        DateTime IPosKeepingAsset_ETD.FirstDeliverySettlementDate
        {
            set { firstDeliverySettlementDate = value; }
            get { return firstDeliverySettlementDate; }
        }
        bool IPosKeepingAsset_ETD.LastDeliverySettlementDateSpecified
        {
            set { lastDeliverySettlementDateSpecified = value; }
            get { return lastDeliverySettlementDateSpecified; }
        }
        DateTime IPosKeepingAsset_ETD.LastDeliverySettlementDate
        {
            set { lastDeliverySettlementDate = value; }
            get { return lastDeliverySettlementDate; }
        }
        #endregion IPosKeepingAsset Members
    }
    #endregion PosKeepingAsset_ETD
    #region PosKeepingAsset_EQUITY
    public partial class PosKeepingAsset_EQUITY : IPosKeepingAsset
    {
        #region NominalValue
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public override decimal NominalValue(decimal pQuantity)
        {
            return nominal * pQuantity;
        }
        #endregion NominalValue

        #region Constructors
        public PosKeepingAsset_EQUITY() : base() { }
        #endregion Constructors
        #region Methods
        #region UnderlyingAsset
        public override Nullable<Cst.UnderlyingAsset> UnderlyingAsset
        {
            get { return Cst.UnderlyingAsset.EquityAsset; }
        }
        #endregion UnderlyingAsset

        #endregion Methods
    }
    #endregion PosKeepingAsset_EQUITY
    #region PosKeepingAsset_INDEX
    public partial class PosKeepingAsset_INDEX : IPosKeepingAsset
    {
        #region Constructors
        public PosKeepingAsset_INDEX() : base() { }
        #endregion Constructors
        #region Methods
        #region UnderlyingAsset
        public override Nullable<Cst.UnderlyingAsset> UnderlyingAsset
        {
            get { return Cst.UnderlyingAsset.Index; }
        }
        #endregion UnderlyingAsset

        #endregion Methods

    }
    #endregion PosKeepingAsset_INDEX
    #region PosKeepingAsset_RATEINDEX
    public partial class PosKeepingAsset_RATEINDEX : IPosKeepingAsset
    {
        #region Constructors
        public PosKeepingAsset_RATEINDEX() : base() { }
        #endregion Constructors
        #region Methods
        #region UnderlyingAsset
        public override Nullable<Cst.UnderlyingAsset> UnderlyingAsset
        {
            get { return Cst.UnderlyingAsset.RateIndex; }
        }
        #endregion UnderlyingAsset

        #endregion Methods
    }
    #endregion PosKeepingAsset_RATEINDEX

    #region PosKeepingAsset_FXRATE
    /// EG 20150302 New CFD Forex 
    public partial class PosKeepingAsset_FXRATE : IPosKeepingAsset
    {
        #region Constructors
        public PosKeepingAsset_FXRATE() : base() { }
        #endregion Constructors
        #region Methods
        #region UnderlyingAsset
        public override Nullable<Cst.UnderlyingAsset> UnderlyingAsset
        {
            get { return Cst.UnderlyingAsset.FxRateAsset; }
        }
        #endregion UnderlyingAsset

        #endregion Methods
    }
    #endregion PosKeepingAsset_FXRATE
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
    public abstract partial class PosKeepingCommon : IPosKeepingCommon
    {
        #region Constructors
        public PosKeepingCommon() { }
        #endregion Constructors
        #region IPosKeepingCommon Members
        PosKeepingAsset IPosKeepingCommon.Asset
        {
            set { asset = value; }
            get { return asset; }
        }
        IPosKeepingTrade IPosKeepingCommon.Trade
        {
            set { trade = (PosKeepingTrade)value; }
            get { return trade; }
        }
        bool IPosKeepingCommon.TradeSpecified
        {
            set { tradeSpecified = value; }
            get { return tradeSpecified; }
        }
        IPosKeepingMarket IPosKeepingCommon.Market
        {
            set { market = (PosKeepingMarket)value; }
            get { return market; }
        }
        bool IPosKeepingCommon.MarketSpecified
        {
            set { marketSpecified = value; }
            get { return marketSpecified; }
        }
        SQL_Instrument IPosKeepingCommon.Instrument
        {
            set { instrument = (SQL_Instrument)value; }
            get { return instrument; }
        }
        SQL_Product IPosKeepingCommon.Product
        {
            set { product = (SQL_Product)value; }
            get { return product; }
        }

        #endregion IPosKeepingCommon Members
        #region IPosKeepingKey Members
        int IPosKeepingKey.IdI
        {
            set { idI = value; }
            get { return idI; }
        }
        int IPosKeepingKey.IdAsset
        {
            set { idAsset = value; }
            get { return idAsset; }
        }
        int IPosKeepingKey.IdA_Dealer
        {
            set { idA_Dealer = value; }
            get { return idA_Dealer; }
        }
        int IPosKeepingKey.IdB_Dealer
        {
            set { idB_Dealer = value; }
            get { return idB_Dealer; }
        }
        int IPosKeepingKey.IdA_Clearer
        {
            set { idA_Clearer = value; }
            get { return idA_Clearer; }
        }
        int IPosKeepingKey.IdB_Clearer
        {
            set { idB_Clearer = value; }
            get { return idB_Clearer; }
        }
        int IPosKeepingKey.IdA_EntityDealer
        {
            set { idA_EntityDealer = value; }
            get { return idA_EntityDealer; }
        }
        int IPosKeepingKey.IdA_EntityClearer
        {
            set { idA_EntityClearer = value; }
            get { return idA_EntityClearer; }
        }
        #endregion IPosKeepingKey Members
    }
    #endregion PosKeepingCommon
    #region PosKeepingData
    /// <summary>
    /// <para>Classe de TRAVAIL utilisée principalement dans le service de Tenue de position</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de la classe abstrait PosKeepingCommon</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    /// EG 20140311 [19734][19702] Add m_isClearingEOD_BookDealer
    public partial class PosKeepingData : IPosKeepingData
    {
        #region Members
        private string m_positionEffect_BookDealer;
        private bool m_isIgnorePositionEffect_BookDealer;
        private bool m_isClearingEOD_BookDealer;
        #endregion Members

        #region Constructors
        public PosKeepingData() : base() { }
        #endregion Constructors

        #region IPosKeepingCommon Members
        PosKeepingAsset IPosKeepingCommon.Asset
        {
            set { asset = value; }
            get { return asset; }
        }
        IPosKeepingTrade IPosKeepingCommon.Trade
        {
            set { trade = (PosKeepingTrade)value; }
            get { return trade; }
        }
        bool IPosKeepingCommon.TradeSpecified
        {
            set { tradeSpecified = value; }
            get { return tradeSpecified; }
        }
        IPosKeepingMarket IPosKeepingCommon.Market
        {
            set { market = (PosKeepingMarket)value; }
            get { return market; }
        }
        bool IPosKeepingCommon.MarketSpecified
        {
            set { marketSpecified = value; }
            get { return marketSpecified; }
        }
        #endregion IPosKeepingCommon Members
        #region IPosKeepingKey Members
        int IPosKeepingKey.IdI
        {
            set { idI = value; }
            get { return idI; }
        }
        int IPosKeepingKey.IdAsset
        {
            set { idAsset = value; }
            get { return idAsset; }
        }
        int IPosKeepingKey.IdA_Dealer
        {
            set { idA_Dealer = value; }
            get { return idA_Dealer; }
        }
        int IPosKeepingKey.IdB_Dealer
        {
            set { idB_Dealer = value; }
            get { return idB_Dealer; }
        }
        int IPosKeepingKey.IdA_Clearer
        {
            set { idA_Clearer = value; }
            get { return idA_Clearer; }
        }
        int IPosKeepingKey.IdB_Clearer
        {
            set { idB_Clearer = value; }
            get { return idB_Clearer; }
        }
        int IPosKeepingKey.IdA_EntityDealer
        {
            set { idA_EntityDealer = value; }
            get { return idA_EntityDealer; }
        }
        int IPosKeepingKey.IdA_EntityClearer
        {
            set { idA_EntityClearer = value; }
            get { return idA_EntityClearer; }
        }
        #endregion IPosKeepingKey Members
        #region IPosKeepingData Members
        #region positionEffect_BookDealer
        public string PositionEffect_BookDealer
        {
            set { m_positionEffect_BookDealer = value; }
            get { return m_positionEffect_BookDealer; }
        }
        public bool IsIgnorePositionEffect_BookDealer
        {
            set { m_isIgnorePositionEffect_BookDealer = value; }
            get { return m_isIgnorePositionEffect_BookDealer; }
        }
        #endregion positionEffect_BookDealer
        #region clearingEOD_BookDealer
        /// EG 20140311 [19734][19702]
        public bool IsClearingEOD_BookDealer
        {
            set { m_isClearingEOD_BookDealer = value; }
            get { return m_isClearingEOD_BookDealer; }
        }
        #endregion clearingEOD_BookDealer

        #region CashSettlement
        /// <summary>
        /// Calcul du montant de CashSettlement lors d'un dénouement d'option
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20151001 [21414] Add Nullable to ClosingPrice and return
        // EG 20170127 Qty Long To Decimal
        Nullable<decimal> IPosKeepingData.CashSettlement(decimal pStrikePrice, Nullable<decimal> pClosingPrice, decimal pQuantity)
        {
            return asset.CashSettlement(pStrikePrice, pClosingPrice, pQuantity);
        }
        #endregion CashSettlement
        #region NominalValue
        /// <summary>
        /// Calcul du nominal sur futures et options 
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        decimal IPosKeepingData.NominalValue(decimal pQuantity)
        {
            return asset.NominalValue(pQuantity);
        }
        #endregion NominalValue
        #region RealizedMargin
        /// <summary>
        /// Calcul du montant de Realized margin sur les clôtures
        /// </summary>
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        Nullable<decimal> IPosKeepingData.RealizedMargin(Nullable<decimal> pClosedPrice, Nullable<decimal> pClosingPrice, decimal pQuantity)
        {
            return asset.RealizedMargin(pClosedPrice, pClosingPrice, pQuantity);
        }
        #endregion RealizedMargin
        #region VariationMargin
        /// <summary>
        /// Calcul du montant de Variation margin
        /// </summary>
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        Nullable<decimal> IPosKeepingData.VariationMargin(Nullable<decimal> pPrice, Nullable<decimal> pClosingPriceVeil, decimal pQuantity)
        {
            return asset.VariationMargin(pPrice, pClosingPriceVeil, pQuantity);
        }
        #endregion VariationMargin
        #region Set
        /// <summary>
        /// Alimentation des identifiants de l'netité du dealer et clearer
        /// </summary>
        void IPosKeepingData.Set(int pIdA_EntityDealer, int pIdA_EntityClearer)
        {
            idA_EntityDealer = pIdA_EntityDealer;
            idA_EntityClearer = pIdA_EntityClearer;
        }
        #endregion Set
        #region SetMarket
        /// <summary>
        /// Alimentation de la classe PosKeepingAsset par les caractéristiques du derivative contract
        /// </summary>
        //void IPosKeepingData.SetAsset(DateTime pMaturityDate, string pCategory,
        //    decimal pNominal, string pCurrency, decimal pStrikePrice, decimal pContractMultiplier,
        //    int pInstrumentNum, int pInstrumentDen, string pIdBC, string pPutCall, 
        //    string pAssetCategory, decimal pFactor, 
        //    DateTime pDeliveryDate, SettlMethodEnum pSettltMethod, IOffset pDeliveryDelayOffset)
        //{
        //    asset = new PosKeepingAsset(pMaturityDate, pCategory, pNominal, pCurrency, pStrikePrice, pContractMultiplier, 
        //        pInstrumentNum, pInstrumentDen, pIdBC, 
        //        pPutCall, pAssetCategory, pFactor, pDeliveryDate, pSettltMethod, pDeliveryDelayOffset);
        //}
        /// <summary>
        /// Alimentation de la classe PosKeepingMarket par les caractéristiques du marché
        /// </summary>
        /// PM 20150422 [20575] Add pDtEntityPrev, pDtEntity, pDtEntityNext
        void IPosKeepingData.SetMarket(int pIdA_Entity, int pIdA_CSS, int pIdEM, int pIdM, string pIdBC, DateTime pDtMarketPrev, DateTime pDtMarket, DateTime pDtMarketNext, DateTime pDtEntityPrev, DateTime pDtEntity, DateTime pDtEntityNext)
        {
            marketSpecified = true;
            market = new PosKeepingMarket(pIdA_Entity, pIdA_CSS, pIdEM, pIdM, pIdBC, pDtMarketPrev, pDtMarket, pDtMarketNext, pDtEntityPrev, pDtEntity, pDtEntityNext);
        }
        #endregion SetMarket
        #region SetTrade
        /// <summary>
        /// Alimentation de la classe PosKeepingTrade par les caractéristiques du trade
        /// </summary>
        // EG 20150716 [21103] Add pDtSettlt
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20171025 [23509] Add pDtExecution
        void IPosKeepingData.SetTrade(int pIdT, string pIdentifier, int pSide, decimal pQty, DateTime pDtBusiness, string pPositionEffect, DateTime pDtExecution, DateTime pDtSettlt)
        {
            tradeSpecified = true;
            trade = new PosKeepingTrade(pIdT, pIdentifier, pSide, pQty, pDtBusiness, pPositionEffect, pDtExecution, pDtSettlt, PositionEffect_BookDealer, IsIgnorePositionEffect_BookDealer);
        }
        #endregion SetTrade
        #region SetPosKey
        /// <summary>
        /// Alimentation de la classe PosKeepingTrade par les caractéristiques de la clé de position
        /// </summary>
        // EG 20180307 [23769] Gestion dbTransaction
        void IPosKeepingData.SetPosKey(string pCS, IDbTransaction pDbTransaction, int pIdI, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, int pIdAsset, int pIdA_Dealer, int pIdB_Dealer, int pIdA_Clearer, int pIdB_Clearer)
        {
            idI = pIdI;
            underlyingAsset = pUnderlyingAsset;
            idAsset = pIdAsset;
            idA_Dealer = pIdA_Dealer;
            idB_Dealer = pIdB_Dealer;
            idA_Clearer = pIdA_Clearer;
            idB_Clearer = pIdB_Clearer;

            if (0 < pIdI)
            {
                SQL_Instrument _instr = new SQL_Instrument(CSTools.SetCacheOn(pCS), pIdI)
                {
                    DbTransaction = pDbTransaction
                };
                if (_instr.IsLoaded)
                {
                    instrument = _instr;
                    SQL_Product _product = new SQL_Product(CSTools.SetCacheOn(pCS), SQL_TableWithID.IDType.Identifier, _instr.Product_Identifier)
                    {
                        DbTransaction = pDbTransaction
                    };
                    if (_product.IsLoaded)
                        product = _product;
                }
            }

        }
        #endregion SetPosKey
        #region SetAdditionalInfo
        void IPosKeepingData.SetAdditionalInfo(int pIdA_EntityDealer, int pIdA_EntityClearer)
        {
            idA_EntityDealer = pIdA_EntityDealer;
            idA_EntityClearer = pIdA_EntityClearer;
        }
        #endregion SetAdditionalInfo
        #region SetBookDealerInfo
        /// <summary>
        /// Initilisation des informations de Tenue de position du Book Dealer 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDtEnabled"></param>
        /// EG 20140311 [19734][19702]
        /// FI 20170908 [23409] Modify
        /// FI 20170913 [23417] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180426 Analyse du code Correction [CA2202]
        void IPosKeepingData.SetBookDealerInfo(string pCS, IDbTransaction pDbTransaction, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, DateTime pDtEnabled)
        {
            #region PL 20130319 New feature v3.3 - Identification de la méthode de tenue de la position
            //POSGLOP Voir pour ne faire cela que dans le cadre de la clôture/compensation
            int idDC = 0;
            string assetCategory = string.Empty;
            bool isOption = false;
            bool isIgnorePositionEffect_Book = false;
            bool isClearingEOD_Book = false;
            string positionEffect_Book = ExchangeTradedDerivativeTools.GetPositionEffect_FIFO();

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDB", DbType.Int32), this.idB_Dealer);
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);

            if (pUnderlyingAsset.HasValue && (pUnderlyingAsset.Value == Cst.UnderlyingAsset.ExchangeTradedContract))
            {
                SQL_AssetETD sqlAssetETD = new SQL_AssetETD(pCS, this.idAsset)
                {
                    DbTransaction = pDbTransaction
                };
                sqlAssetETD.LoadTable(new string[] { "IDDC", "PUTCALL", "ASSETCATEGORY" });
                if (sqlAssetETD.IsLoaded)
                {
                    idDC = sqlAssetETD.IdDerivativeContract;
                    isOption = !String.IsNullOrEmpty(sqlAssetETD.PutCall);
                    assetCategory = sqlAssetETD.DrvContract_AssetCategorie;
                }

                SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(pCS, pDbTransaction, this.idI, false, false, SQL_Table.ScanDataDtEnabledEnum.Yes);
                // EG 20160404 Migration vs2013
                // #warning PL 20140721 TBD - set IDM (see also TRIM 19480)
                //Ici, il n'y a pas urgence à renseigner IDM, car ce référentiel est (pour l'instant) réservé aux ETD.
                int idM = 0;

                // FI 20170913 [23417] Modify SQLContractCriteria utilisé à la place de SQLDerivativeContractCriteria
                //SQLDerivativeContractCriteria sqlDerivativeCriteria = new SQLDerivativeContractCriteria(pCS, idDC, idM, SQL_Table.ScanDataDtEnabledEnum.Yes);
                SQLContractCriteria sqlContractCriteria = new SQLContractCriteria(pCS, pDbTransaction,
                    new Pair<Cst.ContractCategory,int>(Cst.ContractCategory.DerivativeContract, idDC), idM, SQL_Table.ScanDataDtEnabledEnum.Yes);


                // EG 20140311 [19734][19702] Prise en compte de FUT/OPTCLEARINGEOD
                sqlSelect.AppendFormat("isnull(bpe.{0}POSEFFECT,b.{0}POSEFFECT) as POSEFFECT, ", (isOption ? "OPT" : "FUT"));
                sqlSelect.AppendFormat("isnull(bpe.IS{0}POSEFCTIGNORE,b.IS{0}POSEFCTIGNORE) as ISPOSEFCTIGNORE,", (isOption ? "OPT" : "FUT"));
                sqlSelect.AppendFormat("isnull(bpe.IS{0}CLEARINGEOD,b.IS{0}CLEARINGEOD) as ISCLEARINGEOD{1}", (isOption ? "OPT" : "FUT"), Cst.CrLf);

                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b" + Cst.CrLf;

                sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOKPOSEFCT.ToString() + " bpe on bpe.IDB=b.IDB" + Cst.CrLf;
                sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "bpe", pDtEnabled) + Cst.CrLf;
                sqlSelect += OTCmlHelper.GetSQLComment("Instrument filter");
                sqlSelect += SQLCst.AND + sqlInstrCriteria.GetSQLRestriction2("bpe", RoleGInstr.POSKEEPING);
                sqlSelect += OTCmlHelper.GetSQLComment("Market/Contract filter");
                sqlSelect += SQLCst.AND + sqlContractCriteria.GetSQLRestriction("bpe", RoleContractRestrict.POSKEEPING);
                if (StrFunc.IsFilled(assetCategory))
                {
                    parameters.Add(new DataParameter(pCS, "ASSETCATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), assetCategory);
                    sqlSelect += OTCmlHelper.GetSQLComment("Asset category filter");
                    sqlSelect += SQLCst.AND + "( (bpe.ASSETCATEGORY is null) or (bpe.ASSETCATEGORY=@ASSETCATEGORY) )" + Cst.CrLf;
                }
                sqlSelect += SQLCst.WHERE + "b.IDB=@IDB" + Cst.CrLf;
                
                sqlSelect += SQLCst.ORDERBY + @"case TYPECONTRACT 
                                            when 'DerivativeContract' then 1 
                                            when 'CommodityContract' then 1 
                                            when 'GrpContract' then 2 
                                            when 'Market' then 3  
                                            when 'GrpMarket' then 4 else 9 end" + Cst.CrLf;
                sqlSelect += "+ case when ASSETCATEGORY is not null then 11 else 19 end" + Cst.CrLf;
                sqlSelect += "+ case TYPEINSTR when 'INSTR' then 21 when 'GRPINSTR' then 22 else 29 end";
            }
            else
            {
                sqlSelect += "b.OTCPOSEFFECT as POSEFFECT, 1 as ISPOSEFCTIGNORE, 1 as ISCLEARINGEOD" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b" + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "b.IDB=@IDB" + Cst.CrLf;
            }

            QueryParameters qry = new QueryParameters(pCS, sqlSelect.ToString(), parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(qry.Cs, pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
            if (dr.Read())
            {
                if (dr["POSEFFECT"] != Convert.DBNull)
                {
                    positionEffect_Book = Convert.ToString(dr["POSEFFECT"]);
                    isIgnorePositionEffect_Book = Convert.ToBoolean(dr["ISPOSEFCTIGNORE"]);
                    isClearingEOD_Book = Convert.ToBoolean(dr["ISCLEARINGEOD"]);
                }
            }
            }
            PositionEffect_BookDealer = positionEffect_Book;
            IsIgnorePositionEffect_BookDealer = isIgnorePositionEffect_Book;
            IsClearingEOD_BookDealer = isClearingEOD_Book;
            #endregion
        }
        #endregion SetBookDealerInfo
        #region SetQuote
        /// <summary>
        /// Alimentation du prix de clôture (OfficialClose) de l'asset ou du sous-jacent
        /// </summary>
        /// EG 20140120 Report v3.7
        void IPosKeepingData.SetQuote(Cst.UnderlyingAsset pCategory, Quote pQuote, string pSource)
        {
            asset.SetQuote(pCategory, pQuote, pSource);
        }
        #endregion SetQuote
        #region SetQuoteReference
        /// <summary>
        /// Alimentation du prix de référence (OfficialSettlement) de l'asset ou du sous-jacent
        /// </summary>
        /// EG 20140120 Report v3.7
        void IPosKeepingData.SetQuoteReference(Cst.UnderlyingAsset pCategory, Quote pQuote, string pSource)
        {
            asset.SetQuoteReference(pCategory, pQuote, pSource);
        }
        #endregion SetQuoteReference
        #region ToBase100
        Nullable<decimal> IPosKeepingData.ToBase100(Nullable<decimal> pValue)
        {
            return asset.ToBase100(pValue);
        }
        decimal IPosKeepingData.ToBase100(decimal pValue)
        {
            return asset.ToBase100(pValue);
        }
        #endregion ToBase100
        #region ToBase100_UNL
        Nullable<decimal> IPosKeepingData.ToBase100_UNL(Nullable<decimal> pValue)
        {
            return asset.ToBase100_UNL(pValue);
        }
        decimal IPosKeepingData.ToBase100_UNL(decimal pValue)
        {
            return asset.ToBase100_UNL(pValue);
        }
        #endregion ToBase100_UNL
        #region VariableContractValue
        decimal IPosKeepingData.VariableContractValue(decimal pPrice)
        {
            return asset.VariableContractValue(pPrice);
        }
        decimal? IPosKeepingData.VariableContractValue(decimal? pPrice)
        {
            return asset.VariableContractValue(pPrice);
        }
        #endregion VariableContractValue
        #region VariableContractValue_UNL
        decimal IPosKeepingData.VariableContractValue_UNL(decimal pPrice)
        {
            return asset.VariableContractValue_UNL(pPrice);
        }
        decimal? IPosKeepingData.VariableContractValue_UNL(decimal? pPrice)
        {
            return asset.VariableContractValue_UNL(pPrice);
        }
        #endregion VariableContractValue_UNL

        #region Accessors
        int IPosKeepingData.IdA_Issuer
        {
            get { return asset.IdA_Issuer; }
        }
        int IPosKeepingData.IdB_Issuer
        {
            get { return asset.IdB_Issuer; }
        }

        string IPosKeepingData.Message
        {
            get
            {
                return string.Empty;
            }
        }
        // EG 20240115 [WI808] Traitement EOD : Harmonisation et réunification des méthodes
        (int id, string identifier, Cst.UnderlyingAsset underlyer) IPosKeepingData.AssetInfo
        {
            get { return (idAsset, asset.identifier, asset.UnderlyingAsset.Value); }
        }
        #endregion Accessors
        #endregion IPosKeepingData Members
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
    public partial class PosKeepingKey : IPosKeepingKey
    {
        #region Constructors
        public PosKeepingKey() { }
        #endregion Constructors
        #region IPosKeepingKey Members
        int IPosKeepingKey.IdI
        {
            set { idI = value; }
            get { return idI; }
        }
        Nullable<Cst.UnderlyingAsset> IPosKeepingKey.UnderlyingAsset
        {
            set { underlyingAsset = value; }
            get { return underlyingAsset; }
        }

        int IPosKeepingKey.IdAsset
        {
            set { idAsset = value; }
            get { return idAsset; }
        }
        int IPosKeepingKey.IdA_Dealer
        {
            set { idA_Dealer = value; }
            get { return idA_Dealer; }
        }
        int IPosKeepingKey.IdB_Dealer
        {
            set { idB_Dealer = value; }
            get { return idB_Dealer; }
        }
        int IPosKeepingKey.IdA_Clearer
        {
            set { idA_Clearer = value; }
            get { return idA_Clearer; }
        }
        int IPosKeepingKey.IdB_Clearer
        {
            set { idB_Clearer = value; }
            get { return idB_Clearer; }
        }
        int IPosKeepingKey.IdA_EntityDealer
        {
            set { idA_EntityDealer = value; }
            get { return idA_EntityDealer; }
        }
        int IPosKeepingKey.IdA_EntityClearer
        {
            set { idA_EntityClearer = value; }
            get { return idA_EntityClearer; }
        }
        // EG 20151102 [21465] New
        string IPosKeepingKey.LockObjectId
        {
            get
            {
                return String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    idI.ToString(),
                    idAsset.ToString(),
                    (underlyingAsset.HasValue ? underlyingAsset.Value.ToString() : string.Empty),
                    idA_Dealer.ToString(), idB_Dealer.ToString(), 
                    idA_Clearer.ToString(), idB_Clearer.ToString());
            }
        }
        #endregion IPosKeepingKey Members
    }
    #endregion PosKeepingKey
    #region PosKeepingMarket
    /// <summary>
    /// <para>Classe de travail contients diverses caractéristiques liées au MARKET</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► élément implicite de la clé de position</para>
    /// <para>► dates des dates de journées de bourse (PREVIOUS, CURRENT et NEXT)</para>
    /// <para>► business center</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    public partial class PosKeepingMarket : IPosKeepingMarket
    {
        #region Constructors
        public PosKeepingMarket() { }
        /// PM 20150422 [20575] Add pDtEntityPrev, pDtEntity, pDtEntityNext
        public PosKeepingMarket(int pIdA_Entity, int pIdA_CSS, int pIdEM, int pIdM, string pIdBC, DateTime pDtMarketPrev, DateTime pDtMarket, DateTime pDtMarketNext, DateTime pDtEntityPrev, DateTime pDtEntity, DateTime pDtEntityNext)
        {
            Set(pIdA_Entity, pIdA_CSS, pIdEM, pIdM, pIdBC, pDtMarketPrev, pDtMarket, pDtMarketNext, pDtEntityPrev, pDtEntity, pDtEntityNext);
        }
        #endregion Constructors
        #region Methods
        #region Set
        /// <summary>
        /// Alimentation de la classe
        /// </summary>
        /// <param name="pIdEM">Id entité/marché</param>
        /// <param name="pIdM">Id du marché</param>
        /// <param name="pIdBC">Business center du marché</param>
        /// <param name="pDtMarketPrev">Date de journée de bourse précédente pour le marché</param>
        /// <param name="pDtMarket">Date de journée de bourse actuelle pour le marché</param>
        /// <param name="pDtMarketNext">Date de journée de bourse suivante pour le marché</param>
        /// <param name="pDtEntityPrev">Date de journée de bourse précédente pour l'entité</param>
        /// <param name="pDtEntity">Date de journée de bourse actuelle pour l'entité</param>
        /// <param name="pDtEntityNext">Date de journée de bourse suivante pour l'entité</param>
        /// PM 20150422 [20575] Add pDtEntityPrev, pDtEntity, pDtEntityNext
        public void Set(int pIdA_Entity, int pIdA_CSS, int pIdEM, int pIdM, string pIdBC, DateTime pDtMarketPrev, DateTime pDtMarket, DateTime pDtMarketNext, DateTime pDtEntityPrev, DateTime pDtEntity, DateTime pDtEntityNext)
        {
            idA_Entity = pIdA_Entity;
            idA_CSS = pIdA_CSS;
            //idA_CustodianSpecified = pIdA_CSS;
            idEM = pIdEM;
            idM = pIdM;
            idBC = pIdBC;
            dtMarketPrev = pDtMarketPrev;
            dtMarket = pDtMarket;
            dtMarketNext = pDtMarketNext;
            dtEntityPrev = pDtEntityPrev;
            dtEntity = pDtEntity;
            dtEntityNext = pDtEntityNext;
        }
        #endregion Set
        #endregion Methods
        #region IPosKeepingMarket Members
        int IPosKeepingMarket.IdA_Entity
        {
            set { idA_Entity = value; }
            get { return idA_Entity; }
        }
        int IPosKeepingMarket.IdA_CSS
        {
            set { idA_CSS = value; }
            get { return idA_CSS; }
        }
        int IPosKeepingMarket.IdA_Custodian
        {
            set { idA_Custodian = value; }
            get { return idA_Custodian; }
        }
        bool IPosKeepingMarket.IdA_CustodianSpecified
        {
            set { idA_CustodianSpecified = value; }
            get { return idA_CustodianSpecified; }
        }
        int IPosKeepingMarket.IdEM
        {
            set { idEM = value; }
            get { return idEM; }
        }
        int IPosKeepingMarket.IdM
        {
            set { idM = value; }
            get { return idM; }
        }
        string IPosKeepingMarket.IdBC
        {
            set { idBC = value; }
            get { return idBC; }
        }
        DateTime IPosKeepingMarket.DtMarketPrev
        {
            set { dtMarketPrev = value; }
            get { return dtMarketPrev; }
        }
        DateTime IPosKeepingMarket.DtMarket
        {
            set { dtMarket = value; }
            get { return dtMarket; }
        }
        DateTime IPosKeepingMarket.DtMarketNext
        {
            set { dtMarketNext = value; }
            get { return dtMarketNext; }
        }
        DateTime IPosKeepingMarket.DtEntityPrev
        {
            set { dtEntityPrev = value; }
            get { return dtEntityPrev; }
        }
        DateTime IPosKeepingMarket.DtEntity
        {
            set { dtEntity = value; }
            get { return dtEntity; }
        }
        DateTime IPosKeepingMarket.DtEntityNext
        {
            set { dtEntityNext = value; }
            get { return dtEntityNext; }
        }
        // EG 20240520 [WI930] New
        string IPosKeepingMarket.IdA_CSS_Identifier
        {
            set { idA_Css_Identifier = value; }
            get { return idA_Css_Identifier; }
        }
        // EG 20240520 [WI930] New
        string IPosKeepingMarket.IdBCEntity
        {
            set { idBCEntity = value; }
            get { return idBCEntity; }
        }
        // EG 20240520 [WI930] New
        string IPosKeepingMarket.IdM_Identifier
        {
            set { idM_Identifier = value; }
            get { return idM_Identifier; }
        }
        #endregion
    }
    #endregion PosKeepingMarket
    #region PosKeepingQuote
    /// <summary>
    /// <para>Classe de travail contients les caractéristiques d'une cotation d'un asset</para>
    /// </summary>
    /// EG 20140120 Report v3.7 [19356]
    public partial class PosKeepingQuote : IPosKeepingQuote
    {
        #region Constructors
        public PosKeepingQuote() { }
        /// EG 20140120 Report v3.7
        public PosKeepingQuote(Cst.UnderlyingAsset pCategory, int pIdAsset, string pIdentifier, QuotationSideEnum pQuoteSide, QuoteTimingEnum pQuoteTiming, DateTime pQuoteTime, decimal pQuotePrice, string pSource)
        {
            category = pCategory;
            idAsset = pIdAsset;
            identifier = pIdentifier;
            quoteSide = pQuoteSide;
            quoteTiming = pQuoteTiming;
            quoteTime = pQuoteTime;
            quotePrice = pQuotePrice;
            source = pSource;
        }
        #endregion Constructors
        #region IPosKeepingQuote Members
        Cst.UnderlyingAsset IPosKeepingQuote.Category
        {
            set { category = value; }
            get { return category; }
        }
        int IPosKeepingQuote.IdAsset
        {
            set { idAsset = value; }
            get { return idAsset; }
        }
        string IPosKeepingQuote.Identifier
        {
            set { identifier = value; }
            get { return identifier; }
        }
        QuotationSideEnum IPosKeepingQuote.QuoteSide
        {
            set { quoteSide = value; }
            get { return quoteSide; }
        }
        QuoteTimingEnum IPosKeepingQuote.QuoteTiming
        {
            set { quoteTiming = value; }
            get { return quoteTiming; }
        }
        DateTime IPosKeepingQuote.QuoteTime
        {
            set { quoteTime = value; }
            get { return quoteTime; }
        }
        decimal IPosKeepingQuote.QuotePrice
        {
            set { quotePrice = value; }
            get { return quotePrice; }
        }
        string IPosKeepingQuote.Source
        {
            set { source = value; }
            get { return source; }
        }
        #endregion IPosKeepingQuote Members
    }
    #endregion PosKeepingQuote

    #region PosKeepingTrade
    /// <summary>
    /// <para>Classe de travail contients diverses caractéristiques liées à un trade dans un traitement de tenue de position</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques essentielles (sens,PositionEffect, quantité, dtBusiness, dtTimeStamp...</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    public partial class PosKeepingTrade : IPosKeepingTrade
    {
        #region Constructors
        public PosKeepingTrade() { }
        // EG 20150716 [21103] Add pDtSettlt
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
        public PosKeepingTrade(int pIdT, string pIdentifier, int pSide, decimal pQty, DateTime pDtBusiness, string pPositionEffect, DateTime pDtExecution, DateTime pDtSettlt)
        {
            Set(pIdT, pIdentifier, pSide, pQty, pDtBusiness, pPositionEffect, pDtExecution, pDtSettlt);
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
        public PosKeepingTrade(int pIdT, string pIdentifier, int pSide, decimal pQty, DateTime pDtBusiness, string pPositionEffect, DateTime pDtExecution, DateTime pDtSettlt, 
            string pPositionEffect_BookDealer, bool pIsIgnorePositionEffect_BookDealer)
            : this(pIdT, pIdentifier, pSide, pQty, pDtBusiness, pPositionEffect, pDtExecution, pDtSettlt)
        {
            SetBookPositionEffect(pPositionEffect_BookDealer, pIsIgnorePositionEffect_BookDealer);
        }
        #endregion Constructors
        #region Methods
        #region Set
        /// <summary>
        /// Alimentation de la classe
        /// </summary>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pIdentifier">identifiant du trade</param>
        /// <param name="pSide">Sens</param>
        /// <param name="pQty">Quantité initiale</param>
        /// <param name="pDtBusiness">Date de compensation</param>
        /// <param name="pPositionEffect">Ouverture/Clôture</param>
        /// <param name="pDtTimeStamp">Horodatage</param>
        /// <param name="pDtExecution">Date Exécution</param>
        /// <param name="pDtSettlt">Date de Settlement</param>
        // EG 20150716 [21103] Add pDtSettlt
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
        public void Set(int pIdT, string pIdentifier, int pSide, decimal pQty, DateTime pDtBusiness, string pPositionEffect, DateTime pDtExecution, DateTime pDtSettlt)
        {
            idT = pIdT;
            identifier = pIdentifier;
            side = pSide;
            qty = pQty;
            dtBusiness = pDtBusiness;
            positionEffect = pPositionEffect;
            dtExecution = pDtExecution;
            dtSettlt = pDtSettlt;
        }
        public void SetBookPositionEffect(string pPositionEffect_BookDealer, bool pIsIgnorePositionEffect_BookDealer)
        {
            positionEffect_BookDealer = pPositionEffect_BookDealer;
            isIgnorePositionEffect_BookDealer = pIsIgnorePositionEffect_BookDealer;
        }
        #endregion Set
        #endregion Methods
        #region IPosKeepingTrade Members
        int IPosKeepingTrade.IdT
        {
            set { idT = value; }
            get { return idT; }
        }
        string IPosKeepingTrade.Identifier
        {
            set { identifier = value; }
            get { return identifier; }
        }
        int IPosKeepingTrade.Side
        {
            set { side = value; }
            get { return side; }
        }
        // EG 20170127 Qty Long To Decimal
        decimal IPosKeepingTrade.Qty
        {
            set { qty = value; }
            get { return qty; }
        }
        DateTime IPosKeepingTrade.DtBusiness
        {
            set { dtBusiness = value; }
            get { return dtBusiness; }
        }
        // EG 20150716 [21103] New
        DateTime IPosKeepingTrade.DtSettlt
        {
            set { dtSettlt = value; }
            get { return dtSettlt; }
        }
        DateTime IPosKeepingTrade.DtExecution
        {
            set { dtExecution = value; }
            get { return dtExecution; }
        }
        string IPosKeepingTrade.PositionEffect
        {
            set { positionEffect = value; }
            get { return positionEffect; }
        }
        string IPosKeepingTrade.PositionEffect_BookDealer
        {
            set { positionEffect_BookDealer = value; }
            get { return positionEffect_BookDealer; }
        }
        bool IPosKeepingTrade.IsIgnorePositionEffect_BookDealer
        {
            set { isIgnorePositionEffect_BookDealer = value; }
            get { return isIgnorePositionEffect_BookDealer; }
        }
        #endregion IPosKeepingTrade Members
    }
    #endregion PosKeepingTrade

    #region PosKeepingClearingTrade
    /// <summary>
    /// <para>Classe contenant la liste des trades liés à une clôture spécifique</para>
    /// </summary>
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
    public partial class PosKeepingClearingTrade : IPosKeepingClearingTrade
    {
        #region Constructors
        public PosKeepingClearingTrade() { }
        public PosKeepingClearingTrade(int pIdT, string pIdentifier, decimal pAvailableQty, decimal pClosableQty, DateTime pDtBusiness, DateTime pDtExecution)
        {
            Set(pIdT, pIdentifier, pAvailableQty, pClosableQty, pDtBusiness, pDtExecution);
        }
        #endregion Constructors
        #region Methods
        #region Set
        /// <summary>
        /// Alimentation de la classe
        /// </summary>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pIdentifier">identifiant du trade</param>
        /// <param name="pAvailableQty">Quantité disponible</param>
        /// <param name="pClosingQty">Quantité clôturée</param>
        /// <param name="pDtBusiness">Date de compensation</param>
        /// <param name="pDtTimeStamp">Horodatage</param>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
        public void Set(int pIdT, string pIdentifier, decimal pAvailableQty, decimal pClosableQty, DateTime pDtBusiness, DateTime pDtExecution)
        {
            idT = pIdT;
            identifier = pIdentifier;
            availableQty = pAvailableQty;
            closableQty = pClosableQty;
            dtBusiness = pDtBusiness;
            dtExecution = pDtExecution;
        }
        #endregion Set
        #endregion Methods
        #region IPosKeepingClearingTrade Members
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        int IPosKeepingClearingTrade.IdT
        {
            set { idT = value; }
            get { return idT; }
        }
        string IPosKeepingClearingTrade.Identifier
        {
            set { identifier = value; }
            get { return identifier; }
        }
        // EG 20170127 Qty Long To Decimal
        decimal IPosKeepingClearingTrade.AvailableQty
        {
            set { availableQty = value; }
            get { return availableQty; }
        }
        // EG 20170127 Qty Long To Decimal
        decimal IPosKeepingClearingTrade.ClosableQty
        {
            set { closableQty = value; }
            get { return closableQty; }
        }
        DateTime IPosKeepingClearingTrade.DtBusiness
        {
            set { dtBusiness = value; }
            get { return dtBusiness; }
        }
        DateTime IPosKeepingClearingTrade.DtExecution
        {
            set { dtExecution = value; }
            get { return dtExecution; }
        }
        #endregion IPosKeepingClearingTrade Members
    }
    #endregion PosKeepingClosingTrade
    #region SplitNewTrade
    /// <summary>
    /// Classe contenant les caractéristiques d'un trade résultant d'un Splitting
    /// </summary>
    public partial class SplitNewTrade
    {
        #region Constructors
        public SplitNewTrade() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public SplitNewTrade(string pIdStActivation, string pActorIdentifier, int pIdA, string pBookIdentifier, int pIdB, decimal pQty, string pPosEfct)
        {
            IdStActivation = pIdStActivation;
            ActorIdentifier = pActorIdentifier;
            IdA = pIdA;
            BookIdentifier = pBookIdentifier;
            IdB = pIdB;
            Qty = pQty;
            PosEfct = pPosEfct;
        }
        #endregion Constructors
    }
    #endregion SplitNewTrade

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
    /// EG 20150317 [POC] Add GProduct
    /// EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
    public partial class PosRequest : IPosRequest
    {
        #region Accessors
        public virtual string RequestMessage
        {
            get { return null; }
        }
        // EG 20130313 (POC] New
        // EG 20211012 [XXXXX] New Initialisation gProduct
        public string GProduct
        {
            get
            {
                string gProduct = ((IPosRequest)this).GroupProductValue;
                if (idA_CustodianSpecified)
                {
                    //if (idMSpecified && (idM < 0))
                    //    gProduct = Cst.ProductGProduct_MTM;
                    if (idMSpecified && (idM < 0))
                        gProduct = ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(ProductTools.GroupProductEnum.MarkToMarket);
                }
                else
                {
                    gProduct = Cst.ProductGProduct_FUT;
                }
                return gProduct;
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequest() { }
        public PosRequest(Cst.PosRequestTypeEnum pRequestType)
            : this(pRequestType, SettlSessIDEnum.Intraday)
        {
        }

        public PosRequest(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode)
        {
            requestType = pRequestType;
            requestMode = pRequestMode;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequest(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
            : this(pRequestType, pRequestMode)
        {
            dtBusiness = pDtBusiness;
            qty = pQty;
            qtySpecified = true;
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Construction du message de traitement (Envoi et/ou réception) à afficher
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► par clé de position</para>
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pAdditionalMessage">Message complémentaire</param>
        /// <returns>message complet à afficher</returns>
        protected string RequestMessageByKeyPos(string pAdditionalMessage)
        {
            string message = Ressource.GetString(requestType.ToString());
            if (identifiersSpecified)
                message = Ressource.GetString2("Msg_PosRequestByKeyPos", message,
                identifiers.entity, identifiers.cssCustodian, identifiers.market, identifiers.instrument, identifiers.asset,
                identifiers.dealer + " [" + identifiers.bookDealer + "]",
                identifiers.clearer + " [" + identifiers.bookClearer + "]",
                pAdditionalMessage);
            return message;
        }
        /// <summary>
        /// Construction du message de traitement (Envoi et/ou réception) à afficher
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► par trade</para>
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pAdditionalMessage">Message complémentaire</param>
        /// <returns>message complet à afficher</returns>
        protected string RequestMessageByTrade(string pAdditionalMessage)
        {
            string message = Ressource.GetString(requestType.ToString());
            if (identifiersSpecified)
                message = Ressource.GetString2("Msg_PosRequestByTrade", message, identifiers.trade, pAdditionalMessage);
            return message;
        }
        /// <summary>
        /// Construction du message de traitement (Envoi et/ou réception) à afficher
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► par Entité/Chambre/Marché</para>
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pAdditionalMessage">Message complémentaire</param>
        /// <returns>message complet à afficher</returns>
        protected string RequestMessageByEntityMarket(string pAdditionalMessage)
        {
            string message = Ressource.GetString(requestType.ToString());
            if (identifiersSpecified)
                message = Ressource.GetString2("Msg_PosRequestByEntityMarket", message,
                identifiers.entity, identifiers.cssCustodian, identifiers.market, DtFunc.DateTimeToString(dtBusiness, DtFunc.FmtDateTime), pAdditionalMessage);
            return message;
        }
        /// <summary>
        /// Construction du message de traitement (Envoi et/ou réception) à afficher
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► pour une traitement de fin de journée</para>
        /// <para>► pour une clôture de journée</para>
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pAdditionalMessage">Message complémentaire</param>
        /// <returns>message complet à afficher</returns>
        protected string RequestMessageByEntityCSS(string pAdditionalMessage)
        {
            string message = Ressource.GetString(requestType.ToString());
            if (identifiersSpecified)
                message = Ressource.GetString2("Msg_PosRequestByEntityCSS", message,
                identifiers.entity, identifiers.cssCustodian, DtFunc.DateTimeToString(dtBusiness, DtFunc.FmtDateTime), pAdditionalMessage);
            return message;
        }
        /// <summary>
        /// Construction du message de traitement (Envoi et/ou réception) à afficher
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► par Chambre/Marché/Corporate action</para>
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pAdditionalMessage">Message complémentaire</param>
        /// <returns>message complet à afficher</returns>
        /// EG 20130607 [18740] Add RemoveCAExecuted
        protected string RequestMessageByCorporateAction(string pAdditionalMessage)
        {
            string message = Ressource.GetString(requestType.ToString());
            if (identifiersSpecified)
                message = Ressource.GetString2("Msg_PosRequestByCorporateAction", message,
                identifiers.cssCustodian, identifiers.market, identifiers.corporateAction, DtFunc.DateTimeToString(dtBusiness, DtFunc.FmtDateTime), pAdditionalMessage);
            return message;
        }
        #endregion Methods
        #region IPosRequest Members
        // EG 20171128 [23331]
        object IPosRequest.CloneMain()
        {
            IPosRequest clone = (PosRequest)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneField);
            return clone;
        }

        bool IPosRequest.GroupProductSpecified
        {
            set { groupProductSpecified = value; }
            get { return groupProductSpecified; }
        }

        ProductTools.GroupProductEnum IPosRequest.GroupProduct
        {
            set { groupProduct = value; }
            get { return groupProduct; }
        }

        string IPosRequest.GroupProductValue
        {
            get { return groupProductSpecified ? ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(groupProduct) : "-"; }
        }
        Nullable<ProductTools.GroupProductEnum> IPosRequest.GroupProductEnum
        {
            get 
            {
                if (groupProductSpecified)
                    return groupProduct;
                else
                    return null; 
            }
        }
        
        int IPosRequest.IdPR
        {
            set { idPR = value; }
            get { return idPR; }
        }
        int IPosRequest.IdPR_PosRequest
        {
            set { idPR_PosRequest = value; }
            get { return idPR_PosRequest; }
        }
        bool IPosRequest.IdPR_PosRequestSpecified
        {
            set { idPR_PosRequestSpecified = value; }
            get { return idPR_PosRequestSpecified; }
        }
        Cst.PosRequestTypeEnum IPosRequest.RequestType
        {
            set { requestType = value; }
            get { return requestType; }
        }
        SettlSessIDEnum IPosRequest.RequestMode
        {
            set { requestMode = value; }
            get { return requestMode; }
        }
        int IPosRequest.IdA_Entity
        {
            set { idA_Entity = value; }
            get { return idA_Entity; }
        }
        int IPosRequest.IdA_CssCustodian
        {
            set { idA_CssCustodian = value; }
            get { return idA_CssCustodian; }
        }
        int IPosRequest.IdA_Css
        {
            set { idA_Css = value; }
            get { return idA_Css; }
        }
        bool IPosRequest.IdA_CssSpecified
        {
            set { idA_CssSpecified = value; }
            get { return idA_CssSpecified; }
        }
        int IPosRequest.IdA_Custodian
        {
            set { idA_Custodian = value; }
            get { return idA_Custodian; }
        }
        bool IPosRequest.IdA_CustodianSpecified
        {
            set { idA_CustodianSpecified = value; }
            get { return idA_CustodianSpecified; }
        }
        int IPosRequest.IdEM
        {
            set { idEM = value; }
            get { return idEM; }
        }
        bool IPosRequest.IdEMSpecified
        {
            set { idEMSpecified = value; }
            get { return idEMSpecified; }
        }
        int IPosRequest.IdM
        {
            set { idM = value; }
            get { return idM; }
        }
        bool IPosRequest.IdMSpecified
        {
            set { idMSpecified = value; }
            get { return idMSpecified; }
        }
        int IPosRequest.IdCE
        {
            set { idCE = value; }
            get { return idCE; }
        }
        bool IPosRequest.IdCESpecified
        {
            set { idCESpecified = value; }
            get { return idCESpecified; }
        }
        DateTime IPosRequest.DtBusiness
        {
            set { dtBusiness = value; }
            get { return dtBusiness; }
        }
        IPosKeepingKey IPosRequest.PosKeepingKey
        {
            set { posKeepingKey = (PosKeepingKey)value; }
            get { return posKeepingKey; }
        }
        bool IPosRequest.PosKeepingKeySpecified
        {
            set { posKeepingKeySpecified = value; }
            get { return posKeepingKeySpecified; }
        }
        int IPosRequest.IdT
        {
            set { idT = value; }
            get { return idT; }
        }
        bool IPosRequest.IdTSpecified
        {
            set { idTSpecified = value; }
            get { return idTSpecified; }
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequest.Qty
        {
            set { qty = value; }
            get { return qty; }
        }
        bool IPosRequest.QtySpecified
        {
            set { qtySpecified = value; }
            get { return qtySpecified; }
        }
        string IPosRequest.Notes
        {
            set { notes = value; }
            get { return notes; }
        }
        bool IPosRequest.NotesSpecified
        {
            set { notesSpecified = value; }
            get { return notesSpecified; }
        }
        IPosRequestKeyIdentifier IPosRequest.Identifiers
        {
            set { identifiers = (PosRequestKeyIdentifier)value; }
            get { return identifiers; }
        }
        bool IPosRequest.IdentifiersSpecified
        {
            set { identifiersSpecified = value; }
            get { return identifiersSpecified; }
        }
        object IPosRequest.DetailBase
        {
            set { ;}
            get { return null; }
        }
        Type IPosRequest.DetailType
        {
            get { return null; }
        }
        int IPosRequest.IdAIns
        {
            set { idAIns = value; }
            get { return idAIns; }
        }
        DateTime IPosRequest.DtIns
        {
            set { dtIns = value; }
            get { return dtIns; }
        }
        int IPosRequest.IdAUpd
        {
            set { idAUpd = value; }
            get { return idAUpd; }
        }
        bool IPosRequest.IdAUpdSpecified
        {
            set { idAUpdSpecified = value; }
            get { return idAUpdSpecified; }
        }
        DateTime IPosRequest.DtUpd
        {
            set { dtUpd = value; }
            get { return dtUpd; }
        }
        bool IPosRequest.DtUpdSpecified
        {
            set { dtUpdSpecified = value; }
            get { return dtUpdSpecified; }
        }
        ProcessStateTools.StatusEnum IPosRequest.Status
        {
            set { status = value; }
            get { return status; }
        }
        bool IPosRequest.StatusSpecified
        {
            set { statusSpecified = value; }
            get { return statusSpecified; }
        }

        int IPosRequest.IdProcessL
        {
            set { idProcessL = value; }
            get { return idProcessL; }
        }
        bool IPosRequest.IdProcessLSpecified
        {
            set { idProcessLSpecified = value; }
            get { return idProcessLSpecified; }
        }

        string IPosRequest.Source
        {
            set { source = value; }
            get { return source; }
        }
        bool IPosRequest.SourceSpecified
        {
            set { sourceSpecified = value; }
            get { return sourceSpecified; }
        }
        int IPosRequest.SourceIdProcessL
        {
            set { sourceIdProcessL = value; }
            get { return sourceIdProcessL; }
        }
        bool IPosRequest.SourceIdProcessLSpecified
        {
            set { sourceIdProcessLSpecified = value; }
            get { return sourceIdProcessLSpecified; }
        }

        string IPosRequest.ExtlLink
        {
            set { extlLink = value; }
            get { return extlLink; }
        }
        bool IPosRequest.ExtlLinkSpecified
        {
            set { extlLinkSpecified = value; }
            get { return extlLinkSpecified; }
        }
        // EG 20130313 (POC] New
        string IPosRequest.GProduct
        {
            get {return this.GProduct;}
        }

        int IPosRequest.NbTokenIdE
        {
            get { return 0; }
        }
        string IPosRequest.RequestMessage
        {
            get { return RequestMessage; }
        }
        // EG 20130523 New LockMode
        string IPosRequest.LockModeTrade
        {
            get
            {
                string _lockMode = LockTools.Exclusive;
                if (idTSpecified)
                    _lockMode = LockTools.Shared;
                return _lockMode;
            }
        }
        // EG 20130523 New LockMode
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Add Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
        string IPosRequest.LockModeEntityMarket
        {
            get
            {
                string _lockMode = LockTools.Shared;
                if (false == idTSpecified)
                {
                    switch (requestType)
                    {
                        case Cst.PosRequestTypeEnum.EndOfDay:
                        case Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin:
                        case Cst.PosRequestTypeEnum.ClosingDay:
                            _lockMode = LockTools.Exclusive;
                            break;
                        default:
                            _lockMode = LockTools.Shared;
                            break;
                    }
                }
                return _lockMode;
            }
        }
        /// <summary>
        /// Utilisé pour homogénéiser les traitements de Correction|Transfert|Dénouement d'option
        /// - Retour la quantité demandée pour le traitement, la quantité disponible et le détail (XML) du message
        /// </summary>
        // EG 20240115 [WI808] Traitement EOD : Harmonisation et réunification des méthodes
        (decimal qty, decimal availableQty, IPosRequestDetail detail) IPosRequest.GetDetailForAction
        {
            get 
            {
                decimal availableQty = qty;
                IPosRequestDetail detail = (IPosRequestDetail)((IPosRequest)this).DetailBase;
                if (detail is IPosRequestDetOption option)
                    availableQty = option.AvailableQty;
                else if (detail is IPosRequestDetCorrection correction)
                    availableQty = correction.AvailableQty;
                else if (detail is IPosRequestDetTransfer transfer)
                    availableQty = transfer.PositionQty;
                return (qty, availableQty, detail);
            }
        }

        void IPosRequest.SetIdA_CssCustodian(int pIdA_CssCustodian, bool pIsCustodian)
        {
            //this.SetIdA_CssCustodian(pIdA_CssCustodian, pIsCustodian);
            idA_CssCustodian = pIdA_CssCustodian;
            idA_CssSpecified = (false == pIsCustodian);
            if (idA_CssSpecified)
                idA_Css = idA_CssCustodian;
            idA_CustodianSpecified = pIsCustodian;
            if (idA_CustodianSpecified)
                idA_Custodian = idA_CssCustodian;

        }
        void IPosRequest.SetIdA_CssCustodian(Nullable<int> pIdA_Css, Nullable<int> pIdA_Custodian)
        {
            idA_CssSpecified = pIdA_Css.HasValue && (0 < pIdA_Css.Value);
            if (idA_CssSpecified)
                idA_Css = pIdA_Css.Value;

            idA_CustodianSpecified = pIdA_Custodian.HasValue && (0 < pIdA_Custodian.Value);
            if (idA_CustodianSpecified)
            {
                idA_Custodian = pIdA_Custodian.Value;
                idA_CssCustodian = idA_Custodian;
            }
            else
            {
                idA_CssCustodian = idA_Css;
            }
        }
        void IPosRequest.SetPosKey(int pIdI, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, int pIdAsset, int pIdA_Dealer, int pIdB_Dealer, int pIdA_Clearer, int pIdB_Clearer)
        {
            posKeepingKey = new PosKeepingKey
            {
                idI = pIdI,
                underlyingAsset = pUnderlyingAsset,
                idAsset = pIdAsset,
                idA_Dealer = pIdA_Dealer,
                idB_Dealer = pIdB_Dealer,
                idA_Clearer = pIdA_Clearer,
                idB_Clearer = pIdB_Clearer
            };
            posKeepingKeySpecified = true;
        }

        void IPosRequest.SetAdditionalInfo(int pIdA_EntityDealer, int pIdA_EntityClearer)
        {
            if (posKeepingKeySpecified)
            {
                posKeepingKey.idA_EntityDealer = pIdA_EntityDealer;
                posKeepingKey.idA_EntityClearer = pIdA_EntityClearer;
            }
        }
        void IPosRequest.SetIdentifiers(string pEntity, string pCssCustodian, string pMarket,
            string pInstrument, string pAsset,
            string pDealer, string pBookDealer,
            string pClearer, string pBookClearer)
        {
            identifiers = new PosRequestKeyIdentifier
            {
                entity = pEntity,
                cssCustodian = pCssCustodian,
                market = pMarket,
                instrument = pInstrument,
                asset = pAsset,
                dealer = pDealer,
                bookDealer = pBookDealer,
                clearer = pClearer,
                bookClearer = pBookClearer
            };
            identifiersSpecified = true;
        }
        void IPosRequest.SetIdentifiers(string pMarket,
            string pInstrument, string pAsset, string pDealer, string pBookDealer, string pClearer, string pBookClearer)
        {
            identifiers = new PosRequestKeyIdentifier
            {
                market = pMarket,
                instrument = pInstrument,
                asset = pAsset,
                dealer = pDealer,
                bookDealer = pBookDealer,
                clearer = pClearer,
                bookClearer = pBookClearer
            };
            identifiersSpecified = true;
        }
        void IPosRequest.SetIdentifiers(string pTrade)
        {
            identifiers = new PosRequestKeyIdentifier
            {
                trade = pTrade
            };
            identifiersSpecified = true;
        }
        void IPosRequest.SetIdentifiers(string pEntity, string pCssCustodian)
        {
            identifiers = new PosRequestKeyIdentifier
            {
                entity = pEntity,
                cssCustodian = pCssCustodian
            };
            identifiersSpecified = true;
        }
        void IPosRequest.SetIdentifiers(string pEntity, string pCssCustodian, string pMarket)
        {
            identifiers = new PosRequestKeyIdentifier
            {
                entity = pEntity,
                cssCustodian = pCssCustodian,
                market = pMarket
            };
            identifiersSpecified = true;
        }
        void IPosRequest.SetNotes(string pNotes)
        {
            notesSpecified = StrFunc.IsFilled(pNotes);
            if (notesSpecified)
                notes = pNotes;
        }
        // EG 20151102 [21465] New
        void IPosRequest.SetSource(AppInstance pAppInstance)
        {
            // La demande existe déjà => on la met à jour
            sourceSpecified = true;
            source = pAppInstance.AppNameVersion;
            if (pAppInstance.GetType().Equals(typeof(AppInstanceService)))
                source = ((AppInstanceService)pAppInstance).ServiceName;
            else
                source = pAppInstance.AppNameVersion;
        }
        /// <summary>
        /// Retourne MQueueIdInfo d'un POSREQUEST
        /// </summary>
        /// <param name="pGProduct"></param>
        /// <returns></returns>
        /// FI 20130917 [18953] le dictionnaire idInfo.idInfos ne contient plus de item non renseigné
        /// EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
        IdInfo IPosRequest.GetMQueueIdInfo()
        {
            IdInfo idInfo = new IdInfo();
            if (idTSpecified)
            {
                //FI 20130917 [18953] add variable len
                idInfo.id = idT;
                idInfo.idInfos = new DictionaryEntry[4];
                // EG 20150313 (POC] New GProduct remplace pGProduct
                // EG 20170223 groupProductValue remplace  GProduct
                idInfo.idInfos[0] = new DictionaryEntry("GPRODUCT", ((IPosRequest)this).GroupProductValue);
                idInfo.idInfos[1] = new DictionaryEntry("REQUESTTYPE", requestType);
                idInfo.idInfos[2] = new DictionaryEntry("DTBUSINESS", dtBusiness);

                if (identifiersSpecified)
                    idInfo.idInfos[3] = new DictionaryEntry("TRADE", identifiers.trade);
            }
            else
            {
                //FI 20130917 [18953] add variable len
                int len = identifiersSpecified ? 12 : 3;
                idInfo.id = idPR;
                idInfo.idInfos = new DictionaryEntry[len];
                idInfo.idInfos[0] = new DictionaryEntry("GPRODUCT", GProduct);
                idInfo.idInfos[1] = new DictionaryEntry("REQUESTTYPE", requestType);
                idInfo.idInfos[2] = new DictionaryEntry("DTBUSINESS", dtBusiness);
                if (identifiersSpecified)
                {
                    idInfo.idInfos[3] = new DictionaryEntry("ENTITY", identifiers.entity);
                    idInfo.idInfos[4] = new DictionaryEntry("CSSCUSTODIAN", identifiers.cssCustodian);
                    idInfo.idInfos[5] = new DictionaryEntry("MARKET", identifiers.market);
                    idInfo.idInfos[6] = new DictionaryEntry("INSTRUMENT", identifiers.instrument);
                    idInfo.idInfos[7] = new DictionaryEntry("ASSET", identifiers.asset);
                    idInfo.idInfos[8] = new DictionaryEntry("DEALER", identifiers.dealer);
                    idInfo.idInfos[9] = new DictionaryEntry("BOOKDEALER", identifiers.bookDealer);
                    idInfo.idInfos[10] = new DictionaryEntry("CLEARER", identifiers.clearer);
                    idInfo.idInfos[11] = new DictionaryEntry("BOOKCLEARER", identifiers.bookClearer);
                }
            }
            return idInfo;
        }
        #endregion IPosRequest Members

        #region methods
        IPosRequestTradeAdditionalInfo IPosRequest.CreateAdditionalInfoStandard(DataDocumentContainer pTemplateDataDocument,
            SQL_TradeCommon pSQLTrade, SQL_Product pSQLProduct, SQL_Instrument pSQLInstrument, string pScreenName)
        {
            return PosRequestTradeAdditionalInfo.CreateAdditionalInfo(pTemplateDataDocument, pSQLTrade, pSQLProduct, pSQLInstrument, pScreenName);
        }
        /// EG 20160302 (21969] New
        void IPosRequest.SetStatus(Cst.ErrLevel pErrLevel)
        {
            switch (pErrLevel)
            {
                case Cst.ErrLevel.SUCCESS:
                case Cst.ErrLevel.DATAIGNORE:
                    status = ProcessStateTools.StatusEnum.SUCCESS;
                    break;
                case Cst.ErrLevel.NOTHINGTODO:
                    status = ProcessStateTools.StatusEnum.NONE;
                    break;
                case Cst.ErrLevel.FAILUREWARNING:
                    status = ProcessStateTools.StatusEnum.WARNING;
                    break;
                default:
                    status = ProcessStateTools.StatusEnum.ERROR;
                    break;
            }
            statusSpecified = true;
        }
        #endregion methods

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
    public partial class PosRequestGroupLevel : IPosRequestGroupLevel
    {
        #region Accessors
        public override string RequestMessage
        {
            get { return RequestMessageByEntityMarket("Msg_PosRequestGroupLevel"); }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestGroupLevel() : base() { }
        public PosRequestGroupLevel(Cst.PosRequestTypeEnum pRequestType, int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness)
            : base(pRequestType, SettlSessIDEnum.None, pDtBusiness, 0)
        {
            ((IPosRequest)this).SetIdA_CssCustodian(pIdA_Css, pIdA_Custodian);
            idA_Entity = pIdA_Entity;
            idEM = pIdEM;
            idEMSpecified = (0 < pIdEM);
        }
        #endregion Constructors
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
    public partial class PosRequestKeyIdentifier : IPosRequestKeyIdentifier
    {
        #region Constructors
        public PosRequestKeyIdentifier() { }
        #endregion Constructors
        #region IPosRequestKeyIdentifier Members
        string IPosRequestKeyIdentifier.Trade
        {
            set { trade = value; }
            get { return trade; }
        }
        string IPosRequestKeyIdentifier.CssCustodian
        {
            set { cssCustodian = value; }
            get { return cssCustodian; }
        }
        string IPosRequestKeyIdentifier.Market
        {
            set { market = value; }
            get { return market; }
        }
        string IPosRequestKeyIdentifier.Instrument
        {
            set { instrument = value; }
            get { return instrument; }
        }
        string IPosRequestKeyIdentifier.Asset
        {
            set { asset = value; }
            get { return asset; }
        }
        string IPosRequestKeyIdentifier.Entity
        {
            set { entity = value; }
            get { return entity; }
        }
        string IPosRequestKeyIdentifier.Dealer
        {
            set { dealer = value; }
            get { return dealer; }
        }
        string IPosRequestKeyIdentifier.BookDealer
        {
            set { bookDealer = value; }
            get { return bookDealer; }
        }
        string IPosRequestKeyIdentifier.Clearer
        {
            set { clearer = value; }
            get { return clearer; }
        }
        string IPosRequestKeyIdentifier.BookClearer
        {
            set { bookClearer = value; }
            get { return bookClearer; }
        }
        /// EG 20130607 [18740] Add RemoveCAExecuted
        string IPosRequestKeyIdentifier.CorporateAction
        {
            set { corporateAction = value; }
            get { return corporateAction; }
        }
        #endregion IPosRequestKeyIdentifier Members
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
    public partial class PosRequestClearingBLK : IPosRequestClearingBLK
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                string additionalMessage = Ressource.GetString2("Msg_PosRequestClearingBLK", qty.ToString());
                return RequestMessageByKeyPos(additionalMessage);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestClearingBLK() : base(Cst.PosRequestTypeEnum.ClearingBulk) { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestClearingBLK(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
            : base(Cst.PosRequestTypeEnum.ClearingBulk, pRequestMode, pDtBusiness, pQty)
        {
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestClearingBLK(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
            : base(pRequestType, pRequestMode, pDtBusiness, pQty)
        {
        }
        #endregion Constructors
        #region IPosRequestClearingBLK Members
        IPosRequestDetClearingBLK IPosRequestClearingBLK.Detail
        {
            set { detail = (PosRequestDetClearingBLK)value; }
            get { return detail; }
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        void IPosRequestClearingBLK.SetDetail(decimal pAvailableQtyBuy, decimal pAvailableQtySell)
        {
            detail = new PosRequestDetClearingBLK(pAvailableQtyBuy, pAvailableQtySell);
        }
        #endregion IPosRequestClearingBLK Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetClearingBLK)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetClearingBLK); }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 4 EVTs potentiels * 2 (clôturée et clôturante)
            // . OFS/TOT-PAR
            // . TER-INT/NOM
            // . TER-INT/QTY
            // . LPC/RMG
            // . LPC/VMG
            get { return 5 * 2; }
        }
        #endregion IPosRequest Members
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
    public partial class PosRequestClearingEOD : IPosRequestClearingEOD
    {
        #region Accessors
        public override string RequestMessage
        {
            get { return RequestMessageByKeyPos("Msg_PosRequestClearingEOD"); }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestClearingEOD() : base() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestClearingEOD(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
            : base(Cst.PosRequestTypeEnum.ClearingEndOfDay, pRequestMode, pDtBusiness, pQty)
        {
        }
        #endregion Constructors
        #region IPosRequestClearingBLK Members
        IPosRequestDetClearingBLK IPosRequestClearingBLK.Detail
        {
            set { detail = (PosRequestDetClearingBLK)value; }
            get { return detail; }
        }
        #endregion IPosRequestClearingBLK Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetClearingBLK)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetClearingBLK); }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 4 EVTs potentiels * 2 (clôturée et clôturante)
            // . OFS/TOT-PAR
            // . TER-INT/NOM
            // . TER-INT/QTY
            // . LPC/RMG
            // . LPC/VMG
            get { return 5 * 2; }
        }
        #endregion IPosRequest Members
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
    public partial class PosRequestClearingSPEC : IPosRequestClearingSPEC
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                string additionalMessage = Ressource.GetString2("Msg_PosRequestClearingSPEC", qty.ToString());
                return RequestMessageByTrade(additionalMessage);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestClearingSPEC() : base(Cst.PosRequestTypeEnum.ClearingSpecific) { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestClearingSPEC(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
            : base(Cst.PosRequestTypeEnum.ClearingSpecific, pRequestMode, pDtBusiness, pQty) { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestClearingSPEC(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, int pIdT, decimal pQty,
            IPosKeepingClearingTrade[] pTradesTarget)
            : this(pRequestMode, pDtBusiness, pQty)
        {
            idT = pIdT;
            idTSpecified = (0 < pIdT);
            detail = new PosRequestDetClearingSPEC(pTradesTarget);
        }
        #endregion Constructors
        #region IPosRequestClearingSPEC Members
        IPosRequestDetClearingSPEC IPosRequestClearingSPEC.Detail
        {
            set { detail = (PosRequestDetClearingSPEC)value; }
            get { return detail; }
        }
        #endregion IPosRequestClearingSPEC Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetClearingSPEC)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetClearingSPEC); }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 4 EVTs potentiels * 2 (clôturée et clôturante)
            // . OFS/TOT-PAR
            // . TER-INT/NOM
            // . TER-INT/QTY
            // . LPC/VMG
            get { return 5 * 2; }
        }
        #endregion IPosRequest Members
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
    /// EG 20150317 [POC] Set IdEM in Constructor
    /// EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
    public partial class PosRequestClosingDAY : IPosRequestClosingDAY
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                return RequestMessageByEntityCSS(string.Empty);
            }
        }
        #endregion Accessors

        #region Constructors
        public PosRequestClosingDAY() { }
        /// EG 20150317 [POC] Set IdEM in Constructor
        public PosRequestClosingDAY(int pIdA_Entity, int pIdA_CssCustodian,DateTime pDtBusiness, Nullable<int> pIdEM, bool pIsCustodian)
            : base(Cst.PosRequestTypeEnum.ClosingDay, SettlSessIDEnum.Intraday, pDtBusiness, 0)
        {
            idA_Entity = pIdA_Entity;
            ((IPosRequest)this).SetIdA_CssCustodian(pIdA_CssCustodian, pIsCustodian);
            idEMSpecified = (pIdEM.HasValue);
            if (idEMSpecified)
            {
                idEM = pIdEM.Value;
                // EG 20150316 Si IDEM spécifié alors IDM = -1
                idM = -1;
                idMSpecified = true;
            }
        }
        #endregion Constructors

        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { ; }
            get { return null; }
        }
        Type IPosRequest.DetailType
        {
            get { return null; }
        }
        int IPosRequest.NbTokenIdE
        {
            get { return 0; }
        }
        // EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
        IdInfo IPosRequest.GetMQueueIdInfo()
        {
            IdInfo idInfo = new IdInfo
            {
                id = idPR,
                idInfos = new DictionaryEntry[5]
            };
            // EG 20150317 (POC] New GProduct remplace pGProduct
            idInfo.idInfos[0] = new DictionaryEntry("GPRODUCT", GProduct);
            idInfo.idInfos[1] = new DictionaryEntry("REQUESTTYPE", requestType);
            idInfo.idInfos[2] = new DictionaryEntry("DTBUSINESS", dtBusiness);
            if (identifiersSpecified)
            {
                idInfo.idInfos[3] = new DictionaryEntry("ENTITY", identifiers.entity);
                idInfo.idInfos[4] = new DictionaryEntry("CSSCUSTODIAN", identifiers.cssCustodian);
            }
            return idInfo;
        }
        #endregion IPosRequest Members
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
    public partial class PosRequestClosingDayControl : IPosRequestClosingDayControl
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                if (this.idTSpecified)
                    return RequestMessageByTrade(string.Empty);
                else
                    return RequestMessageByKeyPos(string.Empty);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestClosingDayControl() : base() { }
        public PosRequestClosingDayControl(Cst.PosRequestTypeEnum pRequestType, int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, DateTime pDtBusiness)
            : base(pRequestType, SettlSessIDEnum.None, pDtBusiness, 0)
        {
            idA_Entity = pIdA_Entity;
            ((IPosRequest)this).SetIdA_CssCustodian(pIdA_Css, pIdA_Custodian);
            idEMSpecified = false;
        }

        public PosRequestClosingDayControl(Cst.PosRequestTypeEnum pRequestType,
            int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness)
            : base(pRequestType, SettlSessIDEnum.None, pDtBusiness, 0)
        {
            idA_Entity = pIdA_Entity;
            ((IPosRequest)this).SetIdA_CssCustodian(pIdA_Css, pIdA_Custodian);
            idEM = pIdEM;
            idEMSpecified = (0 < pIdEM);
        }
        #endregion Constructors
    }
    #endregion PosRequestClosingDayControl
    #region PosRequestCorporateAction
    /// <summary>
    /// <para>Type de POSREQUEST utilisé pour le traitement d'insertion d'un Trade ajusté suite à CA
    /// <para>──────────────────────────────────────────────────────────────────────────────────────
    /// <para>► Héritage de POSREQUEST
    /// <para>  ● Id Entité/Marché
    /// <para>  ● Clé de position (IDT)
    /// <para>  ● Date business
    /// <para>──────────────────────────────────────────────────────────────────────────────────────
    /// <para>► DETAIL (PosRequestDetCorporateAction)</para>
    /// <para>  Caractéristiques nécessaire à la génération de la nouvelle position</para>
    /// <para>──────────────────────────────────────────────────────────────────────────────────────
    /// </summary>
    // PM 20130213 [18414]
    /// EG 20140516 [19816] Add contractSymbolDCEx
    public partial class PosRequestCorporateAction : IPosRequestCorporateAction
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                if (this.idTSpecified)
                    return RequestMessageByTrade(string.Empty);
                else
                    return RequestMessageByKeyPos(string.Empty);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestCorporateAction() : base() { }
        public PosRequestCorporateAction(Cst.PosRequestTypeEnum pRequestType, int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness)
            : base(pRequestType, SettlSessIDEnum.StartOfDay, pDtBusiness, 0)
        {
            idA_Entity = pIdA_Entity;
            idA_Css = pIdA_Css;
            idA_CustodianSpecified = pIdA_Custodian.HasValue && (0 < pIdA_Custodian.Value);
            if (idA_CustodianSpecified)
                idA_Custodian = pIdA_Custodian.Value;
            idEM = pIdEM;
            idEMSpecified = (0 < pIdEM);
        }
        #endregion Constructors
        #region IPosRequestCorporateAction Members
        object IPosRequestCorporateAction.Clone()
        {
            IPosRequestCorporateAction clone = (PosRequestCorporateAction)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneField);
            return clone;
        }
        IPosRequestDetCorporateAction IPosRequestCorporateAction.Detail
        {
            set { detail = (PosRequestDetCorporateAction)value; }
            get { return detail; }
        }
        /// EG 20140516 [19816] Add contractSymbolDCEx
        /// EG 20171107 [23509] pEffectiveDate
        void IPosRequestCorporateAction.SetDetail(int pIdDCEx, string pIdentifierDCEx, string pContractSymbolDCEx, DateTime pEffectiveDate)
        {
            detail = new PosRequestDetCorporateAction(pIdDCEx, pIdentifierDCEx, pContractSymbolDCEx, pEffectiveDate);
        }
        #endregion IPosRequestCorporateAction Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetCorporateAction)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetCorporateAction); }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 3 EVTs potentiels
            // . OCA/TOT
            // . TER/NOM
            // . TER/QTY
            // . LPC/EQP (Equalisation payment)
            get { return (4); }
        }
        #endregion IPosRequest Members
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
    public partial class PosRequestCorrection : IPosRequestCorrection
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                string additionalMessage = Ressource.GetString2("Msg_PosRequestCorrection", qty.ToString());
                return RequestMessageByTrade(additionalMessage);
            }
        }
        #endregion Accessors

        #region Constructors
        public PosRequestCorrection() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestCorrection(SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
            : base(Cst.PosRequestTypeEnum.PositionCancelation, pRequestMode, pDtBusiness, pQty)
        {
        }
        #endregion Constructors
        #region IPosRequestCorrection Members
        IPosRequestDetCorrection IPosRequestCorrection.Detail
        {
            set { detail = (PosRequestDetCorrection)value; }
            get { return detail; }
        }
        // EG 20150716 [21103] Add pIsReversalSafekeeping
        // EG 20150907 [21317] Add InitialQty
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        void IPosRequestCorrection.SetDetail(decimal pInitialQty, decimal pAvailableQty, IPayment[] pPaymentFees, EFS_Boolean pIsReversalSafekeeping)
        {
            detail = new PosRequestDetCorrection(pInitialQty, pAvailableQty)
            {
                paymentFeesSpecified = ArrFunc.IsFilled(pPaymentFees),
                isReversalSafekeepingSpecified = (null != pIsReversalSafekeeping)
            };
            if (detail.paymentFeesSpecified)
                detail.paymentFees = (Payment[])pPaymentFees;
            // EG 20150716 [21103]
            if (detail.isReversalSafekeepingSpecified)
                detail.isReversalSafekeeping = pIsReversalSafekeeping.BoolValue;
        }
        #endregion IPosRequestCorrection Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetCorrection)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetCorrection); }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 4 EVTs potentiels + Frais éventuels
            // . POC/TOT-PAR
            // . TER-INT/NOM
            // . TER-INT/QTY
            // . LPP/VMG-PRM ou LPP/VMG et PRM 
            // EG 20131205 [193302] Add 1 jeton pour Premium restitution voir aussi [18754]
            // EG 20150624 [21151] Add 2 jetons pour GAM|AIN restitution 
            get { return 7 + detail.nbAdditionalEvents; }
        }
        #endregion IPosRequest Members
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

    public partial class PosRequestEntry : IPosRequestEntry
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                return RequestMessageByKeyPos(string.Empty);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestEntry() : base(Cst.PosRequestTypeEnum.Entry) { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestEntry(DateTime pDtBusiness, decimal pQty)
            : base(Cst.PosRequestTypeEnum.Entry, SettlSessIDEnum.Intraday, pDtBusiness, pQty)
        {
        }
        #endregion Constructors
        #region IPosRequestEntry Members
        IPosRequestDetEntry IPosRequestEntry.Detail
        {
            set { detail = (PosRequestDetEntry)value; }
            get { return detail; }
        }
        void IPosRequestEntry.SetDetail(IPosKeepingTrade pTrade, PosKeepingEntryMQueue pMessage)
        {
            detail = new PosRequestDetEntry
            {
                dtBusiness = pTrade.DtBusiness,
                side = pTrade.Side,
                qty = pTrade.Qty,
                dtExecution = pTrade.DtExecution,
                positionEffect = pTrade.PositionEffect,
                message = pMessage
            };
        }
        #endregion PosRequestEntry Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetEntry)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetEntry); }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 4 EVTs potentiels * 2 (clôturée et clôturante)
            // . OFS/TOT-PAR
            // . TER-INT/NOM
            // . TER-INT/QTY
            // . LPC/RMG
            // . LPC/VMG
            get { return 5 * 2; }
        }
        #endregion IPosRequest Members
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
    /// EG 20150317 [POC] Set IdEM in Constructor
    /// EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
    public partial class PosRequestEOD : IPosRequestEOD
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                return RequestMessageByEntityCSS(string.Empty);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestEOD() { }
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Add Cst.PosRequestTypeEnum pPosRequestType parameter)
        public PosRequestEOD(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int> pIdEM, bool pIsCustodian)
            : this(pIdA_Entity, pIdA_CssCustodian, pDtBusiness, pIdEM, pIsCustodian, Cst.PosRequestTypeEnum.EndOfDay)
        {

        }
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Add Cst.PosRequestTypeEnum pPosRequestType parameter)
        public PosRequestEOD(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int> pIdEM, bool pIsCustodian, Cst.PosRequestTypeEnum pPosRequestType)
            : base(pPosRequestType, SettlSessIDEnum.Intraday, pDtBusiness, 0)
        {
            idA_Entity = pIdA_Entity;
            ((IPosRequest)this).SetIdA_CssCustodian(pIdA_CssCustodian, pIsCustodian);
            idEMSpecified = pIdEM.HasValue;
            if (idEMSpecified)
            {
                idEM = pIdEM.Value;
                // EG 20150316 Si IDEM spécifié alors IDM = -1
                idM = -1;
                idMSpecified = true;
            }
        }
        public PosRequestEOD(int pIdA_Entity, int pIdA_CssCustodian,  int pIdEM, DateTime pDtBusiness, bool pIsCustodian)
            : base(Cst.PosRequestTypeEnum.EOD_MarketGroupLevel, SettlSessIDEnum.Intraday, pDtBusiness, 0)
        {
            idA_Entity = pIdA_Entity;
            ((IPosRequest)this).SetIdA_CssCustodian(pIdA_CssCustodian, pIsCustodian);
            idEM = pIdEM;
            idEMSpecified = (0 < pIdEM);
        }
        #endregion Constructors
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { ; }
            get { return null; }
        }
        Type IPosRequest.DetailType
        {
            get { return null; }
        }
        int IPosRequest.NbTokenIdE
        {
            get { return 0; }
        }
        /// EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
        IdInfo IPosRequest.GetMQueueIdInfo()
        {
            IdInfo idInfo = new IdInfo
            {
                id = idPR,
                idInfos = new DictionaryEntry[5]
            };
            // EG 20150317 (POC] New GProduct remplace pGProduct
            idInfo.idInfos[0] = new DictionaryEntry("GPRODUCT", GProduct);
            idInfo.idInfos[1] = new DictionaryEntry("REQUESTTYPE", requestType);
            idInfo.idInfos[2] = new DictionaryEntry("DTBUSINESS", dtBusiness);
            if (identifiersSpecified)
            {
                idInfo.idInfos[3] = new DictionaryEntry("ENTITY", identifiers.entity);
                idInfo.idInfos[4] = new DictionaryEntry("CSSCUSTODIAN", identifiers.cssCustodian);
            }
            return idInfo;
        }
        #endregion IPosRequest Members
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
    /// EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
    public partial class PosRequestREMOVEEOD : IPosRequestREMOVEEOD
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                if (idEMSpecified && idMSpecified & identifiersSpecified && StrFunc.IsFilled(identifiers.market))
                    return RequestMessageByEntityMarket(string.Empty);
                else
                    return RequestMessageByEntityCSS(string.Empty);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestREMOVEEOD() { }
        public PosRequestREMOVEEOD(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, bool pIsCustodian)
            : base(Cst.PosRequestTypeEnum.RemoveEndOfDay, SettlSessIDEnum.Intraday, pDtBusiness, 0)
        {
            idA_Entity = pIdA_Entity;
            ((IPosRequest)this).SetIdA_CssCustodian(pIdA_CssCustodian, pIsCustodian);
            idEMSpecified = false;
        }
        public PosRequestREMOVEEOD(int pIdA_Entity, int pIdA_CssCustodian, DateTime pDtBusiness, Nullable<int> pIdEM, bool pIsCustodian)
            : base(Cst.PosRequestTypeEnum.RemoveEndOfDay, SettlSessIDEnum.Intraday, pDtBusiness, 0)
        {
            idA_Entity = pIdA_Entity;
            ((IPosRequest)this).SetIdA_CssCustodian(pIdA_CssCustodian, pIsCustodian);
            idEMSpecified = pIdEM.HasValue;
            if (idEMSpecified)
                idEM = pIdEM.Value;
        }
        #endregion Constructors
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { ; }
            get { return null; }
        }
        Type IPosRequest.DetailType
        {
            get { return null; }
        }
        int IPosRequest.NbTokenIdE
        {
            get { return 0; }
        }
        // EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
        IdInfo IPosRequest.GetMQueueIdInfo()
        {
            IdInfo idInfo = new IdInfo
            {
                id = idPR,
                idInfos = new DictionaryEntry[5]
            };
            // EG 20150313 (POC] New GProduct remplace pGProduct
            idInfo.idInfos[0] = new DictionaryEntry("GPRODUCT", GProduct);
            idInfo.idInfos[1] = new DictionaryEntry("REQUESTTYPE", requestType);
            idInfo.idInfos[2] = new DictionaryEntry("DTBUSINESS", dtBusiness);
            if (identifiersSpecified)
            {
                idInfo.idInfos[3] = new DictionaryEntry("ENTITY", identifiers.entity);
                idInfo.idInfos[4] = new DictionaryEntry("CSSCUSTODIAN", identifiers.cssCustodian);
            }
            return idInfo;
        }
        #endregion IPosRequest Members
    }
    #endregion PosRequestREMOVEEOD
    #region PosRequestCascadingShifting
    /// <summary>
    /// <para>Type de POSREQUEST utilisé pour le traitement du cascading shifting à l'échéance
    /// <para>──────────────────────────────────────────────────────────────────────────────────────
    /// <para>► Héritage de POSREQUEST
    /// <para>  ● Id Entité/Marché
    /// <para>  ● Clé de position
    /// <para>  ● Date business
    /// <para>──────────────────────────────────────────────────────────────────────────────────────
    /// <para>► DETAIL (PosRequestDetCascadingShifting)</para>
    /// <para>  Caractéristiques nécessaire à la génération de la nouvelle position</para>
    /// <para>──────────────────────────────────────────────────────────────────────────────────────
    /// </summary>
    // PM 20130213 [18414]
    public partial class PosRequestCascadingShifting : IPosRequestCascadingShifting
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                return RequestMessageByKeyPos(string.Empty);
            }
        }
        #endregion Accessors

        #region Constructors
        public PosRequestCascadingShifting() { }
        public PosRequestCascadingShifting(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness)
            : base(pRequestType, pRequestMode, pDtBusiness, 0)
        {
        }
        #endregion Constructors

        #region IPosRequestCascadingShifting Members
        object IPosRequestCascadingShifting.Clone()
        {
            PosRequestCascadingShifting clone = (PosRequestCascadingShifting)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneField);
            return clone;
        }
        IPosRequestDetCascadingShifting IPosRequestCascadingShifting.Detail
        {
            set { detail = (PosRequestDetCascadingShifting)value; }
            get { return detail; }
        }
        /// EG 20171003 [23452] pTradeDateTime
        void IPosRequestCascadingShifting.SetDetail(int pIdDCDest, string pIdentifierDCDest, string pMaturityDest, DateTime pTradeDateTime)
        {
            detail = new PosRequestDetCascadingShifting(pIdDCDest, pIdentifierDCDest, pMaturityDest, pTradeDateTime);
        }
        #endregion IPosRequestCascadingShifting Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetCascadingShifting)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetCascadingShifting); }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 3 EVTs potentiels
            // . MOF/TOT
            // . TER/NOM
            // . TER/QTY
            get { return (3); }
        }
        #endregion IPosRequest Members
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
    public partial class PosRequestMaturityOffsetting : IPosRequestMaturityOffsetting
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                return RequestMessageByKeyPos(string.Empty);
            }
        }
        #endregion Accessors

        #region Constructors
        public PosRequestMaturityOffsetting() { }
        public PosRequestMaturityOffsetting(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness)
            : base(pRequestType, pRequestMode, pDtBusiness, 0)
        {
        }
        #endregion Constructors

        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { ; }
            get { return null; }
        }
        Type IPosRequest.DetailType
        {
            get { return null; }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 4 EVTs potentiels * 2 (clôturée et clôturante)
            // . MOF/TOT
            // . TER/NOM
            // . TER/QTY
            // . LPC/RMG
            get { return (4); }
        }
        #endregion IPosRequest Members
    }
    #endregion PosRequestMaturityOffsetting
    #region PosRequestPhysicalDeliveryPayment
    /// <summary>
    /// <para>Type de POSREQUEST utilisé pour le traitement de payements relatifs à la livraison periodique sur DC Future
    /// <para>─────────────────────────────────────────────────────────────────────────────────────────────
    /// <para>► Héritage de POSREQUEST
    /// <para>  ● Id Entité/Marché
    /// <para>  ● Clé de position
    /// <para>  ● Date business
    /// <para>─────────────────────────────────────────────────────────────────────────────────────────────
    /// </summary>
    public partial class PosRequestPhysicalPeriodicDelivery : IPosRequestPhysicalPeriodicDelivery
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                return RequestMessageByKeyPos(string.Empty);
            }
        }
        #endregion Accessors

        #region Constructors
        public PosRequestPhysicalPeriodicDelivery() { }
        public PosRequestPhysicalPeriodicDelivery(DateTime pDtBusiness)
            : base(Cst.PosRequestTypeEnum.PhysicalPeriodicDelivery, SettlSessIDEnum.EndOfDay, pDtBusiness, 0)
        {
        }
        #endregion Constructors

        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { ; }
            get { return null; }
        }
        Type IPosRequest.DetailType
        {
            get { return null; }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = (2 EVTs * nb Period)
            // . TER/QTY
            // . LPC/AMT
            get { return (2); }
        }
        #endregion IPosRequest Members
    }
    #endregion PosRequestPhysicalDeliveryPayment
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
    // EG 20160121 [21805] POC-MUREX Add IDTOPTION 
    // EG 20170127 Qty Long To Decimal
    public partial class PosRequestOption : IPosRequestOption
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                //PL 20160331 Test qty==0 (Afin de ne pas afficher cette quantité à zéro sur les "annulations" de dénouement. 
                string additionalMessage = string.Empty;
                if (qty != 0)
                    additionalMessage = Ressource.GetString2("Msg_PosRequestOption", qty.ToString());

                return RequestMessageByTrade(additionalMessage);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestOption() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestOption(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness, decimal pQty)
            : base(pRequestType, pRequestMode, pDtBusiness, pQty)
        {
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestOption(Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DateTime pDtBusiness,
            decimal pQty, decimal pAvailableQty, decimal pStrikePrice)
            : base(pRequestType, pRequestMode, pDtBusiness, pQty)
        {
            detail = new PosRequestDetOption(pAvailableQty, pStrikePrice);
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20151019 [21465] Add Nullable on parameters pQuoteTiming|pPrice|pDtPrice
        // EG 20170127 Qty Long To Decimal
        public PosRequestOption(string pCS, Cst.PosRequestTypeEnum pRequestType, int pIdT, SettlSessIDEnum pRequestMode, DateTime pDtBusiness,
            decimal pQty, decimal pAvailableQty, decimal pStrikePrice,
            Cst.UnderlyingAsset pAssetCategory, int pIdAsset, string pIdentifier, Nullable<QuoteTimingEnum> pQuoteTiming,
            Nullable<decimal> pPrice, Nullable<DateTime> pDtQuote, string pSource)
            : this(pRequestType, pRequestMode, pDtBusiness, pQty, pAvailableQty, pStrikePrice)
        {
            ((IPosRequestDetOption)detail).SetUnderlyer(pCS, pRequestType, pIdT, pAssetCategory, pIdAsset, pIdentifier, pQuoteTiming, pPrice, pDtQuote, pSource);
        }
        #endregion Constructors
        #region IPosRequestOption Members
        object IPosRequestOption.Clone()
        {
            PosRequestOption clone = (PosRequestOption)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneField);
            return clone;
        }
        IPosRequestDetOption IPosRequestOption.Detail
        {
            set { detail = (PosRequestDetOption)value; }
            get { return detail; }
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20151019 [21465] Add Nullable on parameters pQuoteTiming|pPrice|pDtPrice, Add Nullable<bool> pFeeCalculation
        // EG 20170127 Qty Long To Decimal
        void IPosRequestOption.SetDetail(string pCS, decimal pAvailableQty, decimal pStrikePrice,
            Cst.UnderlyingAsset pAssetCategory, int pIdAsset, string pIdentifier, Nullable<QuoteTimingEnum> pQuoteTiming,
            Nullable<decimal> pPrice, Nullable<DateTime> pDtPrice, string pSource, IPayment[] pPaymentFees, Nullable<bool> pIsFeeCalculation)
        {
            detail = new PosRequestDetOption(pCS, requestType, idT, pAvailableQty, pStrikePrice,
                pAssetCategory, pIdAsset, pIdentifier, pQuoteTiming, pPrice, pDtPrice, pSource)
            {
                paymentFeesSpecified = ArrFunc.IsFilled(pPaymentFees),
                feeCalculationSpecified = pIsFeeCalculation.HasValue
            };
            if (detail.paymentFeesSpecified)
                detail.paymentFees = (Payment[])pPaymentFees;
            if (detail.feeCalculationSpecified)
                detail.feeCalculation = pIsFeeCalculation.Value;
        }
        bool IPosRequestOption.IdTOptionSpecified
        {
            set { idTOptionSpecified = value; }
            get { return idTOptionSpecified; }
        }
        int IPosRequestOption.IdTOption
        {
            set { idTOption = value; }
            get { return idTOption; }
        }
        #endregion IPosRequestOption Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetOption)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetOption); }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 11 EVTs potentiels
            // . ABN-ASS-EXE/TOT-PAR
            // . TER-INT/NOM
            // . TER-INT/QTY
            // . TER-INT/SCU
            // . LPP/VMG
            // . LPP/RMG
            // . ABN/PAR si ASS-EXE/PAR et abandon de la quantité restante
            // . TER/NOM si ASS-EXE/PAR et abandon de la quantité restante
            // . TER/QTY si ASS-EXE/PAR et abandon de la quantité restante
            // . LPP/VMG si ASS-EXE/PAR et abandon de la quantité restante
            // . LPP/RMG si ASS-EXE/PAR et abandon de la quantité restante
            get { return 11 + detail.nbAdditionalEvents; }
        }
        #endregion IPosRequest Members
    }
    #endregion PosRequestOption

    #region PosRequestREMOVECAEXECUTED
    /// <summary>
    /// <para>Utilisé pour l'annulation d'un traitement de CA (REQUESTTYPE = REMOVECAEXECUTED)</para>
    /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Clearing House</para>
    /// <para>  ● Id Market</para>
    /// <para>  ● Id Corporate Event</para>
    /// <para>  ● DtBusiness</para>
    /// <para>─────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    /// EG 20130607 [18740] Add RemoveCAExecuted
    /// EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
    public partial class PosRequestRemoveCAExecuted : IPosRequestRemoveCAExecuted
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                return RequestMessageByCorporateAction(string.Empty);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestRemoveCAExecuted() { }
        public PosRequestRemoveCAExecuted(int pIdA_CssCustodian, int pIdM, int pIdCE, DateTime pDtBusiness, bool pIsCustodian)
            : base(Cst.PosRequestTypeEnum.RemoveCAExecuted, SettlSessIDEnum.Intraday, pDtBusiness, 0)
        {
            ((IPosRequest)this).SetIdA_CssCustodian(pIdA_CssCustodian, pIsCustodian);
            idM = pIdM;
            idMSpecified = true;
            idCE = pIdCE;
            idCESpecified = true;
        }
        #endregion Constructors
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { ; }
            get { return null; }
        }
        Type IPosRequest.DetailType
        {
            get { return null; }
        }
        int IPosRequest.NbTokenIdE
        {
            get { return 0; }
        }
        // EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
        IdInfo IPosRequest.GetMQueueIdInfo()
        {
            IdInfo idInfo = new IdInfo
            {
                id = idPR,
                idInfos = new DictionaryEntry[5]
            };
            // EG 20150313 (POC] New GProduct remplace pGProduct
            idInfo.idInfos[0] = new DictionaryEntry("GPRODUCT", GProduct);
            idInfo.idInfos[1] = new DictionaryEntry("REQUESTTYPE", requestType);
            idInfo.idInfos[2] = new DictionaryEntry("DTBUSINESS", dtBusiness);
            if (identifiersSpecified)
            {
                idInfo.idInfos[3] = new DictionaryEntry("CSSCUSTODIAN", identifiers.cssCustodian);
                idInfo.idInfos[4] = new DictionaryEntry("MARKET", identifiers.market);
            }
            return idInfo;
        }
        #endregion IPosRequest Members
    }
    #endregion PosRequestRemoveCAExecuted

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
    public partial class PosRequestPositionOption : IPosRequestPositionOption
    {
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public override string RequestMessage
        {
            get
            {
                string additionalMessage = Ressource.GetString2("Msg_PosRequestOption", qty.ToString());
                return RequestMessageByKeyPos(additionalMessage);
            }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public PosRequestPositionOption() { }
        #endregion Constructors

        #region IPosRequestPositionOption Members
        object IPosRequestPositionOption.Clone()
        {
            PosRequestPositionOption clone = (PosRequestPositionOption)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneField);
            return clone;
        }
        IPosRequestDetPositionOption IPosRequestPositionOption.Detail
        {
            set { detail = (PosRequestDetPositionOption)value; }
            get { return detail; }
        }
        void IPosRequestPositionOption.SetDetail(bool pIsPartialExecutionAllowed, bool pIsFeeCalculation, bool pIsAbandonRemainingQty, IPayment[] pPaymentFees)
        {
            detail = new PosRequestDetPositionOption
            {
                partialExecutionAllowed = pIsPartialExecutionAllowed,
                abandonRemainingQty = pIsAbandonRemainingQty,
                // RD 20200120 [25114] detail.feeCalculationSpecified
                feeCalculationSpecified = true,
                feeCalculation = pIsFeeCalculation,
                paymentFees = (Payment[])pPaymentFees
            };
            detail.paymentFeesSpecified = ArrFunc.IsFilled(detail.paymentFees);

        }
        #endregion IPosRequestOption Members

        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetPositionOption)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetPositionOption); }
        }
        int IPosRequest.NbTokenIdE
        {
            get { return 0; }
        }

        #endregion IPosRequest Members
    }


    #region PosRequestRemoveAlloc
    /// <summary>
    /// <para>Utilisée par l'ANNULATION d'ALLOCATION (REQUESTTYPE = RMVALLOC)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Héritage de POSREQUEST</para>
    /// <para>  ● Id Entité/Marché</para>
    /// <para>  ● Date business</para>
    /// <para>  ● Id du trade source</para>
    /// <para>► DETAIL (PosRequestDetRemoveAlloc)</para>
    /// <para>  Caractéristiques du trade annulé nécessaires à la décompensation éventuelle</para>
    /// <para>  ● Quantité initiale</para>
    /// <para>  ● Quantité en position</para>
    ///───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    public partial class PosRequestRemoveAlloc : IPosRequestRemoveAlloc
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                string dtRemove = DtFunc.DateTimeToString(dtBusiness, DtFunc.FmtShortDate);
                string additionalMessage = Ressource.GetString2("Msg_PosRequestRemoveAlloc", qty.ToString(), dtRemove);
                return RequestMessageByTrade(additionalMessage);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestRemoveAlloc() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestRemoveAlloc(DateTime pDtBusiness, decimal pQty)
            : base(Cst.PosRequestTypeEnum.RemoveAllocation, SettlSessIDEnum.Intraday, pDtBusiness, pQty)
        {
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestRemoveAlloc(DateTime pDtBusiness, decimal pInitialQty, decimal pPositionQty)
            : base(Cst.PosRequestTypeEnum.RemoveAllocation, SettlSessIDEnum.Intraday, pDtBusiness, pInitialQty)
        {
            detail = new PosRequestDetRemoveAlloc(pInitialQty, pPositionQty);
        }
        #endregion Constructors
        #region IPosRequestRemoveAlloc Members
        object IPosRequestRemoveAlloc.Clone()
        {
            PosRequestRemoveAlloc clone = (PosRequestRemoveAlloc)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneField);
            return clone;
        }
        IPosRequestDetRemoveAlloc IPosRequestRemoveAlloc.Detail
        {
            set { detail = (PosRequestDetRemoveAlloc)value; }
            get { return detail; }
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        void IPosRequestRemoveAlloc.SetDetail(string pCS, decimal pInitialQty, decimal pPositionQty)
        {
            detail = new PosRequestDetRemoveAlloc(pInitialQty, pPositionQty);
        }
        #endregion IPosRequestRemoveAlloc Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetRemoveAlloc)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetRemoveAlloc); }
        }
        int IPosRequest.NbTokenIdE
        {
            get { return 4 + detail.nbAdditionalEvents; }
        }
        #endregion IPosRequest Members
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
    /// <para>  Caractéristiques des trades nés du Splitting</para>
    ///───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    public partial class PosRequestSplit : IPosRequestSplit
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                string dtSplit = DtFunc.DateTimeToString(dtBusiness, DtFunc.FmtShortDate);
                string additionalMessage = Ressource.GetString2("Msg_PosRequestSplit", qty.ToString(), dtSplit);
                return RequestMessageByTrade(additionalMessage);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestSplit() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestSplit(DateTime pDtBusiness, decimal pQty)
            : base(Cst.PosRequestTypeEnum.TradeSplitting, SettlSessIDEnum.Intraday, pDtBusiness, pQty)
        {
        }
        #endregion Constructors
        #region IPosRequestSplit Members
        object IPosRequestSplit.Clone()
        {
            PosRequestSplit clone = (PosRequestSplit)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneField);
            return clone;
        }
        IPosRequestDetSplit IPosRequestSplit.Detail
        {
            set { detail = (PosRequestDetSplit)value; }
            get { return detail; }
        }
        void IPosRequestSplit.SetDetail(string pCS, ArrayList pNewTrades)
        {
            detail = new PosRequestDetSplit(pNewTrades);
        }
        #endregion IPosRequestSplit Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetSplit)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetSplit); }
        }
        int IPosRequest.NbTokenIdE 
        {
            //PL: A priori inutilisé dans le cas du Splitting de trade
            //get { return 4 + detail.nbAdditionalEvents; }
            get { return 0; }
        }
        #endregion IPosRequest Members
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
    // EG 20140702 Upd
    public partial class PosRequestTransfer : IPosRequestTransfer
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                string dtTransfer = DtFunc.DateTimeToString(dtBusiness, DtFunc.FmtShortDate);
                string additionalMessage;
                if (detail.idTReplaceSpecified)
                {
                    string dealer = string.Empty;
                    if (null != detail.dealerTarget)
                        dealer = detail.dealerTarget.partyName;
                    if (null != detail.dealerBookIdTarget)
                        dealer += " (" + detail.dealerBookIdTarget.bookName + ")";

                    string clearer = string.Empty;
                    if (null != detail.clearerTarget)
                        clearer = detail.clearerTarget.partyName;
                    if (null != detail.clearerBookIdTarget)
                        clearer += " (" + detail.clearerBookIdTarget.bookName + ")";

                    string tradeReplace = detail.trade_IdentifierReplace + " (" + detail.idTReplace.ToString() + ")";
                    additionalMessage = Ressource.GetString2("Msg_PosRequestPositiontransfer", qty.ToString(), dtTransfer, tradeReplace, dealer, clearer);
                    additionalMessage = RequestMessageByTrade(additionalMessage);
                }
                else
                {
                    string tradeSource = string.Empty;
                    if (identifiersSpecified)
                        tradeSource = identifiers.trade + " (" + idT.ToString() + ")";
                    additionalMessage = Ressource.GetString2("Msg_PosRequestPositiontransferBulk", tradeSource, qty.ToString(), dtTransfer);

                }
                return additionalMessage;
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestTransfer() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestTransfer(DateTime pDtBusiness, decimal pQty)
            : base(Cst.PosRequestTypeEnum.PositionTransfer, SettlSessIDEnum.Intraday, pDtBusiness, pQty)
        {
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestTransfer(DateTime pDtBusiness, decimal pQty, decimal pInitialQty, decimal pPositionQty)
            : base(Cst.PosRequestTypeEnum.PositionTransfer, SettlSessIDEnum.Intraday, pDtBusiness, pQty)
        {
            detail = new PosRequestDetTransfer(pInitialQty, pPositionQty);
        }
        #endregion Constructors
        #region IPosRequestTransfer Members
        object IPosRequestTransfer.Clone()
        {
            PosRequestTransfer clone = (PosRequestTransfer)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneField);
            return clone;
        }
        IPosRequestDetTransfer IPosRequestTransfer.Detail
        {
            set { detail = (PosRequestDetTransfer)value; }
            get { return detail; }
        }
        /// EG 20141210 [20554] 
        /// EG 20150106 [20634]
        /// EG 20150716 [21103] Add pIsReversalSafekeeping
        /// EG 20150920 [21374] Int (int32) to Long (Int64) 
        /// FI 20161005 [XXXXX] Modify
        // EG 20170127 Qty Long To Decimal
        // EG 20180205 [23769] Upd EFS_TradeLibray constructor call  (substitution to the static class EFS_CURRENT)  
        void IPosRequestTransfer.SetDetail(string pCS, decimal pInitialQty, decimal pPositionQty, IPayment[] pPaymentFees, int pIdTReplace, EFS_Boolean pIsReversalSafekeeping)
        {
            detail = new PosRequestDetTransfer(pInitialQty, pPositionQty);
            EFS_TradeLibrary tradeLibReplace = new EFS_TradeLibrary(pCS, null, pIdTReplace);
            if (null == tradeLibReplace.DataDocument)
                throw new Exception(StrFunc.AppendFormat("Trade not found [id:{0}]", pIdTReplace.ToString()));

            string tradeLibReplaceIdentifier = TradeRDBMSTools.GetTradeIdentifier(pCS, pIdTReplace);
            if (StrFunc.IsEmpty(tradeLibReplaceIdentifier))
                throw new Exception(StrFunc.AppendFormat("Trade Identifier not found [id:{0}]", pIdTReplace.ToString()));

            DataDocumentContainer doc = tradeLibReplace.DataDocument;
            RptSideProductContainer _rptSide = doc.CurrentProduct.RptSide(pCS, true);
            // FI 20161005 [XXXXX] Add NotImplementedException
            if (null == _rptSide)
                throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented ", doc.CurrentProduct.ProductBase.ToString()));

            IFixParty partyDealer = _rptSide.GetDealer();
            if (null == partyDealer)
                throw new Exception(StrFunc.AppendFormat("Dealer not found in trade [identifier:{0}]", tradeLibReplaceIdentifier));

            IParty dealerParty = doc.GetParty(partyDealer.PartyId.href);
            IBookId dealerBook = doc.GetBookId(partyDealer.PartyId.href);

            IFixParty partyClearer;
            if (_rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.SELLER))
                partyClearer = _rptSide.GetBuyerSeller(SideEnum.Buy);
            else if (_rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                partyClearer = _rptSide.GetBuyerSeller(SideEnum.Sell);
            else
                throw new Exception(StrFunc.AppendFormat("Buyer or seller not found in trade [identifier:{0}]", tradeLibReplaceIdentifier));

            IParty clearerParty = doc.GetParty(partyClearer.PartyId.href);
            IBookId clearerBook = doc.GetBookId(partyClearer.PartyId.href);

            //trade
            detail.idTReplace = pIdTReplace;
            // EG 20150106 [20634]
            detail.idTReplaceSpecified = (0 < pIdTReplace);
            detail.trade_IdentifierReplace = tradeLibReplaceIdentifier;
            // EG 20150106 [20634]
            detail.trade_IdentifierReplaceSpecified = StrFunc.IsFilled(tradeLibReplaceIdentifier);
            //dealer
            detail.dealerTarget = (Party)dealerParty;
            if (null != dealerBook)
            detail.dealerBookIdTarget = (BookId)dealerBook;
            //clearer
            detail.clearerTarget = (Party)clearerParty;
            if (null != clearerBook)
                detail.clearerBookIdTarget = (BookId)clearerBook;
            //paymentFees
            detail.paymentFeesSpecified = ArrFunc.IsFilled(pPaymentFees);
            if (detail.paymentFeesSpecified)
            {
                detail.paymentFees = new Payment[pPaymentFees.Length];
                pPaymentFees.CopyTo(detail.paymentFees, 0);
            }
            // EG 20150716 [21103]
            detail.isReversalSafekeepingSpecified = (null != pIsReversalSafekeeping) && (doc.CurrentProduct.ProductBase.IsEquitySecurityTransaction || doc.CurrentProduct.ProductBase.IsDebtSecurityTransaction);
            if (detail.isReversalSafekeepingSpecified)
                detail.isReversalSafekeeping = pIsReversalSafekeeping.BoolValue;
        }
        /// EG 20150716 [21103] Add pIsReversalSafekeeping
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        void IPosRequestTransfer.SetDetail(string pCS, decimal pInitialQty, decimal pPositionQty, Pair<IParty, IBookId> pTransferDealer, Pair<IParty, IBookId> pTransferClearer, 
            Pair<bool, bool> pTransferInfo, EFS_Boolean pIsReversalSafekeeping)
        {
            detail = new PosRequestDetTransfer(pInitialQty, pPositionQty)
            {
                idTReplaceSpecified = false,
                isReversalFeesSpecified = true,
                isReversalFees = pTransferInfo.First,
                isCalcNewFeesSpecified = true,
                isCalcNewFees = pTransferInfo.Second,
                isReversalSafekeepingSpecified = (null != pIsReversalSafekeeping)
            };

            //dealer
            if (null != pTransferDealer)
            {
                detail.dealerTarget = (Party)pTransferDealer.First;
                detail.dealerBookIdTarget = (BookId)pTransferDealer.Second;
            }
            //clearer
            if (null != pTransferClearer)
            {
                detail.clearerTarget = (Party)pTransferClearer.First;
                detail.clearerBookIdTarget = (BookId)pTransferClearer.Second;
            }
            // EG 20150716 [21103]
            if (detail.isReversalSafekeepingSpecified)
                detail.isReversalSafekeeping = pIsReversalSafekeeping.BoolValue;
        }
        #endregion IPosRequestTransfer Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetTransfer)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetTransfer); }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 4 EVTs potentiels  + Frais éventuels
            // . POT/TOT-PAR
            // . TER-INT/NOM
            // . TER-INT/QTY
            // . LPP/VMG-PRM ou LPP/VMG et PRM 
            // EG 20131205 [193302] Add 1 jeton pour Premium restitution voir aussi [18754]
            // EG 20150624 [21151] Add 2 jetons pour GAM|AIN restitution 
            get { return 7 + detail.nbAdditionalEvents; }
        }
        #endregion IPosRequest Members
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
    /// EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
    public partial class PosRequestUnclearing : IPosRequestUnclearing
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                string requestTypeTarget = Ressource.GetString(detail.requestType.ToString());
                string dtBusinessTarget = DtFunc.DateTimeToString(detail.dtBusiness, DtFunc.FmtShortDate);
                string additionalMessage = Ressource.GetString2("Msg_PosRequestUnClearing", qty.ToString(),
                    identifiers.entity, identifiers.cssCustodian, identifiers.market,
                    dtBusinessTarget, detail.closing_Identifier, detail.closingQty.ToString(), requestTypeTarget);
                return RequestMessageByTrade(additionalMessage);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestUnclearing() : base(Cst.PosRequestTypeEnum.UnClearing) { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestUnclearing(DateTime pDtMarket, decimal pQty,
            int pIdPR, int pIdPADET, Cst.PosRequestTypeEnum pRequestType, DateTime pDtBusiness, int pIdT_Closing, string pClosing_Identifier, decimal pClosingQty)
            : base(Cst.PosRequestTypeEnum.UnClearing, SettlSessIDEnum.Intraday, pDtMarket, pQty)
        {
            detail = new PosRequestDetUnclearing(pIdPR, pIdPADET, pRequestType, pDtBusiness, pIdT_Closing, pClosing_Identifier, pClosingQty);
        }
        #endregion Constructors
        #region IPosRequestUnclearing Members
        IPosRequestDetUnclearing IPosRequestUnclearing.Detail
        {
            set { detail = (PosRequestDetUnclearing)value; }
            get { return detail; }
        }
        #endregion IPosRequestUnclearing Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetUnclearing)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetUnclearing); }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 4 EVTs potentiels * 2 (clôturée et clôturante)
            // . OFS/TOT-PAR
            // . TER-INT/NOM
            // . TER-INT/QTY
            // . LPC/RMG
            // . LPC/VMG
            get { return 5 * 2; }
        }
        // EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
        IdInfo IPosRequest.GetMQueueIdInfo()
        {
            IdInfo idInfo = new IdInfo
            {
                id = idT,
                idInfos = new DictionaryEntry[13]
            };
            // EG 20150313 (POC] New GProduct remplace pGProduct
            idInfo.idInfos[0] = new DictionaryEntry("GPRODUCT", GProduct);
            idInfo.idInfos[1] = new DictionaryEntry("REQUESTTYPE", requestType);
            idInfo.idInfos[2] = new DictionaryEntry("DTBUSINESS", dtBusiness);
            if (identifiersSpecified)
            {
                idInfo.idInfos[3] = new DictionaryEntry("ENTITY", identifiers.entity);
                idInfo.idInfos[4] = new DictionaryEntry("CSSCUSTODIAN", identifiers.cssCustodian);
                idInfo.idInfos[5] = new DictionaryEntry("MARKET", identifiers.market);
                idInfo.idInfos[6] = new DictionaryEntry("INSTRUMENT", identifiers.instrument);
                idInfo.idInfos[7] = new DictionaryEntry("ASSET", identifiers.asset);
                idInfo.idInfos[8] = new DictionaryEntry("TRADE", identifiers.trade);
                idInfo.idInfos[9] = new DictionaryEntry("DEALER", identifiers.dealer);
                idInfo.idInfos[10] = new DictionaryEntry("BOOKDEALER", identifiers.bookDealer);
                idInfo.idInfos[11] = new DictionaryEntry("CLEARER", identifiers.clearer);
                idInfo.idInfos[12] = new DictionaryEntry("BOOKCLEARER", identifiers.bookClearer);
            }
            return idInfo;
        }
        #endregion IPosRequest Members
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
    /// EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
    public partial class PosRequestUpdateEntry : IPosRequestUpdateEntry
    {
        #region Accessors
        public override string RequestMessage
        {
            get
            {
                return RequestMessageByKeyPos(string.Empty);
            }
        }
        #endregion Accessors
        #region Constructors
        public PosRequestUpdateEntry() : base(Cst.PosRequestTypeEnum.UpdateEntry) { }
        public PosRequestUpdateEntry(SettlSessIDEnum pRequestMode, DateTime pDtBusiness)
            : base(Cst.PosRequestTypeEnum.UpdateEntry, pRequestMode, pDtBusiness, 0)
        {
        }
        #endregion Constructors
        #region IPosRequestUpdateEntry Members
        IPosRequestDetUpdateEntry IPosRequestUpdateEntry.Detail
        {
            set { detail = (PosRequestDetUpdateEntry)value; }
            get { return detail; }
        }
        // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
        void IPosRequestUpdateEntry.SetDetail(IPosKeepingTrade pTrade)
        {
            detail = new PosRequestDetUpdateEntry
            {
                dtBusiness = pTrade.DtBusiness,
                side = pTrade.Side,
                qty = pTrade.Qty,
                dtExecution = pTrade.DtExecution,
                positionEffect = pTrade.PositionEffect
            };
        }
        #endregion PosRequestEntry Members
        #region IPosRequest Members
        object IPosRequest.DetailBase
        {
            set { detail = (PosRequestDetUpdateEntry)value; }
            get { return detail; }
        }
        Type IPosRequest.DetailType
        {
            get { return typeof(PosRequestDetUpdateEntry); }
        }
        int IPosRequest.NbTokenIdE
        {
            // IDE = 4 EVTs potentiels * 2 (clôturée et clôturante)
            // . OFS/TOT-PAR
            // . TER-INT/NOM
            // . TER-INT/QTY
            // . LPC/RMG
            // . LPC/VMG
            get { return 5 * 2; }
        }
        // EG 20150317 [POC] Mod GetMQueueIdInfo (no parameter use gProduct)
        IdInfo IPosRequest.GetMQueueIdInfo()
        {
            IdInfo idInfo = new IdInfo
            {
                id = idT,
                idInfos = new DictionaryEntry[13]
            };
            // EG 20150313 (POC] New GProduct remplace pGProduct
            idInfo.idInfos[0] = new DictionaryEntry("GPRODUCT", GProduct);
            idInfo.idInfos[1] = new DictionaryEntry("REQUESTTYPE", requestType);
            idInfo.idInfos[2] = new DictionaryEntry("DTBUSINESS", dtBusiness);
            if (identifiersSpecified)
            {
                idInfo.idInfos[3] = new DictionaryEntry("ENTITY", identifiers.entity);
                idInfo.idInfos[4] = new DictionaryEntry("CSSCUSTODIAN", identifiers.cssCustodian);
                idInfo.idInfos[5] = new DictionaryEntry("MARKET", identifiers.market);
                idInfo.idInfos[6] = new DictionaryEntry("INSTRUMENT", identifiers.instrument);
                idInfo.idInfos[7] = new DictionaryEntry("ASSET", identifiers.asset);
                idInfo.idInfos[8] = new DictionaryEntry("TRADE", identifiers.trade);
                idInfo.idInfos[9] = new DictionaryEntry("DEALER", identifiers.dealer);
                idInfo.idInfos[10] = new DictionaryEntry("BOOKDEALER", identifiers.bookDealer);
                idInfo.idInfos[11] = new DictionaryEntry("CLEARER", identifiers.clearer);
                idInfo.idInfos[12] = new DictionaryEntry("BOOKCLEARER", identifiers.bookClearer);
            }
            return idInfo;
        }
        #endregion IPosRequest Members
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
    public abstract partial class PosRequestDetail : IPosRequestDetail
    {
        #region Constructors
        public PosRequestDetail()
        {
            EfsMLversion = EfsMLDocumentVersionEnum.Version30;
        }
        #endregion Constructors
        #region IPosRequestDetail Members
        EfsMLDocumentVersionEnum IPosRequestDetail.EfsMLversion
        {
            set { EfsMLversion = value; }
            get { return EfsMLversion; }
        }
        int IPosRequestDetail.NbAdditionalEvents
        {
            set { nbAdditionalEvents = value; }
            get { return nbAdditionalEvents; }
        }
        bool IPosRequestDetail.PaymentFeesSpecified
        {
            set { ;}
            get { return false; }
        }
        IPayment[] IPosRequestDetail.PaymentFees
        {
            set { ; }
            get { return null; }
        }
        void IPosRequestDetail.SetPaymentFees(IPayment[] pPaymentFees)
        {
        }
        // EG 20150716 [21103] New 
        bool IPosRequestDetail.IsReversalSafekeepingSpecified
        {
            set { ;}
            get { return false; }
        }
        bool IPosRequestDetail.IsReversalSafekeeping
        {
            set { ; }
            get { return false; }
        }
        bool IPosRequestDetail.FeeCalculationSpecified
        {
            set { ;}
            get { return false; }
        }
        bool IPosRequestDetail.FeeCalculation
        {
            set { ;}
            get { return false; }
        }
        #endregion IPosRequestDetail Members
    }
    #endregion PosRequestDetail
    #region PosRequestDetCorporateAction
    /// <summary>
    /// <para>Détail d'un trade ajusté issu d'une CA</para>
    /// </summary>
    /// EG 20140516 [19816] Add contractSymbolDCEx
    public partial class PosRequestDetCorporateAction : IPosRequestDetCorporateAction
    {
        #region Constructors
        public PosRequestDetCorporateAction() { }
        /// EG 20140516 [19816] Add contractSymbolDCEx
        public PosRequestDetCorporateAction(int pIdDCEx, string pIdentifierDCEx, string pContractSymbolDCEx, DateTime pBusinessDate)
        {
            idDCEx = pIdDCEx;
            identifierDCEx = pIdentifierDCEx;
            contractSymbolDCEx = pContractSymbolDCEx;
            businessDate = pBusinessDate;
        }
        #endregion Constructors
        #region IPosRequestDetCorporateAction Members
        int IPosRequestDetCorporateAction.IdDCEx
        {
            set { idDCEx = value; }
            get { return idDCEx; }
        }
        string IPosRequestDetCorporateAction.IdentifierDCEx
        {
            set { identifierDCEx = value; }
            get { return identifierDCEx; }
        }
        /// EG 20140516 [19816] Add contractSymbolDCEx
        string IPosRequestDetCorporateAction.ContractSymbolDCEx
        {
            set { contractSymbolDCEx = value; }
            get { return contractSymbolDCEx; }
        }
        DataDocumentContainer IPosRequestDetCorporateAction.DataDocument
        {
            set { dataDocument = value; }
            get { return dataDocument; }
        }
        DateTime IPosRequestDetCorporateAction.BusinessDate
        {
            set { businessDate = value; }
            get { return businessDate; }
        }
        IPosRequestTradeAdditionalInfo IPosRequestDetCorporateAction.CreateAdditionalInfo(DataDocumentContainer pTemplateDataDocument,
            SQL_TradeCommon pSQLTrade, SQL_Product pSQLProduct, SQL_Instrument pSQLInstrument, string pScreenName)
        {
            return PosRequestTradeExAdditionalInfo.CreateAdditionalInfo(pTemplateDataDocument, pSQLTrade, pSQLProduct, pSQLInstrument, pScreenName);
        }

        #endregion IPosRequestDetCorporateAction Members
    }
    #endregion PosRequestDetCorporateAction
    #region PosRequestDetCascadingShifting
    /// <summary>
    /// <para>Détail d'un contrat issu d'un cascading ou shifting</para>
    /// </summary>
    public partial class PosRequestDetCascadingShifting : IPosRequestDetCascadingShifting
    {
        #region Constructors
        public PosRequestDetCascadingShifting() { }
        /// EG 20171107 [23509] pDtExecution
        public PosRequestDetCascadingShifting(int pIdDCDest, string pIdentifierDCDest, string pMaturityDest, DateTime pDtExecution)
        {
            idDCDest = pIdDCDest;
            identifierDCDest = pIdentifierDCDest;
            maturityMonthYearDest = pMaturityDest;
            dtExecution = pDtExecution;
        }
        #endregion Constructors
        /// EG 20171003 [23452] tradeDateTime
        #region IPosRequestDetCascadingShifting Members
        int IPosRequestDetCascadingShifting.IdDCDest
        {
            set { idDCDest = value; }
            get { return idDCDest; }
        }
        string IPosRequestDetCascadingShifting.IdentifierDCDest
        {
            set { identifierDCDest = value; }
            get { return identifierDCDest; }
        }
        string IPosRequestDetCascadingShifting.MaturityMonthYearDest
        {
            set { maturityMonthYearDest = value; }
            get { return maturityMonthYearDest; }
        }
        DataDocumentContainer IPosRequestDetCascadingShifting.DataDocument
        {
            set { dataDocument = value; }
            get { return dataDocument; }
        }
        DateTime IPosRequestDetCascadingShifting.DtExecution
        {
            set { dtExecution = value; }
            get { return dtExecution; }
        }
        IPosRequestTradeAdditionalInfo IPosRequestDetCascadingShifting.CreateAdditionalInfo(DataDocumentContainer pTemplateDataDocument,
            SQL_TradeCommon pSQLTrade, SQL_Product pSQLProduct, SQL_Instrument pSQLInstrument, string pScreenName)
        {
            return PosRequestUnderlyerAdditionalInfo.CreateAdditionalInfo(pTemplateDataDocument, pSQLTrade, pSQLProduct, pSQLInstrument, pScreenName);
        }

        #endregion IPosRequestDetCascadingShifting Members
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
    public partial class PosRequestDetClearingBLK : IPosRequestDetClearingBLK
    {
        #region Constructors
        public PosRequestDetClearingBLK() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestDetClearingBLK(decimal pAvailableQtyBuy, decimal pAvailableQtySell)
        {
            availableQtyBuy = pAvailableQtyBuy;
            availableQtySell = pAvailableQtySell;
        }
        #endregion Constructors
        #region IPosRequestDetClearingBLK Members
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetClearingBLK.AvailableQtyBuy
        {
            set { availableQtyBuy = value; }
            get { return availableQtyBuy; }
        }
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetClearingBLK.AvailableQtySell
        {
            set { availableQtySell = value; }
            get { return availableQtySell; }
        }
        #endregion IPosRequestDetClearingBLK Members
    }
    #endregion PosRequestDetClearingBLK
    #region PosRequestDetClearingSPEC
    /// <summary>
    /// <para>Détail d'une demande de COMPENSATION SPECIFIQUE (REQUESTTYPE = CLEARSPEC)</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques</para>
    /// <para>  ● Id du trade à compenser</para>
    /// <para>  ● Quantité disponible du trade à compenser</para>
    /// <para>  ● Quantité à compenser sur le trade</para>
    /// <para>  ● DtBusiness</para>
    /// <para>  ● DtTimeStamp</para>
    /// <para>───────────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    public partial class PosRequestDetClearingSPEC : IPosRequestDetClearingSPEC
    {
        #region Constructors
        public PosRequestDetClearingSPEC() { }
        public PosRequestDetClearingSPEC(IPosKeepingClearingTrade[] pTradesTarget)
        {
            tradesTarget = new PosKeepingClearingTrade[pTradesTarget.Length];
            for (int i = 0; i < pTradesTarget.Length; i++)
                tradesTarget[i] = (PosKeepingClearingTrade)pTradesTarget[i];
            //tradesTarget = (PosKeepingClearingTrade[])pTradesTarget;
        }
        #endregion Constructors
        #region IPosRequestDetClearingSPEC Members
        IPosKeepingClearingTrade[] IPosRequestDetClearingSPEC.TradesTarget
        {
            set { tradesTarget = (PosKeepingClearingTrade[])value; }
            get { return tradesTarget; }
        }
        #endregion IPosRequestDetClearingSPEC Members
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
    public partial class PosRequestDetCorrection : IPosRequestDetCorrection
    {
        #region Constructors
        public PosRequestDetCorrection() { }
        // EG 20150907 [21317] Add InitialQty
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestDetCorrection(decimal pInitialQty, decimal pAvailableQty)
        {
            initialQty = pInitialQty;
            availableQty = pAvailableQty;
        }
        #endregion Constructors
        #region IPosRequestDetCorrection Members
        // EG 20150907 [21317] Add InitialQty
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetCorrection.InitialQty
        {
            set { initialQty = value; }
            get { return initialQty; }
        }
        decimal IPosRequestDetCorrection.AvailableQty
        {
            set { availableQty = value; }
            get { return availableQty; }
        }
        #endregion IPosRequestDetCorrection Members
        #region IPosRequestDetail Members
        bool IPosRequestDetail.PaymentFeesSpecified
        {
            set { paymentFeesSpecified = value; }
            get { return paymentFeesSpecified; }
        }
        IPayment[] IPosRequestDetail.PaymentFees
        {
            set { paymentFees = (Payment[])value; }
            get { return paymentFees; }
        }
        // EG 20150716 [21103] New 
        bool IPosRequestDetail.IsReversalSafekeepingSpecified
        {
            set { isReversalSafekeepingSpecified = value; }
            get { return isReversalSafekeepingSpecified; }
        }
        bool IPosRequestDetail.IsReversalSafekeeping
        {
            set { isReversalSafekeeping = value; }
            get { return isReversalSafekeeping; }
        }
        void IPosRequestDetail.SetPaymentFees(IPayment[] pPaymentFees)
        {
            paymentFeesSpecified = (ArrFunc.IsFilled(pPaymentFees));
            if (paymentFeesSpecified)
                paymentFees = (Payment[])pPaymentFees;
        }
        #endregion IPosRequestDetail Members
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
    // EG 20171025 [23509] Upd dtExecution replace dtTimestamp 
    public partial class PosRequestDetEntry : IPosRequestDetEntry
    {
        #region Constructors
        public PosRequestDetEntry() { }
        #endregion Constructors
        #region IPosRequestDetEntry Members
        DateTime IPosRequestDetEntry.DtBusiness
        {
            set { dtBusiness = value; }
            get { return dtBusiness; }
        }
        int IPosRequestDetEntry.Side
        {
            set { side = value; }
            get { return side; }
        }
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetEntry.Qty
        {
            set { qty = value; }
            get { return qty; }
        }
        DateTime IPosRequestDetEntry.DtExecution
        {
            set { dtExecution = value; }
            get { return dtExecution; }
        }
        string IPosRequestDetEntry.PositionEffect
        {
            set { positionEffect = value; }
            get { return positionEffect; }
        }
        PosKeepingEntryMQueue IPosRequestDetEntry.Message
        {
            set { message = value; }
            get { return message; }
        }
        #endregion IPosRequestDetEntry Members
    }
    #endregion PosRequestDetEntry

    #region PosRequestDetOption
    /// <summary>
    /// <para>Détail pour un DENOUEMENT D'OPTIONS (REQUESTTYPE = ABN, NEX, NAS, AAB, AAS, ASS, AEX, EXE)</para>
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
    // EG 20151019 [21465] Add isApplyScheduleFeeSpecified|isApplyScheduleFee
    public partial class PosRequestDetOption : IPosRequestDetOption
    {
        #region Constructors
        public PosRequestDetOption() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestDetOption(decimal pAvailableQty, decimal pStrikePrice)
            : base()
        {
            availableQty = pAvailableQty;
            strikePrice = pStrikePrice;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20151019 [21465] Add Nullable on parameters pQuoteTiming|pPrice|pDtPrice
        // EG 20170127 Qty Long To Decimal
        public PosRequestDetOption(string pCS, Cst.PosRequestTypeEnum pRequestTypeOption, int pIdTOption, decimal pAvailableQty,
            decimal pStrikePrice, Cst.UnderlyingAsset pAssetCategory, int pIdAsset, string pIdentifier,
            Nullable<QuoteTimingEnum> pQuoteTiming, Nullable<decimal> pPrice, Nullable<DateTime> pDtQuote, string pSource)
            : this(pAvailableQty, pStrikePrice)
        {
            ((IPosRequestDetOption)this).SetUnderlyer(pCS, pRequestTypeOption, pIdTOption, pAssetCategory, pIdAsset, pIdentifier, pQuoteTiming, pPrice, pDtQuote, pSource);
        }
        #endregion Constructors
        #region IPosRequestDetOption Members
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetOption.AvailableQty
        {
            set { availableQty = value; }
            get { return availableQty; }
        }
        decimal IPosRequestDetOption.StrikePrice
        {
            set { strikePrice = value; }
            get { return strikePrice; }
        }
        IPosRequestDetUnderlyer IPosRequestDetOption.Underlyer
        {
            set { underlyer = (PosRequestDetUnderlyer)value; }
            get { return underlyer; }
        }
        bool IPosRequestDetOption.UnderlyerSpecified
        {
            set { underlyerSpecified = value; }
            get { return underlyerSpecified; }
        }
        // EG 20151019 [21465] Add Nullable on parameters pQuoteTiming|pPrice|pDtPrice
        void IPosRequestDetOption.SetUnderlyer(string pCS, Cst.PosRequestTypeEnum pRequestTypeOption, int pIdTOption,
            Cst.UnderlyingAsset pAssetCategory, int pIdAsset, string pIdentifier,
            Nullable<QuoteTimingEnum> pQuoteTiming, Nullable<decimal> pPrice, Nullable<DateTime> pDtPrice, string pSource)
        {
            underlyerSpecified = true;
            underlyer = new PosRequestDetUnderlyer(pCS, pRequestTypeOption, pIdTOption, pAssetCategory, pIdAsset, pIdentifier, pQuoteTiming, pPrice, pDtPrice, pSource);
        }
        // EG 20151102 [21465] New
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetOption.GetTotalQtySource()
        {
            decimal totalQty = 0;
            foreach (PosRequestSource item in underlyer.posRequestSource)
                totalQty += item.qty;
            return totalQty;
        }

        #endregion IPosRequestDetOption Members
        #region IPosRequestDetail Members
        bool IPosRequestDetail.FeeCalculationSpecified
        {
            set { feeCalculationSpecified = value; }
            get { return feeCalculationSpecified; }
        }
        bool IPosRequestDetail.FeeCalculation
        {
            set { feeCalculation = value; }
            get { return feeCalculation; }
        }
        bool IPosRequestDetail.PaymentFeesSpecified
        {
            set { paymentFeesSpecified = value; }
            get { return paymentFeesSpecified; }
        }
        IPayment[] IPosRequestDetail.PaymentFees
        {
            set { paymentFees = (Payment[])value; }
            get { return paymentFees; }
        }

        void IPosRequestDetail.SetPaymentFees(IPayment[] pPaymentFees)
        {
            paymentFeesSpecified = (ArrFunc.IsFilled(pPaymentFees));
            if (paymentFeesSpecified)
                paymentFees = (Payment[])pPaymentFees;
        }
        #endregion IPosRequestDetail Members
    }
    #endregion PosRequestDetOption


    /// <summary>
    /// 
    /// </summary>
    /// FI 20130315 [18467] add PosRequestDetPositionOption
    public partial class PosRequestDetPositionOption : IPosRequestDetPositionOption
    {
        #region IPosRequestDetail Membres
        // RD 20200120 [25114] Add
        bool IPosRequestDetail.FeeCalculationSpecified
        {
            set { feeCalculationSpecified = value; }
            get { return feeCalculationSpecified; }
        }
        // RD 20200120 [25114] Add
        bool IPosRequestDetail.FeeCalculation
        {
            set { feeCalculation = value; }
            get { return feeCalculation; }
        }
        IPayment[] IPosRequestDetail.PaymentFees
        {
            set { paymentFees = (Payment[])value; }
            get { return paymentFees; }
        }

        void IPosRequestDetail.SetPaymentFees(IPayment[] pPaymentFees)
        {
            paymentFeesSpecified = (ArrFunc.IsFilled(pPaymentFees));
            if (paymentFeesSpecified)
                paymentFees = (Payment[])pPaymentFees;
        }
        #endregion

        #region IPosRequestDetPositionOption Membres

        Cst.Capture.ModeEnum IPosRequestDetPositionOption.CaptureMode
        {
            get
            {
                return captureMode;
            }
            set
            {
                captureMode = value;
            }
        }

        bool IPosRequestDetPositionOption.PartialExecutionAllowed
        {
            get
            {
                return partialExecutionAllowed;
            }
            set
            {
                partialExecutionAllowed = value;
            }
        }
        // EG 20160404 Migration vs2013
        //bool IPosRequestDetPositionOption.feeCalculation
        //{
        //    get
        //    {
        //        return feeCalculation;
        //    }
        //    set
        //    {
        //        feeCalculation = true;
        //    }
        //}

        bool IPosRequestDetPositionOption.AbandonRemainingQty
        {
            get
            {
                return abandonRemainingQty;
            }
            set
            {
                abandonRemainingQty = true;
            }
        }

        #endregion
    }
    
    #region PosRequestDetRemoveAlloc
    /// <summary>
    /// <para>Détail pour un ANNULATION D'ALLOCATION (REQUESTTYPE = RMVALLOC)</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques du trade allocation à annuler nécessaires à la décompensation éventuelle</para>
    /// <para>  ● Quantité initiale</para>
    /// <para>  ● Quantité en position</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    public partial class PosRequestDetRemoveAlloc : IPosRequestDetRemoveAlloc
    {
        #region Constructors
        public PosRequestDetRemoveAlloc() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestDetRemoveAlloc(decimal pInitialQty, decimal pPositionQty)
            : base()
        {
            initialQty = pInitialQty;
            positionQty = pPositionQty;
        }
        #endregion Constructors
        #region IPosRequestDetRemoveAlloc Members
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetRemoveAlloc.InitialQty
        {
            set { initialQty = value; }
            get { return initialQty; }
        }
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetRemoveAlloc.PositionQty
        {
            set { positionQty = value; }
            get { return positionQty; }
        }
        #endregion IPosRequestDetRemoveAlloc Members
    }
    #endregion PosRequestDetRemoveAlloc
    #region PosRequestDetSplit
    /// <summary>
    /// <para>Détail pour un TRADESPLITTING (REQUESTTYPE = TRADESPLITTING)</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// <para>► Caractéristiques du trade allocation à annuler nécessaires à la décompensation éventuelle</para>
    /// <para>  ● Quantité initiale</para>
    /// <para>  ● Quantité en position</para>
    /// <para>────────────────────────────────────────────────────────────────────────────────</para>
    /// </summary>
    public partial class PosRequestDetSplit : IPosRequestDetSplit
    {
        #region Constructors
        public PosRequestDetSplit() { }
        public PosRequestDetSplit(ArrayList pNewTrades)
            : base()
        {
            Set(pNewTrades);
        }
        #endregion Constructors
        #region IPosRequestDetSplit Members
        ArrayList IPosRequestDetSplit.NewTrades
        {
            set 
            {
                Set(value);
            }
            get 
            {
                ArrayList al = new ArrayList();
                foreach (SplitNewTrade snt in splitNewTrades)
                {
                    al.Add(snt);
                }
                return al; 
            }
        }
        #endregion IPosRequestDetSplit Members
        private void Set(ArrayList value)
        {
            splitNewTrades = new SplitNewTrade[value.Count];
            int i = 0;
            foreach (object obj in value)
            {
                splitNewTrades[i++] = (SplitNewTrade)obj;
            }
        }
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
    // EG 20150716 [21103] Add Safekeeping
    // EG 20170127 Qty Long To Decimal
    public partial class PosRequestDetTransfer : IPosRequestDetTransfer
    {
        #region Constructors
        public PosRequestDetTransfer() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestDetTransfer(decimal pInitialQty, decimal pPositionQty)
            : base()
        {
            initialQty = pInitialQty;
            positionQty = pPositionQty;
        }
        #endregion Constructors
        #region IPosRequestDetTransfer Members
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetTransfer.InitialQty
        {
            set { initialQty = value; }
            get { return initialQty; }
        }
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetTransfer.PositionQty
        {
            set { positionQty = value; }
            get { return positionQty; }
        }
        IParty IPosRequestDetTransfer.DealerTarget
        {
            set { dealerTarget = (Party)value; }
            get { return dealerTarget; }
        }
        bool IPosRequestDetTransfer.DealerTargetSpecified
        {
            get { return (null != dealerTarget); }
        }
        IBookId IPosRequestDetTransfer.DealerBookIdTarget
        {
            set { dealerBookIdTarget = (BookId)value; }
            get { return dealerBookIdTarget; }
        }
        bool IPosRequestDetTransfer.DealerBookIdTargetSpecified
        {
            get { return (null != dealerTarget) && (null != dealerBookIdTarget); }
        }
        IParty IPosRequestDetTransfer.ClearerTarget
        {
            set { clearerTarget = (Party)value; }
            get { return clearerTarget; }
        }
        bool IPosRequestDetTransfer.ClearerTargetSpecified
        {
            get { return (null != clearerTarget); }
        }
        IBookId IPosRequestDetTransfer.ClearerBookIdTarget
        {
            set { clearerBookIdTarget = (BookId)value; }
            get { return clearerBookIdTarget; }
        }
        bool IPosRequestDetTransfer.ClearerBookIdTargetSpecified
        {
            get { return (null != clearerTarget) && (null != clearerBookIdTarget); }
        }
        int IPosRequestDetTransfer.IdTReplace
        {
            set { idTReplace = value; }
            get { return idTReplace; }
        }
        bool IPosRequestDetTransfer.IdTReplaceSpecified
        {
            set { idTReplaceSpecified = value; }
            get { return idTReplaceSpecified; }
        }
        string IPosRequestDetTransfer.Trade_IdentifierReplace
        {
            set { trade_IdentifierReplace = value; }
            get { return trade_IdentifierReplace; }
        }
        bool IPosRequestDetTransfer.Trade_IdentifierReplaceSpecified
        {
            set { trade_IdentifierReplaceSpecified = value; }
            get { return trade_IdentifierReplaceSpecified; }
        }
        bool IPosRequestDetTransfer.IsReversalFeesSpecified
        {
            set { isReversalFeesSpecified = value; }
            get { return isReversalFeesSpecified; }
        }
        bool IPosRequestDetTransfer.IsReversalFees
        {
            set { isReversalFees = value; }
            get { return isReversalFees; }
        }
        bool IPosRequestDetTransfer.IsCalcNewFeesSpecified
        {
            set { isCalcNewFeesSpecified = value; }
            get { return isCalcNewFeesSpecified; }
        }

        bool IPosRequestDetTransfer.IsCalcNewFees
        {
            set { isCalcNewFees = value; }
            get { return isCalcNewFees; }
        }
        // EG 20150716 [21103]
        // EG 20160404 Migration vs2013
        //bool IPosRequestDetTransfer.isReversalSafekeepingSpecified
        //{
        //    set { isReversalSafekeepingSpecified = value; }
        //    get { return isReversalSafekeepingSpecified; }
        //}
        // EG 20150716 [21103]
        // EG 20160404 Migration vs2013
        //bool IPosRequestDetTransfer.isReversalSafekeeping
        //{
        //    set { isReversalSafekeeping = value; }
        //    get { return isReversalSafekeeping; }
        //}

        #endregion IPosRequestDetTransfer Members
        #region IPosRequestDetail Members
        bool IPosRequestDetail.PaymentFeesSpecified
        {
            set { paymentFeesSpecified = value; }
            get { return paymentFeesSpecified; }
        }
        IPayment[] IPosRequestDetail.PaymentFees
        {
            set { paymentFees = (Payment[])value; }
            get { return paymentFees; }
        }

        void IPosRequestDetail.SetPaymentFees(IPayment[] pPaymentFees)
        {
            paymentFeesSpecified = (ArrFunc.IsFilled(pPaymentFees));
            if (paymentFeesSpecified)
                paymentFees = (Payment[])pPaymentFees;
        }

        #endregion IPosRequestDetail Members
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
    public partial class PosRequestDetUnclearing : IPosRequestDetUnclearing
    {
        #region Constructors
        public PosRequestDetUnclearing() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public PosRequestDetUnclearing(int pIdPR, int pIdPADET, Cst.PosRequestTypeEnum pRequestType, DateTime pDtBusiness,
            int pIdT_Closing, string pClosing_Identifier, decimal pClosingQty)
        {
            idPR = pIdPR;
            idPADET = pIdPADET;
            idT_Closing = pIdT_Closing;
            closing_Identifier = pClosing_Identifier;
            requestType = pRequestType;
            dtBusiness = pDtBusiness;
            closingQty = pClosingQty;
        }
        #endregion Constructors
        #region IPosRequestDetUnclearing Members
        Cst.PosRequestTypeEnum IPosRequestDetUnclearing.RequestType
        {
            set { requestType = value; }
            get { return requestType; }
        }
        DateTime IPosRequestDetUnclearing.DtBusiness
        {
            set { dtBusiness = value; }
            get { return dtBusiness; }
        }
        int IPosRequestDetUnclearing.IdPR
        {
            set { idPR = value; }
            get { return idPR; }
        }
        int IPosRequestDetUnclearing.IdPADET
        {
            set { idPADET = value; }
            get { return idPADET; }
        }
        int IPosRequestDetUnclearing.IdT_Closing
        {
            set { idT_Closing = value; }
            get { return idT_Closing; }
        }
        string IPosRequestDetUnclearing.Closing_Identifier
        {
            set { closing_Identifier = value; }
            get { return closing_Identifier; }
        }
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetUnclearing.ClosingQty
        {
            set { closingQty = value; }
            get { return closingQty; }
        }
        bool IPosRequestDetUnclearing.IdT_DeliverySpecified
        {
            set { idT_DeliverySpecified = value; }
            get { return idT_DeliverySpecified; }
        }
        int IPosRequestDetUnclearing.IdT_Delivery
        {
            set { idT_Delivery = value; }
            get { return idT_Delivery; }
        }
        bool IPosRequestDetUnclearing.Delivery_IdentifierSpecified
        {
            set { delivery_IdentifierSpecified = value; }
            get { return delivery_IdentifierSpecified; }
        }
        string IPosRequestDetUnclearing.Delivery_Identifier
        {
            set { delivery_Identifier = value; }
            get { return delivery_Identifier; }
        }
        #endregion IPosRequestDetUnclearing Members
    }
    #endregion PosRequestDetUnclearing
    #region PosRequestSource
    // EG 20151102 [21465] New
    // EG 20170127 Qty Long To Decimal
    public partial class PosRequestSource 
    {
        #region Constructors
        public PosRequestSource() { }
        public PosRequestSource(int pIdPR, decimal pQty)
        {
            idPR = pIdPR;
            qty = pQty;
        }
        #endregion Constructors
    }
    #endregion PosRequestSource

    #region PosRequestDetUnderlyer
    /// <summary>
    /// <para>Détail d'un actif sous-jacent d'une option</para>
    /// <para>Utilisé sdans le cas des dénouements d'options</para>
    /// </summary>
    public partial class PosRequestDetUnderlyer : IPosRequestDetUnderlyer
    {
        #region Constructors
        public PosRequestDetUnderlyer() { }
        // EG 20151019 [21465] Add Nullable on parameters pQuoteTiming|pPrice|pDtPrice
        public PosRequestDetUnderlyer(string pCS, Cst.PosRequestTypeEnum pRequestTypeOption, int pIdTOption, Cst.UnderlyingAsset pAssetCategory, int pIdAsset, string pIdentifier,
            Nullable<QuoteTimingEnum> pQuoteTiming, Nullable<decimal> pPrice, Nullable<DateTime> pDtPrice, string pSource)
        {
            requestTypeSource = pRequestTypeOption;
            idTSource = pIdTOption;
            assetCategory = pAssetCategory;
            idAsset = pIdAsset;
            identifier = pIdentifier;
            quoteTimingSpecified = pQuoteTiming.HasValue;
            if (quoteTimingSpecified)
                quoteTiming = pQuoteTiming.Value;
            priceSpecified = pPrice.HasValue;
            if (priceSpecified)
                price = pPrice.Value;
            dtPriceSpecified = pPrice.HasValue;
            if (dtPriceSpecified)
                dtPrice = pDtPrice.Value;
            source = pSource;
            GetLinkedIdI(pCS);
        }
        #endregion Constructors
        #region Methods
        #region GetLinkedIdI
        // EG 20141103 Add Test on ExchangeTradedContract
        private void GetLinkedIdI(string pCS)
        {
            try
            {
                if (assetCategory == Cst.UnderlyingAsset.EquityAsset)
                {
                    SQL_AssetEquity sql_AssetEquity = new SQL_AssetEquity(pCS, idAsset);
                    sql_AssetEquity.LoadTable(new string[] { "IDASSET", "IDI" });
                    if (sql_AssetEquity.IsLoaded)
                        idI = sql_AssetEquity.IdInstrument;
                    // PM 20150305 [POC] Prendre l'Instrument 'share' du Product 'equitySecurityTransaction' lorsque l'instrument n'est pas renseigné
                    if (0 == idI)
                    {
                        SQL_Instrument sqlInstrument = new SQL_Instrument(pCS, "share");
                        if (sqlInstrument.IsLoaded)
                        {
                            idI = sqlInstrument.IdI;
                        }
                    }
                }
                else if ((assetCategory == Cst.UnderlyingAsset.Future) || 
                    (assetCategory == Cst.UnderlyingAsset.ExchangeTradedContract))
                {
                    SQL_AssetETD sql_AssetETD = new SQL_AssetETD(pCS, idAsset);
                    sql_AssetETD.LoadTable(new string[] { "IDASSET", "IDI" });
                    if (sql_AssetETD.IsLoaded)
                        idI = sql_AssetETD.DrvContract_IdInstrument;
                }
            }
            catch (Exception) { throw; }
        }
        #endregion GetLinkedIdI
        #endregion Methods
        #region IPosRequestDetUnderlyer Members
        // EG 20151102 [21465] New
        PosRequestSource[] IPosRequestDetUnderlyer.PosRequestSource
        {
            set { posRequestSource = value; }
            get { return posRequestSource; }
        }
        //int IPosRequestDetUnderlyer.idPRSource
        //{
        //    set { idPRSource = value; }
        //    get { return idPRSource; }
        //}

        Cst.PosRequestTypeEnum IPosRequestDetUnderlyer.RequestTypeSource
        {
            set { requestTypeSource = value; }
            get { return requestTypeSource; }
        }
        int IPosRequestDetUnderlyer.IdTSource
        {
            set { idTSource = value; }
            get { return idTSource; }
        }
        Cst.UnderlyingAsset IPosRequestDetUnderlyer.AssetCategory
        {
            set { assetCategory = value; }
            get { return assetCategory; }
        }
        int IPosRequestDetUnderlyer.IdAsset
        {
            set { idAsset = value; }
            get { return idAsset; }
        }
        string IPosRequestDetUnderlyer.Identifier
        {
            set { identifier = value; }
            get { return identifier; }
        }
        QuoteTimingEnum IPosRequestDetUnderlyer.QuoteTiming
        {
            set { quoteTiming = value; }
            get { return quoteTiming; }
        }
        decimal IPosRequestDetUnderlyer.Price
        {
            set { price = value; }
            get { return price; }
        }
        DateTime IPosRequestDetUnderlyer.DtPrice
        {
            set { dtPrice = value; }
            get { return dtPrice; }
        }
        string IPosRequestDetUnderlyer.Source
        {
            set { source = value; }
            get { return source; }
        }
        int IPosRequestDetUnderlyer.IdI
        {
            set { idI = value; }
            get { return idI; }
        }
        DataDocumentContainer IPosRequestDetUnderlyer.DataDocument
        {
            set { dataDocument = value; }
            get { return dataDocument; }
        }
        // EG 20151102 [21465] New
        // EG 20170127 Qty Long To Decimal
        void IPosRequestDetUnderlyer.SetPosRequestSource(int pIdPR, decimal pQty)
        {
            PosRequestSource source = this[pIdPR];
            if (null == source)
            {
                ArrayList aPosRequestSource = new ArrayList();
                if (ArrFunc.IsFilled(posRequestSource))
                    aPosRequestSource.AddRange(posRequestSource);
                aPosRequestSource.Add(new PosRequestSource(pIdPR, pQty));
                posRequestSource = (PosRequestSource[])aPosRequestSource.ToArray(typeof(PosRequestSource));
            }
            else
            {
                source.qty = pQty;
            }
        }
        // EG 20151102 [21465] New
        void IPosRequestDetUnderlyer.DeletePosRequestSource(int pIdPR)
        {
            ArrayList aPosRequestSource = new ArrayList();
            foreach (PosRequestSource item in posRequestSource)
            {
                if (item.idPR != pIdPR)
                    aPosRequestSource.Add(item);
            }
            posRequestSource = (PosRequestSource[])aPosRequestSource.ToArray(typeof(PosRequestSource));
        }

        // EG 20151102 [21465] New
        int[] IPosRequestDetUnderlyer.GetIdPRSource()
        {
            int[] idPRSource = null;
            foreach(PosRequestSource item in posRequestSource)
            {
                ArrayList aIdPRSource = new ArrayList
                {
                    item.idPR
                };
                idPRSource = (int[])aIdPRSource.ToArray(typeof(int));
            }
            return idPRSource;
        }


        IPosRequestTradeAdditionalInfo IPosRequestDetUnderlyer.CreateAdditionalInfo(DataDocumentContainer pTemplateDataDocument,
            SQL_TradeCommon pSQLTrade, SQL_Product pSQLProduct, SQL_Instrument pSQLInstrument, string pScreenName)
        {
            // PM 20130219 [18414] Remplacement par un méthode static de PosRequestUnderlyerAdditionalInfo
            //PosRequestUnderlyerAdditionalInfo additionalInfo = new PosRequestUnderlyerAdditionalInfo();
            //additionalInfo.templateDataDocument = pTemplateDataDocument;
            //additionalInfo.sqlTemplateTrade = pSQLTrade;
            //additionalInfo.sqlProduct = pSQLProduct;
            //additionalInfo.sqlInstrument = pSQLInstrument;
            //additionalInfo.screenName = pScreenName;
            //return additionalInfo;
            return PosRequestUnderlyerAdditionalInfo.CreateAdditionalInfo(pTemplateDataDocument, pSQLTrade, pSQLProduct, pSQLInstrument, pScreenName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20140116 Report v3.7 
        // EG 20140904 Add AssetCategory
        // EG 20180307 [23769] Gestion dbTransaction
        EFS_Asset IPosRequestDetUnderlyer.GetCharacteristics(string pCS, IDbTransaction pDbTransaction)
        {
            EFS_Asset asset = new EFS_Asset
            {
                idAsset = idAsset,
                time = dtPrice,
                quoteTiming = quoteTiming.ToString(),
                assetCategory = assetCategory
            };

            SQL_AssetBase sqlAsset = AssetTools.NewSQLAsset(pCS, assetCategory, idAsset);
            sqlAsset.DbTransaction = pDbTransaction;
            if ((null != sqlAsset) && sqlAsset.IsLoaded)
            {
                asset.clearanceSystem = sqlAsset.ClearanceSystem;
                asset.IdMarket = sqlAsset.IdM;
                asset.IdMarketIdentifier = sqlAsset.Market_Identifier;
                asset.idC = sqlAsset.IdC;
                // EG 20140116 Report v3.7 Refactoring
                switch (assetCategory)
                {
                    case Cst.UnderlyingAsset.Future:
                    case Cst.UnderlyingAsset.ExchangeTradedContract:
                        SQL_AssetETD _unlETD = sqlAsset as SQL_AssetETD;
                        asset.isinCode = _unlETD.ISINCode;
                        asset.isinCode = _unlETD.DrvContract_Category;
                        if (StrFunc.IsFilled(_unlETD.DrvContract_Category))
                            asset.category = (CfiCodeCategoryEnum)ReflectionTools.EnumParse(new CfiCodeCategoryEnum(), _unlETD.DrvContract_Category);
                        asset.assetSymbol = _unlETD.AssetSymbol;
                        asset.contractIdentifier = _unlETD.Identifier;
                        asset.contractDisplayName = _unlETD.DisplayName;
                        asset.contractMultiplier = new EFS_Decimal(_unlETD.ContractMultiplier);
                        asset.contractSymbol = _unlETD.DrvContract_Symbol;
                        asset.maturityDate = _unlETD.Maturity_MaturityDate;
                        asset.maturityDateSys = _unlETD.Maturity_MaturityDateSys;
                        asset.deliveryDate = _unlETD.Maturity_DeliveryDate;
                        asset.nominalValue = _unlETD.DrvContract_NominalValue;
                        asset.lastTradingDay = _unlETD.Maturity_LastTradingDay;
                        // EG 20140325 [19766]
                        asset.instrumentNum = Convert.ToInt32(_unlETD.GetFirstRowColumnValue("INSTRUMENTNUM"));
                        asset.instrumentDen = Convert.ToInt32(_unlETD.GetFirstRowColumnValue("INSTRUMENTDEN"));
                        break;
                    case Cst.UnderlyingAsset.Index:
                        SQL_AssetIndex _unlIndex = sqlAsset as SQL_AssetIndex;
                        asset.isinCode = _unlIndex.ISINCode;
                        asset.assetSymbol = _unlIndex.AssetSymbol;
                        break;
                    case Cst.UnderlyingAsset.EquityAsset:
                        SQL_AssetEquity _unlEquity = sqlAsset as SQL_AssetEquity;
                        asset.isinCode = _unlEquity.ISINCode;
                        asset.assetSymbol = _unlEquity.AssetSymbol;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // special underlyer category cases..
                switch (assetCategory)
                {
                    case Cst.UnderlyingAsset.RateIndex:

                        //PL 20120830 Debug...
                        //SQL_RateIndex sqlAssetAssetRateindex = new SQL_RateIndex(pCS, idAsset);
                        SQL_AssetRateIndex sqlAssetAssetRateindex = new SQL_AssetRateIndex(pCS, SQL_AssetRateIndex.IDType.IDASSET, idAsset)
                        {
                            DbTransaction = pDbTransaction
                        };
                        if ((null != sqlAssetAssetRateindex) && sqlAssetAssetRateindex.IsLoaded)
                        {
                            //PL 20120830 Debug...
                            //asset.idBC = sqlAssetAssetRateindex.BusinessCenter;
                            asset.idBC = sqlAssetAssetRateindex.Idx_BusinessCenter;
                        }
                        break;
                }
            }
            return asset;
        }
        #endregion IPosRequestDetUnderlyer Members
        #region Indexors
        // EG 20151102 [21465] New
        public PosRequestSource this[int pIdPR]
        {
            get
            {
                if (null != posRequestSource)
                {
                    foreach (PosRequestSource item in posRequestSource)
                    {
                        if (item.idPR == pIdPR)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion Indexors

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
    // EG 20171025 [23509] Upd dtExecution replace dtTimestamp 
    public partial class PosRequestDetUpdateEntry : IPosRequestDetUpdateEntry
    {
        #region Constructors
        public PosRequestDetUpdateEntry() { }
        #endregion Constructors
        #region IPosRequestDetUpdateEntry Members
        DateTime IPosRequestDetUpdateEntry.DtBusiness
        {
            set { dtBusiness = value; }
            get { return dtBusiness; }
        }
        int IPosRequestDetUpdateEntry.Side
        {
            set { side = value; }
            get { return side; }
        }
        // EG 20170127 Qty Long To Decimal
        decimal IPosRequestDetUpdateEntry.Qty
        {
            set { qty = value; }
            get { return qty; }
        }
        DateTime IPosRequestDetUpdateEntry.DtExecution
        {
            set { dtExecution = value; }
            get { return dtExecution; }
        }
        string IPosRequestDetUpdateEntry.PositionEffect
        {
            set { positionEffect = value; }
            get { return positionEffect; }
        }
        #endregion IPosRequestDetUpdateEntry Members
    }
    #endregion PosRequestDetUpdateEntry

    #region PosRequestTradeAdditionalInfo
    // EG 20141230 [20587]
    public partial class PosRequestTradeAdditionalInfo : IPosRequestTradeAdditionalInfo
    {
        #region Constructors
        public PosRequestTradeAdditionalInfo() 
        {
            stActivation = Cst.StatusActivation.DEACTIV;
        }
        #endregion Constructors
        #region IPosRequestTradeAdditionalInfo Members
        DataDocumentContainer IPosRequestTradeAdditionalInfo.TemplateDataDocument
        {
            set { templateDataDocument = value; }
            get { return templateDataDocument; }
        }
        string IPosRequestTradeAdditionalInfo.ScreenName
        {
            set { screenName = value; }
            get { return screenName; }
        }
        SQL_Instrument IPosRequestTradeAdditionalInfo.SqlInstrument
        {
            set { sqlInstrument = value; }
            get { return sqlInstrument; }
        }
        SQL_Product IPosRequestTradeAdditionalInfo.SqlProduct
        {
            set { sqlProduct = value; }
            get { return sqlProduct; }
        }
        SQL_TradeCommon IPosRequestTradeAdditionalInfo.SqlTemplateTrade
        {
            set { sqlTemplateTrade = value; }
            get { return sqlTemplateTrade; }
        }
        Cst.StatusActivation IPosRequestTradeAdditionalInfo.StActivation
        {
            set { stActivation = value; }
            get { return stActivation; }
        }
        string IPosRequestTradeAdditionalInfo.DisplayName
        {
            set { displayName = value; }
            get { return displayName; }
        }
        string IPosRequestTradeAdditionalInfo.Description
        {
            set { description = value; }
            get { return description; }
        }
        string IPosRequestTradeAdditionalInfo.ExtlLink
        {
            set { extlLink = value; }
            get { return extlLink; }
        }
        #endregion IPosRequestTradeAdditionalInfo Members
        #region methods
        public static PosRequestTradeAdditionalInfo CreateAdditionalInfo(DataDocumentContainer pTemplateDataDocument,
        SQL_TradeCommon pSQLTrade, SQL_Product pSQLProduct, SQL_Instrument pSQLInstrument, string pScreenName)
        {
            PosRequestTradeAdditionalInfo additionalInfo = new PosRequestTradeAdditionalInfo
            {
                templateDataDocument = pTemplateDataDocument,
                sqlTemplateTrade = pSQLTrade,
                sqlProduct = pSQLProduct,
                sqlInstrument = pSQLInstrument,
                screenName = pScreenName,
                stActivation = Cst.StatusActivation.REGULAR
            };
            return additionalInfo;
        }
        #endregion methods
    }
    #endregion PosRequestTradeAdditionalInfo

    #region PosRequestTradeExAdditionalInfo
    /// <summary>
    /// <para>Complément d'infos pour l'enregistrement du trade Ex ajusté suite à CA</para>
    /// </summary>
    public partial class PosRequestTradeExAdditionalInfo
    {
        #region Constructors
        public PosRequestTradeExAdditionalInfo() { }
        #endregion Constructors
    }
    #endregion PosRequestTradeExAdditionalInfo

    #region PosRequestUnderlyerAdditionalInfo
    /// <summary>
    /// <para>Complément d'infos pour l'enregistrement du trade</para>
    /// </summary>
    public partial class PosRequestUnderlyerAdditionalInfo : IPosRequestTradeAdditionalInfo
    {
        #region Constructors
        public PosRequestUnderlyerAdditionalInfo() { }
        #endregion Constructors
    }
    #endregion PosRequestUnderlyerAdditionalInfo

    /// <summary>
    /// 
    /// </summary>
    public partial class PosRequestPositionDocument : IPosRequestPositionDocument
    {

        #region IEfsDocument Members
        EfsMLDocumentVersionEnum IEfsDocument.EfsMLversion
        {
            get { return this.EfsMLversion; }
            set { this.EfsMLversion = value; }
        }
        bool IRepositoryDocument .RepositorySpecified
        {
            set { ; }
            get { return false; }
        }
        IRepository IRepositoryDocument.Repository
        {
            set { ;}
            get { return null; }
        }
        IRepository IRepositoryDocument.CreateRepository()
        {
            return new EfsML.v30.Repository.Repository();
        }
        /// <summary>
        ///  Retourne une liste vide 
        ///  <para>Methode à compléter si nécessaire</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20150807 [XXXXX] Add 
        List<IAssetRepository> IRepositoryDocument.GetAllRepositoryAsset()
        {
            return new List<IAssetRepository>(); 
        }

        #endregion IEfsDocument Members

        #region IPosRequestDocument Membres

        SettlSessIDEnum IPosRequestPositionDocument.RequestMode
        {
            get
            {
                return this.requestMode;
            }
            set
            {
                this.requestMode = value;
            }
        }

        Cst.PosRequestTypeEnum IPosRequestPositionDocument.RequestType
        {
            get
            {
                return this.requestType;
            }
            set
            {
                this.requestType = value;
            }
        }

        EFS_Date IPosRequestPositionDocument.ClearingBusinessDate
        {
            get
            {
                return this.clearingBusinessDate;
            }
            set
            {
                this.clearingBusinessDate = value;
            }
        }


        IActorId IPosRequestPositionDocument.ActorEntity
        {
            get
            {
                return this.actorEntity;
            }
            set
            {
                this.actorEntity = (ActorId)value;
            }
        }

        bool IPosRequestPositionDocument.ActorDealerSpecified
        {
            get
            {
                return this.actorDealerSpecified;
            }
            set
            {
                this.actorDealerSpecified = value;
            }
        }

        IActorId IPosRequestPositionDocument.ActorDealer
        {
            get
            {
                return this.actorDealer;
            }
            set
            {
                this.actorDealer = (ActorId)value;
            }
        }

        bool IPosRequestPositionDocument.BookDealerSpecified
        {
            get
            {
                return this.bookDealerSpecified;
            }
            set
            {
                this.bookDealerSpecified = value;
            }
        }

        IBookId IPosRequestPositionDocument.BookDealer
        {
            get
            {
                return this.bookDealer;
            }
            set
            {
                this.bookDealer = (BookId)value;
            }
        }

        IActorId IPosRequestPositionDocument.ActorClearer
        {
            get
            {
                return this.actorClearer;
            }
            set
            {
                this.actorClearer = (ActorId)value;
            }
        }

        IBookId IPosRequestPositionDocument.BookClearer
        {
            get
            {
                return this.bookClearer;
            }
            set
            {
                this.bookClearer = (BookId)value;
            }
        }

        FixML.Interface.IFixInstrument IPosRequestPositionDocument.Instrmt
        {
            get
            {
                return this.Instrmt;
            }
            set
            {
                this.Instrmt = (FixML.v50SP1.InstrumentBlock)value;
            }
        }
        // EG 20170127 Qty Long To Decimal
        EFS_Decimal IPosRequestPositionDocument.Qty
        {
            get
            {
                return this.qty;
            }
            set
            {
                this.qty = value;
            }
        }

        bool IPosRequestPositionDocument.NotesSpecified
        {
            get
            {
                return this.notesSpecified;
            }
            set
            {
                this.notesSpecified = value;
            }
        }

        EFS_String IPosRequestPositionDocument.Notes
        {
            get
            {
                return this.notes;
            }
            set
            {
                this.notes = value;
            }
        }

        EFS_Boolean IPosRequestPositionDocument.IsPartialExecutionAllowed
        {
            get
            {
                return this.isPartialExecutionAllowed;
            }
            set
            {
                this.isPartialExecutionAllowed = value;
            }
        }

        EFS_Boolean IPosRequestPositionDocument.IsFeeCalculation
        {
            get
            {
                return this.isFeeCalculation;
            }
            set
            {
                this.isFeeCalculation = value;
            }
        }


        EFS_Boolean IPosRequestPositionDocument.IsAbandonRemainingQty
        {
            get
            {
                return this.isAbandonRemainingQty;
            }
            set
            {
                this.isAbandonRemainingQty = value;
            }
        }
        IPayment[] IPosRequestPositionDocument.PaymentFees
        {
            get
            {
                return this.paymentFees;
            }
            set
            {
                this.paymentFees = (Payment[])value;
            }
        }



        #endregion

        #region cosntructor
        /// <summary>
        /// 
        /// </summary>
        public PosRequestPositionDocument()
        {
            EfsMLversion = EfsMLDocumentVersionEnum.Version30;

            requestType = default;
            requestMode = default;
            clearingBusinessDate = new EFS_Date();
            // EG 20170127 Qty Long To Decimal
            qty = new EFS_Decimal();

            actorEntity = new ActorId();
            actorDealer = new ActorId();
            bookDealer = new BookId();
            actorClearer = new ActorId();
            bookClearer = new BookId();

            Instrmt = new InstrumentBlock();

            notes = new EFS_String();

            paymentFees = new Payment[] { };

            isPartialExecutionAllowed = new EFS_Boolean();
            this.isFeeCalculation = new EFS_Boolean();
            isAbandonRemainingQty = new EFS_Boolean();
        }
        #endregion cosntructor
    }
}

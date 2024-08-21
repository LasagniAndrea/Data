using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.SpheresRiskPerformance.DataContracts;
//
using EfsML.Enum;
using EfsML.Enum.Tools;
//
using FixML.Enum;
using FixML.v50SP1.Enum;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Data container for the Asset ETD expanded information
    /// </summary>
    /// <remarks>Main Spheres reference table: ASSET_ETD, DERIVATIVEATTRIB, DERIVATIVECONTRACT, MATURITY</remarks>
    [DataContract(Name = DataHelper<AssetExpandedParameter>.DATASETROWNAME,
        Namespace = DataHelper<AssetExpandedParameter>.DATASETNAMESPACE)]
    public sealed class AssetExpandedParameter
    {
        /// <summary>
        /// Derivative Contract internal id
        /// </summary>
        [DataMember(Name = "IDDC", Order = 1)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Derivative Contract Symbol
        /// </summary>
        [DataMember(Name = "CONTRACTSYMBOL", Order = 2)]
        public string ContractSymbol
        { get; set; }

        /// <summary>
        /// Category od the current derivative contract (IDDC), F for Futures, O for Options.
        /// </summary>
        [DataMember(Name = "CATEGORY", Order = 3)]
        public string Category
        { get; set; }

        public Nullable<CfiCodeCategoryEnum> CategoryEnum
        {
            get
            {
                Nullable<CfiCodeCategoryEnum> retCategeory = null;
                if (StrFunc.IsFilled(Category))
                    retCategeory = (CfiCodeCategoryEnum)ReflectionTools.EnumParse(new CfiCodeCategoryEnum(), Category);
                return retCategeory;
            }
        }

        /// <summary>
        /// Contract currency
        /// </summary>
        [DataMember(Name = "IDC_PRICE", Order = 4)]
        public string Currency
        { get; set; }

        /// <summary>
        /// Valuation Method
        /// </summary>
        [DataMember(Name = "FUTVALUATIONMETHOD", Order = 5)]
        public string ValuationMethod
        { get; set; }

        public FuturesValuationMethodEnum ValuationMethodEnum
        {
            get
            {
                //object ret = StringToEnum.GetEnumValue(new FuturesValuationMethodEnum(), ValuationMethod);
                //return (ret != null ? (FuturesValuationMethodEnum)ret : FuturesValuationMethodEnum.FuturesStyleMarkToMarket);
                return ReflectionTools.ConvertStringToEnumOrDefault<FuturesValuationMethodEnum>(ValuationMethod, FuturesValuationMethodEnum.FuturesStyleMarkToMarket);
            }
        }

        /// <summary>
        /// Contract price numerator
        /// </summary>
        [DataMember(Name = "INSTRUMENTNUM", Order = 6)]
        public double InstrumentNumDec
        { get; set; }
        public int InstrumentNum
        {
            get
            {
                return (int)InstrumentNumDec;
            }
        }

        /// <summary>
        /// Contract price denominator
        /// </summary>
        [DataMember(Name = "INSTRUMENTDEN", Order = 7)]
        public int InstrumentDen
        { get; set; }

        /// <summary>
        /// Contract price denominator
        /// </summary>
        /// PM 20171212 [23646] Add PriceDecimalLocator
        [DataMember(Name = "PRICEDECLOCATOR", Order = 8)]
        public int PriceDecimalLocator
        { get; set; }

        /// <summary>
        /// Underlying Asset Category
        /// </summary>
        [DataMember(Name = "ASSETCATEGORY", Order = 9)]
        public string UnderlyningAssetCategory
        { get; set; }

        public Cst.UnderlyingAsset UnderlyingAssetCategoryEnum
        {
            get
            {
                if (false == System.Enum.TryParse<Cst.UnderlyingAsset>(UnderlyningAssetCategory, out Cst.UnderlyingAsset unlAssetEnum))
                {
                    // Si la catégorie d'asset sous-jacent n'est pas renseignée on prend Future par défaut
                    unlAssetEnum = Cst.UnderlyingAsset.Future;
                }
                return unlAssetEnum;
            }
        }

        /// <summary>
        /// Market internal id
        /// </summary>
        [DataMember(Name = "IDM", Order = 10)]
        public int MarketId
        { get; set; }

        /// <summary>
        /// Underlying Contract internal id
        /// </summary>
        [DataMember(Name = "IDDC_UNL", Order = 11)]
        public int UnderlyningContractId
        { get; set; }

        /// <summary>
        /// Underlying Asset internal id
        /// </summary>
        [DataMember(Name = "IDASSET_UNL", Order = 12)]
        public int UnderlyningAssetId
        { get; set; }

        /// <summary>
        /// Asset internal id
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 13)]
        public int AssetId
        { get; set; }

        /// <summary>
        /// Put / Call indicator
        /// </summary>
        [DataMember(Name = "PUTCALL", Order = 14)]
        public string PutCall
        { get; set; }

        public Nullable<PutOrCallEnum> PutOrCall
        {
            get
            {
                Nullable<PutOrCallEnum> retPutOrCall = null;
                if (StrFunc.IsFilled(PutCall))
                    retPutOrCall = (PutOrCallEnum)ReflectionTools.EnumParse(new PutOrCallEnum(), PutCall);
                return retPutOrCall;
            }
        }

        /// <summary>
        /// Strike Price
        /// </summary>
        [DataMember(Name = "STRIKEPRICE", Order = 15)]
        public double StrikePriceDouble
        { get; set; }
        public decimal StrikePrice
        { get { return (decimal)StrikePriceDouble; } }

        /// <summary>
        /// Maturity Code
        /// </summary>
        [DataMember(Name = "MATURITYMONTHYEAR", Order = 16)]
        public string MaturityYearMonth
        { get; set; }

        /// <summary>
        /// Maturity Date
        /// </summary>
        [DataMember(Name = "MATURITYDATE", Order = 17)]
        public DateTime MaturityDate
        { get; set; }

        /// <summary>
        /// Maturity Date System
        /// </summary>
        [DataMember(Name = "MATURITYDATESYS", Order = 18)]
        public DateTime MaturityDateSys
        { get; set; }

        /// <summary>
        /// Contract multiplier
        /// </summary>
        [DataMember(Name = "CONTRACTMULTIPLIER", Order = 19)]
        public double ContractMultiplierDouble
        { get; set; }
        public decimal ContractMultiplier
        { get { return (decimal)ContractMultiplierDouble; } }

        /// <summary>
        /// Round direction
        /// </summary>
        /// PM 20150707 [21104] Add RoundDir
        [DataMember(Name = "ROUNDDIR", Order = 20)]
        public string RoundDir
        { get; set; }

        /// <summary>
        /// Round precision
        /// </summary>
        /// PM 20150707 [21104] Add RoundPrec
        [DataMember(Name = "ROUNDPREC", Order = 21)]
        public int RoundPrec
        { get; set; }

        /// <summary>
        /// Cash Flow Calculation Method
        /// </summary>
        /// PM 20150707 [21104] Add CashFlowCalcMethod
        [DataMember(Name = "CASHFLOWCALCMETHOD", Order = 22)]
        public string CashFlowCalcMethod
        { get; set; }

        /// <summary>
        /// Cash Flow Calculation Method Enum
        /// </summary>
        /// PM 20150707 [21104] Add CashFlowCalcMethodEnum
        public CashFlowCalculationMethodEnum CashFlowCalcMethodEnum
        {
            get
            {
                CashFlowCalculationMethodEnum method = CashFlowCalculationMethodEnum.OVERALL;
                if (StrFunc.IsFilled(CashFlowCalcMethod))
                {
                    method = (CashFlowCalculationMethodEnum)StringToEnum.Parse(CashFlowCalcMethod, method);
                }
                return method;
            }
        }

        /// <summary>
        /// Cash Flow Calculation Method
        /// </summary>
        // PM 20220111 [25617] Ajout MarketAcronym
        [DataMember(Name = "ACRONYM", Order = 23)]
        public string MarketAcronym
        { get; set; }
        /// <summary>
        /// Exchange Acronym (use with SPAN2 méthod with JSONRequest)
        /// </summary>
        // EG 20230811 [26454] Ajout MarketExchangeAcronym
        [DataMember(Name = "EXCHANGEACRONYM", Order = 24)]
        public string MarketExchangeAcronym
        { get; set; }

    }

    /// <summary>
    /// Data container for the Asset expanded information (not only ETD). For now Contains ISINCODE only
    /// </summary>
    [DataContract(Name = DataHelper<AssetExpandedParameter>.DATASETROWNAME,
        Namespace = DataHelper<AssetExpandedParameter>.DATASETNAMESPACE)]
    public sealed class AssetAllExpandedParameter
    {

        /// <summary>
        /// internal id
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int AssetId
        { get; set; }

        /// <summary>
        ///  Asset Category
        /// </summary>
        [DataMember(Name = "ASSETCATEGORY", Order = 2)]
        public string AssetCategory
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Cst.UnderlyingAsset AssetCategoryEnum
        {
            get
            {
                return (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), AssetCategory);
            }
        }

        /// <summary>
        ///  Asset IsinCode
        /// </summary>
        [DataMember(Name = "ISINCODE", Order = 3)]
        public string IsinCode
        { get; set; }
    }

    /// <summary>
    /// Class de chargement des parametres des paniers d'equity en vue de leur utilisation pour reduire une position ETD
    /// </summary>
    // PM 20201028 [25570][25542] Nouvelle class
    [DataContract(
        Name = DataHelper<StocksCoverageParameter>.DATASETROWNAME,
        Namespace = DataHelper<StocksCoverageParameter>.DATASETNAMESPACE)]
    public class EquityBasketSetting
    {
        /// <summary>
        /// Id de l'asset panier
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int BasketIdAsset
        { get; set; }

        /// <summary>
        /// Identifiant de l'asset panier
        /// </summary>
        [DataMember(Name = "IDENTIFIER", Order = 2)]
        public string BasketIdentifier
        { get; set; }

        /// <summary>
        /// Type de poids des constituants du panier d'equity
        /// </summary>
        [DataMember(Name = "UNITTYPEWEIGHT", Order = 3)]
        public BasketUnitTypeWeightEnum BasketUnitTypeWeight
        { get; set; }

        /// <summary>
        /// Id de l'asset equity constituant du panier
        /// </summary>
        [DataMember(Name = "IDASSETREF", Order = 4)]
        public int EquityIdAsset
        { get; set; }

        /// <summary>
        /// Poids l'asset equity constituant du panier afin de pouvoir être utilisé en couverture
        /// </summary>
        [DataMember(Name = "WEIGHT", Order = 5)]
        public decimal EquityWeight
        { get; set; }
    }

    /// <summary>
    /// Paramètres concernant une méthode de calcul de déposit
    /// <remarks>table: IMMETHOD</remarks>
    /// </summary>
    [DataContract(Name = DataHelper<ImMethodParameter>.DATASETROWNAME,
        Namespace = DataHelper<ImMethodParameter>.DATASETNAMESPACE)]
    public sealed class ImMethodParameter
    {
        /// <summary>
        /// Id interne des paramètres de la méthode de calcul
        /// </summary>
        [DataMember(Name = "IDIMMETHOD", Order = 1)]
        public int IdIMMethod
        { get; set; }

        /// <summary>
        /// Identifiant des paramètres de la méthode de calcul
        /// </summary>
        // PM 20231019 [XXXXX] Ajout IDENTIFIER
        [DataMember(Name = "IDENTIFIER", Order = 2)]
        public string Identifier
        { get; set; }

        /// <summary>
        /// Type de la méthode de calcul du déposit
        /// </summary>
        [DataMember(Name = "INITIALMARGINMETH", Order = 3)]
        //public string IMMethod
        //{ get; set; }
        public InitialMarginMethodEnum IMMethodEnum;

        /// <summary>
        /// Methode de calcul du risque pondéré de prix
        /// </summary>
        [DataMember(Name = "IMWEIGHTEDRISKMETH", Order = 4)]
        //public string WeightedRiskMethod
        //{
        //    get;
        //    set
        //    {
        //        WeightedRiskMethodEnum = MarginWeightedRiskCalculationMethodEnum.Normal;
        //        if (StrFunc.IsFilled(value))
        //            WeightedRiskMethodEnum = (MarginWeightedRiskCalculationMethodEnum)ReflectionTools.EnumParse(new MarginWeightedRiskCalculationMethodEnum(), value);
        //    }
        //}
        public MarginWeightedRiskCalculationMethodEnum WeightedRiskMethodEnum;

        /// <summary>
        /// Direction de l'arrondie dans le calcul
        /// </summary>
        [DataMember(Name = "IMROUNDINGDIR", Order = 5)]
        public string RoundingDirection
        { get; set; }

        /// <summary>
        /// Precision de l'arrondie dans le calcul
        /// </summary>
        [DataMember(Name = "IMROUNDINGPREC", Order = 6)]
        public int RoundingPrecision
        { get; set; }

        /// <summary>
        /// Gestion des spreads inter group de contrat
        /// </summary>
        [DataMember(Name = "ISIMINTERCOMSPRD", Order = 7)]
        public bool IsInterCommoditySpreading
        { get; set; }

        #region Paramètres spécifiques à la méthode IMSM
        // PM 20170313 [22833] Ajout des paramètres spécifiques à la méthode IMSM
        /// <summary>
        /// Indicateur de gestion des jours fériés dans la méthode IMSM
        /// </summary>
        [DataMember(Name = "ISWITHHOLIDAYADJ", Order = 8)]
        public bool IsWithHolidayAdjustment
        { get; set; }

        /// <summary>
        /// Multiplicateur pour l'écart-type utilisé dans la partie statistique du calcul de l'IMSM
        /// </summary>
        [DataMember(Name = "ALPHAFACTOR", Order = 9)]
        public double AlphaFactorDouble
        { get; set; }
        public decimal AlphaFactor
        { get { return (decimal)AlphaFactorDouble; } set { AlphaFactorDouble = (double)value; } }

        /// <summary>
        /// Multiplicateur pour la partie déterministe du calcul de l'IMSM
        /// </summary>
        [DataMember(Name = "BETAFACTOR", Order = 10)]
        public double BetaFactorDouble
        { get; set; }
        public decimal BetaFactor
        { get { return (decimal)BetaFactorDouble; } set { BetaFactorDouble = (double)value; } }

        /// <summary>
        /// Paramètre EWMA (Exponentially Weighted Moving Average / Moyennes Mobiles Pondérées Expontiellement) pour calculer la volatilité dans le calcul de l'écart-type du calcul de l'IMSM
        /// </summary>
        [DataMember(Name = "EWMAFACTOR", Order = 11)]
        public double EwmaFactorDouble
        { get; set; }
        public decimal EwmaFactor
        { get { return (decimal)EwmaFactorDouble; } set { EwmaFactorDouble = (double)value; } }

        /// <summary>
        /// Taille de la fenêtre de la partie maximum du calcul de l'IMSM
        /// </summary>
        [DataMember(Name = "WINDOWSIZEFORMAX", Order = 12)]
        public int WindowSizeForMax
        { get; set; }

        /// <summary>
        /// Taille de la fenêtre de prise en compte des données statistiques du calcul de l'IMSM
        /// </summary>
        [DataMember(Name = "STATWINDOWSIZE", Order = 13)]
        public int WindowSizeStatistic
        { get; set; }

        /// <summary>
        /// Minimum absolu de l'IMSM
        /// </summary>
        [DataMember(Name = "MINIMUMAMOUNT", Order = 14)]
        public double MinimumAmountDouble
        { get; set; }
        public decimal MinimumAmount
        { get { return (decimal)MinimumAmountDouble; } set { MinimumAmountDouble = (double)value; } }

        /// <summary>
        /// Minimum absolu initial de l'IMSM pour les "MinimumAmountFirstTerm"(30) premiers jours après l'admission
        /// </summary>
        [DataMember(Name = "MINIMUMAMOUNTFIRST", Order = 15)]
        public double MinimumAmountFirstDouble
        { get; set; }
        public decimal MinimumAmountFirst
        { get { return (decimal)MinimumAmountFirstDouble; } set { MinimumAmountFirstDouble = (double)value; } }

        /// <summary>
        /// Nombre de jours pendant lesquels appliquer le montant minimum absolu initial de l'IMSM
        /// </summary>
        [DataMember(Name = "MINAMTFIRSTTERM", Order = 16)]
        public int MinimumAmountFirstTerm
        { get; set; }
        #endregion Paramètres spécifique à la méthode IMSM

        /// <summary>
        /// Version de la méthode
        /// </summary>
        // PM 20180316 [23840] Ajout
        [DataMember(Name = "METHODVERSION", Order = 17)]
        public double MethodVersionDouble
        { get; set; }
        public decimal MethodVersion
        { get { return (decimal)MethodVersionDouble; } set { MethodVersionDouble = (double)value; } }

        /// <summary>
        /// Gestion du calcul du Concentration Risk Margin de l'ECC
        /// </summary>
        // PM 20190801 [24717] Ajout
        [DataMember(Name = "ISCALCECCCONR", Order = 18)]
        public bool IsCalcECCConcentrationRiskMargin
        { get; set; }

        /// <summary>
        /// Additionnal Add-On in days pour le calcul du Concentration Risk Margin de l'ECC 
        /// </summary>
        // PM 20190801 [24717] Ajout
        [DataMember(Name = "ECCCONRADDON", Order = 19)]
        public double ECCAddOnDaysDouble
        { get; set; }
        public decimal ECCAddOnDays
        { get { return (decimal)ECCAddOnDaysDouble; } set { ECCAddOnDaysDouble = (double)value; } }

        /// <summary>
        /// Calcul uniquement du Current Exposure Spot Margin de l'ECC
        /// </summary>
        // PM 20200910 [25482] Ajout IsCalcCESMOnly
        [DataMember(Name = "ISCESMONLY", Order = 20)]
        public bool IsCalcCESMOnly
        { get; set; }

        #region Paramètres spécifiques à la méthode SPAN 2
        /// <summary>
        /// Base URL pour le calcul CME SPAN 2
        /// </summary>
        // PM 20220111 [25617] Ajout BaseUrl
        [DataMember(Name = "BASEURL", Order = 21)]
        public string BaseUrl
        { get; set; }

        /// <summary>
        /// Id du User URL pour le calcul CME SPAN 2
        /// </summary>
        // PM 20220111 [25617] Ajout UserId
        [DataMember(Name = "USERID", Order = 22)]
        public string UserId
        { get; set; }

        /// <summary>
        /// Password du user pour le calcul CME SPAN 2
        /// </summary>
        // PM 20220111 [25617] Ajout Pwd
        [DataMember(Name = "PWD", Order = 23)]
        public string Pwd
        { get; set; }

        /// <summary>
        /// Namespace de CME Core API CME SPAN 2
        /// </summary>
        // PM 20220111 [25617] Ajout du Namespace de CME Core API
        [DataMember(Name = "CMECORESCHEME", Order = 24)]
        public string CMECoreNamespace
        { get; set; }

        /// <summary>
        /// Type de devise pour le résultat du calcul de déposit (Devise de contrat ou devise de contrevaleur de la chambre)
        /// </summary>
        // PM 20231030 [26547][WI735] Ajout CURRENCYTYPE (MarginCurrencyType & MarginCurrencyTypeEnum)
        [DataMember(Name = "CURRENCYTYPE", Order = 25)]
        public string MarginCurrencyType
        { get; set; }

        /// <summary>
        /// Type de devise pour le résultat du calcul de déposit (Devise de contrat ou devise de contrevaleur de la chambre)
        /// </summary>
        public Cst.InitialMarginCurrencyTypeEnum MarginCurrencyTypeEnum
        {
            get
            {
                return ReflectionTools.ConvertStringToEnumOrDefault<Cst.InitialMarginCurrencyTypeEnum>(MarginCurrencyType, Cst.InitialMarginCurrencyTypeEnum.ContractCurrency);
            }
        }

        /// <summary>
        /// Indicateur d'essai d'écarter les positions sur des assets erronés
        /// </summary>
        // PM 20230209 [XXXXX] Ajout
        [DataMember(Name = "ISEXCLUDEWRONGPOS", Order = 26)]
        public bool IsTryExcludeWrongPosition
        { get; set; }
        #endregion Paramètres spécifiques à la méthode SPAN 2

        /// <summary>
        /// Maintenance or Initial amount
        /// </summary>
        // PM 20230322 [26282][WI607] Ajout
        [DataMember(Name = "ISIMMAINTENANCE", Order = 27)]
        public bool IsMaintenance
        { get; set; }

        /// <summary>
        /// Number of months until expiration to compute margin for long call
        /// </summary>
        // PM 20230322 [26282][WI607] Ajout
        [DataMember(Name = "NBMONTHLONGCALL", Order = 28)]
        public int NbMonthLongCall
        { get; set; }

        /// <summary>
        /// Number of months until expiration to compute margin for long put
        /// </summary>
        // PM 20230322 [26282][WI607] Ajout
        [DataMember(Name = "NBMONTHLONGPUT", Order = 29)]
        public int NbMonthLongPut
        { get; set; }
    }

    /// <summary>
    /// Class representing a position action 
    /// </summary>
    /// <remarks>Inherit from TradeAllocation</remarks>
    [DataContract(
        Name = DataHelper<PositionAction>.DATASETROWNAME,
        Namespace = DataHelper<PositionAction>.DATASETNAMESPACE)]
    public class PositionAction : TradeAllocation
    {
        /// <summary>
        /// ?
        /// </summary>
        [DataMember(Name = "POSITIONTYPE", Order = 14)]
        public string PosType
        { get; set; }

        /// <summary>
        /// DVS factor
        /// </summary>
        [DataMember(Name = "LONGPOSITIONCTRVAL", Order = 15)]
        public double LongCrtValDouble
        { get; set; }
        public decimal LongCrtVal
        { get { return (decimal)LongCrtValDouble; } }

        /// <summary>
        /// DVS Factor
        /// </summary>
        [DataMember(Name = "SHORTPOSITIONCTRVAL", Order = 16)]
        public double ShortCrtValDouble
        { get; set; }
        public decimal ShortCrtVal
        { get { return (decimal)ShortCrtValDouble; } }

        /// <summary>
        /// Currency of the position
        /// </summary>
        [DataMember(Name = "CURRENCY", Order = 17)]
        public string Currency
        { get; set; }

        /// <summary>
        /// Maturity
        /// </summary>
        [DataMember(Name = "MATURITYDATE", Order = 18)]
        public DateTime MaturityDate
        { get; set; }
    }

    /// <summary>
    /// Class representing a stock of actions in order to cover short (calls and futures) positions
    /// </summary>
    /// FI 20160613 [22256] Modify 
    // PM 20201028 [25570][25542] Ajout de membres
    [DataContract(
        Name = DataHelper<StocksCoverageParameter>.DATASETROWNAME,
        Namespace = DataHelper<StocksCoverageParameter>.DATASETNAMESPACE)]
    public class StocksCoverageParameter
    {
        /// <summary>
        /// Derivative Contract internal id
        /// </summary>
        [DataMember(Name = "IDDC", Order = 1)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Derivative contract underlying (stock) asset internal id
        /// </summary>
        [DataMember(Name = "IDASSET_UNL", Order = 2)]
        public int UnlAssetId
        { get; set; }

        /// <summary>
        /// Derivative contract underlying (stock) asset internal identifier
        /// </summary>
        /// FI 20160613 [22256] Add
        [DataMember(Name = "ASSET_UNL_IDENTIFIER", Order = 3)]
        public String UnlAssetIdentifier
        { get; set; }

        /// <summary>
        /// PosEquity Id (POSEQUSECURITY.IDPOSEQUSECURITY)
        /// </summary>
        /// FI 20160613 [22256] Add
        [DataMember(Name = "IDPOSEQUSECURITY", Order = 4)]
        public int IdPosEquity
        { get; set; }

        /// <summary>
        /// PosEquity Id (POSEQUSECURITY.IDENTIFIER)
        /// </summary>
        /// FI 20160613 [22256] Add
        [DataMember(Name = "POSEQUSECURITY_IDENTIFIER", Order = 5)]
        public string PosEquityIdentifier
        { get; set; }

        /// <summary>
        /// Id of the depositor
        /// </summary>
        [DataMember(Name = "IDA_PAY", Order = 6)]
        public int PayActorId
        { get; set; }

        /// <summary>
        /// Identifier of the depositor
        /// </summary>
        [DataMember(Name = "PAYER_ACTOR_IDENTIFIER", Order = 7)]
        public string PayActorIdentifier
        { get; set; }

        /// <summary>
        /// Book id of the depositor
        /// </summary>
        [DataMember(Name = "IDB_PAY", Order = 8)]
        public int PayBookId
        { get; set; }

        /// <summary>
        /// Book identifier of the depositor
        /// </summary>
        [DataMember(Name = "PAYER_BOOK_IDENTIFIER", Order = 9)]
        public string PayBookIdentifier
        { get; set; }

        /// <summary>
        /// Id of the depositary
        /// </summary>
        [DataMember(Name = "IDA_REC", Order = 10)]
        public int RecActorId
        { get; set; }

        /// <summary>
        /// Identifier of the depositary
        /// </summary>
        [DataMember(Name = "RECEIVER_ACTOR_IDENTIFIER", Order = 11)]
        public string RecActorIdentifier
        { get; set; }

        /// <summary>
        /// Book id of the depositary
        /// </summary>
        [DataMember(Name = "IDB_REC", Order = 12)]
        public int RecBookId
        { get; set; }

        /// <summary>
        /// Book identifier of the depositary
        /// </summary>
        [DataMember(Name = "RECEIVER_BOOK_IDENTIFIER", Order = 13)]
        public string RecBookIdentifier
        { get; set; }

        /// <summary>
        /// Number of the stocks
        /// </summary>
        [DataMember(Name = "QTY", Order = 14)]
        /// EG 20150920 [21374] Int (int32) to Long (Int64) 
        /// // EG 20170127 Qty Long To Decimal
        public decimal Quantity
        { get; set; }

        /// <summary>
        /// Coverage type/priority for the loaded stocks
        /// </summary>
        [DataMember(Name = "POSSTOCKCOVER", Order = 15)]
        public PosStockCoverEnum Type
        { get; set; }

        /// <summary>
        /// When different than 0 the coverage quantity is specific to the contract inside of GroupByContractId 
        /// (when > 0 the value of GroupByContractId has the same value than ContractId);
        /// when 0 the coverage quantity has to be shared with all the other contracts having the same underlyer value (UnderlyerAssetId).
        /// </summary>
        [DataMember(Name = "GROUPBYIDDC", Order = 16)]
        public int GroupByContractId
        { get; set; }

        /// <summary>
        /// market internal id of the current derivative contract (ContractId)
        /// </summary>
        [DataMember(Name = "IDM", Order = 17)]
        public int MarketId
        { get; set; }

        /// <summary>
        /// IdAsset de l'equity en position
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 18)]
        public int EquityAssetId
        { get; set; }

        /// <summary>
        /// Equity ou Basket de l'equity en position
        /// </summary>
        // PM 20201028 [25570][25542] Ajout AssetClass & AssetClassEnum
        [DataMember(Name = "ASSETCLASS", Order = 19)]
        public string EquityAssetClass
        { get; set; }

        public Nullable<UnderlyingAssetEnum> EquityAssetClassEnum
        {
            get
            {
                return ReflectionTools.ConvertStringToEnumOrNullable<UnderlyingAssetEnum>(EquityAssetClass); ;
            }
        }

        /// <summary>
        /// Equity ou Basket de l'asset sous-jacent du derivative contract
        /// </summary>
        // PM 20201028 [25570][25542] Ajout AssetClass & AssetClassEnum
        [DataMember(Name = "ASSETCLASS_UNL", Order = 20)]
        public string UnlAssetClass
        { get; set; }
        public Nullable<UnderlyingAssetEnum> UnlAssetClassEnum
        {
            get
            {
                return ReflectionTools.ConvertStringToEnumOrNullable<UnderlyingAssetEnum>(UnlAssetClass); ;
            }
        }

        /// <summary>
        /// Type de poids des constituants du panier d'equity
        /// </summary>
        // PM 20201028 [25570][25542] Ajout BasketUnitTypeWeight
        [DataMember(Name = "UNITTYPEWEIGHT", Order = 21)]
        public BasketUnitTypeWeightEnum BasketUnitTypeWeight
        { get; set; }

        /// <summary>
        /// Poids requis de l'asset en position afin de pouvoir être utilisé en couverture
        /// </summary>
        // PM 20201028 [25570][25542] Ajout EquityWeight
        [DataMember(Name = "WEIGHT", Order = 22)]
        public decimal EquityWeight
        { get; set; }

        /// <summary>
        /// Ientifier de l'asset Equity ou du basket en position
        /// </summary>
        // PM 20201028 [25570][25542] Ajout AssetIdentifier
        [DataMember(Name = "ASSET_IDENTIFIER", Order = 23)]
        public String AssetIdentifier
        { get; set; }

        /// <summary>
        /// Lorsque qu'il s'agit d'un stock utilisé pour la constuction d'un basket, donne la quantité initial du stock
        /// </summary>
        public decimal InitialQuantity
        { get; set; }

        /// <summary>
        /// Lorsque qu'il s'agit d'un basket construit, donne la liste de stocks constituant ce basket
        /// </summary>
        // PM 20201028 [25570][25542] Ajout AssetIdentifier
        public List<StocksCoverageParameter> StocksConstituant
        { get; set; }
    }
}

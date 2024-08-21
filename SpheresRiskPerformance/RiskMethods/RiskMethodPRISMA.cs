using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.Properties;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.v30.Fix;
//
using FixML.Enum;
//
using FpML.Enum;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class de calcul du Time to Expiry Adjustment
    /// </summary>
    // PM 20180903 [24015] Prisma v8.0 : new class PrismaTEACalc
    internal sealed class PrismaTEAExpiryHorizonCalc
    {
        #region members
        private int m_ExpiryHorizon;
        private decimal m_HedgingWeight;
        private readonly IEnumerable<Money> m_MarketRisk;
        private List<Money> m_MRIMEH;
        private List<Money> m_mMRIMEH;
        private List<Money> m_ScaledmMRIMEH;
        #endregion members

        #region accessors
        /// <summary>
        /// Expiry Horizon
        /// </summary>
        public int ExpiryHorizon
        {
            set { m_ExpiryHorizon = value; }
            get { return m_ExpiryHorizon; }
        }
        /// <summary>
        /// Hedging Weight
        /// </summary>
        public decimal HedgingWeight
        {
            set { m_HedgingWeight = value; }
            get { return m_HedgingWeight; }
        }
        /// <summary>
        /// Market Risk
        /// </summary>
        public List<Money> MarketRiskExpiryHorizon
        {
            set { m_MRIMEH = value; }
            get { return m_MRIMEH; }
        }
        /// <summary>
        /// Marginal Market Risk
        /// </summary>
        public List<Money> MarginalMarketRiskExpiryHorizon
        {
            get { return m_mMRIMEH; }
        }
        /// <summary>
        /// Scaled Marginal Market Risk
        /// </summary>
        public IEnumerable<Money> ScaledMarginalMarketRisk
        {
            get { return m_ScaledmMRIMEH; }
        }
        #endregion accessors

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExpiryHorizon"></param>
        /// <param name="pHedgingWeight"></param>
        /// <param name="pMarketRisk"></param>
        public PrismaTEAExpiryHorizonCalc(int pExpiryHorizon, decimal pHedgingWeight, IEnumerable<Money> pMarketRisk)
        {
            m_ExpiryHorizon = pExpiryHorizon;
            m_HedgingWeight = pHedgingWeight;
            m_MarketRisk = pMarketRisk;
        }
        #endregion constructors

        #region methods
        /// <summary>
        /// Calcul du marginal market risk initial margin et application du hedging weights
        /// </summary>
        /// <returns></returns>
        public List<Money> CalcScaledMarginalMarketRisk()
        {
            m_mMRIMEH = new List<Money>();
            m_ScaledmMRIMEH = new List<Money>();
            //
            if ((m_MarketRisk != default) && (m_MarketRisk.Count() > 0)
                && (m_MRIMEH != default) && (m_MRIMEH.Count > 0))
            {
                foreach (Money mrim in m_MarketRisk)
                {
                    string currency = mrim.Currency;
                    Money mrimEH = m_MRIMEH.FirstOrDefault(m => m.Currency == currency);
                    if (mrimEH != default)
                    {
                        decimal mMRIM = mrim.Amount.DecValue - mrimEH.Amount.DecValue;
                        decimal mMRIMScaled = mMRIM * m_HedgingWeight;
                        Money mMRIMEH = new Money(mMRIM, currency);
                        Money mMRIMEHScaled = new Money(mMRIMScaled, currency);
                        m_mMRIMEH.Add(mMRIMEH);
                        m_ScaledmMRIMEH.Add(mMRIMEHScaled);
                    }
                }
            }
            return m_ScaledmMRIMEH;
        }
        #endregion methods
    }

    /// <summary>
    /// Class de calcul du Time to Expiry Adjustment
    /// </summary>
    // PM 20180903 [24015] Prisma v8.0 : new class PrismaTimeToExpiryAdjustmentRisk
    internal sealed class PrismaTimeToExpiryAdjustmentRisk
    {
        #region members
        #region members Parameters
        private readonly IEnumerable<PrismaTimeToExpiryAdjustment> m_TEAParameters;
        private readonly PrismaAggregationMethod m_GroupSplitAggregationMethod;
        private readonly IEnumerable<PrismaPositionRisk> m_PositionRisk;
        private readonly IEnumerable<Money> m_MarketRisk = default;
        #endregion members Parameters
        #region members Calculation & Result
        private readonly List<PrismaTEAExpiryHorizonCalc> m_TEAElements;
        private List<Money> m_TimeToExpiryAdjustment;
        #endregion members Calculation & Result
        #endregion members

        #region accessors
        /// <summary>
        /// Time To Expiry Adjustment
        /// </summary>
        public List<Money> TimeToExpiryAdjustment { get { return m_TimeToExpiryAdjustment; } }
        #endregion accessors

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTEAParameters"></param>
        /// <param name="pGroupSplitAggregationMethod"></param>
        /// <param name="pPositionRisk"></param>
        /// <param name="pMarketRisk"></param>
        public PrismaTimeToExpiryAdjustmentRisk(IEnumerable<PrismaTimeToExpiryAdjustment> pTEAParameters, PrismaAggregationMethod pGroupSplitAggregationMethod,
            IEnumerable<PrismaPositionRisk> pPositionRisk, IEnumerable<Money> pMarketRisk)
        {
            m_TEAParameters = pTEAParameters;
            m_GroupSplitAggregationMethod = pGroupSplitAggregationMethod;
            m_PositionRisk = pPositionRisk;
            m_MarketRisk = pMarketRisk;
            m_TEAElements = new List<PrismaTEAExpiryHorizonCalc>();
        }
        #endregion constructors

        #region methods
        /// <summary>
        /// Calcul du Time to Expiry Adjustment
        /// </summary>
        /// <returns></returns>
        public List<Money> CalcTimeToExpiryAdjustment()
        {
            if ((m_TEAParameters != default) && (m_TEAParameters.Count() > 0)
                && (m_GroupSplitAggregationMethod != default)
                && (m_PositionRisk != default(IEnumerable<PrismaPositionRisk>)) && (m_PositionRisk.Count() > 0)
                && (m_MarketRisk != default(IEnumerable<Money>)) && (m_MarketRisk.Count() > 0))
            {
                // Non utilisé, uniquement pour appel à la méthode SubEvaluateMarketRisk
                // Pour chaque Expiry Horizon
                foreach (PrismaTimeToExpiryAdjustment tea in m_TEAParameters)
                {
                    PrismaTEAExpiryHorizonCalc teaCalc = new PrismaTEAExpiryHorizonCalc(tea.TeaBucketId, tea.HedgingWeight, m_MarketRisk);
                    //
                    // Prendre la position pour les assets qui ne sont pas en Cash Settlement ou dont l'échéance est supérieur à l'expiry horizon
                    IEnumerable<PrismaPositionRisk> position = m_PositionRisk.Where(p => (p.AssetRisk.AssetParameters.SettlementMethod != SettlMethodEnum.CashSettlement) || p.AssetRisk.AssetParameters.DaysToExpiryBusiness > teaCalc.ExpiryHorizon);
                    //
                    // Calcul du market risk pour l'expiry horizon
                    teaCalc.MarketRiskExpiryHorizon = PrismaPositionLgsRisk.SubEvaluateMarketRisk(position, m_GroupSplitAggregationMethod, 
                        out Dictionary<string, List<PrismaRmsSubSamplesRisk>> rmsRiskCur);
                    //
                    // Calcul du marginal market risk initial margin et application du hedging weights
                    teaCalc.CalcScaledMarginalMarketRisk();
                    //
                    m_TEAElements.Add(teaCalc);
                }
                if (m_TEAElements.Count > 0)
                {
                    // Prendre les Scaled Marginal Market Risk négatifs
                    IEnumerable<Money> teaToSum = from teaCalc in m_TEAElements
                                                  from scaledCur in teaCalc.ScaledMarginalMarketRisk
                                                  where scaledCur.Amount.DecValue < 0
                                                  select scaledCur;
                    //
                    // Calcul final du Time To Expiry Adjustment (Somme des Scaled Marginal Market Risk et inversion de signe)
                    m_TimeToExpiryAdjustment = (
                        from tea in teaToSum
                        group tea by tea.Currency into teaByCurrency
                        select new Money(-1 * teaByCurrency.Sum(m => m.Amount.DecValue), teaByCurrency.Key)
                        ).ToList();
                }
            }
            return m_TimeToExpiryAdjustment;
        }
        #endregion methods
    }

    /// <summary>
    /// Classe de gestion des données de risque d'un asset
    /// </summary>
    internal sealed class PrismaAssetRisk
    {
        #region members
        // ----------------
        // Paramètres Eurex
        // ----------------
        /// <summary>
        /// Paramètres de l'asset 
        /// </summary>
        private readonly PrismaAsset m_AssetParameters = default;

        /// <summary>
        /// Paramètres du LGS propre à l'asset 
        /// </summary>
        private readonly PrismaAssetLiquidGroupSplit m_AssetLgsParameters = default;

        /// <summary>
        /// Paramètres du LG (Liquidation Group)
        /// </summary>
        private PrismaLiquidationGroup m_LgParameters = default;

        /// <summary>
        /// Paramètres du LGS (Liquidation Group Split)
        /// </summary>
        private PrismaLiquidationGroupSplit m_LgsParameters = default;

        /// <summary>
        /// Paramètres Factors du LC (Liquidity Class)
        /// </summary>
        /// FI 20140227 [] add member
        private List<PrismaLiquidityFactor> m_LiquidityFactorParameters = default;

        /// <summary>
        /// Paramètres 
        /// </summary>
        private PrismaMarketCapacity m_MarketCapacity = default;

        // -----------------
        // Données de calcul
        // -----------------
        /// <summary>
        /// Données de risque des RMS de l'asset 
        /// </summary>
        private List<PrismaAssetRmsRisk> m_AssetRmsRisk = default;
        /// <summary>
        /// Neutral Scenario Price 
        /// </summary>
        private decimal m_NeutralScenarioPrice = 0;
        /// <summary>
        /// Adjusted Neutral Scenario Price 
        /// </summary>
        /// PM 20140613 [19911] Ajout m_AdjustedNeutralScenarioPrice
        private decimal m_AdjustedNeutralScenarioPrice = 0;
        /// <summary>
        /// Sub-Samples des RMS de l'asset
        /// </summary>
        private List<PrismaRmsSubSamplesRisk> m_SubSamples = default;
        /// <summary>
        /// Devise de clearing 
        /// </summary>
        private string m_ClearingCurrency = default;
        /// <summary>
        /// Taux de change du prix courant
        /// </summary>
        /// PM 20140616 [19911] Ajout de m_NeutralExchangeRate
        private decimal m_NeutralExchangeRate = 1m;
        #endregion

        #region accessors
        #region accessors de PrismaAsset m_AssetParameters

        /// <summary>
        /// Id interne de l'asset
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        public PrismaAsset AssetParameters
        {
            get { return m_AssetParameters; }
        }

        /// <summary>
        /// Id interne de l'asset
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        public int IdAsset
        {
            get { return (m_AssetParameters != default) ? m_AssetParameters.IdAsset : 0; }
        }
        /// <summary>
        /// Currency
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        public string AssetCurrency
        {
            get { return (m_AssetParameters != default) ? m_AssetParameters.Currency : default; }
        }
        /// <summary>
        /// Id interne du liquidation group
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        public int IdLg
        {
            get { return (m_AssetParameters != default) ? m_AssetParameters.IdLg : 0; }
        }
        /// <summary>
        /// Neutral Price
        /// (Requiert PrismaAsset parameter)
        /// </summary>
        public decimal NeutralPrice
        {
            get { return (m_AssetParameters != default) ? m_AssetParameters.NeutralPrice : 0; }
        }
        /// <summary>
        /// Neutral Exchange Rate
        /// </summary>
        /// PM 20140616 [19911] Ajout du Neutral Exchange Rate
        public decimal NeutralExchangeRate
        {
            get { return m_NeutralExchangeRate; }
        }
        /// <summary>
        /// Premium Margin (Uniquement pour les options avec primes)
        /// </summary>
        /// PM 20140616 [19911] New
        /// PM 20150415 [20957] Inclure le CM dans le round pour avoir une prime unitaire arroundie
        public decimal PremiumMargin
        {
            // Uniquement pour les Options avec Primes
            get
            {
                return ((m_AssetParameters != default)
                    && ((m_AssetParameters.PutOrCall == PutOrCallEnum.Call) || (m_AssetParameters.PutOrCall == PutOrCallEnum.Put))
                    && (MarginStyle == PrismaMarginStyleEnum.Traditional)) ? PrismaMethod.PrismaRoundAmount(m_AssetParameters.SettlementPrice * m_AssetParameters.Multiplier) * m_NeutralExchangeRate : 0;
            }
        }
        /// <summary>
        /// Present Value (Uniquement pour les options avec primes)
        /// </summary>
        /// PM 20140618 [19911] New
        public decimal PresentValue
        {
            // Uniquement pour les Options avec Primes
            get
            {
                // PM 20200826 [25467] Utilisation de IsTraditionalOption
                //return ((m_AssetParameters != default)
                //    && ((m_AssetParameters.PutOrCall == PutOrCallEnum.Call) || (m_AssetParameters.PutOrCall == PutOrCallEnum.Put))
                //    && (MarginStyle == PrismaMarginStyleEnum.Traditional)) ? m_AssetParameters.NeutralPrice * m_NeutralExchangeRate * m_AssetParameters.Multiplier : 0;

                return (IsTraditionalOption ? (m_AssetParameters.NeutralPrice * m_NeutralExchangeRate * m_AssetParameters.Multiplier) : 0);
            }
        }
        /// <summary>
        /// Reference Price (Uniquement pour les contrats avec marges)
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        /// PM 20140612 [19911] Pour Prisma Release 2.0
        public decimal ReferencePrice
        {
            get { return ((m_AssetParameters != default) && (MarginStyle == PrismaMarginStyleEnum.FuturesStyle)) ? m_AssetParameters.PVReferencePrice : 0; }
        }
        /// <summary>
        /// Time to expiry bucket Id
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        public string TimeToExpiryBucketId
        {
            get { return (m_AssetParameters != default) ? m_AssetParameters.TimeToExpiryBucketId : default; }
        }
        /// <summary>
        /// Moneyness bucket Id (vide pour les futures)
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        public string MoneynessBucketId
        {
            get { return (m_AssetParameters != default) ? m_AssetParameters.MoneynessBucketId : default; }
        }
        /// <summary>
        /// Risk Bucket
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        public string RiskBucket
        {
            get { return (m_AssetParameters != default) ? m_AssetParameters.RiskBucket : default; }
        }
        /// <summary>
        /// Trade unit (Multiplier)
        /// (Requiert PrismaAsset parameter)
        /// </summary>
        public decimal Multiplier
        {
            get { return (m_AssetParameters != default) ? m_AssetParameters.Multiplier : 0; }
        }
        /// <summary>
        ///  Margin Style
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        public PrismaMarginStyleEnum MarginStyle
        {
            get { return (m_AssetParameters != default) ? m_AssetParameters.MarginStyle : PrismaMarginStyleEnum.NA; }
        }

        /// <summary>
        /// Obtient l'id interne (non significatif) du Liquidity Class 
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        public int IdLiquidClass
        {
            get { return (m_AssetParameters != default) ? m_AssetParameters.IdLiquidClass : 0; }
        }

        /// <summary>
        /// symbol du DC (PRODUCTID sous Eurex)
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        /// FI 20140227 Add property
        public string Product
        {
            get { return (m_AssetParameters != default) ? m_AssetParameters.Product : default; }
        }

        /// <summary>
        /// P,C or null
        /// <para>(Requiert PrismaAsset parameter)</para>
        /// </summary>
        /// FI 20140227 Add property
        public string PutCall
        {
            get
            {
                return (m_AssetParameters != default) ? m_AssetParameters.PutCall : default;
            }
        }

        /// <summary>
        /// Indique s'il s'agit d'une option premium style
        /// </summary>
        // PM 20200826 [25467] New
        public bool IsTraditionalOption
        {
            get
            {
                return ((m_AssetParameters != default) && ((m_AssetParameters.PutOrCall.HasValue)
                    && ((m_AssetParameters.PutOrCall == PutOrCallEnum.Call) || (m_AssetParameters.PutOrCall == PutOrCallEnum.Put)))
                    && (MarginStyle == PrismaMarginStyleEnum.Traditional));
            }
        }
        #endregion
        #region accessors de PrismaAssetLiquidGroupSplit m_AssetLgsParameters
        /// <summary>
        /// Id interne du split de groupe de liquidation
        ///<para>(Requiert PrismaAssetLiquidGroupSplit parameter)</para> 
        /// </summary>
        public int IdLgs
        {
            get { return (m_AssetLgsParameters != default) ? m_AssetLgsParameters.IdLgs : 0; }
        }
        /// <summary>
        /// Liquidation Horizon
        /// <para>(Requiert PrismaAssetLiquidGroupSplit parameter)</para>
        /// </summary>
        /// PM 20161019 [22174] Prisma 5.0 : LiquidationHorizon est obsoléte dans m_AssetLgsParameters
        //public int LiquidationHorizon
        //{
        //    get { return (m_AssetLgsParameters != default) ? m_AssetLgsParameters.LiquidationHorizon : 0; }
        //}
        #endregion
        #region accessors de PrismaLiquidationGroup m_LgParameters
        /// <summary>
        /// Obtient ou défini le Liquidation Group parameters
        /// </summary>
        public PrismaLiquidationGroup LgParameters
        {
            set { m_LgParameters = value; }
            get { return m_LgParameters; }
        }
        /// <summary>
        /// Liquidation Group Identifier
        /// (Requiert PrismaAssetLiquidGroupSplit parameter)
        /// </summary>
        public string LiquidationGroup
        {
            get { return (m_LgParameters != default) ? m_LgParameters.Identifier : default; }
        }
        /// <summary>
        /// Type de devise à utiliser
        /// <para>(Requiert PrismaLiquidationGroup parameter)</para>
        /// </summary>
        public PrismaCurrencyTypeFlagEnum CurrencyTypeFlag
        {
            get { return (m_LgParameters != default) ? m_LgParameters.CurrencyTypeFlag : PrismaCurrencyTypeFlagEnum.ProductCurrency; }
        }
        #endregion
        #region accessors de PrismaLiquidationGroupSplit m_LgsParameters
        /// <summary>
        /// Liquidation Group Split parameters
        /// </summary>
        public PrismaLiquidationGroupSplit LgsParameters
        {
            set { m_LgsParameters = value; }
            get { return m_LgsParameters; }
        }
        /// <summary>
        /// Liquidation Group Split Identifier
        /// (Requiert PrismaLiquidationGroupSplit parameter)
        /// </summary>
        public string LiquidationGroupSplit
        {
            get { return (m_LgsParameters != default) ? m_LgsParameters.Identifier : default; }
        }
        /// <summary>
        /// Méthode d'aggregation
        /// (Requiert PrismaLiquidationGroupSplit parameter)
        /// </summary>
        public PrismaAggregationMethod GroupSplitAggregationMethod
        {
            get { return (m_LgsParameters != default) ? m_LgsParameters.AggregationMethod : PrismaAggregationMethod.NA; }
        }
        /// <summary>
        /// Identifier de la méthode risque
        /// <para>(Requiert PrismaLiquidationGroupSplit parameter)</para>
        /// </summary>
        public string RiskMethod
        {
            get { return (m_LgsParameters != default) ? m_LgsParameters.RiskMethod : default; }
        }
        #endregion
        #region accessors de PrismaLiquidityFactor m_LiquidityFactorParameters
        /// <summary>
        /// Obtient ou définit les facteurs de liquidité
        /// </summary>
        public List<PrismaLiquidityFactor> LiquidityFactorParameters
        {
            set { m_LiquidityFactorParameters = value; }
            get { return m_LiquidityFactorParameters; }
        }
        #endregion
        #region accessors de PrismaMarketCapacity m_MarketCapacity
        /// <summary>
        /// Obtient ou définit les makrets capacities
        /// </summary>
        public PrismaMarketCapacity MarketCapacity
        {
            set { m_MarketCapacity = value; }
            get { return m_MarketCapacity; }
        }
        #endregion


        /// <summary>
        /// Obtient ou définie les données de risque des RMS de l'asset 
        /// </summary>
        public List<PrismaAssetRmsRisk> AssetRmsRisk
        {
            get { return m_AssetRmsRisk; }
            set { m_AssetRmsRisk = value; }
        }
        /// <summary>
        /// Obtient le Neutral Scenario price
        /// </summary>
        public decimal NeutralScenarioPrice
        {
            get { return m_NeutralScenarioPrice; }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<PrismaRmsSubSamplesRisk> SubSamples
        {
            get { return m_SubSamples; }
        }
        /// <summary>
        /// Obtient ou définit la devise de la chambre de compensation
        /// </summary>
        public string ClearingCurrency
        {
            get { return m_ClearingCurrency; }
            set { m_ClearingCurrency = value; }
        }
        /// <summary>
        /// Obtient la devise utilisée pour l'asset
        /// <para>la valeur est fonction de CurrencyTypeFlag présent sur le groupe de liquidation</para>
        /// </summary>
        public string Currency
        {
            get { return (CurrencyTypeFlag == PrismaCurrencyTypeFlagEnum.ClearingCurrency) ? ClearingCurrency : AssetCurrency; }
        }
        #endregion

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAsset"></param>
        /// <param name="pAssetLGS"></param>
        public PrismaAssetRisk(PrismaAsset pAsset, PrismaAssetLiquidGroupSplit pAssetLGS)
        {
            m_AssetParameters = pAsset;
            m_AssetLgsParameters = pAssetLGS;
        }
        #endregion

        #region methods
        /// <summary>
        /// Convertion éventuel des prix et calcul des Profit and Losses unitaires
        /// </summary>
        public List<PrismaRmsSubSamplesRisk> EvaluateProfitAndLosses()
        {
            m_SubSamples = new List<PrismaRmsSubSamplesRisk>();
            if (m_AssetRmsRisk != default)
            {
                // Recherche du Risk Measure Set des données historique
                // PM 20230704 [26433] Ajout FilteredWithoutSampling et HistoricalWithoutSampling
                //PrismaAssetRmsRisk historicalRms = m_AssetRmsRisk.FirstOrDefault(r => (r.HistoricalStressed == PrismaHistoricalStressedEnum.Historical) || (r.HistoricalStressed == PrismaHistoricalStressedEnum.Filtered));
                PrismaAssetRmsRisk historicalRms = m_AssetRmsRisk.FirstOrDefault(r => (r.HistoricalStressed == PrismaHistoricalStressedEnum.Historical)
                || (r.HistoricalStressed == PrismaHistoricalStressedEnum.HistoricalWithoutSampling)
                || (r.HistoricalStressed == PrismaHistoricalStressedEnum.Filtered)
                || (r.HistoricalStressed == PrismaHistoricalStressedEnum.FilteredWithoutSampling));
                //PM 20140616 [19911] Déplacement de neutralExchangeRate en tant que membre
                //decimal neutralExchangeRate = 1m;
                if (historicalRms != default(PrismaAssetRmsRisk))
                {
                    if ((historicalRms.IsFxRatesInverted) && (historicalRms.ExchangeRate != 0))
                    {
                        //PM 20140616 [19911] Ajout arrondi en cas de taux de change inverse
                        m_NeutralExchangeRate = PrismaMethod.PrismaRoundExchangeRate(1 / historicalRms.ExchangeRate);
                    }
                    else
                    {
                        m_NeutralExchangeRate = historicalRms.ExchangeRate;
                    }
                }
                m_NeutralScenarioPrice = NeutralPrice * (m_NeutralExchangeRate != 0 ? m_NeutralExchangeRate : 1);
                //PM 20140612 [19911] Ajout calcul m_AdjustedNeutralScenarioPrice
                m_AdjustedNeutralScenarioPrice = (NeutralPrice - ReferencePrice) * (m_NeutralExchangeRate != 0 ? m_NeutralExchangeRate : 1);
                //
                // Convertion éventuel des prix et calcul des Profit & Losses et des Compression Error Adjustment unitaires sur chaque RMS
                foreach (PrismaAssetRmsRisk rms in m_AssetRmsRisk)
                {
                    //PM 20140612 [19911] Ajout ReferencePrice et utilisation de m_AdjustedNeutralScenarioPrice
                    //PrismaRmsSubSamplesRisk subSample = rms.EvaluateRmsSubSamples(Multiplier, m_NeutralScenarioPrice);
                    PrismaRmsSubSamplesRisk subSample = rms.EvaluateRmsSubSamples(Multiplier, ReferencePrice, m_AdjustedNeutralScenarioPrice);
                    m_SubSamples.Add(subSample);
                }
            }
            return m_SubSamples;
        }
        #endregion
    }

    /// <summary>
    /// Classe de gestion des données de risque unitarie d'un Risk Measure Set d'un asset
    /// </summary>
    internal sealed class PrismaAssetRmsRisk
    {
        #region members
        // ----------------
        // Paramètres Eurex
        // ----------------
        // PM 20161019 [22174] Prisma 5.0 : suppression de m_LiquidationHorizon remplacé par l'accesseur LiquidationHorizon
        //private int m_LiquidationHorizon = 0;
        private readonly PrismaRiskMeasureSet m_RmsParameters = default;
        private readonly PrismaAssetRMSLGS m_AssetRmsLgsParameters = default;
        private PrismaExchangeRate m_ExchangeRateParameters = default;
        private PrismaAssetCompressionError m_CompressionErrors = default;
        //
        private PrismaAssetPrice[] m_Prices = default;
        private PrismaExchangeRateRMS[] m_FxRates = default;
        private PrismaAssetVaR[] m_Vars = default;
        private bool m_IsFxRatesInverted = false;
        // -----------------
        // Données de calcul
        // -----------------
        //
        /// <summary>
        /// Liste de valeurs d'un subsample 
        /// <para>chaque item contient n (Price * TUV - NeutralPrice * TUV) et 1 (compression error * TUV)</para>
        /// </summary>
        private PrismaAssetSubSample[] m_AssetSubSamples = default;
        #endregion
        #region accessors
        #region accessors de PrismaAssetRMSLGS m_AssetParameters
        /// <summary>
        /// Id interne de l'asset
        /// (Requiert PrismaAssetRMSLGS parameter)
        /// </summary>
        public int IdAsset
        {
            get { return (m_AssetRmsLgsParameters != default(PrismaAssetRMSLGS)) ? m_AssetRmsLgsParameters.IdAsset : 0; }
        }
        /// <summary>
        /// Id interne du jeu taux de change
        /// (Requiert PrismaAssetRMSLGS parameter)
        /// </summary>
        public int IdFx
        {
            get { return (m_AssetRmsLgsParameters != default(PrismaAssetRMSLGS)) ? m_AssetRmsLgsParameters.IdFx : 0; }
        }
        /// <summary>
        /// Id interne du jeu de mesure de risque du split de groupe de liquidation de l'asset
        /// (Requiert PrismaAssetRMSLGS parameter)
        /// </summary>
        public int IdAssetRmsLgs
        {
            get { return (m_AssetRmsLgsParameters != default(PrismaAssetRMSLGS)) ? m_AssetRmsLgsParameters.IdAssetRmsLgs : 0; }
        }
        /// <summary>
        /// Liquidation Horizon
        /// (Requiert PrismaAssetRMSLGS parameter)
        /// </summary>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout LiquidationHorizon
        public int LiquidationHorizon
        {
            get { return (m_AssetRmsLgsParameters != default(PrismaAssetRMSLGS)) ? m_AssetRmsLgsParameters.LiquidationHorizon : 0; }
        }
        #endregion
        #region accessors de PrismaRiskMeasureSet m_RmsParameters
        /// <summary>
        /// Id interne du jeu de mesure de risque
        /// <para>(Requiert PrismaRiskMeasureSet parameter)</para>
        /// </summary>
        public int IdRms { get { return (m_RmsParameters != default) ? m_RmsParameters.IdRms : 0; } }
        /// <summary>
        /// Identifier du jeu de mesure de risque
        /// <para>(Requiert PrismaRiskMeasureSet parameter)</para>
        /// </summary>
        public string RiskMeasureSet { get { return (m_RmsParameters != default) ? m_RmsParameters.Identifier : Cst.NotFound; } }
        /// <summary>
        /// Type du jeu de mesure de risque
        /// <para>(Requiert PrismaRiskMeasureSet parameter)</para>
        /// </summary>
        public PrismaHistoricalStressedEnum HistoricalStressed { get { return (m_RmsParameters != default) ? m_RmsParameters.HistoricalStressed : PrismaHistoricalStressedEnum.NA; } }

        /// <summary>
        /// Obtient true si jeu de mesure de risque avec Liquidity component
        /// <para>(Requiert PrismaRiskMeasureSet parameter)</para>
        /// </summary>
        /// FI 20140228 add property
        public Boolean IsLiquidityComponent { get { return (m_RmsParameters != default) && m_RmsParameters.IsLiquidityComponent; } }

        /// <summary>
        /// Obtient true si jeu de mesure de risque avec Correlation break Ajustment
        /// <para>(Requiert PrismaRiskMeasureSet parameter)</para>
        /// </summary>
        /// FI 20140228 add property
        public Boolean IsCorrelactionBreak { get { return (m_RmsParameters != default) && m_RmsParameters.IsCorrelactionBreak; } }

        #endregion
        #region accessors de PrismaExchangeRate m_ExchangeRateParameters
        /// <summary>
        /// Exchange Rate Parameters
        /// </summary>
        public PrismaExchangeRate ExchangeRateParameters
        {
            set { m_ExchangeRateParameters = value; }
        }
        /// <summary>
        /// Id interne du jeu de mesure de risque
        /// (Requiert PrismaExchangeRate parameter)
        /// </summary>
        public int IdFxPair
        {
            get { return IsExchangeRateMissing ? 0 : m_ExchangeRateParameters.IdFxPair; }
        }
        /// <summary>
        /// Taux de change
        /// (Requiert PrismaExchangeRate parameter)
        /// </summary>
        public decimal ExchangeRate
        {
            get { return IsExchangeRateMissing ? 1 : m_ExchangeRateParameters.ExchangeRate; }
        }
        /// <summary>
        /// Is ExchangeRate is missing
        /// </summary>
        public bool IsExchangeRateMissing
        {
            get { return (m_ExchangeRateParameters == default(PrismaExchangeRate)); }
        }

        #endregion
        #region accessors de PrismaAssetCompressionError m_CompressionErrors
        /// <summary>
        /// Compression Errors Parameters
        /// </summary>
        public PrismaAssetCompressionError CompressionErrors
        {
            set { m_CompressionErrors = value; }
        }
        #endregion
        #region accessors de PrismaAssetPrice[] m_Prices
        /// <summary>
        /// Prices Parameters
        /// </summary>
        public IEnumerable<PrismaAssetPrice> Prices
        {
            set
            {
                if (value != default(IEnumerable<PrismaAssetPrice>))
                {
                    m_Prices = value.ToArray();
                }
            }
        }
        #endregion
        #region accessors de PrismaExchangeRateRMS[] m_FxRates
        /// <summary>
        /// FX Rates Parameters
        /// </summary>
        public IEnumerable<PrismaExchangeRateRMS> FxRates
        {
            set
            {
                if (value != default(IEnumerable<PrismaExchangeRateRMS>))
                {
                    m_FxRates = value.ToArray();
                }
            }
        }
        #endregion
        #region accessors de PrismaAssetVaR[] m_Vars
        /// <summary>
        /// Obtient ou définit Vars Parameters
        /// </summary>
        /// FI 20130228 Add get
        public IEnumerable<PrismaAssetVaR> Vars
        {
            set
            {
                if (value != default)
                {
                    m_Vars = value.ToArray();
                }
            }
            get
            {
                return m_Vars;
            }
        }



        #endregion
        /// <summary>
        /// Is FX Rates are inverted
        /// </summary>
        public bool IsFxRatesInverted
        {
            get { return m_IsFxRatesInverted; }
            set { m_IsFxRatesInverted = value; }
        }
        #endregion
        #region constructors
        /// <summary>
        /// Initialisation d'un Risk Measure Set d'un asset
        /// </summary>
        /// <param name="pRms"></param>
        /// <param name="pAssetRmsLgs"></param>
        // PM 20161019 [22174] Prisma 5.0 : Suppression du paramètre pLiquidationHorizon
        //public PrismaAssetRmsRisk(int pLiquidationHorizon, PrismaRiskMeasureSet pRms, PrismaAssetRMSLGS pAssetRmsLgs)
        public PrismaAssetRmsRisk(PrismaRiskMeasureSet pRms, PrismaAssetRMSLGS pAssetRmsLgs)
        {
            // PM 20161019 [22174] Prisma 5.0 : m_LiquidationHorizon n'est plus utilisé
            //m_LiquidationHorizon = pLiquidationHorizon;
            m_RmsParameters = pRms;
            m_AssetRmsLgsParameters = pAssetRmsLgs;
        }
        #endregion
        #region methods
        /// <summary>
        /// Lire le taux de change pour un scénario donnée
        /// </summary>
        /// <param name="pScenarioNumber"></param>
        /// <returns></returns>
        private decimal GetFxRateValue(int pScenarioNumber)
        {
            decimal fxRateValue = 1;
            if (m_FxRates != default(PrismaExchangeRateRMS[]))
            {
                PrismaExchangeRateRMS rate = m_FxRates.FirstOrDefault(p => p.ScenarioNumber == pScenarioNumber);
                if (rate != default(PrismaExchangeRateRMS))
                {
                    if (IsFxRatesInverted)
                    {
                        if (rate.ExchangeRate != 0)
                        {
                            //PM 20140616 [19911] Ajout arrondi en cas de taux de change inverse
                            fxRateValue = (rate.ExchangeRate != 0) ? PrismaMethod.PrismaRoundExchangeRate(1 / rate.ExchangeRate) : 1;
                        }
                    }
                    else
                    {
                        fxRateValue = (rate.ExchangeRate != 0) ? rate.ExchangeRate : 1;
                    }
                }
            }
            return fxRateValue;
        }
        /// <summary>
        /// Calcul des Profit and Losses contrevalorisés et répartition en SubSample
        /// </summary>
        /// <param name="pMultiplier">Multiplier de l'asset</param>
        /// <param name="pReferencePrice">Prix de référence de l'asset (ou 1 si non marginé)</param>
        /// <param name="pAdjustedNeutralPrice">Prix de l'asset (ajusté si marginé)</param>
        /// <returns>SubSample</returns>
        /// PM 20140613 [19911] Ajout ReferencePrice (rename old ReferencePrice to AdjustedNeutralPrice)
        /// PM 20161019 [22174] Prisma 5.0 : Remplacement de m_LiquidationHorizon par LiquidationHorizon
        public PrismaRmsSubSamplesRisk EvaluateRmsSubSamples(decimal pMultiplier, decimal pReferencePrice, decimal pAdjustedNeutralPrice)
        {
            Dictionary<int, decimal>[] scenarios = new Dictionary<int, decimal>[LiquidationHorizon];
            decimal[] compressionErrors = new decimal[LiquidationHorizon];
            for (int i = 0; i < LiquidationHorizon; i += 1)
            {
                scenarios[i] = new Dictionary<int, decimal>();
            }
            //
            decimal AdjNeutralPriceMultiplier = pAdjustedNeutralPrice * pMultiplier;
            //
            int fxScenarioNumber = 0;
            decimal fxRate = 1;
            //PM 20140613 [19911] Prise en compte du Reference Price
            foreach (PrismaAssetPrice price in m_Prices.OrderBy(p => p.ScenarioNumber))
            {
                if (LiquidationHorizon >= 1)
                {
                    fxScenarioNumber += 1;
                    fxRate = GetFxRateValue(fxScenarioNumber);
                    scenarios[0].Add(price.ScenarioNumber, ((price.Price1 - pReferencePrice) * fxRate * pMultiplier) - AdjNeutralPriceMultiplier);
                    compressionErrors[0] = (m_CompressionErrors != default(PrismaAssetCompressionError)) ? m_CompressionErrors.CE1 : 0m;
                    if (LiquidationHorizon >= 2)
                    {
                        fxScenarioNumber += 1;
                        fxRate = GetFxRateValue(fxScenarioNumber);
                        scenarios[1].Add(price.ScenarioNumber, ((price.Price2 - pReferencePrice) * fxRate * pMultiplier) - AdjNeutralPriceMultiplier);
                        compressionErrors[1] = (m_CompressionErrors != default(PrismaAssetCompressionError)) ? m_CompressionErrors.CE2 : 0m;
                        if (LiquidationHorizon >= 3)
                        {
                            fxScenarioNumber += 1;
                            fxRate = GetFxRateValue(fxScenarioNumber);
                            scenarios[2].Add(price.ScenarioNumber, ((price.Price3 - pReferencePrice) * fxRate * pMultiplier) - AdjNeutralPriceMultiplier);
                            compressionErrors[2] = (m_CompressionErrors != default(PrismaAssetCompressionError)) ? m_CompressionErrors.CE3 : 0m;
                            if (LiquidationHorizon >= 4)
                            {
                                fxScenarioNumber += 1;
                                fxRate = GetFxRateValue(fxScenarioNumber);
                                scenarios[3].Add(price.ScenarioNumber, ((price.Price4 - pReferencePrice) * fxRate * pMultiplier) - AdjNeutralPriceMultiplier);
                                compressionErrors[3] = (m_CompressionErrors != default(PrismaAssetCompressionError)) ? m_CompressionErrors.CE4 : 0m;
                                if (LiquidationHorizon == 5)
                                {
                                    fxScenarioNumber += 1;
                                    fxRate = GetFxRateValue(fxScenarioNumber);
                                    scenarios[4].Add(price.ScenarioNumber, ((price.Price5 - pReferencePrice) * fxRate * pMultiplier) - AdjNeutralPriceMultiplier);
                                    compressionErrors[4] = (m_CompressionErrors != default(PrismaAssetCompressionError)) ? m_CompressionErrors.CE5 : 0m;
                                }
                            }
                        }
                    }
                }
            }
            m_AssetSubSamples = new PrismaAssetSubSample[LiquidationHorizon];
            for (int i = 0; i < LiquidationHorizon; i += 1)
            {

                m_AssetSubSamples[i] = new PrismaAssetSubSample(m_RmsParameters, scenarios[i], compressionErrors[i] * pMultiplier);
            }
            //
            return new PrismaRmsSubSamplesRisk(m_RmsParameters, m_AssetSubSamples);
        }
        #endregion
    }

    /// <summary>
    /// Classe de gestion des données de risque d'un Sub-Sample d'un asset
    /// </summary>
    internal sealed class PrismaAssetSubSample
    {
        #region members
        // ----------------
        // Paramètres Eurex
        // ----------------
        private readonly PrismaRiskMeasureSet m_RmsParameters = default;
        // -----------------
        // Données de calcul
        // -----------------
        private readonly Dictionary<int, decimal> m_Scenarios = default;
        private readonly decimal m_CompressionError = 0m;
        #endregion
        #region accessors
        /// <summary>
        /// Obtient les paramètres du RMS
        /// </summary>
        public PrismaRiskMeasureSet RmsParameters
        {
            get { return m_RmsParameters; }
        }
        /// <summary>
        /// Obtient les scenarios
        /// </summary>
        public Dictionary<int, decimal> Scenarios
        {
            get { return m_Scenarios; }
        }
        /// <summary>
        /// Obtient le compression Error
        /// </summary>
        public decimal CompressionError
        {
            get { return m_CompressionError; }
        }
        #endregion
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRms"></param>
        /// <param name="pScenarios"></param>
        /// <param name="pCompressionError"></param>
        public PrismaAssetSubSample(PrismaRiskMeasureSet pRms, Dictionary<int, decimal> pScenarios, decimal pCompressionError)
        {
            m_RmsParameters = pRms;
            m_Scenarios = pScenarios;
            m_CompressionError = pCompressionError;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Ajoute les scénarios de deux Sub-Samples et retourne un nouvel ensemble de scénarios
        /// </summary>
        /// <param name="pSubSampleOne">Premier Sub-Sample</param>
        /// <param name="pSubSampleTwo">Deuxième Sub-Sample</param>
        /// <returns>Nouvel ensemble de scénarios</returns>
        // PM 20200826 [25467] New
        public static Dictionary<int, decimal> AddScenarios(PrismaAssetSubSample pSubSampleOne, PrismaAssetSubSample pSubSampleTwo)
        {
            Dictionary<int, decimal> aggregateScenarios = default;
            if ((pSubSampleOne != default) && (pSubSampleTwo != default))
            {
                aggregateScenarios = (
                    from val1 in pSubSampleOne.Scenarios
                    join val2 in pSubSampleTwo.Scenarios on val1.Key equals val2.Key
                    select new KeyValuePair<int, decimal>(val1.Key, val1.Value + val2.Value)
                    ).ToDictionary(s => s.Key, s => s.Value);
                //
                // Si jamais il y avait plus de valeurs dans certains Sub-Sample
                IEnumerable<KeyValuePair<int, decimal>> missingFromSubSampleOne = pSubSampleOne.Scenarios.Where(a => false == aggregateScenarios.ContainsKey(a.Key));
                IEnumerable<KeyValuePair<int, decimal>> missingFromSubSampleTwo = pSubSampleTwo.Scenarios.Where(a => false == aggregateScenarios.ContainsKey(a.Key));
                //
                Dictionary<int, decimal> scenarios = aggregateScenarios.Concat(missingFromSubSampleOne).Concat(missingFromSubSampleTwo).ToDictionary(s => s.Key, s => s.Value);
            }
            return aggregateScenarios;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de gestion de l'ensemble des valeurs de risque d'un Sub-Sample d'un asset d'un RMS
    /// </summary>
    internal sealed class PrismaSubSampleRisk
    {
        #region members
        // -----------------
        // Données de calcul
        // -----------------
        // Scenario et Compression Error
        private readonly PrismaAssetSubSample m_UnitSubSample = default;
        private PrismaAssetSubSample m_QtySubSample = default;
        // PM 20200826 [25467] Ajout scenarios de position option et indicateur de position option
        private PrismaAssetSubSample m_OptionQtySubSample = default;
        private bool m_IsWithOption = false;
        // Quantité en position
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private decimal m_Quantity = 1;
        // Pour le calcul du Pure Market Risk Component
        private decimal m_ValueAtRisk = 0m;
        private decimal m_ValueAtRiskScaled = 0m;
        // PM 20161019 [22174] Prisma 5.0 : Ajout m_PureMarketRisk
        private decimal m_PureMarketRisk = 0m;
        // Pour le calcul du Correlation Break Adjustment
        private decimal m_CbValueAtRisk = 0m;
        private decimal m_MeanExcessRisk = 0m;
        private decimal m_CbLowerBound = 0m;
        private decimal m_CbUpperBound = 0m;
        private decimal m_CorrelationBreak = 0m;
        // Pour le calcul du Liquidity Risk Component
        private decimal m_ValueAtRiskLiquidityRiskComponent = 0m;
        #endregion
        #region accessors
        #region paramètres
        /// <summary>
        /// Obtient les paramètres du RMS
        /// </summary>
        public PrismaRiskMeasureSet RmsParameters
        {
            get { return (m_UnitSubSample != default) ? m_UnitSubSample.RmsParameters : default; }
        }

        /// <summary>
        /// Obtient la liste des ((Scenario Prices - Neutral scenarion * TUV) * Qte)
        /// </summary>
        public Dictionary<int, decimal> Scenarios
        {
            get { return (m_QtySubSample != default) ? m_QtySubSample.Scenarios : default; }
        }

        /// <summary>
        /// Obtient la liste des ((Scenario Prices - Neutral scenarion * TUV) * Qte) des Options
        /// </summary>
        // PM 20200826 [25467] Ajout OptionSubSamplesRisk pour calcul de Long Option Credit
        public Dictionary<int, decimal> OptionScenarios
        {
            get { return (m_OptionQtySubSample != default) ? m_OptionQtySubSample.Scenarios : default; }
        }

        /// <summary>
        /// Indique si le sub-sample concerne des options
        /// </summary>
        public bool IsWithOption
        {
            get { return m_IsWithOption; }
        }

        /// <summary>
        /// Sub-Sample option
        /// </summary>
        public PrismaAssetSubSample OptionQtySubSample
        {
            get { return m_OptionQtySubSample; }
        }

        /// <summary>
        /// Compression Error de l'asset
        /// </summary>
        //PM 20140613 [19911] Changement de nom de CompressionError a AssetCompressionError
        public decimal AssetCompressionError
        {
            get { return (m_UnitSubSample != default) ? m_UnitSubSample.CompressionError : 0m; }
        }
        #endregion
        #region Pure Market Risk Component
        /// <summary>
        /// Pure Market Risk : VaR
        /// </summary>
        public decimal ValueAtRisk { get { return m_ValueAtRisk; } }
        /// <summary>
        /// Pure Market Risk : VaR scaled
        /// </summary>
        public decimal ValueAtRiskScaled { get { return m_ValueAtRiskScaled; } }
        /// <summary>
        /// Pure Market Risk
        /// </summary>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout PureMarketRisk
        public decimal PureMarketRisk { get { return m_PureMarketRisk; } }
        #endregion

        #region Correlation Break Adjustment
        /// <summary>
        /// Correlation Break ValueAtRisk
        /// </summary>
        public decimal CbValueAtRisk { get { return m_CbValueAtRisk; } }
        /// <summary>
        /// Correlation Break Mean Excess Risk
        /// </summary>
        public decimal MeanExcessRisk { get { return m_MeanExcessRisk; } }
        /// <summary>
        /// Correlation Break Lower Bound
        /// </summary>
        public decimal CbLowerBound { get { return m_CbLowerBound; } }
        /// <summary>
        /// Correlation Break Upper Bound
        /// </summary>
        public decimal CbUpperBound { get { return m_CbUpperBound; } }
        /// <summary>
        /// 
        /// </summary>
        public decimal CorrelationBreakAdjustment { get { return m_CorrelationBreak; } }
        #endregion

        #region CompressionError
        /// <summary>
        /// Compression Error
        /// </summary>
        //PM 20140613 [19911] Ajout nouvel accesseur CompressionError
        public decimal CompressionError
        {
            get { return (m_QtySubSample != default) ? m_QtySubSample.CompressionError : 0m; }
        }
        #endregion

        #region CompressionAdjustment
        /// <summary>
        /// Compression Error Adjustement 
        /// </summary>
        public decimal CompressionAdjustment
        {
            //PM 20140613 [19911] Il faut maintenant prendre la Racine carré car les CompressionError ont été sommés en puissance de 2
            //get { return (m_QtySubSample != default) ? m_QtySubSample.CompressionError : 0m; }
            get { return (m_QtySubSample != default) ? (decimal)System.Math.Sqrt((double)m_QtySubSample.CompressionError) : 0m; }
        }
        #endregion

        #region MarketRiskComponent
        /// <summary>
        /// MarketRisk = PureMarketRisk + CorrelationBreakAdjustment + CompressionAdjustment
        /// </summary>
        /// PM 20161019 [22174] Prisma 5.0 : ValueAtRiskScaled remplacé par PureMarketRisk
        public decimal MarketRiskComponent
        {
            //get { return ValueAtRiskScaled + CorrelationBreakAdjustment + CompressionAdjustment; }
            get { return PureMarketRisk + CorrelationBreakAdjustment + CompressionAdjustment; }
        }
        #endregion

        #region Liquidity Component
        /// <summary>
        /// Liquidity Risk Component : VaR
        /// </summary>
        public decimal ValueAtRiskLiquidityRiskComponent { get { return m_ValueAtRiskLiquidityRiskComponent; } }
        #endregion
        #endregion

        #region constructors
        /// <summary>
        /// Création d'un Sub-Sample pour le calcul du risque
        /// </summary>
        /// <param name="pUnitSubSample">Données unitaires du Sub-Sample</param>
        public PrismaSubSampleRisk(PrismaAssetSubSample pUnitSubSample)
        {
            m_UnitSubSample = pUnitSubSample;
            m_QtySubSample = pUnitSubSample;
        }

        /// <summary>
        /// Création d'un Sub-Sample pour le calcul du risque
        /// </summary>
        /// <param name="pQtySubSample">Sub-Sample pondéré de la quantité</param>
        /// <param name="pOptionQtySubSample">Sub-Sample pondéré de la quantité option</param>
        // PM 20200826 [25467] New
        public PrismaSubSampleRisk(PrismaAssetSubSample pQtySubSample, PrismaAssetSubSample pOptionQtySubSample)
        {
            m_UnitSubSample = pQtySubSample;
            m_QtySubSample = pQtySubSample;
            m_OptionQtySubSample = pOptionQtySubSample;
            if (m_OptionQtySubSample != default)
            {
                m_IsWithOption = true;
            }
        }

        /// <summary>
        /// Création d'un Sub-Sample pour le calcul du risque
        /// </summary>
        /// <param name="pUnitSubSample">Données unitaires du Sub-Sample</param>
        /// <param name="pQuantity">Quantité en position</param>
        /// <param name="pIsPositionOption">Indicateur de position option</param>
        // EG 20150920 [21374] Int (int32) to Long (Int64)   
        // EG 20170127 Qty Long To Decimal
        // PM 20200826 [25467] Ajout indicateur de position option
        //public PrismaSubSampleRisk(PrismaAssetSubSample pUnitSubSample, decimal pQuantity)
        public PrismaSubSampleRisk(PrismaAssetSubSample pUnitSubSample, decimal pQuantity, bool pIsPositionOption)
        {
            m_UnitSubSample = pUnitSubSample;
            ApplyPosition(pQuantity, pIsPositionOption);
        }
        #endregion

        #region methods
        #region PrismaSubSampleRisk ApplyPosition
        /// <summary>
        /// Applique une Quantité au Sub-Sample courant
        /// </summary>
        /// <param name="pQuantity">Quantité en position</param>
        /// <param name="pIsPositionOption">Indicateur de position option</param>
        // EG 20150920 [21374] Int (int32) to Long (Int64)   
        // EG 20170127 Qty Long To Decimal
        // PM 20200826 [25467] Ajout indicateur de position option
        //public void ApplyPosition(decimal pQuantity)
        public void ApplyPosition(decimal pQuantity, bool pIsPositionOption)
        {
            if (m_UnitSubSample != default)
            {
                m_Quantity = pQuantity;
                m_IsWithOption = pIsPositionOption;
                //
                Dictionary<int, decimal> qtyScenarios = (
                    from scenario in m_UnitSubSample.Scenarios
                    select new KeyValuePair<int, decimal>(scenario.Key, scenario.Value * m_Quantity)
                    ).ToDictionary(s => s.Key, s => s.Value);
                //
                //PM 20160613 [19911] Utilisation de Pow au lieu de Abs
                //decimal compressionAdjustement = System.Math.Abs(m_UnitSubSample.CompressionError * m_Quantity);
                // EG 20170127 Qty Long To Decimal
                decimal compressionAdjustement = (decimal)System.Math.Pow((double)m_UnitSubSample.CompressionError * (double)m_Quantity, (double)2);
                m_QtySubSample = new PrismaAssetSubSample(m_UnitSubSample.RmsParameters, qtyScenarios, compressionAdjustement);
                //
                if (m_IsWithOption)
                {
                    m_OptionQtySubSample = m_QtySubSample;
                }
            }
        }
        /// <summary>
        /// Multiplie les prix des scénarios du Sub-Sample en paramètre par la quantité et retourne un nouveau PrismaSubSampleRisk
        /// </summary>
        /// <param name="pSubSambleRiskSource">Sub-Sample</param>
        /// <param name="pQuantity">Quantité en position</param>
        /// <param name="pIsPositionOption">Indicateur de position option</param>
        /// <returns>nouveau PrismaSubSampleRisk</returns>
        // EG 20150920 [21374] Int (int32) to Long (Int64)   
        // EG 20170127 Qty Long To Decimal
        // PM 20200826 [25467] Ajout indicateur de position option
        //public static PrismaSubSampleRisk ApplyPosition(PrismaSubSampleRisk pSubSambleRiskSource, decimal pQuantity)
        public static PrismaSubSampleRisk ApplyPosition(PrismaSubSampleRisk pSubSambleRiskSource, decimal pQuantity, bool pIsPositionOption)
        {
            PrismaSubSampleRisk posSubSampleRisk = default;
            if ((pSubSambleRiskSource != default)
                && (pSubSambleRiskSource.Scenarios != default))
            {
                posSubSampleRisk = new PrismaSubSampleRisk(pSubSambleRiskSource.m_UnitSubSample, pQuantity, pIsPositionOption);
            }
            return posSubSampleRisk;
        }
        #endregion
        #region PrismaSubSampleRisk AddScenarios
        /// <summary>
        /// Ajoute les scénarios de deux Sub-Samples de même Rms entre-eux et retourne un nouveau PrismaSubSampleRisk
        /// </summary>
        /// <param name="pSubSampleOne">Premier Sub-Sample</param>
        /// <param name="pSubSampleTwo">Deuxième Sub-Sample</param>
        /// <returns>nouveau PrismaSubSampleRisk</returns>
        public static PrismaSubSampleRisk AddScenarios(PrismaSubSampleRisk pSubSampleOne, PrismaSubSampleRisk pSubSampleTwo)
        {
            PrismaSubSampleRisk aggregat = default;
            if ((pSubSampleOne != default) && (pSubSampleTwo != default)
                && (pSubSampleOne.RmsParameters != default) && (pSubSampleTwo.RmsParameters != default)
                && (pSubSampleOne.Scenarios != default) && (pSubSampleTwo.Scenarios != default)
                && (pSubSampleOne.RmsParameters.IdRmsLgs == pSubSampleTwo.RmsParameters.IdRmsLgs))
            {
                // // PM 20200826 [25467] Remplacé par méthode: PrismaAssetSubSample.AddScenarios
                //Dictionary<int, decimal> aggregateSubSample = (
                //    from val1 in pSubSampleOne.Scenarios
                //    join val2 in pSubSampleTwo.Scenarios on val1.Key equals val2.Key
                //    select new KeyValuePair<int, decimal>(val1.Key, val1.Value + val2.Value)
                //    ).ToDictionary(s => s.Key, s => s.Value);
                ////
                //// Si jamais il y avait plus de valeurs dans certains Sub-Sample
                //IEnumerable<KeyValuePair<int, decimal>> missingFromSubSampleOne = pSubSampleOne.Scenarios.Where(a => false == aggregateSubSample.ContainsKey(a.Key));
                //IEnumerable<KeyValuePair<int, decimal>> missingFromSubSampleTwo = pSubSampleTwo.Scenarios.Where(a => false == aggregateSubSample.ContainsKey(a.Key));
                ////
                //Dictionary<int, decimal> scenarios = aggregateSubSample.Concat(missingFromSubSampleOne).Concat(missingFromSubSampleTwo).ToDictionary(s => s.Key, s => s.Value);

                Dictionary<int, decimal> scenarios = PrismaAssetSubSample.AddScenarios(pSubSampleOne.m_QtySubSample, pSubSampleTwo.m_QtySubSample);

                //
                //PM 20140613 [19911] Utilisation du Compression Error lors des cumul et non du Compression Adjustement
                //decimal newCompressionAdjustement = pSubSampleOne.CompressionAdjustment + pSubSampleTwo.CompressionAdjustment;
                //PrismaAssetSubSample newSubSample = new PrismaAssetSubSample(pSubSampleOne.RmsParameters, scenarios, newCompressionAdjustement);
                decimal newCompressionError = pSubSampleOne.CompressionError + pSubSampleTwo.CompressionError;
                PrismaAssetSubSample newSubSample = new PrismaAssetSubSample(pSubSampleOne.RmsParameters, scenarios, newCompressionError);

                // PM 20200826 [25467] Gestion des sub-sample option
                PrismaAssetSubSample newOptionSubSample = default;
                if (pSubSampleOne.IsWithOption)
                {
                    if (pSubSampleTwo.IsWithOption)
                    {
                        Dictionary<int, decimal> scenariosOption = PrismaAssetSubSample.AddScenarios(pSubSampleOne.m_OptionQtySubSample, pSubSampleTwo.m_OptionQtySubSample);
                        newOptionSubSample = new PrismaAssetSubSample(pSubSampleOne.RmsParameters, scenariosOption, newCompressionError); ;
                    }
                    else
                    {
                        newOptionSubSample = pSubSampleOne.m_OptionQtySubSample;
                    }
                }
                else if (pSubSampleTwo.IsWithOption)
                {
                    newOptionSubSample = pSubSampleTwo.m_OptionQtySubSample;
                }
                //
                aggregat = new PrismaSubSampleRisk(newSubSample, newOptionSubSample);
            }
            return aggregat;
        }
        #endregion
        /// <summary>
        /// Calcul les valeurs de risque de ce Sub-Sample
        /// </summary>
        /// <returns>Le Market Risk Component du Sub-Sample</returns>
        public decimal EvaluateMarketRiskComponent()
        {
            if ((RmsParameters != default) && (Scenarios != default))
            {
                // Evaluation du Pure Market Risk
                EvaluatePureMarketRisk();
                // Evaluation du Correlation Break Adjustment
                EvaluateCorrelationBreakAdjustment();
                // Market Risk Component
                // => directement calculé dans l'accessor MarketRiskComponent
            }
            return MarketRiskComponent;
        }
        /// <summary>
        /// Calcule de la value at risk du Sub-Sample (Pure Market Risk Component)
        /// </summary>
        /// <returns>Le Pure Market Risk du Sub-Sample</returns>
        private decimal EvaluatePureMarketRisk()
        {
            if ((RmsParameters != default) && (Scenarios != default))
            {
                // Ensemble des prix
                IEnumerable<decimal> scenariosValues = Scenarios.Select(s => s.Value);
                //
                // PM 20161019 [22174] Prisma 5.0 : Ajout calcul du PureMarketRisk et gestion des scénarios "Event"
                if (PrismaHistoricalStressedEnum.Event == RmsParameters.HistoricalStressed)
                {
                    if (scenariosValues.Count() > 0)
                    {
                        // Prendre la somme des NumberOfWorstScenarios plus petites valeurs de scenarios
                        IEnumerable<decimal> scenariosValuesOrdered = scenariosValues.OrderBy(d => d);
                        scenariosValuesOrdered.Take(System.Math.Min(RmsParameters.NumberOfWorstScenarios, scenariosValues.Count()));
                        m_PureMarketRisk = -1 * scenariosValuesOrdered.Sum(d => System.Math.Min(d, 0));
                    }
                }
                else
                {
                    // Paramètres pour le Pure Market Risk Component
                    decimal confidenceLevel = RmsParameters.ConfidenceLevel;
                    bool isUseRobustness = RmsParameters.IsUseRobustness;
                    decimal scalingFactor = RmsParameters.ScalingFactor;
                    //// Ensemble des prix
                    //IEnumerable<decimal> scenariosValues = Scenarios.Select(s => s.Value);
                    // Value At Risk
                    m_ValueAtRisk = PrismaMethod.ValueAtRisk(scenariosValues, confidenceLevel);
                    //
                    if (isUseRobustness)
                    {
                        m_ValueAtRiskScaled = m_ValueAtRisk * scalingFactor;
                    }
                    else
                    {
                        m_ValueAtRiskScaled = m_ValueAtRisk;
                    }
                    m_PureMarketRisk = m_ValueAtRiskScaled;
                }
            }
            //return ValueAtRiskScaled;
            return PureMarketRisk;
        }

        /// <summary>
        /// Calcul le Correlation Break Adjustment du Sub-Sample
        /// <remarks>Nécessite que la calcul de la Value At Risk ait déjà été effectué</remarks>
        /// </summary>
        /// <returns>Le Correlation BreakAdjustment du Sub-Sample</returns>
        private decimal EvaluateCorrelationBreakAdjustment()
        {
            // Uniquement pour les scénarios Filtered Historical
            // PM 20230704 [26433] Ajout FilteredWithoutSampling et HistoricalWithoutSampling
            if ((RmsParameters != default)
                && (Scenarios != default)
                && (RmsParameters.IsCorrelactionBreak)
                && ((RmsParameters.HistoricalStressed == PrismaHistoricalStressedEnum.Filtered)
                  || (RmsParameters.HistoricalStressed == PrismaHistoricalStressedEnum.FilteredWithoutSampling)
                  || (RmsParameters.HistoricalStressed == PrismaHistoricalStressedEnum.Historical)
                  || (RmsParameters.HistoricalStressed == PrismaHistoricalStressedEnum.HistoricalWithoutSampling)))
            {
                // Paramètres pour le Correlation Break Adjustment
                decimal confidenceLevel = RmsParameters.CorrelationBreakConfidenceLevel;
                decimal lowerBound = RmsParameters.CorrelationBreakMin;
                decimal upperBound = RmsParameters.CorrelationBreakMax;
                decimal kappa = RmsParameters.CorrelationBreakMultiplier;
                int subWindowSize = RmsParameters.CorrelationBreakSubWindow;
                //
                IEnumerable<decimal> scenariosValues = Scenarios.Select(s => s.Value);
                // Value At Risk pour l'ensemble de tous les scénarios du Sub-Sample
                m_CbValueAtRisk = PrismaMethod.ValueAtRisk(scenariosValues, confidenceLevel);
                // Correlation Break Upper Bound & Lower Bound
                // PM 20161019 [22174] Prisma 5.0 : m_ValueAtRiskScaled remplacé par PureMarketRisk
                //m_CbLowerBound = m_ValueAtRiskScaled * lowerBound / 100;
                //m_CbUpperBound = m_ValueAtRiskScaled * upperBound / 100;
                m_CbLowerBound = PureMarketRisk * lowerBound / 100;
                m_CbUpperBound = PureMarketRisk * upperBound / 100;
                //
                int nbDecalage = Scenarios.Count - subWindowSize;
                if (nbDecalage > 0)
                {
                    decimal[] timeWindowVaR = new decimal[nbDecalage + 1];
                    int firstScenarioNo = 1;
                    int lastScenarioNo = subWindowSize;
                    int nbScenario = Scenarios.Count;
                    List<double> squaredDeviations = new List<double>();
                    while (lastScenarioNo <= nbScenario)
                    {
                        IEnumerable<decimal> windowValues =
                            from scenario in Scenarios
                            where (scenario.Key >= firstScenarioNo) && (scenario.Key <= lastScenarioNo)
                            select scenario.Value;
                        //
                        timeWindowVaR[firstScenarioNo - 1] = PrismaMethod.ValueAtRisk(windowValues, confidenceLevel);
                        if (timeWindowVaR[firstScenarioNo - 1] > m_CbValueAtRisk)
                        {
                            double deviation = System.Math.Pow((double)(timeWindowVaR[firstScenarioNo - 1] - m_CbValueAtRisk), 2);
                            squaredDeviations.Add(deviation);
                        }
                        //
                        firstScenarioNo += 1;
                        lastScenarioNo += 1;
                    }
                    //
                    if (squaredDeviations.Count > 0)
                    {
                        double sumSquaredDeviations = squaredDeviations.Sum();
                        double nbSquaredDeviations = (double)(squaredDeviations.Count);
                        if (nbSquaredDeviations != 0d)
                        {
                            m_MeanExcessRisk = (decimal)System.Math.Sqrt(1.0 / nbSquaredDeviations * sumSquaredDeviations);
                        }
                        else
                        {
                            m_MeanExcessRisk = 0m;
                        }
                    }
                    else
                    {
                        m_MeanExcessRisk = 0m;
                    }
                    //
                    m_CorrelationBreak = System.Math.Min(m_CbUpperBound, System.Math.Max(m_CbLowerBound, m_MeanExcessRisk * kappa));
                }
            }
            return CorrelationBreakAdjustment;
        }

        /// <summary>
        /// Calcule du VaR(Value at Risk) du Sub-Sample selon les spécifications propres au calcul du Liquidity Risk Component
        /// </summary>
        public decimal EvaluateVaRLiquidityRisk()
        {
            if ((RmsParameters != default) && (Scenarios != default))
            {
                if (RmsParameters.IsLiquidityComponent)
                {
                    // Confidence Level pour le VaR
                    decimal confidenceLevel = RmsParameters.AlphaConfidenceLevel;

                    // Ensemble des prix
                    IEnumerable<decimal> scenariosValues = Scenarios.Select(s => s.Value);

                    // Value At Risk
                    m_ValueAtRiskLiquidityRiskComponent = PrismaMethod.ValueAtRisk(scenariosValues, confidenceLevel);
                }
            }
            return ValueAtRiskLiquidityRiskComponent;
        }
        #endregion
    }

    /// <summary>
    /// Classe de gestion de l'ensemble des valeurs de risque de tous les Sub-Samples d'un RMS
    /// </summary>
    internal sealed class PrismaRmsSubSamplesRisk
    {
        #region members
        // ----------------
        // Paramètres Eurex
        // ----------------
        private readonly PrismaRiskMeasureSet m_RmsParameters = default;
        // -----------------
        // Données de calcul
        // -----------------
        private readonly PrismaAssetSubSample[] m_UnitSubSamples = default;
        private readonly PrismaSubSampleRisk[] m_SubSamplesRisk = default;
        private decimal m_MarketRiskComponent = 0m;
        private decimal m_ValueAtRiskLiquidityRiskComponent = 0m;
        private decimal m_AlphaFactor = 0m;
        #endregion
        #region accessors
        #region accessors de PrismaRiskMeasureSet m_RmsParameters
        /// <summary>
        /// Paramétrs de calcul du Risk Measure Set
        /// </summary>
        public PrismaRiskMeasureSet RmsParameters
        {
            get { return m_RmsParameters; }
        }
        /// <summary>
        /// Id interne du jeu de mesure de risque
        /// (Requiert PrismaRiskMeasureSet parameter)
        /// </summary>
        public int IdRms { get { return (m_RmsParameters != default) ? m_RmsParameters.IdRms : 0; } }
        /// <summary>
        /// Identifier du jeu de mesure de risque
        /// (Requiert PrismaRiskMeasureSet parameter)
        /// </summary>
        public string RiskMeasureSet { get { return (m_RmsParameters != default) ? m_RmsParameters.Identifier : Cst.NotFound; } }
        /// <summary>
        /// Type du jeu de mesure de risque
        /// (Requiert PrismaRiskMeasureSet parameter)
        /// </summary>
        public PrismaHistoricalStressedEnum HistoricalStressed { get { return (m_RmsParameters != default) ? m_RmsParameters.HistoricalStressed : PrismaHistoricalStressedEnum.NA; } }
        /// <summary>
        /// AggregationMethod du Risk Measure Set
        /// </summary>
        public PrismaAggregationMethod AggregationMethod { get { return (m_RmsParameters != default) ? m_RmsParameters.AggregationMethod : PrismaAggregationMethod.NA; } }
        /// <summary>
        /// Niveau de confiance de la mesure de risque
        /// </summary>
        public decimal ConfidenceLevel { get { return (m_RmsParameters != default) ? m_RmsParameters.ConfidenceLevel : 0m; } }
        /// <summary>
        /// Utiliser ou non la robustesse pour la mesure de risque
        /// </summary>
        public bool IsUseRobustness { get { return (m_RmsParameters != default) && m_RmsParameters.IsUseRobustness; } }
        /// <summary>
        /// Facteur d'échelle pour la robustesse
        /// </summary>
        public decimal ScalingFactor { get { return (m_RmsParameters != default) ? m_RmsParameters.ScalingFactor : 0m; } }
        /// <summary>
        /// Niveau de confiance utilisé pour le calcul du correlation break
        /// </summary>
        public decimal CorrelationBreakConfidenceLevel { get { return (m_RmsParameters != default) ? m_RmsParameters.CorrelationBreakConfidenceLevel : 0m; } }
        /// <summary>
        /// Taille de la sous fenêtre utilisée pour le calcul du correlation break 
        /// </summary>
        public int CorrelationBreakSubWindow { get { return (m_RmsParameters != default) ? m_RmsParameters.CorrelationBreakSubWindow : 0; } }
        /// <summary>
        /// Multiplier pour le calcul de l'ajustement de correlation break
        /// </summary>
        public decimal CorrelationBreakMultiplier { get { return (m_RmsParameters != default) ? m_RmsParameters.CorrelationBreakMultiplier : 0m; } }
        /// <summary>
        /// Ajustement de correlation break minimum
        /// </summary>
        public decimal CorrelationBreakMin { get { return (m_RmsParameters != default) ? m_RmsParameters.CorrelationBreakMin : 0m; } }
        /// <summary>
        /// Ajustement de correlation break maximum
        /// </summary>
        public decimal CorrelationBreakMax { get { return (m_RmsParameters != default) ? m_RmsParameters.CorrelationBreakMax : 0m; } }
        #endregion
        /// <summary>
        /// Nombre de Sub-Samples
        /// </summary>
        public int SubSampleCount
        {
            get { return ((m_SubSamplesRisk != default) ? m_SubSamplesRisk.Length : 0); }
        }

        /// <summary>
        /// Sub-Samples
        /// </summary>
        public PrismaSubSampleRisk[] SubSamplesRisk { get { return m_SubSamplesRisk; } }

        /// <summary>
        /// Market Risk Component
        /// </summary>
        public decimal MarketRiskComponent { get { return m_MarketRiskComponent; } }

        /// <summary>
        /// Scaled (Weigthed) Market Risk Component 
        /// </summary>
        public decimal ScaledMarketRiskComponent { get { return m_MarketRiskComponent * (((m_RmsParameters != default) && (m_RmsParameters.WeightingFactor != 0)) ? m_RmsParameters.WeightingFactor / 100 : 1); } }

        /// <summary>
        /// Obtient VaR(Value at Risk) selon les spécifications propres au calcul du Liquidity Risk Component
        /// </summary>
        public decimal ValueAtRiskLiquidityComponent
        {
            get { return m_ValueAtRiskLiquidityRiskComponent; }
        }

        /// <summary>
        /// Obtient ou définit le facteur Alpha du RMS
        /// <para>Ce facteur existe uniquement sur un RMS tel que IsLiquidityComponent = true</para>
        /// </summary>
        public decimal AlphaFactor
        {
            get { return m_AlphaFactor; }
            set { m_AlphaFactor = value; }
        }
        #endregion

        #region constructors
        public PrismaRmsSubSamplesRisk(PrismaRiskMeasureSet pRms, PrismaAssetSubSample[] pUnitSubSamples)
        {
            m_RmsParameters = pRms;
            m_UnitSubSamples = pUnitSubSamples;
            m_SubSamplesRisk = (
                from unitSubSample in m_UnitSubSamples
                select new PrismaSubSampleRisk(unitSubSample)
                ).ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRms"></param>
        /// <param name="pSubSamples"></param>
        /// <param name=""></param>
        private PrismaRmsSubSamplesRisk(PrismaRiskMeasureSet pRms, PrismaSubSampleRisk[] pSubSamples)
        {
            m_RmsParameters = pRms;
            m_SubSamplesRisk = pSubSamples;
        }
        #endregion

        #region methods
        #region PrismaRmsSubSamplesRisk ApplyPosition
        /// <summary>
        /// Multiplie les prix des scénarios des sub-sample par la quantité et retourne un nouveau PrismaRmsSubSamplesRisk
        /// </summary>
        /// <param name="pQuantity">Quantité en position</param>
        /// <param name="pIsPositionOption">Indicateur de position option</param>
        /// <returns>nouveau PrismaRmsSubSamplesRisk</returns>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // PM 20200826 [25467] Ajout indicateur de position option
        //public PrismaRmsSubSamplesRisk ApplyPosition(decimal pQuantity) 
        public PrismaRmsSubSamplesRisk ApplyPosition(decimal pQuantity, bool pIsPositionOption)

        {
            return ApplyPosition(this, pQuantity, pIsPositionOption);
        }

        /// <summary>
        /// Multiplie les prix des scénarios des Sub-Samples par la quantité et retourne un nouveau PrismaRmsSubSamplesRisk
        /// </summary>
        /// <param name="pSubSambleRiskSource">Sub-sample</param>
        /// <param name="pQuantity">Quantité en position</param>
        /// <param name="pIsPositionOption">Indicateur de position option</param>
        /// <returns>nouveau PrismaRmsSubSamplesRisk</returns>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        // PM 20200826 [25467] Ajout indicateur de position option
        //public static PrismaRmsSubSamplesRisk ApplyPosition(PrismaRmsSubSamplesRisk pSubSambleRiskSource, decimal pQuantity)
        public static PrismaRmsSubSamplesRisk ApplyPosition(PrismaRmsSubSamplesRisk pSubSambleRiskSource, decimal pQuantity, bool pIsPositionOption)
        {
            PrismaRmsSubSamplesRisk posSubSampleRisk = default;
            if ((pSubSambleRiskSource != default)
                && (pSubSambleRiskSource.m_SubSamplesRisk != default))
            {
                PrismaSubSampleRisk[] posSubSamples = (
                    from subSample in pSubSambleRiskSource.m_SubSamplesRisk
                    select PrismaSubSampleRisk.ApplyPosition(subSample, pQuantity, pIsPositionOption)
                    ).ToArray();

                posSubSampleRisk = new PrismaRmsSubSamplesRisk(pSubSambleRiskSource.m_RmsParameters, posSubSamples);
            }
            return posSubSampleRisk;
        }
        #endregion
        #region IEnumerable<PrismaRmsSubSamplesRisk> AddScenarios
        /// <summary>
        /// Ajoute les scenarios de deux ensembles de Sub-Samples (en fonction de leur Rms)
        /// </summary>
        /// <param name="pSampleOne"></param>
        /// <param name="pSampleTwo"></param>
        /// <returns></returns>
        public static IEnumerable<PrismaRmsSubSamplesRisk> AddScenarios(IEnumerable<PrismaRmsSubSamplesRisk> pSampleOne, IEnumerable<PrismaRmsSubSamplesRisk> pSampleTwo)
        {
            IEnumerable<PrismaRmsSubSamplesRisk> newSubSample = pSampleOne;
            if (newSubSample == default(IEnumerable<PrismaRmsSubSamplesRisk>))
            {
                newSubSample = pSampleTwo;
            }
            else if ((pSampleOne != default(IEnumerable<PrismaRmsSubSamplesRisk>)) && (pSampleTwo != default(IEnumerable<PrismaRmsSubSamplesRisk>)))
            {
                newSubSample =
                    from s1 in pSampleOne
                    join s2 in pSampleTwo on s1.m_RmsParameters.IdRmsLgs equals s2.m_RmsParameters.IdRmsLgs
                    select AddScenarios(s1, s2);
            }
            return newSubSample;
        }
        /// <summary>
        /// Ajoute deux sub-samples de même Lgs et Rms entre-eux
        /// </summary>
        /// <param name="pSampleOne"></param>
        /// <param name="pSampleTwo"></param>
        /// <returns></returns>
        public static PrismaRmsSubSamplesRisk AddScenarios(PrismaRmsSubSamplesRisk pSampleOne, PrismaRmsSubSamplesRisk pSampleTwo)
        {
            PrismaRmsSubSamplesRisk aggregat = default;
            if ((pSampleOne != default) && (pSampleTwo != default)
                && (pSampleOne.m_RmsParameters != default) && (pSampleTwo.m_RmsParameters != default)
                && (pSampleOne.m_SubSamplesRisk != default) && (pSampleTwo.m_SubSamplesRisk != default)
                && (pSampleOne.m_RmsParameters.IdRmsLgs == pSampleTwo.m_RmsParameters.IdRmsLgs))
            {
                int nbSubSampleMin = System.Math.Min(pSampleOne.m_SubSamplesRisk.Length, pSampleTwo.m_SubSamplesRisk.Length);
                int nbSubSampleMax = System.Math.Max(pSampleOne.m_SubSamplesRisk.Length, pSampleTwo.m_SubSamplesRisk.Length);
                PrismaSubSampleRisk[] subSamples = new PrismaSubSampleRisk[nbSubSampleMax];
                // Ajout des scénarios des 2 Sub-Sample entre-eux
                for (int i = 0; i < nbSubSampleMin; i += 1)
                {
                    subSamples[i] = PrismaSubSampleRisk.AddScenarios(pSampleOne.m_SubSamplesRisk[i], pSampleTwo.m_SubSamplesRisk[i]);
                }
                // Si jamais il n'y avait pas le même nombre de sub-sample
                if (nbSubSampleMin != nbSubSampleMax)
                {
                    PrismaRmsSubSamplesRisk maxSubSampleRisk = (pSampleOne.m_SubSamplesRisk.Length > nbSubSampleMin) ? pSampleOne : pSampleTwo;
                    for (int i = nbSubSampleMin; i < nbSubSampleMax; i += 1)
                    {
                        subSamples[i] = maxSubSampleRisk.m_SubSamplesRisk[i];
                    }
                }
                aggregat = new PrismaRmsSubSamplesRisk(pSampleOne.m_RmsParameters, subSamples);
            }
            return aggregat;
        }
        #endregion
        /// <summary>
        /// Calcul les valeurs de risque du Rms
        /// <para>Step6, Sub-Step2</para>
        /// </summary>
        public decimal EvaluateMarketRiskComponent()
        {
            if ((m_RmsParameters != default) && (m_SubSamplesRisk != default))
            {
                foreach (PrismaSubSampleRisk subSample in m_SubSamplesRisk)
                {
                    subSample.EvaluateMarketRiskComponent();
                }
                IEnumerable<decimal> subMarketRiskComponent = m_SubSamplesRisk.Select(s => s.MarketRiskComponent);
                m_MarketRiskComponent = PrismaMethod.Aggregate(subMarketRiskComponent, AggregationMethod);
            }
            return m_MarketRiskComponent;
        }

        /// <summary>
        /// Calcul du VaR(Value at Risk) selon les spécifications propres au calcul du Liquidity Risk Component
        /// </summary>
        public decimal EvaluateVaRLiquidityRiskComponent()
        {
            if ((m_RmsParameters != default) && (m_SubSamplesRisk != default))
            {
                foreach (PrismaSubSampleRisk subSample in m_SubSamplesRisk)
                {
                    subSample.EvaluateVaRLiquidityRisk();
                }
                IEnumerable<decimal> lstValue = m_SubSamplesRisk.Select(s => s.ValueAtRiskLiquidityRiskComponent);
                m_ValueAtRiskLiquidityRiskComponent = PrismaMethod.Aggregate(lstValue, AggregationMethod);
            }
            return m_ValueAtRiskLiquidityRiskComponent;
        }
        #endregion
    }

    /// <summary>
    /// Classe de gestion des données de risque de la position sur un asset
    /// </summary>
    internal sealed class PrismaPositionRisk
    {
        #region members
        // --------
        // Position
        // --------
        /// <summary>
        /// Représente la position vis à vis de l'asset
        /// </summary>
        private readonly Pair<PosRiskMarginKey, RiskMarginPosition> m_Position = default;
        // -----------------
        // Données de calcul
        // -----------------
        /// <summary>
        /// Représente un asset (avec ses paramètres de risque)  
        /// </summary>
        private readonly PrismaAssetRisk m_AssetRisk = default;
        /// <summary>
        /// 
        /// </summary>
        private List<PrismaRmsSubSamplesRisk> m_RmsSubSamples = default;
        #endregion

        #region accessors
        /// <summary>
        ///  Représente les données de risque d'un asset
        /// </summary>
        public PrismaAssetRisk AssetRisk
        {
            get { return m_AssetRisk; }
        }
        /// <summary>
        /// Représente la position vis vis de l'asset 
        /// </summary>
        public Pair<PosRiskMarginKey, RiskMarginPosition> Position
        {
            get { return m_Position; }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<PrismaRmsSubSamplesRisk> RmsSubSamples
        {
            get { return m_RmsSubSamples; }
        }
        /// <summary>
        /// Premium Margin
        /// </summary>
        // PM 20140618 [19911] New
        public decimal PremiumMargin
        {
            get
            {
                decimal premiumMargin = 0m;
                // Uniquement pour les positions Options avec primes
                if ((m_AssetRisk != default)
                    && (m_Position != default))
                {
                    // Négatif pour Long ("1"), Positif pour Short ("2")
                    premiumMargin = (m_Position.First.Side == SideTools.RetBuyFIXmlSide() ? -1 : 1) * PrismaMethod.PrismaRoundPremium(m_AssetRisk.PremiumMargin * m_Position.Second.Quantity);
                }
                return premiumMargin;
            }
        }
        /// <summary>
        /// Present Value
        /// </summary>
        // PM 20140618 [19911] New
        public decimal PresentValue
        {
            get
            {
                decimal presentValue = 0m;
                // Uniquement pour les positions Options avec primes
                if ((m_AssetRisk != default)
                    && (m_Position != default))
                {
                    // Positif pour Long ("1"), Négatif pour Short ("2")
                    presentValue = (m_Position.First.Side == SideTools.RetBuyFIXmlSide() ? 1 : -1) * m_AssetRisk.PresentValue * m_Position.Second.Quantity;
                }
                return presentValue;
            }
        }

        #endregion
        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAssetRisk">Représente l'asset avec ces paramètres</param>
        /// <param name="pPosition">Représente la position pour l'asset</param>
        public PrismaPositionRisk(PrismaAssetRisk pAssetRisk, Pair<PosRiskMarginKey, RiskMarginPosition> pPosition)
        {
            m_AssetRisk = pAssetRisk;
            m_Position = pPosition;
        }
        #endregion
        #region methods
        /// <summary>
        /// Calcul des Sub-Samples de la position sur chaque Rms de l'asset
        /// </summary>
        public List<PrismaRmsSubSamplesRisk> CreateSubSamples()
        {
            m_RmsSubSamples = new List<PrismaRmsSubSamplesRisk>();
            if ((m_AssetRisk != default) && (m_Position != default)
                && (m_AssetRisk.SubSamples != default))
            {
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                decimal position = (m_Position.First.Side == SideTools.RetBuyFIXmlSide() ? 1 : -1) * m_Position.Second.Quantity;
                // PM 20200826 [25467] Ajout indicateur de position option
                bool isPositionOption = m_AssetRisk.AssetParameters.PutOrCall.HasValue;

                // Application de la quantité en position aux prix des sub samples
                m_RmsSubSamples = (
                    from subSample in m_AssetRisk.SubSamples
                    select subSample.ApplyPosition(position, isPositionOption)
                    ).ToList();
            }
            return m_RmsSubSamples;
        }
        #endregion
    }

    /// <summary>
    /// Classe de gestion des données de risque de la position sur les Assets d'un LGS
    /// </summary>
    internal sealed class PrismaPositionLgsRisk
    {
        #region members
        // ----------------
        // Paramètres Eurex
        // ----------------
        private PrismaLiquidationGroupSplit m_LgsParameters = default;
        // PM 20180903 [24015] Prisma v8.0 : add m_TEAParameters
        private IEnumerable<PrismaTimeToExpiryAdjustment> m_TEAParameters = default;
        // -----------------
        // Données de calcul
        // -----------------
        /// <summary>
        /// Représente les positions
        /// </summary>
        private readonly PrismaPositionRisk[] m_PositionRisk = default;
        /// <summary>
        /// Liste des montants des Rms par devise
        /// <para>En réalité il n'y a qu'une devise dans LG et à fortiori dans un LGS et dans les RMS du LGS</para>
        /// <para>FI/PM décident de laisser en état, cette liste ne contiendra qu'un seul item</para>
        /// </summary>
        private Dictionary<string, List<PrismaRmsSubSamplesRisk>> m_RmsRiskCur = default;
        //
        private List<Money> m_MarketRisk = default;
        // PM 20180903 [24015] Prisma v8.0 : add m_TimeToExpiryAdjustmentRisk
        private PrismaTimeToExpiryAdjustmentRisk  m_TimeToExpiryAdjustmentRisk = default;
        private readonly List<Money> m_LiquidityRisk = default;
        private List<Money> m_InitialMargin = default;
        //PM 20140616 [19911] Ajout m_PremiumMargin, m_LongOptionCredit et m_TotalInitialMargin
        private readonly List<Money> m_PremiumMargin = default;
        private readonly List<Money> m_LongOptionCredit = default;
        private readonly List<Money> m_TotalInitialMargin = default;
        //PM 20150907 [21236] Ajout m_MarginRequirement
        private readonly List<Money> m_MarginRequirement = default;

        // PM 20200826 [25467] Ajout détail du calcul Long Option Credit: Present Value et Maximal Lost
        private readonly List<Money> m_PresentValue = default;
        private readonly List<Money> m_MaximalLost = default;

        /// <summary>
        /// Liste avec certains résultats intermédiaires évalués pendant l'évaluation de la Liquidity Risk
        /// <para>La clé du dictionnaire est l'IdAsset</para>
        /// </summary>
        private Dictionary<int, PrismaAssetLiquidityRisk> m_assetLiquidityRisk = default;
        #endregion

        #region accessors
        #region accessors de PrismaLiquidationGroupSplit m_LgsParameters
        /// <summary>
        /// Obtient ou définit le Liquidation Group Split parameters
        /// </summary>
        public PrismaLiquidationGroupSplit LgsParameters
        {
            set { m_LgsParameters = value; }
            get { return m_LgsParameters; }
        }
        /// <summary>
        /// Obtient ou définit les Time To Expiry Adjustement parameters du Liquidation Group Split
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : new accessor TEAParameters
        public IEnumerable<PrismaTimeToExpiryAdjustment> TEAParameters
        {
            set
            {
                if ((value != default) && (IdLgs != 0))
                {
                    m_TEAParameters =
                        from TEAParams in value
                        where TEAParams.IdLgs == m_LgsParameters.IdLgs
                        select TEAParams;
                }
            }
            get { return m_TEAParameters; }
        }
        /// <summary>
        /// Obtient Id interne du split de groupe de liquidation
        /// (Requiert PrismaLiquidationGroupSplit parameter)
        /// </summary>
        public int IdLgs
        {
            get { return (m_LgsParameters != default) ? m_LgsParameters.IdLgs : 0; }
        }
        /// <summary>
        /// Obtient Liquidation Group Split Identifier
        /// (Requiert PrismaLiquidationGroupSplit parameter)
        /// </summary>
        public string LiquidationGroupSplit
        {
            get { return (m_LgsParameters != default) ? m_LgsParameters.Identifier : Cst.NotFound; }
        }
        /// <summary>
        /// Méthode d'aggregation
        /// <para>(Requiert PrismaLiquidationGroupSplit parameter)</para>
        /// </summary>
        public PrismaAggregationMethod GroupSplitAggregationMethod
        {
            get { return (m_LgsParameters != default) ? m_LgsParameters.AggregationMethod : PrismaAggregationMethod.NA; }
        }
        /// <summary>
        /// Identifier de la méthode risque
        /// <para>(Requiert PrismaLiquidationGroupSplit parameter)</para>
        /// </summary>
        public string RiskMethod
        {
            get { return (m_LgsParameters != default) ? m_LgsParameters.RiskMethod : default; }
        }
        #endregion
        /// <summary>
        ///  Obtient les positions du LGS
        /// </summary>
        public PrismaPositionRisk[] PositionRisk { get { return m_PositionRisk; } }
        /// <summary>
        ///  Obtiens les rms par devise 
        /// <para>En réalité il n'y a qu'une devise dans LG et à fortiori dans un LGS et dans les RMS du LGS</para>
        /// <para>FI/PM décident de laisser en état, cette liste ne contiendra qu'un seul item</para>
        /// </summary>
        public Dictionary<string, List<PrismaRmsSubSamplesRisk>> RmsRiskCur { get { return m_RmsRiskCur; } }
        /// <summary>
        /// Market Risk
        /// </summary>
        public List<Money> MarketRisk { get { return m_MarketRisk; } }
        /// <summary>
        /// Détail du calcul du Time To Expiry Adjustment
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : add TimeToExpiryAdjustmentRisk
        public PrismaTimeToExpiryAdjustmentRisk TimeToExpiryAdjustmentRisk { get { return m_TimeToExpiryAdjustmentRisk; } }
        /// <summary>
        /// Time To Expiry Adjustment
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : add TimeToExpiryAdjustment
        public List<Money> TimeToExpiryAdjustment
        { get { return (m_TimeToExpiryAdjustmentRisk != default) ? m_TimeToExpiryAdjustmentRisk.TimeToExpiryAdjustment : new List<Money>(); } }
        /// <summary>
        /// Liquidity Risk
        /// </summary>
        public List<Money> LiquidityRisk { get { return m_LiquidityRisk; } }
        /// <summary>
        /// Initial Margin
        /// </summary>
        public List<Money> InitialMargin { get { return m_InitialMargin; } }
        /// <summary>
        /// Premium Margin
        /// </summary>
        //PM 20140619 [19911] New
        public List<Money> PremiumMargin { get { return m_PremiumMargin; } }
        /// <summary>
        /// Long Option Credit
        /// </summary>
        //PM 20140619 [19911] New
        public List<Money> LongOptionCredit { get { return m_LongOptionCredit; } }

        /// <summary>
        /// Valeur Option non marginée
        /// </summary>
        // PM 20200826 [25467] Ajout détail du calcul Long Option Credit
        public List<Money> PresentValue { get { return m_PresentValue; } }

        /// <summary>
        /// Perte Maximal
        /// </summary>
        // PM 20200826 [25467] Ajout détail du calcul Long Option Credit
        public List<Money> MaximalLost { get { return m_MaximalLost; } }

        /// <summary>
        /// Total Initial Margin
        /// </summary>
        //PM 20140619 [19911] New
        public List<Money> TotalInitialMargin { get { return m_TotalInitialMargin; } }
        /// <summary>
        /// Unadjusted Margin Requirement
        /// </summary>
        //PM 20150907 [21236] New MarginRequirement
        public List<Money> MarginRequirement { get { return m_MarginRequirement; } }
        /// <summary>
        /// Obtient une liste avec certains résultats intermédiaires évalués pendant l'évaluation de la Liquidity Risk
        /// <para>La clé du dictionnaire est l'IdAsset</para>
        /// </summary>
        public Dictionary<int, PrismaAssetLiquidityRisk> AssetLiquidityRisk { get { return m_assetLiquidityRisk; } }
        #endregion

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLgsParameters">Représente le LGS</param>
        /// <param name="pPositionRisk">Représente les poistions du LGS</param>
        public PrismaPositionLgsRisk(PrismaLiquidationGroupSplit pLgsParameters, IEnumerable<PrismaPositionRisk> pPositionRisk)
        {
            m_LgsParameters = pLgsParameters;
            m_RmsRiskCur = new Dictionary<string, List<PrismaRmsSubSamplesRisk>>();
            m_MarketRisk = new List<Money>();
            m_LiquidityRisk = new List<Money>();
            m_InitialMargin = new List<Money>();
            if (pPositionRisk != default(IEnumerable<PrismaPositionRisk>))
            {
                m_PositionRisk = pPositionRisk.ToArray();
            }
            else
            {
                m_PositionRisk = new PrismaPositionRisk[0];
            }
            //PM 20140616 [19911] Ajout Premium Margin, Long Option Credit et Total Initial Margin
            m_PremiumMargin = new List<Money>();
            m_LongOptionCredit = new List<Money>();
            m_TotalInitialMargin = new List<Money>();
            //PM 20150907 [21236] Ajout Margin Requirement
            m_MarginRequirement = new List<Money>();
            // PM 20200826 [25467] Ajout détail du calcul Long Option Credit: Present Value et Maximal Lost
            m_PresentValue = new List<Money>();
            m_MaximalLost = new List<Money>();
        }
        #endregion

        #region methods
        /// <summary>
        /// Calcul du Market Risk Component de l'Initial Margin
        /// </summary>
        private List<Money> EvaluateMarketRiskComponent()
        {
            // PM 20180903 [24015] Prisma v8.0 : utilisation d'une nouvelle method SubEvaluateMarketRisk pour partage de code
            //if ((m_PositionRisk != default) && (m_PositionRisk.Count() > 0))
            //{
            //    List<PrismaRmsSubSamplesRisk> rmsSubSamplesList = default;
            //    // Calcul des Profit and Losses sur chaque Sub-Sample de chaque Risk Measure Set
            //    if (m_PositionRisk.Count() == 1)
            //    {
            //        // Le Lgs n'a qu'un seul asset en position
            //        PrismaPositionRisk positionRisk = m_PositionRisk.First();
            //        if (positionRisk.AssetRisk.Currency != default)
            //        {
            //            rmsSubSamplesList = positionRisk.CreateSubSamples();
            //            m_RmsRiskCur.Add(positionRisk.AssetRisk.Currency, rmsSubSamplesList);
            //        }
            //    }
            //    else
            //    {
            //        // A priori un LGS est exprimé en 1 seule devise
            //        // - Soit la devise commune à l'ensemble des assets (cas où les assets ont tous la même devise)
            //        // - Soit la devise de clearing (cas où il existe plusieurs assets de devise différente)
            //        //
            //        // La classe est bâtie pour fonctionner si par hasard plusieurs devises étaient présentes dans un même LGS
            //        // On laisse ainsi puisque cela a été bâti ainsi dès l'origine de développement 
            //        // 
            //
            //        // Regrouper les assets en position par devise (Au vu de la remaque précédente, il ne devrait y avoir qu'1 seul item)
            //        var positionRiskByCurrency =
            //            from pos in m_PositionRisk
            //            where pos.AssetRisk.Currency != default
            //            group pos by pos.AssetRisk.Currency into posByCurrency
            //            select posByCurrency;
            //
            //        // Pour chaque devise
            //        foreach (var posByCurrency in positionRiskByCurrency)
            //        {
            //            // Cumul des sub-samples de chaque position (sub-sample par sub-sample)
            //            rmsSubSamplesList = (
            //                from pos in posByCurrency
            //                select pos.CreateSubSamples()).Aggregate((a, b) => PrismaRmsSubSamplesRisk.AddScenarios(a, b).ToList());
            //            //
            //            m_RmsRiskCur.Add(posByCurrency.Key, rmsSubSamplesList);
            //        }
            //    }
            //    // Pour chaque devise
            //    foreach (var rms in m_RmsRiskCur)
            //    {
            //        List<PrismaRmsSubSamplesRisk> rmsList = rms.Value;
            //        // Calcul du Market Risk Component de chaque Risk Measure Set
            //        foreach (PrismaRmsSubSamplesRisk rmsRisk in rmsList)
            //        {
            //            rmsRisk.EvaluateMarketRiskComponent();
            //        }
            //
            //        // Calcul du Market Risk Component du Liquidation Group Split
            //        // PM 20161019 [22174] Prisma 5.0 : Prendre en compte le Scaled Market Risk pour tous les RMS
            //        ////
            //        //IEnumerable<PrismaRmsSubSamplesRisk> stressedRmsList = rmsList.Where(r => r.HistoricalStressed == PrismaHistoricalStressedEnum.Stressed);
            //        //IEnumerable<decimal> stressedRmsScaledMarketRisk = stressedRmsList.Select(r => r.ScaledMarketRiskComponent);
            //        ////
            //        //IEnumerable<PrismaRmsSubSamplesRisk> otherRmsList = rmsList.Where(r => r.HistoricalStressed != PrismaHistoricalStressedEnum.Stressed);
            //        //IEnumerable<decimal> otherRmsMarketRisk = otherRmsList.Select(r => r.MarketRiskComponent);
            //        ////
            //        //IEnumerable<decimal> rmsMarketRisk = otherRmsMarketRisk.Concat(stressedRmsScaledMarketRisk);
            //        ////
            //        IEnumerable<decimal> rmsMarketRisk = rmsList.Select(r => r.ScaledMarketRiskComponent);
            //        //
            //        decimal marketRisk = PrismaMethod.Aggregate(rmsMarketRisk, GroupSplitAggregationMethod);
            //        //
            //        m_MarketRisk.Add(new Money(marketRisk, rms.Key));
            //    }
            //}
            m_MarketRisk = SubEvaluateMarketRisk(m_PositionRisk, GroupSplitAggregationMethod, out m_RmsRiskCur);
            return m_MarketRisk;
        }

        /// <summary>
        /// Calcul du Market Risk pour une position
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : new method SubEvaluateMarketRisk
        public static List<Money> SubEvaluateMarketRisk(IEnumerable<PrismaPositionRisk> pPositionRisk, PrismaAggregationMethod pGroupSplitAggregationMethod, out Dictionary<string, List<PrismaRmsSubSamplesRisk>> pRmsRiskCur)
        {
            List<Money> marketRisk = new List<Money>();
            pRmsRiskCur = new Dictionary<string, List<PrismaRmsSubSamplesRisk>>();
            if ((pPositionRisk != default(IEnumerable<PrismaPositionRisk>)) && (pPositionRisk.Count() > 0) && (pGroupSplitAggregationMethod != default))
            {
                List<PrismaRmsSubSamplesRisk> rmsSubSamplesList = default;
                // Calcul des Profit and Losses sur chaque Sub-Sample de chaque Risk Measure Set
                if (pPositionRisk.Count() == 1)
                {
                    // Le Lgs n'a qu'un seul asset en position
                    PrismaPositionRisk positionRisk = pPositionRisk.First();
                    if (positionRisk.AssetRisk.Currency != default)
                    {
                        rmsSubSamplesList = positionRisk.CreateSubSamples();
                        pRmsRiskCur.Add(positionRisk.AssetRisk.Currency, rmsSubSamplesList);
                    }
                }
                else
                {
                    // A priori un LGS est exprimé en 1 seule devise
                    // - Soit la devise commune à l'ensemble des assets (cas où les assets ont tous la même devise)
                    // - Soit la devise de clearing (cas où il existe plusieurs assets de devise différente)
                    //
                    // La classe est bâtie pour fonctionner si par hasard plusieurs devises étaient présentes dans un même LGS
                    // On laisse ainsi puisque cela a été bâti ainsi dès l'origine de développement 
                    // 

                    // Regrouper les assets en position par devise (Au vu de la remaque précédente, il ne devrait y avoir qu'1 seul item)
                    var positionRiskByCurrency =
                        from pos in pPositionRisk
                        where pos.AssetRisk.Currency != default
                        group pos by pos.AssetRisk.Currency into posByCurrency
                        select posByCurrency;

                    // Pour chaque devise
                    foreach (var posByCurrency in positionRiskByCurrency)
                    {
                        // Cumul des sub-samples de chaque position (sub-sample par sub-sample)
                        rmsSubSamplesList = (
                            from pos in posByCurrency
                            select pos.CreateSubSamples()).Aggregate((a, b) => PrismaRmsSubSamplesRisk.AddScenarios(a, b).ToList());
                        //
                        pRmsRiskCur.Add(posByCurrency.Key, rmsSubSamplesList);
                    }
                }
                // Pour chaque devise
                foreach (var rms in pRmsRiskCur)
                {
                    List<PrismaRmsSubSamplesRisk> rmsList = rms.Value;
                    // Calcul du Market Risk Component de chaque Risk Measure Set
                    foreach (PrismaRmsSubSamplesRisk rmsRisk in rmsList)
                    {
                        rmsRisk.EvaluateMarketRiskComponent();
                    }

                    // Calcul du Market Risk Component du Liquidation Group Split
                    // Prendre en compte le Scaled Market Risk pour tous les RMS
                    IEnumerable<decimal> rmsMarketRisk = rmsList.Select(r => r.ScaledMarketRiskComponent);
                    //
                    decimal risk = PrismaMethod.Aggregate(rmsMarketRisk, pGroupSplitAggregationMethod);
                    //
                    marketRisk.Add(new Money(risk, rms.Key));
                }
            }
            return marketRisk;
        }

        /// <summary>
        /// Calcul du Time to Expiry Adjustment Component de l'Initial Margin
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : new method EvaluateTimeToExpiryAdjustment
        private List<Money> EvaluateTimeToExpiryAdjustment()
        {
            m_TimeToExpiryAdjustmentRisk = new PrismaTimeToExpiryAdjustmentRisk(m_TEAParameters, GroupSplitAggregationMethod, m_PositionRisk, m_MarketRisk);
            m_TimeToExpiryAdjustmentRisk.CalcTimeToExpiryAdjustment();
            return m_TimeToExpiryAdjustmentRisk.TimeToExpiryAdjustment;
        }

        /// <summary>
        /// Calcul du Liquidity Risk Component de l'Initial Margin
        /// </summary>
        private List<Money> EvaluateLiquidityRiskComponent()
        {
            if ((m_PositionRisk != default) && (m_PositionRisk.Count() > 0))
            {
                InitAssetLiquidityRisk();

                Dictionary<int, decimal> netGrossRation = CalcPositionNetGrossRatio();

                Dictionary<int, decimal> effectiveRelativePositionSize = CalcEffectiveRelativePositionSize(netGrossRation);

                Dictionary<int, decimal> marketRiskMeasure = CalcMarketRiskMeasure();

                Dictionary<int, decimal> liquidityAdjustement = CalcLiquidityAdjustment(netGrossRation, effectiveRelativePositionSize, marketRiskMeasure);

                Dictionary<int, decimal> additionalMarketRiskMeasure = CalcAdditionalMarketRiskMeasure();

                foreach (var rmsItem in m_RmsRiskCur)
                {
                    string cur = rmsItem.Key;
                    List<PrismaRmsSubSamplesRisk> rmsList = rmsItem.Value.ToList();

                    var rmsLC = rmsList.Where(rms => rms.RmsParameters.IsLiquidityComponent == true).FirstOrDefault();
                    if (rmsLC != default)
                    {
                        decimal diversifiedVaR = rmsLC.EvaluateVaRLiquidityRiskComponent();

                        decimal sumAggregatedAdditionalMarketRisk =
                            (from pos in m_PositionRisk.Where(mm => mm.AssetRisk.Currency == cur)
                             join addMarketRisk in additionalMarketRiskMeasure on pos.AssetRisk.IdAsset equals addMarketRisk.Key
                             select addMarketRisk).Sum(s => s.Value);

                        //Step5, sub-step10
                        //Calculation of the alpha factor
                        decimal AlphaFactor = 0;
                        if (sumAggregatedAdditionalMarketRisk != 0)
                        {
                            AlphaFactor = System.Math.Max(diversifiedVaR / sumAggregatedAdditionalMarketRisk, rmsLC.RmsParameters.AlphaFloor);
                        }
                        else
                        {
                            AlphaFactor = rmsLC.RmsParameters.AlphaFloor;
                        }
                        rmsLC.AlphaFactor = AlphaFactor;

                        //Step5, sub-step11
                        //Calculation of the liquidity adjustment
                        var liquidityRiskComponentCur =
                            (from pos in m_PositionRisk.Where(mm => mm.AssetRisk.Currency == cur)
                             join liqAdj in liquidityAdjustement on pos.AssetRisk.IdAsset equals liqAdj.Key
                             select liqAdj).Sum(s => System.Math.Abs(s.Value)) * AlphaFactor;

                        m_LiquidityRisk.Add(new Money(liquidityRiskComponentCur, cur));
                    }
                }
            }
            return m_LiquidityRisk;
        }

        /// <summary>
        /// Calcul du Long Option Credit
        /// Step 7
        /// </summary>
        /// <param name="pPrismaRelease"></param>
        /// <returns></returns>
        //PM 20140618 [19911] New
        // PM 20180319 [XXXXX] Ajout paramètre pPrismaRelease pour utilisation de MethodVersion à la place de PrismaRelease
        //private List<Money> EvaluateLongOptionCredit()
        // PM 20200826 [25467] Ajout détail du calcul Long Option Credit et correction calcul Maximal Lost
        private List<Money> EvaluateLongOptionCredit(decimal pPrismaRelease)
        {
            if ((m_PositionRisk != default) && (m_PositionRisk.Count() > 0))
            {
                foreach (var rmsItem in m_RmsRiskCur)
                {
                    string cur = rmsItem.Key;
                    decimal presentValue = 0m;
                    decimal maximalLost = 0m;
                    decimal longOptionCredit = 0m;
                    decimal totalInitialMargin = 0m;
                    decimal marginRequirement = 0m;
                    decimal initialMargin = m_InitialMargin.Sum(m => (m.Currency == cur) ? m.Amount.DecValue : 0m);
                    // Calcul de la Premium Margin
                    decimal premiumMargin = m_PositionRisk.Sum(pr => (pr.AssetRisk.Currency == cur) ? pr.PremiumMargin : 0);
                    //
                    // PM 20150415 [20957] Round Premium to 1 decimal
                    premiumMargin = PrismaMethod.PrismaRoundPremium(premiumMargin);
                    //
                    m_PremiumMargin.Add(new Money(premiumMargin, cur));
                    // Critère 1 : Premium margin credit => Sum of premium margin < 0
                    if (premiumMargin < 0)
                    {
                        decimal initialMarginToCheck;
                        // PM 20150415 [20957] Round Intital Margin to 2 decimal
                        // PM 20180319 [XXXXX] Ajout paramètre pPrismaRelease pour utilisation de MethodVersion à la place de PrismaRelease
                        //if (Settings.Default.PrismaRelease > 2)
                        if (pPrismaRelease > 2)
                        {
                            initialMarginToCheck = PrismaMethod.PrismaRoundAmount(initialMargin);
                        }
                        else
                        {
                            initialMarginToCheck = initialMargin;
                        }
                        // Critère 2 : Initial margin exceeds the premium credit
                        if ((premiumMargin + initialMarginToCheck) > 0)
                        {
                            // Calcul de la Present Value
                            presentValue = m_PositionRisk.Sum(pr => (pr.AssetRisk.Currency == cur) ? pr.PresentValue : 0);
                            // PM 20150415 [20957] Round Present Value to 2 decimal
                            // PM 20180319 [XXXXX] Ajout paramètre pPrismaRelease pour utilisation de MethodVersion à la place de PrismaRelease
                            //if (Settings.Default.PrismaRelease > 2)
                            if (pPrismaRelease > 2)
                            {
                                presentValue = PrismaMethod.PrismaRoundAmount(presentValue);
                            }
                            // Recherche de la perte maximal
                            List<decimal> profitAndLost = new List<decimal>();
                            List<PrismaRmsSubSamplesRisk> rmsList = rmsItem.Value.ToList();

                            foreach (PrismaRmsSubSamplesRisk rms in rmsList)
                            {
                                foreach (PrismaSubSampleRisk subSample in rms.SubSamplesRisk)
                                {
                                    // PM 20200826 [25467] Recherche de la perte maximal uniquement sur les valeurs de risque des options en premium style
                                    //profitAndLost = profitAndLost.Concat(subSample.OptionScenarios.Values.ToList()).ToList();
                                    // PM 20220524 [26038] Contrairement à ce qui a été fait pour le ticket 25467, la perte maximale doit être recherchée sur l'ensemble de la position du group split (voir ticket 34616 pour confirmation d'Eurex)
                                    profitAndLost = profitAndLost.Concat(subSample.Scenarios.Values.ToList()).ToList();
                                }
                            }
                            maximalLost = profitAndLost.Min();

                            maximalLost = -1 * System.Math.Min(0, maximalLost);
                            // PM 20150415 [20957] Round Maximal Lost to 2 decimal
                            // PM 20180319 [XXXXX] Ajout paramètre pPrismaRelease pour utilisation de MethodVersion à la place de PrismaRelease
                            //if (Settings.Default.PrismaRelease > 2)
                            if (pPrismaRelease > 2)
                            {
                                maximalLost = PrismaMethod.PrismaRoundAmount(maximalLost);
                            }
                            //
                            // Critère 3 : Present value is greater than Maximal lost
                            // PM 20151013 [21446] Correction du test: "supérieur ou égal"
                            //if (presentValue > maximalLost)
                            if (presentValue >= maximalLost)
                            {
                                longOptionCredit = System.Math.Abs(premiumMargin) - initialMargin;
                                //m_LongOptionCredit.Add(new Money(longOptionCredit, cur));
                            }
                        }
                    }
                    m_PresentValue.Add(new Money(presentValue, cur));
                    m_MaximalLost.Add(new Money(maximalLost, cur));

                    // PM 20151013 [21446] Toujours alimenter le LongOptionCredit même s'il est de 0
                    m_LongOptionCredit.Add(new Money(longOptionCredit, cur));
                    //
                    totalInitialMargin = initialMargin + longOptionCredit;
                    m_TotalInitialMargin.Add(new Money(totalInitialMargin, cur));
                    // PM 20150907 [21236] Ajout  calcul Margin Requirement
                    marginRequirement = totalInitialMargin + premiumMargin;
                    m_MarginRequirement.Add(new Money(marginRequirement, cur));
                }
            }
            return m_MarketRisk;
        }

        /// <summary>
        /// Calcul de l'Initial Margin du Liquidation Group Split
        /// </summary>
        /// <param name="pPrismaRelease"></param>
        /// <returns></returns>
        // PM 20180319 [XXXXX] Ajout paramètre pPrismaRelease pour utilisation de MethodVersion à la place de PrismaRelease
        //public List<Money> EvaluateInitialMargin()
        public List<Money> EvaluateInitialMargin(decimal pPrismaRelease)
        {
            if (m_LgsParameters != default)
            {

                // Calculation of Market Risk Components, Correlation Break Adjustment, Compression Model Adjustment
                // Step 2, Step 3, Step 4
                EvaluateMarketRiskComponent();

                // PM 20180903 [24015] Prisma v8.0 : add calculate TEA
                if (LgsParameters.IsCalculateTEA)
                {
                    // Calculation of the Time to Expiry Adjustment
                    // Step 5
                    EvaluateTimeToExpiryAdjustment();
                }

                // Calculation of Liquidity Risk Adjustment
                // Step 6
                EvaluateLiquidityRiskComponent();

                // Cumul par devise
                // Calculation of the Initial Margin Figures
                // Step 7
                IEnumerable<Money> allAmounts = m_MarketRisk.Concat(m_LiquidityRisk);
                // PM 20180903 [24015] Prisma v8.0 : Ajout du Time To Expiry Adjustment à tous les montants à cumuler
                allAmounts = allAmounts.Concat(TimeToExpiryAdjustment);
                //
                m_InitialMargin = (
                    from amount in allAmounts
                    group amount by amount.Currency into amountByCurrency
                    //PM 20140619 [19911] Ajout arrondi
                    //select new Money(amountByCurrency.Sum(m => m.Amount.DecValue), amountByCurrency.Key)
                    //PM 20150415 [20957] L'arrondi est réalisé plus tard
                    //select new Money(amountByCurrency.Sum(m => PrismaMethod.PrismaRoundAmount(m.Amount.DecValue)), amountByCurrency.Key)
                    select new Money(amountByCurrency.Sum(m => m.Amount.DecValue), amountByCurrency.Key)
                    ).ToList();

                // PM 20180319 [XXXXX] Ajout paramètre pPrismaRelease pour utilisation de MethodVersion à la place de PrismaRelease
                //EvaluateLongOptionCredit();
                // Calculation of Long Option Credit
                // Step 8
                EvaluateLongOptionCredit(pPrismaRelease);
            }
            //PM 20150416 [20957] Retourner le m_TotalInitialMargin plutôt que le m_InitialMargin
            //return m_InitialMargin;
            //PM 20150907 [21236] Retourner m_MarginRequirement au lieu de m_TotalInitialMargin
            //return m_TotalInitialMargin;
            return m_MarginRequirement;
        }

        /// <summary>
        /// Calculation of the net gross ration for each risk buckets
        /// <para>la clé du dictionnaire contient le risk buckets et la valeur le netGrossRatio</para>
        /// <para>Step5, sub-step3</para>
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, decimal> CalcNetGrossRatio()
        {
            var riskBucketQuantities =
                from pos in m_PositionRisk
                where pos.AssetRisk.RiskBucket != default
                group pos by pos.AssetRisk.RiskBucket into riskBucketGroup
                select new
                {
                    key = riskBucketGroup.Key,
                    netQuantity = riskBucketGroup.Sum(p => (p.Position.First.Side == "1") ?
                                                    p.Position.Second.Quantity : (-1) * p.Position.Second.Quantity),
                    sumQuantity = riskBucketGroup.Sum(p => p.Position.Second.Quantity),
                };

            bool isNGREnabled = BoolFunc.IsTrue(SystemSettings.GetAppSettings("MREurexPrismaNetGrossRatioEnabled", "false"));

            Dictionary<string, decimal> ret =
                (from item in riskBucketQuantities
                 select
                     new
                     {
                         riskBucket = item.key,
                         // RD 20211214 [34593] Set Net-Gross ratio (ngr) equal to 1
                         // Page 30 of document "Eurex Clearing Prisma User Guide Methodology Description (Release 10.2).pdf":
                         // Net-Gross ratio (ngr) has been decommissioned. As it is still part of the system and reports, its value 
                         // has to be set equal to 1 across all instruments and risk buckets.
                         //
                         //netgrossRatio = (item.sumQuantity == 0) ? 0 : (decimal)System.Math.Abs(item.netQuantity) / (decimal)item.sumQuantity
                         //netgrossRatio = (decimal)1

                         // PL 20211224 Add isNGREnabled 
                         netgrossRatio = (item.sumQuantity == 0) ? 0 : (isNGREnabled ? (decimal)System.Math.Abs(item.netQuantity) / (decimal)item.sumQuantity : (decimal)1)
                     }
                 ).ToDictionary(mc => mc.riskBucket, mc => mc.netgrossRatio);

            return ret;
        }

        /// <summary>
        /// Calculate the net gross ration for each position 
        /// <para>la clé du dictionnaire contient l'asset de la position et la valeur le netGrossRatio</para>
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, decimal> CalcPositionNetGrossRatio()
        {
            Dictionary<string, decimal> riskBucketsNGR = CalcNetGrossRatio();

            var ngrPos =
            from pos in m_PositionRisk
            join ngr in riskBucketsNGR on pos.AssetRisk.RiskBucket equals ngr.Key
            select
                     new
                     {
                         idAsset = pos.AssetRisk.IdAsset,
                         netgrossRatio = ngr.Value
                     };

            Dictionary<int, decimal> dicNgrPos =
            ngrPos.ToDictionary(mc => mc.idAsset, mc => mc.netgrossRatio);

            foreach (int item in dicNgrPos.Keys)
            {
                if (m_assetLiquidityRisk.ContainsKey(item))
                    m_assetLiquidityRisk[item].netGrossRatio = dicNgrPos[item];
            }

            return dicNgrPos;
        }

        /// <summary>
        /// Return effective relative position size for each position
        /// <para>calculate the net effective relative position size by multiplying the relative poistion size of a position with the net gross ratio</para>
        /// <para>la clé du dictionnaire contient l'idAsset de la position et la valeur le "effective relative position size"</para>
        /// <para>Step5, sub-step4 and sub-step5</para>
        /// </summary>
        /// <param name="pNetGrossRatio">Représente le "net gross ratio" de chaque position</param>
        /// <returns></returns>
        private Dictionary<int, decimal> CalcEffectiveRelativePositionSize(Dictionary<int, decimal> pNetGrossRatio)
        {

            Dictionary<int, decimal> relativePositionSize = CalcRelativePositionSize();

            var effectiveRelativePositionSize =
                from pos in m_PositionRisk
                join rlposSize in relativePositionSize on pos.AssetRisk.IdAsset equals rlposSize.Key
                join ngr in pNetGrossRatio on pos.AssetRisk.IdAsset equals ngr.Key
                select new
                {
                    idAsset = pos.AssetRisk.IdAsset,
                    effectiveRelativePositionSize = rlposSize.Value
                                                    *
                                                    ngr.Value
                };

            Dictionary<int, decimal>
                dicEffectiveRelativePositionSize = effectiveRelativePositionSize.ToDictionary(mc => mc.idAsset, mc => mc.effectiveRelativePositionSize);

            return dicEffectiveRelativePositionSize;
        }

        /// <summary>
        /// Return relative position size for each position
        /// <para>la clé du dictionnaire contient l'idAsset de la position et la valeur le "effective relative position size"</para>
        /// <para>Step5, sub-step4</para>
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, decimal> CalcRelativePositionSize()
        {
            var relativePositionSize =
                from pos in m_PositionRisk
                select new
                {
                    idAsset = pos.AssetRisk.IdAsset,
                    // PM 20191127 [25098] Ajout test afin d'éviter une possible division par 0
                    //relativePositionSize = (decimal)(pos.Position.Second.Quantity) /pos.AssetRisk.MarketCapacity.MarketCapacity
                    relativePositionSize = (pos.AssetRisk.MarketCapacity.MarketCapacity != 0) ? (decimal)(pos.Position.Second.Quantity) / pos.AssetRisk.MarketCapacity.MarketCapacity : 0
                };

            Dictionary<int, decimal>
                dicRelativePositionSize = relativePositionSize.ToDictionary(mc => mc.idAsset, mc => mc.relativePositionSize);

            return dicRelativePositionSize;
        }

        /// <summary>
        /// Calculation market risk figure
        /// <para>la clé du dictionnaire contient l'idAsset de la position et la valeur le "market risk"</para>
        /// <para>Step5, sub-step6 (sans application du ngr)</para>
        /// <para>Utilisation de IVAR</para>
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, decimal> CalcMarketRiskMeasure()
        {
            Dictionary<int, Pair<PrismaAssetVaR, PrismaAssetVaR>> VaRAsset = GetAssetVAR("IVAR");

            // FI 20140304 Remarques
            // => Dans la doc il y a 
            // "calculate net effective positional VaR figures by multiplication 
            // with the trade unit value (TUV) and the net effective position size taking into account the net gross ratio"
            // Spheres colle au fichier excel qui utilise la qté en position et non le "net effective position size"
            // Voir aussi les notes en bas de page (note 46 en release1 ou note 61 en release2)
            //
            var positionInstrVaR =
                from pos in m_PositionRisk
                join @var in VaRAsset on pos.AssetRisk.IdAsset equals @var.Key
                //join ngr in pNetGrossRatio on pos.AssetRisk.IdAsset equals ngr.Key
                select new
                {
                    idasset = pos.AssetRisk.IdAsset,
                    aggMarketRisk = (
                        // IVAR * POSITION * TUV * NGR
                                     (pos.Position.First.Side == "1" ? @var.Value.First.VaRAmount : @var.Value.Second.VaRAmount)
                                     *
                                     pos.Position.Second.Quantity
                                     *
                                     pos.AssetRisk.Multiplier
                                     *
                                     1
                        //ngr.Value 
                        //Contrairement à la doc 
                        //Spheres® ne multiplie pas par le ngr puisque lorsque le ngr vaut Zéro cela donne Zéro 
                        //Spheres® est ainsi cohérent avec le report CP044
                                     )
                };

            Dictionary<int, decimal> ret = positionInstrVaR.ToDictionary(mc => mc.idasset, mc => mc.aggMarketRisk);


            foreach (int item in ret.Keys)
            {
                if (m_assetLiquidityRisk.ContainsKey(item))
                    m_assetLiquidityRisk[item].riskMeasure = ret[item];
            }

            return ret;
        }

        /// <summary>
        /// Calculate position liquidity risk components
        /// <para>la clé du dictionnaire contient l'idAsset de la position et la valeur le "liquidity Risk Component"</para>
        /// <para>Step5, sub-step8</para>
        /// </summary>
        /// <param name="pNetGrossRatio">Représente le "net gross ratio" de chaque position</param>
        /// <param name="pEffectiveRelativePositionSize">Représente le "net effective relative position" de chaque position</param>
        /// <param name="pMarketRiskMeasure"></param>
        /// <returns></returns>
        private Dictionary<int, decimal> CalcLiquidityAdjustment(Dictionary<int, decimal> pNetGrossRatio,
                                                                    Dictionary<int, decimal> pEffectiveRelativePositionSize,
                                                                    Dictionary<int, decimal> pMarketRiskMeasure)
        {

            Dictionary<int, decimal> liquidityFactor = CalcLiquidityFactor(pEffectiveRelativePositionSize);

            Dictionary<int, decimal> assetLiquidityPremium = GetAssetLiquidityPremium();

            //Calculation market risk figure for net effective positions
            var marketRiskMeasure =
            from pos in m_PositionRisk
            join ngr in pNetGrossRatio on pos.AssetRisk.IdAsset equals ngr.Key
            join mrm in pMarketRiskMeasure on pos.AssetRisk.IdAsset equals mrm.Key
            select
            new
            {
                idAsset = pos.AssetRisk.IdAsset,
                marketRiskMeasure = mrm.Value * ngr.Value
            };
            Dictionary<int, decimal> dicMarketRiskMeasure = marketRiskMeasure.ToDictionary(mr => mr.idAsset, mr => mr.marketRiskMeasure);

            var positionValue =
            from pos in m_PositionRisk
            join ngr in pNetGrossRatio on pos.AssetRisk.IdAsset equals ngr.Key
            select
            new
            {
                idAsset = pos.AssetRisk.IdAsset,
                positionValue = pos.Position.Second.Quantity
                                *
                                ngr.Value
                                *
                                pos.AssetRisk.Multiplier
                                *
                                pos.AssetRisk.NeutralScenarioPrice
            };
            Dictionary<int, decimal> dicPositionValue = positionValue.ToDictionary(mc => mc.idAsset, mc => mc.positionValue);


            var liquidityRiskComponent =
             from positionRisk in m_PositionRisk
             join liqFact in liquidityFactor on positionRisk.AssetRisk.IdAsset equals liqFact.Key
             join mrm in dicMarketRiskMeasure on positionRisk.AssetRisk.IdAsset equals mrm.Key
             join liqPremium in assetLiquidityPremium on positionRisk.AssetRisk.IdAsset equals liqPremium.Key
             join posValue in dicPositionValue on positionRisk.AssetRisk.IdAsset equals posValue.Key
             select
             new
             {
                 idAsset = liqFact.Key,
                 liquidityRiskComponent = (liqFact.Value * mrm.Value)
                                          +
                                          (liqPremium.Value * posValue.Value)
             };
            Dictionary<int, decimal> ret = liquidityRiskComponent.ToDictionary(mc => mc.idAsset, mc => mc.liquidityRiskComponent);


            foreach (int item in ret.Keys)
            {
                if (m_assetLiquidityRisk.ContainsKey(item))
                    m_assetLiquidityRisk[item].liquidityAdjustment = ret[item];
            }

            return ret;
        }

        /// <summary>
        ///  Retourne le résulat de l'interpolation linéaire entre 2 points
        /// </summary>
        /// <param name="value1">Représente l'abcisse et l'ordonnée du point 1</param>
        /// <param name="value2">Représente l'abcisse et l'ordonnée du point 2</param>
        /// <param name="x">Représente une valeur d'abcisse x</param>
        /// <returns></returns>
        private static decimal LinearInperpolation(Pair<Decimal, Decimal> value1, Pair<Decimal, Decimal> value2, Decimal x)
        {
            decimal ret = decimal.Zero;
            // y = (y1 * (x2-x) + y2 * (x-x1)) / (x2-x1)
            // x1 = value1.First, y1 = value1.Second
            // x2 = value2.First, y2 = value2.Second

            if ((value2.First - value1.First) != 0)
            {
                ret = (value1.Second * (value2.First - x)) + (value2.Second * (x - value1.First));
                ret /= (value2.First - value1.First);
            }

            return ret;
        }

        /// <summary>
        ///  Retourne le VAR ou AIVAR des assets en position 
        /// <para>la clé du dictionnaire contient l'asset et les valeurs VAR(Long,Short) ou AIVAR(Long,Short)</para>
        /// </summary>
        /// <param name="pVARTYPE"></param>
        private Dictionary<int, Pair<PrismaAssetVaR, PrismaAssetVaR>> GetAssetVAR(string pVARTYPE)
        {
            switch (pVARTYPE)
            {
                case "IVAR":
                case "AIVAR":
                    break;
                default:
                    throw new ArgumentException(StrFunc.AppendFormat("Value {0} is not valid for parameter pVARTYPE", pVARTYPE));
            }

            var varAsset =
            from positionRisk in m_PositionRisk
            from assetRmsRisk in positionRisk.AssetRisk.AssetRmsRisk
            where (
                // Recherche des vars sur les rms tels que IsLiquidityComponent = true (Release2) ou
                // Recherche des vars sur les rms tels que IsLiquidityComponent = false (Release1)  
                      (assetRmsRisk.IsLiquidityComponent == true & ArrFunc.IsFilled(assetRmsRisk.Vars.ToArray()))
                //PM 20140611 [19911] Passage à Prisma Release v2.0
                //||
                //(assetRmsRisk.IsLiquidityComponent == false & ArrFunc.IsFilled(assetRmsRisk.Vars.ToArray()))
                  )
            orderby assetRmsRisk.IsLiquidityComponent descending
            select new
            {
                idAsset = positionRisk.AssetRisk.IdAsset,
                currency = positionRisk.AssetRisk.Currency,
                varsLong = (from
                       @varItem in assetRmsRisk.Vars
                            where @varItem.Currency == positionRisk.AssetRisk.Currency &
                                  varItem.ShortLong == "L" & varItem.VaRType == pVARTYPE
                            select @varItem).FirstOrDefault(),
                varsShort = (from
                       @varItem in assetRmsRisk.Vars
                             where @varItem.Currency == positionRisk.AssetRisk.Currency &
                             varItem.ShortLong == "S" & varItem.VaRType == pVARTYPE
                             select @varItem).FirstOrDefault(),
            };

            foreach (var item in varAsset)
            {
                if ((item.varsLong == null) || (item.varsLong == null))
                    throw new Exception(StrFunc.AppendFormat("There is no {0} value for asset(id:{1}) and currency(cur:{2})", pVARTYPE, item.idAsset.ToString(), item.currency));
            }

            Dictionary<int, Pair<PrismaAssetVaR, PrismaAssetVaR>> dicVarAsset
            = varAsset.ToDictionary(
                mc => mc.idAsset, mc => new Pair<PrismaAssetVaR, PrismaAssetVaR>(mc.varsLong, mc.varsShort));

            return dicVarAsset;
        }

        /// <summary>
        /// Retourne le "Liquidity Factor" 
        /// <para>Utilisation d'une interpolation linéaire de la matrice des Liquidity Factor</para>
        /// <para>Ce calcul est fonction du "net effective relation Position size"</para>
        /// <para>la clé du dictionnaire contient l'asset de la position et la valeur le LiquidityFactor</para>
        /// </summary>
        /// <param name="pEffectiveRelativePositionSize"></param>
        /// <returns></returns>
        private Dictionary<int, decimal> CalcLiquidityFactor(Dictionary<int, decimal> pEffectiveRelativePositionSize)
        {
            var liquidityFactor =
            from positionRisk in m_PositionRisk
            join erps in pEffectiveRelativePositionSize on positionRisk.AssetRisk.IdAsset equals erps.Key
            from lqd in positionRisk.AssetRisk.LiquidityFactorParameters
            where (
                    (lqd.PctMaxThreshold.HasValue &&
                        lqd.PctMinThreshold <= (System.Math.Abs(erps.Value) * 100) && (System.Math.Abs(erps.Value) * 100) < lqd.PctMaxThreshold)
                    ||
                    ((false == lqd.PctMaxThreshold.HasValue) &&
                        lqd.PctMinThreshold <= (System.Math.Abs(erps.Value) * 100))
                  )
            select
            new
            {
                idAsset = positionRisk.AssetRisk.IdAsset,
                liquidityFactor = (lqd.PctMaxThreshold.HasValue) ?
                                        LinearInperpolation(
                                                      new Pair<Decimal, Decimal>(lqd.PctMinThreshold, lqd.MinThresholdFactor),
                                                      new Pair<Decimal, Decimal>(lqd.PctMaxThreshold.Value, lqd.MaxThresholdFactor.Value), System.Math.Abs(erps.Value) * 100)
                                        : lqd.MinThresholdFactor
            };

            Dictionary<int, decimal> dicLiquidityFactor = liquidityFactor.ToDictionary(mc => mc.idAsset, mc => mc.liquidityFactor);

            foreach (int item in dicLiquidityFactor.Keys)
            {
                if (m_assetLiquidityRisk.ContainsKey(item))
                    m_assetLiquidityRisk[item].liquidityFactor = dicLiquidityFactor[item];
            }

            return dicLiquidityFactor;
        }

        /// <summary>
        /// Retourne les "liquidity premium" des assets en position
        /// <para>la clé du dictionnaire contient l'idAsset et la valeur le "liquidity premium"</para>
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, decimal> GetAssetLiquidityPremium()
        {
            var liquidityPremium =
            from pos in m_PositionRisk
            select
            new
            {
                idAsset = pos.AssetRisk.IdAsset,
                liquidityPremium = pos.AssetRisk.MarketCapacity.LiquidityPremium / 10000
            };

            Dictionary<int, decimal> dicLiquidityPremium = liquidityPremium.ToDictionary(mc => mc.idAsset, mc => mc.liquidityPremium);

            foreach (int item in dicLiquidityPremium.Keys)
            {
                if (m_assetLiquidityRisk.ContainsKey(item))
                    m_assetLiquidityRisk[item].liquidityPremium = dicLiquidityPremium[item];
            }

            return dicLiquidityPremium;
        }
        /// <summary>
        /// <para>la clé du dictionnaire contient l'idAsset de la position et la valeur le "market Risk"</para>
        /// <para>Step5, sub-step9</para>
        /// <para>Utilisation de AIVAR</para>
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, decimal> CalcAdditionalMarketRiskMeasure()
        {
            Dictionary<int, Pair<PrismaAssetVaR, PrismaAssetVaR>> VaRAsset = GetAssetVAR("AIVAR");

            var positionAdditionalInstrVar =
                    from pos in m_PositionRisk
                    join @var in VaRAsset on pos.AssetRisk.IdAsset equals @var.Key
                    select new
                    {
                        idasset = pos.AssetRisk.IdAsset,
                        aggMarketRisk = (
                            // AIVAR * ABS(POSITION) * TUV
                                         (pos.Position.First.Side == "1" ? @var.Value.First.VaRAmount : @var.Value.Second.VaRAmount)
                                         *
                                         pos.Position.Second.Quantity
                                         *
                                         pos.AssetRisk.Multiplier
                                         )
                    };

            Dictionary<int, decimal> ret = positionAdditionalInstrVar.ToDictionary(mc => mc.idasset, mc => mc.aggMarketRisk);

            foreach (int item in ret.Keys)
            {
                if (m_assetLiquidityRisk.ContainsKey(item))
                    m_assetLiquidityRisk[item].additionalRiskMeasure = ret[item];
            }

            return ret;
        }

        /// <summary>
        ///  Nouvelle instance de m_assetLiquidityRisk
        /// </summary>
        private void InitAssetLiquidityRisk()
        {
            m_assetLiquidityRisk = new Dictionary<int, PrismaAssetLiquidityRisk>(m_PositionRisk.Count());
            foreach (PrismaPositionRisk item in m_PositionRisk)
                m_assetLiquidityRisk.Add(item.AssetRisk.IdAsset, new PrismaAssetLiquidityRisk
                {
                    idAsset = item.AssetRisk.IdAsset,
                    tradeUnit = item.AssetRisk.AssetParameters.ContractSize
                });
        }
        #endregion
    }

    /// <summary>
    /// Classe de gestion des données de risque de la position sur les Assets d'un LG
    /// </summary>
    internal sealed class PrismaPositionLgRisk
    {
        #region members
        // ----------------
        // Paramètres Eurex
        // ----------------
        private PrismaLiquidationGroup m_LgParameters = default;
        // -----------------
        // Données de calcul
        // -----------------
        private readonly PrismaPositionLgsRisk[] m_PositionLgsRisk = default;
        private List<Money> m_InitialMargin = default;
        // PM20150907 [] Ajout Premium Margin et Margin Requirement
        private List<Money> m_PremiumMargin = default;
        private List<Money> m_MarginRequirement = default;
        #endregion
        #region accessors
        #region accessors de PrismaLiquidationGroup m_LgParameters
        /// <summary>
        /// Liquidation Group parameters
        /// </summary>
        public PrismaLiquidationGroup LgParameters
        {
            set { m_LgParameters = value; }
            get { return m_LgParameters; }
        }
        /// <summary>
        /// Id interne du groupe de liquidation
        /// (Requiert PrismaLiquidationGroup parameter)
        /// </summary>
        public int IdLg
        {
            get { return (m_LgParameters != default) ? m_LgParameters.IdLg : 0; }
        }
        /// <summary>
        /// Identifier du groupe de liquidation
        /// (Requiert PrismaLiquidationGroup parameter)
        /// </summary>
        public string LiquidationGroup
        {
            get { return (m_LgParameters != default) ? m_LgParameters.Identifier : Cst.NotFound; }
        }
        /// <summary>
        /// Type de devise à utiliser
        /// (Requiert PrismaLiquidationGroup parameter)
        /// </summary>
        public PrismaCurrencyTypeFlagEnum CurrencyTypeFlag
        {
            get { return (m_LgParameters != default) ? m_LgParameters.CurrencyTypeFlag : PrismaCurrencyTypeFlagEnum.NA; }
        }
        #endregion
        /// <summary>
        /// Risk sur la position de chaque Liquidation Group Split
        /// </summary>
        public PrismaPositionLgsRisk[] PositionLgsRisk { get { return m_PositionLgsRisk != default ? m_PositionLgsRisk : new PrismaPositionLgsRisk[0]; } }
        /// <summary>
        /// Initial Margin
        /// </summary>
        public List<Money> InitialMargin { get { return m_InitialMargin; } }
        /// <summary>
        /// Premium Margin
        /// </summary>
        /// PM 20150907 [21236] Add PremiumMargin
        public List<Money> PremiumMargin { get { return m_PremiumMargin; } }
        /// <summary>
        /// Unadjusted Margin Requierement
        /// </summary>
        /// PM 20150907 [21236] Add MarginRequierement
        public List<Money> MarginRequirement { get { return m_MarginRequirement; } }
        #endregion
        #region constructors
        public PrismaPositionLgRisk(PrismaLiquidationGroup pLgParameters, IEnumerable<PrismaPositionLgsRisk> pPositionLgsRisk)
        {
            m_LgParameters = pLgParameters;
            m_InitialMargin = new List<Money>();
            // PM 20150907 [21236] Add m_MarginRequirement
            m_MarginRequirement = new List<Money>();
            if (pPositionLgsRisk != default(IEnumerable<PrismaPositionLgsRisk>))
            {
                m_PositionLgsRisk = pPositionLgsRisk.ToArray();
            }
            else
            {
                m_PositionLgsRisk = new PrismaPositionLgsRisk[0];
            }
        }
        #endregion
        #region methods
        public List<Money> EvaluateInitialMargin()
        {
            if ((m_PositionLgsRisk != default) && (m_PositionLgsRisk.Count() > 0))
            {
                //Cumul Initial Margin
                m_InitialMargin = (
                    from lgs in m_PositionLgsRisk
                    from amount in lgs.TotalInitialMargin
                    group amount by amount.Currency into amountByCurrency
                    select new Money(amountByCurrency.Sum(a => a.Amount.DecValue), amountByCurrency.Key)
                    ).ToList();

                // PM 20150907 [21236] Add m_PremiumMargin
                //Cumul Premium Margin
                m_PremiumMargin = (
                    from lgs in m_PositionLgsRisk
                    from amount in lgs.PremiumMargin
                    group amount by amount.Currency into amountByCurrency
                    select new Money(amountByCurrency.Sum(a => a.Amount.DecValue), amountByCurrency.Key)
                    ).ToList();

                // PM 20150907 [21236] Add m_MarginRequirement
                //Cumul Margin Requirement
                m_MarginRequirement = (
                    from lgs in m_PositionLgsRisk
                    from amount in lgs.MarginRequirement
                    group amount by amount.Currency into amountByCurrency
                    select new Money(amountByCurrency.Sum(a => a.Amount.DecValue), amountByCurrency.Key)
                    ).ToList();

            }
            // PM 20150907 [21236] Retourner m_MarginRequirement au lieu de m_InitialMargin
            //return m_InitialMargin;
            return m_MarginRequirement;
        }
        #endregion
    }

    /// <summary>
    /// Classe de calcul du déposit par la méthode PRISMA
    /// </summary>
    public sealed class PrismaMethod : BaseMethod
    {
        #region members
        #region Referentiel Parameters
        internal IEnumerable<AssetExpandedParameter> m_AssetExpandedParameters = default;
        #endregion Referentiel Parameters
        #region PRISMA Parameters
        private IEnumerable<PrismaLiquidationGroup> m_LiquidationGroupParameters = default;
        private IEnumerable<PrismaLiquidationGroupSplit> m_LiquidationGroupSplitParameters = default;
        // PM 20180903 [24015] Prisma v8.0 : add m_TimeToExpiryAdjusmtentParameters
        private IEnumerable<PrismaTimeToExpiryAdjustment> m_TimeToExpiryAdjusmtentParameters = default;
        private IEnumerable<PrismaRiskMeasureSet> m_RiskMeasureSetParameters = default;
        private IEnumerable<PrismaMarketCapacity> m_MarketCapacityParameters = default;
        /// <summary>
        /// Liste des facteurs de liquidité (toute liquidity Class confondue)
        /// </summary>
        private IEnumerable<PrismaLiquidityFactor> m_LiquidityFactorParameters = default;
        private IEnumerable<PrismaExchangeRate> m_ExchangeRateParameters = default;
        private IEnumerable<PrismaExchangeRateRMS> m_ExchangeRateRMSParameters = default;
        private IEnumerable<PrismaAsset> m_AssetParameters = default;
        private IEnumerable<PrismaAssetLiquidGroupSplit> m_AssetLiquidGroupSplitParameters = default;
        private IEnumerable<PrismaAssetRMSLGS> m_AssetRMSLGSParameters = default;
        private IEnumerable<PrismaAssetPrice> m_AssetPriceParameters = default;
        private IEnumerable<PrismaAssetCompressionError> m_AssetCompressionErrorParameters = default;
        /// <summary>
        /// 
        /// </summary>
        private IEnumerable<PrismaAssetVaR> m_AssetVaRParameters = default;
        #endregion PRISMA Parameters
        #region Empty PRISMA Parameters
        private PrismaAsset m_EmptyAssetParameters = default;
        private PrismaLiquidationGroup m_EmptyLiquidationGroupParameters = default;
        private PrismaLiquidationGroupSplit m_EmptyLiquidationGroupSplitParameters = default;
        private PrismaAssetLiquidGroupSplit m_EmptyAssetLiquidGroupSplitParameters = default;
        /// <summary>
        ///  Retourne une liste vide de facteur de liquidité
        /// </summary>
        /// FI 20140227 add member
        private PrismaLiquidityFactor m_EmptyLiquidityFactor = default;

        /// <summary>
        ///  
        /// </summary>
        /// FI 20140227 add member
        private PrismaMarketCapacity m_EmptyMarketCapacity = default;
        #endregion Empty PRISMA Parameters
        #region RBM for Prisma
        // PM 20151116 [21561] Ajout pour RBM
        // PM 20181002 [XXXXX] Désactivation de RBM dans Prisma
        //TimsEUREXMethod TimsMethod = new TimsEUREXMethod();
        #endregion RBM for Prisma
        #endregion members

        #region override base accessors
        /// <summary>
        /// Type de la Methode
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.EUREX_PRISMA; }
        }

        /// <summary>
        /// Requête utilisée pour connaître l'existance de paramètres de risque pour une date donnée
        /// <remarks>Utilise les paramètres DTBUSINESS & SESSIONID</remarks>
        /// </summary>
        /// PM 20150511 [20575] Add QueryExistRiskParameter
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        protected override string QueryExistRiskParameter
        {
            get
            {
                string query;
                query = @"
                    select distinct 1
                      from dbo.IMPRISMAS_H s
                     inner join dbo.IMPRISMAE_H e on (e.IDIMPRISMAE_H = s.IDIMPRISMAE_H)
                     inner join dbo.IMPRISMAP_H p on (p.IDIMPRISMAP_H = e.IDIMPRISMAP_H)
                     inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = p.IDIMPRISMA_H)
                     inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = s.IDASSET)
                     where (m.DTBUSINESS = @DTBUSINESS)";
                return query;
            }
        }
        #endregion

        #region constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        internal PrismaMethod()
        {
            // PM 20170313 [22833] Ajout alimentation de m_RiskMethodDataType
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }
        #endregion Constructor

        #region override base methods
        /// <summary>
        /// Arrondi un montant en utilisant la règle par défaut et la précision donnée.
        /// </summary>
        /// <param name="pAmount">Montant à arrondir</param>
        /// <param name="pPrecision">Precision d'arrondi</param>
        /// <returns>Le montant arrondi, lorsque le chiffre des décimales à arrondir vaut 5, l'arrondie est réalisé en prenant la valeur la plus éloignée de zéro</returns>
        protected override decimal RoundAmount(decimal pAmount, int pPrecision)
        {
            return System.Math.Round(pAmount, pPrecision, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Charge les paramètres spécifiques à la méthode PRISMA.
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pAssetETDCache">Collection d'assets contenant tous les assets en position</param>
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            _ = new Dictionary<string, object>();
            // PM 20150511 [20575] Ajout gestion dtMarket 
            //DateTime dtBusiness = this.DtBusiness.Date;
            DateTime dtBusiness = GetRiskParametersDate(pCS);

            // Pour les paramètres manquants
            m_EmptyAssetParameters = new PrismaAsset { IdAsset = 0, IdLg = 0, IdLiquidClass = 0 };
            m_EmptyLiquidationGroupParameters = new PrismaLiquidationGroup { Identifier = Cst.NotFound, IdLg = 0 };
            m_EmptyLiquidationGroupSplitParameters = new PrismaLiquidationGroupSplit { Identifier = Cst.NotFound, IdLg = 0, IdLgs = 0 };
            m_EmptyAssetLiquidGroupSplitParameters = new PrismaAssetLiquidGroupSplit { IdAsset = 0, IdLgs = 0, IdSerie = 0, IsDefault = true };
            m_EmptyMarketCapacity = new PrismaMarketCapacity { IdMktCapa = 0, ProductId = Cst.NotFound, TimeToExpiryBucketId = Cst.NotFound, MoneynessBucketId = Cst.NotFound };
            m_EmptyLiquidityFactor = new PrismaLiquidityFactor { IdLiquidClass = 0, Identifier = Cst.NotFound };

            // PM 20180219 [23824] Chargement des paramètres de calcul à partir des fichiers
            bool isLoadParametersFromfile = (m_IdIOTaskRiskData != 0);
            //
            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                // ASSETEXPANDED_ALLMETHOD
                m_AssetExpandedParameters = LoadParametersAssetExpanded(connection);

                // PM 20180219 [23824] Deplacement des chargemements des paramètres à partir de la base de données dans une méthode et appel conditionné à cette méthode
                #region code déplacé
                //// Set Parameters : DTBUSINESS
                //dbParametersValue.Add("DTBUSINESS", dtBusiness);

                //// LIQUIDATIONGROUP_PRISMAMETHOD
                //m_LiquidationGroupParameters = LoadParametersMethod<PrismaLiquidationGroup>.LoadParameters(connection, dbParametersValue, DataContractResultSets.LIQUIDATIONGROUP_PRISMAMETHOD);
                //// Ajout d'un liquidation group pour les paramétres manquants
                //m_LiquidationGroupParameters = m_LiquidationGroupParameters.Concat(new PrismaLiquidationGroup[] { m_EmptyLiquidationGroupParameters });

                //// LIQUIDATIONGROUPSPLIT_PRISMAMETHOD
                //m_LiquidationGroupSplitParameters = LoadParametersMethod<PrismaLiquidationGroupSplit>.LoadParameters(connection, dbParametersValue, DataContractResultSets.LIQUIDATIONGROUPSPLIT_PRISMAMETHOD);
                //// Ajout d'un liquidation group split pour les paramétres manquants
                //m_LiquidationGroupSplitParameters = m_LiquidationGroupSplitParameters.Concat(new PrismaLiquidationGroupSplit[] { m_EmptyLiquidationGroupSplitParameters });

                //// RISKMEASURESET_PRISMAMETHOD
                //m_RiskMeasureSetParameters = LoadParametersMethod<PrismaRiskMeasureSet>.LoadParameters(connection, dbParametersValue, DataContractResultSets.RISKMEASURESET_PRISMAMETHOD);

                //// MARKETCAPACITY_PRISMAMETHOD
                //m_MarketCapacityParameters = LoadParametersMethod<PrismaMarketCapacity>.LoadParameters(connection, dbParametersValue, DataContractResultSets.MARKETCAPACITY_PRISMAMETHOD);

                //// LIQUIDITYFACTOR_PRISMAMETHOD
                //m_LiquidityFactorParameters = LoadParametersMethod<PrismaLiquidityFactor>.LoadParameters(connection, dbParametersValue, DataContractResultSets.LIQUIDITYFACTOR_PRISMAMETHOD);

                //// EXCHANGERATE_PRISMAMETHOD
                //m_ExchangeRateParameters = LoadParametersMethod<PrismaExchangeRate>.LoadParameters(connection, dbParametersValue, DataContractResultSets.EXCHANGERATE_PRISMAMETHOD);

                //// EXCHANGERATERMS_PRISMAMETHOD
                //m_ExchangeRateRMSParameters = LoadParametersMethod<PrismaExchangeRateRMS>.LoadParameters(connection, dbParametersValue, DataContractResultSets.EXCHANGERATERMS_PRISMAMETHOD);

                //// Set Parameters : SESSIONID
                //dbParametersValue.Add("SESSIONID", SessionId);

                //// ASSET_PRISMAMETHOD
                //m_AssetParameters = LoadParametersMethod<PrismaAsset>.LoadParameters(connection, dbParametersValue, DataContractResultSets.ASSET_PRISMAMETHOD);

                //// ASSETLIQUIDGROUPSPLIT_PRISMAMETHOD
                //m_AssetLiquidGroupSplitParameters = LoadParametersMethod<PrismaAssetLiquidGroupSplit>.LoadParameters(connection, dbParametersValue, DataContractResultSets.ASSETLIQUIDGROUPSPLIT_PRISMAMETHOD);

                //// ASSETRMSLGS_PRISMAMETHOD
                //m_AssetRMSLGSParameters = LoadParametersMethod<PrismaAssetRMSLGS>.LoadParameters(connection, dbParametersValue, DataContractResultSets.ASSETRMSLGS_PRISMAMETHOD);

                //// ASSETPRICECENARIO_PRISMAMETHOD
                //m_AssetPriceParameters = LoadParametersMethod<PrismaAssetPrice>.LoadParameters(connection, dbParametersValue, DataContractResultSets.ASSETPRICESCENARIO_PRISMAMETHOD);

                //// ASSETCOMPRESSIONERROR_PRISMAMETHOD
                //m_AssetCompressionErrorParameters = LoadParametersMethod<PrismaAssetCompressionError>.LoadParameters(connection, dbParametersValue, DataContractResultSets.ASSETCOMPRESSIONERROR_PRISMAMETHOD);

                //// ASSETVAR_PRISMAMETHOD
                //m_AssetVaRParameters = LoadParametersMethod<PrismaAssetVaR>.LoadParameters(connection, dbParametersValue, DataContractResultSets.ASSETVAR_PRISMAMETHOD);
                #endregion code déplacé
                if (false == isLoadParametersFromfile)
                {
                    LoadSpecificParametersFromRDBMS(connection, dtBusiness);
                }
            }
            //
            if (isLoadParametersFromfile)
            {
                LoadSpecificParametersFromFile(dtBusiness, pAssetETDCache);
            }
            //
            #region RBM for Prisma
            // PM 20151116 [21561] Ajout pour RBM
            // PM 20181002 [XXXXX] Désactivation de RBM dans Prisma
            //TimsMethod.LoadParameters(pCS, pAssetETDCache);
            #endregion RBM for Prisma
        }

        /// <summary>
        /// Libère les paramètres spécifiques à la méthode.
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            m_AssetExpandedParameters = default;
            m_LiquidationGroupParameters = default;
            m_LiquidationGroupSplitParameters = default;
            // PM 20180903 [24015] Prisma v8.0 : add m_TimeToExpiryAdjusmtentParameters
            m_TimeToExpiryAdjusmtentParameters = default;
            m_RiskMeasureSetParameters = default;
            m_MarketCapacityParameters = default;
            m_LiquidityFactorParameters = default;
            m_ExchangeRateParameters = default;
            m_ExchangeRateRMSParameters = default;
            m_AssetParameters = default;
            m_AssetLiquidGroupSplitParameters = default;
            m_AssetRMSLGSParameters = default;
            m_AssetPriceParameters = default;
            m_AssetCompressionErrorParameters = default;
            m_AssetVaRParameters = default;
        }

        /// <summary>
        /// Calcul du montant de déposit pour la position d'un book d'un acteur
        /// </summary>
        /// <param name="pActorId">L'acteur de la position à évaluer</param>
        /// <param name="pBookId">Le book de la position à évaluer</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="pRiskDataToEvaluate">La position pour laquelle calculer le déposit</param>
        /// <param name="opMethodComObj">Valeur de retour contenant toutes les données à passer à la feuille de calcul
        /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) de sorte à construire le noeud
        /// de la méthode de calcul (<see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
        /// et <see cref="EfsML.Interface.IMarginCalculationMethod"/>)</param>
        /// <returns>Le montant de déposit correspondant à la position</returns>
        /// PM 20160404 [22116] Devient public
        /// FI 20160613 [22256] Modify
        /// FI 20160613 [22256] Add parameter pDepositHierarchyClass
        /// PM 20170313 [22833] Changement de type pour le paramètre pPositionsToEvaluate (=>  RiskData pRiskDataToEvaluate)
        //public override List<Money> EvaluateRiskElementSpecific(
        //    int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
        //    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
        //    out IMarginCalculationMethodCommunicationObject opMethodComObj)
        public override List<Money> EvaluateRiskElementSpecific(
            int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
            RiskData pRiskDataToEvaluate,
            out IMarginCalculationMethodCommunicationObject opMethodComObj)
        {
            List<Money> riskAmounts = null;
            // Creation de l'objet de communication du détail du calcul
            PrismaCalcMethCom methodComObj = new PrismaCalcMethCom();
            opMethodComObj = methodComObj;                          // Affectation de l'objet de communication du détail du calcul en sortie
            methodComObj.MarginMethodType = this.Type;              // Affectation du type de méthode de calcul
            methodComObj.CssCurrency = m_CssCurrency;               // Affectation de la devise de calcul
            //PM 20150416 [20957] Add Affectation de IdA et IdB
            methodComObj.IdA = pActorId;                            // Affectation de l'id de l'acteur
            methodComObj.IdB = pBookId;                             // Affectation de l'id du book
            // PM 20180319 [XXXXX] Utilisation de MethodVersion à la place de PrismaRelease
            //methodComObj.PrismaRelease = Settings.Default.PrismaRelease.ToString();    // Version de Prisma
            methodComObj.PrismaRelease = MethodParameter.MethodVersion.ToString();
            //PM 20150511 [20575] Ajout date des paramètres de risque
            methodComObj.DtParameters = DtRiskParameters;
            //
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsToEvaluate = default;
            if (pRiskDataToEvaluate != default(RiskData))
            {
                // PM 20170313 [22833] Prendre uniquement la position (à l'ancien format)
                positionsToEvaluate = pRiskDataToEvaluate.GetPositionAsEnumerablePair();

                if ((positionsToEvaluate != null) && (positionsToEvaluate.Count() > 0))
                {
                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions;

                    // Grouper les positions par asset (the side of the new merged assets will be set with regards to the long and short quantities)
                    positions = PositionsGrouping.GroupPositionsByAsset(positionsToEvaluate);
                    // Ne garder que les positions dont la quantité est différente de 0
                    positions = from pos in positions
                                where (pos.Second.Quantity != 0)
                                select pos;

                    // Coverage short call and short futures (this one modify the position quantity)
                    IEnumerable<CoverageSortParameters> inputCoverage = GetSortParametersForCoverage(positions);
                    // Reduction de la position couverte
                    // FI 20160613 [22256]  
                    Pair<IEnumerable<StockCoverageCommunicationObject>, IEnumerable<StockCoverageDetailCommunicationObject>> coveredQuantities =
                        ReducePosition(pActorId, pBookId, pDepositHierarchyClass, inputCoverage, ref positions);
                    // FI 20160613 [22256] Alimentation de UnderlyingStock
                    methodComObj.UnderlyingStock = coveredQuantities.Second;

                    // Calculer les montants de risque
                    riskAmounts = EvaluateRisk(methodComObj, positions);
                }
            }
            if (riskAmounts == null)
            {
                riskAmounts = new List<Money>();
            }

            if (riskAmounts.Count == 0)
            {
                // Si aucun montant, créer un montant à zéro
                if (StrFunc.IsEmpty(this.m_CssCurrency))
                {
                    // Si aucune devise de renseignée, utiliser l'euro
                    riskAmounts.Add(new Money(0, "EUR"));
                }
                else
                {
                    riskAmounts.Add(new Money(0, this.m_CssCurrency));
                }
            }
            #region RBM for Prisma
            // PM 20151116 [21561] Ajout pour RBM
            // PM 20181002 [XXXXX] Désactivation de RBM dans Prisma
            // riskAmounts = EvaluateRiskRbm(pActorId, pBookId, pDepositHierarchyClass, positionsToEvaluate, riskAmounts, methodComObj);
            #endregion RBM for Prisma

            return riskAmounts;
        }

        /// <summary>
        /// Get a collection of sorting parameter needed by coverage strategies
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">Positions of the current risk element</param>
        /// <returns>A collection of sorting parameters in order to be used inside of the ReducePosition method</returns>
        protected override IEnumerable<CoverageSortParameters> GetSortParametersForCoverage(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            IEnumerable<CoverageSortParameters> retPos =
                from position in pGroupedPositionsByIdAsset
                join asset in m_AssetParameters on position.First.idAsset equals asset.IdAsset
                where (false == asset.PutOrCall.HasValue) || (asset.PutOrCall == PutOrCallEnum.Call)
                join assetRef in m_AssetExpandedParameters on asset.IdAsset equals assetRef.AssetId
                select new CoverageSortParameters
                {
                    AssetId = position.First.idAsset,
                    ContractId = assetRef.ContractId,
                    MaturityYearMonth = decimal.Parse(asset.ExpiryYear.ToString() + asset.ExpiryMonth.ToString() + asset.ExpiryDay.ToString()),
                    Multiplier = asset.Multiplier,
                    Quote = asset.NeutralPrice,
                    StrikePrice = asset.StrikePrice,
                    Type = asset.PutOrCall.HasValue ? RiskMethodQtyType.Call : RiskMethodQtyType.Future,
                };
            return retPos;
        }

        /// <summary>
        /// Lecture d'informations complémentaire pour les Marchés/Chambre de compensation utilisant la méthode courante 
        /// </summary>
        /// <param name="pEntityMarkets">La collection de entity/market attaché à la chambre de compensation courante</param>
        public override void BuildMarketParameters(IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        {
            base.BuildMarketParameters(pEntityMarkets);

            #region RBM for Prisma
            // PM 20151116 [21561] Ajout pour RBM
            // PM 20181002 [XXXXX] Désactivation de RBM dans Prisma
            //TimsMethod.IdCSS = this.IdCSS;
            //TimsMethod.DtBusiness = this.DtBusiness;
            //// PM 20180219 [23824] Ajout ProcessInfo à la place de IdEntity, Timing, RiskDataTime et SessionId
            //TimsMethod.ProcessInfo = this.ProcessInfo;
            ////TimsMethod.IdEntity = this.IdEntity;
            ////TimsMethod.Timing = this.Timing;
            ////TimsMethod.RiskDataTime = this.RiskDataTime;
            ////TimsMethod.SessionId = this.SessionId;
            //TimsMethod.ImRequestDiagnostics = this.ImRequestDiagnostics;
            //TimsMethod.DtMarket = this.DtMarket;
            //TimsMethod.BuildMarketParameters(pEntityMarkets);
            #endregion RBM for Prisma
        }
        #endregion override base methods

        #region static methods
        /// <summary>
        /// Arrondie une prime selon les préconisations Prisma
        /// </summary>
        /// <param name="pAmount">Prime à arrondir</param>
        /// <returns>Taux arrondi</returns>
        //PM 20150415 [20957] Ajout de la méthode PrismaRoundPremium
        public static decimal PrismaRoundPremium(decimal pAmount)
        {
            return PrismaRoundAmount(pAmount, 1);
        }
        /// <summary>
        /// Arrondie un taux de change selon les préconisations Prisma
        /// </summary>
        /// <param name="pRate">Taux à arrondir</param>
        /// <returns>Taux arrondi</returns>
        //PM 20140616 [19911] Ajout de la méthode PrismaRoundExchangeRate
        public static decimal PrismaRoundExchangeRate(decimal pRate)
        {
            return PrismaRoundAmount(pRate, 12);
        }
        /// <summary>
        /// Arrondie un montant selon les préconisations Prisma
        /// </summary>
        /// <param name="pAmount">Montant à arrondir</param>
        /// <returns>Montant arrondi</returns>
        public static decimal PrismaRoundAmount(decimal pAmount)
        {
            return PrismaRoundAmount(pAmount, 2);
        }
        /// <summary>
        /// Arrondie un montant
        /// </summary>
        /// <param name="pAmount">Montant à arrondir</param>
        /// <param name="pPrecision">Précision de l'arrondi</param>
        /// <returns>Montant arrondi</returns>
        public static decimal PrismaRoundAmount(decimal pAmount, int pPrecision)
        {
            EFS_Round round = new EFS_Round(RoundingDirectionEnum.Nearest, pPrecision, pAmount);
            return round.AmountRounded;
        }
        #endregion static methods

        #region methods
        /// <summary>
        /// Calcul la value at risk d'un ensemble de valeurs avec un niveau de confiance donnée
        /// </summary>
        /// <param name="pValues">Tableau de valeurs</param>
        /// <param name="pConfidenceLevel">Niveau de confiance voulu (entre 0 et 100)</param>
        /// <returns>Value at risk</returns>
        public static decimal ValueAtRisk(IEnumerable<decimal> pValues, decimal pConfidenceLevel)
        {
            decimal valueAtRisk = 0m;
            if ((pValues != default(IEnumerable<decimal>)) && (pValues.Count() > 0))
            {
                decimal[] orderedValues = pValues.OrderBy(v => v).ToArray();
                int nbValue = orderedValues.Length;
                if (nbValue > 0)
                {
                    decimal[] quantiles = new decimal[nbValue];
                    // Calcul des Quantiles
                    // Attention i de 0 à nbValue - 1, mais Quantile de 1 à nbValue.
                    for (int i = 0; i < nbValue; i += 1)
                    {
                        quantiles[i] = 100m * (0.5m + (i + 1) - 1m) / nbValue;
                    }
                    // Recherche de la value at risk
                    decimal confidenceQuantile = 100m - pConfidenceLevel;
                    if (confidenceQuantile <= 0)
                    {
                        valueAtRisk = orderedValues[0];
                    }
                    else if (confidenceQuantile >= quantiles.Max())
                    {
                        valueAtRisk = orderedValues[nbValue - 1];
                    }
                    else
                    {
                        int i = 0;
                        while ((i < nbValue) && (quantiles[i] < confidenceQuantile))
                        {
                            i += 1;
                        }
                        if (i >= nbValue)
                        {
                            valueAtRisk = orderedValues[nbValue - 1];
                        }
                        else if ((quantiles[i] == confidenceQuantile) || (i == 0))
                        {
                            valueAtRisk = orderedValues[i];
                        }
                        else //Interpolation linéaire
                        {
                            decimal quantileBelow = quantiles[i - 1];
                            decimal quantileAbove = quantiles[i];
                            decimal quantileInterval = quantileAbove - quantileBelow;
                            decimal valueAtRiskBelow = orderedValues[i - 1];
                            decimal valueAtRiskAbove = orderedValues[i];
                            //
                            if (quantileInterval != 0m)
                            {
                                valueAtRisk = ((quantileAbove - confidenceQuantile) / quantileInterval * valueAtRiskBelow)
                                    + ((confidenceQuantile - quantileBelow) / quantileInterval * valueAtRiskAbove);
                            }
                            else
                            {
                                valueAtRisk = 0m;
                            }
                        }
                        // Ne garder que si négatif et prendre la valeur absolue
                        if (valueAtRisk > 0m)
                        {
                            valueAtRisk = 0m;
                        }
                        else
                        {
                            valueAtRisk = System.Math.Abs(valueAtRisk);
                        }
                    }
                }
            }
            return valueAtRisk;
        }

        /// <summary>
        /// Aggrége un ensemble de valeur selon la méthode spécifiée
        /// </summary>
        /// <param name="pValues">Ensemble de valeur decimal</param>
        /// <param name="pAggregationMethod">Méthode d'aggrégation</param>
        /// <returns>Valeur aggrégée</returns>
        public static decimal Aggregate(IEnumerable<decimal> pValues, PrismaAggregationMethod pAggregationMethod)
        {
            decimal aggregate = 0m;
            if (pValues != default(IEnumerable<decimal>))
            {
                int count = pValues.Count();
                if (count > 0)
                {
                    switch (pAggregationMethod)
                    {
                        case PrismaAggregationMethod.Avg:
                            aggregate = pValues.Sum() / count;
                            break;
                        case PrismaAggregationMethod.Max:
                            aggregate = pValues.Max();
                            break;
                        case PrismaAggregationMethod.Med:
                            if ((count % 2) == 1)
                            {
                                aggregate = pValues.OrderBy(v => v).ElementAt(((count + 1) / 2) - 1);
                            }
                            else
                            {
                                IEnumerable<decimal> ordered = pValues.OrderBy(v => v);
                                decimal bas = ordered.ElementAt((count / 2) - 1);
                                decimal haut = ordered.ElementAt(count / 2);
                                aggregate = (bas + haut) / 2m;
                            }
                            break;
                        case PrismaAggregationMethod.Min:
                            aggregate = pValues.Min();
                            break;
                        case PrismaAggregationMethod.Sum:
                            aggregate = pValues.Sum();
                            break;
                        case PrismaAggregationMethod.NA:
                        default:
                            // PM 20200916 [25486] Par défaut, ou lorsque 1 seule valeur (normalement le cas lorsque la méthode est NA), on prends le max
                            aggregate = pValues.Max();
                            break;
                    }
                }
            }
            return aggregate;
        }

        /// <summary>
        /// Retourne l'ensemble des paramètres necessaires aux calculs pour chaque asset
        /// </summary>
        /// <param name="pPositions">Position</param>
        /// <param name="pClearingCurrency">Devise de Clearing</param>
        /// <returns>Dictionnaire [ Key = IdAsset, Value = PrismaAssetRisk ]</returns>
        private Dictionary<int, PrismaAssetRisk> BuildUnitParameters(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions, string pClearingCurrency)
        {
            // Construction de l'ensemble des assets en position avec leur Liquidation Group Split par defaut (ou null si pas de paramètres)
            // Stockage dans un dictionnaire indexé par l'IdAsset
            Dictionary<int, PrismaAssetRisk> assetInPos = (
                from pos in pPositions
                join asset in m_AssetParameters on pos.First.idAsset equals asset.IdAsset
                into assetPos
                from asset in assetPos.DefaultIfEmpty(m_EmptyAssetParameters)
                join assetLgs in m_AssetLiquidGroupSplitParameters on asset.IdAsset equals assetLgs.IdAsset
                into assetLgsPos
                from lgsPos in assetLgsPos.DefaultIfEmpty(m_EmptyAssetLiquidGroupSplitParameters)
                where (lgsPos.IsDefault == true)
                select new PrismaAssetRisk(asset, lgsPos)
                ).Distinct((a, b) => a.IdAsset == b.IdAsset, c => c.IdAsset.GetHashCode()).ToDictionary(a => a.IdAsset, a => a);

            SetLiquidationGroup(assetInPos);

            SetLiquidationGroupSplit(assetInPos);

            SetClearingCurrency(assetInPos, pClearingCurrency);

            SetLiquidityFactor(assetInPos);

            SetMarketCapacity(assetInPos);

            // Recherche des Risk Measure Set de chaque assets en position
            // PM 20161019 [22174] Prisma 5.0 : Ne plus prendre LiquidationHorizon dans PrismaAssetRisk (asset), mais utilisé la propriété de PrismaAssetRMSLGS (rmsLgsParam)
            //// PM 20140611 [19911] Ajout filtre sur IdLgs (asset.Value.IdLgs == rmsParam.IdLgs)
            //var rmsOfAssetInPos =
            //    from asset in assetInPos
            //    join rmsLgsParam in m_AssetRMSLGSParameters on asset.Key equals rmsLgsParam.IdAsset
            //    join rmsParam in m_RiskMeasureSetParameters on rmsLgsParam.IdRmsLgs equals rmsParam.IdRmsLgs
            //    where (asset.Value.IdLgs == rmsParam.IdLgs)
            //    group new PrismaAssetRmsRisk(asset.Value.LiquidationHorizon, rmsParam, rmsLgsParam) by asset into assetRms
            //    select assetRms;
            var rmsOfAssetInPos =
                from asset in assetInPos
                join rmsLgsParam in m_AssetRMSLGSParameters on asset.Key equals rmsLgsParam.IdAsset
                join rmsParam in m_RiskMeasureSetParameters on rmsLgsParam.IdRmsLgs equals rmsParam.IdRmsLgs
                where (asset.Value.IdLgs == rmsParam.IdLgs)
                group new PrismaAssetRmsRisk(rmsParam, rmsLgsParam) by asset into assetRms
                select assetRms;
            //

            // Rechercher des données des Risk Measure Set de chaque assets en position et Affectation à l'asset
            foreach (var rmsOfasset in rmsOfAssetInPos)
            {
                // Liste des Risk Measure Set de l'asset en position
                List<PrismaAssetRmsRisk> assetRmsRisk = rmsOfasset.ToList();
                //
                // Recherche des différentes données de chaque Risk Measure Set de l'asset en position
                foreach (PrismaAssetRmsRisk rmsRisk in assetRmsRisk)
                {
                    // Recherche des Prices du Risk Measure Set de l'asset en position
                    rmsRisk.Prices = m_AssetPriceParameters.Where(p => p.IdAssetRmsLgs == rmsRisk.IdAssetRmsLgs);
                    //rmsRisk.Vars = m_AssetVaRParameters.Where(v => v.IdAssetRmsLgs == rmsRisk.IdAssetRmsLgs);
                    rmsRisk.Vars = m_AssetVaRParameters.Where(v => v.IdAssetRmsLgs == rmsRisk.IdAssetRmsLgs);
                    // Recherche des Compression Errors
                    rmsRisk.CompressionErrors = m_AssetCompressionErrorParameters.FirstOrDefault(ce => (ce.IdAssetRmsLgs == rmsRisk.IdAssetRmsLgs) && (ce.Currency == rmsOfasset.Key.Value.Currency));
                    // Si Calcul en Clearing Currency
                    if ((rmsOfasset.Key.Value.CurrencyTypeFlag == PrismaCurrencyTypeFlagEnum.ClearingCurrency)
                        && (pClearingCurrency != rmsOfasset.Key.Value.AssetCurrency))
                    {
                        // Recherche des données utiles à la convertion en Clearing Currency
                        //
                        PrismaExchangeRate exchRateParameters = m_ExchangeRateParameters.FirstOrDefault(r => (r.ClearingCurrency == pClearingCurrency)
                            && (r.SourceCurrency == rmsOfasset.Key.Value.AssetCurrency)
                            && (r.IdFx == rmsRisk.IdFx));
                        //
                        if (exchRateParameters == default(PrismaExchangeRate))
                        {
                            // Taux de change non trouvé : recherche taux de change inverse
                            exchRateParameters = m_ExchangeRateParameters.FirstOrDefault(r => (r.ClearingCurrency == rmsOfasset.Key.Value.AssetCurrency)
                                && (r.SourceCurrency == pClearingCurrency)
                                && (r.IdFx == rmsRisk.IdFx));
                            rmsRisk.IsFxRatesInverted = true;
                        }
                        rmsRisk.ExchangeRateParameters = exchRateParameters;
                        if (rmsRisk.IsExchangeRateMissing)
                        {
                            rmsRisk.IsFxRatesInverted = false;
                        }
                        else
                        {
                            rmsRisk.FxRates = m_ExchangeRateRMSParameters.Where(r => (r.IdRms == rmsRisk.IdRms) && (r.IdFxPair == rmsRisk.IdFxPair));
                        }
                    }
                }
                //
                // Affectation de la liste des Risk Measure Set de l'asset en position à ce dernier
                rmsOfasset.Key.Value.AssetRmsRisk = assetRmsRisk;
                //
                // Evaluation des P&L "unitaires" de chaque scénario
                rmsOfasset.Key.Value.EvaluateProfitAndLosses();
            }

            return assetInPos;
        }

        /// <summary>
        ///  Evalue les montants de dépot de garantie
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pPositions">Position</param>
        /// <returns>Liste des montants calculés</returns>
        private List<Money> EvaluateRisk(PrismaCalcMethCom pMethodComObj, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            List<Money> marginAmountList = new List<Money>();

            // Construction des paramètres de calcul pour chaque Asset en position
            // Dictionnaire [ Key = IdAsset, Value = PrismaAssetRisk ]
            // TODO : Optimisation éventuelle : construire une fois pour toute pour tous les assets, puis selectioner ceux pour la position concernée
            Dictionary<int, PrismaAssetRisk> assetInPos = BuildUnitParameters(pPositions, pMethodComObj.CssCurrency);

            // ------------------------------
            // Prise en compte de la position
            // ------------------------------
            KeyValuePair<int, PrismaAssetRisk> defaultAssetRisk = assetInPos.FirstOrDefault(a => a.Key == 0);

            // Association de la position et des données de risque de l'asset en position
            IEnumerable<PrismaPositionRisk> positionRisk =
                from pos in pPositions
                join asset in assetInPos on pos.First.idAsset equals asset.Key
                into assetPos
                from asset in assetPos.DefaultIfEmpty(defaultAssetRisk)
                select new PrismaPositionRisk(asset.Value, pos);

            // Regroupement des données de risque des positions par Lgs en vue des calculs des montants pour chaque Lgs
            // PM 20161019 [22174] Prisma 5.0 : Ajout tri des LGS
            List<PrismaPositionLgsRisk> lgsRisk = (
                from pos in positionRisk
                group pos by pos.AssetRisk.LgsParameters
                    into lgsPos
                    select new PrismaPositionLgsRisk(lgsPos.Key, lgsPos)
                ).OrderBy(l => l.LiquidationGroupSplit).ToList();

            // Calcul du déposit de chaque LGS
            foreach (PrismaPositionLgsRisk lgs in lgsRisk.Where(lgs => lgs.LgsParameters.IdLgs != 0))
            {
                // PM 20180903 [24015] Prisma v8.0 : Recherche des Time To Expiry Adjustment du LGS 
                lgs.TEAParameters = m_TimeToExpiryAdjusmtentParameters;

                // PM 20180319 [XXXXX] Utilisation de MethodVersion à la place de PrismaRelease
                //lgs.EvaluateInitialMargin();
                lgs.EvaluateInitialMargin(MethodParameter.MethodVersion);
            }

            // Regroupement des données de risque des positions par Lg en vue des calculs des montants pour chaque Lg
            var lgsOfLgPosRisk =
                from posRisk in positionRisk
                where (posRisk != default(PrismaPositionRisk)) && (posRisk.AssetRisk != default)
                group posRisk.AssetRisk.IdLgs by posRisk.AssetRisk.LgParameters into lgsOfLg
                select lgsOfLg;

            // PM 20161019 [22174] Prisma 5.0 : Ajout tri des LG
            List<PrismaPositionLgRisk> lgRisk = (
                from lg in lgsOfLgPosRisk
                from lgs in lg.Distinct()   // Ne garder que les IdLgs distinct
                join lgsPos in lgsRisk on lgs equals lgsPos.IdLgs
                where (lgsPos != default(PrismaPositionLgsRisk))
                group lgsPos by lg.Key into lgPos
                select new PrismaPositionLgRisk(lgPos.Key, lgPos)
                ).OrderBy(l => l.LiquidationGroup).ToList();

            // Calcul du déposit de chaque LG
            foreach (PrismaPositionLgRisk lg in lgRisk.Where(lg => lg.LgParameters.IdLg != 0))
            {
                lg.EvaluateInitialMargin();
            }

            // Cumul par devise des montants de chaque LG
            //PM 20150416 [20957] ajouter un arrondi
            //marginAmountList = (
            //    from lg in lgRisk
            //    from amount in lg.InitialMargin
            //    group amount by amount.Currency into amountByCurrency
            //    select new Money(amountByCurrency.Sum(a => a.Amount.DecValue), amountByCurrency.Key)
            //    ).ToList();
            // PM 20150907 [21236] Prendre le MarginRequirement plutot que l'InitialMargin
            //marginAmountList = (
            //    from lg in lgRisk
            //    from amount in lg.InitialMargin
            //    group amount by amount.Currency into amountByCurrency
            //    select new Money(PrismaRoundAmount(amountByCurrency.Sum(a => a.Amount.DecValue)), amountByCurrency.Key)
            //    ).ToList();
            marginAmountList = (
                from lg in lgRisk
                where (lg.MarginRequirement != default)
                from amount in lg.MarginRequirement
                where (amount != default)
                group amount by amount.Currency into amountByCurrency
                select new Money(PrismaRoundAmount(amountByCurrency.Sum(a => a.Amount.DecValue)), amountByCurrency.Key)
                ).ToList();

            // Log
            pMethodComObj.MarginAmounts = marginAmountList.ToArray();
            SetCalculationLog(pMethodComObj, lgRisk);
            //
            return marginAmountList;
        }

        /// <summary>
        /// Construction du log du calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pPositions"></param>
        /// <param name="pLgRisk"></param>
        private void SetCalculationLog(PrismaCalcMethCom pMethodComObj, List<PrismaPositionLgRisk> pLgRisk)
        {
            // Log niveau Liquidation Group
            PrismaLiquidGroupCom[] lgCom = new PrismaLiquidGroupCom[pLgRisk.Count];
            int currentLg = 0;
            foreach (PrismaPositionLgRisk lgRisk in pLgRisk)
            {
                // PM 20150907 [21236] Ajout PremiumMargin et MarginRequirement
                PrismaLiquidGroupCom lg = new PrismaLiquidGroupCom
                {
                    IdLg = lgRisk.IdLg,
                    Identifier = lgRisk.LiquidationGroup,
                    CurrencyTypeFlag = lgRisk.CurrencyTypeFlag.ToString(),
                    ClearingCurrency = pMethodComObj.CssCurrency,
                    InitialMargin = (lgRisk.InitialMargin != default) ? lgRisk.InitialMargin.ToArray() : default,
                    PremiumMargin = (lgRisk.PremiumMargin != default) ? lgRisk.PremiumMargin.ToArray() : default,
                    MarginRequirement = (lgRisk.MarginRequirement != default) ? lgRisk.MarginRequirement.ToArray() : default,
                    Missing = (lgRisk.LiquidationGroup == Cst.NotFound),
                };
                //
                if (lgRisk.PositionLgsRisk != default)
                {
                    //
                    lg.Positions =
                        from lgsRisk in lgRisk.PositionLgsRisk
                        from pos in lgsRisk.PositionRisk
                        select pos.Position;
                    //
                    // Log niveau Liquidation Group Split
                    lg.Parameters = new PrismaLiquidGroupSplitCom[lgRisk.PositionLgsRisk.Count()];
                    int currentLgs = 0;
                    foreach (PrismaPositionLgsRisk lgsRisk in lgRisk.PositionLgsRisk)
                    {
                        // PM 20200826 [25467] Ajout détail du calcul Long Option Credit: Present Value et Maximal Lost
                        PrismaLiquidGroupSplitCom lgs = new PrismaLiquidGroupSplitCom
                        {
                            IdLgs = lgsRisk.IdLgs,
                            Identifier = lgsRisk.LiquidationGroupSplit,
                            RiskMethod = lgsRisk.LgsParameters.RiskMethod,
                            AggregationMethod = lgsRisk.GroupSplitAggregationMethod.ToString(),
                            AssetLiquidityRisk = BuildPrismaAssetLiquidityRiskCom(lgsRisk.AssetLiquidityRisk),
                            MarketRisk = lgsRisk.MarketRisk.ToArray(),
                            // PM 20180903 [24015] Prisma v8.0 : add TimeToExpiryAdjustment
                            TimeToExpiryAdjustment = lgsRisk.TimeToExpiryAdjustment.ToArray(),
                            LiquidityRisk = lgsRisk.LiquidityRisk.ToArray(),
                            InitialMargin = lgsRisk.InitialMargin.ToArray(),
                            PremiumMargin = lgsRisk.PremiumMargin.ToArray(),
                            PresentValue = lgsRisk.PresentValue.ToArray(),
                            MaximalLost = lgsRisk.MaximalLost.ToArray(),
                            LongOptionCredit = lgsRisk.LongOptionCredit.ToArray(),
                            TotalInitialMargin = lgsRisk.TotalInitialMargin.ToArray(),
                            Positions = from pos in lgsRisk.PositionRisk select pos.Position,
                            Missing = (lgsRisk.LgsParameters.Identifier == Cst.NotFound),
                        };
                        //
                        if (lgsRisk.PositionRisk != default)
                        {
                            // Log niveau Risk Measure Set
                            List<PrismaRiskMeasureSetCom> rmsList = new List<PrismaRiskMeasureSetCom>();
                            foreach (KeyValuePair<string, List<PrismaRmsSubSamplesRisk>> rmsRisk in lgsRisk.RmsRiskCur)
                            {
                                List<PrismaRmsSubSamplesRisk> rmsRiskList = rmsRisk.Value;
                                foreach (PrismaRmsSubSamplesRisk rmsSubSample in rmsRiskList)
                                {
                                    PrismaRiskMeasureSetCom rms = new PrismaRiskMeasureSetCom
                                    {
                                        IdRms = rmsSubSample.IdRms,
                                        Identifier = rmsSubSample.RiskMeasureSet,
                                        HistoricalStressed = rmsSubSample.HistoricalStressed.ToString(),
                                        AggregationMethod = rmsSubSample.AggregationMethod.ToString(),
                                        ConfidenceLevel = rmsSubSample.ConfidenceLevel,
                                        IsUseRobustness = rmsSubSample.IsUseRobustness,
                                        ScalingFactor = rmsSubSample.ScalingFactor,
                                        CorrelationBreakConfidenceLevel = rmsSubSample.CorrelationBreakConfidenceLevel,
                                        CorrelationBreakSubWindow = rmsSubSample.CorrelationBreakSubWindow,
                                        CorrelationBreakMultiplier = rmsSubSample.CorrelationBreakMultiplier,
                                        CorrelationBreakMin = rmsSubSample.CorrelationBreakMin,
                                        CorrelationBreakMax = rmsSubSample.CorrelationBreakMax,
                                        MarketRiskComponent = rmsSubSample.MarketRiskComponent,
                                        ScaledMarketRiskComponent = rmsSubSample.ScaledMarketRiskComponent,
                                        IsLiquidityComponent = rmsSubSample.RmsParameters.IsLiquidityComponent,
                                        ValueAtRiskLiquidityComponent = rmsSubSample.ValueAtRiskLiquidityComponent,
                                        AlphaConfidenceLevel = rmsSubSample.RmsParameters.AlphaConfidenceLevel,
                                        AlphaFloor = rmsSubSample.RmsParameters.AlphaFloor,
                                        AlphaFactor = rmsSubSample.AlphaFactor
                                    };
                                    //
                                    PrismaSubSampleCom[] subSampleCom = new PrismaSubSampleCom[rmsSubSample.SubSamplesRisk.Count()];
                                    int currentSubSample = 0;
                                    foreach (PrismaSubSampleRisk subSample in rmsSubSample.SubSamplesRisk)
                                    {
                                        // PM 20161019 [22174] Prisma 5.0 : Ajout PureMarketRisk
                                        PrismaSubSampleCom ss = new PrismaSubSampleCom
                                        {
                                            CompressionError = subSample.AssetCompressionError,
                                            ValueAtRisk = subSample.ValueAtRisk,
                                            ValueAtRiskScaled = subSample.ValueAtRiskScaled,
                                            ValueAtRiskLiquidityComponentSpecified = rmsSubSample.RmsParameters.IsLiquidityComponent,
                                            ValueAtRiskLiquidityComponent = subSample.ValueAtRiskLiquidityRiskComponent,
                                            MeanExcessRisk = subSample.MeanExcessRisk,
                                            PureMarketRisk = subSample.PureMarketRisk,
                                            CorrelationBreakAdjustment = subSample.CorrelationBreakAdjustment,
                                            CbLowerBound = subSample.CbLowerBound,
                                            CbUpperBound = subSample.CbUpperBound,
                                            CompressionAdjustment = subSample.CompressionAdjustment,
                                            MarketRiskComponent = subSample.MarketRiskComponent,
                                        };
                                        subSampleCom[currentSubSample] = ss;
                                        currentSubSample += 1;
                                    }
                                    //
                                    rms.Parameters = subSampleCom;
                                    rmsList.Add(rms);
                                }
                            }
                            lgs.Parameters = rmsList.ToArray();
                        }
                        //
                        lg.Parameters[currentLgs] = lgs;
                        currentLgs += 1;
                    }
                }
                //
                lgCom[currentLg] = lg;
                currentLg += 1;
            }
            // Affectation du log
            pMethodComObj.Parameters = lgCom;
        }

        /// <summary>
        ///  Affecte les facteurs de liquidité à chaque asset en position 
        /// </summary>
        /// <param name="pAssetInPos">Représente les assets en position</param>
        /// FI 20140227 Add method
        private void SetLiquidityFactor(Dictionary<int, PrismaAssetRisk> pAssetInPos)
        {
            // Lecture des Liquidation Factor qui s'appliquent à chaque assets en position (ou null si pas de paramètres)
            var lfOfAssetInPos =
                from asset in pAssetInPos
                join lfParam in m_LiquidityFactorParameters on asset.Value.IdLiquidClass equals lfParam.IdLiquidClass
                into lfPos
                from lf in lfPos.DefaultIfEmpty(m_EmptyLiquidityFactor)
                select new { asset.Value.IdAsset, Lf = lf };

            foreach (int idAsset in pAssetInPos.Keys)
            {
                IEnumerable<PrismaLiquidityFactor> lf =
                    from item in lfOfAssetInPos
                    where item.IdAsset == idAsset
                    select item.Lf;

                pAssetInPos[idAsset].LiquidityFactorParameters = new List<PrismaLiquidityFactor>(lf);
            }

        }

        /// <summary>
        ///  Affecte les market Capacity à chaque asset en position 
        /// </summary>
        /// <param name="pAssetInPos">Représente les assets en position</param>
        /// FI 20140227 Add method
        private void SetMarketCapacity(Dictionary<int, PrismaAssetRisk> pAssetInPos)
        {
            // Lecture des market Capacity de chaque asset en position
            var mcOfAssetInPos =
              from asset in pAssetInPos
              join mcParam in m_MarketCapacityParameters on
               new
               {
                   asset.Value.Product,
                   asset.Value.TimeToExpiryBucketId,
                   MoneynessBucketId = StrFunc.IsEmpty(asset.Value.MoneynessBucketId) ? "N/A" : asset.Value.MoneynessBucketId,
                   PutCall = StrFunc.IsEmpty(asset.Value.PutCall) ? "N/A" : asset.Value.PutCall
               }
               equals
               new
               {
                   Product = mcParam.ProductId,
                   mcParam.TimeToExpiryBucketId,
                   MoneynessBucketId = StrFunc.IsEmpty(mcParam.MoneynessBucketId) ? "N/A" : mcParam.MoneynessBucketId,
                   PutCall = StrFunc.IsEmpty(mcParam.PutCall) ? "N/A" : mcParam.PutCall,
               }
               into mcPos
              from mc in mcPos.DefaultIfEmpty(m_EmptyMarketCapacity)
              select new { asset.Value.IdAsset, mc };
            //
            foreach (int idAsset in pAssetInPos.Keys)
            {
                PrismaMarketCapacity mc =
                    (from item in mcOfAssetInPos
                     where item.IdAsset == idAsset
                     select item.mc).FirstOrDefault();

                pAssetInPos[idAsset].MarketCapacity = mc;
            }
        }

        /// <summary>
        ///  Affecte les Liquidation Group Split à chaque asset en position 
        /// </summary>
        /// <param name="pAssetInPos"></param>
        private void SetLiquidationGroupSplit(Dictionary<int, PrismaAssetRisk> pAssetInPos)
        {
            // Recherche des paramètres du Liquidation Groupe Split de chaque assets en position (ou null si pas de paramètres)
            var lgsOfAssetInPos =
                from asset in pAssetInPos
                join lgsParam in m_LiquidationGroupSplitParameters on asset.Value.IdLgs equals lgsParam.IdLgs
                into lgsPos
                from lgs in lgsPos.DefaultIfEmpty(m_EmptyLiquidationGroupSplitParameters)
                select new { Asset = asset, Lgs = lgs };

            // Affectation du Liquidation Group Split de chaque assets en position
            foreach (var lgsOfasset in lgsOfAssetInPos)
            {
                lgsOfasset.Asset.Value.LgsParameters = lgsOfasset.Lgs;
            }
        }

        /// <summary>
        ///  Affecte les Liquidation Group à chaque asset en position 
        /// </summary>
        /// <param name="pAssetInPos"></param>
        private void SetLiquidationGroup(Dictionary<int, PrismaAssetRisk> pAssetInPos)
        {
            // Recherche du Liquidation Group de chaque assets en position (ou null si pas de paramètres)
            var lgOfAssetInPos =
                from asset in pAssetInPos
                join lgParam in m_LiquidationGroupParameters on asset.Value.IdLg equals lgParam.IdLg
                into lgPos
                from lg in lgPos.DefaultIfEmpty(m_EmptyLiquidationGroupParameters)
                select new { Asset = asset, Lg = lg };
            //
            // Affectation du Liquidation Group de chaque assets en position
            foreach (var lgOfasset in lgOfAssetInPos)
            {
                lgOfasset.Asset.Value.LgParameters = lgOfasset.Lg;
            }
        }

        /// <summary>
        ///  Affecte la devise de la chambre à chaque asset en position 
        /// </summary>
        /// <param name="pAssetInPos"></param>
        /// <param name="pClearingCurrency"></param>
        private void SetClearingCurrency(Dictionary<int, PrismaAssetRisk> pAssetInPos, string pClearingCurrency)
        {
            foreach (var item in pAssetInPos)
            {
                item.Value.ClearingCurrency = pClearingCurrency;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAssetLiquidityRisk"></param>
        /// <returns></returns>
        private Dictionary<int, PrismaAssetLiquidityRiskCom> BuildPrismaAssetLiquidityRiskCom(Dictionary<int, PrismaAssetLiquidityRisk> pAssetLiquidityRisk)
        {
            Dictionary<int, PrismaAssetLiquidityRiskCom> ret = default;

            if (pAssetLiquidityRisk != null)
            {
                ret = (from item in pAssetLiquidityRisk
                       select new
                       {
                           key = item.Key,
                           value = new PrismaAssetLiquidityRiskCom
                           {
                               tradeUnit = item.Value.tradeUnit,
                               netGrossRatio = item.Value.netGrossRatio,
                               liquidityFactor = item.Value.liquidityFactor,
                               liquidityPremium = item.Value.liquidityPremium,
                               riskMeasure = item.Value.riskMeasure,
                               liquidityAdjustment = item.Value.liquidityAdjustment,
                               additionalRiskMeasure = item.Value.additionalRiskMeasure
                           }
                       }
                      ).ToDictionary(mc => mc.key, mc => mc.value);
            }
            return ret;
        }

        /// <summary>
        /// Chargement des paramètres de calcul à partir de la base  de données
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pAssetETDCache"></param>
        // PM 20180219 [23824] New
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        private void LoadSpecificParametersFromRDBMS(IDbConnection pConnection, DateTime pDtBusiness)
        {
            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>();
            if (pConnection != default(IDbConnection))
            {
                // Set Parameters : DTBUSINESS
                dbParametersValue.Add("DTBUSINESS", pDtBusiness);

                // LIQUIDATIONGROUP_PRISMAMETHOD
                m_LiquidationGroupParameters = LoadParametersMethod<PrismaLiquidationGroup>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.LIQUIDATIONGROUP_PRISMAMETHOD);
                // Ajout d'un liquidation group pour les paramétres manquants
                m_LiquidationGroupParameters = m_LiquidationGroupParameters.Concat(new PrismaLiquidationGroup[] { m_EmptyLiquidationGroupParameters });

                // LIQUIDATIONGROUPSPLIT_PRISMAMETHOD
                m_LiquidationGroupSplitParameters = LoadParametersMethod<PrismaLiquidationGroupSplit>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.LIQUIDATIONGROUPSPLIT_PRISMAMETHOD);
                // Ajout d'un liquidation group split pour les paramétres manquants
                m_LiquidationGroupSplitParameters = m_LiquidationGroupSplitParameters.Concat(new PrismaLiquidationGroupSplit[] { m_EmptyLiquidationGroupSplitParameters });

                // RISKMEASURESET_PRISMAMETHOD
                m_RiskMeasureSetParameters = LoadParametersMethod<PrismaRiskMeasureSet>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.RISKMEASURESET_PRISMAMETHOD);

                // MARKETCAPACITY_PRISMAMETHOD
                m_MarketCapacityParameters = LoadParametersMethod<PrismaMarketCapacity>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.MARKETCAPACITY_PRISMAMETHOD);

                // LIQUIDITYFACTOR_PRISMAMETHOD
                m_LiquidityFactorParameters = LoadParametersMethod<PrismaLiquidityFactor>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.LIQUIDITYFACTOR_PRISMAMETHOD);

                // EXCHANGERATE_PRISMAMETHOD
                m_ExchangeRateParameters = LoadParametersMethod<PrismaExchangeRate>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.EXCHANGERATE_PRISMAMETHOD);

                // EXCHANGERATERMS_PRISMAMETHOD
                m_ExchangeRateRMSParameters = LoadParametersMethod<PrismaExchangeRateRMS>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.EXCHANGERATERMS_PRISMAMETHOD);

                // ASSET_PRISMAMETHOD
                m_AssetParameters = LoadParametersMethod<PrismaAsset>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.ASSET_PRISMAMETHOD);

                // ASSETLIQUIDGROUPSPLIT_PRISMAMETHOD
                m_AssetLiquidGroupSplitParameters = LoadParametersMethod<PrismaAssetLiquidGroupSplit>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.ASSETLIQUIDGROUPSPLIT_PRISMAMETHOD);

                // ASSETRMSLGS_PRISMAMETHOD
                m_AssetRMSLGSParameters = LoadParametersMethod<PrismaAssetRMSLGS>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.ASSETRMSLGS_PRISMAMETHOD);

                // ASSETPRICECENARIO_PRISMAMETHOD
                m_AssetPriceParameters = LoadParametersMethod<PrismaAssetPrice>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.ASSETPRICESCENARIO_PRISMAMETHOD);

                // ASSETCOMPRESSIONERROR_PRISMAMETHOD
                m_AssetCompressionErrorParameters = LoadParametersMethod<PrismaAssetCompressionError>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.ASSETCOMPRESSIONERROR_PRISMAMETHOD);

                // ASSETVAR_PRISMAMETHOD
                m_AssetVaRParameters = LoadParametersMethod<PrismaAssetVaR>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.ASSETVAR_PRISMAMETHOD);
            }
        }

        /// <summary>
        /// Chargement des paramètres de calcul à partir des fichiers
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pAssetETDCache"></param>
        // PM 20180219 [23824] New
        private void LoadSpecificParametersFromFile(DateTime pDtBusiness, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            Logger.Write();
            
            if (pAssetETDCache != default(Dictionary<int, SQL_AssetETD>))
            {
                // Objet qui contiendra les paramètres de calcul lus lors de l'import
                // PM 20190222 [24326] Ajout MethodType
                RiskDataLoadPrisma filePrismaData = new RiskDataLoadPrisma(this.Type, pAssetETDCache.Values);
                
                // Lancement de l'import
                RiskDataImportTask import = new RiskDataImportTask(ProcessInfo.Process, IdIOTaskRiskData);
                import.ProcessTask(pDtBusiness, filePrismaData);

                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1078), 1));
                
                // Déverser les données dans les classes de calcul
                DumpFileParametersToPrismaData(filePrismaData);
            }
            
        }

        /// <summary>
        /// Déverse les données provenant des fichiers de paramètres dans les classes de paramètres utilisées par le calcul du déposit
        /// </summary>
        /// <param name="pPrismaData"></param>
        private void DumpFileParametersToPrismaData(RiskDataLoadPrisma pFilePrismaData)
        {
            if (pFilePrismaData != default(RiskDataLoadPrisma))
            {
                #region Liquidation Group
                if (pFilePrismaData.LiquidationGroupData != default(List<RiskDataPrismaLiquidationGroup>))
                {
                    m_LiquidationGroupParameters = pFilePrismaData.LiquidationGroupData.Select((v, i) => new PrismaLiquidationGroup
                    {
                        CurrencyTypeFlag = v.CurrencyTypeFlag,
                        Identifier = v.LiquidationGroup,
                        IdLg = i + 1,
                    }).ToList();
                    m_LiquidationGroupParameters = m_LiquidationGroupParameters.Concat(new PrismaLiquidationGroup[] { m_EmptyLiquidationGroupParameters });
                }
                else
                {
                    m_LiquidationGroupParameters = new PrismaLiquidationGroup[] { m_EmptyLiquidationGroupParameters };
                }
                #endregion LiquidationGroup

                #region Liquidation Group Split
                if (pFilePrismaData.LiquidationGroupSplitData != default(List<RiskDataPrismaLiquidationGroupSplit>))
                {
                    m_LiquidationGroupSplitParameters = pFilePrismaData.LiquidationGroupSplitData.Select((v, i) => new PrismaLiquidationGroupSplit
                    {
                        AggregationMethod = v.AggregationMethod,
                        Identifier = v.LiquidationGroupSplit,
                        IdLg = m_LiquidationGroupParameters.Where(l => l.Identifier == v.LiquidationGroup).Select(l => l.IdLg).FirstOrDefault(),
                        IdLgs = i + 1,
                        RiskMethod = v.RiskMethod,
                        // PM 20180903 [24015] Prisma v8.0 : affectation IsCalculateTEA
                        IsCalculateTEA = v.IsCalculateTEA,
                    }).ToList();
                    m_LiquidationGroupSplitParameters = m_LiquidationGroupSplitParameters.Concat(new PrismaLiquidationGroupSplit[] { m_EmptyLiquidationGroupSplitParameters });
                }
                else
                { 
                    m_LiquidationGroupSplitParameters = new PrismaLiquidationGroupSplit[] { m_EmptyLiquidationGroupSplitParameters };
                }
                #endregion Liquidation Group Split

                // PM 20180903 [24015] Prisma v8.0 : affectation m_TimeToExpiryAdjusmtentParameters
                #region Time To Expiry Adjustment
                if (pFilePrismaData.TimeToExpiryAdjustmentData != default(List<RiskDataPrismaTimeToExpiryAdjustment>))
                {
                    IEnumerable<RiskDataPrismaTimeToExpiryAdjustment> filledTEA = pFilePrismaData.TimeToExpiryAdjustmentData.Where(t => t.Weight != default(Dictionary<int, Tuple<decimal, decimal>>));
                    //
                    m_TimeToExpiryAdjusmtentParameters = (
                        from teaLgs in filledTEA
                        from teaWeight in teaLgs.Weight
                        select new PrismaTimeToExpiryAdjustment
                        {
                            IdLgs = (from lg in m_LiquidationGroupParameters
                                     join lgs in m_LiquidationGroupSplitParameters on lg.IdLg equals lgs.IdLg
                                     where (lg.Identifier == teaLgs.LiquidationGroup) && (lgs.Identifier == teaLgs.LiquidationGroupSplit)
                                     select lgs.IdLgs).FirstOrDefault(),
                            TeaBucketId = teaWeight.Key,
                            HedgingWeight = teaWeight.Value.Item1,
                            DirectionalWeight = teaWeight.Value.Item2,
                        }).ToList();
                }
                else
                {
                    m_TimeToExpiryAdjusmtentParameters = new List<PrismaTimeToExpiryAdjustment>();
                }
                #endregion Time To Expiry Adjustment

                #region Risk Measure Set
                if (pFilePrismaData.RiskMeasureSetData != default(List<RiskDataPrismaRiskMeasureSet>))
                {
                    m_RiskMeasureSetParameters = pFilePrismaData.RiskMeasureSetData.Select((v, i) => new PrismaRiskMeasureSet
                    {
                        AggregationMethod = v.AggregationMethod,
                        AlphaConfidenceLevel = v.AlphaConfidenceLevel,
                        AlphaFloor = v.AlphaFloor,
                        ConfidenceLevel = v.ConfidenceLevel,
                        CorrelationBreakConfidenceLevel = v.CorrelationBreakConfidenceLevel,
                        CorrelationBreakMax = v.CorrelationBreakMax,
                        CorrelationBreakMin = v.CorrelationBreakMin,
                        CorrelationBreakMultiplier = v.CorrelationBreakMultiplier,
                        CorrelationBreakSubWindow = v.CorrelationBreakSubWindow,
                        HistoricalStressed = v.HistoricalStressed,
                        Identifier = v.RiskMeasureSet,
                        //IdLgs = m_LiquidationGroupSplitParameters.Where(l => l.Identifier == v.LiquidationGroupSplit).FirstOrDefault().IdLgs,
                        IdLgs = (from lg in m_LiquidationGroupParameters
                                 join lgs in m_LiquidationGroupSplitParameters on lg.IdLg equals lgs.IdLg
                                 where (lg.Identifier == v.LiquidationGroup) && (lgs.Identifier == v.LiquidationGroupSplit)
                                 select lgs.IdLgs).FirstOrDefault(),
                        IdRms = i + 1,
                        IdRmsLgs = i + 1,
                        IsCorrelactionBreak = v.IsCorrelactionBreak,
                        IsLiquidityComponent = v.IsLiquidityComponent,
                        IsUseRobustness = v.IsUseRobustness,
                        NumberOfWorstScenarios = v.NumberOfWorstScenarios,
                        RiskMeasureType = v.RiskMeasureType,
                        ScalingFactor = v.ScalingFactor,
                        WeightingFactor = v.WeightingFactor,
                    }).ToList();
                }
                else
                {
                    m_RiskMeasureSetParameters = new List<PrismaRiskMeasureSet>();
                }
                #endregion Risk Measure Set

                #region Market Capacity
                if (pFilePrismaData.MarketCapacityData != default(List<RiskDataPrismaMarketCapacity>))
                {
                    // PM 20191127 [25098] Ajout filtre sur les paramètres Market Capacities afin de ne conserver que ceux correspondant à des asset en position
                    //m_MarketCapacityParameters = pFilePrismaData.MarketCapacityData.Select((v, i) => new PrismaMarketCapacity
                    var usefullParam =
                        from mktCapa in pFilePrismaData.MarketCapacityData
                        join serie in pFilePrismaData.SerieData on mktCapa.ProductId equals serie.Expiration.Product.ProductId
                        where (mktCapa.TimeToExpiryBucketID == serie.TimeToExpiryBucketID)
                           && ( (mktCapa.MoneynessBucketID == serie.MoneynessBucketID)
                             || (StrFunc.IsEmpty(mktCapa.MoneynessBucketID) && StrFunc.IsEmpty(serie.MoneynessBucketID)))
                        select mktCapa;

                    m_MarketCapacityParameters = usefullParam.Select((v, i) => new PrismaMarketCapacity
                    {
                        IdMktCapa = i + 1,
                        LiquidityPremium = v.LiquidityPremium,
                        MarketCapacity = v.MarketCapacity,
                        MoneynessBucketId = v.MoneynessBucketID,
                        ProductId = v.ProductId,
                        ProductLine = v.ProductLine,
                        PutCall = v.PutCallFlag,
                        TimeToExpiryBucketId = v.TimeToExpiryBucketID,
                    }).ToList();
                }
                else
                {
                    m_MarketCapacityParameters = new List<PrismaMarketCapacity>();
                }
                #endregion Market Capacity

                #region Liquidity Factor
                IEnumerable<Tuple<string,int>> liquidClass;
                //
                if (pFilePrismaData.LiquidityFactorData != default(List<RiskDataPrismaLiquidityFactor>))
                {
                    // Génération d'un Id par Liquidity Class
                    liquidClass = pFilePrismaData.LiquidityFactorData.Select(v => v.LiquidityClass).Distinct().Select((v, i) => new Tuple<string, int>(v, i + 1));
                    //
                    m_LiquidityFactorParameters = pFilePrismaData.LiquidityFactorData.Select(v => new PrismaLiquidityFactor
                    {
                        Identifier = v.LiquidityClass,
                        IdLiquidClass = liquidClass.FirstOrDefault(l => l.Item1 == v.LiquidityClass).Item2,
                        MaxThresholdFactor = v.LiquidityFactorMaxThreshold,
                        MinThresholdFactor = v.LiquidityFactorMinThreshold,
                        PctMaxThreshold = v.MaxPercentThreshold,
                        PctMinThreshold = v.MinPercentThreshold,
                    }).ToList();
                }
                else
                {
                    m_LiquidityFactorParameters = new List<PrismaLiquidityFactor>();
                    liquidClass = new List<Tuple<string, int>>();
                }
                #endregion Liquidity Factor

                #region Exchange Rate (Fx Set) & Exchange Rate of RMS
                if (pFilePrismaData.FxRateData != default(List<RiskDataPrismaFxRate>))
                {
                    #region Exchange Rate (Fx Set)
                    // Ensemble des différents Fx Set contenu dans l'ensemble des RiskDataPrismaFxRate
                    var fxSet = pFilePrismaData.FxRateData.Select(v => v.FxSet).Distinct().Select((v, i) => new { FxSet = v, IdFx = i + 1 });
                    // Ensemble des Fx Rate par Fx Set
                    var fxPair = from rate in pFilePrismaData.FxRateData
                                 join fx in fxSet on rate.FxSet equals fx.FxSet
                                 select new { FxSet = fx, FxRate = rate };

                    m_ExchangeRateParameters = fxPair.Select((v, i) => new PrismaExchangeRate
                    {
                        CurrencyPair = v.FxRate.CurrencyPair,
                        ExchangeRate = v.FxRate.ExchangeRate,
                        Identifier = v.FxSet.FxSet,
                        IdFx = v.FxSet.IdFx,
                        IdFxPair = i + 1,
                    }).ToList();
                    #endregion Exchange Rate (Fx Set)

                    #region Exchange Rate of RMS
                    var rateRMS = from pair in fxPair   // Permet d'obtenir les Fx Rate de chaque Fx Set 
                                  join rate in m_ExchangeRateParameters on pair.FxSet.FxSet equals rate.Identifier  // Permet d'obtenir les Id des CurrencyPair (IdFxPair) des chaque Fx Set
                                  where pair.FxRate.CurrencyPair == rate.CurrencyPair
                                  from rmsRate in pair.FxRate.RMSExchangeRate // Permet d'obtenir les Fx Rate de chaque RMS pour chaque CurrencyPair et Fx Set
                                  join rms in m_RiskMeasureSetParameters on rmsRate.Key equals rms.Identifier // Permet d'obtenir l'Id du RMS
                                  from rmsRateScenario in rmsRate.Value // Permet d'obtenir un element par Fx Rate de chaque RMS de chaque CurrencyPair et Fx Set
                                  select new { rate.IdFxPair, rms.IdRms, ScenarioNumber = rmsRateScenario.Key, FxRate = rmsRateScenario.Value };

                    m_ExchangeRateRMSParameters = rateRMS.Select((v, i) => new PrismaExchangeRateRMS
                    {
                        ExchangeRate = v.FxRate,
                        IdFxPair = v.IdFxPair,
                        IdRms = v.IdRms,
                        ScenarioNumber = v.ScenarioNumber,
                    }).ToList();
                    #endregion Exchange Rate of RMS
                }
                else
                {
                    m_ExchangeRateParameters = new List<PrismaExchangeRate>();
                    m_ExchangeRateRMSParameters = new List<PrismaExchangeRateRMS>();
                }
                #endregion Exchange Rate (Fx Set) & Exchange Rate of RMS

                #region Paramètres sur les assets
                if (pFilePrismaData.SerieData != default(List<RiskDataPrismaSerie>))
                {
                    List<PrismaAsset> assetParameters = new List<PrismaAsset>();
                    foreach (RiskDataPrismaSerie serie in pFilePrismaData.SerieData)
                    {
                        PrismaAsset asset = new PrismaAsset
                        {
                            AssetCategory = serie.ContractCategory,
                            ContractSize = serie.TradingUnit,
                            IdAsset = serie.IdAsset,
                            IdExpiration = 0, // Non disponible et non utilisé
                            IdProduct = 0, // Non disponible et non utilisé
                            IdSerie = serie.IdAsset,
                            MoneynessBucketId = serie.MoneynessBucketID,
                            NeutralPrice = serie.NeutralPrice,
                            Product = serie.Expiration.Product.ProductId,
                            PutCall = serie.CallPut,
                            PVReferencePrice = serie.PVReferencePrice,
                            RiskBucket = serie.RiskBucket,
                            SettlementPrice = serie.SettlementPrice,
                            StrikePrice = serie.StrikePriceDecValue,
                            TimeToExpiryBucketId = serie.TimeToExpiryBucketID,
                            Version = serie.Version,
                            // PM 20180903 [24015] Prisma v8.0 : affectation SettlementType & SettlementMethod
                            SettlementType = serie.SettlementType,
                            SettlementMethod = serie.SettlementMethodEnum
                        };
                        //
                        if (serie.Expiration != default)
                        {
                            asset.ContractMonth = IntFunc.IntValue(serie.Expiration.ContractMonth);
                            asset.ContractYear = IntFunc.IntValue(serie.Expiration.ContractYear);
                            asset.CrossMaturityBucketId = serie.Expiration.XMMaturityBucketID;
                            asset.DaysToExpiry = serie.Expiration.DaysToExpiry;
                            asset.ExpiryDay = IntFunc.IntValue(serie.Expiration.ExpirationDay);
                            asset.ExpiryMonth = IntFunc.IntValue(serie.Expiration.ExpirationMonth);
                            asset.ExpiryYear = IntFunc.IntValue(serie.Expiration.ExpirationYear);
                            // PM 20180903 [24015] Prisma v8.0 : affectation DaysToExpiryBusiness
                            asset.DaysToExpiryBusiness = serie.Expiration.DaysToExpiryBusiness;
                            if (serie.Expiration.Product != default)
                            {
                                asset.Currency = serie.Expiration.Product.Currency;
                                PrismaLiquidationGroup lg = m_LiquidationGroupParameters.FirstOrDefault(l => l.Identifier == serie.Expiration.Product.LiquidationGroup);
                                if (lg != default)
                                {
                                    asset.IdLg = lg.IdLg;
                                }
                                else
                                {
                                    asset.IdLg = 0;
                                }
                                Tuple<string,int> lc = liquidClass.FirstOrDefault(l => l.Item1 == serie.Expiration.Product.LiquidityClass);
                                if (lc != default(Tuple<string,int>))
                                {
                                    asset.IdLiquidClass = lc.Item2;
                                }
                                else
                                {
                                    asset.IdLiquidClass = 0;
                                }
                                asset.MarginStyle = serie.Expiration.Product.MarginStyle;
                                asset.Multiplier = (serie.Expiration.Product.TickSize != 0)
                                    ? (serie.TradingUnit * serie.Expiration.Product.TickValue / serie.Expiration.Product.TickSize) : 0;
                                asset.TickSize = serie.Expiration.Product.TickSize;
                                asset.TickValue = serie.Expiration.Product.TickValue;
                            }
                            else
                            {
                                asset.Currency = null;
                                asset.IdLg = 0;
                                asset.IdLiquidClass = 0;
                                asset.MarginStyle = PrismaMarginStyleEnum.NA;
                                asset.Multiplier = 0;
                                asset.TickSize = 0;
                                asset.TickValue = 0;
                            }
                        }
                        else
                        {
                            asset.ContractMonth = 0;
                            asset.ContractYear = 0;
                            asset.CrossMaturityBucketId = 0;
                            asset.Currency = null;
                            asset.DaysToExpiry = 0;
                            asset.ExpiryDay = 0;
                            asset.ExpiryMonth = 0;
                            asset.ExpiryYear = 0;
                            asset.IdLg = 0;
                            asset.IdLiquidClass = 0;
                            asset.MarginStyle = PrismaMarginStyleEnum.NA;
                            asset.Multiplier = 0;
                            asset.TickSize = 0;
                            asset.TickValue = 0;
                        }
                        assetParameters.Add(asset);
                    }
                    m_AssetParameters = assetParameters;
                }
                else
                {
                    m_AssetParameters = new List<PrismaAsset>();
                }
                #endregion Paramètres sur les assets

                #region Liquidation Group Split des assets
                if (pFilePrismaData.AssetLGSData != default(List<RiskDataPrismaAssetLGS>))
                {
                    m_AssetLiquidGroupSplitParameters = pFilePrismaData.AssetLGSData.Select(v => new PrismaAssetLiquidGroupSplit
                    {
                        IdAsset = v.IdAsset,
                        IdLgs = m_LiquidationGroupSplitParameters.FirstOrDefault(l => l.Identifier == v.LiquidationGroupSplit).IdLgs,
                        IdSerie = v.IdAsset,
                        IsDefault = v.IsDefault,
                    }).ToList();
                }
                else
                {
                    m_AssetLiquidGroupSplitParameters = new List<PrismaAssetLiquidGroupSplit>();
                }
                #endregion Liquidation Group Split des assets

                #region Risk Measure Set des assets
                if (pFilePrismaData.AssetLGSRMSData != default(List<RiskDataPrismaAssetLGSRMS>))
                {
                    m_AssetRMSLGSParameters = pFilePrismaData.AssetLGSRMSData.Select(v => new PrismaAssetRMSLGS
                    {
                        IdAsset = v.IdAsset,
                        IdAssetRmsLgs = v.IdAssetRmsLgs,
                        IdFx = m_ExchangeRateParameters.FirstOrDefault(l => l.Identifier == v.FXSet).IdFx,
                        IdRmsLgs = m_RiskMeasureSetParameters.FirstOrDefault(l => (l.Identifier == v.RiskMeasureSet)
                            && (l.IdLgs == m_LiquidationGroupSplitParameters.FirstOrDefault(s => s.Identifier == v.LiquidationGroupSplit).IdLgs)).IdRmsLgs,
                        IdSerie = v.IdAsset,
                        LiquidationHorizon = v.LiquidationHorizon,
                    }).ToList();

                    // PM 20230705 [26433] Forcer le LiquidationHorizon pour les RMS WithoutSampling
                    foreach (PrismaAssetRMSLGS assetRMSLGS in m_AssetRMSLGSParameters)
                    {
                        PrismaRiskMeasureSet rms = m_RiskMeasureSetParameters.FirstOrDefault(p => p.IdRmsLgs == assetRMSLGS.IdRmsLgs);
                        if ((rms != default)
                            && (rms.HistoricalStressed == PrismaHistoricalStressedEnum.FilteredWithoutSampling
                            || rms.HistoricalStressed == PrismaHistoricalStressedEnum.HistoricalWithoutSampling
                            || rms.HistoricalStressed == PrismaHistoricalStressedEnum.StressedWithoutSampling))
                        {
                            assetRMSLGS.LiquidationHorizon = 1;
                        }
                    }
                }
                else
                {
                    m_AssetRMSLGSParameters = new List<PrismaAssetRMSLGS>();
                }
                #endregion Risk Measure Set des assets

                #region Prix théoriques des scénarios des assets
                List<PrismaAssetPrice> assetPriceParameters = new List<PrismaAssetPrice>();
                if (pFilePrismaData.AssetSPData != default(List<RiskDataPrismaAssetSP>))
                {
                    foreach (RiskDataPrismaAssetSP priceData in pFilePrismaData.AssetSPData)
                    {
                        if (priceData.AssetLGSRMS != default(RiskDataPrismaAssetLGSRMS))
                        {
                            PrismaAssetRMSLGS assetRmsLgs = m_AssetRMSLGSParameters.FirstOrDefault(l => (l.IdAssetRmsLgs == priceData.AssetLGSRMS.IdAssetRmsLgs));

                            if (assetRmsLgs != default(PrismaAssetRMSLGS))
                            {
                                int liquidationHorizon = assetRmsLgs.LiquidationHorizon;
                                int scenario = 1;
                                PrismaAssetPrice assetPrice = default;
                                for (int i = 0; i < priceData.Prices.Count(); i += 1)
                                {
                                    int j = i % liquidationHorizon;
                                    switch (j)
                                    {
                                        case 0:
                                            assetPrice = new PrismaAssetPrice
                                            {
                                                IdAsset = priceData.AssetLGSRMS.IdAsset,
                                                IdAssetRmsLgs = priceData.AssetLGSRMS.IdAssetRmsLgs,
                                                ScenarioNumber = scenario
                                            };
                                            scenario += 1;
                                            //
                                            assetPrice.Price1 = priceData.Prices[i];
                                            break;
                                        case 1:
                                            assetPrice.Price2 = priceData.Prices[i];
                                            break;
                                        case 2:
                                            assetPrice.Price3 = priceData.Prices[i];
                                            break;
                                        case 3:
                                            assetPrice.Price4 = priceData.Prices[i];
                                            break;
                                        case 4:
                                            assetPrice.Price5 = priceData.Prices[i];
                                            break;
                                    }
                                    if (j + 1 == liquidationHorizon)
                                    {
                                        assetPriceParameters.Add(assetPrice);
                                    }
                                }
                            }
                        }
                    }
                }
                m_AssetPriceParameters = assetPriceParameters;
                #endregion Prix théoriques des scénarios des assets

                #region Compression Error des assets
                if (pFilePrismaData.AssetCEData != default(List<RiskDataPrismaAssetCE>))
                {
                    m_AssetCompressionErrorParameters = pFilePrismaData.AssetCEData.Select(v => new PrismaAssetCompressionError
                    {
                        CE1 = v.CE1,
                        CE2 = v.CE2,
                        CE3 = v.CE3,
                        CE4 = v.CE4,
                        CE5 = v.CE4,
                        Currency = v.Currency,
                        IdAsset = (v.AssetLGSRMS != default(RiskDataPrismaAssetLGSRMS)) ? v.AssetLGSRMS.IdAsset : 0,
                        IdAssetRmsLgs = (v.AssetLGSRMS != default(RiskDataPrismaAssetLGSRMS)) ? v.AssetLGSRMS.IdAssetRmsLgs : 0,
                    }).ToList();
                }
                else
                {
                    m_AssetCompressionErrorParameters = new List<PrismaAssetCompressionError>();
                }
                #endregion Compression Error des assets

                #region Value at Risk des assets
                if (pFilePrismaData.AssetVaRData != default(List<RiskDataPrismaAssetVaR>))
                {
                    m_AssetVaRParameters = pFilePrismaData.AssetVaRData.Select(v => new PrismaAssetVaR
                    {
                        Currency = v.Currency,
                        IdAsset = (v.AssetLGSRMS != default(RiskDataPrismaAssetLGSRMS)) ? v.AssetLGSRMS.IdAsset : 0,
                        IdAssetRmsLgs = (v.AssetLGSRMS != default(RiskDataPrismaAssetLGSRMS)) ? v.AssetLGSRMS.IdAssetRmsLgs : 0,
                        IdSerie = (v.AssetLGSRMS != default(RiskDataPrismaAssetLGSRMS)) ? v.AssetLGSRMS.IdAsset : 0,
                        ShortLong = v.LongShortIndicator,
                        VaRAmount = v.VaR,
                        VaRType = v.VaRType,
                    }).ToList();
                }
                else
                {
                    m_AssetVaRParameters = new List<PrismaAssetVaR>();
                }
                #endregion Value at Risk des assets
            }
        }

        #region RBM for Prisma
        // PM 20151116 [21561] Ajout pour RBM
        /// <summary>
        /// Calcul les montants de déposit avec la méthode RBM pour la position pour laquelle le calcul avec la méthode Prisma n'a pas été réalisé.
        /// </summary>
        /// <param name="pActorId">L'acteur de la position à évaluer</param>
        /// <param name="pBookId">Le book de la position à évaluer</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="pPositionsToEvaluate">La position pour laquelle calculer le déposit</param>
        /// <param name="pPrismaRiskAmounts">Montant de déposit déjà calculé avec la méthode Prisma</param>
        /// <param name="pPrismaComObj">Journal du calcul réalisé avec le méthode Prisma</param>
        /// <returns>Les montants de déposit correspondants à la position</returns>
        /// FI 20160613 [22256] Add parameter pDepositHierarchyClass
        // PM 20181002 [XXXXX] Désactivation de RBM dans Prisma
        //private List<Money> EvaluateRiskRbm(
        //    int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
        //    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
        //    List<Money> pPrismaRiskAmounts, PrismaCalcMethCom pPrismaComObj)
        //{
        //    if (pPrismaComObj != default(PrismaCalcMethCom))
        //    {
        //        PrismaLiquidGroupCom[] lgCom = (PrismaLiquidGroupCom[])(pPrismaComObj.Parameters);
        //        if ((lgCom != default(PrismaLiquidGroupCom[])) && (lgCom.Count() > 0))
        //        {
        //            IEnumerable<PrismaLiquidGroupCom> lgMissing = lgCom.Where(l => l.Missing);
        //            if (lgMissing.Count() > 0)
        //            {
        //                // Recherche de la position pour laquelle il n'y a pas eu de calcul Prisma
        //                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions
        //                    = from lg in lgMissing
        //                      from pos in lg.Positions
        //                        select pos;

        //                // Lancement du calcul RBM
        //                IMarginCalculationMethodCommunicationObject opRbmMethodComObj = null;
        //                List<Money> rbmAmounts = TimsMethod.EvaluateRiskElementRbmForPrisma(pActorId, pBookId, pDepositHierarchyClass, positions, out opRbmMethodComObj);

        //                if (rbmAmounts != default)
        //                {
        //                    // Journal du calcul RBM
        //                    TimsEurexMarginCalculationMethodCommunicationObject rbmMethodComObj = (TimsEurexMarginCalculationMethodCommunicationObject)opRbmMethodComObj;

        //                    // Ajout du journal du calcul RBM au journal du calcul Prisma
        //                    pPrismaComObj.RbmMethComObj = rbmMethodComObj;

        //                    // Cumul montants RBM + Prisma par devise
        //                    rbmAmounts = rbmAmounts.Concat(pPrismaRiskAmounts).ToList();
        //                    pPrismaRiskAmounts = (
        //                        from amount in rbmAmounts
        //                        where (amount != default)
        //                        group amount by amount.Currency into amountByCurrency
        //                        select new Money(amountByCurrency.Sum(a => a.Amount.DecValue), amountByCurrency.Key)
        //                        ).ToList();

        //                    // Test si un calcul RBM a pu avoir lieu
        //                    if ((rbmMethodComObj != default(TimsEurexMarginCalculationMethodCommunicationObject))
        //                        && (rbmMethodComObj.Parameters != default(IRiskParameterCommunicationObject[]))
        //                        && (rbmMethodComObj.Parameters.Count() > 0))
        //                    {
        //                        // Ensemble des Assets traités par RBM
        //                        IEnumerable<int> rbmAssets
        //                            = from groupParam in (TimsEurexGroupParameterCommunicationObject[])rbmMethodComObj.Parameters
        //                              where (groupParam.Positions != default)
        //                              from position in groupParam.Positions
        //                              select position.First.idAsset;

        //                        // Retirer les positions du journal Prisma des positions dont les paramètres sont manquants
        //                        foreach (PrismaLiquidGroupCom lg in lgMissing)
        //                        {
        //                            lg.Positions = lg.Positions.Where(p => (false == rbmAssets.Contains(p.First.idAsset)));
        //                            foreach (PrismaLiquidGroupSplitCom lgs in lg.Parameters)
        //                            {
        //                                lgs.Positions = lgs.Positions.Where(p => (false == rbmAssets.Contains(p.First.idAsset)));
        //                                if (lgs.Positions.Count() == 0)
        //                                {
        //                                    lg.Parameters = lg.Parameters.Where(l => (l != lgs)).ToArray();
        //                                }
        //                            }
        //                            if (lg.Positions.Count() == 0)
        //                            {
        //                                pPrismaComObj.Parameters = lgCom.Where(l => (l != lg)).ToArray();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return pPrismaRiskAmounts;
        //}
        #endregion RBM for Prisma
        #endregion methods
    }

    /// <summary>
    /// Représente pour un asset, certains résultats intermédaires évalués pendant l'évaluation du LiquidityRisk 
    /// <para></para>
    /// </summary>
    public sealed class PrismaAssetLiquidityRisk
    {
        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        public int idAsset;

        /// <summary>
        /// Trade Unit de l'asset
        /// </summary>
        public decimal tradeUnit;

        /// <summary>
        /// Net Gross Ratio
        /// </summary>
        public decimal netGrossRatio;

        /// <summary>
        /// Liquidity Factor
        /// </summary>
        public decimal liquidityFactor;

        /// <summary>
        /// Liquidity Premium
        /// </summary>
        public decimal liquidityPremium;

        /// <summary>
        /// Risk Measure
        /// </summary>
        public decimal riskMeasure;

        /// <summary>
        /// Liquidity Adjustment
        /// </summary>
        public decimal liquidityAdjustment;

        /// <summary>
        /// Additional Risk Measure
        /// </summary>
        public decimal additionalRiskMeasure;

    }
}

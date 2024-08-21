namespace EFS.SpheresRiskPerformance.RiskMethods
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text.RegularExpressions;
    //
    using EFS.ACommon;
    using EFS.ApplicationBlocks.Data;
    using EFS.Common;
    using EFS.Spheres.DataContracts;
    using EFS.SpheresRiskPerformance.CommunicationObjects;
    using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
    using EFS.SpheresRiskPerformance.Enum;
    //
    using EfsML.Business;
    using EfsML.Enum;
    using EfsML.Enum.Tools;
    //
    using FixML.Enum;
    using FixML.v50SP1.Enum;
    //
    using FpML.Enum;
    using FpML.v44.Shared;

    /// <summary>
    /// Class de représentation et manipulation des matrices de risque
    /// <para>TODO : Il faudrait refactorer le code pour utiliser cette class partout où sont utiliser les matrices de risque (qui sont généralement représentée sous la forme de "Dictionary&lt;int, decimal&gt;"</para>
    /// </summary>
    /// PM 20150930 [21134] New
    internal sealed class SpanMatrice : ICloneable
    {
        #region members
        // EG 20160404 Migration vs2013
        private readonly Dictionary<int, decimal> m_RiskValue = null;
        #endregion members

        #region constructors
        /// <summary>
        /// Construit un nouveau SpanMatrice vide
        /// </summary>
        public SpanMatrice()
        {
            m_RiskValue = new Dictionary<int, decimal>();
        }

        /// <summary>
        /// Construit un nouveau SpanMatrice
        /// </summary>
        /// <param name="pRiskValue">Valeur de la matrice</param>
        public SpanMatrice(Dictionary<int, decimal> pRiskValue)
        {
            if (pRiskValue != default)
            {
                m_RiskValue = pRiskValue.ToDictionary(v => v.Key, v => v.Value);
            }
            else
            {
                m_RiskValue = new Dictionary<int, decimal>();
            }
        }
        #endregion constructors

        #region ICloneable
        /// <summary>
        /// Clone le SpanMatrice
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new SpanMatrice(m_RiskValue);
        }
        #endregion ICloneable

        #region methods
        /// <summary>
        /// Ajoute une valeur à un élément de la matrice
        /// </summary>
        /// <param name="pKeyValue"></param>
        private void Add(KeyValuePair<int, decimal> pKeyValue)
        {
            Add(pKeyValue.Key, pKeyValue.Value);
        }

        /// <summary>
        /// Ajoute une valeur à un élément de la matrice
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pValue"></param>
        public void Add(int pKey, decimal pValue)
        {
            if (m_RiskValue.ContainsKey(pKey))
            {
                m_RiskValue[pKey] += pValue;
            }
            else
            {
                m_RiskValue.Add(pKey, pValue);
            }
        }

        /// <summary>
        /// Fusionne une matrice avec la matrice courante
        /// </summary>
        /// <param name="pRiskValue"></param>
        public void Add(SpanMatrice pRiskValue)
        {
            if (default(SpanMatrice) != pRiskValue)
            {
                foreach (var riskValue in pRiskValue.m_RiskValue)
                {
                    Add(riskValue);
                }
            }
        }

        /// <summary>
        /// Retourne la valeur maximum de la matrice
        /// </summary>
        /// <returns></returns>
        public decimal? Max()
        {
            if (m_RiskValue.Values.Count > 0)
            {
                return m_RiskValue.Values.Max();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Crée un nouveau SpanMatrice en appliquant un facteur sur chaque élément
        /// </summary>
        /// <param name="pFactor"></param>
        /// <returns></returns>
        public SpanMatrice ApplyFactor(decimal pFactor)
        {
            SpanMatrice riskValue = new SpanMatrice(m_RiskValue.ToDictionary(v => v.Key, v => v.Value * pFactor));
            return riskValue;
        }

        /// <summary>
        /// Fusionne deux SpanMatrice et retourne le résultat de la fusion
        /// </summary>
        /// <param name="pRiskValue1"></param>
        /// <param name="pRiskValue2"></param>
        /// <returns></returns>
        public static SpanMatrice Add(SpanMatrice pRiskValue1, SpanMatrice pRiskValue2)
        {
            SpanMatrice sumSpanMatrice = new SpanMatrice();
            if ((default(SpanMatrice) != pRiskValue1) && (default(SpanMatrice) != pRiskValue2))
            {
                sumSpanMatrice = (SpanMatrice)pRiskValue1.Clone();
                sumSpanMatrice.Add(pRiskValue2);
            }
            return sumSpanMatrice;
        }
        #endregion methods
    }

    /// <summary>
    /// Class d'évaluation du risque sur un Exchange Complex
    /// </summary>
    internal sealed class SpanExchangeComplexRisk
    {
        #region members
        /// <summary>
        /// Paramètres de calcul du l'Echange Complex
        /// </summary>
        public SpanExchangeComplex ExchangeComplexParameters = null;
        /// <summary>
        /// Groupes combinés faisant partie de l'Exchange Complex
        /// </summary>
        public List<SpanCombinedGroupRisk> CombinedGroupOfExchange = null;
        /// <summary>
        /// All the Super Inter Commodity Spread done on the Exchange Complex
        /// </summary>
        public SpanInterCommoditySpreadCom[] SuperInterCommoditySpread = null;
        /// <summary>
        /// All the Inter Commodity Spread done on the Exchange Complex
        /// </summary>
        public SpanInterCommoditySpreadCom[] InterCommoditySpread = null;
        /// <summary>
        /// All the Inter Exchange Spread done on the Exchange Complex
        /// </summary>
        public SpanInterCommoditySpreadCom[] InterExchangeSpread = null;
        /// <summary>
        /// One Factor Credit on the Exchange Complex
        /// </summary>
        /// PM 20150930 [21134] Add OneFactorCredit
        public SpanOneFactorCreditCom OneFactorCredit = null;
        #endregion
    }

    /// <summary>
    /// Class d'évaluation du risque sur un groupe combiné de groupe de contrats
    /// </summary>
    internal sealed class SpanCombinedGroupRisk
    {
        #region members
        /// <summary>
        /// Paramètres de calcul concernant ce groupe combiné
        /// </summary>
        public SpanCombinedGroup CombinedGroupParameters = null;
        /// <summary>
        /// Ensemble des groupes de contrat faisant partie de ce groupe combiné
        /// </summary>
        public List<SpanContractGroupRisk> ContractGroupOfCombinedGroup = null;
        #endregion members

        #region accessors
        /// <summary>
        /// Valeures liquidative des positions long option dans chaque devise pour ce groupe combiné
        /// </summary>
        public Money[] LongOptionValue
        {
            get
            {
                Money[] risk = null;
                if (ContractGroupOfCombinedGroup != null)
                {
                    risk = (from contract in ContractGroupOfCombinedGroup
                            group contract by contract.Currency into cg
                            select new Money(cg.Sum(c => c.LongOptionValue), cg.Key)
                           ).ToArray();
                }
                return risk;
            }
        }
        /// <summary>
        /// Valeures liquidative des positions short option dans chaque devise pour ce groupe combiné
        /// </summary>
        public Money[] ShortOptionValue
        {
            get
            {
                Money[] risk = null;
                if (ContractGroupOfCombinedGroup != null)
                {
                    risk = (from contract in ContractGroupOfCombinedGroup
                            group contract by contract.Currency into cg
                            select new Money(cg.Sum(c => c.ShortOptionValue), cg.Key)
                           ).ToArray();
                }
                return risk;
            }
        }
        /// <summary>
        /// Valeures liquidative option dans chaque devise pour ce groupe combiné
        /// </summary>
        public Money[] NetOptionValue
        {
            get
            {
                Money[] risk = null;
                if (ContractGroupOfCombinedGroup != null)
                {
                    risk = (from contract in ContractGroupOfCombinedGroup
                            group contract by contract.Currency into cg
                            select new Money(cg.Sum(c => c.OptionValue), cg.Key)
                           ).ToArray();
                }
                return risk;
            }
        }
        /// <summary>
        /// Risques de maintenance dans chaque devise pour ce groupe combiné
        /// </summary>
        public Money[] RiskMaintenance
        {
            get
            {
                Money[] risk = null;
                if (ContractGroupOfCombinedGroup != null)
                {
                    risk = (from contract in ContractGroupOfCombinedGroup
                            group contract by contract.Currency into cg
                            select new Money(cg.Sum(c => c.RiskMaintenance), cg.Key)
                           ).ToArray();
                }
                return risk;
            }
        }
        /// <summary>
        /// Risques initials dans chaque devise pour ce groupe combiné
        /// </summary>
        public Money[] RiskInitial
        {
            get
            {
                Money[] risk = null;
                if (ContractGroupOfCombinedGroup != null)
                {
                    risk = (from contract in ContractGroupOfCombinedGroup
                            group contract by contract.Currency into cg
                            select new Money(cg.Sum(c => c.RiskInitial), cg.Key)
                           ).ToArray();
                }
                return risk;
            }
        }
        /// <summary>
        /// Déposits initial requis dans chaque devise pour ce groupe combiné
        /// </summary>
        public Money[] InitialRequirement
        {
            get
            {
                Money[] risk = null;
                if (ContractGroupOfCombinedGroup != null)
                {
                    risk = (from contract in ContractGroupOfCombinedGroup
                            group contract by contract.Currency into cg
                            select new Money(System.Math.Max(0, cg.Sum(c => c.RiskInitial) - cg.Sum(c => c.OptionValue)), cg.Key)
                           ).ToArray();
                }
                return risk;
            }
        }
        /// <summary>
        /// Déposits de maintenance requis dans chaque devise pour ce groupe combiné
        /// </summary>
        public Money[] MaintenanceRequirement
        {
            get
            {
                Money[] risk = null;
                if (ContractGroupOfCombinedGroup != null)
                {
                    risk = (from contract in ContractGroupOfCombinedGroup
                            group contract by contract.Currency into cg
                            select new Money(System.Math.Max(0, cg.Sum(c => c.RiskMaintenance) - cg.Sum(c => c.OptionValue)), cg.Key)
                           ).ToArray();
                }
                return risk;
            }
        }
        /// <summary>
        /// Excedants de valeur option par rapport au risque initial dans chaque devise pour ce groupe combiné
        /// </summary>
        public Money[] ExcessInitialOptionValue
        {
            get
            {
                Money[] risk = null;
                if (ContractGroupOfCombinedGroup != null)
                {
                    risk = (from contract in ContractGroupOfCombinedGroup
                            group contract by contract.Currency into cg
                            select new Money(System.Math.Max(0, cg.Sum(c => c.OptionValue) - cg.Sum(c => c.RiskInitial)), cg.Key)
                           ).ToArray();
                }
                return risk;
            }
        }
        /// <summary>
        /// Excedants de valeur option par rapport au risque initial dans chaque devise pour ce groupe combiné
        /// </summary>
        public Money[] ExcessMaintenanceOptionValue
        {
            get
            {
                Money[] risk = null;
                if (ContractGroupOfCombinedGroup != null)
                {
                    risk = (from contract in ContractGroupOfCombinedGroup
                            group contract by contract.Currency into cg
                            select new Money(System.Math.Max(0, cg.Sum(c => c.OptionValue) - cg.Sum(c => c.RiskMaintenance)), cg.Key)
                           ).ToArray();
                }
                return risk;
            }
        }
        #endregion accessors
    }

    /// <summary>
    /// Class d'évaluation du risque sur un groupe de contrats
    /// </summary>
    // PM 20130605 [18730] Ajout héritage de SpanScanningElement
    internal sealed class SpanContractGroupRisk : SpanScanningElement
    {
        #region members
        #region private members
        private readonly IEnumerable<SpanContractRisk> m_ContractOfContractGroup; // Valeurs de risque des contrats du groupe de contrats
        // PM 20161003 [22436] Ajout m_ScanTier et m_SomTier
        private readonly IEnumerable<SpanTierMonthRisk> m_ScanTier = null; // Ensemble des Scantiers
        private readonly IEnumerable<SpanTierMonthRisk> m_SomTier = null; // Ensemble des Somtiers
        private decimal m_AdjustementFactor = 1; // Facteur d'ajustement final du risque
        private decimal m_InitialToMaintenanceRatio = 1; // Ratio de convertion entre Risk Initial et Risk Maintenance
        private readonly IEnumerable<SpanContractGroupRisk> m_SubSpanContractGroupRisk = null; // Ensemble des évaluations des groupes de contrats rassemblés sur un seul groupe lors d'un Scan Spread

        private Dictionary<int, decimal> m_RiskValueLong = null;
        private Dictionary<int, decimal> m_RiskValueShort = null;
        private string m_Currency = null;
        // PM 20161003 [22436] m_ShortOptionMinimumCharge déplacé dans class de base SpanScanningElement
        //private decimal m_ShortOptionMinimumCharge = 0;
        private decimal m_LongOptionValue = 0;
        private decimal m_ShortOptionValue = 0;
        private decimal m_ScanRiskLong = 0;
        private decimal m_ScanRiskShort = 0;
        private Dictionary<int, SpanTierMonthRisk> m_TierMonthDelta = null;
        private decimal m_InterMonthSpreadCharge = 0;
        private decimal m_DeliveryMonthCharge = 0;
        private decimal m_StrategySpreadCharge = 0;
        private decimal m_InterCommoditySpreadCredit = 0;
        private decimal m_InterExchangeSpreadCredit = 0;

        private IEnumerable<SpanInterMonthSpreadLeg> m_IntraLegDelta = null; // Delta par jambe pour le calcul des spreads inter échéance

        #region Journal de calcul
        private SpanIntraCommoditySpreadCom[] m_StrategySpreadCom = null;
        private SpanIntraCommoditySpreadCom[] m_IntraCommoditySpreadCom = null;
        private SpanDeliveryMonthChargeCom[] m_DeliveryMonthChargeCom = null;
        #endregion Journal de calcul
        #endregion private members
        #endregion members

        #region accessors
        public IEnumerable<SpanContractRisk> ContractOfContractGroup { get { return m_ContractOfContractGroup; } }

        public Dictionary<int, decimal> RiskValueLong { get { return m_RiskValueLong; } }
        public Dictionary<int, decimal> RiskValueShort { get { return m_RiskValueShort; } }
        public string Currency { get { return m_Currency; } }
        // PM 20161003 [22436] ShortOptionMinimumCharge déplacé dans class de base SpanScanningElement
        //public decimal ShortOptionMinimumCharge { get { return m_ShortOptionMinimumCharge; } }
        public decimal LongOptionValue { get { return m_LongOptionValue; } }
        public decimal ShortOptionValue { get { return m_ShortOptionValue; } }
        public decimal ScanRiskLong { get { return m_ScanRiskLong; } }
        public decimal ScanRiskShort { get { return m_ScanRiskShort; } }
        public Dictionary<int, SpanTierMonthRisk> TierMonthDelta { get { return m_TierMonthDelta; } }
        public decimal InterMonthSpreadCharge { get { return m_InterMonthSpreadCharge; } }
        public decimal DeliveryMonthCharge { get { return m_DeliveryMonthCharge; } }
        public decimal StrategySpreadCharge { get { return m_StrategySpreadCharge; } }
        public MarginWeightedRiskCalculationMethodEnum WeightedRiskMethod { get { return m_WeightedRiskMethod; } }
        public decimal NornalWeightedFuturesPriceRisk { get { return m_NormalWeighedRisk; } }
        public decimal CappedWeightedFuturesPriceRisk { get { return m_CappedWeighedRisk; } }
        public decimal DeltaNetRemaining { get { return (m_Delta != null) ? m_Delta.DeltaNetRemaining : 0; } }
        public decimal DeltaNet { get { return (m_Delta != null) ? m_Delta.DeltaNet : 0; } }

        public decimal InterCommoditySpreadCredit
        {
            get { return m_InterCommoditySpreadCredit; }
            set { m_InterCommoditySpreadCredit = value; }
        }
        public decimal InterExchangeSpreadCredit
        {
            get { return m_InterExchangeSpreadCredit; }
            set { m_InterExchangeSpreadCredit = value; }
        }

        public override decimal ScanRisk
        {
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //get { return m_SpanMethod.IsOmnibus ? ScanRiskLong + ScanRiskShort : m_ScanRisk; }
            get { return m_EvaluationData.IsOmnibus ? ScanRiskLong + ScanRiskShort : m_ScanRisk; }
        }
        public decimal OptionValue
        {
            get { return LongOptionValue + ShortOptionValue; }
        }
        public decimal TotalGroupRisk
        {
            get { return ScanRisk + StrategySpreadCharge + InterMonthSpreadCharge + DeliveryMonthCharge - InterCommoditySpreadCredit - InterExchangeSpreadCredit; }
        }
        // PM 20180829 [XXXXX] Ajout arrondi du montant Risk Maintenance
        public decimal RiskMaintenance
        {
            get { return m_SpanMethod.SpanRoundAmount(((TotalGroupRisk > ShortOptionMinimumCharge) ? TotalGroupRisk : ShortOptionMinimumCharge) * m_AdjustementFactor); }
        }
        // PM 20180829 [XXXXX] Ajout arrondi du montant Risk Maintenance
        public decimal RiskInitial
        {
            get { return m_SpanMethod.SpanRoundAmount(((TotalGroupRisk > ShortOptionMinimumCharge) ? TotalGroupRisk : ShortOptionMinimumCharge) * m_InitialToMaintenanceRatio); }
        }

        #region Journal de calcul
        public SpanIntraCommoditySpreadCom[] StrategySpreadCom { get { return m_StrategySpreadCom; } }
        public SpanIntraCommoditySpreadCom[] IntraCommoditySpreadCom { get { return m_IntraCommoditySpreadCom; } }
        public SpanDeliveryMonthChargeCom[] DeliveryMonthChargeCom { get { return m_DeliveryMonthChargeCom; } }
        #endregion Journal de calcul
        #endregion accessors

        #region constructors
        /// <summary>
        /// Construction de la classe d'évaluation du risque sur un groupe de contrat
        /// </summary>
        /// <param name="pSpanMethod">Méthode de calcul</param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pContractGroupParameters">Paramètres du groupe de contrat</param>
        /// <param name="pAssetPositionRiskValues">Valeurs de risque des asset en position</param>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //public SpanContractGroupRisk(SPANMethod pSpanMethod, SpanContractGroup pContractGroupParameters, List<SpanAssetRiskValues> pAssetPositionRiskValues)
        //: base(pSpanMethod)
        public SpanContractGroupRisk(SPANMethod pSpanMethod, SpanEvaluationData pEvaluationData, SpanContractGroup pContractGroupParameters, List<SpanAssetRiskValues> pAssetPositionRiskValues)
            : base(pSpanMethod, pEvaluationData)
        {
            m_ContractGroupParameters = pContractGroupParameters;
            m_AssetPositionRiskValues = pAssetPositionRiskValues;

            if ((null != m_SpanMethod) && (null != m_ContractGroupParameters))
            {
                if (null != m_AssetPositionRiskValues)
                {
                    // Construire la liste des valeurs de risque des contrats du groupe de contrat
                    m_ContractOfContractGroup = m_SpanMethod.BuildContractRiskFromAssetRisk(pEvaluationData, pAssetPositionRiskValues);
                }

                // Calcul des montants de risque du groupe de contrat
                EvaluateRisk();
            }
        }
        /// <summary>
        /// Construction de la classe d'évaluation du risque sur un groupe de contrat
        /// </summary>
        /// <param name="pSpanMethod">Méthode de calcul</param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pContractGroupParameters">Paramètres du groupe de contrat</param>
        /// <param name="pContractRisk">Valeurs de risque des contrats de groupe</param>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //public SpanContractGroupRisk(SPANMethod pSpanMethod, SpanContractGroup pContractGroupParameters, List<SpanContractRisk> pContractRisk)
        //: base(pSpanMethod)
        public SpanContractGroupRisk(SPANMethod pSpanMethod, SpanEvaluationData pEvaluationData, SpanContractGroup pContractGroupParameters, List<SpanContractRisk> pContractRisk)
            : base(pSpanMethod, pEvaluationData)
        {
            m_ContractGroupParameters = pContractGroupParameters;
            m_ContractOfContractGroup = pContractRisk;

            if (null != m_ContractOfContractGroup)
            {
                m_AssetPositionRiskValues = new List<SpanAssetRiskValues>();
                foreach (SpanContractRisk contractRisk in pContractRisk)
                {
                    m_AssetPositionRiskValues = m_AssetPositionRiskValues.Concat(contractRisk.AssetPositionRiskValues).ToList();
                }
            }

            if ((null != m_SpanMethod) && (null != m_ContractGroupParameters))
            {
                // Calcul des montants de risque du groupe de contrat
                EvaluateRisk();
            }
        }
        /// <summary>
        /// Construction de la classe d'évaluation du risque sur un groupe de contrat
        /// </summary>
        /// <param name="pSpanMethod"></param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pContractGroupParameters"></param>
        /// <param name="pContractRisk"></param>
        /// <param name="pScanTierRisk"></param>
        /// <param name="pSomTierRisk"></param>
        // PM 20161003 [22436] New
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //public SpanContractGroupRisk(SPANMethod pSpanMethod, SpanContractGroup pContractGroupParameters, IEnumerable<SpanContractRisk> pContractRisk, IEnumerable<SpanTierMonthRisk> pScanTierRisk, IEnumerable<SpanTierMonthRisk> pSomTierRisk)
        //: base(pSpanMethod)
        public SpanContractGroupRisk(SPANMethod pSpanMethod, SpanEvaluationData pEvaluationData, SpanContractGroup pContractGroupParameters, IEnumerable<SpanContractRisk> pContractRisk, IEnumerable<SpanTierMonthRisk> pScanTierRisk, IEnumerable<SpanTierMonthRisk> pSomTierRisk)
            : base(pSpanMethod, pEvaluationData)
        {
            m_ContractGroupParameters = pContractGroupParameters;
            m_ContractOfContractGroup = pContractRisk;
            m_ScanTier = pScanTierRisk;
            m_SomTier = pSomTierRisk;

            if (null != m_ContractOfContractGroup)
            {
                m_AssetPositionRiskValues = new List<SpanAssetRiskValues>();
                foreach (SpanContractRisk contractRisk in pContractRisk)
                {
                    m_AssetPositionRiskValues = m_AssetPositionRiskValues.Concat(contractRisk.AssetPositionRiskValues).ToList();
                }
            }

            if ((null != m_SpanMethod) && (null != m_ContractGroupParameters))
            {
                // Calcul des montants de risque du groupe de contrat
                EvaluateRisk();
            }
        }
        /// <summary>
        /// Construction de la classe d'évaluation du risque sur un groupe de contrat
        /// </summary>
        /// <param name="pSpanMethod"></param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pInterSpreadParam">Paramètres du spread donnant lieu à la création du groupe de contrat</param>
        /// <param name="pTargetContractGroupLeg">Jambe cible du spread</param>
        /// <param name="pContractGroupLeg">Toutes les jambes du spread</param>
        // PM 20170929 [23472] Ajout paramètre pSpanMethod
        //public SpanContractGroupRisk(SpanInterSpread pInterSpreadParam, Pair<SpanInterLeg, SpanContractGroupRisk> pTargetContractGroupLeg, IEnumerable<Pair<SpanInterLeg, SpanContractGroupRisk>> pContractGroupLeg)
        //    : base(null)
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //public SpanContractGroupRisk(SPANMethod pSpanMethod, SpanInterSpread pInterSpreadParam, Pair<SpanInterLeg, SpanContractGroupRisk> pTargetContractGroupLeg, IEnumerable<Pair<SpanInterLeg, SpanContractGroupRisk>> pContractGroupLeg)
        //: base(pSpanMethod)
        public SpanContractGroupRisk(SPANMethod pSpanMethod, SpanEvaluationData pEvaluationData, SpanInterSpread pInterSpreadParam, Pair<SpanInterLeg, SpanContractGroupRisk> pTargetContractGroupLeg, IEnumerable<Pair<SpanInterLeg, SpanContractGroupRisk>> pContractGroupLeg)
            : base(pSpanMethod, pEvaluationData)
        {
            if ((pInterSpreadParam != default)
                && (pTargetContractGroupLeg != default(Pair<SpanInterLeg, SpanContractGroupRisk>)))
            {
                SpanContractGroupRisk targetContractGroup = pTargetContractGroupLeg.Second;
                SpanInterLeg targetLeg = pTargetContractGroupLeg.First;
                //
                m_SpanMethod = targetContractGroup.m_SpanMethod;
                m_AdjustementFactor = targetContractGroup.m_AdjustementFactor;
                m_InitialToMaintenanceRatio = targetContractGroup.m_InitialToMaintenanceRatio;
                m_Currency = targetContractGroup.Currency;
                //
                m_ContractGroupParameters = targetContractGroup.ContractGroupParameters;
                // Ensemble des sous Groupes de Contrat du Groupe de Contrat
                m_SubSpanContractGroupRisk = from contractLeg in pContractGroupLeg
                                             select contractLeg.Second;
                // Contrat du Groupe de Contrat (si != de null)
                m_ContractOfContractGroup = (
                    from contractGroup in m_SubSpanContractGroupRisk
                    where (contractGroup.ContractOfContractGroup != null)
                    select contractGroup.ContractOfContractGroup).Aggregate((contract, next) => (contract != null) ? ((next != null) ? contract.Concat(next) : default) : next);

                // Valeur Option Long
                m_LongOptionValue = m_SubSpanContractGroupRisk.Sum(cg => cg.LongOptionValue);
                // Valeur Option Short
                m_ShortOptionValue += m_SubSpanContractGroupRisk.Sum(cg => cg.ShortOptionValue);
                // Short Option Minimum
                m_ShortOptionMinimumCharge = m_SubSpanContractGroupRisk.Sum(cg => cg.ShortOptionMinimumCharge);
                // Charge de spread intra Groupe de Contrat
                m_StrategySpreadCharge = m_SubSpanContractGroupRisk.Sum(cg => cg.StrategySpreadCharge);
                m_InterMonthSpreadCharge = m_SubSpanContractGroupRisk.Sum(cg => cg.InterMonthSpreadCharge);
                m_DeliveryMonthCharge = m_SubSpanContractGroupRisk.Sum(cg => cg.DeliveryMonthCharge);
                // Credit inter commodity
                m_InterCommoditySpreadCredit = m_SubSpanContractGroupRisk.Sum(cg => cg.m_InterCommoditySpreadCredit);

                // Matrice de risque & Deltas
                m_RiskValue = new Dictionary<int, decimal>();
                m_Delta = new SpanDelta();

                // Ajout des RiskValue et FutureMonthDelta pour le target leg
                AddScanSpread(targetContractGroup, pInterSpreadParam.IsCreditRateSeparated ? targetLeg.CreditRate : pInterSpreadParam.CreditRate, targetLeg.DeltaPerSpread / targetLeg.DeltaPerSpread);
                // Ajout des RiskValue et FutureMonthDelta pour les autres legs
                foreach (Pair<SpanInterLeg, SpanContractGroupRisk> cgl in pContractGroupLeg)
                {
                    if (cgl.First != targetLeg)
                    {
                        AddScanSpread(cgl.Second, pInterSpreadParam.IsCreditRateSeparated ? targetLeg.CreditRate : pInterSpreadParam.CreditRate, targetLeg.DeltaPerSpread / cgl.First.DeltaPerSpread);
                    }
                }
                m_FutureMonthDelta = m_FutureMonthDelta.Values.OrderBy(fm => SPANMethod.SPANMaturityMonthToDateTime(fm.Maturity)).ToDictionary(m => m.Maturity);

                // PM 20130605 [18730] add m_SpanMethod
                //m_TierMonthDelta = SpanTierMonthRisk.EvaluateTierDeltaRisk(m_SpanMethod, m_FutureMonthDelta, m_SpanMethod.m_MaturityTierParameters);
                Dictionary<int, SpanTierMonthRisk> newTierMonth = SpanTierMonthRisk.EvaluateTierDeltaRisk(m_SpanMethod, m_EvaluationData, m_FutureMonthDelta, m_SpanMethod.m_MaturityTierParameters);
                foreach (SpanTierMonthRisk tierMonth in newTierMonth.Values)
                {
                    List<Dictionary<int, decimal>> riskValue = (
                        from tier in m_TierMonthDelta
                        where (tier.Value.TierParameters.TierNumber == tierMonth.TierParameters.TierNumber)
                           && (tier.Value.TierParameters.TierType == tierMonth.TierParameters.TierType)
                        select tier.Value.RiskValue).ToList();

                    tierMonth.SetRiskValue(riskValue);
                }
                m_TierMonthDelta = newTierMonth;

                // Valeurs de risque liés à la matrice
                EvaluateRiskFromRiskValue();

                // Calcul du risque de prix future pondéré
                EvaluateWeighedRisk();
            }
        }
        #endregion constructors

        #region methods
        /// <summary>
        /// Ajout des valeurs de risque du groupe de contrat pContractGroupRisk au contrat courant en fonction d'un taux et d'un ratio
        /// </summary>
        /// <param name="pContractGroupRisk">Valeur de risque du groupe de contrat à ajouter</param>
        /// <param name="pTargetCreditRate">Taux de risque à considérer</param>
        /// <param name="pDeltaPerSpreadRatio">Ratio de delta à considérer</param>
        private void AddScanSpread(SpanContractGroupRisk pContractGroupRisk, decimal pTargetCreditRate, decimal pDeltaPerSpreadRatio)
        {
            if (pContractGroupRisk != default)
            {
                // PM 20130605 [18730] Ajout convertion des scénario de risque des assets en position
                decimal creditRate = pTargetCreditRate / 100;

                // Convertion des scénario de risque des assets en position
                pContractGroupRisk.ApplyRateToAssetRiskValue(creditRate);

                // Convertion scénario de risque du groupe de contrat
                if (pContractGroupRisk.m_RiskValue != default)
                {
                    // Convertion de la matrice
                    Dictionary<int, decimal> convertedRiskValue = (
                        from riskValue in pContractGroupRisk.m_RiskValue
                        select new
                        {
                            riskValue.Key,
                            Value = (riskValue.Value < 0) ? (riskValue.Value * creditRate) : riskValue.Value,
                        }).ToDictionary(v => v.Key, v => v.Value);

                    // Cumul des matrices
                    SPANMethod.AddRiskValues(m_RiskValue, convertedRiskValue);
                }

                // Cumul des Futures Month Delta
                if (m_FutureMonthDelta == default(Dictionary<string, SpanFutureMonthRisk>))
                {
                    // Le groupe de contrat courant n'a pas d'échéance
                    // Vérifier si le groupe de contrat passé en paramètre possède des échéances
                    if (pContractGroupRisk.FutureMonthDelta != default(Dictionary<string, SpanFutureMonthRisk>))
                    {
                        // PM 20181214 [24398] Ajout gestion credit rate
                        //IEnumerable<SpanFutureMonthRisk> futureMonthRisk = (
                        //    from futureMonth in pContractGroupRisk.FutureMonthDelta
                        //    select SpanFutureMonthRisk.CloneWithRatio(m_ContractGroupParameters, futureMonth.Value, pDeltaPerSpreadRatio));
                        IEnumerable<SpanFutureMonthRisk> futureMonthRisk = (
                            from futureMonth in pContractGroupRisk.FutureMonthDelta
                            select SpanFutureMonthRisk.CloneWithRatio(m_ContractGroupParameters, futureMonth.Value, pDeltaPerSpreadRatio, creditRate));

                        // Ajouter les échéances du groupe de contrat passé en paramètre
                        m_FutureMonthDelta = futureMonthRisk.ToDictionary(m => m.Maturity);
                    }
                }
                else
                {
                    // Parcours des échéance du groupe de contrat passé en paramètre
                    foreach (KeyValuePair<string, SpanFutureMonthRisk> futureMonth in pContractGroupRisk.FutureMonthDelta)
                    {
                        // Vérifier si le groupe de contrat courant possède l'échéance
                        if (m_FutureMonthDelta.ContainsKey(futureMonth.Key))
                        {
                            // Ajouter les échéances du groupe de contrat passé en paramètre
                            // PM 20130605 [18730] Ajouter les Valeurs de risque des Asset en position en plus des Deltas
                            //m_FutureMonthDelta[futureMonth.Key].AddSpanDeltaWithRatio(futureMonth.Value.Delta, pDeltaPerSpreadRatio);
                            m_FutureMonthDelta[futureMonth.Key].AddScanElementWithRatio(futureMonth.Value, pDeltaPerSpreadRatio, creditRate);
                        }
                        else
                        {
                            // Ajouter les échéances du groupe de contrat passé en paramètre
                            // PM 20181214 [24398] Ajout gestion credit rate
                            //m_FutureMonthDelta.Add(futureMonth.Key, SpanFutureMonthRisk.CloneWithRatio(m_ContractGroupParameters, futureMonth.Value, pDeltaPerSpreadRatio));
                            m_FutureMonthDelta.Add(futureMonth.Key, SpanFutureMonthRisk.CloneWithRatio(m_ContractGroupParameters, futureMonth.Value, pDeltaPerSpreadRatio, creditRate));
                        }
                    }
                }

                // PM 20130605 [18730] Ajout cumul des Tiers
                // Cumul des Tier
                if (m_TierMonthDelta == default(Dictionary<int, SpanTierMonthRisk>))
                {
                    // Le groupe de contrat courant n'a pas de Tier
                    // Vérifier si le groupe de contrat passé en paramètre possède des Tiers
                    if (pContractGroupRisk.TierMonthDelta != default(Dictionary<int, SpanTierMonthRisk>))
                    {
                        // PM 20181214 [24398] Ajout gestion credit rate
                        //IEnumerable<SpanTierMonthRisk> tierMonthRisk = (
                        //    from tierMonth in pContractGroupRisk.TierMonthDelta
                        //    select SpanTierMonthRisk.CloneWithRatio(m_ContractGroupParameters, tierMonth.Value, pDeltaPerSpreadRatio));
                        IEnumerable<SpanTierMonthRisk> tierMonthRisk = (
                            from tierMonth in pContractGroupRisk.TierMonthDelta
                            select SpanTierMonthRisk.CloneWithRatio(m_ContractGroupParameters, tierMonth.Value, pDeltaPerSpreadRatio, creditRate));

                        // Ajouter les Tiers du groupe de contrat passé en paramètre
                        m_TierMonthDelta = tierMonthRisk.ToDictionary(t => t.TierParameters.SPANTierId);
                    }
                }
                else
                {
                    // Parcours des Tiers du groupe de contrat passé en paramètre
                    foreach (KeyValuePair<int, SpanTierMonthRisk> tierMonth in pContractGroupRisk.TierMonthDelta)
                    {
                        // Vérifier si le groupe de contrat courant possède le Tier
                        if (m_TierMonthDelta.ContainsKey(tierMonth.Key))
                        {
                            // Ajouter les Tiers du groupe de contrat passé en paramètre
                            m_TierMonthDelta[tierMonth.Key].AddScanElementWithRatio(tierMonth.Value, pDeltaPerSpreadRatio, creditRate);
                        }
                        else
                        {
                            // Ajouter les Tiers du groupe de contrat passé en paramètre
                            // PM 20181214 [24398] Ajout gestion credit rate
                            //m_TierMonthDelta.Add(tierMonth.Key, SpanTierMonthRisk.CloneWithRatio(m_ContractGroupParameters, tierMonth.Value, pDeltaPerSpreadRatio));
                            m_TierMonthDelta.Add(tierMonth.Key, SpanTierMonthRisk.CloneWithRatio(m_ContractGroupParameters, tierMonth.Value, pDeltaPerSpreadRatio, creditRate));
                        }
                    }
                }

                // Cumul des Delta
                m_Delta.AddWithRatio(pContractGroupRisk.Delta, pDeltaPerSpreadRatio);
            }
        }
        /// <summary>
        /// Consomme (réduit la quantité disponible) des delta du Contract Group dans son ensemble.
        /// </summary>
        /// <param name="pDelta">Delta à consommer</param>
        /// <returns>Delta réellement consommé</returns>
        public override decimal ConsumeDelta(decimal pDelta)
        {
            return ConsumeDelta(default(string), pDelta);
        }
        /// <summary>
        /// Consomme (réduit la quantité disponible) des delta sur le Tier indiqué du Contract Group.
        /// S'il s'agit du Tier 0 ou que le tier n'existe pas, les delta seront consommés sur le Contract Group dans son ensemble.
        /// </summary>
        /// <param name="pSPANTierId">Identifiant du Tier (Groupe de maturity) sur lequel consommer les deltas</param>
        /// <param name="pDelta">Delta à consommer</param>
        /// <returns>Delta réellement consommé</returns>
        public decimal ConsumeDelta(int pSPANTierId, decimal pDelta)
        {
            decimal deltaConsumed = 0;
            if (pDelta != 0)
            {
                if ((pSPANTierId != 0) && (TierMonthDelta != null))
                {
                    if (TierMonthDelta.ContainsKey(pSPANTierId))
                    {
                        SpanTierMonthRisk tier = TierMonthDelta[pSPANTierId];
                        if (tier != default)
                        {
                            deltaConsumed = tier.ConsumeDelta(pDelta);
                            deltaConsumed = this.Delta.Consume(deltaConsumed);
                        }
                    }
                }
                else
                {
                    // Utiliser les deltas du group de contrat dans son ensemble
                    deltaConsumed = ConsumeDelta(pDelta);
                }
            }
            return (deltaConsumed);
        }
        /// <summary>
        /// Consomme (réduit la quantité disponible) des delta sur la maturity indiquée du Contract Group.
        /// Si la maturity n'est pas renseignée, les delta seront consommés sur le Contract Group dans son ensemble.
        /// </summary>
        /// <param name="pMaturity">Maturity sur laquelle consommer les deltas</param>
        /// <param name="pDelta">Delta à consommer</param>
        /// <returns>Delta réellement consommé</returns>
        public decimal ConsumeDelta(string pMaturity, decimal pDelta)
        {
            decimal deltaConsumed = 0;
            if (pDelta != 0)
            {
                if (StrFunc.IsFilled(pMaturity) && (FutureMonthDelta != null))
                {
                    // Utiliser les deltas du groupe d'échéance et/ou de l'échéance

                    SpanTierMonthRisk tier = default;

                    // PM 20130404 [18555]
                    // Impacter tous les Tiers contenant l'échéance et pas uniquement le premier
                    //if (TierMonthDelta != null)
                    //{
                    //    tier = TierMonthDelta.FirstOrDefault(t => t.Value.ContainsMaturity(pMaturity)).Value;
                    //}
                    //if (tier != default)
                    //{
                    //    deltaConsumed = tier.ConsumeDelta(pMaturity, pDelta);
                    //}
                    List<SpanTierMonthRisk> tierImpacted = (
                        from tierMonth in TierMonthDelta
                        where tierMonth.Value.ContainsMaturity(pMaturity)
                        select tierMonth.Value).ToList();

                    if (tierImpacted.Count > 0)
                    {
                        tier = tierImpacted.First();
                        deltaConsumed = tier.ConsumeDelta(pMaturity, pDelta, false);
                        tierImpacted.Remove(tier);
                        foreach (SpanTierMonthRisk tierNext in tierImpacted)
                        {
                            tierNext.ConsumeDelta(pDelta, true);
                        }
                    }
                    else
                    {
                        if (FutureMonthDelta.ContainsKey(pMaturity))
                        {
                            SpanFutureMonthRisk futureMonthRisk = FutureMonthDelta[pMaturity];
                            if (futureMonthRisk != default(SpanFutureMonthRisk))
                            {
                                deltaConsumed = futureMonthRisk.ConsumeDelta(pDelta);
                            }
                        }
                    }
                }
                else
                {
                    // Repport sur toutes les échéances et tous les tiers
                    decimal deltaToUse = pDelta;
                    decimal deltaFut = 0;

                    foreach (SpanFutureMonthRisk futureMonth in FutureMonthDelta.Values)
                    {
                        if (deltaToUse == 0)
                        {
                            break;
                        }
                        if (deltaToUse > 0)
                        {
                            deltaFut = System.Math.Min(deltaToUse, futureMonth.Delta.DeltaLongRemaining);
                        }
                        else
                        {
                            deltaFut = System.Math.Max(deltaToUse, futureMonth.Delta.DeltaShortRemaining);
                        }
                        if (deltaFut != 0)
                        {
                            foreach (SpanTierMonthRisk tierMonth in TierMonthDelta.Values)
                            {
                                if (tierMonth.ContainsMaturity(futureMonth.Maturity))
                                {
                                    tierMonth.ConsumeDelta(deltaFut, true);
                                }
                            }
                            deltaConsumed += futureMonth.ConsumeDelta(deltaFut);
                            deltaToUse -= deltaFut;
                        }
                    }
                }
                deltaConsumed = this.Delta.Consume(deltaConsumed);
            }
            return (deltaConsumed);
        }

        /// <summary>
        /// 
        /// </summary>
        public void EvaluateSpread()
        {
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //if (m_SpanMethod.IsNotOmnibus)
            if ((m_EvaluationData != default) && m_EvaluationData.IsNotOmnibus)
            {
                // Calcul de la charge pour Strategy Spread Intra Groupe de contrat
                m_StrategySpreadCom = EvaluateStrategySpreadCharge();

                // Calcul de la charge pour Spread Intra Groupe de contrat
                m_IntraCommoditySpreadCom = EvaluateInterMonthSpreadCharge();

                // Calcul de la charge de contrat en livraison
                m_DeliveryMonthChargeCom = EvaluateDeliveryMonthCharge();
            }
        }

        /// <summary>
        /// Caluler la Matrice de risk du groupe de contrat
        /// ainsi que les valeurs d'option
        /// et toutes les valeurs de risque découlant de la matrice et des paramètres Span
        /// </summary>
        private void EvaluateRisk()
        {
            m_LongOptionValue = 0;
            m_ShortOptionValue = 0;

            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //switch (m_SpanMethod.SpanAccountType)
            switch (m_EvaluationData.SpanAccountType)
            {
                case SpanAccountType.Hedger:
                case SpanAccountType.OmnibusHedger:
                    m_AdjustementFactor = m_ContractGroupParameters.HedgerAdjustementFactor;
                    m_InitialToMaintenanceRatio = m_ContractGroupParameters.HedgerInitialToMaintenanceRatio;
                    break;
                case SpanAccountType.Speculator:
                case SpanAccountType.OmnibusSpeculator:
                    m_AdjustementFactor = m_ContractGroupParameters.SpeculatorAdjustementFactor;
                    m_InitialToMaintenanceRatio = m_ContractGroupParameters.SpeculatorInitialToMaintenanceRatio;
                    break;
                case SpanAccountType.Member:
                case SpanAccountType.Normal:
                case SpanAccountType.Default:
                    m_AdjustementFactor = m_ContractGroupParameters.MemberAdjustementFactor;
                    m_InitialToMaintenanceRatio = m_ContractGroupParameters.MemberInitialToMaintenanceRatio;
                    break;
            }

            #region Matrice de risque
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //if (m_SpanMethod.IsOmnibus)
            if ((m_EvaluationData != default) && m_EvaluationData.IsOmnibus)
            {
                m_RiskValueLong = new Dictionary<int, decimal>();
                m_RiskValueShort = new Dictionary<int, decimal>();

                if (null != m_ContractOfContractGroup)
                {
                    // Matrice de risque et valeur d'option
                    foreach (SpanContractRisk contract in m_ContractOfContractGroup)
                    {
                        // Long
                        m_LongOptionValue += contract.ConvertedLongOptionValue;
                        // Short
                        m_ShortOptionValue += contract.ConvertedShortOptionValue;

                        // Cumul des ScanRisk
                        m_ScanRiskLong += contract.ScanRiskLong;
                        m_ScanRiskShort += contract.ScanRiskShort;

                        // Cumul des matrices Long
                        SPANMethod.AddRiskValues(m_RiskValueLong, contract.ConvertedRiskValueLong);
                        // Cumul des matrices Short
                        SPANMethod.AddRiskValues(m_RiskValueShort, contract.ConvertedRiskValueShort);
                    }
                }
                else
                {
                    // Pas de position : Contruction d'une matrice fictive à 0
                    m_RiskValueLong = new Dictionary<int, decimal>(16);
                    m_RiskValueShort = new Dictionary<int, decimal>(16);
                    for (int i = 1; i <= 16; i += 1)
                    {
                        m_RiskValueLong.Add(i, 0);
                        m_RiskValueShort.Add(i, 0);
                    }
                }
            }
            else
            {
                m_RiskValue = new Dictionary<int, decimal>();

                if (null != m_ContractOfContractGroup)
                {
                    // Matrice de risk et Valeur d'option
                    foreach (SpanContractRisk contract in m_ContractOfContractGroup)
                    {
                        // Long
                        m_LongOptionValue += contract.ConvertedLongOptionValue;
                        // Short
                        m_ShortOptionValue += contract.ConvertedShortOptionValue;

                        // Cumul des matrices
                        SPANMethod.AddRiskValues(m_RiskValue, contract.ConvertedRiskValue);
                    }
                }
                else
                {
                    // Pas de position : Contruction d'une matrice fictive à 0
                    m_RiskValue = new Dictionary<int, decimal>(16);
                    for (int i = 1; i <= 16; i += 1)
                    {
                        m_RiskValue.Add(i, 0);
                    }
                }
            }
            #endregion Matrice de risque

            // Si valeur de risque sur asset en position
            if (m_AssetPositionRiskValues != null)
            {
                // Currency
                // PM 20170929 [23472] m_AssetPositionRiskValues peut être vide
                //m_Currency = m_AssetPositionRiskValues.FirstOrDefault(v => v.MarginCurrency != null).MarginCurrency;
                SpanAssetRiskValues firstAssetRiskValues = m_AssetPositionRiskValues.FirstOrDefault(v => v.MarginCurrency != null);
                if (firstAssetRiskValues != default(SpanAssetRiskValues))
                {
                    m_Currency = firstAssetRiskValues.MarginCurrency;
                }

                // Short Option Minimum
                // PM 20131112 [19169][19157] Tenir compte du risk multiplier
                if (ContractGroupParameters != default(SpanContractGroup))
                {
                    // PM 20161003 [22436] Bloc Remplacé par la méthode EvaluateShortOptionMinimum();
                    //// PM 20150902 [21385] Correction calcul SOM
                    ////m_ShortOptionMinimumCharge = m_AssetPositionRiskValues.Sum(v => v.ShortOptionMinimum);
                    //if (ContractGroupParameters.ShortOptionMinimumMethod == "1")
                    //{
                    //    /* SOM method MAX */
                    //    decimal callSOM = m_AssetPositionRiskValues.Where(a => a.PutOrCall == PutOrCallEnum.Call).Sum(v => v.ShortOptionMinimum);
                    //    decimal putSOM = m_AssetPositionRiskValues.Where(a => a.PutOrCall == PutOrCallEnum.Put).Sum(v => v.ShortOptionMinimum);
                    //    m_ShortOptionMinimumCharge = System.Math.Max(callSOM, putSOM);
                    //}
                    //else
                    //{
                    //    /* SOM method GROSS */
                    //    m_ShortOptionMinimumCharge = m_AssetPositionRiskValues.Sum(v => v.ShortOptionMinimum);
                    //}

                    EvaluateShortOptionMinimum();

                    // PM 20161003 [22436] Remplacement du Short Option Minimum par la somme de ceux des SomTiers
                    if ((m_SomTier != null) && (m_SomTier.Count() > 0))
                    {
                        m_ShortOptionMinimumCharge = m_SomTier.Sum(t => t.ShortOptionMinimumCharge);
                    }

                    // FL 20171221 [23660] Mise en commentaire du calcul ci-dessous car il est dèja effectué dans la méthode
                    //  EvaluateShortOptionMinimum en ligne 1046 (10 Lignes Auparavant)
                    // m_ShortOptionMinimumCharge *= ContractGroupParameters.RiskMultiplier;
                }

                // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
                //if (m_SpanMethod.IsNotOmnibus)
                if (m_EvaluationData.IsNotOmnibus)
                {
                    // Valeurs de risque liés à la matrice
                    EvaluateRiskFromRiskValue();

                    // PM 20161003 [22436] Remplacement des TimeRisk, ScanRisk et VolatilityRisk par la somme de ceux des ScanTiers
                    if ((m_ScanTier != null) && (m_ScanTier.Count() > 0))
                    {
                        // Time Risk
                        m_TimeRisk = m_ScanTier.Sum(t => t.TimeRisk);
                        
                        // Scan Risk
                        m_ScanRisk = m_ScanTier.Sum(t => t.ScanRisk);

                        // Volatility Risk
                        m_VolatilityRisk = m_ScanTier.Sum(t => t.VolatilityRisk);
                    }

                    // Calcul des Delta
                    EvaluateDelta();

                    // Calcul du risque de prix future pondéré
                    EvaluateWeighedRisk();
                }
            }
            else
            {
                // Valeurs par défaut (0) et delta à 0
                m_Delta = new SpanDelta(0, 0);
            }
            if ((null != m_ContractGroupParameters)
                && StrFunc.IsEmpty(m_Currency))
            {
                m_Currency = m_ContractGroupParameters.MarginCurrency;
            }
        }

        /// <summary>
        /// Somme des Delta Net par Echéance Future pour obtenir le Delta Net global.
        /// </summary>
        private void SumDelta()
        {
            m_Delta = (
                from delta in FutureMonthDelta
                group delta.Value.Delta by 1 into deltaGrouped
                select new SpanDelta(
                     m_SpanMethod.SpanRoundDelta(deltaGrouped.Sum(d => d.DeltaLong)),
                     m_SpanMethod.SpanRoundDelta(deltaGrouped.Sum(d => d.DeltaShort)))
                ).FirstOrDefault();
        }

        /// <summary>
        /// Calcul des Delta par Echéance Future, par Tier (période) et global.
        /// </summary>
        private void EvaluateDelta()
        {
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //if (m_SpanMethod != null)
            //{
            //    if ((m_SpanMethod.FutureMonth != null) && (m_SpanMethod.TierMonth != null))
            if (m_EvaluationData != default)
            {
                if ((m_EvaluationData.FutureMonth != null) && (m_EvaluationData.TierMonth != null))
                {
                    // Delta par Echeance Future du groupe de contrat
                    m_FutureMonthDelta = (
                        from month in m_EvaluationData.FutureMonth
                        where month.ContractGroupParameters.SPANContractGroupId == ContractGroupParameters.SPANContractGroupId
                        orderby SPANMethod.SPANMaturityMonthToDateTime(month.Maturity)
                        select month).ToDictionary(m => m.Maturity);

                    // Delta par Tier du groupe de contrat
                    m_TierMonthDelta = (
                        from tier in m_EvaluationData.TierMonth
                        where tier.ContractGroupParameters.SPANContractGroupId == ContractGroupParameters.SPANContractGroupId
                        select tier).ToDictionary(tier => tier.TierParameters.SPANTierId);

                    // Calcul de la somme total des Delta pour le Contract Group (utilisé par les spreads InterCommodity)
                    SumDelta();
                }
            }
        }

        #region EvaluateInterMonthSpreadCharge
        /// <summary>
        /// Calcul de la charge pour spread intra groupe de produit : m_InterMonthSpreadCharge
        /// </summary>
        /// <returns>Un tableau de détails du calcul des charges pour spread intra groupe de produit</returns>
        private SpanIntraCommoditySpreadCom[] EvaluateInterMonthSpreadCharge()
        {
            SpanIntraCommoditySpreadCom[] allSpreadCom = null;
            if (null != m_ContractGroupParameters)
            {
                if (null != m_ContractGroupParameters.IntraContractSpreadMethod)
                {
                    List<SpanIntraCommoditySpreadCom> listSpreadCom = new List<SpanIntraCommoditySpreadCom>();
                    switch (IntFunc.IntValue(m_ContractGroupParameters.IntraContractSpreadMethod.Trim()))
                    {
                        case 1:
                            m_InterMonthSpreadCharge = 0;
                            break;
                        case 2:
                            EvaluateInterMonthSpreadChargeMethode2();
                            break;
                        case 3:
                            EvaluateInterMonthSpreadChargeMethode3();
                            break;
                        case 4:
                            EvaluateInterMonthSpreadChargeMethode4();
                            break;
                        case 5:
                            EvaluateInterMonthSpreadChargeMethode5();
                            break;
                        case 10:
                            // Calcul des spreads par Maturity et par Tier
                            listSpreadCom = EvaluateInterMonthSpreadChargeMethode10TableDriven();
                            break;
                        default:
                            m_InterMonthSpreadCharge = 0;
                            break;
                    }
                    if (null != listSpreadCom)
                    {
                        allSpreadCom = listSpreadCom.ToArray();
                    }
                }
            }
            return allSpreadCom;
        }

        #region Obsolete InterMonthSpreadChargeMethode
        // Les méthodes 2,3,4 et 5 de calcul du montant de spread inter échéances ne sont plus supportées dans les fichiers SPAN Expanded et Expanded Paris et pas supportées par les fichiers London SPAN
        // Les paramètres pour ces méthodes ne sont disponibles que dans les fichiers SPAN Standard qui n'est plus exploité par aucune chambre
        private void EvaluateInterMonthSpreadChargeMethode2()
        {
            // Cette méthode ne devrait plus être utilisé (ses paramètres ne sont disponibles que dans le fichier au format "Standard")
            m_InterMonthSpreadCharge = 0;
        }

        private void EvaluateInterMonthSpreadChargeMethode3()
        {
            // Cette méthode ne devrait plus être utilisé (ses paramètres ne sont disponibles que dans le fichier au format "Standard")
            m_InterMonthSpreadCharge = 0;
        }

        private void EvaluateInterMonthSpreadChargeMethode4()
        {
            // Cette méthode ne devrait plus être utilisé (ses paramètres ne sont disponibles que dans le fichier au format "Standard")
            m_InterMonthSpreadCharge = 0;
        }

        private void EvaluateInterMonthSpreadChargeMethode5()
        {
            // Cette méthode ne devrait plus être utilisé (ses paramètres ne sont disponibles que dans le fichier au format "Standard")
            m_InterMonthSpreadCharge = 0;
        }
        #endregion

        /// <summary>
        /// Calcul de la charge pour spread intra groupe de produit par la méthode 10
        /// ( SPAN CME / SPAN C21 / London SPAN )
        /// </summary>
        /// <returns>Un tableau de détails du calcul des charges pour spread intra groupe de produit</returns>
        private List<SpanIntraCommoditySpreadCom> EvaluateInterMonthSpreadChargeMethode10TableDriven()
        {
            // -------------------------------
            // Calcul des spreads par Maturity
            // -------------------------------
            List<SpanIntraCommoditySpreadCom> listSpreadCom = EvaluateInterMonthByMaturitySpreadChargeMethode10(SPANSpreadType.SerieToSerieSpreadType);
            m_InterMonthSpreadCharge = 0;

            // ---------------------------
            // Calcul des spreads par Tier
            // ---------------------------
            // Calcul du nombre de Spread pour Delta Long et du nombre de Spread pour Delta Short
            // pour chaque Jambe de Spread Intra Contract Group
            m_IntraLegDelta =
                from tier in TierMonthDelta
                join intraLegParam in m_SpanMethod.m_IntraLegParameters on tier.Key equals intraLegParam.SPANTierId
                join intraSprdParam in m_SpanMethod.m_IntraSpreadParameters on intraLegParam.SPANIntraSpreadId equals intraSprdParam.SPANIntraSpreadId
                where (intraLegParam.DeltaPerSpread != 0) && (intraSprdParam.SpreadType != SPANSpreadType.StrategySpreadType)
                select new SpanInterMonthSpreadLeg
                {
                    IntraLegParameters = intraLegParam,
                    Tier = tier.Value,
                    Month = null,
                };

            // Selection des Spread Intra Contract Group ayant des Delta par ordre de priorité
            var intraSpread =
                from intraSprdParam in m_SpanMethod.m_IntraSpreadParameters
                join leg in m_IntraLegDelta on intraSprdParam.SPANIntraSpreadId equals leg.IntraLegParameters.SPANIntraSpreadId
                where (intraSprdParam.SPANContractGroupId == ContractGroupParameters.SPANContractGroupId) && (intraSprdParam.SpreadType != SPANSpreadType.StrategySpreadType)
                group intraSprdParam by intraSprdParam into intraSprd
                orderby intraSprd.Key.SpreadPriority
                select intraSprd;

            // Parcours des Spread Intra Contract Group
            foreach (var sprd in intraSpread)
            {
                decimal nbSpreadForLeg = 0;

                // Selection de toutes les Jambes du Spread Intra Contract Group courant ayant des Delta
                SpanInterMonthSpreadLeg[] legSprd = (
                   from leg in m_IntraLegDelta
                   where leg.IntraLegParameters.SPANIntraSpreadId == sprd.Key.SPANIntraSpreadId
                   orderby leg.IntraLegParameters.LegNumber
                   select leg).ToArray();

                // Vérifier que toutes les jambes du spread on des Delta
                if (legSprd.Count() == sprd.Key.NumberOfLeg)
                {
                    // Faire deux passe : 1 pour Long = "A" et 1 pour Long = "B"
                    foreach (string longSide in new string[] { "A", "B" })
                    {
                        decimal nbSpreadPossible = 0;

                        // Parcours des Jambes du Spread Intra Contract Group courant
                        foreach (SpanInterMonthSpreadLeg leg in legSprd)
                        {
                            // Calcul du nombre de spread pour le side
                            // Long = side ("A" ou "B")
                            if (leg.IntraLegParameters.LegSide.ToUpper() == longSide)
                            {
                                nbSpreadForLeg = m_SpanMethod.SpanRoundDelta(leg.NumberSpreadLong);
                            }
                            else
                            {
                                nbSpreadForLeg = m_SpanMethod.SpanRoundDelta(leg.NumberSpreadShort);
                            }
                            // Calcul du nombre de spread possible
                            if (1 == leg.IntraLegParameters.LegNumber)
                            {
                                nbSpreadPossible = nbSpreadForLeg;
                            }
                            else
                            {
                                nbSpreadPossible = System.Math.Min(nbSpreadPossible, nbSpreadForLeg);
                            }
                        }

                        if (nbSpreadPossible > 0)
                        {
                            List<SpanIntraCommoditySpreadLegCom> spreadLegCom = new List<SpanIntraCommoditySpreadLegCom>();

                            SpanIntraCommoditySpreadCom intraSpreadCom = new SpanIntraCommoditySpreadCom
                            {
                                SpreadPriority = sprd.Key.SpreadPriority,
                                NumberOfLeg = sprd.Key.NumberOfLeg,
                                ChargeRate = sprd.Key.ChargeRate,
                                NumberOfSpread = nbSpreadPossible
                            };
                            // PM 20131112 [19169][19157] Tenir compte du risk multiplier
                            if (ContractGroupParameters != default(SpanContractGroup))
                            {
                                intraSpreadCom.ChargeRate *= ContractGroupParameters.RiskMultiplier;
                            }

                            // Le Spread peut être formé.
                            foreach (SpanInterMonthSpreadLeg leg in legSprd)
                            {
                                SpanIntraCommoditySpreadLegCom intraSpreadLegCom = new SpanIntraCommoditySpreadLegCom
                                {
                                    DeltaConsumed = nbSpreadPossible * leg.IntraLegParameters.DeltaPerSpread,
                                    LegNumber = leg.IntraLegParameters.LegNumber,
                                    LegSide = leg.IntraLegParameters.LegSide,
                                    Maturity = leg.IntraLegParameters.MaturityMonthYear,
                                    DeltaPerSpread = leg.IntraLegParameters.DeltaPerSpread,
                                    DeltaLong = leg.Tier.Delta.DeltaLongRemaining,
                                    DeltaShort = leg.Tier.Delta.DeltaShortRemaining,
                                    TierNumber = leg.Tier.TierParameters.TierNumber,
                                    AssumedLongSide = longSide
                                };

                                // Long = side ("A" ou "B")
                                if (leg.IntraLegParameters.LegSide.ToUpper() != longSide)
                                {
                                    intraSpreadLegCom.DeltaConsumed *= -1;
                                }
                                // Consommer les Delta sur la jambe
                                leg.ConsumeDelta(intraSpreadLegCom.DeltaConsumed);

                                spreadLegCom.Add(intraSpreadLegCom);
                            }
                            intraSpreadCom.SpreadLeg = spreadLegCom.ToArray();
                            intraSpreadCom.SpreadCharge = intraSpreadCom.ChargeRate * nbSpreadPossible;

                            listSpreadCom.Add(intraSpreadCom);
                        }
                    }
                }
            }

            // Somme finale de la charge pour l'ensemble des spreads
            m_InterMonthSpreadCharge += listSpreadCom.Sum(s => s.SpreadCharge);
            m_InterMonthSpreadCharge = m_SpanMethod.SpanRoundAmount(m_InterMonthSpreadCharge);

            if (listSpreadCom.Count == 0)
                listSpreadCom = null;

            return (listSpreadCom);
        }
        #endregion EvaluateInterMonthSpreadCharge
        #region EvaluateDeliveryMonthCharge
        /// <summary>
        /// Calcul de la charge pour échéance rapprochée : m_DeliveryMonthCharge
        /// </summary>
        /// <returns>Un tableau de détails du calcul des charges pour échéance rapprochée</returns>
        private SpanDeliveryMonthChargeCom[] EvaluateDeliveryMonthCharge()
        {
            SpanDeliveryMonthChargeCom[] deliveryCom = null;

            if (null != m_ContractGroupParameters)
            {
                if (null != m_ContractGroupParameters.DeliveryChargeMethod)
                {
                    switch (IntFunc.IntValue(m_ContractGroupParameters.DeliveryChargeMethod.Trim()))
                    {
                        case 1:
                            m_DeliveryMonthCharge = 0;
                            break;
                        case 2:
                            EvaluateDeliveryMonthChargeMethode2();
                            break;
                        case 3:
                            EvaluateDeliveryMonthChargeMethode3();
                            break;
                        case 4:
                            EvaluateDeliveryMonthChargeMethode4();
                            break;
                        case 5:
                            EvaluateDeliveryMonthChargeMethode2();
                            break;
                        case 6:
                            EvaluateDeliveryMonthChargeMethode6();
                            break;
                        case 7:
                            EvaluateDeliveryMonthChargeMethode7();
                            break;
                        case 8:
                            EvaluateDeliveryMonthChargeMethode8();
                            break;
                        case 10:
                            deliveryCom = EvaluateDeliveryMonthChargeMethode10();
                            break;
                        case 11:
                            EvaluateDeliveryMonthChargeMethode11();
                            break;
                        default:
                            m_DeliveryMonthCharge = 0;
                            break;
                    }
                }
            }
            return deliveryCom;
        }

        #region Obsolete DeliveryMonthChargeMethode
        // Les méthodes 2,3,4,5,6,7 et 8 de calcul du montant de livraison ne sont plus supportées dans les fichiers SPAN Expanded et Expanded Paris et pas supportées par les fichiers London SPAN
        // Les paramètres pour ces méthodes ne sont disponibles que dans les fichiers SPAN Standard qui n'est plus exploité par aucune chambre
        private void EvaluateDeliveryMonthChargeMethode2()
        {
            // Cette méthode ne devrait plus être utilisé (ses paramètres ne sont disponibles que dans le fichier au format "Standard")
            m_DeliveryMonthCharge = 0;
        }

        private void EvaluateDeliveryMonthChargeMethode3()
        {
            // Cette méthode ne devrait plus être utilisé (ses paramètres ne sont disponibles que dans le fichier au format "Standard")
            m_DeliveryMonthCharge = 0;
        }

        private void EvaluateDeliveryMonthChargeMethode4()
        {
            // Cette méthode ne devrait plus être utilisé (ses paramètres ne sont disponibles que dans le fichier au format "Standard")
            m_DeliveryMonthCharge = 0;
        }

        private void EvaluateDeliveryMonthChargeMethode6()
        {
            // Cette méthode ne devrait plus être utilisé (ses paramètres ne sont disponibles que dans le fichier au format "Standard")
            m_DeliveryMonthCharge = 0;
        }

        private void EvaluateDeliveryMonthChargeMethode7()
        {
            // Cette méthode ne devrait plus être utilisé (ses paramètres ne sont disponibles que dans le fichier au format "Standard")
            m_DeliveryMonthCharge = 0;
        }

        private void EvaluateDeliveryMonthChargeMethode8()
        {
            // Cette méthode ne devrait plus être utilisé (ses paramètres ne sont disponibles que dans le fichier au format "Standard")
            m_DeliveryMonthCharge = 0;
        }
        #endregion

        /// <summary>
        /// Calcul de la charge pour échéance rapprochée par la méthode 10
        /// ( SPAN CME / SPAN C21 / London SPAN )
        /// </summary>
        /// <returns>Un tableau de détails du calcul des charges pour échéance rapprochée</returns>
        private SpanDeliveryMonthChargeCom[] EvaluateDeliveryMonthChargeMethode10()
        {
            SpanDeliveryMonthChargeCom[] deliveryCom = null;

            // Note: pour London SPAN
            // Si delivMonthParam.DeltaSign.ToUpper() == "L" : ne prendre que les Delta Long
            // Si delivMonthParam.DeltaSign.ToUpper() == "S" : ne prendre que les Delta Short
            // Si delivMonthParam.DeltaSign.ToUpper() == "B" : prendre les Delta Long ou Short (comme dans les autres SPAN)

            deliveryCom = (from delivMonthParam in
                               (from dmParam in m_SpanMethod.m_DeliveryMonthParameters
                                where dmParam.SPANContractGroupId == this.m_ContractGroupParameters.SPANContractGroupId
                                select dmParam)
                           join futMonth in this.FutureMonthDelta on delivMonthParam.Maturity equals futMonth.Key
                           select new SpanDeliveryMonthChargeCom
                           {
                               Maturity = futMonth.Key,
                               DeltaSign = delivMonthParam.DeltaSign,
                               DeltaNetUsed = futMonth.Value.Delta.DeltaNetConsumed,
                               DeltaNetRemaining = futMonth.Value.Delta.DeltaNetRemaining,
                               ConsumedChargeRate = delivMonthParam.ConsumedChargeRate,
                               RemainingChargeRate = delivMonthParam.RemainingChargeRate,
                               DeliveryCharge = System.Math.Abs(StrFunc.IsEmpty(delivMonthParam.DeltaSign)
                                                                || (delivMonthParam.DeltaSign.ToUpper() == "B")
                                                                || ((delivMonthParam.DeltaSign.ToUpper() == "L") && (futMonth.Value.Delta.DeltaNet > 0))
                                                                || ((delivMonthParam.DeltaSign.ToUpper() == "S") && (futMonth.Value.Delta.DeltaNet < 0))
                                                                ? futMonth.Value.Delta.DeltaNetRemaining * delivMonthParam.RemainingChargeRate : 0)
                                                +
                                                System.Math.Abs(StrFunc.IsEmpty(delivMonthParam.DeltaSign)
                                                                || (delivMonthParam.DeltaSign.ToUpper() == "B")
                                                                || ((delivMonthParam.DeltaSign.ToUpper() == "L") && (futMonth.Value.Delta.DeltaNet > 0))
                                                                || ((delivMonthParam.DeltaSign.ToUpper() == "S") && (futMonth.Value.Delta.DeltaNet < 0))
                                                                ? futMonth.Value.Delta.DeltaNetConsumed * delivMonthParam.ConsumedChargeRate : 0),
                           }).ToArray();

            m_DeliveryMonthCharge = m_SpanMethod.SpanRoundAmount(deliveryCom.Sum(d => d.DeliveryCharge));

            return deliveryCom;
        }

        private void EvaluateDeliveryMonthChargeMethode11()
        {
            // TODO : Pas de documentation pour cette méthode et aucun contrat l'utilisant trouvé dans aucun fichier SPAN
            m_DeliveryMonthCharge = 0;
        }
        #endregion EvaluateDeliveryMonthCharge
        #region EvaluateStrategySpreadCharge
        /// <summary>
        /// Calcul de la charge pour spread de statégies intra groupe de produit : m_StrategySpreadCharge
        /// (London SPAN uniquement)
        /// </summary>
        /// <returns>Un tableau de détails du calcul des charges pour spread de statégies intra groupe de produit</returns>
        private SpanIntraCommoditySpreadCom[] EvaluateStrategySpreadCharge()
        {
            SpanIntraCommoditySpreadCom[] allSpreadCom = null;
            if (null != m_ContractGroupParameters)
            {
                if (null != m_ContractGroupParameters.StrategySpreadMethod)
                {
                    List<SpanIntraCommoditySpreadCom> listSpreadCom = new List<SpanIntraCommoditySpreadCom>();
                    switch (IntFunc.IntValue(m_ContractGroupParameters.StrategySpreadMethod.Trim()))
                    {
                        case 1:
                            m_StrategySpreadCharge = 0;
                            break;
                        case 10:
                            listSpreadCom = EvaluateStrategySpreadChargeMethode10();
                            break;
                        default:
                            m_StrategySpreadCharge = 0;
                            break;
                    }
                    if (null != listSpreadCom)
                    {
                        allSpreadCom = listSpreadCom.ToArray();
                    }
                }
            }
            return allSpreadCom;
        }
        /// <summary>
        /// Calcul de la charge pour spread de statégies intra groupe de produit par la méthode 10
        /// ( London SPAN )
        /// </summary>
        /// <returns>Un tableau de détails du calcul des charges pour spread de statégies intra groupe de produit</returns>
        private List<SpanIntraCommoditySpreadCom> EvaluateStrategySpreadChargeMethode10()
        {
            List<SpanIntraCommoditySpreadCom> listSpreadCom = EvaluateInterMonthByMaturitySpreadChargeMethode10(SPANSpreadType.StrategySpreadType);

            m_StrategySpreadCharge += listSpreadCom.Sum(s => s.SpreadCharge);
            m_StrategySpreadCharge = m_SpanMethod.SpanRoundAmount(m_StrategySpreadCharge);

            if (listSpreadCom.Count == 0)
                listSpreadCom = null;

            return listSpreadCom;
        }
        #endregion EvaluateStrategySpreadCharge
        #region EvaluateCommonSpreadCharge
        /// <summary>
        /// Calcul de la charge pour spread intra groupe de produit par maturity par la méthode 10
        /// </summary>
        /// <param name="pSpreadType">Type de spread ('S' => Series to Series du SPAN CME; 'L' => Strategy du SPAN LIFFE)</param>
        /// <returns></returns>
        private List<SpanIntraCommoditySpreadCom> EvaluateInterMonthByMaturitySpreadChargeMethode10(string pSpreadType)
        {
            List<SpanIntraCommoditySpreadCom> listSpreadCom = new List<SpanIntraCommoditySpreadCom>();

            // Calcul du nombre de Spread pour Delta Long et du nombre de Spread pour Delta Short
            // pour chaque Jambe de Spread Intra Contract Group
            m_IntraLegDelta =
                from month in FutureMonthDelta
                join intraLegParam in m_SpanMethod.m_IntraLegParameters on month.Key equals intraLegParam.MaturityMonthYear
                join intraSprdParam in m_SpanMethod.m_IntraSpreadParameters on intraLegParam.SPANIntraSpreadId equals intraSprdParam.SPANIntraSpreadId
                where (intraLegParam.DeltaPerSpread != 0) && (intraSprdParam.SpreadType == pSpreadType)
                select new SpanInterMonthSpreadLeg
                {
                    IntraLegParameters = intraLegParam,
                    Month = month.Value,
                    Tier = null,
                };

            // Selection des Spread Intra Contract Group ayant des Delta par ordre de priorité
            var intraSpread =
                from intraSprdParam in m_SpanMethod.m_IntraSpreadParameters
                join leg in m_IntraLegDelta on intraSprdParam.SPANIntraSpreadId equals leg.IntraLegParameters.SPANIntraSpreadId
                where (intraSprdParam.SPANContractGroupId == ContractGroupParameters.SPANContractGroupId) && (intraSprdParam.SpreadType == pSpreadType)
                group intraSprdParam by intraSprdParam into intraSprd
                orderby intraSprd.Key.SpreadPriority
                select intraSprd;

            // Parcours des Spread Intra Contract Group
            foreach (var sprd in intraSpread)
            {
                decimal nbSpreadForLeg = 0;

                // Selection de toutes les Jambes du Spread Intra Contract Group courant ayant des Delta
                SpanInterMonthSpreadLeg[] legSprd = (
                   from leg in m_IntraLegDelta
                   where leg.IntraLegParameters.SPANIntraSpreadId == sprd.Key.SPANIntraSpreadId
                   orderby leg.IntraLegParameters.LegNumber
                   select leg).ToArray();

                // Vérifier que toutes les jambes du spread on des Delta
                if (legSprd.Count() == sprd.Key.NumberOfLeg)
                {
                    // Faire deux passe : 1 pour Long = "A" et 1 pour Long = "B"
                    foreach (string longSide in new string[] { "A", "B" })
                    {
                        decimal nbSpreadPossible = 0;

                        // Parcours des Jambes du Spread Intra Contract Group courant
                        foreach (SpanInterMonthSpreadLeg leg in legSprd)
                        {
                            // Calcul du nombre de spread pour le side
                            // Long = side ("A" ou "B")
                            if (leg.IntraLegParameters.LegSide.ToUpper() == longSide)
                            {
                                nbSpreadForLeg = m_SpanMethod.SpanRoundDelta(leg.NumberSpreadLong);
                            }
                            else
                            {
                                nbSpreadForLeg = m_SpanMethod.SpanRoundDelta(leg.NumberSpreadShort);
                            }
                            // Calcul du nombre de spread possible
                            if (1 == leg.IntraLegParameters.LegNumber)
                            {
                                nbSpreadPossible = nbSpreadForLeg;
                            }
                            else
                            {
                                nbSpreadPossible = System.Math.Min(nbSpreadPossible, nbSpreadForLeg);
                            }
                        }

                        if (nbSpreadPossible > 0)
                        {
                            List<SpanIntraCommoditySpreadLegCom> spreadLegCom = new List<SpanIntraCommoditySpreadLegCom>();

                            SpanIntraCommoditySpreadCom intraSpreadCom = new SpanIntraCommoditySpreadCom
                            {
                                SpreadPriority = sprd.Key.SpreadPriority,
                                NumberOfLeg = sprd.Key.NumberOfLeg,
                                ChargeRate = sprd.Key.ChargeRate,
                                NumberOfSpread = nbSpreadPossible
                            };
                            // PM 20131112 [19169][19157] Tenir compte du risk multiplier
                            if (ContractGroupParameters != default(SpanContractGroup))
                            {
                                intraSpreadCom.ChargeRate *= ContractGroupParameters.RiskMultiplier;
                            }


                            // Le Spread peut être formé.
                            foreach (SpanInterMonthSpreadLeg leg in legSprd)
                            {
                                SpanIntraCommoditySpreadLegCom intraSpreadLegCom = new SpanIntraCommoditySpreadLegCom
                                {
                                    DeltaConsumed = nbSpreadPossible * leg.IntraLegParameters.DeltaPerSpread,
                                    LegNumber = leg.IntraLegParameters.LegNumber,
                                    LegSide = leg.IntraLegParameters.LegSide,
                                    Maturity = leg.IntraLegParameters.MaturityMonthYear,
                                    DeltaPerSpread = leg.IntraLegParameters.DeltaPerSpread,
                                    DeltaLong = leg.Month.Delta.DeltaLongRemaining,
                                    DeltaShort = leg.Month.Delta.DeltaShortRemaining,
                                    AssumedLongSide = longSide
                                };

                                // Long = side ("A" ou "B")
                                if (leg.IntraLegParameters.LegSide.ToUpper() != longSide)
                                {
                                    intraSpreadLegCom.DeltaConsumed *= -1;
                                }
                                // Consommer les Delta sur la jambe
                                this.ConsumeDelta(intraSpreadLegCom.Maturity, intraSpreadLegCom.DeltaConsumed);

                                spreadLegCom.Add(intraSpreadLegCom);
                            }
                            intraSpreadCom.SpreadLeg = spreadLegCom.ToArray();
                            intraSpreadCom.SpreadCharge = intraSpreadCom.ChargeRate * nbSpreadPossible;

                            listSpreadCom.Add(intraSpreadCom);
                        }
                    }
                }
            }
            return (listSpreadCom);
        }
        #endregion EvaluateCommonSpreadCharge
        #endregion methods

        #region Log Methods
        /// <summary>
        /// Alimente l'objet de log avec les données du groupe de contrat courant
        /// </summary>
        /// <param name="pContractGroupComObj">Objet de log à alimenter</param>
        /// <returns>Objet passé en paramètre</returns>
        public SpanContractGroupCom FillComObj(SpanContractGroupCom pContractGroupComObj)
        {
            if (pContractGroupComObj != default(SpanContractGroupCom))
            {
                pContractGroupComObj.StrategyParameters = StrategySpreadCom;
                pContractGroupComObj.IntraCommodityParameters = IntraCommoditySpreadCom;
                pContractGroupComObj.DeliveryMonthParameters = DeliveryMonthChargeCom;
                pContractGroupComObj.WeightedRiskMethod = WeightedRiskMethod;
                //
                pContractGroupComObj.LongOptionValue = new Money(LongOptionValue, Currency);
                pContractGroupComObj.ShortOptionValue = new Money(ShortOptionValue, Currency);
                pContractGroupComObj.NetOptionValue = new Money(OptionValue, Currency);
                pContractGroupComObj.ShortOptionMinimum = new Money(ShortOptionMinimumCharge, Currency);
                //
                pContractGroupComObj.DeltaNet = DeltaNet;
                pContractGroupComObj.DeltaNetRemaining = DeltaNetRemaining;
                if (FutureMonthDelta != default(Dictionary<string, SpanFutureMonthRisk>))
                {
                    pContractGroupComObj.MaturityDelta = (
                        from delta in FutureMonthDelta
                        select new SpanDeltaCom
                        {
                            Period = delta.Key,
                            DeltaNet = delta.Value.Delta.DeltaNet,
                            DeltaNetRemaining = delta.Value.Delta.DeltaNetRemaining,
                        }).ToArray();
                }
                //
                pContractGroupComObj.ActiveScenario = ActiveScenario;
                pContractGroupComObj.LongScanRiskAmount = new Money(ScanRiskLong, Currency);
                pContractGroupComObj.ShortScanRiskAmount = new Money(ScanRiskShort, Currency);
                pContractGroupComObj.ScanRiskAmount = new Money(ScanRisk, Currency);
                pContractGroupComObj.PriceRiskAmount = new Money(FuturesPriceRisk, Currency);
                pContractGroupComObj.TimeRiskAmount = new Money(TimeRisk, Currency);
                pContractGroupComObj.VolatilityRiskAmount = new Money(VolatilityRisk, Currency);
                pContractGroupComObj.NormalWeightedRiskAmount = new Money(NornalWeightedFuturesPriceRisk, Currency);
                pContractGroupComObj.CappedWeightedRiskAmount = new Money(CappedWeightedFuturesPriceRisk, Currency);
                pContractGroupComObj.WeightedRiskAmount = new Money(WeightedFuturesPriceRisk, Currency);
                pContractGroupComObj.StrategySpreadChargeAmount = new Money(StrategySpreadCharge, Currency);
                pContractGroupComObj.IntraSpreadChargeAmount = new Money(InterMonthSpreadCharge, Currency);
                pContractGroupComObj.DeliveryMonthChargeAmount = new Money(DeliveryMonthCharge, Currency);
                //
                pContractGroupComObj.RiskValueLong = RiskValueLong;
                pContractGroupComObj.RiskValueShort = RiskValueShort;
                pContractGroupComObj.RiskValue = RiskValue;
                pContractGroupComObj.InterCommodityCreditAmount = new Money(InterCommoditySpreadCredit, Currency);
                pContractGroupComObj.InterExchangeCreditAmount = new Money(InterExchangeSpreadCredit, Currency);
                //
                pContractGroupComObj.RiskInitialAmount = new Money(RiskInitial, Currency);
                pContractGroupComObj.RiskMaintenanceAmount = new Money(RiskMaintenance, Currency);
            }
            return pContractGroupComObj;
        }
        #endregion
    }

    /// <summary>
    /// Class d'évaluation du risque sur un contrat
    /// </summary>
    internal sealed class SpanContractRisk
    {
        #region members
        #region private members
        private readonly SPANMethod m_SpanMethod = null; // Méthode de calcul SPAN
        private readonly SpanContract m_ContractParameters = null; // Paramètres SPAN du groupe de contrats;
        private readonly List<SpanAssetRiskValues> m_AssetPositionRiskValues = null; // Matrice de risque de chaque asset en position
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        private readonly SpanEvaluationData m_EvaluationData = default;
        //
        private readonly Dictionary<int, decimal> m_RiskValue = null;
        private readonly Dictionary<int, decimal> m_RiskValueLong = null;
        private readonly Dictionary<int, decimal> m_RiskValueShort = null;
        private readonly Dictionary<int, Dictionary<int, decimal>> m_AssetRiskValueLong = null;
        private readonly Dictionary<int, Dictionary<int, decimal>> m_AssetRiskValueShort = null;

        private Dictionary<int, decimal> m_ConvertedRiskValue = null;
        private Dictionary<int, decimal> m_ConvertedRiskValueLong = null;
        private Dictionary<int, decimal> m_ConvertedRiskValueShort = null;

        private string m_Currency = null;
        // PM 20150902 [21385] Nom utilisé
        //private decimal m_ShortOptionMinimumCharge = 0;
        private decimal m_LongOptionValue = 0;
        private decimal m_ShortOptionValue = 0;
        private decimal m_ConvertedLongOptionValue = 0;
        private decimal m_ConvertedShortOptionValue = 0;
        private decimal m_ScanRiskLong = 0;
        private decimal m_ScanRiskShort = 0;
        #endregion private members
        #endregion members

        #region accessors
        public SpanContract ContractParameters { get { return m_ContractParameters; } }
        public List<SpanAssetRiskValues> AssetPositionRiskValues { get { return m_AssetPositionRiskValues; } }

        public string Currency { get { return m_Currency; } }
        public Dictionary<int, decimal> RiskValue { get { return m_RiskValue; } }
        public Dictionary<int, decimal> RiskValueLong { get { return m_RiskValueLong; } }
        public Dictionary<int, decimal> RiskValueShort { get { return m_RiskValueShort; } }
        public Dictionary<int, decimal> ConvertedRiskValue
        {
            get { return m_ConvertedRiskValue; }
            set { m_ConvertedRiskValue = value; }
        }
        public Dictionary<int, decimal> ConvertedRiskValueLong
        {
            get { return m_ConvertedRiskValueLong; }
            set { m_ConvertedRiskValueLong = value; }
        }
        public Dictionary<int, decimal> ConvertedRiskValueShort
        {
            get { return m_ConvertedRiskValueShort; }
            set { m_ConvertedRiskValueShort = value; }
        }

        public decimal LongOptionValue { get { return m_LongOptionValue; } }
        public decimal ShortOptionValue { get { return m_ShortOptionValue; } }
        public decimal ConvertedLongOptionValue
        {
            get { return m_ConvertedLongOptionValue; }
            set { m_ConvertedLongOptionValue = value; }
        }
        public decimal ConvertedShortOptionValue
        {
            get { return m_ConvertedShortOptionValue; }
            set { m_ConvertedShortOptionValue = value; }
        }
        public decimal ScanRiskLong { get { return m_ScanRiskLong; } }
        public decimal ScanRiskShort { get { return m_ScanRiskShort; } }
        #endregion accessors

        #region constructors

        /// <summary>
        /// Créattion d'un objet SpanContractRisk sans position associée
        /// </summary>
        /// <param name="pSpanMethod"></param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pContractParameters"></param>
        // PM 20170929 [23472] Ajout nouveau constructeur
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //public SpanContractRisk(SPANMethod pSpanMethod, SpanContract pContractParameters)
        //    : this(pSpanMethod, pContractParameters, default(List<SpanAssetRiskValues>)) {}
        public SpanContractRisk(SPANMethod pSpanMethod, SpanEvaluationData pEvaluationData, SpanContract pContractParameters)
            : this(pSpanMethod, pEvaluationData, pContractParameters, default) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSpanMethod"></param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pContractParameters"></param>
        /// <param name="pAssetPositionRiskValues"></param>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //public SpanContractRisk(SPANMethod pSpanMethod, SpanContract pContractParameters, List<SpanAssetRiskValues> pAssetPositionRiskValues)
        public SpanContractRisk(SPANMethod pSpanMethod, SpanEvaluationData pEvaluationData, SpanContract pContractParameters, List<SpanAssetRiskValues> pAssetPositionRiskValues)
        {
            m_SpanMethod = pSpanMethod;
            m_EvaluationData = pEvaluationData;
            m_ContractParameters = pContractParameters;
            // PM 20170929 [23472] La position peut être vide
            //m_AssetPositionRiskValues = pAssetPositionRiskValues;
            //if ((null != m_SpanMethod) && (null != m_ContractParameters) && (null != m_AssetPositionRiskValues))
            m_AssetPositionRiskValues = (pAssetPositionRiskValues != default(List<SpanAssetRiskValues>)) ? pAssetPositionRiskValues : new List<SpanAssetRiskValues>();
            if ((pEvaluationData != default) && (null != m_SpanMethod) && (null != m_ContractParameters))
            {
                // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
                //if (m_SpanMethod.IsOmnibus)
                if ((m_EvaluationData != default) && m_EvaluationData.IsOmnibus)
                {
                    // PM 20130212 [18415] TO DELETE
                    m_RiskValueLong = new Dictionary<int, decimal>();
                    m_RiskValueShort = new Dictionary<int, decimal>();
                    // Fin PM 20130212 [18415] TO DELETE

                    m_AssetRiskValueLong = new Dictionary<int, Dictionary<int, decimal>>();
                    m_AssetRiskValueShort = new Dictionary<int, Dictionary<int, decimal>>();
                }
                else
                {
                    m_RiskValue = new Dictionary<int, decimal>();
                }
            }

            // Calcul de tous les montants de risque du contrat
            EvaluateRisk();
        }
        #endregion constructors

        #region methods
        /// <summary>
        /// Caluler la Matrice de risk du contrat
        /// ainsi que les valeurs d'option
        /// et toutes les valeurs de risque découlant de la matrice et des paramètres Span
        /// </summary>
        private void EvaluateRisk()
        {
            m_LongOptionValue = 0;
            m_ShortOptionValue = 0;
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //if (m_SpanMethod.IsOmnibus)
            if ((m_EvaluationData != default) && m_EvaluationData.IsOmnibus)
            {
                // Matrice de risk et Valeur d'option
                foreach (var asset in m_AssetPositionRiskValues)
                {
                    if (asset.Quantity > 0)
                    {
                        m_LongOptionValue += asset.OptionValue;

                        // Cumul des matrices
                        if (false == m_AssetRiskValueLong.ContainsKey(asset.AssetId))
                        {
                            m_AssetRiskValueLong.Add(asset.AssetId, new Dictionary<int, decimal>());
                        }
                        SPANMethod.AddRiskValues(m_AssetRiskValueLong[asset.AssetId], asset.RiskValue);
                        // PM 20130212 [18415] TO DELETE
                        SPANMethod.AddRiskValues(m_RiskValueLong, asset.RiskValue);
                    }
                    else
                    {
                        m_ShortOptionValue -= asset.OptionValue;

                        // Cumul des matrices
                        if (false == m_AssetRiskValueShort.ContainsKey(asset.AssetId))
                        {
                            m_AssetRiskValueShort.Add(asset.AssetId, new Dictionary<int, decimal>());
                        }
                        SPANMethod.AddRiskValues(m_AssetRiskValueShort[asset.AssetId], asset.RiskValue);
                        // PM 20130212 [18415] TO DELETE
                        SPANMethod.AddRiskValues(m_RiskValueShort, asset.RiskValue);
                    }
                }
                foreach (Dictionary<int, decimal> riskValueLong in m_AssetRiskValueLong.Values)
                {
                    m_ScanRiskLong += riskValueLong.Max(value => value.Value);
                }
                foreach (Dictionary<int, decimal> riskValueShort in m_AssetRiskValueShort.Values)
                {
                    m_ScanRiskShort += riskValueShort.Max(value => value.Value);
                }
            }
            else
            {
                // Matrice de risk et Valeur d'option
                foreach (var asset in m_AssetPositionRiskValues)
                {
                    if (asset.Quantity > 0)
                        m_LongOptionValue += asset.OptionValue;
                    else
                        m_ShortOptionValue -= asset.OptionValue;

                    // Cumul des matrices
                    SPANMethod.AddRiskValues(m_RiskValue, asset.RiskValue);
                }
            }
            // Currency
            // PM 20170929 [23472] La position peut être vide
            //m_Currency = m_AssetPositionRiskValues.FirstOrDefault(v => v.MarginCurrency != null).MarginCurrency;
            SpanAssetRiskValues firstAssetRiskValue = m_AssetPositionRiskValues.FirstOrDefault(v => v.MarginCurrency != null);
            if (firstAssetRiskValue != default(SpanAssetRiskValues))
            {
                m_Currency = firstAssetRiskValue.MarginCurrency;
            }

            // Short Option Minimum
            // PM 20150902 [21385] Nom utilisé, le calcul se fait sur le groupe de contrat
            //m_ShortOptionMinimumCharge = m_AssetPositionRiskValues.Sum(v => v.ShortOptionMinimum);

            m_SpanMethod.ConvertContractRiskValues(this);
        }
        #endregion
    }

    /// <summary>
    /// Class des Deltas sur une période (Tier)
    /// </summary>
    // PM 20130605 [18730] Ajout héritage de SpanScanningElement
    internal sealed class SpanTierMonthRisk : SpanScanningElement
    {
        #region members
        private readonly SpanMaturityTier m_TierParameters = null;
        private readonly SpanTierMonthRisk[] m_subTierMonth = null;
        #endregion members

        #region accessors
        public SpanMaturityTier TierParameters
        {
            get { return m_TierParameters; }
        }
        public SpanTierMonthRisk[] SubTierMonth
        {
            get { return m_subTierMonth; }
        }
        #endregion accessors

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSpanMethod"></param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pContractGroupParameters"></param>
        /// <param name="pTierParameters"></param>
        /// <param name="pFutureMonth"></param>
        /// <param name="pDeltaLong"></param>
        /// <param name="pDeltaShort"></param>
        /// <param name="pDeltaLongConsumed"></param>
        /// <param name="pDeltaShortConsumed"></param>
        /// <param name="pAssetRisk"></param>
        // PM 20130605 [18730] New Constructor
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul : add pEvaluationData
        private SpanTierMonthRisk(
            SPANMethod pSpanMethod,
            SpanEvaluationData pEvaluationData,
            SpanContractGroup pContractGroupParameters,
            SpanMaturityTier pTierParameters,
            Dictionary<string, SpanFutureMonthRisk> pFutureMonth,
            decimal pDeltaLong,
            decimal pDeltaShort,
            decimal pDeltaLongConsumed,
            decimal pDeltaShortConsumed,
            List<SpanAssetRiskValues> pAssetRisk)
            : this(pSpanMethod,
                pEvaluationData,
                pContractGroupParameters,
                pTierParameters,
                pFutureMonth,
                default,
                pDeltaLong,
                pDeltaShort,
                pDeltaLongConsumed,
                pDeltaShortConsumed,
                pAssetRisk) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSpanMethod"></param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pContractGroupParameters"></param>
        /// <param name="pTierParameters"></param>
        /// <param name="pSubTierMonth"></param>
        /// <param name="pDeltaLong"></param>
        /// <param name="pDeltaShort"></param>
        /// <param name="pDeltaLongConsumed"></param>
        /// <param name="pDeltaShortConsumed"></param>
        /// <param name="pAssetRisk"></param>
        // PM 20130605 [18730] New Constructor
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul : add pEvaluationData
        private SpanTierMonthRisk(
            SPANMethod pSpanMethod,
            SpanEvaluationData pEvaluationData,
            SpanContractGroup pContractGroupParameters,
            SpanMaturityTier pTierParameters,
            SpanTierMonthRisk[] pSubTierMonth,
            decimal pDeltaLong,
            decimal pDeltaShort,
            decimal pDeltaLongConsumed,
            decimal pDeltaShortConsumed,
            List<SpanAssetRiskValues> pAssetRisk)
            : this(pSpanMethod,
                pEvaluationData,
                pContractGroupParameters,
                pTierParameters,
                default,
                pSubTierMonth,
                pDeltaLong,
                pDeltaShort,
                pDeltaLongConsumed,
                pDeltaShortConsumed,
                pAssetRisk) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSpanMethod"></param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pContractGroupParameters"></param>
        /// <param name="pTierParameters"></param>
        /// <param name="pFutureMonth"></param>
        /// <param name="pSubTierMonth"></param>
        /// <param name="pDeltaLong"></param>
        /// <param name="pDeltaShort"></param>
        /// <param name="pDeltaLongConsumed"></param>
        /// <param name="pDeltaShortConsumed"></param>
        /// <param name="pAssetRisk"></param>
        // PM 20130605 [18730] New Constructor
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul : add pEvaluationData
        private SpanTierMonthRisk(
            SPANMethod pSpanMethod,
            SpanEvaluationData pEvaluationData,
            SpanContractGroup pContractGroupParameters,
            SpanMaturityTier pTierParameters,
            Dictionary<string, SpanFutureMonthRisk> pFutureMonth,
            SpanTierMonthRisk[] pSubTierMonth,
            decimal pDeltaLong,
            decimal pDeltaShort,
            decimal pDeltaLongConsumed,
            decimal pDeltaShortConsumed,
            List<SpanAssetRiskValues> pAssetRisk)
            : base(pSpanMethod, pEvaluationData)
        {
            m_ContractGroupParameters = pContractGroupParameters;
            m_TierParameters = pTierParameters;
            m_FutureMonthDelta = pFutureMonth;
            m_subTierMonth = pSubTierMonth;
            m_Delta = new SpanDelta(pDeltaLong, pDeltaShort);
            m_Delta.Consume(pDeltaLongConsumed);
            m_Delta.Consume(pDeltaShortConsumed);
            // PM 20170929 [23472] s'assurer que pAssetRisk n'est pas null
            m_AssetPositionRiskValues = (pAssetRisk != default(List<SpanAssetRiskValues>)) ? pAssetRisk : new List<SpanAssetRiskValues>();

            if ((default(Dictionary<string, SpanFutureMonthRisk>) == m_FutureMonthDelta)
                && (default(SpanTierMonthRisk[]) != pSubTierMonth))
            {
                m_FutureMonthDelta = new Dictionary<string, SpanFutureMonthRisk>();
                foreach (SpanTierMonthRisk tierMonth in pSubTierMonth)
                {
                    if (null != tierMonth.FutureMonthDelta)
                    {
                        if (tierMonth.FutureMonthDelta.Any(fm => m_FutureMonthDelta.ContainsKey(fm.Key)))
                        {
                            foreach (KeyValuePair<string, SpanFutureMonthRisk> fm in tierMonth.FutureMonthDelta)
                            {
                                if (m_FutureMonthDelta.ContainsKey(fm.Key))
                                {
                                    m_FutureMonthDelta[fm.Key].AddSpanDelta(fm.Value.Delta);
                                }
                                else
                                {
                                    m_FutureMonthDelta.Add(fm.Key, fm.Value);
                                }
                            }
                        }
                        else
                        {
                            m_FutureMonthDelta = m_FutureMonthDelta.Values.Concat(tierMonth.FutureMonthDelta.Values).OrderBy(fm => SPANMethod.SPANMaturityMonthToDateTime(fm.Maturity)).ToDictionary(fm => fm.Maturity);
                        }
                    }
                }
            }
            // Calcul de la matrice du tier
            SumRiskValue();
            // Calcul du short option Minimum
            // PM 20161003 [22436] Ajout EvaluateShortOptionMinimum
            EvaluateShortOptionMinimum();
            // Calcul des valeurs de risque
            EvaluateRiskFromRiskValue();
            // Calcul du risque de prix future pondéré
            EvaluateWeighedRisk();
        }
        #endregion constructors

        #region static methods
        /// <summary>
        /// Calcul du Delta Net par Tier (Période) à partir des Delta Net par Future Maturity
        /// </summary>
        /// <param name="pSpanMethod">Methode de calcul SPAN utilisé</param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pFutureMonth">Dictionnaire des Delta par échéance future</param>
        /// <param name="pMaturityTierParameters"><typeparamref name="pMaturityTierParameters"/>Ensemble du paramètrage des Tier</param>
        /// <returns>Tableau de Delta par Tier</returns>
        // PM 20130605 [18730] Ajout pSpanMethod et utilisation des nouveaux constructeurs de SpanTierMonthRisk
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //public static SpanTierMonthRisk[] EvaluateTierDeltaRisk(SPANMethod pSpanMethod, SpanFutureMonthRisk[] pFutureMonth, IEnumerable<SpanMaturityTier> pMaturityTierParameters)
        public static SpanTierMonthRisk[] EvaluateTierDeltaRisk(SPANMethod pSpanMethod, SpanEvaluationData pEvaluationData, SpanFutureMonthRisk[] pFutureMonth, IEnumerable<SpanMaturityTier> pMaturityTierParameters)
        {
            IEnumerable<SpanTierMonthRisk> tierRisk = null;
            if ((pEvaluationData != default)
                && (pFutureMonth != default(SpanFutureMonthRisk[]))
                && (pMaturityTierParameters != default(IEnumerable<SpanMaturityTier>)))
            {
                // Calcul des deltas par Tier réalisé à partir des deltas NET de chaque échéance future
                // PM 20131230 [19416] Ajout vérification que futMonth.Maturity est valide (IsFilled)
                // PM 20150902 [21385] Ajout du tier sans borne contenant tous les FutureMonths
                // PM 20151127 [21571][21605] Ne pas prendre les Tier de Tier (TierNumber > 0) qui seront générés après, pour le tier sans borne contenant tous les FutureMonths.
                tierRisk =
                    from futMonth in pFutureMonth
                    join tierParam in pMaturityTierParameters on futMonth.ContractGroupParameters.SPANContractGroupId equals tierParam.SPANContractGroupId
                    where (StrFunc.IsFilled(futMonth.Maturity)
                    && StrFunc.IsFilled(tierParam.StartingMonthYear)
                    && StrFunc.IsFilled(tierParam.EndingMonthYear)
                    && SPANMethod.SPANMaturityMonthToDateTime(futMonth.Maturity) >= SPANMethod.SPANMaturityMonthToDateTime(tierParam.StartingMonthYear, SPANMethod.MaturityMonthIntervalEnum.Start)
                    && SPANMethod.SPANMaturityMonthToDateTime(futMonth.Maturity) <= SPANMethod.SPANMaturityMonthToDateTime(tierParam.EndingMonthYear, SPANMethod.MaturityMonthIntervalEnum.End))
                    || (StrFunc.IsEmpty(tierParam.StartingMonthYear) && StrFunc.IsEmpty(tierParam.EndingMonthYear)
                        && (tierParam.StartingTierNumber == 0) && (tierParam.EndingTierNumber == 0))
                    group futMonth by new { futMonth.ContractGroupParameters, tierParam }
                        into tierMonth
                        orderby tierMonth.Key.tierParam.SPANContractGroupId, tierMonth.Key.tierParam.TierNumber
                        select new SpanTierMonthRisk(
                            pSpanMethod,
                            pEvaluationData,
                            tierMonth.Key.ContractGroupParameters,
                            tierMonth.Key.tierParam,
                            tierMonth.OrderBy(fm => SPANMethod.SPANMaturityMonthToDateTime(fm.Maturity)).ToDictionary(fm => fm.Maturity),
                            tierMonth.Sum(fut => fut.Delta.DeltaNetRemaining > 0 ? fut.Delta.DeltaNetRemaining : 0),
                            tierMonth.Sum(fut => fut.Delta.DeltaNetRemaining < 0 ? fut.Delta.DeltaNetRemaining : 0),
                            tierMonth.Sum(fut => fut.Delta.DeltaNetConsumed > 0 ? fut.Delta.DeltaNetConsumed : 0),
                            tierMonth.Sum(fut => fut.Delta.DeltaNetConsumed < 0 ? fut.Delta.DeltaNetConsumed : 0),
                            SpanAssetRiskValues.Aggegate(from futM in tierMonth select futM.AssetPositionRiskValues)
                        );

                if (null != tierRisk)
                {
                    // Calcul des deltas par Tier réalisé à partir des deltas NET d'un ensemble de Tiers
                    IEnumerable<SpanTierMonthRisk> tierRiskMulti =
                        from tierParam in pMaturityTierParameters
                        where (tierParam.StartingTierNumber > 0)
                           && (tierParam.EndingTierNumber > 0)
                           && (tierParam.TierType == SPANSpreadType.SubTierType)
                        join tier in tierRisk on tierParam.SPANContractGroupId equals tier.TierParameters.SPANContractGroupId
                        where (tier.TierParameters.TierNumber >= tierParam.StartingTierNumber)
                        && (tier.TierParameters.TierNumber <= tierParam.EndingTierNumber)
                        group tier by new { tier.ContractGroupParameters, tierParam }
                            into tierIndirect
                            select new SpanTierMonthRisk(
                                pSpanMethod,
                                pEvaluationData,
                                tierIndirect.Key.ContractGroupParameters,
                                tierIndirect.Key.tierParam,
                                tierIndirect.OrderBy(tier => tier.TierParameters.TierNumber).ToArray(),
                                tierIndirect.Sum(fut => fut.Delta.DeltaLong),
                                tierIndirect.Sum(fut => fut.Delta.DeltaShort),
                                tierIndirect.Sum(fut => fut.Delta.DeltaLongConsumed),
                                tierIndirect.Sum(fut => fut.Delta.DeltaShortConsumed),
                                SpanAssetRiskValues.Aggegate(from futM in tierIndirect select futM.AssetPositionRiskValues)
                            );
                    if (null != tierRiskMulti)
                    {
                        tierRisk = tierRisk.Concat(tierRiskMulti);
                    }
                }

            }
            return tierRisk?.ToArray();
        }

        /// <summary>
        /// Calcul du Delta Net par Tier (Période) à partir des Delta Net par Future Maturity
        /// </summary>
        /// <param name="pSpanMethod">Methode de calcul SPAN utilisé</param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pFutureMonth">Dictionnaire des Delta par échéance future</param>
        /// <param name="pMaturityTierParameters"><typeparamref name="pMaturityTierParameters"/>Ensemble du paramètrage des Tier</param>
        /// <returns>Dictionnaire de Delta par Tier (clé = Id interne du tier)</returns>
        // PM 20130605 [18730] Ajout pSpanMethod et appel à la surcharge de cette méthode
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //public static Dictionary<int, SpanTierMonthRisk> EvaluateTierDeltaRisk(SPANMethod pSpanMethod, Dictionary<string, SpanFutureMonthRisk> pFutureMonth, IEnumerable<SpanMaturityTier> pMaturityTierParameters)
        public static Dictionary<int, SpanTierMonthRisk> EvaluateTierDeltaRisk(SPANMethod pSpanMethod, SpanEvaluationData pEvaluationData, Dictionary<string, SpanFutureMonthRisk> pFutureMonth, IEnumerable<SpanMaturityTier> pMaturityTierParameters)
        {
            IEnumerable<SpanTierMonthRisk> tierRisk = null;

            if ((null != pFutureMonth) && (null != pMaturityTierParameters))
            {
                SpanFutureMonthRisk[] futureMonthArray = (from futM in pFutureMonth select futM.Value).ToArray();
                tierRisk = EvaluateTierDeltaRisk(pSpanMethod, pEvaluationData, futureMonthArray, pMaturityTierParameters);
            }
            return (tierRisk?.ToDictionary(t => t.TierParameters.SPANTierId));
        }

        /// <summary>
        /// Clone un objet SpanTierMonthRisk appliquant un ratio au Delta
        /// </summary>
        /// <param name="pContractGroupParam">Groupe de contrat à utiliser comme paramètre d'appartenance de l'échéance</param>
        /// <param name="pTierSource">Objet à cloner</param>
        /// <param name="pDeltaRatio">Ratio à appliquer</param>
        /// <param name="pCreditRate">Taux à appliquer</param>
        /// <returns>Nouvelle objet cloné</returns>
        // PM 20181214 [24398] Ajout gestion credit rate
        //public static SpanTierMonthRisk CloneWithRatio(SpanContractGroup pContractGroupParam, SpanTierMonthRisk pTierSource, decimal pDeltaRatio)
        public static SpanTierMonthRisk CloneWithRatio(SpanContractGroup pContractGroupParam, SpanTierMonthRisk pTierSource, decimal pDeltaRatio, decimal pCreditRate)
        {
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //SpanTierMonthRisk cloned = new SpanTierMonthRisk(pTierSource.m_SpanMethod, pContractGroupParam, pTierSource.TierParameters, pTierSource.FutureMonthDelta, 0, 0, 0, 0, pTierSource.AssetPositionRiskValues);
            SpanTierMonthRisk cloned = new SpanTierMonthRisk(pTierSource.m_SpanMethod, pTierSource.m_EvaluationData, pContractGroupParam, pTierSource.TierParameters, pTierSource.FutureMonthDelta, 0, 0, 0, 0, pTierSource.AssetPositionRiskValues)
            {
                //
                // PM 20181214 [24398] Application du taux sur la matrice de risque au groupe d'échéances et pas de calcul de la matrice de risque à partir de celles des assets
                m_RiskValue = pTierSource.m_RiskValue
            };
            cloned.ApplyRateToRiskValue(pCreditRate);
            //
            cloned.Delta.AddWithRatio(pTierSource.Delta, pDeltaRatio);
            return cloned;
        }
        #endregion static methods

        #region methods
        /// <summary>
        /// Utilise le nombre spécifié de delta "Long" ou "Short" sur la période avec ou non repport sur chaque échéance.
        /// </summary>
        /// <param name="pDelta">Nombre de delta "Long" ou "Short" à utiliser (Positif pour "Long" et négatif pour "Short")</param>
        /// <param name="pIsConsumeTierOnly">Repport ou non sur la/les échéances</param>
        /// <returns>Nombre total de delta "Long" ou "Short" rééllement utilisé</returns>
        public decimal ConsumeDelta(decimal pDelta, bool pIsConsumeTierOnly)
        {
            return ConsumeDelta(null, pDelta, pIsConsumeTierOnly);
        }

        /// <summary>
        /// Utilise le nombre spécifié de delta "Long" ou "Short" sur la période avec repport sur chaque échéance.
        /// </summary>
        /// <param name="pDelta">Nombre de delta "Long" ou "Short" à utiliser (Positif pour "Long" et négatif pour "Short")</param>
        /// <returns>Nombre total de delta "Long" ou "Short" rééllement utilisé</returns>
        public override decimal ConsumeDelta(decimal pDelta)
        {
            return ConsumeDelta(null, pDelta, false);
        }

        /// <summary>
        /// Utilise le nombre spécifié de delta "Long" ou "Short" sur la période avec repport sur l'échéance spécifiée
        /// ou sur chaque échéance si aucune n'est spécifiée.
        /// </summary>
        /// <param name="pMaturity">Echéance sur laquelle effectuer le repport</param>
        /// <param name="pDelta">Nombre de delta "Long" ou "Short" à utiliser (Positif pour "Long" et négatif pour "Short")</param>
        /// <returns>Nombre total de delta "Long" ou "Short" rééllement utilisé</returns>
        public decimal ConsumeDelta(string pMaturity, decimal pDelta)
        {
            return ConsumeDelta(pMaturity, pDelta, false);
        }

        /// <summary>
        /// Utilise le nombre spécifié de delta "Long" ou "Short" sur la période avec ou non repport sur l'échéance spécifiée
        /// ou sur chaque échéance si aucune n'est spécifiée.
        /// </summary>
        /// <param name="pMaturity">Echéance sur laquelle effectuer le repport</param>
        /// <param name="pDelta">Nombre de delta "Long" ou "Short" à utiliser (Positif pour "Long" et négatif pour "Short")</param>
        /// <param name="pIsConsumeTierOnly">Repport ou non sur la/les échéances</param>
        /// <returns>Nombre total de delta "Long" ou "Short" rééllement utilisé</returns>
        public decimal ConsumeDelta(string pMaturity, decimal pDelta, bool pIsConsumeTierOnly)
        {
            decimal deltaConsumed = 0;
            if (pDelta != 0)
            {
                // PM 20130404 [18555] Correction de la gestion du paramètre pIsConsumeTierOnly
                if (pIsConsumeTierOnly)
                {
                    deltaConsumed = pDelta;
                }
                else if (FutureMonthDelta != null)
                {
                    // Répercuter l'utilisation de Delta du Tier sur les Deltas par échéance composant ce Tier
                    if (StrFunc.IsFilled(pMaturity))
                    {
                        // Cas où l'échéance est mentionnée
                        if (FutureMonthDelta.ContainsKey(pMaturity))
                        {
                            SpanFutureMonthRisk month = FutureMonthDelta[pMaturity];
                            if (month != default(SpanFutureMonthRisk))
                            {
                                deltaConsumed = month.ConsumeDelta(pDelta);
                            }
                        }
                    }
                    else
                    {
                        // Repport sur toutes les échéances
                        decimal deltaToUse = pDelta;
                        foreach (SpanFutureMonthRisk futureMonth in FutureMonthDelta.Values)
                        {
                            if (deltaToUse == 0)
                            {
                                break;
                            }
                            decimal deltaFut;
                            if (deltaToUse > 0)
                            {
                                deltaFut = System.Math.Min(deltaToUse, futureMonth.Delta.DeltaLongRemaining);
                            }
                            else
                            {
                                deltaFut = System.Math.Max(deltaToUse, futureMonth.Delta.DeltaShortRemaining);
                            }
                            if (deltaFut != 0)
                            {
                                deltaConsumed += futureMonth.ConsumeDelta(deltaFut);
                                deltaToUse -= deltaFut;
                            }
                        }
                    }
                }

                m_Delta.Consume(deltaConsumed);
            }
            return (deltaConsumed);
        }

        /// <summary>
        /// Indique si le groupe d'échéance courant contient l'échéance passé en paramètre
        /// </summary>
        /// <param name="pMaturity">Echéance</param>
        /// <returns>true si l'échéance est présent, sinon false</returns>
        public bool ContainsMaturity(string pMaturity)
        {
            bool containsMaturity = false;
            if (FutureMonthDelta != null)
            {
                if (FutureMonthDelta.ContainsKey(pMaturity))
                {
                    SpanFutureMonthRisk month = FutureMonthDelta[pMaturity];
                    if (month != default(SpanFutureMonthRisk))
                    {
                        containsMaturity = true;
                    }
                }
            }
            return containsMaturity;
        }

        /// <summary>
        /// Cumul les valeurs de scénario de risque pour chaque scénario est calcul les valeurs de risque dépendantes des scénarios
        /// </summary>
        /// <param name="pRiskValueList">Ensemble de valeurs de scénarios de risque</param>
        public void SetRiskValue(List<Dictionary<int, decimal>> pRiskValueList)
        {
            // Calcul des matrices
            SumRiskValue(pRiskValueList);
            // Calcul du short option Minimum
            // PM 20161003 [22436] Ajout EvaluateShortOptionMinimum
            EvaluateShortOptionMinimum();
            // Calcul des valeurs de risque
            EvaluateRiskFromRiskValue();
            // Calcul du risque de prix future pondéré
            EvaluateWeighedRisk();
        }

        /// <summary>
        /// Calcule Risque pondéré de prix future
        /// </summary>
        /// PM 20160829 [22420] New: Ajout surcharge de la méthode de base pour calcul du montant Cappé
        protected override void EvaluateWeighedRisk()
        {
            base.EvaluateWeighedRisk();

            // PM 20161003 [22436] Ajout gestion méthode ScanRange
            //if ((m_WeightedRiskMethod == MarginWeightedRiskCalculationMethodEnum.Capped) && (FutureMonthDelta.Count > 0))
            if ((MarginWeightedRiskCalculationMethodEnum.Normal != m_WeightedRiskMethod) && (FutureMonthDelta.Count > 0))
            {
                // Recherche de toutes les échéances du groupes d'échéances et calcul de leur montant de Weighed Risk cappé
                IEnumerable<Pair<string, decimal>> futureMonth;

                if ((m_TierParameters.TierType == SPANSpreadType.SubTierType) && (m_TierParameters.StartingTierNumber > 0) && (m_TierParameters.EndingTierNumber > 0))
                {
                    // Cas du Tier réalisé à partir d'un ensemble de Tiers
                    // PM 20180828 [XXXXX] Problème de parathèse dans les conditions de la requête (les échéances restituées ne se limitaient pas au groupe de contrat du Tier)
                    futureMonth =
                        from tier in m_subTierMonth
                        join contractParam in m_SpanMethod.m_ContractParameters on tier.TierParameters.SPANContractGroupId equals contractParam.SPANContractGroupId
                        join maturityParam in m_SpanMethod.m_MaturityParameters on contractParam.SPANContractId equals maturityParam.SPANContractId
                        where (contractParam.SPANContractGroupId == m_TierParameters.SPANContractGroupId)
                           && StrFunc.IsFilled(maturityParam.FutureMaturity)
                           && ( ( StrFunc.IsFilled(tier.m_TierParameters.StartingMonthYear)
                                  && StrFunc.IsFilled(tier.m_TierParameters.EndingMonthYear)
                                  && SPANMethod.SPANMaturityMonthToDateTime(maturityParam.FutureMaturity) >= SPANMethod.SPANMaturityMonthToDateTime(tier.m_TierParameters.StartingMonthYear, SPANMethod.MaturityMonthIntervalEnum.Start)
                                  && SPANMethod.SPANMaturityMonthToDateTime(maturityParam.FutureMaturity) <= SPANMethod.SPANMaturityMonthToDateTime(tier.m_TierParameters.EndingMonthYear, SPANMethod.MaturityMonthIntervalEnum.End)
                                )
                             || ( StrFunc.IsEmpty(tier.m_TierParameters.StartingMonthYear) && StrFunc.IsEmpty(tier.m_TierParameters.EndingMonthYear)
                                  && (tier.m_TierParameters.StartingTierNumber == 0) && (tier.m_TierParameters.EndingTierNumber == 0)
                                )
                              )
                        select new Pair<string, decimal>
                        {
                            First = maturityParam.FutureMaturity,
                            Second = maturityParam.FuturePriceScanRange / (maturityParam.DeltaScalingFactor * contractParam.DeltaScalingFactor),
                        };
                }
                else
                {
                    // Cas d'un Tier direct
                    // PM 20180828 [XXXXX] Problème de parathèse dans les conditions de la requête (les échéances restituées ne se limitaient pas au groupe de contrat du Tier)
                    futureMonth =
                        from maturityParam in m_SpanMethod.m_MaturityParameters
                        join contractParam in m_SpanMethod.m_ContractParameters on maturityParam.SPANContractId equals contractParam.SPANContractId
                        where (contractParam.SPANContractGroupId == m_TierParameters.SPANContractGroupId)
                           && StrFunc.IsFilled(maturityParam.FutureMaturity)
                           && ( ( StrFunc.IsFilled(m_TierParameters.StartingMonthYear)
                                  && StrFunc.IsFilled(m_TierParameters.EndingMonthYear)
                                  && SPANMethod.SPANMaturityMonthToDateTime(maturityParam.FutureMaturity) >= SPANMethod.SPANMaturityMonthToDateTime(m_TierParameters.StartingMonthYear, SPANMethod.MaturityMonthIntervalEnum.Start)
                                  && SPANMethod.SPANMaturityMonthToDateTime(maturityParam.FutureMaturity) <= SPANMethod.SPANMaturityMonthToDateTime(m_TierParameters.EndingMonthYear, SPANMethod.MaturityMonthIntervalEnum.End)
                                )
                             || ( StrFunc.IsEmpty(m_TierParameters.StartingMonthYear) && StrFunc.IsEmpty(m_TierParameters.EndingMonthYear)
                                  && (m_TierParameters.StartingTierNumber == 0) && (m_TierParameters.EndingTierNumber == 0)
                                )
                              )
                        select new Pair<string, decimal>
                        {
                            First = maturityParam.FutureMaturity,
                            Second = maturityParam.FuturePriceScanRange / (maturityParam.DeltaScalingFactor * contractParam.DeltaScalingFactor),
                        };
                }
                if ((futureMonth != null) && (futureMonth.Count() > 0))
                {
                    // Calcul du Weighed Risk à l'aide du Weighed Risk cappé de l'échéance la plus proche
                    DateTime minMaturity = futureMonth.Min(f => SPANMethod.SPANMaturityMonthToDateTime(f.First));
                    decimal cappedWeighedRisk = futureMonth.First(f => SPANMethod.SPANMaturityMonthToDateTime(f.First) == minMaturity).Second;
                    m_CappedWeighedRisk = m_SpanMethod.SpanRoundWeightedFuturePriceRisk(cappedWeighedRisk);

                    // PM 20161003 [22436] Ajout gestion méthode ScanRange
                    //m_WeighedRisk = System.Math.Min(m_CappedWeighedRisk, m_NormalWeighedRisk);
                    if (MarginWeightedRiskCalculationMethodEnum.Capped == m_WeightedRiskMethod)
                    {
                        m_WeighedRisk = System.Math.Min(m_CappedWeighedRisk, m_NormalWeighedRisk);
                    }
                    else
                    {
                        // PM 20200525 [25360] Montant Capped pour le méthode ScanRange : comme c'est le cas sur les groupes de contrats
                        //m_WeighedRisk = m_NormalWeighedRisk;
                        m_WeighedRisk = m_CappedWeighedRisk;
                    }
                }
            }
        }
        #endregion methods
    }

    /// <summary>
    /// Class des Deltas sur une échéance future
    /// </summary>
    // PM 20130605 [18730] Ajout héritage de SpanScanningElement
    internal sealed class SpanFutureMonthRisk : SpanScanningElement
    {
        #region members
        private readonly SpanMaturity m_MaturityParameters = null;
        private readonly string m_Maturity = null;
        // PM 20150902 [21385] Ajout m_CappedWeighedRisk
        // EG 20160404 Migration vs2013
        //private decimal m_CappedWeighedRisk = 0;
        #endregion members

        #region accessors
        /// <summary>
        /// Maturity Parameters
        /// </summary>
        public SpanMaturity MaturityParameters
        {
            get { return m_MaturityParameters; }
        }
        /// <summary>
        /// Maturity
        /// </summary>
        public string Maturity
        {
            get { return m_Maturity; }
        }
        /// <summary>
        /// Capped Weighed Risk
        /// </summary>
        // PM 20150902 [21385] Ajout CappedWeighedRisk
        public decimal CappedWeighedRisk
        {
            get { return m_CappedWeighedRisk; }
        }
        #endregion accessors

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSpanMethod"></param>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pContractGroupParameters"></param>
        /// <param name="pMaturity"></param>
        /// <param name="pMaturityParameters"></param>
        /// <param name="pDeltaLong"></param>
        /// <param name="pDeltaShort"></param>
        /// <param name="pCappedWeighedRisk"></param>
        /// <param name="pAssetRisk"></param>
        // PM 20130605 [18730] New Constructor
        // PM 20150902 [21385] Ajout parameter pCappedWeighedRisk
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        public SpanFutureMonthRisk(
            SPANMethod pSpanMethod,
            SpanEvaluationData pEvaluationData,
            SpanContractGroup pContractGroupParameters,
            string pMaturity,
            SpanMaturity pMaturityParameters,
            decimal pDeltaLong,
            decimal pDeltaShort,
            decimal pCappedWeighedRisk,
            List<SpanAssetRiskValues> pAssetRisk)
            : base(pSpanMethod, pEvaluationData)
        {
            m_ContractGroupParameters = pContractGroupParameters;
            m_Maturity = pMaturity;
            m_MaturityParameters = pMaturityParameters;
            m_Delta = new SpanDelta(pDeltaLong, pDeltaShort);
            m_CappedWeighedRisk = pCappedWeighedRisk;
            // PM 20170929 [23472] s'assurer que pAssetRisk n'est pas null
            m_AssetPositionRiskValues = (pAssetRisk != default(List<SpanAssetRiskValues>)) ? pAssetRisk : new List<SpanAssetRiskValues>();

            // PM 20181214 [24398] Calcul matrice de risque de l'échéance
            this.SumRiskValue();
        }
        #endregion constructor

        #region static methods
        /// <summary>
        /// Clone un objet SpanFutureMonthRisken appliquant un ratio au Delta
        /// </summary>
        /// <param name="pContractGroupParam">Groupe de contrat à utiliser comme paramètre d'appartenance de l'échéance</param>
        /// <param name="pFutureSource">Objet à cloner</param>
        /// <param name="pDeltaRatio">Ratio à appliquer</param>
        /// <param name="pCreditRate">Taux à appliquer</param>
        /// <returns>Nouvelle objet cloné</returns>
        // PM 20181214 [24398] Ajout gestion credit rate
        //public static SpanFutureMonthRisk CloneWithRatio(SpanContractGroup pContractGroupParam, SpanFutureMonthRisk pFutureSource, decimal pDeltaRatio)
        public static SpanFutureMonthRisk CloneWithRatio(SpanContractGroup pContractGroupParam, SpanFutureMonthRisk pFutureSource, decimal pDeltaRatio, decimal pCreditRate)
        {
            // PM 20130605 [18730] Add pDeltaFutureSource.m_SpanMethod & pDeltaFutureSource.AssetPositionRiskValues
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //SpanFutureMonthRisk cloned = new SpanFutureMonthRisk(pDeltaFutureSource.m_SpanMethod, pContractGroupParam, pDeltaFutureSource.Maturity, pDeltaFutureSource.MaturityParameters, 0, 0, pDeltaFutureSource.CappedWeighedRisk, pDeltaFutureSource.AssetPositionRiskValues);
            SpanFutureMonthRisk cloned = new SpanFutureMonthRisk(pFutureSource.m_SpanMethod, pFutureSource.m_EvaluationData, pContractGroupParam, pFutureSource.Maturity, pFutureSource.MaturityParameters, 0, 0, pFutureSource.CappedWeighedRisk, pFutureSource.AssetPositionRiskValues)
            {
                //
                // PM 20181214 [24398] Application du taux sur la matrice de risque de l'échéance et pas de calcul de la matrice de risque à partir de celles des assets
                m_RiskValue = pFutureSource.m_RiskValue
            };
            cloned.ApplyRateToRiskValue(pCreditRate);
            //
            cloned.Delta.AddWithRatio(pFutureSource.Delta, pDeltaRatio);
            return cloned;
        }
        #endregion static methods
    }

    /// <summary>
    /// Class d'évaluation des Deltas
    /// </summary>
    internal sealed class SpanDelta : ICloneable
    {
        #region members
        private decimal m_deltaLong = 0;    // Delta Long total
        private decimal m_deltaShort = 0;   // Delta Short total
        private decimal m_deltaLongConsumed = 0;    // Delta Long déjà utilisé
        private decimal m_deltaShortConsumed = 0;   // Delta Short déjà utilisé

        /// <summary>
        /// Delta Long (Toujours positif ou égal à zéro)
        /// </summary>
        public decimal DeltaLong
        {
            get { return m_deltaLong; }
            //set { m_deltaLong = value >= 0 ? value : value * -1; }
        }
        /// <summary>
        /// Delta Short (Toujours négatif ou égal à zéro)
        /// </summary>
        public decimal DeltaShort
        {
            get { return m_deltaShort; }
            //set { m_deltaShort = value <= 0 ? value : value * -1; }
        }
        /// <summary>
        /// Delta Net Initial
        /// </summary>
        public decimal DeltaNet
        {
            get { return m_deltaLong + m_deltaShort; }
        }
        /// <summary>
        /// Delta Long Consumed (Toujours positif ou égal à zéro)
        /// </summary>
        public decimal DeltaLongConsumed
        {
            get { return m_deltaLongConsumed; }
            //set { m_deltaLongUsed = value >= 0 ? value : value * -1; }
        }
        /// <summary>
        /// Delta Short Consumed (Toujours négatif ou égal à zéro)
        /// </summary>
        public decimal DeltaShortConsumed
        {
            get { return m_deltaShortConsumed; }
            //set { m_deltaShortUsed = value <= 0 ? value : value * -1; }
        }
        /// <summary>
        /// Delta Net Consumed : read only
        /// </summary>
        public decimal DeltaNetConsumed
        {
            get { return DeltaLongConsumed + DeltaShortConsumed; }
        }
        /// <summary>
        /// Delta Long Remaining : read only
        /// </summary>
        public decimal DeltaLongRemaining
        {
            get { return DeltaLong - DeltaLongConsumed; }
        }
        /// <summary>
        /// Delta Short Remaining : read only
        /// </summary>
        public decimal DeltaShortRemaining
        {
            get { return DeltaShort - DeltaShortConsumed; }
        }
        /// <summary>
        /// Delta Net Remaining : read only
        /// </summary>
        public decimal DeltaNetRemaining
        {
            get { return DeltaLongRemaining + DeltaShortRemaining; }
        }
        #endregion members

        #region constructors
        /// <summary>
        /// Création d'un nouvel objet de gestion des Delta Long et Short dont les valeurs sont 0
        /// </summary>
        public SpanDelta() { }
        /// <summary>
        /// Création d'un nouvel objet de gestion des Delta Long et Short
        /// </summary>
        /// <param name="pDelta">Nombre de delta Long ou Short (Long si positif, Short si négatif)</param>
        public SpanDelta(decimal pDelta)
        {
            if (pDelta < 0)
            {
                m_deltaShort = pDelta;
            }
            else
            {
                m_deltaLong = pDelta;
            }
        }
        /// <summary>
        /// Création d'un nouvel objet de gestion des Delta Long et Short
        /// </summary>
        /// <param name="pDeltaLong">Nombre de delta Long (doit être supérieur ou égal à 0)</param>
        /// <param name="pDeltaShort">Nombre de delta Short (doit être inférieur ou égal à 0)</param>
        public SpanDelta(decimal pDeltaLong, decimal pDeltaShort)
        {
            if (pDeltaLong >= 0)
            {
                m_deltaLong = pDeltaLong;
            }
            if (pDeltaShort <= 0)
            {
                m_deltaShort = pDeltaShort;
            }
        }
        #endregion constructors

        #region methods
        /// <summary>
        /// Utilise le nombre spécifié de delta "Long" ou "Short"
        /// </summary>
        /// <param name="pDelta">Nombre de delta "Long" ou "Short" à utiliser (Positif pour "Long" et négatif pour "Short")</param>
        /// <returns>Nombre total de delta "Long" ou "Short" rééllement utilisé</returns>
        public decimal Consume(decimal pDelta)
        {
            if (pDelta > 0)
            {
                return ConsumeLong(pDelta);
            }
            else
            {
                return ConsumeShort(pDelta);
            }
        }
        /// <summary>
        /// Ajout d'un SpanDelta
        /// </summary>
        /// <param name="pDeltaToAdd">SpanDelta à ajouter</param>
        public void Add(SpanDelta pDeltaToAdd)
        {
            m_deltaLong += pDeltaToAdd.DeltaLong;
            m_deltaShort += pDeltaToAdd.DeltaShort;
            m_deltaLongConsumed += pDeltaToAdd.DeltaLongConsumed;
            m_deltaShortConsumed += pDeltaToAdd.DeltaShortConsumed;
        }
        /// <summary>
        /// Ajout d'un SpanDelta en appliquant un Ratio
        /// </summary>
        /// <param name="pDeltaToAdd">SpanDelta à ajouter</param>
        /// <param name="pRatio">Ratio à appliquer</param>
        public void AddWithRatio(SpanDelta pDeltaToAdd, decimal pRatio)
        {
            Add(SpanDelta.CloneWithRatio(pDeltaToAdd, pRatio));
        }
        /// <summary>
        /// Utilise le nombre spécifié de delta "Long"
        /// </summary>
        /// <param name="pDelta">Nombre de delta "Long" à utiliser (doit être positif)</param>
        /// <returns>Nombre total de delta "Long" rééllement utilisé (>=0) sur ceux reçu en paramètres</returns>
        private decimal ConsumeLong(decimal pDelta)
        {
            decimal deltaConsumed = 0;
            if (pDelta > 0)
            {
                deltaConsumed = System.Math.Min(pDelta, m_deltaLong - m_deltaLongConsumed);
                m_deltaLongConsumed += deltaConsumed;
            }
            return deltaConsumed;
        }
        /// <summary>
        /// Utilise le nombre spécifié de delta "Short"
        /// </summary>
        /// <param name="pDelta">Nombre de delta "Short" à utiliser (doit être négatif)</param>
        /// <returns>Nombre total de delta "Short" rééllement utilisé (&lt;0) sur ceux reçu en paramètres </returns>
        private decimal ConsumeShort(decimal pDelta)
        {
            decimal deltaConsumed = 0;
            if (pDelta < 0)
            {
                deltaConsumed = System.Math.Max(pDelta, m_deltaShort - m_deltaShortConsumed);
                m_deltaShortConsumed += deltaConsumed;
            }
            return deltaConsumed;
        }
        /// <summary>
        /// "Deep Copy" Retour un clone du SpanDelta courant
        /// </summary>
        /// <returns>Objet cloné</returns>
        public object Clone()
        {
            SpanDelta cloned = new SpanDelta(DeltaLong, DeltaShort)
            {
                m_deltaLongConsumed = DeltaLongConsumed,
                m_deltaShortConsumed = DeltaShortConsumed
            };
            return cloned;
        }
        #endregion methods

        #region methods static
        /// <summary>
        /// Clonage de Delta avec application d'un Ratio
        /// <para>Lorsque le delta source et null ou que le pratio est de 0</para>
        /// <para>Un nouveau delta à 0 est retourné</para>
        /// </summary>
        /// <param name="pDeltaSource">Delta source</param>
        /// <param name="pRatio">Ratio à appliquer</param>
        /// <returns>Delta cloné avec application du ratio</returns>
        public static SpanDelta CloneWithRatio(SpanDelta pDeltaSource, decimal pRatio)
        {
            SpanDelta cloned;
            // Si delta et ratio présent et != de 0
            if ((pDeltaSource != default(SpanDelta)) && (pRatio != 0))
            {
                cloned = new SpanDelta(pDeltaSource.DeltaLong * pRatio, pDeltaSource.DeltaShort * pRatio)
                {
                    m_deltaLongConsumed = pDeltaSource.DeltaLongConsumed * pRatio,
                    m_deltaShortConsumed = pDeltaSource.DeltaShortConsumed * pRatio
                };
            }
            else
            {
                cloned = new SpanDelta(0, 0);
            }
            return cloned;
        }
        /// <summary>
        /// Ajout d'un SpanDelta en appliquant un Ratio
        /// </summary>
        /// <param name="pDeltaSource">Delta source</param>
        /// <param name="pDeltaToAdd">Delta à ajouter</param>
        /// <param name="pRatio">Ratio à appliquer</param>
        public static void AddWithRatio(SpanDelta pDeltaSource, SpanDelta pDeltaToAdd, decimal pRatio)
        {
            pDeltaSource.AddWithRatio(pDeltaToAdd, pRatio);
        }
        #endregion
    }

    /// <summary>
    /// Class de base pour l'évaluation des scanning risk et weigthed future price risk
    /// </summary>
    // PM 20130605 [18730] Nouvelle Class
    internal class SpanScanningElement
    {
        #region members
        protected SpanContractGroup m_ContractGroupParameters = null; // Paramètres SPAN du groupe de contrats
        protected SPANMethod m_SpanMethod = null; // Méthode de calcul SPAN
        protected List<SpanAssetRiskValues> m_AssetPositionRiskValues = null; // Matrice de risque de chaque asset en position
        protected Dictionary<string, SpanFutureMonthRisk> m_FutureMonthDelta = null;
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        protected SpanEvaluationData m_EvaluationData = default;
        //
        protected Dictionary<int, decimal> m_RiskValue = null;
        protected SpanDelta m_Delta = null;
        protected int m_ActiveScenario = 0;
        protected decimal m_ScanRisk = 0;
        protected decimal m_TimeRisk = 0;
        protected decimal m_VolatilityRisk = 0;
        protected decimal m_WeighedRisk = 0;
        protected MarginWeightedRiskCalculationMethodEnum m_WeightedRiskMethod = MarginWeightedRiskCalculationMethodEnum.Normal;
        protected decimal m_NormalWeighedRisk = 0;
        protected decimal m_CappedWeighedRisk = 0;
        // PM 20161003 [22436] m_ShortOptionMinimumCharge déplacé à partir de class enfant SpanContractGroupRisk
        protected decimal m_ShortOptionMinimumCharge = 0;
        #endregion members

        #region accessors
        public SpanContractGroup ContractGroupParameters { get { return m_ContractGroupParameters; } }
        public List<SpanAssetRiskValues> AssetPositionRiskValues { get { return m_AssetPositionRiskValues; } }
        public Dictionary<string, SpanFutureMonthRisk> FutureMonthDelta { get { return m_FutureMonthDelta; } }
        public Dictionary<int, decimal> RiskValue { get { return m_RiskValue; } }
        public SpanDelta Delta { get { return m_Delta; } }
        public int ActiveScenario { get { return m_ActiveScenario; } }
        public virtual decimal ScanRisk { get { return m_ScanRisk; } }
        public decimal TimeRisk { get { return m_TimeRisk; } }
        public decimal VolatilityRisk { get { return m_VolatilityRisk; } }
        public decimal WeightedFuturesPriceRisk { get { return m_WeighedRisk; } }
        // PM 20161003 [22436] ShortOptionMinimumCharge déplacé à partir de class enfant SpanContractGroupRisk
        public decimal ShortOptionMinimumCharge { get { return m_ShortOptionMinimumCharge; } }
        //
        public decimal FuturesPriceRisk
        {
            get { return System.Math.Max(ScanRisk - TimeRisk - VolatilityRisk, 0); }
        }
        #endregion accessors

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSpanMethod"></param>
        /// <param name="pEvaluationData">Données de calcul</param>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //public SpanScanningElement(SPANMethod pSpanMethod)
        public SpanScanningElement(SPANMethod pSpanMethod, SpanEvaluationData pEvaluationData)
        {
            m_SpanMethod = pSpanMethod;
            m_EvaluationData = pEvaluationData;
        }
        #endregion constructor

        #region methods
        /// <summary>
        /// Utilise un certaine quantité de dleta
        /// </summary>
        /// <param name="pDelta">Valeur de delta à utiliser</param>
        /// <returns>Nombre de delta utilisés</returns>
        public virtual decimal ConsumeDelta(decimal pDelta)
        {
            return m_Delta.Consume(pDelta);
        }
        /// <summary>
        /// Ajout d'un SpanDelta
        /// </summary>
        /// <param name="pDeltaToAdd">SpanDelta à ajouter</param>
        public void AddSpanDelta(SpanDelta pDeltaToAdd)
        {
            m_Delta.Add(pDeltaToAdd);
        }
        /// <summary>
        /// Ajout d'un SpanDelta en appliquant un Ratio
        /// </summary>
        /// <param name="pDeltaToAdd">SpanDelta à ajouter</param>
        /// <param name="pRatio">Ratio à appliquer</param>
        public void AddSpanDeltaWithRatio(SpanDelta pDeltaToAdd, decimal pRatio)
        {
            m_Delta.AddWithRatio(pDeltaToAdd, pRatio);
        }

        /// <summary>
        /// Ajout d'un SpanScanningElement en appliquant un ratio au Delta et un credit rate aux valeurs des scénarios
        /// </summary>
        /// <param name="pScanElement">SpanScanningElement à ajouter</param>
        /// <param name="pRatio">Ratio à appliquer</param>
        /// <param name="pCreditRate">Credit rate à appliquer</param>
        public void AddScanElementWithRatio(SpanScanningElement pScanElement, decimal pRatio, decimal pCreditRate)
        {
            if (default(SpanScanningElement) != pScanElement)
            {
                pScanElement.ApplyRateToRiskValue(pCreditRate);
                m_AssetPositionRiskValues.AddRange(pScanElement.AssetPositionRiskValues);
                AddSpanDeltaWithRatio(pScanElement.Delta, pRatio);
            }
        }

        /// <summary>
        /// Cumul des matrices de risques des asset composant l'élément
        /// </summary>
        protected void SumRiskValue()
        {
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //if ((default(SPANMethod) != m_SpanMethod) && m_SpanMethod.IsNotOmnibus)
            if ((m_EvaluationData != default) && m_EvaluationData.IsNotOmnibus)
            {
                m_RiskValue = new Dictionary<int, decimal>();

                // Matrice de risk
                foreach (SpanAssetRiskValues riskValue in AssetPositionRiskValues)
                {
                    // PM 20160829 [22420] Ajout test pour le cas où un élément de risque n'a pas de matrice de risque
                    if (riskValue != default(SpanAssetRiskValues))
                    {
                        // Cumul des matrices
                        SPANMethod.AddRiskValues(m_RiskValue, riskValue.RiskValue);
                    }
                }
            }
        }
        /// <summary>
        /// Définie les valeurs des scénarios de risque comme la somme de chaque valeur d'un ensemble de valeurs des scénarios de risque
        /// </summary>
        /// <param name="pRiskValues">Ensemble de valeurs des scénarios de risque</param>
        protected void SumRiskValue(IEnumerable<Dictionary<int, decimal>> pRiskValues)
        {
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //if ((default(SPANMethod) != m_SpanMethod) && m_SpanMethod.IsNotOmnibus)
            if ((m_EvaluationData != default) && m_EvaluationData.IsNotOmnibus)
            {
                m_RiskValue = new Dictionary<int, decimal>();

                // Matrice de risk
                foreach (Dictionary<int, decimal> riskValue in pRiskValues)
                {
                    // Cumul des matrices
                    SPANMethod.AddRiskValues(m_RiskValue, riskValue);
                }
            }
        }

        /// <summary>
        /// Application d'un credit rate aux scénarios de risque négatif des assets de l'élément
        /// </summary>
        /// <param name="pCreditRate">Credit rate à appliquer</param>
        protected void ApplyRateToAssetRiskValue(decimal pCreditRate)
        {
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //if ((default(SPANMethod) != m_SpanMethod) && m_SpanMethod.IsNotOmnibus)
            if ((m_EvaluationData != default) && m_EvaluationData.IsNotOmnibus)
            {
                if (default(List<SpanAssetRiskValues>) != AssetPositionRiskValues)
                {
                    foreach (SpanAssetRiskValues assetRiskValue in AssetPositionRiskValues)
                    {
                        assetRiskValue.ApplyRateToRiskValue(pCreditRate);
                    }
                }
            }
        }

        /// <summary>
        /// Application d'un credit rate aux scénarios de risque négatif de l'élément
        /// </summary>
        /// <param name="pCreditRate">Credit rate à appliquer</param>
        protected void ApplyRateToRiskValue(decimal pCreditRate)
        {
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //if ((default(SPANMethod) != m_SpanMethod) && m_SpanMethod.IsNotOmnibus)
            if ((m_EvaluationData != default) && m_EvaluationData.IsNotOmnibus)
            {
                if (default != RiskValue)
                {
                    // Convertion de la matrice
                    m_RiskValue = (
                        from riskValue in m_RiskValue
                        select new
                        {
                            riskValue.Key,
                            Value = (riskValue.Value < 0) ? (riskValue.Value * pCreditRate) : riskValue.Value,
                        }).ToDictionary(v => v.Key, v => v.Value);
                }
            }
        }

        /// <summary>
        /// Calcul des valeurs de risque découlant directement de la matrice de risque
        /// </summary>
        protected void EvaluateRiskFromRiskValue()
        {
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //if ((default(SPANMethod) != m_SpanMethod) && m_SpanMethod.IsNotOmnibus)
            if ((m_EvaluationData != default) && m_EvaluationData.IsNotOmnibus)
            {
                // Valeurs de risque liés à la matrice
                if ((default != m_RiskValue)
                    && (m_RiskValue.Count > 0))
                {
                    // Time Risk
                    m_TimeRisk = m_SpanMethod.SpanRoundAmount((m_RiskValue[1] + m_RiskValue[2]) / 2);

                    // Scan Risk
                    m_ScanRisk = m_RiskValue.Max(value => value.Value);

                    // Active Scenario
                    m_ActiveScenario = m_RiskValue.First(risk => risk.Value == m_ScanRisk).Key;

                    // Volatility Risk
                    if (ScanRisk < 0)
                    {
                        // Cas d'un ScanRisk négatif
                        m_VolatilityRisk = m_SpanMethod.SpanRoundAmount((m_RiskValue[1] - m_RiskValue[2]) / 2);
                        // Et mise à jour du Scan Risk
                        m_ScanRisk = 0;
                    }
                    else
                    {
                        // PM 20130530 Le scénario couplé est le scénario actif lui même pour les scénarios 15 et 16
                        int pairedScenario = m_ActiveScenario;
                        if ((pairedScenario != 15) && (pairedScenario != 16))
                        {
                            pairedScenario = (m_ActiveScenario % 2) == 0 ? m_ActiveScenario - 1 : m_ActiveScenario + 1;
                        }
                        m_VolatilityRisk = m_SpanMethod.SpanRoundAmount((ScanRisk - m_RiskValue[pairedScenario]) / 2);
                    }
                }
            }
        }

        /// <summary>
        /// Calcul du ShortOptionMinimum
        /// </summary>
        // PM 20161003 [22436] New
        protected void EvaluateShortOptionMinimum()
        {
            if ((m_ContractGroupParameters != default(SpanContractGroup)) && (m_AssetPositionRiskValues != default(List<SpanAssetRiskValues>)))
            {
                if (m_ContractGroupParameters.ShortOptionMinimumMethod == "1")
                {
                    /* SOM method MAX */
                    decimal callSOM = m_AssetPositionRiskValues.Where(a => a.PutOrCall == PutOrCallEnum.Call).Sum(v => v.ShortOptionMinimum);
                    decimal putSOM = m_AssetPositionRiskValues.Where(a => a.PutOrCall == PutOrCallEnum.Put).Sum(v => v.ShortOptionMinimum);
                    m_ShortOptionMinimumCharge = System.Math.Max(callSOM, putSOM);
                }
                else
                {
                    /* SOM method GROSS */
                    m_ShortOptionMinimumCharge = m_AssetPositionRiskValues.Sum(v => v.ShortOptionMinimum);
                }
                m_ShortOptionMinimumCharge *= m_ContractGroupParameters.RiskMultiplier;
            }
        }

        /// <summary>
        /// Calcule Risque pondéré de prix future
        /// </summary>
        /// PM 20160829 [22420] La méthode devient virtual (car overridée dans la class SpanTierMonthRisk)
        protected virtual void EvaluateWeighedRisk()
        {
            m_WeighedRisk = 0;
            m_NormalWeighedRisk = 0;
            m_CappedWeighedRisk = 0;
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //if ((default(SPANMethod) != m_SpanMethod)
            //    && (m_SpanMethod.IsNotOmnibus)
            //    && (FuturesPriceRisk > 0)
            //    && (Delta.DeltaNet != 0))
            if ((m_EvaluationData != default)
                && m_EvaluationData.IsNotOmnibus
                && (FuturesPriceRisk > 0)
                && (Delta.DeltaNet != 0))
            {
                // Calcul du Normal Weighted Risk
                m_NormalWeighedRisk = m_SpanMethod.SpanWeightedFuturePriceRisk(FuturesPriceRisk, Delta.DeltaNet);

                // Par défaut le Weighted Risk = le Normal Weighted Risk
                m_WeighedRisk = m_NormalWeighedRisk;

                // Lecture de la méthode de calcul du Weighted Risk
                m_WeightedRiskMethod = MarginWeightedRiskCalculationMethodEnum.Normal;

                //PM 20151127 [21571][21605] Pour LondonSPAN toujours prendre la méthode paramètré sur la Clearing Organization si elle est renseignée
                //if (StrFunc.IsEmpty(ContractGroupParameters.WeightedRiskMethod))
                // PM 20160404 [22116] Utilisation de m_SpanMethod.MethodParameter.WeightedRiskMethodEnum au lieu de m_SpanMethod.ClearingOrganizationParameters.WeightedRiskMethod
                //if ((StrFunc.IsEmpty(ContractGroupParameters.WeightedRiskMethod) || m_SpanMethod is LondonSPANMethod) && (StrFunc.IsFilled(m_SpanMethod.ClearingOrganizationParameters.WeightedRiskMethod)))
                //{
                //    m_WeightedRiskMethod = (MarginWeightedRiskCalculationMethodEnum)ReflectionTools.EnumParse(new MarginWeightedRiskCalculationMethodEnum(), m_SpanMethod.ClearingOrganizationParameters.WeightedRiskMethod);
                //}
                if ((StrFunc.IsEmpty(ContractGroupParameters.WeightedRiskMethod) || m_SpanMethod is LondonSPANMethod))
                {
                    m_WeightedRiskMethod = m_SpanMethod.MethodParameter.WeightedRiskMethodEnum;
                }
                else if (ContractGroupParameters.WeightedRiskMethod == "2")
                {
                    m_WeightedRiskMethod = MarginWeightedRiskCalculationMethodEnum.Capped;
                }
                else if (ContractGroupParameters.WeightedRiskMethod == "3")
                {
                    m_WeightedRiskMethod = MarginWeightedRiskCalculationMethodEnum.ScanRange;
                }

                // PM 20161003 [22436] Ajout gestion méthode ScanRange
                //if ((m_WeightedRiskMethod == MarginWeightedRiskCalculationMethodEnum.Capped) && (FutureMonthDelta.Count > 0))
                //{
                //    DateTime minMaturity = FutureMonthDelta.Min(f => SPANMethod.SPANMaturityMonthToDateTime(f.Key));
                //    SpanFutureMonthRisk futMonth = FutureMonthDelta.First(f => SPANMethod.SPANMaturityMonthToDateTime(f.Key) == minMaturity).Value;

                //    // PM 20150928 [21405][20948] Modification de l'arrondie du capped weightedPriceRisk
                //    //m_CappedWeighedRisk = m_SpanMethod.SpanRoundDelta(futMonth.CappedWeighedRisk);
                //    m_CappedWeighedRisk = m_SpanMethod.SpanRoundWeightedFuturePriceRisk(futMonth.CappedWeighedRisk);
                //    m_WeighedRisk = System.Math.Min(m_CappedWeighedRisk, m_NormalWeighedRisk);
                //}
                // Calcul du ScanRange et Capped Weighted Risk
                if ((MarginWeightedRiskCalculationMethodEnum.Normal != m_WeightedRiskMethod) && (FutureMonthDelta.Count > 0))
                {
                    DateTime minMaturity = FutureMonthDelta.Min(f => SPANMethod.SPANMaturityMonthToDateTime(f.Key));
                    SpanFutureMonthRisk futMonth = FutureMonthDelta.First(f => SPANMethod.SPANMaturityMonthToDateTime(f.Key) == minMaturity).Value;

                    m_CappedWeighedRisk = m_SpanMethod.SpanRoundWeightedFuturePriceRisk(futMonth.CappedWeighedRisk);
                    if (MarginWeightedRiskCalculationMethodEnum.Capped == m_WeightedRiskMethod)
                    {
                        m_WeighedRisk = System.Math.Min(m_CappedWeighedRisk, m_NormalWeighedRisk);
                    }
                    else
                    {
                        m_WeighedRisk = m_CappedWeighedRisk;
                    }
                }
            }
        }
        #endregion methods
    }

    /// <summary>
    /// Class de valeurs de risque par asset
    /// </summary>
    internal sealed class SpanAssetRiskValues
    {
        #region members
        public int AssetId = 0;
        public int SPANContractId = 0;
        public string Category = string.Empty;
        public string FutureMaturity = string.Empty;
        public string OptionMaturity = string.Empty;
        public Nullable<PutOrCallEnum> PutOrCall = null;
        public decimal StrikePrice = 0;
        public SideEnum Side;
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity = 0;
        public decimal Delta = 0;
        public List<KeyValuePair<int, decimal>> RiskValue = null;
        public decimal OptionValue = 0;
        public decimal ShortOptionMinimum = 0;
        public string ContractCurrency = string.Empty;
        public string MarginCurrency = string.Empty;
        #endregion members

        #region methods
        /// <summary>
        /// Applique un credit rate au valeur de risque négative
        /// </summary>
        /// <param name="pCreditRate">Credit rate à appliquer</param>
        public void ApplyRateToRiskValue(decimal pCreditRate)
        {
            if (default(List<KeyValuePair<int, decimal>>) != RiskValue)
            {
                RiskValue = (
                    from value in RiskValue
                    select new KeyValuePair<int, decimal>(value.Key, (value.Value < 0) ? (value.Value * pCreditRate) : value.Value)
                    ).ToList();
            }
        }
        #endregion methods

        #region static methods
        // PM 20130605 [18730] Nouvelle methode
        /// <summary>
        /// Aggrege un ensemble de listes de SpanAssetRiskValues
        /// </summary>
        /// <param name="pAssetRiskToAggregate">Ensemble de listes de SpanAssetRiskValues</param>
        /// <returns>Liste aggrégée des SpanAssetRiskValues</returns>
        public static List<SpanAssetRiskValues> Aggegate(IEnumerable<List<SpanAssetRiskValues>> pAssetRiskToAggregate)
        {
            List<SpanAssetRiskValues> retAssetRiskList = new List<SpanAssetRiskValues>();
            if (null != pAssetRiskToAggregate)
            {
                foreach (List<SpanAssetRiskValues> assetRiskList in pAssetRiskToAggregate)
                {
                    // PM 20170929 [23472] s'assurer que assetRiskList n'est pas null
                    if (assetRiskList != default(List<SpanAssetRiskValues>))
                    {
                        retAssetRiskList.AddRange(assetRiskList);
                    }
                }
            }
            return retAssetRiskList;
        }
        #endregion static methods
    }

    /// <summary>
    /// Class de calcul sur une jambe d'un spread intra groupe de contrat
    /// </summary>
    internal sealed class SpanInterMonthSpreadLeg
    {
        #region members
        public SpanFutureMonthRisk Month = null;
        public SpanTierMonthRisk Tier = null;
        public SpanIntraLeg IntraLegParameters = null;
        #endregion members
        #region accessors
        public decimal NumberSpreadLong
        {
            get
            {
                decimal deltaLong = 0;
                if (null != IntraLegParameters)
                {
                    if (null != Month)
                    {
                        if (Month.Delta.DeltaNetRemaining > 0)
                        {
                            deltaLong = Month.Delta.DeltaNetRemaining / IntraLegParameters.DeltaPerSpread;
                        }
                    }
                    else if (null != Tier)
                    {
                        deltaLong = Tier.Delta.DeltaLongRemaining / IntraLegParameters.DeltaPerSpread;
                    }
                }
                return deltaLong;
            }
        }
        public decimal NumberSpreadShort
        {
            get
            {
                decimal deltaShort = 0;
                if (null != IntraLegParameters)
                {
                    if (null != Month)
                    {
                        if (Month.Delta.DeltaNetRemaining < 0)
                        {
                            deltaShort = System.Math.Abs(Month.Delta.DeltaNetRemaining) / IntraLegParameters.DeltaPerSpread;
                        }
                    }
                    else if (null != Tier)
                    {
                        deltaShort = System.Math.Abs(Tier.Delta.DeltaShortRemaining) / IntraLegParameters.DeltaPerSpread;
                    }
                }
                return deltaShort;
            }
        }
        #endregion accessors
        #region methods
        /// <summary>
        /// Utilise le nombre de delta passé en paramètre
        /// </summary>
        /// <param name="pDelta">Nombre de delta à utiliser</param>
        /// <returns>Nombre de delta réellement utilisé</returns>
        public decimal ConsumeDelta(decimal pDelta)
        {
            decimal deltaConsumed = 0;
            if (pDelta != 0)
            {
                if (null != Month)
                {
                    // Utiliser les Deltas du Month
                    deltaConsumed = Month.ConsumeDelta(pDelta);
                }
                else if (null != Tier)
                {
                    // Utiliser les Deltas du Tier
                    deltaConsumed = Tier.ConsumeDelta(pDelta);
                }
            }
            return deltaConsumed;
        }
        #endregion methods
    }

    /// <summary>
    /// Classe des données de calcul pour une position
    /// </summary>
    /// PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
    internal sealed class SpanEvaluationData
    {
        #region members
        /// <summary>
        /// Positions
        /// </summary>
        // PM 20190401 [24625][24387] Ajout Positions
        private IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> _Positions = default;
        #region Evaluation Result
        /// <summary>
        /// Ensemble des paramètres des échéances en position
        /// </summary>
        private IEnumerable<SpanMaturity> _FutureMonthParam = default;
        /// <summary>
        /// Delta Net par échéance future sur chaque groupe de contrat
        /// </summary>
        private SpanFutureMonthRisk[] _FutureMonth = default;
        /// <summary>
        /// Delta Net par Tier (période) sur chaque groupe de contrat
        /// </summary>
        private SpanTierMonthRisk[] _TierMonth = default;
        /// <summary>
        /// Risque de chaque Exchange Complex
        /// </summary>
        private List<SpanExchangeComplexRisk> _ExchangeComplexRisk = default;
        /// <summary>
        /// Risque de chaque combined group
        /// </summary>
        private List<SpanCombinedGroupRisk> _CombinedGroupRisk = default;
        /// <summary>
        /// Risque de chaque group de contrat
        /// </summary>
        private List<SpanContractGroupRisk> _ContractGroupRisk = default;
        /// <summary>
        /// Risque de chaque contrat
        /// </summary>
        private SpanContractRisk[] _ContractRisk = default;
        #endregion Evaluation Result
        #region SPAN Account Parameters
        /// <summary>
        /// Type de compte SPAN et donc de méthode à utiliser
        /// </summary>
        private SpanAccountType _SpanAccountType = SpanAccountType.Default;
        /// <summary>
        /// Indique si la méthode doit effectuer un calcul de montant de Maintenance
        /// </summary>
        private bool _IsMaintenanceAmount = true;
        /// <summary>
        /// SPAN Scan Risk Offset Cap Percentage's
        /// (1 = 100%)
        /// </summary>
        private decimal _ScanRiskOffsetCapPrct = 1;
        #endregion SPAN Account Parameters
        #endregion members

        #region accessors
        /// <summary>
        /// Positions
        /// </summary>
        // PM 20190401 [24625][24387] Ajout Positions
        internal IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        { get { return _Positions; } set { _Positions = value; } }
        #region Evaluation Result
        /// <summary>
        /// Ensemble des paramètres des échéances en position
        /// </summary>
        internal IEnumerable<SpanMaturity> FutureMonthParam
        { get { return _FutureMonthParam; } set { _FutureMonthParam = value; } }
        /// <summary>
        /// Delta Net par échéance future sur chaque groupe de contrat
        /// </summary>
        internal SpanFutureMonthRisk[] FutureMonth
        { get { return _FutureMonth; } set { _FutureMonth = value; } }
        /// <summary>
        /// Delta Net par Tier (période) sur chaque groupe de contrat
        /// </summary>
        internal SpanTierMonthRisk[] TierMonth
        { get { return _TierMonth; } set { _TierMonth = value; } }
        /// <summary>
        /// Risque de chaque Exchange Complex
        /// </summary>
        internal List<SpanExchangeComplexRisk> ExchangeComplexRisk
        { get { return _ExchangeComplexRisk; } set { _ExchangeComplexRisk = value; } }
        /// <summary>
        /// Risque de chaque combined group
        /// </summary>
        internal List<SpanCombinedGroupRisk> CombinedGroupRisk
        { get { return _CombinedGroupRisk; } set { _CombinedGroupRisk = value; } }
        /// <summary>
        /// Risque de chaque group de contrat
        /// </summary>
        internal List<SpanContractGroupRisk> ContractGroupRisk
        { get { return _ContractGroupRisk; } set { _ContractGroupRisk = value; } }
        /// <summary>
        /// Risque de chaque contrat
        /// </summary>
        internal SpanContractRisk[] ContractRisk
        { get { return _ContractRisk; } set { _ContractRisk = value; } }
        #endregion Evaluation Result
        #region SPAN Account Parameters
        /// <summary>
        /// Type de compte SPAN et donc de méthode à utiliser
        /// </summary>
        internal SpanAccountType SpanAccountType
        { get { return _SpanAccountType; } set { _SpanAccountType = value; } }
        /// <summary>
        /// Indique si la méthode doit effectuer un calcul de montant de Maintenance
        /// </summary>
        internal bool IsMaintenanceAmount
        { get { return _IsMaintenanceAmount; } set { _IsMaintenanceAmount = value; } }
        /// <summary>
        /// SPAN Scan Risk Offset Cap Percentage's
        /// (1 = 100%)
        /// </summary>
        internal decimal ScanRiskOffsetCapPrct
        { get { return _ScanRiskOffsetCapPrct; } set { _ScanRiskOffsetCapPrct = value; } }
        /// <summary>
        /// Indique si la méthode est Omnibus
        /// </summary>
        public bool IsOmnibus
        {
            get { return ((_SpanAccountType == SpanAccountType.OmnibusHedger) || (_SpanAccountType == SpanAccountType.OmnibusSpeculator)); }
        }
        /// <summary>
        /// Indique si la méthode n'est pas en Omnibus
        /// </summary>
        public bool IsNotOmnibus
        {
            get { return (!IsOmnibus); }
        }
        #endregion SPAN Account Parameters
        #endregion accessors
    }

    /// <summary>
    /// Classe de base pour toutes les méthodes de calcul de risque SPAN
    /// </summary>
    public abstract class SPANMethod : BaseMethod
    {
        #region const
        /* 3 constantes pour les codes des contrats utilisés en dur pour la méthode de spread inter commodity n°3 "Crush" Spread */
        private const string SpanProductSoybeanMeal = "06";
        private const string SpanProductSoybeanOil = "07";
        private const string SpanProductSoybean = "S";
        #endregion const

        #region enum
        /// <summary>
        /// Enum utilisé pour les intervals de maturity date
        /// </summary>
        internal enum MaturityMonthIntervalEnum
        {
            Start,
            End
        }
        #endregion enum

        #region members
        #region Evaluation Result
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //internal IEnumerable<SpanMaturity> m_FutureMonthParam = null; // Ensemble des paramètres des échéances en position
        //internal SpanFutureMonthRisk[] m_FutureMonth = null;   // Delta Net par échéance future sur chaque groupe de contrat
        //private SpanTierMonthRisk[] m_TierMonth = null;    // Delta Net par Tier (période) sur chaque groupe de contrat
        //internal List<SpanExchangeComplexRisk> m_ExchangeComplexRisk = null; // Risque de chaque Exchange Complex
        //internal List<SpanCombinedGroupRisk> m_CombinedGroupRisk = null; // Risque de chaque combined group
        //internal List<SpanContractGroupRisk> m_ContractGroupRisk = null; // Risque de chaque group de contrat
        //internal SpanContractRisk[] m_ContractRisk = null; // Risque de chaque contrat
        #endregion Evaluation Result
        #region Referentiel Parameters
        internal IEnumerable<AssetExpandedParameter> m_AssetExpandedParameters = null;
        #endregion Referentiel Parameters
        #region SPAN Contract Parameters
        internal IEnumerable<SpanExchangeComplex> m_ExchangeComplexParameters = null;
        internal IEnumerable<SpanExchange> m_ExchangeParameters = null;
        internal IEnumerable<SpanCurrency> m_CurrencyParameters = null;
        internal IEnumerable<SpanCurConv> m_CurConvParameters = null;
        internal IEnumerable<SpanInterSpread> m_InterSpreadParameters = null;
        internal IEnumerable<SpanInterLeg> m_InterLegParameters = null;
        internal IEnumerable<SpanCombinedGroup> m_CombinedGroupParameters = null;
        internal IEnumerable<SpanContractGroup> m_ContractGroupParameters = null;
        internal IEnumerable<SpanContract> m_ContractParameters = null;
        internal IEnumerable<SpanDeliveryMonth> m_DeliveryMonthParameters = null;
        internal IEnumerable<SpanMaturityTier> m_MaturityTierParameters = null;
        internal IEnumerable<SpanIntraSpread> m_IntraSpreadParameters = null;
        internal IEnumerable<SpanIntraLeg> m_IntraLegParameters = null;
        internal IEnumerable<SpanMaturity> m_MaturityParameters = null;
        internal IEnumerable<SpanRiskArray> m_RiskArrayParameters = null;
        // PM 20190801 [24717] Ajout m_ECCMarketVolumeParameters & m_ECCCombComStressParameters
        internal IEnumerable<ECCRiskMarketVolume> m_ECCMarketVolumeParameters = null;
        internal IEnumerable<ECCRiskCombComStressContract> m_ECCCombComStressParameters = null;
        #endregion SPAN Contract Parameters
        #region Empty SPAN Contract Parameters
        private SpanExchangeComplex m_EmptyExchangeComplexParameters = null;
        private SpanCombinedGroup m_EmptyCombinedGroupParameters = null;
        private SpanContractGroup m_EmptyContractGroupParameters = null;
        private SpanContract m_EmptyContractParameters = null;
        private SpanRiskArray m_EmptyRiskArrayParameters = null;
        #endregion Empty SPAN Contract Parameters
        #region SPAN Account Parameters
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        ///// <summary>
        ///// Type de compte SPAN et donc de méthode à utiliser
        ///// </summary>
        //protected SpanAccountType m_SpanAccountType = SpanAccountType.Default;
        ///// <summary>
        ///// Indique si la méthode doit effectuer un calcul de montant de Maintenance
        ///// </summary>
        //protected bool m_IsMaintenanceAmount = true;
        ///// <summary>
        ///// SPAN Scan Risk Offset Cap Percentage's
        ///// (1 = 100%)
        ///// </summary>
        ///// PM 20150930 [21134] Add m_ScanRiskOffsetCapPrct
        //protected decimal m_ScanRiskOffsetCapPrct = 1;
        #endregion SPAN Account Parameters
        #endregion members

        #region override base accessors
        /// <summary>
        /// Requête utilisée pour connaître l'existance de paramètres de risque pour une date donnée
        /// <remarks>Utilise les paramètres DTBUSINESS & SESSIONID</remarks>
        /// </summary>
        /// PM 20150507 [20575] Add QueryExistRiskParameter
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        protected override string QueryExistRiskParameter
        {
            get
            {
                string query;
                query = @"
                    select distinct 1
                      from dbo.IMSPAN_H s
                     inner join dbo.IMSPANEXCHANGE_H e on (e.IDIMSPAN_H = s.IDIMSPAN_H)
                     inner join dbo.IMSPANCONTRACT_H c on (c.IDIMSPANEXCHANGE_H = e.IDIMSPANEXCHANGE_H)
                     inner join dbo.IMSPANARRAY_H r on (r.IDIMSPANCONTRACT_H = c.IDIMSPANCONTRACT_H)
                     inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = r.IDASSET)
                     where (s.DTBUSINESS = @DTBUSINESS) and (s.SETTLEMENTSESSION = 'EOD')";
                return query;
            }
        }
        #endregion

        #region accessors
        // PM 20220111 [25617] FixBuySide et FixSellSide déplacé dans BaseMethod
        ///// <summary>
        ///// Valeur FIX indiquant un achat
        ///// </summary>
        //protected static string FixBuySide
        //{
        //    get
        //    {
        //        return SideTools.RetBuyFIXmlSide();
        //    }
        //}
        ///// <summary>
        ///// Valeur FIX indiquant une vente
        ///// </summary>
        //protected static string FixSellSide
        //{
        //    get
        //    {
        //        return SideTools.RetSellFIXmlSide();
        //    }
        //}

        /// <summary>
        /// Rouding Precision of Risk Values
        /// Definition de l'arrondi (différent entre la méthode CME et C21)
        /// </summary>
        protected abstract int RiskValuePrecision { get; }
        /// <summary>
        /// Rouding Precision of Delta Values
        /// Definition de l'arrondi (différent entre la méthode CME et C21)
        /// </summary>
        protected abstract int DeltaValuePrecision { get; }

        /// <summary>
        /// Precision des calcul par defaut
        /// </summary>
        protected virtual int DefaultPrecision
        {
            // PM 20160404 [22116] Utilisation de MethodParameter.RoundingPrecision au lieu de m_ClearingOrganizationParameters.RoundingPrecision
            //get { return m_ClearingOrganizationParameters != null ? m_ClearingOrganizationParameters.RoundingPrecision : 4; }
            get { return MethodParameter != null ? MethodParameter.RoundingPrecision : 4; }
        }
        // PM 20160404 [22116] N'est plus utilisé
        //internal SpanClearingOrganization ClearingOrganizationParameters
        //{
        //    get { return m_ClearingOrganizationParameters; }
        //}
        ///// <summary>
        ///// Indique si la méthode doit effectuer un calcul de montant de Maintenance
        ///// </summary>
        // PM 20180104 [CHEETAH] N'est plus utilisé
        //public bool IsMaintenance
        //{
        //    get { return m_EvaluationData.IsMaintenanceAmount; }
        //}
        ///// <summary>
        ///// Type de compte SPAN et donc de méthode à utiliser
        ///// </summary>
        // PM 20180104 [CHEETAH] N'est plus utilisé
        //public SpanAccountType SpanAccountType
        //{
        //    get { return m_EvaluationData.SpanAccountType; }
        //}
        ///// <summary>
        ///// Indique si la méthode est Omnibus
        ///// </summary>
        // PM 20180104 [CHEETAH] N'est plus utilisé
        //public bool IsOmnibus
        //{
        //    get { return ((m_EvaluationData.SpanAccountType == SpanAccountType.OmnibusHedger) || (m_EvaluationData.SpanAccountType == SpanAccountType.OmnibusSpeculator)); }
        //}
        ///// <summary>
        ///// Indique si la méthode n'est pas en Omnibus
        ///// </summary>
        // PM 20180104 [CHEETAH] N'est plus utilisé
        //public bool IsNotOmnibus
        //{
        //    get { return (!IsOmnibus); }
        //}
        ///// <summary>
        ///// Delta Net par échéance future sur chaque groupe de contrat
        ///// </summary>
        // PM 20180104 [CHEETAH] N'est plus utilisé
        //internal SpanFutureMonthRisk[] FutureMonth
        //{
        //    get { return m_FutureMonth; }
        //}
        ///// <summary>
        ///// Delta Net par Tier (période) sur chaque groupe de contrat
        ///// </summary>
        // PM 20180104 [CHEETAH] N'est plus utilisé
        //internal SpanTierMonthRisk[] TierMonth
        //{
        //    get { return m_TierMonth; }
        //}
        #endregion accessors

        #region private class
        /// <summary>
        /// Comparer de SpanMaturity
        /// </summary>
        private sealed class SpanMaturityComparer : IEqualityComparer<SpanMaturity>
        {
            /// <summary>
            /// Les SpanMaturity sont égaux s'ils sont la même échéance d'un même contrat
            /// </summary>
            /// <param name="x">1er SpanMaturity à comparer</param>
            /// <param name="y">2ème SpanMaturity à comparer</param>
            /// <returns>true si x Equals Y, sinon false</returns>
            public bool Equals(SpanMaturity x, SpanMaturity y)
            {

                //Vérifier si les objets référencent les même données
                if (ReferenceEquals(x, y)) return true;

                //Vérifier si un des objets est null
                if (x is null || y is null)
                    return false;

                // Vérifier qu'il s'agit de la même échéance d'un même contrat
                return (x.SPANContractId == y.SPANContractId)
                    && (x.FutureMaturity == y.FutureMaturity)
                    && (x.OptionMaturity == y.OptionMaturity);
            }

            /// <summary>
            /// La méthode GetHashCode fournissant la même valeur pour des objets SpanMaturity qui sont égaux.
            /// </summary>
            /// <param name="pMaturity">Le paramètre d'échéance Span dont on veut le hash code</param>
            /// <returns>La valeur du hash code</returns>
            public int GetHashCode(SpanMaturity pMaturity)
            {
                //Vérifier si l'obet est null
                if (pMaturity is null) return 0;

                //Obtenir le hash code de l'indentifier du contrat.
                int hashSPANContractId = pMaturity.SPANContractId.GetHashCode();

                //Obtenir le hash code du l'échéance future si non null.
                int hashFutureMaturity = pMaturity.FutureMaturity == null ? 0 : pMaturity.FutureMaturity.GetHashCode();

                //Obtenir le hash code du l'échéance d'option si non null.
                int hashOptionMaturity = pMaturity.OptionMaturity == null ? 0 : pMaturity.OptionMaturity.GetHashCode();

                //Calcul du hash code pour le SpanMaturity.
                return (int)(hashSPANContractId ^ hashFutureMaturity ^ hashOptionMaturity);
            }
        }
        #endregion private class

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
        /// Charge les paramètres spécifiques à SPAN.
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pAssetETDCache">Collection d'assets contenant tous les assets en position</param>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>();
            DateTime dtBusiness = this.DtBusiness.Date;
            string timing = ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(this.Timing);

            //PM 20140217 [19493] Ajout de l'heure des paramètres de calcul en cas de traitement Intra-Day
            if (this.Timing == SettlSessIDEnum.Intraday)
            {
                dtBusiness += this.RiskDataTime;
            }
            else
            {
                // PM 20150507 [20575] Ajout gestion dtMarket (EOD only)
                dtBusiness = GetRiskParametersDate(pCS);
            }
            DtRiskParameters = dtBusiness;
            //
            m_EmptyExchangeComplexParameters = new SpanExchangeComplex { SPANExchangeComplexId = 0, ExchangeComplex = Cst.NotFound };
            m_EmptyCombinedGroupParameters = new SpanCombinedGroup { SPANCombinedGroupId = 0, SPANExchangeComplexId = 0, CombinedGroupCode = Cst.NotFound };
            m_EmptyContractGroupParameters = new SpanContractGroup { SPANContractGroupId = 0, SPANCombinedGroupId = 0, ContractGroupCode = Cst.NotFound };
            m_EmptyContractParameters = new SpanContract { SPANExchangeId = 0, SPANContractGroupId = 0, SPANContractId = 0, ContractSymbol = Cst.NotFound };
            m_EmptyRiskArrayParameters = new SpanRiskArray { SPANContractId = 0 };

            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                // Set Parameters
                dbParametersValue.Add("DTBUSINESS", dtBusiness);
                dbParametersValue.Add("IDA_CSS", this.IdCSS);
                // CLEARINGORG_SPANMETHOD
                // PM 20160404 [22116] N'est plus utilisé
                //m_ClearingOrganizationParameters = LoadParametersMethod<SpanClearingOrganization>.LoadParameters(connection, dbParametersValue, DataContractResultSets.CLEARINGORG_SPANMETHOD).FirstOrDefault();

                if (pAssetETDCache.Count > 0)
                {
                    // ASSETEXPANDED_ALLMETHOD
                    m_AssetExpandedParameters = LoadParametersAssetExpanded(connection);

                    // Set Parameters
                    dbParametersValue.Clear();
                    dbParametersValue.Add("DTBUSINESS", dtBusiness);
                    dbParametersValue.Add("TIMING", timing);

                    // CURRENCY_SPANMETHOD
                    m_CurrencyParameters = LoadParametersMethod<SpanCurrency>.LoadParameters(connection, dbParametersValue, DataContractResultSets.CURRENCY_SPANMETHOD);
                    // CURCONV_SPANMETHOD
                    m_CurConvParameters = LoadParametersMethod<SpanCurConv>.LoadParameters(connection, dbParametersValue, DataContractResultSets.CURCONV_SPANMETHOD);

                    // INTERSPREAD_SPANMETHOD
                    m_InterSpreadParameters = LoadParametersMethod<SpanInterSpread>.LoadParameters(connection, dbParametersValue, DataContractResultSets.INTERSPREAD_SPANMETHOD);
                    m_InterSpreadParameters.OrderBy(s => s.SpreadPriority);
                    // INTERLEG_SPANMETHOD
                    m_InterLegParameters = LoadParametersMethod<SpanInterLeg>.LoadParameters(connection, dbParametersValue, DataContractResultSets.INTERLEG_SPANMETHOD);
                    // PM 20130502 [18623] Suppression de la restriction sur les assets en position
                    // MATURITYTIER_SPANMETHOD
                    m_MaturityTierParameters = LoadParametersMethod<SpanMaturityTier>.LoadParameters(connection, dbParametersValue, DataContractResultSets.MATURITYTIER_SPANMETHOD);

                    //PM 20160829 [22420] Chargement des Maturity remonté car n'utilisant plus le paramètre SESSIONID
                    // MATURITY_SPANMETHOD
                    m_MaturityParameters = LoadParametersMethod<SpanMaturity>.LoadParameters(connection, dbParametersValue, DataContractResultSets.MATURITY_SPANMETHOD);

                    // Add Parameters 
                    // EXCHANGECOMPLEX_SPANMETHOD
                    m_ExchangeComplexParameters = LoadParametersMethod<SpanExchangeComplex>.LoadParameters(connection, dbParametersValue, DataContractResultSets.EXCHANGECOMPLEX_SPANMETHOD);
                    // EXCHANGE_SPANMETHOD
                    m_ExchangeParameters = LoadParametersMethod<SpanExchange>.LoadParameters(connection, dbParametersValue, DataContractResultSets.EXCHANGE_SPANMETHOD);
                    // COMBINEDGROUP_SPANMETHOD
                    m_CombinedGroupParameters = LoadParametersMethod<SpanCombinedGroup>.LoadParameters(connection, dbParametersValue, DataContractResultSets.COMBINEDGROUP_SPANMETHOD);
                    // CONTRACTGROUP_SPANMETHOD
                    m_ContractGroupParameters = LoadParametersMethod<SpanContractGroup>.LoadParameters(connection, dbParametersValue, DataContractResultSets.CONTRACTGROUP_SPANMETHOD);
                    // CONTRACT_SPANMETHOD
                    m_ContractParameters = LoadParametersMethod<SpanContract>.LoadParameters(connection, dbParametersValue, DataContractResultSets.CONTRACT_SPANMETHOD);
                    // DELIVERYMONTH_SPANMETHOD
                    m_DeliveryMonthParameters = LoadParametersMethod<SpanDeliveryMonth>.LoadParameters(connection, dbParametersValue, DataContractResultSets.DELIVERYMONTH_SPANMETHOD);
                    // INTRASPREAD_SPANMETHOD
                    m_IntraSpreadParameters = LoadParametersMethod<SpanIntraSpread>.LoadParameters(connection, dbParametersValue, DataContractResultSets.INTRASPREAD_SPANMETHOD);
                    // INTRALEG_SPANMETHOD
                    m_IntraLegParameters = LoadParametersMethod<SpanIntraLeg>.LoadParameters(connection, dbParametersValue, DataContractResultSets.INTRALEG_SPANMETHOD);
                    //PM 20160829 [22420] Chargement des Maturity remonté car n'utilisant plus le paramètre SESSIONID
                    //// MATURITY_SPANMETHOD
                    //m_MaturityParameters = LoadParametersMethod<SpanMaturity>.LoadParameters(connection, dbParametersValue, DataContractResultSets.MATURITY_SPANMETHOD);
                    // RISKARRAY_SPANMETHOD
                    m_RiskArrayParameters = LoadParametersMethod<SpanRiskArray>.LoadParameters(connection, dbParametersValue, DataContractResultSets.RISKARRAY_SPANMETHOD);

                    // PM 20190801 [24717] Ajout chargement paramètres pour le Concentration Risk Margin d'ECC
                    if (m_ImMethodParameter.IsCalcECCConcentrationRiskMargin)
                    {
                        m_ECCMarketVolumeParameters = LoadParametersMethod<ECCRiskMarketVolume>.LoadParameters(connection, dbParametersValue, DataContractResultSets.MARKETVOLUME_ECCSPANMETHOD);
                        m_ECCCombComStressParameters = LoadParametersMethod<ECCRiskCombComStressContract>.LoadParameters(connection, dbParametersValue, DataContractResultSets.STRESSCOMBCOMCONTRACT_ECCSPANMETHOD);
                    }
                }
            }
        }

        /// <summary>
        /// Libère les paramètres spécifiques à SPAN.
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            // PM 20160404 [22116] N'est plus utilisé
            //m_ClearingOrganizationParameters = null;
            m_AssetExpandedParameters = null;
            //
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //m_FutureMonth = null;
            //m_TierMonth = null;
            //
            m_ExchangeComplexParameters = null;
            m_ExchangeParameters = null;
            m_CurrencyParameters = null;
            m_CurConvParameters = null;
            m_InterSpreadParameters = null;
            m_InterLegParameters = null;
            m_CombinedGroupParameters = null;
            m_ContractGroupParameters = null;
            m_ContractParameters = null;
            m_DeliveryMonthParameters = null;
            m_MaturityTierParameters = null;
            m_IntraSpreadParameters = null;
            m_IntraLegParameters = null;
            m_MaturityParameters = null;
            m_RiskArrayParameters = null;
            //
            m_ECCMarketVolumeParameters = null;
            m_ECCCombComStressParameters = null;
            //
            m_EmptyExchangeComplexParameters = null;
            m_EmptyCombinedGroupParameters = null;
            m_EmptyContractGroupParameters = null;
            m_EmptyContractParameters = null;
            m_EmptyRiskArrayParameters = null;
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
            SpanMarginCalcMethCom methodComObj = new SpanMarginCalcMethCom();
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            SpanEvaluationData evaluationData = new SpanEvaluationData();
            //
            GetSpanAccountParameters(evaluationData, pActorId);
            //
            // Create the calculation sheet communication object (used to write the calculation sheet)
            opMethodComObj = methodComObj;
            methodComObj.MarginMethodType = this.Type;
            methodComObj.AccountType = evaluationData.SpanAccountType;
            methodComObj.IsMaintenanceAmount = evaluationData.IsMaintenanceAmount;
            //PM 20150511 [20575] Ajout date des paramètres de risque
            methodComObj.DtParameters = DtRiskParameters;

            // Set the css currency
            // PM 20160404 [22116] utilisation de m_CssCurrency au lieu m_ClearingOrganizationParameters.Currency
            methodComObj.CssCurrency = m_CssCurrency;

            // PM 20190801 [24717] Calcul du Concentration Risk Margin de l'ECC
            methodComObj.IsCalcECCConcentrationRiskMargin = MethodParameter.IsCalcECCConcentrationRiskMargin;

            // PM 20190801 [24717] Addon pour le Concentration Risk Margin de l'ECC
            methodComObj.AdditionalAddOn = MethodParameter.ECCAddOnDays;

            if (pRiskDataToEvaluate != default(RiskData))
            {
                // PM 20170313 [22833] Prendre uniquement la position (à l'ancien format)
                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsToEvaluate = pRiskDataToEvaluate.GetPositionAsEnumerablePair();

                if ((positionsToEvaluate != null) && (positionsToEvaluate.Count() > 0))
                {
                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions;
                    // PM 20190401 [24625][24387] Ajout positionsFutAtExpiry
                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsFutAtExpiry; 

                    // Group the positions by asset (the side of the new merged assets will be set with regards to the long and short quantities)
                    switch (evaluationData.SpanAccountType)
                    {
                        case SpanAccountType.OmnibusHedger:
                        case SpanAccountType.OmnibusSpeculator:
                            positions = positionsToEvaluate;
                            break;
                        default:
                            positions = PositionsGrouping.GroupPositionsByAsset(positionsToEvaluate);
                            break;
                    }

                    // PM 20190401 [24625][24387] Recherche Position Future At Expiry pour le calcul de l'Additional Margin BoM
                    positionsFutAtExpiry = 
                        from pos in positions
                        where (pos.Second.DeliveryQuantity != 0)
                        && (pos.Second.DeliveryStep == InitialMarginDeliveryStepEnum.Expiry)
                        && (pos.Second.DeliveryExpressionType == ExpressionType.ECC_AMBO)
                        select pos;

                    // Ne garder que les positions dont la quantité est différente de 0
                    positions =
                        from pos in positions
                        where pos.Second.Quantity != 0
                        select pos;
                    evaluationData.Positions = positions;

                    // Coverage short call and short futures (this one modify the position quantity)
                    IEnumerable<CoverageSortParameters> inputCoverage = GetSortParametersForCoverage(positions);
                    // Reduction de la position couverte
                    // FI 20160613 [22256]
                    Pair<IEnumerable<StockCoverageCommunicationObject>, IEnumerable<StockCoverageDetailCommunicationObject>> coveredQuantities =
                        ReducePosition(pActorId, pBookId, pDepositHierarchyClass, inputCoverage, ref positions);

                    // Calculer les montants de risque
                    EvaluateRisk(evaluationData, positions);

                    // Build all the SPAN contract group needed to evaluate the positions set,
                    // filling them with the associated positions set
                    methodComObj.Parameters = GetSpanHierarchy(evaluationData, positions).ToArray();
                    // FI 20160613 [22256] Alimentation de UnderlyingStock
                    methodComObj.UnderlyingStock = coveredQuantities.Second;

                    // Remplir les objets pour le journal
                    // PM 20190401 [24625][24387] Changement de paramètre pour pouvoir réaliser des appels multiples
                    //FillMethodCom(evaluationData, methodComObj);
                    FillMethodCom(evaluationData, (SpanExchangeComplexCom[])methodComObj.Parameters);

                    // Cumuler les montants de risque par devise
                    riskAmounts = CumulateRiskElement(evaluationData);

                    // PM 20190401 [24625][24387] Traitement Additional Margin BoM de l'ECC
                    if (positionsFutAtExpiry.Count() > 0)
                    {
                        // Evaluation détaillée
                        SpanEvaluationData evaluationAMBO = EvaluateAMBO(positionsFutAtExpiry);
                        //
                        // Calcul des cumuls par regroupement
                        List<Money> riskAMBOAmounts = CumulateRiskElement(evaluationAMBO);
                        //
                        if ((riskAMBOAmounts != default) && (riskAMBOAmounts.Count > 0))
                        {
                            // Cumul avec les montants classiques de tous les montants par devise
                            riskAmounts = (
                                from amount in riskAMBOAmounts.Concat(riskAmounts)
                                group amount by amount.Currency
                                    into amountCur
                                    select new Money(amountCur.Select(a => a.Amount.DecValue).Sum(), amountCur.Key)
                                    ).ToList();
                            //
                            methodComObj.AdditionalMarginBoM = new SpanAdditionalMarginBoMCom
                            {
                                Parameters = GetSpanHierarchy(evaluationAMBO, evaluationAMBO.Positions).ToArray()
                            };
                            FillMethodCom(evaluationAMBO, (SpanExchangeComplexCom[])methodComObj.AdditionalMarginBoM.Parameters);
                        }
                    }

                    // PM 20190801 [24717] Calcul du Concentration Risk Margin de l'ECC
                    if ((MethodParameter.IsCalcECCConcentrationRiskMargin) && (riskAmounts != default))
                    {
                        List<ECCConRiskMargin> conRiskCur = new List<ECCConRiskMargin>();
                        foreach (Money spanAmount in riskAmounts)
                        {
                            ECCConRiskMargin conrRisk = EvaluteConcentrationRiskMargin(positions, spanAmount, m_ImMethodParameter.ECCAddOnDays);
                            if ((conrRisk != default(ECCConRiskMargin)) && (conrRisk.ConcentrationRiskMargin != default))
                            {
                                spanAmount.Amount.DecValue += conrRisk.ConcentrationRiskMargin.Amount.DecValue;
                                conRiskCur.Add(conrRisk);
                            }
                        }
                        methodComObj.ConcentrationRiskMargin = ConcentrationRiskMarginCom(conRiskCur);
                    }
                }
            }
            if (riskAmounts == null)
            {
                riskAmounts = new List<Money>();
            }
            if (riskAmounts.Count == 0)
            {
                if (StrFunc.IsEmpty(methodComObj.CssCurrency))
                {
                    riskAmounts.Add(new Money(0, "EUR"));
                }
                else
                {
                    riskAmounts.Add(new Money(0, methodComObj.CssCurrency));
                }
            }
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            evaluationData = default;
            //
            return riskAmounts;
        }

        /// <summary>
        /// Get a collection of sorting parameter needed by coverage strategies
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">Positions of the current risk element</param>
        /// <returns>A collection of sorting parameters in order to be used inside of the ReducePosition method </returns>
        protected override IEnumerable<CoverageSortParameters> GetSortParametersForCoverage(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            return from position in pGroupedPositionsByIdAsset
                   join asset in m_RiskArrayParameters on position.First.idAsset equals asset.IdAsset
                   join maturity in m_AssetExpandedParameters on asset.IdAsset equals maturity.AssetId
                   select
                       new CoverageSortParameters
                       {
                           AssetId = position.First.idAsset,
                           ContractId = maturity.ContractId,
                           MaturityYearMonth = decimal.Parse(maturity.MaturityYearMonth),
                           Multiplier = maturity.ContractMultiplier,
                           Quote = asset.Price,
                           StrikePrice = asset.StrikePrice,
                           Type = RiskMethodExtensions.GetTypeFromCategoryPutCall(maturity.CategoryEnum, asset.PutOrCall),
                       }; ;
        }

        /// <summary>
        /// Lecture d'informations complémentaire pour les Marchés/Chambre de compensation utilisant la méthode courante 
        /// </summary>
        /// <param name="pEntityMarkets">La collection de entity/market attaché à la chambre de compensation courante</param>
        public override void BuildMarketParameters(IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        {
            base.BuildMarketParameters(pEntityMarkets);
        }
        #endregion override base methods

        #region static methods
        /// <summary>
        /// Calcul une date en fonction du code Maturity (correspondant au début d'un interval)
        /// </summary>
        /// <param name="pMaturityMonth"></param>
        /// <returns></returns>
        internal static DateTime SPANMaturityMonthToDateTime(string pMaturityMonth)
        {
            return SPANMaturityMonthToDateTime(pMaturityMonth, MaturityMonthIntervalEnum.Start);
        }

        /// <summary>
        /// Calcul une date en fonction du code Maturity d'une borne d'un interval
        /// </summary>
        /// <param name="pMaturityMonth"></param>
        /// <param name="pInterval"></param>
        /// <returns></returns>
        /// EG 20170412 Inversion des test sur valeur de pMaturityMonth
        internal static DateTime SPANMaturityMonthToDateTime(string pMaturityMonth, MaturityMonthIntervalEnum pInterval)
        {
            DateTime maturity = DateTime.MinValue;
            if (StrFunc.IsFilled(pMaturityMonth))
            {
                // Format YYYYMM
                // EG 20170414 Inversion des test sur valeur pMaturityMonth
                if (pMaturityMonth == "00000000")
                {
                    maturity = DateTime.MinValue;
                }
                else if (pMaturityMonth == "99999999")
                {
                    maturity = DateTime.MaxValue;
                }
                else
                {
                    string yearMonth;
                    if ((pMaturityMonth.Length == 6) || ((pMaturityMonth.Length == 8) && (pMaturityMonth.Substring(6, 2) == "00")))
                    {
                        if (pMaturityMonth.Length == 8)
                        {
                            pMaturityMonth = pMaturityMonth.Substring(0, 6);
                        }
                        yearMonth = pMaturityMonth + "01";
                        if (true == StrFunc.IsDate(yearMonth, DtFunc.FmtDateyyyyMMdd))
                        {
                            maturity = DtFunc.ParseDate(yearMonth, DtFunc.FmtDateyyyyMMdd, null);
                        }
                        if ((MaturityMonthIntervalEnum.End == pInterval) && (maturity != DateTime.MaxValue))
                        {
                            maturity = maturity.AddMonths(1).AddDays(-1);
                        }
                    }
                    else if (pMaturityMonth.Length == 8)
                    {
                        int indexW = pMaturityMonth.ToUpper().IndexOf('W');
                        // Format YYYYMMwN
                        if (indexW == 6)
                        {
                            yearMonth = pMaturityMonth.Substring(0, 6) + "01";
                            if (true == StrFunc.IsDate(yearMonth, DtFunc.FmtDateyyyyMMdd))
                            {
                                int week = int.Parse(pMaturityMonth.Substring(6, 1));
                                maturity = DtFunc.ParseDate(yearMonth, DtFunc.FmtDateyyyyMMdd, null).AddDays((week - 1) * 7);
                            }
                        }
                        else if (true == StrFunc.IsDate(pMaturityMonth, DtFunc.FmtDateyyyyMMdd))
                        {
                            // Format YYYYMMDD
                            maturity = DtFunc.ParseDate(pMaturityMonth, DtFunc.FmtDateyyyyMMdd, null);
                        }
                    }
                }
            }
            return maturity;
        }

        /// <summary>
        /// Ajout des valeurs des scénarios de risque de l'asset
        /// </summary>
        /// <param name="pSumRiskValue">Valeurs des scénarios de risque cumulées</param>
        /// <param name="pRiskValues">Valeurs des scénarios de risque de l'asset en position à cumuler</param>
        public static Dictionary<int, decimal> AddRiskValues(Dictionary<int, decimal> pSumRiskValue, IEnumerable<KeyValuePair<int, decimal>> pRiskValues)
        {
            if ((null != pSumRiskValue) && (null != pRiskValues))
            {
                // Cumul des matrices
                foreach (var riskvalue in pRiskValues)
                {
                    if (pSumRiskValue.ContainsKey(riskvalue.Key))
                    {
                        pSumRiskValue[riskvalue.Key] += riskvalue.Value;
                    }
                    else
                    {
                        pSumRiskValue.Add(riskvalue.Key, riskvalue.Value);
                    }
                }
            }
            return pSumRiskValue;
        }
        #endregion static methods

        #region abstract methods
        /// <summary>
        /// Calcul du risque pondéré de prix future.
        /// </summary>
        /// <param name="pRisk"></param>
        /// <param name="pDelta"></param>
        /// <returns></returns>
        public abstract decimal SpanWeightedFuturePriceRisk(decimal pRisk, decimal pDelta);
        #endregion abstract methods

        #region virtual methods
        /// <summary>
        /// Definition de l'arrondi du nombre de delta.
        /// Utilisé en particulier pour le nombre de Spread Intra Contract Group réalisable
        /// (différent entre la méthode CME et C21)
        /// </summary>
        /// <param name="pDeltaNumber"></param>
        /// <returns></returns>
        public virtual decimal SpanRoundDelta(decimal pDeltaNumber)
        {
            return RoundAmount(pDeltaNumber, DeltaValuePrecision);
        }

        /// <summary>
        /// Definition de l'arrondi du nombre de spread.
        /// Utilisé en particulier pour le nombre de Spread Inter Contract Group réalisable
        /// (différent entre la méthode CME et C21)
        /// </summary>
        /// <param name="pSpreadNumber"></param>
        /// <returns></returns>
        public virtual decimal SpanRoundSpreadNumber(decimal pSpreadNumber)
        {
            EFS_Round round = new EFS_Round(RoundingDirectionEnum.Down, DeltaValuePrecision, pSpreadNumber);
            return round.AmountRounded;
        }

        /// <summary>
        /// Arrondie un montant selon les préconisations SPAN
        /// </summary>
        /// <param name="pAmount">Montant à arrondir</param>
        /// <returns>Montant arrondi</returns>
        public virtual decimal SpanRoundAmount(decimal pAmount)
        {
            return RoundAmount(pAmount, RiskValuePrecision);
        }

        /// <summary>
        /// Arrondie utile pour le WeightedFuturePriceRisk, par défaut 2 décimales.
        /// </summary>
        /// <param name="pAmount">Montant à arrondir</param>
        /// <returns>Montant arrondie</returns>
        /// PM 20150928 [21405][20948] New
        public virtual decimal SpanRoundWeightedFuturePriceRisk(decimal pAmount)
        {
            return RoundAmount(pAmount, 2);
        }

        /// <summary>
        /// Calcul les matrices et paramètres de risques de chaque position reçue en paramètres
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pPositions">Ensemble de position</param>
        /// <returns>Les données de risque de chaque position reçue en paramètres</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //internal virtual List<SpanAssetRiskValues> ComputeAssetScanTiersRiskValues(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        internal virtual List<SpanAssetRiskValues> ComputeAssetScanTiersRiskValues(SpanEvaluationData pEvaluationData, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            List<SpanAssetRiskValues> assetScanTiersRiskValues = null;

            // Calcul des matrices de risque sur chaque asset en fonction de la position
            IEnumerable<SpanAssetRiskValues> assetScanValues =
                from position in pPositions
                join riskArrayParam in m_RiskArrayParameters on position.First.idAsset equals riskArrayParam.IdAsset
                join maturityParam in pEvaluationData.FutureMonthParam on new { riskArrayParam.SPANContractId, riskArrayParam.FutureMaturity, riskArrayParam.OptionMaturity } equals new { maturityParam.SPANContractId, maturityParam.FutureMaturity, maturityParam.OptionMaturity }
                join assetMatParam in m_AssetExpandedParameters on riskArrayParam.IdAsset equals assetMatParam.AssetId
                join contractParam in m_ContractParameters on riskArrayParam.SPANContractId equals contractParam.SPANContractId
                join contractGroupParam in m_ContractGroupParameters on contractParam.SPANContractGroupId equals contractGroupParam.SPANContractGroupId
                select new SpanAssetRiskValues
                {
                    AssetId = riskArrayParam.IdAsset,
                    SPANContractId = riskArrayParam.SPANContractId,
                    Category = assetMatParam.Category,
                    FutureMaturity = riskArrayParam.FutureMaturity,
                    OptionMaturity = riskArrayParam.OptionMaturity,
                    PutOrCall = riskArrayParam.PutOrCall,
                    StrikePrice = riskArrayParam.StrikePrice,
                    Side = position.First.Side == SPANMethod.FixBuySide ? SideEnum.Buy : SideEnum.Sell,
                    Quantity = position.Second.Quantity * (position.First.Side == SPANMethod.FixBuySide ? 1 : -1),
                    Delta = riskArrayParam.CompositeDelta * position.Second.Quantity * (position.First.Side == SPANMethod.FixBuySide ? 1 : -1),
                    RiskValue = (from values in new List<KeyValuePair<int, decimal>>
                                   { 
                                        new KeyValuePair<int,decimal>(1,riskArrayParam.RiskValue1),
                                        new KeyValuePair<int,decimal>(2,riskArrayParam.RiskValue2),
                                        new KeyValuePair<int,decimal>(3,riskArrayParam.RiskValue3),
                                        new KeyValuePair<int,decimal>(4,riskArrayParam.RiskValue4),
                                        new KeyValuePair<int,decimal>(5,riskArrayParam.RiskValue5),
                                        new KeyValuePair<int,decimal>(6,riskArrayParam.RiskValue6),
                                        new KeyValuePair<int,decimal>(7,riskArrayParam.RiskValue7),
                                        new KeyValuePair<int,decimal>(8,riskArrayParam.RiskValue8),
                                        new KeyValuePair<int,decimal>(9,riskArrayParam.RiskValue9),
                                        new KeyValuePair<int,decimal>(10,riskArrayParam.RiskValue10),
                                        new KeyValuePair<int,decimal>(11,riskArrayParam.RiskValue11),
                                        new KeyValuePair<int,decimal>(12,riskArrayParam.RiskValue12),
                                        new KeyValuePair<int,decimal>(13,riskArrayParam.RiskValue13),
                                        new KeyValuePair<int,decimal>(14,riskArrayParam.RiskValue14),
                                        new KeyValuePair<int,decimal>(15,riskArrayParam.RiskValue15),
                                        new KeyValuePair<int,decimal>(16,riskArrayParam.RiskValue16)
                                   }
                                 select new KeyValuePair<int, decimal>(values.Key, values.Value * position.Second.Quantity * (position.First.Side == SPANMethod.FixBuySide ? 1 : -1))
                                 ).ToList(),
                    // PM 20130806 [18876] For Contract with Variable Tick Value
                    //OptionValue = assetMatParam.Category == "O" ? RiskMethodExtensions.ContractValue(position.Second.Quantity, riskArrayParam.Price, assetMatParam.ContractMultiplier, assetMatParam.InstrumentNum, assetMatParam.InstrumentDen) : 0,
                    // PM 20150707 [21104] Simplification et Gestion arrondie
                    //OptionValue = assetMatParam.Category == "O" ? RiskMethodExtensions.ContractValue(position.Second.Quantity, riskArrayParam.Price, contractParam.IsOptionVariableTick ? riskArrayParam.ContractValueFactor : assetMatParam.ContractMultiplier, assetMatParam.InstrumentNum, assetMatParam.InstrumentDen) : 0,
                    // PM 20200220 [25195][25215] Ajout restriction sur ValuationMethodEnum == PremiumStyle
                    //OptionValue = assetMatParam.Category == "O" ? RiskMethodExtensions.ContractValue(assetMatParam, position.Second.Quantity, riskArrayParam.Price, contractParam.IsOptionVariableTick, riskArrayParam.ContractValueFactor) : 0,
                    OptionValue = ((assetMatParam.Category == "O") && (assetMatParam.ValuationMethodEnum == FuturesValuationMethodEnum.PremiumStyle)) ? RiskMethodExtensions.ContractValue(assetMatParam, position.Second.Quantity, riskArrayParam.Price, contractParam.IsOptionVariableTick, riskArrayParam.ContractValueFactor) : 0,
                    // PM 20150902 [21385] Ajout contractParam.DeltaScalingFactor
                    //ShortOptionMinimum = ((assetMatParam.Category == "O") && (position.First.side == SPANMethod.FixSellSide)) ? contractGroupParam.ShortOptionChargeRate * maturityParam.DeltaScalingFactor * position.Second.Quantity : 0,
                    ShortOptionMinimum = ((assetMatParam.Category == "O") && (position.First.Side == SPANMethod.FixSellSide)) ? contractGroupParam.ShortOptionChargeRate * maturityParam.DeltaScalingFactor * contractParam.DeltaScalingFactor * position.Second.Quantity : 0,
                    MarginCurrency = contractGroupParam.MarginCurrency ?? assetMatParam.Currency,
                };

            if (null != assetScanValues)
            {
                assetScanTiersRiskValues = assetScanValues.ToList();
            }

            return assetScanTiersRiskValues;
        }

        /// <summary>
        /// Récupert les paramètres de calcul SPAN de l'acteur spécifié
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pActorId">Identifiant de l'acteur pour lequel le calcul SPAN est réalisé</param>
        /// <returns>true si l'acteur a été trouvé, sinon false</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //protected virtual bool GetSpanAccountParameters(int pActorId)
        internal virtual bool GetSpanAccountParameters(SpanEvaluationData pEvaluationData, int pActorId)
        {
            bool actorHasParameter = false;
            //
            if (pEvaluationData != default)
            {
                // Valeurs par défaut
                pEvaluationData.SpanAccountType = SpanAccountType.Member;
                // PM 20230322 [26282][WI607] Ajout utilisation de MethodParameter.IsMaintenance
                // pEvaluationData.IsMaintenanceAmount = true;
                pEvaluationData.IsMaintenanceAmount = MethodParameter.IsMaintenance;
                //
                if (MarginReqOfficeParameters != null)
                {
                    actorHasParameter = MarginReqOfficeParameters.ContainsKey(pActorId);
                    if (actorHasParameter)
                    {
                        MarginReqOfficeParameter specificClearingHouseParam =
                                   (from parameter in MarginReqOfficeParameters[pActorId]
                                    where parameter.CssId == this.IdCSS
                                    select parameter)
                                   .FirstOrDefault();
                        //
                        // PM 20170106 [22633] Ajout test de présence du paramétrage
                        if ((specificClearingHouseParam != default(MarginReqOfficeParameter)) && (specificClearingHouseParam.ActorId != 0))
                        {
                            pEvaluationData.SpanAccountType = specificClearingHouseParam.SpanAccountType;
                            pEvaluationData.IsMaintenanceAmount = specificClearingHouseParam.SpanMaintenanceAmountIndicator;
                            // PM 20150930 [21134] Add ScanRiskOffsetCapPrct
                            pEvaluationData.ScanRiskOffsetCapPrct = specificClearingHouseParam.ScanRiskOffsetCapPrct;
                            if ((pEvaluationData.ScanRiskOffsetCapPrct < 0) || (pEvaluationData.ScanRiskOffsetCapPrct > 1))
                            {
                                pEvaluationData.ScanRiskOffsetCapPrct = 1;
                            }
                        }
                    }
                }
            }
            return actorHasParameter;
        }

        /// <summary>
        /// Cumculate all risk amounts by currency
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <returns>a list of risk amounts</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //internal protected virtual List<Money> CumulateRiskElement()
        internal virtual List<Money> CumulateRiskElement(SpanEvaluationData pEvaluationData)
        {
            List<Money> riskAmmounts = null;
            //
            if ((pEvaluationData != default) && (pEvaluationData.ContractGroupRisk != null))
            {
                bool isOptionValueLimit = false;
                if (m_ExchangeComplexParameters != null)
                {
                    SpanExchangeComplex exchangeComplex = m_ExchangeComplexParameters.FirstOrDefault();
                    if (exchangeComplex != default(SpanExchangeComplex))
                    {
                        isOptionValueLimit = exchangeComplex.IsOptionValueLimit;
                    }
                }

                riskAmmounts = new List<Money>();

                var currencyContractGroup =
                    from contractGroup in pEvaluationData.ContractGroupRisk
                    group contractGroup by contractGroup.Currency into currencyGroup
                    select currencyGroup;

                foreach (var contractGroup in currencyContractGroup)
                {
                    Money deposit = null;
                    if (pEvaluationData.IsMaintenanceAmount == true)
                    {
                        if (isOptionValueLimit == true)
                        {
                            deposit = new Money(contractGroup.Sum(obj => System.Math.Max(0, obj.RiskMaintenance - obj.OptionValue)), contractGroup.Key);
                        }
                        else
                        {
                            deposit = new Money(System.Math.Max(0, contractGroup.Sum(obj => obj.RiskMaintenance) - contractGroup.Sum(obj => obj.OptionValue)), contractGroup.Key);
                        }
                    }
                    else
                    {
                        if (isOptionValueLimit == true)
                        {
                            deposit = new Money(contractGroup.Sum(obj => System.Math.Max(0, obj.RiskInitial - obj.OptionValue)), contractGroup.Key);
                        }
                        else
                        {
                            deposit = new Money(System.Math.Max(0, contractGroup.Sum(obj => obj.RiskInitial) - contractGroup.Sum(obj => obj.OptionValue)), contractGroup.Key);
                        }
                    }
                    riskAmmounts.Add(deposit);
                }
            }
            return riskAmmounts;
        }

        /// <summary>
        /// Converti les matrices de risque d'un contrat de la devise du contrat en la devise de deposit
        /// </summary>
        /// <param name="pContractRisk">Liste des valeurs de risque du contrat</param>
        /// <returns>Liste des valeurs de risque du contrat converties</returns>
        internal virtual SpanContractRisk ConvertContractRiskValues(SpanContractRisk pContractRisk)
        {
            if (null != pContractRisk)
            {
                pContractRisk.ConvertedRiskValue = pContractRisk.RiskValue;
                pContractRisk.ConvertedRiskValueLong = pContractRisk.RiskValueLong;
                pContractRisk.ConvertedRiskValueShort = pContractRisk.RiskValueShort;

                pContractRisk.ConvertedLongOptionValue = pContractRisk.LongOptionValue;
                pContractRisk.ConvertedShortOptionValue = pContractRisk.ShortOptionValue;
            }
            return pContractRisk;
        }

        /// <summary>
        /// Calcul du Delta Net par Future Maturity des positions reçues en paramètres
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pAssetRisk">Valeur de risque de chaque série en position</param>
        /// PM 20151127 [21571][21605] EvaluateFutureMonthDeltaRisk passe de private à internal virtual
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //internal virtual void EvaluateFutureMonthDeltaRisk(List<SpanAssetRiskValues> pAssetRisk)
        internal virtual void EvaluateFutureMonthDeltaRisk(SpanEvaluationData pEvaluationData, List<SpanAssetRiskValues> pAssetRisk)
        {
            // Ensemble des Delta par Contrat et par échéance future avec recherche du Delta Scaling Factor
            // PM 20130605 [18730] Add AssetRisk
            // PM 20150902 [21385] Ajout jointure avec m_ContractParameters pour ajouter les membres ContractParameters et CappedWeighedRisk
            var assetFutureMonth =
                from asset in pAssetRisk
                join contractParam in m_ContractParameters on asset.SPANContractId equals contractParam.SPANContractId
                join maturityParam in pEvaluationData.FutureMonthParam on new { asset.SPANContractId, asset.FutureMaturity, asset.OptionMaturity } equals new { maturityParam.SPANContractId, maturityParam.FutureMaturity, maturityParam.OptionMaturity }
                select new
                {
                    ContractParameters = contractParam,
                    MaturityParameters = maturityParam,
                    DeltaLong = asset.Delta > 0 ? asset.Delta : 0,
                    DeltaShort = asset.Delta < 0 ? asset.Delta : 0,
                    AssetRisk = asset,
                    CappedWeighedRisk = maturityParam.FuturePriceScanRange / (maturityParam.DeltaScalingFactor * contractParam.DeltaScalingFactor),
                };

            // Somme des Delta Long et des Delta Short par echeance
            // PM 20130605 [18730] Ajout paramètre this et liste des SpanAssetRiskValues
            // PM 20150902 [21385] Réécriture de la requête pour alimenter futureMonth
            //IEnumerable<SpanFutureMonthRisk> futureMonth =
            //    from assetMonth in assetFutureMonth
            //    join contractParam in m_ContractParameters on assetMonth.MaturityParameters.SPANContractId equals contractParam.SPANContractId
            //    join contractGroupParam in m_ContractGroupParameters on contractParam.SPANContractGroupId equals contractGroupParam.SPANContractGroupId
            //    group assetMonth by new { contractGroupParam, assetMonth.MaturityParameters.FutureMaturity } into futMonthD
            //    orderby futMonthD.Key.contractGroupParam.SPANContractGroupId, futMonthD.Key.FutureMaturity
            //    select new SpanFutureMonthRisk(
            //        this,
            //        futMonthD.Key.contractGroupParam,
            //        futMonthD.Key.FutureMaturity,
            //        futMonthD.First(f => SPANMaturityMonthToDateTime(f.MaturityParameters.FutureMaturity) == futMonthD.Min(m => SPANMaturityMonthToDateTime(m.MaturityParameters.FutureMaturity))).MaturityParameters,
            //        futMonthD.Sum(a => a.DeltaLong * a.MaturityParameters.DeltaScalingFactor),
            //        futMonthD.Sum(a => a.DeltaShort * a.MaturityParameters.DeltaScalingFactor),
            //        (from futM in futMonthD select futM.AssetRisk).ToList()
            //        )
            //    ;
            IEnumerable<SpanFutureMonthRisk> futureMonth =
                from assetMonth in assetFutureMonth
                join contractGroupParam in m_ContractGroupParameters on assetMonth.ContractParameters.SPANContractGroupId equals contractGroupParam.SPANContractGroupId
                group assetMonth by new { contractGroupParam, assetMonth.MaturityParameters.FutureMaturity } into futMonthD
                orderby futMonthD.Key.contractGroupParam.SPANContractGroupId, futMonthD.Key.FutureMaturity
                select new SpanFutureMonthRisk(
                    this,
                    pEvaluationData,
                    futMonthD.Key.contractGroupParam,
                    futMonthD.Key.FutureMaturity,
                    futMonthD.First(f => SPANMaturityMonthToDateTime(f.MaturityParameters.FutureMaturity) == futMonthD.Min(m => SPANMaturityMonthToDateTime(m.MaturityParameters.FutureMaturity))).MaturityParameters,
                    futMonthD.Sum(a => a.DeltaLong * a.MaturityParameters.DeltaScalingFactor * a.ContractParameters.DeltaScalingFactor),
                    futMonthD.Sum(a => a.DeltaShort * a.MaturityParameters.DeltaScalingFactor * a.ContractParameters.DeltaScalingFactor),
                    futMonthD.First().CappedWeighedRisk * futMonthD.Key.contractGroupParam.RiskMultiplier,
                    (from futM in futMonthD select futM.AssetRisk).ToList()
                    )
                ;

            pEvaluationData.FutureMonth = futureMonth.ToArray();
        }
        #endregion virtual methods

        #region methods
        /// <summary>
        /// Construction de la liste des risques par contrat en position
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pAssetRisk">Ensemble des des risques des assets en position</param>
        /// <returns>Liste des risques par contrat en position</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //internal List<SpanContractRisk> BuildContractRiskFromAssetRisk(IEnumerable<SpanAssetRiskValues> pAssetRisk)
        internal List<SpanContractRisk> BuildContractRiskFromAssetRisk(SpanEvaluationData pEvaluationData, IEnumerable<SpanAssetRiskValues> pAssetRisk)
        {
            // Recherche de toutes les matrices de risque des assets d'un groupe de contrat
            List<SpanContractRisk> contractScanTiersValues =
                (
                from contractParam in m_ContractParameters
                join assetScanTiers in pAssetRisk on contractParam.SPANContractId equals assetScanTiers.SPANContractId
                group assetScanTiers by contractParam into scanTiers
                select new SpanContractRisk(this, pEvaluationData, scanTiers.Key, scanTiers.ToList<SpanAssetRiskValues>())
                ).ToList();

            return contractScanTiersValues;
        }

        /// <summary>
        /// Construit l'ensemble des paramètres des différentes échéances en position
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pPositions">Les positions</param>
        /// <returns>L'ensemble des paramètres des différentes échéances en position</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private IEnumerable<SpanMaturity> BuildAllFutureMonthParametersFromAssetPosition(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        private IEnumerable<SpanMaturity> BuildAllFutureMonthParametersFromAssetPosition(SpanEvaluationData pEvaluationData, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            // Ensemble des échéances futures des assets en position
            var maturityDefaultParameters =
                (from position in pPositions
                 group position by position.First.idAsset
                     into assetPosition
                     join riskArrayParam in m_RiskArrayParameters on assetPosition.Key equals riskArrayParam.IdAsset
                     select new SpanMaturity()
                     {
                         SPANContractId = riskArrayParam.SPANContractId,
                         FutureMaturity = riskArrayParam.FutureMaturity,
                         OptionMaturity = riskArrayParam.OptionMaturity,
                         DeltaScalingFactor = 1,
                         FuturePriceScanRange = 0,
                     }).Distinct(new SpanMaturityComparer());

            // Ensemble des paramètres des échéances futures
            pEvaluationData.FutureMonthParam =
                from maturityDefaultParam in maturityDefaultParameters
                join maturityParam in m_MaturityParameters on new { maturityDefaultParam.SPANContractId, maturityDefaultParam.FutureMaturity, maturityDefaultParam.OptionMaturity } equals new { maturityParam.SPANContractId, maturityParam.FutureMaturity, maturityParam.OptionMaturity }
                into fm
                from outMaturity in fm.DefaultIfEmpty(maturityDefaultParam)
                select outMaturity;

            return pEvaluationData.FutureMonthParam;
        }

        /// <summary>
        /// Calcul du Delta Net par Tier (Période) à partir des Delta Net par Future Maturity
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private void EvaluateTierDeltaRisk()
        private void EvaluateTierDeltaRisk(SpanEvaluationData pEvaluationData)
        {
            // PM 20130605 [18730] Ajout paramètre this
            // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //pEvaluationData.TierMonth = SpanTierMonthRisk.EvaluateTierDeltaRisk(this, FutureMonth, m_MaturityTierParameters);
            pEvaluationData.TierMonth = SpanTierMonthRisk.EvaluateTierDeltaRisk(this, pEvaluationData, pEvaluationData.FutureMonth, m_MaturityTierParameters);
        }

        /// <summary>
        /// Calcul les différents montant de risque de la position
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pPositions">Position pour laquelle calcul les montatns de risque</param>
        // PM 20151224 [POC -MUREX]  Add Test(Type == InitialMarginMethodEnum.SPAN_CME)
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private void EvaluateRisk(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        private void EvaluateRisk(SpanEvaluationData pEvaluationData, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            // PM 20130701 [18791] Suppression du test sur la présence de position
            //if (null != pPositions)
            if ((pEvaluationData != default) && (pPositions != default))
            {
                // Construction des paramètres de l'ensemble des échéances en position
                BuildAllFutureMonthParametersFromAssetPosition(pEvaluationData, pPositions);

                // Calcul des matrices de risque sur chaque asset en fonction de la position
                List<SpanAssetRiskValues> assetScanTiersValues = ComputeAssetScanTiersRiskValues(pEvaluationData, pPositions);

                // Calcul des Delta Net par échéance future sur chaque groupe de contrat
                EvaluateFutureMonthDeltaRisk(pEvaluationData, assetScanTiersValues);

                // Calcul des Delta Net par Tier (période) sur chaque groupe de contrat
                EvaluateTierDeltaRisk(pEvaluationData);

                // Calcul des valeurs de risk des contrats
                pEvaluationData.ContractRisk = BuildContractRiskFromAssetRisk(pEvaluationData, assetScanTiersValues).ToArray();

                // Recherche de toutes les matrices de risque des contrats de chaque groupe de contrat
                var contractGroupScanTiersValues =
                    from contractGroupParam in m_ContractGroupParameters
                    join contractRisk in pEvaluationData.ContractRisk on contractGroupParam.SPANContractGroupId equals contractRisk.ContractParameters.SPANContractGroupId
                    group contractRisk by contractGroupParam into scanTiers
                    select scanTiers;

                // Calcul de risque de chaque groupe de contrat
                List<SpanContractGroupRisk> contractGroupScanTier = new List<SpanContractGroupRisk>(contractGroupScanTiersValues.Count());
                foreach (var contractGroup in contractGroupScanTiersValues)
                {
                    // PM 20161003 [22436] Recherche des ScanTiers et des SomTiers
                    IEnumerable<SpanTierMonthRisk> scanTierRiskOfContractGroup = pEvaluationData.TierMonth.Where(t => (t.TierParameters.TierType == SPANSpreadType.ScanTierType) && (t.TierParameters.SPANContractGroupId == contractGroup.Key.SPANContractGroupId));
                    IEnumerable<SpanTierMonthRisk> somTierRiskOfContractGroup = pEvaluationData.TierMonth.Where(t => (t.TierParameters.TierType == SPANSpreadType.SomTierType) && (t.TierParameters.SPANContractGroupId == contractGroup.Key.SPANContractGroupId));

                    // PM 20161003 [22436] Ajout paramètre tierRiskOfContractGroup
                    //SpanContractGroupRisk newContractGroup = new SpanContractGroupRisk(this, contractGroup.Key, contractGroup.ToList<SpanContractRisk>());
                    SpanContractGroupRisk newContractGroup = new SpanContractGroupRisk(this, pEvaluationData, contractGroup.Key, contractGroup, scanTierRiskOfContractGroup, somTierRiskOfContractGroup);
                    contractGroupScanTier.Add(newContractGroup);
                }

                pEvaluationData.ContractGroupRisk = contractGroupScanTier.ToList();

                // Ensemble des Groupes Combinés
                pEvaluationData.CombinedGroupRisk = (
                    from combinedGroupParam in m_CombinedGroupParameters
                    join contractGroup in pEvaluationData.ContractGroupRisk on combinedGroupParam.SPANCombinedGroupId equals contractGroup.ContractGroupParameters.SPANCombinedGroupId
                    group contractGroup by combinedGroupParam into groupRisk
                    select new SpanCombinedGroupRisk
                    {
                        CombinedGroupParameters = groupRisk.Key,
                        ContractGroupOfCombinedGroup = groupRisk.ToList(),
                    }).ToList();

                // Ensemble des Exchanges Complex
                pEvaluationData.ExchangeComplexRisk = (
                    from exchangeComplex in m_ExchangeComplexParameters
                    join combinedGroup in pEvaluationData.CombinedGroupRisk on exchangeComplex.SPANExchangeComplexId equals combinedGroup.CombinedGroupParameters.SPANExchangeComplexId
                    group combinedGroup by exchangeComplex into exchangeRisk
                    select new SpanExchangeComplexRisk
                    {
                        ExchangeComplexParameters = exchangeRisk.Key,
                        CombinedGroupOfExchange = exchangeRisk.ToList(),
                    }).ToList();

                // Parcours de la hiérachie pour calcul et construction du détail des calculs
                foreach (SpanExchangeComplexRisk exchangeComplex in pEvaluationData.ExchangeComplexRisk)
                {
                    #region Exchange Complex
                    if ((null != exchangeComplex.ExchangeComplexParameters) && (null != exchangeComplex.CombinedGroupOfExchange))
                    {
                        // PM 20151224 [POC -MUREX] 
                        if (Type == InitialMarginMethodEnum.SPAN_CME)
                        {
                            // Calcul du crédit pour super spread inter commodity (groupe de contrat)
                            exchangeComplex.SuperInterCommoditySpread = EvaluateSuperInterCommoditySpreadCredit(pEvaluationData, exchangeComplex.ExchangeComplexParameters.SPANExchangeComplexId);
                        }

                        // Construction des objets détaillant le calcul
                        foreach (SpanCombinedGroupRisk combinedGroup in exchangeComplex.CombinedGroupOfExchange)
                        {
                            #region Combined Group
                            if (null != combinedGroup.ContractGroupOfCombinedGroup)
                            {
                                foreach (SpanContractGroupRisk contractGroup in combinedGroup.ContractGroupOfCombinedGroup)
                                {
                                    #region Contract Group
                                    SpanContractGroupRisk contractGroupRisk = pEvaluationData.ContractGroupRisk.FirstOrDefault(c => c.ContractGroupParameters.SPANContractGroupId == contractGroup.ContractGroupParameters.SPANContractGroupId);
                                    if (contractGroupRisk != default)
                                    {
                                        contractGroupRisk.EvaluateSpread();
                                        if (contractGroupRisk.TierMonthDelta != default(Dictionary<int, SpanTierMonthRisk>))
                                        {
                                            // PM 20151125 [21595] Recalcule des deltas utilisés des Inter Tiers suite à utilisation des deltas dans les IntraCommodity Spreads
                                            SpanTierMonthRisk[] tierMonthToReEvaluate = contractGroupRisk.TierMonthDelta.Select(t => t.Value).Where(t => t.TierParameters.TierType == SPANSpreadType.InterTierType).ToArray();
                                            foreach (SpanTierMonthRisk tier in tierMonthToReEvaluate)
                                            {
                                                decimal deltaLongConsumed = tier.FutureMonthDelta.Sum(f => f.Value.Delta.DeltaLongConsumed);
                                                decimal deltaShortConsumed = tier.FutureMonthDelta.Sum(f => f.Value.Delta.DeltaShortConsumed);
                                                tier.Delta.Consume(deltaLongConsumed);
                                                tier.Delta.Consume(deltaShortConsumed);
                                            }
                                        }
                                    }
                                    #endregion Contract Group
                                }
                            }
                            #endregion Combined Group
                        }
                        // Calcul du crédit pour spread inter commodity (groupe de contrat)
                        exchangeComplex.InterCommoditySpread = EvaluateInterCommoditySpreadCredit(pEvaluationData, exchangeComplex.ExchangeComplexParameters.SPANExchangeComplexId);

                        // PM 20150930 [21134] Uniquement pour méthode C21
                        if (Type == InitialMarginMethodEnum.SPAN_C21)
                        {
                            // Liste des ContractGroup de l'exchangeComplex
                            List<SpanContractGroupRisk> contractGroupRisk = (
                                from combGrp in exchangeComplex.CombinedGroupOfExchange
                                from ctrGrp in combGrp.ContractGroupOfCombinedGroup
                                select ctrGrp).ToList();

                            // Calcul du crédit pour spread inter commodity selon le one-factor model
                            exchangeComplex.OneFactorCredit = EvaluateOneFactorInterCommoditySpreadCredit(pEvaluationData, contractGroupRisk);
                        }
                    }
                    #endregion Exchange Complex
                }
                // PM 20151224 [POC -MUREX] 
                if (Type == InitialMarginMethodEnum.SPAN_CME)
                {
                    // Calcul du crédit pour spread inter exchange
                    EvaluateInterExchangeSpreadCredit(pEvaluationData);
                }
            }
        }

        /// <summary>
        /// Alimentation des objets d'information sur le calcul avec les données de calcul de risque
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pMethodComObj">Objet racine des objets d'information sur le calcul</param>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private void FillMethodCom(SpanMarginCalcMethCom pMethodComObj)
        // PM 20190401 [24625][24387] Changement de paramètre pour pouvoir réaliser des appels multiples
        //private void FillMethodCom(SpanEvaluationData pEvaluationData, SpanMarginCalcMethCom pMethodComObj)
        private void FillMethodCom(SpanEvaluationData pEvaluationData, SpanExchangeComplexCom[] pExchangeComplex)
        {
            // Parcours de la hiérachie pour calcul et construction du détail des calculs
            //if (null != pMethodComObj.Parameters)
            if (default(SpanExchangeComplexCom[]) != pExchangeComplex)
            {
                SpanExchangeComplexRisk exchangeComplexRisk = null;
                SpanCombinedGroupRisk combinedGroupRisk = null;
                SpanContractGroupRisk contractGroupRisk = null;
                //foreach (SpanExchangeComplexCom exchangeComplex in pMethodComObj.Parameters)
                foreach (SpanExchangeComplexCom exchangeComplex in pExchangeComplex)
                {
                    #region Exchange Complex
                    if (null != exchangeComplex.Parameters)
                    {
                        exchangeComplexRisk = pEvaluationData.ExchangeComplexRisk.FirstOrDefault(e => e.ExchangeComplexParameters.SPANExchangeComplexId == exchangeComplex.SPANExchangeComplexId);
                        if (exchangeComplexRisk != default(SpanExchangeComplexRisk))
                        {
                            //PM 20150902 [21385] Add SettlementSession, DtBusinessTime, DtFile, FileIdentifier & FileFormat
                            if (exchangeComplexRisk.ExchangeComplexParameters != default(SpanExchangeComplex))
                            {
                                exchangeComplex.SettlementSession = exchangeComplexRisk.ExchangeComplexParameters.SettlementSession;
                                exchangeComplex.DtBusinessTime = exchangeComplexRisk.ExchangeComplexParameters.DtBusinessTime;
                                exchangeComplex.DtFile = exchangeComplexRisk.ExchangeComplexParameters.DtFile;
                                exchangeComplex.FileIdentifier = exchangeComplexRisk.ExchangeComplexParameters.FileIdentifier;
                                exchangeComplex.FileFormat = exchangeComplexRisk.ExchangeComplexParameters.FileFormat;
                            }
                            exchangeComplex.SuperInterCommoditySpread = exchangeComplexRisk.SuperInterCommoditySpread;
                            exchangeComplex.InterCommoditySpread = exchangeComplexRisk.InterCommoditySpread;
                            exchangeComplex.InterExchangeSpread = exchangeComplexRisk.InterExchangeSpread;
                            // PM 20150930 [21134] Add OneFactorCredit
                            exchangeComplex.OneFactorCredit = exchangeComplexRisk.OneFactorCredit;
                            // Construction des objets détaillant le calcul
                            foreach (SpanCombinedGroupCom combinedGroup in exchangeComplex.Parameters)
                            {
                                #region Combined Group
                                if (null != combinedGroup.Parameters)
                                {
                                    foreach (SpanContractGroupCom contractGroup in combinedGroup.Parameters)
                                    {
                                        #region Contract Group
                                        contractGroupRisk = pEvaluationData.ContractGroupRisk.FirstOrDefault(c => c.ContractGroupParameters.SPANContractGroupId == contractGroup.SPANContractGroupId);
                                        if (contractGroupRisk != default)
                                        {
                                            contractGroupRisk.FillComObj(contractGroup);
                                        }
                                        #endregion Contract Group
                                    }
                                    combinedGroupRisk = pEvaluationData.CombinedGroupRisk.FirstOrDefault(c => c.CombinedGroupParameters.SPANCombinedGroupId == combinedGroup.SPANCombinedGroupId);
                                    if (combinedGroupRisk != default(SpanCombinedGroupRisk))
                                    {
                                        combinedGroup.LongOptionValue = combinedGroupRisk.LongOptionValue;
                                        combinedGroup.ShortOptionValue = combinedGroupRisk.ShortOptionValue;
                                        combinedGroup.NetOptionValue = combinedGroupRisk.NetOptionValue;
                                        combinedGroup.RiskInitialAmount = combinedGroupRisk.RiskInitial;
                                        combinedGroup.RiskMaintenanceAmount = combinedGroupRisk.RiskMaintenance;
                                    }
                                }
                                #endregion Combined Group
                            }
                        }
                    }
                    #endregion Exchange Complex
                }
            }
        }

        #region InterCommoditySpreadCredit
        #region SuperInterCommoditySpreadCredit
        /// <summary>
        /// Calcul tous les super spread inter-commodity d'un Exchange Complex
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pSPANExchangeComplexId">Id de l'Exchange Complex</param>
        /// <returns>Tableau de super spread inter-commodity</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private SpanInterCommoditySpreadCom[] EvaluateSuperInterCommoditySpreadCredit(int pSPANExchangeComplexId)
        private SpanInterCommoditySpreadCom[] EvaluateSuperInterCommoditySpreadCredit(SpanEvaluationData pEvaluationData, int pSPANExchangeComplexId)
        {
            return SubEvaluateInterCommoditySpreadCredit(pEvaluationData, pSPANExchangeComplexId, true);
        }
        #endregion SuperInterCommoditySpreadCredit
        /// <summary>
        /// Calcul tous les spread inter-commodity d'un Exchange Complex
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pSPANExchangeComplexId">Id de l'Exchange Complex</param>
        /// <returns>Tableau de spread inter-commodity</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private SpanInterCommoditySpreadCom[] EvaluateInterCommoditySpreadCredit(int pSPANExchangeComplexId)
        private SpanInterCommoditySpreadCom[] EvaluateInterCommoditySpreadCredit(SpanEvaluationData pEvaluationData, int pSPANExchangeComplexId)
        {
            return SubEvaluateInterCommoditySpreadCredit(pEvaluationData, pSPANExchangeComplexId, false);
        }
        /// <summary>
        ///  Calcul tous les super ou classique spread inter-commodity d'un Exchange Complex
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pSPANExchangeComplexId">Id de l'Exchange Complex</param>
        /// <param name="pIsSuperSpread">True = calcul des super spread inter-commodity, sinon False</param>
        /// <returns>Tableau de spread inter-commodity</returns>
        /// PM 20151224 [POC -MUREX] Add contractGroupLeg (Method London_SPAN only)
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private SpanInterCommoditySpreadCom[] SubEvaluateInterCommoditySpreadCredit(int pSPANExchangeComplexId, bool pIsSuperSpread)
        private SpanInterCommoditySpreadCom[] SubEvaluateInterCommoditySpreadCredit(SpanEvaluationData pEvaluationData, int pSPANExchangeComplexId, bool pIsSuperSpread)
        {
            SpanInterCommoditySpreadCom[] allSpreadCom = null;

            // PM 20160404 [22116] Utilisation de MethodParameter.IsInterCommoditySpreading au lieu de m_ClearingOrganizationParameters.IsInterCommoditySpreading, m_ClearingOrganizationParameters n'est plus utilisé
            //if (IsNotOmnibus
                //&& (null != m_ClearingOrganizationParameters)
                //&& m_ClearingOrganizationParameters.IsInterCommoditySpreading
            if ((pEvaluationData != default)
                && pEvaluationData.IsNotOmnibus
                && MethodParameter.IsInterCommoditySpreading
                && (null != pEvaluationData.ContractGroupRisk)
                && (null != m_InterLegParameters)
                && (null != m_InterSpreadParameters))
            {
                // Recherche des Spreads dont au moins une jambe est un groupe de contrats en position
                IEnumerable<SpanInterSpread> interSpreadParam =
                    (from spreadParam in m_InterSpreadParameters
                     where (spreadParam.SPANExchangeComplexId == pSPANExchangeComplexId)
                        && ((spreadParam.SpreadGroupType == SPANSpreadType.SuperInterSpreadType) == pIsSuperSpread)
                     join interLegParam in m_InterLegParameters on spreadParam.SPANInterSpreadId equals interLegParam.SPANInterSpreadId
                     join contractGroup in pEvaluationData.ContractGroupRisk on interLegParam.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                     orderby spreadParam.SpreadPriority
                     select spreadParam).Distinct().OrderBy(s => s.SpreadPriority);

                // PM 20151224 [POC -MUREX] 
                if (Type == InitialMarginMethodEnum.London_SPAN)
                {
                    // Lecture des groupes de contrat en position sur chaque jambe
                    var contractGroupLeg =
                        from interSpread in interSpreadParam
                        join leg in m_InterLegParameters on interSpread.SPANInterSpreadId equals leg.SPANInterSpreadId
                        join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                        group leg by interSpread
                            into spreadLeg
                            select new { interSpreadParam = spreadLeg.Key, nbLeg = spreadLeg.Count() };

                    interSpreadParam =
                        from interSpread in contractGroupLeg
                        where interSpread.interSpreadParam.NumberOfLeg == interSpread.nbLeg
                        select interSpread.interSpreadParam;
                }

                List<SpanInterCommoditySpreadCom> listSpreadCom = new List<SpanInterCommoditySpreadCom>();
                foreach (SpanInterSpread interSpread in interSpreadParam)
                {
                    SpanInterCommoditySpreadCom spreadCom = null;
                    List<SpanInterCommoditySpreadCom> subListSpreadCom = null;

                    if (StrFunc.IsFilled(interSpread.InterSpreadMethod))
                    {
                        string interSpreadMethod = interSpread.InterSpreadMethod.Trim();
                        int spreadMethod = 0;
                        switch (interSpreadMethod)
                        {
                            case "D":
                                spreadMethod = 20;
                                break;
                            case "S":
                                spreadMethod = 4;
                                break;
                            default:
                                if (false == int.TryParse(interSpreadMethod, out spreadMethod))
                                {
                                    spreadMethod = 0;
                                }
                                break;
                        }
                        switch (spreadMethod)
                        {
                            case 1:
                                // Regular intercommodity spreading
                                spreadCom = EvaluateInterCommoditySpreadCreditMethod1(pEvaluationData, interSpread);
                                break;
                            case 2:
                                if (Type == InitialMarginMethodEnum.London_SPAN)
                                {
                                    // London intercommodity spreading
                                    spreadCom = EvaluateLondonInterCommoditySpreadCreditMethod2(pEvaluationData, interSpread);
                                }
                                else
                                {
                                    // Tiered intercommodity spreading
                                    subListSpreadCom = EvaluateInterCommoditySpreadCreditMethod2(pEvaluationData, interSpread);
                                }
                                break;
                            case 3:
                                // "Crush" Spread
                                subListSpreadCom = EvaluateInterCommoditySpreadCreditMethod3(pEvaluationData, interSpread);
                                break;
                            case 4:
                                // Scanning-based spreading
                                spreadCom = EvaluateInterCommoditySpreadCreditMethod4(pEvaluationData, pSPANExchangeComplexId, interSpread);
                                break;
                            case 10:
                                // London tiered intercommodity spreading
                                spreadCom = EvaluateLondonInterCommoditySpreadCreditMethod10(pEvaluationData, interSpread);
                                break;
                            case 20:
                                // Delta-based tier intercommodity spreading
                                spreadCom = EvaluateInterCommoditySpreadCreditMethod20(pEvaluationData, interSpread);
                                break;
                            default:
                                break;
                        }
                    }
                    if (null != spreadCom)
                    {
                        spreadCom.SpreadPriority = interSpread.SpreadPriority;
                        spreadCom.InterSpreadMethod = interSpread.InterSpreadMethod;
                        spreadCom.IsSeparatedSpreadRate = interSpread.IsCreditRateSeparated;
                        listSpreadCom.Add(spreadCom);
                    }
                    else if (null != subListSpreadCom)
                    {
                        subListSpreadCom.ForEach(s =>
                        {
                            s.SpreadPriority = interSpread.SpreadPriority;
                            s.InterSpreadMethod = interSpread.InterSpreadMethod;
                            s.IsSeparatedSpreadRate = interSpread.IsCreditRateSeparated;
                        });
                        listSpreadCom.AddRange(subListSpreadCom);
                    }
                }
                // Si des spreads ont été réalisés
                if (listSpreadCom.Count != 0)
                {
                    // Arrondir tous les InterCommoditySpreadCredit
                    foreach (SpanContractGroupRisk contractGroup in pEvaluationData.ContractGroupRisk)
                    {
                        contractGroup.InterCommoditySpreadCredit = SpanRoundAmount(contractGroup.InterCommoditySpreadCredit);
                    }

                    allSpreadCom = listSpreadCom.ToArray();
                }
            }
            return allSpreadCom;
        }

        /// <summary>
        /// Calcul du nombre de spreads réalisable en fonction des deltas des groupes de contrats 
        /// et selon la regular intercommodity spreading method
        /// pour le spread dont les caractèristiques sont fournis
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pInterSpreadParam">Caractèristiques de l'Intercommodity Spread</param>
        /// <returns>Nombre de spreads possible</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private decimal EvaluateNumberOfRegularInterCommoditySpread(SpanInterSpread pInterSpreadParam)
        private decimal EvaluateNumberOfRegularInterCommoditySpread(SpanEvaluationData pEvaluationData, SpanInterSpread pInterSpreadParam)
        {
            decimal numberOfSpread = 0;

            // PM 20151224 [POC -MUREX] Modification de la manière de faire pour savoir s'il manque des jambes en position
            //// Lecture des groupes de contrat en position sur chaque jambe
            //var contractGroupLeg =
            //    from leg in m_InterLegParameters
            //    where leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId
            //    join contractGroup in m_ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
            //    into legContractGroup
            //    from contractGroup in legContractGroup.DefaultIfEmpty()
            //    orderby leg.LegNumber
            //    select new { leg, contractGroup };

            //// Calcul du nombre de groupes de contrat manquant
            //int missingContractGroupLeg = contractGroupLeg.Count(cl => cl.contractGroup == default);

            // Jambes du spread avec des groupes de contrat en position
            var contractGroupLeg =
                from leg in m_InterLegParameters
                where leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId
                join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                select new { leg, contractGroup };

            // Nombre de jambes total du spread
            int nbInterLeg = m_InterLegParameters.Where(l => l.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId).Count();

            // Calcul du nombre de groupes de contrat manquant
            int missingContractGroupLeg = nbInterLeg - contractGroupLeg.Count();

            // S'il ne manque pas de jambe
            if (0 == missingContractGroupLeg)
            {
                int signeA = 0;         // Permet de savoir quel est le signe du side A (1 => positif; -1 => négatif)
                numberOfSpread = 0;

                // Parcourt de chaque jambe pour déterminer le nombre de spread pouvant être formés
                foreach (var contractLeg in contractGroupLeg)
                {
                    if ((contractLeg.contractGroup.DeltaNetRemaining == 0) || (contractLeg.leg.DeltaPerSpread <= 0))
                    {
                        numberOfSpread = 0;
                        break;
                    }
                    else
                    {
                        SpanInterLeg leg = contractLeg.leg;
                        decimal deltaNetRemainding = contractLeg.contractGroup.DeltaNetRemaining;
                        int signe = (int)(System.Math.Abs(deltaNetRemainding) / deltaNetRemainding);
                        if (signeA == 0)
                        {
                            // Initialisation du signe du side A
                            signeA = signe;
                            if (leg.LegSide.ToUpper() != "A")
                            {
                                signeA *= -1;
                            }
                            // Initialisation du nombre de Spread
                            numberOfSpread = SpanRoundSpreadNumber(System.Math.Abs(deltaNetRemainding) / leg.DeltaPerSpread);
                        }
                        else
                        {
                            // Vérification que le signe des Delta correspond au signe de la jambe
                            if (((signe == signeA) && (leg.LegSide.ToUpper() != "A"))
                                ||
                                ((signe != signeA) && (leg.LegSide.ToUpper() != "B")))
                            {
                                numberOfSpread = 0;
                                break;
                            }
                            // Calcul du nombre de spread possible
                            numberOfSpread = System.Math.Min(numberOfSpread, SpanRoundSpreadNumber(System.Math.Abs(deltaNetRemainding) / leg.DeltaPerSpread));
                        }
                    }
                }
            }
            return numberOfSpread;
        }
        /// <summary>
        /// Calcul du nombre de spreads réalisable en fonction des deltas de chaque tier des groupes de contrats
        /// et donc selon une tiered intercommodity spreading method
        /// pour le spread dont les caractèristiques sont fournis
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pInterSpreadParam">Caractèristiques de l'Intercommodity Spread</param>
        /// <returns>Nombre de spreads possible</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private decimal EvaluateLondonNumberOfTieredInterCommoditySpread(SpanInterSpread pInterSpreadParam)
        private decimal EvaluateLondonNumberOfTieredInterCommoditySpread(SpanEvaluationData pEvaluationData, SpanInterSpread pInterSpreadParam)
        {
            return EvaluateNumberOfTieredInterCommoditySpread(pEvaluationData, pInterSpreadParam, false);
        }
        /// <summary>
        /// Calcul du nombre de spreads réalisable en fonction des deltas de chaque tier des groupes de contrats
        /// et donc selon une tiered intercommodity spreading method
        /// pour le spread dont les caractèristiques sont fournis
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pInterSpreadParam">Caractèristiques de l'Intercommodity Spread</param>
        /// <param name="pIsWithGlobalTier">Autorise ou non un Tier global d'Id 0</param>
        /// <returns>Nombre de spreads possible</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private decimal EvaluateNumberOfTieredInterCommoditySpread(SpanInterSpread pInterSpreadParam, bool pIsWithGlobalTier)
        private decimal EvaluateNumberOfTieredInterCommoditySpread(SpanEvaluationData pEvaluationData, SpanInterSpread pInterSpreadParam, bool pIsWithGlobalTier)
        {
            decimal numberOfSpread = 0;

            // PM 20151224 [POC -MUREX] Modification de la manière de faire pour savoir s'il manque des jambes en position
            //// Lecture des groupes de contrat en position sur chaque jambe
            //var contractGroupLeg =
            //    from leg in m_InterLegParameters
            //    where leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId
            //    join contractGroup in m_ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
            //    into legContractGroup
            //    from contractGroup in legContractGroup.DefaultIfEmpty()
            //    orderby leg.LegNumber
            //    select new { leg, contractGroup };

            //// Calcul du nombre de groupes de contrat manquant
            //int missingContractGroupLeg = contractGroupLeg.Count(cl => cl.contractGroup == default);

            // Jambes du spread avec des groupes de contrat en position
            var contractGroupLeg =
                from leg in m_InterLegParameters
                where leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId
                join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                select new { leg, contractGroup };

            // Nombre de jambes total du spread
            int nbInterLeg = m_InterLegParameters.Where(l => l.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId).Count();

            // Calcul du nombre de groupes de contrat manquant
            int missingContractGroupLeg = nbInterLeg - contractGroupLeg.Count();

            // S'il ne manque pas de jambe
            if (0 == missingContractGroupLeg)
            {
                int signeA = 0;         // Permet de savoir quel est le signe du side A (1 => positif; -1 => négatif)
                numberOfSpread = 0;

                // Parcourt de chaque jambe pour déterminer le nombre de spread pouvant être formés
                foreach (var contractLeg in contractGroupLeg)
                {
                    // PM 20170120 [22768] Ne pas faire le test sur DeltaNetRemaining total car spread possible entre Tiers (ayant autant en négatif que positif)
                    //if ((contractLeg.contractGroup.DeltaNetRemaining == 0) || (contractLeg.leg.DeltaPerSpread <= 0))
                    if (contractLeg.leg.DeltaPerSpread <= 0)
                        {
                        numberOfSpread = 0;
                        break;
                    }
                    else
                    {
                        decimal deltaNetRemainding = 0;
                        if ((true == pIsWithGlobalTier) && (0 == contractLeg.leg.SPANTierId))
                        {
                            deltaNetRemainding = contractLeg.contractGroup.DeltaNetRemaining;
                        }
                        else
                        {
                            if (contractLeg.contractGroup.TierMonthDelta.ContainsKey(contractLeg.leg.SPANTierId))
                            {
                                SpanTierMonthRisk tier = contractLeg.contractGroup.TierMonthDelta[contractLeg.leg.SPANTierId];
                                if (null == tier)
                                {
                                    numberOfSpread = 0;
                                    break;
                                }
                                else
                                {
                                    deltaNetRemainding = tier.Delta.DeltaNetRemaining;
                                }
                            }
                            else
                            {
                                numberOfSpread = 0;
                                break;
                            }
                        }
                        if (deltaNetRemainding != 0)
                        {
                            SpanInterLeg leg = contractLeg.leg;
                            int signe = (int)(System.Math.Abs(deltaNetRemainding) / deltaNetRemainding);
                            if (signeA == 0)
                            {
                                // Initialisation du signe du side A
                                signeA = signe;
                                if (leg.LegSide.ToUpper() != "A")
                                {
                                    signeA *= -1;
                                }
                                // Initialisation du nombre de Spread
                                numberOfSpread = SpanRoundSpreadNumber(System.Math.Abs(deltaNetRemainding) / leg.DeltaPerSpread);
                            }
                            else
                            {
                                // Vérification que le signe des Delta correspond au signe de la jambe
                                if (((signe == signeA) && (leg.LegSide.ToUpper() != "A"))
                                    ||
                                    ((signe != signeA) && (leg.LegSide.ToUpper() != "B")))
                                {
                                    numberOfSpread = 0;
                                    break;
                                }
                                // Calcul du nombre de spread possible
                                numberOfSpread = System.Math.Min(numberOfSpread, SpanRoundSpreadNumber(System.Math.Abs(deltaNetRemainding) / leg.DeltaPerSpread));
                            }
                        }
                        else
                        {
                            numberOfSpread = 0;
                            break;
                        }
                    }
                }
            }
            return numberOfSpread;
        }
        /// <summary>
        /// Converti les échéances de  certains group de contrat dans le cas des "Crush" spreads (sur le CBOT)
        /// dans le cas où les échéances de tous les groupes de contrat du spread ne coïncide pas.
        /// (SoyBean Octobre ou Décembre converti en Novembre)
        /// </summary>
        /// <param name="pContractGroupCode">Code du groupe de contrat</param>
        /// <param name="pMaturity">Echéance à convertir</param>
        /// <returns>Echéance convertie</returns>
        private string CrushSpreadMaturity(string pContractGroupCode, string pMaturity)
        {
            string maturity = null;
            if ((null != pContractGroupCode) && (null != pMaturity))
            {
                // RegEx pour les échéances Octobre et Décembre
                Regex rxOctobreDecembre = new Regex(@"^\d{4}1(0|2)$", RegexOptions.Compiled);

                // Règle en dur pour les échéance Octobre, Novembre et Décembre des contrats Soybeans, Soybean Meal et soybean Oil.
                // Il y a du Soybean Novembre, mais pas de Soybean Meal et soybean Oil Novembre.
                // Il y a du Soybean Meal et soybean Oil Octobre et Décembre, mais pas de Soybean Octobre et Décembre
                // Le spread avec le Soybeans Novembre se fait avec du Soybean Meal et soybean Oil Octobre ou sinon Décembre.
                if (rxOctobreDecembre.IsMatch(pMaturity) && (pContractGroupCode.Trim().ToUpper() == SpanProductSoybean))
                {
                    // Transformer la recherche des échéances Octobre et Décembre Soybean en Novembre
                    maturity = pMaturity.Substring(0, 4) + "11";
                }
                else
                {
                    maturity = pMaturity;
                }
            }
            return maturity;
        }

        /// <summary>
        /// Regular intercommodity spreading
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pInterSpreadParam">Caractèristiques de l'Intercommodity Spread</param>
        /// <returns></returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private SpanInterCommoditySpreadCom EvaluateInterCommoditySpreadCreditMethod1(SpanInterSpread pInterSpreadParam)
        private SpanInterCommoditySpreadCom EvaluateInterCommoditySpreadCreditMethod1(SpanEvaluationData pEvaluationData, SpanInterSpread pInterSpreadParam)
        {
            SpanInterCommoditySpreadCom spreadCom = null;

            // Détermination du nombre de spread possible
            decimal numberOfSpread = EvaluateNumberOfRegularInterCommoditySpread(pEvaluationData, pInterSpreadParam);

            if (numberOfSpread > 0)
            {
                // Toutes les jambes sont présentes

                List<SpanInterCommoditySpreadLegCom> spreadLegCom = new List<SpanInterCommoditySpreadLegCom>();

                var contractGroupLeg =
                    from leg in m_InterLegParameters
                    where leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId
                    join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                    orderby leg.LegNumber
                    select new { leg, contractGroup };

                foreach (var legContract in contractGroupLeg)
                {
                    SpanInterCommoditySpreadLegCom legCom = new SpanInterCommoditySpreadLegCom();

                    SpanContractGroupRisk contractGroup = legContract.contractGroup;
                    legCom.ExchangeAcronym = legContract.leg.EchangeAcronym;
                    legCom.CombinedCommodityCode = contractGroup.ContractGroupParameters.ContractGroupCode;
                    legCom.WeightedRisk = contractGroup.WeightedFuturesPriceRisk;
                    legCom.DeltaAvailable = contractGroup.DeltaNetRemaining;
                    legCom.DeltaPerSpread = legContract.leg.DeltaPerSpread;
                    legCom.ComputedDeltaConsumed = legContract.leg.DeltaPerSpread * numberOfSpread;
                    legCom.SpreadCredit = legCom.ComputedDeltaConsumed;

                    if (pInterSpreadParam.IsCreditRateSeparated)
                    {
                        legCom.SpreadRate = legContract.leg.CreditRate;
                    }
                    else
                    {
                        legCom.SpreadRate = pInterSpreadParam.CreditRate;
                    }
                    legCom.SpreadCredit *= legCom.SpreadRate;

                    legCom.SpreadCredit = RoundAmount(legCom.WeightedRisk * legCom.SpreadCredit / 100, 8);
                    contractGroup.InterCommoditySpreadCredit += legCom.SpreadCredit;

                    // Consommer les deltas
                    if (contractGroup.DeltaNetRemaining < 0)
                    {
                        legCom.ComputedDeltaConsumed *= -1;
                    }
                    legCom.RealyDeltaConsumed = contractGroup.ConsumeDelta(legCom.ComputedDeltaConsumed);

                    legCom.DeltaRemaining = contractGroup.DeltaNetRemaining;
                    spreadLegCom.Add(legCom);
                }

                spreadCom = new SpanInterCommoditySpreadCom
                {
                    SpreadRate = pInterSpreadParam.CreditRate,
                    NumberOfSpreadLimit = numberOfSpread,
                    NumberOfSpread = numberOfSpread,
                    LegParameters = spreadLegCom.ToArray()
                };
            }
            return spreadCom;
        }
        /// <summary>
        /// "Tiered intercommodity spreading
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pInterSpreadParam">Caractèristiques de l'Intercommodity Spread</param>
        /// <returns></returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private List<SpanInterCommoditySpreadCom> EvaluateInterCommoditySpreadCreditMethod2(SpanInterSpread pInterSpreadParam)
        private List<SpanInterCommoditySpreadCom> EvaluateInterCommoditySpreadCreditMethod2(SpanEvaluationData pEvaluationData, SpanInterSpread pInterSpreadParam)
        {
            // "Tiered" Spread
            List<SpanInterCommoditySpreadCom> spreadComList = new List<SpanInterCommoditySpreadCom>();
            decimal maxNumberOfSpread = EvaluateNumberOfRegularInterCommoditySpread(pEvaluationData, pInterSpreadParam);

            if (maxNumberOfSpread > 0)
            {
                // On sait qu'il ne manque pas de jambe et qu'au plus on peux faire maxNumberOfSpread spread
                decimal totalNumberOfSpread = 0;

                // Recherche des paramètres des jambes du spread
                var legParam =
                    from leg in m_InterLegParameters
                    where (leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId)
                    select leg;

                // Recherche des groupes de contrat en position sur chaque jambe
                var contractGroupLeg =
                    from leg in legParam
                    join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                    orderby leg.LegNumber
                    select new { leg, contractGroup };

                // Recherche des différents tiers des groupes de contrat de chaque jambe
                var tierNumbers = (
                    from contractLeg in contractGroupLeg
                    from tierMonthDelta in contractLeg.contractGroup.TierMonthDelta
                    where (tierMonthDelta.Value.TierParameters.TierType == SPANSpreadType.InterTierType)
                    select tierMonthDelta.Value.TierParameters.TierNumber
                    )
                    .Distinct().OrderBy(tn => tn);

                // Parcours des tiers
                foreach (int tierNo in tierNumbers)
                {
                    decimal numberOfSpread = 0;
                    int signeA = 0;         // Permet de savoir quel est le signe du side A (1 => positif; -1 => négatif)
                    #region Boucle de parcours des jambes pour calculer le nombre de spread possible
                    // Parcours des jambes
                    foreach (var leg in legParam)
                    {
                        // Recherche des Delta du tier
                        SpanTierMonthRisk tierMonthRisk = (
                            from cgl in contractGroupLeg
                            where (cgl.leg == leg) && (cgl.contractGroup != default)
                            from cfm in cgl.contractGroup.TierMonthDelta
                            where (cfm.Value.TierParameters.TierNumber == tierNo) && (cfm.Value.TierParameters.TierType == SPANSpreadType.InterTierType)
                            select cfm).FirstOrDefault().Value;

                        if ((tierMonthRisk != default) && (tierMonthRisk.Delta.DeltaNetRemaining != 0) && (leg.DeltaPerSpread != 0))
                        {
                            int signe = (int)(System.Math.Abs(tierMonthRisk.Delta.DeltaNetRemaining) / tierMonthRisk.Delta.DeltaNetRemaining);
                            if (signeA == 0)
                            {
                                // Initialisation du signe du side A
                                signeA = signe;
                                if (leg.LegSide.ToUpper() != "A")
                                {
                                    signeA *= -1;
                                }
                                // Initialisation du nombre de spread possible
                                numberOfSpread = System.Math.Abs(tierMonthRisk.Delta.DeltaNetRemaining) / leg.DeltaPerSpread;
                            }
                            else
                            {
                                // Vérification que le signe des Delta correspond au signe de la jambe
                                if (((signe == signeA) && (leg.LegSide.ToUpper() != "A"))
                                    ||
                                    ((signe != signeA) && (leg.LegSide.ToUpper() != "B")))
                                {
                                    numberOfSpread = 0;
                                    break;
                                }
                                // Calcul du nombre de spread possible
                                numberOfSpread = System.Math.Min(numberOfSpread, System.Math.Abs(tierMonthRisk.Delta.DeltaNetRemaining) / leg.DeltaPerSpread);
                            }
                        }
                        else if (leg.IsRequired)
                        {
                            numberOfSpread = 0;
                            break;
                        }
                    }
                    #endregion

                    // Arrondi du nombre de spread
                    // Attention Arrondi particulier : SpanRoundDelta et non SpanRoundSpreadNumber
                    numberOfSpread = SpanRoundDelta(numberOfSpread);

                    // Ne pas dépasser le nombre max de spread
                    if (totalNumberOfSpread + numberOfSpread > maxNumberOfSpread)
                    {
                        numberOfSpread = maxNumberOfSpread - totalNumberOfSpread;
                    }
                    //Calcul du spreadcredit si numberOfSpread > 0
                    if (numberOfSpread > 0)
                    {
                        SpanInterCommoditySpreadCom spreadCom = new SpanInterCommoditySpreadCom();
                        List<SpanInterCommoditySpreadLegCom> spreadLegCom = new List<SpanInterCommoditySpreadLegCom>();
                        spreadCom.NumberOfSpreadLimit = maxNumberOfSpread;

                        foreach (var legContract in contractGroupLeg)
                        {
                            SpanInterCommoditySpreadLegCom legCom = new SpanInterCommoditySpreadLegCom();

                            SpanContractGroupRisk contractGroup = legContract.contractGroup;
                            legCom.ExchangeAcronym = legContract.leg.EchangeAcronym;
                            legCom.CombinedCommodityCode = contractGroup.ContractGroupParameters.ContractGroupCode;
                            legCom.WeightedRisk = contractGroup.WeightedFuturesPriceRisk;
                            legCom.TierNumber = tierNo;
                            legCom.DeltaAvailable = contractGroup.DeltaNetRemaining;
                            legCom.DeltaPerSpread = legContract.leg.DeltaPerSpread;
                            legCom.ComputedDeltaConsumed = legContract.leg.DeltaPerSpread * numberOfSpread;
                            legCom.SpreadCredit = legCom.ComputedDeltaConsumed;

                            if (pInterSpreadParam.IsCreditRateSeparated)
                            {
                                legCom.SpreadRate = legContract.leg.CreditRate;
                            }
                            else
                            {
                                legCom.SpreadRate = pInterSpreadParam.CreditRate;
                            }
                            legCom.SpreadCredit *= legCom.SpreadRate;
                            legCom.SpreadCredit = RoundAmount(legCom.WeightedRisk * legCom.SpreadCredit / 100, 8);

                            contractGroup.InterCommoditySpreadCredit += legCom.SpreadCredit;

                            // Consommer les deltas
                            if (contractGroup.DeltaNetRemaining < 0)
                            {
                                legCom.ComputedDeltaConsumed *= -1;
                            }

                            SpanTierMonthRisk tierMonthRisk = contractGroup.TierMonthDelta.Values.FirstOrDefault(t => (t.TierParameters.TierType == SPANSpreadType.InterTierType) && (t.TierParameters.TierNumber == tierNo));
                            if (tierMonthRisk != default)
                            {
                                legCom.RealyDeltaConsumed = contractGroup.ConsumeDelta(tierMonthRisk.TierParameters.SPANTierId, legCom.ComputedDeltaConsumed);
                            }

                            legCom.DeltaRemaining = contractGroup.DeltaNetRemaining;
                            spreadLegCom.Add(legCom);
                        }
                        // Nombre de spreads cumulés déjà réalisés
                        totalNumberOfSpread += numberOfSpread;

                        // Ajout du spread à la liste de spreads
                        spreadCom.SpreadRate = pInterSpreadParam.CreditRate;
                        spreadCom.NumberOfSpread = numberOfSpread;
                        spreadCom.LegParameters = spreadLegCom.ToArray();
                        spreadComList.Add(spreadCom);

                        // Vérification du nombre maximum de spreads
                        if (totalNumberOfSpread >= maxNumberOfSpread)
                        {
                            // Nombre de spread maximum atteind
                            break;
                        }
                    }
                }
            }
            return spreadComList;
        }
        /// <summary>
        /// "Crush" intercommodity spreading
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pInterSpreadParam">Caractèristiques de l'Intercommodity Spread</param>
        /// <returns></returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private List<SpanInterCommoditySpreadCom> EvaluateInterCommoditySpreadCreditMethod3(SpanInterSpread pInterSpreadParam)
        private List<SpanInterCommoditySpreadCom> EvaluateInterCommoditySpreadCreditMethod3(SpanEvaluationData pEvaluationData, SpanInterSpread pInterSpreadParam)
        {
            // "Crush" Spread
            List<SpanInterCommoditySpreadCom> spreadComList = new List<SpanInterCommoditySpreadCom>();
            decimal maxNumberOfSpread = EvaluateNumberOfRegularInterCommoditySpread(pEvaluationData, pInterSpreadParam);

            if (maxNumberOfSpread > 0)
            {
                // On sait qu'il ne manque pas de jambe et qu'au plus on peux faire maxNumberOfSpread spread
                decimal totalNumberOfSpread = 0;

                // Recherche des paramètres des jambes du spread
                var legParam =
                    from leg in m_InterLegParameters
                    where leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId
                    select leg;

                // Recherche des groupes de contrat en position sur chaque jambe
                var contractGroupLeg =
                    from leg in legParam
                    join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                    orderby leg.LegNumber
                    select new { leg, contractGroup };

                // Recherche des différentes échéances des groupes de contrat de chaque jambe
                var futureMaturities = (
                    from contractLeg in contractGroupLeg
                    from futureMonthDelta in contractLeg.contractGroup.FutureMonthDelta
                    select futureMonthDelta.Key
                    )
                    .Distinct().OrderBy(fm => fm);

                // Parcours des échéances
                foreach (string futureMaturity in futureMaturities)
                {
                    decimal numberOfSpread = 0;
                    int signeA = 0;         // Permet de savoir quel est le signe du side A (1 => positif; -1 => négatif)
                    #region Boucle de parcours des jambes pour calculer le nombre de spread possible
                    // Parcours des jambes
                    foreach (var leg in legParam)
                    {
                        // Règle en dur pour les échéance Octobre, Novembre et Décembre des contrats Soybeans, Soybean Meal et Soybean Oil.
                        // Il y a du Soybean Novembre, mais pas de Soybean Meal et Soybean Oil Novembre.
                        // Il y a du Soybean Meal et soybean Oil Octobre et Décembre, mais pas de Soybean Octobre et Décembre
                        // Le spread avec le Soybeans Novembre se fait avec du Soybean Meal et Soybean Oil Octobre ou sinon Décembre.
                        string maturity = CrushSpreadMaturity(leg.ContractGroupCode, futureMaturity);

                        // Recherche des Delta de l'échéance future
                        SpanFutureMonthRisk futMonthRisk = (
                            from cgl in contractGroupLeg
                            where (cgl.leg == leg) && (cgl.contractGroup != default)
                            from cfm in cgl.contractGroup.FutureMonthDelta
                            where cfm.Key == maturity
                            select cfm).FirstOrDefault().Value;

                        if ((futMonthRisk != default(SpanFutureMonthRisk)) && (futMonthRisk.Delta.DeltaNetRemaining != 0) && (leg.DeltaPerSpread != 0))
                        {
                            int signe = (int)(System.Math.Abs(futMonthRisk.Delta.DeltaNetRemaining) / futMonthRisk.Delta.DeltaNetRemaining);
                            if (signeA == 0)
                            {
                                // Initialisation du signe du side A
                                signeA = signe;
                                if (leg.LegSide.ToUpper() != "A")
                                {
                                    signeA *= -1;
                                }
                                // Initialisation du nombre de spread possible
                                numberOfSpread = System.Math.Abs(futMonthRisk.Delta.DeltaNetRemaining) / leg.DeltaPerSpread;
                            }
                            else
                            {
                                // Vérification que le signe des Delta correspond au signe de la jambe
                                if (((signe == signeA) && (leg.LegSide.ToUpper() != "A"))
                                    ||
                                    ((signe != signeA) && (leg.LegSide.ToUpper() != "B")))
                                {
                                    numberOfSpread = 0;
                                    break;
                                }
                                // Calcul du nombre de spread possible
                                numberOfSpread = System.Math.Min(numberOfSpread, System.Math.Abs(futMonthRisk.Delta.DeltaNetRemaining) / leg.DeltaPerSpread);
                            }
                        }
                        else if (leg.IsRequired)
                        {
                            numberOfSpread = 0;
                            break;
                        }
                    }
                    #endregion

                    // Arrondi du nombre de spread
                    // Attention Arrondi particulier : SpanRoundDelta et non SpanRoundSpreadNumber
                    numberOfSpread = SpanRoundDelta(numberOfSpread);

                    // Ne pas dépasser le nombre max de spread
                    if (totalNumberOfSpread + numberOfSpread > maxNumberOfSpread)
                    {
                        numberOfSpread = maxNumberOfSpread - totalNumberOfSpread;
                    }
                    //Calcul du spreadcredit si numberOfSpread > 0
                    if (numberOfSpread > 0)
                    {
                        SpanInterCommoditySpreadCom spreadCom = new SpanInterCommoditySpreadCom();
                        List<SpanInterCommoditySpreadLegCom> spreadLegCom = new List<SpanInterCommoditySpreadLegCom>();
                        spreadCom.NumberOfSpreadLimit = maxNumberOfSpread;

                        foreach (var legContract in contractGroupLeg)
                        {
                            SpanInterCommoditySpreadLegCom legCom = new SpanInterCommoditySpreadLegCom();

                            SpanContractGroupRisk contractGroup = legContract.contractGroup;
                            legCom.ExchangeAcronym = legContract.leg.EchangeAcronym;
                            legCom.CombinedCommodityCode = contractGroup.ContractGroupParameters.ContractGroupCode;
                            legCom.WeightedRisk = contractGroup.WeightedFuturesPriceRisk;
                            legCom.Maturity = CrushSpreadMaturity(legCom.CombinedCommodityCode, futureMaturity);
                            legCom.DeltaAvailable = contractGroup.DeltaNetRemaining;
                            legCom.DeltaPerSpread = legContract.leg.DeltaPerSpread;
                            legCom.ComputedDeltaConsumed = legContract.leg.DeltaPerSpread * numberOfSpread;
                            legCom.SpreadCredit = legCom.ComputedDeltaConsumed;

                            if (pInterSpreadParam.IsCreditRateSeparated)
                            {
                                legCom.SpreadRate = legContract.leg.CreditRate;
                            }
                            else
                            {
                                legCom.SpreadRate = pInterSpreadParam.CreditRate;
                            }
                            legCom.SpreadCredit *= legCom.SpreadRate;
                            legCom.SpreadCredit = RoundAmount(legCom.WeightedRisk * legCom.SpreadCredit / 100, 8);

                            contractGroup.InterCommoditySpreadCredit += legCom.SpreadCredit;

                            // Consommer les deltas
                            if (contractGroup.DeltaNetRemaining < 0)
                            {
                                legCom.ComputedDeltaConsumed *= -1;
                            }
                            legCom.RealyDeltaConsumed = contractGroup.ConsumeDelta(legCom.Maturity, legCom.ComputedDeltaConsumed);

                            legCom.DeltaRemaining = contractGroup.DeltaNetRemaining;
                            spreadLegCom.Add(legCom);
                        }
                        // Nombre de spreads cumulés déjà réalisés
                        totalNumberOfSpread += numberOfSpread;

                        // Ajout du spread à la liste de spreads
                        spreadCom.SpreadRate = pInterSpreadParam.CreditRate;
                        spreadCom.NumberOfSpread = numberOfSpread;
                        spreadCom.LegParameters = spreadLegCom.ToArray();
                        spreadComList.Add(spreadCom);

                        // Vérification du nombre maximum de spreads
                        if (totalNumberOfSpread >= maxNumberOfSpread)
                        {
                            // Nombre de spread maximum atteind
                            break;
                        }
                    }
                }
            }
            return spreadComList;
        }
        /// <summary>
        /// Scanning-based spreading
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pSPANExchangeComplexId">Identifiant interne de l'Exchange Complex</param>
        /// <param name="pInterSpreadParam">Caractèristiques de l'Intercommodity Spread</param>
        /// <returns></returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private SpanInterCommoditySpreadCom EvaluateInterCommoditySpreadCreditMethod4(long pSPANExchangeComplexId, SpanInterSpread pInterSpreadParam)
        private SpanInterCommoditySpreadCom EvaluateInterCommoditySpreadCreditMethod4(SpanEvaluationData pEvaluationData, long pSPANExchangeComplexId, SpanInterSpread pInterSpreadParam)
        {
            SpanInterCommoditySpreadCom spreadCom = null;

            // Lecture des jambes du spread en associant les groupes de contrat en position sur chaque jambe
            IEnumerable<Pair<SpanInterLeg, SpanContractGroupRisk>> contractGroupLeg =
                from leg in m_InterLegParameters
                where leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId
                join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                into legContractGroup
                from contractGroup in legContractGroup.DefaultIfEmpty(default)
                orderby leg.LegNumber
                select new Pair<SpanInterLeg, SpanContractGroupRisk>(leg, contractGroup);

            #region Test Contract Group Minimum
            // Le spread possède-t-il au moins le nombre jambe minimum en position
            if (contractGroupLeg.Count(l => l.Second != default) >= pInterSpreadParam.MinimumNumberOfLeg)
            {
                // Calcul du nombre de jambe sur des groupes de contrat requis manquant ou pas en position
                int missingContractGroupLeg = contractGroupLeg.Count(cl => (cl.First.IsRequired && ((cl.Second == default) || (cl.Second.DeltaNetRemaining == 0))));

                #region Test Jambe requise manquante
                // Il ne manque pas de jambe requise
                if (0 == missingContractGroupLeg)
                {
                    // Vérifier qu'il y a une jambe cible
                    if (contractGroupLeg.Count(cl => cl.First.IsTargetLeg == true) == 1)
                    {
                        // Recherche de la jambe cible
                        Pair<SpanInterLeg, SpanContractGroupRisk> targetContractLeg = contractGroupLeg.First(cl => cl.First.IsTargetLeg == true);

                        // Si la jambe cible n'a pas de position : en construire une en ajoutant le groupe de contrat cible
                        if (targetContractLeg.Second == default)
                        {
                            // Lecture des paramètres SPAN du groupe de contrat cible
                            //PM 20150707 [21104] Utilisation de SPANExchangeComplexId à la place de SPANExchangeId pour les SpanContractGroup
                            //PM 20150707 [21104] Utilisation de m_InterSpreadParameters à la place de m_ExchangeParameters pour accéder à SPANExchangeComplexId
                            //SpanContractGroup contractGroupParam = (from contractGroup in m_ContractGroupParameters
                            //                                        join exchange in m_ExchangeParameters on contractGroup.SPANExchangeId equals exchange.SPANExchangeId
                            //                                        where (exchange.SPANExchangeComplexId == pSPANExchangeComplexId)
                            //                                        && (contractGroup.ContractGroupCode == targetContractLeg.First.ContractGroupCode)
                            //                                        && (exchange.ExchangeAcronym == targetContractLeg.First.EchangeAcronym)
                            //                                        select contractGroup).FirstOrDefault();
                            SpanContractGroup contractGroupParam = (from contractGroup in m_ContractGroupParameters
                                                                    join interSpread in m_InterSpreadParameters on contractGroup.SPANExchangeComplexId equals interSpread.SPANExchangeComplexId
                                                                    where (contractGroup.SPANExchangeComplexId == pSPANExchangeComplexId)
                                                                       && (contractGroup.ContractGroupCode == targetContractLeg.First.ContractGroupCode)
                                                                       && (interSpread.SPANInterSpreadId == targetContractLeg.First.SPANInterSpreadId)
                                                                    select contractGroup).FirstOrDefault();

                            if (contractGroupParam != default(SpanContractGroup))
                            {
                                // PM 20170929 [23472] Construction des élements de risque de tous les contrats du groupe de contrats lorsqu'il y a en aucun en position
                                List<SpanContractRisk> emptyContractOfContractGroup = (
                                    from contractParam in m_ContractParameters
                                    where contractParam.SPANContractGroupId == contractGroupParam.SPANContractGroupId
                                    select new SpanContractRisk(this, pEvaluationData, contractParam)
                                    ).ToList();

                                // PM 20170929 [23472] Utilisation des élements de risque vide sur les contrats
                                //SpanContractGroupRisk newContractGroup = new SpanContractGroupRisk(this, contractGroupParam, default(List<SpanContractRisk>));
                                SpanContractGroupRisk newContractGroup = new SpanContractGroupRisk(this, pEvaluationData, contractGroupParam, emptyContractOfContractGroup);

                                // PM 20170929 [23472] Ajout des éléments de risque de la première échéance des contrats du groupe de contrats lorsqu'il y a en aucune en position
                                if ((newContractGroup.FutureMonthDelta != default(Dictionary<string, SpanFutureMonthRisk>)) && (newContractGroup.FutureMonthDelta.Count == 0))
                                {
                                    SpanMaturity firstMaturity = (
                                        from contract in emptyContractOfContractGroup
                                        join maturityParam in m_MaturityParameters on contract.ContractParameters.SPANContractId equals maturityParam.SPANContractId
                                        select maturityParam
                                        ).OrderBy(m => m.FutureMaturity).FirstOrDefault();
                                     
                                    if (firstMaturity != default(SpanMaturity))
                                    {
                                        SpanContract firstContract = emptyContractOfContractGroup.Where(c => c.ContractParameters.SPANContractId == firstMaturity.SPANContractId).First().ContractParameters;

                                        SpanFutureMonthRisk futureMonth =
                                            new SpanFutureMonthRisk
                                                (this,
                                                pEvaluationData,
                                                contractGroupParam,
                                                firstMaturity.FutureMaturity, firstMaturity,
                                                0, 0,
                                                firstMaturity.FuturePriceScanRange / (firstMaturity.DeltaScalingFactor * firstContract.DeltaScalingFactor) * contractGroupParam.RiskMultiplier,
                                                new List<SpanAssetRiskValues>());

                                        newContractGroup.FutureMonthDelta.Add(futureMonth.Maturity, futureMonth);
                                    }
                                }

                                // Ajout du nouveau groupe de contrat
                                pEvaluationData.ContractGroupRisk.Add(newContractGroup);

                                targetContractLeg.Second = newContractGroup;
                            }
                            else
                            {
                                // Prendre comme jambe cible la première ayant des positions
                                // Attention: On ne devrait jamais arriver ici si les paramètres SPAN sont présent
                                targetContractLeg = contractGroupLeg.FirstOrDefault(cl => cl.Second != default);
                            }
                        }

                        // Construction du groupe de contrat résultant du spread
                        if ((targetContractLeg != null) && (targetContractLeg.Second != default))
                        {
                            spreadCom = new SpanInterCommoditySpreadCom();

                            // Cumul des contrats sur le contrat cible :
                            // Delta = Delta / Contrat Delta Ratio * Target Contrat Delta Ratio
                            // Scan = Sum( scan < 0 scan * Rate Value : scan )
                            IEnumerable<Pair<SpanInterLeg, SpanContractGroupRisk>> contractGroupLegUsed =
                                from leg in contractGroupLeg
                                where (leg.Second != default)
                                select leg;

                            IEnumerable<SpanInterCommoditySpreadLegCom> spreadLegCom =
                                from contractLeg in contractGroupLegUsed
                                select new SpanInterCommoditySpreadLegCom
                                {
                                    CombinedCommodityCode = contractLeg.First.ContractGroupCode,
                                    SpreadRate = pInterSpreadParam.IsCreditRateSeparated ? contractLeg.First.CreditRate : pInterSpreadParam.CreditRate,
                                    DeltaPerSpread = contractLeg.First.DeltaPerSpread,
                                    IsRequired = contractLeg.First.IsRequired,
                                    IsTarget = contractLeg.First.IsTargetLeg,
                                    ContractGroup = contractLeg.Second.FillComObj(new SpanContractGroupCom()),
                                    DeltaAvailable = contractLeg.Second.DeltaNetRemaining,
                                };

                            // Création du nouveau Contract Groupe
                            // PM 20170929 [23472] Ajout paramètre pSpanMethod
                            //SpanContractGroupRisk targetCGR = new SpanContractGroupRisk(pInterSpreadParam, targetContractLeg, contractGroupLegUsed);
                            SpanContractGroupRisk targetCGR = new SpanContractGroupRisk(this, pEvaluationData, pInterSpreadParam, targetContractLeg, contractGroupLegUsed);
                            //
                            spreadCom.SpreadScanRisk = targetCGR.ScanRisk;
                            spreadCom.SpreadRate = pInterSpreadParam.CreditRate;
                            spreadCom.DeltaAvailable = targetCGR.DeltaNetRemaining;
                            spreadCom.IsSeparatedSpreadRate = pInterSpreadParam.IsCreditRateSeparated;
                            spreadCom.IsOffsetChargeMethod = false;
                            spreadCom.LegParameters = spreadLegCom.ToArray();
                            //
                            // Ajout du Contract Groupe à ceux existant
                            IEnumerable<SpanContractGroupRisk> contractGroupUsed =
                                from contractLeg in contractGroupLegUsed
                                select contractLeg.Second;
                            // - ajout aux groupes combinées
                            foreach (SpanContractGroupRisk cg in contractGroupUsed)
                            {
                                SpanCombinedGroupRisk combinedGroup = pEvaluationData.CombinedGroupRisk.Find(g => g.CombinedGroupParameters.SPANCombinedGroupId == cg.ContractGroupParameters.SPANCombinedGroupId);
                                // EG 20130703 Test combinedGroup not null
                                if (combinedGroup != default(SpanCombinedGroupRisk))
                                {
                                    combinedGroup.ContractGroupOfCombinedGroup.Remove(cg);
                                    pEvaluationData.ContractGroupRisk.Remove(cg);
                                }
                            }
                            // - ajout aux groupes de contrat
                            pEvaluationData.ContractGroupRisk.Add(targetCGR);

                            // Ajout du nouveau groupe de contrat à la liste des groupes combinées si besoin
                            SpanCombinedGroupRisk combinedGroupRisk = pEvaluationData.CombinedGroupRisk.Find(g => g.CombinedGroupParameters.SPANCombinedGroupId == targetCGR.ContractGroupParameters.SPANCombinedGroupId);
                            if (combinedGroupRisk != default(SpanCombinedGroupRisk))
                            {
                                combinedGroupRisk.ContractGroupOfCombinedGroup.Add(targetCGR);
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion
            return spreadCom;
        }
        /// <summary>
        /// London tiered intercommodity spreading
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pInterSpreadParam">Caractèristiques de l'Intercommodity Spread</param>
        /// <returns></returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private SpanInterCommoditySpreadCom EvaluateLondonInterCommoditySpreadCreditMethod10(SpanInterSpread pInterSpreadParam)
        private SpanInterCommoditySpreadCom EvaluateLondonInterCommoditySpreadCreditMethod10(SpanEvaluationData pEvaluationData, SpanInterSpread pInterSpreadParam)
        {
            // London intercommodity spreading
            SpanInterCommoditySpreadCom spreadCom = null;

            // Détermination du nombre de spread possible
            decimal numberOfSpread = EvaluateLondonNumberOfTieredInterCommoditySpread(pEvaluationData, pInterSpreadParam);

            if (numberOfSpread > 0)
            {
                // Toutes les jambes sont présentes

                List<SpanInterCommoditySpreadLegCom> spreadLegCom = new List<SpanInterCommoditySpreadLegCom>();

                var contractGroupLeg =
                    from leg in m_InterLegParameters
                    where leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId
                    join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                    orderby leg.LegNumber
                    select new { leg, contractGroup };

                foreach (var legContract in contractGroupLeg)
                {
                    SpanInterCommoditySpreadLegCom legCom = new SpanInterCommoditySpreadLegCom();

                    SpanContractGroupRisk contractGroup = legContract.contractGroup;
                    SpanInterLeg interLeg = legContract.leg;
                    SpanTierMonthRisk tier = contractGroup.TierMonthDelta[interLeg.SPANTierId];

                    legCom.ExchangeAcronym = interLeg.EchangeAcronym;
                    legCom.CombinedCommodityCode = contractGroup.ContractGroupParameters.ContractGroupCode;
                    legCom.WeightedRisk = contractGroup.WeightedFuturesPriceRisk;
                    legCom.DeltaAvailable = tier.Delta.DeltaNetRemaining;
                    legCom.DeltaPerSpread = interLeg.DeltaPerSpread;
                    legCom.TierNumber = tier.TierParameters.TierNumber;
                    legCom.ComputedDeltaConsumed = interLeg.DeltaPerSpread * numberOfSpread;
                    legCom.SpreadCredit = legCom.ComputedDeltaConsumed;

                    if (pInterSpreadParam.IsCreditRateSeparated)
                    {
                        legCom.SpreadRate = interLeg.CreditRate;
                    }
                    else
                    {
                        legCom.SpreadRate = pInterSpreadParam.CreditRate;
                    }
                    legCom.SpreadCredit *= legCom.SpreadRate;

                    legCom.SpreadCredit = RoundAmount(legCom.WeightedRisk * legCom.SpreadCredit / 100, 0);
                    contractGroup.InterCommoditySpreadCredit += legCom.SpreadCredit;

                    // Consommer les deltas
                    if (tier.Delta.DeltaNetRemaining < 0)
                    {
                        legCom.ComputedDeltaConsumed *= -1;
                    }
                    legCom.RealyDeltaConsumed = contractGroup.ConsumeDelta(interLeg.SPANTierId, legCom.ComputedDeltaConsumed);

                    legCom.DeltaRemaining = tier.Delta.DeltaNetRemaining;
                    spreadLegCom.Add(legCom);
                }

                spreadCom = new SpanInterCommoditySpreadCom
                {
                    SpreadRate = pInterSpreadParam.CreditRate,
                    NumberOfSpreadLimit = numberOfSpread,
                    NumberOfSpread = numberOfSpread,
                    LegParameters = spreadLegCom.ToArray()
                };
            }
            return spreadCom;
        }
        /// <summary>
        /// Delta-based tier intercommodity spreading
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pInterSpreadParam">Caractèristiques de l'Intercommodity Spread</param>
        /// <returns></returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private SpanInterCommoditySpreadCom EvaluateInterCommoditySpreadCreditMethod20(SpanInterSpread pInterSpreadParam)
        private SpanInterCommoditySpreadCom EvaluateInterCommoditySpreadCreditMethod20(SpanEvaluationData pEvaluationData, SpanInterSpread pInterSpreadParam)
        {
            SpanInterCommoditySpreadCom spreadCom = null;

            // Détermination du nombre de spread possible
            decimal numberOfSpread = EvaluateNumberOfTieredInterCommoditySpread(pEvaluationData, pInterSpreadParam, true);

            if (numberOfSpread > 0)
            {
                // Toutes les jambes sont présentes

                List<SpanInterCommoditySpreadLegCom> spreadLegCom = new List<SpanInterCommoditySpreadLegCom>();

                var contractGroupLeg =
                    from leg in m_InterLegParameters
                    where leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId
                    join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                    orderby leg.LegNumber
                    select new { leg, contractGroup };

                foreach (var legContract in contractGroupLeg)
                {
                    SpanTierMonthRisk tier = default;
                    SpanInterCommoditySpreadLegCom legCom = new SpanInterCommoditySpreadLegCom();
                    SpanContractGroupRisk contractGroup = legContract.contractGroup;
                    SpanInterLeg interLeg = legContract.leg;
                    if (0 == interLeg.SPANTierId)
                    {
                        legCom.DeltaAvailable = contractGroup.DeltaNetRemaining;
                        legCom.TierNumber = 0;
                        legCom.WeightedRisk = contractGroup.WeightedFuturesPriceRisk;
                    }
                    else
                    {
                        tier = contractGroup.TierMonthDelta[interLeg.SPANTierId];
                        legCom.DeltaAvailable = tier.Delta.DeltaNetRemaining;
                        legCom.TierNumber = tier.TierParameters.TierNumber;
                        legCom.WeightedRisk = tier.WeightedFuturesPriceRisk;
                    }

                    legCom.ExchangeAcronym = interLeg.EchangeAcronym;
                    legCom.CombinedCommodityCode = contractGroup.ContractGroupParameters.ContractGroupCode;
                    legCom.DeltaPerSpread = interLeg.DeltaPerSpread;
                    legCom.ComputedDeltaConsumed = interLeg.DeltaPerSpread * numberOfSpread;
                    legCom.SpreadCredit = legCom.ComputedDeltaConsumed;

                    if (pInterSpreadParam.IsCreditRateSeparated)
                    {
                        legCom.SpreadRate = interLeg.CreditRate;
                    }
                    else
                    {
                        legCom.SpreadRate = pInterSpreadParam.CreditRate;
                    }
                    // PM 20180829 [XXXXX] Différence d'arrondie du Spread Credit
                    //legCom.SpreadCredit *= legCom.SpreadRate;
                    //legCom.SpreadCredit = RoundAmount(legCom.WeightedRisk * legCom.SpreadCredit / 100, 0);
                    decimal roudedWeightedRiskRated = SpanRoundWeightedFuturePriceRisk(legCom.WeightedRisk * legCom.SpreadRate / 100);
                    legCom.SpreadCredit = RoundAmount(roudedWeightedRiskRated * legCom.SpreadCredit, 0);

                    contractGroup.InterCommoditySpreadCredit += legCom.SpreadCredit;

                    // Consommer les deltas
                    // PM 20150902 [21385] Correction du log pour DeltaRemaining
                    //if (0 == interLeg.SPANTierId)
                    //{
                    //    if (contractGroup.DeltaNetRemaining < 0)
                    //    {
                    //        legCom.ComputedDeltaConsumed *= -1;
                    //    }
                    //    legCom.DeltaRemaining = contractGroup.DeltaNetRemaining;
                    //}
                    //else
                    //{
                    //    if (tier.Delta.DeltaNetRemaining < 0)
                    //    {
                    //        legCom.ComputedDeltaConsumed *= -1;
                    //    }
                    //    legCom.DeltaRemaining = tier.Delta.DeltaNetRemaining;
                    //}
                    SpanDelta delta = (0 == interLeg.SPANTierId) ? contractGroup.Delta : tier.Delta;
                    if (delta.DeltaNetRemaining < 0)
                    {
                        legCom.ComputedDeltaConsumed *= -1;
                    }

                    legCom.RealyDeltaConsumed = contractGroup.ConsumeDelta(interLeg.SPANTierId, legCom.ComputedDeltaConsumed);
                    legCom.DeltaRemaining = delta.DeltaNetRemaining;

                    spreadLegCom.Add(legCom);
                }
                spreadCom = new SpanInterCommoditySpreadCom
                {
                    SpreadRate = pInterSpreadParam.CreditRate,
                    NumberOfSpreadLimit = numberOfSpread,
                    NumberOfSpread = numberOfSpread,
                    LegParameters = spreadLegCom.ToArray()
                };
            }
            return spreadCom;
        }
        /// <summary>
        /// London intercommodity spreading method 2
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pInterSpreadParam">Caractèristiques de l'Intercommodity Spread</param>
        /// <returns></returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private SpanInterCommoditySpreadCom EvaluateLondonInterCommoditySpreadCreditMethod2(SpanInterSpread pInterSpreadParam)
        private SpanInterCommoditySpreadCom EvaluateLondonInterCommoditySpreadCreditMethod2(SpanEvaluationData pEvaluationData, SpanInterSpread pInterSpreadParam)
        {
            // London intercommodity spreading
            SpanInterCommoditySpreadCom spreadCom = null;

            // Détermination du nombre de spread possible
            decimal numberOfSpread = EvaluateLondonNumberOfTieredInterCommoditySpread(pEvaluationData, pInterSpreadParam);

            if (numberOfSpread > 0)
            {
                // Toutes les jambes sont présentes
                List<SpanInterCommoditySpreadLegCom> spreadLegCom = new List<SpanInterCommoditySpreadLegCom>();

                var contractGroupLeg =
                    from leg in m_InterLegParameters
                    where leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId
                    join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                    orderby leg.LegNumber
                    select new { leg, contractGroup };

                spreadCom = new SpanInterCommoditySpreadCom
                {
                    IsOffsetChargeMethod = true,
                    OffsetCharge = numberOfSpread * pInterSpreadParam.OffsetRate
                };

                // PM 20131112 [19167][19144] Calcul du PortfolioScanRisk
                //spreadCom.PortfolioScanRisk = m_ContractGroupRisk.Sum(c => c.ScanRisk);
                // Ensemble des groupes de contrats du spread
                var contractGroupOfGroupLeg =
                    from contract in contractGroupLeg
                    select contract.contractGroup;
                // Matrice de risque des groupes de contrats du spread
                Dictionary<int, decimal> riskValue = new Dictionary<int, decimal>();
                foreach (var contractGroup in contractGroupOfGroupLeg)
                {
                    // Cumul des matrices
                    SPANMethod.AddRiskValues(riskValue, contractGroup.RiskValue);
                }
                spreadCom.PortfolioScanRisk = riskValue.Max(a => a.Value);
                //
                spreadCom.PortfolioRisk = spreadCom.OffsetCharge + spreadCom.PortfolioScanRisk;
                spreadCom.SpreadScanRisk = contractGroupLeg.Sum(cl => cl.contractGroup.ScanRisk);

                foreach (var legContract in contractGroupLeg)
                {
                    SpanInterCommoditySpreadLegCom legCom = new SpanInterCommoditySpreadLegCom();

                    SpanContractGroupRisk contractGroup = legContract.contractGroup;
                    SpanInterLeg interLeg = legContract.leg;
                    SpanTierMonthRisk tier = contractGroup.TierMonthDelta[interLeg.SPANTierId];

                    legCom.ExchangeAcronym = interLeg.EchangeAcronym;
                    legCom.CombinedCommodityCode = contractGroup.ContractGroupParameters.ContractGroupCode;
                    legCom.WeightedRisk = contractGroup.WeightedFuturesPriceRisk;
                    legCom.DeltaAvailable = tier.Delta.DeltaNetRemaining;
                    legCom.DeltaPerSpread = interLeg.DeltaPerSpread;
                    legCom.TierNumber = tier.TierParameters.TierNumber;
                    legCom.ComputedDeltaConsumed = interLeg.DeltaPerSpread * numberOfSpread;
                    legCom.SpreadRate = (decimal)pInterSpreadParam.OffsetRate;

                    legCom.SpreadCredit = (spreadCom.SpreadScanRisk - spreadCom.PortfolioRisk) / spreadCom.SpreadScanRisk;
                    // PM 20131112 [19167][19144] Arrondi
                    legCom.SpreadCredit = RoundAmount(legCom.SpreadCredit, DeltaValuePrecision);

                    legCom.SpreadCredit = System.Math.Max(0, RoundAmount(contractGroup.ScanRisk * legCom.SpreadCredit, 0));
                    contractGroup.InterCommoditySpreadCredit += legCom.SpreadCredit;

                    // Consommer les deltas
                    if (tier.Delta.DeltaNetRemaining < 0)
                    {
                        legCom.ComputedDeltaConsumed *= -1;
                    }
                    legCom.RealyDeltaConsumed = contractGroup.ConsumeDelta(interLeg.SPANTierId, legCom.ComputedDeltaConsumed);

                    legCom.DeltaRemaining = tier.Delta.DeltaNetRemaining;
                    spreadLegCom.Add(legCom);
                }

                spreadCom.SpreadRate = (decimal)pInterSpreadParam.OffsetRate;
                spreadCom.NumberOfSpreadLimit = numberOfSpread;
                spreadCom.NumberOfSpread = numberOfSpread;
                spreadCom.LegParameters = spreadLegCom.ToArray();
            }
            return spreadCom;
        }
        #endregion InterCommoditySpreadCredit
        #region Inter Exchange Spread Credit
        /// <summary>
        /// Calcul du nombre de spreads réalisable en fonction des deltas des groupes de contrats 
        /// pour le spread inter exchange dont les caractèristiques sont fournis en paramètre
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pInterSpreadParam">Caractèristiques de l'InterExchange Spread</param>
        /// <returns>Nombre de spreads possible</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private decimal EvaluateNumberOfInterExchangeSpread(SpanInterSpread pInterSpreadParam)
        private decimal EvaluateNumberOfInterExchangeSpread(SpanEvaluationData pEvaluationData, SpanInterSpread pInterSpreadParam)
        {
            decimal numberOfSpread = 0;

            // Lecture des groupes de contrat en position sur chaque jambe
            // Attention: il faut faire la jointure par rapport à ExchangeAcronym et ContractGroupCode
            // car SPANContractGroupId n'est pas renseigné pour les jambes qui font partie d'un autre Exchange Complex
            //PM 20150707 [21104] Utilisation de SPANExchangeComplexId à la place de SPANExchangeId pour les SpanContractGroup
            //PM 20150707 [21104] Utilisation de m_InterSpreadParameters à la place de m_ExchangeParameters pour accéder à SPANExchangeComplexId
            //PM 20150902 [21385] Séparation de la recherche des groupes de contrat en position sur chaque jambe selon qu'il s'agisse d'une jambe Home ou Away
            //var contractGroupLeg =
            //    from leg in m_InterLegParameters
            //    where leg.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId
            //    join exchange in m_ExchangeParameters on leg.EchangeAcronym equals exchange.ExchangeAcronym
            //    join contractGroup in m_ContractGroupRisk on new { leg.ContractGroupCode, exchange.SPANExchangeId } equals new { contractGroup.ContractGroupParameters.ContractGroupCode, contractGroup.ContractGroupParameters.SPANExchangeId }
            //    into legContractGroup
            //    from contractGroup in legContractGroup.DefaultIfEmpty()
            //    orderby leg.LegNumber
            //    select new { leg, contractGroup };

            //Recherche des groupes de contrat par rapport à l'Id du groupe de contrat lorsqu'il est connu sur la jambe
            var contractGroupLegHome =
                from leg in m_InterLegParameters.Where(l => l.SPANInterSpreadId == pInterSpreadParam.SPANInterSpreadId)
                join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                into legContractGroup
                from contractGroup in legContractGroup.DefaultIfEmpty(default)
                orderby leg.LegNumber
                select new { leg, contractGroup };

            //Jambes dont le groupe de contrat n'a pas été trouvé via l'Id
            var contractGroupLegNotFound = contractGroupLegHome.Where(cl => cl.contractGroup == default);

            //Pour les groupes de contrat dont l'Id n'est pas connu sur la jambe,
            //recherche des groupes de contrat par rapport à le ContractGroupCode et l'ExchangeAcronym (pour jointure via le SPANExchangeComplexId)
            var contractGroupLegAway =
                 from legNotFound in contractGroupLegNotFound
                 let leg = legNotFound.leg
                 join exchange in m_ExchangeParameters on leg.EchangeAcronym equals exchange.ExchangeAcronym
                 join contractGroup in pEvaluationData.ContractGroupRisk on new { leg.ContractGroupCode, exchange.SPANExchangeComplexId } equals new { contractGroup.ContractGroupParameters.ContractGroupCode, contractGroup.ContractGroupParameters.SPANExchangeComplexId }
                 into legContractGroup
                 from contractGroup in legContractGroup.DefaultIfEmpty(default)
                 orderby leg.LegNumber
                 select new { leg, contractGroup };

            // Calcul du nombre de groupes de contrat manquant
            //PM 20150902 [21385] Uniquement sur contractGroupLegAway 
            int missingContractGroupLeg = contractGroupLegAway.Count(cl => cl.contractGroup == default);

            // S'il ne manque pas de jambe
            if (0 == missingContractGroupLeg)
            {
                var contractGroupLeg = contractGroupLegHome.Where(cl => cl.contractGroup != default).Concat(contractGroupLegAway);

                int signeA = 0;         // Permet de savoir quel est le signe du side A (1 => positif; -1 => négatif)
                numberOfSpread = 0;

                // Parcourt de chaque jambe pour déterminer le nombre de spread pouvant être formés
                foreach (var contractLeg in contractGroupLeg)
                {
                    if ((contractLeg.contractGroup.DeltaNetRemaining == 0) || (contractLeg.leg.DeltaPerSpread <= 0))
                    {
                        numberOfSpread = 0;
                        break;
                    }
                    else
                    {
                        SpanInterLeg leg = contractLeg.leg;
                        decimal deltaNetRemainding = contractLeg.contractGroup.DeltaNetRemaining;
                        int signe = (int)(System.Math.Abs(deltaNetRemainding) / deltaNetRemainding);
                        if (signeA == 0)
                        {
                            // Initialisation du signe du side A
                            signeA = signe;
                            if (leg.LegSide.ToUpper() != "A")
                            {
                                signeA *= -1;
                            }
                            // Initialisation du nombre de Spread
                            numberOfSpread = SpanRoundSpreadNumber(System.Math.Abs(deltaNetRemainding) / leg.DeltaPerSpread);
                        }
                        else
                        {
                            // Vérification que le signe des Delta correspond au signe de la jambe
                            if (((signe == signeA) && (leg.LegSide.ToUpper() != "A"))
                                ||
                                ((signe != signeA) && (leg.LegSide.ToUpper() != "B")))
                            {
                                numberOfSpread = 0;
                                break;
                            }
                            // Calcul du nombre de spread possible
                            numberOfSpread = System.Math.Min(numberOfSpread, SpanRoundSpreadNumber(System.Math.Abs(deltaNetRemainding) / leg.DeltaPerSpread));
                        }
                    }
                }
            }
            return numberOfSpread;
        }

        /// <summary>
        /// Calcul des spreads inter exchange
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <returns></returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private Dictionary<int, IEnumerable<SpanInterCommoditySpreadCom>> EvaluateInterExchangeSpreadCredit()
        private Dictionary<int, IEnumerable<SpanInterCommoditySpreadCom>> EvaluateInterExchangeSpreadCredit(SpanEvaluationData pEvaluationData)
        //IEnumerable<SpanExchangeComplexCom> pSPANExchangeComplex)
        {
            Dictionary<int, IEnumerable<SpanInterCommoditySpreadCom>> allIntexSpreadByExchangeCom = null;

            // PM 20160404 [22116] Utilisation de MethodParameter.IsInterCommoditySpreading au lieu de m_ClearingOrganizationParameters.IsInterCommoditySpreading, m_ClearingOrganizationParameters n'est plus utilisé
            //if (IsNotOmnibus
                //&& (null != m_ClearingOrganizationParameters)
                //&& m_ClearingOrganizationParameters.IsInterCommoditySpreading
            if ((pEvaluationData != default)
                && pEvaluationData.IsNotOmnibus 
                && MethodParameter.IsInterCommoditySpreading
                && (null != pEvaluationData.ContractGroupRisk)
                && (null != m_InterLegParameters)
                && (null != m_InterSpreadParameters))
            {
                List<KeyValuePair<int, SpanInterCommoditySpreadCom>> intexSpreadComWithExchangeId = new List<KeyValuePair<int, SpanInterCommoditySpreadCom>>();
                //PM 20140123 [19527] Sauvegarde de l'ensemble des spreads possible
                List<SpanInterSpread[]> possibleIntexSpread = new List<SpanInterSpread[]>();

                // Prendre les spreads des marchés pour lesquels il existe des positions sur au moins une jambes
                //PM 20150707 [21104] Utilisation de SPANExchangeComplexId à la place de SPANExchangeId pour les SpanContractGroup
                //IEnumerable<SpanInterSpread> intexSpreadParamPos =
                //    from intexSpread in m_InterSpreadParameters
                //    join exchange in m_ExchangeParameters on intexSpread.SPANExchangeComplexId equals exchange.SPANExchangeComplexId
                //    join contractGroup in m_ContractGroupRisk on exchange.SPANExchangeId equals contractGroup.ContractGroupParameters.SPANExchangeId
                //    select intexSpread;
                IEnumerable<SpanInterSpread> intexSpreadParamPos = (
                    from intexSpread in m_InterSpreadParameters
                    join interLegParam in m_InterLegParameters on intexSpread.SPANInterSpreadId equals interLegParam.SPANInterSpreadId
                    join contractGroup in pEvaluationData.ContractGroupRisk on interLegParam.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                    where intexSpread.SPANExchangeComplexId == contractGroup.ContractGroupParameters.SPANExchangeComplexId
                    select intexSpread).Distinct();

                // Recherche des Spreads dont au moins une jambe est un groupe de contrats en position
                // et dont au moins une jambe n'est pas sur le même Exchange Complex
                //PM 20150707 [21104] Utilisation de SPANExchangeComplexId à la place de SPANExchangeId pour les SpanContractGroup
                //IEnumerable<SpanInterSpread> intexSpreadParam =
                //    (from spreadParam in intexSpreadParamPos
                //     join interLegParam in m_InterLegParameters on spreadParam.SPANInterSpreadId equals interLegParam.SPANInterSpreadId
                //     join exchange in m_ExchangeParameters on interLegParam.EchangeAcronym equals exchange.ExchangeAcronym
                //     join contractGroup in m_ContractGroupRisk on interLegParam.ContractGroupCode equals contractGroup.ContractGroupParameters.ContractGroupCode
                //     where (exchange.SPANExchangeComplexId != spreadParam.SPANExchangeComplexId)
                //        && (exchange.SPANExchangeId == contractGroup.ContractGroupParameters.SPANExchangeId)
                //     //PM 20140123 [19527] L'orderby est ici inutile
                //     //orderby spreadParam.SpreadPriority
                //     select spreadParam).Distinct().OrderBy(s => s.SpreadPriority);
                IEnumerable<SpanInterSpread> intexSpreadParam =
                    (from spreadParam in intexSpreadParamPos
                     join interLegParam in m_InterLegParameters on spreadParam.SPANInterSpreadId equals interLegParam.SPANInterSpreadId
                     join exchange in m_ExchangeParameters on interLegParam.EchangeAcronym equals exchange.ExchangeAcronym
                     join contractGroup in pEvaluationData.ContractGroupRisk on interLegParam.ContractGroupCode equals contractGroup.ContractGroupParameters.ContractGroupCode
                     where (contractGroup.ContractGroupParameters.SPANExchangeComplexId != spreadParam.SPANExchangeComplexId)
                        && (exchange.SPANExchangeComplexId == contractGroup.ContractGroupParameters.SPANExchangeComplexId)
                     select spreadParam).Distinct().OrderBy(s => s.SpreadPriority);

                foreach (SpanInterSpread interSpread in intexSpreadParam)
                {
                    // Calcul du nombre de spread réalisable
                    decimal numberOfSpread = EvaluateNumberOfInterExchangeSpread(pEvaluationData, interSpread);

                    if (0 < numberOfSpread)
                    {
                        //PM 20140123 [19527] Le calcul est déplacé plus bas
                        //// Toutes les jambes sont présentes => calcul des montants de spread

                        //// Liste des jambes calculées
                        //List<SpanInterCommoditySpreadLegCom> spreadLegCom = new List<SpanInterCommoditySpreadLegCom>();

                        // Rechercher un spread identique sur un autre Exchange Complex
                        SpanInterSpread interSpreadOtherExchange = default;

                        // Prendre les spreads des autres Exchange Complex
                        IEnumerable<SpanInterSpread> otherSpreadParam =
                            from spread in intexSpreadParam
                            where (spread.SPANExchangeComplexId != interSpread.SPANExchangeComplexId)
                            select spread;

                        foreach (SpanInterSpread otherSpread in otherSpreadParam)
                        {
                            IEnumerable<SpanInterLeg> interSpreadLegs = m_InterLegParameters.Where(s => s.SPANInterSpreadId == interSpread.SPANInterSpreadId);
                            IEnumerable<SpanInterLeg> otherSpreadLegs = m_InterLegParameters.Where(s => s.SPANInterSpreadId == otherSpread.SPANInterSpreadId);
                            // Les 2 spreads ont-ils le même nombre de jambes
                            if (interSpreadLegs.Count() == otherSpreadLegs.Count())
                            {
                                bool isFirstLeg = true;
                                bool isSameSigne = true;
                                interSpreadOtherExchange = otherSpread;
                                foreach (SpanInterLeg interLeg in interSpreadLegs)
                                {
                                    // Existe-t-il une jambe de même contrat et même delta par spread
                                    //PM 20150902 [21385] Ne plus tester EchangeAcronym qui peut ne pas être renseigné
                                    //SpanInterLeg otherLeg = otherSpreadLegs.FirstOrDefault(s => (s.EchangeAcronym == interLeg.EchangeAcronym)
                                    //    && (s.ContractGroupCode == interLeg.ContractGroupCode)
                                    //    && (s.DeltaPerSpread == interLeg.DeltaPerSpread));
                                    SpanInterLeg otherLeg = otherSpreadLegs.FirstOrDefault(
                                        s => (s.ContractGroupCode == interLeg.ContractGroupCode)
                                          && (s.DeltaPerSpread == interLeg.DeltaPerSpread));

                                    if (otherLeg != default(SpanInterLeg))
                                    {
                                        if (isFirstLeg)
                                        {
                                            // A la première jambe regarder si les 2 spreads sont dans le même sens ou de sens inverse
                                            isSameSigne = (interLeg.LegSide == otherLeg.LegSide);
                                            isFirstLeg = false;
                                        }
                                        else
                                        {
                                            // Vérifier le sens des autres jambes
                                            if (isSameSigne != (interLeg.LegSide == otherLeg.LegSide))
                                            {
                                                // Passer au spread suivant
                                                interSpreadOtherExchange = default;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Passer au spread suivant
                                        interSpreadOtherExchange = default;
                                        break;
                                    }
                                }
                                if (interSpreadOtherExchange != default)
                                {
                                    // Spread sur autre Exchange trouvé
                                    break;
                                }
                            }
                        }

                        SpanInterSpread[] interSpreadArray;
                        if (interSpreadOtherExchange != default)
                        {
                            interSpreadArray = new SpanInterSpread[2] { interSpread, interSpreadOtherExchange };
                        }
                        else
                        {
                            interSpreadArray = new SpanInterSpread[1] { interSpread };
                        }
                        //PM 20140123 [19527] Sauvegarde de l'ensemble des spreads possible
                        possibleIntexSpread.Add(interSpreadArray);
                        //PM 20140123 [19527] Ne plus construire les spreads immédiatement
                    }
                }
                //PM 20140123 [19527] Si des spreads peuvent être formés
                if (possibleIntexSpread.Count > 0)
                {
                    //PM 20140123 [19527] Recherche de la plus petite priorité par ExchangeComplex
                    var spreadOrderer =
                        from spread in intexSpreadParam
                        group spread by spread.SPANExchangeComplexId into spreadByExchangeComplex
                        select new
                        {
                            SPANExchangeComplexId = spreadByExchangeComplex.Key,
                            MinSpreadPriority = spreadByExchangeComplex.Min(s => s.SpreadPriority)
                        };

                    //PM 20140123 [19527] Tri des spreads selon la priorité en fonction des ExchangeComplex
                    List<KeyValuePair<int, SpanInterSpread>> intexSpreadParamPriority = (
                        from spread in intexSpreadParam
                        join priority in spreadOrderer on spread.SPANExchangeComplexId equals priority.SPANExchangeComplexId
                        orderby (spread.SpreadPriority - priority.MinSpreadPriority)
                        select new KeyValuePair<int, SpanInterSpread>((spread.SpreadPriority - priority.MinSpreadPriority), spread)
                        ).ToList();

                    //PM 20140123 [19527] Report du tri sur les spreads pouvant être formés
                    var intexSpreadOnTwoExchange =
                        from spread in possibleIntexSpread
                        where spread.Count() == 2
                        join spreadPri0 in intexSpreadParamPriority on spread[0] equals spreadPri0.Value
                        join spreadPri1 in intexSpreadParamPriority on spread[1] equals spreadPri1.Value
                        orderby (spreadPri0.Key + spreadPri1.Key)
                        select spread;

                    var intexSpreadOnOneExchange =
                        from spread in possibleIntexSpread
                        where spread.Count() == 1
                        join spreadPri0 in intexSpreadParamPriority on spread[0] equals spreadPri0.Value
                        orderby spreadPri0.Key
                        select spread;

                    possibleIntexSpread = (intexSpreadOnTwoExchange.Concat(intexSpreadOnOneExchange)).ToList();

                    //PM 20140123 [19527] Calcul des spreads
                    foreach (SpanInterSpread[] interSpreadArray in possibleIntexSpread)
                    {
                        // Calcul du nombre de spread réalisable
                        decimal numberOfSpread = EvaluateNumberOfInterExchangeSpread(pEvaluationData, interSpreadArray[0]);

                        if (numberOfSpread > 0)
                        {
                            //PM 20140123 [19527] Le calcul est déplacé ici
                            // Toutes les jambes sont présentes => calcul des montants de spread

                            // Liste des jambes calculées
                            List<SpanInterCommoditySpreadLegCom> spreadLegCom = new List<SpanInterCommoditySpreadLegCom>();

                            // Prendre toutes les jambes en position du ou des 2 spreads réalisables
                            var contractGroupLeg =
                                from leg in m_InterLegParameters
                                join exchangeSpread in interSpreadArray on leg.SPANInterSpreadId equals exchangeSpread.SPANInterSpreadId
                                join contractGroup in pEvaluationData.ContractGroupRisk on leg.SPANContractGroupId equals contractGroup.ContractGroupParameters.SPANContractGroupId
                                orderby leg.LegNumber
                                select new { leg, contractGroup, exchangeSpread };

                            // Calcul du montant de credit pour chaque jambe
                            foreach (var legContract in contractGroupLeg)
                            {
                                SpanInterCommoditySpreadLegCom legCom = new SpanInterCommoditySpreadLegCom();

                                SpanContractGroupRisk contractGroup = legContract.contractGroup;
                                legCom.ExchangeAcronym = legContract.leg.EchangeAcronym;
                                legCom.CombinedCommodityCode = contractGroup.ContractGroupParameters.ContractGroupCode;
                                legCom.WeightedRisk = contractGroup.WeightedFuturesPriceRisk;
                                legCom.DeltaAvailable = contractGroup.DeltaNetRemaining;
                                legCom.DeltaPerSpread = legContract.leg.DeltaPerSpread;
                                legCom.ComputedDeltaConsumed = legContract.leg.DeltaPerSpread * numberOfSpread;
                                legCom.SpreadCredit = legCom.ComputedDeltaConsumed;

                                if (legContract.exchangeSpread.IsCreditRateSeparated)
                                {
                                    legCom.SpreadRate = legContract.leg.CreditRate;
                                }
                                else
                                {
                                    legCom.SpreadRate = legContract.exchangeSpread.CreditRate;
                                }
                                legCom.SpreadCredit *= legCom.SpreadRate;

                                legCom.SpreadCredit = RoundAmount(legCom.WeightedRisk * legCom.SpreadCredit / 100, 8);
                                contractGroup.InterExchangeSpreadCredit += legCom.SpreadCredit;

                                // Consommer les deltas
                                if (contractGroup.DeltaNetRemaining < 0)
                                {
                                    legCom.ComputedDeltaConsumed *= -1;
                                }
                                legCom.RealyDeltaConsumed = contractGroup.ConsumeDelta(legCom.ComputedDeltaConsumed);

                                legCom.DeltaRemaining = contractGroup.DeltaNetRemaining;

                                // Ajout de la jambe courante aux jambes du spread
                                spreadLegCom.Add(legCom);
                            }

                            IEnumerable<KeyValuePair<int, SpanInterCommoditySpreadCom>> exchSpreadCom =
                                from exchangeSpread in interSpreadArray
                                select new KeyValuePair<int, SpanInterCommoditySpreadCom>
                                    (
                                        exchangeSpread.SPANExchangeComplexId,
                                        new SpanInterCommoditySpreadCom
                                        {
                                            SpreadPriority = exchangeSpread.SpreadPriority,
                                            InterSpreadMethod = exchangeSpread.InterSpreadMethod,
                                            IsSeparatedSpreadRate = exchangeSpread.IsCreditRateSeparated,
                                            SpreadRate = exchangeSpread.CreditRate,
                                            NumberOfSpreadLimit = numberOfSpread,
                                            NumberOfSpread = numberOfSpread,
                                            LegParameters = spreadLegCom.ToArray(),
                                        }
                                    );

                            intexSpreadComWithExchangeId.AddRange(exchSpreadCom);
                        }
                    }
                }
                if (intexSpreadComWithExchangeId.Count > 0)
                {
                    allIntexSpreadByExchangeCom = (
                        from spread in intexSpreadComWithExchangeId
                        group spread by spread.Key
                            into spreadByExchange
                            select new
                            {
                                spreadByExchange.Key,
                                Value = from spread in spreadByExchange select spread.Value,
                            }).ToDictionary(e => e.Key, e => e.Value);

                    // Répartition des Spread Inter Exchange sur les différents Exchange Complex
                    foreach (int exchangeComplexId in allIntexSpreadByExchangeCom.Keys)
                    {
                        SpanExchangeComplexRisk exchangeComplex = pEvaluationData.ExchangeComplexRisk.FirstOrDefault(e => e.ExchangeComplexParameters.SPANExchangeComplexId == exchangeComplexId);
                        if (default(SpanExchangeComplexRisk) != exchangeComplex)
                        {
                            exchangeComplex.InterExchangeSpread = allIntexSpreadByExchangeCom[exchangeComplexId].ToArray();
                        }
                    }

                }
            }
            return allIntexSpreadByExchangeCom;
        }
        #endregion Inter Exchange Spread Credit

        #region OneFactorInterCommoditySpreadCredit
        /// <summary>
        /// Calcul du Spread Credit par le One-Factor Model
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pContractGroupRisk">Ensemble des SpanContractGroupRisk pour lesquels calculer le One-Factor Spread Credit</param>
        /// PM 20150930 [21134] New
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private SpanOneFactorCreditCom EvaluateOneFactorInterCommoditySpreadCredit(List<SpanContractGroupRisk> pContractGroupRisk)
        private SpanOneFactorCreditCom EvaluateOneFactorInterCommoditySpreadCredit(SpanEvaluationData pEvaluationData, List<SpanContractGroupRisk> pContractGroupRisk)
        {
            SpanOneFactorCreditCom oneFactorSpread = default;
            //
            // PM 20160404 [22116] m_ClearingOrganizationParameters n'est plus utilisé
            //if (IsNotOmnibus
                //&& (null != m_ClearingOrganizationParameters)
            if ((pEvaluationData != default)
                && pEvaluationData.IsNotOmnibus 
                && (null != pContractGroupRisk))
            {
                // Prendre la liste des CombinedCommodity (ContractGroup) pour lesquels il faut utiliser la méthode OneFactor (utilisation des paramètres Lambda)
                List<SpanContractGroupRisk> lambdaContractGroupRisk = pContractGroupRisk.Where(c => c.ContractGroupParameters.IsUseLambda).ToList();

                if (lambdaContractGroupRisk.Count() > 0)
                {
                    oneFactorSpread = new SpanOneFactorCreditCom
                    {
                        OffsetMax = pEvaluationData.ScanRiskOffsetCapPrct
                    };
                    //
                    // Step 1 : Calcul du General Risk Min & Max de chaque Combined Commodity
                    // Construction des matrices de risque des combined commodity (contract group)
                    IEnumerable<SpanMatrice> riskValueComponentAllCC =
                        from contractGroup in lambdaContractGroupRisk
                        select new SpanMatrice(contractGroup.RiskValue);
                    // Application du lambda max correlation parameter aux matrices
                    IEnumerable<SpanMatrice> generalRiskComponentLMax =
                        from contractGroup in lambdaContractGroupRisk
                        select new SpanMatrice(contractGroup.RiskValue).ApplyFactor(contractGroup.ContractGroupParameters.LambdaMax);
                    // Application du lambda min correlation parameter aux matrices
                    IEnumerable<SpanMatrice> generalRiskComponentLMin =
                        from contractGroup in lambdaContractGroupRisk
                        select new SpanMatrice(contractGroup.RiskValue).ApplyFactor(contractGroup.ContractGroupParameters.LambdaMin);

                    // Step 2,3,4 : Calcul du General Risk et Final General Risk Min & Max Global
                    oneFactorSpread.FinalGeneralRiskLMax = FinalGeneralRisk(generalRiskComponentLMax);
                    oneFactorSpread.FinalGeneralRiskLMin = FinalGeneralRisk(generalRiskComponentLMin);

                    // Step 5,6,7 : Calcul du residual iodiosyncratic risk
                    double idiosyncraticRiskLMax = 0;
                    double idiosyncraticRiskLMin = 0;
                    foreach (SpanContractGroupRisk contractGroup in lambdaContractGroupRisk)
                    {
                        // Step 5 : Prendre le pire scénario du combined commodity
                        // Utilisation du ScanRisk en tant que worstScenario puisque "contractGroup.ScanRisk" étant égal à "Max(contractGroup.RiskValue)"
                        double worstScenario = (double)System.Math.Max(contractGroup.ScanRisk, 0);
                        double worstScenarioSquare = System.Math.Pow(worstScenario, 2);
                        // Step 6,7 : Calculer l'idiosyncratic risk de chaque combined commodity et en faire la somme
                        idiosyncraticRiskLMax += (1 - System.Math.Pow((double)contractGroup.ContractGroupParameters.LambdaMax, 2)) * worstScenarioSquare;
                        idiosyncraticRiskLMin += (1 - System.Math.Pow((double)contractGroup.ContractGroupParameters.LambdaMin, 2)) * worstScenarioSquare;
                    }
                    oneFactorSpread.IdiosyncraticRiskLMax = (decimal)idiosyncraticRiskLMax;
                    oneFactorSpread.IdiosyncraticRiskLMin = (decimal)idiosyncraticRiskLMin;

                    // Step 8 : Calculer le ScanRisk after Offset
                    oneFactorSpread.ScanRiskOffsetLMax = (decimal)System.Math.Sqrt(idiosyncraticRiskLMax + (double)oneFactorSpread.FinalGeneralRiskLMax);
                    oneFactorSpread.ScanRiskOffsetLMin = (decimal)System.Math.Sqrt(idiosyncraticRiskLMin + (double)oneFactorSpread.FinalGeneralRiskLMin);
                    // Step 9 : Reprendre les steps 1 à 8 pour le lambda min : réalisé en même temps que pour le lambda max

                    // Step 10 : Prendre le maximum entre les ScanRisk after Offset pour lambda max et lambda min
                    oneFactorSpread.ScanRiskOffset = System.Math.Max(oneFactorSpread.ScanRiskOffsetLMax, oneFactorSpread.ScanRiskOffsetLMin);

                    // Step 11: Calculer l'Inter-Commodity Offset
                    // Calculer le Scanrisk tout combined commodity confondu
                    // PM 20160427 [22110] Prendre la somme des Scanrisk et non pas recalculer le Scanrisk sur la somme des riskvalues
                    oneFactorSpread.GlobalScanRisk = lambdaContractGroupRisk.Sum(cg => cg.ScanRisk);
                    // Calculer la valeur OffsetPercentage (comprise en 0 et 1) représentant le pourcentage d'offset
                    // Cette valeur est cappée en fonction des recommendations réglementaire (oneFactorSpread.OffsetMax => 0.80 [80%])
                    oneFactorSpread.OffsetPercentage = System.Math.Max(System.Math.Min(1 - (oneFactorSpread.ScanRiskOffset / oneFactorSpread.GlobalScanRisk), oneFactorSpread.OffsetMax), 0);

                    // Remplacer de l'Inter-Commodity Spread par le résultat du calcul de l'Inter-Commodity Offset
                    foreach (SpanContractGroupRisk contractGroup in lambdaContractGroupRisk)
                    {
                        contractGroup.InterCommoditySpreadCredit = oneFactorSpread.OffsetPercentage * contractGroup.ScanRisk;
                    }
                }
            }
            return oneFactorSpread;
        }

        /// <summary>
        /// Calcul du Final General Risk
        /// </summary>
        /// <param name="pGeneralRiskComponent"></param>
        /// <returns></returns>
        /// PM 20150930 [21134] New
        private decimal FinalGeneralRisk(IEnumerable<SpanMatrice> pGeneralRiskComponent)
        {
            decimal finalGeneralRisk = 0;
            if ((pGeneralRiskComponent != default(IEnumerable<SpanMatrice>)) && (pGeneralRiskComponent.Count() > 0))
            {
                // Step 2 : Sommer des GeneralRisk par scénario
                SpanMatrice generalRisk = pGeneralRiskComponent.Aggregate((rv, next) => SpanMatrice.Add(rv, next));
                // Step 3 : Prendre le maximum des scénarios
                decimal? maxGeneralRisk = generalRisk.Max();
                if (maxGeneralRisk.HasValue)
                {
                    // Le Maximum General Risk doit être positif (les valeurs de risque positives sont des pertes)
                    decimal mgr = System.Math.Max(0, maxGeneralRisk.Value);
                    // Step 4 : Calculer le Final General Risk
                    finalGeneralRisk = (decimal)System.Math.Pow((double)mgr, 2);
                }
            }
            return finalGeneralRisk;
        }
        #endregion OneFactorInterCommoditySpreadCredit

        /// <summary>
        /// Build the Span hierarchy
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pGroupedPositionsByIdAsset">the positions set related to the current actor/book</param>
        /// <param name="pCoveredQuantities">Collection containing all the covered quantities for short positions/settlements</param>
        /// <returns>the Span hierarchy</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private IEnumerable<SpanExchangeComplexCom> GetSpanHierarchy(
        //    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset,
        //    IEnumerable<StockCoverageCommunicationObject> pCoveredQuantities)
        private IEnumerable<SpanExchangeComplexCom> GetSpanHierarchy(SpanEvaluationData pEvaluationData,IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            // idAsset en position
            var assetPos = (
                from position in pGroupedPositionsByIdAsset
                group position.First.idAsset by position.First.idAsset into idAsset
                select idAsset.Key).Distinct();

            // RiskArray Parameters des assets en position
            var riskArrayParamPos = (
                from idAsset in assetPos
                join riskArray in m_RiskArrayParameters on idAsset equals riskArray.IdAsset
                into posRiskArray
                from riskArray in posRiskArray.DefaultIfEmpty(m_EmptyRiskArrayParameters)
                select new { IdAsset = idAsset, RiskArrayParameters = riskArray }).Distinct();

            // Contract Parameters des assets en position
            var contractParamPos = (
                from riskArray in riskArrayParamPos
                join contract in m_ContractParameters on riskArray.RiskArrayParameters.SPANContractId equals contract.SPANContractId
                into posContract
                from contract in posContract.DefaultIfEmpty(m_EmptyContractParameters)
                select new { ContratParameters = contract, riskArray.IdAsset }).Distinct();

            // Contract Group Parameters des assets en position
            var contractGroupParamPos = (
                from contract in contractParamPos
                join contractGroup in m_ContractGroupParameters on contract.ContratParameters.SPANContractGroupId equals contractGroup.SPANContractGroupId
                into posContractGroup
                from contractGroup in posContractGroup.DefaultIfEmpty(m_EmptyContractGroupParameters)
                select new { ContractGroupParameters = contractGroup, contract.IdAsset }).Distinct();

            // Ajout des Groupes de contrat qui n'ont pas de position
            var contractGroupWithPosition =
                from contractGroup in pEvaluationData.ContractGroupRisk
                join contractGroupPos in contractGroupParamPos on contractGroup.ContractGroupParameters.SPANContractGroupId equals contractGroupPos.ContractGroupParameters.SPANContractGroupId
                select contractGroup;

            var contractGroupWithoutPosition = pEvaluationData.ContractGroupRisk.Except(contractGroupWithPosition);

            var contractGroupParamWithoutPos =
                from contractGroup in contractGroupWithoutPosition
                select new { contractGroup.ContractGroupParameters, IdAsset = (int)0 };

            contractGroupParamPos = contractGroupParamPos.Concat(contractGroupParamWithoutPos);

            // Combined Group Parameters des assets en position
            var combinedGroupParamPos = (
                from contractGroup in contractGroupParamPos
                join combinedGroup in m_CombinedGroupParameters on contractGroup.ContractGroupParameters.SPANCombinedGroupId equals combinedGroup.SPANCombinedGroupId
                into posCombinedGroup
                from combinedGroup in posCombinedGroup.DefaultIfEmpty(m_EmptyCombinedGroupParameters)
                select new { CombinedGroupParameters = combinedGroup, contractGroup.IdAsset }).Distinct();

            // Exchange Complex Parameters des assets en position
            var exchangeComplexParamPos = (
                from combinedGroup in combinedGroupParamPos
                join exchangeComplex in m_ExchangeComplexParameters on combinedGroup.CombinedGroupParameters.SPANExchangeComplexId equals exchangeComplex.SPANExchangeComplexId
                into posExchangeComplex
                from exchangeComplex in posExchangeComplex.DefaultIfEmpty(m_EmptyExchangeComplexParameters)
                select new { ExchangeComplexParameters = exchangeComplex, combinedGroup.IdAsset }).Distinct();

            // Construction de la hierarchie de chaque exchange complex
            IEnumerable<SpanExchangeComplexCom> exchangeCom =
                from exchangeComplex in exchangeComplexParamPos
                group exchangeComplex.IdAsset by exchangeComplex.ExchangeComplexParameters
                    into exchangeComplexAsset
                    select new SpanExchangeComplexCom
                    {
                        SPANExchangeComplexId = exchangeComplexAsset.Key.SPANExchangeComplexId,

                        ExchangeComplex = exchangeComplexAsset.Key.ExchangeComplex,

                        IsOptionValueLimit = exchangeComplexAsset.Key.IsOptionValueLimit,

                        Missing = (exchangeComplexAsset.Key == m_EmptyExchangeComplexParameters),

                        /*Positions = from pos in pGroupedPositionsByIdAsset
                                    join idAsset in exchangeComplexAsset on pos.First.idAsset equals idAsset
                                    select pos,*/

                        Parameters = (from combinedGroup in combinedGroupParamPos
                                      where combinedGroup.CombinedGroupParameters.SPANExchangeComplexId == exchangeComplexAsset.Key.SPANExchangeComplexId
                                      group combinedGroup.IdAsset by combinedGroup.CombinedGroupParameters
                                          into combinedGroupAsset
                                          select new SpanCombinedGroupCom
                                          {
                                              SPANCombinedGroupId = combinedGroupAsset.Key.SPANCombinedGroupId,

                                              CombinedGroup = combinedGroupAsset.Key.CombinedGroupCode,

                                              /*Positions = from pos in pGroupedPositionsByIdAsset
                                                          join idAsset in combinedGroupAsset on pos.First.idAsset equals idAsset
                                                          select pos,*/

                                              Parameters = (from contractGroup in contractGroupParamPos
                                                            where contractGroup.ContractGroupParameters.SPANCombinedGroupId == combinedGroupAsset.Key.SPANCombinedGroupId
                                                            group contractGroup.IdAsset by contractGroup.ContractGroupParameters
                                                                into contractGroupAsset
                                                                select new SpanContractGroupCom
                                                                {
                                                                    SPANContractGroupId = contractGroupAsset.Key.SPANContractGroupId,

                                                                    ContractGroup = contractGroupAsset.Key.ContractGroupCode,

                                                                    InterMonthSpreadChargeMethod = contractGroupAsset.Key.IntraContractSpreadMethod,

                                                                    DeliveryMonthChargeMethod = contractGroupAsset.Key.DeliveryChargeMethod,

                                                                    StrategySpreadChargeMethod = contractGroupAsset.Key.StrategySpreadMethod,

                                                                    // PM 20150930 [21134] Ajout IsUseLambda, LambdaMax, LambdaMin
                                                                    IsUseLambda = contractGroupAsset.Key.IsUseLambda,
                                                                    LambdaMax = contractGroupAsset.Key.LambdaMax,
                                                                    LambdaMin = contractGroupAsset.Key.LambdaMin,

                                                                    /*Positions = from pos in pGroupedPositionsByIdAsset
                                                                                join idAsset in contractGroupAsset on pos.First.idAsset equals idAsset
                                                                                select pos,*/

                                                                    Parameters = (from contract in contractParamPos
                                                                                  where contract.ContratParameters.SPANContractGroupId == contractGroupAsset.Key.SPANContractGroupId
                                                                                  group contract.IdAsset by contract.ContratParameters
                                                                                      into contractAsset
                                                                                      select new SpanContractCom
                                                                                      {
                                                                                          SPANContractId = contractAsset.Key.SPANContractId,

                                                                                          Contract = contractAsset.Key.ContractSymbol,

                                                                                          Positions = from pos in pGroupedPositionsByIdAsset
                                                                                                      join idAsset in contractAsset on pos.First.idAsset equals idAsset
                                                                                                      select pos,

                                                                                          Parameters = null,
                                                                                      }
                                                                                 ).ToArray(),

                                                                }
                                                           ).ToArray(),
                                          }
                                      ).ToArray(),
                    };
            return exchangeCom;
        }
        
        /// <summary>
        /// Calcul de l'Additionnal Margin BoM (AMBO)
        /// </summary>
        /// <param name="pPositions"></param>
        /// <returns></returns>
        // PM 20190401 [24625][24387] New
        private SpanEvaluationData EvaluateAMBO(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            SpanEvaluationData additionalMarginBoM = default;
            if (pPositions != default)
            {
                additionalMarginBoM = new SpanEvaluationData
                {
                    Positions =
                    from pos in pPositions
                    where (pos.Second.DeliveryQuantity != 0) && (pos.Second.DeliveryExpressionType == ExpressionType.ECC_AMBO)
                    select pos
                };

                if (additionalMarginBoM.Positions.Count() > 0)
                {
                    List<Pair<PosRiskMarginKey, RiskMarginPosition>> positionAMBO = new List<Pair<PosRiskMarginKey,RiskMarginPosition>>();
                    //
                    foreach (Pair<PosRiskMarginKey, RiskMarginPosition> pos in additionalMarginBoM.Positions)
                    {
                        RiskMarginPosition marginPos = pos.Second;
                        marginPos.Quantity = marginPos.DeliveryQuantity;
                        positionAMBO.Add(new Pair<PosRiskMarginKey, RiskMarginPosition>(pos.First, marginPos));
                    }

                    EvaluateRisk(additionalMarginBoM, positionAMBO);
                }
            }
            return additionalMarginBoM;
        }

        /// <summary>
        /// Calcul du Concentration Risk Margin de l'ECC
        /// </summary>
        /// <param name="pPositions"></param>
        /// <param name="pSpanAmount"></param>
        /// <param name="pAddOnDays"></param>
        // PM 20190801 [24717] New
        private ECCConRiskMargin EvaluteConcentrationRiskMargin(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions, Money pSpanAmount, decimal pAddOnDays)
        {
            ECCConRiskMargin concentrationRiskMargin = default;
            if ((pPositions != default) && (pSpanAmount != default))
            {
                // Calcul du Position Size détaillé pour chaque asset
                IEnumerable<ECCConRiskMarginAssetPosition> assetPositionSize =
                    from position in pPositions
                    join assetParam in m_AssetExpandedParameters on position.First.idAsset equals assetParam.AssetId
                    where assetParam.Currency == pSpanAmount.Currency
                    select new ECCConRiskMarginAssetPosition
                    (
                        assetParam.AssetId,
                        assetParam.ContractSymbol,
                        assetParam.ContractId,
                        assetParam.ContractMultiplier,
                        position.Second.Quantity * (position.First.Side == SPANMethod.FixBuySide ? 1 : -1),
                        position.Second.Quantity * (position.First.Side == SPANMethod.FixBuySide ? 1 : -1) * assetParam.ContractMultiplier
                    );

                // Cumul du Position Size détaillé par derivative contract
                IEnumerable<ECCConRiskMarginContractPosition> contractPositionSize =
                    from dps in assetPositionSize
                    group dps by new { dps.ContractId, dps.ContractSymbol } into posByContract
                    select new ECCConRiskMarginContractPosition
                    (
                        posByContract.Key.ContractSymbol,
                        posByContract.Key.ContractId,
                        posByContract
                    );

                // Calcul des montants utiles au calcul au concentration risk par derivative contract
                IEnumerable<ECCConRiskMarginUnit> marketPositionSize =
                    from posSize in contractPositionSize
                    join combComParam in m_ECCCombComStressParameters on posSize.ContractId equals combComParam.ContractId
                    join mrkVol in m_ECCMarketVolumeParameters on combComParam.CombinedCommodityStress equals mrkVol.CombinedCommodityStress
                    group posSize by mrkVol into posByCombCom
                    select new ECCConRiskMarginUnit(posByCombCom.Key,
                        posByCombCom.ToDictionary(k => k.ContractId, v => v), pAddOnDays);

                // Calcul du concentration risk 
                concentrationRiskMargin = new ECCConRiskMargin(marketPositionSize, pSpanAmount);
            }
            return concentrationRiskMargin;
        }

        /// <summary>
        /// Création de l'objet Com pour le calcul du Concentration Risk Margin de l'ECC
        /// </summary>
        /// <param name="pConRiskMargin"></param>
        /// <returns></returns>
        // PM 20190801 [24717] New
        private ECCConcentrationRiskMarginCom ConcentrationRiskMarginCom(IEnumerable<ECCConRiskMargin> pConRiskMargin)
        {
            ECCConcentrationRiskMarginCom com = new ECCConcentrationRiskMarginCom();
            if (pConRiskMargin != default(ECCConRiskMargin))
            {
                com.AdditionalAddOn = MethodParameter.ECCAddOnDays;
                //
                com.ConcentrationRiskMarginAmounts = (
                    from conRisk in pConRiskMargin
                    select new ECCConRiskMarginAmountCom
                    {
                        AbsoluteCumulativePosition = conRisk.AbsoluteCumulativePosition,
                        ConcentrationRiskMargin = conRisk.ConcentrationRiskMargin,
                        LiquidationPeriod = conRisk.LiquidationPeriod,
                        WeightedAbsCumulPosition = conRisk.WeightedAbsCumulPosition,
                        //
                        ConcentrationRiskMarginUnits = (
                            from unit in conRisk.ConcentrationRiskMarginUnits
                            select new ECCConRiskMarginUnitCom
                            {
                                AbsoluteCumulativePosition = unit.AbsoluteCumulativePosition,
                                CombinedCommodityStress = unit.CombinedCommodityStress,
                                DailyMarketVolume = unit.DailyMarketVolume,
                                LiquidationPeriod = unit.LiquidationPeriod ?? 0,
                                WeightedAbsCumulPosition = unit.WeightedAbsCumulPosition,
                            }).ToArray(),
                    }).ToArray();
                //
                com.MarketVolume = (
                        from volume in m_ECCMarketVolumeParameters
                        select new ECCMarketVolumeCom
                        {
                            CombinedCommodityStress = volume.CombinedCommodityStress,
                            MarketVolume = volume.MarketVolume,
                        }).ToArray();
            }
            return com;
        }
        #endregion
    }

    /// <summary>
    /// Classe de gestion de la méthode de calcul de risque SPAN C21
    /// 
    /// Les valeurs des matrices de risque sont données avec 2 décimales
    /// Les valeurs des deltas sont exprimés avec 4 décimales
    /// 
    /// Les calculs de deltas sont réalisés avec des deltas arrondis à la 4ème décimale (plus proche)
    /// Le montant de la majoration pour spreads calendaires est un nombre à 2 décimales 
    /// 
    /// Le risque ajusté en volatilité et le risque de temps sont arrondis à 2 décimales (plus proche)
    /// Le risque unitaire de prix (risque de prix / delta net total) est arrondi à 4 décimales (plus proche)
    /// </summary>
    public sealed class SPANC21Method : SPANMethod
    {
        #region override accessors
        /// <summary>
        /// Methode Type
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.SPAN_C21; }
        }

        /// <summary>
        /// Rouding Precision of Risk Values
        /// </summary>
        protected override int RiskValuePrecision
        {
            get { return 2; }

        }
        /// <summary>
        /// Rouding Precision of Delta Values
        /// </summary>
        protected override int DeltaValuePrecision
        {
            get { return 4; }
        }
        #endregion override accessors

        #region constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        internal SPANC21Method()
        {
            // PM 20170313 [22833] Ajout alimentation de m_RiskMethodDataType
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }
        #endregion Constructor

        #region override base methods
        /// <summary>
        /// Calcul du risque pondéré de prix future.
        /// </summary>
        /// <param name="pRisk"></param>
        /// <param name="pDelta"></param>
        /// <returns></returns>
        public override decimal SpanWeightedFuturePriceRisk(decimal pRisk, decimal pDelta)
        {
            // RD 20200402 [25274] Arrondi du Delta avant son utilisation
            // pour corriger le cas ou Delta=0.00000000000000000000000001M 
            // car il est issus d'un calcul avec division de deux entiers (Exemple: 12/18)
            decimal delta = System.Math.Abs(pDelta);

            if (0 == DefaultPrecision)
            {
                delta = SpanRoundAmount(delta);
            }
            else
            {
                delta = SpanRoundDelta(delta);
            }

            decimal weightedPriceRisk;
            if (delta != 0)
            {
                if (0 == DefaultPrecision)
                {
                    weightedPriceRisk = System.Math.Floor(pRisk / delta * 100) / 100;
                }
                else
                {
                    // PM 20150928 [21405][20948] Changement de méthode pour l'arrondie
                    //weightedPriceRisk = SpanRoundDelta(pRisk / System.Math.Abs(pDelta));
                    weightedPriceRisk = SpanRoundWeightedFuturePriceRisk(pRisk / delta);
                }
            }
            else
            {
                weightedPriceRisk = SpanRoundAmount(pRisk);
            }
            return weightedPriceRisk;
        }

        /// <summary>
        /// Arrondie utile pour le WeightedFuturePriceRisk.
        /// </summary>
        /// <param name="pAmount">Montant à arrondir</param>
        /// <returns>Montant arrondie</returns>
        /// PM 20150928 [21405][20948] New
        public override decimal SpanRoundWeightedFuturePriceRisk(decimal pAmount)
        {
            return SpanRoundDelta(pAmount);
        }
        #endregion override base methods
    }

    /// <summary>
    /// Classe de gestion de la méthode de calcul de risque SPAN CME
    /// </summary>
    public sealed class SPANCMEMethod : SPANMethod
    {
        #region override accessors
        /// <summary>
        /// Methode Type
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.SPAN_CME; }
        }

        /// <summary>
        /// Rouding Precision of Risk Values
        /// </summary>
        protected override int RiskValuePrecision
        {
            get { return 0; }
        }
        /// <summary>
        /// Rouding Precision of Delta Values
        /// </summary>
        protected override int DeltaValuePrecision
        {
            get { return DefaultPrecision; }
        }
        #endregion override accessors

        #region constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        internal SPANCMEMethod()
        {
            // PM 20170313 [22833] Ajout alimentation de m_RiskMethodDataType
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }
        #endregion Constructor

        #region override base methods
        /// <summary>
        /// Definition de l'arrondi du nombre de spread.
        /// Utilisé en particulier pour le nombre de Spread Inter Contract Group réalisable
        /// (différent entre la méthode CME et C21)
        /// </summary>
        /// <param name="pSpreadNumber"></param>
        /// <returns></returns>
        // PM 20151125 [21595] Ajout override pour la class SPANCMEMethod
        public override decimal SpanRoundSpreadNumber(decimal pSpreadNumber)
        {
            return SpanRoundDelta(pSpreadNumber);
        }

        /// <summary>
        /// Calcul du risque pondéré de prix future.
        /// </summary>
        /// <param name="pRisk"></param>
        /// <param name="pDelta"></param>
        /// <returns></returns>
        public override decimal SpanWeightedFuturePriceRisk(decimal pRisk, decimal pDelta)
        {
            // RD 20200402 [25274] Arrondi du Delta avant son utilisation
            // pour corriger le cas ou Delta=0.00000000000000000000000001M 
            // car il est issus d'un calcul avec division de deux entiers (Exemple: 12/18)
            decimal delta = System.Math.Abs(pDelta);

            if (0 == DefaultPrecision)
            {
                delta = SpanRoundAmount(delta);
            }
            else
            {
                delta = SpanRoundDelta(delta);
            }

            decimal weightedPriceRisk;
            if (delta != 0)
            {
                if (0 == DefaultPrecision)
                {
                    weightedPriceRisk = System.Math.Floor(pRisk / delta * 100) / 100;
                }
                else
                {
                    // PM 20150928 [21405][20948] Arrondie du weightedPriceRisk lorsque la précision par défaut et différent de 0
                    //weightedPriceRisk = SpanRoundDelta(pRisk / System.Math.Abs(pDelta));
                    weightedPriceRisk = SpanRoundWeightedFuturePriceRisk(pRisk / delta);
                }
            }
            else
            {
                weightedPriceRisk = SpanRoundAmount(pRisk);
            }
            return weightedPriceRisk;
        }
        #endregion override base methods
    }

    /// <summary>
    /// Classe de gestion de la méthode de calcul de risque London SPAN (SPAN LIFFE)
    /// </summary>
    public sealed class LondonSPANMethod : SPANMethod
    {
        #region override accessors
        /// <summary>
        /// Methode Type
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.London_SPAN; }
        }

        /// <summary>
        /// Rouding Precision of Risk Values
        /// </summary>
        protected override int RiskValuePrecision
        {
            get { return 0; }
        }
        /// <summary>
        /// Rouding Precision of Delta Values
        /// </summary>
        protected override int DeltaValuePrecision
        {
            get { return 4; }
        }
        #endregion override accessors

        #region constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        internal LondonSPANMethod()
        {
            // PM 20170313 [22833] Ajout alimentation de m_RiskMethodDataType
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }
        #endregion Constructor

        #region override base methods
        /// <summary>
        /// Definition de l'arrondi du nombre de spread.
        /// Utilisé en particulier pour le nombre de Spread Inter Contract Group réalisable
        /// (différent entre la méthode CME et C21)
        /// </summary>
        /// <param name="pSpreadNumber"></param>
        /// <returns></returns>
        // PM 20151127 [21571][21605] Ajout override pour la class LondonSPANMethod
        public override decimal SpanRoundSpreadNumber(decimal pSpreadNumber)
        {
            return RoundAmount(pSpreadNumber, 6);
        }

        /// <summary>
        /// Calcul du risque pondéré de prix future.
        /// </summary>
        /// <param name="pRisk"></param>
        /// <param name="pDelta"></param>
        /// <returns></returns>
        public override decimal SpanWeightedFuturePriceRisk(decimal pRisk, decimal pDelta)
        {
            // RD 20200402 [25274] Arrondi du Delta avant son utilisation
            // pour corriger le cas ou Delta=0.00000000000000000000000001M 
            // car il est issus d'un calcul avec division de deux entiers (Exemple: 12/18)
            decimal delta = System.Math.Abs(pDelta);

            delta = SpanRoundDelta(delta);

            if (delta != 0)
            {
                // PM 20150928 [21405][20948] Changement de méthode pour l'arrondie
                //return RoundAmount(pRisk / System.Math.Abs(pDelta), 0);
                return SpanRoundWeightedFuturePriceRisk(pRisk / delta);
            }
            else
            {
                // PM 20150928 [21405][20948] Changement de méthode pour l'arrondie
                //return RoundAmount(pRisk, 0);
                return SpanRoundWeightedFuturePriceRisk(pRisk);
            }
        }

        /// <summary>
        /// Arrondie utile pour le WeightedFuturePriceRisk
        /// </summary>
        /// <param name="pAmount">Montant à arrondir</param>
        /// <returns>Montant arrondi</returns>
        /// PM 20150928 [21405][20948] New
        public override decimal SpanRoundWeightedFuturePriceRisk(decimal pAmount)
        {
            return RoundAmount(pAmount, 0);
        }

        /// <summary>
        /// Calcul les matrices et paramètres de risques de chaque position reçue en paramètres
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pPositions">Ensemble de position</param>
        /// <returns>Les données de risque de chaque position reçue en paramètres</returns>
        /// PM 20151127 [21571][21605] Ajout prise en compte de DeltaDen et du lotsize pour le delta et ajout arrondie des montants des scénarios de risque
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //internal override List<SpanAssetRiskValues> ComputeAssetScanTiersRiskValues(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        internal override List<SpanAssetRiskValues> ComputeAssetScanTiersRiskValues(SpanEvaluationData pEvaluationData,  IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            List<SpanAssetRiskValues> assetScanTiersRiskValues = null;

            // Calcul des matrices de risque sur chaque asset en fonction de la position
            IEnumerable<SpanAssetRiskValues> assetScanValues =
                from position in pPositions
                join riskArrayParam in m_RiskArrayParameters on position.First.idAsset equals riskArrayParam.IdAsset
                join maturityParam in pEvaluationData.FutureMonthParam on new { riskArrayParam.SPANContractId, riskArrayParam.FutureMaturity, riskArrayParam.OptionMaturity } equals new { maturityParam.SPANContractId, maturityParam.FutureMaturity, maturityParam.OptionMaturity }
                join assetMatParam in m_AssetExpandedParameters on riskArrayParam.IdAsset equals assetMatParam.AssetId
                join contractParam in m_ContractParameters on riskArrayParam.SPANContractId equals contractParam.SPANContractId
                join contractGroupParam in m_ContractGroupParameters on contractParam.SPANContractGroupId equals contractGroupParam.SPANContractGroupId
                select new SpanAssetRiskValues
                {
                    AssetId = riskArrayParam.IdAsset,
                    SPANContractId = riskArrayParam.SPANContractId,
                    FutureMaturity = riskArrayParam.FutureMaturity,
                    OptionMaturity = riskArrayParam.OptionMaturity,
                    PutOrCall = riskArrayParam.PutOrCall,
                    StrikePrice = riskArrayParam.StrikePrice,
                    Side = position.First.Side == SPANMethod.FixBuySide ? SideEnum.Buy : SideEnum.Sell,
                    Quantity = position.Second.Quantity * (position.First.Side == SPANMethod.FixBuySide ? 1 : -1),
                    // PM 20151127 [21571][21605] Ajout prise en compte de DeltaDen et du lotsize
                    //Delta = riskArrayParam.CompositeDelta * position.Second.Quantity * (position.First.side == SPANMethod.FixBuySide ? 1 : -1),
                    Delta = (riskArrayParam.CompositeDelta * position.Second.Quantity * ((position.First.Side == SPANMethod.FixBuySide) ? 1 : -1))
                            * riskArrayParam.LotSize
                            / ((contractParam.DeltaDen > 0) ? (contractParam.DeltaDen) : 1),
                    RiskValue = (from values in new List<KeyValuePair<int, decimal>>
                                   { 
                                        new KeyValuePair<int,decimal>(1,riskArrayParam.RiskValue1 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(2,riskArrayParam.RiskValue2 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(3,riskArrayParam.RiskValue3 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(4,riskArrayParam.RiskValue4 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(5,riskArrayParam.RiskValue5 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(6,riskArrayParam.RiskValue6 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(7,riskArrayParam.RiskValue7 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(8,riskArrayParam.RiskValue8 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(9,riskArrayParam.RiskValue9 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(10,riskArrayParam.RiskValue10 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(11,riskArrayParam.RiskValue11 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(12,riskArrayParam.RiskValue12 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(13,riskArrayParam.RiskValue13 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(14,riskArrayParam.RiskValue14 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(15,riskArrayParam.RiskValue15 * riskArrayParam.LotSize * contractParam.TickValue),
                                        new KeyValuePair<int,decimal>(16,riskArrayParam.RiskValue16 * riskArrayParam.LotSize * contractParam.TickValue)
                                   }
                                 // PM 20151127 [21571][21605] Ajout arrondie des montants des scénarios de risque
                                 //select new KeyValuePair<int, decimal>(values.Key, values.Value * position.Second.Quantity * (position.First.side == SPANMethod.FixBuySide ? 1 : -1))
                                 select new KeyValuePair<int, decimal>(values.Key, SpanRoundAmount(values.Value * position.Second.Quantity * (position.First.Side == SPANMethod.FixBuySide ? 1 : -1)))
                                 ).ToList(),
                    // PM 20131227 [19407] Ne calculer la valeur option que sur les options en Premium Style
                    //OptionValue = ((assetMatParam.Category == "O") && ((FuturesValuationMethodEnum)StringToEnum.GetEnumValue(new FuturesValuationMethodEnum(), assetMatParam.ValuationMethod) != FuturesValuationMethodEnum.PremiumStyle)) ? RiskMethodExtensions.ContractValue(position.Second.Quantity, riskArrayParam.Price, assetMatParam.ContractMultiplier, assetMatParam.InstrumentNum, assetMatParam.InstrumentDen) : 0,
                    // PM 20150707 [21104] Simplication et gestion arrondie
                    //OptionValue = ((assetMatParam.Category == "O") && ((FuturesValuationMethodEnum)StringToEnum.GetEnumValue(new FuturesValuationMethodEnum(), assetMatParam.ValuationMethod) == FuturesValuationMethodEnum.PremiumStyle)) ? RiskMethodExtensions.ContractValue(position.Second.Quantity, riskArrayParam.Price, assetMatParam.ContractMultiplier, assetMatParam.InstrumentNum, assetMatParam.InstrumentDen) : 0,
                    OptionValue = ((assetMatParam.Category == "O") && (assetMatParam.ValuationMethodEnum == FuturesValuationMethodEnum.PremiumStyle)) ? RiskMethodExtensions.ContractValue(assetMatParam, position.Second.Quantity, riskArrayParam.Price) : 0,
                    // PM 20150902 [21385] Ajout contractParam.DeltaScalingFactor
                    //ShortOptionMinimum = ((assetMatParam.Category == "O") && (position.First.side == SPANMethod.FixSellSide)) ? contractGroupParam.ShortOptionChargeRate * maturityParam.DeltaScalingFactor * position.Second.Quantity : 0,
                    ShortOptionMinimum = ((assetMatParam.Category == "O") && (position.First.Side == SPANMethod.FixSellSide)) ? contractGroupParam.ShortOptionChargeRate * maturityParam.DeltaScalingFactor * contractParam.DeltaScalingFactor * position.Second.Quantity : 0,
                    MarginCurrency = contractGroupParam.MarginCurrency ?? assetMatParam.Currency,
                };

            if (null != assetScanValues)
                assetScanTiersRiskValues = assetScanValues.ToList();

            return assetScanTiersRiskValues;
        }

        /// <summary>
        /// Récupert les paramètres de calcul SPAN de l'acteur spécifié
        /// (Les paramètres sont fixes pour London SPAN)
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pActorId">Identifiant de l'acteur pour lequel le calcul SPAN est réalisé</param>
        /// <returns>true si l'acteur a été trouvé, sinon false</returns>
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //protected override bool GetSpanAccountParameters(int pActorId)
        internal override bool GetSpanAccountParameters(SpanEvaluationData pEvaluationData, int pActorId)
        {
            bool actorHasParameter = MarginReqOfficeParameters.ContainsKey(pActorId);
            //
            if (pEvaluationData != default)
            {
                pEvaluationData.SpanAccountType = SpanAccountType.Default;
                pEvaluationData.IsMaintenanceAmount = false;
            }
            return actorHasParameter;
        }

        /// <summary>
        /// Cumculate all risk amounts by currency
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <returns>a list of risk amounts</returns>
        //internal protected override List<Money> CumulateRiskElement()
        internal override List<Money> CumulateRiskElement(SpanEvaluationData pEvaluationData)
        {
            List<Money> riskAmmounts = null;
            //
            if ((pEvaluationData != default) && (pEvaluationData.CombinedGroupRisk != null))
            {
                List<Money> marginRequirement = new List<Money>();

                // Prendre les margin requierement en devise de chaque margin groupe
                foreach (var combinedGroup in pEvaluationData.CombinedGroupRisk)
                {
                    marginRequirement.AddRange(combinedGroup.InitialRequirement);
                }

                // Cumul des margins requierement par devise
                riskAmmounts = (
                    from margin in marginRequirement
                    group margin by margin.Currency into marginByCurrency
                    select new Money(marginByCurrency.Sum(r => r.Amount.DecValue), marginByCurrency.Key)).ToList();
            }
            return riskAmmounts;
        }

        /// <summary>
        /// Converti les matrices de risque d'un contrat de la devise du contrat en la devise de deposit
        /// </summary>
        /// <param name="pContractRisk">Liste des valeurs de risque du contrat</param>
        /// <returns>Liste des valeurs de risque du contrat converties</returns>
        internal override SpanContractRisk ConvertContractRiskValues(SpanContractRisk pContractRisk)
        {
            if (pContractRisk != null)
            {
                SpanCurConv currencyConversion = default;
                if (m_CurConvParameters != null)
                {
                    currencyConversion = (
                    from curConv in m_CurConvParameters
                    join contractGroup in m_ContractGroupParameters on new { curConv.ContractCurrency, curConv.MarginCurrency } equals new { pContractRisk.ContractParameters.ContractCurrency, contractGroup.MarginCurrency }
                    where ((contractGroup.SPANContractGroupId == pContractRisk.ContractParameters.SPANContractGroupId)
                       && (pContractRisk.ContractParameters.ContractCurrency != contractGroup.MarginCurrency))
                    select curConv).FirstOrDefault();
                }

                if (default(SpanCurConv) != currencyConversion)
                {
                    decimal FXRateUp = currencyConversion.FXRate * (1 + currencyConversion.ShiftUp);
                    decimal FXRateDown = currencyConversion.FXRate * (1 + currencyConversion.ShiftDown);
                    int roundingLevel = 1;

                    // Rechercher la précision d'arrondie de la devise
                    if (null != m_CurrencyParameters)
                    {
                        SpanCurrency currency = m_CurrencyParameters.FirstOrDefault(c => c.Currency == currencyConversion.MarginCurrency);
                        if (default(SpanCurrency) != currency)
                        {
                            roundingLevel = (int)System.Math.Pow(10.0, (double)currency.Exponent);
                        }
                    }

                    if (null != pContractRisk.RiskValue)
                    {
                        pContractRisk.ConvertedRiskValue = (
                            from riskValues in pContractRisk.RiskValue
                            select new KeyValuePair<int, decimal>
                                (
                                riskValues.Key,
                                SpanRoundAmountCurrency(System.Math.Max(riskValues.Value * FXRateUp, riskValues.Value * FXRateDown), currencyConversion.MarginCurrency)
                                )
                            ).ToDictionary(p => p.Key, p => p.Value);
                    }
                    if (null != pContractRisk.RiskValueLong)
                    {
                        pContractRisk.ConvertedRiskValueLong = (
                            from riskValues in pContractRisk.RiskValueLong
                            select new KeyValuePair<int, decimal>
                                (
                                riskValues.Key,
                                SpanRoundAmountCurrency(System.Math.Max(riskValues.Value * FXRateUp, riskValues.Value * FXRateDown), currencyConversion.MarginCurrency)
                                )
                            ).ToDictionary(p => p.Key, p => p.Value);
                    }
                    if (null != pContractRisk.RiskValueShort)
                    {
                        pContractRisk.ConvertedRiskValueShort = (
                            from riskValues in pContractRisk.RiskValueShort
                            select new KeyValuePair<int, decimal>
                                (
                                riskValues.Key,
                                SpanRoundAmountCurrency(System.Math.Max(riskValues.Value * FXRateUp, riskValues.Value * FXRateDown), currencyConversion.MarginCurrency)
                                )
                            ).ToDictionary(p => p.Key, p => p.Value);
                    }
                    pContractRisk.ConvertedLongOptionValue = pContractRisk.LongOptionValue * currencyConversion.FXRate;
                    pContractRisk.ConvertedShortOptionValue = pContractRisk.ShortOptionValue * currencyConversion.FXRate;
                }
                else
                {
                    if (null != pContractRisk.RiskValue)
                    {
                        pContractRisk.ConvertedRiskValue = (
                        from riskValues in pContractRisk.RiskValue
                        select new KeyValuePair<int, decimal>
                            (
                            riskValues.Key,
                            SpanRoundAmountCurrency(riskValues.Value, pContractRisk.Currency)
                            )
                        ).ToDictionary(p => p.Key, p => p.Value);
                    }
                    if (null != pContractRisk.RiskValueLong)
                    {
                        pContractRisk.ConvertedRiskValueLong = (
                            from riskValues in pContractRisk.RiskValueLong
                            select new KeyValuePair<int, decimal>
                                (
                                riskValues.Key,
                                SpanRoundAmountCurrency(riskValues.Value, pContractRisk.Currency)
                                )
                            ).ToDictionary(p => p.Key, p => p.Value);
                    }
                    if (null != pContractRisk.RiskValueShort)
                    {
                        pContractRisk.ConvertedRiskValueShort = (
                            from riskValues in pContractRisk.RiskValueShort
                            select new KeyValuePair<int, decimal>
                                (
                                riskValues.Key,
                                SpanRoundAmountCurrency(riskValues.Value, pContractRisk.Currency)
                                )
                            ).ToDictionary(p => p.Key, p => p.Value);
                    }
                    pContractRisk.ConvertedLongOptionValue = pContractRisk.LongOptionValue;
                    pContractRisk.ConvertedShortOptionValue = pContractRisk.ShortOptionValue;
                }
            }
            return pContractRisk;
        }

        /// <summary>
        /// Calcul du Delta Net par Future Maturity des positions reçues en paramètres
        /// </summary>
        /// <param name="pEvaluationData">Données de calcul</param>
        /// <param name="pAssetRisk">Valeur de risque de chaque série en position</param>
        /// PM 20151127 [21571][21605] Ajout override pour la méthode EvaluateFutureMonthDeltaRisk
        // PM 20180104 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //internal override void EvaluateFutureMonthDeltaRisk(List<SpanAssetRiskValues> pAssetRisk)
        internal override void EvaluateFutureMonthDeltaRisk(SpanEvaluationData pEvaluationData, List<SpanAssetRiskValues> pAssetRisk)
        {
            // Ensemble des Delta par Contrat et par échéance future avec recherche du Delta Scaling Factor
            // PM 20130605 [18730] Add AssetRisk
            // PM 20150902 [21385] Ajout jointure avec m_ContractParameters pour ajouter les membres ContractParameters et CappedWeighedRisk
            var assetFutureMonth =
                from asset in pAssetRisk
                join contractParam in m_ContractParameters on asset.SPANContractId equals contractParam.SPANContractId
                join maturityParam in pEvaluationData.FutureMonthParam on new { asset.SPANContractId, asset.FutureMaturity, asset.OptionMaturity } equals new { maturityParam.SPANContractId, maturityParam.FutureMaturity, maturityParam.OptionMaturity }
                select new
                {
                    ContractParameters = contractParam,
                    MaturityParameters = maturityParam,
                    DeltaLong = asset.Delta > 0 ? asset.Delta : 0,
                    DeltaShort = asset.Delta < 0 ? asset.Delta : 0,
                    AssetRisk = asset,
                    // PM 20151127 [21571][21605] Remplacement du calcule du CappedWeighedRisk classique par celui du London SPAN
                    //CappedWeighedRisk = maturityParam.FuturePriceScanRange / (maturityParam.DeltaScalingFactor * contractParam.DeltaScalingFactor),
                    CappedWeighedRisk = contractParam.ScanningRange * contractParam.MinPriceInc * contractParam.DeltaDen,
                };

            // Somme des Delta Long et des Delta Short par echeance
            // PM 20130605 [18730] Ajout paramètre this et liste des SpanAssetRiskValues
            // PM 20150902 [21385] Réécriture de la requête pour alimenter futureMonth
            //IEnumerable<SpanFutureMonthRisk> futureMonth =
            //    from assetMonth in assetFutureMonth
            //    join contractParam in m_ContractParameters on assetMonth.MaturityParameters.SPANContractId equals contractParam.SPANContractId
            //    join contractGroupParam in m_ContractGroupParameters on contractParam.SPANContractGroupId equals contractGroupParam.SPANContractGroupId
            //    group assetMonth by new { contractGroupParam, assetMonth.MaturityParameters.FutureMaturity } into futMonthD
            //    orderby futMonthD.Key.contractGroupParam.SPANContractGroupId, futMonthD.Key.FutureMaturity
            //    select new SpanFutureMonthRisk(
            //        this,
            //        futMonthD.Key.contractGroupParam,
            //        futMonthD.Key.FutureMaturity,
            //        futMonthD.First(f => SPANMaturityMonthToDateTime(f.MaturityParameters.FutureMaturity) == futMonthD.Min(m => SPANMaturityMonthToDateTime(m.MaturityParameters.FutureMaturity))).MaturityParameters,
            //        futMonthD.Sum(a => a.DeltaLong * a.MaturityParameters.DeltaScalingFactor),
            //        futMonthD.Sum(a => a.DeltaShort * a.MaturityParameters.DeltaScalingFactor),
            //        (from futM in futMonthD select futM.AssetRisk).ToList()
            //        )
            //    ;
            IEnumerable<SpanFutureMonthRisk> futureMonth =
                from assetMonth in assetFutureMonth
                join contractGroupParam in m_ContractGroupParameters on assetMonth.ContractParameters.SPANContractGroupId equals contractGroupParam.SPANContractGroupId
                group assetMonth by new { contractGroupParam, assetMonth.MaturityParameters.FutureMaturity } into futMonthD
                orderby futMonthD.Key.contractGroupParam.SPANContractGroupId, futMonthD.Key.FutureMaturity
                select new SpanFutureMonthRisk(
                    this,
                    pEvaluationData,
                    futMonthD.Key.contractGroupParam,
                    futMonthD.Key.FutureMaturity,
                    futMonthD.First(f => SPANMaturityMonthToDateTime(f.MaturityParameters.FutureMaturity) == futMonthD.Min(m => SPANMaturityMonthToDateTime(m.MaturityParameters.FutureMaturity))).MaturityParameters,
                    futMonthD.Sum(a => a.DeltaLong * a.MaturityParameters.DeltaScalingFactor * a.ContractParameters.DeltaScalingFactor),
                    futMonthD.Sum(a => a.DeltaShort * a.MaturityParameters.DeltaScalingFactor * a.ContractParameters.DeltaScalingFactor),
                    futMonthD.First().CappedWeighedRisk * futMonthD.Key.contractGroupParam.RiskMultiplier,
                    (from futM in futMonthD select futM.AssetRisk).ToList()
                    )
                ;

            pEvaluationData.FutureMonth = futureMonth.ToArray();
        }
        #endregion override base methods

        #region methods
        /// <summary>
        /// Arrondi un montant en fonction de la devise
        /// </summary>
        /// <param name="pAmount">Montant à arrondir</param>
        /// <param name="pCurrency">Devise du montant</param>
        /// <returns>Montant arrondi</returns>
        private decimal SpanRoundAmountCurrency(decimal pAmount, string pCurrency)
        {
            int roundingLevel = 1;
            // Rechercher la précision d'arrondie de la devise
            if (null != m_CurrencyParameters)
            {
                SpanCurrency currency = m_CurrencyParameters.FirstOrDefault(c => c.Currency == pCurrency);
                if (default(SpanCurrency) != currency)
                {
                    roundingLevel = (int)System.Math.Pow(10.0, (double)currency.Exponent);
                }
            }

            return SpanRoundAmount(pAmount / roundingLevel) * roundingLevel;
        }
        #endregion methods
    }
}

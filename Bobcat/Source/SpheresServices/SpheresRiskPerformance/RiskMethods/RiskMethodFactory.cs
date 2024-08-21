using EFS.Common;
using EFS.Process;

using EFS.Spheres.DataContracts;
using EFS.Spheres.Hierarchies;
using EFS.SpheresRiskPerformance.EOD;
using EfsML.Enum;

using System;
using System.Collections.Generic;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class factory gérant les jeux de méthode de calcul de risque
    /// </summary>
    /// PM 20160404 [22116] Classe déplacée à partir de RiskMethodBase.cs, et refactorée
    // EG 20190114 Add detail to ProcessLog Refactoring
    public sealed partial class RiskMethodFactory
    {
        #region Members
        /// <summary>
        /// Jeu de méthodes utilisées pour un CSS
        /// </summary>
        /// PM 20160404 [22116] Ajout
        private Dictionary<int, RiskMethodSet> m_MethodSet = new Dictionary<int, RiskMethodSet>();
        #endregion Members

        #region Accessors
        /// <summary>
        /// Fournit le jeu de méthodes utilisées pour un CSS (clé: IDCSS)
        /// </summary>
        /// PM 20160404 [22116] Ajout
        public Dictionary<int, RiskMethodSet> MethodSet
        {
            get { return m_MethodSet; }
        }
        #endregion Accessors

        #region Methods
        

        /// <summary>
        /// Supprime les jeux de méthodes de toutes les chambres
        /// </summary>
        /// PM 20160404 [22116] New
        public void Clear()
        {
            m_MethodSet.Clear();
        }

        /// <summary>
        /// Construction d'un jeu de méthodes pour une chambre
        /// </summary>
        /// <param name="pProcessInfo"></param>
        /// <param name="pIdCSS"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pDtMarket"></param>
        /// <param name="pEvaluationRepository"></param>
        /// <param name="pImRequestDiagnostics"></param>
        /// <param name="pEntityMarkets"></param>
        /// <param name="pActorsRoleMarginReqOffice"></param>
        /// <returns></returns>
        // PM 20160404 [22116] New
        // PM 20170313 [22833] Passer en paramètre un RiskRepository à la place du dictionnaire de SQL_AssetETD
        //public RiskMethodSet BuildMethodSet(string pCs, int pIdCSS, DateTime pDtBusiness, DateTime pDtMarket, int pIdEntity,
        //    SettlSessIDEnum pTiming, TimeSpan pRiskDataTime, string pSessionId, Dictionary<int, SQL_AssetETD> pAssetETDCache,
        //    IMRequestDiagnostics pImRequestDiagnostics, IEnumerable<EntityMarketWithCSS> pEntityMarkets, List<ActorNodeWithSpecificRoles> pActorsRoleMarginReqOffice)
        // PM 20180219 [23824] Utilisation de RiskPerformanceProcessInfo en paramètre
        //public RiskMethodSet BuildMethodSet(string pCs, int pIdCSS, DateTime pDtBusiness, DateTime pDtMarket, int pIdEntity,
        //    SettlSessIDEnum pTiming, TimeSpan pRiskDataTime, string pSessionId, RiskRepository pEvaluationRepository,
        //    IMRequestDiagnostics pImRequestDiagnostics, IEnumerable<EntityMarketWithCSS> pEntityMarkets, List<ActorNodeWithSpecificRoles> pActorsRoleMarginReqOffice)
        public RiskMethodSet BuildMethodSet(RiskPerformanceProcessInfo pProcessInfo, int pIdCSS, DateTime pDtBusiness, DateTime pDtMarket,
            RiskRepository pEvaluationRepository,  IMRequestDiagnostics pImRequestDiagnostics,
            IEnumerable<EntityMarketWithCSS> pEntityMarkets, List<ActorNodeWithSpecificRoles> pActorsRoleMarginReqOffice)
        {
            // PM 20180219 [23824] Utilisation nouveau constructeur
            //RiskMethodSet methodSet = new RiskMethodSet(pCs, pProcessInfo.pSessionId, pIdCSS, pDtBusiness, pDtMarket);
            RiskMethodSet methodSet = new RiskMethodSet(pProcessInfo, pIdCSS, pDtBusiness, pDtMarket);
            AddMethodSet(pIdCSS, methodSet);
            //
            //methodSet.BuildMethods(pIdEntity, pTiming, pImRequestDiagnostics, pRiskDataTime, pAssetETDCache, pEntityMarkets, pActorsRoleMarginReqOffice);
            // PM 20180219 [23824] Utilisation nouvelle méthode
            //methodSet.BuildMethods(pIdEntity, pTiming, pImRequestDiagnostics, pRiskDataTime, pEvaluationRepository, pEntityMarkets, pActorsRoleMarginReqOffice);
            methodSet.BuildMethods(pImRequestDiagnostics, pEvaluationRepository, pEntityMarkets, pActorsRoleMarginReqOffice);
            //
            return methodSet;
        }

        /// <summary>
        /// Construction d'un jeu de méthodes avec une seul méthode pour utilisation externe
        /// </summary>
        /// <param name="pProcessInfo"></param>
        /// <param name="pIdCSS"></param>
        /// <param name="pMethodType"></param>
        /// <param name="pImRequestDiagnostics"></param>
        /// <param name="pEvaluationRepository"></param>
        /// <param name="pEntityMarkets"></param>
        /// <returns></returns>
        /// PM 20160404 [22116] New
        // PM 20180219 [23824] Utilisation de RiskPerformanceProcessInfo en paramètre à la place de pCs, pDtBusiness, pDtMarket, pIdEntity, pTiming, pRiskDataTime et pSessionId
        //public RiskMethodSet BuildMethodSetExternal(string pCs, int pIdCSS, DateTime pDtBusiness, DateTime pDtMarket, int pIdEntity,
        //    InitialMarginMethodEnum pMethodType, SettlSessIDEnum pTiming, TimeSpan pRiskDataTime, string pSessionId,
        //    IMRequestDiagnostics pImRequestDiagnostics, Dictionary<int, SQL_AssetETD> pAssetETDCache, IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        // PM 20180918 [XXXXX] Suite test Prisma Eurosys : Suppression de pAssetETDCache et ajout de pEvaluationRepository. pAssetETDCache sera construit à partir de pEvaluationRepository
        //public RiskMethodSet BuildMethodSetExternal(RiskPerformanceProcessInfo pProcessInfo,  int pIdCSS,
        //    InitialMarginMethodEnum pMethodType, IMRequestDiagnostics pImRequestDiagnostics,
        //    Dictionary<int, SQL_AssetETD> pAssetETDCache, IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        public RiskMethodSet BuildMethodSetExternal(RiskPerformanceProcessInfo pProcessInfo,  int pIdCSS,
            InitialMarginMethodEnum pMethodType, IMRequestDiagnostics pImRequestDiagnostics,
            RiskRepository pEvaluationRepository, IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        {
            // PM 20180219 [23824] Utilisation nouveau constructeur
            //RiskMethodSet methodSet = new RiskMethodSet(pCs, pSessionId, pIdCSS, pDtBusiness, pDtMarket);
            RiskMethodSet methodSet = new RiskMethodSet(pProcessInfo, pIdCSS, pProcessInfo.DtBusiness, pProcessInfo.DtBusiness); 
            AddMethodSet(pIdCSS, methodSet);
            //
            // PM 20180219 [23824] Utilisation nouvelle méthode
            //methodSet.BuildMethodExternal(pIdEntity, pMethodType, pTiming, pImRequestDiagnostics, pRiskDataTime, pAssetETDCache, pEntityMarkets);
            // PM 20180918 [XXXXX] Suite test Prisma Eurosys : Suppression de pAssetETDCache et ajout de pEvaluationRepository. pAssetETDCache sera construit à partir de pEvaluationRepository
            //methodSet.BuildMethodExternal(pMethodType, pImRequestDiagnostics, pAssetETDCache, pEntityMarkets);
            methodSet.BuildMethodExternal(pMethodType, pImRequestDiagnostics, pEvaluationRepository, pEntityMarkets);
            //
            return methodSet;
        }

        /// <summary>
        /// Ajout d'un jeu de méthodes pour une chambre
        /// </summary>
        /// <param name="pIdCSS"></param>
        /// <param name="pMethodSet"></param>
        /// <returns>Le jeu de méthodes courant de la chambre</returns>
        /// PM 20160404 [22116] New
        private RiskMethodSet AddMethodSet(int pIdCSS, RiskMethodSet pMethodSet)
        {
            RiskMethodSet cssMethodSet = pMethodSet;
            if (pMethodSet != default(RiskMethodSet))
            {
                if (m_MethodSet == default(Dictionary<int, RiskMethodSet>))
                {
                    m_MethodSet = new Dictionary<int, RiskMethodSet>();
                }
                if (false == m_MethodSet.TryGetValue(pIdCSS, out cssMethodSet))
                {
                    m_MethodSet[pIdCSS] = pMethodSet;
                }
            }
            return cssMethodSet;
        }

        /// <summary>
        /// Evaluation des déposit pour une chambre
        /// </summary>
        /// <param name="pIdCss"></param>
        /// PM 20160404 [22116] New
        // EG 20180525 [23979] IRQ Processing
        public void Evaluate(ProcessBase pProcessBase, int pIdCss)
        {
            if (m_MethodSet.TryGetValue(pIdCss, out RiskMethodSet methodSet))
            {
                methodSet.Evaluate(pProcessBase);
            }
        }

        /// <summary>
        /// Calcul des montants de déposit pour une chambre avec la bonne pondération et arrondie en fonction de la devise
        /// </summary>
        /// <param name="pIdCss"></param>
        /// PM 20160404 [22116] New
        public void WeightingRatio(int pIdCss)
        {
            if (m_MethodSet.TryGetValue(pIdCss, out RiskMethodSet methodSet))
            {
                methodSet.WeightingRatio();
            }
        }

        /// <summary>
        /// Reset des collections de parameters de toutes les méthodes d'une chambre
        /// </summary>
        /// <param name="pIdCss"></param>
        /// PM 20160404 [22116] New
        public void ResetParameters(int pIdCss)
        {
            if (m_MethodSet.TryGetValue(pIdCss, out RiskMethodSet methodSet))
            {
                methodSet.ResetParameters();
            }
        }

        /// <summary>
        /// Ensemble des deposits d'une chambre, avec pour clé le couple actor/book
        /// </summary>
        /// <param name="pIdCss"></param>
        /// PM 20160404 [22116] New
        public SerializableDictionary<Pair<int, int>, Deposit> Deposits(int pIdCss)
        {
            SerializableDictionary<Pair<int, int>, Deposit> retDeposit;
            if (m_MethodSet.TryGetValue(pIdCss, out RiskMethodSet methodSet))
            {
                retDeposit = methodSet.Deposits;
            }
            else
            {
                retDeposit = new SerializableDictionary<Pair<int, int>, Deposit>();
            }
            return retDeposit;
        }
    }
    #endregion Methods
}

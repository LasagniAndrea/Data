using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Spheres.DataContracts;
using EFS.Spheres.Hierarchies;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.Enum;
using EFS.SpheresRiskPerformance.EOD;
using EFS.SpheresRiskPerformance.Hierarchies;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.v30.Fix;
//
using FixML.Enum;
//
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Classe gérant un jeu de méthodes de calcul de déposit
    /// </summary>
    /// PM 20160404 [22116] Nouvelle classe
    public sealed partial class RiskMethodSet
    {
        #region Members
        /// <summary>
        /// Connexion string
        /// </summary>
        private readonly string m_Cs;

        /// <summary>
        /// Id interne de la clearing house utilisant le jeu courant de méthodes
        /// </summary>
        private readonly int m_IdCSS;

        ///// <summary>
        ///// Session id du process de calcul
        ///// </summary>
        //// PM 20180219 [23824] utilisation de ProcessInfo et supression m_SessionId
        //private string m_SessionId;

        /// <summary>
        /// Business date en vigueur sur la clearing house
        /// </summary>
        private readonly DateTime m_DtBusiness;

        /// <summary>
        /// Market date la plus récente en vigueur sur la clearing house
        /// </summary>
        private readonly DateTime m_DtMarket;

        /// <summary>
        /// Ensemble des méthodes paramétrées utilisées par les positions présentes sur la clearing house
        /// </summary>
        private List<ImMethodParameter> m_IMMethodParameters = new List<ImMethodParameter>();

        /// <summary>
        /// Ensemble des asset par méthodes de calcul du jeu de méthodes
        /// </summary>
        //  PM 20200910 [25481] Changement de type
        //private Dictionary<ImMethodParameter, Dictionary<int, SQL_AssetETD>> m_AssetByIMMethod = new Dictionary<ImMethodParameter, Dictionary<int, SQL_AssetETD>>();
        private readonly Dictionary<ImMethodParameter, RiskAssetCache> m_AssetByIMMethod = new Dictionary<ImMethodParameter, RiskAssetCache>();

        /// <summary>
        /// Ensemble des méthodes de calcul du jeu de méthodes
        /// </summary>
        private readonly Dictionary<ImMethodParameter, BaseMethod> m_Methods = new Dictionary<ImMethodParameter, BaseMethod>();

        /// <summary>
        /// Parameters to compute the delivery deposit
        /// </summary>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        private List<AssetDeliveryParameter> m_ParametersDeliveries;

        /// <summary>
        /// Additional marginreqoffice actor parameters (the key is the actor internal id)
        /// </summary>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        private Dictionary<int, IEnumerable<MarginReqOfficeParameter>> m_MarginReqOfficeParameters;

        /// <summary>
        /// All the deposits to be generated and computed
        /// </summary>
        /// <remarks>the identifying key is the actor/book pair (the actor must own the book)</remarks>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        private readonly SerializableDictionary<Pair<int, int>, Deposit> m_Deposits = new SerializableDictionary<Pair<int, int>, Deposit>("ActorId_BookId", "Deposit", new PairComparer<int, int>());

        /// <summary>
        /// Devise du CSS
        /// </summary>
        /// PM 20170313 [22833] Ajout
        private string m_CssCurrency;

        /// <summary>
        /// Id interne de la tâche de lecture des RiskDatas
        /// </summary>
        /// PM 20180219 [23824] Ajout m_IdIOTaskRiskData
        private int m_IdIOTaskRiskData;

        /// <summary>
        /// Indique s'il faut lire les données de risque pour les jours fériés marché
        /// </summary>
        /// PM 20180530 [23824] Ajout m_IsRiskDataOnHoliday
        private bool m_IsRiskDataOnHoliday;

        /// <summary>
        /// Information sur le process
        /// </summary>
        // PM 20180219 [23824]  Ajout
        private RiskPerformanceProcessInfo m_ProcessInfo;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Id interne de la clearing house utilisant le jeu courant de méthodes
        /// </summary>
        public int IdCSS
        {
            get { return m_IdCSS; }
        }

       

        /// <summary>
        /// Business date en vigueur sur la clearing house
        /// </summary>
        public DateTime DtBusiness
        {
            get { return m_DtBusiness; }
        }

        /// <summary>
        /// Market date la plus récente en vigueur sur la clearing house
        /// </summary>
        public DateTime DtMarket
        {
            get { return m_DtMarket; }
        }

        /// <summary>
        /// Ensemble des méthodes de calcul du jeu de méthodes
        /// </summary>
        public Dictionary<ImMethodParameter, BaseMethod> Methods
        {
            get { return m_Methods; }
        }

        /// <summary>
        /// Ensemble des types de méthodes du jeu de méthodes
        /// </summary>
        public InitialMarginMethodEnum[] MethodsType
        {
            get { return m_Methods.Keys.Select(m => m.IMMethodEnum).Distinct().ToArray(); }
        }

        /// <summary>
        /// Get the additional marginreqoffice actor parameters (the key is the actor internal id)
        /// </summary>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        public Dictionary<int, IEnumerable<MarginReqOfficeParameter>> MarginReqOfficeParameters
        {
            get { return m_MarginReqOfficeParameters; }
            set { m_MarginReqOfficeParameters = value; }
        }

        /// <summary>
        /// Deposits dictionary, research key by actor/book
        /// </summary>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        public SerializableDictionary<Pair<int, int>, Deposit> Deposits
        {
            get { return m_Deposits; }
        }

        /// <summary>
        /// class containing the diagnostic elements (in order to set their start time/en time/trade deposit properties)
        /// </summary>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        public IMRequestDiagnostics ImRequestDiagnostics
        { get; set; }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pProcessInfo"></param>
        /// <param name="pIdCSS"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pDtMarket"></param>
        // PM 20180219 [23824] Utilisation du paramètre RiskPerformanceProcessInfo
        //public RiskMethodSet(string pCs, string pSessionId, int pIdCSS, DateTime pDtBusiness, DateTime pDtMarket)
        public RiskMethodSet(RiskPerformanceProcessInfo pProcessInfo, int pIdCSS, DateTime pDtBusiness, DateTime pDtMarket)
        {
            m_ProcessInfo = pProcessInfo;
            m_Cs = pProcessInfo.Process.Cs;
            m_IdCSS = pIdCSS;
            m_DtBusiness = pDtBusiness;
            m_DtMarket = pDtMarket;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Charge les paramètres généraux et type des méthodes de calcul
        /// </summary>
        // EG 20180803 PERF Suppresion SESSIONID
        private void LoadMethodsParameters()
        {
            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>();

            using (IDbConnection connection = DataHelper.OpenConnection(m_Cs))
            {
                // PM 20170313 [22833] Ajout paramètre IDA_CSS
                dbParametersValue.Add("DTBUSINESS", DtBusiness);
                dbParametersValue.Add("IDA_CSS", IdCSS);
                // IMMETHODPARAMETER
                m_IMMethodParameters = LoadParametersMethod<ImMethodParameter>.LoadParameters(connection, dbParametersValue, DataContractResultSets.IMMETHODPARAMETER);
            }
        }

        /// <summary>
        /// Construction de toutes les méthodes de calcul du jeu de méthodes
        /// </summary>
        /// <param name="pImRequestDiagnostics"></param>
        /// <param name="pEvaluationRepository">Ensemble des données de risque à évaluer</param>
        /// <param name="pEntityMarkets"></param>
        /// <param name="pActorsRoleMarginReqOffice"></param>
        /// PM 20170313 [22833] Passer en paramètre un RiskRepository à la place du dictionnaire de SQL_AssetETD
        //public void BuildMethods(int pIdEntity, SettlSessIDEnum pTiming, IMRequestDiagnostics pImRequestDiagnostics, TimeSpan pRiskDataTime,
        //    Dictionary<int, SQL_AssetETD> pAssetETDCache, IEnumerable<EntityMarketWithCSS> pEntityMarkets, List<ActorNodeWithSpecificRoles> pActorsRoleMarginReqOffice)
        // PM 20180219 [23824] Retrait des paramètre pIdEntity, pTiming et pRiskDataTime
        //public void BuildMethods(int pIdEntity, SettlSessIDEnum pTiming, IMRequestDiagnostics pImRequestDiagnostics, TimeSpan pRiskDataTime,
        //    RiskRepository pEvaluationRepository, IEnumerable<EntityMarketWithCSS> pEntityMarkets, List<ActorNodeWithSpecificRoles> pActorsRoleMarginReqOffice)
        public void BuildMethods(IMRequestDiagnostics pImRequestDiagnostics, RiskRepository pEvaluationRepository,
            IEnumerable<EntityMarketWithCSS> pEntityMarkets, List<ActorNodeWithSpecificRoles> pActorsRoleMarginReqOffice)
        {
            ImRequestDiagnostics = pImRequestDiagnostics;

            // Initialiser les paramètres pour les MRO
            BuildMarginReqOfficeParameters(pActorsRoleMarginReqOffice);

            // Charger les paramètres généraux et type des méthodes de calcul
            LoadMethodsParameters();

            if ((m_IMMethodParameters != null) && (m_IMMethodParameters.Count > 0))
            {
                // PM 20170313 [22833] Ajout lecture de la devise du CSS
                EntityMarketWithCSS firstEM = pEntityMarkets.First(elem => elem.CssId == this.IdCSS);
                // La devise peut être null car ce n'est pas un paramètre obligatoire
                m_CssCurrency = firstEM.CssCurrency;

                // PM 20180219 [23824] Renseigner m_IdIOTaskRiskData
                m_IdIOTaskRiskData = firstEM.IdIOTaskRiskData;

                // PM 20180530 [23824] Renseigner m_IdIOTaskRiskData
                m_IsRiskDataOnHoliday = firstEM.IsRiskDataOnHoliday;

                // PM 20200910 [25481] Ajout pour gérer à la fois les assets ETD et les assets COM
                RiskAssetCache allAssetCache = new RiskAssetCache(pEvaluationRepository.EvaluationData.AssetETDCache, pEvaluationRepository.EvaluationData.AssetCOMCache);

                // PM 20240524 [WI947] Ajout gestion de la license
                License license = License.Load(m_Cs, Software.SOFTWARE_Spheres);

                // Initialiser les paramètres de chaque méthode
                foreach (ImMethodParameter imMethod in m_IMMethodParameters)
                {
                    if (license.IsLicInitialMarginMethodAuthorised(imMethod.IMMethodEnum))
                    {
                        // Construire la méthode
                        //BaseMethod method = BuildMethod(pIdEntity, imMethod.IMMethodEnum, pTiming, pImRequestDiagnostics, pRiskDataTime, imMethod);
                        BaseMethod method = BuildMethod(imMethod.IMMethodEnum, pImRequestDiagnostics, imMethod);
                        // Ajouter la méthode au jeu de méthodes
                        m_Methods.Add(imMethod, method);

                        //// PM 20170313 [22833] Recherché les assets concernés par la méthode uniquement pour les méthodes basées sur la position
                        //Dictionary<int, SQL_AssetETD> assetCache;
                        //if (method.RiskMethodDataType == RiskMethodDataTypeEnum.Position)
                        //{
                        //    // Construire la liste des IdAsset concernés par la méthode
                        //    assetCache = (
                        //        // PM 20170313 [22833] Utilisation du RiskRepository à la place du dictionnaire de SQL_AssetETD
                        //        //from asset in pAssetETDCache
                        //        from asset in pEvaluationRepository.EvaluationData.AssetETDCache
                        //        where asset.Value.IdImMethod == imMethod.IdIMMethod
                        //        select asset).ToDictionary(a => a.Key, a => a.Value);
                        //}
                        //else
                        //{
                        //    assetCache = default(Dictionary<int, SQL_AssetETD>);
                        //}

                        // PM 20200910 [25481] Bloc ci-dessus remplacé par la nouvelle méthode AssetCacheFromMethod
                        RiskAssetCache assetCache = allAssetCache.AssetCacheFromMethod(imMethod.IdIMMethod);

                        // Ajouter liste des IdAsset pour la méthode
                        m_AssetByIMMethod.Add(imMethod, assetCache);

                        // Initialiser les paramètres pour les marchés
                        method.BuildMarketParameters(pEntityMarkets);

                        // Initialiser les paramètres pour les MRO
                        method.MarginReqOfficeParameters = MarginReqOfficeParameters;

                        // Initialiser les paramètres de risque
                        method.LoadParameters(m_Cs, assetCache);
                    }
                    else
                    {
                        // Pas de licence pour la méthode
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 1040), 0, new LogParam(imMethod.Identifier)));
                    }
                }
            }
            // PM 20170313 [22833] Utilisation du RiskRepository à la place du dictionnaire de SQL_AssetETD
            //else if (pAssetETDCache.Count > 0)
            else if ((pEvaluationRepository.EvaluationData.AssetETDCache.Count > 0) || (pEvaluationRepository.EvaluationData.AssetCOMCache.Count > 0))
            {
                string cssIdentifier = Cst.NotAvailable;
                if (pEntityMarkets != null)
                {
                    cssIdentifier = pEntityMarkets.FirstOrDefault(em => em.CssId == m_IdCSS).Css;
                    if (cssIdentifier == null)
                    {
                        cssIdentifier = Cst.NotAvailable;
                    }
                }
                // Méthode calcul non trouvée alors que des assets sont en position
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-01000", LogTools.IdentifierAndId(cssIdentifier, m_IdCSS));
            }
            // PM 20170313 [22833] Utilisation du RiskRepository à la place du dictionnaire de SQL_AssetETD
            //if (pAssetETDCache.Count > 0)
            if (pEvaluationRepository.EvaluationData.AssetETDCache.Count > 0)
            {
                using (IDbConnection connection = DataHelper.OpenConnection(this.m_Cs))
                {
                    Dictionary<string, object> dbParametersValue = new Dictionary<string, object>
                    {
                        { "DTBUSINESS", DtBusiness.Date }
                    };

                    // PM 20170313 [22833] Utilisation du RiskRepository à la place du dictionnaire de SQL_AssetETD
                    //LoadParametersDeliveries(pAssetETDCache, connection, dbParametersValue, this.m_Cs);
                    LoadParametersDeliveries(connection, dbParametersValue, this.m_Cs);
                }
            }
            else
            {
                m_ParametersDeliveries = new List<AssetDeliveryParameter>();
            }
        }

        /// <summary>
        ///  Construction d'un jeu de méthodes avec une seul méthode pour utilisation externe
        /// </summary>
        /// <param name="pMethodType"></param>
        /// <param name="pImRequestDiagnostics"></param>
        /// <param name="pEvaluationRepository"></param>
        /// <param name="pEntityMarkets"></param>
        // PM 20180219 [23824] Supression des paramètres pIdEntity, pTiming et pRiskDataTime
        //public void BuildMethodExternal(int pIdEntity, InitialMarginMethodEnum pMethodType, SettlSessIDEnum pTiming,
        //    IMRequestDiagnostics pImRequestDiagnostics, TimeSpan pRiskDataTime, Dictionary<int, SQL_AssetETD> pAssetETDCache, IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        // PM 20180918 [XXXXX] Suite test Prisma Eurosys : Suppression de pAssetETDCache et ajout de pEvaluationRepository. pAssetETDCache sera construit à partir de pEvaluationRepository
        //public void BuildMethodExternal(InitialMarginMethodEnum pMethodType, IMRequestDiagnostics pImRequestDiagnostics,
        //    Dictionary<int, SQL_AssetETD> pAssetETDCache, IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        public void BuildMethodExternal(InitialMarginMethodEnum pMethodType, IMRequestDiagnostics pImRequestDiagnostics,
             RiskRepository pEvaluationRepository, IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        {
            // PM 20180918 [XXXXX] Ajout alimentation des membres suite test Prisma Eurosys
            EntityMarketWithCSS firstEM = pEntityMarkets.First(elem => elem.CssId == this.IdCSS);
            m_CssCurrency = firstEM.CssCurrency;
            m_IdIOTaskRiskData = firstEM.IdIOTaskRiskData;
            m_IsRiskDataOnHoliday = firstEM.IsRiskDataOnHoliday;

            // PM 20180918 [XXXXX] Ajout alimentation de ImRequestDiagnostics suite test Prisma Eurosys
            ImRequestDiagnostics = pImRequestDiagnostics;

            // ImMethodParameter vide pour ajouter au jeu de méthodes ayant une seule méthode
            ImMethodParameter imMethod = new ImMethodParameter
            {
                // PM 20180918 [XXXXX] Ajout alimentation de imMethod suite test Prisma Eurosys
                IdIMMethod = 0,
                IMMethodEnum = pMethodType,
                MethodVersionDouble = 5
            };

            // Construire la méthode
            // PM 20180219 [23824] Utilisation nouvelle méthode
            //BaseMethod method = BuildMethod(pIdEntity, pMethodType, pTiming, pImRequestDiagnostics, pRiskDataTime, imMethod);
            BaseMethod method = BuildMethod(pMethodType, pImRequestDiagnostics, imMethod);
            // Ajouter la méthode au jeu de méthodes
            m_Methods.Add(imMethod, method);

            // PM 20180918 [XXXXX] Ajout de l'utisation de AssetETDCache suite test Prisma Eurosys
            Dictionary<int, SQL_AssetETD> assetCache = pEvaluationRepository.EvaluationData.AssetETDCache;

            // Ajouter liste des IdAsset pour la méthode
            // PM 20180918 [XXXXX] Ajout alimentation de m_AssetByIMMethod suite test Prisma Eurosys
            // PM 20200910 [25481] modification de type de m_AssetByIMMethod
            //m_AssetByIMMethod.Add(imMethod, assetCache);
            RiskAssetCache riskAssetCache = new RiskAssetCache(assetCache, default);
            m_AssetByIMMethod.Add(imMethod, riskAssetCache);

            // Initialiser les paramètres pour les marchés
            method.BuildMarketParameters(pEntityMarkets);

            // Initialiser les paramètres pour les MRO
            method.MarginReqOfficeParameters = MarginReqOfficeParameters;

            // Chargement des paramètres spécifique de la méthode
            //method.LoadParameters(m_Cs, pAssetETDCache);
            // PM 20200910 [25481] modification de signature de la méthode LoadParameters
            //method.LoadParameters(m_Cs, assetCache);
            method.LoadParameters(m_Cs, riskAssetCache);
        }

        /// <summary>
        /// Reset des collections de parameters de chaque méthode
        /// </summary>
        public void ResetParameters()
        {
            foreach (BaseMethod method in m_Methods.Values)
            {
                method.ResetParameters();
            }
            m_ParametersDeliveries = null;
            m_MarginReqOfficeParameters = null;
        }

        /// <summary>
        /// Initialise la collection de Deposit
        /// </summary>
        /// <param name="pActorsRoleMarginReqOffice">Ensemble des acteurs MARGINREQOFFICE actors process list</param>
        /// <param name="pTiming">process timing of the evaluation as requested by the user</param>
        /// <returns>list of deposits previously evaluated that we did not find "in position" anymore. No position is attached to them</returns>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        public IEnumerable<Deposit> InitializeDeposits(IEnumerable<ActorNodeWithSpecificRoles> pActorsRoleMarginReqOffice, SettlSessIDEnum pTiming)
        {
            Deposits.Clear();

            var depositInputDatas =
               from actor in pActorsRoleMarginReqOffice
               from attribute in actor.RoleSpecificAttributes
               where attribute is RoleMarginReqOfficeAttribute
               from rootElement in ((RoleMarginReqOfficeAttribute)attribute).RootElements
               select new
               {
                   Root = rootElement,
                   IsGrossMargining =
                        // an additional deposit (or elementary) is to consider as Net ever
                        rootElement.RiskElementClass != RiskElementClass.ADDITIONALDEPOSIT
                        && ((RoleMarginReqOfficeAttribute)attribute).IsGrossMargining,
                   ActorsBooksInPosition =
                       ((RoleMarginReqOfficeAttribute)attribute).
                           GetPairsActorBookConstitutingPosition(rootElement.RiskElementClass, rootElement.AffectedBookId),
                   RiskElements =
                       ((RoleMarginReqOfficeAttribute)attribute).
                           GetCalculationElements(rootElement.RiskElementClass, rootElement.AffectedBookId),
                   Result =
                    ((RoleMarginReqOfficeAttribute)attribute).
                        GetPreviousTradeRisk(this.IdCSS, rootElement.AffectedBookId, pTiming),
                   HierarchyClass =
                        actor.IsDescendingFromClearer ? DepositHierarchyClass.CLEARER : DepositHierarchyClass.ENTITY,
                   WeightingRatio =
                        FindSpecificWeightingRatio(attribute.IdNode, ((RoleMarginReqOfficeAttribute)attribute).WeightingRatio),
                   // TODO 20110516 MF la collection ancestors est vide pour le moment...
                   Ancestors = new List<Pair<int, int>>()

               };

            foreach (var inputDatas in depositInputDatas)
            {
                Deposit deposit = new Deposit(
                    inputDatas.Root,
                    inputDatas.IsGrossMargining,
                    inputDatas.RiskElements,
                    inputDatas.ActorsBooksInPosition,
                    inputDatas.HierarchyClass,
                    inputDatas.Result,
                    inputDatas.WeightingRatio,
                    inputDatas.Ancestors);

                Pair<int, int> pairToAdd = new Pair<int, int>(inputDatas.Root.ActorId, inputDatas.Root.AffectedBookId);

                // 20120625 MF Check equal deposits and exclude them from the repository
                if (!this.Deposits.ContainsKey(pairToAdd))
                {
                    this.Deposits.Add(
                        pairToAdd,
                        deposit);
                }
            }

            // post-process the deposit collection to supprime the deposit not in position

            IEnumerable<KeyValuePair<Pair<int, int>, Deposit>> depositsToDelete =
                from deposit in this.Deposits
                where !this.IsInPosition(deposit.Value)
                select deposit;

            Pair<int, int>[] keyDepositsToDelete = (
                from deposit in depositsToDelete select deposit.Key
                ).ToArray();

            foreach (Pair<int, int> key in keyDepositsToDelete)
            {
                this.Deposits.Remove(key);
            }

            // PM 20160404 [22116] Initialiser les déposit de chaque méthode (pour compatibilité avec existant)
            foreach (BaseMethod method in m_Methods.Values)
            {
                method.Deposits = Deposits;
            }

            return from deposit in depositsToDelete select deposit.Value;
        }

        /// <summary>
        /// Get complementary information about the rolemarginreqoffice actors concerned by the evaluation process.
        /// This method transform elements type of <see cref="EFS.SpheresRiskPerformance.DataContracts.ClearingOrgParameter"/> 
        /// and <see cref="EFS.SpheresRiskPerformance.DataContracts.EquityMarketParameter"/> owned by actors marginreqoffice
        /// in elements type of <see cref="EFS.SpheresRiskPerformance.RiskMethods.MarginReqOfficeParameter"/> for any kind of risk method. 
        /// These elements are grouped by actor. Some margin req office parameters related to the same element (a market, or a clearing house)
        /// could appear twice or more inside the group because of some dirty or duplicated referential values, only the first will be considered
        ///.
        /// </summary>
        /// <param name="pActorsRoleMarginReqOffice">all the marginreqoffice actors we are building at least one deposit for</param>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        public void BuildMarginReqOfficeParameters(List<ActorNodeWithSpecificRoles> pActorsRoleMarginReqOffice)
        {
            IEnumerable<RoleMarginReqOfficeAttribute> attributes =
                from actor in pActorsRoleMarginReqOffice
                from attribute in actor.RoleSpecificAttributes
                where attribute is RoleMarginReqOfficeAttribute
                select (RoleMarginReqOfficeAttribute)attribute;

            // PM 20150930 [21134] Add ScanRiskOffsetCapPrct
            // PM 20170106 [22633] Add IsInitialMarginForExeAssPosition
            m_MarginReqOfficeParameters = (
                from attribute in attributes
                from equityMarketParameter in attribute.EquityMarketParameters
                where DataContractHelper.GetDataContractElementEnabled(equityMarketParameter, this.DtBusiness)
                select new MarginReqOfficeParameter
                {
                    ActorId = attribute.IdNode,
                    BookId = 0,
                    CssId = 0,
                    MarketId = equityMarketParameter.MarketId,
                    StockCoverageType = equityMarketParameter.StockCoverType,
                }
                ).Union(
                from attribute in attributes
                from clearingOrgParameter in attribute.ClearingOrgParameters
                where DataContractHelper.GetDataContractElementEnabled(clearingOrgParameter, this.DtBusiness)
                select new MarginReqOfficeParameter
                {
                    ActorId = attribute.IdNode,
                    BookId = 0,
                    CssId = clearingOrgParameter.CssId,
                    WeightingRatio = clearingOrgParameter.WeightingRatio,
                    SpanAccountType = clearingOrgParameter.SpanAccountType,
                    SpanMaintenanceAmountIndicator = clearingOrgParameter.SpanMaintenanceAmountIndicator,
                    ScanRiskOffsetCapPrct = clearingOrgParameter.ScanRiskOffsetCapPrct,
                    IsInitialMarginForExeAssPosition = clearingOrgParameter.IsInitialMarginForExeAssPosition,
                })
                .GroupBy(elem => elem.ActorId)
                .ToDictionary(groupedElem => groupedElem.Key, groupedElem => from elem in groupedElem select elem);
        }

        /// <summary>
        /// Main evaluation loop on all the built deposit items
        /// </summary>
        /// <returns></returns>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        // EG 20180525 [23979] IRQ Processing
        public Cst.ErrLevel Evaluate(ProcessBase pProcessBase)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            foreach (KeyValuePair<Pair<int, int>, Deposit> keyValue in this.Deposits)
            {
                if (IRQTools.IsIRQRequested(pProcessBase, pProcessBase.IRQNamedSystemSemaphore, ref ret))
                    break;
                // FI 20200818 [XXXXX] Usage de ImRequestDiagnostics.GetDate()
                DateTime Start = ImRequestDiagnostics.GetDate();
                EvaluateDeposit(keyValue.Key.First, keyValue.Key.Second, keyValue.Value);
                // FI 20200818 [XXXXX] Usage de ImRequestDiagnostics.GetDate()
                DateTime End = ImRequestDiagnostics.GetDate();
                this.ImRequestDiagnostics.SetStartEndTime(this.IdCSS, keyValue.Key.First, keyValue.Key.Second, Start, End);
            }
            return ret;

        }

        /// <summary>
        /// Evaluation d'un Deposit
        /// </summary>
        /// <param name="pActorId">L'acteur possédant la position</param>
        /// <param name="pBookId">Le book sur lequel la position a été affecté</param>
        /// <param name="pRiskDataToEvaluate">Les ddonnées de risque à évaluer</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="opMethodComObjs">output value containing all the calculation datas 
        /// to pass to the calculation sheet repository object
        /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) in order to
        /// build a margin calculation node (<see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
        /// and <see cref="EfsML.Interface.IMarginCalculationMethod"/>)</param>
        /// <returns>the partial amount for the current deposit </returns>
        /// PM 20160404 [22116] Déplacé à partir de RiskMethodBase et ajout gestion des multiples méthodes
        /// FI 20160613 [22256] Modify (add parameter pDepositHierarchyClass)
        /// PM 20170313 [22833] Changement de type pour le paramètre pPositionsToEvaluate => pRiskDataToEvaluate
        //public List<Money> EvaluateRiskElement(int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
        //    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
        //    out IMarginCalculationMethodCommunicationObject[] opMethodComObjs)
        // EG 20180307 [23769] Gestion Asynchrone
        public List<Money> EvaluateRiskElement(int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass, RiskData pRiskDataToEvaluate,
            out IMarginCalculationMethodCommunicationObject[] opMethodComObjs)
        {
            // Ensemble des objets de communication
            List<IMarginCalculationMethodCommunicationObject> methodComObjs = new List<IMarginCalculationMethodCommunicationObject>();
            // Ensemble des montants de déposit calculés
            List<Money> allAmounts = new List<Money>();

            // Objets de communication et montants pour chaque méthode
            IMarginCalculationMethodCommunicationObject opComObj;
            List<Money> amounts;
            try
            {
                // Evaluer le déposit pour chaque méthode
                foreach (KeyValuePair<ImMethodParameter, BaseMethod> imMethod in Methods)
                {
                    opComObj = default;

                    // Rechercher les assets concernés par la méthode
                    if (m_AssetByIMMethod.TryGetValue(imMethod.Key, out RiskAssetCache assetCache))
                    {
                        BaseMethod method = imMethod.Value;
                        // EG 20180307 [23769] Gestion Asynchrone
                        method.IsCalculationAsync = m_IsCalculationAsync;
                        // EG 20180205 [23769] New
                        method.SemaphoreDeposit = m_SemaphoreDeposit;

                        // Prendre la position sur les assets concernés par la méthode
                        // PM 20200910 [25481] Le filtrage est maintenant entièrement réalisé dans la méthode GetAssetRiskData
                        // PM 20170313 [22833] Filtrer la position lorsque la méthode s'applique sur un ensemble d'asset ETD
                        //List<Pair<PosRiskMarginKey, RiskMarginPosition>> methodPositionsToEvaluate = (
                        //    from position in pPositionsToEvaluate
                        //    join idAsset in assetCache.Keys on position.First.idAsset equals idAsset
                        //    select position).ToList();
                        //
                        //RiskData filteredRiskData;
                        //if (assetCache != default(Dictionary<int, SQL_AssetETD>))
                        //{
                        //    filteredRiskData = pRiskDataToEvaluate.GetAssetRiskData(assetCache.Keys);
                        //}
                        //else
                        //{
                        //    // Pas d'asset ETD, donc pas de position ETD
                        //    filteredRiskData = new RiskData(default(RiskDataPosition), pRiskDataToEvaluate.TradeValueCOM);
                        //}
                        RiskData filteredRiskData = pRiskDataToEvaluate.GetAssetRiskData(assetCache);

                        // PM 20170106 [22633] Gestion du non calcul de déposit sur position en attente de livraison
                        if (MarginReqOfficeParameters.ContainsKey(pActorId))
                        {
                            MarginReqOfficeParameter specificClearingHouseMROParam =
                                      (from parameter in MarginReqOfficeParameters[pActorId]
                                       where parameter.CssId == this.IdCSS
                                       select parameter)
                                      .FirstOrDefault();

                            if ((specificClearingHouseMROParam != default(MarginReqOfficeParameter)) && (false == specificClearingHouseMROParam.IsInitialMarginForExeAssPosition))
                            {
                                // PM 20170313 [22833]  Changement de type de methodPositionsToEvaluate => filteredRiskData
                                //foreach ( Pair<PosRiskMarginKey, RiskMarginPosition> posKey in methodPositionsToEvaluate)
                                //{
                                //    RiskMarginPosition pos = posKey.Second;
                                //    pos.ExeAssQuantity = 0;
                                //    posKey.Second = pos;
                                //}
                                filteredRiskData.ClearPosExeAss();
                            }
                        }

                        // PM 20170313 [22833] Vérifier qu'il y ait bien des données à évaluer
                        // PM 20200910 [25481] Ajout test pour les méthodes RiskMethodDataTypeEnum.TradeValue 
                        // PM 20221212 [XXXXX] Ajout test pour la méthode RiskMethodDataTypeEnum.TradeNoMargin 
                        if ((filteredRiskData.Count() > 0)
                            && (((method.RiskMethodDataType == RiskMethodDataTypeEnum.Position) && (filteredRiskData.PositionETD.Count() > 0))
                             || ((method.RiskMethodDataType == RiskMethodDataTypeEnum.TradeValue) && (filteredRiskData.TradeValueCOM.Count() > 0))
                             || ((method.RiskMethodDataType == RiskMethodDataTypeEnum.TradeNoMargin) && (filteredRiskData.TradeNoMargin.Count() > 0))
                             ))
                        {
                            // Evaluation du Deposit et construction de l'objet de communication pour chaque méthode
                            // PM 20170313 [22833] Changement de type pour le paramètre methodPositionsToEvaluate
                            //amounts = method.EvaluateRiskElementSpecific(pActorId, pBookId, pDepositHierarchyClass, methodPositionsToEvaluate, out opComObj);
                            amounts = method.EvaluateRiskElementSpecific(pActorId, pBookId, pDepositHierarchyClass, filteredRiskData, out opComObj);
                            opComObj.MarginAmounts = amounts.ToArray();

                            // PM 20200910 [25481] Ajout CalculationMethodType
                            opComObj.MarginMethodType = imMethod.Key.IMMethodEnum;
                            // PM 20230817 [XXXXX] Ajout MethodVersion
                            opComObj.MethodVersion = imMethod.Key.MethodVersion;

                            // PM 20170313 [22833] L'ajout de opComObj est déplacé plus bas pour avoir un élément pour chaque méthode
                            //// Ajout de l'objet de communication à la liste des objets de communications
                            //methodComObjs.Add(opComObj);

                            // Ajout des montants calculés à ceux des autres méthodes
                            BaseMethod.SumAmounts(amounts, ref allAmounts);

                            // Ajout de l'objet de communication à la liste des objets de communications
                            // PM 20200910 [25481] Déplacement de l'ajout de l'objet de communication pour le faire uniquement si l'appel au calcul à eu lieu
                            methodComObjs.Add(opComObj);
                        }
                    }
                }

                // Evaluation du deposit de livraison et construction de son objet de communication
                // PM 20170313 [22833] pPositionsToEvaluate => pRiskDataToEvaluate, et uniquement s'il existe des positions
                //amounts = EvaluateRiskElementsDeliveries(pActorId, pBookId, pPositionsToEvaluate, out opComObj);
                if (pRiskDataToEvaluate.PositionETD.Count() > 0)
                {
                    amounts = EvaluateRiskElementsDeliveries(pRiskDataToEvaluate.PositionETD, out opComObj);
                    opComObj.MarginAmounts = amounts.ToArray();
                    // PM 20200910 [25481] Ajout CalculationMethodType
                    opComObj.MarginMethodType = InitialMarginMethodEnum.Delivery;

                    // Ajout de l'objet de communication à la liste des objets de communications
                    methodComObjs.Add(opComObj);
                    // Ajout des montants de livraison calculés à ceux des méthodes
                    BaseMethod.SumAmounts(amounts, ref allAmounts);
                }

                // Affectation de l'ensemble des objets de communication en sorti
                opMethodComObjs = methodComObjs.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"An exception occured on ActorId: {pActorId} bookId: {pBookId}. Please contact EFS.", ex);
            }

            return allAmounts;
        }

        /// <summary>
        /// Calcul des montants de déposit avec la bonne pondération et arrondie en fonction de la devise
        /// </summary>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase et Suppression du paramètre pCs => utilisation de m_Cs
        //public void WeightingRatio(string pCs)
        public void WeightingRatio()
        {
            // Construction d'un dictionnaire avec les informations sur les arrondis des devises des montants de déposit calculés
            Dictionary<string, CurrencyCashInfo> curInfoDic = (
                from dep in Deposits
                from amount in dep.Value.Amounts
                where StrFunc.IsFilled(amount.Currency)
                select amount.Currency
                ).Distinct().ToDictionary(c => c, c => new CurrencyCashInfo(m_Cs, c));
            CurrencyCashInfo curInfo = default;
            //
            foreach (KeyValuePair<Pair<int, int>, Deposit> keyValue in Deposits)
            {
                List<Money> weightingAmounts = default;
                BaseMethod.SumAmounts(keyValue.Value.Amounts, ref weightingAmounts);
                //
                // Appliquer la pondération et arrondir les montants de déposits pondérés
                foreach (Money amount in weightingAmounts)
                {
                    amount.Amount.DecValue *= keyValue.Value.WeightingRatio;
                    if (curInfoDic.TryGetValue(amount.Currency, out curInfo))
                    {
                        EFS_Cash cash = new EFS_Cash(m_Cs, amount.Amount.DecValue, curInfo);
                        amount.Amount.DecValue = cash.AmountRounded;
                    }
                }
                keyValue.Value.WeightingRatioAmounts = weightingAmounts;
            }
        }

        /// <summary>
        /// Indique si un Deposit contient des positions
        /// </summary>
        /// <returns>"true" lorsque l'acteur du Deposit en gestion Nette possède une position,
        /// ou lorsque l'acteur du Deposit en gestion Brute depends d'au moins un acteur possèdant une postion.</returns>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        private bool IsInPosition(Deposit pDeposit)
        {
            // Vérification qu'il y a une position directement sur le Deposit
            // PM 20170313 [22833] Gestion position et tradevalue
            //bool isInPosition = PositionsExtractor.GetPositions(pDeposit.Factors).Count() > 0;
            RiskData riskData = RiskElement.AggregateRiskData(pDeposit.Factors);
            bool isInPosition = (riskData.Count() > 0);

            // Si le Deposit est en gestion Brute et n'a pas de directement de une position
            if (pDeposit.IsGrossMargining && (false == isInPosition))
            {
                // Un Deposit évalué en gestion Brute est valide si:
                // . soit il possède une position
                // . soit il dépends de sous-éléments possédant une position

                // Recherche des sous-éléments
                var subDeposits = (
                    from factor in pDeposit.Factors
                    where (factor.RiskElementClass == RiskElementClass.ELEMENT)
                    join deposit in Deposits on
                            new
                            {
                                IdA = factor.ActorId,
                                IdB = factor.AffectedBookId
                            } equals
                            new
                            {
                                IdA = deposit.Key.First,
                                IdB = deposit.Key.Second
                            }
                    select new
                    {
                        Deposit = deposit.Value,
                        Factor = factor
                    }
                ).ToArray();

                // Parcours des sous-éléments
                foreach (var pairDepFactor in subDeposits)
                {
                    if (this.IsInPosition(pairDepFactor.Deposit))
                    {
                        // Le sous-élément posséde une position
                        isInPosition = true;
                    }
                    else
                    {
                        // Le sous-élément ne posséde pas de position : inutile de le garder comme sous-élément
                        pDeposit.Factors.Remove(pairDepFactor.Factor);
                    }
                }
            }
            // Traitement du cas d'un Deposit plus en position mais l'ayant été lors d'un précédant traitement
            if (false == isInPosition)
            {
                // Le Deposit a-t-il précédement été évalué
                if ((pDeposit.PrevResult != null) && (pDeposit.PrevResult.ReEvaluation == RiskRevaluationMode.EvaluateWithUpdate))
                {
                    // Un Deposit déja évalué lors d'un précédant traitement, bien que n'ayant plus de position renvérra tout de même toujours "true"
                    isInPosition = true;
                    pDeposit.NotInPosition = true;
                }
            }
            return isInPosition;
        }

        /// <summary>
        /// Evaluation d'un Deposit
        /// </summary>
        /// <param name="pActorId"></param>
        /// <param name="pBookId"></param>
        /// <param name="pDepositToEvaluate"></param>
        /// <returns></returns>
        /// PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        /// FI 20160613 [22256] Modify
        private List<Money> EvaluateDeposit(int pActorId, int pBookId, Deposit pDepositToEvaluate)
        {
            // Si Deposit déjà évalué, ne pas réévaluer et retourner directement les montants
            if (pDepositToEvaluate.Status != DepositStatus.EVALUATED)
            {
                pDepositToEvaluate.Status = DepositStatus.EVALUATING;
                IMarginCalculationMethodCommunicationObject[] calcMethodComObjs = null;
                List<Money> amounts = null;

                // Deposit pas encore encore évalué: Gestion nette
                if (false == pDepositToEvaluate.IsGrossMargining)
                {
                    // Lire toute la position
                    // PM 20170313 [22833] Gestion position et tradevalue
                    //IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions = PositionsExtractor.GetPositions(pDepositToEvaluate.Factors);
                    RiskData riskData = RiskElement.AggregateRiskData(pDepositToEvaluate.Factors);

                    // Lancer l'évaluation
                    // PM 20170313 [22833] Gestion position et tradevalue
                    //amounts = EvaluateRiskElement(pActorId, pBookId, pDepositToEvaluate.HierarchyClass,  positions, out calcMethodComObjs);
                    amounts = EvaluateRiskElement(pActorId, pBookId, pDepositToEvaluate.HierarchyClass, riskData, out calcMethodComObjs);
                }
                else
                // Deposit pas encore encore évalué: Gestion Brute
                {
                    // Evaluer chaque élément séparement
                    foreach (RiskElement factor in pDepositToEvaluate.Factors)
                    {
                        List<Money> subamounts = new List<Money>();
                        Pair<int, int> keyDeposit = new Pair<int, int>(factor.ActorId, factor.AffectedBookId);

                        // L'élément est-il un Deposit d'un autre acteur
                        bool isGetAmountsSubDeposit =
                            ((factor.ActorId != pActorId) && (factor.AffectedBookId != pBookId) && Deposits.ContainsKey(keyDeposit));

                        // L'élément est le Deposit d'un autre acteur faire un appel récursif
                        if (isGetAmountsSubDeposit)
                        {
                            Deposit deposit = Deposits[keyDeposit];
                            if (deposit.Status == DepositStatus.NOTEVALUATED)
                            {
                                subamounts = EvaluateDeposit(keyDeposit.First, keyDeposit.Second, deposit);
                            }
                            else
                            {
                                subamounts = deposit.Amounts;
                            }
                        }
                        else
                        // L'élément est un élément du Deposit courant, alors l'évaluer en utilisant la gestion Brute (book par book)
                        {
                            // Lire la position par book
                            // PM 20170313 [22833] Changement de type et RiskElement.Positions remplacé par RiskElement.RiskDataActorBook
                            //IEnumerable<IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>>> riskDataByBook = PositionsExtractor.GetPositionsByBook(factor);
                            IEnumerable<RiskData> riskDataByBook = factor.GetRiskData();

                            // PM 20170313 [22833] Changement de type
                            //foreach (IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> bookRiskData in riskDataByBook)
                            foreach (RiskData bookRiskData in riskDataByBook)
                            {
                                // FI 20160613 [22256]  Mise en place des variables idA, idB
                                int idA = 0;
                                int idB = 0;
                                // PM 20170313 [22833] Changement de type
                                //PosRiskMarginKey itemPosRiskMarginKey = ((Pair<PosRiskMarginKey, RiskMarginPosition>)bookRiskData.First()).First;
                                RiskActorBook riskActorBookKey = default;
                                if (pDepositToEvaluate.HierarchyClass == DepositHierarchyClass.ENTITY)
                                {
                                    //idA = itemPosRiskMarginKey.idA_Dealer;
                                    //idB = itemPosRiskMarginKey.idB_Dealer;
                                    riskActorBookKey = bookRiskData.GetFirstDealerActorBook();
                                }
                                else if (pDepositToEvaluate.HierarchyClass == DepositHierarchyClass.CLEARER)
                                {
                                    //idA = itemPosRiskMarginKey.idB_Clearer;
                                    //idB = itemPosRiskMarginKey.idB_Clearer;
                                    riskActorBookKey = bookRiskData.GetFirstClearerActorBook();
                                }
                                //else
                                if (riskActorBookKey == default)
                                {
                                    throw new NotImplementedException(StrFunc.AppendFormat("DepositHierarchyClass: {0} is not implemented", pDepositToEvaluate.HierarchyClass));
                                }
                                idA = riskActorBookKey.IdA;
                                idB = riskActorBookKey.IdB;

                                List<Money> bookSubamounts = EvaluateRiskElement(idA, idB, pDepositToEvaluate.HierarchyClass, bookRiskData, out IMarginCalculationMethodCommunicationObject[] itemCalcMethodComObjs);

                                // FI 20160613 [22256] Concaténation des différents résultats afin d'obtenir les réductions de positions 
                                // Remarque : seuls les éléments UnderlyingStock seront exploités
                                if (calcMethodComObjs == null)
                                {
                                    calcMethodComObjs = itemCalcMethodComObjs;
                                }
                                else
                                {
                                    calcMethodComObjs = (calcMethodComObjs.Concat(itemCalcMethodComObjs)).ToArray();
                                }
                                // Mettre à 0 les montants négatifs avant de les ajouter (deposit < 0 -> deposit := 0)
                                bookSubamounts = RiskTools.SetNegativeToZero(bookSubamounts);
                                BaseMethod.SumAmounts(bookSubamounts, ref subamounts);
                            }
                        }
                        // Mettre à 0 les montants négatifs avant de les ajouter (deposit < 0 -> deposit := 0)
                        subamounts = RiskTools.SetNegativeToZero(subamounts);

                        // Ajouter les sous-montants aux montants totaux
                        BaseMethod.SumAmounts(subamounts, ref amounts);
                    }
                }
                // PM 20170313 [22833] Ajout d'un montant à 0 au cas ou aucun montant n'aurait été calculé
                if (amounts == default)
                {
                    amounts = new List<Money>();
                }
                if (amounts.Count == 0)
                {
                    Money zeroAmount;
                    if (StrFunc.IsFilled(m_CssCurrency))
                    {
                        zeroAmount = new Money(0, m_CssCurrency);
                    }
                    else
                    {
                        zeroAmount = new Money(0, "EUR");
                    }
                    amounts.Add(zeroAmount);
                }

                // Le Deposit est évalué, affecter et retourner les montants
                pDepositToEvaluate.Status = DepositStatus.EVALUATED;
                pDepositToEvaluate.Amounts = amounts;
                pDepositToEvaluate.MarginCalculationMethods = calcMethodComObjs;
            }
            return pDepositToEvaluate.Amounts;
        }

        /// <summary>
        /// Evaluation du déposit de livraison
        /// </summary>
        /// <param name="pPositionsToEvaluate"></param>
        /// <param name="opDeliveryComObj"></param>
        /// <returns></returns>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        // PM 20170313 [22833] Changement de type pour le paramètre pPositionsToEvaluate
        //private List<Money> EvaluateRiskElementsDeliveries(int pActorId, int pBookId,
        //    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
        //    out IMarginCalculationMethodCommunicationObject opDeliveryComObj)
        private List<Money> EvaluateRiskElementsDeliveries(RiskDataPosition pPositionsToEvaluate, out IMarginCalculationMethodCommunicationObject opDeliveryComObj)
        {
            DeliveryMarginCalculationMethodCommunicationObject deliveryMethodObj = new DeliveryMarginCalculationMethodCommunicationObject();
            List<Money> amounts = new List<Money>();

            // PM 20180918 [XXXXX] Gestion de l'abscence de position
            if ((pPositionsToEvaluate != default(RiskDataPosition)) && (pPositionsToEvaluate.Count() > 0))
            {
                // 1. Group the settlements by asset (the side of the new merged assets will be set with regards to the long and short quantities)

                // PM 20130904 [17949] Livraison
                // PM 20170313 [22833] Changement de type pour le paramètre pPositionsToEvaluate
                //IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pExeAssToEvaluate =
                //    from position in pPositionsToEvaluate where position.Second.DeliveryQuantity != 0 select position;
                //IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> posExeAssToEvaluate =
                //from position in pPositionsToEvaluate.GetPositionAsEnumerablePair() where position.Second.DeliveryQuantity != 0 select position;

                // PM 20190401 [24625][24387] Ne pas prendre la position qui concerne l'Additional Margin BoM (AMBO) de l'ECC qui est traité directement dans la méthode SPAN
                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> posExeAssToEvaluate =
                    from position in pPositionsToEvaluate.GetPositionAsEnumerablePair()
                    where (position.Second.DeliveryQuantity != 0)
                    && (position.Second.DeliveryExpressionType != ExpressionType.ECC_AMBO)
                    select position;

                // PM 20180918 [XXXXX] Gestion de l'abscence de position
                if ((posExeAssToEvaluate != default) && (posExeAssToEvaluate.Count() > 0))
                {
                    // PM 20140917 [20358] Faire le netting des positions en Pré-Livraison uniquement
                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> preDelivery =
                                from position in posExeAssToEvaluate
                                where position.Second.DeliveryStep == InitialMarginDeliveryStepEnum.PreDelivery
                                select position;

                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedExeAssByIdAsset = PositionsGrouping.GroupPositionsByAsset(preDelivery);

                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> notPreDelivery =
                                from position in posExeAssToEvaluate
                                where position.Second.DeliveryStep != InitialMarginDeliveryStepEnum.PreDelivery
                                select position;

                    posExeAssToEvaluate = groupedExeAssByIdAsset.Concat(notPreDelivery);

                    // 2. create the calculation sheet business object (used to write the calculation sheet report)
                    // PM 20180918 [XXXXX] Gestion de l'abscence de position : deliveryMethodObj déplacé en début de méthode
                    //DeliveryMarginCalculationMethodCommunicationObject deliveryMethodObj = new DeliveryMarginCalculationMethodCommunicationObject();

                    List<DeliveryParameterCommunicationObject> deliveryParameters = new List<DeliveryParameterCommunicationObject>();

                    // 3. cycle on the loaded delivery parameters 

                    // 3.1 Filter on the "in position" parameters

                    ExchangeTradedDerivative unkProduct = new ExchangeTradedDerivative();

                    // PM 20130904 [17949] Livraison
                    //IEnumerable<AssetDeliveryParameter> deliveryParametersInPosition =
                    //    from position in groupedExeAssByIdAsset
                    //    join asset in m_ParametersDeliveries on position.First.idAsset equals asset.AssetId
                    //    select asset;
                    IEnumerable<AssetDeliveryParameter> deliveryParametersInPosition = (
                                from position in posExeAssToEvaluate
                                join asset in m_ParametersDeliveries on position.First.idAsset equals asset.AssetId
                                where asset.DeliveryStep == position.Second.DeliveryStep
                                select asset).Distinct();

                    foreach (AssetDeliveryParameter sqlParameter in deliveryParametersInPosition)
                    {

                        // 4. Build the delivery parameter communication object (used to write the calculation sheet report)

                        DeliveryParameterCommunicationObject deliveryParameter = new DeliveryParameterCommunicationObject();
                        deliveryParameters.Add(deliveryParameter);

                        //deliveryParameter.Positions = (
                        //    from position in groupedExeAssByIdAsset
                        //    where position.First.idAsset == sqlParameter.AssetId
                        //    select position)
                        //    .ToArray();
                        deliveryParameter.Positions = (
                                    from position in posExeAssToEvaluate
                                    where (position.First.idAsset == sqlParameter.AssetId)
                                    && (position.Second.DeliveryStep == sqlParameter.DeliveryStep)
                                    select position)
                            .ToArray();

                        // 4.1 get the contract information (identifier, ...) for the current parameter
                        deliveryParameter.AssetId = sqlParameter.AssetId;
                        deliveryParameter.ExpressionType = sqlParameter.ExpressionType;
                        deliveryParameter.DeliveryValue = sqlParameter.DeliveryValue;
                        // PM 20130911 [17949] Livraison
                        deliveryParameter.DeliveryStep = sqlParameter.DeliveryStep;

                        // the quote is used without exceptions, using 1 by default (because of FixedAmount parameters)
                        deliveryParameter.Quote = 1;
                        // PM 20120305
                        // Contract Size n'est utilisé que pour le calcul en pourcentage.
                        // (Pour un calcul en montant on le mets à 1 pour ne pas en tenir compte)
                        deliveryParameter.Size = 1;

                        if (deliveryParameter.ExpressionType == ExpressionType.Percentage)
                        {
                            int assetQuote =
                                sqlParameter.ContractCategory == "O" ? sqlParameter.UnlAssetId : sqlParameter.AssetId;
                            deliveryParameter.UnderlyerCategory =
                                sqlParameter.ContractCategory == "O" ? sqlParameter.UnlCategory : Cst.UnderlyingAsset.Future;

                            deliveryParameter.SettlementDate = deliveryParameter.Positions.Max(pos => pos.Second.SettlementDate);


                            deliveryParameter.Size = sqlParameter.ContractSize;

                            // 4.2 mining the quote for percentage parameters
                            deliveryParameter.Quote =
                                BaseMethod.GetQuote(unkProduct, sqlParameter.ConnectionString,
                                assetQuote, deliveryParameter.UnderlyerCategory.Value, deliveryParameter.SettlementDate.Value,
                                out string idMarketEnv, out string idValScenario, out DateTime adjustedTime, out string quoteSide, out string quoteTiming, out SystemMSGInfo systemMsgInfo);

                            deliveryParameter.IdMarketEnv = idMarketEnv;
                            deliveryParameter.IdValScenario = idValScenario;
                            deliveryParameter.AdjustedTime = adjustedTime;
                            deliveryParameter.QuoteSide = quoteSide;
                            deliveryParameter.QuoteTiming = quoteTiming;
                            deliveryParameter.SystemMsgInfo = systemMsgInfo;
                        }

                        if (deliveryParameter.Quote == 0)
                        {
                            deliveryParameter.Missing = true;
                        }

                        // 4.3 compute the amount

                        // Prendre la valeur absolue de 'pos.Second.ExeAssQuantity' qui apparement est signée
                        decimal amount = deliveryParameter.Positions.Sum((pos) => (int)System.Math.Abs(pos.Second.DeliveryQuantity))
                            * sqlParameter.DeliveryValue * deliveryParameter.Size * deliveryParameter.Quote;
                        //
                        EFS_Cash cash = new EFS_Cash(sqlParameter.ConnectionString, amount, sqlParameter.Currency);
                        deliveryParameter.MarginAmount = new Money(cash.AmountRounded, sqlParameter.Currency);

                        BaseMethod.SumAmounts(new Money[] { (Money)deliveryParameter.MarginAmount }, ref amounts);

                    } // loop parameter

                    deliveryMethodObj.Parameters = deliveryParameters.ToArray();
                }
            }
            else
            {
                deliveryMethodObj.Parameters = new DeliveryParameterCommunicationObject[0];
            }

            opDeliveryComObj = deliveryMethodObj;

            return amounts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnection"></param>
        /// <param name="pDbParametersValue"></param>
        /// <param name="pCS"></param>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        // EG 20180803 PERF Suppresion SESSIONID
        private void LoadParametersDeliveries(IDbConnection pConnection, Dictionary<string, object> pDbParametersValue, string pCS)
        {
            CommandType delivCmdTyp = DataContractHelper.GetType(DataContractResultSets.ASSETDELIVERY);

            // 1. Init the request using db parameters and objects collection

            string delivRequest =
                DataHelper<object>.IsNullTransform(
                    pConnection,
                    delivCmdTyp,
                    DataContractHelper.GetQuery(DataContractResultSets.ASSETDELIVERY)
                );

            // 2. Load all the needed parameters

            m_ParametersDeliveries =
                DataHelper<AssetDeliveryParameter>.ExecuteDataSet(
                    pConnection,
                    delivCmdTyp,
                    delivRequest,
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.ASSETDELIVERY, pDbParametersValue)
                );

            foreach (AssetDeliveryParameter delivery in m_ParametersDeliveries)
            {
                delivery.ConnectionString = pCS;
            }
        }

        /// <summary>
        /// get the specific ratio 
        /// </summary>
        /// <param name="pActorId">actor whose deposits are using the ratio</param>
        /// <param name="pMainWeightingRatio">main ratio of the input actor (normally owned by the marginreqoffice attribute)</param>
        /// <returns>the more specific ratio found for the given actor</returns>
        // PM 20160404 [22116] Déplacé à partir de RiskMethodBase
        private decimal FindSpecificWeightingRatio(int pActorId, decimal pMainWeightingRatio)
        {
            // default ratio, the main one
            decimal res = (pMainWeightingRatio >= 0) ? pMainWeightingRatio : 1;

            // search the specific ratio for the clearing house, if it exists
            if (MarginReqOfficeParameters.ContainsKey(pActorId))
            {
                MarginReqOfficeParameter specificClearingHouseParam =
                    (from parameter in MarginReqOfficeParameters[pActorId]
                     where parameter.CssId == this.IdCSS
                     select parameter)
                    .FirstOrDefault();

                // PM 20170106 [22633] Ajout test de présence du paramétrage
                if ((specificClearingHouseParam != default(MarginReqOfficeParameter)) && (specificClearingHouseParam.ActorId > 0) && (specificClearingHouseParam.WeightingRatio >= 0))
                {
                    res = specificClearingHouseParam.WeightingRatio;
                }
            }

            return res;
        }

        /// <summary>
        /// Get the method object starting from the given pMethodType type
        /// </summary>
        /// <param name="pMethodType">type of the method we want to get back</param>
        /// <param name="pImRequestDiagnostics">collection of diagnostic elements, can be empty</param>
        /// <param name="pImMethodParameter">Paramètres généraux de la méthode</param>
        /// <returns></returns>
        /// PM 20160404 [22116] Déplacé à partir de RiskMethodFactory, ajout paramètre pImMethodParameter, suppression parametres pIdCSS, pDtBusiness, pDtMarket, pSessionId et passage en private
        //public BaseMethod BuildMethod(int pIdCSS, DateTime pDtBusiness, DateTime pDtMarket, pMethodType, pIdEntity,
        //    SettlSessIDEnum pTiming, InitialMarginMethodEnum pMethodType, string pSessionId, IMRequestDiagnostics pImRequestDiagnostics, TimeSpan pRiskDataTime)
        // PM 20180219 [23824] Supression des paramètres pIdEntity, pTiming et pRiskDataTime
        //private BaseMethod BuildMethod(int pIdEntity, InitialMarginMethodEnum pMethodType,  SettlSessIDEnum pTiming,
        //    IMRequestDiagnostics pImRequestDiagnostics, TimeSpan pRiskDataTime, ImMethodParameter pImMethodParameter)
        private BaseMethod BuildMethod(InitialMarginMethodEnum pMethodType, IMRequestDiagnostics pImRequestDiagnostics, ImMethodParameter pImMethodParameter)
        {
            BaseMethod method;
            switch (pMethodType)
            {
                // PM 20221212 [XXXXX] Ajout méthode NOMARGIN pour aucun calcul
                case InitialMarginMethodEnum.NOMARGIN:
                    method = new NoMarginRiskMethod();
                    break;
                case InitialMarginMethodEnum.Custom:
                    method = new CustomMethod();
                    break;
                case InitialMarginMethodEnum.TIMS_IDEM:
                    method = new TimsIdemMethod();
                    break;
                case InitialMarginMethodEnum.TIMS_EUREX:
                    method = new TimsEUREXMethod();
                    break;
                case InitialMarginMethodEnum.SPAN_C21:
                    method = new SPANC21Method();
                    break;
                case InitialMarginMethodEnum.SPAN_CME:
                    method = new SPANCMEMethod();
                    break;
                case InitialMarginMethodEnum.London_SPAN:
                    method = new LondonSPANMethod();
                    break;
                case InitialMarginMethodEnum.CBOE_Margin:
                    method = new CBOEMarginMethod();
                    break;
                case InitialMarginMethodEnum.MEFFCOM2:
                    method = new MEFFCOM2Method();
                    break;
                case InitialMarginMethodEnum.OMX_RCAR:
                    method = new RiskMethodOMXRCaR();
                    break;
                case InitialMarginMethodEnum.EUREX_PRISMA:
                    method = new PrismaMethod();
                    break;
                // PM 20170313 [22833] Ajout méthode IMSM
                case InitialMarginMethodEnum.IMSM:
                    method = new IMSMMethod();
                    break;
                // PM 20190222 [24326] Nouvelle méthode NOMX_CFM
                case InitialMarginMethodEnum.NOMX_CFM:
                    method = new NOMXCFMMethod();
                    break;
                // PM 20210518 [XXXXX] Ajout méthode SPAN 2, nouvelle méthode de CME Clearing
                case InitialMarginMethodEnum.SPAN_2_CORE:
                    method = new SPAN2CoreApiMethod();
                    break;
                case InitialMarginMethodEnum.SPAN_2_SOFTWARE:
                    method = new SPAN2MarginSoftwareMethod();
                    break;
                // PM 20230612 [26091][WI390] Ajout méthode Euronext Var Based
                case InitialMarginMethodEnum.EURONEXT_VAR_BASED:
                    method = new EuronextVarBasedRiskMethod();
                    break;
                // PM 20240423 [XXXXX] Ajout EURONEXT_LEGACY_VAR
                case InitialMarginMethodEnum.EURONEXT_LEGACY_VAR:
                    method = new EuronextLegacyVarRiskMethod();
                    break;
                default:
                    throw new NotSupportedException();
            }
            //
            // PM 20180219 [23824] utilisation de ProcessInfo et supression SessionId
            //method.SessionId = m_SessionId;
            method.IdCSS = m_IdCSS;
            method.DtBusiness = m_DtBusiness;
            method.DtMarket = m_DtMarket;
            //
            // PM 20180219 [23824] Ajout ProcessInfo et supression IdEntity, Timing et RiskDataTime
            method.ProcessInfo = m_ProcessInfo;
            //method.IdEntity = pIdEntity;
            //method.Timing = pTiming;
            //method.RiskDataTime = pRiskDataTime;
            method.ImRequestDiagnostics = pImRequestDiagnostics;
            //
            method.MethodParameter = pImMethodParameter;
            //
            // PM 20180219 [23824] Renseigner IdIOTaskRiskData
            method.IdIOTaskRiskData = m_IdIOTaskRiskData;
            //
            // PM 20180530 [23824] Renseigner IsRiskDataOnHoliday
            method.IsRiskDataOnHoliday = m_IsRiskDataOnHoliday;
            return method;
        }
        #endregion Methods
    }
}

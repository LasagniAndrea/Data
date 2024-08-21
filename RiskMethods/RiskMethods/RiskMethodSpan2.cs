using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.LoggerClient.LoggerService;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.RiskMethods.Span2CoreAPI;
using EFS.SpheresRiskPerformance.RiskMethods.Span2CoreAPI.v1_14;
using EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin;
using EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin.v1_0_39;
//
using EfsML.Enum;
//
using FixML.Enum;
//
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Classe de calcul du déposit SPAN2
    /// </summary>
    public abstract class SPAN2Method : BaseMethod
    {
        #region Members
        /// <summary>
        /// Date Business
        /// </summary>
        protected DateTime m_DtBusiness = default;
        /// <summary>
        /// Acronym de la Clearing House
        /// </summary>
        protected string m_CssAcronym = string.Empty;
        /// <summary>
        /// Price de chaque Asset
        /// </summary>
        protected List<AssetQuoteParameter> m_AssetQuoteParameters = default;
        /// <summary>
        /// Donnée référentiel Asset
        /// </summary>
        protected IEnumerable<AssetExpandedParameter> m_AssetExpandedParameters = default;
        /// <summary>
        /// Donnée référentiel Asset
        /// </summary>
        // PM 20230929 [XXXXX] Ajout
        protected IEnumerable<AssetExpandedParameter> m_AssetUnlFutureExpandedParameters = default;

        // Client Http
        protected static readonly HttpClientHelper m_HttpClientHelper = new HttpClientHelper();
        #endregion Members

        #region Override base accessors
        #endregion Override base accessors

        #region Constructor
        public SPAN2Method()
        {
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }
        #endregion Constructor

        #region Override base methods
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
        /// Charge les paramètres spécifiques à la méthode.
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// <param name="pAssetETDCache">Collection d'assets contenant tous les assets en position</param>
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            m_DtBusiness = GetRiskParametersDate(pCS);
            //
            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                // ASSETEXPANDED_ALLMETHOD
                m_AssetExpandedParameters = LoadParametersAssetExpanded(connection);

                // UNDERLYNGASSETFUTUREEXPANDED_ALLMETHOD
                m_AssetUnlFutureExpandedParameters = LoadParametersUnderlyingAssetFutureExpanded(connection);
            }

            // Construction de la liste des assets
            m_AssetQuoteParameters = (
                from asset in m_AssetExpandedParameters
                select new AssetQuoteParameter
                {
                    AssetId = asset.AssetId,
                    AssetCategoryEnum = Cst.UnderlyingAsset.ExchangeTradedContract
                }).ToList();

            // Lecture des cotations
            GetQuotes(pCS, m_AssetQuoteParameters, default, pAssetETDCache);

            // Rechercher les asset futures sous-jacent qui ne sont pas déjà dans l'ensemble des assets en position
            IEnumerable<AssetExpandedParameter> assetToAdd = m_AssetUnlFutureExpandedParameters.Where(x => false == m_AssetExpandedParameters.Any(a => a.AssetId == x.AssetId));

            // Ajouter les parameters des futures sous-jacent d'asset en position
            m_AssetExpandedParameters = m_AssetExpandedParameters.Concat(assetToAdd).ToList();
        }

        /// <summary>
        /// Libère les paramètres spécifiques à la méthode.
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            m_AssetExpandedParameters = default;
            m_AssetUnlFutureExpandedParameters = default;
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
        public override List<Money> EvaluateRiskElementSpecific(
            int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
            RiskData pRiskDataToEvaluate,
            out IMarginCalculationMethodCommunicationObject opMethodComObj)
        {
            List<Money> riskAmounts = null;

            // Creation de l'objet de communication du détail du calcul
            SPAN2CalcMethCom methodComObj = new SPAN2CalcMethCom();
            opMethodComObj = methodComObj;                          // Affectation de l'objet de communication du détail du calcul en sortie
            methodComObj.MarginMethodType = this.Type;              // Affectation du type de méthode de calcul
            methodComObj.CssCurrency = m_CssCurrency;               // Affectation de la devise de calcul
            methodComObj.IdA = pActorId;                            // Affectation de l'id de l'acteur
            methodComObj.IdB = pBookId;                             // Affectation de l'id du book
            methodComObj.DtParameters = m_DtBusiness;               // Date Business
            methodComObj.Timing = Timing;                           // Timing: Intraday,EndOfDay
            methodComObj.IsTryExcludeWrongPosition = m_ImMethodParameter.IsTryExcludeWrongPosition; // Indicateur d'essai d'écarter les positions sur des assets erronés
            methodComObj.BaseUrl = m_ImMethodParameter.BaseUrl;     // Base URL pour le calcul CME SPAN 2
            methodComObj.UserId = m_ImMethodParameter.UserId;       // Id du User URL pour le calcul CME SPAN 2
            // PM 20231030 [26547][WI735] Ajout MarginCurrencyTypeEnum
            methodComObj.MarginCurrencyTypeEnum = m_ImMethodParameter.MarginCurrencyTypeEnum; // Type de devise pour le résultat du calcul de déposit (Devise de contrat ou devise de contrevaleur de la chambre)
            methodComObj.Missing = false;
            methodComObj.IsIncomplete = false;
            methodComObj.CounterInfo.NbAssetParameters = m_AssetExpandedParameters.Count(); // Compteur de paramètres d'asset
            GetSpanAccountParameters(methodComObj);                 // Affectation des paramètres de compte pour le calcul
            // PM 20230830 [26470] Ajout ErrorList
            methodComObj.ErrorList = new List<(SysMsgCode, List<LogParam>)>();
            //
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsToEvaluate = default;
            //
            if (pRiskDataToEvaluate != default(RiskData))
            {
                positionsToEvaluate = pRiskDataToEvaluate.GetPositionAsEnumerablePair();

                // Compteur position initiale
                methodComObj.CounterInfo.NbInitialPosition = positionsToEvaluate.Count();

                if (methodComObj.CounterInfo.NbInitialPosition > 0)
                {
                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions;

                    switch (methodComObj.SpanAccountType)
                    {
                        case SpanAccountType.OmnibusHedger:
                        case SpanAccountType.OmnibusSpeculator:
                            positions = positionsToEvaluate;
                            break;
                        default:
                            // Grouper les positions par asset
                            positions = PositionsGrouping.GroupPositionsByAsset(positionsToEvaluate);
                            break;
                    }

                    // Ne garder que les positions dont la quantité est différente de 0
                    positions = from pos in positions
                                where (pos.Second.Quantity != 0)
                                select pos;

                    // Compteur position nette
                    methodComObj.CounterInfo.NbNettedPosition = positions.Count();

                    // Coverage short call and short futures (this one modify the position quantity)
                    IEnumerable<CoverageSortParameters> inputCoverage = GetSortParametersForCoverage(positions);
                    // Reduction de la position couverte
                    Pair<IEnumerable<StockCoverageCommunicationObject>, IEnumerable<StockCoverageDetailCommunicationObject>> coveredQuantities =
                        ReducePosition(pActorId, pBookId, pDepositHierarchyClass, inputCoverage, ref positions);
                    methodComObj.UnderlyingStock = coveredQuantities.Second;

                    // Compteur position réduite
                    methodComObj.CounterInfo.NbReducedPosition = positions.Count();

                    // Calculer les montants de risque
                    List<Money> marginAmountList = EvaluateRisk(methodComObj, positions);

                    riskAmounts = marginAmountList;
                }
            }

            if (riskAmounts == default)
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
            //
            methodComObj.MarginAmounts = riskAmounts.ToArray();
            //
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
                join assetRef in m_AssetExpandedParameters on position.First.idAsset equals assetRef.AssetId
                where (false == assetRef.PutOrCall.HasValue) || (assetRef.PutOrCall == PutOrCallEnum.Call)
                join quote in m_AssetQuoteParameters on assetRef.AssetId equals quote.AssetId
                select new CoverageSortParameters
                {
                    AssetId = position.First.idAsset,
                    ContractId = assetRef.ContractId,
                    MaturityYearMonth = decimal.Parse(assetRef.MaturityDate.Year.ToString() + assetRef.MaturityDate.Month.ToString() + assetRef.MaturityDate.Day.ToString()),
                    Multiplier = assetRef.ContractMultiplier,
                    Quote = quote.Quote,
                    StrikePrice = assetRef.StrikePrice,
                    Type = assetRef.PutOrCall.HasValue ? RiskMethodQtyType.Call : RiskMethodQtyType.Future,
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

            // Recherche des informations sur la chambre courante
            EntityMarketWithCSS currentEMWithCSS = pEntityMarkets.First(elem => elem.CssId == this.IdCSS);

            m_CssAcronym = currentEMWithCSS.CssAcronym;
        }
        #endregion Override base methods

        #region abstract methods
        /// <summary>
        /// Evaluation du déposit via SPAN2
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pMarginData">Données nécessaire au calcul</param>
        /// <returns></returns>
        protected abstract List<Money> EvaluateSpan2(SPAN2CalcMethCom pMethodComObj, SPAN2MarginData pMarginData);
        #endregion virtual methods

        #region Methods
        /// <summary>
        ///  Evalue les montants de dépot de garantie
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pPositions">Position</param>
        /// <returns>Données calculés</returns>
        private List<Money> EvaluateRisk(SPAN2CalcMethCom pMethodComObj, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            List<Money> marginAmountList = default;

            IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> position = new Dictionary<PosRiskMarginKey, RiskMarginPosition>();

            if (pPositions != default)
            {
                // PM 20230830 [26470] Lecture des positions échues qui seront écartées
                List<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsEchues = (
                    from pos in pPositions
                    join asset in m_AssetExpandedParameters on pos.First.idAsset equals asset.AssetId
                    where (asset.MaturityDateSys <= m_DtBusiness) && (asset.MaturityDateSys != asset.MaturityDate)
                    select pos).ToList();

                // Passage de la position de Pair à KeyValuePair (correspondant aux éléments d'un Dictionary)
                // en ne conservant que les positions dont la date d'échéance système ((MaturityDateSys ) est supérieure à la date business du traitement 
                // afin d'exclure les position qui sont normalement échues au sein de la Clearing House mais encore actives dans Spheres 
                // du fait d’un décalage forcée de la date d’échéance (ex. chez XCHANGING MaturityDate > MaturityDateSys)
                // NB : cas où la date d’échéance est non renseignée (ex. MR = DefaultRule), on a « asset.MaturityDateSys = asset.MaturityDate = 1er janvier 0001 »
                //      On considèrera ces positions sans date d’échéance, toutefois elles pourront entrainer une erreur de valorisation si leur date d’échéance réelle est atteinte.
                // RD 20230804 [26454] Use filtre (asset.MaturityDateSys > m_DtBusiness)
                position =
                    from pos in pPositions
                    join asset in m_AssetExpandedParameters on pos.First.idAsset equals asset.AssetId
                    // where ((asset.MaturityDate == asset.MaturityDateSys) || (asset.MaturityDateSys < m_DtBusiness))
                    where ((asset.MaturityDateSys > m_DtBusiness) || (asset.MaturityDateSys == asset.MaturityDate))
                    select new KeyValuePair<PosRiskMarginKey, RiskMarginPosition>(pos.First, pos.Second);

                // Compteur de position non échues
                pMethodComObj.CounterInfo.NbActivePosition = position.Count();

                // Alimentation des données pour la requete à envoyer
                SPAN2MarginData marginData = new SPAN2MarginData(position, m_AssetExpandedParameters)
                {
                    DtBusiness = m_DtBusiness,
                    CssAcronym = m_CssAcronym,
                    ActorId = pMethodComObj.IdA,
                    BookId = pMethodComObj.IdB,
                    IsMaintenance = pMethodComObj.IsMaintenanceAmount,
                    Timing = pMethodComObj.Timing
                };
                marginData.SetCurrency(pMethodComObj.CssCurrency);
                marginData.SetAccountType(pMethodComObj.SpanAccountType);

                // Ne procéder au calcul que s'il reste des positions
                if (position.Count() > 0)
                {
                    // Evaluation via SPAN 2 Core Api ou SPAN 2 Risk Margin Software
                    marginAmountList = EvaluateSpan2(pMethodComObj, marginData);
                }

                // Lecture des positions écartées car échues
                // PM 20230830 [26470] Ajout positions écartées car échues
                pMethodComObj.DiscartedPositions = new List<Pair<PosRiskMarginKey, RiskMarginPosition>>();
                pMethodComObj.CounterInfo.NbDiscartedPosition = 0;
                if (positionsEchues.Count() > 0)
                {
                    // Ajout des positions échues comme positions écartées
                    pMethodComObj.DiscartedPositions = pMethodComObj.DiscartedPositions.Concat(positionsEchues);

                    // Compteur de position écartées du calcul
                    pMethodComObj.CounterInfo.NbDiscartedPosition += positionsEchues.Count();

                    // Message d'erreur informant la présence de positions échues
                    pMethodComObj.ErrorList.Add((new SysMsgCode(SysCodeEnum.SYS, 1037), default(List<LogParam>)));
                    pMethodComObj.Missing = true;
                }
                // Lecture des positions écartées lors du calcul
                if (marginData.DiscartedPosition.Count() > 0)
                {
                    // Ajout des positions écartées lors du calcul
                    pMethodComObj.DiscartedPositions = pMethodComObj.DiscartedPositions.Concat(marginData.DiscartedPosition.Select(p => new Pair<PosRiskMarginKey, RiskMarginPosition>(p.Key, p.Value)));

                    // Compteur de position écartées du calcul
                    pMethodComObj.CounterInfo.NbDiscartedPosition += pMethodComObj.DiscartedPositions.Count();
                }

                // Lecture de la position considérées
                pMethodComObj.ConsideredPositions =
                    from pos in marginData.MarginPosition
                    select new Pair<PosRiskMarginKey, RiskMarginPosition>(pos.Key, pos.Value);

                // Compteur de position considéres dans le calcul
                pMethodComObj.CounterInfo.NbConsideredPosition = pMethodComObj.ConsideredPositions.Count();

                // Récupérer les messages d'erreur
                // PM 20230830 [XXXXX] Amélioration message d'erreur
                if (marginData.Errors.Count() > 0)
                {
                    pMethodComObj.Missing = true;
                    pMethodComObj.ErrorList.AddRange(marginData.GetErrorListForLog());

                    pMethodComObj.ErrorMessage = marginData.GetErrorMessage();
                }
            }

            return marginAmountList;
        }

        /// <summary>
        /// Récupert les paramètres de calcul SPAN de l'acteur spécifié
        /// </summary>
        /// <param name="pMethodComObj">Données de calcul</param>
        /// <param name="pActorId">Identifiant de l'acteur pour lequel le calcul SPAN est réalisé</param>
        /// <returns>true si l'acteur a été trouvé, sinon false</returns>
        private bool GetSpanAccountParameters(SPAN2CalcMethCom pMethodComObj)
        {
            bool actorHasParameter = false;
            //
            if (pMethodComObj != default(SPAN2CalcMethCom))
            {
                // Valeurs par défaut
                pMethodComObj.SpanAccountType = SpanAccountType.Member;
                // PM 20230322 [26282][WI607] Ajout utilisation de MethodParameter.IsMaintenance
                // pMethodComObj.IsMaintenanceAmount = true;
                pMethodComObj.IsMaintenanceAmount = MethodParameter.IsMaintenance;
                //
                if (MarginReqOfficeParameters != null)
                {
                    actorHasParameter = MarginReqOfficeParameters.ContainsKey(pMethodComObj.IdA);
                    if (actorHasParameter)
                    {
                        MarginReqOfficeParameter specificClearingHouseParam =
                                   (from parameter in MarginReqOfficeParameters[pMethodComObj.IdA]
                                    where parameter.CssId == this.IdCSS
                                    select parameter)
                                   .FirstOrDefault();

                        if ((specificClearingHouseParam != default(MarginReqOfficeParameter)) && (specificClearingHouseParam.ActorId != 0))
                        {
                            pMethodComObj.SpanAccountType = specificClearingHouseParam.SpanAccountType;
                            pMethodComObj.IsMaintenanceAmount = specificClearingHouseParam.SpanMaintenanceAmountIndicator;
                        }
                    }
                }
            }
            return actorHasParameter;
        }

        /// <summary>
        /// Lecture du montant de déposit dans le détail de la réponse
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pMarginData">Données de résultat du calcul</param>
        /// <returns></returns>
        internal List<Money> ReadMarginAmount(SPAN2CalcMethCom pMethodComObj, SPAN2MarginData pMarginData)
        {
            List<Money> marginAmountList = default;
            if ((pMarginData != default(SPAN2MarginData)) && (pMarginData.MarginDetailResponseMessage != default))
            {
                MarginDetailResponseMessage marginDetail = pMarginData.MarginDetailResponseMessage;
                if (ArrFunc.IsFilled(marginDetail.Errors))
                {
                    pMarginData.AddError(marginDetail.Errors);
                }
                if (marginDetail.Payload != default(MarginDetailResponse))
                {
                    MarginDetailResponse payload = marginDetail.Payload;
                    if ((payload != default(MarginDetailResponse)) && ArrFunc.IsFilled(payload.Portfolios))
                    {
                        if (payload.PointInTime != default(PointInTime))
                        {
                            // Maj de la date des paramètres de calcul (si encore égale à la date business)
                            if ((payload.PointInTime.BusinessDt != default) && (pMethodComObj.DtParameters == pMarginData.DtBusiness))
                            {
                                pMethodComObj.DtParameters = payload.PointInTime.BusinessDt;
                            }
                            if (StrFunc.IsEmpty(pMethodComObj.CycleCode))
                            {
                                pMethodComObj.CycleCode = payload.PointInTime.CycleCode.ToString();
                            }
                        }

                        // On traite un seul Portfolio
                        PortfolioMarginDetail portfolioMarginDetail = payload.Portfolios.FirstOrDefault();
                        if (portfolioMarginDetail != default(PortfolioMarginDetail))
                        {
                            // Lecture du nombre de positions réellement traitées
                            if (portfolioMarginDetail.TransactionCnt.HasValue)
                            {
                                pMethodComObj.CounterInfo.NbProcessedPosition = portfolioMarginDetail.TransactionCnt.Value;
                            }
                            else
                            {
                                pMethodComObj.CounterInfo.NbProcessedPosition = 0;
                            }
                            if (pMethodComObj.CounterInfo.NbProcessedPosition != pMarginData.MarginPosition.Count())
                            {
                                // Calcul de déposit effectué sur la base de positions partielles
                                pMethodComObj.ErrorCode = new SysMsgCode(SysCodeEnum.SYS, 1034);
                                pMethodComObj.Missing = true;
                                pMarginData.AddError(pMethodComObj.ErrorCode);
                            }

                            // Lecture du montant de déposit
                            // PM 20231030 [26547][WI735] Ajout MarginCurrencyTypeEnum
                            marginAmountList = RequirementAmountsToMoney(portfolioMarginDetail, pMarginData.IsMaintenance, pMethodComObj.MarginCurrencyTypeEnum);

                            // Alimentation des données du calcul pour le journal
                            FillMethodCom(pMethodComObj, portfolioMarginDetail);
                        }
                        else
                        {
                            // PM 20230830 [XXXXX] Ajout SysMsgCode
                            pMethodComObj.ErrorCode = new SysMsgCode(SysCodeEnum.SYS, 1035);
                            pMarginData.AddError(pMethodComObj.ErrorCode, "Portfolio Margin Detail is empty");
                            pMethodComObj.Missing = true;
                            pMethodComObj.IsIncomplete = true;
                        }
                    }
                }
            }
            return marginAmountList;
        }

        /// <summary>
        /// Lecture des montants totaux de risque à partir d'un ensemble de CurrencyMarginAmounts et retour sous forme d'une List de Money
        /// </summary>
        /// <param name="pCurrencyAmts"></param>
        /// <param name="pIsMaintenance"></param>
        /// <returns></returns>
        internal List<Money> CurrencyMarginAmountsToMoney(IEnumerable<CurrencyMarginAmounts> pCurrencyAmts, bool pIsMaintenance)
        {
            List<Money> amounts = new List<Money>();
            foreach (CurrencyMarginAmounts currencyAmts in pCurrencyAmts)
            {
                MarginRequirementAmounts requirementAmts = currencyAmts.RequirementAmts;
                if (currencyAmts.Currency.HasValue && (requirementAmts != default(MarginRequirementAmounts)))
                {
                    if (pIsMaintenance && requirementAmts.TotalMaintenanceMargin.HasValue)
                    {
                        Money maintAmount = new Money(requirementAmts.TotalMaintenanceMargin.Value, currencyAmts.Currency.Value.ToString());
                        amounts.Add(maintAmount);
                    }
                    else if (requirementAmts.TotalInitialMargin.HasValue)
                    {
                        Money initAmount = new Money(requirementAmts.TotalInitialMargin.Value, currencyAmts.Currency.Value.ToString());
                        amounts.Add(initAmount);
                    }
                    else
                    {
                        // Présence d'une devise, mais pas de montant
                        Money zeroAmount = new Money(0, currencyAmts.Currency.Value.ToString());
                        amounts.Add(zeroAmount);
                    }
                }
            }
            return amounts;
        }

        /// <summary>
        /// Lecture du montant total de risque à partir du RequirementAmts et retour du montant dans une List de un Money
        /// </summary>
        /// <param name="pCurrencyAmts"></param>
        /// <param name="pIsMaintenance"></param>
        /// <param name="pMarginCurrencyType"></param>
        /// <returns></returns>
        // PM 20231030 [26547][WI735] Ajout pMarginCurrencyType
        //internal List<Money> RequirementAmountsToMoney(PortfolioMarginDetail pMarginDetail, bool pIsMaintenance)
        internal List<Money> RequirementAmountsToMoney(PortfolioMarginDetail pMarginDetail, bool pIsMaintenance, Cst.InitialMarginCurrencyTypeEnum pMarginCurrencyType)
        {
            List<Money> amounts = new List<Money>();
            // PM 20231030 [26547][WI735] Gestion pMarginCurrencyType
            //if ((pMarginDetail != default(PortfolioMarginDetail))
            //    && (pMarginDetail.RequirementAmts != default(MarginRequirementAmounts)) && pMarginDetail.Currency.HasValue)
            if (pMarginDetail != default(PortfolioMarginDetail))
            {
                if (pMarginCurrencyType == Cst.InitialMarginCurrencyTypeEnum.ClearingHouseCurrency)
                {
                    // Montant en devise de contrevaleur
                    if ((pMarginDetail.RequirementAmts != default(MarginRequirementAmounts)) && pMarginDetail.Currency.HasValue)
                    {
                        if (pIsMaintenance)
                        {
                            if (pMarginDetail.RequirementAmts.TotalMaintenanceMargin.HasValue)
                            {
                                Money maintAmount = new Money(pMarginDetail.RequirementAmts.TotalMaintenanceMargin.Value, pMarginDetail.Currency.Value.ToString());
                                amounts.Add(maintAmount);
                            }
                        }
                        else if (pMarginDetail.RequirementAmts.TotalInitialMargin.HasValue)
                        {
                            Money initAmount = new Money(pMarginDetail.RequirementAmts.TotalInitialMargin.Value, pMarginDetail.Currency.Value.ToString());
                            amounts.Add(initAmount);
                        }
                        else
                        {
                            // Présence d'une devise, mais pas de montant
                            Money zeroAmount = new Money(0, pMarginDetail.Currency.Value.ToString());
                            amounts.Add(zeroAmount);
                        }
                    }
                }
                else
                {
                    // Montant en devises des contrats
                    // Lecture des montants de déposit
                    amounts = CurrencyMarginAmountsToMoney(pMarginDetail.CurrencyAmts, pIsMaintenance);
                }
            }
            return amounts;
        }

        /// <summary>
        /// Alimentation des données du détail des montants pour le journal CCP (Exchange Complex)
        /// </summary>
        /// <param name="pCcpCom"></param>
        /// <param name="pMarginAmounts"></param>
        private void FillSpan2CCPComWithMarginAmounts(Span2CCPCom pCcpCom, CurrencyMarginAmounts[] pMarginAmounts)
        {
            if ((pCcpCom != default(Span2CCPCom)) && (pMarginAmounts != default(CurrencyMarginAmounts[])))
            {
                List<Money> netOptionValue = new List<Money>();
                List<Money> riskInitialAmount = new List<Money>();
                List<Money> riskMaintenanceAmount = new List<Money>();
                List<Money> totalInitialMarginAmount = new List<Money>();
                List<Money> totalMaintenanceMarginAmount = new List<Money>();
                //
                foreach (CurrencyMarginAmounts amount in pMarginAmounts)
                {
                    if (amount.Currency.HasValue && amount.RequirementAmts != default(MarginRequirementAmounts))
                    {
                        string cur = amount.Currency.Value.ToString();
                        //
                        Money nov = new Money(amount.RequirementAmts.netOptionValue, cur);
                        Money riskInitial = new Money(amount.RequirementAmts.riskInitialRequirement, cur);
                        Money riskMaintenance = new Money(amount.RequirementAmts.riskMaintenanceRequirement, cur);
                        Money totalInitialMargin = new Money(amount.RequirementAmts.totalInitialMargin, cur);
                        Money totalMaintenanceMargin = new Money(amount.RequirementAmts.totalMaintenanceMargin, cur);
                        //
                        netOptionValue.Add(nov);
                        riskInitialAmount.Add(riskInitial);
                        riskMaintenanceAmount.Add(riskMaintenance);
                        totalInitialMarginAmount.Add(totalInitialMargin);
                        totalMaintenanceMarginAmount.Add(totalMaintenanceMargin);
                    }
                }
                //
                pCcpCom.NetOptionValue = netOptionValue.ToArray();
                pCcpCom.RiskInitialAmount = riskInitialAmount.ToArray();
                pCcpCom.RiskMaintenanceAmount = riskMaintenanceAmount.ToArray();
                pCcpCom.TotalInitialMarginAmount = totalInitialMarginAmount.ToArray();
                pCcpCom.TotalMaintenanceMarginAmount = totalMaintenanceMarginAmount.ToArray();
            }
        }

        /// <summary>
        /// Alimentation des données du détail des montants pour le journal Pod (Commodity Contract)
        /// </summary>
        /// <param name="pPodCom"></param>
        /// <param name="pMarginAmounts"></param>
        private void FillSpan2PodComWithPodMarginAmounts(Span2PodCom pPodCom, PodMarginAmounts pMarginAmounts)
        {
            if ((pPodCom != default(Span2PodCom)) && (pMarginAmounts != default(PodMarginAmounts)))
            {
                pPodCom.ContractGroup = pMarginAmounts.PodId;
                pPodCom.MarginMethod = pMarginAmounts.MarginMthd.ToString();
                //
                if (pMarginAmounts.Currency.HasValue)
                {
                    string cur = pMarginAmounts.Currency.Value.ToString();
                    //
                    if (pMarginAmounts.RequirementAmts != default(MarginRequirementAmounts))
                    {
                        MarginRequirementAmounts reqAmts = pMarginAmounts.RequirementAmts;
                        //
                        pPodCom.NetOptionValue = new Money(reqAmts.netOptionValue, cur);
                        pPodCom.RiskInitialAmount = new Money(reqAmts.riskInitialRequirement, cur);
                        pPodCom.RiskMaintenanceAmount = new Money(reqAmts.riskMaintenanceRequirement, cur);
                        //
                        if (reqAmts.ComponentAmts != default(MarginComponentAmounts))
                        {
                            MarginComponentAmounts compAmts = reqAmts.ComponentAmts;
                            //
                            pPodCom.FullValueComponent = new Money(compAmts.fullValueComponent, cur);
                            pPodCom.InterCommodityCreditAmount = new Money(compAmts.interCmdtySpreadCredit, cur);
                            pPodCom.InterCommodityVolatilityCredit = new Money(compAmts.interCmdtyVolatilityCredit, cur);
                            pPodCom.InterExchangeCreditAmount = new Money(compAmts.interExchSpreadCredit, cur);
                            pPodCom.IntraSpreadChargeAmount = new Money(compAmts.intraCmdtySpreadCharge, cur);
                            pPodCom.ScanRiskAmount = new Money(compAmts.scanRisk, cur);
                            pPodCom.ShortOptionMinimum = new Money(compAmts.shortOptionMin, cur);
                            pPodCom.DeliveryMonthChargeAmount = new Money(compAmts.spotCharge, cur);
                            //
                            pPodCom.ConcentrationComponent = new Money(compAmts.concentrationComponent, cur);
                            pPodCom.HvarComponent = new Money(compAmts.hvarComponent, cur);
                            pPodCom.LiquidityComponent = new Money(compAmts.liquidityComponent, cur);
                            pPodCom.StressComponent = new Money(compAmts.stressComponent, cur);
                            pPodCom.ImpliedOffset = new Money(compAmts.impliedOffset, cur);
                        }
                    }
                    //
                    if (pMarginAmounts.ValuationAmts != default(MarginValuationAmounts))
                    {
                        MarginValuationAmounts valAmts = pMarginAmounts.ValuationAmts;
                        //
                        pPodCom.LongNonOptionValue = new Money(valAmts.nonOptionValueLong, cur);
                        pPodCom.ShortNonOptionValue = new Money(valAmts.nonOptionValueShort, cur);
                        pPodCom.LongOptionValue = new Money(valAmts.optionValueLongEquityStyle, cur);
                        pPodCom.ShortOptionValue = new Money(valAmts.optionValueShortEquityStyle, cur);
                        pPodCom.LongOptionFuturesStyleValue = new Money(valAmts.optionValueLongFuturesStyle, cur);
                        pPodCom.ShortOptionFuturesStyleValue = new Money(valAmts.optionValueShortFuturesStyle, cur);
                    }
                }
            }
        }

        /// <summary>
        /// Alimentation des données du détail des montants pour le journal Product Group
        /// </summary>
        /// <param name="pProdGrpCom"></param>
        /// <param name="pMarginAmounts"></param>
        private void FillSpan2ProductGroupComWithProductGroupAmounts(Span2ProductGroupCom pProdGrpCom, ProductGroupAmounts pMarginAmounts)
        {
            if ((pProdGrpCom != default(Span2ProductGroupCom)) && (pMarginAmounts != default(ProductGroupAmounts)))
            {
                pProdGrpCom.ProductGroup = pMarginAmounts.ProductGroupId;
                //
                if (pMarginAmounts.Currency.HasValue)
                {
                    string cur = pMarginAmounts.Currency.Value.ToString();
                    //
                    if (pMarginAmounts.RequirementAmts != default(MarginRequirementAmounts))
                    {
                        MarginRequirementAmounts reqAmts = pMarginAmounts.RequirementAmts;
                        //
                        pProdGrpCom.RiskInitialAmount = new Money(reqAmts.riskInitialRequirement, cur);
                        pProdGrpCom.RiskMaintenanceAmount = new Money(reqAmts.riskMaintenanceRequirement, cur);
                        //
                        if (reqAmts.ComponentAmts != default(MarginComponentAmounts))
                        {
                            MarginComponentAmounts compAmts = reqAmts.ComponentAmts;
                            //
                            pProdGrpCom.ConcentrationComponent = new Money(compAmts.concentrationComponent, cur);
                            pProdGrpCom.HvarComponent = new Money(compAmts.hvarComponent, cur);
                            pProdGrpCom.LiquidityComponent = new Money(compAmts.liquidityComponent, cur);
                            pProdGrpCom.StressComponent = new Money(compAmts.stressComponent, cur);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Alimentation des données du détail du calcul pour le journal
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pMarginDetail"></param>
        private void FillMethodCom(SPAN2CalcMethCom pMethodComObj, PortfolioMarginDetail pMarginDetail)
        {
            if (pMarginDetail != default(PortfolioMarginDetail))
            {
                if ((pMarginDetail.RequirementAmts != default(MarginRequirementAmounts)) && pMarginDetail.Currency.HasValue)
                {
                    Span2TotalAmountCom requierementAmounts = new Span2TotalAmountCom();
                    if (pMarginDetail.RequirementAmts.RiskInitialRequirement.HasValue)
                    {
                        requierementAmounts.RiskInitialAmount = new Money(pMarginDetail.RequirementAmts.RiskInitialRequirement.Value, pMarginDetail.Currency.Value.ToString());
                    }
                    if (pMarginDetail.RequirementAmts.RiskMaintenanceRequirement.HasValue)
                    {
                        requierementAmounts.RiskMaintenanceAmount = new Money(pMarginDetail.RequirementAmts.RiskMaintenanceRequirement.Value, pMarginDetail.Currency.Value.ToString());
                    }
                    if (pMarginDetail.RequirementAmts.TotalInitialMargin.HasValue)
                    {
                        requierementAmounts.TotalInitialMarginAmount = new Money(pMarginDetail.RequirementAmts.TotalInitialMargin.Value, pMarginDetail.Currency.Value.ToString());
                    }
                    if (pMarginDetail.RequirementAmts.TotalMaintenanceMargin.HasValue)
                    {
                        requierementAmounts.TotalMaintenanceMarginAmount = new Money(pMarginDetail.RequirementAmts.TotalMaintenanceMargin.Value, pMarginDetail.Currency.Value.ToString());
                    }
                    if (pMarginDetail.RequirementAmts.NetOptionValue.HasValue)
                    {
                        requierementAmounts.NetOptionValue = new Money(pMarginDetail.RequirementAmts.NetOptionValue.Value, pMarginDetail.Currency.Value.ToString());
                    }
                    pMethodComObj.RequierementAmounts = requierementAmounts;
                }
                if (pMarginDetail.Ccps != default(CcpMarginAmounts[]))
                {
                    // Construire un array de Span2CCPSCom avec un élément pour chaque CcpMarginAmounts
                    pMethodComObj.Parameters = (from ccps in pMarginDetail.Ccps
                                                select new Span2CCPCom { ClearingOrganization = ccps.ClearingOrganizationId }).ToArray();
                    //
                    // Alimentation des données pour chaque Span2CCPSCom
                    foreach (Span2CCPCom ccpCom in pMethodComObj.Parameters)
                    {
                        CcpMarginAmounts ccpMarginAmounts = pMarginDetail.Ccps.FirstOrDefault(c => c.ClearingOrganizationId == ccpCom.ClearingOrganization);
                        if (ccpMarginAmounts != default(CcpMarginAmounts))
                        {
                            FillSpan2CCPComWithMarginAmounts(ccpCom, ccpMarginAmounts.CurrencyAmts);
                            //
                            if (ccpMarginAmounts.Pod != default(PodMarginAmounts[]))
                            {
                                ccpCom.Parameters = (from pod in ccpMarginAmounts.Pod
                                                     select new Span2PodCom { ContractGroup = pod.PodId }).ToArray();
                                //
                                // Alimentation des données pour chaque Span2PodCom
                                foreach (Span2PodCom podCom in ccpCom.Parameters)
                                {
                                    PodMarginAmounts podMarginAmounts = ccpMarginAmounts.Pod.FirstOrDefault(p => p.PodId == podCom.ContractGroup);
                                    //
                                    if (podMarginAmounts != default(PodMarginAmounts))
                                    {
                                        FillSpan2PodComWithPodMarginAmounts(podCom, podMarginAmounts);
                                        //
                                        if (podMarginAmounts.ProductGroup != default(ProductGroupAmounts[]))
                                        {
                                            podCom.Parameters = (from productGroup in podMarginAmounts.ProductGroup
                                                                 select new Span2ProductGroupCom { ProductGroup = productGroup.ProductGroupId }).ToArray();
                                            //
                                            // Alimentation des données pour chaque Span2ProductGroupCom
                                            foreach (Span2ProductGroupCom prodGrpCom in podCom.Parameters)
                                            {
                                                ProductGroupAmounts prdGrpAmounts = podMarginAmounts.ProductGroup.FirstOrDefault(p => p.ProductGroupId == prodGrpCom.ProductGroup);
                                                //
                                                if (prdGrpAmounts != default(ProductGroupAmounts))
                                                {
                                                    FillSpan2ProductGroupComWithProductGroupAmounts(prodGrpCom, prdGrpAmounts);
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe pour les messages d'erreur
    public sealed class SPAN2Error
    {
        #region Members
        private readonly string m_Message;
        private readonly SysMsgCode m_SysMsgCode;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Message d'erreur
        /// </summary>
        public string Message
        {
            get { return m_Message; }
        }

        /// <summary>
        /// Code du message d'erreur
        /// </summary>
        public SysMsgCode MsgCode
        {
            get { return m_SysMsgCode; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pSysMsgCode"></param>
        /// <param name="pMessage"></param>
        public SPAN2Error(SysMsgCode pSysMsgCode, string pMessage = default)
        {
            m_Message = pMessage;
            m_SysMsgCode = pSysMsgCode;
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Retourne l'erreur sous la forme d'un tuple pour l'envoie dans le log
        /// </summary>
        /// <returns></returns>
        // PM 20230830 [26470] Ajout
        public (SysMsgCode, List<LogParam>) GetErrorForLog()
        {
            List<LogParam> errParam = new List<LogParam>();
            if (m_Message != default)
            {
                errParam.Add(new LogParam(m_Message));
            }
            return (m_SysMsgCode, errParam);
        }
        #endregion Methods
    }

    /// <summary>
    /// Méthode de calcul SPAN2 via CME CORE Margin Service API
    /// </summary>
    public sealed class SPAN2CoreApiMethod : SPAN2Method
    {
        #region Members
        /// <summary>
        /// Message d'erreur present en retour du CME lorsqu'un asset n'est pas trouvé
        /// </summary>
        private readonly static string m_MsgErrContractNotFind = "Could not find matching contract";
        /// <summary>
        /// Message d'erreur present en retour du CME lorsque le portefeuille n'a aucun trade valide
        /// </summary>
        private readonly static string m_MsgErrNoValidTrades = "has no valid trades";
        /// <summary>
        /// Message d'erreur en retour du CME lorsque la date de calcul demandée n'est pas disponible
        /// </summary>
        private readonly static string m_MsgErrInvalidDate = "Invalid margin date";

        /// <summary>
        /// Classe de gestion du SPAN 2 CME CORE Margin Service API
        /// </summary>
        private readonly SPAN2CoreAPI m_SPAN2CoreAPI;
        #endregion Members

        #region Override base accessors
        /// <summary>
        /// Type de la Methode
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.SPAN_2_CORE; }
        }
        #endregion Override base accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public SPAN2CoreApiMethod() : base()
        {
            m_SPAN2CoreAPI = new SPAN2CoreAPI(m_HttpClientHelper);
        }
        #endregion Constructor

        #region Override base methods
        /// <summary>
        /// Charge les paramètres spécifiques à la méthode.
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// <param name="pAssetETDCache">Collection d'assets contenant tous les assets en position</param>
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            base.LoadSpecificParameters(pCS, pAssetETDCache);

            //SPAN2CoreAPI.SetCoreServiceAccess(@"https://cmecorenr.cmegroup.com/MarginServiceApi/1.14/", "API_EFS_TEST", "APIefs.2021", "http://cmegroup.com/schema/core/1.14");
            m_SPAN2CoreAPI.SetCoreServiceAccess(m_ImMethodParameter.BaseUrl, m_ImMethodParameter.UserId, m_ImMethodParameter.Pwd, m_ImMethodParameter.CMECoreNamespace);
        }

        /// <summary>
        /// Evaluation du déposit via SPAN2 Core Api
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pMarginData">Données nécessaire au calcul</param>
        /// <returns></returns>
        protected override List<Money> EvaluateSpan2(SPAN2CalcMethCom pMethodComObj, SPAN2MarginData pMarginData)
        {
            List<Money> marginAmountList = default;

            if ((pMarginData != default(SPAN2MarginData)) && (pMethodComObj != default(SPAN2CalcMethCom)))
            {
                try
                {
                    bool isStatusFutOk = m_SPAN2CoreAPI.NewSession(pMarginData);

                    if (isStatusFutOk)
                    {
                        //#if DEBUG
                        //                        Test(pMethodComObj, pMarginData);
                        //                        return marginAmountList;
                        //#endif

                        // Calcul avec l'envoie des trades au format CSV  pour tous les comptes
                        //if (pMarginData.IsOmnibus)
                        //{
                        //    marginAmountList = EvaluateSpan2TransactionCSV(pMethodComObj, pMarginData);
                        //}
                        //else
                        //{
                        //    marginAmountList = EvaluateSpan2AllInOne(pMethodComObj, pMarginData);
                        //}
                        marginAmountList = EvaluateSpan2TransactionCSV(pMethodComObj, pMarginData);
                    }
                    else
                    {
                        // PM 20220930 [XXXXX] Passage de SYS-01034 à CME-02570 sur pMethodComObj
                        // PL 20221024 [XXXXX] Passage de CME-02570 à CME-00001 
                        // Erreur: Service indisponible
                        pMethodComObj.ErrorCode = new SysMsgCode(SysCodeEnum.CME, 1);
                        pMethodComObj.Missing = true;
                        pMethodComObj.IsIncomplete = true;

                        // PM 20230830 [26470] Ajout de l'erreur dans ErrorList
                        pMarginData.AddError(pMethodComObj.ErrorCode);
                    }
                }
                catch (Exception e)
                {
                    pMarginData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "EvaluateSpan2" + e.Message);
                    pMethodComObj.Missing = true;
                    pMethodComObj.IsIncomplete = true;
                }
            }
            return marginAmountList;
        }
        #endregion Override base methods

        #region Methods
        /// <summary>
        /// Evaluation du déposit via SPAN2 Core Api et requête All In One
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pMarginData">Données nécessaire au calcul</param>
        /// <returns></returns>
        private List<Money> EvaluateSpan2AllInOne(SPAN2CalcMethCom pMethodComObj, SPAN2MarginData pMarginData)
        {
            List<Money> marginAmountList = default;

            if ((pMarginData != default(SPAN2MarginData)) && (pMethodComObj != default(SPAN2CalcMethCom)))
            {
                try
                {
                    int currentTry = 1;
                    int nbTry = 1;

                    do
                    {
                        // Génération et envoie de la requete de calcul
                        marginReportMessage mrgCompleteRep = m_SPAN2CoreAPI.MarginCompleteJson(pMarginData, false);

                        // Compteur de position du message de calcul
                        pMethodComObj.CounterInfo.NbSpanRiskPosition = pMarginData.NbSpanRiskPosition;

                        if (mrgCompleteRep != default)
                        {
                            pMarginData.ApiPortfolioId = string.Empty;
                            if (mrgCompleteRep.margin != default(margin[]))
                            {
                                margin mrg = mrgCompleteRep.margin.FirstOrDefault();
                                if (mrg != default(margin))
                                {
                                    pMarginData.ApiMarginId = mrg.id;
                                    pMarginData.ApiPortfolioId = mrg.portfolioId;
                                }
                            }

                            if (mrgCompleteRep.error != default(error))
                            {
                                string errMsg = mrgCompleteRep.error.msg;

                                if (errMsg.Contains(m_MsgErrNoValidTrades))
                                {
                                    //Portfolio 999999999 has no valid trades
                                    MatchCollection matches = Regex.Matches(errMsg, @"(\d+)");
                                    if (matches != default(MatchCollection) && matches.Count == 1)
                                    {
                                        string idPortfolio = matches[0].Value;
                                        if (int.TryParse(idPortfolio, out int id))
                                        {
                                            pMarginData.ApiPortfolioId = idPortfolio;
                                        }
                                    }
                                }
                                // Test s'il faut réessayer d'effectuer le calcul en écartant les positions sur des assets erronés
                                else if (m_ImMethodParameter.IsTryExcludeWrongPosition && (currentTry == 1) && (errMsg.Contains(m_MsgErrContractNotFind)))
                                {
                                    // Supprimer le portefeuille avant d'essayer d'écarter les positions erronées
                                    m_SPAN2CoreAPI.DeletePortfolio(pMarginData);
                                    // Ecarter les positions erronées
                                    DiscardWrongPosition(pMethodComObj, pMarginData);
                                    if (pMarginData.GoodPosition.Count > 0)
                                    {
                                        // Tentter de nouveau le calcul
                                        pMarginData.MarginPosition = pMarginData.GoodPosition;
                                        nbTry += 1;
                                    }
                                }
                                // En cas d'erreur: ajouter le message d'erreur s'il n'y a qu'une seul itération ou que l'on est à la deuxième itération
                                if ((currentTry == 2) || (nbTry == 1))
                                {
                                    // Supprimer le portefeuille
                                    m_SPAN2CoreAPI.DeletePortfolio(pMarginData);

                                    //Erreur: Le service a retourné une erreur
                                    pMarginData.AddError(new SysMsgCode(SysCodeEnum.CME, 2), $"Error {mrgCompleteRep.error.code}: {mrgCompleteRep.error.msg}");
                                    pMethodComObj.ErrorCode = new SysMsgCode(SysCodeEnum.CME, 2);
                                    pMethodComObj.Missing = true;
                                    pMethodComObj.IsIncomplete = true;
                                }
                            }
                            else
                            {
                                // Attends l'achèvement du calcul et obtention du résultat
                                bool isMarginAvailable = m_SPAN2CoreAPI.WaitAndGetMargin(pMethodComObj, pMarginData, mrgCompleteRep.margin);
                                if (isMarginAvailable)
                                {
                                    // Interprétation du résultat
                                    marginAmountList = ReadMarginAmount(pMethodComObj, pMarginData);

                                    // Le déposit est calculé mais des positions ont été écartées
                                    if ((currentTry == 2) && (pMarginData.DiscartedPosition.Count > 0))
                                    {
                                        pMethodComObj.ErrorCode = new SysMsgCode(SysCodeEnum.SYS, 1034);
                                        pMethodComObj.Missing = true;

                                        // PM 20230830 [26470] Ajout de l'erreur
                                        pMarginData.AddError(pMethodComObj.ErrorCode);
                                    }
                                }
                                // PM 20230901 [XXXXX] Ajout suppression du portefeuille
                                m_SPAN2CoreAPI.DeletePortfolio(pMarginData);
                            }
                        }
                        else
                        {
                            // PM 20230830 [XXXXX] Ajout SysMsgCode
                            pMarginData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "Error during sending margin message");
                            pMethodComObj.Missing = true;
                            pMethodComObj.IsIncomplete = true;
                            // Sortir de la boucle
                            currentTry = nbTry;
                        }
                        currentTry += 1;
                    }
                    while (currentTry <= nbTry);
                }
                catch (Exception e)
                {
                    pMarginData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "EvaluateSpan2AllInOne" + e.Message);
                    pMethodComObj.Missing = true;
                    pMethodComObj.IsIncomplete = true;
                }
                finally
                {
                    // Alimentation du com de log avec la requete Xml
                    pMethodComObj.XmlRequestMessage = pMarginData.XmlRequestMessage;

                    // Alimentation du com de log avec le message XML reçue en réponse
                    pMethodComObj.XmlResponseMessage = pMarginData.XmlResponseMessage;

                    // Alimentation du com de log avec la requete Json
                    pMethodComObj.JsonRequestMessage = pMarginData.JsonRiskPortfolioRequestMessage;

                    // Alimentation du com de log avec la réponse Json
                    pMethodComObj.JsonResponseMessage = pMarginData.JsonMarginDetailResponseMessage;
                }
            }
            return marginAmountList;
        }

        /// <summary>
        /// Evaluation du déposit via SPAN2 Core Api et requête add transactions CSV
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pMarginData">Données nécessaire au calcul</param>
        /// <returns></returns>
        private List<Money> EvaluateSpan2TransactionCSV(SPAN2CalcMethCom pMethodComObj, SPAN2MarginData pMarginData)
        {
            List<Money> marginAmountList = default;

            if ((pMarginData != default(SPAN2MarginData)) && (pMethodComObj != default(SPAN2CalcMethCom)))
            {
                try
                {
                    // Génération et envoie de la requete de création de la position au format CSV
                    transactionReportMessage transRptMsg = m_SPAN2CoreAPI.AddTransactionsCSV(pMarginData);

                    // Compteur de position du message de calcul
                    pMethodComObj.CounterInfo.NbSpanRiskPosition = pMarginData.NbSpanRiskPosition;

                    if (transRptMsg != default)
                    {
                        if (transRptMsg.error != default(error))
                        {
                            pMarginData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "Error status in transaction response message");

                            //Erreur: Le service a retourné une erreur
                            pMarginData.AddError(new SysMsgCode(SysCodeEnum.CME, 2), $"Error {transRptMsg.error.code}: {transRptMsg.error.msg}");
                            pMethodComObj.ErrorCode = new SysMsgCode(SysCodeEnum.CME, 2);
                            pMethodComObj.Missing = true;
                            pMethodComObj.IsIncomplete = true;
                        }
                        else
                        {
                            if (transRptMsg.status != syncReportStatus.ERROR)
                            {
                                if ((transRptMsg.transaction != default(transaction[])) && (transRptMsg.transaction.Count() > 0))
                                {
                                    // Prendre la première transaction (trade) pour y lire l'Id du portefeuille
                                    transaction trans = transRptMsg.transaction[0];
                                    if (trans != default(transaction))
                                    {
                                        bool isToCompute = true;

                                        // Lecture du PortfolioId utilisé ensuite pour la demande de calcul
                                        pMarginData.ApiPortfolioId = trans.portfolioId;

                                        if (transRptMsg.status == syncReportStatus.SUCCESS_WITH_ERRORS)
                                        {
                                            if ((transRptMsg.transaction != default(transaction[])) && (transRptMsg.transaction.Count() > 0))
                                            {
                                                int nbErrorTrans = transRptMsg.transaction.Count(t => t.error != default(error));
                                                if (nbErrorTrans > 0)
                                                {

                                                    if (nbErrorTrans == pMarginData.NbSpanRiskPosition)
                                                    {
                                                        isToCompute = false;

                                                        // Erreur: Calcul sur position partielle
                                                        pMethodComObj.Missing = true;
                                                        pMethodComObj.ErrorCode = new SysMsgCode(SysCodeEnum.SYS, 1034);
                                                        pMarginData.AddError(pMethodComObj.ErrorCode);

                                                        // Tous les trades sont erronés
                                                        // Suppression du portefeuille
                                                        m_SPAN2CoreAPI.DeletePortfolio(pMarginData);

                                                        pMarginData.DiscartedPosition.AddRange(pMarginData.MarginPosition);
                                                        pMarginData.MarginPosition = new List<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>>();
                                                    }
                                                    else
                                                    {
                                                        foreach (transaction transact in transRptMsg.transaction)
                                                        {
                                                            if (transact.error != default(error))
                                                            {
                                                                pMarginData.AddError(transact.error);
                                                                pMethodComObj.Missing = true;

                                                                m_SPAN2CoreAPI.DeleteTransaction(pMarginData, transact);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (isToCompute)
                                        {
                                            // Demande de procéder au calcul sur le portfeuille
                                            marginReportMessage mrgRptMsg = m_SPAN2CoreAPI.CalcMargin(pMarginData);

                                            // Gestion demande de calcul avec dernières données de risque en cas de date invalide sur première demande
                                            if (mrgRptMsg != default)
                                            {
                                                if ((mrgRptMsg.status == asyncReportStatus.ERROR) && (mrgRptMsg.error != default(error)))
                                                {
                                                    if (StrFunc.IsFilled(mrgRptMsg.error.msg) && mrgRptMsg.error.msg.Contains(m_MsgErrInvalidDate))
                                                    {
                                                        mrgRptMsg = m_SPAN2CoreAPI.CalcMargin(pMarginData, false);
                                                    }
                                                }
                                            }

                                            if (mrgRptMsg != default)
                                            {
                                                if ((mrgRptMsg.status == asyncReportStatus.ERROR) || (mrgRptMsg.status == asyncReportStatus.SUCCESS_WITH_ERRORS))
                                                {
                                                    // La demande de calcul a retourné une erreur
                                                    pMarginData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "Error status in margin response message");

                                                    if (mrgRptMsg.error != default(error))
                                                    {
                                                        //Erreur: Le service a retourné une erreur
                                                        pMarginData.AddError(new SysMsgCode(SysCodeEnum.CME, 2), $"Error {mrgRptMsg.error.code}: {mrgRptMsg.error.msg}");

                                                        pMethodComObj.ErrorCode = new SysMsgCode(SysCodeEnum.CME, 2);
                                                        pMethodComObj.Missing = true;
                                                        pMethodComObj.IsIncomplete = true;
                                                    }
                                                }
                                                if (mrgRptMsg.status != asyncReportStatus.ERROR)
                                                {
                                                    // Attends l'achèvement du calcul et obtention du résultat
                                                    bool isMarginAvailable = m_SPAN2CoreAPI.WaitAndGetMargin(pMethodComObj, pMarginData, mrgRptMsg.margin);
                                                    if (isMarginAvailable)
                                                    {
                                                        // Interprétation du résultat
                                                        marginAmountList = ReadMarginAmount(pMethodComObj, pMarginData);

                                                        // Suppression du portefeuille
                                                        m_SPAN2CoreAPI.DeletePortfolio(pMarginData);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                pMarginData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "Error during sending calculation message");
                                                pMethodComObj.Missing = true;
                                                pMethodComObj.IsIncomplete = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        pMarginData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "No transaction returned");
                                        pMethodComObj.Missing = true;
                                        pMethodComObj.IsIncomplete = true;
                                    }
                                }
                            }
                            else
                            {
                                pMarginData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "Error status in transaction response message");

                                //Erreur: Le service a retourné une erreur
                                string msg = transRptMsg.error != default(error) ? $"Error {transRptMsg.error.code}: {transRptMsg.error.msg}" : "";
                                pMarginData.AddError(new SysMsgCode(SysCodeEnum.CME, 2), msg);
                                pMethodComObj.ErrorCode = new SysMsgCode(SysCodeEnum.CME, 2);
                                pMethodComObj.Missing = true;
                                pMethodComObj.IsIncomplete = true;
                            }
                        }
                    }
                    else
                    {
                        pMarginData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "Error during sending transaction message");
                        pMethodComObj.Missing = true;
                        pMethodComObj.IsIncomplete = true;
                    }
                }
                catch (Exception e)
                {
                    pMarginData.AddError(new SysMsgCode(SysCodeEnum.SYS, 1035), "EvaluateSpan2TransactionCSV" + e.Message);
                    pMethodComObj.Missing = true;
                    pMethodComObj.IsIncomplete = true;
                }
                finally
                {
                    // Alimentation du com de log avec les requetes Xml
                    pMethodComObj.XmlRequestMessage = pMarginData.XmlRequestMessage;

                    // Alimentation du com de log avec le message XML reçue en réponse
                    pMethodComObj.XmlResponseMessage = pMarginData.XmlResponseMessage;

                    // Alimentation du com de log avec la réponse Json
                    pMethodComObj.JsonResponseMessage = pMarginData.JsonMarginDetailResponseMessage;
                }
            }
            return marginAmountList;
        }

        /// <summary>
        /// Essaie d'écarter la position dont les assets ne sont pas valident
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pMarginData"></param>
        private void DiscardWrongPosition(SPAN2CalcMethCom pMethodComObj, SPAN2MarginData pMarginData)
        {
            IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> savePosition = pMarginData.MarginPosition;
            if (savePosition.Count() > 1)
            {
                // Découpe de la position en deux parties
                IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>>[] halfPos = savePosition.Split(2).ToArray();

                // Traitement première moitié
                pMarginData.MarginPosition = (halfPos[0]).ToList();

                if (m_SPAN2CoreAPI.IsCalcMarginPossible(pMarginData))
                {
                    // Cette partie de la position est correct
                    pMarginData.AddToGoodPosition();
                }
                else
                {
                    // Il reste des position erronée sur cette partie: recommencer la recherche
                    DiscardWrongPosition(pMethodComObj, pMarginData);
                }

                // Traitement deuxième moitié
                pMarginData.MarginPosition = (halfPos[1]).ToList();

                if (m_SPAN2CoreAPI.IsCalcMarginPossible(pMarginData))
                {
                    // Cette partie de la position est correct
                    pMarginData.AddToGoodPosition();
                }
                else
                {
                    // Il reste des positions erronée sur cette partie: recommencer la recherche
                    DiscardWrongPosition(pMethodComObj, pMarginData);
                }
            }
            else
            {
                // Il ne reste plus qu'au plus une ligne de position qui est donc erronée
                pMarginData.AddToDiscardPosition();
            }
        }
        #endregion Methods

        #region Methods Test 
        /// <summary>
        /// Méthode de test uniquement
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pMarginData"></param>
        internal void Test(SPAN2MarginData pMarginData)
        {
            m_SPAN2CoreAPI.DeleteAllPortfolios(pMarginData, DateTime.Today.AddDays(1));
            _ = CSVTransaction.CreateAllCSVTransactions(pMarginData);

            marginReportMessage mrgCompleteJsonRep2 = m_SPAN2CoreAPI.MarginCompleteJson(pMarginData);

            if (mrgCompleteJsonRep2 != default)
            {
                pMarginData.ApiMarginId = mrgCompleteJsonRep2.margin[0].id;
            }
            else
            {
                pMarginData.ApiMarginId = string.Empty;
            }

            marginReportMessage getMrgRepComp2 = m_SPAN2CoreAPI.GetMargin(pMarginData);
            while (getMrgRepComp2.status == asyncReportStatus.PROCESSING)
            {
                Thread.Sleep(1000);

                getMrgRepComp2 = m_SPAN2CoreAPI.GetMargin(pMarginData);
            }

            foreach (margin mrg in getMrgRepComp2.margin)
            {
                foreach (riskPayload payload in mrg.payload)
                {
                    _ = m_SPAN2CoreAPI.GetRiskMarginDetail(pMarginData, payload.Value);
                }
            }

            _ = m_SPAN2CoreAPI.ListPortfolios(pMarginData);

            portfolioReportMessage addPortfolios = m_SPAN2CoreAPI.AddPortfolios(pMarginData);
            string porfolioId;
            if (addPortfolios.portfolio != default(portfolio[]))
            {
                porfolioId = addPortfolios.portfolio[0].id;
            }
            else
            {
                porfolioId = string.Empty;
            }

            transactionReportMessage transCSV = m_SPAN2CoreAPI.AddTransactionsCSV(pMarginData);

            transactionReportMessage transFixML = m_SPAN2CoreAPI.TestAddTransactionsFixML(pMarginData);

            transactionReportMessage transJson = m_SPAN2CoreAPI.AddTransactionsJson(pMarginData);

            marginReportMessage mrgRep2 = m_SPAN2CoreAPI.CalcMargin(pMarginData);

            if ((mrgRep2 != default) && (mrgRep2.margin != default(margin[])))
            {
                pMarginData.ApiMarginId = mrgRep2.margin[0].id;
                pMarginData.ApiPortfolioId = mrgRep2.margin[0].portfolioId;
            }
            else
            {
                pMarginData.ApiMarginId = string.Empty;
                pMarginData.ApiPortfolioId = string.Empty;
            }

            marginReportMessage getMrgRep = m_SPAN2CoreAPI.GetMargin(pMarginData);

            marginReportMessage marginList = m_SPAN2CoreAPI.GetMarginList(pMarginData);
        }
        #endregion Methods Test 
    }

    /// <summary>
    /// Méthode de calcul SPAN 2 via Deployable Margin Software
    /// </summary>
    public sealed class SPAN2MarginSoftwareMethod : SPAN2Method
    {
        #region Members
        /// <summary>
        /// Classe de gestion du SPAN 2 Deployable Margin Software
        /// </summary>
        private readonly SPAN2RiskMargin m_SPAN2RiskMargin;
        #endregion Members

        #region Override base accessors
        /// <summary>
        /// Type de la Methode
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.SPAN_2_SOFTWARE; }
        }
        #endregion Override base accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public SPAN2MarginSoftwareMethod() : base()
        {
            m_SPAN2RiskMargin = new SPAN2RiskMargin(m_HttpClientHelper);
        }
        #endregion Constructor

        #region Override base methods
        /// <summary>
        /// Charge les paramètres spécifiques à la méthode.
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// <param name="pAssetETDCache">Collection d'assets contenant tous les assets en position</param>
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            base.LoadSpecificParameters(pCS, pAssetETDCache);

            //m_SPAN2RiskMargin.SetRiskMarginServiceAccess(@"http://127.0.0.1:8082/", "API_EFS_TEST", "APIefs.2021");
            m_SPAN2RiskMargin.SetRiskMarginServiceAccess(m_ImMethodParameter.BaseUrl, m_ImMethodParameter.UserId, m_ImMethodParameter.Pwd);
        }

        /// <summary>
        /// Evaluation du déposit via SPAN2 Risk Margin Software
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pMarginData">Données nécessaire au calcul</param>
        protected override List<Money> EvaluateSpan2(SPAN2CalcMethCom pMethodComObj, SPAN2MarginData pMarginData)
        {
            _ = m_SPAN2RiskMargin.CalcMargin(pMarginData);

            // Interprétation du résultat
            List<Money> marginAmountList = ReadMarginAmount(pMethodComObj, pMarginData);
            return marginAmountList;
        }
        #endregion Override base methods
    }
}


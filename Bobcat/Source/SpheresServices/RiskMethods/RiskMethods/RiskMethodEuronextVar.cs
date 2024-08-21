using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EfsML.Business;
using EfsML.Enum;

using FixML.Enum;
using FixML.v50SP1.Enum;
using FpML.Enum;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Classe de calcul du déposit Euronext Var Based Margin
    /// </summary>
    // PM 20240423 [XXXXX] EuronextVarRiskMethod devient abstraite
    public abstract class EuronextVarRiskMethod : BaseMethod
    {
        #region Members
        #region Referentiel Parameters
        /// <summary>
        /// Données référentiel Assets ETD
        /// </summary>
        private IEnumerable<AssetExpandedParameter> m_AssetExpandedParameters = default;

        /// <summary>
        /// Données référentiel Assets sous-jacents des assets ETD
        /// </summary>
        private IEnumerable<AssetAllExpandedParameter> m_UnderlyingAssetExpandedParameters = default;
        /// <summary>
        /// Date Business
        /// </summary>
        private DateTime m_DtBusiness;
        /// <summary>
        /// Cotations des assets
        /// </summary>
        private List<AssetQuoteParameter> m_AssetQuoteParameters = null;
        /// <summary>
        /// Cotations des assets sous-jacent
        /// </summary>
        private List<AssetQuoteParameter> m_UnderlyingAssetQuoteParameters = null;
        /// <summary>
        /// Données issues des fichiers de RiskData
        /// </summary>
        private RiskDataLoadEuronextVar m_RiskDataLoadEuronextVar = null;
        #endregion Referentiel Parameters
        #endregion Members

        #region Override base accessors
        //// PM 20240423 [XXXXX] Supprimé: déplacé dans class héritée
        ///// <summary>
        ///// Type de la Methode
        ///// </summary>
        //public override InitialMarginMethodEnum Type
        //{
        //    get { return InitialMarginMethodEnum.EURONEXT_VAR_BASED; }
        //}
        #endregion Override base accessors

        #region Constructor
        //// PM 20240423 [XXXXX] Supprimé: déplacé dans class héritée
        ///// <summary>
        ///// Constructeur
        ///// </summary>
        //public EuronextVarRiskMethod()
        //{
        //    // Méthode utilisant les positions
        //    m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        //}
        #endregion Constructor

        #region Override base methods
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
                m_AssetExpandedParameters = LoadParametersAssetExpanded(connection);

                m_UnderlyingAssetExpandedParameters = LoadParametersUnderlyingAssetExpanded(connection);

                // Construction de la liste des asset
                m_AssetQuoteParameters = (
                    from asset in m_AssetExpandedParameters
                    select new AssetQuoteParameter
                    {
                        AssetId = asset.AssetId,
                        AssetCategoryEnum = Cst.UnderlyingAsset.ExchangeTradedContract
                    }).ToList();

                // Construction de la liste des asset sous-jacent
                m_UnderlyingAssetQuoteParameters = (
                    from asset in m_AssetExpandedParameters
                    group asset by new
                    {
                        AssetCategoryEnum = asset.UnderlyingAssetCategoryEnum,
                        AssetId = asset.UnderlyningAssetId,
                    }
                        into underlying
                    select new AssetQuoteParameter
                    {
                        AssetId = underlying.Key.AssetId,
                        AssetCategoryEnum = underlying.Key.AssetCategoryEnum,
                    }
                    ).ToList();

                // Lecture des cotations
                GetQuotes(pCS, m_AssetQuoteParameters, m_UnderlyingAssetQuoteParameters, pAssetETDCache);
            }
            if (pAssetETDCache != default(Dictionary<int, SQL_AssetETD>))
            {
                // Objet qui contiendra les paramètres de calcul lus lors de l'import
                m_RiskDataLoadEuronextVar = new RiskDataLoadEuronextVar(Type, pAssetETDCache.Values);

                // Lancement de l'import
                RiskDataImportTask import = new RiskDataImportTask(ProcessInfo.Process, IdIOTaskRiskData);
                import.ProcessTask(m_DtBusiness, m_RiskDataLoadEuronextVar);

                m_RiskDataLoadEuronextVar.DataFormatting();

                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1078), 1));
            }
        }

        /// <summary>
        /// Libère les paramètres spécifiques à la méthode.
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            if (null != m_AssetQuoteParameters)
            {
                m_AssetQuoteParameters.Clear();
            }
            if (null != m_UnderlyingAssetQuoteParameters)
            {
                m_UnderlyingAssetQuoteParameters.Clear();
            }
            m_AssetExpandedParameters = null;
            m_UnderlyingAssetExpandedParameters = null;
            m_AssetQuoteParameters = null;
            m_UnderlyingAssetQuoteParameters = null;
            m_RiskDataLoadEuronextVar = null;
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
        /// <returns>Le montant de déposit ici à zéro</returns>
        public override List<Money> EvaluateRiskElementSpecific(
            int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
            RiskData pRiskDataToEvaluate,
            out IMarginCalculationMethodCommunicationObject opMethodComObj)
        {
            List<Money> riskAmounts = new List<Money>();

            // Creation de l'objet de communication du détail du calcul
            EuronextVarCalcMethCom methodComObj = new EuronextVarCalcMethCom();
            opMethodComObj = methodComObj;                          // Affectation de l'objet de communication du détail du calcul en sortie
            methodComObj.MarginMethodType = this.Type;              // Affectation du type de méthode de calcul
            methodComObj.CssCurrency = m_CssCurrency;               // Affectation de la devise de calcul
            methodComObj.IdA = pActorId;                            // Affectation de l'id de l'acteur
            methodComObj.IdB = pBookId;                             // Affectation de l'id du book
            methodComObj.DtParameters = m_RiskDataLoadEuronextVar.EvaluationDate;
            methodComObj.Missing = false;
            methodComObj.IsIncomplete = false;
            methodComObj.CounterInfo.NbAssetParameters = m_AssetExpandedParameters.Count(); // Compteur de paramètres d'asset

            //methodComObj.EuronextVarParameters = new EuronextVarParametersCom
            //{
            //    OrdinaryConfidenceLevel = m_RiskDataLoadEuronextVar.Parameters.OrdinaryConfidenceLevel,
            //    StressedConfidenceLevel = m_RiskDataLoadEuronextVar.Parameters.StressedConfidenceLevel,
            //    DecorrelationParameter = m_RiskDataLoadEuronextVar.Parameters.DecorrelationParameter,
            //    OrdinaryWeight = m_RiskDataLoadEuronextVar.Parameters.OrdinaryWeight,
            //    StressedWeight = m_RiskDataLoadEuronextVar.Parameters.StressedWeight
            //};

            //(int TypeS, int TypeU) = m_RiskDataLoadEuronextVar.GetScenarioLookBackPeriod();

            //methodComObj.LookbackPeriod.TypeS = TypeS;
            //methodComObj.LookbackPeriod.TypeU = TypeU;

            //methodComObj.ObservationNumber.TypeS = m_RiskDataLoadEuronextVar.ScenarioObservationCount.TypeS;
            //methodComObj.ObservationNumber.TypeU = m_RiskDataLoadEuronextVar.ScenarioObservationCount.TypeU;
            
            CreateDetailCom(methodComObj, m_RiskDataLoadEuronextVar);

            if (pRiskDataToEvaluate != default(RiskData))
            {
                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsToEvaluate = pRiskDataToEvaluate.GetPositionAsEnumerablePair();

                // Compteur position initiale
                methodComObj.CounterInfo.NbInitialPosition = positionsToEvaluate.Count();
                if (methodComObj.CounterInfo.NbInitialPosition > 0)
                {
                    // Début de constitution de la position NET 
                    var groupedPositionsByIdAsset = PositionsGrouping.GroupPositionsByAsset(positionsToEvaluate);

                    // Ne garder que les positions dont la quantité est différente de 0
                    groupedPositionsByIdAsset = FilterPosition(groupedPositionsByIdAsset);

                    // Compteur position nette
                    methodComObj.CounterInfo.NbNettedPosition = groupedPositionsByIdAsset.Count();

                    // Coverage short call and short futures (this one modify the position quantity)
                    IEnumerable<CoverageSortParameters> inputCoverage = GetSortParametersForCoverage(groupedPositionsByIdAsset);

                    // Reduction de la position couverte
                    var coveredQuantities = ReducePosition(pActorId, pBookId, pDepositHierarchyClass, inputCoverage, ref groupedPositionsByIdAsset);
                    methodComObj.UnderlyingStock = coveredQuantities.Second;

                    // Compteur position réduite
                    methodComObj.CounterInfo.NbReducedPosition = groupedPositionsByIdAsset.Count();

                    // Ne garder que les positions dont la quantité est différente de 0
                    groupedPositionsByIdAsset = FilterPosition(groupedPositionsByIdAsset).ToArray();

                    if ((groupedPositionsByIdAsset.Count() > 0) && IsEvaluateRisk(methodComObj, groupedPositionsByIdAsset))
                    {
                        riskAmounts = EvaluateRisk(methodComObj, groupedPositionsByIdAsset, pActorId, pBookId);
                    }
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

            methodComObj.MarginAmounts = riskAmounts.ToArray();

            return riskAmounts;
        }

        /// <summary>
        /// Get a collection of sorting parameter needed by coverage strategies
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">Positions of the current risk element</param>
        /// <returns>A collection of sorting parameters in order to be used inside of the ReducePosition method</returns>
        protected override IEnumerable<CoverageSortParameters> GetSortParametersForCoverage(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            IEnumerable<CoverageSortParameters> covParam =
                from position in pGroupedPositionsByIdAsset
                join asset in m_AssetExpandedParameters on position.First.idAsset equals (int)asset.AssetId
                join quote in m_AssetQuoteParameters on asset.AssetId equals quote.AssetId
                where (quote.AssetCategoryEnum == Cst.UnderlyingAsset.ExchangeTradedContract)
                select
                new CoverageSortParameters
                {
                    AssetId = position.First.idAsset,
                    ContractId = (int)asset.ContractId,
                    MaturityYearMonth = decimal.Parse(asset.MaturityYearMonth),
                    Multiplier = asset.ContractMultiplier,
                    Quote = quote.Quote,
                    StrikePrice = asset.StrikePrice,
                    Type = RiskMethodExtensions.GetTypeFromCategoryPutCall(asset.CategoryEnum, asset.PutOrCall),
                };
            return covParam;
        }

        /// <summary>
        /// Lecture d'informations complémentaire pour les Marchés/Chambre de compensation utilisant la méthode courante 
        /// </summary>
        /// <param name="pEntityMarkets">La collection de entity/market attaché à la chambre de compensation courante</param>
        public override void BuildMarketParameters(IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        {
            base.BuildMarketParameters(pEntityMarkets);
        }
        #endregion Override base methods

        #region private Methods
        /// <summary>
        /// Création du détails des informations de log par sector
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pRiskData"></param>
        private static void CreateDetailCom(EuronextVarCalcMethCom pMethodComObj, RiskDataLoadEuronextVar pRiskData)
        {
            if ((pMethodComObj != default(EuronextVarCalcMethCom))
                && (pRiskData != default(RiskDataLoadEuronextVar))
                && (pRiskData.RiskDataSet != default(Dictionary<EuronextVarSector, RiskDataEuronextVarSet>))
                && (pRiskData.RiskDataSet.Count() > 0))
            {
                pMethodComObj.EuronextVarSectorDetail = (from set in pRiskData.RiskDataSet
                                                         select new EuronextVarCalcSectorCom
                                                         {
                                                             Sector = set.Key,
                                                             EuronextVarParameters = ( set.Value.Parameters == default(RiskDataEuronextVarParameter) 
                                                             ? default(EuronextVarParametersCom)
                                                             : new EuronextVarParametersCom
                                                             {
                                                                 OrdinaryConfidenceLevel = set.Value.Parameters.OrdinaryConfidenceLevel,
                                                                 StressedConfidenceLevel = set.Value.Parameters.StressedConfidenceLevel,
                                                                 DecorrelationParameter = set.Value.Parameters.DecorrelationParameter,
                                                                 OrdinaryWeight = set.Value.Parameters.OrdinaryWeight,
                                                                 StressedWeight = set.Value.Parameters.StressedWeight,
                                                                 HoldingPeriod = set.Value.Parameters.HoldingPeriod,
                                                                 SubPortfolioSeparatorDaysNumber = set.Value.Parameters.SubPortfolioSeparator,
                                                             }),
                                                             DeliveryParameters = (set.Value.ParametersDelivery == default(Dictionary<string, RiskDataEuronextVarParameterDelivery>)
                                                             ? default(EuronextVarDeliveryParametersCom[])
                                                             : from dlyParam in set.Value.ParametersDelivery.Values
                                                               select new EuronextVarDeliveryParametersCom
                                                               {
                                                                   ContractCode = dlyParam.SymbolCode,
                                                                   Currency = dlyParam.Currency,
                                                                   Sens = dlyParam.Side,
                                                                   ExtraPercentage = dlyParam.ExtraPercentage,
                                                                   MarginPercentage = dlyParam.MarginPercentage,
                                                                   FeePercentage = dlyParam.FeePercentage
                                                               }).ToArray(),
                                                             LookbackPeriod = set.Value.LookBackPeriod,
                                                             ObservationNumber = set.Value.ScenarioObservationCount,
                                                             LookbackPeriodDelivery = set.Value.LookBackPeriodDelivery,
                                                             ObservationNumberDelivery = set.Value.ScenarioDeliveryObservationCount,
                                                         }).ToArray();
            }
        }

        /// <summary>
        /// Retourne parmi <paramref name="pPosition"/> les positions dont la quantité est différente de 0
        /// </summary>
        /// <param name="pPosition"></param>
        /// <returns></returns>
        private static IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> FilterPosition(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPosition)
        {
            var ret = from item in pPosition
                      where (item.Second.Quantity != 0) || (item.Second.ExeAssQuantity != 0)
                      select item;

            return ret;
        }

        /// <summary>
        /// Retourne la position portant sur tous les assets du sector
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPosition">Position</param>
        /// <returns></returns>
        private IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> FilterSectorPosition(RiskDataEuronextVarSet pRiskDataSet, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPosition)
        {
            // Position classique
            var positionETD = from assetPos in pPosition
                              join assetETD in pRiskDataSet.AssetETD on assetPos.First.idAsset equals assetETD.IdAsset
                              join serie in pRiskDataSet.Series on assetPos.First.idAsset equals serie.Value.IdAsset
                              select assetPos;

            // Position en livraison non présente dans position classique
            var positionETDDly = from assetPos in pPosition
                                 join assetETDExt in m_AssetExpandedParameters on assetPos.First.idAsset equals assetETDExt.AssetId
                                 join assetUnl in m_UnderlyingAssetExpandedParameters on assetETDExt.UnderlyningAssetId equals assetUnl.AssetId
                                 join assetBC in pRiskDataSet.AssetBondCash on assetUnl.IsinCode equals assetBC.IsinCode 
                                 where (assetETDExt.UnderlyingAssetCategoryEnum == assetUnl.AssetCategoryEnum)
                                    && (assetPos.Second.ExeAssQuantity != 0)
                                    && (false == positionETD.Any(p => p.First.idAsset == assetPos.First.idAsset))
                                 select assetPos;
            
            // PM 20240620 [XXXXX] Distinct pour être certains de ne pas avoir de position doublé
            return positionETD.Concat(positionETDDly).Distinct();
        }

        /// <summary>
        /// Retourne la position portant sur les assets du sector pour le calcul du cas général
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPosition">Position</param>
        /// <returns></returns>
        private static IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> FilterSectorGeneralPosition(RiskDataEuronextVarSet pRiskDataSet, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPosition)
        {
            var positionETD = pPosition;

            if (pRiskDataSet.Sector == EuronextVarSector.LGY_COM)
            {
                positionETD = from assetPos in pPosition
                              join assetETD in pRiskDataSet.AssetETD on assetPos.First.idAsset equals assetETD.IdAsset
                              join serie in pRiskDataSet.Series on assetPos.First.idAsset equals serie.Value.IdAsset
                              where (serie.Value.SubPortfolio == "SUB1") || (serie.Value.SubPortfolio == "N/A") || (StrFunc.IsEmpty(serie.Value.SubPortfolio))
                              select assetPos;
            }
            return positionETD;
        }

        /// <summary>
        /// Retourne la position portant sur les assets appartement à un groupe de margin
        /// </summary>
        /// <param name="pGroup">Groupe de margin</param>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPosition">Position</param>
        /// <returns></returns>
        private static IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> FilterGroupPosition(string pGroup, RiskDataEuronextVarSet pRiskDataSet, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPosition)
        {
            var positionETD = pPosition;

            if (pRiskDataSet.Sector == EuronextVarSector.LGY_COM)
            {
                positionETD = from assetPos in pPosition
                              join assetETD in pRiskDataSet.AssetETD on assetPos.First.idAsset equals assetETD.IdAsset
                              join serie in pRiskDataSet.Series on assetPos.First.idAsset equals serie.Value.IdAsset
                              where (serie.Value.ProductGroup == pGroup)
                              select assetPos;
            }
            return positionETD;
        }

        /// <summary>
        /// Retourne la position portant sur les assets en livraison physique dont les échéances sont proche
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPosition">Position</param>
        /// <returns></returns>
        private static IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> FilterNearExpiryPosition(RiskDataEuronextVarSet pRiskDataSet, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPosition)
        {
            var positionETD = default(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>>);

            if (pRiskDataSet.Sector == EuronextVarSector.LGY_COM)
            {
                positionETD = from assetPos in pPosition
                              join assetETD in pRiskDataSet.AssetETD on assetPos.First.idAsset equals assetETD.IdAsset
                              join serie in pRiskDataSet.Series on assetPos.First.idAsset equals serie.Value.IdAsset
                              where (serie.Value.SubPortfolio == "SUB2")
                              select assetPos;
            }
            return positionETD;
        }

        /// <summary>
        /// Retourne la position portant sur les assets en livraison physique post échéance
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPosition">Position</param>
        /// <returns></returns>
        private static IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> FilterPhysicalDeliveryPosition(RiskDataEuronextVarSet pRiskDataSet, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPosition)
        {
            var positionETD = default(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>>);

            if (pRiskDataSet.Sector == EuronextVarSector.LGY_COM)
            {
                positionETD = from assetPos in pPosition
                              join assetETD in pRiskDataSet.AssetETD on assetPos.First.idAsset equals assetETD.IdAsset
                              join serie in pRiskDataSet.Series on assetPos.First.idAsset equals serie.Value.IdAsset
                              where (serie.Value.SubPortfolio == "SUB3")
                              select assetPos;
            }
            return positionETD;
        }

        /// <summary>
        /// Retourne la l'enssemble des groupes de margin des asset en position
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPosition">Position</param>
        /// <returns></returns>
        private static IEnumerable<string> ListGroupInPosition(RiskDataEuronextVarSet pRiskDataSet, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPosition)
        {
            IEnumerable<string> groupLst;
            if (pRiskDataSet.Sector == EuronextVarSector.LGY_COM)
            {
                groupLst = (
                    from assetPos in pPosition
                    join assetETD in pRiskDataSet.AssetETD on assetPos.First.idAsset equals assetETD.IdAsset
                    join serie in pRiskDataSet.Series on assetPos.First.idAsset equals serie.Value.IdAsset
                    select serie.Value.ProductGroup
                    ).Distinct();
            }
            else
            {
                groupLst = new List<string> { "N/A" };
            }
            return groupLst;
        }

        /// <summary>
        /// Retourne true s'il existe au minimum 1 position pour laquelle il est possible d'effectuer le calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pPositions"></param>
        private Boolean IsEvaluateRisk(EuronextVarCalcMethCom pMethodComObj, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            Boolean ret = false;

            int[] AssetETDId = m_RiskDataLoadEuronextVar.AssetETD.Select(x => x.IdAsset).ToArray();
            var positionsOpen = pPositions.Where(x => x.Second.Quantity > 0).Select(x => x);
            // Toutes les positions ouvertes avec asset inconnu (*) sont exclues
            // (*) non présent dans le fichier des scenarios
            var discartedPositions1 =
                from item in positionsOpen.Where(x => !ArrFunc.ExistInArray(AssetETDId.Cast<IComparable>().ToArray(), x.First.idAsset))
                select item;

            string[] AssetCashISINCode = m_RiskDataLoadEuronextVar.AssetBondCash.Select(x => x.IsinCode).ToArray();
            var positionsLiv = pPositions.Where(x => x.Second.ExeAssQuantity != 0).Select(x => x);
            // Toutes les positions échues non encore livrées sans sous-jacent sont exclues
            var discartedPositions2 = (
                from item in positionsLiv
                join assetETD in m_AssetExpandedParameters on item.First.idAsset equals assetETD.AssetId
                select new
                {
                    positions = item,
                    assetETD.UnderlyningAssetId
                }).Where(x => x.UnderlyningAssetId == 0).Select(x => x.positions);

            // Toutes les positions échues non encore livrées avec sous-jacent inconnu (*) sont exclues
            // (*) non présent dans le fichier des scenarios
            var discartedPositions3 = (
                from item in positionsLiv
                join assetETD in m_AssetExpandedParameters on item.First.idAsset equals assetETD.AssetId
                join assetUnderlying in m_UnderlyingAssetExpandedParameters on new { AssetId = assetETD.UnderlyningAssetId, AssetCategory = assetETD.UnderlyningAssetCategory }
                                                                            equals new { assetUnderlying.AssetId, assetUnderlying.AssetCategory }
                select new
                {
                    positions = item,
                    UnderlyningIsinCode = assetUnderlying.IsinCode
                }).Where(x => !ArrFunc.ExistInArray(AssetCashISINCode.Cast<IComparable>().ToArray(), x.UnderlyningIsinCode)).Select(x => x.positions);

            Pair<PosRiskMarginKey, RiskMarginPosition>[] discartedPositions =
                (discartedPositions1.Concat(discartedPositions2).Concat(discartedPositions3)).ToArray();

            pMethodComObj.Positions.DiscartedPositions = discartedPositions;
            pMethodComObj.Positions.Missing = (discartedPositions.Length > 0);
            pMethodComObj.CounterInfo.NbDiscartedPosition = discartedPositions.Length;

            ret = (pPositions.Count() > discartedPositions.Length);

            return ret;
        }

        /// <summary>
        ///  Evalue le montant de dépôt de garantie. Montant exprimé en EURO 
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pPositions">positions pour lesquelles calculer le déposit</param>
        /// <param name="pActorId">L'acteur de la position à évaluer</param>
        /// <param name="pBookId">Le book de la position à évaluer</param>
        /// <returns>Liste des montants calculés</returns>
        private List<Money> EvaluateRisk(EuronextVarCalcMethCom pMethodComObj, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions, int pActorId, int pBookId)
        {
            decimal totalMargin = 0;
            int[] idAssetToIgnore = pMethodComObj.Positions.DiscartedPositions.Select(x => x.First.idAsset).Distinct().ToArray();

            Pair<PosRiskMarginKey, RiskMarginPosition>[] positions = (
                    from item in pPositions.Where(x => !ArrFunc.ExistInArray(idAssetToIgnore.Cast<IComparable>().ToArray(), x.First.idAsset))
                    select item).ToArray();

            pMethodComObj.Positions.ConsideredPositions = positions;
            pMethodComObj.CounterInfo.NbConsideredPosition = pMethodComObj.Positions.ConsideredPositions.Length;

            if ((m_RiskDataLoadEuronextVar != default(RiskDataLoadEuronextVar))
                && (m_RiskDataLoadEuronextVar.RiskDataSet != default(Dictionary<EuronextVarSector, RiskDataEuronextVarSet>))
                && (m_RiskDataLoadEuronextVar.RiskDataSet.Count() > 0))
            {
                foreach (EuronextVarSector sector in m_RiskDataLoadEuronextVar.RiskDataSet.Keys)
                {
                    RiskDataEuronextVarSet riskDataSet = m_RiskDataLoadEuronextVar.RiskDataSet[sector];
                    EuronextVarCalcSectorCom sectorMethodComObj = pMethodComObj.EuronextVarSectorDetail.First(o => (o.Sector == sector));

                    Pair<PosRiskMarginKey, RiskMarginPosition>[] sectorPosition = FilterSectorPosition(riskDataSet, positions).ToArray();

                    decimal sectorTotalMargin = EvaluateRiskSector(pMethodComObj, sectorMethodComObj, riskDataSet, sectorPosition, pActorId, pBookId);

                    totalMargin += sectorTotalMargin;
                }
            }

            List<Money> ret = new List<Money>(1)
            {
                new Money(totalMargin, m_CssCurrency)
            };

            return ret;
        }

        /// <summary>
        /// Evalue le montant de dépôt de garantie pour un sector. Montant exprimé en EURO 
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log général</param>
        /// <param name="pSectorMethodComObj">Pour la construction du log du secteur</param>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPositions">Positions pour lesquelles calculer le déposit</param>
        /// <param name="pActorId">L'acteur de la position à évaluer</param>
        /// <param name="pBookId">Le book de la position à évaluer</param>
        /// <returns>Liste des montants calculés</returns>
        private decimal EvaluateRiskSector(EuronextVarCalcMethCom pMethodComObj, EuronextVarCalcSectorCom pSectorMethodComObj, RiskDataEuronextVarSet pRiskDataSet, 
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions, int pActorId, int pBookId)
        {
            decimal totalMarginSector = 0;

            if ((pPositions != default(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>>)) && (pPositions.Count() > 0))
            {
                decimal totalGroupAdditionalMargin = 0;

                // Selection de la position normale
                Pair<PosRiskMarginKey, RiskMarginPosition>[] sectorGeneralPosition = FilterSectorGeneralPosition(pRiskDataSet, pPositions).ToArray();

                // Calcul du Mark To Market
                decimal markToMarket = CalcMarkToMarket(pSectorMethodComObj, pRiskDataSet, sectorGeneralPosition);

                // Recherche des différents groupes de contrats
                List<string> groupLst = ListGroupInPosition(pRiskDataSet, sectorGeneralPosition).ToList();

                pSectorMethodComObj.EuronextVarGroupDetail = (from groupCode in groupLst select new EuronextVarCalcGroupCom { Group = groupCode }).ToArray();

                // Calcul Additional Margin de chaque groupe de contrats
                foreach (string group in groupLst)
                {
                    // Selection de la position normale du groupe
                    Pair<PosRiskMarginKey, RiskMarginPosition>[] groupPosition = FilterGroupPosition(group, pRiskDataSet, sectorGeneralPosition).ToArray();

                    EuronextVarCalcGroupCom groupMethodComObj = pSectorMethodComObj.EuronextVarGroupDetail.First(g => g.Group == group);

                    // Calcul de l'additional margin du groupe
                    decimal groupAdditionalMargin = EvaluateRiskGroup(pMethodComObj, groupMethodComObj, pRiskDataSet, groupPosition, pActorId, pBookId);

                    totalGroupAdditionalMargin += groupAdditionalMargin;
                }

                pSectorMethodComObj.SectorAdditionalMargin = ("Sum for all groups", new Money(totalGroupAdditionalMargin, m_CssCurrency));

                // Calcul du Total Margin de l'enssemble des groupes de contrats
                decimal totalMarginGroup = CalcSectorTotalMarginGroup(pSectorMethodComObj, totalGroupAdditionalMargin, markToMarket);

                decimal sumTotalMarginSector;
                if (pSectorMethodComObj.Sector == EuronextVarSector.LGY_COM)
                {
                    // Partie de calcul propre au secteur Commodity Euronext Legacy

                    // Selection de la position en livraison physique proche de l'échéance
                    Pair<PosRiskMarginKey, RiskMarginPosition>[] nearExpiryPosition = FilterNearExpiryPosition(pRiskDataSet, pPositions).ToArray();

                    // Calcul du Total Margin pour échéance de livraison rapprochée
                    decimal totalMarginNearExpiry = EvaluateRiskNearExpiry(pMethodComObj, pSectorMethodComObj, pRiskDataSet, nearExpiryPosition);

                    // Selection de la position en livraison physique proche de l'échéance
                    Pair<PosRiskMarginKey, RiskMarginPosition>[] physicalDeliveryPosition = FilterPhysicalDeliveryPosition(pRiskDataSet, pPositions).ToArray();

                    // Calcul du Total Margin pour livraison physique post échéance
                    decimal totalMarginPhysicalDelivery = EvaluateRiskPhysicalDelivery(pMethodComObj, pSectorMethodComObj, pRiskDataSet, physicalDeliveryPosition);

                    // Calcul du Total Margin du secteur
                    sumTotalMarginSector = totalMarginGroup + totalMarginNearExpiry + totalMarginPhysicalDelivery;
                }
                else
                {
                    // Le Total Margin du secteur correspond au Total Margin Groups
                    sumTotalMarginSector = totalMarginGroup;
                }
                
                pSectorMethodComObj.SectorTotalMargin = ("Sum", new Money(sumTotalMarginSector, m_CssCurrency));

                totalMarginSector = System.Math.Max(sumTotalMarginSector, 0);

                // Ici arrondi alors que non spécifié dans l'analyse
                totalMarginSector = new EFS_Round(RoundingDirectionEnum.Nearest, 2, totalMarginSector).AmountRounded;

                pSectorMethodComObj.TotalMargin = new EuronextVarCalcMoneyCom
                {
                    Value1 = totalMarginSector,
                    Value2 = 0,
                    Result = ("Max", new Money(totalMarginSector, m_CssCurrency))
                };
            }
            return totalMarginSector;
        }

        /// <summary>
        /// Evalue la partie du montant de dépôt de garantie pour un groupe du sector. Montant exprimé en EURO 
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log général</param>
        /// <param name="pSectorGroupMethodComObj">Pour la construction du log du groupe</param>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPositions">Positions pour lesquelles calculer le déposit</param>
        /// <param name="pActorId">L'acteur de la position à évaluer</param>
        /// <param name="pBookId">Le book de la position à évaluer</param>
        /// <returns></returns>
        private decimal EvaluateRiskGroup(EuronextVarCalcMethCom pMethodComObj, EuronextVarCalcGroupCom pSectorGroupMethodComObj, RiskDataEuronextVarSet pRiskDataSet,
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions, int pActorId, int pBookId)
        {
            (decimal TypeS, decimal TypeU) initialMargin = CalcExpectedShortfalls(pMethodComObj, pSectorGroupMethodComObj, pRiskDataSet, pPositions, pActorId, pBookId,
                                                                            out IEnumerable<EuronextVarScenarioPnLDecoGroup> PnLDecorrelation);

            (decimal TypeS, decimal TypeU) decorrelationAddOn = CalcDecorrelationAddOn(pSectorGroupMethodComObj, pRiskDataSet, PnLDecorrelation, initialMargin);

            decimal additionalMargin = CalcGroupAdditionalMargin(pSectorGroupMethodComObj, pRiskDataSet, initialMargin, decorrelationAddOn);

            return additionalMargin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pSectorMethodComObj"></param>
        /// <param name="pRiskDataSet"></param>
        /// <param name="pPositions"></param>
        /// <returns></returns>
        private decimal EvaluateRiskNearExpiry(EuronextVarCalcMethCom pMethodComObj, EuronextVarCalcSectorCom pSectorMethodComObj, RiskDataEuronextVarSet pRiskDataSet,
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            decimal totalMarginNearExpiry = 0;

            if ((pPositions != default(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>>)) && (pPositions.Count() > 0))
            {
                IEnumerable<decimal> nearExpiryMargin = CalcExpectedShortfallsAssetNearExpiry(pMethodComObj, pSectorMethodComObj, pRiskDataSet, pPositions);

                totalMarginNearExpiry = (nearExpiryMargin != default(IEnumerable<decimal>)) ? nearExpiryMargin.Sum() : 0;

                pSectorMethodComObj.NearExpiryTotalMargin = ("Sum for all assets", new Money(totalMarginNearExpiry, m_CssCurrency));
            }

            return totalMarginNearExpiry;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pSectorMethodComObj"></param>
        /// <param name="pRiskDataSet"></param>
        /// <param name="pPositions"></param>
        /// <returns></returns>
        private decimal EvaluateRiskPhysicalDelivery(EuronextVarCalcMethCom pMethodComObj, EuronextVarCalcSectorCom pSectorMethodComObj, RiskDataEuronextVarSet pRiskDataSet,
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            decimal totalMarginPhysicalDelivery = 0;

            if ((pPositions != default(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>>)) && (pPositions.Count() > 0))
            {
                IEnumerable<decimal> physicalDeliveryMargin = CalcExpectedShortfallsPhysicalDelivery(pMethodComObj, pSectorMethodComObj, pRiskDataSet, pPositions);

                totalMarginPhysicalDelivery = (physicalDeliveryMargin != default(IEnumerable<decimal>)) ? physicalDeliveryMargin.Sum() : 0;

                pSectorMethodComObj.PhysicalDeliveryTotalMargin = ("Sum", new Money(totalMarginPhysicalDelivery, m_CssCurrency));
            }

            return totalMarginPhysicalDelivery;
        }

        /// <summary>
        /// Calcul des Expected ShortFall pour échéance de livraison proche
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pSectorMethodComObj"></param>
        /// <param name="pRiskDataSet"></param>
        /// <param name="pPositions"></param>
        /// <returns></returns>
        private IEnumerable<decimal> CalcExpectedShortfallsAssetNearExpiry(EuronextVarCalcMethCom pMethodComObj, EuronextVarCalcSectorCom pSectorMethodComObj, RiskDataEuronextVarSet pRiskDataSet,
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            List<decimal> ret = new List<decimal>();

            if ((pPositions != default(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>>)) && (pPositions.Count() > 0))
            {
                RiskDataEuronextVarParameter parameters = pRiskDataSet.Parameters;

                EuronextVarScenarioCalc[] lstPoscalc = GetRiskDataEuronextVarScenarioCalc(pRiskDataSet, pPositions).ToArray();

                CheckAssetScenario(pMethodComObj, pRiskDataSet, lstPoscalc);

                // PnL chaque scenario et Asset
                IEnumerable<EuronextVarScenarioPnLIsinCode> PnLContract =
                    (from item in lstPoscalc.GroupBy(elem => new { elem.Serie.IsinCode, ScenarioType = elem.Scenario.Key.Type, ScenarioReferenceDate = elem.Scenario.Key.ReferenceDate })
                     let scenarioKeyItem = new RiskDataEuronextVarScenarioKey(item.Key.ScenarioType, item.Key.ScenarioReferenceDate)
                     select new EuronextVarScenarioPnLIsinCode(item.Key.IsinCode, scenarioKeyItem)
                     {
                         PnL = item.Sum(x => x.PosPnL)
                     });

                // Expected Shortfall de chaque Asset
                IEnumerable<string> IsinCodes = PnLContract.Select(c => c.IsinCode).Distinct();

                if (IsinCodes.Count() > 0)
                {
                    pSectorMethodComObj.EuronextVarNearExpiryDetail =
                    (from isin in IsinCodes
                     select new EuronextVarCalcPhyDlyNearExpiryCom
                     {
                         IsinCode = isin,
                         ExpectedShortfallAssetNearExpiry = (new EuronextVarExpectedShortfallCom(), new EuronextVarExpectedShortfallCom())
                     }).ToArray();

                    // Calcul de tous les Expected ShortFall
                    Dictionary<string, (decimal TypeS, decimal TypeU)> expectedShortFallAssetNearExpiry =
                        (from isin in IsinCodes
                         join nearExpiryCom in pSectorMethodComObj.EuronextVarNearExpiryDetail on isin equals nearExpiryCom.IsinCode
                         let pnlAssetItem = PnLContract.Where(x => x.IsinCode == isin)
                         let expectedShortfallAssetNearExpiryComItem = nearExpiryCom.ExpectedShortfallAssetNearExpiry
                         select new
                         {
                             Isin = isin,
                             TypeS = CalcExpectedShortfallFunc(pRiskDataSet, expectedShortfallAssetNearExpiryComItem.TypeS, "S", pnlAssetItem),
                             TypeU = CalcExpectedShortfallFunc(pRiskDataSet, expectedShortfallAssetNearExpiryComItem.TypeU, "U", pnlAssetItem)
                         }).ToDictionary(g => g.Isin, g => (g.TypeS, g.TypeU));

                    // Calcul des Increase Percentage et Floor Margin
                    foreach (string isin in IsinCodes)
                    {
                        decimal increasePct = 0;
                        decimal typeS = expectedShortFallAssetNearExpiry[isin].TypeS;
                        decimal typeU = expectedShortFallAssetNearExpiry[isin].TypeU;
                        decimal floorMargin = 0;
                        EuronextVarCalcPhyDlyNearExpiryCom nearExpiryCom = pSectorMethodComObj.EuronextVarNearExpiryDetail.First(x => x.IsinCode == isin);

                        if (pRiskDataSet.Series.TryGetValue(isin, out RiskDataEuronextVarSerie serie))
                        {
                            Pair<PosRiskMarginKey, RiskMarginPosition> position = pPositions.Where(p => (p.First.idAsset == serie.IdAsset)).FirstOrDefault();
                            if (position != default(Pair<PosRiskMarginKey, RiskMarginPosition>))
                            {
                                decimal quantity = position.Second.Quantity;
                                string side = (position.First.FixSide == SideEnum.Sell ? "S" : "L");

                                RiskDataEuronextVarParameterDelivery paramDly =
                                    pRiskDataSet.ParametersDelivery.Values.Where(
                                        p => ((p.SymbolCode == serie.ContractSymbol)
                                        && (p.Currency == serie.UnderlyingCurrency)
                                        && (p.Side == side))).FirstOrDefault();

                                if ((paramDly != default(RiskDataEuronextVarParameterDelivery)) && (paramDly.MarginPercentage != 0))
                                {
                                    decimal mrgPct = paramDly.MarginPercentage;
                                    int interval = pRiskDataSet.MarketCalendar.CountInterval(pRiskDataSet.EvaluationDate, serie.MaturityDate);

                                    if ((interval >= 0) && (pRiskDataSet.Parameters.HoldingPeriod >= 0))
                                    {
                                        increasePct = (decimal)(pRiskDataSet.Parameters.HoldingPeriod - interval) / (decimal)(pRiskDataSet.Parameters.HoldingPeriod + 1);
                                    }

                                    floorMargin = System.Math.Max(serie.CurrentValue * quantity * serie.Multiplier * mrgPct * increasePct, 0);
                                }
                            }
                        }

                        decimal riskMeasureMargin = CalcNearExpiryRiskMeasureMargin(nearExpiryCom, pRiskDataSet, (typeS, typeU));
                        decimal marginFloored = riskMeasureMargin + System.Math.Max(floorMargin - riskMeasureMargin, 0);
                        ret.Add(marginFloored);

                        nearExpiryCom.IncreasePercentage = increasePct;
                        nearExpiryCom.FloorMargin = ("Formula", floorMargin);
                        nearExpiryCom.NearExpiryMargin = ("Formula", marginFloored);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Calcul des Expected ShortFall de livraison physique
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pSectorMethodComObj"></param>
        /// <param name="pRiskDataSet"></param>
        /// <param name="pPositions"></param>
        /// <returns></returns>
        private IEnumerable<decimal> CalcExpectedShortfallsPhysicalDelivery(EuronextVarCalcMethCom pMethodComObj, EuronextVarCalcSectorCom pSectorMethodComObj, RiskDataEuronextVarSet pRiskDataSet,
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            List<decimal> ret = new List<decimal>();

            if ((pPositions != default(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>>)) && (pPositions.Count() > 0))
            {
                RiskDataEuronextVarParameter parameters = pRiskDataSet.Parameters;

                EuronextVarScenarioCalc[] lstPoscalc = GetRiskDataEuronextVarScenarioCalcDelivery(pRiskDataSet, pPositions).ToArray();

                CheckAssetScenario(pMethodComObj, pRiskDataSet, lstPoscalc);

                // PnL chaque scenario et Asset
                IEnumerable<EuronextVarScenarioPnLIsinCode> PnLContract =
                    (from item in lstPoscalc.GroupBy(elem => new { elem.Serie.IsinCode, ScenarioType = elem.Scenario.Key.Type, ScenarioReferenceDate = elem.Scenario.Key.ReferenceDate })
                     let scenarioKeyItem = new RiskDataEuronextVarScenarioKey(item.Key.ScenarioType, item.Key.ScenarioReferenceDate)
                     select new EuronextVarScenarioPnLIsinCode(item.Key.IsinCode, scenarioKeyItem)
                     {
                         PnL = item.Sum(x => x.PosPnL)
                     });

                // Expected Shortfall de chaque Asset
                IEnumerable<string> IsinCodes = PnLContract.Select(c => c.IsinCode).Distinct();

                if (IsinCodes.Count() > 0)
                {
                    pSectorMethodComObj.EuronextVarPhysicalDeliveryDetail =
                    (from isin in IsinCodes
                     select new EuronextVarCalcPhysicalDeliveryCom
                     {
                         IsinCode = isin,
                         ExpectedShortfallAssetDelivery = (new EuronextVarExpectedShortfallCom(), new EuronextVarExpectedShortfallCom())
                     }).ToArray();

                    // Calcul de tous les Expected ShortFall
                    Dictionary<string, (decimal TypeS, decimal TypeU)> expectedShortFallAssetDelivery =
                        (from isin in IsinCodes
                         join deliveryCom in pSectorMethodComObj.EuronextVarPhysicalDeliveryDetail on isin equals deliveryCom.IsinCode
                         let pnlAssetItem = PnLContract.Where(x => x.IsinCode == isin)
                         let expectedShortfallAssetDeliveryComItem = deliveryCom.ExpectedShortfallAssetDelivery
                         select new
                         {
                             Isin = isin,
                             TypeS = CalcExpectedShortfallFunc(pRiskDataSet, expectedShortfallAssetDeliveryComItem.TypeS, "S", pnlAssetItem),
                             TypeU = CalcExpectedShortfallFunc(pRiskDataSet, expectedShortfallAssetDeliveryComItem.TypeU, "U", pnlAssetItem)
                         }).ToDictionary(g => g.Isin, g => (g.TypeS, g.TypeU));

                    // Calcul des Increase Percentage et Floor Margin
                    foreach (string isin in IsinCodes)
                    {
                        decimal typeS = expectedShortFallAssetDelivery[isin].TypeS;
                        decimal typeU = expectedShortFallAssetDelivery[isin].TypeU;
                        decimal extraPct = 1;
                        decimal floorMargin = 0;
                        EuronextVarCalcPhysicalDeliveryCom deliveryCom = pSectorMethodComObj.EuronextVarPhysicalDeliveryDetail.First(x => x.IsinCode == isin);

                        if (pRiskDataSet.Series.TryGetValue(isin, out RiskDataEuronextVarSerie serie))
                        {
                            Pair<PosRiskMarginKey, RiskMarginPosition> position = pPositions.Where(p => (p.First.idAsset == serie.IdAsset)).FirstOrDefault();
                            if (position != default(Pair<PosRiskMarginKey, RiskMarginPosition>))
                            {
                                decimal quantity = position.Second.Quantity;
                                string side = (position.First.FixSide == SideEnum.Sell ? "S" : "L");

                                RiskDataEuronextVarParameterDelivery paramDly =
                                    pRiskDataSet.ParametersDelivery.Values.Where(
                                        p => ((p.SymbolCode == serie.ContractSymbol)
                                        && (p.Currency == serie.UnderlyingCurrency)
                                        && (p.Side == side))).FirstOrDefault();

                                if (paramDly != default(RiskDataEuronextVarParameterDelivery))
                                {
                                    decimal pct = (paramDly.MarginPercentage + paramDly.FeePercentage);
                                    
                                    floorMargin = System.Math.Max(serie.CurrentValue * quantity * serie.Multiplier * pct, 0);
                                    extraPct += paramDly.ExtraPercentage;
                                }
                            }
                        }

                        decimal riskMeasureMargin = CalcDeliveryRiskMeasureMargin(deliveryCom, pRiskDataSet, (typeS, typeU));
                        decimal increasedMargin = riskMeasureMargin * extraPct;
                        decimal marginFloored = increasedMargin + System.Math.Max(floorMargin - increasedMargin, 0);
                        ret.Add(marginFloored);

                        deliveryCom.ExtraPercentage = extraPct;
                        deliveryCom.IncreasedMargin = ("Formula", increasedMargin);
                        deliveryCom.FloorMargin = ("Formula", floorMargin);
                        deliveryCom.DeliveryMargin = ("Formula", marginFloored);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Calcul des expected shortfalls 
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pGroupMethodComObj">Pour la construction du log</param>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPositions">positions pour lesquelles calculer le déposit</param>
        /// <param name="pActorId">L'acteur de la position à évaluer</param>
        /// <param name="pBookId">Le book de la position à évaluer</param>
        /// <param name="opPnLDecoGroup">retourne le PnL par couple scenario, ss jacent</param>
        /// <returns></returns>
        private (decimal TypeS, decimal TypeU) CalcExpectedShortfalls(EuronextVarCalcMethCom pMethodComObj, EuronextVarCalcGroupCom pGroupMethodComObj, RiskDataEuronextVarSet pRiskDataSet,
        IEnumerable<Pair<PosRiskMarginKey,RiskMarginPosition>> pPositions, int pActorId, int pBookId,
        out IEnumerable<EuronextVarScenarioPnLDecoGroup> opPnLDecoGroup)
        {
            (decimal TypeS, decimal TypeU) ret;

            EuronextVarScenarioCalc[] lstPoscalc = GetRiskDataEuronextVarScenarioCalc(pRiskDataSet, pPositions).ToArray();

            CheckAssetScenario(pMethodComObj, pRiskDataSet, lstPoscalc);

            // PnL chaque scenario et groupe de decorrelation
            opPnLDecoGroup = (from item in lstPoscalc.GroupBy(elem => new { elem.Serie.DecorrelationGroup, ScenarioType = elem.Scenario.Key.Type, ScenarioReferenceDate = elem.Scenario.Key.ReferenceDate })
                                  let scenarioKeyItem = new RiskDataEuronextVarScenarioKey(item.Key.ScenarioType, item.Key.ScenarioReferenceDate)
                                  select new EuronextVarScenarioPnLDecoGroup(item.Key.DecorrelationGroup, scenarioKeyItem)
                                  {
                                      PnL = item.Sum(x => x.PosPnL)
                                  }).ToArray();

            // PnL du bookId pour chaque scenario
            EuronextVarScenarioPnLBook[] pnlBook = (from item in opPnLDecoGroup.GroupBy(elem => new { elem.ScenarioKey.Type, elem.ScenarioKey.ReferenceDate })
                                                    let scenarioKeyItem = new RiskDataEuronextVarScenarioKey(item.Key.Type, item.Key.ReferenceDate)
                                                    select new EuronextVarScenarioPnLBook(pActorId, pBookId, scenarioKeyItem)
                                                    {
                                                        PnL = item.Sum(x => x.PnL)
                                                    }).ToArray();

            ret.TypeS = CalcExpectedShortfallFunc(pRiskDataSet, pGroupMethodComObj.ExpectedShortfallBook.TypeS, "S", pnlBook);
            ret.TypeU = CalcExpectedShortfallFunc(pRiskDataSet, pGroupMethodComObj.ExpectedShortfallBook.TypeU, "U", pnlBook);

            return ret;
        }

        /// <summary>
        /// Contrôle qu'il existe bien le nombre des scenarions requis pour les assets ETD
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pLstPoscalc"></param>
        private void CheckAssetScenario(EuronextVarCalcMethCom pMethodComObj, RiskDataEuronextVarSet pRiskDataSet, IEnumerable<EuronextVarScenarioCalc> pLstPoscalc)
        {
            var nbScenarioGroupByAsset = (from item in pLstPoscalc
                                          .GroupBy(elem => new { elem.Serie.IdAsset, elem.Serie.IsinCode, ScenarioType = elem.Scenario.Key.Type })
                                          select new
                                          {
                                              Asset = new Tuple<int, string>(item.Key.IdAsset, item.Key.IsinCode),
                                              item.Key.ScenarioType,
                                              NbScenario = item.Count(),
                                          }).ToArray();

            var assetIncomplete = (from item in nbScenarioGroupByAsset.Where(x =>
                                                    (x.ScenarioType == "S" && x.NbScenario < pRiskDataSet.ScenarioCount.TypeS) ||
                                                    (x.ScenarioType == "U" && x.NbScenario < pRiskDataSet.ScenarioCount.TypeU)).GroupBy(x => x.Asset.Item1)
                                   select new EuronextVarAssetIncompleteCom()
                                   {
                                       Asset = nbScenarioGroupByAsset.Where(y => y.Asset.Item1 == item.Key).First().Asset,
                                       NbScenarioTypeS = nbScenarioGroupByAsset.Where(y => y.Asset.Item1 == item.Key && y.ScenarioType == "S").FirstOrDefault().NbScenario,
                                       NbScenarioTypeU = nbScenarioGroupByAsset.Where(y => y.Asset.Item1 == item.Key && y.ScenarioType == "U").FirstOrDefault().NbScenario
                                   }).ToArray();


            if (pMethodComObj.AssetIncomplet != default(EuronextVarAssetIncompleteCom[]))
            {
                pMethodComObj.AssetIncomplet = pMethodComObj.AssetIncomplet.Concat(assetIncomplete).ToArray();
            }
            else
            {
                pMethodComObj.AssetIncomplet = assetIncomplete;
            }
        }

        /// <summary>
        /// Caculation du decorrelation add-on
        /// </summary>
        /// <param name="pGroupMethodComObj">Pour la construction du log</param>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPnLDecorrelation">PnL par couple scenario, code de decorrelation</param>
        /// <param name="pExpectedShortfallsBook">expected shortfalls concernant le book</param>
        /// <returns></returns>
        private (decimal TypeS, decimal TypeU) CalcDecorrelationAddOn(EuronextVarCalcGroupCom pGroupMethodComObj, RiskDataEuronextVarSet pRiskDataSet, IEnumerable<EuronextVarScenarioPnLDecoGroup> pPnLDecorrelation, (decimal TypeS, decimal TypeU) pExpectedShortfallsBook)
        {
            (decimal TypeS, decimal TypeU) ret = default;

            string[] DecorrelationGroup = (from item in pPnLDecorrelation
                                       select item.DecorrelationGroup).Distinct().OrderBy(d => d).ToArray();

            pGroupMethodComObj.ExpectedShortfallDecorrelation =
                        (from item in DecorrelationGroup
                         select new
                         {
                             x = item,
                             y = new EuronextVarExpectedShortfallCom(),
                             z = new EuronextVarExpectedShortfallCom()
                         }).ToDictionary(g => g.x, g => (g.y, g.z));

            Dictionary<string, (decimal TypeS, decimal TypeU)> expectedShortFallDecorrelation = (from item in DecorrelationGroup
                                                                                              let pnlDecorrelationItem = pPnLDecorrelation.Where(x => x.DecorrelationGroup == item)
                                                                                              let expectedShortfallDecorrelationComItem = pGroupMethodComObj.ExpectedShortfallDecorrelation[item]
                                                                                              select new
                                                                                              {
                                                                                                  x = item,
                                                                                                  y = CalcExpectedShortfallFunc(pRiskDataSet, expectedShortfallDecorrelationComItem.TypeS, "S", pnlDecorrelationItem),
                                                                                                  z = CalcExpectedShortfallFunc(pRiskDataSet, expectedShortfallDecorrelationComItem.TypeU, "U", pnlDecorrelationItem)
                                                                                              }).ToDictionary(g => g.x, g => (g.y, g.z));

            decimal decorellationExpectedShortfallTypeS = expectedShortFallDecorrelation.Sum(x => x.Value.TypeS);
            decimal decorellationExpectedShortfallTypeU = expectedShortFallDecorrelation.Sum(x => x.Value.TypeU);

            pGroupMethodComObj.DecorellationExpectedShortfall.TypeS = ("sum", decorellationExpectedShortfallTypeS);
            pGroupMethodComObj.DecorellationExpectedShortfall.TypeU = ("sum", decorellationExpectedShortfallTypeU);

            ret.TypeS = DecorrelationAddOnFunc(pRiskDataSet.Parameters.DecorrelationParameter, decorellationExpectedShortfallTypeS, pExpectedShortfallsBook.TypeS);
            ret.TypeU = DecorrelationAddOnFunc(pRiskDataSet.Parameters.DecorrelationParameter, decorellationExpectedShortfallTypeU, pExpectedShortfallsBook.TypeU);

            pGroupMethodComObj.DecorrelationAddOnResult.TypeS = ("Formula", ret.TypeS);
            pGroupMethodComObj.DecorrelationAddOnResult.TypeU = ("Formula", ret.TypeU);

            return ret;
        }

        /// <summary>
        /// Calcul du MarkToMarket
        /// </summary>
        /// <param name="pSectorMethodComObj">Pour la construction du log</param>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPositions"></param>
        /// <returns></returns>
        private decimal CalcMarkToMarket(EuronextVarCalcSectorCom pSectorMethodComObj, RiskDataEuronextVarSet pRiskDataSet, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            /*********************************************************************************************************** 
             * Remarque les qtés nettes sont ici définies Qté Short - Qté Long. En Effet. 
             * Cela s'explique parce que la formule du total margin est => total margin = Additional Margin + MarkToMarket
             * Il n'y a pas soustraction du MarkToMarket mais bien une Addition
             *****************************************************************************************************************/

            decimal ret = decimal.Zero;

            // 1/ Positions Future en livraison physique échu non encore livré
            Pair<EuronextVarMarkToMarketPos, EuronextVarMarkToMarketPosCom>[] MTMFutLiv =
                (from pos in pPositions.Where(x => x.Second.ExeAssQuantity != 0)
                 join assetETD in m_AssetExpandedParameters.Where(x => x.CategoryEnum == CfiCodeCategoryEnum.Future) on pos.First.idAsset equals assetETD.AssetId
                 let qty = pos.Second.ExeAssQuantity
                 let quoteAssetETD = m_AssetQuoteParameters.Where(x => x.AssetId == assetETD.AssetId).FirstOrDefault()
                 let quoteAssetETDMissing = (quoteAssetETD == null) || (quoteAssetETD.SystemMsgInfo != null)
                 let quoteAssetETDValue = (quoteAssetETD != default) ? quoteAssetETD.Quote : decimal.Zero
                 let quoteAssetUnderlying = m_UnderlyingAssetQuoteParameters.Where(x => x.AssetId == assetETD.UnderlyningAssetId).FirstOrDefault()
                 let quoteAssetUnderlyingMissing = (quoteAssetUnderlying == null) || (quoteAssetUnderlying.SystemMsgInfo != null)
                 let quoteAssetUnderlyingValue = (quoteAssetUnderlying != default) ? quoteAssetUnderlying.Quote : decimal.Zero
                 let multiplier = assetETD.ContractMultiplier
                 let amount = (quoteAssetETDValue != decimal.Zero && quoteAssetUnderlyingValue != decimal.Zero) ? (quoteAssetUnderlyingValue - quoteAssetETDValue) * qty * multiplier : 0
                 let money = new Money(amount, assetETD.Currency)
                 select new Pair<EuronextVarMarkToMarketPos, EuronextVarMarkToMarketPosCom>(
                     new EuronextVarMarkToMarketPos(pos, money),
                     new EuronextVarMarkToMarketPosCom()
                     {
                         Position = pos,
                         Multiplier = multiplier,
                         PriceMissing = new Tuple<bool, SystemMSGInfo>(quoteAssetETDMissing, quoteAssetETD.SystemMsgInfo),
                         Price = quoteAssetETDValue,
                         UnderlyingPriceMissing = new Tuple<bool, SystemMSGInfo>(quoteAssetUnderlyingMissing, quoteAssetUnderlying.SystemMsgInfo),
                         UnderlyingPrice = quoteAssetUnderlyingValue,
                         MarkToMarket = money
                     })).ToArray();

            // 2/ Position ouverte Option 
            Pair<EuronextVarMarkToMarketPos, EuronextVarMarkToMarketPosCom>[] MTMOpt =
                (from pos in pPositions.Where(x => x.Second.Quantity > 0)
                 join assetETD in m_AssetExpandedParameters.Where(x => x.CategoryEnum == CfiCodeCategoryEnum.Option &&
                         x.ValuationMethodEnum == FuturesValuationMethodEnum.PremiumStyle) on pos.First.idAsset equals assetETD.AssetId
                 let qty = ((pos.First.FixSide == SideEnum.Buy) ? (-1 * pos.Second.Quantity) : pos.Second.Quantity)
                 let quoteAssetETD = m_AssetQuoteParameters.Where(x => x.AssetId == assetETD.AssetId).FirstOrDefault()
                 let quoteAssetETDMissing = (quoteAssetETD == null) || (quoteAssetETD.SystemMsgInfo != null)
                 let quoteAssetETDValue = (false == quoteAssetETDMissing) ? quoteAssetETD.Quote : decimal.Zero
                 let multiplier = assetETD.ContractMultiplier
                 let amount = quoteAssetETDValue * qty * multiplier
                 let money = new Money(amount, assetETD.Currency)
                 select new Pair<EuronextVarMarkToMarketPos, EuronextVarMarkToMarketPosCom>(
                     new EuronextVarMarkToMarketPos(pos, money),
                     new EuronextVarMarkToMarketPosCom()
                     {
                         Position = pos,
                         Multiplier = multiplier,
                         PriceMissing = new Tuple<bool, SystemMSGInfo>(quoteAssetETDMissing, quoteAssetETD.SystemMsgInfo),
                         Price = quoteAssetETDValue,
                         UnderlyingPrice = null,
                         MarkToMarket = money
                     })).ToArray();

            // 3/ Positions options exercées/assignées non encore livrée
            Pair<EuronextVarMarkToMarketPos, EuronextVarMarkToMarketPosCom>[] MTMOptLiv =
                (from pos in pPositions.Where(x => x.Second.ExeAssQuantity != 0)
                 join assetETD in m_AssetExpandedParameters.Where(x => x.CategoryEnum == CfiCodeCategoryEnum.Option) on pos.First.idAsset equals assetETD.AssetId
                 let qty = pos.Second.ExeAssQuantity
                 let quoteAssetUnderlying = m_UnderlyingAssetQuoteParameters.Where(x => x.AssetId == assetETD.UnderlyningAssetId).FirstOrDefault()
                 let quoteAssetUnderlyingMissing = (quoteAssetUnderlying == null) || (quoteAssetUnderlying.SystemMsgInfo != null)
                 let quoteAssetUnderlyingValue = (false == quoteAssetUnderlyingMissing) ? quoteAssetUnderlying.Quote : decimal.Zero
                 let multiplier = assetETD.ContractMultiplier
                 let amount = (quoteAssetUnderlyingValue != decimal.Zero) ?
                (((assetETD.PutOrCall.Value == PutOrCallEnum.Call) ? (quoteAssetUnderlyingValue - assetETD.StrikePrice) : (assetETD.StrikePrice - quoteAssetUnderlyingValue)) * qty * multiplier) : decimal.Zero
                 let money = new Money(amount, assetETD.Currency)
                 select new Pair<EuronextVarMarkToMarketPos, EuronextVarMarkToMarketPosCom>(
                     new EuronextVarMarkToMarketPos(pos, money),
                     new EuronextVarMarkToMarketPosCom()
                     {
                         Position = pos,
                         Multiplier = multiplier,
                         Price = null,
                         UnderlyingPriceMissing = new Tuple<bool, SystemMSGInfo>(quoteAssetUnderlyingMissing, quoteAssetUnderlying.SystemMsgInfo),
                         UnderlyingPrice = quoteAssetUnderlyingValue,
                         MarkToMarket = money
                     })).ToArray();

            Dictionary<string, decimal> sumFUtLiv = SumMarkToMarketPos(MTMFutLiv.Select(x => x.First));
            Dictionary<string, decimal> sumOpt = SumMarkToMarketPos(MTMOpt.Select(x => x.First));
            Dictionary<string, decimal> sumOptLiv = SumMarkToMarketPos(MTMOptLiv.Select(x => x.First));

            // Dans l'analyse on lit
            // "Le Mark to Market est exprimé dans la devise du contrat et doit être converti dans la devise de clearing en utilisant le taux de change classique"
            // Cette formulation présente dans l'analyse est issue de la doc de la chambre 
            // Avec PM nous avons acté de convertir en utilisant les taux de change présents dans le fichier forex tels que la date de référence est égale à dtEvaluationDate 

            DateTime dtEvaluationDate = DateTime.MinValue;
            if (ArrFunc.Count(pRiskDataSet.ScenarioFx) > 0)
            {
                dtEvaluationDate = pRiskDataSet.ScenarioFx.First().Value.EvaluationDate;
            }

            RiskDataEuronextVarScenarioKey scenarioKey = new RiskDataEuronextVarScenarioKey("S", dtEvaluationDate);

            ret += (from item in sumFUtLiv
                    let money = (item.Key == m_CssCurrency) ? item.Value : ConvertCSSCurrency(pRiskDataSet, item.Value, item.Key, scenarioKey)
                    select money).Sum();

            ret += (from item in sumOpt
                    let money = (item.Key == m_CssCurrency) ? item.Value : ConvertCSSCurrency(pRiskDataSet, item.Value, item.Key, scenarioKey)
                    select money).Sum();

            ret += (from item in sumOptLiv
                    let money = (item.Key == m_CssCurrency) ? item.Value : ConvertCSSCurrency(pRiskDataSet, item.Value, item.Key, scenarioKey)
                    select money).Sum();


            EuronextVarMarkToMarketPosCom[] MarkToMarketPos = (MTMFutLiv.Select(x => x.Second).Concat(MTMOpt.Select(x => x.Second)).Concat(MTMOptLiv.Select(x => x.Second))).ToArray();
            pSectorMethodComObj.MarkToMarket = new EuronextVarMarkToMarketCom()
            {
                MarkToMarketPos = MarkToMarketPos,
                Missing = (MarkToMarketPos.Where(x => x.MissingPrice == true).Count() > 0),
                MarkToMarketAmount = ("Sum", new Money(ret, m_CssCurrency))
            };

            return ret;
        }

        /// <summary>
        /// Calcul de l'additional Margin
        /// </summary>
        /// <param name="pGroupMethodComObj">Pour la construction du log</param>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pInitialMargin"></param>
        /// <param name="pDecorrelationAddOn"></param>
        /// <returns></returns>
        private static decimal CalcGroupAdditionalMargin(EuronextVarCalcGroupCom pGroupMethodComObj, RiskDataEuronextVarSet pRiskDataSet, 
            (decimal TypeS, decimal TypeU) pInitialMargin, (decimal TypeS, decimal TypeU) pDecorrelationAddOn)
        {
            RiskDataEuronextVarParameter param = pRiskDataSet.Parameters;

            decimal Value1 = param.OrdinaryWeight * (pInitialMargin.TypeS + pDecorrelationAddOn.TypeS) + param.StressedWeight * (pInitialMargin.TypeU + pDecorrelationAddOn.TypeU);
            decimal value2 = pInitialMargin.TypeS + pDecorrelationAddOn.TypeS;

            decimal ret = System.Math.Max(Value1, value2);

            pGroupMethodComObj.AdditionalMargin = new EuronextVarCalcCom
            {
                Value1 = Value1,
                Value2 = value2,
                Result = ("Max", ret)
            };

            return ret;
        }

        /// <summary>
        /// Calcul du RiskMeasure margin
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pScenarioMargin"></param>
        /// <returns></returns>
        private static (decimal MarginValue, EuronextVarCalcCom MarginCalcCom) CalcRiskMeasureMargin(RiskDataEuronextVarSet pRiskDataSet, (decimal TypeS, decimal TypeU) pScenarioMargin)
        {
            RiskDataEuronextVarParameter param = pRiskDataSet.Parameters;

            decimal Value1 = param.OrdinaryWeight * pScenarioMargin.TypeS + param.StressedWeight * pScenarioMargin.TypeU;
            decimal value2 = pScenarioMargin.TypeS;

            decimal ret = System.Math.Max(Value1, value2);

            EuronextVarCalcCom calcCom = new EuronextVarCalcCom
            {
                Value1 = Value1,
                Value2 = value2,
                Result = ("Max", ret)
            };

            return (ret, calcCom);
        }

        /// <summary>
        /// Calcul du risk measure margin pour échéance de livraison physique proche
        /// </summary>
        /// <param name="pNearExpiryMethodComObj">Pour la construction du log</param>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pNearExpiryMargin"></param>
        /// <returns></returns>
        private static decimal CalcNearExpiryRiskMeasureMargin(EuronextVarCalcPhyDlyNearExpiryCom pNearExpiryMethodComObj, RiskDataEuronextVarSet pRiskDataSet, (decimal TypeS, decimal TypeU) pNearExpiryMargin)
        {
            (decimal MarginValue, EuronextVarCalcCom MarginCalcCom) margin = CalcRiskMeasureMargin(pRiskDataSet, pNearExpiryMargin);

            pNearExpiryMethodComObj.RiskMeasureMargin = margin.MarginCalcCom;

            return margin.MarginValue;
        }

        /// <summary>
        /// Calcul du risk measure margin pour échéance de livraison physique
        /// </summary>
        /// <param name="pDeliveryMethodComObj">Pour la construction du log</param>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pDeliveryMargin"></param>
        /// <returns></returns>
        private static decimal CalcDeliveryRiskMeasureMargin(EuronextVarCalcPhysicalDeliveryCom pDeliveryMethodComObj, RiskDataEuronextVarSet pRiskDataSet, (decimal TypeS, decimal TypeU) pDeliveryMargin)
        {
            (decimal MarginValue, EuronextVarCalcCom MarginCalcCom) margin = CalcRiskMeasureMargin(pRiskDataSet, pDeliveryMargin);

            pDeliveryMethodComObj.RiskMeasureMargin = margin.MarginCalcCom;

            return margin.MarginValue;
        }

        /// <summary>
        /// Calcul du Total Margin
        /// </summary>
        /// <param name="pSectorMethodComObj">Pour la construction du log</param>
        /// <param name="pAdditionalMargin"></param>
        /// <param name="pMarkToMarket"></param>
        /// <returns></returns>
        private decimal CalcSectorTotalMarginGroup(EuronextVarCalcSectorCom pSectorMethodComObj, decimal pAdditionalMargin, decimal pMarkToMarket)
        {
            decimal value1 = pAdditionalMargin;
            decimal value2 = pMarkToMarket;

            decimal ret = value1 + value2;

            pSectorMethodComObj.GroupTotalMargin = new EuronextVarCalcMoneyCom
            {
                Value1 = value1,
                Value2 = value2,
                Result = ("Sum", new Money(ret, m_CssCurrency))
            };

            return ret;
        }

        /// <summary>
        /// Calcul du montant de décorrelation add-on
        /// </summary>
        /// <param name="pDdecorrelationParameter">Decorrelation Parameter</param>
        /// <param name="pExpectedShortfallsDecorrelation"></param>
        /// <param name="pExpectedShortfallsBook"></param>
        /// <returns></returns>
        private static decimal DecorrelationAddOnFunc(decimal pDdecorrelationParameter, decimal pExpectedShortfallsDecorrelation, decimal pExpectedShortfallsBook)
        {
            return (1 - pDdecorrelationParameter) * (pExpectedShortfallsDecorrelation - pExpectedShortfallsBook);
        }

        /// <summary>
        /// Effectue le somme des montant des <paramref name="pMarkToMarket"/>. Retourne un dictionnaire où la clé est une devise et la valeur la résultat de la somme 
        /// </summary>
        /// <param name="pMarkToMarket"></param>
        /// <returns></returns>
        private Dictionary<string, decimal> SumMarkToMarketPos(IEnumerable<EuronextVarMarkToMarketPos> pMarkToMarket)
        {
            return SumMonies(from item in pMarkToMarket select item.MTM);
        }

        /// <summary>
        /// Effectue le somme des <paramref name="monies"/>. Retourne un dictionnaire où la clé est une devise et la valeur la résultat de la somme 
        /// </summary>
        /// <param name="monies"></param>
        /// <returns></returns>
        private Dictionary<string, decimal> SumMonies(IEnumerable<Money> monies)
        {
            Dictionary<string, decimal> ret = default;
            if (monies != default)
            {

                ret = (from item in monies
                       group item by item.Currency into groupCurrency
                       select new
                       {
                           groupCurrency.Key,
                           Value = groupCurrency.ToList().Sum(x => x.Amount.DecValue),
                       }).ToDictionary(e => e.Key, e => e.Value);
            }
            else
            {
                ret = new Dictionary<string, decimal>(1)
                {
                    { this.m_CssCurrency, decimal.Zero }
                };
            }

            return ret;
        }

        /// <summary>
        ///  Détermination de l'expected Shortfall (Moyenne des valeurs extrêmes)
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pScenarioType"></param>
        /// <param name="pPnL"></param>
        /// <returns></returns>
        private static decimal CalcExpectedShortfallFunc(RiskDataEuronextVarSet pRiskDataSet, EuronextVarExpectedShortfallCom pEuronextVarExpectedShortfallCom, string pScenarioType,
            IEnumerable<EuronextVarScenarioPnLBase> pPnL)
        {
            if (false == (pScenarioType == "S" || pScenarioType == "U"))
            {
                throw new ArgumentException($"Value:{pScenarioType} not allowed", nameof(pScenarioType));
            }

            Dictionary<int, decimal> dicExtremeEvents = GetExtremeEvents(pRiskDataSet, pScenarioType, pPnL);
            // FI 20231005 [26527] mis en place d'un test count pour ne pas planter lors d'un Average
            decimal ret = decimal.Zero;
            if (dicExtremeEvents.Count > 0)
            {
                ret = dicExtremeEvents.Values.Average();
            }

            pEuronextVarExpectedShortfallCom.ExtremeEvents = dicExtremeEvents;
            pEuronextVarExpectedShortfallCom.Result = ("Average", ret);

            return ret;
        }

        /// <summary>
        /// Retourne les ensembles extrèmes 
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pScenarioType"></param>
        /// <param name="pScenarioPnL"></param>
        /// <returns></returns>
        private static Dictionary<int, decimal> GetExtremeEvents(RiskDataEuronextVarSet pRiskDataSet, string pScenarioType, IEnumerable<EuronextVarScenarioPnLBase> pScenarioPnL)
        {
            if (false == (pScenarioType == "S" || pScenarioType == "U"))
            {
                throw new ArgumentException($"Value:{pScenarioType} not allowed", nameof(pScenarioType));
            }

            int observationCount;
            switch (pScenarioType)
            {
                case "S":
                    observationCount = pRiskDataSet.ScenarioObservationCount.TypeS.intValue;
                    break;

                case "U":
                    observationCount = pRiskDataSet.ScenarioObservationCount.TypeU.intValue;
                    break;

                default:
                    throw new NotImplementedException($"Value:{pScenarioType} not implemented");
            }

            // PM 20230921 [XXXXX] Recherche des événements extrêmes sur l'ensemble des P&Ls et pas sur les ensembles réduits
            //// C'est normalement déja trié mais ...
            //IEnumerable<EuronextVarScenarioPnLBase> pnlScenarioOrder = from item in scenarioPnL.Where(x => x.ScenarioKey.Type == scenarioType)
            //                                                             orderby item.ScenarioKey.ReferenceDate ascending
            //                                                             select item;


            //DateTime[] referenceDate = (from item in pnlScenarioOrder
            //                            select item.ScenarioKey.ReferenceDate).ToArray();


            //Dictionary<int, EuronextVarScenarioPnLBase[]> dicSet = new Dictionary<int, EuronextVarScenarioPnLBase[]>(observationCount);
            //for (int i = 0; i < observationCount; i++)
            //{
            //    DateTime dt1 = referenceDate[i];
            //    DateTime dt2 = referenceDate[referenceDate.Length - observationCount - 1 + i];
            //    dicSet[i] = (from item in pnlScenarioOrder.Where(x => x.ScenarioKey.ReferenceDate >= dt1 && x.ScenarioKey.ReferenceDate <= dt2)
            //                select item).ToArray();
            //}


            //Dictionary<int, decimal> ret = new Dictionary<int, decimal>(observationCount);
            //for (int i = 0; i < observationCount; i++)
            //    ret[i] = GetMaxValue(dicSet[i].Select(x => x.PnL).ToList(), i);

            decimal[] decValuesOrdered = pScenarioPnL.Where(x => x.ScenarioKey.Type == pScenarioType).Select(x => x.PnL).OrderByDescending(x => x).ToArray();
            // FI 20231003 [26512] mis en place de nbValues pour ne plus planter 
            int nbValues = System.Math.Min(observationCount, decValuesOrdered.Length);

            Dictionary<int, decimal> ret = new Dictionary<int, decimal>(observationCount);
            for (int i = 0; i < nbValues; i += 1)
            {
                ret[i] = decValuesOrdered[i];
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="decValues"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static decimal GetMaxValue(List<decimal> decValues, int index)
        {
            if ((index < 0) || index >= decValues.Count)
                throw new ArgumentOutOfRangeException($"{nameof(index)}: Must be non-negative and less than the size of the collection");

            decimal[] decValuesOrder = decValues.OrderByDescending(x => x).ToArray();

            return decValuesOrder[index];
        }

        /// <summary>
        /// Retourne pour les scenarios utiles aux positions <paramref name="pPositions"/>. Pour chaque couple position,scenario des PnLs sont calculés.
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPositions">Positions pour lesquelles calculer le déposit</param>
        /// <returns></returns>
        private IEnumerable<EuronextVarScenarioCalc> GetRiskDataEuronextVarScenarioCalc(RiskDataEuronextVarSet pRiskDataSet, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            List<(PosRiskMarginKey Key, IEnumerable<EuronextVarScenarioCalc> ScenarioCalc)> lstPosCalc = new List<(PosRiskMarginKey, IEnumerable<EuronextVarScenarioCalc>)>();

            // 2 étapes puisque sur une option americaine on peut être, et en position, et en en cours de livraison 
            // Exemple trade de qté 100 initial et partiellement assigné de 40 => 1 position classique de 60 et 1 position en livraison de 40
            // Pour ces 2 cas les montants de PnL sont différents

            //Positions classique
            var positionsOpen = pPositions.Where(x => x.Second.Quantity > 0).Select(x => x);

            foreach (Pair<PosRiskMarginKey, RiskMarginPosition> pos in positionsOpen)
            {
                // seules sont considérées les positions sur assets ETD présents dans le fichier des scenarios (EURONEXTVARSCENARIOSFILE)
                RiskDataEuronextVarSerie serie = m_RiskDataLoadEuronextVar.AssetETD.Where(x => x.IdAsset == pos.First.idAsset).FirstOrDefault();
                AssetExpandedParameter assetETD = m_AssetExpandedParameters.Where(x => x.AssetId == pos.First.idAsset).FirstOrDefault();
                if ((serie != default(RiskDataEuronextVarSerie)) && (assetETD != default(AssetExpandedParameter)))
                {
                    IEnumerable<EuronextVarScenarioCalc> calc = from item in pRiskDataSet.ScenarioPrices[serie.IsinCode].ScenarioPrice
                                                                let currency = pRiskDataSet.ScenarioPrices[serie.IsinCode].Currency
                                                                let assetPnL = PnLAssetFunc(item.Value, pRiskDataSet.ScenarioPrices[serie.IsinCode].CurrentValue, assetETD.ContractMultiplier)
                                                                let assetPnLCSSCurrency = ConvertCSSCurrency(pRiskDataSet, assetPnL, currency, item.Key)
                                                                let qty = (pos.First.FixSide == SideEnum.Buy) ? (-1 * pos.Second.Quantity) : pos.Second.Quantity
                                                                select new EuronextVarScenarioCalc
                                                                {
                                                                    IdAsset = pos.First.idAsset,
                                                                    Serie = serie,
                                                                    Scenario = item,
                                                                    PosType = PosType.ALC,
                                                                    PosAssetPnL = assetPnL,
                                                                    PosQty = qty,
                                                                    PosPnL = assetPnLCSSCurrency * qty,
                                                                };

                    lstPosCalc.Add((pos.First, calc));
                }
            }

            //Positions Options ou Futures échues avec livraison physique non encore livrés
            var positionsLiv = pPositions.Where(x => x.Second.ExeAssQuantity != 0).Select(x => x);
            foreach (Pair<PosRiskMarginKey, RiskMarginPosition> pos in positionsLiv)
            {
                AssetExpandedParameter assetETD = m_AssetExpandedParameters.Where(x => x.AssetId == pos.First.idAsset).FirstOrDefault();

                if (assetETD != default(AssetExpandedParameter))
                {
                    string underlyingIsinCode =
                        m_UnderlyingAssetExpandedParameters.Where(x => x.AssetId == assetETD.UnderlyningAssetId && x.AssetCategory == assetETD.UnderlyningAssetCategory).Select(x => x.IsinCode).FirstOrDefault();

                    string decorrelationGroup = underlyingIsinCode;
                    RiskDataEuronextVarSerie serie;
                    if (pRiskDataSet.Series.TryGetValue(underlyingIsinCode, out serie))
                    {
                        decorrelationGroup = serie.DecorrelationGroup;
                    }

                    if (underlyingIsinCode != default)
                    {
                        if (assetETD.CategoryEnum == CfiCodeCategoryEnum.Future)
                        {
                            IEnumerable<EuronextVarScenarioCalc> calc = from item in pRiskDataSet.ScenarioPrices[underlyingIsinCode].ScenarioPrice
                                                                        let currency = pRiskDataSet.ScenarioPrices[underlyingIsinCode].Currency
                                                                        let underlyingCurrentValue = pRiskDataSet.ScenarioPrices[underlyingIsinCode].CurrentValue
                                                                        let assetPnL = PnLAssetFunc(item.Value, underlyingCurrentValue, assetETD.ContractMultiplier)
                                                                        let assetPnLCSSCurrency = ConvertCSSCurrency(pRiskDataSet, assetPnL, currency, item.Key)
                                                                        let qty = pos.Second.ExeAssQuantity
                                                                        select new EuronextVarScenarioCalc
                                                                        {
                                                                            IdAsset = pos.First.idAsset,
                                                                            Serie = BuildRiskDataEuronextVarSerie(assetETD, underlyingIsinCode, decorrelationGroup),
                                                                            Scenario = item,
                                                                            PosType = PosType.DN,
                                                                            PosAssetPnL = assetPnL,
                                                                            PosQty = qty,
                                                                            PosPnL = assetPnLCSSCurrency * qty
                                                                        };

                            lstPosCalc.Add((pos.First, calc));
                        }
                        else if (assetETD.CategoryEnum == CfiCodeCategoryEnum.Option)
                        {
                            IEnumerable<EuronextVarScenarioCalc> calc = from item in pRiskDataSet.ScenarioPrices[underlyingIsinCode].ScenarioPrice
                                                                        let currency = pRiskDataSet.ScenarioPrices[underlyingIsinCode].Currency
                                                                        let underlyingCurrentValue = pRiskDataSet.ScenarioPrices[underlyingIsinCode].CurrentValue
                                                                        let isCall = (assetETD.PutOrCall.Value == PutOrCallEnum.Call)
                                                                        let assetPnL = PnLAssetFunc(
                                                                            pScenarioValue: isCall ? (item.Value - assetETD.StrikePrice) : (assetETD.StrikePrice - item.Value),
                                                                            pCurrentValue: isCall ? (underlyingCurrentValue - assetETD.StrikePrice) : (assetETD.StrikePrice - underlyingCurrentValue),
                                                                            pContratMultiplier: assetETD.ContractMultiplier)
                                                                        let assetPnLCSSCurrency = ConvertCSSCurrency(pRiskDataSet, assetPnL, currency, item.Key)
                                                                        let qty = pos.Second.ExeAssQuantity
                                                                        select new EuronextVarScenarioCalc
                                                                        {
                                                                            IdAsset = pos.First.idAsset,
                                                                            Serie = BuildRiskDataEuronextVarSerie(assetETD, underlyingIsinCode, decorrelationGroup),
                                                                            Scenario = item,
                                                                            PosType = pos.Second.ExeAssQuantity > 0 ? PosType.AS : PosType.EX,
                                                                            PosAssetPnL = assetPnL,
                                                                            PosQty = qty,
                                                                            PosPnL = assetPnLCSSCurrency * qty
                                                                        };
                            lstPosCalc.Add((pos.First, calc));
                        }
                    }
                }
            }

            IEnumerable<EuronextVarScenarioCalc> ret = from pos in lstPosCalc
                                                       from item in pos.ScenarioCalc
                                                       select item;

            return ret;
        }

        /// <summary>
        /// Retourne pour les scenarios utiles aux positions future livrées <paramref name="pPositions"/>. Pour chaque couple position, scenario des PnLs sont calculés.
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pPositions">Positions pour lesquelles calculer le déposit</param>
        /// <returns></returns>
        private IEnumerable<EuronextVarScenarioCalc> GetRiskDataEuronextVarScenarioCalcDelivery(RiskDataEuronextVarSet pRiskDataSet, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            List<(PosRiskMarginKey Key, IEnumerable<EuronextVarScenarioCalc> ScenarioCalc)> lstPosCalc = new List<(PosRiskMarginKey, IEnumerable<EuronextVarScenarioCalc>)>();

            // Positions Futures échues avec livraison physique non encore livrés
            var positionsDly = pPositions.Where(x => x.Second.ExeAssQuantity != 0).Select(x => x);
            foreach (Pair<PosRiskMarginKey, RiskMarginPosition> pos in positionsDly)
            {
                AssetExpandedParameter assetETD = m_AssetExpandedParameters.Where(x => x.AssetId == pos.First.idAsset).FirstOrDefault();

                if ((assetETD != default(AssetExpandedParameter)) && (assetETD.CategoryEnum == CfiCodeCategoryEnum.Future))
                {
                    string isinCode = pRiskDataSet.ScenarioPricesDelivery.Where(x => x.Value.ContractSymbol == assetETD.ContractSymbol).Select(x => x.Key).FirstOrDefault();

                    string decorrelationGroup = isinCode;
                    RiskDataEuronextVarSerie serie;
                    if (pRiskDataSet.Series.TryGetValue(isinCode, out serie))
                    {
                        decorrelationGroup = serie.DecorrelationGroup;
                    }

                    if (isinCode != default(string))
                    {
                        IEnumerable<EuronextVarScenarioCalc> calc = from item in pRiskDataSet.ScenarioPricesDelivery[isinCode].ScenarioPrice
                                                                    let currency = pRiskDataSet.ScenarioPricesDelivery[isinCode].Currency
                                                                    let underlyingCurrentValue = pRiskDataSet.ScenarioPricesDelivery[isinCode].CurrentValue
                                                                    let assetPnL = PnLAssetFunc(item.Value, underlyingCurrentValue, pRiskDataSet.ScenarioPricesDelivery[isinCode].Multiplier)
                                                                    let assetPnLCSSCurrency = ConvertCSSCurrencyDelivery(pRiskDataSet, assetPnL, currency, item.Key)
                                                                    let qty = pos.Second.ExeAssQuantity
                                                                    select new EuronextVarScenarioCalc
                                                                    {
                                                                        IdAsset = pos.First.idAsset,
                                                                        Serie = BuildRiskDataEuronextVarSerie(assetETD, isinCode, decorrelationGroup),
                                                                        Scenario = item,
                                                                        PosType = PosType.DN,
                                                                        PosAssetPnL = assetPnL,
                                                                        PosQty = qty,
                                                                        PosPnL = assetPnLCSSCurrency * qty
                                                                    };

                        lstPosCalc.Add((pos.First, calc));
                    }
                }
            }

            IEnumerable<EuronextVarScenarioCalc> ret = from pos in lstPosCalc
                                                       from item in pos.ScenarioCalc
                                                       select item;
            return ret;
        }

        /// <summary>
        ///  Construit un RiskDataEuronextVarSerie à partir de l'asset ETD <paramref name="pAssetETD"/>
        /// </summary>
        /// <param name="pAssetETD">représent un asset ETD</param>
        /// <param name="pUnderlyingISINCode">code du ss-facent de l'asset ETD</param>
        /// <param name="pDecorrelationGroup">Groupe de decorrelation</param>
        /// <returns></returns>
        private static RiskDataEuronextVarSerie BuildRiskDataEuronextVarSerie(AssetExpandedParameter pAssetETD, string pUnderlyingISINCode, string pDecorrelationGroup)
        {
            RiskDataEuronextVarSerie ret = new RiskDataEuronextVarSerie()
            {
                IdAsset = pAssetETD.AssetId,
                ContractSymbol = pAssetETD.ContractSymbol,
                AssetType = pAssetETD.Category,
                Multiplier = pAssetETD.ContractMultiplier,
                MaturityDate = pAssetETD.MaturityDateSys,
                IsinCode = string.Empty, /* le type AssetExpandedParameter n'expose pas le code ISISN */
                UnderlyingISINCode = pUnderlyingISINCode,
                DecorrelationGroup = pDecorrelationGroup,
            };

            if (pAssetETD.Category == "O")
            {
                ret.OptionType = (pAssetETD.PutOrCall.Value == PutOrCallEnum.Call) ? "C" : "P";
                ret.StrikePrice = pAssetETD.StrikePrice;
            };

            return ret;
        }

        /// <summary>
        ///  Retourne (<paramref name="pScenarioValue"/> - <paramref name="pCurrentValue"/>) * <paramref name="pContratMultiplier"/>
        /// </summary>
        /// <param name="pScenarioValue"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pContratMultiplier"></param>
        /// <returns></returns>
        private static decimal PnLAssetFunc(decimal pScenarioValue, decimal pCurrentValue, decimal pContratMultiplier)
        {
            return (pScenarioValue - pCurrentValue) * pContratMultiplier;
        }

        /// <summary>
        /// Convertit <paramref name="pValue"/> exprimé en <paramref name="pCurrency"/> dans la devise de Clearing <seealso cref="m_CssCurrency"/>.
        /// Utilise le taux de change présent pour le scénario <paramref name="pScenarioKey"/> des positions classique
        /// Retourne 0 si Taux de change non trouvé. 
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pValue"></param>
        /// <param name="pCurrency"></param>
        /// /// <param name="pScenarioKey">clé d'acès au scénario</param>
        /// <returns></returns>
        private decimal ConvertCSSCurrency(RiskDataEuronextVarSet pRiskDataSet, decimal pValue, string pCurrency, RiskDataEuronextVarScenarioKey pScenarioKey)
        {
            return ConvertScenarioToCSSCurrencyy(pRiskDataSet.ScenarioFx, pValue, pCurrency, pScenarioKey);
        }

        /// <summary>
        /// Convertit <paramref name="pValue"/> exprimé en <paramref name="pCurrency"/> dans la devise de Clearing <seealso cref="m_CssCurrency"/>.
        /// Utilise le taux de change présent pour le scénario <paramref name="pScenarioKey"/> des positions en livraison physique
        /// Retourne 0 si Taux de change non trouvé. 
        /// </summary>
        /// <param name="pRiskDataSet">Jeu de données de risque du sector</param>
        /// <param name="pValue"></param>
        /// <param name="pCurrency"></param>
        /// /// <param name="pScenarioKey">clé d'acès au scénario</param>
        /// <returns></returns>
        private decimal ConvertCSSCurrencyDelivery(RiskDataEuronextVarSet pRiskDataSet, decimal pValue, string pCurrency, RiskDataEuronextVarScenarioKey pScenarioKey)
        {
            return ConvertScenarioToCSSCurrencyy(pRiskDataSet.ScenarioFxDelivery, pValue, pCurrency, pScenarioKey);
        }

        /// <summary>
        /// Convertit <paramref name="pValue"/> exprimé en <paramref name="pCurrency"/> dans la devise de Clearing <seealso cref="m_CssCurrency"/>.
        /// Utilise le taux de change présent sur le scénario <paramref name="pScenarioKey"/>
        /// Retourne 0 si Taux de change non trouvé. 
        /// </summary>
        /// <param name="pScenarioFx">Enssemble des taux de change</param>
        /// <param name="pValue"></param>
        /// <param name="pCurrency"></param>
        /// /// <param name="pScenarioKey">Clé d'acès au scénario</param>
        /// <returns></returns>
        private decimal ConvertScenarioToCSSCurrencyy(Dictionary<string, RiskDataEuronextVarSceFx> pScenarioFx, decimal pValue, string pCurrency, RiskDataEuronextVarScenarioKey pScenarioKey)
        {
            decimal ret = decimal.Zero;

            if (pCurrency == m_CssCurrency)
            {
                ret = pValue;
            }
            else if ((pScenarioFx != default(Dictionary<string, RiskDataEuronextVarSceFx>)) && (pScenarioFx.Count > 0))
            {
                string key = $"{pCurrency}/{m_CssCurrency}";
                if (pScenarioFx.ContainsKey(key))
                {
                    RiskDataEuronextVarScenario riskDataEuronextVarScenario = pScenarioFx[key].ScenarioFx.Where(x => x.Key.ReferenceDate == pScenarioKey.ReferenceDate && x.Key.Type == pScenarioKey.Type).FirstOrDefault();
                    if (riskDataEuronextVarScenario != default)
                    {
                        ret = pValue * riskDataEuronextVarScenario.Value;
                    }
                }
            }
            return ret;
        }
        #endregion private Methods
    }

    /// <summary>
    /// Classe de calcul du déposit Euronext Var Based Margin
    /// </summary>
    // PM 20240423 [XXXXX] Ajout
    public class EuronextVarBasedRiskMethod : EuronextVarRiskMethod
    {
        #region Override base accessors
        /// <summary>
        /// Type de la Methode
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.EURONEXT_VAR_BASED; }
        }
        #endregion Override base accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public EuronextVarBasedRiskMethod()
        {
            // Méthode utilisant les positions
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }
        #endregion Constructor
    }

    /// <summary>
    /// Classe de calcul du déposit Euronext Legacy Var Margin
    /// </summary>
    // PM 20240423 [XXXXX] Ajout
    public class EuronextLegacyVarRiskMethod : EuronextVarRiskMethod
    {
        #region Override base accessors
        /// <summary>
        /// Type de la Methode
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.EURONEXT_LEGACY_VAR; }
        }
        #endregion Override base accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public EuronextLegacyVarRiskMethod()
        {
            // Méthode utilisant les positions
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }
        #endregion Constructor
    }

    /// <summary>
    /// Class représentant un couple position, scenario
    /// </summary>
    public sealed class EuronextVarScenarioCalc
    {
        #region Members
        /// <summary>
        ///  clé de la position
        /// </summary>
        public int IdAsset;

        /// <summary>
        /// Asset impliqué dans la position (contient codeisin, etc)
        /// </summary>
        public RiskDataEuronextVarSerie Serie;

        /// <summary>
        /// Elément de scénario (type, date et sa valeur)
        /// </summary>
        public RiskDataEuronextVarScenario Scenario { get; set; }

        /// <summary>
        /// ALC (pour position classique), EX/AS (pour positions options exercées/assignées en attente de livraison),  DN (pour positions Futur en attente de livraisonà
        /// </summary>
        public PosType PosType;

        /// <summary>
        /// (valeur du scénario - Serie.CurrentValue) * Multiplier (exprimé dans la devise de l'asset).  
        /// </summary>
        public decimal PosAssetPnL { get; set; }

        /// <summary>
        /// Qté nette en position
        /// </summary>
        public decimal PosQty { get; set; }

        /// <summary>
        /// PnL de la position (exprimé dans la devise de clearing (normalement EUR))
        /// </summary>
        public decimal PosPnL { get; set; }
        #endregion Members
    }

    /// <summary>
    ///  Class de base pour regroupemement de PnL 
    /// </summary>
    public class EuronextVarScenarioPnLBase
    {
        #region Members
        /// <summary>
        /// Clé du scénario
        /// </summary>
        public RiskDataEuronextVarScenarioKey ScenarioKey { get; }

        /// <summary>
        /// PnL (exprimé dans la devise de clearing (normalement EUR))
        /// </summary>
        public decimal PnL { get; set; }
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pScenarioKey"></param>
        public EuronextVarScenarioPnLBase(RiskDataEuronextVarScenarioKey pScenarioKey)
        {
            ScenarioKey = pScenarioKey;
        }
        #endregion Constructor
    }

    /// <summary>
    ///  Class pour PnL par Asset (IsinCode)
    /// </summary>
    public sealed class EuronextVarScenarioPnLIsinCode : EuronextVarScenarioPnLBase
    {
        #region Members
        /// <summary>
        /// Code Isin de l'asset
        /// </summary>
        public String IsinCode { get; }
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIsinCode"></param>
        /// <param name="pScenarioKey"></param>
        public EuronextVarScenarioPnLIsinCode(string pIsinCode, RiskDataEuronextVarScenarioKey pScenarioKey) : base(pScenarioKey)
        {
            IsinCode = pIsinCode;
        }
        #endregion Constructor
    }

    /// <summary>
    ///  Class pour PnL par Groupe de decorrelation
    /// </summary>
    public sealed class EuronextVarScenarioPnLDecoGroup : EuronextVarScenarioPnLBase
    {
        #region Members
        /// <summary>
        /// Groupe de decorrelation
        /// </summary>
        public String DecorrelationGroup { get; }
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pDecoGroup"></param>
        /// <param name="pScenarioKey"></param>
        public EuronextVarScenarioPnLDecoGroup(string pDecoGroup, RiskDataEuronextVarScenarioKey pScenarioKey) : base(pScenarioKey)
        {
            DecorrelationGroup = pDecoGroup;
        }
        #endregion Constructor
    }

    /// <summary>
    ///  Class pour PnL par Book
    /// </summary>
    public sealed class EuronextVarScenarioPnLBook : EuronextVarScenarioPnLBase
    {
        #region Members
        /// <summary>
        /// Id Actor
        /// </summary>
        public int IdA { get; }
        /// <summary>
        /// Id Book
        /// </summary>
        public int IdB { get; }
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// <param name="pScenarioKey"></param>
        public EuronextVarScenarioPnLBook(int pIdA, int pIdB, RiskDataEuronextVarScenarioKey pScenarioKey) : base(pScenarioKey)
        {
            IdA = pIdA;
            IdB = pIdB;
        }
        #endregion Constructor
    }

    /// <summary>
    /// MarkToMarket par position
    /// </summary>
    public sealed class EuronextVarMarkToMarketPos
    {
        #region Members
        /// <summary>
        /// Position
        /// </summary>
        public Pair<PosRiskMarginKey, RiskMarginPosition> Position { get; }

        /// <summary>
        /// MarkToMarket exprimé dans la devise de l'asset
        /// </summary>
        public Money MTM { get; }
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pPositions"></param>
        /// <param name="pMtm"></param>
        public EuronextVarMarkToMarketPos(Pair<PosRiskMarginKey, RiskMarginPosition> pPositions, Money pMtm)
        {
            Position = pPositions;
            MTM = pMtm;
        }
        #endregion Constructor
    }
}


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
//
using EfsML.Enum;
//
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    public class NoMarginRiskMethod : BaseMethod
    {
        #region Members
        private DateTime m_DtBusiness;
        #endregion Members

        #region Override base accessors
        /// <summary>
        /// Type de la Methode
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.NOMARGIN; }
        }
        #endregion Override base accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public NoMarginRiskMethod()
        {
            m_RiskMethodDataType = RiskMethodDataTypeEnum.TradeNoMargin;
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
            m_DtBusiness = GetRiskParametersDate(pCS);
        }

        /// <summary>
        /// Libère les paramètres spécifiques à la méthode.
        /// </summary>
        protected override void ResetSpecificParameters()
        {
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
            //
            // Creation de l'objet de communication du détail du calcul
            NoMarginCalcMethCom methodComObj = new NoMarginCalcMethCom();
            opMethodComObj = methodComObj;                          // Affectation de l'objet de communication du détail du calcul en sortie
            methodComObj.MarginMethodType = this.Type;              // Affectation du type de méthode de calcul
            methodComObj.CssCurrency = m_CssCurrency;               // Affectation de la devise de calcul
            methodComObj.IdA = pActorId;                            // Affectation de l'id de l'acteur
            methodComObj.IdB = pBookId;                             // Affectation de l'id du book
            methodComObj.DtParameters = m_DtBusiness;               // Date Business
            methodComObj.Missing = false;
            methodComObj.IsIncomplete = false;
            //
            //
            if (pRiskDataToEvaluate != default(RiskData))
            {
                RiskDataNoMarginTrade dataNoMarginTrade = pRiskDataToEvaluate.TradeNoMargin;

                if (dataNoMarginTrade != default(RiskDataNoMarginTrade))
                {
                    Dictionary<RiskDataNoMarginTradeKey, RiskNoMarginTrade> noMarginTrade = dataNoMarginTrade.NoMarginTrade;
                    if ((noMarginTrade != default(Dictionary<RiskDataNoMarginTradeKey, RiskNoMarginTrade>)) && (noMarginTrade.Count() > 0))
                    {
                        methodComObj.AssetGroup = new List<NoMarginMethAssetGroupCom>();

                        // Rassembler tous les trades
                        var allTrades = from noMarginTrades in noMarginTrade.Values
                                        from trades in noMarginTrades.Trades
                                        select trades;

                        // Générer les clés AssetCategory/IdAsset
                        var allTradesKeys = allTrades.GroupBy(t => new { t.IdAsset, t.AssetCategory });

                        // Parcourir les clés AssetCategory/IdAsset
                        foreach (var tradeKey in allTradesKeys)
                        {
                            // Ensemble des trades correspondant à l'asset
                            var keyTrades = allTrades.Where(t => (t.AssetCategory == tradeKey.Key.AssetCategory) && (t.IdAsset == tradeKey.Key.IdAsset));
                            // Prendre le premier trade de l'ensemble
                            var firstTrade = keyTrades.First();

                            // Construire le log correspondant à l'ensemble des trades portant sur l'asset
                            NoMarginMethAssetGroupCom assetCom = new NoMarginMethAssetGroupCom
                            {
                                IdM = firstTrade.IdM,
                                MarketIdentifier = firstTrade.MarketIdentifier,
                                IdI = firstTrade.IdI,
                                IdContract = firstTrade.IdContract,
                                ContractIdentifier = firstTrade.ContractIdentifier,
                                IdAsset = firstTrade.IdAsset,
                                AssetIdentifier = firstTrade.AssetIdentifier,
                                AssetCategory = firstTrade.AssetCategoryString,
                                Currency = firstTrade.Currency,
                                Trades = (from trd in keyTrades
                                          select new NoMarginMetTradesCom
                                          {
                                              IdT = trd.IdT,
                                              TradeIdentifier = trd.TradeIdentifier,
                                              IdA_Dealer = trd.IdA_Dealer,
                                              IdB_Dealer = trd.IdB_Dealer,
                                              IdA_Clearer = trd.IdA_Clearer,
                                              IdB_Clearer = trd.IdB_Clearer,
                                              DtBusiness = trd.DtBusiness,
                                              DtTimestamp = trd.DtTimestamp,
                                              DtExecution = trd.DtExecution,
                                              TzFacility = trd.TzFacility,
                                              Side = trd.Side,
                                              Quantity = trd.Quantity,
                                              Price = trd.Price,
                                          }).ToList(),
                            };

                            // Ajout à l'ensemble des log
                            methodComObj.AssetGroup.Add(assetCom);
                        }
                    }
                }
            }

            // Aucun montant, créer un montant à zéro
            if (StrFunc.IsEmpty(this.m_CssCurrency))
            {
                // Si aucune devise de renseignée, utiliser l'euro
                riskAmounts.Add(new Money(0, "EUR"));
            }
            else
            {
                riskAmounts.Add(new Money(0, this.m_CssCurrency));
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
            return null;
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
    }
}

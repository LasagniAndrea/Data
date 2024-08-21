using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EfsML.Business;
using EfsML.Enum;
using FixML.Enum;
using FpML.v44.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    internal sealed class MeffValuationRiskArray
    {
        #region members
        private readonly int m_IdAsset = 0;
        private readonly Dictionary<int, decimal> m_RiskValue = null;
        #endregion members

        #region accessors
        public int IdAsset
        {
            get { return m_IdAsset; }
        }
        public Dictionary<int, decimal> RiskValue
        {
            get { return m_RiskValue; }
        }
        #endregion accessors

        #region constructors
        public MeffValuationRiskArray(int pIdAsset, decimal pMultiplier, MeffRiskArray pRiskArrayBuy, MeffRiskArray pRiskArraySell)
        {
            m_IdAsset = pIdAsset;
            if ((pRiskArrayBuy != default(MeffRiskArray))
                && (pRiskArraySell != default(MeffRiskArray))
                && (pRiskArrayBuy.IdAsset == pRiskArraySell.IdAsset)
                && (pRiskArrayBuy.SideEnum == SideEnum.Buy)
                && (pRiskArraySell.SideEnum == SideEnum.Sell))
            {
                var buyValues = pRiskArrayBuy.ToDictionary().Where(r => r.Key <= pRiskArrayBuy.NbValue);

                var sellValues = from value in pRiskArraySell.ToDictionary().Where(r => r.Key <= pRiskArraySell.NbValue)
                                 select new KeyValuePair<int, decimal>(value.Key + pRiskArrayBuy.NbValue, value.Value);

                m_RiskValue = buyValues.Concat(sellValues).ToDictionary(r => r.Key, r => r.Value * pMultiplier);
            }
            else
            {
                m_RiskValue = new Dictionary<int, decimal>();
            }
        }
        #endregion constructors

        #region methods
        public static IEnumerable<MeffValuationRiskArray> ConstructValuationRiskArray(IEnumerable<AssetExpandedParameter> pAssetExpandedParameters, IEnumerable<MeffRiskArray> pRiskArray)
        {
            IEnumerable<MeffValuationRiskArray> riskArray = from asset in pAssetExpandedParameters
                                                            join riskBuy in pRiskArray on asset.AssetId equals riskBuy.IdAsset
                                                            join riskSell in pRiskArray on asset.AssetId equals riskSell.IdAsset
                                                            where (riskBuy.SideEnum == SideEnum.Buy) && (riskSell.SideEnum == SideEnum.Sell)
                                                            select new MeffValuationRiskArray(asset.AssetId, asset.ContractMultiplier, riskBuy, riskSell);
            return riskArray;
        }
        #endregion methods
    }

    /// <summary>
    /// Classe de calcul du déposit par la méthode MEFFCOM2
    /// </summary>
    public sealed class MEFFCOM2Method : BaseMethod
    {
        #region const
        const string PriceFluctuationPercentage = "P";
        const string PriceFluctuationTick = "T";
        #endregion

        #region members
        private IEnumerable<MeffValuationRiskArray> m_TheoreticalPriceRiskArray = null;
        private IEnumerable<MeffValuationRiskArray> m_DeltaRiskArray = null;
        #region Referentiel Parameters
        internal IEnumerable<AssetExpandedParameter> m_AssetExpandedParameters = null;
        #endregion Referentiel Parameters
        #region MEFFCOM2 Parameters
        private IEnumerable<MeffValuationArray> m_ValuationArrayParameters = null;
        private IEnumerable<MeffInterSpread> m_InterSpreadParameters = null;
        private IEnumerable<MeffIntraSpread> m_IntraSpreadParameters = null;
        private IEnumerable<MeffContractAsset> m_ContractAssetParameters = null;
        private IEnumerable<MeffRiskArray> m_TheoreticalPriceArrayParameters = null;
        private IEnumerable<MeffRiskArray> m_DeltaArrayParameters = null;
        #endregion MEFFCOM2 Parameters
        #region Empty MEFFCOM2 Parameters
        private MeffValuationArray m_EmptyValuationArrayParameters = null;
        private MeffContractAsset m_EmptyContractAssetParameters = null;
        private MeffValuationRiskArray m_EmptyValuationRiskArrayParameters = null;
        #endregion Empty MEFFCOM2 Parameters
        #endregion members

        #region override base accessors
        /// <summary>
        /// Type de la Methode
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.MEFFCOM2; }
        }
        
        /// <summary>
        /// Requête utilisée pour connaître l'existance de paramètres de risque pour une date donnée
        /// <remarks>Utilise les paramètres DTBUSINESS & SESSIONID</remarks>
        /// </summary>
        /// PM 20150511 [20575] Add QueryExistRiskParameter
        /// RD 20160331 [22028] use mc.BUSINESSDATE instead rv.BUSINESSDATE
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        protected override string QueryExistRiskParameter
        {
            get
            {
                string query;
                query = @"
                    select distinct 1
                      from dbo.IMMEFFCONTRACT_H mc
                     inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = mc.IDASSET)
                     where (mc.BUSINESSDATE = @DTBUSINESS)";
                return query;
            }
        }
        #endregion

        #region constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        internal MEFFCOM2Method()
        {
            // PM 20170313 [22833] Ajout alimentation de m_RiskMethodDataType
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }
        #endregion Constructor

        #region override base methods
        /// <summary>
        /// Charge les paramètres spécifiques à la méthode.
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pAssetETDCache">Collection d'assets contenant tous les assets en position</param>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>();
            // PM 20150511 [20575] Ajout gestion dtMarket 
            //DateTime dtBusiness = this.DtBusiness.Date;
            DateTime dtBusiness = GetRiskParametersDate(pCS);
            //
            // Pour les paramètres manquants
            m_EmptyContractAssetParameters = new MeffContractAsset { IdAsset = 0, AssetCode = Cst.NotFound, ArrayCode = Cst.NotFound, MgrUnlAssetCode = Cst.NotFound };
            m_EmptyValuationArrayParameters = new MeffValuationArray { ArrayCode = Cst.NotFound };
            m_EmptyValuationRiskArrayParameters = new MeffValuationRiskArray(0, 0, null, null);

            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                // ASSETEXPANDED_ALLMETHOD
                m_AssetExpandedParameters = LoadParametersAssetExpanded(connection);

                // Set Parameters : DTBUSINESS
                dbParametersValue.Add("DTBUSINESS", dtBusiness);

                // VALUATIONARRAY_MEFFCOM2METHOD
                m_ValuationArrayParameters = LoadParametersMethod<MeffValuationArray>.LoadParameters(connection, dbParametersValue, DataContractResultSets.VALUATIONARRAY_MEFFCOM2METHOD);

                // INTERSPREAD_MEFFCOM2METHOD
                m_InterSpreadParameters = LoadParametersMethod<MeffInterSpread>.LoadParameters(connection, dbParametersValue, DataContractResultSets.INTERSPREAD_MEFFCOM2METHOD);

                // INTRASPREAD_MEFFCOM2METHOD
                m_IntraSpreadParameters = LoadParametersMethod<MeffIntraSpread>.LoadParameters(connection, dbParametersValue, DataContractResultSets.INTRASPREAD_MEFFCOM2METHOD);

                // CONTRACTASSET_MEFFCOM2METHOD
                m_ContractAssetParameters = LoadParametersMethod<MeffContractAsset>.LoadParameters(connection, dbParametersValue, DataContractResultSets.CONTRACTASSET_MEFFCOM2METHOD);

                // THEORPRICEARRAY_MEFFCOM2METHOD
                m_TheoreticalPriceArrayParameters = LoadParametersMethod<MeffRiskArray>.LoadParameters(connection, dbParametersValue, DataContractResultSets.THEORPRICEARRAY_MEFFCOM2METHOD);

                // DELTAARRAY_MEFFCOM2METHOD
                m_DeltaArrayParameters = LoadParametersMethod<MeffRiskArray>.LoadParameters(connection, dbParametersValue, DataContractResultSets.DELTAARRAY_MEFFCOM2METHOD);

                // Construction des matrices de risque
                m_TheoreticalPriceRiskArray = MeffValuationRiskArray.ConstructValuationRiskArray(m_AssetExpandedParameters, m_TheoreticalPriceArrayParameters);
                m_DeltaRiskArray = MeffValuationRiskArray.ConstructValuationRiskArray(m_AssetExpandedParameters, m_DeltaArrayParameters);
            }
        }

        /// <summary>
        /// Libère les paramètres spécifiques à la méthode.
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            m_AssetExpandedParameters = null;
            m_ValuationArrayParameters = null;
            m_InterSpreadParameters = null;
            m_IntraSpreadParameters = null;
            m_ContractAssetParameters = null;
            m_TheoreticalPriceArrayParameters = null;
            m_DeltaArrayParameters = null;

            m_TheoreticalPriceRiskArray = null;
            m_DeltaRiskArray = null;
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
            MeffCalcMethCom methodComObj = new MeffCalcMethCom();
            opMethodComObj = methodComObj;
            methodComObj.MarginMethodType = this.Type;
            // Affectation de la devise de calcul
            methodComObj.CssCurrency = m_CssCurrency;
            //PM 20150511 [20575] Ajout date des paramètres de risque
            methodComObj.DtParameters = DtRiskParameters;

            if (pRiskDataToEvaluate != default(RiskData))
            {
                // PM 20170313 [22833] Prendre uniquement la position (à l'ancien format)
                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsToEvaluate = pRiskDataToEvaluate.GetPositionAsEnumerablePair();

                if ((positionsToEvaluate != null) && (positionsToEvaluate.Count() > 0))
                {
                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions;

                    // Group the positions by asset (the side of the new merged assets will be set with regards to the long and short quantities)
                    positions = PositionsGrouping.GroupPositionsByAsset(positionsToEvaluate);
                    // Ne garder que les positions dont la quantité est différente de 0
                    positions =
                        from pos in positions
                        where pos.Second.Quantity != 0
                        select pos;

                    // Coverage short call and short futures (this one modify the position quantity)
                    IEnumerable<CoverageSortParameters> inputCoverage = GetSortParametersForCoverage(positions);
                    // Reduction de la position couverte
                    // FI 20160613 [22256] Modify
                    Pair<IEnumerable<StockCoverageCommunicationObject>, IEnumerable<StockCoverageDetailCommunicationObject>> coveredQuantities =
                        ReducePosition(pActorId, pBookId, pDepositHierarchyClass, inputCoverage, ref positions);

                    // Construction des class de calcul avec les positions associées
                    List<MeffMarginClassCom> marginClass = GetContractGroupHierarchy(positions, coveredQuantities.First);
                    methodComObj.Parameters = marginClass.ToArray();
                    // FI 20160613 [22256] Alimentation de UnderlyingStock
                    methodComObj.UnderlyingStock = coveredQuantities.Second;

                    // Calculer les montants de risque
                    riskAmounts = EvaluateRisk(methodComObj);
                }
            }
            if (riskAmounts == null)
                riskAmounts = new List<Money>();

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
                join asset in m_AssetExpandedParameters on position.First.idAsset equals asset.AssetId
                join quote in m_ContractAssetParameters on asset.AssetId equals quote.IdAsset
                where (quote.AssetCategoryEnum == Cst.UnderlyingAsset.Future)
                || (quote.AssetCategoryEnum == Cst.UnderlyingAsset.ExchangeTradedContract)
                select new CoverageSortParameters
                {
                    AssetId = position.First.idAsset,
                    ContractId = asset.ContractId,
                    MaturityYearMonth = decimal.Parse(asset.MaturityYearMonth),
                    Multiplier = asset.ContractMultiplier,
                    Quote = quote.Price,
                    StrikePrice = asset.StrikePrice,
                    Type = RiskMethodExtensions.GetTypeFromCategoryPutCall(asset.CategoryEnum, asset.PutOrCall),
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
        }
        #endregion override base methods

        #region static methods
        /// <summary>
        /// Cumul les valeurs d'un ensemble de dictionnaire par clé sur un nouveau dictionnaire
        /// </summary>
        /// <param name="pManyRiskValue">Ensemble de dictionnaire</param>
        /// <returns>Nouveau dictionnaire</returns>
        public static Dictionary<int, decimal> CumulRiskValue(IEnumerable<Dictionary<int, decimal>> pManyRiskValue)
        {
            Dictionary<int, decimal> retDic = default;

            if (pManyRiskValue != default(IEnumerable<Dictionary<int, decimal>>))
            {
                retDic = new Dictionary<int, decimal>();

                // Pour chaque matrice de risque
                foreach (var riskValue in pManyRiskValue)
                {
                    // Pour chaque valeur de la matrice
                    foreach (var value in riskValue)
                    {
                        // Cumul des matrices Net Position Margin
                        if (retDic.ContainsKey(value.Key))
                        {
                            retDic[value.Key] += value.Value;
                        }
                        else
                        {
                            retDic.Add(value.Key, value.Value);
                        }
                    }
                }
            }
            return retDic;
        }

        /// <summary>
        /// Retourne un dictionnaire dont les valeur ont été multiplier par un facteur
        /// </summary>
        /// <param name="pSource">Dictionnaire source</param>
        /// <param name="pMult">Facteur de multiplication</param>
        /// <returns>Nouveau dictionnaire</returns>
        public static Dictionary<int, decimal> MultRiskValue(Dictionary<int, decimal> pSource, decimal pMult)
        {
            Dictionary<int, decimal> retDic = default;

            if (pSource != default)
            {
                retDic = pSource.ToDictionary(d => d.Key, d => d.Value * pMult);
            }
            return retDic;
        }

        /// <summary>
        /// Retourne un dictionnaire contenant pour chaque clé, le minimum en valeur absolue des 2 valeurs des 2 dictionnaires en paramètres.
        /// Les valeurs du nouveau dictionnaire sont ajoutées (si valeur initial négative) ou retranchées (si valeur initial positive) des dictionnaires en paramètres.
        /// </summary>
        /// <param name="pRiskValue1">Dictionnaire 1</param>
        /// <param name="pRiskValue2">Dictionnaire 2</param>
        /// <returns>Nouveau dictionnaire</returns>
        public Dictionary<int, decimal> ConsumeSpreadDeltaValues(Dictionary<int, decimal> pRiskValue1, Dictionary<int, decimal> pRiskValue2)
        {
            Dictionary<int, decimal> retDic = default;

            if ((pRiskValue1 != default)
                && (pRiskValue2 != default))
            {
                int[] keys = pRiskValue1.Keys.Union(pRiskValue2.Keys).ToArray();
                retDic = new Dictionary<int, decimal>(keys.Length);
                foreach (int key in keys)
                {
                    decimal spreadValue = 0;
                    if (pRiskValue1.TryGetValue(key, out decimal value1) && pRiskValue2.TryGetValue(key, out decimal value2))
                    {
                        // Si valeurs de signe opposé
                        if (((value1 < 0) && (value2 > 0))
                            || ((value1 > 0) && (value2 < 0)))
                        {
                            spreadValue = System.Math.Min(System.Math.Abs(value1), System.Math.Abs(value2));

                            if (value1 < 0)
                            {
                                pRiskValue1[key] += spreadValue;
                                pRiskValue2[key] -= spreadValue;
                            }
                            else
                            {
                                pRiskValue1[key] -= spreadValue;
                                pRiskValue2[key] += spreadValue;
                            }
                        }
                    }
                    retDic.Add(key, spreadValue);
                }
            }
            return retDic;
        }
        #endregion static methods

        #region methods
        /// <summary>
        /// Groupe les assets par margin class.
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">Position cumulée par asset</param>
        /// <param name="pCoveredQuantities">Position couverte par des actions</param>
        /// <returns>Liste des objets de détail du calcul de déposit hiérarchissés par margin class</returns>
        private List<MeffMarginClassCom> GetContractGroupHierarchy(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset,
            IEnumerable<StockCoverageCommunicationObject> pCoveredQuantities)
        {
            // Paramètres des assets en position
            var posContractAsset =
                from pos in pGroupedPositionsByIdAsset
                join asset in m_AssetExpandedParameters on pos.First.idAsset equals asset.AssetId
                join assetCtr in m_ContractAssetParameters on pos.First.idAsset equals assetCtr.IdAsset
                into posAsset
                from assetCtr in posAsset.DefaultIfEmpty(m_EmptyContractAssetParameters)  // Si les paramètres sont manquants
                select new
                {
                    asset.Currency,
                    MarginAssetCode = asset.CategoryEnum == CfiCodeCategoryEnum.Future ? assetCtr.AssetCode : assetCtr.MgrUnlAssetCode,
                    ContractAsset = assetCtr,
                    Position = pos,
                };

            // Liste des Contracts par Class avec leurs positions et paramètres de calculs
            List<MeffMarginClassCom> marginClass = (
                from posCtrAsset in posContractAsset
                join valGroup in m_ValuationArrayParameters on posCtrAsset.ContractAsset.ArrayCode equals valGroup.ArrayCode
                into posAssetGroup
                from valGroup in posAssetGroup.DefaultIfEmpty(m_EmptyValuationArrayParameters)  // Si les paramètres sont manquants
                group posCtrAsset by new { ValuationArray = valGroup, posCtrAsset.Currency }
                    into contractClass
                    select new MeffMarginClassCom
                    {
                        MarginClassCode = contractClass.Key.ValuationArray.ArrayCode,
                        PriceFluctuationType = contractClass.Key.ValuationArray.PriceFluctuationType,
                        PriceFluctuation = (contractClass.Key.ValuationArray.PriceIncreaseFluctuation + contractClass.Key.ValuationArray.PriceDecreaseFluctuation) / 2,
                        NumberOfValue = contractClass.Key.ValuationArray.NbValue,
                        StocksCoverage = from coveredQty in pCoveredQuantities
                                         join pos in contractClass on coveredQty.AssetId equals pos.Position.First.idAsset
                                         select coveredQty,
                        Positions = from pos in contractClass
                                    select pos.Position,
                        ClassDelta = 0,
                        WorstCaseScenario = 0,
                        MaximumDeltaOffset = 0,
                        DeltaPotentialFutureLoss = 0,
                        MarginClassPotentialFutureLoss = 0,
                        DeltaToOffset = 0,
                        UnderlyingPrice = 0,
                        MarginAmount = new Money(0, contractClass.Key.Currency),
                        NetPositionMarginAmount = new Money(0, contractClass.Key.Currency),
                        TimeSpreadMarginAmount = new Money(0, contractClass.Key.Currency),
                        CommodityMarginAmount = new Money(0, contractClass.Key.Currency),
                        InterCommodityCreditAmount = new Money(0, contractClass.Key.Currency),
                        NetPositionMarginArray = null,
                        TimeSpreadMarginArray = null,
                        CommodityMarginArray = null,
                        Parameters = (from assetClass in contractClass
                                      join assetMrg in m_ContractAssetParameters on assetClass.MarginAssetCode equals assetMrg.AssetCode
                                      into mrgAsset
                                      from assetMrg in mrgAsset.DefaultIfEmpty(m_EmptyContractAssetParameters)  // Si les paramètres sont manquants
                                      // 20131104 PM [19123] Modification de regourpement
                                      //group assetClass by assetMrg
                                      group assetClass by new { assetMrg.MgrUnlAssetCode, assetMrg.MaturityDate, assetMrg.Price }
                                          into assetMaturity
                                          select new MeffMarginMaturityCom
                                          {
                                              //MarginAssetCode = assetMaturity.Key.AssetCode,
                                              MarginAssetCode = assetMaturity.Key.MgrUnlAssetCode,
                                              MaturityDate = assetMaturity.Key.MaturityDate,
                                              Price = assetMaturity.Key.Price,
                                              StocksCoverage = from coveredQty in pCoveredQuantities
                                                               join pos in contractClass on coveredQty.AssetId equals pos.Position.First.idAsset
                                                               select coveredQty,
                                              Positions = from pos in assetMaturity
                                                          select pos.Position,
                                              Missing = (assetMaturity.Key.MgrUnlAssetCode == Cst.NotFound),
                                          }).OrderBy(m => m.MaturityDate).ToArray(),
                        Missing = (contractClass.Key.ValuationArray == m_EmptyValuationArrayParameters),
                    }).OrderBy(c => c.MarginClassCode).ToList();

            return marginClass;
        }

        /// <summary>
        /// Evalue le montant de dépot de garantie
        /// </summary>
        /// <param name="pMethodComObj">Positions par Margin Class et Maturity sur lesquels calculer le risque</param>
        /// <returns>Liste des montants calculés</returns>
        private List<Money> EvaluateRisk(MeffCalcMethCom pMethodComObj)
        {
            List<Money> marginAmountList = new List<Money>();
            MeffMarginClassCom[] marginClass = (MeffMarginClassCom[])pMethodComObj.Parameters;

            // Evaluation du risque de chaque margin class
            EvaluateRiskMarginClass(marginClass);

            // Calcul des Spread Inter Margin Class (Inter Commodity)
            List<MeffInterCommoditySpreadCom> interComSpreadList = new List<MeffInterCommoditySpreadCom>();
            IEnumerable<MeffInterSpread> spreadInterClass =
                from interSpread in m_InterSpreadParameters
                join mrgClass1 in marginClass on interSpread.ArrayCode1 equals mrgClass1.MarginClassCode
                join mrgClass2 in marginClass on interSpread.ArrayCode2 equals mrgClass2.MarginClassCode
                where (interSpread.OffsetMultiplier1 != 0)
                && (interSpread.OffsetMultiplier2 != 0)
                && (((mrgClass1.DeltaToOffset > 0) && (mrgClass2.DeltaToOffset < 0))
                 || ((mrgClass1.DeltaToOffset < 0) && (mrgClass2.DeltaToOffset > 0)))
                orderby interSpread.Priority
                select interSpread;

            foreach (MeffInterSpread interSpread in spreadInterClass)
            {
                MeffMarginClassCom mrgClass1 = marginClass.FirstOrDefault(m => m.MarginClassCode == interSpread.ArrayCode1);
                MeffMarginClassCom mrgClass2 = marginClass.FirstOrDefault(m => m.MarginClassCode == interSpread.ArrayCode2);

                if ((mrgClass1 != default(MeffMarginClassCom))
                    && (mrgClass2 != default(MeffMarginClassCom)))
                {
                    if (((mrgClass1.DeltaToOffset > 0) && (mrgClass2.DeltaToOffset < 0))
                        || ((mrgClass1.DeltaToOffset < 0) && (mrgClass2.DeltaToOffset > 0)))
                    {
                        MeffInterCommoditySpreadCom spreadCom = new MeffInterCommoditySpreadCom();
                        MeffInterCommoditySpreadLegCom spreadLeg1Com = new MeffInterCommoditySpreadLegCom();
                        MeffInterCommoditySpreadLegCom spreadLeg2Com = new MeffInterCommoditySpreadLegCom();
                        spreadCom.Priority = interSpread.Priority;
                        spreadCom.DiscountType = interSpread.DiscountType;
                        spreadCom.LegParameters = new MeffInterCommoditySpreadLegCom[] { spreadLeg1Com, spreadLeg2Com };
                        spreadLeg1Com.MarginClass = mrgClass1;
                        spreadLeg2Com.MarginClass = mrgClass2;
                        spreadLeg1Com.MarginCredit = interSpread.OffsetDiscount1 / 100;
                        spreadLeg2Com.MarginCredit = interSpread.OffsetDiscount2 / 100;
                        spreadLeg1Com.DeltaPerSpread = interSpread.OffsetMultiplier1;
                        spreadLeg2Com.DeltaPerSpread = interSpread.OffsetMultiplier2;
                        spreadLeg1Com.DeltaAvailable = mrgClass1.DeltaToOffset;
                        spreadLeg2Com.DeltaAvailable = mrgClass2.DeltaToOffset;
                        // Calcul du nombre de spreads
                        decimal nbSpread1 = System.Math.Abs(spreadLeg1Com.DeltaAvailable) / spreadLeg1Com.DeltaPerSpread;
                        decimal nbSpread2 = System.Math.Abs(spreadLeg2Com.DeltaAvailable) / spreadLeg2Com.DeltaPerSpread;
                        spreadCom.NumberOfSpread = System.Math.Min(nbSpread1, nbSpread2);
                        // Deltas utilisés
                        spreadLeg1Com.DeltaConsumed = spreadCom.NumberOfSpread * spreadLeg1Com.DeltaPerSpread;
                        spreadLeg2Com.DeltaConsumed = spreadCom.NumberOfSpread * spreadLeg2Com.DeltaPerSpread;
                        // Calcul du Spread Credit
                        spreadLeg1Com.SpreadCredit = spreadLeg1Com.MarginCredit * spreadLeg1Com.DeltaConsumed;
                        spreadLeg2Com.SpreadCredit = spreadLeg2Com.MarginCredit * spreadLeg2Com.DeltaConsumed;
                        if (spreadCom.DiscountType == "P")
                        {
                            spreadLeg1Com.SpreadCredit *= mrgClass1.DeltaPotentialFutureLoss;
                            spreadLeg2Com.SpreadCredit *= mrgClass2.DeltaPotentialFutureLoss;
                        }
                        // Deltas restant
                        if (spreadLeg1Com.DeltaAvailable < 0)
                        {
                            spreadLeg1Com.DeltaConsumed *= -1;
                        }
                        else
                        {
                            spreadLeg2Com.DeltaConsumed *= -1;
                        }
                        spreadLeg1Com.DeltaRemaining = spreadLeg1Com.DeltaAvailable - spreadLeg1Com.DeltaConsumed;
                        spreadLeg2Com.DeltaRemaining = spreadLeg2Com.DeltaAvailable - spreadLeg2Com.DeltaConsumed;
                        // Repport sur les margin class
                        mrgClass1.DeltaToOffset = spreadLeg1Com.DeltaRemaining;
                        mrgClass2.DeltaToOffset = spreadLeg2Com.DeltaRemaining;
                        mrgClass1.InterCommodityCreditAmount.Amount.DecValue += spreadLeg1Com.SpreadCredit;
                        mrgClass2.InterCommodityCreditAmount.Amount.DecValue += spreadLeg2Com.SpreadCredit;
                        // Ajout à la liste des spreads inter commodity
                        interComSpreadList.Add(spreadCom);
                    }
                }
            }
            // Affectation de la liste des spreads inter commodity
            pMethodComObj.InterCommoditySpread = interComSpreadList.ToArray();

            // Montant final par margin class
            foreach (MeffMarginClassCom mgrClass in marginClass)
            {
                mgrClass.MarginAmount.Amount.DecValue = mgrClass.CommodityMarginAmount.Amount.DecValue - mgrClass.InterCommodityCreditAmount.Amount.DecValue;
            }

            // Somme des montants de risque
            marginAmountList = (
                from contractMrg in marginClass
                group contractMrg by contractMrg.MarginAmount.Currency
                    into ctrGrp
                    select new Money(ctrGrp.Sum(a => a.MarginAmount.Amount.DecValue), ctrGrp.Key)
                ).ToList();

            return marginAmountList;
        }

        /// <summary>
        /// Evalue le risque de chaque Margin Class
        /// </summary>
        /// <param name="pMmarginClass">Tableau de Margin Class à évaluer</param>
        private void EvaluateRiskMarginClass(MeffMarginClassCom[] pMmarginClass)
        {
            // Pour chaque Margin Class
            foreach (MeffMarginClassCom mrgClass in pMmarginClass)
            {
                // Prendre les matrices de prix théorique multipliées par chaque quantité en position
                // Pour la matrice de prix théorique : Quantité Long * -1 et Quantité Short * 1
                IEnumerable<Dictionary<int, decimal>> posRiskValue =
                    from pos in mrgClass.Positions
                    join theorPrice in m_TheoreticalPriceRiskArray on pos.First.idAsset equals theorPrice.IdAsset
                    select MultRiskValue(theorPrice.RiskValue, pos.Second.Quantity * (pos.First.Side == SideTools.RetBuyFIXmlSide() ? -1 : 1));

                // Vérifier qu'il ne manque pas de TheoreticalPriceRiskArray
                mrgClass.Missing = (
                    from pos in mrgClass.Positions
                    join theorPrice in m_TheoreticalPriceRiskArray on pos.First.idAsset equals theorPrice.IdAsset
                    into posTheor
                    from theorPrice in posTheor.DefaultIfEmpty(m_EmptyValuationRiskArrayParameters)  // Si les paramètres sont manquants
                    select theorPrice).Count(r => r == m_EmptyValuationRiskArrayParameters) != 0;

                // Cumuler toutes les matrices de prix théorique
                mrgClass.NetPositionMarginArray = CumulRiskValue(posRiskValue);

                // Pour chaque Margin Maturity de la Margin Class
                foreach (MeffMarginMaturityCom mrgMaturity in mrgClass.Parameters)
                {
                    // Prendre les matrices de delta multipliées par chaque quantité en position
                    // Pour la matrice de delta : Quantité Long * 1 et Quantité Short * -1
                    // (qunatité inverse par rapport a la matrice des prix théorique)
                    IEnumerable<Dictionary<int, decimal>> posDeltaValue =
                        from pos in mrgMaturity.Positions
                        join delta in m_DeltaRiskArray on pos.First.idAsset equals delta.IdAsset
                        select MultRiskValue(delta.RiskValue, pos.Second.Quantity * (pos.First.Side == SideTools.RetBuyFIXmlSide() ? 1 : -1));

                    // Vérifier qu'il ne manque pas de DeltaRiskArray
                    mrgMaturity.Missing = (
                        from pos in mrgMaturity.Positions
                        join delta in m_DeltaRiskArray on pos.First.idAsset equals delta.IdAsset
                        into posDelta
                        from delta in posDelta.DefaultIfEmpty(m_EmptyValuationRiskArrayParameters)  // Si les paramètres sont manquants
                        select delta).Count(r => r == m_EmptyValuationRiskArrayParameters) != 0;

                    // Cumuler toutes les matrices de delta
                    mrgMaturity.DeltaArray = CumulRiskValue(posDeltaValue);
                }

                // Réaliser les Time Spreads
                EvaluateTimeSpread(mrgClass);

                // Calcul du Commodity Margin
                if (mrgClass.TimeSpreadMarginArray != default)
                {
                    IEnumerable<Dictionary<int, decimal>> arrays = new[] { mrgClass.NetPositionMarginArray, mrgClass.TimeSpreadMarginArray };
                    mrgClass.CommodityMarginArray = CumulRiskValue(arrays);
                }
                else
                {
                    mrgClass.CommodityMarginArray = mrgClass.NetPositionMarginArray;
                }
                // Commodity Margin Amount
                if ((mrgClass.CommodityMarginArray != default)
                    && (mrgClass.CommodityMarginArray.Count > 0))
                {
                    // Commodity Margin Amount
                    decimal amount = mrgClass.CommodityMarginArray.Max(a => a.Value);
                    mrgClass.CommodityMarginAmount.Amount.DecValue = amount;

                    // Worst Case Scenario
                    int scenario = mrgClass.CommodityMarginArray.Where(a => a.Value == amount).Min(a => a.Key);
                    mrgClass.WorstCaseScenario = scenario;

                    // Net Position Margin Amount
                    if ((mrgClass.NetPositionMarginArray != default)
                        && (mrgClass.NetPositionMarginArray.TryGetValue(scenario, out amount)))
                    {
                        mrgClass.NetPositionMarginAmount.Amount.DecValue = amount;
                    }
                    // Time Spread Margin Amount
                    if ((mrgClass.TimeSpreadMarginArray != default)
                        && (mrgClass.TimeSpreadMarginArray.TryGetValue(scenario, out amount)))
                    {
                        mrgClass.TimeSpreadMarginAmount.Amount.DecValue = amount;
                    }

                    // Class Delta
                    mrgClass.ClassDelta = 0;
                    foreach (MeffMarginMaturityCom marginMaturity in mrgClass.Parameters)
                    {
                        if (marginMaturity.DeltaArray.TryGetValue(scenario, out decimal delta))
                        {
                            mrgClass.ClassDelta += delta;
                        }
                    }

                    // Maximum Delta Offset
                    int closePriceBuyScenario = ((mrgClass.NumberOfValue - 1) / 2) + 1;
                    int closePriceSellcenario = closePriceBuyScenario + mrgClass.NumberOfValue;
                    if (mrgClass.NetPositionMarginArray.TryGetValue(closePriceBuyScenario, out decimal closePriceBuy)
                        && mrgClass.NetPositionMarginArray.TryGetValue(closePriceSellcenario, out decimal closePriceSell))
                    {
                        mrgClass.AccumulatedLossAtClose = (closePriceBuy + closePriceSell) / 2;
                        mrgClass.MarginClassPotentialFutureLoss = mrgClass.CommodityMarginAmount.Amount.DecValue;
                        mrgClass.MarginClassPotentialFutureLoss -= mrgClass.AccumulatedLossAtClose;

                        mrgClass.DeltaPotentialFutureLoss = mrgClass.PriceFluctuation;
                        if (PriceFluctuationPercentage == mrgClass.PriceFluctuationType)
                        {
                            MeffMarginMaturityCom firstMarginMaturity = (MeffMarginMaturityCom)mrgClass.Parameters.FirstOrDefault();
                            mrgClass.UnderlyingPrice = 0;
                            if (firstMarginMaturity != default(MeffMarginMaturityCom))
                            {
                                // 20131104 PM [19123] Suite à modification de regourpement : sur MgrUnlAssetCode
                                //MeffContractAsset assetParameter = m_ContractAssetParameters.FirstOrDefault(a=>a.AssetCode==firstMarginMaturity.MarginAssetCode);
                                //if ((assetParameter != default(MeffContractAsset))
                                //    && StrFunc.IsFilled(assetParameter.MgrUnlAssetCode))
                                //{
                                //assetParameter = m_ContractAssetParameters.FirstOrDefault(a => a.AssetCode == assetParameter.MgrUnlAssetCode);
                                MeffContractAsset assetParameter = m_ContractAssetParameters.FirstOrDefault(a => a.AssetCode == firstMarginMaturity.MarginAssetCode);
                                if (assetParameter != default(MeffContractAsset))
                                {
                                    mrgClass.UnderlyingPrice = assetParameter.Price;
                                }
                                //}
                            }
                            mrgClass.DeltaPotentialFutureLoss = (mrgClass.PriceFluctuation / 100) * mrgClass.UnderlyingPrice;
                        }
                    }
                    if (mrgClass.DeltaPotentialFutureLoss != 0)
                    {
                        mrgClass.MaximumDeltaOffset = mrgClass.MarginClassPotentialFutureLoss / mrgClass.DeltaPotentialFutureLoss;
                    }

                    // Delta To Offset
                    if (mrgClass.MaximumDeltaOffset != 0)
                    {
                        mrgClass.DeltaToOffset = System.Math.Min(System.Math.Abs(mrgClass.ClassDelta), System.Math.Abs(mrgClass.MaximumDeltaOffset));
                        if (mrgClass.ClassDelta < 0)
                        {
                            mrgClass.DeltaToOffset *= -1;
                        }
                    }
                    else
                    {
                        mrgClass.DeltaToOffset = mrgClass.ClassDelta;
                    }
                }
            }
        }

        /// <summary>
        /// Cherche à évaluer les Time Spread d'un Margin Class
        /// </summary>
        /// <param name="pMarginClass">Margin Class à évaluer</param>
        private void EvaluateTimeSpread(MeffMarginClassCom pMarginClass)
        {
            if (pMarginClass != default(MeffMarginClassCom))
            {
                MeffMarginMaturityCom[] marginMaturityCom = (MeffMarginMaturityCom[])pMarginClass.Parameters;
                MeffIntraSpread intraSpreadParameter = m_IntraSpreadParameters.FirstOrDefault(s => s.ArrayCode == pMarginClass.MarginClassCode);

                // S'il y a plus d'une échéance sur la Margin Class
                // et qu'il y a des paramètres de calcul de spread
                if ((marginMaturityCom != default(MeffMarginMaturityCom[]))
                    && (intraSpreadParameter != default(MeffIntraSpread))
                    && (marginMaturityCom.Count() > 1))
                {
                    // 20131104 PM [19123]
                    // Vérifier que chaque échéance est unique
                    var verifyUniqueMaturityKey = from maturity in marginMaturityCom group maturity by maturity.MaturityDate into maturityKey select maturityKey.Key;
                    if (verifyUniqueMaturityKey.Count() == marginMaturityCom.Count())
                    {
                        Dictionary<DateTime, MeffMarginMaturityCom> marginMaturity = marginMaturityCom.ToDictionary(k => k.MaturityDate, k => k);
                        IEnumerable<DateTime> maturityList = marginMaturity.Select(m => m.Key).OrderByDescending(d => d);
                        List<Dictionary<int, decimal>> globalSpreadDeltaValueList = new List<Dictionary<int, decimal>>();
                        foreach (DateTime maturity in maturityList)
                        {
                            List<Dictionary<int, decimal>> spreadDeltaValueList = TryProceedTimeSpread(intraSpreadParameter, marginMaturity, maturity, maturityList);
                            globalSpreadDeltaValueList = globalSpreadDeltaValueList.Concat(spreadDeltaValueList).ToList();
                        }

                        pMarginClass.TimeSpreadMarginArray = CumulRiskValue(globalSpreadDeltaValueList);
                        if (pMarginClass.TimeSpreadMarginArray.Count == 0)
                        {
                            pMarginClass.TimeSpreadMarginArray = default;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tente de créer un Time Spread entre la Maturity courante et les Maturities suivantes
        /// </summary>
        /// <param name="pIntraSpreadParameter">Paramètres de Time Spread</param>
        /// <param name="pMarginMaturity">Dictionnaire de Margin Maturity</param>
        /// <param name="pMaturity">Maturity courante</param>
        /// <param name="pMaturityList">Liste des Maturities suivantes</param>
        /// <returns></returns>
        private List<Dictionary<int, decimal>> TryProceedTimeSpread(MeffIntraSpread pIntraSpreadParameter,
            Dictionary<DateTime, MeffMarginMaturityCom> pMarginMaturity,
            DateTime pMaturity,
            IEnumerable<DateTime> pMaturityList)
        {
            List<Dictionary<int, decimal>> spreadDeltaValueList = new List<Dictionary<int, decimal>>();
            if ((pMaturityList != default(IEnumerable<DateTime>))
                && (pIntraSpreadParameter != default(MeffIntraSpread)))
            {
                IEnumerable<DateTime> previousMaturityList = pMaturityList.Where(d => d < pMaturity);
                // S'il existe des échéances plus proche
                if (previousMaturityList.Count() > 0)
                {
                    foreach (DateTime maturity in previousMaturityList)
                    {
                        Dictionary<int, decimal> spreadDeltaValue = TryCreateTimeSpread(pIntraSpreadParameter, pMarginMaturity, pMaturity, maturity);
                        if (spreadDeltaValue != default)
                        {
                            spreadDeltaValueList.Add(spreadDeltaValue);
                        }
                        spreadDeltaValueList.Concat(TryProceedTimeSpread(pIntraSpreadParameter, pMarginMaturity, maturity, previousMaturityList));
                    }
                }
            }
            return spreadDeltaValueList;
        }

        /// <summary>
        /// Tente de créer un Time Spread entre deux Maturities
        /// </summary>
        /// <param name="pIntraSpreadParameter">Paramètres de Time Spread</param>
        /// <param name="pMarginMaturity">Dictionnaire de Margin Maturity</param>
        /// <param name="pMaturityFirst">Première Maturity</param>
        /// <param name="pMaturitySecond">Deuxième Maturity</param>
        /// <returns></returns>
        private Dictionary<int, decimal> TryCreateTimeSpread(MeffIntraSpread pIntraSpreadParameter,
            Dictionary<DateTime, MeffMarginMaturityCom> pMarginMaturity,
            DateTime pMaturityFirst,
            DateTime pMaturitySecond)
        {
            Dictionary<int, decimal> spreadDeltaValue = default;
            if (pMarginMaturity.TryGetValue(pMaturityFirst, out MeffMarginMaturityCom firstMarginMaturity)
                && pMarginMaturity.TryGetValue(pMaturitySecond, out MeffMarginMaturityCom secondMarginMaturity))
            {
                bool isSpreadPossible = false;
                // Vérifier si les matrices de delta ont au moins une valeur opposée
                foreach (var firstMaturityValue in firstMarginMaturity.DeltaArray)
                {
                    if (secondMarginMaturity.DeltaArray.TryGetValue(firstMaturityValue.Key, out decimal deltaValue))
                    {
                        if ((deltaValue < 0 && firstMaturityValue.Value > 0)
                            || (deltaValue > 0 && firstMaturityValue.Value < 0))
                        {
                            isSpreadPossible = true;
                            break;
                        }
                    }
                }
                if (isSpreadPossible)
                {
                    decimal spreadMarginAmount = pIntraSpreadParameter.Spread;
                    if (spreadMarginAmount == 0)
                    {
                        spreadMarginAmount = System.Math.Max(System.Math.Abs(firstMarginMaturity.Price - secondMarginMaturity.Price), pIntraSpreadParameter.MinimumValue);
                        spreadMarginAmount *= pIntraSpreadParameter.Factor;
                    }
                    if (spreadMarginAmount != 0)
                    {
                        spreadDeltaValue = ConsumeSpreadDeltaValues(firstMarginMaturity.DeltaArray, secondMarginMaturity.DeltaArray);
                        spreadDeltaValue = MultRiskValue(spreadDeltaValue, spreadMarginAmount);
                    }
                }
            }
            return spreadDeltaValue;
        }
        #endregion methods
    }
}

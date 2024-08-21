using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Spheres.Hierarchies;
//
using EfsML.Business;
using EfsML.Enum;
//
using FixML.Enum;
//
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class de constitution des paniers d'actions ne vue de réduire les positions ETD
    /// </summary>
    // PM 20201028 [25570][25542] New
    public class BasketBuilder
    {
        #region private class
        /// <summary>
        /// Class de manipulation des positions et paramétres d'un equity membre du basket
        /// </summary>
        private class BasketConstituantInfo
        {
            #region Members
            /// <summary>
            /// Id de l'equity
            /// </summary>
            public int IdAsset;
            /// <summary>
            /// Poids paramétré de l'equity au sein du basket
            /// </summary>
            public decimal InitialWeight;
            /// <summary>
            /// Poids calculé de l'equity au sein du basket
            /// </summary>
            public decimal Weight;
            /// <summary>
            /// Ensemble des positions présentes sur l'equity
            /// </summary>
            public List<StocksCoverageParameter> Position;
            /// <summary>
            /// Quantité total de l'ensemble des positions présentes sur l'equity
            /// </summary>
            public decimal TotalQuantity = 0;
            /// <summary>
            /// Ensemble des facteurs premiers résultant de la décomposition du poids paramétré
            /// </summary>
            public List<int> Factor;
            #endregion Members

            #region Constructor
            /// <summary>
            /// Constrution
            /// </summary>
            /// <param name="pIdAsset">Id de l'equity</param>
            /// <param name="pInitialWeight">Poids paramétré de l'equity au sein du basket</param>
            /// <param name="pPosition">Ensemble des positions présentes sur l'equity</param>
            public BasketConstituantInfo(int pIdAsset, decimal pInitialWeight, List<StocksCoverageParameter> pPosition)
            {
                IdAsset = pIdAsset;

                InitialWeight = pInitialWeight;
                Weight = pInitialWeight;

                if (pPosition != default(List<StocksCoverageParameter>))
                {
                    Position = pPosition;
                }
                else
                {
                    Position = new List<StocksCoverageParameter>();
                }

                if (Position.Count() > 0)
                {
                    // Quantité total en position pour le constituant
                    TotalQuantity = Position.Sum(e => e.Quantity);
                }
            }
            #endregion Constructor

            #region Methods
            /// <summary>
            /// Décomposition du poids paramétré en facteurs premier
            /// </summary>
            public void ComputeFactor()
            {
                Factor = BasketBuilder.DecompositionFacteurPremier((int)InitialWeight);
            }
            #endregion Methods
        }
        #endregion private class

        #region Members
        /// <summary>
        /// Paramétrage du basket
        /// </summary>
        private readonly BasketCoverageSetting m_BasketSetting;

        /// <summary>
        /// Ensemble des positions et paramétres des equities constituant le basket
        /// </summary>
        private Dictionary<int, BasketConstituantInfo> m_Constituant;
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pBasketSetting">Paramétrage du basket</param>
        public BasketBuilder(BasketCoverageSetting pBasketSetting)
        {
            m_BasketSetting = pBasketSetting;
            m_Constituant = new Dictionary<int, BasketConstituantInfo>();
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Construction d'un basket à partir de position des ses constituants
        /// </summary>
        /// <param name="pStocks">Ensemble des positions equities constituant le basket</param>
        /// <returns></returns>
        public StocksCoverageParameter BuildBasket(List<StocksCoverageParameter> pStocks)
        {
            StocksCoverageParameter newBasket = default;
            m_Constituant.Clear();
            if ((m_BasketSetting != default(BasketCoverageSetting)) && (pStocks != default(List<StocksCoverageParameter>)) && (m_BasketSetting.EquityConstituent != default))
            {
                // Vérifier que le poids de chaque constituant est supérieur à 0
                if (m_BasketSetting.EquityConstituent.Where(e => e.Value <= 0).Count() == 0)
                {
                    // Constitution de l'ensemble des informations sur les constituants du basket à partir de la définition du basket et des positions equity
                    m_Constituant = m_BasketSetting.EquityConstituent.ToDictionary(b => b.Key, b => new BasketConstituantInfo(b.Key, b.Value, pStocks.Where(e => e.EquityAssetId == b.Key).ToList()));

                    // Vérifier qu'il y a une position pour chaque constituant
                    if (m_Constituant.Where(c => (c.Value.Position.Count() == 0) || (c.Value.TotalQuantity == 0)).Count() == 0)
                    {
                        // Plus petit poids (il est forcement supérieur à 0)
                        decimal minWeigth = m_BasketSetting.EquityConstituent.Values.Min();

                        #region Pré-gestion pour un basket en pourcentage
                        // Pré-gestion pour un basket en pourcentage
                        if (m_BasketSetting.BasketUnitTypeWeight == BasketUnitTypeWeightEnum.basketPercentage)
                        {
                            bool isPercentMultiple = true;

                            // Recherche si le poids de chaque constituant est un multiple du plus petit poids
                            foreach (BasketConstituantInfo constituant in m_Constituant.Values)
                            {
                                decimal newWeigth = System.Math.Round((constituant.InitialWeight / minWeigth), 8);
                                if ((int)newWeigth == newWeigth)
                                {
                                    constituant.Weight = newWeigth;
                                }
                                else
                                {
                                    isPercentMultiple = false;
                                    break;
                                }
                            }

                            if (false == isPercentMultiple)
                            {
                                int minFactor = 1;

                                // Inutile de faire une décomposition, il suffit de prendre comme poids unitaire le plus petit des poids et de calcul les autres poids en fonction de celui-ci
                                //// Décomposition en facteurs premier
                                //foreach (BasketConstituantInfo constituant in m_Constituant.Values)
                                //{
                                //    constituant.ComputeFactor();
                                //}
                                //// Recherche de facteurs premier commun
                                //IEnumerable<int> commonFactor = m_Constituant.Values.Select(c => c.Factor).Aggregate((a, b) => (a).Intersect<int>(b).ToList());
                                // Recherche du plus petit facteur premier commun
                                //if (commonFactor.Count() != 0)
                                //{
                                //    minFactor = commonFactor.Min();
                                //}

                                // Calcul des poids unitaire
                                foreach (BasketConstituantInfo constituant in m_Constituant.Values)
                                {
                                    if (constituant.InitialWeight == minWeigth)
                                    {
                                        constituant.Weight = minFactor;
                                    }
                                    else
                                    {
                                        constituant.Weight = (constituant.InitialWeight / minWeigth * minFactor);
                                    }
                                }
                            }
                        }
                        #endregion Pré-gestion pour un basket en pourcentage

                        // Construction du basket
                        bool isFirstConstituant = true;
                        int nbPossibleBasket = 0;

                        // Première étape: Calcul du nombre de basket pouvant être construit
                        foreach (BasketConstituantInfo constituant in m_Constituant.Values)
                        {
                            int nbBasket = (int)System.Math.Floor(constituant.TotalQuantity / constituant.Weight);
                            if (isFirstConstituant)
                            {
                                nbPossibleBasket = nbBasket;
                                isFirstConstituant = false;
                            }
                            else
                            {
                                nbPossibleBasket = System.Math.Min(nbPossibleBasket, nbBasket);
                            }
                        }
                        if (nbPossibleBasket > 0)
                        {
                            // Deuxième étape: Construction du basket
                            foreach (KeyValuePair<int, decimal> assetSetting in m_BasketSetting.EquityConstituent)
                            {
                                BasketConstituantInfo constituant = m_Constituant[assetSetting.Key];
                                List<StocksCoverageParameter> constituantPos = constituant.Position;
                                int quantityForBasket = (int)System.Math.Ceiling(nbPossibleBasket * constituant.Weight);
                                foreach (StocksCoverageParameter stock in constituantPos)
                                {
                                    stock.InitialQuantity = stock.Quantity;
                                    if (stock.Quantity >= quantityForBasket)
                                    {
                                        stock.Quantity -= quantityForBasket;
                                        quantityForBasket = 0;
                                    }
                                    else
                                    {
                                        quantityForBasket -= (int)stock.Quantity;
                                        stock.Quantity = 0;
                                    }
                                    if (quantityForBasket == 0)
                                    {
                                        break;
                                    }
                                }
                            }
                            // Génération du basket
                            StocksCoverageParameter model = m_Constituant.First().Value.Position.First();
                            newBasket = new StocksCoverageParameter
                            {
                                UnlAssetId = model.UnlAssetId,
                                ContractId = model.ContractId,
                                MarketId = model.MarketId,
                                PayActorId = model.PayActorId,
                                PayActorIdentifier = model.PayActorIdentifier,
                                PayBookId = model.PayBookId,
                                PayBookIdentifier = model.PayBookIdentifier,
                                RecActorId = model.RecActorId,
                                RecActorIdentifier = model.RecActorIdentifier,
                                RecBookId = model.RecBookId,
                                RecBookIdentifier = model.RecBookIdentifier,
                                Type = model.Type,
                                BasketUnitTypeWeight = model.BasketUnitTypeWeight,
                                GroupByContractId = model.GroupByContractId,

                                EquityAssetId = model.UnlAssetId,
                                AssetIdentifier = model.UnlAssetIdentifier,
                                UnlAssetIdentifier = model.UnlAssetIdentifier,
                                EquityAssetClass = model.UnlAssetClass,
                                UnlAssetClass = model.UnlAssetClass,
                                PosEquityIdentifier = "Built Basket from: " + pStocks.Select(e => e.PosEquityIdentifier).Aggregate((e, f) => e + "," + f),
                                Quantity = nbPossibleBasket,
                                InitialQuantity = nbPossibleBasket,
                                StocksConstituant = pStocks
                            };
                        }
                        else
                        {
                            m_Constituant.Clear();
                        }
                    }
                    else
                    {
                        m_Constituant.Clear();
                    }
                }
            }
            return newBasket;
        }

        /// <summary>
        /// Décomposition en facteur premier
        /// </summary>
        /// <param name="pNombre"></param>
        /// <returns>Liste des facteurs premiers</returns>
        private static List<int> DecompositionFacteurPremier(int pNombre)
        {
            List<int> facteur = new List<int>();
            if (pNombre >= 2)
            {
                for (int i = 2; i <= pNombre; i++)
                {
                    while ((pNombre % i) == 0)
                    {
                        facteur.Add(i);
                        pNombre /= i;
                    }
                }
            }
            return facteur;
        }
        #endregion Methods
    }

    /// <summary>
    /// Class de stockage des paniers d'actions en vue de réduire les positions ETD
    /// </summary>
    // PM 20201028 [25570][25542] New
    public class BasketCoverageSetting
    {
        #region Members
        /// <summary>
        /// Id de l'asset panier
        /// </summary>
        private readonly int m_BasketIdAsset;

        /// <summary>
        /// Identifier de l'asset panier
        /// </summary>
        private readonly string m_BasketIdentifier;

        /// <summary>
        /// Type de poids des constituants du panier d'equity
        /// </summary>
        private readonly BasketUnitTypeWeightEnum m_BasketUnitTypeWeight;

        /// <summary>
        /// Ensemble des Equity du Basket (IdAsset, Poids)
        /// </summary>
        private readonly Dictionary<int, decimal> m_EquityConstituent;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Id de l'asset panier
        /// </summary>
        public int BasketIdAsset
        { get { return m_BasketIdAsset; } }

        /// <summary>
        /// Identifier de l'asset panier
        /// </summary>
        public string BasketIdentifier
        { get { return m_BasketIdentifier; } }

        /// <summary>
        /// Type de poids des constituants du panier d'equity
        /// </summary>
        public BasketUnitTypeWeightEnum BasketUnitTypeWeight
        { get { return m_BasketUnitTypeWeight; } }

        /// <summary>
        /// Ensemble des Equity du Basket (IdAsset, Poids)
        /// </summary>
        public Dictionary<int, decimal> EquityConstituent
        { get { return m_EquityConstituent; } }

        /// <summary>
        /// Somme des Poids des constituent du panier
        /// </summary>
        public decimal TotalWeight
        {
            get
            {
                return (m_EquityConstituent != default ? m_EquityConstituent.Sum(e => e.Value) : 0);
            }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pBasketIdAsset"></param>
        /// <param name="pBasketIdentifier"></param>
        /// <param name="pBasketUnitTypeWeight"></param>
        /// <param name="pEquityConstituent"></param>
        public BasketCoverageSetting(int pBasketIdAsset, string pBasketIdentifier, BasketUnitTypeWeightEnum pBasketUnitTypeWeight, Dictionary<int, decimal> pEquityConstituent)
        {
            m_BasketIdAsset = pBasketIdAsset;
            m_BasketIdentifier = pBasketIdentifier;
            m_BasketUnitTypeWeight = pBasketUnitTypeWeight;
            m_EquityConstituent = pEquityConstituent;
        }
        #endregion Constructor
    }


    /// <summary>
    /// Classe de chargement d'un jeu de paramètres de Type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class LoadParametersMethod<T>
    {
        /// <summary>
        /// Charge un jeu de paramètres dans une liste.
        /// </summary>
        /// <param name="pConnection">Connection courante</param>
        /// <param name="pDbParametersValue">Collection de valeurs pour les paramètres de la requetes de chargement</param>
        /// <param name="pResultSets">Type du jeu de paramètres à charger</param>
        /// <returns>Liste des paramètres lus</returns>
        public static List<T> LoadParameters(IDbConnection pConnection, Dictionary<string, object> pDbParametersValue, DataContractResultSets pResultSets)
        {
            return LoadParameters(pConnection, pDbParametersValue, null, pResultSets);
        }
        /// <summary>
        /// Charge un jeu de paramètres dans une liste.
        /// </summary>
        /// <param name="pConnection">Connection courante</param>
        /// <param name="pDbParametersValue">Collection de valeurs pour les paramètres de la requetes de chargement</param>
        /// <param name="pId">Liste d'Id d'éléments (Asset, DerivativeContract, ...) en position</param>
        /// <param name="pResultSets">Type du jeu de paramètres à charger</param>
        /// <returns>Liste des paramètres lus</returns>
        public static List<T> LoadParameters(IDbConnection pConnection, Dictionary<string, object> pDbParametersValue, int[] pId, DataContractResultSets pResultSets)
        {
            CommandType cmdTyp = DataContractHelper.GetType(pResultSets);
            string request;
            if (null != pId)
            {
                object[] boxedId = new object[pId.Length];
                pId.CopyTo(boxedId, 0);

                object[][] valuesMatrix = { boxedId };

                request = DataContractHelper.GetQuery(pResultSets, valuesMatrix);
            }
            else
            {
                request = DataContractHelper.GetQuery(pResultSets);
            }

            IDbDataParameter[] dbParameters = DataContractHelper.GetDbDataParameters(pResultSets, pDbParametersValue);

            return DataHelper<T>.ExecuteDataSet(pConnection, cmdTyp, request, dbParameters);
        }
    }

    /// <summary>
    /// helper class containing all the extensions valid for the risk method namespace
    /// </summary>
    public static class RiskMethodExtensions
    {
        /// <summary>
        /// Extends the DateTime type to return the numeric year month of the current date 
        /// </summary>
        /// <param name="businessDate"></param>
        /// <returns>the numeric representation of the input date in YYYYMM</returns>
        public static decimal InNumericYearMonth(this DateTime businessDate)
        {
            return (businessDate.Year * 100) + businessDate.Month;
        }

        /// <summary>
        /// Extends the RiskMarginPosition struct to get the right margin sign of a net quantity, according with the TIMS market specification.
        /// </summary>
        /// <remarks>
        /// The net assignation/exercise/liquidation quantities come with the right sign. no manipulation is required.
        /// The net short/long (and position actions) quantities come with an absolute value, 
        /// a manipulation is required according with the following rules: 
        /// <list type="">
        /// <item>For net short open quantities, the margin represents the liquidation costs and thus the corresponding margin requirements,
        /// which has positive sign. 
        /// </item>
        /// <item>On the other hand, the margin of net long quantities represents the liquidation proceeds and consequently the margin credit, 
        /// which has a NEGATIVE sign. </item>
        /// </list>
        /// </remarks>
        /// <param name="position">the quantity to transform</param>
        /// <param name="pQtyType">execution/assingation indicator</param>
        /// <param name="side">Side of the position, http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag54.html</param>
        /// <returns>the same quantity value, but with the right margin sign</returns>
        public static decimal QuantityWithMarginSign(this RiskMarginPosition position, RiskMethodQtyType pQtyType, string side)
        {
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal signedquantity;
            switch (pQtyType)
            {
                case RiskMethodQtyType.ExeAssCall:
                case RiskMethodQtyType.ExeAssPut:
                case RiskMethodQtyType.FutureMoff:

                    signedquantity = position.ExeAssQuantity;

                    break;


                case RiskMethodQtyType.PositionAction:
                case RiskMethodQtyType.Future:
                case RiskMethodQtyType.Call:
                case RiskMethodQtyType.Put:
                default:

                    signedquantity = (side == "2") ?
                        position.Quantity
                        :
                        // long quantity
                        (-1) * position.Quantity;

                    break;
            }

            return signedquantity;
        }

        /// <summary>
        /// Extends RiskMarginPosition struct in order to transcode from RiskMethodQtyType internal enum to Fix PosType enum
        /// </summary>
        /// <remarks>the case RiskMethodQtyType.PositionAction is treated partially, 
        /// beacuse no cross margin scenario can be recognized, attend to get PosType.PA ever. no PosType.XM can be returned</remarks>
        /// <param name="position">the position dataset</param>
        /// <param name="pQtyType">the quantity type we want to transcode to Fix </param>
        /// <returns></returns>
        public static PosType GetPosType(this RiskMarginPosition position, RiskMethodQtyType pQtyType)
        {
            // by default we consider allocations
            PosType type;
            switch (pQtyType)
            {
                case RiskMethodQtyType.ExeAssCall:
                case RiskMethodQtyType.ExeAssPut:

                    type = position.ExeAssQuantity > 0 ? PosType.AS : PosType.EX;

                    break;

                case RiskMethodQtyType.FutureMoff:

                    type = PosType.DN;

                    break;

                case RiskMethodQtyType.PositionAction:

                    type = PosType.PA;

                    break;

                case RiskMethodQtyType.Future:
                case RiskMethodQtyType.Call:
                case RiskMethodQtyType.Put:
                default:

                    type = PosType.ALC;

                    break;
            }

            return type;


        }

        /// <summary>
        /// Fournit la valeur de l'enum RiskMethodQtyType correspondant pour la Catégorie de DerivativeContract et le type d'option Call/Put
        /// </summary>
        /// <param name="pCategory">Catégorie de DerivativeContract ('F'uture ou 'O'ption)</param>
        /// <param name="pPutOrCall">Indicateur FIX du type d'option Call ou Put pour une option</param>
        /// <returns></returns>
        public static RiskMethodQtyType GetTypeFromCategoryPutCall(Nullable<CfiCodeCategoryEnum> pCategory, Nullable<PutOrCallEnum> pPutOrCall)
        {
            RiskMethodQtyType type = default;

            switch (pCategory)
            {
                case CfiCodeCategoryEnum.Option:
                    if (pPutOrCall == PutOrCallEnum.Put)
                    {
                        type = RiskMethodQtyType.Put;
                    }
                    else if (pPutOrCall == PutOrCallEnum.Call)
                    {
                        type = RiskMethodQtyType.Call;
                    }
                    break;
                case CfiCodeCategoryEnum.Future:
                default:
                    type = RiskMethodQtyType.Future;
                    break;
            }
            return type;
        }

        /// <summary>
        /// Calcul la valeur d'une position sur un contrat
        /// </summary>
        /// <param name="pQuantity">Quantité en position</param>
        /// <param name="pPrice">Prix unitaire du contrat</param>
        /// <param name="pMultiplier">Multiplieur du contrat</param>
        /// <param name="pInstrumentNum">Numérateur du prix du contrat</param>
        /// <param name="pInstrumentDen">Dénominateur du prix du contrat</param>
        /// <returns>La quantité valorisée</returns>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        public static decimal ContractValue(decimal pQuantity, decimal pPrice, decimal pMultiplier, int pInstrumentNum, int pInstrumentDen)
        {
            if (pInstrumentNum <= 0) pInstrumentNum = 1;
            return pQuantity * pMultiplier * ExchangeTradedDerivativeTools.ToBase100(pPrice, pInstrumentNum, pInstrumentDen);
        }
        /// <summary>
        /// Calcul la valeur d'une position sur un contrat
        /// </summary>
        /// <param name="pQuantity">Quantité en position</param>
        /// <param name="pPrice">Prix unitaire du contrat</param>
        /// <param name="pMultiplier">Multiplieur du contrat</param>
        /// <param name="pInstrumentNum">Numérateur du prix du contrat</param>
        /// <param name="pInstrumentDen">Dénominateur du prix du contrat</param>
        /// <param name="pCashFlowCalculationMethod">Méthode d'arrondie de la valorisation</param>
        /// <param name="pRoundDir">Direction de l'arrondie</param>
        /// <param name="pRoundPrec">Précision de l'arrondie</param>
        /// <returns>La quantité valorisée</returns>
        /// PM 20150707 [21104] New
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        public static decimal ContractValue(decimal pQuantity, decimal pPrice, decimal pMultiplier, int pInstrumentNum, int pInstrumentDen, CashFlowCalculationMethodEnum pCashFlowCalculationMethod, string pRoundDir, int pRoundPrec)
        {
            if (pInstrumentNum <= 0) pInstrumentNum = 1;
            decimal price100 = ExchangeTradedDerivativeTools.ToBase100(pPrice, pInstrumentNum, pInstrumentDen);
            decimal value = ExchangeTradedDerivativeTools.CashFlowValorization(pCashFlowCalculationMethod, price100, 0, pMultiplier, pQuantity, pRoundDir, pRoundPrec);
            return value;
        }

        /// <summary>
        /// Calcul la valeur d'une position sur un contrat
        /// </summary>
        /// <param name="pAssetParameter">Caractèristique de l'asset</param>
        /// <param name="pQuantity">Quantité en position</param>
        /// <param name="pPrice">Prix unitaire du contrat</param>
        /// <returns>La quantité valorisée</returns>
        /// PM 20150707 [21104] New
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        public static decimal ContractValue(AssetExpandedParameter pAssetParameter, decimal pQuantity, decimal pPrice)
        {
            return ContractValue(pQuantity, pPrice, pAssetParameter.ContractMultiplier, pAssetParameter.InstrumentNum, pAssetParameter.InstrumentDen, pAssetParameter.CashFlowCalcMethodEnum, pAssetParameter.RoundDir, pAssetParameter.RoundPrec);
        }

        /// <summary>
        /// Calcul la valeur d'une position sur un contrat
        /// </summary>
        /// <param name="pAssetParameter">Caractèristique de l'asset</param>
        /// <param name="pQuantity">Quantité en position</param>
        /// <param name="pPrice">Prix unitaire du contrat</param>
        /// <param name="pUseContractValueFactor">Indique s'il faut utiliser le ContractValueFactor</param>
        /// <param name="pContractValueFactor">ContractValueFactor</param>
        /// <returns>La quantité valorisée</returns>
        /// PM 20150707 [21104] New
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        public static decimal ContractValue(AssetExpandedParameter pAssetParameter, decimal pQuantity, decimal pPrice, bool pUseContractValueFactor, decimal pContractValueFactor)
        {
            decimal cvf = (pUseContractValueFactor && (pContractValueFactor != 0)) ? pContractValueFactor : pAssetParameter.ContractMultiplier;
            return ContractValue(pQuantity, pPrice, cvf, pAssetParameter.InstrumentNum, pAssetParameter.InstrumentDen, pAssetParameter.CashFlowCalcMethodEnum, pAssetParameter.RoundDir, pAssetParameter.RoundPrec);
        }

        /// <summary>
        /// Delete all the actor ids (we found in position) related to the current session
        /// </summary>
        /// <param name="pHierarchy">instance object (not used by now)</param>
        /// <param name="pCS">the connection string to the DB</param>
        /// <param name="pSessionId">session id of the current application instance</param>
        /// <remarks>20120712 MF Ticket 18004</remarks>
        // EG 20180803 PERF New
        public static void TruncateImActor(this ActorRoleHierarchy _, string pCS)
        {
            string queryTruncate = DataContractHelper.GetQuery(DataContractResultSets.TRUNCATEIMACTOR);

            if (String.IsNullOrEmpty(queryTruncate))
            {
                return;
            }

            CommandType queryType = DataContractHelper.GetType(DataContractResultSets.TRUNCATEIMACTOR);
            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                DataHelper.ExecuteNonQuery(connection, queryType, queryTruncate);
            }
        }

        /// <summary>
        /// Insert all the loaded actors inside the IMACTOR table
        /// </summary>
        /// <param name="pHierarchy">instance object</param>
        /// <param name="pCS">the connection string to the DB</param>
        /// <param name="pTableId"></param>
        /// <remarks>20120712 MF Ticket 18004</remarks>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTOR_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        // EG 20181119 PERF Correction post RC (Step 2)
        public static void InsertImActor(this ActorRoleHierarchy pHierarchy, string pCS, string pTableId)
        {
            string queryInsert = DataContractHelper.GetQuery(DataContractResultSets.INSERTIMACTOR);

            CommandType queryType = DataContractHelper.GetType(DataContractResultSets.INSERTIMACTOR);

            List<ActorNode> actors = new List<ActorNode>
            {
                pHierarchy.Root
            };
            // Using Union instead AddRange to ge rid of elements duplicated
            actors = actors.Union(pHierarchy.Root.FindChilds(actornode => actornode != null && actornode.Built)).ToList();
            Dictionary<string, object> dbParameterValues = new Dictionary<string, object>();

            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                foreach (ActorNode actor in actors)
                {
                    int idA = actor.Id;
                    // UNDONE 20120712 MF the book is actually not used (propose the column suppression onto IMACTOR table)
                    int idB = 0;
                    dbParameterValues.Add("IDA", idA);
                    dbParameterValues.Add("IDB", idB);

                    DataHelper.ExecuteNonQuery(connection, queryType, queryInsert,
                        DataContractHelper.GetDbDataParameters(DataContractResultSets.INSERTIMACTOR, dbParameterValues));

                    dbParameterValues.Remove("IDA");
                    dbParameterValues.Remove("IDB");
                }
            }
            if (DataHelper.GetSvrInfoConnection(pCS).IsOracle)
                DataHelper.UpdateStatTable(pCS, String.Format("IMACTOR_{0}_W", pTableId).ToUpper());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class RiskTools
    {
        /// <summary>
        /// Indique si une échéance est l'échéance spot par rapport à une date de bourse
        /// Attention: Ne fonctionne qu'avec les échéances au format YYYYMM
        /// </summary>
        /// <param name="pMaturity">Echéance à vérifier</param>
        /// <param name="pMaturityFrequency">Régle d'échéance de l'échéance</param>
        /// <param name="pBusinessDate">Date de bourse</param>
        /// <returns>True si l'échéance et l'échéance spot</returns>
        public static bool IsSpotMonth(decimal pMaturity, string pMaturityFrequency, DateTime pBusinessDate)
        {
            return IsSpotMonth(pMaturity.ToString(), pMaturityFrequency, pBusinessDate);
        }
        /// <summary>
        /// Indique si une échéance est l'échéance spot par rapport à une date de bourse
        /// Attention: Ne fonctionne qu'avec les échéances au format YYYYMM
        /// </summary>
        /// <param name="pMaturity">Echéance à vérifier</param>
        /// <param name="pMaturityFrequency">Régle d'échéance de l'échéance</param>
        /// <param name="pBusinessDate">Date de bourse</param>
        /// <returns></returns>
        public static bool IsSpotMonth(string pMaturity, string pMaturityFrequency, DateTime pBusinessDate)
        {
            bool isSpot = false;
            if (StrFunc.IsFilled(pMaturity))
            {
                if (pMaturity.Length == 6)
                {
                    int maturityMonthYear = IntFunc.IntValue(pMaturity);   // Convertion de l'échéance en numérique
                    int businessMonthYear = (int)pBusinessDate.InNumericYearMonth();    // Convertion du mois et de l'année de la date de bourse en numérique

                    // Cas où la date de bourse est dans le mois de l'échéance
                    isSpot = (maturityMonthYear == businessMonthYear);

                    if (!isSpot && StrFunc.IsFilled(pMaturityFrequency))
                    {
                        // !isSpot => Le mois/année de la date de bourse ne correspond pas à l'échéance
                        int businessMonth = pBusinessDate.Month;
                        int spotMonth = 0; // Mois de l'échéance spot
                        int spotYear = pBusinessDate.Year;  // Année de l'échéance spot

                        // Transformer le code de fréquence des échéances en un ensemble de numéro de mois trié par ordre croissant
                        IEnumerable<int> monthFrequency = (from monthLetter in pMaturityFrequency.ToCharArray()
                                                           select IntFunc.IntValue(StrFunc.GetMonthMM(monthLetter.ToString()))).OrderBy(i => i);

                        // Si le mois de la date de bourse et supérieur à tous les mois d'échéance
                        if (monthFrequency.Max() < businessMonth)
                        {
                            spotYear += 1; // L'année d'échéance spot est donc l'année suivante
                            spotMonth = monthFrequency.Min(); // Le mois de l'échéance spot est donc le premier mois d'échéance de l'année
                            isSpot = (maturityMonthYear == (spotYear * 100 + spotMonth));
                        }
                        else
                        {
                            // Vérifier que l'année de l'échéance correspond à l'année de l'échéance spot
                            if (Int64.Parse(pMaturity.Substring(0, 4)) == spotYear)
                            {
                                // Prendre le plus petit mois d'échéance supérieur ou égal au mois de la date de bourse
                                spotMonth = monthFrequency.Where(i => i >= businessMonth).Min();
                                isSpot = (maturityMonthYear == (spotYear * 100 + spotMonth));
                            }
                        }
                    }
                }
            }
            return isSpot;
        }
        /// <summary>
        /// Mets à 0 les montants négatifs d'une liste de Money
        /// </summary>
        /// <param name="pAmount">Liste de Money</param>
        /// <returns>La liste reçu en paramètre</returns>
        public static List<Money> SetNegativeToZero(List<Money> pAmount)
        {
            if (pAmount != default)
            {
                foreach (Money amount in pAmount)
                {
                    if (amount.Amount.DecValue < 0)
                    {
                        amount.Amount.DecValue = 0;
                    }
                }
            }
            return pAmount;
        }
    }
}

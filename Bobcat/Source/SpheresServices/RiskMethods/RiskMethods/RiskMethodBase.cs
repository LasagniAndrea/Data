using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient.LoggerService;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.EOD;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.v30.Fix;
//
using FixML.Enum;
//
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Base class representing a generic Risk method used to compute a deposit
    /// </summary>
    public abstract class BaseMethod
    {
        #region Members
        /// <summary>
        /// Paramètres généraux de la méthode
        /// </summary>
        /// PM 20160404 [22116] New
        protected ImMethodParameter m_ImMethodParameter;

        /// <summary>
        /// Type de données utilisées par la méthode pour l'évaluation du déposit
        /// </summary>
        /// PM 20170313 [22833] Ajout
        protected RiskMethodDataTypeEnum m_RiskMethodDataType;

        /// <summary>
        /// Id interne de la tâche de lecture des RiskDatas
        /// </summary>
        /// PM 20180219 [23824] Ajout m_IdIOTaskRiskData
        protected int m_IdIOTaskRiskData;

        /// <summary>
        /// Indique s'il faut lire les données de risque pour les jours fériés marché
        /// </summary>
        /// PM 20180530 [23824] Ajout m_IsRiskDataOnHoliday
        protected bool m_IsRiskDataOnHoliday;

        /// <summary>
        /// Information sur le process
        /// </summary>
        // PM 20180219 [23824]  Ajout
        protected RiskPerformanceProcessInfo m_ProcessInfo;

        /// <summary>
        /// Devise par défaut, celle du CSS
        /// </summary>
        protected string m_CssCurrency = null;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Paramètres généraux de la méthode
        /// </summary>
        /// PM 20160404 [22116] New
        public ImMethodParameter MethodParameter
        {
            get { return m_ImMethodParameter; }
            set { m_ImMethodParameter = value; }
        }

        /// <summary>
        /// Type de données utilisées par la méthode pour l'évaluation du déposit
        /// </summary>
        /// PM 20170313 [22833] Ajout
        public RiskMethodDataTypeEnum RiskMethodDataType
        {
            get { return m_RiskMethodDataType; }
        }

        /// <summary>
        /// Id interne de la tâche de lecture des RiskDatas
        /// </summary>
        /// PM 20180219 [23824] Ajout IdIOTaskRiskData
        public int IdIOTaskRiskData
        {
            get { return m_IdIOTaskRiskData; }
            set { m_IdIOTaskRiskData = value; }
        }

        /// <summary>
        /// Id interne de la tâche de lecture des RiskDatas
        /// </summary>
        /// PM 20180219 [23824] Ajout IdIOTaskRiskData
        public bool IsRiskDataOnHoliday
        {
            get { return m_IsRiskDataOnHoliday; }
            set { m_IsRiskDataOnHoliday = value; }
        }

        /// <summary>
        /// Information sur le process
        /// </summary>
        // PM 20180219 [23824]  Ajout
        public RiskPerformanceProcessInfo ProcessInfo
        {
            get { return m_ProcessInfo; }
            set { m_ProcessInfo = value; }
        }

        /// <summary>
        /// Ide de l'Entité
        /// </summary>
        // PM 20180219 [23824] Ajout 
        protected int IdEntity
        {
            get { return m_ProcessInfo.Entity; }
        }

        /// <summary>
        /// Risk Timing
        /// </summary>
        // PM 20180219 [23824] Ajout
        protected SettlSessIDEnum Timing
        {
            get { return m_ProcessInfo.Timing; }
        }

        /// <summary>
        /// Horaire des prix et paramètres de calcul à considérer pour un calcul Intra-Day
        /// </summary>
        // PM 20180219 [23824] Ajout
        protected TimeSpan RiskDataTime
        {
            get { return m_ProcessInfo.RiskDataTime; }
        }

        
        /// <summary>
        /// Valeur FIX indiquant un achat
        /// </summary>
        protected static string FixBuySide
        {
            get
            {
                return SideTools.RetBuyFIXmlSide();
            }
        }

        /// <summary>
        /// Valeur FIX indiquant une vente
        /// </summary>
        protected static string FixSellSide
        {
            get
            {
                return SideTools.RetSellFIXmlSide();
            }
        }
        #endregion Accessors

        /// <summary>
        /// Gestion Asynchrone du déposit
        /// </summary>
        // EG 20180307 [23769] New
        public bool IsCalculationAsync { get; set; }

        /// <summary>
        /// Semaphore for Deposit Calculation
        /// </summary>
        // EG 20180205 [23769] New
        public SemaphoreSlim SemaphoreDeposit { get; set; }
        /// <summary>
        /// method type
        /// </summary>
        public abstract InitialMarginMethodEnum Type { get; }

        /// <summary>
        /// internal id of the clearing house using the current method
        /// </summary>
        public int IdCSS { get; set; }

      
        /// <summary>
        /// Business date
        /// </summary>
        public DateTime DtBusiness { get; set; }

        /// <summary>
        /// Last Market date
        /// </summary>
        /// PM 20150423 [20575] Add DtMarket
        public DateTime DtMarket { get; set; }

        /// <summary>
        /// Date des paramètres de risque utilisés
        /// </summary>
        /// PM 20150507 [20575] Add DtRiskParameters
        public DateTime DtRiskParameters { get; set; }

        /// <summary>
        /// Stocks equity for short (call and futures) position coverage
        /// <para>Liste des potitions actions utilisables compte tenu des postions ETD en postion</para>
        /// </summary>
        List<StocksCoverageParameter> m_EquityStockCoverages;

        /// <summary>
        /// Parametres des paniers d'equity en vue de leur utilisation pour reduire une position ETD
        /// </summary>
        // PM 20201028 [25570][25542] Ajout m_BasketCoverageSetting
        List<BasketCoverageSetting> m_BasketCoverageSetting;

        /// <summary>
        /// position actions 
        /// </summary>
        List<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> m_PositionActions;

        /// <summary>
        /// Parameters to compute the delivery deposit
        /// </summary>
        // PM 20160404 [22116] m_ParametersDeliveries déplacé dans RiskMethodSet
        //List<AssetDeliveryParameter> m_ParametersDeliveries;

        /// <summary>
        /// Additional market parameters (the key is the market internal id)
        /// </summary>
        Dictionary<int, MarketParameter> m_MarketParameters;

        /// <summary>
        /// Get the additional market parameters  (the key is the market internal id)
        /// </summary>
        protected Dictionary<int, MarketParameter> MarketParameters
        {
            get
            {
                return m_MarketParameters;
            }
        }

        /// <summary>
        /// Additional marginreqoffice actor parameters (the key is the actor internal id)
        /// </summary>
        private Dictionary<int, IEnumerable<MarginReqOfficeParameter>> m_MarginReqOfficeParameters;

        /// <summary>
        /// Get the additional marginreqoffice actor parameters (the key is the actor internal id)
        /// </summary>
        // PM 20141216 [9700] Eurex Prisma for Eurosys Futures : ajout accesseur "set", et passage de protected à public
        public Dictionary<int, IEnumerable<MarginReqOfficeParameter>> MarginReqOfficeParameters
        {
            get
            {
                return m_MarginReqOfficeParameters;
            }
            set
            {
                m_MarginReqOfficeParameters = value;
            }
        }

        /// <summary>
        /// All the deposits to be generated and computed
        /// </summary>
        /// <remarks>the identifying key is the actor/book pair (the actor must own the book)</remarks>
        private SerializableDictionary<Pair<int, int>, Deposit> m_Deposits =
            new SerializableDictionary<Pair<int, int>, Deposit>("ActorId_BookId", "Deposit", new PairComparer<int, int>());

        /// <summary>
        /// Deposits dictionary, research key by actor/book
        /// </summary>
        // PM 20160404 [22116] Ajout accesseur "set"
        public SerializableDictionary<Pair<int, int>, Deposit> Deposits
        {
            get
            {
                return m_Deposits;
            }
            set { m_Deposits = value; }
        }

        /// <summary>
        /// class containing the diagnostic elements (in order to set their start time/en time/trade deposit properties)
        /// </summary>
        public IMRequestDiagnostics ImRequestDiagnostics
        { get; set; }

        /// <summary>
        /// Requête utilisée pour connaître l'existance de paramètres de risque pour une date donnée
        /// <remarks>Devrait utiliser le paramètre DTBUSINESS</remarks>
        /// </summary>
        /// PM 20150507 [20575] Add QueryExistRiskParameter
        protected virtual string QueryExistRiskParameter
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Get complementary information from the markets/clearing house which is using the current method
        /// </summary>
        /// <param name="pEntityMarkets">the entity/market collection concerned by the current clearing house</param>
        public virtual void BuildMarketParameters(IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        {
            m_MarketParameters = (
                from entitymarket in pEntityMarkets
                where entitymarket.CssId == this.IdCSS
                select new MarketParameter
                {
                    MarketId = entitymarket.MarketId,
                    // the default crossmargin for positions on securities (equity, etc..) is calculated basing on the flags
                    //  inside of the tables market and entitymarket 
                    CrossMarginActivated = entitymarket.CrossMarginActivated || entitymarket.MarketCrossMarginActivated,
                    StockCoverageType =
                        entitymarket.MarketStockCoverage != null && System.Enum.IsDefined(typeof(PosStockCoverEnum), entitymarket.MarketStockCoverage) ?
                        (PosStockCoverEnum)System.Enum.Parse(typeof(PosStockCoverEnum), entitymarket.MarketStockCoverage)
                        :
                        default,
                    // PM 20130328 Ajout BusinessCenter pour l'AGREX
                    BusinessCenter = entitymarket.MarketBusinessCenter,
                })
                .ToDictionary(key => key.MarketId);

            // Recherche des informations sur la chambre courante
            EntityMarketWithCSS currentEMWithCSS = pEntityMarkets.First(elem => elem.CssId == this.IdCSS);

            // La devise peut être null car ce n'est pas un paramètre obligatoire
            m_CssCurrency = currentEMWithCSS.CssCurrency;
            if (m_CssCurrency == default)
            {
                // Prendre alors Euro par défaut
                m_CssCurrency = "EUR";
            }
        }

        /// <summary>
        /// Initialize the method, loading common and specific parameters needed by the calcul. 
        /// Common parameters include: coverage stocks, positions on actions (IDEM file D13), delivery parameters.
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pAssetCache">complete set of assets</param>
        //  PM 20200910 [25481] Changement paramètre Dictionary<int, SQL_AssetETD> en RiskAssetCache
        //public void LoadParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        public void LoadParameters(string pCS, RiskAssetCache pAssetCache)
        {
            //// PM 20170313 [22833] pAssetETDCache peut être null
            ////if (pAssetETDCache.Count > 0))
            //if ((pAssetETDCache != default(Dictionary<int, SQL_AssetETD>)) && (pAssetETDCache.Count > 0))
            //{
            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                Dictionary<string, object> dbParametersValue = new Dictionary<string, object>
                {
                    { "DTBUSINESS", this.DtBusiness.Date }
                };

                //int[] contractsId = (from value in pAssetETDCache.Values select value.IdDerivativeContract).Distinct().ToArray();

                LoadPositionActions(connection, dbParametersValue, pCS);

                // PM 20160404 [22116] Déplacé dans RiskMethodSet
                //LoadParametersDeliveries(pAssetETDCache, connection, dbParametersValue, pCS);

                // PM 20130326 Ajout parameter SESSIONID pour limiter la requête des positions actions en couverture sur les DC en position
                //LoadStockCoverages(pAssetETDCache, connection, dbParametersValue, contractsId);
                LoadStockCoverages(connection);
            }
            //}
            //else
            //{
            //    m_EquityStockCoverages = new List<StocksCoverageParameter>();

            //    m_PositionActions = new List<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>>();

            //    // PM 20160404 [22116] Déplacé dans RiskMethodSet
            //    //m_ParametersDeliveries = new List<AssetDeliveryParameter>();
            //}

            // PM 20150507 [20575] Par défaut la date des paramètres de risque est la date business de traitement
            this.DtRiskParameters = this.DtBusiness;

            //LoadSpecificParameters(pCS, pAssetETDCache);
            if (pAssetCache != default(RiskAssetCache))
            {
                LoadSpecificParameters(pCS, pAssetCache.AssetETDCache);
            }
        }

        /// <summary>
        /// Reset parameters collections 
        /// </summary>
        public void ResetParameters()
        {
            m_MarketParameters = null;

            m_MarginReqOfficeParameters = null;

            m_EquityStockCoverages = null;
            // PM 20201028 [25570][25542] Ajout m_BasketCoverageSetting
            m_BasketCoverageSetting = null;

            m_PositionActions = null;

            // PM 20160404 [22116] Déplacé dans RiskMethodSet
            //m_ParametersDeliveries = null;

            ResetSpecificParameters();
        }

        /// <summary>
        /// Get the position actions for a specific actor/book
        /// </summary>
        /// <param name="pActorId">actor owning the position action</param>
        /// <param name="pBookId">book owned by the actor where the position action is registered</param>
        /// <returns>a collection of position action, empty when no positions actions are found</returns>
        public IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> GetPositionActions(int pActorId, int pBookId)
        {
            return from position in m_PositionActions
                   where position.First.idA_Dealer == pActorId && position.First.idB_Dealer == pBookId
                   select position;
        }

        /// <summary>
        /// Initialize the method
        /// </summary>
        protected abstract void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache);

        /// <summary>
        /// Evaluate one deposit factor
        /// </summary>
        /// <param name="opMethodComObj">output value containing all the calculation datas 
        /// to pass to the calculation sheet repository object
        /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) in order to
        /// build a margin calculation node (<see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
        /// and <see cref="EfsML.Interface.IMarginCalculationMethod"/>)</param>
        /// <param name="pActorId">the actor owning the positions set</param>
        /// <param name="pBookId">the book where the positions set has been registered</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="pPositionsToEvaluate">the positions set</param>
        /// <returns>the partial amount for the current deposit</returns>
        /// PM 20160404 [22116] Devient public
        /// FI 20160613 [22256] Add parameter pDepositHierarchyClass
        /// PM 20170313 [22833] Changement de type pour le paramètre pPositionsToEvaluate => pRiskDataToEvaluate
        //public abstract List<Money> EvaluateRiskElementSpecific(
        //    int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
        //    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
        //    out IMarginCalculationMethodCommunicationObject opMethodComObj);
        public abstract List<Money> EvaluateRiskElementSpecific(
            int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
            RiskData pRiskDataToEvaluate,
            out IMarginCalculationMethodCommunicationObject opMethodComObj);

        /// <summary>
        /// Reset parameters collections for a specific method
        /// </summary>
        protected abstract void ResetSpecificParameters();

        /// <summary>
        /// Collection of sorting parameter needed by coverage strategies
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">positions, netted by asset, for the current risk element</param>
        /// <returns>a collection of sorting parameter in order to be used inside of the ReducePosition method </returns>
        protected abstract IEnumerable<CoverageSortParameters> GetSortParametersForCoverage(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset);

        /// <summary>
        /// Merge two input money collections (multiple currencies) into the second one, making the sum of the amounts by currency
        /// </summary>
        /// <param name="pSourceAmounts">source money collection</param>
        /// <param name="pDestAmounts">dest money collection, can be null if empty</param>
        public static void SumAmounts(IEnumerable<Money> pSourceAmounts, ref List<Money> pDestAmounts)
        {
            if (pDestAmounts == null)
            {
                pDestAmounts = new List<Money>();
            }

            // PM 20130528 Ajout vérification de l'argument pSourceAmounts
            if (pSourceAmounts != null)
            {
                IEnumerable<Money> sourceAmounts = pSourceAmounts.Where(money => money != default);
                if (sourceAmounts.Count() != 0)
                {
                    pDestAmounts = (
                        from amountsByCurrency
                            in (sourceAmounts.Union(pDestAmounts).GroupBy(money => money.Currency))
                        select
                            new Money(
                                (from amount in amountsByCurrency select amount.Amount.DecValue).Sum(),
                                amountsByCurrency.Key
                            )
                       ).ToList();
                }
            }
        }

        /// <summary>
        /// Sum the input money list by currency and multiply the totals per the given offset
        /// </summary>
        /// <param name="pSourceAmounts">source money collection</param>
        /// <param name="pOffset">multiplier factor</param>
        /// <returns>the grouped by currency money collection, applying the offset factor </returns>
        public IEnumerable<Money> SumAmounts(IEnumerable<Money> pSourceAmounts, decimal? pOffset)
        {
            return SumAmounts(pSourceAmounts, pOffset, false);
        }

        /// <summary>
        /// Sum the input money list by currency and multiply the totals per the given offset  
        /// </summary>
        /// <param name="pSourceAmounts">source money collection, MAY NOT be null</param>
        /// <param name="pOffset">multiplier factor</param>
        /// <param name="pCheckNegativeAmount">when true the offset is performed just when the amount is negative</param>
        /// <returns>the grouped by currency money collection </returns>
        public IEnumerable<Money> SumAmounts(IEnumerable<Money> pSourceAmounts, decimal? pOffset, bool pCheckNegativeAmount)
        {
            if (pSourceAmounts == null)
            {
                throw new ArgumentNullException("pSourceAmounts", "Error, the amounts source collection may not be null.");
            }

            List<Money> groupedAmounts = null;

            SumAmounts(pSourceAmounts, ref groupedAmounts);

            if (pOffset.HasValue)
            {
                foreach (Money amount in groupedAmounts)
                {
                    bool performOffset = (!pCheckNegativeAmount) || (pCheckNegativeAmount && amount.amount.DecValue < 0);

                    if (performOffset)
                    {
                        amount.amount.DecValue = AdjustAmount(amount.amount.DecValue, pOffset.Value, false);
                    }
                }
            }

            return groupedAmounts;
        }

        /// <summary>
        /// Adjust the input amount using the input offset
        /// </summary>
        /// <param name="pAmount">amount to be adjusted</param>
        /// <param name="pOffset">adjusting factor</param>
        /// <param name="pRoundOffsetOnly">when true, only the adjusting factor (pOffset) is rounded</param>
        /// <returns>the adjusted amount. res:= pAmount * pOffset</returns>
        public decimal AdjustAmount(decimal pAmount, decimal pOffset, bool pRoundOffsetOnly)
        {
            decimal res;
            if (pRoundOffsetOnly)
            {
                res = pAmount * RoundAmount(pOffset);
            }
            else
            {
                res = RoundAmount(pAmount * pOffset);
            }

            return res;
        }

        /// <summary>
        /// Round an amount using the default rule (2 decimals and rounding rule "banker's rounding")
        /// </summary>
        /// <remarks>
        /// It's based on IEEE Standard 754, section 4
        /// http://progblog10.blogspot.com/2011/01/bankers-rounding-in-c-and-sql.html
        /// </remarks>
        /// <param name="pAmount">mount to be rounded</param>
        /// <returns>the rounded amount, when the last digit is a 5, it rounds up if the preceding digit is odd, and down if even.</returns>
        protected virtual decimal RoundAmount(decimal pAmount)
        {
            return RoundAmount(pAmount, 2);
        }

        /// <summary>
        /// Round an amount using the default rule (rounding rule "banker's rounding") at a custom precision
        /// </summary>
        /// <param name="pAmount">mount to be rounded</param>
        /// <param name="pPrecision">rounding precision</param>
        /// <returns>the rounded amount, when the last digit is a 5, it rounds up if the preceding digit is odd, and down if even</returns>
        protected virtual decimal RoundAmount(decimal pAmount, int pPrecision)
        {
            return System.Math.Round(pAmount, pPrecision, MidpointRounding.ToEven);
        }

        // UNDONE MF 20111110 l'activation du cross margin peut être différent par marché, mettre à disposition une logique pour...
        /// <summary>
        /// Get the cross margin activation status
        /// </summary>
        /// <returns>the activation status of the cross margin for the current markets</returns>
        protected bool IsCrossMarginActivated()
        {
            return (from elem in this.MarketParameters.Values select elem.CrossMarginActivated).Aggregate((curr, next) => curr || next);
        }

        /// <summary>
        /// Reduce the current positions set (pGroupedPositionsByIdAsset) using the loaded stock quantities
        /// </summary>
        /// <param name="pActorId">the actor owning the positions set</param>
        /// <param name="pBookId">actor'sbook where the positions are registered in</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="pSortParameters">sorting parameters relative to the current assets collection</param>
        /// <param name="pGroupedPositionsByIdAsset">positions set, netted by asset</param>
        /// <returns>a collection of covered quantities, the positions will be emended consequently</returns>
        /// <remarks>Actually just stocks type Equity will be loaded and used to reduce positions</remarks>
        /// FI 20160613 [22256] Modify
        /// FI 20160613 [22256] Add parameter pDepositHierarchyClass
        protected Pair<IEnumerable<StockCoverageCommunicationObject>, IEnumerable<StockCoverageDetailCommunicationObject>>
            ReducePosition(int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
            IEnumerable<CoverageSortParameters> pSortParameters,
            ref IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            // Filtrer les positions Equity utilisable (en fonction de l'acteur/book en cours de traitement)
            // PM 20201028 [25570][25542] Ajout condition afin de ne pas prendre les Equities utilisés pour la constitution de panier (ne pendre que les Equity ayant la même class que celle du sous-jacent du DC)
            var filteredEquityStockCoverages =
                from stockParameter in m_EquityStockCoverages
                where ((pDepositHierarchyClass == DepositHierarchyClass.ENTITY && stockParameter.PayActorId == pActorId && stockParameter.PayBookId == pBookId)
                      ||
                       (pDepositHierarchyClass == DepositHierarchyClass.CLEARER && stockParameter.RecActorId == pActorId && stockParameter.RecBookId == pBookId))
                       && (stockParameter.EquityAssetClass == stockParameter.UnlAssetClass)
                select stockParameter;

            #region Détail de la couverture potentiellement utilisable
            // FI 20160613 [22256] add stockCoverageDetCommunicationObject
            // stockCoverageDetCommunicationObject : Liste des positions Stokcs déposées pour le couple pActorId, pBookId et potentiellement utilisable compte tenu des assets ETD en position
            List<StockCoverageDetailCommunicationObject> stockCoverageDetCommunicationObject =
                new List<StockCoverageDetailCommunicationObject>();

            IEnumerable<StockCoverageDetailCommunicationObject>
                filteredEquityStock = from item in filteredEquityStockCoverages
                                      group item by new
                                      {
                                          PosId = item.IdPosEquity,
                                          PosIdentifier = item.PosEquityIdentifier,
                                          ActorId = (pDepositHierarchyClass == DepositHierarchyClass.ENTITY) ? item.PayActorId : item.RecActorId,
                                          ActorIdentifier = (pDepositHierarchyClass == DepositHierarchyClass.ENTITY) ? item.PayActorIdentifier : item.RecActorIdentifier,
                                          BookId = (pDepositHierarchyClass == DepositHierarchyClass.ENTITY) ? item.PayBookId : item.RecBookId,
                                          BookIdentifier = (pDepositHierarchyClass == DepositHierarchyClass.ENTITY) ? item.PayBookIdentifier : item.RecBookIdentifier,
                                          AssetId = item.UnlAssetId,
                                          AssetIdentifier = item.UnlAssetIdentifier,
                                          QtyAvailable = item.Quantity,
                                      } into groupbyResult
                                      select new StockCoverageDetailCommunicationObject
                                      {
                                          PosId = groupbyResult.Key.PosId,
                                          PosIdentifier = groupbyResult.Key.PosIdentifier,
                                          ActorId = groupbyResult.Key.ActorId,
                                          ActorIdentifier = groupbyResult.Key.ActorIdentifier,
                                          BookId = groupbyResult.Key.BookId,
                                          BookIdentifier = groupbyResult.Key.BookIdentifier,
                                          AssetId = groupbyResult.Key.AssetId,
                                          AssetIdentifier = groupbyResult.Key.AssetIdentifier,
                                          QtyAvailable = groupbyResult.Key.QtyAvailable,
                                          QtyUsedFut = 0,
                                          QtyUsedOpt = 0
                                      };
            foreach (StockCoverageDetailCommunicationObject item in filteredEquityStock)
            {
                stockCoverageDetCommunicationObject.Add(item);
            }
            #endregion Détail de la couverture potentiellement utilisable

            // 1. Evaluate the positions collection, giving it writing permissions (and allow the quantity reduction)
            pGroupedPositionsByIdAsset = pGroupedPositionsByIdAsset.ToArray();

            // 2. Get all the quantities, in position (allocations) or in delivery (settlements), which may be covered 
            //  (short call, short futures and assignations for the current actor)
            var elementsCouldBeCovered =
                // 2.1 allocations
                (from position in pGroupedPositionsByIdAsset
                 where (position.First.FixSide == SideEnum.Sell) && (position.Second.Quantity > 0)
                 join inputCoverage in pSortParameters on position.First.idAsset equals inputCoverage.AssetId
                 where
                     inputCoverage.Type == RiskMethodQtyType.Call
                     || inputCoverage.Type == RiskMethodQtyType.Future
                 join stock in m_EquityStockCoverages on inputCoverage.ContractId equals stock.ContractId
                 where
                 stock.Type != PosStockCoverEnum.None
                 && (
                     (pDepositHierarchyClass == DepositHierarchyClass.ENTITY && stock.PayActorId == pActorId && stock.PayBookId == pBookId)
                     ||
                     (pDepositHierarchyClass == DepositHierarchyClass.CLEARER && stock.RecActorId == pActorId && stock.RecBookId == pBookId)
                 )
                 select new
                 {
                     AssetId = position.First.idAsset,
                     inputCoverage.ContractId,
                     inputCoverage.Type,
                     inputCoverage.Quote,
                     position.Second.Quantity,
                     ExeAss = false,
                     inputCoverage.MaturityYearMonth,
                     inputCoverage.StrikePrice,
                     Position = position,
                     inputCoverage.Multiplier,
                 })
                // 2.2 settlements 
                .Union(from position in pGroupedPositionsByIdAsset
                       where (position.First.FixSide == SideEnum.Sell) && (position.Second.ExeAssQuantity > 0)
                       join inputCoverage in pSortParameters on position.First.idAsset equals inputCoverage.AssetId
                       where
                           // change this condition having care of changing the  building of the anonymous element 
                           inputCoverage.Type == RiskMethodQtyType.ExeAssCall
                           || inputCoverage.Type == RiskMethodQtyType.FutureMoff
                       join stock in m_EquityStockCoverages on inputCoverage.ContractId equals stock.ContractId
                       where
                       stock.Type != PosStockCoverEnum.None
                       && (
                            (pDepositHierarchyClass == DepositHierarchyClass.ENTITY && stock.PayActorId == pActorId && stock.PayBookId == pBookId)
                            ||
                            (pDepositHierarchyClass == DepositHierarchyClass.CLEARER && stock.RecActorId == pActorId && stock.RecBookId == pBookId)
                       )
                       select new
                       {
                           AssetId = position.First.idAsset,
                           inputCoverage.ContractId,
                           // type replacement, to make possible the sorting by type (depending by the coverage strategy)
                           Type = inputCoverage.Type == RiskMethodQtyType.ExeAssCall ? RiskMethodQtyType.Call : RiskMethodQtyType.Future,
                           inputCoverage.Quote,
                           // the ExeAssQuantity value is positive in any case, because we choose only short "2" positions
                           Quantity = position.Second.ExeAssQuantity,
                           ExeAss = true,
                           inputCoverage.MaturityYearMonth,
                           inputCoverage.StrikePrice,
                           Position = position,
                           inputCoverage.Multiplier,
                       }
                ).ToArray();

            // 3. init the list hosting the covered quantities to be passed to the calculation sheet builder
            List<StockCoverageCommunicationObject> stockCoverageCommunicationObject =
                new List<StockCoverageCommunicationObject>();

            if (elementsCouldBeCovered.Count() > 0)
            {

                // PM 20130920 [18976] Somme des quantités des positions Equity de même caractèristiques
                var summedEquityStockCoverages =
                    from filteredStock in filteredEquityStockCoverages
                    group filteredStock by new
                    {
                        filteredStock.PayActorId,
                        filteredStock.PayBookId,
                        filteredStock.RecActorId,
                        filteredStock.RecBookId,
                        filteredStock.ContractId,
                        filteredStock.GroupByContractId,
                        filteredStock.MarketId,
                        filteredStock.Type,
                        filteredStock.UnlAssetId,
                    } into cumulatedStock
                    select new
                    {
                        //FI 20160531 [21885] add IdsPosEquity
                        IdsPosEquity = ((from item in filteredEquityStockCoverages
                                         where item.PayActorId == cumulatedStock.Key.PayActorId &&
                                               item.PayBookId == cumulatedStock.Key.PayBookId &&
                                               item.ContractId == cumulatedStock.Key.ContractId &&
                                               item.GroupByContractId == cumulatedStock.Key.GroupByContractId &&
                                               item.MarketId == cumulatedStock.Key.MarketId &&
                                               item.RecActorId == cumulatedStock.Key.RecActorId &&
                                               item.RecBookId == cumulatedStock.Key.RecBookId &&
                                               item.Type == cumulatedStock.Key.Type &&
                                               item.UnlAssetId == cumulatedStock.Key.UnlAssetId
                                         select item.IdPosEquity
                                    ).Distinct()).ToArray(),
                        cumulatedStock.Key.PayActorId,
                        cumulatedStock.Key.PayBookId,
                        cumulatedStock.Key.ContractId,
                        cumulatedStock.Key.GroupByContractId,
                        cumulatedStock.Key.MarketId,
                        cumulatedStock.Key.RecActorId,
                        cumulatedStock.Key.RecBookId,
                        cumulatedStock.Key.Type,
                        cumulatedStock.Key.UnlAssetId,
                        Quantity = cumulatedStock.Sum(s => s.Quantity)
                    };

                // PM 20130920 [18976] Regrouper les positions Equity utilisable (par sous-jacent, éventuellement contrat d'utilisation, et type de couverture)
                var groupedEquityStockCoverages = summedEquityStockCoverages.GroupBy(elem => new { UnderlyerAssetId = elem.UnlAssetId, FilterContractId = elem.GroupByContractId, CoverageType = elem.Type });

                // PM 20130920 [18976] Ordonner les groupes de positions Equity utilisable par type de couverture
                // et construction de la liste des contrats pour lesquels ils peuvent être utilisés
                var validStockParameters = (
                    from groupedStock in groupedEquityStockCoverages
                    select new
                    {
                        //FI 20160531 [21885] add IdsPosEquity
                        IdsPosEquity = (from item in groupedStock select item.IdsPosEquity).FirstOrDefault(),
                        // list of the contracts of the group
                        // PM 20130325 Ajout du "Distinct" sur les ContractId
                        Contracts = (from stock in groupedStock select stock.ContractId).Distinct(),
                        // the quantity shared among the contracts of the group
                        Quantity = (from stock in groupedStock select stock.Quantity).FirstOrDefault(),
                        Type = groupedStock.Key.CoverageType,
                    }).OrderByDescending(s => s.Type.ToString());

                // 5. for each grouped stock parameter (each group contains the max quantity to be covered for one or more contracts)....
                foreach (var validStockParameter in validStockParameters)
                {

                    PosStockCoverEnum coverageType = validStockParameter.Type;

                    // 6. Find the positions  may be covered by the current parameter
                    // PM 20130325 Ne prendre que les elements qui sont encore en position
                    var elementsContractCouldBeCovered =
                        from elem in elementsCouldBeCovered
                        join contract in validStockParameter.Contracts on elem.ContractId equals contract
                        // PM 20130923 [18992] Bug dans test de quantity
                        //where (elem.ExeAss ? elem.Position.Second.Quantity : elem.Position.Second.Quantity) != 0
                        where (elem.ExeAss ? elem.Position.Second.ExeAssQuantity : elem.Position.Second.Quantity) != 0
                        select elem;

                    if (elementsContractCouldBeCovered.Count() == 0)
                    {
                        continue;
                    }

                    // PM 20130924 [18976] Tri des élements à couvrir différent entre les Futures et les Options pour les règles autres que Default
                    // Pour les Futures, l'ordre de prise en compte est Quantité (de la plus grande à la plus petite), puis Echeance (de la plus lointaine à la plus proche)
                    var elementsContractFutureCouldBeCovered =
                        (from elem in elementsContractCouldBeCovered where elem.Type == RiskMethodQtyType.Future select elem)
                        .OrderByDescending(orderby => orderby.MaturityYearMonth)
                        .OrderByDescending(orderby => orderby.Quantity);
                    // Pour les Options, l'ordre de prise en compte doit être identique à la règle Default
                    // C'est à dire Cours (du plus grand au plus petit), puis Echéance (de la plus lointaine à la plus proche), puis Strike (du plus petit au plus grand)
                    var elementsContractOptionCouldBeCovered =
                        (from elem in elementsContractCouldBeCovered where elem.Type == RiskMethodQtyType.Call select elem)
                        .OrderBy(orderby => orderby.StrikePrice)
                        .OrderByDescending(orderby => orderby.MaturityYearMonth)
                        .OrderByDescending(orderby => orderby.Quote);

                    // 6.1 apply the choosen covering strategy
                    switch (coverageType)
                    {
                        case PosStockCoverEnum.Default:

                            elementsContractCouldBeCovered =
                                elementsContractCouldBeCovered
                                .OrderBy(orderby => orderby.StrikePrice)
                                .OrderByDescending(orderby => orderby.MaturityYearMonth)
                                .OrderByDescending(orderby => orderby.Quote);

                            break;

                        case PosStockCoverEnum.PriorityStockFuture:

                            // PM 20130924 [18976] Pour les options, l'ordre de prise en compte doit être identique à règle Default
                            // C'est à dire Cours, puis Echéance, puis Strike
                            //elementsContractCouldBeCovered =
                            //    elementsContractCouldBeCovered
                            //    .OrderByDescending(orderby => orderby.Quantity)
                            //    .OrderBy(orderby => orderby.StrikePrice)
                            //    .OrderByDescending(orderby => orderby.MaturityYearMonth)
                            //    .OrderByDescending(orderby => orderby.Type);

                            elementsContractCouldBeCovered = elementsContractFutureCouldBeCovered.Concat(elementsContractOptionCouldBeCovered);

                            break;

                        case PosStockCoverEnum.PriorityStockOption:

                            // PM 20130924 [18976] Pour les options, l'ordre de prise en compte doit être identique à règle Default
                            // C'est à dire Cours, puis Echéance, puis Strike
                            //elementsContractCouldBeCovered =
                            //    elementsContractCouldBeCovered
                            //    .OrderByDescending(orderby => orderby.Quantity)
                            //    .OrderBy(orderby => orderby.StrikePrice)
                            //    .OrderByDescending(orderby => orderby.MaturityYearMonth)
                            //    .OrderBy(orderby => orderby.Type);

                            elementsContractCouldBeCovered = elementsContractOptionCouldBeCovered.Concat(elementsContractFutureCouldBeCovered);

                            break;

                        case PosStockCoverEnum.StockFuture:

                            // PM 20130924 [18976]
                            //elementsContractCouldBeCovered =
                            //    (from elem in elementsContractCouldBeCovered where elem.Type == RiskMethodQtyType.Future select elem)
                            //    .OrderByDescending(orderby => orderby.Quantity)
                            //    .OrderBy(orderby => orderby.StrikePrice)
                            //    .OrderByDescending(orderby => orderby.MaturityYearMonth);
                            elementsContractCouldBeCovered = elementsContractFutureCouldBeCovered;

                            break;

                        case PosStockCoverEnum.StockOption:

                            // PM 20130924 [18976] Ordre de prise en compte doit être identique à règle Default
                            // C'est à dire Cours, puis Echéance, puis Strike
                            //elementsContractCouldBeCovered =
                            //    (from elem in elementsContractCouldBeCovered where elem.Type == RiskMethodQtyType.Call select elem)
                            //    .OrderByDescending(orderby => orderby.Quantity)
                            //    .OrderBy(orderby => orderby.StrikePrice)
                            //    .OrderByDescending(orderby => orderby.MaturityYearMonth);
                            elementsContractCouldBeCovered = elementsContractOptionCouldBeCovered;

                            break;
                    }

                    // RD 20130722 [18842] Bug en cas ou la liste elementsContractCouldBeCovered est vide
                    // Ajout du test ci-dessous
                    if (elementsContractCouldBeCovered.Count() == 0)
                    {
                        continue;
                    }

                    // RD 20210414 [25706] Utiliser le multiplier de chaque actif à couvrir.
                    //decimal multiplier = elementsContractCouldBeCovered.First().Multiplier;
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    // RD 20170517 [23159] Utiliser System.Math.Floor à  la place de System.Math.Round
                    // Il s'agit de trouver le nombre de lots du DC possible de couvrir avec la quantité d'actions
                    // Il faut donc prendre le plus grand entier inférieur au résultat de la division
                    //decimal currentStockQuantity = System.Math.Round(validStockParameter.Quantity / multiplier, 0, MidpointRounding.AwayFromZero);
                    // RD 20210414 [25706] Recalculer la quantité disponible en utilisant le multiplier de chaque actif à couvrir.
                    //decimal currentStockQuantity = System.Math.Floor(validStockParameter.Quantity / multiplier);
                    decimal availableStockQuantity = validStockParameter.Quantity;

                    int idxCoveredOrder = 1;

                    // 7. Finally reduce the quantity on the sorted collection
                    foreach (var elem in elementsContractCouldBeCovered)
                    {
                        // RD 20210414 [25706] Utiliser le multiplier de chaque actif à couvrir pour calculer la quantité d'actions disponible
                        decimal multiplier = elem.Multiplier;
                        decimal currentStockQuantity = System.Math.Floor(availableStockQuantity / multiplier);

                        // PM 20130325 Recalculer la quantity de l'élément à partir de sa quantité réellement en position
                        // PM 20130923 [18992] Bug dans affectation de quantity
                        // EG 20150920 [21374] Int (int32) to Long (Int64) 
                        // EG 20170127 Qty Long To Decimal
                        decimal elemQty = elem.ExeAss ? elem.Position.Second.ExeAssQuantity : elem.Position.Second.Quantity;
                        decimal coveredQuantity = System.Math.Min(elemQty, currentStockQuantity);

                        if (coveredQuantity == 0)
                        {
                            break;
                        }
                        // FI 20160613 [22256] Alimentation de UsedQtyOptions et UsedQtyFutures
                        // EG 20170127 Qty Long To Decimal
                        decimal stockQty = coveredQuantity * multiplier;
                        foreach (StockCoverageDetailCommunicationObject item in (from item in stockCoverageDetCommunicationObject
                                                                                 join idPosEquity in validStockParameter.IdsPosEquity on item.PosId equals idPosEquity
                                                                                 orderby idPosEquity
                                                                                 select item
                                                                                 ))
                        {
                            if (stockQty == 0)
                                break;
                            if ((item.QtyAvailable - item.QtyUsedFut - item.QtyUsedOpt) > 0)
                            {
                                if ((elem.Type == RiskMethodQtyType.Call) || (elem.Type == RiskMethodQtyType.ExeAssCall))
                                {
                                    // EG 20170127 Qty Long To Decimal
                                    decimal QtyOpt = System.Math.Min(item.QtyAvailable - item.QtyUsedFut - item.QtyUsedOpt, stockQty);
                                    item.QtyUsedOpt += QtyOpt;
                                    stockQty -= QtyOpt;

                                }
                                else if ((elem.Type == RiskMethodQtyType.Future) || (elem.Type == RiskMethodQtyType.FutureMoff))
                                {
                                    // EG 20170127 Qty Long To Decimal
                                    decimal QtyFut = System.Math.Min(item.QtyAvailable - item.QtyUsedFut - item.QtyUsedOpt, stockQty);
                                    item.QtyUsedFut += QtyFut;
                                    stockQty -= QtyFut;
                                }
                                else
                                {
                                    throw new NotImplementedException(StrFunc.AppendFormat("RiskMethodQtyType:{0} is not implemented", elem.Type.ToString()));
                                }
                            }
                        }

                        StockCoverageCommunicationObject stockComObj =
                            new StockCoverageCommunicationObject
                            {
                                ContractId = elem.ContractId,
                                AssetId = elem.AssetId,
                                Order = idxCoveredOrder++,
                                Quantity = coveredQuantity,
                            };

                        elem.Position.Second =
                            new RiskMarginPosition
                            {
                                TradeIds = elem.Position.Second.TradeIds,
                                Quantity = elem.ExeAss ?
                                    elem.Position.Second.Quantity : elem.Position.Second.Quantity - coveredQuantity,
                                ExeAssQuantity = elem.ExeAss ?
                                    elem.Position.Second.ExeAssQuantity - coveredQuantity : elem.Position.Second.ExeAssQuantity,
                            };

                        // RD 20210414 [25706] Recalculer la quantité disponible en utilisant le multiplier de chaque actif à couvrir.
                        //currentStockQuantity -= coveredQuantity;
                        availableStockQuantity -= coveredQuantity * multiplier;

                        stockCoverageCommunicationObject.Add(stockComObj);
                    }
                }
            }
            return new Pair<IEnumerable<StockCoverageCommunicationObject>, IEnumerable<StockCoverageDetailCommunicationObject>>
                (stockCoverageCommunicationObject, stockCoverageDetCommunicationObject);
        }

        /// <summary>
        /// Initialize the communication object where we put the positions with no parameters attached
        /// </summary>
        /// <typeparam name="T">type of the communication object where we put the positions with no parameters attrached</typeparam>
        /// <param name="pGroupedPositionsByIdAsset">Positions set grouped by asset</param>
        /// <param name="assetWithParamsInPosition">Asset Id for which we assure that a related parameter exists</param>
        /// <param name="pCommunicationObjectForMissingParameters">Instance of the ommunication object to be initialized</param>
        /// <returns>true, when we found some positions with no parameters attached, otherwise false. When false you do not need to</returns>
        protected static bool InitializeCommunicationObjectForMissingParameters<T>
            (IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset,
            IEnumerable<int> assetWithParamsInPosition,
            T pCommunicationObjectForMissingParameters) where T : IMissingCommunicationObject, IRiskParameterCommunicationObject
        {

            bool bMissingParameter = false;

            // 4.1 get all the position keys
            IEnumerable<PosRiskMarginKey> positionKeys = from position in pGroupedPositionsByIdAsset select position.First;

            // 4.1 find the positions list for which no associated parameter has been found
            IEnumerable<PosRiskMarginKey> positionKeysWithoutProductClass =
                // 4.1.2 get all the position keys
                (from position in pGroupedPositionsByIdAsset select position.First)
                // 4.1.3 get all the positions 
                .Except(
                    from positionKey in positionKeys
                    join assetId in assetWithParamsInPosition on positionKey.idAsset equals assetId
                    select positionKey,
                    new PosRiskMarginKeyComparer()
                );

            if (positionKeysWithoutProductClass.Count() > 0)
            {
                bMissingParameter = true;

                // 4.3 build the fake product parameter where we stock the previous positions set

                pCommunicationObjectForMissingParameters.Missing = true;

                pCommunicationObjectForMissingParameters.Parameters = null;

                pCommunicationObjectForMissingParameters.Positions = from positionKey in positionKeysWithoutProductClass
                                                                     join position in pGroupedPositionsByIdAsset on positionKey.idAsset equals position.First.idAsset
                                                                     select position;

            }

            return bMissingParameter;
        }

        /// <summary>
        /// Get the quote for a specific asset
        /// </summary>
        /// <param name="pUnkProd">pass any instance of productbase extended class</param>
        /// <param name="pCS">connection string</param>
        /// <param name="pAssetId">id of the asset we want the quote</param>
        /// <param name="pAssetCategory">category of the asset we want the quote</param>
        /// <param name="pDateQuote">date of the official closed quote</param>
        /// <param name="opIdMarketEnv">get the market environment for the demanded quotee</param>
        /// <param name="opIdValScenario">get the scenario for the demanded quote</param>
        /// <param name="opAdjustedTime">get the datetime for the demanded quote</param>
        /// <param name="opQuoteSide">get the side for the demanded quote</param>
        /// <param name="opQuoteTiming">get the timing for the demanded quote</param>
        /// <param name="opSystemMsgInfo">get the quote system message information when quote can not be read</param>
        /// <returns></returns>
        /// PM 20160404 [22116] Devient public static
        public static decimal GetQuote(IProductBase pUnkProd, string pCS, int pAssetId, Cst.UnderlyingAsset pAssetCategory, DateTime pDateQuote,
            out string opIdMarketEnv, out string opIdValScenario, out DateTime opAdjustedTime, out string opQuoteSide, out string opQuoteTiming, out SystemMSGInfo opSystemMsgInfo)
        {

            decimal res = 0;

            SQL_Quote quote = new SQL_Quote(
                    pCS,
                    AssetTools.ConvertUnderlyingAssetToQuoteEnum(pAssetCategory),
                    AvailabilityEnum.NA,
                    pUnkProd,
                    new KeyQuote(pCS, pDateQuote, null, null, QuotationSideEnum.OfficialClose, QuoteTimingEnum.Close),
                    pAssetId);

            if ((false == quote.IsLoaded) || (quote.QuoteValueCodeReturn != Cst.ErrLevel.SUCCESS))
            {
                opSystemMsgInfo = quote.SystemMsgInfo;
            }
            else
            {
                opSystemMsgInfo = null;
                res = quote.QuoteValue;
            }

            opIdMarketEnv = quote.IdMarketEnv;
            opIdValScenario = quote.IdValScenario;
            opAdjustedTime = quote.AdjustedTime;
            opQuoteTiming = String.IsNullOrEmpty(quote.QuoteTiming) ? Cst.NotAvailable : quote.QuoteTiming;
            opQuoteSide = String.IsNullOrEmpty(quote.QuoteSide) ? Cst.NotAvailable : quote.QuoteSide;

            return res;
        }

        /// <summary>
        /// Lecture des cotations de tous les assets utilisés
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// <param name="pAssetQuoteParametersd">liste des assets en position</param>
        /// <param name="pUnderlyingAssetQuoteParameters">liste des assets sous-jacents des assets en position</param>
        /// <param name="pAssetETDCache"></param>
        /// EG 20140210 [19587] Lecture cours en Date réelle échéance si DtBusiness >= DTMaturityDateSys
        protected void GetQuotes(string pCS, List<AssetQuoteParameter> pAssetQuoteParametersd, List<AssetQuoteParameter> pUnderlyingAssetQuoteParameters,
            Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            ExchangeTradedDerivative product = new ExchangeTradedDerivative();
            DateTime dtQuote;

            if (pAssetQuoteParametersd != default)
            {
                // Lecture des cotations des assets en position
                foreach (AssetQuoteParameter asset in pAssetQuoteParametersd)
                {
                    if (asset.AssetId != 0)
                    {
                        // EG 20140210 [19587]
                        dtQuote = GetQuoteDate(asset, pAssetETDCache);
                        asset.Quote = GetQuote(product, pCS, asset.AssetId, asset.AssetCategoryEnum, dtQuote,
                           out asset.IdMarketEnv, out asset.IdValScenario, out asset.AdjustedTime, out asset.QuoteSide, out asset.QuoteTiming, out asset.SystemMsgInfo);
                    }
                }
            }
            if (pUnderlyingAssetQuoteParameters != default)
            {
                // Lecture des cotations des assets sous-jacents des assets en position
                foreach (AssetQuoteParameter asset in pUnderlyingAssetQuoteParameters)
                {
                    if (asset.AssetId != 0)
                    {
                        // EG 20140210 [19587] ici UNL est toujours un EquityAsset mais l'appel GetQuoteDate est maintenu
                        dtQuote = GetQuoteDate(asset, pAssetETDCache);
                        asset.Quote = GetQuote(product, pCS, asset.AssetId, asset.AssetCategoryEnum, dtQuote,
                           out asset.IdMarketEnv, out asset.IdValScenario, out asset.AdjustedTime, out asset.QuoteSide, out asset.QuoteTiming, out asset.SystemMsgInfo);
                    }
                }
            }
        }

        // EG 2040210 [19587]
        private DateTime GetQuoteDate(AssetQuoteParameter pAsset, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            // PM 20150512 [20575] Utilisation de DtRiskParameters est alimenté avec DtBusiness ou DtMarket si pas de cotation en date DtBusiness
            //DateTime dtQuote = this.DtBusiness;
            DateTime dtQuote = this.DtRiskParameters;

            if ((pAsset.AssetCategoryEnum == Cst.UnderlyingAsset.ExchangeTradedContract) ||
                (pAsset.AssetCategoryEnum == Cst.UnderlyingAsset.Future))
            {
                if (pAssetETDCache.ContainsKey(pAsset.AssetId))
                {
                    SQL_AssetETD sql_AssetETD = pAssetETDCache[pAsset.AssetId];
                    DateTime _maturityDate = sql_AssetETD.Maturity_MaturityDateSys;
                    if (DtFunc.IsDateTimeEmpty(_maturityDate))
                        _maturityDate = sql_AssetETD.Maturity_MaturityDate;
                    if (DtFunc.IsDateTimeFilled(_maturityDate) && (dtQuote >= _maturityDate))
                        dtQuote = _maturityDate;
                }
            }
            return dtQuote;
        }

        /// <summary>
        /// Get the ITM/OTm amount for an option asset
        /// </summary>
        /// <param name="pType">asset type: Put, Call, ExeAssPut, ExeAssCall. Other values do not return </param>
        /// <param name="pUnderlyingQuote">theoretical price of the asset underlying</param>
        /// <param name="pStrikePrice">exercice price of the asset option</param>
        /// <returns>the ITM/OTm amount</returns>
        protected decimal GetInTheMoneyAmount(RiskMethodQtyType pType, decimal pUnderlyingQuote, decimal pStrikePrice)
        {
            return
                (pType == RiskMethodQtyType.Call || pType == RiskMethodQtyType.ExeAssCall) ?
                    pUnderlyingQuote - pStrikePrice :
                    pStrikePrice - pUnderlyingQuote;
        }

        /// <summary>
        /// Transform the asset data in a risk quantity type 
        /// </summary>
        /// <param name="pContractCategory">contract category, O for Options, any other value for Futures</param>
        /// <param name="pPutCall">put call of the option category asset, 0 for Put 1 for Call, null for Futures</param>
        /// <param name="pExeAss">true, when the asset is linked to an exercise/assignation </param>
        /// <returns>a risk quantity type, when the category is unknown the Put default value is returned</returns>
        protected RiskMethodQtyType GetRiskMethodQtyType(string pContractCategory, string pPutCall, bool pExeAss)
        {
            RiskMethodQtyType type;
            switch (pContractCategory)
            {
                case "O":
                    if (pExeAss)
                    {
                        type = (pPutCall == "0") ? RiskMethodQtyType.ExeAssPut : RiskMethodQtyType.ExeAssCall;
                    }
                    else
                    {
                        type = (pPutCall == "0") ? RiskMethodQtyType.Put : RiskMethodQtyType.Call;
                    }
                    break;

                case "F":
                default:
                    type = (pExeAss) ? RiskMethodQtyType.FutureMoff : RiskMethodQtyType.Future;
                    break;
            }

            return type;
        }

        // PM 20130326 Ne plus modifier la requête avec la liste de DC en position, mais utiliser la table IMASSET_ETD
        //private void LoadStockCoverages(
        //    Dictionary<int, SQL_AssetETD> pAssetETDCache, IDbConnection pConnection,
        //    Dictionary<string, object> pDbParametersValue, int[] pContractsId)
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        private void LoadStockCoverages(IDbConnection pConnection)
        {
            //object[] boxedContractsId = new object[pContractsId.Length];
            //pContractsId.CopyTo(boxedContractsId, 0);

            //object[][] valuesMatrix = { boxedContractsId };

            CommandType stockCovCmdTyp = DataContractHelper.GetType(DataContractResultSets.EQUITYSTOCKSCOVERAGE);

            //string stocksCoverageRequest =
            //    DataHelper<object>.IsNullTransform(
            //        pConnection,
            //        stockCovCmdTyp,
            //        DataContractHelper.GetQuery(DataContractResultSets.EQUITYSTOCKSCOVERAGE, valuesMatrix)
            //    );
            string stocksCoverageRequest = DataContractHelper.GetQuery(DataContractResultSets.EQUITYSTOCKSCOVERAGE);

            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>
            {
                { "DTBUSINESS", this.DtBusiness.Date }
            };

            m_EquityStockCoverages =
                DataHelper<StocksCoverageParameter>.ExecuteDataSet(
                    pConnection,
                    stockCovCmdTyp,
                    stocksCoverageRequest,
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.EQUITYSTOCKSCOVERAGE, dbParametersValue)
                );

            // Searching for a more specific stock coverage strategy if it exists
            foreach (StocksCoverageParameter stockElement in m_EquityStockCoverages)
            {
                stockElement.Type = FindSpecificPosStockCover(stockElement.PayActorId, stockElement.MarketId, stockElement.Type);
            }

            // PM 20201028 [25570][25542] Ajout gestion basket
            if (m_EquityStockCoverages.Count > 0)
            {
                // Vérifier qu'il y a des positions equity de type basket constituant pour réduire les postions ETD
                // (C'est le cas si le sous-jacent ETD est un Basket et la position est un Equity)
                if (m_EquityStockCoverages.Where(e => ((e.UnlAssetClassEnum.HasValue) && (e.UnlAssetClassEnum == UnderlyingAssetEnum.Basket))
                && ((e.EquityAssetClassEnum.HasValue) && (e.EquityAssetClassEnum == UnderlyingAssetEnum.StockEquities))).Count() > 0)
                {
                    // Chargement des caractèristiques des baskets d'equity
                    List<EquityBasketSetting> m_EquityBasketSetting = LoadParametersMethod<EquityBasketSetting>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.EQUITYBASKETSETTING);

                    // Constitution de l'ensemble des objets contenant le paramètrage des baskets
                    m_BasketCoverageSetting = (
                        from bsk in m_EquityBasketSetting
                        group bsk by new { bsk.BasketIdAsset, bsk.BasketIdentifier, bsk.BasketUnitTypeWeight } into bskPct
                        select new BasketCoverageSetting(
                                   bskPct.Key.BasketIdAsset,
                                   bskPct.Key.BasketIdentifier,
                                   bskPct.Key.BasketUnitTypeWeight,
                                   bskPct.ToDictionary(e => e.EquityIdAsset, e => e.EquityWeight)
                                   )
                        ).ToList();

                    // Retirer les baskets paramètrés en pourcentage et qui n'ont pas 100% de constituant
                    List<BasketCoverageSetting> basketNotCorrect = m_BasketCoverageSetting.Where(b => (b.BasketUnitTypeWeight == BasketUnitTypeWeightEnum.basketPercentage) && (b.TotalWeight != 100)).ToList();
                    if (basketNotCorrect.Count() > 0)
                    {
                        ProcessInfo.Process.ProcessState.Status = ProcessStateTools.StatusEnum.WARNING;

                        #region log
                        string allIdForMsg = basketNotCorrect.GetRange(0, System.Math.Min(basketNotCorrect.Count(), 10)).Select(b => b.BasketIdentifier).Aggregate((a, b) => a + ", " + b);
                        if (basketNotCorrect.Count() > 10)
                        {
                            allIdForMsg += ", ...";
                        }
                        LoggerClient.Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 1033), 1, new LogParam(allIdForMsg)));
                        #endregion log

                        foreach (BasketCoverageSetting basketToRemove in basketNotCorrect)
                        {
                            m_BasketCoverageSetting.Remove(basketToRemove);
                        }
                    }

                    // Regrouper les positions Equity constituant par Basket
                    var basketInPo = from equity in m_EquityStockCoverages
                                     where (equity.EquityAssetClassEnum.HasValue) && (equity.EquityAssetClassEnum == UnderlyingAssetEnum.StockEquities)
                                     && (equity.UnlAssetClassEnum.HasValue) && (equity.UnlAssetClassEnum == UnderlyingAssetEnum.Basket)
                                     join basket in m_BasketCoverageSetting on equity.UnlAssetId equals basket.BasketIdAsset
                                     group equity by new
                                     {
                                         equity.UnlAssetId,
                                         equity.ContractId,
                                         equity.MarketId,
                                         equity.PayActorId,
                                         equity.PayBookId,
                                         equity.RecActorId,
                                         equity.RecBookId,
                                         equity.Type,
                                         equity.BasketUnitTypeWeight,
                                         equity.GroupByContractId,
                                     }
                                       into posEquityofBasket
                                     select posEquityofBasket;

                    // Parcours des Baskets pouvant être formés
                    foreach (var basket in basketInPo)
                    {
                        BasketCoverageSetting basketSetting = m_BasketCoverageSetting.FirstOrDefault(e => e.BasketIdAsset == basket.Key.UnlAssetId);
                        if (basketSetting != default(BasketCoverageSetting))
                        {
                            // Construction du basket
                            BasketBuilder builder = new BasketBuilder(basketSetting);
                            StocksCoverageParameter newBasket = builder.BuildBasket(basket.ToList());
                            if (newBasket != default(StocksCoverageParameter))
                            {
                                m_EquityStockCoverages.Add(newBasket);
                            }
                        }
                    }
                }
            }
        }

        // UNDONE 20110923 la position sur actions est partagée entre toutes les méthodes qui souhaitent la utiliser, par contre la version actuelle
        //  marche que sur le TIMS IDEM et les valeurs de la table CCG_D13A
        private void LoadPositionActions(IDbConnection pConnection, Dictionary<string, object> dbParametersValue, string pCS)
        {
            dbParametersValue.Add("IDACSS", this.IdCSS);
            dbParametersValue.Add("IDENTITY", this.IdEntity);

            object[] castToStringClearingCode = new object[] { DataHelper.SQLNumberToChar(pCS, "d13.MEMBERCLEARINGCODE") };
            object[] castToStringMarketId = new object[] { DataHelper.SQLNumberToChar(pCS, "d13.MARKETID") };
            object[] substringCompartmentCode = new object[] { DataHelper.SQLSubstring(pCS, "cc.COMPARTMENTCODE", 1, 1) };

            object[][] valuesMatrix = { castToStringClearingCode, castToStringMarketId, substringCompartmentCode };

            CommandType type = DataContractHelper.GetType(DataContractResultSets.POSITIONACTIONSCCG);

            string positionActionsRequest =
                    DataContractHelper.GetQuery(DataContractResultSets.POSITIONACTIONSCCG, valuesMatrix);

            positionActionsRequest = DataHelper<PositionAction>.IsNullTransform(pConnection, type, positionActionsRequest);

            List<PositionAction> positionActions =
                DataHelper<PositionAction>.ExecuteDataSet(
                    pConnection,
                    type,
                    positionActionsRequest,
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.POSITIONACTIONSCCG, dbParametersValue)
                );

            m_PositionActions =
                (from position in positionActions
                 select new Pair<PosActionRiskMarginKey, RiskMarginPositionAction>
                 {

                     First = new PosActionRiskMarginKey
                     {
                         derivativeContractSymbol = position.DerivativeContractSymbol,
                         maturityDate = position.MaturityDate,
                         idAsset = position.AssetId,
                         Side = position.Side,
                         idA_Dealer = position.ActorId,
                         idB_Dealer = position.BookId,
                         idA_EntityDealer = this.IdEntity,
                         idA_EntityClearer = this.IdEntity,
                         idA_Clearer = 0,
                         idB_Clearer = 0,
                         idI = 0,
                     },

                     Second = new RiskMarginPositionAction
                     {
                         // EG 20150920 [21374] Int (int32) to Long (Int64) 
                         // EG 20170127 Qty Long To Decimal
                         Quantity = (decimal)position.Quantity,
                         LongCrtVal = position.LongCrtVal,
                         ShortCrtVal = position.ShortCrtVal,
                         Currency = position.Currency,
                     }
                 }).ToList();

        }

        /// <summary>
        /// Charge le jeu de paramètres contenant les informations étendues sur les asset_etd
        /// </summary>
        /// <param name="pConnection">Connexion courante</param>
        /// <returns>Liste des informations étendues sur les asset_etd</returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        public List<AssetExpandedParameter> LoadParametersAssetExpanded(IDbConnection pConnection)
        {
            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>
            {
                { "DTBUSINESS", DtBusiness.Date }
            };

            // ASSETEXPANDED_ALLMETHOD
            List<AssetExpandedParameter> ret = LoadParametersMethod<AssetExpandedParameter>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.ASSETEXPANDED_ALLMETHOD);
            return ret;
        }

        /// <summary>
        /// Charge le jeu de paramètres contenant les informations étendues sur les asset_etd future qui sont sous-jacents d'options
        /// </summary>
        /// <param name="pConnection">Connexion courante</param>
        /// <returns>Liste des informations étendues sur les asset_etd future qui sont sous-jacents d'options</returns>
        public List<AssetExpandedParameter> LoadParametersUnderlyingAssetFutureExpanded(IDbConnection pConnection)
        {
            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>
            {
                { "DTBUSINESS", DtBusiness.Date }
            };

            // UNDERLYNGASSETFUTUREEXPANDED_ALLMETHOD
            List<AssetExpandedParameter> ret = LoadParametersMethod<AssetExpandedParameter>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.UNDERLYNGASSETFUTUREEXPANDED_ALLMETHOD);
            return ret;
        }
        
        /// <summary>
        /// Charge le jeu de paramètres contenant les informations étendues sur les assets sous-jacents des assets ETD
        /// </summary>
        /// <param name="pConnection">Connexion courante</param>
        /// <returns>Liste des informations étendues sur les sous jacents des asset_etd</returns>
        public List<AssetAllExpandedParameter> LoadParametersUnderlyingAssetExpanded(IDbConnection pConnection)
        {

            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>
            {
                { "DTBUSINESS", DtBusiness.Date }
            };


            List<AssetAllExpandedParameter> ret = LoadParametersMethod<AssetAllExpandedParameter>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.UNDERLYNGASSETEXPANDED_ALLMETHOD);
            return ret;
        }

        /// <summary>
        /// get the specific coverage type
        /// </summary>
        /// <param name="pActorId">actor owning the stocks</param>
        /// <param name="pMarketId">market reference id of the current coverage element</param>
        /// <param name="pMainPosStockCover">specific coverage type (normally owned by the current coverage elemen)</param>
        /// <returns>the more specific (and valid) coverage type found for the input actor</returns>
        private PosStockCoverEnum FindSpecificPosStockCover(int pActorId, int pMarketId, PosStockCoverEnum pMainPosStockCover)
        {
            // default coverage type, the more specific 
            PosStockCoverEnum res = pMainPosStockCover;

            // when the coverage type is none we pass to the higher level (the coverage type given for the pair actor/market)
            if (res == PosStockCoverEnum.None && MarginReqOfficeParameters.ContainsKey(pActorId))
            {
                MarginReqOfficeParameter specificClearingHouseParam =
                    (from parameter in MarginReqOfficeParameters[pActorId]
                     where parameter.MarketId == pMarketId
                     select parameter)
                    .FirstOrDefault();

                // PM 20170106 [22633] Ajout test de présence du paramétrage
                if ((specificClearingHouseParam != default(MarginReqOfficeParameter)) && (specificClearingHouseParam.ActorId > 0))
                {
                    res = specificClearingHouseParam.StockCoverageType;
                }
            }

            // if the coverage type is none again we pass to the highest level (the coverage type given for the market itself)
            if (res == PosStockCoverEnum.None && MarketParameters.ContainsKey(pMarketId))
            {
                res = MarketParameters[pMarketId].StockCoverageType;
            }

            return res;
        }

        /// <summary>
        /// Indique s'il existe des paramètres de risque pour une date donnée
        /// </summary>
        /// <param name="pCs">Chaîne de connexion</param>
        /// <param name="pDtBusiness">Date pour laquelle vérifier l'existance de paramètres de risque</param>
        /// <returns>True s'il existe des paramètres de risque pour la date donnée</returns>
        /// PM 20150507 [20575] Add ExistRiskParameter
        // EG 20180803 PERF IMASSET_ETD_{BuildTableId}_W
        // PM 20180926 [XXXXX] Correction : ne pas prendre "SessionId" mais "pBuildTableId"
        protected virtual bool ExistRiskParameter(string pCs, DateTime pDtBusiness)
        {
            bool existData = false;
            // PM 20180926 [XXXXX] Correction : ne pas prendre "SessionId" mais "BuildTableId"
            //string _tableName = StrFunc.AppendFormat("IMASSET_ETD_{0}_W", SessionId).ToUpper();
            string buildTableId = ProcessInfo.Process.Session.BuildTableId();
            string _tableName = StrFunc.AppendFormat("IMASSET_ETD_{0}_W", buildTableId).ToUpper();
            string query = QueryExistRiskParameter.Replace("IMASSET_ETD_MODEL", _tableName);

            if (StrFunc.IsFilled(query))
            {
                if (query.ToUpper().IndexOf("@" + DataParameter.ParameterEnum.DTBUSINESS.ToString()) > -1)
                {
                    DataParameters dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness);
                    if (query.ToUpper().IndexOf("@" + DataParameter.ParameterEnum.SESSIONID.ToString()) > -1)
                    {
                        dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.SESSIONID), ProcessInfo.Process.Session.SessionId);
                    }

                    QueryParameters qry = new QueryParameters(pCs, query, dp);

                    object obj = DataHelper.ExecuteScalar(pCs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

                    existData = (null != obj);
                }
                else
                {
                    object obj = DataHelper.ExecuteScalar(pCs, CommandType.Text, query);
                    existData = (null != obj);
                }
            }
            return existData;
        }

        /// <summary>
        /// Vérifia s'il existe des paramètres de risque à la date DtBusiness, retourne DtBusiness s'il en existe, sinon retourne DtMarket.
        /// </summary>
        /// <param name="pCs">Chaîne de connexion</param>
        /// <returns>DtBusiness s'il existe des paramètres de risques à cette date, sinon DtMarket</returns>
        /// PM 20150511 [20575] Add GetRiskParametersDate
        protected DateTime GetRiskParametersDate(string pCs)
        {
            DateTime dtBusiness = this.DtBusiness.Date;
            DateTime dtMarket = this.DtMarket.Date;
            if (dtBusiness > dtMarket)
            {
                // Date business entité fériée sur la chambre
                //    
                // PM 20180530 [23824] Ajout test pour prendre en compte les données lus directement dans les fichiers
                if (m_IdIOTaskRiskData != 0)
                {
                    if (false == m_IsRiskDataOnHoliday)
                    {
                        // Pas de paramètres pour la journée courante, prendre ceux de la dernière journée chambre
                        dtBusiness = dtMarket;
                    }
                }
                else if (false == ExistRiskParameter(pCs, dtBusiness))
                {
                    // Pas de paramètres pour la journée courante, prendre ceux de la dernière journée chambre
                    dtBusiness = dtMarket;
                }
            }
            DtRiskParameters = dtBusiness;
            return DtRiskParameters;
        }
    }

}

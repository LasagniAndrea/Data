using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
//
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
    /// Classe de calcul du déposit par la méthode CBOE Margin
    /// </summary>
    public sealed class CBOEMarginMethod : BaseMethod
    {
        #region members
        private readonly static ExchangeTradedDerivative m_Product = new ExchangeTradedDerivative();
        private IEnumerable<CboeAssetExpandedParameter> m_AssetExpandedParameters = null;
        private IEnumerable<CboeContractParameter> m_ContractParameters = null;
        private List<AssetQuoteParameter> m_AssetQuoteParameters = null;
        private List<AssetQuoteParameter> m_UnderlyingAssetQuoteParameters = null;
        #endregion members

        #region override base accessors
        /// <summary>
        /// Type de la Methode
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.CBOE_Margin; }
        }

        /// <summary>
        /// Requête utilisée pour connaître l'existance de paramètres de risque pour une date donnée
        /// <remarks>Utilise les paramètres DTBUSINESS & SESSIONID</remarks>
        /// </summary>
        /// PM 20150512 [20575] Add QueryExistRiskParameter
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        protected override string QueryExistRiskParameter
        {
            get
            {
                string query;
                query = @"
                    select distinct 1
                      from dbo.QUOTE_ETD_H q
                     inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = q.IDASSET)
                     where (q.TIME = @DTBUSINESS) and (q.QUOTESIDE = 'OfficialClose') and (q.QUOTETIMING = 'Close')";
                return query;
            }
        }
        #endregion

        #region constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        internal CBOEMarginMethod()
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
            // PM 20150512 [20575] Ajout gestion dtMarket 
            //DateTime dtBusiness = this.DtBusiness.Date;
            DateTime dtBusiness = GetRiskParametersDate(pCS);

            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {

                if (pAssetETDCache.Count > 0)
                {
                    // Set Parameters
                    dbParametersValue.Add("DTBUSINESS", dtBusiness);
                    //dbParametersValue.Add("SESSIONID", SessionId);

                    // ASSETEXPANDEDPARAM_CBOEMARGINMETHOD
                    m_AssetExpandedParameters = LoadParametersMethod<CboeAssetExpandedParameter>.LoadParameters(connection, dbParametersValue, DataContractResultSets.ASSETEXPANDEDPARAM_CBOEMARGINMETHOD);

                    // CONTRACTPARAM_CBOEMARGINMETHOD
                    m_ContractParameters = LoadParametersMethod<CboeContractParameter>.LoadParameters(connection, dbParametersValue, DataContractResultSets.CONTRACTPARAM_CBOEMARGINMETHOD);

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
            }
        }

        /// <summary>
        /// Libère les paramètres spécifiques à la méthode.
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            // PM 20130812 [18881] Correction pour le cas où aucune cotation chargée
            if (null != m_AssetQuoteParameters)
                m_AssetQuoteParameters.Clear();
            if (null != m_UnderlyingAssetQuoteParameters)
                m_UnderlyingAssetQuoteParameters.Clear();
            m_AssetExpandedParameters = null;
            m_ContractParameters = null;
            m_AssetQuoteParameters = null;
            m_UnderlyingAssetQuoteParameters = null;
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
            CboeMarginCalcMethCom methodComObj = new CboeMarginCalcMethCom();
            opMethodComObj = methodComObj;
            methodComObj.MarginMethodType = this.Type;
            // Affectation de la devise de calcul
            methodComObj.CssCurrency = m_CssCurrency;
            //PM 20150512 [20575] Ajout date des paramètres de risque
            methodComObj.DtParameters = DtRiskParameters;
            // PM 20191025 [24983] Ajout IsMaintenanceAmount
            methodComObj.IsMaintenanceAmount = IsMaintenance(pActorId);
            //
            if (pRiskDataToEvaluate != default(RiskData))
            {
                // PM 20170313 [22833] Prendre uniquement la position (à l'ancien format)
                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsToEvaluate = pRiskDataToEvaluate.GetPositionAsEnumerablePair();

                if ((positionsToEvaluate != null) && (positionsToEvaluate.Count() > 0))
                {
                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions;

                    // Regrouper les position par asset (le sens de la position cumulée par asset sera defini en fonction de la quantité long ou short)
                    positions = PositionsGrouping.GroupPositionsByAsset(positionsToEvaluate);

                    // Ne garder que les positions dont la quantité est différente de 0
                    positions =
                        from pos in positions
                        where pos.Second.Quantity != 0
                        select pos;

                    // Couverture des short call et short futures (cela modifiera la quantité en position)
                    IEnumerable<CoverageSortParameters> inputCoverage = GetSortParametersForCoverage(positions);
                    // Reduction de la position couverte
                    // FI 20160613 [22256] 
                    Pair<IEnumerable<StockCoverageCommunicationObject>, IEnumerable<StockCoverageDetailCommunicationObject>> coveredQuantities =
                        ReducePosition(pActorId, pBookId, pDepositHierarchyClass, inputCoverage, ref positions);

                    // Construction de la hierarchie contrat/asset avec la position associée
                    List<CboeContractMarginCom> contractMargin = GetContractHierarchy(positions, coveredQuantities.First);
                    methodComObj.Parameters = contractMargin.ToArray();
                    // FI 20160613 [22256] Alimentation de UnderlyingStock
                    methodComObj.UnderlyingStock = coveredQuantities.Second;

                    // Calculer les montants de risque
                    // PM 20191025 [24983] Ajout paramètre pMethodComObj
                    riskAmounts = EvaluateRisk(contractMargin, methodComObj);
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
        /// Collection of sorting parameter needed by coverage strategies
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">positions, netted by asset, for the current risk element</param>
        /// <returns>a collection of sorting parameter in order to be used inside of the ReducePosition method </returns>
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
        #endregion override base methods

        #region methods
        /// <summary>
        /// Groupe les assets par contrat.
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">Position cumulée par asset</param>
        /// <param name="pCoveredQuantities">Position couverte par des actions</param>
        /// <returns>Liste des objets de détail du calcul de déposit hiérarchisés par contrat/asset</returns>
        // PM 20130322 Ajout position couverte par des actions
        // PM 20191025 [24983] Ajout Montant de Maintenance
        private List<CboeContractMarginCom> GetContractHierarchy(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset,
            IEnumerable<StockCoverageCommunicationObject> pCoveredQuantities)
        {
            string[] datas = new string[5];

            // Liste des Contract avec leurs position et paramètres de calculs
            List<CboeContractMarginCom> contractMargin = (
                from pos in pGroupedPositionsByIdAsset
                join asset in m_AssetExpandedParameters on pos.First.idAsset equals asset.AssetId
                join contract in m_ContractParameters on asset.ContractId equals contract.ContractId
                join underlyingQuote in m_UnderlyingAssetQuoteParameters on asset.UnderlyningAssetId equals underlyingQuote.AssetId
                where (underlyingQuote.AssetCategoryEnum == asset.UnderlyingAssetCategoryEnum)
                group pos by new { Contract = contract, Underlying = underlyingQuote, asset.Currency }
                    into contractPos
                    select new CboeContractMarginCom
                    {
                        Contract = contractPos.Key.Contract,
                        UnderlyingAssetId = contractPos.Key.Underlying.AssetId,
                        UnderlyingAssetCategoryEnum = contractPos.Key.Underlying.AssetCategoryEnum,
                        UnderlyingQuote = contractPos.Key.Underlying.Quote,
                        Positions = contractPos,
                        MarginAmount = new Money(0, contractPos.Key.Currency),
                        Missing = contractPos.Key.Underlying.SystemMsgInfo != null,
                        NormalMarginAmountInit = new Money(0, contractPos.Key.Currency),
                        NormalMarginAmountMaint = new Money(0, contractPos.Key.Currency),
                        StrategyMarginAmountInit = new Money(0, contractPos.Key.Currency),
                        StrategyMarginAmountMaint = new Money(0, contractPos.Key.Currency),
                        SystemMsgInfo = contractPos.Key.Underlying.SystemMsgInfo,
                    }).OrderBy(c => c.Contract.ContractSymbol).ToList();

            foreach (CboeContractMarginCom contractMrg in contractMargin)
            {
                // Si aucune erreur déjà recensée
                if (contractMrg.SystemMsgInfo == default(SystemMSGInfo))
                {
                    // Vérification de l'existance du sous-jacent
                    if (contractMrg.UnderlyingAssetId == 0)
                    {
                        datas[0] = contractMrg.Contract.ContractIdentifier;
                        datas[1] = contractMrg.UnderlyingAssetCategoryEnum.ToString();
                        datas[2] = null;
                        datas[3] = null;
                        datas[4] = null;
                        ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND);
                        
                        //contractMrg.SystemMsgInfo = new SystemMSGInfo("SYS-01029", processState, datas);
                        contractMrg.SystemMsgInfo = new SystemMSGInfo(SysCodeEnum.SYS, 1029, processState, datas);
                        contractMrg.Missing = true;
                    }
                    // Vérification de la cohérence des paramètres
                    else if ((contractMrg.Contract.PctOptionValue == 0)
                        || (contractMrg.Contract.PctUnderlyingValue == 0)
                        || (contractMrg.Contract.PctMinimumUnderlyingValue == 0))
                    {
                        datas[0] = contractMrg.Contract.ContractIdentifier;
                        datas[1] = contractMrg.UnderlyingAssetCategoryEnum.ToString();
                        datas[2] = contractMrg.Contract.PctOptionValue.ToString();
                        datas[3] = contractMrg.Contract.PctUnderlyingValue.ToString();
                        datas[4] = contractMrg.Contract.PctMinimumUnderlyingValue.ToString();
                        ProcessState processState = new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.MISSINGPARAMETER);
                        
                        //contractMrg.SystemMsgInfo = new SystemMSGInfo("SYS-01030", processState, datas);
                        contractMrg.SystemMsgInfo = new SystemMSGInfo(SysCodeEnum.SYS, 1030, processState, datas);
                        contractMrg.Missing = true;
                    }
                }

                // Pour chaque Contract, affecter la liste de ses Asset avec leur position et paramètres de calculs
                contractMrg.Parameters = (
                    from pos in contractMrg.Positions
                    join asset in m_AssetExpandedParameters on pos.First.idAsset equals asset.AssetId
                    join quote in m_AssetQuoteParameters on asset.AssetId equals quote.AssetId
                    join contract in m_ContractParameters on asset.ContractId equals contract.ContractId
                    where quote.AssetCategoryEnum == Cst.UnderlyingAsset.ExchangeTradedContract
                    select new CboeNormalMarginCom
                    {
                        Asset = asset,
                        InitialQuantity = (ReflectionTools.ConvertStringToEnum<SideEnum>(pos.First.Side) == SideEnum.Sell ? -1 : 1) * pos.Second.Quantity,
                        UnitContractValue = RiskMethodExtensions.ContractValue(1, quote.Quote, asset.ContractMultiplier, asset.InstrumentNum, asset.InstrumentDen),
                        UnitUnderlyingValue = RiskMethodExtensions.ContractValue(1, contractMrg.UnderlyingQuote, asset.ContractMultiplier, asset.InstrumentNum, asset.InstrumentDen),
                        Positions = new Pair<PosRiskMarginKey, RiskMarginPosition>[] { pos },
                        Parameters = null,
                        UnitMarginInit = 0,
                        UnitMarginMaint = 0,
                        UnitMinimumMargin = 0,
                        Quote = quote.Quote,
                        Missing = quote.SystemMsgInfo != null,
                        SystemMsgInfo = quote.SystemMsgInfo,
                    }).OrderBy(a => a.Asset.PutCall).OrderBy(a => a.Asset.StrikePrice).OrderBy(a => a.Asset.MaturityYearMonth).ToArray();

                // PM 20130322 Ajout position couverte par des actions
                contractMrg.StocksCoverage = (
                    from coveredQuantity in pCoveredQuantities
                    where coveredQuantity.ContractId == contractMrg.Contract.ContractId
                    select coveredQuantity).ToArray();
            }
            return contractMargin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pContractMargin"></param>
        /// <param name="methodComObj"></param>
        /// <returns></returns>
        // PM 20191025 [24983] Ajout paramètre pMethodComObj
        private List<Money> EvaluateRisk(List<CboeContractMarginCom> pContractMargin, CboeMarginCalcMethCom pMethodComObj)
        {
            List<Money> marginAmountList = null;
            foreach (CboeContractMarginCom contractMrg in pContractMargin)
            {
                foreach (CboeNormalMarginCom assetMrg in contractMrg.Parameters)
                {
                    assetMrg.Quantity = assetMrg.InitialQuantity;
                    // Déposit uniquement sur position vendeuse
                    if (assetMrg.Quantity < 0)
                    {
                        // Short Put or Short Call
                        //
                        assetMrg.UnitMarginInit = assetMrg.UnitContractValue + (contractMrg.Contract.PctUnderlyingValue * assetMrg.UnitUnderlyingValue);
                        //
                        // Gestion des positions hors la money
                        if (((assetMrg.Asset.PutOrCall == PutOrCallEnum.Call) && (assetMrg.Asset.StrikePrice > contractMrg.UnderlyingQuote))
                            ||
                            ((assetMrg.Asset.PutOrCall == PutOrCallEnum.Put) && (assetMrg.Asset.StrikePrice < contractMrg.UnderlyingQuote)))
                        {
                            assetMrg.UnitMarginInit -= RiskMethodExtensions.ContractValue(1, System.Math.Abs(assetMrg.Asset.StrikePrice - contractMrg.UnderlyingQuote), assetMrg.Asset.ContractMultiplier, assetMrg.Asset.InstrumentNum, assetMrg.Asset.InstrumentDen);
                        }
                        // Calcul du minimum
                        assetMrg.UnitMinimumMargin = assetMrg.UnitContractValue;
                        if (assetMrg.Asset.PutOrCall == PutOrCallEnum.Call)
                        {
                            assetMrg.UnitMinimumMargin += (contractMrg.Contract.PctMinimumUnderlyingValue * assetMrg.UnitUnderlyingValue);
                        }
                        else
                        {
                            assetMrg.UnitMinimumMargin += (contractMrg.Contract.PctMinimumUnderlyingValue * RiskMethodExtensions.ContractValue(1, assetMrg.Asset.StrikePrice, assetMrg.Asset.ContractMultiplier, assetMrg.Asset.InstrumentNum, assetMrg.Asset.InstrumentDen));
                        }
                        // Application du minimum
                        if (assetMrg.UnitMinimumMargin > assetMrg.UnitMarginInit)
                        {
                            assetMrg.UnitMarginInit = assetMrg.UnitMinimumMargin;
                        }
                        //
                        assetMrg.UnitMarginMaint = assetMrg.UnitMarginInit - assetMrg.UnitContractValue;
                    }
                    else
                    {
                        // Long Put or Long Call
                        // PM 20191025 [24983] Ajout calcul position Long
                        // PM 20230322 [26282][WI607] Ajout gestion NbMonthLongCall et NbMonthLongPut et exclusion asset PremiumStyle
                        int nbMonth = 0;
                        if ((assetMrg.Asset.ValuationMethodEnum != FixML.v50SP1.Enum.FuturesValuationMethodEnum.PremiumStyle) && (assetMrg.Asset.PutOrCall.HasValue))
                        {
                            if (assetMrg.Asset.PutOrCall.Value == PutOrCallEnum.Call)
                            {
                                nbMonth = MethodParameter.NbMonthLongCall;
                            }
                            else
                            {
                                nbMonth = MethodParameter.NbMonthLongPut;
                            }
                            nbMonth = System.Math.Max(nbMonth, 0);
                        }
                        int diffMonth = 0;
                        if (nbMonth > 0)
                        { 
                            // PM 20230113 [XXXXX][WI538] Correction de la méthode DiffMonth
                            diffMonth = DiffMonth(DtBusiness, assetMrg.Asset.MaturityDate);
                        }
                        //if (diffMonth > 9)
                        if ((nbMonth > 0 ) && (diffMonth > nbMonth))
                        {
                            // Plus de 9 mois avant l'échéance
                            assetMrg.UnitMarginMaint = (contractMrg.Contract.PctOptionValue * assetMrg.UnitContractValue);
                            assetMrg.UnitMarginInit = assetMrg.UnitMarginMaint;
                        }
                        else
                        {
                            // 9 mois ou moins avant l'échéance
                            assetMrg.UnitMarginMaint = 0;
                            assetMrg.UnitMarginInit = assetMrg.UnitMarginMaint;
                        }
                    }
                }
                // Calcul des déposit pour stratégie
                contractMrg.StrategyMarginList = EvaluateStrategy(contractMrg);
                contractMrg.StrategyMarginAmountInit.Amount.DecValue = contractMrg.StrategyMarginList.Sum(a => a.MarginAmountInit.Amount.DecValue);
                contractMrg.StrategyMarginAmountMaint.Amount.DecValue = contractMrg.StrategyMarginList.Sum(a => a.MarginAmountMaint.Amount.DecValue);
                //
                // Cumul des déposits "Normal"
                // PM 20191025 [24983] Ajout gestion Maintenance et Initial
                foreach (CboeNormalMarginCom assetMrg in contractMrg.Parameters)
                {
                    // EG 20170127 Qty Long To Decimal
                    decimal quantity = System.Math.Abs(assetMrg.Quantity);
                    assetMrg.ContractValue = new Money(quantity * assetMrg.UnitContractValue, assetMrg.Asset.Currency);
                    assetMrg.MarginAmountInit = new Money(quantity * assetMrg.UnitMarginInit, assetMrg.Asset.Currency);
                    assetMrg.MarginAmountMaint = new Money(quantity * assetMrg.UnitMarginMaint, assetMrg.Asset.Currency);
                    assetMrg.MinMarginAmount = new Money(quantity * assetMrg.UnitMinimumMargin, assetMrg.Asset.Currency);
                    assetMrg.MarginAmount = pMethodComObj.IsMaintenanceAmount ? assetMrg.MarginAmountMaint : assetMrg.MarginAmountInit;
                }
                contractMrg.NormalMarginAmountInit.Amount.DecValue = contractMrg.Parameters.Sum(a => ((CboeNormalMarginCom)a).MarginAmountInit.Amount.DecValue);
                contractMrg.NormalMarginAmountMaint.Amount.DecValue = contractMrg.Parameters.Sum(a => ((CboeNormalMarginCom)a).MarginAmountMaint.Amount.DecValue);

                // Cumul pour le contrat
                if (pMethodComObj.IsMaintenanceAmount)
                {
                    contractMrg.MarginAmount.Amount.DecValue = contractMrg.NormalMarginAmountMaint.Amount.DecValue + contractMrg.StrategyMarginAmountMaint.Amount.DecValue;
                }
                else
                {
                    contractMrg.MarginAmount.Amount.DecValue = contractMrg.NormalMarginAmountInit.Amount.DecValue + contractMrg.StrategyMarginAmountInit.Amount.DecValue;
                }
            }

            marginAmountList = (
                from contractMrg in pContractMargin
                group contractMrg by contractMrg.MarginAmount.Currency
                    into ctrGrp
                    select new Money(
                        ctrGrp.Sum(a => a.MarginAmount.Amount.DecValue),
                        ctrGrp.Key)
                ).ToList();

            return marginAmountList;
        }

        /// <summary>
        /// Calcul les stratégie possible entre les positions des assets d'un contrat
        /// </summary>
        /// <param name="pContractMargin">Paramètres du contrat sur lequel effectuer le calcul</param>
        /// <returns>La liste des stratégies trouvées avec le montant de déposit engendré par chaque stratégie</returns>
        private List<CboeStrategyMarginCom> EvaluateStrategy(CboeContractMarginCom pContractMargin)
        {
            List<CboeStrategyMarginCom> strategyList = new List<CboeStrategyMarginCom>();
            if (pContractMargin != default(CboeContractMarginCom))
            {
                List<CboeNormalMarginCom> posForStrategy = (
                    from pos in pContractMargin.Parameters
                    where ((CboeNormalMarginCom)pos).Missing != true
                    select (CboeNormalMarginCom)pos
                ).ToList();

                // Vérification qu'il y a au moins 2 assets en position sur le contrat
                if (posForStrategy.Count > 1)
                {
                    CboeStrategyMarginCom strategy = default;
                    // Recherche de position pouvant faire l'objet d'une strategie
                    // dans l'ordre des échéances lointaines vers les échéances proches
                    // et des strikes élevés vers les strikes faibles
                    List<CboeNormalMarginCom> posForStrategyFirst = posForStrategy.
                        OrderBy(p => (p.Asset.PutOrCall == PutOrCallEnum.Call) ? 1 : 2).
                        OrderByDescending(p => p.Asset.StrikePrice).
                        OrderByDescending(p => p.Asset.MaturityYearMonth).
                        OrderByDescending(p => p.UnitMarginInit).
                        OrderBy(p => (p.Quantity < 0) ? 1 : 2).ToList();

                    foreach (CboeNormalMarginCom posFirst in posForStrategyFirst)
                    {
                        // Recherche de position pouvant faire l'objet d'une strategie
                        // autre que la position courante
                        List<CboeNormalMarginCom> posForStrategySecond = (
                            from posSecond in posForStrategy
                            where (posSecond.Quantity != 0)
                            && (posSecond.Asset.MaturityYearMonth.CompareTo(posFirst.Asset.MaturityYearMonth) <= 0)
                            && ((posSecond.Asset.MaturityYearMonth.CompareTo(posFirst.Asset.MaturityYearMonth) != 0)
                             || (posSecond.Asset.PutOrCall != posFirst.Asset.PutOrCall)
                             || (posSecond.Asset.StrikePrice != posFirst.Asset.StrikePrice))
                            select posSecond).
                            OrderBy(p => (p.Asset.PutOrCall == PutOrCallEnum.Call) ? 1 : 2).
                            OrderByDescending(p => p.Asset.StrikePrice).
                            OrderByDescending(p => p.Asset.MaturityYearMonth).
                            OrderByDescending(p => p.UnitMarginInit).
                            OrderBy(p => (p.Quantity < 0) ? 1 : 2).ToList();

                        foreach (CboeNormalMarginCom posSecond in posForStrategySecond)
                        {
                            // Tenter de réaliser des stratégies dans l'ordre : Spread, Straddle, Combination
                            //
                            // Tenter de réaliser un Spread
                            strategy = EvaluateOneSpread(posFirst, posSecond);
                            if (strategy != default(CboeStrategyMarginCom))
                            {
                                strategyList.Add(strategy);
                            }
                            // Tenter de réaliser un Straddle
                            strategy = EvaluateOneStraddle(posFirst, posSecond);
                            if (strategy != default(CboeStrategyMarginCom))
                            {
                                strategyList.Add(strategy);
                            }
                            // Tenter de réaliser une Combination
                            strategy = EvaluateOneCombination(posFirst, posSecond);
                            if (strategy != default(CboeStrategyMarginCom))
                            {
                                strategyList.Add(strategy);
                            }
                        }
                    }
                }
            }
            return strategyList;
        }

        /// <summary>
        /// Tente de réaliser une stratégie comprenant :
        /// des positions sur deux assets de même PutOrCall et de Side opposé.
        /// Evaluation du déposit pour cette stratégie.
        /// </summary>
        /// <param name="pPosFirst">Position sur le premier asset</param>
        /// <param name="pPosSecond">Position sur le deuxième asset</param>
        /// <returns>Null si la stratégie n'a pas pu être réalisée, sinon un objet CboeStrategyMarginCom représentant la stratégie et son déposit</returns>
        // PM 20191025 [24983] Ajout Maintenance Margin et modification calcul des spreads
        private CboeStrategyMarginCom EvaluateOneSpread(CboeNormalMarginCom pPosFirst, CboeNormalMarginCom pPosSecond)
        {
            CboeStrategyMarginCom strategy = default;
            // Vérification positions ayant même PutOrCall et Side opposé
            if ((pPosFirst.Asset.PutOrCall == pPosSecond.Asset.PutOrCall)
               && ((pPosFirst.Quantity * pPosSecond.Quantity) < 0))
            {
                bool isSpreadPossible = true;

                #region remplacé par le bloc de conditions ci-dessous
                #endregion remplacé par le bloc de conditions ci-dessous
                CboeNormalMarginCom posLong = (pPosFirst.Quantity > 0) ? pPosFirst : pPosSecond;
                CboeNormalMarginCom posShort = (pPosFirst.Quantity > 0) ? pPosSecond : pPosFirst;
                CboeStrategyTypeEnum strategyType = CboeStrategyTypeEnum.Spread;
                decimal unitMarginInit;
                //
                if (posLong.Asset.PutOrCall == PutOrCallEnum.Call)
                {
                    #region CALL
                    if (posLong.Asset.MaturityYearMonth == posShort.Asset.MaturityYearMonth)
                    {
                        #region Même échéance
                        if (posLong.Asset.StrikePrice == posShort.Asset.StrikePrice)
                        {
                            #region Même strike
                            if (posLong.Quote > posShort.Quote)
                            {
                                unitMarginInit = posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            else
                            {
                                // Spread => 0 (car montant négatif)
                                unitMarginInit = 0;
                            }
                            #endregion Même strike
                        }
                        else if (posLong.Asset.StrikePrice < posShort.Asset.StrikePrice)
                        {
                            #region Strike Long < Strike Short
                            if (posLong.Quote > posShort.Quote)
                            {
                                // Long Call Spread
                                strategyType = CboeStrategyTypeEnum.LongCallSpread;
                                unitMarginInit = posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            else
                            {
                                // Spread => 0 (car montant négatif)
                                unitMarginInit = 0;
                            }
                            #endregion Strike Long < Strike Short
                        }
                        else // (posLong.Asset.StrikePrice > posShort.Asset.StrikePrice)
                        {
                            #region Strike Long > Strike Short
                            if (posLong.Quote < posShort.Quote)
                            {
                                // Short Call Spread
                                strategyType = CboeStrategyTypeEnum.ShortCallSpread;
                                unitMarginInit = StrikeDifference(posLong, posShort);
                                unitMarginInit += posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            else
                            {
                                unitMarginInit = StrikeDifference(posLong, posShort);
                                unitMarginInit += posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            #endregion Strike Long > Strike Short
                        }
                        #endregion Même échéance
                    }
                    else if (posLong.Asset.MaturityYearMonth.CompareTo(posShort.Asset.MaturityYearMonth) > 0)
                    {
                        #region Long expire après Short
                        if (posLong.Asset.StrikePrice == posShort.Asset.StrikePrice)
                        {
                            #region Même strike
                            if (posLong.Quote > posShort.Quote)
                            {
                                // Long Call Time Spread
                                strategyType = CboeStrategyTypeEnum.LongCallTimeSpread;
                                unitMarginInit = posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            else
                            {
                                // Spread => 0 (car montant négatif)
                                unitMarginInit = 0;
                            }
                            #endregion Même strike
                        }
                        else if (posLong.Asset.StrikePrice < posShort.Asset.StrikePrice)
                        {
                            #region Strike Long < Strike Short
                            if (posLong.Quote > posShort.Quote)
                            {
                                unitMarginInit = posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            else
                            {
                                // Spread => 0 (car montant négatif)
                                unitMarginInit = 0;
                            }
                            #endregion Strike Long < Strike Short
                        }
                        else // (posLong.Asset.StrikePrice > posShort.Asset.StrikePrice)
                        {
                            #region Strike Long > Strike Short
                            unitMarginInit = StrikeDifference(posLong, posShort);
                            unitMarginInit += posLong.UnitContractValue - posShort.UnitContractValue;
                            #endregion Strike Long > Strike Short
                        }
                        #endregion Long expire après Short
                    }
                    else
                    {
                        #region Short expire après Long
                        if (posLong.Asset.StrikePrice == posShort.Asset.StrikePrice)
                        {
                            #region Même strike
                            if (posLong.Quote < posShort.Quote)
                            {
                                // Short Call Time Spread
                                strategyType = CboeStrategyTypeEnum.ShortCallTimeSpread;
                                unitMarginInit = posLong.UnitMarginInit + posShort.UnitMarginInit - posShort.UnitContractValue;
                            }
                            else
                            {
                                unitMarginInit = posLong.UnitMarginInit + posShort.UnitMarginInit - posShort.UnitContractValue;
                            }
                            #endregion Même strike
                        }
                        else
                        {
                            unitMarginInit = posLong.UnitMarginInit + posShort.UnitMarginInit - posShort.UnitContractValue;
                        }
                        #endregion Short expire après Long
                    }
                    #endregion CALL
                }
                else
                {
                    #region PUT
                    if (posLong.Asset.MaturityYearMonth == posShort.Asset.MaturityYearMonth)
                    {
                        #region Même échéance
                        if (posLong.Asset.StrikePrice == posShort.Asset.StrikePrice)
                        {
                            #region Même strike
                            if (posLong.Quote > posShort.Quote)
                            {
                                unitMarginInit = posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            else
                            {
                                // Spread => 0 (car montant négatif)
                                unitMarginInit = 0;
                            }
                            #endregion Même strike
                        }
                        else if (posLong.Asset.StrikePrice < posShort.Asset.StrikePrice)
                        {
                            #region Strike Long < Strike Short
                            if (posLong.Quote > posShort.Quote)
                            {
                                // Short Put Spread
                                strategyType = CboeStrategyTypeEnum.ShortPutSpread;
                                unitMarginInit = StrikeDifference(posShort, posLong);
                                unitMarginInit += posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            else
                            {
                                unitMarginInit = StrikeDifference(posShort, posLong);
                                unitMarginInit += posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            #endregion Strike Long < Strike Short
                        }
                        else // (posLong.Asset.StrikePrice > posShort.Asset.StrikePrice)
                        {
                            #region Strike Long > Strike Short
                            if (posLong.Quote < posShort.Quote)
                            {
                                // Long Put Spread
                                strategyType = CboeStrategyTypeEnum.LongPutSpread;
                                unitMarginInit = posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            else
                            {
                                // Spread => 0 (car montant négatif)
                                unitMarginInit = 0;
                            }
                            #endregion Strike Long > Strike Short
                        }
                        #endregion Même échéance
                    }
                    else if (posLong.Asset.MaturityYearMonth.CompareTo(posShort.Asset.MaturityYearMonth) > 0)
                    {
                        #region Long expire après Short
                        if (posLong.Asset.StrikePrice == posShort.Asset.StrikePrice)
                        {
                            #region Même strike
                            if (posLong.Quote > posShort.Quote)
                            {
                                // Long Put Time Spread
                                strategyType = CboeStrategyTypeEnum.LongPutTimeSpread;
                                unitMarginInit = posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            else
                            {
                                // Spread => 0 (car montant négatif)
                                unitMarginInit = 0;
                            }
                            #endregion Même strike
                        }
                        else if (posLong.Asset.StrikePrice < posShort.Asset.StrikePrice)
                        {
                            #region Strike Long < Strike Short
                            unitMarginInit = StrikeDifference(posShort, posLong);
                            unitMarginInit += posLong.UnitContractValue - posShort.UnitContractValue;
                            #endregion Strike Long < Strike Short
                        }
                        else // (posLong.Asset.StrikePrice > posShort.Asset.StrikePrice)
                        {
                            #region Strike Long > Strike Short
                            if (posLong.Quote > posShort.Quote)
                            {
                                unitMarginInit = posLong.UnitContractValue - posShort.UnitContractValue;
                            }
                            else
                            {
                                // Spread => 0 (car montant négatif)
                                unitMarginInit = 0;
                            }
                            #endregion Strike Long > Strike Short
                        }
                        #endregion Long expire après Short
                    }
                    else
                    {
                        #region Short expire après Long
                        if (posLong.Asset.StrikePrice == posShort.Asset.StrikePrice)
                        {
                            #region Même strike
                            if (posLong.Quote < posShort.Quote)
                            {
                                // Short Put Time Spread
                                strategyType = CboeStrategyTypeEnum.ShortPutTimeSpread;
                                unitMarginInit = posLong.UnitMarginInit + posShort.UnitMarginInit - posShort.UnitContractValue;
                            }
                            else
                            {
                                unitMarginInit = posLong.UnitMarginInit + posShort.UnitMarginInit - posShort.UnitContractValue;
                            }
                            #endregion Même strike
                        }
                        else
                        {
                            unitMarginInit = posLong.UnitMarginInit + posShort.UnitMarginInit - posShort.UnitContractValue;
                        }
                        #endregion Short expire après Long
                    }
                    #endregion PUT
                }
                //
                // Maintenance
                decimal unitMarginMaint = unitMarginInit;

                if (isSpreadPossible)
                {
                    // EG 20170127 Qty Long To Decimal
                    decimal quantityUsed;
                    // Le spread est possible
                    if (System.Math.Abs(pPosFirst.Quantity) < System.Math.Abs(pPosSecond.Quantity))
                    {
                        quantityUsed = pPosFirst.Quantity;
                    }
                    else
                    {
                        quantityUsed = -1 * pPosSecond.Quantity;
                    }
                    // EG 20170127 Qty Long To Decimal
                    decimal quantityUsedAbs = System.Math.Abs(quantityUsed);
                    //
                    strategy = new CboeStrategyMarginCom
                    {
                        Asset = pPosFirst.Asset,
                        AssetCombined = pPosSecond.Asset,
                        Quantity = quantityUsed,
                        QuantityCombined = -1 * quantityUsed,
                        UnitMarginInit = unitMarginInit,
                        UnitMarginMaint = unitMarginMaint,
                        ContractValue = new Money(pPosFirst.UnitContractValue * quantityUsedAbs, pPosFirst.Asset.Currency),
                        ContractValueCombined = new Money(pPosSecond.UnitContractValue * quantityUsedAbs, pPosSecond.Asset.Currency),
                        // PM 20131129 [19267][19263] Ramener le montant unitaire à 0 lorsqu'il est négatif pour ne pas calculer de crédit de déposit
                        //MarginAmount = new Money(unitMargin * quantityUsedAbs, pPosFirst.Asset.Currency),
                        MarginAmount = new Money(System.Math.Max(0m, unitMarginMaint) * quantityUsedAbs, pPosFirst.Asset.Currency),
                        MarginAmountInit = new Money(System.Math.Max(0m, unitMarginInit) * quantityUsedAbs, pPosFirst.Asset.Currency),
                        MarginAmountMaint = new Money(System.Math.Max(0m, unitMarginMaint) * quantityUsedAbs, pPosFirst.Asset.Currency),
                        // PM 20191025 [24983] Ajout autres types de spread
                        //StrategyTypeEnum = CboeStrategyTypeEnum.Spread,
                        StrategyTypeEnum = strategyType,
                    };
                    //
                    // Mise à jour des quantités
                    if (pPosFirst.Quantity < 0)
                    {
                        pPosFirst.Quantity += quantityUsedAbs;
                        pPosSecond.Quantity -= quantityUsedAbs;
                    }
                    else
                    {
                        pPosFirst.Quantity -= quantityUsedAbs;
                        pPosSecond.Quantity += quantityUsedAbs;
                    }
                }
            }
            return strategy;
        }

        /// <summary>
        /// Tente de réaliser une stratégie comprenant :
        /// des positions vendeuses sur deux assets de PutOrCall différent et de même Maturity et StrikePrice.
        /// Evaluation du déposit pour cette stratégie.
        /// </summary>
        /// <param name="pPosFirst">Position sur le premier asset</param>
        /// <param name="pPosSecond">Position sur le deuxième asset</param>
        /// <returns>Null si la stratégie n'a pas pu être réalisée, sinon un objet CboeStrategyMarginCom représentant la stratégie et son déposit</returns>
        private CboeStrategyMarginCom EvaluateOneStraddle(CboeNormalMarginCom pPosFirst, CboeNormalMarginCom pPosSecond)
        {
            CboeStrategyMarginCom strategy = default;
            // Vérification positions vendeuses ayant PutOrCall différent et même Maturity et StrikePrice
            if ((pPosFirst.Asset.PutOrCall != pPosSecond.Asset.PutOrCall)
                && (pPosFirst.Asset.MaturityYearMonth.CompareTo(pPosSecond.Asset.MaturityYearMonth) == 0)
                && (pPosFirst.Asset.StrikePrice == pPosSecond.Asset.StrikePrice)
                && (pPosFirst.Quantity < 0)
                && (pPosSecond.Quantity < 0))
            {
                // Calcul identique à celui de la Combination (mais prioritaire à celui-ci)
                strategy = EvaluateOneCombination(pPosFirst, pPosSecond);
                if (strategy != default(CboeStrategyMarginCom))
                {
                    strategy.StrategyTypeEnum = CboeStrategyTypeEnum.Straddle;
                }
            }
            return strategy;
        }

        /// <summary>
        /// Tente de réaliser une stratégie comprenant :
        /// des positions vendeuses sur deux assets de PutOrCall différent.
        /// Evaluation du déposit pour cette stratégie.
        /// </summary>
        /// <param name="pPosFirst">Position sur le premier asset</param>
        /// <param name="pPosSecond">Position sur le deuxième asset</param>
        /// <returns>Null si la stratégie n'a pas pu être réalisée, sinon un objet CboeStrategyMarginCom représentant la stratégie et son déposit</returns>
        // PM 20191025 [24983] Ajout Maintenance Margin
        private CboeStrategyMarginCom EvaluateOneCombination(CboeNormalMarginCom pPosFirst, CboeNormalMarginCom pPosSecond)
        {
            CboeStrategyMarginCom strategy = default;
            // Vérification positions vendeuses ayant PutOrCall différent
            if ((pPosFirst.Asset.PutOrCall != pPosSecond.Asset.PutOrCall)
                && (pPosFirst.Quantity < 0)
                && (pPosSecond.Quantity < 0))
            {
                decimal unitMarginInit;

                // Initiale
                if (pPosFirst.UnitMarginInit < pPosSecond.UnitMarginInit)
                {
                    unitMarginInit = pPosSecond.UnitMarginInit + pPosFirst.UnitContractValue;
                }
                else
                {
                    unitMarginInit = pPosFirst.UnitMarginInit + pPosSecond.UnitContractValue;
                }

                // Maintenance
                decimal unitMarginMaint = unitMarginInit;
                // EG 20170127 Qty Long To Decimal
                decimal quantityUsed;

                // Prendre la plus grande des deux quantités négatives
                if (pPosFirst.Quantity < pPosSecond.Quantity)
                {
                    quantityUsed = pPosSecond.Quantity;
                }
                else
                {
                    quantityUsed = pPosFirst.Quantity;
                }

                // Mise à jour des quantités
                pPosFirst.Quantity -= quantityUsed;
                pPosSecond.Quantity -= quantityUsed;

                decimal quantityUsedAbs = System.Math.Abs(quantityUsed);

                strategy = new CboeStrategyMarginCom
                {
                    Asset = pPosFirst.Asset,
                    AssetCombined = pPosSecond.Asset,
                    Quantity = quantityUsed,
                    QuantityCombined = quantityUsed,
                    UnitMarginInit = unitMarginInit,
                    UnitMarginMaint = unitMarginMaint,
                    ContractValue = new Money(pPosFirst.UnitContractValue * quantityUsedAbs, pPosFirst.Asset.Currency),
                    ContractValueCombined = new Money(pPosSecond.UnitContractValue * quantityUsedAbs, pPosSecond.Asset.Currency),
                    // PM 20131129 [19267][19263] Ramener le montant unitaire à 0 lorsqu'il est négatif pour ne pas calculer de crédit de déposit
                    //MarginAmount = new Money(unitMargin * quantityUsedAbs, pPosFirst.Asset.Currency),
                    MarginAmount = new Money(System.Math.Max(0m, unitMarginMaint) * quantityUsedAbs, pPosFirst.Asset.Currency),
                    MarginAmountInit = new Money(System.Math.Max(0m, unitMarginInit) * quantityUsedAbs, pPosFirst.Asset.Currency),
                    MarginAmountMaint = new Money(System.Math.Max(0m, unitMarginMaint) * quantityUsedAbs, pPosFirst.Asset.Currency),
                    StrategyTypeEnum = CboeStrategyTypeEnum.Combination,
                };
            }
            return strategy;
        }

        /// <summary>
        /// Return Strike de pPosA - Strike de pPosB
        /// </summary>
        /// <param name="pPosA"></param>
        /// <param name="pPosB"></param>
        /// <returns></returns>
        // PM 20191025 [24983] New
        private decimal StrikeDifference(CboeNormalMarginCom pPosA, CboeNormalMarginCom pPosB)
        {
            decimal ret = 0;
            if ((pPosA != default(CboeNormalMarginCom)) && (pPosB != default(CboeNormalMarginCom)))
            {
                ret = RiskMethodExtensions.ContractValue(1, pPosA.Asset.StrikePrice, pPosA.Asset.ContractMultiplier, pPosA.Asset.InstrumentNum, pPosA.Asset.InstrumentDen)
                    - RiskMethodExtensions.ContractValue(1, pPosB.Asset.StrikePrice, pPosB.Asset.ContractMultiplier, pPosB.Asset.InstrumentNum, pPosB.Asset.InstrumentDen);
            }
            return ret;
        }

        /// <summary>
        /// Indique si le déposit à considérer est celui de maintenance (True par défaut)
        /// </summary>
        /// <param name="pActorId"></param>
        /// <returns></returns>
        // PM 20191025 [24983] New
        private bool IsMaintenance(int pActorId)
        {
            // PM 20230322 [26282][WI607] Ajout utilisation de MethodParameter.IsMaintenance
            //bool isMaintenance = true;
            bool isMaintenance = MethodParameter.IsMaintenance;
            if (MarginReqOfficeParameters != null)
            {
                if (MarginReqOfficeParameters.ContainsKey(pActorId))
                {
                    MarginReqOfficeParameter specificClearingHouseParam =
                               (from parameter in MarginReqOfficeParameters[pActorId]
                                where parameter.CssId == this.IdCSS
                                select parameter)
                               .FirstOrDefault();
                    //
                    // Test de présence du paramétrage
                    if ((specificClearingHouseParam != default(MarginReqOfficeParameter)) && (specificClearingHouseParam.ActorId != 0))
                    {
                        isMaintenance = specificClearingHouseParam.SpanMaintenanceAmountIndicator;
                    }
                }
            }
            return isMaintenance;
        }

        /// <summary>
        /// Nombre de mois entre 2 dates
        /// </summary>
        /// <param name="pDtDebut"></param>
        /// <param name="pDtFin"></param>
        /// <returns></returns>
        // PM 20230113 [XXXXX][WI538] Correction de la méthode
        private int DiffMonth(DateTime pDtDebut, DateTime pDtFin)
        {
            if (pDtDebut > pDtFin)
            {
                DateTime dtInv = pDtFin;
                pDtFin = pDtDebut;
                pDtDebut = dtInv;
            }

            int nbMonth = 12 * (pDtFin.Year - pDtDebut.Year) + pDtFin.Month - pDtDebut.Month;
            if (pDtDebut.Day > pDtFin.Day)
            {
                nbMonth -= 1;
            }
            return nbMonth;
        }
        #endregion methods
    }
}
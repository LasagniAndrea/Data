using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;

using EfsML.Business;
using EfsML.Enum;

using FixML.Enum;

using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class representing the "TIMS IDEM" Risk method, as used by the CCG italian clearing house (IDEM derivative market)
    /// </summary>
    public sealed partial class TimsIdemMethod : BaseMethod
    {
        #region enum
        /// <summary>
        /// Enum pour les étapes de convertion de contrat et asset pour le déposit de pré-livraison AGREX
        /// </summary>
        internal enum DeliveryStepEnum
        {
            DELIVERY,
            UNCOVEREDDLV,
            MATCHEDDLV,
            MATCHEDWITHDRAW
        }
        #endregion enum

        #region evaluation parameters structures

        private struct MinimumEvaluationParameters
        {
            public RiskMethodQtyType Type;

            public int AssetID;

            public decimal MinimumRate;

            public string Currency;

            public bool CrossMargin;
        }

        private struct PremiumEvaluationParameters
        {
            public int AssetID;

            public decimal StrikePrice;

            public decimal Quote;

            public RiskMethodQtyType Type;

            public decimal UnlQuote;

            public decimal Multiplier;

            public string Currency;
        }

        private struct AdditionalEvaluationParameters
        {
            public int AssetID;

            public string PositionActionIdentifier;

            public PosType PosType;

            public decimal DownSide5;

            public decimal DownSide4;

            public decimal DownSide3;

            public decimal DownSide2;

            public decimal DownSide1;

            public decimal UpSide1;

            public decimal UpSide2;

            public decimal UpSide3;

            public decimal UpSide4;

            public decimal UpSide5;

            public decimal Multiplier;

            public string Currency;

            public bool CrossMargin;
        }

        private struct SpreadEvaluationParameters
        {
            public int AssetID;

            public decimal MaturityYearMonth;

            public decimal SpotSpreadRate;

            public decimal NonSpotSpreadRate;

            public string Currency;

            public string MaturityRuleFrequency;
        }

        private struct MtmEvaluationParameters
        {
            public int AssetID ;

            public RiskMethodQtyType Type;

            public decimal MtmQuote ;

            public decimal CmvQuote;

            public decimal DvpAmount;

            public decimal Multiplier;

            public string Currency;

            public string PositionActionIdentifier;

            public DateTime? MaturityDate;

            public bool CrossMargin;

        }

        #endregion

        #region members
        /// <summary>
        /// Class file parameters set, one element is relative to one ETD contract.
        /// the dictionary key is the pair made by the internal id of the ETD contract (0 for security of the MTA segment) + the contractsymbol.
        /// </summary>
        Dictionary<Pair<int, string>, ClassParameterTimsIdem> m_ClassParameters;

        /// <summary>
        /// Risk array parameters set, one element is relative to one ETD asset.
        /// the dictionary key is the pair made by the type of the risk array element + the internal id of the ETD asset.
        /// </summary>
        Dictionary<Pair<RiskMethodQtyType, int>, RiskArrayParameterTimsIdem> m_RiskArrayParameters;

        /// <summary>
        /// Informations étendues sur les assets
        /// </summary>
        internal List<AssetExpandedParameter> m_AssetExpandedParameters = null;

        /// <summary>
        /// Paramètres de calcul propre au Derivative Contract
        /// </summary>
        internal List<TimsIdemContractParameter> m_TimsIdemContractParameters = null;

        /// <summary>
        /// Convertion d'asset pour la livraison AGREX
        /// </summary>
        internal List<TimsIdemAssetDeliveryParameter> m_AssetDeliveryParameters = null;

        /// <summary>
        /// Pour l'utilisation des méthodes de IProductBase
        /// </summary>
        internal IProductBase m_Product = null;
        #endregion members

        #region override base accessors
        /// <summary>
        /// Get the TimsIdemMethod enum type (<see cref="InitialMarginMethodEnum"/>)
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.TIMS_IDEM; }
        }

        /// <summary>
        /// Requête utilisée pour connaître l'existance de paramètres de risque pour une date donnée
        /// <remarks>Utilise les paramètres DTBUSINESS & SESSIONID</remarks>
        /// </summary>
        /// PM 20150511 [20575] Add QueryExistRiskParameter
        /// PM 20170222 [22881][22942] Rename PARAMSTIMSIDEM_RISKARRAY to IMMARSRISKARRAY_H and add IMMARS_H
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        protected override string QueryExistRiskParameter
        {
            get
            {
                string query = @"select distinct 1
                from dbo.IMMARSRISKARRAY_H ra 
                inner join dbo.IMMARS_H m on (m.IDIMMARS_H = ra.IDIMMARS_H)
                inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = ra.IDASSET)
                where (m.DTBUSINESS = @DTBUSINESS)";
                return query;
            }
        }
        #endregion

        #region constructors
        /// <summary>
        /// Constructeur de la methode de calcul TIMS IDEM
        /// </summary>
        internal TimsIdemMethod()
        {
            m_Product = Tools.GetNewProductBase();
            // PM 20170313 [22833] Ajout alimentation de m_RiskMethodDataType
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }
        #endregion constructors

        #region methods
        #region static methods
        /// <summary>
        /// Aggregate the input collection symbols in one string 
        /// </summary>
        /// <param name="pSymbols">a collection not NULL of contract symbols</param>
        /// <returns>a string containing all the symbols, </returns>
        public static string GroupedSymbol(IEnumerable<string> pSymbols)
        {
            return pSymbols.Aggregate((accumulator, next) => String.Format("{0}.{1}", accumulator, next));
        }
        #endregion static methods

        #region overrided methods
        /// <summary>
        /// Initialize the TIMS method, load and check the parameters for all the contracts in position
        /// </summary>
        /// <param name="pAssetETDCache">collection containing all the assets in position</param>
        /// <param name="pCS">connection string</param>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            if (pAssetETDCache.Count > 0)
            {
                using (IDbConnection connection = DataHelper.OpenConnection(pCS))
                {
                    Dictionary<string, object> dbParametersValue = new Dictionary<string, object>();

                    // PM 20150511 [20575] Ajout gestion dtMarket 
                    //dbParametersValue.Add("DTBUSINESS", this.DtBusiness.Date);
                    DateTime dtBusiness = GetRiskParametersDate(pCS);
                    dbParametersValue.Add("DTBUSINESS", dtBusiness);

                    // PM 20130403 AGREX Chargement des paramètres de gestion de la pré-livraison
                    LoadDeliveryParameters(pCS, pAssetETDCache, connection, dbParametersValue);

                    // PM 20170222 [22881][22942] Charger les Class avant les RiskArray
                    LoadClassParameters(connection, dbParametersValue);

                    // 20120717 MF Ticket 18004 - Using join on IMASSET_ETD table to get the contract
                    LoadRiskArrayParameters(pAssetETDCache, connection, dbParametersValue);

                    //LoadClassParameters(connection, dbParametersValue);
                }
            }
            else
            {
                m_RiskArrayParameters = new Dictionary<Pair<RiskMethodQtyType, int>, RiskArrayParameterTimsIdem>
                    (new PairComparer<RiskMethodQtyType, int>());

                m_ClassParameters = new Dictionary<Pair<int, string>, ClassParameterTimsIdem>
                    (new PairComparer<int, string>());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            m_RiskArrayParameters = null;

            m_ClassParameters = null;
        }

        /// <summary>
        /// Evaluate a deposit item (deposit factor), according with the parameters of the TIMS IDEM method
        /// </summary>
        /// <param name="pActorId">the actor owning the positions set</param>
        /// <param name="pBookId">the book where the positions set has been registered</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="pRiskDataToEvaluate">the positions to evaluate the partial amount for the current deposit item</param>
        /// <param name="opMethodComObj">output value containing all the datas to pass to the calculation sheet repository object
        /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository.BuildTimsIdemMarginCalculationMethod"/>) 
        /// in order to build a margin calculation node (type of <see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
        /// and <see cref="EfsML.Interface.IMarginCalculationMethod"/>)</param>
        /// <returns>the partial amount for the current deposit item</returns>
        /// PM 20160404 [22116] Devient public
        /// FI 20160613 [22256] Modify
        /// FI 20160613 [22256] Add parameter pDepositHierarchyClass
        /// PM 20170313 [22833] Changement de type pour le paramètre pPositionsToEvaluate (=>  RiskData pRiskDataToEvaluate)
        //public override List<Money> EvaluateRiskElementSpecific(
        //    int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass, 
        //    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
        //    out IMarginCalculationMethodCommunicationObject opMethodComObj)
        // EG 20180205 [23769] Refactoring
        // EG 20180307 [23769] Gestion Asynchrone : IsRiskPerformanceGenAsync
        public override List<Money> EvaluateRiskElementSpecific(
            int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
            RiskData pRiskDataToEvaluate,
            out IMarginCalculationMethodCommunicationObject opMethodComObj)
        {
            List<Money> amounts = new List<Money>();

            // PM 20170313 [22833] Prendre uniquement la position (à l'ancien format)
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsToEvaluate = pRiskDataToEvaluate.GetPositionAsEnumerablePair();

            #region Constitution de la position nécessaire au calcul du Déposit TIMS-IDEM
                      
                // Début de constitution de la position NET nécessaire au calcul du Déposit TIMS-IDEM.
                // Cette position inclus les Exercices et Assignations d’options
                //  Remarque: La quantité exercé/assigné sera négatif dans le cas de la quantité exercé est supérieure à celle attribuée
                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedPositionsByIdAsset =
                    PositionsGrouping.GroupPositionsByAsset(positionsToEvaluate);

                // PM 20130528 : Ne garder que les positions dont la quantité est différente de 0
                groupedPositionsByIdAsset =
                    from pos in groupedPositionsByIdAsset
                    where (pos.Second.Quantity != 0) || (pos.Second.ExeAssQuantity != 0)
                    select pos;

                //PM 20141114 [20491] Ajout du calcul des spreads au niveau Contract
                // Recherche des class ayant des positions
                var classTimsFutureInPos = (
                    from pos in groupedPositionsByIdAsset
                    join riskArray in m_RiskArrayParameters.Values on pos.First.idAsset equals riskArray.AssetId
                    join classTims in m_ClassParameters.Values on riskArray.ContractId equals classTims.ContractId
                    where (classTims.Category == "F")
                    select classTims
                    ).Distinct();

                // Recherche du plus petit contract multiplier de chaque ClassGroup ayant des positions
                // PM 20170222 [22881][22942] Recherche uniquement sur les class actives
                var classGroupInPosMinMultiplier =
                    from classTims in classTimsFutureInPos
                    where classTims.IsActive
                    group classTims by classTims.ClassGroup into positionClassGroup
                    select new
                    {
                        ClassGroup = positionClassGroup.Key,
                        MinMultiplier = positionClassGroup.Min(e => e.ClassFileContractMultiplier),
                    };

                // Recherche des contracts dont il serait possible de réunir les positions
                var classWithIsToMerge =
                    from classGroup in classGroupInPosMinMultiplier
                    join classTims in classTimsFutureInPos on classGroup.ClassGroup equals classTims.ClassGroup
                    select new
                    {
                        classGroup.ClassGroup,
                        classTims.ContractId,
                        IsToMerge = (classTims.ClassFileContractMultiplier != classGroup.MinMultiplier)
                        && ((classTims.ClassFileContractMultiplier % classGroup.MinMultiplier) == 0),
                    };

                // Recherche des ClassGroup dont on peux regrouper les positions Future
                var classGroupIsToMerge =
                    from classToTest in classWithIsToMerge
                    group classToTest by classToTest.ClassGroup into classGroup
                    select new
                    {
                        ClassGroup = classGroup.Key,
                        IsToMerge = classGroup.Any(e => e.IsToMerge),
                    };

                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedFuturesPositionsByUnderlying;

                // Definition des critères de regroupement pour le traitement de constitution de la position sur laquelle est calculée les spreads
                // PM 20130422 [18592] Remplacement de UnlAssetId (int) par UnderlyingContractSymbol (string)
                //PM 20141114 [20491] Ajout jointure avec classGroupIsToMerge pour savoir s'il faut regrouper par Contract ou par ClassGroup(Underlying)
                IEnumerable<PositionsGrouping.NettingParameters<string, decimal>> assetParametersXGrouping =
                    from riskArray in this.m_RiskArrayParameters.Values
                    join classTims in this.m_ClassParameters.Values on riskArray.ContractId equals classTims.ContractId
                    join classGroup in classGroupIsToMerge on classTims.ClassGroup equals classGroup.ClassGroup
                    select new PositionsGrouping.NettingParameters<string, decimal>
                    {
                        AssetId = riskArray.AssetId,
                        Type = riskArray.Type,
                        ContractCategory = classTims.Category,
                        Multiplier = classTims.ClassFileContractMultiplier,
                        Maturity = riskArray.MaturityYearMonth,
                        GroupingParameters = new Pair<string, decimal>(classGroup.IsToMerge ? classTims.UnderlyingContractSymbol : classTims.ContractSymbol, riskArray.MaturityYearMonth),
                    };

                // Constitution de la position sur laquelle est calculée les spreads, cette position est constituée des
                // DC de catégorie Future. Sur toutes les positions portant sur un même sous-jacent dans le DC ayant
                // Plusieurs multiplier, on convertira les quantités sur le DC ayant le plus petit multilplier par échéance.
                // Exemple: Cas du FTMIB (multiplier: 5) et du MINI (multilpier: 1) qui sont tous les deux des DC de catégorie
                // Future ayant pour sous-jacent l’indice MIB30.
                // groupedFuturesPositionsByUnderlying : Contient la position sur laquelle sont calculée les spreads
                // PM 20130422 [18592] Passage de int à string pour le 1er type générique
                groupedFuturesPositionsByUnderlying = PositionsGrouping.GroupFuturePositions<string, decimal>
                    (groupedPositionsByIdAsset, assetParametersXGrouping);

                // Rajout à la position NET nécessaire au calcul du Déposit TIMS-IDEM de la position des position action.
                //  (Position action provenant du fichier CCeG-D13A) 
                IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> positionActionActorBook = 
                    GetPositionActions(pActorId, pBookId);

                // Réduction des positions vendeuses d’Option CALL (Short CALL) et/ou  vendeuse de Future par les positions action déposées en garantie. 
                //  Remarque: Cette réduction de position est commune pour toutes les méthodes de calcul de déposit (TIMS-IDEM, TIMS-EUREX, SPAN….) 
                //     et ce règle via un paramètre définit dans le référentiel Marché (Réduction de position) qui peut prendre les valeurs suivante 
                //      (Stock Future, Stock Option, Priorité Stock Future, Priorité Stock Option)
                IEnumerable<CoverageSortParameters> sortParametersForCoverage = GetSortParametersForCoverage(groupedPositionsByIdAsset);

                // FI 20160613 [22256]    
                Pair<IEnumerable<StockCoverageCommunicationObject>, IEnumerable<StockCoverageDetailCommunicationObject>> coveredQuantities = 
                    ReducePosition(pActorId, pBookId, pDepositHierarchyClass, sortParametersForCoverage, ref groupedPositionsByIdAsset);

            #endregion

            // PM 20130403 Convertion de la position pré livraison (AGREX)
            groupedPositionsByIdAsset = ConvertPreDeliveryPosition(groupedPositionsByIdAsset);

            //PM 20140812 Ajout tri de la position (pour rendu dans le log)
            groupedPositionsByIdAsset = from position in groupedPositionsByIdAsset
                                        join asset in m_AssetExpandedParameters on position.First.idAsset equals asset.AssetId
                                        orderby asset.Category, asset.ContractSymbol, asset.PutCall descending, asset.MaturityYearMonth, asset.StrikePrice
                                        select position;

            // Création de l'objet de LOG relatif au calcul du déposit
            TimsIdemMarginCalculationMethodCommunicationObject methodComObj = new TimsIdemMarginCalculationMethodCommunicationObject
            {

                // Set the cross margin activation status
                CrossMarginActivated = base.IsCrossMarginActivated(),
                //PM 20150511 [20575] Ajout date des paramètres de risque
                DtParameters = DtRiskParameters
            };

            #region Chargement de tous les paramètres nécessaires au différent montant à calculer dans la méthode TIMS-IDEM pour la position ouverte considérée

            // Deposit Minimum
            IEnumerable<MinimumEvaluationParameters> minimumEvalParams = 
                    GetMinimumEvaluationParameters(groupedPositionsByIdAsset, positionActionActorBook);

                // Premium margin
                IEnumerable<PremiumEvaluationParameters> premiumEvalParams = 
                    GetPremiumEvaluationParameters(groupedPositionsByIdAsset);

                // Additional Margin
                IEnumerable<AdditionalEvaluationParameters> additionalEvalParams = 
                    GetAdditionalEvaluationParameters(groupedPositionsByIdAsset, positionActionActorBook);

                // Spread Margin
                IEnumerable<SpreadEvaluationParameters> spreadEvalParams =
                    GetSpreadEvaluationParameters(groupedFuturesPositionsByUnderlying);

                // Marked To Marked Margin
                IEnumerable<MtmEvaluationParameters> mtmEvalParams = 
                    GetMtmEvaluationParameters(groupedPositionsByIdAsset, positionActionActorBook);
            
            #endregion 

            // Constitution de la liste des Products/Classes/Contracts nécessaires pour évaluer les positions établies, les remplir avec les positions y afférentes prévus
            methodComObj.Parameters = GetProductClassHierarchy(groupedPositionsByIdAsset, groupedFuturesPositionsByUnderlying, coveredQuantities.First).ToArray();
            // FI 20160613 [22256]  Alimentation de posEquities
            methodComObj.UnderlyingStock = coveredQuantities.Second; 

            #region Calcul du Déposit TIMS-IDEM pour la position ouverte considérée par Product/Classe/Contract
                IEnumerable<TimsIdemProductParameterCommunicationObject> validProductComObj =
                        from productComObj
                            in methodComObj.Parameters
                        where !((IMissingCommunicationObject)productComObj).Missing
                        select (TimsIdemProductParameterCommunicationObject)productComObj;

                #region Calcul du Déposit TIMS-IDEM pour la position ouverte considérée par Product
                foreach (TimsIdemProductParameterCommunicationObject productComObj in validProductComObj)
                {
                    #region Calcul du Déposit TIMS-IDEM pour la position ouverte considérée par Classe
                    foreach (TimsIdemClassParameterCommunicationObject classComObj in productComObj.Parameters)
                    {
                        //PM 20141114 [20491] Ajout du calcul des spreads au niveau Contract
                        bool isClassGroupToMerge = classGroupIsToMerge.Any(c => c.IsToMerge && c.ClassGroup == classComObj.Class);

                        #region Calcul du Déposit TIMS-IDEM pour la position ouverte considérée par Contract
                        // EG 20180205 [23769] New
                        foreach (TimsIdemContractParameterCommunicationObject contractComObj in classComObj.Parameters)
                        {
                            // Calcul par contrat (ASYNCHRONE|SYNCHRONE)
                            if (IsCalculationAsync)
                                EvaluateRiskElementContractByTask(pActorId, pBookId, contractComObj, positionActionActorBook, 
                                    spreadEvalParams, premiumEvalParams, additionalEvalParams, mtmEvalParams, isClassGroupToMerge);
                            else
                                EvaluateRiskElementContract(pActorId, pBookId, contractComObj, positionActionActorBook, 
                                    spreadEvalParams, premiumEvalParams, additionalEvalParams, mtmEvalParams, isClassGroupToMerge);
                        }
                        #endregion

                        // Calcul par classe (ASYNCHRONE|SYNCHRONE)
                        if (IsCalculationAsync)
                            EvaluateRiskElementClassByTask(pActorId, pBookId, classComObj, positionActionActorBook, 
                                spreadEvalParams, minimumEvalParams, isClassGroupToMerge);
                        else
                            EvaluateRiskElementClass(pActorId, pBookId, classComObj, positionActionActorBook, 
                                spreadEvalParams, minimumEvalParams, isClassGroupToMerge);

                    }
                    #endregion

                    // Calcul par produit (ASYNCHRONE|SYNCHRONE)
                    if (IsCalculationAsync)
                        EvaluateRiskElementProductByTask(pActorId, pBookId, productComObj, ref amounts);
                    else
                        EvaluateRiskElementProduct(pActorId, pBookId, productComObj, ref amounts);
                }
                #endregion

            #endregion

            // Dans le cas ou a aucun montant  de déposit de calculé, on force un montant à 0.00 EUR
            //  de manière à avoir la création d’un trade MRO avec un montant à zéro
            if (amounts.Count <= 0)
            {
                amounts.Add(new Money(0, "EUR"));
            }

            opMethodComObj = methodComObj;

            return amounts;
        }

        /// <summary>
        /// Round the given amount with a 2 decimals precision and rounding rule half Down
        /// </summary>
        /// <param name="pAmount"></param>
        /// <returns></returns>
        protected override decimal RoundAmount(decimal pAmount)
        {
            //return base.RoundAmount(pAmount);

            // Essay pour faire l'arrondi half up

            //return System.Math.Round(pAmount, 2, MidpointRounding.AwayFromZero);

            // PM 20130530 Passage en arrondi Nearest car présente moins de différence de centimes qu'avec HalfDown
            //EFS_Round round = new EFS_Round(FpML.Enum.RoundingDirectionEnum.HalfDown, 2, pAmount);
            EFS_Round round = new EFS_Round(FpML.Enum.RoundingDirectionEnum.Nearest, 2, pAmount);

            pAmount = round.AmountRounded;

            return pAmount;
        }

        /// <summary>
        /// Get a collection of sorting parameter needed by coverage strategies
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">Positions of the current risk element</param>
        /// <returns>A collection of sorting parameters in order to be used inside of the ReducePosition method</returns>
        protected override IEnumerable<CoverageSortParameters> GetSortParametersForCoverage(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            IEnumerable<CoverageSortParameters> sortParameters =
             from riskArray in m_RiskArrayParameters
             where
                 // all the types could cover
                 riskArray.Key.First == RiskMethodQtyType.Call
                 || riskArray.Key.First == RiskMethodQtyType.Future
                 || riskArray.Key.First == RiskMethodQtyType.ExeAssCall
                 || riskArray.Key.First == RiskMethodQtyType.FutureMoff
             join classParam in m_ClassParameters on riskArray.Value.ContractId equals classParam.Key.First
             // PM 20130926 [18998] Ajout jointure avec la position pour limiter le jeu de résultat
             join pos in pGroupedPositionsByIdAsset on riskArray.Key.Second equals pos.First.idAsset
             where classParam.Key.First != 0
             select new CoverageSortParameters
             {
                 AssetId = riskArray.Key.Second,
                 ContractId = classParam.Key.First,
                 Type = riskArray.Key.First,
                 // PM 20130926 [18998] Pour les Short Call en livraison, on ne prend pas le cours, mais le strike - le cours du sous-jacent
                 //Quote = riskArray.Value.RiskArrayQuote,
                 Quote = (riskArray.Key.First == RiskMethodQtyType.ExeAssCall) ? System.Math.Max(0, classParam.Value.AssetUnlQuote - riskArray.Value.StrikePrice) : riskArray.Value.RiskArrayQuote,
                 MaturityYearMonth = riskArray.Value.MaturityYearMonth,
                 // PM 20130926 [18998] prendre StrikePrice et pas RiskArrayStrikePrice
                 //StrikePrice = riskArray.Value.RiskArrayStrikePrice,
                 StrikePrice = riskArray.Value.StrikePrice,
                 Multiplier = classParam.Value.ClassFileContractMultiplier,
             };

            return sortParameters;
        }
        #endregion ovverrided methods

        #region EvaluateRiskElementContract
        /// <summary>
        /// Calcul du Déposit TIMS-IDEM pour la position ouverte considérée par Contract
        /// </summary>
        /// <param name="pActorId">Acteur en position</param>
        /// <param name="pBookId">Book en position</param>
        /// <param name="pContractComObj">Contrat</param>
        /// <param name="pPositionActionActorBook">position NET nécessaire au calcul du Déposit de la position des positions action</param>
        /// <param name="pSpreadEvalParams">Spread margin parameters</param>
        /// <param name="pPremiumEvalParams">Premium margin parameters</param>
        /// <param name="pAdditionalEvalParams">Additional margin parameters</param>
        /// <param name="pMTMEvalParams">MTM margin parameters</param>
        /// <param name="pIsClassGroupToMerge">Indicateur de regroupement des positions Future</param>
        // EG 20180205 [23769] New
        private void EvaluateRiskElementContract(int pActorId, int pBookId, TimsIdemContractParameterCommunicationObject pContractComObj,
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pPositionActionActorBook,
            IEnumerable<SpreadEvaluationParameters> pSpreadEvalParams,
            IEnumerable<PremiumEvaluationParameters> pPremiumEvalParams,
            IEnumerable<AdditionalEvaluationParameters> pAdditionalEvalParams,
            IEnumerable<MtmEvaluationParameters> pMTMEvalParams,
            bool pIsClassGroupToMerge)
        {
            string key = String.Format("(ActorId: {0} BookId: {1} Contract: {2})", pActorId, pBookId, pContractComObj.Contract);

            AppInstance.TraceManager.TraceVerbose(this, String.Format("START EvaluateRiskElementContract {0}", key));

            // Calcul du Futures Spread Margin par Contract
            if (false == pIsClassGroupToMerge)
                pContractComObj.Spread = GetSpreadMargin(pContractComObj.SpreadPositions, pSpreadEvalParams);

            // Calcul du Premium Margin par Contract
            pContractComObj.Premium = GetPremiumMargin(pContractComObj.Positions, pPremiumEvalParams);

            // Calcul de l'Additional Margin par Contract
            // Calcul des montants Additional, une fois sans Offset, une fois avec Offset

            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> positionActionContract =
                from position in pPositionActionActorBook
                where position.First.derivativeContractSymbol == pContractComObj.Contract
                select position;

            pContractComObj.Additional = GetAdditionalMargin(pContractComObj.Positions, positionActionContract, pAdditionalEvalParams, null);

            pContractComObj.AdditionalWithOffset = GetAdditionalMargin(pContractComObj.Positions, positionActionContract, pAdditionalEvalParams, pContractComObj.Offset);

            // Calcul du Mark to Market Margin for unsettled stock futures contracts par Contract
            pContractComObj.Mtm = GetMtmMargin(pContractComObj.Positions, positionActionContract, pMTMEvalParams);

            AppInstance.TraceManager.TraceVerbose(this, String.Format("STOP EvaluateRiskElementContract {0}", key));
        }
        #endregion EvaluateRiskElementContract
        #region EvaluateRiskElementClass
        /// <summary>
        /// Calcul du Déposit TIMS-IDEM pour la position ouverte considérée par Classe
        /// </summary>
        /// <param name="pActorId">Acteur en position</param>
        /// <param name="pBookId">Book en position</param>
        /// <param name="pClassComObj">Classe</param>
        /// <param name="pPositionActionActorBook">position NET nécessaire au calcul du Déposit de la position des positions action</param>
        /// <param name="pSpreadEvalParams">Spread Margin parameters</param>
        /// <param name="pMinimumEvalParams">Deposit Minimum parameters</param>
        /// <param name="pIsClassGroupToMerge">Indicateur de regroupement des positions Future</param>
        // EG 20180205 [23769] New
        private void EvaluateRiskElementClass(int pActorId, int pBookId, TimsIdemClassParameterCommunicationObject pClassComObj,
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pPositionActionActorBook,
            IEnumerable<SpreadEvaluationParameters> pSpreadEvalParams, IEnumerable<MinimumEvaluationParameters> pMinimumEvalParams, bool pIsClassGroupToMerge)
        {
            string key = String.Format("(ActorId: {0} BookId: {1} Class: {2})", pActorId, pBookId, pClassComObj.Class);

            AppInstance.TraceManager.TraceVerbose(this, String.Format("START EvaluateRiskElementClass {0}", key));

            // Calcul du Futures Spread Margin par Class
            if (pIsClassGroupToMerge)
                pClassComObj.Spread = GetSpreadMargin(pClassComObj.SpreadPositions, pSpreadEvalParams);
            else
                pClassComObj.Spread = AggregateMargin(
                    from parameter in pClassComObj.Parameters
                    select ((TimsIdemContractParameterCommunicationObject)parameter).Spread);

            // Cumul des Mark to Market Margin for unsettled stock futures contracts par Class
            pClassComObj.Mtm = AggregateMargin(
                from parameter in pClassComObj.Parameters
                select ((TimsIdemContractParameterCommunicationObject)parameter).Mtm);

            // Cumul des Premium Margin par Class
            pClassComObj.Premium = AggregateMargin(from parameter in pClassComObj.Parameters
                                                   select ((TimsIdemContractParameterCommunicationObject)parameter).Premium);
            // Cumul des Additional Margin par Class
            // Sommer uniquement les matrices et pas les montants
            // Gestion offset sur la somme des riskarrays des contracts de la class
            IEnumerable<TimsDecomposableParameterCommunicationObject> additional =
                from parameter in pClassComObj.Parameters
                select ((TimsIdemContractParameterCommunicationObject)parameter).Additional;

            IEnumerable<TimsDecomposableParameterCommunicationObject> additionalWithOffset =
                from parameter in pClassComObj.Parameters
                select ((TimsIdemContractParameterCommunicationObject)parameter).AdditionalWithOffset;

            pClassComObj.Additional = AggregateClassAdditionalMargin(additional, additionalWithOffset);

            // Calcul du Deposit Minimum par Class
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> positionActionClass =
                    from position in pPositionActionActorBook
                    join contractComObj in pClassComObj.Parameters
                    on position.First.derivativeContractSymbol
                    equals ((TimsIdemContractParameterCommunicationObject)contractComObj).Contract
                    select position;

            pClassComObj.Minimum = GetMinimumMargin(
                pClassComObj.Positions, positionActionClass, pMinimumEvalParams,
                pClassComObj.Premium?.MarginAmount);

            AppInstance.TraceManager.TraceVerbose(this, String.Format("STOP EvaluateRiskElementClass {0}", key));
        }
        #endregion EvaluateRiskElementClass
        #region EvaluateRiskElementProduct
        /// <summary>
        /// Calcul du Déposit TIMS-IDEM pour la position ouverte considérée par Produit
        /// </summary>
        /// <param name="pActorId">Acteur en position</param>
        /// <param name="pBookId">Book en position</param>
        /// <param name="pProductComObj">Produit</param>
        /// <param name="pAmounts">Liste des montants calculés</param>
        // EG 20180205 [23769] New
        private void EvaluateRiskElementProduct(int pActorId, int pBookId, TimsIdemProductParameterCommunicationObject pProductComObj, ref List<Money> pAmounts)
        {
            string key = String.Format("(ActorId: {0} BookId: {1} Product: {2})", pActorId, pBookId, pProductComObj.Product);

            AppInstance.TraceManager.TraceVerbose(this, String.Format("START EvaluateRiskElementProduct {0}", key));

            // Cumul des Spread Margin par Product
            pProductComObj.Spread = AggregateMargin(
                from parameter in pProductComObj.Parameters
                select ((TimsIdemClassParameterCommunicationObject)parameter).Spread);

            // Cumul des Mark to Market Margin for unsettled stock futures contracts par Product
            pProductComObj.Mtm = AggregateMargin(
                from parameter in pProductComObj.Parameters
                select ((TimsIdemClassParameterCommunicationObject)parameter).Mtm);

            // Cumul des Premium Margin par Product
            pProductComObj.Premium = AggregateMargin(
                from parameter in pProductComObj.Parameters
                select ((TimsIdemClassParameterCommunicationObject)parameter).Premium);

            // Cumul des Additional Margin par Product
            pProductComObj.Additional = AggregateAdditionalMargin(
                from parameter in pProductComObj.Parameters
                select ((TimsIdemClassParameterCommunicationObject)parameter).Additional);

            // Cumul des Deposit Minimum par Product
            pProductComObj.Minimum = AggregateMargin(
                from parameter in pProductComObj.Parameters
                select ((TimsIdemClassParameterCommunicationObject)parameter).Minimum);

            // Calcul du deposit final par Product
            pProductComObj.MarginAmount = GetProductMarginAmount(
                    pProductComObj.Mtm?.MarginAmount,
                    pProductComObj.Spread?.MarginAmount,
                    pProductComObj.Premium?.MarginAmount,
                    pProductComObj.Additional?.MarginAmount,
                    pProductComObj.Minimum?.MarginAmount);

            SumAmounts(new Money[] { (Money)pProductComObj.MarginAmount }, ref pAmounts);

            AppInstance.TraceManager.TraceVerbose(this, String.Format("STOP EvaluateRiskElementProduct {0}", key));
        }
        #endregion EvaluateRiskElementProduct

        /// <summary>
        /// Gestion livraison AGREX
        /// Insertion des assets de calcul du déposit de livraison
        /// </summary>
        /// <param name="pConnection">Connection</param>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        private void InsertImAssetEtdForDelivery(IDbConnection pConnection)
        {
            string queryInsert = DataContractHelper.GetQuery(DataContractResultSets.INSERTIMASSETETD_TIMSIDEMMETHOD);
            CommandType queryType = DataContractHelper.GetType(DataContractResultSets.INSERTIMASSETETD_TIMSIDEMMETHOD);
            Dictionary<string, object> dbParameterValues = new Dictionary<string, object>();
            DataHelper.ExecuteNonQuery(pConnection, queryType, queryInsert,
                DataContractHelper.GetDbDataParameters(DataContractResultSets.INSERTIMASSETETD_TIMSIDEMMETHOD, dbParameterValues));
        }

        /// <summary>
        /// Construction et chargement des paramètres de gestion de la pré-livraison
        /// </summary>
        /// <param name="pCS">Chaine de connection</param>
        /// <param name="pAssetETDCache">Dictionnaire des assets en position</param>
        /// <param name="pConnection">DB Connection</param>
        /// <param name="pDbParametersValue">Parameters de chargement des paramètres</param>
        private void LoadDeliveryParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache, IDbConnection pConnection, Dictionary<string, object> pDbParametersValue)
        {
            // PM 20130327 AGREX
            // Insertion des assets de calcul du déposit de livraison AGREX
            InsertImAssetEtdForDelivery(pConnection);

            // PM 20130327 AGREX
            // Lecture des paramètres de convertion d'asset pour la livraison AGREX
            m_AssetDeliveryParameters = LoadParametersMethod<TimsIdemAssetDeliveryParameter>.LoadParameters(pConnection, pDbParametersValue, DataContractResultSets.ASSETDELIVERY_TIMSIDEMMETHOD);
            m_AssetDeliveryParameters.ForEach(a => { if (a.ApplyEndDate < a.ApplyStartDate) a.ApplyEndDate = a.ApplyStartDate; });

            // PM 20130318 AGREX
            // Lecture des informations sur les assets en position
            m_AssetExpandedParameters = LoadParametersAssetExpanded(pConnection);

            // Ajout des Assets a convertir pour gérer la livraison AGREX
            foreach (TimsIdemAssetDeliveryParameter assetNew in m_AssetDeliveryParameters)
            {
                int idAsset = assetNew.NewAssetId;
                if (!pAssetETDCache.ContainsKey(idAsset))
                {
                    SQL_AssetETD asset = new SQL_AssetETD(pCS, idAsset);
                    pAssetETDCache.Add(idAsset, asset);
                }
            }

            // PM 20130322 AGREX
            // Lecture des paramètres de calcul propre au DC en position utilisant TIMS IDEM
            m_TimsIdemContractParameters = LoadParametersMethod<TimsIdemContractParameter>.LoadParameters(pConnection, pDbParametersValue, DataContractResultSets.CONTRACTPARAM_TIMSIDEMMETHOD);

            // PM 20130329 AGREX
            // Mise à jour des dates de début et fin d'application des déposit pré-livraison
            var dateToCompute =
                from contract in m_TimsIdemContractParameters
                join assetDly in m_AssetDeliveryParameters on contract.ContractId equals assetDly.ContractId
                group assetDly by contract into contractDly
                select contractDly;

            foreach (var contract in dateToCompute)
            {
                // Lire le marché et son BC
                int marketId = contract.Key.MarketId;
                string idBC = MarketParameters[marketId].BusinessCenter;
                // Gestion pré-livraison avant échéance
                var assetDelivery = contract.Where(a => (a.NewContractId == contract.Key.DeliveryContractId) && (a.DeliveryStep == DeliveryStepEnum.DELIVERY.ToString()));
                if ((assetDelivery.Count() > 0) && (contract.Key.DeliveryMarginOffsetMultiplier < 0))
                {
                    IOffset offset = m_Product.CreateOffset(contract.Key.DeliveryMarginOffsetEnum, contract.Key.DeliveryMarginOffsetMultiplier, contract.Key.DeliveryMarginOffsetDayTypeEnum);
                    IBusinessDayAdjustments bda = m_Product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.PRECEDING, idBC);
                    foreach (var asset in assetDelivery)
                    {
                        asset.ApplyEndDate = asset.ApplyStartDate;
                        asset.ApplyStartDate = Tools.ApplyOffset(pCS, asset.ApplyStartDate, offset, bda, null);
                    }
                }
                // Gestion pré-livraison non couverte avant échéance
                var assetUncoveredDelivery = contract.Where(a => (a.NewContractId == contract.Key.UncoveredDeliveryContractId) && (a.DeliveryStep == DeliveryStepEnum.UNCOVEREDDLV.ToString()));
                if ((assetUncoveredDelivery.Count() > 0)
                    && (contract.Key.IncreaseMarginOffsetMultiplier < 0)
                    && (contract.Key.IncreaseMarginOffsetMultiplier > contract.Key.DeliveryMarginOffsetMultiplier))
                {
                    IOffset offset = m_Product.CreateOffset(contract.Key.IncreaseMarginOffsetEnum, contract.Key.IncreaseMarginOffsetMultiplier, contract.Key.IncreaseyMarginOffsetDayTypeEnum);
                    IBusinessDayAdjustments bda = m_Product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.PRECEDING, idBC);
                    foreach (var asset in assetUncoveredDelivery)
                    {
                        asset.ApplyEndDate = asset.ApplyStartDate;
                        asset.ApplyStartDate = Tools.ApplyOffset(pCS, asset.ApplyStartDate, offset, bda, null);
                    }
                }
            }
            // Ne garder que les données pouvant s'appliquer à la date de bourse en cours
            m_AssetDeliveryParameters = m_AssetDeliveryParameters.Where(a => (a.ApplyStartDate <= DtBusiness) && (a.ApplyEndDate >= DtBusiness)).ToList();

        }

        // 20120717 MF Ticket 18004, pContractsId deleted
        private void LoadClassParameters(IDbConnection pConnection, Dictionary<string, object> pDbParametersValue)
        {
            // 1. Load class parameters

            CommandType classCmdTyp = DataContractHelper.GetType(DataContractResultSets.CLASS_TIMSIDEMMETHOD);

            string classParametersRequest =
                DataHelper<object>.IsNullTransform(
                    pConnection,
                    classCmdTyp,
                    DataContractHelper.GetQuery(DataContractResultSets.CLASS_TIMSIDEMMETHOD)
                );

            IEnumerable<ClassParameterTimsIdem> classParameters =
                DataHelper<ClassParameterTimsIdem>.ExecuteDataSet(
                    pConnection,
                    classCmdTyp,
                    classParametersRequest,
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.CLASS_TIMSIDEMMETHOD, pDbParametersValue)
                );

            // PM 20170222 [22881][22942] Ne prendre que les Class des Futures et Options
            classParameters = classParameters.Where(c => (("F" == c.Category) || ("O" == c.Category)));

            // 2. transform the contract/class collection in dictionary

            m_ClassParameters = classParameters.ToDictionary(
                obj => new Pair<int, string> { First = obj.ContractId, Second = obj.ContractSymbol }, 
                new PairComparer<int,string>());
        }

        // 20120717 MF Ticket 18004, pContractsId deleted
        private void LoadRiskArrayParameters(Dictionary<int, SQL_AssetETD> pAssetETDCache,
            IDbConnection connection, Dictionary<string, object> dbParametersValue)
        {
            // 1. load risk array parameters

            string riskArrayParametersRequest =
                    DataContractHelper.GetQuery(DataContractResultSets.RISKARRAY_TIMSIDEMMETHOD);

            IEnumerable<RiskArrayParameterTimsIdem> riskArrayParameters =
                DataHelper<RiskArrayParameterTimsIdem>.ExecuteDataSet(
                    connection,
                    DataContractHelper.GetType(DataContractResultSets.RISKARRAY_TIMSIDEMMETHOD),
                    riskArrayParametersRequest,
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.RISKARRAY_TIMSIDEMMETHOD, dbParametersValue)
                );

            // PM 20170222 [22881][22942] Ne prendre les RiskArray des Futures et Options
            riskArrayParameters = riskArrayParameters.Where(r => (("F" == r.Category) || ("O" == r.Category)));

            // 2. get asset specific parameter from the asset input collection and build special "stock futures"/"option underlying" parameters

            //2.1 complete ordinary parameters  initialization
            foreach (RiskArrayParameterTimsIdem riskArrayOrdinaryParameter in
                (from elem in riskArrayParameters where elem.AssetId != 0 select elem))
            {
                AddInfo(pAssetETDCache, riskArrayOrdinaryParameter, false);
            }

            //2.2 complete security parameter (position action) initialization

            int idSecurityAssetId = 1;

            foreach (RiskArrayParameterTimsIdem riskArraySecurityParameter in
                (from elem in riskArrayParameters where elem.AssetId == 0 && elem.ContractId == 0 select elem))
            {
                RiskArrayParameterTimsIdem relatedETDParameter = 
                    (from elem in riskArrayParameters
                     where elem.AssetId != 0
                         && elem.ContractSymbol == riskArraySecurityParameter.ContractSymbol
                     select elem).FirstOrDefault();

                riskArraySecurityParameter.AssetId = idSecurityAssetId++;
                riskArraySecurityParameter.Type = GetRiskArrayTypeFromCategory(riskArraySecurityParameter.Category, false, null);
                riskArraySecurityParameter.CrossMarginActivated = 
                    relatedETDParameter != null && relatedETDParameter.CrossMarginActivated;
            }

            //2.3 build additional parameters for stock futures and for option exercises/assignements 

            IEnumerable<RiskArrayParameterTimsIdem> toAddRiskArrayParameter = null;
            List<RiskArrayParameterTimsIdem> toRemoveRiskArrayParameter = null;

            // Stock futures parameters (as well as underlying parameters) 
            //  present no maturity values inside them own risk array, the single parameter
            //  found inside of the market file has to be copied for all the others assets making part of the same future/option contract
            // PM 20130529 Sortie de la requête Linq hors du foreach
            IEnumerable<RiskArrayParameterTimsIdem> allRiskArraySpecialParameter = from elem in riskArrayParameters where elem.AssetId == 0 && elem.ContractId != 0 select elem;
            foreach (RiskArrayParameterTimsIdem riskArraySpecialParameter in allRiskArraySpecialParameter)
            {

                CloneSpecialParameters(pAssetETDCache, riskArraySpecialParameter,
                    ref toAddRiskArrayParameter, ref toRemoveRiskArrayParameter);
            }

            if (toAddRiskArrayParameter != null)
            {
                riskArrayParameters = riskArrayParameters.Union(toAddRiskArrayParameter);

                riskArrayParameters = riskArrayParameters.Except(toRemoveRiskArrayParameter);
            }

            // Debug: vérifier s'il y'a des doublons dans la liste riskArrayParameters
            //var duplicatePrameters = riskArrayParameters.GroupBy(key => new { key.AssetId, key.Type })
            //    .Select(x => new
            //    {
            //        Count = x.Count(),
            //        IdAsset = x.Key.AssetId,
            //        Category = x.Key.Type
            //    })
            //    .OrderByDescending(x => x.Count);

            // RD 20130222 / Bug: Supprimer les doublons de la liste riskArrayParameters
            List<RiskArrayParameterTimsIdem> noDuplicateRiskArrayParameters = new List<RiskArrayParameterTimsIdem>();
            foreach (RiskArrayParameterTimsIdem riskArraySpecialParameter in riskArrayParameters)
            {
                List<RiskArrayParameterTimsIdem> riskArray = (from elem in noDuplicateRiskArrayParameters
                                                              where elem.AssetId == riskArraySpecialParameter.AssetId &&
                                                              elem.Type == riskArraySpecialParameter.Type
                                                              select elem).ToList();

                if (riskArray.Count() == 0)
                    noDuplicateRiskArrayParameters.Add(riskArraySpecialParameter);
            }

            // 3. transform the asset collection in dictionary

            m_RiskArrayParameters = noDuplicateRiskArrayParameters.ToDictionary(
                obj => new Pair<RiskMethodQtyType, int> { First = obj.Type, Second = obj.AssetId },
                new PairComparer<RiskMethodQtyType, int>());
        }

        private void AddInfo(Dictionary<int, SQL_AssetETD> pAssetETDCache, RiskArrayParameterTimsIdem riskArrayParameter, bool pExeAssMof)
        {

            riskArrayParameter.StrikePrice = pAssetETDCache[riskArrayParameter.AssetId].StrikePrice;

            if (riskArrayParameter.MaturityYearMonth == 0)
            {
                riskArrayParameter.MaturityYearMonth = decimal.Parse(pAssetETDCache[riskArrayParameter.AssetId].Maturity_MaturityMonthYear);
            }

            riskArrayParameter.Type = 
                GetRiskArrayTypeFromCategory(riskArrayParameter.Category, pExeAssMof, pAssetETDCache[riskArrayParameter.AssetId].PutCall);

            riskArrayParameter.CrossMarginActivated = this.MarketParameters[pAssetETDCache[riskArrayParameter.AssetId].IdM].CrossMarginActivated;
                
        }

        private static RiskMethodQtyType GetRiskArrayTypeFromCategory(string pCategory, bool pExeAssMof, string pPutCall)
        {
            RiskMethodQtyType type = default;

            switch (pCategory)
            {
                case "F":

                    type = pExeAssMof ? RiskMethodQtyType.FutureMoff : RiskMethodQtyType.Future;

                    break;

                case "O":

                    if (pPutCall == "1")
                    {
                        type = pExeAssMof ? RiskMethodQtyType.ExeAssCall : RiskMethodQtyType.Call;
                    }

                    if (pPutCall == "0")
                    {
                        type = pExeAssMof ? RiskMethodQtyType.ExeAssPut : RiskMethodQtyType.Put;
                    }

                    break;

                case "C":

                    type = RiskMethodQtyType.PositionAction;

                    break;
            }

            return type;
        }

        /// <summary>
        /// Clone the input parameter (a "Special" parameter having no asset id attached, valid for stock futures and underlying parameters).
        /// The input parameter has to be copied for all the others assets making part of the same future/option contract.
        /// </summary>
        /// <param name="pAssetETDCache">assets set to get all the assets related to the same contract than the input parameter</param>
        /// <param name="riskArraySpecialParameter">Special parameter, asset id == 0</param>
        /// <param name="toAddRiskArrayParameters">Resulting set of cloned parameters</param>
        /// <param name="toRemoveRiskArrayParameters"></param>
        private void CloneSpecialParameters
            (Dictionary<int, SQL_AssetETD> pAssetETDCache, RiskArrayParameterTimsIdem riskArraySpecialParameter,
            ref IEnumerable<RiskArrayParameterTimsIdem> toAddRiskArrayParameters,
            ref List<RiskArrayParameterTimsIdem> toRemoveRiskArrayParameters)
        {
            // PM 20130529 Ajout restriction sur la Category (pour ne garder que les Options)
            //IEnumerable<RiskArrayParameterTimsIdem> clonedParameters =  
            //    from asset in pAssetETDCache
            //    where
            //        asset.Value.IdDerivativeContract == riskArraySpecialParameter.ContractId
            //    select riskArraySpecialParameter.Clone(asset.Key, riskArraySpecialParameter.ContractId);
            // PM 20170222 [22881][22942] Modification du clonnage pour les Options afin de tenir compte des contrats devenus inatifs
            //IEnumerable<RiskArrayParameterTimsIdem> clonedParameters =
            //    from asset in pAssetETDCache
            //    where ((asset.Value.IdDerivativeContract == riskArraySpecialParameter.ContractId)
            //        && (asset.Value.DrvContract_Category == riskArraySpecialParameter.Category))
            //    select riskArraySpecialParameter.Clone(asset.Key, riskArraySpecialParameter.ContractId);

            // PM 20170222 [22881][22942] Rechercher les Class concernées par le clonnage
            ClassParameterTimsIdem clonedClass = (
                from classParams in m_ClassParameters
                where (classParams.Value.ContractSymbol == riskArraySpecialParameter.ContractSymbol)
                    && (classParams.Value.Category == riskArraySpecialParameter.Category)
                    && (classParams.Value.IsActive == riskArraySpecialParameter.IsActive)
                select classParams.Value).FirstOrDefault();

            IEnumerable<RiskArrayParameterTimsIdem> clonedDlvParameters;
            List<RiskArrayParameterTimsIdem> clonedFutureParameters;
            if (clonedClass != default(ClassParameterTimsIdem))
            {
                // PM 20170222 [22881][22942] Clonner les RiskArray pour tous les assets portant sur le contrat de la Class
                clonedDlvParameters =
                      from asset in pAssetETDCache
                      where (asset.Value.IdDerivativeContract == clonedClass.ContractId)
                        && ("O" == asset.Value.DrvContract_Category)
                      select riskArraySpecialParameter.Clone(asset.Key, asset.Value.IdDerivativeContract);

                clonedFutureParameters = (
                      from asset in pAssetETDCache
                      where (asset.Value.IdDerivativeContract == clonedClass.ContractId)
                        && ("F" == asset.Value.DrvContract_Category)
                      select riskArraySpecialParameter.Clone(asset.Key, asset.Value.IdDerivativeContract)
                      ).ToList();

                // Enrichir les paramètres pour les Futures clonés
                foreach (RiskArrayParameterTimsIdem parameter in clonedFutureParameters)
                {
                    AddInfo(pAssetETDCache, parameter, false);
                }
            }
            else
            {
                clonedDlvParameters = new List<RiskArrayParameterTimsIdem>();
                clonedFutureParameters = new List<RiskArrayParameterTimsIdem>();
            }

            if (riskArraySpecialParameter.Category == "O")
            {
                // PM 20130529 Correction de la requête générant les paramètres de risque des Futures en Livraison
                // 
                //IEnumerable<RiskArrayParameterTimsIdem> clonedOptionParametersForFutures = (
                //    from asset in pAssetETDCache
                //    where
                //        asset.Value.IdDerivativeContract == riskArraySpecialParameter.ContractId
                //    join assetFuture in pAssetETDCache on asset.Value.DrvContract_Symbol equals assetFuture.Value.DrvContract_Symbol
                //    where
                //        assetFuture.Value.DrvContract_Category == "F"
                //        && 
                //        (assetFuture.Value.Maturity_MaturityDate == null 
                //        || assetFuture.Value.Maturity_MaturityDate.InNumericYearMonth() <= DtBusiness.InNumericYearMonth()) 
                //    select riskArraySpecialParameter.Clone
                //    (assetFuture.Key, assetFuture.Value.IdDerivativeContract, assetFuture.Value.DrvContract_Category)
                //    );
                // PM 20170222 [22881][22942] Modification du clonnage pour les Futures afin de tenir compte des contrats devenus inatifs
                //IEnumerable<RiskArrayParameterTimsIdem> clonedOptionParametersForFutures = (
                //    from asset in pAssetETDCache
                //    where ((asset.Value.DrvContract_Symbol == riskArraySpecialParameter.ContractSymbol)
                //        && (asset.Value.DrvContract_Category != riskArraySpecialParameter.Category))
                //    join assetFuture in pAssetETDCache on asset.Value.DrvContract_Symbol equals assetFuture.Value.DrvContract_Symbol
                //    where (assetFuture.Value.DrvContract_Category == "F")
                //       && ((assetFuture.Value.Maturity_MaturityDate == null)
                //        || (assetFuture.Value.Maturity_MaturityDate.InNumericYearMonth() <= DtBusiness.InNumericYearMonth()))
                //    group assetFuture by  new { assetFuture.Key, assetFuture.Value.IdDerivativeContract, assetFuture.Value.DrvContract_Category }
                //    into newRiskArrayKey
                //        select riskArraySpecialParameter.Clone
                //        (newRiskArrayKey.Key.Key, newRiskArrayKey.Key.IdDerivativeContract, newRiskArrayKey.Key.DrvContract_Category)
                //    );

                //clonedParameters = clonedParameters.Union(clonedOptionParametersForFutures);

                ClassParameterTimsIdem dlvFutureClass = (
                    from classParams in m_ClassParameters
                    where (classParams.Value.ContractSymbol == riskArraySpecialParameter.ContractSymbol)
                    && (classParams.Value.UnlIsinCode == riskArraySpecialParameter.UnlIsinCode)
                    && (classParams.Value.IsActive == riskArraySpecialParameter.IsActive)
                    && (classParams.Value.Category != riskArraySpecialParameter.Category)
                    && (classParams.Value.Category == "F")
                    select classParams.Value).FirstOrDefault();

                if (dlvFutureClass != default(ClassParameterTimsIdem))
                {
                    IEnumerable<RiskArrayParameterTimsIdem> clonedOptionParametersForDlvFutures = (
                        from assetFuture in pAssetETDCache
                        where (assetFuture.Value.IdDerivativeContract == dlvFutureClass.ContractId)
                            && ((assetFuture.Value.Maturity_MaturityDate == null)
                            || (assetFuture.Value.Maturity_MaturityDate.InNumericYearMonth() <= DtBusiness.InNumericYearMonth()))
                        group assetFuture by new { assetFuture.Key, assetFuture.Value.IdDerivativeContract, assetFuture.Value.DrvContract_Category }
                            into newRiskArrayKey
                            select riskArraySpecialParameter.Clone
                            (newRiskArrayKey.Key.Key, newRiskArrayKey.Key.IdDerivativeContract, newRiskArrayKey.Key.DrvContract_Category)
                        );

                    // Union des paramètes clonés pour les Futures en livraison à ceux des options en livraison
                    clonedDlvParameters = clonedDlvParameters.Union(clonedOptionParametersForDlvFutures);
                }
            }

            // cast to a standard collection in order to evaluate the enumeration and to give write rights to the collection
            clonedDlvParameters = clonedDlvParameters.ToArray();

            foreach (RiskArrayParameterTimsIdem parameter in clonedDlvParameters)
            {
                // PM 20170222 [22881][22942] Les paramètres clonnés sont pour les positions en attende de livraison
                //AddInfo(pAssetETDCache, parameter, riskArraySpecialParameter.Category == "O" ? true : false);
                AddInfo(pAssetETDCache, parameter, true);
            }

            // PM 20170222 [22881][22942] Union des paramètres de livraison clonés et des paramètres de futures clonés
            IEnumerable<RiskArrayParameterTimsIdem> clonedParameters = clonedDlvParameters.Union(clonedFutureParameters);

            if (toAddRiskArrayParameters == null)
            {
                toAddRiskArrayParameters = clonedParameters;
            }
            else
            {
                toAddRiskArrayParameters = toAddRiskArrayParameters.Union(clonedParameters);
            }


            if (toRemoveRiskArrayParameters == null)
            {
                toRemoveRiskArrayParameters = new List<RiskArrayParameterTimsIdem>();
            }

            toRemoveRiskArrayParameters.Add(riskArraySpecialParameter);
        }

        /// <summary>
        /// Convertion de la position pré livraison
        /// (Pour l'AGREX (ex: DWHEAT en DWHDLV, DWHINC, DWHMAD et DWHMAW)
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">Position initiale</param>
        /// <returns>Position convertie</returns>
        private IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> ConvertPreDeliveryPosition
            (IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            List<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedPositionList = pGroupedPositionsByIdAsset.ToList();
            // Convertion de la position ExeAss
            // => convertion du contrat et de l'asset
            // => convertion en position classique (et non plus en position Exercée/assignée)
            var positionExeAssToModify =
                from position in groupedPositionList
                join assetDly in m_AssetDeliveryParameters on position.First.idAsset equals assetDly.AssetId
                join contract in m_TimsIdemContractParameters on assetDly.ContractId equals contract.ContractId
                where (position.Second.ExeAssQuantity != 0)
                   && ((contract.MatchedDeliveryContractId == assetDly.NewContractId) || (contract.MatchedWithdrawContractId == assetDly.NewContractId))
                select new
                {
                    Position = position,
                    Asset = assetDly,
                    contract.MatchedDeliveryContractId,
                    contract.MatchedWithdrawContractId,
                };
            // 
            List<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedPositionListAdded = new List<Pair<PosRiskMarginKey, RiskMarginPosition>>();
            List<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedPositionListRemoved = new List<Pair<PosRiskMarginKey, RiskMarginPosition>>();
            foreach (var position in positionExeAssToModify)
            {
                PosRiskMarginKey key = position.Position.First;
                RiskMarginPosition mgrPos = position.Position.Second;
                //
                if ((mgrPos.ExeAssQuantity > 0) && (position.Asset.NewContractId == position.MatchedDeliveryContractId))
                {
                    // Si position Assignée à livrer
                    key.idAsset = position.Asset.NewAssetId;
                    key.Side = SideTools.RetBuyFIXmlSide();
                    mgrPos.Quantity = System.Math.Abs(mgrPos.ExeAssQuantity);
                    mgrPos.ExeAssQuantity = 0;
                    //
                    groupedPositionListRemoved.Add(position.Position);
                    groupedPositionListAdded.Add(new Pair<PosRiskMarginKey, RiskMarginPosition>(key, mgrPos));
                }
                else if ((mgrPos.ExeAssQuantity < 0) && (position.Asset.NewContractId == position.MatchedWithdrawContractId))
                {
                    // Si position Exercée à recevoir
                    key.idAsset = position.Asset.NewAssetId;
                    key.Side = SideTools.RetSellFIXmlSide();
                    mgrPos.Quantity = System.Math.Abs(mgrPos.ExeAssQuantity);
                    mgrPos.ExeAssQuantity = 0;
                    //
                    groupedPositionListRemoved.Add(position.Position);
                    groupedPositionListAdded.Add(new Pair<PosRiskMarginKey, RiskMarginPosition>(key, mgrPos));
                }
            }
            // Retirer la position exercée/assignée convertie
            foreach(Pair<PosRiskMarginKey, RiskMarginPosition> position in groupedPositionListRemoved)
            {
                groupedPositionList.Remove(position);
            }
            // Ajouter la position résultant de la convertion de la position exercée/assignée
            groupedPositionList.AddRange(groupedPositionListAdded);

            // Convertion de la position en pré-livraison normal
            // et convertion de la position en pré-livraison non couverte pour les positions vendeuses
            var positionToModify =
                from position in groupedPositionList
                join assetDly in m_AssetDeliveryParameters on position.First.idAsset equals assetDly.AssetId
                join contract in m_TimsIdemContractParameters on assetDly.ContractId equals contract.ContractId
                where (position.Second.Quantity != 0)
                where ((contract.DeliveryContractId == assetDly.NewContractId)
                    && (contract.DeliveryContractId != 0)
                    && (contract.DeliveryMarginOffsetMultiplier != 0))
                   || ((contract.UncoveredDeliveryContractId == assetDly.NewContractId)
                    && (contract.UncoveredDeliveryContractId != 0)
                    && (contract.IncreaseMarginOffsetMultiplier != 0)
                    && (position.First.Side == SideTools.RetSellFIXmlSide()))
                orderby assetDly.ApplyStartDate
                select new
                {
                    Position = position,
                    Asset = assetDly,
                }
                ;
            //
            foreach (var position in positionToModify)
            {
                position.Position.First.idAsset = position.Asset.NewAssetId;
            }

            return groupedPositionList;
        }

        /// <summary>
        /// Build the TIMS product/class hierarchy
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">the positions set related to the current actor/book</param>
        /// <param name="pSpreadPositionsByIdAsset">Ensemble de position en spread par asset</param>
        /// <param name="pCoveredQuantities">Collection containing all the covered quantities for short positions/settlements</param>
        /// <returns>the TIMS product/class heirarchy</returns>
        private IEnumerable<TimsIdemProductParameterCommunicationObject> GetProductClassHierarchy(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset,
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pSpreadPositionsByIdAsset,
            IEnumerable<StockCoverageCommunicationObject> pCoveredQuantities)
        {
            // PM 20130318 AGREX
            #region to remove : PM 20130318 AGREX
            //// 1. defining the assets/contracts matrix , 
            ////  all the elements of the matrix are in position (pGroupedPositionsByIdAsset) and related to a TIMS parameter (m_RiskArrayParameters)
            //var contractAssetPairsInPosition =
            //    from position in pGroupedPositionsByIdAsset
            //    join contractAssetPair in
            //        (from riskArray
            //             in m_RiskArrayParameters
            //         select new Pair<int, int> { First = riskArray.Value.ContractId, Second = riskArray.Key.Second })
            //         .Distinct(new PairComparer<int, int>())
            //    on position.First.idAsset equals contractAssetPair.Second
            //    select new
            //    {
            //        ContractId = contractAssetPair.First,
            //        AssetId = contractAssetPair.Second
            //    };

            //// 1.1 defining the contracts in position list starting by the assets/contracts matrix 
            //IEnumerable<int> contractsInPosition =
            //    (from contractAssetPair in contractAssetPairsInPosition select contractAssetPair.ContractId).Distinct();

            //// 2. building the "Class" parameters list (starting by the loaded m_ClassParameters) will be used in the evaluation process
            //IEnumerable<ClassParameterTimsIdem> classParametersInPosition =
            //    from parameter in m_ClassParameters.Values
            //    join contract in contractsInPosition on parameter.ContractId equals contract
            //    select parameter;

            //// 3. build the hierarchy 
            //IEnumerable<TimsIdemProductParameterCommunicationObject> partitionsSet =
            //    from products in
            //        // 3.1 group by product....
            //        (from classParameter in classParametersInPosition select classParameter)
            //        .GroupBy(key => key.ProductGroup)

            //    select new TimsIdemProductParameterCommunicationObject
            //    {
            //        Product = products.Key,

            //        Positions = from position in pGroupedPositionsByIdAsset
            //                    join contractAssetPair in contractAssetPairsInPosition on position.First.idAsset equals contractAssetPair.AssetId
            //                    join product in products on contractAssetPair.ContractId equals product.ContractId
            //                    select position,

            //        SpreadPositions = from position in pSpreadPositionsByIdAsset
            //                          join contractAssetPair in contractAssetPairsInPosition on position.First.idAsset equals contractAssetPair.AssetId
            //                          join product in products on contractAssetPair.ContractId equals product.ContractId
            //                          select position,

            //        Parameters = (from classes in
            //                          // 3.2  group by class....
            //                          (from product in products select product)
            //                         .GroupBy(key => key.ClassGroup)

            //                      select new TimsIdemClassParameterCommunicationObject
            //                      {
            //                          Class = classes.Key,
                                      
            //                          ContractSymbols = from classT in classes select classT.ContractSymbol,

            //                          ClassFileContractMultipliers = from classT in classes select classT.ClassFileContractMultiplier,

            //                          Positions = from position in pGroupedPositionsByIdAsset
            //                                      join contractAssetPair in contractAssetPairsInPosition on position.First.idAsset equals contractAssetPair.AssetId
            //                                      join classT in classes on contractAssetPair.ContractId equals classT.ContractId
            //                                      select position,

            //                          SpreadPositions = from position in pSpreadPositionsByIdAsset
            //                                            join contractAssetPair in contractAssetPairsInPosition on position.First.idAsset equals contractAssetPair.AssetId
            //                                            join classT in classes on contractAssetPair.ContractId equals classT.ContractId
            //                                            select position,

            //                          Parameters = (from contracts in
            //                                            // 3.3  group by contract symbol....
            //                                            (from classT in classes select classT)
            //                                           .GroupBy(key => key.ContractSymbol)

            //                                        select new TimsIdemContractParameterCommunicationObject
            //                                        {
            //                                            Contract = contracts.Key,

            //                                            Description = (from contract in contracts select contract.ContractDescription).First(),

            //                                            Offset = (from contract in contracts select contract.Offset).First(),

            //                                            Positions = from position in pGroupedPositionsByIdAsset
            //                                                        join contractAssetPair in contractAssetPairsInPosition on position.First.idAsset equals contractAssetPair.AssetId
            //                                                        join contract in contracts on contractAssetPair.ContractId equals contract.ContractId
            //                                                        select position,

            //                                            SpreadPositions = from position in pSpreadPositionsByIdAsset
            //                                                              join contractAssetPair in contractAssetPairsInPosition on position.First.idAsset equals contractAssetPair.AssetId
            //                                                              join contract in contracts on contractAssetPair.ContractId equals contract.ContractId
            //                                                              select position,

            //                                            StocksCoverage = from coveredQuantity in pCoveredQuantities
            //                                                             join contract in contracts on coveredQuantity.ContractId equals contract.ContractId
            //                                                             select coveredQuantity,

            //                                            Parameters = null,
            //                                        }
            //                                       ).ToArray(),

            //                          // No total amounts for class objet must be computed
            //                          //MarginAmount = new Money(0, (from contract in classes select contract.Currency).First()),

            //                      }).ToArray(),

            //        Missing = false,

            //        // One currency only is attended for each IDEM class group
            //        MarginAmount = new Money(0, (from product in products select product.Currency).First()),
            //    };
            #endregion

            // PM 20130318 AGREX
            // Asset en position
            var contractAssetInPosition =
                from position in pGroupedPositionsByIdAsset
                join asset in m_AssetExpandedParameters on position.First.idAsset equals asset.AssetId
                join contract in m_TimsIdemContractParameters on asset.ContractId equals contract.ContractId
                join riskArrayAsset in m_RiskArrayParameters.Select(k => k.Key.Second).Distinct() on asset.AssetId equals riskArrayAsset
                orderby asset.ContractId, asset.AssetId
                select new
                {
                    asset.ContractId,
                    asset.AssetId,
                    MaturityYearMonth = contract.IsClassByMaturity ? asset.MaturityYearMonth : "",
                };

            // Maturity en position
            IEnumerable<Pair<int, string>> contractMaturityInPosition = (
                from asset in contractAssetInPosition
                orderby asset.ContractId
                select new Pair<int, string> { First = (int)asset.ContractId, Second = asset.MaturityYearMonth }
                ).Distinct(new PairComparer<int, string>());

            // Contrat en position
            IEnumerable<int> contractInPosition = (
                from contractAssetPair in contractAssetInPosition
                orderby contractAssetPair.ContractId
                select contractAssetPair.ContractId
                ).Distinct();

            // Class en position
            IEnumerable<Pair<ClassParameterTimsIdem, string>> classInPosition =
                from parameter in m_ClassParameters.Values
                join maturity in contractMaturityInPosition on parameter.ContractId equals maturity.First
                select new Pair<ClassParameterTimsIdem, string> { First = parameter, Second = maturity.Second };

            // Regroupement des class par product
            var productsInPosition = classInPosition.GroupBy(key => key.First.ProductGroup);

            // Construction des paramètres de calcul au niveau Product
            // PM 20130528 Ajout d'un tri par Product
            List<TimsIdemProductParameterCommunicationObject> productSet = (
                from products in productsInPosition
                select new TimsIdemProductParameterCommunicationObject
                {
                    Product = products.Key,

                    Positions = from position in pGroupedPositionsByIdAsset
                                join contractAsset in contractAssetInPosition on position.First.idAsset equals contractAsset.AssetId
                                join product in products on contractAsset.ContractId equals product.First.ContractId
                                select position,

                    SpreadPositions = from position in pSpreadPositionsByIdAsset
                                      join contractAsset in contractAssetInPosition on position.First.idAsset equals contractAsset.AssetId
                                      join product in products on contractAsset.ContractId equals product.First.ContractId
                                      select position,
                    Missing = false,

                    // One currency only is attended for each IDEM class group
                    MarginAmount = new Money(0, (from product in products select product.First.Currency).First()),
                }).OrderBy(p => p.Product).ToList();

            foreach (TimsIdemProductParameterCommunicationObject product in productSet)
            {
                // Regroupement des class par class et maturity
                // PM 20130511 Changement de la clé du GroupBy suite à problème de Class dupliquée
                //var classesOfProduct = classInPosition.Where(key => key.First.ProductGroup == product.Product).GroupBy(key => new Pair<string, string>(key.First.ClassGroup, key.Second));
                var classesOfProduct = classInPosition.Where(key => key.First.ProductGroup == product.Product).GroupBy(key => key.First.ClassGroup + key.Second);
                
                // Construction des paramètres de calcul au niveau Class
                // PM 20130528 Ajout d'un tri par Class
                product.Parameters = (
                    from classes in classesOfProduct
                    select new TimsIdemClassParameterCommunicationObject
                    {
                        // PM 20130511 Répercution du changement de la clé du GroupBy ci-dessus suite à problème de Class dupliquée
                        //Class = classes.Key.First + classes.Key.Second,
                        Class = classes.Key,

                        ContractSymbols = from classT in classes select classT.First.ContractSymbol,

                        ClassFileContractMultipliers = from classT in classes select classT.First.ClassFileContractMultiplier,

                        Positions = from position in pGroupedPositionsByIdAsset
                                    join contractAssetPair in contractAssetInPosition on position.First.idAsset equals contractAssetPair.AssetId
                                    join classT in classes on contractAssetPair.ContractId equals classT.First.ContractId
                                    where contractAssetPair.MaturityYearMonth == classT.Second
                                    select position,

                        SpreadPositions = from position in pSpreadPositionsByIdAsset
                                          join contractAssetPair in contractAssetInPosition on position.First.idAsset equals contractAssetPair.AssetId
                                          join classT in classes on contractAssetPair.ContractId equals classT.First.ContractId
                                          where contractAssetPair.MaturityYearMonth == classT.Second
                                          select position,

                    }).OrderBy(c => c.Class).ToArray();

                foreach (TimsIdemClassParameterCommunicationObject classe in product.Parameters)
                {
                    // Regroupement des class par contrat
                    var contractsOfClass = classInPosition.Where(key => (key.First.ClassGroup + key.Second) == classe.Class).GroupBy(key => key.First.ContractSymbol);

                    // Construction des paramètres de calcul au niveau Contract
                    // PM 20130528 Ajout d'un tri par Contract
                    classe.Parameters = (
                        from contracts in contractsOfClass
                        select new TimsIdemContractParameterCommunicationObject
                        {
                            Contract = contracts.Key,

                            Description = (from contract in contracts select contract.First.ContractDescription).First(),

                            Offset = (from contract in contracts select contract.First.Offset).First(),

                            Positions = from position in pGroupedPositionsByIdAsset
                                        join contractAssetPair in contractAssetInPosition on position.First.idAsset equals contractAssetPair.AssetId
                                        join contract in contracts on contractAssetPair.ContractId equals contract.First.ContractId
                                        where contractAssetPair.MaturityYearMonth == contract.Second
                                        select position,

                            SpreadPositions = from position in pSpreadPositionsByIdAsset
                                              join contractAssetPair in contractAssetInPosition on position.First.idAsset equals contractAssetPair.AssetId
                                              join contract in contracts on contractAssetPair.ContractId equals contract.First.ContractId
                                              where contractAssetPair.MaturityYearMonth == contract.Second
                                              select position,

                            StocksCoverage = from coveredQuantity in pCoveredQuantities
                                           join contract in contracts on coveredQuantity.ContractId equals contract.First.ContractId
                                           select coveredQuantity,

                            Parameters = null,
                        }).OrderBy(c => c.Contract).ToArray();
                }
            }

            IEnumerable<TimsIdemProductParameterCommunicationObject> partitionsSet = productSet;

            // 4. build a fake parameter container of all the assets for which no parameters have been found

            // PM 20130318 AGREX
            //IEnumerable<int> assetWithParamsInPosition = from elem in contractAssetPairsInPosition select elem.AssetId;
            IEnumerable<int> assetWithParamsInPosition = from elem in contractAssetInPosition select elem.AssetId;

            TimsIdemProductParameterCommunicationObject communicationObjectForMissingParameters =
                new TimsIdemProductParameterCommunicationObject();

            if (InitializeCommunicationObjectForMissingParameters<TimsIdemProductParameterCommunicationObject>
                (pGroupedPositionsByIdAsset, assetWithParamsInPosition, communicationObjectForMissingParameters))
            {
                communicationObjectForMissingParameters.Product = Cst.NotFound;

                partitionsSet = partitionsSet.Union
                       (
                           new TimsIdemProductParameterCommunicationObject[] { communicationObjectForMissingParameters }
                       );
            }

            return partitionsSet;
        }

        private IEnumerable<MinimumEvaluationParameters> GetMinimumEvaluationParameters(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset,
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pPositionActionActorBook)
        {
            IEnumerable<MinimumEvaluationParameters> ordinaryParams =
                from riskArrayParameter in m_RiskArrayParameters

                join position in pGroupedPositionsByIdAsset on riskArrayParameter.Key.Second equals position.First.idAsset

                // the minimum margin is computed with open positions only according to the IDEM specification, 
                //  but EUROSYS use exercises/assignations also to return this value

                where
                    (riskArrayParameter.Key.First == RiskMethodQtyType.Put
                    || riskArrayParameter.Key.First == RiskMethodQtyType.Call
                    || riskArrayParameter.Key.First == RiskMethodQtyType.Future)
                    ||
                    (position.Second.ExeAssQuantity != 0
                        && (riskArrayParameter.Key.First == RiskMethodQtyType.ExeAssPut
                           || riskArrayParameter.Key.First == RiskMethodQtyType.ExeAssCall
                           || riskArrayParameter.Key.First == RiskMethodQtyType.FutureMoff))
        
                join classParameter in m_ClassParameters on
                    new { riskArrayParameter.Value.ContractId, riskArrayParameter.Value.ContractSymbol }
                    equals
                    new { ContractId = classParameter.Key.First, ContractSymbol = classParameter.Key.Second }

                select new MinimumEvaluationParameters
                {
                    AssetID = riskArrayParameter.Key.Second,
                    MinimumRate = classParameter.Value.MinimumMargin,
                    Currency = classParameter.Value.Currency,
                    Type = riskArrayParameter.Value.Type
                };

            IEnumerable<MinimumEvaluationParameters> positionActionsParams =
                from groupPositionActionParameter in (
                    from riskArrayParameter in m_RiskArrayParameters
                    where riskArrayParameter.Value.Type == RiskMethodQtyType.PositionAction

                    join classParameter in m_ClassParameters on
                        riskArrayParameter.Value.ContractSymbol
                        equals
                        classParameter.Key.Second
                    where classParameter.Key.First == 0

                    join position in pPositionActionActorBook on 
                    riskArrayParameter.Value.ContractSymbol equals position.First.derivativeContractSymbol

                    select new MinimumEvaluationParameters
                    {
                        AssetID = position.First.idAsset,
                        MinimumRate = classParameter.Value.MinimumMargin,
                        Currency = position.Second.Currency,
                        Type = riskArrayParameter.Value.Type,
                        CrossMargin = riskArrayParameter.Value.CrossMarginActivated,
                    })
                .GroupBy(key => key.AssetID)
                select groupPositionActionParameter.First();

            return ordinaryParams.Union(positionActionsParams);
        }

        private IEnumerable<PremiumEvaluationParameters> GetPremiumEvaluationParameters
            (IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            // 1 get the premium ordinary parameters for open positions
            IEnumerable<PremiumEvaluationParameters> ordinaryParams =

               from riskArrayParameter in m_RiskArrayParameters
               where riskArrayParameter.Value.Type == RiskMethodQtyType.Call || riskArrayParameter.Value.Type == RiskMethodQtyType.Put

               // exclude the parameter for the assets not in position for the current risk element
               join position in pGroupedPositionsByIdAsset on riskArrayParameter.Key.Second equals position.First.idAsset

               // join the class parameter set
               join classParameter in m_ClassParameters on
                    new { riskArrayParameter.Value.ContractId, riskArrayParameter.Value.ContractSymbol }
                    equals
                    new { ContractId = classParameter.Key.First, ContractSymbol = classParameter.Key.Second }

               select new PremiumEvaluationParameters
               {
                   AssetID = riskArrayParameter.Key.Second,
                   StrikePrice = riskArrayParameter.Value.RiskArrayStrikePrice,
                   Quote = riskArrayParameter.Value.RiskArrayQuote,
                   Type = riskArrayParameter.Value.Type,
                   UnlQuote = classParameter.Value.ClassFileAssetUnlQuote,
                   Multiplier = classParameter.Value.ClassFileContractMultiplier,
                   Currency = classParameter.Value.Currency,
               };

            // 2. get the exercises/assignements parameters
            IEnumerable<PremiumEvaluationParameters> exeAssParams =

               from riskArrayParameter in m_RiskArrayParameters
               where riskArrayParameter.Value.Type == RiskMethodQtyType.ExeAssPut || riskArrayParameter.Value.Type == RiskMethodQtyType.ExeAssCall

               // ... see case 1 ...
               join position in pGroupedPositionsByIdAsset on riskArrayParameter.Key.Second equals position.First.idAsset
               where position.Second.ExeAssQuantity != 0

               // ... see case 1 ...
               join classParameter in m_ClassParameters on
                    new { riskArrayParameter.Value.ContractId, riskArrayParameter.Value.ContractSymbol }
                    equals
                    new { ContractId = classParameter.Key.First, ContractSymbol = classParameter.Key.Second }

               select new PremiumEvaluationParameters
               {
                   AssetID = riskArrayParameter.Key.Second,
                   StrikePrice = riskArrayParameter.Value.StrikePrice,
                   Type = riskArrayParameter.Value.Type,
                   UnlQuote = classParameter.Value.ClassFileAssetUnlQuote,
                   Multiplier = classParameter.Value.ClassFileContractMultiplier,
                   Currency = classParameter.Value.Currency,
               };

            return ordinaryParams.Union(exeAssParams);
        }

        private IEnumerable<AdditionalEvaluationParameters> GetAdditionalEvaluationParameters
            (IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset,
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pPositionActionActorBook)
        {
            IEnumerable<AdditionalEvaluationParameters> ordinaryParams =

               from riskArrayParameter in m_RiskArrayParameters
               where
                   riskArrayParameter.Value.Type == RiskMethodQtyType.Call
                   || riskArrayParameter.Value.Type == RiskMethodQtyType.Put
                   || riskArrayParameter.Value.Type == RiskMethodQtyType.Future

               join position in pGroupedPositionsByIdAsset on riskArrayParameter.Key.Second equals position.First.idAsset

               join classParameter in m_ClassParameters on
                    new { riskArrayParameter.Value.ContractId, riskArrayParameter.Value.ContractSymbol }
                    equals
                    new { ContractId = classParameter.Key.First, ContractSymbol = classParameter.Key.Second }

               select new AdditionalEvaluationParameters
               {
                   AssetID = riskArrayParameter.Key.Second,
                   PosType = PosType.ALC,

                   DownSide5 = GetSidePoint5(
                                    riskArrayParameter.Value.DownSide5, riskArrayParameter.Value.ShortAdj, false,
                                    position.First.Side, riskArrayParameter.Value.Type,
                                    riskArrayParameter.Value.RiskArrayStrikePrice, classParameter.Value.ClassFileAssetUnlQuote),

                   DownSide4 = riskArrayParameter.Value.DownSide4,
                   DownSide3 = riskArrayParameter.Value.DownSide3,
                   DownSide2 = riskArrayParameter.Value.DownSide2,
                   DownSide1 = riskArrayParameter.Value.DownSide1,
                   UpSide1 = riskArrayParameter.Value.UpSide1,
                   UpSide2 = riskArrayParameter.Value.UpSide2,
                   UpSide3 = riskArrayParameter.Value.UpSide3,
                   UpSide4 = riskArrayParameter.Value.UpSide4,

                   UpSide5 = GetSidePoint5(
                                    riskArrayParameter.Value.UpSide5, riskArrayParameter.Value.ShortAdj, true,
                                    position.First.Side, riskArrayParameter.Value.Type,
                                    riskArrayParameter.Value.RiskArrayStrikePrice, classParameter.Value.ClassFileAssetUnlQuote),

                   Multiplier = classParameter.Value.ClassFileContractMultiplier,

                   Currency = classParameter.Value.Currency,
               };

            IEnumerable<AdditionalEvaluationParameters> exeAssParams =

               from riskArrayParameter in m_RiskArrayParameters
               where
                riskArrayParameter.Value.Type == RiskMethodQtyType.ExeAssPut
                || riskArrayParameter.Value.Type == RiskMethodQtyType.ExeAssCall

               join position in pGroupedPositionsByIdAsset on riskArrayParameter.Key.Second equals position.First.idAsset
               where position.Second.ExeAssQuantity != 0

               join classParameter in m_ClassParameters on
                    new { riskArrayParameter.Value.ContractId, riskArrayParameter.Value.ContractSymbol }
                    equals
                    new { ContractId = classParameter.Key.First, ContractSymbol = classParameter.Key.Second }

               select new AdditionalEvaluationParameters
               {
                   AssetID = riskArrayParameter.Key.Second,
                   PosType = position.Second.ExeAssQuantity > 0 ? PosType.AS : PosType.EX,
                   DownSide5 =
                       GetInTheMoneyAmount(
                       riskArrayParameter.Value.Type, riskArrayParameter.Value.DownSide5, classParameter.Value.ClassFileAssetUnlQuote),

                   DownSide4 =
                       GetInTheMoneyAmount(
                       riskArrayParameter.Value.Type, riskArrayParameter.Value.DownSide4, classParameter.Value.ClassFileAssetUnlQuote),

                   DownSide3 =
                       GetInTheMoneyAmount(
                       riskArrayParameter.Value.Type, riskArrayParameter.Value.DownSide3, classParameter.Value.ClassFileAssetUnlQuote),

                   DownSide2 =
                       GetInTheMoneyAmount(
                       riskArrayParameter.Value.Type, riskArrayParameter.Value.DownSide2, classParameter.Value.ClassFileAssetUnlQuote),

                   DownSide1 =
                       GetInTheMoneyAmount(
                       riskArrayParameter.Value.Type, riskArrayParameter.Value.DownSide1, classParameter.Value.ClassFileAssetUnlQuote),

                   UpSide1 =
                       GetInTheMoneyAmount(
                       riskArrayParameter.Value.Type, riskArrayParameter.Value.UpSide1, classParameter.Value.ClassFileAssetUnlQuote),

                   UpSide2 =
                       GetInTheMoneyAmount(
                       riskArrayParameter.Value.Type, riskArrayParameter.Value.UpSide2, classParameter.Value.ClassFileAssetUnlQuote),

                   UpSide3 =
                       GetInTheMoneyAmount(
                      riskArrayParameter.Value.Type, riskArrayParameter.Value.UpSide3, classParameter.Value.ClassFileAssetUnlQuote),

                   UpSide4 =
                       GetInTheMoneyAmount(
                       riskArrayParameter.Value.Type, riskArrayParameter.Value.UpSide4, classParameter.Value.ClassFileAssetUnlQuote),

                   UpSide5 =
                       GetInTheMoneyAmount(
                       riskArrayParameter.Value.Type, riskArrayParameter.Value.UpSide5, classParameter.Value.ClassFileAssetUnlQuote),

                   Multiplier = classParameter.Value.ClassFileContractMultiplier,

                   Currency = classParameter.Value.Currency,
               };

            IEnumerable<AdditionalEvaluationParameters> moffParams =

               from riskArrayParameter in m_RiskArrayParameters
               where riskArrayParameter.Value.Type == RiskMethodQtyType.FutureMoff

               join position in pGroupedPositionsByIdAsset on riskArrayParameter.Key.Second equals position.First.idAsset

               join classParameter in m_ClassParameters on
                    new { riskArrayParameter.Value.ContractId, riskArrayParameter.Value.ContractSymbol }
                    equals
                    new { ContractId = classParameter.Key.First, ContractSymbol = classParameter.Key.Second }

               select new AdditionalEvaluationParameters
               {
                   AssetID = riskArrayParameter.Key.Second,

                   PosType = PosType.DN,

                   DownSide5 = riskArrayParameter.Value.DownSide5 - classParameter.Value.ClassFileAssetUnlQuote,
                   DownSide4 = riskArrayParameter.Value.DownSide4 - classParameter.Value.ClassFileAssetUnlQuote,
                   DownSide3 = riskArrayParameter.Value.DownSide3 - classParameter.Value.ClassFileAssetUnlQuote,
                   DownSide2 = riskArrayParameter.Value.DownSide2 - classParameter.Value.ClassFileAssetUnlQuote,
                   DownSide1 = riskArrayParameter.Value.DownSide1 - classParameter.Value.ClassFileAssetUnlQuote,
                   UpSide1 = riskArrayParameter.Value.UpSide1 - classParameter.Value.ClassFileAssetUnlQuote,
                   UpSide2 = riskArrayParameter.Value.UpSide2 - classParameter.Value.ClassFileAssetUnlQuote,
                   UpSide3 = riskArrayParameter.Value.UpSide3 - classParameter.Value.ClassFileAssetUnlQuote,
                   UpSide4 = riskArrayParameter.Value.UpSide4 - classParameter.Value.ClassFileAssetUnlQuote,
                   UpSide5 = riskArrayParameter.Value.UpSide5 - classParameter.Value.ClassFileAssetUnlQuote,

                   Multiplier = classParameter.Value.ClassFileContractMultiplier,

                   Currency = classParameter.Value.Currency,
               };

            IEnumerable<AdditionalEvaluationParameters> positionActionParams =
            from groupPositionActionParameter in (
               from riskArrayParameter in m_RiskArrayParameters
               where riskArrayParameter.Value.Type == RiskMethodQtyType.PositionAction

               join classParameter in m_ClassParameters on
                   riskArrayParameter.Value.ContractSymbol
                   equals
                   classParameter.Key.Second
               where classParameter.Key.First == 0

               join position in pPositionActionActorBook 
               on riskArrayParameter.Value.ContractSymbol equals position.First.derivativeContractSymbol

               select new AdditionalEvaluationParameters
               {
                   AssetID = position.First.idAsset,
                   PositionActionIdentifier = position.First.derivativeContractSymbol,

                   PosType = PosType.XM,

                   // 20110823 MF no exchange rate,  EUROSYS use  TIMS_DEV_ACT.TX_CHANGE to compute uf_valtims_downupside, but that is not needed
                   // anymore

                   DownSide5 = riskArrayParameter.Value.DownSide5,
                   DownSide4 = riskArrayParameter.Value.DownSide4,
                   DownSide3 = riskArrayParameter.Value.DownSide3,
                   DownSide2 = riskArrayParameter.Value.DownSide2,
                   DownSide1 = riskArrayParameter.Value.DownSide1,
                   UpSide1 = riskArrayParameter.Value.UpSide1,
                   UpSide2 = riskArrayParameter.Value.UpSide2,
                   UpSide3 = riskArrayParameter.Value.UpSide3,
                   UpSide4 = riskArrayParameter.Value.UpSide4,
                   UpSide5 = riskArrayParameter.Value.UpSide5,

                   Multiplier = classParameter.Value.ClassFileContractMultiplier,

                   Currency = position.Second.Currency,

                   CrossMargin = riskArrayParameter.Value.CrossMarginActivated,
               })
                .GroupBy(key => key.AssetID)
                select groupPositionActionParameter.First();

            return ordinaryParams.Union(exeAssParams).Union(moffParams).Union(positionActionParams);
        }

        private decimal GetSidePoint5(
            decimal pSidePoint5, decimal pShortAdj, bool pUp, string pSide, RiskMethodQtyType pType, decimal pStrikePrice, decimal pAssetUnlQuote)
        {
            decimal sidePoint5 = pSidePoint5;

            if (pSide == SideTools.RetSellFIXmlSide())
            {
                bool canSwap = (pUp && pType == RiskMethodQtyType.Call) || (!pUp && pType == RiskMethodQtyType.Put);

                decimal inTheMoneyAmount = GetInTheMoneyAmount(pType, pAssetUnlQuote, pStrikePrice);

                if (canSwap && inTheMoneyAmount < 0 && pShortAdj > pSidePoint5)
                {
                    sidePoint5 = pShortAdj;
                }
            }

            return sidePoint5;

        }

        private IEnumerable<SpreadEvaluationParameters> GetSpreadEvaluationParameters
            (IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            return
                from riskArrayParameter in m_RiskArrayParameters
                where riskArrayParameter.Value.Type == RiskMethodQtyType.Future

                // filter the previous set with the id of the assets in positions for the current risk element
                join position in pGroupedPositionsByIdAsset on riskArrayParameter.Key.Second equals position.First.idAsset

                // join the class parameter set
                join classParameter in m_ClassParameters on
                    new { riskArrayParameter.Value.ContractId, riskArrayParameter.Value.ContractSymbol }
                    equals
                    new { ContractId = classParameter.Key.First, ContractSymbol = classParameter.Key.Second }

                select new SpreadEvaluationParameters
                {
                    AssetID = riskArrayParameter.Key.Second,
                    MaturityYearMonth = riskArrayParameter.Value.MaturityYearMonth,
                    SpotSpreadRate = classParameter.Value.SpotSpreadRate,
                    NonSpotSpreadRate = classParameter.Value.NonSpotSpreadRate,
                    Currency = classParameter.Value.Currency,
                    MaturityRuleFrequency = classParameter.Value.MaturityRuleFrequency,
                };
        }

        private IEnumerable<MtmEvaluationParameters> GetMtmEvaluationParameters
            (IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset,
             IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pPositionActionActorBook)
        {
            IEnumerable<MtmEvaluationParameters> moffParameters =
                from riskArrayParameterMoff in m_RiskArrayParameters
                where riskArrayParameterMoff.Value.Type == RiskMethodQtyType.FutureMoff

                // filter the previous set with the id of the assets in positions for the current risk element
                join position in pGroupedPositionsByIdAsset on riskArrayParameterMoff.Key.Second equals position.First.idAsset

                // join the class parameter set
                join classParameter in m_ClassParameters on
                    new { riskArrayParameterMoff.Value.ContractId, riskArrayParameterMoff.Value.ContractSymbol }
                    equals
                    new { ContractId = classParameter.Key.First, ContractSymbol = classParameter.Key.Second }

                join riskArrayParameter in m_RiskArrayParameters on riskArrayParameterMoff.Key.Second equals riskArrayParameter.Key.Second
                where riskArrayParameter.Value.Type == RiskMethodQtyType.Future

                select new MtmEvaluationParameters
                {
                    AssetID = riskArrayParameterMoff.Key.Second,
                    Type = riskArrayParameterMoff.Value.Type,
                    MtmQuote = classParameter.Value.ClassFileAssetUnlQuote - riskArrayParameter.Value.RiskArrayQuote,
                    Multiplier = classParameter.Value.ClassFileContractMultiplier,
                    Currency = classParameter.Value.Currency,

                };

            IEnumerable<MtmEvaluationParameters> positionActionParameters = 
                from riskArrayParameter in m_RiskArrayParameters
                where riskArrayParameter.Value.Type == RiskMethodQtyType.PositionAction

                join classParameter in m_ClassParameters on
                    riskArrayParameter.Value.ContractSymbol 
                    equals
                    classParameter.Key.Second
                where classParameter.Key.First == 0
                
                join position in pPositionActionActorBook 
                 on riskArrayParameter.Value.ContractSymbol equals position.First.derivativeContractSymbol

                select new MtmEvaluationParameters
                {
                    AssetID = position.First.idAsset,
                    MaturityDate = position.First.maturityDate,
                    PositionActionIdentifier = position.First.PosActionIdentifier,
                    Type = riskArrayParameter.Value.Type,
                    CmvQuote = classParameter.Value.ClassFileAssetUnlQuote,
                    DvpAmount = position.Second.ShortCrtVal - position.Second.LongCrtVal, 
                    Multiplier = classParameter.Value.ClassFileContractMultiplier,
                    Currency = position.Second.Currency,
                    CrossMargin = riskArrayParameter.Value.CrossMarginActivated,

                };

            return moffParameters.Union(positionActionParameters);
        }

        private TimsDecomposableParameterCommunicationObject GetSpreadMargin(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pClassPositions,
            IEnumerable<SpreadEvaluationParameters> pSpreadEvalParams)
        {
            TimsDecomposableParameterCommunicationObject spreadComObj = null;

            SpreadEvaluationParameters contractSpreadParameterRates =
                (from position in pClassPositions
                 join data in pSpreadEvalParams on position.First.idAsset equals data.AssetID
                 select data).OrderBy(d => d.NonSpotSpreadRate)
                 .FirstOrDefault();

            if ((contractSpreadParameterRates.AssetID != 0) &&
                (pClassPositions != default))
            {

                var spreadFuturesPositions =
                    (from position in pClassPositions
                     join data in pSpreadEvalParams on position.First.idAsset equals data.AssetID
                     select new
                     {
                         Position = position,
                         Spread = data,
                     }).OrderBy(p => p.Spread.MaturityYearMonth);;

                if (spreadFuturesPositions.Any(p => p.Position.First.Side == SideTools.RetBuyFIXmlSide()) && spreadFuturesPositions.Any(p => p.Position.First.Side == SideTools.RetSellFIXmlSide()))
                {
                    List<SpreadPositionCommunicationObject> spreadLong =
                        (from pos in spreadFuturesPositions
                         where pos.Position.First.Side == SideTools.RetBuyFIXmlSide()
                         select new SpreadPositionCommunicationObject
                         {
                             AssetId = pos.Position.First.idAsset,
                             Quantity = pos.Position.Second.Quantity,
                             Ratio = RiskTools.IsSpotMonth(pos.Spread.MaturityYearMonth, pos.Spread.MaturityRuleFrequency, this.DtBusiness) ? contractSpreadParameterRates.SpotSpreadRate : contractSpreadParameterRates.NonSpotSpreadRate,
                         }).ToList();

                    List<SpreadPositionCommunicationObject> spreadShort =
                        (from pos in spreadFuturesPositions
                         where pos.Position.First.Side == SideTools.RetSellFIXmlSide()
                         select new SpreadPositionCommunicationObject
                         {
                             AssetId = pos.Position.First.idAsset,
                             Quantity = pos.Position.Second.Quantity,
                             Ratio = RiskTools.IsSpotMonth(pos.Spread.MaturityYearMonth, pos.Spread.MaturityRuleFrequency, this.DtBusiness) ? contractSpreadParameterRates.SpotSpreadRate : contractSpreadParameterRates.NonSpotSpreadRate,
                             SpreadQuantity = 0,
                         }).ToList();

                    foreach (SpreadPositionCommunicationObject positionLong in spreadLong)
                    {
                        List<SpreadPositionCommunicationObject> spreadShortToScan = spreadShort.Where(p => p.SpreadQuantity < p.Quantity).ToList();
                        if (spreadShortToScan.Count() > 0)
                        {
                            foreach (SpreadPositionCommunicationObject positionShort in spreadShortToScan)
                            {
                                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                // EG 20170127 Qty Long To Decimal
                                decimal qtyLong = positionLong.Quantity - positionLong.SpreadQuantity;
                                decimal qtyShort = positionShort.Quantity - positionShort.SpreadQuantity;
                                if (qtyShort >= qtyLong)
                                {
                                    positionShort.SpreadQuantity += qtyLong;
                                    positionLong.SpreadQuantity += qtyLong;
                                    break;
                                }
                                else
                                {
                                    positionShort.SpreadQuantity += qtyShort;
                                    positionLong.SpreadQuantity += qtyShort;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    /* Insertion du signe de la quantity pour les position short */
                    spreadShort.ForEach(s => s.SpreadQuantity *= -1);
                    /* Concaténation des spread long et short */
                    List<SpreadPositionCommunicationObject> spreadPosition = spreadLong.Where(p => p.SpreadQuantity > 0)
                        .Concat(spreadShort.Where(p => p.SpreadQuantity < 0)).ToList();
                    /* Calcul du montant de spread */
                    spreadPosition.ForEach(s => s.Amount = new Money(System.Math.Abs(s.SpreadQuantity) * s.Ratio, contractSpreadParameterRates.Currency));

                    IEnumerable<TimsFactorCommunicationObject> spreadFactor =
                    from spread in spreadPosition
                        select new TimsFactorCommunicationObject
                        {
                            AssetId = spread.AssetId,
                            PosType = PosType.IAS,
                            Quantity = spread.SpreadQuantity,
                            Quote = spread.Ratio,
                            MarginAmount = spread.Amount,
                        };

                    /*-------------*/

                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pContractFuturesPositions =
                        (from position in pClassPositions
                         join data in pSpreadEvalParams on position.First.idAsset equals data.AssetID
                         select position);
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    decimal qtyLongNet = (from position in pContractFuturesPositions
                                      where position.First.Side == SideTools.RetBuyFIXmlSide()
                                      select position.Second.Quantity).Sum();
                    decimal qtyShortNet = (from position in pContractFuturesPositions
                                       where position.First.Side == SideTools.RetSellFIXmlSide()
                                       select position.Second.Quantity).Sum();

                    // class group spread == min between net long and net short quantities
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    decimal qtyClassSpread = System.Math.Min(qtyLongNet, qtyShortNet);
                    decimal qtySpot = (
                        from position in pContractFuturesPositions
                        join data in pSpreadEvalParams on position.First.idAsset equals data.AssetID
                        where RiskTools.IsSpotMonth(data.MaturityYearMonth, data.MaturityRuleFrequency, this.DtBusiness)
                        select new
                        {
                            position.First.Side,
                            position.Second.Quantity,
                        } into QtySidePairs
                        group QtySidePairs by QtySidePairs.Side into QtySidePairsGroupedBySide
                        select
                            QtySidePairsGroupedBySide.Key == SideTools.RetSellFIXmlSide() ?
                            (from pair in QtySidePairsGroupedBySide select pair.Quantity).Sum()
                            :
                            (from pair in QtySidePairsGroupedBySide select -1 * pair.Quantity).Sum()
                        ).Sum();

                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    decimal qtyClassSpot = System.Math.Abs(qtySpot) <= qtyClassSpread ? System.Math.Abs(qtySpot) : qtyClassSpread;
                    decimal qtyClassSpreadTotal = qtyClassSpread * 2;
                    decimal qtyClassNonSpot = qtyClassSpreadTotal - qtyClassSpot;

                    TimsFactorCommunicationObject factorSpot =
                        qtyClassSpot != 0 ?
                            new TimsFactorCommunicationObject
                            {
                                Identifier = "Spot",

                                Quantity = qtyClassSpot,

                                Quote = contractSpreadParameterRates.SpotSpreadRate,

                                MarginAmount =
                                new Money(
                                    this.RoundAmount(
                                    //
                                    qtyClassSpot * contractSpreadParameterRates.SpotSpreadRate
                                    //
                                    ),
                                    contractSpreadParameterRates.Currency),
                            }
                        :
                            null;

                    TimsFactorCommunicationObject factorNonSpot =
                        qtyClassNonSpot != 0 ?

                            new TimsFactorCommunicationObject
                            {
                                Identifier = "NonSpot",

                                Quantity = qtyClassNonSpot,

                                Quote = contractSpreadParameterRates.NonSpotSpreadRate,

                                MarginAmount =
                                new Money(
                                    this.RoundAmount(
                                    //
                                    qtyClassNonSpot * contractSpreadParameterRates.NonSpotSpreadRate
                                    //
                                    ),
                                    contractSpreadParameterRates.Currency),
                            }
                        :
                            null;

                    TimsFactorCommunicationObject[] factors = new TimsFactorCommunicationObject[] { factorSpot, factorNonSpot };

                    factors = (from factor in factors where factor != null select factor).ToArray();

                    if (factors.Length > 0)
                    {

                        spreadComObj = new TimsDecomposableParameterCommunicationObject();

                        List<Money> amounts = new List<Money>();
                        SumAmounts(from factor in factors select (Money)factor.MarginAmount, ref amounts);

                        IEnumerable<TimsFactorCommunicationObject> qtyFactors =
                            GetSpreadQtyFactors(qtyLongNet, qtyShortNet, qtySpot);

                        spreadComObj.Factors = spreadFactor.Concat(qtyFactors.Concat(factors));

                        // ona currency only is attended for the class spread margin amount, taking the 0 index amount
                        spreadComObj.MarginAmount = amounts[0];

                    }
                }
            }
            return spreadComObj;
        }

        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private IEnumerable<TimsFactorCommunicationObject> GetSpreadQtyFactors
            (decimal pQtyLongNet, decimal pQtyShortNet, decimal pQtySpotOriginal)
        {
            return new TimsFactorCommunicationObject[] 
            {
                new TimsFactorCommunicationObject
                {
                    Identifier = "LongNet",

                    Quantity = pQtyLongNet,
                },
                new TimsFactorCommunicationObject
                {
                    Identifier = "ShortNet",

                    Quantity = -1 * pQtyShortNet,
                },
                new TimsFactorCommunicationObject
                {
                    Identifier = "SpotOriginal",

                    Quantity = pQtySpotOriginal,
                }
            };
        }

        private TimsDecomposableParameterCommunicationObject GetMtmMargin(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pContractPositions,
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pContractPositionAction,
            IEnumerable<MtmEvaluationParameters> pMtmEvalParams)
        {

            IEnumerable<TimsFactorCommunicationObject> factorsMoff =
                from position in pContractPositions
                join data in pMtmEvalParams on position.First.idAsset equals data.AssetID
                where data.Type == RiskMethodQtyType.FutureMoff
                select new TimsFactorCommunicationObject
                {
                    AssetId = data.AssetID,
                    Multiplier = data.Multiplier,
                    Quote = data.MtmQuote,
                    Quantity = position.Second.ExeAssQuantity,
                    PosType = PosType.DN,

                    MarginAmount = new Money(

                        this.RoundAmount(
                        //
                            position.Second.ExeAssQuantity
                            * data.MtmQuote
                            * data.Multiplier
                        //
                        )

                        , data.Currency),

                };

            IEnumerable<TimsFactorCommunicationObject> factorsPositionAction =
                from data in pMtmEvalParams 
                where data.Type == RiskMethodQtyType.PositionAction
                join position in pContractPositionAction on
                    new { AssetId = data.AssetID, MaturityDate = data.MaturityDate.Value } 
                    equals
                    new { AssetId = position.First.idAsset, MaturityDate = position.First.maturityDate }
                select new TimsFactorCommunicationObject
                {

                    Identifier = data.PositionActionIdentifier,
                    Multiplier = data.Multiplier,
                    Quote = data.CmvQuote,
                    DvpAmount = data.DvpAmount,
                    Quantity = position.First.Side == "1" ? (-1) * position.Second.Quantity : position.Second.Quantity,
                    PosType = data.CrossMargin ? PosType.XM : PosType.PA,

                    MarginAmount = new Money(

                        this.RoundAmount(
                        //
                            ((position.First.Side == "1" ? (-1) * position.Second.Quantity : position.Second.Quantity)
                            * data.Multiplier
                            * data.CmvQuote)
                            -
                            (data.DvpAmount)
                        //
                        )

                        , data.Currency),

                };

            TimsFactorCommunicationObject [] factors = (factorsMoff.Union(factorsPositionAction)).ToArray();

            TimsDecomposableParameterCommunicationObject mtmComObj = null;

            if (factors.Length > 0)
            {

                mtmComObj = new TimsDecomposableParameterCommunicationObject();

                List<Money> amounts = new List<Money>();
                SumAmounts(from factor in factors 
                           where factor.PosType != PosType.PA 
                           select (Money)factor.MarginAmount, ref amounts);

                mtmComObj.Factors = factors;

                if (amounts.Count > 0)
                {
                    mtmComObj.MarginAmount = amounts[0];
                }
                else
                {
                    // One currency only is attended for an mtm margin amount...
                    mtmComObj.MarginAmount = new Money(0, (from factor in factors select factor.MarginAmount.GetCurrency.Value).First());
                }
                
            }

            return mtmComObj;
        }

        private TimsDecomposableParameterCommunicationObject GetPremiumMargin(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pContractPositions,
            IEnumerable<PremiumEvaluationParameters> pPremiumEvalParams)
        {
            // 1. Calculate the premium margin for each ordinary position
            IEnumerable<TimsFactorCommunicationObject> factorsOrdinaryPos =
                from position in pContractPositions
                join data in pPremiumEvalParams on position.First.idAsset equals data.AssetID
                where data.Type == RiskMethodQtyType.Put || data.Type == RiskMethodQtyType.Call
                select new TimsFactorCommunicationObject
                {
                    AssetId = data.AssetID,
                    Quote = data.Quote,
                    Multiplier = data.Multiplier,
                    Quantity = position.First.Side == "1" ? (-1) * position.Second.Quantity : position.Second.Quantity,
                    PosType = PosType.ALC,

                    MarginAmount = new Money(

                        this.RoundAmount(
                        //
                            (position.First.Side == "1" ? (-1) * position.Second.Quantity : position.Second.Quantity)
                            * data.Quote
                            * data.Multiplier
                        //
                        )

                        , data.Currency),

                };

            IEnumerable<TimsFactorCommunicationObject> factorsExeAssPos =
                from position in pContractPositions
                join data in pPremiumEvalParams on position.First.idAsset equals data.AssetID
                where data.Type == RiskMethodQtyType.ExeAssPut || data.Type == RiskMethodQtyType.ExeAssCall
                select new TimsFactorCommunicationObject
                {
                    AssetId = data.AssetID,
                    Quote = GetInTheMoneyAmount(data.Type, data.UnlQuote, data.StrikePrice),
                    StrikePrice = data.StrikePrice,
                    Multiplier = data.Multiplier,
                    Quantity = position.Second.ExeAssQuantity,
                    PosType = position.Second.ExeAssQuantity > 0 ? PosType.AS : PosType.EX,

                    DeliveryDate = position.Second.DeliveryDate,

                    MarginAmount = new Money(

                        this.RoundAmount(
                        //
                            GetInTheMoneyAmount(data.Type, data.UnlQuote, data.StrikePrice)
                            * position.Second.ExeAssQuantity
                            * data.Multiplier
                        //
                        )

                        , data.Currency),
                };

            TimsFactorCommunicationObject[] factors = factorsOrdinaryPos.Union(factorsExeAssPos).ToArray();

            TimsDecomposableParameterCommunicationObject premiumComObj = null;

            if (factors.Length > 0)
            {

                premiumComObj = new TimsDecomposableParameterCommunicationObject();

                List<Money> amounts = new List<Money>();
                SumAmounts(from factor in factors select (Money)factor.MarginAmount, ref amounts);

                premiumComObj.Factors = factors;

                premiumComObj.MarginAmount = amounts[0];

            }

            return premiumComObj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pContractPositions"></param>
        /// <param name="pContractPositionAction"></param>
        /// <param name="pAdditionalEvalParams"></param>
        /// <param name="pOffset"></param>
        /// <returns></returns>
        /// PM 20170516 [23118][23157] pOffset devient nullable
        private TimsDecomposableParameterCommunicationObject GetAdditionalMargin(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pContractPositions,
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pContractPositionAction,
            IEnumerable<AdditionalEvaluationParameters> pAdditionalEvalParams, decimal? pOffset)
        {
            IEnumerable<TimsFactorCommunicationObject> factorsOrdinary =
                from position in pContractPositions
                join data in pAdditionalEvalParams on position.First.idAsset equals data.AssetID
                where data.PosType == PosType.ALC || data.PosType == PosType.AS || data.PosType == PosType.EX || data.PosType == PosType.DN
                select GetAdditionalContractFactor(position.Second.Quantity, position.Second.ExeAssQuantity, position.First.Side, data);

            IEnumerable<TimsFactorCommunicationObject> factorsPositionAction =
                from position in pContractPositionAction
                join data in pAdditionalEvalParams on position.First.idAsset equals data.AssetID
                where data.PosType == PosType.XM
                select GetAdditionalContractFactor(position.Second.Quantity, 0, position.First.Side, data);

            IEnumerable<TimsFactorCommunicationObject> factors = factorsOrdinary.Union(factorsPositionAction);

            TimsDecomposableParameterCommunicationObject additionalComObj = null;

            if (factors.Count() > 0)
            {
                TimsFactorCommunicationObject sumFactor =
                   new TimsFactorCommunicationObject
                   {
                       RiskArray = (
                           from riskArrayGroup in
                               (
                                   from factor in factors
                                    where factor.PosType != PosType.PA
                                   from sidePoint in factor.RiskArray
                                   group sidePoint by sidePoint.Identifier
                               )
                           select new TimsFactorCommunicationObject
                           {
                               Identifier = riskArrayGroup.Key,
                               MarginAmount = SumAmounts(
                                   from sidePoint in riskArrayGroup
                                   // PM 20170516 [23118][23157] L'offset, si renseigné, s'applique sur tous les montants (positifs et négatifs)
                                   //select (Money)sidePoint.MarginAmount, pOffset, true).First()
                                   select (Money)sidePoint.MarginAmount, pOffset, false).First()
                           }
                       ).ToArray()
                   };

                factors = (factors.Union(new TimsFactorCommunicationObject[] { sumFactor })).ToArray();

                additionalComObj = new TimsDecomposableParameterCommunicationObject
                {
                    Factors = factors
                };
            }

            return additionalComObj;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private TimsFactorCommunicationObject GetAdditionalContractFactor(
            decimal pQuantity, decimal pExeAssQuantity, string pSide, AdditionalEvaluationParameters pParameter)
        {
            TimsFactorCommunicationObject factor = new TimsFactorCommunicationObject();
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal quantity;
            if (pParameter.PosType == PosType.ALC || pParameter.PosType == PosType.XM)
            {
                quantity = pSide == "1" ? (-1) * pQuantity : pQuantity;
            }
            else
            {
                quantity = pExeAssQuantity;
            }

            if (pParameter.PosType == PosType.XM)
            {
                factor.Identifier = pParameter.PositionActionIdentifier;
            }
            else
            {
                factor.AssetId = pParameter.AssetID;
            }

            factor.RiskArray = new TimsFactorCommunicationObject[10];

            factor.RiskArray[0] = new TimsFactorCommunicationObject
            {
                Identifier = "down5",
                Quote = pParameter.DownSide5
            };

            factor.RiskArray[1] = new TimsFactorCommunicationObject
            {
                Identifier = "down4",
                Quote = pParameter.DownSide4
            };

            factor.RiskArray[2] = new TimsFactorCommunicationObject
            {
                Identifier = "down3",
                Quote = pParameter.DownSide3
            };

            factor.RiskArray[3] = new TimsFactorCommunicationObject
            {
                Identifier = "down2",
                Quote = pParameter.DownSide2
            };

            factor.RiskArray[4] = new TimsFactorCommunicationObject
            {
                Identifier = "down1",
                Quote = pParameter.DownSide1
            };

            factor.RiskArray[5] = new TimsFactorCommunicationObject
            {
                Identifier = "up1",
                Quote = pParameter.UpSide1
            };

            factor.RiskArray[6] = new TimsFactorCommunicationObject
            {
                Identifier = "up2",
                Quote = pParameter.UpSide2
            };

            factor.RiskArray[7] = new TimsFactorCommunicationObject
            {
                Identifier = "up3",
                Quote = pParameter.UpSide3
            };

            factor.RiskArray[8] = new TimsFactorCommunicationObject
            {
                Identifier = "up4",
                Quote = pParameter.UpSide4
            };

            factor.RiskArray[9] = new TimsFactorCommunicationObject
            {
                Identifier = "up5",
                Quote = pParameter.UpSide5
            };

            foreach (TimsFactorCommunicationObject sidePoint in factor.RiskArray)
            {
                sidePoint.MarginAmount = new Money(

                    this.RoundAmount(
                    //
                        sidePoint.Quote.Value *
                        quantity *
                        pParameter.Multiplier
                    //
                    )
                    , pParameter.Currency);
            }

            factor.Quantity = quantity;
            factor.Multiplier = pParameter.Multiplier;
            factor.PosType = pParameter.PosType == PosType.XM && !pParameter.CrossMargin ? PosType.PA : pParameter.PosType;

            return factor;
        }

        private TimsDecomposableParameterCommunicationObject GetMinimumMargin
            (IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pClassPositions,
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pClassPositionAction,
            IEnumerable<MinimumEvaluationParameters> pMinimumEvalParameters, IMoney pPremiumAmount)
        {

            AppInstance.TraceManager.TraceVerbose(this, String.Format("Start GetMinimumMargin {0}","GetMinimumMarginFactors"));
            List<TimsFactorCommunicationObject> factors = 
                GetMinimumMarginFactors(pClassPositions, pClassPositionAction, pMinimumEvalParameters);
            AppInstance.TraceManager.TraceVerbose(this, String.Format("End GetMinimumMargin {0}", "GetMinimumMarginFactors"));

            IEnumerable<TimsFactorCommunicationObject> factorsFuture =
                from factor in factors
                where (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.Future))
                   || (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.FutureMoff))
                select factor;

            List<TimsFactorCommunicationObject> factorsAction = (
                from factor in factors
                where (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.PositionAction))
                select factor).ToList();

            IEnumerable<TimsFactorCommunicationObject> factorsCall =
                from factor in factors
                where (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.Call))
                   || (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.ExeAssCall))
                select factor;

            IEnumerable<TimsFactorCommunicationObject> factorsPut =
                from factor in factors
                where (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.Put))
                   || (factor.Identifier == System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.ExeAssPut))
                select factor;

            // Abs( Sum( Montant Minimum Future ) ) + Abs( Sum( Montant Minimum Action ) )
            IEnumerable<IMoney> factorAmounts = (
                from factor in factorsFuture
                group factor by factor.MarginAmount.Currency into factorCur
                select (IMoney) new Money( RoundAmount(System.Math.Abs(factorCur.Sum( f => f.MarginAmount.Amount.DecValue ))), factorCur.Key )
                ).Concat(
                from factor in factorsAction
                group factor by factor.MarginAmount.Currency into factorCur
                select (IMoney)new Money(RoundAmount(System.Math.Abs(factorCur.Sum(f => f.MarginAmount.Amount.DecValue))), factorCur.Key)
                );

            // Abs( Sum( Montant Minimum Call ) ) + Abs( Sum( Montant Minimum Put ) )
            IEnumerable<IMoney> factorOptionAmounts = (
                from factor in factorsCall
                group factor by factor.MarginAmount.Currency into factorCur
                select (IMoney) new Money( RoundAmount(System.Math.Abs(factorCur.Sum( f => f.MarginAmount.Amount.DecValue ))), factorCur.Key )
                ).Concat(
                from factor in factorsPut
                group factor by factor.MarginAmount.Currency into factorCur
                select (IMoney) new Money( RoundAmount(System.Math.Abs(factorCur.Sum( f => f.MarginAmount.Amount.DecValue ))), factorCur.Key )
                );

            bool bUsePremiumAmount = false;

            // using premium amount when the premium amount is a credit and the minimum amount for option is greater than the premium amount
            if (pPremiumAmount == null || pPremiumAmount.Amount.DecValue <= 0)
            {
                decimal optionMinAmounts = factorOptionAmounts.Sum(a => a.Amount.DecValue);

                if ( (pPremiumAmount == null && optionMinAmounts > 0) 
                   || (pPremiumAmount != null && optionMinAmounts > System.Math.Abs(pPremiumAmount.Amount.DecValue)))
                {
                    bUsePremiumAmount = true;
                }
            }

            if (bUsePremiumAmount)
            {
                if (pPremiumAmount != null)
                {
                    factorAmounts = factorAmounts.Concat(new IMoney[] { new Money(System.Math.Abs(pPremiumAmount.Amount.DecValue), pPremiumAmount.Currency) });
                }
            }
            else
            {
                factorAmounts = factorAmounts.Concat(factorOptionAmounts);
            }

            IEnumerable<Money> amounts = SumAmounts(from amount in factorAmounts select (Money)amount, null);

            TimsDecomposableParameterCommunicationObject minimumComObj = new TimsDecomposableParameterCommunicationObject
            {
                Factors = factors
            };
            // One currency only is attended for a minimum margin (taking the first amount)
            // PM 20130528 Vérification qu'il y ait au moins un montant
            if (amounts.Count() != 0)
            {
                minimumComObj.MarginAmount = amounts.First();
            }
            //else
            //{
            //    minimumComObj.MarginAmount = new Money();
            //}
            return minimumComObj;
        }

        private List<TimsFactorCommunicationObject> GetMinimumMarginFactors(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pClassPositions, 
            IEnumerable<Pair<PosActionRiskMarginKey, RiskMarginPositionAction>> pClassPositionAction, 
            IEnumerable<MinimumEvaluationParameters> pMinimumEvalParameters)
        {
            List<TimsFactorCommunicationObject> factor = null;

            var posCategoryAndMinimum = (
                from position in pClassPositions
                join data in pMinimumEvalParameters on position.First.idAsset equals data.AssetID
                select new
                {
                    EvaluationData = data,
                    Position = position
                }).GroupBy(key => new { key.EvaluationData.Type, key.EvaluationData.MinimumRate } );

            var posActionCategoryAndMinimum = (
                from position in pClassPositionAction
                join data in pMinimumEvalParameters on position.First.idAsset equals data.AssetID
                where data.Type == RiskMethodQtyType.PositionAction
                select new
                {
                    EvaluationData = data,
                    Position = position
                }).GroupBy(key => new { key.EvaluationData.Type, key.EvaluationData.MinimumRate } );

            AppInstance.TraceManager.TraceVerbose(this, String.Format("Start GetMinimumMarginFactors {0}", "Set factor"));

            RiskMethodQtyType[] method = new RiskMethodQtyType[] { RiskMethodQtyType.Future, RiskMethodQtyType.Call, RiskMethodQtyType.Put };

            factor = (
                from positionsByCategory in posCategoryAndMinimum
                select new TimsFactorCommunicationObject
                {
                    Identifier = System.Enum.GetName(typeof(RiskMethodQtyType), positionsByCategory.Key.Type),

                    MinimumRate = positionsByCategory.Key.MinimumRate,

                    Quantity =

                        (positionsByCategory.Key.Type == RiskMethodQtyType.Future
                        || positionsByCategory.Key.Type == RiskMethodQtyType.Call
                        || positionsByCategory.Key.Type == RiskMethodQtyType.Put)

                        ?

                        (from elem in positionsByCategory where elem.Position.First.Side == SideTools.RetSellFIXmlSide() select elem.Position.Second.Quantity).Sum()
                         -
                        (from elem in positionsByCategory where elem.Position.First.Side == SideTools.RetBuyFIXmlSide() select elem.Position.Second.Quantity).Sum()
                        :
                        // the input position grouped by series have an exe/ass quantity value already provided by sign (ass positive, exe negative)
                        (from elem in positionsByCategory select elem.Position.Second.ExeAssQuantity).Sum(),

                    MarginAmount = new Money(0, (from elem in positionsByCategory select elem.EvaluationData.Currency).First())
                })
                .Union(
                    from positionsByCategory in posActionCategoryAndMinimum
                    select new TimsFactorCommunicationObject
                    {
                        Identifier = (from elem in positionsByCategory select elem.EvaluationData.CrossMargin).First() ?
                            System.Enum.GetName(typeof(RiskMethodQtyType), RiskMethodQtyType.PositionAction) : "NoCrossMargin",

                        MinimumRate = positionsByCategory.Key.MinimumRate,

                        Quantity =

                            (from elem in positionsByCategory where elem.Position.First.Side == SideTools.RetSellFIXmlSide() select elem.Position.Second.Quantity).Sum()
                             -
                            (from elem in positionsByCategory where elem.Position.First.Side == SideTools.RetBuyFIXmlSide() select elem.Position.Second.Quantity).Sum(),

                        MarginAmount = new Money(0, (from elem in positionsByCategory select elem.EvaluationData.Currency).First())
                    }
                ).ToList();

            AppInstance.TraceManager.TraceVerbose(this, String.Format("Stop GetMinimumMarginFactors {0}", "Set factor"));

            factor.ForEach(f => f.MarginAmount.Amount.DecValue = (int)f.Quantity * (decimal)f.MinimumRate);

            return factor;
        }

        private TimsDecomposableParameterCommunicationObject AggregateMargin
            (IEnumerable<TimsDecomposableParameterCommunicationObject> pDecomposableParameters)
        {

            pDecomposableParameters = from parameter in pDecomposableParameters where parameter != null select parameter;

            if (pDecomposableParameters.Count() == 0)
            {
                return null;
            }

            List<Money> amounts = new List<Money>();
            SumAmounts(from factor in pDecomposableParameters select (Money)factor.MarginAmount, ref amounts);

            TimsDecomposableParameterCommunicationObject decomposableComObj = new TimsDecomposableParameterCommunicationObject();

            //PM 20130528 Ajout vérification que la liste à bien au moin 1 élément
            if (amounts.Count != 0)
            {
                decomposableComObj.MarginAmount = amounts[0];
            }

            return decomposableComObj;
        }

        // RD 20160114 [21722] Ajouter une surcharge avec pWithMarginAmount
        private TimsDecomposableParameterCommunicationObject AggregateAdditionalMargin
            (IEnumerable<TimsDecomposableParameterCommunicationObject> pDecomposableParameters)
        {
            return AggregateAdditionalMargin(pDecomposableParameters, true);
        }

        private TimsDecomposableParameterCommunicationObject AggregateAdditionalMargin
            (IEnumerable<TimsDecomposableParameterCommunicationObject> pDecomposableParameters, bool pWithMarginAmount)
        {

            pDecomposableParameters = from parameter in pDecomposableParameters where parameter != null select parameter;

            int howManyParams = pDecomposableParameters.Count();

            if (howManyParams <= 0)
            {
                return null;
            }

            TimsDecomposableParameterCommunicationObject decomposableComObj = new TimsDecomposableParameterCommunicationObject();

            IEnumerable<TimsFactorCommunicationObject> riskArray =
                from riskArrayGroup in
                    (
                        from subParameter in pDecomposableParameters
                        from factor in subParameter.Factors
                        where !factor.PosType.HasValue
                        from sidePoint in factor.RiskArray
                        group sidePoint by sidePoint.Identifier)
                select new TimsFactorCommunicationObject
                {
                    Identifier = riskArrayGroup.Key,
                    MarginAmount = SumAmounts(
                        from sidePoint in riskArrayGroup
                        select (Money)sidePoint.MarginAmount, null).First()
                };

            // RD 20160114 [21722] Ajouter le test sur pWithMarginAmount
            if (pWithMarginAmount)
            {
                decomposableComObj.MarginAmount =
                    new Money
                        (
                            (from sidePoint in riskArray select sidePoint.MarginAmount.Amount.DecValue).Max(),
                            (from sidePoint in riskArray select sidePoint.MarginAmount.Currency).First()
                        );
            }

            decomposableComObj.Factors =
                new TimsFactorCommunicationObject[] 
                { 
                    new TimsFactorCommunicationObject 
                    { 
                        RiskArray = riskArray.ToArray() 
                    } 
                };

            return decomposableComObj;
        }

        /// <summary>
        /// Cumul les matrices de risques d'une Class à partir des matrices de risques des différents Contracts qui la compose
        /// </summary>
        /// <param name="pAdditionalParameters">Ensemble des valeurs des matrices des contracts de la class sans application de l'offset de chaque contract</param>
        /// <param name="pAdditionalWithOffsetParameters">Ensemble des valeurs des matrices des contracts de la class avec application de l'offset de chaque contract</param>
        /// <returns></returns>
        /// PM 20170516 [23118][23157] Nouvelle méthode
        private TimsDecomposableParameterCommunicationObject AggregateClassAdditionalMargin
            (IEnumerable<TimsDecomposableParameterCommunicationObject> pAdditionalParameters,
            IEnumerable<TimsDecomposableParameterCommunicationObject> pAdditionalWithOffsetParameters)
        {
            TimsDecomposableParameterCommunicationObject additionalComObj = default;

            if ((pAdditionalParameters != default(IEnumerable<TimsDecomposableParameterCommunicationObject>)) && (pAdditionalParameters.Count() > 0)
                && (pAdditionalWithOffsetParameters != default(IEnumerable<TimsDecomposableParameterCommunicationObject>)) && (pAdditionalWithOffsetParameters.Count() > 0))
            {
                // Ne prendre que les éléments non null des ensembles de données
                pAdditionalParameters = from parameter in pAdditionalParameters where parameter != null select parameter;
                pAdditionalWithOffsetParameters = from parameter in pAdditionalWithOffsetParameters where parameter != null select parameter;

                if ((pAdditionalParameters.Count() > 0) && (pAdditionalWithOffsetParameters.Count() > 0))
                {
                    additionalComObj = new TimsDecomposableParameterCommunicationObject();

                    // Groupement des matrices sans offset
                    var classSidePoint =
                        from subParameter in pAdditionalParameters
                        from factor in subParameter.Factors
                        where (false == factor.PosType.HasValue)
                        from sidePoint in factor.RiskArray
                        group sidePoint by sidePoint.Identifier;

                    // Groupement des matrices avec offset
                    var classSidePointWithOffset =
                        from subParameter in pAdditionalWithOffsetParameters
                        from factor in subParameter.Factors
                        where (false == factor.PosType.HasValue)
                        from sidePoint in factor.RiskArray
                        group sidePoint by sidePoint.Identifier;

                    // Calcul des montants additionals avec et sans offset
                    var additionalAmount =
                        from riskArrayGroup in classSidePoint
                        join riskArrayGroupOffset in classSidePointWithOffset on riskArrayGroup.Key equals riskArrayGroupOffset.Key
                        select new
                        {
                            Identifier = riskArrayGroup.Key,
                            MarginAmount = SumAmounts(from sidePoint in riskArrayGroup select (Money)sidePoint.MarginAmount, null).First(),
                            MarginAmountOffset = SumAmounts(from sidePoint in riskArrayGroupOffset select (Money)sidePoint.MarginAmount, null).First()
                        };

                    // Prendre les montants additionals sans offset dont les valeurs sont positives et les montants additionals avec offset dont les valeurs sont négatives
                    IEnumerable<TimsFactorCommunicationObject> riskArray = 
                        from additional in additionalAmount
                        select new TimsFactorCommunicationObject
                        {
                            Identifier = additional.Identifier,
                            MarginAmount = (additional.MarginAmount.amount.DecValue < 0) ? additional.MarginAmountOffset : additional.MarginAmount
                        };

                    additionalComObj.Factors = new TimsFactorCommunicationObject[] 
                    { 
                        new TimsFactorCommunicationObject 
                        { 
                            RiskArray = riskArray.ToArray() 
                        } 
                    };
                }
            }
            return additionalComObj;
        }

        private IMoney GetProductMarginAmount
            (IMoney pMtmAmount, IMoney pSpreadAmount, IMoney pPremiumAmount, IMoney pAdditionalAmount, IMoney pMinimumAmount)
        {
            List<Money> amounts = new List<Money>();
            Money retAmount = default;

            if (pMtmAmount != null)
            {
                amounts.Add((Money)pMtmAmount);
            }

            if (pSpreadAmount != null)
            {
                amounts.Add((Money)pSpreadAmount);
            }

            if (pPremiumAmount != null)
            {
                amounts.Add((Money)pPremiumAmount);
            }

            if (pAdditionalAmount != null || pMinimumAmount != null)
            {
                if (pMinimumAmount == null)
                {
                    amounts.Add((Money)pAdditionalAmount);
                }
                else if (pAdditionalAmount == null)
                {
                    amounts.Add((Money)pMinimumAmount);
                }
                else if (pAdditionalAmount.Amount.DecValue > pMinimumAmount.Amount.DecValue)
                {
                    amounts.Add((Money)pAdditionalAmount);
                }
                else
                {
                    amounts.Add((Money)pMinimumAmount);
                }
            }

            // PM 20130528 Ajout vérification qu'il y ait bien au moins un montant
            if (amounts.Count != 0)
            {
                retAmount = SumAmounts(amounts, null).First();
            }
            return retAmount;
        }
        #endregion methods
    }
}

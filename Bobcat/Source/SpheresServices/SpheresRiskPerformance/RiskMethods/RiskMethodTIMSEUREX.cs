using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient.LoggerService;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
//
using EfsML.Enum;
//
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class representing the "TIMS EUREX" Risk method, as used by the EUREX AG german clearing house (EUREX derivative market)
    /// </summary>
    public sealed class TimsEUREXMethod : BaseMethod
    {
        #region constante
        private const string cm_MaxUnadjusted = "Risk";
        private const string cm_MaxUnadjustedNeutralPrices = "RiskWithNeutralPrices";
        private const string cm_MaxAdjusted = "RiskWithNeutralPricesAndOffset";

        // Constante                           Nouvelle Valeur                      Ancienne valeur
        // cm_MaxUnadjusted                    "Risk"                               "MaxUnadjusted"
        // cm_MaxUnadjustedNeutralPrices       "RiskWithNeutralPrices"              "MaxUnadjustedNeutralPrices"
        // cm_MaxAdjusted                      "RiskWithNeutralPricesAndOffset"     "MaxAdjusted"

        #endregion

        #region constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        internal TimsEUREXMethod()
        {
            // PM 20170313 [22833] Ajout alimentation de m_RiskMethodDataType
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }
        #endregion Constructor

        # region evaluation parameters structs

        /// <summary>
        /// this structure aggregates all the data needed by the evaluation of the premium/liquidating margin.
        /// For the premium, Any instance is related to an option (premium-style) asset.
        /// For the liquidating, Any instance is related to any asset.
        /// </summary>
        /// <remarks>
        /// the premium formule is : quantity * multiplier * [closing price(Put, call)/ITM amount(ExeAssPut, ExeAssCall)].
        /// the liquidating formule is : quantity * multiplier * closing price</remarks>
        private struct PremiumLiquidatingEvaluationParameters
        {
            /// <summary>
            /// identify the net by asset position
            /// </summary>
            public int AssetId;

            /// <summary>
            /// Position type: for the premium we have to consider Put, call, ExeAssPut, ExeAssCall
            /// </summary>
            public RiskMethodQtyType Type;

            /// <summary>
            /// True, for standard options having no derivative contracts (Futures) as underlyings (IOW "Future style options")
            /// </summary>
            public bool PremiumStyle;

            /// <summary>
            /// Containing the closing price of the asset
            /// </summary>
            public decimal Quote;

            /// <summary>
            /// Asset exercise price, used to compute the ITM/OTM amount together with the underlying quote. 
            /// </summary>
            public decimal StrikePrice;

            /// <summary>
            /// Containing the theoretical value of the asset underlying,
            /// used to compute the ITM/OTM amount together with the exercise price.
            /// </summary>
            public decimal UnlQuote;

            /// <summary>
            /// Asset muliplier, 
            /// </summary>
            public decimal Multiplier;

            /// <summary>
            /// Derivative contract currency
            /// </summary>
            public string Currency;

        }

        /// <summary>
        /// this structure aggregates all the data needed by the evaluation of the spread margin.
        /// Any instance is related to a Future asset.
        /// </summary>
        private struct SpreadEvaluationParameters
        {
            /// <summary>
            /// Id of the future asset
            /// </summary>
            public int AssetId;

            /// <summary>
            /// Position type: for the spread we have to consider Future or FutureMoff
            /// </summary>
            public RiskMethodQtyType Type;

            /// <summary>
            /// Maturity of the ETD asset, given in the format AAAAMM according to the EUREX market specification. 
            /// <list type="">
            /// <listheader>The maturity rule has a fixed format [AAMM] as given in the FPPARA file specification:</listheader>
            /// <item>EXPI-YR-DAT-RMPARA PIC 9(2)</item>
            /// <item>EXPI-MTH-DAT-RMPARA PIC 9(2)</item>
            /// </list>
            /// </summary>
            public decimal MaturityYearMonth;

            /// <summary>
            /// Currency of the future contract
            /// </summary>
            public string Currency;

            /// <summary>
            /// Maturity Rule Frequency
            /// </summary>
            public string MaturityRuleFrequency;
        }

        /// <summary>
        /// this class aggregates all the data needed by the evaluation of the additional margin.
        /// </summary>
        /// <remarks>Use class type base in order to preserve performances during the short option compensation procedure</remarks>
        private class AdditionalEvaluationParameters
        {
            /// <summary>
            /// identify the net by asset position
            /// </summary>
            public int AssetId;

            /// <summary>
            /// Position type: for the additional we have to consider Put, call, ExeAssPut, ExeAssCall, Future, FutureMoff
            /// </summary>
            public RiskMethodQtyType Type;

            /// <summary>
            /// Index of the array/scenario 
            /// </summary>
            /// <value>from 1 to max 29</value>
            public int RiskArrayIndex;

            /// <summary>
            /// Indicator whether the projected underlying price is lesser, equals or greater than the closing price
            /// </summary>
            /// <value>D -> downside ; N -> neutral ; U -> Upside</value>
            public string QuoteUnlVsQuote_Indicator;

            /// <summary>
            /// Indicator for the projected underlying price 
            /// </summary>
            /// <value>0 -> pure volatility bucket ; 1 -> zero price movement ; 2 ->  underlying price ; 3 ->  in-between strike </value>
            public int QuoteUnlIndicator;

            /// <summary>
            /// volatility when the projected underlying price is greater than the closing price
            /// </summary>
            public decimal UpVolatility;

            /// <summary>
            /// Risk value/ Margin parameter when the projected underlying price is greater than the closing price
            /// </summary>
            public decimal UpTheoreticalValue;

            /// <summary>
            /// Price Up Risk value/ Margin exists
            /// </summary>
            public bool UpTheoreticalExists;

            /// <summary>
            /// Short option adjustement when the projected underlying price is greater than the closing price
            /// </summary>
            public decimal UpShortAdj;

            /// <summary>
            /// volatility when the projected underlying price is equals than the closing price
            /// </summary>
            public decimal NtrlVolatility;

            /// <summary>
            /// Risk value/ Margin parameter when the projected underlying price is equals than the closing price
            /// </summary>
            public decimal NtrlTheoreticalValue;

            /// <summary>
            /// Price Neutral Risk value/ Margin exists
            /// </summary>
            public bool NtrlTheoreticalExists;

            /// <summary>
            /// Short option adjustement when the projected underlying price is equals than the closing price
            /// </summary>
            public decimal NtrlShortAdj;

            /// <summary>
            /// volatility when the projected underlying price is lesser than the closing price
            /// </summary>
            public decimal DownVolatility;

            /// <summary>
            /// Risk value/ Margin parameter when the projected underlying price is lesser than the closing price
            /// </summary>
            public decimal DownTheoreticalValue;

            /// <summary>
            /// Price Down Risk value/ Margin exists
            /// </summary>
            public bool DownTheoreticalExists;

            /// <summary>
            /// Short option adjustement when the projected underlying price is lesser than the closing price
            /// </summary>   
            public decimal DownShortAdj;

            /// <summary>
            /// Exercise Assignment adjustement
            /// </summary>   
            public decimal RiskValueExeAss;

            /// <summary>
            /// Asset muliplier, 
            /// </summary>
            public decimal Multiplier;

            /// <summary>
            /// Maturity of the ETD asset, given in the format AAAAMM according to the EUREX market specification. 
            /// <list type="">
            /// <listheader>The maturity rule has a fixed format [AAMM] as given in the FPPARA file specification:</listheader>
            /// <item>EXPI-YR-DAT-RMPARA PIC 9(2)</item>
            /// <item>EXPI-MTH-DAT-RMPARA PIC 9(2)</item>
            /// </list>
            /// </summary>
            public decimal MaturityYearMonth;

            /// <summary>
            /// Maturity switch activation status. 
            /// When activated (value: Y) the maturity factor enters the risk evaluation process for the current derivative contract.
            /// </summary>
            /// <value>Y -> Yes, N -> Not</value>
            public MaturitySwitch MaturitySwitch;

            /// <summary>
            /// Used to adjust the max additional amount (worst evaluated scenario) 
            /// when the maturity switch (MaturitySwitch) is activated for the current parameter
            /// </summary>
            public decimal MaturityFactor;

            /// <summary>
            /// Currency of the future contract
            /// </summary>
            public string Currency;
        }

        #endregion evaluation parameters structs

        # region private margin structs and classes

        /// <summary>
        /// Struct used to perform the spreading among two maturity months
        /// </summary>
        private class SpreadElement
        {
            /// <summary>
            /// Maturity of the ETD asset, given in the format AAAAMM according to the EUREX market specification. 
            /// <list type="">
            /// <listheader>The maturity rule has a fixed format [AAMM] as given in the FPPARA file specification:</listheader>
            /// <item>EXPI-YR-DAT-RMPARA PIC 9(2)</item>
            /// <item>EXPI-MTH-DAT-RMPARA PIC 9(2)</item>
            /// </list>
            /// </summary>
            public decimal MaturityYearMonth;

            /// <summary>
            /// quantity related to this maturity element
            /// </summary>
            // EG 20150920 [21374] Int (int32) to Long (Int64)  
            // EG 20170127 Qty Long To Decimal
            public decimal Quantity;

            /// <summary>
            /// side of the spread quantity
            /// </summary>
            /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag54.html</remarks>
            public string Side;

            /// <summary>
            /// Currency of the related future contract
            /// </summary>
            public string Currency;

            /// <summary>
            /// decrementing quantity according to the possible spreads among other maturity elements
            /// </summary>
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            public decimal QuantityToSpread;

            /// <summary>
            /// Maturity Rule Frequency
            /// </summary>
            public string MaturityRuleFrequency;
        }

        /// <summary>
        /// Struct used to adjust the max additional risk value inside of a specific margin class
        /// </summary>
        // EG 20160404 Migration vs2013
        //private struct MarginFactorElement
        //{
        //    /// <summary>
        //    /// Contract symbol, used to identify the MarginFactorElement (with MaturityYearMonth)
        //    /// </summary>
        //    public string ContractSymbol;

        //    /// <summary>
        //    /// Expiry code, used to identify the MarginFactorElement (with ContractSymbol)
        //    /// </summary>
        //    public decimal MaturityYearMonth;

        //    /// <summary>
        //    /// Maturity factor.
        //    /// </summary>
        //    public decimal MaturityFactor;
        //}

        /// <summary>
        /// Struct used during the short option compensation.
        /// When short option positions are embedded in complex portfolios, 
        /// a major part of the risk may be compensated either by long option positions or by corresponding future positions. 
        /// For the uncompensated part of short options positions, short option minimum is used. 
        /// This cross margining is provided within a margin class
        /// </summary>
        private class Compensation
        {
            /// <summary>
            /// identify the compensated/compensating asset
            /// </summary>
            public int AssetId;

            /// <summary>
            /// Position type: for the short option compensation we have to consider Put, Call, Future
            /// </summary>
            public RiskMethodQtyType Type;

            /// <summary>
            /// Asset muliplier, 
            /// </summary>
            public decimal Multiplier;

            /// <summary>
            /// Maturity of the ETD asset, given in the format AAAAMM according to the EUREX market specification. 
            /// </summary>
            public decimal MaturityYearMonth;

            // UNDONE MF - remplacer le type string du side par une énumération
            /// <summary>
            /// Dealer side of the trade/position 
            /// </summary>
            /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag54.html</remarks>
            public string Side;

            /// <summary>
            /// Asset exercise price, used to compute the ITM/OTM amount together with the underlying quote. 
            /// </summary>
            public decimal StrikePrice;

            /// <summary>
            /// decrementing compensated/compensating quantity
            /// </summary>
            public decimal CompensationQuantity;

            /// <summary>
            /// decrementing compensated/compensating quantity
            /// </summary>
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            public decimal Quantity;

            /// <summary>
            /// incrementing compensated quantity (incremented just for compensated short options)
            /// </summary>
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            public decimal CompensatedQuantity;
        }

        #endregion private margin structs and classes

        # region market parameters

        private IEnumerable<ContractParameterTimsEurex> m_ContractParameters;

        private IEnumerable<MaturityParameterTimsEurex> m_MaturityParameters;

        private IEnumerable<AssetParameterTimsEurex> m_AssetParameters;

        private IEnumerable<VolatilityParameterTimsEurex> m_VolatilityParameters;

        private IEnumerable<FxRateTimsEurex> m_ExchangeRates;
        #endregion market parameters

        #region override base accessors
        /// <summary>
        /// Get the TIMS EUREX type
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.TIMS_EUREX; }
        }

        /// <summary>
        /// Requête utilisée pour connaître l'existance de paramètres de risque pour une date donnée
        /// <remarks>Utilise les paramètres DTBUSINESS & SESSIONID</remarks>
        /// </summary>
        /// PM 20150511 [20575] Add QueryExistRiskParameter
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        protected override string QueryExistRiskParameter
        {
            get
            {
                string query;
                query = @"
                    select distinct 1
                      from dbo.PARAMSEUREX_ASSETETD ae
                     inner join dbo.PARAMSEUREX_MATURITY m on (m.IDPARAMSEUREX_MATURITY = ae.IDPARAMSEUREX_MATURITY)
                     inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = ae.IDASSET)
                     where (m.DTMARKET = @DTBUSINESS)";
                return query;
            }
        }
        #endregion

        /// <summary>
        /// Load specific EUREX parameters.
        /// <list type="">
        /// <listheader>Parameters:</listheader>
        /// <item>contract parameters, defining the EUREX contract hierarchy</item>
        /// </list>
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pAssetETDCache">assets collection containing all the assets in position</param>
        // EG 20180307 [23769] Gestion dbTransaction
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            if (pAssetETDCache.Count > 0)
            {
                Dictionary<string, object> dbParameterValues = new Dictionary<string, object>();

                // Just the date part of the datetime business parameter is passed to the request 
                // (in case an intraday generation hass been launched). 
                // Only EOD market files are imported. 
                // the market data of the integrated datas is at midnight and the SQL WHERE condition use the equals operator.
                // PM 20150511 [20575] Ajout gestion dtMarket 
                //dbParameterValues.Add("DTBUSINESS", this.DtBusiness.Date);
                DateTime dtBusiness = GetRiskParametersDate(pCS);
                dbParameterValues.Add("DTBUSINESS", dtBusiness);

                // 20120712 MF Ticket 18004 contract id vector suppression, using IMASSET column IDDC

                using (IDbConnection connection = DataHelper.OpenConnection(pCS))
                {
                    // 1 load the contract parameters (hierarchy elements)
                    m_ContractParameters = LoadContractParameters(connection);

                    // 2 load maturity parameters (maturity factor and maturity rates)
                    m_MaturityParameters = LoadMaturityParameters(connection, dbParameterValues);

                    // 3 load asset/volatility parameters
                    m_AssetParameters = LoadAssetParameters(connection, dbParameterValues);

                    // Limiter les paramètres de la méthode aux assets reçus en paramètre
                    m_AssetParameters = from assetParam in m_AssetParameters
                                        join asset in pAssetETDCache on assetParam.AssetId equals asset.Key
                                        select assetParam;

                    // 3.1 finalize the asset parameters using datas from the input Spheres asset collection
                    foreach (AssetParameterTimsEurex param in m_AssetParameters)
                    {
                        // UNDONE Currently not used, using parameter data directly
                        // EG 20160404 Migration vs2013
                        //param.UnlAssetQuote = 0;
                        // UNDONE Currently not used , using parameter data directly
                        // EG 20160404 Migration vs2013
                        //param.AssetQuote = 0;

                        param.Strike = pAssetETDCache[param.AssetId].StrikePrice;
                        param.Multiplier = pAssetETDCache[param.AssetId].ContractMultiplier > 0 ?
                            pAssetETDCache[param.AssetId].ContractMultiplier
                            :
                            ComputeMultiplierByParameter(
                                m_ContractParameters.FirstOrDefault(
                                    elem => elem.ContractId == pAssetETDCache[param.AssetId].IdDerivativeContract),
                                param.TradeUnit);
                        // it will be different than 0 when the contract underlying is kind of Futures
                        param.UnlAssetId = pAssetETDCache[param.AssetId].DrvAttrib_IdAssetUnl;

                    }

                    // 4 load volatility/risk array parameters

                    m_VolatilityParameters = LoadVolatilityParameters(connection, dbParameterValues);

                    // 5 load fx rates

                    if (!String.IsNullOrEmpty(this.m_CssCurrency))
                    {
                        dbParameterValues.Add("CSSCURRENCY", this.m_CssCurrency);

                        m_ExchangeRates = LoadExchangeRates(connection, dbParameterValues);
                    }
                    else
                    {
                        m_ExchangeRates = new List<FxRateTimsEurex>();

                        // UNDONE log in the calculation sheet the missing reference currency
                    }
                }

                // RD 20161102 [22369] param.Currency est écrasée par la devise cotée (cas du GBX qui est remplacé par GBP)
                foreach (ContractParameterTimsEurex param in m_ContractParameters)
                {
                    if (!String.IsNullOrEmpty(param.Currency))
                    {
                        EfsML.Business.Tools.GetQuotedCurrency(CSTools.SetCacheOn(pCS), null, SQL_Currency.IDType.IdC, param.Currency, out string idcQuoted, out int? factor);
                        if (StrFunc.IsFilled(idcQuoted))
                            param.Currency = idcQuoted;
                    }
                }
            }
            else
            {
                InitEmptyCollections();
            }

        }

        /// <summary>
        /// Evaluate a deposit item (deposit factor), according with the parameters of the TIMS EUREX method
        /// </summary>
        /// <param name="pActorId">the actor owning the positions set</param>
        /// <param name="pBookId">the book where the positions set has been registered</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="pRiskDataToEvaluate">the positions to evaluate the partial amount for the current deposit item</param>
        /// <param name="opMethodComObj">output value containing all the datas to pass to the calculation sheet repository object
        /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository.BuildTimsEurexMarginCalculationMethod"/>) 
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
        public override List<Money> EvaluateRiskElementSpecific(
            int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
            RiskData pRiskDataToEvaluate,
            out IMarginCalculationMethodCommunicationObject opMethodComObj)
        {
            List<Money> amounts = new List<Money>();

            // PM 20170313 [22833] Prendre uniquement la position (à l'ancien format)
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsToEvaluate = pRiskDataToEvaluate.GetPositionAsEnumerablePair();

            #region Constitution de la position nécessaire au calcul du Déposit TIMS-EUREX V13
            // PM 20130910 [18946] Ne pas prendre les positions en livraison si le sous-jacent n'est pas un Equity
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> filteredPosition =
                from position in positionsToEvaluate
                join asset in m_AssetParameters on position.First.idAsset equals asset.AssetId
                join contract in m_ContractParameters on asset.ContractId equals contract.ContractId
                where (position.Second.Quantity != 0)
                   || (((contract.UnlCategory == Cst.UnderlyingAsset.EquityAsset) ? position.Second.ExeAssQuantity : 0) != 0)
                select new Pair<PosRiskMarginKey, RiskMarginPosition>
                {
                    First = position.First,
                    Second =
                    new RiskMarginPosition
                    {
                        TradeIds = position.Second.TradeIds,
                        Quantity = position.Second.Quantity,
                        // Equity uniquement
                        ExeAssQuantity = (contract.UnlCategory == Cst.UnderlyingAsset.EquityAsset) ? position.Second.ExeAssQuantity : 0,
                        DeliveryDate = position.Second.DeliveryDate,
                        SettlementDate = position.Second.SettlementDate,
                        DeliveryQuantity = position.Second.DeliveryQuantity,
                        DeliveryStep = position.Second.DeliveryStep,
                        DeliveryStepDate = position.Second.DeliveryStepDate,
                    }
                };

            // Début de constitution de la position NET nécessaire au calcul du Déposit TIMS-EUREX V13.
            // Cette position inclus les Exercices et Assignations d’options
            //  Remarque: La quantité exercé/assigné sera négatif dans le cas de la quantité exercé est supérieure à celle attribuée
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedPositionsByIdAsset =
                PositionsGrouping.GroupPositionsByAsset(filteredPosition);

            // PM 20131021 [19023][19078] : Ne garder que les positions dont la quantité est différente de 0
            groupedPositionsByIdAsset =
                from pos in groupedPositionsByIdAsset
                where (pos.Second.Quantity != 0) || (pos.Second.ExeAssQuantity != 0)
                select pos;

            // Réduction des positions vendeuses d’Option CALL (Short CALL) et/ou  vendeuse de Future par les positions action déposées en garantie. 
            //  Remarque: Cette réduction de position est commune pour toutes les méthodes de calcul de déposit (TIMS-IDEM, TIMS-EUREX, SPAN….) 
            //     et ce règle via un paramètre définit dans le référentiel Marché (Réduction de position) qui peut prendre les valeurs suivante 
            //      (Stock Future, Stock Option, Priorité Stock Future, Priorité Stock Option)
            IEnumerable<CoverageSortParameters> inputCoverage = GetSortParametersForCoverage(groupedPositionsByIdAsset);

            // FI 20160613 [22256]
            Pair<IEnumerable<StockCoverageCommunicationObject>, IEnumerable<StockCoverageDetailCommunicationObject>> coveredQuantities =
                ReducePosition(pActorId, pBookId, pDepositHierarchyClass, inputCoverage, ref groupedPositionsByIdAsset);
            #endregion

            // Création de l'objet de LOG relatif au calcul du déposit
            TimsEurexMarginCalculationMethodCommunicationObject methodComObj = new TimsEurexMarginCalculationMethodCommunicationObject();

            opMethodComObj = methodComObj;

            // Set the css currency (used to convert the additional group margin amounts, when not null or vide)
            methodComObj.CssCurrency = this.m_CssCurrency;
            //PM 20150511 [20575] Ajout date des paramètres de risque
            methodComObj.DtParameters = DtRiskParameters;

            #region Chargement de tout les paramètres nécessaires au différent montant à calculer dans la méthode TIMS-EUREX V13 pour la position ouverte considérée
            // Premium margin - Liquidating Margin
            IEnumerable<PremiumLiquidatingEvaluationParameters> premiumLiquidatingEvalParams =
                GetPremiumLiquidatingEvaluationParameters(groupedPositionsByIdAsset);

            // Spread Margin
            IEnumerable<SpreadEvaluationParameters> spreadingEvalParam =
                GetSpreadingEvaluationParameters(groupedPositionsByIdAsset);

            // Additional Margin
            IEnumerable<AdditionalEvaluationParameters> additionalEvalParam =
                GetAdditionalEvaluationParameters(groupedPositionsByIdAsset);
            #endregion

            // Constitution de la liste des Products/Classes/Contract nécessaires pour évaluer les positions établies, les remplir avec les positions y afférentes prévus
            methodComObj.Parameters = GetGroupClassHierarchy(groupedPositionsByIdAsset, coveredQuantities.First).ToArray();
            // FI 20160613 [22256] alimentation de UnderlyingStock
            methodComObj.UnderlyingStock = coveredQuantities.Second;  

            #region Calcul du Déposit TIMS-EUREX V13 pour la position ouverte considérée par MarginGroup/MarginClass/Contract
            IEnumerable<TimsEurexGroupParameterCommunicationObject> validGroupComObj =
                        from groupComObj
                            in methodComObj.Parameters
                        where !((IMissingCommunicationObject)groupComObj).Missing
                        select (TimsEurexGroupParameterCommunicationObject)groupComObj;

            #region Calcul du Déposit TIMS-EUREX V13 pour la position ouverte considérée par MarginGroup
            foreach (TimsEurexGroupParameterCommunicationObject groupComObj in validGroupComObj)
            {

                #region Calcul du Déposit TIMS-EUREX V13 pour la position ouverte considérée par MarginClass
                foreach (TimsEurexClassParameterCommunicationObject classComObj in groupComObj.Parameters)
                {
                    #region Ajustement par MarginClass de toutes les positions Short CALL et Short PUT
                    // Ajustement par MarginClass de toutes les positions Short CALL et Short PUT avec des positions Futures ou Options Long portant
                    // sur le même sous-jacent.
                    //
                    //  - Une position Short CALL peut être couverte par:
                    //          - Une position Long CALL de même échéance et de Strike (prix d’exercice) inférieur ou égal 
                    //	        - Ou une position Long Future nette toutes échéances confondues 
                    //
                    //  - Une position Short PUT peut être couverte par:
                    //          - une position Long Put de même échéance et de Strike (prix d’exercice)supérieur ou égal 
                    //          - ou une position Short Future nette toutes échéances confondues 
                    //
                    // ps. pour plus de détail voir documentation EUREX (EUREX - Risk Based Margining - Final - (EN).pdf)
                    //  Chapitre - Short Option Adjustment - Page 23
                    IEnumerable<ShortOptionCompensationCommunicationObject> compensatedShortOptionQuantities = ShortOptionCompensation(classComObj.Positions);
                    #endregion

                    #region Calcul du Déposit TIMS-EUREX V13 pour la position ouverte considérée par Contract
                    foreach (TimsEurexContractParameterCommunicationObject contractComObj in classComObj.Parameters)
                    {

                        // Affectation des positions Ajustées (Short CALL et Short PUT) par Contract
                        contractComObj.CompensatedShortOptionQuantities =
                            from compensatedQuantity in compensatedShortOptionQuantities
                            join position in contractComObj.Positions on compensatedQuantity.AssetId equals position.First.idAsset
                            select compensatedQuantity;

                        // Calcul du Premium Margin par Contract
                        contractComObj.Premium =
                            GetPremiumMargin(contractComObj.Positions, premiumLiquidatingEvalParams);

                        // Calcul de la Liquidating Margin par Contract
                        contractComObj.Liquidating =
                            GetLiquidatingMargin(contractComObj.Positions, premiumLiquidatingEvalParams);

                        // Calcul de l'Additional Margin par Contract
                        contractComObj.Additional =
                            GetContractAdditionalMargin(contractComObj.Positions,
                            additionalEvalParam, contractComObj.Contract, contractComObj.CompensatedShortOptionQuantities);

                    }
                    #endregion

                    // Cumul des Premium Margin par MarginClass
                    classComObj.Premium = AggregateMargin(
                        from parameter in classComObj.Parameters
                        select ((TimsEurexContractParameterCommunicationObject)parameter).Premium);

                    // Cumul de la Liquidating Margin par MarginClass
                    classComObj.Liquidating = AggregateMargin(
                        from parameter in classComObj.Parameters
                        select ((TimsEurexContractParameterCommunicationObject)parameter).Liquidating);

                    // Calcul des Future Spread Margin par MarginClass
                    classComObj.Spread = GetSpreadMargin(
                        classComObj.Positions, spreadingEvalParam,
                        classComObj.Class, classComObj.SpotMonthSpreadRate, classComObj.BackMonthSpreadRate);

                    #region Cumul des Additional Margin par MarginClass afin de déterminé le scenario le plus défavorable
                    // Cumul des Additional Margin par MarginClass en tenant compte de l'OFFSET(*) afin de déterminé 
                    //  le scenario le plus défavorable.
                    //
                    //  Attention : Le scenario déterminé à cette étape peut être incorrecte dans le cas ou l’on a des DC 
                    //              de MarginClass différentes appartenant au même MarginGroup. 
                    //              Dans ce cas il se peut que le scenario le plus défavorable par MarginGroup ne soit pas 
                    //              le même que celui par MarginClass et c’est le scenario par MarginGroup qui prédomine
                    //
                    //  Exemple : 
                    //	        MarginGroup 	MarginClass	    DC	    Scenario le plus défavorable     Scenario le plus défavorable 
                    //						                            par MarginClass	                 par MarginGroup
                    //	        BUBO		    FGBL		    FGBL
                    //	        BUBO		    FGBL		    OGBL
                    //			                FGBL			....    Price Down Vola Down (PDVD)
                    //	        BUBO		    FGBS		    FGBS
                    //	        BUBO		    FGBS		    OGBS
                    //			                FGBS			....    Price Up Vola Down (PUVD)
                    //
                    //	        BUBO			....			....		                            Price Down Vola Down (PDVD)
                    //
                    //          ps. Dans cet exemple le scenario le plus défavorable pour le MarginGroup BUBO est (Price Down Vola Down) 
                    //               alors que celui pour la MarginClass FGBS est (Price Up Vola Down). 
                    //              Dans ce cas le scénario a prendre en compte pour toutes les MarginClass appartenant au MarginGroup
                    //               BUBO sera (Price Down Vola Down).
                    //
                    // (*) Les Additional Margin par MarginClass négatifs sont multipilier par un facteur % (OFFSET)
                    //
                    // Pour info : Le ticket 11279 a été initié, pour confirmer ce comportement auprès de la chambre de compensation EUREX.

                    classComObj.Additional = GetClassAdditionalMargin(
                        from parameter in classComObj.Parameters
                        select ((TimsEurexContractParameterCommunicationObject)parameter).Additional,
                        classComObj.Liquidating != null ? classComObj.Liquidating.MarginAmount.Amount.DecValue : 0,
                        groupComObj.Offset);
                    #endregion
                }
                #endregion

                // Cumul des Premium Margin par MarginGroup
                groupComObj.Premiums = AggregateMargins(
                    from parameter in groupComObj.Parameters
                    select ((TimsEurexClassParameterCommunicationObject)parameter).Premium);

                // Cumul des Future Spread Margin par MarginGroup
                groupComObj.Spreads = AggregateMargins(
                    from parameter in groupComObj.Parameters
                    select ((TimsEurexClassParameterCommunicationObject)parameter).Spread);

                #region Cumul des Additional Margin par MarginGroup afin de déterminé le scenario le plus défavorable
                // PM 20150122 [20712][20724] Déplacé : Ce trouvait avant plus bas.
                //// Recherche des taux de change utiles pour le calcul du Cross Currency Margining
                IEnumerable<TimsDecomposableParameterCommunicationObject> additionals =
                    from parameter in groupComObj.Parameters
                    select ((TimsEurexClassParameterCommunicationObject)parameter).Additional;
                IEnumerable<TimsEurexExchRateCommunicationObject> ratesExchange = GetExchRate(additionals);
                SetExchRateMethodComObj(methodComObj, ratesExchange);


                // Cumul des Additional Margin par MarginGroup en tenant compte de l'OFFSET* afin de déterminé 
                //  le scenario le plus défavorable par MarginGroup.
                //
                //  Attention: Le scenario le plus défavorable par MarginGroup peut être différnet de celui qui a
                //              déterminé par MarginClass dans ce cas c’est le scenario par MarginGroup qui prédominent.
                //
                //  Pour plus d'information voir exemple d'écrit juste au dessus  dans la partie (Cumul des Additional Margin par MarginClass)
                //
                // (*) Les Additional Margin par MarginClass négatifs sont multipilié par un facteur % (OFFSET)  
                //
                // Pour info : Le ticket 11279 a été initié, pour confirmer ce comportement auprès de la chambre de compensation EUREX.            

                // PM 20150122 [20712][20724] Prise en compte des ExchangesRates
                //groupComObj.Additionals = GetGroupAdditionalMargin(
                //    from parameter in groupComObj.Parameters
                //    select ((TimsEurexClassParameterCommunicationObject)parameter).Additional);
                groupComObj.Additionals = GetGroupAdditionalMargin(additionals, ratesExchange);
                #endregion

                #region Mise à jour des MarginAmount
                // PM 20130628 Ajout vérifiation qu'il y ait des valeurs "additional"
                if (groupComObj.Additionals != null)
                {
                    TimsDecomposableParameterCommunicationObject groupAdditional = groupComObj.Additionals.FirstOrDefault();
                    if (groupAdditional != default(TimsDecomposableParameterCommunicationObject))
                    {
                        // Correction du LOG relatif au calcul du DEPOSIT dans le cas ou le scenario le plus défavorable par MarginGroup
                        // est différent de celui qui a été déterminé par MarginClass
                        TimsFactorCommunicationObject groupMarginAmount = groupAdditional.Factors.FirstOrDefault(f => f.MarginAmount != null);
                        if (groupMarginAmount != default(TimsFactorCommunicationObject))
                        {
                            foreach (TimsEurexClassParameterCommunicationObject classComObj in groupComObj.Parameters)
                            {
                                if (classComObj.Additional != default(TimsDecomposableParameterCommunicationObject))
                                {
                                    TimsFactorCommunicationObject classMarginAmount = classComObj.Additional.Factors.FirstOrDefault(f => f.MarginAmount != null);
                                    if (classMarginAmount != default(TimsFactorCommunicationObject))
                                    {
                                        if (classMarginAmount.Identifier != groupMarginAmount.Identifier)
                                        {
                                            TimsFactorCommunicationObject maxAdjusted = classComObj.Additional.Factors.FirstOrDefault(f => f.Identifier == cm_MaxAdjusted);
                                            TimsFactorCommunicationObject margin = maxAdjusted.RiskArray.FirstOrDefault(r => r.Identifier == groupMarginAmount.Identifier);
                                            if (margin != default(TimsFactorCommunicationObject))
                                            {
                                                classMarginAmount.Identifier = margin.Identifier;
                                                classMarginAmount.QuoteUnlVsQuote_Indicator = margin.QuoteUnlVsQuote_Indicator;
                                                classMarginAmount.Volatility_Indicator = margin.Volatility_Indicator;
                                                classMarginAmount.RiskArrayIndex = margin.RiskArrayIndex;
                                                classMarginAmount.MaturityYearMonth = margin.MaturityYearMonth;
                                                classMarginAmount.MaturityFactor = margin.MaturityFactor;
                                                classMarginAmount.ShortAdj = margin.ShortAdj;
                                                classMarginAmount.Quote = margin.Quote;
                                                classMarginAmount.MarginAmount = new Money(margin.MarginAmount.Amount.DecValue, margin.MarginAmount.GetCurrency.Value);
                                                //
                                                classComObj.Additional.MarginAmount = AdjustFactor(
                                                                                        margin,
                                                                                        margin.MaturityFactor ?? 0)
                                                                                            .MarginAmount;
                                            }
                                        }
                                    }
                                    classComObj.MarginAmount = AggregateMarginAmounts(
                                        new TimsDecomposableParameterCommunicationObject[] { classComObj.Premium, classComObj.Spread, classComObj.Additional })
                                        .First();
                                }
                            }
                        }
                    }
                }
                #endregion

                // PM 20150122 [20712][20724] Déplacé plus haut
                //// Recherche des taux de change utiles pour le calcul du Cross Currency Margining
                //IEnumerable<TimsDecomposableParameterCommunicationObject> additionals =
                //    from parameter in groupComObj.Parameters
                //    select ((TimsEurexClassParameterCommunicationObject)parameter).Additional;
                //IEnumerable<TimsEurexExchRateCommunicationObject> ratesExchange = GetExchRate(additionals);
                //SetExchRateMethodComObj(methodComObj, ratesExchange);

                // Calcul du Margin Amount(*) par MarginGroup 
                // * Pour un MarginGroup Donné celui-ci est égale à l'Additional Margin + Premium Margin + Future Spread Margin
                groupComObj.MarginAmounts = AggregateMarginAmounts(
                        groupComObj.Premiums.Union(groupComObj.Spreads).Union(groupComObj.Additionals));

                SumAmounts(groupComObj.MarginAmounts.Cast<Money>(), ref amounts);
            }

            #endregion

            #endregion

            #region Calcul du Cross Currency Margining (ShortFall)
            // Dans le cas où on a un calcul de déposit pour un même MRO sur plusieurs devises, on peut opérer une réduction du déposit dans une devise 
            //  donnée par le surplus(*) de déposit d'une autre devise.
            //
            // (*) On parle de suprlus dans le cas où le déposit calculé pour une devise est négatif (Position sans Risque).
            //  Ce surplus permet de couvrir les deposits positif dans les autres devises en utilisant la priorité suivante sur les devises
            //  ( CHF, USD, … )et ensuite EUR.
            //
            // Exemple:
            //
            //    Pour un acteur donné, j’ai la situation suivante:
            //
            //      Devise	Parité	Taux de Change  Déposit		
            //      EUR	    EUR	    1,000000	     54 000,00	
            //      CHF	    EUR	    1,264868	    - 1 950,00	
            //
            //
            //    Sur cette position on un surplus de 1 950,00 CHF 
            //
            //      Devise	Parité	Taux de Change	Déposit     Surplus EUR		
            //      EUR	    EUR	    1,000000	     54 000,00				
            //      CHF	    EUR	    1,264868	    - 1 950,00	- 1 541,66	
            //
            //
            //    Ce Surplus permet de réduire le deposit en EUR  de (1 541,66 (Correspond au montant en CHF convertis en EUR))
            //
            //      Devise	Parité	Taux de Change	Déposit		Surplus	EUR 	Réduction	Deposit Final
            //      EUR	    EUR	    1,000000	     54 000,00			       - 1 541,66	    52 458,34	
            //      CHF	    EUR	    1,264868	    - 1 950,00	- 1 541,66			                 0,00
            //
            methodComObj.Cross = GetMethodCrossMargin(
            methodComObj.ExchRates,
            amounts.ToArray());

            if (methodComObj.Cross != null)
            {
                methodComObj.NotCrossedMarginAmounts = amounts.ToArray();

                // Garder les montants de déposit négatifs et les forcer à 0
                Money[] surplusAmount = (from amount in amounts where (amount.Amount.DecValue <= 0) select new Money(0, amount.Currency)).ToArray();

                amounts.Clear();

                foreach (var crossAmount in methodComObj.Cross)
                {
                    amounts.Add((Money)crossAmount.MarginAmount);
                }

                if (surplusAmount != null)
                {
                    amounts.AddRange(surplusAmount);
                }
            }
            #endregion

            // Dans le cas ou a aucun montant de déposit de calculé, on force un montant à 0.00 EUR
            //  de manière à avoir la création d’un trade MRO avec un montant à zéro
            if (amounts.Count <= 0)
            {
                amounts.Add(new Money(0, "EUR"));
            }

            return amounts;
        }

        /// <summary>
        /// Reset internal EUREX parameter collections
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            m_ContractParameters = null;

            m_MaturityParameters = null;

            m_AssetParameters = null;

            m_VolatilityParameters = null;

            m_ExchangeRates = null;
        }

        /// <summary>
        /// Get sorting coverage parameters for all the loaded parameters set
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">Positions of the current risk element</param>
        /// <returns>A collection of sorting parameters in order to be used inside of the ReducePosition method</returns>
        protected override IEnumerable<CoverageSortParameters> GetSortParametersForCoverage(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            return from position in pGroupedPositionsByIdAsset
                   join asset in m_AssetParameters on position.First.idAsset equals asset.AssetId
                   join maturity in m_MaturityParameters on asset.MaturityId equals maturity.MaturityId
                   join contract in m_ContractParameters on asset.ContractId equals contract.ContractId
                   // FL & PM 20130916 [18955] Prendre les Futures et les Calls
                   where (contract.Category == "F") || (maturity.PutOrCall == "1")
                   select
                       new CoverageSortParameters
                       {
                           AssetId = position.First.idAsset,
                           ContractId = contract.ContractId,
                           MaturityYearMonth = maturity.MaturityYearMonth,
                           Multiplier =
                               // the asset multiplier has to be used at first, when different than 0 it could be 
                               // - or the contract multiplier 
                               // - or the derivative attribute multiplier 
                               // - or the asset multiplier
                                asset.Multiplier != 0 ?
                                    asset.Multiplier : contract.ContractMultiplier,
                           Quote = asset.AssetQuoteParameter,
                           StrikePrice = asset.Strike,
                           // FL & PM 20130916 [18955] Ne pas mettre Future en dur (mais Future ou Call selon les cas)
                           //Type = RiskMethodQtyType.Future,
                           Type = (contract.Category == "F") ? RiskMethodQtyType.Future : RiskMethodQtyType.Call,
                       };
        }

        /// <summary>
        /// Get complementary information from the markets/clearing house which is using the current method 
        /// </summary>
        /// <param name="pEntityMarkets">the entity/market collection concerned by the current clearing house</param>
        public override void BuildMarketParameters(IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        {
            base.BuildMarketParameters(pEntityMarkets);
        }

        /// <summary>
        /// Round the given amount with a 2 decimals precision and rounding rule half Up
        /// </summary>
        /// <param name="pAmount">amount to be rounded</param>
        /// <returns>the roundedn amount</returns>
        protected override decimal RoundAmount(decimal pAmount)
        {
            return System.Math.Round(pAmount, 2, MidpointRounding.AwayFromZero);
        }

        private void InitEmptyCollections()
        {
            m_ContractParameters = new List<ContractParameterTimsEurex>();

            m_MaturityParameters = new List<MaturityParameterTimsEurex>();

            m_AssetParameters = new List<AssetParameterTimsEurex>();

            m_VolatilityParameters = new List<VolatilityParameterTimsEurex>();

            m_ExchangeRates = new List<FxRateTimsEurex>();
        }

        /// <summary>
        /// Load contract parameters
        /// </summary>
        /// <param name="pConnection">current connection</param>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        private IEnumerable<ContractParameterTimsEurex> LoadContractParameters(IDbConnection pConnection)
        {
            // 20120716 MF Ticket 18004
            Dictionary<string, object> dbParameterValues = new Dictionary<string, object>();

            CommandType cmdTyp = DataContractHelper.GetType(DataContractResultSets.CONTRACT_TIMSEUREXMETHOD);

            string request =
                DataHelper<object>.IsNullTransform(
                    pConnection,
                    cmdTyp,
                    DataContractHelper.GetQuery(DataContractResultSets.CONTRACT_TIMSEUREXMETHOD)
                );

            return
                DataHelper<ContractParameterTimsEurex>.ExecuteDataSet(
                    pConnection,
                    cmdTyp,
                    request,
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.CONTRACT_TIMSEUREXMETHOD, dbParameterValues)
                );
        }

        /// <summary>
        /// Load maturity parameters 
        /// </summary>
        /// <param name="pConnection">current connection</param>
        /// <param name="pDbParameterValues">query parameter values collection, a DTBUSINESS is requested</param>
        /// <returns>a collection of maturity elements for all the input contracts</returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        private IEnumerable<MaturityParameterTimsEurex> LoadMaturityParameters(IDbConnection pConnection,
            Dictionary<string, object> pDbParameterValues)
        {
            CommandType cmdTyp = DataContractHelper.GetType(DataContractResultSets.MATURITY_TIMSEUREXMETHOD);

            string request =
                DataHelper<object>.IsNullTransform(
                    pConnection,
                    cmdTyp,
                    DataContractHelper.GetQuery(DataContractResultSets.MATURITY_TIMSEUREXMETHOD)
                );

            IDbDataParameter[] parameters =
                DataContractHelper.GetDbDataParameters(DataContractResultSets.MATURITY_TIMSEUREXMETHOD, pDbParameterValues);

            return
                DataHelper<MaturityParameterTimsEurex>.ExecuteDataSet(
                    pConnection,
                    cmdTyp,
                    request,
                    parameters
                );

        }

        /// <summary>
        /// Load asset parameters
        /// </summary>
        /// <param name="pConnection">current connection</param>
        /// <param name="pDbParameterValues">query parameter values collection, a DTBUSINESS is requested</param>
        /// <returns>a collection of asset risk parameters valid for the current business date for all the input assets</returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        private IEnumerable<AssetParameterTimsEurex> LoadAssetParameters(IDbConnection pConnection,
            Dictionary<string, object> pDbParameterValues)
        {
            CommandType cmdTyp = DataContractHelper.GetType(DataContractResultSets.ASSET_TIMSEUREXMETHOD);

            string request =
                DataHelper<object>.IsNullTransform(
                    pConnection,
                    cmdTyp,
                    DataContractHelper.GetQuery(DataContractResultSets.ASSET_TIMSEUREXMETHOD)
                );

            IDbDataParameter[] parameters =
                DataContractHelper.GetDbDataParameters(DataContractResultSets.ASSET_TIMSEUREXMETHOD, pDbParameterValues);

            return
                DataHelper<AssetParameterTimsEurex>.ExecuteDataSet(
                    pConnection,
                    cmdTyp,
                    request,
                    parameters
                );


        }

        /// <summary>
        /// Computing a multiplier from the contract/asset EUREX parameters. 
        /// "Extrema ratio" when no multipliers has been defined for the current asset/contract
        /// </summary>
        /// <param name="pContractParameter"></param>
        /// <param name="pTradeUnit"></param>
        /// <returns></returns>
        private decimal ComputeMultiplierByParameter(ContractParameterTimsEurex pContractParameter, decimal pTradeUnit)
        {
            decimal res = 1;

            if ((pContractParameter != null) && (pContractParameter.TickSize != 0))
            {
                res = pTradeUnit * (pContractParameter.TickValue / pContractParameter.TickSize);
            }

            // add logging on journal for this evaluation

            return res;
        }

        /// <summary>
        /// Load volatility parameters
        /// </summary>
        /// <param name="pConnection">current connection</param>
        /// <param name="pDbParameterValues">query parameter values collection, a DTBUSINESS is requested</param>
        /// <returns>a collection of volatility parameters valid for the current business date for all the input assets</returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        private IEnumerable<VolatilityParameterTimsEurex> LoadVolatilityParameters(
            IDbConnection pConnection, Dictionary<string, object> pDbParameterValues)
        {
            CommandType cmdTyp = DataContractHelper.GetType(DataContractResultSets.VOLATILITY_TIMSEUREXMETHOD);

            string request =
                DataHelper<object>.IsNullTransform(
                    pConnection,
                    cmdTyp,
                    DataContractHelper.GetQuery(DataContractResultSets.VOLATILITY_TIMSEUREXMETHOD)
                );

            IDbDataParameter[] parameters =
                DataContractHelper.GetDbDataParameters(DataContractResultSets.VOLATILITY_TIMSEUREXMETHOD, pDbParameterValues);

            return
                DataHelper<VolatilityParameterTimsEurex>.ExecuteDataSet(
                    pConnection,
                    cmdTyp,
                    request,
                    parameters
                );
        }

        private IEnumerable<FxRateTimsEurex> LoadExchangeRates(IDbConnection pConnection, Dictionary<string, object> pDbParameterValues)
        {
            CommandType cmdTyp = DataContractHelper.GetType(DataContractResultSets.FXRATE_TIMSEUREXMETHOD);

            string request =
                DataHelper<object>.IsNullTransform(
                    pConnection,
                    cmdTyp,
                    DataContractHelper.GetQuery(DataContractResultSets.FXRATE_TIMSEUREXMETHOD)
                );

            // 1. Load

            IEnumerable<FxRateTimsEurex> exchangeRates = DataHelper<FxRateTimsEurex>.ExecuteDataSet(
                    pConnection,
                    cmdTyp,
                    request,
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.FXRATE_TIMSEUREXMETHOD, pDbParameterValues)
                );

            // 2. return rates

            return exchangeRates;
        }

        private IEnumerable<TimsEurexGroupParameterCommunicationObject> GetGroupClassHierarchy(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset,
            IEnumerable<StockCoverageCommunicationObject> pCoveredQuantities)
        {
            // 1. defining an {assets id/contracts symbols} list, 
            //  all the asset of the list are "in position" (pGroupedPositionsByIdAsset) and related to a loaded parameter
            var assetContractsWithParamsInPosition = (
                from maturityParam in m_MaturityParameters
                join assetParam in m_AssetParameters on maturityParam.MaturityId equals assetParam.MaturityId
                join position in pGroupedPositionsByIdAsset on assetParam.AssetId equals position.First.idAsset
                select new
                {
                    Symbol = maturityParam.ContractSymbol,
                    assetParam.AssetId,
                    assetParam.ContractId,
                })
                .Distinct();


            // 1.1 defining the contracts list starting by the previous list 
            IEnumerable<int> contractsWithParamsInPosition =
                (from contractAssetPair in assetContractsWithParamsInPosition select contractAssetPair.ContractId).Distinct();

            // 2. filtering the "contract" parameters list 
            //  (starting by the loaded m_ContractParameters) filtering on contractsWithParamsInPosition
            IEnumerable<ContractParameterTimsEurex> contractParametersInPosition =
                from parameter in m_ContractParameters
                join ContractId in contractsWithParamsInPosition on parameter.ContractId equals ContractId
                select parameter;

            // 3. build the hierarchy 
            // 3.1 group the contract parameters by "margin group"....
            var contractByMarginGroup = (from contractParameter in contractParametersInPosition select contractParameter)
                    .GroupBy(key => new
                    {
                        // the margin group identifier may be null in case of group containing just one class only.
                        // When it arrives we use the margin class identifier instead
                        Group = key.MarginGroup ?? key.MarginClass,
                        key.OutOfTheMoneyMinValue
                    });

            IEnumerable<TimsEurexGroupParameterCommunicationObject> partitionsSet =
                from marginGroup in contractByMarginGroup
                select new TimsEurexGroupParameterCommunicationObject
                {
                    Group = marginGroup.Key.Group,

                    OutOfTheMoneyMinValue = marginGroup.Key.OutOfTheMoneyMinValue,

                    Offset = marginGroup.Select(elem => elem.Offset).Min(),

                    Positions = from position in pGroupedPositionsByIdAsset
                                join contractAssetPair in assetContractsWithParamsInPosition on position.First.idAsset equals contractAssetPair.AssetId
                                join marginClass in marginGroup on contractAssetPair.ContractId equals marginClass.ContractId
                                select position,

                    Parameters = (from marginClass in
                                      // 3.2  group by "margin class"....
                                      (from marginClass in marginGroup select marginClass)
                                     .GroupBy(key => new
                                     {
                                         Class = key.MarginClass,
                                         key.SpotMonthSpreadRate,
                                         key.BackMonthSpreadRate
                                     })

                                  select new TimsEurexClassParameterCommunicationObject
                                  {
                                      Class = marginClass.Key.Class,

                                      SpotMonthSpreadRate = marginClass.Key.SpotMonthSpreadRate,

                                      BackMonthSpreadRate = marginClass.Key.BackMonthSpreadRate,

                                      Offset = marginClass.Select(elem => elem.Offset).Min(),

                                      // The real maturity factor will be identified during the additional margin for the current margin class,
                                      //    the maturity factor will be > 0 just in case the current class 
                                      //    has the maturity switch activated
                                      MaturityFactor = 0,

                                      ContractSymbols = from contract in marginClass select contract.ContractSymbol,

                                      Positions = from position in pGroupedPositionsByIdAsset
                                                  join contractAssetPair in assetContractsWithParamsInPosition on position.First.idAsset equals contractAssetPair.AssetId
                                                  join contract in marginClass on contractAssetPair.ContractId equals contract.ContractId
                                                  select position,

                                      Parameters = (from contract in
                                                        // 3.3  group by contract symbol....
                                                        (from contract in marginClass select contract)
                                                       .GroupBy(key => key.ContractSymbol)

                                                    select new TimsEurexContractParameterCommunicationObject
                                                    {
                                                        Contract = contract.Key,

                                                        Offsets = contract.Select(elem => elem.Offset),

                                                        Positions = from position in pGroupedPositionsByIdAsset
                                                                    join contractAssetPair in assetContractsWithParamsInPosition on position.First.idAsset equals contractAssetPair.AssetId
                                                                    join elem in contract on contractAssetPair.ContractId equals elem.ContractId
                                                                    select position,

                                                        StocksCoverage = from coveredQuantity in pCoveredQuantities
                                                                         join elem in contract on coveredQuantity.ContractId equals elem.ContractId
                                                                         select coveredQuantity,

                                                        Parameters = null,

                                                        // PM 20130628 Ajout information comme quoi il manque des données de calcul pour ce contrat
                                                        Missing = (from position in pGroupedPositionsByIdAsset
                                                                   join contractAssetPair in assetContractsWithParamsInPosition on position.First.idAsset equals contractAssetPair.AssetId
                                                                   join elem in contract on contractAssetPair.ContractId equals elem.ContractId
                                                                   select position.First.idAsset
                                                                  ).Except
                                                                  (
                                                                    from volatilityParameter in m_VolatilityParameters
                                                                    select volatilityParameter.AssetId
                                                                  ).Count() != 0,
                                                    }
                                                   ).ToArray(),

                                  }).ToArray(),

                    Missing = false,

                    MarginAmounts = (from currency in
                                         (from marginClass in marginGroup select marginClass.Currency).Distinct()
                                     select new Money(0, currency))
                                    .ToArray(),

                };


            // 4. build a fake parameter container of all the assets for which no parameters have been found

            IEnumerable<int> assetWithParamsInPosition = from elem in assetContractsWithParamsInPosition select elem.AssetId;

            TimsEurexGroupParameterCommunicationObject communicationObjectForMissingParameters =
                new TimsEurexGroupParameterCommunicationObject();

            if (InitializeCommunicationObjectForMissingParameters<TimsEurexGroupParameterCommunicationObject>
                (pGroupedPositionsByIdAsset, assetWithParamsInPosition, communicationObjectForMissingParameters))
            {
                communicationObjectForMissingParameters.Group = Cst.NotFound;

                partitionsSet = partitionsSet.Union
                       (
                           new TimsEurexGroupParameterCommunicationObject[] { communicationObjectForMissingParameters }
                       );
            }

            return partitionsSet;
        }

        private IEnumerable<PremiumLiquidatingEvaluationParameters> GetPremiumLiquidatingEvaluationParameters(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            return (
                // 1. Open positions
                from position in pGroupedPositionsByIdAsset
                join assetParameter in m_AssetParameters on position.First.idAsset equals assetParameter.AssetId
                join maturityParameter in m_MaturityParameters on assetParameter.MaturityId equals maturityParameter.MaturityId
                join contractParameter in m_ContractParameters on assetParameter.ContractId equals contractParameter.ContractId

                select

                new PremiumLiquidatingEvaluationParameters
                {
                    AssetId = assetParameter.AssetId,

                    Type =
                        GetRiskMethodQtyType(contractParameter.Category, maturityParameter.PutOrCall, false),

                    PremiumStyle =
                        (contractParameter.UnlCategory != Cst.UnderlyingAsset.Future)
                        && contractParameter.MarginStyle == MarginStyle.Traditional,

                    Multiplier = assetParameter.Multiplier,

                    Currency = contractParameter.Currency,

                    Quote = assetParameter.AssetQuoteParameter,

                    UnlQuote = assetParameter.UnlAssetQuoteParameter,
                    StrikePrice = assetParameter.Strike,
                })
                .Union(
                // 2. in delivery positions
                from position in pGroupedPositionsByIdAsset
                where position.Second.ExeAssQuantity != 0
                join assetParameter in m_AssetParameters on position.First.idAsset equals assetParameter.AssetId
                join maturityParameter in m_MaturityParameters on assetParameter.MaturityId equals maturityParameter.MaturityId
                join contractParameter in m_ContractParameters on assetParameter.ContractId equals contractParameter.ContractId

                select

                new PremiumLiquidatingEvaluationParameters
                {
                    AssetId = assetParameter.AssetId,

                    Type =
                        GetRiskMethodQtyType(contractParameter.Category, maturityParameter.PutOrCall, true),

                    PremiumStyle =
                        (contractParameter.UnlCategory != Cst.UnderlyingAsset.Future)
                        && contractParameter.MarginStyle == MarginStyle.Traditional,

                    Multiplier = assetParameter.Multiplier,

                    Currency = contractParameter.Currency,

                    Quote = assetParameter.AssetQuoteParameter,

                    UnlQuote = assetParameter.UnlAssetQuoteParameter,
                    StrikePrice = assetParameter.Strike,
                }
                );
        }

        private IEnumerable<SpreadEvaluationParameters> GetSpreadingEvaluationParameters(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            return (
                // 1. Open positions
                from position in pGroupedPositionsByIdAsset
                join assetParameter in m_AssetParameters on position.First.idAsset equals assetParameter.AssetId
                join maturityParameter in m_MaturityParameters on assetParameter.MaturityId equals maturityParameter.MaturityId
                join contractParameter in m_ContractParameters on assetParameter.ContractId equals contractParameter.ContractId

                select new SpreadEvaluationParameters
                {
                    AssetId = assetParameter.AssetId,

                    Type =
                        GetRiskMethodQtyType(contractParameter.Category, maturityParameter.PutOrCall, false),

                    Currency = contractParameter.Currency,

                    MaturityYearMonth = maturityParameter.MaturityYearMonth,

                    MaturityRuleFrequency = contractParameter.MaturityRuleFrequency
                })
                .Union(
                // 2. in delivery positions
                from position in pGroupedPositionsByIdAsset
                where position.Second.ExeAssQuantity != 0
                join assetParameter in m_AssetParameters on position.First.idAsset equals assetParameter.AssetId
                join maturityParameter in m_MaturityParameters on assetParameter.MaturityId equals maturityParameter.MaturityId
                join contractParameter in m_ContractParameters on assetParameter.ContractId equals contractParameter.ContractId

                select new SpreadEvaluationParameters
                {
                    AssetId = assetParameter.AssetId,

                    Type =
                        GetRiskMethodQtyType(contractParameter.Category, maturityParameter.PutOrCall, true),

                    Currency = contractParameter.Currency,

                    MaturityYearMonth = maturityParameter.MaturityYearMonth,

                    MaturityRuleFrequency = contractParameter.MaturityRuleFrequency
                }
                );
        }

        private IEnumerable<AdditionalEvaluationParameters> GetAdditionalEvaluationParameters(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            // 1. Open positions
            IEnumerable<AdditionalEvaluationParameters> addParamOpenPosition =
                from position in pGroupedPositionsByIdAsset
                join volatilityParameter in m_VolatilityParameters on position.First.idAsset equals volatilityParameter.AssetId
                join assetParameter in m_AssetParameters on
                    new { volatilityParameter.AssetId, volatilityParameter.MaturityId }
                    equals
                    new { assetParameter.AssetId, assetParameter.MaturityId }
                join maturityParameter in m_MaturityParameters on assetParameter.MaturityId equals maturityParameter.MaturityId
                join contractParameter in m_ContractParameters on assetParameter.ContractId equals contractParameter.ContractId
                orderby volatilityParameter.AssetId ascending, volatilityParameter.RiskArrayIndex ascending
                select new AdditionalEvaluationParameters
                {
                    AssetId = assetParameter.AssetId,

                    Type = GetRiskMethodQtyType(contractParameter.Category, maturityParameter.PutOrCall, false),

                    RiskArrayIndex = volatilityParameter.RiskArrayIndex,

                    // trimming value, because the DB field is a char(4)
                    QuoteUnlVsQuote_Indicator = volatilityParameter.QuoteUnlVsQuote_Indicator.Trim(),

                    QuoteUnlIndicator = volatilityParameter.QuoteUnlIndicator,

                    UpVolatility = (decimal)volatilityParameter.UpVolatility,

                    UpTheoreticalValue = (decimal)volatilityParameter.UpTheoreticalValue,

                    UpTheoreticalExists = volatilityParameter.UpTheoreticalExists,

                    UpShortAdj = (decimal)volatilityParameter.UpShortAdj,

                    NtrlVolatility = (decimal)volatilityParameter.NtrlVolatility,

                    NtrlTheoreticalValue = (decimal)volatilityParameter.NtrlTheoreticalValue,

                    NtrlTheoreticalExists = volatilityParameter.NtrlTheoreticalExists,

                    NtrlShortAdj = (decimal)volatilityParameter.NtrlShortAdj,

                    DownVolatility = (decimal)volatilityParameter.DownVolatility,

                    DownTheoreticalValue = (decimal)volatilityParameter.DownTheoreticalValue,

                    DownTheoreticalExists = volatilityParameter.DownTheoreticalExists,

                    DownShortAdj = (decimal)volatilityParameter.DownShortAdj,

                    RiskValueExeAss = volatilityParameter.RiskValueExeAss,

                    Multiplier = assetParameter.Multiplier,

                    MaturityYearMonth = maturityParameter.MaturityYearMonth,

                    MaturitySwitch = contractParameter.MaturitySwitch,

                    MaturityFactor = maturityParameter.MaturityFactor,

                    Currency = contractParameter.Currency,
                };
            // 2. in delivery positions
            IEnumerable<AdditionalEvaluationParameters> addParamDeliveryPosition =
                from position in pGroupedPositionsByIdAsset
                where position.Second.ExeAssQuantity != 0
                join volatilityParameter in m_VolatilityParameters on position.First.idAsset equals volatilityParameter.AssetId
                join assetParameter in m_AssetParameters on
                new { volatilityParameter.AssetId, volatilityParameter.MaturityId }
                equals
                new { assetParameter.AssetId, assetParameter.MaturityId }
                join maturityParameter in m_MaturityParameters on assetParameter.MaturityId equals maturityParameter.MaturityId
                join contractParameter in m_ContractParameters on assetParameter.ContractId equals contractParameter.ContractId
                orderby volatilityParameter.AssetId ascending, volatilityParameter.RiskArrayIndex ascending
                select new AdditionalEvaluationParameters
                {
                    AssetId = assetParameter.AssetId,

                    Type = GetRiskMethodQtyType(contractParameter.Category, maturityParameter.PutOrCall, true),

                    RiskArrayIndex = volatilityParameter.RiskArrayIndex,

                    // trimming value, because the DB field is a char(4)
                    QuoteUnlVsQuote_Indicator = volatilityParameter.QuoteUnlVsQuote_Indicator.Trim(),

                    UpVolatility = (decimal)volatilityParameter.UpVolatility,

                    UpTheoreticalValue = (decimal)volatilityParameter.UpTheoreticalValue,

                    UpTheoreticalExists = volatilityParameter.UpTheoreticalExists,

                    UpShortAdj = (decimal)volatilityParameter.UpShortAdj,

                    NtrlVolatility = (decimal)volatilityParameter.NtrlVolatility,

                    NtrlTheoreticalValue = (decimal)volatilityParameter.NtrlTheoreticalValue,

                    NtrlTheoreticalExists = volatilityParameter.NtrlTheoreticalExists,

                    NtrlShortAdj = (decimal)volatilityParameter.NtrlShortAdj,

                    DownVolatility = (decimal)volatilityParameter.DownVolatility,

                    DownTheoreticalValue = (decimal)volatilityParameter.DownTheoreticalValue,

                    DownTheoreticalExists = volatilityParameter.DownTheoreticalExists,

                    DownShortAdj = (decimal)volatilityParameter.DownShortAdj,

                    Multiplier = assetParameter.Multiplier,

                    MaturityYearMonth = maturityParameter.MaturityYearMonth,

                    MaturitySwitch = contractParameter.MaturitySwitch,

                    MaturityFactor = maturityParameter.MaturityFactor,

                    Currency = contractParameter.Currency,
                };

            IEnumerable<AdditionalEvaluationParameters> addParam = addParamOpenPosition.Union(addParamDeliveryPosition);

            return addParam;
        }

        private TimsDecomposableParameterCommunicationObject GetPremiumMargin(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pContractPositions,
            IEnumerable<PremiumLiquidatingEvaluationParameters> pPremiumEvalParams)
        {
            // Calculate the premium margin for each open position or exercise/assingnation
            IEnumerable<TimsFactorCommunicationObject> factors =
                from position in pContractPositions
                join data in pPremiumEvalParams on position.First.idAsset equals data.AssetId
                where
                    // categories for which we may evaluate a premium margin
                    data.PremiumStyle
                    &&
                    (data.Type == RiskMethodQtyType.Put ||
                    data.Type == RiskMethodQtyType.Call ||
                    data.Type == RiskMethodQtyType.ExeAssCall ||
                    data.Type == RiskMethodQtyType.ExeAssPut)
                select new TimsFactorCommunicationObject
                {
                    AssetId = data.AssetId,

                    // the quantity either for open positions either for executions/assignations
                    Quantity =
                        position.Second.QuantityWithMarginSign(data.Type, position.First.Side),

                    Multiplier = data.Multiplier,

                    Quote = GetPremiumQuote(data.Type, data.Quote, data.UnlQuote, data.StrikePrice),

                    DeliveryDate = (data.Type == RiskMethodQtyType.ExeAssCall || data.Type == RiskMethodQtyType.ExeAssPut) ?
                        position.Second.DeliveryDate
                        :
                        null,

                    PosType = position.Second.GetPosType(data.Type),

                    MarginAmount =
                     new Money(
                        //
                         position.Second.QuantityWithMarginSign(data.Type, position.First.Side) *
                         this.RoundAmount(
                            GetPremiumQuote(data.Type, data.Quote, data.UnlQuote, data.StrikePrice)
                            * data.Multiplier

                        )
                        ,
                        //
                         data.Currency),
                };

            TimsDecomposableParameterCommunicationObject premiumComObj = null;

            if (factors.Count() > 0)
            {

                premiumComObj = new TimsDecomposableParameterCommunicationObject();

                List<Money> amounts = new List<Money>();
                SumAmounts(from factor in factors select (Money)factor.MarginAmount, ref amounts);

                premiumComObj.Factors = factors;

                premiumComObj.MarginAmount = amounts[0];

            }

            return premiumComObj;
        }

        private TimsDecomposableParameterCommunicationObject GetLiquidatingMargin(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pContractPositions,
            IEnumerable<PremiumLiquidatingEvaluationParameters> pLiquidatingEvalParams)
        {
            // Calculate the liquidating margin for each open position
            IEnumerable<TimsFactorCommunicationObject> factors =
                from position in pContractPositions
                join data in pLiquidatingEvalParams on position.First.idAsset equals data.AssetId
                where
                    (data.Type == RiskMethodQtyType.Put ||
                    data.Type == RiskMethodQtyType.Call ||
                    data.Type == RiskMethodQtyType.ExeAssCall ||
                    data.Type == RiskMethodQtyType.ExeAssPut ||
                    data.Type == RiskMethodQtyType.Future ||
                    data.Type == RiskMethodQtyType.FutureMoff)
                select new TimsFactorCommunicationObject
                {
                    AssetId = data.AssetId,

                    // the quantity either for open positions either for executions/assignations
                    Quantity = position.Second.QuantityWithMarginSign(data.Type, position.First.Side),

                    Multiplier = data.Multiplier,

                    Quote = GetLiquidatingQuote(data.Type, data.Quote, data.UnlQuote, data.StrikePrice),

                    DeliveryDate = (
                        data.Type == RiskMethodQtyType.ExeAssCall ||
                        data.Type == RiskMethodQtyType.ExeAssPut ||
                        data.Type == RiskMethodQtyType.FutureMoff) ?
                        position.Second.DeliveryDate
                        :
                        null,

                    PosType = position.Second.GetPosType(data.Type),

                    MarginAmount =
                     new Money(
                        //
                         position.Second.QuantityWithMarginSign(data.Type, position.First.Side) *
                         this.RoundAmount(
                            GetLiquidatingQuote(data.Type, data.Quote, data.UnlQuote, data.StrikePrice)
                            * data.Multiplier
                        )
                        //
                        ,
                         data.Currency),
                };

            TimsDecomposableParameterCommunicationObject liquidatingComObj = null;

            if (factors.Count() > 0)
            {

                liquidatingComObj = new TimsDecomposableParameterCommunicationObject();

                List<Money> amounts = new List<Money>();
                SumAmounts(from factor in factors select (Money)factor.MarginAmount, ref amounts);

                // There is no interest to publish the liquidating factors, the factors collection is empty
                liquidatingComObj.Factors = null;

                liquidatingComObj.MarginAmount = amounts[0];

            }

            return liquidatingComObj;
        }

        /// <summary>
        /// Compute the spread margin for a margin class
        /// </summary>
        /// <param name="pClassPositions">positions related to the margin class (all of them, 
        /// filtering on futures contracts is made internally)</param>
        /// <param name="pSpreadEvalParams">spreading parameters</param>
        /// <param name="pMarginClass">
        /// Margin class identifier. Second level of the EUREX product hierarchy. 
        /// More derivative contracts could be part of a margin class. 
        /// Yhe futures positions used to compute the spread margin will be grouped by margin class and maturity (MaturityYearMonth).
        /// </param>
        /// <param name="pSpotMonthSpreadRate">
        /// Once the business date month starts in which a future contract expires, 
        /// it is automatically assumed that this future will demonstrate a higher degree of volatility. 
        /// From this day onwards, all spread pairs containing a this future (front-month) position must be backed 
        /// at the higher spot-month spread margin rate
        /// </param>
        /// <param name="pBackMonthSpreadRate">
        /// As long as the spot-month has not yet been reached, the margin calculation is based
        /// on the normal spread margin rate: the back-month spread margin rate
        /// </param>
        /// <returns>a communication object containing the spreading margin and the related details</returns>
        private TimsDecomposableParameterCommunicationObject GetSpreadMargin(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pClassPositions,
            IEnumerable<SpreadEvaluationParameters> pSpreadEvalParams,
            string pMarginClass, decimal pSpotMonthSpreadRate, decimal pBackMonthSpreadRate)
        {

            // 1 - Grouping positions by maturity (and filtering on Futures contracts)

            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedFuturesPositionsByClassMaturity;

            IEnumerable<PositionsGrouping.NettingParameters<string, decimal>> parametersXGrouping =
                from spreadParameter in pSpreadEvalParams
                join assetParameter in m_AssetParameters on spreadParameter.AssetId equals assetParameter.AssetId
                // UNDONE demande à Phil s'il faut prendre les futures en livraison pour le spread (maintenant il font part de la position spot)
                where spreadParameter.Type == RiskMethodQtyType.Future || spreadParameter.Type == RiskMethodQtyType.FutureMoff
                select new PositionsGrouping.NettingParameters<string, decimal>
                {
                    AssetId = spreadParameter.AssetId,
                    Type = spreadParameter.Type,
                    // we may force F because the parameters are filtered by RiskMethodQtyType Future, FutureMoff
                    ContractCategory = "F",
                    Maturity = spreadParameter.MaturityYearMonth,
                    // This value should be unuseful, because the multiplier should be the same for future contracts inside the same class
                    Multiplier = assetParameter.Multiplier,
                    GroupingParameters = new Pair<string, decimal>(pMarginClass, spreadParameter.MaturityYearMonth),
                };

            groupedFuturesPositionsByClassMaturity = PositionsGrouping.GroupFuturePositions<string, decimal>
                (pClassPositions, parametersXGrouping);

            // 2 - Create the list of all the possible quantities to spread
            //  where we balance the spot/back month positions in order to identifiy the net spreading positions

            SpreadElement[] quantitiesToSpread = (
                from position in groupedFuturesPositionsByClassMaturity
                join spreadParameter in pSpreadEvalParams on position.First.idAsset equals spreadParameter.AssetId
                // the expiration month closest to the maturity date is called the “spot month”, 
                // and the associated contract is the “front contract”. 
                // According to the market specification the quantity related to the front contract has to be spreaded as first. 
                // A quantity related to the front contract it is the first of the list (according to the sp), earlier positions should have already reached their own maturity date.
                orderby spreadParameter.MaturityYearMonth ascending
                select new SpreadElement
                {
                    MaturityYearMonth = spreadParameter.MaturityYearMonth,

                    Quantity = System.Math.Abs(position.Second.QuantityWithMarginSign(spreadParameter.Type, position.First.Side)),

                    Side = position.First.Side,

                    QuantityToSpread = System.Math.Abs(position.Second.QuantityWithMarginSign(spreadParameter.Type, position.First.Side)),

                    Currency = spreadParameter.Currency,

                    MaturityRuleFrequency = spreadParameter.MaturityRuleFrequency
                }
                ).ToArray();

            // 3 - Compute the spreading quantity

            List<TimsFactorCommunicationObject> factors = new List<TimsFactorCommunicationObject>();

            foreach (SpreadElement quantityToSpread in quantitiesToSpread)
            {
                // PM 20130705 : Sur Eurex le Spot Month ne démarre que si c'est le mois courant
                //bool spotMonth = RiskTools.IsSpotMonth(quantityToSpread.MaturityYearMonth, quantityToSpread.MaturityRuleFrequency, this.DtBusiness);
                bool spotMonth = (quantityToSpread.MaturityYearMonth == this.DtBusiness.InNumericYearMonth());

                // Identifying when we are treating a spot element
                decimal SpreadRate =
                    spotMonth ? pSpotMonthSpreadRate : pBackMonthSpreadRate;
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                decimal currQuantity = quantityToSpread.QuantityToSpread;

                // balancing elements list
                SpreadElement[] quantitiesToBalanceVs = (
                    from quantityToBalanceVs in quantitiesToSpread
                    where
                        // the current quantity can be balanced against late maturity with opposite side
                        quantityToBalanceVs.MaturityYearMonth > quantityToSpread.MaturityYearMonth &&
                        quantityToBalanceVs.Side != quantityToSpread.Side
                    select quantityToBalanceVs)
                    .ToArray();

                for (int idx = 0; idx < quantitiesToBalanceVs.Length; idx++)
                {
                    SpreadElement quantityToBalanceVs = quantitiesToBalanceVs[idx];
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    decimal spreadedQuantity = System.Math.Min(quantityToBalanceVs.QuantityToSpread, currQuantity);

                    // the spread quantity can be balanced just against one other element, the spread quantity must be decremented
                    quantityToBalanceVs.QuantityToSpread -= spreadedQuantity;

                    factors.Add(
                        new TimsFactorCommunicationObject
                        {
                            SpotMonth = spotMonth,

                            Identifier = String.Format("{0}vs{1}", quantityToSpread.MaturityYearMonth, quantityToBalanceVs.MaturityYearMonth),

                            Quote = SpreadRate,

                            Quantity = spreadedQuantity,

                            MarginAmount =
                                new Money(
                                    this.RoundAmount(
                                //
                                    spreadedQuantity * SpreadRate
                                //
                                ),
                                quantityToSpread.Currency),

                        });

                    currQuantity -= spreadedQuantity;

                    // If the back/spot month net position can be completely balanced, 
                    //  the back/spot month spread margin corresponds to the spot month net position of the current element 
                    //  and no others spreads are possible.
                    if (currQuantity == 0)
                    {
                        break;
                    }
                }
            }

            TimsDecomposableParameterCommunicationObject spreadComObj = null;

            if (factors.Count() > 0)
            {

                spreadComObj = new TimsDecomposableParameterCommunicationObject();

                List<Money> amounts = new List<Money>();
                SumAmounts(from factor in factors select (Money)factor.MarginAmount, ref amounts);

                spreadComObj.Factors = factors;

                spreadComObj.MarginAmount = amounts[0];

            }

            return spreadComObj;

        }

        /// <summary>
        /// Compensate short option open positions.
        /// When short option positions are embedded in complex portfolios, 
        /// a major part of the risk may be compensated either by long option positions or by corresponding future positions. 
        /// For the uncompensated part of short options positions, short option minimum is used. 
        /// This cross margining is provided within a margin class
        /// </summary>
        /// <param name="pMarginClassPositions">the margin class positions used in the cross margining procedure. 
        /// Be aware that all the provided set of positions enter the cross margining. 
        /// According to the specification just the positions inside of the same margin class may be compensated.</param>
        /// <returns>a collection of compensated quantities, the original positions will NOT changed at all</returns>
        private IEnumerable<ShortOptionCompensationCommunicationObject> ShortOptionCompensation(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pMarginClassPositions)
        {
            IEnumerable<Compensation> elementsToCompensate = (
                from position in pMarginClassPositions
                // just short (side 2) open (quantity != 0) position will be compensated
                where position.First.Side == "2" && position.Second.Quantity != 0
                join assetParam in m_AssetParameters on position.First.idAsset equals assetParam.AssetId
                join maturityParam in m_MaturityParameters on assetParam.MaturityId equals maturityParam.MaturityId
                // if PutOrCall == null the related position 
                where maturityParam.PutOrCall != null
                select new Compensation
                {
                    AssetId = assetParam.AssetId,

                    // just open option positions will be compensated
                    Type = base.GetRiskMethodQtyType("O", maturityParam.PutOrCall, false),

                    Multiplier = assetParam.Multiplier,

                    // just short position will be compensated
                    Side = "2",

                    MaturityYearMonth = maturityParam.MaturityYearMonth,

                    StrikePrice = assetParam.Strike,

                    CompensationQuantity = position.Second.Quantity * assetParam.Multiplier,

                    Quantity = position.Second.Quantity,

                    CompensatedQuantity = 0,

                })/*.ToArray()*/;


            Compensation[] elementsToCompensateOrdered = (
                from elementToCompensate in elementsToCompensate
                where elementToCompensate.Type == RiskMethodQtyType.Call
                // PM 20130918 [18819] Ne pas faire de test sur les valeurs max de ShortAdj et Theoretical
                //join max in MaxMarginParameters on elementToCompensate.AssetId equals max.AssetId
                //where max.MaxShortAdj > max.MaxTheoreticalValue
                orderby elementToCompensate.MaturityYearMonth ascending, elementToCompensate.StrikePrice descending
                select elementToCompensate
                ).Union(
                from elementToCompensate in elementsToCompensate
                where elementToCompensate.Type == RiskMethodQtyType.Put
                // PM 20130918 [18819] Ne pas faire de test sur les valeurs max de ShortAdj et Theoretical
                //join max in MaxMarginParameters on elementToCompensate.AssetId equals max.AssetId
                //where max.MaxShortAdj > max.MaxTheoreticalValue
                orderby elementToCompensate.MaturityYearMonth ascending, elementToCompensate.StrikePrice ascending
                select elementToCompensate
                ).ToArray();

            IEnumerable<Compensation> elementsCompensatingOption =
                from position in pMarginClassPositions
                // just open (quantity != 0) position will be useed to compensate 
                where position.Second.Quantity != 0
                join assetParam in m_AssetParameters on position.First.idAsset equals assetParam.AssetId
                join maturityParam in m_MaturityParameters on assetParam.MaturityId equals maturityParam.MaturityId
                where
                    // Any short call position risk is limited by long call positions. 
                    // Any short put position risk is limited by long put positions.
                (maturityParam.PutOrCall != null && position.First.Side == "1")
                select new Compensation
                {
                    AssetId = assetParam.AssetId,

                    Type = base.GetRiskMethodQtyType("O", maturityParam.PutOrCall, false),

                    Multiplier = assetParam.Multiplier,

                    // compensating positions can include long and short positions (futures contracts)
                    Side = position.First.Side,

                    MaturityYearMonth = maturityParam.MaturityYearMonth,

                    StrikePrice = assetParam.Strike,

                    CompensationQuantity = position.Second.Quantity * assetParam.Multiplier,

                    Quantity = position.Second.Quantity,

                    // not used for compensating elements
                    CompensatedQuantity = 0,

                };

            IEnumerable<Compensation> elementsCompensatingFuture =
                from position in pMarginClassPositions
                // just open (quantity != 0) position will be useed to compensate 
                where position.Second.Quantity != 0
                join assetParam in m_AssetParameters on position.First.idAsset equals assetParam.AssetId
                join maturityParam in m_MaturityParameters on assetParam.MaturityId equals maturityParam.MaturityId
                where
                    // Any short call position risk is limited by any long future position. 
                    // Any short put position risk is limited by any short future position.
                (maturityParam.PutOrCall == null)
                select new Compensation
                {
                    AssetId = assetParam.AssetId,

                    Type = base.GetRiskMethodQtyType("F", null, false),

                    Multiplier = assetParam.Multiplier,

                    // compensating positions can include long and short positions (futures contracts)
                    Side = position.First.Side,

                    MaturityYearMonth = maturityParam.MaturityYearMonth,

                    StrikePrice = assetParam.Strike,

                    CompensationQuantity = position.Second.Quantity * assetParam.Multiplier,

                    Quantity = position.Second.Quantity,

                    // not used for compensating elements
                    CompensatedQuantity = 0,

                };

            IEnumerable<Compensation> elementsCompensatingFutureGrouped =
                from elementsFuture in elementsCompensatingFuture
                group elementsFuture by elementsFuture.Type into elementsFutureGrouped
                select new Compensation
                {
                    AssetId = elementsFutureGrouped.Min(c => c.AssetId),

                    Type = elementsFutureGrouped.Key,

                    Multiplier = elementsFutureGrouped.Min(c => c.Multiplier),

                    // compensating positions can include long and short positions (futures contracts)
                    Side = elementsFutureGrouped.Sum(c => (c.Side == "1" ? 1 : -1) * c.Quantity) < 0 ? "2" : "1",

                    MaturityYearMonth = elementsFutureGrouped.Min(c => c.MaturityYearMonth),

                    StrikePrice = elementsFutureGrouped.Min(c => c.StrikePrice),

                    CompensationQuantity = System.Math.Abs(elementsFutureGrouped.Sum(c => (c.Side == "1" ? 1 : -1) * c.Quantity * c.Multiplier)),

                    Quantity = System.Math.Abs(elementsFutureGrouped.Sum(c => (c.Side == "1" ? 1 : -1) * c.Quantity)),

                    // not used for compensating elements
                    CompensatedQuantity = 0,
                };

            Compensation[] elementsCompensating = elementsCompensatingOption.Concat(elementsCompensatingFutureGrouped).ToArray();

            /*
            Compensation[] elementsCompensating = (
                from position in pMarginClassPositions
                // just open (quantity != 0) position will be useed to compensate 
                where position.Second.Quantity != 0
                join assetParam in m_AssetParameters on position.First.idAsset equals assetParam.AssetId
                join maturityParam in m_MaturityParameters on assetParam.MaturityId equals maturityParam.MaturityId
                where 
                // Any short call position risk is limited by long call positions or by any long future position. 
                // Any short put position risk is limited by long put positions or by any short future position.
                (maturityParam.PutOrCall != null && position.First.side == "1") 
                ||
                (maturityParam.PutOrCall == null)
                select new Compensation
                {
                    AssetId = assetParam.AssetId,

                    Type = base.GetRiskMethodQtyType(
                        maturityParam.PutOrCall != null ? "O" : "F", 
                        maturityParam.PutOrCall, false),

                    Multiplier = assetParam.Multiplier,

                    // compensating positions can include long and short positions (futures contracts)
                    Side = position.First.side,

                    MaturityYearMonth = maturityParam.MaturityYearMonth,

                    StrikePrice = assetParam.Strike,

                    CompensationQuantity = position.Second.Quantity * assetParam.Multiplier,

                    Quantity = position.Second.Quantity,

                    // not used for compensating elements
                    CompensatedQuantity = 0,

                }).ToArray();
            */
            IEnumerable<ShortOptionCompensationCommunicationObject> shortCompensatedOptions = null;

            for (
                int iElemToCompensate = 0;
                elementsCompensating.Length > 0 && iElemToCompensate < elementsToCompensateOrdered.Length;
                iElemToCompensate++)
            {
                Compensation elemToCompensate = elementsToCompensateOrdered[iElemToCompensate];

                IEnumerable<Compensation>/*[]*/ elementsCompensatingFiltered =
                    FilterCompensationElements(elemToCompensate, elementsCompensating);

                if (elementsCompensatingFiltered.Count() == 0)
                {
                    continue;
                }

                Compensation[] elementsCompensatingOrdered =
                    OrderCompensationElements(elemToCompensate, elementsCompensatingFiltered);

                for (int iElemCompensating = 0; iElemCompensating < elementsCompensatingOrdered.Length; iElemCompensating++)
                {
                    Compensation elemCompensating = elementsCompensatingOrdered[iElemCompensating];

                    decimal minQuantity = System.Math.Min(elemCompensating.CompensationQuantity, elemToCompensate.CompensationQuantity);

                    elemCompensating.CompensationQuantity -= minQuantity;
                    // for  positions used for compensation lowest integer value not smaller than x.
                    // ceiling( number of long positions * trade unit value of long position / trade unit value of long position )
                    // EG 20170127 Qty Long To Decimal
                    elemCompensating.Quantity = System.Math.Ceiling(elemCompensating.CompensationQuantity / elemCompensating.Multiplier);

                    elemToCompensate.CompensationQuantity -= minQuantity;
                    // for compensated short positions highest integer value not greater than x
                    // floor( compensated short positionss * trade unit value of short position / trade unit value of short position )
                    // EG 20170127 Qty Long To Decimal
                    elemToCompensate.Quantity = System.Math.Floor(elemToCompensate.CompensationQuantity / elemToCompensate.Multiplier);
                    // for compensated short positions highest integer value not greater than x
                    // EG 20170127 Qty Long To Decimal
                    elemToCompensate.CompensatedQuantity += System.Math.Floor(minQuantity / elemToCompensate.Multiplier);

                    if (elemToCompensate.CompensationQuantity == 0)
                    {
                        break;
                    }
                }

            }

            shortCompensatedOptions =
                from elemToCompensate in elementsToCompensateOrdered
                where elemToCompensate.CompensatedQuantity > 0
                select new ShortOptionCompensationCommunicationObject
                {
                    AssetId = elemToCompensate.AssetId,

                    Quantity = elemToCompensate.CompensatedQuantity
                };

            return shortCompensatedOptions;
        }

        private Compensation[] OrderCompensationElements(
            Compensation pElemToCompensate, IEnumerable<Compensation> pElementsFiltered)
        {
            Compensation[] elementsCompensatingOrdered = null;

            switch (pElemToCompensate.Type)
            {
                case RiskMethodQtyType.Call:
                    elementsCompensatingOrdered = (
                        pElementsFiltered
                        .OrderByDescending(elem => elem.StrikePrice)
                        .OrderBy(elem => elem.MaturityYearMonth)
                        // Call positions at first, then Futures
                        .OrderBy(elem => elem.Type)
                        ).ToArray();

                    break;

                case RiskMethodQtyType.Put:

                    elementsCompensatingOrdered = (
                        pElementsFiltered
                        .OrderBy(elem => elem.StrikePrice)
                        .OrderBy(elem => elem.MaturityYearMonth)
                        // Put positions at first, then Futures
                        .OrderByDescending(elem => elem.Type)
                        ).ToArray();

                    break;
            }

            return elementsCompensatingOrdered;
        }

        private static IEnumerable<Compensation>/*[]*/ FilterCompensationElements(
            Compensation pElemToCompensate, Compensation[] pElementsCompensating)
        {
            IEnumerable<Compensation>/*[]*/ elementsCompensatingFiltered = null;

            switch (pElemToCompensate.Type)
            {
                case RiskMethodQtyType.Call:
                    elementsCompensatingFiltered = pElementsCompensating.Where(
                        compensatingElement => (
                            compensatingElement.CompensationQuantity > 0 &&
                            // any short call position risk is limited by any long future position
                            ((compensatingElement.Type == RiskMethodQtyType.Future && compensatingElement.Side == "1")
                            ||
                            // any short call position risk is limited by long call positions 
                            //  with the same or longer time to expiration and lower or equal strike
                            (compensatingElement.Type == RiskMethodQtyType.Call && compensatingElement.Side == "1"
                            && pElemToCompensate.MaturityYearMonth <= compensatingElement.MaturityYearMonth
                            && pElemToCompensate.StrikePrice >= compensatingElement.StrikePrice))))
                        /*.ToArray()*/;

                    break;

                case RiskMethodQtyType.Put:

                    elementsCompensatingFiltered = pElementsCompensating.Where(
                        compensatingElement => (
                            compensatingElement.CompensationQuantity > 0 &&
                            // any short put position risk is limited by any short future position
                            ((compensatingElement.Type == RiskMethodQtyType.Future && compensatingElement.Side == "2")
                            ||
                            // any short put position risk is limited by long put positions 
                            //  with the same or longer time to expiration and higher or equal strike
                            (compensatingElement.Type == RiskMethodQtyType.Put && compensatingElement.Side == "1"
                            && pElemToCompensate.MaturityYearMonth <= compensatingElement.MaturityYearMonth
                            && pElemToCompensate.StrikePrice <= compensatingElement.StrikePrice))))
                        /*.ToArray()*/;

                    break;
            }

            return elementsCompensatingFiltered;
        }

        /// <summary>
        /// Compute the additional margin for a specific contract
        /// </summary>
        /// <param name="pContractPositions">positions related to the contract</param>
        /// <param name="pMarginContract">contract symbol</param>
        /// <param name="pCompensatedShortOptionQuantities">
        /// short options quantities where the short option compensation (cross margining for a margin class) has been used</param>
        /// <returns>a communication object containing the additional margin and the related details</returns>
        private TimsDecomposableParameterCommunicationObject GetContractAdditionalMargin(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pContractPositions,
            IEnumerable<AdditionalEvaluationParameters> pAdditionalEvalParams, string pMarginContract,
            IEnumerable<ShortOptionCompensationCommunicationObject> pCompensatedShortOptionQuantities)
        {
            List<TimsFactorCommunicationObject> factors = new List<TimsFactorCommunicationObject>();

            // 1. adding the calculation factors per any asset in position

            factors.AddRange(

                from position in pContractPositions
                join additionalParam in pAdditionalEvalParams on position.First.idAsset equals additionalParam.AssetId
                where
                    additionalParam.Type == RiskMethodQtyType.Future ||
                    additionalParam.Type == RiskMethodQtyType.Call ||
                    additionalParam.Type == RiskMethodQtyType.Put ||
                    // UNDONE demande Phil s'il faut prendre les dénouements/liquidations
                    additionalParam.Type == RiskMethodQtyType.ExeAssCall ||
                    additionalParam.Type == RiskMethodQtyType.ExeAssPut ||
                    additionalParam.Type == RiskMethodQtyType.FutureMoff

                group additionalParam

                by new
                {

                    AssetId = position.First.idAsset,
                    additionalParam.Type,
                    PosType = position.Second.GetPosType(additionalParam.Type),
                    Quantity = position.Second.QuantityWithMarginSign(additionalParam.Type, position.First.Side),
                    position.Second.DeliveryDate,
                    additionalParam.Multiplier,

                }

                    into groupedParameters

                    let compensatedQuantity = pCompensatedShortOptionQuantities
                                .Where(elem => elem.AssetId == groupedParameters.Key.AssetId)
                                .Select(elem => elem.Quantity).FirstOrDefault()

                    select new TimsFactorCommunicationObject
                    {
                        AssetId = groupedParameters.Key.AssetId,

                        PosType = groupedParameters.Key.PosType,

                        Multiplier = groupedParameters.Key.Multiplier,

                        RiskArray = GetRiskArray(
                            groupedParameters.Key.Quantity,
                            compensatedQuantity,
                            groupedParameters.Key.Multiplier,
                            groupedParameters.Key.Type,
                            from elem in groupedParameters select elem),

                        DeliveryDate = groupedParameters.Key.DeliveryDate,

                        Quantity = groupedParameters.Key.Quantity,
                    }


            );

            //2. Compute the total additional margin (for the current product) by risk array index and underlying price indicator

            TimsDecomposableParameterCommunicationObject additionalComObj = null;

            if (factors.Count() > 0)
            {
                additionalComObj = new TimsDecomposableParameterCommunicationObject();

                // The previously built sidepoint elements will be grouped by 
                // projected underlying price/volatility indicator, index of the riskarray, maturity 
                // (but the maturity is counted just when the maturity switch is activated)
                TimsFactorCommunicationObject sumFactor =
                   new TimsFactorCommunicationObject
                   {

                       Identifier = pMarginContract,

                       RiskArray = (
                        from factor in factors
                        join parameter in pAdditionalEvalParams on factor.AssetId equals parameter.AssetId
                        from sidepoint in factor.RiskArray
                        group sidepoint by new
                        {
                            sidepoint.RiskArrayIndex,
                            sidepoint.QuoteUnlVsQuote_Indicator,
                            sidepoint.Volatility_Indicator,
                            MaturityYearMonth = (parameter.MaturitySwitch == MaturitySwitch.Yes ? parameter.MaturityYearMonth : (decimal?)null),
                            MaturityFactor = (parameter.MaturitySwitch == MaturitySwitch.Yes ? parameter.MaturityFactor : (decimal?)null)
                        }
                            into groupedSidePoints

                            select
                                GetRiskArrayElement(
                                null, null,
                                groupedSidePoints.Key.QuoteUnlVsQuote_Indicator,
                                groupedSidePoints.Key.Volatility_Indicator,
                                groupedSidePoints.Key.RiskArrayIndex,
                                groupedSidePoints.Key.MaturityYearMonth,
                                groupedSidePoints.Key.MaturityFactor,
                                SumAmounts(from sidePoint in groupedSidePoints select (Money)sidePoint.MarginAmount, null).First()
                                )

                       ).ToArray(),
                   };

                // 2.1 Build the Price Up/Price down elements, usually missing for futures contracts
                List<TimsFactorCommunicationObject> completedRiskArray =
                    BuildLackingVolatilityElements(sumFactor.RiskArray);

                if (completedRiskArray.Count > sumFactor.RiskArray.Length)
                {
                    sumFactor.RiskArray = completedRiskArray.ToArray();
                }

                factors.Add(sumFactor);

                additionalComObj.Factors = factors;
            }

            return additionalComObj;
        }

        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private TimsFactorCommunicationObject[] GetRiskArray(
            decimal pQuantity, decimal pCompensatedQuantity,
            decimal pMultiplier, RiskMethodQtyType pType,
            IEnumerable<AdditionalEvaluationParameters> pParameters)
        {
            // order all the risk projected parameters by price indicator and scenario (to help the building of the PDF report) 
            pParameters = pParameters
                .OrderBy(elem => elem.RiskArrayIndex)
                .OrderBy(elem => elem.QuoteUnlVsQuote_Indicator);

            // the risk array lenght is computed on the total number of the parameters (we have one parameter per "scenario/price indicator") time
            //  the constant 3 which is the number of the possible volatility types (U, N, D) 
            int lengtharray = pParameters.Count() * 3;

            TimsFactorCommunicationObject[] riskarray = new TimsFactorCommunicationObject[lengtharray];

            int idxarray = 0;

            foreach (AdditionalEvaluationParameters parameter in pParameters)
            {

                // UNDONE MF 20120103 - this control is EUROSYS specific, the EUREX market specification does not ..
                //  .. say anything about a particular case where the quote underlying indicator should be ..
                //  ... used for the additional margin evaluation.

                // the short option adjustement is used just for open option positions having underlying price projection type of 
                //   1 -> zero price movement or 2 ->  underlying price.
                bool mayUseShortAdj =
                    (
                    (parameter.Type == RiskMethodQtyType.Call && parameter.QuoteUnlIndicator == 1)
                    ||
                    (parameter.Type == RiskMethodQtyType.Put && parameter.QuoteUnlIndicator == 2)
                    )
                    // the quantity is proposed with the margin sign (according with the RiskMethodQtyType)
                    && pQuantity > 0;

                if (pType == RiskMethodQtyType.Future ||
                    pType == RiskMethodQtyType.Call ||
                    pType == RiskMethodQtyType.Put ||
                    // FutureMoff ?
                    pType == RiskMethodQtyType.FutureMoff)
                {
                    // Up volatility

                    riskarray[idxarray] = GetRiskArrayElement
                        (pQuantity, pCompensatedQuantity, pMultiplier, mayUseShortAdj,
                        parameter.UpShortAdj, parameter.UpTheoreticalValue, parameter.UpTheoreticalExists,
                        parameter.QuoteUnlVsQuote_Indicator, parameter.RiskArrayIndex, parameter.Currency, "U");

                    // Neutral volatility

                    idxarray++;

                    riskarray[idxarray] =
                        GetRiskArrayElement
                        (pQuantity, pCompensatedQuantity, pMultiplier, mayUseShortAdj,
                        parameter.NtrlShortAdj, parameter.NtrlTheoreticalValue, parameter.NtrlTheoreticalExists,
                        parameter.QuoteUnlVsQuote_Indicator, parameter.RiskArrayIndex, parameter.Currency, "N");

                    // Down volatility

                    idxarray++;

                    riskarray[idxarray] =
                        GetRiskArrayElement
                        (pQuantity, pCompensatedQuantity, pMultiplier, mayUseShortAdj,
                        parameter.DownShortAdj, parameter.DownTheoreticalValue, parameter.DownTheoreticalExists,
                        parameter.QuoteUnlVsQuote_Indicator, parameter.RiskArrayIndex, parameter.Currency, "D");

                    // next 3 parameters..

                    idxarray++;
                }
                if (pType == RiskMethodQtyType.ExeAssCall ||
                    pType == RiskMethodQtyType.ExeAssPut)
                {
                    // EA

                    // Up volatility

                    riskarray[idxarray] = GetRiskArrayElement
                        (pQuantity, pCompensatedQuantity, pMultiplier, mayUseShortAdj,
                        parameter.UpShortAdj, parameter.RiskValueExeAss, true,
                        parameter.QuoteUnlVsQuote_Indicator, parameter.RiskArrayIndex, parameter.Currency, "U");

                    // Neutral volatility

                    idxarray++;

                    riskarray[idxarray] =
                        GetRiskArrayElement
                        (pQuantity, pCompensatedQuantity, pMultiplier, mayUseShortAdj,
                        parameter.NtrlShortAdj, parameter.RiskValueExeAss, true,
                        parameter.QuoteUnlVsQuote_Indicator, parameter.RiskArrayIndex, parameter.Currency, "N");

                    // Down volatility

                    idxarray++;

                    riskarray[idxarray] =
                        GetRiskArrayElement
                        (pQuantity, pCompensatedQuantity, pMultiplier, mayUseShortAdj,
                        parameter.DownShortAdj, parameter.RiskValueExeAss, true,
                        parameter.QuoteUnlVsQuote_Indicator, parameter.RiskArrayIndex, parameter.Currency, "D");

                    // next 3 parameters..

                    idxarray++;


                }

            }

            // excluding the null (0 amount) sidepoint
            return riskarray.Where(sidepoint => sidepoint != null).ToArray();
        }

        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private TimsFactorCommunicationObject GetRiskArrayElement(
            decimal pQuantity, decimal pCompensatedQuantity, decimal pMultiplier, bool pMayUseShort,
            decimal pShortAdj, decimal pTheoreticalValue, bool pTheoreticalValueExists, string pQuoteUnlVsQuote_Indicator,
            int pRiskArrayIndex, string pCurrency, string pVolatility_Indicator)
        {
            bool shortAdj = pMayUseShort && (pShortAdj > pTheoreticalValue);

            decimal riskValue = pTheoreticalValue;

            if (shortAdj)
            {
                riskValue = pShortAdj;

                pQuantity -= pCompensatedQuantity;
            }
            else
            {
                pCompensatedQuantity = 0;
            }

            TimsFactorCommunicationObject arrayElement = null;

            if (pTheoreticalValueExists)
            {
                arrayElement = new TimsFactorCommunicationObject
                {
                    Identifier = GetSidePointIdentifier(pQuoteUnlVsQuote_Indicator, pVolatility_Indicator),

                    QuoteUnlVsQuote_Indicator = pQuoteUnlVsQuote_Indicator,

                    Volatility_Indicator = pVolatility_Indicator,

                    RiskArrayIndex = pRiskArrayIndex,

                    ShortAdj = shortAdj,

                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    CompensatedQuantity = pCompensatedQuantity == 0 ? (decimal?)null : pCompensatedQuantity,

                    Quote = riskValue,

                    MarginAmount =
                        new Money(
                        //
                            pQuantity *
                            this.RoundAmount
                            (

                            riskValue * pMultiplier

                            )

                            +

                            pCompensatedQuantity *
                            this.RoundAmount
                            (

                            pTheoreticalValue * pMultiplier

                            )
                        //
                            ,
                            pCurrency),
                };
            }

            return arrayElement;
        }

        private List<TimsFactorCommunicationObject> BuildLackingVolatilityElements(
            IEnumerable<TimsFactorCommunicationObject> pRiskArray)
        {
            List<TimsFactorCommunicationObject> riskArrayCompleted = pRiskArray.ToList();

            var groupedElements =
                from elem in pRiskArray
                group elem by
                    new
                    {
                        elem.RiskArrayIndex,

                        elem.MaturityYearMonth,

                        elem.MaturityFactor,

                        elem.QuoteUnlVsQuote_Indicator
                    };

            foreach (var groupedElement in groupedElements)
            {
                TimsFactorCommunicationObject volatilityNeutral = groupedElement.FirstOrDefault(elem => elem.Volatility_Indicator == "N");

                if (volatilityNeutral != null)
                {
                    TimsFactorCommunicationObject volatilityUp = groupedElement.FirstOrDefault(elem => elem.Volatility_Indicator == "U");

                    if (volatilityUp == null)
                    {
                        volatilityUp = GetRiskArrayElement(
                            volatilityNeutral.ShortAdj,
                            volatilityNeutral.Quote,
                            groupedElement.Key.QuoteUnlVsQuote_Indicator,
                            "U",
                            groupedElement.Key.RiskArrayIndex,
                            groupedElement.Key.MaturityYearMonth,
                            groupedElement.Key.MaturityFactor,
                            volatilityNeutral.MarginAmount);

                        riskArrayCompleted.Add(volatilityUp);
                    }

                    TimsFactorCommunicationObject volatilityDown = groupedElement.FirstOrDefault(elem => elem.Volatility_Indicator == "D");

                    if (volatilityDown == null)
                    {
                        volatilityDown = GetRiskArrayElement(
                            volatilityNeutral.ShortAdj,
                            volatilityNeutral.Quote,
                            groupedElement.Key.QuoteUnlVsQuote_Indicator,
                            "D",
                            groupedElement.Key.RiskArrayIndex,
                            groupedElement.Key.MaturityYearMonth,
                            groupedElement.Key.MaturityFactor,
                            volatilityNeutral.MarginAmount);

                        riskArrayCompleted.Add(volatilityDown);
                    }

                }
            }

            return riskArrayCompleted;
        }

        private TimsFactorCommunicationObject GetRiskArrayElement(
            bool? pShortAdj, decimal? pQuote, string pQuoteUnlVsQuote_Indicator, string pVolatility_Indicator,
            int? pRiskArrayIndex, decimal? pMaturityYearMonth, decimal? pMaturityFactor, IMoney pAmount)
        {
            return new TimsFactorCommunicationObject
            {
                Identifier = GetSidePointIdentifier(pQuoteUnlVsQuote_Indicator, pVolatility_Indicator),

                QuoteUnlVsQuote_Indicator = pQuoteUnlVsQuote_Indicator,

                Volatility_Indicator = pVolatility_Indicator,

                RiskArrayIndex = pRiskArrayIndex,

                MaturityYearMonth = pMaturityYearMonth,

                MaturityFactor = pMaturityFactor,

                ShortAdj = pShortAdj,

                Quote = pQuote,

                MarginAmount = new Money(pAmount.Amount.DecValue, pAmount.GetCurrency.Value),

            };
        }

        private string GetSidePointIdentifier(string pQuoteUnlVsQuote_Indicator, string pVolatility_Indicator)
        {
            return String.Format("P{0}V{1}", pQuoteUnlVsQuote_Indicator, pVolatility_Indicator);
        }

        private TimsDecomposableParameterCommunicationObject GetClassAdditionalMargin(IEnumerable<TimsDecomposableParameterCommunicationObject> pDecomposableParameters,
            decimal pLiquidatingClassMarginAmount, decimal pOffsetGroup)
        {
            TimsDecomposableParameterCommunicationObject marginComObj = null;

            // PM 20130628 Correction pour si aucune valeur de risque
            if ((pDecomposableParameters != default(IEnumerable<TimsDecomposableParameterCommunicationObject>))
                && (pDecomposableParameters.Count() > 0)
                && (pDecomposableParameters.Any(p => p != default(TimsDecomposableParameterCommunicationObject))))
            {
                // build the risk array including the total additional margins for the current class
                IEnumerable<TimsFactorCommunicationObject> riskArrayContract =
                    from parameter in pDecomposableParameters
                    // filter on the lower level parameters that have an additional margin
                    where parameter != null
                    from factor in parameter.Factors
                    // get the total factor of each valid parameter (it is the only one that does not have any asset id related to)
                    where !factor.AssetId.HasValue
                    from sidepoint in factor.RiskArray

                    group sidepoint by
                        // grouping the result by risk array index and underlyingprice/volatility indicators
                    new
                    {
                        sidepoint.RiskArrayIndex,

                        sidepoint.QuoteUnlVsQuote_Indicator,
                        sidepoint.Volatility_Indicator,

                        sidepoint.MaturityYearMonth,
                        sidepoint.MaturityFactor,

                        Currency = sidepoint.MarginAmount.GetCurrency.Value,
                    }

                        into groupedSidePoints

                        select GetRiskArrayElement(
                            null,
                            null,
                            groupedSidePoints.Key.QuoteUnlVsQuote_Indicator,
                            groupedSidePoints.Key.Volatility_Indicator,
                            groupedSidePoints.Key.RiskArrayIndex,
                            groupedSidePoints.Key.MaturityYearMonth,
                            groupedSidePoints.Key.MaturityFactor,
                            SumAmounts(
                                from sidePoint in groupedSidePoints
                                select (Money)sidePoint.MarginAmount, null).First());

                // Find the max per  underlyingprice/volatility indicators
                IEnumerable<TimsFactorCommunicationObject> riskArrayMax =
                    from sidepoint in riskArrayContract
                    group sidepoint by new
                    {
                        sidepoint.QuoteUnlVsQuote_Indicator,
                        sidepoint.Volatility_Indicator,

                        sidepoint.MaturityYearMonth,
                        sidepoint.MaturityFactor,

                        Currency = sidepoint.MarginAmount.GetCurrency.Value,
                    }
                        into groupedSidePoints
                        select groupedSidePoints.Aggregate(
                            (curr, next) => next != null && curr.MarginAmount.Amount.DecValue > next.MarginAmount.Amount.DecValue ?
                                curr
                                :
                                next)
                            into maxSidePoints
                            select GetRiskArrayElement(
                                null,
                                null,
                                maxSidePoints.QuoteUnlVsQuote_Indicator,
                                maxSidePoints.Volatility_Indicator,
                                maxSidePoints.RiskArrayIndex,
                                maxSidePoints.MaturityYearMonth,
                                maxSidePoints.MaturityFactor,
                                maxSidePoints.MarginAmount);

                TimsFactorCommunicationObject[] riskArrayMaxTreatedNeutralPrices =
                    TreatNeutralPrices(riskArrayMax, pLiquidatingClassMarginAmount);

                TimsFactorCommunicationObject[] riskArrayMaxAdjusted =
                    AdjustRiskArrayWithPremium(riskArrayMaxTreatedNeutralPrices, pLiquidatingClassMarginAmount);

                TimsFactorCommunicationObject[] riskArrayMaxAdjustedWithOffset =
                    OffsetRiskArray(riskArrayMaxAdjusted, pOffsetGroup, true);

                TimsFactorCommunicationObject maxAdjusted =
                    riskArrayMaxAdjustedWithOffset.Aggregate(
                        (curr, next) => next != null && curr.MarginAmount.Amount.DecValue > next.MarginAmount.Amount.DecValue ?
                            curr
                            :
                            next);

                marginComObj = new TimsDecomposableParameterCommunicationObject
                {
                    Factors =
                    new TimsFactorCommunicationObject[]
                { 
                    // total Additional margins per class aggregate by scenario,underlying price/volatitlity indicator, maturity
                    new TimsFactorCommunicationObject
                    {
                        RiskArray = riskArrayContract.ToArray(),
                    },

                    // max Additional margins per underlying price/volatitlity indicator,maturity
                    new TimsFactorCommunicationObject
                    {
                        Identifier = cm_MaxUnadjusted,

                        RiskArray = riskArrayMax.ToArray(),
                    }, 

                    // max Additional margins with credit amounts adjusted according with the neutral prices values 
                    // see also the TreatNeutralPrices method
                    new TimsFactorCommunicationObject
                    {
                        Identifier = cm_MaxUnadjustedNeutralPrices,

                        RiskArray = riskArrayMaxAdjusted,
                    },

                    // max Additional margins adjusted via the margin class premium amount
                    new TimsFactorCommunicationObject
                    {
                        Identifier = cm_MaxAdjusted,

                        RiskArray = riskArrayMaxAdjustedWithOffset,
                    } ,

                    // Additional amount (pure, without maturity factor)
                    GetRiskArrayElement(null, null,
                        maxAdjusted.QuoteUnlVsQuote_Indicator, maxAdjusted.Volatility_Indicator,
                        maxAdjusted.RiskArrayIndex, maxAdjusted.MaturityYearMonth, maxAdjusted.MaturityFactor, maxAdjusted.MarginAmount),

                },

                    MarginAmount = AdjustFactor(
                    maxAdjusted,
                    maxAdjusted.MaturityFactor ?? 0)
                        .MarginAmount
                };

            }
            return marginComObj;
        }

        private TimsFactorCommunicationObject[] TreatNeutralPrices(
            IEnumerable<TimsFactorCommunicationObject> pRiskArrayMax, decimal pPremiumClassMarginAmount)
        {
            TimsFactorCommunicationObject[] riskArrayNeutralPrices = (
                from elem in pRiskArrayMax
                select
                    GetRiskArrayElement(
                    null, null, elem.QuoteUnlVsQuote_Indicator, elem.Volatility_Indicator, elem.RiskArrayIndex,
                    elem.MaturityYearMonth, elem.MaturityFactor,

                    elem.MarginAmount
                    )
                ).ToArray();

            var groupedElements =
                from elem in riskArrayNeutralPrices group elem by elem.Volatility_Indicator;

            foreach (var groupedElement in groupedElements)
            {
                TimsFactorCommunicationObject priceNeutral = groupedElement.FirstOrDefault(elem => elem.QuoteUnlVsQuote_Indicator == "N");

                TimsFactorCommunicationObject priceUp = groupedElement.FirstOrDefault(elem => elem.QuoteUnlVsQuote_Indicator == "U");

                TimsFactorCommunicationObject priceDown = groupedElement.FirstOrDefault(elem => elem.QuoteUnlVsQuote_Indicator == "D");

                if (priceNeutral != null && priceUp != null && priceDown != null)
                {
                    decimal amountNeutral = priceNeutral.MarginAmount.Amount.DecValue;

                    decimal amountUp = priceUp.MarginAmount.Amount.DecValue;

                    decimal amountDown = priceDown.MarginAmount.Amount.DecValue;

                    if (
                        // when neutral price for the current volatility is a credit...
                        amountNeutral < 0 &&
                        // when the premium is an equal or bigger credit ...
                        amountNeutral >= pPremiumClassMarginAmount &&
                        // when the down/up prices for the current volatility are bigger credits...
                        amountNeutral > amountUp && amountNeutral > amountDown)
                    {
                        // ...then we take the bigger one and we overwrite the value with the neutral amount

                        if (amountUp >= amountDown)
                        {
                            amountUp = amountNeutral;
                        }

                        if (amountDown >= amountUp)
                        {
                            amountDown = amountNeutral;
                        }
                    }

                    priceUp.MarginAmount.Amount.DecValue = amountUp;

                    priceDown.MarginAmount.Amount.DecValue = amountDown;

                }
            }

            return riskArrayNeutralPrices;
        }

        private TimsFactorCommunicationObject[] AdjustRiskArrayWithPremium(
            TimsFactorCommunicationObject[] pRiskArrayMax, decimal pPremiumClassMarginAmount)
        {
            TimsFactorCommunicationObject[] AdjustRiskArrayWithPremium = (
                from elem in pRiskArrayMax
                select
                    GetRiskArrayElement(
                    null, null, elem.QuoteUnlVsQuote_Indicator, elem.Volatility_Indicator, elem.RiskArrayIndex,
                    elem.MaturityYearMonth, elem.MaturityFactor,

                    new Money
                    (
                    // Adjusted price (additional - premium)
                        elem.MarginAmount.Amount.DecValue - pPremiumClassMarginAmount,
                    //
                        elem.MarginAmount.Currency
                    )

                    )
                ).ToArray();

            return AdjustRiskArrayWithPremium;
        }

        /// <summary>
        /// multiply the factors amount per the given offset  
        /// </summary>
        /// <param name="pFactors">source factors collection</param>
        /// <param name="pOffset">multiplier factor</param>
        /// <param name="pCheckNegativeAmount">when true the offset is performed just when the amount is negative</param>
        /// <returns>the factors with the adjusted amount</returns>
        private TimsFactorCommunicationObject[] OffsetRiskArray(
            TimsFactorCommunicationObject[] pFactors, decimal pOffset, bool pCheckNegativeAmount)
        {
            TimsFactorCommunicationObject[] res = pFactors;

            // FL PM 20130617 [18748]: Correction d’un bug, sur l’application de l’Offset quand celui-ci est égale à zéro. 
            // Dans Spheres il y avait un bug dans le scenario suivant :
            //
            //    Offset du Margin Group BUBO : 0.00
            //
            //    Margin Group 	Margin Class	   Additional Margin	  unadjusted Margin Requirement
            //    BUBO			FGBL			   47 835,56			  47 835,56
            //    BUBO			FGBM			      175,42			     175,42
            //    BUBO			FGBS			    - 120,00			   - 120,00
            //                                                    Total   47 890,98
            //
            // Description du bug : On est dans le cas ou l’Offset du Margin Group BUBO est 0.00.
            // Dans ce cas lorsque le montant d’Additional Margin est négatif celui-ci doit être considère comme égale à 0.
            // C'est à dire que dans le cas présent, le montant «unadjusted Margin Requirement» pour la Margin Class FGBS doit être égale à 0.00 et non à - 120,00.
            //
            // Pour plus d’info cf ticket 18748 (De plus le ticket N° 18757 a été initié, pour confirmer ce comportement auprès de la chambre de compensation EUREX).

            // Constitution de l'ensemble des montants auquels il faut appliquer un Offset
            //   - On applique un Offset si le montant est négatif. 
            var factorsToOffset = pFactors.Where(e => (!pCheckNegativeAmount) || (pCheckNegativeAmount && e.MarginAmount.Amount.DecValue < 0));

            // Constitution de l'ensemble des montants auquels ne pas appliquer d'Offset.
            var factorsNotToOffset = pFactors.Except(factorsToOffset);

            // Constitution du jeu de résultat constitué de la concaténation des montants auquels ont été appliqué un Offset
            // et des autres montants auquels n'ont pas été appliqué d'Offset.
            res = (factorsToOffset.Select(e => AdjustFactor(e, pOffset, true)).Concat(factorsNotToOffset)).ToArray();

            // Code source Avant Modif: 
            //res = pFactors.
            //    Select(elem =>
            //        AdjustFactor(
            //        elem,
            //        (!pCheckNegativeAmount) || (pCheckNegativeAmount && elem.MarginAmount.amount.DecValue < 0) ?
            //            pOffset
            //            :
            //            0)
            //    ).ToArray();

            return res;
        }

        private TimsFactorCommunicationObject AdjustFactor(TimsFactorCommunicationObject pFactor, decimal pOffset)
        {
            return (AdjustFactor(pFactor, pOffset, false));
        }
        /// <summary>
        /// Multiplication du montant du factor par un Offset donné
        /// </summary>
        /// <param name="pFactor">Factor dont multiplier le montant</param>
        /// <param name="pOffset">Offset (pourcentage) par lequel ajouster le MarginAmount de <paramref name="pFactor"/></param>
        /// <param name="pIsZeroAllowed">true pour considérer les offsets à zéro, false pour les ignorer</param>
        /// <returns>Nouveau Factor résultat de la multiplication par l'Offset</returns>
        // FL & PM 20130617 [18748] : Nouvelle signature de la méthode AdjustFactor : Ajout paramètre "pIsZeroAllowed"
        private TimsFactorCommunicationObject AdjustFactor(TimsFactorCommunicationObject pFactor, decimal pOffset, bool pIsZeroAllowed)
        {
            TimsFactorCommunicationObject resFactor = pFactor;

            // FL & PM 20130617 [18748] : Application de l’offset quand celui est égale à zéro : ((pOffset == 0) && pIsZeroAllowed)
            // Code source Avant Modif: 
            //  if ((pOffset > 0) 
            if ((pOffset > 0) || ((pOffset == 0) && pIsZeroAllowed))
            {
                resFactor = GetRiskArrayElement(
                        pFactor.ShortAdj, pFactor.Quote, pFactor.QuoteUnlVsQuote_Indicator, pFactor.Volatility_Indicator,
                        pFactor.RiskArrayIndex, pFactor.MaturityYearMonth, pFactor.MaturityFactor,

                        new Money(
                    //
                            this.AdjustAmount(pFactor.MarginAmount.Amount.DecValue, pOffset, false),
                    //
                            pFactor.MarginAmount.Currency)
                        );
            }
            return resFactor;
        }

        //PM 20150122 [20712][20724] Réécriture de la méthode GetGroupAdditionalMargin avec prise en compte des ExchangeRates
        private IEnumerable<TimsDecomposableParameterCommunicationObject> GetGroupAdditionalMargin(
            IEnumerable<TimsDecomposableParameterCommunicationObject> pDecomposableParameters,
            IEnumerable<TimsEurexExchRateCommunicationObject> pExchangeRates)
        {
            // Découpage de la requête repris de l'ancienne méthode : à priori cimul des matrices par devise ?

            // Prendre l'ensemble des matrices de type RiskWithNeutralPricesAndOffset
            var notNullAdjustedFactor =
                from parameter in pDecomposableParameters
                where ((parameter != null) && (parameter.Factors != null))
                from factor in parameter.Factors
                where ((factor != null) && (factor.Identifier == cm_MaxAdjusted))
                select factor;

            // Regrouper les matrices par devise
            var sidepointByCurrency =
                from factor in notNullAdjustedFactor
                from sidepoint in factor.RiskArray
                group sidepoint by sidepoint.MarginAmount.GetCurrency.Value into groupedSidePointByCurrency
                select groupedSidePointByCurrency;

            // A priori cumul des matrices par devise ? (issu de la méthode d'origine)
            var timsFactorByCurrency =
                from groupedSidePointByCurrency in sidepointByCurrency
                select new TimsFactorCommunicationObject
                {
                    Identifier = cm_MaxAdjusted,

                    // List of the adjusted for the current currency (sidepoint.MarginAmount.Currency.Value)
                    RiskArray = (
                        from sidepoint in groupedSidePointByCurrency
                        group sidepoint by
                        new
                        {
                            sidepoint.QuoteUnlVsQuote_Indicator,
                            sidepoint.Volatility_Indicator,

                            sidepoint.MaturityYearMonth,
                            sidepoint.MaturityFactor,

                            Currency = sidepoint.MarginAmount.GetCurrency.Value,
                        }
                            into groupedSidePointByQuoteVolatilityIndicator
                            select GetRiskArrayElement(
                                null,
                                null,
                                groupedSidePointByQuoteVolatilityIndicator.Key.QuoteUnlVsQuote_Indicator,
                                groupedSidePointByQuoteVolatilityIndicator.Key.Volatility_Indicator,
                                null,
                                groupedSidePointByQuoteVolatilityIndicator.Key.MaturityYearMonth,
                                groupedSidePointByQuoteVolatilityIndicator.Key.MaturityFactor,
                                SumAmounts(from sidePoint in groupedSidePointByQuoteVolatilityIndicator
                                           select (Money)sidePoint.MarginAmount, null
                                          ).First()
                                )
                    ).ToArray(),
                };

            var timsFactor = timsFactorByCurrency;
            if (timsFactorByCurrency.Count() > 1)
            {
                // Convertion des valeurs des matrices afin de pouvoir les sommer et les comparer en devise m_CssCurrency
                timsFactor =
                   from factor in timsFactorByCurrency
                   from sidepointCur in factor.RiskArray
                   group sidepointCur by this.m_CssCurrency into allSidePoint
                   select new TimsFactorCommunicationObject
                   {
                       Identifier = cm_MaxAdjusted,

                       // List of the adjusted for the current currency (sidepoint.MarginAmount.Currency.Value)
                       RiskArray = (
                           from sidepoint in allSidePoint
                           group sidepoint by
                           new
                           {
                               sidepoint.QuoteUnlVsQuote_Indicator,
                               sidepoint.Volatility_Indicator,

                               sidepoint.MaturityYearMonth,
                               sidepoint.MaturityFactor,

                               Currency = this.m_CssCurrency,
                           }
                               into groupedSidePointByQuoteVolatilityIndicator
                               select GetRiskArrayElement(
                                   null,
                                   null,
                                   groupedSidePointByQuoteVolatilityIndicator.Key.QuoteUnlVsQuote_Indicator,
                                   groupedSidePointByQuoteVolatilityIndicator.Key.Volatility_Indicator,
                                   null,
                                   groupedSidePointByQuoteVolatilityIndicator.Key.MaturityYearMonth,
                                   groupedSidePointByQuoteVolatilityIndicator.Key.MaturityFactor,
                                   SumAmounts(from sidePoint in groupedSidePointByQuoteVolatilityIndicator
                                              select (Money)ConvertAmount(sidePoint.MarginAmount, pExchangeRates), null
                                             ).First()
                                   )
                       ).ToArray(),
                   };
            }

            var maxFactor =
                from factor in timsFactor
                select
                new
                {
                    // max list
                    MaxAdjustedList = factor,
                    // max
                    MaxAdjusted = (from sidepoint in factor.RiskArray select sidepoint).Aggregate(
                        (curr, next) => next != null && curr.MarginAmount.Amount.DecValue > next.MarginAmount.Amount.DecValue ?
                            curr
                            :
                            next),
                };

            // Recalcul du Group.Additional par devise en fonction du scénario max en contrevaleur retenu
            var maxFactorByCur =
                from factorByCur in timsFactorByCurrency
                select
                new
                {
                    // max list
                    MaxAdjustedList = factorByCur,
                    // max
                    MaxAdjusted = (from sidepoint in factorByCur.RiskArray
                                   join factor in maxFactor on sidepoint.Identifier equals factor.MaxAdjusted.Identifier
                                   select sidepoint).FirstOrDefault(),
                };

            IEnumerable<TimsDecomposableParameterCommunicationObject> groupAddMargin =
                //from factor in maxFactor
                from factor in maxFactorByCur
                select new TimsDecomposableParameterCommunicationObject
                {
                    Factors = new TimsFactorCommunicationObject[] 
                            {
                                factor.MaxAdjustedList, 
                                factor.MaxAdjusted
                            },
                    MarginAmount = factor.MaxAdjusted.MarginAmount,
                };

            return groupAddMargin;
        }

        private IEnumerable<TimsDecomposableParameterCommunicationObject> GetGroupAdditionalMargin(
            IEnumerable<TimsDecomposableParameterCommunicationObject> pDecomposableParameters)
        {
            var timsFactor =
            from parameter in pDecomposableParameters
            where ((parameter != null)
            && (parameter.Factors != null))

            from factor in parameter.Factors
            where ((factor != null)
            && (factor.Identifier == cm_MaxAdjusted))

            from sidepoint in factor.RiskArray

            group sidepoint by sidepoint.MarginAmount.GetCurrency.Value

                into groupedSidePointByCurrency

                select new TimsFactorCommunicationObject
                {
                    Identifier = cm_MaxAdjusted,

                    // list of the adjusted max for the current currency (sidepoint.MarginAmount.Currency.Value)
                    RiskArray = (
                    from sidepoint in groupedSidePointByCurrency
                    group sidepoint by
                    new
                    {
                        sidepoint.QuoteUnlVsQuote_Indicator,
                        sidepoint.Volatility_Indicator,

                        sidepoint.MaturityYearMonth,
                        sidepoint.MaturityFactor,

                        Currency = sidepoint.MarginAmount.GetCurrency.Value,
                    }
                        into groupedSidePointByQuoteVolatilityIndicator
                        select GetRiskArrayElement(
                            null,
                            null,
                            groupedSidePointByQuoteVolatilityIndicator.Key.QuoteUnlVsQuote_Indicator,
                            groupedSidePointByQuoteVolatilityIndicator.Key.Volatility_Indicator,
                            null,
                            groupedSidePointByQuoteVolatilityIndicator.Key.MaturityYearMonth,
                            groupedSidePointByQuoteVolatilityIndicator.Key.MaturityFactor,
                            SumAmounts(
                                from sidePoint in groupedSidePointByQuoteVolatilityIndicator
                                select (Money)sidePoint.MarginAmount, null).First())
                    ).ToArray(),
                };

            var maxFactor =
                from factor in timsFactor
                select
                new
                {
                    // max list
                    MaxAdjustedList = factor,
                    // max
                    MaxAdjusted = (from sidepoint in factor.RiskArray select sidepoint).Aggregate(
                        (curr, next) => next != null && curr.MarginAmount.Amount.DecValue > next.MarginAmount.Amount.DecValue ?
                            curr
                            :
                            next),
                };

            IEnumerable<TimsDecomposableParameterCommunicationObject> groupAddMargin =
                from factor in maxFactor
                select new TimsDecomposableParameterCommunicationObject
                {
                    Factors = new TimsFactorCommunicationObject[] 
                            {
                                factor.MaxAdjustedList, 
                                factor.MaxAdjusted
                            },
                    MarginAmount = factor.MaxAdjusted.MarginAmount,
                };

            return groupAddMargin;
        }

        private IEnumerable<TimsEurexExchRateCommunicationObject> GetExchRate(IEnumerable<TimsDecomposableParameterCommunicationObject> pDecomposableParameters)
        {
            // 1.  build the additional communication obects collection to be converted using the loaded exchange rates
            //      .. cast to array to give writing permission
            IEnumerable<TimsDecomposableParameterCommunicationObject> additionals = (
                from parameter in GetGroupAdditionalMargin(pDecomposableParameters)
                where
                    parameter != null &&
                    parameter.MarginAmount.Currency != this.m_CssCurrency &&
                    !String.IsNullOrEmpty(this.m_CssCurrency)
                select parameter)
                    .ToArray();

            // 2. build the exchange rate communication objects
            //      .. cast to array to give writing permission
            IEnumerable<TimsEurexExchRateCommunicationObject> exchRates = (
                from parameter in additionals
                select new TimsEurexExchRateCommunicationObject
                {
                    CurrencyFrom = parameter.MarginAmount.Currency,

                    CurrencyTo = this.m_CssCurrency,
                }).ToArray();

            // 3. Verify the exchange rate
            foreach (TimsEurexExchRateCommunicationObject fxRateCommObj in exchRates)
            {

                FxRateTimsEurex fxRateCredit =
                    m_ExchangeRates.Where(fxRate => fxRate.CurrencyFrom == fxRateCommObj.CurrencyFrom
                        && fxRate.CurrencyTo == fxRateCommObj.CurrencyTo
                        && fxRate.RateSide == ExchangeRateSide.Credit)
                    .FirstOrDefault();

                FxRateTimsEurex fxRateDebit =
                    m_ExchangeRates.Where(fxRate => fxRate.CurrencyFrom == fxRateCommObj.CurrencyFrom
                        && fxRate.CurrencyTo == fxRateCommObj.CurrencyTo
                        && fxRate.RateSide == ExchangeRateSide.Debit)
                    .FirstOrDefault();

                // 3.1. missing fx rate case
                if (fxRateCredit == null || fxRateDebit == null)
                {
                    string[] datas = new string[5];

                    fxRateCommObj.Missing = true;
                    fxRateCommObj.ErrorType = TimsEurexExchRateCommunicationObject.ErrorTypeList.Default;

                    datas[0] += " [" + Cst.UnderlyingAsset.FxRateAsset.ToString() + "]";
                    datas[1] = (null == fxRateCredit ? QuotationSideEnum.Bid.ToString() + " " : "") + (null == fxRateDebit ? QuotationSideEnum.Ask.ToString() : "");
                    datas[2] = "Close";
                    datas[3] = DtFunc.DateTimeToStringISO(this.DtBusiness);
                    datas[4] = "{null} - {null}";
                    
                    //fxRateCommObj.SystemMsgInfo = new SystemMSGInfo("SYS-00103", new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.QUOTENOTFOUND), datas);
                    fxRateCommObj.SystemMsgInfo = new SystemMSGInfo(SysCodeEnum.SYS, 103, new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.QUOTENOTFOUND), datas);

                    // next element...
                    break;
                }
                else if ((false == fxRateCredit.IsEnabled || false == fxRateDebit.IsEnabled))
                {
                    string[] datas = new string[5];

                    fxRateCommObj.Missing = true;
                    fxRateCommObj.ErrorType = TimsEurexExchRateCommunicationObject.ErrorTypeList.Default;

                    datas[0] += " [" + Cst.UnderlyingAsset.FxRateAsset.ToString() + "]";
                    datas[1] = (false == fxRateCredit.IsEnabled ? QuotationSideEnum.Bid.ToString() + " " : "") + (false == fxRateDebit.IsEnabled ? QuotationSideEnum.Ask.ToString() : "");
                    datas[2] = "Close";
                    datas[3] = DtFunc.DateTimeToStringISO(this.DtBusiness);
                    datas[4] = (StrFunc.IsFilled(fxRateCredit.IdMarketEnv) ? fxRateCredit.IdMarketEnv.ToString() : "{null}") + " - " +
                               (StrFunc.IsFilled(fxRateCredit.IdValScenario) ? fxRateCredit.IdValScenario.ToString() : "{null}");
                    
                    //fxRateCommObj.SystemMsgInfo = new SystemMSGInfo("SYS-00104", new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.QUOTEDISABLED), datas);
                    fxRateCommObj.SystemMsgInfo = new SystemMSGInfo(SysCodeEnum.SYS, 104, new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.QUOTEDISABLED), datas);
                }

                // 3.2. related fx asset disabled case
                if (!DataContractHelper.GetDataContractElementEnabled(fxRateCredit, this.DtBusiness) ||
                    !DataContractHelper.GetDataContractElementEnabled(fxRateDebit, this.DtBusiness))
                {
                    fxRateCommObj.Missing = true;
                    fxRateCommObj.ErrorType = TimsEurexExchRateCommunicationObject.ErrorTypeList.FxAssetDisabled;

                    // next element...
                    break;
                }

                fxRateCommObj.RateCredit = fxRateCredit.RateValue;
                fxRateCommObj.RateDebit = fxRateDebit.RateValue;
            }

            return exchRates;
        }

        private static void SetExchRateMethodComObj(
            TimsEurexMarginCalculationMethodCommunicationObject pMethodComObj,
            IEnumerable<TimsEurexExchRateCommunicationObject> pNewRatesExchange)
        {
            if (pMethodComObj.ExchRates == null)
            {
                pMethodComObj.ExchRates = pNewRatesExchange;
            }
            else
            {

                IEnumerable<TimsEurexExchRateCommunicationObject> temp = pMethodComObj.ExchRates.Union(pNewRatesExchange);

                pMethodComObj.ExchRates =
                    from fxRate in temp
                    group fxRate by new
                    {
                        fxRate.CurrencyTo,
                        fxRate.CurrencyFrom,
                        fxRate.RateDebit,
                        fxRate.RateCredit,
                        fxRate.Missing,
                        fxRate.ErrorCode,
                    } into groupedFxRate
                    let first = groupedFxRate.First()
                    select first;
            }
        }

        /// <summary>
        /// Calculate the cross currency margin to evaluate the final deposit for the current actor/book pair
        /// </summary>
        /// <param name="pExchRates">All the valid echange rates used to evaluate the deposits</param>
        /// <param name="pMethodAmounts">multi currencies amounts for the actor/book pair, grouped by currency</param>
        /// <returns></returns>
        private IEnumerable<TimsDecomposableParameterCommunicationObject> GetMethodCrossMargin(
            IEnumerable<TimsEurexExchRateCommunicationObject> pExchRates, IEnumerable<Money> pMethodAmounts)
        {
            List<TimsDecomposableParameterCommunicationObject> crossMargin = null;

            if (pExchRates != null)
            {
                // 1. build the converted amounts list

                // FL&PM 20130320 : Correction d’un bug évitant une erreur de type
                // "System.DivideByZeroException Attempted to divide by zero."
                // quand la classe exchRate ne contient pas les cours des taux de change
                // par rajout du test (exchRate.Missing == false)
                var convertedAmounts =
                    from amount in pMethodAmounts
                    where !String.IsNullOrEmpty(this.m_CssCurrency) &&
                          (amount.Currency != this.m_CssCurrency)
                    join exchRate in pExchRates on amount.Currency equals exchRate.CurrencyFrom
                    where (exchRate.CurrencyTo == this.m_CssCurrency)
                    && (exchRate.Missing == false)
                    select new
                    {
                        Amount = amount,

                        // convert the amount using the exchange rate
                        ConvertedAmount = new Money(this.AdjustAmount(
                            amount.Amount.DecValue,
                            1 / (amount.Amount.DecValue > 0 ? exchRate.RateDebit : exchRate.RateCredit),
                            false), this.m_CssCurrency),

                        CurrencyFrom = amount.Currency,

                        ExchRate = 1 / (amount.Amount.DecValue > 0 ? exchRate.RateDebit : exchRate.RateCredit),
                    };

                // 2. Add the not converted amounts to the converted amounts list, in order to identify the whol cross currency amounts list
                //  ... Be aware that the not converted amounts list should count just ONE amount, because 
                //      the amounts list as input argument is grouped by currency
                var amountsToBeCrossed =
                    convertedAmounts.Union(pMethodAmounts.Where(amount => amount.Currency == this.m_CssCurrency).
                        Select(elem => new
                        {
                            Amount = elem,
                            ConvertedAmount = elem,
                            CurrencyFrom = elem.Currency,
                            ExchRate = (decimal)1.0,
                        }));


                // the cross margin object it is in place just in case we have more than one currency 
                if (amountsToBeCrossed.Count() > 1)
                {
                    // 3.1 Get all the surplus (margin credit) amounts
                    IEnumerable<Money> amountsSurplus = amountsToBeCrossed.Where(
                        elem => elem.ConvertedAmount.Amount.DecValue <= 0).Select(elem => elem.ConvertedAmount);

                    decimal sumSurPlus = amountsSurplus.Sum(surPlus => surPlus.Amount.DecValue);

                    // 3.2 Get all the shortfall (real margin) amounts
                    IEnumerable<Money> amountsShortFall = amountsToBeCrossed.Where(
                        elem => elem.ConvertedAmount.Amount.DecValue > 0).Select(elem => elem.ConvertedAmount);

                    decimal sumShortFall = amountsShortFall.Sum(shortFall => shortFall.Amount.DecValue);

                    if ((sumSurPlus < 0) && (sumShortFall > 0))
                    {
                        crossMargin = new List<TimsDecomposableParameterCommunicationObject>();

                        sumSurPlus = System.Math.Abs(sumSurPlus);

                        var amountToCover = amountsToBeCrossed.Where(amount => (amount.ConvertedAmount.Amount.DecValue > 0)
                            && (amount.CurrencyFrom != this.m_CssCurrency)).OrderBy(amount => amount.CurrencyFrom).
                            Union(amountsToBeCrossed.Where(amount => (amount.ConvertedAmount.Amount.DecValue > 0)
                            && (amount.CurrencyFrom == this.m_CssCurrency)));

                        foreach (var amount in amountToCover)
                        {
                            decimal amountCovered = System.Math.Min(sumSurPlus, amount.ConvertedAmount.Amount.DecValue);
                            decimal amountResult = (amount.ConvertedAmount.Amount.DecValue - amountCovered) * amount.ExchRate;
                            List<TimsFactorCommunicationObject> factorsList = new List<TimsFactorCommunicationObject>
                            {
                                new TimsFactorCommunicationObject
                                {
                                    Identifier = String.Format("Surplus-{0}", amount.ConvertedAmount.Currency),

                                    MarginAmount = new Money(-1 * amountCovered, amount.ConvertedAmount.Currency),
                                },
                                new TimsFactorCommunicationObject
                                {
                                    Identifier = String.Format("Shortfall-{0}", amount.CurrencyFrom),

                                    MarginAmount = amount.Amount,
                                }
                            };

                            TimsDecomposableParameterCommunicationObject crossed =
                                new TimsDecomposableParameterCommunicationObject
                                {
                                    Factors = factorsList,

                                    MarginAmount = new Money(amountResult, amount.CurrencyFrom),
                                };

                            crossMargin.Add(crossed);

                            sumSurPlus -= amountCovered;
                            if (sumSurPlus <= 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return crossMargin;
        }

        private IMoney[] AggregateMarginAmounts(IEnumerable<TimsDecomposableParameterCommunicationObject> pEvaluatedMargins)
        {
            IEnumerable<Money> amounts =
                from margin in pEvaluatedMargins
                where margin != null
                select (Money)margin.MarginAmount;

            List<Money> resAmounts = null;

            SumAmounts(amounts, ref resAmounts);

            return resAmounts.ToArray();
        }

        private decimal GetLiquidatingQuote(RiskMethodQtyType pQtyType, decimal pQuote, decimal pUnlQuote, decimal pStrikePrice)
        {
            decimal quote = pQuote;

            if (pQtyType == RiskMethodQtyType.ExeAssCall || pQtyType == RiskMethodQtyType.ExeAssPut)
            {
                // exercised/assigned position quote
                quote = GetInTheMoneyAmount(pQtyType, pUnlQuote, pStrikePrice);
            }
            return quote;
        }

        private decimal GetPremiumQuote(RiskMethodQtyType pQtyType, decimal pQuote, decimal pUnlQuote, decimal pStrikePrice)
        {
            return
                (pQtyType == RiskMethodQtyType.Put || pQtyType == RiskMethodQtyType.Call) ?
                // open position quote 
                        pQuote
                        :
                // exercised/assigned position quote
                        GetInTheMoneyAmount(pQtyType, pUnlQuote, pStrikePrice);
        }

        private IEnumerable<TimsDecomposableParameterCommunicationObject> AggregateMargins (IEnumerable<TimsDecomposableParameterCommunicationObject> pDecomposableParameters)
        {

            pDecomposableParameters = from parameter in pDecomposableParameters where parameter != null select parameter;

            List<Money> amounts = new List<Money>();
            SumAmounts(from factor in pDecomposableParameters select (Money)factor.MarginAmount, ref amounts);

            return
                from amount in amounts
                select new TimsDecomposableParameterCommunicationObject
                {
                    MarginAmount = amount,
                };
        }

        private TimsDecomposableParameterCommunicationObject AggregateMargin (IEnumerable<TimsDecomposableParameterCommunicationObject> pDecomposableParameters)
        {

            return AggregateMargins(pDecomposableParameters).FirstOrDefault();
        }

        /// <summary>
        /// Calcul de la contrevaleur d'un montant
        /// </summary>
        /// <param name="pAmount"></param>
        /// <param name="pExchangeRates"></param>
        /// <returns></returns>
        //PM 20150122 [20712][20724] Méthode ajoutée
        private IMoney ConvertAmount(IMoney pAmount, IEnumerable<TimsEurexExchRateCommunicationObject> pExchangeRates)
        {
            Money convertedAmount = (Money)pAmount;
            if (StrFunc.IsFilled(this.m_CssCurrency)
                && (pExchangeRates != default(IEnumerable<TimsEurexExchRateCommunicationObject>))
                && (pAmount != default)
                && StrFunc.IsFilled(pAmount.Currency))
            {
                TimsEurexExchRateCommunicationObject exchRate = pExchangeRates.FirstOrDefault(rate => (rate.CurrencyFrom == pAmount.Currency)
                    && (rate.CurrencyTo == this.m_CssCurrency)
                    && (rate.Missing == false));
                if (exchRate != default(TimsEurexExchRateCommunicationObject))
                {
                    convertedAmount = new Money(
                        this.AdjustAmount(pAmount.Amount.DecValue,
                        1 / (pAmount.Amount.DecValue > 0 ? exchRate.RateDebit : exchRate.RateCredit),
                        false), this.m_CssCurrency);
                }
            }
            return convertedAmount;
        }

        #region RBM for Prisma
        // PM 20151116 [21561] Ajout pour RBM dans Prisma
        /// <summary>
        /// Rend public l'appel au calcul des montants de déposit avec la méthode RBM.
        /// </summary>
        /// <param name="pActorId">L'acteur de la position à évaluer</param>
        /// <param name="pBookId">Le book de la position à évaluer</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="pRiskDataToEvaluate">La position pour laquelle calculer le déposit</param>
        /// <param name="opMethodComObj">Journal du calcul réalisé en sortie</param>
        /// <returns>Les montants de déposit correspondants à la position</returns>
        /// FI 20160613 [22256] Add parameter pDepositHierarchyClass
        // PM 20181002 [XXXXX] Désactivation de RBM dans Prisma
        //public List<Money> EvaluateRiskElementRbmForPrisma(
        //    int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
        //    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
        //    out IMarginCalculationMethodCommunicationObject opMethodComObj)
        //{
        //    // PM 20170313 [22833] Changement de type pour le paramètre pPositionsToEvaluate (=>  RiskData pRiskDataToEvaluate)
        //    //List<Money> riskAmounts = EvaluateRiskElementSpecific(pActorId, pBookId, pDepositHierarchyClass, pPositionsToEvaluate, out  opMethodComObj);
        //    List<Money> riskAmounts = default;

        //    if ((pPositionsToEvaluate != default) && (pPositionsToEvaluate.Count() > 0))
        //    {
        //        Dictionary<PosRiskMarginKey, RiskMarginPosition> positionDic = pPositionsToEvaluate.ToDictionary(k => k.First, k => k.Second);
        //        RiskDataPosition rbmRiskDataPosition = new RiskDataPosition(positionDic);
        //        RiskData rbmRiskData = new RiskData(rbmRiskDataPosition, default(RiskDataTradeValue));

        //        riskAmounts = EvaluateRiskElementSpecific(pActorId, pBookId, pDepositHierarchyClass, rbmRiskData, out opMethodComObj);

        //        if ((opMethodComObj == default)
        //            || (opMethodComObj.Parameters == default(IRiskParameterCommunicationObject[]))
        //            || (opMethodComObj.Parameters.Count() == 0))
        //        {
        //            riskAmounts = default;
        //        }
        //    }
        //    else
        //    {
        //        opMethodComObj = default;
        //    }
        //    return riskAmounts;
        //}
        #endregion RBM for Prisma
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using EFS.ACommon;
using EFS.Common;
using EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin;
using EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin.v1_0_39;
using EfsML.Enum;
using FixML.Enum;
using FixML.v50SP1;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Classe de méthodes utiles pour SPAN 2
    /// </summary>
    public static class RiskMethodSpan2Helper
    {
        #region Members
        // Limites de charge par point d'accès
        private readonly static int m_GetMarginDelay = 5000; // 5 secondes
        private readonly static int m_GetPortfolioDelay = 1000; // 1 seconde
        private readonly static int m_GetTransactionDelay = 1000; // 1 seconde
        private readonly static int m_GetOtherDelay = 1000; // 1 seconde
        private readonly static int m_PostMarginDelay = 2000; // 2 secondes
        private readonly static int m_PostPortfolioDelay = 2000; // 2 secondes
        private readonly static int m_PostTransactionDelay = 2000; // 2 secondes

        // DateTime de dernière émission de requêtes REST
        private static DateTime m_LastGetMargin = DateTime.MinValue;
        private static DateTime m_LastGetTransaction = DateTime.MinValue;
        private static DateTime m_LastGetPortfolio = DateTime.MinValue;
        private static DateTime m_LastGetOther = DateTime.MinValue;
        private static DateTime m_LastPostMargin = DateTime.MinValue;
        private static DateTime m_LastPostPortfolio = DateTime.MinValue;
        private static DateTime m_LastPostTransaction = DateTime.MinValue;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Valeur du delay d'attente entre 2 appels Margin (en milisecondes)
        /// </summary>
        public static int GetMarginDelay
        {
            get { return m_GetMarginDelay; }
        }
        #endregion Accessors

        #region Methods
        /// <summary>
        /// Attente éventuelle avant de pouvoir faire une requête GET sur le point d'accès Margin
        /// </summary>
        public static void WaitGetMargin()
        {
            m_LastGetMargin = Wait(m_LastGetMargin, m_GetMarginDelay);
        }

        /// <summary>
        /// Attente éventuelle avant de pouvoir faire une requête GET sur le point d'accès Portfolio
        /// </summary>
        public static void WaitGetPortfolio()
        {
            m_LastGetPortfolio = Wait(m_LastGetPortfolio, m_GetPortfolioDelay);
        }

        /// <summary>
        /// Attente éventuelle avant de pouvoir faire une requête GET sur le point d'accès Transaction
        /// </summary>
        public static void WaitGetTransaction()
        {
            m_LastGetTransaction = Wait(m_LastGetTransaction, m_GetTransactionDelay);
        }

        /// <summary>
        /// Attente éventuelle avant de pouvoir faire une requête GET sur un point d'accès autre que Margin, Portfolio et Transaction
        /// </summary>
        public static void WaitGetOther()
        {
            m_LastGetOther = Wait(m_LastGetOther, m_GetOtherDelay);
        }

        /// <summary>
        /// Attente éventuelle avant de pouvoir faire une requête PUT/POST/DELETE sur le point d'accès Margin
        /// </summary>
        public static void WaitPostMargin()
        {
            m_LastPostMargin = Wait(m_LastPostMargin, m_PostMarginDelay);
        }

        /// <summary>
        /// Attente éventuelle avant de pouvoir faire une requête PUT/POST/DELETE sur le point d'accès Portfolio
        /// </summary>
        public static void WaitPostPortfolio()
        {
            m_LastPostPortfolio = Wait(m_LastPostPortfolio, m_PostPortfolioDelay);
        }

        /// <summary>
        /// Attente éventuelle avant de pouvoir faire une requête PUT/POST/DELETE sur le point d'accès Transaction
        /// </summary>
        public static void WaitPostTransaction()
        {
            m_LastPostTransaction = Wait(m_LastPostTransaction, m_PostTransactionDelay);
        }

        /// <summary>
        /// Attente éventuelle avant de pouvoir faire une requête sur un point d'accès.
        /// </summary>
        /// <param name="pLastAction">Horaire de la dernière requête</param>
        /// <param name="pDelay">Delai entre 2 requêtes</param>
        /// <returns></returns>
        private static DateTime Wait(DateTime pLastAction, int pDelay)
        {
            TimeSpan diffTime = DateTime.Now - pLastAction;
            if (diffTime.TotalMilliseconds < pDelay)
            {
                int waitTime = (int)Math.Min(diffTime.TotalMilliseconds, pDelay);
                Thread.Sleep(waitTime);
            }
            return DateTime.Now;
        }

        /// <summary>
        /// Obtient le ProductType Span en fonction de la Category et de l'Underlying Asset Category du derivative contract
        /// </summary>
        /// <param name="pCategory"></param>
        /// <param name="pUnderlyingAssetCategory"></param>
        /// <returns></returns>
        public static ProductType GetProductType(Nullable<CfiCodeCategoryEnum> pCategory, Cst.UnderlyingAsset pUnderlyingAssetCategory)
        {
            ProductType productType = ProductType.FUT;
            if (pCategory.HasValue && pCategory.Value == CfiCodeCategoryEnum.Option)
            {
                switch (pUnderlyingAssetCategory)
                {
                    case Cst.UnderlyingAsset.EquityAsset:
                        productType = ProductType.OOP;
                        break;
                    case Cst.UnderlyingAsset.Commodity:
                        productType = ProductType.OOC;
                        break;
                    default:
                        productType = ProductType.OOF;
                        break;
                }
            }
            return productType;
        }

        /// <summary>
        /// Convertion de la position Spheres en position Span2
        /// </summary>
        /// <param name="pCssAcronym">Acronym de la Clearing House</param>
        /// <param name="pAssetExpandedParameters">Informations sur les assets</param>
        /// <param name="pPosition">Position à convertir</param>
        /// <param name="pIsOmnibus">Indique s'il s'agit d'un compte omnibus</param>
        /// <returns></returns>
        // EG 20230811 [26454] MarketExchangeAcronym insteadOf MarketExchangeAcronym
        public static IEnumerable<RiskPosition> BuildRiskPosition(string pCssAcronym, IEnumerable<AssetExpandedParameter> pAssetExpandedParameters, IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> pPosition, bool pIsOmnibus)
        {
            IEnumerable<RiskPosition> riskPos;
            if (pIsOmnibus)
            {
                // Position brute pour les comptes omnibus
                riskPos =
                    from positionAsset in (pPosition.GroupBy(position => position.Key.idAsset))
                    let quantityLong = positionAsset.Where(pos => pos.Key.FixSide == SideEnum.Buy).Select(pos => pos.Value.Quantity).Sum()
                    let quantityShort = positionAsset.Where(pos => pos.Key.FixSide == SideEnum.Sell).Select(pos => pos.Value.Quantity).Sum()
                    join assetRef in pAssetExpandedParameters on positionAsset.Key equals assetRef.AssetId
                    select
                    new RiskPosition
                    {
                        Instrument = new Instrument
                        {
                            ClearingOrganizationId = pCssAcronym,
                            // EG 20230811 [26454] MarketExchangeAcronym insteadOf MarketExchangeAcronym
                            ExchangeId = assetRef.MarketExchangeAcronym,
                            ProductCode = assetRef.ContractSymbol,
                            PeriodCode = assetRef.MaturityYearMonth,
                            ProductType = GetProductType(assetRef.CategoryEnum, assetRef.UnderlyingAssetCategoryEnum),
                            PutCallInd = (assetRef.PutOrCall.HasValue ? (assetRef.PutOrCall == PutOrCallEnum.Call ? PutCallIndicator.C : PutCallIndicator.P) : default(PutCallIndicator?)),
                            // PM 20230829 [26470][WI695] Prise en compte des strikes en cents (InstrumentDen >= 10000)
                            Strike = (assetRef.PutOrCall.HasValue ? (assetRef.InstrumentDen >= 10000 ? assetRef.StrikePrice / (assetRef.InstrumentDen / 100) : assetRef.StrikePrice) : default)
                        },
                        NakedLongQty = quantityLong,
                        NakedShortQty = quantityShort,
                    };
            }
            else
            {
                // Position nette pour les comptes non omnibus
                riskPos =
                    from position in pPosition
                    join assetRef in pAssetExpandedParameters on position.Key.idAsset equals assetRef.AssetId
                    select
                    new RiskPosition
                    {
                        Instrument = new Instrument
                        {
                            ClearingOrganizationId = pCssAcronym,
                            // EG 20230811 [26454] MarketExchangeAcronym insteadOf MarketExchangeAcronym
                            ExchangeId = assetRef.MarketExchangeAcronym,
                            ProductCode = assetRef.ContractSymbol,
                            PeriodCode = assetRef.MaturityYearMonth,
                            ProductType = GetProductType(assetRef.CategoryEnum, assetRef.UnderlyingAssetCategoryEnum),
                            PutCallInd = (assetRef.PutOrCall.HasValue ? (assetRef.PutOrCall == PutOrCallEnum.Call ? PutCallIndicator.C : PutCallIndicator.P) : default(PutCallIndicator?)),
                            // PM 20230829 [26470][WI695] Prise en compte des strikes en cents (InstrumentDen >= 10000)
                            Strike = (assetRef.PutOrCall.HasValue ? (assetRef.InstrumentDen >= 10000 ? assetRef.StrikePrice / (assetRef.InstrumentDen / 100) : assetRef.StrikePrice) : default)
                        },
                        NetQty = ((position.Key.FixSide == SideEnum.Sell) ? (-1 * position.Value.Quantity) : position.Value.Quantity),
                    };
            }

            return riskPos;
        }
        #endregion Methods
    }

    /// <summary>
    /// Gestion des éléments Instrument et Position d'une transaction au format CSV
    /// </summary>
    public class CSVInstrumentPosition
    {
        #region Members
        /// <summary>
        /// Données du header des éléments Instrument et Position d'une transaction au format CSV
        /// </summary>
        public static string InstrumentHeader = "netQty,nakedLongQty,nakedShortQty,clearingOrganizationId,exchangeId,productCode,productType,periodCode,putCallInd,strike,underlyingPeriodCode";

        /// <summary>
        /// User defined clearing organization
        /// Required string
        /// CSS Acronym
        /// </summary>
        public string ClearingOrganizationId;
        /// <summary>
        /// Name of Exchange in which the contracts are listed
        /// Required string : ("CBT")
        /// For CME Group Exchanges, acceptable values are CME, CBT, NYMEX, COMEX, NYM, CMX
        /// Market Exchange Acronym
        /// </summary>
        public string ExchangeId;
        /// <summary>
        /// CME Clearing House Product Code
        /// Required string : ("17")
        /// Contract Symbol
        /// </summary>
        public string ProductCode;
        /// <summary>
        /// Name of the product type
        /// Required string : ("FUT")
        /// Acceptable values are FUT, OOF, OOP, OOC, FWD
        /// Catégorie de contrat
        /// </summary>
        public string ProductType;
        /// <summary>
        /// Maturity Date of Product
        /// Required string : ("201809" "20180920" "201809W1")
        /// Example values are: YYYYMM, YYYYMMDD, YYYYMMW1, YYYYMMW2, YYYYMMW3, YYYYMMW4, YYYYMMW5
        /// Code de l'échéance
        /// </summary>
        public string PeriodCode;
        /// <summary>
        /// Whether the option trade is a PUT or CALL
        /// Conditional string : ("C")
        /// Required if ProductType = OOF, OOC, OOP
        /// Acceptable values are P or C
        /// Put (P) ou Call (C)
        /// </summary>
        public string PutCallInd;
        /// <summary>
        /// Strike Price for options
        /// Conditional decimal : ("80.00")
        /// Required if ProductType = OOF, OOC, OOP
        /// </summary>
        public decimal? Strike;
        /// <summary>
        /// Maturity Date of underlying product
        /// Optional string : ("201809" "20180920" "201809W1")
        /// Used if ProductType = OOF, OOC, OOP
        /// If an option’s values have multiple similarities(i.e.product code and period code), than this field is required
        /// Code de l'échéance du sous-jacent (Non utilisé)
        /// </summary>
        public string UnderlyningPeriodCode;
        /// <summary>
        /// Net quantity is expressed as either positive (long) and negative (short)
        /// Conditional decimal (can be negative) : ("1.0")
        /// Required if portfolio AccountType is not Omnibus
        /// Acceptable values can only be integers
        /// Quantité nette (compte non omnibus)
        /// </summary>
        public decimal? NetQty;
        /// <summary>
        /// Buy quantity for naked margin treatment (see omnibus/PID notes)
        /// Conditional decimal (cannot be negative) : ("1.0")
        /// Only allowable for Omnibus account types, but never required
        /// Acceptable values can only be integers
        /// Quantité à l'achat (compte omnibus)
        /// </summary>
        public decimal? NakedLongQty;
        /// <summary>
        /// Sell Quantity for naked margin treatment (See omnibus/PID notes)
        /// Conditional decimal (cannot be negative) : ("1.0")
        /// Only allowable for Omnibus account types, but never required
        /// Acceptable values can only be integers
        /// Quantité à la vente (compte omnibus)
        /// </summary>
        public decimal? NakedShortQty;
        #endregion Members

        #region Methods
        /// <summary>
        /// Retourne les éléments Instrument et Position d'une transaction au format CSV
        /// </summary>
        /// <returns></returns>
        public string GetCSVString()
        {
            string csvInstrument = $"{NetQty},{NakedShortQty},{NakedLongQty},{ClearingOrganizationId},{ExchangeId},{ProductCode},{ProductType},{PeriodCode},{PutCallInd},{Strike},{UnderlyningPeriodCode}";
            return csvInstrument;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de génération de la position au format CSV
    /// </summary>
    public static class CSVTransaction
    {
        #region Members
        /// <summary>
        /// Partie de l'entête commune à toutes les transactions
        /// </summary>
        public static string CommonHeader = "requestId,version,sentTime,businessDate,cycleCode,runNumber,id,currency,customerAccountType,omnibusInd,parentPortfolioId,memo,firmId,accountId,accountName,originType,segregationType,customerAccountType,";
        #endregion Members

        #region Methods
        /// <summary>
        /// Construction de la données CSV contenant la position et autres informations pour le calcul
        /// </summary>
        /// <param name="pMarginData">Données nécessaire au calcul</param>
        /// <returns></returns>
        public static List<string> CreateAllCSVTransactions(SPAN2MarginData pMarginData)
        {
            // Ensembles des lignes CSV en sortie: entête + transactions
            List<string> transactions = new List<string>();

            if (pMarginData != default(SPAN2MarginData))
            {
                #region Déclaration des données communes
                // User generated Request ID for margin request
                // Optional string : "INPUT_abc123456789"
                string requestId;
                // Version of risk API
                // Optional decimal : "1.0"
                // Users should specify the version of the Risk API they are using
                decimal version;
                // User generated system create time for message
                // Optional dateTime : "2018-03-01T17:43:09.422Z"
                DateTime sentTime;
                // Business date of margin run
                // Required date : "2018-02-28"
                // Date format expressed as: YYYY-MM-DD
                DateTime businessDate;
                // Defined description to distinguish between different point in time
                // Optional string : "EOD"
                // Acceptable values are: AM, EARLY, ITD, or EOD (Note: Values AM, EARLY, and ITD are not yet supported)
                string cycleCode;
                // Run Number field will increment if used multiple times within a specific CycleCode
                // Optional integer : "1"
                int runNumber;
                /// Populated with the current timestamp when the market data file gets created
                /// Optional time : "17:43:09"
                /// Non utilisé
                DateTime time;
                /// User-defined ID for the portfolio, margin results at portfolio level will correspond to this id
                /// Conditional string : "PORTFOLIO_1"
                /// Required for omnibus portfolios. A unique key should be used to identify each portfolio in a request payload
                string id;
                /// User Defined Portfolio Currency
                /// Required string : "USD"
                string currency;
                /// Account Type considerations impacting the margin ratio
                /// Required string : "HEDGE"
                /// Users set the default account type for the portfolio through this attribute.
                /// Acceptable values are MEMBER, HEDGE, SPECULATOR, HEIGHTENED*, NON_HEIGHTENED*.
                /// Omnibus accounts can only be HEDGE, SPEC, HEIGHTENED*, or NON_HEIGHTENED*.
                /// *(Note: Values Heightened and Non_Heightened are not yet supported.The values SPECULATOR and HEDGE are implied as HEIGHTENED and NON_HEIGHTENED , respectively.)
                string customerAccountType;
                /// Omnibus Indicator
                /// Optional string : "NO"
                /// Acceptable values are YES or No; Defaults to NO
                /// YES: Customer Account is Omnibus, and would have children only if it’s Partially or Fully Disclosed
                /// NO: Customer Account is not Omnibus, and has no children
                string omnibusInd;
                /// ID of the parent portfolio
                /// Optional string
                /// Used for omnibus child portfolios
                /// This field is the linkage between parent and child omnibus relationship
                string parentPortfolioId;
                /// Free form field which can be used to pass through any information to the response message
                /// Optional string
                string memo;
                /// User defined Clearing firm alphanumeric Id
                /// Required string : "001"
                string firmId;
                /// User defined account alphanumeric ID
                /// Required string : "Account1"
                string accountId;
                // User defined name for account
                // Optional string : "John Doe"
                string accountName;
                // Used to designate the manner in which transactions, positions, and funds are segregated as required by regulators
                // Required string : "CUST"
                // Acceptable values are: HOUS, CUST, CUSTOMER, HOUSE
                // Users can only supply one of the listed values; otherwise error message
                string originType;
                // Fund segregation type
                // Optional string : "CSEG"
                // Acceptable values are CSEG, CNSEG, COTC, NSEG, SECURED
                string segregationType;
                // Position
                List<CSVInstrumentPosition> instrumentPositions;
                #endregion Déclaration des données communes

                #region Alimentation des données communes
                requestId = pMarginData.RequestId;
                version = 1;
                sentTime = DateTime.UtcNow;
                businessDate = pMarginData.DtBusiness;
                cycleCode = pMarginData.JsonCycleCode.ToString();
                runNumber = 1;
                id = pMarginData.PortfolioId;
                currency = pMarginData.Currency.ToString();
                customerAccountType = pMarginData.CustomerAccountType.ToString();
                omnibusInd = pMarginData.OmnibusInd.ToString();
                parentPortfolioId = string.Empty;
                memo = string.Empty;
                firmId = pMarginData.FirmId;
                accountId = pMarginData.AccountId;
                accountName = pMarginData.AccountId;
                originType = pMarginData.OriginType.ToString();
                segregationType = "CSEG";
                #endregion Alimentation des données communes

                // Alimenter la partie position des lignes de transaction
                instrumentPositions = BuildCSVPosition(pMarginData.CssAcronym, pMarginData.AssetExpandedParameters, pMarginData.MarginPosition, pMarginData.IsOmnibus);

                int instPosCount = instrumentPositions.Count;
                if (instPosCount > 0)
                {
                    string header = CommonHeader + CSVInstrumentPosition.InstrumentHeader;
                    string csvCommonData = $"{requestId},{version},{sentTime:u},{businessDate:yyyy-MM-dd},{cycleCode},{runNumber},{id},{currency},{customerAccountType},{omnibusInd},{parentPortfolioId},{memo},{firmId},{accountId},{accountName},{originType},{segregationType},{customerAccountType},";
                    // Ajout de l'entête
                    transactions.Add(header);
                    // Construction et ajout des données (en ajoutant la partie commune aux parties position des lignes de transaction)
                    transactions.AddRange(instrumentPositions.Select(i => csvCommonData + i.GetCSVString()));
                }
            }
            return transactions;
        }

        /// <summary>
        /// Convertion de la position Spheres en ensemble de CSVInstrumentPosition
        /// </summary>
        /// <param name="pCssAcronym">Acronym de la Clearing House</param>
        /// <param name="pAssetExpandedParameters">Informations sur les assets</param>
        /// <param name="pPosition">Position à convertir</param>
        /// <param name="pIsOmnibus">Indique s'il s'agit d'un compte omnibus</param>
        /// <returns></returns>
        private static List<CSVInstrumentPosition> BuildCSVPosition(string pCssAcronym, IEnumerable<AssetExpandedParameter> pAssetExpandedParameters, IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> pPosition, bool pIsOmnibus)
        {
            List<CSVInstrumentPosition> instPos;
            if ((pAssetExpandedParameters != default) && (pPosition != default(IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>>)))
            {
                // Prendre les paramètres des assets en y associant l'éventuel asset future sous-jacent
                var assetParams = from position in pPosition
                                   join assetRef in pAssetExpandedParameters on position.Key.idAsset equals assetRef.AssetId
                                   join assetUnl in pAssetExpandedParameters on assetRef.UnderlyningAssetId equals assetUnl.AssetId into assetFutUnl
                                   select new
                                   {
                                       Asset = assetRef,
                                       UnlFutAsset = assetFutUnl.FirstOrDefault()
                                   };

                if (pIsOmnibus)
                {
                    // Position brute pour les comptes omnibus
                    instPos = (
                        from positionAsset in (pPosition.GroupBy(position => position.Key.idAsset))
                        let quantityLong = positionAsset.Where(pos => pos.Key.FixSide == SideEnum.Buy).Select(pos => pos.Value.Quantity).Sum()
                        let quantityShort = positionAsset.Where(pos => pos.Key.FixSide == SideEnum.Sell).Select(pos => pos.Value.Quantity).Sum()
                        join assetRef in assetParams on positionAsset.Key equals assetRef.Asset.AssetId
                        select
                        new CSVInstrumentPosition
                        {
                            ClearingOrganizationId = pCssAcronym,
                            ExchangeId = assetRef.Asset.MarketExchangeAcronym,
                            ProductCode = assetRef.Asset.ContractSymbol,
                            PeriodCode = assetRef.Asset.MaturityYearMonth,
                            ProductType = RiskMethodSpan2Helper.GetProductType(assetRef.Asset.CategoryEnum, assetRef.Asset.UnderlyingAssetCategoryEnum).ToString(),
                            PutCallInd = (assetRef.Asset.PutOrCall.HasValue ? (assetRef.Asset.PutOrCall == PutOrCallEnum.Call ? "C" : "P") : ""),
                            Strike = (assetRef.Asset.PutOrCall.HasValue ? (assetRef.Asset.InstrumentDen >= 10000 ? assetRef.Asset.StrikePrice / (assetRef.Asset.InstrumentDen / 100) : assetRef.Asset.StrikePrice) : default),
                            NakedLongQty = quantityLong,
                            NakedShortQty = quantityShort,
                            NetQty = default,
                            UnderlyningPeriodCode = (assetRef.UnlFutAsset != default(AssetExpandedParameter) ? assetRef.UnlFutAsset.MaturityYearMonth : "")
                        }).ToList();
                }
                else
                {
                    // Position nette pour les comptes non omnibus
                    instPos = (
                        from position in pPosition
                        join assetRef in assetParams on position.Key.idAsset equals assetRef.Asset.AssetId
                        select
                        new CSVInstrumentPosition
                        {
                            ClearingOrganizationId = pCssAcronym,
                            ExchangeId = assetRef.Asset.MarketExchangeAcronym,
                            ProductCode = assetRef.Asset.ContractSymbol,
                            PeriodCode = assetRef.Asset.MaturityYearMonth,
                            ProductType = RiskMethodSpan2Helper.GetProductType(assetRef.Asset.CategoryEnum, assetRef.Asset.UnderlyingAssetCategoryEnum).ToString(),
                            PutCallInd = (assetRef.Asset.PutOrCall.HasValue ? (assetRef.Asset.PutOrCall == PutOrCallEnum.Call ? "C" : "P") : ""),
                            Strike = (assetRef.Asset.PutOrCall.HasValue ? (assetRef.Asset.InstrumentDen >= 10000 ? assetRef.Asset.StrikePrice / (assetRef.Asset.InstrumentDen / 100) : assetRef.Asset.StrikePrice) : default),
                            NetQty = ((position.Key.FixSide == SideEnum.Sell) ? (-1 * position.Value.Quantity) : position.Value.Quantity),
                            NakedLongQty = default,
                            NakedShortQty = default,
                            UnderlyningPeriodCode = (assetRef.UnlFutAsset != default(AssetExpandedParameter) ? assetRef.UnlFutAsset.MaturityYearMonth : "")
                        }).ToList();
                }
            }
            else
            {
                instPos = new List<CSVInstrumentPosition>();
            }
            return instPos;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de methodes pour test
    /// </summary>
    public static class RiskMethodSpan2TestTools
    {
        /// <summary>
        /// Construction d'un RiskPortfolioRequestMessage de test
        /// </summary>
        /// <param name="pMarginData"></param>
        /// <returns></returns>
        public static RiskPortfolioRequestMessage TestRiskPortfolio(SPAN2MarginData pMarginData)
        {
            RiskPortfolioRequestMessage portfolioRequestMessage = new RiskPortfolioRequestMessage();

            RequestHeader reqHeader = new RequestHeader
            {
                RequestId = pMarginData.RequestId,
                SentTime = DateTime.UtcNow,
                Version = "1.0"
            };

            portfolioRequestMessage.header = reqHeader;

            RiskPortfolioRequest portfolioReq = new RiskPortfolioRequest();

            portfolioRequestMessage.payload = portfolioReq;
            PointInTime pointInTime = new PointInTime
            {
                BusinessDt = pMarginData.DtBusiness,
                CycleCode = pMarginData.JsonCycleCode,
                RunNumber = 1
            };

            portfolioReq.PointInTime = pointInTime;
            portfolioReq.Portfolios = new RiskPortfolio[1];

            RiskPortfolio riskPortfolio = new RiskPortfolio
            {
                Id = pMarginData.PortfolioId,
                Currency = pMarginData.Currency,
                CustomerAccountType = pMarginData.CustomerAccountType,
                OmnibusInd = pMarginData.OmnibusInd,

                Positions = new RiskPosition[3]
                {
                    new RiskPosition{
                        Instrument = new Instrument
                        {
                            ClearingOrganizationId = "CME",
                            ExchangeId = "NYMEX",
                            PeriodCode = "202206",
                            ProductCode = "CL",
                            ProductType = Span2RiskMargin.v1_0_39.ProductType.FUT
                        },
                        NetQty = 3
                    },
                    new RiskPosition{
                        Instrument = new Instrument
                        {
                            ClearingOrganizationId = "CME",
                            ExchangeId = "NYMEX",
                            PeriodCode = "202109",
                            ProductCode = "BG",
                            ProductType = Span2RiskMargin.v1_0_39.ProductType.FUT
                        },
                        NetQty = -3
                    },
                    new RiskPosition{
                        Instrument = new Instrument
                        {
                            ClearingOrganizationId = "CME",
                            ExchangeId = "NYMEX",
                            PeriodCode = "202306",
                            ProductCode = "OH",
                            ProductType = Span2RiskMargin.v1_0_39.ProductType.OOF,
                            PutCallInd = PutCallIndicator.C,
                            Strike = 1.2m,
                            UnderlyingPeriodCode = "202306"
                        },
                        NetQty = -9
                    }
                }
            };

            portfolioReq.Portfolios[0] = riskPortfolio;

            return portfolioRequestMessage;
        }

        public static FIXML TestCreateFixml(int pActorId, int pBookId, IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> pPosition, bool pIsOmnibus)
        {
            FIXML fixML = new FIXML
            {
                V = "5.0 SP2",
                VSpecified = true,
                Xv = "109",
                XvSpecified = true,
                S = "20090815"
            };
            TradeCaptureReport_message tradeCaptureReport_Message = new TradeCaptureReport_message();
            fixML.fixMsgTypeMessage = tradeCaptureReport_Message;
            fixML.fixMsgTypeMessageSpecified = true;

            tradeCaptureReport_Message.LastQty = 3;
            tradeCaptureReport_Message.LastQtySpecified = true;
            tradeCaptureReport_Message.LastPx = 10;
            tradeCaptureReport_Message.LastPxSpecified = true;

            InstrumentBlock instrumentBlock = new InstrumentBlock();
            tradeCaptureReport_Message.Instrmt = instrumentBlock;
            instrumentBlock.ID = "CL";
            instrumentBlock.IDSpecified = true;
            instrumentBlock.Src = FixML.v50SP1.Enum.SecurityIDSourceEnum.ClearingOrganization;
            instrumentBlock.SrcSpecified = true;
            instrumentBlock.SecTyp = FixML.v50SP1.Enum.SecurityTypeEnum.Future;
            instrumentBlock.SecTypSpecified = true;
            instrumentBlock.MMY = "202206";
            instrumentBlock.MMYSpecified = true;

            tradeCaptureReport_Message.RptSide = new TrdCapRptSideGrp_Block[1];
            TrdCapRptSideGrp_Block trdCapRptSideGrp_Block = new TrdCapRptSideGrp_Block();
            tradeCaptureReport_Message.RptSide[0] = trdCapRptSideGrp_Block;
            trdCapRptSideGrp_Block.Side = SideEnum.Buy;
            trdCapRptSideGrp_Block.InptDev = "API";
            trdCapRptSideGrp_Block.InptDevSpecified = true;

            trdCapRptSideGrp_Block.Pty = new Parties_Block[2]
            { 
                new Parties_Block
                {
                    R = FixML.v50SP1.Enum.PartyRoleEnum.ClearingFirm,
                    RSpecified = true,
                    ID = pActorId.ToString(),
                    IDSpecified = true,
                },
                new Parties_Block
                {
                    R = FixML.v50SP1.Enum.PartyRoleEnum.CustomerAccount,
                    RSpecified = true,
                    ID = pBookId.ToString(),
                    IDSpecified = true,
                }
            };
            return fixML;
        }

        public static string TestFixmlSerialized(int pActorId, int pBookId, IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> pPosition)
        {
            XmlSerializer xs = new XmlSerializer(typeof(FIXML));
            StringBuilder sb = new StringBuilder();
            StringWriterWithEncoding writer = new StringWriterWithEncoding(sb, Encoding.UTF8);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("xmlns", "www.cmegroup.com/fixml50/1");

            FIXML fixmlmessage = TestCreateFixml(pActorId, pBookId, pPosition, false);
            xs.Serialize(writer, fixmlmessage, namespaces);

            string xmlmsg1 = sb.ToString();
            return xmlmsg1;
        }
    }
}

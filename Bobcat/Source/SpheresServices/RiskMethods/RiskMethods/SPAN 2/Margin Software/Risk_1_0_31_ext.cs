/*
 * Risk_1_0_31.cs a été généré avec la commande suivante :
 * "C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\xsd" Risk_1_0_31.xsd /classes /eld /nologo /namespace:EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin.v1_0_31 /fields
 * */

using System.Text.Json.Serialization;

/// <summary>
/// <remarks> Ajout des accesseurs.
/// Certains sont devenus nullable en fonction de la valeur booléenne du nom du membre de même nom suivi de "Specified" lorsqu'il existe.</remarks>
/// </summary>
namespace EFS.SpheresRiskPerformance.RiskMethods.Span2RiskMargin.v1_0_31
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RiskPortfolioRequestMessage
    {
        #region Accessors
        public RiskPortfolioRequest Payload
        {
            get { return payload; }
            set { payload = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class RiskPortfolioRequest
    {
        #region Accessors
        public PointInTime PointInTime
        {
            get { return pointInTime; }
            set { pointInTime = value; }
        }

        public RiskPortfolio[] Portfolios
        {
            get { return portfolios; }
            set { portfolios = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PointInTime
    {
        #region Accessors
        /// <summary>
        /// BusinessDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonConverter(typeof(SPAN2BusinessDateJsonConverter))]
        public System.DateTime BusinessDt
        {
            get { return (businessDtSpecified ? businessDt : default); }
            set
            {
                businessDt = value;
                businessDtSpecified = (value != default);
            }
        }

        public CycleCode CycleCode
        {
            get { return cycleCode; }
            set { cycleCode = value; }
        }

        public int? RunNumber
        {
            get { return (runNumberSpecified ? runNumber : default); }
            set
            {
                runNumberSpecified = value.HasValue;
                if (value.HasValue)
                {
                    runNumber = value.Value;
                }
            }
        }

        /// <summary>
        /// Time n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type Time.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime Time
        {
            get { return (timeSpecified ? time : default); }
            set
            {
                time = value;
                timeSpecified = (value != default);
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarginSensitivityAmounts
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarginValuationAmounts
    {
        #region Accessors
        public decimal? NetPresentValue
        {
            get { return (netPresentValueSpecified ? netPresentValue : default); }
            set
            {
                netPresentValueSpecified = value.HasValue;
                if (value.HasValue)
                {
                    netPresentValue = value.Value;
                }
            }
        }

        public decimal? NonOptionValueLong
        {
            get { return (nonOptionValueLongSpecified ? nonOptionValueLong : default); }
            set
            {
                nonOptionValueLongSpecified = value.HasValue;
                if (value.HasValue)
                {
                    nonOptionValueLong = value.Value;
                }
            }
        }

        public decimal? NonOptionValueShort
        {
            get { return (nonOptionValueShortSpecified ? nonOptionValueShort : default); }
            set
            {
                nonOptionValueShortSpecified = value.HasValue;
                if (value.HasValue)
                {
                    nonOptionValueShort = value.Value;
                }
            }
        }

        public decimal? OptionValueLongEquityStyle
        {
            get { return (optionValueLongEquityStyleSpecified ? optionValueLongEquityStyle : default); }
            set
            {
                optionValueLongEquityStyleSpecified = value.HasValue;
                if (value.HasValue)
                {
                    optionValueLongEquityStyle = value.Value;
                }
            }
        }

        public decimal? OptionValueLongFuturesStyle
        {
            get { return (optionValueLongFuturesStyleSpecified ? optionValueLongFuturesStyle : default); }
            set
            {
                optionValueLongFuturesStyleSpecified = value.HasValue;
                if (value.HasValue)
                {
                    optionValueLongFuturesStyle = value.Value;
                }
            }
        }

        public decimal? OptionValueShortEquityStyle
        {
            get { return (optionValueShortEquityStyleSpecified ? optionValueShortEquityStyle : default); }
            set
            {
                optionValueShortEquityStyleSpecified = value.HasValue;
                if (value.HasValue)
                {
                    optionValueShortEquityStyle = value.Value;
                }
            }
        }

        public decimal? OptionValueShortFuturesStyle
        {
            get { return (optionValueShortFuturesStyleSpecified ? optionValueShortFuturesStyle : default); }
            set
            {
                optionValueShortFuturesStyleSpecified = value.HasValue;
                if (value.HasValue)
                {
                    optionValueShortFuturesStyle = value.Value;
                }
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarginComponentAmounts
    {
        #region Accessors
        public decimal? ConcentrationComponent
        {
            get { return (concentrationComponentSpecified ? concentrationComponent : default); }
            set
            {
                concentrationComponentSpecified = value.HasValue;
                if (value.HasValue)
                {
                    concentrationComponent = value.Value;
                }
            }
        }

        public decimal? FullValueComponent
        {
            get { return (fullValueComponentSpecified ? fullValueComponent : default); }
            set
            {
                fullValueComponentSpecified = value.HasValue;
                if (value.HasValue)
                {
                    fullValueComponent = value.Value;
                }
            }
        }

        public decimal? HvarComponent
        {
            get { return (hvarComponentSpecified ? hvarComponent : default); }
            set
            {
                hvarComponentSpecified = value.HasValue;
                if (value.HasValue)
                {
                    hvarComponent = value.Value;
                }
            }
        }

        public decimal? ImpliedOffset
        {
            get { return (impliedOffsetSpecified ? impliedOffset : default); }
            set
            {
                impliedOffsetSpecified = value.HasValue;
                if (value.HasValue)
                {
                    impliedOffset = value.Value;
                }
            }
        }

        public decimal? InterCmdtySpreadCredit
        {
            get { return (interCmdtySpreadCreditSpecified ? interCmdtySpreadCredit : default); }
            set
            {
                interCmdtySpreadCreditSpecified = value.HasValue;
                if (value.HasValue)
                {
                    interCmdtySpreadCredit = value.Value;
                }
            }
        }

        public decimal? InterCmdtyVolatilityCredit
        {
            get { return (interCmdtyVolatilityCreditSpecified ? interCmdtyVolatilityCredit : default); }
            set
            {
                interCmdtyVolatilityCreditSpecified = value.HasValue;
                if (value.HasValue)
                {
                    interCmdtyVolatilityCredit = value.Value;
                }
            }
        }

        public decimal? InterExchSpreadCredit
        {
            get { return (interExchSpreadCreditSpecified ? interExchSpreadCredit : default); }
            set
            {
                interExchSpreadCreditSpecified = value.HasValue;
                if (value.HasValue)
                {
                    interExchSpreadCredit = value.Value;
                }
            }
        }

        public decimal? IntraCmdtySpreadCharge
        {
            get { return (intraCmdtySpreadChargeSpecified ? intraCmdtySpreadCharge : default); }
            set
            {
                intraCmdtySpreadChargeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    intraCmdtySpreadCharge = value.Value;
                }
            }
        }

        public decimal? LiquidityComponent
        {
            get { return (liquidityComponentSpecified ? liquidityComponent : default); }
            set
            {
                liquidityComponentSpecified = value.HasValue;
                if (value.HasValue)
                {
                    liquidityComponent = value.Value;
                }
            }
        }

        public decimal? NakedLongComponent
        {
            get { return (nakedLongComponentSpecified ? nakedLongComponent : default); }
            set
            {
                nakedLongComponentSpecified = value.HasValue;
                if (value.HasValue)
                {
                    nakedLongComponent = value.Value;
                }
            }
        }

        public decimal? NakedShortComponent
        {
            get { return (nakedShortComponentSpecified ? nakedShortComponent : default); }
            set
            {
                nakedShortComponentSpecified = value.HasValue;
                if (value.HasValue)
                {
                    nakedShortComponent = value.Value;
                }
            }
        }

        public decimal? ScanRisk
        {
            get { return (scanRiskSpecified ? scanRisk : default); }
            set
            {
                scanRiskSpecified = value.HasValue;
                if (value.HasValue)
                {
                    scanRisk = value.Value;
                }
            }
        }

        public decimal? ShortOptionMin
        {
            get { return (shortOptionMinSpecified ? shortOptionMin : default); }
            set
            {
                shortOptionMinSpecified = value.HasValue;
                if (value.HasValue)
                {
                    shortOptionMin = value.Value;
                }
            }
        }

        public decimal? SpotCharge
        {
            get { return (spotChargeSpecified ? spotCharge : default); }
            set
            {
                spotChargeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    spotCharge = value.Value;
                }
            }
        }

        public decimal? StressComponent
        {
            get { return (stressComponentSpecified ? stressComponent : default); }
            set
            {
                stressComponentSpecified = value.HasValue;
                if (value.HasValue)
                {
                    stressComponent = value.Value;
                }
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarginRequirementAmounts
    {
        #region Accessors
        public MarginComponentAmounts ComponentAmts
        {
            get { return componentAmts; }
            set { componentAmts = value; }
        }

        public decimal? CrossModelOffset
        {
            get { return (crossModelOffsetSpecified ? crossModelOffset : default); }
            set
            {
                crossModelOffsetSpecified = value.HasValue;
                if (value.HasValue)
                {
                    crossModelOffset = value.Value;
                }
            }
        }

        public decimal? NetOptionValue
        {
            get { return (netOptionValueSpecified ? netOptionValue : default); }
            set
            {
                netOptionValueSpecified = value.HasValue;
                if (value.HasValue)
                {
                    netOptionValue = value.Value;
                }
            }
        }

        public decimal? RiskInitialRequirement
        {
            get { return (riskInitialRequirementSpecified ? riskInitialRequirement : default); }
            set
            {
                riskInitialRequirementSpecified = value.HasValue;
                if (value.HasValue)
                {
                    riskInitialRequirement = value.Value;
                }
            }
        }

        public decimal? RiskMaintenanceRequirement
        {
            get { return (riskMaintenanceRequirementSpecified ? riskMaintenanceRequirement : default); }
            set
            {
                riskMaintenanceRequirementSpecified = value.HasValue;
                if (value.HasValue)
                {
                    riskMaintenanceRequirement = value.Value;
                }
            }
        }

        public decimal? TotalInitialMargin
        {
            get { return (totalInitialMarginSpecified ? totalInitialMargin : default); }
            set
            {
                totalInitialMarginSpecified = value.HasValue;
                if (value.HasValue)
                {
                    totalInitialMargin = value.Value;
                }
            }
        }

        public decimal? TotalMaintenanceMargin
        {
            get { return (totalMaintenanceMarginSpecified ? totalMaintenanceMargin : default); }
            set
            {
                totalMaintenanceMarginSpecified = value.HasValue;
                if (value.HasValue)
                {
                    totalMaintenanceMargin = value.Value;
                }
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarginAmountsBase
    {
        #region Accessors
        public MarginRequirementAmounts RequirementAmts
        {
            get { return requirementAmts; }
            set { requirementAmts = value; }
        }

        public MarginValuationAmounts ValuationAmts
        {
            get { return valuationAmts; }
            set { valuationAmts = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PortfolioMarginDetail : MarginAmountsBase
    {
        #region Accessors
        public CcpMarginAmounts[] Ccps
        {
            get { return ccps; }
            set { ccps = value; }
        }

        public Currency? Currency
        {
            get { return (currencySpecified ? currency : default(Currency?)); }
            set
            {
                currencySpecified = value.HasValue;
                if (value.HasValue)
                {
                    currency = value.Value;
                }
            }
        }

        public CurrencyMarginAmounts[] CurrencyAmts
        {
            get { return currencyAmts; }
            set { currencyAmts = value; }
        }

        public CustomerAccountType? CustomerAccountType
        {
            get { return (customerAccountTypeSpecified ? customerAccountType : default(CustomerAccountType?)); }
            set
            {
                customerAccountTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    customerAccountType = value.Value;
                }
            }
        }

        public RiskPortfolioEntities Entities
        {
            get { return entities; }
            set { entities = value; }
        }

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Memo
        {
            get { return memo; }
            set { memo = value; }
        }

        public YesNoIndicator? OmnibusInd
        {
            get { return (omnibusIndSpecified ? omnibusInd : default(YesNoIndicator?)); }
            set
            {
                omnibusIndSpecified = value.HasValue;
                if (value.HasValue)
                {
                    omnibusInd = value.Value;
                }
            }
        }

        public string ParentPortfolioId
        {
            get { return parentPortfolioId; }
            set { parentPortfolioId = value; }
        }

        public int? TransactionCnt
        {
            get { return (transactionCntSpecified ? transactionCnt : default); }
            set
            {
                transactionCntSpecified = value.HasValue;
                if (value.HasValue)
                {
                    transactionCnt = value.Value;
                }
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CcpMarginAmounts : MarginAmountsBase
    {
        #region Accessors
        public string ClearingOrganizationId
        {
            get { return clearingOrganizationId; }
            set { clearingOrganizationId = value; }
        }

        public CurrencyMarginAmounts[] CurrencyAmts
        {
            get { return currencyAmts; }
            set { currencyAmts = value; }
        }

        public CustomerAccountType? CustomerAccountType
        {
            get { return (customerAccountTypeSpecified ? customerAccountType : default(CustomerAccountType?)); }
            set
            {
                customerAccountTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    customerAccountType = value.Value;
                }
            }
        }

        public PodMarginAmounts[] Pod
        {
            get { return pod; }
            set { pod = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CurrencyMarginAmounts : MarginAmountsBase
    {
        #region Accessors
        public Currency? Currency
        {
            get { return (currencySpecified ? currency : default(Currency?)); }
            set
            {
                currencySpecified = value.HasValue;
                if (value.HasValue)
                {
                    currency = value.Value;
                }
            }
        }

        public decimal? ExchangeRt
        {
            get { return (exchangeRtSpecified ? exchangeRt : default); }
            set
            {
                exchangeRtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    exchangeRt = value.Value;
                }
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PodMarginAmounts : MarginAmountsBase
    {
        #region Accessors
        public Currency? Currency
        {
            get { return (currencySpecified ? currency : default(Currency?)); }
            set
            {
                currencySpecified = value.HasValue;
                if (value.HasValue)
                {
                    currency = value.Value;
                }
            }
        }

        public CurrencyMarginAmounts[] CurrencyAmts
        {
            get { return currencyAmts; }
            set { currencyAmts = value; }
        }

        public CustomerAccountType? CustomerAccountType
        {
            get { return (customerAccountTypeSpecified ? customerAccountType : default(CustomerAccountType?)); }
            set
            {
                customerAccountTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    customerAccountType = value.Value;
                }
            }
        }

        public MarginMethod MarginMthd
        {
            get { return marginMthd; }
            set { marginMthd = value; }
        }

        public string PodId
        {
            get { return podId; }
            set { podId = value; }
        }

        public string ProductDescription
        {
            get { return productDescription; }
            set { productDescription = value; }
        }

        public ProductGroupAmounts[] ProductGroup
        {
            get { return productGroup; }
            set { productGroup = value; }
        }

        public MarginSensitivityAmounts SensitivityAmts
        {
            get { return sensitivityAmts; }
            set { sensitivityAmts = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ProductGroupAmounts : MarginAmountsBase
    {
        #region Accessors
        public Currency? Currency
        {
            get { return (currencySpecified ? currency : default(Currency?)); }
            set
            {
                currencySpecified = value.HasValue;
                if (value.HasValue)
                {
                    currency = value.Value;
                }
            }
        }

        public CustomerAccountType? CustomerAccountType
        {
            get { return (customerAccountTypeSpecified ? customerAccountType : default(CustomerAccountType?)); }
            set
            {
                customerAccountTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    customerAccountType = value.Value;
                }
            }
        }

        public string ProductDescription
        {
            get { return productDescription; }
            set { productDescription = value; }
        }

        public string ProductGroupId
        {
            get { return productGroupId; }
            set { productGroupId = value; }
        }

        public MarginSensitivityAmounts SensitivityAmts
        {
            get { return SensitivityAmts; }
            set { sensitivityAmts = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class RiskPortfolioEntities
    {
        #region Accessors
        public string AccountId
        {
            get { return accountId; }
            set { accountId = value; }
        }

        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }

        public string FirmId
        {
            get { return firmId; }
            set { firmId = value; }
        }

        public OriginType? OriginType
        {
            get { return (originTypeSpecified ? originType : default(OriginType?)); }
            set
            {
                originTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    originType = value.Value;
                }
            }
        }

        public FundSegregationType? SegregationType
        {
            get { return (segregationTypeSpecified ? segregationType : default(FundSegregationType?)); }
            set
            {
                segregationTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    segregationType = value.Value;
                }
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarginDetailResponse
    {
        #region Accessors
        public PointInTime PointInTime
        {
            get { return pointInTime; }
            set { pointInTime = value; }
        }

        public PortfolioMarginDetail[] Portfolios
        {
            get { return portfolios; }
            set { portfolios = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class Error
    {
        #region Accessors
        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ResponseBase
    {
        #region Accessors
        public Error[] Errors
        {
            get { return errors; }
            set { errors = value; }
        }

        public ResponseHeader Header
        {
            get { return header; }
            set { header = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ResponseHeader : HeaderBase
    {
        #region Accessors
        public AcknowledgementStatus? AcknowledgementStatus
        {
            get { return (acknowledgementStatusSpecified ? acknowledgementStatus : default(AcknowledgementStatus?)); }
            set
            {
                acknowledgementStatusSpecified = value.HasValue;
                if (value.HasValue)
                {
                    acknowledgementStatus = value.Value;
                }
            }
        }

        public ResponseType? ResponseType
        {
            get { return (responseTypeSpecified ? responseType : default(ResponseType?)); }
            set
            {
                responseTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    responseType = value.Value;
                }
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class HeaderBase
    {
        #region Accessors
        public string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }

        public string ApplicationVendor
        {
            get { return applicationVendor; }
            set { applicationVendor = value; }
        }

        public string ApplicationVersion
        {
            get { return applicationVersion; }
            set { applicationVersion = value; }
        }

        public string RequestId
        {
            get { return requestId; }
            set { requestId = value; }
        }

        /// <summary>
        /// SentTime n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime SentTime
        {
            get { return (sentTimeSpecified ? sentTime : default); }
            set
            {
                sentTime = value;
                sentTimeSpecified = (value != default);
            }
        }

        public string Version
        {
            get { return version; }
            set { version = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class RequestHeader : HeaderBase
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PositionBase
    {
        #region Accessors
        public Instrument Instrument
        {
            get { return instrument; }
            set { instrument = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class Instrument
    {
        #region Accessors
        public string ClearingOrganizationId
        {
            get { return clearingOrganizationId; }
            set { clearingOrganizationId = value; }
        }

        public string ExchangeId
        {
            get { return exchangeId; }
            set { exchangeId = value; }
        }

        /// <summary>
        /// ExpirationDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime ExpirationDt
        {
            get { return (expirationDtSpecified ? expirationDt : default); }
            set
            {
                expirationDt = value;
                expirationDtSpecified = (value != default);
            }
        }

        /// <summary>
        /// FixingDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime FixingDt
        {
            get { return (fixingDtSpecified ? fixingDt : default); }
            set
            {
                fixingDt = value;
                fixingDtSpecified = (value != default);
            }
        }

        public string PeriodCode
        {
            get { return periodCode; }
            set { periodCode = value; }
        }

        public string ProductCode
        {
            get { return productCode; }
            set { productCode = value; }
        }

        public ProductSubType? ProductSubType
        {
            get { return (productSubTypeSpecified ? productSubType : default(ProductSubType?)); }
            set
            {
                productSubTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    productSubType = value.Value;
                }
            }
        }

        public ProductType? ProductType
        {
            get { return (productTypeSpecified ? productType : default(ProductType?)); }
            set
            {
                productTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    productType = value.Value;
                }
            }
        }

        public PutCallIndicator? PutCallInd
        {
            get { return (putCallIndSpecified ? putCallInd : default(PutCallIndicator?)); }
            set
            {
                putCallIndSpecified = value.HasValue;
                if (value.HasValue)
                {
                    putCallInd = value.Value;
                }
            }
        }

        public decimal? Strike
        {
            get { return (strikeSpecified ? strike : default); }
            set
            {
                strikeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    strike = value.Value;
                }
            }
        }

        public string Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }

        public string UnderlyingPeriodCode
        {
            get { return underlyingPeriodCode; }
            set { underlyingPeriodCode = value; }
        }

        public string UnderlyingProductCode
        {
            get { return underlyingProductCode; }
            set { underlyingProductCode = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class RiskPosition : PositionBase
    {
        #region Accessors
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CustomerAccountType? CustomerAccountType
        {
            get { return customerAccountTypeSpecified ? customerAccountType : default(CustomerAccountType?); }
            set
            {
                customerAccountTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    customerAccountType = value.Value;
                }
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? NakedLongQty
        {
            get { return (nakedLongQtySpecified ? nakedLongQty : default); }
            set
            {
                nakedLongQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    nakedLongQty = value.Value;
                }
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? NakedShortQty
        {
            get { return (nakedShortQtySpecified ? nakedShortQty : default); }
            set
            {
                nakedShortQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    nakedShortQty = value.Value;
                }
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? NetQty
        {
            get { return (netQtySpecified ? netQty : default); }
            set
            {
                netQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    netQty = value.Value;                    
                }
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class TradePayments
    {
        #region Accessors
        public decimal? TotalPresentValueAmt
        {
            get { return totalPresentValueAmtSpecified ? totalPresentValueAmt : default; }
            set
            {
                totalPresentValueAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    totalPresentValueAmt = value.Value;
                }
            }
        }

        public decimal? TransferAmt
        {
            get { return transferAmtSpecified ? transferAmt : default; }
            set
            {
                transferAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    transferAmt = value.Value;
                }
            }
        }

        /// <summary>
        /// TransferDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime TransferDt
        {
            get { return (transferDtSpecified ? transferDt : default); }
            set
            {
                transferDt = value;
                transferDtSpecified = (value != default);
            }
        }

        public decimal? UpfrontAmt
        {
            get { return upfrontAmtSpecified ? upfrontAmt : default; }
            set
            {
                upfrontAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    upfrontAmt = value.Value;
                }
            }
        }

        /// <summary>
        /// UpfrontDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime UpfrontDt
        {
            get { return (upfrontDtSpecified ? upfrontDt : default); }
            set
            {
                upfrontDt = value;
                upfrontDtSpecified = (value != default);
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class NotionalSchedule
    {
        #region Accessors
        public decimal? NotionalAmt
        {
            get { return notionalAmtSpecified ? notionalAmt : default; }
            set
            {
                notionalAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    notionalAmt = value.Value;
                }
            }
        }

        /// <summary>
        /// StepDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime StepDt
        {
            get { return (stepDtSpecified ? stepDt : default); }
            set
            {
                stepDt = value;
                stepDtSpecified = (value != default);
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class TradeAmounts
    {
        #region Accessors
        public decimal? AccuredInterestAmt
        {
            get { return accuredInterestAmtSpecified ? accuredInterestAmt : default; }
            set
            {
                accuredInterestAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    accuredInterestAmt = value.Value;
                }
            }
        }

        public decimal? CouponAmt
        {
             get { return couponAmtSpecified ? couponAmt : default; }
            set
            {
                couponAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    couponAmt = value.Value;
                }
            }
        }

        public decimal? PriceAlignmentAmt
        {
            get { return priceAlignmentAmtSpecified ? priceAlignmentAmt : default; }
            set
            {
                priceAlignmentAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    priceAlignmentAmt = value.Value;
                }
            }
        }

        public decimal? PriceAlignmentRt
        {
            get { return priceAlignmentRtSpecified ? priceAlignmentRt : default; }
            set
            {
                priceAlignmentRtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    priceAlignmentRt = value.Value;
                }
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class IrsTradeLeg
    {
        #region Accessors
        public TradeAmounts[] Amts
        {
            get { return amts; }
            set { amts = value; }
        }

        public AveragingMethod? AveragingMthd
        {
            get { return (averagingMthdSpecified ? averagingMthd : default(AveragingMethod?)); }
            set
            {
                averagingMthdSpecified = value.HasValue;
                if (value.HasValue)
                {
                    averagingMthd = value.Value;
                }
            }
        }

        public BusinessCenter[] CalcPeriodBusinessCenters
        {
            get { return calcPeriodBusinessCenters; }
            set { calcPeriodBusinessCenters = value; }
        }

        public BusinessDateConvention? CalcPeriodConvention
        {
            get { return (calcPeriodConventionSpecified ? calcPeriodConvention : default(BusinessDateConvention?)); }
            set
            {
                calcPeriodConventionSpecified = value.HasValue;
                if (value.HasValue)
                {
                    calcPeriodConvention = value.Value;
                }
            }
        }

        public int? CalcPeriodFrequencyQty
        {
            get { return (calcPeriodFrequencyQtySpecified ? calcPeriodFrequencyQty : default); }
            set
            {
                calcPeriodFrequencyQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    calcPeriodFrequencyQty = value.Value;
                }
            }
        }

        public TimeUnit? CalcPeriodFrequencyUnit
        {
            get { return (calcPeriodFrequencyUnitSpecified ? calcPeriodFrequencyUnit : default(TimeUnit?)); }
            set
            {
                calcPeriodFrequencyUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    calcPeriodFrequencyUnit = value.Value;
                }
            }
        }

        /// <summary>
        /// CalcPeriodRegularEndDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime CalcPeriodRegularEndDt
        {
            get { return (calcPeriodRegularEndDtSpecified ? calcPeriodRegularEndDt : default); }
            set
            {
                calcPeriodRegularEndDt = value;
                calcPeriodRegularEndDtSpecified = (value != default);
            }
        }

        /// <summary>
        /// CalcPeriodRegularStartDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime CalcPeriodRegularStartDt
        {
            get { return (calcPeriodRegularStartDtSpecified ? calcPeriodRegularStartDt : default); }
            set
            {
                calcPeriodRegularStartDt = value;
                calcPeriodRegularStartDtSpecified = (value != default);
            }
        }

        public decimal? CleanPrice
        {
            get { return (cleanPriceSpecified ? cleanPrice : default); }
            set
            {
                cleanPriceSpecified = value.HasValue;
                if (value.HasValue)
                {
                    cleanPrice = value.Value;
                }
            }
        }

        public CompoundingMethod? CompoundingMthd
        {
            get { return (compoundingMthdSpecified ? compoundingMthd : default(CompoundingMethod?)); }
            set
            {
                compoundingMthdSpecified = value.HasValue;
                if (value.HasValue)
                {
                    compoundingMthd = value.Value;
                }
            }
        }

        public Currency? Currency
        {
            get { return (currencySpecified ? currency : default(Currency?)); }
            set
            {
                currencySpecified = value.HasValue;
                if (value.HasValue)
                {
                    currency = value.Value;
                }
            }
        }

        public PaymentDayCount? DayCnt
        {
            get { return (dayCntSpecified ? dayCnt : default(PaymentDayCount?)); }
            set
            {
                dayCntSpecified = value.HasValue;
                if (value.HasValue)
                {
                    dayCnt = value.Value;
                }
            }
        }

        public DirectionIndicator? DirectionInd
        {
             get { return (directionIndSpecified ? directionInd : default(DirectionIndicator?)); }
            set
            {
                directionIndSpecified = value.HasValue;
                if (value.HasValue)
                {
                    directionInd = value.Value;
                }
            }
        }

        public decimal? DirtyPrice
        {
            get { return (dirtyPriceSpecified ? dirtyPrice : default); }
            set
            {
                dirtyPriceSpecified = value.HasValue;
                if (value.HasValue)
                {
                    dirtyPrice = value.Value;
                }
            }
        }

        public DiscountingType? DiscountingType
        {
            get { return (discountingTypeSpecified ? discountingType : default(DiscountingType?)); }
            set
            {
                discountingTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    discountingType = value.Value;
                }
            }
        }

        /// <summary>
        /// EndDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime EndDt
        {
            get { return (endDtSpecified ? endDt : default); }
            set
            {
                endDt = value;
                endDtSpecified = (value != default);
            }
        }

        public BusinessCenter[] EndDtBusinessCenters
        {
            get { return endDtBusinessCenters; }
            set { endDtBusinessCenters = value; }
        }

        public BusinessDateConvention? EndDtConvention
        {
            get { return (endDtConventionSpecified ? endDtConvention : default(BusinessDateConvention?)); }
            set
            {
                endDtConventionSpecified = value.HasValue;
                if (value.HasValue)
                {
                    endDtConvention = value.Value;
                }
            }
        }

        public int? FinalStubIndex1TenorQty
        {
            get { return (finalStubIndex1TenorQtySpecified ? finalStubIndex1TenorQty : default); }
            set
            {
                finalStubIndex1TenorQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    finalStubIndex1TenorQty = value.Value;
                }
            }
        }

        public TimeUnit? FinalStubIndex1TenorUnit
        {
            get { return (finalStubIndex1TenorUnitSpecified ? finalStubIndex1TenorUnit : default(TimeUnit?)); }
            set
            {
                finalStubIndex1TenorUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    finalStubIndex1TenorUnit = value.Value;
                }
            }
        }

        public int? FinalStubIndex2TenorQty
        {
            get { return (finalStubIndex2TenorQtySpecified ? finalStubIndex2TenorQty : default); }
            set
            {
                finalStubIndex2TenorQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    finalStubIndex2TenorQty = value.Value;
                }
            }
        }

        public TimeUnit? FinalStubIndex2TenorUnit
        {
            get { return (finalStubIndex2TenorUnitSpecified ? finalStubIndex2TenorUnit : default(TimeUnit?)); }
            set
            {
                finalStubIndex2TenorUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    finalStubIndex2TenorUnit = value.Value;
                }
            }
        }

        public decimal? FinalStubRt
        {
            get { return (finalStubRtSpecified ? finalStubRt : default); }
            set
            {
                finalStubRtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    finalStubRt = value.Value;
                }
            }
        }

        public RateCalculationMethod? FinalStubRtCalcMthd
        {
            get { return (finalStubRtCalcMthdSpecified ? finalStubRtCalcMthd : default(RateCalculationMethod?)); }
            set
            {
                finalStubRtCalcMthdSpecified = value.HasValue;
                if (value.HasValue)
                {
                    finalStubRtCalcMthd = value.Value;
                }
            }
        }

        public IrsStubType? FinalStubType
        {
            get { return (finalStubTypeSpecified ? finalStubType : default(IrsStubType?)); }
            set
            {
                finalStubTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    finalStubType = value.Value;
                }
            }
        }

        public decimal? FixedRt
        {
            get { return (fixedRtSpecified ? fixedRt : default); }
            set
            {
                fixedRtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    fixedRt = value.Value;
                }
            }
        }

        public BusinessCenter[] FixingDtBusinessCenters
        {
            get { return fixingDtBusinessCenters; }
            set { fixingDtBusinessCenters = value; }
        }

        public BusinessDateConvention? FixingDtConvention
        {
            get { return (fixingDtConventionSpecified ? fixingDtConvention : default(BusinessDateConvention?)); }
            set
            {
                fixingDtConventionSpecified = value.HasValue;
                if (value.HasValue)
                {
                    fixingDtConvention = value.Value;
                }
            }
        }

        public BusinessCenter[] FixingDtInitialBusinessCenters
        {
            get { return fixingDtInitialBusinessCenters; }
            set { fixingDtInitialBusinessCenters = value; }
        }

        public BusinessDateConvention? FixingDtInitialConvention
        {
            get { return (fixingDtInitialConventionSpecified ? fixingDtInitialConvention : default(BusinessDateConvention?)); }
            set
            {
                fixingDtInitialConventionSpecified = value.HasValue;
                if (value.HasValue)
                {
                    fixingDtInitialConvention = value.Value;
                }
            }
        }

        public int? FixingDtInitialOffsetQty
        {
            get { return (fixingDtInitialOffsetQtySpecified ? fixingDtInitialOffsetQty : default); }
            set
            {
                fixingDtInitialOffsetQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    fixingDtInitialOffsetQty = value.Value;
                }
            }
        }

        public TimeUnit? FixingDtInitialOffsetUnit
        {
            get { return (fixingDtInitialOffsetUnitSpecified ? fixingDtInitialOffsetUnit : default(TimeUnit?)); }
            set
            {
                fixingDtInitialOffsetUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    fixingDtInitialOffsetUnit = value.Value;
                }
            }
        }

        public DateType? FixingDtInitialType
        {
            get { return (fixingDtInitialTypeSpecified ? fixingDtInitialType : default(DateType?)); }
            set
            {
                fixingDtInitialTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    fixingDtInitialType = value.Value;
                }
            }
        }

        public int? FixingDtOffsetQty
        {
            get { return (fixingDtOffsetQtySpecified ? fixingDtOffsetQty : default); }
            set
            {
                fixingDtOffsetQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    fixingDtOffsetQty = value.Value;
                }
            }
        }

        public TimeUnit? FixingDtOffsetUnit
        {
            get { return (fixingDtOffsetUnitSpecified ? fixingDtOffsetUnit : default(TimeUnit?)); }
            set
            {
                fixingDtOffsetUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    fixingDtOffsetUnit = value.Value;
                }
            }
        }

        public DateType? FixingDtType
        {
            get { return (fixingDtTypeSpecified ? fixingDtType : default(DateType?)); }
            set
            {
                fixingDtTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    fixingDtType = value.Value;
                }
            }
        }

        public decimal? FutureValueNotionalAmt
        {
            get { return (futureValueNotionalAmtSpecified ? futureValueNotionalAmt : default); }
            set
            {
                futureValueNotionalAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    futureValueNotionalAmt = value.Value;
                }
            }
        }

        public string Index
        {
            get { return index; }
            set { index = value; }
        }

        public int? IndexTenor1Qty
        {
            get { return (indexTenor1QtySpecified ? indexTenor1Qty : default); }
            set
            {
                indexTenor1QtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    indexTenor1Qty = value.Value;
                }
            }
        }

        public TimeUnit? IndexTenor1Unit
        {
            get { return (indexTenor1UnitSpecified ? indexTenor1Unit : default(TimeUnit?)); }
            set
            {
                indexTenor1UnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    indexTenor1Unit = value.Value;
                }
            }
        }

        public int? IndexTenor2Qty
        {
            get { return (indexTenor2QtySpecified ? indexTenor2Qty : default); }
            set
            {
                indexTenor2QtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    indexTenor2Qty = value.Value;
                }
            }
        }

        public TimeUnit? IndexTenor2Unit
        {
            get { return (indexTenor2UnitSpecified ? indexTenor2Unit : default(TimeUnit?)); }
            set
            {
                indexTenor2UnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    indexTenor2Unit = value.Value;
                }
            }
        }

        public int? InitialStubIndex1TenorQty
        {
            get { return (initialStubIndex1TenorQtySpecified ? initialStubIndex1TenorQty : default); }
            set
            {
                initialStubIndex1TenorQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    initialStubIndex1TenorQty = value.Value;
                }
            }
        }

        public TimeUnit? InitialStubIndex1TenorUnit
        {
            get { return (initialStubIndex1TenorUnitSpecified ? initialStubIndex1TenorUnit : default(TimeUnit?)); }
            set
            {
                initialStubIndex1TenorUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    initialStubIndex1TenorUnit = value.Value;
                }
            }
        }

        public int? InitialStubIndex2TenorQty
        {
            get { return (initialStubIndex2TenorQtySpecified ? initialStubIndex2TenorQty : default); }
            set
            {
                initialStubIndex2TenorQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    initialStubIndex2TenorQty = value.Value;
                }
            }
        }

        public TimeUnit? InitialStubIndex2TenorUnit
        {
            get { return (initialStubIndex2TenorUnitSpecified ? initialStubIndex2TenorUnit : default(TimeUnit?)); }
            set
            {
                initialStubIndex2TenorUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    initialStubIndex2TenorUnit = value.Value;
                }
            }
        }

        public decimal? InitialStubRt
        {
            get { return (initialStubRtSpecified ? initialStubRt : default); }
            set
            {
                initialStubRtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    initialStubRt = value.Value;
                }
            }
        }

        public RateCalculationMethod? InitialStubRtCalcMthd
        {
            get { return (initialStubRtCalcMthdSpecified ? initialStubRtCalcMthd : default(RateCalculationMethod?)); }
            set
            {
                initialStubRtCalcMthdSpecified = value.HasValue;
                if (value.HasValue)
                {
                    initialStubRtCalcMthd = value.Value;
                }
            }
        }

        public IrsStubType? InitialStubType
        {
            get { return (initialStubTypeSpecified ? initialStubType : default(IrsStubType?)); }
            set
            {
                initialStubTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    initialStubType = value.Value;
                }
            }
        }

        public decimal? KnownAmt
        {
            get { return (knownAmtSpecified ? knownAmt : default); }
            set
            {
                knownAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    knownAmt = value.Value;
                }
            }
        }

        public decimal? NetCashFlowAmt
        {
            get { return (netCashFlowAmtSpecified ? netCashFlowAmt : default); }
            set
            {
                netCashFlowAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    netCashFlowAmt = value.Value;
                }
            }
        }

        public decimal? NotionalAmt
        {
            get { return (notionalAmtSpecified ? notionalAmt : default); }
            set
            {
                notionalAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    notionalAmt = value.Value;
                }
            }
        }

        public NotionalSchedule[] NotionalSchedules
        {
            get { return notionalSchedules; }
            set { notionalSchedules = value; }
        }

        public BusinessCenter[] PaymentDtBusinessCenters
        {
            get { return paymentDtBusinessCenters; }
            set { paymentDtBusinessCenters = value; }
        }

        public BusinessDateConvention? PaymentDtConvention
        {
            get { return (paymentDtConventionSpecified ? paymentDtConvention : default(BusinessDateConvention?)); }
            set
            {
                paymentDtConventionSpecified = value.HasValue;
                if (value.HasValue)
                {
                    paymentDtConvention = value.Value;
                }
            }
        }

        public int? PaymentDtOffsetQty
        {
            get { return (paymentDtOffsetQtySpecified ? paymentDtOffsetQty : default); }
            set
            {
                paymentDtOffsetQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    paymentDtOffsetQty = value.Value;
                }
            }
        }

        public TimeUnit? PaymentDtOffsetUnit
        {
            get { return (paymentDtOffsetUnitSpecified ? paymentDtOffsetUnit : default(TimeUnit?)); }
            set
            {
                paymentDtOffsetUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    paymentDtOffsetUnit = value.Value;
                }
            }
        }

        public DateRelationshipType? PaymentDtRelationshipType
        {
            get { return (paymentDtRelationshipTypeSpecified ? paymentDtRelationshipType : default(DateRelationshipType?)); }
            set
            {
                paymentDtRelationshipTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    paymentDtRelationshipType = value.Value;
                }
            }
        }

        public PaymentDateRollConvention? PaymentDtRollConvention
        {
            get { return (paymentDtRollConventionSpecified ? paymentDtRollConvention : default(PaymentDateRollConvention?)); }
            set
            {
                paymentDtRollConventionSpecified = value.HasValue;
                if (value.HasValue)
                {
                    paymentDtRollConvention = value.Value;
                }
            }
        }

        public int? PaymentFrequencyQty
        {
            get { return (paymentFrequencyQtySpecified ? paymentFrequencyQty : default); }
            set
            {
                paymentFrequencyQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    paymentFrequencyQty = value.Value;
                }
            }
        }

        public TimeUnit? PaymentFrequencyUnit
        {
            get { return (paymentFrequencyUnitSpecified ? paymentFrequencyUnit : default(TimeUnit?)); }
            set
            {
                paymentFrequencyUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    paymentFrequencyUnit = value.Value;
                }
            }
        }

        public TradePayments Payments
        {
            get { return payments; }
            set { payments = value; }
        }

        public DateType? ResetCutOffDtType
        {
             get { return (resetCutOffDtTypeSpecified ? resetCutOffDtType : default(DateType?)); }
            set
            {
                resetCutOffDtTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    resetCutOffDtType = value.Value;
                }
            }
        }

        public int? ResetCutOffOffsetQty
        {
            get { return (resetCutOffOffsetQtySpecified ? resetCutOffOffsetQty : default); }
            set
            {
                resetCutOffOffsetQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    resetCutOffOffsetQty = value.Value;
                }
            }
        }

        public TimeUnit? ResetCutOffOffsetUnit
        {
            get { return (resetCutOffOffsetUnitSpecified ? resetCutOffOffsetUnit : default(TimeUnit?)); }
            set
            {
                resetCutOffOffsetUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    resetCutOffOffsetUnit = value.Value;
                }
            }
        }

        public BusinessCenter[] ResetDtBusinessCenters
        {
            get { return resetDtBusinessCenters; }
            set { resetDtBusinessCenters = value; }
        }

        public BusinessDateConvention? ResetDtConvention
        {
            get { return (resetDtConventionSpecified ? resetDtConvention : default(BusinessDateConvention?)); }
            set
            {
                resetDtConventionSpecified = value.HasValue;
                if (value.HasValue)
                {
                    resetDtConvention = value.Value;
                }
            }
        }

        public DateRelationshipType? ResetDtRelationshipType
        {
            get { return (resetDtRelationshipTypeSpecified ? resetDtRelationshipType : default(DateRelationshipType?)); }
            set
            {
                resetDtRelationshipTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    resetDtRelationshipType = value.Value;
                }
            }
        }

        public int? ResetFrequencyQty
        {
            get { return (resetFrequencyQtySpecified ? resetFrequencyQty : default); }
            set
            {
                resetFrequencyQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    resetFrequencyQty = value.Value;
                }
            }
        }

        public TimeUnit? ResetFrequencyUnit
        {
            get { return (resetFrequencyUnitSpecified ? resetFrequencyUnit : default(TimeUnit?)); }
            set
            {
                resetFrequencyUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    resetFrequencyUnit = value.Value;
                }
            }
        }

        public Currency? SettlementCurrency
        {
            get { return (settlementCurrencySpecified ? settlementCurrency : default(Currency?)); }
            set
            {
                settlementCurrencySpecified = value.HasValue;
                if (value.HasValue)
                {
                    settlementCurrency = value.Value;
                }
            }
        }

        public decimal? SettlementRt
        {
            get { return (settlementRtSpecified ? settlementRt : default); }
            set
            {
                settlementRtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    settlementRt = value.Value;
                }
            }
        }

        /// <summary>
        /// SettlementRtFixingDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime SettlementRtFixingDt
        {
            get { return (settlementRtFixingDtSpecified ? settlementRtFixingDt : default); }
            set
            {
                settlementRtFixingDt = value;
                settlementRtFixingDtSpecified = (value != default);
            }
        }

        public BusinessCenter[] SettlementRtFixingDtBusinessCenters
        {
            get { return settlementRtFixingDtBusinessCenters; }
            set { settlementRtFixingDtBusinessCenters = value; }
        }

        public BusinessDateConvention? SettlementRtFixingDtConvention
        {
            get { return (settlementRtFixingDtConventionSpecified ? settlementRtFixingDtConvention : default(BusinessDateConvention?)); }
            set
            {
                settlementRtFixingDtConventionSpecified = value.HasValue;
                if (value.HasValue)
                {
                    settlementRtFixingDtConvention = value.Value;
                }
            }
        }

        public int? SettlementRtFixingDtOffsetQty
        {
            get { return (settlementRtFixingDtOffsetQtySpecified ? settlementRtFixingDtOffsetQty : default); }
            set
            {
                settlementRtFixingDtOffsetQtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    settlementRtFixingDtOffsetQty = value.Value;
                }
            }
        }

        public TimeUnit? SettlementRtFixingDtOffsetUnit
        {
            get { return (settlementRtFixingDtOffsetUnitSpecified ? settlementRtFixingDtOffsetUnit : default(TimeUnit?)); }
            set
            {
                settlementRtFixingDtOffsetUnitSpecified = value.HasValue;
                if (value.HasValue)
                {
                    settlementRtFixingDtOffsetUnit = value.Value;
                }
            }
        }

        public DateType? SettlementRtFixingDtType
        {
            get { return (settlementRtFixingDtTypeSpecified ? settlementRtFixingDtType : default(DateType?)); }
            set
            {
                settlementRtFixingDtTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    settlementRtFixingDtType = value.Value;
                }
            }
        }

        public decimal? SpreadAmt
        {
            get { return (spreadAmtSpecified ? spreadAmt : default); }
            set
            {
                spreadAmtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    spreadAmt = value.Value;
                }
            }
        }

        /// <summary>
        /// StartDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime StartDt
        {
            get { return (startDtSpecified ? startDt : default); }
            set
            {
                startDt = value;
                startDtSpecified = (value != default);
            }
        }

        public IrsLegType? Type
        {
            get { return (typeSpecified ? type : default(IrsLegType?)); }
            set
            {
                typeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    type = value.Value;
                }
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class Payment
    {
        #region Accessors
        public decimal? Amt
        {
            get { return (amtSpecified ? amt : default); }
            set
            {
                amtSpecified = value.HasValue;
                if (value.HasValue)
                {
                    amt = value.Value;
                }
            }
        }

        public BusinessCenter[] BusinessCenters
        {
            get { return businessCenters; }
            set { businessCenters = value; }
        }

        public Currency? Currency
        {
            get { return (currencySpecified ? currency : default(Currency?)); }
            set
            {
                currencySpecified = value.HasValue;
                if (value.HasValue)
                {
                    currency = value.Value;
                }
            }
        }

        /// <summary>
        /// Dt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime Dt
        {
            get { return (dtSpecified ? dt : default); }
            set
            {
                dt = value;
                dtSpecified = (value != default);
            }
        }

        public BusinessDateConvention? DtConvention
        {
            get { return (dtConventionSpecified ? dtConvention : default(BusinessDateConvention?)); }
            set
            {
                dtConventionSpecified = value.HasValue;
                if (value.HasValue)
                {
                    dtConvention = value.Value;
                }
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class TradeSide
    {
        #region Accessors
        public Payment PremiumPayment
        {
            get { return premiumPayment; }
            set { premiumPayment = value; }
        }

        public Payment TransferPayment
        {
            get { return transferPayment; }
            set { transferPayment = value; }
        }

        public Payment UpfrontPayment
        {
            get { return upfrontPayment; }
            set { upfrontPayment = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class TransactionBase
    {
        #region Accessors
        /// <summary>
        /// TransactionTime n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime TransactionTime
        {
            get { return (transactionTimeSpecified ? transactionTime : default); }
            set
            {
                transactionTime = value;
                transactionTimeSpecified = (value != default);
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class TradeBase : TransactionBase
    {
        #region Accessors
        /// <summary>
        /// ExecutionTime n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime ExecutionTime
        {
            get { return (executionTimeSpecified ? executionTime : default); }
            set
            {
                executionTime = value;
                executionTimeSpecified = (value != default);
            }
        }

        public Instrument Instrument
        {
            get { return instrument; }
            set { instrument = value; }
        }

        public decimal? Price
        {
            get { return (priceSpecified ? price : default); }
            set
            {
                priceSpecified = value.HasValue;
                if (value.HasValue)
                {
                    price = value.Value;
                }
            }
        }

        public decimal? Qty
        {
            get { return (qtySpecified ? qty : default); }
            set
            {
                qtySpecified = value.HasValue;
                if (value.HasValue)
                {
                    qty = value.Value;
                }
            }
        }

        public string SideId
        {
            get { return sideId; }
            set { sideId = value; }
        }

        public MarketSideIndicator SideInd
        {
            get { return sideInd; }
            set { sideInd = value; }
        }

        public TradeStatus? Status
        {
            get { return (statusSpecified ? status : default(TradeStatus?)); }
            set
            {
                statusSpecified = value.HasValue;
                if (value.HasValue)
                {
                    status = value.Value;
                }
            }
        }

        /// <summary>
        /// TradeDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime TradeDt
        {
            get { return (tradeDtSpecified ? tradeDt : default); }
            set
            {
                tradeDt = value;
                tradeDtSpecified = (value != default);
            }
        }

        public string TradeId
        {
            get { return tradeId; }
            set { tradeId = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class SingleSidedTrade : TradeBase
    {
        #region Accessors
        public TradeSide Side
        {
            get { return side; }
            set { side = value; }
        }

        public string VenueTradeId
        {
            get { return venueTradeId; }
            set { venueTradeId = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class IrsSingleSidedTrade : SingleSidedTrade
    {
        #region Accessors
        /// <summary>
        /// EndDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime EndDt
        {
            get { return (endDtSpecified ? endDt : default); }
            set
            {
                endDt = value;
                endDtSpecified = (value != default);
            }
        }

        /// <summary>
        /// ExpirationDt n'est pas sérialisé en Json lorsque sa valeur est la valeur par défaut du type DateTime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public System.DateTime ExpirationDt
        {
            get { return (expirationDtSpecified ? expirationDt : default); }
            set
            {
                expirationDt = value;
                expirationDtSpecified = (value != default);
            }
        }

        public BusinessCenter[] ExpirationDtBusinessCenters
        {
            get { return expirationDtBusinessCenters; }
            set { expirationDtBusinessCenters = value; }
        }

        public BusinessDateConvention? ExpirationDtConvention
        {
            get { return (expirationDtConventionSpecified ? expirationDtConvention : default(BusinessDateConvention?)); }
            set
            {
                expirationDtConventionSpecified = value.HasValue;
                if (value.HasValue)
                {
                    expirationDtConvention = value.Value;
                }
            }
        }

        public IrsTradeLeg Leg1
        {
            get { return leg1; }
            set { leg1 = value; }
        }

        public IrsTradeLeg Leg2
        {
            get { return leg2; }
            set { leg2 = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class RiskPortfolioBase
    {
        #region Accessors
        public IrsSingleSidedTrade[] IrsTrades
        {
            get { return irsTrades; }
            set { irsTrades = value; }
        }

        public RiskPosition[] Positions
        {
            get { return positions; }
            set { positions = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class RiskPortfolio : RiskPortfolioBase
    {
        #region Accessors
        public Currency? Currency
        {
            get { return (currencySpecified ? currency : default(Currency?)); }
            set
            {
                if (value.HasValue)
                {
                    currency = value.Value;
                    currencySpecified = true;
                }
                else
                {
                    currencySpecified = false;
                }
            }
        }

        public CustomerAccountType? CustomerAccountType
        {
            get { return (customerAccountTypeSpecified ? customerAccountType : default(CustomerAccountType?)); }
            set
            {
                customerAccountTypeSpecified = value.HasValue;
                if (value.HasValue)
                {
                    customerAccountType = value.Value;
                }
            }
        }

        public RiskPortfolioEntities Entities
        {
            get { return entities; }
            set { entities = value; }
        }

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Memo
        {
            get { return memo; }
            set { memo = value; }
        }

        public YesNoIndicator? OmnibusInd
        {
            get { return (omnibusIndSpecified ? omnibusInd : default(YesNoIndicator?)); }
            set
            {
                omnibusIndSpecified = value.HasValue;
                if (value.HasValue)
                {
                    omnibusInd = value.Value;
                }
            }
        }

        public string ParentPortfolioId
        {
            get { return parentPortfolioId; }
            set { parentPortfolioId = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class RequestBase
    {
        #region Accessors
        public RequestHeader Header
        {
            get { return header; }
            set { header = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarginDetailResponseMessage : ResponseBase
    {
        #region Accessors
        public MarginDetailResponse Payload
        {
            get { return payload; }
            set { payload = value; }
        }
        #endregion Accessors
    }
}

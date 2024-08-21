#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient.LoggerService;
using EfsML.Enum;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Data;
#endregion Using Directives

namespace EfsML.Business
{
    #region SQL_Quote
    /// <summary>
    /// Classe de recherche d'une cotation
    /// </summary>
    public class SQL_Quote : SQL_Table
    {
        /// <summary>
        /// Définit le comportement appliqué lors de la recherche d'un prix  OfficialSettlement
        /// </summary>
        public enum OfficialSettlementBehaviorEnum
        {
            /// <summary>
            /// Recherche des cotations OfficialSettlement uniquement
            /// </summary>
            OfficialSettlementOnly,
            /// <summary>
            /// Recherche des cotations OfficialSettlement et OfficialClose
            /// </summary>
            OfficialAll
        }

        #region Members
        /// <summary>
        /// Type de prix demandé (Enabled,Disabled)
        /// </summary>
        protected AvailabilityEnum m_AvailabilityIN;
        /// <summary>
        /// id (OTCmlId) unique de la cotation demandée
        /// <para>Doit être alimenté lorsque la recherche de quotation s'effectue via son id (OTCmlId)</para>
        /// </summary>
        protected int m_IdQuoteIN;
        /// <summary>
        /// id (OTCmlId) unique de l'asset demandé
        /// <para>Peut être alimenté pour identifier l'asset sur lequel la recherche de quotation s'effectue</para>
        /// </summary>
        protected string m_Asset_IdentifierIN;
        /// <summary>
        /// identifiant unique de l'asset demandé
        /// <para>Peut être alimenté pour identifier l'asset sur lequel la recherche de quotation s'effectue</para>
        /// </summary>
        protected int m_IdAssetIN;
        /// <summary>
        /// Object qui permet d'identifier l'asset demandé
        /// <para>Peut être alimenté pour identifier l'asset sur lequel la recherche de quotation s'effectue</para>
        /// </summary>
        protected IKeyAsset m_KeyAssetIN;
        /// <summary>
        /// Clef d'accès à la quotation demandée
        /// </summary>
        protected KeyQuote m_KeyQuoteIN;
        /// <summary>
        /// Représente le type d'asset demandé
        /// </summary>
        protected QuoteEnum m_QuoteObjIN;
        /// <summary>
        /// Représente le comportement appliqué lors d'une recherche de cotation OfficialSettlement
        /// <para>null est identique à OfficialSettlementOnly</para>
        /// </summary>
        protected Nullable<SQL_Quote.OfficialSettlementBehaviorEnum> m_OfficialSettlementBehavior;

        // AL 20240725 [WI1008] Added 
        /// <summary>
        ///  If this flag is true and the quotation for the current business date is missing, set SystemMsgInfo.processState.Status = Warning instead of Error.
        ///  Default value is false
        /// </summary>
        protected bool m_IsBusinessDateMissingWarning;
        /// <summary>
        /// Obtient true si la date de cotation demandée a été ajustée 
        /// <para>Si true la date utilisée pour la recherche de prix est différente de celle demandée</para>
        /// </summary>
        protected bool m_IsAdjustedTime;
        /// <summary>
        /// Représente la date de cotation retenue après ajustement en fonction des businessCenters associé à l'asset
        /// <para>Représente la date utilisée pour la recherche</para>
        /// </summary>
        protected DateTime m_AdjustedTime;
        //
        private Cst.ErrLevel m_QuoteValueCodeReturn;
        private string m_QuoteValueMessage;
        private decimal m_QuoteValue;
        //
        private bool isInProgress;
        private readonly IProductBase m_ProductBase;
        
        //
        private int m_Market_IDM;
        private string m_Market_ISO10383_ALPHA4;
        private string m_Market_IDBC;
        #endregion Members

        #region Accessors
        // AL 20240725 [WI1008] Added
        public bool IsBusinessDateMissingWarning {
            get { return m_IsBusinessDateMissingWarning; } 
            set { m_IsBusinessDateMissingWarning = value; }
        }

        /// <summary>
        /// Obtient un message d'information lorsque la cotation n'existe pas
        /// <para>Permet l'alimentation du log</para>
        /// <para>Obtient null lorsque la cotation existe</para>
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1065]
        public SystemMSGInfo SystemMsgInfo
        {
            get
            {
                SystemMSGInfo systemMsgInfo = null;
                Cst.UnderlyingAsset assetCategory = Cst.UnderlyingAsset.ExchangeTradedContract;
                if (m_QuoteValueCodeReturn != Cst.ErrLevel.SUCCESS)
                {
                    string[] datas = new string[5];

                    // identifiant de l'erreur
                    // PM 20200102 [XXXXX] New Log
                    //string identifier = string.Empty;
                    SysMsgCode sysMsgIdentifier;
                    switch (m_QuoteValueCodeReturn)
                    {
                        case Cst.ErrLevel.QUOTEDISABLED:
                            //identifier = (null != KeyAssetIN) ? "SYS-00104" : "SYS-00101";
                            sysMsgIdentifier = (null != KeyAssetIN) ? new SysMsgCode(SysCodeEnum.SYS, 104) : new SysMsgCode(SysCodeEnum.SYS, 101);
                            break;
                        case Cst.ErrLevel.MULTIQUOTEFOUND:
                            //identifier = (null != KeyAssetIN) ? "SYS-00105" : "SYS-00102";
                            sysMsgIdentifier = (null != KeyAssetIN) ? new SysMsgCode(SysCodeEnum.SYS, 105) : new SysMsgCode(SysCodeEnum.SYS, 102);
                            break;
                        case Cst.ErrLevel.QUOTENOTFOUND:
                        default:
                            //identifier = (null != KeyAssetIN) ? "SYS-00103" : "SYS-00100";
                            sysMsgIdentifier = (null != KeyAssetIN) ? new SysMsgCode(SysCodeEnum.SYS, 103) : new SysMsgCode(SysCodeEnum.SYS, 100);
                            break;
                    }

                    // Asset Identifier
                    if (0 < IdAssetIN)
                    {
                        SQL_AssetBase asset;
                        switch (QuoteObjIN)
                        {
                            case QuoteEnum.DEBTSECURITY:
                                asset = new SQL_AssetDebtSecurity(CS, IdAssetIN);
                                assetCategory = Cst.UnderlyingAsset.Bond;
                                break;
                            case QuoteEnum.EQUITY:
                                asset = new SQL_AssetEquity(CS, IdAssetIN);
                                assetCategory = Cst.UnderlyingAsset.EquityAsset;
                                break;
                            case QuoteEnum.ETD:
                                asset = new SQL_AssetETD(CS, IdAssetIN);
                                assetCategory = Cst.UnderlyingAsset.ExchangeTradedContract;
                                break;
                            case QuoteEnum.EXCHANGETRADEDFUND:
                                asset = new SQL_AssetExchangeTradedFund(CS, IdAssetIN);
                                assetCategory = Cst.UnderlyingAsset.ExchangeTradedFund;
                                break;
                            case QuoteEnum.FXRATE:
                                asset = new SQL_AssetFxRate(CS, IdAssetIN);
                                assetCategory = Cst.UnderlyingAsset.FxRateAsset;
                                break;
                            case QuoteEnum.INDEX:
                                asset = new SQL_AssetIndex(CS, IdAssetIN);
                                assetCategory = Cst.UnderlyingAsset.Index;
                                break;
                            case QuoteEnum.RATEINDEX:
                                asset = new SQL_AssetRateIndex(CS, SQL_AssetRateIndex.IDType.IDASSET, IdAssetIN);
                                assetCategory = Cst.UnderlyingAsset.RateIndex;
                                break;
                            case QuoteEnum.SIMPLEIRS:
                                asset = new SQL_AssetSimpleIRSwap(CS, IdAssetIN);
                                assetCategory = Cst.UnderlyingAsset.SimpleIRSwap;
                                break;
                            default:
                                throw new InvalidOperationException(StrFunc.AppendFormat("Quote {0} is not managed, please contact EFS", QuoteObjIN.ToString()));
                        }
                        asset.DbTransaction = DbTransaction;
                        if (asset.IsLoaded)
                            datas[0] = asset.Identifier + " [id:" + IdAssetIN.ToString() + "]";
                    }
                    else if (null != KeyAssetIN)
                    {
                        switch (QuoteObjIN)
                        {
                            case QuoteEnum.FXRATE:
                                KeyAssetFxRate keyAssetFxRate = (KeyAssetFxRate)KeyAssetIN;
                                if (keyAssetFxRate.QuoteBasis == QuoteBasisEnum.Currency1PerCurrency2)
                                    datas[0] = keyAssetFxRate.IdC2 + "./" + keyAssetFxRate.IdC1;
                                else
                                    datas[0] = keyAssetFxRate.IdC1 + "./" + keyAssetFxRate.IdC2;
                                datas[0] += " [" + keyAssetFxRate.QuoteBasis.ToString() + "]";
                                assetCategory = Cst.UnderlyingAsset.FxRateAsset;
                                break;
                            case QuoteEnum.DEBTSECURITY:
                            case QuoteEnum.EQUITY:
                            case QuoteEnum.ETD:
                            case QuoteEnum.EXCHANGETRADEDFUND:
                            case QuoteEnum.INDEX:
                            case QuoteEnum.RATEINDEX:
                            case QuoteEnum.SIMPLEIRS:
                                break;
                            default:
                                throw new InvalidOperationException(StrFunc.AppendFormat("Quote {0} is not managed, please contact EFS", QuoteObjIN.ToString()));
                        }
                    }

                    datas[0] += " [" + assetCategory.ToString() + "]";
                    datas[1] = GetQuotationSideRequest().ToString();
                    datas[2] = GetQuotationTimingRequest().ToString();
                    // EG 20140930 Test m_IsAdjustedTime
                    //datas[3] = DtFunc.DateTimeToStringISO(m_KeyQuoteIN.Time);
                    if (m_IsAdjustedTime)
                        datas[3] = DtFunc.DateTimeToStringISO(m_AdjustedTime) + " (Holiday: " + DtFunc.DateTimeToStringISO(m_KeyQuoteIN.Time) + ")";
                    else
                        datas[3] = DtFunc.DateTimeToStringISO(m_KeyQuoteIN.Time);
                    datas[4] = (StrFunc.IsFilled(m_KeyQuoteIN.IdMarketEnv) ? m_KeyQuoteIN.IdMarketEnv.ToString() : "{null}") + " - " +
                               (StrFunc.IsFilled(m_KeyQuoteIN.IdValScenario) ? m_KeyQuoteIN.IdValScenario.ToString() : "{null}");

                    // AL 20240725 [WI1008] If m_IsBusinessDateMissingWarning is true and Quotation is missing for current business date, set Warning
                    DateTime businessDate = OTCmlHelper.GetDateBusiness(CS, DbTransaction);
                    ProcessStateTools.StatusEnum status = 
                        (m_KeyQuoteIN.Time.Date < businessDate) ? ProcessStateTools.StatusErrorEnum : 
                        (m_KeyQuoteIN.Time.Date == businessDate && m_QuoteValueCodeReturn == Cst.ErrLevel.QUOTENOTFOUND && m_IsBusinessDateMissingWarning) ? ProcessStateTools.StatusWarningEnum :
                        (m_KeyQuoteIN.Time.Date > businessDate) ? ProcessStateTools.StatusWarningEnum :
                        ProcessStateTools.StatusErrorEnum;
                    
                    ProcessState processState = new ProcessState(status, m_QuoteValueCodeReturn);
                    systemMsgInfo = new SystemMSGInfo(sysMsgIdentifier, processState, datas);
                }
                return systemMsgInfo;
            }
        }

        /// <summary>
        /// Obtient ou définit l'Identifier de l'asset demandé 
        /// </summary>
        public string Asset_IdentifierIN
        {
            get { return m_Asset_IdentifierIN; }
            set
            {
                if (m_Asset_IdentifierIN != value)
                {
                    InitProperty();
                    m_Asset_IdentifierIN = value;
                    IdAssetIN = 0;
                }
            }
        }

        /// <summary>
        /// Obtient ou définit l'Id de l'asset demandé
        /// </summary>
        public int IdAssetIN
        {
            get { return m_IdAssetIN; }
            set
            {
                if (m_IdAssetIN != value)
                {
                    InitProperty();
                    m_IdAssetIN = value;
                }
            }
        }

        /// <summary>
        /// Obtient ou définit le type de prix demandé (Enabled,Disabled)
        /// </summary>
        public AvailabilityEnum AvailabilityIN
        {
            get { return m_AvailabilityIN; }
            set
            {
                if (m_AvailabilityIN != value)
                {
                    InitProperty();
                    m_AvailabilityIN = value;
                }
            }
        }

        /// <summary>
        /// Obtient ou définit l'id de la cotation demandée
        /// </summary>
        public int IdQuoteIN
        {
            get { return m_IdQuoteIN; }
            set
            {
                if (m_IdQuoteIN != value)
                {
                    InitProperty();
                    m_IdQuoteIN = value;
                }
            }
        }

        /// <summary>
        /// Obtient ou définit l'asset demandé
        /// </summary>
        public IKeyAsset KeyAssetIN
        {
            get { return m_KeyAssetIN; }
            set
            {
                if (m_KeyAssetIN != value)
                {
                    InitProperty();
                    m_KeyAssetIN = value;

                    IdAssetIN = 0;
                    Asset_IdentifierIN = null;
                }
            }
        }

        /// <summary>
        /// Obtient ou définit la clef d'accès demandée
        /// </summary>
        public KeyQuote KeyQuoteIN
        {
            get { return m_KeyQuoteIN; }
            set
            {
                if (m_KeyQuoteIN != value)
                {
                    InitProperty();
                    m_KeyQuoteIN = value;

                    IdQuoteIN = 0;
                }
            }
        }

        /// <summary>
        /// Obtient ou définit le typologie de l'asset demandée
        /// </summary>
        public QuoteEnum QuoteObjIN
        {
            get { return m_QuoteObjIN; }
            set
            {
                if (m_QuoteObjIN != value)
                {
                    InitProperty();
                    m_QuoteObjIN = value;
                }
            }
        }

        /// <summary>
        /// Représente le comportement appliqué lors d'une recherche de cotation OfficialSettlement
        /// <para>null est identique à OfficialSettlementOnly</para>
        /// </summary>
        public Nullable<SQL_Quote.OfficialSettlementBehaviorEnum> OfficialSettlementBehavior
        {
            get { return m_OfficialSettlementBehavior; }
            set
            {
                if (m_OfficialSettlementBehavior != value)
                {
                    InitProperty();
                    m_OfficialSettlementBehavior = value;
                }
            }
        }

        /// <summary>
        /// Obtient True si la recherche retourne au minimum 1 ligne
        /// <para>Alimente la cotation [property QuoteValue]</para>
        /// <para>Alimente le message de retour [property QuoteValueMessage]</para>
        /// <para>Alimente la code retour [property QuoteValueCodeReturn]</para>
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1065]
        public override bool IsLoaded
        {
            get
            {
                bool isLoaded = base.IsLoaded;
                //
                if (!isInProgress)
                {
                    isInProgress = true;
                    switch (QuoteObjIN)
                    {
                        case QuoteEnum.EQUITY:
                        case QuoteEnum.ETD:
                        case QuoteEnum.EXCHANGETRADEDFUND:
                            m_QuoteValueMessage = "Price: ";
                            break;
                        case QuoteEnum.FXRATE:
                            m_QuoteValueMessage = "Fixing: ";
                            break;
                        case QuoteEnum.RATEINDEX:
                            m_QuoteValueMessage = "Rate: ";
                            break;
                        case QuoteEnum.DEBTSECURITY:
                        case QuoteEnum.INDEX:
                        case QuoteEnum.SIMPLEIRS:
                            m_QuoteValueMessage = "Quote: ";
                            break;
                        default:
                            throw new InvalidOperationException(StrFunc.AppendFormat("Quote {0} is not managed, please contact EFS", QuoteObjIN.ToString()));
                    }
                    //
                    if (isLoaded)
                    {
                        if (m_QuoteValueCodeReturn == Cst.ErrLevel.UNDEFINED)
                        {
                            Nullable<decimal> tmpQuoteValue = GetQuoteValue2();
                            if (tmpQuoteValue != null)
                            {
                                m_QuoteValue = (decimal)tmpQuoteValue;
                                m_QuoteValueMessage = "Found value";
                                m_QuoteValueCodeReturn = Cst.ErrLevel.SUCCESS;
                            }
                            else
                            {
                                int totalRows = this.Rows.Count;
                                int enabledRows = 0;
                                int disabledRows = 0;
                                foreach (DataRow quoteRow in this.Rows)
                                {
                                    if (BoolFunc.IsTrue(quoteRow["ISENABLED"]))
                                        enabledRows++;
                                    else
                                        disabledRows++;
                                }
                                //
                                if (disabledRows == totalRows)
                                {
                                    //All values are disabled
                                    m_QuoteValueMessage += "Disabled value";
                                    //m_QuoteValueCodeReturn = Cst.ErrLevel.DATADISABLED;
                                    m_QuoteValueCodeReturn = Cst.ErrLevel.QUOTEDISABLED;
                                }
                                else if (enabledRows == 1)
                                {
                                    //Only on value is enabled --> Incorrect Side (cf GetQuoteValue())
                                    m_QuoteValueMessage += "Side value, not found";
                                    //m_QuoteValueCodeReturn = Cst.ErrLevel.DATANOTFOUND;
                                    m_QuoteValueCodeReturn = Cst.ErrLevel.QUOTENOTFOUND;
                                }
                                else 
                                {
                                    m_QuoteValueMessage += "Too many found enabled values";
                                    //m_QuoteValueCodeReturn = Cst.ErrLevel.MULTIDATAFOUND;
                                    m_QuoteValueCodeReturn = Cst.ErrLevel.MULTIQUOTEFOUND;
                                }
                            }
                        }
                    }
                    else
                    {
                        m_QuoteValueMessage += "Value not found";
                        //m_QuoteValueCodeReturn = Cst.ErrLevel.DATANOTFOUND;
                        m_QuoteValueCodeReturn = Cst.ErrLevel.QUOTENOTFOUND;
                    }
                    //
                    isInProgress = false;
                }
                return isLoaded;
            }
        }

        /// <summary>
        /// Obtient true si la recherche retourne au minimum 1 ligne
        /// <para>Un prix est Enabled si l'on trouve un QuoteSide enabled égal au QuoteSide demandé</para>
        /// <para>Un prix est Enabled si l'on trouve un QuoteSide enabled autre que celui demandé avec un spreadValue renseigné</para>
        /// </summary>
        /// <remarks>
        ///  Equivaut à : public override bool IsEnabled
        ///  Sauf qu'ici on écrase completement la propriété mère car équivalence (pas de DTENABLED/DTDISABLED)
        /// </remarks>
        public new bool IsEnabled
        {
            get { return (0 <= IndexRowSelectedItem); }
        }

        /// <summary>
        /// Obtient la date de cotation retenue après ajustement (prise en compte des business Centers de l'asset) 
        /// </summary>
        public DateTime AdjustedTime
        {
            get { return m_AdjustedTime; }
        }

        /// <summary>
        /// Obtient true si la date de cotation demandée a été ajustée
        /// </summary>
        public bool IsAdjustedTime
        {
            get { return m_IsAdjustedTime; }
        }

        /// <summary>
        /// Obtient ASSETMEASURE  de la cotation retenue 
        /// </summary>
        public string AssetMeasure
        {
            get { return Convert.ToString(GetColumnValue("ASSETMEASURE")); }
        }

        /// <summary>
        /// Obtient EXPIRITYTIME de la cotation retenue
        /// </summary>
        public DateTime ExpirityTime
        {
            get { return Convert.ToDateTime(GetColumnValue("EXPIRITYTIME")); }
        }

        /// <summary>
        /// Obtient l'id asset de la cotation retenue
        /// </summary>
        public int IdAsset
        {
            get { return Convert.ToInt32(GetColumnValue("IDASSET")); }
        }

        /// <summary>
        /// Obtient le businessCenter de la cotation retenue
        /// </summary>
        public string IdBC
        {
            get { return Convert.ToString(GetColumnValue("IDBC")); }
        }

        /// <summary>
        /// Obtient l'IdMarketEnv de la cotation retenue
        /// </summary>
        public string IdMarketEnv
        {
            get { return Convert.ToString(GetColumnValue("IDMARKETENV")); }
        }

        /// <summary>
        /// Obtient l'id (OTCmlId) de la cotation retenue (colonne IDQUOTE_H)
        /// </summary>
        public int IdQuote
        {
            get
            {
                return Convert.ToInt32(GetColumnValue("IDQUOTE_H"));
            }
        }

        /// <summary>
        /// Obtient le scenario de valorisation de la cotation retenue
        /// </summary>
        public string IdValScenario
        {
            get { return Convert.ToString(GetColumnValue("IDVALSCENARIO")); }
        }

        /// <summary>
        /// Obtient true si la date de cotation demandée (avant ajustement) est identique au Time de la cotation
        /// </summary>
        public bool IsFoundTime
        {
            get { return (KeyQuoteIN.Time == Time); }
        }

        /// <summary>
        /// Obtient la source du prix (Bid,Mid,Ask,OfficialClose,OfficialSettlement)  de la cotation retenue     
        /// </summary>
        public string QuoteSide
        {
            get { return Convert.ToString(GetColumnValue("QUOTESIDE")); }
        }

        /// <summary>
        /// Obtient le quoteTiming de la cotation retenue
        /// </summary>
        public string QuoteTiming
        {
            get { return Convert.ToString(GetColumnValue("QUOTETIMING")); }
        }

        /// <summary>
        /// Obtient l'unité de la cotation retenue
        /// </summary>
        public string QuoteUnit
        {
            get { return Convert.ToString(GetColumnValue("QUOTEUNIT")); }
        }

        /// <summary>
        /// Obtient le fournisseur source de la cotation retenue
        /// </summary>
        public string QuoteSource
        {
            get { return Convert.ToString(GetColumnValue("SOURCE")); }
        }

        /// <summary>
        /// Obtient le spread de la cotation retenue 
        /// </summary>
        public decimal SpreadValue
        {
            get { return Convert.ToDecimal(GetColumnValue("SPREADVALUE")); }
        }

        /// <summary>
        /// Obtient CashFlowType de la cotation retenue
        /// </summary>
        public string CashFlowType
        {
            get { return Convert.ToString(GetColumnValue("CASHFLOWTYPE")); }
        }

        /// <summary>
        /// Obtient le Time de la cotation retenue
        /// </summary>
        public DateTime Time
        {
            get { return Convert.ToDateTime(GetColumnValue("TIME")); }
        }

        /// <summary>
        /// Obtient le Delta de la cotation retenue
        /// </summary>
        public Nullable<decimal> Delta
        {
            get { return Convert.ToDecimal(GetColumnValue("DELTA")); }
        }

        /// <summary>
        /// Obtient la cotation
        /// </summary>
        public decimal QuoteValue
        {
            get { return m_QuoteValue; }
        }

        /// <summary>
        ///Obtient le message retour généré durant la recherche de cotation
        /// </summary>
        public string QuoteValueMessage
        {
            get { return m_QuoteValueMessage; }
        }

        /// <summary>
        /// Obtient le code retour généré durant la recherche de cotation
        /// </summary>
        public Cst.ErrLevel QuoteValueCodeReturn
        {
            get { return m_QuoteValueCodeReturn; }
        }

        /// <summary>
        /// Obtient l'index de ligne retenue lorsque la recherche de la cotation retourne n lignes
        /// <para>Obtient -1 si aucune ligne</para>
        /// </summary>
        /// FI 20110705 Spheres® retourne le prix Officialclose si le prix OfficialSettlement n'existe pas
        private int IndexRowSelectedItem
        {
            get
            {
                int ret = -1;
                //
                QuotationSideEnum quoteSideRequest = GetQuotationSideRequest();
                //
                //Attention la facon deont les lignes sont ordonnées est ultraimportante
                //Voir le SetSQLOrderBy()
                for (int i = 0; i < RowsCount; i++)
                {
                    DataRow row = Rows[i];
                    bool isEnabled = Convert.ToBoolean(row["ISENABLED"]);
                    bool isSpreadReturnSpecified = !Convert.IsDBNull(row["SPREADVALUE"]);
                    QuotationSideEnum quoteSourceReturn = GetRowQuoteSide(row);
                    //
                    if (isEnabled)
                    {
                        if (quoteSideRequest == quoteSourceReturn)
                        {
                            ret = i;
                        }
                        else if (quoteSideRequest == QuotationSideEnum.Mid ||
                                 quoteSideRequest == QuotationSideEnum.Ask ||
                                 quoteSideRequest == QuotationSideEnum.Bid)
                        {
                            if (quoteSourceReturn != QuotationSideEnum.OfficialClose)
                            {
                                if (isSpreadReturnSpecified)
                                {
                                    if ((QuotationSideEnum.Ask == quoteSideRequest) &&
                                            ((QuotationSideEnum.Mid == quoteSourceReturn) ||
                                            (QuotationSideEnum.Bid == quoteSourceReturn)))
                                        ret = i;
                                    else if ((QuotationSideEnum.Bid == quoteSideRequest) &&
                                                ((QuotationSideEnum.Mid == quoteSourceReturn) ||
                                                (QuotationSideEnum.Ask == quoteSourceReturn)))
                                        ret = i;
                                    else if ((QuotationSideEnum.Mid == quoteSideRequest) &&
                                                ((QuotationSideEnum.Ask == quoteSourceReturn) ||
                                                (QuotationSideEnum.Bid == quoteSourceReturn)))
                                        ret = i;
                                }
                            }
                            else
                            {
                                //Spheres® rentre ici lors de la recherche d'un prix Bid/Mid/Ask 
                                //s'il n'existe pas de prix  Bid/Mid/Ask et il existe un prix close
                                //Spheres® considère le prix close sans exploitation du spread
                                ret = i;
                            }
                        }
                        else if ((quoteSideRequest == QuotationSideEnum.OfficialSettlement) && (OfficialSettlementBehavior == OfficialSettlementBehaviorEnum.OfficialAll))
                        {
                            //FI 20110705 Spheres® retourne le prix Officialclose si le prix OfficialSettlement n'existe pas
                            if (quoteSourceReturn == QuotationSideEnum.OfficialClose)
                                ret = i;
                        }
                        if (ret > -1)
                            break;
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient le contract multiplier d'un ETD
        /// </summary>
        public Nullable<decimal> ContractMultiplier
        {
            get { return Convert.ToDecimal(GetColumnValue("CONTRACTMULTIPLIER")); }
        }

        /// <summary>
        /// Obtient l'IDM du marché (uniquement pour l'instant si la recherche a donné lie à ajustement de la date)
        /// </summary>
        public int Market_IDM
        {
            get { return m_Market_IDM; }
        }
        /// <summary>
        /// Obtient l'ISO10383_ALPHA4 du marché (uniquement pour l'instant si la recherche a donné lie à ajustement de la date)
        /// </summary>
        public string Market_ISO10383_ALPHA4
        {
            get { return m_Market_ISO10383_ALPHA4; }
        }
        /// <summary>
        /// Obtient l'IDBC du marché (uniquement pour l'instant si la recherche a donné lie à ajustement de la date)
        /// </summary>
        public string Market_IDBC
        {
            get { return m_Market_IDBC; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Basic Initialize 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQuote"></param>
        /// <param name="pAvailability"></param>
        /// <param name="pProductBase"></param>
        public SQL_Quote(string pCS, QuoteEnum pQuote, AvailabilityEnum pAvailability, IProductBase pProductBase)
            : base(pCS, Cst.OTCml_TBL.QUOTE_x_H.ToString().Replace("x", pQuote == QuoteEnum.DEBTSECURITY ? "DEBTSEC" : (pQuote == QuoteEnum.EXCHANGETRADEDFUND ? "EXTRDFUND" : pQuote.ToString())))
        {
            QuoteObjIN = pQuote;
            AvailabilityIN = pAvailability;
            m_ProductBase = pProductBase;
            m_QuoteValueCodeReturn = Cst.ErrLevel.UNDEFINED;
            m_OfficialSettlementBehavior = null;
        }
        
        /// <summary>
        /// Initialize for scan with IdQuote (ie: 1)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQuote"></param>
        /// <param name="pAvailability"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pIdQuote"></param>
        public SQL_Quote(string pCS, QuoteEnum pQuote, AvailabilityEnum pAvailability, IProductBase pProductBase, int pIdQuote)
            : this(pCS, pQuote, pAvailability, pProductBase)
        {
            IdQuoteIN = pIdQuote;
            m_ProductBase = pProductBase;
            m_QuoteValueCodeReturn = Cst.ErrLevel.UNDEFINED;
            m_OfficialSettlementBehavior = null;
        }
        
        /// <summary>
        /// Initialize for scan with KeyQuote (ie: Time, TimeOperator, [IdMarketEnv], [IdValScenario], [QuoteSide], ...) et IdAsset (ie: 1)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQuote"></param>
        /// <param name="pAvailability"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pKeyQuote"></param>
        /// <param name="pIdAsset"></param>
        public SQL_Quote(string pCS, QuoteEnum pQuote, AvailabilityEnum pAvailability, IProductBase pProductBase, KeyQuote pKeyQuote, int pIdAsset)
            : this(pCS, pQuote, pAvailability, pProductBase)
        {
            KeyQuoteIN = pKeyQuote;
            IdAssetIN = pIdAsset;
            m_ProductBase = pProductBase;
            m_QuoteValueCodeReturn = Cst.ErrLevel.UNDEFINED;
            m_OfficialSettlementBehavior = null;
        }
        
        /// <summary>
        ///  Initialize for scan with KeyQuote (ie: Time, TimeOperator, [IdMarketEnv], [IdValScenario], [QuoteSide], ...) et Asset_Identifier (ie: USD/EUR ECB, GBP/EUR ECB, ...)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQuote"></param>
        /// <param name="pAvailability"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pKeyQuote"></param>
        /// <param name="pAsset_Identifier"></param>
        public SQL_Quote(string pCS, QuoteEnum pQuote, AvailabilityEnum pAvailability, IProductBase pProductBase, KeyQuote pKeyQuote, string pAsset_Identifier)
            : this(pCS, pQuote, pAvailability, pProductBase)
        {
            KeyQuoteIN = pKeyQuote;
            Asset_IdentifierIN = pAsset_Identifier;
            m_ProductBase = pProductBase;
            m_QuoteValueCodeReturn = Cst.ErrLevel.UNDEFINED;
            m_OfficialSettlementBehavior = null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQuote"></param>
        /// <param name="pAvailability"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pKeyQuote"></param>
        /// <param name="pKeyAsset"></param>
        public SQL_Quote(string pCS, QuoteEnum pQuote, AvailabilityEnum pAvailability, IProductBase pProductBase, KeyQuote pKeyQuote, IKeyAsset pKeyAsset)
            : this(pCS, pQuote, pAvailability, pProductBase)
        {
            KeyQuoteIN = pKeyQuote;
            KeyAssetIN = pKeyAsset;
            m_ProductBase = pProductBase;
            m_QuoteValueCodeReturn = Cst.ErrLevel.UNDEFINED;
            m_OfficialSettlementBehavior = null;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Ajustement de Time (DateTime), dans le cas ou le Time (Date) demandée est fériée de type {pDayType}
        /// </summary>
        /// <param name="pIdBC"></param>
        /// <param name="pIdBCAdd"></param>
        /// <param name="pDayType"></param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180411 [23769] Set & Use dbTransaction
        private void CalcAdjustedTime(string pIdBC, string pIdBCAdd, DayTypeEnum pDayType)
        {

            DateTime datetime = KeyQuoteIN.Time;
            //BCs
            IBusinessDayAdjustments bda = m_ProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.PRECEDING, pIdBC, pIdBCAdd);
            //Sauvegarde et Suppression de l'heure pour le contrôle Holiday
            TimeSpan tsTime = datetime.TimeOfDay;
            DateTime dtTime = datetime.Date;
            EFS_AdjustableDate efs_AdjustableDate = new EFS_AdjustableDate(CS, DbTransaction, dtTime, bda, pDayType, null);
            //
            m_AdjustedTime = efs_AdjustableDate.adjustedDate.DateValue;
            //Rajout de l'heure supprimer précédemment pour le contrôle Holiday
            m_AdjustedTime = m_AdjustedTime.Add(tsTime);
            //
            //Utilisation de la date ajustée si Holiday
            if (m_AdjustedTime != datetime)
                m_IsAdjustedTime = true;
        }

        /// <summary>
        /// Retourne la valeur de la colonne {pColumnName} pour la quotation obtenue
        /// <para>Retourne null si aucune quotation n'a été obtenue </para>
        /// </summary>
        /// <param name="pColumnName">Nom de la colonne</param>
        /// <returns></returns>
        public object GetColumnValue(string pColumnName)
        {
            int rowItem = IndexRowSelectedItem;
            //
            if (0 <= rowItem)
                return base.GetColumnValue(rowItem, pColumnName);
            //
            return null;
        }

        

        /// <summary>
        /// Return an existing quote value or a calculated quote value from a spreadValue and an existing quote value 
        /// </summary>
        /// <returns></returns>
        /// FI 20110624 [17490] ajout de GetQuoteValue2, cette fonction est désormais utilisée à la place de GetQuoteValue
        private Nullable<decimal> GetQuoteValue2()
        {
            Nullable<decimal> ret = null;
            //
            int i = IndexRowSelectedItem;
            //
            if (i > -1)
            {
                decimal quoteValue = Convert.ToDecimal(GetColumnValue("VALUE"));
                //
                QuotationSideEnum quoteSideRequest = GetQuotationSideRequest();
                QuotationSideEnum quoteSideResponse = GetRowQuoteSide(Rows[i]);
                //
                if (quoteSideResponse == quoteSideRequest)
                    ret = quoteValue;
                else if ((quoteSideRequest == QuotationSideEnum.Mid ||
                             quoteSideRequest == QuotationSideEnum.Ask ||
                             quoteSideRequest == QuotationSideEnum.Bid))
                {
                    if (quoteSideResponse == QuotationSideEnum.OfficialClose)
                    {
                        ret = quoteValue;
                    }
                    else if (this.SpreadValue >= 0)
                    {
                        switch (quoteSideRequest)
                        {
                            case QuotationSideEnum.Ask:
                                switch (quoteSideResponse)
                                {
                                    case QuotationSideEnum.Bid:
                                        ret = quoteValue + SpreadValue;
                                        break;
                                    case QuotationSideEnum.Mid:
                                        ret = quoteValue + SpreadValue / 2;
                                        break;
                                }
                                break;
                            case QuotationSideEnum.Bid:
                                switch (quoteSideResponse)
                                {
                                    case QuotationSideEnum.Ask:
                                        ret = quoteValue - SpreadValue;
                                        break;
                                    case QuotationSideEnum.Mid:
                                        ret = quoteValue - SpreadValue / 2;
                                        break;
                                }
                                break;
                            case QuotationSideEnum.Mid:
                                switch (quoteSideResponse)
                                {
                                    case QuotationSideEnum.Ask:
                                        ret = quoteValue - SpreadValue / 2;
                                        break;
                                    case QuotationSideEnum.Bid:
                                        ret = quoteValue + SpreadValue / 2;
                                        break;
                                }
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", quoteSideRequest));
                        }
                    }
                }
                else if ((quoteSideRequest == QuotationSideEnum.OfficialSettlement) && (OfficialSettlementBehavior == OfficialSettlementBehaviorEnum.OfficialAll))
                {
                    //FI 20110705 Spheres® retourne le prix Officialclose si le prix OfficialSettlement n'existe pas
                    if (quoteSideResponse == QuotationSideEnum.OfficialClose)
                        ret = quoteValue;
                }
            }
            return ret;
        }

        /// <summary>
        /// Alimente IdAssetIN à partir de Asset_IdentifierIN ou de KeyAssetIN
        /// </summary>
        private void SetIdAssetIN()
        {
            if (StrFunc.IsFilled(Asset_IdentifierIN))
            {
                //Recherche d'un IDASSET à partir de son Identifier
                // EG 20140930 Use SQL_AssetBase
                SQL_AssetBase sql_Asset;
                switch (QuoteObjIN)
                {
                    case QuoteEnum.DEBTSECURITY:
                        sql_Asset = new SQL_AssetDebtSecurity(_csManager.CsSpheres, SQL_TableWithID.IDType.Identifier, Asset_IdentifierIN);
                        break;
                    case QuoteEnum.EQUITY:
                        sql_Asset = new SQL_AssetEquity(_csManager.CsSpheres, SQL_TableWithID.IDType.Identifier, Asset_IdentifierIN, ScanDataDtEnabledEnum.Yes);
                        break;
                    case QuoteEnum.ETD:
                        sql_Asset = new SQL_AssetETD(_csManager.CsSpheres, SQL_TableWithID.IDType.Identifier, Asset_IdentifierIN, ScanDataDtEnabledEnum.Yes);
                        break;
                    case QuoteEnum.EXCHANGETRADEDFUND:
                        sql_Asset = new SQL_AssetExchangeTradedFund(_csManager.CsSpheres, SQL_TableWithID.IDType.Identifier, Asset_IdentifierIN, ScanDataDtEnabledEnum.Yes);
                        break;
                    case QuoteEnum.FXRATE:
                        sql_Asset = new SQL_AssetFxRate(_csManager.CsSpheres, SQL_TableWithID.IDType.Identifier, Asset_IdentifierIN, SQL_Table.ScanDataDtEnabledEnum.Yes);
                        break;
                    case QuoteEnum.RATEINDEX:
                        sql_Asset = new SQL_AssetRateIndex(_csManager.CsSpheres, SQL_AssetRateIndex.IDType.Asset_Identifier, Asset_IdentifierIN, ScanDataDtEnabledEnum.Yes);
                        break;
                    case QuoteEnum.INDEX:
                        sql_Asset = new SQL_AssetIndex(_csManager.CsSpheres, SQL_TableWithID.IDType.Identifier, Asset_IdentifierIN, ScanDataDtEnabledEnum.Yes);
                        break;
                    case QuoteEnum.SIMPLEIRS:
                        sql_Asset = new SQL_AssetSimpleIRSwap(_csManager.CsSpheres, SQL_TableWithID.IDType.Identifier, Asset_IdentifierIN, ScanDataDtEnabledEnum.Yes);
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("Quote {0} is not managed, please contact EFS", QuoteObjIN.ToString()));
                        // EG 20160404 Migration vs2013
                        //break;
                }

                if (null != sql_Asset) 
                {
                    sql_Asset.DbTransaction = DbTransaction;
                    if (sql_Asset.IsLoaded)
                        IdAssetIN = sql_Asset.IdAsset;
                }
            }
            else
            {
                //Recherche d'un IDASSET à partir de caractéristiques
                IdAssetIN = KeyAssetIN.GetIdAsset(_csManager.CsSpheres, DbTransaction);
            }

            //Sur les assets de taux, prise en considération de l'asset de substitution pour lire la quotation
            //ex pour les CMS-1Y (3Mois) on utilisera le CMS-1Y (12 mois)
            if (QuoteEnum.RATEINDEX == QuoteObjIN)
            {
                SQL_AssetRateIndex sql_AssetRateIndex = new SQL_AssetRateIndex(_csManager.CsSpheres, SQL_AssetRateIndex.IDType.IDASSET, IdAssetIN, ScanDataDtEnabledEnum.Yes)
                {
                    DbTransaction = DbTransaction
                };
                if (sql_AssetRateIndex.LoadTable(new string[] { "Asset_IDASSET_ASSET" }))
                {
                    int idAssetSubstitute = sql_AssetRateIndex.Asset_IdAsset_Asset;
                    if (idAssetSubstitute > 0)
                        IdAssetIN = idAssetSubstitute;
                }
            }
        }

        /// <summary>
        /// Alimente IDM, ISO10383_ALPHA4 et IDBC à partir de SQL_AssetBase
        /// </summary>
        private void SetMarketInformation(SQL_AssetBase pSQL_AssetBase)
        {
            m_Market_IDM = pSQL_AssetBase.IdM;
            m_Market_ISO10383_ALPHA4 = pSQL_AssetBase.Market_ISO10383_ALPHA4;
            m_Market_IDBC = pSQL_AssetBase.Market_IDBC;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLOrderBy()
        {
            // 20090610 RD Pour pouvoir relancer le chargement de table ( voir GetQuoteByPivotCurrency())
            SQLOrderBy = string.Empty;
            //
            base.SetSQLOrderBy();
            //
            StrBuilder sqlColumOrderBy = new StrBuilder(string.Empty);
            if (null != KeyQuoteIN)
            {
                sqlColumOrderBy += "TIME" + SQLCst.DESC + ",ISENABLED" + SQLCst.DESC;
                sqlColumOrderBy += StrFunc.AppendFormat(",{0}", GetExpressionOrderByQuoteSide()) + SQLCst.DESC;
                sqlColumOrderBy += StrFunc.AppendFormat(",{0}", GetExpressionOrderByQuoteTiming()) + SQLCst.DESC;
                ConstructOrderBy(sqlColumOrderBy.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20190716 [VCL : New FixedIncome] Use KeyQuoteAdditional value
        protected override void SetSQLWhere()
        {
            // 20090609 RD Ajout de SQL Parameters
            // 20090610 RD 
            // Pour pouvoir relancer le chargement de la table (voir GetQuoteByPivotCurrency())
            Dataparameters.Clear();
            //
            m_IsAdjustedTime = false;
            //
            SQLWhere sqlWhere = new SQLWhere();
            //
            #region Contruct Where
            //
            //ISENABLED 
            if (AvailabilityEnum.Enabled == AvailabilityIN)
                sqlWhere.Append(this.SQLObject + ".ISENABLED = 1");
            else if (AvailabilityEnum.Disabled == AvailabilityIN)
                sqlWhere.Append(this.SQLObject + ".ISENABLED = 0");
            //
            //IDQUOTE_H (Demande d'une cotation précise)
            if (0 != IdQuoteIN)
            {
                //Demande d'une cotation précise
                sqlWhere.Append(this.SQLObject + ".IDQUOTE_H = @IDQUOTE_H");
                Dataparameters.Add(new DataParameter(CS, "IDQUOTE_H", DbType.Int32), IdQuoteIN);
            }
            else if (null != KeyQuoteIN)
            {
                //Demande d'une cotation à partir de la clef d'accès aux prix + ASSET
                //Recherche de l'asset
                if (0 == IdAssetIN)
                    SetIdAssetIN();
                //   
                //IDASSET 
                sqlWhere.Append(this.SQLObject + ".IDASSET = @IDASSET");
                Dataparameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), IdAssetIN);
                //
                //IDMARKETENV
                sqlWhere.Append(this.SQLObject + ".IDMARKETENV = @IDMARKETENV");
                Dataparameters.Add(new DataParameter(CS, "IDMARKETENV", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), KeyQuoteIN.IdMarketEnv);
                //
                //IDVALSCENARIO
                sqlWhere.Append(this.SQLObject + ".IDVALSCENARIO = @IDVALSCENARIO", true);
                Dataparameters.Add(new DataParameter(CS, "IDVALSCENARIO", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), KeyQuoteIN.IdValScenario);
                //
                //QUOTEUNIT 
                if (KeyQuoteIN.QuoteUnit.HasValue)
                {
                    sqlWhere.Append(this.SQLObject + ".QUOTEUNIT = @QUOTEUNIT", true);
                    Dataparameters.Add(new DataParameter(CS, "QUOTEUNIT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), KeyQuoteIN.QuoteUnit.ToString());
                }
                //
                //QUOTESIDE (Bid,Mid,Ask,OfficialClose,OfficialSettlement)
                //FI 20110706 [17490] (add gestion du QuoteSide, avant il n'y avait rien)
                sqlWhere.Append(GetExpressionWhereQuoteSide(), true);
                //
                //QUOTETIMING (Open, Close, Low, ...)
                //FI 20110706 [17490]
                sqlWhere.Append(GetExpressionWhereQuoteTiming(), true);
                //
                if (null != KeyQuoteIN.KeyQuoteAdditional)
                {
                    KeyQuoteAdditional keyQuoteAdd = KeyQuoteIN.KeyQuoteAdditional;
                    //ASSETMEASURE
                    if (keyQuoteAdd.AssetMeasure.HasValue)
                    {
                        sqlWhere.Append(this.SQLObject + ".ASSETMEASURE = @ASSETMEASURE");
                        Dataparameters.Add(new DataParameter(CS, "ASSETMEASURE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), keyQuoteAdd.AssetMeasure.Value);
                    }
                    //
                    //CASHFLOWTYPE
                    if (keyQuoteAdd.CashflowType.HasValue)
                    {
                        sqlWhere.Append(this.SQLObject + ".CASHFLOWTYPE = @CASHFLOWTYPE");
                        Dataparameters.Add(new DataParameter(CS, "CASHFLOWTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), keyQuoteAdd.CashflowType.Value);
                    }
                }

                #region TIME [Warning: Toujours laisser cette section en fin de if car elle utilise sqlWhere]
                m_AdjustedTime = KeyQuoteIN.Time;
                //
                if ("=" == KeyQuoteIN.TimeOperator)
                {
                    // EG 20140930 Use SQL_AssetBase
                    SQL_AssetBase sql_Asset;

                    #region Holiday management
                    switch (QuoteObjIN)
                    {
                        case QuoteEnum.DEBTSECURITY:
                            sql_Asset = new SQL_AssetDebtSecurity(_csManager.CsSpheres, IdAssetIN);
                            break;
                        case QuoteEnum.EQUITY:
                            sql_Asset = new SQL_AssetEquity(_csManager.CsSpheres, IdAssetIN);
                            break;
                        case QuoteEnum.ETD:
                            sql_Asset = new SQL_AssetETD(_csManager.CsSpheres, IdAssetIN);
                            break;
                        case QuoteEnum.EXCHANGETRADEDFUND:
                            sql_Asset = new SQL_AssetExchangeTradedFund(_csManager.CsSpheres, IdAssetIN);
                            break;
                        case QuoteEnum.FXRATE:
                            sql_Asset = new SQL_AssetFxRate(_csManager.CsSpheres, IdAssetIN);
                            break;
                        case QuoteEnum.RATEINDEX:
                            sql_Asset = new SQL_AssetRateIndex(_csManager.CsSpheres, SQL_AssetRateIndex.IDType.IDASSET, IdAssetIN);
                            break;
                        case QuoteEnum.INDEX:
                            sql_Asset = new SQL_AssetIndex(_csManager.CsSpheres, IdAssetIN);
                            break;
                        case QuoteEnum.SIMPLEIRS:
                            sql_Asset = new SQL_AssetSimpleIRSwap(_csManager.CsSpheres, IdAssetIN);
                            break;
                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("Quote {0} is not managed, please contact EFS", QuoteObjIN.ToString()));
                            // EG 20160404 Migration vs2013
                            //break;
                    }

                    if (null != sql_Asset)
                    {
                        sql_Asset.DbTransaction = DbTransaction;
                        if (sql_Asset.IsLoaded)
                        {
                            string idBCAdditional = string.Empty;
                            DayTypeEnum dayType = DayTypeEnum.Business;
                            string idBC;
                            if (sql_Asset is SQL_AssetRateIndex index)
                            {
                                idBC = index.Idx_BusinessCenter;
                                idBCAdditional = index.Idx_BusinessCenterAdditional;
                            }
                            else if (sql_Asset is SQL_AssetFxRate rate)
                            {
                                idBC = rate.IdBC_RateSrc;
                            }
                            else if (sql_Asset is SQL_AssetDebtSecurity)
                            {
                                idBC = sql_Asset.Market_IDBC;
                            }
                            else
                            {
                                idBC = sql_Asset.Market_IDBC;
                                dayType = DayTypeEnum.ExchangeBusiness;
                            }

                            if (StrFunc.IsFilled(idBC))
                                CalcAdjustedTime(idBC, idBCAdditional, dayType);

                            SetMarketInformation(sql_Asset);
                        }

                    }

                    #endregion Holiday management
                    QuoteTimingEnum quoteTiming = GetQuotationTimingRequest();
                    //
                    //TIME (Date et Heure)
                    bool isBetween;
                    if ((QuoteTimingEnum.Open == quoteTiming) ||
                        (QuoteTimingEnum.Close == quoteTiming) ||
                        (QuoteTimingEnum.High == quoteTiming) ||
                        (QuoteTimingEnum.Low == quoteTiming))
                    {
                        //FI 20101213 Pour les prix Open, Close, High, Low il ne peut y avoir qu'un prix (peu importe l'heure)
                        // EG 20140120 Report 3.7
                        //isBetween = true;
                        isBetween = (AdjustedTime.Date == AdjustedTime);
                    }
                    else if (QuoteTimingEnum.Intraday == quoteTiming)
                    {
                        isBetween = false;
                    }
                    else
                        throw new NotImplementedException(StrFunc.AppendFormat("QuoteTiming {0} is not implemented", quoteTiming.ToString()));
                    //
                    if (isBetween)
                    {
                        //NB: Dans le cas où il existe un QuoteTiming (ex. Close), on ne tient "plus" compte de l'heure, mais juste de la date
                        sqlWhere.Append(this.SQLObject + ".TIME " + SQLCst.SUPEQUAL + "@TIME1");
                        sqlWhere.Append(this.SQLObject + ".TIME " + SQLCst.INFEQUAL + "@TIME2");
                        //
                        Dataparameters.Add(new DataParameter(CS, "TIME1", DbType.DateTime), AdjustedTime.Date);
                        Dataparameters.Add(new DataParameter(CS, "TIME2", DbType.DateTime), AdjustedTime.Date.Add(new TimeSpan(23, 59, 59)));
                    }
                    else
                    {
                        //Lorsque QuoteTiming n'est pas précisé ou Intraday Spheres recherche absoument le prix à la date, heure demandé
                        // ou simplement heure de spécifiée
                        sqlWhere.Append(this.SQLObject + ".TIME " + KeyQuoteIN.TimeOperator + " @TIME");
                        Dataparameters.Add(new DataParameter(CS, "TIME", DbType.DateTime), AdjustedTime);
                    }
                }
                else
                {
                    //Recherche d'une cotation par rapport à une Date/Heure (operator différent de "=")
                    string whereTmp = sqlWhere.ToString();
                    whereTmp += SQLCst.AND + this.SQLObject + ".TIME " + KeyQuoteIN.TimeOperator + " @TIME";
                    Dataparameters.Add(new DataParameter(CS, "TIME", DbType.DateTime), AdjustedTime);
                    //
                    string minmax = (KeyQuoteIN.TimeOperator.Trim().StartsWith("<") ? SQLCst.MAX : SQLCst.MIN);
                    //
                    string quote_TableName;
                    switch (QuoteObjIN)
                    {
                        case QuoteEnum.DEBTSECURITY:
                            quote_TableName = Cst.OTCml_TBL.QUOTE_DEBTSEC_H.ToString();
                            break;
                        case QuoteEnum.EXCHANGETRADEDFUND:
                            quote_TableName = Cst.OTCml_TBL.QUOTE_EXTRDFUND_H.ToString();
                            break;
                        default:
                            quote_TableName = Cst.OTCml_TBL.QUOTE_x_H.ToString().Replace("x", QuoteObjIN.ToString());
                            break;
                    }

                    string whereTime = this.SQLObject + ".TIME = (" + Cst.CrLf;
                    whereTime += SQLCst.SELECT + minmax + "(TIME)" + Cst.CrLf;
                    whereTime += SQLCst.FROM_DBO + quote_TableName + Cst.CrLf;
                    whereTime += whereTmp;
                    whereTime += ")";
                    sqlWhere.Append(whereTime);
                }
                #endregion
            }
            #endregion Contruct Where
            //
            base.InitSQLWhere(sqlWhere.ToString(), false);
        }

        /// <summary>
        /// Get Quotation using pIdCPivot Currency like pivot
        /// </summary>
        /// <param name="pIdCPivot"></param>
        /// <returns></returns>
        // RD 20130802 [18865] réfatoring
        public bool GetQuoteByPivotCurrency(string pIdCPivot)
        {
            return GetQuoteByPivotCurrency(pIdCPivot, out _, out _);
        }

        /// <summary>
        /// Get Quotation using pIdCPivot Currency like pivot, and return informations about Quote used
        /// </summary>
        /// <param name="pIdCPivot"></param>
        /// <returns></returns>
        public bool GetQuoteByPivotCurrency(string pIdCPivot,
            out KeyQuoteFxRate pQuote_IdC1vsPivot, out KeyQuoteFxRate pQuote_IdC2vsPivot)
        {
            bool ret = false;
            pQuote_IdC1vsPivot = new KeyQuoteFxRate();
            pQuote_IdC2vsPivot = new KeyQuoteFxRate();

            if ((null != KeyAssetIN) && KeyAssetIN.GetType().Equals(typeof(KeyAssetFxRate)))
            {
                KeyAssetFxRate keyAssetFXRate = (KeyAssetFxRate)KeyAssetIN;
                //
                if ((keyAssetFXRate.IdC1 != pIdCPivot) && (keyAssetFXRate.IdC2 != pIdCPivot))
                {
                    string original_idC1 = keyAssetFXRate.IdC1;
                    string original_idC2 = keyAssetFXRate.IdC2;
                    QuoteBasisEnum original_QuoteBasis = keyAssetFXRate.QuoteBasis;
                    
                    //decimal quoteValue_IDC1vsPivot = 0;
                    //decimal quoteValue_IDC2vsPivot = 0;
                    //QuoteBasisEnum quoteBasis_IDC1vsPivot;
                    //QuoteBasisEnum quoteBasis_IDC2vsPivot;

                    #region Step 1/3: Search IDC1 vs Pivot
                    //Initialize with new currencies
                    keyAssetFXRate.IdC1 = original_idC1;
                    keyAssetFXRate.IdC2 = pIdCPivot;
                    keyAssetFXRate.SetQuoteBasis(true);
                    IdAssetIN = 0;
                    _isExecutedColumn = false; // Pour recharger la table
                    m_QuoteValueCodeReturn = Cst.ErrLevel.UNDEFINED;
                    //Load
                    bool isLoaded = IsLoaded;
                    ret = isLoaded && (QuoteValueCodeReturn == Cst.ErrLevel.SUCCESS) && (m_QuoteValue != 0);
                    #endregion
                    //
                    if (ret)
                    {
                        //quoteValue_IDC1vsPivot = m_QuoteValue;
                        //quoteBasis_IDC1vsPivot = keyAssetFXRate.QuoteBasis;
                        pQuote_IdC1vsPivot.QuoteValue = m_QuoteValue;
                        pQuote_IdC1vsPivot.QuoteBasis = keyAssetFXRate.QuoteBasis;
                        pQuote_IdC1vsPivot.IdQuote = IdQuote;
                        
                        #region Step 2/3: Search IDC2 vs Pivot
                        //Initialize with new currencies
                        keyAssetFXRate.IdC1 = original_idC2;
                        keyAssetFXRate.IdC2 = pIdCPivot;
                        keyAssetFXRate.SetQuoteBasis(true);
                        IdAssetIN = 0;
                        _isExecutedColumn = false; // Pour recharger la table
                        m_QuoteValueCodeReturn = Cst.ErrLevel.UNDEFINED;
                        //Load
                        isLoaded = this.IsLoaded;
                        ret = isLoaded && (QuoteValueCodeReturn == Cst.ErrLevel.SUCCESS) && (m_QuoteValue != 0);
                        #endregion
                        //
                        if (ret)
                        {
                            //quoteValue_IDC2vsPivot = m_QuoteValue;
                            //quoteBasis_IDC2vsPivot = keyAssetFXRate.QuoteBasis;
                            pQuote_IdC2vsPivot.QuoteValue = m_QuoteValue;
                            pQuote_IdC2vsPivot.QuoteBasis = keyAssetFXRate.QuoteBasis;
                            pQuote_IdC2vsPivot.IdQuote = IdQuote;
                            
                            #region Step 3/3: Calculate IDC1 vs IDC2
                            //Re-Initialize with original currencies
                            keyAssetFXRate.IdC1 = original_idC1;
                            keyAssetFXRate.IdC2 = original_idC2;
                            keyAssetFXRate.QuoteBasis = original_QuoteBasis;
                            //Calcul de la cotation finale. 
                            //ATTENTION: lLa valorisation des 2 booléens ci-dessous est fonction des 2 cotations précédentes,
                            //pour lesquelles il est IMPERATIF de garder dans les 2 cas, la devise pivot en IDC2 (PL 20100422).
                            
                            //bool isMultiply = (quoteBasis_IDC1vsPivot != quoteBasis_IDC2vsPivot);
                            //bool isReverse = false;
                            //if (keyAssetFXRate.QuoteBasis == QuoteBasisEnum.Currency2PerCurrency1)
                            //{
                            //    if ((quoteBasis_IDC1vsPivot == quoteBasis_IDC2vsPivot) && (quoteBasis_IDC1vsPivot == QuoteBasisEnum.Currency1PerCurrency2))
                            //        isReverse = true;
                            //    else if ((quoteBasis_IDC1vsPivot != quoteBasis_IDC2vsPivot) && (quoteBasis_IDC1vsPivot == QuoteBasisEnum.Currency1PerCurrency2))
                            //        isReverse = true;
                            //}
                            //else if (keyAssetFXRate.QuoteBasis == QuoteBasisEnum.Currency1PerCurrency2)
                            //{
                            //    if ((quoteBasis_IDC1vsPivot == quoteBasis_IDC2vsPivot) && (quoteBasis_IDC1vsPivot == QuoteBasisEnum.Currency2PerCurrency1))
                            //        isReverse = true;
                            //    else if ((quoteBasis_IDC1vsPivot != quoteBasis_IDC2vsPivot) && (quoteBasis_IDC1vsPivot == QuoteBasisEnum.Currency2PerCurrency1))
                            //        isReverse = true;
                            //}
                            ////
                            //m_QuoteValue = (isMultiply ? (quoteValue_IDC1vsPivot * quoteValue_IDC2vsPivot) : (quoteValue_IDC1vsPivot / quoteValue_IDC2vsPivot));

                            bool isMultiply = (pQuote_IdC1vsPivot.QuoteBasis != pQuote_IdC2vsPivot.QuoteBasis);
                            bool isReverse = false;
                            if (keyAssetFXRate.QuoteBasis == QuoteBasisEnum.Currency2PerCurrency1)
                            {
                                if ((pQuote_IdC1vsPivot.QuoteBasis == pQuote_IdC2vsPivot.QuoteBasis) && (pQuote_IdC1vsPivot.QuoteBasis == QuoteBasisEnum.Currency1PerCurrency2))
                                    isReverse = true;
                                else if ((pQuote_IdC1vsPivot.QuoteBasis != pQuote_IdC2vsPivot.QuoteBasis) && (pQuote_IdC1vsPivot.QuoteBasis == QuoteBasisEnum.Currency1PerCurrency2))
                                    isReverse = true;
                            }
                            else if (keyAssetFXRate.QuoteBasis == QuoteBasisEnum.Currency1PerCurrency2)
                            {
                                if ((pQuote_IdC1vsPivot.QuoteBasis == pQuote_IdC2vsPivot.QuoteBasis) && (pQuote_IdC1vsPivot.QuoteBasis == QuoteBasisEnum.Currency2PerCurrency1))
                                    isReverse = true;
                                else if ((pQuote_IdC1vsPivot.QuoteBasis != pQuote_IdC2vsPivot.QuoteBasis) && (pQuote_IdC1vsPivot.QuoteBasis == QuoteBasisEnum.Currency2PerCurrency1))
                                    isReverse = true;
                            }
                            //
                            m_QuoteValue = (isMultiply ? (pQuote_IdC1vsPivot.QuoteValue * pQuote_IdC2vsPivot.QuoteValue) : (pQuote_IdC1vsPivot.QuoteValue / pQuote_IdC2vsPivot.QuoteValue));

                            if (isReverse)
                                m_QuoteValue = 1 / m_QuoteValue;
                            #endregion
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne la valeur de la colonne QUOTESIDE présente une ligne du jeu re résultat
        ///  <para>Retourne QuotationSideEnum.OfficialClose si la valeur est null</para>
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static QuotationSideEnum GetRowQuoteSide(DataRow row)
        {
            QuotationSideEnum ret = QuotationSideEnum.OfficialClose;
            string quoteSide = Convert.IsDBNull(row["QUOTESIDE"]) ? string.Empty : row["QUOTESIDE"].ToString();

            if (System.Enum.IsDefined(typeof(QuotationSideEnum), quoteSide))
                ret = (QuotationSideEnum)System.Enum.Parse(typeof(QuotationSideEnum), quoteSide, true);
            return ret;
        }

        /// <summary>
        /// Retourne le QuoteSide demandé (si null alors OfficialClose)
        /// </summary>
        /// <returns></returns>
        private QuotationSideEnum GetQuotationSideRequest()
        {
            QuotationSideEnum ret = QuotationSideEnum.OfficialClose;
            if (null != this.KeyQuoteIN)
            {
                if (KeyQuoteIN.QuoteSide.HasValue)
                {
                    if (System.Enum.IsDefined(typeof(QuotationSideEnum), KeyQuoteIN.QuoteSide.Value))
                        ret = (QuotationSideEnum)System.Enum.Parse(typeof(QuotationSideEnum), KeyQuoteIN.QuoteSide.Value.ToString(), true);
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne le QuoteTiming demandé (si null alors Close)
        /// </summary>
        /// <returns></returns>
        private QuoteTimingEnum GetQuotationTimingRequest()
        {
            QuoteTimingEnum ret = QuoteTimingEnum.Close;
            if (null != this.KeyQuoteIN)
            {
                if (KeyQuoteIN.QuoteTiming.HasValue)
                {
                    if (System.Enum.IsDefined(typeof(QuoteTimingEnum), KeyQuoteIN.QuoteTiming.Value))
                        ret = (QuoteTimingEnum)System.Enum.Parse(typeof(QuoteTimingEnum), KeyQuoteIN.QuoteTiming.Value.ToString(), true);
                }
            }
            return ret;
        }

        /// <summary>
        /// Obtient l'expression utilisée pour trier selon le quoteSide demandé
        /// </summary>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory (Nouveau type de cotation)
        private string GetExpressionOrderByQuoteSide()
        {
            StrBuilder ret = new StrBuilder();
            QuotationSideEnum quoteSideRequest = GetQuotationSideRequest();
            //
            switch (quoteSideRequest)
            {
                case QuotationSideEnum.OfficialSettlement:
                    ret += SQLCst.CASE + this.SQLObject + ".QUOTESIDE" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.OfficialSettlement.ToString()) + SQLCst.CASE_THEN + "10" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.OfficialClose.ToString()) + SQLCst.CASE_THEN + "5" + Cst.CrLf;
                    ret += SQLCst.CASE_ELSE + "0" + Cst.CrLf;
                    ret += SQLCst.CASE_END;
                    break;

                case QuotationSideEnum.FairValue:
                    ret += SQLCst.CASE + this.SQLObject + ".QUOTESIDE" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.FairValue.ToString()) + SQLCst.CASE_THEN + "5" + Cst.CrLf;
                    ret += SQLCst.CASE_ELSE + "0" + Cst.CrLf;
                    ret += SQLCst.CASE_END;
                    break;

                case QuotationSideEnum.OfficialClose:
                    ret += SQLCst.CASE + this.SQLObject + ".QUOTESIDE" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.OfficialClose.ToString()) + SQLCst.CASE_THEN + "5" + Cst.CrLf;
                    ret += SQLCst.CASE_ELSE + "0" + Cst.CrLf;
                    ret += SQLCst.CASE_END;
                    break;

                case QuotationSideEnum.Mid:
                    ret += SQLCst.CASE + this.SQLObject + ".QUOTESIDE" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.Mid.ToString()) + SQLCst.CASE_THEN + "10" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.Bid.ToString()) + SQLCst.CASE_THEN + "5" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.Ask.ToString()) + SQLCst.CASE_THEN + "5" + Cst.CrLf;
                    ret += SQLCst.CASE_ELSE + "0" + Cst.CrLf;
                    ret += SQLCst.CASE_END;
                    break;

                case QuotationSideEnum.Bid:
                    ret += SQLCst.CASE + this.SQLObject + ".QUOTESIDE" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.Bid.ToString()) + SQLCst.CASE_THEN + "10" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.Ask.ToString()) + SQLCst.CASE_THEN + "7" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.Mid.ToString()) + SQLCst.CASE_THEN + "5" + Cst.CrLf;
                    ret += SQLCst.CASE_ELSE + "0" + Cst.CrLf;
                    ret += SQLCst.CASE_END;
                    break;

                case QuotationSideEnum.Ask:
                    ret += SQLCst.CASE + this.SQLObject + ".QUOTESIDE" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.Ask.ToString()) + SQLCst.CASE_THEN + "10" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.Bid.ToString()) + SQLCst.CASE_THEN + "7" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuotationSideEnum.Mid.ToString()) + SQLCst.CASE_THEN + "5" + Cst.CrLf;
                    ret += SQLCst.CASE_ELSE + "0" + Cst.CrLf;
                    ret += SQLCst.CASE_END;
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", quoteSideRequest.ToString()));
            }
            return ret.ToString();
        }

        /// <summary>
        /// Obtient l'expression utilisée pour trier selon le QuoteTiming demandé
        /// </summary>
        /// <returns></returns>
        private string GetExpressionOrderByQuoteTiming()
        {
            StrBuilder ret = new StrBuilder();
            QuoteTimingEnum quoteTiming = GetQuotationTimingRequest();
            //
            switch (quoteTiming)
            {
                case QuoteTimingEnum.Close:
                    ret += SQLCst.CASE + this.SQLObject + ".QUOTETIMING" + Cst.CrLf;
                    ret += SQLCst.CASE_WHEN + DataHelper.SQLString(QuoteTimingEnum.Close.ToString()) + SQLCst.CASE_THEN + "5" + Cst.CrLf;
                    ret += SQLCst.CASE_ELSE + "0" + Cst.CrLf;
                    ret += SQLCst.CASE_END;
                    break;
                case QuoteTimingEnum.High:
                case QuoteTimingEnum.Intraday:
                case QuoteTimingEnum.Low:
                case QuoteTimingEnum.Open:
                    ret += this.SQLObject + ".QUOTETIMING";
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", quoteTiming.ToString()));
            }
            return ret.ToString();
        }

        /// <summary>
        /// Retourne l'expression Where specifique à QUOTESIDE
        /// </summary>
        /// <returns></returns>
        /// FI 20110706 [17490] 
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory (Nouveau type de cotation)
        private string GetExpressionWhereQuoteSide()
        {
            SQLWhere sqlWhere = new SQLWhere(EFS.ApplicationBlocks.Data.SQLWhere.StartEnum.StartWithWhere);
            //QUOTESIDE (Bid,Mid,Ask,OfficialClose,OfficialSettlement)
            //FI 20110706 [17490] (add gestion du QuoteSide, avant il n'y avait rien)
            QuotationSideEnum quoteSideRequest = GetQuotationSideRequest();
            if (quoteSideRequest == QuotationSideEnum.OfficialSettlement)
            {
                //Si OfficialSettlement les prix OfficialSettlement sont candidats + eventuellement les prix OfficialClose(ou null)
                sqlWhere.Append("(" + this.SQLObject + ".QUOTESIDE = @QUOTESIDE");
                Dataparameters.Add(new DataParameter(CS, "QUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), QuotationSideEnum.OfficialSettlement.ToString());
                if (OfficialSettlementBehaviorEnum.OfficialAll == OfficialSettlementBehavior)
                {
                    sqlWhere.Append(this.SQLObject + ".QUOTESIDE=" + DataHelper.SQLString(QuotationSideEnum.OfficialClose.ToString()), SQLCst.OR);
                    sqlWhere.Append(this.SQLObject + ".QUOTESIDE" + SQLCst.IS_NULL, SQLCst.OR); //null signifie OfficialClose
                }
                sqlWhere.Append(")", string.Empty);
            }
            else if ((quoteSideRequest == QuotationSideEnum.Mid) ||
                         (quoteSideRequest == QuotationSideEnum.Bid) ||
                         (quoteSideRequest == QuotationSideEnum.Ask))
            {
                //Si Bid/Mid/Ask les prix Bid/Mid/Ask et OfficialClose(ou null) sont candidats 
                sqlWhere.Append("(" + DataHelper.SQLColumnIn(CS, this.SQLObject + ".QUOTESIDE",
                new string[] { QuotationSideEnum.Mid.ToString(), 
                                            QuotationSideEnum.Bid.ToString(), 
                                            QuotationSideEnum.Ask.ToString(),
                                            QuotationSideEnum.OfficialClose.ToString(),
                                            }, TypeData.TypeDataEnum.@string));
                sqlWhere.Append(this.SQLObject + ".QUOTESIDE" + SQLCst.IS_NULL, SQLCst.OR); //null signifie OfficialClose
                sqlWhere.Append(")", string.Empty);
            }
            else if (quoteSideRequest == QuotationSideEnum.OfficialClose)
            {
                //les OfficialClose(ou null) sont candidats
                sqlWhere.Append("(" + this.SQLObject + ".QUOTESIDE=" + DataHelper.SQLString(QuotationSideEnum.OfficialClose.ToString()));
                sqlWhere.Append(this.SQLObject + ".QUOTESIDE" + SQLCst.IS_NULL, SQLCst.OR); //null signifie OfficialClose
                sqlWhere.Append(")", string.Empty);
            }
            else if (quoteSideRequest == QuotationSideEnum.FairValue)
            {
                //les fairValue sont candidats
                sqlWhere.Append("(" + this.SQLObject + ".QUOTESIDE=" + DataHelper.SQLString(QuotationSideEnum.FairValue.ToString()));
                sqlWhere.Append(")", string.Empty);
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", quoteSideRequest));
            //    
            string ret = sqlWhere.ToString().Replace(SQLCst.WHERE, string.Empty);
            return ret;
        }

        /// <summary>
        /// Retourne l'expression Where specifique à QUOTETIMING
        /// </summary>
        /// <returns></returns>
        /// FI 20110706 [17490] 
        private string GetExpressionWhereQuoteTiming()
        {
            SQLWhere sqlWhere = new SQLWhere(EFS.ApplicationBlocks.Data.SQLWhere.StartEnum.StartWithWhere);
            QuoteTimingEnum quoteTiming = GetQuotationTimingRequest();
            //
            sqlWhere.Append("(" + this.SQLObject + ".QUOTETIMING = @QUOTETIMING");
            Dataparameters.Add(new DataParameter(CS, "QUOTETIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), quoteTiming);
            //
            if (quoteTiming == QuoteTimingEnum.Close)
            {
                //les Close(ou null) sont candidats
                sqlWhere.Append(this.SQLObject + ".QUOTETIMING" + SQLCst.IS_NULL, SQLCst.OR); //null signifie Close
            }
            //
            sqlWhere.Append(")", string.Empty);
            //
            string ret = sqlWhere.ToString().Replace(SQLCst.WHERE, string.Empty);
            return ret;
        }
        #endregion Methods
    }
    #endregion SQL_Quote
}
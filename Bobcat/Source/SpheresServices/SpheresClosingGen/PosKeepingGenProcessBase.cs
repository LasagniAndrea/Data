#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process.EventsGen;
using EFS.SpheresRiskPerformance.Interface;
using EFS.SpheresRiskPerformance.Properties;
using EFS.TradeInformation;
using EFS.TradeLink;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.StrategyMarker;
using EfsML.v30.PosRequest;
//
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
//
using FpML.Enum;
using FpML.Interface;
//
using SpheresClosingGen.Properties;
//
using Tz = EFS.TimeZone;

//using AsyncConfiguration;
//using AsyncConfiguration2;
#endregion Using Directives

namespace EFS.Process.PosKeeping
{
    #region CacheAsset
    // EG 20150624 [21151] New
    public class CacheAsset
    {
        #region Members
        public Nullable<Cst.UnderlyingAsset> UnderlyingAsset { set; get; }
        public int IdAsset { set; get; }
        public Cst.PosRequestAssetQuoteEnum PosRequestAssetQuote { set; get; }
        #endregion Members
        #region Constructors
        public CacheAsset() { }
        public CacheAsset(CacheAsset pCacheAsset)
            : this(pCacheAsset.UnderlyingAsset, pCacheAsset.IdAsset, pCacheAsset.PosRequestAssetQuote)
        {
        }
        public CacheAsset(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, int pIdAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            UnderlyingAsset = pUnderlyingAsset;
            IdAsset = pIdAsset;
            PosRequestAssetQuote = pPosRequestAssetQuote;
        }
        #endregion Constructors
    }
    #endregion CacheAsset
    #region CacheAssetComparer
    /// <summary>
    /// Comparer de trades par TradeKey
    /// </summary>
    // EG 20150624 [21151] New
    internal class CacheAssetComparer : IEqualityComparer<CacheAsset>
    {
        #region IEqualityComparer
        /// <summary>
        /// Les TradeKey sont égaux s'ils ont les même:
        /// DEALER  (ACTEUR/BOOK), CLEARER (ACTEUR/BOOK), IDASSET
        /// DTBUSINESS, DTTRADE, SIDE, EXECUTIVE BROKER (si existe)
        /// </summary>
        /// <param name="x">1er CacheAsset à comparer</param>
        /// <param name="y">2ème CacheAsset à comparer</param>
        /// <returns>true si x Equals Y, sinon false</returns>
        public bool Equals(CacheAsset pCacheAsset1, CacheAsset pCacheAsset2)
        {

            //Vérifier si les objets référencent les même données
            if (ReferenceEquals(pCacheAsset1, pCacheAsset2)) return true;

            //Vérifier si un des objets est null
            if (pCacheAsset1 is null || pCacheAsset2 is null)
                return false;

            // Vérifier qu'il s'agit des même TradeInfo
            return (pCacheAsset1.UnderlyingAsset == pCacheAsset2.UnderlyingAsset) &&
                   (pCacheAsset1.IdAsset == pCacheAsset2.IdAsset) &&
                   (pCacheAsset1.PosRequestAssetQuote == pCacheAsset2.PosRequestAssetQuote);
        }

        /// <summary>
        /// La méthode GetHashCode fournissant la même valeur pour des objets CacheAsset qui sont égaux.
        /// </summary>
        /// <param name="pCombinedCommodity">Le paramètre CacheAsset dont on veut le hash code</param>
        /// <returns>La valeur du hash code</returns>
        public int GetHashCode(CacheAsset pCacheAsset)
        {
            //Vérifier si l'obet est null
            if (pCacheAsset is null) return 0;

            int hashUnderlyingAsset = pCacheAsset.UnderlyingAsset.GetHashCode();
            int hashIdAsset = pCacheAsset.IdAsset.GetHashCode();
            int hashPosRequestAssetQuote = pCacheAsset.PosRequestAssetQuote.GetHashCode();

            //Calcul du hash code pour le CacheAsset.
            return (int)(hashUnderlyingAsset ^ hashIdAsset ^ hashPosRequestAssetQuote);
        }
        #endregion IEqualityComparer

    }
    #endregion CacheAssetComparer

    #region PosKeepingEntityMarket
    public class PosKeepingEntityMarket
    {
        #region Members
        public int idEM;
        public int idM;
        public int idA_Entity;
        public string entityIdentifier;
        public Nullable<int> idA_Custodian;
        public int idA_Css;
        public int idA_CssCustodian;
        public string cssCustodianIdentifier; 
        public string market;
        public string marketShortAcronym;
        public string idBC;

        public bool isCommodity;
        public bool isExchangeTradedDerivative;
        public bool isMarkToMarket;
        public bool isInitialMargin;
        public bool isOverTheCounter;
        public bool isSecurity;

        private ProductTools.GroupProductEnum currentGroupProduct;
        #endregion Members

        #region Accessors
        // EG 20170510 [21153]
        public bool IsNoActivity
        {
            get
            {
                return (false == isCommodity) && (false == isExchangeTradedDerivative) && (false == isMarkToMarket) &&
                       (false == isOverTheCounter) && (false == isSecurity);
            }
        }
     
        #endregion Accessors

        #region CurrentgProduct
        public ProductTools.GroupProductEnum CurrentGroupProduct
        {
            set { currentGroupProduct = value; }
            get { return currentGroupProduct; }
        }
        #endregion CurrentgProduct

        #region Constructors
        /// EG 20170221 Remove restriction on IDSTACTIVATION
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20201201 [25562] Ajout index (IDM, IDA_ENTITY, IDA_CSSCUSTODIAN etIDSTBUSINESS) et Suppression jointure sur MARKET et Modification GroupBy
        public PosKeepingEntityMarket(string pCs, DataRow pRowEntityMarket)
        {
            idEM = Convert.ToInt32(pRowEntityMarket["IDEM"]);
            idA_Entity = Convert.ToInt32(pRowEntityMarket["IDA_ENTITY"]);
            if (Convert.IsDBNull(pRowEntityMarket["IDA_CUSTODIAN"]))
            {
                idA_CssCustodian = Convert.ToInt32(pRowEntityMarket["IDA_CSS"]);
            }
            else
            {
                idA_CssCustodian = Convert.ToInt32(pRowEntityMarket["IDA_CUSTODIAN"]);
                idA_Custodian = idA_CssCustodian;
            }
            cssCustodianIdentifier = pRowEntityMarket["CSSCUSTODIAN_IDENTIFIER"].ToString();
            entityIdentifier = pRowEntityMarket["ENTITY_IDENTIFIER"].ToString();

            idM = Convert.ToInt32(pRowEntityMarket["IDM"]);
            market = pRowEntityMarket["IDENTIFIER"].ToString();
            marketShortAcronym = pRowEntityMarket["SHORT_ACRONYM"].ToString();
            idBC = pRowEntityMarket["IDBC"].ToString();

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCs, "IDM", DbType.Int32), idM);
            parameters.Add(new DataParameter(pCs, "IDA_ENTITY", DbType.Int32), idA_Entity);
            parameters.Add(new DataParameter(pCs, "IDA_CSSCUSTODIAN", DbType.Int32), idA_CssCustodian);

            // EG 20170221 Plus de test sur IDSTACTIVATION
            string sqlSelect = @"select count(*) as NBTRADES, pr.GPRODUCT
            from dbo.TRADE tr 
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
            where (tr.IDM = @IDM) and (tr.IDA_ENTITY = @IDA_ENTITY) and (tr.IDA_CSSCUSTODIAN = @IDA_CSSCUSTODIAN) and (tr.IDSTBUSINESS = 'ALLOC')
            group by pr.GPRODUCT";

            QueryParameters qryParameters = new QueryParameters(pCs, sqlSelect, parameters);
            DataSet ds = DataHelper.ExecuteDataset(pCs, CommandType.Text, qryParameters.QueryHint, qryParameters.GetArrayDbParameterHint());
            if (null == ds)
                throw new NullReferenceException("PosKeepingGenProcessBase.PosKeepingEntityMarket return null"); 

            DataRowCollection rowNbTradesByGProduct = ds.Tables[0].Rows;
            foreach (DataRow row in rowNbTradesByGProduct)
            {
                ProductTools.GroupProductEnum gProduct = (ProductTools.GroupProductEnum)ReflectionTools.EnumParse(new ProductTools.GroupProductEnum(), row["GPRODUCT"].ToString());
                if ((false == Convert.IsDBNull(row["NBTRADES"])) && (0 < Convert.ToInt32(row["NBTRADES"])))
                {
                    switch (gProduct)
                    {
                        case ProductTools.GroupProductEnum.Commodity:
                            isCommodity = true;
                            isInitialMargin = true;
                            break;
                        case ProductTools.GroupProductEnum.ExchangeTradedDerivative:
                            isExchangeTradedDerivative = true;
                            isInitialMargin = true;
                            break;
                        case ProductTools.GroupProductEnum.ForeignExchange:
                        case ProductTools.GroupProductEnum.MarkToMarket:
                            isMarkToMarket = idA_Custodian.HasValue && (-1 == idM);
                            break;
                        case ProductTools.GroupProductEnum.OverTheCounter:
                            isOverTheCounter = true;
                            break;
                        case ProductTools.GroupProductEnum.Security:
                            isSecurity = true;
                            break;
                    }
                }
            }
        }
        #endregion Constructors
        #region Methods
        #region Increment_IDPR
        public int Increment_IDPR(Cst.PosRequestTypeEnum pPosRequestType, ProductTools.GroupProductEnum pGroup)
        {
            int incr = 0;
            switch (pPosRequestType)
            {
                // Contrôles
                case Cst.PosRequestTypeEnum.ClosingDay_ControlGroupLevel:
                    switch (pGroup)
                    {
                        case ProductTools.GroupProductEnum.Commodity:
                            incr = 7;
                            break;
                        case ProductTools.GroupProductEnum.ExchangeTradedDerivative:
                            // EG 20170206 [22787]
                            incr = 15;
                            break;
                        case ProductTools.GroupProductEnum.OverTheCounter:
                        case ProductTools.GroupProductEnum.Security:
                            incr = 10;
                            break;
                        case ProductTools.GroupProductEnum.MarkToMarket:
                            incr = 5;
                            break;
                    }
                    break;
                // Traitements à la clôture de journée
                case Cst.PosRequestTypeEnum.ClosingDay_EOD_MarketGroupLevel:
                    incr = 1;
                    //switch (pGroup)
                    //{
                    //    case ProductTools.GroupProductEnum.Commodity:
                    //        incr = 2;
                    //        break;
                    //    case ProductTools.GroupProductEnum.ExchangeTradedDerivative:
                    //        // EG 20170206 [22787]
                    //        incr = 4;
                    //        break;
                    //    case ProductTools.GroupProductEnum.OverTheCounter:
                    //    case ProductTools.GroupProductEnum.Security:
                    //        incr = 3;
                    //        break;
                    //    case ProductTools.GroupProductEnum.MarkToMarket:
                    //        incr = 2;
                    //        break;
                    //}
                    break;
            }
            return incr;
        }
        #endregion Increment_IDPR
        #region GetNbToken_IDPR
        public int GetNbToken_IDPR(Cst.PosRequestTypeEnum pPosRequestType)
        {
            int nbToken = 0;
            if (isCommodity)
                nbToken += Increment_IDPR(pPosRequestType, ProductTools.GroupProductEnum.Commodity);
            if (isExchangeTradedDerivative)
                nbToken += Increment_IDPR(pPosRequestType, ProductTools.GroupProductEnum.ExchangeTradedDerivative);
            if (isOverTheCounter)
                nbToken += Increment_IDPR(pPosRequestType, ProductTools.GroupProductEnum.OverTheCounter);
            if (isSecurity)
                nbToken += Increment_IDPR(pPosRequestType, ProductTools.GroupProductEnum.Security);
            if (isMarkToMarket)
                nbToken += Increment_IDPR(pPosRequestType, ProductTools.GroupProductEnum.MarkToMarket);
            return nbToken;
        }
        #endregion GetNbToken_IDPR

        #endregion Methods

    }

    #endregion PosKeepingEntityMarket

    #region PayerReceiverAmountInfo
    // EG 20140212 [20726] New
    // EG 20180221 [23769] Déplacé (auparavant dans PosKeepingGenBase)
    public class PayerReceiverAmountInfo
    {
        #region Members
        private Pair<Nullable<int>, Nullable<int>> _payer;
        private Pair<Nullable<int>, Nullable<int>> _receiver;
        private readonly Pair<Nullable<int>, Nullable<int>> _dealer;
        private readonly Pair<Nullable<int>, Nullable<int>> _clearer;
        private readonly string _eventType;
        private readonly bool _isDealerBuyer;
        private readonly Nullable<PutOrCallEnum> _putCall;

        private Nullable<decimal> _amount;
        private Nullable<QuoteTimingEnum> _quoteTiming;
        private Nullable<QuotationSideEnum> _quoteSide;
        private Nullable<decimal> _settlementPrice;
        private Nullable<decimal> _settlementPrice100;
        private Nullable<DateTime> _dtSettlementPrice;

        private Nullable<decimal> _quotePrice;
        private Nullable<decimal> _quotePrice100;
        private Nullable<decimal> _quoteDelta;

        #endregion Members
        #region Accessors
        #region Amount
        public Nullable<decimal> Amount
        {
            get
            {
                return _amount;
            }
        }
        #endregion Amount
        #region DtSettlementPrice
        public Nullable<DateTime> DtSettlementPrice
        {
            get
            {
                return _dtSettlementPrice;
            }
        }
        #endregion DtSettlementPrice
        #region IdA_Payer
        public Nullable<int> IdA_Payer
        {
            get
            {
                return _payer.First;
            }
        }
        #endregion IdA_Payer
        #region IdB_Payer
        public Nullable<int> IdB_Payer
        {
            get
            {
                return _payer.Second;
            }
        }
        #endregion IdB_Payer
        #region IdA_Receiver
        public Nullable<int> IdA_Receiver
        {
            get
            {
                return _receiver.First;
            }
        }
        #endregion IdA_Receiver
        #region IdB_Receiver
        public Nullable<int> IdB_Receiver
        {
            get
            {
                return _receiver.Second;
            }
        }
        #endregion IdB_Receiver
        #region IdStCalcul
        public StatusCalculEnum IdStCalcul
        {
            get
            {
                if (_amount.HasValue)
                    return StatusCalculEnum.CALC;
                else
                    return StatusCalculEnum.TOCALC;
            }
        }
        #endregion IdStCalcul
        #region QuoteDelta
        public Nullable<decimal> QuoteDelta
        {
            get
            {
                return _quoteDelta;
            }
        }
        #endregion QuoteDelta
        #region QuotePrice
        public Nullable<decimal> QuotePrice
        {
            get
            {
                return _quotePrice;
            }
        }
        #endregion QuotePrice
        #region QuotePrice100
        public Nullable<decimal> QuotePrice100
        {
            get
            {
                return _quotePrice100;
            }
        }
        #endregion QuotePrice100
        #region QuoteSide
        public Nullable<QuotationSideEnum> QuoteSide
        {
            get
            {
                return _quoteSide;
            }
        }
        #endregion QuoteSide
        #region QuoteTiming
        public Nullable<QuoteTimingEnum> QuoteTiming
        {
            get
            {
                return _quoteTiming;
            }
        }
        #endregion QuoteTiming
        #region SettlementPrice
        public Nullable<decimal> SettlementPrice
        {
            get
            {
                return _settlementPrice;
            }
        }
        #endregion SettlementPrice
        #region SettlementPrice100
        public Nullable<decimal> SettlementPrice100
        {
            get
            {
                return _settlementPrice100;
            }
        }
        #endregion SettlementPrice100
        #endregion Accessors
        #region Constructors
        public PayerReceiverAmountInfo(IPosKeepingData pPosKeepingData, string pEventType, bool pIsDealerBuyer)
        {
            _dealer = new Pair<int?, int?>(pPosKeepingData.IdA_Dealer, pPosKeepingData.IdB_Dealer);
            _clearer = new Pair<int?, int?>(pPosKeepingData.IdA_Clearer, pPosKeepingData.IdB_Clearer);
            _eventType = pEventType;
            _isDealerBuyer = pIsDealerBuyer;
            if (pPosKeepingData.Asset is PosKeepingAsset_ETD asset)
            {
                if (asset.putCallSpecified)
                    _putCall = asset.putCall;
            }
        }
        #endregion Constructors
        #region Methods
        #region SetCloseInfo
        public void SetCloseInfo(IPosKeepingData pPosKeepingData, Quote pQuote)
        {
            if (pQuote.valueSpecified)
            {
                _quotePrice = pQuote.value;
                _quotePrice100 = pPosKeepingData.ToBase100(_quotePrice.Value);
                _quotePrice100 = pPosKeepingData.VariableContractValue(_quotePrice100);
            }
            if (pQuote is Quote_ETDAsset quote)
            {
                if (quote.deltaSpecified)
                    _quoteDelta = quote.delta;
            }
        }
        #endregion SetCloseInfo
        #region SetPayerReceiver
        // EG 20151001 [21414] Refactoring
        public void SetPayerReceiver(Nullable<decimal> pAmount)
        {
            _payer = new Pair<int?, int?>();
            _receiver = new Pair<int?, int?>();
            _amount = pAmount;
            if (EventTypeFunc.IsVariationMargin(_eventType))
            {
                #region VariationMargin
                // SI >=0 Le payeur est le vendeur de l'opération
                // SI <0  Le payeur est l'acheteur de l'opération
                if ((pAmount.HasValue && (pAmount.Value < 0) && (false == _isDealerBuyer)) ||
                    ((false == _amount.HasValue) || (0 <= pAmount.Value)) && _isDealerBuyer)
                {
                    _payer = _clearer;
                    _receiver = _dealer;
                }
                else
                {
                    _payer = _dealer;
                    _receiver = _clearer;
                }
                #endregion VariationMargin
            }
            else if (EventTypeFunc.IsRealizedMargin(_eventType))
            {
                #region RealizedMargin
                if ((pAmount.HasValue && (pAmount.Value < 0) && _isDealerBuyer) ||
                    ((false == _amount.HasValue) || (0 <= pAmount.Value)) && (false == _isDealerBuyer))
                {
                    _payer = _dealer;
                    _receiver = _clearer;
                }
                else
                {
                    _payer = _clearer;
                    _receiver = _dealer;
                }
                #endregion RealizedMargin
            }
            else if (EventTypeFunc.IsSettlementCurrency(_eventType) && _putCall.HasValue)
            {
                #region SettlementCurrency
                if ((((false == _amount.HasValue) || (0 <= pAmount.Value)) && (PutOrCallEnum.Put == _putCall.Value)) ||
                    (_amount.HasValue && (0 > pAmount.Value) && (PutOrCallEnum.Call == _putCall.Value)))
                {
                    //---------------------------------------------------------------------------------------------
                    // Le payeur = Acheteur : (si CashSettlement >=0 et Put) ou (si CashSettlement <0 et Call)
                    //---------------------------------------------------------------------------------------------
                    if (_isDealerBuyer)
                    {
                        _payer = _dealer;
                        _receiver = _clearer;
                    }
                    else
                    {
                        _payer = _clearer;
                        _receiver = _dealer;
                    }
                }
                else
                {
                    //---------------------------------------------------------------------------------------------
                    // Le payeur = Vendeur : (si CashSettlement >=0 et Call) ou (si CashSettlement <0 et Put)
                    //---------------------------------------------------------------------------------------------
                    if (_isDealerBuyer)
                    {
                        _payer = _clearer;
                        _receiver = _dealer;
                    }
                    else
                    {
                        _payer = _dealer;
                        _receiver = _clearer;
                    }
                }
                #endregion SettlementCurrency
            }
        }
        #endregion SetPayerReceiver
        #region SetSettlementInfo
        public void SetSettlementInfo(IPosKeepingData pPosKeepingData, IPosKeepingQuote pQuote)
        {
            _quoteTiming = pQuote.QuoteTiming;
            _quoteSide = pQuote.QuoteSide;
            _settlementPrice = pQuote.QuotePrice;
            _dtSettlementPrice = pQuote.QuoteTime;

            if (_settlementPrice.HasValue)
            {
                _settlementPrice100 = pPosKeepingData.ToBase100_UNL(_settlementPrice.Value);
                _settlementPrice100 = pPosKeepingData.VariableContractValue_UNL(_settlementPrice100);
            }
        }
        #endregion SetSettlementInfo
        #endregion Methods
    }
    #endregion PayerReceiverAmountInfo

    #region AgreggatePayerReceiverAmountInfo
    // EG 20190613 [24683] New
    public class AgreggatePayerReceiverAmountInfo
    {
        #region Members
        private readonly Pair<Nullable<int>, Nullable<int>> _payer;
        private readonly Pair<Nullable<int>, Nullable<int>> _receiver;
        private Nullable<decimal> _amount;
        #endregion Members
        #region Accessors
        #region Amount
        public Nullable<decimal> Amount
        {
            get {return _amount;}
            set { _amount = value; }
        }
        #endregion Amount
        #region IdA_Payer
        public Nullable<int> IdA_Payer
        {
            get {return _payer.First;}
        }
        #endregion IdA_Payer
        #region IdB_Payer
        public Nullable<int> IdB_Payer
        {
            get {return _payer.Second;}
        }
        #endregion IdB_Payer
        #region IdA_Receiver
        public Nullable<int> IdA_Receiver
        {
            get {return _receiver.First;}
        }
        #endregion IdA_Receiver
        #region IdB_Receiver
        public Nullable<int> IdB_Receiver
        {
            get {return _receiver.Second;}
        }
        #endregion IdB_Receiver
        #endregion Accessors
        #region Constructors
        public AgreggatePayerReceiverAmountInfo(Nullable<decimal> pAmount,
            Nullable<int> pIdA_Payer, Nullable<int> pIdB_Payer, 
            Nullable<int> pIdA_Receiver, Nullable<int> pIdB_Receiver)
        {
            _payer = new Pair<int?, int?>(pIdA_Payer, pIdB_Payer);
            _receiver = new Pair<int?, int?>(pIdA_Receiver, pIdB_Receiver);
            _amount = pAmount;
        }
        #endregion Constructors
    }
    #endregion AgreggatePayerReceiverAmountInfo

    /// <summary>
    /// 
    /// </summary>
    // EG 20190308 Upd [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
    // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
    public abstract partial class PosKeepingGenProcessBase
    {
        #region Members
        protected RptSideProductContainer m_RptSideProductContainer;
        // EG 20150708 [21103] Add (before PosKeepingGen_ETD)
        protected IPosKeepingMarket m_EntityMarketInfo;

        protected PosKeepingGenProcess m_PKGenProcess;
        protected EFS_TradeLibrary m_TradeLibrary;
        
        protected EventQuery m_EventQuery;
        protected DataTable m_DtPosition;
        protected DataTable m_DtPosActionDet;
        protected DataTable m_DtPosActionDet_Working;
        protected IPosRequest m_MasterPosRequest;
        protected IPosRequest m_MarketPosRequest;
        protected IPosRequest m_PosRequest;
        protected IProductBase m_Product;
        protected LockObject m_LockObject;
        protected DataSetEventTrade m_DsEvents;
        protected IPosKeepingData m_PosKeepingData;
        protected Dictionary<int, TradeFeesCalculation> m_TradeCandidatesForFees = new Dictionary<int, TradeFeesCalculation>();
        protected Dictionary<int, TradeSafeKeepingCalculation> m_TradeCandidatesForSafeKeeping = new Dictionary<int, TradeSafeKeepingCalculation>();
        /// <summary>
        /// Liste des contextes de merge
        /// </summary>
        public List<TradeMergeRule> m_LstTradeMergeRule;
        /// <summary>
        /// Liste des trades candidats potentiels à merge (avec données de clé)
        /// </summary>
        public Dictionary<int, TradeCandidate> m_DicTradeCandidate;





        // EG 20180307 [23769] Gestion List<IPosRequest>
        protected List<IPosRequest> m_LstMarketSubPosRequest;
        protected List<IPosRequest> m_LstSubPosRequest;
        protected Hashtable m_TemplateDataDocumentTrade;
        /// <summary>
        /// Cache asset ETD
        /// </summary>
        // EG 20150624 [21151] CacheAsset
        protected Dictionary<CacheAsset, PosKeepingAsset> m_AssetCache;

        #region POSREQUEST Parameters variables
        protected IDbDataParameter paramIdPR;
        protected IDbDataParameter paramIdProcess_L;
        #endregion POSREQUEST Parameters variables

        /* FI 20180830 [24152] Mise en commentaire des Parameters variables
        #region POSACTION/POSACTIONDET Parameters variables
        protected IDbDataParameter paramIdPA;
        protected IDbDataParameter paramDtBusiness;
        protected IDbDataParameter paramDtIns;
        protected IDbDataParameter paramIdAIns;

        protected IDbDataParameter paramIdPADet;
        protected IDbDataParameter paramIdTBuy;
        protected IDbDataParameter paramIdTSell;
        protected IDbDataParameter paramIdTClosing;
        protected IDbDataParameter paramQty;
        protected IDbDataParameter paramDtCan;
        protected IDbDataParameter paramIdACan;
        protected IDbDataParameter paramCanDescription;
        #endregion POSACTION/POSACTIONDET Parameters variables
        */
        protected List<int> m_LockIdEM;

        protected string m_WorkTableUpdateEntry;

        /// <summary>
        /// Identifiant du process dans PROCESS_L
        /// </summary>
        
        protected int m_IdProcess;

        /// <summary>
        /// Identifiant du tracker dans TRACKER_L
        /// </summary>
        
        protected int m_IdTracker;
        #endregion Members

        #region Accessors

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // VIRTUAL ACCESSORS (OVERRIDE SUR ETD / OTC)
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region ActionDate
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual DateTime ActionDate
        {
            get { return DateTime.MinValue; }
        }
        #endregion ActionDate

        #region ClearingBusinessDate
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual DateTime ClearingBusinessDate
        {
            get { return DateTime.MinValue; }
        }
        #endregion ClearingBusinessDate

        #region DtBusiness
        protected DateTime DtBusiness
        {
            get
            {
                if (m_PKGenProcess.IsEntry)
                    return m_PosKeepingData.Trade.DtBusiness;
                else
                    return m_MasterPosRequest.DtBusiness;
            }
        }
        #endregion DtBusiness

        #region DtSettlement
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual DateTime DtSettlement
        {
            get {return DtBusiness;}
        }
        #endregion DtSettlement

        #region IsFuturesStyleMarkToMarket
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual bool IsFuturesStyleMarkToMarket
        {
            get { return false; }
        }
        #endregion IsFuturesStyleMarkToMarket
        #region IsOption
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual bool IsOption
        {
            get { return false; }
        }
        #endregion IsOption
        #region IsPremiumStyle
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual bool IsPremiumStyle
        {
            get { return false; }
        }
        #endregion IsPremiumStyle
        #region IsVariationMarginAuthorized
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual bool IsVariationMarginAuthorized
        {
            get { return false; }
        }
        #endregion IsVariationMarginAuthorized

        #region MaturityDate
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual DateTime MaturityDate
        {
            get { return DateTime.MinValue; }
        }
        #endregion MaturityDate
        #region MaturityDateSys
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual DateTime MaturityDateSys
        {
            get { return DateTime.MinValue; }
        }
        #endregion MaturityDateSys

        #region StoreProcedure_RemoveEOD
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual string StoreProcedure_RemoveEOD
        {
            get { return null; }
        }
        #endregion StoreProcedure_RemoveEOD

        #region VW_TRADE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string VW_TRADE_POS
        {
            get { return string.Empty; }
        }
        #endregion VW_TRADE_POS
        #region VW_TRADE_FUNGIBLE_LIGHT
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string VW_TRADE_FUNGIBLE_LIGHT
        {
            get { return string.Empty; }
        }
        #endregion VW_TRADE_FUNGIBLE_LIGHT
        #region VW_TRADE_FUNGIBLE
        protected virtual string VW_TRADE_FUNGIBLE
        {
            get { return string.Empty; }
        }
        #endregion VW_TRADE_FUNGIBLE
        #region RESTRICT_EVENT_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string RESTRICT_EVENT_POS
        {
            get { return string.Empty; }
        }
        #endregion RESTRICT_EVENT_POS
        #region RESTRICT_EVENTCODE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string RESTRICT_EVENTCODE_POS
        {
            get { return "EVENTCODE = 'STA'"; }
        }
        #endregion RESTRICT_EVENTCODE_POS
        #region RESTRICT_EVENTTYPE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string RESTRICT_EVENTTYPE_POS
        {
            get { return "EVENTTYPE = 'QTY'"; }
        }
        #endregion RESTRICT_EVENTTYPE_POS
        #region RESTRICT_TRDTYPE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string RESTRICT_TRDTYPE_POS
        {
            get { return string.Empty; }
        }
        #endregion RESTRICT_TRDTYPE_POS
        #region RESTRICT_GPRODUCT_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string RESTRICT_GPRODUCT_POS
        {
            get { return string.Empty; }
        }
        #endregion RESTRICT_GPRODUCT_POS
        #region RESTRICT_ISCUSTODIAN
        // EG 20181010 PERF EOD New
        protected virtual string RESTRICT_ISCUSTODIAN
        {
            get { return "0"; }
        }
        #endregion RESTRICT_ISCUSTODIAN
        #region RESTRICT_EMCUSTODIAN_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string RESTRICT_EMCUSTODIAN_POS
        {
            get { return string.Empty; }
        }
        #endregion RESTRICT_EMCUSTODIAN_POS
        #region RESTRICT_ASSET_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string RESTRICT_ASSET_POS
        {
            get { return string.Empty; }
        }
        #endregion RESTRICT_ASSET_POS
        #region RESTRICT_EVENTMERGE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string RESTRICT_EVENTMERGE_POS
        {
            get { return string.Empty; }
        }
        #endregion RESTRICT_EVENTMERGE_POS
        #region RESTRICT_COLMERGE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string RESTRICT_COLMERGE_POS
        {
            get { return string.Empty; }
        }
        #endregion RESTRICT_COLMERGE_POS
        #region RESTRICT_PARTYROLE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected virtual string RESTRICT_PARTYROLE_POS
        {
            get { return string.Empty; }
        }
        #endregion RESTRICT_PARTYROLE_POS
        #region GPRODUCT_VALUE
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        public virtual string GPRODUCT_VALUE
        {
            get { return string.Empty; }
        }
        #endregion GPRODUCT_VALUE


        //────────────────────────────────────────────────────────────────────────────────────────────────
        // COMMON ACCESSORS 
        //────────────────────────────────────────────────────────────────────────────────────────────────


        #region CS
        protected string CS
        {
            get { return m_PKGenProcess.Cs; }
        }
        #endregion CS

        
        #region IsPosKeepingByTrade
        /// <summary>
        /// Tenue de position via IDT 
        /// CAS POSSIBLES:
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>  ● ENTRY / UPDENTRY (demande utilisateur)</para>
        ///<para>  ● Dénouement d'options (demande utilisateur)</para>
        ///<para>  ● Transfert de position (demande utilisateur)</para>
        ///<para>  ● Correction de position (demande utilisateur)</para>
        ///<para>  ● Dénouement automatique d'options (via Traitement EOD)</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
        ///</summary>
        ///FI 20130313 [18467] les denouements d'option par position sont possibles si EndOfDay => add (m_PosRequest.idT > 0)
        ///FI 20131026 [19007] Gestion des denouements d'option par position en mode intraday
        ///                    la property retourne false si m_PKGenProcess.IsRequestDenouementOption et si IDT est non renseigné
        /// EG 20131127 [19254/19239]
        public bool IsPosKeepingByTrade
        {
            get
            {
                return m_PKGenProcess.IsEntryOrRequestUpdateEntry ||
                    (m_PKGenProcess.IsRequestDenouementOption && m_PosRequest.IdT > 0) ||
                        m_PKGenProcess.IsRequestPositionCancelation || m_PKGenProcess.IsRequestPositionTransfer ||
                        m_PKGenProcess.IsRequestRemoveAllocation || m_PKGenProcess.IsRequestSplit ||
                        m_PKGenProcess.IsRequestClearSpec ||
                        (m_PKGenProcess.IsRequestEndOfDay && ((IsRequestOption && m_PosRequest.IdT > 0) || IsRequestPositionCancelation)) ||
                        (IsRequestRemoveUnderlyer && m_PosRequest.IdT > 0);
            }
        }
        #endregion IsPosKeepingByTrade
        #region IsPosKeepingByKeyPos
        /// <summary>
        /// Tenue de position via Clé de position (contraire de via IDT) 
        ///</summary>
        public bool IsPosKeepingByKeyPos
        {
            get { return (false == IsPosKeepingByTrade); }
        }
        #endregion IsPosKeepingByKeyPos
        #region IsQuoteOk
        /// EG 20140116 [19456]
        protected bool IsQuoteOk
        {
            get
            {
                bool isOk;
                if (PosKeepingData.Asset.isOfficialCloseMandatory)
                    isOk = IsQuoteOfficialCloseExist;
                else if (PosKeepingData.Asset.isOfficialSettlementMandatory)
                    isOk = IsQuoteOfficialSettlementExist;
                else
                    isOk = IsQuoteOfficialCloseExist || IsQuoteOfficialSettlementExist;
                return isOk;
            }
        }
        #endregion IsQuoteOk
        #region IsQuoteOfficialCloseExist
        /// <summary>
        /// Le cours Official Close existe
        /// </summary>
        /// EG 20131218 [19353/19355]
        /// EG 20140116 [19456]
        protected bool IsQuoteOfficialCloseExist
        {
            get
            {
                return PosKeepingData.Asset.quoteSpecified;
            }
        }
        #endregion IsQuoteOfficialCloseExist
        #region IsQuoteOfficialSettlementExist
        /// <summary>
        /// Le cours Official Settlement existe
        /// </summary>
        /// EG 20131218 [19353/19355]
        /// EG 20140116 [19456]
        protected bool IsQuoteOfficialSettlementExist
        {
            get
            {
                return PosKeepingData.Asset.quoteReferenceSpecified;
            }
        }
        #endregion IsQuoteOfficialSettlementExist
        #region IsRequestAutomaticDenouementOption
        /// <summary>
        /// Obtient true si m_PosRequest.requestType vaut AutomaticOptionAbandon,AutomaticOptionAssignment ou AutomaticOptionExercise 
        /// </summary>
        private bool IsRequestAutomaticDenouementOption
        {
            get
            {
                return (null != m_PosRequest) &&
                       ((m_PosRequest.RequestType == Cst.PosRequestTypeEnum.AutomaticOptionAbandon) ||
                        (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.AutomaticOptionAssignment) ||
                        (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.AutomaticOptionExercise));
            }
        }
        #endregion IsRequestAutomaticDenouementOption
        #region IsRequestDenouementOption
        /// <summary>
        /// Obtient true si m_PosRequest.requestType vaut OptionAbandon,OptionAssignment ou OptionExercise 
        /// </summary>
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        private bool IsRequestDenouementOption
        {
            get
            {
                return (null != m_PosRequest) &&
                       ((m_PosRequest.RequestType == Cst.PosRequestTypeEnum.OptionAbandon) ||
                        (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.OptionNotExercised) ||
                        (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.OptionNotAssigned) ||
                        (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.OptionAssignment) ||
                        (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.OptionExercise));
            }
        }
        #endregion IsRequestDenouementOption
        #region IsRequestOption
        private bool IsRequestOption
        {
            get
            {
                return (null != m_PosRequest) && (IsRequestDenouementOption || IsRequestAutomaticDenouementOption || IsRequestUnderlyerDelivery);
            }
        }
        #endregion IsRequestOption
        #region IsRequestOptionAbandon
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        protected bool IsRequestOptionAbandon
        {
            get
            {
                return (null != m_PosRequest) &&
                       ((m_PosRequest.RequestType == Cst.PosRequestTypeEnum.AutomaticOptionAbandon) ||
                        (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.OptionAbandon) ||
                        (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.OptionNotExercised) ||
                        (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.OptionNotAssigned));
            }
        }
        #endregion IsRequestOptionAbandon
        #region IsRequestPositionCancelation
        protected bool IsRequestPositionCancelation
        {
            get
            {
                return (null != m_PosRequest) && (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.PositionCancelation);
            }
        }
        #endregion IsRequestPositionCancelation
        #region IsRequestPositionTransfer
        protected bool IsRequestPositionTransfer
        {
            get
            {
                return (null != m_PosRequest) && (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.PositionTransfer);
            }
        }
        #endregion IsRequestPositionTransfer
        #region IsRequestRemoveAllocation
        private bool IsRequestRemoveAllocation
        {
            get
            {
                return (null != m_PosRequest) && (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.RemoveAllocation);
            }
        }
        #endregion IsRequestRemoveAllocation
        #region IsRequestRemoveUnderlyer
        private bool IsRequestRemoveUnderlyer
        {
            get
            {
                return (null != m_PosRequest) && (IsRequestRemoveAllocation);
            }
        }
        #endregion IsRequestOption
        #region IsRequestUnclearing
        protected bool IsRequestUnclearing
        {
            get
            {
                return (null != m_PosRequest) && (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.UnClearing);
            }
        }
        #endregion IsRequestUnclearing
        #region IsRequestUnderlyerDelivery
        private bool IsRequestUnderlyerDelivery
        {
            get
            {
                return (null != m_PosRequest) && (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.UnderlyerDelivery);
            }
        }
        #endregion IsRequestUnderlyerDelivery
        #region IsTradeBuyer
        // EG 20180221 [23769] Public pour gestion Asynchrone
        public bool IsTradeBuyer(string pSide)
        {
            return SideTools.IsFIXmlBuyer(pSide);
        }
        #endregion IsTradeBuyer

        

        #region LstMarketSubPosRequest
        // EG 20180307 [23769] Gestion List<IPosRequest>
        public List<IPosRequest> LstMarketSubPosRequest
        {
            get { return m_LstMarketSubPosRequest; }
        }
        #endregion LstMarketSubPosRequest
        #region LstSubPosRequest
        // EG 20180307 [23769] Gestion List<IPosRequest>
        public List<IPosRequest> LstSubPosRequest
        {
            get { return m_LstSubPosRequest; }
        }
        #endregion LstSubPosRequest

        #region OptionEventCode
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        protected string OptionEventCode
        {
            get
            {
                string eventCode = string.Empty;
                switch (m_PosRequest.RequestType)
                {
                    case Cst.PosRequestTypeEnum.OptionAbandon:
                    case Cst.PosRequestTypeEnum.OptionNotExercised:
                    case Cst.PosRequestTypeEnum.OptionNotAssigned:
                        eventCode = EventCodeFunc.Abandon;
                        break;
                    case Cst.PosRequestTypeEnum.OptionAssignment:
                        eventCode = EventCodeFunc.Assignment;
                        break;
                    case Cst.PosRequestTypeEnum.OptionExercise:
                        eventCode = EventCodeFunc.Exercise;
                        break;
                    case Cst.PosRequestTypeEnum.AutomaticOptionAbandon:
                        eventCode = EventCodeFunc.AutomaticAbandon;
                        break;
                    case Cst.PosRequestTypeEnum.AutomaticOptionAssignment:
                        eventCode = EventCodeFunc.AutomaticAssignment;
                        break;
                    case Cst.PosRequestTypeEnum.AutomaticOptionExercise:
                        eventCode = EventCodeFunc.AutomaticExercise;
                        break;
                }
                return eventCode;
            }
        }
        #endregion OptionEventCode

        #region PosKeepingData
        protected IPosKeepingData PosKeepingData
        {
            set { m_PosKeepingData = value; }
            get { return m_PosKeepingData; }
        }
        #endregion PosKeepingData

        #region PosKeepingEventCode
        protected string PosKeepingEventCode
        {
            get
            {
                string eventCode = string.Empty;
                if (m_PKGenProcess.IsEntry)
                    eventCode = EventCodeFunc.Offsetting;
                else if ((m_PKGenProcess.IsRequest) && (null != m_PosRequest))
                {
                    switch (m_PosRequest.RequestType)
                    {
                        case Cst.PosRequestTypeEnum.ClearingBulk:
                        case Cst.PosRequestTypeEnum.ClearingEndOfDay:
                        case Cst.PosRequestTypeEnum.ClearingSpecific:
                        case Cst.PosRequestTypeEnum.UpdateEntry:
                            eventCode = EventCodeFunc.Offsetting;
                            break;
                        case Cst.PosRequestTypeEnum.PositionCancelation:
                            eventCode = EventCodeFunc.PositionCancelation;
                            break;
                        case Cst.PosRequestTypeEnum.PositionTransfer:
                            eventCode = EventCodeFunc.PositionTransfer;
                            break;
                        case Cst.PosRequestTypeEnum.UnClearing:
                            eventCode = EventCodeFunc.UnclearingOffsetting;
                            break;
                        case Cst.PosRequestTypeEnum.EndOfDay:
                            break;
                    }
                }
                return eventCode;
            }
        }
        #endregion PosKeepingEventCode
        #region ProcessBase
        public ProcessBase ProcessBase
        {
            get { return (ProcessBase)m_PKGenProcess; }
        }
        #endregion ProcessBase

        #region Queue
        public MQueueBase Queue
        {
            get { return m_PKGenProcess.MQueue; }
        }
        #endregion Queue

        #region GProduct
        // EG 20150317 [POC] Add ProductGProduct_MTM
        public string GProduct
        {
            get 
            {
                string gProduct = string.Empty;
                if (this is PosKeepingGen_ETD)
                    gProduct = Cst.ProductGProduct_FUT;
                else if (this is PosKeepingGen_MTM)
                    gProduct = Cst.ProductGProduct_MTM;
                return gProduct; 
            }
        }
        #endregion GProduct

        #region RestoreMarketRequest
        protected IPosRequest RestoreMarketRequest
        {
            get
            {
                return PosKeepingTools.ClonePosRequest(m_MarketPosRequest);
            }
        }
        #endregion RestoreMarketRequest
        #region RestoreMasterRequest
        protected IPosRequest RestoreMasterRequest
        {
            get
            {
                return PosKeepingTools.ClonePosRequest(m_MasterPosRequest);
            }
        }
        #endregion RestoreMasterRequest
        #region RetReverseTradeSide
        protected string RetReverseTradeSide(string pSide)
        {
            return SideTools.RetReverseFIXmlSide(pSide);
        }
        #endregion RetReverseTradeSide

        #region InitializePosRequestByMarket
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20240520 [WI930] Update
        public void InitializePosRequestByMarket(IPosRequest pMasterPosRequest, List<IPosRequest> pLstSubPosRequest, List<IPosKeepingMarket> pLstEntityMarketInfoChangedAfterCRP)
        {
            InitializePosRequestByMarket(pMasterPosRequest, new List<IPosRequest>(), pLstSubPosRequest, pLstEntityMarketInfoChangedAfterCRP);
        }
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20240520 [WI930] Update
        public void InitializePosRequestByMarket(IPosRequest pMasterPosRequest, List<IPosRequest> pLstMarketSubPosRequest, List<IPosRequest> pLstSubPosRequest, List<IPosKeepingMarket> pLstEntityMarketInfoChangedAfterCRP)
        {
            m_MasterPosRequest = pMasterPosRequest;
            m_PosRequest = RestoreMasterRequest;
            m_LstSubPosRequest = pLstSubPosRequest;
            m_LstMarketSubPosRequest = pLstMarketSubPosRequest;
            m_LstEntityMarketInfoChangedAfterCRP = pLstEntityMarketInfoChangedAfterCRP;
        }
        #endregion InitializePosRequestByMarket
        #region SetListeSourceToTarget
        // EG 20190308 Upd TradeMerge|ClosingReopeningPosition
        public void SetListeSourceToTarget(PosKeepingGenProcessBase pPosKeepingGenSource)
        {
            m_LstTradeMergeRule = pPosKeepingGenSource.m_LstTradeMergeRule;
            m_LstClosingReopeningAction = pPosKeepingGenSource.m_LstClosingReopeningAction;
        }
        #endregion SetListeSourceToTarget

        /// <summary>
        /// Identifiant du process dans PROCESS_L
        /// </summary>
        
        public int IdProcess
        {
            get { return m_IdProcess; }
        }

        /// <summary>
        /// Identifiant du tracker dans TRACKER_L
        /// </summary>
        
        public int IdTracker
        {
            get { return m_IdTracker; }
        }
        #endregion Accessors

        #region Constructors
        /// EG 20130719 RequestType appelant (use by ReadQuote)
        // EG 20180502 Analyse du code Correction [CA2214]
        public PosKeepingGenProcessBase(PosKeepingGenProcess pPKGenProcess)
        {
            m_PKGenProcess = pPKGenProcess;
            m_Product = Tools.GetNewProductBase();
            m_PKGenProcess.ProcessCacheContainer = new ProcessCacheContainer(CS, m_Product, pPKGenProcess.RequestType);
            //InitParameters();

            
            m_IdProcess = m_PKGenProcess.IdProcess;
            m_IdTracker = m_PKGenProcess.Tracker.IdTRK_L;

            m_EventQuery = new EventQuery(ProcessBase.Session, m_PKGenProcess.ProcessType, m_IdTracker);
        }
        #endregion Constructors

        #region Methods

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // METHODES PRINCIPALES DE TRAITEMENT
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region Generate
        /// <summary>
        ///<para>Méthode principale de traitement de la tenue de position en fonction du type de message en entrée</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para> 1 ► Appel de PosKeep_EntryGen si message STP de type PosKeepingEntryMQueue </para>
        ///<para>     ● Entrée d'une nouvelle allocation</para>
        ///<para> ou</para>
        ///
        ///<para> 2 ► Appel de PosKeep_RequestGen si message de type PosKeepingRequestMQueue posté </para>
        ///<para>     par une demande utilisateur </para>
        ///<para>    ● Mise à jour des clôtures</para>
        ///<para>    ● Dénouement manuel d'options</para>
        ///<para>    ● Correction de position</para>
        ///<para>    ● Transfert de position</para>
        ///<para>    ● Compensation globale</para>
        ///<para>    ● Clôture et compensation spécifiques</para>
        ///<para>    ● Traitement de fin de journée</para>
        ///<para>    ● Clôture de journée</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        public virtual Cst.ErrLevel Generate()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;

            if (m_PKGenProcess.IsEntry)
            {
                codeReturn = EntryGen();
            }
            else if (m_PKGenProcess.IsRequest)
            {
                codeReturn = RequestGen();
            }

            return codeReturn;
        }
        #endregion Generate
        #region EntryGen
        /// <summary>
        /// TRAITEMENT D'UNE ENTREE D'ALLOCATION (Mode STP)
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Message de type PosKeepingEntryMQueue</para>
        ///<para>  ● Pour un trade donné</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        protected virtual Cst.ErrLevel EntryGen()
        {
            // Si DtBusiness < DtMarket alors pas d'envoi
            Cst.ErrLevel codeReturn = InitPosKeepingData(Queue.id, Cst.PosRequestAssetQuoteEnum.None, true);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                // Traitement si la Date de compensation du trade est >= DtMarket (Trade jour ou future).
                if (DtBusiness >= PosKeepingData.Market.DtMarket)
                {
                    // Contrôle si le message nécessite un recalcul de la position
                    bool isMandatoryPosKeepingCalc = MandatoryPosKeep_Calc(Queue.id);
                    if (isMandatoryPosKeepingCalc)
                    {
                        codeReturn = CommonEntryGen();
                    }
                }
            }
            else if (Cst.ErrLevel.DATAIGNORE == codeReturn)
            {
                codeReturn = Cst.ErrLevel.SUCCESS;
            }
            return codeReturn;
        }
        #endregion EntryGen
        #region RequestGen
        /// <summary>
        /// TRAITEMENT D'UN MESSAGE EN PROVENANCE DE POSREQUEST
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Message de type PosKeepingRequestMQueue</para>
        ///<para>► Lecture dans la table POSREQUEST de la demande associé au message (via IDPR)</para>
        ///<para>APPEL DE METHODES</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► REQUESTTYPE</para>
        ///<para>  ● = CLEARBULK                        ClearingBLKGen</para>
        ///<para>  ● = CLEARSPEC                        ClearingSPECGen</para>
        ///<para>  ● = AAB,ABN,NEX,NAS,AAS,ASS,AEX ou EXE   OptionGen</para>
        ///<para>  ● = POC                              PositionCancelationGen</para>
        ///<para>  ● = POT                              PositionTransferGen</para>
        ///<para>  ● = UNCLEARING                       UnclearingGen</para>
        ///<para>  ● = UPDENTRY                         UpdateEntryGen</para>
        ///<para>  ● = EOD                              EODGen</para>
        ///<para>  ● = CLOSINGDAY                       ClosingGen</para>
        ///<para>  ● = RMVALLOC                         RemoveAllocation</para>
        ///<para>  ● = TRADESPLITTING                   TradeSplitting</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///</summary>
        /// EG 20130704 Lock de POSREQUEST dans le cas d'une demande via NORMMSGFACTOREY (donc avec ID = 0)
        /// FI 20130917 [18953] Gestion des dénouements d'option manuel par position en ITD
        /// FI 20170406 [23053] Modify
        // EG 20180221 [23769] Used collection List<IposRequest> m_LstMarketSubPosRequest|m_LstSubPosRequest
        // EG 20180525 [23979] IRQ Processing
        // EG 20181119 PERF Correction post RC (Pb tabulation Editeur Visual Studio)
        // EG 20190114 Add detail to ProcessLog Refactoring
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        // EG 20220210 [25939] Amélioration du Log sur une demande d'annulation rejetée pour cause de Lock exclusif sur un autre traitement
        protected Cst.ErrLevel RequestGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                // La demande de traitement ne contient pas d'IDPOSREQUEST (Cas possible : Demande via scheduler)
                // Un ligne est donc créée dans POSREQUEST en fonction:
                // 1. du REQUESTTYPE présent dans le message
                // 2. des PARAMETERS présent dans le message
                if (m_PKGenProcess.IsCreatedByNormalizedMessageFactory)
                {
                    m_MasterPosRequest = PosKeepingTools.CreateScheduledPosRequest(CS, null, (PosKeepingRequestMQueue)Queue, m_Product, ProcessBase.Session);
                    m_PKGenProcess.Tracker.UpdateIdData("POSREQUEST", m_MasterPosRequest.IdPR);
                    m_PKGenProcess.CurrentId = m_MasterPosRequest.IdPR;
                    // EG 20151123 ObjectId = string
                    m_PKGenProcess.LockObjectId(m_PKGenProcess.TypeLock, m_MasterPosRequest.IdPR.ToString(), Queue.identifier, Queue.ProcessType.ToString(), LockTools.Exclusive);
                }
                else
                {
                    m_MasterPosRequest = PosKeepingTools.GetPosRequest(CS, m_Product, Queue.id);
                    string tradeIdentifier = Queue.GetStringValueIdInfoByKey("TRADE");
                    if (StrFunc.IsFilled(tradeIdentifier))
                        m_MasterPosRequest.SetIdentifiers(tradeIdentifier);

                }

                if (null != m_MasterPosRequest)
                {
                    m_PosRequest = RestoreMasterRequest;
                    if (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                    {
                        codeReturn = LockProcessByRequest();

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            switch (m_MasterPosRequest.RequestType)
                            {
                                case Cst.PosRequestTypeEnum.ClearingBulk:
                                    codeReturn = ClearingBLKGen(true);
                                    break;
                                case Cst.PosRequestTypeEnum.ClearingSpecific:
                                    codeReturn = ClearingSPECGen();
                                    break;
                                case Cst.PosRequestTypeEnum.OptionAbandon:
                                case Cst.PosRequestTypeEnum.OptionNotExercised:
                                case Cst.PosRequestTypeEnum.OptionNotAssigned:
                                case Cst.PosRequestTypeEnum.OptionAssignment:
                                case Cst.PosRequestTypeEnum.OptionExercise:
                                    if (m_PosRequest.IdT > 0)
                                        codeReturn = RequestTradeOptionGen();
                                    else
                                        codeReturn = RequestPositionOptionGen();
                                    break;
                                case Cst.PosRequestTypeEnum.PositionCancelation:
                                    codeReturn = PositionCancelationGen();
                                    break;
                                case Cst.PosRequestTypeEnum.PositionTransfer:
                                    codeReturn = PositionTransferGen();
                                    break;
                                case Cst.PosRequestTypeEnum.RemoveAllocation:
                                    codeReturn = RemoveAllocationGen();
                                    break;
                                case Cst.PosRequestTypeEnum.UnClearing:
                                    codeReturn = UnclearingGen();
                                    break;
                                case Cst.PosRequestTypeEnum.UpdateEntry:
                                    codeReturn = UpdateEntryGen();
                                    break;
                                case Cst.PosRequestTypeEnum.RemoveEndOfDay:
                                    codeReturn = RemoveEODGen();
                                    break;
                                case Cst.PosRequestTypeEnum.RemoveCAExecuted:
                                    codeReturn = RemoveCAExecutedGen();
                                    break;
                                case Cst.PosRequestTypeEnum.TradeSplitting:
                                    codeReturn = SplitGen();
                                    break;
                                default:
                                    codeReturn = Cst.ErrLevel.DATANOTFOUND;
                                    break;
                            }

                            if (m_MasterPosRequest.IdTSpecified)
                            {
                                if ((null != m_LockObject) && (m_LockObject.ObjectId == m_MasterPosRequest.IdT.ToString()))
                                    if (LockTools.UnLock(CS, m_LockObject, m_PKGenProcess.Session.SessionId))
                                        m_LockObject = null;
                            }
                            else
                            {
                                if (m_MasterPosRequest.IdEMSpecified)
                                {
                                    m_PKGenProcess.UnLockObjectId(m_MasterPosRequest.IdEM);
                                }
                                else
                                {
                                    if (null != m_LockIdEM)
                                    {
                                        m_LockIdEM.ForEach
                                            (
                                                _lock =>
                                                {
                                                    foreach (LockObject item in m_PKGenProcess.LockObject)
                                                    {
                                                        // FI 20170406 [23053] Add restriction ENTITYMARKET
                                                        if (item.ObjectId == _lock.ToString() && (item.ObjectType == TypeLockEnum.ENTITYMARKET))
                                                            LockTools.UnLock(CS, item, m_PKGenProcess.Session.SessionId);

                                                        // FI 20170406 [23053] Mise en commentaire => Trop tôt pour faire un lockObject.Clear()
                                                        //m_PKGenProcess.lockObject.Clear();
                                                    }
                                                }
                                            );
                                    }
                                    // FI 20170406 [23053] Appel ici
                                    m_PKGenProcess.LockObject.Clear();
                                }
                            }
                        }
                        else
                        {
                            DisplayRequestGenError(Cst.ErrLevel.LOCKUNSUCCESSFUL == codeReturn);
                            codeReturn = Cst.ErrLevel.REQUEST_REJECTED;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // FI 20200918 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                Common.AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));

                codeReturn = Cst.ErrLevel.FAILURE;

                /* FI 20200623 [XXXXX] Call AddException
                //FI 20130804 [18882] appel à  SpheresExceptionParser.GetSpheresException => cela permet d'avoir la méthode qui a généré l'exception
                m_PKGenProcess.ProcessState.LastSpheresException = SpheresExceptionParser.GetSpheresException(string.Empty, ex);
                */

                // FI 20130804 [18882] appel à  SpheresExceptionParser.GetSpheresException => cela permet d'avoir la méthode qui a généré l'exception
                // FI 20200623 [XXXXX] Add AddException
                m_PKGenProcess.ProcessState.AddException(SpheresExceptionParser.GetSpheresException(string.Empty, ex));
                // EG 20130723
                //m_PKGenProcess.ProcessState.LastSpheresException = new SpheresException(MethodInfo.GetCurrentMethod().Name,ex);
                // FI 20200623 [XXXXX] SetCodeReturnFromLastException2
                m_PKGenProcess.ProcessState.SetCodeReturnFromLastException2();

                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                    new LogParam(LogTools.IdentifierAndId(GetPosRequestLogValue(((PosKeepingRequestMQueue)Queue).requestType), Queue.id)),
                    new LogParam(Queue.GetStringValueIdInfoByKey("DTBUSINESS"))));

                // FI 20200916 [XXXXX] pas de throw pour eviter double ajout dans le log  de l'exception
                //throw;
            }
            finally
            {
                // EG 20180312 Test (null != m_LstMarketSubPosRequest)
                if ((null != m_LstMarketSubPosRequest) && (0 < m_LstMarketSubPosRequest.Count))
                {
                    m_MasterPosRequest.Status = PosKeepingTools.GetStatusGroupLevel(m_LstMarketSubPosRequest, m_MasterPosRequest.IdPR, ProcessStateTools.StatusUnknownEnum);
                    m_PKGenProcess.ProcessState.SetErrorWarning(m_MasterPosRequest.Status);
                }
                else if (false == ProcessStateTools.IsStatusProgress(m_PKGenProcess.ProcessState.Status))
                {
                    m_MasterPosRequest.Status = m_PKGenProcess.ProcessState.Status;
                }
                else
                {
                    m_MasterPosRequest.Status = (ProcessStateTools.IsCodeReturnSuccess(codeReturn) ? ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusErrorEnum);
                }
                m_MasterPosRequest.StatusSpecified = true;
                // FI 20170406 [23053] ne pas écraser codeReturn
                //codeReturn = UpdatePosRequestTerminated(m_MasterPosRequest.idPR, m_MasterPosRequest.status);
                UpdatePosRequestTerminated(m_MasterPosRequest.IdPR, m_MasterPosRequest.Status);
            }

            return codeReturn;
        }
        #endregion RequestGen
        /// <summary>
        /// - Alimentation du libellé associé au statut IDSTUSEDBY (RESERVED)
        ///   en cas de lock refusé (car un traitement a déjà posé un lock)
        /// - Nouveau n° Log (5111)
        /// </summary>
        /// <param name="pIsUnsuccesfulLock"></param>
        /// EG 20220210 [25939] New
        private void DisplayRequestGenError(bool pIsUnsuccesfulLock)
        {
            int errNumber = 5102;
            if (pIsUnsuccesfulLock)
            {
                errNumber = 5111;
                switch (m_MasterPosRequest.RequestType)
                {
                    case Cst.PosRequestTypeEnum.RemoveAllocation:
                    case Cst.PosRequestTypeEnum.PositionTransfer:
                        UpdateTradeStUsedBy(CS, m_MasterPosRequest.IdT, Cst.StatusUsedBy.RESERVED,
                            m_MasterPosRequest.RequestType.ToString() + "is rejected - Lock unsuccessfull");
                        break;
                    default:
                        break;
                }
            }
            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, errNumber), 0,
                new LogParam(LogTools.IdentifierAndId(GetPosRequestLogValue(((PosKeepingRequestMQueue)Queue).requestType), Queue.id)),
                new LogParam(Queue.GetStringValueIdInfoByKey("DTBUSINESS"))));
        }
        //────────────────────────────────────────────────────────────────────────────────────────────────
        // EOD/CLOSINGDAY VIRTUAL METHODS (OVERRIDE SUR ETD, OTC, COM, MTM)
        //────────────────────────────────────────────────────────────────────────────────────────────────
        #region IsSomeClosingDayControlWillBeSkipped
        /// <summary>
        /// Gestion du marquage d'un traitement de clôture de journée précédemment en erreur 
        /// pour pouvoir débloquer le changement de journée.
        /// 
        /// - Si le POSREQUEST associé au contrôle du marché est en erreur
        /// - Recherche d'existence d'un traitement de clôture de journée opéré précédemment
        ///   respectant les conditions suivantes : 
        ///   - avec statut ERROR
        ///   - avec mêmes critères (ENTITE / MARCHE / DATE)
        ///   - avec une date de mise à jour (POSREQUEST) datant de moins de 10 minutes 
        ///   - marqué dans le tracker par un utilisateur ayant le rôle SYSOPER ou SYSADMIN)
        /// 
        /// Si les conditions sont réunis les contrôles de dénouements automatiques et manuels du traitement de clôture de journée
        /// seront exclus.
        /// Le contrôle du dernier traitement de fin de journée avec le statut ERROR se verra attribuer le statut WARNING.
        /// </summary>
        /// [26063][WI397] Gestion du marquage d'un traitement de clôture de journée précédemment en erreur
        /// pour pouvoir débloquer le changement de journée.
        protected virtual bool IsSomeClosingDayControlWillBeSkipped(IsolationLevel pIsolationLevel, int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness)
        {
            bool ret = false;
            IPosRequest eodControl = m_LstSubPosRequest.Find(item => item.RequestType == Cst.PosRequestTypeEnum.ClosingDay_EndOfDayGroupLevel);
            if (eodControl.StatusSpecified && (eodControl.Status == ProcessStateTools.StatusEnum.ERROR))
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness);
                parameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), pIdEM);
                parameters.Add(new DataParameter(CS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),
                    ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(Cst.PosRequestTypeEnum.ClosingDayMarketGroupLevel));
                parameters.Add(new DataParameter(CS, "DTCONTROL", DbType.DateTime), eodControl.DtIns);

                string sqlSelect = @"select 1
                from dbo.POSREQUEST pr
			    inner join dbo.PROCESS_L pl on (pl.IDPROCESS_L = pr.IDPROCESS_L)
			    inner join dbo.TRACKER_L tk on (tk.IDTRK_L = pl.IDTRK_L)
			    inner join dbo.ACTOR ac on (ac.IDA = tk.IDAMARKED) 
			    inner join dbo.ACTORROLE acr on (acr.IDA = ac.IDA) and (acr.IDROLEACTOR in ('SYSOPER','SYSADMIN'))
                where (pr.REQUESTTYPE = @REQUESTTYPE) and (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) and (pr.STATUS = 'ERROR') and ";

                DbSvrType svrType = DataHelper.GetDbSvrType(CS);

                if (svrType == DbSvrType.dbSQL)
                    sqlSelect += "(DATEDIFF(minute, pr.DTUPD, @DTCONTROL) <= 10)";
                else if (svrType == DbSvrType.dbORA)
                    sqlSelect += "(((@DTCONTROL - pr.DTUPD) * 1440) <= 10)";

                QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);

                IDbTransaction dbTransaction = null;
                bool isException = false;

                try
                {
                    dbTransaction = DataHelper.BeginTran(CS, pIsolationLevel);

                    object obj = DataHelper.ExecuteScalar(CS, dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    ret = (obj != null);
                    if (ret)
                        eodControl.Status = ProcessStateTools.StatusEnum.WARNING;

                    DataHelper.CommitTran(dbTransaction);
                }
                catch (Exception)
                {
                    isException = true;
                    throw;
                }
                finally
                {
                    if (null != dbTransaction)
                    {
                        if (isException) { DataHelper.RollbackTran(dbTransaction); }

                        if (DataHelper.IsDbSqlServer(CS) && (pIsolationLevel != IsolationLevel.ReadCommitted))
                        {
                            //Restauration du niveau d'isolation à Read Commited, car maintenu sur la Connexion !
                            dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadCommitted);
                            DataHelper.CommitTran(dbTransaction);
                        }
                        dbTransaction.Dispose();
                    }
                }
            }
            return ret;
        }
        #endregion IsSomeClosingDayControlWillBeSkipped

        #region CLOSINGDAY_EODControl
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        // EG 20180525 [23979] IRQ Processing
        protected virtual Cst.ErrLevel CLOSINGDAY_EODControl(int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness, int pIdPR_Parent, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CLOSINGDAY_EODControl
        #region CLOSINGDAY_EODMarketGen
        /// <summary>
        /// Méthode principale de traitement de tenue de position de cloture de journée par ENTITE/MARCHE
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Traitement séquentiel</para>
        /// <para>  1 ● Shifting de position</para>
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>/// </summary>
        /// <param name="pIdPR">Id de la ligne de regroupement</param>
        /// <param name="pIdEM">Id ENTITE/MARCHE</param>
        /// <param name="pMarketIdentifier">Identifiant du marché</param>
        /// <param name="pIdM">Id du marché</param>
        /// <param name="pIdM">Id du business center du marché</param>
        /// <returns>Cst.ErrLevel</returns>
        public virtual Cst.ErrLevel CLOSINGDAY_EODMarketGen(int pIdPR, int pIdEM, string pMarketShortAcronym, int pIdM, string pIdBC)
        {
            return CLOSINGDAY_EODMarketGen(pIdPR, pIdEM, pMarketShortAcronym, pIdM, pIdBC, null);
        }
        // EG 20180307 [23769] Gestion List<IPosRequest>
        public virtual Cst.ErrLevel CLOSINGDAY_EODMarketGen(int pIdPR, int pIdEM, string pMarketShortAcronym, int pIdM, string pIdBC, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            m_LstSubPosRequest = new List<IPosRequest>();
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CLOSINGDAY_EODMarketGen

        #region EOD_CascadingGen
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual Cst.ErrLevel EOD_CascadingGen()
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_CascadingGen
        #region EOD_CorporateActionsGen
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual Cst.ErrLevel EOD_CorporateActionsGen(int pIdM)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_CorporateActionsGen

        #region EOD_InitialMarginGen
        /// <summary>
        /// CALCUL DES DEPOSITS
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Lancé APRES le traitement des TOUS les couples ENTITE/MARCHE d'une demande de traitement EOD</para>
        ///<para>► Appel direct du service RISKPERFORMANCE sans postage de message</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20140204 [19586] Gestion des erreurs de type TIMEOUT/DEADLOCK (Relance du traitement InitialMargin avec garde-fou de 5)
        // EG 20180205 [23769] AsyncMode
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel EOD_InitialMarginGen()
        {

            m_PosRequest = RestoreMasterRequest;

            // INSERTION LOG
            MQueueparameter paramEntity = Queue.parameters[PosKeepingRequestMQueue.PARAM_ENTITY] as MQueueparameter;
            MQueueparameter paramCssCustodian = Queue.parameters[PosKeepingRequestMQueue.PARAM_CSSCUSTODIAN] as MQueueparameter;

            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5010), 0,
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));
            Logger.Write();

            // Insert POSREQUEST REGROUPEMENT
            SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusProgressEnum;
            IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_InitialMarginGroupLevel, newIdPR, m_PosRequest, 0,
                status, m_MasterPosRequest.IdPR, m_LstMarketSubPosRequest, null);


            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = CS,
                id = m_PosRequest.IdA_Entity,
                identifier = Queue.GetStringValueIdInfoByKey("ENTITY"),
                requester = Queue.header.requester,
                date = m_MasterPosRequest.DtBusiness
            };
            RiskPerformanceMQueue _queue = new RiskPerformanceMQueue(Cst.ProcessTypeEnum.RISKPERFORMANCE, mQueueAttributes);

            if (DtFunc.IsDateTimeEmpty(_queue.header.creationTimestamp))
                _queue.header.creationTimestamp = OTCmlHelper.GetDateSysUTC(CS);
            _queue.AddParameter(paramEntity);
            _queue.AddParameter(paramCssCustodian);
            _queue.AddParameter(RiskPerformanceMQueue.PARAM_DTBUSINESS, m_PosRequest.DtBusiness);
            _queue.AddParameter(RiskPerformanceMQueue.PARAM_REQUESTTYPE, ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(m_PosRequest.RequestType));
            _queue.AddParameter(RiskPerformanceMQueue.PARAM_TIMING, ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(SettlSessIDEnum.EndOfDay));
            _queue.AddParameter(RiskPerformanceMQueue.PARAM_IDPR, m_PosRequest.IdPR);
            // force ISRESET to true, all the existant trades deposit will be reevaluated
            _queue.AddParameter(RiskPerformanceMQueue.PARAM_ISRESET, true);

            // Timeout / Deadlock / Lock lockUnsucces
            // FI 20210118 [XXXXX] 5 tentatives 
            int nbProcess = 0;
            ProcessState processStateIM = null;
            while (++nbProcess <= 5)
            {
                processStateIM = RiskPerformanceAPI.ExecuteSlaveCall(_queue, m_PKGenProcess, false);

                if (Cst.ErrLevel.TIMEOUT == processStateIM.CodeReturn || Cst.ErrLevel.DEADLOCK == processStateIM.CodeReturn || Cst.ErrLevel.LOCKUNSUCCESSFUL == processStateIM.CodeReturn)
                {
                    // Ecriture de l'info qui indique si retraitement  (Niveau Info, visible si niveau de trace => 3 uniquement)
                    string msgLog = StrFunc.AppendFormat("Error occurred: <b>{0}</b>", processStateIM.CodeReturn);
                    msgLog += Cst.CrLf + StrFunc.AppendFormat("<b>Spheres® {0} to execute again.</b>", (nbProcess < 5) ? "tries" : "doesn't try");

                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, msgLog));

                }
                else
                {
                    //si (nbProcess>1) => il y a erreur et retraitement 
                    if (1 < nbProcess) //Traitement effectué en succès ou en erreur après un DeadLock, Timeout ou Lock unsucessful. Spheres® informe du nbr de tentatives effectuées
                    {
                        string msglog = $"<b>Initial margin calculation process executed after DeadLock or Timeout or Lock unsucessful. Number of attempts: {nbProcess}.</b>";
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, msglog));
                    }

                    break; //Traitement effectué en succès ou en erreur => fin de la boucle
                }
            }

            // si après 5 tentatives sucesssives, il y a encore Timeout / Deadlock / Lock Unsuccessful alors pas de nouvelle tentative et arrêt du traitement en erreur
            if (Cst.ErrLevel.TIMEOUT == processStateIM.CodeReturn ||
                Cst.ErrLevel.DEADLOCK == processStateIM.CodeReturn ||
                Cst.ErrLevel.LOCKUNSUCCESSFUL == processStateIM.CodeReturn)
            {
                // FI 20200623 [XXXXX] Call SetErrorWarning
                processStateIM.SetErrorWarning(ProcessStateTools.StatusEnum.ERROR);
                processStateIM.CodeReturn = Cst.ErrLevel.FAILURE;
            }
            
            // Mise à jour POSREQUEST
            UpdatePosRequestGroupLevel(posRequestGroupLevel, processStateIM.Status);
            
            // Mise à jour Satut du traitement EOD
            m_PKGenProcess.ProcessState.SetErrorWarning(processStateIM.Status);
            
            
            Cst.ErrLevel ret = processStateIM.CodeReturn;
            if (Cst.ErrLevel.IRQ_EXECUTED != processStateIM.CodeReturn)
            {
                if (ProcessStateTools.IsStatusErrorWarning(processStateIM.Status) ||
                    (false == ProcessStateTools.IsCodeReturnSuccess(processStateIM.CodeReturn)))
                    ret = Cst.ErrLevel.FAILURE;
            }
            return ret;
        }

        #endregion EOD_InitialMarginGen

        #region EOD_MarketGen
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        public virtual Cst.ErrLevel EOD_MarketGen(int pIdPR, int pIdEM, string pMarketShortAcronym, int pIdM, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_MarketGen

        #region EOD_PhysicalPeriodicDeliveryGen
        // EG 20170206 [22787] (See PosKeepingGen_XXX - Override)
        protected virtual Cst.ErrLevel EOD_PhysicalPeriodicDeliveryGen()
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_PhysicalPeriodicDeliveryGen

        #region EOD_RecalculationAmountGen
        // EG 20150128 [20726]
        protected virtual Cst.ErrLevel EOD_RecalculationAmountGen()
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_RecalculationAmountGen

        #region EOD_Safekeeping
        // EG 20150708 [21103] New (See PosLeepingGen_OTC - Override : PosKeepingGen_SafeKeeping.cs)
        // RD 20171012 [23502] Move method to partial class in "SpheresClosingGen\PosKeepingGen_SafeKeeping.cs"
        //protected virtual Cst.ErrLevel EOD_Safekeeping()
        //{
        //    return Cst.ErrLevel.SUCCESS;
        //}
        #endregion EOD_Safekeeping
        #region EOD_ShiftingGen
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual Cst.ErrLevel EOD_ShiftingGen(string pIdBC)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_ShiftingGen

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // EOD/CLOSINGDAY COMMON METHODS 
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region EOD_AllocMissing
        /// <summary>
        /// Contrôle de l'existence de trades incomplets dans le traitement EOD : non bloquant (STATUS = WARNING)
        /// <para>Retourne SUCCESS OU IRQ_EXECUTED</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20140204 [19586] IsolationLevel 
        /// EG 20180307 [23769] Gestion List<IPosRequest>
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel EOD_AllocMissing(IPosRequest pPosRequestControl)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            ProcessState _state = new ProcessState(ProcessStateTools.StatusNoneEnum, Cst.ErrLevel.SUCCESS);
            bool isClosingDay = (pPosRequestControl.RequestType == Cst.PosRequestTypeEnum.ClosingDay_EOD_MarketGroupLevel);
            m_PosRequest = pPosRequestControl;

            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            //PL 20220113 Use IsolationLevel.ReadCommitted 
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            #region
            //Une erreur "ORA-08177: can't serialize access for this transaction" est survenue sur chaque EOD sur DB01V19 dans la nuit du 12 ou 13 janvier 2022
            //DB01V19:  Oracle 19.3.0.0.0 / ODP.NET 4.122.19.1;19.3.0.0.0

            //Pour info, cette nuit là une maj Windows était en attente d'un redémarrage: KB5009718 (CU for .NET Framework) et KB5009557 (CU for Windows Server)
            //KB5009718: https://www.catalog.update.microsoft.com/ScopedViewInline.aspx?updateid=148819ed-5f74-4426-a455-38ac288a5e67#PackageDetails

            //Cette erreur peut faire suite à l'usage du code suivant, ce qui est le cas en amont de cette méthode: Create Table, Create Index, insert into Table (cf. EOD_MarketPriceControl)
            //Oracle:  https://support.oracle.com/epmos/faces/DocumentDisplay?_afrLoop=257204655403252&id=1291983.1&_adf.ctrl-state=18e4shhdaw_9

            //J'ai essayé ALTER SYSTEM SET DEFERRED_SEGMENT_CREATION = TRUE, sans succès
            //J'ai essayé de désinstaller le KB5009718, mais malheureusement que partiellement, sans succès
            //J'ai essayé en bypassant l'exécution de EOD_MarketPriceControl, et l'erreur ne survenait plus.
            //La survenue de cette erreur est-elle liée à la maj Windows Server ? On peut fortement le penser car aucune autre maj, mais malheureusement sans certitude réelle...

            //NB: L'usage du mode IsolationLevel.Serializable n'était opéré dans Spheres qu'à cette unique endroit.
            #endregion
            //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.Serializable, Cst.PosRequestTypeEnum.AllocMissing,
            //    m_MasterPosRequest.dtBusiness, m_MarketPosRequest.idEM);
            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.AllocMissing,
                m_MasterPosRequest.DtBusiness, m_MarketPosRequest.IdEM);
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;

                // Insert POSREQUEST REGROUPEMENT
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);

                if (0 < nbRow)
                    _state.Status = ProcessStateTools.StatusProgressEnum;
                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(isClosingDay ? Cst.PosRequestTypeEnum.ClosingDay_AllocMissing : Cst.PosRequestTypeEnum.AllocMissing, newIdPR,
                    m_PosRequest, m_PosRequest.IdEM,
                    _state.Status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

                if (0 < nbRow)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                            break;

                        if (isClosingDay)
                        {
                            // EG 20150218 Add GProduct
                            Cst.ErrLevel ret = RemoveTrade(Convert.ToInt32(row["IDT"]), row["IDENTIFIER"].ToString(), m_PosRequest.DtBusiness,
                                "Deactiv by closing day");
                            if (Cst.ErrLevel.SUCCESS != ret)
                            {
                                _state.Status = ProcessStateTools.StatusErrorEnum;
                                _state.CodeReturn = ret;
                                // FI 20200623 [XXXXX] Call SetErrorWarning
                                m_PKGenProcess.ProcessState.SetErrorWarning(_state.Status);
                            }

                            // INSERTION LOG
                            
                            Logger.Log(new LoggerData(((Cst.ErrLevel.SUCCESS != ret) ? LogLevelEnum.Error : LogLevelEnum.None), new SysMsgCode(SysCodeEnum.LOG, 5076), 2,
                                new LogParam(LogTools.IdentifierAndId(row["IDENTIFIER"].ToString(), Convert.ToInt32(row["IDT"].ToString())))));
                        }
                        else
                        {
                            // FI 20200623 [XXXXX] Call SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                            // INSERTION LOG
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 5074), 1,
                                new LogParam(LogTools.IdentifierAndId(row["IDENTIFIER"].ToString(), Convert.ToInt32(row["IDT"].ToString())))));
                        }
                    }

                    if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) && (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn)))
                    {
                        if (isClosingDay)
                        {
                            if (_state.Status == ProcessStateTools.StatusProgressEnum)
                                _state.Status = ProcessStateTools.StatusSuccessEnum;
                        }
                        else
                        {
                            _state.Status = ProcessStateTools.StatusWarningEnum;

                            // FI 20200623 [XXXXX] Call SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(_state.Status);

                            // INSERTION LOG
                            
                            Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(_state.Status), new SysMsgCode(SysCodeEnum.SYS, 5176), 0,
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                                new LogParam(m_MarketPosRequest.GroupProductValue),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));
                        }
                        posRequestGroupLevel.StatusSpecified = true;
                        posRequestGroupLevel.Status = _state.Status;
                        UpdatePosRequestGroupLevel(posRequestGroupLevel, _state.Status);
                        AddSubPosRequest(m_LstSubPosRequest, posRequestGroupLevel);
                    }
                    else
                    {
                        _state.Status = ProcessStateTools.StatusInterruptEnum;
                        UpdateIRQPosRequestGroupLevel(posRequestGroupLevel);
                    }

                }
            }
            return _state.CodeReturn;
        }
        #endregion EOD_AllocMissing

        #region EOD_CashFlowsGen
        /// <summary>
        /// CALCUL DES MARGES des NEOGICATIONS EN POSITIONS (EOD)
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Calcul/Recalcul des événements de marges des trades jour ou en position veille, à savoir:</para>
        ///<para>  ● VMG : Variation margin</para>
        ///<para>  ● LOV : Liquidative option value</para>
        ///<para>  ● UMG : Unrealized margin</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// <para>Retourne Cst.ErrLevel.IRQ_EXECUTED ou Cst.ErrLevel.FAILURE (si exception) ou Cst.ErrLevel.SUCCESS</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20140204 [19586] IsolationLevel
        /// EG 20150317 [POC] change to virtual (override by MTM)
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        /// EG 20180205 [23769] Asynchrone (Queue EventsVal et Appel en mode Slave SpheresEventsVal)
        /// EG 20180307 [23769] Gestion List de IPosRequest
        /// EG 20180307 [23769] Gestion Asynchrone
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20181127 PERF Post RC (Step 3)
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// EG 20190308 Upd [VCL_Migration] MultiThreading refactoring
        protected virtual Cst.ErrLevel EOD_CashFlowsGen()
        {
            Boolean isException = false;
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            m_PosRequest = RestoreMarketRequest;

            bool isParallelProcess = ProcessBase.IsParallelProcess(ParallelProcess.CashFlows);

            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5008), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                new LogParam(isParallelProcess ? "YES" : "NO"),
                new LogParam(isParallelProcess ? Convert.ToString(m_PKGenProcess.GetHeapSize(ParallelProcess.CashFlows)) + "/" +
                    Convert.ToString(m_PKGenProcess.GetMaxThreshold(ParallelProcess.CashFlows)) : "-")));
            Logger.Write();

            AppInstance.TraceManager.TraceInformation(this, "CashFlowsGen - Start");

            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel,
            //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM, 480, null, null);
            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel,
                m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM, 480, null, null);

            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                // Insert POSREQUEST REGROUPEMENT
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

                // FI 20190709 [24752] Mise en place d'un try catch
                try
                {
                    if (0 < nbRow)
                    {
                        // INSERTION LOG
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5011), 1,
                            new LogParam(nbRow),
                            new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                            new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                            new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                            new LogParam(m_PosRequest.GroupProductValue),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
                        Logger.Write();

                        Pair<Cst.ErrLevel, ProcessState> cashFlowsRet = new Pair<Cst.ErrLevel, ProcessState>();

                        cashFlowsRet = CashFlowsGenMultiThreading(ds.Tables[0].Rows);

                        codeReturn = cashFlowsRet.First;

                        // Calcul des SCU manquant sur Dénouement (ITD)
                        if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
                        {
                            EOD_RecalculationAmountGen();
                            UpdatePosRequestGroupLevel(posRequestGroupLevel, cashFlowsRet.Second.Status);
                        }
                        else
                        {
                            UpdateIRQPosRequestGroupLevel(posRequestGroupLevel);
                        }
                    }
                    else
                    {
                        // Calcul des SCU manquant sur Dénouement (ITD)
                        EOD_RecalculationAmountGen();
                    }
                }
                catch (Exception ex)
                {
                    codeReturn = Cst.ErrLevel.FAILURE;
                    // FI 20200623 [XXXXX] Call SetErrorWarning
                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    // FI 20200623 [XXXXX] Cal AddCriticalException
                    m_PKGenProcess.ProcessState.AddCriticalException(ex);
                    
                    
                    Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                        new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

                    isException = true;

                    UpdatePosRequestGroupLevel(posRequestGroupLevel, ProcessStateTools.StatusEnum.ERROR);
                }
            }

            
            Logger.Write();

            // FI 20190709 [24752] gestion de isException
            AppInstance.TraceManager.TraceInformation(this, "CashFlowsGen - End");
            return (isException ? Cst.ErrLevel.FAILURE : (Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ? Cst.ErrLevel.IRQ_EXECUTED : Cst.ErrLevel.SUCCESS);
        }
        #endregion EOD_CashFlowsGen

        #region EOD_ControlGen
        /// <summary>
        /// Contrôle avant fin de journée 
        /// <para>Retourne Cst.ErrLevel.QUOTENOTFOUND ou Cst.ErrLevel.SUCCES</para>
        /// </summary>
        /// <returns></returns>
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        /// EG 20180307 [23769] Gestion List de IPosRequest
        /// EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel EOD_ControlGen()
        {
            m_PosRequest = RestoreMarketRequest;

            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5001), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            Logger.Write();

            // Insert POSREQUEST REGROUPEMENT
            SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
            IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_ControlGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                ProcessStateTools.StatusProgressEnum, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);
            AddSubPosRequest(m_LstSubPosRequest, posRequestGroupLevel);

            // CONTROLS PRICE
            // EG 20170222 [22717] Controle des prix si traitement non schedulé
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if (false == m_PKGenProcess.IsCreatedByNormalizedMessageFactory)
                codeReturn = EOD_MarketPriceControl(posRequestGroupLevel);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EOD_AllocMissing(posRequestGroupLevel);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                if (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                    codeReturn = EOD_EventMissing(posRequestGroupLevel);
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            UpdatePosRequestGroupLevel(posRequestGroupLevel, ProcessStateTools.StatusSuccessEnum);
            else
                UpdateIRQPosRequestGroupLevel(posRequestGroupLevel);

            m_PosRequest = RestoreMarketRequest;

            return codeReturn;
        }
        #endregion EOD_ControlGen
        #region EOD_ClearingEndOfDay
        /// <summary>
        /// COMPENSATION AUTOMATIQUE (EOD)
        /// <para>Retourne Cst.ErrLevel.IRQ_EXECUTED ou Cst.ErrLevel.SUCCES</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20180307 [23769] Gestion List de IPosRequest
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel EOD_ClearingEndOfDay()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            m_PosRequest = RestoreMarketRequest;

            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5007), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            Logger.Write();

            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.ClearingEndOfDay,
            //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM, 480, null, null);
            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.ClearingEndOfDay,
                m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM, 480, null, null);

            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                // Insert POSREQUEST REGROUPEMENT
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, nbRow + 1);
                ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_ClearingEndOfDayGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    status, m_PosRequest.IdPR, m_LstSubPosRequest, m_PosRequest.GroupProductEnum);

                if (0 < nbRow)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                            break;

                        try
                        {
                            // Insertion POSREQUEST (CLEAREOD)    
                            newIdPR++;
                            m_PosRequest = PosKeepingTools.SetPosRequestClearing(m_Product, Cst.PosRequestTypeEnum.ClearingEndOfDay, SettlSessIDEnum.EndOfDay, dr);
                            InitStatusPosRequest(m_PosRequest, newIdPR, posRequestGroupLevel.IdPR, ProcessStateTools.StatusProgressEnum);

                            // EG 20170109 New currentGroupProduct
                            m_PosRequest.GroupProductSpecified = (m_MarketPosRequest.GroupProductSpecified);
                            if (m_PosRequest.GroupProductSpecified)
                                m_PosRequest.GroupProduct = m_MarketPosRequest.GroupProduct;

                            // Compensation automatique (identique à compensation manuelle)
                            codeReturn = PosKeepingTools.InsertPosRequest(CS, newIdPR, m_PosRequest, m_PKGenProcess.Session);
                            if (Cst.ErrLevel.SUCCESS == codeReturn)
                            {
                                codeReturn = ClearingBLKGen(true);
                            }
                        }
                        catch (Exception)
                        {
                            codeReturn = Cst.ErrLevel.FAILURE;
                            throw;
                        }
                        finally
                        {
                            // Update POSREQUEST (CLEAREOD)    
                            UpdatePosRequest(codeReturn, m_PosRequest, posRequestGroupLevel.IdPR);
                        }
                    }
                    // Update POSREQUEST GROUP
                    if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn) 
                        UpdateIRQPosRequestGroupLevel(posRequestGroupLevel);
                    else
                    UpdatePosRequestGroupLevel(posRequestGroupLevel);
                }
            }
            return (Cst.ErrLevel.IRQ_EXECUTED == codeReturn ? Cst.ErrLevel.IRQ_EXECUTED : Cst.ErrLevel.SUCCESS);
        }
        #endregion EOD_ClearingEndOfDay

        #region EOD_EventMissing
        /// <summary>
        /// Contrôle de l'existence de trades sans événements dans le traitement EOD : non bloquant (STATUS = WARNING)
        /// Retourne toujours SUCCESS
        /// </summary>
        /// <returns></returns>
        /// EG 20140204 [19586] IsolationLevel
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel EOD_EventMissing(IPosRequest pPosRequestControl)
        {
            m_PosRequest = pPosRequestControl;

            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.EventMissing,
            //    m_MasterPosRequest.dtBusiness, m_MarketPosRequest.idEM);
            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.EventMissing,
                m_MasterPosRequest.DtBusiness, m_MarketPosRequest.IdEM);

            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;

                // Insert POSREQUEST REGROUPEMENT
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EventMissing, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);


                if (0 < nbRow)
                {
                    // FI 20200623 [XXXXX] Call SetErrorWarning
                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                    //codeReturn = Cst.ErrLevel.FAILURE;
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {

                        // INSERTION LOG
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 5075), 0,
                            new LogParam(LogTools.IdentifierAndId(row["IDENTIFIER"].ToString(), Convert.ToInt32(row["IDT"].ToString())))));
                    }

                    // INSERTION LOG
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5184), 0,
                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                        new LogParam(m_MarketPosRequest.GroupProductValue),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));

                    posRequestGroupLevel.StatusSpecified = true;
                    posRequestGroupLevel.Status = ProcessStateTools.StatusWarningEnum;
                    UpdatePosRequestGroupLevel(posRequestGroupLevel, ProcessStateTools.StatusWarningEnum);
                    AddSubPosRequest(m_LstSubPosRequest, posRequestGroupLevel);
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_EventMissing

        #region EOD_FeesGen
        /// <summary>
        /// CALCUL DES FRAIS (EOD)
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► STEP 1: Sélection de tous les trades du JOUR s'il existe au moins un trade calculé avec barème dégressif</para> 
        ///<para>► STEP 2: Sélection de tous les trades du JOUR en provenance d'une STRATEGIE</para> 
        ///<para>► STEP 3: Sélection de tous les trades du JOUR sans frais et pour lesquels un contrôle d'absence de frais est spécifié 
        ///<para>          sur le book DEALER (WARNING/BLOQUANT)</para> 
        ///<para>► Re-calcul des frais sur la collection des trades candidats</para> 
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20120319 Ticket 17706 : Calcul des frais manquants 
        /// EG 20130911 [18076] : Barèmes dégressifs - Refactoring
        /// EG 20160208 (POC-MUREX] Refactoring (2 étapes : Calcul des frais et Ecriture des frais)
        /// EG 20180307 [23769] Gestion List<IPosRequest>
        /// EG 20180307 [23769] Gestion Asynchrone
        /// FI 20180314 [XXXXX] Alimentation de la trade des durée de traitement
        // EG 20180502 Analyse du code Correction [CA2200]
        // EG 20180525 [23979] IRQ Processing
        // EG 20181127 PERF Post RC (Step 3)
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190308 Upd [VCL_Migration] MultiThreading refactoring
        protected Cst.ErrLevel EOD_FeesGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            bool isParallelCalculation = ProcessBase.IsParallelProcess(ParallelProcess.FeesCalculation);
            bool isParallelWriting = ProcessBase.IsParallelProcess(ParallelProcess.FeesWriting);

            m_PosRequest = RestoreMarketRequest;
            m_TradeCandidatesForFees = new Dictionary<int, TradeFeesCalculation>();

            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5009), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                new LogParam(isParallelCalculation ? "YES" : "NO"),
                new LogParam(isParallelCalculation ? Convert.ToString(m_PKGenProcess.GetHeapSize(ParallelProcess.FeesCalculation)) + "/" +
                    Convert.ToString(m_PKGenProcess.GetMaxThreshold(ParallelProcess.FeesCalculation)) : "-"),
                new LogParam(isParallelWriting ? "YES" : "NO"),
                new LogParam(isParallelWriting ? Convert.ToString(m_PKGenProcess.GetHeapSize(ParallelProcess.FeesWriting)) + "/" +
                    Convert.ToString(m_PKGenProcess.GetMaxThreshold(ParallelProcess.FeesWriting)) : "-")));
            Logger.Write();

            // Init the control variables when something exists in the trade candidates process queue
            SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusProgressEnum;
            IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_FeesGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                status, m_PosRequest.IdPR, m_LstSubPosRequest, m_PosRequest.GroupProductEnum);
            
            // FI 20210802 [XXXXX] Add try catch
            Boolean isException = false;
            try
            {

                // STEP 1
                if (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                {
                    codeReturn = AddTradesWithDigressiveFees();
                }

                // STEP 2
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = AddTradesFromStg();
                }

                // STEP 3
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = AddTradesWithMissingFees();
                }

                User user = new User(m_PKGenProcess.Session.IdA, null, RoleActor.SYSADMIN);

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    // Process all the trade candidates
                    /// EG 20160208 [POC-MUREX] Calcul des frais
                    /// 
                    if (isParallelCalculation)
                    {
                        // Calcul des frais asynchrone
                        codeReturn = CommonGenThreading(ParallelProcess.FeesCalculation, m_TradeCandidatesForFees.Values.ToList(), user).CodeReturn;
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            //Il existe au moins un book sans frais, disposant d'une vrFee = Error --> on considèrera le traitement comme erroné.
                            if (m_TradeCandidatesForFees.Values.ToList().Exists(item => item.IsNotExistFeesOnBookWithVRError))
                                codeReturn = Cst.ErrLevel.FAILURE;
                            else if (m_TradeCandidatesForFees.Values.ToList().Exists(item => item.IsNotExistFeesOnBookWithVRWarning))
                                codeReturn = Cst.ErrLevel.DATANOTFOUND;
                        }
                    }
                    else
                    {
                        // Calcul des frais synchrone
                        foreach (KeyValuePair<int, TradeFeesCalculation> trade in m_TradeCandidatesForFees)
                        {
                            if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                break;

                            string key = String.Format("(IDT: {0} )", trade.Key);
                            AppInstance.TraceManager.TraceTimeBegin("FeesCalculationGen", key);
                            FeesCalculationGen(trade.Value);
                            AppInstance.TraceManager.TraceTimeEnd("FeesCalculationGen", key);
                        }
                        AppInstance.TraceManager.TraceTimeSummary("FeesCalculationGen");
                    }
                }

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (isParallelWriting)
                    {
                        // Ecriture des frais asynchrone
                        codeReturn = CommonGenThreading(ParallelProcess.FeesWriting, m_TradeCandidatesForFees.Values.ToList(), user).CodeReturn;
                    }
                    else
                    {
                        // Ecriture des frais synchrone
                        IDbTransaction dbTransaction = null;
                        int newIdE = 0;
                        try
                        {
                            dbTransaction = DataHelper.BeginTran(CS);
                            SQLUP.GetId(out newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, m_TradeCandidatesForFees.Sum(item => item.Value.NbEvents));
                            DataHelper.CommitTran(dbTransaction);
                        }
                        catch (Exception) { if (null != dbTransaction) DataHelper.RollbackTran(dbTransaction); throw; }
                        finally { if (null != dbTransaction) dbTransaction.Dispose(); }


                        /// EG 20160208 (POC-MUREX] Ecriture finale
                        Cst.ErrLevel codeReturnFee = Cst.ErrLevel.SUCCESS;
                        foreach (KeyValuePair<int, TradeFeesCalculation> trade in m_TradeCandidatesForFees)
                        {
                            if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                break;

                            string key = String.Format("(IDT: {0} )", trade.Key);
                            AppInstance.TraceManager.TraceTimeBegin("FeesWritingGen", key);
                            codeReturnFee = FeesWritingGen(trade.Value, newIdE);
                            newIdE += trade.Value.NbEvents;
                            if (Cst.ErrLevel.SUCCESS != codeReturnFee)
                            {
                                if (Cst.ErrLevel.FAILURE != codeReturn)
                                    codeReturn = codeReturnFee;
                            }
                            AppInstance.TraceManager.TraceTimeEnd("FeesWritingGen", key);
                        }
                        AppInstance.TraceManager.TraceTimeSummary("FeesWritingGen");
                    }
                }

                if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
                {
                    // Finalize the control variables
                    if (m_TradeCandidatesForFees.Count == 0)
                        status = ProcessStateTools.StatusNoneEnum;
                    else
                    {
                        // EG 20130723
                        switch (codeReturn)
                        {
                            case Cst.ErrLevel.SUCCESS:
                                status = ProcessStateTools.StatusSuccessEnum;
                                break;
                            case Cst.ErrLevel.DATANOTFOUND: // VRFEE = WARNING
                                status = ProcessStateTools.StatusWarningEnum;
                                break;
                            default: // VRFEE = ERROR
                                status = ProcessStateTools.StatusErrorEnum;
                                break;
                        }
                    }
                    // Update POSREQUEST GROUP
                    UpdatePosRequestGroupLevel(posRequestGroupLevel, status);
                }
                else
                    UpdateIRQPosRequestGroupLevel(posRequestGroupLevel);

            }
            catch (Exception ex) 
            {
                // FI 20210802 [XXXXX] Alimentation du log mis à jour du posRequestGroupLevel à l'image des autres étapes

                codeReturn = Cst.ErrLevel.FAILURE;
                // FI 20200623 [XXXXX] Call SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // FI 20200623 [XXXXX] Cal AddCriticalException
                m_PKGenProcess.ProcessState.AddCriticalException(ex);
                
                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                    new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

                isException = true;

                UpdatePosRequestGroupLevel(posRequestGroupLevel, ProcessStateTools.StatusEnum.ERROR);

            }

            m_PosRequest = RestoreMasterRequest;

            
            Logger.Write();

            // FI 20210802 [XXXXX]  si exception => Failure
            return (isException ? Cst.ErrLevel.FAILURE : (Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ? Cst.ErrLevel.IRQ_EXECUTED : Cst.ErrLevel.SUCCESS);
        }
        #endregion EOD_FeesGen

        #region EOD_MarketControl
        /// <summary>
        /// Contrôle du traitement de fin de journée pour un couple ENTITE/MARCHE
        ///</summary>
        /// <param name="pIdPR">Id de la ligne de regroupement</param>
        /// <param name="pIdEM">Id ENTITE/MARCHE</param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20141223 [20605]
        // EG 20171128 [23331] Alimentation du log PROCESSDET_L 
        // EG 20171128 [23331] New signature to posKeepingGenProcessBase.EOD_MarketControl : add entityMarket.marketShortAcronym, entityMarket.idM
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel EOD_MarketControl(int pIdPR, int pIdEM, int pIdPR_Parent, ProductTools.GroupProductEnum? pGroupProduct, string pMarketShortAcronym, int pIdM)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IPosRequest posRequestGroupLevel = null;
            IDataReader dr = null;
            try
            {
                //m_LstSubPosRequest = new List<IPosRequest>();

                // STEP 0 : INITIALISATION POSREQUEST REGROUPEMENT
                posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.ClosingDayMarketGroupLevel, pIdPR, m_PosRequest, pIdEM,
                    ProcessStateTools.StatusProgressEnum, pIdPR_Parent, m_LstMarketSubPosRequest, pGroupProduct);

                m_MarketPosRequest = posRequestGroupLevel;

                // EG 20171128 [23331]
                int idxSubPosRequest = 0;

                if (0 < m_LstSubPosRequest.Count)
                {
                    AddSubPosRequest(m_LstMarketSubPosRequest, m_LstSubPosRequest.Last());
                    idxSubPosRequest = m_LstSubPosRequest.Count;
                }

                // STEP 1 : CONTROLE TRAITEMENT EOD en SUCCESS et COMPLET
                codeReturn = CLOSINGDAY_EODControl(m_PosRequest.IdA_Entity, m_PosRequest.IdA_Css, m_PosRequest.IdA_Custodian, pIdEM, m_PosRequest.DtBusiness, pIdPR, pGroupProduct);

                if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                    UpdateIRQPosRequestGroupLevel(posRequestGroupLevel);
                else
                {
                    UpdatePosRequestGroupLevel(posRequestGroupLevel);
                    // EG 20171128 [23331]
                    // Lecture des POSREQUEST générés lors du contrôle CLOSING DAY pour alimentation du LOG
                    m_LstSubPosRequest.Skip(idxSubPosRequest).ToList().ForEach(subPosRequest =>
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                            (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn)))
                        {
                            if (null != subPosRequest)
                            {
                                ProcessStateTools.StatusEnum _status = subPosRequest.Status;
                                if (ProcessStateTools.IsStatusSuccess(subPosRequest.Status))
                                    _status = ProcessStateTools.StatusEnum.NA;

                                string tradeLog = Cst.NotAvailable;
                                if (subPosRequest.IdTSpecified && subPosRequest.IdentifiersSpecified)
                                    tradeLog = LogTools.IdentifierAndId(subPosRequest.Identifiers.Trade, subPosRequest.IdT);

                                string requestTypeValue = GetPosRequestLogValue(subPosRequest.RequestType);
                                // PosRequest avec PosKeepingKey
                                // UpdateEntry|ClearingEndOfDay|MaturityOffSettingFuture
                                if (subPosRequest.PosKeepingKeySpecified)
                                {
                                    // Récupération des IDENTIFIERS (DEALER, CLEARER et ASSET)
                                    // FI 20180820 [24138] from dbo.DUAL supprimé
                                    // FI 20190228 [24541] il faut DUAL sous Oracle® et dbo.DUAL sous SQLSERVER®  
                                    string @from = string.Empty;
                                    if (DataHelper.IsDbOracle(CS))
                                        @from = "DUAL";
                                    else if (DataHelper.IsDbSqlServer(CS))
                                        @from = "dbo.DUAL"; // from DUAL est remplacé par SString.Empty => Il faut donc bin dbo.DUAL 
                                    else
                                        throw new NotImplementedException("RDBMS not implemented");

                                    // RD 20210624 [25789] Utiliser outer join
                                    string sqlQuery = StrFunc.AppendFormat(@"select ad.IDA as DEALER_IDA, ad.IDENTIFIER as DEALER_IDENTIFIER, 
                                    bd.IDB as DEALER_IDB, bd.IDENTIFIER as BDEALER_IDENTIFIER, 
                                    ac.IDA as CLEARER_IDA, bc.IDA as CLEARER_IDA2, ac.IDENTIFIER as CLEARER_IDENTIFIER, 
                                    bc.IDB as CLEARER_IDB, bc.IDENTIFIER as BCLEARER_IDENTIFIER,
                                    ast.IDENTIFIER as ASSET_IDENTIFIER
                                    from {0}
                                    left outer join dbo.ACTOR ad on (ad.IDA = @IDA_DEALER)
                                    left outer join dbo.ACTOR ac on (ac.IDA = @IDA_CLEARER)
                                    left outer join dbo.BOOK bd on (bd.IDB = @IDB_DEALER)
                                    left outer join dbo.BOOK bc on (bc.IDB = @IDB_CLEARER)
                                    left outer join dbo.VW_ASSET ast on (ast.IDASSET = @IDASSET) and (ast.ASSETCATEGORY = @ASSETCATEGORY)", @from);

                                    DataParameters dp = new DataParameters();
                                    // RD 20210624 [25789] Ajouter un test sur les ID
                                    dp.Add(new DataParameter(CS, "IDA_DEALER", DbType.Int32), subPosRequest.PosKeepingKey.IdA_Dealer > 0 ? subPosRequest.PosKeepingKey.IdA_Dealer : null as object);
                                    dp.Add(new DataParameter(CS, "IDB_DEALER", DbType.Int32), subPosRequest.PosKeepingKey.IdB_Dealer > 0 ? subPosRequest.PosKeepingKey.IdB_Dealer : null as object);
                                    dp.Add(new DataParameter(CS, "IDA_CLEARER", DbType.Int32), subPosRequest.PosKeepingKey.IdA_Clearer > 0 ? subPosRequest.PosKeepingKey.IdA_Clearer : null as object);
                                    dp.Add(new DataParameter(CS, "IDB_CLEARER", DbType.Int32), subPosRequest.PosKeepingKey.IdB_Clearer > 0 ? subPosRequest.PosKeepingKey.IdB_Clearer : null as object);
                                    dp.Add(new DataParameter(CS, "IDASSET", DbType.Int32), subPosRequest.PosKeepingKey.IdAsset > 0 ? subPosRequest.PosKeepingKey.IdAsset : null as object);
                                    dp.Add(new DataParameter(CS, "ASSETCATEGORY", DbType.AnsiString, 64), subPosRequest.PosKeepingKey.UnderlyingAsset.Value);

                                    QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, dp);
                                    dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query.ToString(), qryParameters.Parameters?.GetArrayDbParameter());
                                    if (dr.Read())
                                    {
                                        if (false == subPosRequest.IdentifiersSpecified)
                                            subPosRequest.SetIdentifiers(string.Empty);

                                        subPosRequest.Identifiers.Asset = dr["ASSET_IDENTIFIER"].ToString();
                                        subPosRequest.Identifiers.Dealer = dr["DEALER_IDENTIFIER"].ToString();
                                        subPosRequest.Identifiers.BookDealer = dr["BDEALER_IDENTIFIER"].ToString();
                                        subPosRequest.Identifiers.Clearer = dr["CLEARER_IDENTIFIER"].ToString();
                                        subPosRequest.Identifiers.BookClearer = dr["BCLEARER_IDENTIFIER"].ToString();
                                    }

                                    // FI 20200623 [XXXXX] Call SetErrorWarning
                                    m_PKGenProcess.ProcessState.SetErrorWarning(_status);


                                    
                                    Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(_status), new SysMsgCode(SysCodeEnum.LOG, 5080), 2,
                                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                                        new LogParam(LogTools.IdentifierAndId(subPosRequest.Identifiers.Dealer, subPosRequest.PosKeepingKey.IdA_Dealer) + " - " +
                                            LogTools.IdentifierAndId(subPosRequest.Identifiers.BookDealer, subPosRequest.PosKeepingKey.IdB_Dealer)),
                                        new LogParam(LogTools.IdentifierAndId(subPosRequest.Identifiers.Clearer, subPosRequest.PosKeepingKey.IdA_Clearer) + " - " +
                                            LogTools.IdentifierAndId(subPosRequest.Identifiers.BookClearer, subPosRequest.PosKeepingKey.IdB_Clearer)),
                                        new LogParam(LogTools.IdentifierAndId(pMarketShortAcronym, pIdM)),
                                        new LogParam(subPosRequest.GroupProductValue),
                                        new LogParam(LogTools.IdentifierAndId(subPosRequest.Identifiers.Asset, subPosRequest.PosKeepingKey.IdAsset)),
                                        new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness)),
                                        new LogParam(tradeLog)));
                                }
                                // PosRequest avec IdT
                                // OptionExercise|OptionAbandon|OptionAssignment|UnderlyerDelivery|Entry|RemoveAllocation|AllocMissing|EventMissing
                                else
                                {
                                    // FI 20200623 [XXXXX] Call SetErrorWarning
                                    m_PKGenProcess.ProcessState.SetErrorWarning(_status);

                                    
                                    Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(_status), new SysMsgCode(SysCodeEnum.LOG, 5081), 2,
                                        new LogParam(requestTypeValue),
                                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                                        new LogParam(LogTools.IdentifierAndId(pMarketShortAcronym, pIdM)),
                                        new LogParam(subPosRequest.GroupProductValue),
                                        new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness)),
                                        new LogParam(tradeLog)));
                                }
                            }
                        }
                    });
                }
                return codeReturn;
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                if (null != posRequestGroupLevel)
                    UpdatePosRequest(codeReturn, posRequestGroupLevel, posRequestGroupLevel.IdPR_PosRequest);
                throw;
            }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
        }
        #endregion EOD_MarketControl
        #region EOD_MarketPriceControl
        /// <summary>
        /// Méthode de contrôle de la présence d'un cours de cloture certifié par ENTITY|CLEARINGHOUSE|MARKET
        /// <para>Cst.ErrLevel.QUOTENOTFOUND s'il nexiste pas de prix</para>
        /// </summary>
        /// <param name="pPosRequestControl"></param>
        /// <returns>Cst.ErrLevel.QUOTENOTFOUND s'il nexiste pas de prix</returns>
        /// EG 20170222 [22717] New
        /// EG 20180307 [23769] Gestion List
        /// EG 20180502 Analyse du code Correction [CA2200]
        /// EG 20180803 PERF (ASSETCTRL_{buildTableId}_W sur la base de ASSETCTRL_MODEL à la place de ASSETCONTROL_EOD)
        /// EG 20180803 PERF (TRADEASSETCTRL_{buildTableId}_W sur la base de TRADEASSETCTRL_MODEL -> SQLServer Table pour une unique évaluation dans la clause WITH)
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// EG 20190924 [24949] Replace "ASSETCTRL_MODEL" déplacé après celui de "TRADEASSETCTRL_MODEL"
        /// EG 20240604 [XXXXX] Correctif pb Fusion CS36201
        public Cst.ErrLevel EOD_MarketPriceControl(IPosRequest pPosRequestControl)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                // EG 20231218 [XXXXX] Traitement End Of Day : Lecture de la date de marché DTMARKET
                // pour l'étape de contrôle de l'existence d'un prix par marché
                int? idA_Custodian = null;
                if (m_MarketPosRequest.IdA_CustodianSpecified)
                    idA_Custodian = m_MarketPosRequest.IdA_Custodian;
                m_EntityMarketInfo = m_PKGenProcess.ProcessCacheContainer.GetEntityMarket(m_MarketPosRequest.IdA_Entity, m_MarketPosRequest.IdM, idA_Custodian);

                // INSERTION LOG
                
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5015), 0,
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MarketPosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MarketPosRequest.IdA_CssCustodian)),
                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                    new LogParam(m_MarketPosRequest.GroupProductValue),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_EntityMarketInfo.DtMarket))));

                #region Requête insertion des assets du jour
                // EG 20140225 Jointure BOOK / TRADEINSTRUMENT (IDB_DEALER)
                string sqlInsert = GetQueryInsertPriceControl_EOD();
                #endregion Requête insertion des assets du jour

                if (StrFunc.IsFilled(sqlInsert))
                {
                    bool isMustBeTruncated = false;
                    string buildTableId = PKGenProcess.Session.BuildTableId();
                    string _tableName = StrFunc.AppendFormat("ASSETCTRL_{0}_W", buildTableId).ToUpper();
                    if (false == DataHelper.IsExistTable(CS, _tableName))
                    {
                        DataHelper.CreateTableAsSelect(CS, "ASSETCTRL_MODEL", _tableName);
                        DataHelper.ExecuteNonQuery(CS, CommandType.Text, String.Format("create unique index UX_{0} on dbo.{0} (IDASSET)", _tableName));
                    }
                    else
                    {
                        isMustBeTruncated = true;
                    }

                    // EG 20190924 [24949]
                    //sqlInsert = sqlInsert.Replace("ASSETCTRL_MODEL", _tableName);

                    string _tableName2 = string.Empty;
                    if (DbSvrType.dbSQL == DataHelper.GetDbSvrType(CS))
                    {
                        _tableName2 = StrFunc.AppendFormat("TRADEASSETCTRL_{0}_W", buildTableId).ToUpper();
                        if (false == DataHelper.IsExistTable(CS, _tableName2))
                        {
                            DataHelper.CreateTableAsSelect(CS, "TRADEASSETCTRL_MODEL", _tableName2);
                            DataHelper.ExecuteNonQuery(CS, CommandType.Text, String.Format("create clustered index UX_{0} on dbo.{0} (IDT)", _tableName2));
                        }
                        sqlInsert = sqlInsert.Replace("TRADEASSETCTRL_MODEL", _tableName2);
                    }

                    // EG 20190924 [24949]
                    sqlInsert = sqlInsert.Replace("ASSETCTRL_MODEL", _tableName);

                    m_PosRequest = pPosRequestControl;

                    // Suppression des lignes d'un éventuel précédent traitement 
                    if (isMustBeTruncated)
                        TruncateAssetControl_EOD(_tableName, _tableName2);

                    DataParameters dataParameters = new DataParameters();
                    dataParameters.Add(new DataParameter(CS, "ENTITY", DbType.Int32), m_MarketPosRequest.IdA_Entity);
                    dataParameters.Add(new DataParameter(CS, "CLEARINGHOUSE", DbType.Int32), m_MarketPosRequest.IdA_Css);
                    dataParameters.Add(new DataParameter(CS, "IDM", DbType.Int32), m_MarketPosRequest.IdM);
                    dataParameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), m_EntityMarketInfo.DtMarket);

                    QueryParameters queryParameters = new QueryParameters(CS, sqlInsert, dataParameters);
                    int rowAffected = DataHelper.ExecuteNonQuery(CS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

                    SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                    IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.PriceControl, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                        (0 < rowAffected) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

                    if (0 < rowAffected)
                    {
                        
                        //string logMsg = "LOG-05016";
                        SysMsgCode logMsg = new SysMsgCode(SysCodeEnum.LOG, 5016);

                        ProcessStateTools.StatusEnum status = ProcessStateTools.StatusSuccessEnum;
                        bool isExistPrice = true;
                        // FI 20181123 [24339] Contrôle Effectué uniquement s'il existe des assets en position
                        string queryCount = StrFunc.AppendFormat("select count(1) from dbo.{0}", _tableName);
                        int count = Convert.ToInt32(DataHelper.ExecuteScalar(CS, CommandType.Text, queryCount));
                        if (count > 0)
                        {
                            isExistPrice = IsExistMarketPrice_EOD(m_EntityMarketInfo.DtMarket, _tableName);
                            if (false == isExistPrice)
                            {
                                //logMsg = "SYS-05110";
                                logMsg = new SysMsgCode(SysCodeEnum.SYS, 5110);

                                status = ProcessStateTools.StatusErrorEnum;
                                codeReturn = Cst.ErrLevel.QUOTENOTFOUND;

                                // FI 20200623 [XXXXX] Call SetErrorWarning
                                m_PKGenProcess.ProcessState.SetErrorWarning(status);

                            }
                        }

                        // INSERTION LOG : EXISTENCE ou NON d'au moins un prix 
                        
                        Logger.Log(new LoggerData(((false == isExistPrice) ? LoggerTools.StatusToLogLevelEnum(status) : LogLevelEnum.Info), logMsg, 1,
                            new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MarketPosRequest.IdA_Entity)),
                            new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MarketPosRequest.IdA_CssCustodian)),
                            new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                            new LogParam(m_MarketPosRequest.GroupProductValue),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_EntityMarketInfo.DtMarket))));

                        posRequestGroupLevel.StatusSpecified = true;
                        posRequestGroupLevel.Status = status;
                        UpdatePosRequestGroupLevel(posRequestGroupLevel, status);
                    }

                    AddSubPosRequest(m_LstSubPosRequest, posRequestGroupLevel);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return codeReturn;
        }
        #endregion EOD_MarketPriceControl

        #region EOD_UnclearingMaturityOffsettingGen
        /// EG 20170412 [23081] New EOD_UnclearingMaturityOptionOffsettingGen
        /// EG 20170424 [23064] Rename EOD_UnclearingMaturityOffsettingGen
        protected virtual Cst.ErrLevel EOD_UnclearingMaturityOffsettingGen(int pIdPR_Parent)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_UnclearingMaturityOffsettingGen
        #region EOD_MergeGen
        /// <summary>
        /// MERGES DU JOURS : Point d'entrée
        ///<para>──────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Gestion des fusions du jour</para>
        ///<para>──────────────────────────────────────────────────────────────────────────</para>
        ///<para>──────────────────────────────────────────────────────────────────────────</para>
        /// <para>
        /// MERGES (EOD)
        /// </para>
        /// <para>
        /// 3 fichiers LOG seront constitués (MODE FULL)
        /// </para>
        /// <para>
        /// TradeMerge_IDEM_DTBUSINESS.init.log   = Trades candidats potentiels
        /// </para>
        /// <para>
        /// TradeMerge_IDEM_DTBUSINESS.start.log  = Trades mergeables après application des poids de contextes
        /// </para>
        /// <para>
        /// TradeMerge_IDEM_DTBUSINESS.end.log    = Trades mergés
        /// </para>
        ///<para>──────────────────────────────────────────────────────────────────────────</para>
        ///<para>Retourne Cst.ErrLevel.SUCCES ou Cst.ErrLevel.SUCCES.IRQ_EXECUTED ou  Cst.ErrLevel.FAILURE (si exception)</para>
        /// </summary>
        /// EG 20140326 [19775]  
        /// EG 20180307 [23769] Gestion List de IPosRequest
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel EOD_MergeGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;

            m_PosRequest = RestoreMarketRequest;

            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5025), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            Logger.Write();

            SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);

            // Insertion POSREQUEST (CORPORATEACTION)    
            IPosRequest _posRequestMerge = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.TradeMerging, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                ProcessStateTools.StatusProgressEnum, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

            try
            {

                // Reset des contextes de MERGE (POIDS, IDT candidates et mergeables)
                if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                codeReturn = ResetTradeMergeBeforeProcessing();

                // Chargement des trades du jour et alimentation des contextes par matchage
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                    codeReturn = LoadTradeIntoTradeMergeRules();
                }
                // Assemblage des trades candidats à MERGING présents dans chaque contexte par regroupement 
                // (en fonction de leur clé de merging - Données identiques)
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                    codeReturn = AssociatedTradeCandidates();
                }       
                // Traitement final de merge
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                    codeReturn = TradeMergeGen();
                }
            }
            catch (Exception ex)
            {
                /// EG 20140326 [19775] 
                codeReturn = Cst.ErrLevel.FAILURE;

                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                // FI 20200623 [XXXXX] AddCriticalException
                m_PKGenProcess.ProcessState.AddCriticalException(ex);

                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                    new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            }
            finally
            {
                // Update POSREQUEST
                switch (codeReturn)
                {
                    case Cst.ErrLevel.SUCCESS:
                        _posRequestMerge.Status = ProcessStateTools.StatusSuccessEnum;
                        break;
                    case Cst.ErrLevel.NOTHINGTODO:
                        _posRequestMerge.Status = ProcessStateTools.StatusNoneEnum;
                        codeReturn = Cst.ErrLevel.SUCCESS;
                        break;
                    case Cst.ErrLevel.IRQ_EXECUTED:
                        _posRequestMerge.Status = ProcessStateTools.StatusInterruptEnum;
                        break;
                    default:
                        _posRequestMerge.Status = ProcessStateTools.StatusErrorEnum;
                        break;
                }
                
                //PosKeepingTools.UpdatePosRequest(CS, null, newIdPR, _posRequestMerge, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, m_MarketPosRequest.idPR);
                PosKeepingTools.UpdatePosRequest(CS, null, newIdPR, _posRequestMerge, m_PKGenProcess.Session.IdA, IdProcess, m_MarketPosRequest.IdPR);
            }
            return codeReturn;
        }
        #endregion EOD_MergeGen

        #region EOD_UpdateEntryGen
        /// <summary>
        /// MISE A JOUR DES CLOTURES (encapsulée dans un traitement de fin de journée)
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>1. ► Initialisation des classes de travail (Clé de position...)</para>
        ///<para>2. ► Traitement</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>Retourne Cst.ErrLevel.IRQ_EXECUTED ou Cst.ErrLevel.SUCCESS</para>
        /// </summary>
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        /// EG 20170412 [23081] Add EOD_UnclearingMaturityOptionOffsettingGen
        /// EG 20180307 [23769] Gestion List de IPosRequest
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// EG 20201015 [XXXXX] Correction diverses (N°LOG sur EOD UPDENTRY)
        /// EG 20201124 [XXXXX] Appel à la méthode DeletePosActionWithoutDetail en fin de traitement (auparavant dans PosKeep_Updating)
        protected Cst.ErrLevel EOD_UpdateEntryGen()
        {
             m_PosRequest = RestoreMarketRequest;
             
             
             Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5002), 0,
                 new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                 new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                 new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                 new LogParam(m_PosRequest.GroupProductValue),
                 new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
             Logger.Write();

            // Insert POSREQUEST REGROUPEMENT
            SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusProgressEnum;
            IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_UpdateEntryGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);


            // EG 20170412 [23081] New
            // EG 20170424 [23064] Rename to  EOD_UnclearingMaturityOffsettingGen
            Cst.ErrLevel codeReturn = EOD_UnclearingMaturityOffsettingGen(posRequestGroupLevel.IdPR);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                // PL 20180312 WARNING: Use Read Commited !
                //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.UpdateEntry,
                //    m_PosRequest.dtBusiness, m_PosRequest.idEM, 480, null, null);
                DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.UpdateEntry,
                    m_PosRequest.DtBusiness, m_PosRequest.IdEM, 480, null, null);

                if (null != ds)
                {
                    int nbRow = ds.Tables[0].Rows.Count;
                    SQLUP.GetId(out newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, nbRow);

                    if (0 < nbRow)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                break;

                            try
                            {
                                // Insertion POSREQUEST (UPDENTRY)    
                                // EG 20130703 Remonté avant InitPosKeepingData (pour rentrer dans SetBookDealerInfo)
                                m_PosRequest = PosKeepingTools.SetPosRequestUpdateEntry(m_Product, SettlSessIDEnum.EndOfDay, dr);
                                // Initialisation de la clé de position
                                IPosKeepingKey posKeepingKey = PosKeepingTools.SetPosKeepingKey(m_Product, dr);
                                codeReturn = InitPosKeepingData(null, posKeepingKey, Convert.ToInt32(dr["IDEM"]), Cst.PosRequestAssetQuoteEnum.None, false);
                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    // Insertion POSREQUEST (UPDENTRY)    
                                    InitStatusPosRequest(m_PosRequest, newIdPR, posRequestGroupLevel.IdPR, ProcessStateTools.StatusProgressEnum);

                                    // EG 20170109 New currentGroupProduct
                                    m_PosRequest.GroupProductSpecified = (posRequestGroupLevel.GroupProductSpecified);
                                    if (m_PosRequest.GroupProductSpecified)
                                        m_PosRequest.GroupProduct = posRequestGroupLevel.GroupProduct;

                                    PosKeepingTools.InsertPosRequest(CS, newIdPR, m_PosRequest, m_PKGenProcess.Session);

                                    // Mise à jour des clôtures
                                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                                    {
                                        try
                                        {
                                            codeReturn = CommonEntryGen();
                                        }
                                        catch (Exception ex)
                                        {
                                            SQLErrorEnum err = DataHelper.AnalyseSQLException(CS, ex);
                                            if (err == SQLErrorEnum.DeadLock)
                                                throw new Exception("DeadLock on CommonEntryGen", ex);
                                            else
                                                throw new Exception("Error on CommonEntryGen", ex);
                                        }
                                    }
                                }
                                newIdPR++;

                            }
                            catch (Exception)
                            {
                                codeReturn = Cst.ErrLevel.FAILURE;
                                throw;
                            }
                            finally
                            {
                                UpdatePosRequest(codeReturn, m_PosRequest, posRequestGroupLevel.IdPR);
                            }
                        }
                    }
                }
            }

            // Update POSREQUEST GROUP
            if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                UpdateIRQPosRequestGroupLevel(posRequestGroupLevel);
            else
            UpdatePosRequestGroupLevel(posRequestGroupLevel);

            return (Cst.ErrLevel.IRQ_EXECUTED == codeReturn ? Cst.ErrLevel.IRQ_EXECUTED : Cst.ErrLevel.SUCCESS);
        }
        #endregion EOD_UpdateEntryGen
        #region EOD_UTICalculation
        /// <summary>
        /// UTI CALCULATION
        /// </summary>
        ///<para>──────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Calcul des UTI</para>
        ///<para>──────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// FI 20150206 [20750] Modify
        /// FI 20150213 [XXXXX] Modify
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180307 [23769] Gestion Asynchrone
        // EG 20181127 PERF Post RC (Step 3)
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel EOD_UTICalculation()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;

            m_PosRequest = RestoreMarketRequest;



            // INSERTION LOG
            bool isParallelCalculation = ProcessBase.IsParallelProcess(ParallelProcess.UTICalculation);
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5013), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                new LogParam(isParallelCalculation ? "YES" : "NO"),
                new LogParam(isParallelCalculation ? Convert.ToString(m_PKGenProcess.GetHeapSize(ParallelProcess.FeesCalculation)) + "/" +
                    Convert.ToString(m_PKGenProcess.GetMaxThreshold(ParallelProcess.FeesCalculation)) : "-")));
            Logger.Write();

            SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);

            // Insertion POSREQUEST (UTI)    
            IPosRequest _posRequestUTI = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_UTICalculationGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                ProcessStateTools.StatusProgressEnum, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

            try
            {
                //FI 20150213 [XXXXX]    
                //L'entité doit être RegulatoryOffice => Les UTI/PUTI sont calculés par Spheres® uniquement si l'entité a le rôle RegulatoryOffice
                int IdARegulatoryOfficeRelativeToEntity = RegulatoryTools.GetActorRegulatoryOffice(CSTools.SetCacheOn(CS), m_PosRequest.IdA_Entity);
                Boolean isOk = (IdARegulatoryOfficeRelativeToEntity > 0);

                if (isOk)
                {
                    // PL 20180312 WARNING: Use Read Commited !
                    //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.EOD_UTICalculationGroupLevel,
                    //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM, 480, null, null);
                    DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.EOD_UTICalculationGroupLevel,
                        m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM, 480, null, null);

                    if (null != ds)
                    {
                        int nbRow = ds.Tables[0].Rows.Count;
                        if (0 < nbRow)
                        {
                            
                            Logger.Write();

                            Cst.ErrLevelMessage errMessage = CalcAndRecordUTIThreading(ds.Tables[0]);

                            codeReturn = errMessage.ErrLevel;
                            // FI 20150206 [20750] Add log message
                            if (codeReturn == Cst.ErrLevel.FAILURE)
                            {
                                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                                Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5191), 2, new LogParam(errMessage.Message)));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] AddCriticalException
                m_PKGenProcess.ProcessState.AddCriticalException(ex);

                
                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));

                codeReturn = Cst.ErrLevel.FAILUREWARNING;
            }
            finally
            {
                // Update POSREQUEST (CORPOACTION)
                switch (codeReturn)
                {
                    case Cst.ErrLevel.SUCCESS:
                        _posRequestUTI.Status = ProcessStateTools.StatusSuccessEnum;
                        break;
                    case Cst.ErrLevel.NOTHINGTODO:
                        _posRequestUTI.Status = ProcessStateTools.StatusNoneEnum;
                        break;
                    default:
                        _posRequestUTI.Status = ProcessStateTools.StatusWarningEnum;
                        break;
                }
                
                PosKeepingTools.UpdatePosRequest(CS, null, newIdPR, _posRequestUTI, m_PKGenProcess.Session.IdA, IdProcess, m_MarketPosRequest.IdPR);
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_UTICalculation

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // VIRTUAL METHODS (OVERRIDE SUR ETD / OTC / COMD)
        //────────────────────────────────────────────────────────────────────────────────────────────────
        #region AddTradesWithDigressiveFees
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        // EG 20170106 Refactoring
        protected virtual Cst.ErrLevel AddTradesWithDigressiveFees()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), m_MarketPosRequest.DtBusiness); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_MarketPosRequest.IdEM);

            // Il existe au moins un trade avec frais calculé sur barème dégressif 

            QueryParameters qryParameters = new QueryParameters(CS, GetQueryExistDigressiveFees(), parameters);
            object obj = DataHelper.ExecuteScalar(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
            {
                qryParameters = new QueryParameters(CS, GetQueryDigressiveFees(), parameters);
                // PL 20180312 WARNING: Use Read Commited !
                //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, null);
                DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, null);
                if (null != ds)
                {
                    
                    //AddTradeForFeesCalculation("LOG-05026", ds.Tables[0].Rows);
                    AddTradeForFeesCalculation(new SysMsgCode(SysCodeEnum.LOG, 5026), ds.Tables[0].Rows);
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion AddTradesWithDigressiveFees

        #region CreateTradeMerge
        /// <summary>
        /// Création du trade matérialisant la fusion
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTmr">Contexte de la fusion : TradeMergeRules</param>
        /// <param name="pLstTradeMerge">Liste des treades à fusionner</param>
        /// <param name="pIdTMerge">IdT du trade MERGE</param>
        /// <returns></returns>
        /// EG 20150723 New
        /// EG 20171016 [23509] Upd Tri sur DTEXECUTION
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected virtual Cst.ErrLevel CreateTradeMerge(IDbTransaction pDbTransaction, TradeMergeRule pTmr, List<TradeCandidate> pLstTradeMerge, out int pIdTMerge)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                int idTMerge = 0;
                //// Tri sur DTEXECUTION + IDT  descendant pour prendre l'IDT le plus récent (Trade base pour la fusion)
                TradeCandidate _sourceTradeBase = pLstTradeMerge.OrderByDescending(order => order.DtExecution).ThenByDescending(order => order.IdT).First();

                // Trade de base
                SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CS, _sourceTradeBase.IdT)
                {
                    IsWithTradeXML = true,
                    DbTransaction = pDbTransaction,
                    IsAddRowVersion = true
                };

                // RD 20210304 Add "trx."            
                if (sqlTrade.LoadTable(new string[] { "TRADE.IDT", "IDENTIFIER", "trx.TRADEXML" }))
                {
                    EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(sqlTrade.TradeXml);
                    IDataDocument _dataDoc = (IDataDocument)CacheSerializer.Deserialize(serializerInfo);
                    DataDocumentContainer _dataDocContainer = (DataDocumentContainer)new DataDocumentContainer(_dataDoc).Clone();

                    // EG 20150907 [21317] 
                    if (_dataDocContainer.CurrentProduct.IsEquitySecurityTransaction ||
                        _dataDocContainer.CurrentProduct.IsDebtSecurityTransaction ||
                        _dataDocContainer.CurrentProduct.IsReturnSwap ||
                        _dataDocContainer.CurrentProduct.IsExchangeTradedDerivative)
                    {
                        // Affectation
                        SetTradeMerge(pDbTransaction, _dataDocContainer, pTmr, pLstTradeMerge, _sourceTradeBase);
                        // Enregistrement
                        codeReturn = RecordTradeMerge(pDbTransaction, _dataDocContainer, _sourceTradeBase, pLstTradeMerge, sqlTrade, out idTMerge);
                    }
                }
                else
                {
                    
                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8950), 2,
                        new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                        new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                        new LogParam(m_PosRequest.GroupProductValue),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                        new LogParam(LogTools.IdentifierAndId(_sourceTradeBase.TradeIdentifier, _sourceTradeBase.IdT))));

                    codeReturn = Cst.ErrLevel.FAILURE;
                }
                pIdTMerge = idTMerge;
                return codeReturn;
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                throw;
            }
        }
        #endregion CreateTradeMerge


        #region DeserializeTrade
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual void DeserializeTrade(IDbTransaction pDbTransaction, int pIdT)
        {
            m_TradeLibrary = new EFS_TradeLibrary(CS, pDbTransaction, pIdT);
        }
        #endregion DeserializeTrade

        #region EOD_InitialMarginControl
        // EG 20180307 [23769] New
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected virtual Cst.ErrLevel EOD_InitialMarginControl(int pIdPR_Parent)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IPosRequest posRequestGroupLevel = null;
            try
            {
                // INSERTION LOG
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5073), 1,
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_PosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_PosRequest.IdA_CssCustodian)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));

                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 2);
                // STEP 0 : INITIALISATION POSREQUEST REGROUPEMENT
                posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.ClosingDay_InitialMarginControlGroupLevel, newIdPR, m_PosRequest, 0,
                    ProcessStateTools.StatusProgressEnum, pIdPR_Parent, m_LstSubPosRequest, null);

                posRequestGroupLevel.IdEMSpecified = false;

                // STEP 1 : CONTROLE CALCUL DE DEPOSIT en SUCCESS
                newIdPR++;
                // Exécuter la troisième requête dans la méthode ClosingDay_InitialMarginControl uniquement si ENTITY.ISCLOSINGDAYIMCTRL est à true.
                // C'est pour pouvoir bypasser ce contrôle (chez XI dans notre cas) car la requête récursive génère un timeout.
                bool is_IMCTRL = true;
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CS, "IDA_ENTITY", DbType.Int32), m_PosRequest.IdA_Entity);
                string sqlSelect = "select ISCLOSINGDAYIMCTRL from dbo.ENTITY where IDA = @IDA_ENTITY";
                DataSet ds_imctrl = DataHelper.ExecuteDataset(CS, CommandType.Text, sqlSelect, parameters.GetArrayDbParameter());

                if ((null != ds_imctrl) && ArrFunc.IsFilled(ds_imctrl.Tables[0].Rows))
                {
                    is_IMCTRL = BoolFunc.IsTrue(ds_imctrl.Tables[0].Rows[0]["ISCLOSINGDAYIMCTRL"]);
                }

                if (is_IMCTRL == false)
                {
                    // Message d'info sur la désactivation du contrôle de calcul de déposit
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.SYS, 5077), 2));
                }

                int errCode = ClosingDay_InitialMarginControl(m_PosRequest, is_IMCTRL);

                if (0 < errCode)
                {
                    codeReturn = Cst.ErrLevel.FAILURE;
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, errCode), 2,
                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_PosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_PosRequest.IdA_CssCustodian)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));
                }
                return codeReturn;
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                throw;
            }
            finally
            {
                // Update POSREQUEST GROUP
                UpdatePosRequest(codeReturn, posRequestGroupLevel, pIdPR_Parent);
                // EG [WI762] End of Day processing : Possibility to request processing without initial margin
                // Alimentation du posRequest dans la liste pour contrôle final du statut)
                AddSubPosRequest(m_LstSubPosRequest, posRequestGroupLevel);
            }
        }
        #endregion EOD_InitialMarginControl


        #region GetQueueForEODCashFlows
        protected virtual EventsValMQueue GetQueueForEODCashFlows(DataRow pRow)
        {
            return null;
        }
        #endregion GetQueueForEODCashFlows

        #region InsertEventDet
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20190613 [24683] Upd (PosKeepingData)
        // EG 20190730 Add TypePrice parameter
        protected virtual Cst.ErrLevel InsertEventDet(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, int pIdE, decimal pQty, decimal pContractMultiplier, 
            Nullable<AssetMeasureEnum> pTypePrice, Nullable<decimal> pPrice, Nullable<decimal> pClosingPrice)
        {
            return m_EventQuery.InsertEventDet_Closing(pDbTransaction, pIdE, pQty, pContractMultiplier, pTypePrice, pPrice, pPrice, pClosingPrice, pClosingPrice);
        }
        #endregion InsertEventDet
        #region InsertOptionEvents

        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual Cst.ErrLevel InsertOptionEvents(IDbTransaction pDbTransaction, DataRow pRow, DataRow pRowPosActionDet, int pIdPADET, ref int pIdE)
        {
            return Cst.ErrLevel.NOTHINGTODO;
        }
        #endregion InsertOptionEvents
        #region InsertPremiumRestitutionEvent
        // EG 20130730 [18754] Add parameter (pIsPaymentSettlement)
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        protected virtual Cst.ErrLevel InsertPremiumRestitutionEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event,
            bool pIsDealerBuyer, decimal pQty, bool pIsPaymentSettlement)
        {
            return Cst.ErrLevel.UNDEFINED;
        }
        #endregion InsertPremiumRestitutionEvent


        #region LoadTradeMergeRules
        /// <summary>
        /// Chargement des TRADEMERGERULE (Alimentation de m_LstTradeMergeRule)
        /// <para>Retourne  Cst.ErrLevel.SUCCESS</para>
        /// </summary>
        /// <returns></returns>
        /// EG 20190114 Add detail to ProcessLog Refactoring
        protected virtual Cst.ErrLevel LoadTradeMergeRules()
        {
            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5000), 0,
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), m_PosRequest.DtBusiness); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDA), m_PosRequest.IdA_Entity);

            string sqlSelect = @"select tmr.IDTRADEMERGERULE, tmr.IDENTIFIER, tmr.DISPLAYNAME, tmr.DESCRIPTION, 
            tmr.TYPEPARTY, tmr.IDPARTY, tmr.TYPEINSTR, tmr.IDINSTR, tmr.TYPECONTRACT , tmr.IDCONTRACT,
            tmr.POSITIONEFFECT, tmr.PRICEVALUE
            from dbo.TRADEMERGERULE tmr
            where ( (tmr.DTDISABLED is null) or (tmr.DTDISABLED > @DTBUSINESS)) and (isnull(tmr.IDA,@IDA) = @IDA)" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, null);
            DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, null);
            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                if (0 < nbRow)
                {
                    m_LstTradeMergeRule = new List<TradeMergeRule>();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        m_LstTradeMergeRule.Add(new TradeMergeRule(row));
                    }
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion LoadTradeMergeRules

        #region MandatoryPosKeep_Calc
        /// <summary>
        /// Détermine si un recalcul de la tenue de position est nécessaire suite à l'arrivée d'un trade
        /// <para>L'entrée du trade est à traiter, car elle peut avoir un impact sur la tenue de position, si :</para>
        /// <para>● Le trade n’a pas pris part à une clôture/compensation valide et postérieure à son entrée en portefeuille.</para>
        /// </summary>
        /// <returns></returns>
        protected virtual bool MandatoryPosKeep_Calc(int pIdT)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);
            string sqlSelect = GetQueryMandatoryPosKeep_Calc_Cond1();
            object obj = DataHelper.ExecuteScalar(CS, CommandType.Text, sqlSelect, parameters.GetArrayDbParameter());
            bool isMandatoryPosKeep_Calc = null == obj;
            return isMandatoryPosKeep_Calc;
        }
        #endregion MandatoryPosKeep_Calc
        #region MaturityOffsettingFutureGen
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected virtual Cst.ErrLevel MaturityOffsettingFutureGen(IPosRequest pPosRequestMOF)
        {
            return Cst.ErrLevel.NOTHINGTODO;
        }
        #endregion MaturityOffsettingFutureGen

        #region PhysicalPeriodicDeliveryGen
        // EG 20170206 [22787] New  (See PosKeepingGen_XXX - Override)
        protected virtual Cst.ErrLevel PhysicalPeriodicDeliveryGen(IPosRequest pPosRequestMOF)
        {
            return Cst.ErrLevel.NOTHINGTODO;
        }
        #endregion PhysicalPeriodicDeliveryGen


        #region RequestPositionOptionGen
        protected virtual Cst.ErrLevel RequestPositionOptionGen()
        {
            return Cst.ErrLevel.NOTHINGTODO;
        }
        #endregion RequestPositionOptionGen
        #region RequestTradeOptionGen
        protected virtual Cst.ErrLevel RequestTradeOptionGen()
        {
            return Cst.ErrLevel.NOTHINGTODO;
        }
        #endregion RequestTradeOptionGen

        #region SetPaymentFeesForEvent
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public virtual void SetPaymentFeesForEvent(int pIdT, decimal pQty, IPayment[] pPaymentFees, IPosRequestDetail pDetail)
        {
        }
        #endregion SetPaymentFeesForEvent
        #region SetPosKeepingAsset
        /// <summary>
        /// Alimentation de Asset via un MapDataReaderRow
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pUnderlyingAsset"></param>
        /// <param name="pMapDr"></param>
        /// <returns></returns>
        public virtual PosKeepingAsset SetPosKeepingAsset(IDbTransaction pDbTransaction, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, MapDataReaderRow pMapDr)
        {
            PosKeepingAsset asset = m_Product.CreatePosKeepingAsset(pUnderlyingAsset);
            if (null != asset)
            {
                int idAsset = Convert.ToInt32(pMapDr["IDASSET"].Value);
                string identifier = pMapDr["ASSET_IDENTIFIER"].Value.ToString();

                asset.idAsset = idAsset;
                asset.identifier = identifier;
                asset.contractMultiplier = 1;

                if (null != pMapDr["NOMINALVALUE"])
                    asset.nominal = Convert.IsDBNull(pMapDr["NOMINALVALUE"].Value) ? 0 : Convert.ToDecimal(pMapDr["NOMINALVALUE"].Value);

                if (null != pMapDr["NOMINALCURRENCY"])
                    asset.nominalCurrency = pMapDr["NOMINALCURRENCY"].Value.ToString();

                // FI 20181218 [24410] ajout de .Value
                if (null != pMapDr["IDC"])
                    asset.currency = pMapDr["IDC"].Value.ToString();

                if (null != pMapDr["PRICECURRENCY"])
                    asset.priceCurrency = pMapDr["PRICECURRENCY"].Value.ToString();

                if (null != pMapDr["CONTRACTMULTIPLIER"])
                    asset.contractMultiplier = Convert.IsDBNull(pMapDr["CONTRACTMULTIPLIER"].Value) ? 1 : Convert.ToDecimal(pMapDr["CONTRACTMULTIPLIER"].Value);

                if (null != pMapDr["IDBC"])
                    asset.idBC = pMapDr["IDBC"].Value.ToString();
            }
            return asset;
        }
        #endregion SetPosKeepingAsset

        #region GetQuoteLockWithKeyQuote
        // EG 20190716 [VCL : New FixedIncome] New
        public virtual Quote GetQuoteLockWithKeyQuote(int pIdAsset, DateTime pDate, string pAssetIdentifier,
            Cst.UnderlyingAsset pUnderlyingAsset, ref SystemMSGInfo pErrReadOfficialClose)
        {
            return m_PKGenProcess.ProcessCacheContainer.GetQuoteLock(pIdAsset, pDate, pAssetIdentifier,
                QuotationSideEnum.OfficialClose, pUnderlyingAsset, new KeyQuoteAdditional(), ref pErrReadOfficialClose) as Quote;
        }
        #endregion GetQuoteLockWithKeyQuote
        //────────────────────────────────────────────────────────────────────────────────────────────────
        // COMMON METHODS
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region AddSubPosRequest
        /*
        /// <summary>
        /// Ajoute un élément T à la liste m_LstSubPosRequest avec lock
        /// </summary>
        // EG 20180221 [23769] New
        public void AddSubPosRequest<T>(List<T> pList, T pValue)
        {
            AddSubPosRequest(pList, pValue, false);
        }
        public void AddSubPosRequest<T>(List<T> pList, T pValue, bool pIsInitialize)
        {
            object spinLock = ((ICollection)pList).SyncRoot;
            lock (spinLock)
            {
                if (pIsInitialize && (null == pList))
                    pList = new List<T>();
                pList.Add(pValue);
            }
        }
        */
        /// <summary>
        /// Ajoute un élément T à la liste pList avec lock
        /// </summary>
        /// EG 20180221 [23769] 
        /// FI 20181219 [24399] Nouvelle écriture 
        public static void AddSubPosRequest<T>(List<T> pList, T pValue)
        {
            if (null == pList)
                throw new NullReferenceException("pList Argument is null");

            object spinLock = ((ICollection)pList).SyncRoot;
            lock (spinLock)
            {
                pList.Add(pValue);
            }
        }
        #endregion AddSubPosRequest
        #region AddRangeSubPosRequest
        /*
        /// <summary>
        /// Ajoute une liste à la liste m_LstSubPosRequest avec lock
        /// </summary>
        // EG 20180221 [23769] New
        public void AddRangeSubPosRequest<T>(List<T> pList, List<T> pValue)
        {
            AddRangeSubPosRequest(pList, pValue, false);
        }
        public void AddRangeSubPosRequest<T>(List<T> pList, List<T> pValue, bool pIsInitialize)
        {
            object spinLock = ((ICollection)pList).SyncRoot;
            lock (spinLock)
            {
                if (0 < pValue.Count)
                {
                    if (pIsInitialize && (null == pList))
                        pList = new List<T>();
                    pList.AddRange(pValue);
                }
            }
        }
        */
        /// <summary>
        /// Ajoute une list pValue à la liste pList avec lock
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pList"></param>
        /// <param name="pValue"></param>
        /// EG 20180221 [23769] New
        /// FI 20181219 [24399] Nouvelle écriture 
        public static void AddRangeSubPosRequest<T>(List<T> pList, List<T> pValue)
        {
            if (null == pList)
                throw new NullReferenceException("pList Argument is null");

            object spinLock = ((ICollection)pList).SyncRoot;
            lock (spinLock)
            {
                if (0 < pValue.Count)
                {
                    pList.AddRange(pValue);
                }
            }
        }
        #endregion AddRangeSubPosRequest

        #region AddParameter_IDPR
        // EG 20151125 [20979] Refactoring
        protected void AddParameter_IDPR(DataParameters pParameters)
        {
            Nullable<int> idPR = GetIdPR();
            if (idPR.HasValue)
                pParameters.Add(new DataParameter(CS, "IDPR", DbType.Int32), idPR.Value);
        }
        #endregion AddParameter_IDPR
        #region AddPosRequestCorporateActionTrade
        /// <summary>
        /// Insertion d'un POSREQUEST d'un trade candidat à CA
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para> ► REQUESTTYPE = CORPOACTION </para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pRequestType"></param>
        /// <param name="pDr"></param>
        /// <param name="pNewIdPR"></param>
        /// <param name="pIdPR_Parent"></param>
        /// <returns></returns>
        // EG 20181203 [24360] Add parameter pIsCA (= true pour le traitement des CA (pas de dbTransaction sur Sql_Table - un dataReader est déjà ouvert sur la transaction propre à la CA)
        // EG 20181205 [24360] Refactoring Correction
        // EG 20211028 [XXXXX] Mise à jour du GProduct pour ligne détail CA 
        protected IPosRequest AddPosRequestCorporateActionTrade(IDbTransaction pDbTransaction, Cst.PosRequestTypeEnum pRequestType, DataRow pDr, IPosRequest pPosRequestParent)
        {
            int newIdPR = 0;
            int idPR_Parent = pPosRequestParent.IdPR;
            IPosKeepingKey posKeepingKey = PosKeepingTools.SetPosKeepingKey(m_Product, pDr);
            InitPosKeepingData(pDbTransaction, posKeepingKey, m_MarketPosRequest.IdEM, Cst.PosRequestAssetQuoteEnum.Asset, true);
            IPosRequest posRequest = PosKeepingTools.SetPosRequestCorporateAction(m_Product, pRequestType, pDr);
            posRequest.GroupProductSpecified = pPosRequestParent.GroupProductSpecified;
            posRequest.GroupProduct = pPosRequestParent.GroupProduct;
            InitStatusPosRequest(posRequest, newIdPR, idPR_Parent, ProcessStateTools.StatusProgressEnum);
            
            //PosKeepingTools.AddNewPosRequest(CS, null, out newIdPR, posRequest, m_PKGenProcess.appInstance, LogHeader.IdProcess, idPR_Parent);
            PosKeepingTools.AddNewPosRequest(CS, null, out newIdPR, posRequest, m_PKGenProcess.Session, IdProcess, idPR_Parent);

            if (false == posRequest.IdentifiersSpecified)
            {
                posRequest.Identifiers = m_Product.CreatePosRequestKeyIdentifier();
            }
            posRequest.Identifiers.Trade = pDr["IDENTIFIER"].ToString();
            posRequest.IdPR = newIdPR;
            posRequest.IdCE = pPosRequestParent.IdCE;
            posRequest.IdCESpecified = pPosRequestParent.IdCESpecified;
            return posRequest;
        }
        #endregion AddPosRequestCorporateActionTrade

        #region AddSqlRestrictDealer
        /// <summary>
        ///  Ajoute dans un where SQL des restrictions supplémentaires sur le dealer défini dans PosKeepingData
        ///  <para>les restrictions s'appliquent sur la table VW_TRADE_POSETD (alias tr)</para>
        /// </summary>
        /// <param name="sql"></param>
        /// FI 20130318 [18467] new method sur les assignations, le dealer n'est pas nécessairement connu
        /// C'est notamment le cas lors des assignations issues de la chambre de compensation
        protected void AddSqlRestrictDealer(ref string sql)
        {
            bool isDefaultBehavior = true;

            if (null != m_PosRequest)
            {

                switch (m_PosRequest.RequestType)
                {
                    case Cst.PosRequestTypeEnum.OptionAssignment:
                        if (PosKeepingData.IdA_Dealer > 0)
                            sql += SQLCst.AND + "(tr.IDA_DEALER = @IDA_DEALER)";
                        if (PosKeepingData.IdB_Dealer > 0)
                            sql += SQLCst.AND + "(tr.IDB_DEALER = @IDB_DEALER)";
                        if (PosKeepingData.IdA_EntityDealer > 0)
                            sql += SQLCst.AND + "(tr.IDA_ENTITYDEALER = @IDA_ENTITYDEALER)";
                        isDefaultBehavior = false;
                        break;
                    default:
                        isDefaultBehavior = true;
                        break;
                }
            }
            if (isDefaultBehavior)
            {
                sql += SQLCst.AND + "(tr.IDA_DEALER = @IDA_DEALER)" + SQLCst.AND + "(tr.IDB_DEALER = @IDB_DEALER)" + Cst.CrLf;
            }
        }
        #endregion AddSqlRestrictDealer
        #region AddTradeForFeesCalculation
        /// <summary>
        /// Alimentation du dictionnaire "m_TradeCandidatesForFees" des trades candidats à calcul des frais (EOD)
        /// </summary>
        /// <param name="pLogNumber"></param>
        /// <param name="pRow"></param>
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190308 Upd [VCL_Migration] MultiThreading refactoring
        //protected void AddTradeForFeesCalculation(string pLogNumber, DataRowCollection pRow)
        protected void AddTradeForFeesCalculation(SysMsgCode pLogNumber, DataRowCollection pRow)
        {
            if (0 < pRow.Count)
            {
                IEnumerable<int> nbIdT = pRow.Cast<DataRow>().Select(row => Convert.ToInt32(row["IDT"])).Distinct();

                
                Logger.Log(new LoggerData(LogLevelEnum.None, pLogNumber, 1,
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                    new LogParam(m_PosRequest.GroupProductValue),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                    new LogParam(nbIdT.Count())));
                Logger.Write();

                foreach (DataRow row in pRow)
                {
                    int idT = Convert.ToInt32(row["IDT"]);
                    TradeFeesCalculation tradeFeesCalculation = null;
                    ActorFeesCalculation actorFeesCalculation = null;
                    if (false == m_TradeCandidatesForFees.ContainsKey(idT))
                    {
                        // Alimentation Trade dans dictionnaire
                        tradeFeesCalculation = new TradeFeesCalculation
                        {
                            idT = idT,
                            identifier = row["IDENTIFIER"].ToString(),
                            idE_Event = Convert.ToInt32(row["IDE_EVENT"]),
                            qty = Convert.ToDecimal(row["QTY"])
                        };
                        m_TradeCandidatesForFees.Add(idT, tradeFeesCalculation);
                    }
                    tradeFeesCalculation = m_TradeCandidatesForFees[idT];
                    if (null == tradeFeesCalculation.actorFee)
                        tradeFeesCalculation.actorFee = new Dictionary<int, ActorFeesCalculation>();

                    int idB = Convert.ToInt32(row["IDB"]);
                    if (false == tradeFeesCalculation.actorFee.ContainsKey(idB))
                    {
                        // Alimentation Acteur frais dans dictionnaire
                        actorFeesCalculation = new ActorFeesCalculation
                        {
                            idB = idB,
                            idA = Convert.ToInt32(row["IDA"]),
                            actorIdentifier = row["ACTOR_IDENTIFIER"].ToString(),
                            bookIdentifier = row["BOOK_IDENTIFIER"].ToString(),
                            idRoleActor = row["IDROLEACTOR"].ToString()
                        };

                        if ((row["VRFEE"] != null) && (Enum.IsDefined(typeof(Cst.CheckModeEnum), row["VRFEE"].ToString())))
                            actorFeesCalculation.vrFee = (Cst.CheckModeEnum)Enum.Parse(typeof(Cst.CheckModeEnum), Convert.ToString(row["VRFEE"]), true);
                        tradeFeesCalculation.actorFee.Add(idB, actorFeesCalculation);
                    }
                }
            }
        }
        #endregion AddTradeForFeesCalculation

        #region ClearingBLKGen
        /// <summary>
        /// COMPENSATION AUTOMATIQUE (EOD / INTRADAY) 
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para> ► Message de type PosKeepingRequestMQueue</para> 
        ///<para>   ● REQUESTYPE = CLEARBULK</para>
        ///<para>   ● REQUESTYPE = EOD (Cas de la liquidation de future à l'échéance : Compensation manuelle)</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pReloadPosKeepingData">Flag pour recharge des caractéristiques</param>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20140311 [19734][19702]
        /// EG 20170315 [22967] m_PosRequest.groupProductValue instead of m_MarketPosRequest.groupProductValue
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel ClearingBLKGen(bool pReloadPosKeepingData)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            // Lecture de la clé de position et de la quantité à compenser                            
            if (pReloadPosKeepingData)
            {
                codeReturn = InitPosKeepingData(null, m_PosRequest.PosKeepingKey, Cst.PosRequestAssetQuoteEnum.None, false);
            }

            // INSERTION LOG
            string market = "-";
            string asset = "-";
            #region MARKET/ASSET
            if (StrFunc.IsFilled(Queue.GetStringValueIdInfoByKey("MARKET")))
                market = Queue.GetStringValueIdInfoByKey("MARKET");
            else if (m_PosRequest.IdentifiersSpecified)
                market = m_PosRequest.Identifiers.Market;
            else if (m_MarketPosRequest.IdentifiersSpecified)
                market = m_MarketPosRequest.Identifiers.Market;

            if (StrFunc.IsFilled(Queue.GetStringValueIdInfoByKey("ASSET")))
                asset = Queue.GetStringValueIdInfoByKey("ASSET");
            else if (m_PosRequest.IdentifiersSpecified)
                asset = m_PosRequest.Identifiers.Asset;
            if (StrFunc.IsFilled(Queue.GetStringValueIdInfoByKey("ASSET")))
                asset = Queue.GetStringValueIdInfoByKey("ASSET");
            else if (null != PosKeepingData)
                asset = PosKeepingData.Asset.identifier;
            #endregion

            // EG 20131216 [19329/19345] Prise en compte des paramètrages sur BOOKPOSEFCT
            if (PosKeepingData.IsIgnorePositionEffect_BookDealer && ExchangeTradedDerivativeTools.IsPositionEffect_Open(PosKeepingData.PositionEffect_BookDealer))
            {
                //Tous les trades sont à considérer comme "Open"

                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5064), 1,
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_PosRequest.IdA_Entity)),
                    new LogParam(market),
                    new LogParam(m_PosRequest.GroupProductValue),
                    new LogParam(asset),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                    new LogParam(m_PosRequest.Qty)));

                codeReturn = Cst.ErrLevel.NOTHINGTODO;
            }
            else if ((false == PosKeepingData.IsClearingEOD_BookDealer) && (Cst.PosRequestTypeEnum.ClearingEndOfDay == m_PosRequest.RequestType))
            {
                // EG 20140311 [19734][19702] Compensation automatique EOD non autorisée par référentiel - Prise en compte des paramètrages sur BOOK/BOOKPOSEFCT
                PosRequestPositionSetIdentifiers(CS, m_PosRequest);
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5065), 1,
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_PosRequest.IdA_Entity)),
                    new LogParam(market),
                    new LogParam(m_PosRequest.GroupProductValue),
                    new LogParam(asset),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.BookDealer, m_PosRequest.PosKeepingKey.IdB_Dealer)),
                    new LogParam(m_PosRequest.Qty)));

                codeReturn = Cst.ErrLevel.NOTHINGTODO;
            }
            else
            {
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5058), 1,
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_PosRequest.IdA_Entity)),
                    new LogParam(market),
                    new LogParam(m_PosRequest.GroupProductValue),
                    new LogParam(asset),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                    new LogParam(m_PosRequest.Qty)));

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    DataSet ds = GetPositionDetailAndPositionAction(m_PosRequest.DtBusiness);
                    m_DtPosition = ds.Tables[0];

                    m_DtPosActionDet = ds.Tables[1];
                    m_DtPosActionDet_Working = ds.Tables[1].Clone();
                    m_DtPosition.PrimaryKey = new DataColumn[] { m_DtPosition.Columns["IDT"] };

                    if (ExchangeTradedDerivativeTools.IsPositionEffect_HILO(this.PosKeepingData.PositionEffect_BookDealer))
                    {
                        //Compensation HILO - Position et Trade Jour 
                        codeReturn = ClearingBLKCalc_HILO(DateTime.MinValue);
                    }
                    else
                    {
                        codeReturn = ClearingBLKCalc();
                    }

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        MergePosActionWorkingBeforeUpdating();

                        int idPA = 0;
                        codeReturn = PosKeep_Updating(m_PosRequest.NbTokenIdE, ref idPA);
                    }
                }
            }
            return codeReturn;
        }
        #endregion ClearingBLKGen
        #region ClosingDayControlDetail
        /// <summary>
        /// ALIMENTATION DES LIGNES DETAIL DEs CONTROLES DE CLOTURE DE JOURNEE
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pProduct">Product</param>
        /// <param name="pIdPR">Id POSREQUEST</param>
        /// <param name="pIdA_Entity">Id Entité</param>
        /// <param name="pIdA_CSS">Id Chambre</param>
        /// <param name="pIdEM">Id du couple ENTITE/MARCHE</param>
        /// <param name="pDtBusiness">Date de journée</param>
        /// <param name="pRows">Lignes concernées (DataRow) concernées par le contrôle</param>
        /// <param name="pRequestTypeGroupLevel">Type de contrôle</param>
        /// <param name="pIdPR_Parent">Id du POSREQUEST parent</param>
        /// <param name="pAppInstance">Application /Id de l'utilisateur à la source de la demande</param>
        /// <param name="pIdProcess">Id du Log associé au traitement</param>
        /// <param name="pSubRequest">Liste des traitements déjà exécutés</param>
        /// <returns>Code retour</returns>
        // EG 20180221 [23769] Déplacé de PosKeepingTools
        public Cst.ErrLevel ClosingDayControlDetail(int pIdPR, int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness,
            DataRowCollection pRows, Cst.PosRequestTypeEnum pRequestTypeGroupLevel, int pIdPR_Parent)
        {
            return ClosingDayControlDetail(pIdPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, pRows, pRequestTypeGroupLevel, pIdPR_Parent, null);
        }
        // EG 20180221 [23769] Déplacé de PosKeepingTools
        // EG 20180525 [23979] IRQ Processing
        // EG 20180620 Gestion Status Contrôle des frais en fonction VRFEE
        // EG 20190308 Upd UTI status always success
        // EG 20190121 [MIGRATION VCL] Gestion Status SKP
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        public Cst.ErrLevel ClosingDayControlDetail(int pIdPR, int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness,
            DataRowCollection pRows, Cst.PosRequestTypeEnum pRequestTypeGroupLevel, int pIdPR_Parent, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn)))
            {
            int nbRow = 0;
            if (ArrFunc.IsFilled(pRows))
                nbRow = pRows.Count;

            ProcessStateTools.StatusEnum status = (0 == nbRow) ? ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusErrorEnum;
            Cst.PosRequestTypeEnum requestTypeItem = Cst.PosRequestTypeEnum.None;
            switch (pRequestTypeGroupLevel)
            {
                case Cst.PosRequestTypeEnum.ClosingDay_EndOfDayGroupLevel:
                    // EG 20140120 Report 3.7
                    //status = (1 == nbRow) ? ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusErrorEnum;
                    if (1 == nbRow)
                    {
                        status = (ProcessStateTools.StatusEnum)ReflectionTools.EnumParse(new ProcessStateTools.StatusEnum(), pRows[0]["STATUS"].ToString());
                    }
                    else
                    {
                        status = ProcessStateTools.StatusErrorEnum;
                    }
                    requestTypeItem = Cst.PosRequestTypeEnum.None;
                    break;
                case Cst.PosRequestTypeEnum.EOD_ManualOptionGroupLevel:
                    requestTypeItem = Cst.PosRequestTypeEnum.OptionExercise;
                    break;
                case Cst.PosRequestTypeEnum.EOD_AutomaticOptionGroupLevel:
                    requestTypeItem = Cst.PosRequestTypeEnum.MaturityOffsettingOption;
                    break;
                case Cst.PosRequestTypeEnum.EOD_UnderlyerDeliveryGroupLevel:
                    requestTypeItem = Cst.PosRequestTypeEnum.UnderlyerDelivery;
                    break;
                case Cst.PosRequestTypeEnum.EOD_MaturityOffsettingFutureGroupLevel:
                    requestTypeItem = Cst.PosRequestTypeEnum.MaturityOffsettingFuture;
                    break;
                // EG 20170206 [22787]
                case Cst.PosRequestTypeEnum.EOD_PhysicalPeriodicDelivery:
                    // Test sur STATUS retourné 
                    status = (0 == nbRow) ? ProcessStateTools.StatusSuccessEnum :
                        (ProcessStateTools.StatusEnum)ReflectionTools.EnumParse(new ProcessStateTools.StatusEnum(), pRows[0]["STATUS"].ToString());
                    requestTypeItem = Cst.PosRequestTypeEnum.None;
                    break;
                case Cst.PosRequestTypeEnum.EOD_UpdateEntryGroupLevel:
                    requestTypeItem = Cst.PosRequestTypeEnum.UpdateEntry;
                    break;
                case Cst.PosRequestTypeEnum.EOD_ClearingEndOfDayGroupLevel:
                    requestTypeItem = Cst.PosRequestTypeEnum.ClearingEndOfDay;
                    break;
                case Cst.PosRequestTypeEnum.ClosingDay_EntryGroupLevel:
                    requestTypeItem = Cst.PosRequestTypeEnum.Entry;
                    // EG 20140206 Add status
                    status = (0 == nbRow) ? ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusErrorEnum;
                    break;
                case Cst.PosRequestTypeEnum.ClosingDay_RemoveAllocationGroupLevel:
                    // FI 20160819 [22364] Add
                    requestTypeItem = Cst.PosRequestTypeEnum.RemoveAllocation;
                    break;
                case Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel:
                    // EG 2014027[19588] Test sur STATUS retourné 
                    //status = (0 == nbRow) ? ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusErrorEnum;
                    status = (0 == nbRow) ? ProcessStateTools.StatusSuccessEnum :
                        (ProcessStateTools.StatusEnum)ReflectionTools.EnumParse(new ProcessStateTools.StatusEnum(), pRows[0]["STATUS"].ToString());
                    requestTypeItem = Cst.PosRequestTypeEnum.None;
                    break;
                case Cst.PosRequestTypeEnum.EOD_CascadingGroupLevel:
                    requestTypeItem = Cst.PosRequestTypeEnum.Cascading;
                    break;
                case Cst.PosRequestTypeEnum.AllocMissing:
                    requestTypeItem = Cst.PosRequestTypeEnum.AllocMissing;
                    break;
                // EG 20140120 Report 3.7 New
                case Cst.PosRequestTypeEnum.EventMissing:
                    requestTypeItem = Cst.PosRequestTypeEnum.EventMissing;
                    status = (0 == nbRow) ? ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusWarningEnum;
                    break;
                case Cst.PosRequestTypeEnum.EOD_SafekeepingGroupLevel:
                    requestTypeItem = Cst.PosRequestTypeEnum.None;
                    //status = (0 == nbRow) ? ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusWarningEnum;
                    if (0 == nbRow)
                    {
                        status = ProcessStateTools.StatusSuccessEnum;
                    }
                    else
                    {
                        // EG 20190320 Upd Mise à jour du status
                        List<DataRow> rows = (from DataRow item in pRows.Cast<DataRow>() select item).ToList();
                        // Le trade n'est plus en position et il a un SKP
                        if (rows.Exists(row => (false == Convert.IsDBNull(row["IDE_SKP"])) && (0 == Convert.ToDecimal(row["QTY"]))))
                            status = ProcessStateTools.StatusErrorEnum;
                        else
                            status = ProcessStateTools.StatusSuccessEnum;
                    }

                    break;
                case Cst.PosRequestTypeEnum.EOD_UTICalculationGroupLevel:
                    requestTypeItem = Cst.PosRequestTypeEnum.None;
                    status = ProcessStateTools.StatusSuccessEnum;
                    break;
                case Cst.PosRequestTypeEnum.EOD_FeesGroupLevel:
                    requestTypeItem = Cst.PosRequestTypeEnum.None;
                    if (0 == nbRow)
                    {
                        status = ProcessStateTools.StatusSuccessEnum;
                    }
                    else
                    {
                        List<DataRow> rows = (from DataRow item in pRows.Cast<DataRow>() select item).ToList();
                        if (rows.Exists(row => row["VRFEE"].ToString().ToLower() == "error"))
                            status = ProcessStateTools.StatusErrorEnum;
                        else if (rows.Exists(row => row["VRFEE"].ToString().ToLower() == "warning"))
                            status = ProcessStateTools.StatusWarningEnum;
                        else
                            status = ProcessStateTools.StatusSuccessEnum;
                    }
                    break;
                default:
                    requestTypeItem = Cst.PosRequestTypeEnum.None;
                    break;
            }

            // Insert POSREQUEST GROUP
            IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(pRequestTypeGroupLevel, pIdPR, 
                pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, status, pIdPR_Parent, m_LstSubPosRequest, pGroupProduct);

            if ((Cst.PosRequestTypeEnum.None != requestTypeItem) && (0 < nbRow))
            {
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, nbRow);

                IPosRequest posRequest = m_Product.CreatePosRequestClosingDayControl(requestTypeItem,
                    pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness);
                int idI = 0;
                int idAsset = 0;
                int idA_Dealer = 0;
                int idB_Dealer = 0;
                int idA_Clearer = 0;
                int idB_Clearer = 0;
                string identifier = string.Empty;

                foreach (DataRow row in pRows)
                {
                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                            break;


                    posRequest.IdPR = newIdPR;
                    posRequest.Status = status;

                    if (row.Table.Columns.Contains("STATUS"))
                        posRequest.Status = (ProcessStateTools.StatusEnum)ReflectionTools.EnumParse(new ProcessStateTools.StatusEnum(), row["STATUS"].ToString());

                    posRequest.StatusSpecified = true;
                    posRequest.IdPR_PosRequest = pIdPR;
                    posRequest.IdPR_PosRequestSpecified = true;
                    posRequest.PosKeepingKeySpecified = false;
                    posRequest.IdTSpecified = false;

                    posRequest.GroupProductSpecified = (pGroupProduct.HasValue);
                    if (posRequest.GroupProductSpecified)
                        posRequest.GroupProduct = pGroupProduct.Value;

                    Nullable<Cst.UnderlyingAsset> _underlyingAsset = PosKeepingTools.GetUnderLyingAssetRelativeToInstrument(row);

                    switch (requestTypeItem)
                    {
                        case Cst.PosRequestTypeEnum.Entry:
                        case Cst.PosRequestTypeEnum.MaturityOffsettingOption:
                        case Cst.PosRequestTypeEnum.OptionAbandon:
                        case Cst.PosRequestTypeEnum.OptionNotExercised:
                        case Cst.PosRequestTypeEnum.OptionNotAssigned:
                        case Cst.PosRequestTypeEnum.OptionAssignment:
                        case Cst.PosRequestTypeEnum.OptionExercise:
                        case Cst.PosRequestTypeEnum.UnderlyerDelivery:
                        case Cst.PosRequestTypeEnum.AllocMissing:
                        case Cst.PosRequestTypeEnum.RemoveAllocation:
                        case Cst.PosRequestTypeEnum.EventMissing:
                            posRequest.IdTSpecified = (false == Convert.IsDBNull(row["IDT"]));
                            if (posRequest.IdTSpecified)
                            {
                                posRequest.IdT = Convert.ToInt32(row["IDT"]);
                                posRequest.SetIdentifiers(Convert.ToString(row["IDENTIFIER"]));
                            }
                            else
                            {
                                posRequest.PosKeepingKeySpecified = true;
                                if (false == Convert.IsDBNull(row["IDI"]))
                                    idI = Convert.ToInt32(row["IDI"]);
                                if (false == Convert.IsDBNull(row["IDASSET"]))
                                    idAsset = Convert.ToInt32(row["IDASSET"]);
                                if (false == Convert.IsDBNull(row["IDA_DEALER"]))
                                    idA_Dealer = Convert.ToInt32(row["IDA_DEALER"]);
                                if (false == Convert.IsDBNull(row["IDB_DEALER"]))
                                    idB_Dealer = Convert.ToInt32(row["IDB_DEALER"]);
                                if (false == Convert.IsDBNull(row["IDA_CLEARER"]))
                                    idA_Clearer = Convert.ToInt32(row["IDA_CLEARER"]);
                                if (false == Convert.IsDBNull(row["IDB_CLEARER"]))
                                    idB_Clearer = Convert.ToInt32(row["IDB_CLEARER"]);


                                posRequest.SetPosKey(idI, _underlyingAsset, idAsset, idA_Dealer, idB_Dealer, idA_Clearer, idB_Clearer);
                            }
                            posRequest.QtySpecified = (false == Convert.IsDBNull(row["QTY"]));
                            if (posRequest.IdTSpecified && posRequest.QtySpecified)
                                posRequest.Qty = Convert.ToDecimal(row["QTY"]);

                            break;
                        case Cst.PosRequestTypeEnum.EOD_SafekeepingGroupLevel:
                            posRequest.IdTSpecified = (false == Convert.IsDBNull(row["IDT"]));
                            if (posRequest.IdTSpecified)
                            {
                                posRequest.IdT = Convert.ToInt32(row["IDT"]);
                                posRequest.SetIdentifiers(Convert.ToString(row["TRADE_IDENTIFIER"]));
                            }
                            break;
                        case Cst.PosRequestTypeEnum.UpdateEntry:
                        case Cst.PosRequestTypeEnum.ClearingEndOfDay:
                        case Cst.PosRequestTypeEnum.MaturityOffsettingFuture:
                        case Cst.PosRequestTypeEnum.Cascading:
                            posRequest.PosKeepingKeySpecified = true;
                            idI = Convert.ToInt32(row["IDI"]);
                            idAsset = Convert.ToInt32(row["IDASSET"]);
                            idA_Dealer = Convert.ToInt32(row["IDA_DEALER"]);
                            idB_Dealer = Convert.ToInt32(row["IDB_DEALER"]);
                            idA_Clearer = Convert.ToInt32(row["IDA_CLEARER"]);
                            idB_Clearer = Convert.ToInt32(row["IDB_CLEARER"]);
                            posRequest.SetPosKey(idI, _underlyingAsset, idAsset, idA_Dealer, idB_Dealer, idA_Clearer, idB_Clearer);
                            break;
                        default:
                            continue;
                    }
                    
                    //PosKeepingTools.InsertPosRequest(CS, null, newIdPR, posRequest, m_PKGenProcess.appInstance, LogHeader.IdProcess, posRequestGroupLevel.idPR);
                    PosKeepingTools.InsertPosRequest(CS, null, newIdPR, posRequest, m_PKGenProcess.Session, IdProcess, posRequestGroupLevel.IdPR);
                    AddSubPosRequest(m_LstSubPosRequest, (IPosRequest)posRequest.CloneMain());

                    newIdPR++;
                }
            }

                if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                    UpdateIRQPosRequestGroupLevel(posRequestGroupLevel);
                else
            UpdatePosRequestGroupLevel(posRequestGroupLevel, posRequestGroupLevel.Status);

            }
            return codeReturn;
        }
        #endregion ClosingDayControlDetail

        #region CommonEntryGen
        /// <summary>
        /// MISE A JOUR DES CLOTURES (STP ou EOD)
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>1. ► Chargement des trades candidats</para>
        ///<para>     ● Position détaillée VEILLE et trades JOUR</para>
        ///<para>     ● Clôtures JOUR (POSACTIONDET)</para>
        ///<para>2. ► Traitement des clôtures dans une table de travail de type POSACTIONDET</para>
        ///<para>3. ► Fusion des 2 tables POSACTIONDET</para>
        ///<para>4. ► Mise à jour dans les tables</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20171016 [23509] Upd Tri sur DTEXECUTION
        protected Cst.ErrLevel CommonEntryGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (PosKeepingData.IsIgnorePositionEffect_BookDealer && ExchangeTradedDerivativeTools.IsPositionEffect_Open(PosKeepingData.PositionEffect_BookDealer))
            {
                //Tous les trades sont à considérer comme "Open"
                codeReturn = Cst.ErrLevel.NOTHINGTODO;
            }
            else
            {
                //1. Chargement des trades candidats
                #region NB
                // On charge :
                // - par ordre d'entré en portefeuille (sort = DTEXECUTION ASC, IDT ASC) 
                // - tous les trades, du jour, destinés à avoir une incidence sur la Position (= CLOSE)
                #endregion
                DataSet ds = GetPositionDetailVeilAndTradeDay(DtBusiness);
                if (null != ds)
                {
                    m_DtPosition = ds.Tables[0];
                    m_DtPosActionDet = ds.Tables[1];
                    m_DtPosActionDet_Working = ds.Tables[1].Clone();
                    // Existe-t-il des clôtures JOUR ?
                    DateTime dtBusiness = DtBusiness;
                    if (dtBusiness > PosKeepingData.Market.DtMarket)
                    {
                        dtBusiness = PosKeepingData.Market.DtMarket;
                    }

                    string sort = "DTEXECUTION ASC, IDT ASC";
                    string filter = "DTBUSINESS='{0}'";
                    if (!PosKeepingData.IsIgnorePositionEffect_BookDealer)
                    {
                        //On limite aux seuls trades saisis en "Close"
                        filter += " and POSITIONEFFECT='{1}'";
                    }
                    DataRow[] rowTradeDayClosing = m_DtPosition.Select(
                        String.Format(filter, DtFunc.DateTimeToStringDateISO(dtBusiness), ExchangeTradedDerivativeTools.GetPositionEffect_Close()),
                        sort, DataViewRowState.CurrentRows);
                    if (ArrFunc.IsFilled(rowTradeDayClosing))
                    {
                        // 2. Traitement des clôtures dans table de travail de type POSACTIONDET
                        codeReturn = EntryCalculation(rowTradeDayClosing);

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            // 3. Fusion des 2 tables POSACTIONDET
                            codeReturn = MergePosActionDetBeforeUpdating();
                        }

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {

                            //FI 20130814 [18884] Mise en commentaire des tests sur DeadLock (code temporaire pour diag)
                            //try
                            //{
                            int nbTokenIDE = 6 * 2; // 6 evts potentiels pour le trade clôturé et le trade clôturant 
                            // 4. Mise à jour dans les tables
                            int idPA = 0;
                            codeReturn = PosKeep_Updating(nbTokenIDE, ref idPA);

                            //}
                            //catch (Exception ex)
                            //{
                            //    SQLErrorEnum err = DataHelper.AnalyseSQLException(CS, ex);
                            //    if (err == SQLErrorEnum.DeadLock)
                            //        throw new Exception("DeadLock on PosKeep_Updating", ex);
                            //    else
                            //        throw new Exception("Error on PosKeep_Updating", ex);
                            //}
                        }
                    }
                    else
                        codeReturn = Cst.ErrLevel.NOTHINGTODO;
                }
            }
            return codeReturn;
        }
        #endregion CommonEntryGen
        #region ControlAndGetEntityMarkets
        /// <summary>
        /// Chargement des données ENTITYMARKET et
        /// Contrôle cohérence DTMARKET avec la date DTBUSINESS de la demande m_MasterPosRequest
        /// </summary>
        /// <returns></returns>
        /// EG 20131004 [19027] New 
        /// EG 20140225 [19575][19666]
        /// EG 20150317 [POC] GetEntityMarkets (m_MasterPosRequest parameter)
        /// EG 20190114 Add detail to ProcessLog Refactoring
        protected DataRowCollection ControlAndGetEntityMarkets(ref Cst.ErrLevel pCodeReturn)
        {
            bool isErr = false;

            DataSet ds = PosKeepingTools.GetEntityMarkets(CS, m_MasterPosRequest);
            if (null == ds)
                throw new NullReferenceException("PosKeepingTools.GetEntityMarkets return null");

            DataRowCollection rowEntityMarkets = ds.Tables[0].Rows;
            foreach (DataRow rowEntitymarket in rowEntityMarkets)
            {
                /// PM 20150422 [20575] DTMARKET => DTENTITY
                //DateTime dtMarket = Convert.ToDateTime(rowEntitmarket["DTMARKET"]);
                DateTime dtEntity = Convert.ToDateTime(rowEntitymarket["DTENTITY"]);
                if (dtEntity != m_MasterPosRequest.DtBusiness)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5000), 0,
                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                        new LogParam(LogTools.IdentifierAndId(rowEntitymarket["SHORT_ACRONYM"].ToString(), rowEntitymarket["IDM"].ToString())),
                        new LogParam(DtFunc.DateTimeToStringDateISO(dtEntity)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));

                    isErr = true;
                    break;
                }
            }

            if (isErr)
            {
                pCodeReturn = Cst.ErrLevel.REQUEST_REJECTED;
            }

            return rowEntityMarkets;
        }
        #endregion ControlAndGetEntityMarkets

        #region DeletePosActionDetAndLinkedEventByRequest
        /// <summary>
        /// Suppression des Evénements liés à la ligne de clôture à supprimer via EVENTPOSACTIONDET
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdPR"></param>
        /// <returns></returns>
        // EG 20170223 [22717]
        protected Cst.ErrLevel DeleteAssetControl_EOD(IPosRequest pPosRequest)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(CS, "ENTITY", DbType.Int32), pPosRequest.IdA_Entity);
            dataParameters.Add(new DataParameter(CS, "CLEARINGHOUSE", DbType.Int32), pPosRequest.IdA_Css);
            dataParameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), pPosRequest.DtBusiness);
            string sqlDelete = @"delete 
            from dbo.ASSETCONTROL_EOD 
            where (IDA_ENTITY = @ENTITY) and (IDA_CSS = @CLEARINGHOUSE) and (DTBUSINESS = @DTBUSINESS)";

            if (pPosRequest.IdMSpecified)
            {
                dataParameters.Add(new DataParameter(CS, "IDM", DbType.Int32), pPosRequest.IdM);
                sqlDelete += " and (IDM = @IDM)";
            }
            QueryParameters queryParameters = new QueryParameters(CS, sqlDelete, dataParameters);
            DataHelper.ExecuteNonQuery(CS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        /// <summary>
        /// Truncate des tables "temporaire"
        /// pTableAssetControl (=>ASSETCTRL_{buildTableId}_W) et 
        /// pTableTradeAssetControl (=>TRADEASSETCTRL_{buildTableId}_W
        /// </summary>
        /// <returns></returns>
        // EG 20180803 PERF New
        protected Cst.ErrLevel TruncateAssetControl_EOD(string pTableAssetControl, string pTableTradeAssetControl)
        {
            string sqlDelete = @"truncate table dbo.ASSETCTRL_MODEL".Replace("ASSETCTRL_MODEL", pTableAssetControl);
            if (StrFunc.IsFilled(pTableTradeAssetControl))
            {
                sqlDelete += @";" + Cst.CrLf;
                sqlDelete += @"truncate table dbo.TRADEASSETCTRL_MODEL".Replace("TRADEASSETCTRL_MODEL", pTableTradeAssetControl);
            }
            DataHelper.ExecuteNonQuery(CS, CommandType.Text, sqlDelete, null);
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion DeleteAssetControl_EOD
        #region DeletePosActionDetAndLinkedEventByRequest
        /// <summary>
        /// Suppression des Evénements liés à la ligne de clôture à supprimer via EVENTPOSACTIONDET
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdPR"></param>
        /// <returns></returns>
        // EG 20160208 [POC-MUREX] Refactoring
        // EG 20180307 [23769] Gestion Asynchrone
        // EG 20180307 [23769] Gestion dbTransaction
        public Cst.ErrLevel DeleteAllPosActionAndLinkedEventByRequest(IDbTransaction pDbTransaction, int pIdPR)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDPR", DbType.Int32), pIdPR);

            // Delete EVENT via EVENTPOSACTIONDET link

            string sqlQuery = string.Empty;
            if (DataHelper.IsDbSqlServer(CS))
            {
                sqlQuery += @"delete ev
                from dbo.EVENT ev
                inner join dbo.EVENTPOSACTIONDET epad  on (epad.IDE = ev.IDE)
                inner join dbo.POSACTIONDET pad on (pad.IDPADET = epad.IDPADET)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) and (pa.IDPR = @IDPR)";
            }
            else if (DataHelper.IsDbOracle(CS))
            {
                sqlQuery += @"delete from dbo.EVENT
                where IDE in 
                (
                    select ev.IDE 
                    from dbo.EVENT ev
                    inner join dbo.EVENTPOSACTIONDET epad  on (epad.IDE = ev.IDE)
                    inner join dbo.POSACTIONDET pad on (pad.IDPADET = epad.IDPADET)
                    inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) and (pa.IDPR = @IDPR)
                )";
            }

            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());

            // Delete POSACTION/POSACTIONDET et EVENTPOSACTIONDET (cascade)
            sqlQuery = @"delete from dbo.POSACTION where (IDPR = @IDPR)";
            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion DeletePosActionDetAndLinkedEventByRequest
        #region DeserializeTrade
        protected void DeserializeTrade(int pIdT)
        {
            DeserializeTrade(null, pIdT);
        }
        #endregion DeserializeTrade

        #region EntryCalculation
        /// <summary>
        /// Calcul de la tenue de position: Traitement principal d'un message de type PosKeepingEntryGen
        /// </summary>
        /// <returns></returns>
        // EG 20131112 [19103]
        // EG 20171016 [23509] Upd dtExecution is used instead of Timestamp
        // EG 20171123 BUG UPDENTRY
        protected Cst.ErrLevel EntryCalculation(DataRow[] pRowTradeDayClosing)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            string positionEffect = ExchangeTradedDerivativeTools.GetPositionEffect_FIFO();
            if (this.PosKeepingData.TradeSpecified)
            {
                positionEffect = this.PosKeepingData.Trade.UltimatelyPositionEffect;
            }
            else if (this.PosKeepingData.IsIgnorePositionEffect_BookDealer)
            {
                if (!String.IsNullOrEmpty(this.PosKeepingData.PositionEffect_BookDealer))
                {
                    positionEffect = this.PosKeepingData.PositionEffect_BookDealer;
                }
            }

            if (ExchangeTradedDerivativeTools.IsPositionEffect_HILO(positionEffect))
            {
                #region HILO
                //Clôture HILO - Trade Jour en Close/HILO uniquement
                ret = ClearingBLKCalc_HILO(Convert.ToDateTime(pRowTradeDayClosing[0]["DTBUSINESS"]));
                #endregion
            }
            else
            {
                #region FIFO,LIFO
                // EG 120131112 [19103]
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                long idTClosing;
                decimal qtyClosing;
                string sideClosing, positionEffectClosing;
                DateTime dtTimestampClosing, dtBusinessClosing, dtExecutionClosing;
                DataRow[] rowsTradeClosable;

                // EG 20131112 [19103] Critères de sélection Trade cloturable (ajout filtre sur IDT en cas d'égalité DTEXECUTION)
                // EG 20171016 [23509] Upd
                string FILTERClosable = "QTY<>0 and SIDE<>'{0}' and ((DTEXECUTION < #{1}#) or (DTEXECUTION = #{1}# and IDT < {2}))";


                // Boucle sur négociations clôturantes
                foreach (DataRow rowTradeClosing in pRowTradeDayClosing)
                {
                    sideClosing = rowTradeClosing["SIDE"].ToString();
                    //positionEffectClosing = rowTradeClosing["POSITIONEFFECT"].ToString();
                    if (this.PosKeepingData.TradeSpecified)
                    {
                        positionEffectClosing = this.PosKeepingData.Trade.UltimatelyPositionEffect;
                    }
                    else
                    {
                        positionEffectClosing = positionEffect;
                    }
                    // EG 20131112 [19103]
                    idTClosing = Convert.ToInt32(rowTradeClosing["IDT"]);
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    qtyClosing = Convert.ToDecimal(rowTradeClosing["QTY"]);
                    dtBusinessClosing = Convert.ToDateTime(rowTradeClosing["DTBUSINESS"]);
                    // EG 20171016 [23509] Upd
                    dtTimestampClosing = Convert.ToDateTime(rowTradeClosing["DTTIMESTAMP"]);
                    dtExecutionClosing = Convert.ToDateTime(rowTradeClosing["DTEXECUTION"]);


                    // Recherche de la règle de tri applicable aux négociations clôturables, et le cas échéant d'un filtre additionnel (ex. FIFO-Intraday)
                    string rulesClosable = GetOffSettingRulesForClosable(positionEffectClosing, out string addFilterClosable);
                    // EG 20140410 Gestion TIMESTAMP : ajour paramètre IdTClosing au Filtre
                    // EG 20171016 [23509] Upd DTEXECUTION
                    // EG 20171123 BUG UPDENTRY
                    //string filterClosable = String.Format(FILTERClosable + addFilterClosable, sideClosing, DtFunc.DateTimeOffsetUTCToStringISO(dtExecutionClosing),
                    //    idTClosing, DtFunc.DateTimeToStringDateISO(dtBusinessClosing));
                    string filterClosable = String.Format(FILTERClosable + addFilterClosable, sideClosing, Tz.Tools.ToString(dtExecutionClosing),
                        idTClosing, DtFunc.DateTimeToStringDateISO(dtBusinessClosing));
                    rowsTradeClosable = m_DtPosition.Select(filterClosable, rulesClosable, DataViewRowState.CurrentRows);
                    if (ArrFunc.IsFilled(rowsTradeClosable))
                    {
                        // Boucle sur négociations clôturables
                        foreach (DataRow rowTradeClosable in rowsTradeClosable)
                        {
                            if (qtyClosing > 0)
                            {
                                qtyClosing -= AddNewPosActionDet(rowTradeClosable, rowTradeClosing, qtyClosing, positionEffectClosing);
                            }
                            else
                            {
                                // Quantité du trade totalement clôturée --> on sort de la boucle
                                break;
                            }
                        }
                    }
                }
                #endregion
            }

            return ret;
        }
        #endregion EntryCalculation

        #region FeesCalculationGen
        /// <summary>
        /// (RE)Calcul des frais manquants pour un trade donné candidat
        /// TRADE CANDIDAT = Il exist un book d'un de ses acteurs (TRADEACTOR) avec :
        /// 1) règle "Absence de frais" non autorisée  
        /// 2) sans frais sur ce book (pas de frais avec ce book payeur ou receveur)
        /// </summary>
        /// <param name="pTradeFeesCalculation">Caractéristiques du trade</param>
        /// <returns>Cst.ErrLevel</returns>
        /// <summary>
        /// EG 20120328 Ticket 17706 : Calcul des frais manquants 
        /// EG 20130701 Frais forcés préservés au recalcul (ITD / EOD)
        /// EG 20160208 (POC-MUREX] Refactoring 
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel FeesCalculationGen(TradeFeesCalculation pTradeFeesCalculation)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                int nbTotPayment = 0;
                ArrayList aAutoPayments = new ArrayList();
                ArrayList aManualPayments = new ArrayList();
                ArrayList aForcedPayments = new ArrayList();
                
                IPayment[] payments = null;
                User user = new User(m_PKGenProcess.Session.IdA, null, RoleActor.SYSADMIN);
                TradeInput tradeInput = new TradeInput();
                tradeInput.SearchAndDeserializeShortForm(CS, null, pTradeFeesCalculation.idT.ToString(), SQL_TableWithID.IDType.Id, user, m_PKGenProcess.Session.SessionId);
                pTradeFeesCalculation.Product = tradeInput.Product.Product;
                pTradeFeesCalculation.DataDocument = tradeInput.DataDocument;

                #region SAUVEGARDE DES FRAIS MANUELS / FORCES
                // Les frais manuels et forcés sont toujours préservés
                if (tradeInput.CurrentTrade.OtherPartyPaymentSpecified)
                {
                    // RD 20160705 [22320] Valorisation de la variable nbTotPayment
                    nbTotPayment = tradeInput.CurrentTrade.OtherPartyPayment.Length;
                    foreach (IPayment item in (IPayment[])tradeInput.CurrentTrade.OtherPartyPayment)
                    {
                        // Ligne MANUELLE = (Ligne sans paymentSource ou sans status)
                        // EG 20130701 Frais forcés préservés au recalcul (ITD / EOD)
                        bool isManualPayment = (false == item.PaymentSourceSpecified) || (item.PaymentSourceSpecified && (false == item.PaymentSource.StatusSpecified));
                        bool isForcedPayment = item.PaymentSourceSpecified && item.PaymentSource.StatusSpecified && (item.PaymentSource.Status == SpheresSourceStatusEnum.Forced);

                        if (isManualPayment) aManualPayments.Add(item);
                        if (isForcedPayment) aForcedPayments.Add(item);
                    }
                }
                #endregion SAUVEGARDE DES FRAIS MANUELS / FORCES

                #region RECALCUL DES FRAIS
                tradeInput.CurrentTrade.OtherPartyPayment = null;
                tradeInput.CurrentTrade.OtherPartyPaymentSpecified = false;

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7001), 0, new LogParam(LogTools.IdentifierAndId(pTradeFeesCalculation.identifier, pTradeFeesCalculation.idT))));


                tradeInput.RecalculFeeAndTax(CS, null);
                if (tradeInput.CurrentTrade.OtherPartyPaymentSpecified)
                    aAutoPayments.AddRange(tradeInput.CurrentTrade.OtherPartyPayment);
                #endregion RECALCUL DES FRAIS

                #region CONTROLE DES FRAIS FORCES
                // EG 20130701 Frais forcés préservés au recalcul (ITD / EOD)
                if (0 < aForcedPayments.Count)
                {
                    // On recherche si les frais forcés ont leurs correspondances dans les frais recalculés
                    // Si oui on conserve les frais Forcés d'origine
                    // Si non on garde les frais recalculés
                    foreach (IPayment _forcedPayment in aForcedPayments)
                    {
                        IPayment _autoPaymentFounded = _forcedPayment.DeleteMatchPayment(aAutoPayments);
                        if (null != _autoPaymentFounded)
                        {
                            aAutoPayments.Remove(_autoPaymentFounded);
                            aManualPayments.Add(_forcedPayment);
                        }
                    }
                }
                #endregion CONTROLE DES FRAIS FORCES

                #region CONTROLE EXISTENCE FRAIS SUR BOOKS (VRFEE = WARNING/ERROR)
                /// On contrôle l'existence d'au moins une ligne de frais (MANUELLE/CALCULEES) pour chaque book concerné (VRFEE = WARNING/ERROR)
                ArrayList aPayments = new ArrayList();
                if ((0 < aManualPayments.Count) || (0 < aAutoPayments.Count))
                {
                    if (0 < aManualPayments.Count) aPayments.AddRange(aManualPayments);
                    if (0 < aAutoPayments.Count) aPayments.AddRange(aAutoPayments);
                    payments = (IPayment[])aPayments.ToArray(typeof(IPayment));
                    aPayments.Clear();
                    IBookId bookPayer = null;
                    IBookId bookReceiver = null;
                    bool isFound;
                    // Contrôle pour tous les books
                    foreach (KeyValuePair<int, ActorFeesCalculation> actor in pTradeFeesCalculation.actorFee)
                    {
                        isFound = false;

                        foreach (IPayment payment in payments)
                        {
                            bookPayer = tradeInput.DataDocument.GetBookId(payment.PayerPartyReference.HRef);
                            bookReceiver = tradeInput.DataDocument.GetBookId(payment.ReceiverPartyReference.HRef);
                            if ((bookPayer != null && bookPayer.OTCmlId == actor.Key) || (bookReceiver != null && bookReceiver.OTCmlId == actor.Key))
                            {
                                // SUCCESS : Frais trouvé pour le book
                                isFound = true;
                                m_PKGenProcess.AddLogFeeInformation(tradeInput.DataDocument, payment);
                                break;
                            }
                        }
                        if (!isFound)
                        {
                            //EG 20130723
                            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusEnum.ERROR;
                            if (actor.Value.vrFee == Cst.CheckModeEnum.Warning)
                                status = ProcessStateTools.StatusEnum.WARNING;
                            
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(status);

                            // ERROR : Aucun frais trouvé pour le book
                            
                            int errNum = ((status == ProcessStateTools.StatusEnum.WARNING) ? 5174 : 05175);
                            Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(status), new SysMsgCode(SysCodeEnum.SYS, errNum), 0,
                                new LogParam(LogTools.IdentifierAndId(pTradeFeesCalculation.identifier, pTradeFeesCalculation.idT)),
                                new LogParam(LogTools.IdentifierAndId(actor.Value.actorIdentifier, actor.Value.idA)),
                                new LogParam(LogTools.IdentifierAndId(actor.Value.bookIdentifier, actor.Value.idB)),
                                new LogParam(actor.Value.idRoleActor),
                                new LogParam(actor.Value.vrFee)));

                            if (actor.Value.vrFee == Cst.CheckModeEnum.Error)//Si vrFee = Warning, on ne considèrera pas le traitement comme erroné.
                                pTradeFeesCalculation.IsNotExistFeesOnBookWithVRError = true;
                            else if (actor.Value.vrFee == Cst.CheckModeEnum.Warning)
                                pTradeFeesCalculation.IsNotExistFeesOnBookWithVRWarning = true;
                        }
                    }
                }
                else
                {
                    // Aucun frais sur le trade 
                    foreach (KeyValuePair<int, ActorFeesCalculation> actor in pTradeFeesCalculation.actorFee)
                    {
                        ProcessStateTools.StatusEnum status = ProcessStateTools.StatusEnum.ERROR;
                        if (actor.Value.vrFee == Cst.CheckModeEnum.Warning)
                            status = ProcessStateTools.StatusEnum.WARNING;
                        
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_PKGenProcess.ProcessState.SetErrorWarning(status);

                        
                        int errNum = ((status == ProcessStateTools.StatusEnum.WARNING) ? 5174 : 05175);
                        Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(status), new SysMsgCode(SysCodeEnum.SYS, errNum), 0,
                            new LogParam(LogTools.IdentifierAndId(pTradeFeesCalculation.identifier, pTradeFeesCalculation.idT)),
                            new LogParam(LogTools.IdentifierAndId(actor.Value.actorIdentifier, actor.Value.idA)),
                            new LogParam(LogTools.IdentifierAndId(actor.Value.bookIdentifier, actor.Value.idB)),
                            new LogParam(actor.Value.idRoleActor),
                            new LogParam(actor.Value.vrFee)));

                        //FL/PL 20120530
                        if (actor.Value.vrFee == Cst.CheckModeEnum.Error)//Si vrFee = Warning, on ne considèrera pas le traitement comme erroné.
                            pTradeFeesCalculation.IsNotExistFeesOnBookWithVRError = true;
                        else if (actor.Value.vrFee == Cst.CheckModeEnum.Warning)
                            pTradeFeesCalculation.IsNotExistFeesOnBookWithVRWarning = true;
                    }
                }
                #endregion CONTROLE EXISTENCE FRAIS SUR BOOKS (VRFEE = WARNING/ERROR)

                aPayments.Clear();
                pTradeFeesCalculation.IsDeleted = (aManualPayments.Count <= nbTotPayment) && (0 < nbTotPayment);
                if (0 < aManualPayments.Count + aAutoPayments.Count)
                {
                    aPayments.AddRange(aAutoPayments);
                    if (aManualPayments.Count <= nbTotPayment)
                        aPayments.AddRange(aManualPayments);

                    pTradeFeesCalculation.IsInserted = (0 < aPayments.Count);
                    if (pTradeFeesCalculation.IsInserted)
                    {
                        int nbEvents = 0;
                        pTradeFeesCalculation.Payments = EventQuery.PrepareFeeEvents(CS, pTradeFeesCalculation.Product, pTradeFeesCalculation.DataDocument, pTradeFeesCalculation.idT, aPayments, ref nbEvents);
                        pTradeFeesCalculation.NbEvents = nbEvents;
                        pTradeFeesCalculation.DataDocument.CurrentTrade.SetOtherPartyPayment(aPayments);
                        pTradeFeesCalculation.DataDocument.CurrentTrade.OtherPartyPaymentSpecified = pTradeFeesCalculation.IsInserted;
                    }
                }
                EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(pTradeFeesCalculation.DataDocument.DataDocument);
                pTradeFeesCalculation.TradeXML = CacheSerializer.Serialize(serializerInfo);
            }
            catch (Exception) {codeReturn = Cst.ErrLevel.FAILURE;}
            return codeReturn;
        }
        #endregion FeesCalculationGen
        #region FeesWritingGen
        /// <summary>
        /// Ecriture des frais calculés précédemment
        /// </summary>
        /// EG 20160208 [POC-MUREX] New
        /// FI 20160304 [XXXXX] Modify
        /// FI 20170306 [22225] Modify
        /// FI 20170323 [XXXXX] Modify
        // EG 20180502 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel FeesWritingGen(TradeFeesCalculation pTradeFeesCalculation, int pNewIdE)
        {
            IDbTransaction dbTransaction = null;
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            bool isException = false;
            string tradeLogIdentifier = LogTools.IdentifierAndId(pTradeFeesCalculation.identifier, pTradeFeesCalculation.idT);

            try
            {
                // EG 20160115 [POC-MUREX]
                if (pTradeFeesCalculation.IsDeleted || pTradeFeesCalculation.IsInserted)
                    dbTransaction = DataHelper.BeginTran(CS);

                #region SUPPRESSION DES EVENEMENTS DE FRAIS PRECEDENTS
                if (pTradeFeesCalculation.IsDeleted)
                {
                    try
                    {
                        // DELETE
                        EventQuery.DeleteFeeEvents(CS, dbTransaction, pTradeFeesCalculation.idT);
                    }
                    catch (DbException ex)
                    {
                        AppInstance.TraceManager.TraceInformation(this, string.Format("Error={0}", ex.Message));
                        if (ex.Message.Contains(Cst.OTCml_Constraint.FK_EARDAY_EVENTCLASS))
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5178), 0, new LogParam(LogTools.IdentifierAndId(pTradeFeesCalculation.identifier, pTradeFeesCalculation.idT))));
                        }
                        throw;
                    }
                }
                #endregion SUPPRESSION DES EVENEMENTS DE FRAIS PRECEDENTS

                #region INSERTION DES EVENEMENTS DES NOUVEAUX FRAIS
                if (pTradeFeesCalculation.IsInserted)
                {
                    // EG 20160115 [POC-MUREX]
                    m_EventQuery.InsertFeeEvents(CS, dbTransaction, pTradeFeesCalculation.DataDocument,
                        pTradeFeesCalculation.idT, DtBusiness, pTradeFeesCalculation.idE_Event, pTradeFeesCalculation.Payments, pNewIdE);
                }
                #endregion INSERTION DES EVENEMENTS DES NOUVEAUX FRAIS

                #region MISE A JOUR TRADEXML
                // ------------------------------------------------------------------ 
                // TRAITEMENT FINAL DES FRAIS
                // Mise à jour des caractéristiques des frais sur le trade XML
                // A FAIRE S'IL Y A EU SUPPRESSION et/ou INSERTION
                // ------------------------------------------------------------------ 
                if ((Cst.ErrLevel.SUCCESS == codeReturn) && (false == isException))
                {
                    if (pTradeFeesCalculation.IsDeleted || pTradeFeesCalculation.IsInserted)
                    {
                        // FI 20170323 [XXXXX] lecture Dtsys
                        DateTime dtSys = OTCmlHelper.GetDateSysUTC(CS);

                        // FI 20170306 [22225] call TradeRDBMSTools.UpdateTradeXML
                        //EventQuery.UpdateTradeXMLForFees(dbTransaction, pTradeFeesCalculation.idT, pTradeFeesCalculation.tradeXML);
                        // FI 20170323 [XXXXX] Mise en place des paramètres nécessaires à l'alimentation de TRADETRAIL

                        
                        //TradeRDBMSTools.UpdateTradeXML(dbTransaction, pTradeFeesCalculation.idT, pTradeFeesCalculation.tradeXML, dtSys,
                        //m_PKGenProcess.UserId, m_PKGenProcess.appInstance, m_PKGenProcess.processLog.header.idTRK_L, m_PKGenProcess.processLog.header.IdProcess);
                        TradeRDBMSTools.UpdateTradeXML(dbTransaction, pTradeFeesCalculation.idT, pTradeFeesCalculation.TradeXML, dtSys,
                        m_PKGenProcess.UserId, m_PKGenProcess.Session, m_PKGenProcess.Tracker.IdTRK_L, m_PKGenProcess.IdProcess);
                    }
                }
                if ((null != dbTransaction) && (false == isException))
                {
                    DataHelper.CommitTran(dbTransaction);
                }
                #endregion MISE A JOUR TRADEXML
            }
            catch (Exception ex)
            {
                // FI 20160304 [XXXXX] add tradeLogIdentifier
                AppInstance.TraceManager.TraceInformation(this, string.Format("FeesWritingGen={0};Error={1};Trade={2}", "Catch", ex.Message, tradeLogIdentifier));
                codeReturn = Cst.ErrLevel.FAILURE;
                isException = true;
                bool blockException = (ex is DbException) && ex.Message.Contains(Cst.OTCml_Constraint.FK_EARDAY_EVENTCLASS);
                if (!blockException) { throw; }
            }
            finally
            {
                if (pTradeFeesCalculation.IsNotExistFeesOnBookWithVRError)
                {
                    //Il existe au moisn un book sans frais, disposant d'une vrFee = Error --> on considèrera le traitement comme erroné.
                    codeReturn = Cst.ErrLevel.FAILURE;
                }
                else if (pTradeFeesCalculation.IsNotExistFeesOnBookWithVRWarning)
                    codeReturn = Cst.ErrLevel.DATANOTFOUND;

                if (null != dbTransaction)
                {
                    if (isException)
                    {
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    dbTransaction.Dispose();
                }
            }
            return codeReturn;
        }
        #endregion FeesWritingGen

        #region GetAsset
        /// <summary>
        /// Retourne les caractéristiques (IPosKeepingAsset) de l'asset {pIdAsset} 
        /// </summary>
        /// <param name="pIdAssetETD"></param>
        /// <param name="pPosRequestAssetQuote"></param>
        /// <returns></returns>
        /// FI 20130404 [18467] Modification de la clef d'accès au cache (utilisation des méthodes CacheAssetFind, CacheAssetAdd)
        // EG 20180307 [23769] Gestion Asynchrone
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20181203 [24360] Add parameter pIsCA (= true pour le traitement des CA (pas de dbTransaction sur Sql_Table - un dataReader est déjà ouvert sur la transaction propre à la CA)
        // EG 20181205 [24360] Refactoring de la correction
        // EG 20190613 [24683] Upd (Lock)
        public PosKeepingAsset GetAsset(IDbTransaction pDbTransaction, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset,
            int pIdAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            PosKeepingAsset asset = CacheAssetFindLock(pUnderlyingAsset, pIdAsset, pPosRequestAssetQuote);
            if (asset == null)
            {
                MapDataReaderRow mapDr = null;
                using (IDataReader dr = SearchPosKeepingDataByAsset(pDbTransaction, pUnderlyingAsset, pIdAsset, pPosRequestAssetQuote))
                {
                    mapDr = DataReaderExtension.DataReaderMapToSingle(dr);
                }
                if (null != mapDr)
                {
                    asset = SetPosKeepingAsset(pDbTransaction, pUnderlyingAsset, mapDr);
                    CacheAssetAdd(pUnderlyingAsset, pIdAsset, pPosRequestAssetQuote, asset);
                }
            }
            return asset;
        }
        #endregion GetAsset
        #region GetDataRequest
        /// <summary>
        /// Retourne les enregistrement POSREQUEST à traiter
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pRequestType"></param>
        /// <param name="pDate"></param>
        /// <param name="pIdEM"></param>
        /// <returns></returns>
        protected DataSet GetDataRequest(string pCS, Cst.PosRequestTypeEnum pRequestType, DateTime pDate, int pIdEM)
        {
            return GetDataRequest(pCS, null, pRequestType, pDate, pIdEM, null, null);
        }

        /// <summary>
        /// Retourne les enregistrement POSREQUEST à traiter
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pRequestType"></param>
        /// <param name="pDate"></param>
        /// <param name="pIdEM"></param>
        /// <param name="pPosActionType">Doit être renseigné uniquement sur les denouements d'option</param>
        /// <param name="pSettlSessIDEnum">Doit être renseigné uniquement sur les denouements d'option</param>
        /// <returns></returns>
        /// EG 20120328 Add EOD_FeesGroupLevel Ticket 17706 Recalcul des frais 
        /// PM 20130304 [18434] Add Shifting
        /// PM 20130212 [18414] Add Cascading
        /// PM 20130212 [18414] Add PositionCascading
        /// FI 20130312 [18467] Add pDbTransaction parameter (parce Spheres® insère désormais dans POSREQUEST dans le cadre d'une transaction SQL)
        /// EG 20130701 New AllocIsMissing
        /// FI 20130917 [18953] Add parameter pSettlSessIDEnum
        /// FI 20130917 [18953] Rename le la méthode en GetDataToRequest
        // EG 20141224 [20566] @DTPOS replace by @DTBUSINESS
        // EG 20141224 [20566] GetQueryPositionEntityMarketOption replace by GetQueryPositionOption
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataSet
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        protected DataSet GetDataRequest(string pCS, IDbTransaction pDbTransaction, Cst.PosRequestTypeEnum pRequestType, DateTime pDate, int pIdEM,
            Nullable<PosKeepingTools.PosActionType> pPosActionType, Nullable<SettlSessIDEnum> pSettlSessIDEnum)
        {
            /// FI 20130917 pSettlSessIDEnum est géré uniquement dans le cas des denouement d'option manuel 
            /// Ces derniers peuvent être effectué en Mode ITD
            /// La méthode ne gère pas le mode ITD pour les autres {pRequestType}
            if (pSettlSessIDEnum.HasValue && pSettlSessIDEnum.Value == SettlSessIDEnum.Intraday)
            {
                // FI 20130917 [18953]    
                switch (pRequestType)
                {
                    case Cst.PosRequestTypeEnum.OptionAbandon:
                    case Cst.PosRequestTypeEnum.OptionNotExercised:
                    case Cst.PosRequestTypeEnum.OptionNotAssigned:
                    case Cst.PosRequestTypeEnum.OptionAssignment:
                    case Cst.PosRequestTypeEnum.OptionExercise:
                        //NOTHING ce cas est correctement géré
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented for Request Type {1}", pSettlSessIDEnum.Value, pRequestType));
                }
            }

            DataSet ds = null;
            Pair<string, DataParameters> _query = GetQueryDataRequest(pCS, pDbTransaction, pRequestType, pDate, pIdEM, pPosActionType, pSettlSessIDEnum);
            if ((null != _query) && StrFunc.IsFilled(_query.First))
            {
                QueryParameters qryParameters = new QueryParameters(pCS, _query.First, _query.Second);
                ds = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, qryParameters.QueryHint, qryParameters.GetArrayDbParameterHint());
            }

            return ds;
        }
        #endregion GetDataRequest
        #region GetDataRequestWithIsolationLevel
        /// <summary>
        /// Exécute une requete avec un niveau d'isolation
        /// Objectif : éviter les inter-blocages entre Select et Mise à jour sur traitement EOD multi-instance
        /// </summary>
        /// <param name="pCS">ConnexionString</param>
        /// <param name="pIsolationLevel">Niveau d'isolation</param>
        /// <param name="pPosRequestType">Type de PosRequest demandé</param>
        /// <param name="pDtBusiness">Date de traitement</param>
        /// <param name="pIdEM">Id ENTITYMARKET</param>
        /// <returns></returns>
        /// EG 20140204 [19586] Appel aux requêtes de sélection des étapes du traitement EOD avec un niveau d'isolation (ReadUncommitted)
        protected DataSet GetDataRequestWithIsolationLevel(string pCS, IsolationLevel pIsolationLevel, Cst.PosRequestTypeEnum pPosRequestType, DateTime pDtBusiness, int pIdEM)
        {
            return GetDataRequestWithIsolationLevel(pCS, pIsolationLevel, pPosRequestType, pDtBusiness, pIdEM, null, null, null);
        }
        /// <summary>
        /// Exécute une requete avec un niveau d'isolation
        /// Objectif : éviter les inter-blocages entre Select et Mise à jour sur traitement EOD multi-instance
        /// </summary>
        /// <param name="pCS">ConnexionString</param>
        /// <param name="pIsolationLevel">Niveau d'isolation</param>
        /// <param name="pPosRequestType">Type de PosRequest demandé</param>
        /// <param name="pDtBusiness">Date de traitement</param>
        /// <param name="pIdEM">Id ENTITYMARKET</param>
        /// <param name="pMaxTimeout">Timeout par defaut (en sec)</param>
        /// <param name="pPosActionType">Action qui impacte sur la quantité attribué à un trade ou une position (voir intégration des demandes de dénouemebnt automatique)</param>
        /// <param name="pSettlSessIDEnum">Mode : IntradDay , EndofDay ...</param>
        /// <returns></returns>
        /// PL 20180202 Read Uncommited/Read Commited
        protected DataSet GetDataRequestWithIsolationLevel(string pCS, IsolationLevel pIsolationLevel, Cst.PosRequestTypeEnum pPosRequestType, DateTime pDtBusiness, int pIdEM,
            Nullable<int> pMaxTimeout, Nullable<PosKeepingTools.PosActionType> pPosActionType, Nullable<SettlSessIDEnum> pSettlSessIDEnum)
        {
            IDbTransaction dbTransaction = null;
            DataSet ds;
            bool isException = false;
            string cs = pCS;
            try
            {
                if (pMaxTimeout.HasValue)
                    cs = CSTools.SetMaxTimeOut(pCS, pMaxTimeout.Value);

                dbTransaction = DataHelper.BeginTran(cs, pIsolationLevel);
                ds = GetDataRequest(cs, dbTransaction, pPosRequestType, pDtBusiness, pIdEM, pPosActionType, pSettlSessIDEnum);
                DataHelper.CommitTran(dbTransaction);

                return ds;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                {
                    if (isException) { DataHelper.RollbackTran(dbTransaction); }

                    //PL 20180202 WARNING +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    if (DataHelper.IsDbSqlServer(cs) && (pIsolationLevel != IsolationLevel.ReadCommitted))
                    {
                        //Restauration du niveau d'isolation à Read Commited, car maintenu sur la Connexion !
                        dbTransaction = DataHelper.BeginTran(cs, IsolationLevel.ReadCommitted);
                        DataHelper.CommitTran(dbTransaction);
                    }
                    //PL 20180202 WARNING +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

                    dbTransaction.Dispose();
                }
            }
        }
        #endregion GetDataRequestWithIsolationLevel
        #region GetIdPR
        // EG 20151125 [20979] Refactoring : returnValue is Nullable<int> instead of int 
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        protected Nullable<int> GetIdPR()
        {
            Nullable<int> idPR = null;
            if (null != m_PosRequest)
            {
                switch (m_PosRequest.RequestType)
                {
                    case Cst.PosRequestTypeEnum.OptionAbandon:
                    case Cst.PosRequestTypeEnum.OptionNotExercised:
                    case Cst.PosRequestTypeEnum.OptionNotAssigned:
                    case Cst.PosRequestTypeEnum.OptionAssignment:
                    case Cst.PosRequestTypeEnum.OptionExercise:
                    case Cst.PosRequestTypeEnum.PositionCancelation:
                        idPR = m_PosRequest.IdPR;
                        break;
                }
            }
            return idPR;
        }
        #endregion GetIdPR
        #region GetOffSettingRulesForClosable
        // EG 20131112 [19103]
        // EG 20171016 [23509] Upd DTEXECUTION replace DTTIMESTAMP
        protected string GetOffSettingRulesForClosable(string pPositionEffect, out string opFilterClosableAdd)
        {
            opFilterClosableAdd = string.Empty;

            string ret;
            if (ExchangeTradedDerivativeTools.IsPositionEffect_Close(pPositionEffect)
                || ExchangeTradedDerivativeTools.IsPositionEffect_FIFO(pPositionEffect))
            {
                ret = "DTEXECUTION ASC, IDT ASC";
            }
            else if (ExchangeTradedDerivativeTools.IsPositionEffect_FIFO_ITD(pPositionEffect))
            {
                // EG 20131112 [19103] #{2}# -> #{3}#
                opFilterClosableAdd = " and DTBUSINESS=#{3}#";
                ret = "DTEXECUTION ASC, IDT ASC";
            }
            else if (ExchangeTradedDerivativeTools.IsPositionEffect_LIFO(pPositionEffect))
            {
                ret = "DTEXECUTION DESC, IDT DESC";
            }
            else if (ExchangeTradedDerivativeTools.IsPositionEffect_HILO(pPositionEffect))
            {
                //Tip: En HILO, on suffixe ici le paramètre "pPositionEffect" par le sens du trade CLOSE 
                string side = (String.IsNullOrEmpty(pPositionEffect) ? "0" : pPositionEffect.Substring(pPositionEffect.Length - 1));
                if (SideTools.IsFIXmlBuyer(side))
                {
                    //Le trade CLOSE est un ACHAT, on recherche donc les VENTES de prix Maxi à clôturer
                    ret = "PRICE DESC, IDT ASC";
                }
                else if (SideTools.IsFIXmlSeller(side))
                {
                    //Le trade CLOSE est un VENTE, on recherche docn les ACHATS de prix Mini à clôturer
                    ret = "PRICE ASC, IDT ASC";
                }
                else
                {
                    throw new NotImplementedException(StrFunc.AppendFormat("Invalid Side {0} on Position Effect {1}, please contact EFS", side, pPositionEffect));
                }
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("Position Effect {0} is not managed, please contact EFS", pPositionEffect));
            }

            return ret;
        }
        #endregion
        #region GetPositionDetailAndPositionAction
        // EG 20151125 [20979] Refactoring
        protected DataSet GetPositionDetailAndPositionAction(DateTime pDate)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pDate); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "IDI", DbType.Int32), PosKeepingData.IdI);
            parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), PosKeepingData.IdAsset);
            parameters.Add(new DataParameter(CS, "IDA_DEALER", DbType.Int32), PosKeepingData.IdA_Dealer);
            parameters.Add(new DataParameter(CS, "IDB_DEALER", DbType.Int32), PosKeepingData.IdB_Dealer);
            parameters.Add(new DataParameter(CS, "IDA_CLEARER", DbType.Int32), PosKeepingData.IdA_Clearer);
            parameters.Add(new DataParameter(CS, "IDB_CLEARER", DbType.Int32), PosKeepingData.IdB_Clearer);

            // EG 20151125 [20979] Refactoring
            string sqlSelect = GetQueryPositionAndActionOnDtBusiness();
            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, 480);
            DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, 480);
            // EG 20150920 [21314] ORIGINALQTY_CLOSED|ORIGINALQTY_CLOSING added after Dataset construction
            // EG 20170127 Qty Long To Decimal
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSED", typeof(decimal));
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSING", typeof(decimal));
            return ds;
        }
        #endregion GetPositionDetailAndPositionAction
        #region GetPositionTransferOrCancellation
        /// <summary>
        /// 1. ds.Table[0] : Retourne la clé de position du TRADE en cours de traitement (POC|POT) 
        ///                  et la quantité en position (à l'exception de celle concernant le POSREQUEST en cours)
        /// 2. ds.Table[1] : Retourne les actions sur position operées sur ce trade (à l'exception de celle concernant le POSREQUEST en cours)
        /// </summary>
        /// <param name="pDbTransaction">Transaction en cours (POT)</param>
        /// <param name="pDate">DtBusiness</param>
        /// <param name="pIdT">Id du Trade</param>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring : New 
        private DataSet GetPositionTransferOrCancellation(IDbTransaction pDbTransaction, DateTime pDate, int pIdT)
        {
            Nullable<int> idPR = GetIdPR();
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pDate); // FI 20201006 [XXXXX] DbType.Date
            if (idPR.HasValue)
                parameters.Add(new DataParameter(CS, "IDPR", DbType.Int32), idPR.Value);

            string sqlSelect = GetQueryPositionTradeAndAction(idPR.HasValue);
            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);

            DataSet ds;
            if (null != pDbTransaction)
            {
                ds = DataHelper.ExecuteDataset(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }
            else
            {
                // PL 20180312 WARNING: Use Read Commited !
                //ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, 480);
                ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, 480);
            }

            // EG 20170127 Qty Long To Decimal
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSED", typeof(decimal));
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSING", typeof(decimal));
            return ds;
        }
        #endregion GetPositionTransferOrCancellation
        #region GetQueryDataRequest
        /// EG 20160106 Global Refactoring
        // EG 20190308 Add ClosingReopeningPosition
        // EG 20190613 [24683] Update
        protected virtual Pair<string, DataParameters> GetQueryDataRequest(string pCS, IDbTransaction pDbTransaction, Cst.PosRequestTypeEnum pRequestType, DateTime pDate, int pIdEM,
            Nullable<PosKeepingTools.PosActionType> pPosActionType, Nullable<SettlSessIDEnum> pSettlSessIDEnum)
        {

            Pair<string, DataParameters> _query = null;

            string sqlSelect = string.Empty;
            
            DataParameters parameters = new DataParameters();
             parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDate);
            
            parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pIdEM);
            switch (pRequestType)
            {
                case Cst.PosRequestTypeEnum.ClosingDay:
                case Cst.PosRequestTypeEnum.EndOfDay:
                    parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),
                        ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(Cst.PosRequestTypeEnum.EOD_MarketGroupLevel));
                    sqlSelect = PosKeepingTools.GetQueryProcesEndOfDayInSucces();
                    break;
                case Cst.PosRequestTypeEnum.Entry:
                    parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),
                        ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(Cst.PosRequestTypeEnum.EOD_MarketGroupLevel));
                    // RD 20180615 [24027] Modify
                    //parameters.Add(new DataParameter(pCS, "STATUS", DbType.String, SQLCst.UT_STATUS_LEN), ProcessStateTools.StatusSuccess);
                    sqlSelect = GetQueryNewTradeAfterProcessEndOfDayInSuccess(pCS);
                    break;
                case Cst.PosRequestTypeEnum.RemoveAllocation:// FI 20160819 [22364] Modify
                    parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),
                        ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(Cst.PosRequestTypeEnum.EOD_MarketGroupLevel));
                    // RD 20180615 [24027] Modify
                    //parameters.Add(new DataParameter(pCS, "STATUS", DbType.String, SQLCst.UT_STATUS_LEN), ProcessStateTools.StatusSuccess);
                    sqlSelect = GetQueryNewRMVAllocAfterProcessEndOfDayInSuccess();
                    break;
                case Cst.PosRequestTypeEnum.UpdateEntry:
                    sqlSelect = GetQueryCandidatesToUpdateEntry(pDbTransaction, pDate, pIdEM);
                    break;
                case Cst.PosRequestTypeEnum.ClearingEndOfDay:
                    sqlSelect = GetQueryCandidatesToClearing(pCS);
                    break;
                case Cst.PosRequestTypeEnum.ClearingBulk:
                    sqlSelect = GetQueryCandidatesToClearing(pCS);
                    break;
                case Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel:
                    sqlSelect = GetQueryCandidatesToCashFlows(pCS);
                    break;
                case Cst.PosRequestTypeEnum.EOD_SafekeepingGroupLevel:
                    sqlSelect = GetQueryCandidatesToSafekeeping(pCS);
                    break;
                case Cst.PosRequestTypeEnum.EOD_FeesGroupLevel:
                    sqlSelect = GetQueryCandidatesToFeesCalculation();
                    break;
                case Cst.PosRequestTypeEnum.TradeMerging:
                    parameters.Clear();
                    parameters.Add(new DataParameter(pCS, "ROLE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), RoleGActor.TRADEMERGING.ToString());
                    parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pIdEM);
                    parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), pDate);
                    parameters.Add(new DataParameter(pCS, "IDSTENVIRONMENT", DbType.AnsiString, SQLCst.UT_STATUS_LEN), Cst.StatusEnvironment.REGULAR.ToString());
                    sqlSelect = GetQueryCandidatesToMerging();
                    break;
                case Cst.PosRequestTypeEnum.AllocMissing:
                    SQL_EntityMarket entityMarket = new SQL_EntityMarket(pCS, pIdEM);
                    parameters.Clear();
                    parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), pDate);
                    parameters.Add(new DataParameter(pCS, "IDM", DbType.Int32), entityMarket.IdM);
                    parameters.Add(new DataParameter(pCS, "IDA", DbType.Int32), entityMarket.IdA);
                    parameters.Add(new DataParameter(pCS, "STATUSMISSING", DbType.AnsiString, SQLCst.UT_STATUS_LEN), Cst.StatusActivation.MISSING);
                    parameters.Add(new DataParameter(pCS, "STATUSALLOC", DbType.AnsiString, SQLCst.UT_STATUS_LEN), Cst.StatusBusiness.ALLOC);
                    // EG 20170309 22884 GPRODUCT_VALUE instead of RESTRICT_GPRODUCT_POS
                    parameters.Add(new DataParameter(pCS, "GPRODUCT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), GPRODUCT_VALUE);
                    sqlSelect = GetQueryAllocMissing();
                    break;

                case Cst.PosRequestTypeEnum.EventMissing:
                    parameters.Add(new DataParameter(pCS, "STATUSREGULAR", DbType.AnsiString, SQLCst.UT_STATUS_LEN), Cst.StatusActivation.REGULAR);
                    parameters.Add(new DataParameter(pCS, "STATUSALLOC", DbType.AnsiString, SQLCst.UT_STATUS_LEN), Cst.StatusBusiness.ALLOC);
                    sqlSelect = GetQueryEventMissing();
                    break;
                case Cst.PosRequestTypeEnum.EOD_UTICalculationGroupLevel:
                    sqlSelect = GetQueryCandidatesToUTICalculation(pCS);
                    break;

                case Cst.PosRequestTypeEnum.ClosingPosition:
                case Cst.PosRequestTypeEnum.ClosingReopeningPosition:
                    parameters.Add(new DataParameter(CS, "ROLE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), RoleGActor.CLOSINGREOPENING.ToString());
                    if (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay)
                        sqlSelect = GetQueryCandidatesToClosingReopeningPosition(pCS);
                    else if (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.ClosingDay)
                        sqlSelect = GetQueryCandidatesToClosingReopeningPosition2(pCS);
                    break;
            }
            if (StrFunc.IsFilled(sqlSelect))
                _query = new Pair<string, DataParameters>(sqlSelect, parameters);

            return _query;
        }
        #endregion GetQueryDataRequest
        #region GetStatusGroupLevel
        /// <summary>
        /// Détermine le statut le plus fort sur la base des POSREQUEST de même PARENT (IDPR_POSREQUEST)
        /// </summary>
        /// <param name="pIdPR"></param>
        /// <param name="pDefaultStatus">statut par défaut</param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion Asynchrone
        public ProcessStateTools.StatusEnum GetStatusGroupLevel(int pIdPR, ProcessStateTools.StatusEnum pDefaultStatus)
        {
            return PosKeepingTools.GetStatusGroupLevel(m_LstSubPosRequest, pIdPR, pDefaultStatus);
        }
        #endregion GetStatusGroupLevel
        #region GetSubRequest
        /// EG 20180221 [23769] Used collection List<IposRequest> m_LstSubPosRequest
        protected IPosRequest GetSubRequest(Cst.PosRequestTypeEnum pRequestType, int pIdEM, IPosKeepingKey pPosKeepingKey)
        {
            IPosRequest ret = null;
            // FI 20181219 [24399] Add test valeur null avant de faire le lock
            if (null != m_LstSubPosRequest)
            {
                object spinLock = ((ICollection)m_LstSubPosRequest).SyncRoot;
                lock (spinLock)
                {
                    ret = (from item in m_LstSubPosRequest
                           where (item.RequestType == pRequestType) && (item.IdEM == pIdEM) &&
                                   (item.PosKeepingKeySpecified && (item.PosKeepingKey.LockObjectId == pPosKeepingKey.LockObjectId))
                           select item).ToList().FirstOrDefault();
                }
            }
            return ret;
        }
        #endregion GetSubRequest
        #region GetTemplate
        protected static IDataReader GetTemplate(string pCS, int pIdI)
        {
            return GetTemplate(pCS, null, pIdI);
        }
        // EG 20180205 [23769] Upd DataHelper.ExecuteReader
        protected static IDataReader GetTemplate(string pCS, IDbTransaction pDbTransaction, int pIdI)
        {
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "tr.IDENTIFIER as TEMPLATENAME,ig.SCREENNAME" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.INSTRUMENTGUI.ToString() + " ig ";
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE.ToString() + " tr ";
            sqlSelect += SQLCst.ON + "(tr.IDT = ig.IDT_TEMPLATE)" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(ig.IDI = " + pIdI.ToString() + ")" + SQLCst.AND + "(ig.ISDEFAULT = 1)" + Cst.CrLf;
            return DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text, sqlSelect.ToString());
        }
        #endregion GetTemplate

       

        #region InitPosKeepingData
        /// <summary>
        /// Initialise les données essentielles au traitement (la clé, l'asset avec sa cotation, le marché etc..)
        /// </summary>
        /// <returns>DATANOTFOUND lorsque l'asset ETD est inexistant</returns>
        /// FI 20130313 [18467] suppression du message SYS-05103, existe déjà ds la surcharge sans pIdEM
        // EG 20181203 [24360] Add parameter pIsCA (= true pour le traitement des CA (pas de dbTransaction sur Sql_Table - un dataReader est déjà ouvert sur la transaction propre à la CA)
        // EG 20181205 [24360] Refactoring correction
        protected Cst.ErrLevel InitPosKeepingData(IDbTransaction pDbTransaction, IPosKeepingKey pPosKeepingKey, int pIdEM,
            Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote, bool isQuoteCanBeReaded)
        {
            Cst.ErrLevel codeReturn = InitPosKeepingData(pDbTransaction, pPosKeepingKey, pPosRequestAssetQuote, isQuoteCanBeReaded);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                SetPosKeepingMarket(pIdEM);

            return codeReturn;
        }
        /// <summary>
        /// Initialise les données essentielles au traitement (la clé, l'asset avec sa cotation,etc..)
        /// </summary>
        /// <returns></returns>
        /// <param name="pPosKeepingKey"></param>
        /// <param name="pPosRequestAssetQuote"></param>
        /// <param name="isQuoteCanBeReaded"></param>
        /// <returns>DATANOTFOUND lorsque l'asset ETD est inexistant</returns>
        /// EG 20160302 (21969] 
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20181203 [24360] Add parameter pIsCA (= true pour le traitement des CA (pas de dbTransaction sur Sql_Table - un dataReader est déjà ouvert sur la transaction propre à la CA)
        // EG 20181205 [24360] Refactoring correction
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190918 Refactoring PosRequestTypeEnum (Maturity Redemption DEBTSECURITY)
        protected Cst.ErrLevel InitPosKeepingData(IDbTransaction pDbTransaction, IPosKeepingKey pPosKeepingKey, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote, bool isQuoteCanBeReaded)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            //
            PosKeepingData = m_Product.CreatePosKeepingData();
            PosKeepingData.SetPosKey(CS, pDbTransaction, pPosKeepingKey.IdI, pPosKeepingKey.UnderlyingAsset, pPosKeepingKey.IdAsset,
                                       pPosKeepingKey.IdA_Dealer, pPosKeepingKey.IdB_Dealer,
                                       pPosKeepingKey.IdA_Clearer, pPosKeepingKey.IdB_Clearer);
            PosKeepingData.SetAdditionalInfo(pPosKeepingKey.IdA_EntityDealer, pPosKeepingKey.IdA_EntityClearer);
            // Initilisation des informations de Tenue de position du Book Dealer si requestType de m_PosRequest  est dans ClearingBulk | ClearingEndOfDay | ClearingSpecific | Entry | UpdateEntry.
            if ((null != m_PosRequest) && ((Cst.PosRequestTypeEnum.RequestTypePosEffectOnly & m_PosRequest.RequestType) == m_PosRequest.RequestType))
                PosKeepingData.SetBookDealerInfo(CS, pDbTransaction, pPosKeepingKey.UnderlyingAsset, DtBusiness); //PL 20130326 POSGLOP DtBusiness
            PosKeepingData.TradeSpecified = false;
            PosKeepingData.MarketSpecified = false;
            PosKeepingData.Asset = GetAsset(pDbTransaction, pPosKeepingKey.UnderlyingAsset, pPosKeepingKey.IdAsset, pPosRequestAssetQuote);
            if (null != PosKeepingData.Asset)
            {
                if (isQuoteCanBeReaded)
                {
                    // EG 20160302 (21969] 
                    codeReturn = SetPosKeepingQuote();
                }
            }
            else
            {
                codeReturn = Cst.ErrLevel.DATANOTFOUND;

                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5103), 0, new LogParam(pPosKeepingKey.IdAsset)));
            }
            return codeReturn;
        }
        /// <summary>
        /// Initialise les données essentielles au traitement (la clé, l'asset avec sa cotation...)
        /// </summary>
		// RD 20161212 [22660] Modify
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20181205 [24360] Refactoring 
        protected Cst.ErrLevel InitPosKeepingData(int pIdT, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote, bool isQuoteCanBeReaded)
        {
            MapDataReaderRow mapDr = null;
            using (IDataReader dr = SearchPosKeepingData(null, pIdT, pPosRequestAssetQuote))
            {
                mapDr = DataReaderExtension.DataReaderMapToSingle(dr);
            }

            Cst.ErrLevel codeReturn;
            if (null != mapDr)
            {
                Cst.StatusBusiness statusBusiness = (Cst.StatusBusiness)StringToEnum.Parse(mapDr["IDSTBUSINESS"].Value.ToString(), Cst.StatusBusiness.UNDEFINED);
                Cst.StatusActivation statusActivation = (Cst.StatusActivation)StringToEnum.Parse(mapDr["IDSTACTIVATION"].Value.ToString(), Cst.StatusActivation.REGULAR);

                bool isStatusOk = (Cst.StatusBusiness.ALLOC == statusBusiness);
                // RD 20161212 [22660] Add condition m_PKGenProcess.IsEntry
                if (m_PKGenProcess.IsEntry)
                {
                    isStatusOk &= (Cst.StatusActivation.REGULAR == statusActivation);
                }
                else
                {
                    switch (m_MasterPosRequest.RequestType)
                    {
                        case Cst.PosRequestTypeEnum.RemoveAllocation:
                        case Cst.PosRequestTypeEnum.TradeSplitting:
                            //Autorisation de traitement à partir d'un trade Verrouillé
                            isStatusOk &= ((Cst.StatusActivation.REGULAR == statusActivation)
                                            || (Cst.StatusActivation.LOCKED == statusActivation));
                            break;
                        default:
                            isStatusOk &= (Cst.StatusActivation.REGULAR == statusActivation);
                            break;
                    }
                }

                if (isStatusOk)
                {
                    bool isPosKeepingBook = true;
                    if (m_PKGenProcess.IsEntry)
                    {
                        isPosKeepingBook = Convert.ToBoolean(mapDr["POSKEEPBOOK_DEALER"].Value);
                    }
                    if (isPosKeepingBook)
                    {
                        // EG 20150624 [21151] Add pPosRequestAssetQuote parameter
                        codeReturn = SetPosKeepingData(mapDr, pPosRequestAssetQuote, isQuoteCanBeReaded);
                    }
                    else
                    {
                        codeReturn = Cst.ErrLevel.DATAIGNORE;
                    }
                }
                else
                {
                    string identifier = mapDr["IDENTIFIER"].Value.ToString();

                    
                    //string sysCode = string.Empty;
                    SysMsgCode sysCode;
                    if (Cst.StatusActivation.REGULAR != statusActivation)
                    {
                        //sysCode = "SYS-05105"; //DEACTIV-LOCKED
                        sysCode = new SysMsgCode(SysCodeEnum.SYS, 5105); //DEACTIV-LOCKED
                        if (Cst.StatusActivation.MISSING == statusActivation)
                        {
                            //sysCode = "SYS-05106";
                            sysCode = new SysMsgCode(SysCodeEnum.SYS, 5106);
                        }

                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, sysCode, 0,
                            new LogParam(LogTools.IdentifierAndId(identifier, pIdT)),
                            new LogParam(statusActivation)));
                    }
                    if (false == m_PKGenProcess.IsEntryOrRequestUpdateEntry)
                    {
                        if (Cst.StatusBusiness.ALLOC != statusBusiness)
                        {

                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                         
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5107), 0,
                                new LogParam(LogTools.IdentifierAndId(identifier, pIdT)),
                                new LogParam(statusBusiness)));
                        }
                    }
                    codeReturn = Cst.ErrLevel.DATAIGNORE;
                }
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5104), 0, new LogParam(pIdT)));

                codeReturn = Cst.ErrLevel.DATAIGNORE;
            }
            return codeReturn;
        }
        #endregion InitPosKeepingData
        #region InitStatusPosRequest
        /// <summary>
        /// Initialisation du statut  
        /// </summary>
        // EG 20180221 [23769] Déplacé de PosKeepingTools
        public void InitStatusPosRequest(IPosRequest pPosRequest, int pIdPR, Nullable<int> pIdPR_Parent, ProcessStateTools.StatusEnum pStatus)
        {
            InitStatusPosRequest(pPosRequest, pIdPR, pIdPR_Parent, pStatus, null);
        }
        public void InitStatusPosRequest(IPosRequest pPosRequest, int pIdPR, Nullable<int> pIdPR_Parent, ProcessStateTools.StatusEnum pStatus, List<IPosRequest> pLstSubPosRequest)
        {
            pPosRequest.IdPR = pIdPR;

            pPosRequest.Status = pStatus;
            pPosRequest.StatusSpecified = true;

            pPosRequest.IdPR_PosRequestSpecified = pIdPR_Parent.HasValue;
            if (pPosRequest.IdPR_PosRequestSpecified)
                pPosRequest.IdPR_PosRequest = pIdPR_Parent.Value;

            if (null != pLstSubPosRequest)
                AddSubPosRequest(pLstSubPosRequest, pPosRequest);
        }
        #endregion InitStatusPosRequest

        #region InsertNominalQuantityEvent
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20190613 [24683] Upd (PosKeepingData)
        protected virtual Cst.ErrLevel InsertNominalQuantityEvent(IDbTransaction pDbTransaction, int pIdT, ref int pIdE, int pIdE_Event, 
            DateTime pDtBusiness, bool pIsDealerBuyer, decimal pQty, decimal pRemainQty, int pIdPADET)
        {
            return InsertNominalQuantityEvent(pDbTransaction, PosKeepingData, pIdT, ref pIdE, pIdE_Event, pDtBusiness, pIsDealerBuyer, pQty, pRemainQty, pIdPADET);
        }
        protected virtual Cst.ErrLevel InsertNominalQuantityEvent(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, int pIdT, ref int pIdE, int pIdE_Event,
            DateTime pDtBusiness, bool pIsDealerBuyer, decimal pQty, decimal pRemainQty, int pIdPADET)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertNominalQuantityEvent
        #region InsertPosAction
        /// <summary>
        /// Insertion de la ligne de traitement de la clôture
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdPA">Identifiant du traitement de la clôture</param>
        /// <param name="pBusinessDate">Date de journée</param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180906 PERF Add DTOUT (Alloc ETD only)
        // EG 20190613 [24683] Test MaxValue
        public void InsertPosAction(IDbTransaction pDbTransaction, int pIdPA, DateTime pDtBusiness, int pIdPR)
        {
            DateTime dtOut = DateTime.MinValue;
            if ((this is PosKeepingGen_ETD) && (null != m_PosKeepingData) && (null != m_PosKeepingData.Asset))
            {
                dtOut = ((PosKeepingAsset_ETD)m_PosKeepingData.Asset).maturityDateSys;
                // FI 20190222 [24502] 63 jours à la place de 2 mois 
                // FI 20190320 [24594] add test != DateTime.MaxValue
                if (DtFunc.IsDateTimeFilled(dtOut) && dtOut != DateTime.MaxValue.Date)
                    dtOut = dtOut.AddDays(63);
                
            }
            InsertPosAction(pDbTransaction, pIdPA, pDtBusiness, pIdPR, dtOut);
        }
        // EG 20180906 PERF Add DTOUT (Alloc ETD only)
        public void InsertPosAction(IDbTransaction pDbTransaction, int pIdPA, DateTime pDtBusiness, int pIdPR, DateTime pDtOut)
        {
            string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.POSACTION.ToString();
            sqlQuery += @"(IDPA, IDPR, DTBUSINESS, DTOUT, DTINS, IDAINS) values (@IDPA, @IDPR, @DTBUSINESS, @DTOUT, @DTINS, @IDAINS)";
            // FI 20200820 [25468] Dates systems en UTC
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDPA", DbType.Int32), pIdPA);
            parameters.Add(new DataParameter(CS, "IDPR", DbType.Int32), pIdPR);
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTINS), OTCmlHelper.GetDateSysUTC(CS));
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDAINS), m_PKGenProcess.Session.IdA);
            parameters.Add(new DataParameter(CS, "DTOUT", DbType.Date), Convert.DBNull);

            if (DtFunc.IsDateTimeFilled(pDtOut))
                parameters["DTOUT"].Value = pDtOut;
            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
        }
        #endregion InsertPosAction
        #region InsertPosActionDet
        /// <summary>
        /// Insertion de la ligne matérialisant la clôture (Identifiant clôturée, clôturante et quantité clôturée)
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdPA">Identifiant du traitement de la clôture</param>
        /// <param name="pIdPADET">Identifiant de la ligne matérialisant la clôture</param>
        /// <returns></returns>
        /// EG 20141205 Add Parameter pPositionEffect
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20180307 [23769] Gestion dbTransaction
        public Cst.ErrLevel InsertPosActionDet(IDbTransaction pDbTransaction, int pIdPA, int pIdPADET,
                                                  Nullable<int> pIdT_Buy, Nullable<int> pIdT_Sell, int pIdT_Closing, decimal pQty, string pPositionEffect)
        {
            string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.POSACTIONDET.ToString();
            sqlQuery += @"(IDPA, IDPADET, IDT_BUY, IDT_SELL, IDT_CLOSING, QTY, POSITIONEFFECT, DTINS, IDAINS)";
            sqlQuery += "values (@IDPA, @IDPADET, @IDT_BUY, @IDT_SELL, @IDT_CLOSING, @QTY, @POSITIONEFFECT, @DTINS, @IDAINS)";
            
            // FI 20200820 [25468] Dates systemes en UTC
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDPA", DbType.Int32), pIdPA);
            parameters.Add(new DataParameter(CS, "IDPADET", DbType.Int32), pIdPADET);
            parameters.Add(new DataParameter(CS, "IDT_BUY", DbType.Int32), pIdT_Buy ?? Convert.DBNull);
            parameters.Add(new DataParameter(CS, "IDT_SELL", DbType.Int32), pIdT_Sell ?? Convert.DBNull);
            parameters.Add(new DataParameter(CS, "IDT_CLOSING", DbType.Int32), pIdT_Closing);
            // EG 20150920 [21374] Int32 to Int64
            // EG 20170127 Qty Long To Decimal
            parameters.Add(new DataParameter(CS, "QTY", DbType.Decimal), pQty);
            parameters.Add(new DataParameter(CS, "POSITIONEFFECT", DbType.AnsiStringFixedLength, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN),
                StrFunc.IsFilled(pPositionEffect) ? pPositionEffect : Convert.DBNull);
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTINS), OTCmlHelper.GetDateSysUTC(CS));
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDAINS), m_PKGenProcess.Session.IdA);

            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertPosActionDet
        #region InsertPosActionDetAndLinkedEvent
        /// <summary>
        /// Insertion des éléments matérialisant la clôture dans POSACTIONDET/EVENTPOSACTIONDET/EVENT
        /// pour la négociation clôturante et clôturée
        /// ATTENTION dans le cas des dénouements d'options et des corrections sur position la colonne IDT_CLOSING = Négociation traitée soit:
        /// (IDT_CLOSING = IDT_BUY et IDT_SELL = null) ou (IDT_CLOSING = IDT_SELL et IDT_BUY = null)
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdPA">Identifiant POSACTION</param>
        /// <param name="pIdPADET">Identifiant POSACTIONDET</param>
        /// <param name="pIdE">Identifiant EVENT</param>
        /// <param name="pRowPosActionDet">Nouvelles données de clôture</param>
        /// <param name="pRowClosing">Données de la négociation clôturante</param>
        /// <param name="pRowClosed">Données de la négociation clôturée</param>
        /// <returns></returns>
        protected Cst.ErrLevel InsertPosActionDetAndLinkedEvent(IDbTransaction pDbTransaction, int pIdPA, int pIdPADET, ref int pIdE,
                                                                DataRow pRowPosActionDet, DataRow pRowClosing, DataRow pRowClosed)
        {
            // POSACTIONDET
            Nullable<int> idT_Buy = null;
            if (false == Convert.IsDBNull(pRowPosActionDet["IDT_BUY"]))
                idT_Buy = Convert.ToInt32(pRowPosActionDet["IDT_BUY"]);
            Nullable<int> idT_Sell = null;
            if (false == Convert.IsDBNull(pRowPosActionDet["IDT_SELL"]))
                idT_Sell = Convert.ToInt32(pRowPosActionDet["IDT_SELL"]);
            int idT_Closing = Convert.ToInt32(pRowPosActionDet["IDT_CLOSING"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal qty = Convert.ToDecimal(pRowPosActionDet["QTY"]);
            string positionEffect = pRowPosActionDet["POSITIONEFFECT"].ToString();
            // EG 20141205 positionEffect
            Cst.ErrLevel codeReturn = InsertPosActionDet(pDbTransaction, pIdPA, pIdPADET, idT_Buy, idT_Sell, idT_Closing, qty, positionEffect);

            // EVENT CLOTUREE
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != pRowClosed))
            {
                codeReturn = InsertPosKeepingEvents(pDbTransaction, pRowClosed, pRowClosing, pRowPosActionDet, pIdPADET, ref pIdE, true);
                pIdE++;
            }
            // EVENT CLOTURANTE
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != pRowClosing))
                codeReturn = InsertPosKeepingEvents(pDbTransaction, pRowClosing, pRowClosed, pRowPosActionDet, pIdPADET, ref pIdE, false);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            pRowPosActionDet["ORIGINALQTY_CLOSED"] = Convert.ToDecimal(pRowPosActionDet["ORIGINALQTY_CLOSED"]) - qty;
            pRowPosActionDet["ORIGINALQTY_CLOSING"] = Convert.ToDecimal(pRowPosActionDet["ORIGINALQTY_CLOSING"]) - qty;
            return codeReturn;
        }
        #endregion InsertPosActionDetAndLinkedEvent
        #region InsertPosKeepingEvents
        /// <summary>
        /// <para>Insertion d'un package Evénement : OFFSETTING/POSITIONCORRECTION</para>
        /// <para>Evénement           Code    Type        Class</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>OFFSETTING          OFS     PAR/TOT     GRP</para>
        /// <para>ou</para>
        /// <para>POSITION CORRECTION POC     PAR/TOT     GRP</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>NOMINAL             NOM     INT/TER     REC</para>
        /// <para>QUANTITY            QTY     INT/TER     REC</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>REALIZED MARGIN     RMG     LPC         REC</para>
        /// <para>VARIATION           VMG     LPC         REC</para>
        /// </summary>
        /// <param name="pDbTransaction">Current Transaction</param>
        /// <param name="pRow">ClosedTrade or ClosingTrade)</param>
        /// <param name="pRow2">if pRow = ClosedTrade then pRow2 = ClosingTrade else ClosedTrade</param>
        /// <param name="pActionQuantity"></param>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        // EG 20120327 Méthode InitPaymentForEvent déplacée dans Trade.cs de Process
        protected virtual Cst.ErrLevel InsertPosKeepingEvents(IDbTransaction pDbTransaction, DataRow pRow, DataRow pRow2,
            DataRow pRowPosActionDet, int pIdPADET, ref int pIdE, bool pIsClosed)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertPosKeepingEvents
        #region InsertPosRequestGroupLevel
        /// <summary>
        /// <para>Création d'un POSREQUEST de regroupement (EOD)</para>
        /// <para>Ajout dans la table POSREQUEST de l'émément créé</para>
        /// </summary>
        // EG 20180221 [23769] Déplacé de PosKeepingTools
        public IPosRequest InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum pRequestType, int pIdPR, IPosRequest pPosRequest, int pIdEM,
            ProcessStateTools.StatusEnum pStatus, int pIdPR_Parent, List<IPosRequest> pLstSubPosRequest, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            return InsertPosRequestGroupLevel(pRequestType, pIdPR, pPosRequest.IdA_Entity, pPosRequest.IdA_Css, pPosRequest.IdA_Custodian, pIdEM, pPosRequest.DtBusiness,
                pStatus, pIdPR_Parent, pLstSubPosRequest, pGroupProduct);
        }
        public IPosRequest InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum pRequestType, int pIdPR,
            int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness, ProcessStateTools.StatusEnum pStatus, int pIdPR_Parent,
            List<IPosRequest> pLstSubPosRequest, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            IPosRequest posRequestGroupLevel = m_Product.CreatePosRequestGroupLevel(pRequestType, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness);
            InitStatusPosRequest(posRequestGroupLevel, pIdPR, pIdPR_Parent, pStatus, pLstSubPosRequest);
            // EG 20170109 New currentGroupProduct
            posRequestGroupLevel.GroupProductSpecified = (pGroupProduct.HasValue);
            if (posRequestGroupLevel.GroupProductSpecified)
                posRequestGroupLevel.GroupProduct = pGroupProduct.Value;

            
            //PosKeepingTools.InsertPosRequest(CS, null, pIdPR, posRequestGroupLevel, m_PKGenProcess.appInstance, LogHeader.IdProcess, posRequestGroupLevel.idPR_PosRequest);
            PosKeepingTools.InsertPosRequest(CS, null, pIdPR, posRequestGroupLevel, m_PKGenProcess.Session, IdProcess, posRequestGroupLevel.IdPR_PosRequest);

            return posRequestGroupLevel;
        }
        #endregion InsertPosRequestGroupLevel

        #region InsertReversalFeesEvents
        protected virtual Cst.ErrLevel InsertReversalFeesEvents(IDbTransaction pDbTransaction, DataRow pRow, int pIdPADET, int pIdE_Event, ref int pIdE)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            int idE = pIdE;

            IPosRequestDetail detail = (IPosRequestDetail)m_PosRequest.DetailBase;
            if (detail.PaymentFeesSpecified)
            {
                foreach (IPayment payment in detail.PaymentFees)
                {
                    idE++;
                    int savIdE = idE;
                    codeReturn = m_EventQuery.InsertPaymentEvents(pDbTransaction, m_TradeLibrary.DataDocument, Convert.ToInt32(pRow["IDT"]),
                        payment, DtBusiness, 1, 1, ref idE, pIdE_Event);
                    // EG 20120511 Add EVENTPOSACTIONDET pour toutes les lignes de FRAIS
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        for (int i = savIdE; i <= idE; i++)
                        {
                            codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, i);
                            if (Cst.ErrLevel.SUCCESS != codeReturn)
                                break;
                        }
                    }
                    if (Cst.ErrLevel.SUCCESS != codeReturn)
                        break;
                }
            }
            return codeReturn;
        }
        #endregion InsertReversalFeesEvents
        #region InsertReversalSafekeepingEvents
        /// <summary>
        /// Insertion des frais de garde matérialisant la restitution lors d'unre action POT|POC
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        protected virtual Cst.ErrLevel InsertReversalSafekeepingEvents(IDbTransaction pDbTransaction, int pIdT, int pIdE_Event, decimal pQty, int pIdPADET)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            /* 
            IPosRequestDetail detail = (IPosRequestDetail)m_PosRequest.Detail;
            if (detail.isReversalSafekeepingSpecified && detail.isReversalSafekeeping && 
                PosKeepingData.tradeSpecified && (PosKeepingData.trade.dtBusiness <= DtBusiness) )
            {
                IDataReader dr = null;
                try
                {
                    Nullable<int> idA_Payer = null;
                    Nullable<int> idB_Payer = null;
                    Nullable<int> idA_Receiver = null;
                    Nullable<int> idB_Receiver = null;
                    Nullable<decimal> amount_SKP = null;
                    string currency_SKP = string.Empty;
                    Nullable<decimal> qty_SKP = null;

                    DataParameters parameters = new DataParameters(new DataParameter[] { });
                    parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);
                    parameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.DateTime), DtBusiness);
                    string sqlSelect = @"select ev.IDA_PAY, ev.IDB_PAY, ev.IDA_REC, ev.IDB_REC, ev.EVENTCODE, ev.EVENTTYPE, ev.VALORISATION, ev.UNIT, 
                    ev.DTSTARTUNADJ, ev.DTSTARTADJ, ev.DTENDUNADJ, ev.DTENDADJ, evd.DAILYQUANTITY, evd.CONTRACTMULTIPLIER, evd.FACTOR, evd.NOTIONALREFERENCE
                    from dbo.EVENT ev
                    inner join EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.DTEVENT < @DTBUSINESS) and (ec.EVENCLASS = 'REC')
                    inner join EVENT evp on (evp.IDE = ev.IDE_EVENT) and (evp.EVENTCODE = 'LPC' and evp.EVENTTYPE = 'AMT')
                    where (IDT = @IDT)" + Cst.CrLf;

                    int newIdE = 0;

                    dr = DataHelper.ExecuteReader(CS, CommandType.Text, sqlSelect, parameters.GetArrayDbParameter());
                    while (dr.Read())
                    {
                        // Inversion des Payer/Receiver
                        if (false == Convert.IsDBNull(dr["IDA_PAY"])) idA_Payer = Convert.ToInt32(dr["IDA_PAY"]);
                        if (false == Convert.IsDBNull(dr["IDB_PAY"])) idB_Payer = Convert.ToInt32(dr["IDB_PAY"]);
                        if (false == Convert.IsDBNull(dr["IDA_REC"])) idA_Receiver = Convert.ToInt32(dr["IDA_REC"]);
                        if (false == Convert.IsDBNull(dr["IDB_REC"])) idB_Receiver = Convert.ToInt32(dr["IDB_REC"]);
                        if (false == Convert.IsDBNull(dr["DAILYQUANTITY"])) qty_SKP = Convert.ToDecimal(dr["DAILYQUANTITY"]);
                        if (false == Convert.IsDBNull(dr["VALORISATION"])) amount_SKP = Convert.ToDecimal(dr["VALORISATION"]) * pQty / qty_SKP;
                        if (false == Convert.IsDBNull(dr["UNIT"])) currency_SKP = dr["UNIT"].ToString();

                        

                        SQLUP.GetId(out newIdE, pDbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
                        codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, newIdE, pIdE_Event, null, 1, 1, idA_Receiver, idB_Receiver, idA_Payer, idB_Payer,
                            dr["EVENTCODE"].ToString(), dr["EVENTTYPE"].ToString(), 
                            Convert.ToDateTime(dr["DTSTARTUNADJ"]), Convert.ToDateTime(dr["DTSTARTADJ"]), Convert.ToDateTime(dr["DTENDUNADJ"]), Convert.ToDateTime(dr["DTENDADJ"]),
                            amount_SKP, currency_SKP, UnitTypeEnum.Currency.ToString(), null, null);

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, newIdE, EventClassFunc.Recognition, DtBusiness, false);

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = m_EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, newIdE);


                    }
                    #region Insertion des frais proratés


                    #endregion Insertion des frais proratés

                }
                catch (Exception)
                {
                    codeReturn = Cst.ErrLevel.FAILURE;
                }
                finally
                {
                    if (null != dr)
                        dr.Close();
                }
            }
            */
            return codeReturn;
        }
        #endregion InsertReversalSafekeepingEvents
        #region InsertRealizedMarginEvent
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdE"></param>
        /// <param name="pIdE_Event"></param>
        /// <param name="pIsDealerBuyer"></param>
        /// <param name="pPrice"></param>
        /// <param name="pClosingPrice"></param>
        /// <param name="pQty"></param>
        /// <param name="pIdPADET"></param>
        /// <param name="pIsClosed"></param>
        /// <returns></returns>
        // EG 20140410 Add pIsClosed parameter (pour mise à jour dans EVENTDET des données PRICE et CLOSINGPRICE dans les bonnes colonnes = Cas de la MAJ des clôtures)
        // PM 20140514 [19970][19259] Utilisation de PosKeepingData.asset.currency au lieu de PosKeepingData.asset.priceCurrency
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150616 [21124] New EventClass VAL : ValueDate
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20190613 [24683] Upd (PosKeepingData)
        // EG 20190730 Add TypePrice parameter
        protected Cst.ErrLevel InsertRealizedMarginEvent(IDbTransaction pDbTransaction, int pIdT, ref int pIdE, int pIdE_Event,
            bool pIsDealerBuyer, Nullable<decimal> pPrice, Nullable<decimal> pClosingPrice, decimal pQty, int pIdPADET, bool pIsClosed)
        {
            return InsertRealizedMarginEvent(pDbTransaction, pIdT, ref pIdE, pIdE_Event, pIsDealerBuyer, null, pPrice, pClosingPrice, pQty, pIdPADET, pIsClosed);
        }
        // EG 20190730 Add TypePrice parameter
        protected Cst.ErrLevel InsertRealizedMarginEvent(IDbTransaction pDbTransaction, int pIdT, ref int pIdE, int pIdE_Event,
        bool pIsDealerBuyer, Nullable<AssetMeasureEnum> pTypePrice, Nullable<decimal> pPrice, Nullable<decimal> pClosingPrice, decimal pQty, int pIdPADET, bool pIsClosed)
        {

            #region EVENT RMG (Realized margin)
            // EG 20150130 [20726]
            Nullable<decimal> realizedMargin = PosKeepingData.RealizedMargin(pClosingPrice, pPrice, pQty);
            PayerReceiverAmountInfo _payrec = new PayerReceiverAmountInfo(PosKeepingData, EventTypeFunc.RealizedMargin, pIsDealerBuyer);
            _payrec.SetPayerReceiver(realizedMargin);
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1,
    _payrec.IdA_Payer, _payrec.IdB_Payer, _payrec.IdA_Receiver, _payrec.IdB_Receiver,
    EventCodeFunc.LinkedProductClosing, EventTypeFunc.RealizedMargin, DtBusiness, DtBusiness, DtBusiness, DtBusiness,
    realizedMargin, PosKeepingData.Asset.currency, UnitTypeEnum.Currency.ToString(), null, null);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, DtBusiness, false);

            // EG 20150616 [21124]
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, DtBusiness, false);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                codeReturn = InsertEventDet(pDbTransaction, PosKeepingData, pIdE, pQty, PosKeepingData.Asset.contractMultiplier, 
                    pTypePrice, pIsClosed ? pPrice : pClosingPrice, pIsClosed ? pClosingPrice : pPrice);
            }
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);
            #endregion EVENT RMG (Realized margin)

            return codeReturn;
        }
        #endregion InsertRealizedMarginEvent
        #region InsertVariationMarginEvent
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        protected Cst.ErrLevel InsertVariationMarginEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event, bool pIsDealerBuyer, 
            decimal pPrice, decimal pQty)
        {
            return InsertVariationMarginEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, pIsDealerBuyer, pPrice, pQty, DateTime.MinValue);
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        protected virtual Cst.ErrLevel InsertVariationMarginEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event, bool pIsDealerBuyer,
            decimal pPrice, decimal pQty, DateTime pDtControl)
        {
            return Cst.ErrLevel.UNDEFINED;
        }
        #endregion InsertVariationMarginEvent
        #region InsertUnclearingEvents
        /// <summary>
        /// <para>Insertion d'un package Evénement : UNCLEARING</para>
        /// <para>Evénement           Code    Type        Class</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>UNCLEARING          UOF     PAR/TOT     GRP</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>VARIATION           VMG     LPC         REC</para>
        /// </summary>
        /// <param name="pDbTransaction">Current Transaction</param>
        /// <param name="pRow">Unclearing Event</param>
        /// <param name="pUnclearingQty">unclearing qty</param>
        /// <returns></returns>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20160106 [21679][POC-MUREX]
        // EG 20170127 Qty Long To Decimal
        protected Cst.ErrLevel InsertUnclearingEvents(IDbTransaction pDbTransaction, DataRow pRowEventPosActionDet, DataRow pRowEvent, int pIdPADET,
            bool pIsPartialUnclearing, decimal pUnclearingQty, DateTime pDtOffsetting)
        {
            #region Variables
            int idT = Convert.ToInt32(pRowEvent["IDT"]);
            int idE_Event = Convert.ToInt32(pRowEvent["IDE_EVENT"]);

            // EG 20160106 [21679][POC-MUREX] pDbTransaction replace CS
            _ = SQLUP.GetId(out int newIdE, pDbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 2);

            bool isDealerBuyer = IsTradeBuyer(pRowEventPosActionDet["SIDE"].ToString());
            #endregion Variables

            #region EVENT UOF (Unclearing OffSetting)
            string eventCode = PosKeepingEventCode;
            string eventType = pIsPartialUnclearing ? EventTypeFunc.Partiel : EventTypeFunc.Total;
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEvent(pDbTransaction, idT, newIdE, idE_Event, null, 1, 1, null, null, null, null,
                eventCode, eventType, DtBusiness, DtBusiness, DtBusiness, DtBusiness,
                pUnclearingQty, null, UnitTypeEnum.Qty.ToString(), null, null);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, newIdE, EventClassFunc.GroupLevel, DtBusiness, false);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, newIdE);
            #endregion EVENT UOF (Unclearing OffSetting)

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region EVENT VMG
                idE_Event = newIdE;
                newIdE++;
                decimal price = Convert.ToDecimal(pRowEventPosActionDet["PRICE"]);
                if (IsFuturesStyleMarkToMarket)
                {
                    // EG 20130516 Calcul VMG correctif sauf sur les UNCLEARING d'un OFFSETTING JOUR
                    if (pDtOffsetting < DtBusiness)
                    {
                        codeReturn = InsertVariationMarginEvent(pDbTransaction, idT, newIdE, idE_Event, isDealerBuyer, price, pUnclearingQty, pDtOffsetting);
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, newIdE);
                    }
                }
                #endregion EVENT VMG
            }
            return codeReturn;
        }
        #endregion InsertPosKeepingEvents
        #region IsSubRequest_Error
        // EG 20141013 
        // EG 20180307 [23769] Gestion List<IPosRequest>
        protected bool IsSubRequest_Error(Cst.PosRequestTypeEnum pListRequestType)
        {
            IPosRequest _posRequest = null;
            // FI 20181219 [24399] Add test valeur null avant de faire le lock
            if (null != m_LstSubPosRequest)
            {
                object spinLock = ((ICollection)m_LstSubPosRequest).SyncRoot;
                lock (spinLock)
                {
                    _posRequest = (from item in m_LstSubPosRequest
                                   where ((pListRequestType & item.RequestType) == item.RequestType) &&
                                           (item.StatusSpecified && ProcessStateTools.IsStatusError(item.Status))
                                   select item
                                    ).ToList().FirstOrDefault();
                }
            }
            return (null != _posRequest);
        }
        #endregion IsSubRequest_Error
        #region IsSubRequest_Status
        // EG 20180307 [23769] Gestion List<IPosRequest>
        protected bool IsSubRequest_Status(Cst.PosRequestTypeEnum pListRequestType, ProcessStateTools.StatusEnum pStatusToCompare)
        {
            // FI 20181219 [24399] Add test valeur null avant de faire le lock
            IPosRequest _posRequest = null;
            if (null != m_LstSubPosRequest)
            {
                object spinLock = ((ICollection)m_LstSubPosRequest).SyncRoot;
                lock (spinLock)
                {
                    _posRequest = (from item in m_LstSubPosRequest
                                   where ((pListRequestType & item.RequestType) == item.RequestType) &&
                                           (item.StatusSpecified && ((pStatusToCompare & item.Status) == item.Status))
                                   select item
                                    ).ToList().FirstOrDefault();
                }
            }
            return (null != _posRequest);
        }
        #endregion IsSubRequest_Status

        #region PosActionDetCalculation
        // EG 20120327 Méthode InitPaymentForEvent déplacée dans Trade.cs de Process
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel PosActionDetCalculation(decimal pAvailableQty, decimal pQty, IPayment[] pPaymentFees)
        {
            IPosRequestDetail detail = (IPosRequestDetail)m_PosRequest.DetailBase;
            DataRow rowTradeClosing = m_DtPosition.Rows.Find(m_PosRequest.IdT);

            Cst.ErrLevel codeReturn;
            if (null != rowTradeClosing)
            {
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                decimal closableQty = Convert.ToDecimal(rowTradeClosing["QTY"]);
                if ((pQty <= closableQty) && (0 < pQty))
                {
                    int idT_Closing = Convert.ToInt32(rowTradeClosing["IDT"]);
                    string sideClosing = rowTradeClosing["SIDE"].ToString();
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    decimal closingQty = Math.Min(pQty, closableQty);

                    // Insertion de la clôture dans la dataTable m_DtPosActionDet_Working
                    DataRow newRow = m_DtPosActionDet.NewRow();
                    newRow.BeginEdit();
                    newRow["IDPA"] = 0;
                    newRow["IDT_CLOSING"] = idT_Closing;
                    newRow["SIDE_CLOSING"] = sideClosing;
                    if (IsTradeBuyer(sideClosing))
                    {
                        newRow["IDT_BUY"] = idT_Closing;
                        newRow["IDT_SELL"] = Convert.DBNull;
                    }
                    else
                    {
                        newRow["IDT_SELL"] = idT_Closing;
                        newRow["IDT_BUY"] = Convert.DBNull;
                    }
                    newRow["QTY"] = closingQty;
                    newRow["ORIGINALQTY_CLOSING"] = closableQty;
                    newRow["ORIGINALQTY_CLOSED"] = closableQty;
                    newRow["POSITIONEFFECT"] = Convert.DBNull;
                    newRow.EndEdit();
                    rowTradeClosing.BeginEdit();
                    rowTradeClosing["QTY"] = closableQty - closingQty;
                    rowTradeClosing.EndEdit();
                    m_DtPosActionDet.Rows.Add(newRow);

                    // EG 20140711 (See PosKeepingGen_XXX - Override)
                    SetPaymentFeesForEvent(idT_Closing, closingQty, pPaymentFees, detail);

                    codeReturn = Cst.ErrLevel.SUCCESS;
                }
                else if (0 < pQty)
                {
                    // FI 20141118 [XXXXX] ds le message d'erreur contient closableQty (véritable quantité disponible) plutôt que pAvailableQty (quantité disponible au moment de l'action)
                    codeReturn = Cst.ErrLevel.FAILURE;

                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5150), 0,
                        new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                        new LogParam(LogTools.IdentifierAndId(rowTradeClosing["IDENTIFIER"].ToString(), m_PosRequest.IdT)),
                        new LogParam(pQty),
                        new LogParam(closableQty),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
                }
                else
                {
                    codeReturn = Cst.ErrLevel.SUCCESS;
                }
            }
            else
            {
                codeReturn = Cst.ErrLevel.DATAIGNORE;
                // EG 20131206 [19308] Sur ce ticket le trade en demande de correction n'avait pas de book Clearer.
                string identifier = Queue.GetStringValueIdInfoByKey("TRADE");
                if (m_PosRequest.IdentifiersSpecified)
                    identifier = m_PosRequest.Identifiers.Trade;

                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5163), 0,
                    new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                    new LogParam(LogTools.IdentifierAndId(identifier, m_PosRequest.IdT)),
                    new LogParam(pQty),
                    new LogParam(pAvailableQty),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            }
            return codeReturn;
        }
        #endregion PosActionDetCalculation
        #region PosKeep_Updating
        protected Cst.ErrLevel PosKeep_Updating(int pNbTokenIdE, ref int pIdPA)
        {
            return PosKeep_Updating(null, pNbTokenIdE, ref pIdPA);
        }
        // EG 20151125 [20979] Refactoring
        // PL 20180309 UP_GETID use Shared Sequence on POSACTION/POSACTIONDET
        // EG 20201124 [XXXXX] DeletePosActionWithoutDetail n'est plus appelé en EOD
        protected Cst.ErrLevel PosKeep_Updating(IDbTransaction pDbTransaction, int pNbTokenIdE, ref int pIdPA)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IDbTransaction dbTransaction = pDbTransaction;
            bool isException = false;
            try
            {
                if (null == pDbTransaction)
                    dbTransaction = DataHelper.BeginTran(CS);
                //FI 20130814 [18884] Mise en commentaire des tests sur DeadLock (code temporaire pour diag)
                //try
                //{
                // EG 20151125 [20979] Refactoring
                Nullable<int> idPR = GetIdPR();
                if (idPR.HasValue)
                    codeReturn = DeleteAllPosActionAndLinkedEventByRequest(dbTransaction, idPR.Value);
                //}
                //catch (Exception ex)
                //{
                //    SQLErrorEnum err = DataHelper.AnalyseSQLException(CS, ex);
                //    if (err == SQLErrorEnum.DeadLock)
                //        throw new Exception("DeadLock on DeleteAllPosActionAndLinkedEventByRequest", ex);
                //    else
                //        throw new Exception("Error on DeleteAllPosActionAndLinkedEventByRequest", ex);
                //}

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    DataTable dtChanged = m_DtPosActionDet.GetChanges();
                    if (null != dtChanged)
                    {
                        m_DtPosition.PrimaryKey = new DataColumn[] { m_DtPosition.Columns["IDT"] };
                        DataRow rowTradeClosing = null;
                        DataRow rowTradeClosable = null;

                        #region GetId of POSACTION/POSACTIONDET/EVENT
                        int nbOfTokenIDPADET = dtChanged.Rows.Count;
                        // IDE = 6 EVTs 
                        // . OFS/TOT-PAR (PosKeepingEntry) ou POC/TOT-PAR (PosRequestCorrection)
                        // . TER-INT/NOM
                        // . TER-INT/QTY
                        // . LPP/VMG (PosRequestCorrection) ou LPP/PRM (PosRequestCorrection)
                        // . LPC/GAM (PosRequestCorrection|PosRequestCancellation)
                        // . OPP (PosRequestCorrection)
                        int newIdPADET = 0;
                        int newIdE = 0;
                        int newIdPR = 0;

                        //FI 20130814 [18884] Mise en commentaire des tests sur DeadLock (code temporaire pour diag)
                        //try
                        //{
                        codeReturn = SQLUP.GetId(out int newIdPA, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);
                        //if (Cst.ErrLevel.SUCCESS == codeReturn)
                        //    codeReturn = SQLUP.GetId(out newIdPADET, dbTransaction, SQLUP.IdGetId.POSACTIONDET, SQLUP.PosRetGetId.First, nbOfTokenIDPADET);
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            newIdPADET = newIdPA;
                            codeReturn = SQLUP.GetId(out newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbOfTokenIDPADET * pNbTokenIdE);
                        }
                        //}
                        //catch (Exception ex)
                        //{
                        //    SQLErrorEnum err = DataHelper.AnalyseSQLException(CS, ex);
                        //    if (err == SQLErrorEnum.DeadLock)
                        //        throw new Exception("DeadLock on GetId", ex);
                        //    else
                        //        throw new Exception("Error on GetId", ex);
                        //}



                        // EG 20120504 Valeur retour de IDPA (used by transfert)
                        pIdPA = newIdPA;

                        #endregion GetId of POSACTION/POSACTIONDET/EVENT

                        #region Insertion dans POSACTION / POSREQUEST (Entry)
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            if (m_PKGenProcess.IsEntry)
                                codeReturn = SetPosRequestEntry(dbTransaction, out newIdPR);
                            else
                                newIdPR = m_PosRequest.IdPR;

                            //FI 20130814 [18884] Mise en commentaire des tests sur DeadLock (code temporaire pour diag)
                            //try
                            //{
                            InsertPosAction(dbTransaction, newIdPA, DtBusiness, newIdPR);
                            //}
                            //catch (Exception ex)
                            //{
                            //    SQLErrorEnum err = DataHelper.AnalyseSQLException(CS, ex);
                            //    if (err == SQLErrorEnum.DeadLock)
                            //        throw new Exception("DeadLock on InsertPosAction", ex);
                            //    else
                            //        throw new Exception("Error on InsertPosAction", ex);
                            //}
                        }
                        #endregion Insertion dans POSACTION / POSREQUEST (Entry)

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            #region Lecture des lignes de POSACTIONDET post traitement
                            foreach (DataRow row in dtChanged.Rows)
                            {
                                // On récupère le DataRow de la négociation clôturante
                                DataRowVersion rowVersion = DataRowVersion.Current;
                                if (row.RowState == DataRowState.Deleted)
                                    rowVersion = DataRowVersion.Original;

                                rowTradeClosing = m_DtPosition.Rows.Find(row["IDT_CLOSING", rowVersion]);
                                // On récupère le DataRow de la négociation clôturée
                                // dans le cas des corrections rowTradeClosable => null
                                if (IsTradeBuyer(row["SIDE_CLOSING", rowVersion].ToString()))
                                    rowTradeClosable = m_DtPosition.Rows.Find(row["IDT_SELL", rowVersion]);
                                else
                                    rowTradeClosable = m_DtPosition.Rows.Find(row["IDT_BUY", rowVersion]);

                                // EG 20170206 Delete des paiements sur livraison physique (opérés dans un traitement précédent)
                                codeReturn = DeleteEventPhysicalPeriodicDelivery(dbTransaction, rowTradeClosing, rowTradeClosable);

                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    if (row.RowState == DataRowState.Deleted)
                                    {
                                        // Clôture supprimée 
                                        codeReturn = DeletePosActionDetAndLinkedEvent(dbTransaction, Convert.ToInt32(row["IDPADET", rowVersion]));
                                    }
                                    else if (row.RowState == DataRowState.Modified)
                                    {
                                        // Clôture modifiée
                                        codeReturn = DeletePosActionDetAndLinkedEvent(dbTransaction, Convert.ToInt32(row["IDPADET"]));
                                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                                            codeReturn = InsertPosActionDetAndLinkedEvent(dbTransaction, newIdPA, newIdPADET, ref newIdE,
                                                                                          row, rowTradeClosing, rowTradeClosable);
                                    }
                                    else if (row.RowState == DataRowState.Added)
                                    {
                                        // Nouvelle clôture 
                                        codeReturn = InsertPosActionDetAndLinkedEvent(dbTransaction, newIdPA, newIdPADET, ref newIdE,
                                                                                      row, rowTradeClosing, rowTradeClosable);
                                    }
                                }

                                //newIdPADET++;
                                //newIdE++;
                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    codeReturn = SQLUP.GetId(out newIdPADET, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);
                                    newIdE++;
                                }
                                if (Cst.ErrLevel.SUCCESS != codeReturn)
                                {
                                    isException = true;
                                    break;
                                }
                            }
                            #endregion Lecture des lignes de POSACTIONDET post traitement
                        }
                    }
                    else
                    {
                        if ((null != m_MasterPosRequest) && (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay))
                            codeReturn = Cst.ErrLevel.NOTHINGTODO;
                        else
                            codeReturn = Cst.ErrLevel.SUCCESS;
                    }
                }

                if ((null == pDbTransaction) && (null != dbTransaction) && (false == isException))
                    DataHelper.CommitTran(dbTransaction);

                return codeReturn;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null == pDbTransaction) && (null != dbTransaction))
                {
                    if (isException)
                        DataHelper.RollbackTran(dbTransaction);
                    dbTransaction.Dispose();
                }
            }
        }
        #endregion PosKeep_Updating

        #region RemoveTrade
        
        /// <summary>
        ///  Annulation d'un trade (Appel au process TRADEACTIONGEN)
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pNotes"></param>
        /// <returns></returns>
        /// EG 20150218 Remove pGProduct parameter
        protected Cst.ErrLevel RemoveTrade(int pIdT, string pIdentifier, DateTime pDtBusiness, string pNotes)
        {
            return RemoveTrade(null, pIdT, pIdentifier, pDtBusiness, pNotes);
        }
        /// <summary>
        ///  Annulation d'un trade (Appel au process TRADEACTIONGEN)
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pNotes"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel RemoveTrade(IDbTransaction pDbTransaction, int pIdT, string pIdentifier, DateTime pDtBusiness, string pNotes)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            
            
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5053), 4,
                new LogParam(LogTools.IdentifierAndId(pIdentifier, pIdT)),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

            RemoveTradeMsg removeTradeMsg = new RemoveTradeMsg(pIdT, pIdentifier, pDtBusiness, null, pNotes);

            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString= CS,
                id = pIdT,
                requester = Queue.header.requester,
                idInfo = new IdInfo()
                {
                    id = pIdT,
                    idInfos = new DictionaryEntry[3]{
                                    new DictionaryEntry("ident", "TRADE"),
                                    new DictionaryEntry("identifier", pIdentifier),
                                    new DictionaryEntry("GPRODUCT", GProduct)
                    }
                }
            };
            
            TradeActionGenMQueue mQueue = new TradeActionGenMQueue(mQueueAttributes, removeTradeMsg);
            
            ProcessState processState = New_TradeActionGenAPI.ExecuteSlaveCall(pDbTransaction, mQueue, m_PKGenProcess, true, true);
            
            if (false == ProcessStateTools.IsCodeReturnSuccess(processState.CodeReturn))
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(processState.Status);

                
                Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(processState.Status), new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                    new LogParam(LogTools.IdentifierAndId(GetPosRequestLogValue(((PosKeepingRequestMQueue)Queue).requestType), Queue.id)),
                    new LogParam(Queue.GetStringValueIdInfoByKey("DTBUSINESS"))));

                codeReturn = processState.CodeReturn;
            }
            return codeReturn;
        }
        #endregion RemoveTrade
        #region RestorePosRequest
        /// <summary>
        ///  Affecte la variable m_PosRequest ( et m_LstSubPosRequest)
        /// </summary>
        /// <param name="pPosRequest"></param>
        /// EG 20180307 [23769] Gestion List<IPosRequest>
        protected void RestorePosRequest(IPosRequest pPosRequest)
        {
            // FI 20181219 [24399] Add test valeur null avant de faire le lock
            if (null != m_LstSubPosRequest)
            {
                object spinLock = ((ICollection)m_LstSubPosRequest).SyncRoot;
                lock (spinLock)
                {
                    m_PosRequest = pPosRequest;

                    m_LstSubPosRequest.ForEach(posRequest =>
                    {
                        if (posRequest.IdPR == m_PosRequest.IdPR)
                            posRequest = m_PosRequest;
                    });
                }
            }
        }
        #endregion RestorePosRequest
        #region RequestCanBeExecuted
        /// <summary>
        /// Retourne si une demande peut-être exécutée par lecture du STATUT des
        /// traitements EOD matchant avec la liste passé en paramètre (stockés dans m_LstSubPosRequest)
        /// </summary>
        /// <param name="pListRequestType">RequestType (List)</param>
        /// <returns>boolean</returns>
        /// FI 20130917 [18953] Add test if (ArrFunc.IsFilled(m_LstSubPosRequest)) 
        /// EG 20180307 [23769] Gestion List<IPosRequest>
        protected bool RequestCanBeExecuted(Cst.PosRequestTypeEnum pListRequestType)
        {
            bool ret = true;
            // FI 20181219 [24399] Add test valeur null avant de faire le lock
            if (null != m_LstSubPosRequest)
            {
                object spinLock = ((ICollection)m_LstSubPosRequest).SyncRoot;
                lock (spinLock)
                {
                    ret =
                    (
                        false == m_LstSubPosRequest.Exists(item =>
                        ((pListRequestType & item.RequestType) == item.RequestType) &&
                        (item.StatusSpecified && (ProcessStateTools.IsStatusErrorWarning(item.Status))))
                    );
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne si une demande peut-être exécutée par lecture des STATUT des
        /// traitements EOD matchant avec la liste passé en paramètre
        /// avec l'entité/marché ainsi que la clé de position spécifiée (stockés dans m_LstSubPosRequest)
        /// </summary>
        /// <param name="pListRequestType">RequestType (List)</param>
        /// <param name="pIdEM">Id Entité/Marché</param>
        /// <param name="pPosKeepingKey">Clé de position</param>
        /// <returns>boolean</returns>
        /// FI 20130917 [18953] Add test if (ArrFunc.IsFilled(m_LstSubPosRequest)) 
        /// EG 20180221 [23769] Public pour gestion asynchrone
        /// EG 20180307 [23769] Gestion List<IPosRequest>
        public bool RequestCanBeExecuted(Cst.PosRequestTypeEnum pListRequestType, int pIdEM, IPosKeepingKey pPosKeepingKey)
        {
            bool ret = true;
            // FI 20181219 [24399] Add test valeur null avant de faire le lock
            if (null != m_LstSubPosRequest)
            {
                object spinLock = ((ICollection)m_LstSubPosRequest).SyncRoot;
                lock (spinLock)
                {
                    ret =
                    (
                        false == m_LstSubPosRequest.Exists(item =>
                        ((pListRequestType & item.RequestType) == item.RequestType) &&
                        (item.StatusSpecified && (ProcessStateTools.IsStatusErrorWarning(item.Status))) &&
                        (item.IdEM == pIdEM) &&
                        (item.PosKeepingKeySpecified && (item.PosKeepingKey.LockObjectId == pPosKeepingKey.LockObjectId)))
                    );
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne si une demande peut-être exécutée par lecture des STATUTs des
        /// traitements EOD intermédiaires matchant avec l'id du trade passé en paramètre
        /// (stockés dans m_LstSubPosRequest)
        /// </summary>
        /// <param name="pListRequestType">RequestType (List)</param>
        /// <returns>boolean</returns>
        /// FI 20130917 [18953] Add test if (ArrFunc.IsFilled(m_LstSubPosRequest)) 
        /// EG 20180307 [23769] Gestion List<IPosRequest>
        protected bool RequestCanBeExecuted(Cst.PosRequestTypeEnum pListRequestType, int pIdT)
        {
            bool ret = true;
            // FI 20181219 [24399] Add test valeur null avant de faire le lock
            if (null != m_LstSubPosRequest)
            {
                object spinLock = ((ICollection)m_LstSubPosRequest).SyncRoot;
                lock (spinLock)
                {
                    ret =
                    (
                        false == m_LstSubPosRequest.Exists(item =>
                        ((pListRequestType & item.RequestType) == item.RequestType) &&
                        (item.StatusSpecified && (ProcessStateTools.IsStatusErrorWarning(item.Status))) &&
                        (item.IdTSpecified && (item.IdT == pIdT)))
                    );
                }
            }
            return ret;
        }
        #endregion RequestCanBeExecuted
        #region RecordTrade
        /// <summary>
        /// Insertion d'un nouveau trade lié à un trade existant
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDataDocument">Document du nouveau trade</param>
        /// <param name="pAdditionalInfo">Information nécéssaire à la saisie d'un trade</param>
        /// <param name="pIdPRSource"></param>
        /// <param name="pRequestTypeSource">Type de requête ayant provoqué l'insertion d'un nouveau trade</param>
        /// <param name="pTradeLinkInfo">IDT et Identifier du trade source à lier avec le nouveau trade (plusieur trades possibles dans le cas de l'action Merge)</param>
        /// <param name="pIdT">IDT du trade créé</param>
        /// <returns></returns>
        /// PM 20130220 [18414] Nouvelle méthode issue de l'extrait de l'ancienne méthode RecordUnderlyingTrade
        /// PM 20130308 [18434] Ajout shifting
        /// EG 20130805 [18859] List pour TradeLink
        /// FI 20161206 [22092] Modify
        /// FI 20170404 [23039] Modify
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180502 Analyse du code Correction [CA2200]
        // EG 20190603 [24683] Upd (Out pIdentifier)
        // EG 20190613 [24683] Upd (Closing/Reopening Positions)
        // EG 20200226 [25562] Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20210421 [25723] Gestion du dépassement de capacité sur la colonne DESCRIPTION (CA et CRP)
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        public Cst.ErrLevel RecordTrade(IDbTransaction pDbTransaction,
            DataDocumentContainer pDataDocument,
            IPosRequestTradeAdditionalInfo pAdditionalInfo,
            int[] pIdPRSource, Cst.PosRequestTypeEnum pRequestTypeSource,
            List<Pair<int, string>> pTradeLinkInfo, out int pIdT, out string pIdentifier)
        {
            int idT = 0;
            IDbTransaction dbTransaction = pDbTransaction;
            bool isDbTransactionSelfCommit = (null == pDbTransaction);
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {

                TradeCaptureGen captureGen = new TradeCaptureGen();
                TradeInput input = captureGen.Input;
                input.Identification = new SpheresIdentification();
                input.DataDocument = pDataDocument;

                // EG 20130805 [18859] List pour TradeLink
                if (pRequestTypeSource == Cst.PosRequestTypeEnum.CorporateAction)
                    input.Identification.Identifier = pTradeLinkInfo[0].Second;
                else
                    input.Identification.Identifier = pAdditionalInfo.SqlTemplateTrade.Identifier;

                input.Identification.OTCmlId = pAdditionalInfo.SqlTemplateTrade.Id;
                input.SQLTrade = pAdditionalInfo.SqlTemplateTrade;
                input.SQLProduct = pAdditionalInfo.SqlProduct;
                input.SQLInstrument = pAdditionalInfo.SqlInstrument;
                //status
                input.TradeStatus.Initialize(CS, dbTransaction, pAdditionalInfo.SqlTemplateTrade.IdT);
                input.TradeStatus.stEnvironment.CurrentSt = Cst.StatusEnvironment.REGULAR.ToString();
                input.TradeStatus.stActivation.CurrentSt = pAdditionalInfo.StActivation.ToString();
                input.TradeStatus.stPriority.CurrentSt = Cst.StatusPriority.REGULAR.ToString();
                lock (m_StUserLock)
                {
                    input.InitStUserFromPartiesRole(CSTools.SetCacheOn(CS), dbTransaction);
                }

                SetNotification(input);

                TradeRecordSettings recordSettings = new TradeRecordSettings
                {
                    extLink = string.Empty,
                    idScreen = pAdditionalInfo.ScreenName,
                    isGetNewIdForIdentifier = true,
                    isCheckValidationRules = true,
                    isCheckValidationXSD = true,
                    isCheckLicense = false
                };

                switch (pRequestTypeSource)
                {
                    case Cst.PosRequestTypeEnum.CorporateAction:
                        recordSettings.displayName = pAdditionalInfo.SqlInstrument.Identifier;
                        recordSettings.description = TradeIdentificationBuilder.CheckColumnDataCapacity(128, recordSettings.displayName + " - adjustment of ", pTradeLinkInfo[0].Second, " (post CA)");
                        IExchangeTradedDerivative _exTrade = (IExchangeTradedDerivative)pDataDocument.CurrentProduct.Product;
                        recordSettings.dtCorpoAction = _exTrade.TradeCaptureReport.ClearingBusinessDate.DateValue;
                        recordSettings.isCopyAttachedDoc = false;
                        recordSettings.isCopyNotePad = false;
                        recordSettings.isSaveUnderlyingInParticularTransaction = false;
                        recordSettings.dtRefForDtEnabled = recordSettings.dtCorpoAction;
                        break;
                    case Cst.PosRequestTypeEnum.PositionTransfer:
                        // EG 20141230 [20587]
                        if (StrFunc.IsFilled(pAdditionalInfo.DisplayName))
                            recordSettings.displayName = pAdditionalInfo.DisplayName;
                        else
                            recordSettings.displayName = pAdditionalInfo.SqlInstrument.Identifier;
                        if (StrFunc.IsFilled(pAdditionalInfo.Description))
                            recordSettings.description = pAdditionalInfo.Description;
                        else
                            recordSettings.description = recordSettings.displayName;
                        if (StrFunc.IsFilled(pAdditionalInfo.ExtlLink))
                            recordSettings.extLink = pAdditionalInfo.ExtlLink;
                        break;
                    case Cst.PosRequestTypeEnum.AutomaticOptionAssignment:
                    case Cst.PosRequestTypeEnum.AutomaticOptionExercise:
                    case Cst.PosRequestTypeEnum.OptionAssignment:
                    case Cst.PosRequestTypeEnum.OptionExercise:
                    case Cst.PosRequestTypeEnum.UnderlyerDelivery:
                        recordSettings.isSaveUnderlyingInParticularTransaction = false;
                        recordSettings.displayName = pAdditionalInfo.SqlInstrument.Identifier;
                        recordSettings.description = recordSettings.displayName + " - " + pRequestTypeSource.ToString();
                        break;
                    case Cst.PosRequestTypeEnum.ClosingPosition:
                        recordSettings.isSaveUnderlyingInParticularTransaction = false;
                        recordSettings.displayName = pAdditionalInfo.SqlInstrument.Identifier;
                        recordSettings.description = TradeIdentificationBuilder.CheckColumnDataCapacity(128, recordSettings.displayName + " - closing of ", pTradeLinkInfo[0].Second, null);
                        recordSettings.isCopyNotePad = false;
                        recordSettings.isCopyAttachedDoc = false;
                        recordSettings.isUpdateTradeXMLWithTradeLink = false;
                        recordSettings.typeForClosingReopeningPosition = PositionEffectEnum.Close;
                        IExchangeTradedDerivative _closingTrade = (IExchangeTradedDerivative)pDataDocument.CurrentProduct.Product;
                        recordSettings.dtRefForDtEnabled = _closingTrade.TradeCaptureReport.ClearingBusinessDate.DateValue.AddDays(-1); // GLOP EG 20230821
                        break;
                    case Cst.PosRequestTypeEnum.ClosingReopeningPosition:
                        recordSettings.isSaveUnderlyingInParticularTransaction = false;
                        recordSettings.displayName = pAdditionalInfo.SqlInstrument.Identifier;
                        recordSettings.description = TradeIdentificationBuilder.CheckColumnDataCapacity(128, recordSettings.displayName + " - reopening of ", pTradeLinkInfo[0].Second, null);
                        recordSettings.typeForClosingReopeningPosition = PositionEffectEnum.Open;
                        recordSettings.isCopyNotePad = false;
                        recordSettings.isCopyAttachedDoc = false;
                        recordSettings.isUpdateTradeXMLWithTradeLink = false;
                        break;
                    default:
                        // EG 20141230 [20587]
                        recordSettings.displayName = pAdditionalInfo.SqlInstrument.Identifier;
                        recordSettings.description = recordSettings.displayName + " - " + pRequestTypeSource.ToString();
                        break;
                }

                
                string identifier = string.Empty;
                // FI 20170404 [23039] gestion de underlying et trader
                Pair<int, string>[] underlying = null;
                Pair<int, string>[] trader = null;

                
                TradeCommonCaptureGen.ErrorLevel lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
                TradeCommonCaptureGenException errExc = null;
                try
                {
                    #region Start Transaction
                    //Begin Tran  doit être la 1er instruction Car si Error un  roolback est fait de manière systematique
                    if (isDbTransactionSelfCommit)
                    {
                        try { dbTransaction = DataHelper.BeginTran(CS); }
                        catch (Exception ex)
                        {
                            throw new TradeCommonCaptureGenException(System.Reflection.MethodInfo.GetCurrentMethod().Name,
                                ex, TradeCommonCaptureGen.ErrorLevel.BEGINTRANSACTION_ERROR);
                        }
                    }
                    #endregion Start Transaction

                    #region Enregistrement du Trade
                    CaptureSessionInfo sessionInfo = new CaptureSessionInfo
                    {
                        user = new User(ProcessBase.Session.IdA, null, RoleActor.SYSADMIN),
                        session = ProcessBase.Session,
                        licence = ProcessBase.License,
                        idProcess_L = ProcessBase.IdProcess,
                        idTracker_L = ProcessBase.Tracker.IdTRK_L
                    };

                    captureGen.CheckAndRecord(CS, dbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade), Cst.Capture.ModeEnum.New, sessionInfo, recordSettings,
                        ref identifier, ref idT,
                        out underlying, out trader);
                    #endregion Enregistrement du Trade

                    #region TradeLink
                    TradeLinkType tradeLinkType = default;
                    switch (pRequestTypeSource)
                    {
                        case Cst.PosRequestTypeEnum.OptionAssignment:
                            tradeLinkType = TradeLinkType.UnderlyerDeliveryAfterOptionAssignment;
                            break;
                        case Cst.PosRequestTypeEnum.AutomaticOptionAssignment:
                            tradeLinkType = TradeLinkType.UnderlyerDeliveryAfterAutomaticOptionAssignment;
                            break;
                        case Cst.PosRequestTypeEnum.OptionExercise:
                            tradeLinkType = TradeLinkType.UnderlyerDeliveryAfterOptionExercise;
                            break;
                        case Cst.PosRequestTypeEnum.AutomaticOptionExercise:
                            tradeLinkType = TradeLinkType.UnderlyerDeliveryAfterAutomaticOptionExercise;
                            break;
                        case Cst.PosRequestTypeEnum.Cascading:
                            tradeLinkType = TradeLinkType.PositionAfterCascading;
                            break;
                        case Cst.PosRequestTypeEnum.Shifting:
                            tradeLinkType = TradeLinkType.PositionAfterShifting;
                            break;
                        case Cst.PosRequestTypeEnum.CorporateAction:
                            tradeLinkType = TradeLinkType.PositionAfterCorporateAction;
                            break;
                        case Cst.PosRequestTypeEnum.TradeMerging:
                            tradeLinkType = TradeLinkType.MergedTrade;
                            break;
                        case Cst.PosRequestTypeEnum.TradeSplitting:
                            tradeLinkType = TradeLinkType.SplitTrade;
                            break;
                        case Cst.PosRequestTypeEnum.PositionTransfer:
                            // EG 20141224 [20566]
                            tradeLinkType = TradeLinkType.PositionTransfert;
                            break;
                        case Cst.PosRequestTypeEnum.ClosingPosition:
                            // EG 20141224 [20566]
                            tradeLinkType = TradeLinkType.ClosingPosition;
                            break;
                        case Cst.PosRequestTypeEnum.ClosingReopeningPosition:
                            // EG 20141224 [20566]
                            tradeLinkType = TradeLinkType.ReopeningPosition;
                            break;
                    }

                    TradeLinkDataIdentification tradeLinkData = TradeLinkDataIdentification.TradeIdentifier;

                    pTradeLinkInfo.ForEach(x =>
                    {
                        TradeLink.TradeLink tradeLink = new TradeLink.TradeLink( idT, x.First, tradeLinkType,
                                    null, null,
                                    new string[2] { x.Second, identifier },
                                    new string[2] { tradeLinkData.ToString(), TradeLinkDataIdentification.TradeIdentifier.ToString() });
                        tradeLink.Insert(CS, dbTransaction);

                    });
                    // m_PosRequest.identifiers.trade
                    #endregion TradeLink

                    #region Link PosRequest
                    // EG 20130924 New: Insert du trade lié dans TRLINKPOSREQUEST
                    // Cas : Livraison sous-jacent sur dénouement, Trade ajusté sur CA
                    InsertTradeLinkPosRequest(dbTransaction, pIdPRSource, pRequestTypeSource, idT);
                    #endregion

                }
                catch (TradeCommonCaptureGenException ex)
                {
                    //Erreur reconnue
                    errExc = ex;
                    lRet = errExc.ErrLevel;
                }
                catch (Exception) { throw; }//Error non gérée
                finally
                {
                    #region End Transaction
                    if (isDbTransactionSelfCommit && (null != dbTransaction))
                    {
                        if (TradeCommonCaptureGen.ErrorLevel.SUCCESS != lRet)
                            DataHelper.RollbackTran(dbTransaction);
                        else
                            DataHelper.CommitTran(dbTransaction);

                        dbTransaction.Dispose();
                    }
                    #endregion End Transaction
                }
                //Si une exception se produit après l'enregistrement du trade=> cela veut dire que le trade est correctement rentré en base, on est en succès
                if ((null != errExc) && TradeCaptureGen.IsRecordInSuccess(errExc.ErrLevel))
                    lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;

                // FI 20170404 [23039] underlying, trader
                string msg = captureGen.GetResultMsgAfterCheckAndRecord(CS, errExc, Cst.Capture.ModeEnum.New, identifier, underlying, trader, true, out string msgDet);
                if (StrFunc.IsFilled(msgDet))
                    msg = msg + Cst.CrLf + msgDet;
                
                if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == lRet)
                {
                    Logger.Log(new LoggerData(LogLevelEnum.Info, msg, 4, new LogParam(idT, default, "TRADE", Cst.LoggerParameterLink.IDDATA)));
                }
                else
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, msg);

                pIdT = idT;
                pIdentifier = identifier;
                return codeReturn;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion RecordTrade
        #region RecordTradeEvent
        /// <summary>
        /// Génération des événements d'un trade inséré.
        /// </summary>
        /// <param name="pIdT">Id du trade inséré</param>
        /// <param name="pTradeIdentifier">Identifiant du trade inséré</param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20150302 GProduct accessor replace Cst.ProductGProduct_FUT
        // EG 20150302 "Trade" replace "UnderlyingTrade" (XslFileNameMain)
        // EG 20180307 [23769] Gestion dbTransaction
        protected Cst.ErrLevel RecordTradeEvent(int pIdT, string pTradeIdentifier)
        {
            return RecordTradeEvent(null, pIdT, pTradeIdentifier);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Upd (Use DbTransaction)
        protected Cst.ErrLevel RecordTradeEvent(IDbTransaction pDbTransaction, int pIdT, string pTradeIdentifier)
        {
            
            IdInfo idInfo = new IdInfo()
            {
                id = pIdT,
                idInfos = new DictionaryEntry[] {
                    new DictionaryEntry("ident", "TRADE"),
                    new DictionaryEntry("identifier", pTradeIdentifier),
                    new DictionaryEntry("GPRODUCT", GProduct) }
            };

            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = CS,
                id = pIdT,
                idInfo = idInfo,
                requester = Queue.header.requester,
            };

            EventsGenMQueue mQueue = new EventsGenMQueue(mQueueAttributes);

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 500), 5, new LogParam(LogTools.IdentifierAndId(pTradeIdentifier, pIdT))));

            // EG 20150302 GProduct accessor replace Cst.ProductGProduct_FUT
            ProcessState processState = New_EventsGenAPI.ExecuteSlaveCall(mQueue, pDbTransaction, m_PKGenProcess, true);

            return processState.CodeReturn;
        }
        #endregion RecordTradeEvent
        #region RecordTradeMerge
        // EG 20150723 New
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190613 [24683] Upd (Out identifierMerge)
        // EG 20220816 [XXXXX] [WI396] TradeMerge (idem Split) : Passage pDbTransaction en paramètre
        protected Cst.ErrLevel RecordTradeMerge(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocContainer, TradeCandidate pSourceTradeBase,
            List<TradeCandidate> pLstTradeMerge, SQL_TradeCommon pSqlTrade, out int pIdTMerge)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            string identifierMerge = string.Empty;
            IPosRequest _posRequest = m_Product.CreatePosRequest();
            _posRequest.IdTSpecified = true;
            _posRequest.IdT = pSourceTradeBase.IdT;
            _posRequest.PosKeepingKeySpecified = true;
            _posRequest.PosKeepingKey = m_Product.CreatePosKeepingKey();
            _posRequest.PosKeepingKey.IdI = pSourceTradeBase.Context.idI;
            // EG 20220816 [XXXXX] [WI396] TradeMerge (idem Split) : Passage pDbTransaction en paramètre
            codeReturn = SetAdditionalInfoFromTrade(pDbTransaction, _posRequest);

            IPosRequestTradeAdditionalInfo additionalInfo = (IPosRequestTradeAdditionalInfo)m_TemplateDataDocumentTrade[pSourceTradeBase.Context.idI];

            #region RECALCUL DES FRAIS
            // EG 20130911 [18076]
            TradeInput tradeInput = new TradeInput
            {
                SQLProduct = additionalInfo.SqlProduct,
                SQLTrade = pSqlTrade,
                DataDocument = pDataDocContainer
            };
            tradeInput.TradeStatus.Initialize(ProcessBase.Cs, pDbTransaction, pSqlTrade.IdT);
            tradeInput.RecalculFeeAndTax(CS, pDbTransaction);
            #endregion RECALCUL DES FRAIS

            // Listes des <IDT,IDENTIFIER> pour TradeLink
            List<Pair<int, string>> _tradeLink = (
                from trade in pLstTradeMerge
                select new Pair<int, string>(trade.IdT, trade.TradeIdentifier)).ToList();

            codeReturn = RecordTrade(pDbTransaction, pDataDocContainer, additionalInfo, null, Cst.PosRequestTypeEnum.TradeMerging, _tradeLink, out int idTMerge, out identifierMerge);

            pIdTMerge = idTMerge;
            return codeReturn;
        }
        #endregion RecordTradeMerge

        #region SetAdditionalInfoFromTrade
        // PM 20130219 [18414] et [18434] Pour la création des trades post Cascading et post Shifting
        // EG 20130417 Moidification pour appel à la création des trades ajustés suite à CA
        /// <summary>
        /// Complète la demande de cascading, shifting et CA reçu en paramètre à l'aide d'informations additionnelles sur l'Instrument
        /// </summary>
        /// <param name="pRequest">Demande de cascading, Shifting ou CA</param>
        /// <returns>Cst.ErrLevel</returns>
        protected Cst.ErrLevel SetAdditionalInfoFromTrade(IPosRequest pPosRequest)
        {
            return SetAdditionalInfoFromTrade(null, pPosRequest);
        }
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected Cst.ErrLevel SetAdditionalInfoFromTrade(IDbTransaction pDbTransaction, IPosRequest pPosRequest)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            string identifierDC = string.Empty;
            int idDC = 0;

            if (pPosRequest is IPosRequestCascadingShifting)
            {
                identifierDC = (pPosRequest.DetailBase as IPosRequestDetCascadingShifting).IdentifierDCDest;
                idDC = (pPosRequest.DetailBase as IPosRequestDetCascadingShifting).IdDCDest;
            }
            else if (pPosRequest is IPosRequestCorporateAction)
            {
                identifierDC = (pPosRequest.DetailBase as IPosRequestDetCorporateAction).IdentifierDCEx;
                idDC = (pPosRequest.DetailBase as IPosRequestDetCorporateAction).IdDCEx;
            }
            int idI = pPosRequest.PosKeepingKey.IdI;
            try
            {
                string logMessage = string.Empty;
                if (false == m_TemplateDataDocumentTrade.ContainsKey(idI))
                {
                    SQL_Instrument sqlInstrument = new SQL_Instrument(CS, idI)
                    {
                        DbTransaction = pDbTransaction
                    };
                    if (sqlInstrument.IsLoaded)
                    {
                        string templateIdentifier = string.Empty;
                        string screenName = string.Empty;

                        using (IDataReader dr = GetTemplate(CS, pDbTransaction, idI))
                        {
                            if (dr.Read())
                            {
                                templateIdentifier = dr.GetValue(0).ToString();
                                screenName = dr.GetValue(1).ToString();
                            }
                            else
                            {
                                codeReturn = Cst.ErrLevel.FAILURE;
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                                
                                
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5181), 0,
                                    new LogParam(GetPosRequestLogValue(pPosRequest.RequestType)),
                                    new LogParam(LogTools.IdentifierAndId(identifierDC, idDC)),
                                    new LogParam(LogTools.IdentifierAndId(sqlInstrument.Identifier, sqlInstrument.IdI))));
                            }
                        }
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CS, templateIdentifier)
                            {
                                IsWithTradeXML = true,
                                DbTransaction = pDbTransaction,
                                IsAddRowVersion = true
                            };
                            if (sqlTrade.LoadTable(new string[] { "TRADE.IDT", "IDENTIFIER", "trx.TRADEXML" }))
                            {
                                EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(sqlTrade.TradeXml);
                                IDataDocument dataDoc = (IDataDocument)CacheSerializer.Deserialize(serializerInfo);
                                DataDocumentContainer dataDocContainer = new DataDocumentContainer(dataDoc);

                                IPosRequestTradeAdditionalInfo additionalInfo = null;
                                SQL_Product sqlProduct = new SQL_Product(CS, sqlInstrument.IdP)
                                {
                                    DbTransaction = pDbTransaction
                                };
                                if (pPosRequest is IPosRequestCascadingShifting)
                                {
                                    additionalInfo = (pPosRequest.DetailBase as IPosRequestDetCascadingShifting).CreateAdditionalInfo(dataDocContainer,
                                        sqlTrade, sqlProduct, sqlInstrument, screenName);
                                }
                                else if (pPosRequest is IPosRequestCorporateAction)
                                {
                                    additionalInfo = (pPosRequest.DetailBase as IPosRequestDetCorporateAction).CreateAdditionalInfo(dataDocContainer,
                                        sqlTrade, sqlProduct, sqlInstrument, screenName);
                                }
                                else
                                {
                                    additionalInfo = (pPosRequest as IPosRequest).CreateAdditionalInfoStandard(dataDocContainer,
                                        sqlTrade, sqlProduct, sqlInstrument, screenName);
                                }

                                m_TemplateDataDocumentTrade.Add(idI, additionalInfo);
                            }
                        }
                    }
                    else
                    {
                        codeReturn = Cst.ErrLevel.FAILURE;

                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5182), 0,
                            new LogParam(GetPosRequestLogValue(pPosRequest.RequestType)),
                            new LogParam(LogTools.IdentifierAndId(identifierDC, idDC)),
                            new LogParam(idI)));
                    }
                }
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5181), 0,
                    new LogParam(GetPosRequestLogValue(pPosRequest.RequestType)),
                    new LogParam(LogTools.IdentifierAndId(identifierDC, idDC)),
                    new LogParam(LogTools.IdentifierAndId(null, idI))));
            }
            return codeReturn;
        }
        #endregion SetAdditionalInfoFromTrade
        #region SetPosKeepingQuote
        // EG 20160302 (21969] Add Cst.ErrLevel
        protected Cst.ErrLevel SetPosKeepingQuote()
        {
            return SetPosKeepingQuote(m_PosRequest);
        }
        // EG 20160302 (21969] Add Cst.ErrLevel
        protected virtual Cst.ErrLevel SetPosKeepingQuote(IPosRequest pPosRequest)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion SetPosKeepingQuote
        #region SetTradeMerge
        // EG 20150723 New
        protected virtual void SetTradeMerge(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocumentContainer, 
            TradeMergeRule pTmr, List<TradeCandidate> pLstTradeMerge, TradeCandidate pSourceTradeBase)
        {
        }
        #endregion SetTradeMerge

        #region UpdatePosRequest
        /// <summary>
        /// Mise à jour du POSREQUEST (avec mise à jour du statut en fonction de {pCodeReturn}) 
        /// <para>Le statut du POSREQUEST respecte la règle suivante</para>
        /// <para> si pCodeReturn = SUCCESS ou DATAIGNORE alors statut = SUCCESS</para>
        /// <para> si pCodeReturn = NOTHINGTODO alors statut = NONE</para>
        /// <para> sinon statut = ERROR</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCodeReturn"></param>
        /// <param name="pPosRequest"></param>
        /// <param name="pIdPR_Parent"></param>
        /// FI 20130917 [18953] Modification de la signature pIdPR_Parent est nullable
        /// EG 20140113 New Status Updating
        protected void UpdatePosRequest(Cst.ErrLevel pCodeReturn, IPosRequest pPosRequest, Nullable<int> pIdPR_Parent)
        {
            UpdatePosRequest(null, pCodeReturn, pPosRequest, pIdPR_Parent);
        }
        // EG 20180525 [23979] IRQ Processing
        protected void UpdatePosRequest(IDbTransaction pDbTransaction, Cst.ErrLevel pCodeReturn, IPosRequest pPosRequest, Nullable<int> pIdPR_Parent)
        {
            switch (pCodeReturn)
            {
                case Cst.ErrLevel.SUCCESS:
                case Cst.ErrLevel.DATAIGNORE:
                    pPosRequest.Status = ProcessStateTools.StatusEnum.SUCCESS;
                    break;
                case Cst.ErrLevel.NOTHINGTODO:
                    pPosRequest.Status = ProcessStateTools.StatusEnum.NONE;
                    break;
                case Cst.ErrLevel.FAILUREWARNING:
                    pPosRequest.Status = ProcessStateTools.StatusEnum.WARNING;
                    break;
                case Cst.ErrLevel.IRQ_EXECUTED:
                    pPosRequest.Status = ProcessStateTools.StatusEnum.IRQ;
                    break;
                default:
                    pPosRequest.Status = ProcessStateTools.StatusEnum.ERROR;
                    break;
            }
            pPosRequest.StatusSpecified = true;
            
            //PosKeepingTools.UpdatePosRequest(CS, pDbTransaction, pPosRequest.idPR, pPosRequest, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, pIdPR_Parent);
            PosKeepingTools.UpdatePosRequest(CS, pDbTransaction, pPosRequest.IdPR, pPosRequest, m_PKGenProcess.Session.IdA, IdProcess, pIdPR_Parent);
        }
        #endregion UpdatePosRequest
        #region UpdatePosRequestGroupLevel
        /// <summary>
        /// Mise à jour du status d'un POSREQUEST en fonction de ses POSREQUEST enfants
        /// </summary>
        /// <param name="pPosRequest">Demande POSREQUEST</param>
        protected void UpdatePosRequestGroupLevel(IPosRequest pPosRequest)
        {
            UpdatePosRequestGroupLevel(pPosRequest, ProcessStateTools.StatusNoneEnum);
        }
        /// <summary>
        /// Mise à jour du status d'un POSREQUEST en fonction de ses POSREQUEST enfants
        /// </summary>
        /// <param name="pPosRequest">Demande POSREQUEST</param>
        /// <param name="pDefaultStatus">Statut par défaut</param>
        // EG 20180307 [23769] Gestion List<IPosRequest>
        protected void UpdatePosRequestGroupLevel(IPosRequest pPosRequest, ProcessStateTools.StatusEnum pDefaultStatus)
        {
            UpdatePosRequestGroupLevel(pPosRequest, m_LstSubPosRequest, pDefaultStatus);
        }
        /// <summary>
        /// Mise à jour du status d'un POSREQUEST en fonction de sous POSREQUEST 
        /// </summary>
        /// <param name="pPosRequest">Demande POSREQUEST</param>
        /// <param name="pLstSubPosRequest">liste des sous POSREQUEST</param>
        /// <param name="pDefaultStatus">Statut par défaut</param>
        // EG 20180307 [23769] Gestion List<IPosRequest>
        protected void UpdatePosRequestGroupLevel(IPosRequest pPosRequest, List<IPosRequest> pLstSubPosRequest, ProcessStateTools.StatusEnum pDefaultStatus)
        {
            
            //PosKeepingTools.UpdatePosRequestGroupLevel(CS, pPosRequest, pLstSubPosRequest, pDefaultStatus, m_PKGenProcess.appInstance, LogHeader.IdProcess);
            PosKeepingTools.UpdatePosRequestGroupLevel(CS, pPosRequest, pLstSubPosRequest, pDefaultStatus, m_PKGenProcess.Session, IdProcess);
        }
        // EG 20180525 [23979] IRQ Processing
        protected void UpdateIRQPosRequestGroupLevel(IPosRequest pPosRequest)
        {
            
            //PosKeepingTools.UpdateIRQPosRequestGroupLevel(CS, pPosRequest, m_PKGenProcess.appInstance, LogHeader.IdProcess);
            PosKeepingTools.UpdateIRQPosRequestGroupLevel(CS, pPosRequest, m_PKGenProcess.Session, IdProcess);
        }
        #endregion UpdatePosRequestGroupLevel

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // PRIVATE METHODS
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region AccruedInterestProrataCalculation
        protected virtual IMoney AccruedInterestProrataCalculation(DataDocumentContainer pDataDocument, decimal pProrata)
        {
            return null;
        }
        #endregion AccruedInterestProrataCalculation

        #region AddCBOMARKET
        /// <summary>
        /// AutoAlimentation de la table CBOMARKET (Table qui énumère les Marchés en activité pour le couple (IDA_CBO, IDB_CBO)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pIdAEntity"></param>
        // FI 20161027 [22151] Add Method
        // FI 20170627 [XXXXX] Modify
        // PL 20181203 [XXXXX] Catch Duplicate Key 
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected static void AddCBOMARKET(string pCS, Int32 pIdAEntity, DateTime pDtBusiness, AppSession session)
        {
            // FI 20170627 [XXXXX] Mise en place d'une transaction pour éviter les collisions si plusieurs chgt de journée en simultané
            IDbTransaction dbTransaction = null; 
            try
            {
                dbTransaction = DataHelper.BeginTran(pCS);

                DateTime dtSys = OTCmlHelper.GetDateSysUTC(pCS);
                // RD 20171024 [23511] Use CBOMARKET.IDA_CSSCUSTODIAN
                string sql = @"insert into dbo.CBOMARKET (IDA_CBO, IDB_CBO, IDM, IDA_CSSCUSTODIAN, DTBUSINESS_OPENING, EXTLLINK, DTENABLED, IDAINS, DTINS)
                select  tr.IDA_RISK, tr.IDB_RISK, tr2.IDM, tr2.IDA_CSSCUSTODIAN, @DTBUSINESS, 'generated by Spheres', @DTBUSINESS, @IDAINS, @DT
                from dbo.TRADE tr
                inner join dbo.INSTRUMENT i on (i.IDI = tr.IDI) 
                inner join dbo.PRODUCT p on (p.IDP = i.IDP) and (p.IDENTIFIER = 'cashBalance')
                inner join dbo.TRADELINK tl on (tl.IDT_A = tr.IDT) and (tl.LINK ='ExchangeTradedDerivativeInCashBalance')
                inner join dbo.TRADE tr2 on (tr2.IDT = tl.IDT_B) 
                where (tr.DTBUSINESS = @DTBUSINESS) and (tr.IDA_ENTITY = @IDA_ENTITY) and (tr2.IDM is not null)
                  and not exists (select 1 from dbo.CBOMARKET where (IDA_CBO = tr.IDA_RISK) and (IDB_CBO = tr.IDB_RISK) and (IDM = tr2.IDM) and (IDA_CSSCUSTODIAN = isnull(tr2.IDA_CSSCUSTODIAN, IDA_CSSCUSTODIAN)))  
                group by tr.IDA_RISK, tr.IDB_RISK, tr2.IDM, tr2.IDA_CSSCUSTODIAN, tr.DTBUSINESS";

                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), pIdAEntity);
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness);
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), session.IdA);
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), dtSys);

                QueryParameters qryParameters = new QueryParameters(pCS, sql, dp);
                DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                DataHelper.CommitTran(dbTransaction);
            }
            catch (Exception ex)
            {
                if (null != dbTransaction)
                {
                    DataHelper.RollbackTran(dbTransaction);
                    //FI 20190621 [XXXXX] Add 
                    dbTransaction.Dispose();
                }
                // PL 20181203 If Duplicate Key => On ignore l'erreur, le RECORD vient priori d'être inséré par un autre chgt de journée en simultané.
                if (!DataHelper.IsDuplicateKeyError(pCS, ex))
                    throw;
            }
        }
        // EG 20190613 [24683] New
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected  Nullable<SQLErrorEnum> AddCBOMARKET2(string pCS, Int32 pIdAEntity, DateTime pDtBusiness, AppSession session)
        {
            Nullable<SQLErrorEnum> ret = null;

            // FI 20170627 [XXXXX] Mise en place d'une transaction pour éviter les collisions si plusieurs chgt de journée en simultané
            using (IDbTransaction dbTransaction = DataHelper.BeginTran(pCS))
            {
                try
                {
                    DateTime dtSys = OTCmlHelper.GetDateSysUTC(pCS);
                    // RD 20171024 [23511] Use CBOMARKET.IDA_CSSCUSTODIAN
                    string sql = @"insert into dbo.CBOMARKET (IDA_CBO, IDB_CBO, IDM, IDA_CSSCUSTODIAN, DTBUSINESS_OPENING, EXTLLINK, DTENABLED, IDAINS, DTINS)
                select  tr.IDA_RISK, tr.IDB_RISK, tr2.IDM, tr2.IDA_CSSCUSTODIAN, @DTBUSINESS, 'generated by Spheres', @DTBUSINESS, @IDAINS, @DT
                from dbo.TRADE tr
                inner join dbo.INSTRUMENT i on (i.IDI = tr.IDI) 
                inner join dbo.PRODUCT p on (p.IDP = i.IDP) and (p.IDENTIFIER = 'cashBalance')
                inner join dbo.TRADELINK tl on (tl.IDT_A = tr.IDT) and (tl.LINK ='ExchangeTradedDerivativeInCashBalance')
                inner join dbo.TRADE tr2 on (tr2.IDT = tl.IDT_B) 
                where (tr.DTBUSINESS = @DTBUSINESS) and (tr.IDA_ENTITY = @IDA_ENTITY) and (tr2.IDM is not null) and
                not exists( select 1 from dbo.CBOMARKET 
		                    where (IDA_CBO = tr.IDA_RISK) and (IDB_CBO = tr.IDB_RISK) and (IDM = tr2.IDM) and (IDA_CSSCUSTODIAN = isnull(tr2.IDA_CSSCUSTODIAN, IDA_CSSCUSTODIAN)))  
                group by  tr.IDA_RISK, tr.IDB_RISK, tr2.IDM, tr2.IDA_CSSCUSTODIAN, tr.DTBUSINESS";

                    DataParameters dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), pIdAEntity);
                    dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness);
                    dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), session.IdA);
                    dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), dtSys);


                    QueryParameters qryParameters = new QueryParameters(pCS, sql, dp);
                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                    DataHelper.CommitTran(dbTransaction);
                }
                catch (Exception ex)
                {
                    if (null != dbTransaction)
                        DataHelper.RollbackTran(dbTransaction);

                    // PL 20181203 If Duplicate Key => On ignore l'erreur, le RECORD vient priori d'être inséré par un autre chgt de journée en simultané.
                    // EG 20190613 If Timeout|Deadlock => On réexecute avec un garde-fou.
                    ret = DataHelper.AnalyseSQLException(pCS, ex);
                    switch (ret)
                    {
                        case SQLErrorEnum.DeadLock:
                        case SQLErrorEnum.Timeout:
                            break;
                        case SQLErrorEnum.DuplicateKey:
                            ret = SQLErrorEnum.DuplicateKey;
                            break;
                        default:
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, StrFunc.AppendFormat("AddCBOMARKET : Unexpected Error occured : {0}", ex.Message)));
                            throw;
                    }
                }
            }
            return ret;
        }
        #endregion AddCBOMARKET

        #region AddEventPosActionDet
        private void AddEventPosActionDet(DataTable pDtEventPosActionDet, int pNewIdE, int pNewIdPADET)
        {
            DataRow rowEventPosActionDet = pDtEventPosActionDet.NewRow();
            rowEventPosActionDet.BeginEdit();
            rowEventPosActionDet["IDE"] = pNewIdE;
            rowEventPosActionDet["IDPADET"] = pNewIdPADET;
            rowEventPosActionDet.EndEdit();
            pDtEventPosActionDet.Rows.Add(rowEventPosActionDet);
        }
        #endregion AddEventPosActionDet
        #region AddNewPosActionDet
        // EG 20141205 New Parameter pPositionEffectEnum
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private decimal AddNewPosActionDet(DataRow pRowTradeClosable, DataRow pRowTradeClosing, decimal pQty, string pPositionEffect)
        {
            int idT_Closing = Convert.ToInt32(pRowTradeClosing["IDT"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal closingQty = Convert.ToDecimal(pRowTradeClosing["QTY"]);
            string sideClosing = pRowTradeClosing["SIDE"].ToString();

            int idT_Closable = Convert.ToInt32(pRowTradeClosable["IDT"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal closableQty = Convert.ToDecimal(pRowTradeClosable["QTY"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal qty = Math.Min(Math.Min(closingQty, closableQty), pQty);

            #region Insertion de la clôture dans la dataTable m_DtPosActionDet_Working
            DataRow newRow = m_DtPosActionDet_Working.NewRow();
            newRow.BeginEdit();
            newRow["IDPA"] = 0;
            newRow["IDT_CLOSING"] = idT_Closing;
            newRow["SIDE_CLOSING"] = sideClosing;
            if (IsTradeBuyer(sideClosing))
            {
                newRow["IDT_BUY"] = idT_Closing;
                newRow["IDT_SELL"] = idT_Closable;
            }
            else
            {
                newRow["IDT_SELL"] = idT_Closing;
                newRow["IDT_BUY"] = idT_Closable;
            }
            newRow["QTY"] = qty;
            newRow["ORIGINALQTY_CLOSED"] = closableQty;
            newRow["ORIGINALQTY_CLOSING"] = closingQty;
            // EG 20141205 New
            newRow["POSITIONEFFECT"] = StrFunc.IsFilled(pPositionEffect) ? pPositionEffect : Convert.DBNull;
            newRow.EndEdit();
            m_DtPosActionDet_Working.Rows.Add(newRow);
            #endregion

            // Mise à jour dtPosition 
            pRowTradeClosable.BeginEdit();
            pRowTradeClosable["QTY"] = Math.Max(0, closableQty - qty);
            pRowTradeClosable.EndEdit();

            pRowTradeClosing.BeginEdit();
            pRowTradeClosing["QTY"] = Math.Max(0, closingQty - qty);
            pRowTradeClosing.EndEdit();


            return qty;
        }
        #endregion AddNewPosActionDet
        #region AddParameterDealer
        protected void AddParameterDealer(DataParameters pParameters)
        {
            bool isDefaultBehavior = true;

            if (null != m_PosRequest)
            {
                switch (m_PosRequest.RequestType)
                {
                    case Cst.PosRequestTypeEnum.OptionAssignment:
                        if (PosKeepingData.IdA_Dealer > 0)
                            pParameters.Add(new DataParameter(CS, "IDA_DEALER", DbType.Int32), PosKeepingData.IdA_Dealer);
                        if (PosKeepingData.IdB_Dealer > 0)
                            pParameters.Add(new DataParameter(CS, "IDB_DEALER", DbType.Int32), PosKeepingData.IdB_Dealer);
                        if (PosKeepingData.IdA_EntityDealer > 0)
                            pParameters.Add(new DataParameter(CS, "IDA_ENTITYDEALER", DbType.Int32), PosKeepingData.IdA_EntityDealer);
                        isDefaultBehavior = false;
                        break;
                    default:
                        isDefaultBehavior = true;
                        break;
                }
            }

            if (isDefaultBehavior)
            {
                pParameters.Add(new DataParameter(CS, "IDA_DEALER", DbType.Int32), PosKeepingData.IdA_Dealer);
                pParameters.Add(new DataParameter(CS, "IDB_DEALER", DbType.Int32), PosKeepingData.IdB_Dealer);
            }
        }
        #endregion AddParameterDealer
        #region AddTradesWithMissingFees
        /// <summary>
        /// Sélection de tous les trades du JOUR sans frais et pour lesquels un contrôle d'absence de frais est spécifié 
        /// sur le book DEALER (WARNING/BLOQUANT)
        /// </summary>
        /// <returns></returns>
        // EG 20140225 [19575][19666]
        // EG 20210916 [XXXXX] Implémentation TryMultiple
        protected virtual Cst.ErrLevel AddTradesWithMissingFees()
        {
            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.EOD_FeesGroupLevel,
            //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM);
            //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.EOD_FeesGroupLevel,
            //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM);
            //if (null != ds)
            //{
            //    
            //    //AddTradeForFeesCalculation("LOG-05020", ds.Tables[0].Rows);
            //    AddTradeForFeesCalculation(new SysMsgCode(SysCodeEnum.LOG, 5020), ds.Tables[0].Rows);
            //}

            TryMultiple tryMultiple = new TryMultiple(CS, "GetDataRequestWithIsolationLevel", "GetDataRequestWithIsolationLevel")
            {
                SetErrorWarning = m_PKGenProcess.ProcessState.SetErrorWarning,
                IsModeTransactional = false,
                ThreadSleep = 5 //blocage de 5 secondes entre chaque tentative
            };

            DataSet ds = tryMultiple.Exec<string, IsolationLevel, Cst.PosRequestTypeEnum, DateTime, Int32, DataSet >(
                delegate (String arg1, IsolationLevel arg2, Cst.PosRequestTypeEnum arg3, DateTime arg4, Int32 arg5) 
                { return GetDataRequestWithIsolationLevel(arg1, arg2, arg3, arg4, arg5); },
                CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.EOD_FeesGroupLevel, m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM);

            if (null != ds)
                AddTradeForFeesCalculation(new SysMsgCode(SysCodeEnum.LOG, 5020), ds.Tables[0].Rows);

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion AddTradesWithMissingFees
        #region AddTradesFromStg
        /// <summary>
        /// Sélection de tous les trades du JOUR en provenance d'une STRATEGIE pour calcul des FRAIS
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel AddTradesFromStg()
        {
            StrategyMarker strategyMarker = new StrategyMarker(CS, m_MarketPosRequest,  m_PKGenProcess.ProcessState.SetErrorWarning)
            {
                StrategySerializationWithCarriageReturn = Settings.Default.StrategySerializationWithCarriageReturn
            };

            if (strategyMarker.Init())
            {
                strategyMarker.ComposeStrategies();
                strategyMarker.StrategyIdentification();
                strategyMarker.SetAllocsToUpdate();
                strategyMarker.UpdateAllocs();
                IEnumerable<TradeFeesCalculation> tradesStg = strategyMarker.GetTradeFeesCalculationObjects();
                if (null != tradesStg)
                {
                    IEnumerable<TradeFeesCalculation> trades =
                        from trade in tradesStg
                        where (false == m_TradeCandidatesForFees.ContainsKey(trade.idT))
                        select trade;

                    if (0 < trades.Count())
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5027), 0,
                            new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                            new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                            new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                            new LogParam(m_MarketPosRequest.GroupProductValue),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                            new LogParam(trades.Count())));
                        Logger.Write();

                        foreach (TradeFeesCalculation trade in trades)
                        {
                            m_TradeCandidatesForFees.Add(trade.idT, trade);
                        }
                    }
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion AddTradesFromStg
        #region AssociatedTradeCandidates
        /// <summary>
        /// Construction des UPLETS de trade candidats par contexte de MERGE
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel AssociatedTradeCandidates()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8951), 1,
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                new LogParam(m_MarketPosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness))));

            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;
            // on ne travaille que sur les contextes de merge avec plus d'un trade candidat
            bool isExistCandidates = m_LstTradeMergeRule.Exists(tmr => (null != tmr.IdT_Candidate) && (1 < tmr.IdT_Candidate.Count));
            if (isExistCandidates)
            {
                // Selection de tous les contextes qui ont des trades CANDIDATS à MERGING ( > 1 nécessairement)
                List<TradeMergeRule> _lstTmr = m_LstTradeMergeRule.FindAll(tmr => ((null != tmr.IdT_Candidate) && (1 < tmr.IdT_Candidate.Count)));
                // Regroupement des trades à MERGER
                _lstTmr.ForEach(tmr =>
                {
                    // Filtrage des trades sur la base de ceux candidats dans le contexte courant
                    List<TradeKey> _lstTradeKey = (
                    from idT in tmr.IdT_Candidate
                    join tradeCandidate in m_DicTradeCandidate on idT equals tradeCandidate.Key
                    select new TradeKey(tradeCandidate.Value)).ToList();

                    //// Agrégation sur les valeurs identiques
                    List<IGrouping<TradeKey, TradeKey>> _lstTradeKeyGroup =
                        _lstTradeKey.GroupBy(group => group, new TradeKeyComparer()).ToList();

                    // Construction des assemblages de trades à merger
                    tmr.SetTradeMergable(m_DicTradeCandidate, _lstTradeKeyGroup);
                });

                // Exclusion des trades orphelin dans la cas d'un contexte PRIX = IDENTICAL
                codeReturn = ExcludeTradeOrphanPriceIdentical();
            }
            return codeReturn;
        }
        #endregion AssociatedTradeCandidates

        #region CacheAssetAdd
        /// <summary>
        ///  Ajoute l'asset dans le cache
        /// </summary>
        /// <param name="pIdAssetETD"></param>
        /// <param name="pPosRequestAssetQuote"></param>
        /// <param name="pPosKeepingAsset"></param>
        // EG 20150624 [21151] CacheAsset
        private void CacheAssetAdd(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, int pIdAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote, PosKeepingAsset pPosKeepingAsset)
        {
            if (null == m_AssetCache)
                m_AssetCache = new Dictionary<CacheAsset, PosKeepingAsset>(new CacheAssetComparer());
            CacheAsset _key = new CacheAsset(pUnderlyingAsset, pIdAsset, pPosRequestAssetQuote);
            m_AssetCache.Add(_key, pPosKeepingAsset);
        }
        #endregion CacheAssetAdd
        #region CacheAssetFind
        /// <summary>
        ///  Recherche l'asset dans le cache
        /// </summary>
        /// <param name="pIdAssetETD"></param>
        /// <param name="pPosRequestAssetQuote"></param>
        /// <returns></returns>
        // EG 20150624 [21151] CacheAsset
        private PosKeepingAsset CacheAssetFind(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, int pIdAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            PosKeepingAsset _posKeepingAsset = null;
            if (null != m_AssetCache)
            {
                CacheAsset _key = new CacheAsset(pUnderlyingAsset, pIdAsset, pPosRequestAssetQuote);
                if (m_AssetCache.ContainsKey(_key))
                    _posKeepingAsset = m_AssetCache[_key];
            }
            return _posKeepingAsset;
        }
        #endregion CacheAssetFind

        #region ClearingBLKCalc
        /// <summary>
        /// Calcul de la compensation manuelle de type BULK ou compensation automatique (traitement fin de journée)
        /// </summary>
        /// <returns></returns>
        //PL 20130306 Nouvelle méthode considérant pour le FIFO, le plus vieux trades comme étant les trades clôturés et non pas les trades clôturants.
        private Cst.ErrLevel ClearingBLKCalc()
        {
            string positionEffectClosing = this.PosKeepingData.PositionEffect_BookDealer;
            bool isLIFO = ExchangeTradedDerivativeTools.IsPositionEffect_LIFO(positionEffectClosing);

            // Recherche de la règle de tri applicable aux négociations à compenser et compensantes
            // EG 20141205 New
            string positionEffect = isLIFO ? ExchangeTradedDerivativeTools.GetPositionEffect_LIFO() : ExchangeTradedDerivativeTools.GetPositionEffect_FIFO();
            string rules = GetOffSettingRulesForClosable(positionEffect, out _);
            string filter_A = "QTY<>0";
            string filter_B;
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal qty_A;
            string side_A;
            DataRow[] rowsTrade_B;

            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal totalQtyToClose = m_PosRequest.Qty; //Qté à compenser demandée

            System.Diagnostics.Debug.WriteLine("TotalQtyToClose " + totalQtyToClose.ToString(), "INFO");

            DataRow[] rowsTrade_A = m_DtPosition.Select(filter_A, rules, DataViewRowState.CurrentRows);
            if (ArrFunc.IsFilled(rowsTrade_A))
            {
                #region Lecture et traitement des lignes à compenser
                foreach (DataRow rowTrade_A in rowsTrade_A)
                {
                    side_A = rowTrade_A["SIDE"].ToString();
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    qty_A = Convert.ToDecimal(rowTrade_A["QTY"]);
                    decimal remainderQtyToClose = Math.Min(totalQtyToClose, qty_A);
                    if (qty_A == 0)
                    {
                        //NB: On se trouve dans ce cas lorsqu'un trade a été totalement clôturé lors d'une itération précédente.
                        continue;
                    }

                    System.Diagnostics.Debug.WriteLine("Start ".PadRight(80, '-'), "INFO");
                    System.Diagnostics.Debug.WriteLine("CLOSING " + rowTrade_A["IDENTIFIER"].ToString() + " (Id:" + rowTrade_A["IDT"].ToString() + ") "
                    + (SideTools.IsFIXmlBuyer(side_A) ? "Buy" : "Sell") + " " + qty_A.ToString() + " PosEfct:" + rowTrade_A["POSITIONEFFECT"].ToString() + "/" + positionEffectClosing, "INFO");

                    filter_B = "QTY<>0 and SIDE<>'" + side_A + "'";
                    rowsTrade_B = m_DtPosition.Select(filter_B, rules, DataViewRowState.CurrentRows);
                    if (ArrFunc.IsFilled(rowsTrade_B))
                    {
                        // Boucle sur négociations compensantes
                        foreach (DataRow rowTrade_B in rowsTrade_B)
                        {
                            System.Diagnostics.Debug.WriteLine("CLOSABLE " + rowTrade_B["IDENTIFIER"].ToString() + " (Id:" + rowTrade_B["IDT"].ToString() + ") "
                            + (SideTools.IsFIXmlBuyer(rowTrade_B["SIDE"].ToString()) ? "Buy" : "Sell") + " " + rowTrade_B["QTY"].ToString(), "INFO");
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            decimal qty = 0;
                            if (isLIFO)
                            {
                                // EG 20141205 New PositionEffect parameter
                                qty = AddNewPosActionDet(rowTrade_B, rowTrade_A, remainderQtyToClose, positionEffect);
                            }
                            else
                            {
                                // EG 20141205 New PositionEffect parameter
                                qty = AddNewPosActionDet(rowTrade_A, rowTrade_B, remainderQtyToClose, positionEffect);
                            }
                            remainderQtyToClose -= qty;
                            totalQtyToClose -= qty;

                            System.Diagnostics.Debug.WriteLine("OFFSETTING " + qty.ToString() + " (Remaining " + totalQtyToClose.ToString() + ")", "INFO");

                            if (remainderQtyToClose == 0)
                            {
                                // Quantité du trade totalement compensée --> on sort de la boucle
                                System.Diagnostics.Debug.WriteLine("OFFSETTING Complete! ".PadRight(80, '*'), "INFO");
                                break;
                            }
                        }

                        if (totalQtyToClose == 0)
                        {
                            // Quantité demandée totalement compensée --> on sort de la boucle
                            System.Diagnostics.Debug.WriteLine("End ".PadRight(80, '-'), "INFO");
                            break;
                        }
                        else if (remainderQtyToClose > 0)
                        {
                            //Warning: Si la Qté est ici supérieure à zéro, cela indique que le trade n'a pu clôturer la qté totale.
                            //         Par conséquent, il devient inutile de tenter de clôturer les autres trades CLOSE, et ce qu'ils soient ACHAT ou VENTE.
                            System.Diagnostics.Debug.WriteLine("OFFSETTING Incomplete! ".PadRight(80, '*'), "INFO");
                            System.Diagnostics.Debug.WriteLine("End ".PadRight(80, '-'), "INFO");
                            break;
                        }
                    }
                    else
                    {
                        //Warning: Il n'existe pas de Trade/Position en sens inverse.
                        System.Diagnostics.Debug.WriteLine("OFFSETTING Impossible! ".PadRight(80, '*'), "INFO");
                    }
                    System.Diagnostics.Debug.WriteLine("End ".PadRight(80, '-'), "INFO");
                }
                #endregion Lecture et traitement des lignes compensantes
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No trade", "INFO");
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion ClearingBLKCalc
        #region ClearingBLKCalc_HILO
        /// <summary>
        /// Compensation HILO
        /// <para>Achat de prix le plus faible face à Vente de prix le plus fort</para>
        /// </summary>
        /// <param name="pDtBusiness">Limitation aux seuls trades Close/HILO d'une journée (Utilisé dans le cadre de clôture HILO en mode STP)</param>
        /// <returns></returns>
        /// EG 20171016 [23509] Upd DTEXECUTION replace DTTIMESTAMP
        // EG 20171123 BUG UPDENTRY 
        private Cst.ErrLevel ClearingBLKCalc_HILO(DateTime pDtBusiness)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal totalQtyToClose = 0;                //Qté à compenser demandée

            bool isBusinessDay = (DtFunc.IsDateTimeFilled(pDtBusiness));
            if (!isBusinessDay)
            {
                totalQtyToClose = m_PosRequest.Qty;
            }
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal qtyClosing;
            long idtClosing;
            string sideClosing, positionEffectClosing;
            // EG 20171123 BUG UPDENTRY
            //DateTimeOffset dtTimestampClosing, dtBusinessClosing, dtExecutionClosing;
            DateTime dtExecutionClosing;
            DataRow[] rowsTradeClosable;

            string addFilter = " and QTY<>0";
            if (isBusinessDay)
            {
                //On limite aux seuls trades du jour (Clôture HILO en mode STP)
                addFilter += " and DTBUSINESS=#" + DtFunc.DateTimeToStringDateISO(pDtBusiness) + "#";

                if (!PosKeepingData.IsIgnorePositionEffect_BookDealer)
                {
                    //On limite aux seuls trades saisis en "Close"
                    addFilter += " and POSITIONEFFECT='" + ExchangeTradedDerivativeTools.GetPositionEffect_Close() + "'";
                }
            }

            string IDENTIFIER_ClosingDay_SellHigh, IDENTIFIER_ClosingDay_BuyLow;
            decimal PRICE_ClosingDay_SellHigh = 0, PRICE_ClosingDay_BuyLow = 0;
            // EG 20171016 [23509] Upd
            DateTime TIMESTAMP_ClosingDay_SellHigh = DateTime.MinValue, TIMESTAMP_ClosingDay_BuyLow = DateTime.MinValue;
            int guard = 0, maxGuard = m_DtPosition.Rows.Count * 2;

            DataRow rowTradeClosing;
            do
            {
                guard++;
                if (guard > maxGuard)
                {
                    throw new NotImplementedException(StrFunc.AppendFormat("Infinite loop detected on ClearingBLKCalc_HILO() method. Number of items: {0}. Please contact EFS",
                        guard.ToString()));
                }

                #region Recherche Achat au prix Mini ou Vente au prix Maxi en CLOSE
                System.Diagnostics.Debug.WriteLine("Start ".PadRight(80, '-'), "INFO");
                System.Diagnostics.Debug.WriteLine("- CLOSE Candidates  ".PadRight(80, '.'), "INFO");
                int IDT_ClosingDay_SellHigh = 0;
                int IDT_ClosingDay_BuyLow = 0;
                decimal PRICE_SellHigh = 0;
                decimal PRICE_BuyLow = 0;

                DataRow[] rowsTradeClosingDay_BuyLow = m_DtPosition.Select("SIDE='1'" + addFilter, "PRICE ASC, IDT ASC", DataViewRowState.CurrentRows);
                if (ArrFunc.IsFilled(rowsTradeClosingDay_BuyLow))
                {
                    IDT_ClosingDay_BuyLow = Convert.ToInt32(rowsTradeClosingDay_BuyLow[0]["IDT"]);
                    IDENTIFIER_ClosingDay_BuyLow = rowsTradeClosingDay_BuyLow[0]["IDENTIFIER"].ToString();
                    PRICE_ClosingDay_BuyLow = Convert.ToDecimal(rowsTradeClosingDay_BuyLow[0]["PRICE"]);
                    // EG 20171016 [23509] Upd
                    TIMESTAMP_ClosingDay_BuyLow = Convert.ToDateTime(rowsTradeClosingDay_BuyLow[0]["DTTIMESTAMP"]);
                    System.Diagnostics.Debug.WriteLine("  BuyLow  : " + IDENTIFIER_ClosingDay_BuyLow + " (Id:"
                        + IDT_ClosingDay_BuyLow + ") Price: " + PRICE_ClosingDay_BuyLow.ToString(), "INFO");
                }
                DataRow[] rowsTradeClosingDay_SellHigh = m_DtPosition.Select("SIDE='2'" + addFilter, "PRICE DESC, IDT ASC", DataViewRowState.CurrentRows);
                if (ArrFunc.IsFilled(rowsTradeClosingDay_SellHigh))
                {
                    IDT_ClosingDay_SellHigh = Convert.ToInt32(rowsTradeClosingDay_SellHigh[0]["IDT"]);
                    IDENTIFIER_ClosingDay_SellHigh = rowsTradeClosingDay_SellHigh[0]["IDENTIFIER"].ToString();
                    PRICE_ClosingDay_SellHigh = Convert.ToDecimal(rowsTradeClosingDay_SellHigh[0]["PRICE"]);
                    // EG 20171016 [23509] Upd
                    TIMESTAMP_ClosingDay_SellHigh = Convert.ToDateTime(rowsTradeClosingDay_SellHigh[0]["DTTIMESTAMP"]);
                    System.Diagnostics.Debug.WriteLine("  SellHigh: " + IDENTIFIER_ClosingDay_SellHigh + " (Id:"
                        + IDT_ClosingDay_SellHigh + ") Price: " + PRICE_ClosingDay_SellHigh.ToString(), "INFO");
                }
                if ((IDT_ClosingDay_SellHigh > 0) && (IDT_ClosingDay_BuyLow > 0))
                {
                    //Il existe au moins un trade ACHAT et un trade VENTE en CLOSE --> On recherche ce qu'il y a en Position et Jour,  
                    //afin de s'assurer de traiter prioritairement l'ACHAT ou VENTE en CLOSE le plus "judicieux" !
                    #region Exemple
                    /*
                    En Position : Achat de 10 à 100
                    En Jour     : Achat de 10 à 101 en CLOSE/HILO
                                  Vente de 10 à 102 en CLOSE/HILO 
                    Si on traite la clôture de l'Achat de 10 à 101, celui-ci va clôturer la Vente de 10 à 102 
                    Il faut traiter prioritairement la clôture de la Vente de 10 à 102 ce qui va clôturer l'Achat de 10 à 100 en Position.
                    Pour cela, on identifie s'il existe en position veille un Achat de cours inférieur à l'Achat jour, si oui on prévilégie la Vente pour le process. 
                    */
                    #endregion
                    System.Diagnostics.Debug.WriteLine("- OPEN Available ".PadRight(80, '.'), "INFO");

                    DataRow[] rowsTrade_BuyLow = m_DtPosition.Select("QTY<>0 and SIDE='1' and IDT<>" + IDT_ClosingDay_BuyLow.ToString(), "PRICE ASC", DataViewRowState.CurrentRows);
                    if (ArrFunc.IsFilled(rowsTrade_BuyLow))
                    {
                        PRICE_BuyLow = Convert.ToDecimal(rowsTrade_BuyLow[0]["PRICE"]);
                        System.Diagnostics.Debug.WriteLine("  BuyLow  : " + rowsTrade_BuyLow[0]["IDENTIFIER"].ToString()
                            + " (Id:" + rowsTrade_BuyLow[0]["IDT"].ToString() + ") Price: " + PRICE_BuyLow.ToString(), "INFO");
                    }
                    DataRow[] rowsTrade_SellHigh = m_DtPosition.Select("QTY<>0 and SIDE='2' and IDT<>" + IDT_ClosingDay_SellHigh.ToString(), "PRICE DESC", DataViewRowState.CurrentRows);
                    if (ArrFunc.IsFilled(rowsTrade_SellHigh))
                    {
                        PRICE_SellHigh = Convert.ToDecimal(rowsTrade_SellHigh[0]["PRICE"]);
                        System.Diagnostics.Debug.WriteLine("  SellHigh: " + rowsTrade_SellHigh[0]["IDENTIFIER"].ToString()
                            + " (Id:" + rowsTrade_SellHigh[0]["IDT"].ToString() + ") Price: " + PRICE_SellHigh.ToString(), "INFO");
                    }
                    if (ArrFunc.IsEmpty(rowsTrade_BuyLow) && ArrFunc.IsEmpty(rowsTrade_SellHigh))
                    {
                        System.Diagnostics.Debug.WriteLine("  None", "INFO");
                    }

                    //if ((PRICE_ClosingDay_BuyLow <= PRICE_BuyLow) && (PRICE_SellHigh >= PRICE_ClosingDay_SellHigh)) 
                    //20131121
                    // FI/PL 20131206 [19143]  <= et >= remplacé par < et >
                    //if (((PRICE_BuyLow == 0) || !(PRICE_BuyLow <= PRICE_ClosingDay_BuyLow)) && (PRICE_SellHigh >= PRICE_ClosingDay_SellHigh))
                    if (((PRICE_BuyLow == 0) || !(PRICE_BuyLow < PRICE_ClosingDay_BuyLow)) && (PRICE_SellHigh > PRICE_ClosingDay_SellHigh))
                    {
                        //Il n'existe pas de trade ACHAT, en PO ou Jour Open, avec un prix inférieur au Trade ACHAT CLOSE 
                        //et
                        //il existe un trade VENTE, en PO ou Jour Open, avec un prix supérieur au Trade VENTE CLOSE 
                        //--> On traite donc prioritairement la demande de clôture du trade ACHAT CLOSE afin qu'il clôture le trade VENTE en PO de prix supérieur 
                        //    afin d'obtenir le plus grand écart prix ACHAT / prix VENTE
                        rowTradeClosing = rowsTradeClosingDay_BuyLow[0];
                    }
                    // FI/PL 20131206 [19143]  <= et >= remplacé par < et >
                    //else if ((!(PRICE_SellHigh >= PRICE_ClosingDay_SellHigh)) && (PRICE_BuyLow <= PRICE_ClosingDay_BuyLow) && (PRICE_BuyLow != 0))
                    else if ((!(PRICE_SellHigh > PRICE_ClosingDay_SellHigh)) && (PRICE_BuyLow < PRICE_ClosingDay_BuyLow) && (PRICE_BuyLow != 0))
                    {
                        //Il n'existe pas de trade VENTE, en PO ou Jour Open, avec un prix supérieur au Trade VENTE CLOSE 
                        //et
                        //il existe un trade ACHAT, en PO ou Jour Open, avec un prix inférieur au Trade ACHAT CLOSE 
                        //--> On traite prioritairement donc la demande de clôture du trade VENTE CLOSE afin qu'il clôture le trade ACAHT en PO de prix inférieur 
                        //    afin d'obtenir le plus grand écart prix ACHAT / prix VENTE
                        rowTradeClosing = rowsTradeClosingDay_SellHigh[0];
                    }
                    else
                    {
                        //On traite prioritairement la demande de clôture du trade ACHAT ou VENTE CLOSE le plus récent
                        if (TIMESTAMP_ClosingDay_BuyLow != TIMESTAMP_ClosingDay_SellHigh)
                        {
                            rowTradeClosing = (TIMESTAMP_ClosingDay_BuyLow > TIMESTAMP_ClosingDay_SellHigh) ? rowsTradeClosingDay_BuyLow[0] : rowsTradeClosingDay_SellHigh[0];
                        }
                        else
                        {
                            rowTradeClosing = (IDT_ClosingDay_BuyLow > IDT_ClosingDay_SellHigh) ? rowsTradeClosingDay_BuyLow[0] : rowsTradeClosingDay_SellHigh[0];
                        }
                    }
                }
                else if (IDT_ClosingDay_BuyLow > 0)
                {
                    //Aucun trade VENTE en CLOSE --> On traite donc la clôture du trade ACHAT CLOSE
                    rowTradeClosing = rowsTradeClosingDay_BuyLow[0];
                }
                else if (IDT_ClosingDay_SellHigh > 0)
                {
                    //Aucun trade ACHAT en CLOSE --> On traite donc la clôture du trade VENTE CLOSE
                    rowTradeClosing = rowsTradeClosingDay_SellHigh[0];
                }
                else
                {
                    //Aucun trade ACHAT ou VENTE en CLOSE --> Exit
                    System.Diagnostics.Debug.WriteLine("No trade", "INFO");
                    rowTradeClosing = null;
                }
                #endregion

                if (rowTradeClosing != null)
                {
                    System.Diagnostics.Debug.WriteLine("- OFFSETTING Performed ".PadRight(80, '.'), "INFO");
                    idtClosing = Convert.ToInt32(rowTradeClosing["IDT"]);
                    sideClosing = rowTradeClosing["SIDE"].ToString();
                    positionEffectClosing = ExchangeTradedDerivativeTools.GetPositionEffect_HILO();
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    qtyClosing = Convert.ToDecimal(rowTradeClosing["QTY"]);
                    // EG 20171016 [23509] Upd
                    dtExecutionClosing = Convert.ToDateTime(rowTradeClosing["DTEXECUTION"]);
                    System.Diagnostics.Debug.WriteLine("  Closing : " + rowTradeClosing["IDENTIFIER"].ToString() + " (Id:" + rowTradeClosing["IDT"].ToString() + ") "
                        + (SideTools.IsFIXmlBuyer(sideClosing) ? "Buy " : "Sell") + " " + qtyClosing.ToString() + " PosEfct:"
                        + rowTradeClosing["POSITIONEFFECT"].ToString() + "/" + positionEffectClosing, "INFO");

                    decimal remainderQtyToClose;
                    if (isBusinessDay)
                    {
                        remainderQtyToClose = qtyClosing;
                    }
                    else
                    {
                        remainderQtyToClose = Math.Min(totalQtyToClose, qtyClosing);
                    }

                    string FILTERClosable = "QTY<>0 and SIDE<>'{0}'";

                    //Tip: En HILO, on suffixe pour GetOffSettingRulesForClosable() le paramètre "pPositionEffect" par le sens du trade CLOSE 
                    string rulesClosable = GetOffSettingRulesForClosable(positionEffectClosing + sideClosing, out string addFilterClosable);
                    string filterClosable = String.Format(FILTERClosable + addFilterClosable, sideClosing);
                    rowsTradeClosable = m_DtPosition.Select(filterClosable, rulesClosable, DataViewRowState.CurrentRows);
                    if (ArrFunc.IsFilled(rowsTradeClosable))
                    {
                        // Boucle sur négociations clôturables1
                        foreach (DataRow rowTradeClosable in rowsTradeClosable)
                        {
                            System.Diagnostics.Debug.WriteLine("  Closable: " + rowTradeClosable["IDENTIFIER"].ToString() + " (Id:" + rowTradeClosable["IDT"].ToString() + ") "
                                + (SideTools.IsFIXmlBuyer(rowTradeClosable["SIDE"].ToString()) ? "Buy " : "Sell") + " " + rowTradeClosable["QTY"].ToString(), "INFO");

                            int idtClosable = Convert.ToInt32(rowTradeClosable["IDT"]); ;

                            // EG 20171016 [23509] Upd
                            DateTime dtTimestampClosable = Convert.ToDateTime(rowTradeClosable["DTTIMESTAMP"]);
                            DateTime dtExecutionClosable = Convert.ToDateTime(rowTradeClosable["DTEXECUTION"]);
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            decimal qty = 0;
                            // EG 20171016 [23509] Upd
                            if ((dtExecutionClosing > dtExecutionClosable) || (dtExecutionClosing == dtExecutionClosable && idtClosing > idtClosable))
                            {
                                qty = AddNewPosActionDet(rowTradeClosable, rowTradeClosing, remainderQtyToClose, positionEffectClosing);
                                System.Diagnostics.Debug.WriteLine("  --> Offsetting Qty: " + qty.ToString(), "INFO");
                            }
                            else
                            {
                                #region WARNING: PL 20131121
                                /*
                                Ici afin de considérer le trade le plus récent comme CLOSING, on inverse les 2 datarow dans l'appel à AddNewPosActionDet() 
                                On ne peut pas procéder différemment, en restreignant par exemple aux seuls trades de Timestamp inférieur à celui du trade Closing en cours
                                de traitement. Car de ce fait on ne respecterait plus le principe du HILO.
                                ex: Veille       n°11 V  8 à 1705.25
                                    Veille       n°16 V 12 à 1764.00
                                    Jour   10h00 n°74 A 15 à 1764.25
                                    Jour   12h00 n°98 V 15 à 1755.50
                                    
                                    Dans cet exemple, on retient l'Achat trade clôturant (CLOSING)
                                    il cloture classiquement la vente au plus haut prix donc la vente veille n°16 à 1764.00 pour 12 lots
                                    si on ne considérait pas la vente n°98 à 1755.50 du fait qu'elle est postérieure, l'achat cloturerait "à tort" la la vente veille n°11 à 1705.25 
                                    par contre en considérant la vente n°98 à 1755.50, pour respecter le principe de base qu'un trade ne peut cloturer que des trades antérieure à lui
                                    on inverse ici les 2 datarow afin de considérer le trade postérieure, dans notre exemple la vente n°98 comme clôturant (CLOSING).
                                */
                                #endregion
                                qty = AddNewPosActionDet(rowTradeClosing, rowTradeClosable, remainderQtyToClose, positionEffectClosing);
                                System.Diagnostics.Debug.WriteLine("  --> Offsetting Qty: " + qty.ToString() + " Warning: Closable is Closing and inversely.", "INFO");
                            }
                            remainderQtyToClose -= qty;
                            if (!isBusinessDay)
                            {
                                totalQtyToClose -= qty;
                            }

                            if (remainderQtyToClose == 0)
                            {
                                // Quantité du trade totalement clôturée --> on sort de la boucle
                                System.Diagnostics.Debug.WriteLine("  ==> FULL Offsetting. ".PadRight(80, '.'), "INFO");
                                break;
                            }
                        }

                        if (isBusinessDay)
                        {
                            if (remainderQtyToClose > 0)
                            {
                                //Warning: Si la Qté est ici supérieure à zéro, cela indique que le trade n'a pu clôturer la qté totale.
                                //         Par conséquent, il devient inutile de tenter de clôturer les autres trades CLOSE, et ce qu'ils soient ACHAT ou VENTE.
                                //         On initialise alors "rowTradeClosing" à null afin de sortir de la boucle.
                                rowTradeClosing = null;
                                System.Diagnostics.Debug.WriteLine("  ==> PARTIAL Offsetting! ".PadRight(80, '*'), "INFO");
                            }
                        }
                        else
                        {
                            if (totalQtyToClose == 0)
                            {
                                // Info: Quantité demandée totalement compensée
                                //       On initialise alors "rowTradeClosing" à null afin de sortir de la boucle.
                                rowTradeClosing = null;
                                System.Diagnostics.Debug.WriteLine("  ==> FULL Offsetting. ".PadRight(80, '*'), "INFO");
                            }
                        }
                    }
                    else
                    {
                        //Warning: Il n'existe pas de Trade/Position en sens inverse.
                        //         Par conséquent, il devient inutile de tenter de clôturer les autres trades CLOSE, qui sont de fait de même sens que le trade courant.
                        //         On initialise alors "rowTradeClosing" à null afin de sortir de la boucle.
                        rowTradeClosing = null;
                        System.Diagnostics.Debug.WriteLine("  ==> NONE Offsetting! ".PadRight(80, '*'), "INFO");
                    }
                }
                System.Diagnostics.Debug.WriteLine("End ".PadRight(80, '-'), "INFO");
            } while (rowTradeClosing != null);

            return ret;
        }
        #endregion ClearingBLKCalc_HILO
        #region ClearingSPECCalc
        /// <summary>
        /// Calcul de la clôture/compensation spécifique
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel ClearingSPECCalc()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IPosRequestDetClearingSPEC detail = (IPosRequestDetClearingSPEC)m_PosRequest.DetailBase;
            DataRow rowTradeClosing = m_DtPosition.Rows.Find(m_PosRequest.IdT);
            if (null != rowTradeClosing)
            {
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                decimal closingQty = Math.Min(m_PosRequest.Qty, Convert.ToDecimal(rowTradeClosing["QTY"]));
                if (closingQty < m_PosRequest.Qty)
                {
                    codeReturn = Cst.ErrLevel.FAILURE;
                    
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5170), 1,
                        new LogParam(LogTools.IdentifierAndId(rowTradeClosing["IDENTIFIER"].ToString(), m_PosRequest.IdT)),
                        new LogParam(m_PosRequest.Qty),
                        new LogParam(closingQty),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
                }
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    string sideClosing = rowTradeClosing["SIDE"].ToString();
                    foreach (IPosKeepingClearingTrade tradeTarget in detail.TradesTarget)
                    {
                        // EG 20170127 Qty Long To Decimal
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5059), 1,
                            new LogParam(LogTools.IdentifierAndId(tradeTarget.Identifier, tradeTarget.IdT)),
                            new LogParam(StrFunc.FmtDecimalToCurrentCulture(tradeTarget.ClosableQty.ToString())),
                            new LogParam(DtFunc.DateTimeToStringDateISO(tradeTarget.DtBusiness))));
                        
                        int idT_Closable = tradeTarget.IdT;
                        DataRow rowTradeClosable = m_DtPosition.Rows.Find(tradeTarget.IdT);
                        DataRow newRow = null;
                        if (null != rowTradeClosable)
                        {
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            // EG 20170127 Qty Long To Decimal
                            decimal closableQty = Math.Min(tradeTarget.ClosableQty, Convert.ToDecimal(rowTradeClosable["QTY"]));

                            if (closableQty < tradeTarget.ClosableQty)
                            {
                                codeReturn = Cst.ErrLevel.FAILURE;
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                
                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5170), 2,
                                    new LogParam(LogTools.IdentifierAndId(tradeTarget.Identifier, tradeTarget.IdT)),
                                    new LogParam(tradeTarget.ClosableQty),
                                    new LogParam(closableQty),
                                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

                                break;
                            }
                            newRow = m_DtPosActionDet.NewRow();
                            newRow.BeginEdit();
                            newRow["IDPA"] = 0;
                            newRow["IDT_CLOSING"] = m_PosRequest.IdT;
                            newRow["SIDE_CLOSING"] = sideClosing;
                            if (IsTradeBuyer(sideClosing))
                            {
                                newRow["IDT_BUY"] = m_PosRequest.IdT;
                                newRow["IDT_SELL"] = tradeTarget.IdT;
                            }
                            else
                            {
                                newRow["IDT_SELL"] = m_PosRequest.IdT;
                                newRow["IDT_BUY"] = tradeTarget.IdT;
                            }
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            decimal qty = Math.Min(closableQty, closingQty);
                            newRow["QTY"] = qty;
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            newRow["ORIGINALQTY_CLOSED"] = Convert.ToDecimal(rowTradeClosable["QTY"]);
                            newRow["ORIGINALQTY_CLOSING"] = Convert.ToDecimal(rowTradeClosing["QTY"]);
                            newRow.EndEdit();
                            m_DtPosActionDet.Rows.Add(newRow);
                            closingQty -= qty;
                            // Mise à jour DtPosition (pour la poursuite du traitement)
                            rowTradeClosable.BeginEdit();
                            rowTradeClosable["QTY"] = closableQty - qty;
                            rowTradeClosable.EndEdit();
                            // Mise à jour DtPosition (pour la poursuite du traitement)
                            rowTradeClosing.BeginEdit();
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            rowTradeClosing["QTY"] = Convert.ToDecimal(rowTradeClosing["QTY"]) - qty;
                            rowTradeClosing.EndEdit();
                        }
                    }
                }
            }
            return codeReturn;
        }
        #endregion ClearingSPECCalc
        #region ClearingSPECGen
        /// <summary>
        /// COMPENSATION/CLOTURE SPECIFIQUE (INTRADAY) 
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para> ► Message de type PosKeepingRequestMQueue</para> 
        ///<para>   ● REQUESTYPE = CLEARSPEC</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel ClearingSPECGen()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5059), 0,
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("TRADE"), m_PosRequest.IdT)),
                new LogParam(StrFunc.FmtIntegerToCurrentCulture(m_PosRequest.Qty.ToString())),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

            Cst.ErrLevel codeReturn = InitPosKeepingData(m_PosRequest.IdT, Cst.PosRequestAssetQuoteEnum.None, false);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                DataSet ds = GetPositionDetailAndPositionAction(DtBusiness);
                if (null != ds)
                {
                    m_DtPosition = ds.Tables[0];
                    m_DtPosActionDet = ds.Tables[1];
                    m_DtPosActionDet_Working = ds.Tables[1].Clone();
                    m_DtPosition.PrimaryKey = new DataColumn[] { m_DtPosition.Columns["IDT"] };
                    // Row clôturante
                    codeReturn = ClearingSPECCalc();
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        int idPA = 0;
                        codeReturn = PosKeep_Updating(m_PosRequest.NbTokenIdE, ref idPA);
                    }

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        UpdateTradeStUsedBy(CS, m_PosRequest.IdT, Cst.StatusUsedBy.REGULAR, null);
                        IPosRequestDetClearingSPEC detail = (IPosRequestDetClearingSPEC)m_PosRequest.DetailBase;
                        foreach (IPosKeepingClearingTrade tradeTarget in detail.TradesTarget)
                        {
                            UpdateTradeStUsedBy(CS, tradeTarget.IdT, Cst.StatusUsedBy.REGULAR, null);
                        }
                    }
                }
            }
            return codeReturn;
        }
        #endregion ClearingSPECGen
        #region CreateTradeTransfer
        /// <summary>
        /// POSITIONTRANSFER (DE MASSE/ BULK) (REQUESTYPE = POT)
        ///<para>
        /// Warning: Rien n'est opérer concernant les Brokers (Entity, Clearing et Executing).  
        ///</para>
        ///<para>
        /// Par conséquent, si les Dealers définis pour les nouveaux trades diffèrent totalement du Dealer du trade source, 
        /// on pourra (avec cette version) se retrouver à générer des trades incohérents.
        ///</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20141230 [20587] Refactoring
        /// FI 20161005 [XXXXX] Modify
        // EG 20180205 [23769] Used dbTransaction      
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20181022 New Mise à jour des dates sur anciens trades (DTORDERENTERED, TZFACILITY etc.)
        // EG 20190613 [24683] Upd (Use DbTransaction)
        // EG 20190613 [24683] Upd (Out newTrade_Identifier)
        // EG 20210315 [24683] Ajout dbTransaction sur SetActorTransfer
        // EG 20211029 [25696] Transfert de position : Ajout préfixe sur EXTLLINK
        // EG 20220816 [XXXXX] [WI396] Transfert (idem Split) : Passage pDbTransaction en paramètre
        private Cst.ErrLevel CreateTradeTransfer(IDbTransaction pDbTransaction, ref Pair<int, string> pTradeTransferResult)
        {
            #region Création du nouveau trade sur la base du trade transféré
            //------------------------------------------------------------------
            //Warning: concernant les Brokers, voir le summary de cette méthode.
            //------------------------------------------------------------------
            DataDocumentContainer _dataDocContainer = (DataDocumentContainer)m_TradeLibrary.DataDocument.Clone();
            ProductContainer _productContainer = new ProductContainer(_dataDocContainer.CurrentProduct.Product, _dataDocContainer);

            IPosRequestTransfer posRequestTransfer = (IPosRequestTransfer)m_PosRequest;
            IPosRequestDetTransfer detail = (IPosRequestDetTransfer)posRequestTransfer.Detail;


            m_MasterPosRequest.PosKeepingKeySpecified = true;
            m_MasterPosRequest.PosKeepingKey = m_Product.CreatePosKeepingKey();
            m_MasterPosRequest.PosKeepingKey.IdI = _productContainer.IdI;
            // EG 20220816 [XXXXX] [WI396] Transfert (idem Split) : Passage pDbTransaction en paramètre
            Cst.ErrLevel codeReturn = SetAdditionalInfoFromTrade(pDbTransaction, m_MasterPosRequest);

            if (codeReturn == Cst.ErrLevel.SUCCESS)
            {
                IPosRequestTradeAdditionalInfo additionalInfo = (IPosRequestTradeAdditionalInfo)m_TemplateDataDocumentTrade[_productContainer.IdI];

                DataSetTrade dsTrade = new DataSetTrade(CS, pDbTransaction, m_PosRequest.IdT);
                if (false == Convert.IsDBNull(dsTrade.DtTrade.Rows[0]["DISPLAYNAME"]))
                    additionalInfo.DisplayName = dsTrade.DtTrade.Rows[0]["DISPLAYNAME"].ToString();
                if (false == Convert.IsDBNull(dsTrade.DtTrade.Rows[0]["DESCRIPTION"]))
                    additionalInfo.Description = dsTrade.DtTrade.Rows[0]["DESCRIPTION"].ToString();
                if (false == Convert.IsDBNull(dsTrade.DtTrade.Rows[0]["EXTLLINK"]))
                {
                    // EG 20211029 [25696] Transfert de position : Ajout préfixe sur EXTLLINK
                    string prefix = "[" + ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(posRequestTransfer.RequestType) + "]";
                    additionalInfo.ExtlLink = dsTrade.DtTrade.Rows[0]["EXTLLINK"].ToString();
                    additionalInfo.ExtlLink = prefix + additionalInfo.ExtlLink.Replace(prefix, string.Empty);
                }

                RptSideProductContainer rptSide = _dataDocContainer.CurrentProduct.RptSide(CS, true);
                // FI 20161005 [XXXXX] Add  NotImplementedException
                if (null == rptSide)
                    throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented ", _dataDocContainer.CurrentProduct.Product.ProductBase.ToString()));

                IPayment paymentGrossAmount = null;
                ExchangeTradedContainer _exchangeTradedContainer = null;
                if (_dataDocContainer.CurrentProduct.IsExchangeTradedDerivative)
                {
                    _exchangeTradedContainer = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)_dataDocContainer.CurrentProduct.Product);
                    _exchangeTradedContainer.InitPositionTransfer(posRequestTransfer.Qty, posRequestTransfer.DtBusiness.Date);
                    // EG 20211029 [25696] Mise à jour de ClearedDate avec la DtBusiness du Transfert
                    _dataDocContainer.SetClearedDate(posRequestTransfer.DtBusiness);
                }
                // EG 20150630 Upd Calcul du GAM
                else if (_dataDocContainer.CurrentProduct.IsEquitySecurityTransaction)
                {
                    #region EquitySecurityTransaction
                    IEquitySecurityTransaction _equitySecurityTransaction = (IEquitySecurityTransaction)_dataDocContainer.CurrentProduct.Product;
                    _exchangeTradedContainer = new EquitySecurityTransactionContainer(_equitySecurityTransaction);
                    _exchangeTradedContainer.InitPositionTransfer(posRequestTransfer.Qty, posRequestTransfer.DtBusiness.Date);
                    // EG 20211029 [25696] Mise à jour de ClearedDate avec la DtBusiness du Transfert
                    _dataDocContainer.SetClearedDate(posRequestTransfer.DtBusiness.Date);
                    // GAM
                    paymentGrossAmount = _equitySecurityTransaction.GrossAmount;
                    Pair<Nullable<decimal>, string> priceGrossAmount =
                        Tools.ConvertToQuotedCurrency(CS, pDbTransaction, new Pair<Nullable<decimal>, string>(
                            _equitySecurityTransaction.TradeCaptureReport.LastPx.DecValue, paymentGrossAmount.PaymentAmount.Currency));
                    paymentGrossAmount.PaymentAmount.Amount.DecValue = posRequestTransfer.Qty * priceGrossAmount.First.Value;
                    #endregion EquitySecurityTransaction
                }
                // EG 20150630 New  
                else if (_dataDocContainer.CurrentProduct.IsDebtSecurityTransaction)
                {
                    #region DebtSecurityTransaction
                    IDebtSecurityTransaction _debtSecurityTransaction = (IDebtSecurityTransaction)_dataDocContainer.CurrentProduct.Product;
                    DebtSecurityTransactionContainer _debtSecurityTransactionContainer = new DebtSecurityTransactionContainer(_debtSecurityTransaction, _dataDocContainer);
                    _debtSecurityTransactionContainer.SetOrderQuantityForAction(posRequestTransfer.Qty);

                    decimal prorata = NotionalProrata(detail.InitialQty, posRequestTransfer.Qty);
                    // GrossAmount prorata
                    IMoney grossAmount = GrossAmountProrataCalculation(_dataDocContainer, prorata);
                    paymentGrossAmount = _debtSecurityTransaction.GrossAmount;
                    paymentGrossAmount.PaymentAmount.Amount.DecValue = grossAmount.Amount.DecValue;
                    // AccruedInterest prorata
                    if (_debtSecurityTransaction.Price.AccruedInterestAmountSpecified)
                    {
                        IMoney accruedInterest = AccruedInterestProrataCalculation(_dataDocContainer, prorata);
                        _debtSecurityTransaction.Price.AccruedInterestAmount = accruedInterest;
                    }

                    // DTBUSINESS
                    // EG 20211029 [25696] Mise à jour de ClearedDate avec la DtBusiness du Transfert
                    _dataDocContainer.SetClearedDate(posRequestTransfer.DtBusiness);
                    #endregion DebtSecurityTransaction
                }
                else if (_dataDocContainer.CurrentProduct.IsReturnSwap)
                {
                    #region ReturnSwap
                    ReturnSwapContainer _returnSwapContainer = new ReturnSwapContainer(CS, pDbTransaction, (IReturnSwap)_dataDocContainer.CurrentProduct.Product, _dataDocContainer);
                    _returnSwapContainer.SetMainOpenUnits(posRequestTransfer.Qty);
                    // EG 20211029 [25696] Mise à jour de ClearedDate avec la DtBusiness du Transfert
                    _dataDocContainer.SetClearedDate(posRequestTransfer.DtBusiness);
                    #endregion ReturnSwap

                }

                if (null != rptSide)
                {
                    #region Dealer|Clearer Update
                    if (detail.DealerBookIdTargetSpecified)
                    {
                        IParty _dealerParty = _dataDocContainer.GetParty(rptSide.GetDealer().PartyId.href);
                        SetActorTransfer(pDbTransaction, _dataDocContainer, _dealerParty, detail.DealerTarget.OTCmlId, detail.DealerBookIdTarget.OTCmlId);
                        SideEnum side = (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER) ? SideEnum.Buy : SideEnum.Sell);
                        rptSide.SetBuyerSeller(_dealerParty.Id, side, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                    }
                    // Clearer|Custodian --> Update (Traders/Sales --> Remove)
                    if (detail.ClearerBookIdTargetSpecified)
                    {
                        IParty _clearerParty = _dataDocContainer.GetParty(rptSide.GetClearerCustodian().PartyId.href);
                        SetActorTransfer(pDbTransaction, _dataDocContainer, _clearerParty, detail.ClearerTarget.OTCmlId, detail.ClearerBookIdTarget.OTCmlId);
                        // RD 20151210 [21519] Set new Clearer as Buyer or Seller and its role.
                        SideEnum side = (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER) ? SideEnum.Sell : SideEnum.Buy);

                        if (ActorTools.IsActorWithRole(CS, pDbTransaction, _clearerParty.OTCmlId, RoleActor.CSS))
                        {
                            rptSide.SetBuyerSeller(_clearerParty.Id, side, PartyRoleEnum.ClearingOrganization);
                        }
                        else
                        {
                            if (_exchangeTradedContainer.IsExchangeTradedDerivative)
                            {
                                if (ActorTools.IsActorWithRole(CS, pDbTransaction, _clearerParty.OTCmlId, RoleActor.CLEARER))
                                {
                                    rptSide.SetBuyerSeller(_clearerParty.Id, side, PartyRoleEnum.ClearingFirm);
                                }
                            }
                            else if (ActorTools.IsActorWithRole(CS, pDbTransaction, _clearerParty.OTCmlId, RoleActor.CUSTODIAN))
                            {
                                rptSide.SetBuyerSeller(_clearerParty.Id, side, PartyRoleEnum.Custodian);
                            }
                        }
                    }
                    _productContainer.SynchronizeFromDataDocument();
                    #endregion Dealer|Clearer Update

                    #region Others Buyer|seller|Payer|receiver Update
                    // EG 20150630 Upd Maj des parties du GAM
                    if (null != paymentGrossAmount)
                    {
                        if (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                        {

                            paymentGrossAmount.PayerPartyReference.HRef = rptSide.GetDealer().PartyId.href;
                            paymentGrossAmount.ReceiverPartyReference.HRef = rptSide.GetClearerCustodian().PartyId.href;
                        }
                        else
                        {
                            paymentGrossAmount.ReceiverPartyReference.HRef = rptSide.GetDealer().PartyId.href;
                            paymentGrossAmount.PayerPartyReference.HRef = rptSide.GetClearerCustodian().PartyId.href;
                        }
                        if (0 < posRequestTransfer.DtBusiness.Date.CompareTo(paymentGrossAmount.PaymentDate.UnadjustedDate.DateValue))
                            paymentGrossAmount.PaymentDate.UnadjustedDate.DateValue = posRequestTransfer.DtBusiness.Date;

                    }

                    if (_dataDocContainer.CurrentProduct.IsDebtSecurityTransaction)
                    {
                        IDebtSecurityTransaction _debtSecurityTransaction = (IDebtSecurityTransaction)_dataDocContainer.CurrentProduct.Product;
                        if (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                        {
                            _debtSecurityTransaction.BuyerPartyReference.HRef = rptSide.GetDealer().PartyId.href;
                            _debtSecurityTransaction.SellerPartyReference.HRef = rptSide.GetClearerCustodian().PartyId.href;
                        }
                        else
                        {
                            _debtSecurityTransaction.BuyerPartyReference.HRef = rptSide.GetClearerCustodian().PartyId.href;
                            _debtSecurityTransaction.SellerPartyReference.HRef = rptSide.GetDealer().PartyId.href;
                        }
                    }
                    if (_dataDocContainer.CurrentProduct.IsReturnSwap)
                    {
                        ReturnSwapContainer _returnSwapContainer = new ReturnSwapContainer(CS, pDbTransaction, (IReturnSwap)_dataDocContainer.CurrentProduct.Product, _dataDocContainer);
                        if (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                        {
                            _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef = rptSide.GetDealer().PartyId.href;
                            _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef = rptSide.GetClearerCustodian().PartyId.href;
                            _returnSwapContainer.ReturnLeg.PayerPartyReference.HRef = _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef;
                            _returnSwapContainer.ReturnLeg.ReceiverPartyReference.HRef = _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef;
                            _returnSwapContainer.InterestLeg.PayerPartyReference.HRef = _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef;
                            _returnSwapContainer.InterestLeg.ReceiverPartyReference.HRef = _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef;
                        }
                        else
                        {
                            _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef = rptSide.GetDealer().PartyId.href;
                            _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef = rptSide.GetClearerCustodian().PartyId.href;
                            _returnSwapContainer.ReturnLeg.PayerPartyReference.HRef = _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef;
                            _returnSwapContainer.ReturnLeg.ReceiverPartyReference.HRef = _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef;
                            _returnSwapContainer.InterestLeg.PayerPartyReference.HRef = _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef;
                            _returnSwapContainer.InterestLeg.ReceiverPartyReference.HRef = _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef;

                        }
                    }

                    #endregion Others Buyer|seller|Payer|receiver Update
                }

                // EG 20181022 New
                SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CS, m_PosRequest.IdT)
                {
                    DbTransaction = pDbTransaction
                };
                // EG 20240531 [WI926] Add Parameter pIsTemplate
                _dataDocContainer.UpdateMissingTimestampAndFacility(CS, sqlTrade, false);


                if (detail.IsCalcNewFeesSpecified && detail.IsCalcNewFees)
                {
                    // Trade transféré
                    //SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CS, m_PosRequest.idT);
                    //sqlTrade.dbTransaction = pDbTransaction;

                    // Recalcul des frais si demandé 
                    // EG 20150112 New
                    TradeInput tradeInput = new TradeInput
                    {
                        SQLProduct = additionalInfo.SqlProduct,
                        SQLTrade = sqlTrade,
                        DataDocument = _dataDocContainer
                    };
                    tradeInput.TradeStatus.Initialize(ProcessBase.Cs, pDbTransaction, sqlTrade.IdT);
                    tradeInput.RecalculFeeAndTax(CS, pDbTransaction);
                }
                else
                {
                    _dataDocContainer.OtherPartyPaymentSpecified = false;
                }

                //------------------------------------------------------------------
                // Sauvegarde du nouveau trade dans la base de données
                //------------------------------------------------------------------
                List<Pair<int, string>> tradeLinkInfo = new List<Pair<int, string>>
                {
                    new Pair<int, string>(posRequestTransfer.IdT, posRequestTransfer.Identifiers.Trade)
                };

                codeReturn = RecordTrade(pDbTransaction, _dataDocContainer, additionalInfo, null, Cst.PosRequestTypeEnum.PositionTransfer, tradeLinkInfo, out int newTrade_IdT, out string newTrade_Identifier);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    pTradeTransferResult = new Pair<int, string>(newTrade_IdT, newTrade_Identifier);
                }
            }
            #endregion Création du nouveau trade sur la base du trade transféré
            return codeReturn;
        }
        #endregion CreateTradeTransfer

        #region ClosingDay_InitialMarginControl
        // EG 20180221 [23769] New (auparavant sur PosKeepingTools)
        private int ClosingDay_InitialMarginControl(IPosRequest pPosRequest, bool pIs_IMCTRL)
        {
            // ------------------------------------------------------------------------------------------
            //      1. Il existe des lignes dans IMREQUEST ......................................= FAUX
            // OK = 2. Il y a une position à cette date..........................................= FAUX
            //      4. Un traitement de tenue de position a eu lieu APRES calcul de DEPOSITS.....= FAUX
            // ─ OU ────────────────────────────────────────────────────────────────────────────────────
            //      1. Il existe des lignes dans IMREQUEST ......................................= VRAI
            // OK = 3. Elles sont en SUCCESS / WARNING...........................................= VRAI
            //      4. Un traitement de tenue de position a eu lieu APRES calcul de DEPOSITS.....= FAUX
            // -----------------------------------------------------------------------------------------
            DataSet ds = PosKeepingTools.GetDataInitialMarginControl(CS, 0, pPosRequest.IdA_Entity, pPosRequest.IdA_Css, pPosRequest.DtBusiness);
            int ret = 0;
            int errCode = 5131;
            if (null != ds)
            {
                if (0 == ds.Tables[0].Rows.Count)
                {
                    ds = PosKeepingTools.GetDataInitialMarginControl(CS, 1, pPosRequest.IdA_Entity, pPosRequest.IdA_Css, pPosRequest.DtBusiness);
                    if (0 == ds.Tables[0].Rows.Count)
                    {
                        // 1. FAUX et 2. FAUX
                        ds = PosKeepingTools.GetDataInitialMarginControl(CS, 2, pPosRequest.IdA_Entity, pPosRequest.IdA_Css, pPosRequest.DtBusiness);
                        errCode = 5132;
                    }
                }
                else if (0 == ds.Tables[1].Rows.Count)
                {
                    // 1. VRAI et 3.VRAI
                    // RD/FL 20140623 [20123]
                    // Exécuter la troisième requête uniquement si ENTITY.ISCLOSINGDAYIMCTRL est à true.
                    // C'est pour pouvoir bypasser ce contrôle (chez XI dans notre cas) car la requête récursive génère un timeout.
                    // A faire:
                    // - Peut être déplacer ce code carrement à l'appel de la méthode ClosingDayInitilaMarginControl, voir avant.
                    // - Peut être crééer une table enfant de la table ENTITY pour pouvoir brancher ce genre de bypasse sur d'autres contrôles, voir d'autres traitements.                
                    //ds = GetDataInitialMarginControl(pCS, 2, pIdA_Entity, pIdA_CSS, pDtBusiness);
                    //errCode = 5132;
                    if (pIs_IMCTRL)
                    {
                        ds = PosKeepingTools.GetDataInitialMarginControl(CS, 2, pPosRequest.IdA_Entity, pPosRequest.IdA_Css, pPosRequest.DtBusiness);
                        errCode = 5132;
                    }
                    else
                        ds = null;
                }

                // RD/FL 20140623 [20123]
                // Ajouter test ds != null 
                if (ds != null && 0 < ds.Tables[0].Rows.Count)
                {
                    // SUCCESS---------------------------
                    // 1. FAUX et 2.FAUX et 4.FAUX
                    // 1. VRAI et 3.FAUX
                    // 1. VRAI et 3.VRAI et 4.FAUX
                    // ERROR ---------------------------
                    // 1. FAUX et 2.FAUX et 4.VRAI
                    // 1. FAUX et 2.VRAI
                    // 1. VRAI et 3.VRAI et 4.VRAI
                    ret = errCode;
                }
            }
            return ret;
        }
        #endregion ClosingDay_InitialMarginControl


        #region CopyEventClass
        /// EG 20141224 [20566] La date DTEVENT des événements de la quantité non décompensée passe à la date DTBUSINESS
        private void CopyEventClass(DataRow pRowSource, int pNewIdE)
        {
            DataRow[] rowClassChilds = pRowSource.GetChildRows(m_DsEvents.ChildEventClass);
            foreach (DataRow rowClassChild in rowClassChilds)
            {
                if (false == EventClassFunc.IsRemoveEvent(rowClassChild["EVENTCLASS"].ToString()))
                {
                    object[] clone = (object[])rowClassChild.ItemArray.Clone();
                    DataRow rowNewClassChild = m_DsEvents.DtEventClass.NewRow();
                    rowNewClassChild.ItemArray = clone;
                    rowNewClassChild.BeginEdit();
                    rowNewClassChild["IDE"] = pNewIdE;
                    //EG 20141224 DTEVENT = DTBUSINESS
                    rowNewClassChild["DTEVENT"] = m_MasterPosRequest.DtBusiness;
                    rowNewClassChild["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(CS, m_MasterPosRequest.DtBusiness);
                    rowNewClassChild.EndEdit();
                    m_DsEvents.DtEventClass.Rows.Add(rowNewClassChild);
                }
            }
        }
        #endregion CopyEventClass
        #region CopyEventDet
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private void CopyEventDet(DataRow pRowSource, int pNewIdE, decimal pRemainderQty)
        {
            if (null != pRowSource)
            {
                object[] clone = (object[])pRowSource.ItemArray.Clone();
                DataRow rowNewEventDet = m_DsEvents.DtEventDet.NewRow();
                rowNewEventDet.ItemArray = clone;
                rowNewEventDet.BeginEdit();
                rowNewEventDet["IDE"] = pNewIdE;
                rowNewEventDet["DAILYQUANTITY"] = pRemainderQty;
                rowNewEventDet.EndEdit();
                m_DsEvents.DtEventDet.Rows.Add(rowNewEventDet);
            }
        }
        #endregion CopyEventDet

        #region DeleteEventPhysicalPeriodicDelivery
        // EG 20170206 [22787] new
        protected virtual Cst.ErrLevel DeleteEventPhysicalPeriodicDelivery(IDbTransaction pDbTransaction, DataRow pRowClosing, DataRow pRowClosed)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion DeleteEventPhysicalPeriodicDelivery
        #region DeletePosActionDetAndLinkedEvent
        /// <summary>
        /// Suppression des Evénements liés à la ligne de clôture à supprimer via EVENTPOSACTIONDET
        /// Evénements liés : 
        /// . NQS-QTY/NOM
        /// . OFS-PAR/TOT et ses enfants
        /// Suppression de la ligne de clôture dans POSACTIONDET
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdPADET"></param>
        /// <returns></returns>
        private Cst.ErrLevel DeletePosActionDetAndLinkedEvent(IDbTransaction pDbTransaction, int pIdPADET)
        {
            // Delete EVENT via EVENTPOSACTIONDET link
            string sqlQuery = SQLCst.DELETE_DBO + Cst.OTCml_TBL.EVENT.ToString() + Cst.CrLf;
            if (DataHelper.IsDbSqlServer(CS))
            {
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT.ToString() + " ev" + Cst.CrLf;
                sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTPOSACTIONDET.ToString() + " epad ";
                sqlQuery += SQLCst.ON + "(epad.IDE = ev.IDE)" + SQLCst.AND + "(epad.IDPADET = @IDPADET)";
            }
            else if (DataHelper.IsDbOracle(CS))
            {
                sqlQuery += SQLCst.WHERE + "IDE" + SQLCst.IN + Cst.CrLf;
                sqlQuery += "(" + SQLCst.SELECT + "ev.IDE" + SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT.ToString() + " ev" + Cst.CrLf;
                sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTPOSACTIONDET.ToString() + " epad ";
                sqlQuery += SQLCst.ON + "(epad.IDE = ev.IDE)" + SQLCst.AND + "(epad.IDPADET = @IDPADET)";
                sqlQuery += ")";
            }
            // Delete POSACTIONDET et EVENTPOSACTIONDET (cascade)
            sqlQuery += SQLCst.SEPARATOR_MULTIDML;
            sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.POSACTIONDET.ToString();
            sqlQuery += SQLCst.WHERE + "(IDPADET = @IDPADET)";
            sqlQuery += SQLCst.SEPARATOR_MULTIDML;

            // POSACTIONDET Delete
            // FI 20180830 [24152] use of DataParameters
            //paramIdPADet.Value = pIdPADET;
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(CS, "IDPADET", DbType.Int32), pIdPADET);
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery, dp.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion DeletePosActionDetAndLinkedEvent
        #region DeserializeAndLockTrade
        private Cst.ErrLevel DeserializeAndLockTrade(int pIdT, string pIdentifier)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            DeserializeTrade(pIdT);

            m_LockObject = m_PKGenProcess.LockElement(TypeLockEnum.TRADE, pIdT, pIdentifier, true, LockTools.Exclusive);
            if (null == m_LockObject)
                codeReturn = Cst.ErrLevel.LOCKUNSUCCESSFUL;

            return codeReturn;
        }
        #endregion DeserializeAndLockTrade

        #region ExcludeTradeOrphanPriceIdentical
        /// <summary>
        /// Traitement des trades candidats sur un contexte avec PRIX = IDENTIQUE
        /// . Suppression des trades orphelin (count()=1
        /// . Suppresion de la clé de regroupement des trades, si elle ne comporte plus de trades ou 1 seul  
        /// </summary>
        /// <returns></returns>
        private Cst.ErrLevel ExcludeTradeOrphanPriceIdentical()
        {
            // on ne travaille que sur les contextes de merge avec plus d'un trade mergable
            bool isExistMergable = m_LstTradeMergeRule.Exists(tmr => ((null != tmr.IdT_Mergeable) && (1 < tmr.IdT_Mergeable.Count)));
            if (isExistMergable)
            {
                // Selection de tous les contextes qui ont des trades "MERGABLE" regroupés par Clé
                List<TradeMergeRule> _lstTmr = m_LstTradeMergeRule.FindAll(tmr => ((null != tmr.IdT_Mergeable) && (0 < tmr.IdT_Mergeable.Count)));
                // Application du contexte "PriceValue"
                _lstTmr.ForEach(tmr =>
                {
                    if (tmr.Context.PriceValue.HasValue &&
                        tmr.Context.PriceValue.Value == PriceValueEnum.IDENTICAL)
                    {
                        // Prix identiques
                        foreach (TradeKey key in tmr.IdT_Mergeable.Keys)
                        {
                            // Suppression des Trades MERGABLE car PRIX IDENTIQUE ORPHELIN
                            List<decimal> _priceOrphan = (
                                from p in tmr.IdT_Mergeable[key]
                                group p by p.Second into grp
                                where grp.Count() == 1
                                select grp.Key).ToList();

                            tmr.IdT_Mergeable[key].RemoveAll(idT => _priceOrphan.Exists(price => price == idT.Second));
                        }
                        // Suppression de l'élément avec valeurs vides ou = 1
                        tmr.IdT_Mergeable.ToList().Where(item => (null == item.Value) || (item.Value.Count < 2)).ToList().ForEach(item => tmr.IdT_Mergeable.Remove(item.Key));
                    }
                });
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion ExcludeTradeOrphanPriceIdentical

        #region GetPositionDetailVeilAndTradeDay
        /// <summary>
        /// Retourne 2 datatable pour une clé de position:
        /// <para>- un datatable avec les allocations en Position Veille et les allocations du Jour</para>
        /// <para>- un datatable image de POSACTIONDET pour les actions en Position Jour</para> 
        /// </summary>
        /// <returns></returns>
        /// FI 20130313 [18467] add qryParameters
        /// FI 20140403 [18467] use AddParameterDealer
        /// EG 20140204 [19586] IsolationLevel 
        // EG 20141224 [20566] DTPOS unused
        private DataSet GetPositionDetailVeilAndTradeDay(DateTime pDate)
        {
            // Si la date du message (POSKEEPENTRY) est > à la date de bourse du marché
            // alors la date veille = Date de bourse du marché en cours
            // sinon la date veille = Date de bourse veille du marché.
            DateTime dtBusiness = (pDate < PosKeepingData.Market.DtMarket) ? PosKeepingData.Market.DtMarket : pDate;

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), dtBusiness); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "IDI", DbType.Int32), PosKeepingData.IdI);
            parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), PosKeepingData.IdAsset);
            parameters.Add(new DataParameter(CS, "IDA_DEALER", DbType.Int32), PosKeepingData.IdA_Dealer);
            parameters.Add(new DataParameter(CS, "IDB_DEALER", DbType.Int32), PosKeepingData.IdB_Dealer);
            parameters.Add(new DataParameter(CS, "IDA_CLEARER", DbType.Int32), PosKeepingData.IdA_Clearer);
            parameters.Add(new DataParameter(CS, "IDB_CLEARER", DbType.Int32), PosKeepingData.IdB_Clearer);

            // EG 20151125 [20979] Refactoring
            string sqlSelect = GetQueryPositionVeilAndTradeDayAndActionOnDtBusiness();
            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, 480);
            DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, 480);
            // EG 20150920 [21314] ORIGINALQTY_CLOSED|ORIGINALQTY_CLOSING added after Dataset construction
            // EG 20170127 Qty Long To Decimal
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSED", typeof(decimal));
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSING", typeof(decimal));
            return ds;
        }
        #endregion GetPositionDetailVeilAndTradeDay
        #region GetPosRequestLogValue
        // EG 20180307 [23769] Gestion Asynchrone
        public string GetPosRequestLogValue(Cst.PosRequestTypeEnum pPosRequest)
        {
            string ret = pPosRequest.ToString();
            string @value = ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(pPosRequest);
            if (StrFunc.IsFilled(@value))
            {
                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                //ExtendEnum enums = ExtendEnumsTools.ListEnumsSchemes["PosRequestTypeEnum"];
                ExtendEnum enums = DataEnabledEnumHelper.GetDataEnum(CS, "PosRequestTypeEnum");
                ExtendEnumValue enumValue = enums.GetExtendEnumValueByValue(@value);
                if (null != enumValue)
                    ret = Ressource.GetString(enumValue.ExtValue, ret);
            }
            return ret;
        }
        #endregion GetPosRequestLogValue

        #region GetProduct()
        protected virtual IProduct GetProduct()
        {
            IProduct _product = null;
            if ((null != m_RptSideProductContainer) && m_RptSideProductContainer.DataDocumentSpecified)
            {
                _product = m_RptSideProductContainer.DataDocument.CurrentProduct.Product;
            }
            return _product;
        }
        #endregion GetProduct()
        #region GetUnclearingPosactionDet
        /// <summary>
        /// Retourne un DataSet de la ligne POSACTIONDET à décompenser
        /// </summary>
        /// <returns></returns>
        private DataSet GetUnclearingPosactionDet(IDbTransaction pDbTransaction, int pIdPADET, int pIdT_Closed, int pIdT_Closing)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDPADET", DbType.Int32), pIdPADET);
            parameters.Add(new DataParameter(CS, "IDT_CLOSED", DbType.Int32), pIdT_Closed);
            parameters.Add(new DataParameter(CS, "IDT_CLOSING", DbType.Int32), pIdT_Closing);
            string sqlSelect = GetQueryUnclearingPosactionDet();
            return DataHelper.ExecuteDataset(pDbTransaction, CommandType.Text, sqlSelect, parameters.GetArrayDbParameter());
        }
        /// <summary>
        /// Retourne la liste des POSACTIONDET à décompenser pour un IDT
        /// </summary>
        /// <returns></returns>
        protected DataSet GetUnclearingPosactionDet(IDbTransaction pDbTransaction, int pIdT)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);
            string sqlSelect = GetQueryUnclearingPosactionDetByTrade();
            return DataHelper.ExecuteDataset(pDbTransaction, CommandType.Text, sqlSelect, parameters.GetArrayDbParameter());
        }
        #endregion
        #region GrossAmountProrataCalculation
        protected virtual IMoney GrossAmountProrataCalculation(DataDocumentContainer pDataDocument, decimal pProrata)
        {
            return null;
        }
        #endregion GrossAmountProrataCalculation

        #region InsertTradeLinkPosRequest
        // EG [21465] int[] pIdPR instead of Nullable<int> pIdPR
        // EG 20190918 Refactoring PosRequestTypeEnum (Maturity Redemption DEBTSECURITY)
        private Cst.ErrLevel InsertTradeLinkPosRequest(IDbTransaction pDbTransaction, int[] pIdPR, Cst.PosRequestTypeEnum pPosRequestType, int pIdT)
        {
            if (ArrFunc.IsFilled(pIdPR) && ((Cst.PosRequestTypeEnum.RequestTypeTradeLinkPosRequest & pPosRequestType) == pPosRequestType))
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);
                string sqlQuery = string.Empty;
                foreach (int idPR in pIdPR)
                {
                    sqlQuery += @"insert into dbo.TRLINKPOSREQUEST (IDT, IDPR) values (@IDT," + idPR + ")" + Cst.CrLf;
                }
                DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertTradeLinkPosRequest

        #region IsExistMarketPrice_EOD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDtprice">Date pour laquelle vérifier l'existence des prix</param>
        /// <returns></returns>
        /// EG 20170222 [22717] New
        // EG 20180803 PERF (Utilisation de ASSETCTRL_{buildTableId}_W) seul parameter restant DTBUSINESS
        private bool IsExistMarketPrice_EOD(DateTime pDtBusiness, string pTableName)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), pDtBusiness);

            #region Requête contrôle existence Prix
            string sqlQuery = GetQuerySelectPriceControl_EOD().Replace("ASSETCTRL_MODEL", pTableName);
            QueryParameters queryParameters = new QueryParameters(CS, sqlQuery, dataParameters);
            #endregion Requête contrôle existence Prix

            object obj = DataHelper.ExecuteScalar(CS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            return (obj != null);
        }
        #endregion IsExistMarketPrice_EOD

        #region LoadTradeIntoTradeMergeRules
        /// <summary>
        /// Candidature des trades JOUR à la fusion
        /// 1. Chargement des trades JOUR
        /// 2. Test de matchage de chaque trade avec toutes les lignes du contexte MERGE
        /// 3. Construction de la liste des trades CANDIDATS
        /// <para>Retourne  Cst.ErrLevel.NOTHINGTODO ou Cst.ErrLevel.SUCCESS </para>
        /// </summary>
        /// <returns></returns>
        /// EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel LoadTradeIntoTradeMergeRules()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;

            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8950), 0,
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                new LogParam(m_MarketPosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness))));

            m_DicTradeCandidate = new Dictionary<int, TradeCandidate>();

            // 1. Chargement des trades JOUR
            DataSet ds = GetDataRequest(CS, Cst.PosRequestTypeEnum.TradeMerging, m_PosRequest.DtBusiness, m_PosRequest.IdEM);
            Nullable<int> nbTrades = null;
            if (null != ds)
            {
                nbTrades = ds.Tables[0].Rows.Count;
                if (0 < nbTrades)
                {
                    // Boucle sur TRADES DU JOUR
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        TradeContext _context = new TradeContext(row);
                        // 2. Test de matchage de chaque trade avec toutes les lignes du référentiel de contexte MERGE
                        m_LstTradeMergeRule.ForEach(tmr => tmr.Weighting(_context));

                        // 3. Construction de la liste des trades CANDIDATS
                        // Un trade est CANDIDAT POTENTIEL A MERGING pour un contexte SI Context.ResultMatching > 0
                        List<TradeMergeRule> _lstTmr = m_LstTradeMergeRule.FindAll(tmr => tmr.ResultMatching > 0);
                        if (0 < _lstTmr.Count)
                        {
                            // Stockage des infos complètes du trade candidat
                            TradeCandidate _tradeCandidate = new TradeCandidate(row, _context);
                            m_DicTradeCandidate.Add(_tradeCandidate.IdT, _tradeCandidate);

                            _lstTmr.ForEach(tmr =>
                            {
                                if (null == tmr.IdT_Candidate)
                                    tmr.IdT_Candidate = new List<int>();
                                // Ajout de l'IDT du trade candidat sur chaque contexte qui matche
                                if (false == tmr.IdT_Candidate.Exists(idT => idT == _tradeCandidate.IdT))
                                    tmr.IdT_Candidate.Add(_tradeCandidate.IdT);
                            });
                        }
                    }
                    if (0 < m_DicTradeCandidate.Keys.Count)
                        codeReturn = Cst.ErrLevel.SUCCESS;
                }
            }
            // Si LOG >= Mode LEVEL3 construction du fichier LOG INIT(actuellement mode TXT)
            if (m_PKGenProcess.IsLevelToLog(LogLevelDetail.LEVEL3))
            {
                SaveResultTradeMergeToFileLog("INIT", nbTrades);
            }

            return codeReturn;
        }
        #endregion LoadTradeIntoTradeMergeRules
        #region LockProcessByRequest
        /// <summary>
        /// Alimentation de la table EFSLOCK en fonction du type de demande
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Si la demande comporte un IDT  alors Lock du trade</para>
        ///<para>► Si la demande comporte un IDEM alors Lock de ENTITYMARKET</para>
        ///<para>► Si la demande ne comporte pas d'IDEM alors Lock de tous les ENTITYMARKET appartenant à ENTITE/CSS</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20150317 [POC] GetEntityMarkets (m_MasterPosRequest parameter)
        // EG 20190318 Gestion CodeReturn IRQ sur Lock
        protected Cst.ErrLevel LockProcessByRequest()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (m_MasterPosRequest.IdTSpecified)
                codeReturn = DeserializeAndLockTrade(m_MasterPosRequest.IdT, Queue.GetStringValueIdInfoByKey("TRADE"));

            // EG 20130523 New LockMode
            if (m_MasterPosRequest.IdEMSpecified)
            {
                codeReturn = LockEntityMarket(m_MasterPosRequest.IdEM, m_MasterPosRequest.RequestType.ToString(), m_MasterPosRequest.LockModeEntityMarket);
            }
            else
            {
                // EG 20150317 [POC] mod
                DataSet ds = PosKeepingTools.GetEntityMarkets(CS, m_MasterPosRequest);
                if ((null != ds) && ArrFunc.IsFilled(ds.Tables[0].Rows))
                {
                    m_LockIdEM = new List<int>();
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int idEM = Convert.ToInt32(dr["IDEM"]);
                        // EG 20190318 Gestion Code Return 
                        codeReturn = LockEntityMarket(idEM, m_MasterPosRequest.RequestType.ToString(), m_MasterPosRequest.LockModeEntityMarket);
                        if (Cst.ErrLevel.SUCCESS != codeReturn)
                            break;
                        m_LockIdEM.Add(idEM);
                    }
                }
            }
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                // EG 20151019 [21112] New Lock POSREQUEST (Denouement par position)
                if (m_MasterPosRequest is IPosRequestPositionOption)
                {
                    string objectId = m_MasterPosRequest.IdPR.ToString();
                    TypeLockEnum typeLock = TypeLockEnum.POSREQUEST;
                    // IDEM|IDI|IDASSET|ASSETCATEGORY|DEALER|CLEARER
                    if ((m_MasterPosRequest.IdEMSpecified) && (m_MasterPosRequest.PosKeepingKeySpecified))
                    {
                        typeLock = TypeLockEnum.POSKEEPINGKEY;
                        objectId = m_MasterPosRequest.IdEM.ToString() + "|" + m_MasterPosRequest.PosKeepingKey.LockObjectId;
                    }
                    codeReturn = m_PKGenProcess.LockObjectId(typeLock, objectId, m_MasterPosRequest.GetType().Name, m_PosRequest.RequestType.ToString(), LockTools.Exclusive);
                }
            }
            return codeReturn;
        }
        #endregion LockProcessByRequest
        #region LockEntityMarket
        /// <summary>
        /// LOCK de la ligne ENTITYMARKET (IDEM)
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        private Cst.ErrLevel LockEntityMarket(int pIdEM, string pAction, string pLockMode)
        {
            string identifier = Queue.GetStringValueIdInfoByKey("ENTITY");
            identifier += " - " + Queue.GetStringValueIdInfoByKey("MARKET");
            // EG 20151123 ObjectId = string
            return m_PKGenProcess.LockObjectId(TypeLockEnum.ENTITYMARKET, pIdEM.ToString(), identifier, pAction, pLockMode);
        }
        #endregion LockEntityMarket
        #region LogDisplayTradeMergeResult
        /// <summary>
        /// Alimentation du fichier LOG avec les résultats (INIT, START, END)
        /// </summary>
        /// <param name="pMode">INIT, START, END (type de fichier)</param>
        /// <param name="pNbTrade">Nombre de trades candidats</param>
        /// <returns></returns>
        private string LogDisplayTradeMergeResult(string pMode, Nullable<int> pNbTrade)
        {
            StringBuilder result = new StringBuilder();
            result.Append(LogDisplayTradeMergeTitle(pMode, pNbTrade));
            #region Lecture des contextes
            m_LstTradeMergeRule.ForEach(tmr =>
            {
                // Ecriture du contexte
                result.Append(LogDisplayTradeMergeRuleContext(tmr, pMode));

                if (null != tmr.IdT_Mergeable)
                {
                    #region Trades mergeables
                    foreach (TradeKey tradeKey in tmr.IdT_Mergeable.Keys)
                    {
                        // Ecriture de la clé
                        List<Pair<int, decimal>> _lstTrade = tmr.IdT_Mergeable[tradeKey];
                        result.Append(LogDisplayTradeKey(tradeKey));
                        // Ecriture des trades
                        result.Append(LogDisplayTradeMergeable(_lstTrade));
                    }
                    #endregion Trades mergeables
                }
            });
            return result.ToString();
            #endregion Lecture des contextes
        }
        /// <summary>
        /// Affiche dans Sortie la liste des UPLETS DE TRADES A MERGER 
        /// par CLE ET CONTEXT (tri par POIDS)
        /// </summary>
        private string LogDisplayTradeMergeTitle(string pMode, Nullable<int> pNbTrade)
        {
            StringBuilder result = new StringBuilder();
            result.Append("==========================================================================" + Cst.CrLf);
            result.AppendFormat("AUTOMATIC MERGING           : {0}" + Cst.CrLf, pMode.ToUpper());
            result.Append("--------------------------------------------------------------------------" + Cst.CrLf);
            result.AppendFormat("Entity                      : {0}" + Cst.CrLf, LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity));
            result.AppendFormat("Clearing House / Custodian  : {0}" + Cst.CrLf, LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian));
            result.AppendFormat("Market                      : {0}" + Cst.CrLf, LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM));
            result.AppendFormat("Total trades candidates     : {0} / {1}" + Cst.CrLf, (m_DicTradeCandidate == null) ? 0 : m_DicTradeCandidate.Keys.Count(), pNbTrade.HasValue ? pNbTrade.ToString() : "N/A");
            result.Append("==========================================================================" + Cst.CrLf);
            result.Append(Cst.CrLf + Cst.CrLf);
            return result.ToString();
        }
        /// <summary>
        /// Affichage du contexte
        /// </summary>
        /// <param name="pTmr">TradeMergeRule context</param>
        private string LogDisplayTradeMergeRuleContext(TradeMergeRule pTmr, string pMode)
        {
            StringBuilder result = new StringBuilder();
            result.AppendFormat("CONTEXT DATA            : {0}" + Cst.CrLf, LogTools.IdentifierAndId(pTmr.Identifier, pTmr.IdTradeMergeRule));
            result.Append("--------------------------------------------------------------------------" + Cst.CrLf);
            result.AppendFormat("Party                   : {0} {1}" + Cst.CrLf, (pTmr.Context.PartyHasValue ? pTmr.Context.Party.Value.ToString() : "N/A"),
                (pTmr.Context.TypeParty.HasValue ? "(" + pTmr.Context.TypeParty.Value.ToString() + ")" : string.Empty));
            result.AppendFormat("Instr                   : {0} {1}" + Cst.CrLf, (pTmr.Context.InstrHasValue ? pTmr.Context.Instr.Value.ToString() : "N/A"),
                (pTmr.Context.TypeInstr.HasValue ? "(" + pTmr.Context.TypeInstr.Value.ToString() + ")" : string.Empty));
            result.AppendFormat("Contract                : {0} {1}" + Cst.CrLf, (pTmr.Context.ContractHasValue ? pTmr.Context.Contract.Value.ToString() : "N/A"),
                (pTmr.Context.TypeContract.HasValue ? "(" + pTmr.Context.TypeContract.Value.ToString() + ")" : string.Empty));
            result.AppendFormat("Pos.Effect              : {0}" + Cst.CrLf, (pTmr.Context.PositionEffect.HasValue ? pTmr.Context.PositionEffect.Value.ToString() : "N/A"));
            result.AppendFormat("Price                   : {0}" + Cst.CrLf, (pTmr.Context.PriceValue.HasValue ? pTmr.Context.PriceValue.Value.ToString() : "N/A"));
            result.Append(Cst.CrLf);
            // Total trades candidats potentiels
            if ("init" != pMode.ToLower())
            {
                result.AppendFormat("   MATCHING" + Cst.CrLf);
                result.AppendFormat("   --------" + Cst.CrLf);
                result.AppendFormat("   Weight               : {0}" + Cst.CrLf, (int)pTmr.ResultMatching);
                result.AppendFormat("   Trades candidates    : {0} / {1}" + Cst.CrLf, (null != pTmr.IdT_Candidate) ? pTmr.IdT_Candidate.Count : 0, m_DicTradeCandidate.Keys.Count());
                result.AppendFormat("   ");
            }
            else
                result.AppendFormat("Trades candidates       : {0} / {1}" + Cst.CrLf, (null != pTmr.IdT_Candidate) ? pTmr.IdT_Candidate.Count : 0, m_DicTradeCandidate.Keys.Count());
            result.Append(Cst.CrLf + Cst.CrLf);
            return result.ToString();
        }
        /// <summary>
        /// Affichage clé unique pour merge
        /// </summary>
        /// <param name="pTradeKey"></param>
        private string LogDisplayTradeKey(TradeKey pTradeKey)
        {
            StringBuilder result = new StringBuilder();
            result.AppendFormat("   TRADE UNIQUE KEY" + Cst.CrLf);
            result.AppendFormat("   ----------------" + Cst.CrLf);
            result.AppendFormat("   Dealer (book)        : {0} ({1})" + Cst.CrLf, pTradeKey.IdA_Dealer, pTradeKey.IdB_Dealer);
            result.AppendFormat("   Contract             : {0}" + Cst.CrLf, pTradeKey.IdDC);
            result.AppendFormat("   Clearer(book)        : {0} ({1})" + Cst.CrLf, pTradeKey.IdA_Clearer, pTradeKey.IdB_Clearer);
            result.AppendFormat("   Asset                : {0}" + Cst.CrLf, pTradeKey.IdAsset);
            result.AppendFormat("   Date business        : {0}" + Cst.CrLf, DtFunc.DateTimeToStringDateISO(pTradeKey.DtBusiness));
            result.AppendFormat("   Date transac         : {0}" + Cst.CrLf, DtFunc.DateTimeToStringDateISO(pTradeKey.DtTrade));
            result.AppendFormat("   Side                 : {0}" + Cst.CrLf, pTradeKey.Side);
            return result.ToString();
        }
        /// <summary>
        /// Affichage des UPLETs (TRADES A MERGER ENSEMBLE) 
        /// </summary>
        /// <param name="pTradeMergeable"></param>
        /// EG 20171016 [23509] Upd DtExecution
        // EG 20171123 BUG UPDENTRY
        private string LogDisplayTradeMergeable(List<Pair<int, decimal>> pTradeMergeable)
        {
            StringBuilder result = new StringBuilder();
            result.Append(Cst.CrLf);
            result.AppendFormat("      UPLET Trades (Total : {0})" + Cst.CrLf, pTradeMergeable.Count);
            result.AppendFormat("      ----------------------{0}-" + Cst.CrLf, "-".PadRight(pTradeMergeable.Count.ToString().Length, '-'));

            pTradeMergeable.OrderByDescending(trade => trade.First).ToList().ForEach(trade =>
            {
                TradeCandidate _trade = m_DicTradeCandidate[trade.First];
                result.AppendFormat("      Trade : {0} - Timestamp : {1} - PosEffect : {2} - Price : {3}" + Cst.CrLf,
                    LogTools.IdentifierAndId(_trade.TradeIdentifier, trade.First), Tz.Tools.ToString(_trade.DtExecution),
                    //LogTools.IdentifierAndId(_trade.TradeIdentifier, trade.First), DtFunc.DateTimeOffsetUTCToStringISO(_trade.DtExecution),
                    _trade.PositionEffect.ToString(), trade.Second.ToString("0.##########"));
            }
            );
            result.Append(Cst.CrLf);
            return result.ToString();
        }
        #endregion Methods DEBUG

        #region MergePosActionDetBeforeUpdating
        /// <summary>
        /// Merge de nouveaux calculs de la position entre la datatable temporaire (nouveau calcul) et la datatable réelle de POSACTION 
        /// </summary>
        /// <returns></returns>
        // EG 20141224 [20566] 
        private Cst.ErrLevel MergePosActionDetBeforeUpdating()
        {
            if (0 < m_DtPosActionDet_Working.Rows.Count)
            {
                m_DtPosActionDet_Working.PrimaryKey = new DataColumn[]{ m_DtPosActionDet_Working.Columns["IDT_CLOSING"],
                                                                                m_DtPosActionDet_Working.Columns["IDT_BUY"],
                                                                                m_DtPosActionDet_Working.Columns["IDT_SELL"]};
                foreach (DataRow row in m_DtPosActionDet.Rows)
                {
                    decimal qty = Convert.ToDecimal(row["QTY"]);
                    int idT_Buy = Convert.ToInt32(row["IDT_BUY"]);
                    int idT_Sell = Convert.ToInt32(row["IDT_SELL"]);
                    int idT_Closing = Convert.ToInt32(row["IDT_CLOSING"]);
                    object[] pkValues = new object[] { idT_Closing, idT_Buy, idT_Sell };
                    // La ligne dans POSACTIONDET existe toujours dans la table de résultat du nouveau calcul de position
                    DataRow rowWorking = m_DtPosActionDet_Working.Rows.Find(pkValues);
                    if (null == rowWorking)
                    {
                        // La ligne n'est plus présente dans le nouveau calcul
                        // Delete cette ligne dans la table POSACTIONDET 
                        if ((0 != idT_Buy) && (0 != idT_Sell))
                            row.Delete();
                    }
                    else
                    {
                        // EG 20141216 [20566]
                        // La ligne est présente dans le nouveau calcul 
                        //int idT = m_PKGenProcess.mQueue.id;
                        // il y a également modification si la négociation à l'origine de ce traitement est acteur de la position action en cours.
                        //bool isMQueueIdTIsInCurrentPosAction = ((idT_Buy == idT) || (idT_Sell == idT) || (idT_Closing == idT));
                        bool isIdTIsInCurrentPosAction = false;
                        if ((null != m_PosRequest) && m_PosRequest.IdTSpecified)
                            isIdTIsInCurrentPosAction = ((idT_Buy == m_PosRequest.IdT) || (idT_Sell == m_PosRequest.IdT) || (idT_Closing == m_PosRequest.IdT));

                        // il y a également modification si les quantités cloturées changent
                        // EG 20150920 [21374] Int (int32) to Long (Int64) 
                        // EG 20170127 Qty Long To Decimal
                        bool isQuantityChanged = (qty != Convert.ToDecimal(rowWorking["QTY"]));

                        if (isQuantityChanged || isIdTIsInCurrentPosAction)
                        {
                            // on récupère les quantités du nouveau calcul
                            row["QTY"] = rowWorking["QTY"];
                            row["ORIGINALQTY_CLOSED"] = rowWorking["ORIGINALQTY_CLOSED"];
                            row["ORIGINALQTY_CLOSING"] = rowWorking["ORIGINALQTY_CLOSING"];
                            row["POSITIONEFFECT"] = rowWorking["POSITIONEFFECT"];
                        }
                        // Delete cette ligne dans la table TEMPORAIRE
                        rowWorking.Delete();
                    }
                }
                MergePosActionWorkingBeforeUpdating();
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion MergePosActionDetBeforeUpdating
        #region MergePosActionWorkingBeforeUpdating
        private Cst.ErrLevel MergePosActionWorkingBeforeUpdating()
        {
            // Transfert des lignes présente dans m_DtPosActionDet_Working vers m_DtPosActionDet
            // Nouvelles positions
            foreach (DataRow row in m_DtPosActionDet_Working.Rows)
            {
                DataRow newRow = m_DtPosActionDet.NewRow();
                newRow.ItemArray = (object[])row.ItemArray.Clone();
                m_DtPosActionDet.Rows.Add(newRow);
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion MergePosActionWorkingBeforeUpdating

        #region NotionalProrata
        /// <summary>
        /// Retourne le Nominal proraté sur (POC|POT|SPLIT) par prorata quantité (corrigée|transférée|Split) / quantité initiale
        /// </summary>
        /// <param name="pPosRequestTransfer"></param>
        /// <returns></returns>
        /// EG 20150907 [21317] New
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        protected decimal NotionalProrata(decimal pInitialQty, decimal pQty)
        {
            decimal initial = PosKeepingData.NominalValue(pInitialQty);
            decimal action = PosKeepingData.NominalValue(pQty);
            return (action / initial);
        }
        #endregion NotionalProrata

        #region PositionCancelationGen
        /// <summary>
        /// CORRECTION DE POSITION PAR TRADE (REQUESTYPE = POC)
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20151125 [20979] Refactoring GetPositionTransferOrCancellation instead of GetPositionTrade(DtBusiness)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel PositionCancelationGen()
        {
            // EG 20170127 Qty Long To Decimal
            // EG 20240115 [WI808] Update SysNumber Log
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5060), 0,
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("TRADE"), m_PosRequest.IdT)),
                new LogParam(StrFunc.FmtDecimalToCurrentCulture(m_PosRequest.Qty.ToString())),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

            Cst.ErrLevel codeReturn = InitPosKeepingData(m_PosRequest.IdT, Cst.PosRequestAssetQuoteEnum.None, true);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                // EG 20151125 [20979] Refactoring
                DataSet ds = GetPositionTransferOrCancellation(null, DtBusiness, PosKeepingData.Trade.IdT);
                if (null != ds)
                {
                    m_DtPosition = ds.Tables[0];
                    m_DtPosActionDet = ds.Tables[1];
                    m_DtPosition.PrimaryKey = new DataColumn[] { m_DtPosition.Columns["IDT"] };
                    // EG 20240115 [WI808] Harmonisation et réunification des méthodes
                    //IPosRequestDetCorrection detail = (IPosRequestDetCorrection)m_PosRequest.DetailBase;
                    //codeReturn = PosActionDetCalculation(detail.AvailableQty, m_PosRequest.Qty, detail.PaymentFees);
                    TradeCancellationData tradeData = new TradeCancellationData(this, m_PosRequest, PosKeepingData, (m_TradeLibrary, m_DtPosition, m_DtPosActionDet));
                    codeReturn = tradeData.PosActionDetCalculation(null);
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        int idPA = 0;
                        codeReturn = PosKeep_Updating(m_PosRequest.NbTokenIdE, ref idPA);
                    }
                    // Valuation
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        // EG 20240115 [WI808] Harmonisation et réunification des méthodes
                        (int id, string identifier) = (PosKeepingData.Trade.IdT, PosKeepingData.Trade.Identifier);
                        codeReturn = ValuationAmountGen(null, (id, identifier), PosKeepingData.AssetInfo, true);
                    }
                }
            }
            return codeReturn;
        }
        #endregion PositionCancelationGen
        #region PositionTransferGen
        /// <summary>
        /// TRANSFERT DE POSITION PAR TRADE (REQUESTYPE = POT)
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20141224 [20566] 
        // EG 20150306 [20838] 
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20211012 [XXXXX] Setting Quote avant appel à SpheresEventsVal
        private Cst.ErrLevel PositionTransferGen()
        {
            IDbTransaction dbTransaction = null;
            m_TemplateDataDocumentTrade = new Hashtable();
            bool isException = false;
            try
            {
                Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
                Cst.ErrLevel codeReturnValuation = Cst.ErrLevel.UNDEFINED;

                List<Pair<int, string>> lstIdTForValuation = new List<Pair<int, string>>();
                int idPA = 0;
                // INSERTION LOG
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5054), 0,
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Trade, m_PosRequest.IdT)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                    new LogParam(m_PosRequest.Qty)));

                IPosRequestTransfer posRequestTransfer = (IPosRequestTransfer)m_PosRequest;
                IPosRequestDetTransfer detail = (IPosRequestDetTransfer)posRequestTransfer.Detail;
                // Initialisation du trade remplacant
                // S'il existe (transfert unitaire sur une trade (le trade remplacant existe déjà)
                // Sinon il sera créé durant le traitement (mode transfert de masse)
                Pair<int, string> _tradeTransferResult = null;
                if (posRequestTransfer.Detail.IdTReplaceSpecified)
                    _tradeTransferResult = new Pair<int, string>(detail.IdTReplace, detail.Trade_IdentifierReplaceSpecified? detail.Trade_IdentifierReplace:string.Empty);

                codeReturn = InitPosKeepingData(m_MasterPosRequest.IdT, Cst.PosRequestAssetQuoteEnum.None, false);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {

                    string dealer = string.Empty;
                    if (detail.DealerTargetSpecified)
                        dealer = LogTools.IdentifierAndId(detail.DealerTarget.PartyName, detail.DealerTarget.OTCmlId);
                    if (detail.DealerBookIdTargetSpecified)
                        dealer += " - " + detail.DealerBookIdTarget.BookName;

                    string clearer = string.Empty;
                    if (detail.ClearerTargetSpecified)
                        clearer = LogTools.IdentifierAndId(detail.ClearerTarget.PartyName, detail.ClearerTarget.OTCmlId);
                    if (detail.ClearerBookIdTargetSpecified)
                        clearer += " -" + detail.ClearerBookIdTarget.BookName;


                    dbTransaction = DataHelper.BeginTran(CS);
                    if (posRequestTransfer.Detail.InitialQty == posRequestTransfer.Qty)
                    {
                        // INSERTION LOG
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5052), 0,
                            new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Trade, m_PosRequest.IdT)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                            new LogParam(posRequestTransfer.Qty)));

                        #region UNCLEARING (DECOMPENSATION SI TRANSFERT DE LA POSITION INITIALE ET NON JOUR)
                        //  EG 20170412 [23081] New Add parameter idPR_Parent = m_PosRequest.idPR
                        codeReturn = UnclearingAllTrade(dbTransaction, posRequestTransfer.IdT, lstIdTForValuation, m_PosRequest.IdPR);
                        #endregion UNCLEARING (DECOMPENSATION SI TRANSFERT DE LA POSITION INITIALE ET NON JOUR)
                    }

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        #region TRANSFER (POSACTION/POSACTIONDET/EVENEMENT sur le TRADE TRANSFEREE)

                        // INSERTION LOG
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5055), 0,
                            new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("TRADE"), m_PosRequest.IdT)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                            new LogParam(posRequestTransfer.Qty),
                            new LogParam(dealer),
                            new LogParam(clearer)));

                        codeReturn = PosActionDetTransfer(dbTransaction, ref idPA);
                        #endregion TRANSFER (POSACTION/POSACTIONDET/EVENEMENT sur le TRADE TRANSFEREE)
                    }

                    if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null == _tradeTransferResult))
                    {
                        #region CREATION DU TRADE REMPLACANT
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5066), 0,
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                            new LogParam(posRequestTransfer.Qty),
                            new LogParam(dealer),
                            new LogParam(clearer))); 
                        
                        codeReturn = CreateTradeTransfer(dbTransaction, ref _tradeTransferResult);
                        #endregion CREATION DU TRADE REMPLACANT
                    }

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        if ((0 < idPA) && (null != _tradeTransferResult))
                        {
                            #region TradeLink
                            string sqlUpdate = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRADELINK + SQLCst.SET + Cst.CrLf;
                            sqlUpdate += "IDDATA=" + idPA.ToString();
                            sqlUpdate += ", IDDATAIDENT=" + DataHelper.SQLString(Cst.OTCml_TBL.POSACTION.ToString()) + Cst.CrLf;
                            sqlUpdate += SQLCst.WHERE + "IDT_B=" + posRequestTransfer.IdT.ToString() + SQLCst.AND;
                            sqlUpdate += "IDT_A=" + _tradeTransferResult.First.ToString();
                            DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, sqlUpdate);
                            #endregion TradeLink

                            #region TradeLinkPosRequest
                            // EG [21465] replace m_PosRequest.idPR by new int[]{m_PosRequest.idPR}
                            codeReturn = InsertTradeLinkPosRequest(dbTransaction, new int[]{m_PosRequest.IdPR}, posRequestTransfer.RequestType, _tradeTransferResult.First);
                            #endregion TradeLinkPosRequest
                        }
                    }

                    if ((null != dbTransaction) && (Cst.ErrLevel.SUCCESS == codeReturn))
                    {
                        DataHelper.CommitTran(dbTransaction);
                        // EG 20141224 [20566][20601] Si non décompensation on ajoute le trade transféré pour recalcul 
                        if (0 == lstIdTForValuation.Count)
                            lstIdTForValuation.Add(new Pair<int, string>(m_PosRequest.IdT, m_PosRequest.Identifiers.Trade));

                        lstIdTForValuation.ForEach(item =>
                            {
                                //  EG 20170412 [23081] New Add parameter pDbTransaction = null
                                // EG 20240115 [WI808] Harmonisation et réunification des méthodes
                                codeReturnValuation = ValuationAmountGen(null, (item.First, item.Second), PosKeepingData.AssetInfo, true);
                                if (Cst.ErrLevel.SUCCESS != codeReturnValuation)
                                    codeReturn = codeReturnValuation;
                            }
                       );
                    }
                    else
                        isException = true;

                    #region EVENTS (GENERATION ET VALORISATION DES EVENEMENTS SUR LA REMPLACANTE / CHANGEMENT STATUS)
                    if ((Cst.ErrLevel.SUCCESS == codeReturn) || (Cst.ErrLevel.FAILUREWARNING == codeReturn))
                    {
                        string logTradeIdentifier = LogTools.IdentifierAndId(_tradeTransferResult.Second, _tradeTransferResult.First);
                        if (0 < _tradeTransferResult.First)
                        {
                            // INSERTION LOG
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5056), 0, new LogParam(logTradeIdentifier)));

                            //
                            MQueueAttributes mQueueAttributes = new MQueueAttributes()
                            {
                                connectionString = CS,
                                id = _tradeTransferResult.First,
                                idInfo = new IdInfo()
                                {
                                    id = _tradeTransferResult.First,
                                    idInfos = new DictionaryEntry[]{
                                                new DictionaryEntry("ident", "TRADE"),
                                                new DictionaryEntry("identifier", _tradeTransferResult.Second),
                                                new DictionaryEntry("GPRODUCT", GProduct)}
                                },
                                requester = Queue.header.requester
                            };
                            MQueueBase mQueue = new EventsGenMQueue(mQueueAttributes);
                            
                            ProcessState processState = New_EventsGenAPI.ExecuteSlaveCall((EventsGenMQueue)mQueue, null, m_PKGenProcess, false);
                            codeReturn = processState.CodeReturn;
                            if (Cst.ErrLevel.SUCCESS == codeReturn)
                            {
                                // EG 20211012 [XXXXX] Setting Quote avant appel à SpheresEventsVal
                                SystemMSGInfo errReadOfficialClose = null;
                                Quote quote = m_PKGenProcess.ProcessCacheContainer.GetQuote(PosKeepingData.IdAsset, m_MasterPosRequest.DtBusiness,
                                PosKeepingData.Asset.identifier, QuotationSideEnum.OfficialClose, PosKeepingData.Asset.UnderlyingAsset.Value, new KeyQuoteAdditional(), ref errReadOfficialClose) as Quote;
                                if (null != quote)
                                    quote.action = DataRowState.Added.ToString();

                                mQueue = new EventsValMQueue(mQueueAttributes, quote);

                                
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 600), 1, new LogParam(logTradeIdentifier)));

                                processState = New_EventsValAPI.ExecuteSlaveCall(null, (EventsValMQueue)mQueue, m_PKGenProcess, m_PKGenProcess.ProcessCacheContainer, false, false);
                                codeReturn = processState.CodeReturn;
                                // EG 20150305
                                if ((Cst.ErrLevel.SUCCESS == codeReturn) || (Cst.ErrLevel.FAILUREWARNING == codeReturn))
                                {
                                    codeReturn = UpdateTradeStUsedBy(CS, _tradeTransferResult.First, Cst.StatusUsedBy.REGULAR, null);
                                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                                        codeReturn = UpdateTradeStUsedBy(CS, posRequestTransfer.IdT, Cst.StatusUsedBy.REGULAR, null);
                                    // EG 2050305
                                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                                        codeReturn = codeReturnValuation;
                                }
                                else
                                {
                                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5154), 0, new LogParam(logTradeIdentifier)));
                                    codeReturn = Cst.ErrLevel.FAILURE;
                                }
                            }
                            else
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                
                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5153), 0, new LogParam(logTradeIdentifier)));

                                codeReturn = Cst.ErrLevel.FAILURE;
                            }
                        }
                        else
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5152), 0, new LogParam(logTradeIdentifier)));

                            codeReturn = Cst.ErrLevel.DATANOTFOUND;
                        }
                    }
                    #endregion EVENTS (GENERATION ET VALORISATION DES EVENEMENTS SUR LA REMPLACANTE / CHANGEMENT STATUS)
                }
                return codeReturn;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                {
                    if (isException)
                        DataHelper.RollbackTran(dbTransaction);
                    dbTransaction.Dispose();
                }
            }
        }
        #endregion PositionTransferGen
        #region PosActionDetTransfer
        // EG 20151125 [20979] Refactoring : GetPositionTransferOrCancellation instead of GetPositionTransfer
        private Cst.ErrLevel PosActionDetTransfer(IDbTransaction pDbTransaction, ref int pIdPA)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            m_PosRequest = RestoreMasterRequest;
            // EG 20151125 [20979] Refactoring
            DataSet ds = GetPositionTransferOrCancellation(pDbTransaction, DtBusiness, m_PosRequest.IdT);
            if (null != ds)
            {
                m_DtPosition = ds.Tables[0];
                m_DtPosActionDet = ds.Tables[1];
                m_DtPosition.PrimaryKey = new DataColumn[] { m_DtPosition.Columns["IDT"] };
                // EG 20240115 [WI808] Harmonisation et réunification des méthodes
                //IPosRequestTransfer posRequestTransfer = (IPosRequestTransfer)m_PosRequest;
                //IPosRequestDetTransfer detail = (IPosRequestDetTransfer)posRequestTransfer.Detail;
                //IPayment[] _paymentFees = null;
                //if (detail.IsReversalFeesSpecified)
                //{
                //    SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CS, m_PosRequest.IdT);
                //    SpheresIdentification identification = new SpheresIdentification(sqlTrade.Identifier, sqlTrade.DisplayName, sqlTrade.Description, sqlTrade.ExtlLink)
                //    {
                //        OTCmlId = sqlTrade.Id
                //    };

                //    TradePositionTransfer positionTransfer = new TradePositionTransfer(identification, detail.InitialQty, m_PosRequest.Qty, DtBusiness, null);
                //    positionTransfer.isFeeRestitution.BoolValue = detail.IsReversalFees;
                //    positionTransfer.CalcFeeRestitution(CS);

                //    detail.SetPaymentFees(positionTransfer.otherPartyPayment);

                //    _paymentFees = detail.PaymentFees;
                //}
                //else if (detail.IdTReplaceSpecified)
                //    _paymentFees = detail.PaymentFees;

                //codeReturn = PosActionDetCalculation(detail.PositionQty, m_PosRequest.Qty, _paymentFees);
                TradeTransferData tradeData = new TradeTransferData(this, m_PosRequest, PosKeepingData, (m_TradeLibrary, m_DtPosition, m_DtPosActionDet));
                codeReturn = tradeData.PosActionDetCalculation(pDbTransaction);

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = PosKeep_Updating(pDbTransaction, m_PosRequest.NbTokenIdE, ref pIdPA);
            }
            return codeReturn;
        }
        #endregion PosActionDetTransfer
        #region PosRequestPositionSetIdentifiers
        /// <summary>
        /// Alimente les identifiers des éléments suivants d'un POSREQUEST
        /// <para>- market</para>
        /// <para>- instrument</para>
        /// <para>- asset</para>
        /// <para>- dealer and book dealer</para>
        /// <para>- clearer and book clearer</para>
        /// </summary>
        /// FI 20130917 [18953] Add Method
        /// EG 20141224 [20566]
        protected void PosRequestPositionSetIdentifiers(string pCS, IPosRequest pPosRequest)
        {
            if (null == pPosRequest)
                throw new ArgumentNullException("pPosRequest is null");
            /*
            string query = @"
            select  
                m.SHORTIDENTIFIER as M_IDENTIFIER, 
                i.IDENTIFIER as I_IDENTIFIER, 
                asset.IDENTIFIER as A_IDENTIFIER,
                dealer.IDENTIFIER as DEALER_IDENTIFIER,
                bdealer.IDENTIFIER as BDEALER_IDENTIFIER,
                clearer.IDENTIFIER as CLEARER_IDENTIFIER,
                bclearer.IDENTIFIER as BCLEARER_IDENTIFIER
            from dbo.POSREQUEST  pr
            inner join dbo.VW_ASSET_ETD_EXPANDED asset on asset.IDASSET = pr.IDASSET
            inner join dbo.INSTRUMENT i on i.IDI = asset.IDI
            inner join dbo.VW_MARKET_IDENTIFIER m on m.IDM = asset.IDM 
            left outer join dbo.ACTOR dealer on dealer.IDA = pr.IDA_DEALER
            left outer join dbo.BOOK bdealer on bdealer.IDB = pr.IDB_DEALER
            inner join dbo.ACTOR clearer on clearer.IDA = pr.IDA_CLEARER
            left outer join dbo.BOOK bclearer on bclearer.IDB = pr.IDB_CLEARER
            where IDPR=@IDPR";
            */

            string query = @"select  mk.SHORTIDENTIFIER as M_IDENTIFIER, ns.IDENTIFIER as NS_IDENTIFIER, asset.IDENTIFIER as A_IDENTIFIER,
            dealer.IDENTIFIER as DEALER_IDENTIFIER, bdealer.IDENTIFIER as BDEALER_IDENTIFIER, clearer.IDENTIFIER as CLEARER_IDENTIFIER,
            bclearer.IDENTIFIER as BCLEARER_IDENTIFIER
            from dbo.POSREQUEST  pr
            inner join dbo.VW_INSTR_PRODUCT ns on (ns.IDI = pr.IDI)
            inner join dbo.VW_ASSET asset on (asset.IDASSET = pr.IDASSET) and (asset.ASSETCATEGORY = ns.ASSETCATEGORY)
            inner join dbo.VW_MARKET_IDENTIFIER mk on (mk.IDM = asset.IDM)
            left outer join dbo.ACTOR dealer on (dealer.IDA = pr.IDA_DEALER)
            left outer join dbo.BOOK bdealer on (bdealer.IDB = pr.IDB_DEALER)
            inner join dbo.ACTOR clearer on (clearer.IDA = pr.IDA_CLEARER)
            left outer join dbo.BOOK bclearer on (bclearer.IDB = pr.IDB_CLEARER)
            where IDPR=@IDPR";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDPR), pPosRequest.IdPR);

            QueryParameters qry = new QueryParameters(pCS, query, dp);

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
            if (false == ArrFunc.IsFilled(dt.Rows))
                throw new Exception("Error on loading POSREQUEST for identifiers, no data found");

            DataRow dr = dt.Rows[0];

            string market = Convert.ToString(dr["M_IDENTIFIER"]);
            string instrument = Convert.ToString(dr["NS_IDENTIFIER"]);
            string asset = Convert.ToString(dr["A_IDENTIFIER"]);
            string dealer = Convert.IsDBNull(dr["DEALER_IDENTIFIER"]) ? null : Convert.ToString(dr["DEALER_IDENTIFIER"]);
            string bookDealer = Convert.IsDBNull(dr["BDEALER_IDENTIFIER"]) ? null : Convert.ToString(dr["BDEALER_IDENTIFIER"]);
            string clearer = Convert.ToString(dr["CLEARER_IDENTIFIER"]);
            string bookClearer = Convert.IsDBNull(dr["BCLEARER_IDENTIFIER"]) ? null : Convert.ToString(dr["BCLEARER_IDENTIFIER"]);

            pPosRequest.SetIdentifiers(market, instrument, asset, dealer, bookDealer, clearer, bookClearer);
        }
        #endregion PosRequestPositionSetIdentifiers

        #region RemoveAllocationGen
        /// <summary>
        /// ANNULATION D'ALLOCATION (REQUESTYPE = RMVALLOC)
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        private Cst.ErrLevel RemoveAllocationGen()
        {
            List<Pair<int, string>> _lstIdtForValuation = null;
            //  EG 20170412 [23081] New Add parameter pIdPR_Parent = m_MasterPosRequest.idPR
            return RemoveAllocationGen(null, ref _lstIdtForValuation, m_MasterPosRequest.IdPR);
        }
        /// <summary>
        /// ANNULATION D'ALLOCATION (REQUESTYPE = RMVALLOC)
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pLstIdtForValuation">Renseigné uniquement si pDbTransaction is not null</param>
        /// <returns>Cst.ErrLevel</returns>
        //  EG 20170412 [23081] Add parameter : pIdPR_Parent
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel RemoveAllocationGen(IDbTransaction pDbTransaction, ref List<Pair<int, string>> pLstIdtForValuation, int pIdPR_Parent)
        {
            IDbTransaction dbTransaction = pDbTransaction;
            bool isException = false;
            try
            {
                Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
                List<Pair<int, string>> lstIdtForValuation = new List<Pair<int, string>>();

                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5051), 0,
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("TRADE"), m_PosRequest.IdT)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
                
                /// EG 20131127 [19254/19239] m_MasterPosRequest.idT -> m_PosRequest.idT
                codeReturn = InitPosKeepingData(m_PosRequest.IdT, Cst.PosRequestAssetQuoteEnum.None, false);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    IPosRequest posRequest = (IPosRequest)m_PosRequest;
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    decimal initialQty = 0;

                    if (null == pDbTransaction)
                    {
                        // PL 20180312 WARNING: Use Read Commited !
                        //dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadUncommitted);
                        dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadCommitted);
                    }

                    IPosRequestRemoveAlloc posRequestRemoveAlloc = (IPosRequestRemoveAlloc)m_PosRequest;
                    initialQty = posRequestRemoveAlloc.Detail.InitialQty;

                    // INSERTION LOG
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5052), 0,
                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("TRADE"), m_PosRequest.IdT)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                        new LogParam(initialQty)));
                
                    //Décompensation éventuelles de toutes les quantités compensées
                    //  EG 20170412 [23081] New Add parameter pIdPR_Parent
                    codeReturn = UnclearingAllTrade(dbTransaction, posRequest.IdT, lstIdtForValuation, pIdPR_Parent);

                    // EG 20131127 [19254/19239]
                    if ((null != dbTransaction) && (Cst.ErrLevel.SUCCESS == codeReturn))
                    {
                        if (null != pDbTransaction)
                        {
                            pLstIdtForValuation = lstIdtForValuation;
                        }
                        else
                        {
                            DataHelper.CommitTran(dbTransaction);
                            lstIdtForValuation.ForEach(item =>
                            {
                                bool isInstrumentFungible = false;
                                SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CS, item.First);
                                if (sqlTrade.LoadTable(new string[] { "TRADE.IDT", "IDENTIFIER", "IDI" }))
                                {
                                    SQL_Instrument sqlInstrument = new SQL_Instrument(CSTools.SetCacheOn(CS), sqlTrade.IdI);
                                    if (sqlInstrument.LoadTable(new string[] { "IDI", "IDP", "IDENTIFIER", "FUNGIBILITYMODE" }))
                                        isInstrumentFungible = sqlInstrument.IsFungible;
                                }

                                // RD 20230413 [26296] Call ValuationAmountGen only for Fungible instruments                                    
                                if (isInstrumentFungible)
                                    // EG 20240115 [WI808] Harmonisation et réunification des méthodes
                                    codeReturn = ValuationAmountGen(null, (item.First, item.Second), PosKeepingData.AssetInfo, true);
                            }
                            );

                            #region ANNULATION
                            if (Cst.ErrLevel.SUCCESS == codeReturn)
                            {
                                string identifier = Queue.GetStringValueIdInfoByKey("TRADE");
                                if (posRequestRemoveAlloc.IdentifiersSpecified)
                                    identifier = posRequestRemoveAlloc.Identifiers.Trade;
                                codeReturn = RemoveTrade(posRequest.IdT, identifier, posRequest.DtBusiness, posRequest.Notes);
                            }
                            #endregion ANNULATION
                        }
                    }
                    else
                    {
                        isException = true;
                    }
                }
                else if (Cst.ErrLevel.DATAIGNORE == codeReturn)
                {
                    UpdateTradeStUsedBy(CS, m_PosRequest.IdT, Cst.StatusUsedBy.REGULAR, null);
                }

                return codeReturn;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null == pDbTransaction) && (null != dbTransaction))
                {
                    if (isException)
                    {
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    dbTransaction.Dispose();
                }
            }
        }
        #endregion RemoveAllocationGen
        #region RemoveCAExecutedGen
        /// <summary>
        /// Méthode principale d'annulation d'un traitement de corporate action
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Message de type PosKeepingRequestMQueue avec REQUESTYPE = RMVCAEXECUTED</para>
        ///<para>  ● Pour un couple CSS/IDM/IDCE donné</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>/// 
        ///</summary>
        /// <returns>Cst.ErrLevel</returns>
        private Cst.ErrLevel RemoveCAExecutedGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                // STEP 0 : INITIALIZE / IN PROGRESS
                m_MasterPosRequest.StatusSpecified = true;
                m_MasterPosRequest.Status = ProcessStateTools.StatusProgressEnum;
                
                //PosKeepingTools.UpdatePosRequest(CS, null, m_MasterPosRequest.idPR, m_MasterPosRequest, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, null);
                PosKeepingTools.UpdatePosRequest(CS, null, m_MasterPosRequest.IdPR, m_MasterPosRequest, m_PKGenProcess.Session.IdA, IdProcess, null);

                bool isSimul = m_PKGenProcess.MQueue.GetBoolValueParameterById(MQueueBase.PARAM_ISSIMUL);
                bool isKeepHistory = m_PKGenProcess.MQueue.GetBoolValueParameterById(MQueueBase.PARAM_ISKEEPHISTORY);

                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdCE", DbType.Int32), m_MasterPosRequest.IdCE);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdTRK_L", DbType.Int32), m_PKGenProcess.Tracker.IdTRK_L);
                
                //parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdProcess_L", DbType.Int32), m_PKGenProcess.processLog.header.IdProcess);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdProcess_L", DbType.Int32), m_PKGenProcess.IdProcess);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIsSimul", DbType.Boolean), isSimul);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIsKeepHistory", DbType.Boolean), isKeepHistory);

                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pCodeReturn", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
                parameters["pCodeReturn"].Direction = ParameterDirection.Output;
                IDbDataParameter[] arParameters = parameters.GetArrayDbParameter();

                int retUP = DataHelper.ExecuteNonQuery(CS, CommandType.StoredProcedure, SQLCst.DBO.Trim() + "UP_REMOVE_CA_EXECUTED", arParameters);
                if (0 != retUP)
                {
                    ProcessStateTools.StatusEnum status = ProcessStateTools.ParseStatus(parameters["pCodeReturn"].Value.ToString());
                    m_PKGenProcess.ProcessState.SetErrorWarning(status);
                    switch (status)
                    {
                        case ProcessStateTools.StatusEnum.ERROR:
                            codeReturn = Cst.ErrLevel.FAILURE;
                            break;
                        case ProcessStateTools.StatusEnum.NA:
                            codeReturn = Cst.ErrLevel.NOTHINGTODO;
                            break;
                    }
                }
                return codeReturn;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion RemoveCAExecutedGen
        #region RemoveEODGen
        /// <summary>
        /// Méthode principale d'annulation d'un traitement de fin de journée
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Message de type PosKeepingRequestMQueue avec REQUESTYPE = REMOVEEOD</para>
        ///<para>  ● Pour un couple ENTITE/CSS donné (IDEM de la table ENTITYMARKET)</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>/// 
        ///</summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20150630 REMOVEEOD pour OTC
        /// EG 20150630 REMOVEEOD pour ORACLE
        private Cst.ErrLevel RemoveEODGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                // STEP 0 : INITIALIZE / IN PROGRESS
                m_MasterPosRequest.StatusSpecified = true;
                m_MasterPosRequest.Status = ProcessStateTools.StatusProgressEnum;
                
                //PosKeepingTools.UpdatePosRequest(CS, null, m_MasterPosRequest.idPR, m_MasterPosRequest, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, null);
                PosKeepingTools.UpdatePosRequest(CS, null, m_MasterPosRequest.IdPR, m_MasterPosRequest, m_PKGenProcess.Session.IdA, IdProcess, null);

                bool isSimul = m_PKGenProcess.MQueue.GetBoolValueParameterById(MQueueBase.PARAM_ISSIMUL);
                bool isKeepHistory = m_PKGenProcess.MQueue.GetBoolValueParameterById(MQueueBase.PARAM_ISKEEPHISTORY);

                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdA_Entity", DbType.Int32), m_MasterPosRequest.IdA_Entity);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdA_CSSCUSTODIAN", DbType.Int32), m_MasterPosRequest.IdA_CssCustodian);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdEM", DbType.Int32), (m_MasterPosRequest.IdEMSpecified ? m_MasterPosRequest.IdEM : Convert.DBNull));
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdTRK_L", DbType.Int32), m_PKGenProcess.Tracker.IdTRK_L);
                
                //parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdProcess_L", DbType.Int32), m_PKGenProcess.processLog.header.IdProcess);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdProcess_L", DbType.Int32), m_PKGenProcess.IdProcess);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIsSimul", DbType.Boolean), isSimul);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIsKeepHistory", DbType.Boolean), isKeepHistory);

                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pCodeReturn", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
                parameters["pCodeReturn"].Direction = ParameterDirection.Output;
                IDbDataParameter[] arParameters = parameters.GetArrayDbParameter();

                // EG 20150320 (POC] New Gestion ETD|OTC|MTM
                string storedProcedure = StoreProcedure_RemoveEOD;
                if (StrFunc.IsFilled(storedProcedure))
                {
                    int retUP = DataHelper.ExecuteNonQuery(CS, CommandType.StoredProcedure, SQLCst.DBO.Trim() + storedProcedure, arParameters);
                    if (0 != retUP)
                    {
                        ProcessStateTools.StatusEnum status = ProcessStateTools.ParseStatus(parameters["pCodeReturn"].Value.ToString());
                        m_PKGenProcess.ProcessState.SetErrorWarning(status);
                        switch (status)
                        {
                            case ProcessStateTools.StatusEnum.ERROR:
                                codeReturn = Cst.ErrLevel.FAILURE;
                                break;
                            case ProcessStateTools.StatusEnum.NA:
                                codeReturn = Cst.ErrLevel.NOTHINGTODO;
                                break;
                        }
                    }
                }
                else
                    throw new NotImplementedException("RemoveEODGen is not managed, please contact EFS");

                return codeReturn;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion RemoveEODGen
        #region ResetTradeMergeBeforeProcessing
        /// <summary>
        /// Reset des trades candidats sur un contexte de MERGE avant TRAITEMENT
        /// . IdT_Candidate et IdT_Mergeable
        /// <para>Retourne Cst.ErrLevel.NOTHINGTODO ou Cst.ErrLevel.SUCCESS</para>
        /// </summary>
        /// <returns></returns>
        protected Cst.ErrLevel ResetTradeMergeBeforeProcessing()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;
            if ((null != m_LstTradeMergeRule) && (0 < m_LstTradeMergeRule.Count))
            {
                m_LstTradeMergeRule.ForEach(tmr =>
                {
                    if (null != tmr.IdT_Candidate)
                        tmr.IdT_Candidate.Clear();
                    if (null != tmr.IdT_Mergeable)
                        tmr.IdT_Mergeable.Clear();
                }
                );
                codeReturn = Cst.ErrLevel.SUCCESS;
            }
            return codeReturn;
        }
        #endregion ResetTradeMergeBeforeProcessing

        #region SaveResultTradeMergeToFileLog
        /// <summary>
        /// Création et écriture dans les fichiers LOG
        /// </summary>
        /// <param name="pSuffix">Extension du fichier LOG (TradeMerge_IDEM_DTBUSINESS.pSuffix.Log)</param>
        /// <param name="pNbTrade">Nombre de trades candidats</param>
        private void SaveResultTradeMergeToFileLog(string pSuffix, Nullable<int> pNbTrade)
        {
            string _fileName = "TradeMerge" + "_" + m_MarketPosRequest.IdEM + "_" + DtFunc.DateTimeToStringyyyyMMdd(DtBusiness) + "." + pSuffix.ToLower() + ".log";
            string _fileNameFull = m_PKGenProcess.Session.MapTemporaryPath(_fileName, EFS.Common.AppSession.AddFolderSessionId.True);

            StringBuilder result = new StringBuilder();
            result.Append(LogDisplayTradeMergeResult(pSuffix, pNbTrade));
            FileTools.WriteStringToFile(result.ToString(), _fileNameFull);
            try
            {
                byte[] data = FileTools.ReadFileToBytes(_fileNameFull);
                LogTools.AddAttachedDoc(CS, m_PKGenProcess.IdProcess, m_PKGenProcess.Session.IdA, data, _fileName, "txt");
            }
            catch (Exception ex)
            {
                string logMessage;
                if (ex.Message.Contains("OutOfMemory"))
                    logMessage = "The XML file was not written in the attachments of the Log, because of its big volume" + Cst.CrLf + "[File:" + _fileNameFull + "]";
                else
                    logMessage = "The XML file was not written in the attachments of the Log" + Cst.CrLf + "[File:" + _fileNameFull + "]";

                throw new Exception(logMessage, ex);
            }

        }
        #endregion SaveResultTradeMergeToFileLog
        #region SearchPosKeepingData
        /// <summary>
        /// Retourne les données nécessaires
        /// </summary>
        /// <param name="pId">Représente un trade ou un asset</param>
        /// <param name="pPosRequestAssetQuote"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataHelper.ExecuteReader
        // EG 20180307 [23769] Gestion Asynchrone
        public IDataReader SearchPosKeepingData(IDbTransaction pDbTransaction, int pId, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.ID), pId);

            string sqlSelect = GetQueryPosKeepingData(pPosRequestAssetQuote);
            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
            return DataHelper.ExecuteReader(CS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion SearchPosKeepingData
        #region SearchPosKeepingDataByAsset
        // EG 20180205 [23769] Upd DataHelper.ExecuteReader
        private IDataReader SearchPosKeepingDataByAsset(IDbTransaction pDbTransaction, 
            Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, int pIdAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.ID), pIdAsset);

            string sqlSelect = GetQueryPosKeepingDataAsset(pUnderlyingAsset,pPosRequestAssetQuote);
            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
            return DataHelper.ExecuteReader(CS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion SearchPosKeepingDataByAsset
        #region SearchPosKeepingDataByTrade
        // EG 20180205 [23769] Upd DataHelper.ExecuteReader
        // EG 20180307 [23769] Gestion Asynchrone
        public IDataReader SearchPosKeepingDataByTrade(IDbTransaction pDbTransaction, 
            Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, int pIdT, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.ID), pIdT);

            string sqlSelect = GetQueryPosKeepingDataTrade(pUnderlyingAsset, pPosRequestAssetQuote);
            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
            return DataHelper.ExecuteReader(CS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion SearchPosKeepingDataByTrade
        #region SetActorTransfer
        // EG 20210315 [24683] Ajout dbTransaction sur SetActorTransfer
        private void SetActorTransfer(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocumentContainer, IParty pParty, int pIdA, int pIdB)
        {
            IPartyTradeInformation _partyTradeInformation = pDataDocumentContainer.GetPartyTradeInformation(pParty.Id);
            IPartyTradeIdentifier _partyTradeIdentifier = pDataDocumentContainer.GetPartyTradeIdentifier(pParty.Id);

            //Suppresson du tradeside
            pDataDocumentContainer.RemoveTradeSide(pParty);

            //Suppression des éventuels Traders et Sales du dealer
            if (null != _partyTradeInformation)
            {
                _partyTradeInformation.TraderSpecified = false;
                _partyTradeInformation.SalesSpecified = false;
            }

            //Mise à jour avec le nouvel acteur (DEALER/CLEARER|CUSTODIAN)
            SQL_Actor sql_Actor = new SQL_Actor(CS, pIdA)
            {
                DbTransaction = pDbTransaction
            };
            SQL_Book sql_Book = new SQL_Book(CS, pIdB)
            {
                DbTransaction = pDbTransaction
            };
            Tools.SetBookId(_partyTradeIdentifier.BookId, sql_Book);
            Tools.SetParty(pParty, sql_Actor, true);

            if (null != _partyTradeInformation)
                _partyTradeInformation.PartyReference = pParty.Id;
            _partyTradeIdentifier.PartyReference.HRef = pParty.Id;

            //Génération du tradeside du nouveL acteur
            pDataDocumentContainer.SetTradeSide(CS, pDbTransaction, pParty);
        }
        #endregion SetActorTransfer
        #region SetNotification
        /// <summary>
        /// Active la messagerie pour l'acteur Dealer
        /// </summary>
        /// <param name="tradeInput"></param>
        private void SetNotification(TradeInput tradeInput)
        {
            DataDocumentContainer dataDocument = tradeInput.DataDocument;
            RptSideProductContainer rptSide = dataDocument.CurrentProduct.RptSide(CS, true);
            if (null != rptSide)
            {
                IFixParty dealer = rptSide.GetDealer();
                if (null == dealer)
                    throw new Exception(StrFunc.AppendFormat("dealer is not found"));
                IParty partyDealer = dataDocument.GetParty(dealer.PartyId.href);
                //
                IFixParty clearingParty;
                if (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                    clearingParty = rptSide.GetBuyerSeller(SideEnum.Buy);
                else if (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.SELLER))
                    clearingParty = rptSide.GetBuyerSeller(SideEnum.Sell);
                else
                    throw new Exception(StrFunc.AppendFormat("Dealer [PartyId:{0}] is not buyer ou seller", dealer.PartyId));
                IParty partyClearingParty = dataDocument.GetParty(clearingParty.PartyId.href);
                //
                tradeInput.TradeNotification.PartyNotification[0].IdActor = partyDealer.OTCmlId;
                tradeInput.TradeNotification.PartyNotification[1].IdActor = partyClearingParty.OTCmlId;
                //
                ActorNotification dealerNotification = tradeInput.TradeNotification.GetActorNotification(partyDealer.OTCmlId);
                dealerNotification.SetConfirmation(true);
            }
        }
        #endregion SetNotification
        #region SetPosKeepingData
        /// <summary>
        /// Affecte le membre PosKeepingData
        /// </summary>
        /// <param name="pDr"></param>
        /// <param name="isQuoteCanBeReaded"></param>
        /// <returns></returns>
        /// FI 20130313 [18467] call SetPosKeepingMarket Method 
        /// FI 20130404 [18467] Modification de la clef d'accès au cache (utilisation des méthodes CacheAssetFind, CacheAssetAdd)
        /// EG 20150624 [21151] Add pPosRequestAssetQuote parameter
        /// EG 20160302 [21969]  
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20181205 [24360] Refactoring
        // EG 20190926 Refactoring Cst.PosRequestTypeEnum
        private Cst.ErrLevel SetPosKeepingData(MapDataReaderRow pMapDr, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote, bool pIsQuoteCanBeReaded)
        {
            int idI = Convert.ToInt32(pMapDr["IDI"].Value);
            Nullable<Cst.UnderlyingAsset> _underlyingAsset = PosKeepingTools.GetUnderLyingAssetRelativeToInstrument(pMapDr);
            int idAsset = Convert.ToInt32(pMapDr["IDASSET"].Value);
            int idA_Dealer = Convert.ToInt32(pMapDr["IDA_DEALER"].Value);
            int idB_Dealer = Convert.ToInt32(pMapDr["IDB_DEALER"].Value);
            int idA_Clearer = Convert.ToInt32(pMapDr["IDA_CLEARER"].Value);
            int idB_Clearer = 0;
            if (false == Convert.IsDBNull(pMapDr["IDB_CLEARER"].Value))
            {
                idB_Clearer = Convert.ToInt32(pMapDr["IDB_CLEARER"].Value);
            }
            int idA_EntityDealer = 0;
            if (false == Convert.IsDBNull(pMapDr["IDA_ENTITYDEALER"].Value))
            {
                idA_EntityDealer = Convert.ToInt32(pMapDr["IDA_ENTITYDEALER"].Value);
            }
            int idA_EntityClearer = 0;
            if (false == Convert.IsDBNull(pMapDr["IDA_ENTITYCLEARER"].Value))
            {
                idA_EntityClearer = Convert.ToInt32(pMapDr["IDA_ENTITYCLEARER"].Value);
            }
            PosKeepingData = m_Product.CreatePosKeepingData();
            PosKeepingData.SetPosKey(CS, null, idI, _underlyingAsset, idAsset, idA_Dealer, idB_Dealer, idA_Clearer, idB_Clearer);
            PosKeepingData.SetAdditionalInfo(idA_EntityDealer, idA_EntityClearer);
            // RD 20161212 [22660] Add condition m_PKGenProcess.IsEntry
            if (m_PKGenProcess.IsEntry || ((null != m_PosRequest) && ((Cst.PosRequestTypeEnum.RequestTypePosEffectOnly & m_PosRequest.RequestType) == m_PosRequest.RequestType)))
                PosKeepingData.SetBookDealerInfo(CS, null, _underlyingAsset, Convert.ToDateTime(pMapDr["DTBUSINESS"].Value)); //PL 20130326

            //
            // FI 20130404 Alimentation de PosKeepingData.asset            
            // EG 20150624 [21151]
            //Cst.PosRequestAssetQuoteEnum assetQuote = Cst.PosRequestAssetQuoteEnum.Asset;
            Cst.PosRequestAssetQuoteEnum assetQuote = pPosRequestAssetQuote;
            if (null != pMapDr["IDASSET_UNDERLYER"])
                assetQuote = Cst.PosRequestAssetQuoteEnum.UnderlyerAsset;
            PosKeepingAsset asset = CacheAssetFind(_underlyingAsset, idAsset, assetQuote);
            if (null == asset)
            {
                asset = SetPosKeepingAsset(null, _underlyingAsset, pMapDr);
                CacheAssetAdd(_underlyingAsset, idAsset, assetQuote, asset);
            }
            PosKeepingData.Asset = asset;

            Cst.ErrLevel codeReturn = SetPosKeepingTrade(pMapDr);
            if (codeReturn == Cst.ErrLevel.SUCCESS)
            {
                if ((false == m_PKGenProcess.IsEntry) && pIsQuoteCanBeReaded)
                {
                    // EG 20160302 (21969] Add CodeReturn
                    codeReturn = SetPosKeepingQuote();
                }

                SetPosKeepingMarket(Convert.ToInt32(pMapDr["IDEM"].Value));
            }

            return codeReturn;
        }
        #endregion SetPosKeepingData
        #region SetPosKeepingMarket
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdEM"></param>
        /// FI 20130313 [18467] add Method
        // EG 20181205 [24360] Refactoring
        private void SetPosKeepingMarket(int pIdEM)
        {
            PosKeepingData.Market = m_PKGenProcess.ProcessCacheContainer.GetEntityMarket(pIdEM);
            PosKeepingData.MarketSpecified = (null != PosKeepingData.Market);
        }
        #endregion SetPosKeepingMarket
        #region SetPosKeepingTrade
        // EG 20150716 [21103] Add DTSETTLT
        // EG 20171016 [23509] Upd dtTimeStamp, Add dtExecution
        // EG 20181205 [24360] Refactoring
        private Cst.ErrLevel SetPosKeepingTrade(MapDataReaderRow pMapDr)
        {
            int idT = Convert.ToInt32(pMapDr["IDT"].Value);
            string identifier = pMapDr["IDENTIFIER"].Value.ToString();
            int side = Convert.ToInt32(pMapDr["SIDE"].Value);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal qty = Convert.ToDecimal(pMapDr["QTY"].Value);
            DateTime dtBusiness = Convert.ToDateTime(pMapDr["DTBUSINESS"].Value);
            //PL 20171116 Pour compatibilité ascendante
            //DateTime dtTimeStamp = Convert.ToDateTime(pDr["DTTIMESTAMP"]);
            //DateTime dtExecution = Convert.ToDateTime(pDr["DTEXECUTION"]); 
            DateTime dtExecution;
            if (Convert.IsDBNull(pMapDr["DTEXECUTION"].Value))
                dtExecution = Convert.ToDateTime(pMapDr["DTTIMESTAMP"].Value); 
            else
                dtExecution = Convert.ToDateTime(pMapDr["DTEXECUTION"].Value);

            string positionEffect = pMapDr["POSITIONEFFECT"].Value.ToString();

            DateTime dtSettlement = Convert.ToDateTime(pMapDr["DTSETTLT"].Value);

            PosKeepingData.SetTrade(idT, identifier, side, qty, dtBusiness, positionEffect, dtExecution, dtSettlement);
            //PosKeepingData.SetTrade(idT, identifier, side, qty, dtBusiness, positionEffect, dtTimeStamp, book_PositionEffect, book_IsPositionEffectIgnore);

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion SetPosKeepingTrade
        #region SetPosRequestEntry
        private Cst.ErrLevel SetPosRequestEntry(IDbTransaction pDbTransaction, out int pIdPR)
        {
            IPosRequestEntry entry = m_Product.CreatePosRequestEntry(DtBusiness, PosKeepingData.Trade.Qty);
            entry.IdA_Entity = PosKeepingData.Market.IdA_Entity;
            entry.IdA_Css = PosKeepingData.Market.IdA_CSS;
            entry.IdA_CustodianSpecified = PosKeepingData.Market.IdA_CustodianSpecified;
            if (entry.IdA_CustodianSpecified)
                entry.IdA_Custodian = PosKeepingData.Market.IdA_Custodian;

            entry.IdEM = PosKeepingData.Market.IdEM;
            entry.IdEMSpecified = (0 < entry.IdEM);
            entry.IdT = PosKeepingData.Trade.IdT;
            entry.IdTSpecified = true;
            entry.StatusSpecified = true;
            entry.Status = ProcessStateTools.StatusSuccessEnum;
            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            { 
                connectionString = CS,
                id = Queue.id,
                identifier = Queue.identifier,
                idInfo = Queue.idInfo,
                requester = Queue.header.requester
            };
            PosKeepingEntryMQueue entryMQueue = new PosKeepingEntryMQueue(mQueueAttributes)
            {
                ProcessType = Cst.ProcessTypeEnum.POSKEEPENTRY
            };
            entryMQueue.header.messageQueueNameSpecified = true;
            entryMQueue.header.messageQueueName = Queue.header.messageQueueName;

            entry.SetDetail(PosKeepingData.Trade, entryMQueue);
            MQueueConnectionString _cs = entry.Detail.Message.header.connectionString;
            _cs.cryptSpecified = true;
            _cs.crypt = true;
            _cs.Value = Cryptography.Encrypt(_cs.Value);

            
            //codeReturn = PosKeepingTools.FillPosRequest(CS, pDbTransaction, entry, 
            //    ProcessBase.appInstance, ProcessBase.processLog.header.IdProcess, null);
            Cst.ErrLevel codeReturn = PosKeepingTools.FillPosRequest(CS, pDbTransaction, entry, ProcessBase.Session, ProcessBase.IdProcess, null);

            pIdPR = entry.IdPR;
            return codeReturn;
        }
        #endregion SetPosRequestEntry
        #region SetValorisationForPartialUnclearing
        /// <summary>
        /// Recalcul du montant d'un nouvel événement crée suite à décompensation partielle
        /// Evénement qui concerne la quantité non décompensée.
        /// Pas de prorata sur la quantité, mais un recalcul  à l'aide des données stockées dans la table EVENTDET sur la ligne 
        /// événement en cours de décompensation.
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private Nullable<decimal> SetValorisationForPartialUnclearing(DataRow pRowEvent, DataRow pRowEventDet, decimal pRemainderQty)
        {
            Nullable<decimal> amount = null;
            EfsML.Enum.EventTypeEnum eventType = (EfsML.Enum.EventTypeEnum)ReflectionTools.EnumParse(EfsML.Enum.EventTypeEnum.QTY, pRowEvent["EVENTTYPE"].ToString()); ;
            Nullable<decimal> price = null;
            Nullable<decimal> closingPrice = null;
            if (null != pRowEventDet)
            {
                if (false == Convert.IsDBNull(pRowEventDet["PRICE"]))
                    price = Convert.ToDecimal(pRowEventDet["PRICE"]);

                if (false == Convert.IsDBNull(pRowEventDet["CLOSINGPRICE"]))
                    closingPrice = Convert.ToDecimal(pRowEventDet["CLOSINGPRICE"]);
            }
            switch (eventType)
            {
                case EfsML.Enum.EventTypeEnum.NOM:
                    amount = PosKeepingData.NominalValue(pRemainderQty);
                    break;
                case EfsML.Enum.EventTypeEnum.RMG:
                    // EG 20141128 [20520] Nullable<decimal>
                    amount = PosKeepingData.RealizedMargin(closingPrice, price, pRemainderQty);
                    if (amount.HasValue)
                        amount = Math.Abs(amount.Value);
                    break;
            }
            return amount;
        }
        #endregion SetValorisationForPartialUnclearing
        #region Splitting
        /// <summary>
        /// TRADESPLITTING (REQUESTYPE = TRADESPLITTING)
        ///<para>
        /// Warning: Rien n'est opérer concernant les Brokers (Entity, Clearing et Executing).  
        ///</para>
        ///<para>
        /// Par conséquent, si les Dealers définis pour les nouveaux trades diffèrent totalement du Dealer du trade source, 
        /// on pourra (avec cette version) se retrouver à générer des trades incohérents.
        ///</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20150302 SetMainReturnLeg (for ReturnSwap)
        /// FI 20161005 [XXXXX] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180502 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Upd (Out newTrade_Identifier)
        // EG 20220816 [XXXXX] [WI396] Split : Barème dégressif non appliqué
        private Cst.ErrLevel SplitGen()
        {
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
                m_TemplateDataDocumentTrade = new Hashtable();

                int srcTrade_IdT = m_PosRequest.IdT;
                string srcTrade_Identifier = Queue.GetStringValueIdInfoByKey("TRADE");
                DateTime srcTrade_DtBusiness = m_PosRequest.DtBusiness;

                // INSERTION LOG 
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5061), 0,
                    new LogParam(LogTools.IdentifierAndId(srcTrade_Identifier, srcTrade_IdT)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(srcTrade_DtBusiness))));

                codeReturn = InitPosKeepingData(srcTrade_IdT, Cst.PosRequestAssetQuoteEnum.None, false);

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    // PL 20180312 WARNING: Use Read Commited !
                    //dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadUncommitted);
                    dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadCommitted);

                    // EG 20150907 (21317] New
                    if (PosKeepingData.Product.IsDebtSecurityTransaction)
                        PosKeepingData.Asset = GetAsset(dbTransaction, PosKeepingData.UnderlyingAsset, PosKeepingData.IdAsset, Cst.PosRequestAssetQuoteEnum.Asset);

                    #region ANNULATION du trade source (Unclearing et Remove)
                    // Décompensation éventuelle sur le trade à merger
                    //  EG 20170412 [23081] New Add parameter pIdPR_Parent = m_MasterPosRequest.idPR
                    codeReturn = UnClearingTradeMergeSplit(dbTransaction, srcTrade_IdT, m_MasterPosRequest.IdPR);
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        // Annulation du Trade
                        codeReturn = RemoveTrade(dbTransaction, srcTrade_IdT, srcTrade_Identifier, srcTrade_DtBusiness, "Remove after Split");
                    }
                    #endregion ANNULATION du trade source (Unclearing et Remove)

                    #region SPLITTING du trade source en N nouveaux trades
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5062), 0, new LogParam(DtFunc.DateTimeToStringDateISO(srcTrade_DtBusiness))));

                        List<Pair<int, string>> lstSplitTrades = new List<Pair<int, string>>();

                        ArrayList alDetail = ((IPosRequestDetSplit)m_MasterPosRequest.DetailBase).NewTrades;
                        foreach (EfsML.v30.PosRequest.SplitNewTrade newTrade in alDetail)
                        {
                            #region Création des nouveaux trades sur la base du trade source
                            //------------------------------------------------------------------
                            //Warning: concernant les Brokers, voir le summary de cette méthode.
                            //------------------------------------------------------------------
                            DataDocumentContainer _dataDocContainer = (DataDocumentContainer)m_TradeLibrary.DataDocument.Clone();
                            ProductContainer _productContainer = new ProductContainer(_dataDocContainer.CurrentProduct.Product, _dataDocContainer);


                            m_MasterPosRequest.PosKeepingKeySpecified = true;
                            m_MasterPosRequest.PosKeepingKey = m_Product.CreatePosKeepingKey();
                            m_MasterPosRequest.PosKeepingKey.IdI = _productContainer.IdI;
                            // EG 20220816 [XXXXX] [WI396] Passage pDbTransaction en paramètre
                            codeReturn = SetAdditionalInfoFromTrade(dbTransaction, m_MasterPosRequest);

                            if (codeReturn == Cst.ErrLevel.SUCCESS)
                            {
                                IPosRequestTradeAdditionalInfo additionalInfo = (IPosRequestTradeAdditionalInfo)m_TemplateDataDocumentTrade[_productContainer.IdI];

                                #region StActivation --> Update
                                additionalInfo.StActivation = (Cst.StatusActivation)Enum.Parse(typeof(Cst.StatusActivation), newTrade.IdStActivation);
                                #endregion StActivation --> Update

                                RptSideProductContainer rptSide = _dataDocContainer.CurrentProduct.RptSide(CS, true);
                                // FI 20161005 [XXXXX] Add NotImplementedException
                                if (null == rptSide)
                                    throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented ", _dataDocContainer.CurrentProduct.ProductBase.ToString()));

                                ExchangeTradedContainer _exchangeTradedContainer = null;
                                ReturnSwapContainer _returnSwapContainer = null;
                                DebtSecurityTransactionContainer _dstContainer = null; // EG 20150907 [21317] New
                                if (_dataDocContainer.CurrentProduct.IsExchangeTradedDerivative)
                                {
                                    // FI 20161005 [XXXXX] Modify
                                    //_exchangeTradedContainer = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)_dataDocContainer.currentProduct.product);
                                    _exchangeTradedContainer = rptSide as ExchangeTradedDerivativeContainer;
                                }
                                else if (_dataDocContainer.CurrentProduct.IsEquitySecurityTransaction)
                                {
                                    // FI 20161005 [XXXXX] Modify
                                    //_exchangeTradedContainer = new EquitySecurityTransactionContainer((IEquitySecurityTransaction)_dataDocContainer.currentProduct.product, _dataDocContainer);
                                    _exchangeTradedContainer = rptSide as EquitySecurityTransactionContainer;
                                }
                                else if (_dataDocContainer.CurrentProduct.IsReturnSwap)
                                {
                                    // FI 20161005 [XXXXX] Modify
                                    //_returnSwapContainer = new ReturnSwapContainer(CS, dbTransaction, (IReturnSwap)_dataDocContainer.currentProduct.product, _dataDocContainer);
                                    //_returnSwapContainer.InitRptSide(CS, true);
                                    _returnSwapContainer = rptSide as ReturnSwapContainer;
                                }
                                else if (_dataDocContainer.CurrentProduct.IsDebtSecurityTransaction) // EG 20150907 [21317] New
                                {
                                    // FI 20161005 [XXXXX] Modify
                                    //_dstContainer = new DebtSecurityTransactionContainer(CS, dbTransaction, (IDebtSecurityTransaction)_dataDocContainer.currentProduct.product, _dataDocContainer);
                                    //_dstContainer.InitRptSide(CS, true);

                                    _dstContainer = rptSide as DebtSecurityTransactionContainer;
                                }

                                if (null != _exchangeTradedContainer)
                                {
                                    #region TradeType --> Update
                                    if (_exchangeTradedContainer.TradeCaptureReport.TrdTypeSpecified)
                                    {
                                        //Sauvegarde du TrdType en vigueur sur le trade source dans SecondaryTrdType
                                        // EG 20130808 Secondary n'autorise pas de valeur > 1000 donc on ne copie pas le TrdType dans ce cas
                                        int _trdTypeValue = Convert.ToInt32(ReflectionTools.ConvertEnumToString<TrdTypeEnum>(_exchangeTradedContainer.TradeCaptureReport.TrdType));
                                        _exchangeTradedContainer.TradeCaptureReport.SecondaryTrdTypeSpecified = (_trdTypeValue < 1000);
                                        if (_exchangeTradedContainer.TradeCaptureReport.SecondaryTrdTypeSpecified)
                                            _exchangeTradedContainer.TradeCaptureReport.SecondaryTrdType =
                                                (SecondaryTrdTypeEnum)((int)_exchangeTradedContainer.TradeCaptureReport.TrdType);
                                    }
                                    _exchangeTradedContainer.TradeCaptureReport.TrdTypeSpecified = true;
                                    _exchangeTradedContainer.TradeCaptureReport.TrdType = TrdTypeEnum.SplitTrade;
                                    #endregion TradeType --> Update

                                    #region Quantity --> Update
                                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                    // EG 20170127 Qty Long To Decimal
                                    _exchangeTradedContainer.TradeCaptureReport.LastQty.DecValue = newTrade.Qty;
                                    #endregion Quantity --> Update

                                    #region PosEfct --> Update
                                    _exchangeTradedContainer.TradeCaptureReport.TrdCapRptSideGrp[0].PositionEffect =
                                        (PositionEffectEnum)Enum.Parse(typeof(PositionEffectEnum), newTrade.PosEfct);
                                    #endregion PosEfct --> Update
                                }


                                #region Dealer --> Update (Traders/Sales --> Remove)
                                //Initialisation des objets SQL avec le nouveau dealer
                                SQL_Actor sql_Actor = new SQL_Actor(CS, newTrade.IdA);
                                SQL_Book sql_Book = new SQL_Book(CS, newTrade.IdB);

                                //Récupération du dealer en vigueur sur le trade source
                                IFixParty _dealerFixParty = rptSide.GetDealer();
                                IParty _dealerParty = _dataDocContainer.GetParty(_dealerFixParty.PartyId.href);

                                IPartyTradeInformation _dealerPartyTradeInformation = _dataDocContainer.GetPartyTradeInformation(_dealerParty.Id);
                                IPartyTradeIdentifier _dealerPartyTradeIdentifier = _dataDocContainer.GetPartyTradeIdentifier(_dealerParty.Id);

                                SideEnum side = (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER) ? SideEnum.Buy : SideEnum.Sell);

                                //Suppresson du tradeside du dealer
                                _dataDocContainer.RemoveTradeSide(_dealerParty);

                                //Suppression des éventuels Traders et Sales du dealer
                                if (null != _dealerPartyTradeInformation)
                                {
                                    _dealerPartyTradeInformation.TraderSpecified = false;
                                    _dealerPartyTradeInformation.SalesSpecified = false;
                                }

                                //Mise à jour du Book et de l'Actor avec le nouveau dealer
                                Tools.SetBookId(_dealerPartyTradeIdentifier.BookId, sql_Book);
                                Tools.SetParty(_dealerParty, sql_Actor, true);

                                if (null != _dealerPartyTradeInformation)
                                    _dealerPartyTradeInformation.PartyReference = _dealerParty.Id;
                                _dealerPartyTradeIdentifier.PartyReference.HRef = _dealerParty.Id;

                                rptSide.SetBuyerSeller(_dealerParty.Id, side, FixML.v50SP1.Enum.PartyRoleEnum.BuyerSellerReceiverDeliverer);
                                _productContainer.SynchronizeFromDataDocument();

                                //Génération du tradeside du nouveau dealer
                                _dataDocContainer.SetTradeSide(CS, null, _dealerParty);

                                //PS: Le TradeId ayant pour tradeIdScheme="http://www.euro-finance-systems.fr/otcml/tradeid", 
                                //    et contenant donc l'Identifier du trade, est automatiquement écrasé lors de l'enregeistrement du trade par CheckAndRecord()
                                #endregion Dealer --> Update

                                if (_dataDocContainer.CurrentProduct.IsEquitySecurityTransaction)
                                {
                                    #region EquitySecurityTransaction
                                    // EG 20150723 Recalcul du GAM
                                    EquitySecurityTransactionContainer _estContainer = _exchangeTradedContainer as EquitySecurityTransactionContainer;
                                    _estContainer.SetAssetEquity(CS);
                                    _estContainer.GrossAmount.PaymentAmount.Amount.DecValue = _estContainer.CalcGrossAmount(CS, dbTransaction, newTrade.Qty).Amount.DecValue;

                                    if (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                                        _estContainer.GrossAmount.PayerPartyReference.HRef = _dealerParty.Id;
                                    else
                                        _estContainer.GrossAmount.ReceiverPartyReference.HRef = _dealerParty.Id;
                                    #endregion EquitySecurityTransaction
                                }
                                else if (_dataDocContainer.CurrentProduct.IsReturnSwap)
                                {
                                    #region ReturnSwap
                                    IReturnSwap _returnSwap = _dataDocContainer.CurrentProduct.Product as IReturnSwap;

                                    // Recalcul du notionalAmount
                                    if (_returnSwapContainer.MainReturnLeg.First.Notional.NotionalAmountSpecified)
                                    {
                                        decimal multiplier = 1;
                                        if (_returnSwapContainer.ReturnLeg.Underlyer.UnderlyerSingleSpecified &&
                                            _returnSwapContainer.ReturnLeg.Underlyer.UnderlyerSingle.NotionalBaseSpecified)
                                            multiplier = _returnSwapContainer.ReturnLeg.Underlyer.UnderlyerSingle.NotionalBase.Amount.DecValue / _returnSwapContainer.MainOpenUnits.Value;
                                        _returnSwapContainer.MainReturnLeg.First.Notional.NotionalAmount.Amount.DecValue =
                                        newTrade.Qty * _returnSwapContainer.MainInitialNetPrice.Value * multiplier;
                                    }

                                    // Nouvelle Quantité (Split)
                                    _returnSwapContainer.SetMainOpenUnits(newTrade.Qty);

                                    // Recalcul du Buyer|Seller (ReturnSwap), Payer|Receiver (ReturnLeg et InterestLeg)
                                    if (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                                    {
                                        _returnSwap.BuyerPartyReference.HRef = _dealerParty.Id;
                                        _returnSwapContainer.MainReturnLeg.First.ReceiverPartyReference.HRef = _dealerParty.Id;
                                        _returnSwapContainer.MainInterestLeg.First.PayerPartyReference.HRef = _dealerParty.Id;
                                    }
                                    else
                                    {
                                        _returnSwap.SellerPartyReference.HRef = _dealerParty.Id;
                                        _returnSwapContainer.MainReturnLeg.First.PayerPartyReference.HRef = _dealerParty.Id;
                                        _returnSwapContainer.MainInterestLeg.First.ReceiverPartyReference.HRef = _dealerParty.Id;
                                    }
                                    #endregion ReturnSwap
                                }
                                // EG 20150907 [21317] New
                                else if (_dataDocContainer.CurrentProduct.IsDebtSecurityTransaction)
                                {
                                    #region DebtSecurityTransaction
                                    IDebtSecurityTransaction _dst = _dstContainer.DebtSecurityTransaction;

                                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                    // EG 20170127 Qty Long To Decimal
                                    decimal prorata = NotionalProrata(_dst.Quantity.NumberOfUnits.DecValue, newTrade.Qty);

                                    // Qty - NotionalAmount
                                    if (_dst.Quantity.NumberOfUnitsSpecified)
                                    {
                                        _dst.Quantity.NumberOfUnits = new EFS_Decimal(newTrade.Qty);
                                        _dst.Quantity.NotionalAmount.Amount.DecValue = PosKeepingData.NominalValue(newTrade.Qty);
                                    }

                                    // GrossAmount prorata
                                    _dst.GrossAmount.PaymentAmount.Amount.DecValue = GrossAmountProrataCalculation(_dataDocContainer, prorata).Amount.DecValue;
                                    if (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                                    {
                                        _dst.GrossAmount.PayerPartyReference.HRef = _dealerParty.Id;
                                        _dst.BuyerPartyReference.HRef = _dealerParty.Id;
                                    }
                                    else
                                    {
                                        _dst.GrossAmount.ReceiverPartyReference.HRef = _dealerParty.Id;
                                        _dst.SellerPartyReference.HRef = _dealerParty.Id;
                                    }

                                    // AccruedInterest prorata
                                    if (_dst.Price.AccruedInterestAmountSpecified)
                                        _dst.Price.AccruedInterestAmount = AccruedInterestProrataCalculation(_dataDocContainer, prorata);


                                    #endregion DebtSecurityTransaction
                                }


                                //Recalcul des frais
                                TradeInput tradeInput = new TradeInput();
                                // EG 20220816 [XXXXX] [WI396] Initialisation des données avec le trade source du split
                                User user = new User(m_PKGenProcess.Session.IdA, null, RoleActor.SYSADMIN);
                                tradeInput.SearchAndDeserializeShortForm(CS, dbTransaction, srcTrade_IdT.ToString(), SQL_TableWithID.IDType.Id, user, m_PKGenProcess.Session.SessionId);
                                tradeInput.DataDocument = _dataDocContainer;
                                tradeInput.RecalculFeeAndTax(CS, dbTransaction);

                                //------------------------------------------------------------------
                                // Sauvegarde du nouveau trade dans la base de données
                                //------------------------------------------------------------------
                                List<Pair<int, string>> tradeLinkInfo = new List<Pair<int, string>>
                                {
                                    new Pair<int, string>(srcTrade_IdT, srcTrade_Identifier)
                                };

                                codeReturn = RecordTrade(dbTransaction, _dataDocContainer, additionalInfo, null, Cst.PosRequestTypeEnum.TradeSplitting,
                                                        tradeLinkInfo,
                                                        out int newTrade_IdT, out string newTrade_Identifier);

                                if ((codeReturn == Cst.ErrLevel.SUCCESS) && (newTrade.IdStActivation != Cst.StatusActivation.MISSING.ToString()))
                                {
                                    // Enregistrement des trades pour génération des événements post Commit
                                    lstSplitTrades.Add(new Pair<int, string>(newTrade_IdT, newTrade_Identifier));
                                }
                                //------------------------------------------------------------------
                            }

                            if (codeReturn != Cst.ErrLevel.SUCCESS)
                            {
                                break;
                            }
                            #endregion Création des nouveaux trades sur la base du trade source
                        }
                        if (codeReturn == Cst.ErrLevel.SUCCESS)
                        {
                            DataHelper.CommitTran(dbTransaction);

                            #region Génération des événements (Uniquement pour les trades où StActivation != MISSING)
                            if (0 < lstSplitTrades.Count)
                            {
                                lstSplitTrades.ForEach(trade =>
                                {
                                    codeReturn = RecordTradeEvent(trade.First, trade.Second);
                                    if (false == ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                                    {
                                        
                                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.SYS, 5179), 0,
                                            new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                                            new LogParam(LogTools.IdentifierAndId(trade.Second, trade.First))));
                                    }
                                });
                            }
                            #endregion Génération des événements (Uniquement pour les trades où StActivation != MISSING)
                        }
                    }

                    if (codeReturn != Cst.ErrLevel.SUCCESS)
                    {
                        throw (new Exception("Return code: " + codeReturn.ToString()));
                    }
                    #endregion SPLITTING du trade source en N nouveaux trades
                }
                else if (Cst.ErrLevel.DATAIGNORE == codeReturn)
                {
                    UpdateTradeStUsedBy(CS, srcTrade_IdT, Cst.StatusUsedBy.REGULAR, null);
                }

                return codeReturn;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                {
                    if (isException)
                    {
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    dbTransaction.Dispose();
                }
            }
        }
        #endregion Splitting

        #region TradeMergeGen
        /// <summary>
        /// Boucle finale de traitement des MERGES
        /// 1. Les uplets de trades à merger sont constitués (application du context PRIX)
        /// 2. Appel au traitement de merge par uplets
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel TradeMergeGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;

            
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8952), 1,
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                new LogParam(m_MarketPosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness))));

            // Calcul du poids des contextes pour merger les n-uplets dans le bon ordre
            if (Cst.ErrLevel.SUCCESS == WeightingContextRule())
            {
                m_LstTradeMergeRule.ForEach(tmr =>
                {
                    // Suppression des clés sans trades ou avec 1 seul trade
                    if (null != tmr.IdT_Mergeable)
                        tmr.IdT_Mergeable.ToList().Where(item => (null == item.Value) || (item.Value.Count < 2)).ToList().ForEach(item => tmr.IdT_Mergeable.Remove(item.Key));

                });

                // Tri par POIDS de contexte
                m_LstTradeMergeRule = m_LstTradeMergeRule.OrderByDescending(item => item.ResultMatching).ToList();

                // Si LOG >= Mode LEVEL3 construction du fichier LOG START(actuellement mode TXT)
                if (m_PKGenProcess.IsLevelToLog(LogLevelDetail.LEVEL3))
                {
                    SaveResultTradeMergeToFileLog("START", null);
                }

                // Si aucun candidat après construction alors on sort...
                bool isContinue = m_LstTradeMergeRule.Exists(tmr => (null != tmr.IdT_Mergeable) && (0 < tmr.IdT_Mergeable.Count));

                if (isContinue)
                {
                    m_LstTradeMergeRule.ForEach(tmr =>
                    {
                        // Traitement
                        if (null != tmr.IdT_Mergeable)
                        {
                            foreach (TradeKey key in tmr.IdT_Mergeable.Keys)
                            {
                                List<Pair<int, decimal>> _list = tmr.IdT_Mergeable[key];
                                if (tmr.Context.PriceValue.HasValue && tmr.Context.PriceValue.Value == PriceValueEnum.IDENTICAL)
                                {
                                    // Constitution des trades à merger par prix
                                    List<decimal> _groupPrice = (from p in _list
                                                                 group p by p.Second into grp
                                                                 select grp.Key).ToList();

                                    _groupPrice.ForEach(price =>
                                    {
                                        // Traitement (mode prix identique)
                                        codeReturn = TradeMergeGen(tmr, _list.Where(trade => trade.Second == price).ToList());
                                    });

                                }
                                else
                                {
                                    // Traitement (mode prix indifférent)
                                    codeReturn = TradeMergeGen(tmr, _list);
                                }

                            }
                        };
                    });
                }
                else
                {
                    
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8955), 1,
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                        new LogParam(m_MarketPosRequest.GroupProductValue),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness))));
                }
            }
            // Si LOG >= Mode LEVEL3 construction du fichier LOG FINAL(actuellement mode TXT)
            if (m_PKGenProcess.IsLevelToLog(LogLevelDetail.LEVEL3))
            {
                SaveResultTradeMergeToFileLog("END", null);
            }
            return codeReturn;
        }
        /// <summary>
        /// Traitement final de MERGE
        /// 1. Recherche trade de l'UPLET le plus récent qui servira de base pour la création du Trade MERGE
        /// 2. Annulation des trades sources (avec éventuellement MAJ POSITION)
        /// 3. Création du trade mergé
        /// 4. Génération des événement du trade mergé
        /// </summary>
        /// <param name="pTmr">Contexte TradeMergeRule courant</param>
        /// <param name="pTradeKey">Clé d'unicité des trades à merger</param>
        /// <param name="pTradeMergeable">Liste des trades à merger</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel TradeMergeGen(TradeMergeRule pTmr, List<Pair<int, decimal>> pTradeMergeable)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                #region LOG MODE FULL
                if (m_PKGenProcess.IsLevelToLog(LogLevelDetail.LEVEL4))
                {
                    
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 8953), 2,
                        new LogParam(LogTools.IdentifierAndId(pTmr.Identifier, pTmr.IdTradeMergeRule)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                        new LogParam(m_MarketPosRequest.GroupProductValue),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness))));

                    string lstTradeToLog = string.Empty;
                    pTradeMergeable.ForEach(trade => lstTradeToLog += LogTools.IdentifierAndId(m_DicTradeCandidate[trade.First].TradeIdentifier, trade.First) + " ");

                    lstTradeToLog = lstTradeToLog.Substring(0, Math.Min(125, lstTradeToLog.Length));
                    if (125 == lstTradeToLog.Length)
                        lstTradeToLog += "...";

                    
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 8954), 3,
                        new LogParam(LogTools.IdentifierAndId(pTmr.Identifier, pTmr.IdTradeMergeRule)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                        new LogParam(m_MarketPosRequest.GroupProductValue),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness)),
                        new LogParam(lstTradeToLog)));
                }
                #endregion LOG MODE FULL

                // Filtrage des trades sur la base de ceux candidats dans le contexte courant
                List<TradeCandidate> _lstTradeKey = (
                from idT in pTradeMergeable.Select(id => id.First)
                join tradeCandidate in m_DicTradeCandidate on idT equals tradeCandidate.Key
                select new TradeCandidate(tradeCandidate.Value)).ToList();

                // PL 20180312 WARNING: Use Read Commited !
                //dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadUncommitted);
                dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadCommitted);

                #region UNCLEARING AND REMOVE TRADE SOURCE
                _lstTradeKey.ForEach(trade =>
                {

                    if (isException == false)
                    {
                        // Décompensation éventuelle sur le trade à merger
                        //  EG 20170412 [23081] New Add parameter pIdPR_Parent = m_MasterPosRequest.idPR
                        if (Cst.ErrLevel.SUCCESS != UnClearingTradeMergeSplit(dbTransaction, trade.IdT, m_MasterPosRequest.IdPR))
                            isException = true;
                    }

                    if (isException == false)
                    {
                        // Annulation du Trade
                        if (Cst.ErrLevel.SUCCESS != RemoveTrade(dbTransaction, trade.IdT, trade.TradeIdentifier, trade.DtBusiness, "Remove after Merge"))
                            isException = true;
                    }

                });
                #endregion UNCLEARING AND REMOVE TRADE SOURCE

                if (isException == false)
                {

                    #region CREATE TRADE MERGE
                    if (Cst.ErrLevel.SUCCESS == CreateTradeMerge(dbTransaction, pTmr, _lstTradeKey, out int idTMerge))
                    {
                        // Enregistrement des trades pour génération des événements post Commit
                        string tradeMergeIdentifier = TradeRDBMSTools.GetTradeIdentifier(CS, dbTransaction, idTMerge);
                        // L'annulation des trades source et la création trade mergé sont commitées
                        DataHelper.CommitTran(dbTransaction);
                        // Génération des événements
                        if (Cst.ErrLevel.SUCCESS != RecordTradeEvent(idTMerge, tradeMergeIdentifier))
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5179), 0,
                                new LogParam(Cst.PosRequestTypeEnum.TradeMerging),
                                new LogParam(LogTools.IdentifierAndId(tradeMergeIdentifier, idTMerge))));
                        }
                    }
                    else
                        isException = true;
                    #endregion CREATE TRADE MERGE
                }

                if (isException == false)
                {
                    #region REMOVE TRADE SOURCE IN OTHER CONTEXT
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        m_LstTradeMergeRule.ForEach(tmr =>
                        {
                            if (false == pTmr.Equals(tmr))
                            {
                                // Suppression des trades traités existant éventuellement sur d'autres contextes
                                if (null != tmr.IdT_Mergeable)
                                {
                                    foreach (TradeKey key in tmr.IdT_Mergeable.Keys)
                                    {
                                        (from currentMerge in pTradeMergeable
                                         join nextMerge in tmr.IdT_Mergeable[key] on currentMerge.First equals nextMerge.First
                                         select nextMerge).ToList().ForEach(item => tmr.IdT_Mergeable[key].Remove(item));
                                    }
                                }
                                // Suppression des clés sans trades ou avec 1 seul trade
                                if (null != tmr.IdT_Mergeable)
                                    tmr.IdT_Mergeable.ToList().Where(item => (null == item.Value) || (item.Value.Count < 2)).ToList().ForEach(item => tmr.IdT_Mergeable.Remove(item.Key));
                            }
                        });
                    }
                    else
                        isException = true;
                    #endregion REMOVE TRADE SOURCE IN OTHER CONTEXT
                }

            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                {
                    if (isException)
                    {
                        codeReturn = Cst.ErrLevel.FAILURE;
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    dbTransaction.Dispose();
                }
            }
            return codeReturn;
        }
        #endregion TradeMergeGen

        #region UnclearingAddPartialEvent
        /// <summary>
        /// DECOMPENSATION UNITAIRE
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Ajout des événements liés à une décompensation partielle</para> 
        ///<para>  ● Pour chaque événement décompensé au statut IDSTACTIVATION = DEACTIV, ajout par copie d'un même </para> 
        ///<para>    ajout par copie d'un même événement en date de compensation en date de compensation d'origine </para> 
        ///<para>    pour la quantité NON DECOMPENSEE</para>
        ///<para>  ● Recalcul des montants (et non PRORATA) en fonction de l'EVENTTYPE et</para>
        ///<para>    à l'aide des valeurs originales stockées dans EVENTDET</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pDtEventPosActionDet"><Table EVENTPOSACTIONDET/param>
        /// <param name="pRowEvent">Ligne Evénement</param>
        /// <param name="pRemainderQty">Quantité restante</param>
        /// <param name="pNewIdPADET">Nouvel Id POSACTIONDET(</param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20141224 [20566] Les dates des événements de la quantité non décompensée passe à la date DTBUSINESS
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20160106 [21679][POC-MUREX] Add pDbTransaction parameter 
        // EG 20170127 Qty Long To Decimal
        private Cst.ErrLevel UnclearingAddPartialEvent(IDbTransaction pDbTransaction, DataTable pDtEventPosActionDet, DataRow pRowEvent, decimal pRemainderQty, int pNewIdPADET)
        {
            DataRow[] rowEventChilds = pRowEvent.GetChildRows(m_DsEvents.ChildEvent);

            if (ArrFunc.IsFilled(rowEventChilds))
            {
                // On récupère des jetons en cas de décompensation partielle pour insertion 
                // des événements avec la quantité non décompensée
                // EG 20160106 [21679][POC-MUREX] pDbTransaction replace CS
                SQLUP.GetId(out int newIdE, pDbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, rowEventChilds.Length + 1);

                // ------------------------------------------------------ //
                // Copie de l'événement de regroupement (OFS / POC / ...) //
                // ------------------------------------------------------ //
                // EVENT
                object[] clone = (object[])pRowEvent.ItemArray.Clone();
                DataRow rowEventGroup = m_DsEvents.DtEvent.NewRow();
                rowEventGroup.ItemArray = clone;
                rowEventGroup.BeginEdit();
                rowEventGroup["IDE"] = newIdE;
                rowEventGroup["VALORISATION"] = pRemainderQty;
                rowEventGroup["IDASTACTIVATION"] = ProcessBase.Session.IdA;
                rowEventGroup["DTSTACTIVATION"]  = OTCmlHelper.GetDateSysUTC(ProcessBase.Cs); 
                rowEventGroup["IDSTACTIVATION"] = Cst.StatusActivation.REGULAR;
                // EG 20141224 [20566]   DTSTARTADJ, DTENDADJ = DtBUSINESS
                rowEventGroup["DTSTARTADJ"] = m_MasterPosRequest.DtBusiness;
                rowEventGroup["DTSTARTUNADJ"] = m_MasterPosRequest.DtBusiness;
                rowEventGroup["DTENDADJ"] = m_MasterPosRequest.DtBusiness;
                rowEventGroup["DTENDUNADJ"] = m_MasterPosRequest.DtBusiness; 
                rowEventGroup.EndEdit();
                m_DsEvents.DtEvent.Rows.Add(rowEventGroup);
                // EVENTCLASS
                CopyEventClass(pRowEvent, newIdE);
                // EVENTPOSACTIONDET
                AddEventPosActionDet(pDtEventPosActionDet, newIdE, pNewIdPADET);

                //
                // ------------------------------------------------------ //
                // Copie des événements enfants du regroupement
                // ------------------------------------------------------ //
                int newIdEParent = newIdE;
                foreach (DataRow rowEventChild in rowEventChilds)
                {
                    newIdE++;
                    DataRow[] rowEventDets = rowEventChild.GetChildRows(m_DsEvents.ChildEventDet);
                    DataRow rowEventDet = null;
                    if (ArrFunc.IsFilled(rowEventDets))
                        rowEventDet = rowEventDets[0];

                    // EVENT
                    clone = (object[])rowEventChild.ItemArray.Clone();
                    DataRow rowNewEventChild = m_DsEvents.DtEvent.NewRow();
                    rowNewEventChild.ItemArray = clone;
                    rowNewEventChild.BeginEdit();
                    rowNewEventChild["IDE"] = newIdE;
                    rowNewEventChild["IDE_EVENT"] = newIdEParent;
                    rowNewEventChild["IDASTACTIVATION"] = ProcessBase.Session.IdA;
                    // FI 20200820 [25468] Dates systèmes en UTC
                    rowNewEventChild["DTSTACTIVATION"] = OTCmlHelper.GetDateSysUTC(CS);
                    rowNewEventChild["IDSTACTIVATION"] = Cst.StatusActivation.REGULAR;
                    // EG 20141224 [20566] DTSTARTADJ, DTENDADJ = DtBUSINESS
                    rowEventGroup["DTSTARTADJ"] = m_MasterPosRequest.DtBusiness;
                    rowEventGroup["DTSTARTUNADJ"] = m_MasterPosRequest.DtBusiness;
                    rowEventGroup["DTENDADJ"] = m_MasterPosRequest.DtBusiness;
                    rowEventGroup["DTENDUNADJ"] = m_MasterPosRequest.DtBusiness; 

                    string eventType = rowEventChild["EVENTTYPE"].ToString();
                    if (EventTypeFunc.IsQuantity(eventType) || EventTypeFunc.IsNominal(eventType))
                        rowNewEventChild["EVENTCODE"] = EventCodeFunc.Intermediary;

                    if (EventTypeFunc.IsQuantity(rowEventChild["EVENTTYPE"].ToString()))
                        rowNewEventChild["VALORISATION"] = pRemainderQty;
                    else
                        rowNewEventChild["VALORISATION"] = SetValorisationForPartialUnclearing(rowEventChild, rowEventDet, pRemainderQty);

                    rowNewEventChild.EndEdit();
                    m_DsEvents.DtEvent.Rows.Add(rowNewEventChild);

                    // EVENTCLASS
                    CopyEventClass(rowEventChild, newIdE);
                    // EVENTDET
                    CopyEventDet(rowEventDet, newIdE, pRemainderQty);
                    // EVENTPOSACTIONDET
                    AddEventPosActionDet(pDtEventPosActionDet, newIdE, pNewIdPADET);
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UnclearingAddPartialEvent
        #region UnclearingAllTrade
        /// <summary>
        /// DECOMPENSATION TOTALE
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Utilisé par REQUESTYPE = RMVALLOC/POT</para> 
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pDbTransaction">Mode transactionnel (REQUESTYPE = POT, RMVALLOC)</param>
        /// <param name="pIdT"></param>
        /// <param name="pLstIdTForValuation"></param>
        /// <param name="pIdPR_Parent"></param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20160106 [21679][POC-MUREX]
        // EG 20170412 [23081] Add parameter : pIdPR_Parent
        // EG 20180614 [XXXXX] Use pDbTransaction
        private Cst.ErrLevel UnclearingAllTrade(IDbTransaction pDbTransaction, int pIdT, List<Pair<int, string>> pLstIdTForValuation, int pIdPR_Parent)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            Pair<int, string> pair = new Pair<int, string>(pIdT, Queue.GetStringValueIdInfoByKey("TRADE"));
            pLstIdTForValuation.Add(pair);

            using (DataSet ds = GetUnclearingPosactionDet(pDbTransaction, pIdT))
            {
                int nbRows = ds.Tables[0].Rows.Count;
                if (0 < nbRows)
                {
                    // EG 20160106 [21679][POC-MUREX] pDbTransaction replace CS
                    SQLUP.GetId(out int newIdPR, pDbTransaction, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, nbRows);

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        m_PosRequest = PosKeepingTools.SetPosRequestUnclearing(m_Product, dr);
                        if (null != m_PosRequest)
                        {
                            // On ne décompense que les POSACTIONDET de couple IDT (CLOTUREE/CLOTURANTE)
                            // cela exclu donc naturellement les TRANSFERT, DENOUEMENT, CORRECTION etc... mono trade
                            // mais pas les MOF
                            IPosRequestUnclearing PosRequestUnclearing = (IPosRequestUnclearing)m_PosRequest;
                            //  EG 20170412 [23081] Add test (unclearing.detail.requestType == Cst.PosRequestTypeEnum.MaturityOffsettingFuture)
                            //  EG 20170412 [23081] Add parameter : pIdPR_Parent
                            if ((PosRequestUnclearing.Detail.IdT_Closing != PosRequestUnclearing.IdT) || (PosRequestUnclearing.Detail.RequestType == Cst.PosRequestTypeEnum.MaturityOffsettingFuture))
                            {
                                // FI 20201117 [24872] Si annulation de trades en masse, il est possible que l'annulation d'une compensation soit traitée par 2 instances SpheresClosingGen® simultanément 
                                // (1 qui traite l'annulation du trade closing et l'autre qui traite l'annulation du trade closed)
                                // Pour gérer ce cas mis en plase d'un lock sur l'IDPADET pour que l'annulation soit traitée 2 fois, certes, mais séquentiellement 
                                LockObject lockPosactionDet = null;
                                if (PosRequestUnclearing.Detail.IdT_Closing != PosRequestUnclearing.IdT)
                                {
                                    lockPosactionDet = LockPosActionDet(CS, null, PosRequestUnclearing.Detail.IdPADET, PosRequestUnclearing.RequestType.ToString());
                                    if (null == lockPosactionDet)
                                    {
                                        // On ne devrait pas renter ici. Si une annulation est en cours pour cet IDPADET (par une autre instance), elle ne devrait pas dépasser les 30 secondes.
                                        // Si toutefois le Lock ne peut etre poser le traitement termine en erreur (LOCKUNSUCCESSFUL) (un roolback sera opéré)  
                                        codeReturn = Cst.ErrLevel.LOCKUNSUCCESSFUL;
                                        break;
                                    }
                                }

                                try // FI 20201117 [24872] try cath pour assurer la suppression du lock en fin de unclearing
                                {
                                    // Insert POSREQUEST (UNCLEARING)   
                                    PosRequestUnclearing.IdPR = newIdPR;
                                    PosRequestUnclearing.StatusSpecified = true;
                                    PosRequestUnclearing.Status = ProcessStateTools.StatusPendingEnum;
                                    PosRequestUnclearing.IdPR_PosRequestSpecified = true;
                                    PosRequestUnclearing.IdPR_PosRequest = pIdPR_Parent;
                                    
                                    //PosKeepingTools.InsertPosRequest(CS, pDbTransaction, newIdPR, m_PosRequest, m_PKGenProcess.appInstance, LogHeader.IdProcess, pIdPR_Parent);
                                    PosKeepingTools.InsertPosRequest(CS, pDbTransaction, newIdPR, PosRequestUnclearing, m_PKGenProcess.Session, IdProcess, pIdPR_Parent);
                                    try
                                    {
                                        // DECOMPENSATION
                                        //  EG 20170412 [23081] Add parameter : pIdPR_Parent
                                        codeReturn = UnclearingGen(pDbTransaction, false, pIdPR_Parent);

                                        Pair<int, string> closingTrade = new Pair<int, string>(PosRequestUnclearing.Detail.IdT_Closing, PosRequestUnclearing.Detail.Closing_Identifier);
                                        pLstIdTForValuation.Add(closingTrade);
                                    }
                                    catch
                                    {
                                        codeReturn = Cst.ErrLevel.FAILURE;
                                        throw;
                                    }
                                    finally
                                    {
                                        // Update POSREQUEST (UNCLEARING)    
                                        //  EG 20170412 [23081] Add parameter : pIdPR_Parent
                                        UpdatePosRequest(pDbTransaction, codeReturn, PosRequestUnclearing, pIdPR_Parent);
                                    }
                                }
                                finally
                                {
                                    if (null != lockPosactionDet)
                                        LockTools.UnLock(CS, lockPosactionDet, this.ProcessBase.Session.SessionId);
                                }
                                newIdPR++;
                            }
                        }
                    }
                }
            }
            return codeReturn;
        }
        #endregion UnclearingAllTrade

        #region UnclearingDeactivEvent
        /// <summary>
        /// DECOMPENSATION UNITAIRE
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Traitement des événements liés à la DECOMPENSATION</para> 
        ///<para>  ● Statut IDSTACTIVATION = DEACTIV</para>
        ///<para>  ● Ajout d'un EVENTCLASS de type RMV en date de décompensation (DTBUSINESS du PosRequest)</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pRowEvent">Ligne Evénement</param>
        /// <returns>Cst.ErrLevel</returns>
        private Cst.ErrLevel UnclearingDeactivEvent(DataRow pRowEvent)
        {
            // EVENT (DEACTIV)
            pRowEvent.BeginEdit();
            pRowEvent["IDASTACTIVATION"] = ProcessBase.Session.IdA;
            pRowEvent["DTSTACTIVATION"]  = OTCmlHelper.GetDateSysUTC(CS);
            pRowEvent["IDSTACTIVATION"] = Cst.StatusActivation.DEACTIV;
            pRowEvent.EndEdit();
            // EVENTCLASS (RMV)
            DataRow rowClass = m_DsEvents.DtEventClass.NewRow();
            rowClass.BeginEdit();
            rowClass["IDE"] = pRowEvent["IDE"];
            rowClass["EVENTCLASS"] = EventClassFunc.RemoveEvent;
            rowClass["DTEVENT"] = m_MasterPosRequest.DtBusiness;
            rowClass["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(CS, m_MasterPosRequest.DtBusiness);
            rowClass["ISPAYMENT"] = false;
            rowClass.EndEdit();
            m_DsEvents.DtEventClass.Rows.Add(rowClass);
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UnclearingDeactivEvent
        #region UnclearingEvents
        /// <summary>
        /// DECOMPENSATION UNITAIRE
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Décompensation des EVENEMENTS (pour la clôturée et la clôturante)</para> 
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pDbTransaction">Mode transactionnel</param>
        /// <param name="pDtEventPosActionDet">Table EVENTPOSACTIONDET</param>
        /// <param name="pIdT">Id du trade à décompenser</param>
        /// <param name="pClosingQty">Quantité initiale</param>
        /// <param name="pUnclearingQty">Quantité à décompenser</param>
        /// <param name="pNewIdPADET">Nouvel Id de POSACTIONDET (cas DECOMPENSATION PARTIELLE)</param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20160106 [21679][POC-MUREX]
        // EG 20170127 Qty Long To Decimal
        private Cst.ErrLevel UnclearingEvents(IDbTransaction pDbTransaction, DataTable pDtEventPosActionDet, int pIdT, 
            decimal pClosingQty, decimal pUnclearingQty, int pNewIdPADET)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if (ArrFunc.IsFilled(pDtEventPosActionDet.Rows))
            {

                // EG 20150612 [20665] Chargement EVENT|EVENTCLASS|EVENTDET 
                m_DsEvents = new DataSetEventTrade(CS, pDbTransaction,  pIdT);
                m_DsEvents.Load(EventTableEnum.Class | EventTableEnum.Detail, null, null);
                bool isPartialUnclearing = (pClosingQty != pUnclearingQty);

                int nbRows = pDtEventPosActionDet.Rows.Count;
                for (int i = 0; i < nbRows; i++)
                {
                    int idE = Convert.ToInt32(pDtEventPosActionDet.Rows[i]["IDE"]);
                    DataRow rowEvent = m_DsEvents.RowEvent(idE);
                    string eventCode = rowEvent["EVENTCODE"].ToString();
                    if (isPartialUnclearing)
                    {
                        if (EventCodeFunc.IsClearing(eventCode))
                        {
                            // EG 20160106 [21679][POC-MUREX] Add pDbTransaction 
                            UnclearingAddPartialEvent(pDbTransaction, pDtEventPosActionDet, rowEvent, pClosingQty - pUnclearingQty, pNewIdPADET);
                        }
                    }
                    codeReturn = UnclearingDeactivEvent(rowEvent);
                    if ((codeReturn == Cst.ErrLevel.SUCCESS) && EventCodeFunc.IsClearing(eventCode))
                    {
                        DateTime dtOffsetting = Convert.ToDateTime(rowEvent["DTSTARTADJ"]);
                        codeReturn = InsertUnclearingEvents(pDbTransaction, pDtEventPosActionDet.Rows[i], rowEvent, pNewIdPADET, isPartialUnclearing, pUnclearingQty, dtOffsetting);
                    }
                }
                // Mise à jour EVENT
                DataTable dtChange = m_DsEvents.DtEvent.GetChanges();
                m_DsEvents.Update(pDbTransaction);
                //m_DsEvents.UpdateEventDet(pDbTransaction);
                m_DsEvents.Update(pDbTransaction,m_DsEvents.DtEventDet, Cst.OTCml_TBL.EVENTDET);
                UpdateEventProcess(pDbTransaction, dtChange);
                if (isPartialUnclearing)
                {
                    // Mise à jour EVENTPOSACTIONDET
                    string sqlSelect = SQLCst.SELECT + "IDPADET, IDE" + SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENTPOSACTIONDET;
                    DataHelper.ExecuteDataAdapter(pDbTransaction, sqlSelect, pDtEventPosActionDet);
                }
            }
            return codeReturn;
        }
        #endregion UnclearingEvents
        #region UnclearingGen
        /// <summary>
        /// DECOMPENSATION UNITAIRE
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        protected Cst.ErrLevel UnclearingGen()
        {
            return UnclearingGen(null, true, m_MasterPosRequest.IdPR);
        }
        /// <summary>
        /// DECOMPENSATION UNITAIRE
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► REQUESTYPE = UNCLEARING/POT</para> 
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pDbTransaction">Mode transactionnel (REQUESTYPE = POT)</param>
        /// <param name="pReloadPosKeepingData">Indicateur de rechargement des données de clé de position (REQUESTYPE = UNCLEARING)</param>
        /// <returns>Cst.ErrLevel</returns>
        //  EG 20170412 [23081] Add parameter : pIdPR_Parent
        // EG 20181019 PERF(Commit manquant)
        // EG 20190926 Refactoring Cst.PosRequestTypeEnum
        protected Cst.ErrLevel UnclearingGen(IDbTransaction pDbTransaction, bool pReloadPosKeepingData, int pIdPR_Parent)
        {
            IDbTransaction dbTransaction = pDbTransaction;
            if (null == pDbTransaction)
            {
                // PL 20180312 WARNING: Use Read Commited !
                //dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadUncommitted);
                dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadCommitted);
            }

            bool isException = false;
            try
            {
                // Chargement de la ligne POSACTIONDET à décompenser
                Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
                if (pReloadPosKeepingData)
                    InitPosKeepingData(null, m_PosRequest.PosKeepingKey, Cst.PosRequestAssetQuoteEnum.None, false);
                
                IPosRequestUnclearing posRequest = (IPosRequestUnclearing)m_PosRequest;
                int idT_Closed = posRequest.IdT;
                int idT_Closing = posRequest.Detail.IdT_Closing;
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                decimal unclearingQty = posRequest.Qty;
                decimal closingQty = posRequest.Detail.ClosingQty;
                //bool isPartialUnclearing = (closingQty != unclearingQty);

                DataSet ds = GetUnclearingPosactionDet(dbTransaction, posRequest.Detail.IdPADET, idT_Closed, idT_Closing);
                if (null != ds)
                {
                    DataTable dtPosAction = ds.Tables[0];
                    DataTable dtPosActionDet = ds.Tables[1];
                    DataTable dtEventPosActionClosed = ds.Tables[2];
                    DataTable dtEventPosActionClosing = ds.Tables[3];

                    // FI 20201117 [24872] Add test if. 
                    // Le unclearing a peut-être été prise en charge par une autre instance (cas possible si demande d'annulation de trade en masse). 
                    // Si tel est le cas Dans il n'y a pas de unclearing (puisque déjà opéré) et codeReturn reste à Cst.ErrLevel.SUCCESS
                    if (dtPosActionDet.Rows.Count > 0) 
                    {
                        int newIdPADET = UnclearingPosActionDet(dbTransaction, dtPosAction, dtPosActionDet, closingQty, unclearingQty);
                        // Décompensation de la négociation CLOTUREE
                        if (-1 < newIdPADET)
                        {
                            codeReturn = UnclearingEvents(dbTransaction, dtEventPosActionClosed, idT_Closed, closingQty, unclearingQty, newIdPADET);
                            // Décompensation de la négociation CLOTURANTE
                            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (idT_Closed != idT_Closing))
                                codeReturn = UnclearingEvents(dbTransaction, dtEventPosActionClosing, idT_Closing, closingQty, unclearingQty, newIdPADET);
                        }
                        else
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5151), 0,
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("TRADE"), posRequest.IdT)),
                                new LogParam(posRequest.Qty),
                                new LogParam(posRequest.Detail.IdPADET)));

                            codeReturn = Cst.ErrLevel.DATAUNMATCH;
                        }
                    }
                    //  EG 20170412 [23081] Change next condition / Comment next line
                    //if ((null == pDbTransaction) && (null != dbTransaction) && (Cst.ErrLevel.SUCCESS == codeReturn))
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        // Remove de la livraison suite à dénouement
                        /// EG 20131127 [19254/19239] Décompensation éventuelle de la livraison du future
                        // Création d'un posrequest et appel RemoveAllocationGen

                        List<Pair<int, string>> lstIdtForValuation = new List<Pair<int, string>>();
                        if ((Cst.ErrLevel.SUCCESS == codeReturn) && posRequest.Detail.IdT_DeliverySpecified)
                        {
                            // Début de traitement nouvelle clé de position : INSERT POSREQUEST (RemoveAllocationGen)
                            m_PosRequest = PosKeepingTools.CreatePosRequestRemoveAlloc(CS, m_Product,
                                posRequest.Detail.IdT_Delivery, posRequest.Detail.Delivery_Identifier,
                                posRequest.IdA_Entity, posRequest.IdA_Css, posRequest.IdA_Custodian, posRequest.IdEM, posRequest.DtBusiness, posRequest.Qty, posRequest.Qty, 
                                posRequest.Notes, posRequest.GroupProductEnum);

                            //  EG 20170412 [23081] Add parameter : pIdPR_Parent
                            codeReturn = RemoveAllocationGen(dbTransaction, ref lstIdtForValuation, pIdPR_Parent);
                        }

                        // EG 20170424 [23064]  add {}
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            if ((null == pDbTransaction) && (null != dbTransaction))
                            {
                                // PM 20170628 [23270] Ajout nouveau BeginTran après le CommitTran car la transaction est de nouveau utilisée plus loin
                                // (suite à "Object reference not set to an instance of an object" lors de décompensation sur base QUALITY)
                                DataHelper.CommitTran(dbTransaction);
                                // PL 20180312 WARNING: Use Read Commited !
                                //dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadUncommitted);
                                dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadCommitted);
                            }
                        }
                        else
                        {
                            isException = true;
                        }

                        if ((Cst.ErrLevel.SUCCESS == codeReturn) && posRequest.Detail.IdT_DeliverySpecified)
                        {
                            //  EG 20170412 [23081] Add parameter : dbTransaction
                            if (Cst.ErrLevel.SUCCESS == codeReturn)
                                codeReturn = RemoveTrade(dbTransaction, posRequest.Detail.IdT_Delivery, posRequest.Detail.Delivery_Identifier, 
                                    posRequest.DtBusiness, posRequest.Notes);

                            if (Cst.ErrLevel.SUCCESS == codeReturn)
                            {
                                //  EG 20170412 [23081] New
                                codeReturn = PosKeepingTools.DeletePosRequestUnderlyerDelivery(CS, dbTransaction,
                                    posRequest.Detail.IdT_Delivery, posRequest.DtBusiness, posRequest.IdT);
                            }

                            if (Cst.ErrLevel.SUCCESS == codeReturn)
                            {
                                lstIdtForValuation.ForEach(item =>
                                {
                                    if (item.First != posRequest.Detail.IdT_Delivery)
                                        // EG 20240115 [WI808] Harmonisation et réunification des méthodes
                                        codeReturn = ValuationAmountGen(dbTransaction, (item.First, item.Second), PosKeepingData.AssetInfo, true);
                                }
                                );
                            }
                        }

                        if (Cst.ErrLevel.SUCCESS == codeReturn && (dtPosActionDet.Rows.Count > 0)) // FI 20201117 [24872] Add test if. Ne pas refaire ValuationAmountGen lorsque déjà opéré par une autre instance  (cas possible si demande d'annulation de trade en masse). 
                        {
                            // EG 20140206 Lecture du type de compensation à décompenser pour déterminer si Recalcul des montants sous LPP/AMT
                            // Pas de recalcul pour les dénouements à l'échéance (Dénouement auto ou manuel à l'échéance)
                            IPosRequestDetUnclearing detail = (IPosRequestDetUnclearing)posRequest.Detail;
                            //  EG 20170412 [23081] change _requestTypeOption by _requestTypeOptionAndMaturityOffSetting
                            bool isAvailableValuation = ((Cst.PosRequestTypeEnum.RequestTypeOptionAndMaturityOffSetting & detail.RequestType) != detail.RequestType);
                            if (false == isAvailableValuation)
                                isAvailableValuation = (detail.DtBusiness < MaturityDateSys);

                            //  EG 20170412 [23081] add parameter : dbTransaction
                            if (isAvailableValuation)
                            {
                                // EG 20240115 [WI808] Harmonisation et réunification des méthodes
                                codeReturn = ValuationAmountGen(dbTransaction, (idT_Closed, Queue.GetStringValueIdInfoByKey("TRADE")), PosKeepingData.AssetInfo, true);
                                if ((Cst.ErrLevel.SUCCESS == codeReturn) && (idT_Closed != idT_Closing))
                                    // EG 20240115 [WI808] Harmonisation et réunification des méthodes
                                    codeReturn = ValuationAmountGen(dbTransaction, (idT_Closing, posRequest.Detail.Closing_Identifier), PosKeepingData.AssetInfo, true);
                            }
                        }
                    }
                }
                return codeReturn;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null == pDbTransaction) && (null != dbTransaction))
                {
                    if (isException)
                        DataHelper.RollbackTran(dbTransaction);
                    else
                        DataHelper.CommitTran(dbTransaction);

                    dbTransaction.Dispose();
                }
            }
        }
        #endregion UnclearingGen
        #region UnclearingPosActionDet
        /// <summary>
        /// DECOMPENSATION UNITAIRE
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Décompensation dans POSACTION/POSACTIONDET</para> 
        ///<para>  ● ANNULATION de la ligne compensée</para>
        ///<para>  ● INSERTION éventuelle d'une nouvelle ligne dans POSACTION dans le cas d'un transfert partiel</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pDbTransaction">Mode transactionnel</param>
        /// <param name="pDtPosAction">Table POSACTION</param>
        /// <param name="pDtPosActionDet">Table POSACTIONDET</param>
        /// <param name="pClosingQty">Quantité initiale</param>
        /// <param name="pUnclearingQty">Quantité à décompenser</param>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20141205 Add POSITIONEFFECT
        // EG 20141224 [20566] La ligne qui reste suite à décompensation partielle sera à la date DTBUSINESS courante.
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // PL 20180309 UP_GETID use a Shared Sequence on POSACTION/POSACTIONDET
        private int UnclearingPosActionDet(IDbTransaction pDbTransaction, DataTable pDtPosAction, DataTable pDtPosActionDet, decimal pClosingQty, decimal pUnclearingQty)
        {
            int newIdPADET = -1;
            if (1 == pDtPosActionDet.Rows.Count)
            {
                bool isPartialUnclearing = (pClosingQty != pUnclearingQty);
                DataRow rowPosActionDet = pDtPosActionDet.Rows[0];
                // FI 20200820 [25468] dates systemes en UTC
                DateTime dtSys = OTCmlHelper.GetDateSysUTC(CS);
                if (isPartialUnclearing)
                {
                    // EG 20140522 Récupération de l'IDPR de ligne ligne décompensée
                    int _idPRSource = m_MasterPosRequest.IdPR;
                    if (m_MasterPosRequest is IPosRequestUnclearing posRequestUnclearing)
                        _idPRSource = posRequestUnclearing.Detail.IdPR;
                    // Nouvelles lignes dans POSACTION/POSACTIONDET pour insertion la quantité non DECOMPENSEE 
                    // rattachées au POSREQUEST de la demande de DECOMPENSATION
                    // POSACTION
                    SQLUP.GetId(out int newIdPA, CS, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);
                    DataRow rowNewPosAction = pDtPosAction.NewRow();
                    rowNewPosAction.BeginEdit();
                    rowNewPosAction["IDPA"] = newIdPA;
                    rowNewPosAction["IDPR"] = _idPRSource;
                    rowNewPosAction["DTBUSINESS"] = m_MasterPosRequest.DtBusiness;
                    rowNewPosAction["DTINS"] = dtSys;
                    rowNewPosAction["IDAINS"] = ProcessBase.Session.IdA;
                    rowNewPosAction.EndEdit();
                    pDtPosAction.Rows.Add(rowNewPosAction);
                    // POSACTIONDET
                    newIdPADET = newIdPA;
                    object[] clone = (object[])rowPosActionDet.ItemArray.Clone();
                    DataRow rowNewPosActionDet = pDtPosActionDet.NewRow();
                    rowNewPosActionDet.ItemArray = clone;
                    rowNewPosActionDet.BeginEdit();
                    rowNewPosActionDet["IDPA"] = newIdPA;
                    rowNewPosActionDet["IDPADET"] = newIdPADET;
                    rowNewPosActionDet["QTY"] = pClosingQty - pUnclearingQty;
                    rowNewPosActionDet["DTINS"] = dtSys;
                    rowNewPosActionDet["IDAINS"] = ProcessBase.Session.IdA;
                    rowNewPosActionDet.EndEdit();
                    pDtPosActionDet.Rows.Add(rowNewPosActionDet);
                }
                else
                {
                    newIdPADET = Convert.ToInt32(rowPosActionDet["IDPADET"]);
                }
                // POSACTIONDET (CANCEL INFO)
                rowPosActionDet.BeginEdit();
                rowPosActionDet["IDACAN"] = ProcessBase.Session.IdA;
                rowPosActionDet["DTCAN"] = m_MasterPosRequest.DtBusiness;
                rowPosActionDet["CANDESCRIPTION"] = Cst.PosRequestTypeEnum.UnClearing;
                //FI 20120502  Alimentation des colonnes DTUPD et IDAUPD
                rowPosActionDet["DTUPD"] = dtSys;
                rowPosActionDet["IDAUPD"] = ProcessBase.Session.IdA;
                rowPosActionDet.EndEdit();
                //
                string sqlSelect = SQLCst.SELECT + "pa.IDPA, pa.IDPR, pa.DTBUSINESS, pa.DTINS, pa.IDAINS";
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSACTION + " pa" + Cst.CrLf;
                _ = DataHelper.ExecuteDataAdapter(pDbTransaction, sqlSelect, pDtPosAction);
                //
                sqlSelect = SQLCst.SELECT + "pad.IDPA, pad.IDPADET, pad.QTY, pad.POSITIONEFFECT, pad.DTCAN, pad.IDACAN, pad.CANDESCRIPTION,pad.DTUPD,pad.IDAUPD,";
                sqlSelect += "pad.IDT_BUY, pad.IDT_SELL, pad.IDT_CLOSING, pad.DTINS, pad.IDAINS" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSACTIONDET + " pad" + Cst.CrLf;
                _ = DataHelper.ExecuteDataAdapter(pDbTransaction, sqlSelect, pDtPosActionDet);
            }
            return newIdPADET;
        }
        #endregion UnclearingPosActionDet
        #region UnClearingTradeMergeSplit
        /// <summary>
        /// Décompensation Trade(s) source(s) d'un Merge / Split 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        //  EG 20170412 [23081] add parameter : pIdPR_Parent 
        private Cst.ErrLevel UnClearingTradeMergeSplit(IDbTransaction pDbTransaction, int pIdT, int pIdPR_Parent)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            using (DataSet ds = GetUnclearingPosactionDet(pDbTransaction, pIdT))
            {
                int nbRows = ds.Tables[0].Rows.Count;
                if (0 < nbRows)
                {
                    DeserializeTrade(pIdT);
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        m_PosRequest = PosKeepingTools.SetPosRequestUnclearing(m_Product, dr);
                        if (null != m_PosRequest)
                        {
                            IPosRequestUnclearing unclearing = (IPosRequestUnclearing)m_PosRequest;
                            //  EG 20170412 [23081] add parameter : pIdPR_Parent 
                            if (unclearing.Detail.IdT_Closing != unclearing.IdT)
                                codeReturn = UnclearingGen(pDbTransaction, false, pIdPR_Parent);
                        }
                    }
                }

            }
            return codeReturn;
        }
        #endregion UnClearingTradeMergeSplit
        #region UpdateEntryGen
        /// <summary>
        /// MISE A JOUR DES CLOTURES (Demande utilisateur)
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>1. ► Initialisation des classes de travail (Clé de position...)</para>
        ///<para>2. ► Traitement</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20170315 [22967] m_PosRequest.groupProductValue instead of m_MarketPosRequest.groupProductValue
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel UpdateEntryGen()
        {
            Cst.ErrLevel ret = InitPosKeepingData(m_PosRequest.IdT, Cst.PosRequestAssetQuoteEnum.None, true);

            
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5057), 0,
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_PosRequest.IdA_CssCustodian)),
                new LogParam(Queue.GetStringValueIdInfoByKey("MARKET")),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(Queue.GetStringValueIdInfoByKey("ASSET")),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("DEALER"), m_PosRequest.PosKeepingKey.IdA_Dealer)),
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("BOOKDEALER"), m_PosRequest.PosKeepingKey.IdB_Dealer)),
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CLEARER"), m_PosRequest.PosKeepingKey.IdA_Clearer)),
                new LogParam((0 < m_PosRequest.PosKeepingKey.IdB_Clearer) ?
                    LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("BOOKCLEARER"), m_PosRequest.PosKeepingKey.IdB_Clearer) : "-")));

            if (Cst.ErrLevel.SUCCESS == ret)
            {
                ret = CommonEntryGen();
            }
            else if (Cst.ErrLevel.DATAIGNORE == ret)
            {
                ret = Cst.ErrLevel.SUCCESS;
            }

            return ret;
        }
        #endregion UpdateEntryGen
        #region UpdateEventProcess
        private void UpdateEventProcess(IDbTransaction pDbTransaction, DataTable pDtEventChanged)
        {

            EventProcess eventProcess = new EventProcess(CS); 

            if (null != pDtEventChanged)
            {
                DateTime dtSys = OTCmlHelper.GetDateSysUTC(CS);
                foreach (DataRow row in pDtEventChanged.Rows)
                {
                    if (DataRowState.Deleted != row.RowState)
                    {
                        int idE = Convert.ToInt32(row["IDE"]);
                        eventProcess.Write(pDbTransaction, idE, Cst.ProcessTypeEnum.CLOSINGGEN, ProcessStateTools.StatusSuccessEnum, dtSys, ProcessBase.Tracker.IdTRK_L);
                    }
                }
            }
        }
        #endregion UpdateEventProcess
        #region UpdatePosRequestTerminated
        /// <summary>
        /// FIN DE TRAITEMENT
        ///<para>Mise à jour de la delande dans la table POSREQUEST</para>
        ///<para>─────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para> ► Id User et date de mise à jour (IDAUPD et DTUPD)</para>
        ///<para> ► Id du LOG associé</para>
        ///<para> ► Statut</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pIdPR">Id de la demande</param>
        /// <param name="pCodeReturn">Code retour du traitement</param>
        /// <returns>Cst.ErrLevel</returns>
        /// FI 20170406 [23053] Méthode void
        protected void UpdatePosRequestTerminated(int pIdPR, ProcessStateTools.StatusEnum pStatus)
        {
            if (Queue.header.requesterSpecified)
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CS, "IDPR", DbType.Int32), pIdPR);
                
                //parameters.Add(new DataParameter(CS, "IDPROCESS_L", DbType.Int32), ProcessBase.processLog.header.IdProcess);
                parameters.Add(new DataParameter(CS, "IDPROCESS_L", DbType.Int32), ProcessBase.IdProcess);
                parameters.Add(new DataParameter(CS, "STATUS", DbType.String, SQLCst.UT_STATUS_LEN), pStatus.ToString());
                parameters.Add(new DataParameter(CS, "IDAUPD", DbType.Int32), ProcessBase.Session.IdA);
                // FI 20200820 [25468] dats systemes en UTC
                parameters.Add(new DataParameter(CS, "DTUPD", DbType.DateTime), OTCmlHelper.GetDateSysUTC(CS));

                // Mise à jour de POSREQUEST avec IDREQUEST_L
                string sqlQuery = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.POSREQUEST.ToString();
                sqlQuery += SQLCst.SET + "IDPROCESS_L = @IDPROCESS_L, STATUS = @STATUS, DTUPD = @DTUPD, IDAUPD = @IDAUPD" + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + "(IDPR = @IDPR)";
                _ = DataHelper.ExecuteNonQuery(Queue.ConnectionString, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
            }
        }
        #endregion UpdatePosRequestTerminated
        #region UpdateTradeStUsedBy
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private Cst.ErrLevel UpdateTradeStUsedBy(string pCS, int pIdT, Cst.StatusUsedBy pStatus, string pLibStUsedBy)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCS, "IDSTUSEDBY", DbType.AnsiString), pStatus);
            parameters.Add(new DataParameter(pCS, "IDASTUSEDBY", DbType.Int32), m_PKGenProcess.Session.IdA);
            parameters.Add(new DataParameter(pCS, "DTSTUSEDBY", DbType.DateTime), OTCmlHelper.GetDateSysUTC(CS));
            parameters.Add(new DataParameter(pCS, "LIBSTUSEDBY", DbType.AnsiString), String.IsNullOrEmpty(pLibStUsedBy) ? Convert.DBNull : pLibStUsedBy);

            string sqlUpdate = @"update dbo.TRADE set
            IDSTUSEDBY = @IDSTUSEDBY, IDASTUSEDBY = @IDASTUSEDBY, DTSTUSEDBY = @DTSTUSEDBY, LIBSTUSEDBY = @LIBSTUSEDBY
            where (IDT = @IDT)";

            QueryParameters queryParameters = new QueryParameters(pCS, sqlUpdate, parameters);
            _ = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UpdateTradeStUsedBy

        #region ValuationAmountGen
        /// <summary>
        /// Appel à EVENTSVAL pour CALCUL DES MARGES des NEGOCIATIONS EN POSITIONS (EOD / POC / POT / UNCLEARING)
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        //  EG 20170412 [23081] add parameter : pDbTransaction 
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Upd GetQuote
        // EG 20240115 [WI808] Upd parameters pTrade & pAsset
        private Cst.ErrLevel ValuationAmountGen(IDbTransaction pDbTransaction, (int id, string identifier) pTrade, (int id, string identifier, Cst.UnderlyingAsset underlyer) pAsset, bool pIsNoLockcurrentId)
        {
            bool isEOD = (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay);
            SystemMSGInfo errReadOfficialClose = null;
            Cst.ErrLevel codeReturn;
            if (m_PKGenProcess.ProcessCacheContainer.GetQuote(pAsset.id, m_MasterPosRequest.DtBusiness, pAsset.identifier, 
                QuotationSideEnum.OfficialClose, pAsset.underlyer, new KeyQuoteAdditional(), ref errReadOfficialClose) is Quote quote)
            {
                quote.action = DataRowState.Added.ToString();
                quote.isEODSpecified = true;
                quote.isEOD = isEOD;

                MQueueAttributes mQueueAttributes = new MQueueAttributes()
                {
                    connectionString = CS,
                    id = pTrade.id,
                    requester = Queue.header.requester
                };

                EventsValMQueue mQueue = new EventsValMQueue(mQueueAttributes, quote)
                {
                    idInfo = new IdInfo()
                    {
                        id = pTrade.id,
                        idInfos = new DictionaryEntry[]{
                                new DictionaryEntry("ident", "TRADE"),
                                new DictionaryEntry("identifier", pTrade.identifier),
                                new DictionaryEntry("GPRODUCT", GProduct)}
                    },
                    idInfoSpecified = true
                };

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 600), 1, new LogParam(LogTools.IdentifierAndId(pTrade.identifier, pTrade.id))));

                //  EG 20170412 [23081] add parameter : pDbTransaction 
                ProcessState processState = New_EventsValAPI.ExecuteSlaveCall(pDbTransaction, mQueue, m_PKGenProcess, m_PKGenProcess.ProcessCacheContainer, pIsNoLockcurrentId, false);
                // EG 20150305 
                //if (false == ProcessStateTools.IsCodeReturnSuccess(processState.CodeReturn))
                codeReturn = processState.CodeReturn;
                if ((Cst.ErrLevel.SUCCESS != codeReturn) &&
                    (Cst.ErrLevel.FAILUREWARNING != codeReturn))
                {
                    if (Cst.ErrLevel.LOCKUNSUCCESSFUL == codeReturn)
                    {
                        int stepLock = 0;
                        while ((Cst.ErrLevel.LOCKUNSUCCESSFUL == codeReturn) && (stepLock < 5))
                        {
                            //  EG 20170412 [23081] add parameter : pDbTransaction 
                            _ = New_EventsValAPI.ExecuteSlaveCall(pDbTransaction, mQueue, m_PKGenProcess, m_PKGenProcess.ProcessCacheContainer, pIsNoLockcurrentId, false);
                            stepLock++;
                        }
                    }
                    else
                        codeReturn = Cst.ErrLevel.FAILURE;
                }
            }
            else if ((null != errReadOfficialClose) && isEOD)
            {
                codeReturn = Cst.ErrLevel.FAILURE;

                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialClose.processState.Status);

                
                Logger.Log(errReadOfficialClose.ToLoggerData(0));
                
                Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(errReadOfficialClose.processState.Status), new SysMsgCode(SysCodeEnum.SYS, 5161), 0, 
                    new LogParam(LogTools.IdentifierAndId(pTrade.identifier, pTrade.id))));
            }

            else
                codeReturn = Cst.ErrLevel.FAILURE;
            return codeReturn;
        }
        #endregion ValuationAmountGen

        #region WeightingContextRule
        /// <summary>
        /// Tri final des contextes de Merge
        /// Principe :
        /// Un contexte doit posséder des trades à merger pour calculer son poids.
        /// Le poids est calculé sur le 1er trade à merger de la 1ere clé de trade.
        /// Un tri par poids des contextes est appliqué à la fin de la boucle.
        /// </summary>
        /// <returns></returns>
        private Cst.ErrLevel WeightingContextRule()
        {
            m_LstTradeMergeRule.ForEach(tmr =>
            {
                tmr.ResultMatching = 1;
                if ((null != tmr.IdT_Mergeable) && (0 < tmr.IdT_Mergeable.Keys.Count))
                {
                    TradeKey key = tmr.IdT_Mergeable.Keys.First();
                    if (null != key)
                    {
                        int _idT = tmr.IdT_Mergeable[key].First().First;
                        tmr.Weighting(m_DicTradeCandidate[_idT].Context);
                    }
                }
                else
                {
                    tmr.ResultMatching = 0;
                }
            });
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion WeightingContextRule

        /// <summary>
        /// Calcul des CashFlows en mode Multi thread
        /// </summary>
        /// <param name="pRows">Liste des trades candidats</param>
        /// <returns></returns>
        /// FI 20190709 [24752] Add Method
        /// EG 20191014 Valorisation isQuoteNotFound : Ajout test QTY > 0
        /// EG 20191014 Tri de cashFlowsInfo (row["IDASSET"] descending, row["QTY"] descending)
        private Pair<Cst.ErrLevel, ProcessState> CashFlowsGenMultiThreading(DataRowCollection pRows)
        {

            Pair<Cst.ErrLevel, ProcessState> lRet = new Pair<Cst.ErrLevel, ProcessState>(
                Cst.ErrLevel.SUCCESS,
                new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS)
            );

            AppInstance.TraceManager.TraceVerbose(this, "START Construct List Queue");
            Task<IEnumerable<EventsValMQueue>> ret = GetListQueueForEODCashFlowsAsync(pRows.Cast<DataRow>().ToList());
            List<EventsValMQueue> lstEventsValMQueue = ret.Result.ToList();
            AppInstance.TraceManager.TraceVerbose(this, "STOP Construct List Queue");

            
            Logger.Write();
            

            IEnumerable<Pair<EventsValMQueue, DataRow>> cashFlowsInfo =
                        (from DataRow row in pRows
                         orderby row["IDASSET"] descending, row["QTY"] descending
                        select new Pair<EventsValMQueue, DataRow>(lstEventsValMQueue.Find(q => q.idInfo.id == Convert.ToInt32(row["IDT"])), row));
            Boolean isQuoteNotFound = (null != cashFlowsInfo.Where(x => (false == x.First.quote.valueSpecified) && (0 < Convert.ToDecimal(x.Second["QTY"]))).FirstOrDefault());

            ProcessState processState = CommonGenThreading(ParallelProcess.CashFlows, cashFlowsInfo.ToList(), null);
            lRet.Second = processState;
            if (processState.CodeReturn == Cst.ErrLevel.IRQ_EXECUTED)
                lRet.First = processState.CodeReturn;

            // Lorsque qu'il existe des erreurs de lecture de quotation, le status est en warning 
            if ((lRet.First == Cst.ErrLevel.SUCCESS) && isQuoteNotFound)
            {
                lRet.Second.Status = ProcessStateTools.StatusEnum.WARNING;
                lRet.Second.CodeReturn = Cst.ErrLevel.QUOTENOTFOUND;
            }

            return lRet;
        }
        /// <summary>
        /// Pose un lock exclusif sur POSACTIONDET. Returne null si le lock n'a pu être posé (timeout de 30 secondes).
        /// <para></para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdPADet"></param>
        /// <param name="pAction"></param>
        /// <returns></returns>
        /// FI 20201117 [24872] Add Method
        private LockObject LockPosActionDet(string pCS, IDbTransaction pDbTransaction, int pIdPADet, string pAction)
        {
            LockObject lockObject = new LockObject(TypeLockEnum.POSACTIONDET, pIdPADet, null, LockTools.Exclusive);
            Lock lck = new Lock(pCS, pDbTransaction, lockObject, this.ProcessBase.Session, pAction);
            if (LockTools.LockMode2(lck, out _))
                return lockObject;
            else
                return null;
        }
        /// <summary>
        /// Construction d'une chaine de constitution des données d'un trade (Identifier et id) pour 
        /// alimentation du Logger dans le service ClosingGen
        /// </summary>
        /// <param name="pPosKeepingData"></param>
        /// <param name="pPosRequest"></param>
        /// <returns></returns>
        /// EG 20220627 [26082] New
        public string TradeForLog (IPosKeepingData pPosKeepingData, IPosRequest pPosRequest)
        {
            string ret;
            if (pPosKeepingData.TradeSpecified)
                ret = LogTools.IdentifierAndId(pPosKeepingData.Trade.Identifier, pPosKeepingData.Trade.IdT);
            else if (pPosRequest.IdentifiersSpecified)
                ret = LogTools.IdentifierAndId(pPosRequest.Identifiers.Trade, pPosRequest.IdTSpecified ? pPosRequest.IdT : 0);
            else
                ret = LogTools.IdentifierAndId(string.Empty, pPosRequest.IdTSpecified ? pPosRequest.IdT : 0);

            return ret;
        }
        #endregion Methods
    }
}

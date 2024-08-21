#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML;
using EfsML.Business;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process
{
    #region QuoteInfo
    public abstract class QuoteInfo
    {
        #region Members
        public DataRowState rowState;
        public DateTime dtBusiness;
        public DateTime dtQuote;
        public Quote quote;
        public SystemMSGInfo quoteMsgInfo;
        protected ProcessCacheContainer processCacheContainer;
        #endregion Members
        #region Constructors
        public QuoteInfo() { }
        // EG 20180205 [23769] Use function with lock
        public QuoteInfo(EventsValProcessBase pProcess, NextPreviousEnum pType, Nullable<DateTime> pDate)
        {
            Quote quoteSource = pProcess.Quote;
            processCacheContainer = pProcess.EventsValProcess.ProcessCacheContainer;
            rowState = DataRowState.Added;
            if (pDate.HasValue)
                dtBusiness = pDate.Value;
            if (null != quoteSource)
            {
                rowState = (DataRowState)Enum.Parse(typeof(DataRowState), quoteSource.action);
                dtBusiness = quoteSource.time.Date;
            }

            switch (pType)
            {
                case NextPreviousEnum.Next:
                    dtBusiness = processCacheContainer.GetBusinessDayLock(dtBusiness, BusinessDayConventionEnum.FOLLOWING, pProcess.EntityMarketInfo.IdBC);
                    break;
                case NextPreviousEnum.Previous:
                    if (dtBusiness == pProcess.EntityMarketInfo.DtMarket)
                        dtBusiness = pProcess.EntityMarketInfo.DtMarketPrev;
                    else
                        dtBusiness = processCacheContainer.GetBusinessDayLock(dtBusiness, BusinessDayConventionEnum.PRECEDING, pProcess.EntityMarketInfo.IdBC);
                    break;
                default:
                    break;
            }
        }
        #endregion Constructors
        #region Methods
        #region BusinessDayControl
        // EG 20180205 [23769] Use function with lock
        // RD 20200911 [25475] Set code return to Warning and improve message
        public void BusinessDayControl(EventsValProcessBase pProcess)
        {
            if (dtQuote.Date != processCacheContainer.GetBusinessDayLock(dtQuote.Date, BusinessDayConventionEnum.NONE, pProcess.EntityMarketInfo.IdBC))
            {
                // Date invalide (car fériée)=> génération d'une exception
                #region Log error message
                Quote quoteSource = pProcess.Quote;
                string errMsg = "<b>" + DtFunc.DateTimeToStringDateISO(dtQuote) + " is an Exchange holiday</b>";
                string quoteLogMessage = string.Empty;
                if (null != quoteSource)
                {
                    quoteLogMessage += "- Asset: <b>" + quoteSource.idAsset_Identifier + " [id: " + quoteSource.idAsset.ToString() + "]" + "</b>" + Cst.CrLf;
                    quoteLogMessage += "- Type: <b>" + (quoteSource.QuoteType.HasValue ? quoteSource.QuoteType.ToString() : "{null}") + "</b>" + Cst.CrLf;
                    quoteLogMessage += "- QuoteTiming: <b>" + (quoteSource.QuoteTiming.HasValue ? quoteSource.QuoteTiming.ToString() : "{null}") + "</b>" + Cst.CrLf;
                    quoteLogMessage += "- Time: <b>" + DtFunc.DateTimeToStringISO(dtQuote) + "</b>" + Cst.CrLf;
                    quoteLogMessage += "- Market Environment: <b>" + (StrFunc.IsFilled(quoteSource.idMarketEnv) ? quoteSource.idMarketEnv.ToString() : "{null}") + "</b>" + Cst.CrLf;
                    quoteLogMessage += "- Val. Scenario: <b>" + (StrFunc.IsFilled(quoteSource.idValScenario) ? quoteSource.idValScenario.ToString() : "{null}") + "</b>" + Cst.CrLf;
                }
                errMsg = StrFunc.GetProcessLogMessage(errMsg, quoteLogMessage);
                ProcessState processState = new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.FAILUREWARNING);
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, errMsg, processState);
                #endregion Log error message
            }
        }
        #endregion BusinessDayControl
        #region InitQuote
        // EG 20190716 [VCL : New FixedIncome] Upd (Use KeyQuoteAdditional)
        // EG 20190730 Upd (parameter pQuoteNotFoundIsLog) 
        public void InitQuote(EventsValProcessBase pProcess)
        {
            InitQuote(pProcess, true);
        }
        // EG 20190730 Upd (parameter pQuoteNotFoundIsLog) 
        public void InitQuote(EventsValProcessBase pProcess, bool pQuoteNotFoundIsLog)
        {
            Quote quoteSource = pProcess.Quote;
            KeyQuoteAdditional keyQuoteAdditional = new KeyQuoteAdditional(quoteSource, pQuoteNotFoundIsLog);
            quote = processCacheContainer.GetQuoteLock(quoteSource.idAsset, dtQuote, quoteSource.idAsset_Identifier,
                quoteSource.QuoteType.Value, quoteSource.UnderlyingAsset,keyQuoteAdditional, ref quoteMsgInfo);
            quote.action = rowState.ToString();
            if (quoteMsgInfo != null && quoteMsgInfo.processState.CodeReturn != Cst.ErrLevel.SUCCESS)
                throw new SpheresException2(quoteMsgInfo.processState);
        }
        #endregion InitQuote

        #region SetQuote
        // EG 20150306 [POC-BERKELEY] : Gestion ESE
        // EG 20180502 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190308 Upd ProcessState WARNING
        // EG 20190716 [VCL : New FixedIncome] Upd GetQuoteLock
        // EG 20190925 [24949] Upd Initialisation KeyQuoteAdditional en fonction type de Quote
        public bool SetQuote(EventsValProcessBase pProcess)
        {

            bool isQuoteFound = true;
            Quote quoteSource = pProcess.Quote;
            try
            {
                if (null != quoteSource)
                {
                    //quote = processCacheContainer.GetQuoteLock(quoteSource.idAsset, dtQuote, quoteSource.idAsset_Identifier,
                    //    quoteSource.QuoteType.Value, quoteSource.UnderlyingAsset, new KeyQuoteAdditional(quoteSource), ref quoteMsgInfo);
                    // EG 20190925 [24949] La recherche du prix veille d'un asset ETD dont le prix jour avait déjà été lu réutilisait les paramètre du prix jour.
                    // Si le prix jour avait MarketQuote comme AssetMeasure on cherchait le prix veille avec MarketQuote. Si le prix n'avait pas cette donnée de renseingé
                    // il était logiquement déclaré comme non trouvé.
                    KeyQuoteAdditional keyQuoteAdditional = null;
                    if (pProcess.IsQuote_DebtSecurity)
                        keyQuoteAdditional = new KeyQuoteAdditional(quoteSource);
                    else
                        keyQuoteAdditional = new KeyQuoteAdditional();
                    quote = processCacheContainer.GetQuoteLock(quoteSource.idAsset, dtQuote, quoteSource.idAsset_Identifier,
                        quoteSource.QuoteType.Value, quoteSource.UnderlyingAsset, keyQuoteAdditional, ref quoteMsgInfo);
                }
                else
                {
                    int assetId = 0;
                    string assetIdentifier = string.Empty;
                    Nullable<Cst.UnderlyingAsset> underlyingAsset = null;
                    if (pProcess is EventsValProcessETD)
                    {
                        EventsValProcessETD _process = pProcess as EventsValProcessETD;
                        assetId = _process.ETDContainer.AssetETD.Id;
                        assetIdentifier = _process.ETDContainer.AssetETD.Identifier;
                        underlyingAsset = Cst.UnderlyingAsset.ExchangeTradedContract;
                    }
                    else if (pProcess is EventsValProcessRTS)
                    {
                        EventsValProcessRTS _process = pProcess as EventsValProcessRTS;
                        assetId = _process.ReturnSwapContainer.IdAssetReturnLeg.Value;
                        assetIdentifier = _process.ReturnSwapContainer.Identifier;
                        underlyingAsset = _process.ReturnSwapContainer.MainReturnLeg.Second.UnderlyerAsset;
                    }
                    else if (pProcess is EventsValProcessESE)
                    {
                        EventsValProcessESE _process = pProcess as EventsValProcessESE;
                        assetId = _process.EquitySecurityTransactionContainer.AssetEquity.Id;
                        assetIdentifier = _process.EquitySecurityTransactionContainer.AssetEquity.Identifier;
                        underlyingAsset = Cst.UnderlyingAsset.EquityAsset;
                    }
                    quote = processCacheContainer.GetQuoteLock(assetId, dtQuote, assetIdentifier, 
                        QuotationSideEnum.OfficialClose, underlyingAsset.Value, new KeyQuoteAdditional(), ref quoteMsgInfo);
                }
                quote.action = rowState.ToString();
                if ((null != quoteMsgInfo) && (quoteMsgInfo.processState.CodeReturn != Cst.ErrLevel.SUCCESS))
                    throw new SpheresException2(quoteMsgInfo.processState);
            }
            catch (SpheresException2 ex)
            {
                bool isThrow = true;
                if (false == ProcessStateTools.IsCodeReturnUndefined(ex.ProcessState.CodeReturn))
                {
                    isQuoteFound = false;
                    if ((null != quoteSource) && quoteSource.isEODSpecified && quoteSource.isEOD)
                    {
                        quoteMsgInfo.processState.Status = ProcessStateTools.StatusEnum.WARNING;
                        if (dtQuote > pProcess.EntityMarketInfo.DtMarket)
                            isThrow = false;
                    }
                    else if (dtQuote >= pProcess.EntityMarketInfo.DtMarket)
                        isThrow = false;

                    if (isThrow && (null != quoteMsgInfo))
                    {
                        
                        Logger.Log(quoteMsgInfo.ToLoggerData(0));

                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05160", quoteMsgInfo.processState,
                            LogTools.IdentifierAndId(pProcess.EventsValMQueue.GetStringValueIdInfoByKey("identifier"), pProcess.EventsValMQueue.id));
                    }
                }
                else if (isThrow)
                {
                    throw;
                }
            }
            return isQuoteFound;
        }
        #endregion SetQuote
        #endregion Methods
    }
    #endregion QuoteInfo

    /// <summary>
    /// 
    /// </summary>
    public class EventsValProcess : ProcessTradeBase
    {
        #region Members
        private EventsValProcessBase eventsValProcessBase;
        private DataSetTrade m_DsTrade;
        private EFS_TradeLibrary m_TradeLibrary;
        private string m_BizCmptLevel;
        #endregion Members

        #region Accessors
        public EventsValProcessBase EventsValProcessBase
        {
            get { return eventsValProcessBase; }
        }
        
        /// <summary>
        /// Le traitement de VALO ne poste pas de message vers d'autres traitements si le trade est de type ASSET
        /// Exemple les TITRES. Il n'y a pas de SI etc... 
        /// </summary>
        protected override bool IsProcessSendMessage
        {
            get
            {
                bool ret;
                if ((null != m_TradeLibrary) && m_TradeLibrary.DataDocument.CurrentProduct.Product.ProductBase.IsASSET)
                    ret = false;
                else
                    ret = base.IsProcessSendMessage;
                return ret;
            }
        }

        /// <summary>
        /// Contient l'éventuel n° de version dont on souhaite assurer la compatibilité business. 
        /// </summary>
        public string BizCmptLevel 
        {
            get { return m_BizCmptLevel; }
        }
        #endregion Accessors

        #region Constructor
        public EventsValProcess(MQueueBase pMQueue, AppInstanceService pAppInstance) : base(pMQueue, pAppInstance) { }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Use slaveDbTransaction
        // EG 20201006 [25350] Contrôle Traitement en cours (EOD,CB, MR) pour éviter doublons des événements lors de la mise à jour des prix via QUOTEHANDLING
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            //PL 20131010 New feature
            m_BizCmptLevel = (string)SystemSettings.GetAppSettings("BizCmptLevel", typeof(string), string.Empty); 

            // Chargement du trade
            if (ProcessCall == ProcessCallEnum.Master)
            {
                
                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 600), 0, new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));
            }

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 501), 4,
                new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                new LogParam(MQueue.GetStringValueIdInfoByKey("GPRODUCT"))));

            #region Select Trade and deserialization
            m_DsTrade = new DataSetTrade(Cs, SlaveDbTransaction, CurrentId, TradeTable);
            m_TradeLibrary = new EFS_TradeLibrary(Cs, SlaveDbTransaction, m_DsTrade.IdT);
            #endregion Select Trade and deserialization

            // EG 20201006 [25350] Via QUOTEHANDLING IDEM est passé en paramètre dans le message
            int idEM = MQueue.GetIntValueIdInfoByKey("IDEM");
            if (0 < idEM)
            {
                LockObject lockEntityMarket = new LockObject(TypeLockEnum.ENTITYMARKET, idEM, MQueue.Identifier, LockTools.Exclusive);
                // Contrôle Lock ENTITYMARKET
                string lockMsg = LockTools.SearchProcessLocks(Cs, null, lockEntityMarket, this.Session);
                if (StrFunc.IsFilled(lockMsg))
                {
                    codeReturn = Cst.ErrLevel.DATAREJECTED;
                    ProcessState.SetErrorWarning(ProcessStateTools.StatusEnum.WARNING);
                    
                    Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(ProcessState.Status), new SysMsgCode(SysCodeEnum.SYS, 690), 2,
                    new LogParam(lockMsg),
                    new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                    new LogParam(MQueue.GetStringValueIdInfoByKey("GPRODUCT"))));
                }
            }
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = ProcessExecuteProduct(m_TradeLibrary.Product);
            
            return codeReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20180502 Analyse du code Correction [CA2214]
        /// EG 202207010 [XXXXX] Pas de Throw si eventsValProcessBase est nul OTCmlId peut être négatif [Corrections Diverses (Demo OTC/BFF)]
        protected override Cst.ErrLevel ProcessExecuteSimpleProduct(IProduct pProduct)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            eventsValProcessBase = null;
            // PM 20120824 [18058] Add pProduct.productBase.IsCashBalanceInterest in test 
            if (pProduct.ProductBase.IsIRD || pProduct.ProductBase.IsCashBalanceInterest)
                eventsValProcessBase = new EventsValProcessIRD(this, m_DsTrade, m_TradeLibrary, pProduct);
            else if (pProduct.ProductBase.IsFx)
                eventsValProcessBase = new EventsValProcessFX(this, m_DsTrade, m_TradeLibrary, pProduct);
            else if (pProduct.ProductBase.IsADM)
                eventsValProcessBase = new EventsValProcessADM(this, m_DsTrade, m_TradeLibrary, pProduct);
            else if (pProduct.ProductBase.IsExchangeTradedDerivative)
                eventsValProcessBase = new EventsValProcessETD(this, m_DsTrade, m_TradeLibrary, pProduct);
            else if (pProduct.ProductBase.IsReturnSwap)
                eventsValProcessBase = new EventsValProcessRTS(this, m_DsTrade, m_TradeLibrary, pProduct);
            else if (pProduct.ProductBase.IsEquitySecurityTransaction)
                eventsValProcessBase = new EventsValProcessESE(this, m_DsTrade, m_TradeLibrary, pProduct);
            else if (pProduct.ProductBase.IsDebtSecurityTransaction)
                eventsValProcessBase = new EventsValProcessDST(this, m_DsTrade, m_TradeLibrary, pProduct);
            else if (pProduct.ProductBase.IsSEC)
                eventsValProcessBase = new EventsValProcessSEC(this, m_DsTrade, m_TradeLibrary, pProduct);

            //if (null == eventsValProcessBase)
            //    throw new InvalidProgramException("eventsValProcessBase is null");

            if (null != eventsValProcessBase)
            {
                eventsValProcessBase.EndOfInitialize();
                codeReturn = eventsValProcessBase.Valorize();
            }
            return codeReturn;
        }
        #endregion Methods
    }
    
    /// <summary>
    /// 
    /// </summary>
    // EG 20180205 [23769] Add partial
    public static partial class New_EventsValAPI
    {

        /// <summary>
        /// Execution of a EventsVal process from calling Process
        /// </summary>
        /// <param name="pQueue">Execution parameters</param>
        /// <param name="ProcessBase">Reference at calling Process</param>
        /// <returns></returns>
        // EG 20170412 [23081] Set parameter pDbTransaction = null
        public static ProcessState ExecuteSlaveCall(EventsValMQueue pQueue, ProcessBase pCallProcess, bool pIsNoLockcurrentId, bool pIsSendMessage_PostProcess)
        {
            EventsValProcess process = new EventsValProcess(pQueue, pCallProcess.AppInstance);
            return StartProcessSlave(null, process, pCallProcess, pIsNoLockcurrentId, pIsSendMessage_PostProcess);
        }
        public static ProcessState ExecuteSlaveCall(IDbTransaction pDbTransaction, EventsValMQueue pQueue, ProcessBase pCallProcess,
            ProcessCacheContainer pProcessCacheContainer, bool pIsNoLockcurrentId, bool pIsSendMessage_PostProcess)
        {
            EventsValProcess process = new EventsValProcess(pQueue, pCallProcess.AppInstance)
            {
                ProcessCacheContainer = pProcessCacheContainer
            };
            return StartProcessSlave(pDbTransaction, process, pCallProcess, pIsNoLockcurrentId, pIsSendMessage_PostProcess);
        }
        // EG 20170412 [23081] Add parameter pDbTransaction
        // EG 20190613 [24683] Set Tracker to Slave
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20220324 [XXXXX] UPD Set pProcess.TradeTable with TradeTableEnum.TRADEXML
        private static ProcessState StartProcessSlave(IDbTransaction pDbTransaction, EventsValProcess pProcess, ProcessBase pCallProcess, bool pIsNoLockcurrentId, bool pIsSendMessage_PostProcess)
        {
            
            pProcess.ProcessCall = ProcessBase.ProcessCallEnum.Slave;
            pProcess.IsDataSetTrade_AllTable = false;
            pProcess.TradeTable = TradeTableEnum.TRADEXML;
            pProcess.IsSendMessage_PostProcess = pIsSendMessage_PostProcess;
            
            pProcess.IdProcess = pCallProcess.IdProcess;
            //
            pProcess.LogDetailEnum = pCallProcess.LogDetailEnum;
            pProcess.NoLockCurrentId = pIsNoLockcurrentId;
            pProcess.SlaveDbTransaction = pDbTransaction;
            // FI 20190605 [XXXXX] valorisation du tracker
            pProcess.Tracker = pCallProcess.Tracker;
            pProcess.ProcessStart();
            if (null != pProcess.EventsValProcessBase)
                pProcess.EventsValProcessBase.SlaveCallDispose();
            return pProcess.ProcessState;
        }

    }
}

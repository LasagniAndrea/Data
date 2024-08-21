#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Linq;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;


using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresService;
using EFS.Tuning;

using EfsML;
using EfsML.Business;
using EfsML.Curve;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.PosRequest;

using FixML.Interface;
using FpML.Interface;
using FpML.Enum;
#endregion Using Directives

namespace EFS.Process
{
    #region ESEQuoteInfo
    public class ESEQuoteInfo : QuoteInfo, ICloneable
    {
        #region Members

        #endregion Members
        #region Constructors
        public ESEQuoteInfo() { }
        public ESEQuoteInfo(EventsValProcessESE pProcess, NextPreviousEnum pType) : 
            this(pProcess, pType, pProcess.EquitySecurityTransactionContainer.ClearingBusinessDate.Date) 
        {  
        }
        public ESEQuoteInfo(EventsValProcessESE pProcess, NextPreviousEnum pType, Nullable<DateTime> pDate)
            : base(pProcess, pType, pDate)
        {
            Pair<Cst.UnderlyingAsset, int> _underlyingAsset = pProcess.EquitySecurityTransactionContainer.GetUnderlyingAsset(pProcess.Process.Cs);
            PosKeepingAsset posKeepingAsset = pProcess.EquitySecurityTransactionContainer.ProductBase.CreatePosKeepingAsset(_underlyingAsset.First); 
            posKeepingAsset.idAsset = pProcess.EquitySecurityTransactionContainer.AssetEquity.IdAsset;
            posKeepingAsset.identifier = pProcess.EquitySecurityTransactionContainer.AssetEquity.Identifier;
            dtQuote = posKeepingAsset.GetOfficialCloseQuoteTime(dtBusiness);
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            ESEQuoteInfo clone = new ESEQuoteInfo
            {
                dtBusiness = this.dtBusiness,
                dtQuote = this.dtQuote,
                processCacheContainer = this.processCacheContainer,
                rowState = this.rowState
            };
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion ESEQuoteInfo
    #region EventsValProcessESE
    public class EventsValProcessESE : EventsValProcessBase
    {
        #region Members
        private readonly CommonValParameters m_Parameters;
        private EquitySecurityTransactionContainer m_EquitySecurityTransactionContainer;

        #endregion Members
        #region Accessors

        #region Multiplier
        // EG 20160404 Migration vs2013
        //private decimal Multiplier
        //{
        //    get
        //    {
        //        decimal ret = 1;
        //        return ret;
        //    }
        //}
        #endregion Multiplier
        #region Parameters
        public override CommonValParameters Parameters
        {
            get { return m_Parameters; }
        }
        #endregion Parameters

        #region EquitySecurityTransactionContainer
        public EquitySecurityTransactionContainer EquitySecurityTransactionContainer
        {
            get { return m_EquitySecurityTransactionContainer; }
        }
        #endregion EquitySecurityTransactionContainer
        #endregion Accessors
        #region Constructors
        public EventsValProcessESE(EventsValProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pProcess, pDsTrade, pTradeLibrary,pProduct)
        {
            m_Parameters = new CommonValParametersESE();
            if (ProcessBase.ProcessCallEnum.Master == pProcess.ProcessCall)
                pProcess.ProcessCacheContainer = new ProcessCacheContainer(pProcess.Cs, (IProductBase)pProduct);

        }
        #endregion Constructors
        #region Methods
        #region EndOfInitialize
        // EG 20180502 Analyse du code Correction [CA2214]
        public override void EndOfInitialize()
        {
            if (false == m_tradeLibrary.DataDocument.CurrentProduct.IsStrategy)
            {
                Initialize();
                InitializeDataSetEvent();
            }
        }
        #endregion EndOfInitialize
        #region InitializeDataSetEvent
        /// <summary>
        /// Cette méthode override la méthode virtuelle pour le traitement EOD
        /// Dans ce cas 
        /// 1./ Le nombre de tables EVENTXXX chargées est réduit : EVENT|EVENTCLASS|EVENTDET|EVENTASSET
        /// 2./ La date DTBUSINESS est utilisé pour restreindre le nombre d'EVTS chargé
        /// tels que DtBusiness between DTSTARTADJ and DTENDADJ
        /// </summary>
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        public override void InitializeDataSetEvent()
        {
            m_DsEvents = new DataSetEventTrade(m_CS, SlaveDbTransaction,  ParamIdT);
            Nullable<DateTime> dtBusiness = null;
            if (IsEndOfDayProcess)
                dtBusiness = m_Quote.time;
            // EG 20150617 [20665]
            m_DsEvents.Load(EventTableEnum.Class | EventTableEnum.Detail | EventTableEnum.Asset, dtBusiness, null);
        }
        #endregion InitializeDataSetEvent

        #region CalculationEquitySecurityTransaction
        // EG 201807413 Use eodComplement
        // EG 20190327 [MIGRATION VCL] Correction SlaveDbTransaction sur Query Qty
        private Cst.ErrLevel CalculationEquitySecurityTransaction(ESEQuoteInfo pQuote)
        {
            ESEQuoteInfo _quotePrev = new ESEQuoteInfo(this, NextPreviousEnum.Previous);
            // EG 20141224 [20566]
            // EG 20151102 [20979] Refactoring
            // EG 20190327 [MIGRATION VCL]
            m_PosAvailableQuantity = PosKeepingTools.GetAvailableQuantity(m_EventsValProcess.Cs, SlaveDbTransaction, pQuote.dtBusiness, m_EventsValProcess.CurrentId);
            if ((null != m_Quote) && m_Quote.eodComplementSpecified)
                m_PosQuantityPrevAndActionsDay = m_Quote.eodComplement.posQuantityPrevAndActionsDay;
            else
                m_PosQuantityPrevAndActionsDay = PosKeepingTools.GetPreviousAvailableQuantity(m_EventsValProcess.Cs, SlaveDbTransaction, pQuote.dtBusiness.Date, m_EventsValProcess.CurrentId);

            // RD 20160805 [22415] Add parameter pIsZeroQtyForced = true
            Calculation_LPC_AMT(pQuote, _quotePrev, true);

            #region Calculations at ClearingDate+n <= DtMarket (HORS EOD)
            bool isQuotationToCalc = (false == IsEndOfDayProcess);
            int guard = 0;

            DateTime dtQuotation = _quotePrev.dtQuote;
            ESEQuoteInfo _savQuoteInfo = (ESEQuoteInfo)_quotePrev.Clone();
            while (isQuotationToCalc)
            {
                guard++;
                if (guard == 999)
                {
                    string msgException = "Incoherence during the calculation. Infinite loop detected" + Cst.CrLf;
                    throw new Exception(msgException);
                }
                ESEQuoteInfo _quoteNextInfo = new ESEQuoteInfo(this, NextPreviousEnum.Next, dtQuotation);
                isQuotationToCalc = (_quoteNextInfo.dtBusiness <= m_EntityMarketInfo.DtMarket);
                if (isQuotationToCalc)
                    isQuotationToCalc = _quoteNextInfo.SetQuote(this);

                if (isQuotationToCalc)
                {
                    m_PosAvailableQuantityPrev = m_PosAvailableQuantity;
                    // EG 20151102 [20979] Refactoring
                    // EG 20190327 [MIGRATION VCL] Correction SlaveDbTransaction sur Query Qty
                    m_PosAvailableQuantity = PosKeepingTools.GetAvailableQuantity(m_EventsValProcess.Cs, SlaveDbTransaction, _quoteNextInfo.dtBusiness, m_EventsValProcess.CurrentId);
                    // RD 20160805 [22415] Add parameter pIsZeroQtyForced = false
                    Calculation_LPC_AMT(_quoteNextInfo, _savQuoteInfo, false);
                }
                if ((DataRowState.Modified == _quoteNextInfo.rowState) || (false == isQuotationToCalc))
                    break;

                _savQuoteInfo = (ESEQuoteInfo)_quoteNextInfo.Clone();
                // EG 20150623 SET _quoteNextInfo.dtQuote to dtQuotation
                dtQuotation = _quoteNextInfo.dtQuote;
            }
            #endregion Calculations at ClearingDate+n <= DtMarket (HORS EOD)
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CalculationEquitySecurityTransaction
                
        #region Initialize
        /// <summary>
        /// Alimente les membres m_ReturnSwapContainer,_buyer,_bookBuyer,_seller,_bookSeller de la classe
        /// </summary>
        // EG 201807413 Use eodComplement 
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void Initialize()
        {
            m_EquitySecurityTransactionContainer = new EquitySecurityTransactionContainer(m_CS,(IEquitySecurityTransaction)m_CurrentProduct, TradeLibrary.DataDocument);
            m_EventsValProcess.ProcessCacheContainer.SetAsset(m_EquitySecurityTransactionContainer);

            // Buyer / Seller
            IFixParty buyer = m_EquitySecurityTransactionContainer.GetBuyer();
            IFixParty seller = m_EquitySecurityTransactionContainer.GetSeller();
            if (null == buyer)
                throw new NotSupportedException("buyer is not Found");
            if (null == seller)
                throw new NotSupportedException("seller is not Found");

            IReference buyerReference = buyer.PartyId;
            m_Buyer = TradeLibrary.DataDocument.GetParty(buyerReference.HRef);
            m_BookBuyer = TradeLibrary.DataDocument.GetOTCmlId_Book(m_Buyer.Id);

            IReference sellerReference = seller.PartyId;
            m_Seller = TradeLibrary.DataDocument.GetParty(sellerReference.HRef);
            m_BookSeller = TradeLibrary.DataDocument.GetOTCmlId_Book(m_Seller.Id);

            int idEntity;
            if ((null != m_Quote) && m_Quote.eodComplementSpecified)
            {
                m_IsPosKeeping_BookDealer = m_Quote.eodComplement.isPosKeeping_BookDealer;
                idEntity = m_Quote.eodComplement.idAEntity;
            }
            else
            {
                m_IsPosKeeping_BookDealer = m_EquitySecurityTransactionContainer.IsPosKeepingOnBookDealer(m_CS);
                idEntity = TradeLibrary.DataDocument.GetFirstEntity(CSTools.SetCacheOn(m_CS));
            }

            m_EntityMarketInfo = m_EventsValProcess.ProcessCacheContainer.GetEntityMarketLock(idEntity, m_EquitySecurityTransactionContainer.AssetEquity.IdM, 
                m_EquitySecurityTransactionContainer.IdA_Custodian);
        }
        #endregion Initialize

        #region IsRowMustBeCalculate
        public override bool IsRowMustBeCalculate(DataRow pRow)
        {
            return true;
        }
        #endregion IsRowMustBeCalculate

        #region Calculation_LPC_AMT
        /// <summary>
        /// Génération des évènement UnrealizedMargin, MarketValue, MarginRequirement, TotalMargin
        /// </summary>
        /// <param name="pQuote"></param>
        /// <param name="pQuotePrev"></param>
        /// <param name="pIsZeroQtyForced">Generate Event, even if the quantity is zero</param>
        // EG 20150306 [POC-BERKELEY] : Refactoring
        // EG 20150306 [POC-BERKELEY] : New IsMargining
        // RD 20160805 [22415] Add parameter pIsZeroQtyForced
        // EG 20170412 [23081] Gestion  dbTransaction et SlaveDbTransaction
        // EG 20180502 Analyse du code Correction [CA2200]
        // EG 20180713 Global refactoring
        // EG 20190613 [24683] Upd Calling WriteClosingAmount
        // EG 20190925 Retour arrière CashFlows (Mise en veille de l'appel à )EndOfDayWriteClosingAmountGen
        private void Calculation_LPC_AMT(ESEQuoteInfo pQuote, ESEQuoteInfo pQuotePrev, bool pIsZeroQtyForced)
        {
            DataRow rowEventAMT = GetRowAmountGroup();
            m_ParamInstrumentNo.Value = Convert.ToInt32(rowEventAMT["INSTRUMENTNO"]);
            m_ParamStreamNo.Value = Convert.ToInt32(rowEventAMT["STREAMNO"]);

            Pair<string, int> quotedCurrency = new Pair<string, int>
            {
                First = m_EquitySecurityTransactionContainer.AssetEquity.IdC,
                Second = Tools.GetQuotedCurrencyFactor(CSTools.SetCacheOn(m_CS), null, m_EquitySecurityTransactionContainer.AssetEquity.IdC)
            };

            List<VAL_Event> lstEvents = new List<VAL_Event>()
            {
                PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.UnrealizedMargin, quotedCurrency, pQuote, pQuotePrev, pIsZeroQtyForced),
                PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.MarketValue, quotedCurrency, pQuote, pQuotePrev, pIsZeroQtyForced),
            };

            if (m_IsMargining)
            {
                lstEvents.Add(PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.MarginRequirement, quotedCurrency, pQuote, pQuotePrev, pIsZeroQtyForced));
                lstEvents.Add(PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.TotalMargin, quotedCurrency, pQuote, pQuotePrev, pIsZeroQtyForced));
            }
            WriteClosingAmountGen(lstEvents, pQuote.dtBusiness);
        }
        #endregion Calculation_LPC_AMT
        #region PrepareClosingAmountGen
        /// <summary>
        /// Calcul des montants
        /// </summary>
        // EG 20180711 New
        // EG 20190613 [24683] Use slaveDbTransaction
        private VAL_Event PrepareClosingAmountGen(DataRow pRowEventAMT, string pEventType, Pair<string, int> pQuotedCurrency, ESEQuoteInfo pQuote, ESEQuoteInfo pQuotePrev, bool pIsZeroQtyForced)
        {
            Pair<Nullable<decimal>, string> closingAmount = new Pair<Nullable<decimal>, string>(null, string.Empty);

            VAL_Event @event = new VAL_Event();

            string eventCodeLink = EventCodeLink(EventTypeFunc.Amounts, pEventType, pQuote.quote.QuoteTiming.Value);

            @event.Value = GetRowAmount(pEventType, EventClassFunc.Recognition, m_Quote.QuoteTiming.Value, pQuote.dtBusiness);
            @event.IsNewRow = (null == @event.Value);

            bool isOkToGenerate = IsEndOfDayProcess || ((false == @event.IsNewRow) && (false == IsEndOfDayProcess));
            if (isOkToGenerate)
            {
                Pair<int, Nullable<int>> payer = new Pair<int, Nullable<int>>();
                Pair<int, Nullable<int>> receiver = new Pair<int, Nullable<int>>();

                if (@event.IsNewRow)
                {
                    @event.Value = NewRowEvent2(pRowEventAMT, eventCodeLink, pEventType, pQuote.dtBusiness, pQuote.dtBusiness, m_EventsValProcess.AppInstance);
                    @event.ClassREC = NewRowEventClass(-1, EventClassFunc.Recognition, pQuote.dtBusiness, false);
                    if (EventTypeFunc.IsUnrealizedMargin(pEventType) || EventTypeFunc.IsMarginRequirement(pEventType) || EventTypeFunc.IsMarketValue(pEventType))
                        @event.ClassVAL = NewRowEventClass(-1, EventClassFunc.ValueDate, pQuote.dtBusiness, false);

                    @event.Detail = NewRowEventDet(@event.Value);
                }
                else
                {
                    @event.Detail = GetRowEventDetail(Convert.ToInt32(@event.Value["IDE"]));
                }

                Nullable<decimal> quote = null;
                Nullable<decimal> quote100 = null;
                if (pQuote.quote.valueSpecified)
                {
                    quote = pQuote.quote.value;
                    quote100 = pQuote.quote.value;
                }

                decimal _initialNetPrice = m_EquitySecurityTransactionContainer.Price;
                // EG 20190325 [MIGRATION VCL] 
                //decimal multiplier = Multiplier / pQuotedCurrency.Second;
                decimal multiplier = Multiplier;
                bool isTradeDay = (m_EquitySecurityTransactionContainer.ClearingBusinessDate.Date == pQuote.dtBusiness);

                @event.Detail["DTACTION"] = pQuote.dtQuote;
                @event.Detail["QUOTETIMING"] = pQuote.quote.quoteTiming;
                @event.Detail["QUOTEPRICE"] = quote ?? Convert.DBNull;
                @event.Detail["QUOTEPRICE100"] = quote100 ?? Convert.DBNull;
                @event.Detail["CONTRACTMULTIPLIER"] = multiplier;
                @event.Detail["QUOTEDELTA"] = DBNull.Value;

                @event.Qty = m_PosAvailableQuantity;

                if (EventTypeFunc.IsUnrealizedMargin(pEventType))
                {
                    #region UnrealizedMargin
                    closingAmount = SetUnrealizedMargin(SlaveDbTransaction, @event.Qty, multiplier, pQuotedCurrency.First, quote100, _initialNetPrice, ref payer, ref receiver);
                    @event.Detail["PRICE"] = _initialNetPrice;
                    @event.Detail["PRICE100"] = _initialNetPrice;
                    #endregion UnrealizedMargin
                }
                else if (EventTypeFunc.IsMarketValue(pEventType))
                {
                    #region MarketValue
                    closingAmount = SetMarketValue(SlaveDbTransaction, @event.Qty, multiplier, pQuotedCurrency.First, quote100, ref payer, ref receiver);
                    #endregion MarketValue
                }
                else if (EventTypeFunc.IsMarginRequirement(pEventType))
                {
                    #region MarginRequirement
                    closingAmount = SetMarginRequirement(SlaveDbTransaction, m_EquitySecurityTransactionContainer, @event.Qty, multiplier, pQuotedCurrency.First, pQuote.dtBusiness, quote100, ref payer, ref receiver);
                    #endregion MarginRequirement
                }
                else if (EventTypeFunc.IsTotalMargin(pEventType))
                {
                    #region TotalMargin
                    if (false == isTradeDay)
                        pQuotePrev.SetQuote(this);
                    closingAmount = SetTotalMargin(SlaveDbTransaction, isTradeDay, @event.Qty, _initialNetPrice, multiplier, pQuotedCurrency.First,
                        pQuote.dtBusiness, quote100, pQuotePrev.dtBusiness, (isTradeDay ? 0 : pQuotePrev.quote.value), ref payer, ref receiver);
                    #endregion TotalMargin
                }

                @event.ClosingAmount = closingAmount.First;
                CommonValFunc.SetPayerReceiver(@event.Value, payer.First, payer.Second, receiver.First, receiver.Second);
                @event.Currency = closingAmount.Second;
                @event.SetRowEventClosingAmountGen(m_DsEvents, pQuotedCurrency.Second, pIsZeroQtyForced);
            }
            return @event;
        }
        #endregion PrepareClosingAmountGen

        #region RemoveClosingAmountGen
        private void RemoveClosingAmountGen(DateTime pDate)
        {
        }
        #endregion RemoveClosingAmountGen
        #region Valorize
        // EG 20180502 Analyse du code Correction [CA2214]
        // RD 20200911 [25475] Add try catch in order to Log the Exception
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (m_tradeLibrary.DataDocument.CurrentProduct.IsStrategy)
                return ret;

            try
            {
                //Initialize();
                // EG 20150617 [20665]
                //InitializeDataSetEvent();

                ESEQuoteInfo _quoteInfo = new ESEQuoteInfo(this, NextPreviousEnum.None);

                //Suppression d'un prix
                if ((null != _quoteInfo) && (_quoteInfo.rowState == DataRowState.Deleted))
                {
                    _quoteInfo.SetQuote(this);
                    RemoveClosingAmountGen(_quoteInfo.dtBusiness);
                }

                if (_quoteInfo.rowState != DataRowState.Deleted)
                {
                    bool isFirstQuotationFound = true;
                    if (false == IsEndOfDayProcess)
                    {
                        //Spheres® vérifie que la date de cotation ne correspond pas à un jour férié
                        _quoteInfo.BusinessDayControl(this);
                        isFirstQuotationFound = _quoteInfo.SetQuote(this);
                    }
                    else
                    {
                        _quoteInfo.InitQuote(this);
                    }

                    bool isCalculation = (false == _quoteInfo.quote.QuoteTiming.HasValue) || (QuoteTimingEnum.Close == _quoteInfo.quote.QuoteTiming.Value);

                    if (isCalculation)
                        ret = CalculationEquitySecurityTransaction(_quoteInfo);
                }
            }
            catch (SpheresException2 ex)
            {
                Logger.Log(new LoggerData(ex));
                throw ex;
            }
            catch (Exception ex)
            {
                SpheresException2 sphEx = new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
                Logger.Log(new LoggerData(sphEx));
                throw sphEx;
            }

            return ret;
        }
        #endregion Valorize

        #endregion Methods
    }
    #endregion EventsValProcessESE
}

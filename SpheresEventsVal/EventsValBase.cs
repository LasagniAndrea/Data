#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FixML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
#endregion Using Directives

namespace EFS.Process
{
    #region EventsValProcessBase
    // EG 20150306 [POC-BERKELEY] : Déplacement Buyer|Seller (en provenance des classes EventsValRTS|EventsValESE|EventsValETD
    // EG 20150306 [POC-BERKELEY] : Déplacement IsPosKeeping_BookDealer|EntityMarketInfo|PosAvailableQuantity etc.
    // EG 20150306 [POC-BERKELEY] : New IsMargining|Isfunding
    // EG 20190613 [24683] Add m_UpdateLock
    // EG 20231127 [WI749] Implementation Return Swap : Add m_IsFungible
    // EG 20231127 [WI749] Implementation Return Swap : Refactoring Code Analysis
    public abstract class EventsValProcessBase : CommonValProcessBase
    {
        #region Members
        private static readonly object m_UpdateLock = new object();
        protected EventsValProcess m_EventsValProcess;
        protected EventsValMQueue m_EventsValMQueue;

        protected bool m_IsPosKeeping_BookDealer;
        protected bool m_IsMargining;
        protected bool m_IsFunding;
        protected bool m_IsFungible;

        // EntityMarket
        protected IPosKeepingMarket m_EntityMarketInfo;
        //Représente la quantité disponible jour
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        protected decimal m_PosAvailableQuantity;
        //Représente la quantité disponible veille
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        protected decimal m_PosAvailableQuantityPrev;
        //Représente la quantité disponible : 
        //quantité disponible veille +/- celles des actions de correction du jour (Correction - POC , Transfert - POT , Décompensation - UNCLEARING)
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        protected decimal m_PosQuantityPrevAndActionsDay;
        #endregion Members
        #region Accessors
        // EG 20231127 [WI749] Implementation Return Swap : New
        public bool IsFungible
        {
            get { return m_IsFungible; }
        }
        #region CommonValDate
        public override DateTime CommonValDate
        {
            get
            {
                if (m_Process.MQueue.IsMasterDateSpecified)
                    return m_Process.MQueue.GetMasterDate();
                return DateTime.MinValue;
            }
        }
        #endregion CommonValDate
        #region EventsValMQueue
        public EventsValMQueue EventsValMQueue
        {
            set
            {

                m_EventsValMQueue = value;
                if (m_EventsValMQueue.quoteSpecified && (null != m_EventsValMQueue.quote))
                {
                    Type tQuote = m_EventsValMQueue.quote.GetType();
                    if (tQuote.Equals(typeof(Quote_FxRate)))
                        m_Quote = (Quote_FxRate)m_EventsValMQueue.quote;
                    else if (tQuote.Equals(typeof(Quote_RateIndex)))
                        m_Quote = (Quote_RateIndex)m_EventsValMQueue.quote;
                    else if (tQuote.Equals(typeof(Quote_SecurityAsset)))
                        m_Quote = (Quote_SecurityAsset)m_EventsValMQueue.quote;
                    else if (tQuote.Equals(typeof(Quote_ETDAsset)))
                        m_Quote = (Quote_ETDAsset)m_EventsValMQueue.quote;
                    else if (tQuote.Equals(typeof(Quote_Equity)))
                        m_Quote = (Quote_Equity)m_EventsValMQueue.quote;
                    else if (tQuote.Equals(typeof(Quote_Index)))
                        m_Quote = (Quote_Index)m_EventsValMQueue.quote;
                    else if (tQuote.Equals(typeof(Quote_DebtSecurityAsset)))
                        m_Quote = (Quote_DebtSecurityAsset)m_EventsValMQueue.quote;
                    else if (tQuote.Equals(typeof(Quote_SimpleIRSwap)))
                        m_Quote = (Quote_SimpleIRSwap)m_EventsValMQueue.quote;
                    else
                        m_Quote = (Quote_ToDefine)m_EventsValMQueue.quote;

                }
                /*
                if (null != m_EventsValMQueue.quote)
                {
                    Type tQuote = m_EventsValMQueue.quote.GetType();
                    if (tQuote.Equals(typeof(Quote_FxRate)))
                        m_Quote_FxRate = (Quote_FxRate)m_EventsValMQueue.quote;
                    else if (tQuote.Equals(typeof(Quote_RateIndex)))
                        m_Quote_RateIndex = (Quote_RateIndex)m_EventsValMQueue.quote;
                    else if (tQuote.Equals(typeof(Quote_SecurityAsset)))
                        m_Quote_SecurityAsset = (Quote_SecurityAsset)m_EventsValMQueue.quote;
                    else if (tQuote.Equals(typeof(Quote_ETDAsset)))
                        m_Quote_ETDAsset = (Quote_ETDAsset)m_EventsValMQueue.quote;
                }
                */
            }
            get { return m_EventsValMQueue; }
        }
        #endregion EventsValMQueue

        #region EntityMarketInfo
        public override IPosKeepingMarket EntityMarketInfo
        {
            get { return m_EntityMarketInfo; }
        }
        #endregion EntityMarketInfo
        #region FundingRateQuotationSide
        // EG 20150311 POC - BERKELEY New
        public virtual QuotationSideEnum FundingRateQuotationSide
        {
            get
            {
                return QuotationSideEnum.OfficialClose;
            }
        }
        #endregion FundingRateQuotationSide

        #region Multiplier
        // EG 20150306 [POC-BERKELEY] : New (Get ContractMultipler on CFD FOREX)
        protected virtual decimal Multiplier
        {
            get
            {
                return 1;
            }
        }
        #endregion Multiplier

        #region PosAvailableQuantity
        /// <summary>
        /// Obtient la quantité disponible en date de cotation
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 New (Before in EventValETD) 
        protected decimal PosAvailableQuantity
        {
            get { return m_PosAvailableQuantity; }
        }
        #endregion PosAvailableQuantity
        /// <summary>
        /// Obtient la quantité disponible en date de cotation - 1 (Date veille)
        /// </summary>
        // EG 20170127 New (Before in EventValETD) 
        protected decimal PosAvailableQuantityPrev
        {
            get { return m_PosAvailableQuantityPrev; }
        }
        /// <summary>
        /// Obtient la quantité disponible en date de cotation - 1 +/- celles des actions de correction du jour 
        /// Correction de position (POC) (-)
        /// Transfert (POT) (-)
        /// Unclearing (UNCLEARING)(+)
        /// </summary>
        // EG 20170127 new (Before in EventValETD)
        protected decimal PosQuantityPrevAndActionsDay
        {
            get { return m_PosQuantityPrevAndActionsDay; }
        }

        public EventsValProcess EventsValProcess
        {
            get { return m_EventsValProcess; }
        }

        #endregion Accessors
        #region Constructors
        // EG 20150306 [POC-BERKELEY] : New IsMargining|Isfunding
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20180502 Analyse du code Correction [CA2214]
        // EG 20180503 SetCacheOn GetInstrument
        // EG 20231127 [WI749] Implementation Return Swap : Add IsFungible
        // EG 20231127 [WI749] Implementation Return Swap : Refactoring Code Analysis
        public EventsValProcessBase(EventsValProcess pEventsValProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            :base(pEventsValProcess, pDsTrade, pTradeLibrary, pProduct)
        {
            m_EventsValProcess = pEventsValProcess;
            EventsValMQueue = (EventsValMQueue)m_EventsValProcess.MQueue;

            // EG 20150617 [20665] InitializeDataSetEvent used directly by the calling class
            // EG 20150612 [20665]
            //InitializeDataSetEvent();

            ProductContainer _product = new ProductContainer(m_CurrentProduct);
            _ = _product.GetInstrument(CSTools.SetCacheOn(m_CS), out SQL_Instrument _sql_Instrument);
            if (null != _sql_Instrument)
            {
                m_IsMargining = _sql_Instrument.IsMargining;
                m_IsFunding = _sql_Instrument.IsFunding;
                m_IsFungible = _sql_Instrument.IsFungible;
            }
        }
        #endregion Constructors
        #region Methods
        #region Valorize
        public override Cst.ErrLevel Valorize()
        {
            return Cst.ErrLevel.UNDEFINED;
        }
        #endregion Valorize

        #region SetMarginRequirement
        /// <summary>
        /// Calcul du MGR
        /// </summary>
        // EG 20150306 [POC-BERKELEY] : New Mutualisation RTS|ESE
        // EG 20150706 [21021] Nullable<int> for pPayer|pReceiver
        // EG 20150910 [21315] Payer|Receiver renseignés même si Montant = null
        // EG 20180307 [23769] Gestion dbTransaction
        protected Pair<Nullable<decimal>, string> SetMarginRequirement(IDbTransaction pDbTransaction, 
            RptSideProductContainer pRptSideProductContainer,  decimal pQuantity, decimal pMultiplier, string pCurrency,
            DateTime pDtBusiness, Nullable<decimal> pQuote100, ref Pair<int, Nullable<int>> pPayer, ref Pair<int, Nullable<int>> pReceiver)
        {
            Pair<Nullable<decimal>, string> closingAmount = new Pair<Nullable<decimal>, string>(null, string.Empty);

            Pair<Nullable<decimal>,Nullable<decimal>> _marginRatio = GetMarginRatio(pDbTransaction, pRptSideProductContainer, pDtBusiness);
            if (pQuote100.HasValue && _marginRatio.First.HasValue)
            {
                closingAmount = Tools.ConvertToQuotedCurrency(CSTools.SetCacheOn(m_CS), pDbTransaction, 
                                new Pair<Nullable<decimal>, string>((pQuote100.Value * pQuantity * pMultiplier) * _marginRatio.First.Value, pCurrency));
            }

            // EG 20150910 [21315] Sortie du if (pQuote100.HasValue && _marginRatio.First.HasValue)
            // EG/PM 20140916 Payer = Dealer
            IFixParty partyDealer = pRptSideProductContainer.GetDealer();
            IParty dealerParty = pRptSideProductContainer.DataDocument.GetParty(partyDealer.PartyId.href);

            IFixParty partyCustodian = pRptSideProductContainer.GetClearerCustodian();
            IParty custodianParty = pRptSideProductContainer.DataDocument.GetParty(partyCustodian.PartyId.href);

            // EG 20150706 [21021] Refactoring
            pPayer = new Pair<int, Nullable<int>>(dealerParty.OTCmlId, pRptSideProductContainer.DataDocument.GetOTCmlId_Book(partyDealer.PartyId.href));
            pReceiver = new Pair<int, Nullable<int>>(custodianParty.OTCmlId, pRptSideProductContainer.DataDocument.GetOTCmlId_Book(partyCustodian.PartyId.href));

            return closingAmount;
        }
        #endregion SetMarginRequirement
        #region SetMarketValue
        /// <summary>
        /// Calcul du MKV
        /// </summary>
        // EG 20150306 [POC-BERKELEY] : New Mutualisation RTS|ESE
        // EG 20150706 [21021] Nullable<int> for pPayer|pReceiver
        // EG 20150910 [21315] Payer|Receiver renseignés même si Montant = null
        // EG 20180307 [23769] Gestion dbTransaction
        protected Pair<Nullable<decimal>, string> SetMarketValue(IDbTransaction pDbTransaction, decimal pQuantity, decimal pMultiplier, string pCurrency,
            Nullable<decimal> pQuote100, ref Pair<int, Nullable<int>> pPayer, ref Pair<int, Nullable<int>> pReceiver)
        {
            Pair<Nullable<decimal>, string> closingAmount = new Pair<Nullable<decimal>, string>(null, string.Empty);
            // EG 20150910 [21315]
            if (pQuote100.HasValue)
            {
                closingAmount = Tools.ConvertToQuotedCurrency(CSTools.SetCacheOn(m_CS), pDbTransaction,
                                new Pair<Nullable<decimal>, string>(pQuote100.Value * pQuantity * pMultiplier, pCurrency));
            }
            pPayer.First = m_Seller.OTCmlId;
            pPayer.Second = m_BookSeller;
            pReceiver.First = m_Buyer.OTCmlId;
            pReceiver.Second = m_BookBuyer;
            return closingAmount;
        }
        #endregion SetMarketValue
        #region SetTotalMargin
        /// <summary>
        /// Calcul du TMG
        /// </summary>
        // EG 20150306 [POC-BERKELEY] : New Mutualisation RTS|ESE
        // EG 20150706 [21021] Nullable<int> for pPayer|pReceiver
        // EG 20150910 [21315] Payer|Receiver renseignés même si Montant = null
        // EG 20231127 [WI749] Implementation Return Swap : Refactoring Code Analysis
        protected Pair<Nullable<decimal>, string> SetTotalMargin(IDbTransaction pDbTransaction, bool pIsTradeDay, decimal pQuantity, decimal pPrice, decimal pMultiplier, string pCurrency,
            DateTime pDtBusiness, Nullable<decimal> pQuote100, DateTime pDtBusinessPrev, Nullable<decimal> pQuotePrev100,
            ref Pair<int, Nullable<int>> pPayer, ref Pair<int, Nullable<int>> pReceiver)
        {
            Pair<Nullable<decimal>, string> closingAmount = new Pair<Nullable<decimal>, string>(null, string.Empty);
            // FI 20141106 [20466]
            // EG 20150219 [20520] Nullable<decimal>
            Nullable<decimal> _marginFactor = GetAmount(pDtBusiness, EventTypeFunc.MarginRequirementRatio);
            if (pQuote100.HasValue && _marginFactor.HasValue)
            {
                Pair<decimal?, string> _dayMargin = Tools.ConvertToQuotedCurrency(CSTools.SetCacheOn(m_CS), pDbTransaction, 
                    new Pair<Nullable<decimal>, string>((pQuote100.Value * pQuantity * pMultiplier) * _marginFactor.Value, pCurrency));
                decimal? _previousMargin;
                Pair<decimal?, string> _profitAndLoss;
                // MGR précédente (ou IMG)
                if (pIsTradeDay)
                {
                    _previousMargin = GetAmount(pDtBusiness, EventTypeFunc.InitialMargin);
                    _profitAndLoss = Tools.ConvertToQuotedCurrency(CSTools.SetCacheOn(m_CS), pDbTransaction,
                    new Pair<Nullable<decimal>, string>((pQuote100.Value - pPrice) * pQuantity * pMultiplier, pCurrency));
                }
                else
                {
                    _previousMargin = GetAmount(pDtBusinessPrev, EventTypeFunc.MarginRequirement);
                    _profitAndLoss = Tools.ConvertToQuotedCurrency(CSTools.SetCacheOn(m_CS), pDbTransaction,
                    new Pair<Nullable<decimal>, string>((pQuote100.Value - pQuotePrev100.Value) * pQuantity * pMultiplier, pCurrency));
                }

                // EG 20150219 [20520] Nullable<decimal>
                // EG 20150910 [21315] 
                if (_previousMargin.HasValue)
                {
                    closingAmount = new Pair<decimal?, string>(_dayMargin.First.Value - (_previousMargin.Value + _profitAndLoss.First.Value), _dayMargin.Second);
                }
            }
            // EG 20150910 [21315] 
            bool amountValuatedAndPositive = closingAmount.First.HasValue && (0 < closingAmount.First.Value);
            pPayer.First = amountValuatedAndPositive ? m_Buyer.OTCmlId : m_Seller.OTCmlId;
            pPayer.Second = amountValuatedAndPositive ? m_BookBuyer : m_BookSeller;
            pReceiver.First = amountValuatedAndPositive ? m_Seller.OTCmlId : m_Buyer.OTCmlId;
            pReceiver.Second = amountValuatedAndPositive ? m_BookSeller : m_BookBuyer;
            return closingAmount;
        }
        #endregion SetTotalMargin

        #region SetRowAssetToFundingAmountOrReset
        public virtual void SetRowAssetToFundingAmountOrReset(DataRow pRow, IInterestCalculation pInterestCalculation)
        {
        }
        #endregion SetRowAssetToFundingAmountOrReset
        #region SetRowDetailToFundingAmount
        public virtual void SetRowDetailToFundingAmount(int pIdE, Pair<IInterestLeg, IInterestCalculation> pCurrentInterestLeg)
        {
        }
        #endregion SetRowDetailToFundingAmount
        #region SetRowAssetToReturnLegAmount
        // EG 20231127 [WI749] Implementation Return Swap : New
        public virtual void SetRowAssetToReturnLegAmount(DataRow pRow, IReturnLegMainUnderlyer pReturnLegUnderlyer)
        {
        }
        #endregion SetRowAssetToReturnLegAmount
        #region SetRowAssetToInterestLegAmountOrReset
        // EG 20231127 [WI749] Implementation Return Swap : New
        public virtual void SetRowAssetToInterestLegAmountOrReset(DataRow pRow, IInterestCalculation pInterestCalculation)
        {
        }
        #endregion SetRowAssetToInterestLegAmountOrReset
        #region SetRowDetailToInterestLegAmount
        // EG 20231127 [WI749] Implementation Return Swap : New
        public virtual void SetRowDetailToInterestLegAmount(int pIdE, Pair<IInterestLeg, IInterestCalculation> pCurrentInterestLeg)
        {
        }
        #endregion SetRowDetailToFundingAmount

        #region WriteClosingAmountGen
        /// <summary>
        /// Ecriture des événement calculé via PrepareClosingAmountGen (dans Calculation_LPC_AMT)
        /// </summary>
        // EG 20170412 [23081] Gestion  dbTransaction et SlaveDbTransaction
        // EG 20170510 [23153] New (en provenance de EventValETD)
        // EG 20180205 [23769] Refactoring
        // PL 20180207 Refactoring Add & User variable isUseLocalDbTransaction
        // EG 20180502 Analyse du code Correction [CA2200]
        // EG 20190613 [24683] Add Lock
        // EG 20191018 [25020] Upd
        // EG 20221221 [26183] Usage d'un seul Lock C# pour la totalité de la mise à jour des événements de CashFlows
        protected void WriteClosingAmountGen(List<VAL_Event> pEvents, DateTime pDtBusiness)
        {
            bool isUseLocalDbTransaction = true;
            IDbTransaction dbTransaction = null;

            if (null != SlaveDbTransaction)
            {
                isUseLocalDbTransaction = false;
                dbTransaction = SlaveDbTransaction;
            }

            bool isException = false;
            try
            {
                int _isSelectable = pEvents.Count(item => item.IsSelectable);
                if (0 < _isSelectable)
                {
                    AppInstance.TraceManager.TraceVerbose(this, "WriteClosingAmountGen _isSelectable");

                    int nbIdE = pEvents.Count(item => item.IsNewRow);
                    int newIdE = 0;
                    if (0 < nbIdE)
                    {
                        AppInstance.TraceManager.TraceVerbose(this, "WriteClosingAmountGen isNewRow");
                        if (isUseLocalDbTransaction)
                            dbTransaction = DataHelper.BeginTran(m_EventsValProcess.Cs);

                        SQLUP.GetId(out newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbIdE);

                        if (isUseLocalDbTransaction)
                            DataHelper.CommitTran(dbTransaction);
                    }

                    // EG 20221221 [26183]
                    lock (m_UpdateLock)
                    {
                        if (isUseLocalDbTransaction)
                            dbTransaction = DataHelper.BeginTran(m_EventsValProcess.Cs);

                        pEvents.ForEach(item =>
                        {
                            if (item.IsNewRow)
                            {
                                item.Value["IDE"] = newIdE;
                                if (null != item.ClassREC) item.ClassREC["IDE"] = newIdE;
                                if (null != item.ClassSTL) item.ClassSTL["IDE"] = newIdE;
                                if (null != item.ClassVAL) item.ClassVAL["IDE"] = newIdE;
                                if (null != item.Detail) item.Detail["IDE"] = newIdE;
                                newIdE++;
                            }
                        });

                        if (pEvents.Exists(item => item.IsDeleting))
                        {
                            AppInstance.TraceManager.TraceVerbose(this, "UPDATE EAR/EVENTDET isDeleting");
                            // EG 20191018 [25020] 
                            pEvents.Where(item => item.IsDeleting).ToList().ForEach(item => item.DeleteEAR(dbTransaction, pDtBusiness));
                            m_DsEvents.Update(dbTransaction, Cst.OTCml_TBL.EVENTDET);
                        }

                        if (pEvents.Exists(item => item.IsUpdating))
                        {
                            AppInstance.TraceManager.TraceVerbose(this, "UPDATE isUpdating");
                            m_DsEvents.Update(dbTransaction, IsEndOfDayProcess);
                            m_DsEvents.Update(dbTransaction, Cst.OTCml_TBL.EVENTDET);
                        }

                        if (isUseLocalDbTransaction)
                        {
                            DataHelper.CommitTran(dbTransaction);
                            dbTransaction = null;
                        }
                    }
                }
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && isUseLocalDbTransaction && isException)
                {
                    try { DataHelper.RollbackTran(dbTransaction); }
                    catch { }
                }
            }
        }
        #endregion WriteClosingAmountGen

        /// <summary>
        /// Recherche de l'événement AED ou EED de l'ETD option et mise à jour de l'EVENTCODE si non trouvé
        /// </summary>
        /// <param name="pEventCode">Code EVT à recherche</param>
        /// <param name="pNewEventCode">Code EVT à substituter si code à rechercher non trouvé</param>
        /// <returns></returns>
        // EG 20201009 [25504] Recherche de l'événement Group d'une option ETD (AED-EED) et mise à jour du code EVENTCODE de celui-ci si erroné avant valorisation des événements enfants.
        // EG 20231127 [WI749] Implementation Return Swap : Refactoring Code Analysis
        protected DataRow[] UpdateEventCodeExchangeTradedDerivativeOption(string pEventCode, string pNewEventCode)
        {
            bool isUseLocalDbTransaction = true;
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                DataRow[] rowStreams = GetRowEventByEventCode(pEventCode);
                if (ArrFunc.IsEmpty(rowStreams))
                {
                    rowStreams = GetRowEventByEventCode(pNewEventCode);
                    if (ArrFunc.IsFilled(rowStreams))
                    {
                        EventProcess eventProcess = new EventProcess(this.m_CS);
                        int idE = Convert.ToInt32(rowStreams[0]["IDE"]);

                        rowStreams[0].BeginEdit();
                        rowStreams[0]["EVENTCODE"] = pEventCode;
                        rowStreams[0].EndEdit();

                        if (null != SlaveDbTransaction)
                        {
                            isUseLocalDbTransaction = false;
                            dbTransaction = SlaveDbTransaction;
                        }

                        if (isUseLocalDbTransaction)
                            dbTransaction = DataHelper.BeginTran(m_EventsValProcess.Cs);

                        lock (m_UpdateLock)
                        {
                            m_DsEvents.Update(dbTransaction, Cst.OTCml_TBL.EVENT);
                            eventProcess.Write(dbTransaction, idE, m_Process.MQueue.ProcessType, ProcessStateTools.StatusSuccessEnum, OTCmlHelper.GetDateSysUTC(m_CS), Process.Tracker.IdTRK_L);
                        }

                        if (isUseLocalDbTransaction)
                        {
                            DataHelper.CommitTran(dbTransaction);
                            dbTransaction = null;
                        }
                    }
                }
                return rowStreams;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && isUseLocalDbTransaction && isException)
                {
                    try { DataHelper.RollbackTran(dbTransaction); }
                    catch { }
                }
            }
        }

        // EG 20190613 [24683] New
        // EG 20190924 Insert Duplicate key (ReturnLeg|InterestLeg) = LA maj InterestLeg rejoue les Inserts ReturnLeg
        protected void EndOfDayWriteClosingAmountGen(List<VAL_Event> pEvents)
        {
            bool isUseLocalDbTransaction = true;
            IDbTransaction dbTransaction = null;

            if (null != SlaveDbTransaction)
            {
                isUseLocalDbTransaction = false;
                dbTransaction = SlaveDbTransaction;
            }

            bool isException = false;
            try
            {
                int _isSelectable = pEvents.Count(item => item.IsSelectable);
                if (0 < _isSelectable)
                {
                    AppInstance.TraceManager.TraceVerbose(this, "WriteClosingAmountGen _isSelectable");

                    lock (m_UpdateLock)
                    {
                        int nbIdE = pEvents.Count(item => item.IsNewRow);
                        int newIdE = 0;

                        if (0 < nbIdE)
                        {
                            AppInstance.TraceManager.TraceVerbose(this, "WriteClosingAmountGen isNewRow");
                            if (isUseLocalDbTransaction)
                                dbTransaction = DataHelper.BeginTran(m_EventsValProcess.Cs);

                            SQLUP.GetId(out newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbIdE);

                            if (isUseLocalDbTransaction)
                                DataHelper.CommitTran(dbTransaction);
                        }

                        if (isUseLocalDbTransaction)
                            dbTransaction = DataHelper.BeginTran(m_EventsValProcess.Cs);

                        pEvents.ForEach(item =>
                        {
                            // Insertion distribution des IDE
                            if (item.IsNewRow)
                            {
                                item.Value["IDE"] = newIdE;
                                if (null != item.ClassREC) item.ClassREC["IDE"] = newIdE;
                                if (null != item.ClassSTL) item.ClassSTL["IDE"] = newIdE;
                                if (null != item.ClassVAL) item.ClassVAL["IDE"] = newIdE;
                                if (null != item.Detail) item.Detail["IDE"] = newIdE;

                                Insert(dbTransaction, item);

                                newIdE++;

                            }
                            else if (item.IsDeleting)
                                Delete(dbTransaction, item);
                            else if (item.IsUpdating)
                                Update(dbTransaction, item);
                        });

                        if (isUseLocalDbTransaction)
                        {
                            DataHelper.CommitTran(dbTransaction);
                            dbTransaction = null;
                        }
                    }
                }
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && isUseLocalDbTransaction && isException)
                {
                    try { DataHelper.RollbackTran(dbTransaction); }
                    catch { }
                }
            }
        }
        // EG 20220921 [XXXXX][WI417] les dates d'audit restent en DATETIME (TMDB trop long)
        // EG 20231127 [WI749] Implementation Return Swap : Refactoring Code Analysis
        private Cst.ErrLevel Insert(IDbTransaction pDbTransaction, VAL_Event pEvent)
        {

            string cs = pDbTransaction.Connection.ConnectionString;

            DataParameters parameters = new DataParameters();

            #region EVENT
            parameters.Add(new DataParameter(cs, "IDT", DbType.Int32), pEvent.Value["IDT"]);
            parameters.Add(new DataParameter(cs, "IDE", DbType.Int32), pEvent.Value["IDE"]);
            parameters.Add(new DataParameter(cs, "IDE_EVENT", DbType.Int32), pEvent.Value["IDE_EVENT"]);
            parameters.Add(new DataParameter(cs, "IDE_SOURCE", DbType.Int32), pEvent.Value["IDE_SOURCE"]);
            parameters.Add(new DataParameter(cs, "INSTRUMENTNO", DbType.Int32), pEvent.Value["INSTRUMENTNO"]);
            parameters.Add(new DataParameter(cs, "STREAMNO", DbType.Int32), pEvent.Value["STREAMNO"]);
            parameters.Add(new DataParameter(cs, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), pEvent.Value["EVENTCODE"]);
            parameters.Add(new DataParameter(cs, "EVENTTYPE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), pEvent.Value["EVENTTYPE"]);
            parameters.Add(new DataParameter(cs, "DTSTARTADJ", DbType.Date), pEvent.Value["DTSTARTADJ"]); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(cs, "DTSTARTUNADJ", DbType.Date), pEvent.Value["DTSTARTUNADJ"]); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(cs, "DTENDADJ", DbType.Date), pEvent.Value["DTENDADJ"]); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(cs, "DTENDUNADJ", DbType.Date), pEvent.Value["DTENDUNADJ"]);// FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(cs, "IDA_PAY", DbType.Int32), pEvent.Value["IDA_PAY"]);
            parameters.Add(new DataParameter(cs, "IDB_PAY", DbType.Int32), pEvent.Value["IDB_PAY"]);
            parameters.Add(new DataParameter(cs, "IDA_REC", DbType.Int32), pEvent.Value["IDA_REC"]);
            parameters.Add(new DataParameter(cs, "IDB_REC", DbType.Int32), pEvent.Value["IDB_REC"]);
            parameters.Add(new DataParameter(cs, "VALORISATION", DbType.Decimal), pEvent.Value["VALORISATION"]);
            parameters.Add(new DataParameter(cs, "UNIT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pEvent.Value["UNIT"]);
            parameters.Add(new DataParameter(cs, "UNITTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pEvent.Value["UNITTYPE"]);
            parameters.Add(new DataParameter(cs, "TAXLEVYOPT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pEvent.Value["TAXLEVYOPT"]);
            parameters.Add(new DataParameter(cs, "IDSTACTIVATION", DbType.AnsiString, SQLCst.UT_STATUS_LEN), pEvent.Value["IDSTACTIVATION"]);
            parameters.Add(new DataParameter(cs, "IDASTACTIVATION", DbType.Int32), pEvent.Value["IDASTACTIVATION"]);
            // EG 20220921 [XXXXX][WI417] les dates d'audit restent en DATETIME (TMDB trop long)
            parameters.Add(new DataParameter(cs, "DTSTACTIVATION", DbType.DateTime), pEvent.Value["DTSTACTIVATION"]); // EG 20220921 DTSTACTIVATION reste encore en DateTime (longueur de migration TMDB)
            parameters.Add(new DataParameter(cs, "SOURCE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pEvent.Value["SOURCE"]);
            parameters.Add(new DataParameter(cs, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), pEvent.Value["EXTLLINK"]);
            parameters.Add(new DataParameter(cs, "IDSTCALCUL", DbType.AnsiString, SQLCst.UT_STATUS_LEN), pEvent.Value["IDSTCALCUL"]);
            parameters.Add(new DataParameter(cs, "IDSTTRIGGER", DbType.AnsiString, SQLCst.UT_STATUS_LEN), pEvent.Value["IDSTTRIGGER"]);

            string sqlInsert = @"insert into dbo.EVENT (IDE, IDT, INSTRUMENTNO, STREAMNO, IDE_EVENT,IDE_SOURCE, IDA_PAY, IDB_PAY, IDA_REC, IDB_REC, EVENTCODE, EVENTTYPE, 
            DTSTARTADJ, DTSTARTUNADJ, DTENDADJ, DTENDUNADJ, VALORISATION, UNIT, UNITTYPE, VALORISATIONSYS, UNITSYS, UNITTYPESYS, TAXLEVYOPT, IDSTCALCUL, 
            IDSTTRIGGER, SOURCE, EXTLLINK,  IDSTACTIVATION, IDASTACTIVATION, DTSTACTIVATION) 
            values 
            (@IDE, @IDT, @INSTRUMENTNO, @STREAMNO, @IDE_EVENT, @IDE_SOURCE, @IDA_PAY, @IDB_PAY, @IDA_REC, @IDB_REC, @EVENTCODE, @EVENTTYPE, 
            @DTSTARTADJ, @DTSTARTUNADJ, @DTENDADJ, @DTENDUNADJ, @VALORISATION, @UNIT, @UNITTYPE, @VALORISATION, @UNIT, @UNITTYPE, @TAXLEVYOPT, @IDSTCALCUL, 
            @IDSTTRIGGER, @SOURCE, @EXTLLINK, @IDSTACTIVATION, @IDASTACTIVATION, @DTSTACTIVATION)";

            QueryParameters queryParameters = new QueryParameters(cs, sqlInsert, parameters);
            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            #endregion EVENT
            #region EVENTCLASS
            if (null != pEvent.ClassREC)
                InsertEventClass(pDbTransaction, pEvent.ClassREC);
            if (null != pEvent.ClassSTL)
                InsertEventClass(pDbTransaction, pEvent.ClassSTL);
            if (null != pEvent.ClassVAL)
                InsertEventClass(pDbTransaction, pEvent.ClassVAL);
            #endregion EVENTCLASS
            #region EVENTDET
            if (null != pEvent.Detail)
                InsertEventDet(pDbTransaction, pEvent.Detail);
            #endregion EVENTDET

            return Cst.ErrLevel.SUCCESS;
        }
        private DataParameters SetParametersEventDet(string pCs, DataRow pRowEventDet)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCs, "IDE", DbType.Int32), pRowEventDet["IDE"]);
            parameters.Add(new DataParameter(pCs, "BASIS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pRowEventDet["BASIS"]);
            parameters.Add(new DataParameter(pCs, "CLOSINGPRICE", DbType.Decimal), pRowEventDet["CLOSINGPRICE"]);
            parameters.Add(new DataParameter(pCs, "CLOSINGPRICE100", DbType.Decimal), pRowEventDet["CLOSINGPRICE100"]);
            parameters.Add(new DataParameter(pCs, "CONTRACTMULTIPLIER", DbType.Decimal), pRowEventDet["CONTRACTMULTIPLIER"]);
            parameters.Add(new DataParameter(pCs, "CONVERSIONRATE", DbType.Decimal), pRowEventDet["CONVERSIONRATE"]);
            parameters.Add(new DataParameter(pCs, "DAILYQUANTITY", DbType.Decimal), pRowEventDet["DAILYQUANTITY"]);
            parameters.Add(new DataParameter(pCs, "UNITDAILYQUANTITY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pRowEventDet["UNITDAILYQUANTITY"]);
            parameters.Add(new DataParameter(pCs, "DCF", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pRowEventDet["DCF"]);
            parameters.Add(new DataParameter(pCs, "DCFDEN", DbType.Int32), pRowEventDet["DCFDEN"]);
            parameters.Add(new DataParameter(pCs, "DCFNUM", DbType.Int32), pRowEventDet["DCFNUM"]);
            parameters.Add(new DataParameter(pCs, "DTACTION", DbType.DateTime), pRowEventDet["DTACTION"]);
            parameters.Add(new DataParameter(pCs, "DTFIXING", DbType.DateTime), pRowEventDet["DTFIXING"]);
            parameters.Add(new DataParameter(pCs, "DTSETTLTPRICE", DbType.DateTime), pRowEventDet["DTSETTLTPRICE"]);
            parameters.Add(new DataParameter(pCs, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), pRowEventDet["EXTLLINK"]);
            parameters.Add(new DataParameter(pCs, "FACTOR", DbType.Decimal), pRowEventDet["FACTOR"]);
            parameters.Add(new DataParameter(pCs, "FWDPOINTS", DbType.Decimal), pRowEventDet["FWDPOINTS"]);
            parameters.Add(new DataParameter(pCs, "FXTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pRowEventDet["FXTYPE"]);
            parameters.Add(new DataParameter(pCs, "GAPRATE", DbType.Decimal), pRowEventDet["GAPRATE"]);
            parameters.Add(new DataParameter(pCs, "IDBC", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pRowEventDet["IDBC"]);
            parameters.Add(new DataParameter(pCs, "IDC1", DbType.AnsiString, SQLCst.UT_CURR_LEN), pRowEventDet["IDC1"]);
            parameters.Add(new DataParameter(pCs, "IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN), pRowEventDet["IDC2"]);
            parameters.Add(new DataParameter(pCs, "IDC_BASE", DbType.AnsiString, SQLCst.UT_CURR_LEN), pRowEventDet["IDC_BASE"]);
            parameters.Add(new DataParameter(pCs, "IDC_REF", DbType.AnsiString, SQLCst.UT_CURR_LEN), pRowEventDet["IDC_REF"]);
            parameters.Add(new DataParameter(pCs, "INTEREST", DbType.Decimal), pRowEventDet["INTEREST"]);
            parameters.Add(new DataParameter(pCs, "MULTIPLIER", DbType.Decimal), pRowEventDet["MULTIPLIER"]);
            parameters.Add(new DataParameter(pCs, "NOTE", DbType.AnsiString, SQLCst.UT_NOTE_LEN), pRowEventDet["NOTE"]);
            parameters.Add(new DataParameter(pCs, "NOTIONALAMOUNT", DbType.Decimal), pRowEventDet["NOTIONALAMOUNT"]);
            parameters.Add(new DataParameter(pCs, "NOTIONALREFERENCE", DbType.Decimal), pRowEventDet["NOTIONALREFERENCE"]);
            parameters.Add(new DataParameter(pCs, "PCTPAYOUT", DbType.Decimal), pRowEventDet["PCTPAYOUT"]);
            parameters.Add(new DataParameter(pCs, "PCTRATE", DbType.Decimal), pRowEventDet["PCTRATE"]);
            parameters.Add(new DataParameter(pCs, "PERIODPAYOUT", DbType.Int32), pRowEventDet["PERIODPAYOUT"]);
            parameters.Add(new DataParameter(pCs, "PRICE", DbType.Decimal), pRowEventDet["PRICE"]);
            parameters.Add(new DataParameter(pCs, "PRICE100", DbType.Decimal), pRowEventDet["PRICE100"]);
            parameters.Add(new DataParameter(pCs, "QUOTEDELTA", DbType.Decimal), pRowEventDet["QUOTEDELTA"]);
            parameters.Add(new DataParameter(pCs, "QUOTETIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pRowEventDet["QUOTETIMING"]);
            parameters.Add(new DataParameter(pCs, "QUOTEPRICE", DbType.Decimal), pRowEventDet["QUOTEPRICE"]);
            parameters.Add(new DataParameter(pCs, "QUOTEPRICE100", DbType.Decimal), pRowEventDet["QUOTEPRICE100"]);
            parameters.Add(new DataParameter(pCs, "QUOTEPRICEYEST", DbType.Decimal), pRowEventDet["QUOTEPRICEYEST"]);
            parameters.Add(new DataParameter(pCs, "QUOTEPRICEYEST100", DbType.Decimal), pRowEventDet["QUOTEPRICEYEST100"]);
            parameters.Add(new DataParameter(pCs, "RATE", DbType.Decimal), pRowEventDet["RATE"]);
            parameters.Add(new DataParameter(pCs, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN), pRowEventDet["ROWATTRIBUT"]);
            parameters.Add(new DataParameter(pCs, "SETTLTPRICE", DbType.Decimal), pRowEventDet["SETTLTPRICE"]);
            parameters.Add(new DataParameter(pCs, "SETTLTPRICE100", DbType.Decimal), pRowEventDet["SETTLTPRICE100"]);
            parameters.Add(new DataParameter(pCs, "SETTLTQUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pRowEventDet["SETTLTQUOTESIDE"]);
            parameters.Add(new DataParameter(pCs, "SETTLTQUOTETIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pRowEventDet["SETTLTQUOTETIMING"]);
            parameters.Add(new DataParameter(pCs, "SETTLEMENTRATE", DbType.Decimal), pRowEventDet["SETTLEMENTRATE"]);
            parameters.Add(new DataParameter(pCs, "SPOTRATE", DbType.Decimal), pRowEventDet["SPOTRATE"]);
            parameters.Add(new DataParameter(pCs, "SPREAD", DbType.Decimal), pRowEventDet["SPREAD"]);
            parameters.Add(new DataParameter(pCs, "STRIKEPRICE", DbType.Decimal), pRowEventDet["STRIKEPRICE"]);
            parameters.Add(new DataParameter(pCs, "TOTALOFDAY", DbType.Int32), pRowEventDet["TOTALOFDAY"]);
            parameters.Add(new DataParameter(pCs, "TOTALOFYEAR", DbType.Int32), pRowEventDet["TOTALOFYEAR"]);
            parameters.Add(new DataParameter(pCs, "TOTALPAYOUTAMOUNT", DbType.Decimal), pRowEventDet["TOTALPAYOUTAMOUNT"]);
            parameters.Add(new DataParameter(pCs, "PIP", DbType.Decimal), pRowEventDet["PIP"]);
            parameters.Add(new DataParameter(pCs, "DTDLVYSTART", DbType.DateTime2), pRowEventDet["DTDLVYSTART"]);
            parameters.Add(new DataParameter(pCs, "DTDLVYEND", DbType.DateTime2), pRowEventDet["DTDLVYEND"]);
            return parameters;
        }
        // EG 20231127 [WI749] Implementation Return Swap : Refactoring Code Analysis
        private Cst.ErrLevel InsertEventClass(IDbTransaction pDbTransaction, DataRow pRowEventClass)
        {
            string cs = pDbTransaction.Connection.ConnectionString;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(cs, "IDE", DbType.Int32), pRowEventClass["IDE"]);
            parameters.Add(new DataParameter(cs, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN), pRowEventClass["EVENTCLASS"]);
            parameters.Add(new DataParameter(cs, "DTEVENT", DbType.Date), pRowEventClass["DTEVENT"]); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(cs, "DTEVENTFORCED", DbType.Date), pRowEventClass["DTEVENTFORCED"]); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(cs, "ISPAYMENT", DbType.Boolean), pRowEventClass["ISPAYMENT"]);
            parameters.Add(new DataParameter(cs, "IDNETCONVENTION", DbType.Int32), pRowEventClass["IDNETCONVENTION"]);
            parameters.Add(new DataParameter(cs, "IDNETDESIGNATION", DbType.Int32), pRowEventClass["IDNETDESIGNATION"]);
            parameters.Add(new DataParameter(cs, "NETMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pRowEventClass["NETMETHOD"]);
            parameters.Add(new DataParameter(cs, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), pRowEventClass["EXTLLINK"]);

            string sqlInsert = @"insert into dbo.EVENTCLASS (IDE, EVENTCLASS, DTEVENT, DTEVENTFORCED, ISPAYMENT, NETMETHOD, IDNETCONVENTION, IDNETDESIGNATION, EXTLLINK)
            values
            (@IDE, @EVENTCLASS, @DTEVENT, @DTEVENTFORCED, @ISPAYMENT, @NETMETHOD, @IDNETCONVENTION, @IDNETDESIGNATION, @EXTLLINK)" + Cst.CrLf;
            QueryParameters queryParameters = new QueryParameters(cs, sqlInsert, parameters);
            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        // EG 20231127 [WI749] Implementation Return Swap : Refactoring Code Analysis
        private Cst.ErrLevel InsertEventDet(IDbTransaction pDbTransaction, DataRow pRowEventDet)
        {
            string cs = pDbTransaction.Connection.ConnectionString;
            DataParameters parameters = SetParametersEventDet(cs, pRowEventDet);

            string sqlInsert = @"insert into dbo.EVENTDET 
            (IDE, BASIS, CLOSINGPRICE, CLOSINGPRICE100, CONTRACTMULTIPLIER, CONVERSIONRATE, 
            DAILYQUANTITY, UNITDAILYQUANTITY, DCF, DCFDEN, DCFNUM, DTACTION, DTFIXING, DTSETTLTPRICE, EXTLLINK, 
            FACTOR, FWDPOINTS, FXTYPE, GAPRATE, IDBC, IDC_BASE, IDC_REF, IDC1, IDC2, INTEREST, 
            MULTIPLIER, NOTE, NOTIONALAMOUNT, NOTIONALREFERENCE, PCTPAYOUT, PCTRATE, PERIODPAYOUT, PRICE, PRICE100, 
            QUOTEDELTA, QUOTEPRICE, QUOTEPRICE100, QUOTEPRICEYEST, QUOTEPRICEYEST100, QUOTETIMING, 
            RATE, ROWATTRIBUT, SETTLEMENTRATE, SETTLTPRICE, SETTLTPRICE100, SETTLTQUOTESIDE, SETTLTQUOTETIMING, 
            SPOTRATE, SPREAD, STRIKEPRICE, TOTALOFDAY, TOTALOFYEAR, TOTALPAYOUTAMOUNT, PIP, DTDLVYSTART, DTDLVYEND)
            values
            (@IDE, @BASIS, @CLOSINGPRICE, @CLOSINGPRICE100, @CONTRACTMULTIPLIER, @CONVERSIONRATE, 
            @DAILYQUANTITY, @UNITDAILYQUANTITY, @DCF, @DCFDEN, @DCFNUM, @DTACTION, @DTFIXING, @DTSETTLTPRICE, @EXTLLINK, 
            @FACTOR, @FWDPOINTS, @FXTYPE, @GAPRATE, @IDBC, @IDC_BASE, @IDC_REF, @IDC1, @IDC2, @INTEREST, 
            @MULTIPLIER, @NOTE, @NOTIONALAMOUNT, @NOTIONALREFERENCE, @PCTPAYOUT, @PCTRATE, @PERIODPAYOUT, @PRICE, @PRICE100, 
            @QUOTEDELTA, @QUOTEPRICE, @QUOTEPRICE100, @QUOTEPRICEYEST, @QUOTEPRICEYEST100, @QUOTETIMING, 
            @RATE, @ROWATTRIBUT, @SETTLEMENTRATE, @SETTLTPRICE, @SETTLTPRICE100, @SETTLTQUOTESIDE, @SETTLTQUOTETIMING, 
            @SPOTRATE, @SPREAD, @STRIKEPRICE, @TOTALOFDAY, @TOTALOFYEAR, @TOTALPAYOUTAMOUNT, @PIP, @DTDLVYSTART, @DTDLVYEND)" + Cst.CrLf;
            QueryParameters queryParameters = new QueryParameters(cs, sqlInsert, parameters);
            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        // EG 20231127 [WI749] Implementation Return Swap : Refactoring Code Analysis
        private Cst.ErrLevel Update(IDbTransaction pDbTransaction, VAL_Event pEvent)
        {
            string cs = pDbTransaction.Connection.ConnectionString;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(cs, "IDE", DbType.Int32), pEvent.Value["IDE"]);
            parameters.Add(new DataParameter(cs, "IDA_PAY", DbType.Int32), pEvent.Value["IDA_PAY"]);
            parameters.Add(new DataParameter(cs, "IDB_PAY", DbType.Int32), pEvent.Value["IDB_PAY"]);
            parameters.Add(new DataParameter(cs, "IDA_REC", DbType.Int32), pEvent.Value["IDA_REC"]);
            parameters.Add(new DataParameter(cs, "IDB_REC", DbType.Int32), pEvent.Value["IDB_REC"]);
            parameters.Add(new DataParameter(cs, "DTSTARTADJ", DbType.Date), pEvent.Value["DTSTARTADJ"]); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(cs, "DTSTARTUNADJ", DbType.Date), pEvent.Value["DTSTARTUNADJ"]); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(cs, "DTENDADJ", DbType.Date), pEvent.Value["DTENDADJ"]); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(cs, "DTENDUNADJ", DbType.Date), pEvent.Value["DTENDUNADJ"]); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(cs, "VALORISATION", DbType.Decimal), pEvent.Value["VALORISATION"]);
            parameters.Add(new DataParameter(cs, "UNIT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pEvent.Value["UNIT"]);
            parameters.Add(new DataParameter(cs, "UNITTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pEvent.Value["UNITTYPE"]);
            parameters.Add(new DataParameter(cs, "VALORISATIONSYS", DbType.Decimal), pEvent.Value["VALORISATIONSYS"]);
            parameters.Add(new DataParameter(cs, "UNITSYS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pEvent.Value["UNITSYS"]);
            parameters.Add(new DataParameter(cs, "UNITTYPESYS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pEvent.Value["UNITTYPESYS"]);
            parameters.Add(new DataParameter(cs, "IDSTACTIVATION", DbType.AnsiString, SQLCst.UT_STATUS_LEN), pEvent.Value["IDSTACTIVATION"]);
            parameters.Add(new DataParameter(cs, "IDASTACTIVATION", DbType.Int32), pEvent.Value["IDASTACTIVATION"]);
            parameters.Add(new DataParameter(cs, "DTSTACTIVATION", DbType.DateTime), pEvent.Value["DTSTACTIVATION"]);
            parameters.Add(new DataParameter(cs, "SOURCE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pEvent.Value["SOURCE"]);
            parameters.Add(new DataParameter(cs, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), pEvent.Value["EXTLLINK"]);
            parameters.Add(new DataParameter(cs, "IDSTCALCUL", DbType.AnsiString, SQLCst.UT_STATUS_LEN), pEvent.Value["IDSTCALCUL"]);
            parameters.Add(new DataParameter(cs, "IDSTTRIGGER", DbType.AnsiString, SQLCst.UT_STATUS_LEN), pEvent.Value["IDSTTRIGGER"]);

            string sqlUpdate = @"update dbo.EVENT set IDA_PAY = @IDA_PAY, IDB_PAY = @IDB_PAY, IDA_REC = @IDA_REC, IDB_REC = @IDB_REC, 
            DTSTARTADJ = @DTSTARTADJ, DTSTARTUNADJ = @DTSTARTUNADJ, DTENDADJ = @DTENDADJ, DTENDUNADJ = @DTENDUNADJ, 
            VALORISATION = @VALORISATION, UNIT = @UNIT, UNITTYPE = @UNITTYPE, VALORISATIONSYS = @VALORISATIONSYS, UNITSYS = @UNITSYS, UNITTYPESYS = @UNITTYPESYS, 
            IDSTCALCUL = @IDSTCALCUL, IDSTTRIGGER = @IDSTTRIGGER, SOURCE = @SOURCE, EXTLLINK = @EXTLLINK,  
            IDSTACTIVATION = @IDSTACTIVATION, IDASTACTIVATION = @IDASTACTIVATION, DTSTACTIVATION = @DTSTACTIVATION
            where (IDE = @IDE)";

            QueryParameters queryParameters = new QueryParameters(cs, sqlUpdate, parameters);
            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            #region EVENTDET
            if (null != pEvent.Detail)
                UpdateEventDet(pDbTransaction, pEvent.Detail);
            #endregion EVENTDET

            return Cst.ErrLevel.SUCCESS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pRowEventDet"></param>
        /// <returns></returns>
        /// RD 20190911 [24944] Une parenthèse en trop à la fin de la requête
        // EG 20231127 [WI749] Implementation Return Swap : Refactoring Code Analysis
        private Cst.ErrLevel UpdateEventDet(IDbTransaction pDbTransaction, DataRow pRowEventDet)
        {
            string cs = pDbTransaction.Connection.ConnectionString;
            DataParameters parameters = SetParametersEventDet(cs, pRowEventDet);

            string sqlUpdate = @"update dbo.EVENTDET set
            BASIS = @BASIS, CLOSINGPRICE = @CLOSINGPRICE, CLOSINGPRICE100 = @CLOSINGPRICE100, CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER, CONVERSIONRATE = @CONVERSIONRATE, 
            DAILYQUANTITY = @DAILYQUANTITY, UNITDAILYQUANTITY = @UNITDAILYQUANTITY, DCF = @DCF, DCFDEN = @DCFDEN, DCFNUM = @DCFNUM, DTACTION = @DTACTION, DTFIXING = @DTFIXING, 
            DTSETTLTPRICE = @DTSETTLTPRICE, EXTLLINK = @EXTLLINK, FACTOR = @FACTOR, FWDPOINTS = @FWDPOINTS, FXTYPE = @FXTYPE, GAPRATE = @GAPRATE, IDBC = @IDBC, IDC_BASE = @IDC_BASE, 
            IDC_REF = @IDC_REF, IDC1 = @IDC1, IDC2 = @IDC2, INTEREST = @INTEREST, MULTIPLIER = @MULTIPLIER, NOTE = @NOTE, NOTIONALAMOUNT = @NOTIONALAMOUNT, 
            NOTIONALREFERENCE = @NOTIONALREFERENCE, PCTPAYOUT = @PCTPAYOUT, PCTRATE = @PCTRATE, PERIODPAYOUT = @PERIODPAYOUT, PRICE = @PRICE, PRICE100 = @PRICE100, 
            QUOTEDELTA = @QUOTEDELTA, QUOTEPRICE = @QUOTEPRICE, QUOTEPRICE100 = @QUOTEPRICE100, QUOTEPRICEYEST = @QUOTEPRICEYEST, QUOTEPRICEYEST100 = @QUOTEPRICEYEST100, 
            QUOTETIMING = @QUOTETIMING, RATE = @RATE, ROWATTRIBUT = @ROWATTRIBUT, SETTLEMENTRATE = @SETTLEMENTRATE, SETTLTPRICE = @SETTLTPRICE, SETTLTPRICE100 = @SETTLTPRICE100, 
            SETTLTQUOTESIDE = @SETTLTQUOTESIDE, SETTLTQUOTETIMING = @SETTLTQUOTETIMING, SPOTRATE = @SPOTRATE, SPREAD = @SPREAD, 
            STRIKEPRICE = @STRIKEPRICE, TOTALOFDAY = @TOTALOFDAY, TOTALOFYEAR = @TOTALOFYEAR, TOTALPAYOUTAMOUNT = @TOTALPAYOUTAMOUNT, PIP = @PIP, 
            DTDLVYSTART = @DTDLVYSTART, DTDLVYEND = @DTDLVYEND
            where IDE = @IDE" + Cst.CrLf;

            QueryParameters queryParameters = new QueryParameters(cs, sqlUpdate, parameters);
            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        // EG 20231127 [WI749] Implementation Return Swap : Refactoring Code Analysis
        private Cst.ErrLevel Delete (IDbTransaction pDbTransaction, VAL_Event pEvent)
        {
            string cs = pDbTransaction.Connection.ConnectionString;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(cs, "IDE", DbType.Int32), Convert.ToInt32(pEvent.Value["IDE",DataRowVersion.Original]));

            #region EVENTDET
            string sqlDelete = @"delete from dbo.EVENTDET where (IDE = @IDE)";
            QueryParameters queryParameters = new QueryParameters(cs, sqlDelete, parameters);
            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            #endregion EVENTDET

            #region EVENT
            sqlDelete = @"delete from dbo.EVENT where (IDE = @IDE)";
            queryParameters = new QueryParameters(cs, sqlDelete, parameters);
            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            #endregion EVENT

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Methods
    }
    #endregion EventsValProcessBase
}

#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.MarkToMarket
{
    #region MarkToMarketGenProcessFX
    // EG 20150318 [POC] 
    public class MarkToMarketGenProcessFX : MarkToMarketGenProcessFxBase
    {
        #region Members
        private DateTime m_ExpiryDate;
        private DateTime m_UsanceDelayDate;
        private Nullable<decimal> m_MarkToMarketAmount;
        #endregion Members
        #region Accessors

        #region Currency1
        /// <summary>
        /// Obtient la devise 1
        /// </summary>
        private string Currency1
        {
            get { return RowStartCurrency1["UNIT"].ToString(); }
        }
        #endregion Currency1
        #region Currency2
        /// <summary>
        /// Obtient la devise 2
        /// </summary>
        private string Currency2
        {
            get { return RowStartCurrency2["UNIT"].ToString(); }
        }
        #endregion Currency2

        #region GetQuoteBasis
        protected QuoteBasisEnum QuoteBasis
        {
            get
            {
                DataRow row = GetRowEventDetail(Convert.ToInt32(RowStartCurrency1["IDE"]));
                return (QuoteBasisEnum)ReflectionTools.EnumParse(new QuoteBasisEnum(), row["BASIS"].ToString());
            }
        }
        #endregion GetQuoteBasis

        #region IsDailyClosingInUsanceDelay
        /// <summary>
        /// Obtient true si la date de valorisation se trouve entre la date liée au délai d'usance et le date d'échéance
        /// </summary>
        private bool IsDailyClosingInUsanceDelay
        {
            get { return (0 <= CommonValDate.CompareTo(UsanceDelayDate)) && (0 <= m_ExpiryDate.CompareTo(CommonValDate)); }
        }
        #endregion IsDailyClosingInUsanceDelay
        #region NotionalReference
        /// <summary>
        /// Obtient le Montant DEV1
        /// </summary>
        private decimal NotionalReference
        {
            get { return Convert.ToDecimal(RowStartCurrency1["VALORISATION"]); }
        }
        #endregion NotionalReference
        #region UsanceDelayDate
        /// <summary>
        /// Date d'échéance - le délai d'usance
        /// </summary>
        private DateTime UsanceDelayDate
        {
            get
            {
                return m_UsanceDelayDate;
            }
        }
        #endregion UsanceDelayDate
        #endregion Accessors
        
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcess"></param>
        /// <param name="pDsTrade"></param>
        /// <param name="pTradeLibrary"></param>
        /// <param name="pProduct"></param>
        public MarkToMarketGenProcessFX(MarkToMarketGenProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pProcess, pDsTrade, pTradeLibrary, pProduct)
        {
            m_Parameters = new CommonValParametersFX();
        }
        #endregion Constructors
        #region Methods
   
        #region GetDCF
        /// <summary>
        /// Get EFS_DayCountFraction for a currency for the duration 
        /// between the DailyClosing date (CommonValDate) and the delay of usance
        /// </summary>
        /// <param name="pIdC">Currency</param>
        /// <returns>DayCountFraction</returns>
        private EFS_DayCountFraction GetDCF(string pIdC)
        {
            DayCountFractionEnum dcf = DayCountFractionEnum.ACT360;
            SQL_Currency currency = new SQL_Currency(m_CS, SQL_Currency.IDType.Iso4217, pIdC);
            if (currency.IsLoaded)
                dcf = currency.FpML_Enum_DayCountFraction;
            IInterval interval = m_tradeLibrary.Product.ProductBase.CreateInterval(PeriodEnum.D, 0);
            return new EFS_DayCountFraction(CommonValDate, UsanceDelayDate, dcf, interval);
        }
        #endregion GetDCF
        #region MarkToMarketFxForward
        /// <summary>
        /// General process to valorize a MarkToMarket amount for an FxForward
        /// Case 1 : DailClosingDate is in between the delay of usance and the expiry date
        ///          MarkToMarket amount is calculated with the delayUsanceDate spotRate
        /// Case 2 : DailClosingDate is before the delay of usance
        ///          MarkToMarket amount is calculated with the forward rate on the duration 
        ///          between DailyClosing date and UsanceDelay date
        /// </summary>
        /// <param name="pRowCandidate">Row candidate to valuation (TER/CU2 for Deliverable or TER/SCU for undeliverable forward</param>
        // EG 20150317 [POC] Mod signature
        // EG 20150317 [POC] Test m_MarkToMarketGenMQueue.isEOD (EVT = MGR enfant de LPC/AMT) sinon inchangé (EVT = MTM)
        // EG 20150408 [POC] Global Refactoring (for CrossMargin)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Nullable<decimal> MarkToMarketFxForward(IDbTransaction pDbTransaction, string pEventType, decimal pTradedForwardeRate, PayerReceiverInfo pPayerReceiverInfo)
        {
            Pair<Nullable<decimal>, Nullable<decimal>> marginRatio = GetMarginRatio(pDbTransaction, m_RptSideProductContainer, CommonValDate);
            decimal? mtmAmount;
            if (marginRatio.First.HasValue)
            {

                if (Cst.MarginingMode.MarkToMarketAmount == m_MarginingMode.Value)
                {
                    SpotRateAndFxForwardCalculation(pPayerReceiverInfo);
                    mtmAmount = MarkToMarketCalculationAmount(pEventType, NotionalReference, pTradedForwardeRate);
                }
                else
                {
                    mtmAmount = Convert.ToDecimal(RowStartCurrency2["VALORISATION"]);
                }

                if (mtmAmount.HasValue)
                {
                    EFS_Cash cashAmount = new EFS_Cash(m_CS, mtmAmount.Value, EventTypeFunc.IsNonDeliverable(pEventType) ? Currency1 : Currency2);
                    mtmAmount = cashAmount.AmountRounded * marginRatio.First;
                    if (m_IsApplyMinMargin)
                        mtmAmount = CompareMarginRequirementToInitialMargin(mtmAmount.Value);
                }
                else
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_MarkToMarketGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    // ERREUR CALCUL MTM
                    ProcessState _processState = new ProcessState(ProcessStateTools.StatusErrorEnum);
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, "Error on MarkToMarketCalculation (MarkToMarketFxForward)", 2));

                    throw new SpheresException2(_processState);
                }
            }
            else
            {
                // Pas de margin Factor
                ProcessState _processState = new ProcessState(ProcessStateTools.StatusErrorEnum);
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6680), 2,
                    new LogParam(IsDealerBuyer ? LogTools.IdentifierAndId(m_Buyer.PartyName, m_Buyer.OTCmlId) : LogTools.IdentifierAndId(m_Seller.PartyName, m_Seller.OTCmlId)),
                    new LogParam(IsDealerBuyer ? LogTools.IdentifierAndId(m_Seller.PartyName, m_Seller.OTCmlId) : LogTools.IdentifierAndId(m_Buyer.PartyName, m_Buyer.OTCmlId)),
                    new LogParam(LogTools.IdentifierAndId(m_MarkToMarketGenMQueue.GetStringValueIdInfoByKey("identifier"), m_DsTrade.IdT))));

                throw new SpheresException2(_processState);
            }
            return mtmAmount;
        }
        #endregion MarkToMarketFxForward
        #region CrossMarginMarkToMarketFxForward
        // EG 20150408 [POC] New (for CrossMargin)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Nullable<decimal> CrossMarginMarkToMarketFxForward(IDbTransaction pDbTransaction, string pEventType, decimal pTradedForwardeRate, PayerReceiverInfo pPayerReceiverInfo)
        {
            // Lecture du MarginRatio/CrossMarginRatio pour le trade en cours de valo (identique quel que soit les trades liés car même caractéristiques)
            Pair<Nullable<decimal>,Nullable<decimal>> marginRatio = GetMarginRatio(pDbTransaction, m_RptSideProductContainer, CommonValDate);
            Nullable<decimal> mtmAmount = null;

            if (marginRatio.First.HasValue && marginRatio.Second.HasValue)
            {
                // Calcul des taux pour le trade à valoriser
                if (Cst.MarginingMode.MarkToMarketAmount == m_MarginingMode.Value)
                    SpotRateAndFxForwardCalculation(pPayerReceiverInfo);

                // Calcul du montant sur la base des taux et du notionel de chaque élément de CrossMarging
                m_MarkToMarketGenMQueue.crossMarginTrades.ForEach(item =>
                {
                    // Initialisation par le ReferenceAmount
                    Nullable<decimal> amount = item.Second;

                    // Calcul du MTM par trade
                    if (Cst.MarginingMode.MarkToMarketAmount == m_MarginingMode.Value)
                        amount = MarkToMarketCalculationAmount(pEventType, item.Second, pTradedForwardeRate);

                    if (amount.HasValue)
                    {
                        // Application du ratio par trade
                        // Si IdT = trade en cours de valorisation = MarginRatio (STANDARD) sinon CrossMarginRatio (SPREAD)
                        if (false == mtmAmount.HasValue)
                            mtmAmount = 0;
                        mtmAmount += amount * (item.First == m_DsTrade.IdT ? marginRatio.First : marginRatio.Second);
                    }
                    else
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_MarkToMarketGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        Logger.Log(new LoggerData(LogLevelEnum.Error, "Error on MarkToMarketCalculation (CrossMarginMarkToMarketFxForward)", 2));
                        
                        // ERREUR CALCUL MTM
                        throw new SpheresException2(new ProcessState(ProcessStateTools.StatusErrorEnum));
                    }

                });

                // Arrondi final
                if (mtmAmount.HasValue)
                {
                    EFS_Cash cashAmount = new EFS_Cash(m_CS, mtmAmount.Value, EventTypeFunc.IsNonDeliverable(pEventType) ? Currency1 : Currency2);
                    mtmAmount = cashAmount.AmountRounded;
                    if (m_IsApplyMinMargin)
                        mtmAmount = CompareMarginRequirementToInitialMargin(mtmAmount.Value);
                }
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_MarkToMarketGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 680), 2,
                    new LogParam(IsDealerBuyer ? LogTools.IdentifierAndId(m_Buyer.PartyName, m_Buyer.OTCmlId) : LogTools.IdentifierAndId(m_Seller.PartyName, m_Seller.OTCmlId)),
                    new LogParam(IsDealerBuyer ? LogTools.IdentifierAndId(m_Seller.PartyName, m_Seller.OTCmlId) : LogTools.IdentifierAndId(m_Buyer.PartyName, m_Buyer.OTCmlId)),
                    new LogParam(LogTools.IdentifierAndId(m_MarkToMarketGenMQueue.GetStringValueIdInfoByKey("identifier"), m_DsTrade.IdT))));

                throw new SpheresException2(new ProcessState(ProcessStateTools.StatusErrorEnum));
            }
            return mtmAmount;
        }
        #endregion CrossMarginMarkToMarketFxForward

        #region MarkToMarketCalculationAmount
        /// <summary>
        /// Calcul du montant MarkToMarket (NDF|FXFORWARD)
        /// Case 1 : DailClosingDate is in between the delay of usance and the expiry date
        ///          MarkToMarket amount is calculated with the delayUsanceDate spotRate
        /// Case 2 : DailClosingDate is before the delay of usance
        ///          MarkToMarket amount is calculated with the forward rate on the duration 
        ///          between DailyClosing date and UsanceDelay date
        /// </summary>
        /// <param name="pEventType">EvenType permettant de connaitre le type de FX</param>
        /// <param name="pNotionalReference">Notional de référence du trade à valorisé ou des trades liés par CrossMargin</param>
        /// <param name="pTradedForwardeRate">Taux négocié du trade à valoriser</param>
        /// <returns></returns>
        // EG 20150408 [POC] Global Refactoring (for CrossMargin)
        private Nullable<decimal> MarkToMarketCalculationAmount(string pEventType, decimal pNotionalReference, decimal pTradedForwardeRate)
        {
            Nullable<decimal> mtmAmount = null;
            Nullable<decimal> rate = IsDailyClosingInUsanceDelay ? m_PricingValues.SpotRate : m_PricingValues.ForwardPrice;
            if (rate.HasValue)
            {
                if (EventTypeFunc.IsNonDeliverable(pEventType))
                {
                    // Differential of forwardRate and initialForwardRate
                    // MT = notional (devise de settlemement) * (1 - cours Forward négocié / cours Spot|cours Forward calculé)
                    // SI MT >0 le vendeur de la devise de settlemement paye
                    // SI MT <0 l'acheteur de la devise de settlemement reçoit
                    mtmAmount = CommonValFunc.CalcDifferentialAmount(pNotionalReference, pTradedForwardeRate, rate);
                }
                else
                {
                    EFS_Cash cashAmount = new EFS_Cash(m_CS, Currency1, Currency2, pNotionalReference, rate, QuoteBasis);
                    mtmAmount = cashAmount.ExchangeAmount;
                }
            }
            return mtmAmount;
        }
        #endregion MarkToMarketCalculationAmount
        #region SpotRateAndFxForwardCalculation
        // EG 20150408 [POC] Global Refactoring (for CrossMargin)
        /// <summary>
        /// 1. Lecture du taux Spot (UsanceDelayDate) si valo dans le delai d'usance
        /// 2. Lecture du taux Spot (CommonValDate = Date de valo) 
        ///    Calcul du taux forward sur la base des courbes de taux si valo hors  delai d'usance
        /// </summary>
        /// <param name="pPayerReceiverInfo"></param>
        private void SpotRateAndFxForwardCalculation(PayerReceiverInfo pPayerReceiverInfo)
        {
            if (IsDailyClosingInUsanceDelay)
            {
                //Cours en date date Dtfin-délai d'usance
                Nullable<decimal> spotRate = GetFxRate(pPayerReceiverInfo, UsanceDelayDate, Currency1, Currency2, QuoteBasis);
                //PricingValues setting
                m_PricingValues = new PricingValues(spotRate, Currency1, Currency2);
            }
            else
            {
                //Cours en date de valo cours xdevise1 pour 1devise2
                Nullable<decimal> spotRate = GetFxRate(pPayerReceiverInfo, CommonValDate, Currency1, Currency2, QuoteBasis);

                // Interest rate UsanceDelayDate with a YieldCurve date = DailyClosing (CommonValDate)
                Pair<Nullable<decimal>, Nullable<decimal>> interestRate1 = InterestRateAndDiscountFactor(Currency1, UsanceDelayDate, pPayerReceiverInfo);
                Pair<Nullable<decimal>, Nullable<decimal>> interestRate2 = InterestRateAndDiscountFactor(Currency2, UsanceDelayDate, pPayerReceiverInfo);


                EFS_DayCountFraction dcf1 = GetDCF(Currency1);
                EFS_DayCountFraction dcf2 = GetDCF(Currency2);

                Nullable<decimal> forwardRate = null;
                if (spotRate.HasValue && interestRate1.First.HasValue & interestRate2.First.HasValue)
                {
                    decimal? denominator = 1 + (interestRate1.First.Value * dcf1.Factor);
                    decimal? numerator = 1 + (interestRate2.First.Value * dcf2.Factor);
                    forwardRate = spotRate * numerator / denominator;
                }
                //PricingValues setting
                m_PricingValues = new PricingValues(spotRate, forwardRate, Currency1, dcf1, interestRate1.First, Currency2, dcf2, interestRate2.First);
            }
        }
        #endregion SpotRateAndFxForwardCalculation

        #region SetUsanceDelayDate
        // EG 20180307 [23769] Gestion dbTransaction
        private void SetUsanceDelayDate()
        {
            IOffset offset = m_tradeLibrary.Product.ProductBase.CreateOffset(PeriodEnum.D, -2, DayTypeEnum.Business);
            IBusinessCenters businessCenters = offset.GetBusinessCentersCurrency(m_CS, null, Currency1, Currency2);
            m_UsanceDelayDate = Tools.ApplyOffset(m_CS, m_ExpiryDate, offset, businessCenters);
        }
        #endregion
        #region IsRowMustBeCalculate
        public override bool IsRowMustBeCalculate(DataRow pRow)
        {
            string eventCode = pRow["EVENTCODE"].ToString();
            return (EventCodeFunc.IsFxForward(eventCode) && (0 < m_ExpiryDate.CompareTo(CommonValDate)));
        }
        #endregion IsRowMustBeCalculate

        #region  Valorize
        /// <summary>
        /// Master process to valorize a MarkToMarket amount for an FxForward
        /// </summary>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Use slaveDbTransaction
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                DataRow[] rowEventProducts = GetRowEventProducts();
                foreach (DataRow rowEventProduct in rowEventProducts)
                {
                    dbTransaction = DataHelper.BeginTran(m_MarkToMarketGenProcess.Cs);

                    DataRow[] rowChilds = rowEventProduct.GetChildRows(DsEvents.ChildEvent);
                    foreach (DataRow rowChild in rowChilds)
                    {
                        decimal initialForwardRate = 0;
                        m_ExpiryDate = Convert.ToDateTime(rowChild["DTENDUNADJ"]);
                        bool isRowMustBeCalculate = IsRowMustBeCalculate(rowChild);
                        if (isRowMustBeCalculate)
                            isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_MarkToMarketGenProcess.ScanCompatibility_Event(Convert.ToInt32(rowChild["IDE"])));

                        if (isRowMustBeCalculate)
                        {
                            string eventType = rowChild["EVENTTYPE"].ToString();

                            m_ParamInstrumentNo.Value = Convert.ToInt32(rowChild["INSTRUMENTNO"]);
                            m_ParamStreamNo.Value = Convert.ToInt32(rowChild["STREAMNO"]);

                            // EG 20150319 [POC] Lecture de la QuoteBasis
                            DataRow rowStartCurrency2 = RowStartCurrency2;
                            PayerReceiverInfo payerReceiverInfoCur = new PayerReceiverInfo(this, rowStartCurrency2);

                            PayerReceiverInfoDet payer = new PayerReceiverInfoDet(PayerReceiverEnum.Payer,
                                payerReceiverInfoCur.Payer, null, payerReceiverInfoCur.BookPayer, null);
                            PayerReceiverInfoDet receiver = new PayerReceiverInfoDet(PayerReceiverEnum.Receiver,
                                payerReceiverInfoCur.Receiver, null, payerReceiverInfoCur.BookReceiver, null);

                            SetUsanceDelayDate();

                            DataRow rowCandidate = null;
                            if (EventTypeFunc.IsNonDeliverable(eventType))
                            {
                                rowCandidate = RowSettlementCurrency;
                                initialForwardRate = GetInitialForwardRate(rowCandidate);
                            }
                            else if (EventTypeFunc.IsDeliverable(eventType))
                            {
                                rowCandidate = RowTerminationCurrency2;
                            }

                            if ((null != rowCandidate) && (m_MarginingMode.HasValue))
                            {
                                string currency = rowCandidate["UNIT"].ToString();
                                string eventClass = (IsDailyClosingInUsanceDelay ? EventClassFunc.SpotRate : EventClassFunc.ForwardRate);

                                // MGR en % du notionel ou du MTM
                                #region MarkToMarket Calculation
                                DataRowEvent rowInfo = new DataRowEvent(rowCandidate);
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 611), 1,
                                    new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                                    new LogParam(LogTools.IdentifierAndId(rowInfo.EventCode + " / " + rowInfo.EventType, rowInfo.Id))));

                                if (ArrFunc.IsFilled(m_MarkToMarketGenMQueue.crossMarginTrades))
                                {
                                    if (m_MarkToMarketGenMQueue.IsCrossMargining)
                                    {
                                        #region MarkToMarket par CrossMargin
                                        m_MarkToMarketAmount = CrossMarginMarkToMarketFxForward(dbTransaction, eventType, initialForwardRate, payerReceiverInfoCur);
                                        ret = (m_MarkToMarketAmount.HasValue ? Cst.ErrLevel.SUCCESS : Cst.ErrLevel.FAILURE);
                                    }
                                    #endregion MarkToMarket par CrossMargin
                                }
                                else
                                {
                                    #region MarkToMarket unitaire
                                    m_MarkToMarketAmount = MarkToMarketFxForward(dbTransaction, eventType, initialForwardRate, payerReceiverInfoCur);
                                    ret = (m_MarkToMarketAmount.HasValue ? Cst.ErrLevel.SUCCESS : Cst.ErrLevel.FAILURE);
                                    #endregion MarkToMarket unitaire
                                }
                                #endregion MarkToMarket Calculation

                                if (m_MarkToMarketGenMQueue.isEOD)
                                {
                                    // EG 20150402 MGR
                                    rowCandidate = GetRowAmountGroup();
                                    if (null == rowCandidate)
                                    {
                                        DateTime dtStart = Convert.ToDateTime(rowChild["DTSTARTADJ"]);
                                        rowCandidate = NewRowEvent(SlaveDbTransaction, rowChild, EventCodeFunc.LinkedProductClosing, EventTypeFunc.Amounts, dtStart,
                                            Convert.ToDateTime(rowChild["DTENDADJ"]), m_MarkToMarketGenProcess.AppInstance);
                                        m_DsEvents.DtEvent.Rows.Add(rowCandidate);
                                        DataRow rowEventClass = NewRowEventClass(Convert.ToInt32(rowCandidate["IDE"]), EventClassFunc.GroupLevel, dtStart, false);
                                        m_DsEvents.DtEventClass.Rows.Add(rowEventClass);
                                    }

                                    if (m_MarkToMarketAmount.HasValue)
                                    {
                                        // EG 20150608 [21011]
                                        payerReceiverInfoCur.Payer = (IsDealerBuyer ? m_Buyer.OTCmlId : m_Seller.OTCmlId);
                                        payerReceiverInfoCur.BookPayer = (IsDealerBuyer ? m_BookBuyer : m_BookSeller);
                                        payerReceiverInfoCur.Receiver = (IsDealerBuyer ? m_Seller.OTCmlId : m_Buyer.OTCmlId);
                                        payerReceiverInfoCur.BookReceiver = (IsDealerBuyer ? m_BookSeller : m_BookBuyer);
                                        //AddRowMarkToMarket(rowCandidate, m_MarkToMarketAmount, currency, eventClass, payerReceiverInfoCur);

                                        if (IsDealerBuyer)
                                        {
                                            // EG 20150706 [21021]
                                            payer.actor = new Pair<Nullable<int>, string>(m_Buyer.OTCmlId, m_Buyer.PartyName);
                                            payer.book = new Pair<Nullable<int>, string>(m_BookBuyer, string.Empty);
                                            receiver.actor = new Pair<Nullable<int>, string>(m_Seller.OTCmlId, m_Seller.PartyName);
                                            receiver.book = new Pair<Nullable<int>, string>(m_BookSeller, string.Empty);
                                        }
                                        else
                                        {
                                            // EG 20150706 [21021]
                                            payer.actor = new Pair<Nullable<int>, string>(m_Seller.OTCmlId, m_Seller.PartyName);
                                            payer.book = new Pair<Nullable<int>, string>(m_BookSeller, string.Empty);
                                            receiver.actor = new Pair<Nullable<int>, string>(m_Buyer.OTCmlId, m_Buyer.PartyName);
                                            receiver.book = new Pair<Nullable<int>, string>(m_BookBuyer, string.Empty);

                                        }
                                        AddRowMarkToMarket(rowCandidate, m_MarkToMarketAmount, currency, eventClass, payer, receiver);
                                    }

                                    // EG 20150407 (POC] Add UnrealizedMargin
                                    DataRow rowStartCurrency1 = RowStartCurrency1;
                                    DataRow rowEventDet = GetRowEventDetail(Convert.ToInt32(rowStartCurrency1["IDE"]));
                                    if (null != rowEventDet)
                                    {
                                        AddRowUnrealizedMargin(dbTransaction, rowCandidate, Convert.ToDecimal(rowStartCurrency1["VALORISATION"]), Currency2, 
                                            Convert.ToDecimal(rowEventDet["RATE"]), Currency1, Currency2, QuoteBasis);
                                    }
                                }
                                else
                                {
                                    // EG 20150402 MTM RAS
                                    // EG 20150608 [21011]
                                    //AddRowMarkToMarket(rowCandidate, m_MarkToMarketAmount, currency, eventClass, payerReceiverInfoCur);
                                    AddRowMarkToMarket(rowCandidate, m_MarkToMarketAmount, currency, eventClass, payer, receiver);
                                }


                                Update(dbTransaction, Convert.ToInt32(rowCandidate["IDE"]), false);

                            }
                        }
                    }
                    DataHelper.CommitTran(dbTransaction);

                }
                dbTransaction = null;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && (isException))
                {
                    try
                    {
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    catch { }
                }
            }
            return ret;
        }
        #endregion Valorize

        
        #region GetInitialForwardRate
        // EG 20150317 [POC] New
        private decimal GetInitialForwardRate(DataRow pRow)
        {
            decimal initialForwardRate = 0;
            m_ParamIdE.Value = Convert.ToInt32(pRow["IDE"]);
            DataRow rowEventDetail = GetRowEventDetail(Convert.ToInt32(pRow["IDE"]));
            if (null != rowEventDetail)
                initialForwardRate = Convert.IsDBNull(rowEventDetail["RATE"]) ? 0 : Convert.ToDecimal(rowEventDetail["RATE"]);
            return initialForwardRate;
        }
        #endregion GetInitialForwardRate
        #region CompareMarginRequirementToInitialMargin
        /// <summary>
        /// Mode MarkToMarket : Compare le montant valorisé et marginé avec le montant InitialMargin et retourne :
        /// Si GAGNANT : Le plus petit
        /// Si PERDANT : Le plus grand
        /// </summary>
        /// <param name="pMarginRequirement"></param>
        /// <returns></returns>
        private decimal CompareMarginRequirementToInitialMargin(decimal pMarginRequirement)
        {
            decimal amount = Math.Abs(pMarginRequirement);
            if (m_MarginingMode.Value == Cst.MarginingMode.MarkToMarketAmount)
            {
                Nullable<decimal> initialMargin = InitialMarginAmount;
                if ((IsDealerBuyer && (0 < pMarginRequirement)) || ((false == IsDealerBuyer) && (pMarginRequirement < 0)))
                {
                    // GAGNANT
                    amount = Math.Min(amount, initialMargin ?? amount);
                }
                else
                {
                    // PERDANT
                    amount = Math.Max(amount, initialMargin ?? 0);
                }
            }
            return amount;
        }
        #endregion CompareMarginRequirementToInitialMargin

        #region AddRowUnrealizedMargin
        // EG 20150706 [21021] Pair<int, Nullable<int>> payer|receiver
        // EG 20180307 [23769] Gestion dbTransaction
        private void AddRowUnrealizedMargin(IDbTransaction pDbTransaction, DataRow pRowAMT, decimal pAmount, string pCurrency, decimal pInitialRate, string pCurrency1, string pCurrency2, QuoteBasisEnum pQuoteBasis)
        {
            KeyQuote keyQuote = new KeyQuote(m_CS, CommonValDate, null, QuotationSideEnum.OfficialClose);
            Nullable<decimal> closingRate = GetFxRate(keyQuote, pCurrency1, pCurrency2, pQuoteBasis);
            Pair<int, Nullable<int>> payer = new Pair<int, Nullable<int>>();
            Pair<int, Nullable<int>> receiver = new Pair<int, Nullable<int>>();
            Pair<Nullable<decimal>, string> unrealizedMarginAmount = base.SetUnrealizedMargin(pDbTransaction, pAmount, 1, pCurrency, closingRate, pInitialRate, ref payer, ref receiver);

            AddRowUnrealizedMargin(pDbTransaction, pRowAMT, unrealizedMarginAmount.First, unrealizedMarginAmount.Second, pInitialRate, closingRate.Value, payer, receiver);
        }
        #endregion AddRowUnrealizedMargin
        #endregion Methods
    }
    #endregion MarkToMarketGenProcessFX
}

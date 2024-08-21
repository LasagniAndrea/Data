#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresService;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Curve;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.PosRequest;
//
using FixML.Interface;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process
{
    #region DSTQuoteInfo
    public class DSTQuoteInfo : QuoteInfo, ICloneable
    {
        #region Members
        // EG 20190716 [VCL : New FixedIncome] New
        // EG 20190730 Upd (change accrualsDCF type)
        public Pair<EFS_DayCountFraction, EFS_DayCountFraction> accrualsDCF;
        #endregion Members
        #region Constructors
        public DSTQuoteInfo() { }
        public DSTQuoteInfo(EventsValProcessDST pProcess, NextPreviousEnum pType) : 
            this(pProcess, pType, pProcess.DebtSecurityTransactionContainer.ClearingBusinessDate.Date) 
        {  
        }
        public DSTQuoteInfo(EventsValProcessDST pProcess, NextPreviousEnum pType, Nullable<DateTime> pDate)
            : base(pProcess, pType, pDate)
        {
            Pair<Cst.UnderlyingAsset, int> _underlyingAsset = pProcess.DebtSecurityTransactionContainer.GetUnderlyingAsset(pProcess.Process.Cs);
            PosKeepingAsset posKeepingAsset = pProcess.DebtSecurityTransactionContainer.ProductBase.CreatePosKeepingAsset(_underlyingAsset.First);
            posKeepingAsset.idAsset = pProcess.DebtSecurityTransactionContainer.AssetDebtSecurity.IdAsset;
            posKeepingAsset.identifier = pProcess.DebtSecurityTransactionContainer.AssetDebtSecurity.Identifier;
            dtQuote = posKeepingAsset.GetOfficialCloseQuoteTime(dtBusiness);
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            DSTQuoteInfo clone = new DSTQuoteInfo
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
    #region EventsValProcessDST
    // EG 20190716 [VCL : FixedIncome] Refactoring
    // EG 20190823 [FIXEDINCOME] Upd (Perpetual|Ordinary DebtSecurity)
    public class EventsValProcessDST : EventsValProcessBase
    {
        #region Members
        private readonly CommonValParameters m_Parameters;
        private DebtSecurityTransactionContainer m_DebtSecurityTransactionContainer;
        // EG 20190716 [VCL : New FixedIncome] New Use for calculation MKV, MKP, MKA and UMG
        private List<Pair<string, IMoney>> m_LPCAmounts;
        private List<Pair<AssetMeasureEnum, decimal>> m_LPCPriceAndRate;
        private DataSetEventTrade m_DsEventsDebtSecurity;
        private SQL_AssetDebtSecurity m_SqlAssetDebtSecurity; 
        private bool isDebSecurityOrdinary;
        #endregion Members
        #region Accessors
        // EG 20190716 [VCL : FixedIncome] New
        private Quote_DebtSecurityAsset Quote_DebtSecurityAsset
        {
            get { return m_Quote as Quote_DebtSecurityAsset; }
        }

        #region DebtSecurityTransactionContainer
        public DebtSecurityTransactionContainer DebtSecurityTransactionContainer
        {
            get { return m_DebtSecurityTransactionContainer; }
        }
        #endregion DebtSecurityTransactionContainer

        #region Parameters
        public override CommonValParameters Parameters
        {
            get { return m_Parameters; }
        }
        #endregion Parameters
        #endregion Accessors
        #region Constructors
        public EventsValProcessDST(EventsValProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pProcess, pDsTrade, pTradeLibrary,pProduct)
        {
            m_Parameters = new CommonValParametersDST();
            if (ProcessBase.ProcessCallEnum.Master == pProcess.ProcessCall)
                pProcess.ProcessCacheContainer = new ProcessCacheContainer(pProcess.Cs, (IProductBase)pProduct);

        }
        #endregion Constructors
        #region Methods
        #region EndOfInitialize
        // EG 20180502 Analyse du code Correction [CA2214]
        // EG 20191025 Upd
        public override void EndOfInitialize()
        {
            //if ((false == m_tradeLibrary.dataDocument.currentProduct.isStrategy) && IsEndOfDayProcess )
            if ((false == m_tradeLibrary.DataDocument.CurrentProduct.IsStrategy) && (ProcessBase.ProcessCallEnum.Slave == Process.ProcessCall))
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
        /// 1./ Le nombre de tables EVENTXXX chargées est réduit : EVENT|EVENTCLASS|EVENTDET
        /// 2./ La date DTBUSINESS est utilisé pour restreindre le nombre d'EVTS chargé
        /// tels que DtBusiness between DTSTARTADJ and DTENDADJ
        /// </summary>
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20150617 [20665]
        public override void InitializeDataSetEvent()
        {
            m_DsEvents = new DataSetEventTrade(m_CS, SlaveDbTransaction,  ParamIdT);
            Nullable<DateTime> dtBusiness = null;
            if (IsEndOfDayProcess)
                dtBusiness = m_Quote.time;
            // EG 20150617 [20665] New parameter null (DtBusinessPrev)
            m_DsEvents.Load(EventTableEnum.Class | EventTableEnum.Detail, dtBusiness, null);
        }
        #endregion InitializeDataSetEvent

        #region CalculationDebtSecurityTransaction
        // EG 20190716 [VCL : FixedIncome] New
        private Cst.ErrLevel CalculationDebtSecurityTransaction(DSTQuoteInfo pQuote)
        {
            DSTQuoteInfo _quotePrev = new DSTQuoteInfo(this, NextPreviousEnum.Previous);
            m_PosAvailableQuantity = PosKeepingTools.GetAvailableQuantity(m_EventsValProcess.Cs, SlaveDbTransaction, pQuote.dtBusiness, m_EventsValProcess.CurrentId);
            if ((null != m_Quote) && m_Quote.eodComplementSpecified)
                m_PosQuantityPrevAndActionsDay = m_Quote.eodComplement.posQuantityPrevAndActionsDay;
            else
                m_PosQuantityPrevAndActionsDay = PosKeepingTools.GetPreviousAvailableQuantity(m_EventsValProcess.Cs, SlaveDbTransaction, pQuote.dtBusiness.Date, m_EventsValProcess.CurrentId);

            Calculation_LPC_AMT(pQuote, true);

            #region Calculations at ClearingDate+n <= DtMarket (HORS EOD)
            bool isQuotationToCalc = (false == IsEndOfDayProcess);
            int guard = 0;

            DateTime dtQuotation = _quotePrev.dtQuote;
            _ = (DSTQuoteInfo)_quotePrev.Clone();
            while (isQuotationToCalc)
            {
                guard++;
                if (guard == 999)
                {
                    string msgException = "Incoherence during the calculation. Infinite loop detected" + Cst.CrLf;
                    throw new Exception(msgException);
                }
                DSTQuoteInfo _quoteNextInfo = new DSTQuoteInfo(this, NextPreviousEnum.Next, dtQuotation);
                isQuotationToCalc = (_quoteNextInfo.dtBusiness <= m_EntityMarketInfo.DtMarket);
                if (isQuotationToCalc)
                    isQuotationToCalc = _quoteNextInfo.SetQuote(this);

                if (isQuotationToCalc)
                {
                    m_PosAvailableQuantityPrev = m_PosAvailableQuantity;
                    m_PosAvailableQuantity = PosKeepingTools.GetAvailableQuantity(m_EventsValProcess.Cs, SlaveDbTransaction, _quoteNextInfo.dtBusiness, m_EventsValProcess.CurrentId);
                    Calculation_LPC_AMT(_quoteNextInfo, false);
                }
                if ((DataRowState.Modified == _quoteNextInfo.rowState) || (false == isQuotationToCalc))
                    break;

                _ = (DSTQuoteInfo)_quoteNextInfo.Clone();
                dtQuotation = _quoteNextInfo.dtQuote;
            }
            #endregion Calculations at ClearingDate+n <= DtMarket (HORS EOD)
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CalculationDebtSecurityTransaction
        #region CalculationDebtSecurityStream
        // EG 20170412 [23081] Gestion  dbTransaction et SlaveDbTransaction
        // EG 20180502 Analyse du code Correction [CA2200]
        // EG 20190716 [VCL : FixedIncome] Refactoring
        // EG 20190823 [FIXEDINCOME] Refactoring (Perpetual|Ordinary DebtSecurity)
        // EG 20191011 Upd
        private Cst.ErrLevel CalculationDebtSecurityStream(DateTime pDate)
        {
            #region Lecture du stream (DSS- FIX|FLO)
            DataRow _rowStream_DSE = m_DsEventsDebtSecurity.DtEvent.Select("EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.DebtSecurityStream)).FirstOrDefault();
            if (_rowStream_DSE != default(DataRow))
            {
                // Lecture des événements d'intérêts du stream (TITRE)
                IEnumerable<DataRow> lstRowInterest_DSE =
                    _rowStream_DSE.GetChildRows(m_DsEventsDebtSecurity.ChildEvent).Where(row => EventTypeFunc.IsInterest(row["EVENTTYPE"].ToString()));

                DataRow _rowInterest_DSE = null;

                if (null != lstRowInterest_DSE)
                {
                    // Lecture de la période d'intérêts en cours (TITRE)
                    _rowInterest_DSE = lstRowInterest_DSE.FirstOrDefault(row =>
                        (Convert.ToDateTime(row["DTSTARTUNADJ"]) < pDate) && (pDate <= Convert.ToDateTime(row["DTENDUNADJ"])));

                    if (_rowInterest_DSE != default(DataRow))
                    {
                        DateTime startDate = Convert.ToDateTime(_rowInterest_DSE["DTSTARTUNADJ"]);
                        DateTime endDate = Convert.ToDateTime(_rowInterest_DSE["DTENDUNADJ"]);
                        int idE = Convert.ToInt32(_rowInterest_DSE["IDE"]);

                        // Lecture des EVENTCLASS de la période d'intérêts en cours (TITRE)
                        List<DataRow> lstRowEventClass = _rowInterest_DSE.GetChildRows(m_DsEventsDebtSecurity.ChildEventClass).ToList();
                        if (null != lstRowEventClass)
                        {
                            // Lecture de EVENTCLASS = RECORDDATE de la période d'intérêts en cours (TITRE)
                            DataRow _rowRecordDate_DSE = lstRowEventClass.FirstOrDefault(row =>
                            (Convert.ToInt32(row["IDE"]) == idE) && (EventClassFunc.IsRecordDate(row["EVENTCLASS"].ToString())));
                            if (_rowRecordDate_DSE != default(DataRow))
                                endDate = Convert.ToDateTime(_rowRecordDate_DSE["DTEVENT"]);
                        }

                        // EG 20191011 Add TEST isTradeDay to valorize isCalculation
                        bool istradeDay = (m_DebtSecurityTransactionContainer.ClearingBusinessDate == pDate);
                        bool isCalculation = (startDate < pDate) && ((endDate == pDate) || ((endDate < pDate) && istradeDay));

                        if (isCalculation)
                        {
                            bool isException = false;
                            IDbTransaction dbTransaction = null;
                            if (null != SlaveDbTransaction)
                                dbTransaction = SlaveDbTransaction;

                            try
                            {
                                if (null == SlaveDbTransaction)
                                    dbTransaction = DataHelper.BeginTran(m_EventsValProcess.Cs);

                                if (false == isDebSecurityOrdinary)
                                {
                                    // Création de la prochaine ligne d'intérêts si elle n'existe pas
                                    if (lstRowInterest_DSE.FirstOrDefault(row2 => 
                                        Convert.ToDateTime(row2["DTSTARTUNADJ"]) == Convert.ToDateTime(_rowInterest_DSE["DTENDUNADJ"])) == default(DataRow))
                                        AddRowNextInterest_DSE(dbTransaction, _rowInterest_DSE);
                                }

                                m_PosAvailableQuantity = PosKeepingTools.GetAvailableQuantity(m_EventsValProcess.Cs, pDate, m_EventsValProcess.CurrentId);
                                m_PosQuantityPrevAndActionsDay = PosKeepingTools.GetPreviousAvailableQuantity(m_EventsValProcess.Cs, pDate, m_EventsValProcess.CurrentId);

                                // Recherche de la ligne d'intérêts en cours (TRADE)
                                DataRow _rowInterest_DST = GetRowEventBySource(idE);

                                // Recherche de la ligne détail de la ligne d'intérêt en cours (TRADE)
                                DataRow _rowDetail_DST = null;
                                if (null != _rowInterest_DST)
                                    _rowDetail_DST = GetRowEventDetail(Convert.ToInt32(_rowInterest_DST["IDE"]));

                                if (0 == m_PosAvailableQuantity)
                                {
                                    // Trade jour et Quantité dispo = 0 pas d'insertion de l'événement d'intérêt donc si existe on supprime.
                                    if (null != _rowInterest_DST)
                                    {
                                        _rowInterest_DST.Delete();
                                        m_DsEvents.Update(dbTransaction, IsEndOfDayProcess);
                                    }
                                }
                                else
                                {
                                    if (null == _rowInterest_DST)
                                    {
                                        // La ligne N'EXISTE PAS (TRADE) alors Création par duplication de celle présente (TITRE)
                                        #region EVENT
                                        DataRow _rowStream = GetRowEventBySource(Convert.ToInt32(_rowStream_DSE["IDE"]));
                                        SQLUP.GetId(out int newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
                                        _rowInterest_DST = m_DsEvents.DtEvent.NewRow();
                                        _rowInterest_DST.ItemArray = (object[])_rowInterest_DSE.ItemArray.Clone();
                                        _rowInterest_DST.BeginEdit();
                                        _rowInterest_DST["IDT"] = _rowStream["IDT"];
                                        _rowInterest_DST["IDE"] = newIdE;
                                        _rowInterest_DST["IDE_SOURCE"] = _rowInterest_DSE["IDE"];
                                        _rowInterest_DST["IDE_EVENT"] = _rowStream["IDE"];
                                        _rowInterest_DST["SOURCE"] = m_EventsValProcess.AppInstance.ServiceName;
                                        _rowInterest_DST.EndEdit();
                                        m_DsEvents.DtEvent.Rows.Add(_rowInterest_DST);
                                        SetRowStatus(_rowInterest_DST, TuningOutputTypeEnum.OES);
                                        #endregion EVENT
                                        #region EVENTCLASS
                                        lstRowEventClass.ForEach(row =>
                                        {
                                            DataRow _newRow = m_DsEvents.DtEventClass.NewRow();
                                            _newRow.ItemArray = (object[])row.ItemArray.Clone();
                                            _newRow.BeginEdit();
                                            _newRow["IDE"] = newIdE;
                                            _newRow["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_EventsValProcess.Cs, pDate);
                                            _newRow.EndEdit();
                                            m_DsEvents.DtEventClass.Rows.Add(_newRow);
                                        });
                                        #endregion EVENTCLASS
                                        #region EVENTDET
                                        DataRow _rowDetail_DSE = _rowInterest_DSE.GetChildRows(m_DsEventsDebtSecurity.ChildEventDet).FirstOrDefault();
                                        if (_rowDetail_DSE != default(DataRow))
                                        {
                                            _rowDetail_DST = m_DsEvents.DtEventDet.NewRow();
                                            _rowDetail_DST.ItemArray = (object[])_rowDetail_DSE.ItemArray.Clone();
                                            _rowDetail_DST.BeginEdit();
                                            _rowDetail_DST["IDE"] = newIdE;
                                            _rowDetail_DST["INTEREST"] = Convert.DBNull;
                                            _rowDetail_DST.EndEdit();
                                            m_DsEvents.DtEventDet.Rows.Add(_rowDetail_DST);
                                        }
                                        #endregion EVENTDET
                                    }

                                    if (null != _rowInterest_DST)
                                    {
                                        #region VALORISATION
                                        _rowInterest_DST["VALORISATION"] = Convert.ToDecimal(_rowInterest_DSE["VALORISATION"]) *
                                            (istradeDay ? m_PosAvailableQuantity : m_PosQuantityPrevAndActionsDay);
                                        _rowInterest_DST["IDA_REC"] = m_Buyer.OTCmlId;
                                        _rowInterest_DST["IDB_REC"] = m_BookBuyer;
                                        _rowInterest_DST["IDA_PAY"] = m_Seller.OTCmlId;
                                        _rowInterest_DST["IDB_PAY"] = m_BookSeller;

                                        if (null != _rowDetail_DST)
                                            _rowDetail_DST["DAILYQUANTITY"] = (istradeDay ? m_PosAvailableQuantity : m_PosQuantityPrevAndActionsDay);
                                        #endregion VALORISATION

                                        Update(dbTransaction, Convert.ToInt32(_rowInterest_DST["IDE"]), false, IsEndOfDayProcess);
                                    }
                                }
                                if (null == SlaveDbTransaction)
                                {
                                    DataHelper.CommitTran(dbTransaction);
                                    dbTransaction = null;
                                }
                            }
                            catch (Exception)
                            {
                                isException = true;
                                throw;
                            }
                            finally
                            {
                                if ((null != dbTransaction) && (null == SlaveDbTransaction) && (isException))
                                {
                                    try { DataHelper.RollbackTran(dbTransaction); }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }
            #endregion Lecture des stream (DSS- FIX|FLO)
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CalculationDebtSecurityStream
        #region AddRowNextInterest_DSE
        /// <summary>
        /// Nouvelle ligne EVENT - période d'interêts sur le DebtSecurity
        /// </summary>
        // EG 20190823 [FIXEDINCOME] Refactoring (Perpetual|Ordinary DebtSecurity)
        private DataRow AddRowNextInterest_DSE(IDbTransaction pDbTransaction, DataRow pPreviousRowInterest_DSE)
        {
            DateTime previousCouponDate = Convert.ToDateTime(pPreviousRowInterest_DSE["DTENDUNADJ"]);
            IDebtSecurityTransaction debtSecurityTransaction = m_DebtSecurityTransactionContainer.DebtSecurityTransaction;

            IInterestRateStream stream = debtSecurityTransaction.DebtSecurity.Stream.FirstOrDefault();
            DataRow rowPreviousPeriod_DSE = pPreviousRowInterest_DSE.GetChildRows(m_DsEventsDebtSecurity.ChildEvent).Where(row =>
                    EventCodeFunc.IsCalculationPeriod(row["EVENTCODE"].ToString()) && EventTypeFunc.IsFixedRate(row["EVENTTYPE"].ToString())).FirstOrDefault();

            DataRow _rowInterest_DSE = null;
            if (stream != default(IInterestRateStream) && (rowPreviousPeriod_DSE != default(DataRow)))
            {
                ICalculationPeriodDates calculationPeriodDates = debtSecurityTransaction.CalcNextInterestPeriodDates(m_EventsValProcess.Cs, m_DebtSecurityTransactionContainer, previousCouponDate);
                EFS_PaymentDate paymentDate = stream.PaymentDates.Efs_PaymentDates.paymentDates.FirstOrDefault();


                int nbPeriods = calculationPeriodDates.Efs_CalculationPeriodDates.calculationPeriods.Count();
                SQLUP.GetId(out int newIdE, pDbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1 + nbPeriods);

                #region INT/INT (Payment)
                _rowInterest_DSE = m_DsEventsDebtSecurity.DtEvent.NewRow();
                _rowInterest_DSE.ItemArray = (object[])pPreviousRowInterest_DSE.ItemArray.Clone();
                _rowInterest_DSE.BeginEdit();
                _rowInterest_DSE["IDT"] = pPreviousRowInterest_DSE["IDT"];
                _rowInterest_DSE["IDE"] = newIdE;
                _rowInterest_DSE["IDE_EVENT"] = pPreviousRowInterest_DSE["IDE_EVENT"];

                _rowInterest_DSE["DTSTARTUNADJ"] = paymentDate.StartPeriod.unadjustedDate.DateValue;
                _rowInterest_DSE["DTSTARTADJ"] = paymentDate.StartPeriod.adjustedDate.DateValue;
                _rowInterest_DSE["DTENDUNADJ"] = paymentDate.EndPeriod.unadjustedDate.DateValue;
                _rowInterest_DSE["DTENDADJ"] = paymentDate.EndPeriod.adjustedDate.DateValue;

                _rowInterest_DSE["VALORISATION"] = Convert.DBNull;
                _rowInterest_DSE["VALORISATIONSYS"] = Convert.DBNull;

                _rowInterest_DSE["SOURCE"] = m_EventsValProcess.AppInstance.ServiceName;
                _rowInterest_DSE.EndEdit();
                m_DsEventsDebtSecurity.DtEvent.Rows.Add(_rowInterest_DSE);

                #region Class (Payment)
                AddRowClassForInterest_DSE(newIdE, paymentDate);
                #endregion Class (Payment)

                SetRowStatus(_rowInterest_DSE, TuningOutputTypeEnum.OES);

                    #region PER/FIX (Period)
                    EFS_CalculAmount calculAmount = null;
                    calculationPeriodDates.Efs_CalculationPeriodDates.calculationPeriods.ToList().ForEach((Action<EFS_CalculationPeriod>)(calculationPeriod =>
                    {
                        EFS_DayCountFraction dcf = calculationPeriod.DayCountFraction(stream.DayCountFraction);
                        EFS_Decimal rate = calculationPeriod.Rate((object)stream.CalculationPeriodAmount, "FixedRate",
                            stream.Rate("None"), stream.Rate("Initial"), stream.Rate("Final")) as EFS_Decimal;

                        newIdE++;
                        DataRow _rowPeriod_DSE = m_DsEventsDebtSecurity.DtEvent.NewRow();
                        _rowPeriod_DSE.ItemArray = (object[]) rowPreviousPeriod_DSE.ItemArray.Clone();
                        _rowPeriod_DSE.BeginEdit();
                        _rowPeriod_DSE["IDT"] = pPreviousRowInterest_DSE["IDT"];
                        _rowPeriod_DSE["IDE"] = newIdE;
                        _rowPeriod_DSE["IDE_EVENT"] = _rowInterest_DSE["IDE"];

                        _rowPeriod_DSE["DTSTARTUNADJ"] = calculationPeriod.StartPeriod.unadjustedDate.DateValue;
                        _rowPeriod_DSE["DTSTARTADJ"] = calculationPeriod.StartPeriod.adjustedDate.DateValue;
                        _rowPeriod_DSE["DTENDUNADJ"] = calculationPeriod.EndPeriod.unadjustedDate.DateValue;
                        _rowPeriod_DSE["DTENDADJ"] = calculationPeriod.EndPeriod.adjustedDate.DateValue;

                        DebtSecurityContainer _debtSecurityContainer = new DebtSecurityContainer(m_DebtSecurityTransactionContainer.DebtSecurityTransaction.DebtSecurity);
                        IMoney securityNominal = _debtSecurityContainer.GetNominal(m_DebtSecurityTransactionContainer.ProductBase);
                        calculAmount = new EFS_CalculAmount(securityNominal.Amount.DecValue, rate.DecValue,
                            calculationPeriod.StartPeriod.adjustedDate.DateValue,
                            calculationPeriod.EndPeriod.adjustedDate.DateValue, dcf.DayCountFraction, 
                            stream.CalculationPeriodDates.CalculationPeriodFrequency.Interval);

                        _rowPeriod_DSE["VALORISATION"] = calculAmount.calculatedAmount.Value;
                        _rowPeriod_DSE["VALORISATIONSYS"] = calculAmount.calculatedAmount.Value;

                        _rowPeriod_DSE["SOURCE"] = m_EventsValProcess.AppInstance.ServiceName;
                        _rowPeriod_DSE.EndEdit();
                        m_DsEventsDebtSecurity.DtEvent.Rows.Add(_rowPeriod_DSE);

                        #region Class (Period)
                        DataRow _rowClassPeriod_DSE = NewRowEventClass(m_DsEventsDebtSecurity, newIdE, EventClassFunc.GroupLevel, calculationPeriod.AdjustedStartPeriod.DateValue, false);
                        m_DsEventsDebtSecurity.DtEventClass.Rows.Add(_rowClassPeriod_DSE);
                        #endregion Class (Period)

                        #region Detail (Period)
                        DataRow _rowDetailPeriod_DSE = m_DsEventsDebtSecurity.DtEventDet.NewRow();
                        _rowDetailPeriod_DSE.BeginEdit();
                        _rowDetailPeriod_DSE["IDE"] = newIdE;
                        _rowDetailPeriod_DSE["RATE"] = rate.DecValue;
                        _rowDetailPeriod_DSE["MULTIPLIER"] = calculationPeriod.multiplierSpecified? calculationPeriod.Multiplier.DecValue: Convert.DBNull;
                        _rowDetailPeriod_DSE["DCF"] = dcf.DayCountFraction_FpML;
                        _rowDetailPeriod_DSE["DCFNUM"] = dcf.Numerator;
                        _rowDetailPeriod_DSE["TOTALOFYEAR"] = dcf.NumberOfCalendarYears;
                        _rowDetailPeriod_DSE["TOTALOFDAY"] = dcf.TotalNumberOfCalendarDays;
                        _rowDetailPeriod_DSE["SPREAD"] = calculationPeriod.spreadSpecified? calculationPeriod.Spread.DecValue: Convert.DBNull;
                        _rowDetailPeriod_DSE.EndEdit();

                        m_DsEventsDebtSecurity.DtEventDet.Rows.Add(_rowDetailPeriod_DSE);
                        #endregion Detail (Period)

                        SetRowStatus(_rowPeriod_DSE, TuningOutputTypeEnum.OES);
                    }));
                    #endregion PER/FIX (Period)

                decimal interestAmount = RoundingDebtSecurityUnitCouponAmount(calculAmount.calculatedAmount.Value);
                _rowInterest_DSE["VALORISATION"] = interestAmount;
                _rowInterest_DSE["VALORISATIONSYS"] = interestAmount;

                #region Detail (Period)
                DataRow _rowDetailInterest_DSE = m_DsEventsDebtSecurity.DtEventDet.NewRow();
                _rowDetailInterest_DSE.BeginEdit();
                _rowDetailInterest_DSE["IDE"] = _rowInterest_DSE["IDE"];
                _rowDetailInterest_DSE["INTEREST"] = interestAmount;
                _rowDetailInterest_DSE.EndEdit();
                m_DsEventsDebtSecurity.DtEventDet.Rows.Add(_rowDetailInterest_DSE);
                #endregion Detail (Period)

                m_DsEventsDebtSecurity.Update(pDbTransaction, IsEndOfDayProcess);
                m_DsEventsDebtSecurity.Update(pDbTransaction, Cst.OTCml_TBL.EVENTDET);
                #endregion INT/INT (Payment)
            }
            return _rowInterest_DSE;
        }
        #endregion AddRowNextInterest_DSE
        #region AddRowClassForInterest_DSE
        /// <summary>
        /// Nouvelle ligne EVENTCLASS
        /// </summary>
        // EG 20190823 [FIXEDINCOME] Refactoring (Perpetual|Ordinary DebtSecurity)
        private void AddRowClassForInterest_DSE(int pIdE, EFS_PaymentDate pPaymentDate)
        {
            DataRow _row = NewRowEventClass(m_DsEventsDebtSecurity, pIdE, EventClassFunc.Recognition, 
                pPaymentDate.recordDateAdjustmentSpecified ? pPaymentDate.AdjustedRecordDate.DateValue : pPaymentDate.AdjustedEndPeriod.DateValue, false);
            m_DsEventsDebtSecurity.DtEventClass.Rows.Add(_row);

            if (pPaymentDate.exDateAdjustmentSpecified)
            {
                _row = NewRowEventClass(m_DsEventsDebtSecurity, pIdE, EventClassFunc.ExDate, pPaymentDate.AdjustedExDate.DateValue, false);
                m_DsEventsDebtSecurity.DtEventClass.Rows.Add(_row);
            }
            if (pPaymentDate.recordDateAdjustmentSpecified)
            {
                _row = NewRowEventClass(m_DsEventsDebtSecurity, pIdE, EventClassFunc.RecordDate, pPaymentDate.AdjustedRecordDate.DateValue, false);
                m_DsEventsDebtSecurity.DtEventClass.Rows.Add(_row);
            }
            if (pPaymentDate.preSettlementSpecified)
            {
                _row = NewRowEventClass(m_DsEventsDebtSecurity, pIdE, EventClassFunc.PreSettlement, pPaymentDate.AdjustedPreSettlementDate.DateValue, false);
                m_DsEventsDebtSecurity.DtEventClass.Rows.Add(_row);
            }
            _row = NewRowEventClass(m_DsEventsDebtSecurity, pIdE, EventClassFunc.ValueDate, pPaymentDate.AdjustedPaymentDate.DateValue, false);
            m_DsEventsDebtSecurity.DtEventClass.Rows.Add(_row);
            _row = NewRowEventClass(m_DsEventsDebtSecurity, pIdE, EventClassFunc.Settlement, pPaymentDate.AdjustedPaymentDate.DateValue, true);
            m_DsEventsDebtSecurity.DtEventClass.Rows.Add(_row);
            if (pPaymentDate.rateCutOffDateSpecified)
            {
                _row = NewRowEventClass(m_DsEventsDebtSecurity, pIdE, EventClassFunc.RateCutOffDate, pPaymentDate.AdjustedRateCutOff.DateValue, false);
                m_DsEventsDebtSecurity.DtEventClass.Rows.Add(_row);
            }
        }
        #endregion AddRowClassForInterest_DSE

        #region Calculation_LPC_AMT
        /// <summary>
        /// Génération des évènement UMG, MKV, MKP, MKA
        /// </summary>
        // EG 20170412 [23081] Gestion  dbTransaction et SlaveDbTransaction 
        // EG 20180502 Analyse du code Correction [CA2200]
        // EG 20190716 [VCL : FixedIncome] Refactoring
        private void Calculation_LPC_AMT(DSTQuoteInfo pQuote,  bool pIsZeroQtyForced)
        {
            DataRow rowEventAMT = GetRowAmountGroup();
            m_ParamInstrumentNo.Value = Convert.ToInt32(rowEventAMT["INSTRUMENTNO"]);
            m_ParamStreamNo.Value = Convert.ToInt32(rowEventAMT["STREAMNO"]);

            Pair<string, int> quotedCurrency = new Pair<string, int>
            {
                First = m_DebtSecurityTransactionContainer.AssetDebtSecurity.IdC,
                Second = Tools.GetQuotedCurrencyFactor(CSTools.SetCacheOn(m_CS), null, m_DebtSecurityTransactionContainer.AssetDebtSecurity.IdC)
            };

            MarketValueAmountCalculation(pQuote);

            List<VAL_Event> lstEvents = new List<VAL_Event>()
            { 
                PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.MarketValuePrincipalAmount, quotedCurrency, pQuote, pIsZeroQtyForced),
                PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.MarketValueAccruedInterest, quotedCurrency, pQuote, pIsZeroQtyForced),
                PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.MarketValue, quotedCurrency, pQuote, pIsZeroQtyForced),
                PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.UnrealizedMargin, quotedCurrency, pQuote, pIsZeroQtyForced)
            };
            WriteClosingAmountGen(lstEvents, pQuote.dtBusiness);
        }
        #endregion Calculation_LPC_AMT

        #region Initialize
        /// <summary>
        /// Alimente les membres m_DebtSecurityTransactionContainer,_buyer,_bookBuyer,_seller,_bookSeller de la classe
        /// </summary>
        // EG 20190716 [VCL : FixedIncome] Upd
        // EG 20190823 [FIXEDINCOME] Upd (Perpetual|Ordinary DebtSecurity)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void Initialize()
        {
            m_DebtSecurityTransactionContainer = new DebtSecurityTransactionContainer((IDebtSecurityTransaction)m_CurrentProduct, TradeLibrary.DataDocument);
            DebtSecurityContainer _debtSecurityContainer = new DebtSecurityContainer(m_DebtSecurityTransactionContainer.DebtSecurityTransaction.DebtSecurity);
            isDebSecurityOrdinary = (_debtSecurityContainer.DebtSecurity.DebtSecurityType == DebtSecurityTypeEnum.Ordinary);
            m_EventsValProcess.ProcessCacheContainer.SetAsset(m_DebtSecurityTransactionContainer);
            string tradeStBusiness = DsTrade.DtTrade.Rows[0]["IDSTBUSINESS"].ToString();
            m_DebtSecurityTransactionContainer.InitRptSide(m_CS, (Cst.StatusBusiness.ALLOC.ToString() == tradeStBusiness));

            // Buyer / Seller
            IFixParty buyer = m_DebtSecurityTransactionContainer.GetBuyerSeller(FixML.Enum.SideEnum.Buy);
            IFixParty seller = m_DebtSecurityTransactionContainer.GetBuyerSeller(FixML.Enum.SideEnum.Sell);
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
            if ((null != Quote_DebtSecurityAsset) && Quote_DebtSecurityAsset.eodComplementSpecified)
            {
                m_IsPosKeeping_BookDealer = Quote_DebtSecurityAsset.eodComplement.isPosKeeping_BookDealer;
                idEntity = Quote_DebtSecurityAsset.eodComplement.idAEntity;
            }
            else
            {
                m_IsPosKeeping_BookDealer = m_DebtSecurityTransactionContainer.IsPosKeepingOnBookDealer(m_CS);
                idEntity = TradeLibrary.DataDocument.GetFirstEntity(CSTools.SetCacheOn(m_CS));
            }

            m_EntityMarketInfo = m_EventsValProcess.ProcessCacheContainer.GetEntityMarketLock(idEntity, 
                _debtSecurityContainer.GetIdMarket(), m_DebtSecurityTransactionContainer.IdA_Custodian);

            _ = m_DebtSecurityTransactionContainer.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER);

            #region Chargement des événements du titre associé à la transaction
            // EG 20190823 [FIXEDINCOME] (Perpetual|Ordinary DebtSecurity)
            if (isDebSecurityOrdinary)
                m_DsEventsDebtSecurity = new DataSetEventTrade(m_EventsValProcess.Cs, m_DebtSecurityTransactionContainer.DebtSecurityTransaction.SecurityAssetOTCmlId);
            #endregion Chargement des événements du titre associé à la transaction

            m_SqlAssetDebtSecurity = new SQL_AssetDebtSecurity(m_CS, Quote_DebtSecurityAsset.idAsset);
            m_SqlAssetDebtSecurity.LoadTable();

        }
        #endregion Initialize

        #region IsRowMustBeCalculate
        public override bool IsRowMustBeCalculate(DataRow pRow)
        {
            return true;
        }
        #endregion IsRowMustBeCalculate

        #region AccruedInteretRateCalculation
        // EG 20190716 [VCL : FixedIncome] New
        // EG 20190730 Upd (change accrualsDCF type)
        // EG 20190823 [FIXEDINCOME] Refactoring (Perpetual|Ordinary DebtSecurity)
        // EG 20191129 [FIXEDINCOME] Calcul DCF (Avec|sans Exdividend date
        // EG 20121204 [XXXXX] Correction calcul Denominateur sur méthode ACTACTICMA|ACTACTISMA
        // EG 20191209 Gestion des Stubs sur DCF (ICMA/ISMA)
        private Nullable<decimal> AccruedInteretRateCalculation(SecurityAssetContainer pSecurityAsset, DateTime pDate, out Pair<EFS_DayCountFraction, EFS_DayCountFraction> pDCF)
        {
            EFS_DayCountFraction fullDayCountFraction = null;
            EFS_DayCountFraction dayCountFraction = null;
            bool canBeCalculated = true;
            Nullable<decimal> ret = null;
            // Lecture du Stream du titre associé (TAUX FIXE|ZERO COUPON)
            IDebtSecurityStream securityStream = pSecurityAsset.DebtSecurity.Stream.FirstOrDefault();
            bool isZeroCoupon = Tools.IsPeriodZeroCoupon(securityStream.CalculationPeriodDates.CalculationPeriodFrequency.Interval);

            DataRow _rowStream_DSE = m_DsEventsDebtSecurity.DtEvent.Select(StrFunc.AppendFormat(@"EVENTCODE = '{0}' and EVENTTYPE = '{1}'",
                EventCodeFunc.DebtSecurityStream, isZeroCoupon ? EventTypeFunc.ZeroCoupon : EventTypeFunc.FixedLeg)).FirstOrDefault();

            if (isZeroCoupon)
                ret = 0;

            canBeCalculated = (_rowStream_DSE != default(DataRow)) && 
                              (securityStream != default(IDebtSecurityStream))  && 
                              securityStream.CalculationPeriodAmount.CalculationSpecified &&
                              securityStream.CalculationPeriodAmount.Calculation.RateFixedRateSpecified;

            if (canBeCalculated)
            {
                // Lecture des événements enfants du stream à taux fixe
                List<DataRow> lstRowStreamChilds = _rowStream_DSE.GetChildRows(m_DsEventsDebtSecurity.ChildEvent).ToList();
                if (null != lstRowStreamChilds)
                {
                    ISecurity security = pSecurityAsset.DebtSecurity.Security;

                    // Lecture de la période d'intérêts en cours (événements enfants du stream à taux fixe)
                    DataRow _rowInterest_DSE = lstRowStreamChilds.Find(row => EventTypeFunc.IsInterest(row["EVENTTYPE"].ToString()) &&
                                                    (Convert.ToDateTime(row["DTSTARTUNADJ"]) <= pDate) && (pDate <= Convert.ToDateTime(row["DTENDUNADJ"])));
                    if (null != _rowInterest_DSE)
                    {
                        // Lecture du taux facial du titre
                        decimal interestRate = securityStream.CalculationPeriodAmount.Calculation.RateFixedRate.InitialValue.DecValue;
                        DateTime dtStart = Convert.ToDateTime(_rowInterest_DSE["DTSTARTADJ"]);
                        DateTime dtEnd = Convert.ToDateTime(_rowInterest_DSE["DTENDADJ"]);
                        // Règle d'arrondi par défaut si non spécifiée
                        IRounding roundingAccruedRate = security.GetRoundingAccruedRate(RoundingDirectionEnum.Nearest, 8);
                        DayCountFractionEnum dayCountFractionEnum = security.GetDayCountFractionForAccruedRate(pSecurityAsset.DebtSecurity.Stream[0].CalculationPeriodAmount.Calculation.DayCountFraction);
                        fullDayCountFraction = new EFS_DayCountFraction(dtStart, dtEnd, dayCountFractionEnum, securityStream.PaymentDates.PaymentFrequency);

                        // On ajoute l'offset du règlement
                        DateTime dtCC = pDate;
                        if (security.OrderRulesSpecified && security.OrderRules.SettlementDaysOffsetSpecified)
                            dtCC = pSecurityAsset.CalcPaymentDate(m_CS, dtCC);


                        List<DataRow> lstRowEventClass = _rowInterest_DSE.GetChildRows(m_DsEventsDebtSecurity.ChildEventClass).ToList();
                        if (null != lstRowEventClass)
                        {
                            DataRow _rowExDate_DSE = lstRowEventClass.FirstOrDefault(row => EventClassFunc.IsExDate(row["EVENTCLASS"].ToString()));
                            bool isInExDividendPeriod = false;
                            if (_rowExDate_DSE != default(DataRow))
                                isInExDividendPeriod = (Convert.ToDateTime(_rowExDate_DSE["DTEVENT"]) <= pDate);
                            // dtEnd passé comme paramètre pDateEndCoupon (EFS_DayCountFraction)
                            StubEnum stub = StubEnum.None;
                            if (securityStream.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
                            {
                                if (pDate < securityStream.CalculationPeriodDates.FirstRegularPeriodStartDate.DateValue)
                                    stub = StubEnum.Initial;
                            }
                            if (securityStream.CalculationPeriodDates.LastRegularPeriodEndDateSpecified)
                            {
                                if (pDate > securityStream.CalculationPeriodDates.LastRegularPeriodEndDate.DateValue)
                                    stub = StubEnum.Final;
                            }
                            dayCountFraction = new EFS_DayCountFraction(
                                (isInExDividendPeriod ? dtEnd : dtStart), 
                                dtCC, dayCountFractionEnum, securityStream.PaymentDates.PaymentFrequency, dtEnd, stub);

                            EFS_Round round = new EFS_Round(roundingAccruedRate, interestRate * dayCountFraction.Factor);
                            ret = round.AmountRounded;

                        }
                    }
                }
            }
            pDCF = new Pair<EFS_DayCountFraction,EFS_DayCountFraction>(fullDayCountFraction, dayCountFraction);
            return ret;
        }
        #endregion AccruedInteretRateCalculation
        #region MarketValueAmountCalculation
        // EG 20190716 [VCL : FixedIncome] New
        /// <summary>
        /// Calcul du MKV, MKP, MKA et UMG du jour
        /// </summary>
        /// <param name="pQuote"></param>
        // EG 20190716 [VCL : FixedIncome] New
        // EG 20190730 Upd (parameter pQuoteNotFoundIsLog) 
        private void MarketValueAmountCalculation(DSTQuoteInfo pQuote)
        {
            m_LPCAmounts = new List<Pair<string, IMoney>>();
            m_LPCPriceAndRate = new List<Pair<AssetMeasureEnum, decimal>>();

            string idC = m_DebtSecurityTransactionContainer.AssetDebtSecurity.IdC;
            IMoney marketValueAccruedInterest = m_DebtSecurityTransactionContainer.ProductBase.CreateMoney(0, idC);
            IMoney marketValuePrincipalAmount = m_DebtSecurityTransactionContainer.ProductBase.CreateMoney(0, idC);
            IMoney marketValueGrossAmount = m_DebtSecurityTransactionContainer.ProductBase.CreateMoney(0, idC); 

            SecurityAssetContainer securityAsset = new SecurityAssetContainer(m_DebtSecurityTransactionContainer.GetSecurityAssetInDataDocument());
            Cst.PriceQuoteUnits priceQuoteUnits = ReflectionTools.ConvertStringToEnum<Cst.PriceQuoteUnits>(m_DebtSecurityTransactionContainer.DebtSecurityTransaction.Price.PriceUnits.Value);
            IMoney notional = securityAsset.GetNominal(m_DebtSecurityTransactionContainer.ProductBase).Clone();
            //notional.amount.DecValue *= m_PosAvailableQuantity;

#if DEBUG
            #region TEST EN DEBUG
            //================================================================================================================================ 
            // TEST DE CALCUL D'UN TAUX DE COUPON COURU SUR UNE PERIODE DONNEE (PERIODE EXDIV INCL.)
            // TRADEWEB FTSE GILT Closing Prices (https://reports.tradeweb.com/closing-prices/gilts/)
            //================================================================================================================================ 
            IOffset offset = m_DebtSecurityTransactionContainer.ProductBase.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.ExchangeBusiness);
            IDebtSecurityStream stream = m_DebtSecurityTransactionContainer.DebtSecurityTransaction.DebtSecurity.Stream[0];
            IBusinessCenters bcs = null;
            if (stream.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessCentersDefineSpecified)
                bcs = stream.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessCentersDefine;
            else if (stream.PaymentDates.PaymentDatesAdjustments.BusinessCentersDefineSpecified)
                bcs = stream.PaymentDates.PaymentDatesAdjustments.BusinessCentersDefine;

            //DateTime dtCC = m_EntityMarketInfo.dtMarket;
            //int i = 0;
            //while (i < 20)
            //    {
            //    dtCC = Tools.ApplyOffset(m_CS, dtCC, offset, bcs);
            //    Pair<EFS_DayCountFraction,EFS_DayCountFraction> dcf;
            //    Nullable<decimal> rate = AccruedInteretRateCalculation(securityAsset, dtCC, out dcf);
            //    if (rate.HasValue)
            //        System.Diagnostics.Debug.WriteLine(dtCC.ToShortDateString() + " => " + rate.Value.ToString());
            //    i++;
            //}
            #endregion TEST EN DEBUG
            //================================================================================================================================ 
#endif
            Nullable<decimal> cleanPrice = null;
            Nullable<decimal> dirtyPrice = null;
            Nullable<decimal> accruedInterestRate = null;
            SystemMSGInfo errReadQuote = null;
            EFS_Cash round = null;

            int idAsset = m_DebtSecurityTransactionContainer.AssetDebtSecurity.IdAsset;
            string assetIdentifier = m_DebtSecurityTransactionContainer.AssetDebtSecurity.Identifier;



            // Recherche du taux du coupon couru dans l'historique des prix.
            KeyQuoteAdditional keyQuoteAdditional = new KeyQuoteAdditional(AssetMeasureEnum.AccruedInterest, false);
            Quote quoteAccruedInterest = m_EventsValProcess.ProcessCacheContainer.GetQuoteLock(idAsset, pQuote.dtQuote, assetIdentifier,
                QuotationSideEnum.OfficialClose, Cst.UnderlyingAsset.Bond, keyQuoteAdditional, ref errReadQuote) as Quote;

            if ((null != quoteAccruedInterest) && quoteAccruedInterest.valueSpecified)
                accruedInterestRate = quoteAccruedInterest.value;

            if (StrFunc.IsFilled(pQuote.quote.assetMeasure))
            {
                AssetMeasureEnum assetMeasure = ReflectionTools.ConvertStringToEnum<AssetMeasureEnum>(pQuote.quote.assetMeasure);
                Quote quotePrice = null;
                switch (assetMeasure)
                {
                    case AssetMeasureEnum.CleanPrice:
                        // La mesure du prix de cotation présent de le message mQueue est de type : CleanPrice
                        // == > Recherche du DirtyPrice dans l'historique des prix.
                        quotePrice = m_EventsValProcess.ProcessCacheContainer.GetQuoteLock(idAsset, pQuote.dtQuote, assetIdentifier,
                        QuotationSideEnum.OfficialClose, Cst.UnderlyingAsset.Bond, new KeyQuoteAdditional(AssetMeasureEnum.DirtyPrice, false), ref errReadQuote);
                        if ((null != quotePrice) && quotePrice.valueSpecified)
                            dirtyPrice = quotePrice.value;
                        cleanPrice = pQuote.quote.value;
                        break;
                    case AssetMeasureEnum.DirtyPrice:
                        // La mesure du prix de cotation présent de le message mQueue est de type : DirtyPrice
                        // == > Recherche du CleanPrice dans l'historique des prix.
                        quotePrice = m_EventsValProcess.ProcessCacheContainer.GetQuoteLock(idAsset, pQuote.dtQuote, assetIdentifier,
                        QuotationSideEnum.OfficialClose, Cst.UnderlyingAsset.Bond, new KeyQuoteAdditional(AssetMeasureEnum.CleanPrice, false), ref errReadQuote);
                        if ((null != quotePrice) && quotePrice.valueSpecified)
                            cleanPrice = quotePrice.value;
                        dirtyPrice = pQuote.quote.value;
                        break;
                    default:
                        break;
                }
                }

            bool isaccruedInterestRateReading = accruedInterestRate.HasValue;
            bool isPriceReading = cleanPrice.HasValue || dirtyPrice.HasValue;
            bool isCleanAndDirtyPriceReading = cleanPrice.HasValue && dirtyPrice.HasValue;

            Pair<EFS_DayCountFraction, EFS_DayCountFraction> accDCF;
            Nullable<decimal> calcAccruedInterestRate = null;
            // Il existe au moins un des 2 prix CleanPrice ou DirtyPrice
            if (isPriceReading)
            {
                // Pour récupérer le DCF (récupéer le calcul du taux du coupon couru lorsque celui n'est pas dans l'historique)
                calcAccruedInterestRate = AccruedInteretRateCalculation(securityAsset, pQuote.dtQuote, out accDCF);
                if (isaccruedInterestRateReading)
                {
                    if (false == cleanPrice.HasValue)
                    {
                        // Le DirtyPrice et le taux du coupon couru sont présents dans l'historique
                        // => on en déduit le CleanPrice
                        cleanPrice = dirtyPrice.Value - accruedInterestRate.Value;
                    }
                    else if (false == dirtyPrice.HasValue)
                    {
                        // Le CleanPrice et le taux du coupon couru sont présents dans l'historique
                        // => on en déduit le DirtyPrice
                        dirtyPrice = cleanPrice.Value + accruedInterestRate.Value;
                    }
                }
                else
                {
                    // Le taux du coupon couru n'est pas présent dans l'historique
                    calcAccruedInterestRate = AccruedInteretRateCalculation(securityAsset, pQuote.dtQuote, out accDCF);
                    if (isCleanAndDirtyPriceReading)
                    {
                        // Les 2 prix sont présents dans l'historique
                        //-------------------------------------------
                        // => on en déduit le taux du coupon couru
                        accruedInterestRate = dirtyPrice.Value - cleanPrice.Value;
                    }
                    else
                    {
                        // Un seul prix est présent dans l'historique
                        //------------------------------------------------------------------
                        // 1 => Calcul du taux coupon couru à partir du taux facial du titre
                        // 2 => Détermination du prix non présent dans l'historique
                        //accruedInterestRate = AccruedInteretRateCalculation(securityAsset, pQuote.dtQuote, out accDCF);
                        accruedInterestRate = calcAccruedInterestRate;
                        if (cleanPrice.HasValue && accruedInterestRate.HasValue)
                            dirtyPrice = cleanPrice.Value + (accruedInterestRate.Value * 100);
                        else if (dirtyPrice.HasValue && accruedInterestRate.HasValue)
                            cleanPrice = dirtyPrice.Value - (accruedInterestRate.Value * 100);
                    }
                }

                #region GrossAmount
                // Calcul du GAM 
                marketValueGrossAmount = m_DebtSecurityTransactionContainer.CalcGrossAmount(m_CS);
                #endregion GrossAmount

                #region AccruedInterestAmount
                if (accruedInterestRate.HasValue)
                {
                    decimal rate = accruedInterestRate.Value;
                    if (isaccruedInterestRateReading)
                    {
                        if (Cst.PriceQuoteUnits.ParValueDecimal == quoteAccruedInterest.QuoteUnit)
                            rate /= 100;
                    }
                    else if (isCleanAndDirtyPriceReading)
                    {
                        if (Cst.PriceQuoteUnits.ParValueDecimal == priceQuoteUnits)
                            rate /= 100;
                    }
                    // Calcul du coupon couru sur la base de son taux lu ou calculé (MKA)
                    round = new EFS_Cash(m_CS, notional.Amount.DecValue * m_PosAvailableQuantity * rate, marketValueAccruedInterest.Currency);
                    marketValueAccruedInterest.Amount.DecValue = round.AmountRounded;
                }
                #endregion AccruedInterestAmount

                #region PrincipalAmount
                // Calcul du Principal amount (MKP)
                marketValuePrincipalAmount = m_DebtSecurityTransactionContainer.CalcPrincipalAmount(m_CS, notional.Amount.DecValue, notional.Currency, m_PosAvailableQuantity, cleanPrice.Value);
                #endregion PrincipalAmount
            }
            else
            {
                // ERROR
                #region Log error message
                // EG 20130620 Si Traitement EOD alors Message Warning + CodeErreur = QUOTENOTFOUND
                ProcessState _processState = new ProcessState(ProcessStateTools.StatusErrorEnum);
                if (m_Quote.isEOD)
                {
                    _processState.Status = ProcessStateTools.StatusEnum.WARNING;
                    _processState.CodeReturn = ProcessStateTools.CodeReturnQuoteNotFoundEnum;
                    m_EventsValProcess.ProcessState.SetProcessState(_processState);
                }

                // FI 20200623 [XXXXX] SetErrorWarning
                m_EventsValProcess.ProcessState.SetErrorWarning(_processState.Status);

               // EG 20131231 [19419]
                Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(_processState.Status), new SysMsgCode(SysCodeEnum.SYS, 5158), 2,
                    new LogParam(LogTools.IdentifierAndId(m_EventsValMQueue.GetStringValueIdInfoByKey("identifier"), m_DsTrade.IdT)),
                    new LogParam(LogTools.IdentifierAndId(m_Quote.idAsset_Identifier, m_Quote.idAsset)),
                    new LogParam(DtFunc.DateTimeToString(pQuote.dtQuote, DtFunc.FmtDateTime)),
                    new LogParam(AssetMeasureEnum.CleanPrice.ToString() + "|" + AssetMeasureEnum.DirtyPrice)));

                throw new SpheresException2(_processState);
                #endregion Log error message

            }

            // EG 20191209 Test Nullable
            int accruedMultiplier = ((false == isaccruedInterestRateReading) && (false == isCleanAndDirtyPriceReading)) ? 100 : 1;
            m_LPCPriceAndRate.Add(new Pair<AssetMeasureEnum, decimal>(AssetMeasureEnum.AccruedInterest, (accruedInterestRate ?? 0) * accruedMultiplier));
            m_LPCPriceAndRate.Add(new Pair<AssetMeasureEnum, decimal>(AssetMeasureEnum.CleanPrice, cleanPrice ?? 0));
            m_LPCPriceAndRate.Add(new Pair<AssetMeasureEnum, decimal>(AssetMeasureEnum.DirtyPrice, dirtyPrice ?? 0));

            m_LPCAmounts.Add(new Pair<string, IMoney>(EventTypeFunc.MarketValuePrincipalAmount, marketValuePrincipalAmount));
            m_LPCAmounts.Add(new Pair<string, IMoney>(EventTypeFunc.MarketValueAccruedInterest, marketValueAccruedInterest));
            m_LPCAmounts.Add(new Pair<string, IMoney>(EventTypeFunc.GrossAmount, marketValueGrossAmount));
            m_LPCAmounts.Add(new Pair<string, IMoney>(EventTypeFunc.Nominal, notional));
            pQuote.accrualsDCF = accDCF;
        }
        #endregion MarketValueAmountCalculation
        #region PrepareClosingAmountGen
        /// <summary>
        /// Ecriture des événements de cashflows (MKV, MKP, MKA et UMG
        /// </summary>
        /// <param name="pRowEventAMT">Evénement parent (LPC/AMT)</param>
        /// <param name="pEventType">Type d'événement à insérer (MKV, MKP, MKA ou UMG)</param>
        /// <param name="pQuotedCurrency"></param>
        /// <param name="pQuote"></param>
        /// <param name="pQuotePrev"></param>
        /// <param name="pIsZeroQtyForced"></param>
        /// <returns></returns>
        // EG 20190716 [VCL : FixedIncome] New
        // EG 20190730 Upd (Payer|Receiver on MKA - selon signe du taux) 
        // EG 2019102 Upd (ZeroCoupon)
        private VAL_Event PrepareClosingAmountGen(DataRow pRowEventAMT, string pEventType, Pair<string, int> pQuotedCurrency, DSTQuoteInfo pQuote, bool pIsZeroQtyForced)
        {
            Pair<Nullable<decimal>, string> closingAmount = new Pair<Nullable<decimal>, string>(null, string.Empty);

            VAL_Event @event = new VAL_Event();

            string eventCodeLink = EventCodeLink(EventTypeFunc.Amounts, pEventType, pQuote.quote.QuoteTiming.Value);

            @event.Value = GetRowAmount(pEventType, m_Quote.QuoteTiming.Value, pQuote.dtBusiness);
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
                    if (false == EventTypeFunc.IsTotalMargin(pEventType))
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

                decimal multiplier = Multiplier;
                bool isTradeDay = (m_DebtSecurityTransactionContainer.ClearingBusinessDate.Date == pQuote.dtBusiness);

                @event.Detail["DTACTION"] = pQuote.dtQuote;
                @event.Detail["QUOTEPRICE100"] = Convert.DBNull;
                @event.Detail["QUOTEDELTA"] = Convert.DBNull;
                @event.Detail["DCF"] = Convert.DBNull;
                @event.Detail["DCFNUM"] = Convert.DBNull;
                @event.Detail["DCFDEN"] = Convert.DBNull;
                @event.Detail["TOTALOFDAY"] = Convert.DBNull;
                @event.Detail["NOTIONALAMOUNT"] = Convert.DBNull;
                @event.Detail["NOTIONALREFERENCE"] = Convert.DBNull;

                @event.Qty = m_PosAvailableQuantity;

                IMoney grossAmount = m_LPCAmounts.Find(amount => amount.First == EventTypeFunc.GrossAmount).Second;
                IMoney principalAmount = m_LPCAmounts.Find(amount => amount.First == EventTypeFunc.MarketValuePrincipalAmount).Second;
                IMoney accruedInterest = m_LPCAmounts.Find(amount => amount.First == EventTypeFunc.MarketValueAccruedInterest).Second;
                IMoney notional = m_LPCAmounts.Find(amount => amount.First == EventTypeFunc.Nominal).Second;

                if (EventTypeFunc.IsAllMarketValue(pEventType))
                {
                    @event.Detail["CONTRACTMULTIPLIER"] = Multiplier;
                    @event.Detail["ASSETMEASURE"] = pQuote.quote.assetMeasure;
                    @event.Detail["QUOTETIMING"] = pQuote.quote.quoteTiming;
                    @event.Detail["QUOTEPRICE"] = m_LPCPriceAndRate.Find(item => item.First == AssetMeasureEnum.CleanPrice).Second;
                    @event.Detail["QUOTEPRICE100"] = m_LPCPriceAndRate.Find(item => item.First == AssetMeasureEnum.DirtyPrice).Second;
                    @event.Detail["RATE"] = m_LPCPriceAndRate.Find(item => item.First == AssetMeasureEnum.AccruedInterest).Second;
                    @event.Detail["NOTIONALREFERENCE"] = notional.Amount.DecValue;
                }

                if (EventTypeFunc.IsMarketValuePrincipalAmount(pEventType))
                {
                    #region MarketValuePrincipalAmount
                    closingAmount.First = principalAmount.Amount.DecValue;
                    closingAmount.Second = principalAmount.Currency;
                    payer.First = m_Seller.OTCmlId;
                    payer.Second = m_BookSeller;
                    receiver.First = m_Buyer.OTCmlId;
                    receiver.Second = m_BookBuyer;
                    #endregion MarketValuePrincipalAmount
                }
                else if (EventTypeFunc.IsMarketValueAccruedInterest(pEventType))
                {
                    #region MarketValueAccruedInterest
                    closingAmount.First = accruedInterest.Amount.DecValue;
                    closingAmount.Second = accruedInterest.Currency;
                    payer.First = (accruedInterest.Amount.DecValue >= 0) ? m_Seller.OTCmlId : m_Buyer.OTCmlId;
                    payer.Second = (accruedInterest.Amount.DecValue >= 0) ? m_BookSeller : m_BookBuyer;
                    receiver.First = (accruedInterest.Amount.DecValue >= 0) ? m_Buyer.OTCmlId : m_Seller.OTCmlId;
                    receiver.Second = (accruedInterest.Amount.DecValue >= 0) ? m_BookBuyer : m_BookSeller;

                    if ((null != pQuote.accrualsDCF) && (null != pQuote.accrualsDCF.First) && (null != pQuote.accrualsDCF.Second))
                    {
                        DayCountFractionEnum dcf = ReflectionTools.ConvertStringToEnum<DayCountFractionEnum>(pQuote.accrualsDCF.Second.DayCountFraction);
                        @event.Detail["DCF"] = pQuote.accrualsDCF.Second.DayCountFraction;
                        @event.Detail["DCFNUM"] = pQuote.accrualsDCF.Second.Numerator;
                        @event.Detail["DCFDEN"] = pQuote.accrualsDCF.Second.Denominator;
                        if (pQuote.accrualsDCF.Second.Numerator < 0)
                            @event.Detail["TOTALOFDAY"] = pQuote.accrualsDCF.First.Numerator + pQuote.accrualsDCF.Second.Numerator;
                        else
                            @event.Detail["TOTALOFDAY"] = pQuote.accrualsDCF.Second.Numerator;
                    }
                    #endregion MarketValueAccruedInterest
                }
                else if (EventTypeFunc.IsMarketValue(pEventType))
                {
                    #region MarketValue
                    closingAmount.First = principalAmount.Amount.DecValue + accruedInterest.Amount.DecValue;
                    closingAmount.Second = principalAmount.Currency;
                    payer.First = m_Seller.OTCmlId;
                    payer.Second = m_BookSeller;
                    receiver.First = m_Buyer.OTCmlId;
                    receiver.Second = m_BookBuyer;
                    @event.Detail["NOTIONALAMOUNT"] = principalAmount.Amount.DecValue;
                    @event.Detail["INTEREST"] = accruedInterest.Amount.DecValue;
                    #endregion MarketValue
                }
                else if (EventTypeFunc.IsUnrealizedMargin(pEventType))
                {
                    #region UnrealizedMargin
                    closingAmount.First = (m_PosAvailableQuantity > 0 ? (principalAmount.Amount.DecValue + accruedInterest.Amount.DecValue) - grossAmount.Amount.DecValue : 0);
                    closingAmount.Second = principalAmount.Currency;
                    payer.First = (0 < closingAmount.First) ? m_Seller.OTCmlId : m_Buyer.OTCmlId;
                    payer.Second = (0 < closingAmount.First) ? m_BookSeller : m_BookBuyer;
                    receiver.First = (0 < closingAmount.First) ? m_Buyer.OTCmlId : m_Seller.OTCmlId;
                    receiver.Second = (0 < closingAmount.First) ? m_BookBuyer : m_BookSeller;
                    if (0 < m_PosAvailableQuantity)
                    {
                        @event.Detail["NOTIONALREFERENCE"] = notional.Amount.DecValue;
                        @event.Detail["NOTIONALAMOUNT"] = principalAmount.Amount.DecValue;
                        @event.Detail["INTEREST"] = accruedInterest.Amount.DecValue;
                    }
                    #endregion UnrealizedMargin
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
        // EG 20190716 [VCL : FixedIncome] New
        private void RemoveClosingAmountGen(DateTime pDate)
        {

            IDbTransaction dbTransaction = null;
            if (null != SlaveDbTransaction)
                dbTransaction = SlaveDbTransaction;

            bool isException = false;
            try
            {
                if (null == SlaveDbTransaction)
                    dbTransaction = DataHelper.BeginTran(m_EventsValProcess.Cs);

                DataRow rowEventAMT = GetRowAmountGroup();
                if (null != rowEventAMT)
                {
                    m_ParamInstrumentNo.Value = Convert.ToInt32(rowEventAMT["INSTRUMENTNO"]);
                    m_ParamStreamNo.Value = Convert.ToInt32(rowEventAMT["STREAMNO"]);
                    //
                    RemoveClosingAmountGen(dbTransaction, rowEventAMT, pDate, EventTypeFunc.MarketValue);
                    RemoveClosingAmountGen(dbTransaction, rowEventAMT, pDate, EventTypeFunc.MarketValuePrincipalAmount);
                    RemoveClosingAmountGen(dbTransaction, rowEventAMT, pDate, EventTypeFunc.MarketValueAccruedInterest);
                    RemoveClosingAmountGen(dbTransaction, rowEventAMT, pDate, EventTypeFunc.UnrealizedMargin);
                }
                if (null == SlaveDbTransaction)
                {
                    DataHelper.CommitTran(dbTransaction);
                    dbTransaction = null;
                }
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && (null == SlaveDbTransaction) && isException)
                {
                    try
                    {
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    catch { }
                }
            }
        }
        // EG 20190716 [VCL : FixedIncome] New
        private void RemoveClosingAmountGen(IDbTransaction pDbTransaction, DataRow pRowAmountGroup, DateTime pDate, string pEventType)
        {
            string eventCodeLink = EventCodeLink(pRowAmountGroup["EVENTTYPE"].ToString(), pEventType, m_Quote.QuoteTiming.Value);
            DataRow[] rowChilds = pRowAmountGroup.GetChildRows(m_DsEvents.ChildEvent);
            foreach (DataRow rowChild in rowChilds)
            {
                if ((eventCodeLink == rowChild["EVENTCODE"].ToString()) &&
                    (pEventType == rowChild["EVENTTYPE"].ToString()) &&
                    (pDate.Date <= Convert.ToDateTime(rowChild["DTSTARTUNADJ"])))
                {
                    int idE = Convert.ToInt32(rowChild["IDE"]);
                    // Clear amount
                    rowChild["VALORISATION"] = Convert.DBNull;
                    rowChild["IDA_PAY"] = Convert.DBNull;
                    rowChild["IDB_PAY"] = Convert.DBNull;
                    rowChild["IDA_REC"] = Convert.DBNull;
                    rowChild["IDB_REC"] = Convert.DBNull;
                    // Clear amount details + Modify NOTE = 
                    DataRow rowEventDet = GetRowEventDetail(idE);
                    rowEventDet["QUOTEPRICE"] = Convert.DBNull;
                    rowEventDet["QUOTEPRICE100"] = Convert.DBNull;
                    // RD 20120821 [18087] Add QUOTEDELTA
                    rowEventDet["QUOTEDELTA"] = Convert.DBNull;
                    rowEventDet["DCF"] = Convert.DBNull;
                    rowEventDet["DCFNUM"] = Convert.DBNull;
                    rowEventDet["DCFDEN"] = Convert.DBNull;
                    rowEventDet["NOTIONALAMOUNT"] = Convert.DBNull;
                    rowEventDet["NOTIONALREFERENCE"] = Convert.DBNull;
                    rowEventDet["INTEREST"] = Convert.DBNull;

                    rowEventDet["NOTE"] = "Quotation was deleted or deactivated";
                    //
                    Update(pDbTransaction, idE, false, IsEndOfDayProcess);
                }
            }
        }
        #endregion RemoveClosingAmountGen

        #region Valorize
        // EG 20180502 Analyse du code Correction [CA2214]
        // EG 20190716 [VCL : FixedIncome] Upd
        // EG 20190823 [FIXEDINCOME] Refactoring (Perpetual|Ordinary DebtSecurity)
        // RD 20200911 [25475] Add try catch in order to Log the Exception
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (m_tradeLibrary.DataDocument.CurrentProduct.IsStrategy)
                return ret;

            try
            {
                //if (IsEndOfDayProcess)
                if (m_IsPosKeeping_BookDealer)
                {
                    DSTQuoteInfo _quoteInfo = new DSTQuoteInfo(this, NextPreviousEnum.None);

                    //Suppression d'un prix
                    if ((null != _quoteInfo) && (_quoteInfo.rowState == DataRowState.Deleted))
                    {
                        _quoteInfo.SetQuote(this);
                        RemoveClosingAmountGen(_quoteInfo.dtBusiness);
                    }

                    if (isDebSecurityOrdinary)
                    {
                        CalculationDebtSecurityStream(m_EntityMarketInfo.DtMarket);
                    }
                    else
                    {
                        lock (m_EventsValProcess.ProcessCacheContainer.EventsValLock)
                        {
                            #region Chargement des événements du titre associé à la transaction
                            m_DsEventsDebtSecurity = new DataSetEventTrade(m_EventsValProcess.Cs, m_DebtSecurityTransactionContainer.DebtSecurityTransaction.SecurityAssetOTCmlId);
                            #endregion Chargement des événements du titre associé à la transaction
                            CalculationDebtSecurityStream(m_EntityMarketInfo.DtMarket);
                        }
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
                            _quoteInfo.InitQuote(this, false);
                        }

                        bool isCalculation = (false == _quoteInfo.quote.QuoteTiming.HasValue) || (QuoteTimingEnum.Close == _quoteInfo.quote.QuoteTiming.Value);
                        if (isCalculation)
                            CalculationDebtSecurityTransaction(_quoteInfo);
                    }

                    //CalculationDebtSecurityStream();
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
    #endregion EventsValProcessDST
}

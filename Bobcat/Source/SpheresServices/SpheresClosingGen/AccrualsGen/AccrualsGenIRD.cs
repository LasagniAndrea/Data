#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
//
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
using System.Reflection;
#endregion Using Directives

namespace EFS.Process.Accruals
{
    #region AccrualsGenProcessIRD
    public class AccrualsGenProcessIRD : AccrualsGenProcessBase
    {
        #region Accessors
        #region Parameters
        public override CommonValParameters ParametersIRD
        {
            get { return m_Parameters; }
        }
        #endregion Parameters
        #endregion Accessors
        #region Constructors
        public AccrualsGenProcessIRD(AccrualsGenProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pProcess, pDsTrade, pTradeLibrary, pProduct)
        {
            m_Parameters = new CommonValParametersIRD();
        }
        #endregion Constructors
        #region Methods
        #region IsEventClassToAccountForBook
        private bool IsEventClassToAccountForBook(string pEventClass, int pIdBook)
        {
            bool isToAccount = false;
            SQL_Book book = new SQL_Book(m_CS, pIdBook);
            if (book.IsLoaded)
            {
                string accrualIntMethod = book.AccruedIntMethod;
                switch (accrualIntMethod)
                {
                    case "LINEAR":
                        accrualIntMethod = EventClassFunc.Linear;
                        break;
                    case "COMPOUNDING":
                        accrualIntMethod = EventClassFunc.Compounded;
                        break;
                    case "PRORATA":
                        accrualIntMethod = EventClassFunc.Prorata;
                        break;
                    default:
                        accrualIntMethod = "NATIVE";
                        break;
                }
                if (accrualIntMethod != "NATIVE")
                {
                    if (accrualIntMethod == pEventClass)
                        isToAccount = true;
                }
                else
                {
                    // A terminer pour récupérer CompoundingMethod du Trade
                    if (EventClassFunc.Linear == pEventClass)
                        isToAccount = true;
                }
            }
            return isToAccount;
        }
        #endregion IsEventClassToAccountForBook
        #region AddNewRowAccrual
        private void AddNewRowAccrual(DataRow pRow, bool pIsRowLinearAdded, bool pIsRowCompoundAdded)
        {
            if (pIsRowLinearAdded)
                AddNewRowAccrual2(pRow);
            if (pIsRowCompoundAdded)
                AddNewRowAccrual2(pRow);
        }
        private void AddNewRowAccrual2(DataRow pRow)
        {
            DataRow rowAccrualInterests = DsEvents.DtEvent.NewRow();
            rowAccrualInterests.ItemArray = (object[])pRow.ItemArray.Clone();
            rowAccrualInterests.BeginEdit();
            rowAccrualInterests["IDE"] = 0;
            rowAccrualInterests["IDE_EVENT"] = pRow["IDE"];
            rowAccrualInterests["EVENTCODE"] = EventCodeFunc.DailyClosing;
            rowAccrualInterests["EVENTTYPE"] = EventTypeFunc.AccrualInterests;
            rowAccrualInterests["SOURCE"] = m_AccrualsGenProcess.AppInstance.ServiceName;
            rowAccrualInterests["IDSTTRIGGER"] = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
            rowAccrualInterests["DTENDADJ"] = CommonValDate;
            rowAccrualInterests["DTENDUNADJ"] = CommonValDate;
            rowAccrualInterests.EndEdit();
            DsEvents.DtEvent.Rows.Add(rowAccrualInterests);
        }
        #endregion AddNewRowAccrual
        #region AddRowAccrual
        private void AddRowAccrual(DataRow pRowInterest, EFS_PaymentEvent pPaymentEvent)
        {
            EFS_DayCountFraction dcf = pPaymentEvent.GetDayCountFraction();
            bool isCompound = pPaymentEvent.IsCompounding;
            bool isKnownAmount = false;
            decimal nominal = 0;

            #region Read Nominal Period
            DataRow rowNominal = GetCurrentNominal(CommonValDate);
            if ((null != rowNominal) && (false == Convert.IsDBNull(rowNominal["VALORISATION"])))
            {
                nominal = Convert.ToDecimal(rowNominal["VALORISATION"]);
            }
            else
            {
                DataRow parentRow = pRowInterest.GetParentRow(m_DsEvents.ChildEvent);
                isKnownAmount = (null != parentRow) && (false == Convert.IsDBNull(parentRow["EVENTTYPE"]))
                                && (EventTypeFunc.IsKnownAmount(parentRow["EVENTTYPE"].ToString()));
                if (!isKnownAmount)
                {
                    dcf = new EFS_DayCountFraction(pPaymentEvent.StartDate, pPaymentEvent.EndDate, DayCountFractionEnum.DCF30360, pPaymentEvent.intervalFrequency);
                }
            }
            #endregion Read Nominal Period

            m_ParamIdE.Value = Convert.ToInt32(pRowInterest["IDE"]);
            bool isRowCompoundAdded = ((pPaymentEvent.IsPaymentRelativeToStartDate || isKnownAmount) == false);

            DataRow[] rowAccruals = GetRowAccrual();
            if (ArrFunc.IsEmpty(rowAccruals))
            {
                AddNewRowAccrual(pRowInterest, true, isRowCompoundAdded);
            }
            else
            {
                #region Update Row Accrual
                bool isModifAccrual = false;
                foreach (DataRow row in rowAccruals)
                {
                    DataRow[] rowEventsClass = row.GetChildRows(DsEvents.ChildEventClass);
                    foreach (DataRow rowEventClass in rowEventsClass)
                    {
                        if (CommonValDate == Convert.ToDateTime(rowEventClass["DTEVENT"]))
                        {
                            isModifAccrual = true;
                            // EG 20150608 [21011] Alimenbtation payer|receiver
                            row["IDA_PAY"] = pRowInterest["IDA_PAY"];
                            row["IDB_PAY"] = pRowInterest["IDB_PAY"];
                            row["IDA_REC"] = pRowInterest["IDA_REC"];
                            row["IDB_REC"] = pRowInterest["IDB_REC"];
                            row["VALORISATION"] = pRowInterest["VALORISATION"];
                            break;
                        }
                    }
                }
                if (false == isModifAccrual)
                    AddNewRowAccrual(pRowInterest, true, isRowCompoundAdded);
                #endregion Update Row Accrual
            }

            //CalculAmountAccrual
            if (null != DsEvents.DtEvent.GetChanges())
            {
                rowAccruals = GetRowAccrual();
                if (null != rowAccruals)
                {
                    int numRow = -1;
                    foreach (DataRow row in rowAccruals)
                    {
                        numRow++;
                        if (DataRowState.Unchanged != row.RowState)
                            CalculAmountAccrual(numRow, row, nominal, dcf, pPaymentEvent.IsPaymentRelativeToStartDate, isCompound, isKnownAmount);
                    }
                }
            }
        }
        #endregion AddRowAccrual
        #region CalculAmountAccrual
        /// <revision>
        ///     <version>1.1.5</version><date>20070412</date><author>EG</author>
        ///     <EurosysSupport>N° xxxxx</EurosysSupport>
        ///     <comment>
        ///     Gestion eventClass comptables.
        ///		</comment>
        /// </revision>
        /// <revision>
        ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
        ///     <comment>Ticket 15859
        ///     Add new parameter IntervalFrequency to EFS_DayCountFraction and EFS_EquivalentRate
        ///     </comment>
        /// </revision>
        /// <revision>
        ///     <version>2.0.1</version><date>20080319</date><author>PL</author>
        ///     <comment>Ticket 16107
        ///     Add isPeriodRemaining
        ///     </comment>
        /// </revision>
        /// <revision>
        ///     <version>2.0.1</version><date>20080327</date><author>RD</author>
        ///     <comment>Ticket 16107
        ///     1 - Manage KnownAmount with CalculationPeriod at StartDate
        ///     2 - Manage Interest with CalculationPeriod at StartDate
        ///     </comment>
        /// </revision>
        private Cst.ErrLevel CalculAmountAccrual(int pNumRow, DataRow pRowAccrual, decimal pNominal, EFS_DayCountFraction pDcf,
            bool pIsPayRelativeToStartDate, bool pIsCompound, bool pIsKnownAmount)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            DataRow rowParent = pRowAccrual.GetParentRow(DsEvents.ChildEvent);
            
            #region Accrued Interest Parameters calculated
            DateTime startDate = Convert.ToDateTime(rowParent["DTSTARTADJ"]);
            DateTime endDate = Convert.ToDateTime(rowParent["DTENDADJ"]);
            decimal nativeAmount = 0;
            if (false == Convert.IsDBNull(pRowAccrual["VALORISATION"]))
                nativeAmount = Convert.ToDecimal(pRowAccrual["VALORISATION"]);

            string currency = pRowAccrual["UNIT"].ToString();

            #endregion Accrued Interest Parameters calculated
            #region DayCountFraction
            EFS_DayCountFraction dcfTotal = new EFS_DayCountFraction(startDate, endDate, pDcf.DayCountFraction, pDcf.IntervalFrequency);
            //-- 20080327 RD Ticket 16107                     
            bool isPeriodRemaining = false;
            if (pIsPayRelativeToStartDate)
                isPeriodRemaining = m_AccrualsGenProcess.CheckAccruedIntPeriod(pRowAccrual["IDB_PAY"].ToString(), pRowAccrual["IDB_REC"].ToString());

            EFS_DayCountFraction dcfAccrued;
            if (isPeriodRemaining)
            {
                dcfAccrued = new EFS_DayCountFraction(CommonValDateIncluded, endDate, pDcf.DayCountFraction, pDcf.IntervalFrequency, endDate);
            }
            else
            {
                dcfAccrued = new EFS_DayCountFraction(startDate, CommonValDateIncluded, pDcf.DayCountFraction, pDcf.IntervalFrequency, endDate);
            }
            decimal fullCouponFraction = 0;
            if (null != dcfTotal)
            {
                fullCouponFraction = dcfTotal.Factor;
            }
            decimal accruedFraction = 0;
            if (null != dcfAccrued)
            {
                accruedFraction = dcfAccrued.Factor;
            }

            #endregion DayCountFraction
            #region Amount
            decimal compoundAmount = 0;

            #endregion Amount
            #region Rate
            decimal linearRate = 0;
            decimal compoundRate = 0;
            decimal linearAmount;
            #endregion Rate

            if (pIsPayRelativeToStartDate)
            {
                if (isPeriodRemaining)
                {
                    pNumRow = 3;
                }
                else
                {
                    pNumRow = 4;
                }
                linearAmount = RoundingCurrencyAmount(currency, (nativeAmount * accruedFraction / fullCouponFraction));
            }
            else if (pIsKnownAmount)
            {
                #region Simple amount
                linearAmount = RoundingCurrencyAmount(currency, (nativeAmount * accruedFraction / fullCouponFraction));
                #endregion Simple amount
            }
            else
            {

                #region Accruals Amounts and Rates Calculation
                #region Native rate / Equivalent Rate (Compound and simple rate calculation)
                decimal y = 0;
                EFS_EquivalentRate equivalentRate;
                if (pIsCompound)
                {
                    #region Compound Rate
                    decimal x = (nativeAmount / pNominal) + 1;
                    decimal y2 = (pDcf.Denominator * pDcf.NumberOfCalendarYears) + pDcf.Numerator;
                    if (y2 != 0)
                        y = pDcf.Denominator / y2;
                    compoundRate = Convert.ToDecimal(Math.Pow(Convert.ToDouble(x), Convert.ToDouble(y)) - 1);
                    #endregion Compound Rate
                    #region Simple rate
                    equivalentRate = new EFS_EquivalentRate(EquiRateMethodEnum.CompoundToSimple, startDate, endDate, compoundRate, pDcf.DayCountFraction, pDcf.IntervalFrequency);
                    #endregion Simple rate
                    _ = Math.Round(equivalentRate.compoundRate, 9, MidpointRounding.AwayFromZero);
                }
                else
                {
                    #region Simple Rate
                    y = pDcf.Factor;
                    if ((0 == pNominal) || (0 == y))
                        linearRate = 0;
                    else
                        linearRate = nativeAmount / (pNominal * y);
                    #endregion Simple Rate
                    #region Compound Rate
                    equivalentRate = new EFS_EquivalentRate(EquiRateMethodEnum.SimpleToCompound, startDate, endDate,
                        linearRate, pDcf.DayCountFraction, pDcf.IntervalFrequency, DayCountFractionEnum.ACTACTISDA.ToString(), pDcf.IntervalFrequency);
                    #endregion Compound Rate
                    _ = Math.Round(equivalentRate.simpleRate, 9, MidpointRounding.AwayFromZero);
                }
                #endregion Native rate / Equivalent Rate (Compound and simple rate calculation)

                linearRate = equivalentRate.simpleRate;
                compoundRate = equivalentRate.compoundRate;

                #region FullCoupon amount
                EFS_CalculAmount calculAmount = new EFS_CalculAmount(pNominal, linearRate, startDate, endDate, pDcf.DayCountFraction, pDcf.IntervalFrequency);
                decimal fullCouponAmount = RoundingCurrencyAmount(currency, calculAmount.calculatedAmount.Value);
                #endregion FullCoupon amount

                #region Linear result
                linearAmount = RoundingCurrencyAmount(currency, (fullCouponAmount * accruedFraction / fullCouponFraction));
                #endregion Linear result
                #region Actuarial result
                compoundAmount = pNominal * (Convert.ToDecimal(Math.Pow(Convert.ToDouble(1 + compoundRate), Convert.ToDouble(accruedFraction))) - 1);
                compoundAmount = RoundingCurrencyAmount(currency, compoundAmount);
                #endregion Actuarial result
                #endregion Accruals Amounts and Rates Calculation
            }

            #region DataRow Updated And Inserted
            int idE = Convert.ToInt32(pRowAccrual["IDE"]);
            m_ParamIdE.Value = idE;
            DataRow rowAccrualDetail = RowEventDetail;
            if (null == rowAccrualDetail)
            {
                rowAccrualDetail = m_DsEvents.DtEventDet.NewRow();
                m_DsEvents.DtEventDet.Rows.Add(rowAccrualDetail);
            }
            if (0 == idE)
                ret = SQLUP.GetId(out idE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                string eventClass = "***";
                #region Accrual EventClass
                DeleteAllRowEventClass();
                DataRow rowAccrualClass = RowEventClass;
                bool isExistRowAccrualClass = (null != rowAccrualClass);
                if (false == isExistRowAccrualClass)
                {
                    switch (pNumRow)
                    {
                        case 0:
                            eventClass = EventClassFunc.Linear;
                            break;
                        case 1:
                            eventClass = EventClassFunc.Compounded;
                            break;
                        case 2:
                            eventClass = EventClassFunc.Prorata;
                            break;
                        case 3:
                            eventClass = EventClassFunc.LinearDepRemaining;
                            break;
                        case 4:
                            eventClass = EventClassFunc.LinearDepreciation;
                            break;
                        default:
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "EventClass Error (numRow=" + pNumRow.ToString() + ")");
                    }
                    rowAccrualClass = DsEvents.DtEventClass.NewRow();
                    rowAccrualClass.BeginEdit();
                    rowAccrualClass["IDE"] = idE;
                    rowAccrualClass["EVENTCLASS"] = eventClass;
                    rowAccrualClass.EndEdit();
                    DsEvents.DtEventClass.Rows.Add(rowAccrualClass);
                }

                rowAccrualClass.BeginEdit();
                rowAccrualClass["DTEVENT"] = CommonValDate;
                rowAccrualClass["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, CommonValDate);
                rowAccrualClass["ISPAYMENT"] = false;
                rowAccrualClass.EndEdit();
                #endregion Accrual EventClass
                #region	Accrual Accounting EventClass (CLA , ...)

                bool isEventClassToAccountForBook = false;
                // EG 20150706 [210210]
                Nullable<int> idBook = null;
                if (false == Convert.IsDBNull(pRowAccrual["IDB_PAY"]))
                    idBook = Convert.ToInt32(pRowAccrual["IDB_PAY"]);
                else if (false == Convert.IsDBNull(pRowAccrual["IDB_REC"]))
                    idBook = Convert.ToInt32(pRowAccrual["IDB_REC"]);

                //
                if (idBook.HasValue)
                {
                    //Pour les intérêts fin de période, génération de ces arrêtés en fonction du paramétrage existant sur le BOOK (soit sur le LIN, soit sur le CMP)
                    isEventClassToAccountForBook = true;
                    if (!pIsPayRelativeToStartDate)
                        isEventClassToAccountForBook = IsEventClassToAccountForBook(rowAccrualClass["EVENTCLASS"].ToString(), idBook.Value);
                }

                if (isEventClassToAccountForBook)
                {
                    #region	Accrual Accounting EventClass (CLA , ...)
                    AddRowAccountingEventClass(idE);
                    #endregion	Accrual Accounting EventClass (CLA , ...)
                }
                #endregion Accrual EventClass CLA
                pRowAccrual.BeginEdit();
                pRowAccrual["IDE"] = idE;
                pRowAccrual["DTENDADJ"] = CommonValDate;
                pRowAccrual["DTENDUNADJ"] = CommonValDate;
                #region Accrual Event & EventDet
                rowAccrualDetail.BeginEdit();
                rowAccrualDetail["IDE"] = idE;
                rowAccrualDetail["DCF"] = dcfAccrued.DayCountFraction_FpML;
                rowAccrualDetail["DCFNUM"] = dcfAccrued.Numerator;
                rowAccrualDetail["DCFDEN"] = dcfAccrued.Denominator;
                rowAccrualDetail["TOTALOFYEAR"] = dcfAccrued.NumberOfCalendarYears;
                rowAccrualDetail["TOTALOFDAY"] = dcfAccrued.TotalNumberOfCalculatedDays;

                string eventType = pRowAccrual["EVENTTYPE"].ToString();
                if (EventTypeFunc.IsAccrualInterests(eventType) &&
                    (EventClassFunc.IsAccrualLinear(eventClass)) || EventClassFunc.IsLinearDepreciation(eventClass) || EventClassFunc.IsLinearDepRemaining(eventClass))
                {
                    pRowAccrual["VALORISATION"] = linearAmount;
                    pRowAccrual["VALORISATIONSYS"] = linearAmount;
                    rowAccrualDetail["RATE"] = linearRate == 0 ? Convert.DBNull : linearRate;
                }
                else if (EventTypeFunc.IsAccrualInterests(eventType) && EventClassFunc.IsAccrualCompound(eventClass))
                {
                    pRowAccrual["VALORISATION"] = compoundAmount;
                    pRowAccrual["VALORISATIONSYS"] = compoundAmount;
                    rowAccrualDetail["RATE"] = compoundRate == 0 ? Convert.DBNull : compoundRate;
                }
                pRowAccrual.EndEdit();
                SetRowStatus(pRowAccrual, Tuning.TuningOutputTypeEnum.OES);
                rowAccrualDetail.EndEdit();
                #endregion Accrual Event & EventDet
            }
            #endregion DataRow Updated And Inserted
            return ret;
        }
        #endregion CalculAmountAccrual


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        public override bool IsRowMustBeCalculate(DataRow pRow)
        {
            bool ret = true;
            DateTime startDate = Convert.ToDateTime(pRow["DTSTARTUNADJ"]);
            DateTime endDate = Convert.ToDateTime(pRow["DTENDUNADJ"]);
            string eventCode = pRow["EVENTCODE"].ToString();
            string eventType = pRow["EVENTTYPE"].ToString();
            //
            if (EventTypeFunc.IsInterest(eventType) || EventCodeFunc.IsCalculationPeriod(eventCode) ||
                EventCodeFunc.IsReset(eventCode) || EventCodeFunc.IsSelfAverage(eventCode) || EventCodeFunc.IsSelfReset(eventCode))
            {
                if ((CommonValDate < startDate) || (CommonValDate >= endDate))
                    ret = false;
            }
            else if (EventCodeFunc.IsDailyClosing(eventCode))
                ret = false;
            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRows"></param>
        /// <returns></returns>
        public override bool IsRowsEventCalculated(DataRow[] pRows)
        {
            foreach (DataRow row in pRows)
            {
                DateTime startDate = Convert.ToDateTime(row["DTSTARTUNADJ"]);
                DateTime endDate = Convert.ToDateTime(row["DTENDUNADJ"]);

                if (StatusCalculFunc.IsToCalculate(row["IDSTCALCUL"].ToString()))
                {
                    if (DtFunc.IsDateTimeFilled(CommonValDate))
                    {
                        if ((CommonValDate > startDate) && (CommonValDate <= endDate))
                        {
                            DataRow[] rowChilds = row.GetChildRows(DsEvents.ChildEvent);
                            if (0 != rowChilds.Length)
                                return IsRowsEventCalculated(rowChilds);
                            return false;
                        }
                    }
                    else
                        return false;
                }
            }
            return true;
        }


        /// <revision>
        ///     <build>23</build><date>20050808</date><author>PL</author>
        ///     <comment>
        ///     Add CancelEdit()
        ///     </comment>
        /// </revision>
        /// RD 20150525 [21011] Modify
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            ArrayList alSpheresException = new ArrayList();

            #region Payment Process
            DataRow[] rowsInterest = GetRowInterest();
            if (ArrFunc.IsFilled(rowsInterest))
            {
                foreach (DataRow rowInterest in rowsInterest)
                {
                    int idE = Convert.ToInt32(rowInterest["IDE"]);
                    bool isError = false;
                    m_ParamInstrumentNo.Value = Convert.ToInt32(rowInterest["INSTRUMENTNO"]);
                    m_ParamStreamNo.Value = Convert.ToInt32(rowInterest["STREAMNO"]);
                    // ScanCompatibility_Event
                    bool isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_AccrualsGenProcess.ScanCompatibility_Event(Convert.ToInt32(rowInterest["IDE"])));
                    // isRowMustBeCalculate
                    isRowMustBeCalculate = isRowMustBeCalculate && IsRowMustBeCalculate(rowInterest);
                    if (isRowMustBeCalculate)
                    {
                        try
                        {
                            ParametersIRD.Add(m_CS, m_tradeLibrary, rowInterest);
                            //RD 20150525 [21011] Faire le reject des modifications ci-dessous dans "finally"
                            //rowInterest.BeginEdit();

                            //RD 20150525 [21011] Synchronisation des acteurs/books avec la ligne Parent (Stream)                            
                            // EG 20150608 [21011] Mise en commentaires de la synchronisation (fait dans PaymentInfo via PayerPartyReference et ReceiverPartyReference)
                            //DataRow rowStream = rowInterest.GetParentRow(DsEvents.ChildEvent);
                            //rowInterest["IDA_PAY"] = rowStream["IDA_PAY"];
                            //rowInterest["IDB_PAY"] = rowStream["IDB_PAY"];
                            //rowInterest["IDA_REC"] = rowStream["IDA_REC"];
                            //rowInterest["IDB_REC"] = rowStream["IDB_REC"];

                            EFS_PaymentEvent paymentEvent = new EFS_PaymentEvent(CommonValDateIncluded, (CommonValProcessBase)this, rowInterest);
                            AddRowAccrual(rowInterest, paymentEvent);
                        }
                        catch (SpheresException2 ex)
                        {
                            isError = true;
                            if (ex.IsStatusError)
                                alSpheresException.Add(ex);
                        }
                        catch (Exception ex)
                        {
                            isError = true;
                            throw new SpheresException2("AccrualsGenProcessIRD.Valorize (RowInterest)", ex);
                        }
                        finally
                        {
                            //RD 20150525 [21011] Rejeter toutes les modifs "rowInterest" dans "EFS_PaymentEvent"
                            //RejectChangesInRowInterest(rowInterest, true);
                            //rowInterest.CancelEdit();
                            RejectChangesInRowInterest(rowInterest);
                            Update(idE, isError);
                        }
                    }
                }
            }
            #endregion Payment Process

            if (ArrFunc.IsFilled(alSpheresException))
            {
                ret = Cst.ErrLevel.ABORTED;
                SpheresException2[] spheresExceptions = (SpheresException2[])alSpheresException.ToArray(typeof(SpheresException2));

                foreach (SpheresException2 ex in spheresExceptions)
                {
                    Logger.Log(new LoggerData(ex));
                }
            }
            return ret;
        }
        #endregion Methods
    }
    #endregion AccrualsGenProcessIRD
}


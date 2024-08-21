#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML.Business;
using EfsML.Curve;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
#endregion Using Directives

namespace EFS.Process
{
    #region EventsValProcessRTS
    public partial class EventsValProcessRTS
    {
        #region Methods
        /// <summary>
        /// Valorisation des montants périodiques d'un ReturnSwap 
        /// </summary>
        /// <returns></returns>
        private Cst.ErrLevel ValorizeReturnSwap()
        {
            Cst.ErrLevel ret;
            try
            {
                RTSQuoteInfo _quoteInfo = new RTSQuoteInfo(this, NextPreviousEnum.None);
                // Valorisation de la jambe de rendement
                ret = ValorizeReturnLeg(_quoteInfo.dtBusiness);
                if ((Cst.ErrLevel.SUCCESS == ret) || (Cst.ErrLevel.FAILUREWARNING == ret))
                    // Valorisation de la jambe d'intérêt
                    ret = ValorizeInterestLeg(_quoteInfo.dtBusiness);
            }
            catch (SpheresException2 ex)
            {
                ret = Cst.ErrLevel.FAILURE;
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
        /// <summary>
        /// Valorisation de la jambe de rendement
        /// </summary>
        /// <param name="pCommonValDate"></param>
        /// <returns></returns>
        private Cst.ErrLevel ValorizeReturnLeg(DateTime pCommonValDate)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            List<SpheresException2> lstSpheresException = new List<SpheresException2>();

            bool isError = false;
            bool isRowMustBeCalculate = false;

            DataRow[] rowsReturnLegAmount = GetRowReturnLegAmount();
            if (ArrFunc.IsFilled(rowsReturnLegAmount))
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 601), 3,
                    new LogParam(LogTools.IdentifierAndId(m_EventsValProcess.MQueue.Identifier, m_EventsValProcess.MQueue.id)),
                    new LogParam("INT-TER / RLA")));

                if (null == m_ReturnSwapContainer.MainReturnLeg.First.Efs_ReturnLeg)
                    m_ReturnSwapContainer.MainReturnLeg.First.Efs_ReturnLeg = new EFS_ReturnLeg(m_EventsValProcess.Cs, m_ReturnSwapContainer.MainReturnLeg, TradeLibrary.DataDocument);

                IDbTransaction dbTransaction = null;
                if (null != SlaveDbTransaction)
                    dbTransaction = SlaveDbTransaction;

                bool isException = false;
                try
                {
                    if (null == SlaveDbTransaction)
                        dbTransaction = DataHelper.BeginTran(m_EventsValProcess.Cs);

                    foreach (DataRow rowReturnLegAmount in rowsReturnLegAmount)
                    {
                        isError = false;
                        m_ParamInstrumentNo.Value = Convert.ToInt32(rowReturnLegAmount["INSTRUMENTNO"]);
                        m_ParamStreamNo.Value = Convert.ToInt32(rowReturnLegAmount["STREAMNO"]);
                        int idE = Convert.ToInt32(rowReturnLegAmount["IDE"]);

                        // ScanCompatibility_Event
                        isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_EventsValProcess.ScanCompatibility_Event(idE));
                        // isRowMustBeCalculate
                        isRowMustBeCalculate = isRowMustBeCalculate && IsRowMustBeCalculate(rowReturnLegAmount);
                        //
                        if (isRowMustBeCalculate)
                        {
                            try
                            {
                                rowReturnLegAmount.BeginEdit();
                                Parameters.Add(m_CS, m_tradeLibrary, rowReturnLegAmount);
                                CommonValFunc.SetRowCalculated(rowReturnLegAmount);
                                DataRow _rowRLA = rowReturnLegAmount.GetParentRow(DsEvents.ChildEvent);

                                EFS_ReturnLegEvent returnLegEvent = new EFS_ReturnLegEvent(pCommonValDate, this, m_ReturnSwapContainer.MainReturnLeg, rowReturnLegAmount);

                            }
                            catch (SpheresException2 ex)
                            {
                                // EG 20150305 New
                                if (ProcessStateTools.IsStatusErrorWarning(ex.ProcessState.Status))
                                {
                                    lstSpheresException.Add(ex);
                                    ret = (ProcessStateTools.IsStatusError(ex.ProcessState.Status) ? Cst.ErrLevel.FAILURE : Cst.ErrLevel.FAILUREWARNING);
                                }
                            }
                            catch (Exception ex)
                            {
                                // EG 20150305 New
                                ret = Cst.ErrLevel.FAILURE;
                                lstSpheresException.Add(new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex));
                            }
                            finally
                            {
                                rowReturnLegAmount.EndEdit();
                                Update(dbTransaction, idE, isError);
                            }
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
                    if ((null != dbTransaction) && (null == SlaveDbTransaction) && isException)
                    {
                        try { DataHelper.RollbackTran(dbTransaction); }
                        catch { }
                    }
                }
            }

            if (0 < lstSpheresException.Count)
            {
                lstSpheresException.ForEach(ex => Logger.Log(new LoggerData(ex)));
            }
            return ret;
        }
        private Cst.ErrLevel ValorizeInterestLeg(DateTime pCommonValDate)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            List<SpheresException2> lstSpheresException = new List<SpheresException2>();

            bool isError = false;
            bool isRowMustBeCalculate = false;

            DataRow[] rowsInterestLegAmount = GetRowInterest();
            if (ArrFunc.IsFilled(rowsInterestLegAmount))
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 601), 3,
                    new LogParam(LogTools.IdentifierAndId(m_EventsValProcess.MQueue.Identifier, m_EventsValProcess.MQueue.id)),
                    new LogParam("INT-TER / INT")));

                Pair<IReturnLeg, IReturnLegMainUnderlyer> _returnLeg = m_ReturnSwapContainer.MainReturnLeg;
                Pair<IInterestLeg, IInterestCalculation> _currentInterestLeg = m_ReturnSwapContainer.MainInterestLeg;

                if (null == _currentInterestLeg.First.Efs_InterestLeg)
                {
                    _currentInterestLeg.First.Efs_InterestLeg = new EFS_InterestLeg(m_EventsValProcess.Cs, TradeLibrary.DataDocument);
                    _currentInterestLeg.First.Efs_InterestLeg.InitMembers(_currentInterestLeg.First, _currentInterestLeg.First.Notional);
                }

                IDbTransaction dbTransaction = null;
                if (null != SlaveDbTransaction)
                    dbTransaction = SlaveDbTransaction;

                bool isException = false;
                try
                {
                    if (null == SlaveDbTransaction)
                        dbTransaction = DataHelper.BeginTran(m_EventsValProcess.Cs);

                    foreach (DataRow rowInterestLegAmount in rowsInterestLegAmount)
                    {
                        isError = false;
                        m_ParamInstrumentNo.Value = Convert.ToInt32(rowInterestLegAmount["INSTRUMENTNO"]);
                        m_ParamStreamNo.Value = Convert.ToInt32(rowInterestLegAmount["STREAMNO"]);
                        int idE = Convert.ToInt32(rowInterestLegAmount["IDE"]);

                        SetRowAssetToInterestLegAmountOrReset(rowInterestLegAmount, _currentInterestLeg.Second);
                        SetRowDetailToInterestLegAmount(idE, _currentInterestLeg);

                        // ScanCompatibility_Event
                        isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_EventsValProcess.ScanCompatibility_Event(idE));
                        // isRowMustBeCalculate
                        isRowMustBeCalculate = isRowMustBeCalculate && IsRowMustBeCalculate(rowInterestLegAmount);
                        //
                        if (isRowMustBeCalculate)
                        {
                            try
                            {
                                rowInterestLegAmount.BeginEdit();
                                Parameters.Add(m_CS, m_tradeLibrary, rowInterestLegAmount);
                                CommonValFunc.SetRowCalculated(rowInterestLegAmount);
                                DataRow _rowInterestLeg = rowInterestLegAmount.GetParentRow(DsEvents.ChildEvent);

                                EFS_InterestLegEvent interestLegEvent = new EFS_InterestLegEvent(pCommonValDate, this, _returnLeg, _currentInterestLeg, _rowInterestLeg, rowInterestLegAmount);
                            }
                            catch (SpheresException2 ex)
                            {
                                // EG 20150305 New
                                if (ProcessStateTools.IsStatusErrorWarning(ex.ProcessState.Status))
                                {
                                    lstSpheresException.Add(ex);
                                    ret = (ProcessStateTools.IsStatusError(ex.ProcessState.Status) ? Cst.ErrLevel.FAILURE : Cst.ErrLevel.FAILUREWARNING);
                                }
                            }
                            catch (Exception ex)
                            {
                                // EG 20150305 New
                                ret = Cst.ErrLevel.FAILURE;
                                lstSpheresException.Add(new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex));
                            }
                            finally
                            {
                                rowInterestLegAmount.EndEdit();
                                Update(dbTransaction, idE, isError);
                            }
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
                    if ((null != dbTransaction) && (null == SlaveDbTransaction) && isException)
                    {
                        try { DataHelper.RollbackTran(dbTransaction); }
                        catch { }
                    }
                }
            }

            if (0 < lstSpheresException.Count)
            {
                lstSpheresException.ForEach(ex => Logger.Log(new LoggerData(ex)));
            }

            return ret;
        }




        #region InterestLegInfo
        public class InterestLegInfo
        {
            #region Members
            protected int m_IdE;
            protected EventsValProcessBase m_EventsValProcess;

            protected DateTime m_CommonValDate;
            public DateTime startDate;
            public DateTime endDate;
            public DateTime startDateUnAdj;
            protected DateTime endDateUnAdj;
            protected DateTime m_RateCutOffDate;
            protected string m_PaymentCurrency;
            protected int m_IdAsset;
            protected decimal fixedRate;

            protected bool m_FinalRateRoundingSpecified;
            protected IRounding m_FinalRateRounding;
            public Nullable<decimal> multiplier;
            public Nullable<decimal> spread;
            public Nullable<Decimal> percentageInPoint;
            public string dayCountFraction;
            public IInterval intervalFrequency;

            protected LegNotionalInfo m_LegNotionalInfo;

            #endregion
            #region Accessors
            #region EventsValProcess
            public EventsValProcessBase EventsValProcess
            {
                get { return m_EventsValProcess; }
            }
            #endregion
            #region public Currency
            public string PaymentCurrency
            {
                get { return m_PaymentCurrency; }
            }
            #endregion
            #region public IdE
            public int IdE
            {
                get { return m_IdE; }
            }
            #endregion
            #endregion Accessors
            #region Constructors
            public InterestLegInfo(DateTime pDtBusiness, EventsValProcessBase pEventsValProcess, 
                Pair<IReturnLeg, IReturnLegMainUnderlyer> pReturnLeg, Pair<IInterestLeg, IInterestCalculation> pCurrentInterestLeg, DataRow pRowInterestLegAmount)
            {
                SetInfoBase(pDtBusiness, pEventsValProcess, pReturnLeg, pCurrentInterestLeg, pRowInterestLegAmount);
            }
            #endregion Constructors
            #region Methods
            #region SetInfoBase
            /// <summary>
            /// Initialisation des données pour calcul du FDA
            ///  1. Sans Reset du notionnel (BFL)
            ///     FDA (en QCU) = "BCU Amount(1)" * "Taux lu" * "Nbre de jours(2) " " * "DCF"
            ///  2. Avec Reset du notionnel
            ///     FDA (en BCU) = "QCU Amount(3)" * "Taux lu" * "Nbre de jours(2) " " * "DCF"
            /// 
            /// </summary>
            /// <param name="pDtBusiness"></param>
            /// <param name="pEventsValProcess"></param>
            /// <param name="pRowFDA"></param>
            /// <param name="pReturnLeg"></param>
            /// <param name="pCurrentInterestLeg"></param>
            // FI 20141215 [20570] Modify
            // EG 20150311 [POC - BERKELEY] Notionel de base pour calcul du FDA (FOREX)
            // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
            private void SetInfoBase(DateTime pDtBusiness, EventsValProcessBase pEventsValProcess, Pair<IReturnLeg, IReturnLegMainUnderlyer> pReturnLeg, 
                Pair<IInterestLeg, IInterestCalculation> pCurrentInterestLeg, DataRow pRowInterestLegAmount)
            {
                m_EventsValProcess = pEventsValProcess;

                m_CommonValDate = pDtBusiness;//PL 20150126 Affectation de m_CommonValDate pour en disposer plus tard dans la méthode UpdatingRow().
                m_IdE = Convert.ToInt32(pRowInterestLegAmount["IDE"]);
                startDate = Convert.ToDateTime(pRowInterestLegAmount["DTSTARTADJ"]);
                startDateUnAdj = Convert.ToDateTime(pRowInterestLegAmount["DTSTARTUNADJ"]);
                endDate = Convert.ToDateTime(pRowInterestLegAmount["DTENDADJ"]);
                endDateUnAdj = Convert.ToDateTime(pRowInterestLegAmount["DTENDUNADJ"]);

                m_PaymentCurrency = pCurrentInterestLeg.First.InterestAmount.MainLegAmountCurrency;

                if (pCurrentInterestLeg.Second.FloatingRateSpecified)
                {
                    DataRow[] rowAssets = m_EventsValProcess.GetRowAsset(Convert.ToInt32(pRowInterestLegAmount["IDE"]));
                    if ((null != rowAssets) && (0 < rowAssets.Length))
                    {
                        DataRow rowAsset = rowAssets[0];
                        #region FloatingRate
                        if ((null != rowAsset) && (false == Convert.IsDBNull(rowAsset["IDASSET"])))
                        {
                            m_IdAsset = Convert.ToInt32(rowAsset["IDASSET"]);

                            // EG 20150309 POC - BERKELEY Lecture du PIP
                            if (pCurrentInterestLeg.Second.SqlAssetSpecified &&
                                (pCurrentInterestLeg.Second.SqlAsset.AssetCategory == Cst.UnderlyingAsset.RateIndex))
                            {
                                SQL_AssetRateIndex sql_AssetRateIndex = pCurrentInterestLeg.Second.SqlAsset as SQL_AssetRateIndex;
                                if (sql_AssetRateIndex.Idx_IndexUnit == Cst.IdxUnit_currency.ToString())
                                {
                                    if (pReturnLeg.Second.SqlAssetSpecified &&
                                        (pReturnLeg.Second.UnderlyerAsset == Cst.UnderlyingAsset.FxRateAsset))
                                    {
                                        SQL_AssetFxRate sql_AssetFxRate = pReturnLeg.Second.SqlAsset as SQL_AssetFxRate;
                                        percentageInPoint = sql_AssetFxRate.PercentageInPoint;
                                    }
                                }
                            }
                        }
                        #endregion FloatingRate
                    }
                }
                else if (pCurrentInterestLeg.Second.FixedRateSpecified)
                {
                    fixedRate = pCurrentInterestLeg.Second.FixedRate.DecValue;
                }

                Pair<string, string> payerReceiverReference = new Pair<string,
                    string>(pCurrentInterestLeg.First.PayerPartyReference.HRef,
                    pCurrentInterestLeg.First.ReceiverPartyReference.HRef);

                m_LegNotionalInfo = new LegNotionalInfo(m_EventsValProcess, pReturnLeg, pRowInterestLegAmount, payerReceiverReference);


                DataRow rowDetail = m_EventsValProcess.GetRowDetail(m_IdE);
                dayCountFraction = rowDetail["DCF"].ToString();
                multiplier = Convert.IsDBNull(rowDetail["MULTIPLIER"]) ? 1 : Convert.ToDecimal(rowDetail["MULTIPLIER"]);
                if (false == Convert.IsDBNull(rowDetail["SPREAD"]))
                    spread = Convert.ToDecimal(rowDetail["SPREAD"]);
                if (false == Convert.IsDBNull(rowDetail["PIP"]))
                    percentageInPoint = Convert.ToDecimal(rowDetail["PIP"]);

                SetParameter();
            }
            #endregion SetInfoBase
            #region SetParameter
            public void SetParameter()
            {
                CommonValParameterRTS parameter = (CommonValParameterRTS)m_EventsValProcess.Parameters[m_EventsValProcess.ParamInstrumentNo, m_EventsValProcess.ParamStreamNo];
                #region FloatingRate
                if ((0 != m_IdAsset) && (null == parameter.Rate))
                {
                    parameter.Rate = new SQL_AssetRateIndex(parameter.CS, SQL_AssetRateIndex.IDType.IDASSET, m_IdAsset)
                    {
                        WithInfoSelfCompounding = Cst.IndexSelfCompounding.CASHFLOW
                    };
                }
                #endregion FloatingRate
                #region Calculation Period Frequency
                intervalFrequency = parameter.CalculationPeriodFrequency;
                #endregion Calculation Period Frequency
                #region FinalRateRounding
                m_FinalRateRounding = parameter.FinalRateRounding;
                #endregion FinalRateRounding
            }
            #endregion SetParameter
            #endregion Methods
        }
        #endregion InterestLegInfo
        #region EFS_InterestLegEvent
        public class EFS_InterestLegEvent : InterestLegInfo
        {
            #region Members
            protected EFS_InterestLegResetEvent[] m_ResetEvents;
            public Decimal averagedRate;
            public Decimal capFlooredRate;
            public Nullable<Decimal> calculatedRate;
            public Nullable<Decimal> calculatedAmount;
            public Nullable<Decimal> roundedCalculatedAmount;
            public Nullable<Decimal> compoundCalculatedAmount;
            #endregion Members
            #region Accessors
            public EFS_InterestLegResetEvent[] ResetEvents
            {
                get { return m_ResetEvents; }
            }
            #endregion Accessors

            #region Constructors
            /// <summary>
            /// Calcul des intérets (Periods / Reset)
            /// </summary>
            /// <param name="pDtBusiness"></param>
            /// <param name="pEventsValProcess"></param>
            /// <param name="pReturnLeg"></param>
            /// <param name="pCurrentInterestLeg"></param>
            /// <param name="pRowInterestLeg"></param>
            /// <param name="pRowInterestLegAmount"></param>
            /// <param name="pIdE_PreviousInterestLegAmount"></param>
            public EFS_InterestLegEvent(DateTime pDtBusiness, EventsValProcessBase pEventsValProcess,
                Pair<IReturnLeg, IReturnLegMainUnderlyer> pReturnLeg, Pair<IInterestLeg, IInterestCalculation> pCurrentInterestLeg, DataRow pRowInterestLeg, DataRow pRowInterestLegAmount)
                : base(pDtBusiness, pEventsValProcess, pReturnLeg, pCurrentInterestLeg, pRowInterestLegAmount)
            {
                //bool isRowUpdating = true;
                try
                {
                    EFS_CalculAmount calculAmount;
                    DataRow[] rowResets = pRowInterestLegAmount.GetChildRows(m_EventsValProcess.DsEvents.ChildEvent);
                    if ((0 != rowResets.Length) && (EventTypeFunc.IsFloatingRate(pRowInterestLeg["EVENTTYPE"].ToString())))
                    {
                        #region FloatingRate
                        EFS_InterestLegResetEvent resetEvent;
                        ArrayList aResetEvent = new ArrayList();
                        #region Reset Process
                        foreach (DataRow rowReset in rowResets)
                        {
                            if (m_EventsValProcess.IsRowMustBeCalculate(rowReset))
                            {
                                #region CapFloorPeriods Excluded
                                if (EventTypeFunc.IsCapFloorLeg(rowReset["EVENTTYPE"].ToString()))
                                {
                                    rowReset["IDSTCALCUL"] = StatusCalculFunc.CalculatedAndRevisable;
                                    continue;
                                }
                                #endregion CapFloorPeriods Excluded

                                m_EventsValProcess.SetRowAssetToInterestLegAmountOrReset(rowReset, pCurrentInterestLeg.Second);

                                CommonValFunc.SetRowCalculated(rowReset);
                                resetEvent = new EFS_InterestLegResetEvent(m_CommonValDate, m_EventsValProcess, rowReset, m_IdAsset);
                                aResetEvent.Add(resetEvent);
                                if (false == CommonValFunc.IsRowEventCalculated(rowReset))
                                    break;
                            }
                        }
                        m_ResetEvents = (EFS_InterestLegResetEvent[])aResetEvent.ToArray(typeof(EFS_InterestLegResetEvent));
                        #endregion Reset Process
                        //
                        #region Final CalculationPeriod Process
                        if (m_EventsValProcess.IsRowsEventCalculated(rowResets))
                        {
                            // FinalRateRounding / AmountCalculation / Compounding
                            calculatedRate = ((EFS_InterestLegResetEvent)m_ResetEvents.GetValue(0)).observedRate;
                            FinalRateRounding();

                            calculAmount = new EFS_CalculAmount(m_LegNotionalInfo.NotionalAmountPeriod, multiplier, calculatedRate, spread, startDate, endDate, dayCountFraction,
                                intervalFrequency, percentageInPoint);
                            calculatedAmount = calculAmount.calculatedAmount;
                        }
                        #endregion Final CalculationPeriod Process
                        #endregion FloatingRate
                    }
                    else if (EventTypeFunc.IsFixedRate(pRowInterestLeg["EVENTTYPE"].ToString()))
                    {
                        #region FixedRate
                        calculatedRate = fixedRate;
                        calculAmount = new EFS_CalculAmount(m_LegNotionalInfo.NotionalAmountPeriod, calculatedRate, startDate, endDate, dayCountFraction, intervalFrequency);
                        calculatedAmount = calculAmount.calculatedAmount;
                        #endregion FixedRate
                    }
                }
                catch (Exception)
                {
                    CommonValProcessBase.ResetRowCalculated(pRowInterestLegAmount);
                    m_EventsValProcess.SetRowStatus(pRowInterestLegAmount, Tuning.TuningOutputTypeEnum.OEE);
                    throw;
                }
                finally
                {
                    UpdatingRow(pRowInterestLegAmount);
                }
            }
            #endregion Constructors
            //
            #region Methods

            #region FinalRateRounding
            private void FinalRateRounding()
            {
                if ((null != m_FinalRateRounding) && calculatedRate.HasValue)
                {
                    EFS_Round round = new EFS_Round(m_FinalRateRounding, calculatedRate.Value);
                    calculatedRate = round.AmountRounded;
                }
            }
            #endregion FinalRateRounding
            #region UpdatingRow
            private void UpdatingRow(DataRow pRow)
            {
                // EG 20150120 Arrondi du FDA 
                if (calculatedAmount.HasValue)
                    roundedCalculatedAmount = m_EventsValProcess.RoundingCurrencyAmount(m_PaymentCurrency, calculatedAmount.Value);
                //compoundCalculatedAmount = calculatedAmount;
                pRow["UNIT"] = m_PaymentCurrency;
                pRow["UNITTYPE"] = UnitTypeEnum.Currency.ToString();
                pRow["VALORISATION"] = (calculatedAmount.HasValue ? Math.Abs(calculatedAmount.Value) : Convert.DBNull);
                pRow["UNITSYS"] = m_PaymentCurrency;
                pRow["UNITTYPESYS"] = UnitTypeEnum.Currency.ToString();
                pRow["VALORISATIONSYS"] = (calculatedAmount.HasValue ? Math.Abs(calculatedAmount.Value) : Convert.DBNull);
                pRow["IDA_PAY"] = Convert.DBNull;
                pRow["IDB_PAY"] = Convert.DBNull;
                pRow["IDA_REC"] = Convert.DBNull;
                pRow["IDB_REC"] = Convert.DBNull;
                if (calculatedAmount.HasValue)
                {
                    if (0 < calculatedAmount)
                        CommonValFunc.SetPayerReceiver(pRow, 
                            m_LegNotionalInfo.Payer.First, m_LegNotionalInfo.Payer.Second, 
                            m_LegNotionalInfo.Receiver.First, m_LegNotionalInfo.Receiver.Second);
                    else
                        CommonValFunc.SetPayerReceiver(pRow, 
                            m_LegNotionalInfo.Receiver.First, m_LegNotionalInfo.Receiver.Second, 
                            m_LegNotionalInfo.Payer.First, m_LegNotionalInfo.Payer.Second);
                }
                CommonValFunc.SetRowCalculated(pRow);
                m_EventsValProcess.SetRowStatus(pRow, Tuning.TuningOutputTypeEnum.OES);

                DataRow rowDetail = m_EventsValProcess.GetRowDetail(m_IdE);
                if (null == rowDetail)
                {
                    rowDetail = m_EventsValProcess.DsEvents.DtEventDet.NewRow();
                    rowDetail["IDE"] = pRow["IDE"];
                    rowDetail.SetParentRow(pRow);
                    m_EventsValProcess.DsEvents.DtEventDet.Rows.Add(rowDetail);
                }
                if (DtFunc.IsDateTimeFilled(m_CommonValDate))
                {
                    EFS_DayCountFraction dcf = new EFS_DayCountFraction(startDate, endDate, dayCountFraction, intervalFrequency);
                    rowDetail["DCFNUM"] = dcf.Numerator;
                    rowDetail["DCFDEN"] = dcf.Denominator;
                    rowDetail["TOTALOFYEAR"] = dcf.NumberOfCalendarYears;
                    rowDetail["TOTALOFDAY"] = dcf.TotalNumberOfCalendarDays;
                }
                rowDetail["INTEREST"] = (calculatedAmount.HasValue ? Math.Abs(calculatedAmount.Value) : Convert.DBNull);
                rowDetail["DAILYQUANTITY"] = m_LegNotionalInfo.OpenUnits;
                rowDetail["RATE"] = (calculatedRate.HasValue) ? calculatedRate : Convert.DBNull;
                rowDetail["MULTIPLIER"] = (multiplier.HasValue && (1 != multiplier)) ? multiplier.Value : Convert.DBNull;
                rowDetail["SPREAD"] = (spread.HasValue && (0 != spread)) ? spread.Value : Convert.DBNull;
                rowDetail["PIP"] = percentageInPoint ?? Convert.DBNull;

                rowDetail["NOTIONALAMOUNT"] = m_LegNotionalInfo.NotionalAmountPeriod;
            }
            #endregion UpdatingRow
            #endregion Methods
        }
        #endregion EFS_InterestLegEvent

        #region InterestLegResetInfo
        public class InterestLegResetInfo
        {
            #region Members
            protected EventsValProcessBase m_EventsValProcess;
            protected int m_IdE;
            protected DateTime m_CommonValDate;
            protected DateTime m_StartDate;
            protected DateTime m_EndDate;
            public DateTime resetDate;
            protected DateTime m_FixingDate;
            protected DateTime m_RateCutOffDate;
            protected DateTime m_ObservedRateDate;
            protected DateTime m_EndPeriodDate;
            protected int m_IdAsset;
            protected bool m_RateTreatmentSpecified;
            protected RateTreatmentEnum m_RateTreatment;
            protected IInterval m_PaymentFrequency;
            protected int m_RoundingPrecision;

            protected IRounding m_RateRounding;

            protected DataRow[] m_RowAssets;
            #endregion Members
            #region Accessors
            #region IdE
            public int IdE
            {
                get { return m_IdE; }
            }
            #endregion
            #endregion Accessors
            #region Constructors
            public InterestLegResetInfo(DateTime pDtBusiness, EventsValProcessBase pEventsValProcess, DataRow pRowReset)
            {
                SetInfoBase(pDtBusiness, pEventsValProcess, pRowReset);
                SetParameter();
            }
            public InterestLegResetInfo(DateTime pDtBusiness, EventsValProcessBase pEventsValProcess, DataRow pRowReset, int pIdAsset)
            {
                SetInfoBase(pDtBusiness, pEventsValProcess, pRowReset);
                m_IdAsset = pIdAsset;
                m_ObservedRateDate = m_FixingDate;
                DataRow rowCalcPeriod = pRowReset.GetParentRow(m_EventsValProcess.DsEvents.ChildEvent);
                m_EndPeriodDate = Convert.ToDateTime(rowCalcPeriod["DTENDADJ"]);
                SetParameter();
            }
            #endregion Constructors
            #region Methods
            #region SetInfoBase
            private void SetInfoBase(DateTime pDtBusiness, EventsValProcessBase pEventsValProcess, DataRow pRowReset)
            {
                m_EventsValProcess = pEventsValProcess;
                m_CommonValDate = pDtBusiness;
                m_IdE = Convert.ToInt32(pRowReset["IDE"]);
                m_StartDate = Convert.ToDateTime(pRowReset["DTSTARTADJ"]);
                m_EndDate = Convert.ToDateTime(pRowReset["DTENDADJ"]);
                m_RowAssets = m_EventsValProcess.GetRowAsset(Convert.ToInt32(pRowReset["IDE"]));
                DataRow[] rowEventClass = pRowReset.GetChildRows(m_EventsValProcess.DsEvents.ChildEventClass);
                foreach (DataRow dr in rowEventClass)
                {
                    string eventClass = dr["EVENTCLASS"].ToString();
                    if (EventClassFunc.IsGroupLevel(eventClass))
                        resetDate = Convert.ToDateTime(dr["DTEVENT"]);
                    else if (EventClassFunc.IsFixing(eventClass))
                        m_FixingDate = Convert.ToDateTime(dr["DTEVENT"]);
                }
            }
            #endregion SetInfoBase
            #region SetParameter
            public void SetParameter()
            {
                Cst.ErrLevel ret;
                CommonValParameterRTS parameter = (CommonValParameterRTS)m_EventsValProcess.Parameters[m_EventsValProcess.ParamInstrumentNo, m_EventsValProcess.ParamStreamNo];
                m_PaymentFrequency = parameter.CalculationPeriodFrequency;

                if (0 != m_IdAsset)
                {
                    int precisionRate = Convert.ToInt32(parameter.Rate.Idx_RoundPrec);
                    m_RoundingPrecision = Math.Max(precisionRate, 3);
                }
                else
                {
                    RoundingDirectionEnum direction = (RoundingDirectionEnum)StringToEnum.Parse(parameter.Rate.Idx_RoundDir, RoundingDirectionEnum.Nearest);
                    m_RateRounding = m_PaymentFrequency.GetRounding(direction, parameter.Rate.Idx_RoundPrec);
                }

                #region RateTreatment
                ret = parameter.RateTreatment(out m_RateTreatment);
                m_RateTreatmentSpecified = (Cst.ErrLevel.SUCCESS == ret);
                #endregion RateTreatment
            }
            #endregion SetParameter
            #endregion
        }
        #endregion InterestLegResetInfo
        #region EFS_InterestLegResetEvent
        public class EFS_InterestLegResetEvent : InterestLegResetInfo
        {
            #region Members
            protected EFS_SelfAveragingEvent[] m_SelfAveragingEvents;
            //
            /// <summary>
            /// <para>Taux lu ou estimé s'il n'existe pas des selfAverage</para>
            /// <para>Taux Compound s'il existe des selfAverage</para>
            /// </summary>
            public Nullable<decimal> observedRate;
            /// <summary>
            /// Taux obtenu après traitement 
            /// </summary>
            public Nullable<decimal> treatedRate;
            #endregion Members


            #region Constructors
            /// <summary>
            /// Constructor où observedRate et  treatedRate sont obtenus par lecture des table  EVENT (treatedRate) et EVENTDET (observedRate)
            /// </summary>
            /// <param name="pAccrualDate"></param>
            /// <param name="pCommonValProcess"></param>
            /// <param name="pRowReset"></param>
            public EFS_InterestLegResetEvent(DateTime pDtBusiness, EventsValProcessBase pEventsValProcess, DataRow pRowReset)
                : base(pDtBusiness, pEventsValProcess, pRowReset)
            {
                DataRow rowDetail = pEventsValProcess.GetRowDetail(Convert.ToInt32(pRowReset["IDE"]));
                if ((null != rowDetail) && (false == Convert.IsDBNull(rowDetail["RATE"])))
                    observedRate = Convert.ToDecimal(rowDetail["RATE"]);
                if (false == Convert.IsDBNull(pRowReset["VALORISATION"]))
                    treatedRate = Convert.ToDecimal(pRowReset["VALORISATION"]);
            }
            /// <summary>
            /// Constructor où
            /// </summary>
            /// <param name="pAccrualDate"></param>
            /// <param name="pCommonValProcess"></param>
            /// <param name="pRowReset"></param>
            /// <param name="pIdAsset"></param>
            /// <param name="pIdAsset2"></param>
            /// <param name="pRateCutOffDate"></param>
            // EG 20150306 [POC-BERKELEY] : Refactoring Gestion des erreurs
            // EG 20150311 [POC-BERKELEY] : Lecture d'un prix Bid/ASk si FDA sur CFD FOREX
            // EG 20180502 Analyse du code Correction [CA2200]
            // EG 20190114 Add detail to ProcessLog Refactoring
            // EG 20190716 [VCL : FixedIncome] Upd GetQuoteLock
            public EFS_InterestLegResetEvent(DateTime pDtBusiness, EventsValProcessBase pEventsValProcess, DataRow pRowReset, int pIdAsset)
                : base(pDtBusiness, pEventsValProcess, pRowReset, pIdAsset)
            {
                SystemMSGInfo quoteMsgInfo = null;
                try
                {
                    #region Process
                    DataRow[] rowSelfAverages = pRowReset.GetChildRows(m_EventsValProcess.DsEvents.ChildEvent);
                    if (ArrFunc.IsEmpty(rowSelfAverages))
                    {
                        #region Observated Rate
                        // Lecture d'un prix Bid/ASk si FDA sur CFD FOREX
                        Quote _quote = pEventsValProcess.Process.ProcessCacheContainer.GetQuoteLock(m_IdAsset, m_ObservedRateDate, string.Empty,
                            pEventsValProcess.FundingRateQuotationSide, Cst.UnderlyingAsset.RateIndex, new KeyQuoteAdditional(), ref quoteMsgInfo);


                        if ((null != quoteMsgInfo) && (quoteMsgInfo.processState.CodeReturn != Cst.ErrLevel.SUCCESS))
                        {
                            UpdatingRow(pRowReset);
                            quoteMsgInfo.processState = new ProcessState(ProcessStateTools.StatusWarningEnum, quoteMsgInfo.processState.CodeReturn);
                            throw new SpheresException2(quoteMsgInfo.processState);
                        }

                        m_RowAssets[0]["QUOTESIDE"] = _quote.QuoteSide;
                        m_RowAssets[0]["IDMARKETENV"] = _quote.idMarketEnv;
                        m_RowAssets[0]["IDVALSCENARIO"] = _quote.idValScenario;
                        if (_quote.valueSpecified)
                            observedRate = _quote.value;
                        RateTreatement();
                        UpdatingRow(pRowReset);
                        #endregion Observated Rate
                    }
                    else
                    {
                        #region SelfCompounding
                        // TBD
                        #endregion SelfCompounding
                    }
                    CommonValFunc.SetRowCalculated(pRowReset);
                    m_EventsValProcess.SetRowStatus(pRowReset, Tuning.TuningOutputTypeEnum.OES);
                    #endregion Process
                }

                catch (SpheresException2 ex)
                {
                    bool isThrow = true;
                    if (false == ProcessStateTools.IsCodeReturnUndefined(ex.ProcessState.CodeReturn))
                    {
                        if (isThrow && (null != quoteMsgInfo))
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_EventsValProcess.Process.ProcessState.SetErrorWarning(quoteMsgInfo.processState.Status);

                            Logger.Log(quoteMsgInfo.ToLoggerData(0));

                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05160", quoteMsgInfo.processState,
                                LogTools.IdentifierAndId(m_EventsValProcess.EventsValMQueue.GetStringValueIdInfoByKey("identifier"), m_EventsValProcess.EventsValMQueue.id));
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    CommonValProcessBase.ResetRowCalculated(pRowReset);
                    m_EventsValProcess.SetRowStatus(pRowReset, Tuning.TuningOutputTypeEnum.OEE);
                    throw;
                }
            }

            #endregion Constructors
            #region Methods
            #region RateRounding
            #endregion RateRounding
            #region RateTreatement
            private void RateTreatement()
            {
                treatedRate = observedRate;
                if (m_RateTreatmentSpecified && observedRate.HasValue)
                    treatedRate = CommonValFunc.TreatedRate(m_EventsValProcess.ProductBase, m_RateTreatment, observedRate.Value, m_StartDate, m_EndDate, m_PaymentFrequency);
            }
            #endregion RateTreatement
            #region UpdatingRow
            private void UpdatingRow(DataRow pRow)
            {
                pRow["VALORISATION"] = treatedRate ?? Convert.DBNull;
                pRow["UNITTYPE"] = UnitTypeEnum.Rate.ToString();
                pRow["VALORISATIONSYS"] = (treatedRate ?? Convert.DBNull);
                pRow["UNITTYPESYS"] = UnitTypeEnum.Rate.ToString();
                DataRow rowDetail = m_EventsValProcess.GetRowDetail(Convert.ToInt32(pRow["IDE"]));
                if (null != rowDetail)
                    rowDetail["RATE"] = (observedRate ?? Convert.DBNull);
            }

            #endregion UpdatingRow
            #endregion Methods
        }
        #endregion EFS_InterestLegResetEvent

        public class LegNotionalInfo
        {
            protected EventsValProcessBase m_EventsValProcess;

            protected decimal m_NotionalAmount;
            protected decimal m_ResetNotionalAmount;
            protected decimal m_ResetNotionalReferenceAmount;
            protected decimal m_NotionalReferenceAmount;
            protected DataRow RowNominalStepPeriod { get; set; }
            public Pair<Nullable<int>, Nullable<int>> Payer { get; set; }
            public Pair<Nullable<int>, Nullable<int>> Receiver { get; set; }

            public bool IsSingleUnderlyer { get; set; }
            public bool IsNotionalReset { get; set; }
            public string Currency { get; set; }
            public string PaymentCurrency { get; set; }
            public decimal InitialPrice { get; set; }
            public DateTime FixingDate { get; set; }
            public decimal OpenUnits { get; set; }
            public Nullable<decimal> PreviousObservedPrice { get; set; }

            public LegNotionalInfo(EventsValProcessBase pEventsValProcess, Pair<IReturnLeg, IReturnLegMainUnderlyer> pMainReturnLeg, DataRow pRowLegAmount, Pair<string, string> pPayerReceiverReference)
            {
                m_EventsValProcess = pEventsValProcess;
                DataDocumentContainer dataDocument = pEventsValProcess.TradeLibrary.DataDocument;
                Payer = new Pair<Nullable<int>, Nullable<int>>(dataDocument.GetOTCmlId_Party(pPayerReceiverReference.First), dataDocument.GetOTCmlId_Book(pPayerReceiverReference.First));
                Receiver = new Pair<Nullable<int>, Nullable<int>>(dataDocument.GetOTCmlId_Party(pPayerReceiverReference.Second), dataDocument.GetOTCmlId_Book(pPayerReceiverReference.Second));

                IsSingleUnderlyer = pMainReturnLeg.First.Underlyer.UnderlyerSingleSpecified;


                IReturnLegValuation rateOfReturn = pMainReturnLeg.First.RateOfReturn;
                IsNotionalReset = rateOfReturn.NotionalResetSpecified && rateOfReturn.NotionalReset.BoolValue;

                if (pMainReturnLeg.Second.SqlAssetSpecified)
                    Currency = pMainReturnLeg.Second.SqlAsset.IdC;

                m_NotionalAmount = pMainReturnLeg.First.Efs_ReturnLeg.NotionalAmount.DecValue;
                m_NotionalReferenceAmount = m_NotionalAmount;

                PaymentCurrency = pMainReturnLeg.First.ReturnSwapAmount.MainLegAmountCurrency;

                DataRow rowFixing = pRowLegAmount.GetChildRows(m_EventsValProcess.DsEvents.ChildEventClass)
                    .Where(row => EventClassFunc.IsFixing(row["EVENTCLASS"].ToString()))
                    .FirstOrDefault();
                if (null != rowFixing)
                    FixingDate = Convert.ToDateTime(rowFixing["DTEVENT"]);

                m_ResetNotionalAmount = m_NotionalAmount;
                m_ResetNotionalReferenceAmount = m_ResetNotionalAmount;

                // Prix initial de l'actif du ReturnLeg (utilisé
                IReturnLegValuationPrice _initialPrice = pMainReturnLeg.First.RateOfReturn.InitialPrice;
                if (_initialPrice.NetPriceSpecified)
                {
                    InitialPrice = _initialPrice.NetPrice.Amount.DecValue;
                    PreviousObservedPrice = InitialPrice;
                }

                // Quantity
                if (pMainReturnLeg.Second.OpenUnitsSpecified)
                    OpenUnits = pMainReturnLeg.Second.OpenUnits.DecValue;

                // Recherche du Notional à utiliser pour la période
                DateTime dtStartPeriod = Convert.ToDateTime(pRowLegAmount["DTSTARTUNADJ"]);
                DateTime dtEndPeriod = Convert.ToDateTime(pRowLegAmount["DTENDUNADJ"]);

                // ATTENTION : Le NOS|NOM est sur le STREAM 1
                // Lecture EN DUR (actuellement 2 streams possibles : 1 RETURNLEG (Stream 1) et un INTERESTLEG (Stream 2))
                if (IsNotionalReset)
                {
                    RowNominalStepPeriod = m_EventsValProcess.GetRowNominalStep(1) 
                        .Where(row => (dtStartPeriod == Convert.ToDateTime(row["DTSTARTUNADJ"])) && (dtEndPeriod == Convert.ToDateTime(row["DTENDUNADJ"])))
                        .FirstOrDefault();
                }
                else
                {
                    RowNominalStepPeriod = m_EventsValProcess.GetRowNominalStep(1).FirstOrDefault();
                }
                if (null != RowNominalStepPeriod)
                    m_ResetNotionalAmount = Convert.ToDecimal(RowNominalStepPeriod["VALORISATION"]);

                DataRow rowPreviousReturnLegAmount = m_EventsValProcess.GetRowEventByEventType(EventTypeFunc.ReturnLegAmount)
                    .Where(row => dtStartPeriod == Convert.ToDateTime(row["DTENDUNADJ"])).FirstOrDefault();
                if (null != rowPreviousReturnLegAmount)
                {
                    DataRow rowDetail = m_EventsValProcess.GetRowDetail(Convert.ToInt32(rowPreviousReturnLegAmount["IDE"]));
                    if ((null != rowDetail) && (false == Convert.IsDBNull(rowDetail["QUOTEPRICE"])))
                        PreviousObservedPrice = Convert.ToDecimal(rowDetail["QUOTEPRICE"]);
                }
            }

            /// <summary>
            /// Retourne le notionel de référence 
            /// si Reset = Notional au Prix de cloture
            /// sinon = Notional d'origine
            /// </summary>
            public decimal NotionalAmountReferencePeriod
            {
                get { return IsNotionalReset ? m_ResetNotionalReferenceAmount : m_NotionalReferenceAmount; }
            }

            /// <summary>
            /// Calcul le montant Notionnel pour évaluer le Return Leg amont en tenant compte :
            /// 1. De l'existence d'un Notionel de base (dans ce cas application d'un taux de change)
            /// 2. D'un reset ( dans ce cas lecture du Notionnel calculé sur la période précédente)
            /// </summary>
            public decimal NotionalAmountPeriod
            {
                get
                {
                    return (IsNotionalReset ? m_ResetNotionalAmount : m_NotionalAmount);
                }
            }
        }
        #region ReturnLegInfo
        public class ReturnLegInfo
        {
            #region Members
            protected EventsValProcessBase m_EventsValProcess;
            protected DataDocumentContainer m_DataDocument;
            protected int m_IdE;
            protected DateTime m_CommonValDate;
            protected DateTime m_StartDate;
            protected DateTime m_EndDate;
            protected DateTime m_FixingDate;
            protected DataRow m_RowAsset;
            protected Cst.UnderlyingAsset m_AssetCategory;

            protected int m_IdAsset;

            protected LegNotionalInfo m_LegNotionalInfo;

            public bool m_IsSingleUnderlyer;
            #endregion Members
            #region Accessors
            public Nullable<decimal> PreviousPriceAmount
            {
                get 
                {
                    Nullable<decimal> price = null;
                    if (m_LegNotionalInfo.IsNotionalReset && m_LegNotionalInfo.PreviousObservedPrice.HasValue)
                            price = m_LegNotionalInfo.PreviousObservedPrice.Value;
                    else if (false == m_LegNotionalInfo.IsNotionalReset)
                        price = m_LegNotionalInfo.InitialPrice;
                    return price; 
                }
            }
            #region IdE
            public int IdE
            {
                get { return m_IdE; }
            }
            #endregion
            #endregion Accessors
            #region Constructors
            public ReturnLegInfo(DateTime pDtBusiness, EventsValProcessBase pEventsValProcess, Pair<IReturnLeg, IReturnLegMainUnderlyer> pMainReturnLeg, DataRow pRowReturnLegAmount)
            {
                SetInfoBase(pDtBusiness, pEventsValProcess, pMainReturnLeg, pRowReturnLegAmount);
            }
            #endregion Constructors
            #region SetInfoBase
            private void SetInfoBase(DateTime pDtBusiness, EventsValProcessBase pEventsValProcess, Pair<IReturnLeg, IReturnLegMainUnderlyer> pMainReturnLeg, DataRow pRowReturnLegAmount)
            {
                m_EventsValProcess = pEventsValProcess;

                m_CommonValDate = pDtBusiness;
                m_IdE = Convert.ToInt32(pRowReturnLegAmount["IDE"]);
                m_StartDate = Convert.ToDateTime(pRowReturnLegAmount["DTSTARTADJ"]);
                m_EndDate = Convert.ToDateTime(pRowReturnLegAmount["DTENDADJ"]);
                m_RowAsset = m_EventsValProcess.GetRowAsset(Convert.ToInt32(pRowReturnLegAmount["IDE"])).First();
                if (null != m_RowAsset)
                {
                    m_IdAsset = Convert.ToInt32(m_RowAsset["IDASSET"]);
                    m_AssetCategory = ReflectionTools.ConvertStringToEnum<Cst.UnderlyingAsset>(m_RowAsset["ASSETCATEGORY"].ToString()); 
                }

                m_IsSingleUnderlyer = pMainReturnLeg.First.Underlyer.UnderlyerSingleSpecified;

                // Initialisation des données utile pour évaluer le Notionel à utiliser pour la période

                Pair<string, string> payerReceiverReference = new Pair<string,
                    string>(pMainReturnLeg.First.PayerPartyReference.HRef,
                    pMainReturnLeg.First.ReceiverPartyReference.HRef);

                m_LegNotionalInfo = new LegNotionalInfo(m_EventsValProcess, pMainReturnLeg, pRowReturnLegAmount, payerReceiverReference);
            }
            #endregion SetInfoBase
        }
        #endregion ReturnLegInfo
        #region EFS_ReturnLegEvent
        public class EFS_ReturnLegEvent : ReturnLegInfo
        {
            #region Members
            public Nullable<decimal> observedPrice;
            public Nullable<decimal> returnRate;
            public Nullable<decimal> returnLegAmount;
            #endregion Members
            #region Constructors
            public EFS_ReturnLegEvent(DateTime pDtBusiness, EventsValProcessRTS pEventsValProcess, Pair<IReturnLeg, IReturnLegMainUnderlyer> pMainReturnLeg, DataRow pRowReturnLegAmount)
            :base(pDtBusiness, pEventsValProcess, pMainReturnLeg, pRowReturnLegAmount )
            {
                Quote _quote = null;
                SystemMSGInfo quoteMsgInfo = null;
                try
                {
                    #region Process
                    if (null != m_RowAsset)
                    {
                        // Lecture de la quotation du Return Leg 
                        if (m_IsSingleUnderlyer)
                        {
                            _quote = pEventsValProcess.Process.ProcessCacheContainer.GetQuoteLock(m_IdAsset, m_LegNotionalInfo.FixingDate, string.Empty, 
                                QuotationSideEnum.OfficialClose, m_AssetCategory, new KeyQuoteAdditional(), ref quoteMsgInfo);
                        }
                        else
                        {
                            // Autres cas = Basket
                            // TBD
                        }

                        if ((null != quoteMsgInfo) && (quoteMsgInfo.processState.CodeReturn != Cst.ErrLevel.SUCCESS))
                        {
                            UpdatingRow(pRowReturnLegAmount);
                            quoteMsgInfo.processState = new ProcessState(ProcessStateTools.StatusWarningEnum, quoteMsgInfo.processState.CodeReturn);
                            throw new SpheresException2(quoteMsgInfo.processState);
                        }

                        if (_quote.valueSpecified)
                        {
                            observedPrice = _quote.value;
                            if (PreviousPriceAmount.HasValue)
                            {
                                returnRate = (observedPrice / PreviousPriceAmount) - 1;
                                returnLegAmount = returnRate * m_LegNotionalInfo.NotionalAmountPeriod;
                            }
                        }

                        UpdatingRow(pRowReturnLegAmount);
                    }
                    CommonValFunc.SetRowCalculated(pRowReturnLegAmount);
                    m_EventsValProcess.SetRowStatus(pRowReturnLegAmount, Tuning.TuningOutputTypeEnum.OES);
                    #endregion Process
                }

                catch (SpheresException2 ex)
                {
                    bool isThrow = true;
                    if (false == ProcessStateTools.IsCodeReturnUndefined(ex.ProcessState.CodeReturn))
                    {
                        if (isThrow && (null != quoteMsgInfo))
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_EventsValProcess.Process.ProcessState.SetErrorWarning(quoteMsgInfo.processState.Status);
                            
                            Logger.Log(quoteMsgInfo.ToLoggerData(0));

                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05160", quoteMsgInfo.processState,
                                LogTools.IdentifierAndId(m_EventsValProcess.EventsValMQueue.GetStringValueIdInfoByKey("identifier"), m_EventsValProcess.EventsValMQueue.id));
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    CommonValProcessBase.ResetRowCalculated(pRowReturnLegAmount);
                    m_EventsValProcess.SetRowStatus(pRowReturnLegAmount, Tuning.TuningOutputTypeEnum.OEE);
                    throw;
                }
            }
            #endregion Constructors
            #region Methods
            #region UpdatingRow
            
            private void UpdatingRow(DataRow pRow)
            {
                pRow["VALORISATION"] = (returnLegAmount.HasValue ? Math.Abs(returnLegAmount.Value) : Convert.DBNull);
                pRow["UNIT"] = m_LegNotionalInfo.PaymentCurrency;
                pRow["UNITTYPE"] = UnitTypeEnum.Currency.ToString();
                pRow["VALORISATIONSYS"] = (returnLegAmount.HasValue ? Math.Abs(returnLegAmount.Value) : Convert.DBNull);
                pRow["UNITSYS"] = m_LegNotionalInfo.PaymentCurrency;
                pRow["UNITTYPESYS"] = UnitTypeEnum.Currency.ToString();
                pRow["IDA_PAY"] = Convert.DBNull;
                pRow["IDB_PAY"] = Convert.DBNull;
                pRow["IDA_REC"] = Convert.DBNull;
                pRow["IDB_REC"] = Convert.DBNull;

                if (returnLegAmount.HasValue)
                {
                    if (0 <= returnLegAmount)
                        CommonValFunc.SetPayerReceiver(pRow, 
                            m_LegNotionalInfo.Payer.First, m_LegNotionalInfo.Payer.Second,
                            m_LegNotionalInfo.Receiver.First, m_LegNotionalInfo.Receiver.Second);
                    else
                        CommonValFunc.SetPayerReceiver(pRow, m_LegNotionalInfo.Receiver.First, m_LegNotionalInfo.Receiver.Second,
                            m_LegNotionalInfo.Payer.First, m_LegNotionalInfo.Payer.Second);
                }

                DataRow rowDetail = m_EventsValProcess.GetRowDetail(Convert.ToInt32(pRow["IDE"]));
                if (null == rowDetail)
                {
                    rowDetail = m_EventsValProcess.DsEvents.DtEventDet.NewRow();
                    rowDetail["IDE"] = pRow["IDE"];
                    rowDetail.SetParentRow(pRow);
                    m_EventsValProcess.DsEvents.DtEventDet.Rows.Add(rowDetail);
                }
                rowDetail["RATE"] = returnRate ?? Convert.DBNull;
                rowDetail["PRICE"] = m_LegNotionalInfo.InitialPrice;
                rowDetail["QUOTEPRICE"] = observedPrice ?? Convert.DBNull;
                rowDetail["QUOTEPRICE100"] = observedPrice ?? Convert.DBNull;
                rowDetail["DAILYQUANTITY"] = m_LegNotionalInfo.OpenUnits;
                rowDetail["NOTIONALAMOUNT"] = m_LegNotionalInfo.NotionalAmountPeriod;


                // Cours veille et Notionel (NOS|NOM) resetté sur période suivante (si elle existe)
                if (m_LegNotionalInfo.IsNotionalReset)
                {
                    rowDetail["QUOTEPRICEYEST"] = m_LegNotionalInfo.PreviousObservedPrice ?? Convert.DBNull;
                    rowDetail["QUOTEPRICEYEST100"] = m_LegNotionalInfo.PreviousObservedPrice ?? Convert.DBNull;

                    // Mise à jour du Notionel de la période suivante
                    if (observedPrice.HasValue)
                    {
                        m_EventsValProcess.GetRowNominalStep().Where(row => Convert.ToDateTime(row["DTSTARTUNADJ"]) >= m_EndDate).ToList().ForEach(row =>
                        {
                            row.BeginEdit();
                            row["VALORISATION"] = observedPrice.Value * m_LegNotionalInfo.OpenUnits;
                            row.EndEdit();
                        });
                    }
                }
            }

            #endregion UpdatingRow
            #endregion Methods
        }
        #endregion EFS_ReturnLegEvent

        /// <summary>
        /// Mise à jour du Notionel de la période suivante (si elle existe et si Reset)
        /// </summary>
        public void SetAmountToNotionalAmountNextPeriod()
        {
        }


        #region SetRowAssetToReturnLegAmount
        public override void SetRowAssetToReturnLegAmount(DataRow pRow, IReturnLegMainUnderlyer pReturnLegUnderlyer)
        {
            int idE = Convert.ToInt32(pRow["IDE"]);
            DataRow[] rowAssets = GetRowAsset(idE);
            DataRow rowAsset;
            if (ArrFunc.IsEmpty(rowAssets))
            {
                rowAsset = NewRowEventAsset(GetRowAsset(Convert.ToInt32(pRow["IDE_EVENT"])).FirstOrDefault(), idE);
                m_DsEvents.DtEventAsset.Rows.Add(rowAsset);
            }
            rowAsset = GetRowAsset(idE).FirstOrDefault();

            SQL_AssetBase _asset = pReturnLegUnderlyer.SqlAsset;
            rowAsset.BeginEdit();
            rowAsset["IDASSET"] = _asset.Id;
            rowAsset["IDC"] = _asset.IdC;
            rowAsset["IDM"] = (_asset.IdM > 0) ? _asset.IdM : Convert.DBNull;
            rowAsset["ASSETCATEGORY"] = _asset.AssetCategory;
            rowAsset.EndEdit();
        }
        #endregion SetRowAssetToReturnLegAmount

        #region SetRowAssetToInterestLegAmountOrReset
        public override void SetRowAssetToInterestLegAmountOrReset(DataRow pRow, IInterestCalculation pInterestCalculation)
        {
            if (pInterestCalculation.FloatingRateSpecified)
            {
                int idE = Convert.ToInt32(pRow["IDE"]);
                DataRow[] rowAssets = GetRowAsset(idE);
                DataRow rowAsset;
                if (ArrFunc.IsEmpty(rowAssets))
                {
                    rowAsset = NewRowEventAsset(GetRowAsset(Convert.ToInt32(pRow["IDE_EVENT"])).FirstOrDefault(), idE);
                    m_DsEvents.DtEventAsset.Rows.Add(rowAsset);
                }
                rowAsset = GetRowAsset(idE).FirstOrDefault();

                SQL_AssetBase _asset = pInterestCalculation.SqlAsset;
                rowAsset.BeginEdit();
                rowAsset["IDASSET"] = _asset.Id;
                rowAsset["IDC"] = _asset.IdC;
                rowAsset["IDM"] = (_asset.IdM > 0) ? _asset.IdM : Convert.DBNull;
                rowAsset["ASSETCATEGORY"] = _asset.AssetCategory;
                if (_asset is SQL_AssetRateIndex)
                {
                    SQL_AssetRateIndex _assetRateIndex = _asset as SQL_AssetRateIndex;
                    rowAsset["IDBC"] = _assetRateIndex.Idx_IdBc;
                }
                rowAsset.EndEdit();
            }
        }
        #endregion SetRowAssetToInterestLegAmountOrReset
        #region SetRowDetailToInterestLegAmount
        // EG 20150921 Test paymentPeriods
        public override void SetRowDetailToInterestLegAmount(int pIdE, Pair<IInterestLeg, IInterestCalculation> pCurrentInterestLeg)
        {
            if (ArrFunc.IsFilled(pCurrentInterestLeg.First.Efs_InterestLeg.paymentPeriods))
            {
                DataRow rowDetails = GetRowDetail(pIdE);
                EFS_InterestLegPaymentDate paymentDate = pCurrentInterestLeg.First.Efs_InterestLeg.paymentPeriods.First();
                rowDetails.BeginEdit();
                rowDetails["DCF"] = pCurrentInterestLeg.Second.DayCountFraction.ToString();
                rowDetails["MULTIPLIER"] = paymentDate.multiplierSpecified ? paymentDate.Multiplier.DecValue : Convert.DBNull;
                rowDetails["SPREAD"] = paymentDate.spreadSpecified ? paymentDate.Spread.DecValue : Convert.DBNull;
                if (pCurrentInterestLeg.Second.FixedRateSpecified)
                    rowDetails["RATE"] = pCurrentInterestLeg.Second.FixedRate.DecValue;
                rowDetails.EndEdit();
            }
        }
        #endregion SetRowDetailToInterestLegAmount

        #endregion Methods
    }
    #endregion EventsValProcessRTS
}

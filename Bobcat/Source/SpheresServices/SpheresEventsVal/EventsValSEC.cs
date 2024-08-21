#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresService;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process
{
    #region EventsValProcessSEC
    public class EventsValProcessSEC : EventsValProcessBase
    {
        #region Members
        private readonly CommonValParameters m_Parameters;
        private readonly CommonValParameters m_ParametersIRD;
        #endregion Members
        #region Accessors
        #region Parameters
        public override CommonValParameters ParametersIRD
        {
            get { return m_ParametersIRD; }
        }
        public override CommonValParameters Parameters
        {
            get { return m_Parameters; }
        }
        #endregion Parameters
        #endregion Accessors
        #region Constructors
        public EventsValProcessSEC(EventsValProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pProcess, pDsTrade, pTradeLibrary,pProduct)
        {
            m_Parameters = new CommonValParametersSEC();
            m_ParametersIRD = new CommonValParametersIRD();
        }
        #endregion Constructors
        #region Methods

        #region IsRowHasNominalChanged
        private bool IsRowHasNominalChanged(DataRow pRow)
        {
            DataRow rowNominal = GetCurrentNominal(Convert.ToDateTime(pRow["DTSTARTADJ"]));
            DataRow rowVariationNominal = GetRowVariationNominal(Convert.ToDateTime(rowNominal["DTSTARTADJ"]));
            if (null != rowVariationNominal)
            {
                DataRow[] rowAssets = GetRowAsset(Convert.ToInt32(rowVariationNominal["IDE"]));
                if ((null != rowAssets) && (0 < rowAssets.Length))
                {
                    DataRow rowAsset = rowAssets[0];
                    if (IsQuote_FxRate && (m_Quote.time <= Convert.ToDateTime(rowNominal["DTSTARTADJ"])) && (m_Quote.idAsset == Convert.ToInt32(rowAsset["IDASSET"])))
                        return true;
                }
            }
            return false;
        }
        #endregion IsRowHasNominalChanged
        #region IsRowMustBeCalculate
        public override bool IsRowMustBeCalculate(DataRow pRow)
        {
            string eventCode = pRow["EVENTCODE"].ToString();
            string eventType = pRow["EVENTTYPE"].ToString();

            if (null != m_EventsValMQueue.quote)
            {
                if (IsQuote_RateIndex)
                {
                    if (EventCodeFunc.IsReset(eventCode) || EventCodeFunc.IsSelfReset(eventCode))
                    {
                        if (IsRowHasFixingEvent(pRow))
                            return true;
                        else
                            return IsRowHasChildrens(pRow);
                    }
                    else
                        return IsRowHasChildrens(pRow);

                }
                else if (IsQuote_FxRate)
                {
                    if (EventCodeAndEventTypeFunc.IsNominalPeriodVariation(eventCode, eventType))
                    {
                        if (IsRowHasFixingEvent(pRow))
                            return true;
                        else
                            return IsRowHasNominalChanged(pRow);
                    }
                    else
                    {
                        if (EventTypeFunc.IsInterest(eventType) || EventCodeFunc.IsCalculationPeriod(eventCode))
                            return IsRowHasNominalChanged(pRow);
                        else
                            return true;
                    }
                }
                else if (IsQuote_SecurityAsset)
                {
                    return (((Quote_SecurityAsset)m_Quote).idE_Source == Convert.ToInt32(pRow["IDE_SOURCE"]));
                }
            }
            else
                return true;
            return false;
        }
        #endregion IsRowMustBeCalculate

        #region RoundingDebtSecurityTransactionFullCouponAmount
        public override decimal RoundingDebtSecurityTransactionFullCouponAmount(decimal pAmount)
        {
            decimal amountRounded = pAmount;
			CommonValParameterSEC parameter = (CommonValParameterSEC) Parameters[ParamInstrumentNo,ParamStreamNo];
            IRounding rounding = parameter.FullCouponRounding;
            if (null != rounding)
            {
                EFS_Round round = new EFS_Round(rounding,pAmount);
                amountRounded = round.AmountRounded;
            }
            return amountRounded;
        }
        #endregion RoundingDebtSecurityTransactionFullCouponAmount
        #region RoundingDebtSecurityUnitCouponAmount
        public override decimal RoundingDebtSecurityUnitCouponAmount(decimal pAmount)
        {
            decimal amountRounded = pAmount;
			CommonValParameterSEC parameter = (CommonValParameterSEC) Parameters[ParamInstrumentNo,ParamStreamNo];
            IRounding rounding = parameter.UnitCouponRounding;
            if (null != rounding)
            {
                EFS_Round round = new EFS_Round(rounding,pAmount);
                amountRounded = round.AmountRounded;
            }
            return amountRounded;
        }
        #endregion RoundingDebtSecurityUnitCouponAmount
        #region SetParametersIRD
        private void SetParametersIRD()
        {
            m_ParametersIRD.Parameters = (CommonValParameterIRD[])m_Parameters.Parameters;
        }
        #endregion Valorize

        #region Update
        // EG 20170412 [23081] Gestion  dbTransaction et SlaveDbTransaction
        // EG 20180502 Analyse du code Correction [CA2200]
        protected override void Update(int pIdE, bool pIsError)
        {
            IDbTransaction dbTransaction = null;
            if (null != SlaveDbTransaction)
                dbTransaction = SlaveDbTransaction;

            bool isException = false;
            try
            {
                if (null == SlaveDbTransaction)
                    dbTransaction = DataHelper.BeginTran(m_CS);

                CommonValParameterSEC parameter = (CommonValParameterSEC)Parameters[ParamInstrumentNo, ParamStreamNo];
                if (parameter.Product.ProductBase.IsDebtSecurity)
                    UpdateAssociatedTransactionWithDebtSecurity(dbTransaction, pIdE);
                base.Update(dbTransaction,pIdE,pIsError);
                if (null == SlaveDbTransaction)
                    DataHelper.CommitTran(dbTransaction);
            }
            catch (SpheresException2)
            {
                isException = true;
                throw;
            }
            catch (Exception ex)
            {
                isException = true;
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
            }
            finally
            {
                if (isException && (null != dbTransaction) && (null == SlaveDbTransaction))
                {
                    try
                    {
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    catch { }
                }
            }
        }
        #endregion Update
        #region UpdateAssociatedTransactionWithDebtSecurity
        private Cst.ErrLevel UpdateAssociatedTransactionWithDebtSecurity(IDbTransaction pDbTransaction, int pIdE)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            DataTable dtChanged = DsEvents.DtEvent.GetChanges();
            if (null != dtChanged)
            {
                foreach (DataRow row in dtChanged.Rows)
                {
                    if (pIdE == Convert.ToInt32(row["IDE"]))
                    {
                        #region Le coupon unitaire a-t-il changé ?
                        bool originalUnitCouponSpecified = (false == Convert.IsDBNull(row["VALORISATION",DataRowVersion.Original]));
                        decimal originalUnitCoupon = 0;
                        if (originalUnitCouponSpecified)
                            originalUnitCoupon = Convert.ToDecimal(row["VALORISATION",DataRowVersion.Original]);

                        bool currentUnitCouponSpecified = (false == Convert.IsDBNull(row["VALORISATION",DataRowVersion.Current]));
                        decimal currentUnitCoupon = 0;
                        if (currentUnitCouponSpecified)
                            currentUnitCoupon = Convert.ToDecimal(row["VALORISATION",DataRowVersion.Current]);


                        bool isChanged = (originalUnitCouponSpecified != currentUnitCouponSpecified) ||
                                         (originalUnitCoupon != currentUnitCoupon);

                        #endregion Le coupon unitaire a-t-il changé ?

                        if (isChanged)
                        {
                            Quote currentQuote = (Quote)m_EventsValMQueue.Quote;
                            Quote_SecurityAsset quote = new Quote_SecurityAsset
                            {
                                action = row.RowState.ToString(),
                                idE_Source = pIdE,
                                unitCouponSpecified = (false == Convert.IsDBNull(row["VALORISATION"])),
                                unitCoupon = Convert.ToDecimal(row["VALORISATION"])
                            };

                            if (null != currentQuote)
                            {
                                quote.idQuoteSpecified = currentQuote.idQuoteSpecified;
                                quote.idQuote = currentQuote.idQuote;
                                quote.idMarketEnv = currentQuote.idMarketEnv;
                                quote.idValScenario = currentQuote.idValScenario;
                                quote.idAsset = currentQuote.idAsset;
                                quote.idAsset_Identifier = currentQuote.idAsset_Identifier;
                                quote.time = currentQuote.time;
                                quote.idBC = currentQuote.idBC;
                                quote.quoteSide = currentQuote.quoteSide;
                                quote.cashFlowType = currentQuote.cashFlowType;
                                quote.valueSpecified = currentQuote.valueSpecified;
                                quote.value = currentQuote.value;
                            }
                            DataParameters parameters = new DataParameters();
                            parameters.Add(new DataParameter(m_CS, "IDE_SOURCE", DbType.Int32), pIdE);

                            StrBuilder sqlSelect = new StrBuilder();
                            sqlSelect += SQLCst.SELECT_DISTINCT + @"e.IDT,tr.IDENTIFIER, ip.GPRODUCT";
                            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e " + Cst.CrLf;
                            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE + " tr " + Cst.CrLf;
                            sqlSelect += SQLCst.ON + "(tr.IDT = e.IDT)" + Cst.CrLf;
                            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_INSTR_PRODUCT + " ip " + Cst.CrLf;
                            sqlSelect += SQLCst.ON + "(ip.IDI = tr.IDI)" + Cst.CrLf;
                            sqlSelect += SQLCst.WHERE + @"e.IDE_SOURCE = @IDE_SOURCE";

                            #region DataSet
                            DataSet dsAssociatedEvent = DataHelper.ExecuteDataset(m_CS, pDbTransaction, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
                            foreach (DataRow associatedRow in dsAssociatedEvent.Tables[0].Rows)
                            {
                                int idT = Convert.ToInt32(associatedRow["IDT"]);
                                MQueueAttributes mQueueAttributes = new MQueueAttributes()
                                {
                                    connectionString = m_CS,
                                    id = idT,
                                    idInfo = new IdInfo()
                                    {
                                        id = idT,
                                        idInfos = new DictionaryEntry[]
                                        {
                                            new DictionaryEntry("ident", "TRADE"),
                                            new DictionaryEntry("identifier", associatedRow["IDENTIFIER"].ToString()),
                                            new DictionaryEntry("GPRODUCT", associatedRow["GPRODUCT"].ToString())
                                        }
                                    },
                                    requester = m_EventsValMQueue.header.requester
                                };
                                
                                EventsValMQueue eventsValMQueue = new EventsValMQueue(mQueueAttributes, quote);

                                try
                                {
                                    MQueueSendInfo sendInfo = ServiceTools.GetMqueueSendInfo(Cst.ProcessTypeEnum.EVENTSVAL, Process.AppInstance);
                                    MQueueTools.Send(eventsValMQueue, sendInfo);
                                    Logger.Log(new LoggerData(LogLevelEnum.Info, "send Events valuation", 0, new LogParam(idT, null, "TRADE", Cst.LoggerParameterLink.IDDATA)));
                                }
                                catch (SpheresException2 ex)
                                {
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                                    Logger.Log(new LoggerData(ex, new LogParam(idT, null, "TRADE", Cst.LoggerParameterLink.IDDATA)));

                                    codeReturn = Cst.ErrLevel.ABORTED;
                                }
                                catch (Exception ex)
                                {
                                    codeReturn = Cst.ErrLevel.ABORTED;
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                    SpheresException2 oex = new SpheresException2("ProcessExecuteSpecific", ex);
                                    Logger.Log(new LoggerData(oex, new LogParam(idT, null, "TRADE", Cst.LoggerParameterLink.IDDATA)));
                                }
                            }
                            #endregion DataSet
                        }
                        break;
                    }
                    
                }
            }
            return codeReturn;
        }
        #endregion UpdateAssociatedTransactionWithDebtSecurity
        // EG 20180502 Analyse du code Correction [CA2214]
        #region Valorize
        /// <revision>
        ///     <build>23</build><date>20050808</date><author>PL</author>
        ///     <comment>
        ///     Add CancelEdit()
        ///     </comment>
        /// </revision>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            // EG 20150617 [20665]
            //InitializeDataSetEvent();

            ArrayList alSpheresException = new ArrayList();
            #region Payment Process
            DataRow[] rowsInterest = GetRowInterest();
            if (ArrFunc.IsFilled(rowsInterest))
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 601), 3,
                    new LogParam(LogTools.IdentifierAndId(m_EventsValProcess.MQueue.Identifier, m_EventsValProcess.MQueue.id)),
                    new LogParam("STA-INT-TER / INT")));

                foreach (DataRow rowInterest in rowsInterest)
                {
                    bool isError = false;
                    m_ParamInstrumentNo.Value = Convert.ToInt32(rowInterest["INSTRUMENTNO"]);
                    m_ParamStreamNo.Value = Convert.ToInt32(rowInterest["STREAMNO"]);
                    int idE = Convert.ToInt32(rowInterest["IDE"]);
                    // ScanCompatibility_Event
                    bool isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_EventsValProcess.ScanCompatibility_Event(idE));
                    // isRowMustBeCalculate
                    isRowMustBeCalculate = isRowMustBeCalculate && IsRowMustBeCalculate(rowInterest);
                    //
                    if (isRowMustBeCalculate)
                    {
                        EFS_PaymentEvent paymentEvent = null;
                        try
                        {
                            Parameters.Add(m_CS, m_tradeLibrary, rowInterest);
                            SetParametersIRD();
                            rowInterest.BeginEdit();
                            paymentEvent = new EFS_PaymentEvent(CommonValDate, this, rowInterest);
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
                            alSpheresException.Add(new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex));
                        }
                        finally
                        {
                            if ((null != paymentEvent) || (isError))
                                rowInterest.EndEdit();
                            Update(idE, isError);
                        }
                    }
                }
            }
            #endregion Payment Process
            //
            if (ArrFunc.IsFilled(alSpheresException))
            {
                ret = Cst.ErrLevel.ABORTED;
                foreach (SpheresException2 ex in alSpheresException)
                {
                    Logger.Log(new LoggerData(ex));
                }
            }
            return ret;
        }
        #endregion Valorize
        #endregion Methods
    }
    #endregion EventsValIRD
}

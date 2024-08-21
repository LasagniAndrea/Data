using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
//
using FpML.Enum;
using FpML.Interface;

namespace EFS.Process.MarkToMarket
{
    // PM 20160811 [RATP] New 
    #region MarkToMarketGenProcessETD
    public class MarkToMarketGenProcessETD : MarkToMarketGenProcessFxBase
    {
        #region Members
        private DateTime m_ExpiryDate;
        private IMoney m_MarkToMarketAmount;
        private ExchangeTradedDerivativeContainer m_ExchangeTradedDerivative;
        #endregion Members

        #region Constructors
        // EG 20180502 Analyse du code Correction [CA2214]
        public MarkToMarketGenProcessETD(MarkToMarketGenProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pProcess, pDsTrade, pTradeLibrary, pProduct) 
        {
        }
        #endregion Constructors

        #region Methods
        #region Valorize
        /// <summary>
        /// Master process to valorize a MarkToMarket amount for an FxSimpleOption
        /// </summary>
        /// <returns></returns>
        // EG 20180502 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
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
                    //
                    m_ExchangeTradedDerivative = new ExchangeTradedDerivativeContainer(Process.Cs, dbTransaction, (IExchangeTradedDerivative)Product, TradeLibrary.DataDocument);
                    //
                    DataRow[] rowChilds = rowEventProduct.GetChildRows(DsEvents.ChildEvent);
                    foreach (DataRow rowChild in rowChilds)
                    {
                        string eventCode = rowChild["EVENTCODE"].ToString();
                        string eventType = rowChild["EVENTTYPE"].ToString();
                        m_ExpiryDate = Convert.ToDateTime(rowChild["DTENDUNADJ"]);
                        //
                        bool isRowMustBeCalculate = IsRowMustBeCalculate(rowChild);
                        if (isRowMustBeCalculate)
                        {
                            isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_MarkToMarketGenProcess.ScanCompatibility_Event(Convert.ToInt32(rowChild["IDE"])));
                        }
                        //
                        if (isRowMustBeCalculate)
                        {
                            int streamNo = Convert.ToInt32(rowChild["STREAMNO"]);
                            //m_ParamInstrumentNo.Value = Convert.ToInt32(rowChild["INSTRUMENTNO"]);
                            m_ParamStreamNo.Value = streamNo;
                            //
                            DataRow rowCandidate = GetRowAmountGroup();
                            //
                            if (null != rowCandidate)
                            {
                                DateTime dtQuote = CommonValDate;
                                DateTime maturityDateSys = m_ExchangeTradedDerivative.AssetETD.Maturity_MaturityDateSys;
                                if (DtFunc.IsDateTimeFilled(maturityDateSys) && (dtQuote >= maturityDateSys))
                                {
                                    dtQuote = maturityDateSys;
                                }
                                Nullable<decimal> quote = GetETDQuote(CommonValDate);

                                if (quote.HasValue)
                                {

                                    DataRowEvent rowInfo = new DataRowEvent(rowChild);
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 611), 1,
                                        new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                                        new LogParam(LogTools.IdentifierAndId(rowInfo.EventCode + " / " + rowInfo.EventType, rowInfo.Id))));

                                    string currency = m_ExchangeTradedDerivative.PriceCurrency;
                                    // EG 20170127 Qty Long To Decimal
                                    decimal quantity = m_ExchangeTradedDerivative.Qty;
                                    decimal contractMultiplier = m_ExchangeTradedDerivative.ContractMultiplierQuotedCurrency;
                                    decimal price100 = m_ExchangeTradedDerivative.Price100;
                                    decimal strikePrice = m_ExchangeTradedDerivative.StrikePrice;
                                    decimal strikePrice100 = m_ExchangeTradedDerivative.ToBase100(strikePrice);
                                    decimal quote100 = m_ExchangeTradedDerivative.ToBase100(quote.Value);
                                    quote100 = m_ExchangeTradedDerivative.VariableContractValue(strikePrice100, quote100);
                                    //
                                    decimal cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(quote100, price100, contractMultiplier, quantity);

                                    DataRow rowQuantity = GetRowEventByEventType(EventTypeFunc.Quantity, streamNo).FirstOrDefault();

                                    PayerReceiverInfo payerReceiverInfoCur = new PayerReceiverInfo(this, rowQuantity);
                                    PayerReceiverInfoDet payer = new PayerReceiverInfoDet(PayerReceiverEnum.Payer,
                                        payerReceiverInfoCur.Payer, null, payerReceiverInfoCur.BookPayer, null);
                                    PayerReceiverInfoDet receiver = new PayerReceiverInfoDet(PayerReceiverEnum.Payer,
                                        payerReceiverInfoCur.Receiver, null, payerReceiverInfoCur.BookReceiver, null);

                                    if (0 > cashFlowAmount)
                                    {
                                        PayerReceiverInfoDet payRecSave = payer;
                                        payer = receiver;
                                        receiver = payRecSave;
                                        cashFlowAmount = System.Math.Abs(cashFlowAmount);
                                    }
                                    m_MarkToMarketAmount = ProductBase.CreateMoney(cashFlowAmount, currency);


                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 617), 2,
                                        new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                                        new LogParam(MtMEventCodes.First + " / " + MtMEventCodes.Second + " / " + EventClassFunc.SpotRate),
                                        new LogParam((null != m_MarkToMarketAmount) ? LogTools.AmountAndCurrency(m_MarkToMarketAmount.Amount.DecValue, m_MarkToMarketAmount.Currency) : "NaN")));

                                    AddRowMarkToMarket(rowCandidate, m_MarkToMarketAmount.Amount.DecValue, m_MarkToMarketAmount.Currency, EventClassFunc.SpotRate, payer, receiver);

                                    Update(dbTransaction, Convert.ToInt32(rowCandidate["IDE"]), false);
                                }
                            }
                        }
                    }
                    DataHelper.CommitTran(dbTransaction);
                }
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

        #region IsRowMustBeCalculate
        public override bool IsRowMustBeCalculate(DataRow pRow)
        {
            string eventCode = pRow["EVENTCODE"].ToString();
            return ((EventCodeFunc.IsETDFuture(eventCode) || EventCodeFunc.IsETDOption(eventCode)) && (0 < m_ExpiryDate.CompareTo(CommonValDate)));
        }
        #endregion
        
        #region GetETDQuote
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Upd GetQuoteLock 
        private Nullable<decimal> GetETDQuote(DateTime pDtQuote)
        {
            Nullable<decimal> ret = null;
            SystemMSGInfo errReadOfficialClose = null;
            try
            {
                int idAsset = m_ExchangeTradedDerivative.AssetETD.IdAsset;
                string assetIdentifier = m_ExchangeTradedDerivative.AssetETD.Identifier;

                if (Process.ProcessCacheContainer.GetQuoteLock(idAsset, pDtQuote, assetIdentifier, 
                    QuotationSideEnum.OfficialClose, Cst.UnderlyingAsset.ExchangeTradedContract, new KeyQuoteAdditional() , ref errReadOfficialClose) is Quote_ETDAsset quote && quote.valueSpecified)
                {
                    ret = quote.value;
                }
                if (null != errReadOfficialClose)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    Process.ProcessState.SetErrorWarning(errReadOfficialClose.processState.Status);
                    Logger.Log(errReadOfficialClose.ToLoggerData(0));
                }

            }
            catch (SpheresException2 ex)
            {
                if (false == ProcessStateTools.IsCodeReturnUndefined(ex.ProcessState.CodeReturn))
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    Process.ProcessState.SetErrorWarning(errReadOfficialClose.processState.Status);
                    Logger.Log(errReadOfficialClose.ToLoggerData(0));

                    throw new SpheresException2(errReadOfficialClose.processState);
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return ret;
        }
        #endregion GetETDQuote
        #endregion Methods
    }
    #endregion MarkToMarketGenProcessETD
}

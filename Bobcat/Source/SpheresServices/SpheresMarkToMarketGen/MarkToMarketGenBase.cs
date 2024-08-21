#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Status;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Curve;
using EfsML.Enum;
using EfsML.Enum.Tools;
//
using FixML.Enum;
using FixML.Interface;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.MarkToMarket
{
    #region MarkToMarketGenProcessBase
    // EG 20150317 [POC] : New accessors MarkToMarketGenProcess|MtMEventCodes
    public abstract class MarkToMarketGenProcessBase : CommonValProcessBase
    {
        #region Members
        protected CommonValParameters m_Parameters;
        protected MarkToMarketGenProcess m_MarkToMarketGenProcess;
        protected MarkToMarketGenMQueue m_MarkToMarketGenMQueue;
        protected PricingValues m_PricingValues;
        protected DataSet m_DsEventsPricing;
        protected RptSideProductContainer m_RptSideProductContainer;
        protected bool m_IsDealerBuyer;
        protected Nullable<Cst.MarginingMode> m_MarginingMode;
        #endregion Members
        #region Accessors
        // EG 20150317 [POC]
        public MarkToMarketGenProcess MarkToMarketGenProcess
        {
            get { return m_MarkToMarketGenProcess; }
        }

        #region CommonValDate
        public override DateTime CommonValDate
        {
            get
            {
                DateTime ret = DateTime.MinValue;
                if (m_Process.MQueue.IsMasterDateSpecified)
                    ret = m_Process.MQueue.GetMasterDate();
                return ret;
            }
        }
        #endregion CommonValDate
        #region IsDealerBuyer
        public bool IsDealerBuyer
        {
            get { return m_IsDealerBuyer;}
        }
        #endregion IsDealerBuyer
        #region DsEventsPricing
        public DataSet DsEventsPricing
        {
            get { return m_DsEventsPricing; }
        }
        #endregion DsEventsPricing
        #region DtEventPricing
        public DataTable DtEventPricing
        {
            get
            {
                return m_DsEventsPricing.Tables["EventPricing"];
            }
        }
        #endregion DtEventPricing
        #region DtEventPricing2
        public DataTable DtEventPricing2
        {
            get
            {
                return m_DsEventsPricing.Tables["EventPricing2"];
            }
        }
        #endregion DtEventPricing2
        #region MtMEventCodes
        // EG 20150317 [POC]
        public Pair<string, string> MtMEventCodes
        {
            get
            {
                Pair<string, string> mtmEventCodes = new Pair<string, string>(EventCodeFunc.DailyClosing, EventTypeFunc.MarkToMarket);
                if (m_MarkToMarketGenMQueue.isEOD)
                {
                    mtmEventCodes.First = EventCodeFunc.LinkedProductClosing;
                    mtmEventCodes.Second = EventTypeFunc.MarginRequirement;
                }
                return mtmEventCodes;
            }
        }
        #endregion MtMEventCodes

        #region Parameters
        public override CommonValParameters Parameters
        {
            get { return m_Parameters; }
        }
        #endregion Parameters
        #endregion Accessors
        #region Constructor
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20180502 Analyse du code Correction [CA2214]
        public MarkToMarketGenProcessBase(MarkToMarketGenProcess pMarkToMarketGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pMarkToMarketGenProcess, pDsTrade, pTradeLibrary, pProduct)
        {
            m_MarkToMarketGenProcess = pMarkToMarketGenProcess;
            m_MarkToMarketGenMQueue = (MarkToMarketGenMQueue)m_MarkToMarketGenProcess.MQueue;
            pMarkToMarketGenProcess.ProcessCacheContainer = new ProcessCacheContainer(pMarkToMarketGenProcess.Cs, (IProductBase)pProduct);

            // EG 20150612 [20665]
            //InitializeDataSetEvent();
            //LoadDsEventPricing();
            //Initialize();
        }
        #endregion Constructor
        #region Methods
        #region AddNewRowEvent
        /// <summary>
        /// Ajoute un évènement CLO/MTM à l'évènement parent {pRow}
        /// <para>L'évènement parent est cloné, puis certaines propriété sont modifiées ( EVENTCODE,EVENTTYPE,DTSTARTADJ, etc....)</para>
        /// </summary>
        /// <param name="pRow">Evènement parent</param>
        /// <param name="pIdE">IdE du nouvel évènement</param>
        // EG 20150317 [POC] use MtMEventCodes
        private void AddNewRowEvent(DataRow pRow, int pIdE)
        {
            DataRow rowMarkToMarket = DsEvents.DtEvent.NewRow();
            rowMarkToMarket.ItemArray = (object[])pRow.ItemArray.Clone();
            rowMarkToMarket.BeginEdit();
            rowMarkToMarket["IDE"] = pIdE;
            rowMarkToMarket["IDE_EVENT"] = pRow["IDE"];
            rowMarkToMarket["EVENTCODE"] = MtMEventCodes.First;
            rowMarkToMarket["EVENTTYPE"] = MtMEventCodes.Second;
            rowMarkToMarket["SOURCE"] = m_MarkToMarketGenProcess.AppInstance.ServiceName;
            rowMarkToMarket["IDSTTRIGGER"] = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
            rowMarkToMarket["DTSTARTADJ"] = CommonValDate;
            rowMarkToMarket["DTSTARTUNADJ"] = CommonValDate;
            rowMarkToMarket["DTENDADJ"] = CommonValDate;
            rowMarkToMarket["DTENDUNADJ"] = CommonValDate;
            rowMarkToMarket.EndEdit();
            DsEvents.DtEvent.Rows.Add(rowMarkToMarket);
        }
        #endregion AddNewRowEvent
        #region AddRowMarkToMarket
        // EG 20150608 [21011] Change PayerReceiverInfo pPayerReceiverInfo with PayerReceiverInfoDet pPayer|pReceiver
        protected void AddRowMarkToMarket(DataRow pRow, Nullable<decimal> pAmount, string pCurrency, string pEventClass,
            PayerReceiverInfoDet pPayer, PayerReceiverInfoDet pReceiver)
        {
            int idEParent = Convert.ToInt32(pRow["IDE"]);
            DataRow rowMarkToMarket = GetRowMarkToMarket(idEParent);
            int idE;
            if (null == rowMarkToMarket)
            {
                Cst.ErrLevel errLevel = SQLUP.GetId(out idE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
                if (Cst.ErrLevel.SUCCESS != errLevel)
                    throw new Exception(StrFunc.AppendFormat("Error:{0} on get Id for MTM Event", errLevel.ToString()));
                AddNewRowEvent(pRow, idE);
            }
            else
            {
                idE = Convert.ToInt32(rowMarkToMarket["IDE"]);
            }

            #region MarkToMarket Row valuation
            rowMarkToMarket = GetRowMarkToMarket(idEParent);
            rowMarkToMarket.BeginEdit();
            rowMarkToMarket["IDE"] = idE;
            rowMarkToMarket["IDA_PAY"] = Convert.DBNull;
            rowMarkToMarket["IDB_PAY"] = Convert.DBNull;
            rowMarkToMarket["IDA_REC"] = Convert.DBNull;
            rowMarkToMarket["IDB_REC"] = Convert.DBNull;

            if (pAmount.HasValue)
            {
                // EG 20150605 [21011]
                //if (null != pPayerReceiverInfo)
                //{
                //    rowMarkToMarket["IDA_PAY"] = pPayerReceiverInfo.Payer;
                //    rowMarkToMarket["IDB_PAY"] = pPayerReceiverInfo.BookPayer;
                //    rowMarkToMarket["IDA_REC"] = pPayerReceiverInfo.Receiver;
                //    rowMarkToMarket["IDB_REC"] = pPayerReceiverInfo.BookReceiver;
                //}
                //if (-1 == Math.Sign(pAmount.Value))
                //    CommonValFunc.SwapPayerAndReceiver(rowMarkToMarket);
                CommonValFunc.SetPayerAndReceiver(rowMarkToMarket, pAmount.Value, pPayer, pReceiver);
            }

            rowMarkToMarket["VALORISATION"] = pAmount.HasValue ? Math.Abs(pAmount.Value) : Convert.DBNull;
            rowMarkToMarket["UNIT"] = pAmount.HasValue ? pCurrency : Convert.DBNull;
            rowMarkToMarket["UNITTYPE"] = UnitTypeEnum.Currency.ToString();
            rowMarkToMarket["VALORISATIONSYS"] = pAmount.HasValue ? Math.Abs(pAmount.Value) : Convert.DBNull;
            rowMarkToMarket["UNITSYS"] = pAmount.HasValue ? pCurrency : Convert.DBNull;
            rowMarkToMarket["UNITTYPESYS"] = UnitTypeEnum.Currency.ToString();

            SetRowStatus(rowMarkToMarket, Tuning.TuningOutputTypeEnum.OES);
            rowMarkToMarket.EndEdit();
            #endregion MarkToMarket Row valuation

            #region MarkToMarket Row EventClass
            DeleteAllRowEventClass(idE);
            if (null == GetRowEventClass(idE))
                AddNewRowEventClass(idE, pEventClass, CommonValDate, false);

            DataRow rowMarkToMarketClass = GetRowEventClass(idE)[0];
            rowMarkToMarketClass.BeginEdit();
            rowMarkToMarketClass["EVENTCLASS"] = pEventClass;
            rowMarkToMarketClass.EndEdit();
            #endregion MarkToMarket Row EventClass

            if (false == m_MarkToMarketGenMQueue.isEOD)
            {
                #region	MarkToMarket Accounting EventClass (CLA , ...)
                AddRowAccountingEventClass(idE);
                #endregion	MarkToMarket Accounting EventClass (CLA , ...)
            }
        }
        #endregion AddRowMarkToMarket
        
        #region AddRowLiquidationOptionValue
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pEventClass"></param>
        /// <param name="pPayer"></param>
        /// <param name="pReceiver"></param>
        /// EG 20150407 [POC]
        /// EG 20150608 [21011] Suppression paramètre pPayerReceiverInfo
        /// FI 20180220 [XXXXX] Modify
        // EG 20190613 [24683] Use slaveDbTransaction
        protected void AddRowLiquidationOptionValue(DataRow pRow, Nullable<decimal> pAmount, string pCurrency, string pEventClass,
            PayerReceiverInfoDet pPayer, PayerReceiverInfoDet pReceiver)
        {
            int idE = 0;
            int idEParent = Convert.ToInt32(pRow["IDE"]);
            DataRow rowLOV = GetRowLiquidationOptionValue(idEParent);
            if (null == rowLOV)
            {
                rowLOV = NewRowEvent(SlaveDbTransaction, pRow, EventCodeFunc.LinkedProductClosing, EventTypeFunc.LiquidationOptionValue, CommonValDate, CommonValDate, m_MarkToMarketGenProcess.AppInstance);
                DsEvents.DtEvent.Rows.Add(rowLOV);
            }
            if (null != rowLOV)
                idE = Convert.ToInt32(rowLOV["IDE"]);

            #region LiquidationOptionValue Row valuation
            rowLOV.BeginEdit();
            rowLOV["IDA_PAY"] = Convert.DBNull;
            rowLOV["IDB_PAY"] = Convert.DBNull;
            rowLOV["IDA_REC"] = Convert.DBNull;
            rowLOV["IDB_REC"] = Convert.DBNull;

            if (pAmount.HasValue)
            {
                if ((null != pPayer) && (null != pReceiver))
                {
                    // FI 20180220 [XXXXX] add tests sur First.HasValue
                    rowLOV["IDA_PAY"] = pPayer.actor.First ?? Convert.DBNull;
                    rowLOV["IDB_PAY"] = pPayer.book.First ?? Convert.DBNull;
                    rowLOV["IDA_REC"] = pReceiver.actor.First ?? Convert.DBNull;
                    rowLOV["IDB_REC"] = pReceiver.book.First ?? Convert.DBNull;
                }
            }

            rowLOV["VALORISATION"] = pAmount.HasValue ? Math.Abs(pAmount.Value) : Convert.DBNull;
            rowLOV["UNIT"] = pAmount.HasValue ? pCurrency : Convert.DBNull;
            rowLOV["UNITTYPE"] = UnitTypeEnum.Currency.ToString();
            rowLOV["VALORISATIONSYS"] = pAmount.HasValue ? Math.Abs(pAmount.Value) : Convert.DBNull;
            rowLOV["UNITSYS"] = pAmount.HasValue ? pCurrency : Convert.DBNull;
            rowLOV["UNITTYPESYS"] = UnitTypeEnum.Currency.ToString();

            SetRowStatus(rowLOV, Tuning.TuningOutputTypeEnum.OES);
            rowLOV.EndEdit();
            #endregion LiquidationOptionValue Row valuation

            #region LiquidationOptionValue Row EventClass
            if (null == GetRowEventClass(idE))
                AddNewRowEventClass(idE, pEventClass, CommonValDate, false);

            DataRow rowLOVClass = GetRowEventClass(idE)[0];
            rowLOVClass.BeginEdit();
            rowLOVClass["EVENTCLASS"] = pEventClass;
            rowLOVClass.EndEdit();
            #endregion LiquidationOptionValue Row EventClass
        }
        #endregion AddRowLiquidationOptionValue
        #region AddRowEventPricing2
        /// <summary>
        /// Ajoute une ligne dans EVENTPRICING
        /// </summary>
        /// <param name="pRow"></param>
        protected void AddRowEventPricing2(int pIdE)
        {
            DataRow row = DtEventPricing2.NewRow();
            row.BeginEdit();
            row["IDE_SOURCE"] = pIdE;
            row["DTCLOSING"] = CommonValDate;
            row.EndEdit();
            DtEventPricing2.Rows.Add(row);
        }
        #endregion AddRowEventPricing2

        #region CalcActualizedAmount
        /// <summary>
        /// Retourne l'actualisation du montant {pMoney}, ce montant est réglé en date {pDtSettlement}
        /// </summary>
        /// <param name="pMoney">Montant source</param>
        /// <param name="pMoney">Date de rglt du montant</param>
        /// <param name="pPayerReceiverInfo">payer,receiver du montant</param>
        /// <param name="pYieldCurveDef">Courbe de taux utilisée</param>
        /// <param name="pDiscountFactor">discount Factor obtenu pour actualiser</param>
        /// <returns></returns>
        protected decimal CalcActualizedAmount(IMoney pMoney, DateTime pDtSettlement, PayerReceiverInfo pPayerReceiverInfo, out  YieldCurveVal pYieldCurveVal, out decimal pDiscountFactor)
        {
            string idYieldCurveDef = string.Empty;
            SQL_AssetCash sqlAssetCash = SQL_AssetCash.GetAssetCash(m_CS, pMoney.Currency, CommonValDate);
            if (null != sqlAssetCash)
                idYieldCurveDef = sqlAssetCash.IdYieldCurveDef;

            YieldCurve yieldCurve = new YieldCurve(m_CS, ProductBase, CommonValDate, pMoney.Currency, idYieldCurveDef,
                pPayerReceiverInfo.Payer, pPayerReceiverInfo.BookPayer, pPayerReceiverInfo.Receiver, pPayerReceiverInfo.BookReceiver);

            Decimal discountFactor = Decimal.Zero;
            if (yieldCurve.IsLoaded(m_CS, ProductBase, m_Process.Session))
                discountFactor = Convert.ToDecimal(yieldCurve.GetPointValue(YieldCurveValueEnum.discountFactor, pDtSettlement));

            decimal ret = pMoney.Amount.DecValue * discountFactor;
            pYieldCurveVal = yieldCurve.YieldCurveVal;
            pDiscountFactor = discountFactor;
            return ret;
        }
        #endregion CalcActualize

        #region DeleteRowsEventPricing2
        /// <summary>
        /// Supprime toutes les lignes existantes sous EVENTPRICING2 associées à l'évènement 
        /// <para>L'évènement est représentatif d'un stream (ex EVENTCODE = 'IRS')</para>
        /// </summary>
        /// <param name="pIdE"></param>
        protected void DeleteRowsEventPricing2(DataRow pRowEvent)
        {
            DataRow rowMTM = GetRowMarkToMarket(Convert.ToInt32(pRowEvent["IDE"]));
            if (null != rowMTM)
            {
                DataRow[] rows = GetRowEventPricing2(Convert.ToInt32(rowMTM["IDE"]), false);
                if (ArrFunc.IsFilled(rows))
                {
                    foreach (DataRow row in rows)
                        row.Delete();
                }
            }
        }
        #endregion DeleteRowsEventPricing2

        #region GetRowMarkToMarket
        /// <summary>
        /// Retourne la ligne MTM en date {CommonValDate} rattaché à l'évènement parent {pIdEParent}
        /// <para>Retourne null si non trouvée</para>
        /// </summary>
        // EG 20150317 [POC] use MtMEventCodes
        protected DataRow GetRowMarkToMarket(int pIdEParent)
        {
            DataRow ret = null;
            DataRow[] rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDE_EVENT = {0} and EVENTTYPE = '{1}' and DTENDADJ ='{2}'",
                pIdEParent, MtMEventCodes.Second, DtFunc.DateTimeToStringDateISO(CommonValDate)), "IDE");
            if (ArrFunc.IsFilled(rows))
                ret = rows[0];
            return ret;
        }
        #endregion GetRowMarkToMarket
        #region GetRowLiquidationOptionValue
        /// <summary>
        /// Retourne la ligne LOV en date {CommonValDate} rattaché à l'évènement parent {pIdEParent}
        /// <para>Retourne null si non trouvée</para>
        /// </summary>
        // EG 20150407 [POC]
        protected DataRow GetRowLiquidationOptionValue(int pIdEParent)
        {
            DataRow ret = null;
            DataRow[] rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDE_EVENT = {0} and EVENTTYPE = '{1}' and DTENDADJ ='{2}'",
                pIdEParent, EventTypeFunc.LiquidationOptionValue, DtFunc.DateTimeToStringDateISO(CommonValDate)), "IDE");
            if (ArrFunc.IsFilled(rows))
                ret = rows[0];
            return ret;
        }
        #endregion GetRowLiquidationOptionValue
        #region GetRowEventPricing2
        /// <summary>
        /// Retourne les lignes de EVENTPRICING2 rattachées à l'évènement source {pIdE_Source}
        /// <para>Retourne null si non trouvée</para>
        /// </summary>
        protected DataRow[] GetRowEventPricing2(int pIdE)
        {
            return GetRowEventPricing2(pIdE, true);
        }
        protected DataRow[] GetRowEventPricing2(int pIdE, bool pIsIdE_Source)
        {
            DataRow[] rows = null;
            if (null != DsEventsPricing)
            {
                rows = DtEventPricing2.Select(StrFunc.AppendFormat(@"{0} = {1} and DTCLOSING = '{2}'",
                    (pIsIdE_Source ? "IDE_SOURCE" : "IDE"), pIdE, DtFunc.DateTimeToStringDateISO(CommonValDate)), "IDE");
            }
            return rows;
        }
        #endregion GetRowEventPricing2

        #region EndOfInitialize
        // EG 20180502 Analyse du code Correction [CA2214]
        public override void EndOfInitialize()
        {
            base.EndOfInitialize();
            LoadDsEventPricing();
            Initialize();
        }
        #endregion EndOfInitialize
        #region Initialize
        /// <summary>
        /// Alimente les membres m_DebtSecurityTransactionContainer,_buyer,_bookBuyer,_seller,_bookSeller de la classe
        /// </summary>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void Initialize()
        {
            string tradeStBusiness = DsTrade.DtTrade.Rows[0]["IDSTBUSINESS"].ToString();

            if (m_CurrentProduct.ProductBase.IsFxLeg)
                m_RptSideProductContainer = new FxLegContainer((IFxLeg)m_CurrentProduct, TradeLibrary.DataDocument);
            else if (m_CurrentProduct.ProductBase.IsFxOptionLeg)
                m_RptSideProductContainer = new FxOptionLegContainer((IFxOptionLeg)m_CurrentProduct, TradeLibrary.DataDocument);

            if (null != m_RptSideProductContainer)
            {
                m_RptSideProductContainer.InitRptSide(m_CS, (Cst.StatusBusiness.ALLOC.ToString() == tradeStBusiness));

                // Buyer - Seller
                IFixParty buyer = m_RptSideProductContainer.GetBuyerSeller(SideEnum.Buy);
                IFixParty seller = m_RptSideProductContainer.GetBuyerSeller(SideEnum.Sell);
                if (null == buyer)
                    throw new NotSupportedException("buyer is not Found");
                if (null == seller)
                    throw new NotSupportedException("seller is not Found");

                IReference buyerReference = buyer.PartyId;
                m_Buyer = m_RptSideProductContainer.DataDocument.GetParty(buyerReference.HRef);
                m_BookBuyer = m_RptSideProductContainer.DataDocument.GetOTCmlId_Book(m_Buyer.Id);

                IReference sellerReference = seller.PartyId;
                m_Seller = m_RptSideProductContainer.DataDocument.GetParty(sellerReference.HRef);
                m_BookSeller = m_RptSideProductContainer.DataDocument.GetOTCmlId_Book(m_Seller.Id);

                m_IsDealerBuyer = m_RptSideProductContainer.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER);

                ProductContainer _product = new ProductContainer(m_CurrentProduct);
                _ = _product.GetInstrument(m_CS, out SQL_Instrument _sql_Instrument);
                if (null != _sql_Instrument)
                    m_MarginingMode = InstrTools.GetActorMarginingMode(m_CS, _sql_Instrument.Id, m_IsDealerBuyer ? m_Buyer.OTCmlId : m_Seller.OTCmlId);
            }
        }
        #endregion Initialize

        #region GetYieldCurve
        /// <summary>
        /// Get the curve created for the date CommonValDate with respect to the given Book, Currency and date of the required interest.
        /// </summary>
        /// <param name="pIdC"></param>
        /// <param name="pDtPoint"></param>
        /// <param name="pPayerReceiverInfo"></param>
        /// <returns></returns>
        /// EG 20150402 [POC] New
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected YieldCurve GetYieldCurve(string pIdC, DateTime pDtPoint, PayerReceiverInfo pPayerReceiverInfo)
        {
            string idYieldCurveDef = string.Empty;
            SQL_AssetCash sqlAssetCash = SQL_AssetCash.GetAssetCash(m_CS, pIdC, pDtPoint);
            if (null != sqlAssetCash)
                idYieldCurveDef = sqlAssetCash.IdYieldCurveDef;

            YieldCurve yieldCurve = new YieldCurve(m_CS, m_tradeLibrary.Product.ProductBase, CommonValDate, pIdC, idYieldCurveDef,
                pPayerReceiverInfo.Payer, pPayerReceiverInfo.BookPayer, pPayerReceiverInfo.Receiver, pPayerReceiverInfo.BookReceiver);

            // EG 201503
            Process.ProcessCacheContainer.InitDelegate(Process.ProcessState.SetErrorWarning);
            yieldCurve.ProcessCacheContainer = Process.ProcessCacheContainer;
            return yieldCurve;
        }
        #endregion GetYieldCurve
        #region InterestRateAndDiscountFactor
        /// <summary>
        /// Read the interest rate and Discount factor in the curve created for the date CommonValDate with respect to the given Book, Currency and date of the required interest.
        /// </summary>
        /// <param name="pIdC"></param>
        /// <param name="pDtPoint"></param>
        /// <param name="pPayerReceiverInfo"></param>
        /// <returns></returns>
        /// EG 20150402 [POC] New 
        protected Pair<Nullable<decimal>, Nullable<decimal>> InterestRateAndDiscountFactor(string pIdC, DateTime pDtPoint, PayerReceiverInfo pPayerReceiverInfo)
        {
            Pair<Nullable<decimal>, Nullable<decimal>> _interestRate = new Pair<Nullable<decimal>, Nullable<decimal>>();
            YieldCurve yieldCurve = GetYieldCurve(pIdC, pDtPoint, pPayerReceiverInfo);
            if (yieldCurve.IsLoaded(m_CS, m_tradeLibrary.Product.ProductBase, m_Process.Session))
            {
                _interestRate.First = Convert.ToDecimal(yieldCurve.GetPointValue(pDtPoint));
                _interestRate.Second = Convert.ToDecimal(yieldCurve.GetPointValue(YieldCurveValueEnum.discountFactor, pDtPoint));
            }
            return _interestRate;
        }
        #endregion InterestRateAndDiscountFactor
        #region InterestRate
        /// <summary>
        /// Read the interest rate in the curve created for the date CommonValDate with respect to the given Book, Currency and date of the required interest.
        /// </summary>
        /// <param name="pIdC">Currency Id</param>
        /// <param name="pDtPoint">Date of the required interest</param>
        /// <returns></returns>
        /// EG 20150402 [POC] Modify Return : pair(Rate,DiscountFactor)
        protected Nullable<decimal> InterestRate(string pIdC, DateTime pDtPoint, PayerReceiverInfo pPayerReceiverInfo)
        {
            Nullable<decimal> _rate = null;
            YieldCurve yieldCurve = GetYieldCurve(pIdC, pDtPoint, pPayerReceiverInfo);
            if (yieldCurve.IsLoaded(m_CS, m_tradeLibrary.Product.ProductBase, m_Process.Session))
            {
                _rate = Convert.ToDecimal(yieldCurve.GetPointValue(pDtPoint));
            }
            return _rate;
        }
        #endregion InterestRate

        #region LoadDsEventPricing
        /// <summary>
        /// Chargement du DataSet des tables EVENTPRICING|EVENTPRICING2
        /// </summary>
        private void LoadDsEventPricing()
        {
            string sqlWhereOrder = "where (ev.IDT = @IDT) order by ev.IDE" + Cst.CrLf;
            string sqlSelect = string.Empty;
            sqlSelect += QueryLibraryTools.GetQuerySelect(m_CS, Cst.OTCml_TBL.EVENTPRICING) + sqlWhereOrder;
            sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += QueryLibraryTools.GetQuerySelect(m_CS, Cst.OTCml_TBL.EVENTPRICING2) + sqlWhereOrder;

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(m_CS, "IDT", DbType.Int32), Convert.ToInt32(m_ParamIdT.Value));
            QueryParameters qryParameters = new QueryParameters(m_CS, sqlSelect, parameters);
            m_DsEventsPricing = DataHelper.ExecuteDataset(m_CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            m_DsEventsPricing.DataSetName = "EventPricing";
            DataTable dtEventPricing = m_DsEventsPricing.Tables[0];
            dtEventPricing.TableName = "EventPricing";
            DataTable dtEventPricing2 = m_DsEventsPricing.Tables[1];
            dtEventPricing2.TableName = "EventPricing2";
        }
        #endregion LoadDsEventPricing

        #region Update
        protected override void Update(IDbTransaction pDbTransaction, int pIdE, bool pIsError)
        {
            base.Update(pDbTransaction, pIdE, pIsError);
            UpdateEventPricing(pDbTransaction, pIdE, EventTypeFunc.MarkToMarket);
            UpdateEventPricing(pDbTransaction, pIdE, EventTypeFunc.LiquidationOptionValue);
        }

        // EG 20180502 Analyse du code Correction [CA2200]
        protected override void Update(int pIdE, bool pIsError)
        {
            IDbTransaction dbTransaction = DataHelper.BeginTran(m_CS);
            bool isException = false;
            try
            {
                base.Update(dbTransaction, pIdE, pIsError);
                UpdateEventPricing(dbTransaction, pIdE, EventTypeFunc.MarkToMarket);
                UpdateEventPricing(dbTransaction, pIdE, EventTypeFunc.LiquidationOptionValue);
                DataHelper.CommitTran(dbTransaction);
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((isException) && (null != dbTransaction))
                {
                    try {DataHelper.RollbackTran(dbTransaction);}
                    catch { }
                }
            }
        }
        #endregion
        #region UpdateEventPricing
        private void UpdateEventPricing(IDbTransaction pDbTransaction, int pIdE, string pEventType)
        {
            DataRow row = null;

            if (pEventType == EventTypeFunc.MarkToMarket)
                row = GetRowMarkToMarket(pIdE);
            else if (pEventType == EventTypeFunc.LiquidationOptionValue)
                row = GetRowLiquidationOptionValue(pIdE);

            if (null != row)
            {
                int idE_MTM = Convert.ToInt32(row["IDE"]);

                if (null != m_PricingValues)
                    EventQuery.UpdateEventPricing(pDbTransaction, idE_MTM, m_PricingValues, m_tradeLibrary.Product);

                if (null != DtEventPricing2.GetChanges())
                {
                    DataRow[] changeRows = DtEventPricing2.Select(StrFunc.AppendFormat(@"DTCLOSING = '{0}'", DtFunc.DateTimeToStringDateISO(CommonValDate)), null,
                        DataViewRowState.Added | DataViewRowState.ModifiedCurrent);

                    if (ArrFunc.IsFilled(changeRows))
                    {
                        foreach (DataRow changeRow in changeRows)
                        {
                            changeRow.BeginEdit();
                            changeRow["IDE"] = idE_MTM;
                            changeRow.EndEdit();
                        }
                    }
                }

                string sqlSelect = QueryLibraryTools.GetQuerySelect(m_CS, Cst.OTCml_TBL.EVENTPRICING2, true);
                DataHelper.ExecuteDataAdapter(pDbTransaction, sqlSelect, DtEventPricing2);
            }
        }
        #endregion UpdateEventPricing

        #region Valorize
        public override Cst.ErrLevel Valorize()
        {
            return Cst.ErrLevel.UNDEFINED;
        }
        #endregion Valorize

        #endregion Methods
    }
    #endregion MarkToMarketGenProcessBase

    #region MarkToMarketGenProcessFxBase
    public abstract class MarkToMarketGenProcessFxBase : MarkToMarketGenProcessBase
    {
        #region Constructors
        public MarkToMarketGenProcessFxBase(MarkToMarketGenProcess pMarkToMarketGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pMarkToMarketGenProcess, pDsTrade, pTradeLibrary, pProduct)
        {
        }
        #endregion Constructors
        #region Methods
        #region GetFxRate
        /// <summary>
        /// Read a spot rate to a date for a pair of currencies
        /// </summary>
        /// <param name="pTime">Spot date</param>
        /// <param name="pIdC1">Currency1</param>
        /// <param name="pIdC2">Currency2</param>
        /// <param name="pQuoteBasis">QuoteBasis</param>
        /// <returns>observedRate</returns>
        protected Nullable<decimal> GetFxRate(PayerReceiverInfo pPayerReceiverInfo, DateTime pTime, string pIdC1, string pIdC2, QuoteBasisEnum pQuoteBasis)
        {
            KeyQuote keyQuote = new KeyQuote(m_CS, pTime, pPayerReceiverInfo.Payer, pPayerReceiverInfo.BookPayer,
                pPayerReceiverInfo.Receiver, pPayerReceiverInfo.BookReceiver, QuoteTimingEnum.Close);
            return GetFxRate(keyQuote, pIdC1, pIdC2, pQuoteBasis);
        }
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Nullable<decimal> GetFxRate(KeyQuote pKeyQuote,string pIdC1, string pIdC2, QuoteBasisEnum pQuoteBasis)
        {
            SystemMSGInfo systemMsgInfo = null;
            try
            {
                #region KeyAssetFXRate
                KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate
                {
                    IdC1 = pIdC1,
                    IdC2 = pIdC2
                };
                #endregion KeyAssetFXRate

                Quote quote = Process.ProcessCacheContainer.ReadQuote(Cst.UnderlyingAsset.FxRateAsset, keyAssetFXRate, pKeyQuote, "xxx", ref systemMsgInfo);
                if ((null != systemMsgInfo))
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    Process.ProcessState.SetErrorWarning(systemMsgInfo.processState.Status);

                    
                    Logger.Log(systemMsgInfo.ToLoggerData(0));
                }
                Nullable<decimal> ret = null;
                if (null != quote && quote.valueSpecified)
                {
                    if (keyAssetFXRate.IsReverseQuotation(pQuoteBasis, pIdC1, pIdC2))
                        ret = 1 / quote.value;
                    else
                        ret = quote.value;
                }
                return ret;
            }
            catch (SpheresException2 ex)
            {
                if (false == ProcessStateTools.IsCodeReturnUndefined(ex.ProcessState.CodeReturn))
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    Process.ProcessState.SetErrorWarning(systemMsgInfo.processState.Status);

                    
                    Logger.Log(systemMsgInfo.ToLoggerData(0));

                    throw new SpheresException2(systemMsgInfo.processState);
                }
                else
                    throw;
            }
            catch (Exception) { throw; }
        }
        #endregion GetFxRate

        #region AddRowUnrealizedMargin
        // EG 20150706 [21021] Pair<int, Nullable<int>> for pPayer|pReceiver
        public void AddRowUnrealizedMargin(IDbTransaction pDbTransaction, DataRow pRowAMT, Nullable<decimal> pAmount, string pCurrency,
            decimal pInitialRate, decimal pClosingRate, Pair<int, Nullable<int>> pPayer, Pair<int, Nullable<int>> pReceiver)
        {
            if (null != pRowAMT)
            {
                string eventCodeLink = EventCodeLink(EventTypeFunc.Amounts, EventTypeFunc.UnrealizedMargin, QuoteTimingEnum.Close);
                m_ParamInstrumentNo.Value = Convert.ToInt32(pRowAMT["INSTRUMENTNO"]);
                m_ParamStreamNo.Value = Convert.ToInt32(pRowAMT["STREAMNO"]);

                DataRow rowEventClassREC = null;
                DataRow rowEvent = GetRowAmount(EventTypeFunc.UnrealizedMargin, EventClassFunc.Recognition, QuoteTimingEnum.Close, CommonValDate);
                bool isNewRow = (null == rowEvent);

                DataRow rowEventDet;
                if (isNewRow)
                {
                    // EG 20160106 [21679]POC-MUREX Add pDbTransaction
                    rowEvent = NewRowEvent(pDbTransaction, pRowAMT, eventCodeLink, EventTypeFunc.UnrealizedMargin, CommonValDate, m_MarkToMarketGenProcess.AppInstance);
                    rowEventClassREC = NewRowEventClass(Convert.ToInt32(rowEvent["IDE"]), EventClassFunc.Recognition, CommonValDate, false);
                    rowEventDet = NewRowEventDet(rowEvent);
                }
                else
                {
                    rowEventDet = GetRowEventDetail(Convert.ToInt32(rowEvent["IDE"]));
                }

                rowEventDet["DTACTION"] = CommonValDate;
                rowEventDet["QUOTETIMING"] = QuoteTimingEnum.Close;
                rowEventDet["QUOTEPRICE"] = pClosingRate;
                rowEventDet["QUOTEPRICE100"] = pClosingRate;
                rowEventDet["QUOTEDELTA"] = DBNull.Value;
                rowEventDet["PRICE"] = pInitialRate;
                rowEventDet["PRICE100"] = pInitialRate;

                int idE = Convert.ToInt32(rowEvent["IDE"]);
                // EG 20150616 [21124] Add null parameter for EventClass.VAL
                SetRowEventClosingAmountGen(pDbTransaction, isNewRow, idE, rowEvent, rowEventDet, rowEventClassREC, null, null, 1, 1, pAmount, pCurrency);
                CommonValFunc.SetPayerReceiver(rowEvent, pPayer.First, pPayer.Second, pReceiver.First, pReceiver.Second);
            }
        }
        #endregion AddRowUnrealizedMargin
        #endregion Methods
    }
    #endregion MarkToMarketGenProcessFxBase
}

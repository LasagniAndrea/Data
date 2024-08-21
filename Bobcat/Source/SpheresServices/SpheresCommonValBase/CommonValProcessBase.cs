#region Using Directives
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using System.Globalization;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.ApplicationBlocks.Data;

using EFS.TradeInformation;

using EFS.Status;
using EFS.Actor;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using FixML.Interface;
#endregion Using Directives


namespace EFS.Process
{
    /// <summary>
    /// 
    /// </summary>
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public abstract class CommonValProcessBase
    {
        #region Members
        protected EfsMLProcessBase m_Process;
        protected string m_CS;
        protected EFS_TradeLibrary m_tradeLibrary;
        protected DataSetTrade m_DsTrade;
        protected DataSetEventTrade m_DsEvents;

        protected DataSet m_DsEventsDetails;
        

        protected Quote m_Quote;

        protected IDbDataParameter m_ParamIdT;
        protected IDbDataParameter m_ParamIdI;
        protected IDbDataParameter m_ParamIdE;
        protected IDbDataParameter m_ParamIdEParent;
        protected IDbDataParameter m_ParamInstrumentNo;
        protected IDbDataParameter m_ParamStreamNo;
        protected IDbDataParameter m_ParamInstrumentNoReference;
        protected IDbDataParameter m_ParamStreamNoReference;

        protected IProduct m_CurrentProduct;
        // EG 20150331 (POC] Move EventsValProcessBase to CommonValProcessBase
        protected IParty m_Buyer;
        // EG 20150706 [21021] Nullable<int>
        protected Nullable<int> m_BookBuyer;
        protected IParty m_Seller;
        // EG 20150706 [21021] Nullable<int>
        protected Nullable<int> m_BookSeller;
        protected bool m_IsApplyMinMargin;
        // EG 20170510 [23153] New
        protected FeeRequest m_CurrentFeeRequest;
        #endregion Members

        #region Accessors
        #region SlaveDbTransaction
        // EG 20130807 Utilisation du Mode Slave avec une transaction (actuellement TradeActionGen only)
        // EG 20170410 [23081] Utilisation du Mode Slave avec une transaction (actuellement TradeActionGen et PoskeepingGen)
        // EG 20150612 [20665] New
        protected IDbTransaction SlaveDbTransaction
        {
            get
            {
                IDbTransaction dbTransaction = null;
                if ((m_Process.ProcessCall == ProcessBase.ProcessCallEnum.Slave) && (null != m_Process.SlaveDbTransaction))
                    dbTransaction = m_Process.SlaveDbTransaction;
                return dbTransaction;
            }
        }
        #endregion SlaveDbTransaction
        #region IsEndOfDayProcess
        public bool IsEndOfDayProcess
        {
            get
            {
                return (ProcessBase.ProcessCallEnum.Slave == Process.ProcessCall) &&
                      (null != m_Quote) && (m_Quote.isEODSpecified) && (m_Quote.isEOD);
            }
        }
        #endregion IsEndOfDayProcess

        #region IsRowHasChildrens
        public virtual bool IsRowHasChildrens(DataRow pRow)
        {
            DataRow[] rowChilds = pRow.GetChildRows(DsEvents.ChildEvent);
            foreach (DataRow rowChild in rowChilds)
            {
                if (IsRowMustBeCalculate(rowChild))
                    return true;
            }
            return false;
        }
        #endregion IsRowHasChildrens

        #region EntityMarketInfo
        public virtual IPosKeepingMarket EntityMarketInfo
        {
            get { return null; }
        }
        #endregion EntityMarketInfo


        #region virtual ParametersIRD
        public virtual CommonValParameters ParametersIRD
        {
            get { return null; }
        }
        #endregion virtual ParametersIRD
        #region virtual Parameters
        public virtual CommonValParameters Parameters
        {
            get { return null; }
        }
        #endregion virtual Parameters

        //        
        #region public virtual CommonValDate
        public virtual DateTime CommonValDate
        {
            get { return DateTime.MinValue; }
        }
        #endregion virtual CommonValDate
        #region public virtual CommonValDateIncluded
        public virtual DateTime CommonValDateIncluded
        {
            get { return DateTime.MinValue; }
        }
        #endregion CommonValDateIncluded
        //
        #region public tradeLibrary
        /// <summary>
        /// Représente le trade
        /// </summary>
        public EFS_TradeLibrary TradeLibrary
        {
            get { return m_tradeLibrary; }
        }
        #endregion
        #region public product
        /// <summary>
        /// Représente le product en cours de traitement
        /// <para>Ce produit est différent du produit du trade lorsque Spheres® traite une strategie</para>
        /// </summary>
        public IProduct Product
        {
            get { return m_CurrentProduct; }
        }
        #endregion
        #region public productBase
        /// <summary>
        /// Représente le productBase du product du trade
        /// <para>A utiliser uniquement pour faire appel aux méthodes de type Create de l'interface IProductBase (Ex CreateAdjustableOffset)</para>
        /// </summary>
        public IProductBase ProductBase
        {
            get { return m_tradeLibrary.Product.ProductBase; }
        }
        #endregion
        #region public Process
        public EfsMLProcessBase Process
        {
            get { return m_Process; }
        }
        #endregion virtual Process
        //
        #region public DsTrade
        public DataSetTrade DsTrade
        {
            get { return m_DsTrade; }
        }
        #endregion DsTrade
        #region public DsEvents
        public DataSetEventTrade DsEvents
        {
            get { return m_DsEvents; }
        }
        #endregion DsEvents
        #region public DsEventsDetails
        public DataSet DsEventsDetails
        {
            get { return m_DsEventsDetails; }
        }
        #endregion DsEventsDetails
        #region RowEventClass
        public DataRow RowEventClass
        {
            get
            {
                if ((null != DsEvents.DtEventClass) && (null != m_ParamIdE))
                {
                    DataRow[] dataRow = DsEvents.DtEventClass.Select("IDE=" + m_ParamIdE.Value.ToString(), "IDE");
                    if (1 == dataRow.Length)
                        return dataRow[0];
                }
                return null;
            }
        }
        #endregion RowEventClass
        #region RowEventDetail
        public DataRow RowEventDetail
        {
            get
            {
                if (null != DsEvents.DtEventDet)
                {
                    DataRow[] dataRow = m_DsEvents.DtEventDet.Select("IDE=" + m_ParamIdE.Value.ToString(), "IDE");
                    if (1 == dataRow.Length)
                        return dataRow[0];
                }
                return null;
            }
        }
        #endregion RowEventDetail

        #region RowNominalReference
        private DataRow[] RowNominalReference
        {
            get
            {
                return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE = '{3}' and EVENTTYPE = '{4}'",
                    m_DsTrade.IdT.ToString(), m_ParamInstrumentNoReference.Value, m_ParamStreamNoReference.Value, EventCodeFunc.NominalStep, EventTypeFunc.Nominal), "DTSTARTADJ");
            }
        }
        #endregion RowNominalReference

        #region RowPremium
        /// <summary>
        /// Retourne l'évènement LPP/PRM
        /// </summary>
        public DataRow RowPremium
        {
            get
            {
                DataRow[] dataRow = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and EVENTCODE = '{2}' and EVENTTYPE = '{3}'",
                    m_DsTrade.IdT.ToString(), m_ParamInstrumentNo.Value, EventCodeFunc.LinkedProductPayment, EventTypeFunc.Premium), "IDE");
                if (1 == dataRow.Length)
                    return dataRow[0];
                return null;
            }
        }
        #endregion RowPremium

        #region RowStartCurrency1
        /// <summary>
        /// Obtient l'évènement STA/CU1
        /// </summary>
        public DataRow RowStartCurrency1
        {
            get
            {
                return GetRowCurrency(EventCodeFunc.Start, EventTypeFunc.Currency1);
            }
        }
        #endregion RowStartCurrency1
        #region RowStartCurrency2
        /// <summary>
        /// Obtient l'évènement STA/CU2
        /// </summary>
        public DataRow RowStartCurrency2
        {
            get { return GetRowCurrency(EventCodeFunc.Start, EventTypeFunc.Currency2); }
        }
        #endregion RowStartCurrency2
        #region RowStartCallCurrency
        /// <summary>
        /// Returns the StartCallCurrency TradeEvent of the product
        /// </summary>
        protected DataRow RowStartCallCurrency
        {
            get
            {
                if (null != DsEvents)
                {
                    DataRow[] dataRow = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE = '{3}' and EVENTTYPE = '{4}'",
                        m_DsTrade.IdT.ToString(), m_ParamInstrumentNo.Value, m_ParamStreamNo.Value, EventCodeFunc.Start, EventTypeFunc.CallCurrency), "IDE");
                    if (1 == dataRow.Length)
                        return dataRow[0];
                }
                return null;
            }
        }
        #endregion RowStartCallCurrency
        #region RowStartPutCurrency
        /// <summary>
        /// Returns the StartPutCurrency TradeEvent of the product
        /// </summary>
        protected DataRow RowStartPutCurrency
        {
            get
            {
                if (null != DsEvents)
                {
                    DataRow[] dataRow = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE = '{3}' and EVENTTYPE = '{4}'",
                        m_DsTrade.IdT.ToString(), m_ParamInstrumentNo.Value, m_ParamStreamNo.Value, EventCodeFunc.Start, EventTypeFunc.PutCurrency), "IDE");
                    if (1 == dataRow.Length)
                        return dataRow[0];
                }
                return null;
            }
        }
        #endregion RowStartPutCurrency

        #region RowSettlementCurrency
        /// <summary>
        /// Retourne l'évènement TER/SCU
        /// </summary>
        public DataRow RowSettlementCurrency
        {
            get
            {
                DataRow[] dataRow = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE = '{3}' and EVENTTYPE = '{4}'",
                    m_DsTrade.IdT.ToString(), m_ParamInstrumentNo.Value, m_ParamStreamNo.Value, EventCodeFunc.Termination, EventTypeFunc.SettlementCurrency), "IDE");
                if (1 == dataRow.Length)
                    return dataRow[0];
                return null;
            }
        }
        #endregion RowSettlementCurrency

        #region RowTerminationCurrency1
        /// <summary>
        /// Retourne l'évènement TER/CU1
        /// </summary>
        public DataRow RowTerminationCurrency1
        {
            get { return GetRowCurrency(EventCodeFunc.Termination, EventTypeFunc.Currency1); }
        }
        #endregion RowTerminationCurrency1
        #region RowTerminationCurrency2
        /// <summary>
        /// Retourne l'évènement TER/CU2
        /// </summary>
        public DataRow RowTerminationCurrency2
        {
            get { return GetRowCurrency(EventCodeFunc.Termination, EventTypeFunc.Currency2); }
        }
        #endregion RowTerminationCurrency2
        #region RowInitialMargin
        /// <summary>
        /// Obtient l'évènement LPP/IMG (InitialMargin)
        /// </summary>
        public DataRow RowInitialMargin
        {
            get 
            {
                DataRow[] dataRow = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and EVENTCODE = '{2}' and EVENTTYPE = '{3}'",
                    m_DsTrade.IdT.ToString(), m_ParamInstrumentNo.Value, EventCodeFunc.LinkedProductPayment, EventTypeFunc.InitialMargin), "IDE");
                if (1 == dataRow.Length)
                    return dataRow[0];
                return null;
            }
        }
        #endregion RowInitialMargin
        #region InitialMarginAmount
        /// <summary>
        /// Obtient le montant de l'événement LPP/IMG (InitialMargin)
        /// </summary>
        public Nullable<decimal> InitialMarginAmount
        {
            get
            {
                Nullable<decimal> amount = null;
                DataRow rowInitialMargin = RowInitialMargin;
                if (null != rowInitialMargin)
                    amount = Convert.ToDecimal(rowInitialMargin["VALORISATION"]);
                return amount;
            }
        }
        #endregion InitialMarginAmount

        #region ParamIdT
        protected int ParamIdT
        {
            get { return Convert.ToInt32(m_ParamIdT.Value); }
        }
        #endregion ParamIdT
        #region ParamInstrumentNo
        public int ParamInstrumentNo
        {
            get { return Convert.ToInt32(m_ParamInstrumentNo.Value); }
        }
        #endregion ParamInstrumentNo
        #region ParamStreamNo
        public int ParamStreamNo
        {
            get { return Convert.ToInt32(m_ParamStreamNo.Value); }
        }
        #endregion ParamStreamNo

        /// <summary>
        /// Représente la cotation de l'asset qui active le traitement de valo
        /// </summary>
        public Quote Quote
        {
            get { return m_Quote; }
        }
        public bool IsQuote_SecurityAsset
        {
            get { return m_Quote is Quote_SecurityAsset; }
        }
        public bool IsQuote_Equity
        {
            get { return m_Quote is Quote_Equity; }
        }
        public bool IsQuote_FxRate
        {
            get { return m_Quote is Quote_FxRate; }
        }
        public bool IsQuote_Index
        {
            get { return m_Quote is Quote_Index; }
        }
        public bool IsQuote_RateIndex
        {
            get { return m_Quote is Quote_RateIndex; }
        }
        public bool IsQuote_ETDAsset
        {
            get { return m_Quote is Quote_ETDAsset; }
        }
        // EG 20190925 [24949] New
        public bool IsQuote_DebtSecurity
        {
            get { return m_Quote is Quote_DebtSecurityAsset; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsAccrualProcess
        {
            get
            {
                bool ret = (this.GetType().Namespace == "EFS.Process.Accruals");
                return ret;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public bool IsMarkToMarket
        {
            get
            {
                bool ret = (this.GetType().Namespace == "EFS.Process.MarkToMarket");
                return ret;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public bool IsProvisionProcess
        {
            get
            {
                bool ret = (this.GetType().Namespace == "EFS.Process.Provision");
                return ret;
            }
        }
        
        #endregion Accessors
        //
        #region Constructor
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        public CommonValProcessBase(EfsMLProcessBase pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
        {
            m_Process = pProcess;
            m_CS = pProcess.Cs;
            m_DsTrade = pDsTrade;
            m_tradeLibrary = pTradeLibrary;
            m_CurrentProduct = pProduct;

            // EG 20150612 [20665] See Accessor SlaveDbTransaction
            // EG 20130807 Utilisation du Mode Slave avec une transaction (actuellement TradeActionGen only)
            //IDbTransaction dbTransaction = null;
            //if ((pProcess.processCall == ProcessBase.ProcessCallEnum.Slave) &&
            //    (null != pProcess.SlaveDbTransaction))
            //    dbTransaction = pProcess.SlaveDbTransaction;

            // EG 20150612 [20665] See InitializeDataSetEvent on constructor parent
            //m_DsEvents = new DataSetEventTrade(m_CS, dbTransaction, pProduct, pDsTrade.IdT);
            

            InitParameters();
            m_ParamIdT.Value = pDsTrade.IdT;
            m_ParamIdI.Value = pDsTrade.IdI;

        }
        #endregion Constructor
        //
        #region Methods
        #region InitalizeFeeRequest
        // EG 20170510 [23153] New
        // EG 20190613 [24683] Use slaveDbTransaction
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        protected void InitalizeFeeRequest()
        {
            User user = new User(m_Process.Session.IdA, null, RoleActor.SYSADMIN);
            TradeInput tradeInput = new TradeInput
            {
                DataDocument = m_tradeLibrary.DataDocument
            };
            // Le trade est déjà désérialisé (donc dernier paramètre  = false)
            tradeInput.SearchAndDeserializeShortForm(m_CS, SlaveDbTransaction, m_DsTrade.IdT.ToString(), SQL_TableWithID.IDType.Id, user, m_Process.Session.SessionId, false);
            m_CurrentFeeRequest = new FeeRequest(CSTools.SetCacheOn(m_CS), SlaveDbTransaction, tradeInput, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade));
        }
        #endregion SetFeeRequestForInterestLeg

        #region EndOfInitialize
        // EG 20180502 Analyse du code Correction [CA2214]
        public virtual void EndOfInitialize()
        {
            InitializeDataSetEvent();
        }
        #endregion EndOfInitialize

        #region InitializeDataSetEvent
        /// <summary>
        /// Chargement par défaut de tous les événements d'un trade
        /// sans restriction de table
        /// </summary>
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        public virtual void InitializeDataSetEvent()
        {
            m_DsEvents = new DataSetEventTrade(m_CS, SlaveDbTransaction, m_DsTrade.IdT);
            m_DsEvents.Load();
        }
        #endregion InitializeDataSetEvent

        //
        #region protected AddNewRowEventClass
        /// <summary>
        /// Retourne un nouveau datarow de DtEventClass 
        /// <para>Ce DataRow est ajouté dans DtEventClass</para>
        /// </summary>
        /// <param name="pIdE">Id de l'évènement parent</param>
        /// <param name="pEventClass"></param>
        /// <param name="pDtEvent"></param>
        /// <param name="pIsPayment"></param>
        /// <returns></returns>
        protected DataRow AddNewRowEventClass(int pIdE, string pEventClass, DateTime pDtEvent, Boolean pIsPayment)
        {
            DataRow row = NewRowEventClass(pIdE, pEventClass, pDtEvent, pIsPayment);
            DsEvents.DtEventClass.Rows.Add(row);
            return row;

        }
        #endregion AddNewRowEventClass

        #region NewRowEvent
        /// <summary>
        /// Retourne un nouveau DataRow de la table DtEvent
        /// </summary>
        /// <param name="pRowSource"></param>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType"></param>
        /// <param name="pEventDate"></param>
        /// <returns></returns>
        /// FI 20141215 [20570] Modify Method
        /// EG 20150317 [POC] 
        //protected DataRow NewRowEvent(DataRow pRowSource, string pEventCode, string pEventType, DateTime pEventDate, AppInstanceService pAppInstance)
        //{
        //    // FI 20141215 [20570] Appel avec pDtEnd == pDtStart == pEventDate
        //    return NewRowEvent(pRowSource, pEventCode, pEventType, pEventDate, pEventDate, pAppInstance);
        //}

        /// <summary>
        /// Retourne un nouveau DataRow de la table DtEvent
        /// </summary>
        /// <param name="pRowSource"></param>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType"></param>
        /// <param name="pDtStart"></param>
        /// <param name="pDtEnd"></param>
        /// <returns></returns>
        /// FI 20141215 [20570] Add Method
        /// EG 20150317 [POC] 
        // EG 20160106 [21679]POC-MUREX Add dbTransaction
        protected DataRow NewRowEvent(IDbTransaction pDbTransaction, DataRow pRowSource, string pEventCode, string pEventType, DateTime pEventDate, AppInstanceService pAppInstance)
        {
            // FI 20141215 [20570] Appel avec pDtEnd == pDtStart == pEventDate
            return NewRowEvent(pDbTransaction, pRowSource, pEventCode, pEventType, pEventDate, pEventDate, pAppInstance);
        }
        protected DataRow NewRowEvent(IDbTransaction pDbTransaction, DataRow pRowSource, string pEventCode, string pEventType, DateTime pDtStart, DateTime pDtEnd, AppInstanceService pAppInstance)
        {
            int newIdE;
            if (null != pDbTransaction)
                SQLUP.GetId(out newIdE, pDbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
            else
                SQLUP.GetId(out newIdE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);

            DataRow newRow = m_DsEvents.DtEvent.NewRow();
            newRow.ItemArray = (object[])pRowSource.ItemArray.Clone();
            newRow.BeginEdit();
            newRow["IDE"] = newIdE;
            newRow["IDE_EVENT"] = pRowSource["IDE"];
            newRow["EVENTCODE"] = pEventCode;
            newRow["EVENTTYPE"] = pEventType;
            newRow["SOURCE"] = pAppInstance.ServiceName;

            newRow["IDSTTRIGGER"] = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
            newRow["DTSTARTADJ"] = pDtStart.Date;
            newRow["DTSTARTUNADJ"] = pDtStart.Date;
            newRow["DTENDADJ"] = pDtEnd.Date;
            newRow["DTENDUNADJ"] = pDtEnd.Date;
            newRow.EndEdit();

            SetRowStatus(newRow, TuningOutputTypeEnum.OES);
            return newRow;
        }
        #endregion NewRowEvent
        #region NewRowEvent2
        /// <summary>
        /// Insertion d'un nouvel événement.
        /// l'IdE est initialisé à -1, il sera mis à jour ultérieurement
        /// </summary>
        /// EG 20160208 [POC-MUREX] 
        protected DataRow NewRowEvent2(DataRow pRowSource, string pEventCode, string pEventType, DateTime pDtStart, DateTime pDtEnd, AppInstanceService pAppInstance)
        {
            DataRow newRow = m_DsEvents.DtEvent.NewRow();
            newRow.ItemArray = (object[])pRowSource.ItemArray.Clone();
            newRow.BeginEdit();
            newRow["IDE"] = -1;
            newRow["IDE_EVENT"] = pRowSource["IDE"];
            newRow["EVENTCODE"] = pEventCode;
            newRow["EVENTTYPE"] = pEventType;
            newRow["SOURCE"] = pAppInstance.ServiceName;

            newRow["IDSTTRIGGER"] = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
            newRow["DTSTARTADJ"] = pDtStart.Date;
            newRow["DTSTARTUNADJ"] = pDtStart.Date;
            newRow["DTENDADJ"] = pDtEnd.Date;
            newRow["DTENDUNADJ"] = pDtEnd.Date;
            newRow.EndEdit();

            SetRowStatus(newRow, TuningOutputTypeEnum.OES);
            return newRow;
        }
        #endregion NewRowEvent2

        #region NewRowEventDet
        /// <summary>
        /// Retourne un nouveau DataRow de la table DtEventDetails (EVENTDET)
        /// </summary>
        /// <param name="pRowSource"></param>
        /// <returns></returns>
        protected DataRow NewRowEventDet(DataRow pRowSource)
        {
            DataRow newRow = m_DsEvents.DtEventDet.NewRow();
            newRow.BeginEdit();
            newRow["IDE"] = Convert.ToInt32(pRowSource["IDE"]);
            newRow.EndEdit();
            return newRow;
        }
        #endregion NewRowEventDet
        #region protected NewRowEventClass
        /// <summary>
        /// Retourne un nouveau datarow du datatable DtEventClass
        /// </summary>
        /// <param name="pIdE">Id de l'évènement parent</param>
        /// <param name="pEventClass"></param>
        /// <param name="pDtEvent"></param>
        /// <param name="pIsPayment"></param>
        /// <returns></returns>
        // EG 20180503 CacheOn
        // EG 20190613 [24683] Use slaveDbTransaction
        // EG 20190823 [FIXEDINCOME] Upd (Dataset parameter)
        protected DataRow NewRowEventClass(int pIdE, string pEventClass, DateTime pDtEvent, Boolean pIsPayment)
        {
            return NewRowEventClass(DsEvents, pIdE, pEventClass, pDtEvent, pIsPayment);
        }
        // EG 20190823 [FIXEDINCOME] Upd (Dataset parameter)
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        protected DataRow NewRowEventClass(DataSetEventTrade pDataSet, int pIdE, string pEventClass, DateTime pDtEvent, Boolean pIsPayment)
        {
            DataRow row = pDataSet.DtEventClass.NewRow();
            row.BeginEdit();
            row["IDE"] = pIdE;
            row["EVENTCLASS"] = pEventClass;
            row["DTEVENT"] = pDtEvent;
            row["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(CSTools.SetCacheOn(m_CS), SlaveDbTransaction, pDtEvent);
            row["ISPAYMENT"] = pIsPayment;
            row.EndEdit();
            return row;
        }

        #endregion NewRowEventClass
        #region protected NewRowEventAsset
        /// <summary>
        /// Retourne un nouveau datarow du datatable DtEventAsset
        /// </summary>
        /// <param name="pIdE">Id de l'évènement parent</param>
        /// <param name="pEventClass"></param>
        /// <param name="pDtEvent"></param>
        /// <param name="pIsPayment"></param>
        /// <returns></returns>
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        protected DataRow NewRowEventAsset(DataRow pRowSource, int pIdE)
        {

            DataRow row = DsEvents.DtEventAsset.NewRow();
            row.ItemArray = (object[])pRowSource.ItemArray.Clone();
            row.BeginEdit();
            row["IDE"] = pIdE;
            row.EndEdit();
            return row;

        }
        #endregion NewRowEventClass

        //
        #region public AddRowAccountingEventClass;
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public void AddRowAccountingEventClass(int pIdE)
        {
            
                EFS_AccountingEventClass.AddRowAccountingEventClass(m_CS,
                    ProductBase, pIdE, Convert.ToInt32(m_ParamIdI.Value), CommonValDate, DsEvents.DtEventClass, m_tradeLibrary.DataDocument);
            
        }
        #endregion AddRowAccountingEventClass;
        #region public DeleteAllRowEventClass
        /// <summary>
        /// Supprime tous les lignes dans EVENTCLASS d'un évènement
        /// </summary>
        /// <param name="pIdE"></param>
        public void DeleteAllRowEventClass()
        {
            
                if ((null != DsEvents.DtEventClass) && (null != m_ParamIdE))
                {
                    DataRow[] rows = DsEvents.DtEventClass.Select("IDE=" + m_ParamIdE.Value.ToString(), "IDE");
                    if (null != rows)
                    {
                        foreach (DataRow row in rows)
                            row.Delete();
                    }
                }
            
        }
        /// <summary>
        /// Supprime tous les lignes dans EVENTCLASS d'un évènement
        /// </summary>
        /// <param name="pIdE"></param>
        public void DeleteAllRowEventClass(int pIdE)
        {
                if ((null != DsEvents.DtEventClass) && (pIdE > 0))
                {
                    DataRow[] rows = DsEvents.DtEventClass.Select("IDE=" + pIdE.ToString(), "IDE");
                    if (ArrFunc.IsFilled(rows))
                    {
                        foreach (DataRow row in rows)
                            row.Delete();
                    }
                }
            
        }
        #endregion DeleteAllRowEventClass

        #region EventCodeLink
        /// <summary>
        /// Retourne LPI (prix intraday),LPP (prix close ou théorique) ou LPC
        /// </summary>
        /// <param name="pEventType"></param>
        /// <returns></returns>
        /// EG 20130617 Prise en compte LPC/LPP
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        protected string EventCodeLink(string pEventTypeParent, string pEventType, QuoteTimingEnum pQuoteTiming)
        {
            string eventCode;
            if (QuoteTimingEnum.Intraday == pQuoteTiming)
                eventCode = EventCodeFunc.LinkedProductIntraday;
            else if ((QuoteTimingEnum.Close == pQuoteTiming) && EventTypeFunc.IsVariationMargin(pEventType))
                eventCode = (EventTypeFunc.IsAmounts(pEventTypeParent) ? EventCodeFunc.LinkedProductPayment : EventCodeFunc.LinkedProductClosing);
            else if ((QuoteTimingEnum.Close == pQuoteTiming) && EventTypeFunc.IsSettlementCurrency(pEventType))
                eventCode = (EventTypeFunc.IsPartiel(pEventTypeParent) ? EventCodeFunc.Intermediary : EventCodeFunc.Termination);
            else
                eventCode = EventCodeFunc.LinkedProductClosing;
            return eventCode;
        }
        #endregion EventCodeLink

        #region GetAmount
        // EG 20150219 Return Nullable<decimal> en lieu et place de decimal
        public Nullable<decimal> GetAmount(DateTime pDate, string pEventType)
        {
            Nullable<decimal> _amount = null;
            DataRow[] _rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and EVENTTYPE = '{2}' and DTSTARTADJ <= '{3}' and '{3}' <= DTENDADJ",
            m_DsTrade.IdT.ToString(), StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), pEventType, DtFunc.DateTimeToStringDateISO(pDate)));
            if (ArrFunc.IsFilled(_rows))
            {
                if (false == Convert.IsDBNull(_rows.First()["VALORISATION"]))
                    _amount = Convert.ToDecimal(_rows.First()["VALORISATION"]);
            }
            return _amount;
        }
        #endregion GetAmount

        #region public GetRowCurrentNominalReference
        public DataRow GetRowCurrentNominalReference(DateTime pDate)
        {
            
                foreach (DataRow rowNominal in RowNominalReference)
                {
                    if ((Convert.ToDateTime(rowNominal["DTSTARTADJ"]) <= pDate) && (pDate < Convert.ToDateTime(rowNominal["DTENDADJ"])))
                        return rowNominal;
                }
                //
                int nbRow = RowNominalReference.Length;
                if (0 < nbRow)
                {
                    DataRow rowTerminationNominal = RowNominalReference[nbRow - 1];
                    if (pDate == Convert.ToDateTime(rowTerminationNominal["DTENDADJ"]))
                        return rowTerminationNominal;
                }
                //
                return null;
            
        }
        #endregion GetRowCurrentNominalReference
        #region public GetCurrentNominal
        // 20071008 EG Tocket : 15830 (CurrentNominal find by only one type date (adjusted and unadjusted) and not all)
        public DataRow GetCurrentNominal(DateTime pDate)
        {
            return GetCurrentNominal(pDate, true);
        }
        // 20071008 EG Tocket : 15830 (CurrentNominal find by only one type date (adjusted and unadjusted) and not all)
        public DataRow GetCurrentNominal(DateTime pDate, bool pIsDtAdjusted)
        {
            DataRow[] rowsNominal = GetRowNominalStep();
            int nbRow = ArrFunc.Count(rowsNominal);
            //
            if (ArrFunc.IsFilled(rowsNominal))
            {
                foreach (DataRow rowNominal in rowsNominal)
                {
                    if (pIsDtAdjusted &&
                        (Convert.ToDateTime(rowNominal["DTSTARTADJ"]) <= pDate) && (pDate < Convert.ToDateTime(rowNominal["DTENDADJ"])))
                        return rowNominal;
                    else if ((false == pIsDtAdjusted) &&
                        (Convert.ToDateTime(rowNominal["DTSTARTUNADJ"]) <= pDate) && (pDate < Convert.ToDateTime(rowNominal["DTENDUNADJ"])))
                        return rowNominal;
                }
            }
            if (0 < nbRow)
            {
                DataRow rowTerminationNominal = rowsNominal[nbRow - 1];
                if (pDate == Convert.ToDateTime(rowTerminationNominal["DTENDADJ"]))
                    return rowTerminationNominal;
            }
            return null;
        }
        // 20071008 EG Tocket : 15830 (CurrentNominal find by only one type date (adjusted and unadjusted) and not all)
        public DataRow GetCurrentNominal(DateTime pDate, DateTime pDateUnadjusted)
        {
            DataRow rowNominal = GetCurrentNominal(pDate);
            if (null == rowNominal)
                rowNominal = GetCurrentNominal(pDateUnadjusted, false);
            return rowNominal;
        }
        #endregion GetCurrentNominal
        #region public GetCurrentQuantity
        public DataRow GetCurrentQuantity(DateTime pDate)
        {
            return GetCurrentQuantity(pDate, true);
        }
        public DataRow GetCurrentQuantity(DateTime pDate, bool pIsDtAdjusted)
        {
            DataRow[] rowsQuantity = GetRowQuantity();
            int nbRow = rowsQuantity.Length;
            foreach (DataRow rowQuantity in rowsQuantity)
            {
                if (pIsDtAdjusted &&
                    (Convert.ToDateTime(rowQuantity["DTSTARTADJ"]) <= pDate) && (pDate < Convert.ToDateTime(rowQuantity["DTENDADJ"])))
                    return rowQuantity;
                else if ((false == pIsDtAdjusted) &&
                    (Convert.ToDateTime(rowQuantity["DTSTARTUNADJ"]) <= pDate) && (pDate < Convert.ToDateTime(rowQuantity["DTENDUNADJ"])))
                    return rowQuantity;
            }
            if (0 < nbRow)
            {
                DataRow rowTerminationQuantity = rowsQuantity[nbRow - 1];
                if (pDate == Convert.ToDateTime(rowTerminationQuantity["DTENDADJ"]))
                    return rowTerminationQuantity;
            }
            return null;
        }
        public DataRow GetCurrentQuantity(DateTime pDate, DateTime pDateUnadjusted)
        {
            DataRow rowQuantity = GetCurrentQuantity(pDate);
            if (null == rowQuantity)
                rowQuantity = GetCurrentQuantity(pDateUnadjusted, false);
            return rowQuantity;
        }
        #endregion GetCurrentQuantity
        #region GetCurrentMarginRatio
        // EG 20150305 New
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        public DataRow GetCurrentMarginRatio(DateTime pDate)
        {
            DataRow[] rowsMarginRatio = GetRowMarginRatio();
            _ = rowsMarginRatio.Length;
            foreach (DataRow rowMarginRatio in rowsMarginRatio)
            {
                if ((Convert.ToDateTime(rowMarginRatio["DTSTARTADJ"]) <= pDate) && (pDate < Convert.ToDateTime(rowMarginRatio["DTENDADJ"])))
                    return rowMarginRatio;
            }
            return null;
        }
        #endregion GetCurrentMarginRatio

        #region public GetPreviousNominal
        public DataRow GetPreviousNominal(DataRow pRowNominal)
        {
            DataRow[] rowsNominal = GetRowNominalStep();
            if (ArrFunc.IsFilled(rowsNominal))
            {
                for (int i = 0; i < rowsNominal.Length; i++)
                {
                    if (rowsNominal[i].Equals(pRowNominal))
                    {
                        if (0 == i)
                            return rowsNominal[i];
                        else
                            return rowsNominal[i - 1];
                    }
                }
            }
            return null;
        }
        #endregion GetCurrentNominal
        #region public GetRowAllVariationNominal
        public DataRow[] GetRowAllVariationNominal()
        {
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and EVENTTYPE = '{2}' and EVENTCODE <> '{3}'",
            m_DsTrade.IdT.ToString(), StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), EventTypeFunc.Nominal, EventCodeFunc.NominalStep), "DTSTARTADJ");
        }
        #endregion GetRowAllVariationNominal
        #region public GetRowAsset
        public DataRow[] GetRowAsset(int pIdE)
        {
            DataRow[] ret = null;
            if (null != m_DsEvents.DtEventAsset)
            {
                DataRow[] dataRow = m_DsEvents.DtEventAsset.Select("IDE=" + pIdE.ToString(), "IDE");
                if (ArrFunc.IsFilled(dataRow))
                    ret = dataRow;
            }
            return ret;

        }
        #endregion GetRowAsset
        #region public GetRowDetail
        public DataRow GetRowDetail(int pIdE)
        {
            try
            {
                DataRow ret = null;
                if (null != m_DsEvents.DtEventDet)
                {
                    DataRow[] dataRow = m_DsEvents.DtEventDet.Select("IDE=" + pIdE.ToString(), "IDE");
                    if (1 == ArrFunc.Count(dataRow))
                        ret = dataRow[0];
                }
                return ret;
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion GetRowDetail
        #region public GetRowFee
        public DataRow GetRowFee(int pIdE)
        {
            try
            {
                DataRow ret = null;
                if (null != m_DsEvents.DtEventFee)
                {
                    DataRow[] dataRow = m_DsEvents.DtEventFee.Select("IDE=" + pIdE.ToString(), "IDE");
                    if (1 == ArrFunc.Count(dataRow))
                        ret = dataRow[0];
                }
                return ret;
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion GetRowFee
        #region public GetRowEvent
        /// <summary>
        /// Obtient le ligne de la table EVENT tel que IDE= {pIdE}
        /// <para>Obtient null si non trouvée</para>
        /// </summary>
        /// <param name="pID"></param>
        /// <returns></returns>
        public DataRow GetRowEvent(int pIdE)
        {
            DataRow ret = null;
            if (null != DsEvents.DtEvent)
            {
                DataRow[] dataRow = DsEvents.DtEvent.Select("IDE=" + pIdE.ToString(), "IDE");
                if (1 == ArrFunc.Count(dataRow))
                    ret = dataRow[0];
            }
            return ret;
        }
        #endregion GetRowEvent
        #region public GetRowEventBySource
        /// <summary>
        /// Obtient le ligne de la table EVENT tel que IDE_SOURCE= {pIdE}
        /// <para>Obtient null si non trouvée</para>
        /// </summary>
        /// <param name="pID"></param>
        /// <returns></returns>
        public DataRow GetRowEventBySource(int pIdE)
        {
            DataRow ret = null;
            if (null != DsEvents.DtEvent)
            {
                DataRow[] rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and IDE_SOURCE = {1}", m_DsTrade.IdT.ToString(), pIdE.ToString()), "IDE");
                if (1 == ArrFunc.Count(rows))
                    ret = rows[0];
            }
            return ret;
        }
        #endregion GetRowEventBySource
        #region public GetRowEventProducts
        /// <summary>
        /// Retourne les évènements tels que EVENTCODE 'PRD' et EVENTTYPE 'DAT' triées selon IDE
        /// </summary>
        /// <returns></returns>
        protected DataRow[] GetRowEventProducts()
        {
            DataRow[] rows = null;
            if (null != DsEvents)
            {
                rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and EVENTCODE = '{2}' and EVENTTYPE = '{3}'",
                    m_DsTrade.IdT.ToString(), StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), EventCodeFunc.Product, EventTypeFunc.Date), "IDE");
            }
            return rows;
        }
        #endregion GetRowEventProducts

        #region GetRowAmount
        /// <summary>
        /// Obtient l'évènement enfant de l'évènement de regroupement des montants (LPC,AMT) tel que DTSTARTUNADJ est identique à la date de l'asset
        /// </summary>
        /// <param name="pEventType"></param>
        /// <returns></returns>
        /// EG 20210415 [25584][25702] Filtre sur le statut des EVTs (on exclus les DEACTIV)
        protected DataRow GetRowAmount(DataRow pRowParent, string pEventType, QuoteTimingEnum pQuoteTiming, DateTime pDate)
        {
            DataRow ret = null;
            if (null != pRowParent)
            {
                string eventCodeLink = EventCodeLink(pRowParent["EVENTTYPE"].ToString(), pEventType, pQuoteTiming);
                DataRow[] rowChilds = pRowParent.GetChildRows(m_DsEvents.ChildEvent);
                foreach (DataRow rowChild in rowChilds)
                {
                    if ((eventCodeLink == rowChild["EVENTCODE"].ToString()) &&
                        (pEventType == rowChild["EVENTTYPE"].ToString()) &&
                        (Cst.StatusActivation.DEACTIV.ToString() != rowChild["IDSTACTIVATION"].ToString()) &&
                        (pDate == Convert.ToDateTime(rowChild["DTSTARTUNADJ"])))
                    {
                        ret = rowChild;
                        break;
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// Obtient l'évènement enfant de l'évènement de regroupement des montants (LPC,AMT) tel que DTSTARTUNADJ est identique à la date de l'asset
        /// </summary>
        /// <param name="pEventType"></param>
        /// <returns></returns>
        protected DataRow GetRowAmount(string pEventType, QuoteTimingEnum pQuoteTiming, DateTime pDate)
        {
            DataRow rowAmount = GetRowAmountGroup();
            return GetRowAmount(rowAmount, pEventType, pQuoteTiming, pDate);
        }
        protected DataRow GetRowAmount(string pEventType, string pEventClass, QuoteTimingEnum pQuoteTiming, DateTime pDate)
        {
            DataRow rowAmount = GetRowAmountGroup();
            return GetRowAmount(rowAmount, pEventType, pEventClass, pQuoteTiming, pDate);
        }
        protected DataRow GetRowAmount(DataRow pRowParent, string pEventType, string pEventClass, QuoteTimingEnum pQuoteTiming, DateTime pDate)
        {
            DataRow ret = null;
            if (null != pRowParent)
            {
                string eventCodeLink = EventCodeLink(pRowParent["EVENTTYPE"].ToString(), pEventType, pQuoteTiming);
                DataRow[] rowChilds = pRowParent.GetChildRows(m_DsEvents.ChildEvent);
                foreach (DataRow rowChild in rowChilds)
                {
                    if ((eventCodeLink == rowChild["EVENTCODE"].ToString()) &&
                        (pEventType == rowChild["EVENTTYPE"].ToString()))
                    {
                        DataRow[] rowEventsClass = rowChild.GetChildRows(DsEvents.ChildEventClass);
                        if (ArrFunc.IsFilled(rowEventsClass))
                        {
                            foreach (DataRow rowEventClass in rowEventsClass)
                            {
                                if ((rowEventClass["EVENTCLASS"].ToString() == pEventClass) &&
                                    (pDate == Convert.ToDateTime(rowEventClass["DTEVENT"])))
                                {
                                    ret = rowChild;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }
        #endregion GetRowAmount
        #region GetRowAmountsGroup
        protected DataRow[] GetRowAmountsGroup()
        {
            DataRow[] rows = null;
            if (null != DsEvents)
            {
                rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and EVENTTYPE = '{2}' and EVENTCODE = '{3}'",
                m_DsTrade.IdT.ToString(), StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), EventTypeFunc.Amounts, EventCodeFunc.LinkedProductClosing), "IDE");
            }
            return rows;
        }
        #endregion GetRowAmountsGroup
        #region GetRowAmountGroup
        /// <summary>
        /// Retourne la ligne de la table EVENT tel que {EVENTCODE:LPC et EVENTYPE:AMT}
        /// <para>Retourne null si la ligne n'existe pas</para>
        /// </summary>
        /// <returns></returns>
        protected DataRow GetRowAmountGroup()
        {
            DataRow ret = null;
            DataRow[] rows = GetRowAmountsGroup();
            if (ArrFunc.IsFilled(rows))
                ret = rows[0];
            return ret;
        }
        #endregion GetRowAmountGroup

        #region public GetRowEventByEventCode
        /// <summary>
        /// Retourne les évènements tels que EVENTCODE = {pEventCode} associés au stream {pStreamNo} triées selon ID
        /// <para>Retourne null si non trouvé</para>
        /// </summary>
        /// <param name="pCode"></param>
        /// <param name="pStreamNo"></param>
        /// <returns></returns>
        public DataRow[] GetRowEventByEventCode(string pEventCode, int pStreamNo)
        {
            DataRow[] ret = null;
            DataRow[] rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE = '{3}'",
                m_DsTrade.IdT.ToString(), StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), (pStreamNo > -1 ? pStreamNo.ToString() : "STREAMNO"), pEventCode), "IDE");
            if (ArrFunc.IsFilled(rows))
                ret = rows;
            return ret;
        }
        /// <summary>
        /// Retourne les évènements tels que EVENTCODE = {pCode}, tous streams confondus, triées selon ID
        /// <para>Retourne null si non trouvé</para>
        /// </summary>
        /// <param name="pCode"></param>
        /// <returns></returns>
        public DataRow[] GetRowEventByEventCode(string pCode)
        {
            return GetRowEventByEventCode(pCode, -1);
        }
        #endregion
        #region public GetRowEventByEventType
        /// <summary>
        /// Retourne les évènements tels que EVENTTYPE = {pEventType} associés au stream {pStreamNo}, triées selon ID
        /// <para>Retourne null si non trouvé</para>
        /// </summary>
        /// <param name="pEventType"></param>
        /// <param name="pStreamNo"></param>
        /// <returns></returns>
        // EG 20231127 [WI755] Implementation Return Swap : Upd
        public DataRow[] GetRowEventByEventType(string pEventType, int pStreamNo)
        {
            DataRow[] ret = null;
            int instrumentNo = StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id);
            string streamNo = (pStreamNo > -1 ? pStreamNo.ToString() : "STREAMNO");
            DataRow[] rows = DsEvents.DtEvent.Select($@"IDT = {m_DsTrade.IdT} and INSTRUMENTNO = {instrumentNo} and STREAMNO = {streamNo} and EVENTTYPE = '{pEventType}'", "IDE");
            if (ArrFunc.IsFilled(rows))
                ret = rows;
            return ret;
        }
        /// <summary>
        /// Retourne les évènements tels que EVENTTYPE = {pEventType}, tous streams confondus, triées selon ID
        /// <para>Retourne null si non trouvé</para>
        /// </summary>
        /// <param name="pEventType"></param>
        /// <returns></returns>
        public DataRow[] GetRowEventByEventType(string pEventType)
        {
            return GetRowEventByEventType(pEventType,-1);
        }

        #endregion GetRowEventByEventType
        #region public GetRowAdditionalPayment
        /// <summary>
        /// Retourne toutes les évènements tels que EVENTCODE = 'ADP',  triées selon ID
        /// <para>Retourne null si non trouvée</para>
        /// </summary>
        /// <returns></returns>
        public DataRow[] GetRowAdditionalPayment()
        {
            return GetRowEventByEventCode(EventCodeFunc.AdditionalPayment.ToString(),0);   
        }
        #endregion
        #region public GetRowInterestRateStream
        /// <summary>
        /// Retourne tous les évènements tels que EVENTCODE = 'IRS'  triées selon ID
        /// <para>Retourne null si non trové</para>
        /// </summary>
        /// <returns></returns>
        public DataRow[] GetRowInterestRateStream()
        {
            return GetRowEventByEventCode(EventCodeEnum.IRS.ToString(),-1); 
        }
        #endregion GetRowInterestRateStream
        #region public GetRowInterest
        /// <summary>
        /// Retourne tous les évènements tels que EVENTTYPE = 'INT' triées selon ID
        /// </summary>
        /// <returns></returns>
        public DataRow[] GetRowInterest()
        {
            return GetRowInterest(-1);
        }
        /// <summary>
        /// Retourne tous les évènements tels que EVENTTYPE = 'INT' rattachées au stream {pStreamNo} triées selon ID
        /// </summary>
        /// <param name="pStreamNo"></param>
        /// <returns></returns>
        protected DataRow[] GetRowInterest(int pStreamNo)
        {
            return GetRowEventByEventType(EventTypeFunc.Interest.ToString(), pStreamNo);
        }
        #endregion GetRowInterest
        // EG 20231127 [WI755] Implementation Return Swap : New
        #region public GetRowReturnLegAmount
        public DataRow[] GetRowReturnLegAmount()
        {
            return GetRowReturnLegAmount(-1);
        }
        // EG 20231127 [WI749] Implementation Return Swap : New
        protected DataRow[] GetRowReturnLegAmount(int pStreamNo)
        {
            return GetRowEventByEventType(EventTypeFunc.ReturnLegAmount.ToString(), pStreamNo);
        }
        #endregion GetRowReturnLegAmount
        #region public GetRowEventClass
        /// <summary>
        /// Retourne les lignes EVENTCLASS assocées à l'évènement {pID} triées selon ID
        /// <para>Retourne null si non trouvée</para>
        /// </summary>
        public DataRow[] GetRowEventClass(int pID)
        {
            DataRow[] rows = null;
            if (null != DsEvents.DtEventClass)
            {
                rows = DsEvents.DtEventClass.Select("IDE=" + pID.ToString(), "IDE");
                if (ArrFunc.IsEmpty(rows))
                    rows = null;
            }
            return rows;
        }
        /// <summary>
        /// Retourne les lignes EVENTCLASS assocées à l'évènement {pID}
        /// <para>Retourne null si non trouvée</para>
        /// </summary>
        /// <param name="pID"></param>
        /// <param name="pEventClass"></param>
        /// <returns></returns>
        /// EG 20150608 DtEventClass
        public DataRow GetRowEventClass(int pIdE, string pEventClass)
        {
            DataRow row = null;
            if (null != DsEvents.DtEventClass)
            {
                DataRow[] rows = DsEvents.DtEventClass.Select(StrFunc.AppendFormat(@"IDE = {0} and EVENTCLASS = '{1}'", pIdE, pEventClass), "IDE");
                if (ArrFunc.IsFilled(rows))
                    row = rows[0];
            }
            return row;
        }
        #endregion GetRowEventClass
        #region public GetRowEventDetail
        /// <summary>
        /// Retourne la ligne de EVENTDET associée à l'évènement {pIdE} 
        /// <para>Retourne null si non trouvée</para>
        /// </summary>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        public DataRow GetRowEventDetail(int pIdE)
        {
            DataRow ret = null;
            if (null != m_DsEvents.DtEventDet)
            {
                DataRow[] dataRow = m_DsEvents.DtEventDet.Select("IDE=" + pIdE.ToString(), "IDE");
                if (ArrFunc.IsFilled(dataRow))
                    ret = dataRow[0];
            }
            return ret;
        }
        #endregion RowEventDetail


        #region public GetRowParentWithPayerReceiver
        public DataRow GetRowParentWithPayerReceiver(DataRow pRow)
        {
            if ((null != pRow) && Convert.IsDBNull(pRow["IDA_PAY"]) && Convert.IsDBNull(pRow["IDA_REC"]))
            {
                if (EventCodeFunc.IsTermination(pRow["EVENTCODE"].ToString()) && EventTypeFunc.IsSettlementCurrency(pRow["EVENTTYPE"].ToString()))
                {
                    DataRow drDetail = GetRowEventDetail(Convert.ToInt32(pRow["IDE"]));
                    if (null != drDetail)
                    {
                        DataRow rowParent = pRow.GetParentRow(DsEvents.ChildEvent);
                        string referencCurrency = drDetail["IDC_REF"].ToString();
                        foreach (DataRow dr in rowParent.GetChildRows(DsEvents.ChildEvent))
                        {
                            if (EventCodeFunc.IsStart(dr["EVENTCODE"].ToString()) && (referencCurrency == dr["UNIT"].ToString()))
                                return dr;
                        }
                    }
                }
                else
                {
                    DataRow rowFounded = pRow.GetParentRow(m_DsEvents.ChildEvent);
                    if (null != rowFounded)
                        return GetRowParentWithPayerReceiver(rowFounded);
                }
            }
            return pRow;
        }
        #endregion GetRowParentWithPayerReceiver

        #region public GetRowEventParentRatio
        /// <summary>
        /// Retourne Le DataRow Parent d'un MarginRatioFactor en fonction du produit/instrument
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pSource"></param>
        /// <returns></returns>
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        public DataRow GetRowEventParentRatio<T>(Nullable<int> pIdEParent, T pSource)
        {
            DataRow row = null;
            if (pIdEParent.HasValue)
            {
                row = GetRowEvent(pIdEParent.Value);
            }
            else
            {
                if (pSource is FxLegContainer)
                {
                    DataRow[] rows = GetRowEventByEventCode(EventCodeFunc.FxForward);
                    if (ArrFunc.IsFilled(rows))
                        row = rows[0];
                }
                else if (pSource is FxOptionLegContainer)
                {
                    if (pSource is FxOptionLegContainer fxol)
                    {
                        DataRow[] rows = null;
                        switch (fxol.FxOptionLeg.ExerciseStyle)
                        {
                            case ExerciseStyleEnum.American:
                                rows = GetRowEventByEventCode(EventCodeFunc.AmericanSimpleOption);
                                break;
                            case ExerciseStyleEnum.Bermuda:
                                rows = GetRowEventByEventCode(EventCodeFunc.BermudaSimpleOption);
                                break;
                            case ExerciseStyleEnum.European:
                                rows = GetRowEventByEventCode(EventCodeFunc.EuropeanSimpleOption);
                                break;
                        }
                        if (ArrFunc.IsFilled(rows))
                            row = rows[0];
                    }
                }
                else if (pSource is EquitySecurityTransactionContainer)
                {
                    DataRow[] rows = GetRowEventByEventCode(EventCodeFunc.EquitySecurityTransaction);
                    if (ArrFunc.IsFilled(rows))
                        row = rows[0];
                }
                else if (pSource is ReturnSwapContainer)
                {
                    if (pSource is ReturnSwapContainer rts)
                    {
                        DataRow[] rows = GetRowEventByEventCode(rts.ReturnLeg.Underlyer.UnderlyerSingleSpecified ? EventCodeFunc.SingleUnderlyer : EventCodeFunc.Basket);
                        if (ArrFunc.IsFilled(rows))
                            row = rows[0];
                    }
                }
            }
            return row;
        }
        #endregion GetRowEventParentRatio


        #region public GetRowCurrency
        public DataRow GetRowCurrency(string pEventCode, string pEventType)
        {
            DataRow[] dataRow = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE = '{3}' and EVENTTYPE = '{4}'",
            m_DsTrade.IdT.ToString(), StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), m_ParamStreamNo.Value, pEventCode, pEventType), "IDE");
            if (1 == dataRow.Length)
                return dataRow[0];
            return null;
        }
        #endregion GetRowCurrency
        #region public GetRowMarginRatio
        /// EG 20150306 [POC-BERKELEY] : New
        public DataRow[] GetRowMarginRatio()
        {
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTTYPE = '{3}'",
            m_DsTrade.IdT.ToString(), StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), m_ParamStreamNo.Value, EventTypeFunc.MarginRequirementRatio), "DTSTARTADJ");
        }
        #endregion GetRowMarginRatio
        #region public GetRowNominalStep
        /// <summary>
        /// Retourne toutes les lignes NOS/NOM triées selon DTSTARTADJ
        /// <para>Retourne null si non trouvée</para>
        /// </summary>
        /// <returns></returns>
        // EG 20231127 [WI755] Implementation Return Swap : Upd && Surcharge
        public DataRow[] GetRowNominalStep()
        {
            return GetRowNominalStep(Convert.ToInt32(m_ParamStreamNo.Value));
        }
        public DataRow[] GetRowNominalStep(int pStreamNo)
        {
            DataRow[] ret = null;
            int instrumentNo = StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id);
            DataRow[] rows = DsEvents.DtEvent.Select($@"IDT = {m_DsTrade.IdT} and INSTRUMENTNO = {instrumentNo} and STREAMNO = {pStreamNo} and EVENTCODE = '{EventCodeFunc.NominalStep}'and EVENTTYPE = '{EventTypeFunc.Nominal}'", "DTSTARTADJ");
            if (ArrFunc.IsFilled(rows))
                ret = rows;
            return ret;
        }
        /// <summary>
        /// Retourne la ligne NOS/NOM tel que  DTENDADJ = {pDtEnd}
        /// <para>Retourne null si non trouvée</para>
        /// </summary>
        public DataRow GetRowNominalStep(DateTime pDtEnd)
        {
            DataRow[] rows = GetRowNominalStep();
            if (ArrFunc.IsFilled(rows))
            {
                foreach (DataRow row in rows)
                {
                    if (pDtEnd == Convert.ToDateTime(row["DTENDADJ"]))
                        return row;
                }
            }
            return null;
        }
        /// <summary>
        /// Retourne la ligne NOS/NOM tel que  DTSTARTADJ ={pDtStart} et DTENDADJ = {pDtEnd}
        /// <para>Retourne null si non trouvée</para>
        /// </summary>
        public DataRow GetRowNominalStep(DateTime pDtStart, DateTime pDtEnd)
        {

            DataRow[] rows = GetRowNominalStep();
            if (ArrFunc.IsFilled(rows))
            {
                foreach (DataRow row in rows)
                {
                    if ((pDtStart == Convert.ToDateTime(row["DTSTARTADJ"])) && (pDtEnd == Convert.ToDateTime(row["DTENDADJ"])))
                        return row;
                }
            }
            return null;
        }
        #endregion GetRowNominalStep
        #region public GetRowNominal
        /// <summary>
        /// Retourne toutes les lignes de EVENTTYPE = 'NOM' triées selon ID 
        /// </summary>
        /// <returns></returns>
        public DataRow[] GetRowNominal()
        {
            return GetRowNominal(-1);
        }
        /// <summary>
        /// Obtient toutes les lignes de EVENTTYPE = 'NOM' triées selon ID rattachées au stream {pStreamNo}
        /// </summary>
        /// <returns></returns>
        public DataRow[] GetRowNominal(int pStreamNo)
        {
            return GetRowEventByEventType(EventTypeFunc.Nominal.ToString(), pStreamNo);
        }
        #endregion
        #region public GetRowQuantity
        public DataRow[] GetRowQuantity()
        {
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE = '{3}' and EVENTTYPE = '{4}'",
            m_DsTrade.IdT.ToString(), StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), m_ParamStreamNo.Value, EventCodeFunc.NominalStep, EventTypeFunc.Quantity), "DTSTARTADJ");
        }
        #endregion GetRowQuantity
        #region public GetRowTerminationCurrency
        public DataRow[] GetRowTerminationCurrency()
        {
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE = '{3}' and EVENTTYPE in ('{4}','{5}')",
            m_DsTrade.IdT.ToString(), StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), m_ParamStreamNo.Value, EventCodeFunc.Termination,
            EventTypeFunc.Currency1, EventTypeFunc.Currency2), "IDE");
        }
        #endregion GetRowTerminationCurrency
        #region public GetRowTerminationVariationNominal
        public DataRow[] GetRowTerminationVariationNominal()
        {
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE = '{3}' and EVENTTYPE = '{4}')",
            m_DsTrade.IdT.ToString(), StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), m_ParamStreamNo.Value, EventCodeFunc.Termination,
            EventTypeFunc.Nominal), "DTSTARTADJ");
        }
        #endregion GetRowTerminationVariationNominal
        #region public GetRowTradeStream
        public DataRow[] GetRowTradeStream()
        {
            return m_DsTrade.DtTradeStream.Select("INSTRUMENTNO=" + m_ParamInstrumentNo.Value.ToString() +
                " and STREAMNO=" + m_ParamStreamNo.Value.ToString(), "IDT");
        }
        #endregion GetRowTradeStream
        #region public GetRowVariationNominal
        public DataRow[] GetRowVariationNominal()
        {
            // RD 20150325 [20821] Supprimer une parenthèse en trop dans le filtre
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE <> '{3}' and EVENTTYPE = '{4}'",
                m_DsTrade.IdT.ToString(), StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), m_ParamStreamNo.Value, EventCodeFunc.NominalStep,
                EventTypeFunc.Nominal), "DTSTARTADJ");
        }
        public DataRow GetRowVariationNominal(DateTime pDtStart)
        {
            DataRow[] rows = GetRowVariationNominal();
            if (ArrFunc.IsFilled(rows))
            {
                foreach (DataRow row in rows)
                {
                    if (pDtStart == Convert.ToDateTime(row["DTSTARTADJ"]))
                        return row;
                }
            }
            return null;
        }
        public DataRow GetRowVariationNominal(DateTime pDtStart, DateTime pDtEnd)
        {
            DataRow[] rows = GetRowVariationNominal();
            if (ArrFunc.IsFilled(rows))
            {
                foreach (DataRow row in rows)
                {
                    if ((pDtStart == Convert.ToDateTime(row["DTSTARTADJ"])) && (pDtEnd == Convert.ToDateTime(row["DTENDADJ"])))
                        return row;
                }
            }
            return null;
        }
        #endregion GetRowVariationNominal

        #region public IsRowHasFixingEvent
        // EG 20231127 [WI755] Implementation Return Swap : Add Test on Equity and Index
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        public bool IsRowHasFixingEvent(DataRow pRow)
        {
            #region RowEventClass Reader
            DataRow[] rowEventsClass = pRow.GetChildRows(DsEvents.ChildEventClass);
            _ = GetRowDetail(Convert.ToInt32(pRow["IDE"]));
            DataRow[] rowAssets = GetRowAsset(Convert.ToInt32(pRow["IDE"]));
            if (ArrFunc.IsFilled(rowEventsClass))
            {
                foreach (DataRow rowEventClass in rowEventsClass)
                {
                    if (EventClassFunc.IsFixing(rowEventClass["EVENTCLASS"].ToString()))
                    {
                        if (null == m_Quote)
                            return true;
                        else if (IsQuote_RateIndex && (m_Quote.time == Convert.ToDateTime(rowEventClass["DTEVENT"])))
                            return true;
                        else if (IsQuote_FxRate && ArrFunc.IsFilled(rowAssets) &&
                            (m_Quote.time.Date <= Convert.ToDateTime(rowEventClass["DTEVENT"])) &&
                            (m_Quote.idAsset == Convert.ToInt32(rowAssets[0]["IDASSET"])))
                            return true;
                        else if ((IsQuote_Equity || IsQuote_Index) && ArrFunc.IsFilled(rowAssets) &&
                            (m_Quote.time.Date == Convert.ToDateTime(rowEventClass["DTEVENT"])) &&
                            (m_Quote.idAsset == Convert.ToInt32(rowAssets[0]["IDASSET"])))
                            return true;
                    }
                }
            }
            #endregion RowEventClass Reader
            return false;
        }
        #endregion IsRowHasFixingEvent
        #region public IsRowPrecededAccrual
        /// <summary>
        /// Retourne true si CommonValDate=> DTENDUNADJ et que IsRowEventCalculated retourne true;
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        public bool IsRowPrecededAccrual(DataRow pRow)
        {
            bool ret = CommonValFunc.IsRowEventCalculated(pRow);
            ret = ret && (CommonValDate >= Convert.ToDateTime(pRow["DTENDUNADJ"]));
            return ret;
        }
        #endregion IsRowPrecededAccrual
        #region public FixedRateAndDCFFromRowInterest
        /// <summary>
        /// Retourne Cst.ErrLevel.SUCCESS s'il existe un stream d'intérêt à taux fixe  tel que DTSTARTADJ &lt;= pDate et &gt; pDate
        /// <para></para>
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pFixedRate">Returne la valeur du taux fixe de la rémunération</param>
        /// <param name="pDayCountFraction">Returne le DCF de la rémunération</param>
        /// <returns></returns>
        public Cst.ErrLevel FixedRateAndDCFFromRowInterest(DateTime pDate, out decimal pFixedRate, out string pDayCountFraction)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            pFixedRate = 0;
            pDayCountFraction = string.Empty;
            DataRow[] rowsInterest = GetRowInterest();
            if (ArrFunc.IsFilled(rowsInterest))
            {
                foreach (DataRow rowInterest in rowsInterest)
                {
                    if ((Convert.ToDateTime(rowInterest["DTSTARTADJ"]) <= pDate) && (pDate < Convert.ToDateTime(rowInterest["DTENDADJ"])))
                    {
                        DataRow[] rowCalcPeriods = rowInterest.GetChildRows(DsEvents.ChildEvent);
                        foreach (DataRow rowCalcPeriod in rowCalcPeriods)
                        {
                            if (EventTypeFunc.IsFixedRate(rowCalcPeriod["EVENTTYPE"].ToString()))
                            {
                                DataRow rowDetail = GetRowDetail(Convert.ToInt32(rowCalcPeriod["IDE"]));
                                if (null != rowDetail)
                                {
                                    pFixedRate = Convert.ToDecimal(rowDetail["RATE"]);
                                    pDayCountFraction = rowDetail["DCF"].ToString();
                                }
                                ret = Cst.ErrLevel.SUCCESS;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            return ret;
        }
        #endregion FixedRateAndDCFFromRowInterest
        #region public ResetRowCalculated
        /// <summary>
        /// alimente VALORISATION avec null et IDSTCALCUL avec ToCalculate
        /// </summary>
        /// <param name="pRow"></param>
        public static void ResetRowCalculated(DataRow pRow)
        {
            pRow["VALORISATION"] = Convert.DBNull;
            pRow["IDSTCALCUL"] = StatusCalculFunc.ToCalculate;
        }
        #endregion ResetRowCalculated
        #region public RoundingCurrencyAmount
        public decimal RoundingCurrencyAmount(string pCurrency, decimal pAmount)
        {
            EFS_Cash cash = new EFS_Cash(m_CS, pAmount, pCurrency);
            return cash.AmountRounded;
        }
        #endregion RoundingCurrencyAmount
        #region public SetRowStatus
        public void SetRowStatus(DataRow pRow, TuningOutputTypeEnum pTypeEnum)
        {
            if (m_Process.ProcessTuningSpecified)
            {
                if ((TuningOutputTypeEnum.OES == pTypeEnum) || (TuningOutputTypeEnum.OEE == pTypeEnum))
                {
                    int idE = Convert.ToInt32(pRow["IDE"]);
                    ProcessTuningOutput processTuning = m_Process.ProcessTuning.GetProcessTuningOutput(pTypeEnum);
                    // FI 20200820 [25468] dates systemes en UTC
                    DsEvents.SetEventStatus(idE, processTuning, m_Process.Session.IdA, OTCmlHelper.GetDateSysUTC(m_Process.Cs));
                }
            }
        }
        #endregion SetEventRowStatus

        #region protected RejectChangesInRowInterest
        /// <summary>
        /// Cancel Payment, CalculationPeriod Edit in Rowinterest
        /// </summary>
        /// <param name="pRowInterest"></param>
        protected void RejectChangesInRowInterest(DataRow pRowInterest)
        {
            RejectChangesInRowInterest(pRowInterest, false);
        }
        /// <summary>
        /// Cancel Payment, CalculationPeriod Edit in Rowinterest
        /// </summary>
        /// <param name="pRowInterest"></param>
        /// <param name="pIsOnlyOnchildRows">si true, annule uniquement les lignes enfans de la ligne d'intérêts</param>
        protected void RejectChangesInRowInterest(DataRow pRowInterest, bool pIsOnlyOnchildRows)
        {
            DataRow[] rowChilds = pRowInterest.GetChildRows(DsEvents.ChildEvent);
            foreach (DataRow rowChild in rowChilds)
            {
                if (EventCodeFunc.IsCalculationPeriod(rowChild["EVENTCODE"].ToString()))
                {
                    DataRow[] rowResets = rowChild.GetChildRows(DsEvents.ChildEvent);
                    foreach (DataRow rowReset in rowResets)
                    {
                        if (EventCodeFunc.IsReset(rowReset["EVENTCODE"].ToString()))
                        {
                            DataRow[] rowSelfAverages = rowReset.GetChildRows(DsEvents.ChildEvent);
                            foreach (DataRow rowSelfAverage in rowSelfAverages)
                            {
                                if (EventCodeFunc.IsSelfAverage(rowSelfAverage["EVENTCODE"].ToString()))
                                {
                                    DataRow[] rowSelfResets = rowSelfAverage.GetChildRows(DsEvents.ChildEvent);
                                    foreach (DataRow rowSelfReset in rowSelfResets)
                                    {
                                        if (EventCodeFunc.IsSelfReset(rowSelfReset["EVENTCODE"].ToString()))
                                            rowSelfReset.RejectChanges();
                                    }
                                    rowSelfAverage.RejectChanges();
                                    RejectDetail(rowSelfAverage);
                                }
                            }
                        }
                        rowReset.RejectChanges();
                        RejectDetail(rowReset);
                    }
                    rowChild.RejectChanges();
                    RejectDetail(rowChild);
                }
            }
            if (false == pIsOnlyOnchildRows)
            {
                pRowInterest.RejectChanges();
                RejectDetail(pRowInterest);
            }
        }
        #endregion RejectChangesInRowInterest
        #region protected RejectDetail
        /// <summary>
        /// Cancel Edit in RowDetail (EVENTDET)
        /// </summary>
        /// <param name="pRow"></param>
        protected void RejectDetail(DataRow pRow)
        {
            int idE = Convert.ToInt32(pRow["IDE"]);
            DataRow row = GetRowDetail(idE);
            if (null != row)
                row.RejectChanges();
        }
        #endregion RejectDetail
        #region protected RejectAsset
        /// <summary>
        /// Cancel Edit in RowDetail (EVENTASSET)
        /// </summary>
        /// <param name="pRow"></param>
        protected void RejectAsset(DataRow pRow)
        {
            int idE = Convert.ToInt32(pRow["IDE"]);
            DataRow[] rows = GetRowAsset(idE);
            if (ArrFunc.IsFilled(rows))
            {
                foreach (DataRow row in rows)
                    row.RejectChanges();
            }
        }
        #endregion RejectAsset

        #region protected UpdateEventDet
        #endregion UpdateEventDet

        #region private InitParameters
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        private void InitParameters()
        {
            #region Global
            m_ParamIdT = new DataParameter(m_CS, "IDT", DbType.Int32);
            m_ParamIdI = new DataParameter(m_CS, "IDI", DbType.Int32);
            m_ParamInstrumentNo = new DataParameter(m_CS, "INSTRUMENTNO", DbType.Int32);
            m_ParamStreamNo = new DataParameter(m_CS, "STREAMNO", DbType.Int32);
            m_ParamIdE = new DataParameter(m_CS, "IDE", DbType.Int32);
            m_ParamIdEParent = new DataParameter(m_CS, "IDE_EVENT", DbType.Int32);
            m_ParamInstrumentNoReference = new DataParameter(m_CS, "INSTRUMENTNO", DbType.Int32);
            m_ParamStreamNoReference = new DataParameter(m_CS, "STREAMNO", DbType.Int32);
            #endregion Global
        }
        #endregion InitParameters
        #region protected virtual Update
        protected virtual void Update(int pIdE, bool pIsError)
        {
            Update(null, pIdE, pIsError);
        }
        protected virtual void Update(IDbTransaction pDbTransaction, int pIdE, bool pIsError)
        {
            Update(pDbTransaction, pIdE, pIsError, false);
        }
        // EG 20180502 Analyse du code Correction [CA2200]
        protected virtual void Update(IDbTransaction pDbTransaction, int pIdE, bool pIsError, bool pIsEndOfDayProcess)
        {
            IDbTransaction dbTransaction = pDbTransaction;
            bool isException = false;
            try
            {
                if (null == pDbTransaction)
                    dbTransaction = DataHelper.BeginTran(m_CS);

                DsEvents.Update(dbTransaction, pIsEndOfDayProcess);

                DsEvents.Update(dbTransaction, Cst.OTCml_TBL.EVENTDET);

                //if (false == pIsEndOfDayProcess)
                //    UpdateEventAsset(dbTransaction);
                //UpdateEventDet(dbTransaction);

                ProcessStateTools.StatusEnum statusEnum = pIsError ? ProcessStateTools.StatusErrorEnum : ProcessStateTools.StatusSuccessEnum;
                EventProcess eventProcess = new EventProcess(this.m_CS);
                eventProcess.Write(dbTransaction, pIdE, m_Process.MQueue.ProcessType, statusEnum, OTCmlHelper.GetDateSysUTC(m_CS), m_Process.Tracker.IdTRK_L);

                if (null == pDbTransaction)
                    DataHelper.CommitTran(dbTransaction);
            }
            catch (SpheresException2)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((isException) && (null != pDbTransaction))
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

        #region public virtual RoundingDebtSecurityUnitCouponAmount
        public virtual decimal RoundingDebtSecurityUnitCouponAmount(decimal pAmount)
        {
            return pAmount;
        }
        #endregion RoundingDebtSecurityUnitCouponAmount
        #region public virtual RoundingDebtSecurityTransactionFullCouponAmount
        public virtual decimal RoundingDebtSecurityTransactionFullCouponAmount(decimal pAmount)
        {
            return pAmount;
        }
        #endregion RoundingDebtSecurityTransactionFullCouponAmount
        #region public virtual IsRowMustBeCalculate
        public virtual bool IsRowMustBeCalculate(DataRow pRow)
        {
            return false;
        }
        #endregion IsRowMustBeCalculate
        #region public virtual IsRowsEventCalculated
        public virtual bool IsRowsEventCalculated(DataRow[] pRows)
        {
            if (ArrFunc.IsFilled(pRows))
            {
                foreach (DataRow row in pRows)
                {
                    if (StatusCalculFunc.IsToCalculate(row["IDSTCALCUL"].ToString()))
                        return false;
                }
                return true;
            }
            return false;
        }
        #endregion IsRowMustBeCalculate
        #region public virtual Valorize
        public virtual Cst.ErrLevel Valorize()
        {
            return Cst.ErrLevel.UNDEFINED;
        }
        #endregion virtual Valorize

        // EG 20120529 Dispose
        /// <summary>
        /// Libération des connexions sur appel via API Call 
        /// </summary>
        public virtual void SlaveCallDispose()
        {
            if ((null != m_DsTrade) &&(null != m_DsTrade.DsTrade))
                m_DsTrade.DsTrade.Dispose();
            if ((null != m_DsEvents) && (null != m_DsEvents.DsEvent))
                m_DsEvents.DsEvent.Dispose();
            if (null != m_DsEventsDetails)
                m_DsEventsDetails.Dispose();

            m_DsTrade = null;
            m_DsEvents = null;
            m_tradeLibrary = null;
        }

        #region SetRowEventClosingAmountGen
        /// <summary>
        /// Ecriture dans les tables EVENTxxxx des EVTs de CASH-FLOWS 
        /// = Enfants de LPC/AMT 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIsNewRow"></param>
        /// <param name="pIdE"></param>
        /// <param name="pRowEvent"></param>
        /// <param name="pRowEventDet"></param>
        /// <param name="pRowEventClassREC"></param>
        /// <param name="pRowEventClassSTL"></param>
        /// <param name="pQuantity"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        // RD 20160805 [22415] Add method
        protected void SetRowEventClosingAmountGen(IDbTransaction pDbTransaction, bool pIsNewRow,
            int pIdE, DataRow pRowEvent, DataRow pRowEventDet, DataRow pRowEventClassREC, DataRow pRowEventClassVAL, DataRow pRowEventClassSTL,
            decimal pQuantity, int pQuotedCurrencyFactor, Nullable<decimal> pAmount, string pCurrency)
        {
            SetRowEventClosingAmountGen(pDbTransaction, pIsNewRow,
                pIdE, pRowEvent, pRowEventDet, pRowEventClassREC, pRowEventClassVAL, pRowEventClassSTL,
                pQuantity, pQuotedCurrencyFactor, pAmount, pCurrency, false);
        }
        /// <summary>
        /// Ecriture dans les tables EVENTxxxx des EVTs de CASH-FLOWS 
        /// = Enfants de LPC/AMT 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIsNewRow"></param>
        /// <param name="pIdE"></param>
        /// <param name="pRowEvent"></param>
        /// <param name="pRowEventDet"></param>
        /// <param name="pRowEventClassREC"></param>
        /// <param name="pRowEventClassVAL"></param>
        /// <param name="pRowEventClassSTL"></param>
        /// <param name="pQuantity"></param>
        /// <param name="pQuotedCurrencyFactor"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pIsZeroQtyForced">Generate Event, even if the quantity is zero</param>
        // EG 20150407 (POC] Add pNotionalReference Parameter (se subtitue à la quantité si renseigné (cas des FX)
        // EG 20150616 [21124] New EventClass VAL : ValueDate
        // RD 20160805 [22415] Add parameter pIsZeroQtyForced
        protected void SetRowEventClosingAmountGen(IDbTransaction pDbTransaction, bool pIsNewRow,
            int pIdE, DataRow pRowEvent, DataRow pRowEventDet, DataRow pRowEventClassREC, DataRow pRowEventClassVAL, DataRow pRowEventClassSTL,
            decimal pQuantity, int pQuotedCurrencyFactor, Nullable<decimal> pAmount, string pCurrency, bool pIsZeroQtyForced)
        {
            pRowEventDet["DAILYQUANTITY"] = pQuantity;
            pRowEventDet["FACTOR"] = pQuotedCurrencyFactor;
            pRowEvent["VALORISATION"] = pAmount.HasValue ? Math.Abs(pAmount.Value) : Convert.DBNull;
            pRowEvent["VALORISATIONSYS"] = pAmount.HasValue ? Math.Abs(pAmount.Value) : Convert.DBNull;
            pRowEvent["UNITTYPE"] = UnitTypeEnum.Currency;
            pRowEvent["UNIT"] = pCurrency;

            //EG 20110606
            // RD 20160805 [22415] Add pIsZeroQtyForced
            if (0 < pQuantity || pIsZeroQtyForced)
            {
                if (pIsNewRow)
                {
                    pRowEvent["UNITTYPESYS"] = UnitTypeEnum.Currency;
                    pRowEvent["UNITSYS"] = pRowEvent["UNIT"];

                    m_DsEvents.DtEvent.Rows.Add(pRowEvent);
                    m_DsEvents.DtEventClass.Rows.Add(pRowEventClassREC);

                    // EG 20150616 [21124]
                    if (null != pRowEventClassVAL)
                        m_DsEvents.DtEventClass.Rows.Add(pRowEventClassVAL);

                    if (null != pRowEventClassSTL)
                        m_DsEvents.DtEventClass.Rows.Add(pRowEventClassSTL);
                    m_DsEvents.DtEventDet.Rows.Add(pRowEventDet);
                }

                bool isEventChanged = (false == pIsNewRow);
                if (isEventChanged)
                {
                    // RD 20150909 [21315] Distinguish Null from Zero value for "valCurrent" and "valOriginal variables
                    Nullable<decimal> valCurrent = null;
                    Nullable<decimal> valOriginal = null;
                    if (false == Convert.IsDBNull(pRowEvent["VALORISATION", DataRowVersion.Original]))
                        valOriginal = Convert.ToDecimal(pRowEvent["VALORISATION", DataRowVersion.Original]);
                    if (false == Convert.IsDBNull(pRowEvent["VALORISATION", DataRowVersion.Current]))
                        valCurrent = Convert.ToDecimal(pRowEvent["VALORISATION", DataRowVersion.Current]);
                    isEventChanged = (valOriginal != valCurrent);

                    if (false == isEventChanged)
                    {
                        valCurrent = null;
                        valOriginal = null;
                        if (false == Convert.IsDBNull(pRowEventDet["DAILYQUANTITY", DataRowVersion.Original]))
                            valOriginal = Convert.ToDecimal(pRowEventDet["DAILYQUANTITY", DataRowVersion.Original]);
                        if (false == Convert.IsDBNull(pRowEventDet["DAILYQUANTITY", DataRowVersion.Current]))
                            valCurrent = Convert.ToDecimal(pRowEventDet["DAILYQUANTITY", DataRowVersion.Current]);
                        isEventChanged = (valOriginal != valCurrent);
                    }

                    if (false == isEventChanged)
                    {
                        valCurrent = null;
                        valOriginal = null;
                        if (false == Convert.IsDBNull(pRowEventDet["PRICE", DataRowVersion.Original]))
                            valOriginal = Convert.ToDecimal(pRowEventDet["PRICE", DataRowVersion.Original]);
                        if (false == Convert.IsDBNull(pRowEventDet["PRICE", DataRowVersion.Current]))
                            valCurrent = Convert.ToDecimal(pRowEventDet["PRICE", DataRowVersion.Current]);
                        isEventChanged = (valOriginal != valCurrent);
                    }

                    if (false == isEventChanged)
                    {
                        valCurrent = null;
                        valOriginal = null;
                        if (false == Convert.IsDBNull(pRowEventDet["QUOTEPRICE", DataRowVersion.Original]))
                            valOriginal = Convert.ToDecimal(pRowEventDet["QUOTEPRICE", DataRowVersion.Original]);
                        if (false == Convert.IsDBNull(pRowEventDet["QUOTEPRICE", DataRowVersion.Current]))
                            valCurrent = Convert.ToDecimal(pRowEventDet["QUOTEPRICE", DataRowVersion.Current]);
                        isEventChanged = (valOriginal != valCurrent);
                    }

                }
                // EG 20120627 Add isNewRow condition
                if (isEventChanged || pIsNewRow)
                    Update(pDbTransaction, pIdE, false, IsEndOfDayProcess);
                else
                {
                    pRowEvent.CancelEdit();
                    pRowEventDet.CancelEdit();
                }
            }
            else if (false == pIsNewRow)
            {
                pRowEvent.Delete();
                if (null != pRowEventClassREC)
                    pRowEventClassREC.Delete();

                // EG 20150616 [21124]
                if (null != pRowEventClassVAL)
                    pRowEventClassVAL.Delete();

                if (null != pRowEventClassSTL)
                    pRowEventClassSTL.Delete();
                if (null != pRowEventDet)
                {
                    pRowEventDet.Delete();
                    //PM 20150210 [20771][20783] Supprimer physiquement EVENTDET car sinon ils seront supprimés en cascade lors du delete de EVENT
                    //et cela générera une erreur lors du prochain update du dataset.
                    m_DsEvents.Update(pDbTransaction, Cst.OTCml_TBL.EVENTDET);
                }
                m_DsEvents.Update(pDbTransaction, IsEndOfDayProcess);
            }


        }
        #endregion SetRowEventClosingAmountGen

        #region GetMarginRatio
        /// <summary>
        /// Lecture du Margin factor en cours (dans référentiel). 
        /// Puis écriture dans événement si nécessaire
        /// </summary>
        /// <returns></returns>
        // EG 20150306 [POC] : New
        // EG 20150309 [POC] : Add SpreadSchedule amount
        // EG 20150317 [POC] : New Signature NewRowEvent
        // EG 20170510 [23153] Upd
        // EG 20180713 [23740] Report
        protected Pair<Nullable<decimal>,Nullable<decimal>> GetMarginRatio(IDbTransaction pDbTransaction, RptSideProductContainer pRptSideProductContainer, DateTime pDtBusiness)
        {
            Pair<Nullable<decimal>,Nullable<decimal>> ratioAndCrossMarginRatio = new Pair<decimal?,decimal?>();
            Nullable<decimal> marginRatioAmount = null;
            Nullable<decimal> crossMarginRatio = null;
            IMarginRatio marginRatio = null;
            IMarginRatio currentMarginRatio = null;

            // Recherche du Ratio en cours dans le référentiel
            // EG 20170510 [23153] New
            if (null == m_CurrentFeeRequest)
                InitalizeFeeRequest();
            MarginProcessing margin = TradeCaptureGen.GetMarging(m_CurrentFeeRequest);

            // EG 20150407 [POC] Add Test margin.MarginResponse.MarginRatioAmountSpecified
            if ((null != margin) && margin.MarginResponse.MarginRatioAmountSpecified)
            {
                IProduct product = margin.ScheduleRequest.TradeInput.DataDocument.CurrentProduct.Product;
                // Lecture du nouveau MarginFactor

                if (pRptSideProductContainer is EquitySecurityTransactionContainer)
                {
                    EquitySecurityTransactionContainer eqs = new EquitySecurityTransactionContainer((IEquitySecurityTransaction)product);
                    if (eqs.MarginRatioSpecified)
                        currentMarginRatio = eqs.MarginRatio;
                    marginRatio = eqs.EquitySecurityTransaction.CreateMarginRatio;
                }
                else if (pRptSideProductContainer is ReturnSwapContainer)
                {
                    ReturnSwapContainer rts = new ReturnSwapContainer((IReturnSwap)product);
                    // EG 20170510 [23153] New
                    //rts.SetMainLegs(m_CS, pDbTransaction);
                    if (rts.ReturnLeg.RateOfReturn.MarginRatioSpecified)
                        currentMarginRatio = rts.ReturnLeg.RateOfReturn.MarginRatio;
                    marginRatio = rts.ReturnLeg.CreateMarginRatio;
                }
                else if (pRptSideProductContainer is FxLegContainer)
                {
                    FxLegContainer fxl = new FxLegContainer((IFxLeg)product);
                    if (fxl.MarginRatioSpecified)
                        currentMarginRatio = fxl.MarginRatio;
                    marginRatio = fxl.FxLeg.CreateMarginRatio;
                }
                else if (pRptSideProductContainer is FxOptionLegContainer)
                {
                    FxOptionLegContainer fxol = new FxOptionLegContainer((IFxOptionLeg)product);
                    if (fxol.MarginRatioSpecified)
                        currentMarginRatio = fxol.MarginRatio;
                    marginRatio = fxol.FxOptionLeg.CreateMarginRatio;
                }

                if (null != currentMarginRatio)
                {
                    // EG 20150323 [POC]
                    if (margin.MarginResponse.MarginRatioAmountSpecified)
                        currentMarginRatio.Amount.DecValue = margin.MarginResponse.MarginRatioAmount.Value;
                    if (margin.MarginResponse.CrossMarginRatioAmountSpecified)
                    {
                        currentMarginRatio.CrossMarginRatioSpecified = margin.MarginResponse.CrossMarginRatioAmountSpecified;
                        currentMarginRatio.CrossMarginRatio.Amount.DecValue = margin.MarginResponse.CrossMarginRatioAmount.Value;
                    }
                    if (margin.MarginResponse.SpreadMarginRatioAmountSpecified)
                    {
                        currentMarginRatio.SpreadScheduleSpecified = margin.MarginResponse.SpreadMarginRatioAmountSpecified;
                        if (margin.MarginResponse.SpreadMarginRatioAmountSpecified)
                            currentMarginRatio.CreateSpreadMarginRatio(margin.MarginResponse.SpreadMarginRatioAmount.Value);
                    }
                    m_IsApplyMinMargin = margin.MarginResponse.IsApplyMinMargin;
                }

                DataRow rowMarginRatio = GetCurrentMarginRatio(pDtBusiness);
                Nullable<int> idERatioParent = null;
                if (null != rowMarginRatio)
                {
                    idERatioParent = Convert.ToInt32(rowMarginRatio["IDE_EVENT"]);
                    marginRatio.CurrencySpecified = (false == Convert.IsDBNull(rowMarginRatio["UNIT"]));
                    if (marginRatio.CurrencySpecified)
                    {
                        marginRatio.Currency.Value = rowMarginRatio["UNIT"].ToString();
                        marginRatio.PriceExpression = PriceExpressionEnum.AbsoluteTerms;
                    }
                    else
                    {
                        marginRatio.PriceExpression = PriceExpressionEnum.PercentageOfNotional;
                    }
                    marginRatio.Amount.DecValue = Convert.ToDecimal(rowMarginRatio["VALORISATION"]);
                }

                decimal currentMarginRatioAndSpread = 0;
                bool isPriceExpressionChanged = false;
                bool isMarginRatioAmountChanged = false;

                if (null != currentMarginRatio)
                {
                    currentMarginRatioAndSpread = currentMarginRatio.Amount.DecValue;
                    if (currentMarginRatio.CrossMarginRatioSpecified)
                        crossMarginRatio = currentMarginRatio.CrossMarginRatio.Amount.DecValue;

                    if (currentMarginRatio.SpreadScheduleSpecified)
                    {
                        currentMarginRatioAndSpread += currentMarginRatio.SpreadSchedule.InitialValue.DecValue;
                        if (currentMarginRatio.CrossMarginRatioSpecified)
                            crossMarginRatio += currentMarginRatio.SpreadSchedule.InitialValue.DecValue;
                    }
                    isPriceExpressionChanged = (marginRatio.PriceExpression != currentMarginRatio.PriceExpression);
                    isMarginRatioAmountChanged = (marginRatio.Amount.DecValue != currentMarginRatioAndSpread);
                }

                // Nouveau MarginRatio 
                // = (Nouveau marginRatio et pas d'ancien MarginRatio) | (Nouveau marginRatio et différent de l'ancien MarginRatio)
                if ((null != currentMarginRatio) && (isPriceExpressionChanged || isMarginRatioAmountChanged))
                {
                    // Création Event MGF 
                    DataRow rowEventParent = GetRowEventParentRatio(idERatioParent, pRptSideProductContainer);

                    // RD 20160627 [22295] Add pDbTransaction
                    DataRow newRowEvent = NewRowEvent(pDbTransaction, rowEventParent, EventCodeFunc.LinkedProductPayment, EventTypeFunc.MarginRequirementRatio, pDtBusiness, m_Process.AppInstance);
                    DataRow newrowEventClass = NewRowEventClass(Convert.ToInt32(newRowEvent["IDE"]), EventClassFunc.Recognition, pDtBusiness, false);

                    switch (currentMarginRatio.PriceExpression)
                    {
                        case PriceExpressionEnum.AbsoluteTerms:
                            newRowEvent["UNITTYPE"] = UnitTypeEnum.Currency;
                            newRowEvent["UNITTYPESYS"] = UnitTypeEnum.Currency;
                            newRowEvent["UNIT"] = currentMarginRatio.CurrencySpecified ? currentMarginRatio.Currency.Value : Convert.DBNull;
                            newRowEvent["UNITSYS"] = currentMarginRatio.CurrencySpecified ? currentMarginRatio.Currency.Value : Convert.DBNull;
                            break;
                        case PriceExpressionEnum.PercentageOfNotional:
                            newRowEvent["UNITTYPE"] = UnitTypeEnum.Percentage;
                            newRowEvent["UNITTYPESYS"] = UnitTypeEnum.Percentage;
                            newRowEvent["UNIT"] = Convert.DBNull;
                            newRowEvent["UNITSYS"] = Convert.DBNull;
                            break;
                    }
                    newRowEvent["VALORISATION"] = currentMarginRatioAndSpread;
                    newRowEvent["VALORISATIONSYS"] = currentMarginRatioAndSpread;
                    newRowEvent["DTSTARTADJ"] = pDtBusiness;
                    newRowEvent["DTSTARTADJ"] = pDtBusiness;

                    DataRow rowEvent = GetCurrentMarginRatio(pDtBusiness);
                    if (null != rowEvent)
                    {
                        newRowEvent["DTENDADJ"] = rowEvent["DTENDADJ"];
                        newRowEvent["DTENDUNADJ"] = rowEvent["DTENDUNADJ"];
                    }
                    else
                    {
                        newRowEvent["DTENDADJ"] = rowEventParent["DTENDADJ"];
                        newRowEvent["DTENDUNADJ"] = rowEventParent["DTENDUNADJ"];
                    }
                    m_DsEvents.DtEvent.Rows.Add(newRowEvent);
                    m_DsEvents.DtEventClass.Rows.Add(newrowEventClass);

                    if (null != rowEvent)
                    {
                        rowEvent["DTENDADJ"] = pDtBusiness;
                        rowEvent["DTENDUNADJ"] = pDtBusiness;
                        // RD 20180124 [23740]
                        //Update(pDbTransaction, Convert.ToInt32(rowEvent["IDE"]), false, IsEndOfDayProcess);
                        //Update(pDbTransaction, Convert.ToInt32(rowEvent["IDE"]), false, IsEndOfDayProcess);
                    }
                    //Update(pDbTransaction, Convert.ToInt32(newRowEvent["IDE"]), false, IsEndOfDayProcess);
                    // RD 20180124 [23740]
                    //Update(pDbTransaction, Convert.ToInt32(newRowEvent["IDE"]), false, IsEndOfDayProcess);
                }
                rowMarginRatio = GetCurrentMarginRatio(pDtBusiness);
                if (null != rowMarginRatio)
                    marginRatioAmount = Convert.ToDecimal(rowMarginRatio["VALORISATION"]);
            }
            ratioAndCrossMarginRatio.First = marginRatioAmount;
            ratioAndCrossMarginRatio.Second = crossMarginRatio;
            return ratioAndCrossMarginRatio;
        }
        #endregion GetMarginRatio
        #region GetCrossMarginRatio
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        protected Nullable<decimal> GetCrossMarginRatio()
        {
            Nullable<decimal> crossMarginRatioAmount = null;
            // Recherche du Ratio en cours dans le référentiel
            User user = new User(m_Process.Session.IdA, null, RoleActor.SYSADMIN);
            TradeInput tradeInput = new TradeInput
            {
                DataDocument = m_tradeLibrary.DataDocument
            };
            tradeInput.SearchAndDeserializeShortForm(m_CS,null, m_DsTrade.IdT.ToString(), SQL_TableWithID.IDType.Id, user, m_Process.Session.SessionId);
            FeeRequest feeRequest = new FeeRequest(CSTools.SetCacheOn(m_CS),null, tradeInput, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade));
            MarginProcessing margin = TradeCaptureGen.GetMarging(feeRequest);

            if ((null != margin) && margin.MarginResponse.CrossMarginRatioAmountSpecified)
            {
                crossMarginRatioAmount = margin.MarginResponse.CrossMarginRatioAmount;
                if (margin.MarginResponse.SpreadMarginRatioAmountSpecified)
                    crossMarginRatioAmount += margin.MarginResponse.SpreadMarginRatioAmount.Value;
            }
            return crossMarginRatioAmount;
        }
        #endregion GetCrossMarginRatio

        #region SetUnrealizedMargin
        /// <summary>
        /// Calcul du UMG
        /// </summary>
        // EG 20150306 [POC-BERKELEY] : New Mutualisation RTS|ESE
        // EG 20150706 [21021] Pair<int,Nullable<int>> for pPayer|pReceiver
        // EG 20150910 [21315] Payer|Receiver renseignés même si Montant = null
        // EG 20180307 [23769] Gestion dbTransaction
        protected Pair<Nullable<decimal>, string> SetUnrealizedMargin(IDbTransaction pDbTransaction, decimal pQuantity, decimal pMultiplier, string pCurrency,
            Nullable<decimal> pQuote100, decimal pPrice, ref Pair<int, Nullable<int>> pPayer, ref Pair<int, Nullable<int>> pReceiver)
        {
            Pair<Nullable<decimal>, string> closingAmount = new Pair<Nullable<decimal>, string>(null, string.Empty);
            if (pQuote100.HasValue)
            {
                closingAmount = Tools.ConvertToQuotedCurrency(CSTools.SetCacheOn(m_CS), pDbTransaction, 
                                new Pair<Nullable<decimal>, string>((pQuote100.Value - pPrice) * pQuantity * pMultiplier, pCurrency));
            }
            // EG 20150910 [21315]
            bool amountValuatedAndPositive = closingAmount.First.HasValue && (0 < closingAmount.First.Value);
            pPayer.First = amountValuatedAndPositive ? m_Seller.OTCmlId : m_Buyer.OTCmlId;
            pPayer.Second = amountValuatedAndPositive ? m_BookSeller : m_BookBuyer;
            pReceiver.First = amountValuatedAndPositive ? m_Buyer.OTCmlId : m_Seller.OTCmlId;
            pReceiver.Second = amountValuatedAndPositive ? m_BookBuyer : m_BookSeller;

            return closingAmount;
        }
        #endregion SetUnrealizedMargin

        #endregion Methods
    }
    
}

#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.Administrative.Invoicing;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Status;
using EFS.TradeInformation;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
using EfsML.Interface;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process
{
    public class InvoicingSettlement_AllocatedInvoice : InvoicingGenProcessBase
    {
        #region Members
        private readonly DataSetTrade m_DsTrade;
        private readonly DataSetEventTrade m_DsEvents;
        private readonly EFS_TradeLibrary m_TradeLibrary;

        private DataSetTrade m_DsTradeAllocatedInvoice;
        private DataSetEventTrade m_DsEventsAllocatedInvoice;
        #endregion Members
        #region Accessors
        #region ActionDate
        public override DateTime ActionDate
        {
            get { return Queue.GetMasterDate(); }
        }
        #endregion ActionDate
        #region CurrentTrade
        public ITrade CurrentTrade
        {
            get { return m_TradeLibrary.CurrentTrade; }
        }
        #endregion CurrentTrade
        #region DtEvent
        public DataTable DtEvent
        {
            get { return m_DsEvents.DtEvent; }
        }
        #endregion DtEvent
        #region DtEventClass
        protected DataTable DtEventClass
        {
            get { return m_DsEvents.DtEventClass; }
        }
        #endregion DtEventClass
        #region DtEventAllocatedInvoice
        public DataTable DtEventAllocatedInvoice
        {
            get { return m_DsEventsAllocatedInvoice.DtEvent; }
        }
        #endregion DtEventAllocatedInvoice
        #region DtEventClassAllocatedInvoice
        protected DataTable DtEventClassAllocatedInvoice
        {
            get { return m_DsEventsAllocatedInvoice.DtEventClass; }
        }
        #endregion DtEventClassAllocatedInvoice

        #region GetRowInvoicePeriod
        public DataRow GetRowInvoicePeriod
        {
            get
            {
                DataRow[] rows = m_DsEventsAllocatedInvoice.DtEvent.Select(StrFunc.AppendFormat(@"EVENTCODE='{0}' and EVENTTYPE='{1}'",
                EventCodeFunc.InvoicingDates, EventTypeFunc.Period), "IDE");
                if ((null != rows) && (0 < rows.Length))
                    return rows[0];
                return null;
            }
        }
        #endregion GetRowInvoicePeriod
        #region GetRowInvoiceSettlement
        public DataRow GetRowInvoiceSettlement
        {
            get
            {
                //DataRow[] rows = m_DsEventsAllocatedInvoice.DtEvent.Select(StrFunc.AppendFormat(@"EVENTCODE='{0}'", EventCodeFunc.InvoiceSettlement), "IDE");
                DataRow[] rows = m_DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"EVENTCODE='{0}'", EventCodeFunc.InvoiceSettlement), "IDE");
                if ((null != rows) && (0 < rows.Length))
                    return rows[0];
                return null;
            }
        }
        #endregion GetRowInvoiceSettlement
        #region GetRowProfitAndLoss
        public DataRow GetRowProfitAndLoss
        {
            get
            {
                //DataRow[] rows = m_DsEventsAllocatedInvoice.DtEvent.Select(StrFunc.AppendFormat(@"EVENTTYPE in ('{0}','{1}')",
                //    EventTypeFunc.ForeignExchangeProfit, EventTypeFunc.ForeignExchangeLoss), "IDE");
                DataRow[] rows = m_DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"EVENTTYPE in ('{0}','{1}')",
                    EventTypeFunc.ForeignExchangeProfit, EventTypeFunc.ForeignExchangeLoss), "IDE");
                if ((null != rows) && (0 < rows.Length))
                    return rows[0];
                return null;
            }
        }
        #endregion GetRowProfitAndLoss
        #region GetRowUnallocatedAmount
        public DataRow GetRowUnallocatedAmount
        {
            get
            {
                //DataRow[] rows = m_DsEventsAllocatedInvoice.DtEvent.Select(StrFunc.AppendFormat(@"EVENTTYPE ='{0}'", EventTypeFunc.NonAllocatedAmount), "IDE");
                DataRow[] rows = m_DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"EVENTTYPE ='{0}'", EventTypeFunc.NonAllocatedAmount), "IDE");
                if ((null != rows) && (0 < rows.Length))
                    return rows[0];
                return null;
            }
        }
        #endregion GetRowUnallocatedAmount
        #region IsSimulation
        public override bool IsSimulation
        {
            get {return false;}
        }
        #endregion IsSimulation
        #region MasterDate
        public override DateTime MasterDate
        {
            get {return ((IInvoice)m_TradeLibrary.CurrentTrade.Product).InvoiceDate.DateValue;}
        }
        #endregion MasterDate
        #endregion Accessors
        #region Constructors
        // EG 20190613 [24683] Use slaveDbTransaction
        public InvoicingSettlement_AllocatedInvoice(InvoicingGenProcess pInvoicingGenProcess)
            : base(pInvoicingGenProcess)
		{
            m_DsTrade = new DataSetTrade(ProcessBase.Cs, ProcessBase.SlaveDbTransaction, ProcessBase.CurrentId);
            m_DsEvents = new DataSetEventTrade(ProcessBase.Cs, ProcessBase.SlaveDbTransaction, ProcessBase.CurrentId);
            m_EventProcess = new EventProcess(ProcessBase.Cs);
            m_TradeLibrary = new EFS_TradeLibrary(ProcessBase.Cs, ProcessBase.SlaveDbTransaction, m_DsTrade.IdT);
		}
		#endregion Constructors
        #region Methods
        #region AddAllocatedInvoiceDatesEvent
        protected DataRow AddAllocatedInvoiceDatesEvent(int pIdE, int pIdESource, DataSetEventTrade pDsEventTrade, DataRow pRowSource, EFS_AllocatedInvoice pAllocatedInvoice)
        {
            // EG 20160404 Migration vs2013
            //int tmpInt = 0;
            DataRow row = pDsEventTrade.DtEvent.NewRow();
            row.ItemArray = (object[])pRowSource.ItemArray.Clone();
            row.BeginEdit();
            row["IDE"] = pIdE;
            row["IDE_SOURCE"] = pIdESource;
            row["IDE_EVENT"] = pRowSource["IDE"];
            row["EVENTCODE"] = EventCodeFunc.AllocatedInvoiceDates;
            row["EVENTTYPE"] = EventTypeFunc.CashSettlement;
            row["DTSTARTADJ"] = pAllocatedInvoice.receptionDate.DateValue.Date;
            row["DTSTARTUNADJ"] = pAllocatedInvoice.receptionDate.DateValue.Date;
            row["DTENDADJ"] = pAllocatedInvoice.receptionDate.DateValue.Date;
            row["DTENDUNADJ"] = pAllocatedInvoice.receptionDate.DateValue.Date;

            // EG 20150706 [21021]
            Nullable<int> idA_Pay = m_TradeLibrary.DataDocument.GetOTCmlId_Party(pAllocatedInvoice.payerPartyReference.HRef);
            Nullable<int> idB_Pay = m_TradeLibrary.DataDocument.GetOTCmlId_Book(pAllocatedInvoice.payerPartyReference.HRef); 
            Nullable<int> idA_Rec = m_TradeLibrary.DataDocument.GetOTCmlId_Party(pAllocatedInvoice.receiverPartyReference.HRef);
            Nullable<int> idB_Rec = m_TradeLibrary.DataDocument.GetOTCmlId_Book(pAllocatedInvoice.receiverPartyReference.HRef);

            row["IDA_PAY"] = (idA_Pay ?? Convert.DBNull);
            row["IDB_PAY"] = (idB_Pay ?? Convert.DBNull);
            row["IDA_REC"] = (idA_Rec ?? Convert.DBNull);
            row["IDB_REC"] = (idB_Rec ?? Convert.DBNull);

            if (m_InvoicingGenProcess.CurrentId == pDsEventTrade.IdT)
            {
                #region Côté InvoiceSettlement
                row["VALORISATION"] = pAllocatedInvoice.accountingAmount.Amount.DecValue;
                row["UNIT"] = pAllocatedInvoice.accountingAmount.Currency;
                row["VALORISATIONSYS"] = pAllocatedInvoice.accountingAmount.Amount.DecValue;
                row["UNITSYS"] = pAllocatedInvoice.accountingAmount.Currency;
                #endregion Côté InvoiceSettlement
            }
            row["IDSTTRIGGER"] = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
            row["IDSTCALCUL"] = StatusCalculFunc.Calculated;
            // FI 20131007 [] SOURCE contient l'instance du service
            //row["SOURCE"] = m_InvoicingGenProcess.appInstance.AppNameVersion;
            row["SOURCE"] = m_InvoicingGenProcess.AppInstance.ServiceName;
            row.EndEdit();
            pDsEventTrade.DtEvent.Rows.Add(row);
            SetRowStatus(row, pDsEventTrade, TuningOutputTypeEnum.OES);

            AddAllocatedInvoiceEventClass(pIdE, pDsEventTrade.DtEventClass, EventClassFunc.GroupLevel, pAllocatedInvoice.receptionDate.DateValue.Date);
            return row;
        }
        #endregion AddAllocatedInvoiceDatesEvent
        #region AddAllocatedInvoiceEventClass
        protected DataRow AddAllocatedInvoiceEventClass(int pIdE,DataTable pDtEventClass, string pEventClass, DateTime pClassDate)
        {
            #region EventClass Allocated invoice (ALD-CSH, LPP-NTO/NTI/NTA, LPP-FXP/FXL)
            DataRow row = pDtEventClass.NewRow();
            row.BeginEdit();
            row["IDE"] = pIdE;
            row["EVENTCLASS"] = pEventClass;
            row["DTEVENT"] = pClassDate;
            row["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_InvoicingGenProcess.Cs, pClassDate);
            row["ISPAYMENT"] = EventClassFunc.IsSettlement(pEventClass);
            row.EndEdit();
            pDtEventClass.Rows.Add(row);
            return row;
            #endregion EventClass Allocated invoice (ALD-CSH, LPP-NTO/NTI/NTA, LPP-FXP/FXL)
        }
        #endregion AddEventClassAbandonExerciseOut
        #region AddInvoiceEvents
        private Cst.ErrLevel AddInvoiceEvents(IDbTransaction pDbTransaction, EFS_AllocatedInvoice pEfs_AllocatedInvoice, int pIdE, int pNbTaxDetail)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;
            bool isTaxSpecified = (0 < pNbTaxDetail);
            DataRow rowINV = GetRowInvoicePeriod;
            if (null != rowINV)
            {
                bool isFullyAllocated = IsFullyAllocated(rowINV, pEfs_AllocatedInvoice);

                int newIdE = pIdE+1; // ne pas déplacer ni supprimer
                DateTime receptionDate = pEfs_AllocatedInvoice.AdjustedReceptionDate.DateValue;
                #region Add Event ALD - CSH
                DataRow rowALD = AddAllocatedInvoiceDatesEvent(newIdE, pIdE, m_DsEventsAllocatedInvoice, rowINV, pEfs_AllocatedInvoice);
                newIdE++;
                #endregion Add Event ALD - CSH
                #region Add Event LPP - NTO/NTI/NTA
                DataRow rowNTx = AddNetTurnOverEvents(newIdE, rowALD, EventTypeFunc.NetTurnOverAmount, pEfs_AllocatedInvoice.amount, receptionDate);
                newIdE++;
                if (isTaxSpecified && isFullyAllocated)
                {
                    AddTaxEvents(newIdE, rowINV, rowNTx, EventTypeFunc.NetTurnOverAmount, pEfs_AllocatedInvoice.amount, receptionDate);
                    newIdE += 1 + pNbTaxDetail;
                }
                rowNTx = AddNetTurnOverEvents(newIdE, rowALD, EventTypeFunc.NetTurnOverIssueAmount, pEfs_AllocatedInvoice.issueAmount, receptionDate);
                newIdE++;
                if (isTaxSpecified && isFullyAllocated)
                {
                    AddTaxEvents(newIdE, rowINV, rowNTx, EventTypeFunc.NetTurnOverIssueAmount, pEfs_AllocatedInvoice.issueAmount, receptionDate);
                    newIdE += 1 + pNbTaxDetail;
                }
                rowNTx = AddNetTurnOverEvents(newIdE, rowALD, EventTypeFunc.NetTurnOverAccountingAmount, pEfs_AllocatedInvoice.accountingAmount, receptionDate);
                newIdE++;
                if (isTaxSpecified && isFullyAllocated)
                {
                    AddTaxEvents(newIdE, rowINV, rowNTx, EventTypeFunc.NetTurnOverAccountingAmount, pEfs_AllocatedInvoice.accountingAmount, receptionDate);
                    newIdE += 1 + pNbTaxDetail;
                }
                #endregion Add Event LPP - NTO/NTI/NTA
                #region Add Event LPP - FXP/FXL
                if (pEfs_AllocatedInvoice.fxGainOrLossAmountSpecified)
                    AddProfitAndLossEvent(newIdE, m_DsEventsAllocatedInvoice, rowALD, pEfs_AllocatedInvoice.fxGainOrLossAmount, receptionDate);
                #endregion Add Event LPP - FXP/FXL

                #region Update ROWATTRIBUT éventuel [C]lose
                if (isFullyAllocated)
                {
                    DataRow rowTrade = m_DsTradeAllocatedInvoice.DtTrade.Rows[0];
                    rowTrade.BeginEdit();
                    rowTrade["ROWATTRIBUT"] = Cst.RowAttribut_InvoiceClosed;
                    rowTrade.EndEdit();
                }
                #endregion Update ROWATTRIBUT éventuel [C]lose
                Update(pDbTransaction);
                codeReturn = Cst.ErrLevel.SUCCESS;
            }
            return codeReturn;
        }
        #endregion AddInvoiceEvents
        #region AddProfitAndLossEvent
        // 20090408 EG Math.Abs sur FXP/FXL
        protected DataRow AddProfitAndLossEvent(int pIdE, DataSetEventTrade pDsEventTrade, DataRow pRowSource, IMoney pAmount, DateTime pDtEventClass)
        {
            DataRow row = pDsEventTrade.DtEvent.NewRow();
            row.ItemArray = (object[])pRowSource.ItemArray.Clone();
            row.BeginEdit();
            row["IDE"] = pIdE;
            row["IDE_EVENT"] = pRowSource["IDE"];
            row["EVENTCODE"] = EventCodeFunc.LinkedProductPayment;
            row["EVENTTYPE"] = ((0 <= pAmount.Amount.DecValue) ? EventTypeFunc.ForeignExchangeProfit : EventTypeFunc.ForeignExchangeLoss);
            if (0 > pAmount.Amount.DecValue)
            {
                row["IDA_PAY"] = pRowSource["IDA_REC"];
                row["IDB_PAY"] = pRowSource["IDB_REC"];
                row["IDA_REC"] = pRowSource["IDA_PAY"];
                row["IDB_REC"] = pRowSource["IDB_PAY"];
            }
            // 20090408 EG Math.Abs sur FXP/FXL
            row["VALORISATION"] = Math.Abs(pAmount.Amount.DecValue);
            row["UNIT"] = pAmount.Currency;
            // 20090408 EG Math.Abs sur FXP/FXL
            row["VALORISATIONSYS"] = Math.Abs(pAmount.Amount.DecValue);
            row["UNITSYS"] = pAmount.Currency;
            row.EndEdit();
            pDsEventTrade.DtEvent.Rows.Add(row);
            SetRowStatus(row,pDsEventTrade, TuningOutputTypeEnum.OES);
            AddAllocatedInvoiceEventClass(pIdE,pDsEventTrade.DtEventClass, EventClassFunc.Recognition, pDtEventClass);
            return row;
        }
        #endregion AddProfitAndLossEvent
        #region AddNetTurnOverEvents
        protected DataRow AddNetTurnOverEvents(int pIdE, DataRow pRowSource, string pEventType,IMoney pAmount, DateTime pDtEventClass)
        {
            DataRow row = m_DsEventsAllocatedInvoice.DtEvent.NewRow();
            row.ItemArray = (object[])pRowSource.ItemArray.Clone();
            row.BeginEdit();
            row["IDE"] = pIdE;
            row["IDE_EVENT"] = pRowSource["IDE"];
            row["EVENTCODE"] = EventCodeFunc.LinkedProductPayment;
            row["EVENTTYPE"] = pEventType;
            row["VALORISATION"] = pAmount.Amount.DecValue;
            row["UNIT"] = pAmount.Currency;
            row["UNITTYPE"] = UnitTypeEnum.Currency;
            row["VALORISATIONSYS"] = pAmount.Amount.DecValue;
            row["UNITSYS"] = pAmount.Currency;
            row["UNITTYPESYS"] = UnitTypeEnum.Currency;
            // EG 20100225 
            row["IDE_SOURCE"] = Convert.DBNull;
            row.EndEdit();
            DtEventAllocatedInvoice.Rows.Add(row);
            SetRowStatus(row,m_DsEventsAllocatedInvoice, TuningOutputTypeEnum.OES);
            AddAllocatedInvoiceEventClass(pIdE,m_DsEventsAllocatedInvoice.DtEventClass, EventClassFunc.Settlement, pDtEventClass);
            return row;
        }
        #endregion AddNetTurnOverEvents
        #region AddTaxEvents
        protected DataRow AddTaxEvents(int pIdE, DataRow pRowINV, DataRow pRowNTx, string pEventType, IMoney pAmount, DateTime pDtEventClass)
        {
            int newIdE = pIdE;
            decimal baseAmountForTax = 0;
            DataRow rowInitialNetTurnOver = GetRowNetTurnOver(pRowINV, pEventType);
            DataRow[] rowInitialTax = null;
            DataRow[] rowInitialTaxDetails = null;
            // NTO/NTI/NTA -REC
            if (null != rowInitialNetTurnOver)
            {
                rowInitialTax = rowInitialNetTurnOver.GetChildRows(m_DsEventsAllocatedInvoice.ChildEvent);
                if ((null != rowInitialTax) && (0 < rowInitialTax.Length))
                {
                    foreach (DataRow row in rowInitialTax)
                    {
                        string eventTypeTax = row["EVENTTYPE"].ToString();
                        if (EventTypeFunc.IsTax(eventTypeTax))
                        {
                            rowInitialTaxDetails = row.GetChildRows(m_DsEventsAllocatedInvoice.ChildEvent);
                            break;
                        }
                    }
                }
            }
            EFS_Cash cash;
            #region Calcul du montant de base pour le calcul des taxes réelles STL 
            // en fonction des Nets réels alloués (INVOICE - AVOIRS)
            if (null != rowInitialTaxDetails)
            {
                decimal taxRate = 0;
                foreach (DataRow rowInitialTaxDetail in rowInitialTaxDetails)
                {
                    DataRow[] rowEventFee = rowInitialTaxDetail.GetChildRows(m_DsEventsAllocatedInvoice.ChildEventFee);
                    if (null != rowEventFee)
                        taxRate += Convert.ToDecimal(rowEventFee[0]["TAXRATE"]);
                }
                decimal totalAllocatedNetTurnOverAmount = GetTotalAllocatedNetTurnOverAmount(pEventType);
                cash = new EFS_Cash(ProcessBase.Cs, totalAllocatedNetTurnOverAmount / (1 + taxRate), pAmount.Currency);
                baseAmountForTax = cash.AmountRounded;
            }
            #endregion Calcul du montant de base pour le calcul des taxes réelles STL 
            #region Ajout row Taxes STL
            DataRow rowFinalTax = m_DsEventsAllocatedInvoice.DtEvent.NewRow();
            rowFinalTax.ItemArray = (object[])rowInitialTax[0].ItemArray.Clone();
            rowFinalTax.BeginEdit();
            rowFinalTax["IDE"] = newIdE;
            rowFinalTax["IDE_EVENT"] = pRowNTx["IDE"];
            rowFinalTax["EVENTTYPE"] = rowInitialTax[0]["EVENTTYPE"];
            decimal totalTax = 0;
            if (null != rowInitialTaxDetails)
            {
                #region Add row TAXDET STL
                foreach (DataRow rowInitialTaxDetail in rowInitialTaxDetails)
                {
                    newIdE++;
                    DataRow rowFinalTaxDetail = m_DsEventsAllocatedInvoice.DtEvent.NewRow();
                    rowFinalTaxDetail.ItemArray = (object[])rowInitialTaxDetail.ItemArray.Clone();
                    rowFinalTaxDetail.BeginEdit();
                    rowFinalTaxDetail["IDE"] = newIdE;
                    rowFinalTaxDetail["IDE_EVENT"] = rowFinalTax["IDE"];
                    //
                    DataRow[] rowInitialFee = rowInitialTaxDetail.GetChildRows(m_DsEventsAllocatedInvoice.ChildEventFee);
                    decimal taxRate = Convert.ToDecimal(rowInitialFee[0]["TAXRATE"]);
                    cash = new EFS_Cash(ProcessBase.Cs, baseAmountForTax * taxRate, pAmount.Currency);
                    totalTax += cash.AmountRounded;
                    rowFinalTaxDetail["VALORISATION"] = cash.AmountRounded;
                    rowFinalTaxDetail["VALORISATIONSYS"] = cash.AmountRounded;
                    //
                    #region Add row EVENTFEE
                    DataRow rowFinalFee = m_DsEventsAllocatedInvoice.DtEventFee.NewRow();
                    rowFinalFee.ItemArray = (object[])rowInitialFee[0].ItemArray.Clone();
                    rowFinalFee.BeginEdit();
                    rowFinalFee["IDE"] = rowFinalTaxDetail["IDE"];
                    rowFinalFee.EndEdit();
                    m_DsEventsAllocatedInvoice.DtEventFee.Rows.Add(rowFinalFee);
                    #endregion Add row EVENTFEE
                    //
                    rowFinalTaxDetail.EndEdit();
                    DtEventAllocatedInvoice.Rows.Add(rowFinalTaxDetail);
                    SetRowStatus(rowFinalTaxDetail, m_DsEventsAllocatedInvoice, TuningOutputTypeEnum.OES);
                    AddAllocatedInvoiceEventClass(newIdE, m_DsEventsAllocatedInvoice.DtEventClass, EventClassFunc.Settlement, pDtEventClass);

                }
                #endregion Add row TAXDET STL
            }
            rowFinalTax["VALORISATION"] = totalTax;
            rowFinalTax["VALORISATIONSYS"] = totalTax;
            // EG 20100225 
            rowFinalTax["IDE_SOURCE"] = Convert.DBNull;
            rowFinalTax.EndEdit();
            DtEventAllocatedInvoice.Rows.Add(rowFinalTax);
            SetRowStatus(rowFinalTax, m_DsEventsAllocatedInvoice, TuningOutputTypeEnum.OES);
            AddAllocatedInvoiceEventClass(pIdE, m_DsEventsAllocatedInvoice.DtEventClass, EventClassFunc.Settlement, pDtEventClass);
            #endregion Ajout row Taxes STL
            return rowFinalTax;
        }
        #endregion AddTaxEvents

        #region Calculation
        public Cst.ErrLevel Calculation()
        {
            IProduct product = (IProduct)CurrentTrade.Product;
            IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)product;
            invoiceSettlement.Efs_InvoiceSettlement = new EFS_InvoiceSettlement(m_InvoicingGenProcess.Cs, invoiceSettlement);
            Cst.ErrLevel codeReturn = invoiceSettlement.Efs_InvoiceSettlement.ErrLevel;
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                m_TradeLibrary.DataDocument.AddParty(invoiceSettlement.Efs_InvoiceSettlement.bankActor);
            return codeReturn;
        }
        #endregion Calculation
        #region Generate
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Generate()
        {
            bool isException = false;
            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;
            IDbTransaction dbTransaction = null;
            try
            {
                IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)m_TradeLibrary.Product;
                EFS_AllocatedInvoice efs_AllocatedInvoice = null;
                if (invoiceSettlement.AllocatedInvoiceSpecified)
                {
                    #region Alimentation de la classe EFS_InvoiceSettlement
                    codeReturn = Calculation();
                    #endregion Alimentation de la classe EFS_InvoiceSettlement

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        #region recherche Evenement IST-AMT
                        DataRow rowInvoiceSettlement = GetRowInvoiceSettlement;
                        #endregion recherche Evenement IST-AMT

                        dbTransaction = DataHelper.BeginTran(m_InvoicingGenProcess.Cs);
                        foreach (IAllocatedInvoice allocatedInvoice in invoiceSettlement.AllocatedInvoice)
                        {
                            if (allocatedInvoice.Id.StartsWith("NEW_"))
                            {
                                #region Nouvelle allocation
                                efs_AllocatedInvoice = invoiceSettlement.Efs_InvoiceSettlement[allocatedInvoice.OTCmlId];
                                if (null != efs_AllocatedInvoice)
                                {
                                    #region Chargement de la facture alloué
                                    // Recherche du nombre d'événement potentiels à inserer (NB jeton : SQLUP.GetId)
                                    // 1 -> ALD - CSH (Côté facture)
                                    // 3 -> LPP - NTO/NTI/NTA STL
                                    // 1 -> LPP - FXP/FXL
                                    // 3 -> LPP - TXO/TXI/TXA STL
                                    // n -> LPP - TAX enfants de TXx
                                    // 1 -> ALD - CSH (Côté règlement)
                                    m_DsTradeAllocatedInvoice = new DataSetTrade(m_InvoicingGenProcess.Cs, efs_AllocatedInvoice.invoiceIdT);
                                    m_DsEventsAllocatedInvoice = new DataSetEventTrade(m_InvoicingGenProcess.Cs, efs_AllocatedInvoice.invoiceIdT);
                                    int nbTaxDet = GetNbTaxDet();
                                    int nbIdE = 5 + (3 + (3 * nbTaxDet));
                                    #endregion Chargement de la facture alloué
                                    codeReturn = SQLUP.GetId(out int newIdE, m_InvoicingGenProcess.Cs, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbIdE);
                                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                                        codeReturn = AddInvoiceEvents(dbTransaction, efs_AllocatedInvoice, newIdE, nbTaxDet);
                                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                                        AddAllocatedInvoiceDatesEvent(newIdE, newIdE + 1, m_DsEvents, rowInvoiceSettlement, efs_AllocatedInvoice);
                                }
                                #endregion Nouvelle allocation
                            }
                        }
                        #region Mise à jour du montant en attente d'allocation
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = SetUnallocatedAmountEvent(rowInvoiceSettlement);
                        #endregion Mise à jour du montant en attente d'allocation
                        #region Mise à jour du montant d'écart de change
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = SetFxGainOrLossAmountEvent(rowInvoiceSettlement);
                        #endregion Mise à jour du montant d'écart de change
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = UpdateGen(dbTransaction);

                        #region END Transaction (Commit)
                        try { DataHelper.CommitTran(dbTransaction); }
                        catch (Exception ex)
                        {
                            throw new TradeCommonCaptureGenException("InvoicingSettlement_AllocatedInvoice..Generate", ex, TradeCommonCaptureGen.ErrorLevel.COMMIT_ERROR);
                        }
                        #endregion END Transaction (Commit)
                    }
                }
                return codeReturn;
            }
            catch (Exception)
            {
                isException = true;
                codeReturn = Cst.ErrLevel.FAILURE;

                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5223), 0, new LogParam(LogTools.IdentifierAndId(Queue.identifier, Queue.id))));

                throw;
            }
            finally
            {
                if (isException)
                {
                    try {DataHelper.RollbackTran(dbTransaction);}
                    catch { }
                }
            }
        }
        #endregion Generate
        #region GetNbTaxDet
        private int GetNbTaxDet()
        {
            int nbTaxDet = 0;
            DataRow rowINV = GetRowInvoicePeriod;
            if (null != rowINV)
            {
                DataRow rowNTOAmount = GetRowNetTurnOver(rowINV,EventTypeFunc.NetTurnOverAmount);
                if (null != rowNTOAmount)
                {
                    DataRow[] rowTXOAmount = rowNTOAmount.GetChildRows(m_DsEventsAllocatedInvoice.ChildEvent);
                    if ((null != rowTXOAmount) && (1 == rowTXOAmount.Length))
                        nbTaxDet = rowTXOAmount[0].GetChildRows(m_DsEventsAllocatedInvoice.ChildEvent).Length;
                }
            }
            return nbTaxDet;
        }
        #endregion GetNbTaxDet
        #region GetRowNetTurnOver
        public DataRow GetRowNetTurnOver(DataRow pRowParent, string pEventType)
        {
            DataRow[] rows = m_DsEventsAllocatedInvoice.DtEvent.Select(StrFunc.AppendFormat(@"EVENTCODE = '{0}' and EVENTTYPE = '{1}' and IDE_EVENT = {2}",
                EventCodeFunc.LinkedProductPayment, pEventType, Convert.ToInt32(pRowParent["IDE"])), "IDE");
            if ((null != rows) && (0 < rows.Length))
                return rows[0];
            return null;
        }
        #endregion GetRowNetTurnOver
        #region GetAllRowsNetTurnOverAmount
        // EG 20120215 Exclusion des montants alloués désactivés
        public DataRow[] GetAllRowsNetTurnOverAmount(string pEventType)
        {
            return m_DsEventsAllocatedInvoice.DtEvent.Select(StrFunc.AppendFormat(@"EVENTCODE = '{0}' and EVENTTYPE = '{1}' and IDSTACTIVATION = '{2}'",
                EventCodeFunc.LinkedProductPayment, pEventType, Cst.StatusActivation.REGULAR), "IDE");
        }
        #endregion GetAllRowsNetTurnOverAmount
        #region GetTotalCreditNoteAmount
        public decimal GetTotalCreditNoteAmount()
        {
            decimal amount = 0;
            DataRow[] rows = m_DsEventsAllocatedInvoice.GetRowsCreditNoteDates();
            if ((null != rows) && (0 < rows.Length))
            {
                foreach (DataRow row in rows)
                {
                    amount += Convert.ToDecimal(row["VALORISATION"]);
                }
            }
            return amount;
        }
        #endregion GetTotalCreditNoteAmount

        #region GetTotalAllocatedNetTurnOverAmount
        public decimal GetTotalAllocatedNetTurnOverAmount(string pEventType)
        {
            decimal amount = 0;
            DataRow[] rowAllocatedNetTurnOverAmount = GetAllRowsNetTurnOverAmount(pEventType);
            if ((null != rowAllocatedNetTurnOverAmount) && (0 < rowAllocatedNetTurnOverAmount.Length))
            {
                foreach (DataRow rowAllocated in rowAllocatedNetTurnOverAmount)
                {
                    DataRow[] rowClass = rowAllocated.GetChildRows(m_DsEventsAllocatedInvoice.ChildEventClass);
                    if (null!= rowClass)
                    {
                        foreach (DataRow row in rowClass)
                        {
                            if (EventClassFunc.IsSettlement(row["EVENTCLASS"].ToString()))
                            {
                                amount += Convert.ToDecimal(rowAllocated["VALORISATION"]);
                                break;
                            }
                        }
                    }
                        
                }
            }
            return amount;
        }
        #endregion GetTotalAllocatedNetTurnOverAmount

        #region IsFullyAllocated
        public bool IsFullyAllocated(DataRow pRowParent, EFS_AllocatedInvoice pEfs_AllocatedInvoice)
        {

            DataRow rowInitNTIAmount = GetRowNetTurnOver(pRowParent,EventTypeFunc.NetTurnOverIssueAmount);
            decimal initalNetTurnOverIssueAmount = Convert.ToDecimal(rowInitNTIAmount["VALORISATION"]);
            decimal currentAllocatedNetTurnOverIssueAmount = pEfs_AllocatedInvoice.issueAmount.Amount.DecValue;
            decimal totalCreditNoteAmount = GetTotalCreditNoteAmount();
            decimal totalAllocatedAmount = GetTotalAllocatedNetTurnOverAmount(EventTypeFunc.NetTurnOverIssueAmount);
            return (initalNetTurnOverIssueAmount == (currentAllocatedNetTurnOverIssueAmount + totalAllocatedAmount + totalCreditNoteAmount));
        }
        #endregion IsFullyAllocated
        #region SetFxGainOrLossAmountEvent
        // 20090408 EG Math.Abs sur FXP/FXL
        // EG 20150706 [21021] Nullable<int> IDA_PAY|IDB_PAY|IDA_REC|IDB_REC
        private Cst.ErrLevel SetFxGainOrLossAmountEvent(DataRow pRowInvoiceSettlement)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            // EG 20160404 Migration vs2013
            //int tmpInt = 0;
            DataRow row = GetRowProfitAndLoss;
            IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)m_TradeLibrary.Product;
            if (invoiceSettlement.Efs_InvoiceSettlement.fxGainOrLossAmountSpecified)
            {
                if (null == row)
                {
                    codeReturn = SQLUP.GetId(out int idE, m_InvoicingGenProcess.Cs, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        row = m_DsEvents.DtEvent.NewRow();
                        row.ItemArray = (object[])pRowInvoiceSettlement.ItemArray.Clone();
                        row.BeginEdit();
                        row["IDE"] = idE;
                        row["IDE_EVENT"] = pRowInvoiceSettlement["IDE"];
                        row["EVENTCODE"] = EventCodeFunc.LinkedProductPayment;
                        row.EndEdit();
                        m_DsEvents.DtEvent.Rows.Add(row);
                        SetRowStatus(row,m_DsEvents, TuningOutputTypeEnum.OES);
                        AddAllocatedInvoiceEventClass(idE, m_DsEvents.DtEventClass, EventClassFunc.Recognition, invoiceSettlement.Efs_InvoiceSettlement.receptionDate.DateValue);
                    }
                }
                row.BeginEdit();
                IMoney fxAmount = invoiceSettlement.Efs_InvoiceSettlement.fxGainOrLossAmount;
                row["EVENTTYPE"] = ((0 <= fxAmount.Amount.DecValue) ? EventTypeFunc.ForeignExchangeProfit : EventTypeFunc.ForeignExchangeLoss);
                string payer = invoiceSettlement.Efs_InvoiceSettlement.payerPartyReference.HRef;
                string receiver = invoiceSettlement.Efs_InvoiceSettlement.receiverPartyReference.HRef;
                // EG 20150706 [21021]
                Nullable<int> idA_Payer = m_TradeLibrary.DataDocument.GetOTCmlId_Party(payer);
                Nullable<int> idB_Payer = m_TradeLibrary.DataDocument.GetOTCmlId_Book(payer);
                Nullable<int> idA_Receiver = m_TradeLibrary.DataDocument.GetOTCmlId_Party(receiver);
                Nullable<int> idB_Receiver = m_TradeLibrary.DataDocument.GetOTCmlId_Book(receiver); 

                if (0 > fxAmount.Amount.DecValue)
                {
                    row["IDA_PAY"] = (idA_Receiver ?? Convert.DBNull);
                    row["IDB_PAY"] = (idB_Receiver ?? Convert.DBNull);
                    row["IDA_REC"] = (idA_Payer ?? Convert.DBNull);
                    row["IDB_REC"] = (idB_Payer ?? Convert.DBNull);
                }
                else
                {
                    row["IDA_PAY"] = idA_Payer ?? Convert.DBNull;
                    row["IDB_PAY"] = (idB_Payer ?? Convert.DBNull);
                    row["IDA_REC"] = (idA_Receiver ?? Convert.DBNull);
                    row["IDB_REC"] = (idB_Receiver ?? Convert.DBNull);
                }
                // 20090408 EG Math.Abs sur FXP/FXL
                row["VALORISATION"] = Math.Abs(fxAmount.Amount.DecValue);
                row["UNIT"] = fxAmount.Currency;
                // 20090408 EG Math.Abs sur FXP/FXL
                row["VALORISATIONSYS"] = Math.Abs(fxAmount.Amount.DecValue);
                row["UNITSYS"] = fxAmount.Currency;
                row.EndEdit();
            }
            else if (null != row)
            {
                row.BeginEdit();
                row["VALORISATION"] = 0;
                row["VALORISATIONSYS"] = 0;
                row.EndEdit();
            }
            return codeReturn;
        }
        #endregion SetFxGainOrLossAmountEvent
        #region SetUnallocatedAmountEvent
        private Cst.ErrLevel SetUnallocatedAmountEvent(DataRow pRowInvoiceSettlement)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            DataRow row = GetRowUnallocatedAmount;
            IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)m_TradeLibrary.Product;
            if (invoiceSettlement.Efs_InvoiceSettlement.unallocatedAmountSpecified)
            {
                if (null == row)
                {
                    codeReturn = SQLUP.GetId(out int idE, m_InvoicingGenProcess.Cs, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        row = m_DsEvents.DtEvent.NewRow();
                        row.ItemArray = (object[])pRowInvoiceSettlement.ItemArray.Clone();
                        row.BeginEdit();
                        row["IDE"] = idE;
                        row["IDE_EVENT"] = pRowInvoiceSettlement["IDE"];
                        row["EVENTCODE"] = EventCodeFunc.LinkedProductPayment;
                        row["EVENTTYPE"] = EventTypeFunc.NonAllocatedAmount;
                        row.EndEdit();
                        m_DsEvents.DtEvent.Rows.Add(row);
                        SetRowStatus(row, m_DsEvents, TuningOutputTypeEnum.OES);
                        AddAllocatedInvoiceEventClass(idE, m_DsEvents.DtEventClass, EventClassFunc.Recognition, invoiceSettlement.Efs_InvoiceSettlement.receptionDate.DateValue);
                    }
                }
                row.BeginEdit();
                IMoney unallocatedAmount = invoiceSettlement.Efs_InvoiceSettlement.unallocatedAmount;
                row["VALORISATION"] = unallocatedAmount.Amount.DecValue;
                row["VALORISATIONSYS"] = unallocatedAmount.Amount.DecValue;
                row.EndEdit();
            }
            else if (null != row)
            {
                row.BeginEdit();
                row["VALORISATION"] = 0;
                row["VALORISATIONSYS"] = 0;
                row.EndEdit();
            }
            return codeReturn;
        }
        #endregion SetUnallocatedAmountEvent

        #region Update
        // EG 20100225 Add UpdateTradeXML
        protected override TradeCommonCaptureGen.ErrorLevel Update(IDbTransaction pDbTransaction)
        {
            TradeCommonCaptureGen.ErrorLevel ret = UpdateStUsedByForEndOfAllocation();
            if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == ret)
            {
                DataTable dtChanges = DtEventAllocatedInvoice.GetChanges();
                m_DsTradeAllocatedInvoice.UpdateTradeStSys(pDbTransaction);
                // EG 20100225 Add UpdateTradeXML
                m_DsTradeAllocatedInvoice.UpdateTradeXML(pDbTransaction);
                m_DsEventsAllocatedInvoice.Update(pDbTransaction);
                if (null != dtChanges)
                {
                    DateTime dtSysBusiness = OTCmlHelper.GetDateSys(m_InvoicingGenProcess.Cs);
                    foreach (DataRow row in dtChanges.Rows)
                    {
                        // 20081107 EG Pas de mise à jour sur Row Deleted
                        if (DataRowState.Deleted != row.RowState)
                        {
                            int idE = Convert.ToInt32(row["IDE"]);
                            m_EventProcess.Write(pDbTransaction, idE, Cst.ProcessTypeEnum.INVOICINGGEN, ProcessStateTools.StatusSuccessEnum, 
                                dtSysBusiness, m_InvoicingGenProcess.Tracker.IdTRK_L);
                        }
                    }
                }
            }
            return ret;
        }
        #endregion Update
        #region UpdateGen
        protected Cst.ErrLevel UpdateGen(IDbTransaction pDbTransaction)
        {
            DataTable dtChanges = DtEvent.GetChanges();
            m_DsEvents.Update(pDbTransaction);
            if (null != dtChanges)
            {
                DateTime dtSys = OTCmlHelper.GetDateSysUTC(m_InvoicingGenProcess.Cs);
                foreach (DataRow row in dtChanges.Rows)
                {
                    // 20081107 EG Pas de mise à jour sur Row Deleted
                    if (DataRowState.Deleted != row.RowState)
                    {
                        int idE = Convert.ToInt32(row["IDE"]);
                        m_EventProcess.Write(pDbTransaction, idE, Cst.ProcessTypeEnum.INVOICINGGEN, ProcessStateTools.StatusSuccessEnum, dtSys, m_InvoicingGenProcess.Tracker.IdTRK_L);
                    }
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UpdateGen
        #region UpdateStUsedByForEndOfAllocation
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected TradeCommonCaptureGen.ErrorLevel UpdateStUsedByForEndOfAllocation()
        {
            DataRow rowTradeStSys = m_DsTradeAllocatedInvoice.DtTrade.Rows[0];
            rowTradeStSys.BeginEdit();
            rowTradeStSys["IDSTUSEDBY"] = Cst.StatusActivation.REGULAR.ToString();
            rowTradeStSys["IDASTUSEDBY"] = m_InvoicingGenProcess.Session.IdA;
            rowTradeStSys["DTSTUSEDBY"] = OTCmlHelper.GetDateSysUTC(m_InvoicingGenProcess.Cs);
            rowTradeStSys["LIBSTUSEDBY"] = Convert.DBNull;
            rowTradeStSys.EndEdit();
            return TradeCommonCaptureGen.ErrorLevel.SUCCESS;
        }
        #endregion UpdateStUsedByForEndOfAllocation
        #endregion Methods

    }
}

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
    /// <summary>
    /// Classe utilisée pour les opération de correction d'une facture
    /// Une correction de facture s'opère sur la facture initiale par modification d'un ou plusieurs montants de frais
    /// Pour cela une facture théorique est "générée" avec ces nouveaux montants et un differentiel avec la facture initiale ou intermédiaire (si présence de factures additionnelles) est opéré
    /// pour déterminer s'il y aur génération d'un avoir (baisse du montant total de frais) ou d'une facture additionnelle (augmentation du montant total de frais) 
    /// </summary>
    public class InvoicingGen_Correction : InvoicingGenProcessBase
    {
        #region Members
        private readonly DataSetTrade m_DsTrade;
        private readonly DataSetEventTrade m_DsEvents;
        private readonly EFS_TradeLibrary m_TradeLibrary;
        private DataSet m_DsInvoicingRules;
        // EG 20101020 Ticket 17185 : Add Dataset to Update elementary TRADE/EVENTS sources of invoice
        private EFS_TradeLibrary m_TradeLibrarySource;
        private DataSetTrade m_DsTradeSource;
        private DataSetEventTrade m_DsEventsSource;
        #endregion Members
        #region Accessors
        #region ActionDate
        public override DateTime ActionDate
        {
            get 
            {
                DateTime dtAction = DateTime.MinValue;
                InvoicingCorrectionGenMQueue invoiceCorrection = (InvoicingCorrectionGenMQueue)Queue;
                if (invoiceCorrection.itemSpecified && (0 < invoiceCorrection.item.Length))
                {
                    if (ArrFunc.IsFilled(invoiceCorrection.item[0].actionMsgs))
                    {
                        InvoicingDetailMsg invoicingDetailEvent = (InvoicingDetailMsg)invoiceCorrection.item[0].actionMsgs[0];
                        if (null != invoicingDetailEvent)
                            dtAction = invoicingDetailEvent.actionDate.Date;
                    }
                }
                return dtAction; 
            }
        }
        #endregion ActionDate
        #region BracketByInvoicingRules
        public DataRelation BracketByInvoicingRules
        {
            get
            {
                DataRelation ret = null;
                if (m_DsInvoicingRules.Relations.Contains("BracketByInvoicingRules"))
                    ret = m_DsInvoicingRules.Relations["BracketByInvoicingRules"];
                return ret;
            }
        }
        #endregion BracketByInvoicingRules
        #region CurrentInvoicingRules
        public int CurrentInvoicingRules
        {
            get {return ((IInvoice)m_TradeLibrary.CurrentTrade.Product).Scope.OTCmlId;}
        }
        #endregion CurrentInvoicingRules
        #region DtInvoicingRules
        public DataTable DtInvoicingRules
        {
            get
            {
                DataTable ret = null;
                if ((null != m_DsInvoicingRules) && m_DsInvoicingRules.Tables.Contains("InvoicingRules"))
                    ret = m_DsInvoicingRules.Tables["InvoicingRules"];
                return ret;
            }
        }
        #endregion DtInvoicingRules
        #region DtInvRulesBracket
        public DataTable DtInvRulesBracket
        {
            get
            {
                DataTable ret = null;
                if ((null != m_DsInvoicingRules) && m_DsInvoicingRules.Tables.Contains("InvRulesBracket"))
                    ret = m_DsInvoicingRules.Tables["InvRulesBracket"];
                return ret;
            }
        }
        #endregion DtInvRulesBracket
        #region Entity
        // EG 20150706 [21021] Nullable<int>
        // EG 20240205 [WI640] L'entité est toujours en premiere postion dans Party
        public override Nullable<int> Entity
        {
            get
            {
                //string hRef = ((IInvoice)m_TradeLibrary.CurrentTrade.Product).ReceiverPartyReference.HRef;
                //return m_TradeLibrary.DataDocument.GetOTCmlId_Party(hRef);
                return m_TradeLibrary.Party[0].OTCmlId;
            }
        }
        #endregion Entity
        #region GetRowInvoicePeriod
        public DataRow GetRowInvoicePeriod
        {
            get
            {   DataRow[] rows = m_DsEvents.DtEvent.Select("EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.InvoicingDates));
                if (null != rows)
                    return rows[0];
                return null;
            }
        }
        #endregion GetRowInvoicePeriod
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
        #region TradeAdminTitle
        public override string TradeAdminTitle(string pValue, InvoicingScope pScope)
        {
            return base.TradeAdminTitle(m_DsTrade.Identifier, pScope);
        }
        #endregion TradeAdminTitle
        #endregion Accessors
        #region Constructors
        // EG 20190613 [24683] Use slaveDbTransaction
        public InvoicingGen_Correction(InvoicingGenProcess pInvoicingGenProcess)
            : base(pInvoicingGenProcess)
		{
            m_DsTrade = new DataSetTrade(ProcessBase.Cs, ProcessBase.SlaveDbTransaction, ProcessBase.CurrentId);
            m_DsEvents = new DataSetEventTrade(ProcessBase.Cs, ProcessBase.SlaveDbTransaction, ProcessBase.CurrentId);
            m_EventProcess = new EventProcess(ProcessBase.Cs);
            m_TradeLibrary = new EFS_TradeLibrary(ProcessBase.Cs, ProcessBase.SlaveDbTransaction, m_DsTrade.IdT);
		}
		#endregion Constructors
        #region Methods
        #region AddPaymentCorrectionInTradeXML
        // EG 20101020 Ticket 17185 : Add Payment in XML Trade with CORRECTED status
        // EG 20150706 [21021] Nullable<int> for idA_Pay|idA_REc
        // FI 20170306 [22225] Modify
        private void AddPaymentCorrectionInTradeXML(EFS_TradeLibrary pTradeLibrary, DataRow pRowFeeBefore, DataRow pRowFeeAfter)
        {
            if (pTradeLibrary.CurrentTrade.OtherPartyPaymentSpecified)
            {
                for (int i = 0; i < pTradeLibrary.CurrentTrade.OtherPartyPayment.Length; i++)
                {
                    IPayment payment = pTradeLibrary.CurrentTrade.OtherPartyPayment[i];
                    if (Tools.IsPaymentSourceScheme(payment, Cst.OTCml_RepositoryFeeInvoicingScheme) && payment.PaymentDateSpecified)
                    {
                        string eventType = payment.GetPaymentType(pRowFeeBefore["EVENTCODE"].ToString());
                        decimal amount = Convert.ToDecimal(pRowFeeBefore["VALORISATION"]);
                        DateTime dtPayment = Convert.ToDateTime(pRowFeeBefore["DTSTARTADJ"]);
                        string currency = pRowFeeBefore["UNIT"].ToString();
                        if ((pRowFeeBefore["EVENTTYPE"].ToString() == eventType) &&
                            (payment.PaymentAmount.Amount.DecValue == amount) &&
                            (payment.PaymentAmount.Currency == currency) &&
                            (payment.PaymentDate.UnadjustedDate.DateValue == dtPayment))
                        {
                            // EG 20150706 [21021]
                            Nullable<int> idA_Pay = m_TradeLibrarySource.DataDocument.GetOTCmlId_Party(payment.PayerPartyReference.HRef);
                            Nullable<int> idA_Rec = m_TradeLibrarySource.DataDocument.GetOTCmlId_Party(payment.ReceiverPartyReference.HRef);
                            if (idA_Pay.HasValue && idA_Rec.HasValue &&
                               (idA_Pay.Value == Convert.ToInt32(pRowFeeBefore["IDA_PAY"])) &&
                               (idA_Rec.Value == Convert.ToInt32(pRowFeeBefore["IDA_REC"])))
                            {
                                if (EfsML.Enum.SpheresSourceStatusEnum.Corrected == payment.PaymentSource.Status)
                                {
                                    // Mise à jour du montant seulement 
                                    payment.PaymentAmount.Amount.DecValue = Convert.ToDecimal(pRowFeeAfter["VALORISATION"]);
                                }
                                else
                                {
                                    // RAZ du montant de la ligne de frais source
                                    payment.PaymentAmount.Amount.DecValue = 0;
                                    // Création d'une nouvelle ligne corrective à insérer juste après la ligne source
                                    IPayment _payment = pTradeLibrary.DataDocument.CurrentProduct.ProductBase.CreatePayment();
                                    _payment.PayerPartyReference.HRef = payment.PayerPartyReference.HRef;
                                    _payment.ReceiverPartyReference.HRef = payment.ReceiverPartyReference.HRef;
                                    _payment.PaymentTypeSpecified = payment.PaymentTypeSpecified;
                                    _payment.PaymentType.Value = payment.PaymentType.Value;
                                    _payment.PaymentAmount.Amount.DecValue = Convert.ToDecimal(pRowFeeAfter["VALORISATION"]);
                                    _payment.PaymentAmount.Currency = payment.PaymentAmount.Currency;
                                    _payment.PaymentDateSpecified = payment.PaymentDateSpecified;
                                    _payment.PaymentDate = (IAdjustableDate)payment.PaymentDate.Clone();
                                    _payment.PaymentQuoteSpecified = false;
                                    _payment.PaymentSourceSpecified = true;
                                    _payment.PaymentSource.StatusSpecified = true;
                                    _payment.PaymentSource.Status = EfsML.Enum.SpheresSourceStatusEnum.Corrected;
                                    _payment.PaymentSource.SpheresId = m_TradeLibrarySource.DataDocument.CurrentProduct.ProductBase.CreateSpheresId(2);
                                    _payment.PaymentSource.SpheresId[0].Scheme = Cst.OTCml_RepositoryFeeInvoicingScheme;
                                    _payment.PaymentSource.SpheresId[0].Value = Cst.FpML_Boolean_True;
                                    _payment.PaymentSource.SpheresId[1].Scheme = Cst.OTCml_RepositoryFeeEventTypeScheme;
                                    _payment.PaymentSource.SpheresId[1].Value = eventType;
                                    // Insertion de la ligne corrective juste après la ligne source
                                    ArrayList aPayment = new ArrayList();
                                    aPayment.AddRange(pTradeLibrary.CurrentTrade.OtherPartyPayment);
                                    aPayment.Insert(i + 1, _payment);
                                    // FI 20170306 [22225] call dataDocument.SetOtherPartyPayment
                                    //pTradeLibrary.currentTrade.SetOtherPartyPayment(aPayment);
                                    pTradeLibrary.DataDocument.SetOtherPartyPayment((IPayment[])aPayment.ToArray(typeof(IPayment)));
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion AddPaymentCorrectionInTradeXML
        #region AddTradeActionEventOnInvoiceSource
        //EG 20100331 [Cast product]
        private TradeCommonCaptureGen.ErrorLevel AddTradeActionEventOnInvoiceSource()
        {
            DataRow rowInvoiceDates = GetRowInvoicePeriod;
            if ((null != rowInvoiceDates) &&
                (Cst.ErrLevel.SUCCESS == SQLUP.GetId(out int newIdE, ProcessBase.Cs, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1)))
            {
                InvoicingScope scope = m_Invoicing.scopes[0];
                //EG 20100331 [Cast product]
                IInvoiceSupplement finalInvoice = (IInvoiceSupplement)scope.DataDocument.CurrentProduct.Product;

                #region Event
                DateTime dtAction = ActionDate;
                DataRow row = m_DsEvents.DtEvent.NewRow();
                row.ItemArray = (object[])rowInvoiceDates.ItemArray.Clone();
                row.BeginEdit();
                row["IDE"] = newIdE;
                row["IDE_EVENT"] = Convert.ToInt32(rowInvoiceDates["IDE"]);
                row["EVENTCODE"] = CorrectionInvoiceDates;
                row["EVENTTYPE"] = EventTypeFunc.Period;
                row["VALORISATION"] = finalInvoice.NetTurnOverIssueAmount.Amount.DecValue;
                row["UNITTYPE"] = UnitTypeEnum.Currency;
                row["UNIT"] = finalInvoice.NetTurnOverIssueAmount.Currency;
                row["VALORISATIONSYS"] = finalInvoice.NetTurnOverIssueAmount.Amount.DecValue;
                row["UNITTYPESYS"] = UnitTypeEnum.Currency;
                row["UNITSYS"] = finalInvoice.NetTurnOverIssueAmount.Currency;

                row["IDSTTRIGGER"] = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
                row["IDSTCALCUL"] = StatusCalculFunc.Calculated;
                row["SOURCE"] = ProcessBase.AppInstance.ServiceName;
                row.EndEdit();
                m_DsEvents.DtEvent.Rows.Add(row);
                #endregion Event
                #region EventClass
                DataRow rowClass = m_DsEvents.DtEventClass.NewRow();
                DataRow[] rowClassInvoiceDates = rowInvoiceDates.GetChildRows(m_DsEvents.ChildEventClass);
                if (null != rowClassInvoiceDates)
                {
                    rowClass.ItemArray = (object[])rowClassInvoiceDates[0].ItemArray.Clone();
                    rowClass.BeginEdit();
                    rowClass["IDE"] = newIdE;
                    rowClass["EVENTCLASS"] = EventClassFunc.GroupLevel;
                    rowClass["DTEVENT"] = dtAction;
                    rowClass["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(ProcessBase.Cs, dtAction);
                    rowClass["ISPAYMENT"] = false;
                    rowClass.EndEdit();
                    m_DsEvents.DtEventClass.Rows.Add(rowClass);
                }
                #endregion EventClass
            }
            return TradeCommonCaptureGen.ErrorLevel.SUCCESS;
        }
        #endregion AddTradeActionEventOnInvoiceSource        
        #region GetRowFeeAmount
        public DataRow GetRowFeeAmount(int pIdE)
        {
            DataRow[] rows = m_DsEvents.DtEvent.Select("IDE_SOURCE=" + pIdE.ToString());
            if (null != rows)
                return rows[0];
            return null;
        }
        #endregion GetRowFeeAmount
        #region GetRowSourceFeeAmount
        // EG 20101020 Ticket 17185
        public DataRow GetRowSourceFeeAmount(int pIdE)
        {
            DataRow[] rows = m_DsEventsSource.DtEvent.Select("IDE=" + pIdE.ToString());
            if (null != rows)
                return rows[0];
            return null;
        }
        #endregion GetRowSourceFeeAmount
        #region UpdateCorrectedFeesAmountEventOnInvoiceSource
        /// <summary>
        /// Mise à jour des montants de frais corrigés sur une facture 
        /// - Frais en devise comptable par prorata
        /// - Ajout de l'éventuelle notes sur l'évenement corrigé 
        /// </summary>
        /// <returns></returns>
        // 20090420 EG
        // EG 201106015 Ticket : 17480 
        private TradeCommonCaptureGen.ErrorLevel UpdateCorrectedFeesAmountEventOnInvoiceSource()
        {
            InvoicingScope scope = m_Invoicing.scopes[0];
            IInvoice invoice = (IInvoice)m_TradeLibrary.CurrentTrade.Product;
            foreach (InvoicingTrade trade in scope.trades)
            {
                if (trade.isInvoicingEventIsCorrected)
                {
                    foreach (InvoicingEvent _event in trade.events)
                    {
                        //EG 20091110
                        decimal previousAmount = _event.baseAmount; 
                        decimal previousAccountingAmount = 0;
                        decimal prorataAmount = 0;
                        if (_event.isAmountIsCorrected)
                        {
                            DataRow rowFeeAmount = GetRowFeeAmount(_event.idE);
                            if (null != rowFeeAmount)
                            {
                                // Events
                                previousAmount = Convert.ToDecimal(rowFeeAmount["VALORISATION"]);
                                rowFeeAmount.BeginEdit();
                                rowFeeAmount["VALORISATION"] = _event.amount;
                                rowFeeAmount.EndEdit();

                                DataRow[] rowFeeAccountingAmount = rowFeeAmount.GetChildRows(m_DsEvents.ChildEvent);
                                if (ArrFunc.IsFilled(rowFeeAccountingAmount))
                                {
                                    if (false == Convert.IsDBNull(rowFeeAccountingAmount[0]["VALORISATION"]))
                                        previousAccountingAmount = Convert.ToDecimal(rowFeeAccountingAmount[0]["VALORISATION"]);

                                    // EG 201106015 Ticket : 17480 
                                    if (0 == previousAmount)
                                    {
                                        if (0 < _event.initialAmount)
                                            prorataAmount = (_event.amount / _event.initialAmount) * _event.initialAccountingAmount;
                                        else
                                            prorataAmount = _event.amount * previousAccountingAmount;
                                    }
                                    else
                                        prorataAmount = (_event.amount / previousAmount) * previousAccountingAmount;

                                    EFS_Cash cash = new EFS_Cash(ProcessBase.Cs, prorataAmount, _event.accountingCurrency);
                                    _event.accountingAmount = cash.AmountRounded;
                                    rowFeeAccountingAmount[0].BeginEdit();
                                    rowFeeAccountingAmount[0]["VALORISATION"] = _event.accountingAmount;
                                    rowFeeAccountingAmount[0].EndEdit();
                                }
                            }
                            // EG 20101020 Ticket 17185
                            if (_event.noteSpecified)
                                SetNoteCorrectionToNotePad(m_DsTrade.IdT, m_DsTrade.DtTradeNotepad, trade,_event);

                        }
                        // Fee dans TradeXML
                        IInvoiceTrade invoiceTrade = invoice.InvoiceDetails.GetInvoiceTrade(trade.idT);
                        if (null != invoiceTrade)
                        {
                            IInvoiceFee invoiceFee = invoiceTrade.InvoiceFee(_event.idE);
                            if (null != invoiceFee)
                            {
                                invoiceFee.FeeAmount.Amount.DecValue = _event.amount;
                                // EG 20091126 Test Nullable
                                if (null != invoiceFee.FeeAccountingAmount)
                                    invoiceFee.FeeAccountingAmount.Amount.DecValue = _event.accountingAmount;
                                // 20091126 EG Test Nullable
                                invoiceFee.FeeBaseAmount.Amount.DecValue = previousAmount;
                                // EG 201106015 Ticket : 17480 
                                //if ((0 < previousAccountingAmount) && (null != invoiceFee.feeBaseAccountingAmount))
                                if (null != invoiceFee.FeeBaseAccountingAmount)
                                    invoiceFee.FeeBaseAccountingAmount.Amount.DecValue = previousAccountingAmount;
                            }
                        }
                    }
                }
                // EG 20091110
                else if (scope.isInvoicingEventIsCorrected)
                {
                    IInvoiceTrade invoiceTrade = invoice.InvoiceDetails.GetInvoiceTrade(trade.idT);
                    if (null != invoiceTrade)
                    {
                        foreach (InvoicingEvent _event in trade.events)
                        {
                            IInvoiceFee invoiceFee = invoiceTrade.InvoiceFee(_event.idE);
                            if (null != invoiceFee)
                            {
                                invoiceFee.FeeBaseAmount.Amount.DecValue = _event.amount;
                                // 20091126 EG Test Nullable
                                if (null != invoiceFee.FeeBaseAccountingAmount)
                                    invoiceFee.FeeBaseAccountingAmount.Amount.DecValue = _event.accountingAmount;
                            }
                        }
                    }
                }
            }
            return TradeCommonCaptureGen.ErrorLevel.SUCCESS;
        }
        #endregion UpdateCorrectedFeesAmountEventOnInvoiceSource
        #region UpdateCorrectedFeesAmountElementaryTradesAndEvents
        // EG 20101020 Ticket 17185 : Update elementary TRADE/EVENTS sources of invoice
        // EG 20190613 [24683] Use slaveDbTransaction
        // EG 20200914 [25077] RDBMS : Correction DtTradeXML
        private TradeCommonCaptureGen.ErrorLevel UpdateCorrectedFeesAmountElementaryTradesAndEvents(IDbTransaction pDbTransaction)
        {
            InvoicingScope scope = m_Invoicing.scopes[0];
            foreach (InvoicingTrade trade in scope.trades)
            {
                if (trade.isInvoicingEventIsCorrected)
                {
                    m_DsTradeSource = new DataSetTrade(ProcessBase.Cs, pDbTransaction, trade.idT);
                    m_TradeLibrarySource = new EFS_TradeLibrary(ProcessBase.Cs, pDbTransaction, m_DsTradeSource.IdT);
                    m_DsEventsSource = new DataSetEventTrade(ProcessBase.Cs, pDbTransaction, trade.idT);
                    // EG 20101020 Ticket 17185
                    foreach (InvoicingEvent _event in trade.events)
                    {
                        if (_event.isAmountIsCorrected)
                        {
                            // Evénement côté trade avant correction
                            DataRow rowSourceFeeAmount = GetRowSourceFeeAmount(_event.idE);
                            // Evénement côté facture après correction
                            DataRow rowFeeAmount = GetRowFeeAmount(_event.idE);
                            if ((null != rowFeeAmount) && (null != rowSourceFeeAmount))
                            {
                                // EG 20101020 Ticket 17185 : Add Corrected Payment in XML Trade
                                AddPaymentCorrectionInTradeXML(m_TradeLibrarySource, rowSourceFeeAmount, rowFeeAmount);
                                //
                                #region Event
                                // EG 20101020 Ticket 17185 : Add Corrected Amount in source event
                                rowSourceFeeAmount.BeginEdit();
                                rowSourceFeeAmount["VALORISATION"] = Convert.ToDecimal(rowFeeAmount["VALORISATION"]);
                                rowSourceFeeAmount.EndEdit();
                                #endregion Event
                            }
                            // EG 20101020 Ticket 17185 : Add note to NotePad
                            if (_event.noteSpecified)
                                SetNoteCorrectionToNotePad(trade.idT, m_DsTradeSource.DtTradeNotepad,trade, _event);
                        }
                    }
                    EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(m_TradeLibrarySource.DataDocument.DataDocument);
                    StringBuilder sb = CacheSerializer.Serialize(serializerInfo);
                    m_DsTradeSource.DtTradeXML.Rows[0]["TRADEXML"] = sb.ToString();
                    m_DsTradeSource.UpdateTradeXML(pDbTransaction);

                    // EG 20101020 Ticket 17185 : Update EVENT source
                    m_DsEventsSource.Update(pDbTransaction);
                    // EG 20101020 Ticket 17185 : Add note to NotePad
                    m_DsTradeSource.UpdateTradeNotepad(pDbTransaction);
                }
            }
            return TradeCommonCaptureGen.ErrorLevel.SUCCESS;
        }
        #endregion UpdateCorrectedFeesAmountElementaryTradesAndEvents
        #region Generate
        /// <summary>
        /// Traitement de la correction d'une facture
        /// - Alimentation des classes de travail
        /// - Recalcul de la facture avec prise en compte des modifications opérées sur les frais qui la composent
        /// - Détermination du type de document : Facture additionnelle ou Avoir
        /// - Génération et Ecriture de la Facture Additionnelle ou de l'Avoir
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Generate()
        {
            Cst.ErrLevel codeReturn;
            try
            {
                #region Construction des classes de travail
                m_Invoicing = new Invoicing(this);
                codeReturn = SetInvoicingScope();
                #endregion Construction des classes de travail
                #region Recalcul de la facture après prise en compte des modifications
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    AdministrativeInstrumentBase currentInstrument = CurrentInstrument;
                    if (null != currentInstrument)
                        codeReturn = currentInstrument.CreateDataDocument();
                    else
                        codeReturn = Cst.ErrLevel.DATANOTFOUND;
                }

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = SetDataDocument(m_Invoicing.scopes[0]);
                #endregion Recalcul de la facture après prise en compte des modifications
                #region Alimentation finale du document: Facture additionnelle ou Avoir
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = SetFinalInvoice();
                #endregion Alimentation finale du document : Facture additionnelle ou Avoir
                #region Ecriture de la facture
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    Save(m_Invoicing.scopes[0]);
                #endregion Ecriture de la facture
                return codeReturn;
            }
            catch (Exception)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5222), 0,
                    new LogParam(LogTools.IdentifierAndId(Queue.identifier, Queue.id)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(MasterDate))));

                throw;
            }
        }
        #endregion Generate
        #region SetCurrentInvoiceSupplementAmounts
        // EG 20110314 Ticket: 17350
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        // EG 20220406 [XXXXX][WI614] Correction paramètre IdT et refactoring Query
        public void SetCurrentInvoiceSupplementAmounts(IInvoiceSupplement pFinalInvoice)
        {
            InvoicingScope scope = m_Invoicing.scopes[0];
            IProduct product = scope.DataDocument.CurrentProduct.Product;
            decimal amount = pFinalInvoice.InitialInvoiceAmount.NetTurnOverAmount.Amount.DecValue;
            string currency = pFinalInvoice.InitialInvoiceAmount.NetTurnOverAmount.Currency;
            pFinalInvoice.BaseNetInvoiceAmount.Amount = product.ProductBase.CreateMoney(amount, currency);
            amount = pFinalInvoice.InitialInvoiceAmount.NetTurnOverIssueAmount.Amount.DecValue;
            currency = pFinalInvoice.InitialInvoiceAmount.NetTurnOverIssueAmount.Currency;
            pFinalInvoice.BaseNetInvoiceAmount.IssueAmountSpecified = true;
            pFinalInvoice.BaseNetInvoiceAmount.IssueAmount = product.ProductBase.CreateMoney(amount, currency);
            amount = pFinalInvoice.InitialInvoiceAmount.NetTurnOverAccountingAmount.Amount.DecValue;
            currency = pFinalInvoice.InitialInvoiceAmount.NetTurnOverAccountingAmount.Currency;
            pFinalInvoice.BaseNetInvoiceAmount.AccountingAmount = product.ProductBase.CreateMoney(amount, currency);
            // EG 201103 Ticket:17350
            pFinalInvoice.BaseNetInvoiceAmount.AccountingAmountSpecified = pFinalInvoice.InitialInvoiceAmount.NetTurnOverAccountingAmountSpecified;

            if (pFinalInvoice.InitialInvoiceAmount.TaxSpecified)
            {
                amount = pFinalInvoice.InitialInvoiceAmount.Tax.Amount.Amount.DecValue;
                currency = pFinalInvoice.InitialInvoiceAmount.Tax.Amount.Currency;
                pFinalInvoice.BaseNetInvoiceAmount.Tax.Amount = product.ProductBase.CreateMoney(amount, currency);
                amount = pFinalInvoice.InitialInvoiceAmount.Tax.IssueAmount.Amount.DecValue;
                currency = pFinalInvoice.InitialInvoiceAmount.Tax.IssueAmount.Currency;
                pFinalInvoice.BaseNetInvoiceAmount.Tax.IssueAmount = product.ProductBase.CreateMoney(amount, currency);
                pFinalInvoice.BaseNetInvoiceAmount.Tax.IssueAmountSpecified = true;
                pFinalInvoice.BaseNetInvoiceAmount.Tax.AccountingAmountSpecified = pFinalInvoice.InitialInvoiceAmount.Tax.AccountingAmountSpecified;
                if (pFinalInvoice.BaseNetInvoiceAmount.Tax.AccountingAmountSpecified)
                {
                    amount = pFinalInvoice.InitialInvoiceAmount.Tax.AccountingAmount.Amount.DecValue;
                    currency = pFinalInvoice.InitialInvoiceAmount.Tax.AccountingAmount.Currency;
                    pFinalInvoice.BaseNetInvoiceAmount.Tax.AccountingAmount = product.ProductBase.CreateMoney(amount, currency);
                }
            }

            string NTOCode = DataHelper.SQLString(EventTypeFunc.NetTurnOverAmount);
            string NTICode = DataHelper.SQLString(EventTypeFunc.NetTurnOverIssueAmount);
            string NTACode = DataHelper.SQLString(EventTypeFunc.NetTurnOverAccountingAmount);
            string TXOCode = DataHelper.SQLString(EventTypeFunc.TaxAmount);
            string TXICode = DataHelper.SQLString(EventTypeFunc.TaxIssueAmount);
            string TXACode = DataHelper.SQLString(EventTypeFunc.TaxAccountingAmount);

            string LPPCode = DataHelper.SQLString(EventCodeFunc.LinkedProductPayment);
            string RECCode = DataHelper.SQLString(EventClassFunc.Recognition);

            string sqlSelect = $@"select ev.EVENTTYPE, SUM(case pr.IDENTIFIER 
            when 'credit' then -ev.VALORISATION 
            when 'additionalInvoice' then ev.VALORISATION else 0 end) as NETAMOUNT
            from dbo.EVENT ev
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS = {RECCode})
            inner join dbo.TRADE tr on (tr.IDT = ev.IDT)
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
            inner join dbo.TRADELINK tl on (tl.IDT_A = ev.IDT) and (tl.IDT_B = @IDT)
            where (ev.EVENTTYPE in ({NTOCode},{NTICode},{NTACode},{TXOCode},{TXICode},{TXACode})) and (ev.EVENTCODE = {LPPCode})
            group by ev.EVENTTYPE";

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(ProcessBase.Cs, DataParameter.ParameterEnum.IDT), pFinalInvoice.InitialInvoiceAmount.OTCmlId);
            QueryParameters qry = new QueryParameters(ProcessBase.Cs, sqlSelect, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(ProcessBase.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    amount = Convert.ToDecimal(dr["NETAMOUNT"]);
                    string eventType = dr["EVENTTYPE"].ToString();
                    if (EventTypeFunc.IsNetTurnOverAmount(eventType))
                        pFinalInvoice.BaseNetInvoiceAmount.Amount.Amount.DecValue += amount;
                    else if (EventTypeFunc.IsNetTurnOverIssueAmount(eventType))
                        pFinalInvoice.BaseNetInvoiceAmount.IssueAmount.Amount.DecValue += amount;
                    else if (EventTypeFunc.IsNetTurnOverAccountingAmount(eventType))
                        pFinalInvoice.BaseNetInvoiceAmount.AccountingAmount.Amount.DecValue += amount;
                    else if (EventTypeFunc.IsTaxAmount(eventType))
                        pFinalInvoice.BaseNetInvoiceAmount.Tax.Amount.Amount.DecValue += amount;
                    else if (EventTypeFunc.IsTaxIssueAmount(eventType))
                        pFinalInvoice.BaseNetInvoiceAmount.Tax.IssueAmount.Amount.DecValue += amount;
                    else if (EventTypeFunc.IsTaxAccountingAmount(eventType))
                        pFinalInvoice.BaseNetInvoiceAmount.Tax.AccountingAmount.Amount.DecValue += amount;

                }
            }
        }
        #endregion SetCurrentInvoiceSupplementAmounts
        #region SetCurrentInvoiceSupplementAmounts
        // EG 20110314 Ticket: 17350
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        // EG 20220406 [XXXXX][WI614] Correction paramètre IdT et refactoring Query
        public INetInvoiceAmounts SetCurrentInvoiceSupplementAmounts(IInvoice pInitialInvoice, int pOTCmlId)
        {
            InvoicingScope scope = m_Invoicing.scopes[0];
            IProduct product = scope.DataDocument.CurrentProduct.Product;

            INetInvoiceAmounts netInvoiceAmounts = product.ProductBase.CreateNetInvoiceAmounts();
            netInvoiceAmounts.Amount = product.ProductBase.CreateMoney(pInitialInvoice.NetTurnOverAmount.Amount.DecValue, pInitialInvoice.NetTurnOverAmount.Currency);
            netInvoiceAmounts.IssueAmount = product.ProductBase.CreateMoney(pInitialInvoice.NetTurnOverIssueAmount.Amount.DecValue, pInitialInvoice.NetTurnOverIssueAmount.Currency);
            netInvoiceAmounts.IssueAmountSpecified = true;
            netInvoiceAmounts.AccountingAmount = product.ProductBase.CreateMoney(pInitialInvoice.NetTurnOverAccountingAmount.Amount.DecValue, pInitialInvoice.NetTurnOverAccountingAmount.Currency);
            // EG 20110314 Ticket: 17350
            netInvoiceAmounts.AccountingAmountSpecified = pInitialInvoice.NetTurnOverAccountingAmountSpecified;

            netInvoiceAmounts.TaxSpecified = (pInitialInvoice.TaxSpecified);
            if (netInvoiceAmounts.TaxSpecified)
            {
                IInvoiceTax tax = pInitialInvoice.Tax;
                netInvoiceAmounts.Tax.Amount = product.ProductBase.CreateMoney(tax.Amount.Amount.DecValue, tax.Amount.Currency);
                netInvoiceAmounts.Tax.IssueAmountSpecified = true;
                netInvoiceAmounts.Tax.IssueAmount = product.ProductBase.CreateMoney(tax.IssueAmount.Amount.DecValue, tax.IssueAmount.Currency);
                netInvoiceAmounts.Tax.AccountingAmountSpecified = tax.AccountingAmountSpecified;
                if (netInvoiceAmounts.Tax.AccountingAmountSpecified)
                    netInvoiceAmounts.Tax.AccountingAmount = product.ProductBase.CreateMoney(tax.AccountingAmount.Amount.DecValue, tax.AccountingAmount.Currency);
            }

            string NTOCode = DataHelper.SQLString(EventTypeFunc.NetTurnOverAmount);
            string NTICode = DataHelper.SQLString(EventTypeFunc.NetTurnOverIssueAmount);
            string NTACode = DataHelper.SQLString(EventTypeFunc.NetTurnOverAccountingAmount);
            string TXOCode = DataHelper.SQLString(EventTypeFunc.TaxAmount);
            string TXICode = DataHelper.SQLString(EventTypeFunc.TaxIssueAmount);
            string TXACode = DataHelper.SQLString(EventTypeFunc.TaxAccountingAmount);

            string LPPCode = DataHelper.SQLString(EventCodeFunc.LinkedProductPayment);
            string RECCode = DataHelper.SQLString(EventClassFunc.Recognition);

            string sqlSelect = $@"select ev.EVENTTYPE, SUM(case pr.IDENTIFIER 
            when 'credit' then -ev.VALORISATION 
            when 'additionalInvoice' then ev.VALORISATION else 0 end) as NETAMOUNT
            from dbo.EVENT ev
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS = {RECCode})
            inner join dbo.TRADE tr on (tr.IDT = ev.IDT)
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
            inner join dbo.TRADELINK tl on (tl.IDT_A = ev.IDT) and (tl.IDT_B = @IDT)
            where (ev.EVENTTYPE in ({NTOCode},{NTICode},{NTACode},{TXOCode},{TXICode},{TXACode})) and (ev.EVENTCODE = {LPPCode})
            group by ev.EVENTTYPE";

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(ProcessBase.Cs, DataParameter.ParameterEnum.IDT), pOTCmlId);
            QueryParameters qry = new QueryParameters(ProcessBase.Cs, sqlSelect, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(ProcessBase.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    decimal amount = Convert.ToDecimal(dr["NETAMOUNT"]);
                    string eventType = dr["EVENTTYPE"].ToString();
                    if (EventTypeFunc.IsNetTurnOverAmount(eventType))
                        netInvoiceAmounts.Amount.Amount.DecValue += amount;
                    else if (EventTypeFunc.IsNetTurnOverIssueAmount(eventType))
                        netInvoiceAmounts.IssueAmount.Amount.DecValue += amount;
                    else if (EventTypeFunc.IsNetTurnOverAccountingAmount(eventType))
                        netInvoiceAmounts.AccountingAmount.Amount.DecValue += amount;
                    else if (EventTypeFunc.IsTaxAmount(eventType))
                        netInvoiceAmounts.Tax.Amount.Amount.DecValue += amount;
                    else if (EventTypeFunc.IsTaxIssueAmount(eventType))
                        netInvoiceAmounts.Tax.IssueAmount.Amount.DecValue += amount;
                    else if (EventTypeFunc.IsTaxAccountingAmount(eventType))
                        netInvoiceAmounts.Tax.AccountingAmount.Amount.DecValue += amount;
                }
            }
            return netInvoiceAmounts;
        }
        #endregion SetCurrentInvoiceSupplementAmounts
        #region SetNoteCorrectionToNotePad
        // EG 20101020 Ticket 17185 : Update Notepad with correction notes
        private TradeCommonCaptureGen.ErrorLevel SetNoteCorrectionToNotePad(int pIdT, DataTable pDtNotepad,InvoicingTrade pTrade, InvoicingEvent pEvent)
        {
            string note = Cst.CrLf + $@"{Ressource.GetString("InvoicingCorrectionEvents")} [{ DtFunc.DateTimeToString(ActionDate, DtFunc.FmtShortDate)}] 
            { Ressource.GetString("event_trade_identifier")} [{ pTrade.tradeIdentifier}] { Ressource.GetString("Event_Title")} [{ pEvent.idE}]{Cst.CrLf}{pEvent.note}";
            DataRow row;
            if (0 == pDtNotepad.Rows.Count)
            {
                row = pDtNotepad.NewRow();
                pDtNotepad.Rows.Add(row);
            }
            else
            {
                row = pDtNotepad.Rows[0];
            }
            row.BeginEdit();
            row["IDT"] = pIdT;
            row["TABLENAME"] = Cst.OTCml_TBL.TRADE.ToString();
            row["LONOTE"] += note;
            if (Queue.header.requesterSpecified)
            {
                if (Queue.header.requester.idASpecified)
                    row["IDA"] = Queue.header.requester.idA;
                row["DTUPD"] = Queue.header.requester.date;
            }
            row.EndEdit();
            return TradeCommonCaptureGen.ErrorLevel.SUCCESS;
        }
        #endregion SetNoteCorrectionToNotePad
        #region SelectInvoicingScope
        // EG 20220406 [XXXXX][WI614] Refactoring Query
        private Cst.ErrLevel SelectInvoicingScope()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                #region InvoicingRules
                string sqlInvoiceRules = $@"select ir.IDINVOICINGRULES, ir.IDENTIFIER, ir.DISPLAYNAME, ir.IDA,ir.IDA_INVOICED, 
                ir.IDC_FEE, ir.EVENTTYPE, ir.GPRODUCT, ir.TYPEINSTR, ir.IDINSTR as INSTR, ir.TYPEINSTR_UNL, ir.IDINSTR_UNL as INSTR_UNL,
                ir.TYPEMARKETCONTRACT, ir.IDMARKETCONTRACT as MARKETCONTRACT, ir.TYPEBOOK, ir.IDBOOK as BOOK, ir.PAYMENTTYPE, ir.IDA_TRADER, ir.IDC_TRADE,
                ir.ADDRESSIDENT, ir.IDC_INVOICING, ir.IDASSET_FXRATE_INV, ir.PERIODMLTPOFFSET, ir.PERIODOFFSET, ir.DAYTYPEOFFSET,
                ir.PERIODMLTP, ir.PERIOD, ir.ROLLCONVENTION,
                ir.TAXAPPLICATION, ir.TAXCONDITION,
                ir.RELATIVESTLDELAY, ir.PERIODMLTPSTLDELAY, ir.PERIODSTLDELAY, ir.DAYTYPESTLDELAY, ir.BDC_STLDELAY,
                ir.MAXVALUE, ir.MAXPERIODMLTP, ir.MAXPERIOD, ir.DISCOUNTPERIODMLTP, ir.DISCOUNTPERIOD, ir.BRACKETAPPLICATION 
                from dbo.INVOICINGRULES ir 
                where (ir.IDINVOICINGRULES = {CurrentInvoicingRules}){SQLCst.SEPARATOR_MULTISELECT}";

                #endregion InvoicingRules
                #region InvoicingRules Brackets
                string sqlInvoiceRulesBracket = $@"select irb.IDINVRULESBRACKET,irb.IDINVOICINGRULES, irb.IDENTIFIER, irb.DISPLAYNAME, irb.LOWVALUE, irb.HIGHVALUE, irb.DISCOUNTRATE 
                from dbo.INVRULESBRACKET irb
                where (irb.IDINVOICINGRULES = {CurrentInvoicingRules}){SQLCst.SEPARATOR_MULTISELECT}";
                #endregion InvoicingRules Brackets
                #region Dataset
                m_DsInvoicingRules = DataHelper.ExecuteDataset(ProcessBase.Cs, CommandType.Text, sqlInvoiceRules + sqlInvoiceRulesBracket);
                if ((null == m_DsInvoicingRules) || (0 == m_DsInvoicingRules.Tables[0].Rows.Count))
                {
                    codeReturn = Cst.ErrLevel.DATANOTFOUND;
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05224",
                        new ProcessState(ProcessStateTools.StatusWarningEnum, codeReturn));
                }
                m_DsInvoicingRules.DataSetName = "InvoicingRules";
                DataTable dtInvoicingRules = m_DsInvoicingRules.Tables[0];
                dtInvoicingRules.TableName = "InvoicingRules";
                DataTable dtInvRulesBracket = m_DsInvoicingRules.Tables[1];
                dtInvRulesBracket.TableName = "InvRulesBracket";
                if ((null != dtInvoicingRules) && (null != dtInvRulesBracket))
                {
                    DataRelation relBracketByInvoicingRules = new DataRelation("BracketByInvoicingRules",
                        dtInvoicingRules.Columns["IDINVOICINGRULES"], dtInvRulesBracket.Columns["IDINVOICINGRULES"], false);
                    m_DsInvoicingRules.Relations.Add(relBracketByInvoicingRules);
                }
                #endregion Dataset
                return codeReturn;
            }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                    StrFunc.GetProcessLogMessage("Error in Invoicing scope selection", null, ex.Message)); 
            }
        }
        #endregion SelectInvoicingScope
        #region SetDataCalculation
        //EG 20100331 [Cast product]
        private Cst.ErrLevel SetDataCalculation(DataDocumentContainer pTheoricDocument,INetInvoiceAmounts pNetInvoiceAmounts)
        {
            InvoicingScope scope = m_Invoicing.scopes[0];
            IProduct product = scope.DataDocument.CurrentProduct.Product;
            #region Initial Informations Members Settings
            IInvoice initialInvoice = (IInvoice)m_TradeLibrary.DataDocument.CurrentProduct.Product;
            bool isTaxSpecified = initialInvoice.TaxSpecified;
            string invoiceCurrency = initialInvoice.GrossTurnOverAmount.Currency;
            string invoiceIssueCurrency = initialInvoice.NetTurnOverIssueAmount.Currency;
            decimal initialGrossTurnOverAmount = initialInvoice.GrossTurnOverAmount.Amount.DecValue;
            decimal initialNetTurnOverAmount = initialInvoice.NetTurnOverAmount.Amount.DecValue;
            decimal initialNetTurnOverIssueAmount = initialInvoice.NetTurnOverIssueAmount.Amount.DecValue;
            bool isInitialRebate = initialInvoice.RebateAmountSpecified;
            decimal initialRebateAmount = (isInitialRebate?initialInvoice.RebateAmount.Amount.DecValue:0);
            //
            decimal initialNetTurnOverAccountingAmount = 0;
            string invoiceAccountingCurrency = string.Empty;
            bool isInitialAccountingAmount = initialInvoice.NetTurnOverAccountingAmountSpecified;
            if (isInitialAccountingAmount)
            {
                invoiceAccountingCurrency = initialInvoice.NetTurnOverAccountingAmount.Currency;
                initialNetTurnOverAccountingAmount = initialInvoice.NetTurnOverAccountingAmount.Amount.DecValue;
            }
            #region Tax
            decimal initialTaxAmount = 0;
            decimal initialTaxIssueAmount = 0;
            decimal initialTaxAccountingAmount = 0;
            bool isInitialTaxAccountingAmount = false;
            if (isTaxSpecified)
            {
                IInvoiceTax tax = initialInvoice.Tax;
                initialTaxAmount = tax.Amount.Amount.DecValue;
                initialTaxIssueAmount = tax.IssueAmount.Amount.DecValue;
                isInitialTaxAccountingAmount = tax.AccountingAmountSpecified;
                if (isInitialTaxAccountingAmount)
                    initialTaxAccountingAmount = tax.AccountingAmount.Amount.DecValue;
            }
            #endregion Tax
            #endregion Initial Informations Members Settings

            #region Theoric Informations Members Settings
            IInvoice theoricInvoice = (IInvoice)pTheoricDocument.CurrentProduct.Product;
            decimal theoricGrossTurnOverAmount = theoricInvoice.GrossTurnOverAmount.Amount.DecValue;
            decimal theoricNetTurnOverAmount = theoricInvoice.NetTurnOverAmount.Amount.DecValue;
            decimal theoricNetTurnOverIssueAmount = theoricInvoice.NetTurnOverIssueAmount.Amount.DecValue;
            bool isTheoricRebate = (theoricInvoice.RebateAmountSpecified);
            decimal theoricRebateAmount = (isTheoricRebate ? theoricInvoice.RebateAmount.Amount.DecValue : 0);
            #region NetTurnOverAccountingAmount
            decimal theoricNetTurnOverAccountingAmount = 0;
            bool isTheoricAccountingAmount = theoricInvoice.NetTurnOverAccountingAmountSpecified;
            bool isTheoricTaxAccountingAmount = false;
            if (isTheoricAccountingAmount)
                theoricNetTurnOverAccountingAmount = theoricInvoice.NetTurnOverAccountingAmount.Amount.DecValue;
            #endregion NetTurnOverAccountingAmount

            #region Tax
            decimal theoricTaxAmount = 0;
            decimal theoricTaxIssueAmount = 0;
            decimal theoricTaxAccountingAmount = 0;
            if (isTaxSpecified)
            {
                IInvoiceTax tax = theoricInvoice.Tax;
                theoricTaxAmount = tax.Amount.Amount.DecValue;
                theoricTaxIssueAmount = tax.IssueAmount.Amount.DecValue;
                isTheoricTaxAccountingAmount = tax.AccountingAmountSpecified;
                if (isTheoricTaxAccountingAmount)
                    theoricTaxAccountingAmount = tax.AccountingAmount.Amount.DecValue;
            }
            #endregion Tax

            #endregion Theoric Informations Members Settings

            //EG 20100331 [Cast product]
            IInvoiceSupplement finalInvoice = (IInvoiceSupplement)scope.DataDocument.CurrentProduct.Product;

            #region InitialInvoiceAmounts
            IInitialInvoiceAmounts initialInvoiceAmount = (IInitialInvoiceAmounts)finalInvoice.InitialInvoiceAmount;
            #region GrossTurnOverAmount
            initialInvoiceAmount.GrossTurnOverAmount = product.ProductBase.CreateMoney(initialGrossTurnOverAmount, invoiceCurrency);
            #endregion GrossTurnOverAmount
            #region TotalRebateAmount
            initialInvoiceAmount.RebateAmountSpecified = isInitialRebate;
            if (initialInvoiceAmount.RebateAmountSpecified)
                initialInvoiceAmount.RebateAmount = product.ProductBase.CreateMoney(initialRebateAmount, invoiceCurrency);
            #endregion TotalRebateAmount
            #region NetTurnOverAmount
            initialInvoiceAmount.NetTurnOverAmount = product.ProductBase.CreateMoney(initialNetTurnOverAmount, invoiceCurrency);
            #endregion NetTurnOverAmount
            #region NetTurnOverIssueAmount
            initialInvoiceAmount.NetTurnOverIssueAmount = product.ProductBase.CreateMoney(initialNetTurnOverIssueAmount, invoiceIssueCurrency);
            #endregion NetTurnOverIssueAmount
            #region NetTurnOverAccountingAmount
            initialInvoiceAmount.NetTurnOverAccountingAmountSpecified = isInitialAccountingAmount;
            if (isInitialAccountingAmount)
                initialInvoiceAmount.NetTurnOverAccountingAmount = product.ProductBase.CreateMoney(initialNetTurnOverAccountingAmount, invoiceAccountingCurrency);
            #endregion NetTurnOverAccountingAmount
            #region Tax
            if (isTaxSpecified)
            {
                initialInvoiceAmount.TaxSpecified = true;
                initialInvoiceAmount.Tax = finalInvoice.CreateInvoiceTax(initialInvoice.Tax.Details.Length);
                #region TaxAmount
                initialInvoiceAmount.Tax.Amount = product.ProductBase.CreateMoney(initialTaxAmount, invoiceCurrency);
                #endregion TaxAmount
                #region TaxIssueAmount
                initialInvoiceAmount.Tax.IssueAmountSpecified = true;
                initialInvoiceAmount.Tax.IssueAmount = product.ProductBase.CreateMoney(initialTaxIssueAmount, invoiceIssueCurrency);
                #endregion TaxIssueAmount
                #region TaxAccountingAmount
                initialInvoiceAmount.Tax.AccountingAmountSpecified = isInitialTaxAccountingAmount;
                if (isInitialTaxAccountingAmount)
                    initialInvoiceAmount.Tax.AccountingAmount = product.ProductBase.CreateMoney(initialTaxAccountingAmount, invoiceAccountingCurrency);
                #endregion TaxAccountingAmount
                #region Detail Tax
                SetTaxDetailCalculation(initialInvoice.Tax, initialInvoiceAmount.Tax);
                #endregion Detail Tax
            }
            #endregion Tax
            #region Identifiers
            initialInvoiceAmount.Identifier = new EFS_String(m_DsTrade.Identifier);
            initialInvoiceAmount.OTCmlId = m_DsTrade.IdT;
            #endregion Identifiers
            #endregion InitialInvoiceAmounts

            #region BaseNetInvoiceAmounts
            #region NetTurnOverAmount
            finalInvoice.BaseNetInvoiceAmount.Amount = product.ProductBase.CreateMoney(pNetInvoiceAmounts.Amount.Amount.DecValue, pNetInvoiceAmounts.Amount.Currency);
            #endregion NetTurnOverAmount
            #region NetTurnOverIssueAmount
            finalInvoice.BaseNetInvoiceAmount.IssueAmountSpecified = true;
            finalInvoice.BaseNetInvoiceAmount.IssueAmount = product.ProductBase.CreateMoney(pNetInvoiceAmounts.IssueAmount.Amount.DecValue, pNetInvoiceAmounts.IssueAmount.Currency);
            #endregion NetTurnOverIssueAmount
            #region NetTurnOverAccountingAmount
            if (pNetInvoiceAmounts.AccountingAmountSpecified)
            {
                finalInvoice.BaseNetInvoiceAmount.AccountingAmountSpecified = true;
                finalInvoice.BaseNetInvoiceAmount.AccountingAmount = product.ProductBase.CreateMoney(pNetInvoiceAmounts.AccountingAmount.Amount.DecValue, pNetInvoiceAmounts.AccountingAmount.Currency);
            }
            #endregion NetTurnOverTurnOverAccountingAmount
            #region Tax
            if (isTaxSpecified)
            {
                #region Detail Tax
                finalInvoice.BaseNetInvoiceAmount.TaxSpecified = true;
                finalInvoice.BaseNetInvoiceAmount.Tax = finalInvoice.CreateInvoiceTax(initialInvoice.Tax.Details.Length);
                #region TaxAmount
                finalInvoice.BaseNetInvoiceAmount.Tax.Amount = product.ProductBase.CreateMoney(pNetInvoiceAmounts.Tax.Amount.Amount.DecValue, invoiceCurrency);
                #endregion TaxAmount
                #region TaxIssueAmount
                finalInvoice.BaseNetInvoiceAmount.Tax.IssueAmountSpecified = true;
                finalInvoice.BaseNetInvoiceAmount.Tax.IssueAmount = product.ProductBase.CreateMoney(pNetInvoiceAmounts.Tax.IssueAmount.Amount.DecValue, invoiceIssueCurrency);
                #endregion TaxIssueAmount
                #region TaxAccountingAmount
                finalInvoice.BaseNetInvoiceAmount.Tax.AccountingAmountSpecified = pNetInvoiceAmounts.Tax.AccountingAmountSpecified;
                if (finalInvoice.BaseNetInvoiceAmount.Tax.AccountingAmountSpecified)
                    finalInvoice.BaseNetInvoiceAmount.Tax.AccountingAmount = product.ProductBase.CreateMoney(pNetInvoiceAmounts.Tax.AccountingAmount.Amount.DecValue, invoiceAccountingCurrency);
                #endregion TaxAccountingAmount
                SetTaxDetailCalculation(initialInvoice.Tax, finalInvoice.BaseNetInvoiceAmount.Tax);
                #endregion Detail Tax
            }
            #endregion Tax
            #endregion BaseNetInvoiceAmounts

            #region FinalInvoice amounts (Additional Invoice or Credit Note)
            #region RebateCapConditions
            if (isTheoricRebate)
            {
                #region BracketConditions
                if (theoricInvoice.RebateConditions.BracketConditionsSpecified)
                {
                    IRebateBracketConditions bracketConditions = theoricInvoice.RebateConditions.BracketConditions;
                    IRebateBracketResult theoricResult = bracketConditions.Result;
                    IRebateBracketResult finalResult = finalInvoice.RebateConditions.BracketConditions.Result;

                    #region TotalRebateBracketAmount
                    finalResult.TotalRebateBracketAmountSpecified = theoricResult.TotalRebateBracketAmountSpecified;
                    if (finalResult.TotalRebateBracketAmountSpecified)
                    {
                        decimal theoricTotalRebateBracketAmount = theoricResult.TotalRebateBracketAmount.Amount.DecValue;
                        finalResult.TotalRebateBracketAmount = product.ProductBase.CreateMoney(theoricTotalRebateBracketAmount,invoiceCurrency);
                        finalResult.Calculations = theoricResult.Calculations;
                    }
                    #endregion TotalRebateBracketAmount
                    #region SumOfGrossTurnOverPreviousPeriodAmount
                    finalResult.SumOfGrossTurnOverPreviousPeriodAmountSpecified = theoricResult.SumOfGrossTurnOverPreviousPeriodAmountSpecified;
                    if (finalResult.SumOfGrossTurnOverPreviousPeriodAmountSpecified)
                    {
                        decimal theoricSumOfGTOPreviousPeriodAmount = theoricResult.SumOfGrossTurnOverPreviousPeriodAmount.Amount.DecValue;
                        finalResult.SumOfGrossTurnOverPreviousPeriodAmount = product.ProductBase.CreateMoney(theoricSumOfGTOPreviousPeriodAmount, invoiceCurrency);
                    }
                    #endregion SumOfGrossTurnOverPreviousPeriodAmount
                }
                #endregion BracketConditions
                #region CapConditions
                if (theoricInvoice.RebateConditions.CapConditionsSpecified)
                {
                    IRebateCapConditions capConditions = theoricInvoice.RebateConditions.CapConditions;
                    IRebateCapResult theoricResult = capConditions.Result;
                    IRebateCapResult finalResult = finalInvoice.RebateConditions.CapConditions.Result;
                    #region NetTurnOverInExcessAmount
                    finalResult.NetTurnOverInExcessAmountSpecified = theoricResult.NetTurnOverInExcessAmountSpecified;
                    if (finalResult.NetTurnOverInExcessAmountSpecified)
                    {
                        decimal theoricNetTurnOverInExcessAmount = theoricResult.NetTurnOverInExcessAmount.Amount.DecValue;
                        finalResult.NetTurnOverInExcessAmount = product.ProductBase.CreateMoney(theoricNetTurnOverInExcessAmount, invoiceCurrency);
                    }
                    #endregion NetTurnOverInExcessAmount
                    #region SumOfNetTurnOverPreviousPeriodAmount
                    finalResult.SumOfNetTurnOverPreviousPeriodAmountSpecified = theoricResult.SumOfNetTurnOverPreviousPeriodAmountSpecified;
                    if (finalResult.SumOfNetTurnOverPreviousPeriodAmountSpecified)
                    {
                        decimal theoricSumOfNTOPreviousPeriodAmount = theoricResult.SumOfNetTurnOverPreviousPeriodAmount.Amount.DecValue;
                        finalResult.SumOfNetTurnOverPreviousPeriodAmount = product.ProductBase.CreateMoney(theoricSumOfNTOPreviousPeriodAmount, invoiceCurrency);
                    }
                    #endregion SumOfNetTurnOverPreviousPeriodAmount
                }
                #endregion CapConditions
            }
            #endregion RebateCapConditions
            #region RebateAmount
            finalInvoice.RebateConditions.TotalRebateAmountSpecified = isTheoricRebate;
            if (finalInvoice.RebateConditions.TotalRebateAmountSpecified)
                finalInvoice.RebateConditions.TotalRebateAmount = product.ProductBase.CreateMoney(Math.Abs(theoricRebateAmount), invoiceCurrency);

            if (isInitialRebate || isTheoricRebate)
            {
                decimal diffRebateAmount = theoricRebateAmount - initialRebateAmount;
                finalInvoice.RebateAmountSpecified = (0 != diffRebateAmount);
                if (finalInvoice.RebateAmountSpecified)
                    finalInvoice.RebateAmount = product.ProductBase.CreateMoney(Math.Abs(diffRebateAmount), invoiceCurrency);
                finalInvoice.RebateIsInExcessSpecified = (0 > diffRebateAmount);
                if (finalInvoice.RebateIsInExcessSpecified)
                    finalInvoice.RebateIsInExcess = new EFS_Boolean(true);
            }
            #endregion RebateAmount

            #region NetTurnOverAmount
            decimal diffNetTurnOverAmount = Math.Abs(theoricNetTurnOverAmount - finalInvoice.BaseNetInvoiceAmount.Amount.Amount.DecValue);
            finalInvoice.NetTurnOverAmount = product.ProductBase.CreateMoney(diffNetTurnOverAmount, invoiceCurrency);

            #endregion NetTurnOverAmount
            #region NetTurnOverIssueAmount
            finalInvoice.NetTurnOverIssueRateSpecified = initialInvoice.NetTurnOverIssueRateSpecified;
            EFS_Cash cash;
            decimal diffNetTurnOverIssueAmount;
            if (invoiceIssueCurrency == invoiceCurrency)
            {
                diffNetTurnOverIssueAmount = Math.Abs(theoricNetTurnOverIssueAmount - finalInvoice.BaseNetInvoiceAmount.IssueAmount.Amount.DecValue);
                finalInvoice.NetTurnOverIssueAmount = product.ProductBase.CreateMoney(diffNetTurnOverIssueAmount, invoiceIssueCurrency);
            }
            else if (finalInvoice.NetTurnOverIssueRateSpecified)
            {
                finalInvoice.NetTurnOverIssueRate = new EFS_Decimal(initialInvoice.NetTurnOverIssueRate.DecValue);
                finalInvoice.IssueRateReadSpecified = initialInvoice.IssueRateReadSpecified;
                if (finalInvoice.IssueRateReadSpecified)
                    finalInvoice.IssueRateRead = new EFS_Decimal(initialInvoice.IssueRateRead.DecValue);
                finalInvoice.IssueRateIsReverseSpecified = initialInvoice.IssueRateIsReverseSpecified;
                if (finalInvoice.IssueRateIsReverseSpecified)
                    finalInvoice.IssueRateIsReverse = new EFS_Boolean(initialInvoice.IssueRateIsReverse.BoolValue);
                diffNetTurnOverIssueAmount = finalInvoice.NetTurnOverAmount.Amount.DecValue * initialInvoice.NetTurnOverIssueRate.DecValue;
                cash = new EFS_Cash(ProcessBase.Cs, diffNetTurnOverIssueAmount, invoiceIssueCurrency);
                finalInvoice.NetTurnOverIssueAmount = product.ProductBase.CreateMoney(cash.AmountRounded, invoiceIssueCurrency);
            }
            finalInvoice.InvoiceRateSourceSpecified = initialInvoice.InvoiceRateSourceSpecified;
            if (finalInvoice.InvoiceRateSourceSpecified)
                finalInvoice.InvoiceRateSource = (IInvoiceRateSource) initialInvoice.InvoiceRateSource.Clone();
            #endregion NetTurnOverIssueAmount
            #region NetTurnOverAccountingAmount
            finalInvoice.NetTurnOverAccountingRateSpecified = initialInvoice.NetTurnOverAccountingRateSpecified;
            decimal diffNetTurnOverAccountingAmount;
            if (invoiceAccountingCurrency == invoiceCurrency)
            {
                finalInvoice.NetTurnOverAccountingAmountSpecified = true;
                _ = Math.Abs(theoricNetTurnOverAccountingAmount - finalInvoice.BaseNetInvoiceAmount.AccountingAmount.Amount.DecValue);
                finalInvoice.NetTurnOverAccountingAmount = product.ProductBase.CreateMoney(diffNetTurnOverAmount, invoiceCurrency); ;
            }
            else if (finalInvoice.NetTurnOverAccountingRateSpecified)
            {
                finalInvoice.NetTurnOverAccountingAmountSpecified = true;
                finalInvoice.NetTurnOverAccountingRate = new EFS_Decimal(initialInvoice.NetTurnOverAccountingRate.DecValue);
                finalInvoice.AccountingRateReadSpecified = initialInvoice.AccountingRateReadSpecified;
                if (finalInvoice.AccountingRateReadSpecified)
                    finalInvoice.AccountingRateRead = new EFS_Decimal(initialInvoice.AccountingRateRead.DecValue);
                finalInvoice.AccountingRateIsReverseSpecified = initialInvoice.AccountingRateIsReverseSpecified;
                if (finalInvoice.AccountingRateIsReverseSpecified)
                    finalInvoice.AccountingRateIsReverse = new EFS_Boolean(initialInvoice.AccountingRateIsReverse.BoolValue);

                diffNetTurnOverAccountingAmount = finalInvoice.NetTurnOverAmount.Amount.DecValue * initialInvoice.NetTurnOverAccountingRate.DecValue;
                cash = new EFS_Cash(ProcessBase.Cs, diffNetTurnOverAccountingAmount, invoiceAccountingCurrency);
                finalInvoice.NetTurnOverAccountingAmount = product.ProductBase.CreateMoney(cash.AmountRounded, invoiceAccountingCurrency);
            }
            #endregion NetTurnOverAccountingAmount
            #region Tax
            if (isTaxSpecified)
            {
                finalInvoice.TaxSpecified = true;
                finalInvoice.Tax = finalInvoice.CreateInvoiceTax(initialInvoice.Tax.Details.Length);
                #region TaxAmount
                IInvoiceTax baseTax = finalInvoice.BaseNetInvoiceAmount.Tax;
                
                decimal diffTaxAmount = Math.Abs(theoricTaxAmount - baseTax.Amount.Amount.DecValue);
                finalInvoice.Tax.Amount = product.ProductBase.CreateMoney(diffTaxAmount, invoiceCurrency);
                #endregion TaxAmount
                #region TaxIssueAmount
                decimal diffTaxIssueAmount = Math.Abs(theoricTaxIssueAmount - baseTax.IssueAmount.Amount.DecValue);
                finalInvoice.Tax.IssueAmountSpecified = true;
                finalInvoice.Tax.IssueAmount = product.ProductBase.CreateMoney(diffTaxIssueAmount, invoiceIssueCurrency);
                #endregion TaxIssueAmount
                #region TaxAccountingAmount
                if (invoiceAccountingCurrency == invoiceCurrency)
                {
                    finalInvoice.Tax.AccountingAmountSpecified = true;
                    decimal diffTaxAccountingAmount = Math.Abs(theoricTaxAccountingAmount - baseTax.AccountingAmount.Amount.DecValue);
                    finalInvoice.Tax.AccountingAmount = product.ProductBase.CreateMoney(diffTaxAccountingAmount, invoiceCurrency); ;
                }
                #endregion NetTurnOverAccountingAmount
                #region Detail Tax
                SetTaxDetailCalculation(initialInvoice.Tax, finalInvoice.Tax);
                #endregion Detail Tax
            }
            #endregion Tax
            #endregion FinalInvoice amounts (Additional Invoice or Credit Note)


            #region TheoricInvoiceAmounts
            IInvoiceAmounts theoricInvoiceAmount = (IInvoiceAmounts)finalInvoice.TheoricInvoiceAmount;
            #region GrossTurnOverAmount
            theoricInvoiceAmount.GrossTurnOverAmount = product.ProductBase.CreateMoney(theoricGrossTurnOverAmount,invoiceCurrency);
            #endregion GrossTurnOverAmount
            #region TotalRebateAmount
            theoricInvoiceAmount.RebateAmountSpecified = isTheoricRebate;
            if (theoricInvoiceAmount.RebateAmountSpecified)
                theoricInvoiceAmount.RebateAmount = product.ProductBase.CreateMoney(theoricRebateAmount,invoiceCurrency);
            #endregion TotalRebateAmount
            #region NetTurnOverAmount
            theoricInvoiceAmount.NetTurnOverAmount = product.ProductBase.CreateMoney(theoricNetTurnOverAmount,invoiceCurrency);
            #endregion NetTurnOverAmount
            #region NetTurnOverIssueAmount
            theoricInvoiceAmount.NetTurnOverIssueAmount = product.ProductBase.CreateMoney(theoricNetTurnOverIssueAmount, invoiceIssueCurrency);
            #endregion NetTurnOverIssueAmount
            #region NetTurnOverAccountingAmount
            theoricInvoiceAmount.NetTurnOverAccountingAmountSpecified = isTheoricAccountingAmount;
            if (isTheoricAccountingAmount)
                theoricInvoiceAmount.NetTurnOverAccountingAmount = product.ProductBase.CreateMoney(theoricNetTurnOverAccountingAmount, invoiceAccountingCurrency);
            #endregion NetTurnOverAccountingAmount
            #region Tax
            if (isTaxSpecified)
            {
                theoricInvoiceAmount.TaxSpecified = true;
                theoricInvoiceAmount.Tax = finalInvoice.CreateInvoiceTax(initialInvoice.Tax.Details.Length);
                #region TaxAmount
                theoricInvoiceAmount.Tax.Amount= product.ProductBase.CreateMoney(theoricTaxAmount, invoiceCurrency);
                #endregion TaxAmount
                #region TaxIssueAmount
                theoricInvoiceAmount.Tax.IssueAmountSpecified = true;
                theoricInvoiceAmount.Tax.IssueAmount = product.ProductBase.CreateMoney(theoricTaxIssueAmount, invoiceIssueCurrency);
                #endregion TaxIssueAmount
                #region TaxAccountingAmount
                theoricInvoiceAmount.Tax.AccountingAmountSpecified = isTheoricTaxAccountingAmount;
                if (isTheoricTaxAccountingAmount)
                    theoricInvoiceAmount.Tax.AccountingAmount = product.ProductBase.CreateMoney(theoricTaxAccountingAmount, invoiceAccountingCurrency);
                #endregion TaxAccountingAmount
                #region Detail Tax
                SetTaxDetailCalculation(initialInvoice.Tax, theoricInvoiceAmount.Tax);
                #endregion Detail Tax
            }
            #endregion Tax
            #endregion TheoricInvoiceAmounts
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion SetDataCalculation
        #region SetTaxDetailCalculation
        private void SetTaxDetailCalculation(IInvoiceTax pInvoiceTaxSource,IInvoiceTax pInvoiceTaxDest)
        {
            int nbTaxDet = pInvoiceTaxSource.Details.Length;
            decimal sourceTotalTaxAmount = pInvoiceTaxSource.Amount.Amount.DecValue;
            for (int i = 0; i < nbTaxDet; i++)
            {
                pInvoiceTaxDest.Details[i] = (ITaxSchedule)pInvoiceTaxSource.Details[i].Clone();
                #region Calculation
                decimal pctTaxDet = pInvoiceTaxSource.Details[i].TaxAmount.Amount.Amount.DecValue / sourceTotalTaxAmount;
                decimal amount = pctTaxDet * pInvoiceTaxDest.Amount.Amount.DecValue;
                EFS_Cash cash = new EFS_Cash(ProcessBase.Cs, amount, pInvoiceTaxDest.Amount.Currency);
                pInvoiceTaxDest.Details[i].TaxAmount.Amount.Amount.DecValue = cash.AmountRounded;
                amount = pctTaxDet * pInvoiceTaxDest.IssueAmount.Amount.DecValue;
                cash = new EFS_Cash(ProcessBase.Cs, amount, pInvoiceTaxDest.IssueAmount.Currency);
                pInvoiceTaxDest.Details[i].TaxAmount.IssueAmount.Amount.DecValue = cash.AmountRounded;
                if (pInvoiceTaxDest.Details[i].TaxAmount.AccountingAmountSpecified)
                {
                    amount = pctTaxDet * pInvoiceTaxDest.AccountingAmount.Amount.DecValue;
                    cash = new EFS_Cash(ProcessBase.Cs, amount, pInvoiceTaxDest.AccountingAmount.Currency);
                    pInvoiceTaxDest.Details[i].TaxAmount.AccountingAmount.Amount.DecValue = cash.AmountRounded;
                }
                #endregion Calculation
            }
        }
        #endregion SetTaxDetailCalculation
        #region SetFinalInvoice
        /// <summary>
        /// Construction du trade correctif (Facture additionnelle ou avoir)
        /// - Lecture du montant net de facturation
        /// - Comparaison avec le montant net théorique
        /// - Génération Facture additionnelle ou Avoir
        /// </summary>
        /// <returns></returns>
        private Cst.ErrLevel SetFinalInvoice()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            InvoicingScope scope = m_Invoicing.scopes[0];
            IInvoice initialInvoice = (IInvoice)m_TradeLibrary.Product;
            IInvoice theoricInvoice = (IInvoice)scope.DataDocument.CurrentTrade.Product;
            INetInvoiceAmounts netInvoiceamounts = SetCurrentInvoiceSupplementAmounts(initialInvoice,ProcessBase.CurrentId);
            if (netInvoiceamounts.Amount.Amount.DecValue < theoricInvoice.NetTurnOverAmount.Amount.DecValue)
                m_Invoicing.entity.defaultInstrumentType = InvoicingInstrumentTypeEnum.AdditionalInvoicing;
            else if (netInvoiceamounts.Amount.Amount.DecValue > theoricInvoice.NetTurnOverAmount.Amount.DecValue)
                m_Invoicing.entity.defaultInstrumentType = InvoicingInstrumentTypeEnum.CreditNote;
            else
            {
                codeReturn = Cst.ErrLevel.DATANOTFOUND;
                WarningNetIsEqualToZero();
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                DataDocumentContainer theoricDocument = (DataDocumentContainer)scope.DataDocument.Clone();
                codeReturn = CurrentInstrument.CreateDataDocument();
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = scope.SetDataDocument(InvoiceDate, m_Invoicing.entity);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = SetDataCalculation(theoricDocument, netInvoiceamounts);
            }
            return codeReturn;
        }
        #endregion SetFinalInvoice
        #region SetInvoicingScope
        /// <summary>
        /// Lecture de la facture et chargement
        /// - de son périmètre de facturation
        /// - des frais élémentaires
        /// - application des montants de frais modifiés
        /// </summary>
        /// <returns></returns>
        // EG 20090420 Ajout Flag signalant la modification du montant unitaire de frais pour un trade
        // EG 20090420 isInvoicingEventIsCorrected et isAmountIsCorrected
        // EG 20101019 Ajout gestion des notes associées à la modification de montants élémentaires de frais
        // EG 20150706 [21021] Nullable<int> for idA_Counterparty
        private Cst.ErrLevel SetInvoicingScope()
        {
            string methodName = "InvoicingGen_Correction.SetInvoicingScope";
            try
            {
                #region Lecture de la facture : Chargement de son périmètre et des frais élémentaires
                IInvoice invoice = (IInvoice)m_TradeLibrary.CurrentTrade.Product;
                #region Chargement du Périmètre
                Cst.ErrLevel codeReturn = SelectInvoicingScope();
                #endregion Chargement du Périmètre
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    DataRow[] rowInvoicingRules = DtInvoicingRules.Select();
                    if ((null != rowInvoicingRules) && (1 == rowInvoicingRules.GetLength(0)))
                    {
                        #region Affectation du Périmètre (contexte + règle + conditions de remise)
                        m_Invoicing.scopes = new InvoicingScope[] { new InvoicingScope(m_Invoicing.entity, rowInvoicingRules[0], BracketByInvoicingRules) };
                        InvoicingScope scope = m_Invoicing.scopes[0];
                        #endregion Affectation du Périmètre (contexte + règle + conditions de remise)
                        InvoicingCorrectionGenMQueue invoiceCorrection = (InvoicingCorrectionGenMQueue)Queue;
                        IInvoiceTrade[] trades = invoice.InvoiceDetails.InvoiceTrade;
                        scope.trades = new InvoicingTrade[trades.Length];
                        for (int i = 0; i < trades.Length; i++)
                        {
                            IInvoiceTrade trade = trades[i];
                            IInvoiceFee[] fees = trade.InvoiceFees.InvoiceFee;
                            // EG 20150706 [21021]
                            Nullable<int> idA_Counterparty = m_TradeLibrary.DataDocument.GetOTCmlId_Party(trade.CounterpartyPartyReference.HRef);
                            if (idA_Counterparty.HasValue)
                            {
                                InvoicingTrade invoicingTrade = new InvoicingTrade(ProcessBase.Cs, trade, idA_Counterparty.Value);
                                codeReturn = invoicingTrade.ErrLevel;
                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    invoicingTrade.events = new InvoicingEvent[fees.Length];
                                    invoicingTrade.isInvoicingEventIsCorrected = false;
                                    for (int j = 0; j < fees.Length; j++)
                                    {
                                        invoicingTrade.events[j] = new InvoicingEvent(trade.OTCmlId, fees[j])
                                        {
                                            isAmountIsCorrected = false
                                        };
                                        #region Application du montant de frais modifié par l'action diverse
                                        InvoicingDetailMsg invoiceDetailEvent = invoiceCorrection.InvoicingDetailEvent(invoicingTrade.events[j].idE);
                                        if (null != invoiceDetailEvent)
                                        {
                                            // EG 20090420 Ajout Flag signalant la modification du montant unitaire de frais pour un trade
                                            invoicingTrade.events[j].amount = invoiceDetailEvent.amount;
                                            invoicingTrade.events[j].baseAmount = invoiceDetailEvent.originalAmount;
                                            // EG 20091105 Add scope.isInvoicingEventIsCorrected
                                            scope.isInvoicingEventIsCorrected = true;
                                            invoicingTrade.isInvoicingEventIsCorrected = true;
                                            invoicingTrade.events[j].isAmountIsCorrected = true;
                                            // EG 20101020 Ticket 17185 : Add Note & Lock Trade
                                            invoicingTrade.events[j].noteSpecified = invoiceDetailEvent.noteSpecified;
                                            invoicingTrade.events[j].note = invoiceDetailEvent.note;
                                            scope.LockTrade(invoicingTrade.idT, invoicingTrade.tradeIdentifier);
                                        }
                                        else
                                        {
                                            // EG 20091105 
                                            invoicingTrade.events[j].baseAmount = invoicingTrade.events[j].amount;
                                            invoicingTrade.events[j].baseAccountingAmount = invoicingTrade.events[j].accountingAmount;
                                        }
                                        #endregion Application du montant de frais modifié par l'action diverse
                                    }
                                    scope.trades[i] = invoicingTrade;
                                }
                            } 
                        }
                    }
                    else
                    {
                        //Aucun périmètre 
                    }
                }
                #endregion Lecture de la facture : Chargement de son périmètre et des frais élémentaires
                return codeReturn;
            }
            catch (Exception ex)
            {
                string logMessage = StrFunc.GetProcessLogMessage("Error in InvoicingRules and Events candidates", null, ex.Message);
                throw new SpheresException2(methodName, logMessage);
            }
        }
        #endregion SetInvoicingScope
        #region Update
        /// <summary>
        /// Mise à jour des tables
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <returns></returns>
        // EG 20200914 [25077] RDBMS : Correction DtTradeXML
        protected override TradeCommonCaptureGen.ErrorLevel Update(IDbTransaction pDbTransaction)
        {
            TradeCommonCaptureGen.ErrorLevel codeReturn = UpdateStUsedByForEndOfTradeAction();
            if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == codeReturn)
                codeReturn = AddTradeActionEventOnInvoiceSource();

            if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == codeReturn)
                codeReturn = UpdateCorrectedFeesAmountEventOnInvoiceSource();

            // EG 20101020 Ticket 17185 : Update corrected Fees source (trade)
            if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == codeReturn)
                codeReturn = UpdateCorrectedFeesAmountElementaryTradesAndEvents(pDbTransaction);

            if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == codeReturn)
            {
                EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(m_TradeLibrary.DataDocument.DataDocument);
                StringBuilder sb = CacheSerializer.Serialize(serializerInfo);
                m_DsTrade.DtTradeXML.Rows[0]["TRADEXML"] = sb.ToString();
                m_DsTrade.UpdateTradeXML(pDbTransaction);
            }

            if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == codeReturn)
            {
                // EG 20101020 Ticket 17185 : Update NotePad
                m_DsTrade.UpdateTradeNotepad(pDbTransaction);

                m_DsTrade.UpdateTradeStSys(pDbTransaction);
                DataTable dtChanges = m_DsEvents.DtEvent.GetChanges();
                m_DsEvents.Update(pDbTransaction);
                if (null != dtChanges)
                {
                    DateTime dtSys = OTCmlHelper.GetDateSysUTC(ProcessBase.Cs);
                    foreach (DataRow row in dtChanges.Rows)
                    {
                        if (DataRowState.Deleted != row.RowState)
                        {
                            int idE = Convert.ToInt32(row["IDE"]);
                            m_EventProcess.Write(pDbTransaction, idE, Cst.ProcessTypeEnum.INVOICINGGEN, 
                                ProcessStateTools.StatusSuccessEnum, dtSys , ProcessBase.Tracker.IdTRK_L);
                        }
                    }
                }
            }
            return codeReturn;
        }
        #endregion Update
        #region UpdateStUsedByForEndOfTradeAction
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected TradeCommonCaptureGen.ErrorLevel UpdateStUsedByForEndOfTradeAction()
        {
            DataRow rowTradeStSys = m_DsTrade.DtTrade.Rows[0];
            rowTradeStSys.BeginEdit();
            rowTradeStSys["IDSTUSEDBY"] = Cst.StatusActivation.REGULAR.ToString();
            rowTradeStSys["IDASTUSEDBY"] = ProcessBase.Session.IdA;
            rowTradeStSys["DTSTUSEDBY"] = OTCmlHelper.GetDateSysUTC(ProcessBase.Cs);
            rowTradeStSys["LIBSTUSEDBY"] = Convert.DBNull;
            rowTradeStSys.EndEdit();
            return TradeCommonCaptureGen.ErrorLevel.SUCCESS;
        }
        #endregion UpdateStUsedByForEndOfTradeAction
        #region WarningNetIsEqualToZero
        /// <summary>
        /// La demande de correction est anihilée : Le montant théorique Net est EGAL au montant initial net
        /// </summary>
        // EG 201106015 Ticket : 17480 
        // EG 20180423 Analyse du code Correction [CA2200]
        private void WarningNetIsEqualToZero()
        {
            TradeCommonCaptureGen.ErrorLevel codeReturn = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
            IDbTransaction dbTransaction = null;
            string methodName = "InvoicingGen_Correction.WarningNetIsEqualToZero";
            try
            {
                #region START Transaction (Begin Tran)
                //Begin Tran  doit être la 1er instruction Car si Error un  roolback est fait de manière systematique
                try { dbTransaction = DataHelper.BeginTran(ProcessBase.Cs); }
                catch (Exception ex) { throw new TradeCommonCaptureGenException(methodName, ex, TradeCommonCaptureGen.ErrorLevel.BEGINTRANSACTION_ERROR); }
                #endregion START Transaction (Begin Tran)
                codeReturn = UpdateStUsedByForEndOfTradeAction();
                m_DsTrade.UpdateTradeStSys(dbTransaction);
            }
            catch (SpheresException2)
            {
                codeReturn = TradeCommonCaptureGen.ErrorLevel.FAILURE;
                throw;
            }
            catch (Exception ex)
            {
                codeReturn = TradeCommonCaptureGen.ErrorLevel.FAILURE;
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
            }
            finally
            {
                if (null != dbTransaction)
                {
                    if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == codeReturn)
                    {
                        #region END Transaction (Commit)
                        try { DataHelper.CommitTran(dbTransaction); }
                        catch (Exception ex) { throw new TradeCommonCaptureGenException(methodName, ex, TradeCommonCaptureGen.ErrorLevel.COMMIT_ERROR); }
                        #endregion END Transaction (Commit)
                    }
                    else
                        DataHelper.RollbackTran(dbTransaction);
                }
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StrFunc.GetProcessLogMessage("Theoric Net = Initial Net", null),
                    new ProcessState(ProcessStateTools.StatusWarningEnum, ProcessStateTools.ParseCodeReturn(codeReturn.ToString())), ProcessBase.CurrentId); 

            }
        }
        #endregion WarningNetIsEqualToZero
        #endregion Methods
    }
}

#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.TradeLink;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum.Tools;
using System;
using System.Collections;
using System.Data;
using System.Reflection;
#endregion Using Directives

namespace EFS.Process
{
    #region TradeAdminActionGenRemoveTrade
    public class TradeAdminActionGenRemoveTrade : TradeActionGenProcessBase
    {
        #region Members
        protected DataSetTrade m_DsTradeLinked;
        protected DataSetEventTrade m_DsEventLinked;
        protected int[] m_LinkedCreditNote;
        protected int[] m_LinkedInvoiceSettlement;
        protected int[] m_LinkedInvoice;
        #endregion Members
        #region Accessors
        #region DtEventClassLinked
        protected DataTable DtEventClassLinked
        {
            get { return m_DsEventLinked.DtEventClass; }
        }
        #endregion DtEventClassLinked
        #region DtEventLinked
        public DataTable DtEventLinked
        {
            get { return m_DsEventLinked.DtEvent; }
        }

        #endregion DtEventLinked
        #region IsInvoiceSettlement
        private bool IsInvoiceSettlement
        {
            get { return TradeLibrary.Product.ProductBase.IsInvoiceSettlement; }
        }
        #endregion IsInvoiceSettlement
        #region RowALDAmount
        protected DataRow[] RowALDAmount
        {
            get
            {
                return DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}' and EVENTTYPE = '{2}'",
                m_ParamIdT.Value, EventCodeFunc.AllocatedInvoiceDates, EventTypeFunc.CashSettlement) , "IDE");
            }
        }
        #endregion RowALDAmount
        #region RowLinkedALDAmount
        protected DataRow[] RowLinkedALDAmount
        {
            get
            {
                return DtEventLinked.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}' and EVENTTYPE = '{2}'",
                m_ParamIdT.Value, EventCodeFunc.AllocatedInvoiceDates, EventTypeFunc.CashSettlement), "IDE");
            }
        }
        #endregion RowLinkedALDAmount
        #region RowLinkedTrade
        protected DataRow RowLinkedTrade
        {
            get
            {
                DataRow[] rowTrade = m_DsEventLinked.DtEvent.Select("IDT=" + m_ParamIdT.Value.ToString(), "IDE");
                if (0 < rowTrade.Length)
                    return rowTrade[0];
                return null;
            }
        }
        #endregion RowLinkedTrade
        #region RowLinkedGTOAmount
        protected DataRow[] RowLinkedGTOAmount
        {
            get
            {
                return m_DsEventLinked.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTTYPE = '{1}'", m_ParamIdT.Value,  EventTypeFunc.GrossTurnOverAmount), "IDE");
            }
        }
        #endregion RowLinkedGTOAmount
        #endregion Accessors
        #region Constructors
        // EG 20180502 Analyse du code Correction [CA2214]
        public TradeAdminActionGenRemoveTrade(TradeActionGenProcess pTradeActionGenProcess, DataSetTrade pDsTrade,
            EFS_TradeLibrary pTradeLibrary, TradeAdminActionMQueue pTradeAction)
            : base(pTradeActionGenProcess, pDsTrade, pTradeLibrary, pTradeAction)
        {
            //if (null == RowRemoveTrade)
            //    CodeReturn = Valorize();
            //else
            //{
            //    throw new SpheresException(MethodInfo.GetCurrentMethod().Name,"Trade is yet removed",
            //        new ProcessState(ProcessStateTools.StatusWarningEnum, ProcessStateTools.CodeReturnAbortedEnum));
            //}
        }
        #endregion Constructors
        #region Methods
        #region Valorize
        // EG 20180502 Analyse du code Correction [CA2214]
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (null == RowRemoveTrade)
            {
                bool isOk = true;
                IDbTransaction dbTransaction = null;
                try
                {
                    if (m_TradeActionGenProcess.ProcessCall == ProcessBase.ProcessCallEnum.Master)
                    {
                        // PM 20210121 [XXXXX] Passage du message au niveau de log None
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, IsInvoiceSettlement ? 5240 : 5241), 0,
                            new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id))));
                    }
                    
                    foreach (ActionMsgBase actionMsg in m_TradeAction.ActionMsgs)
                    {
                        if (IsRemoveTradeMsg(actionMsg))
                        {
                            RemoveTradeMsg removeTradeMsg = actionMsg as RemoveTradeMsg;
                            #region Remove Trade Process
                            if (removeTradeMsg.lstLinkedTradeIdSpecified)
                            {
                                dbTransaction = DataHelper.BeginTran(m_CS);
                                //
                                int newIdE = 0;
                                int nbIdE = 0;
                                ret = SetLinkedTrade(removeTradeMsg.lstLinkedTradeId);
                                if (Cst.ErrLevel.SUCCESS == ret)
                                {
                                    #region Nombre de jetons pour inserer l'événement REMOVE informatif
                                    // Si annulation REGLEMENT
                                    // 1 EVT RMV pour REGLEMENT 
                                    if (IsInvoiceSettlement)
                                        nbIdE = m_LinkedInvoiceSettlement.Length;
                                    else
                                    {
                                        nbIdE = m_LinkedInvoice.Length;
                                        if (null != m_LinkedCreditNote)
                                            nbIdE += m_LinkedCreditNote.Length;
                                    }
                                    ret = SQLUP.GetId(out newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbIdE);
                                    #endregion Nombre de jeton pour inserer l'événement REMOVE informatif


                                    if (Cst.ErrLevel.SUCCESS == ret)
                                    {
                                        if (IsInvoiceSettlement)
                                        {
                                            #region ORIGINE = ANNULATION REGLEMENT
                                            #region Factures liées
                                            ret = RemoveInvoiceSettlement(dbTransaction, removeTradeMsg, newIdE);
                                            newIdE += m_LinkedInvoice.Length;
                                            #endregion Factures liées
                                            #endregion ORIGINE = ANNULATION REGLEMENT
                                        }
                                        else
                                        {
                                            #region ORIGINE = ANNULATION FACTURE/AVOIR
                                            // NB : Il n'y a pas/plus de règlements liés 
                                            #region Factures liées
                                            ret = RemoveLinkedTrade(dbTransaction, m_LinkedInvoice, removeTradeMsg, newIdE, EFS.TradeLink.TradeLinkDataIdentification.AddInvoiceIdentifier);
                                            newIdE += m_LinkedInvoice.Length;
                                            #endregion Factures liées
                                            #region Avoirs liés
                                            if ((Cst.ErrLevel.SUCCESS == ret) && (null != m_LinkedCreditNote))
                                            {
                                                ret = RemoveLinkedTrade(dbTransaction, m_LinkedCreditNote, removeTradeMsg, newIdE, EFS.TradeLink.TradeLinkDataIdentification.CreditNoteIdentifier);
                                                newIdE += m_LinkedCreditNote.Length;
                                            }
                                            #endregion Avoirs liés
                                            #endregion ORIGINE = ANNULATION FACTURE/AVOIR
                                        }
                                    }
                                }
                                DataHelper.CommitTran(dbTransaction);
                            }
                            #endregion Remove Trade Process
                        }
                    }
                    return ret;
                }
                catch (SpheresException2)
                {
                    isOk = false;
                    throw;
                }
                catch (Exception ex)
                {
                    isOk = false;
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
                }
                finally
                {
                    if ((false == isOk) && (null != dbTransaction))
                    {
                        try { DataHelper.RollbackTran(dbTransaction); }
                        catch (Exception) { }
                    }
                }
            }
            else
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Trade is yet removed",
                    new ProcessState(ProcessStateTools.StatusWarningEnum, ProcessStateTools.CodeReturnAbortedEnum));
            }
            //return ret;
        }
        #endregion Valorize
        #region RemoveInvoiceSettlement
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private Cst.ErrLevel RemoveInvoiceSettlement(IDbTransaction pDbTransaction,RemoveTradeMsg pRemoveTrade, int pIdE)
        {
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5242), 1,
                new LogParam(LogTools.IdentifierAndId(m_DsTrade.Identifier, m_DsTrade.IdT))));

            int idE = pIdE;
            m_ParamIdT.Value = m_DsTrade.IdT;
            DataRow rowRemoveSource = RowTrade;
            int idEParent = Convert.ToInt32(rowRemoveSource["IDE"]);
            #region UpdateRow Event/EventDetail/EventClass Remove Trade
            Cst.ErrLevel ret = AddRowRemove(rowRemoveSource, idE, idEParent, pRemoveTrade.actionDate.Date, pRemoveTrade.note);
            #endregion UpdateRow Event/EventDetail/EventClass Remove Trade
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                #region UpdateRow Event/EventDetail/EventClass Remove Trade
                ret = base.DeactivAllEvents(DtEvent);
                #endregion UpdateRow Event/EventDetail/EventClass Remove Trade
            }
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                #region Facture liée
                ret = ReadLinkedALD(pDbTransaction, m_LinkedInvoice, pRemoveTrade);
                #endregion Facture liée
            }
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7103), 1,
                    new LogParam(LogTools.IdentifierAndId(m_DsTrade.Identifier, m_DsTrade.IdT))));

                #region UpdateRow IDSTACTIVATION = DEACTIV for Trade (InvoiceSettlement face)
                // FI 20200820 [25468] Dates systèmes en UTC
                DateTime dtsys = OTCmlHelper.GetDateSysUTC(m_CS);
                DataRow rowTradeStSys = m_DsTrade.DtTrade.Rows[0];
                rowTradeStSys.BeginEdit();
                rowTradeStSys["IDSTACTIVATION"] = Cst.StatusActivation.DEACTIV.ToString();
                rowTradeStSys["IDASTACTIVATION"] = m_TradeActionGenProcess.Session.IdA;
                rowTradeStSys["DTSTACTIVATION"] = dtsys;
                rowTradeStSys["IDSTUSEDBY"] = Cst.StatusActivation.REGULAR.ToString();
                rowTradeStSys["IDASTUSEDBY"] = m_TradeActionGenProcess.Session.IdA;
                rowTradeStSys["DTSTUSEDBY"] = dtsys;
                rowTradeStSys["LIBSTUSEDBY"] = Convert.DBNull;
                rowTradeStSys.EndEdit();
                #endregion UpdateRow IDSTACTIVATION = DEACTIV for Trade (InvoiceSettlement face)
            }

            if (Cst.ErrLevel.SUCCESS == ret)
            {
                UpdateLinked(pDbTransaction);
                m_DsEvents.Update(pDbTransaction, m_DsEvents.DtEventDet, Cst.OTCml_TBL.EVENTDET);
                //UpdateEventDet(pDbTransaction);
            }
            return ret;
        }
        #endregion RemoveInvoiceSettlement
        #region RemoveLinkedTrade
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel RemoveLinkedTrade(IDbTransaction pDbTransaction, int[] pLinkId, RemoveTradeMsg pRemoveTrade, int pIdE,
            EFS.TradeLink.TradeLinkDataIdentification pTradeLinkDataIdentification)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            int idE = pIdE;
            foreach (int linkId in pLinkId)
            {
                m_DsTradeLinked = new DataSetTrade(Process.Cs, linkId);
                m_DsEventLinked = new DataSetEventTrade(Process.Cs, linkId);
                m_ParamIdT.Value = linkId;
                DataRow rowRemoveSource = RowLinkedTrade;
                int idEParent = Convert.ToInt32(rowRemoveSource["IDE"]);
                #region UpdateRow Event/EventDetail/EventClass Remove Trade

                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7102), 3,
                    new LogParam(LogTools.IdentifierAndId(m_DsTradeLinked.Identifier, m_DsTradeLinked.IdT))));

                ret = AddRowRemove(rowRemoveSource, idE, idEParent, pRemoveTrade.actionDate.Date, pRemoveTrade.note,true);
                #endregion UpdateRow Event/EventDetail/EventClass Remove Trade
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    #region UpdateRow Event/EventDetail/EventClass Remove Trade
                    ret = base.DeactivAllEvents(DtEventLinked);
                    #endregion UpdateRow Event/EventDetail/EventClass Remove Trade
                }
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    #region Reset IDE Source for Elementary Fees
                    ret = ResetGTOEventSource();
                    #endregion Reset IDE Source for Elementary Fees
                }
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    #region UpdateRow IDSTACTIVATION = DEACTIV for Trade
                    ret = DeactivLinkedTrade();
                    #endregion UpdateRow IDSTACTIVATION = DEACTIV for Trade
                }
                if ((Cst.ErrLevel.SUCCESS == ret) && (m_DsTradeLinked.IdT != m_DsTrade.IdT))
                {
                    #region New TRADELINK (REMOVE SETTLEMENT)
                    EFS.TradeLink.TradeLink tradeLink = new EFS.TradeLink.TradeLink(
                        m_DsTrade.IdT, linkId, EFS.TradeLink.TradeLinkType.RemoveInvoice, null, null,
                            new string[2] { m_DsTrade.Identifier, m_DsTradeLinked.Identifier },
                            new string[2] { EFS.TradeLink.TradeLinkDataIdentification.RemoveInvoiceIdentifier.ToString(), pTradeLinkDataIdentification.ToString() });
                    tradeLink.Insert(Process.Cs, pDbTransaction);
                    #endregion New TRADELINK (REMOVE SETTLEMENT)

                }
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    UpdateLinked(pDbTransaction,true);
                    if (m_DsTradeLinked.IdT == m_DsTrade.IdT)
                        m_DsEvents.Update(pDbTransaction, m_DsEvents.DtEventDet, Cst.OTCml_TBL.EVENTDET);
                        //UpdateEventDet(pDbTransaction);
                }
                idE++;
            }
            return ret;
        }
        #endregion RemoveLinkedTrade

        #region AddRowRemove
        private Cst.ErrLevel AddRowRemove(DataRow pRowSource, int pIdE, int pIdEParent, DateTime pRemoveDate, string pNotes)
        {
            return AddRowRemove(pRowSource, pIdE, pIdEParent, pRemoveDate, pNotes, false);
        }
        private Cst.ErrLevel AddRowRemove(DataRow pRowSource, int pIdE, int pIdEParent, DateTime pRemoveDate, string pNotes, bool pIsLinkedData)
        {

            #region Event
            int idT = Convert.ToInt32(pRowSource["IDT"]);
            DataRow rowRemove;
            if (pIsLinkedData)
                rowRemove = DtEventLinked.NewRow();
            else
                rowRemove = DtEvent.NewRow();
            rowRemove.ItemArray = (object[])pRowSource.ItemArray.Clone();
            rowRemove.BeginEdit();
            rowRemove["IDE"] = pIdE;
            rowRemove["EVENTCODE"] = EventCodeFunc.RemoveTrade;
            rowRemove["EVENTTYPE"] = EventTypeFunc.Date;
            rowRemove["IDE_EVENT"] = pIdEParent;

            // FI 20131007 [] SOURCE contient l'instance du service
            //rowRemove["SOURCE"] = m_TradeActionGenProcess.appInstance.AppNameVersion;
            rowRemove["SOURCE"] = m_TradeActionGenProcess.AppInstance.ServiceName;

            rowRemove.EndEdit();
            SetRowStatus(rowRemove, Tuning.TuningOutputTypeEnum.OES);
            if (pIsLinkedData)
                DtEventLinked.Rows.Add(rowRemove);
            else
                DtEvent.Rows.Add(rowRemove);
            #endregion Event
            #region EventDetail
            if (idT == m_DsTrade.IdT)
            {
                DataRow rowRemoveDetail = m_DsEvents.DtEventDet.NewRow();
                rowRemoveDetail.BeginEdit();
                rowRemoveDetail["IDE"] = pIdE;
                rowRemoveDetail["NOTE"] = pNotes;
                rowRemoveDetail["DTACTION"] = pRemoveDate;
                rowRemoveDetail.EndEdit();
                m_DsEvents.DtEventDet.Rows.Add(rowRemoveDetail);
            }
            DataRow rowEventClassRemove;

            #endregion EventDetail
            #region EventClass
            if (pIsLinkedData)
                rowEventClassRemove = DtEventClassLinked.NewRow();
            else
                rowEventClassRemove = DtEventClass.NewRow();
            rowEventClassRemove.BeginEdit();
            rowEventClassRemove["IDE"] = pIdE;
            rowEventClassRemove["EVENTCLASS"] = EventClassFunc.GroupLevel;
            rowEventClassRemove["DTEVENT"] = pRemoveDate.Date;
            rowEventClassRemove["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, pRemoveDate.Date);
            rowEventClassRemove["ISPAYMENT"] = false;
            rowEventClassRemove.EndEdit();
            if (pIsLinkedData)
                DtEventClassLinked.Rows.Add(rowEventClassRemove);
            else
                DtEventClass.Rows.Add(rowEventClassRemove);
            #endregion EventClass
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion AddRowRemove
        #region DeactivLinkedTrade
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private Cst.ErrLevel DeactivLinkedTrade()
        {
            // FI 20200820 [25468] dates systèmes en UTC
            DateTime dtsys = OTCmlHelper.GetDateSysUTC(m_CS);

            #region UpdateRow IDSTACTIVATION = DEACTIV for linked Trade
            DataRow rowTradeStSys = m_DsTradeLinked.DtTrade.Rows[0];
            //
            rowTradeStSys.BeginEdit();
            rowTradeStSys["IDSTACTIVATION"] = Cst.StatusActivation.DEACTIV.ToString();
            rowTradeStSys["IDASTACTIVATION"] = m_TradeActionGenProcess.Session.IdA;
            rowTradeStSys["DTSTACTIVATION"] = dtsys;
            if (m_DsTradeLinked.IdT == m_DsTrade.IdT)
            {
                rowTradeStSys["IDSTUSEDBY"] = Cst.StatusActivation.REGULAR.ToString();
                rowTradeStSys["IDASTUSEDBY"] = m_TradeActionGenProcess.Session.IdA;
                rowTradeStSys["DTSTUSEDBY"] = dtsys;
                rowTradeStSys["LIBSTUSEDBY"] = Convert.DBNull;
            }
            rowTradeStSys.EndEdit();
            #endregion UpdateRow IDSTACTIVATION = DEACTIV for linked Trade
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion DeactivLinkedTrade
        #region ReadLinkedALD
        // EG 20120116 Plus de RAZ du montant des 3 NETS mais MAJ STATUT ACTIVATION = DEACTIV
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20230222 [WI853][26600] Facturation : Corrections diverses Règlements (ROWATTRIBUT sur annulation)
        private Cst.ErrLevel ReadLinkedALD(IDbTransaction pDbTransaction, int[] pLinkId, RemoveTradeMsg pRemoveTrade)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            bool isLinkedInvoiceFound = false;
            #region Reset VALORISATION for NTO/NTO/NTA child of ALD/CSH  (+ Add EVENTCLASS RMV)
                // RAZ du montant des 3 NETS, des TAXES ASSOCIEES et de l'ECART DE CHANGE POTENTIEL
                foreach (int linkId in pLinkId)
                {
                    m_DsTradeLinked = new DataSetTrade(Process.Cs, linkId);
                    m_DsEventLinked = new DataSetEventTrade(Process.Cs, linkId);
                    m_ParamIdT.Value = linkId;

                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5242), 1,
                        new LogParam(LogTools.IdentifierAndId(m_DsTradeLinked.Identifier, m_DsTradeLinked.IdT))));

                    DataRow[] rowsALD = RowLinkedALDAmount;
                    if ((null != rowsALD) && (0 < rowsALD.Length))
                    {
                        foreach (DataRow rowALD in rowsALD)
                        {
                            int idESource = 0;
                            if (false == Convert.IsDBNull(rowALD["IDE_SOURCE"]))
                                idESource = Convert.ToInt32(rowALD["IDE_SOURCE"]);
                            if (null != RowEvent(idESource))
                            {
                                // Cette allocation correspond bien au règlement en cours d'annulation donc :
                                // REMISE A ZERO DU MONTANT et AJOUT EVENTCLASS = RMV
                                // pour tous les événements enfants de ALD/CSH (NTO/NTI/NTA,TXO/TXI/TXA,FXP,FXL et leurs enfants (TAX DETAILS)
                                isLinkedInvoiceFound = true;
                                // EG 20120116 Désactivation des montants de règlement sur chaque facture allouée par le règlement en cours d'annulation
                                ret = ResetLinkedALD(rowALD, pRemoveTrade);
                            }
                        }
                        if (isLinkedInvoiceFound)
                        {
                            #region Mise à jour ROWATTRIBUT (CLOSE -> NULL)
                            m_DsTradeLinked.UpdateUnallocatedTradeInvoiceRowAttribute(pDbTransaction, linkId);
                            #endregion Mise à jour ROWATTRIBUT (CLOSE -> NULL)

                            #region New TRADELINK (REMOVE SETTLEMENT)
                            EFS.TradeLink.TradeLink tradeLink = new EFS.TradeLink.TradeLink(
                                m_DsTrade.IdT, linkId, EFS.TradeLink.TradeLinkType.RemoveStlInvoice, null, null,
                                    new string[2] { m_DsTrade.Identifier, m_DsTradeLinked.Identifier},
                                    new string[2] { EFS.TradeLink.TradeLinkDataIdentification.InvoiceSettlementIdentifier.ToString(), EFS.TradeLink.TradeLinkDataIdentification.AllocatedInvoiceIdentifier.ToString() });
                            tradeLink.Insert(Process.Cs, pDbTransaction);
                            #endregion New TRADELINK (REMOVE SETTLEMENT)

                            if (Cst.ErrLevel.SUCCESS == ret)
                                UpdateLinked(pDbTransaction, true);
                        }
                    }
                }
                #endregion UpdateRow IDE_SOURCE = null for ALD/CSH (InvoiceSettlement)
            return ret;
        }
        #endregion ReadLinkedALD
        #region ResetLinkedALD
        private Cst.ErrLevel ResetLinkedALD(DataRow pRow, RemoveTradeMsg pRemoveTrade)
        {
            
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            DataRow[] rowChilds = pRow.GetChildRows(m_DsEventLinked.ChildEvent);
            if (null != rowChilds)
            {
                DataRow rowEventClassRemove;
                foreach (DataRow rowChild in rowChilds)
                {
                    rowChild.BeginEdit();
                    // EG 20120116 Désactivation des montants de règlement sur chaque facture allouée par le règlement en cours d'annulation
                    // AVANT MODIF : Montant de valorisation mis à ZERO
                    // APRES MODIF : Montant d'allocation non touché, mais STATUT d'ACTIVATION = DEACTIV 
                    //               Et la vue VW_ALLOCATEDINVOICESTL est modifiée pour ne prendre que les montant REGULAR des factures allouées 
                    //rowChild["VALORISATION"] = 0;
                    rowChild["IDSTACTIVATION"] = Cst.StatusActivation.DEACTIV.ToString();
                    rowChild["IDASTACTIVATION"] = m_TradeActionGenProcess.Session.IdA;
                    rowChild["DTSTACTIVATION"] = OTCmlHelper.GetDateSysUTC(m_CS);
                    rowChild.EndEdit();
                    #region EventClass
                    rowEventClassRemove = DtEventClassLinked.NewRow();
                    rowEventClassRemove.BeginEdit();
                    rowEventClassRemove["IDE"] = Convert.ToInt32(rowChild["IDE"]);
                    rowEventClassRemove["EVENTCLASS"] = EventClassFunc.RemoveEvent;
                    rowEventClassRemove["DTEVENT"] = pRemoveTrade.actionDate.Date;
                    rowEventClassRemove["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, pRemoveTrade.actionDate.Date);
                    rowEventClassRemove["ISPAYMENT"] = true;
                    rowEventClassRemove.EndEdit();
                    DtEventClassLinked.Rows.Add(rowEventClassRemove);
                    #endregion EventClass

                    ret = ResetLinkedALD(rowChild, pRemoveTrade);
                }
                // EG 20120214 Remove de l'événement ALD/CSH
                if (EventCodeFunc.IsAllocatedInvoiceDates(pRow["EVENTCODE"].ToString()))
                {
                    pRow.BeginEdit();
                    pRow["IDSTACTIVATION"] = Cst.StatusActivation.DEACTIV.ToString();
                    pRow["IDASTACTIVATION"] = m_TradeActionGenProcess.Session.IdA;
                    pRow["DTSTACTIVATION"] = OTCmlHelper.GetDateSysUTC(m_CS);
                    pRow.EndEdit();
                    #region EventClass
                    rowEventClassRemove = DtEventClassLinked.NewRow();
                    rowEventClassRemove.BeginEdit();
                    rowEventClassRemove["IDE"] = Convert.ToInt32(pRow["IDE"]);
                    rowEventClassRemove["EVENTCLASS"] = EventClassFunc.RemoveEvent;
                    rowEventClassRemove["DTEVENT"] = pRemoveTrade.actionDate.Date;
                    rowEventClassRemove["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, pRemoveTrade.actionDate.Date);
                    rowEventClassRemove["ISPAYMENT"] = false;
                    rowEventClassRemove.EndEdit();
                    DtEventClassLinked.Rows.Add(rowEventClassRemove);
                }
                #endregion EventClass
            }
            return ret;
        }
        #endregion ReadLinkedALD
        #region ResetGTOEventSource
        private Cst.ErrLevel ResetGTOEventSource()
        {
            #region UpdateRow IDE_SOURCE = null for FEE (Childs of GTO)
            DataRow[] rowsGTO = RowLinkedGTOAmount;
            if (null != rowsGTO)
            {
                foreach (DataRow rowGTO in rowsGTO)
                {
                    DataRow[] rowChilds = rowGTO.GetChildRows(m_DsEventLinked.ChildEvent);
                    if (null != rowChilds)
                    {
                        foreach (DataRow rowChild in rowChilds)
                        {
                            rowChild.BeginEdit();
                            rowChild["IDE_SOURCE"] = Convert.DBNull;
                            rowChild.EndEdit();
                        }
                    }
                }
            }
            #endregion UpdateRow IDE_SOURCE = null for FEE (Childs of GTO)
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion ResetGTOEventSource
        #region SetLinkedTrade
        private Cst.ErrLevel SetLinkedTrade(DictionaryEntry[] pLstLinkedTradeId)
        {
            foreach (DictionaryEntry de in pLstLinkedTradeId)
            {
                TradeLinkDataIdentification key = (TradeLinkDataIdentification)Enum.Parse(typeof(TradeLinkDataIdentification), de.Key.ToString(), true);

                string[] lstId = de.Value.ToString().Split(';');
                int nbLstId = lstId.Length;
                int [] ids = new int[nbLstId];
                int i=0;
                foreach (string id in lstId)
                {
                    ids[i] = Convert.ToInt32(id);
                    i++;
                }
                switch (key)
                {
                    case TradeLinkDataIdentification.CreditNoteIdentifier:
                        m_LinkedCreditNote = ids;
                        break;
                    case TradeLinkDataIdentification.InvoiceIdentifier:
                        m_LinkedInvoice = ids;
                        break;
                    case TradeLinkDataIdentification.InvoiceSettlementIdentifier:
                        m_LinkedInvoiceSettlement = ids;
                        break;
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion SetLinkedTrade
        #region UpdateLinked
        protected void UpdateLinked(IDbTransaction pDbTransaction)
        {
            UpdateLinked(pDbTransaction, false);
        }
        protected void UpdateLinked(IDbTransaction pDbTransaction, bool pIsLinkedData)
        {
            DataTable dtChanges;
            if (pIsLinkedData)
            {
                dtChanges = DtEventLinked.GetChanges();
                m_DsTradeLinked.UpdateTradeStSys(pDbTransaction);
                m_DsTradeLinked.UpdateTradeXML(pDbTransaction);
                m_DsEventLinked.Update(pDbTransaction);
            }
            else
            {
                dtChanges = DtEvent.GetChanges();
                m_DsTrade.UpdateTradeStSys(pDbTransaction);
                m_DsEvents.Update(pDbTransaction);
            }
            if (null != dtChanges)
            {
                DateTime dtSys = OTCmlHelper.GetDateSys(m_CS);
                EventProcess eventProcess = new EventProcess(m_CS); 
                foreach (DataRow row in dtChanges.Rows)
                {
                    if (DataRowState.Modified == row.RowState)
                    {
                        int idE = Convert.ToInt32(row["IDE"]);
                        eventProcess.Write(pDbTransaction, idE, Cst.ProcessTypeEnum.TRADEACTGEN, ProcessStateTools.StatusSuccessEnum, 
                            dtSys, m_TradeActionGenProcess.Tracker.IdTRK_L);
                    }
                }
            }
        }
        #endregion Update
        #endregion Methods
    }
    #endregion TradeActionGenCancellation
}

#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process.EventsGen;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum.Tools;
using System;
using System.Collections;
using System.Collections.Generic;//PL 20200109 Add
using System.Data;
using System.Reflection;
#endregion Using Directives

namespace EFS.Process
{
    #region TradeActionGenRemoveTrade
    public class TradeActionGenProcessRemoveTrade : TradeActionGenProcessBase
	{
		#region Members
		protected DataSetTrade m_DsTradeReplace;
		#endregion Members
		#region Constructors
        // EG 20180502 Analyse du code Correction [CA2214]
        public TradeActionGenProcessRemoveTrade(TradeActionGenProcess pTradeActionGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, TradeActionMQueue pTradeAction)
            : base(pTradeActionGenProcess, pDsTrade, pTradeLibrary, pTradeAction)
		{
            //if (null == RowRemoveTrade)
            //{
            //    CodeReturn = Valorize();
            //}
            //else
            //{
            //    throw new SpheresException(MethodInfo.GetCurrentMethod().Name, "Trade is yet removed",
            //        new ProcessState(ProcessStateTools.StatusWarningEnum, ProcessStateTools.CodeReturnAbortedEnum));
            //}
		}
		#endregion Constructors

		#region Methods
		#region Valorize
        // EG 20180502 Analyse du code Correction [CA2214]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (null == RowRemoveTrade)
            {
                int idTReplace = 0;
                string identifierReplace = string.Empty;
                bool isEventsReplace = false;
                

                if (m_TradeActionGenProcess.ProcessCall == ProcessBase.ProcessCallEnum.Master)
                {
                    // PM 20210121 [XXXXX] Passage du message au niveau de log None
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7100), 0,
                        new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id))));
                }
                foreach (ActionMsgBase actionMsg in m_TradeAction.ActionMsgs)
                {
                    if (IsRemoveTradeEventMsg(actionMsg) || IsRemoveTradeMsg(actionMsg))
                    {
                        DataRow rowRemoveSource = null;
                        int newIdE = 0;
                        // EG 20160107 [POC-MUREX] Utilisation Transaction si slaveCall avec transaction
                        if ((m_TradeActionGenProcess.ProcessCall == ProcessBase.ProcessCallEnum.Slave) &&
                            (null != m_TradeActionGenProcess.SlaveDbTransaction))
                            ret = SQLUP.GetId(out newIdE, m_TradeActionGenProcess.SlaveDbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
                        else
                            ret = SQLUP.GetId(out newIdE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);

                        //ret = SQLUP.GetId(out newIdE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
                        if (Cst.ErrLevel.SUCCESS == ret)
                        {
                            if (IsRemoveTradeEventMsg(actionMsg))
                            {
                                #region Remove Process
                                RemoveTradeEventMsg removeTradeEventMsg = actionMsg as RemoveTradeEventMsg;
                                rowRemoveSource = RowEvent(removeTradeEventMsg.idE);
                                if (Cst.ErrLevel.SUCCESS == ret)
                                {
                                    #region UpdateRow Event/EventDetail/EventClass Cancellation
                                    ret = AddRowRemove(rowRemoveSource, newIdE, removeTradeEventMsg.idE, removeTradeEventMsg.actionDate.Date, removeTradeEventMsg.note);
                                    #endregion UpdateRow Event/EventDetail/EventClass Cancellation
                                }
                                #endregion Cancellation Process
                            }
                            else if (IsRemoveTradeMsg(actionMsg))
                            {
                                #region Remove Trade Process
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7102), 3,
                                    new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id))));

                                RemoveTradeMsg removeTradeMsg = actionMsg as RemoveTradeMsg;
                                rowRemoveSource = RowTrade;
                                idTReplace = removeTradeMsg.idTReplace;
                                isEventsReplace = removeTradeMsg.isEventsReplace;
                                if (null != rowRemoveSource)
                                {
                                    // UpdateRow Event/EventDetail/EventClass Remove Trade
                                    ret = AddRowRemove(rowRemoveSource, newIdE, Convert.ToInt32(rowRemoveSource["IDE"]), removeTradeMsg.actionDate.Date, removeTradeMsg.note);
                                }
                                #endregion Remove Trade Process
                            }
                        }
                        if (Cst.ErrLevel.SUCCESS == ret)
                        {
                            ret = base.DeactivAllEvents(DtEvent);
                            #region UpdateRow IDSTACTIVATION = DEACTIV for trade
                            DataRow rowTradeStSys = DtTrade.Rows[0];
                            rowTradeStSys.BeginEdit();
                            rowTradeStSys["IDSTACTIVATION"] = Cst.StatusActivation.DEACTIV.ToString();
                            rowTradeStSys["IDASTACTIVATION"] = m_TradeActionGenProcess.Session.IdA;
                            rowTradeStSys["DTSTACTIVATION"] = OTCmlHelper.GetDateSysUTC(m_CS);
                            rowTradeStSys.EndEdit();
                            #endregion UpdateRow IDSTACTIVATION = DEACTIV for trade
                        }

                        //GLOPXXX See EG
                        if (m_tradeLibrary.CurrentTrade.OtherPartyPaymentSpecified
                            && (m_tradeLibrary.CurrentTrade.OtherPartyPayment[0].Efs_Payment == null))
                        {
                            ret = PaymentTools.CalcPayments(m_CS, m_TradeActionGenProcess.SlaveDbTransaction, m_tradeLibrary.Product,
                                m_tradeLibrary.CurrentTrade.OtherPartyPayment, m_tradeLibrary.DataDocument);
                        }

                        bool existsPayment_OnFeeScopeOrderId = false;
                        bool existsPayment_OnFeeScopeFolderId = false;
                        // PL 20200107 [25099] New
                        if (IsRemoveTradeMsg(actionMsg)
                            && ProcessStateTools.IsCodeReturnSuccess(ret)
                            && m_tradeLibrary.CurrentTrade.OtherPartyPaymentSpecified
                            && PaymentTools.IsExistsPayment_OnFeeScopeOrderIdOrFolderId_WithMinMax(m_tradeLibrary.CurrentTrade.OtherPartyPayment,
                                                                                                   ref existsPayment_OnFeeScopeOrderId, ref existsPayment_OnFeeScopeFolderId)
                            )
                        {
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            #region Lock de l'ordre et/ou du dossier
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            string orderId = string.Empty;
                            string folderId = string.Empty;
                            if (existsPayment_OnFeeScopeOrderId)
                            {
                                orderId = m_tradeLibrary.DataDocument.GetOrderId();
                                ret = m_TradeActionGenProcess.LockObjectId(TypeLockEnum.TRADE, Cst.FeeScopeEnum.OrderId.ToString() + ":" + orderId, orderId, m_TradeActionGenMQueue.ProcessType.ToString(), LockTools.Exclusive.ToString());
                            }
                            if (existsPayment_OnFeeScopeFolderId && ProcessStateTools.IsCodeReturnSuccess(ret))
                            {
                                folderId = ""; // m_tradeLibrary.dataDocument.GetFolderId(); //GLOP25099 TODO GetFolderId()
                                ret = m_TradeActionGenProcess.LockObjectId(TypeLockEnum.TRADE, Cst.FeeScopeEnum.FolderId.ToString() + ":" + folderId, folderId, m_TradeActionGenMQueue.ProcessType.ToString(), LockTools.Exclusive.ToString());
                                ret = Cst.ErrLevel.LOCKUNSUCCESSFUL;
                            }
                            #endregion Lock de l'ordre et/ou du dossier

                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            #region Recalcul des Frais relatifs à Ordre/Dossier avec Min/max sur les autres trades, qui appartiennent au même Ordre/Dossier
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            if (ProcessStateTools.IsCodeReturnSuccess(ret))
                            {
                                if (existsPayment_OnFeeScopeOrderId)
                                {
                                    #region Recalcul des Frais relatifs à un Ordre
                                    if (null != m_DsEvents.DtEventFee)
                                    {
                                        List<KeyValuePair<int, string>> allTradeIdToPerform = new List<KeyValuePair<int, string>>();
                                        List<FeeSheduleId> allFeeScheduleIdToPerform = new List<FeeSheduleId>();

                                        int idT = m_TradeActionGenProcess.MQueue.id;
                                        DataRow[] aDrEvent_OPP = m_DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT={0} and EVENTCODE='{1}'",
                                                                                           idT, EventCodeFunc.OtherPartyPayment), "IDE");
                                        foreach (DataRow drEvent_OPP in aDrEvent_OPP)
                                        {
                                            DataRow[] aDrEventFee = m_DsEvents.DtEventFee.Select(StrFunc.AppendFormat(@"IDE={0} and STATUS='{1}' and FEESCOPE='{2}' 
                                                                                                 and IDFEESCHEDULE>0 and IDFEEMATRIX>0 and (FORMULAMIN is not null or FORMULAMAX is not null)",
                                                                                                 drEvent_OPP["IDE"], EfsML.Enum.SpheresSourceStatusEnum.Default.ToString(), Cst.FeeScopeEnum.OrderId.ToString()), "IDE");
                                            foreach (DataRow drEventFee in aDrEventFee)
                                            {
                                                #region EventFee
                                                string formulaMin = drEventFee["FORMULAMIN"].ToString();
                                                string formulaMax = drEventFee["FORMULAMAX"].ToString();

                                                //Lecture des frais déjà appliqués sur toutes les exécutions relatives au même Ordre.
                                                //NB: le trade en cours d'annulation y est exclu de par son statut IDSTUSEDBY='Reserved'
                                                QueryParameters qry = null;
                                                int order_NumberOfExecution = PaymentTools.CountNumberOfTrades(m_CS, m_TradeActionGenProcess.SlaveDbTransaction, Cst.FeeScopeEnum.OrderId, orderId,
                                                                                                           m_tradeLibrary.DataDocument.TradeHeader.ClearedDate.DateValue,
                                                                                                           ref qry,
                                                                                                           drEvent_OPP["EVENTTYPE"].ToString(), drEvent_OPP["UNIT"].ToString(), 
                                                                                                           Convert.ToInt32(drEventFee["IDFEE"]), Convert.ToInt32(drEventFee["IDFEEMATRIX"]), Convert.ToInt32(drEventFee["IDFEESCHEDULE"]),
                                                                                                           Convert.ToInt32(drEvent_OPP["IDA_PAY"]), Convert.ToInt32(drEvent_OPP["IDA_REC"]));
                                                if (order_NumberOfExecution > 0)
                                                {
                                                    if (!string.IsNullOrEmpty(formulaMin))
                                                    {
                                                        using (IDataReader dr = DataHelper.ExecuteReader(m_CS, m_TradeActionGenProcess.SlaveDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
                                                        {
                                                            bool isExistsAnotherTradeWithTOUCHED_NO_AmountGreaterThanMIN = false; 
                                                            bool isExistsAnotherTradeWithTOUCHED_NO = false; 
                                                            bool isExistsAnotherTradeWithTOUCHED_YES = false;
                                                            bool isExistsAnotherTradeWithTOUCHED_YESPARTIALLY = false;
                                                            FeeSheduleId currentFeeScheduleId = null;
                                                            while (dr.Read())
                                                            {
                                                                if (currentFeeScheduleId == null)
                                                                    currentFeeScheduleId = new FeeSheduleId(new KeyValuePair<int, string>(Convert.ToInt32(drEventFee["IDFEESCHEDULE"]), dr["FEESCHEDULE_IDENTIFIER"].ToString()));
                                                                
                                                                if (0 <= Convert.ToDateTime(dr["DTBUSINESS"]).CompareTo(m_tradeLibrary.DataDocument.TradeHeader.ClearedDate.DateValue))
                                                                {
                                                                    //WARNING: Audit de tous les Trades, mais seuls seront recalculés ceux ayant une date supérieure ou égale à celle du Trade en cours d'annulation
                                                                    if (!allTradeIdToPerform.Exists(item => item.Key == Convert.ToInt32(dr["IDT"])))
                                                                        allTradeIdToPerform.Add(new KeyValuePair<int, string>(Convert.ToInt32(dr["IDT"]), dr["IDENTIFIER"].ToString()));
                                                                }

                                                                #region Vérification d'application du MIN sur une des autres exécutions 
                                                                if (dr["FORMULAMIN"].ToString().IndexOf(Cst.TOUCHED_NO) > 0)
                                                                {
                                                                    isExistsAnotherTradeWithTOUCHED_NO = true;

                                                                    string[] separators = { "[", "]" };
                                                                    string[] split = Convert.ToString(dr["FORMULAMIN"]).Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                                                    string MINMoney = split[0];
                                                                    decimal MINAmountValue = 0;
                                                                    if (MINMoney.StartsWith(drEvent_OPP["UNIT"].ToString()))
                                                                    {
                                                                        //NB: Si devise du montant MIN différente de la devise du montant de frais --> Comparaison impossible
                                                                        MINAmountValue = DecFunc.DecValue(MINMoney.Substring(4));
                                                                    }
                                                                    if ((Convert.ToDecimal(dr["VALORISATION"]) > MINAmountValue) && (MINAmountValue > 0))
                                                                        isExistsAnotherTradeWithTOUCHED_NO_AmountGreaterThanMIN = true;
                                                                }
                                                                isExistsAnotherTradeWithTOUCHED_YES |= (dr["FORMULAMIN"].ToString().IndexOf(Cst.TOUCHED_YES) > 0);
                                                                isExistsAnotherTradeWithTOUCHED_YESPARTIALLY |= (dr["FORMULAMIN"].ToString().IndexOf(Cst.TOUCHED_YESPARTIALLY) > 0);
                                                                
                                                                if (isExistsAnotherTradeWithTOUCHED_YES && isExistsAnotherTradeWithTOUCHED_YESPARTIALLY && isExistsAnotherTradeWithTOUCHED_NO && isExistsAnotherTradeWithTOUCHED_NO_AmountGreaterThanMIN)
                                                                    break;
                                                                #endregion
                                                            }

                                                            #region Règles d'identification d'un recalcul à opérer
                                                            bool isPerformRecalc = false;

                                                            //Sur le Trade en cours d'annulation, le MIN n'avait pas été appliqué (soit déjà appliqué sur un autre Trade, soit aucun autre Trade n'était sous le MIN)
                                                            if (formulaMin.IndexOf(Cst.TOUCHED_NO) > 0)
                                                            {
                                                                if (isExistsAnotherTradeWithTOUCHED_YES)
                                                                {
                                                                    //Le montant MIN est appliqué.
                                                                    isPerformRecalc = false;
                                                                }
                                                                else if (isExistsAnotherTradeWithTOUCHED_NO && isExistsAnotherTradeWithTOUCHED_NO_AmountGreaterThanMIN)
                                                                {
                                                                    //Le montant MIN est dépassé. Si on opérait à nouveau le calcul on pourrait éventuellement avoir l’application du montant MIN, mais le total sur l’ordre resterait identique.
                                                                    isPerformRecalc = false;
                                                                }
                                                                else
                                                                {
                                                                    isPerformRecalc = true;
                                                                }
                                                            }
                                                            //Sur le Trade en cours d'annulation, le MIN avait été appliqué
                                                            else if (formulaMin.IndexOf(Cst.TOUCHED_YES) > 0)
                                                            {
                                                                isPerformRecalc = true;
                                                            }
                                                            //Sur le Trade en cours d'annulation, le MIN n'avait pas été appliqué car déjà appliqué sur un autre Trade
                                                            else if (formulaMin.IndexOf(Cst.TOUCHED_YESALREADY) > 0)
                                                            {
                                                                if (isExistsAnotherTradeWithTOUCHED_NO || isExistsAnotherTradeWithTOUCHED_YESPARTIALLY)
                                                                    isPerformRecalc = true;
                                                            }
                                                            //Sur le Trade en cours d'annulation, le MIN n'avait pas été appliqué car déjà appliqué sur un autre Trade, cependant le montant calculé est partiel...
                                                            else if (formulaMin.IndexOf(Cst.TOUCHED_YESPARTIALLY) > 0)
                                                            {
                                                                if (isExistsAnotherTradeWithTOUCHED_NO)
                                                                    isPerformRecalc = true;
                                                            }

                                                            if (isPerformRecalc)
                                                            {
                                                                if (!allFeeScheduleIdToPerform.Exists(item => item.OTCmlId == Convert.ToInt32(drEventFee["IDFEESCHEDULE"])))
                                                                    allFeeScheduleIdToPerform.Add(currentFeeScheduleId);
                                                            }
                                                            #endregion 
                                                        }
                                                    }
                                                }
                                                #endregion EventFee
                                            }
                                        }

                                        if (allFeeScheduleIdToPerform.Count > 0)
                                        {
                                            //Pour chaque trade relatif au même ordre que celui en cours d'annulation et avec une date supérieure ou égale à celui-ci
                                            //--> on opère un recalcul des frais, uniquement pour le ou les barèmes concernés...
                                            FeesCalculationSettingsMode2 feesCalculationSetting = new FeesCalculationSettingsMode2
                                            {
                                                feeSheduleSpecified = true,
                                                feeShedule = allFeeScheduleIdToPerform.ToArray()
                                            };
                                            for (int i = 0; i < allTradeIdToPerform.Count; i++)
                                            {
                                                feesCalculationSetting.trade = allTradeIdToPerform[i];
                                                FeeCalculationAndWrite(feesCalculationSetting);
                                            }
                                        }
                                    }
                                    #endregion Recalcul des Frais relatifs à un Ordre
                                }
                            }
                            #endregion Recalcul des Frais relatifs à Ordre/Dossier avec Min/max sur les autres trades, qui appartiennent au même Ordre/Dossier
                        }
                    }
                }

                if ((Cst.ErrLevel.SUCCESS == ret) && (0 < idTReplace) && isEventsReplace)
                {
                    #region Remplacante
                    // Insertion LOG Détail Remplacante
                    // PM 20210121 [XXXXX] Passage du message au niveau de log None
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7101), 0,
                        new LogParam(LogTools.IdentifierAndId(identifierReplace, idTReplace))));
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7103), 1,
                        new LogParam(LogTools.IdentifierAndId(identifierReplace, idTReplace))));

                    ret = UpdateStUsedByForTradeReplace(idTReplace);
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    Update();
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        // Insertion LOG Détail génération des événements
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 500), 1,
                            new LogParam(LogTools.IdentifierAndId(identifierReplace, idTReplace))));

                        

                        MQueueAttributes mQueueAttributes = new MQueueAttributes()
                        {
                            connectionString = m_TradeActionGenProcess.Cs,
                            id = idTReplace,
                            idInfo = new IdInfo()
                            {
                                id = idTReplace,
                                idInfos = new DictionaryEntry[]{
                                                new DictionaryEntry("ident", "TRADE"),
                                                new DictionaryEntry("identifier", identifierReplace),
                                                new DictionaryEntry("GPRODUCT", m_TradeActionGenProcess.MQueue.GetStringValueIdInfoByKey("GPRODUCT"))}
                            },
                            requester = m_TradeActionGenProcess.MQueue.header.requester
                        };
                        ProcessState processState = New_EventsGenAPI.ExecuteSlaveCall(new EventsGenMQueue(mQueueAttributes), null,  m_TradeActionGenProcess,  false);
                        ret = processState.CodeReturn;
                        if (Cst.ErrLevel.SUCCESS == ret)
                        {
                            // Insertion LOG Detail Valorisation des événements
                            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 600), 1,
                                new LogParam(LogTools.IdentifierAndId(identifierReplace, idTReplace))));

                            processState = New_EventsValAPI.ExecuteSlaveCall(new EventsValMQueue(mQueueAttributes), m_TradeActionGenProcess, false, true);
                            ret = processState.CodeReturn;
                        }
                    }
                    #endregion Remplacante
                }
                else
                {
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    Update();
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                }
            }
            else
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Trade is yet removed",
                                           new ProcessState(ProcessStateTools.StatusWarningEnum, ProcessStateTools.CodeReturnAbortedEnum));
            }
            return ret;
        }	
		#endregion Valorize
		#region AddRowRemove
        private Cst.ErrLevel AddRowRemove(DataRow pRowSource, int pIdE, int pIdEParent, DateTime pRemoveDate, string pNotes)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            #region Event
            DataRow rowRemove = DtEvent.NewRow();
            rowRemove.ItemArray = (object[])pRowSource.ItemArray.Clone();
            rowRemove.BeginEdit();
            rowRemove["IDE"] = pIdE;
            rowRemove["EVENTCODE"] = EventCodeFunc.RemoveTrade;
            rowRemove["EVENTTYPE"] = EventTypeFunc.Date;
            rowRemove["IDE_EVENT"] = pIdEParent;
            rowRemove["SOURCE"] = m_TradeActionGenProcess.AppInstance.ServiceName;
            rowRemove.EndEdit();
            SetRowStatus(rowRemove, Tuning.TuningOutputTypeEnum.OES);
            DtEvent.Rows.Add(rowRemove);
            #endregion Event
            #region EventDetail
            DataRow rowRemoveDetail = DtEventDet.NewRow();
            rowRemoveDetail.BeginEdit();
            rowRemoveDetail["IDE"] = pIdE;
            rowRemoveDetail["NOTE"] = pNotes;
            rowRemoveDetail["DTACTION"] = pRemoveDate;
            rowRemoveDetail.EndEdit();
            DtEventDet.Rows.Add(rowRemoveDetail);
            #endregion EventDetail
            #region EventClass
            DataRow rowEventClassRemove = DtEventClass.NewRow();
            rowEventClassRemove.BeginEdit();
            rowEventClassRemove["IDE"] = pIdE;
            rowEventClassRemove["EVENTCLASS"] = EventClassFunc.GroupLevel;
            rowEventClassRemove["DTEVENT"] = pRemoveDate.Date;
            rowEventClassRemove["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, pRemoveDate.Date);
            rowEventClassRemove["ISPAYMENT"] = false;
            rowEventClassRemove.EndEdit();
            DtEventClass.Rows.Add(rowEventClassRemove);
            #endregion EventClass

            return ret;
        }
		#endregion AddRowRemove
		#region UpdateStUsedByForTradeReplace
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private Cst.ErrLevel UpdateStUsedByForTradeReplace( int pIdT)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            m_DsTradeReplace = new DataSetTrade(m_CS, pIdT);
            DataRow rowTradeStSys = m_DsTradeReplace.DtTrade.Rows[0];
            //
            rowTradeStSys.BeginEdit();
            rowTradeStSys["IDSTUSEDBY"] = Cst.StatusActivation.REGULAR.ToString();
            rowTradeStSys["IDASTUSEDBY"] = m_TradeActionGenProcess.Session.IdA;
            rowTradeStSys["DTSTUSEDBY"] = OTCmlHelper.GetDateSysUTC(m_CS);
            rowTradeStSys["LIBSTUSEDBY"] = Convert.DBNull;
            rowTradeStSys.EndEdit();
            return ret;
        }
		#endregion UpdateStUsedByForTradeReplace
		#region Update
		protected override void Update()
		{
            bool isOk = true;
            IDbTransaction dbTransaction = null;
            if ((m_TradeActionGenProcess.ProcessCall == ProcessBase.ProcessCallEnum.Slave) &&
                (null != m_TradeActionGenProcess.SlaveDbTransaction))
            {
                dbTransaction = m_TradeActionGenProcess.SlaveDbTransaction;
            }

            try
            {
                if (null == dbTransaction)
                    dbTransaction = DataHelper.BeginTran(m_CS);

                base.Update(dbTransaction);
                if (null != m_DsTradeReplace)
                    m_DsTradeReplace.UpdateTradeStSys(dbTransaction);

                if ((m_TradeActionGenProcess.ProcessCall == ProcessBase.ProcessCallEnum.Master) || (null == m_TradeActionGenProcess.SlaveDbTransaction))
                    DataHelper.CommitTran(dbTransaction);
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
                    try
                    {
                        if ((m_TradeActionGenProcess.ProcessCall ==  ProcessBase.ProcessCallEnum.Master) || (null == m_TradeActionGenProcess.SlaveDbTransaction))
                            DataHelper.RollbackTran(dbTransaction);
                    }
                    catch (Exception ) { }
                }
            }
		}
		#endregion Update
		#endregion Methods
	}
	#endregion TradeActionGenCancellation
}
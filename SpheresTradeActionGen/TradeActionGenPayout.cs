#region Using Directives
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
//
using FpML.Enum;
using FpML.Interface;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.Process
{
    #region TradeActionGenPayout
    public class TradeActionGenProcessPayout : TradeActionGenProcessBase
	{
		#region Constructors
        // EG 20180502 Analyse du code Correction [CA2214]
        public TradeActionGenProcessPayout(TradeActionGenProcess pTradeActionGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary,
            TradeActionMQueue pTradeAction)
            : base(pTradeActionGenProcess, pDsTrade, pTradeLibrary, pTradeAction)
		{
			//CodeReturn = Valorize();
		}
		#endregion Constructors
		#region Methods
		#region AddEventClassCurrencyAmount
        protected DataRow AddEventClassCurrencyAmount(int pIdE, DataRow pRowSource, ActionMsgBase pActionMsg)
		{
			DataRow rowCurrencyAmount = DtEventClass.NewRow();
			rowCurrencyAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
			rowCurrencyAmount.BeginEdit();
			rowCurrencyAmount["IDE"] = pIdE;
			rowCurrencyAmount["EVENTCLASS"] = EventClassFunc.Recognition;
            rowCurrencyAmount["DTEVENT"] = pActionMsg.actionDate.Date;
            rowCurrencyAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, pActionMsg.actionDate.Date);
			rowCurrencyAmount["ISPAYMENT"] = false;
			rowCurrencyAmount.EndEdit();
			DtEventClass.Rows.Add(rowCurrencyAmount);
			return rowCurrencyAmount;
		}
		#endregion AddEventClassCurrencyAmount
		#region AddEventCurrencyAmount
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdE"></param>
        /// <param name="pIdEParent"></param>
        /// <param name="pRowSource"></param>
        /// <param name="pRebateMsg"></param>
        /// <param name="pCurrencyType"></param>
        /// <returns></returns>
        /// FI 20180220 [XXXXX] Modify
        protected DataRow AddEventCurrencyAmount(int pIdE, int pIdEParent, DataRow pRowSource, RebateMsg pRebateMsg, string pCurrencyType)
        {
            
            AmountPayerReceiverInfo amount = null;
            if (pCurrencyType == EventTypeFunc.CallCurrency)
                amount = (AmountPayerReceiverInfo)pRebateMsg.callAmount;
            else if (pCurrencyType == EventTypeFunc.PutCurrency)
                amount = (AmountPayerReceiverInfo)pRebateMsg.putAmount;

            DataRow rowCurrencyAmount = DtEvent.NewRow();
            rowCurrencyAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
            rowCurrencyAmount.BeginEdit();
            rowCurrencyAmount["IDE"] = pIdE;
            rowCurrencyAmount["IDE_EVENT"] = pIdEParent;
            rowCurrencyAmount["EVENTCODE"] = EventCodeFunc.Termination;
            rowCurrencyAmount["EVENTTYPE"] = pCurrencyType;
            rowCurrencyAmount["DTSTARTADJ"] = pRebateMsg.actionDate.Date;
            rowCurrencyAmount["DTSTARTUNADJ"] = pRebateMsg.actionDate.Date;
            rowCurrencyAmount["DTENDADJ"] = pRebateMsg.actionDate.Date;
            rowCurrencyAmount["DTENDUNADJ"] = pRebateMsg.actionDate.Date;
            rowCurrencyAmount["VALORISATION"] = amount.Amount;
            rowCurrencyAmount["UNIT"] = amount.Currency;
            rowCurrencyAmount["UNITTYPE"] = UnitTypeEnum.Currency;
            rowCurrencyAmount["VALORISATIONSYS"] = amount.Amount;
            rowCurrencyAmount["UNITTYPESYS"] = UnitTypeEnum.Currency;
            rowCurrencyAmount["UNITSYS"] = amount.Currency;
            
            // FI 20180220 [XXXXX] Add différents tests sur .First.HasValue
            rowCurrencyAmount["IDA_PAY"] = amount.payer.actor.First ?? Convert.DBNull;
            rowCurrencyAmount["IDB_PAY"] = amount.payer.book.First ?? Convert.DBNull;
            rowCurrencyAmount["IDA_REC"] = amount.receiver.actor.First ?? Convert.DBNull;
            rowCurrencyAmount["IDB_REC"] = amount.receiver.book.First ?? Convert.DBNull;
            
            rowCurrencyAmount.EndEdit();
            DtEvent.Rows.Add(rowCurrencyAmount);
            SetRowStatus(rowCurrencyAmount, TuningOutputTypeEnum.OES);
            return rowCurrencyAmount;
        }
		#endregion AddEventCurrencyAmount
		#region AddEventPayout/Rebate
        protected DataRow AddEventPayout(int pIdE, int pIdEParent, DataRow pRowSource, PayoutMsg pPayoutMsg)
        {
            return AddEventPayout(pIdE, pIdEParent, pRowSource, pPayoutMsg, false);
        }
        protected DataRow AddEventPayout(int pIdE, int pIdEParent, DataRow pRowSource, PayoutMsg pPayoutMsg, bool pIsPayoutBase)
		{
            decimal amount = pPayoutMsg.amount;
            string currency = pPayoutMsg.currency;
            if (pIsPayoutBase)
            {
                amount = pPayoutMsg.baseAmount;
                currency = pPayoutMsg.baseCurrency;
            }
            return AddEventPayoutRebate(pIdE, pIdEParent, pRowSource, (ActionMsgBase)pPayoutMsg,
                EventTypeFunc.Payout, pPayoutMsg.payer, pPayoutMsg.receiver, amount, currency);
		}
        protected DataRow AddEventRebate(int pIdE, int pIdEParent, DataRow pRowSource, RebateMsg pRebateMsg)
        {
            return AddEventRebate(pIdE, pIdEParent, pRowSource, pRebateMsg, false);
        }
        protected DataRow AddEventRebate(int pIdE, int pIdEParent, DataRow pRowSource, RebateMsg pRebateMsg, bool pIsRebateBase)
		{
            decimal amount = pRebateMsg.amount;
            string currency = pRebateMsg.currency;
            if (pIsRebateBase)
            {
                amount = pRebateMsg.baseAmount;
                currency = pRebateMsg.baseCurrency;
            }
            return AddEventPayoutRebate(pIdE, pIdEParent, pRowSource, (ActionMsgBase)pRebateMsg,
                    EventTypeFunc.Rebate, pRebateMsg.payer, pRebateMsg.receiver, amount, currency);
        }
        #endregion AddEventPayout/Rebate
        #region AddEventPayoutRebate
        // EG 20150706 [21021] Nullable<int> IDA_PAY|IDB_PAY|IDA_REC|IDB_REC
        protected DataRow AddEventPayoutRebate(int pIdE, int pIdEParent, DataRow pRowSource, ActionMsgBase pActionMsg,
            string pEventType, string pPayer, string pReceiver, decimal pAmount, string pCurrency)
        {
            DataDocumentContainer dataDocument = m_tradeLibrary.DataDocument;
            // EG 20150706 [21021]
            //IParty payer = dataDocument.GetParty(pPayer);
            //IParty receiver = dataDocument.GetParty(pReceiver);

            DataRow row = DtEvent.NewRow();
            row.ItemArray = (object[])pRowSource.ItemArray.Clone();
            row.BeginEdit();
            row["IDE"] = pIdE;
            row["IDE_EVENT"] = pIdEParent;
            row["EVENTCODE"] = EventCodeFunc.LinkedProductPayment;
            row["EVENTTYPE"] = pEventType;
            row["DTSTARTADJ"] = pActionMsg.actionDate.Date;
            row["DTSTARTUNADJ"] = pActionMsg.actionDate.Date;
            row["DTENDADJ"] = pActionMsg.actionDate.Date;
            row["DTENDUNADJ"] = pActionMsg.actionDate.Date;
            row["UNIT"] = pCurrency;
            row["UNITTYPE"] = UnitTypeEnum.Currency;
            row["VALORISATION"] = pAmount;
            row["UNITSYS"] = pCurrency;
            row["UNITTYPESYS"] = UnitTypeEnum.Currency;
            row["VALORISATIONSYS"] = pAmount;

            // EG 20150706 [21021]
            int? idA_Pay = dataDocument.GetOTCmlId_Party(pPayer);
            int? idB_Pay = dataDocument.GetOTCmlId_Book(pPayer);
            int? idA_Rec = dataDocument.GetOTCmlId_Party(pReceiver);
            int? idB_Rec = dataDocument.GetOTCmlId_Book(pReceiver);
            row["IDA_PAY"] = idA_Pay ?? Convert.DBNull;
            row["IDB_PAY"] = idB_Pay ?? Convert.DBNull;
            row["IDA_REC"] = idA_Rec ?? Convert.DBNull;
            row["IDB_REC"] = idB_Rec ?? Convert.DBNull;
            row["IDSTTRIGGER"] = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
            row["IDSTCALCUL"] = StatusCalculFunc.Calculated;
            row["SOURCE"] = m_TradeActionGenProcess.AppInstance.ServiceName;
            row.EndEdit();
            DtEvent.Rows.Add(row);
            SetRowStatus(row, TuningOutputTypeEnum.OES);
            return row;
        }
		#endregion AddEventPayoutRebate
		#region AddEventClassPayoutRebate
		protected DataRow AddEventClassPayoutRebate(int pIdE, string pClassCode, DateTime pClassDate)
		{
			DataRow row = DtEventClass.NewRow();
			row.BeginEdit();
			row["IDE"]           = pIdE;
			row["EVENTCLASS"]    = pClassCode;
			row["DTEVENT"]       = pClassDate;
			row["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS,pClassDate);
			row["ISPAYMENT"]     = EventClassFunc.IsSettlement(pClassCode);
			row.EndEdit();
			DtEventClass.Rows.Add(row);
			return row;
		}
		#endregion AddEventClassPayoutRebate
		#region AddEventDetailPayout
        protected void AddEventDetailPayout(int pIdE, PayoutMsg pPayoutMsg)
		{
            DataRow rowDetail = DtEventDet.NewRow();
			rowDetail.BeginEdit();
			rowDetail["IDE"]               = pIdE;
            rowDetail["TOTALPAYOUTAMOUNT"] = pPayoutMsg.originalAmount;
            rowDetail["PERIODPAYOUT"] = pPayoutMsg.nbPeriodSpecified ? pPayoutMsg.nbPeriod : Convert.DBNull;
            rowDetail["PCTPAYOUT"] = pPayoutMsg.percentageSpecified ? pPayoutMsg.percentage : Convert.DBNull;
            rowDetail["SPOTRATE"] = pPayoutMsg.payoutRateSpecified ? pPayoutMsg.payoutRate : Convert.DBNull;
            rowDetail["GAPRATE"] = pPayoutMsg.gapRateSpecified ? pPayoutMsg.gapRate : Convert.DBNull;
            rowDetail["DTFIXING"] = pPayoutMsg.actionDate;
            rowDetail["NOTE"] = pPayoutMsg.note;
            rowDetail["DTACTION"] = pPayoutMsg.actionDate;
			rowDetail.EndEdit();
            if (pPayoutMsg.currency != pPayoutMsg.baseCurrency)
            {
                AddEventDetailCustomerPayoutRebate(rowDetail, (PayoutRebateMsgBase)pPayoutMsg);
            }
            DtEventDet.Rows.Add(rowDetail);
		}
		#endregion AddEventDetailPayout
        #region AddEventDetailCustomerPayoutRebate
        protected static void AddEventDetailCustomerPayoutRebate(DataRow pRowDetail, PayoutRebateMsgBase pPayoutRebateMsg)
        {
            pRowDetail.BeginEdit();
            pRowDetail["IDC1"] = StrFunc.IsFilled(pPayoutRebateMsg.currency1) ? pPayoutRebateMsg.currency1 : Convert.DBNull;
            pRowDetail["IDC2"] = StrFunc.IsFilled(pPayoutRebateMsg.currency2) ? pPayoutRebateMsg.currency2 : Convert.DBNull;
            pRowDetail["BASIS"] = pPayoutRebateMsg.quoteBasis;
            pRowDetail["RATE"] = pPayoutRebateMsg.rate;
            pRowDetail["SPOTRATE"] = pPayoutRebateMsg.spotRateSpecified ? pPayoutRebateMsg.spotRate : Convert.DBNull;
            pRowDetail["FWDPOINTS"] = pPayoutRebateMsg.forwardPointsSpecified ? pPayoutRebateMsg.forwardPoints : Convert.DBNull;
            pRowDetail.EndEdit();
        }
        #endregion AddEventDetailCustomerPayoutRebate
        #region AddEventClassPreSettlementPayoutRebate
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected void AddEventClassPreSettlementPayoutRebate(int pIdE, DataRow pRow, DateTime pSettlementDate)
		{
            int idBookPayer		= Convert.IsDBNull(pRow["IDB_PAY"])?0:Convert.ToInt32(pRow["IDB_PAY"]);
            int idBookReceiver	= Convert.IsDBNull(pRow["IDB_REC"])?0:Convert.ToInt32(pRow["IDB_REC"]);
			string currency		= pRow["UNIT"].ToString();

            IOffset offset		= m_tradeLibrary.Product.ProductBase.CreateOffset(PeriodEnum.D, -2, DayTypeEnum.Business);
            // EG 20150706 [21021] Remove idPayer|idReceiver
            EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_CS, currency, idBookPayer, idBookReceiver, offset);

            if (preSettlementInfo.IsUsePreSettlement)
			{
				#region PreSettlementDate
                EFS_PreSettlement preSettlement = new EFS_PreSettlement(m_CS, null, pSettlementDate, currency, 
                    preSettlementInfo.OffsetPreSettlement, m_tradeLibrary.DataDocument);
                #endregion PreSettlementDate
                _ = AddEventClassPayoutRebate(pIdE, EventClassFunc.PreSettlement, preSettlement.AdjustedPreSettlementDate.DateValue);
            }
		}
		#endregion AddEventClassPreSettlementPayoutRebate

		#region Valorize
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Valorize()
		{
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            int newIdE;
            int newIdEParent;
            foreach (ActionMsgBase actionMsg in m_TradeAction.ActionMsgs)
			{
                PayoutRebateMsgBase payoutRebateMsg = actionMsg as PayoutRebateMsgBase;
                bool customerSettlementPayoutRebateSpecified = (payoutRebateMsg.baseCurrency != payoutRebateMsg.currency);

                if (IsPayoutMsg(actionMsg) && IsAllTriggerDeclared)
				{
                    PayoutMsg payoutMsg = actionMsg as PayoutMsg;
					DataRow rowPayoutSource = RowEvent(m_TradeAction.idE);
					ret = SQLUP.GetId(out newIdE,m_CS,SQLUP.IdGetId.EVENT,SQLUP.PosRetGetId.First,(customerSettlementPayoutRebateSpecified ? 3 : 2));
					if (Cst.ErrLevel.SUCCESS == ret)
					{
						if (EventCodeFunc.IsAbandon(m_TradeAction.code) ||
							EventCodeFunc.IsExercise(m_TradeAction.code) ||
							EventCodeFunc.IsOut(m_TradeAction.code))                  
						{
                            
                            //string code = string.Empty;
                            SysMsgCode code = new SysMsgCode(SysCodeEnum.LOG, 0);
                            if (EventCodeFunc.IsAbandon(m_TradeAction.code))
                            {
                                //code = "LOG-07211";
                                code = new SysMsgCode(SysCodeEnum.LOG, 7211);
                            }
                            else if (EventCodeFunc.IsOut(m_TradeAction.code))
                            {
                                //code = "LOG-07213";
                                code = new SysMsgCode(SysCodeEnum.LOG, 7213);
                            }
                            else if (EventCodeFunc.IsExercise(m_TradeAction.code))
                            {
                                //code = "LOG-07221";
                                code = new SysMsgCode(SysCodeEnum.LOG, 7221);
                            }
                            
                            // PM 20210121 [XXXXX] Passage du message au niveau de log None
                            Logger.Log(new LoggerData(LogLevelEnum.None, code, 0,
                                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id))));

							#region Abandon/Exercise/Out event
                            DataRow row = AddEventAbandonExerciseOut(newIdE, rowPayoutSource, (ActionMsgBase)payoutMsg, m_TradeAction.code, EventTypeFunc.Total);
                            DataRow rowClass = AddEventClassAbandonExerciseOut(newIdE, payoutMsg.settlementDate);

							newIdEParent = newIdE;
							newIdE++;

                            DataRow rowPayout = AddEventPayout(newIdE, newIdEParent, row, payoutMsg);
                            DataRow rowClassPayout = AddEventClassPayoutRebate(newIdE, EventClassFunc.Recognition, payoutMsg.actionDate.Date);
							if (EventCodeFunc.IsAbandon(m_TradeAction.code) || EventCodeFunc.IsOut(m_TradeAction.code))
                                AddEventDetailAbandonExerciseOut(newIdEParent, (ActionMsgBase)payoutMsg);
							else
							{
								#region Payout
								AddEventClassPreSettlementPayoutRebate(newIdE,rowPayout,payoutMsg.settlementDate);
                                rowClassPayout = AddEventClassPayoutRebate(newIdE, EventClassFunc.Settlement, payoutMsg.settlementDate);
                                AddEventDetailPayout(newIdE, payoutMsg);
								#endregion Payout
							}

                            if (customerSettlementPayoutRebateSpecified)
                            {
                                newIdEParent = newIdE;
                                newIdE++;
                                rowPayout = AddEventPayout(newIdE, newIdEParent, rowPayout, payoutMsg, customerSettlementPayoutRebateSpecified);
                                rowClassPayout = AddEventClassPayoutRebate(newIdE, EventClassFunc.GroupLevel, payoutMsg.settlementDate);
                            }

							#endregion Abandon Exercise event
						}
					}
				}
                else if (IsRebateMsg(actionMsg))
				{
					#region Rebate Process
                    RebateMsg rebateMsg = actionMsg as RebateMsg;
					DataRow rowRebateSource = RowEvent(m_TradeAction.idE);
                    bool isRebateSpecified = DtFunc.IsDateTimeFilled(rebateMsg.settlementDate);
                    // 20090922 RD
                    int numberOfToken = 3;
                    if (isRebateSpecified)
                    {
                        numberOfToken ++;
                        if (customerSettlementPayoutRebateSpecified)
                            numberOfToken ++;
                    }

                    ret = SQLUP.GetId(out newIdE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, numberOfToken);
					if (Cst.ErrLevel.SUCCESS == ret)
					{
                        DataRow row = AddEventAbandonExerciseOut(newIdE, rowRebateSource, (ActionMsgBase)rebateMsg, EventCodeFunc.Out, EventTypeFunc.Total);
                        DataRow rowClass = AddEventClassAbandonExerciseOut(newIdE, rebateMsg.actionDate);

						#region CallCurrency amount
						newIdEParent = newIdE;
						newIdE++;
                        DataRow rowCcyAmount = AddEventCurrencyAmount(newIdE, newIdEParent, rowRebateSource, rebateMsg, EventTypeFunc.CallCurrency);
                        DataRow rowEventClassCcyAmount = AddEventClassCurrencyAmount(newIdE, rowClass, actionMsg);
						#endregion CallCurrency amount
						#region PutCurrency amount
						newIdE++;
                        AddEventCurrencyAmount(newIdE, newIdEParent, rowCcyAmount, rebateMsg, EventTypeFunc.PutCurrency);
                        AddEventClassCurrencyAmount(newIdE, rowEventClassCcyAmount, actionMsg);
						#endregion PutCurrency amount

						if (isRebateSpecified)
						{
							newIdE++;
                            DataRow rowRebate = AddEventRebate(newIdE, newIdEParent, rowRebateSource, rebateMsg);
                            DateTime dtRecognition = rebateMsg.actionDate.Date;
                            if (PayoutEnum.Immediate == rebateMsg.payoutStyle)
                                dtRecognition = rebateMsg.settlementDate;
							DataRow rowClassRebate = AddEventClassPayoutRebate(newIdE, EventClassFunc.Recognition, dtRecognition);
                            AddEventClassPreSettlementPayoutRebate(newIdE, rowRebate, rebateMsg.settlementDate);
                            rowClassRebate = AddEventClassPayoutRebate(newIdE, EventClassFunc.Settlement, rebateMsg.settlementDate);
                            DataRow rowDetail = AddEventDetailAbandonExerciseOut(newIdE, (ActionMsgBase)rebateMsg);
                            AddEventDetailAbandonExerciseOut(newIdEParent, (ActionMsgBase)rebateMsg);

                            if (customerSettlementPayoutRebateSpecified)
                            {
                                newIdEParent = newIdE;
                                newIdE++;
                                rowRebate = AddEventRebate(newIdE, newIdEParent, rowRebate, rebateMsg, customerSettlementPayoutRebateSpecified);
                                rowClassRebate = AddEventClassPayoutRebate(newIdE, EventClassFunc.GroupLevel, rebateMsg.settlementDate);
                                AddEventDetailCustomerPayoutRebate(rowDetail, payoutRebateMsg);
                            }
						}
						else
                            AddEventDetailAbandonExerciseOut(newIdEParent, (ActionMsgBase)rebateMsg);
					}
					#endregion Rebate Process
				}
			}
			Update();
			return ret;
		}	
		#endregion Valorize
		#endregion Methods
	}
	#endregion TradeActionGenPayout
}

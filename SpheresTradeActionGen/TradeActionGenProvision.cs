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
using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection;
#endregion Using Directives

namespace EFS.Process.Provision
{
    #region TradeActionGenProvision
    public class TradeActionGenProcessProvision : TradeActionGenProcessBase
	{
		#region Members
		private bool                   m_IsTotalExercise;
		private ProvisionMsgBase       m_ProvisionMsg;
        // EG 20180514 Report 23812
        private FxOptionalEarlyTerminationProvisionMsg m_FxProvisionMsg;
		private readonly CommonValParameters m_Parameters;
		#endregion Members
		#region Accessors
		#region CommonValDate
		public override DateTime CommonValDate
		{
			get 
			{
				if (null != m_ProvisionMsg)
					return m_ProvisionMsg.provisionDate;
				return DateTime.MinValue;
			}
		}
		#endregion CommonValDate
		#region CommonValDateIncluded
		public override DateTime CommonValDateIncluded
		{
			get {return CommonValDate;}
		}
		#endregion CommonValDateIncluded
		#region Parameters
		public override CommonValParameters Parameters
		{
			get {return m_Parameters;}
        }
        // RD 20150325 [20821] Add
        public override CommonValParameters ParametersIRD
        {
            get { return m_Parameters; }
        }
		#endregion Parameters
		#endregion Accessors
		#region Constructors
        // EG 20180502 Analyse du code Correction [CA2214]
        public TradeActionGenProcessProvision(TradeActionGenProcess pTradeActionGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, TradeActionMQueue pTradeAction)
            : base(pTradeActionGenProcess, pDsTrade, pTradeLibrary, pTradeAction)
		{
			m_Parameters = new CommonValParametersIRD();
			//CodeReturn   = Valorize();
		}
		#endregion Constructors
		#region Methods
		#region AddEventCashSettlement
        // EG 20180514 [23812] Report 
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected void AddEventCashSettlement(int pIdE, int pIdEParent, DataRow pRowSource)
		{
            CashSettlementProvisionMsg cashSettlement = m_ProvisionMsg.cashSettlementProvision;
			DataRow rowCashSettlementAmount = DtEvent.NewRow();
			rowCashSettlementAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
            // EG 20180514 Report 23812
            // string eventCode = m_IsTotalExercise?EventCodeFunc.Termination:EventCodeFunc.Intermediary;
            string eventCode = EventTypeFunc.IsMultiple(m_ProvisionMsg.exerciseType) ? EventCodeFunc.Intermediary : EventCodeFunc.Termination;

            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7240), 1,
                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id)),
                new LogParam(LogTools.AmountAndCurrency(cashSettlement.amount, cashSettlement.currency))));

			rowCashSettlementAmount.BeginEdit();
			rowCashSettlementAmount["IDE"]             = pIdE;
			rowCashSettlementAmount["IDE_EVENT"]       = pIdEParent;
			rowCashSettlementAmount["EVENTCODE"]       = eventCode;
			rowCashSettlementAmount["EVENTTYPE"]       = EventTypeFunc.CashSettlement;
			rowCashSettlementAmount["VALORISATION"]    = cashSettlement.amount;
			rowCashSettlementAmount["UNIT"]            = cashSettlement.currency;
			rowCashSettlementAmount["UNITTYPE"]        = UnitTypeEnum.Currency;
			rowCashSettlementAmount["VALORISATIONSYS"] = cashSettlement.amount;
			rowCashSettlementAmount["UNITSYS"]         = cashSettlement.currency;
			rowCashSettlementAmount["UNITTYPESYS"]     = UnitTypeEnum.Currency;
			rowCashSettlementAmount["IDA_PAY"]         = cashSettlement.idA_Payer;
			rowCashSettlementAmount["IDB_PAY"]         = cashSettlement.idB_PayerSpecified?cashSettlement.idB_Payer:Convert.DBNull;
			rowCashSettlementAmount["IDA_REC"]         = cashSettlement.idA_Receiver;
			rowCashSettlementAmount["IDB_REC"]         = cashSettlement.idB_ReceiverSpecified?cashSettlement.idB_Receiver:Convert.DBNull;
			rowCashSettlementAmount.EndEdit();
			DtEvent.Rows.Add(rowCashSettlementAmount);
			SetRowStatus(rowCashSettlementAmount, TuningOutputTypeEnum.OES); 
		}
		#endregion AddEventCashSettlement
        #region AddEventCurrencyAmount
        // EG 20180514 Report 23812
        protected DataRow AddEventCurrencyAmount(int pIdE, int pIdEParent, DataRow pRowSource, string pCurrencyType)
        {
            DataRow rowCurrencyAmount = DtEvent.NewRow();
            rowCurrencyAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
            rowCurrencyAmount.BeginEdit();
            rowCurrencyAmount["IDE"] = pIdE;
            rowCurrencyAmount["IDE_EVENT"] = pIdEParent;
            rowCurrencyAmount["EVENTCODE"] = EventCodeFunc.Termination;
            rowCurrencyAmount["EVENTTYPE"] = pCurrencyType;

            AmountPayerReceiverInfo amount;
            if (pCurrencyType == EventTypeFunc.CallCurrency)
                amount = (AmountPayerReceiverInfo)m_FxProvisionMsg.callAmount;
            else
                amount = (AmountPayerReceiverInfo)m_FxProvisionMsg.putAmount;

            rowCurrencyAmount["VALORISATION"] = amount.Amount;
            rowCurrencyAmount["UNIT"] = amount.Currency;
            rowCurrencyAmount["UNITTYPE"] = UnitTypeEnum.Currency;
            rowCurrencyAmount["VALORISATIONSYS"] = amount.Amount;
            rowCurrencyAmount["UNITSYS"] = amount.Currency;
            rowCurrencyAmount["UNITTYPESYS"] = UnitTypeEnum.Currency;
            rowCurrencyAmount["IDA_PAY"] = amount.payer.actor.First <= 0 ? Convert.DBNull : amount.payer.actor.First;
            rowCurrencyAmount["IDB_PAY"] = amount.payer.book.First <= 0 ? Convert.DBNull : amount.payer.book.First;
            rowCurrencyAmount["IDA_REC"] = amount.receiver.actor.First <= 0 ? Convert.DBNull : amount.receiver.actor.First;
            rowCurrencyAmount["IDB_REC"] = amount.receiver.book.First <= 0 ? Convert.DBNull : amount.receiver.book.First;
            rowCurrencyAmount.EndEdit();
            DtEvent.Rows.Add(rowCurrencyAmount);
            SetRowStatus(rowCurrencyAmount, TuningOutputTypeEnum.OES);
            return rowCurrencyAmount;
        }
        #endregion AddEventCurrencyAmount
		#region AddEventClassCashSettlement
        // EG 20180514 [23812] Report 
        protected void AddEventClassCashSettlement(int pIdE, DataRow pRowSource)
        {

            #region Pre-Settlement

            // EG 20180514 [23812] Report 
            EFS_PreSettlement preSettlement = null;
            #region PreSettlementDate
            // FI 20180906 [XXXXX] Test sur null != m_FxProvisionMsg
            if (null != m_FxProvisionMsg)
            {
                IOffset offset = m_tradeLibrary.Product.ProductBase.CreateOffset(PeriodEnum.D, -2, DayTypeEnum.Business);
                EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_CS,
                                            m_ProvisionMsg.cashSettlementProvision.currency,
                                            m_ProvisionMsg.cashSettlementProvision.idB_Payer, m_ProvisionMsg.cashSettlementProvision.idB_Receiver, offset);
                
                preSettlement = new EFS_PreSettlement(m_CS, null, m_ProvisionMsg.cashSettlementProvision.cashSettlementPaymentDate,
                                    m_ProvisionMsg.cashSettlementProvision.currency, preSettlementInfo.OffsetPreSettlement, m_tradeLibrary.DataDocument);
            }
            #endregion PreSettlementDate
            #endregion Pre-Settlement

            DataRow rowCashSettlementAmount = DtEventClass.NewRow();
            #region Recognition (ExerciseDate)
            DateTime dtEvent = m_ProvisionMsg.actionDate.Date;

            rowCashSettlementAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
            rowCashSettlementAmount.BeginEdit();
            rowCashSettlementAmount["IDE"] = pIdE;
            rowCashSettlementAmount["EVENTCLASS"] = EventClassFunc.Recognition;
            rowCashSettlementAmount["DTEVENT"] = dtEvent;
            rowCashSettlementAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, dtEvent);
            // EG 20180514 [23812] Report 
            rowCashSettlementAmount["ISPAYMENT"] = false;
            rowCashSettlementAmount.EndEdit();
            DtEventClass.Rows.Add(rowCashSettlementAmount);
            #endregion Recognition (ExerciseDate)

            if (null != m_FxProvisionMsg)
            {
                #region PreSettlement
                dtEvent = preSettlement.AdjustedPreSettlementDate.DateValue;
                // EG 20180514 [23812] Report 
                rowCashSettlementAmount = DtEventClass.NewRow();
                rowCashSettlementAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
                rowCashSettlementAmount.BeginEdit();
                rowCashSettlementAmount["IDE"] = pIdE;
                rowCashSettlementAmount["EVENTCLASS"] = EventClassFunc.PreSettlement;
                rowCashSettlementAmount["DTEVENT"] = dtEvent;
                rowCashSettlementAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, dtEvent);
                rowCashSettlementAmount["ISPAYMENT"] = false;
                rowCashSettlementAmount.EndEdit();
                DtEventClass.Rows.Add(rowCashSettlementAmount);
                #endregion PreSettlement
            }


            #region ValueDate
            dtEvent = (null != m_FxProvisionMsg) ? m_FxProvisionMsg.provisionDate : m_ProvisionMsg.provisionDate;
            rowCashSettlementAmount = DtEventClass.NewRow();
            rowCashSettlementAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
            rowCashSettlementAmount.BeginEdit();
            rowCashSettlementAmount["IDE"] = pIdE;
            rowCashSettlementAmount["EVENTCLASS"] = EventClassFunc.ValueDate;
            rowCashSettlementAmount["DTEVENT"] = dtEvent;
            rowCashSettlementAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, dtEvent);
            rowCashSettlementAmount["ISPAYMENT"] = false;
            rowCashSettlementAmount.EndEdit();
            DtEventClass.Rows.Add(rowCashSettlementAmount);
            #endregion ValueDate


            #region Settlement (CashSettlementPaymentDate)
            dtEvent = m_ProvisionMsg.cashSettlementProvision.cashSettlementPaymentDate;
            rowCashSettlementAmount = DtEventClass.NewRow();
            rowCashSettlementAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
            rowCashSettlementAmount.BeginEdit();
            rowCashSettlementAmount["IDE"] = pIdE;
            rowCashSettlementAmount["EVENTCLASS"] = EventClassFunc.Settlement;
            rowCashSettlementAmount["DTEVENT"] = dtEvent;
            rowCashSettlementAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, dtEvent);
            // EG 20180514 Report 23812
            rowCashSettlementAmount["ISPAYMENT"] = true;
            rowCashSettlementAmount.EndEdit();
            DtEventClass.Rows.Add(rowCashSettlementAmount);
            #endregion Settlement (CashSettlementPaymentDate)
        }		
		#endregion AddEventClassCashSettlement
        #region AddEventClassAmount
        // EG 20180514 [23812] Report 
        protected DataRow AddEventClassAmount(int pIdE, DataRow pRowSource, bool pIsSettlement, EFS_PreSettlement pPreSettlement)
        {
            object[] clone = (object[])pRowSource.ItemArray.Clone();
            DataRow rowCurrencyAmount = DtEventClass.NewRow();
            rowCurrencyAmount.ItemArray = clone;
            rowCurrencyAmount.BeginEdit();
            rowCurrencyAmount["IDE"] = pIdE;
            rowCurrencyAmount["EVENTCLASS"] = EventClassFunc.Recognition;
            rowCurrencyAmount["DTEVENT"] = m_FxProvisionMsg.actionDate.Date;
            rowCurrencyAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, m_FxProvisionMsg.actionDate.Date);
            rowCurrencyAmount["ISPAYMENT"] = false;
            rowCurrencyAmount.EndEdit();
            DtEventClass.Rows.Add(rowCurrencyAmount);
            if (pIsSettlement)
            {
                if (null != pPreSettlement)
                {
                    rowCurrencyAmount = DtEventClass.NewRow();
                    rowCurrencyAmount.ItemArray = clone;
                    rowCurrencyAmount.BeginEdit();
                    rowCurrencyAmount["IDE"] = pIdE;
                    rowCurrencyAmount["EVENTCLASS"] = EventClassFunc.PreSettlement;
                    rowCurrencyAmount["DTEVENT"] = pPreSettlement.AdjustedPreSettlementDate.DateValue;
                    rowCurrencyAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, pPreSettlement.AdjustedPreSettlementDate.DateValue);
                    rowCurrencyAmount["ISPAYMENT"] = false;
                    rowCurrencyAmount.EndEdit();
                    DtEventClass.Rows.Add(rowCurrencyAmount);
                }

                rowCurrencyAmount = DtEventClass.NewRow();
                rowCurrencyAmount.ItemArray = clone;
                rowCurrencyAmount.BeginEdit();
                rowCurrencyAmount["IDE"] = pIdE;
                rowCurrencyAmount["EVENTCLASS"] = EventClassFunc.Settlement;
                rowCurrencyAmount["DTEVENT"] = m_FxProvisionMsg.provisionDate;
                rowCurrencyAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, m_FxProvisionMsg.provisionDate);
                rowCurrencyAmount["ISPAYMENT"] = true;
                rowCurrencyAmount.EndEdit();
                DtEventClass.Rows.Add(rowCurrencyAmount);
            }
            return rowCurrencyAmount;
        }
        #endregion AddEventClassCurrencyAmount
        #region AddEventClassExercise
		protected DataRow AddEventClassExercise(int pIdE)
		{
			DataRow rowExercise = DtEventClass.NewRow();
			rowExercise.BeginEdit();
			rowExercise["IDE"] = pIdE;
			rowExercise["EVENTCLASS"] = EventClassFunc.GroupLevel;
            rowExercise["DTEVENT"] = m_ProvisionMsg.provisionDate;
            rowExercise["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, m_ProvisionMsg.provisionDate);
            rowExercise["ISPAYMENT"] = false; 
            rowExercise.EndEdit();
			DtEventClass.Rows.Add(rowExercise);
			return rowExercise;
		}
		#endregion AddEventClassExercise
        #region AddEventClassInfoExercieFxProvision
        /// <summary>
        /// Ajout d'un EVENTCLASS matérialisant l'exercice total d'une provision (EXO) 
        /// sur les événement TRD|PRD + STREAM 
        /// avec mise à jour de la date de fin de période = Date exercice de la provision
        /// </summary>
        // EG 20180614 New
        protected void AddEventClassInfoExercieFxProvision()
        {
            DataRow rowStream;
            if (m_tradeLibrary.Product.ProductBase.IsFxDigitalOption)
            {
                // ADO|EDO
                rowStream = DsEvents.DtEvent.Select(StrFunc.AppendFormat("EVENTCODE in ('{0}','{1}')", EventCodeFunc.AmericanDigitalOption, EventCodeFunc.EuropeanDigitalOption)).First();
            }
            else
            {
                // ABO|BBO|EBO, AAO|BAO|EAO, ASO|BSO|ESO via STA|CCU
                rowStream = DsEvents.DtEvent.Select(StrFunc.AppendFormat("EVENTCODE = '{0}' and  EVENTTYPE = '{1}'", EventCodeFunc.Start, EventTypeFunc.CallCurrency)).First().GetParentRow(DsEvents.ChildEvent);
            }
            if (null != rowStream)
            {
                UpdEventDtEndPeriod(rowStream);
                DataRow rowParent = rowStream.GetParentRow(DsEvents.ChildEvent);
                if (null != rowParent)
                {
                    // PRD|DAT
                    UpdEventDtEndPeriod(rowParent);
                    rowParent = rowParent.GetParentRow(DsEvents.ChildEvent);
                    if ((null != rowParent) && EventCodeFunc.IsTrade(rowParent["EVENTCODE"].ToString()))
                    {
                        // TRD|DAT
                        UpdEventDtEndPeriod(rowParent);
                    }
                }
            }
        }
        #endregion AddEventClassInfoExercieFxProvision
		#region AddEventClassInfoExerciseProvision
		protected void AddEventClassInfoExerciseProvision(int pIdE,DataRow pRowSource)
		{
			AddEventClassInfoExerciseProvision(pIdE,pRowSource,GetEventClassProvision);
		}
		protected void AddEventClassInfoExerciseProvision(int pIdE,DataRow pRowSource,string pEventClass)
		{
			DataRow row = DtEventClass.NewRow();
			row.ItemArray = (object[])pRowSource.ItemArray.Clone();
			row.BeginEdit();
			row["IDE"] = pIdE;
			row["EVENTCLASS"] = pEventClass;
            row["DTEVENT"] = m_ProvisionMsg.actionDate.Date;
            row["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, m_ProvisionMsg.actionDate.Date);
			row.EndEdit();
			DtEventClass.Rows.Add(row);
		}		
		#endregion AddEventClassInfoExerciseProvision
		#region AddEventClassFee
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected void AddEventClassFee(int pIdE, DataRow pRowSource)
		{
			DataRow rowFeeAmount = DtEventClass.NewRow();
			#region Recognition (ExerciseDate)
			rowFeeAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
			rowFeeAmount.BeginEdit();
			rowFeeAmount["IDE"] = pIdE;
			rowFeeAmount["EVENTCLASS"] = EventClassFunc.Recognition;
            rowFeeAmount["DTEVENT"] = m_ProvisionMsg.actionDate.Date;
            rowFeeAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, m_ProvisionMsg.actionDate.Date);
			rowFeeAmount.EndEdit();
			DtEventClass.Rows.Add(rowFeeAmount);
			#endregion Recognition (ExerciseDate)
			// 20070823 EG Ticket : 15643
			#region PreSettlementInfo
            IOffset offset = m_tradeLibrary.Product.ProductBase.CreateOffset(PeriodEnum.D, -2, DayTypeEnum.Business);
            // EG 20150706 [21021] Remove m_ProvisionMsg.feeProvision.idA_Payer|m_ProvisionMsg.feeProvision.idA_Receiver
            EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_CS, m_ProvisionMsg.feeProvision.currency, 
                m_ProvisionMsg.feeProvision.idB_Payer,m_ProvisionMsg.feeProvision.idB_Receiver, offset);
			#endregion PreSettlementInfo

            if (preSettlementInfo.IsUsePreSettlement)
			{
				#region PreSettlementDate
                EFS_PreSettlement preSettlement = new EFS_PreSettlement(m_CS, null, m_ProvisionMsg.feeProvision.feePaymentDate,
                    m_ProvisionMsg.feeProvision.currency, preSettlementInfo.OffsetPreSettlement, m_tradeLibrary.DataDocument);
				#endregion PreSettlementDate
				#region Settlement (FeePaymentDate)
				rowFeeAmount = DtEventClass.NewRow();
				rowFeeAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
				rowFeeAmount.BeginEdit();
				rowFeeAmount["IDE"] = pIdE;
				rowFeeAmount["EVENTCLASS"] = EventClassFunc.PreSettlement;
				rowFeeAmount["DTEVENT"] = preSettlement.AdjustedPreSettlementDate.DateValue;
				rowFeeAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS,preSettlement.AdjustedPreSettlementDate.DateValue);
				rowFeeAmount.EndEdit();
				DtEventClass.Rows.Add(rowFeeAmount);
				#endregion Settlement (FeePaymentDate)

			}

            #region Settlement (FeePaymentDate)
			rowFeeAmount = DtEventClass.NewRow();
			rowFeeAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
			rowFeeAmount.BeginEdit();
			rowFeeAmount["IDE"] = pIdE;
			rowFeeAmount["EVENTCLASS"] = EventClassFunc.Settlement;
            rowFeeAmount["DTEVENT"] = m_ProvisionMsg.feeProvision.feePaymentDate;
            rowFeeAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, m_ProvisionMsg.feeProvision.feePaymentDate);
			rowFeeAmount.EndEdit();
			DtEventClass.Rows.Add(rowFeeAmount);
			#endregion Settlement (FeePaymentDate)
		}		
		#endregion AddEventClassFee
		#region AddEventDetailExercise
		protected void AddEventDetailExercise(int pIdE)
		{
            DataRow rowDetail = DtEventDet.NewRow();
			rowDetail.BeginEdit();
			rowDetail["IDE"] = pIdE;
            rowDetail["NOTE"] = m_ProvisionMsg.note;
            rowDetail["DTACTION"] = m_ProvisionMsg.actionDate;
			rowDetail.EndEdit();
            DtEventDet.Rows.Add(rowDetail);
		}
		#endregion AddEventDetailExercise
		#region AddEventDetailFee
		protected void AddEventDetailFee(int pIdE)
		{
            FeeProvisionMsg fee = m_ProvisionMsg.feeProvision;
            DataRow rowFeeAmount = DtEventDet.NewRow();

			rowFeeAmount.BeginEdit();
			rowFeeAmount["IDE"] = pIdE;
			if (fee.feeRateSpecified)
				rowFeeAmount["RATE"] = fee.feeRate;
			if (fee.notionalReferenceSpecified)
				rowFeeAmount["NOTIONALREFERENCE"] = fee.notionalReference;
			rowFeeAmount["IDC_REF"] = fee.currencyReference;

			rowFeeAmount.EndEdit();
            DtEventDet.Rows.Add(rowFeeAmount);
		}
		#endregion AddEventDetailFee
		#region AddEventExercise
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected DataRow AddEventExercise(int pIdE, DataRow pRowSource)
        {
            // Event Exercise Provision (EXC / EXT / EXO / EXM)
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7239), 1,
                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id)),
                new LogParam(m_ProvisionMsg.eventCode + " / " + m_ProvisionMsg.exerciseType)));

            DataRow rowExercise = DtEvent.NewRow();
            rowExercise.ItemArray = (object[])pRowSource.ItemArray.Clone();
            rowExercise.BeginEdit();
            rowExercise["IDE"] = pIdE;
            rowExercise["IDE_EVENT"] = m_TradeAction.idE;
            rowExercise["EVENTCODE"] = m_ProvisionMsg.eventCode;
            rowExercise["EVENTTYPE"] = m_ProvisionMsg.exerciseType;
            rowExercise["DTSTARTADJ"] = m_ProvisionMsg.actionDate.Date;
            rowExercise["DTSTARTUNADJ"] = m_ProvisionMsg.actionDate.Date;
            rowExercise["DTENDADJ"] = m_ProvisionMsg.actionDate.Date;
            rowExercise["DTENDUNADJ"] = m_ProvisionMsg.actionDate.Date;
            rowExercise["IDSTTRIGGER"] = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
            rowExercise["IDSTCALCUL"] = StatusCalculFunc.Calculated;
            rowExercise["SOURCE"] = m_TradeActionGenProcess.AppInstance.ServiceName;
            rowExercise.EndEdit();
            DtEvent.Rows.Add(rowExercise);
            SetRowStatus(rowExercise, TuningOutputTypeEnum.OES);
            return rowExercise;
        }
		#endregion AddEventExercise
		#region AddEventFee
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected void AddEventFee(int pIdE, int pIdEParent, DataRow pRowSource)
		{
			#region Event Fee Amount provision
            FeeProvisionMsg fee = m_ProvisionMsg.feeProvision;
			DataRow rowFeeAmount   = DtEvent.NewRow();
			rowFeeAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
			string eventCode       = m_IsTotalExercise?EventCodeFunc.Termination:EventCodeFunc.Intermediary;

            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7241), 1,
                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id)),
                new LogParam(LogTools.AmountAndCurrency(fee.amount, fee.currency))));

			rowFeeAmount.BeginEdit();
			rowFeeAmount["IDE"]              = pIdE;
			rowFeeAmount["IDE_EVENT"]  = pIdEParent;
			rowFeeAmount["EVENTCODE"]       = eventCode;
			rowFeeAmount["EVENTTYPE"]       = EventTypeFunc.Fee;
			rowFeeAmount["VALORISATION"]    = fee.amount;
			rowFeeAmount["UNIT"]            = fee.currency;
			rowFeeAmount["UNITTYPE"]        = UnitTypeEnum.Currency;
			rowFeeAmount["VALORISATIONSYS"] = fee.amount;
			rowFeeAmount["UNITSYS"]         = fee.currency;
			rowFeeAmount["UNITTYPESYS"]     = UnitTypeEnum.Currency;
			rowFeeAmount["IDA_PAY"]         = fee.idA_Payer;
			rowFeeAmount["IDB_PAY"]         = fee.idB_PayerSpecified?fee.idB_Payer:Convert.DBNull;
			rowFeeAmount["IDA_REC"]         = fee.idA_Receiver;
			rowFeeAmount["IDB_REC"]         = fee.idB_ReceiverSpecified?fee.idB_Receiver:Convert.DBNull;
			rowFeeAmount.EndEdit();
			DtEvent.Rows.Add(rowFeeAmount);
			SetRowStatus(rowFeeAmount, TuningOutputTypeEnum.OES); 
			#endregion Event Fee Amount provision
		}
		#endregion AddEventFeeAmount
		#region AddEventIntermediaryVariationNominal
		/// <summary>
		/// Add an intermediary variation nominal event in case of :
		/// MULTIPLE OR PARTIAL exercise provision (CANCELABLE & OPTIONALEARLYTERMINATION provision)
		/// </summary>
		/// <param name="pNotionalProvisionEvent"></param>
        private void AddEventIntermediaryVariationNominal(NotionalProvisionMsg pNotionalProvisionMsg)
		{
			#region Event Intermediary Variation nominal
			DataRow rowTermination      = RowTerminationVariationNominal;
			string eventCodeTermination = EventCodeFunc.Termination;
			if (EventCodeFunc.IsTerminationIntermediary(rowTermination["EVENTCODE"].ToString()))
				eventCodeTermination = EventCodeFunc.TerminationIntermediary;

			DataRow rowIntermediary   = DtEvent.NewRow();
			rowIntermediary.ItemArray = (object[])rowTermination.ItemArray.Clone();
            if (Cst.ErrLevel.SUCCESS == SQLUP.GetId(out int idE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First))
            {
                #region Event Intermediary
                rowIntermediary.BeginEdit();
                rowIntermediary["IDE"] = idE;
                rowIntermediary["EVENTCODE"] = (m_IsTotalExercise ? eventCodeTermination : EventCodeFunc.Intermediary);
                rowIntermediary["EVENTTYPE"] = EventTypeFunc.Nominal;
                rowIntermediary["DTSTARTUNADJ"] = m_ProvisionMsg.provisionDate;
                rowIntermediary["DTSTARTADJ"] = m_ProvisionMsg.provisionDate;
                rowIntermediary["DTENDUNADJ"] = m_ProvisionMsg.provisionDate;
                rowIntermediary["DTENDADJ"] = m_ProvisionMsg.provisionDate;
                rowIntermediary["VALORISATION"] = pNotionalProvisionMsg.provisionAmount;
                rowIntermediary["UNIT"] = pNotionalProvisionMsg.provisionCurrency;
                rowIntermediary["UNITTYPE"] = UnitTypeEnum.Currency;
                rowIntermediary["VALORISATIONSYS"] = pNotionalProvisionMsg.provisionAmount;
                rowIntermediary["UNITSYS"] = pNotionalProvisionMsg.provisionCurrency;
                rowIntermediary["UNITTYPESYS"] = UnitTypeEnum.Currency;
                rowIntermediary.EndEdit();
                DtEvent.Rows.Add(rowIntermediary);
                SetRowStatus(rowIntermediary, TuningOutputTypeEnum.OES);
                #endregion Event Intermediary
                #region EventClass Intermediary
                DataRow[] rowTerminationClass = rowTermination.GetChildRows(DsEvents.ChildEventClass);
                foreach (DataRow row in rowTerminationClass)
                {
                    DataRow rowIntermediaryClass = DtEventClass.NewRow();
                    rowIntermediaryClass.BeginEdit();
                    rowIntermediaryClass.ItemArray = (object[])row.ItemArray.Clone();
                    rowIntermediaryClass["IDE"] = idE;
                    rowIntermediaryClass["DTEVENT"] = m_ProvisionMsg.provisionDate;
                    rowIntermediaryClass["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, m_ProvisionMsg.provisionDate);
                    rowIntermediaryClass.EndEdit();
                    DtEventClass.Rows.Add(rowIntermediaryClass);
                }
                AddEventClassInfoExerciseProvision(idE, rowTerminationClass[0]);
                #endregion EventClass Intermediary
            }
            #endregion Event Intermediary Variation nominal
        }
		#endregion AddEventIntermediaryVariationNominal
		#region AddEventIntermediaryVariationNominalNextVersionStandBy
		/// <summary>
		/// Add a new intermediary variation nominal event in case of :
		/// MULTIPLE OR PARTIAL exercise provision (CANCELABLE & OPTIONALEARLYTERMINATION & STEPUP provision)
		/// </summary>
		/// <param name="pNotionalProvisionEvent"></param>
        private void AddEventIntermediaryVariationNominalNextVersionStandBy(DataRow pRowSourceVariation, NotionalProvisionMsg pNotionalProvisionMsg)
		{
			#region Event Intermediary Variation nominal
			string eventCodeSource = EventCodeFunc.Intermediary;
			DataRow rowIntermediary = DtEvent.NewRow();
			rowIntermediary.ItemArray = (object[])pRowSourceVariation.ItemArray.Clone();

            if (IsStepDownProvisionMsg(m_ProvisionMsg) && m_IsTotalExercise)
            {
                eventCodeSource = EventCodeFunc.Termination;
                if (EventCodeFunc.IsTerminationIntermediary(pRowSourceVariation["EVENTCODE"].ToString()))
                    eventCodeSource = EventCodeFunc.TerminationIntermediary;
            }
            if (Cst.ErrLevel.SUCCESS == SQLUP.GetId(out int idE,m_CS,SQLUP.IdGetId.EVENT,SQLUP.PosRetGetId.First))
			{
				#region Event Intermediary
				rowIntermediary.BeginEdit();
				rowIntermediary["IDE"] = idE;
				rowIntermediary["EVENTCODE"] = eventCodeSource;
				rowIntermediary["EVENTTYPE"] = EventTypeFunc.Nominal;
                rowIntermediary["DTSTARTUNADJ"] = m_ProvisionMsg.provisionDate;
                rowIntermediary["DTSTARTADJ"] = m_ProvisionMsg.provisionDate;
                rowIntermediary["DTENDUNADJ"] = m_ProvisionMsg.provisionDate;
                rowIntermediary["DTENDADJ"] = m_ProvisionMsg.provisionDate;
                rowIntermediary["VALORISATION"] = pNotionalProvisionMsg.provisionAmount;
                rowIntermediary["UNIT"] = pNotionalProvisionMsg.provisionCurrency;
				rowIntermediary["UNITTYPE"] = UnitTypeEnum.Currency;
                rowIntermediary["VALORISATIONSYS"] = pNotionalProvisionMsg.provisionAmount;
                rowIntermediary["UNITSYS"] = pNotionalProvisionMsg.provisionCurrency;
				rowIntermediary["UNITTYPESYS"] = UnitTypeEnum.Currency;
				rowIntermediary.EndEdit();
				DtEvent.Rows.Add(rowIntermediary);
				SetRowStatus(rowIntermediary, TuningOutputTypeEnum.OES); 
				#endregion Event Intermediary
				#region EventClass Intermediary
				DataRow[] rowSourceVariationClass = pRowSourceVariation.GetChildRows(DsEvents.ChildEventClass);
				foreach (DataRow row in rowSourceVariationClass)
				{
					DataRow rowIntermediaryClass = DtEventClass.NewRow();
					rowIntermediaryClass.BeginEdit();
					rowIntermediaryClass.ItemArray = (object[])row.ItemArray.Clone();
					rowIntermediaryClass["IDE"] = idE;
                    rowIntermediaryClass["DTEVENT"] = m_ProvisionMsg.provisionDate;
                    rowIntermediaryClass["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, m_ProvisionMsg.provisionDate);
                    rowIntermediaryClass.EndEdit();
					DtEventClass.Rows.Add(rowIntermediaryClass);
				}
				AddEventClassInfoExerciseProvision(idE,rowSourceVariationClass[0]);
				#endregion EventClass Intermediary
			}
				#endregion Event Intermediary Variation nominal
		}
		#endregion AddEventIntermediaryVariationNominal
		#region AddEventNominalStep
		private DataRow AddEventNominalStep(DataRow pRowCurrentNominalStep)
		{

            #region Event Nominal Step
            DataRow rowNominalStep = null;
            if (Cst.ErrLevel.SUCCESS == SQLUP.GetId(out int idE,m_CS,SQLUP.IdGetId.EVENT,SQLUP.PosRetGetId.First))
			{
				#region Add Event NominalStep
				rowNominalStep                    = DtEvent.NewRow();
				rowNominalStep.ItemArray          = (object[])pRowCurrentNominalStep.ItemArray.Clone();
				rowNominalStep.BeginEdit();
				rowNominalStep["IDE"]              = idE;
				rowNominalStep["EVENTCODE"]       = EventCodeFunc.NominalStep;
				rowNominalStep["EVENTTYPE"]       = EventTypeFunc.Nominal;
				rowNominalStep.EndEdit();
				DtEvent.Rows.Add(rowNominalStep);
				SetRowStatus(rowNominalStep, TuningOutputTypeEnum.OES); 
				#endregion Add Event NominalStep

				#region Add EventClass NominalStep
				DataRow[] rowCurrentNominalStepClass = pRowCurrentNominalStep.GetChildRows(DsEvents.ChildEventClass);
				foreach (DataRow row in rowCurrentNominalStepClass)
				{
					DataRow rowNominalStepClass          = DtEventClass.NewRow();
					rowNominalStepClass.ItemArray        = (object[])row.ItemArray.Clone();
					rowNominalStepClass.BeginEdit();
					rowNominalStepClass["IDE"]      = idE;
					rowNominalStepClass.EndEdit();
					DtEventClass.Rows.Add(rowNominalStepClass);
				}
				#endregion Add EventClass NominalStep
			}
			return rowNominalStep;
			#endregion Event Nominal Step
		}
		#endregion AddEventNominalStep
		#region CalculEventVariationNominal
        private void CalculEventVariationNominal(NotionalProvisionMsg pNotionalProvisionMsg)
		{
            DataRow row = GetRowVariationNominal(m_ProvisionMsg.provisionDate);
			if (null == row)
			{
                AddEventIntermediaryVariationNominal(pNotionalProvisionMsg);
			}
			else
			{
				row.BeginEdit();
				decimal previousAmount = Convert.ToDecimal(row["VALORISATION"]);
				int idE                = Convert.ToInt32(row["IDE"]);
                row["VALORISATION"] = previousAmount + pNotionalProvisionMsg.provisionAmount;
                row["VALORISATIONSYS"] = previousAmount + pNotionalProvisionMsg.provisionAmount;
				if(m_IsTotalExercise)
					row["EVENTCODE"]   = EventCodeFunc.Termination;
				row.EndEdit();
				DataRow[] rowEventClass = row.GetChildRows(DsEvents.ChildEventClass);
				if (null != rowEventClass)
				{
					bool isCanBeAdded = true;
					string eventClass = GetEventClassProvision;
					foreach (DataRow rowClass in rowEventClass)
					{
						DateTime dtEvent = Convert.ToDateTime(rowClass["DTEVENT"]);
                        if ((dtEvent.Date == m_ProvisionMsg.actionDate.Date) && (eventClass == rowClass["EVENTCLASS"].ToString()))
							isCanBeAdded = false;

					}
					if (isCanBeAdded)
						AddEventClassInfoExerciseProvision(idE,rowEventClass[0],eventClass);
				}
			}

            DataRow[] rowVariation = GetRowVariationNominal();
			if (null != rowVariation)
			{
				decimal currentNominalStep = Convert.ToDecimal(rowVariation[0]["VALORISATION"]);
                for (int i=1;i<rowVariation.Length;i++)
				{
                    DateTime dtStart = Convert.ToDateTime(rowVariation[i]["DTSTARTADJ"]);
                    if (0 < dtStart.CompareTo(m_ProvisionMsg.provisionDate))
					{
						if (m_IsTotalExercise || (0 == currentNominalStep))
						{
							DataRow[] rowsSi = rowVariation[i].GetChildRows(DsEvents.ChildEventSi);
							if (null != rowsSi)
							{
								foreach (DataRow rowSi in rowsSi)
									rowSi.Delete();
							}
							rowVariation[i].Delete();
							continue;
						}
						else if (currentNominalStep <= Convert.ToDecimal(rowVariation[i]["VALORISATION"]))
						{
							rowVariation[i].BeginEdit();
							rowVariation[i]["VALORISATION"]    = currentNominalStep;
							rowVariation[i]["VALORISATIONSYS"] = currentNominalStep;
							rowVariation[i]["EVENTCODE"]       = EventCodeFunc.Termination;
							rowVariation[i].EndEdit();
							currentNominalStep = 0;
						}
					}
					currentNominalStep -= Convert.ToDecimal(rowVariation[i]["VALORISATION"]);
				}
			}
		}
		#endregion CalculEventVariationNominal
		#region CalculEventNominalStep
        private void CalculEventNominalStep(NotionalProvisionMsg pNotionalProvisionMsg)
		{
			DataRow[] rowVariation = GetRowVariationNominal();
			if (null != rowVariation)
			{
                decimal currentNominalStep = Convert.ToDecimal(rowVariation[0]["VALORISATION"]);
				DateTime dtStart           = Convert.ToDateTime(rowVariation[0]["DTSTARTADJ"]);
                for (int i=1;i<rowVariation.Length;i++)
				{
                    DateTime dtEnd = Convert.ToDateTime(rowVariation[i]["DTSTARTADJ"]);
                    DataRow rowNominalStep = GetRowNominalStep(dtStart, dtEnd);
                    if (null == rowNominalStep)
					{
						rowNominalStep = GetRowNominalStep(dtEnd);
						if (null == rowNominalStep)
						{
							#region Add NominalStep
                            DataRow rowCurrentNominalStep = RowCurrentNotionalStep(pNotionalProvisionMsg.instrumentNo, pNotionalProvisionMsg.streamNo, m_ProvisionMsg.provisionDate);
							rowNominalStep                = AddEventNominalStep(rowCurrentNominalStep);
							#endregion Add NominalStep
						}
					}
					#region Update Event NominalStep
					UpdEventNominalStep(rowNominalStep,dtStart,dtEnd,currentNominalStep);
					#endregion Update EventClass NominalStep

					currentNominalStep -= Convert.ToDecimal(rowVariation[i]["VALORISATION"]);
					dtStart             = dtEnd;
				}
				if (m_IsTotalExercise)
					DelEventNominalStep();
			}
		}
		#endregion CalculEventNominalStep
		#region DelEventNominalStep
        private void DelEventNominalStep()
        {
            #region Delete Event NominalStep
            DataRow[] rowNominalStep = GetRowNominalStep();
            if (ArrFunc.IsFilled(rowNominalStep))
            {
                foreach (DataRow row in rowNominalStep)
                {
                    DateTime dtEnd = Convert.ToDateTime(row["DTENDADJ"]);
                    if (0 < dtEnd.CompareTo(m_ProvisionMsg.provisionDate))
                    {
                        DataRow[] rowsSi = row.GetChildRows(DsEvents.ChildEventSi);
                        if (null != rowsSi)
                        {
                            foreach (DataRow rowSi in rowsSi)
                                rowSi.Delete();
                        }
                        row.Delete();
                    }
                }
            }
            #endregion Delete EventClass NominalStep
        }
		#endregion DelEventNominalStep 
		#region ExcludeRowChilds
		private Cst.ErrLevel ExcludeRowChilds(DataRow pRowParent)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
			if (m_IsTotalExercise)
			{
				DataRow[] rowsChild = pRowParent.GetChildRows(DsEvents.ChildEvent);
				if (null != rowsChild)
				{
					foreach (DataRow rowChild in rowsChild)
					{
						DateTime startDate = Convert.ToDateTime(rowChild["DTSTARTADJ"]);
						DateTime endDate = Convert.ToDateTime(rowChild["DTENDADJ"]);
						string eventCode = rowChild["EVENTCODE"].ToString();
						if (0 <= startDate.CompareTo(CommonValDate))
							// period > Provision Date
							rowChild.Delete();
						else if (IsRowMustBeCalculate(rowChild))
						{
							ret = ExcludeRowChilds(rowChild);
							if (Cst.ErrLevel.SUCCESS != ret)
								break;
						}
					}
				}
			}
			return ret;
		}
        #endregion UpdateAndExcludeRowChilds
		#region GetEventClassProvision
		/// <summary>
		/// Return the code of EventClass for a notional event inserted after an exercise provision
		/// </summary>
		// EG 20180614 Upd (FxOptionalEarlyTerminationProvisionMsg)
        private string GetEventClassProvision
		{
			get
			{
				string eventClass = String.Empty;
                Type tProvision = m_ProvisionMsg.GetType();
				if (tProvision.Equals(typeof(ExtendibleProvisionMsg)))
					eventClass = EventClassFunc.ExerciseExtendible;
				else if (tProvision.Equals(typeof(CancelableProvisionMsg)))
					eventClass = EventClassFunc.ExerciseCancelable;
				else if (tProvision.Equals(typeof(MandatoryEarlyTerminationProvisionMsg)))
					eventClass = EventClassFunc.ExerciseMandatoryEarlyTermination;
				else if (tProvision.Equals(typeof(OptionalEarlyTerminationProvisionMsg)))
					eventClass = EventClassFunc.ExerciseOptionalEarlyTermination;
				else if (tProvision.Equals(typeof(StepUpProvisionMsg)))
					eventClass = EventClassFunc.ExerciseStepUp;
                else if (tProvision.Equals(typeof(FxOptionalEarlyTerminationProvisionMsg)))
                    eventClass = EventClassFunc.ExerciseOptionalEarlyTermination;
				return eventClass;
			}
		}
		#endregion GetEventClassProvision
		#region IsRowMustBeCalculate
		public override bool IsRowMustBeCalculate(DataRow pRow)
		{
			bool ret = true;
            DateTime endDate   = Convert.ToDateTime(pRow["DTENDUNADJ"]);
			string eventCode   = pRow["EVENTCODE"].ToString();
			string eventType   = pRow["EVENTTYPE"].ToString();

            if (DtFunc.IsDateTimeFilled(CommonValDate))
			{
				if (EventTypeFunc.IsInterest(eventType) || EventCodeFunc.IsCalculationPeriod(eventCode) ||
					EventCodeFunc.IsReset(eventCode) || EventCodeFunc.IsSelfAverage(eventCode) || EventCodeFunc.IsSelfReset(eventCode))
				{
					if (CommonValDate >= endDate)
						ret = false;
				}
				else if (EventCodeFunc.IsDailyClosing(eventCode))
					ret = false;
			}
			else
				ret = false;
			return ret;
		}
		#endregion IsRowMustBeCalculate
		#region IsRowsEventCalculated
        public override bool IsRowsEventCalculated(DataRow[] pRows)
        {
            if (ArrFunc.IsFilled(pRows))
            {
                foreach (DataRow row in pRows)
                {
                    DateTime startDate = Convert.ToDateTime(row["DTSTARTUNADJ"]);
                    DateTime endDate = Convert.ToDateTime(row["DTENDUNADJ"]);

                    if (StatusCalculFunc.IsToCalculate(row["IDSTCALCUL"].ToString()))
                    {
                        if (DtFunc.IsDateTimeFilled(CommonValDate))
                        {
                            if ((CommonValDate > startDate) && (CommonValDate <= endDate))
                            {
                                DataRow[] rowChilds = row.GetChildRows(DsEvents.ChildEvent);
                                if (0 != rowChilds.Length)
                                    return IsRowsEventCalculated(rowChilds);
                                return false;
                            }
                        }
                        else
                            return false;
                    }
                }
            }
            return true;
        }
		#endregion IsRowsEventCalculated
		#region IsStepDownRowVariation
		public static bool IsSamePayerReceiverRowVariation(DataRow pRow1,DataRow pRow2)
		{
			int idA_PAY1 = Convert.ToInt32(pRow1["IDA_PAY"]);
			int idB_PAY1 = Convert.IsDBNull(pRow1["IDB_PAY"])?0:Convert.ToInt32(pRow1["IDB_PAY"]);
			int idA_REC1 = Convert.ToInt32(pRow1["IDA_REC"]);
			int idB_REC1 = Convert.IsDBNull(pRow1["IDB_REC"])?0:Convert.ToInt32(pRow1["IDB_REC"]);

			int idA_PAY2 = Convert.ToInt32(pRow2["IDA_PAY"]);
			int idB_PAY2 = Convert.IsDBNull(pRow2["IDB_PAY"])?0:Convert.ToInt32(pRow2["IDB_PAY"]);
			int idA_REC2 = Convert.ToInt32(pRow2["IDA_REC"]);
			int idB_REC2 = Convert.IsDBNull(pRow2["IDB_REC"])?0:Convert.ToInt32(pRow2["IDB_REC"]);

			return (idA_PAY1 == idA_PAY2) && (idB_PAY1 == idB_PAY2) && (idA_REC1 == idA_REC2) && (idB_REC1 == idB_REC2);
		}
		#endregion IsRowsEventCalculated
		#region RecalculationCashFlows
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel RecalculationCashFlows()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            ArrayList alSpheresException = new ArrayList();
            #region Payment Process
            DataRow[] rowsInterest = GetRowInterest();
            if (ArrFunc.IsFilled(rowsInterest))
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7243), 1,
                    new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id))));

                foreach (DataRow rowInterest in rowsInterest)
                {
                    DateTime startDate = Convert.ToDateTime(rowInterest["DTSTARTADJ"]);
                    bool isError = false;
                    m_ParamInstrumentNo.Value = Convert.ToInt32(rowInterest["INSTRUMENTNO"]);
                    m_ParamStreamNo.Value = Convert.ToInt32(rowInterest["STREAMNO"]);
                    IBusinessDayAdjustments bda = null;

                    // ScanCompatibility_Event
                    bool isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_TradeActionGenProcess.ScanCompatibility_Event(Convert.ToInt32(rowInterest["IDE"])));
                    // isRowMustBeCalculate
                    isRowMustBeCalculate = isRowMustBeCalculate && IsRowMustBeCalculate(rowInterest);
                    //
                    if (isRowMustBeCalculate)
                    {
                        EFS_PaymentEvent paymentEvent = null;
                        try
                        {
                            if (m_IsTotalExercise && (0 <= startDate.CompareTo(CommonValDate)))
                            {
                                DataRow[] rowsSi = rowInterest.GetChildRows(DsEvents.ChildEventSi);
                                if (null != rowsSi)
                                {
                                    foreach (DataRow rowSi in rowsSi)
                                        rowSi.Delete();
                                }
                                ret = ExcludeRowChilds(rowInterest);
                                if (Cst.ErrLevel.SUCCESS == ret)
                                    rowInterest.Delete();
                            }
                            else
                            {
                                ParametersIRD.Add(m_CS, m_tradeLibrary, rowInterest);
                                CommonValParameterIRD parameter = (CommonValParameterIRD) ParametersIRD[ParamInstrumentNo, ParamStreamNo];
                                bda = parameter.CalculationPeriodDateAdjustment;
                                ret = ExcludeRowChilds(rowInterest);
                                if (Cst.ErrLevel.SUCCESS == ret)
                                {
                                    rowInterest.BeginEdit();
                                    if (m_IsTotalExercise)
                                        paymentEvent = new EFS_PaymentEvent(CommonValDate, (CommonValProcessBase)this, rowInterest);
                                    else
                                        paymentEvent = new EFS_PaymentEvent(DateTime.MinValue, (CommonValProcessBase)this, rowInterest);
                                }
                            }
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
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
                        }
                        finally
                        {
                            if ((null != paymentEvent) || (isError))
                            {
                                ret = UpdateEndDateToRowChilds(rowInterest);
                                if (m_IsTotalExercise)
                                    UpdateEventCashFlows(rowInterest, bda);
                                rowInterest.EndEdit();
                            }
                        }
                    }
                }
            }
            #endregion Payment Process

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
		#endregion RecalculationCashFlows
        #region SetFxOptionCurrencyAmount()
        // EG 20180514 [23812] Report 
        private void SetFxOptionCurrencyAmount(ref int pNewIdE, int pNewIdEParent, DataRow pRowExercise, DataRow pRowEventClassExercise)
        {
            #region Pre-Settlement
            bool isWithSettlement = (false == m_FxProvisionMsg.cashSettlementProvisionSpecified);
            EFS_PreSettlement preSettlement = null;
            if (isWithSettlement)
            {
                #region PreSettlementInfo
                IOffset offset = m_tradeLibrary.Product.ProductBase.CreateOffset(PeriodEnum.D, -2, DayTypeEnum.Business);
                EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_CS,
                                            m_FxProvisionMsg.callAmount.Currency, m_FxProvisionMsg.callAmount.payer.book.First, m_FxProvisionMsg.callAmount.receiver.book.First, offset);
                preSettlementInfo.SetOffset(m_FxProvisionMsg.callAmount.Currency, m_FxProvisionMsg.putAmount.Currency);
                #endregion PreSettlementInfo
                #region PreSettlementDate
                preSettlement = new EFS_PreSettlement(m_CS, null, m_FxProvisionMsg.cashSettlementProvision.cashSettlementPaymentDate,
                                    m_FxProvisionMsg.callAmount.Currency, m_FxProvisionMsg.putAmount.Currency, preSettlementInfo.OffsetPreSettlement, preSettlementInfo.PreSettlementMethod, m_tradeLibrary.DataDocument);
                #endregion PreSettlementDate
            }
            #endregion Pre-Settlement

            #region CallCurrency amount
            pNewIdE++;
            _ = AddEventCurrencyAmount(pNewIdE, pNewIdEParent, pRowExercise, EventTypeFunc.CallCurrency);
            _ = AddEventClassAmount(pNewIdE, pRowEventClassExercise, isWithSettlement, preSettlement);
            #endregion CallCurrency amount

            #region PutCurrency amount
            pNewIdE++;
            _ = AddEventCurrencyAmount(pNewIdE, pNewIdEParent, pRowExercise, EventTypeFunc.PutCurrency);
            _ = AddEventClassAmount(pNewIdE, pRowEventClassExercise, isWithSettlement, preSettlement);
            #endregion PutCurrency amount

        }
        #endregion SetFxOptionCurrencyAmount()
		#region StepDownNominalStep
        private void StepDownNominalStep(NotionalProvisionMsg pNotionalProvisionMsg)
		{
			DataRow[] rowVariation = GetRowVariationNominal();
			if (null != rowVariation)
			{
                decimal currentNominalStep = Convert.ToDecimal(rowVariation[0]["VALORISATION"]);
				DateTime dtStart           = Convert.ToDateTime(rowVariation[0]["DTSTARTADJ"]);
                for (int i=1;i<rowVariation.Length;i++)
				{
                    DateTime dtEnd = Convert.ToDateTime(rowVariation[i]["DTSTARTADJ"]);
                    DataRow rowNominalStep = GetRowNominalStep(dtStart, dtEnd);
                    if (null == rowNominalStep)
					{
						rowNominalStep = GetRowNominalStep(dtEnd);
						if (null == rowNominalStep)
						{
							#region Add NominalStep
                            DataRow rowCurrentNominalStep = RowCurrentNotionalStep(pNotionalProvisionMsg.instrumentNo, pNotionalProvisionMsg.streamNo, m_ProvisionMsg.provisionDate);
							rowNominalStep = AddEventNominalStep(rowCurrentNominalStep);
							#endregion Add NominalStep
						}
					}
					#region Update Event NominalStep
					UpdEventNominalStep(rowNominalStep,dtStart,dtEnd,currentNominalStep);
					#endregion Update EventClass NominalStep

					currentNominalStep -= Convert.ToDecimal(rowVariation[i]["VALORISATION"]);
					dtStart             = dtEnd;
				}
				if (m_IsTotalExercise)
					DelEventNominalStep();
			}
		}
		#endregion StepDownNominalStep
		#region StepDownVariationNominal
        private void StepDownVariationNominal(NotionalProvisionMsg pNotionalProvisionMsg)
		{
			DataRow rowSourceVariation = RowTerminationVariationNominal;
            DataRow row = GetRowVariationNominal(m_ProvisionMsg.provisionDate);
			if (null == row)
			{
                AddEventIntermediaryVariationNominalNextVersionStandBy(rowSourceVariation, pNotionalProvisionMsg);
			}
			else
			{
				row.BeginEdit();
				decimal previousAmount = Convert.ToDecimal(row["VALORISATION"]);
                int idE = Convert.ToInt32(row["IDE"]);
                // on compare les payer/receiver de la ligne de variation courante et de la ligne de termination
                // si identique nous sommes en présence d'une variation de type step down sinon step
                bool rowIsStepDown     = IsSamePayerReceiverRowVariation(rowSourceVariation,row);
                if (rowIsStepDown)
                {
                    _ = previousAmount + pNotionalProvisionMsg.provisionAmount;
                }
                else
                {
                    _ = Math.Abs(pNotionalProvisionMsg.provisionAmount - previousAmount);
                }
                row["VALORISATION"] = Math.Abs(pNotionalProvisionMsg.provisionAmount - previousAmount);
                row["VALORISATIONSYS"] = Math.Abs(pNotionalProvisionMsg.provisionAmount - previousAmount);
				if(m_IsTotalExercise)
					row["EVENTCODE"]   = EventCodeFunc.Termination;
				row.EndEdit();
				DataRow[] rowEventClass = row.GetChildRows(DsEvents.ChildEventClass);
				if (null != rowEventClass)
				{
					bool isCanBeAdded = true;
					string eventClass = GetEventClassProvision;
					foreach (DataRow rowClass in rowEventClass)
					{
						DateTime dtEvent = Convert.ToDateTime(rowClass["DTEVENT"]);
                        if ((dtEvent.Date == m_ProvisionMsg.actionDate.Date) && (eventClass == rowClass["EVENTCLASS"].ToString()))
							isCanBeAdded = false;

					}
					if (isCanBeAdded)
						AddEventClassInfoExerciseProvision(idE,rowEventClass[0],eventClass);
				}
			}

			DataRow[] rowVariation = GetRowVariationNominal();
			if (null != rowVariation)
			{
				decimal currentNominalStep = Convert.ToDecimal(rowVariation[0]["VALORISATION"]);
                _ = Convert.ToDateTime(rowVariation[0]["DTSTARTADJ"]);
                for (int i=1;i<rowVariation.Length;i++)
				{
                    DateTime dtStart = Convert.ToDateTime(rowVariation[i]["DTSTARTADJ"]);
                    if (0 < dtStart.CompareTo(m_ProvisionMsg.provisionDate))
					{
						if (m_IsTotalExercise || (0 == currentNominalStep))
						{
							DataRow[] rowsSi = rowVariation[i].GetChildRows(DsEvents.ChildEventSi);
							if (null != rowsSi)
							{
								foreach (DataRow rowSi in rowsSi)
									rowSi.Delete();
							}
							rowVariation[i].Delete();
							continue;
						}
						else if (currentNominalStep <= Convert.ToDecimal(rowVariation[i]["VALORISATION"]))
						{
							rowVariation[i].BeginEdit();
							rowVariation[i]["VALORISATION"]    = currentNominalStep;
							rowVariation[i]["VALORISATIONSYS"] = currentNominalStep;
							rowVariation[i]["EVENTCODE"]       = EventCodeFunc.Termination;
							rowVariation[i].EndEdit();
							currentNominalStep = 0;
						}
					}
					currentNominalStep -= Convert.ToDecimal(rowVariation[i]["VALORISATION"]);
				}
			}
		}
		#endregion StepDownVariationNominal
		#region UpdateEventCashFlows
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void UpdateEventCashFlows(DataRow pRow, IBusinessDayAdjustments pBusinessDayAdjustments)
		{
            #region Update Event
            pRow.BeginEdit();
			pRow["EVENTCODE"]	= EventCodeFunc.Termination;
			pRow["DTENDADJ"]	= CommonValDate;
			pRow["DTENDUNADJ"]	= CommonValDate;
			pRow.EndEdit();
			#endregion Update Event
			#region Update EventClass
			DataRow[] rowClass			= pRow.GetChildRows(DsEvents.ChildEventClass);
			DataRow   rowPreSettlement	= null;
			if ((null != rowClass) && (0 < rowClass.Length))
			{
				EFS_AdjustableDate adjustableSettlementDate = null;
				foreach (DataRow row in rowClass)
				{
					string eventClass = row["EVENTCLASS"].ToString();
					if (EventClassFunc.IsSettlement(eventClass))
					{
						#region Settlement Calculation
						row.BeginEdit();
                        adjustableSettlementDate = new EFS_AdjustableDate(m_CS, CommonValDate, pBusinessDayAdjustments, m_tradeLibrary.DataDocument);
						row["DTEVENT"]				= adjustableSettlementDate.adjustedDate.DateValue;
						row.EndEdit();
						#endregion Settlement Calculation
					}
					else if (EventClassFunc.IsPreSettlement(eventClass))
						rowPreSettlement = row;
				}
				#region Pre-Settlement Calculation
				if ((null != rowPreSettlement) && (null != adjustableSettlementDate))
				{
					//int idA_PAY		= Convert.ToInt32(pRow["IDA_PAY"]);
                    Nullable<int> idB_PAY = null;
                    if (false == Convert.IsDBNull(pRow["IDB_PAY"]))
                        idB_PAY = Convert.ToInt32(pRow["IDB_PAY"]);
					//int idA_REC		= Convert.ToInt32(pRow["IDA_REC"]);
                    Nullable<int> idB_REC = null;
                    if (false == Convert.IsDBNull(pRow["IDB_REC"]))
                        idB_REC = Convert.ToInt32(pRow["IDB_REC"]);
                    string currency = pRow["UNIT"].ToString();

                    #region Pre-Settlement
                    #region PreSettlementInfo
                    IOffset offset = m_tradeLibrary.Product.ProductBase.CreateOffset(PeriodEnum.D, -2, DayTypeEnum.Business);
                    // EG 20150706 [21021] Remove idA_PAY|idA_REC
                    EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_CS, currency, idB_PAY, idB_REC, offset);
                    #endregion PreSettlementInfo
                    #region PreSettlementDate
                    _ = new EFS_PreSettlement(m_CS, null, adjustableSettlementDate.adjustedDate.DateValue,
                        currency, preSettlementInfo.OffsetPreSettlement, m_tradeLibrary.DataDocument);
                    #endregion PreSettlementDate
                    #endregion Pre-Settlement
                }
				#endregion Pre-Settlement Calculation

			}
			#endregion Update EventClass
		}
		#endregion UpdateEventCashFlows		
		#region UpdEventDtEndPeriod
		private void UpdEventDtEndPeriod(DataRow pRowParent)
		{
			#region Update Event
			int idE = Convert.ToInt32(pRowParent["IDE"]);
			pRowParent.BeginEdit();
            pRowParent["DTENDUNADJ"] = m_ProvisionMsg.provisionDate;
            pRowParent["DTENDADJ"] = m_ProvisionMsg.provisionDate;
			pRowParent.EndEdit();
			#endregion Update Event
			#region Update EventClass
			DataRow[] rowParentClass = pRowParent.GetChildRows(DsEvents.ChildEventClass);
			if ((null != rowParentClass) && (0 < rowParentClass.Length))
			{
				AddEventClassInfoExerciseProvision(idE,rowParentClass[0]);
			}
			#endregion Update EventClass
		}
		#endregion UpdEventDtEndPeriod		
		#region UpdEventNominalStep 
		private void UpdEventNominalStep(DataRow pRowNominalStep,DateTime pDtStart,DateTime pDtEnd,decimal pAmount)
		{
			#region Update Event NominalStep
			pRowNominalStep.BeginEdit();
			pRowNominalStep["DTSTARTUNADJ"]    = pDtStart;
			pRowNominalStep["DTSTARTADJ"]      = pDtStart;
			pRowNominalStep["DTENDUNADJ"]      = pDtEnd;
			pRowNominalStep["DTENDADJ"]        = pDtEnd;
			pRowNominalStep["VALORISATION"]    = pAmount;
			pRowNominalStep["VALORISATIONSYS"] = pAmount;
			pRowNominalStep.EndEdit();
			#endregion Update Event NominalStep
			#region Update EventClass NominalStep
			DataRow[] rowNominalStepClass = pRowNominalStep.GetChildRows(DsEvents.ChildEventClass);
			foreach (DataRow row in rowNominalStepClass)
			{
				row.BeginEdit();
				row["DTEVENT"]       = pDtStart;
				row["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS,pDtStart);
				row.EndEdit();
			}
			#endregion Update EventClass NominalStep
		}
		#endregion UpdEventNominalStep 
		#region UpdEventStream
		private void UpdEventStream()
		{
			DataRow rowTerminationVariationNominal = RowTerminationVariationNominal;
			DataRow rowParent                      = rowTerminationVariationNominal.GetParentRow(DsEvents.ChildEvent);
			if (null != rowParent)
			{
				#region Stream
				UpdEventDtEndPeriod(rowParent);
				#endregion Stream
			}
		}
		#endregion UpdEventParent		
		#region UpdEventStreamParent
		private void UpdEventStreamParent()
		{
			DataRow rowTerminationVariationNominal = RowTerminationVariationNominal;
			DataRow rowParent                      = rowTerminationVariationNominal.GetParentRow(DsEvents.ChildEvent);
			if (null != rowParent)
			{
				rowParent = rowParent.GetParentRow(DsEvents.ChildEvent);
				if (null != rowParent)
				{
					#region Product
					UpdEventDtEndPeriod(rowParent);
					#endregion Product
					rowParent = rowParent.GetParentRow(DsEvents.ChildEvent);
					if ((null != rowParent) && EventCodeFunc.IsTrade(rowParent["EVENTCODE"].ToString()))
					{
						#region Trade
						UpdEventDtEndPeriod(rowParent);
						#endregion Trade
					}
				}
			}
		}
		#endregion UpdEventParent		
		#region UpdateEndDateToRowChilds
		private Cst.ErrLevel UpdateEndDateToRowChilds(DataRow pRowParent)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
			if (m_IsTotalExercise)
			{
				DataRow[] rowsChild = pRowParent.GetChildRows(DsEvents.ChildEvent);
				if (null != rowsChild)
				{
					foreach (DataRow rowChild in rowsChild)
					{
						if (IsRowMustBeCalculate(rowChild))
						{
							rowChild["DTENDADJ"] = CommonValDate;
							rowChild["DTENDUNADJ"] = CommonValDate;
							ret = UpdateEndDateToRowChilds(rowChild);
							if (Cst.ErrLevel.SUCCESS != ret)
								break;
						}
					}
				}
			}
			return ret;
		}
		#endregion UpdateAndExcludeRowChilds
		#region Valorize
        // EG 20180514 [23812] Report 
        // EG 20180614 Upd (Call AddEventClassInfoExercieFxProvision)
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Valorize()
        {
            int newIdEParent;
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            foreach (ActionMsgBase actionMsg in m_TradeAction.ActionMsgs)
            {
                if (IsProvisionMsg(actionMsg))
                {
                    #region ProvisionEvent Process
                    int nbIdE = 1;
                    // EG 20180514 [23812] Report 
                    if (actionMsg is FxOptionalEarlyTerminationProvisionMsg)
                    {
                        m_FxProvisionMsg = actionMsg as FxOptionalEarlyTerminationProvisionMsg;

                        if (m_FxProvisionMsg.callAmountSpecified)
                        {
							// PM 20210121 [XXXXX] Passage du message au niveau de log None
							Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7220), 0,
                                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id)),
                                new LogParam(m_FxProvisionMsg.exerciseType),
                                new LogParam(LogTools.AmountAndCurrency(m_FxProvisionMsg.callAmount.Amount, m_FxProvisionMsg.callAmount.Currency)),
                                new LogParam(LogTools.AmountAndCurrency(m_FxProvisionMsg.putAmount.Amount, m_FxProvisionMsg.putAmount.Currency)),
                                new LogParam(LogTools.AmountAndCurrency(m_FxProvisionMsg.cashSettlementProvision.amount, m_FxProvisionMsg.cashSettlementProvision.currency))));

                            nbIdE += 2;
                        }
                        else
                        {
							// PM 20210121 [XXXXX] Passage du message au niveau de log None
							Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7220), 0,
                                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id)),
                                new LogParam(m_FxProvisionMsg.exerciseType),
                                new LogParam("-"),
                                new LogParam("-"),
                                new LogParam(LogTools.AmountAndCurrency(m_FxProvisionMsg.cashSettlementProvision.amount, m_FxProvisionMsg.cashSettlementProvision.currency))));
                        }
                    }
                    m_ProvisionMsg = actionMsg as ProvisionMsgBase;
                    m_IsTotalExercise = EventTypeFunc.IsTotal(m_ProvisionMsg.exerciseType);
                    DataRow rowExerciseSource = RowEvent(m_TradeAction.idE);

                    if (m_ProvisionMsg.cashSettlementProvisionSpecified)
                        nbIdE++;
                    if (m_ProvisionMsg.feeProvisionSpecified)
                        nbIdE++;
                    ret = SQLUP.GetId(out int newIdE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbIdE);
                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        #region Exercise provision event
                        DataRow rowExercise = AddEventExercise(newIdE, rowExerciseSource);
                        DataRow rowEventClassExercise = AddEventClassExercise(newIdE);
                        AddEventDetailExercise(newIdE);
                        newIdEParent = newIdE;
                        #endregion Exercise provision event

                        #region EventClass (EXO on TRD|PRD|STREAM)
                        if ((null != m_FxProvisionMsg) && m_IsTotalExercise)
                            AddEventClassInfoExercieFxProvision();
                        #endregion EventClass (EXO on TRD|PRD|STREAM)

                        #region FxProvision Call & Put Currency event
                        // EG 20180514 [23812] Report 
                        if ((null != m_FxProvisionMsg) && m_FxProvisionMsg.callAmountSpecified)
                            SetFxOptionCurrencyAmount(ref newIdE, newIdEParent, rowExercise, rowEventClassExercise);
                        #endregion FxProvision Call & Put Currency event

                        #region Fee amount
                        if (m_ProvisionMsg.feeProvisionSpecified)
                        {
                            newIdE++;
                            AddEventFee(newIdE, newIdEParent, rowExercise);
                            AddEventClassFee(newIdE, rowEventClassExercise);
                            AddEventDetailFee(newIdE);
                        }
                        #endregion Fee amount
                        #region CashSettlement amount
                        if (m_ProvisionMsg.cashSettlementProvisionSpecified)
                        {
                            newIdE++;
                            AddEventCashSettlement(newIdE, newIdEParent, rowExercise);
                            AddEventClassCashSettlement(newIdE, rowEventClassExercise);
                            AddEventDetailExercise(newIdE);
                        }
                        #endregion CashSettlement amount
                    }
                    #endregion ProvisionEvent Process
                    if (m_ProvisionMsg.notionalProvisionSpecified)
                    {
                        #region Notional Stream inserting/updating
                        foreach (NotionalProvisionMsg item in m_ProvisionMsg.notionalProvision)
                        {
                            m_IsTotalExercise = m_IsTotalExercise || (item.originalAmount == item.provisionAmount);
                            if (false == IsExtendibleProvisionMsg(actionMsg))
                            {
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7242), 1,
                                    new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id)),
                                    new LogParam(LogTools.AmountAndCurrency(item.originalAmount, item.originalCurrency)),
                                    new LogParam(LogTools.AmountAndCurrency(item.provisionAmount, item.provisionCurrency))));

                                m_ParamInstrumentNo.Value = Convert.ToInt32(item.instrumentNo);
                                m_ParamStreamNo.Value = Convert.ToInt32(item.streamNo);
                                CalculEventVariationNominal(item);
                                CalculEventNominalStep(item);
                                if (m_IsTotalExercise)
                                    UpdEventStream();
                            }
                        }
                        #endregion Notional Stream inserting/updating
                        #region Recalculation Cashflows
                        RecalculationCashFlows();
                        #endregion Recalculation Cashflows
                        #region Update DtPeriod Product/Trade
                        if (m_IsTotalExercise)
                            UpdEventStreamParent();
                        #endregion Update DtPeriod Product/Trade
                    }
                }
            }
            Update();
            return ret;
        }	
		#endregion Valorize
		#region ValorizeNextVersionStandBy
        public Cst.ErrLevel ValorizeNextVersionStandBy()
        {
            int newIdEParent;
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            foreach (ActionMsgBase actionMsg in m_TradeAction.ActionMsgs)
            {
                if (IsProvisionMsg(actionMsg))
                {
                    #region ProvisionEvent Process
                    m_ProvisionMsg = actionMsg as ProvisionMsgBase;
                    m_IsTotalExercise = EventTypeFunc.IsTotal(m_ProvisionMsg.exerciseType);
                    DataRow rowExerciseSource = RowEvent(m_TradeAction.idE);
                    int nbIdE = 1;
                    if (m_ProvisionMsg.cashSettlementProvisionSpecified)
                        nbIdE++;
                    if (m_ProvisionMsg.feeProvisionSpecified)
                        nbIdE++;
                    ret = SQLUP.GetId(out int newIdE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbIdE);
                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        #region Exercise provision event
                        DataRow rowExercise = AddEventExercise(newIdE, rowExerciseSource);
                        DataRow rowEventClassExercise = AddEventClassExercise(newIdE);
                        AddEventDetailExercise(newIdE);
                        newIdEParent = newIdE;
                        #endregion Exercise provision event
                        #region Fee amount
                        if (m_ProvisionMsg.feeProvisionSpecified)
                        {
                            newIdE++;
                            AddEventFee(newIdE, newIdEParent, rowExercise);
                            AddEventClassFee(newIdE, rowEventClassExercise);
                            AddEventDetailFee(newIdE);
                        }
                        #endregion Fee amount
                        #region CashSettlement amount
                        if (m_ProvisionMsg.cashSettlementProvisionSpecified)
                        {
                            newIdE++;
                            AddEventCashSettlement(newIdE, newIdEParent, rowExercise);
                            AddEventClassCashSettlement(newIdE, rowEventClassExercise);
                        }
                        #endregion CashSettlement amount
                    }
                    #endregion ProvisionEvent Process
                    if (m_ProvisionMsg.notionalProvisionSpecified)
                    {
                        #region Notional Stream inserting/updating
                        foreach (NotionalProvisionMsg item in m_ProvisionMsg.notionalProvision)
                        {
                            m_ParamInstrumentNo.Value = Convert.ToInt32(item.instrumentNo);
                            m_ParamStreamNo.Value = Convert.ToInt32(item.streamNo);
                            if (IsStepDownProvisionMsg(actionMsg))
                            {
                                m_IsTotalExercise = m_IsTotalExercise || (item.originalAmount == item.provisionAmount);
                                StepDownVariationNominal(item);
                                StepDownNominalStep(item);
                                if (m_IsTotalExercise)
                                    UpdEventStream();
                            }
                            else if (IsStepUpProvisionMsg(actionMsg))
                            {
                                //item.provisionAmount = -1 * item.provisionAmount;
                                //StepUpVariationNominal(item);
                                //StepUpNominalStep(item);
                            }
                        }
                        #endregion Notional Stream inserting/updating
                        #region Recalculation Cashflows
                        RecalculationCashFlows();
                        #endregion Recalculation Cashflows
                        #region Update DtPeriod Product/Trade
                        if (m_IsTotalExercise)
                            UpdEventStreamParent();
                        #endregion Update DtPeriod Product/Trade
                    }
                }
            }
            Update();
            return ret;
        }	
		#endregion ValorizeNextVersionStandBy
		#endregion Methods
	}
	#endregion TradeActionGenProvision
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
//
using EFS.ACommon;
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
using FpML.Interface;

namespace EFS.Process
{
	#region TradeActionGenBase
	public class TradeActionGenProcessBase : CommonValProcessBase
	{
		#region Members
		protected TradeActionGenProcess m_TradeActionGenProcess;
        protected TradeActionGenMQueue m_TradeActionGenMQueue;
        protected TradeActionBaseMQueue m_TradeAction;
		private Cst.ErrLevel m_CodeReturn;
		#endregion Members
		#region Accessors
		#region CurrentTrade
		public ITrade CurrentTrade
		{
			get {return m_tradeLibrary.CurrentTrade;}
		}
		#endregion CurrentTrade
        #region TradeAction
        public TradeActionBaseMQueue TradeAction
        {
            get { return m_TradeAction; }
        }
        #endregion TradeAction
        #region DataSet objects
        #region DtTrade
        public DataTable DtTrade
		{
			get{return m_DsTrade.DtTrade;}
		}
		#endregion DtTrade
		#region DtTradeStream
		public DataTable DtTradeStream
		{
			get{return m_DsTrade.DtTradeStream;}
		}
		#endregion DtTradeStream

		#region DtEvent
		public DataTable DtEvent
		{
			get{return m_DsEvents.DtEvent;}
		}
		#endregion DtEvent
        #region DtEventAsset
        protected DataTable DtEventAsset
        {
            get { return m_DsEvents.DtEventAsset; }
        }
        #endregion DtEventAsset
		#region DtEventClass
		protected DataTable DtEventClass
		{
			get{return m_DsEvents.DtEventClass;}
		}
		#endregion DtEventClass
        #region DtEventDet
        protected DataTable DtEventDet
        {
            get { return m_DsEvents.DtEventDet; }
        }
        #endregion DtEventDet

        #region RowNominalStep
        private  DataRow[] RowNominalStep
		{
			get
			{
                return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE = '{3}'",
                m_ParamIdT.Value, m_ParamInstrumentNo.Value, m_ParamStreamNo.Value, EventCodeFunc.NominalStep), "IDE");
			}
		}
		#endregion RowNominalStep
		#region RowOption
		protected DataRow RowOption
		{
			get 
			{
				DataRow[] rowOption = DtEvent.Select("IDE=" + m_TradeAction.idE.ToString(),"IDE");
				if (0 < rowOption.Length)
					return rowOption[0];
				return null;
			}
		}
		#endregion RowTrigger
		#region RowPayout
		protected DataRow RowPayout
		{
			get
			{
                DataRow[] rowPayout = DtEvent.Select(StrFunc.AppendFormat(@"IDE_EVENT = {0} and EVENTCODE = '{1}' and EVENTTYPE = '{2}' ",
                m_TradeAction.idE, EventCodeFunc.Termination, EventTypeFunc.Payout), "IDE");
				if (0 < rowPayout.Length)
					return rowPayout[0];
				return null;
			}
		}
		#endregion RowProduct
		#region RowRemoveTrade
		protected DataRow RowRemoveTrade
		{
			get
			{
                DataRow[] rowRemoveTrade = DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}' and EVENTTYPE = '{2}' ",
                m_ParamIdT.Value, EventCodeFunc.RemoveTrade, EventTypeFunc.Date), "IDE");
				if (0 < rowRemoveTrade.Length)
					return rowRemoveTrade[0];
				return null;
			}
		}
		#endregion RowProduct
		#region RowTrade
		protected DataRow RowTrade
		{
			get 
			{
				DataRow[] rowTrade = DtEvent.Select("IDT=" + m_ParamIdT.Value.ToString(),"IDE");
				if (0 < rowTrade.Length)
					return rowTrade[0];
				return null;
			}
        }
        #endregion RowTrade
		#endregion DataSet objects
		#region IsAllBarrierDeclared
		protected bool IsAllBarrierDeclared
		{
			get
			{
                DataRow[] rowsBarrier = GetRowBarriers();
                foreach (DataRow rowBarrier in rowsBarrier)
				{
					if (false == Cst.StatusTrigger.IsStatusDeclared(rowBarrier["IDSTTRIGGER"].ToString()))
						return false;
				}
				return true;
			}
		}
		#endregion IsAllBarrierDeclared
		#region IsAllTriggerActivated
		protected bool IsAllTriggerActivated
		{
			get
			{
                DataRow[] rowsTrigger = GetRowTriggers();
                foreach (DataRow rowTrigger in rowsTrigger)
				{
					if (Cst.StatusTrigger.IsStatusDeActivated(rowTrigger["IDSTTRIGGER"].ToString()))
						return false;
				}
				return true;
			}
		}
		#endregion IsAllTriggerActivated
		#region IsAllTriggerDeclared
		protected bool IsAllTriggerDeclared
		{
			get
			{
                DataRow[] rowsTrigger = GetRowTriggers();
                foreach (DataRow rowTrigger in rowsTrigger)
				{
					if (false == Cst.StatusTrigger.IsStatusDeclared(rowTrigger["IDSTTRIGGER"].ToString()))
						return false;
				}
				return true;
			}
		}
		#endregion IsAllTriggerDeclared
		#region IsNoBarrierActivated
		protected bool IsNoBarrierActivated
		{
			get
			{
                DataRow[] rowsBarrier = GetRowBarriers();
                foreach (DataRow rowBarrier in rowsBarrier)
				{
					if (Cst.StatusTrigger.IsStatusActivated(rowBarrier["IDSTTRIGGER"].ToString()))
						return false;
				}
				return true;
			}
		}
		#endregion IsNoBarrierActivated
		#region IsNoTriggerActivated
		protected bool IsNoTriggerActivated
		{
			get
			{
                DataRow[] rowsTrigger = GetRowTriggers();
                foreach (DataRow rowTrigger in rowsTrigger)
				{
					if (Cst.StatusTrigger.IsStatusActivated(rowTrigger["IDSTTRIGGER"].ToString()))
						return false;
				}
				return true;
			}
		}
		#endregion IsNoTriggerActivated
		#region IsOneBarrierActivated
		protected bool IsOneBarrierActivated
		{
			get
			{
                DataRow[] rowsBarrier = GetRowBarriers();
                foreach (DataRow rowBarrier in rowsBarrier)
				{
					if (Cst.StatusTrigger.IsStatusActivated(rowBarrier["IDSTTRIGGER"].ToString()))
						return true;
				}
				return false;
			}
		}
		#endregion IsOneBarrierActivated
		#region IsOneTriggerActivated
		protected bool IsOneTriggerActivated
		{
			get
			{
                DataRow[] rowsTrigger = GetRowTriggers();
                foreach (DataRow rowTrigger in rowsTrigger)
				{
					if (Cst.StatusTrigger.IsStatusActivated(rowTrigger["IDSTTRIGGER"].ToString()))
						return true;
				}
				return false;
			}
		}
		#endregion IsOneTriggerActivated
        #region DataDocContainer
        protected DataDocumentContainer DataDocument
		{
			get { return m_tradeLibrary.DataDocument; }
        }
        #endregion DataDocContainer
		#region TradeHeader
		protected ITradeHeader TradeHeader
		{
			get { return CurrentTrade.TradeHeader; }
		}
		#endregion TradeHeader
		#region CodeReturn
		public Cst.ErrLevel CodeReturn
		{
			set{m_CodeReturn = value;}
			get{return m_CodeReturn;}
        }
        #endregion CodeReturn
        #endregion Accessors
        #region Constructors
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20180502 Analyse du code Correction [CA2214]
        public TradeActionGenProcessBase(TradeActionGenProcess pTradeActionGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, TradeActionBaseMQueue pTradeAction)
            : base(pTradeActionGenProcess, pDsTrade, pTradeLibrary, pTradeLibrary.Product)
        {
            m_TradeActionGenProcess = pTradeActionGenProcess;
            m_TradeActionGenMQueue = (TradeActionGenMQueue)m_TradeActionGenProcess.MQueue;
            m_TradeAction = pTradeAction;
            m_ParamIdEParent.Value = m_TradeAction.idE;
            // EG 20150612 [20665]
            //InitializeDataSetEvent();
        }
		#endregion Constructors

		#region Methods
        #region DeactivAllEvents
        public Cst.ErrLevel DeactivAllEvents(DataTable pDtEvent)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            #region UpdateRow IDSTACTIVATION = DEACTIV for all events (excluded TRD/DAT and RMV/DAT)
            foreach (DataRow row in pDtEvent.Rows)
            {
                string eventCode = row["EVENTCODE"].ToString();
                if ((false == EventCodeFunc.IsTrade(eventCode)) && (false == EventCodeFunc.IsRemoveTrade(eventCode)))
                {
                    row.BeginEdit();
                    row["IDSTACTIVATION"] = Cst.StatusActivation.DEACTIV.ToString();
                    row["IDASTACTIVATION"] = m_TradeActionGenProcess.Session.IdA;
                    row["DTSTACTIVATION"] = OTCmlHelper.GetDateSysUTC(m_CS);
                    row.EndEdit();
                }
            }
            #endregion UpdateRow IDSTACTIVATION = DEACTIV for all events (excluded TRD/DAT and RMV/DAT)
            return ret;
        }
        #endregion DeactivAllEvents

		#region RowCurrentNotionalStep
		protected DataRow RowCurrentNotionalStep(string pInstrumentNo,string pStreamNo,DateTime pDate)
		{
            m_ParamInstrumentNo.Value = Convert.ToInt32(pInstrumentNo);
			m_ParamStreamNo.Value     = Convert.ToInt32(pStreamNo);
			DataRow[] rowsNominalStep = RowNominalStep;
			foreach (DataRow dr in rowsNominalStep)
			{
                DateTime dtStart = Convert.ToDateTime(dr["DTSTARTADJ"]);
                DateTime dtEnd = Convert.ToDateTime(dr["DTENDADJ"]);
                if ((0 <= pDate.CompareTo(dtStart)) && (0 <= dtEnd.CompareTo(pDate)))
					return dr;
			}
			return null;
		}
		#endregion RowCurrentNotionalStep
		#region RowStartVariationNominal
		protected DataRow RowStartVariationNominal
		{
			get
			{
                DataRow[] row = DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE in ('{3}','{4}') and EVENTTYPE = '{5}' ",
                m_ParamIdT.Value, m_ParamInstrumentNo.Value, m_ParamStreamNo.Value, EventCodeFunc.Start, EventCodeFunc.StartIntermediary, EventTypeFunc.Nominal), "IDE");
				if ((null != row) && (1 == row.Length))
					return row[0];
				return null;
			} 
		}
		#endregion RowStartVariationNominal
		#region RowTerminationVariationNominal
		protected DataRow RowTerminationVariationNominal
		{
			get
			{
                DataRow[] row = DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and STREAMNO = {2} and EVENTCODE in ('{3}','{4}') and EVENTTYPE = '{5}' ",
                m_ParamIdT.Value, m_ParamInstrumentNo.Value, m_ParamStreamNo.Value, EventCodeFunc.Termination, EventCodeFunc.TerminationIntermediary, EventTypeFunc.Nominal), "IDE");
				if ((null != row) && (1 == row.Length))
					return row[0];
				return null;
			} 
		}
		#endregion RowTerminationVariationNominal

		#region AddEventAbandonExerciseOut
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected DataRow AddEventAbandonExerciseOut(int pIdE, DataRow pRowSource, ActionMsgBase pActionMsg, string pCode, string pType)
		{
            
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7239), 1,
                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id)),
                new LogParam(pCode + " / " + pType)));

            DataRow row = DtEvent.NewRow();
			row.ItemArray = (object[])pRowSource.ItemArray.Clone();
			row.BeginEdit();
			row["IDE"] = pIdE;
			row["IDE_EVENT"] = m_TradeAction.idE;
			row["EVENTCODE"] = pCode;
			row["EVENTTYPE"] = pType;
            row["DTSTARTADJ"] = pActionMsg.actionDate.Date;
            row["DTSTARTUNADJ"] = pActionMsg.actionDate.Date;
            row["DTENDADJ"] = pActionMsg.actionDate.Date;
            row["DTENDUNADJ"] = pActionMsg.actionDate.Date;
			row["IDSTTRIGGER"] = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
			row["IDSTCALCUL"] = StatusCalculFunc.Calculated;
            row["SOURCE"] = m_TradeActionGenProcess.AppInstance.ServiceName;
            if (pActionMsg is BO_AbandonExerciseMsgBase)
            {
                BO_AbandonExerciseMsgBase abandonExerciseMsg = pActionMsg as BO_AbandonExerciseMsgBase;
                row["VALORISATION"] = abandonExerciseMsg.nbOptions.Quantity;
                row["UNIT"] = Convert.DBNull;
                row["UNITTYPE"] = UnitTypeEnum.Qty;
                row["VALORISATIONSYS"] = abandonExerciseMsg.nbOptions.Quantity;
                row["UNITSYS"] = Convert.DBNull;
                row["UNITTYPESYS"] = UnitTypeEnum.Qty;
            }
			row.EndEdit();
			DtEvent.Rows.Add(row);
			SetRowStatus(row,TuningOutputTypeEnum.OES);
			return row;
		}
		#endregion AddEventAbandonExerciseOut
		#region AddEventClassAbandonExerciseOut
		protected DataRow AddEventClassAbandonExerciseOut(int pIdE, DateTime pClassDate)
		{
			DataRow row = DtEventClass.NewRow();
			row.BeginEdit();
			row["IDE"]      = pIdE;
			row["EVENTCLASS"]    = EventClassFunc.GroupLevel;
			row["DTEVENT"]       = pClassDate;
			row["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS,pClassDate);
            row["ISPAYMENT"] = false;
            row.EndEdit();
			DtEventClass.Rows.Add(row);
			return row;
		}
		#endregion AddEventClassAbandonExerciseOut
		#region AddEventDetailAbandonExerciseOut
        protected DataRow AddEventDetailAbandonExerciseOut(int pIdE, ActionMsgBase pActionMsg)
		{
			DataRow rowDetail = m_DsEvents.DtEventDet.NewRow();
			rowDetail.BeginEdit();
			rowDetail["IDE"] = pIdE;
            rowDetail["NOTE"] = pActionMsg.note;
            rowDetail["DTACTION"] = pActionMsg.actionDate;
			rowDetail.EndEdit();
            m_DsEvents.DtEventDet.Rows.Add(rowDetail);
            return rowDetail;
		}
		#endregion AddEventDetailAbandonExerciseOut

		/// <summary>
		/// Recalcul de certains frais sur un trade (cf. FeesCalculationSettingsMode2)
		/// <para>- possibilité de conserver les frais forcés manuellement</para>
		/// </summary>
		/// <param name="pFeesCalculationSetting">Définition du périmètre des frais à recalculer (Liste de barèmes)</param>
		/// <returns></returns>
		// FI 20170323 [XXXXX] Modify
		// EG 20190114 Add detail to ProcessLog Refactoring
		// PL 20191218 [25099] Refactoring
		// PL 20200109 [25099] private to internal
		// PL 20200115 [25099] Rename from FeeCalculation() to FeeCalculationAndWrite()
		internal Cst.ErrLevel FeeCalculationAndWrite(FeesCalculationSettingsMode2 pFeesCalculationSetting)
		{
			// Pour l'instant une seule fonctionalité => Recalcul des frais sur certains barêmes
			if ((!pFeesCalculationSetting.feeSheduleSpecified) || (null == pFeesCalculationSetting.feeShedule))
				throw new NotSupportedException("feeShedule is not specified");

			EFS.TradeInformation.TradeInput tradeInput = new EFS.TradeInformation.TradeInput();
			List<Pair<IPayment, string>> allPayments = new List<Pair<IPayment, string>>();
			IEnumerable<IPayment> modifiedPayments = null;
			bool isForcedFeesPreserved = m_TradeActionGenMQueue.GetBoolValueParameterById(TradeActionGenMQueue.PARAM_ISFORCEDFEES_PRESERVED);

			Cst.ErrLevel codeReturn = EFS.Process.EventsGen.New_EventsGenAPI.FeeCalculation(pFeesCalculationSetting,
																	  m_TradeActionGenProcess, isForcedFeesPreserved,
																	  ref tradeInput, ref allPayments, ref modifiedPayments);

			if (modifiedPayments.Count() > 0)
			{
				#region MAJ des tables SQL (EVENT et TRADE)
				int idT = pFeesCalculationSetting.trade.Key;

				//En 1er les frais déjà existants (PreservedOpp) pour que l'ordre des frais dans les événements soit identique à l'ordre des frais dans OPP
				IPayment[] allPayments_Sorted = ((from item in allPayments.Where(x => x.Second == "PreservedOpp")
												  select item.First).Concat(from item in modifiedPayments
																			select item)).ToArray();

				// FI 20200106 [XXXXX] Call Tools.SetPaymentId
				// Mise en place des Id uniquement sur les frais modifiés
				// Il ne faut pas mettre des Id sur les frais préservés (ceux-ci conservent leur Id initial qui par ailleurs peut être renseigné ou pas)  
				// => il ne faut pas mettre un Id sur un frais préservé sur lequel l'Id est non renseigné. Pour ce frais EVENTFEE.PAYMENTID est non renseigné.  
				Tools.SetPaymentId(modifiedPayments.ToArray(), "OPP", Tools.GetMaxIndex(allPayments_Sorted.ToArray(), "OPP") + 1);

				IProduct product = tradeInput.Product.Product;

				int idE_Event = GetIdE_TRDDAT(); //PL 20191218 Move here 
				DateTime businessDate = new ProductContainer(product, m_tradeLibrary.DataDocument).GetBusinessDate2();

				string CS = m_TradeActionGenProcess.Cs;


				using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS))
				{
					try
					{
						//Delete EVENT
						foreach (IPayment payment in modifiedPayments)
							TradeRDBMSTools.DeleteFeeEventAuto(CS, dbTransaction, idT, PaymentTools.GetIdFeeShedule(payment), PaymentTools.GetIdFeeMatrix(payment));

						//Prepare Modified Payments
						int nbEvent = 0;
						IPayment[] modifiedPayments_Prepared = EventQuery.PrepareFeeEvents(CS, product, tradeInput.DataDocument, idT, modifiedPayments.ToArray(), ref nbEvent);

					//Insert EVENT
					SQLUP.GetId(out int newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbEvent);
					EventQuery eventQuery = new EventQuery(m_TradeActionGenProcess.Session, m_TradeActionGenProcess.ProcessType, m_TradeActionGenProcess.Tracker.IdTRK_L);
					eventQuery.InsertFeeEvents(CS, dbTransaction, tradeInput.DataDocument, idT, businessDate, idE_Event, modifiedPayments_Prepared, newIdE);

						//Update TRADE
						// FI 20170323 [XXXXX] Mise en place des paramètres nécessaires à l'alimentation de TRADETRAIL
						// FI 20200820 [25468] dates systèmes en UTC
						// RD 20230727 [26451] Process.ProcessLog null, usage m_TradeActionGenProcess.Tracker à la place. 
						EventQuery.UpdateTradeXMLForFees(dbTransaction, idT, tradeInput.DataDocument, allPayments_Sorted, OTCmlHelper.GetDateSysUTC(CS), Process.UserId,
							Process.Session, m_TradeActionGenProcess.Tracker.IdTRK_L, m_TradeActionGenProcess.Tracker.IdProcess);

						DataHelper.CommitTran(dbTransaction);

						// LOG-07066 => <b>Liste des frais modifiés.</b> - Négociation: <b>{1}</b>
						Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7066), 0,
												new LogParam(LogTools.IdentifierAndId(pFeesCalculationSetting.trade))));

						m_TradeActionGenProcess.AddLogFeeInformation(tradeInput.DataDocument, modifiedPayments.ToArray(), LogLevelDetail.LEVEL3, 1);

					}
					catch (Exception)
					{
						codeReturn = Cst.ErrLevel.FAILURE;
						if (DataHelper.IsTransactionValid(dbTransaction))
							DataHelper.RollbackTran(dbTransaction);
						throw;
					}
				}
				#endregion MAJ des tables SQL (EVENT et TRADE)
			}

			return codeReturn;
		}

/// <summary>
/// Retoure l'IDE de l'évènement TRD/DAT
/// </summary>
/// <returns></returns>
/// <exception cref="Exception Lorsque l'événement n'existe pas"/>
internal int GetIdE_TRDDAT()
{
DataRow[] row = DtEvent.Select("EVENTCODE = 'TRD' and EVENTTYPE='DAT'");
if (ArrFunc.IsEmpty(row))
throw new Exception(StrFunc.AppendFormat("Event (EventCode:({0}), EventType:({1}) not found", "TRD", "DAT"));

return Convert.ToInt32(row[0]["IDE"]);
}

#region GetParties
protected IParty[] GetParties()
{
return m_tradeLibrary.Party;
}
#endregion GetParties
#region GetRowBarriers
protected DataRow[] GetRowBarriers()
{
return DtEvent.Select(StrFunc.AppendFormat(@"IDE_EVENT = {0} and EVENTCODE = '{1}'", m_TradeAction.idE, EventCodeFunc.Barrier), "IDE");
}
#endregion GetRowBarriers
#region GetRowTriggers
protected DataRow[] GetRowTriggers()
{
return DtEvent.Select(StrFunc.AppendFormat(@"IDE_EVENT = {0} and EVENTCODE = '{1}'", m_TradeAction.idE, EventCodeFunc.Trigger), "IDE");
}
#endregion GetRowTriggers

#region IsAbandonMsg
protected static bool IsAbandonMsg(ActionMsgBase pActionMsg)
{
return IsBO_AbandonMsg(pActionMsg) || IsEQD_AbandonMsg(pActionMsg) || IsFX_AbandonMsg(pActionMsg);
}
#endregion IsAbandonMsg
#region IsExerciseMsg
protected static bool IsExerciseMsg(ActionMsgBase pActionMsg)
{
return IsBO_ExerciseMsg(pActionMsg) || IsEQD_ExerciseMsg(pActionMsg) || IsFX_ExerciseMsg(pActionMsg);
}
#endregion IsExerciseMsg

#region IsBO_AbandonExerciseMsg
protected static bool IsBO_AbandonExerciseMsg(ActionMsgBase pActionMsg)
{
return IsBO_AbandonMsg(pActionMsg) || IsBO_ExerciseMsg(pActionMsg);
}
#endregion IsBO_AbandonExerciseMsg

#region IsBO_AbandonMsg
protected static bool IsBO_AbandonMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is BO_AbandonMsg);
}
#endregion IsBO_AbandonMsg
#region IsBO_ExerciseMsg
protected static bool IsBO_ExerciseMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is BO_ExerciseMsg);
}
#endregion IsBO_ExerciseMsg

#region IsEQD_AbandonExerciseMsg
protected static bool IsEQD_AbandonExerciseMsg(ActionMsgBase pActionMsg)
{
return IsEQD_AbandonMsg(pActionMsg) || IsEQD_ExerciseMsg(pActionMsg);
}
#endregion IsEQD_AbandonExerciseMsg

#region IsEQD_AbandonMsg
protected static bool IsEQD_AbandonMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is EQD_AbandonMsg);
}
#endregion IsEQD_AbandonMsg
#region IsEQD_ExerciseMsg
protected static bool IsEQD_ExerciseMsg(ActionMsgBase pEventLine)
{
return (pEventLine is EQD_ExerciseMsg);
}
#endregion IsEQD_ExerciseMsg

#region IsFX_AbandonExerciseMsg
protected static bool IsFX_AbandonExerciseMsg(ActionMsgBase pActionMsg)
{
return IsFX_AbandonMsg(pActionMsg) || IsFX_ExerciseMsg(pActionMsg);
}
#endregion IsFX_AbandonExerciseMsg

#region IsFX_AbandonMsg
protected static bool IsFX_AbandonMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is FX_AbandonMsg);
}
#endregion IsFX_AbandonMsg
#region IsFX_ExerciseMsg
protected static bool IsFX_ExerciseMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is FX_ExerciseMsg);
}
#endregion IsFX_ExerciseMsg

#region IsBarrierOrTriggerMsg
protected static bool IsBarrierOrTriggerMsg(ActionMsgBase pActionMsg)
{
return IsBarrierMsg(pActionMsg) || IsTriggerMsg(pActionMsg);
}
#endregion IsBarrierOrTriggerMsg
#region IsBarrierMsg
protected static bool IsBarrierMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is BarrierMsg);
}
#endregion IsBarrierMsg
#region IsTriggerMsg
protected static bool IsTriggerMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is TriggerMsg);
}
#endregion IsTriggerMsg

#region IsProvisionMsg
protected static bool IsProvisionMsg(ActionMsgBase pActionMsg)
{
return IsCancelableProvisionMsg(pActionMsg) ||
IsExtendibleProvisionMsg(pActionMsg) ||
IsOptionalEarlyTerminationProvisionMsg(pActionMsg) ||
IsMandatoryEarlyTerminationProvisionMsg(pActionMsg) ||
IsStepDownProvisionMsg(pActionMsg) ||
IsStepUpProvisionMsg(pActionMsg);
}
#endregion IsProvisionMsg
#region IsCancelableProvisionMsg
protected static bool IsCancelableProvisionMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is CancelableProvisionMsg);
}
#endregion IsCancelableProvisionMsg
#region IsExtendibleProvisionMsg
protected static bool IsExtendibleProvisionMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is ExtendibleProvisionMsg);
}
#endregion IsExtendibleProvisionMsg
#region IsOptionalEarlyTerminationProvisionMsg
// EG 20180514 [23812] Report 
protected static bool IsOptionalEarlyTerminationProvisionMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is OptionalEarlyTerminationProvisionMsg) || (pActionMsg is FxOptionalEarlyTerminationProvisionMsg);
}
#endregion IsOptionalEarlyTerminationProvisionMsg
#region IsMandatoryEarlyTerminationProvisionMsg
protected static bool IsMandatoryEarlyTerminationProvisionMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is MandatoryEarlyTerminationProvisionMsg);
}
#endregion IsMandatoryEarlyTerminationProvisionMsg
#region IsStepDownProvisionMsg
protected static bool IsStepDownProvisionMsg(ActionMsgBase pActionMsg)
{
return (IsCancelableProvisionMsg(pActionMsg) ||
IsOptionalEarlyTerminationProvisionMsg(pActionMsg) ||
IsMandatoryEarlyTerminationProvisionMsg(pActionMsg));
}
#endregion IsStepDownProvisionMsg
#region IsStepUpProvisionMsg
protected static bool IsStepUpProvisionMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is StepUpProvisionMsg);
}
#endregion IsStepUpProvisionMsg

#region IsRemoveTradeEventMsg
protected static bool IsRemoveTradeEventMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is RemoveTradeEventMsg);
}
#endregion IsRemoveTradeEventMsg
#region IsRemoveTradeMsg
protected static bool IsRemoveTradeMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is RemoveTradeMsg);
}
#endregion IsRemoveTradeMsg

#region IsFxCustomerSettlementMsg
protected static bool IsFxCustomerSettlementMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is CustomerSettlementRateMsg);
}
#endregion IsFxCustomerSettlement

#region IsPayoutMsg
protected static bool IsPayoutMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is PayoutMsg);
}
#endregion IsPayoutMsg
#region IsRebateMsg
protected static bool IsRebateMsg(ActionMsgBase pActionMsg)
{
return (pActionMsg is RebateMsg);
}
#endregion IsRebateMsg

#region RowDetail
public  DataRow RowDetail(int pID)
{
if (null != m_DsEvents.DtEventDet)
{
DataRow[] dataRow = m_DsEvents.DtEventDet.Select("IDE=" + pID.ToString(), "IDE");
if (1 == dataRow.Length)
return dataRow[0];
}
return null;
}
#endregion RowDetail
#region RowEvent
protected DataRow RowEvent(int pIdE)
{
return  m_DsEvents.RowEvent(pIdE);  
}
#endregion RowEvent
#region RowFixing
protected DataRow RowFixing(int pIdE)
{
if (null != DtEvent)
{
DataRow[] rowFixing = DtEventClass.Select(StrFunc.AppendFormat(@"IDE = {0} and EVENTCLASS = '{1}'", pIdE, EventClassFunc.Fixing), "IDE");
if (0 < rowFixing.Length)
return rowFixing[0];
}
return null;
}
#endregion RowFixing
#region RowGroup
protected DataRow RowGroup(int pIdE)
{
if (null != DtEvent)
{
DataRow[] rowGroup = DtEventClass.Select(StrFunc.AppendFormat(@"IDE = {0} and EVENTCLASS = '{1}'", pIdE, EventClassFunc.GroupLevel), "IDE");
if (0 < rowGroup.Length)
return rowGroup[0];
}
return null;
}
#endregion RowGroup
#region RowPreSettlement
protected DataRow RowPreSettlement(int pIdE)
{
if (null != DtEvent)
{
DataRow[] rowPreSettlement = DtEventClass.Select(StrFunc.AppendFormat(@"IDE = {0} and EVENTCLASS = '{1}'", pIdE, EventClassFunc.PreSettlement), "IDE");
if (0 < rowPreSettlement.Length)
return rowPreSettlement[0];
}
return null;
}
#endregion RowPreSettlement
#region RowRecognition
protected DataRow RowRecognition(int pIdE)
{
if (null != DtEvent)
{
DataRow[] rowRecognition = DtEventClass.Select(StrFunc.AppendFormat(@"IDE = {0} and EVENTCLASS = '{1}'", pIdE, EventClassFunc.Recognition), "IDE");
if (0 < rowRecognition.Length)
return rowRecognition[0];
}
return null;
}
#endregion RowRecognition
#region RowSettlement
protected DataRow RowSettlement(int pIdE)
{
if (null != DtEvent)
{
DataRow[] rowSettlement = DtEventClass.Select(StrFunc.AppendFormat(@"IDE = {0} and EVENTCLASS = '{1}'", pIdE, EventClassFunc.Settlement), "IDE");
if (0 < rowSettlement.Length)
return rowSettlement[0];
}
return null;
}
#endregion RowSettlement
#region Update
// 20081107 EG Ticket 16393
// EG 20150317 [POC] Update EVENTDET
protected Cst.ErrLevel Update(IDbTransaction pDbTransaction)
{
Cst.ErrLevel ret = UpdateStUsedByForEndOfTradeAction();
if (Cst.ErrLevel.SUCCESS == ret)
{
m_DsTrade.UpdateTradeStSys(pDbTransaction);
DataTable dtChanges = DtEvent.GetChanges();
m_DsEvents.Update(pDbTransaction);
// EG 20150317 [POC] Update EVENTDET
m_DsEvents.Update(pDbTransaction, m_DsEvents.DtEventDet, Cst.OTCml_TBL.EVENTDET);

if (null != dtChanges)
{
// FI 20200820 [25468] dates systèmes en UTC
DateTime dtSysBusiness = OTCmlHelper.GetDateSysUTC(m_CS);

EventProcess eventProcess = new EventProcess(m_CS); 
foreach (DataRow row in dtChanges.Rows)
{
    if (DataRowState.Deleted != row.RowState)
    {
        int idE = Convert.ToInt32(row["IDE"]);
        eventProcess.Write(pDbTransaction, idE, Cst.ProcessTypeEnum.TRADEACTGEN, ProcessStateTools.StatusSuccessEnum,
            dtSysBusiness, m_TradeActionGenProcess.Tracker.IdTRK_L);
    }
}
}
}
return ret;
}
// EG 20180502 Analyse du code Correction [CA2200]
protected virtual void Update()
{
bool isOk = true;
IDbTransaction dbTransaction = null;
try
{
dbTransaction = DataHelper.BeginTran(m_CS);
Update(dbTransaction);
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
    DataHelper.RollbackTran(dbTransaction);
}
catch (Exception ) { }
}
}
}
#endregion Update
#region UpdateStUsedByForEndOfTradeAction
// EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
protected Cst.ErrLevel UpdateStUsedByForEndOfTradeAction()
{
DataRow rowTradeStSys = DtTrade.Rows[0];
rowTradeStSys.BeginEdit();
rowTradeStSys["IDSTUSEDBY"]  = Cst.StatusActivation.REGULAR.ToString();
rowTradeStSys["IDASTUSEDBY"] = m_TradeActionGenProcess.Session.IdA;
// FI 20200820 [25468] dates systèmes en UTC
rowTradeStSys["DTSTUSEDBY"]  = OTCmlHelper.GetDateSysUTC(m_CS);
//20070801 PL Ticket 15620,15622
rowTradeStSys["LIBSTUSEDBY"] = Convert.DBNull;
rowTradeStSys.EndEdit();
return Cst.ErrLevel.SUCCESS;
}
#endregion UpdateStUsedByForEndOfTradeAction
#endregion Methods
}
#endregion TradeActionGenBase
}

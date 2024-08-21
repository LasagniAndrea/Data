#region Using Directives
using System.Data;
//
using EFS.ACommon;
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
#endregion Using Directives

namespace EFS.Process
{
	#region TradeActionGenTrigger
    public class TradeActionGenProcessTrigger : TradeActionGenProcessBase
    {
        #region Constructors
        // EG 20180502 Analyse du code Correction [CA2214]
        public TradeActionGenProcessTrigger(TradeActionGenProcess pTradeActionGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, TradeActionMQueue pTradeAction)
            : base(pTradeActionGenProcess, pDsTrade, pTradeLibrary, pTradeAction)
		{
			//CodeReturn = Valorize();
		}
		#endregion Constructors
		#region Methods
		#region Valorize
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            #region Barrier/Trigger Event Status Update
            foreach (ActionMsgBase actionMsg in m_TradeAction.ActionMsgs)
            {
                if (IsBarrierOrTriggerMsg(actionMsg))
                {
                    BarrierTriggerMsgBase barrierTriggerMsg = actionMsg as BarrierTriggerMsgBase;

                    // PM 20210121 [XXXXX] Passage du message au niveau de log None
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7230), 0,
                        new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id)),
                        new LogParam(barrierTriggerMsg.status),
                        new LogParam(StrFunc.FmtDecimalToInvariantCulture(barrierTriggerMsg.touchRate)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(barrierTriggerMsg.actionDate))));

                    DataRow rowBarrierTrigger = RowEvent(barrierTriggerMsg.idE);
                    if (null != rowBarrierTrigger)
                    {
                        #region Barrier/Trigger Fixing info for Activate/unactivate
                        DataRow rowFixing = RowFixing(barrierTriggerMsg.idE);
                        DataRow rowDetail = RowDetail(barrierTriggerMsg.idE);
                        if (null == rowFixing)
                        {
                            rowFixing = DtEventClass.NewRow();
                            DtEventClass.Rows.Add(rowFixing);
                        }
                        if (null == rowDetail)
                        {
                            rowDetail = DtEventDet.NewRow();
                            DtEventDet.Rows.Add(rowDetail);
                        }
                        rowBarrierTrigger.BeginEdit();
                        rowBarrierTrigger["IDSTTRIGGER"] = barrierTriggerMsg.status.ToString();
                        rowBarrierTrigger["VALORISATION"] = barrierTriggerMsg.touchRate;
                        rowBarrierTrigger["UNITTYPE"] = UnitTypeEnum.Rate.ToString();
                        rowBarrierTrigger.EndEdit();

                        #region UpdateRow EventDet Payout
                        rowDetail.BeginEdit();
                        rowDetail["IDE"] = barrierTriggerMsg.idE;
                        rowDetail["DTFIXING"] = barrierTriggerMsg.actionDate;
                        rowDetail["NOTE"] = barrierTriggerMsg.note;
                        rowDetail["DTACTION"] = barrierTriggerMsg.actionDate;
                        rowDetail.EndEdit();
                        #endregion UpdateRow EventDet Payout

                        rowFixing.BeginEdit();
                        rowFixing["IDE"] = barrierTriggerMsg.idE;
                        rowFixing["EVENTCLASS"] = EventClassFunc.Fixing;
                        rowFixing["DTEVENT"] = barrierTriggerMsg.actionDate.Date;
                        rowFixing["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, barrierTriggerMsg.actionDate.Date);
                        rowFixing["ISPAYMENT"] = false;
                        rowFixing.EndEdit();

                        #endregion Barrier/Trigger Fixing info for Activate/unactivate
                    }
                }
            }
            #endregion Barrier/Trigger Event Status Update
            Update();
            return ret;
        }	
		#endregion Valorize
		#endregion Methods
	}
	#endregion TradeActionGenTrigger
}

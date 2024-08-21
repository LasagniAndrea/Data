#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Restriction;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Settlement.Message;
//
using FpML.Enum;
#endregion Using Directives

namespace EFS.Process.SettlementMessage
{

    /// <summary>
    ///  Génération des ESR  (Event Settlement Report) : Netting IntraTrade ou pas de netting
    ///  <para>Montant = netting par trade des flux à régler</para>
    /// </summary>
    public class ESRStandardGenProcess : ESRGenProcessBase
    {
        #region Members
        /// <summary>
        /// Message Queue à l'origine
        /// </summary>
        private readonly ESRStandardGenMQueue esrStandardGenMQueue;
        #endregion Members

        #region Accessors
        protected override string DataIdent
        {
            get
            {
                return Cst.OTCml_TBL.TRADE.ToString();
            }
        }
        protected override TypeLockEnum DataTypeLock
        {
            get
            {
                return TypeLockEnum.TRADE;
            }
        }
        #endregion Accessors

        #region Constructor
        public ESRStandardGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            esrStandardGenMQueue = (ESRStandardGenMQueue)pMQueue;
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessInitialize()
        {
            base.ProcessInitialize();
            if (false == IsProcessObserver)
            {
                #region ProcessTuning => Initialisation from Trade
                SQL_TradeCommon sqlTrade = new SQL_TradeCommon(Cs, CurrentId);
                if (sqlTrade.LoadTable(new string[] { "IDT", "IDI" }))
                {
                    ProcessTuning = new ProcessTuning(Cs, sqlTrade.IdI, MQueue.ProcessType, AppInstance.ServiceName, AppInstance.HostName);
                }
                if (false == ProcessTuningSpecified)
                {
                    ProcessTuning = new ProcessTuning(Cs, sqlTrade.IdI, Cst.ProcessTypeEnum.ESRGEN, AppInstance.ServiceName, AppInstance.HostName);
                }
                if (ProcessTuningSpecified)
                {
                    LogDetailEnum = ProcessTuning.LogDetailEnum;

                    
                    Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(LogDetailEnum));
                }
                #endregion ProcessTuning => Initialisation from Trade
            }
        }

        #region ProcessPreExecute
        protected override void ProcessPreExecute()
        {
            base.ProcessPreExecute();

            if (false == IsProcessObserver)
            {
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                {
                    #region LockTrade
                    ProcessState.CodeReturn = LockCurrentObjectId();
                    #endregion
                }
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                {
                    #region Scan compatibility
                    ProcessState.CodeReturn = ScanCompatibility_Trade(CurrentId);
                    #endregion
                }
            }
        }
        #endregion
        #region GetQueryEvent
        public override QueryParameters GetQueryEvent()
        {
            return GetQueryEvent(false);
        }
        public override QueryParameters GetQueryEvent(bool pIsForRestrict)
        {
            //DataParameters
            DataParameters parameters = GetMQueueDataParameters();
            parameters.Add(new DataParameter(Cs, "IDT", DbType.Int32), esrStandardGenMQueue.id);

            //Select
            StrBuilder sqlSelect = new StrBuilder();
            if (pIsForRestrict)
                sqlSelect += SQLCst.SELECT + "e.IDE as ID,e.IDE As IDENTIFIER";
            else
                sqlSelect += SQLCst.SELECT + "e.IDE as IDE,e.IDT as IDT,e.IDT as IDDATA";
            //From
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EVENTESR + " e " + Cst.CrLf;
            //Where
            SQLWhere sqlWhere = GetMqueueSqlWhere();
            sqlWhere.Append("e.IDT=@IDT");

            SQLWhere sqlWhereNetStd = new SQLWhere(sqlWhere.ToString());
            sqlWhereNetStd.Append("e.NETMETHOD='Standard'");
            SQLWhere sqlWhereNetNone = new SQLWhere(sqlWhere.ToString());
            sqlWhereNetNone.Append("e.NETMETHOD='None'");

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += sqlSelect.ToString() + Cst.CrLf + sqlWhereNetStd.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
            sqlQuery += sqlSelect.ToString() + Cst.CrLf + sqlWhereNetNone.ToString() + Cst.CrLf;
            QueryParameters qry = new QueryParameters(Cs, sqlQuery.ToString(), parameters);
            return qry;
        }
        #endregion

        /// <summary>
        /// Requête qui retourne le montant à régler en fonction des évènements retenus
        /// </summary>
        /// <returns></returns>
        public override QueryParameters GetQueryAggregateEvent()
        {
            //Parameters
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "SESSIONID", DbType.AnsiString, SQLCst.UT_SESSIONID_LEN), Session.SessionId);
            //From 
            StrBuilder sqlFrom = new StrBuilder();
            sqlFrom += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EVENTESR + " e " + Cst.CrLf;
            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE + " t on t.IDT = e.IDT " + Cst.CrLf;
            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.SESSIONRESTRICT + " sr on sr.ID=e.IDE and sr.CLASS='EVENT' and sr.SESSIONID=@SESSIONID" + Cst.CrLf;
            /* Requête calcul netting standard */
            StrBuilder sqlSelectNetStd = new StrBuilder(SQLCst.SELECT);
            sqlSelectNetStd += @"e.IDT,e.DTSTM,e.DTSTMFORCED,e.EVENTCLASS," + Cst.CrLf;
            sqlSelectNetStd += @"e.IDA_SENDERPARTY,e.IDA_SENDER,e.IDA_RECEIVER,e.IDA_RECEIVERPARTY," + Cst.CrLf;
            sqlSelectNetStd += @"e.IDA_CSS,e.SCREF,e.IDC," + Cst.CrLf;
            sqlSelectNetStd += @"e.NETMETHOD,0 As IDNETCONVENTION,0 As IDNETDESIGNATION," + Cst.CrLf;
            sqlSelectNetStd += @"abs(SUM(case when PAYER_RECEIVER='Payer' Then -e.AMOUNT Else e.AMOUNT end)) as AMOUNT," + Cst.CrLf;
            sqlSelectNetStd += @"case when SUM(case when PAYER_RECEIVER='Payer' Then -e.AMOUNT Else e.AMOUNT end)<0" + Cst.CrLf;
            sqlSelectNetStd += @"then 'Payer' else 'Receiver' end as PAYER_RECEIVER" + Cst.CrLf;
            sqlSelectNetStd += sqlFrom + Cst.CrLf;

            /* Requête sans netting standard */
            StrBuilder sqlSelectNetNone = new StrBuilder(SQLCst.SELECT);
            // 20090605 EG Add column EVENTCLASS To Select instruction
            sqlSelectNetNone += @"e.IDT,e.DTSTM,e.DTSTMFORCED,e.EVENTCLASS," + Cst.CrLf;
            sqlSelectNetNone += @"e.IDA_SENDERPARTY,e.IDA_SENDER, e.IDA_RECEIVER,e.IDA_RECEIVERPARTY," + Cst.CrLf;
            sqlSelectNetNone += @"e.IDA_CSS,e.SCREF,e.IDC," + Cst.CrLf;
            sqlSelectNetNone += @"e.NETMETHOD,0 As IDNETCONVENTION,0 As IDNETDESIGNATION," + Cst.CrLf;
            sqlSelectNetNone += @"e.AMOUNT," + Cst.CrLf;
            sqlSelectNetNone += @"e.PAYER_RECEIVER as PAYER_RECEIVER" + Cst.CrLf;
            sqlSelectNetNone += sqlFrom + Cst.CrLf;

            SQLWhere sqlWhereNetStd = new SQLWhere();
            sqlWhereNetStd.Append("e.NETMETHOD='Standard'");

            SQLWhere sqlWhereNetNone = new SQLWhere();
            sqlWhereNetNone.Append("e.NETMETHOD='None'");

            StrBuilder sqlGoupByNetStd = new StrBuilder(SQLCst.GROUPBY);
            sqlGoupByNetStd += "e.IDT, e.DTSTM, e.DTSTMFORCED, e.EVENTCLASS, " + Cst.CrLf;
            sqlGoupByNetStd += "e.IDA_SENDERPARTY,e.IDA_SENDER,e.IDA_RECEIVER,e.IDA_RECEIVERPARTY, " + Cst.CrLf;
            sqlGoupByNetStd += "e.IDA_CSS,e.SCREF,e.IDC, " + Cst.CrLf;
            sqlGoupByNetStd += "t.EFSMLVERSION, " + Cst.CrLf;
            sqlGoupByNetStd += "e.NETMETHOD" + Cst.CrLf;

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += sqlSelectNetStd.ToString() + Cst.CrLf + sqlWhereNetStd.ToString() + Cst.CrLf;
            sqlQuery += sqlGoupByNetStd + Cst.CrLf;
            sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
            sqlQuery += sqlSelectNetNone.ToString() + Cst.CrLf + sqlWhereNetNone.ToString() + Cst.CrLf;
            //Order
            sqlQuery += SQLCst.ORDERBY + "DTSTMFORCED,IDT";

            QueryParameters ret = new QueryParameters(Cs, sqlQuery.ToString(), parameters);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessTerminateSpecific()
        {
            SetTradeStatus(CurrentId, ProcessState.Status);
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void AddLogTitleProcess()
        {
            if (ProcessCall == ProcessCallEnum.Master)
            {
                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 3430), 0,
                    new LogParam(DtFunc.DateTimeToStringDateISO(esrStandardGenMQueue.GetMasterDate()))));
            }
        }

        #endregion Methods
    }

}
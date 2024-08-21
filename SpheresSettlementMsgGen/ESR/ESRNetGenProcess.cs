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
    ///  Génération des ESR  (Event Settlement Report) : Netting par Convention
    ///  <para>Montant =  netting par trade de même convention des flux à régler</para>
    /// </summary>
    public class ESRNetGenProcess : ESRGenProcessBase
    {
        #region Members
        private readonly ESRNetGenMQueue esrNetGenMQueue;
        #endregion Members
        #region Accessors
        protected override string DataIdent
        {
            get
            {
                string ret = string.Empty;
                if (NettingMethodEnum.Convention == esrNetGenMQueue.nettingMethod)
                    ret = Cst.OTCml_TBL.NETCONVENTION.ToString();
                else if (NettingMethodEnum.Designation == esrNetGenMQueue.nettingMethod)
                    ret = Cst.OTCml_TBL.NETDESIGNATION.ToString();
                return ret;
            }
        }

        protected override TypeLockEnum DataTypeLock
        {
            get
            {
                TypeLockEnum ret = base.DataTypeLock;
                if (NettingMethodEnum.Convention == esrNetGenMQueue.nettingMethod)
                    ret = TypeLockEnum.NETCONVENTION;
                else if (NettingMethodEnum.Designation == esrNetGenMQueue.nettingMethod)
                    ret = TypeLockEnum.NETDESIGNATION;
                return ret;
            }
        }
        #endregion Accessors
        #region Constructor
        public ESRNetGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            esrNetGenMQueue = (ESRNetGenMQueue)pMQueue;
        }
        #endregion Constructor
        #region Methods
        #region ProcessInitialize
        protected override void ProcessInitialize()
        {
            base.ProcessInitialize();

            if (false == IsProcessObserver)
            {
                ProcessTuning = new ProcessTuning(Cs, 0, MQueue.ProcessType, AppInstance.ServiceName, AppInstance.HostName);
                if (false == ProcessTuningSpecified)
                    ProcessTuning = new ProcessTuning(Cs, 0, Cst.ProcessTypeEnum.ESRGEN, AppInstance.ServiceName, AppInstance.HostName);
                
                if (ProcessTuningSpecified)
                {
                    LogDetailEnum = ProcessTuning.LogDetailEnum;

                    
                    Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(LogDetailEnum));
                }
            }
        }
        #endregion ProcessInitialize
        #region ProcessPreExecute
        protected override void ProcessPreExecute()
        {
            base.ProcessPreExecute();
            if (false == IsProcessObserver)
            {
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                {
                    #region Lock NETCONVENTION or NETCONSIGNATION
                    //Pour eviter qu'un autre process Traite ce netting 
                    //(même si le perimètre des différent ex Date différente)
                    ProcessState.CodeReturn = LockCurrentObjectId();
                    #endregion Lock NETCONVENTION or NETCONSIGNATION
                }
                //
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                {
                    Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
                    int[] idT = GetIdT();
                    for (int i = 0; i < ArrFunc.Count(idT); i++)
                    {
                        #region Lock Trade
                        LockObject lockObject = new LockObject(TypeLockEnum.TRADE, idT[i], "Trade " + idT[i].ToString(), LockTools.Exclusive);
                        if (null != lockObject)
                        {
                            Lock lck = new Lock(Cs, lockObject, Session, MQueue.ProcessType.ToString());
                            if (false == LockTools.LockMode2(lck, out _))
                            {
                                errLevel = Cst.ErrLevel.LOCKUNSUCCESSFUL;
                            }
                        }
                        #endregion

                        #region Verify Trade Compatibility
                        if (ProcessStateTools.IsCodeReturnSuccess(errLevel))
                            errLevel = ScanCompatibility_Trade(idT[i]);
                        #endregion Verify Trade Compatibility

                        if (Cst.ErrLevel.SUCCESS != errLevel)
                            break;
                    }
                    ProcessState.CodeReturn = errLevel;
                }
            }
        }
        #endregion ProcessPreExecute
        #region GetQueryEvent
        public override QueryParameters GetQueryEvent()
        {
            return GetQueryEvent(false);
        }
        public override QueryParameters GetQueryEvent(bool pIsForRestrict)
        {
            //DataParameters
            DataParameters parameters = GetMQueueDataParameters();
            parameters.Add(new DataParameter(Cs, "NETMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), esrNetGenMQueue.nettingMethod.ToString());
            if (esrNetGenMQueue.idSpecified)
                parameters.Add(new DataParameter(Cs, "IDNET", DbType.Int32), esrNetGenMQueue.id);
            if (esrNetGenMQueue.identifierSpecified)
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDENTIFIER), esrNetGenMQueue.identifier);
            parameters.Add(new DataParameter(Cs, "NETMETHODSTD", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), NettingMethodEnum.Standard.ToString());
            parameters.Add(new DataParameter(Cs, "NETMETHODNONE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), NettingMethodEnum.None.ToString());

            //Select
            string columnIdNet = (NettingMethodEnum.Convention == esrNetGenMQueue.nettingMethod) ? "e.IDNETCONVENTION" : "e.IDNETDESIGNATION";
            string columnNetIdentifier = (NettingMethodEnum.Convention == esrNetGenMQueue.nettingMethod) ? "conv.IDENTIFIER" : "des.IDENTIFIER";

            StrBuilder sqlSelect = new StrBuilder();
            if (pIsForRestrict)
                sqlSelect += SQLCst.SELECT + "e.IDE As ID,e.IDE As IDENTIFIER";
            else
                sqlSelect += SQLCst.SELECT + "e.IDE,e.IDT," + columnIdNet + " as IDDATA";

            //From
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EVENTESR.ToString() + " e " + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.NETCONVENTION.ToString() + " conv on conv.IDNETCONVENTION= e.IDNETCONVENTION" + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.NETDESIGNATION.ToString() + " des on des.IDNETDESIGNATION= e.IDNETDESIGNATION" + Cst.CrLf;

            //Where
            SQLWhere sqlWhere = GetMqueueSqlWhere();
            sqlWhere.Append("e.NETMETHOD!=@NETMETHODSTD And e.NETMETHOD!=@NETMETHODNONE");

            if (parameters.Contains("IDNET"))
                sqlWhere.Append(columnIdNet + "=@IDNET");
            else if (parameters.Contains("IDENTIFIER"))
                sqlWhere.Append(columnNetIdentifier + "=@IDENTIFIER");
            //
            if (parameters.Contains("NETMETHOD"))
                sqlWhere.Append("e.NETMETHOD=@NETMETHOD");

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += sqlSelect.ToString() + Cst.CrLf + sqlWhere.ToString() + Cst.CrLf;
            QueryParameters ret = new QueryParameters(Cs, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryEvent
        #region GetQueryAggregateEvent
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override QueryParameters GetQueryAggregateEvent()
        {
            //Parameters
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "SESSIONID", DbType.AnsiString, SQLCst.UT_SESSIONID_LEN), Session.SessionId);

            string sqlSelect = @"select 0 as IDT, e.DTSTM,e.DTSTMFORCED, e.EVENTCLASS, e.IDA_SENDERPARTY, e.IDA_SENDER, e.IDA_RECEIVER, e.IDA_RECEIVERPARTY,
            e.IDA_CSS, e.SCREF, e.IDC, e.NETMETHOD, isnull(e.IDNETCONVENTION,0) as IDNETCONVENTION, isnull(e.IDNETDESIGNATION,0) as IDNETDESIGNATION, 
            abs(SUM(case when PAYER_RECEIVER='Payer' Then -e.AMOUNT Else e.AMOUNT end)) as AMOUNT,
            case when SUM(case when PAYER_RECEIVER='Payer' Then -e.AMOUNT Else e.AMOUNT end)<0
            then 'Payer' else 'Receiver' end as PAYER_RECEIVER
            from dbo.VW_EVENTESR e
            inner join TRADEXML t on (t.IDT = e.IDT)
            inner join SESSIONRESTRICT sr on (sr.ID = e.IDE) and (sr.CLASS = 'EVENT') and (sr.SESSIONID = @SESSIONID)
            group by e.DTSTM, e.DTSTMFORCED, e.EVENTCLASS, e.IDA_SENDERPARTY, e.IDA_SENDER, e.IDA_RECEIVER, e.IDA_RECEIVERPARTY,
            e.IDA_CSS, e.SCREF, e.IDC, t.EFSMLVERSION, e.NETMETHOD, isnull(e.IDNETCONVENTION,0), isnull(e.IDNETDESIGNATION,0)
             order by e.DTSTMFORCED,e.NETMETHOD" + Cst.CrLf;

            QueryParameters ret = new QueryParameters(Cs, sqlSelect, parameters);
            return ret;
        }
        #endregion GetQueryAggregateEvent
        #region ProcessTerminateSpecific
        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessTerminateSpecific()
        {
            //Ici mqueue.Id est forcément renseigné
            //Retourne les trades considérés par le netting en cours
            int[] idT = GetIdT();
            for (int i = 0; i < ArrFunc.Count(idT); i++)
                SetTradeStatus(idT[i], ProcessState.Status);
        }
        #endregion ProcessTerminateSpecific
        #region GetIdT
        //
        /// <summary>
        /// Retourne la liste des trades considérés par le traitement
        /// <para>prise en considération des paramètres existants dans le message Mqueue</para>
        /// </summary>
        /// 
        /// <returns></returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        private int[] GetIdT()
        {
            int[] ret = null;
            QueryParameters queryParameters = GetQueryEvent();
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT_DISTINCT + @"e.IDT As IDT" + Cst.CrLf);
            sqlSelect += SQLCst.X_FROM + Cst.CrLf;
            sqlSelect += "(" + queryParameters.Query + ") e";

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, sqlSelect.ToString(), queryParameters.Parameters.GetArrayDbParameter()))
            {
                ArrayList al = new ArrayList();
                while (dr.Read())
                    al.Add(Convert.ToInt32(dr.GetValue(0)));
                ret = (int[])al.ToArray(typeof(int));
            }
            return ret;
        }
        #endregion GetIdT

        /// <summary>
        /// 
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void AddLogTitleProcess()
        {
            if (ProcessCall == ProcessCallEnum.Master)
            {
                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 3431), 0,
                    new LogParam(DtFunc.DateTimeToStringDateISO(esrNetGenMQueue.GetMasterDate()))));
            }
        }
        #endregion Methods
    }
}

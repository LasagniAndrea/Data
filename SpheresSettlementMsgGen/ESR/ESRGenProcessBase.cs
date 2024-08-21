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
    ///  Génération des ESR (Event Settlement Report == (Netting IntraTrade ou Netting par Convention))
    /// </summary>
    public abstract class ESRGenProcessBase : ProcessBase
    {
        #region Members
        /// <summary>
        /// Msq à l'origine de la demande de traitement
        /// </summary>
        private readonly ESRGenMQueueBase esrGenMQueue;
        /// <summary>
        /// 
        /// </summary>
        private RestrictionElement restrictionEvent;
        #endregion Members
        #region Constructor
        public ESRGenProcessBase(MQueueBase pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            esrGenMQueue = (ESRGenMQueueBase)pMQueue;
        }
        #endregion Constructor
        #region Methods
        #region SelectDatas
        protected override void SelectDatas()
        {
            QueryParameters queryParamers = GetQueryEvent();
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT_DISTINCT + @"e.IDDATA As IDDATA" + Cst.CrLf);
            sqlSelect += SQLCst.X_FROM + Cst.CrLf;
            sqlSelect += "(" + queryParamers.Query + ") e";
            DsDatas = DataHelper.ExecuteDataset(Cs, CommandType.Text, sqlSelect.ToString(), queryParamers.Parameters.GetArrayDbParameter());
        }
        #endregion SelectDatas
        #region  ProcessExecuteSpecific
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            AddLogTitleProcess();

            //Recherche d'évènements 
            Cst.ErrLevel codeReturn = SetRestrictionEvent();
            if (codeReturn == Cst.ErrLevel.SUCCESS)
            {
                //Alimentation
                SetESR();
            }

            return codeReturn;
        }
        #endregion ProcessExecuteSpecific
        #region GetQueryAggregateEvent
        public virtual QueryParameters GetQueryAggregateEvent()
        {
            return null;
        }
        #endregion
        #region GetQueryEvent
        public virtual QueryParameters GetQueryEvent()
        {
            return null;
        }
        public virtual QueryParameters GetQueryEvent(bool pIsForRestrict)
        {
            return null;
        }
        #endregion GetQueryEvent
        #region GetMQueueDataParameters
        protected DataParameters GetMQueueDataParameters()
        {
            DataParameters ret = new DataParameters();
            if (esrGenMQueue.parametersSpecified)
            {
                if (null != esrGenMQueue.GetObjectValueParameterById(ESRGenMQueueBase.PARAM_DATE1))
                {
                    ret.Add(new DataParameter(Cs, "DATE1", DbType.Date),
                        Convert.ToDateTime(esrGenMQueue.GetDateTimeValueParameterById(ESRGenMQueueBase.PARAM_DATE1)));
                }
            }
            return ret;
        }
        #endregion
        #region GetMqueueSqlWhere
        protected SQLWhere GetMqueueSqlWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();
            DataParameters parameters = GetMQueueDataParameters();
            if (parameters.Contains(ESRGenMQueueBase.PARAM_DATE1))
                sqlWhere.Append("e.DTSTMFORCED=@DATE1");
            return sqlWhere;
        }
        #endregion GetMqueueSqlWhere
        #region IsEventEnabled
        public bool IsEventEnabled(int pIdE)
        {
            bool ret = true;
            if (Cst.ErrLevel.SUCCESS != ScanCompatibility_Event(pIdE))
                ret = false;
            if (ret)
                ret = (false == SettlementMessageTools.IsEventUseByMSO(Cs, pIdE));
            return ret;
        }
        #endregion IsEventEnabled
        #region DeleteESR
        /// <summary>
        /// Suppression des ESRs qui portent sur les évènements retenus par le Traitement
        /// Rappel Ces ESRs n'ont pas été utilisés par un message (MSO)
        /// </summary>
        private void DeleteESR(IDbTransaction pDbTransaction)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.SESSIONID), Session.SessionId);
            
            StrBuilder query = new StrBuilder();
            // Syntaxe Plus Rapide
            query += SQLCst.DELETE_DBO + "ESR" + Cst.CrLf;
            query += SQLCst.WHERE + SQLCst.EXISTS + Cst.CrLf;
            query += "(select 1 " + SQLCst.FROM_DBO + "ESRDET esrdet" + Cst.CrLf;
            query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.SESSIONRESTRICT.ToString() + " r " + Cst.CrLf;
            query += "on r.ID=esrdet.IDE And r.SESSIONID=@SESSIONID And r.CLASS='EVENT'" + Cst.CrLf;
            query += SQLCst.WHERE + "esrdet.IDESR=ESR.IDESR)";
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, query.ToString(), dataParameters.GetArrayDbParameter());
        }
        #endregion DeleteESR
        #region SetESR
        /// <summary>
        /// Alimentation de la table ESR à partir des évènements retenus (voir restrictionEvent)
        /// Les évènements retenus sont sont ceux issus de la vue VW_EVENTESR après application des restrictions (paramètres Mqueue)
        /// Les évènements doivent être compatibles vis à vis de processTuning
        /// Les évènements ne doivent pas être impliqués dans un ESR lui même impliqué dans un message (1 MSO)
        /// </summary>
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void SetESR()
        {
            IDbTransaction dbTransaction = null;
            bool isOk = true;
            try
            {
                DataSetEventTrade dsEvent = null;
                if (ProcessTuningSpecified)
                {
                    int[] idE = restrictionEvent.GetIdEnabled();
                    if (ArrFunc.IsFilled(idE))
                    {
                        // EG 20150612 [20665] Chargement direct
                        dsEvent = new DataSetEventTrade(Cs, idE);
                    }
                }
                dbTransaction = DataHelper.BeginTran(Cs);

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Step 1 : Delete Existing ESR"));
                DeleteESR(dbTransaction);

                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Step 2 : Add ESR"));
                ArrayList lds = new ArrayList();
                DatasetSettlementMessage ds = null;
                DateTime dtCurrent = DateTime.MinValue;

                QueryParameters queryParameters = GetQueryAggregateEvent();
                using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
                {
                    while (dr.Read())
                    {
                        bool isNewDate = (null == ds) || (dtCurrent != dr.GetDateTime(dr.GetOrdinal("DTSTMFORCED")));
                        dtCurrent = dr.GetDateTime(dr.GetOrdinal("DTSTMFORCED"));
                        PayerReceiverEnum side = (PayerReceiverEnum)Enum.Parse(typeof(PayerReceiverEnum), Convert.ToString(dr["PAYER_RECEIVER"]), true);
                        if (isNewDate)
                        {
                            ds = new DatasetSettlementMessage(Cs, dtCurrent, LoadDataSetSettlementMessage.LoadESR, IsModeSimul);
                            ds.LoadDs(dbTransaction);
                            lds.Add(ds);
                        }
                        // AddRowESR
                        EventSettlementReportKey esrKey = new EventSettlementReportKey(dr);
                        //
                        if (Convert.ToDecimal(dr["AMOUNT"]) > 0)
                        {
                            ds.AddRowESR(esrKey, side, Convert.ToDecimal(dr["AMOUNT"]), Session.IdA);
                        }
                        else
                        {
                            Logger.Log(new LoggerData(LogLevelEnum.Debug, "ESRGenProcessBase.SetESR", 0,
                                new LogParam(" Amount is null For Key " + Cst.CrLf + esrKey.ToString())));
                        }
                    }
                }

                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Step 3 : Add ESRDET"));

                DatasetSettlementMessage[] ads = (DatasetSettlementMessage[])lds.ToArray(typeof(DatasetSettlementMessage));
                for (int i = 0; i < ArrFunc.Count(ads); i++)
                {
                    ads[i].SetIdESR(dbTransaction);
                    DataTable dt = ads[i].DtESR.GetChanges(DataRowState.Added);
                    if (null != dt)
                    {
                        EventProcess eventProcess = new EventProcess(Cs);
                        foreach (DataRow row in dt.Rows)
                        {
                            queryParameters = GetQueryAggregateDetEvent(new EventSettlementReportKey(row));
                            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
                            {
                                while (dr.Read())
                                {
                                    int idE = Convert.ToInt32(dr["IDE"]);
                                    ads[i].AddRowESRDET(Convert.ToInt32(row["ID"]), Convert.ToInt32(dr["IDE"]), Session.IdA);
                                    // FI 20200820 [25468] dates systèmes en UTC
                                    eventProcess.Write(dbTransaction, idE, MQueue.ProcessType, ProcessStateTools.StatusSuccessEnum, OTCmlHelper.GetDateSysUTC(Cs), Tracker.IdTRK_L);
                                    if (null != dsEvent)
                                    {
                                        // FI 20200820 [25468] dates systèmes en UTC
                                        dsEvent.SetEventStatus(idE, ProcessTuning.GetProcessTuningOutput(TuningOutputTypeEnum.OES), Session.IdA, OTCmlHelper.GetDateSysUTC(Cs));
                                    }
                                }
                            }
                        }
                    }
                    ads[i].UpdateESR(dbTransaction);
                    ads[i].UpdateESRDET(dbTransaction);
                }
                if (null != dsEvent)
                {
                    dsEvent.Update(dbTransaction);
                }
                //PL 20151229 Use DataHelper.CommitTran()
                //dbTransaction. Commit();
                DataHelper.CommitTran(dbTransaction);
            }
            catch (SpheresException2) { isOk = false; throw; }
            catch (Exception ex)
            {
                isOk = false;
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
            }
            finally
            {
                if (null != dbTransaction)
                {
                    if (false == isOk)
                    {
                        //PL 20151229 Use DataHelper.RollbackTran()
                        //dbTransaction. Rollback();
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    dbTransaction.Dispose();
                }
            }
        }
        #endregion SetESR
        #region GetQueryAggregateDetEvent
        private QueryParameters GetQueryAggregateDetEvent(EventSettlementReportKey pESR)
        {
            //Parameters
            DataParameters parameters = new DataParameters();
            if (pESR.idT > 0)
                parameters.Add(new DataParameter(Cs, "IDT", DbType.Int32), pESR.idT);
            parameters.Add(new DataParameter(Cs, "DTSTM", DbType.Date), pESR.dtSTM); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(Cs, "DTSTMFORCED", DbType.Date), pESR.dtSTMForced); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(Cs, "IDA_SENDERPARTY", DbType.Int32), pESR.idASenderParty);
            parameters.Add(new DataParameter(Cs, "IDA_SENDER", DbType.Int32), pESR.idASender);
            parameters.Add(new DataParameter(Cs, "IDA_RECEIVER", DbType.Int32), pESR.idAReceiver);
            parameters.Add(new DataParameter(Cs, "IDA_RECEIVERPARTY", DbType.Int32), pESR.idAReceiverParty);
            parameters.Add(new DataParameter(Cs, "IDA_CSS", DbType.Int32), pESR.idACss);
            parameters.Add(new DataParameter(Cs, "SCREF", DbType.AnsiString, 1024), pESR.settlementChainRef);
            parameters.Add(new DataParameter(Cs, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN), pESR.idC);
            parameters.Add(new DataParameter(Cs, "NETMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pESR.netMethod);
            parameters.Add(new DataParameter(Cs, "IDNETCONVENTION", DbType.Int32), pESR.idNetConvention);
            parameters.Add(new DataParameter(Cs, "IDNETDESIGNATION", DbType.Int32), pESR.idNetDesignation);
            parameters.Add(new DataParameter(Cs, "SESSIONID", DbType.AnsiString, SQLCst.UT_SESSIONID_LEN), Session.SessionId);
            
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "e.IDE";
            // From 
            StrBuilder sqlFrom = new StrBuilder();
            sqlFrom += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EVENTESR + " e " + Cst.CrLf;
            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.SESSIONRESTRICT + " sr on sr.ID=e.IDE and sr.CLASS='EVENT' and sr.SESSIONID=@SESSIONID" + Cst.CrLf;

            SQLWhere sqlWhere = new SQLWhere();
            if (parameters.Contains("IDT"))
                sqlWhere.Append("e.IDT=@IDT");
            sqlWhere.Append("e.DTSTM=@DTSTM");
            sqlWhere.Append("e.DTSTMFORCED=@DTSTMFORCED");
            sqlWhere.Append("e.IDA_SENDERPARTY=@IDA_SENDERPARTY");
            sqlWhere.Append("e.IDA_SENDER=@IDA_SENDER");
            sqlWhere.Append("e.IDA_RECEIVER=@IDA_RECEIVER");
            sqlWhere.Append("e.IDA_RECEIVERPARTY=@IDA_RECEIVERPARTY");
            sqlWhere.Append("e.IDA_CSS=@IDA_CSS");
            sqlWhere.Append("e.SCREF=@SCREF");
            sqlWhere.Append("e.IDC=@IDC");
            sqlWhere.Append("e.NETMETHOD=@NETMETHOD");
            sqlWhere.Append(DataHelper.SQLIsNull(Cs, "e.IDNETCONVENTION", "0") + "=@IDNETCONVENTION");
            sqlWhere.Append(DataHelper.SQLIsNull(Cs, "e.IDNETDESIGNATION", "0") + "=@IDNETDESIGNATION");

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += sqlSelect.ToString() + Cst.CrLf + sqlFrom + Cst.CrLf + sqlWhere.ToString() + Cst.CrLf;
            QueryParameters ret = new QueryParameters(Cs, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryAggregateDetEvent
        #region SetRestrictionEvent
        /// <summary>
        /// Recherche des évènements considérés par le Process (Prise en compte des paramètres du Mqueue)
        /// Alimentation de la Table SESSIONRESTRICT => Sauvegarde SQL des Ide retenus pour les futurs Queries du traitement
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel SetRestrictionEvent()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            Logger.Log(new LoggerData(LogLevelEnum.Debug, "Step 0 : Events Selection"));

            //Add Restriction on EVENT 
            ESRProcessEventRestriction processEventRestriction = new ESRProcessEventRestriction(this);
            restrictionEvent = new RestrictionElement(processEventRestriction);
            restrictionEvent.Initialize(this.Cs);
            RestrictionItem[] item = restrictionEvent.GetRestrictItem();
            if (ArrFunc.IsFilled(item))
            {
                for (int i = 0; i < item.Length; i++)
                {
                    if (false == item[i].IsEnabled)
                    {
                        string msg = StrFunc.AppendFormat(
                            "EVENT (Id:{0}, Identifier:{1}) is not considered because is not compliant with ProcessTuning Or because it is already used in a message",
                            item[i].id.ToString(), item[i].identifier);
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, msg, 0,
                            new LogParam(Convert.ToInt32(item[i].id), default, "EVENT", Cst.LoggerParameterLink.IDDATA)));
                    }
                }
            }

            if (ArrFunc.IsEmpty(restrictionEvent.GetRestrictItemEnabled()))
            {
                Logger.Log(new LoggerData(LogLevelEnum.Info, "No EVENT is considered"));
                ret = Cst.ErrLevel.NOTHINGTODO;
            }
            else
            {
                SqlSessionRestrict.SetRestrictUseSelectUnion(restrictionEvent);
            }

            return ret;
        }
        #endregion SetRestrictionEvent

        /// <summary>
        ///  Génération d'un message dans le log qui indique le démarrage du process
        /// </summary>
        /// <returns></returns>
        protected virtual void AddLogTitleProcess()
        {
            throw new NotImplementedException("Override of AddLogTitleProcess is mandatory");
        }


        #endregion Methods
    }
}

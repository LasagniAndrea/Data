using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Acknowledgment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace EFS.Process
{
    #region TrackerBase
    // EG 20190315 New version of Naming IRQ Semaphore IRQ{IDTRK_L}{SERVERNAME}{DATABASENAME}
    // EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
    public class TrackerBase : DatetimeProfiler
    {
        #region Members

        public string CS { set; get; }
        /// <summary>
        /// process demandé (process Maître => celui qui alimente le Tracker)
        /// </summary>
        /// FI 20201106 [XXXXX] Add
        public Cst.ProcessTypeEnum ProcessRequested { set; get; }
        /// <summary>
        /// Id Tracker (Id de la demande)
        /// </summary>
        public int IdTRK_L { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public Request Request
        {
            get;
            protected set;
        }

        /// <summary>
        /// idLogProcess (issu de PROCESS_L) du process demandé
        /// <para></para>
        /// </summary>
        public int IdProcess { set; get; }

        public ProcessStateTools.ReadyStateEnum ReadyState { set; get; }
        public ProcessStateTools.StatusEnum Status { set; get; }
        public Cst.GroupTrackerEnum Group { set; get; }
        public string ExtlLink { set; get; }
        public string RowAttribut { set; get; }
        #endregion Members

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// EG 20190315 New version of Naming IRQ Semaphore
        /// EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
        /// FI 20200817 [XXXXX] Usage de OTCmlHelper.GetDateSysUTC(pCs)
        public TrackerBase(string pCS)
            : base(OTCmlHelper.GetDateSysUTC(pCS))
        {
            CS = pCS;
            ExtlLink = string.Empty;
            RowAttribut = Cst.RowAttribut_Protected;
        }
        #endregion Constructors
    }
    #endregion TrackerBase

    #region Tracker
    [Serializable]
    // EG 20180525 [23979] IRQ Processing
    public class Tracker : TrackerBase
    {
        #region Members
        // EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
        private string _irqProcess;

        // EG 20190315 New version of Naming IRQ Semaphore
        public string IRQDatabase { get; set; }

        // EG 20180525 [23979] IRQ Processing
        public Nullable<int> IrqIdTRK_L
        {
            set;
            get;
        }
        // EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
        // EG 20190325 Test (null != trackerSystemMsg)
        public string IrqProcess
        {
            get
            {
                Pair<string, Cst.TrackerSystemMsgAttribute> trackerSystemMsg;
                if (StrFunc.IsEmpty(_irqProcess))
                {
                    if (null != Data)
                    {
                        trackerSystemMsg = IRQTools.GetTrackerMsgAttribute(Group, Data.sysNumber);
                        if (null != trackerSystemMsg)
                            _irqProcess = trackerSystemMsg.First;
                    }
                }
                return _irqProcess;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public IdData IdData
        {
            set;
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public TrackerData Data
        {
            set;
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public TrackerAcknowledgmentInfo Ack
        {
            get;
            set;
        }
        #endregion Members

        #region Constructors
        public Tracker(string pCS)
            : base(pCS)
        {
            IRQDatabase = IRQTools.IRQDatabase(CS);
            Ack = new TrackerAcknowledgmentInfo();
        }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Ajoute une nouvelle demande de traitement dans TRACKER_L
        /// </summary>
        public void Insert(AppSession pSession, int pPostedMsg)
        {
            QueryParameters qryParameters = new TrackerQuery(CS).GetQueryInsert();
            DataParameters dataParameters = qryParameters.Parameters;

            DateTime dtIns = GetDate();
            Request = new Request(dtIns, pSession, pPostedMsg);

            using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS))
            {
                try
                {

                    SQLUP.GetId(out int _idTRK_L, dbTransaction, SQLUP.IdGetId.TRACKER_L);
                    IdTRK_L = _idTRK_L;

                    dataParameters["IDTRK_L"].Value = IdTRK_L;
                    dataParameters["GROUPTRACKER"].Value = Group.ToString();
                    dataParameters["READYSTATE"].Value = ReadyState.ToString();
                    dataParameters["STATUSTRACKER"].Value = Status.ToString();

                    dataParameters["IDDATAIDENT"].Value = StrFunc.IsFilled(IdData.idIdent) ? IdData.idIdent : Convert.DBNull;
                    dataParameters["IDDATA"].Value = IdData.id == 0 ? Convert.DBNull : IdData.id;
                    dataParameters["IDDATAIDENTIFIER"].Value = StrFunc.IsFilled(IdData.idIdentifier) ? IdData.idIdentifier : Convert.DBNull;

                    dataParameters["SYSCODE"].Value = Data.sysCode;
                    dataParameters["SYSNUMBER"].Value = Data.sysNumber;
                    dataParameters["DATA1"].Value = Data.data1;
                    dataParameters["DATA2"].Value = Data.data2;
                    dataParameters["DATA3"].Value = Data.data3;
                    dataParameters["DATA4"].Value = Data.data4;
                    dataParameters["DATA5"].Value = Data.data5;

                    dataParameters["IDAINS"].Value = Request.Session.IdA;
                    // FI 20200814 [XXXXX] Alientation avec dStart. dStart est également utilisée pour alimenter TrackerRequest et MqueueRequest
                    dataParameters["DTINS"].Value = dtIns;
                    dataParameters["HOSTNAMEINS"].Value = Request.Session.AppInstance.HostName;
                    dataParameters["IDSESSIONINS"].Value = Request.Session.SessionId;
                    dataParameters["POSTEDMSG"].Value = Request.PostedMsg;
                    dataParameters["NONEMSG"].Value = 0; // FI 20201030 [25537]
                    dataParameters["SUCCESSMSG"].Value = 0;
                    dataParameters["ERRORMSG"].Value = 0;
                    dataParameters["WARNINGMSG"].Value = 0;
                    dataParameters["POSTEDSUBMSG"].Value = 0;

                    dataParameters["EXTLID"].Value = StrFunc.IsEmpty(Ack.extlId) ? Convert.DBNull : Ack.extlId;
                    dataParameters["ACKXML"].Value = (null == Ack.schedules) ? Convert.DBNull : SerializeTrackerAck(Ack);
                    dataParameters["ACKSTATUS"].Value = (null == Ack.schedules) ? Convert.DBNull : ProcessStateTools.StatusUnknown;
                    dataParameters["IRQIDTRK_L"].Value = Convert.DBNull;

                    dataParameters["EXTLLINK"].Value = ExtlLink;
                    dataParameters["ROWATTRIBUT"].Value = RowAttribut;
                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, dataParameters.GetArrayDbParameter());
                    DataHelper.CommitTran(dbTransaction);
                }
                catch (Exception)
                {
                    DataHelper.RollbackTran(dbTransaction);
                    throw;
                }
            }
        }

        /// <summary>
        /// chargement du tracker à partir de idTRK_L 
        /// </summary>
        ///  EG 20180425 Analyse du code Correction [CA2202]
        /// EG 20180525 [23979] IRQ Processing
        public void Select()
        {
            TrackerQuery trackerqry = new TrackerQuery(this.CS);
            QueryParameters qryParameters = trackerqry.GetQuerySelect();
            DataParameters dataParameters = qryParameters.Parameters;
            dataParameters["IDTRK_L"].Value = IdTRK_L;

            using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, dataParameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["GROUPTRACKER"]))
                    {
                        string enumValue = Convert.ToString(dr["GROUPTRACKER"]);
                        if (Enum.IsDefined(typeof(Cst.GroupTrackerEnum), enumValue))
                            Group = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), enumValue, true);
                    }
                    if (false == Convert.IsDBNull(dr["READYSTATE"]))
                        ReadyState = ProcessStateTools.ParseReadyState(Convert.ToString(dr["READYSTATE"]));
                    if (false == Convert.IsDBNull(dr["STATUSTRACKER"]))
                        Status = ProcessStateTools.ParseStatus(Convert.ToString(dr["STATUSTRACKER"]));
                    if (false == Convert.IsDBNull(dr["EXTLLINK"]))
                        ExtlLink = Convert.ToString(dr["EXTLLINK"]);

                    AppInstance appInstance = new AppInstance(dr["HOSTNAMEINS"].ToString(), "N/A", "N/A");
                    AppSession session = new AppSession(appInstance)  
                    {
                        IdA = Convert.ToInt32(dr["IDAINS"]),
                        SessionId = Convert.ToString(dr["IDSESSIONINS"])
                    };
                    if (false == Convert.IsDBNull(dr["IDAINS_IDENTIFIER"]))
                        session.IdA_Identifier = dr["IDAINS_IDENTIFIER"].ToString();

                    if (false == Convert.IsDBNull(dr["SYSNUMBER"]))
                    {
                        IdData = new IdData();
                        if (false == Convert.IsDBNull(dr["IDDATA"]))
                            IdData.id = Convert.ToInt32(dr["IDDATA"]);
                        if (false == Convert.IsDBNull(dr["IDDATAIDENT"]))
                            IdData.idIdent = Convert.ToString(dr["IDDATAIDENT"]);
                        if (false == Convert.IsDBNull(dr["IDDATAIDENTIFIER"]))
                            IdData.idIdentifier = Convert.ToString(dr["IDDATAIDENTIFIER"]);

                        Data = new TrackerData
                        {
                            sysNumber = Convert.ToInt32(dr["SYSNUMBER"])
                        };

                        if (false == Convert.IsDBNull(dr["SYSCODE"]))
                            Data.sysCode = Convert.ToString(dr["SYSCODE"]);
                        if (false == Convert.IsDBNull(dr["DATA1"]))
                            Data.data1 = Convert.ToString(dr["DATA1"]);
                        if (false == Convert.IsDBNull(dr["DATA2"]))
                            Data.data2 = Convert.ToString(dr["DATA2"]);
                        if (false == Convert.IsDBNull(dr["DATA3"]))
                            Data.data3 = Convert.ToString(dr["DATA3"]);
                        if (false == Convert.IsDBNull(dr["DATA4"]))
                            Data.data4 = Convert.ToString(dr["DATA4"]);
                        if (false == Convert.IsDBNull(dr["DATA5"]))
                            Data.data5 = Convert.ToString(dr["DATA5"]);
                    }
                    // FI 20200820 [25468] DTINS étant une date UTC Appel à la méthode DateTime.SpecifyKind
                    Request = new Request(DateTime.SpecifyKind(Convert.ToDateTime(dr["DTINS"]), DateTimeKind.Utc), session, Convert.ToInt32(dr["POSTEDMSG"]));

                    Ack = new TrackerAcknowledgmentInfo();
                    if (false == Convert.IsDBNull(dr["ACKXML"]))
                        Ack = DeSerializeTrackerAck(dr["ACKXML"].ToString());
                    if (false == Convert.IsDBNull(dr["EXTLID"]))
                        Ack.extlId = dr["EXTLID"].ToString();
                }
                else
                {
                    // FI 20201013 [XXXXX] Add
                    throw new InvalidProgramException($"Traker (Id:{IdTRK_L}) not found");
                }
            }
        }

        /// <summary>
        /// Mise à jour de POSTEDSUBMSG (POSTEDSUBMSG = POSTEDSUBMSG + <paramref name="pPostedSubMsg"/>)
        /// </summary>
        /// <param name="pPostedSubMsg"></param>
        /// <param name="pSession"></param>
        public void AddPostedSubMsg(int pPostedSubMsg, AppSession pSession)
        {

            TrackerQuery trackerqry = new TrackerQuery(CS);

            QueryParameters qryParameters = trackerqry.GetQueryAddPostedSubMsg();
            DataParameters dataParameters = qryParameters.Parameters;

            dataParameters["IDAUPD"].Value = pSession.IdA;
            dataParameters["DTUPD"].Value = GetDate();
            dataParameters["HOSTNAMEUPD"].Value = pSession.AppInstance.HostName;
            dataParameters["IDSESSIONUPD"].Value = pSession.SessionId;
            dataParameters["IDTRK_L"].Value = IdTRK_L;
            dataParameters["POSTEDSUBMSG"].Value = pPostedSubMsg;

            DataHelper.ExecuteNonQuery(qryParameters.Cs, CommandType.Text, qryParameters.Query, dataParameters.GetArrayDbParameter());

        }

        /// <summary>
        /// Mise à jour de GROUPTRACKER, READYSTATE, STATUSTRACKER
        /// </summary>
        /// <param name="appInstance"></param>
        public void Update(AppSession pSession)
        {
            TrackerQuery trackerqry = new TrackerQuery(CS);
            QueryParameters qryParameters = trackerqry.GetQueryUpdate();

            DataParameters dataParameters = qryParameters.Parameters;
            dataParameters["IDTRK_L"].Value = IdTRK_L;
            dataParameters["GROUPTRACKER"].Value = Group.ToString();
            dataParameters["READYSTATE"].Value = ReadyState.ToString();
            dataParameters["STATUSTRACKER"].Value = Status.ToString();

            // User UPD
            dataParameters["IDAUPD"].Value = pSession.IdA;
            dataParameters["DTUPD"].Value = GetDate();
            dataParameters["HOSTNAMEUPD"].Value = pSession.AppInstance.HostName;
            dataParameters["IDSESSIONUPD"].Value = pSession.SessionId;

            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, dataParameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Mise à jour du status de génération de l'accusé de traitement
        /// </summary>
        /// <param name="ackStatusResult"></param>
        /// <param name="acknowledgmentInfoResult"></param>
        private void UpdateAckStatus(ProcessStateTools.StatusEnum ackStatusResult, TrackerAcknowledgmentInfo acknowledgmentInfoResult)
        {
            TrackerQuery trackerqry = new TrackerQuery(CS);
            QueryParameters qryParameters = trackerqry.GetQueryUpdateAckStatus();

            AckRemoveCSPwd(acknowledgmentInfoResult);

            DataParameters dataParameters = qryParameters.Parameters;
            dataParameters["ACKSTATUS"].Value = ackStatusResult.ToString();
            dataParameters["ACKXML"].Value = SerializeTrackerAck(acknowledgmentInfoResult);
            dataParameters["IDTRK_L"].Value = IdTRK_L;

            DataHelper.ExecuteNonQuery(qryParameters.Cs, CommandType.Text, qryParameters.Query, dataParameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Mise à jour des compteurs et des statuts 
        /// <para>Liste des colonnes : READYSTATE, STATUSTRACKER ,POSTEDMSG, SUCCESSMSG, WARNINGMSG, ERRORMSG, POSTEDSUBMSG</para>
        /// </summary>
        /// <param name="pStatus"></param>
        /// <param name="pPostedSubMsg"></param>
        /// FI 20160422 [22076] Modify
        /// RD 20160801 [22407] Modify
        public void SetCounter(ProcessStateTools.StatusEnum pStatus, int pPostedSubMsg, AppSession pSession)
        {
            TryMultiple tryMultiple = new TryMultiple(CS, "TrackerSetCounter", $"Tracker SetCounter")
            {
                MaxAttemptNumber = 10,
                ThreadSleep = 1 //blocage de 1 secondes entre chaque tentative
            };

            tryMultiple.Exec(() =>
            {
                using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS))
                {
                    try
                    {
                        UpdateCounter(dbTransaction, pStatus, pPostedSubMsg, pSession);
                        ReadCounter(dbTransaction);
                        DataHelper.CommitTran(dbTransaction);
                    }
                    catch (Exception ex)
                    {
                        DataHelper.RollbackTran(dbTransaction);
                        throw ex;
                    }
                };
            });
        }

        /// <summary>
        /// Mise à jour des compteurs 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pStatus"></param>
        /// <param name="pPostedSubMsg"></param>
        /// <param name="pSession"></param>
        /// FI 20160422 [22076] Modify (add parameter pDbTransaction, methode private)
        /// EG 20180525 [23979] IRQ Processing
        private void UpdateCounter(IDbTransaction pDbTransaction, ProcessStateTools.StatusEnum pStatus, int pPostedSubMsg, AppSession pSession)
        {
            // FI 20160422 [22076] add ArgumentNullException
            if (null == pDbTransaction)
                throw new ArgumentNullException("parameter pDbTransaction is null");

            QueryParameters qryParameters = new TrackerQuery(CS).GetQueryUpdateCounter(pStatus);

            DataParameters dataParameters = qryParameters.Parameters;
            dataParameters["IDAUPD"].Value = pSession.IdA;
            dataParameters["DTUPD"].Value = GetDate();
            dataParameters["HOSTNAMEUPD"].Value = pSession.AppInstance.HostName;
            dataParameters["IDSESSIONUPD"].Value = pSession.SessionId;
            dataParameters["IRQIDTRK_L"].Value = IrqIdTRK_L ?? Convert.DBNull;
            dataParameters["IDTRK_L"].Value = IdTRK_L;
            dataParameters["POSTEDSUBMSG"].Value = pPostedSubMsg;

            // FI 20160422 [22076] use pDbTransaction
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, dataParameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Lecture des colonnes READYSTATE, STATUSTRACKER ,POSTEDMSG, SUCCESSMSG, WARNINGMSG, ERRORMSG, POSTEDSUBMSG du tracker
        /// </summary>
        /// FI 20160422 [22076] Modify (add parameter pDbTransaction, methode private)
        // EG 20180425 Analyse du code Correction [CA2202]
        private void ReadCounter(IDbTransaction pDbTransaction)
        {
            // FI 20160422 [22076] add ArgumentNullException
            if (null == pDbTransaction)
                throw new ArgumentNullException("parameter pDbTransaction is null");

            QueryParameters qryParameters = new TrackerQuery(CS).GetQuerySelect();
            DataParameters dataParameters = qryParameters.Parameters;
            dataParameters["IDTRK_L"].Value = IdTRK_L;

            using (IDataReader dr = DataHelper.ExecuteReader(pDbTransaction, CommandType.Text, qryParameters.Query, dataParameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["READYSTATE"]))
                        ReadyState = ProcessStateTools.ParseReadyState(Convert.ToString(dr["READYSTATE"]));
                    if (false == Convert.IsDBNull(dr["STATUSTRACKER"]))
                        Status = ProcessStateTools.ParseStatus(Convert.ToString(dr["STATUSTRACKER"]));
                    if (false == Convert.IsDBNull(dr["POSTEDMSG"]))
                        Request.PostedMsg = Convert.ToInt32(dr["POSTEDMSG"]);
                    if (false == Convert.IsDBNull(dr["NONEMSG"])) // FI 20201030 [25537]
                        Request.NoneMsg = Convert.ToInt32(dr["NONEMSG"]);
                    if (false == Convert.IsDBNull(dr["SUCCESSMSG"]))
                        Request.SuccessMsg = Convert.ToInt32(dr["SUCCESSMSG"]);
                    if (false == Convert.IsDBNull(dr["WARNINGMSG"]))
                        Request.WarningMsg = Convert.ToInt32(dr["WARNINGMSG"]);
                    if (false == Convert.IsDBNull(dr["ERRORMSG"]))
                        Request.ErrorMsg = Convert.ToInt32(dr["ERRORMSG"]);
                    if (false == Convert.IsDBNull(dr["POSTEDSUBMSG"]))
                        Request.PostedSubMsg = Convert.ToInt32(dr["POSTEDSUBMSG"]);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="postedMsg"></param>
        /// <param name="pSession"></param>
        public void UpdatePostedMsg(int postedMsg, AppSession pSession)
        {
            Request.PostedMsg = postedMsg;

            TrackerQuery trackerqry = new TrackerQuery(CS);

            QueryParameters qryParameters = trackerqry.GetQueryUpdatePostedMsg();
            DataParameters dataParameters = qryParameters.Parameters;

            dataParameters["IDAUPD"].Value = pSession.IdA;
            dataParameters["DTUPD"].Value = GetDate();
            dataParameters["HOSTNAMEUPD"].Value = pSession.AppInstance.HostName;
            dataParameters["IDSESSIONUPD"].Value = pSession.SessionId;
            dataParameters["IDTRK_L"].Value = IdTRK_L;
            dataParameters["POSTEDMSG"].Value = Request.PostedMsg;

            DataHelper.ExecuteNonQuery(qryParameters.Cs, CommandType.Text, qryParameters.Query, dataParameters.GetArrayDbParameter());

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdDataIdent"></param>
        /// <param name="pIdData"></param>
        /// <returns></returns>
        public void UpdateIdData(string pIdDataIdent, int pIdData)
        {
            IdData.idIdent = pIdDataIdent;
            IdData.id = pIdData;

            TrackerQuery trackerqry = new TrackerQuery(CS);
            QueryParameters qryParameters = trackerqry.GetQueryUpdateIdData();
            DataParameters dataParameters = qryParameters.Parameters;

            dataParameters["IDDATA"].Value = IdData.id;
            dataParameters["IDDATAIDENT"].Value = IdData.idIdent;
            dataParameters["IDDATAIDENTIFIER"].Value = IdData.idIdentifier;
            dataParameters["IDTRK_L"].Value = IdTRK_L;

            DataHelper.ExecuteNonQuery(qryParameters.Cs, CommandType.Text, qryParameters.Query, dataParameters.GetArrayDbParameter());
        }

        /// <summary>
        /// <para>Génération de l'accusé de reception</para>
        /// <para>Mise à jour du tracker (colonne TRACKER.ACKSTATUS, TRACKER.ACKXML)</para>
        /// </summary>
        /// FI 20170406 [23053] Modify
        /// EG 20190318 Test IRQ
        /// <param name="process">Procces à l'origine de l'accusé</param>
        public void AckGenerate(Cst.ProcessTypeEnum process)
        {
            // FI 20170406 [23053] Appel à _ack.schedules.Generate(this) effectué dans le try catch
            //Cst.ErrLevel[] codeReturns = _ack.schedules.Generate(this);
            if (ArrFunc.IsFilled(Ack.schedules.Item))
            {
                AppInstance.TraceManager.TraceInformation(this, $"Acknowledgment Generation: \r\n{SerializeTrackerAck(Ack)}");

                // FI 20230123 [XXXXX] Usage de SerializationHelper.DeepClone ainsi le Ack initial n'est pas modifié
                // ackInfoResult contiendra l'interprétation des variables et le statut retour de chaque ack et sera stocké dans TRACKER_L 
                TrackerAcknowledgmentInfo ackInfoResult = SerializationHelper.DeepClone<TrackerAcknowledgmentInfo>(Ack);

                ProcessStateTools.StatusEnum ackStatus = ProcessStateTools.StatusEnum.NA;

                // FI 20170406 [23053] Mise en place d'un Try Catch pour que la mise à jour de TRACKER.ACKSTATUS s'effectue lorsque se produit un exception
                try
                {
                    ackInfoResult.schedules.InitTraceError(AppInstance.TraceManager.TraceError);

                    ProcessAcknowledgment processAcknowledgment = new ProcessAcknowledgment
                    {
                        CS = CS,
                        ProcessRequested = ProcessRequested,
                        IdData = IdData,
                        IdInfo = ackInfoResult.idInfo,
                        ExtlId = ackInfoResult.extlId,
                        RequestId = IdTRK_L,
                        IdLogProcess = IdProcess,
                        Request = Request,
                        ReadyState = ReadyState,
                        Status = Status,
                        // Process à l'origine de l'accusé de traitement 
                        Process = process
                    };

                    Cst.ErrLevel[] codeReturns = ackInfoResult.schedules.Generate(processAcknowledgment);

                    foreach (Cst.ErrLevel codeReturn in codeReturns)
                    {
                        switch (codeReturn)
                        {
                            case Cst.ErrLevel.SUCCESS:
                                if (ackStatus == ProcessStateTools.StatusEnum.NA ||
                                    ackStatus == ProcessStateTools.StatusEnum.NONE)
                                    ackStatus = ProcessStateTools.StatusEnum.SUCCESS;
                                break;
                            case Cst.ErrLevel.NOTHINGTODO:
                                if (ackStatus == ProcessStateTools.StatusEnum.NA)
                                    ackStatus = ProcessStateTools.StatusEnum.NONE;
                                break;
                            case Cst.ErrLevel.IRQ_EXECUTED:
                                ackStatus = ProcessStateTools.StatusEnum.IRQ;
                                break;
                            default:
                                ackStatus = ProcessStateTools.StatusEnum.ERROR;
                                break;
                        }
                        if (ProcessStateTools.StatusEnum.ERROR == ackStatus)
                            break;
                    }

                }
                catch
                {
                    ackStatus = ProcessStateTools.StatusEnum.ERROR;
                    throw;
                }
                finally
                {
                    UpdateAckStatus(ackStatus, ackInfoResult);
                }
            }
        }

        /// <summary>
        /// Supprime le password associé à une connectionString
        /// </summary>
        /// <param name="acknowledgmentInfo"></param>
        /// FI 20230209 [XXXXX] Add
        private static void AckRemoveCSPwd(TrackerAcknowledgmentInfo acknowledgmentInfo)
        {
            if (null == acknowledgmentInfo)
                throw new ArgumentNullException(nameof(acknowledgmentInfo));


            IEnumerable<AckSQLCommandSchedule> ackSQL = (from item in acknowledgmentInfo.schedules.Item.Where(x => x.GetType().Equals(typeof(AckSQLCommandSchedule)))
                                                         select item).Cast<AckSQLCommandSchedule>();

            foreach (AckSQLCommandSchedule item in ackSQL.Where(x => x.connectionStringSpecified))
                item.connectionString = new CSManager(item.connectionString).GetCSAnonymizePwd();
        }

        /// <summary>
        /// Serialisation du détail d'une demande POSREQUEST
        /// </summary>

        private static string SerializeTrackerAck(TrackerAcknowledgmentInfo ack)
        {
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(TrackerAcknowledgmentInfo), ack);
            // FI 20230103 [26204] Encoding.Unicode (puisque accepté par Oracle et sqlServer)
            StringBuilder sb = CacheSerializer.Serialize(serializeInfo, Encoding.Unicode);
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXML"></param>
        /// <returns></returns>
        private static TrackerAcknowledgmentInfo DeSerializeTrackerAck(string pXML)
        {
            return (TrackerAcknowledgmentInfo)CacheSerializer.Deserialize(new EFS_SerializeInfoBase(typeof(TrackerAcknowledgmentInfo), pXML));

        }

        #endregion Methods

        #region ProcessAcknowledgment
        /// <summary>
        /// 
        /// </summary>
        protected class ProcessAcknowledgment : IProcessAcknowledgment
        {

            public string CS;

            public IdData IdData;

            public IdInfo IdInfo;

            public string ExtlId;

            public int RequestId;

            public Request Request;

            public int IdLogProcess;

            public Cst.ProcessTypeEnum ProcessRequested;

            public Cst.ProcessTypeEnum Process;

            public ProcessStateTools.ReadyStateEnum ReadyState;

            public ProcessStateTools.StatusEnum Status;

            public ProcessAcknowledgment()
            {
            }


            string IProcessAcknowledgment.CS => CS;

            IdData IProcessAcknowledgment.IdData => IdData;

            IdInfo IProcessAcknowledgment.IdInfo => IdInfo;

            string IProcessAcknowledgment.ExtlId => ExtlId;

            int IProcessAcknowledgment.RequestId => RequestId;

            Request IProcessAcknowledgment.Request => Request;

            int IProcessAcknowledgment.IdLogProcess => IdLogProcess;

            Cst.ProcessTypeEnum IProcessAcknowledgment.ProcessRequested => ProcessRequested;

            Cst.ProcessTypeEnum IProcessAcknowledgment.Process => Process;

            ProcessStateTools.ReadyStateEnum IProcessAcknowledgment.ReadyState => ReadyState;

            ProcessStateTools.StatusEnum IProcessAcknowledgment.Status => Status;

        }
        #endregion

    }
    #endregion Tracker

}

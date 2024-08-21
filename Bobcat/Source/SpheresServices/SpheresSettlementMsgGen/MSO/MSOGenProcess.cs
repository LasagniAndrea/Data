#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
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
using EfsML.Interface;
using EfsML.Settlement;
using EfsML.Settlement.Message;
//
using FpML.Enum;
#endregion Using Directives

namespace EFS.Process.SettlementMessage
{
    #region MSOGenProcess
    /// <summary>
    /// Description résumée de MSOGenProcess.
    /// </summary>
    public class MSOGenProcess : ProcessBase
    {
        #region Members
        readonly MSOGenMQueue msoGenMQueue;
        /// <summary>
        /// Liste des Trades impliqués dans les messages envoyés 
        /// </summary>
        int[] idT; 
        #endregion Members
        #region Accessors
        #region dataIdent
        protected override string DataIdent
        {
            get
            {
                return Cst.OTCml_TBL.STLMESSAGE.ToString();
            }
        }
        #endregion dataIdent
        #region TypeLockEnum
        protected override TypeLockEnum DataTypeLock
        {
            get
            {
                return TypeLockEnum.STLMESSAGE;
            }
        }
        #endregion TypeLockEnum
        #endregion Accessors
        #region Constructor
        public MSOGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance): base(pMQueue, pAppInstance)
        {
            msoGenMQueue = (MSOGenMQueue)pMQueue;
        }
        #endregion Constructor
        #region Methods
        #region SelectDatas
        protected override void SelectDatas()
        {
            DataParameters parameters = new DataParameters();
            if (MQueue.idSpecified)
                parameters.Add(new DataParameter(Cs, "IDSTLMESSAGE", DbType.Int32), MQueue.id);
            else if (MQueue.identifierSpecified)
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDENTIFIER), MQueue.identifier);

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT + @"m.IDSTLMESSAGE As IDPARENT" + Cst.CrLf);
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.STLMESSAGE + " m" + Cst.CrLf;

            SQLWhere sqlWhere = new SQLWhere();
            if (MQueue.idSpecified)
                sqlWhere.Append(@"(m.IDSTLMESSAGE=@IDSTLMESSAGE)");
            else if (MQueue.identifierSpecified)
                sqlWhere.Append(@"(m.IDENTIFIER=@IDENTIFIER)");
            sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(Cs, "m"));

            sqlSelect += sqlWhere.ToString();
            string sqlSelectTrade = sqlSelect.ToString();
            DsDatas = DataHelper.ExecuteDataset(Cs, CommandType.Text, sqlSelectTrade, parameters.GetArrayDbParameter());
        }
        #endregion SelectDatas
        #region ProcessInitialize
        protected override void ProcessInitialize()
        {
            base.ProcessInitialize();

            if (false == IsProcessObserver)
            {
                ProcessTuning = new ProcessTuning(Cs, 0, MQueue.ProcessType, AppInstance.ServiceName, AppInstance.HostName);
                if (! ProcessTuningSpecified)
                    ProcessTuning = new ProcessTuning(Cs, 0, Cst.ProcessTypeEnum.MSOGEN, AppInstance.ServiceName, AppInstance.HostName);
                
                if (ProcessTuningSpecified)
                {
                    LogDetailEnum = ProcessTuning.LogDetailEnum;

                    
                    Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(LogDetailEnum));
                }
            }
        }
        #endregion
        #region ProcessExecuteSpecific
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            SQLWhere sqlWhere = GetMqueueSqlWhere();
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT_DISTINCT + "e.DTESR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ESR.ToString() + " e" + Cst.CrLf;
            sqlSelect += sqlWhere.ToString();
            sqlSelect += SQLCst.ORDERBY + "DTESR";

            ArrayList lds = new ArrayList();
            using (IDataReader dr = DataHelper.ExecuteReader(Cs, System.Data.CommandType.Text, sqlSelect.ToString(), GetMQueueDataParameters().GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    DatasetSettlementMessage ds = new DatasetSettlementMessage(Cs, dr.GetDateTime(0), LoadDataSetSettlementMessage.LoadESRAndMSO, IsModeSimul);
                    ds.LoadDs();
                    lds.Add(ds);
                }
            }

            if (lds.Count == 0)
            {
                // Pour l'instant pas gééré le sender 
                if (null != msoGenMQueue.parameters[MSOGenMQueue.PARAM_IDA_SENDER])
                {
                    _ = LogTools.IdentifierAndId(msoGenMQueue.parameters[MSOGenMQueue.PARAM_IDA_SENDER].ExValue,
                    msoGenMQueue.GetIntValueParameterById(MSOGenMQueue.PARAM_IDA_SENDER));
                }

                ret = Cst.ErrLevel.NOTHINGTODO;

                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 3451), 0,
                    new LogParam(DtFunc.DateTimeToStringDateISO(msoGenMQueue.GetDateTimeValueParameterById(MSOGenMQueue.PARAM_DTMSO)))));
            }

            IDbTransaction dbTransaction = DataHelper.BeginTran(Cs);
            try
            {
                if (lds.Count > 0)
                {

                    ret = Cst.ErrLevel.NOTHINGTODO;

                    Logger.Log(new LoggerData(LogLevelEnum.Info, "Generation of Message(s)"));

                    SQL_SettlementMessage sqlStlMsg = new SQL_SettlementMessage(Cs, CurrentId);

                    ArrayList listIdE = new ArrayList();

                    DatasetSettlementMessage[] ads = (DatasetSettlementMessage[])lds.ToArray(typeof(DatasetSettlementMessage));
                    for (int i = 0; i < ArrFunc.Count(ads); i++)
                    {
                        SettlementMessageGenerator msgGen = new SettlementMessageGenerator(ads[i], sqlStlMsg);
                        msgGen.LoadMessageAndDetails();
                        SettlementMessageAndDetails[] settlementMsgAndDetails = msgGen.GetSettlementMsgAndDetails();
                        if (ArrFunc.IsFilled(settlementMsgAndDetails))
                        {
                            for (int j = 0; j < settlementMsgAndDetails.Length; j++)
                            {
                                SettlementMessageAndDetails item = settlementMsgAndDetails[j];

                                bool isOk = true;
                                if (sqlStlMsg.NbMinFlow > 0)
                                    isOk = item.NbFlow >= sqlStlMsg.NbMinFlow;
                                //Mise a jour database
                                if (isOk)
                                {
                                    SQLUP.GetId(out int newId, dbTransaction, SQLUP.IdGetId.MSO, SQLUP.PosRetGetId.First, 1);
                                    item.InitializeMessageDocument(Cs, newId, sqlStlMsg);

                                    ISettlementMessageDocument doc = item.SettlementMessageDocument;
                                    EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(doc.GetType(), doc);
                                    StringBuilder msgXml = CacheSerializer.Serialize(serializeInfo);
                                    StrBuilder msgXmlRemoveAlias = new StrBuilder(XSLTTools.RemoveXmlnsAlias(msgXml));

                                    string xsltFile = sqlStlMsg.XsltFile;
                                    AppInstance.SearchFile2(Cs, sqlStlMsg.XsltFile, ref xsltFile);
                                    string msgMso = XSLTTools.TransformXml(msgXmlRemoveAlias.StringBuilder, xsltFile, null, null);
                                    ads[i].AddRowMSO(newId, msgXmlRemoveAlias.ToString(), sqlStlMsg.Id, xsltFile, msgMso, Session.IdA);
                                    for (int k = 0; k < item.NbFlow; k++)
                                        ads[i].AddRowMSODET(newId, item.GetStlMsgPayment()[k].id, Session.IdA);

                                    //Sauvegarde des évènements considérés par le traitement
                                    for (int k = 0; k < item.NbFlow; k++)
                                    {
                                        int[] idEInEsr = SettlementMessageTools.GetEventInEsr(Cs, item.GetStlMsgPayment()[k].id);
                                        if (ArrFunc.IsFilled(idEInEsr))
                                            listIdE.AddRange(idEInEsr);
                                    }

                                    // dès que 1 message est généré => Traitement en succès
                                    ret = Cst.ErrLevel.SUCCESS;
                                }
                                else
                                {
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                                    Logger.Log(new LoggerData(LogLevelEnum.Error, "No enough flow for this message"));
                                }
                            }
                            ads[i].UpdateMSO(dbTransaction);
                            ads[i].UpdateMSODET(dbTransaction);
                        }
                        else
                        {
                            
                            // PM 20210121 [XXXXX] Passage du message au niveau de log None
                            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 3452), 0,
                                new LogParam(DtFunc.DateTimeToStringDateISO(ads[i].Date)),
                                new LogParam(sqlStlMsg.Identifier)));
                        }
                    }

                    #region EVENT => mise à jour des status [ProcessTuning] et alimentation de EVENTPROCESS

                    //
                    if (listIdE.Count > 0)
                    {
                        int[] idE = (int[])listIdE.ToArray(typeof(int));

                        if (ProcessTuningSpecified)
                        {
                            // EG 20150612 [20665] Chargement direct
                            DataSetEventTrade dsEvent = new DataSetEventTrade(Cs, idE);
                            if (ArrFunc.IsFilled(dsEvent.DtEvent.Rows))
                            {
                                foreach (DataRow datarow in dsEvent.DtEvent.Rows)
                                {
                                    // FI 20200820 [25468] dates systèmes en UTC
                                    dsEvent.SetEventStatus(Convert.ToInt32(datarow["ID"]), ProcessTuning.GetProcessTuningOutput(TuningOutputTypeEnum.OES), Session.IdA, OTCmlHelper.GetDateSysUTC(Cs));
                                }
                            }
                            dsEvent.Update(dbTransaction);
                        }

                        
                        EventProcess eventProcess = new EventProcess(Cs);
                        for (int i = 0; i < ArrFunc.Count(idE); i++)
                        {
                            // FI 20200820 [25468] dates systèmes en UTC
                            eventProcess.Write(dbTransaction, idE[i], MQueue.ProcessType, ProcessStateTools.StatusSuccessEnum, OTCmlHelper.GetDateSysUTC(Cs), Tracker.IdTRK_L);
                        }

                        // Sauvegarde de la liste des trades impliqués
                        idT = TradeRDBMSTools.GetIdTradeFromIdEvent(Cs, idE);
                    }
                    #endregion

                    //PL 20151229 Use DataHelper.CommitTran()
                    //dbTransaction. Commit();
                    DataHelper.CommitTran(dbTransaction);
                }
            }
            catch
            {
                if (null != dbTransaction)
                {
                    DataHelper.RollbackTran(dbTransaction);
                    dbTransaction.Dispose();
                }
                throw;
            }

            return ret;
        }
        #endregion ProcessExecuteSpecific
        #region ProcessTerminateSpecific
        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessTerminateSpecific()
        {
            //Mise à jour des statuts sur le trade
            for (int i = 0; i < ArrFunc.Count(idT); i++)
                SetTradeStatus(idT[i], ProcessState.Status);
        }
        #endregion ProcessTerminateSpecific
        #region GetMQueueDataParameters
        protected DataParameters GetMQueueDataParameters()
        {
            DataParameters dataParameters = new DataParameters();
            if (msoGenMQueue.parametersSpecified)
            {
                if (null != msoGenMQueue.GetObjectValueParameterById(MSOGenMQueue.PARAM_DTMSO))
                {
                    dataParameters.Add(new DataParameter(Cs, "DTESR", DbType.Date));
                    dataParameters["DTESR"].Value = Convert.ToDateTime(msoGenMQueue.GetDateTimeValueParameterById(MSOGenMQueue.PARAM_DTMSO));
                }
                if (null != msoGenMQueue.GetObjectValueParameterById(MSOGenMQueue.PARAM_IDA_SENDER))
                {
                    dataParameters.Add(new DataParameter(Cs, "IDA_SENDER", DbType.Int32));
                    dataParameters["IDA_SENDER"].Value = Convert.ToInt32(msoGenMQueue.GetIntValueParameterById(MSOGenMQueue.PARAM_IDA_SENDER));
                }
            }
            return dataParameters;
        }
        #endregion GetMQueueDataParameters
        #region GetMqueueSqlWhere
        protected SQLWhere GetMqueueSqlWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();
            DataParameters parameters = GetMQueueDataParameters();
            if (parameters.Contains("DTESR"))
                sqlWhere.Append("e.DTESR=@DTESR");
            if (parameters.Contains(MSOGenMQueue.PARAM_IDA_SENDER.ToString()))
                sqlWhere.Append("e.IDA_SENDER=@IDA_SENDER");
            return sqlWhere;

        }
        #endregion
        #endregion Methods
    }
    #endregion MSOGenProcess

    #region SettlementMessageAndDetails
    /// <summary>
    /// Représente un message de règelement et les paiements qui s'y trouvent
    /// </summary>
    public class SettlementMessageAndDetails
    {
        #region Members
        /// <summary>
        /// Représente le message de règlement 
        /// </summary>
        private readonly SettlementMessageDocumentContainer _stlMsgDocContainer;
        /// <summary>
        /// <summary>
        /// Représente le(s)  montant(s) de règlement 
        /// <para>Il peut y a voir plusieurs montants lorsque le message l'admet (Exemple MT210)</para>
        /// </summary>
        /// </summary>
        private readonly SettlementMessagePaymentStructure[] _stlMsgPayment;
        #endregion Members
        #region Accessors
        public int NbFlow
        {
            get
            {
                return ArrFunc.Count(_stlMsgPayment);
            }
        }
        public ISettlementMessageDocument SettlementMessageDocument
        {
            get
            {
                return _stlMsgDocContainer.settlementMessageDoc;
            }
        }
        #endregion Accessors
        #region Constructors
        public SettlementMessageAndDetails(ISettlementMessageDocument pstlmsgDoc, SettlementMessagePaymentStructure[] pstlmsgPayment)
        {
            _stlMsgDocContainer = new SettlementMessageDocumentContainer(pstlmsgDoc);
            _stlMsgPayment = pstlmsgPayment;
        }
        #endregion Constructors
        #region Methods
        #region GetStlMsgPayment
        public SettlementMessagePaymentStructure[] GetStlMsgPayment()
        {
            return _stlMsgPayment;
        }
        #endregion GetStlMsgPayment
        #region InitializeMessageDocument
        public void InitializeMessageDocument(string pCs, int pOTCmlId, SQL_SettlementMessage pSqlStlMsg)
        {
            if (ArrFunc.IsFilled(_stlMsgPayment))
                _stlMsgDocContainer.Initialize(pCs, pOTCmlId, _stlMsgPayment, pSqlStlMsg);
        }
        #endregion InitializeMessageDocument
        #endregion Methods
    }
    #endregion SettlementMessageAndDetails

    #region SettlementMessageGenerator
    /// <summary>
    ///  Génération d'un  message de règlement
    /// </summary>
    public class SettlementMessageGenerator
    {
        #region Members
        private readonly DatasetSettlementMessage _dsEsr;
        private readonly SQL_SettlementMessage _sqlStlMsg;
        private readonly ArrayList _listMsg;
        #endregion Members
        #region Accessors
        public string Cs
        {
            get { return _dsEsr.Cs; }
        }
        public DataTable DtEsr
        {
            get { return _dsEsr.DtESR; }
        }
        private PayerReceiverEnum MessageSide
        {
            get
            {
                return (PayerReceiverEnum)Enum.Parse(typeof(PayerReceiverEnum), _sqlStlMsg.Payer_Receiver);
            }
        }
        private bool IsMessagePayer
        {
            get
            {
                return (PayerReceiverEnum.Payer == this.MessageSide);
            }
        }
        #endregion Accessors
        #region Constructors
        public SettlementMessageGenerator(DatasetSettlementMessage pDs, SQL_SettlementMessage pSqlStlMsg)
        {
            _dsEsr = pDs;
            _sqlStlMsg = pSqlStlMsg;
            _listMsg = new ArrayList();
        }
        #endregion Constructors
        #region Methods
        #region LoadMessageAndDetails
        /// <summary>
        /// Charge les Montants de règlements évenstuellements disponibles
        /// </summary>
        public void LoadMessageAndDetails()
        {
            SetFlagInCompatibleESR();
            Load();
        }
        #endregion LoadMessageAndDetails
        #region SetFlagInCompatibleESR
        private void SetFlagInCompatibleESR()
        {
            for (int i = 0; i < DtEsr.Rows.Count; i++)
            {
                bool isCompatible = IsRowESRCompatible(i);
                if (isCompatible)
                {
                    DataRow[] rows = DtEsr.Rows[i].GetChildRows(_dsEsr.ChildESR_MSODET);
                    isCompatible = ArrFunc.IsEmpty(rows);
                }
                if (isCompatible)
                    DtEsr.Rows[i]["EXTLLINK"] = "ISCOMPATIBLE";
            }
        }
        #endregion SetFlagInCompatibleESR
        #region MessageGenerate
        private void Load()
        {
            SettlementMessagePaymentStructure[] paymentMsg = GetPaymentForMessage();
            if (ArrFunc.IsFilled(paymentMsg))
            {
                ISettlementMessageDocument stlMsgDocument;
                if (EfsMLDocumentVersionEnum.Version30 == paymentMsg[0].efsMLversion)
                    stlMsgDocument = new EfsML.v30.Settlement.Message.SettlementMessageDocument();
                else
                    throw new Exception("SettlementMessageDocument version  not supported");
                //
                _listMsg.Add(new SettlementMessageAndDetails(stlMsgDocument, paymentMsg));
            }
            //
            //GENERATION DU MESSAGE
            if (ArrFunc.IsFilled(DtEsr.Select("EXTLLINK ='ISCOMPATIBLE'")))
                Load();
            //	
        }
        #endregion MessageGenerate
        #region GetPaymentForMessage
        /// <summary>
        ///  Retourne les payments qui seront inclus ds le message
        ///  Les payments issu de trade de versions différentes ne peuvent pas être inclus dans le même message
        /// </summary>
        /// <returns></returns>
        private SettlementMessagePaymentStructure[] GetPaymentForMessage()
        {
            SettlementMessagePaymentStructure[] ret = null;
            ArrayList alEsr = new ArrayList();
            DataRow[] rows = DtEsr.Select("EXTLLINK ='ISCOMPATIBLE'");
            if (ArrFunc.IsFilled(rows))
            {
                string idC = string.Empty;
                string siPayer = string.Empty;
                string siReceiver = string.Empty;
                int idASender = 0;
                int idAReceiver = 0;
                int idACss = 0;
                EfsMLDocumentVersionEnum esrVersion = EfsMLDocumentVersionEnum.Version20;
                for (int i = 0; i < rows.Length; i++)
                {
                    bool isAddEsr = true;
                    //
                    if (_sqlStlMsg.IsMultiFlow)
                    {
                        if (0 == i)
                        {
                            idASender = Convert.ToInt32(rows[i]["IDA_SENDER"]);
                            idAReceiver = Convert.ToInt32(rows[i]["IDA_RECEIVER"]);
                            idACss = Convert.ToInt32(rows[i]["IDA_CSS"]);
                            esrVersion = SettlementMessageTools.GetEsrVersion(_dsEsr.Cs, Convert.ToInt32(rows[i]["ID"]));
                            if (_sqlStlMsg.IsSameIDC)
                                idC = Convert.ToString(rows[i]["IDC"]);
                            if ((_sqlStlMsg.IsSameSiPayer || _sqlStlMsg.IsSameSiReceiver))
                            {
                                siPayer = GetSi(PayerReceiverEnum.Payer, Convert.ToString(rows[i]["SCREF"]));
                                siReceiver = GetSi(PayerReceiverEnum.Receiver, Convert.ToString(rows[i]["SCREF"]));
                            }
                        }
                        else
                        {
                            // Pour regrouper des paiements (ESR)=> Il faut le même auteur et le même destinataire et même CSS
                            if ((idASender != Convert.ToInt32(rows[i]["IDA_SENDER"])) ||
                                    (idAReceiver != Convert.ToInt32(rows[i]["IDA_RECEIVER"])) ||
                                    (idACss != Convert.ToInt32(rows[i]["IDA_CSS"])) ||
                                    (esrVersion != SettlementMessageTools.GetEsrVersion(_dsEsr.Cs, Convert.ToInt32(rows[i]["ID"])))
                                )
                                isAddEsr = false;
                            if (StrFunc.IsFilled(idC) && (idC != Convert.ToString(rows[i]["IDC"])))
                                isAddEsr = false;
                            if (StrFunc.IsFilled(siPayer) || StrFunc.IsFilled(siReceiver))
                            {
                                string siPayerCurrent = GetSi(PayerReceiverEnum.Payer, Convert.ToString(rows[i]["SCREF"]));
                                string siReceiverCurrent = GetSi(PayerReceiverEnum.Receiver, Convert.ToString(rows[i]["SCREF"]));
                                if ((StrFunc.IsFilled(siPayer)) && (siPayer != siPayerCurrent))
                                    isAddEsr = false;
                                if ((StrFunc.IsFilled(siReceiver)) && (siReceiver != siReceiverCurrent))
                                    isAddEsr = false;
                            }
                        }
                        if (isAddEsr)
                        {
                            rows[i]["EXTLLINK"] = Convert.DBNull;
                            alEsr.Add(new SettlementMessagePaymentStructure(rows[i], Cst.OTCml_EsrIdScheme, esrVersion));
                            if ((_sqlStlMsg.NbMaxFlow > 0) && (_sqlStlMsg.NbMaxFlow == ArrFunc.Count(alEsr)))
                                break;
                        }
                    }
                    else
                    {
                        rows[i]["EXTLLINK"] = Convert.DBNull;
                        esrVersion = SettlementMessageTools.GetEsrVersion(_dsEsr.Cs, Convert.ToInt32(rows[i]["ID"]));
                        alEsr.Add(new SettlementMessagePaymentStructure(rows[i], Cst.OTCml_EsrIdScheme, esrVersion));
                        break;
                    }
                }
            }
            if (ArrFunc.IsFilled(alEsr))
                ret = (SettlementMessagePaymentStructure[])alEsr.ToArray(typeof(SettlementMessagePaymentStructure));
            return ret;
        }
        #endregion GetPaymentForMessage
        #region GetSi
        private string GetSi(PayerReceiverEnum pPayerReceiver, string pSCRef)
        {
            string[] si = pSCRef.Split(new char[] { '&' });
            string siPayer = IsMessagePayer ? si[0] : si[1];
            string siReceiver = IsMessagePayer ? si[1] : si[0];
            //SCREF  est constitué selon le schéma suivant	(Circuit côté partie gérée / Circuit Contrepartie)   
            //
            // Sur un messge de Type Payer (il ne comptient que des ESR de Type Payer, la partie gerée doit payer) (Ex 202)
            // Les circuits payer sont du côté [0] ds SIREF
            // Sur un messge de Type Receiver (il ne comptient que des ESR de Type Receiver, la partie gerée doit recevoir) (Ex 210)
            // Les circuits receiver sont du côté [0] ds SIREF
            string ret;
            if (PayerReceiverEnum.Payer == pPayerReceiver)
                ret = siPayer;
            else
                ret = siReceiver;

            return ret;
        }
        #endregion GetSi
        #region GetSettlementMsgAndDetails
        public SettlementMessageAndDetails[] GetSettlementMsgAndDetails()
        {
            SettlementMessageAndDetails[] ret = null;
            if (ArrFunc.IsFilled(_listMsg))
                ret = (SettlementMessageAndDetails[])_listMsg.ToArray(typeof(SettlementMessageAndDetails));
            return ret;
        }
        #endregion GetSettlementMsgAndDetails
        #region IsRowESRCompatible
        private bool IsRowESRCompatible(int pIndex)
        {
            PayerReceiverEnum side = (PayerReceiverEnum)Enum.Parse(typeof(PayerReceiverEnum), Convert.ToString(_dsEsr.DtESR.Rows[pIndex]["PAYER_RECEIVER"]), true);
            int css = Convert.ToInt32(_dsEsr.DtESR.Rows[pIndex]["IDA_CSS"]);
            int idASender = Convert.ToInt32(_dsEsr.DtESR.Rows[pIndex]["IDA_SENDER"]);
            int idAReceiver = Convert.ToInt32(_dsEsr.DtESR.Rows[pIndex]["IDA_RECEIVER"]);

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "PAYER_RECEIVER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), side.ToString());
            parameters.Add(new DataParameter(Cs, "IDCSS", DbType.Int32), css);
            parameters.Add(new DataParameter(Cs, "IDA_SENDER", DbType.Int32), idASender);
            parameters.Add(new DataParameter(Cs, "IDA_RECEIVER", DbType.Int32), idAReceiver);
            parameters.Add(new DataParameter(Cs, "IDSTLMESSAGE", DbType.Int32), _sqlStlMsg.Id);

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "stlmsg.IDSTLMESSAGE" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.STLMESSAGE.ToString() + " stlmsg" + Cst.CrLf;

            StrBuilder sqlwhere = new StrBuilder();
            sqlwhere += SQLCst.WHERE + "(stlmsg.IDSTLMESSAGE=@IDSTLMESSAGE)" + Cst.CrLf;
            sqlwhere += SQLCst.AND + "(stlmsg.PAYER_RECEIVER=@PAYER_RECEIVER)" + Cst.CrLf;
            sqlwhere += SQLCst.AND + "((stlmsg.IDA_CSS is null) Or (stlmsg.IDA_CSS=@IDCSS))" + Cst.CrLf;
            sqlwhere += SQLCst.AND + "((stlmsg.IDA_SENDER is null) Or (stlmsg.IDA_SENDER=@IDA_SENDER))" + Cst.CrLf;
            sqlwhere += SQLCst.AND + "((stlmsg.IDA_RECEIVER is null) Or (stlmsg.IDA_RECEIVER=@IDA_RECEIVER))" + Cst.CrLf;
            sqlSelect += sqlwhere.ToString();

            object obj = DataHelper.ExecuteScalar(Cs, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            bool isRet = null != obj;
            if (_sqlStlMsg.IsInfoInstrSpecified)
            {
                if (isRet)
                {
                    if (_sqlStlMsg.GProductSpecified)
                        isRet = IsInstrCompatible(pIndex, "GPRODUCT", _sqlStlMsg.GProduct);
                }
                if (isRet)
                {
                    if (_sqlStlMsg.IdPSpecified)
                        isRet = IsInstrCompatible(pIndex, "IDP", _sqlStlMsg.IdP);
                }
                if (isRet)
                {
                    if (_sqlStlMsg.IdGInstrSpecified)
                        isRet = IsInstrCompatible(pIndex, "IDGINSTR", _sqlStlMsg.IdGInstr);
                }
                if (isRet)
                {
                    if (_sqlStlMsg.IdISpecified)
                        isRet = IsInstrCompatible(pIndex, "IDI", _sqlStlMsg.IdI);
                }
            }
            return isRet;
        }
        #endregion IsRowESRCompatible
        #region IsInstrCompatible
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private bool IsInstrCompatible(int pIndex, string pCol, object pValue)
        {
            string cs = _dsEsr.Cs;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(cs, "IDESR", DbType.Int32), _dsEsr.DtESR.Rows[pIndex]["ID"]);
            if (pValue.GetType().Equals(typeof(System.Int32)))
                parameters.Add(new DataParameter(cs, pCol, DbType.Int32), pValue);
            else
                parameters.Add(new DataParameter(cs, pCol, DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pValue);
            parameters.Add(new DataParameter(cs, "IDROLEGINSTR", DbType.AnsiString, SQLCst.UT_ROLEGINSTR_LEN), RoleGInstr.STL.ToString());

            string sqlSelect = String.Format(@"select IDE, count(1) As COUNTCOL
            from (
                select ev.IDE, i.IDI, i.IDP, i.GPRODUCT, gir.IDGINSTR
                from dbo.ESRDET ed
                inner join dbo.EVENT ev on (ev.IDE = ed.IDE)
                inner join dbo.VW_ALLTRADEINSTRUMENT ti on (ti.IDT = ev.IDT) and (ti.INSTRUMENTNO = ev.INSTRUMENTNO)
                inner join dbo.VW_INSTR_PRODUCT i on (i.IDI = ti.IDI)
                left outer join dbo.INSTRG gi on (gi.IDI = i.IDI)
                left outer join dbo.GINSTRROLE gir on (gir.IDGINSTR = gi.IDGINSTR) and (gir.IDROLEGINSTR = @IDROLEGINSTR)
                where (ed.IDESR = @IDESR) and ({0} = @pCol)
            ) result
            group by IDE, {0}", pCol);

            DataSet ds = DataHelper.ExecuteDataset(cs, CommandType.Text, sqlSelect, parameters.GetArrayDbParameter());
            bool isRet = ds.Tables[0].Rows.Count > 0;
            if (isRet)
            {
                // Chaque IDE doit Coller avec Le critère définit sur le message
                DataRow[] rows = _dsEsr.DtESR.Rows[pIndex].GetChildRows(_dsEsr.ChildESR_ESRDET);
                foreach (DataRow row in rows)
                {
                    int ide = Convert.ToInt32(row["IDE"]);
                    DataRow[] rows2 = ds.Tables[0].Select("IDE=" + ide.ToString());
                    isRet = ArrFunc.IsFilled(rows2);
                    if (isRet)
                        isRet = (Convert.ToInt32(rows2[0]["COUNTCOL"]) >= 1);   // >=1 car un instrument peut appartenir à plusieurs groupes 
                    if (false == isRet)
                        break;
                }
            }
            return isRet;
        }
        #endregion IsInstrCompatible
        #endregion Methods
    }
    #endregion SettlementMessageGenerator
}

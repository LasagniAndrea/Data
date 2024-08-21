#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Restriction;
using EFS.SpheresService;
using EFS.Status;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Notification;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.Notification
{
    /// <summary>
    /// Classe pour la génération des éditions (avis d'opéré, actions sur positions, synthèse, situation financière..) et Exportation (envoi via IO)
    /// </summary>
    public class ReportMsgGenProcess : ConfMsgGenProcess
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly ReportMsgGenMQueue rptMsgGenQueue;
        #endregion Members

        #region accessors
        /// <summary>
        ///  Obtient true si envoi du message 
        /// </summary>
        /// <returns></returns>
        protected override bool IsWithIO
        {
            get { return rptMsgGenQueue.GetBoolValueParameterById(MQueueBase.PARAM_ISWITHIO); }
        }

        /// <summary>
        ///  Obtient true si regeneration d'un message existant
        /// </summary>
        protected override bool IsModeRegenerate
        {
            get { return rptMsgGenQueue.GetBoolValueParameterById(ReportMsgGenMQueue.PARAM_ISMODEREGENERATE); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Génération des éditions
        /// </summary>
        /// <param name="pMQueue">Message queue declencheur</param>
        /// <param name="pAppInstance">Service</param>
        public ReportMsgGenProcess(ReportMsgGenMQueue pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            rptMsgGenQueue = pMQueue;
        }
        #endregion Constructor
    }

    /// <summary>
    /// Traitement de génération des éditions
    /// </summary>
    public class ReportInstrMsgGenProcess : ProcessBase
    {
        #region Members
        /// <summary>
        /// Représente le Message queue 
        /// </summary>
        private readonly ReportInstrMsgGenMQueue _rptMsgGenMqueue;

        /// <summary>
        /// Représente la chaîne de confirmation
        /// </summary>
        private ConfirmationChainProcess _confirmationChain;

        /// <summary>
        /// Représente les paramètres du traitement
        /// </summary>
        private ReportInstrMsgGenProcessSettings _settings;

        #endregion Members
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected override bool IsMonoDataProcess
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool IsProcessSendMessage
        {
            get
            {
                return base.IsProcessSendMessage;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override TypeLockEnum DataTypeLock
        {
            get
            {
                return TypeLockEnum.ACTOR;
            }
        }

        /// <summary>
        /// Obtient le type d'identification associé au currentId
        /// </summary>
        protected override string DataIdent
        {
            get { return Cst.OTCml_TBL.ACTOR.ToString(); }
        }

        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pAppInstance"></param>
        /// FI 20120903 [17773] Modify
        public ReportInstrMsgGenProcess(ReportInstrMsgGenMQueue pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            _rptMsgGenMqueue = pMQueue;

            if (false == IsProcessObserver)
                _settings = LoadProcessSettings(Cs, pMQueue);

            //FI 20120903 [17773] Si l'id est non renseigné, Spheres® effectue son alimentation pour ne pas rentrer dans SelectDatas
            if (false == _rptMsgGenMqueue.idSpecified)
            {
                _rptMsgGenMqueue.idSpecified = true;
                _rptMsgGenMqueue.id = _settings.idA;
            }
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void SelectDatas()
        {
            //Spheres® ne doit jamais rentré ici
            //Spheres® rentre ici si le message n'a pas d'Id de spécifié
            //Un id est désormais obligatoire, il représente l'entité pour laquelle est demandé le traitement 
            throw new Exception("Message queue without Id");
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessPreExecute()
        {
            base.ProcessPreExecute();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessInitialize()
        {
            base.ProcessInitialize();
            if (false == IsProcessObserver)
            {
                #region ProcessTuning => Initialisation from Trade
                ProcessTuning = new ProcessTuning(Cs, 0, MQueue.ProcessType, AppInstance.ServiceName, AppInstance.HostName);
                if (ProcessTuningSpecified)
                {
                    LogDetailEnum = ProcessTuning.LogDetailEnum;

                    
                    Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(LogDetailEnum));
                }
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                if (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret))
                {
                    if (ProcessCall == ProcessCallEnum.Master)
                    {
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3300), 0,
                            new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));
                    }

                    //Chargement de la chaîne de confirmation
                    LoadConfirmationChain();

                    //Normalement isOk est tjs à vrai
                    //Dans le grid de lancement du process, les enregistrements retenus donneront tjs true
                    //Laisser ici par sécurité
                    string msg = string.Empty;
                    bool isOk = _confirmationChain.IsGenerateMessage(CSTools.SetCacheOn(Cs), _settings.notificationClass);
                    if (isOk)
                    {
                        if (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret))
                        {
                            if (_settings.notificationClass == NotificationClassEnum.MULTITRADES)
                            {
                                ret = MessageMultiTradesGen();
                            }
                            else if (_settings.notificationClass == NotificationClassEnum.MULTIPARTIES)
                            {
                                ret = MessageMultiPartiesGen();
                            }
                            else
                            {
                                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", _settings.notificationClass.ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // FI 20220719 [XXXXX] Trace déjà alimentée par le logger
                if (false == LoggerManager.IsEnabled)
                {
                    // FI 20200918 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                    Common.AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));
                }

                // FI 20200623 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(ex);
                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 3460), 0));

                ret = Cst.ErrLevel.FAILURE;
            }
            return ret;
        }

        /// <summary>
        /// Récupère le type de notification présent dans le message queue
        /// </summary>
        /// <param name="pRptMsgGenMqueue"></param>
        /// <exception cref="Exception si type de notification inconnue"></exception>
        /// <returns></returns>
        private static NotificationTypeEnum GetNotificationTypeEnum(ReportInstrMsgGenMQueue pRptMsgGenMqueue)
        {
            string cnfType = pRptMsgGenMqueue.GetStringValueParameterById(ReportInstrMsgGenMQueue.PARAM_CNFTYPE);
            if (false == Enum.IsDefined(typeof(NotificationTypeEnum), cnfType))
                throw new Exception(StrFunc.AppendFormat("{0} is not defined in NotificationTypeEnum", cnfType));
            return ReflectionTools.ConvertStringToEnum<NotificationTypeEnum>(cnfType);
        }

        /// <summary>
        /// Récupère la class de notification présent dans le message queue
        /// </summary>
        /// <param name="pRptMsgGenMqueue"></param>
        /// <returns></returns>
        /// <exception cref="Exception si classe de notification inconnue ou non autorisé"></exception>
        private static NotificationClassEnum GetNotificationClassEnum(ReportInstrMsgGenMQueue pRptMsgGenMqueue)
        {
            string cnfClass = pRptMsgGenMqueue.GetStringValueParameterById(ReportInstrMsgGenMQueue.PARAM_CNFCLASS);
            Nullable<NotificationClassEnum> cnfClassEnum = ReflectionTools.ConvertStringToEnumOrNullable<NotificationClassEnum>(cnfClass);
            if (false == cnfClassEnum.HasValue)
                throw new Exception(StrFunc.AppendFormat("{0} is not defined in NotificationClassEnum", cnfClass));
            else if (cnfClassEnum.Value == NotificationClassEnum.MONOTRADE)
                throw new Exception(StrFunc.AppendFormat("{0} is not authorized", cnfClassEnum.ToString()));
            NotificationClassEnum ret = cnfClassEnum.Value;
            return ret;
        }

        /// <summary>
        /// Récupère un paramètre date présent dans le message queue
        /// </summary>
        /// <param name="pRptMsgGenMqueue"></param>
        /// <param name="pParameterName">Nom du paramètre</param>
        /// <returns></returns>
        /// <exception cref="Exception when parameter is not filled"></exception>
        /// FI 20120829 [18048] modification de signature
        private static DateTime GetDate(ReportInstrMsgGenMQueue pRptMsgGenMqueue, string pParameterName)
        {
            DateTime ret = pRptMsgGenMqueue.GetDateTimeValueParameterById(pParameterName);
            if (DtFunc.IsDateTimeEmpty(ret))
                throw new Exception(StrFunc.AppendFormat("Message queue without date parameter"));
            return ret;
        }


        /// <summary>
        /// Récupère le paramètre entity
        /// </summary>
        /// <param name="pRptMsgGenMqueue"></param>
        /// <returns></returns>
        /// <exception cref="Exception si Entité inconnu"></exception>
        /// FI 20120903 [17773] modification de la signature de la fonction
        /// Gestion du paramètre PARAM_ENTITY_IDENTIFIER s'il n'existe pas le paramètre PARAM_ENTITY
        private static Int32 GetEntity(string pCS, ReportInstrMsgGenMQueue pRptMsgGenMqueue)
        {
            MQueueparameter parameter = pRptMsgGenMqueue.parameters[ReportInstrMsgGenMQueue.PARAM_ENTITY];
            if (null == parameter)
                parameter = pRptMsgGenMqueue.parameters["ENTITY_IDENTIFIER"];
            if (null == parameter)
                throw new Exception(StrFunc.AppendFormat("Message queue without entity parameter"));

            int ret;
            if (parameter.dataType == TypeData.TypeDataEnum.integer)
            {
                ret = pRptMsgGenMqueue.GetIntValueParameterById(parameter.id);
            }
            else
            {
                string entity = pRptMsgGenMqueue.GetStringValueParameterById(parameter.id);
                SQL_Entity sqlEntity = new SQL_Entity(pCS, entity);
                if (sqlEntity.LoadTable(new string[] { sqlEntity.AliasActorTable + ".IDA" }))
                    ret = sqlEntity.Id;
                else
                    throw new Exception(StrFunc.AppendFormat("Message queue, parameter {0} is not a Entity", ReportInstrMsgGenMQueue.PARAM_ENTITY));
            }

            if (ret == 0)
                throw new Exception(StrFunc.AppendFormat("Message queue without entity parameter"));

            return ret;
        }

        /// <summary>
        /// Récupère le paramètre id 
        /// <para>Contient l'acteur destinataire</para>
        /// </summary>
        /// <param name="pRptMsgGenMqueue"></param>
        /// <param name="pCS"></param>
        /// <param name="pFromId"></param>
        /// <returns></returns>
        /// <exception cref="Exception si Actor inconnu"></exception>
        /// FI 20120903 [17773] modification de la signature de la fonction
        /// gestion du paramètre IDENTIFIER s'il n'existe pas id sur le message
        /// FI 20141230 [20616] Modify=> Modification de signature
        private static Int32 GetActor(string pCS, ReportInstrMsgGenMQueue pRptMsgGenMqueue, Boolean pFromId)
        {
            int ret = 0;
            if (pFromId)
            {

                if (pRptMsgGenMqueue.idSpecified)
                {
                    ret = pRptMsgGenMqueue.id;
                }
                else if (pRptMsgGenMqueue.identifierSpecified)
                {
                    SQL_Actor sqlActor = new SQL_Actor(pCS, pRptMsgGenMqueue.identifier);
                    if (sqlActor.LoadTable(new string[] { "IDA" }))
                        ret = sqlActor.Id;
                    else
                        throw new Exception(StrFunc.AppendFormat("Message queue identifier is not an ACTOR"));
                }
                if (ret == 0)
                    throw new Exception(StrFunc.AppendFormat("Message queue without id/identifier"));
            }
            else
            {

                MQueueparameter parameter = pRptMsgGenMqueue.parameters[ReportInstrMsgGenMQueue.PARAM_ACTOR];
                if (null == parameter)
                    parameter = pRptMsgGenMqueue.parameters["ACTOR_IDENTIFIER"];

                if (null != parameter)
                {
                    if (parameter.dataType == TypeData.TypeDataEnum.integer)
                    {
                        ret = pRptMsgGenMqueue.GetIntValueParameterById(parameter.id);
                    }
                    else
                    {
                        string identifier = pRptMsgGenMqueue.GetStringValueParameterById(parameter.id);
                        SQL_Actor sqlactor = new SQL_Actor(pCS, identifier);
                        if (sqlactor.LoadTable(new string[] { "IDA" }))
                            ret = sqlactor.Id;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Récupère le paramètre Book
        /// </summary>
        /// <param name="pRptMsgGenMqueue"></param>
        /// <returns></returns>
        /// <exception cref="Exception si book inconnu"></exception>
        /// FI 20120903 [17773] gestion du paramètre BOOK_IDENTIFIER s'il n'existe pas le paramètre IDB
        /// FI 20141230 [20616] Modify
        private static Int32 GetBook(string pCS, ReportInstrMsgGenMQueue pRptMsgGenMqueue)
        {
            int ret = 0;
            
            MQueueparameter parameter = pRptMsgGenMqueue.parameters[ReportInstrMsgGenMQueue.PARAM_BOOK];
            if (null == parameter)
                parameter = pRptMsgGenMqueue.parameters["BOOK_IDENTIFIER"];

            if (null != parameter)
            {
                if (parameter.dataType == TypeData.TypeDataEnum.integer)
                {
                    ret = pRptMsgGenMqueue.GetIntValueParameterById(parameter.id);
                }
                else
                {
                    string book = pRptMsgGenMqueue.GetStringValueParameterById(parameter.id);
                    SQL_Book sqlBook = new SQL_Book(pCS, SQL_TableWithID.IDType.Identifier, book);
                    if (sqlBook.LoadTable(new string[] { "IDB" }))
                        ret = sqlBook.Id;
                    else
                        throw new Exception(StrFunc.AppendFormat("Message queue, parameter {0} is not an BOOK", ReportInstrMsgGenMQueue.PARAM_BOOK));
                }
            }

            return ret;
        }

        /// <summary>
        /// Récupère le paramètre Marché
        /// </summary>
        /// FI 20120903 [17773] add GetMarket Method
        private static Nullable<Int32> GetMarket(string pCS, ReportInstrMsgGenMQueue pRptMsgGenMqueue)
        {
            Nullable<Int32> ret = null;

            MQueueparameter parameter = pRptMsgGenMqueue.parameters[ReportInstrMsgGenMQueue.PARAM_MARKET];
            if (null == parameter)
                parameter = pRptMsgGenMqueue.parameters["MARKET_IDENTIFIER"];

            if (null != parameter)
            {
                if (parameter.dataType == TypeData.TypeDataEnum.integer)
                {
                    ret = pRptMsgGenMqueue.GetIntValueParameterById(parameter.id);
                    if (ret == 0)
                        ret = null;
                }
                else
                {
                    string marketIdentifier = pRptMsgGenMqueue.GetStringValueParameterById(parameter.id);
                    if (StrFunc.IsFilled(marketIdentifier))
                    {
                        SQL_Market sqlMarket = new SQL_Market(pCS, marketIdentifier);
                        if (sqlMarket.LoadTable(new string[] { "IDM" }))
                            ret = sqlMarket.Id;
                    }
                }
            }

            return ret;
        }


        /// <summary>
        /// Récupère le paramètre groupe de Marché
        /// </summary>
        private static Nullable<Int32> GetGroupe(string pCS, ReportInstrMsgGenMQueue pRptMsgGenMqueue, Cst.OTCml_TBL pTableGroup)
        {
            Nullable<Int32> ret = null;

            string parameterName = string.Empty;
            switch (pTableGroup)
            {
                case Cst.OTCml_TBL.GACTOR:
                    parameterName = ReportInstrMsgGenMQueue.PARAM_GACTOR;
                    break;
                case Cst.OTCml_TBL.GBOOK:
                    parameterName = ReportInstrMsgGenMQueue.PARAM_GBOOK;
                    break;
                case Cst.OTCml_TBL.GMARKET:
                    parameterName = ReportInstrMsgGenMQueue.PARAM_GMARKET;
                    break;
            }

            MQueueparameter parameter = pRptMsgGenMqueue.parameters[parameterName];
            if (null == parameter)
                parameter = pRptMsgGenMqueue.parameters[StrFunc.AppendFormat("{0}_IDENTIFIER", parameterName)];


            if (null != parameter)
            {
                if (parameter.dataType == TypeData.TypeDataEnum.integer)
                {
                    ret = pRptMsgGenMqueue.GetIntValueParameterById(parameter.id);
                    if (ret == 0)
                        ret = null;
                }
                else
                {
                    string gIdentifier = pRptMsgGenMqueue.GetStringValueParameterById(parameter.id);
                    if (StrFunc.IsFilled(gIdentifier))
                    {
                        SQL_Group sqlg = new SQL_Group(pCS, pTableGroup, gIdentifier);
                        if (sqlg.LoadTable(new string[] { OTCmlHelper.GetColunmID(pTableGroup.ToString()) }))
                            ret = sqlg.Id;
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pRptMsgGenMqueue"></param>
        /// FI 20120829 [18048] lecture du paramètre PARAM_DATE2
        /// FI 20120903 [17773] Modification de signature de la fonction => ajout du paramètre pCS
        /// EG 20121203 Paramètres nullable dans lecas d'un message en provenance de la factory
        /// FI 20141230 [20616] Modify gestion idGActorDealer, idActorDealer, idGBookDealer, idBookDealer
        /// FI 20150427 [20987] Modify
        /// RD 20150904 Modify
        private static ReportInstrMsgGenProcessSettings LoadProcessSettings(string pCS, ReportInstrMsgGenMQueue pRptMsgGenMqueue)
        {
            ReportInstrMsgGenProcessSettings ret = new ReportInstrMsgGenProcessSettings
            {
                notificationType = GetNotificationTypeEnum(pRptMsgGenMqueue),
                notificationClass = GetNotificationClassEnum(pRptMsgGenMqueue)
            };

            // FI 20150427 [20987] pour l'instant PARAM_DTBUSINESS ou PARAM_DATE1 accepté (tant que NormMsgFactory n'a pas évolué pour gérer PARAM_DATE1 et PARAM_DATE2)
            if (null != pRptMsgGenMqueue.parameters[ReportInstrMsgGenMQueue.PARAM_DATE1])
                ret.date1 = GetDate(pRptMsgGenMqueue, ReportInstrMsgGenMQueue.PARAM_DATE1);
            else if (null != pRptMsgGenMqueue.parameters[ReportInstrMsgGenMQueue.PARAM_DTBUSINESS])
                ret.date1 = GetDate(pRptMsgGenMqueue, ReportInstrMsgGenMQueue.PARAM_DTBUSINESS);
            if (DateTime.MinValue  == ret.date1)
                throw new Exception(StrFunc.AppendFormat("Message queue without date parameter"));

            switch (ret.notificationType)
            {
                // RD 20150904 Ajouter NotificationTypeEnum.FINANCIAL pour valoriser PARAM_DATE2 pour le report NotificationTypeEnum.FINANCIAL
                case NotificationTypeEnum.FINANCIAL:
                case NotificationTypeEnum.FINANCIALPERIODIC:
                case NotificationTypeEnum.SYNTHESIS:
                    ret.date2 = ret.date1;
                    if (null != pRptMsgGenMqueue.parameters[ReportInstrMsgGenMQueue.PARAM_DATE2])
                        ret.date2 = GetDate(pRptMsgGenMqueue, ReportInstrMsgGenMQueue.PARAM_DATE2);
                    break;
            }

            ret.idAEntity = GetEntity(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue);
            ret.idA = GetActor(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue, true );

            if (NotificationClassEnum.MULTITRADES == ret.notificationClass)
            {
                ret.idB = GetBook(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue);
                if (ret.idB ==0)
                    throw new Exception(StrFunc.AppendFormat("Message queue without book parameter"));
            }
            
            if (NotificationClassEnum.MULTIPARTIES == ret.notificationClass)
            {
                ret.idGActorDealer = GetGroupe(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue, Cst.OTCml_TBL.GACTOR);   
                ret.idActorDealer = GetActor(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue, false);

                ret.idGBookDealer = GetGroupe(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue, Cst.OTCml_TBL.GBOOK);
                ret.idBookDealer = GetBook(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue);
                
            }

            ret.idGMarket = GetGroupe(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue, Cst.OTCml_TBL.GMARKET );
            ret.idMarket = GetMarket(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue);
            
            ret.isWithIO = pRptMsgGenMqueue.GetBoolValueParameterById(ReportInstrMsgGenMQueue.PARAM_ISWITHIO);

            return ret;
        }

        /// <summary>
        /// Chargement de la chaîne de confirmation
        /// </summary>
        private void LoadConfirmationChain()
        {
            ConfirmationChain confirmationChain = new ConfirmationChain();
            //SendBy
            int idASendBy = _settings.idAEntity;
            int idASendByCO = ConfirmationTools.GetContactOfficeIdA(CSTools.SetCacheOn(Cs), null, idASendBy, null);
            confirmationChain[SendEnum.SendBy].LoadActor(CSTools.SetCacheOn(Cs), idASendBy);
            confirmationChain[SendEnum.SendBy].LoadContactOffice(CSTools.SetCacheOn(Cs), idASendByCO);

            //SendTo
            int idASendTO = _settings.idA;
            // EG 20150828 [21021][21288]
            Nullable<int> idBSendTO = null;
            if (_settings.notificationClass == NotificationClassEnum.MULTITRADES)
                idBSendTO = _settings.idB;
            int idASendToCO = ConfirmationTools.GetContactOfficeIdA(CSTools.SetCacheOn(Cs), null, idASendTO, idBSendTO);

            //Chargement de la chaîne de confirmation
            confirmationChain[SendEnum.SendTo].LoadActor(CSTools.SetCacheOn(Cs), idASendTO);
            confirmationChain[SendEnum.SendTo].LoadContactOffice(CSTools.SetCacheOn(Cs), idASendToCO);
            confirmationChain[SendEnum.SendTo].LoadBook(CSTools.SetCacheOn(Cs), idBSendTO);

            confirmationChain.IsSendByActor_Entity = true;
            confirmationChain.IsSendTo_Broker = false;

            _confirmationChain = new ConfirmationChainProcess(confirmationChain);

            string sendCheck = _confirmationChain.CheckConfirmationChain(SendEnum.SendBy);
            if (StrFunc.IsFilled(sendCheck))
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-03302",
                      new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.ABORTED),
                      LogTools.IdentifierAndId(MQueue.Identifier, CurrentId), "SendBy", sendCheck);
            }
            sendCheck = _confirmationChain.CheckConfirmationChain(SendEnum.SendTo);
            if (StrFunc.IsFilled(sendCheck))
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-03302",
                      new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.ABORTED),
                      LogTools.IdentifierAndId(MQueue.Identifier, CurrentId), "SendTo", sendCheck);
            }
            _confirmationChain.InitLogAddProcessLogInfoDelegate(this.ProcessState.SetErrorWarning);
        }

        /// <summary>
        /// Retourne les trades impliqués
        /// </summary>
        /// FI 20140808 [20275] Modify
        /// FI 20170913 [23417] Modify
        // EG 20180425 Analyse du code Correction [CA2202]
        private List<TradeInfo> LoadTradeReportInfo(Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            List<TradeInfo> ret = new List<TradeInfo>();
            QueryParameters qryParameters = GetQueryTrade(notificationMultiPartiesEnum);
            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    //PL 20140710 [20179] Alimentation de statusMatch et statusCheck
                    TradeStatus tradeSt = new TradeStatus();
                    tradeSt.InitializeStUsers(Cs, Convert.ToInt32(dr["IDT"]));

                    TradeInfo info = new TradeInfo
                    {
                        idT = Convert.ToInt32(dr["IDT"]),
                        identifier = Convert.ToString(dr["IDENTIFIER"]),
                        idI = Convert.ToInt32(dr["IDI"]),
                        contractId = null,
                        idC = null,
                        idM = 0,
                        statusBusiness = (Cst.StatusBusiness)Enum.Parse(typeof(Cst.StatusBusiness), dr["IDSTBUSINESS"].ToString()),
                        statusMatch = tradeSt.GetTickedTradeStUser(StatusEnum.StatusMatch, "}{"),
                        statusCheck = tradeSt.GetTickedTradeStUser(StatusEnum.StatusCheck, "}{")
                    };

                    if (dr["IDDC"] != Convert.DBNull)
                        info.contractId = new Pair<Cst.ContractCategory, int>(Cst.ContractCategory.DerivativeContract, Convert.ToInt32(dr["IDDC"]));
                    if (dr["IDC"] != Convert.DBNull)
                        info.idC = Convert.ToString(dr["IDC"]);
                    
                    //FI 20140808 add IDM
                    if (dr["IDM"] != Convert.DBNull)
                        info.idM = Convert.ToInt32(dr["IDM"]);
                 
                    ret.Add(info);
                }
            }
            return ret;
        }

        /// <summary>
        ///  Requête de chargement des trades impliqués 
        /// </summary>
        /// <returns></returns>
        private QueryParameters GetQueryTrade(Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            QueryParameters ret;
            switch (_settings.notificationType)
            {
                case NotificationTypeEnum.ALLOCATION:
                case NotificationTypeEnum.POSITION:
                case NotificationTypeEnum.POSSYNTHETIC:
                    ret = GetQueryTradeALLOC(notificationMultiPartiesEnum);
                    break;
                case NotificationTypeEnum.POSACTION:
                    ret = GetQueryTradeALLOCPosAction(notificationMultiPartiesEnum);
                    break;
                case NotificationTypeEnum.SYNTHESIS:
                case NotificationTypeEnum.FINANCIAL:
                case NotificationTypeEnum.FINANCIALPERIODIC:
                    // FI 20120731 [18048] add FINANCIALPERIODIC
                    // RD 20160912 [22447] Add notificationMultiPartiesEnum parameter
                    ret = GetQueryTradeCashBalance(notificationMultiPartiesEnum);
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", _settings.notificationType.ToString()));
            }

            return ret;
        }


        /// <summary>
        /// Retourne la requête qui charge les trades de type ALLOC
        /// </summary>
        /// <returns></returns>
        /// <param name="notificationMultiPartiesEnum"></param>
        /// FI 20120605 Modification de la requête MULTIPARTIES (utilisation de BOOKACTOR_R à la place d'une requête récursive dont la syntaxe est spécifique au moteur)
        /// RD 20120926 [18147] 
        /// Ne plus inclure dans les Avis d'opéré, les Trades "Position Opening" résultants d'une initialisation du progiciel Spheres®
        /// Ajout du critère "and ((ti.TRDTYPE is null) or (ti.TRDTYPE <> 1000) or (ti.TRDTYPE = 1000 and ti.TRDSUBTYPE is null))", au chargement des trades pour ALLOCATION
        /// RD 20121108 Ne plus inclure dans les Avis d'opéré, les Trades annulés
        /// Ajout du critère:  "and ts.IDSTACTIVATION='REGULAR'"
        /// CC 20130912 Ticket 18949, ajout variable #AND_BOOKPOSKEEPINGCRITERIA# pour ne tenir compte que des books avec gestion de position pour les reports POSITION et POSSYNTHETIC
        /// FI 20140730 [XXXXX] Modify (Tuning) 
        /// FI 20141230 [20616] Modify
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private QueryParameters GetQueryTradeALLOC(Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            QueryParameters ret = null;
            //
            if (_settings.notificationClass == NotificationClassEnum.MULTITRADES)
            {
                //FI 20140808 [20275] null pour IDM pour l'instant
                //FI 20140813 [20275] la requête s'applique aux alloc de tout type (product.FAMILY in ('ESE','LSD','RTS')) 
                //FI 20140813 [20275] TODO La devise est valorisée uniquement sur les ETDs (où chercher la devise pour les autres type d'asset ?)
                string query = @"
                select t.IDT, t.IDENTIFIER, t.IDI, assetetd.IDDC , assetetd.IDC, t.IDSTBUSINESS, t.IDM as IDM
                from dbo.TRADE t
                inner join dbo.BOOK b on (b.IDB=t.IDB_DEALER) and (b.ISRECEIVENCMSG=1) and (b.IDA_ENTITY=@ENTITY)#AND_BOOKPOSKEEPINGCRITERIA#
                left outer join dbo.VW_ASSET_ETD_EXPANDED assetetd on (assetetd.IDASSET=t.IDASSET)  and (assetetd.ASSETCATEGORY='ExchangeTradedContract')
                inner join dbo.ENTITY e on e.IDA=@ENTITY
                where exists (  select 1    
                                from dbo.EVENT e
                                inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE
                                inner join dbo.CNFMESSAGE cnfMsg on cnfMsg.EVENTCODE=e.EVENTCODE and 
                                                                    isnull(cnfMsg.EVENTTYPE, e.EVENTTYPE)=e.EVENTTYPE and
                                                                    cnfMsg.EVENTCLASS=ec.EVENTCLASS and
                                                                    cnfMsg.CNFTYPE=@CNFTYPE and 
                                                                    cnfMsg.MSGTYPE=@MSGTYPE
                                where e.IDT=t.IDT and ec.DTEVENT=@DATE1)
                and (b.IDB=@IDB) and (t.IDSTBUSINESS='ALLOC') and (t.IDSTENVIRONMENT='REGULAR') and (t.IDSTACTIVATION='REGULAR')
                #AND_MARKETCRITERIA#
                #AND_GMARKETCRITERIA#
                #AND_POSOPENINGCRITERIA#";

                User user = new User(Session.IdA, null, RoleActor.SYSADMIN);
                SessionRestrictHelper sr = new SessionRestrictHelper(user, Session.SessionId, true);
                query = sr.ReplaceKeyword(query);

                query = OTCmlHelper.ReplaceKeyword(Cs, query);

                if (_settings.notificationType == NotificationTypeEnum.ALLOCATION)
                    query = query.Replace("#AND_POSOPENINGCRITERIA#", " and ((t.TRDTYPE is null) or (t.TRDTYPE <> '1000') or (t.TRDTYPE = '1000' and t.TRDSUBTYPE is not null))");
                else
                    query = query.Replace("#AND_POSOPENINGCRITERIA#", string.Empty);

                if (_settings.notificationType == NotificationTypeEnum.POSITION || _settings.notificationType == NotificationTypeEnum.POSSYNTHETIC)
                    query = query.Replace("#AND_BOOKPOSKEEPINGCRITERIA#", " and b.ISPOSKEEPING=1");
                else
                    query = query.Replace("#AND_BOOKPOSKEEPINGCRITERIA#", string.Empty);

                DataParameters qryParameters = new DataParameters();
                qryParameters.Add(new DataParameter(Cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                qryParameters.Add(new DataParameter(Cs, "DATE1", DbType.Date), _settings.date1);
                qryParameters.Add(new DataParameter(Cs, "IDB", DbType.Int32), _settings.idB);
                qryParameters.Add(new DataParameter(Cs, "CNFTYPE", DbType.String), _settings.notificationType.ToString());
                qryParameters.Add(new DataParameter(Cs, "MSGTYPE", DbType.String), ReflectionTools.ConvertEnumToString<NotificationClassEnum>(_settings.notificationClass));

                ret = new QueryParameters(Cs, query, qryParameters);
                ret = ReplaceMarketCriteria(ret);
                ret = ReplaceGrpMarketCriteria(ret);
            }
            else if (_settings.notificationClass == NotificationClassEnum.MULTIPARTIES)
            {
                //FI 20140808 [20275] null pour IDM pour l'instant
                //FI 20140813 [20275] la requête s'applique aux alloc de tout type (product.FAMILY in ('ESE','LSD','RTS')) 
                //GLOP la devise est valorisée uniquement sur les ETDs (où chercher la devise pour les autres type d'asset ?)
                //FI 20141230 [20616] La Requête est en phase avec le fichier MCO_RIMGEN.xml. Ajout des mots clés #AND_ACTORDEALERCRITERIA# #AND_GACTORDEALERCRITERIA# #AND_BOOKDEALERCRITERIA# #AND_GBOOKDEALERCRITERIA#
                string query =
                @"
                    select t.IDT, t.IDENTIFIER, t.IDI, assetetd.IDDC , assetetd.IDC, t.IDSTBUSINESS, t.IDM
                    from 
                    (
                        select link.IDA as IDA, link.IDENTIFIERLIST, link.LEVELACTOR
                        from dbo.ACTOR a
                        inner join (select distinct IDA,IDA_ACTOR,IDENTIFIERLIST,LEVELACTOR from dbo.BOOKACTOR_R where ISPARTYCONSO=1) link on link.IDA_ACTOR = a.IDA
                        where a.IDA=@IDA and #LEVELPREDICATE#                    
                    ) actorlst
                    inner join dbo.BOOK b on (b.IDA = actorlst.IDA)  and (b.IDA_ENTITY = @ENTITY) #AND_BOOKPOSKEEPINGCRITERIA#
                    inner join dbo.ENTITY e on (e.IDA = @ENTITY)
                    inner join dbo.TRADE t on (t.IDB_DEALER = b.IDB) 
                    left outer join dbo.VW_ASSET_ETD_EXPANDED assetetd on (assetetd.IDASSET = t.IDASSET) and (assetetd.ASSETCATEGORY = 'ExchangeTradedContract')
                    where exists (  select 1  
                                    from dbo.EVENT e
                                    inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE
                                    inner join dbo.CNFMESSAGE cnfMsg on cnfMsg.EVENTCODE=e.EVENTCODE and 
                                                                    isnull(cnfMsg.EVENTTYPE, e.EVENTTYPE)=e.EVENTTYPE and
                                                                    cnfMsg.EVENTCLASS=ec.EVENTCLASS and
                                                                    cnfMsg.CNFTYPE=@CNFTYPE and 
                                                                    cnfMsg.MSGTYPE=@MSGTYPE
                                      where e.IDT=t.IDT and ec.DTEVENT=@DATE1)
                    and (t.IDSTBUSINESS = 'ALLOC') and (t.IDSTENVIRONMENT = 'REGULAR') and (t.IDSTACTIVATION = 'REGULAR')
                    #AND_MARKETCRITERIA#
                    #AND_GMARKETCRITERIA#
                    #AND_ACTORDEALERCRITERIA#
                    #AND_GACTORDEALERCRITERIA#
                    #AND_BOOKDEALERCRITERIA#
                    #AND_GBOOKDEALERCRITERIA#
                    #AND_POSOPENINGCRITERIA#";


                DataParameters qryParameters = new DataParameters();
                qryParameters.Add(new DataParameter(Cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                qryParameters.Add(new DataParameter(Cs, "DATE1", DbType.Date), _settings.date1);
                qryParameters.Add(new DataParameter(Cs, "IDA", DbType.Int32), _settings.idA);
                qryParameters.Add(new DataParameter(Cs, "CNFTYPE", DbType.String), _settings.notificationType.ToString());
                qryParameters.Add(new DataParameter(Cs, "MSGTYPE", DbType.String), ReflectionTools.ConvertEnumToString<NotificationClassEnum>(_settings.notificationClass));

                query = OTCmlHelper.ReplaceKeyword(Cs, query);

                if (_settings.notificationType == NotificationTypeEnum.ALLOCATION)
                    query = query.Replace("#AND_POSOPENINGCRITERIA#", " and ((t.TRDTYPE is null) or (t.TRDTYPE <> '1000') or (t.TRDTYPE = '1000' and t.TRDSUBTYPE is not null))");
                else
                    query = query.Replace("#AND_POSOPENINGCRITERIA#", string.Empty);

                if (_settings.notificationType == NotificationTypeEnum.POSITION || _settings.notificationType == NotificationTypeEnum.POSSYNTHETIC)
                    query = query.Replace("#AND_BOOKPOSKEEPINGCRITERIA#", " and b.ISPOSKEEPING=1");
                else
                    query = query.Replace("#AND_BOOKPOSKEEPINGCRITERIA#", string.Empty);

                if (notificationMultiPartiesEnum.Value == NotificationMultiPartiesEnum.OWN)
                {
                    //Acteur @IDA seul
                    query = query.Replace("#LEVELPREDICATE#", "LEVELACTOR=1");
                }
                else if (notificationMultiPartiesEnum.Value == NotificationMultiPartiesEnum.CHILD)
                {
                    //tous les acteurs enfants de IDA
                    query = query.Replace("#LEVELPREDICATE#", "LEVELACTOR>1");
                }
                else if (notificationMultiPartiesEnum.Value == NotificationMultiPartiesEnum.ALL)
                {
                    //tous les acteurs enfants de IDA (y compris IDA)
                    query = query.Replace("#LEVELPREDICATE#", "LEVELACTOR>=1");
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not Implemented", notificationMultiPartiesEnum.Value.ToString()));

                ret = new QueryParameters(Cs, query, qryParameters);
                
                ret = ReplaceMarketCriteria(ret);
                ret = ReplaceGrpMarketCriteria(ret);

                ret = ReplaceActorDealerCriteria(ret);
                ret = ReplaceGrpActorDealerCriteria(ret);

                ret = ReplaceBookDealerCriteria(ret);
                ret = ReplaceGrpBookDealerCriteria(ret);

            }
            return ret;
        }

        /// <summary>
        /// Retourne la requête qui charge le(s) trade(s) de type CASHBALANCE
        /// </summary>
        /// <returns></returns>
        /// FI 20120731 [18048] gestion de FINANCIALPERIODIC
        /// FI 20120829 [18048] gestion du paramètre date2
        /// FI 20130612 [18745] gestion de SYNTHESIS
        /// FI 20150427 [20987] Modify
        /// RD 20160912 [22447] Add notificationMultiPartiesEnum parameter
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20210329 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
        private QueryParameters GetQueryTradeCashBalance(Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            QueryParameters ret = null;
            
            if (_settings.notificationClass == NotificationClassEnum.MULTITRADES)
            {
                // FI 20150427 [20987] Merge des requêtes  FINANCIAL, FINANCIALPERIODIC, SYNTHESIS
                switch (_settings.notificationType)
                {
                    case NotificationTypeEnum.FINANCIAL:
                    case NotificationTypeEnum.FINANCIALPERIODIC:
                    case NotificationTypeEnum.SYNTHESIS:
                        // FI 20140808 [20275] null pour IDM 
                        // FI 20150427 [20987] Gestion du paramètre DATE2 et reecriture de la requête
                        string query =
                        @"select t.IDT, t.IDENTIFIER, i.IDI, null as IDDC , null as IDC, t.IDSTBUSINESS, null as IDM
                        from dbo.TRADE t
                        inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
                        inner join dbo.PRODUCT p on p.IDP=i.IDP and p.IDENTIFIER='cashBalance'
                        where exists (  select 1    
                                            from dbo.EVENT e
                                            inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE
                                            inner join dbo.CNFMESSAGE cnfMsg on cnfMsg.EVENTCODE=e.EVENTCODE and 
                                                                                isnull(cnfMsg.EVENTTYPE, e.EVENTTYPE)=e.EVENTTYPE and
                                                                                cnfMsg.EVENTCLASS=ec.EVENTCLASS and
                                                                                cnfMsg.CNFTYPE=@CNFTYPE and 
                                                                                cnfMsg.MSGTYPE=@MSGTYPE
                                            where e.IDT=t.IDT and ec.DTEVENT between @DATE1 and @DATE2)
                        and (t.IDSTENVIRONMENT='REGULAR') and (t.IDA_ENTITY = @ENTITY) and (t.IDB_RISK = @IDB)";
                        DataParameters qryParameters = new DataParameters();
                        qryParameters.Add(new DataParameter(Cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                        qryParameters.Add(new DataParameter(Cs, "DATE1", DbType.Date), _settings.date1);
                        qryParameters.Add(new DataParameter(Cs, "DATE2", DbType.Date), _settings.date2);
                        qryParameters.Add(new DataParameter(Cs, "IDB", DbType.Int32), _settings.idB);
                        qryParameters.Add(new DataParameter(Cs, "CNFTYPE", DbType.String), _settings.notificationType.ToString());
                        qryParameters.Add(new DataParameter(Cs, "MSGTYPE", DbType.String), ReflectionTools.ConvertEnumToString<NotificationClassEnum>(_settings.notificationClass));
                        
                        ret = new QueryParameters(Cs, query, qryParameters);
                        break;
                }
            }
            else if (_settings.notificationClass == NotificationClassEnum.MULTIPARTIES)
            {
                //Messagerie avec plusieurs CASH BALANCE n'est pas au programme
                //On verra plus tard si le besoin s'en fait sentir
                // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message
                //throw new NotImplementedException(StrFunc.AppendFormat("{0} is not Implemented", _settings.notificationClass.ToString()));
				switch (_settings.notificationType)
                {
                    case NotificationTypeEnum.FINANCIAL:
                    case NotificationTypeEnum.FINANCIALPERIODIC:
                    case NotificationTypeEnum.SYNTHESIS:
                        string query =
                        @"select t.IDT, t.IDENTIFIER, i.IDI, null as IDDC , null as IDC, t.IDSTBUSINESS, null as IDM
                        from 
                        (
                            select link.IDA as IDA, link.IDENTIFIERLIST, link.LEVELACTOR
                            from dbo.ACTOR a
                            inner join (select distinct IDA,IDA_ACTOR,IDENTIFIERLIST,LEVELACTOR from dbo.BOOKACTOR_R where (ISPARTYCONSO=1)) link on (link.IDA_ACTOR = a.IDA)
                            where (a.IDA=@IDA) and #LEVELPREDICATE#                    
                        ) actorlst
                        inner join dbo.BOOK b on (b.IDA = actorlst.IDA)  and (b.IDA_ENTITY = @ENTITY)
                        inner join dbo.TRADE t on (t.IDA_ENTITY = @ENTITY) and (t.IDB_RISK = b.IDB)
                        inner join dbo.INSTRUMENT i on (i.IDI=t.IDI) 
                        inner join dbo.PRODUCT p on (p.IDP=i.IDP) and (p.IDENTIFIER='cashBalance')
                        where exists (  select 1    
                                            from dbo.EVENT e
                                            inner join dbo.EVENTCLASS ec on (ec.IDE=e.IDE)
                                            inner join dbo.CNFMESSAGE cnfMsg on (cnfMsg.EVENTCODE=e.EVENTCODE) and 
                                                                                (isnull(cnfMsg.EVENTTYPE, e.EVENTTYPE)=e.EVENTTYPE) and
                                                                                (cnfMsg.EVENTCLASS=ec.EVENTCLASS) and
                                                                                (cnfMsg.CNFTYPE=@CNFTYPE) and 
                                                                                (cnfMsg.MSGTYPE=@MSGTYPE)
                                            where (e.IDT=t.IDT) and (ec.DTEVENT between @DATE1 and @DATE2))
                        and (t.IDSTENVIRONMENT='REGULAR')";



                        if (notificationMultiPartiesEnum.Value == NotificationMultiPartiesEnum.OWN)
                        {
                            //Acteur @IDA seul
                            query = query.Replace("#LEVELPREDICATE#", "LEVELACTOR=1");
                        }
                        else if (notificationMultiPartiesEnum.Value == NotificationMultiPartiesEnum.CHILD)
                        {
                            //tous les acteurs enfants de IDA
                            query = query.Replace("#LEVELPREDICATE#", "LEVELACTOR>1");
                        }
                        else if (notificationMultiPartiesEnum.Value == NotificationMultiPartiesEnum.ALL)
                        {
                            //tous les acteurs enfants de IDA (y compris IDA)
                            query = query.Replace("#LEVELPREDICATE#", "LEVELACTOR>=1");
                        }
                        else
                            throw new NotImplementedException(StrFunc.AppendFormat("{0} is not Implemented", notificationMultiPartiesEnum.Value.ToString()));

                        DataParameters qryParameters = new DataParameters();
                        qryParameters.Add(new DataParameter(Cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                        qryParameters.Add(new DataParameter(Cs, "DATE1", DbType.Date), _settings.date1);
                        qryParameters.Add(new DataParameter(Cs, "DATE2", DbType.Date), _settings.date2);
                        qryParameters.Add(new DataParameter(Cs, "IDA", DbType.Int32), _settings.idA);
                        qryParameters.Add(new DataParameter(Cs, "CNFTYPE", DbType.String), _settings.notificationType.ToString());
                        qryParameters.Add(new DataParameter(Cs, "MSGTYPE", DbType.String), ReflectionTools.ConvertEnumToString<NotificationClassEnum>(_settings.notificationClass));

                        ret = new QueryParameters(Cs, query, qryParameters);
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne les lignes qui seront injectées dans la table MCO
        /// </summary>
        /// <param name="notificationMultiPartiesEnum"></param>
        /// <returns></returns>
        /// FI 20170913 [23417] Modify
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        private List<TradeMcoInput> GetListTradeMcoInput(Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum, ref Cst.ErrLevel pCodeReturn)
        {
            List<TradeMcoInput> ret = new List<TradeMcoInput>();

            //Chargement des trades
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3330), 2));

            if (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref pCodeReturn))
            {
                List<TradeInfo> lstTradeReportInfo = LoadTradeReportInfo(notificationMultiPartiesEnum);
                if (ArrFunc.IsEmpty(lstTradeReportInfo))
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3331), 2));
                }
                if (ArrFunc.IsFilled(lstTradeReportInfo))
                {
                    //Chargement des instructions de confirmation pour chaque trade impliqué
                    
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3340), 2));

                    foreach (TradeInfo tradeReportInfo in lstTradeReportInfo)
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED != pCodeReturn) &&
                            (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref pCodeReturn)))
                        {

                            bool isOk = true;
                            _confirmationChain.cnfMessages = null;
                            _confirmationChain.cnfMessageToSend = null;

                            //Chargement des messages compatibles avec le trade et la chaîne de confirmation
                            //20140710 PL [TRIM 20179] 
                            // FI 20170913 [23417] Use tradeReportInfo.contractId
                            LoadMessageSettings settings = new LoadMessageSettings(
                                                             new NotificationClassEnum[] { _settings.notificationClass },
                                                             new NotificationTypeEnum[] { _settings.notificationType },
                                                                     tradeReportInfo.idI, tradeReportInfo.idM, tradeReportInfo.contractId,
                                                             tradeReportInfo.statusBusiness, tradeReportInfo.statusMatch, tradeReportInfo.statusCheck,
                                                             new NotificationStepLifeEnum[] { NotificationStepLifeEnum.EOD }, null);

                            _confirmationChain.LoadMessages(CSTools.SetCacheOn(Cs), settings);

                            //FI 20120502 Test Message non compatible avec au minimum 1 NCS
                            CheckNCSMessage();

                            isOk = (_confirmationChain.cnfMessages.Count > 0); //cela peut arriver si le message est disabled ou sans NCS
                            if (false == isOk)
                            {
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3341), 3, new LogParam(tradeReportInfo.identifier)));
                            }
                            if (isOk)
                            {
                                if (_settings.notificationType == NotificationTypeEnum.POSACTION)
                                    SetEventTriggerForPosAction();

                                //Chargement des messages pour lesquels des évènements déclencheurs existent
                                //FI 20120731 optimisation de la messagerie (La méthode LoadCnfMessageToSend effectue des select dans EVENT,EVENTCLASS etc..)
                                //Il n'est pas nécessaire d'effectuer ces selects dans le contexte de la génération des éditions
                                //Les requêtes initiales se charge déjà de cela, les trades remontés possèdent bien des évènements à la date de traitement
                                //_confirmationChain.LoadCnfMessageToSend(Cs, tradeReportInfo.idT, Tools.GetNewProductBase(), _settings.date, false);
                                LoadCnfMessageToSend();

                                //Recherche des instructions pour chaque couple Message,Ncs qui s'appliquent compte tenu des caractéristiques du trade
                                isOk = _confirmationChain.SetNcsInciChain(CSTools.SetCacheOn(Cs), tradeReportInfo);
                                if (false == isOk)
                                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03461", new ProcessState(ProcessStateTools.StatusErrorEnum));
                            }

                            if (isOk)
                                BuildListTradeMcoInput(ret, tradeReportInfo);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        ///  Mise à jour de la liste des enregistrements MCO {pLstMcoInput} en fonction du trade candidat {pTrade} 
        /// </summary>
        /// <param name="pLstMcoInput">Liste des enregistrement dans MCO</param>
        /// <param name="pTrade">Représente un trade</param>
        /// <returns></returns>
        /// FI 20120829 [18048] Modification de la signature, la méthode n'est plus static 
        /// FI 20150427 [20987] Modify
        private void BuildListTradeMcoInput(List<TradeMcoInput> pLstMcoInput, TradeInfo pTrade)
        {
            // Pour chaque Message injection des MCO pour chaque NCS retenu  
            //for (int i = 0; i < ArrFunc.Count(_confirmationChain.cnfMessageToSend); i++)
            foreach (CnfMessageToSend cnfMessageToSend in _confirmationChain.cnfMessageToSend.Where(x => ArrFunc.Count(x.NcsInciChain) > 0))
            {
                // FI 20120731 [18048] appel à SetEventTradeInfo
                // FI 20150427 [20987] nouveaux arguments
                SetEventTradeInfo(CSTools.SetCacheOn(Cs), ref pTrade, cnfMessageToSend, _settings.date1, _settings.date2);

                for (int k = 0; k < ArrFunc.Count(cnfMessageToSend.NcsInciChain); k++)
                {
                    TradeMcoInput tradeMco = null;

                    //Les trades qui donnent lieu aux mêmes instructions rentre dans un même enregistrement MCO 
                    if (ArrFunc.IsFilled(pLstMcoInput))
                    {
                        tradeMco =
                               (from item in pLstMcoInput
                                where
                                ((item.cnfMessage.idCnfMessage == cnfMessageToSend.idCnfMessage) &&
                                (item.ncs.idNcs == cnfMessageToSend.NcsInciChain[k].ncs.idNcs) &&
                                (item.idInci_SendBy == cnfMessageToSend.NcsInciChain[k].inciChain[SendEnum.SendBy].idInci) &&
                                (item.idInci_SendTo == cnfMessageToSend.NcsInciChain[k].inciChain[SendEnum.SendTo].idInci))
                                select item).FirstOrDefault();
                    }

                    if (null == tradeMco)
                    {
                        tradeMco = new TradeMcoInput
                        {
                            cnfChain = _confirmationChain,
                            dtMco = _settings.date1,
                            cnfMessage = cnfMessageToSend,
                            ncs = cnfMessageToSend.NcsInciChain[k].ncs,
                            idInci_SendBy = cnfMessageToSend.NcsInciChain[k].inciChain[SendEnum.SendBy].idInci,
                            idInci_SendTo = cnfMessageToSend.NcsInciChain[k].inciChain[SendEnum.SendTo].idInci
                        };
                        if (_settings.date2.CompareTo(_settings.date1) > 0)
                            tradeMco.dtMco2 = _settings.date2;
                        tradeMco.trade.Add(pTrade);

                        pLstMcoInput.Add(tradeMco);
                    }
                    else
                    {
                        tradeMco.trade.Add(pTrade);
                    }
                }
            }

        }

        /// <summary>
        /// Alimentation des tables MCO et MCODET et, le cas échéant, exportation des enregistrements insérés.
        /// </summary>
        /// <param name="lstMcoInput">Liste des enregistrements à insérer</param>
        /// <param name="notificationMultiPartiesEnum"></param>
        /// <param name="pCodeReturn"></param>
        // EG 20180525 [23979] IRQ Processing
        private void SetMCO(List<TradeMcoInput> lstMcoInput, Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum, ref Cst.ErrLevel pCodeReturn)
        {
            //Alimentation du dataset MCO pour ce qui concerne les instructions. Chaque TradeMcoInput donne naissance à un nouvel enregistrement.
            DatasetConfirmationMessage ds = LoadDsInstruction(Cs, lstMcoInput, notificationMultiPartiesEnum);

            //Alimentation des messages et mise à jour de la base de données
            BuildMessages(Cs, ds);

            //Exportation des messages
            if (_settings.isWithIO && !IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref pCodeReturn))
            {
                ExportMessages(ds);
            }
        }

        /// <summary>
        /// Alimente le datatset MCO. Chaque elément de {lstMcoInput} donne naissance à un nouvel enregistrement
        /// <para>Alimente toutes les colonnes sauf celles spécifiques au message (CNFMESSAGEXML,CNFMESSAGEXSL,etc...)</para>
        /// <para>Alimente les colonnes DTMCO,IDCNFMESSAGE, IDA_SENDBYPARTY,IDA_SENDBYOFFICE, IDINCI_SENDBY,.... </para>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="lstMcoInput"></param>
        /// <param name="notificationMultiPartiesEnum"></param>
        /// <returns></returns>
        private DatasetConfirmationMessage LoadDsInstruction(string pCS, List<TradeMcoInput> lstMcoInput,
            Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            //Mise à jopur de la Db
            DatasetConfirmationMessage ds = new DatasetConfirmationMessage(IsModeSimul);
            ds.LoadDs(CSTools.SetCacheOn(pCS), null, 0);
            
            int idMCO = 0;
            foreach (TradeMcoInput item in lstMcoInput)
            {
                idMCO++;
                ds.AddRowMCOReport(CSTools.SetCacheOn(pCS), idMCO, item.dtMco, item.dtMco2, item.cnfMessage, notificationMultiPartiesEnum, item.cnfChain, item.ncs.idNcs, item.idInci_SendBy, item.idInci_SendTo, Session.IdA);
                foreach (TradeInfo itemTrade in item.trade)
                {
                    foreach (int idE in itemTrade.idE)
                    {
                        ds.AddRowMCODET(CSTools.SetCacheOn(pCS), idMCO, idE, this.Session.IdA);
                    }
                }
            }
            
            ds.SetIDT(pCS, this.Session.SessionId);
            
            return ds;
        }

        /// <summary>
        ///  Génère les messages pour chaque enregistrement MCO
        ///  <para>Effectue les mise à jour dans la base de donnée</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="ds"></param>
        /// RD 20160411 [22069] Modify
        /// FI 20160624 [22286] Modify 
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void BuildMessages(string pCS, DatasetConfirmationMessage ds)
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3369), 2));

            DatasetConfirmationMessageManager mcoMng = new DatasetConfirmationMessageManager(ds, this);
            mcoMng.InitLogAddProcessLogInfoDelegate(LogLevelDetail.LEVEL4);

            //Génération des IDCMO
            ds.SetIdMCO(pCS, null, true);

            //Génération des messages et mise à jour de la DB
            foreach (DataRow dr in ds.DtMCO.Select())
            {
                int idMCO = Convert.ToInt32(dr["ID"]);
                // FI 20160624 [22286]  pIsLoadPrevious = true
                mcoMng.GenerateMessage(pCS, null, idMCO, true, true);
            }

        }

        /// <summary>
        /// Exporte les messages présents dans chaque enregistrement de MCO
        /// </summary>
        /// <param name="ds"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void ExportMessages(DatasetConfirmationMessage ds)
        {
            bool isOk = true;
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3380), 2));

            DatasetConfirmationMessageManager mcoMng = new DatasetConfirmationMessageManager(ds, this);
            mcoMng.InitLogAddProcessLogInfoDelegate(LogLevelDetail.LEVEL4);

            foreach (DataRow dr in ds.DtMCO.Select())
            {
                try
                {
                    int idMCO = Convert.ToInt32(dr["ID"]);
                    mcoMng.ExportationMessage(Cs, idMCO);
                }
                catch (Exception ex)
                {
                    ProcessState.AddCriticalException(ex);
                    
                    Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));

                    isOk = false;
                }
            }
            
            if (false == isOk)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03480",
                    new ProcessState(ProcessStateTools.StatusErrorEnum));
            }
        }

        /// <summary>
        /// Process de génération des messages (Mode Edition simple)
        /// </summary>
        /// FI 20150427 [20987] Modify
        /// EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel MessageMultiTradesGen()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            #region  Log title
            SQL_Book sqlBook = _confirmationChain[SendEnum.SendTo].sqlBook;
            SQL_Actor sqlEntity = _confirmationChain[SendEnum.SendBy].sqlActor;

            // FI 20150427 [20987] add arg
            string arg = DtFunc.DateTimeToStringDateISO(_settings.date1);
            if (_settings.date2.CompareTo(_settings.date1) > 0)
                arg += " to " + DtFunc.DateTimeToStringDateISO(_settings.date2);

            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3310), 1,
                new LogParam(_settings.notificationType),
                new LogParam(sqlEntity.Identifier),
                new LogParam(sqlBook.Identifier),
                new LogParam(arg)));
            #endregion

            try
            {
                //Chargement des lignes MCO à générer
                List<TradeMcoInput> lstMcoInput = GetListTradeMcoInput(null, ref ret);

                if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                {
                    if (ArrFunc.IsFilled(lstMcoInput))
                    {
                        //Alimentation des tables MCO et MCODET
                        SetMCO(lstMcoInput, null, ref ret);
                    }
                    else
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3343), 2));
                    }
                }
            }
            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // FI 20200623 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(ex);
                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 3483), 0,
                    new LogParam(_settings.notificationType),
                    new LogParam(sqlEntity.Identifier),
                    new LogParam(sqlBook.Identifier),
                    new LogParam(arg)));

                ret = Cst.ErrLevel.FAILURE;
            }

            return ret;
        }

        /// <summary>
        /// Process de génération des messages (Mode Edition consolidée)
        /// </summary>
        /// FI 20150427 [20987] Modify
        /// EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel MessageMultiPartiesGen()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            #region Log Title
            SQL_Actor sqlActor = _confirmationChain[SendEnum.SendTo].sqlActor;
            SQL_Actor sqlEntity = _confirmationChain[SendEnum.SendBy].sqlActor;

            // FI 20150427 [20987] add arg
            string arg = DtFunc.DateTimeToStringDateISO(_settings.date1);
            if (_settings.date2.CompareTo(_settings.date1) > 0)
                arg += " to " + DtFunc.DateTimeToStringDateISO(_settings.date2);

            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3320), 1,
                new LogParam(_settings.notificationType),
                new LogParam(sqlEntity.Identifier),
                new LogParam(sqlActor.Identifier),
                new LogParam(arg)));
            #endregion

            try
            {
                NotificationMultiPartiesEnum[] notificationMultiParties = sqlActor.NotificationMultiParties;
                if (ArrFunc.IsFilled(notificationMultiParties))
                {
                    foreach (NotificationMultiPartiesEnum item in notificationMultiParties)
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED == ret) || IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret))
                            break;


                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3321), 2,
                            new LogParam(Ressource.GetString("CNFCLASS_" + item.ToString()))));

                        //Chargement des lignes MCO à générer
                        List<TradeMcoInput> lstMcoInput = GetListTradeMcoInput(item, ref ret);

                        if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                            (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                        {
                            if (ArrFunc.IsFilled(lstMcoInput))
                            {
                                //Alimentation des tables MCO et MCODET
                                SetMCO(lstMcoInput, item, ref ret);
                            }
                            else
                            {
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3343), 2));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(ex);

                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 3484), 0,
                    new LogParam(_settings.notificationType),
                    new LogParam(sqlEntity.Identifier),
                    new LogParam(sqlActor.Identifier),
                    new LogParam(arg)));

                ret = Cst.ErrLevel.FAILURE;
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        /// EG 20210329 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
        private QueryParameters ReplaceGrpMarketCriteria(QueryParameters pQuery)
        {
            QueryParameters ret = pQuery;
            //
            if (null != _settings.idGMarket && _settings.idGMarket > 0)
            {
                string sqlExistGMarket = @"and exists (select 1 from dbo.MARKET m
		                                                inner join dbo.MARKETG mg on mg.IDM = m.IDM
		                                                inner join dbo.GMARKET gm on gm.IDGMARKET = mg.IDGMARKET 
		                                                inner join dbo.GMARKETROLE gmr on gmr.IDGMARKET=gm.IDGMARKET and gmr.IDROLEGMARKET = 'CNF'
		                                                where m.IDM = t.IDM  and gm.IDGMARKET=@IDGMARKET)";
                ret.Parameters.Add(new DataParameter(pQuery.Cs, "IDGMARKET", DbType.Int32), _settings.idGMarket.Value);
                ret.Query = ret.Query.Replace("#AND_GMARKETCRITERIA#", sqlExistGMarket);
            }
            else
            {
                ret.Query = ret.Query.Replace("#AND_GMARKETCRITERIA#", string.Empty);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        /// EG 20210329 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
        private QueryParameters ReplaceMarketCriteria(QueryParameters pQuery)
        {
            QueryParameters ret = pQuery;
            
            if (null != _settings.idMarket && _settings.idMarket > 0)
            {
                ret.Query = ret.Query.Replace("#AND_MARKETCRITERIA#", "and t.IDM=@IDM");
                ret.Parameters.Add(new DataParameter(pQuery.Cs, "IDM", DbType.Int32), _settings.idMarket.Value);
            }
            else
            {
                ret.Query = ret.Query.Replace("#AND_MARKETCRITERIA#", string.Empty);
            }
            return ret;
        }

        /// <summary>
        ///  Remplace le mot clé #AND_GACTORDEALERCRITERIA#
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        /// FI 20141230 [20616] Add Method
        /// EG 20210329 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
        private QueryParameters ReplaceGrpActorDealerCriteria(QueryParameters pQuery)
        {
            QueryParameters ret = pQuery;
            if (null != _settings.idGActorDealer && _settings.idGActorDealer > 0)
            {
                string sqlExist = @"and exists (select 1 from dbo.ACTOR a
		                                                inner join dbo.ACTORG ag on ag.IDA = a.IDA
		                                                inner join dbo.GACTOR ga on ga.IDGACTOR = ag.IDGACTOR 
		                                                inner join dbo.GACTORROLE gar on gar.IDGACTOR=ga.IDGACTOR and gar.IDROLEGACTOR = 'CNF'
		                                                where a.IDA = t.IDA_DEALER  and ga.IDGACTOR=@IDGACTOR_DEALER)";
                ret.Parameters.Add(new DataParameter(pQuery.Cs, "IDGACTOR_DEALER", DbType.Int32), _settings.idGActorDealer.Value);
                ret.Query = ret.Query.Replace("#AND_GACTORDEALERCRITERIA#", sqlExist);
            }
            else
            {
                ret.Query = ret.Query.Replace("#AND_GACTORDEALERCRITERIA#", string.Empty);
            }
            return ret;
        }

        /// <summary>
        ///  Remplace le mot clé #AND_ACTORDEALERCRITERIA#
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        /// FI 20141230 [20616] Add Method
        /// EG 20210329 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
        private QueryParameters ReplaceActorDealerCriteria(QueryParameters pQuery)
        {
            QueryParameters ret = pQuery;
            if (null != _settings.idActorDealer && _settings.idActorDealer > 0)
            {
                ret.Query = ret.Query.Replace("#AND_ACTORDEALERCRITERIA#", "and t.IDA_DEALER=@IDA_DEALER");
                ret.Parameters.Add(new DataParameter(pQuery.Cs, "IDA_DEALER", DbType.Int32), _settings.idActorDealer.Value);
            }
            else
            {
                ret.Query = ret.Query.Replace("#AND_ACTORDEALERCRITERIA#", string.Empty);
            }
            return ret;
        }

        /// <summary>
        ///  Remplace le mot clé #AND_GBOOKDEALERCRITERIA#
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        /// FI 20141230 [20616] Add Method
        /// EG 20210329 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
        private QueryParameters ReplaceGrpBookDealerCriteria(QueryParameters pQuery)
        {
            QueryParameters ret = pQuery;
            if (null != _settings.idGBookDealer && _settings.idGBookDealer > 0)
            {
                string sqlExist = @"and exists (select 1 from dbo.BOOK b
		                                                inner join dbo.BOOKG bg on bg.IDB = b.IDB
		                                                inner join dbo.GBOOK gb on gb.IDGBOOK = bg.IDGBOOK
		                                                inner join dbo.GBOOKROLE gbr on gbr.IDGBOOK=gb.IDGBOOK and gbr.IDROLEGBOOK = 'CNF'
		                                                where b.IDB = t.IDB_DEALER  and gb.IDGBOOK=@IDGBOOK_DEALER)";
                ret.Parameters.Add(new DataParameter(pQuery.Cs, "IDGBOOK_DEALER", DbType.Int32), _settings.idGBookDealer.Value);
                ret.Query = ret.Query.Replace("#AND_GBOOKDEALERCRITERIA#", sqlExist);
            }
            else
            {
                ret.Query = ret.Query.Replace("#AND_GBOOKDEALERCRITERIA#", string.Empty);
            }
            return ret;
        }

        /// <summary>
        ///  Remplace le mot clé #AND_BOOKDEALERCRITERIA#
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        /// FI 20141230 [20616] Add Method
        /// EG 20210329 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
        private QueryParameters ReplaceBookDealerCriteria(QueryParameters pQuery)
        {
            QueryParameters ret = pQuery;
            if (null != _settings.idBookDealer && _settings.idBookDealer > 0)
            {
                ret.Query = ret.Query.Replace("#AND_BOOKDEALERCRITERIA#", "and t.IDB_DEALER=@IDB_DEALER");
                ret.Parameters.Add(new DataParameter(pQuery.Cs, "IDB_DEALER", DbType.Int32), _settings.idBookDealer.Value);
            }
            else
            {
                ret.Query = ret.Query.Replace("#AND_BOOKDEALERCRITERIA#", string.Empty);
            }
            return ret;
        }




        /// <summary>
        /// Retourne la requête qui charge les trades de type ALLOC qui seront injecté dans une édition d'action sur POSITION
        /// </summary>
        /// <returns></returns>
        /// <param name="notificationMultiPartiesEnum"></param>
        /// FI 20120430 Les décompensations effectuées en même jour que la compensation sont prises en compte
        /// FI 20120605 Modification de la requête MULTIPARTIES (utilisation de BOOKACTOR_R à la place d'une requête récursive dont la syntaxe est spécifique au moteur)
        /// FI 20140730 [XXXXX] Tuning
        /// FI 20141230 [20616] Modify 
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private QueryParameters GetQueryTradeALLOCPosAction(Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            QueryParameters ret = null;
            
            if (_settings.notificationClass == NotificationClassEnum.MULTITRADES)
            {
                //FI 20140813 [20275] la requête s'applique aux alloc de tout type (product.FAMILY in ('ESE','LSD','RTS')) 
                //GLOP la devise est valorisée uniquement sur les ETDs (où chercher la devise pour les autres type d'asset ?)
                //FI 20140813 [20275] Spheres applique le même tuning que celui présent dans le ficher xml MCO_RIMGEN.xml (application web)
                //FI 20141230 [20616] La Requête est en phase avec le fichier MCO_RIMGEN.xml
                string query =
                @"
                select t.IDT, t.IDENTIFIER, t.IDI, assetetd.IDDC , assetetd.IDC, t.IDSTBUSINESS, t.IDM
                from dbo.TRADE t
                inner join dbo.BOOK b on b.IDB=t.IDB_DEALER and b.ISPOSKEEPING=1 and b.ISRECEIVENCMSG=1 and b.IDA_ENTITY=@ENTITY
                left outer join dbo.VW_ASSET_ETD_EXPANDED assetetd on (assetetd.IDASSET = t.IDASSET)
                inner join dbo.ENTITY e on e.IDA=@ENTITY
                where (t.IDSTBUSINESS='ALLOC') and (t.IDSTENVIRONMENT='REGULAR')  and (t.ASSETCATEGORY = 'ExchangeTradedContract') and
                exists (select 1 from dbo.CNFMESSAGE cnfMsg  where cnfMsg.CNFTYPE='POSACTION' and cnfMsg.MSGTYPE='MULTI-TRADES')
                and 
                exists(
                      select 1
                      from dbo.POSACTIONDET pad 
                      inner join dbo.POSACTION pa on pa.IDPA=pad.IDPA 
                        where 
                                ( 
                                  (pa.DTBUSINESS=@DATE1 and ((pad.DTCAN is null) or (pad.DTCAN > @DATE1)))
                                  or
                                  (pad.DTCAN=@DATE1 and pad.DTCAN>pa.DTBUSINESS and pad.CANDESCRIPTION='UnClearing')
                                )
                                and 
                                (
                                  (t.IDT=pad.IDT_BUY or t.IDT=pad.IDT_SELL)         
                                )
                      )
                and (b.IDB=@IDB)
                #AND_MARKETCRITERIA#
                #AND_GMARKETCRITERIA#
                ";
                
                
                DataParameters qryParameters = new DataParameters();
                qryParameters.Add(new DataParameter(Cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                qryParameters.Add(new DataParameter(Cs, "DATE1", DbType.Date), _settings.date1);
                qryParameters.Add(new DataParameter(Cs, "IDB", DbType.Int32), _settings.idB);
                qryParameters.Add(new DataParameter(Cs, "CNFTYPE", DbType.String), _settings.notificationType.ToString());
                qryParameters.Add(new DataParameter(Cs, "MSGTYPE", DbType.String), ReflectionTools.ConvertEnumToString<NotificationClassEnum>(_settings.notificationClass));
                
                ret = new QueryParameters(Cs, query, qryParameters);

                ret = ReplaceMarketCriteria(ret);
                ret = ReplaceGrpMarketCriteria(ret);
            }
            else if (_settings.notificationClass == NotificationClassEnum.MULTIPARTIES)
            {
                //FI 20140813 [20275] la requête s'applique aux alloc de tout type (product.FAMILY in ('ESE','LSD','RTS')) 
                //GLOP la devise est valorisée uniquement sur les ETDs (où chercher la devise pour les autres type d'asset ?) 
                //FI 20141230 [20616] La Requête est en phase avec le fichier MCO_RIMGEN.xml. Ajout des mots clés #AND_ACTORDEALERCRITERIA# #AND_GACTORDEALERCRITERIA# #AND_BOOKDEALERCRITERIA# #AND_GBOOKDEALERCRITERIA#
                string query =
                     @"
                    select t.IDT, t.IDENTIFIER, t.IDI, assetetd.IDDC , assetetd.IDC, t.IDSTBUSINESS, t.IDM
                    from 
                    (
                        select link.IDA as IDA, link.IDENTIFIERLIST, link.LEVELACTOR
                        from dbo.ACTOR a
                        inner join (select distinct IDA,IDA_ACTOR,IDENTIFIERLIST,LEVELACTOR from dbo.BOOKACTOR_R where ISPARTYCONSO=1) link on link.IDA_ACTOR = a.IDA
                        where a.IDA=@IDA and #LEVELPREDICATE#                    
                    ) actorlst                    
                    inner join dbo.BOOK b on b.IDA=actorlst.IDA  and b.IDA_ENTITY=@ENTITY and b.ISPOSKEEPING=1 
                    inner join dbo.ENTITY e on e.IDA=@ENTITY
                    inner join dbo.TRADE t on (t.IDB_DEALER = b.IDB)
                    left outer join dbo.VW_ASSET_ETD_EXPANDED assetetd on (assetetd.IDASSET = t.IDASSET)
                    where (t.IDSTBUSINESS = 'ALLOC') and (t.IDSTENVIRONMENT = 'REGULAR')  and (t.ASSETCATEGORY = 'ExchangeTradedContract') and 
                    exists (select 1 from dbo.CNFMESSAGE cnfMsg  where cnfMsg.CNFTYPE='POSACTION' and cnfMsg.MSGTYPE='MULTI-TRADES') 
                    and 
                    exists
                    (
                      select 1
                      from dbo.POSACTIONDET pad 
                      inner join dbo.POSACTION pa on pa.IDPA=pad.IDPA 
                      where 
                        ( 
                          (pa.DTBUSINESS=@DATE1 and ((pad.DTCAN is null) or (pad.DTCAN > @DATE1)))
                          or
                          (pad.DTCAN=@DATE1 and pad.DTCAN>pa.DTBUSINESS and pad.CANDESCRIPTION='UnClearing')
                        )
                        and 
                        (
                          (t.IDT=pad.IDT_BUY or t.IDT=pad.IDT_SELL)         
                        )
                    )                    
                    #AND_MARKETCRITERIA#
                    #AND_GMARKETCRITERIA#
                    #AND_ACTORDEALERCRITERIA#
                    #AND_GACTORDEALERCRITERIA#
                    #AND_BOOKDEALERCRITERIA#
                    #AND_GBOOKDEALERCRITERIA#";

                DataParameters qryParameters = new DataParameters();
                qryParameters.Add(new DataParameter(Cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                qryParameters.Add(new DataParameter(Cs, "DATE1", DbType.Date), _settings.date1);
                qryParameters.Add(new DataParameter(Cs, "IDA", DbType.Int32), _settings.idA);
                qryParameters.Add(new DataParameter(Cs, "CNFTYPE", DbType.String), _settings.notificationType.ToString());
                qryParameters.Add(new DataParameter(Cs, "MSGTYPE", DbType.String), ReflectionTools.ConvertEnumToString<NotificationClassEnum>(_settings.notificationClass));
                
                query = OTCmlHelper.ReplaceKeyword(Cs, query);
                
                if (notificationMultiPartiesEnum.Value == NotificationMultiPartiesEnum.OWN)
                {
                    //Acteur @IDA seul
                    query = query.Replace("#LEVELPREDICATE#", "LEVELACTOR=1");
                }
                else if (notificationMultiPartiesEnum.Value == NotificationMultiPartiesEnum.CHILD)
                {
                    //tous les acteurs enfants de IDA
                    query = query.Replace("#LEVELPREDICATE#", "LEVELACTOR>1");
                }
                else if (notificationMultiPartiesEnum.Value == NotificationMultiPartiesEnum.ALL)
                {
                    //tous les acteurs enfants de IDA (y compris IDA)
                    query = query.Replace("#LEVELPREDICATE#", "LEVELACTOR>=1");
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not Implemented", notificationMultiPartiesEnum.Value.ToString()));

                ret = new QueryParameters(Cs, query, qryParameters);
                
                ret = ReplaceMarketCriteria(ret);
                ret = ReplaceGrpMarketCriteria(ret);

                ret = ReplaceActorDealerCriteria(ret);
                ret = ReplaceGrpActorDealerCriteria(ret);

                ret = ReplaceBookDealerCriteria(ret);
                ret = ReplaceGrpBookDealerCriteria(ret);

            }
            return ret;
        }

        /// <summary>
        /// Définit les évènemnets trigger  présents sur les éditions "actions sur Position"
        /// </summary>
        private void SetEventTriggerForPosAction()
        {
            foreach (CnfMessage cnfMsg in _confirmationChain.cnfMessages.cnfMessage)
                ConfirmationTools.SetEventTriggerForPosAction(cnfMsg);
        }

        /// <summary>
        /// Vérifie que les messages chargés sont compatbles avec au minimum avec 1 NCS  
        /// <para>Supprime tous les messages sans NCS</para>
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void CheckNCSMessage()
        {
            //Test Message non compatible avec au minimum 1 NCS
            if (ArrFunc.IsFilled(_confirmationChain.cnfMessages.cnfMessage))
            {
                for (int i = 0; i < ArrFunc.Count(_confirmationChain.cnfMessages.cnfMessage); i++)
                {
                    if (false == _confirmationChain.cnfMessages[i].IsNcsMatching())
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.LOG, 3360), 0,
                            new LogParam(_confirmationChain.cnfMessages[i].identifier)));
                    }
                }
            }
            _confirmationChain.cnfMessages.RemoveMessageWithoutNcsMatching();
        }

        /// <summary>
        ///  Alimente _confirmationChain.cnfMessageToSend (Liste des message à envoyer)
        /// </summary>
        /// FI 20120731 [18048] add method 
        /// FI 20120830 [18048] 
        /// FI 20150427 [20987] Modify
        private void LoadCnfMessageToSend()
        {
            List<CnfMessageToSend> lstCnfMessageToSend = new List<CnfMessageToSend>();
            for (int i = 0; i < ArrFunc.Count(_confirmationChain.cnfMessages.cnfMessage); i++)
            {
                CnfMessage cnfMessage = _confirmationChain.cnfMessages.cnfMessage[i];
                CnfMessageToSend item = new CnfMessageToSend(cnfMessage);

                //FI 20150427 [20987]  dateinfo n'est plus renseigné
                //FI 20120830  item.dateInfo est renseigné uniquement s'il existe des évènement déclencheur
                //Lorsqu'il existe il sont nécessairement à la date de traitement _settings.date
                //if (null != cnfMessage.eventTrigger)
                //{
                //    DateTime dtEvent = _settings.date;
                //    DateTime dtToSend = cnfMessage.GetDateToSend(CSTools.SetCacheOn(Cs), dtEvent, _confirmationChain, Tools.GetNewProductBase());
                //    DateTime dtToSendForced = OTCmlHelper.GetAnticipatedDate(Cs, dtToSend);

                //    item.dateInfo = new NotificationSendDateInfo[] { 
                //        new NotificationSendDateInfo(dtEvent, dtToSend, dtToSendForced)};
                //}
                lstCnfMessageToSend.Add(item);
            }

            if (lstCnfMessageToSend.Count > 0)
                _confirmationChain.cnfMessageToSend = lstCnfMessageToSend.ToArray();
        }

        /// <summary>
        /// Retourne l'IDE de l'évènement TRD,DAT du trade {pIdT}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// FI 20120731 [18048] add method 
        private static int GetEVENT_TRD_DAT(string pCS, int pIdT)
        {
            int[] idE = EventRDBMSTools.GetEvents(pCS, pIdT, EventCodeEnum.TRD.ToString(),
                    EventTypeEnum.DAT.ToString(), 0, 0);
            if (ArrFunc.Count(idE) == 0)
                throw new Exception(StrFunc.AppendFormat("there is no event TRD,DAT for trade (id:{0})", pIdT));
            return idE[0];
        }

        /// <summary>
        /// Alimente l'élement idE de {pTrade} (liste des évènements associés au trade pTrade)
        /// <para>Ces évènements alimententeront MCODET</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTrade"></param>
        /// <param name="pCnfMessage"></param>
        /// FI 20120731 [18048] add method 
        private static void SetEventTradeInfo(string pCS, ref TradeInfo pTrade, CnfMessageToSend pCnfMessage,DateTime pDate, DateTime pDate2)
        {
            if (ArrFunc.IsFilled(pCnfMessage.eventTrigger))
            {
                // FI 20150427 [20987] Appel à GetTriggerEvent
                pTrade.idE = GetTriggerEvent(pCS, pTrade.idT, pCnfMessage, pDate, pDate2);
            }
            else
            {
                //lorsqu'il n'existe pas d'évènement déclencheur
                //Spheres alimente la liste à l'évènement TRD,DAT du trade=>Ceci afin d'alimenter MCODET
                //L'alimentation de MCODET est nécessaire à Spheres® pour générer un message XML (voir DatasetConfirmationMessageManager.GenerateMessage) 
                pTrade.idE = new List<int>(1)
                {
                    GetEVENT_TRD_DAT(pCS, pTrade.idT)
                };
            }
        }


        /// <summary>
        /// Retourne la liste des évènements déclencheurs du message associés au trade {pIdT}  
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <param name="pCnfMessage"></param>
        /// <param name="pDate1"></param>
        /// <param name="pDate2"></param>
        /// <returns></returns>
        /// FI 20150427 [20987] Add Method
        // EG 20180425 Analyse du code Correction [CA2202]
        private static List<int> GetTriggerEvent(string pCS, int pIdT, CnfMessage pCnfMessage, DateTime pDate1, DateTime pDate2)
        {
            List<int> lstIdE = new List<int>();

            DateTime dtStart = pDate1;
            Nullable<DateTime> dtEnd = null;
            Boolean isToEnd = false;
            if (pDate2.CompareTo(pDate1) > 0)
            {
                dtEnd = pDate2;
                isToEnd = true;
            }
            // FI 20180616 [24718] pIsUseDtEVENTForced = false;
            QueryParameters queryparameters = ConfirmationTools.GetQueryTriggerEventOfTrade(pCS, pIdT, pCnfMessage, dtStart, dtEnd, isToEnd,false);

            using (IDataReader dr = DataHelper.ExecuteReader(queryparameters.Cs, CommandType.Text, queryparameters.Query, queryparameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    int idE = Convert.ToInt32(dr["ID"]);
                    if (false == lstIdE.Contains(idE))
                        lstIdE.Add(idE);
                }

            }
            return lstIdE;
        }
        #endregion Methods
    }

    /// <summary>
    /// Représente les paramètres du traitement de génération des éditions
    /// </summary>
    /// FI 20150427 [20987] Modify
    internal struct ReportInstrMsgGenProcessSettings
    {
        #region Members
        /// <summary>
        /// Type des éditions générées 
        /// </summary>
        internal NotificationTypeEnum notificationType;

        /// <summary>
        /// Classe des éditions générées
        /// </summary>
        internal NotificationClassEnum notificationClass;

        /// <summary>
        /// Représente la date "From" 
        /// </summary>
        /// FI 20150427 [20987] Rename
        internal DateTime date1;

        /// <summary>
        /// Représente la date "To"
        /// <para>Sur les états quotidien date2 est identique à date1</para>
        /// </summary>
        // FI 20150427 [20987] date2 n'est plus nullable
        internal DateTime date2;

        /// <summary>
        /// Représente l'entité
        /// <para>Tous les trades considérés sont gérés par l'entité</para>
        /// </summary>
        internal int idAEntity;

        /// <summary>
        /// Acteur destinataire 
        /// </summary>
        internal int idA;

        /// <summary>
        /// Book destinataire (null si messagerie consolidée)
        /// </summary>
        internal Nullable<int> idB;

        /// <summary>
        /// Critère Marché 
        /// <para>Spheres® ne considère que les trades qui se négocient sur ce marché</para>
        /// </summary>
        internal Nullable<int> idMarket;

        /// <summary>
        /// Critère Groupe de Marché 
        /// <para>Spheres® ne considère que les trades qui se négocient sur les marchés du groupe</para>
        /// </summary>
        internal Nullable<int> idGMarket;

        /// <summary>
        /// Critère Dealer
        /// <para>Ce paramètre est utilisé uniquement pour les éditions consolidée</para>
        /// <para>Spheres® considère uniquement les trades négociés avec ce dealer</para>
        /// </summary>
        /// FI 20141230 [20616] Add
        internal Nullable<int> idActorDealer;

        /// <summary>
        /// Critère Groupe de Dealer
        /// <para>Ce paramètre est utilisé uniquement pour les éditions consolidée</para>
        /// <para>Spheres® considère uniquement les trades négociés avec ce groupe de dealer</para>
        /// </summary>
        /// FI 20141230 [20616] Add
        internal Nullable<int> idGActorDealer;


        /// <summary>
        /// Critère book Dealer
        /// <para>Ce paramètre est utilisé uniquement pour les éditions consolidée</para>
        /// <para>Spheres® considère uniquement les trades négociés avec ce groupe de book dealer</para>
        /// </summary>
        /// FI 20141230 [20616] Add
        internal Nullable<int> idBookDealer;

        /// <summary>
        /// Critère Groupe de book Dealer
        /// <para>Ce paramètre est utilisé uniquement pour les éditions consolidée</para>
        /// <para>Spheres® ne considère que les trades négociés avec ce groupe de book dealer</para>
        /// </summary>
        /// FI 20141230 [20616] Add
        internal Nullable<int> idGBookDealer;

        
        /// <summary>
        /// Indicateur d'exportation oui/non 
        /// </summary>
        internal bool isWithIO;



        #endregion
    }


}

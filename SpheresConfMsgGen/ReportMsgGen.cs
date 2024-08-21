#region Using Directives
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection; 

using System.IO;
using System.Xml;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.Book;
using EFS.EFSTools;

using EFS.SpheresService;
using EFS.Tuning;


using EfsML;
using EfsML.Business;
using EfsML.Confirmation;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Interface;

#endregion Using Directives

namespace EFS.Process.Confirmation
{
    /// <summary>
    /// Classe pour la génération des messages avis d'opéré (flux du Message) et Exportation ( envoi via IO)
    /// </summary>
    public class ReportMsgGenProcess : ConfMsgGenProcess
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private ReportMsgGenMQueue rptMsgGenQueue;
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
        ///  Obtient true si regeneration du message
        /// </summary>
        protected override bool isModeRegenerate
        {
            get { return rptMsgGenQueue.GetBoolValueParameterById(ReportMsgGenMQueue.PARAM_ISMODEREGENERATE); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pAppInstance"></param>
        public ReportMsgGenProcess(ReportMsgGenMQueue pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            rptMsgGenQueue = pMQueue;
        }
        #endregion Constructor
    }

    /// <summary>
    /// Traitement de génération des éditions (new génération)
    /// </summary>
    public class ReportInstrMsgGenProcess : ProcessBase
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private ReportInstrMsgGenMQueue _rptMsgGenMqueue;

        /// <summary>
        /// 
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
        /// GLOP FI MESS
        /// </summary>
        protected override bool isProcessSendMessage
        {
            get
            {
                return base.isProcessSendMessage;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override TypeLockEnum dataTypeLock
        {
            get
            {
                return TypeLockEnum.ACTOR;
            }
        }

        /// <summary>
        /// Obtient le type d'identification associé au currentId
        /// </summary>
        protected override string dataIdent
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
        /// FI 20120903 [17773] Almentation de msqueue.Id
        public ReportInstrMsgGenProcess(ReportInstrMsgGenMQueue pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            _rptMsgGenMqueue = pMQueue;
            if (false == IsProcessObserver)
                _settings = LoadProcessSettings(cs, pMQueue);

            //Si l'id est non renseigné, Spheres® effectue son alimentation pour ne pas rentrer dans SelectDatas
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
                processTuning = new ProcessTuning(cs, 0, mQueue.ProcessType, appInstance.serviceName, appInstance.HostName);
                if (processTuningSpecified)
                    logDetailEnum = processTuning.logDetailEnum;
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                if (processCall == ProcessCallEnum.Master)
                    ProcessLogAddDetail(ProcessStateTools.StatusNoneEnum, ErrorManager.DetailEnum.DEFAULT, "LOG-03300",
                    LogTools.IdentifierAndId(mQueue.LogTradeIdentifier, currentId));

                //Chargement de la chaîne de confirmation
                LoadConfirmationChain();
                //
                //FI 20120420 GLOP FI MESS
                //Normalement isOk est tjs à vrai
                //Dans le grid de lancement du process, les enregistrements retenus donneront tjs true
                //Laisser ici par sécurité
                //Lorsque _confirmationChain aura un delegate il faudra utiliser les SYSTEMMSG 
                string msg = string.Empty;
                bool isOk = _confirmationChain.IsGenerateMessage(this, CSTools.SetCacheOn(cs), _settings.notificationClass);
                if (isOk)
                {
                    if (_settings.notificationClass == NotificationClassEnum.MULTITRADES)
                    {
                        MessageMultiTradesGen();
                    }
                    else if (_settings.notificationClass == NotificationClassEnum.MULTIPARTIES)
                    {
                        MessageMultiPartiesGen();
                    }
                    else
                    {
                        throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", _settings.notificationClass.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessLogAddDetail(ex, ProcessStateTools.StatusErrorEnum, ErrorManager.DetailEnum.NONE, "SYS-03460");
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
            NotificationTypeEnum ret = (NotificationTypeEnum)Enum.Parse(typeof(NotificationTypeEnum), cnfType);
            return ret;
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
            Nullable<NotificationClassEnum> cnfClassEnum = (Nullable<NotificationClassEnum>)StringToEnum.GetEnumValue(new NotificationClassEnum(), cnfClass);
            if (null == cnfClassEnum)
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

            int ret = 0;
            if (parameter.dataType == TypeData.TypeDataEnum.integer)
            {
                ret = pRptMsgGenMqueue.GetIntValueParameterById(parameter.id);
            }
            else
            {
                string entity = pRptMsgGenMqueue.GetStringValueParameterById(parameter.id);
                SQL_Entity sqlEntity = new SQL_Entity(pCS, entity);
                if (sqlEntity.LoadTable(new string[] { sqlEntity.aliasActorTable + ".IDA" }))
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
        /// <returns></returns>
        /// <exception cref="Exception si Actor inconnu"></exception>
        /// FI 20120903 [17773] modification de la signature de la fonction
        /// gestion du paramètre IDENTIFIER s'il n'existe pas id sur le message
        private static Int32 GetActor(string pCS, ReportInstrMsgGenMQueue pRptMsgGenMqueue)
        {
            int ret = 0;
            if (pRptMsgGenMqueue.idSpecified)
            {
                ret = pRptMsgGenMqueue.id;
            }
            else if (pRptMsgGenMqueue.identifierSpecified)
            {
                SQL_Actor sqlActor = new SQL_Actor(pCS, pRptMsgGenMqueue.identifier);
                if (sqlActor.LoadTable(new string[] {"IDA"}))
                    ret = sqlActor.Id;
                else
                    throw new Exception(StrFunc.AppendFormat("Message queue identifier is not an ACTOR"));
            }
            if (ret == 0)
                throw new Exception(StrFunc.AppendFormat("Message queue without id/identifier"));
            return ret;
        }

        /// <summary>
        /// Récupère le paramètre Book
        /// </summary>
        /// <param name="pRptMsgGenMqueue"></param>
        /// <returns></returns>
        /// <exception cref="Exception si book inconnu"></exception>
        /// FI 20120903 [17773] gestion du paramètre BOOK_IDENTIFIER s'il n'existe pas le paramètre IDB
        private static Int32 GetBook(string pCS, ReportInstrMsgGenMQueue pRptMsgGenMqueue)
        {
            MQueueparameter parameter = pRptMsgGenMqueue.parameters[ReportInstrMsgGenMQueue.PARAM_BOOK];
            if (null == parameter)
                parameter = pRptMsgGenMqueue.parameters["BOOK_IDENTIFIER"];
            if (null == parameter)
                throw new Exception(StrFunc.AppendFormat("Message queue without book parameter"));


            int ret = 0;
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

            if (ret == 0)
                throw new Exception(StrFunc.AppendFormat("Message queue without book parameter"));

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
        /// FI 20120903 [17773] add GetGMarket Method
        private static Nullable<Int32> GetGMarket(string pCS, ReportInstrMsgGenMQueue pRptMsgGenMqueue)
        {
            Nullable<Int32> ret = null;

            MQueueparameter parameter = pRptMsgGenMqueue.parameters[ReportInstrMsgGenMQueue.PARAM_GMARKET];
            if (null == parameter)
                parameter = pRptMsgGenMqueue.parameters["GMARKET_IDENTIFIER"];


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
                    string gMarketIdentifier = pRptMsgGenMqueue.GetStringValueParameterById(parameter.id);
                    if (StrFunc.IsFilled(gMarketIdentifier))
                    {
                        SQL_GMarket sqlgMarket = new SQL_GMarket(pCS, gMarketIdentifier);
                        if (sqlgMarket.LoadTable(new string[] { "IDGMARKET" }))
                            ret = sqlgMarket.Id;
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
        private static ReportInstrMsgGenProcessSettings LoadProcessSettings(string pCS, ReportInstrMsgGenMQueue pRptMsgGenMqueue)
        {
            ReportInstrMsgGenProcessSettings ret = new ReportInstrMsgGenProcessSettings();
            ret.notificationType = GetNotificationTypeEnum(pRptMsgGenMqueue);
            ret.notificationClass = GetNotificationClassEnum(pRptMsgGenMqueue);


            if (NotificationTypeEnum.FINANCIALPERIODIC == ret.notificationType)
            {
                ret.date = GetDate(pRptMsgGenMqueue, ReportInstrMsgGenMQueue.PARAM_DATE1);
                ret.date2 = GetDate(pRptMsgGenMqueue, ReportInstrMsgGenMQueue.PARAM_DATE2);
            }
            else
                ret.date = GetDate(pRptMsgGenMqueue, ReportInstrMsgGenMQueue.PARAM_DTBUSINESS);

            ret.idAEntity = GetEntity(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue);

            ret.idA = GetActor(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue);

            if (NotificationClassEnum.MULTITRADES == ret.notificationClass)
                ret.idB = GetBook(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue);

            ret.idGMarket = GetGMarket(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue);
            ret.idM = GetMarket(CSTools.SetCacheOn(pCS), pRptMsgGenMqueue);
            ret.isWithIO = pRptMsgGenMqueue.GetBoolValueParameterById(ReportInstrMsgGenMQueue.PARAM_ISWITHIO);

            return ret;
        }

        /// <summary>
        /// Chargement de la châine de confirmation
        /// </summary>
        private void LoadConfirmationChain()
        {
            ConfirmationChain confirmationChain = new ConfirmationChain();
            //SendBy
            int idASendBy = _settings.idAEntity;
            int idASendByCO = ConfirmationTools.GetContactOfficeIdA(CSTools.SetCacheOn(cs), idASendBy, 0);
            confirmationChain[SendEnum.SendBy].LoadActor(CSTools.SetCacheOn(cs), idASendBy);
            confirmationChain[SendEnum.SendBy].LoadContactOffice(CSTools.SetCacheOn(cs), idASendByCO);

            //SendTo
            int idASendTO = _settings.idA;
            int idBSendTO = 0;
            if (_settings.notificationClass == NotificationClassEnum.MULTITRADES)
                idBSendTO = _settings.idB.Value;
            int idASendToCO = ConfirmationTools.GetContactOfficeIdA(CSTools.SetCacheOn(cs), idASendTO, idBSendTO);

            //Chargement de la chaîne de confirmation
            confirmationChain[SendEnum.SendTo].LoadActor(CSTools.SetCacheOn(cs), idASendTO);
            confirmationChain[SendEnum.SendTo].LoadContactOffice(CSTools.SetCacheOn(cs), idASendToCO);
            confirmationChain[SendEnum.SendTo].LoadBook(CSTools.SetCacheOn(cs), idBSendTO);

            confirmationChain.isSendByActor_Entity = true;
            confirmationChain.isSendTo_Broker = false;

            _confirmationChain = new ConfirmationChainProcess(confirmationChain);

            string sendCheck = _confirmationChain.CheckConfirmationChain(SendEnum.SendBy);
            if (StrFunc.IsFilled(sendCheck))
            {
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name, "SYS-03302",
                      new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.ABORTED), 
                      LogTools.IdentifierAndId(mQueue.LogTradeIdentifier, currentId), "SendBy", sendCheck);
            }
            sendCheck = _confirmationChain.CheckConfirmationChain(SendEnum.SendTo);
            if (StrFunc.IsFilled(sendCheck))
            {
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name, "SYS-03302",
                      new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.ABORTED), 
                      LogTools.IdentifierAndId(mQueue.LogTradeIdentifier, currentId), "SendTo", sendCheck);
            }
            _confirmationChain.InitLogAddProcessLogInfoDelegate(this.ProcessLogAddDetail);
        }

        /// <summary>
        /// Retourne les trades impliqués pour la génération du message
        /// </summary>
        private List<TradeInfo> LoadTradeReportInfo(Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            List<TradeInfo> ret = new List<TradeInfo>();
            //
            QueryParameters qryParameters = GetQueryTrade(notificationMultiPartiesEnum);
            //
            IDataReader dr = null;
            try
            {
                dr = DataHelper.ExecuteReader(cs, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
                while (dr.Read())
                {
                    TradeInfo info = new TradeInfo();
                    info.idT = Convert.ToInt32(dr["IDT"]);
                    info.identifier = Convert.ToString(dr["IDENTIFIER"]);
                    info.idI = Convert.ToInt32(dr["IDI"]);

                    info.idDC = 0;
                    if (dr["IDDC"] != Convert.DBNull)
                        info.idDC = Convert.ToInt32(dr["IDDC"]);

                    info.idC = null;
                    if (dr["IDC"] != Convert.DBNull)
                        info.idC = Convert.ToString(dr["IDC"]);

                    info.statusBusiness = (Cst.StatusBusiness)Enum.Parse(typeof(Cst.StatusBusiness), dr["IDSTBUSINESS"].ToString());

                    ret.Add(info);
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
            return ret;
        }

        /// <summary>
        ///  Requête de chargement des trades impliqués par le traitement
        /// </summary>
        /// <returns></returns>
        private QueryParameters GetQueryTrade(Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            QueryParameters ret = null;

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
                case NotificationTypeEnum.FINANCIAL:
                case NotificationTypeEnum.FINANCIALPERIODIC:
                    // FI 20120731 [18048] add FINANCIALPERIODIC
                    ret = GetQueryTradeCashBalance();
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
        /// FI 20120605 Modification de la requête MULTIPARTIES (utilisation de BOOKACTOR_R à la place d'une requête récursive dont la syntaxe est spécifique au moteur) 
        /// RD 20120926 [18147] 
        /// Ne plus inclure dans les Avis d'opéré, les Trades "Position Opening" résultants d'une initialisation du progiciel Spheres®
        /// Ajout du critère "and ((ti.TRDTYPE is null) or (ti.TRDTYPE <> '1000') or (ti.TRDTYPE = '1000' and ti.TRDSUBTYPE is null))", au chargement des trades pour 'ALLOCATION'
        /// RD 20121108 / Ne plus inclure dans les Avis d'opéré, les Trades annulés
        /// Ajout du critère:  "and ts.IDSTACTIVATION='REGULAR'"
        private QueryParameters GetQueryTradeALLOC(Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            QueryParameters ret = null;
            //
            if (_settings.notificationClass == NotificationClassEnum.MULTITRADES)
            {
                string query =
                @"
                            select t.IDT, t.IDENTIFIER, i.IDI, asset.IDDC , asset.IDC, ts.IDSTBUSINESS
                            from dbo.TRADE t
                            inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
                            inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='FUT'
                            inner join dbo.TRADEINSTRUMENT ti on ti.IDT=t.IDT and ti.INSTRUMENTNO=1#AND_POSOPENINGCRITERIA#
                            inner join dbo.TRADESTSYS ts on ts.IDT=t.IDT and ts.IDSTBUSINESS='ALLOC' and ts.IDSTENVIRONMENT='REGULAR' and ts.IDSTACTIVATION='REGULAR'
                            inner join dbo.TRADEACTOR ta on ta.IDT=t.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE='27'
                            inner join dbo.ACTOR a on a.IDA=ta.IDA
                            inner join dbo.BOOK b on b.IDB=ta.IDB and b.ISRECEIVENCMSG=1 and b.IDA_ENTITY=@ENTITY
                            inner join dbo.VW_ASSET_ETD_EXPANDED asset on asset.IDASSET=ti.IDASSET
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
                            and (b.IDB=@IDB)
                            #AND_MARKETCRITERIA#
                            #AND_GMARKETCRITERIA#
                            ";
                //
                User user = new User(appInstance.IdA, null, RoleActor.SYSADMIN);
                SessionRestrictHelper2 sr = new SessionRestrictHelper2(user, appInstance.SessionId, true);
                query = sr.ReplaceKeyword(query);
                //
                query = OTCmlHelper.ReplaceKeyword(cs, query);

                if (_settings.notificationType == NotificationTypeEnum.ALLOCATION)
                    query = query.Replace("#AND_POSOPENINGCRITERIA#", " and ((ti.TRDTYPE is null) or (ti.TRDTYPE <> '1000') or (ti.TRDTYPE = '1000' and ti.TRDSUBTYPE is not null))");
                else
                    query = query.Replace("#AND_POSOPENINGCRITERIA#", string.Empty);

                DataParameters qryParameters = new DataParameters();
                qryParameters.Add(new DataParameter(cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                qryParameters.Add(new DataParameter(cs, "DATE1", DbType.Date), _settings.date);
                qryParameters.Add(new DataParameter(cs, "IDB", DbType.Int32), _settings.idB);
                qryParameters.Add(new DataParameter(cs, "CNFTYPE", DbType.String), _settings.notificationType.ToString());
                string msgType = EnumToString.GetSerializationValue(new NotificationClassEnum().GetType(), _settings.notificationClass);
                qryParameters.Add(new DataParameter(cs, "MSGTYPE", DbType.String), msgType);
                //
                ret = new QueryParameters(cs, query, qryParameters);

                ret = ReplaceMarketCriteria(ret);
                ret = ReplaceGrpMarketCriteria(ret);
            }
            else if (_settings.notificationClass == NotificationClassEnum.MULTIPARTIES)
            {

                string query =
                @"
                    select t.IDT, t.IDENTIFIER, i.IDI, asset.IDDC , asset.IDC, ts.IDSTBUSINESS
                    from 
                    (
                        select link.IDA as IDA, link.IDENTIFIERLIST, link.LEVELACTOR
                        from dbo.ACTOR a
                        inner join (select distinct IDA,IDA_ACTOR,IDENTIFIERLIST,LEVELACTOR from dbo.BOOKACTOR_R where ISPARTYCONSO=1) link on link.IDA_ACTOR = a.IDA
                        where a.IDA=@IDA and #LEVELPREDICATE#                    
                    ) actorlst
                    inner join dbo.BOOK b on b.IDA=actorlst.IDA  and b.IDA_ENTITY=@ENTITY
                    inner join dbo.ENTITY e on e.IDA=@ENTITY
                    inner join dbo.TRADEACTOR ta on ta.IDB=b.IDB and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE='27'
                    inner join dbo.TRADE t on t.IDT=ta.IDT
                    inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
                    inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='FUT'
                    inner join dbo.TRADEINSTRUMENT ti on ti.IDT=t.IDT and ti.INSTRUMENTNO=1#AND_POSOPENINGCRITERIA#
                    inner join dbo.TRADESTSYS ts on ts.IDT=t.IDT and ts.IDSTBUSINESS='ALLOC' and ts.IDSTENVIRONMENT='REGULAR' and ts.IDSTACTIVATION='REGULAR'
                    inner join dbo.VW_ASSET_ETD_EXPANDED asset on asset.IDASSET=ti.IDASSET            
                    where exists (  select 1  
                                    from dbo.EVENT e
                                    inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE
                                    inner join dbo.CNFMESSAGE cnfMsg on cnfMsg.EVENTCODE=e.EVENTCODE and 
                                                                    isnull(cnfMsg.EVENTTYPE, e.EVENTTYPE)=e.EVENTTYPE and
                                                                    cnfMsg.EVENTCLASS=ec.EVENTCLASS and
                                                                    cnfMsg.CNFTYPE=@CNFTYPE and 
                                                                    cnfMsg.MSGTYPE=@MSGTYPE
                                      where e.IDT=t.IDT and ec.DTEVENT=@DATE1)
                    #AND_MARKETCRITERIA#
                    #AND_GMARKETCRITERIA#";


                DataParameters qryParameters = new DataParameters();
                qryParameters.Add(new DataParameter(cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                qryParameters.Add(new DataParameter(cs, "DATE1", DbType.Date), _settings.date);
                qryParameters.Add(new DataParameter(cs, "IDA", DbType.Int32), _settings.idA);
                qryParameters.Add(new DataParameter(cs, "CNFTYPE", DbType.String), _settings.notificationType.ToString());
                string msgType = EnumToString.GetSerializationValue(new NotificationClassEnum().GetType(), _settings.notificationClass);
                qryParameters.Add(new DataParameter(cs, "MSGTYPE", DbType.String), msgType);
                //
                query = OTCmlHelper.ReplaceKeyword(cs, query);

                if (_settings.notificationType == NotificationTypeEnum.ALLOCATION)
                    query = query.Replace("#AND_POSOPENINGCRITERIA#", " and ((ti.TRDTYPE is null) or (ti.TRDTYPE <> '1000') or (ti.TRDTYPE = '1000' and ti.TRDSUBTYPE is not null))");
                else
                    query = query.Replace("#AND_POSOPENINGCRITERIA#", string.Empty);

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

                ret = new QueryParameters(cs, query, qryParameters);
                ret = ReplaceMarketCriteria(ret);
                ret = ReplaceGrpMarketCriteria(ret);
            }
            return ret;
        }

        /// <summary>
        /// Retourne la requête qui charge le trade de type CASHBALANCE
        /// </summary>
        /// <returns></returns>
        /// FI 20120731 [18048] gestion de FINANCIALPERIODIC
        /// FI 20120829 [18048] gestion du paramètre date2
        private QueryParameters GetQueryTradeCashBalance()
        {
            QueryParameters ret = null;
            //
            if (_settings.notificationClass == NotificationClassEnum.MULTITRADES)
            {
                if (_settings.notificationType == NotificationTypeEnum.FINANCIAL)
                {
                    string query =
                    @"  select t.IDT, t.IDENTIFIER, i.IDI, null as IDDC , null as IDC, ts.IDSTBUSINESS
                        from dbo.TRADE t
                        %%SR:TRADE_JOIN%%(t.IDT)
                        inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
                        inner join dbo.PRODUCT p on p.IDP=i.IDP and p.IDENTIFIER='cashBalance'
                        inner join dbo.TRADESTSYS ts on ts.IDT=t.IDT and ts.IDSTENVIRONMENT='REGULAR'
                        inner join dbo.TRADEACTOR te on te.IDT=t.IDT and te.IDROLEACTOR='ENTITY'
                        inner join dbo.TRADEACTOR ta on ta.IDT=t.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.IDA!=te.IDA
                        inner join dbo.ACTOR a on a.IDA=ta.IDA
                        inner join dbo.BOOK b on b.IDB=ta.IDB and b.ISRECEIVENCMSG=1 and b.IDA_ENTITY=@ENTITY
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
                        and (%%SR:TRADE_WHERE_PREDICATE%%)
                        and (b.IDB=@IDB)";
                    //
                    //
                    User user = new User(appInstance.IdA, null, RoleActor.SYSADMIN);
                    SessionRestrictHelper2 sr = new SessionRestrictHelper2(user, appInstance.SessionId, true);
                    query = sr.ReplaceKeyword(query);
                    //
                    query = OTCmlHelper.ReplaceKeyword(cs, query);
                    //
                    DataParameters qryParameters = new DataParameters();
                    qryParameters.Add(new DataParameter(cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                    qryParameters.Add(new DataParameter(cs, "DATE1", DbType.Date), _settings.date);
                    qryParameters.Add(new DataParameter(cs, "IDB", DbType.Int32), _settings.idB);
                    qryParameters.Add(new DataParameter(cs, "CNFTYPE", DbType.String), _settings.notificationType.ToString());
                    string msgType = EnumToString.GetSerializationValue(new NotificationClassEnum().GetType(), _settings.notificationClass);
                    qryParameters.Add(new DataParameter(cs, "MSGTYPE", DbType.String), msgType);
                    //
                    ret = new QueryParameters(cs, query, qryParameters);
                }
                else if (_settings.notificationType == NotificationTypeEnum.FINANCIALPERIODIC)
                {
                    string query =
                    @"  select t.IDT, t.IDENTIFIER, i.IDI, null as IDDC , null as IDC, ts.IDSTBUSINESS
                        from dbo.TRADE t
                        %%SR:TRADE_JOIN%%(t.IDT)
                        inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
                        inner join dbo.PRODUCT p on p.IDP=i.IDP and p.IDENTIFIER='cashBalance'
                        inner join dbo.TRADESTSYS ts on ts.IDT=t.IDT and ts.IDSTENVIRONMENT='REGULAR'
                        inner join dbo.TRADEACTOR te on te.IDT=t.IDT and te.IDROLEACTOR='ENTITY'
                        inner join dbo.TRADEACTOR ta on ta.IDT=t.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.IDA!=te.IDA
                        inner join dbo.ACTOR a on a.IDA=ta.IDA
                        inner join dbo.BOOK b on b.IDB=ta.IDB and b.ISRECEIVENCMSG=1 and b.IDA_ENTITY=@ENTITY
                        inner join dbo.ENTITY e on e.IDA=@ENTITY
                        where t.DTBUSINESS between @DATE1 and @DATE2
                        and (%%SR:TRADE_WHERE_PREDICATE%%)
                        and (b.IDB=@IDB)";
                    
                    User user = new User(appInstance.IdA, null, RoleActor.SYSADMIN);
                    SessionRestrictHelper2 sr = new SessionRestrictHelper2(user, appInstance.SessionId, true);
                    query = sr.ReplaceKeyword(query);
                    //
                    query = OTCmlHelper.ReplaceKeyword(cs, query);
                    //
                    DataParameters qryParameters = new DataParameters();
                    qryParameters.Add(new DataParameter(cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                    qryParameters.Add(new DataParameter(cs, "DATE1", DbType.Date), _settings.date);
                    qryParameters.Add(new DataParameter(cs, "DATE2", DbType.Date), _settings.date2);
                    qryParameters.Add(new DataParameter(cs, "IDB", DbType.Int32), _settings.idB);

                    ret = new QueryParameters(cs, query, qryParameters);
                }
            }
            else if (_settings.notificationClass == NotificationClassEnum.MULTIPARTIES)
            {
                //Messagerie avec plusieurs CASH BALANCE n'est pas au programme
                //On verra plus tard sir le besoin s'en fait sentir
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not Implemented", _settings.notificationClass.ToString()));
            }
            return ret;
        }

        /// <summary>
        /// Retourne les lignes qui seront injectées dans la table MCO
        /// </summary>
        /// <param name="notificationMultiPartiesEnum"></param>
        /// <returns></returns>
        private List<TradeMcoInput> GetListTradeMcoInput(Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            List<TradeMcoInput> ret = new List<TradeMcoInput>();
            //
            //Chargement des trades
            ProcessLogAddDetail(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.DEFAULT, 2, "LOG-03330");

            List<TradeInfo> lstTradeReportInfo = LoadTradeReportInfo(notificationMultiPartiesEnum);
            if (ArrFunc.IsEmpty(lstTradeReportInfo))
                ProcessLogAddDetail(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.DEFAULT, 2, "LOG-03331");

            //Chargement des instructions de confirmation pour chaque trade impliqué
            if (ArrFunc.IsFilled(lstTradeReportInfo))
            {
                ProcessLogAddDetail(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.DEFAULT, 2, "LOG-03340"); 
                foreach (TradeInfo tradeReportInfo in lstTradeReportInfo)
                {
                    bool isOk = true;
                    _confirmationChain.cnfMessages = null;
                    _confirmationChain.cnfMessageToSend = null;
                    //
                    //Sur la chaîne de confirmation, Chgt des messages compatibles avec le trade
                    LoadMessageSettings settings = new LoadMessageSettings(
                         new NotificationClassEnum[] { _settings.notificationClass},
                         new NotificationTypeEnum[] { _settings.notificationType},
                         tradeReportInfo.idI, tradeReportInfo.idDC, tradeReportInfo.statusBusiness,
                         new NotificationStepLifeEnum[] { NotificationStepLifeEnum.EOD }, null);

                    _confirmationChain.LoadMessages(CSTools.SetCacheOn(cs), settings);

                    //FI 20120502 Test Message non compatible avec au minimum 1 NCS
                    CheckNCSMessage();
                    //
                    isOk = (_confirmationChain.cnfMessages.Count > 0); //cela peut arriver si le message est disabled ou sans NCS
                    if (false == isOk)
                        ProcessLogAddDetail(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.DEFAULT, 3, "LOG-03341",
                            tradeReportInfo.identifier);
                    //
                    if (isOk)
                    {
                        if (_settings.notificationType == NotificationTypeEnum.POSACTION)
                            SetEventTriggerForPosAction();

                        //Chargement des messages pour lesquels des évènements déclencheurs sont présents en date de traitement
                        //
                        //FI 20120731 optimisation de la messagerie (La méthode LoadCnfMessageToSend effectue des select dans EVENT,EVENTCLASS etc..)
                        //Il n'est pas nécessaire d'effectuer ces selects dans le contexte de la génération des éditions
                        //Les requêtes initiales se charge déjà de cela, les trades remontés sont ceux possèdent bien des évènements à la daet de traitement
                        //_confirmationChain.LoadCnfMessageToSend(cs, tradeReportInfo.idT, Tools.GetNewProductBase(), _settings.date, false);
                        LoadCnfMessageToSend();
                        //
                        isOk = ArrFunc.IsFilled(_confirmationChain.cnfMessageToSend);
                        if (false == isOk)
                            ProcessLogAddDetail(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.DEFAULT, 3, "LOG-03342",
                                tradeReportInfo.identifier);
                    }
                    //
                    if (isOk)
                    {
                        //Recherche des instructions pour chaque couple Message,ncs
                        isOk = _confirmationChain.SetNcsInciChain(CSTools.SetCacheOn(cs), tradeReportInfo);
                        if (false == isOk)
                        {
                            throw new SpheresException(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03461",
                                new ProcessState(ProcessStateTools.StatusErrorEnum));
                        }
                    }
                    //
                    if (isOk)
                        BuildListTradeMcoInput(ret, tradeReportInfo);
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstMcoInput"></param>
        /// <param name="pTrade"></param>
        /// <returns></returns>
        /// FI 20120829 [18048] Modification de la signature, la méthode n'est plus static 
        private void BuildListTradeMcoInput(List<TradeMcoInput> lstMcoInput, TradeInfo pTrade)
        {
            // Pour chaque Message injection des MCO pour chaque NCS retenu  
            for (int i = 0; i < ArrFunc.Count(_confirmationChain.cnfMessageToSend); i++)
            {
                CnfMessageToSend cnfMessageToSend = _confirmationChain.cnfMessageToSend[i];
                //FI 20120731 [18048] appel à SetEventTradeInfo
                SetEventTradeInfo(CSTools.SetCacheOn(cs), ref pTrade, cnfMessageToSend);
                //
                if (ArrFunc.IsFilled(cnfMessageToSend.ncsInciChain))
                {
                    for (int k = 0; k < ArrFunc.Count(cnfMessageToSend.ncsInciChain); k++)
                    {
                        TradeMcoInput tradeMco = null;
                        if (ArrFunc.IsFilled(lstMcoInput))
                        {
                            tradeMco =
                                   (from item in lstMcoInput
                                    where
                                    ((item.cnfMessage.idCnfMessage == cnfMessageToSend.idCnfMessage) &&
                                    (item.ncs.idNcs == cnfMessageToSend.ncsInciChain[k].ncs.idNcs) &&
                                    (item.idInci_SendBy == cnfMessageToSend.ncsInciChain[k].inciChain[SendEnum.SendBy].idInci) &&
                                    (item.idInci_SendTo == cnfMessageToSend.ncsInciChain[k].inciChain[SendEnum.SendTo].idInci))
                                    select item).FirstOrDefault();
                        }
                        //
                        if (null == tradeMco)
                        {
                            tradeMco = new TradeMcoInput();
                            tradeMco.cnfChain = (ConfirmationChain)_confirmationChain;
                            
                            if (null != cnfMessageToSend.eventTrigger)
                            {
                                tradeMco.dtMco = cnfMessageToSend.dateInfo[0].dateToSend;
                            }
                            else
                            {
                                tradeMco.dtMco = _settings.date;
                                if (_settings.date2.HasValue)
                                    tradeMco.dtMco2 = _settings.date2.Value;
                            }
                            tradeMco.cnfMessage = (CnfMessage)cnfMessageToSend;
                            tradeMco.ncs = cnfMessageToSend.ncsInciChain[k].ncs;
                            tradeMco.idInci_SendBy = cnfMessageToSend.ncsInciChain[k].inciChain[SendEnum.SendBy].idInci;
                            tradeMco.idInci_SendTo = cnfMessageToSend.ncsInciChain[k].inciChain[SendEnum.SendTo].idInci;
                            tradeMco.trade.Add(pTrade);
                            //
                            lstMcoInput.Add(tradeMco);
                        }
                        else
                        {
                            tradeMco.trade.Add(pTrade);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Alimente les tables MCO et MCODET et exporte les enregistrements insérés
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="lstMcoInput">Liste des enregistrements à insérer</param>
        /// <param name="notificationMultiPartiesEnum"></param>
        private void SetMCO(string pCS, List<TradeMcoInput> lstMcoInput, 
            Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            //Alimentation du dataset MCO pour ce qui concerne les instructions
            //Chaque TradeMcoInput donne naissance à un nouvel enregistrement
            DatasetConfirmationMessage ds = LoadDsInstruction(cs, lstMcoInput, notificationMultiPartiesEnum);

            //Alimentation des messages et mise à jour de la base de donnée
            BuildMessages(cs, ds);

            //Exportation des messages
            if (_settings.isWithIO)
            {
                ExportMessages(pCS, ds);
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
            DatasetConfirmationMessage ds = new DatasetConfirmationMessage(isProcessSimul);
            ds.LoadDs(CSTools.SetCacheOn(pCS), null, 0);
            //
            int idMCO = 0;
            foreach (TradeMcoInput item in lstMcoInput)
            {
                idMCO++;
                ds.AddRowMCOReport(CSTools.SetCacheOn(pCS), idMCO, item.dtMco, item.dtMco2, item.cnfMessage, notificationMultiPartiesEnum, item.cnfChain, item.ncs.idNcs, item.idInci_SendBy, item.idInci_SendTo, appInstance.IdA);
                foreach (TradeInfo itemTrade in item.trade)
                {
                    foreach (int idE in itemTrade.idE)
                    {
                        ds.AddRowMCODET(CSTools.SetCacheOn(pCS), idMCO, idE, this.appInstance.IdA);
                    }
                }
            }
            //
            ds.SetIDT(pCS, this.appInstance.SessionId);
            //
            return ds;
        }

        /// <summary>
        ///  Génère les messages pour chaque enregistrement MCO
        ///  <para>Effectue les mise à jour dans la base de donnée</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="ds"></param>
        private void BuildMessages(string pCS, DatasetConfirmationMessage ds)
        {
            ProcessLogAddDetail(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.DEFAULT, 2, "LOG-03369");

            DatasetConfirmationMessageManager mcoMng = new DatasetConfirmationMessageManager(ds, this);
            mcoMng.InitLogAddProcessLogInfoDelegate(ErrorManager.DetailEnum.FULL, this.ProcessLogAddDetail);

            IDbTransaction dbTransaction = null;
            try
            {
                dbTransaction = DataHelper.BeginTran(pCS);

                //Génération des IDCMO
                ds.SetIdMCO(pCS, dbTransaction, true);

                //Génération des messages et mise à jour de la DB
                foreach (DataRow dr in ds.dtMCO.Select())
                {
                    int idMCO = Convert.ToInt32(dr["ID"]);
                    mcoMng.GenerateMessage(pCS, dbTransaction, idMCO, true);
                }

                //Commit
                dbTransaction.Commit();
            }
            catch
            {
                try
                {
                    if (null != dbTransaction)
                        DataHelper.RollbackTran(dbTransaction);
                }
                catch { }
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                    dbTransaction.Dispose();
            }
        }

        /// <summary>
        /// Exporte les messages présents dans chaque enregistrement de MCO
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="ds"></param>
        private void ExportMessages(string pCS, DatasetConfirmationMessage ds)
        {
            bool isOk = true;

            ProcessLogAddDetail(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.DEFAULT, 2, "LOG-03380");

            DatasetConfirmationMessageManager mcoMng = new DatasetConfirmationMessageManager(ds, this);
            mcoMng.InitLogAddProcessLogInfoDelegate(ErrorManager.DetailEnum.FULL, ProcessLogAddDetail);

            foreach (DataRow dr in ds.dtMCO.Select())
            {
                try
                {
                    int idMCO = Convert.ToInt32(dr["ID"]);
                    mcoMng.ExportationMessage(cs, idMCO);
                }
                catch (Exception ex)
                {
                    ProcessLogAddDetail(ex);
                    isOk = false;
                }
            }
            //
            if (false == isOk)
            {
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03480",
                    new ProcessState(ProcessStateTools.StatusErrorEnum));
            }
        }

        /// <summary>
        /// Process de génération des message (Mode Edition simple)
        /// </summary>
        private void MessageMultiTradesGen()
        {
            #region  Log title
            SQL_Book sqlBook = _confirmationChain[SendEnum.SendTo].sqlBook;
            SQL_Actor sqlEntity = _confirmationChain[SendEnum.SendBy].sqlActor;

            ProcessLogAddDetail(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.DEFAULT, 1, "LOG-03310",
                _settings.notificationType.ToString(),
                sqlEntity.Identifier, sqlBook.Identifier, DtFunc.DateTimeToStringDateISO(_settings.date));
            #endregion

            //Chargement des lignes MCO à générer
            List<TradeMcoInput> lstMcoInput = GetListTradeMcoInput(null);
            if (ArrFunc.IsFilled(lstMcoInput))
            {
                //Alimentation des tables MCO et MCODET
                SetMCO(cs, lstMcoInput, null);
            }
        }

        /// <summary>
        /// Process de génération des message (Mode Edition consolidée)
        /// </summary>
        private void MessageMultiPartiesGen()
        {
            #region Log Title
            SQL_Actor sqlActor = _confirmationChain[SendEnum.SendTo].sqlActor;
            SQL_Actor sqlEntity = _confirmationChain[SendEnum.SendBy].sqlActor;
            ProcessLogAddDetail(ProcessStateTools.StatusNoneEnum, ErrorManager.DetailEnum.DEFAULT, 1, "LOG-03320",
                _settings.notificationType.ToString(),
                sqlEntity.Identifier, sqlActor.Identifier,
                DtFunc.DateTimeToStringDateISO(_settings.date));
            #endregion

            NotificationMultiPartiesEnum[] notificationMultiParties = sqlActor.NotificationMultiParties;
            if (ArrFunc.IsFilled(notificationMultiParties))
            {
                foreach (NotificationMultiPartiesEnum item in notificationMultiParties)
                {
                    string itemRes = Ressource.GetString("CNFCLASS_" + item.ToString());
                    ProcessLogAddDetail(ProcessStateTools.StatusUnknownEnum, ErrorManager.DetailEnum.DEFAULT, 2, "LOG-03321", itemRes);

                    //Chargement des lignes MCO à générer
                    List<TradeMcoInput> lstMcoInput = GetListTradeMcoInput(item);

                    if (ArrFunc.IsFilled(lstMcoInput))
                    {
                        //Alimentation des tables MCO et MCODET
                        SetMCO(cs, lstMcoInput, item);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private QueryParameters ReplaceGrpMarketCriteria(QueryParameters pQuery)
        {
            QueryParameters ret = pQuery;
            //
            if (null != _settings.idGMarket && _settings.idGMarket > 0)
            {
                string sqlExistGMarket = string.Empty;
                sqlExistGMarket = @"and exists (select 1 from dbo.MARKET m
		                                                inner join dbo.MARKETG mg on mg.IDM = m.IDM
		                                                inner join dbo.GMARKET gm on gm.IDGMARKET = mg.IDGMARKET 
		                                                inner join dbo.GMARKETROLE gmr on gmr.IDGMARKET=gm.IDGMARKET and gmr.IDROLEGMARKET = 'CNF'
		                                                where m.IDM = ti.IDM  and gm.IDGMARKET=@IDGMARKET)";
                ret.parameters.Add(new DataParameter(pQuery.cs, "IDGMARKET", DbType.Int32), _settings.idGMarket.Value);
                ret.query = ret.query.Replace("#AND_GMARKETCRITERIA#", sqlExistGMarket);
            }
            else
            {
                ret.query = ret.query.Replace("#AND_GMARKETCRITERIA#", string.Empty);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private QueryParameters ReplaceMarketCriteria(QueryParameters pQuery)
        {
            QueryParameters ret = pQuery;
            //
            if (null != _settings.idM && _settings.idM > 0)
            {
                ret.query = ret.query.Replace("#AND_MARKETCRITERIA#", "and ti.IDM=@IDM");
                ret.parameters.Add(new DataParameter(pQuery.cs, "IDM", DbType.Int32), _settings.idM.Value);
            }
            else
            {
                ret.query = ret.query.Replace("#AND_MARKETCRITERIA#", string.Empty);
            }
            return ret;
        }

        /// <summary>
        /// Retourne la requête qui charge les trades de type ALLOC qui seront injecté dans une édition d'action sur POSITION
        /// </summary>
        /// <returns></returns>
        /// FI 20120430 Les décompensations effectuées en même jour que la compensation sont prises en compte
        /// FI 20120605 Modification de la requête MULTIPARTIES (utilisation de BOOKACTOR_R à la place d'une requête récursive dont la syntaxe est spécifique au moteur)
        private QueryParameters GetQueryTradeALLOCPosAction(Nullable<NotificationMultiPartiesEnum> notificationMultiPartiesEnum)
        {
            QueryParameters ret = null;
            //
            if (_settings.notificationClass == NotificationClassEnum.MULTITRADES)
            {
                string query =
                @"
                select t.IDT, t.IDENTIFIER, i.IDI, asset.IDDC , asset.IDC, ts.IDSTBUSINESS
                from dbo.TRADE t
                inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
                inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='FUT'
                inner join dbo.TRADEINSTRUMENT ti on ti.IDT=t.IDT and ti.INSTRUMENTNO=1
                inner join dbo.TRADESTSYS ts on ts.IDT=t.IDT and ts.IDSTBUSINESS='ALLOC' and ts.IDSTENVIRONMENT='REGULAR'
                inner join dbo.TRADEACTOR ta on ta.IDT=t.IDT and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE='27'
                inner join dbo.ACTOR a on a.IDA=ta.IDA
                inner join dbo.BOOK b on b.IDB=ta.IDB and b.ISRECEIVENCMSG=1 and b.IDA_ENTITY=@ENTITY
                inner join dbo.VW_ASSET_ETD_EXPANDED asset on asset.IDASSET=ti.IDASSET
                inner join dbo.ENTITY e on e.IDA=@ENTITY
                where 
                exists (select 1 from dbo.CNFMESSAGE cnfMsg  where cnfMsg.CNFTYPE='POSACTION' and cnfMsg.MSGTYPE='MULTI-TRADES')
                and 
                exists(
                      select 1
                      from dbo.POSACTIONDET pad 
                      inner join dbo.POSACTION pa on pa.IDPA=pad.IDPA 
                      where 
                      (
                        (pa.DTBUSINESS=@DATE1)
                        or
                        (pad.DTCAN = @DATE1 and pad.CANDESCRIPTION='UnClearing')
                      )                                  
                      and  
                      ( 
                           (t.IDT =  case when pad.IDT_CLOSING=pad.IDT_BUY then 
                                         isnull(pad.IDT_SELL,pad.IDT_BUY)
                                    else 
                                         isnull(pad.IDT_BUY,pad.IDT_SELL) end)
                        or
                           (t.IDT = pad.IDT_CLOSING and pad.IDT_BUY is not null and pad.IDT_SELL is not null)
                      )
                      )
                and (b.IDB=@IDB)
                #AND_MARKETCRITERIA#
                #AND_GMARKETCRITERIA#
                ";

                //union
                //select 1 from dbo.TRADELINK tl 
                //inner join dbo.POSACTIONDET pad on isnull(pad.IDT_BUY,pad.IDT_SELL)= tl.IDT_B
                //inner join dbo.POSACTION pa on pa.IDPA=pad.IDPA 
                //where 
                //tl.IDT_A = t.IDT and 
                //                  tl.LINK in ('UnderlyerDeliveryAfterOptionExercise',
                //                  'UnderlyerDeliveryAfterAutomaticOptionExercise',
                //                  'UnderlyerDeliveryAfterOptionAssignment',
                //                  'UnderlyerDeliveryAfterAutomaticOptionAssignment',
                //                  'PositionTransfert') and
                //pa.DTBUSINESS=@DATE1

                //
                User user = new User(appInstance.IdA, null, RoleActor.SYSADMIN);
                SessionRestrictHelper2 sr = new SessionRestrictHelper2(user, appInstance.SessionId, true);
                query = sr.ReplaceKeyword(query);
                //
                query = OTCmlHelper.ReplaceKeyword(cs, query);
                //
                DataParameters qryParameters = new DataParameters();
                qryParameters.Add(new DataParameter(cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                qryParameters.Add(new DataParameter(cs, "DATE1", DbType.Date), _settings.date);
                qryParameters.Add(new DataParameter(cs, "IDB", DbType.Int32), _settings.idB);
                qryParameters.Add(new DataParameter(cs, "CNFTYPE", DbType.String), _settings.notificationType.ToString());
                string msgType = EnumToString.GetSerializationValue(new NotificationClassEnum().GetType(), _settings.notificationClass);
                qryParameters.Add(new DataParameter(cs, "MSGTYPE", DbType.String), msgType);
                //
                ret = new QueryParameters(cs, query, qryParameters);

                ret = ReplaceMarketCriteria(ret);
                ret = ReplaceGrpMarketCriteria(ret);
            }
            else if (_settings.notificationClass == NotificationClassEnum.MULTIPARTIES)
            {

                string query =
                     @"
                    select t.IDT, t.IDENTIFIER, i.IDI, asset.IDDC , asset.IDC, ts.IDSTBUSINESS
                    from 
                    (
                        select link.IDA as IDA, link.IDENTIFIERLIST, link.LEVELACTOR
                        from dbo.ACTOR a
                        inner join (select distinct IDA,IDA_ACTOR,IDENTIFIERLIST,LEVELACTOR from dbo.BOOKACTOR_R where ISPARTYCONSO=1) link on link.IDA_ACTOR = a.IDA
                        where a.IDA=@IDA and #LEVELPREDICATE#                    
                    ) actorlst                    
                    inner join dbo.BOOK b on b.IDA=actorlst.IDA  and b.IDA_ENTITY=@ENTITY
                    inner join dbo.ENTITY e on e.IDA=@ENTITY
                    inner join dbo.TRADEACTOR ta on ta.IDB=b.IDB and ta.IDROLEACTOR='COUNTERPARTY' and ta.FIXPARTYROLE='27'
                    inner join dbo.TRADE t on t.IDT=ta.IDT
                    inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
                    inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='FUT'
                    inner join dbo.TRADEINSTRUMENT ti on ti.IDT=t.IDT and ti.INSTRUMENTNO=1
                    inner join dbo.TRADESTSYS ts on ts.IDT=t.IDT and ts.IDSTBUSINESS='ALLOC' and ts.IDSTENVIRONMENT='REGULAR'
                    inner join dbo.VW_ASSET_ETD_EXPANDED asset on asset.IDASSET=ti.IDASSET            
                    where 
                    exists (select 1 from dbo.CNFMESSAGE cnfMsg  where cnfMsg.CNFTYPE='POSACTION' and cnfMsg.MSGTYPE='MULTI-TRADES') 
                    and 
                    exists
                    (
                      select 1
                      from dbo.POSACTIONDET pad 
                      inner join dbo.POSACTION pa on pa.IDPA=pad.IDPA 
                      where 
                      (
                        (pa.DTBUSINESS=@DATE1)
                        or
                        (pad.DTCAN = @DATE1 and pad.CANDESCRIPTION='UnClearing')
                      )                                  
                      and  
                      ( 
                           (t.IDT =  case when pad.IDT_CLOSING=pad.IDT_BUY then 
                                         isnull(pad.IDT_SELL,pad.IDT_BUY)
                                    else 
                                         isnull(pad.IDT_BUY,pad.IDT_SELL) end)
                        or
                           (t.IDT = pad.IDT_CLOSING and pad.IDT_BUY is not null and pad.IDT_SELL is not null)
                      )
                    )                    
                    #AND_MARKETCRITERIA#
                    #AND_GMARKETCRITERIA#";

                DataParameters qryParameters = new DataParameters();
                qryParameters.Add(new DataParameter(cs, MQueueBase.PARAM_ENTITY, DbType.Int32), _settings.idAEntity);
                qryParameters.Add(new DataParameter(cs, "DATE1", DbType.Date), _settings.date);
                qryParameters.Add(new DataParameter(cs, "IDA", DbType.Int32), _settings.idA);
                qryParameters.Add(new DataParameter(cs, "CNFTYPE", DbType.String), _settings.notificationType.ToString());
                string msgType = EnumToString.GetSerializationValue(new NotificationClassEnum().GetType(), _settings.notificationClass);
                qryParameters.Add(new DataParameter(cs, "MSGTYPE", DbType.String), msgType);
                //
                query = OTCmlHelper.ReplaceKeyword(cs, query);
                //
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

                ret = new QueryParameters(cs, query, qryParameters);
                ret = ReplaceMarketCriteria(ret);
                ret = ReplaceGrpMarketCriteria(ret);

            }
            return ret;
        }

        /// <summary>
        /// Définit les évènemnets trigger  présents sur les éditions "actions sur Position"
        /// </summary>
        private void SetEventTriggerForPosAction()
        {
            if (_settings.notificationType == NotificationTypeEnum.POSACTION)
            {
                foreach (CnfMessage cnfMsg in _confirmationChain.cnfMessages.cnfMessage)
                {
                    ConfirmationTools.SetEventTriggerForPosAction(cnfMsg);
                }
            }
        }

        /// <summary>
        /// Vérifie que les messages chargés sont compatbles avec au minimum avec 1 NCS  
        /// <para>Supprime tous les messages sans NCS</para>
        /// </summary>
        private void CheckNCSMessage()
        {
            //Test Message non compatible avec au minimum 1 NCS
            if (ArrFunc.IsFilled(_confirmationChain.cnfMessages.cnfMessage))
            {
                for (int i = 0; i < ArrFunc.Count(_confirmationChain.cnfMessages.cnfMessage); i++)
                {
                    if (false == _confirmationChain.cnfMessages[i].isNcsMatching())
                    {
                        ProcessLogAddDetail(ProcessStateTools.StatusErrorEnum, ErrorManager.DetailEnum.DEFAULT, "LOG-03360", 
                            _confirmationChain.cnfMessages[i].identifier);
                    }
                }
            }
            _confirmationChain.cnfMessages.RemoveMessageWithoutNcsMatching();
        }
        
        /// <summary>
        ///  Alimente _confirmationChain.cnfMessageToSend
        /// </summary>
        /// FI 20120731 [18048] add method 
        /// FI 20120830 [18048] 
        private void LoadCnfMessageToSend()
        {
            List<CnfMessageToSend> lstCnfMessageToSend = new List<CnfMessageToSend>();
            for (int i = 0; i < ArrFunc.Count(_confirmationChain.cnfMessages.cnfMessage); i++)
            {
                CnfMessage cnfMessage = _confirmationChain.cnfMessages.cnfMessage[i];
                CnfMessageToSend item = new CnfMessageToSend(cnfMessage);

                //FI 20120830  item.dateInfo n'est renseigné uniquement s'il existe des évènement déclencheur
                //Lorsqu'il existe il sont nécessairement à la date de traitement _settings.date
                if (null != cnfMessage.eventTrigger)
                {
                    DateTime dtEvent = _settings.date;
                    DateTime dtToSend = cnfMessage.GetDateToSend(CSTools.SetCacheOn(cs), dtEvent, _confirmationChain, Tools.GetNewProductBase());
                    DateTime dtToSendForced = OTCmlHelper.GetAnticipatedDate(cs, dtToSend);

                    item.dateInfo = new MessageSendDateInfo[] { 
                        new MessageSendDateInfo(dtEvent, dtToSend, dtToSendForced)};
                }
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
        private static void SetEventTradeInfo(string pCS, ref TradeInfo pTrade, CnfMessageToSend pCnfMessage)
        {
            if (ArrFunc.IsFilled(pCnfMessage.eventTrigger))
            {
                DateTime dtMco = pCnfMessage.dateInfo[0].dateToSend;
                pTrade.idE = new List<int>(pCnfMessage.GetTriggerEvent(pCS, dtMco, pTrade.idT));
            }
            else
            {
                //lorsqu'il n'existe pas d'évènement déclencheur
                //Spheres alimente la liste à l'évènement TRD,DAT du trade=>Ceci afin d'alimenter MCODET
                //L'alimentation de MCODET est nécessaire à Spheres® pour générer un message XML (voir DatasetConfirmationMessageManager.GenerateMessage) 
                pTrade.idE = new List<int>(1);
                pTrade.idE.Add(GetEVENT_TRD_DAT(pCS, pTrade.idT));
            }
        }
        #endregion Methods
    }

    /// <summary>
    ///  Représente les informations issues d'un trade nécessaires à la messagerie
    /// </summary>
    internal struct TradeInfo
    {
        #region Members
        /// <summary>
        /// IdT
        /// </summary>
        internal int idT;

        /// <summary>
        /// identifier du trade
        /// </summary>
        internal string identifier;

        /// <summary>
        /// Instrument
        /// </summary>
        internal int idI;

        /// <summary>
        ///  Derivative Contrat
        /// </summary>
        internal int idDC;

        /// <summary>
        ///  Devise du trade
        /// </summary>
        internal string idC;

        /// <summary>
        /// Statut business
        /// </summary>
        internal Cst.StatusBusiness statusBusiness;

        /// <summary>
        /// 
        /// </summary>
        internal int idIUnderlyer;

        /// <summary>
        /// Représente les évènements déclencheurs
        /// </summary>
        internal List<Int32> idE;
        #endregion
    }

    /// <summary>
    /// Représente les paramètres du traitement de génération des éditions
    /// </summary>
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
        /// date de traitement
        /// <para>Représente la date "From"  sur la messagerie "extrait de compte"</para>
        /// </summary>
        internal DateTime date;


        /// <summary>
        /// Représente la date "To" sur un extrait de compte
        /// <para>Renseigné uniquement sur la messagerie "extrait de compte"</para>
        /// </summary>
        internal Nullable<DateTime> date2;


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
        internal Nullable<int> idM;

        /// <summary>
        /// Critère Groupe de Marché 
        /// <para>Spheres® ne considère que les trades qui se négocient sur les marchés du groupe</para>
        /// </summary>
        internal Nullable<int> idGMarket;

        /// <summary>
        /// Indicateur d'exportation oui/non 
        /// </summary>
        internal bool isWithIO;
        #endregion
    }

    /// <summary>
    /// Représente un enregistrement MCO et les trades qui rentrent dans la construction de cet enregisteremnt
    /// </summary>
    internal class TradeMcoInput : McoInput
    {
        #region Members
        /// <summary>
        /// Liste des trades inclus dans un enregistrement MCO
        /// </summary>
        internal List<TradeInfo> trade;
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        internal TradeMcoInput()
            : base()
        {
            trade = new List<TradeInfo>();
        }
        #endregion
    }

    /// <summary>
    /// Représente un enregistrement MCO
    /// </summary>
    internal class McoInput
    {
        #region members
        /// <summary>
        /// Représente la date du Message
        /// <para>Représente la date "from" sur un message "extrait de compte"</para>
        /// </summary>
        internal DateTime dtMco;

        /// <summary>
        /// Représente la date "to" sur un message "extrait de compte"
        /// </summary>
        internal Nullable<DateTime> dtMco2;

        /// <summary>
        /// Représente le message
        /// </summary>
        internal CnfMessage cnfMessage;
        /// <summary>
        /// Représente la chaîne de confirmation
        /// </summary>
        internal ConfirmationChain cnfChain;
        /// <summary>
        /// Représente le système de messagerie 
        /// </summary>
        internal NotificationConfirmationSystem ncs;
        /// <summary>
        /// Représente l'instruction Emetteur
        /// </summary>
        internal int idInci_SendBy;
        /// <summary>
        /// Représente l'instruction Destinataire
        /// </summary>
        internal int idInci_SendTo;
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        internal McoInput()
        {
            cnfMessage = new CnfMessage();
            cnfChain = new ConfirmationChain();
            ncs = new NotificationConfirmationSystem();
        }
        #endregion
    }
}



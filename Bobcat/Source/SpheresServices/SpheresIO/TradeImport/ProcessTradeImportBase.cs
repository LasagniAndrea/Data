#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Authenticate;
using EFS.Common;
using EFS.Common.EfsSend;
using EFS.Common.IO;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.CCI;
using EFS.Import;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.TradeInformation;
using EFS.TradeInformation.Import;
//
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Interface;
//
using FixML.Enum;
//
using FpML.Interface;
#endregion

namespace EFS.SpheresIO.Trade
{
    /// <summary>
    /// Classe de base pour importation des trades
    /// Importation des Trades, des debtSecurity, et des factures
    /// </summary>
    internal class ProcessTradeImportBase : IOProcessImportBase
    {
        #region Members
        /// <summary>
        /// Représente les données à importer
        /// </summary>
        protected TradeImport _tradeImport;
        /// <summary>
        /// Représente le trade qui sera importé
        /// </summary>
        protected TradeCommonCaptureGen _captureGen;

        /// <summary>
        /// Représente les sous-jacents créés en même temps que la création/modification du trade
        /// </summary>
        protected int[] _idUnderlying;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient le trade qui sera importée
        /// </summary>
        public TradeCommonCaptureGen CaptureGen
        {
            get { return _captureGen; }
        }

        /// <summary>
        /// Obtient le trade qui sera importée
        /// </summary>
        public TradeCommonInput CommonInput
        {
            get { return CaptureGen.TradeCommonInput; }
        }


        /// <summary>
        /// Obtient l'IDT du trade généré/modifié
        /// <para>Obtient 0 si le trade n'a pas été généré/modifié ( par exemple lorsqu'une condition est non respectée)</para>
        /// </summary>
        public int IdT
        {
            get
            {
                int ret = 0;
                //FI Si le trade est corrctement généré/modifié alors CaptureGen.TradeCommonInput.identification est !=null
                if (null != CaptureGen.TradeCommonInput.Identification)
                    ret = CaptureGen.TradeCommonInput.Identification.OTCmlId;
                //
                return ret;
            }
        }

        /// <summary>
        /// Représente le type d'importation
        /// </summary>
        public override string Key
        {
            get
            {
                return TradeKey;
            }
        }

        /// <summary>
        /// Représente le type de trade importé
        /// </summary>
        public virtual string TradeKey
        {
            get { return "Trade"; }
        }

        /// <summary>
        /// Obntient true lorsque le paramétrage PARTYTEMPLATE est à exploiter 
        /// </summary>
        public virtual bool IsInitFromPartyTemplateAvailable
        {
            get { return false; }
        }

        /// <summary>
        /// Obntient true lorsque le paramétrage CLEARINGTEMPLATE est à exploiter 
        /// </summary>
        /// FI 20140815 [XXXXX] add property
        public virtual bool IsInitFromClearingTemplateAvailable
        {
            get { return false; }
        }


        /// <summary>
        /// Obtient true lorsque qu'il y a potentiellemnt des calculs de frais
        /// </summary>
        public virtual bool IsFeeCalcAvailable
        {
            get { return false; }
        }

        /// <summary>
        /// Obtient true lorsque la génération de l'identifier doit faire appel à UP_GETID
        /// </summary>
        public virtual bool IsGetNewIdForIdentifier
        {
            get { return false; }
        }

        /// <summary>
        /// Obtient true si action modification de frais
        /// <para>Action possible uniquement sur l'importation des trades de marché</para>
        /// </summary>
        /// FI 20160907 [21831] Add
        public virtual bool IsModeUpdateFeesOnly
        {
            get { return false; }
        }


        /// <summary>
        /// Obtient true si action mise à jour du trader
        /// <para>Action possible uniquement sur l'importation des trades de marché </para>
        /// <para>Utilisé exclusivement pour l'imporation de trade depuis BCS</para>
        /// </summary>
        /// FI 20170824 [23339] Add
        public virtual bool IsModeUpdateTraderOnly
        {
            get { return false; }
        }

        /// <summary>
        ///  clé pour écriture dans la trace des durée de traitement
        /// </summary>
        /// FI 20180319 [XXXXX] Modify 
        protected override string KeyTraceTime
        {
            get
            {
                return String.Format("ExtlLink/Mode: {0}/{1}", GetParameter(TradeImportCst.extlLink), _captureMode);
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTradeImport"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTask"></param>
        public ProcessTradeImportBase(TradeImport pTradeImport, IDbTransaction pDbTransaction, Task pTask) :
            base(pDbTransaction, pTask, pTradeImport.settings)
        {
            _tradeImport = pTradeImport;
            InitializeCaptureGen();
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected virtual void InitializeCaptureGen()
        {
            _captureGen = null;
        }

        /// <summary>
        /// Désactiver le trade, en mettant son statut d'activation à "DEACTIV"
        /// </summary>
        /// <param name="pSessionInfo"></param>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void DeactivateTrade(CaptureSessionInfo pSessionInfo)
        {
            CommonInput.TradeStatus.stActivation.CurrentSt = Cst.StatusActivation.DEACTIV.ToString();

            DateTime dtSysBusiness = OTCmlHelper.GetDateBusiness(_task.Cs);

            SQL_TradeCommon sqlTradeStSys = new SQL_TradeCommon(_task.Cs, this.IdT);
            string sqlQuery = sqlTradeStSys.GetQueryParameters(
                    new string[]{"IDT",
                    "IDSTENVIRONMENT", "DTSTENVIRONMENT", "IDASTENVIRONMENT",
                    "IDSTBUSINESS", "DTSTBUSINESS", "IDASTBUSINESS",
                    "IDSTACTIVATION", "DTSTACTIVATION", "IDASTACTIVATION",
                    "IDSTUSEDBY", "DTSTUSEDBY", "IDASTUSEDBY", "LIBSTUSEDBY",
                    "IDSTPRIORITY", "DTSTPRIORITY", "IDASTPRIORITY", "ROWATTRIBUT"}).QueryReplaceParameters;

            DataSet ds = DataHelper.ExecuteDataset(_task.Cs, CommandType.Text, sqlQuery);
            DataTable dt = ds.Tables[0];

            DataRow dr = dt.Rows[0];

            CommonInput.TradeStatus.UpdateRowTradeStSys(dr, pSessionInfo.user.IdA, dtSysBusiness, null);

            DataHelper.ExecuteDataAdapter(_task.Cs, sqlQuery, dt);
        }

        /// <summary>
        /// Envoi d'un messageQueue à destination de TradeActionGenMQueue pour annulation du trade
        /// </summary>
        private void SendMQueueForRemove()
        {
            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = _task.Cs,
                id = CommonInput.RemoveTrade.idTCancel,
                idInfo = CaptureTools.GetMqueueIdInfo(_task.Cs, CommonInput.RemoveTrade.idTCancel)
            };

            TradeActionGenMQueue[] mQueue = new TradeActionGenMQueue[] { new TradeActionGenMQueue(mQueueAttributes, CommonInput.RemoveTrade) };
            // RD 20121224 [18323] Pour utiliser le même ticket du tracker dans «Position Keeping »
            mQueue[0].header.requesterSpecified = (null != _task.Requester);
            mQueue[0].header.requester = _task.Requester;

            MQueueTaskInfo taskInfo = new MQueueTaskInfo
            {
                connectionString = _task.Cs,
                Session = _task.Session,
                process = mQueue[0].ProcessType,
                mQueue = (MQueueBase[])mQueue,
                sendInfo = EFS.SpheresService.ServiceTools.GetMqueueSendInfo(Cst.ProcessTypeEnum.TRADEACTGEN, _task.Process.AppInstance)
            };

            int idTRK_L = _task.Requester.idTRK;
            MQueueTaskInfo.SendMultiple(taskInfo, ref idTRK_L);

        }

        /// <summary>
        /// INSERTION / MISE A JOUR DE LA TABLE POSREQUEST 
        /// <para>- Ecriture de la demande dans la table POSREQUEST ou mise à jour si demande déjà insérée</para> 
        /// <para>- Postage d'un message PosKeepingRequestMQueue (REQUESTMODE = INTRADAY)</para> 
        /// </summary>
        /// <param name="pdbTransaction"></param>
        /// <param name="pUserIda"></param>
        /// <param name="pMsgLog"></param>
        /// <returns></returns>
        private bool SendPosRequest(out string pMsgLog)
        {
            IPosRequest posRequest = ((TradeInput)_captureGen.TradeCommonInput).NewPostRequest(_task.Cs, null, _captureMode);
            //
            Cst.ErrLevel errLevelMessage = PosKeepingTools.FillPosRequest(_task.Cs, null, posRequest, _task.Session);
            pMsgLog = posRequest.RequestMessage;

            bool isOk = (Cst.ErrLevel.SUCCESS == errLevelMessage);

            if (isOk)
            {
                if (posRequest.RequestMode == SettlSessIDEnum.Intraday)
                {
                    // SendMessage POSREQUESTMQUEUE
                    MQueueAttributes mQueueAttributes = new MQueueAttributes()
                    {
                        connectionString = _task.Cs,
                        id = posRequest.IdPR,
                        idInfo = posRequest.GetMQueueIdInfo()

                    };

                    PosKeepingRequestMQueue[] mQueue = new PosKeepingRequestMQueue[] { new PosKeepingRequestMQueue(posRequest.RequestType, mQueueAttributes) };
                    // RD 20121224 [18323] Pour utiliser le même ticket du tracker dans «Position Keeping »
                    mQueue[0].header.requesterSpecified = (null != _task.Requester);
                    mQueue[0].header.requester = _task.Requester;

                    MQueueTaskInfo taskInfo = new MQueueTaskInfo
                    {
                        connectionString = _task.Cs,
                        Session = _task.Session,
                        process = mQueue[0].ProcessType,
                        mQueue = (MQueueBase[])mQueue,
                        sendInfo = EFS.SpheresService.ServiceTools.GetMqueueSendInfo(Cst.ProcessTypeEnum.POSKEEPREQUEST, _task.Process.AppInstance)
                    };

                    int idTRK_L = _task.Requester.idTRK;
                    MQueueTaskInfo.SendMultiple(taskInfo, ref idTRK_L);
                }
            }
            return isOk;
        }

        /// <summary>
        /// Liste des sous jacents créés avec le trade 
        /// <para>Exemple création de titre depuis la saisie => La méthode retourne les IDT des titres générés</para>
        /// </summary>
        /// <returns></returns>
        public int[] GetIdUnderlying()
        {
            if (ArrFunc.IsFilled(_idUnderlying))
                return (int[])_idUnderlying;
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCaptureMode"></param>
        protected virtual void CheckValidationRule(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, User pUser)
        {
            FireException(new NotImplementedException("CheckValidationRule is not Implemented"));
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void CheckParameter()
        {
            FireException(new NotImplementedException("CheckParameter is not Implemented"));
        }

        /// <summary>
        /// initialisation de CommonInput.CustomCaptureInfos 
        /// </summary>
        protected virtual void SetCustomCaptureInfos(string pCS)
        {
        }

        /// <summary>
        /// Cette méthode doit être overridée lorsqu'il est nécessaire de modifier les clientIds 
        /// <para> Méthode nécessaire pour assurer une compatibilité ascendante en cas de modification de nommage</para>
        /// </summary>
        protected virtual void ShiftCcisClientId()
        {
        }

        /// <summary>
        /// Méthode vituelle 
        /// <para>Elle est appellée après le dump et avant CleanUP</para>
        /// <para>Peut-être overrider pour injecter des éléments dans le dataDocument</para>
        /// <para>Exemple les frais sur les trades transactions</para>
        /// </summary>
        protected virtual void ProcessSpecific()
        {
        }

        /// <summary>
        /// Détermine les paramètres de l'importation (Template, Instrument, Screen, etc...)
        /// </summary>
        /// FI 20140815 [XXXXX] Modify
        /// FI 20160907 [21831] Modify
        /// FI 20160929 [22507] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void LoadParameter()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "Loading parameters...", 4));

            #region Set Parameter

            IScheme scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.templateIdentifier);
            if (null != scheme)
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

            scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.identifier);
            if (null != scheme)
            {

                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                //PL 20180907 Tip - Test d'optimisation: pas d'exécution du select en mode New
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                bool isGetIdentifier = true;
                if (IsModeNew)
                {
                    DataSQL sql = ((ImportParameter)scheme).sql;
                    if ((sql != null) && (sql.result == "IDENTIFIER")
                        && (sql.Value.IndexOf("select t.IDENTIFIER, 0 as SORT") >= 0) && (sql.Value.IndexOf("union") < 0))
                    {
                        isGetIdentifier = false;
                        SetParameter(scheme.Scheme, null);
                    }
                }
                if (isGetIdentifier)
                    SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            }

            scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.displayName);
            if (null != scheme)
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

            scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.description);
            if (null != scheme)
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

            scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.extlLink);
            if (null != scheme)
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

            scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.screen);
            if (null != scheme)
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

            if (IsFeeCalcAvailable)
            {
                //PL 20130718 FeeCalculation Project
                //scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.isApplyFeeCalculation);
                scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.feeCalculation);
                if (null != scheme)
                {
                    SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));
                }

                // FI 20160907 [21831] Add
                scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.updateMode);
                if (null != scheme)
                {
                    SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));
                }
            }

            if (IsInitFromPartyTemplateAvailable)
            {
                scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.isApplyPartyTemplate);
                if (null != scheme)
                    SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

                // FI 20140815 [XXXXX] add isInitFromClearingTemplateAvailable
                if (IsInitFromClearingTemplateAvailable)
                {
                    scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.isApplyClearingTemplate);
                    if (null != scheme)
                        SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

                    // FI 20160929 [22507] Add isApplyReverseClearingTemplate
                    scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.isApplyReverseClearingTemplate);
                    if (null != scheme)
                        SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));
                }

            }
            // FI 20140815 [XXXXX] add isInitFromClearingTemplateAvailable
            if (IsInitFromClearingTemplateAvailable)
            {
                scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.isApplyClearingTemplate);
                if (null != scheme)
                    SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));
            }

            scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.isPostToEventsGen);
            if (null != scheme)
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

            scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.isApplyValidationRules);
            if (null != scheme)
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

            scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.isApplyValidationXSD);
            if (null != scheme)
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

            // FI 20140331 [19793] add isCopyAttachedDoc
            scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.isCopyAttachedDoc);
            if (null != scheme)
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

            // FI 20140331 [19793] add isCopyNotePad
            scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.isCopyNotePad);
            if (null != scheme)
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));

            #endregion

            //Si parametre instrument existe mais qu'il manque soit le templateIdentifier, soit le screen
            //Recherche des valeurs par défaut dans INSTRUMENTGUI
            bool isGetDefaultFromInstr = false;
            if (IsModeNew)
                isGetDefaultFromInstr = (StrFunc.IsEmpty(GetParameter(TradeImportCst.templateIdentifier)) || StrFunc.IsEmpty(GetParameter(TradeImportCst.screen)));
            else if (IsModeUpdate || IsModeRemoveOnlyAll)
                isGetDefaultFromInstr = StrFunc.IsEmpty(GetParameter(TradeImportCst.screen));

            if (isGetDefaultFromInstr)
            {
                StringData[] data = InstrTools.GetDefaultInstrumentGui(_task.Cs, GetParameter(TradeImportCst.instrumentIdentifier));
                if (ArrFunc.IsFilled(data))
                {
                    if (StrFunc.IsEmpty(GetParameter(TradeImportCst.templateIdentifier)))
                        SetParameter(TradeImportCst.templateIdentifier, ((StringData)ArrFunc.GetFirstItem(data, "TEMPLATENAME")).value);
                    //
                    if (StrFunc.IsEmpty(GetParameter(TradeImportCst.screen)))
                        SetParameter(TradeImportCst.screen, ((StringData)ArrFunc.GetFirstItem(data, "SCREENNAME")).value);
                }
            }

            //Screen
            if (StrFunc.IsEmpty(GetParameter(TradeImportCst.screen)))
                SetParameter(TradeImportCst.screen, Cst.FpML_ScreenFullCapture);

            if (IsModeNew)
            {
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Template: <b>{0}</b>, Screen: <b>{1}</b>", GetParameter(TradeImportCst.templateIdentifier), GetParameter(TradeImportCst.screen)), 4));
            }
            else if (IsModeUpdate || IsModeRemoveOnlyAll)
            {

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, StrFunc.AppendFormat(TradeKey + ": <b>{0}</b>, Screen: <b>{1}</b>", GetParameter(TradeImportCst.identifier), GetParameter(TradeImportCst.screen)), 4));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUser"></param>
        /// <param name="pInputUser"></param>
        /// FI 20130101 [] Mise en cache des données User et InputUser
        private void LoadUser(out User pUser, out InputUser pInputUser)
        {
            pUser = null;
            pInputUser = null;
            try
            {
                User user = base.GetUser();

                IdMenu.Menu idMenu = GetMenu(_captureMode);
                InputUser inputUser = LoadCacheInputUser(_task.Cs, user, idMenu);
                if (inputUser == null)
                    FireException("InputUser is null");

                // RD 20120606 [17857]
                // Le capture mode n'est pas initialisé, il reste à sa valeur par défaut: "New"
                // ce qui implique, en mode Update, la non purge des frais déjà calculés dans une précédente importation du trade
                inputUser.CaptureMode = _captureMode;
                //
                if ((IsModeNew) && (false == inputUser.IsCreateAuthorised))
                    FireException(StrFunc.AppendFormat("<b>Creation not authorized</b> due to insufficient privileges for user [identifier:{0}]", user.Identification.Identifier));

                else if ((IsModeUpdate) && (false == inputUser.IsModifyAuthorised))
                    FireException(StrFunc.AppendFormat("<b>Modification not authorized</b> due to insufficient privileges for user [identifier:{0}]", user.Identification.Identifier));

                else if ((IsModeRemoveAllocation) && (false == inputUser.IsCreateAuthorised))
                    FireException(StrFunc.AppendFormat("<b>Allocation Cancellation not authorized</b> due to insufficient privileges for user [identifier:{0}]", user.Identification.Identifier));

                else if ((IsModeRemoveOnly) && (false == inputUser.IsCreateAuthorised))
                    FireException(StrFunc.AppendFormat("<b>Cancellation not authorized</b> due to insufficient privileges for user [identifier:{0}]", user.Identification.Identifier));

                pUser = user;
                pInputUser = inputUser;
            }
            catch (Exception ex)
            {
                FireException("<b>Error on loading user</b>", ex);
            }
        }

        /// <summary>
        /// Initialise le membre _posTradeInput
        /// <para>En mode Création, charge le template</para>
        /// <para>En mode Modification, Suppression charge le trade source</para>
        /// </summary>
        /// FI 20130204 [] Mise en cache de TradeCommonInput ds l'importation en mode New uniquement
        /// FI 20120226 [] Suppression de la mise en cache TradeCommonInput
        /// FI 20180319 [XXXXX] Modify
        /// <exception cref="Lorsque que le template ou le trade source n'existe pas"></exception> 
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void InitTradeInput(CaptureSessionInfo sessionInfo)
        {
            if (IsModeNew)
            {
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Loading template: <b>{0}</b>", GetParameter(TradeImportCst.templateIdentifier)), 4));
            }
            else if (IsModeUpdate || IsModeRemoveOnlyAll)
            {

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, StrFunc.AppendFormat("Loading " + TradeKey + " <b>{0}</b>", GetParameter(TradeImportCst.identifier)), 4));
            }

            try
            {
                // FI 20180319 [XXXXX] Mise en place de TraceTime 
                AppInstance.TraceManager.TraceTimeBegin("LoadTemplate/LoadTrade", KeyTraceTime);

                string identifier = IsModeNew ? GetParameter(TradeImportCst.templateIdentifier) : GetParameter(TradeImportCst.identifier);
                string key = IsModeNew ? "template" : TradeKey;

                string csLoad = _task.Cs;
                if (IsModeNew)
                    csLoad = CSTools.SetCacheOn(_task.Cs);

                bool isFound = _captureGen.Load(csLoad, null, identifier, SQL_TableWithID.IDType.Identifier, _captureMode, sessionInfo.user, sessionInfo.session.SessionId, true);
                if (false == isFound)
                    FireException(StrFunc.AppendFormat("<b>{0} identifier {1} not found</b>", key, identifier));
            }
            catch (Exception ex)
            {
                FireException("Error on loading document.", ex);
            }
            finally
            {
                AppInstance.TraceManager.TraceTimeEnd("LoadTemplate/LoadTrade", KeyTraceTime);
            }
        }

        /// <summary>
        /// Alimente les status d'activation, d'environnement, business et priorité
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void SetTradeStatus()
        {
            // FI 20130702 [18798] Initialisation des statuts
            // Ce code était anciennement appelé dans InitializeStatusCheckAndMatch
            TradeImportInput tradeImportInput = _tradeImport.tradeInput;
            if (tradeImportInput.TradeStatusSpecified)
            {
                if (tradeImportInput.TradeStatus.stActivationSpecified && StrFunc.IsFilled(tradeImportInput.TradeStatus.stActivation.NewSt))
                    CommonInput.TradeStatus.stActivation.CurrentSt = tradeImportInput.TradeStatus.stActivation.NewSt;
                //
                if (tradeImportInput.TradeStatus.stBusinessSpecified && StrFunc.IsFilled(tradeImportInput.TradeStatus.stBusiness.NewSt))
                    CommonInput.TradeStatus.stBusiness.CurrentSt = tradeImportInput.TradeStatus.stBusiness.NewSt;
                //
                if (tradeImportInput.TradeStatus.stEnvironmentSpecified && StrFunc.IsFilled(tradeImportInput.TradeStatus.stEnvironment.NewSt))
                    CommonInput.TradeStatus.stEnvironment.CurrentSt = tradeImportInput.TradeStatus.stEnvironment.NewSt;
                //
                if (tradeImportInput.TradeStatus.stPrioritySpecified && StrFunc.IsFilled(tradeImportInput.TradeStatus.stPriority.NewSt))
                    CommonInput.TradeStatus.stPriority.CurrentSt = tradeImportInput.TradeStatus.stPriority.NewSt;
            }

            // FI 20130702 [18798] 
            // Lorsque le trade est en statut MISSING, tous les champs peuvent être non renseignés ou erronés
            // Cela permet de rentrer des trades sans DerivativeContrat par exemple
            // Le code se veut générale, mais pour l'instant nous n'avons testé que  le cas où le dealer, le clearer et le DC peuvent être non renseignés ou erronés   
            // Rien n'est garanti si un champ autre que ceux enoncés
            //
            // Spheres rentre dans ce cas 
            //- en mode New si le template est MISSING ou si le statut d'activation est à MISSING dans le flux postMapping
            //- en Mode Modify le trade modifié est déja MISSING ou si le statut d'activation est à MISSING dans le flux postMapping
            // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
            if (CommonInput.TradeStatus.stActivation.CurrentSt == Cst.StatusActivation.MISSING.ToString())
            {
                for (int i = 0; i < ArrFunc.Count(_tradeImport.tradeInput.CustomCaptureInfos); i++)
                {
                    if (false == _tradeImport.tradeInput.CustomCaptureInfos[i].IsMissingModeSpecified)
                    {
                        _tradeImport.tradeInput.CustomCaptureInfos[i].IsMissingModeSpecified = true;
                        _tradeImport.tradeInput.CustomCaptureInfos[i].IsMissingMode = true;
                    }
                }
            }

            if (IsMissingMode())
            {
                // RD 20120322 / Intégration de trade "Incomplet"
                // Dans un premier temps, on considère le trade comme étant incomplet, pour les raisons suivantes:
                //  1- Restituer les données source dans le DataDocument
                //  2- Restituer les messages d'erreur clairs pour les Cci
                CommonInput.TradeStatus.stActivation.CurrentSt = Cst.StatusActivation.MISSING.ToString();
            }

            if (IsModeNew)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Environnement status: <b>Set to REGULAR</b>", 4));

                //En création: on ecrase systématiquement le StatusEnvironment (issu du template) par REGULAR
                CommonInput.TradeStatus.stEnvironment.CurrentSt = Cst.StatusEnvironment.REGULAR.ToString();
            }
        }

        /// <summary>
        /// <para>Initialisation des ccis en fonction des données du template ou du trade modifié</para>
        /// <para>Préparation du datadocument en fonction de la présence des ccis (instanciation des objects non présents ds le template mais nécessaire du fait de la présence de cci)</para>
        /// </summary>
        /// FI 20180319 [XXXXX] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void LoadCcis()
        {
            string msg = string.Empty;
            if (IsModeNew)
            {
                msg = "Loading default values from template (Loading cci collection)";
            }
            else if (IsModeUpdate)
            {
                msg = StrFunc.AppendFormat("Loading values from {0} (Loading cci collection)", this.TradeKey);
            }

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, msg, 4));

            // FI 20180319 [XXXXX] Mise en place de TraceTime 
            AppInstance.TraceManager.TraceTimeBegin("LoadCcis", KeyTraceTime);

            try
            {
                //Initialisation des ccis
                SetCustomCaptureInfos(_task.Cs);
                TradeCommonCustomCaptureInfos ccis = CommonInput.CustomCaptureInfos;
                // RD 20100706 / en mode Importation, ne charger l'Asset que si toutes les infos sont saisies
                ccis.IsModeIO = true;

                CustomCaptureInfosBase ccisImport = _tradeImport.tradeInput.CustomCaptureInfos;

                for (int i = 0; i < ArrFunc.Count(ccisImport); i++)
                    ccis.Add((CustomCaptureInfo)ccisImport[i].Clone(CustomCaptureInfo.CloneMode.CciAttribut));

                ShiftCcisClientId();

                //Initialisation des ccis en fonction des données du template ou du trade à modifier
                ccis.LoadDocument(_captureMode);

                // FI 20240531 [WI900] call 
                InitDefaultISInvoicingOnCCIOppPayment();

            }
            catch (Exception ex)
            {
                FireException("Error on loading customCaptureInfo.", ex);
            }
            finally
            {
                AppInstance.TraceManager.TraceTimeEnd("LoadCcis", KeyTraceTime);
            }
        }

        /// <summary>
        /// Alimente le document avec les valeurs des ccis de l'import
        /// </summary>
        /// FI 20140814 [XXXXX] refactoring de la fonctionalité InitFromClearingTemplate
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected virtual void DumpToDocument()
        {
            TradeCommonCustomCaptureInfos ccis = CommonInput.CustomCaptureInfos;

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "Dump data (Dump cci to DataDocument)", 4));

            try
            {
                DumpPartyToDocument();

                DumpAllToDocument();

                //ACTOR1 => Party[0] , ACTOR2 => Party[1], etc...
                ccis.CciTradeCommon.SetPartyInOrder();

                // FI 20201105 [25554] call CheckCciCurrency
                CheckCciCurrency(CSTools.SetCacheOn(_task.Cs), _dbTransaction, ccis.Cast<CustomCaptureInfo>());
            }
            catch (Exception ex)
            {
                FireException("Error on dump ccis to Datadocument.", ex);
            }
        }

        /// <summary>
        ///  Alimentation des ccis de la saisie liés aux Parties à partir des ccis de l'importation, suivie de la maj du DataDocument.
        /// </summary>
        /// FI 20140815 [XXXXX] add Method
        protected virtual void DumpPartyToDocument()
        {
            //=========================================
            //Dump des ccis Party et Broker en priorité  (PL: "En priorité" de quoi ?)
            //=========================================
            bool isApplyPartyTemplate = BoolFunc.IsTrue(GetParameter(TradeImportCst.isApplyPartyTemplate));
            bool isApplyClearingTemplate = BoolFunc.IsTrue(GetParameter(TradeImportCst.isApplyClearingTemplate));

            //ccis de la saisie
            TradeCommonCustomCaptureInfos ccis = CommonInput.CustomCaptureInfos;

            CciTradeParty[] cciParty = ccis.CciTradeCommon.cciParty;
            int cciParty_Count = ArrFunc.Count(cciParty);

            for (int i = 0; i < cciParty_Count; i++) //(PL: Pourquoi une boucle distincte de celle ci-dessous ?)
            {
                cciParty[i].IsInitFromPartyTemplate = isApplyPartyTemplate;
                cciParty[i].IsInitFromClearingTemplate = isApplyClearingTemplate;
            }

            CustomCaptureInfo cci = null;
            try
            {
                //Affectation des ccis PARTY avec les données issues de l'importation, puis maj du DataDocument

                #region IMPORTANT! Ordre de traitement des parties...
                #region Règle concernant l'ordre:
                // Lorsqu'un "Template" comporte une contrepartie (2ème acteur) de renseignée, si la partie (1er acteur) que l'on importe est identique à celle-ci 
                // l'importation du trade échoue, car lors du traitement de la partie (1er acteur) on se retrouve avec "partie = contrepartie", ce qui est interdit.
                // Pour parer à cela, on procède donc dans ce cas, au traitement dans un premier temps de la contrepartie (2ème acteur).
                // Pour cela on inverse simplement l'ordre dasn la boucle for{} ci-dessous ( for{i=0; i<count; i++} devient for{i=count; i>=0; i--} )
                // See also TRIM [21071]
                #endregion 
                int start = 0;
                int end = cciParty_Count;
                int step = 1;
                if ((end > 1)
                    && (_importCustomCaptureInfos[cciParty[0].Cci(CciTradeParty.CciEnum.actor).ClientId_WithoutPrefix].NewValue ==
                            cciParty[1].Cci(CciTradeParty.CciEnum.actor).LastValue))
                {

                    start = end - 1;
                    end = 0;
                    step = -1;
                }
                #endregion

                for (int i = start; (step == 1 ? (i < end) : (i >= end)); i += step)
                {
                    cci = cciParty[i].Cci(CciTradeParty.CciEnum.actor);
                    // FI 20200429 [XXXXX] false sur pCciIsPayerOrReceiver
                    SetCciFromCciImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cci, false);
                    #region Check New Party 
                    // PL 20150610 Je pense que l'utilité du code de cette région est à revoir ou à amender...

                    // RD 20120322 / Intégration de trade "Incomplet" - Rejeter un trade si les deux contreparties sont identiques
                    //CheckNewParty(i, cci.NewValue, cciParty);
                    // Contrôle si la partie est identique à une autre partie. Si la partie existe déjà une exception est générée !
                    if (i != start) //Inutile d'opérer une vérification après la mise de la 1ère partie traitée.
                    {
                        //for (int j = start; j < pIndex; j += step)
                        for (int j = start; (step == 1 ? (j < i) : (j > i)); j += step)
                        {
                            if (cciParty[j].Cci(CciTradeParty.CciEnum.actor).NewValue == cci.NewValue)
                                FireException("Counterparties are identical");
                        }
                    }
                    #endregion Check New Party
                    ccis.Dump_ToDocument(0);
                    // RD 20100629/FI 20100629 / Pour ne PLUS considérer ce Cci comme étant le dernier renseigné (PL: Pourquoi, quelle est l'utilité ?)
                    cci.IsLastInputByUser = false;

                    #region Affectation des ccis BOOK relatifs aux parties
                    cci = cciParty[i].Cci(CciTradeParty.CciEnum.book);
                    if (null != cci)
                    {
                        // FI 20200429 [XXXXX] false sur pCciIsPayerOrReceiver
                        SetCciFromCciImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cci, false);
                        ccis.Dump_ToDocument(0);
                        cci.IsLastInputByUser = false; //(PL: Partout on a un commentaire RD/FI mais pas ici. Pourquoi ?)
                    }
                    #endregion

                    #region Affectation des ccis BROKER relatifs aux parties
                    for (int j = 0; j < cciParty[i].BrokerLength; j++)
                    {
                        cci = cciParty[i].cciBroker[j].Cci(CciTradeParty.CciEnum.actor);
                        // FI 20200429 [XXXXX] false sur pCciIsPayerOrReceiver
                        SetCciFromCciImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cci, false);
                        ccis.Dump_ToDocument(0);
                        // RD 20100629/FI 20100629 / Pour ne PLUS considérer ce Cci comme étant le dernier renseigné
                        cci.IsLastInputByUser = false;
                    }
                    #endregion
                }

                #region Affectation des ccis BROKER avec les données issues de l'importation, puis maj du DataDocument
                CciTradeParty[] cciBroker = ccis.CciTradeCommon.cciBroker;

                for (int i = 0; i < ArrFunc.Count(cciBroker); i++)
                {
                    cciBroker[i].IsInitFromPartyTemplate = isApplyPartyTemplate;

                    cci = cciBroker[i].Cci(CciTradeParty.CciEnum.actor);
                    // FI 20200429 [XXXXX] false sur pCciIsPayerOrReceiver
                    SetCciFromCciImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cci, false);
                    ccis.Dump_ToDocument(0);
                    // RD 20100629/FI 20100629 / Pour ne PLUS considérer ce Cci comme étant le dernier renseigné
                    cci.IsLastInputByUser = false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                FireException(StrFunc.AppendFormat("Error on dump cci {0} to Datadocument.", cci.ClientId_WithoutPrefix), ex);
            }
        }

        /// <summary>
        ///  <para>○ Alimentation des ccis de la saisie à partir ccis de l'import</para>
        ///  <para>○ Alimentation du datadocument dans la foulée</para>
        ///  <para>○ Les ccis déjà initialisés à partir des ccis de l'import sont exclus (performances accrues, permet de ne pas écraser les initialisations opérées via ClearingTemplate)</para>  
        /// </summary>
        /// FI 20140815 [XXXXX] add Method
        private void DumpAllToDocument()
        {
            TradeCommonCustomCaptureInfos ccis = CommonInput.CustomCaptureInfos;

            //Affectation des ccis avec les données issues de l'import puis maj du doc
            foreach (CustomCaptureInfo cci in ccis)
            {
                // FI 20140815 [XXXXX] est sur currentCci.accessKey
                if (StrFunc.IsEmpty(cci.AccessKey)) //Les ccis déjà initialisés à partir des ccis de l'import sont exclus, cela de façon à ne pas écraser les initialisations générées par CLEARINGTEMPLATE
                {
                    try
                    {
                        SetCciFromCciImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cci, ccis.CciTradeCommon.IsClientId_PayerOrReceiver(cci));
                        ccis.Dump_ToDocument(0);
                        // RD 20100629/FI 20100629 / Pour ne PLUS considérer ce Cci comme étant le dernier renseigné
                        cci.IsLastInputByUser = false;
                    }
                    catch (Exception ex)
                    {
                        FireException(StrFunc.AppendFormat("Error on dump cci {0} to Datadocument.", cci.ClientId_WithoutPrefix), ex);
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void FullCustomCaptureInfos()
        {
            try
            {
                //FI 20120226 tradeInput est de type TradeImportInput
                //TradeCommonInput tradeImportInput = m_tradeImport.tradeInput;
                //CustomCaptureInfosBase ccisImport = m_tradeImport.tradeInput.CustomCaptureInfos;

                TradeImportInput tradeImportInput = _tradeImport.tradeInput;
                CustomCaptureInfosBase ccisImport = _tradeImport.tradeInput.CustomCaptureInfos;

                FullCustomCaptureInfo[] fullCcisImport = tradeImportInput.FullCustomCaptureInfos;
                for (int i = 0; i < ArrFunc.Count(fullCcisImport); i++)
                {
                    FullCustomCaptureInfo fullCci = (FullCustomCaptureInfo)fullCcisImport[i];
                    fullCci.ClientId = "IMP" + fullCci.ClientId;
                    try
                    {
                        ;
                        //
                        string newClientId = fullCci.ClientId_WithoutPrefix.Replace("//", string.Empty);
                        //
                        if (newClientId.StartsWith("//"))
                            newClientId = newClientId.Replace("//", string.Empty);
                        if (newClientId.StartsWith("efs:EfsML/"))
                            newClientId = newClientId.Replace("efs:EfsML/", string.Empty);
                        //
                        object fullObj = ReflectionTools.GetElementByXPath(CommonInput.DataDocument.DataDocument, newClientId, 
                            out FieldInfo fld, out FieldInfo specifiedFld, out object parentObj);
                        //
                        // RD 20110207 / à revoir pour les types Array
                        if ((null != fullObj) && (false == fullObj.GetType().IsArray))
                        {
                            Type fullObjType = fullObj.GetType();
                            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(fullObjType, fullCci.NewValue.Text);
                            Object fullCciObj = CacheSerializer.Deserialize(serializeInfo);
                            //
                            fld.SetValue(parentObj, fullCciObj);
                            if (null != specifiedFld)
                                specifiedFld.SetValue(parentObj, true);
                        }
                        else
                            FireException("<b>Element not found in DataDocument using XPath specified on fullCci.</b>");
                    }
                    catch (Exception ex)
                    {
                        string errCii = StrFunc.AppendFormat("Error on fullCci [{0}].", fullCci.ClientId_WithoutPrefix);
                        FireException(errCii, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                FireException("<b>Error on dump fullCcis to DataDocument.</b>", ex);
            }
        }

        /// <summary>
        /// Contrôle la présence de tradeInput et tradeInput.CustomCaptureInfos 
        /// </summary>
        private void CheckTradeInput()
        {
            try
            {
                if (false == _tradeImport.tradeInputSpecified)
                    FireException("<b>TradeInput not specified</b>.");

                if (ArrFunc.IsEmpty(_tradeImport.tradeInput.CustomCaptureInfos))
                    FireException("<b>CustomCaptureInfos not specified</b>.");
            }
            catch (Exception ex)
            {
                FireException("<b>Error on initialize</b>", ex);
            }
        }

        /// <summary>
        /// Contrôle des ccis en erreur
        /// </summary>
        // FI 20131122 [19233]
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void CheckCCiErr()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "Data checking (cci checking)", 4));

            try
            {
                CommonInput.CustomCaptureInfos.FinaliseAll();
                // RD 20120322 / Intégration de trade "Incomplet"

                // C'est pour avoir le code du message en premier avec la liste des arguments du message
                // FI 20131122 [19233] modification du message 6025
                string[] errMsgShort = new string[] {  LogHeader , GetParameter(TradeImportCst.extlLink), string.Empty, string.Empty,
                                                                   string.Empty , string.Empty , string.Empty, string.Empty, string.Empty, string.Empty};
                string errMsg = CommonInput.CustomCaptureInfos.GetErrorMessage(out bool isAllCciMissingMode, ref errMsgShort);

                if (CommonInput.TradeStatus.IsStActivation_Missing)
                {
                    // S'il existe des erreurs:
                    //  - Intégrer le trade en statut "MISSING", si uniquement des Cci avec IsMissingMode=true, qui sont en erreur.
                    //  - Rejeter le trade dans le cas contraire
                    // Sinon
                    //  Intégrer le trade en statut "REGULAR"
                    if (StrFunc.IsFilled(errMsg))
                    {
                        if (isAllCciMissingMode)
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            // FI 20200706 [XXXXX] use SetErrorWarning delegate
                            //_task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 6025), 4, errMsgShort.Select(e => new LogParam(e))));
                        }
                        else
                        {
                            FireException(errMsg);
                        }
                    }
                    else
                    {
                        CommonInput.TradeStatus.stActivation.CurrentSt = Cst.StatusActivation.REGULAR.ToString();
                    }
                }
                else if (StrFunc.IsFilled(errMsg))
                    FireException(errMsg);
            }
            catch (Exception ex)
            {
                FireException("Error on checking customCaptureInfo data.", ex);
            }
        }

        /// <summary>
        /// Initialisation des statuts Check et Match
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void InitializeStatusCheckAndMatch(InputUser inputUser)
        {

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "Check and Match status: Setting", 4));

            try
            {

                //Set default m_User status (Check/Match) from ACTORROLE (Utile au cas où un acteur est présent dans le Template)
                CommonInput.InitStUserFromPartiesRole(CSTools.SetCacheOn(_task.Cs), null);
                //Set default m_User status (Check/Match) from ACTIONTUNING
                inputUser.InitActionTuning(CSTools.SetCacheOn(_task.Cs), _captureGen.TradeCommonInput.Product.IdI);
                CommonInput.InitStUserFromTuning(inputUser.ActionTuning);
            }
            catch (Exception ex)
            {
                FireException("<b>Error on initialize Check and Match status</b>", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetImportCustomCaptureInfos()
        {
            _importCustomCaptureInfos = _tradeImport.tradeInput.CustomCaptureInfos;
        }


        /// <summary>
        /// Traite l'enregistrement (Création ou Modification ou Annulation d'un trade)
        /// </summary>
        /// <returns></returns>
        /// FI 20160907 [21831] Modify
        /// FI 20170116 [21916] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override IOCommonTools.SqlAction ProcessExecute()
        {
            IOCommonTools.SqlAction retAction = IOCommonTools.SqlAction.NA;

            CheckParameter();

            LoadUser(out User user, out InputUser inputUser);

            //CaptureSessionInfo
            CaptureSessionInfo sessionInfo = new CaptureSessionInfo
            {
                user = user,
                session = _task.Process.Session,
                licence = _task.Process.License,
                idProcess_L = _task.Process.IdProcess,
                idTracker_L = _task.Process.Tracker.IdTRK_L
            };


            InitTradeInput(sessionInfo);

            // RD 20170217 [22863] 
            // L'importation des trades (qu'ils proviennent d'une Gateway ou autre) 
            // ne devrait pas modifier un trade d'une date antérieure à la journée de bourse courante dans Spheres
            if (IsModeNew == false && _captureGen.TradeCommonInput.IsAllocationFromPreviousBusinessDate)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                // FI 20200706 [XXXXX] use SetErrorWarning delegate
                //_task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 6074), 4,
                        new LogParam(LogTools.IdentifierAndId(_captureGen.TradeCommonInput.Identifier, _captureGen.TradeCommonInput.IdT)),
                        new LogParam(GetParameter(TradeImportCst.extlLink)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(_captureGen.TradeCommonInput.ClearingBusinessDate)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(_captureGen.TradeCommonInput.CurrentBusinessDate))));

                return retAction;
            }

            // RD 20120323 / Annulation de trade
            // Ne pas charger les Cci en mode "Annulation" pour ne pas modfier le trade existant, avant de l'annuler 
            if (IsModeRemoveOnlyAll == false)
            {
                // FI 20130328 [19793] Mise en place du cache SQL
                CalcCcisImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction);


                if (IsModeUpdateFeesOnly)
                {
                    // FI 20160907 [21831] Suppression des frais présents sur le trades (tous les frais => frais manuels ou frais issus des barêmes)
                    // Par défaut seul les frais auto sont supprimés (voir méthode InitBeforeCaptureMode)
                    ((TradeCaptureGen)_captureGen).Input.ClearFee(TradeInput.FeeTarget.trade, TradeInput.ClearFeeMode.All);
                }
                else if (IsModeUpdateTraderOnly)
                {
                    // FI 20170824 [23339] NE RIEN FAIRE. 
                    // Il s'agit uniquement d'ajouter un trader 
                }
                else
                {
                    _captureGen.InitBeforeCaptureMode(_task.Cs, _dbTransaction, inputUser, sessionInfo);
                }

                //
                SetTradeStatus();

                //
                LoadCcis();

                //
                DumpToDocument();

                // FI 20160907 [21831] FullCustomCaptureInfos is not call
                if (false == (IsModeUpdateFeesOnly || IsModeUpdateTraderOnly))
                    FullCustomCaptureInfos();

                //
                CheckCCiErr();

                //
                ProcessSpecific();

                if ((false == CommonInput.CustomCaptureInfos.IsPreserveData))  //Pas de cleanup sur un template par ex 
                    CommonInput.CustomCaptureInfos.CciContainer.CleanUp();

                InitializeStatusCheckAndMatch(inputUser);

                //
                //FI 20110924 [17630] Appel à SetDefaultValue avant de faire un checkValidationRule
                CommonInput.SetDefaultValue(_task.Cs, null);

                CheckValidationRule(sessionInfo.user);

            }
            else if (IsModeRemoveOnlyAll)
            {
                // FI 20170116 [21916] l'annulation RemoveAllocation devient une annulation classique (injection d'un évènement d'annulation) si l'instrument est non fungible
                if ((false == (CommonInput.SQLInstrument.IsFungible)) && _captureMode == Cst.Capture.ModeEnum.RemoveAllocation)
                    _captureMode = Cst.Capture.ModeEnum.RemoveOnly;
            }

            //
            //FI 20120710 [18005] Spheres deactive les trades qui ont le statut Missing   
            bool isTradeToDeactiv = (IsModeRemoveOnlyAll && CommonInput.TradeStatus.IsCurrentStActivation_Missing);


            if (false == isTradeToDeactiv)
            {
                CheckAndRecord(sessionInfo);
            }

            if (IsModeRemoveOnlyAll)
            {
                if (isTradeToDeactiv)
                {
                    // Si le trade est en statut "Missing", cela voudra dire:
                    // - que les événements n'ont pas été générés 
                    // - que le trade n'est pas en position,
                    //
                    // donc il suffira de désactiver le Trade sans passer par POSKEEPREQUEST                            
                    DeactivateTrade(sessionInfo);
                }
                else
                {
                    #region SendMQueueForRemove for Remove Trade
                    // EG 20121213 Increment 1 To column POSTEDSUBMSG (TRACKER)
                    _task.Process.Tracker.AddPostedSubMsg(1, _task.Process.Session);

                    if (IsModeRemoveAllocation)
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, "Sending request for Position Keeping", 4));

                        if (SendPosRequest(out string msgLog))
                        {
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Info, msgLog.Replace("●", ""), 4, new LogParam(IdT, default, "TRADE", Cst.LoggerParameterLink.IDDATA)));
                        }
                        else
                            FireException(msgLog);
                    }
                    else
                    {
                        SendMQueueForRemove();
                    }
                    

                    #endregion
                }
            }
            else if (IsModeUpdateFeesOnly) // FI 20160907 [21831] Add
            {
                SendMQueueFeesEventsGen();
            }
            else
            {
                SendMqueueEventsGen();
            }


            if (IsModeNew)
                retAction = IOCommonTools.SqlAction.I;
            else if (IsModeUpdate)
                retAction = IOCommonTools.SqlAction.U;
            else if (IsModeRemoveOnlyAll)
                retAction = IOCommonTools.SqlAction.D;

            return retAction;
        }

        /// <summary>
        ///  Enregistrement du trade
        ///  <param name="sessionInfo"></param>
        /// </summary>
        /// FI 20131122 [19233] Modif dans écritures dans le log
        /// FI 20160517 [22148] Modify
        /// FI 20160907 [21831] Modify
        /// FI 20160907 [21831] Modify
        /// FI 20170404 [23039] Modify
        /// FI 20180319 [XXXXX] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private void CheckAndRecord(CaptureSessionInfo sessionInfo)
        {

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "Recording of the " + TradeKey + "...", 4));

            // FI 20180319 [XXXXX] Mise en place de TraceTime 
            AppInstance.TraceManager.TraceTimeBegin("CheckAndRecord", KeyTraceTime);
            try
            {
                string identifier = GetParameter(TradeImportCst.identifier);

                TradeRecordSettings recordSettings = new TradeRecordSettings
                {
                    displayName = GetParameter(TradeImportCst.displayName),
                    description = GetParameter(TradeImportCst.description),
                    extLink = GetParameter(TradeImportCst.extlLink),
                    idScreen = GetParameter(TradeImportCst.screen),
                    isCheckValidationXSD = IsApplyValidationXSD(),
                    isCheckValidationRules = IsApplyValidationRules(),
                    isGetNewIdForIdentifier = IsGetNewIdForIdentifier,
                    // RD 20121031 ne pas vérifier la license pour les services pour des raisons de performances
                    isCheckLicense = false,
                    isCheckActionTuning = false,
                    // FI 20140328 [19793] Tuning de l'importation
                    isCheckValidationLock = false,
                    // FI 20140331 [19793] Tuning de l'importation
                    isCopyAttachedDoc = IsApplyCopyAttachedDoc(),
                    // FI 20140331 [19793] Tuning de l'importation
                    isCopyNotePad = IsApplyCopyNotePad(),
                    // FI 20160907 [21831] Add
                    isUpdateFeesOnly = IsModeUpdateFeesOnly,
                    
                };

                if (recordSettings.isGetNewIdForIdentifier)
                    recordSettings.isGetNewIdForIdentifier = (false == CommonInput.TradeStatus.IsStEnvironment_Template);

                // FI 20201120 [XXXXX]  S'il existe une transaction alors le mode 0 est obligatoire 
                if (null != _dbTransaction)
                    recordSettings.recordMode = 0;
                else
                    recordSettings.recordMode = (int)SystemSettings.GetAppSettings("ImportTrade_recordMode", typeof(int), 0);
                
                
                // FI 20170404 [23039] add underlying and trader 
                Pair<int, string>[] underlying = null;
                Pair<int, string>[] trader = null;

                TradeCommonCaptureGen.ErrorLevel lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
                TradeCommonCaptureGenException errExc = null;
                try
                {
                    int idT = 0;
                    /* FI 20211206 [XXXXX] usage de tryMultiple
                    _captureGen.CheckAndRecord(_task.Cs, _dbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade), _captureMode, sessionInfo, recordSettings,
                        ref identifier, ref idT,
                        out underlying, out trader,
                        _task.requester.idTRK, out processLog);
                    */
                    try
                    {
                        TryMultiple tryMultiple = new TryMultiple(_task.Cs, "ImportTrade_CheckAndRecord", $"Record Trade")
                        {
                            SetErrorWarning = SetErrorWarning,
                            LogRankOrder = 4,
                            LogHeader = LogHeader,
                            ThreadSleep = 5 //blocage de 5 secondes entre chaque tentative
                        };
                        tryMultiple.InitIsRetryException(IsRetryException);

                        tryMultiple.Exec(() =>
                        {
                            _captureGen.CheckAndRecord(_task.Cs, _dbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade), _captureMode, sessionInfo, recordSettings,
                            ref identifier, ref idT,
                            out underlying, out trader);
                        });
                    }
                    catch (TryMultipleException ex)
                    {
                        if (ex.IsFromTargetInvocationException)
                        {
                            Logger.Log(new LoggerData(LogLevelEnum.Error, ex.Message, 4));
                            throw ex.ActionException;
                        }
                        else
                            throw;
                    }

                    // FI 20200505 [XXXXX] Alimentation de _idUnderlying
                    _idUnderlying = null;
                    if (ArrFunc.IsFilled(underlying))
                    {
                        _idUnderlying = (from item in underlying
                                         select item.First).ToArray();
                    }

                    //
                    //FI 20100630 Mise à jour de TradeCommonInput.identification puisque utilisé par la suite
                    //L'idéal aurait été de faire appel à la méthode TradeCommonCaptureGen.Load() mais ce n'est pas envisagé pour des raison de performance
                    SpheresIdentification identification = TradeRDBMSTools.GetTradeIdentification(_task.Cs, _dbTransaction, idT);
                    CaptureGen.TradeCommonInput.Identification = identification;
                }
                catch (TradeCommonCaptureGenException ex)
                {
                    //Erreur reconnue
                    errExc = ex;
                    lRet = errExc.ErrLevel;
                }

                
                //Si une exception se produit après l'enregistrement du trade => cela veut dire que le trade est correctement rentré en base, on est en succès
                if ((null != errExc) && TradeCaptureGen.IsRecordInSuccess(errExc.ErrLevel))
                    lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;

                // FI 20140124 [XXXXX] ne pas alimenter le log en mode remove
                if (false == IsModeRemoveOnlyAll)
                    AddLogTradeXML(lRet);

                // FI 20170404 [23039] underlying, trader 
                string msgResult = _captureGen.GetResultMsgAfterCheckAndRecord(_task.Cs, errExc, _captureMode, identifier, underlying, trader,
                                                                               (_task.Process.LogDetailEnum >= LogLevelDetail.LEVEL4),
                                                                               out string msgResultDet);
                //FI 20131122 [19233] Add LogHeader 
                //FI 20131122 [19233] Add ExtLink code usage recordSettings.extLink puisque en cas d'erreur CaptureGen.TradeCommonInput.identification peut être incorrectement renseigné
                //FI 20160517 [22148] slogHeader ajouté uniquement si succes. 
                //Si erreur alors cette fonction génère une exception et dans ce cas IO produit automatiquement le logHeader (Cela évite de répéter 2 fois la même chose)
                string sLogHearder = (TradeCommonCaptureGen.ErrorLevel.SUCCESS == lRet) ? LogHeader + Cst.CrLf : string.Empty;
                string msg = StrFunc.AppendFormat("{0}{1}{2}{3}",
                            sLogHearder,
                            StrFunc.AppendFormat("Trade Id: <b>{0}</b>", recordSettings.extLink),
                            Cst.CrLf2,
                            msgResult);

                if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == lRet)
                {
                    
                    if (StrFunc.IsFilled(msgResultDet))
                        msg += Cst.CrLf + msgResultDet;
                    
                    Logger.Log(new LoggerData(LogLevelEnum.None, msg, 4, new LogParam(IdT, default, default, Cst.LoggerParameterLink.IDDATA)));
                }
                else
                {
                    // FI 20131216 [19337] Ajout de l'exception errExc
                    // FI 20160517 [22148] Modify (Add if => Cela évite de répéter plusieurs fois le message dans le log de SpheresIO le message associé à l'exception)
                    if (false == errExc.IsInnerException)
                    {
                        if (StrFunc.IsFilled(msgResultDet))
                            msg += Cst.CrLf + msgResultDet;

                        //On génère une exception avec uniquement le message (il est suffisamment clair)    
                        FireException(msg);
                    }
                    else
                    {
                        //ici lorsque l'exception de TradeCommonCaptureGenException est issue d'une exception précédente
                        //Ds ce cas on  conserve l'exeption précédente pour avoir un max de détail    
                        FireException(msg, errExc);
                    }
                }
            }
            catch (Exception ex)
            {
                FireException("<b>Error on the recording of the " + TradeKey + ".</b> The process is aborted.", ex);
            }
            finally
            {
                AppInstance.TraceManager.TraceTimeEnd("CheckAndRecord", KeyTraceTime);
            }
        }
        /// <summary>
        /// Delegue qui indique à TryMultiple de retenter CheckAndRecord
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// FI 20211206 [XXXXX] Add Method
        private Boolean IsRetryException(Exception ex, out string message)
        {
            bool ret = false;
            message = string.Empty;

            if (ex.GetType().Equals(typeof(TradeCommonCaptureGenException)))
            {
                TradeCommonCaptureGenException tradeEx = ((TradeCommonCaptureGenException)ex);

                if (IsModeRemoveOnlyAll)
                {
                    // RD 20150709 [21056] Faire plusieurs tentatives si Annulation d'un trade locké
                    ret = (tradeEx.ErrLevel == TradeCommonCaptureGen.ErrorLevel.LOCK_ERROR);
                    if (ret)
                        message = TradeCommonCaptureGen.ErrorLevel.LOCK_ERROR.ToString();
                }
                else if ((tradeEx.ErrLevel == TradeCommonCaptureGen.ErrorLevel.SAVEUNDERLYING_ERROR))
                {
                    Exception exFirst = ExceptionTools.GetFirstRDBMSException(ex);
                    if (null != exFirst)
                    {
                        SQLErrorEnum sqlErr = DataHelper.AnalyseSQLException(_task.Cs, exFirst);
                        ret = (sqlErr == SQLErrorEnum.DuplicateKey);
                        if (ret)
                            message = TradeCommonCaptureGen.ErrorLevel.SAVEUNDERLYING_ERROR.ToString();
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Vérification des validationRules 
        /// <para>- sauf si le trade est incomplet</para>
        /// <para>- sauf si le contrôle des validatioRule est désactivé</para>
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void CheckValidationRule(User pUser)
        {
            if (!_captureGen.IsInputIncompleteAllow(_captureMode))
            {
                if (IsApplyValidationRules())
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, "Validation Rules checking", 4));

                    CheckValidationRule(_task.Cs, null, CaptureMode, pUser);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCapture"></param>
        /// <returns></returns>
        private static IdMenu.Menu GetMenu(Cst.Capture.ModeEnum pCapture)
        {
            //
            // RD 20120323 / Annulation de trade
            IdMenu.Menu idMenu = IdMenu.Menu.InputTrade;
            switch (pCapture)
            {
                case Cst.Capture.ModeEnum.New:
                case Cst.Capture.ModeEnum.Update:
                    idMenu = IdMenu.Menu.InputTrade;
                    break;
                case Cst.Capture.ModeEnum.RemoveAllocation:
                    idMenu = IdMenu.Menu.InputTrade_RMVALLOC;
                    break;
                case Cst.Capture.ModeEnum.RemoveOnly:
                    idMenu = IdMenu.Menu.InputTrade_RMV;
                    break;
            }
            return idMenu;
        }

        /// <summary>
        /// Retourne InputUser
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pUser"></param>
        /// <param name="pMenu"></param>
        /// <returns></returns>
        /// FI 20130204 [] Mise en cache de certaines ds l'importation
        private InputUser LoadCacheInputUser(string pCS, User pUser, IdMenu.Menu pIdMenu)
        {
            string key = pUser.IdA.ToString() + "|" + pIdMenu.ToString();
            key = IOProcessImportCache.KeyBuilder(pCS, key);

            InputUser inputUser;
            if (IOProcessImportCache.cacheInputUser.Contains(key))
            {
                inputUser = (InputUser)IOProcessImportCache.cacheInputUser[key];
            }
            else
            {
                inputUser = new InputUser(IdMenu.GetIdMenu(pIdMenu), pUser);
                inputUser.InitializeFromMenu(CSTools.SetCacheOn(pCS));
                IOProcessImportCache.cacheInputUser.Add(key, inputUser);
            }
            return inputUser;
        }

        /// <summary>
        ///  Retourne true si l'importation doit contrôler les validations Rules 
        ///  <para>Lecture du paramètre "isApplyValidationRules"</para>
        /// </summary>
        /// <returns></returns>
        private bool IsApplyValidationRules()
        {
            Boolean ret = true;
            string parameter = GetParameter(TradeImportCst.isApplyValidationRules);
            if (StrFunc.IsFilled(parameter))
                ret = BoolFunc.IsTrue(parameter);
            return ret;
        }

        /// <summary>
        ///  Retourne true si l'importation doit effecter la validation XSD 
        ///  <para>Lecture du paramètre "isApplyValidationXSD"</para>
        /// </summary>
        private bool IsApplyValidationXSD()
        {
            Boolean ret = true;
            string parameter = GetParameter(TradeImportCst.isApplyValidationXSD);
            if (StrFunc.IsFilled(parameter))
                ret = BoolFunc.IsTrue(parameter);
            return ret;
        }

        /// <summary>
        /// Retourne true si Spheres® doit copier les notes présentes dans le template
        /// </summary>
        /// <returns></returns>
        /// FI 20140331 [19793] add method
        private bool IsApplyCopyNotePad()
        {
            Boolean ret = false;
            string parameter = GetParameter(TradeImportCst.isCopyNotePad);
            if (StrFunc.IsFilled(parameter))
                ret = BoolFunc.IsTrue(parameter);
            return ret;
        }

        /// <summary>
        /// Retourne true si Spheres® doit re-copier les doc attachés au template
        /// </summary>
        /// FI 20140331 [19793] add method
        private bool IsApplyCopyAttachedDoc()
        {
            Boolean ret = false;
            string parameter = GetParameter(TradeImportCst.isCopyAttachedDoc);
            if (StrFunc.IsFilled(parameter))
                ret = BoolFunc.IsTrue(parameter);
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCcis"></param>
        /// <returns></returns>
        private static string GetkeyCcis(CustomCaptureInfosBase pCcis)
        {
            string ret = string.Empty;
            if (ArrFunc.IsFilled(pCcis))
            {
                StringBuilder strBuilder = new StringBuilder();
                foreach (CustomCaptureInfo cci in pCcis)
                {
                    strBuilder.Append(cci.ClientId);
                }
                ret = strBuilder.ToString();
            }
            return ret;
        }

        /// <summary>
        ///  Insère le trade XML dans le log
        ///  <para>Le trade est inséré si le niveau de log est full où s'il existe une erreur </para>
        /// </summary>
        /// <param name="lRet"></param>
        /// FI 2131213 [19337] add logHeader ds les messages d'erreurs
        private void AddLogTradeXML(TradeCaptureGen.ErrorLevel lRet)
        {
            if ((TradeCommonCaptureGen.ErrorLevel.SUCCESS != lRet) ||
                _task.Process.LogDetailEnum >= LogLevelDetail.LEVEL4)
            {
                bool isError = (TradeCommonCaptureGen.ErrorLevel.SUCCESS != lRet);

                // FI 20200409 [XXXXX] fileName commence par logHeaderNoHTMLTag => (Exemple Row (idr2, src2)_Trade_24193.xml)
                string folder = _task.Session.MapTemporaryPath("Trade_xml", AppSession.AddFolderSessionId.True);
                SystemIOTools.CreateDirectory(folder);

                string fileName = FileTools.ReplaceFilenameInvalidChar(LogHeaderNoHTMLTag + "_Trade" + "_" + _captureGen.TradeCommonInput.Identifier);
                string fileXml = fileName + ".xml";

                try
                {
                    try
                    {
                        _captureGen.WriteTradeXMLOnTemporary(_task.Session, folder, fileXml);
                    }
                    catch (Exception ex)
                    {
                        FireException("<b>Error on writting tradeXML on temporary folder</b>.", ex);
                    }

                    try
                    {
                        byte[] data = FileTools.ReadFileToBytes(folder + @"\" + fileXml);
                        //_task.process.processLog.AddAttachedDoc(_task.Cs, data, filename, "xml");
                        LogTools.AddAttachedDoc(_task.Cs, _task.Process.IdProcess, _task.Process.Session.IdA, data, fileXml, "xml");
                    }
                    catch (Exception ex)
                    {
                        FireException("<b>Error on writting tradeXML in AttachedDoc table</b>.", ex);
                    }
                    
                    LoggerData logData = default;
                    if (isError)
                    {
                        // FI 2131213 [19337] add logHeader ds les messages d'ereurs
                        if (TradeCommonCaptureGen.ErrorLevel.XMLDOCUMENT_NOTCONFORM == lRet)
                        {
                            logData = new LoggerData(LogLevelEnum.Error, LogHeader + Cst.CrLf + string.Format("<b>Trade not conform</b>, see xml File <b>{0}</b> in the attached documents", fileXml), 4);
                        }
                        else
                        {
                            logData = new LoggerData(LogLevelEnum.Error, LogHeader + Cst.CrLf + "<b>Error on recording the trade</b>", 4);
                        }
                    }
                    else
                    {
                        logData = new LoggerData(LogLevelEnum.Debug, string.Format("Trade conform , see xml File <b>{0}</b> in the attached documents", fileXml), 4);
                    }

                    // FI 20200623 [XXXXX] SetErrorWarning
                    // FI 20200706 [XXXXX] ne pas appeler SetErrorWarning. Le traitement ne peut finir en erreur parque l'écriture du log plante
                    // _task.process.ProcessState.SetErrorWarning(ProcessStateTools.ParseStatus(info.status));

                    
                    Logger.Log(logData);
                }
                catch (Exception ex)
                {
                    // FI 20200623 [XXXXX] AddCriticalException
                    _task.Process.ProcessState.AddCriticalException(ex);

                    
                    
                    Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                }
            }
        }

        /// <summary>
        /// Contrôle la présence des données à importer
        /// </summary>
        protected override void CheckInput()
        {
            CheckTradeInput();

        }


        /// <summary>
        ///  Retourne true si le lock pour le record en cours d'importation s'est correctement déroulé
        ///  <param name="pLockObject">le lock que l'on cherche à poser</param>
        /// </summary>
        /// FI 20130528 [18662] 
        /// FI 20131122 [19233] add logHeader
        /// EG 20190114 Add detail to ProcessLog Refactoring
        protected override LockRecordReturn LockRecord(out LockObject pLockObject)
        {
            pLockObject = null;
            LockRecordReturn ret = LockRecordReturn.LockRecordSucces;
            //
            // L'identifiant d'un trade (champ TRID si importation ETD) doit être renseigné dans l'EXTLLINK du trade
            string extlLink = string.Empty;
            IScheme scheme = Tools.GetScheme(_settings.parameter, TradeImportCst.extlLink);
            if (null != scheme)
                extlLink = ((ImportParameter)scheme).GetDataValue(_task.Cs, null);
            //
            if (StrFunc.IsFilled(extlLink))
            {
                string action = GetLockAction();

                LockObject lockTrade = new LockObject(TypeLockEnum.TRADE, extlLink, extlLink, LockTools.Exclusive);
                pLockObject = lockTrade;

                Lock lck = new Lock(_task.Cs, lockTrade, _task.Session, action);

                bool isLockSuccessful = LockTools.LockMode1(lck, out Lock lockRecordExisting);

                if (isLockSuccessful)
                {
                    ret = LockRecordReturn.LockRecordSucces;
                }
                else
                {
                    string actionExisting = lockRecordExisting.Action;

                    if (action == actionExisting)
                        ret = LockRecordReturn.LockRecordIgnore;
                    else
                        ret = LockRecordReturn.LockRecordError;

                    switch (ret)
                    {
                        case LockRecordReturn.LockRecordIgnore:
                            // FI 20200623 [XXXXX] SetErrorWarning
                            // FI 20200706 [XXXXX] use SetErrorWarning delegate
                            //_task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 6035), 4,
                                new LogParam(LogHeader),
                                new LogParam(action),
                                new LogParam(extlLink)));

                            break;
                        case LockRecordReturn.LockRecordError:

                            // FI 20200623 [XXXXX] SetErrorWarning
                            // FI 20200706 [XXXXX] Mis en commentaire puisque SetErrorWarning
                            //_task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6011), 4,
                                new LogParam(LogHeader),
                                new LogParam(action),
                                new LogParam(extlLink),
                                new LogParam(actionExisting)));

                            //Spheres® génère une exception
                            ProcessState state = new ProcessState(ProcessStateTools.StatusEnum.ERROR, Cst.ErrLevel.LOCKUNSUCCESSFUL);
                            FireException(new SpheresException2("LockRecord", lockRecordExisting.ToString(), state));
                            break;
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne le code action qui alimentera la colonne EFSLOCK.ACTION
        /// </summary>
        /// <returns></returns>
        private string GetLockAction()
        {
            Nullable<ActionTypeEnum> actionType = GetActionType();
            string action;
            if (actionType.HasValue)
            {
                action = actionType.Value.ToString();
            }
            else
            {
                SetCaptureMode();
                action = _captureMode.ToString();
            }
            return action;
        }
        /// <summary>
        /// Retourne true s'il existe un cci dans le flux post mapping tel que isMissingMode = true
        /// <para>
        /// Dans ce mode de fonctionnement:
        /// <para>
        /// - Spheres® importe le trade en statut incomplet s'il existe un cci (isMissingMode = true) où la donnée est incorrectes (absente ou inconnue) 
        /// </para>
        /// <para>
        /// - Spheres® est en erreur s'il existe un cci autre (isMissingModeSpeciefied = false) en erreur
        /// </para>
        /// </para>
        /// </summary>
        /// <param name="pCCis"></param>
        /// <returns></returns>
        /// FI 20130701 [18798] Add IsMissingMode
        private Boolean IsMissingMode()
        {
            Boolean ret = false;

            CustomCaptureInfosBase ccisImport = _tradeImport.tradeInput.CustomCaptureInfos;
            for (int i = 0; i < ArrFunc.Count(ccisImport); i++)
            {
                if (ccisImport[i].IsMissingMode)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        ///  génération de 1 message Queue à destination su service TradeActionGen pour génération des évènements de frais
        /// </summary>
        /// FI 20160907 [21831] Add
        protected virtual void SendMQueueFeesEventsGen()
        {
        }

        /// <summary>
        /// Génération de 1 à n message Queue à destination du service EventsGen pour génration des évènements 
        /// </summary>
        /// FI 20160907 [21831] Add
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected virtual void SendMqueueEventsGen()
        {
            if ((BoolFunc.IsTrue(GetParameter(TradeImportCst.isPostToEventsGen))))
            {
                #region SendMQueue for Event Calculation
                //si l'importation n'a pas eu lieu (ex condition non respectée alors captureGenImport.IdT =0) 
                //20100317 PL-StatusBusiness
                if ((IdT > 0) && (!CaptureGen.IsInputIncompleteAllow(CaptureMode)))
                {
                    // L'idéal aurait été de recharcher tradeCaptureGen via la méthode Load 
                    // mais cette méthode est bien trop lourde pour recherche un simple IDT 
                    // de plus elle ne fonctionne pas en mode transactionnel
                    if (CommonInput.IsInstrumentEvents())
                    {
                        int[] idTUnderlying = null;
                        if (CommonInput.Product.IsDebtSecurityTransaction)
                            idTUnderlying = GetIdUnderlying();


                        int nbPostedSubMsg = ArrFunc.Count(idTUnderlying) + 1;

                        if (0 < nbPostedSubMsg)
                        {
                            // EG 20121208 Mise à jour du compteur de message de la tâche en cours (ajout des messages postés)                            
                            _task.Process.Tracker.AddPostedSubMsg(nbPostedSubMsg, _task.Process.Session);
                            SendMQueue(IdT, idTUnderlying);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 301), 4,
                                new LogParam(Ressource.GetString(Cst.ProcessTypeEnum.EVENTSGEN.ToString())),
                                new LogParam(Cst.ProcessTypeEnum.EVENTSGEN),
                                new LogParam(TradeKey),
                                new LogParam(LogTools.IdentifierAndId(CommonInput.Identifier, IdT))));
                        }
                    }
                    else
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        // FI 20200623 [XXXXX] use SetErrorWarning delegate
                        //_task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                        SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, string.Format("Event generation is unchecked for <b>{0}</b>", CommonInput.SQLInstrument.DisplayName), 4,
                            new LogParam(Ressource.GetString(Cst.ProcessTypeEnum.EVENTSGEN.ToString())),
                            new LogParam(Cst.ProcessTypeEnum.EVENTSGEN),
                            new LogParam(TradeKey),
                            new LogParam("id: " + IdT.ToString())));
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Send MQueue For Event Calculation, used by TradeImport
        /// <para></para>
        /// </summary>
        /// <param name="pIdT">Id non significatif du trade</param>
        /// <param name="pIdtUnderlying">Liste des sous-jacent de type trade créé(titre) </param>
        // EG 20131024 Add CaptureMode parameter
        private void SendMQueue(int pIdT, int[] pIdtUnderlying)
        {
            // Alimentation d'un array avec les différents IDT
            // pIDT (idT du trade) doit obligatoirement être le dernier item de l'array 
            // les évènements du trade sont à générés après ceux des titres créés
            //Alimentation avec les  pIdtUnderlying
            Array targetArray = Array.CreateInstance(typeof(int), ArrFunc.Count(pIdtUnderlying) + 1);
            if (null != pIdtUnderlying)
                pIdtUnderlying.CopyTo(targetArray, 0);
            //Alimentation avec pIdT
            int[] idt = (int[])targetArray;
            idt[ArrFunc.Count(idt) - 1] = pIdT;

            Boolean isDelEvent = false; //CheckAndRecord se charge dejà du delete
            EventsGenMQueue[] mQueue = CaptureTools.GetMQueueForEventProcess(_task.Cs, idt, isDelEvent, _task.Requester);
            if (ArrFunc.IsFilled(mQueue))
            {
                MQueueTaskInfo taskInfo = new MQueueTaskInfo
                {
                    connectionString = _task.Cs,
                    Session = _task.Session,
                    process = mQueue[0].ProcessType,
                    mQueue = (MQueueBase[])mQueue,
                };

                MQueueSendInfo sendInfo = EFS.SpheresService.ServiceTools.GetMqueueSendInfo(Cst.ProcessTypeEnum.EVENTSGEN, _task.AppInstance);
#if DEBUG
                //PL for debug on SVR-DB01 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                if (Environment.MachineName == "DWS-136")
                {
                    if (!sendInfo.IsInfoValid)
                    {
                        sendInfo.MOMSetting.MOMType = Cst.MOM.MOMEnum.FileWatcher;
                        //sendInfo.MOMSetting.MOMPath = @"\\SVR-DB01\Queues\Queue_v8.1";
                        sendInfo.MOMSetting.MOMPath = @"C:\SpheresServices\Queue";
                    }
                }
                //PL for debug on SVR-DB01 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
#endif

                taskInfo.sendInfo = sendInfo;

                int idTRK_L = _task.Requester.idTRK;
                MQueueTaskInfo.SendMultiple(taskInfo, ref idTRK_L);
            }

        }

        /// <summary>
        /// Vérifie que les items représenant des devises de la liste de CustomCaptureInfo sont correctement alimentés avec des devises existantes (Lecture de la table CURRENCY)
        /// <para>Si tel n'est pas le cas, alimentation pour chacun d'eux de la propriété errorMsg</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCcis">Liste de ccis à contrôles</param>
        /// FI 20201105 [25554] Add Method
        protected static void CheckCciCurrency(string pCS, IDbTransaction pDbTransaction, IEnumerable<CustomCaptureInfo> pCcis)
        {
            IEnumerable<IGrouping<string, CustomCaptureInfo>> currencies = (from item in pCcis.Where(x => StrFunc.IsFilled(x.NewValue) && x.ClientId.EndsWith("currency"))
                                                                            select item).GroupBy(y => y.NewValue);
            if (currencies.Count() > 0)
            {
                string query = string.Empty;
                foreach (IGrouping<string, CustomCaptureInfo> item in currencies)
                {
                    string queryItem = $"select IDC from dbo.CURRENCY where IDC = {DataHelper.SQLString(item.Key)}";
                    query += queryItem + Cst.CrLf + SQLCst.UNIONALL;
                }
                query = StrFunc.Before(query, SQLCst.UNIONALL, OccurenceEnum.Last);

                using (DataTable dt = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(pCS), pDbTransaction, query))
                {
                    foreach (IGrouping<string, CustomCaptureInfo> item in currencies.Where(y => ArrFunc.IsEmpty(dt.Select($"IDC ='{y.Key}'"))))
                    {
                        item.ToList().ForEach(
                            x => { x.ErrorMsg = $"{x.NewValue} not found"; }
                        );
                    }
                }
            }
        }

        /// <summary>
        ///  Initialisation de <see cref="CciPayment.DefaultISINVOICING"/> sur les frais des trades de marché (objet <see cref="CciPayment"/>) 
        /// </summary>
        /// FI 20240531 [WI900] Add
        private void InitDefaultISInvoicingOnCCIOppPayment()
        {
            if (CommonInput.CustomCaptureInfos is TradeCustomCaptureInfos tradeCustomCaptureInfos)
            {
                if ((tradeCustomCaptureInfos.CciTrade is CciTrade cciTrade) && ArrFunc.IsFilled(cciTrade.cciOtherPartyPayment))
                {
                    foreach (CciPayment cciOppPayment in cciTrade.cciOtherPartyPayment)
                    {
                        string cciInvoicingClientId = cciOppPayment.CciClientId(CciPayment.CciEnumPayment.paymentSource_feeInvoicing);

                        CustomCaptureInfoDynamicData cciImport = (CustomCaptureInfoDynamicData)_importCustomCaptureInfos[cciInvoicingClientId];
                        if (null != cciImport)
                            cciOppPayment.DefaultISINVOICING = BoolFunc.IsTrue(cciImport.NewValue);
                    }
                }
            }
        }
        #endregion
    }
}
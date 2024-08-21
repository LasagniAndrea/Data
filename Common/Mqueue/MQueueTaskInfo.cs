using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Acknowledgment;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;


namespace EFS.Process
{

    /// <summary>
    /// 
    /// </summary>
    public partial class MQueueTaskInfo
    {
        #region Members
        public MQueueBase[] mQueue;
        public IdInfo[] idInfo;
        public MQueueparameters mQueueParameters;
        /// <summary>
        /// Destination des messages Queue 
        /// <para>Si null, Spheres® utilise les informations présentes dans le fhicher de config</para>
        /// </summary>
        public MQueueSendInfo sendInfo;
        /// <summary>
        /// Process demandé 
        /// </summary>
        public Cst.ProcessTypeEnum process;
        public string connectionString;
        public AppSession Session;
        /// <summary>
        ///  Propriétés pour alimentation du Tracker
        ///  <para>Doit être != null si insertion dans TRACKER_L</para>
        /// </summary>
        public TrackerAttributes trackerAttrib;

        public RegisteredWaitHandle handle;
        /// <summary>
        /// Génère une exception en plus d'un enregistrement dans le tracker en cas d'exception rencontrée pendant l'envoi des msgQueue
        /// </summary>
        /// FI 20131219 [19374] add member 
        public Boolean isExceptionOnTrackerMode;
        #endregion Members
        #region Constructors
        public MQueueTaskInfo()
        {
            
        }
        #endregion Constructors
        #region Method
        /// <summary>
        /// Alimentation des données permettant de disposer du numéro de Trade dans 
        /// une ligne du tracker sur un traitement de dénouement manuel 
        /// </summary>
        /// EG 20221212 [WI496] New
        public void SetTrackerTradeIdentifier()
        {
            if (1 == ArrFunc.Count(mQueue))
            {
                MQueueBase _queue = mQueue[0];
                Cst.PosRequestTypeEnum? _process = ReflectionTools.ConvertStringToEnumOrNullable<Cst.PosRequestTypeEnum>(_queue.LibProcessType);
                if (_process.HasValue)
                {
                    if ((Cst.PosRequestTypeEnum.RequestTypeManualOption & _process.Value) == _process.Value)
                    {
                        if (_queue.idInfoSpecified)
                        {
                            List<DictionaryEntry> lstData = new List<DictionaryEntry>
                            {
                                new DictionaryEntry("IDDATA", _queue.idInfo.id),
                                new DictionaryEntry("IDDATAIDENT", "TRADE"),
                                new DictionaryEntry("IDDATAIDENTIFIER", _queue.idInfo.idInfos[3].Value.ToString())
                            };
                            trackerAttrib.info = lstData;
                        }
                    }
                }
            }
        }

        

        /// <summary>
        ///  Mise en place d'un accusé de traitement de type <see cref="AckWebSessionSchedule"/>
        /// </summary>
        /// <param name="idInfo"></param>
        /// <returns></returns>
        public void SetTrackerAckWebSessionSchedule(IdInfo idInfo)
        {
            trackerAttrib.acknowledgment = new TrackerAcknowledgmentInfo();
            trackerAttrib.acknowledgment.SetAckWebSessionSchedule(idInfo);
        }
        #endregion
    }

    

    /// <summary>
    /// 
    /// </summary>
    public partial class MQueueTaskInfo
    {

        /// <summary>
        /// Postage de messages à destination du MOM (FileWatcher, MSMQ, ...) avec insertion dans TRACKER_L (usage d'un thread indépendant)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProcess"></param>
        /// <param name="pGProduct"></param>
        /// <param name="pCaller"></param>
        /// <param name="pSession"></param>
        /// <param name="pListMQueue"></param>
        ///<remarks>NB: SendMultiple peut être utilisé pour n'envoyer qu'un seul message</remarks> 
        /// EG 20150106 New
        /// EG 20221212 [WI496] New
        public static void SetAndSendMultipleThreadPool(string pCS, Cst.ProcessTypeEnum pProcess, string pGProduct, string pCaller, AppSession pSession, List<MQueueBase> pListMQueue)
        {
            MQueueTaskInfo taskInfo = new MQueueTaskInfo
            {
                process = pProcess,
                connectionString = pCS,
                Session = pSession,
                trackerAttrib = new TrackerAttributes()
                {
                    process = pProcess,
                    gProduct = pGProduct,
                    caller = pCaller
                },

                mQueue = pListMQueue.ToArray()
            };

            taskInfo.SetTrackerAckWebSessionSchedule(ArrFunc.Count(taskInfo.mQueue) == 1 ? taskInfo.mQueue[0].idInfo : null);

            // EG 20221212 [WI496] New
            taskInfo.SetTrackerTradeIdentifier();

            SendMultipleThreadPool(taskInfo, true);
        }

        /// <summary>
        /// Postage de messages à destination du MOM (FileWatcher, MSMQ, ...) avec insertion dans TRACKER_L (Eventuellement dans un thread indépendant)
        /// </summary>
        /// <param name="pTaskInfo"></param>
        /// <param name="pIsDefaultThreadPool">Mode ThreadPool. Valeur par défaut lorsque le paramétrage ThreadPool est absent dans le fichier de configuration</param>
        ///<remarks>NB: SendMultiple peut être utilisé pour n'envoyer qu'un seul message</remarks> 
        /// EG 20150106 New
        public static void SendMultipleThreadPool(MQueueTaskInfo pTaskInfo, bool pIsDefaultThreadPool)
        {
            bool isThreadPool = (bool)SystemSettings.GetAppSettings("ThreadPool", typeof(Boolean), pIsDefaultThreadPool);
            if (isThreadPool)
            {
                using (AutoResetEvent autoResetEvent = new AutoResetEvent(false))
                {
                    pTaskInfo.handle = ThreadPool.RegisterWaitForSingleObject(autoResetEvent, new WaitOrTimerCallback(SendMultipleForThreadPool), pTaskInfo, 1000, false);
                    Thread.Sleep(1000);
                    autoResetEvent.Set();
                }
            }
            else
            {
                SendMultipleForThreadPool(pTaskInfo, false);
            }
        }

        /// <summary>
        /// Postage de messages à destination du MOM (FileWatcher, MSMQ, ...) avec insertion dans TRACKER_L
        /// </summary>
        /// <param name="pTaskInfo"></param>
        /// <param name="pTimeOut"></param>
        ///<remarks>NB: SendMultiple peut être utilisé pour n'envoyer qu'un seul message</remarks> 
        private static void SendMultipleForThreadPool(object pTaskInfo, bool pTimeOut)
        {
            int idTRK = 0;
            SendMultiple((MQueueTaskInfo)pTaskInfo, pTimeOut,  ref idTRK);
        }

        /// <summary>
        /// Postage de messages à destination du MOM (FileWatcher, MSMQ, ...) avec insertion dans TRACKER_L (Usage du thread courant)
        /// </summary>
        /// <param name="pTaskInfo"></param>
        /// <returns>(bool retIsOk, string retErrMsg)</returns> 
        /// <returns></returns>
        public static (bool retIsOk, string retErrMsg) SendMultiple(object pTaskInfo)
        {
            int idTRK = 0;
            return SendMultiple((MQueueTaskInfo)pTaskInfo, false, ref idTRK);
        }

        /// <summary>
        /// Postage de messages à destination du MOM (FileWatcher, MSMQ, ...) avec insertion éventuelle dans TRACKER_L (Usage du thread courant)
        /// </summary>
        /// <param name="pTaskInfo"></param>
        /// <param name="pIdTRK">si 0 alors insertion d'un enregistrement dans TRACKER_L</param>
        /// <returns>(bool retIsOk, string retErrMsg)</returns>
        public static (bool retIsOk, string retErrMsg) SendMultiple(MQueueTaskInfo pTaskInfo, ref int pIdTRK)
        {
            return SendMultiple(pTaskInfo, false,  ref pIdTRK);
        }

        

        /// <summary>
        /// Postage de messages à destination du MOM (FileWatcher, MSMQ, ...) avec insertion éventuelle dans TRACKER_L
        /// </summary>
        /// <param name="pTaskInfo"></param>
        /// <param name="pTimeOut"></param>
        /// <param name="pIdTRK">si 0 alors insertion d'un enregistrement dans TRACKER_L</param>
        /// FI 20221230 [XXXXX] Refactoring
        private static (bool retIsOk, string retErrMsg) SendMultiple(MQueueTaskInfo pTaskInfo, bool pTimeOut,  ref int pIdTRK)
        {
            bool isInsertTraker = (pIdTRK == 0);
            (bool IsOk, string ErrMsg) ret = (true, string.Empty);
            Tracker tracker = null;

            try
            {
                if (false == pTimeOut)
                {
                    #region INIT: Identification du nombre de message à poster
                    int nbSend = 0;
                    if (ArrFunc.IsFilled(pTaskInfo.mQueue))
                        nbSend = ArrFunc.Count(pTaskInfo.mQueue);
                    else if (ArrFunc.IsFilled(pTaskInfo.idInfo))
                        nbSend = ArrFunc.Count(pTaskInfo.idInfo);
                    if (0 == nbSend)
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "No Message to Send");

                    if (pTaskInfo.Session == null)
                        throw new NullReferenceException("Session is null");

                    if (isInsertTraker && (null == pTaskInfo.trackerAttrib))
                        throw new NullReferenceException("trackerAttrib is null");
                    #endregion INIT

                    #region MOM Setting: Initialisation des caractéristiques du MOM à utiliser pour le postage des messages
                    //PL 20220614 J'ai remonté cette region "MOM Setting" au dessus de la region suivante "TRACKER" et y ai rajouté un appel à la nouvelle méthode "CheckMOMSettings"
                    //            NB: l'appel à CheckMOMSettings() avant l'écriture dans le Tracker permet d'éviter une écriture dans celui-ci en cas de MOM incorrect.
                    MQueueSendInfo mqSendInfo = pTaskInfo.sendInfo;
                    if (null == mqSendInfo)
                    {
                        mqSendInfo = new MQueueSendInfo
                        {
                            MOMSetting = LoadMOMSettings(pTaskInfo.process, ArrFunc.IsFilled(pTaskInfo.mQueue) ? pTaskInfo.mQueue[0] : null)
                        };
                    }

                    //PL 20221025 New signature of CheckMOMSettings() with 3 parameters
                    mqSendInfo.MOMSetting.CheckMOMSettings(pTaskInfo.connectionString, pTaskInfo.process, pTaskInfo.Session.IdA_Entity);
                    #endregion MOM Setting

                    #region TRACKER: Insert éventuel. Notamment dans le cadre d'une nouvelle demande.
                    // PM 20080519 Ecriture du log dans la base données en fonction de pUpdateTracker
                    // FI/EG 20121105 Insert d'une nouvelle ligne dans le tracker uniquement si pIdTRK_L==0
                    MQueueRequester queueRequester = null;
                    if (isInsertTraker)
                    {
                        tracker = new Tracker(pTaskInfo.connectionString)
                        {
                            ProcessRequested = pTaskInfo.process,
                            Status = ProcessStateTools.StatusEnum.PENDING,// FI 20201102 [XXXXX] Statut Pending (la demande est en attente de traitement par les services)
                            ReadyState = ProcessStateTools.ReadyStateEnum.REQUESTED,
                            Group = pTaskInfo.trackerAttrib.BuildTrackerGroup(),
                            IdData = pTaskInfo.trackerAttrib.BuildTrackerIdData(),
                            Data = pTaskInfo.trackerAttrib.BuildTrackerData()
                        };

                        if (null != pTaskInfo.trackerAttrib.acknowledgment)
                            tracker.Ack = pTaskInfo.trackerAttrib.acknowledgment;

                        tracker.Insert(pTaskInfo.Session, nbSend);
                        pIdTRK = tracker.IdTRK_L;

                        queueRequester = new MQueueRequester(tracker.IdTRK_L, tracker.Request.Session, tracker.Request.DtRequest);
                    }
                    #endregion TRACKER

                    #region SEND: Postage des messages dans le MOM
                    if (ArrFunc.IsFilled(pTaskInfo.mQueue))
                    {
                        MQueueBase[] mQueue = pTaskInfo.mQueue;
                        for (int i = 0; i < mQueue.GetLength(0); i++)
                        {
                            if (mQueue[i].header.requesterSpecified)
                            {
                                mQueue[i].header.requester.idTRK = pIdTRK;
                                mQueue[i].header.requester.idTRKSpecified = true;
                            }
                            else if (isInsertTraker)
                            {
                                mQueue[i].header.requester = queueRequester;
                                mQueue[i].header.requesterSpecified = (null != queueRequester);
                            }

                            MQueueTools.Send(mQueue[i], mqSendInfo);
                        }
                    }
                    else if (ArrFunc.IsFilled(pTaskInfo.idInfo))
                    {
                        MQueueBase mQueue = MQueueTools.GetMQueueByProcess(pTaskInfo.process);
                        if (mQueue == null)
                            throw new InvalidProgramException($"Message queue null for {pTaskInfo.process}");

                        for (int i = 0; i < pTaskInfo.idInfo.GetLength(0); i++)
                        {

                            MQueueAttributes mQueueAttributes = new MQueueAttributes()
                            {
                                connectionString = pTaskInfo.connectionString,
                                parameters = pTaskInfo.mQueueParameters,
                                requester = queueRequester,
                                id = pTaskInfo.idInfo[i].id,
                                idInfo = pTaskInfo.idInfo[i]
                            };

                            mQueue.Set(mQueueAttributes);

                            MQueueTools.Send(mQueue, mqSendInfo);
                        }
                    }
                    #endregion SEND
                }
            }
            catch (Exception ex) 
            { 
            
                ret.IsOk = false;
                ret.ErrMsg = ExceptionTools.GetMessageExtended(ex);

                if (isInsertTraker)
                {
                    LogException(pTaskInfo, pIdTRK, ex);

                    tracker.Status = ProcessStateTools.StatusEnum.ERROR;
                    tracker.Update(pTaskInfo.Session);
                }

                if (pTaskInfo.isExceptionOnTrackerMode)
                    throw;
            }
            finally
            {
                if (false == pTimeOut)
                {
                    if (null != pTaskInfo.handle)
                        pTaskInfo.handle.Unregister(null);

                    //PL 20220614 Comment next line (IDE0059 Assignation inutile)
                    //taskInfo = null;
                }
            }
            return ret;
        }

        /// <summary>
        /// Alimentation log et de la trace
        /// </summary>
        /// <param name="pTaskInfo"></param>
        /// <param name="pIdTRK"></param>
        /// <param name="ex"></param>
        /// FI 20240111 [WI793] Add Method
        private static void LogException(MQueueTaskInfo pTaskInfo, int pIdTRK, Exception ex)
        {
            // Rapel Logger et ProcessLog alimente la trace si Erreur

            SpheresException2 exception = SpheresExceptionParser.GetSpheresException(ex.Message, ex);
            if (AppInstance.IsSpheresWebApp(AppInstance.MasterAppInstance.AppName))
            {
                // Demande de traitement effectuée via l'application web => Alimentation de PROCESS_L/PROCESSDET_L si le tracker a été ajouté
                if (pIdTRK > 0)
                {
                    if (LoggerManager.IsInitialized)
                    {
                        LogScope scope = new LogScope(pTaskInfo.process.ToString(), pTaskInfo.connectionString, pIdTRK);

                        LoggerScope logger = new LoggerScope();
                        logger.BeginScope(OTCmlHelper.GetDateSysUTC, scope);
                        logger.Log(new LoggerData(LogLevelEnum.None, Ressource.GetString(pTaskInfo.process.ToString(), System.Globalization.CultureInfo.InvariantCulture)));
                        logger.Log(new LoggerData(exception));
                        logger.EndScope(ProcessStateTools.StatusEnum.ERROR.ToString());
                    }
                    else
                    {
                        ProcessLog processLog = new ProcessLog(pTaskInfo.connectionString, pTaskInfo.process, pTaskInfo.Session);
                        processLog.AddDetail(exception);
                        processLog.SetHeaderStatus(ProcessStateTools.StatusEnum.ERROR);
                        processLog.header.IdTRK_L = pIdTRK;
                        processLog.SQLWrite();
                    }
                }
                else
                    AppInstance.TraceManager.TraceError(nameof(MQueueTaskInfo), ExceptionTools.GetMessageAndStackExtended(ex));
            }
            else
            {
                // Demande de traitement effectuée par un service => PROCESS_L/PROCESSDET_L sont déjà alimenté. Appel au Logger classiquement 
                Logger.Log(new LoggerData(exception));
                Logger.Write();
            }
        }


        /// <summary>
        /// Retourne les informations spécifiques au MOM présentes dans le fichier de configuration
        /// <para>Lecture de AppSettingsSection</para>
        /// </summary>
        /// <param name="process"></param>
        /// <param name="mQueue"></param>
        /// <returns></returns>
        /// PL 20130621 Surcharge créée pour si besoin customiser un MOM spécifique pour les Tasks I/O
        private static MOMSettings LoadMOMSettings(Cst.ProcessTypeEnum process, MQueueBase mQueue)
        {
            MOMSettings ret = MOMSettings.LoadMOMSettings(process);
            
            if (process == Cst.ProcessTypeEnum.IO && (null != mQueue)) //mQueue == null Lorsque IO sollicité en mode Observer 
            {
                string suffix1 = @"_" + process.ToString();
                string suffix2 = @"_" + mQueue.GetStringValueIdInfoByKey("IN_OUT");    //Type de Task (ex. INPUT )
                string suffix3 = @"_" + mQueue.identifier;                             //Identifier de la Task (ex. EONIA)

                Cst.MOM.MOMEnum momType = Cst.MOM.GetMOMEnum(SystemSettings.GetAppSettings(Cst.MOM.MOMType + suffix1 + suffix2 + suffix3));
                if (momType == Cst.MOM.MOMEnum.Unknown)
                    momType = Cst.MOM.GetMOMEnum(SystemSettings.GetAppSettings(Cst.MOM.MOMType + suffix1 + suffix2));
                if (momType != Cst.MOM.MOMEnum.Unknown)
                    ret.MOMType = momType;

                string momPath = SystemSettings.GetAppSettings(Cst.MOM.MOMPath + suffix1 + suffix2 + suffix3);
                if (StrFunc.IsEmpty(momPath))
                    momPath = SystemSettings.GetAppSettings(Cst.MOM.MOMPath + suffix1 + suffix2);
                if (StrFunc.IsFilled(momPath))
                    ret.MOMPath = MOMSettings.TranslateMOMPath(momPath, Software.VersionBuild);
            }

            return ret;
        }
    }
}

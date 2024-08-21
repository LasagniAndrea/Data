using System;
using System.Collections;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Data;
using System.Messaging;
using System.Web.UI;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.Script.Serialization;

using System.Text.RegularExpressions;


using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;
using EfsML.Business;
using EFS.Common.Log;
using EFS.Referential;
/// <summary>
/// Ensemble de fonctions utilisées en webServices pour gérer l'affichage des messages de notification
/// de fin de traitement à chaque refresh du tracker
/// </summary>
namespace EFS.Spheres.WebServices
{
    /// <summary>
    /// WebService utilisé par le Tracker (sur Default.aspx)
    /// </summary>
    [WebService(Namespace = "http://EFS.Spheres.Services/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class TrackerWebService : WebService
    {
        private string m_FullPathRecipient;
        private MessageQueue m_QueueResponse;
        private MQueueSendInfo m_SendInfo;
        private string m_CS;
        private bool m_IsTrackerAlert;
        private Int64 m_TrackerAlertProcess;
        private string m_SessionID;
        private ArrayList m_DialogMessage;
        private Pair<string, string> retMessage;

        /// <summary>
        /// Message géré avec FileWatcher
        /// </summary>
        private bool IsFileWatcher
        {
            get
            {
                return (null != m_SendInfo) && (null != m_SendInfo.MOMSetting) && (Cst.MOM.MOMEnum.FileWatcher == m_SendInfo.MOMSetting.MOMType);
            }
        }
        /// <summary>
        /// Message géré avec MSMQueue
        /// </summary>
        private bool IsMsMQueue
        {
            get
            {
                return (null != m_SendInfo) && (null != m_SendInfo.MOMSetting) && (Cst.MOM.MOMEnum.MSMQ == m_SendInfo.MOMSetting.MOMType);
            }
        }
        /// <summary>
        /// Chemin complet de lecture/stockage des messages REPONSE
        /// </summary>
        private string FullPathRecipient
        {
            get
            {
                if (StrFunc.IsEmpty(m_FullPathRecipient) && (null != m_SendInfo))
                {
                    m_FullPathRecipient = m_SendInfo.MOMSetting.MOMPath;
                    if (IsFileWatcher)
                        m_FullPathRecipient += @"\";
                    m_FullPathRecipient += EFS.SpheresService.ServiceTools.GetQueueSuffix(m_CS, Cst.ServiceEnum.SpheresResponse, SessionTools.Collaborator_ENTITY_IDA);
                }
                return m_FullPathRecipient;
            }
        }
        private string FilterFile
        {
            get
            {
                StrBuilder filterFile = new StrBuilder();
                filterFile += EFS.SpheresService.ServiceTools.GetQueueSuffix(Cst.ServiceEnum.SpheresResponse)
                    + "???????";                            //15 ? -> ProcessType (avec X à Droite)   
                filterFile += "_" + "???????????????????";	//19 ? -> System date (yyyyMMddHHmmssfffff)
                for (int i = 1; i <= 3; i++)
                    filterFile += "_" + "????????";			//n*3 ? -> Trade status 
                filterFile += "_" + m_SessionID;			//SessionId
                filterFile += ".xml";
                return filterFile.ToString();
            }
        }
        /// <summary>
        /// Un message de réponse est-il attendu pour ce traitement
        /// </summary>
        /// <param name="pProcessType">Traitement</param>
        /// <returns></returns>
        private bool IsTrackerAlertProcess(Cst.ProcessTypeEnum pProcessType)
        {
            int i = int.Parse(Enum.Format(typeof(Cst.ProcessTypeEnum), pProcessType, "d"));
            return (0 < (m_TrackerAlertProcess & Convert.ToInt64(Math.Pow(2, i))));
        }


        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        /// <summary>
        /// Lecture des messages de réponse
        /// </summary>
        /// EG 20221029 [XXXXX] Refactoring (Suite lenteur sur PEEK des reponses de traitement sur MSSMQ)
        public Pair<string, string> GetResponseRecipient()
        {
            try
            {
                m_DialogMessage = new ArrayList();
                m_IsTrackerAlert = SessionTools.IsTrackerAlert;
                m_TrackerAlertProcess = SessionTools.TrackerAlertProcess;
                m_CS = SessionTools.CS;
                m_SessionID = SessionTools.SessionID;
                m_SendInfo = new MQueueSendInfo()
                {
                    MOMSetting = MOMSettings.LoadMOMSettings(Cst.ProcessTypeEnum.NA)
                };

                if (m_IsTrackerAlert)
                {
                    if ((null != m_SendInfo) && (m_SendInfo.IsInfoValid))
                    {
                        if (IsFileWatcher)
                        {
                            CallBackFileWatcher();
                        }
                        else if (IsMsMQueue)
                        {
                            m_QueueResponse = new MessageQueue(FullPathRecipient);
                            CallBackMSMQueue();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AspTools.WriteLogException(this, ex);
                string errMessage = $"An error occurred while processing your request.{Cst.HTMLBreakLine}{Cst.HTMLBreakLine}Message: {ExceptionTools.GetMessageExtended(ex)}";
                retMessage = new Pair<string, string>(LevelStatusTools.StatusError.ToLower(), errMessage);
            }
            return retMessage;
        }


        /// <summary>
        /// Lecture des messages de réponse à partir d'une queue MSMQUEUE (SYNCHRONE)
        /// </summary>
        /// EG 20221029 [XXXXX] New (Suite lenteur sur PEEK des reponses de traitement sur MSSMQ)
        protected void CallBackMSMQueue()
        {
            retMessage = new Pair<string, string>();
            MessageEnumerator mqe = null;
            try
            {
                string lblMessage = string.Empty;
                if (StrFunc.IsFilled(m_SessionID))
                {
                    int i = 0;
                    mqe = m_QueueResponse.GetMessageEnumerator2();
                    mqe.Reset();
                    bool isContinue = mqe.MoveNext();
                    if (isContinue)
                    {
                        while (isContinue)
                        {
                            lblMessage = mqe.Current.Label;
                            if (lblMessage.IndexOf("_" + m_SessionID) > -1)
                            {
                                ResponseRequestMQueue queue = (ResponseRequestMQueue)MQueueTools.ReadFromMessage(mqe.RemoveCurrent());
                                if (m_IsTrackerAlert && IsTrackerAlertProcess(queue.requestProcess))
                                {
                                    AddDialogMessage(queue);
                                    if (i == 11)
                                        isContinue = false;
                                    i++;
                                }
                            }
                            else
                                isContinue = mqe.MoveNext();
                        }
                    }
                }
                retMessage.First = LevelStatusTools.StatusSuccess.ToLower();
                retMessage.Second = DisplayDialogMessage();
            }
            catch (MessageQueueException mqex)
            {
                if (mqex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout || mqex.MessageQueueErrorCode == MessageQueueErrorCode.MessageAlreadyReceived)
                {
                    if (ArrFunc.IsFilled(m_DialogMessage))
                    {
                        retMessage.First = LevelStatusTools.StatusSuccess.ToLower();
                        retMessage.Second = DisplayDialogMessage();
                    }
                }
                else
                {
                    throw;
                }

            }
            finally
            {
                if (null != mqe) { mqe.Dispose(); }
            }
        }

        /// <summary>
        /// Lecture et affichage des messages de réponse en mode FileWatcher (SYNCHRONE)
        /// </summary>
        private void CallBackFileWatcher()
        {
            retMessage = new Pair<string, string>();

            if (StrFunc.IsFilled(FullPathRecipient))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(FullPathRecipient);
                if (directoryInfo.Exists)
                {
                    FileInfo[] fileInfos = directoryInfo.GetFiles(FilterFile);
                    int i = 0;
                    if (ArrFunc.IsFilled(fileInfos))
                    {
                        foreach (FileInfo fileInfo in fileInfos)
                        {
                            ResponseRequestMQueue queue = (ResponseRequestMQueue)MQueueTools.ReadFromFile(fileInfo.FullName);
                            FileTools.FileDelete2(fileInfo.FullName);
                            if (IsTrackerAlertProcess(queue.requestProcess))
                            {
                                if (i <= 10)
                                    AddDialogMessage(queue);
                                else if (i == 11)
                                    break;
                                i++;
                            }
                        }
                    }
                    retMessage.First = LevelStatusTools.StatusSuccess.ToLower();
                    retMessage.Second = DisplayDialogMessage();
                }
            }
        }

        /// <summary>
        /// Message d'erreur 
        /// </summary>
        /// <returns></returns>
        private static string GetExceptionMessage(Exception pEx)
        {
            string ret = $"An error occurred while processing your request.{Cst.HTMLBreakLine}Message: {ExceptionTools.GetMessageExtended(pEx)}";
            return ret;
        }

        /// <summary>
        /// Ajout d'un message de réponse en fonction d'un message de réponse MQueue
        /// </summary>
        /// <param name="pQueue">Message de réponse</param>
        /// EG 20221029 [XXXXX] Refactoring (Suite lenteur sur PEEK des reponses de traitement sur MSSMQ)
        private void AddDialogMessage(ResponseRequestMQueue pQueue)
        {

            Cst.ProcessTypeEnum process = pQueue.requestProcess;
            string dtTracker = string.Empty;
            if (pQueue.header.requesterSpecified)
            {
                if (SessionTools.IsConnected)
                {
                    dtTracker = DtFuncExtended.DisplayTimestampUTC(pQueue.header.requester.date, new AuditTimestampInfo
                    {
                        Collaborator = SessionTools.Collaborator,
                        TimestampZone = SessionTools.AuditTimestampZone,
                        Precision = Cst.AuditTimestampPrecision.Minute
                    });
                }
                else
                {
                    dtTracker = DtFunc.DateTimeToStringDateISO(pQueue.header.requester.date);
                }
            }

            string message;
            if (pQueue.idInfoSpecified && ArrFunc.IsFilled(pQueue.idInfo.idInfos) &&
                (process == Cst.ProcessTypeEnum.IO))
                message = Ressource.GetString2("Msg_PROCESS_RESPONSE_IOTASK",
                    dtTracker,
                    pQueue.GetStringValueIdInfoByKey("identifier"),
                    pQueue.GetStringValueIdInfoByKey("displayName"));
            else if (process == Cst.ProcessTypeEnum.POSKEEPREQUEST)
            {
                message = GetMessageResponse(pQueue, dtTracker);
            }
            else
            {
                message = Ressource.GetString2("Msg_PROCESS_RESPONSE_GEN",
                    Ressource.GetString(process.ToString()), dtTracker, pQueue.nbMessage.ToString());
            }
            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusNoneEnum;
            if (0 < pQueue.nbError)
            {
                status = ProcessStateTools.StatusErrorEnum;
                message += Ressource.GetString("Msg_PROCESS_RESPONSE_ERROR");
            }
            else if (0 < pQueue.nbWarning)
            {
                status = ProcessStateTools.StatusWarningEnum;
                message += Ressource.GetString("Msg_PROCESS_RESPONSE_WARNING");
            }
            else if (0 < pQueue.nbNone)
            {
                status = ProcessStateTools.StatusNoneEnum;
                message += Ressource.GetString("Msg_PROCESS_RESPONSE_NONE");
            }
            if (pQueue.nbMessage > (pQueue.nbError + pQueue.nbWarning + pQueue.nbNone +  pQueue.nbSucces))
            {
                status = ProcessStateTools.StatusWarningEnum;
                message += Ressource.GetString("Msg_PROCESS_RESPONSE_INCOMPLET");
            }
            StringWriter stringWriter = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "white");
                writer.AddAttribute(HtmlTextWriterAttribute.Target, Cst.HyperLinkTargetEnum._blank.ToString());
                writer.AddAttribute(HtmlTextWriterAttribute.Href, SpheresURL.GetURL(IdMenu.Menu.TRACKER_L, pQueue.idTRK_L.ToString()));
                writer.RenderBeginTag(HtmlTextWriterTag.A);  // Start Tag A
                writer.Write(Ressource.GetString("ShowLog"));
                writer.RenderEndTag();  // End Tag A
                AddAdditionalLinkToResponse(writer, pQueue);
            }
            m_DialogMessage.Add(new JQuery.DialogMessage(status, message, stringWriter.ToString()));
        }

        /// <summary>
        /// Affichage du message final de réponse en fonction d'un message de réponse MQueue
        /// </summary>
        /// <param name="pQueue"></param>
        private string DisplayDialogMessage()
        {
            int i = 1;
            string ret = string.Empty;
            if ((null != m_DialogMessage) && (0 < m_DialogMessage.Count))
            {
                ProcessStateTools.StatusEnum status = ProcessStateTools.StatusNoneEnum;

                StringWriter stringWriter = new StringWriter();
                using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);  // Start Tag Table
                    foreach (JQuery.DialogMessage dialog in m_DialogMessage)
                    {
                        if (status != ProcessStateTools.StatusErrorEnum)
                        {
                            if (dialog.Status == ProcessStateTools.StatusErrorEnum)
                                status = ProcessStateTools.StatusErrorEnum;
                            else if (dialog.Status == ProcessStateTools.StatusWarningEnum)
                                status = ProcessStateTools.StatusWarningEnum;
                            else if (dialog.Status == ProcessStateTools.StatusUnknownEnum)
                                status = ProcessStateTools.StatusUnknownEnum;
                        }
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr); // Start Tag Tr
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, dialog.Status.ToString().ToLower(), true);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td); // Start Tag Td
                        writer.Write(dialog.Message.Replace(Cst.CrLf, Cst.HTMLBreakLine));
                        writer.RenderEndTag(); // End Tag Td

                        writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                        writer.RenderBeginTag(HtmlTextWriterTag.Td); // Start Tag Td
                        writer.Write(dialog.Link);
                        writer.RenderEndTag(); // End Tag Td
                        writer.RenderEndTag(); // End Tag Tr
                        if (i < m_DialogMessage.Count)
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // Start Tag Tr
                            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2", true);
                            writer.RenderBeginTag(HtmlTextWriterTag.Td); // Start Tag Td
                            writer.Write(Cst.HTMLSpace);
                            writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                            writer.RenderEndTag();
                            writer.RenderEndTag(); // End Tag Td
                            writer.RenderEndTag();
                        }
                        i++;
                    }
                    writer.RenderEndTag(); // End Tag Table 
                }
                ret = stringWriter.ToString();
            }
            return ret;
        }

        /// <summary>
        /// Structuration du message de réponse posté par le service de tenue de Position en fonction du type RequestType
        /// </summary>
        /// <param name="pQueue">Le message de réponse</parparam>
        /// <param name="pDtRequester">La date de la demande</param>
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin(Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
        private string GetMessageResponse(ResponseRequestMQueue pQueue, string pDtRequester)
        {
            string message;
            Nullable<Cst.PosRequestTypeEnum> requestType = pQueue.GetEnumValueIdInfoByKey<Cst.PosRequestTypeEnum>("REQUESTTYPE");
            // EG 20151102 [21465]
            if (requestType.HasValue && (requestType.Value != default))
            {
                string resResquestType = Ressource.GetString(requestType.ToString());
                string trade = pQueue.GetStringValueIdInfoByKey("TRADE");
                if (StrFunc.IsFilled(trade) && (Cst.PosRequestTypeEnum.UpdateEntry != requestType.Value))
                {
                    // Identifiant Trade
                    message = Ressource.GetString2("Msg_RESPONSE_POSREQUEST_TRADE",
                        Ressource.GetString(pQueue.requestProcess.ToString()),
                        resResquestType,
                        pDtRequester,
                        pQueue.GetStringValueIdInfoByKey("TRADE"),
                        pQueue.nbMessage.ToString());
                }
                else if ((Cst.PosRequestTypeEnum.EndOfDay == requestType) ||
                         (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin == requestType) ||
                         (Cst.PosRequestTypeEnum.ClosingDay == requestType) ||
                         (Cst.PosRequestTypeEnum.RemoveEndOfDay == requestType))
                {
                    // Entité/Chambre
                    message = Ressource.GetString2("Msg_RESPONSE_POSREQUEST_ENTITYCSSCUSTODIAN",
                        Ressource.GetString(pQueue.requestProcess.ToString()),
                        resResquestType,
                        pDtRequester,
                        pQueue.GetStringValueIdInfoByKey("ENTITY"),
                        pQueue.GetStringValueIdInfoByKey("CSSCUSTODIAN"),
                        pQueue.GetStringValueIdInfoByKey("DTBUSINESS"),
                        pQueue.nbMessage.ToString());

                }
                else
                {
                    // Clé de position
                    message = Ressource.GetString2("Msg_RESPONSE_POSREQUEST_KEYPOS",
                        Ressource.GetString(pQueue.requestProcess.ToString()),
                        resResquestType,
                        pDtRequester,
                        pQueue.GetStringValueIdInfoByKey("MARKET"),
                        pQueue.GetStringValueIdInfoByKey("ASSET"),
                        pQueue.GetStringValueIdInfoByKey("DEALER") + " [" +
                        pQueue.GetStringValueIdInfoByKey("BOOKDEALER") + "]",
                        pQueue.GetStringValueIdInfoByKey("CLEARER") + " [" +
                        pQueue.GetStringValueIdInfoByKey("BOOKCLEARER") + "]",
                        pQueue.nbMessage.ToString());
                }
            }
            else
            {
                // EG 20151102 [21465] Ajout Message générique du tracker
                string trackerMessage = GetTrackerMessage(pQueue.idTRK_L, SessionTools.Collaborator_Culture_ISOCHAR2);
                message = Ressource.GetString2("Msg_PROCESS_RESPONSE_GEN",
                    Ressource.GetString(pQueue.ProcessType.ToString()) + (StrFunc.IsFilled(trackerMessage) ? "\r\n\r\n" + trackerMessage : string.Empty),
                    pDtRequester,
                    pQueue.nbMessage.ToString());
            }
            return message;
        }

        /// <summary>
        /// Ajout des hyperlink pour accès au LOG et infos IO ou POSREQUEST
        /// </summary>
        /// <param name="pWriter"></param>
        /// <param name="pQueue"></param>
        private void AddAdditionalLinkToResponse(HtmlTextWriter pWriter, ResponseRequestMQueue pQueue)
        {
            string url = string.Empty;

            if (pQueue.requestProcess == Cst.ProcessTypeEnum.IO)
                url = SpheresURL.GetURL(IdMenu.Menu.IOTRACK, pQueue.idProcess.ToString(), null, SpheresURL.LinkEvent.href, Cst.ConsultationMode.Normal, "&P1=IDPROCESS_L", null);
            else if (pQueue.requestProcess == Cst.ProcessTypeEnum.POSKEEPREQUEST)
            {
                url = SpheresURL.GetURL(IdMenu.Menu.TrackerPosRequest, pQueue.idTRK_L.ToString(), pQueue.id.ToString(),
                    SpheresURL.LinkEvent.href, Cst.ConsultationMode.Normal, null, null);
            }

            if (StrFunc.IsFilled(url))
            {
                pWriter.RenderBeginTag(HtmlTextWriterTag.Br);
                pWriter.RenderEndTag();
                pWriter.AddAttribute(HtmlTextWriterAttribute.Class, "white");
                pWriter.AddAttribute(HtmlTextWriterAttribute.Target, Cst.HyperLinkTargetEnum._blank.ToString());
                pWriter.AddAttribute(HtmlTextWriterAttribute.Href, url);
                pWriter.RenderBeginTag(HtmlTextWriterTag.A);  // Start Tag A
                pWriter.Write(Ressource.GetString("ShowResult"));
                pWriter.RenderEndTag();  // End Tag A
            }
        }

        private string GetTrackerMessage(int pIdTRK_L, string pCulture)
        {
            string message = string.Empty;

            IDataReader dr = null;
            IDbTransaction dbTransaction = null;
            try
            {
                dbTransaction = DataHelper.BeginTran(m_CS, IsolationLevel.ReadUncommitted);
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(m_CS, "IDTRK_L", DbType.Int64, SQLCst.UT_ENUM_MANDATORY_LEN), pIdTRK_L);
                parameters.Add(new DataParameter(m_CS, "CULTURE", DbType.AnsiString, SQLCst.UT_CULTURE_LEN), pCulture);

                string sqlQuery = @"select tk.IDTRK_L, tk.SYSCODE, tk.SYSNUMBER, case when smd.SYSCODE is null and smd_gb.SYSCODE is null then 'Request' else" + Cst.CrLf;
                sqlQuery += DataHelper.SQLIsNull(m_CS, "smd.SHORTMESSAGE", "smd_gb.SHORTMESSAGE") + @"end as SHORTMESSAGETRACKER, 
                case when smd.SYSCODE is null and smd_gb.SYSCODE is null then 'No Message' else" + Cst.CrLf;
                sqlQuery += DataHelper.SQLReplace(m_CS, DataHelper.SQLReplace(m_CS, DataHelper.SQLReplace(m_CS, DataHelper.SQLReplace(m_CS, DataHelper.SQLReplace(m_CS,
                            DataHelper.SQLIsNull(m_CS, "smd.MESSAGE", "smd_gb.MESSAGE"),
                            "'{1}'", DataHelper.SQLIsNull(m_CS, "tk.DATA1", "'{1}'")),
                            "'{2}'", DataHelper.SQLIsNull(m_CS, "tk.DATA2", "'{2}'")),
                            "'{3}'", DataHelper.SQLIsNull(m_CS, "tk.DATA3", "'{3}'")),
                            "'{4}'", DataHelper.SQLIsNull(m_CS, "tk.DATA4", "'{4}'")),
                            "'{5}'", DataHelper.SQLIsNull(m_CS, "tk.DATA5", "'{5}'"));
                sqlQuery += @"end as MESSAGETRACKER
                from dbo.TRACKER_L tk
                left outer join dbo.SYSTEMMSG sm on (sm.SYSCODE = tk.SYSCODE) and (sm.SYSNUMBER = tk.SYSNUMBER)
                left outer join dbo.SYSTEMMSGDET smd on (sm.SYSCODE = sm.SYSCODE) and (smd.SYSNUMBER = sm.SYSNUMBER) and (smd.CULTURE = @CULTURE)
                left outer join dbo.SYSTEMMSGDET smd_gb on (smd_gb.SYSCODE = sm.SYSCODE) and (smd_gb.SYSNUMBER = sm.SYSNUMBER) and (smd_gb.CULTURE = 'en')
                where (tk.IDTRK_L = @IDTRK_L)" + Cst.CrLf;

                QueryParameters qryParameters = new QueryParameters(m_CS, sqlQuery.ToString(), parameters);

                dr = DataHelper.ExecuteReader(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                if (dr.Read())
                    message = dr["MESSAGETRACKER"].ToString();
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                {
                    dr.Dispose();
                }
                if (null != dbTransaction)
                    DataHelper.RollbackTran(dbTransaction);
            }
            return message;
        }

        /// <summary>
        /// Récupération des paramètres de Refresh du tracker 
        /// </summary>
        /// <returns></returns>
        // EG 20220629 [XXXXX] New 
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        /// EG 20221029 [XXXXX] Refactoring
        public Tuple<bool, int, string> TrackerRefreshParam()
        {
            return new Tuple<bool, int, string>(SessionTools.IsTrackerRefreshActive, SessionTools.TrackerRefreshInterval, SessionTools.TrackerHistoric);
        }

        /// <summary>
        /// Récupération de la date histo en cours pour Tracker
        /// </summary>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        // EG 20221030 New
        public string GetDateTrackerDetail()
        {
            string dateTracker = "null";
            string histo = SessionTools.TrackerHistoric;
            
            Nullable<DateTime> ret = null;
            if (StrFunc.IsFilled(histo) && ("Beyond" != histo))
                ret = new DtFunc().StringToDateTime("-" + histo);

            if (ret.HasValue)
                dateTracker = DtFuncML.DateTimeToStringDateISO(ret.Value);

            return dateTracker;
        }

    }
}


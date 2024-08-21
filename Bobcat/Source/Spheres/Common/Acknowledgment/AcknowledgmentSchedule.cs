using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Xml.Serialization;


using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.EfsSend;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.Process;

namespace EFS.Common.Acknowledgment
{

    /// <summary>
    ///  Delegue destiné à la journalisation d'une erreur 
    /// </summary>
    /// <param name="pSource"></param>
    /// <param name="pMessage">Message d'erreur</param>
    public delegate void TraceAcknowledgmentError(object pSource, string pMessage);


    /// <summary>
    /// Interface à respecter pour générer l'accusé d'un traitement (nbr de message en succès , en erreur, statut etc..)
    /// </summary>
    public interface IProcessAcknowledgment
    {
        /// <summary>
        /// 
        /// </summary>
        string CS { get; }

        /// <summary>
        ///  Identifiant sur lequel s'applique le traitement (Exemple IO => IdTask, IdentiantTask)
        /// </summary>
        IdData IdData { get; }

        /// <summary>
        /// Informations complémentaires 
        /// <para></para>
        /// </summary>
        IdInfo IdInfo { get; }

        /// <summary>
        ///  Identifiant Ext de la demande de traitement
        /// </summary>
        string ExtlId { get; }

        /// <summary>
        /// Id non significatif associé à la demande de traitement <see cref="Request"/> (exemple IDTRK_L)
        /// </summary>
        int RequestId { get; }

        /// <summary>
        ///  Demande de traitement 
        /// </summary>
        Request Request { get; }

        /// <summary>
        /// Id non significatif du log associé au process (voir <see cref="Process"/>)
        /// </summary>
        int IdLogProcess { get; }

        /// <summary>
        /// traitement demandé
        /// </summary>
        Cst.ProcessTypeEnum ProcessRequested { get; }

        /// <summary>
        /// Traitement à l'origine de l'accusé de traitement 
        /// </summary>
        Cst.ProcessTypeEnum Process { get; }

        /// <summary>
        /// Etat du traitement demandé
        /// </summary>
        ProcessStateTools.ReadyStateEnum ReadyState { get; }

        /// <summary>
        /// Statut du traitement demandé
        /// </summary>
        ProcessStateTools.StatusEnum Status { get; }

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IAckSchedule
    {
        /// <summary>
        /// Génération des accusés de traitement
        /// </summary>
        /// <param name="processAck"></param>
        /// <returns></returns>
        Cst.ErrLevel[] Generate(IProcessAcknowledgment processAck);
    }

    /// <summary>
    /// Liste des accusés de traitement
    /// </summary>
    [Serializable]
    public class AckSchedules : IAckSchedule
    {
        /// <summary>
        ///  
        /// </summary>
        TraceAcknowledgmentError _traceError;

        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlElement("webSession", typeof(AckWebSessionSchedule))]
        [XmlElement("sqlCommand", typeof(AckSQLCommandSchedule))]
        [XmlElement("messageQueue", typeof(AckMessageQueueSchedule))]
        [XmlElement("eventLog", typeof(AckEventLogSchedule))]
        [XmlElement("email", typeof(AckEmailSchedule))]
        [XmlElement("file", typeof(AckFileSchedule))]
        public AckScheduleBase[] Item;
        #endregion Members

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public AckSchedules()
        {
        }
        /// <summary>
        /// constructeur avec 1 accusé de traitement
        /// </summary>
        /// <param name="ackSchedule"></param>
        public AckSchedules(AckScheduleBase ackSchedule)
        {
            Item = new AckScheduleBase[1] { ackSchedule };
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        ///  Initialisation du delegué destiné à la journalisation des erreurs
        /// </summary>
        /// <param name="traceError"></param>
        public void InitTraceError(TraceAcknowledgmentError traceError)
        {
            _traceError = traceError;
        }


        /// <summary>
        ///  Génère les accusés de traitement et retourne un array de Cst.ErrLevel
        /// </summary>
        /// <param name="processAck"></param>
        /// <returns></returns>
        public Cst.ErrLevel[] Generate(IProcessAcknowledgment processAck)
        {
            Cst.ErrLevel[] codeReturn;
            
            if (0 < Item.Length)
            {
                //PrepareGenerate
                foreach (AckScheduleBase item in Item)
                {
                    try
                    {
                        item.PrepareGenerate(processAck);
                    }
                    catch (Exception ex)
                    {
                        TraceException(ex);
                        item.ackErrLevelSpecified = true;
                        item.ackErrLevel = Cst.ErrLevel.FAILURE;
                    }
                }

                //Generate (pour lorsque l'étape précédente est Ok)
                foreach (AckScheduleBase item in Item.Where(x=> x.ackErrLevel == Cst.ErrLevel.SUCCESS))
                {
                    try
                    {
                        item.Generate(processAck);
                    }
                    catch (Exception ex)
                    {
                        TraceException(ex);
                        item.ackErrLevelSpecified = true;
                        item.ackErrLevel = Cst.ErrLevel.FAILURE;
                    }
                }
                

                // Retour
                codeReturn = (from item in Item
                              select item.ackErrLevel).ToArray();
            }
            else
            {
                codeReturn = new Cst.ErrLevel[1] { Cst.ErrLevel.NOTHINGTODO };
            }

            return codeReturn;
        }

        /// <summary>
        /// Ecriture dans la trace de l'exception  via le delegue <see cref="TraceAcknowledgmentError"/> 
        /// </summary>
        /// <param name="ex"></param>
        private void TraceException(Exception ex)
        {
            if (null != _traceError)
                _traceError.Invoke(this, ExceptionTools.GetMessageAndStackExtended(ex));

        }
        #endregion Methods
    }


    /// <summary>
    /// Accusé de traitement (Acknowledge)
    /// </summary>
    [Serializable]
    public abstract class AckScheduleBase 
    {
        #region Parameters constants
        private const string ACK_REQUESTID = "%%REQUESTID%%";
        private const string ACK_TRKID = "%%TRKID%%";
        private const string ACK_EXTLID = "%%EXTLID%%";
        private const string ACK_GLOBALINFO = "%%GLOBALINFO%%";

        private const string ACK_NBPOSTEDMSG = "%%NBPOSTEDMSG%%";
        private const string ACK_NBPOSTEDSUBMSG = "%%NBPOSTEDSUBMSG%%";
        private const string ACK_NBNONESMSG = "%%NBNONESMSG%%"; 
        private const string ACK_NBSUCCESSMSG = "%%NBSUCCESSMSG%%";
        private const string ACK_NBWARNINGMSG = "%%NBWARNINGMSG%%";
        private const string ACK_NBERRORMSG = "%%NBERRORMSG%%";
        /// <summary>
        /// Représente le process initial
        /// </summary>
        /// FI 20141028 [20455] add
        private const string ACK_REQUESTEDPROCESSTYPE = "%%REQUESTEDPROCESSTYPE%%";
        /// <summary>
        /// Représente le dernier process (celui à l'origine de l'accusé de traitement) 
        /// </summary>
        private const string ACK_PROCESSTYPE = "%%PROCESSTYPE%%";
        private const string ACK_READYSTATE = "%%READYSTATE%%";
        private const string ACK_STATUS = "%%STATUS%%";
        #endregion Parameters constants

        #region Members

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool idSpecified;
        /// <summary>
        ///  Id
        /// </summary>
        [XmlAttributeAttribute()]
        public string id;

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool onstatusSpecified;
        /// <summary>
        ///  Liste des statuts acceptés pour déclenché l'accusé
        /// </summary>
        [XmlAttributeAttribute("onstatus")]
        public string onstatus;

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool ackErrLevelSpecified;
        /// <summary>
        ///  Valeur retour obtenue post génération de l'accusé
        /// </summary>
        [XmlAttributeAttribute("ackerrlevel")]
        public Cst.ErrLevel ackErrLevel;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsValid
        {
            get { return false; }
        }
        #endregion Accessors

        #region Constructors
        public AckScheduleBase() { }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Génération de l'accusé de traitement
        /// </summary>
        /// <param name="processAck"></param>
        /// <returns></returns>
        public virtual void Generate(IProcessAcknowledgment processAck)
        {
            throw new NotImplementedException("virtual method Generate have to be overridden");
        }

        /// <summary>
        ///  Préparation de l'accusé de traitement (Remplacement des mots clés)
        /// </summary>
        /// <param name="processAck"></param>
        /// <returns></returns>
        public virtual void PrepareGenerate(IProcessAcknowledgment processAck)
        {
            ackErrLevelSpecified = true;
            ackErrLevel = Cst.ErrLevel.SUCCESS;
            
            if (!IsValid)
            {
                ackErrLevel = Cst.ErrLevel.INCORRECTPARAMETER;
            }
            else
            {
                ReplaceKeyword(processAck);
                if (!IsStatusCompatible(processAck))
                    ackErrLevel = Cst.ErrLevel.NOTHINGTODO;
            }
        }

        /// <summary>
        ///  Remplacement des mots clés
        /// </summary>
        /// <param name="processAck"></param>
        protected virtual void ReplaceKeyword(IProcessAcknowledgment processAck)
        {
        }

        /// <summary>
        ///  Remplace les mots clés (autre que ACK_GLOBALINFO) présents dans <paramref name="data"/>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="processAck"></param>
        /// <returns></returns>
        /// FI 20141028 [20455] Modify
        protected static string ReplaceKeyword(string data, IProcessAcknowledgment processAck)
        {
            string ret = data;

            ret = ret.Replace(ACK_EXTLID, processAck.ExtlId);
            ret = ret.Replace(ACK_REQUESTEDPROCESSTYPE, processAck.ProcessRequested.ToString());
            ret = ret.Replace(ACK_PROCESSTYPE, processAck.Process.ToString());
            ret = ret.Replace(ACK_REQUESTID, processAck.RequestId.ToString());
            ret = ret.Replace(ACK_TRKID, processAck.RequestId.ToString());
            ret = ret.Replace(ACK_NBPOSTEDMSG, processAck.Request.PostedMsg.ToString());
            ret = ret.Replace(ACK_NBPOSTEDSUBMSG, processAck.Request.PostedSubMsg.ToString());
            ret = ret.Replace(ACK_NBNONESMSG, processAck.Request.SuccessMsg.ToString()); // FI 20201030 [25537]
            ret = ret.Replace(ACK_NBSUCCESSMSG, processAck.Request.SuccessMsg.ToString());
            ret = ret.Replace(ACK_NBWARNINGMSG, processAck.Request.WarningMsg.ToString()); 
            ret = ret.Replace(ACK_NBERRORMSG, processAck.Request.ErrorMsg.ToString());
            ret = ret.Replace(ACK_READYSTATE, processAck.ReadyState.ToString());
            ret = ret.Replace(ACK_STATUS, processAck.Status.ToString());

            return ret;
        }

        /// <summary>
        ///  Remplace les mots clés (ACK_GLOBALINFO inclus) présents dans <paramref name="data"/>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="processAck"></param>
        /// <returns></returns>
        /// FI 20160411 [22068] Modify
        protected static string ReplaceContents(string data, IProcessAcknowledgment processAck)
        {
            string ret = data;

            if (data.Contains(ACK_GLOBALINFO))
            {
                CSManager csManager = new CSManager(processAck.CS);
                int padLength = 30;
                StrBuilder globalInfo = new StrBuilder(string.Empty);

                globalInfo += new String('-', padLength * 2) + Cst.CrLf;
                globalInfo += "Connection string: " + csManager.GetCSAnonymizePwd() + Cst.CrLf;

                globalInfo += new String('-', padLength * 2) + Cst.CrLf;
                globalInfo += "Requested Process type: ".PadRight(padLength) + processAck.ProcessRequested.ToString() + Cst.CrLf;
                globalInfo += "Process type: ".PadRight(padLength) + processAck.Process.ToString() + Cst.CrLf;
                globalInfo += "Status: ".PadRight(padLength) + processAck.Status.ToString() + Cst.CrLf;
                globalInfo += "Ready state: ".PadRight(padLength) + processAck.ReadyState.ToString() + Cst.CrLf;
                if (null != processAck.IdData && processAck.IdData.id > 0)
                {
                    globalInfo += new String('-', padLength * 2) + Cst.CrLf;
                    globalInfo += "Data Id: ".PadRight(padLength) + processAck.IdData.id.ToString() + Cst.CrLf;
                    if (StrFunc.IsFilled(processAck.IdData.idIdent))
                        globalInfo += "Data IdIdent: ".PadRight(padLength) + processAck.IdData.idIdent.ToString() + Cst.CrLf;
                    if (StrFunc.IsFilled(processAck.IdData.idIdentifier))
                        globalInfo += "Data Identifier: ".PadRight(padLength) + processAck.IdData.idIdentifier.ToString() + Cst.CrLf;
                }
                globalInfo += new String('-', padLength * 2) + Cst.CrLf;
                globalInfo += "External Id: ".PadRight(padLength) + processAck.ExtlId + Cst.CrLf;
                globalInfo += "Request Id: ".PadRight(padLength) + processAck.RequestId.ToString() + Cst.CrLf;
                globalInfo += new String('-', padLength * 2) + Cst.CrLf;
                globalInfo += "Posted messages: ".PadRight(padLength) + processAck.Request.PostedMsg.ToString() + Cst.CrLf;
                globalInfo += "Posted linked messages: ".PadRight(padLength) + processAck.Request.PostedSubMsg.ToString() + Cst.CrLf;
                globalInfo += "Error messages: ".PadRight(padLength) + processAck.Request.ErrorMsg.ToString() + Cst.CrLf;
                globalInfo += "Warning messages: ".PadRight(padLength) + processAck.Request.WarningMsg.ToString() + Cst.CrLf;
                globalInfo += "Success messages: ".PadRight(padLength) + processAck.Request.SuccessMsg.ToString() + Cst.CrLf;
                globalInfo += "None messages: ".PadRight(padLength) + processAck.Request.NoneMsg.ToString() + Cst.CrLf;  // FI 20201030 [25537] Add
                //--------------------------------------------------------
                //PL 20141217 Newness 
                //--------------------------------------------------------
                globalInfo += new String('-', padLength * 2) + Cst.CrLf;
                globalInfo += "Local Timestamp: ".PadRight(padLength) + DateTime.Now.ToString("yyyyMMddHHmmssfffff") + Cst.CrLf;
                globalInfo += "UTC Timestamp: ".PadRight(padLength) + DateTime.UtcNow.ToString("yyyyMMddHHmmssfffff") + Cst.CrLf;
                //--------------------------------------------------------
                globalInfo += new String('-', padLength * 2) + Cst.CrLf;

                ret = ret.Replace(ACK_GLOBALINFO, globalInfo.ToString());
            }

            ret = ReplaceKeyword(ret, processAck);

            return ret;
        }


        /// <summary>
        /// Retourne true si l'accusé de traitement est à générer (en fonction du statut spécifié sur <paramref name="processAck"/> et du statut <see cref="onstatus" />)
        /// </summary>
        /// <param name="processAck"></param>
        /// <returns></returns>
        private bool IsStatusCompatible(IProcessAcknowledgment processAck)
        {
            return ((!onstatusSpecified) || $";{onstatus};".Contains(processAck.Status.ToString()));
        }

        #endregion Methods
    }

    /// <summary>
    /// Accusé de traitement
    /// TYPE: Fichier (Création d'un fichier)
    /// </summary>
    [Serializable]
    public class AckFileSchedule : AckScheduleBase
    {
        #region Members
        [XmlElementAttribute()]
        public string pathName;
        [XmlIgnoreAttribute()]
        public bool pathNameSpecified;
        [XmlElementAttribute()]
        public string fileName;
        [XmlIgnoreAttribute()]
        public bool fileNameSpecified;
        [XmlElementAttribute()]
        public string fileContents;
        [XmlIgnoreAttribute()]
        public bool fileContentsSpecified;
        #endregion Members
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public bool ExistKeyword
        {
            get
            {
                return (pathNameSpecified && pathName.Contains("%%"))
                || (fileNameSpecified && fileName.Contains("%%"))
                || (fileContentsSpecified && fileContents.Contains("%%"));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool IsValid
        {
            get { return pathNameSpecified && fileNameSpecified; }
        }
        #endregion Accessors
        #region Constructors
        public AckFileSchedule() { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Génération du fichier faisant office d'accusé de traitement.
        /// </summary>
        /// <param name="processAck"></param>
        /// <returns></returns>
        public override void Generate(IProcessAcknowledgment processAck)
        {
            ackErrLevelSpecified = true;
            
            if (!Directory.Exists(pathName))
            {
                ackErrLevel = Cst.ErrLevel.FOLDERNOTFOUND;
            }
            else
            {
                //Creation du fichier
                string fullName = pathName.Trim();
                if (!fullName.EndsWith(@"\"))
                    fullName += @"\";
                fullName += fileName.Trim();

                byte[] contents = System.Text.Encoding.UTF8.GetBytes(fileContents);

                ackErrLevel = FileTools.WriteBytesToFile(contents, fullName, FileTools.WriteFileOverrideMode.New); //NB: Cette méthode vérifie l'existence d'un fichier.
            }
            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processAck"></param>
        protected override void ReplaceKeyword(IProcessAcknowledgment processAck)
        {
            if (ExistKeyword)
            {
                pathName = ReplaceKeyword(pathName, processAck);
                fileName = ReplaceKeyword(fileName, processAck);
                if (fileContentsSpecified)
                {
                    fileContents = ReplaceContents(fileContents, processAck);
                }
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Accusé de traitement
    /// TYPE: Email (Envoi d'un e-mail)
    /// </summary>
    [Serializable]
    public class AckEmailSchedule : AckScheduleBase
    {
        #region Members
        [XmlElementAttribute()]
        public string server;
        [XmlIgnoreAttribute()]
        public bool serverSpecified;
        [XmlElementAttribute()]
        public string from;
        [XmlIgnoreAttribute()]
        public bool fromSpecified;
        [XmlElementAttribute()]
        public string to;
        [XmlIgnoreAttribute()]
        public bool toSpecified;
        [XmlElementAttribute()]
        public string cc;
        [XmlIgnoreAttribute()]
        public bool ccSpecified;
        [XmlElementAttribute()]
        public string bcc;
        [XmlIgnoreAttribute()]
        public bool bccSpecified;
        [XmlElementAttribute()]
        public string priority;
        [XmlIgnoreAttribute()]
        public bool prioritySpecified;
        [XmlElementAttribute()]
        public string subject;
        [XmlIgnoreAttribute()]
        public bool subjectSpecified;
        [XmlElementAttribute()]
        public string body;
        [XmlIgnoreAttribute()]
        public bool bodySpecified;
        #endregion Members
        #region Accessors
        private bool ExistKeyword
        {
            get
            {
                return (serverSpecified && server.Contains("%%"))
                    || (fromSpecified && from.Contains("%%"))
                    || (toSpecified && to.Contains("%%"))
                    || (ccSpecified && cc.Contains("%%"))
                    || (bccSpecified && bcc.Contains("%%"))
                    || (subjectSpecified && subject.Contains("%%"))
                    || (bodySpecified && body.Contains("%%"));
            }
        }
        public override bool IsValid
        {
            get { return serverSpecified && toSpecified && subjectSpecified; }
        }
        #endregion Accessors
        #region Constructors
        public AckEmailSchedule() { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Génération de l'e-mail faisant office d'accusé de traitement.
        /// </summary>
        /// <param name="processAck"></param>
        /// <returns></returns>
        public override void Generate(IProcessAcknowledgment processAck)
        {
            ackErrLevelSpecified = true;

            EfsSendSMTPPriorityEnum priorityEnum = EfsSendSMTPPriorityEnum.Normal;
            if (Enum.IsDefined(typeof(EfsSendSMTPPriorityEnum), priority))
                priorityEnum = (EfsSendSMTPPriorityEnum)Enum.Parse(typeof(EfsSendSMTPPriorityEnum), priority, true);

            SMTPNotification smtpNotification = new SMTPNotification(processAck.CS, server);
            string[] address = to.Split(';');
            foreach (string s in address)
            {
                smtpNotification.SendTo.Add(new EfsSendSMTPContact(s));
            }
            if (ccSpecified)
            {
                address = cc.Split(';');
                foreach (string s in address)
                {
                    smtpNotification.SendCc.Add(new EfsSendSMTPContact(s));
                }
            }
            if (bccSpecified)
            {
                address = bcc.Split(';');
                foreach (string s in address)
                {
                    smtpNotification.SendBcc.Add(new EfsSendSMTPContact(s));
                }
            }

            ackErrLevel = smtpNotification.Send(from, subject, body, priorityEnum);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processAck"></param>
        protected override void ReplaceKeyword(IProcessAcknowledgment processAck)
        {
            if (ExistKeyword)
            {
                server = ReplaceKeyword(server, processAck);
                to = ReplaceKeyword(to, processAck);
                subject = ReplaceKeyword(subject, processAck);
                
                if (fromSpecified)
                    from = ReplaceKeyword(from, processAck);

                if (ccSpecified)
                    cc = ReplaceKeyword(cc, processAck);

                if (bccSpecified)
                    bcc = ReplaceKeyword(bcc, processAck);

                if (bodySpecified)
                    body = ReplaceContents(body, processAck);
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Accusé de traitement (Acknowledge)
    /// TYPE: EventLog (Entrée dans le journal des événements de Windows®)
    /// </summary>
    [Serializable]
    public class AckEventLogSchedule : AckScheduleBase
    {
        #region Members
        [XmlElementAttribute()]
        public string logName;
        [XmlIgnoreAttribute()]
        public bool logNameSpecified;
        [XmlElementAttribute()]
        public string source;
        [XmlIgnoreAttribute()]
        public bool sourceSpecified;
        [XmlElementAttribute()]
        public string entryType;
        [XmlIgnoreAttribute()]
        public bool entryTypeSpecified;
        [XmlElementAttribute()]
        public int eventID;
        [XmlIgnoreAttribute()]
        public bool eventIDSpecified;
        [XmlElementAttribute()]
        public string category;
        [XmlIgnoreAttribute()]
        public bool categorySpecified;
        [XmlElementAttribute()]
        public string contents;
        [XmlIgnoreAttribute()]
        public bool contentsSpecified;
        #endregion Members
        #region Accessors
        private bool ExistKeyword
        {
            get
            {
                return (logNameSpecified && logName.Contains("%%"))
                    || (sourceSpecified && source.Contains("%%"))
                    || (entryTypeSpecified && entryType.Contains("%%"))
                    || (categorySpecified && category.Contains("%%"))
                    || (contentsSpecified && contents.Contains("%%"));
            }
        }
        public override bool IsValid
        {
            get { return logNameSpecified && sourceSpecified && entryTypeSpecified && eventIDSpecified; }
        }
        #endregion Accessors
        #region Constructors
        public AckEventLogSchedule() { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Génération de l'entrée dans le journal des événements de Windows® faisant office d'accusé de traitement.
        /// </summary>
        /// <param name="processAck"></param>
        /// <returns></returns>
        public override void Generate(IProcessAcknowledgment processAck)
        {
            ackErrLevelSpecified = true;
            ackErrLevel = Cst.ErrLevel.SUCCESS;

            if (Enum.IsDefined(typeof(EventLogEntryType), entryType))
            {
                string hostName = Environment.MachineName;

                EventLogEntryType levelEnum = (EventLogEntryType)Enum.Parse(typeof(EventLogEntryType), entryType, true);

                string[] data = null;
                if (contentsSpecified)
                {
                    data = new string[1] { contents };
                }

                EventLog_Categories categoryEnum = EventLog_Categories.Information;
                if (categorySpecified && Enum.IsDefined(typeof(EventLog_Categories), category))
                {
                    categoryEnum = (EventLog_Categories)Enum.Parse(typeof(EventLog_Categories), category, true);
                }
                else
                {
                    categorySpecified = false;
                }

                EventLog_EventId eventIdEnum = EventLog_EventId.SpheresServices_Error_Undefined;
                if (eventIDSpecified && Enum.IsDefined(typeof(EventLog_EventId), eventID))
                {
                    eventIdEnum = (EventLog_EventId)Enum.Parse(typeof(EventLog_EventId), eventID.ToString(), true);//GLOP vérifier eventID.ToString()
                }


                EventLogCharateristics elc = new EventLogCharateristics(hostName, logName, source, levelEnum, eventIdEnum, data);
                if (categorySpecified)
                {
                    elc.TaskCategory = categoryEnum;
                }

                using (EventLogEx eventLogEx = new EventLogEx(elc))
                {
                    eventLogEx.ReportEvent();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processAck"></param>
        protected override void ReplaceKeyword(IProcessAcknowledgment processAck)
        {
            if (ExistKeyword)
            {
                logName = ReplaceKeyword(logName, processAck);
                source = ReplaceKeyword(source, processAck);
                entryType = ReplaceKeyword(entryType, processAck);
                if (categorySpecified)
                    category = ReplaceKeyword(category, processAck);
                if (contentsSpecified)
                    contents = ReplaceContents(contents, processAck);
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Accusé de traitement (Acknowledge) de type: SQLCommand (Entrée dans une table SQL)
    /// </summary>
    [Serializable]
    public class AckSQLCommandSchedule : AckScheduleBase
    {
        #region Parameters/Parameter
        [Serializable]
        public class Parameters
        {
            #region Members
            [XmlElementAttribute()]
            public Parameter[] parameter;
            [XmlIgnoreAttribute()]
            public bool parameterSpecified;
            #endregion Members
            #region Constructors
            public Parameters() { }
            #endregion Constructors
        }
        [Serializable]
        public class Parameter
        {
            #region Members
            [XmlAttributeAttribute()]
            public string id;
            [XmlIgnoreAttribute()]
            public bool idSpecified;

            [XmlAttributeAttribute()]
            public string name;
            [XmlIgnoreAttribute()]
            public bool nameSpecified;

            [XmlAttributeAttribute()]
            public TypeData.TypeDataEnum datatype;
            [XmlIgnoreAttribute()]
            public bool datatypeSpecified;

            [XmlTextAttribute()]
            public string Value;
            #endregion Members
            #region Constructors
            public Parameter() { }
            #endregion Constructors
        }
        #endregion Parameters/Parameter
        #region Members
        [XmlElementAttribute()]
        public string connectionString;
        [XmlIgnoreAttribute()]
        public bool connectionStringSpecified;
        [XmlElementAttribute()]
        public string commandType;
        [XmlIgnoreAttribute()]
        public bool commandTypeSpecified;
        [XmlElementAttribute()]
        public string commandText;
        [XmlIgnoreAttribute()]
        public bool commandTextSpecified;
        [XmlElementAttribute()]
        public Parameters parameters;
        [XmlIgnoreAttribute()]
        public bool parametersSpecified;
        [XmlElementAttribute()]
        #endregion Members
        #region Accessors
        private bool ExistKeyword
        {
            get
            {
                bool ret = (connectionStringSpecified && connectionString.Contains("%%"))
                    || (commandTextSpecified && commandText.Contains("%%"));

                if ((!ret) && parametersSpecified && parameters.parameterSpecified)
                {
                    foreach (Parameter p in parameters.parameter)
                    {
                        if (p.Value.Contains("%%"))
                        {
                            ret = true;
                            break;
                        }
                    }
                }
                return ret;
            }
        }
        public override bool IsValid
        {
            get { return commandTypeSpecified && commandTextSpecified; }
        }
        #endregion Accessors
        #region Constructors
        public AckSQLCommandSchedule() { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Génération de l'entrée dans une table SQL faisant office d'accusé de traitement.
        /// <para>Warning: Valoriser auparavant s'il y a lieu les accesseurs public (ex. ExtlRequestId)</para>
        /// </summary>
        /// <param name="processAck"></param>
        /// <returns></returns>
        /// FI 20230130 [26512] Modify
        public override void Generate(IProcessAcknowledgment processAck)
        {
            ackErrLevelSpecified = true;
            ackErrLevel = Cst.ErrLevel.SUCCESS;

            if (!connectionStringSpecified)
                connectionString = processAck.CS;

            if (false == System.Enum.TryParse<CommandType>(commandType, out CommandType commandTypeEnum))
                throw new InvalidOperationException($"commandType:{commandType} is not recognized");
            
            DataParameters pa = new DataParameters(GetDataParameters(commandTypeEnum));
            QueryParameters qryParameters;

            switch (commandTypeEnum)
            {
                case CommandType.Text:
                    qryParameters = new QueryParameters(connectionString, commandText, pa);
                    DataHelper.ExecuteNonQuery(connectionString, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    break;
                case CommandType.StoredProcedure:
                    int retSP = DataHelper.ExecuteNonQuery(connectionString, commandTypeEnum, commandText, pa.GetArrayDbParameter());
                    //Warning: SqlServer retourne -1 mais Oracle retourne 1 
                    ackErrLevel = (retSP != 0) ? Cst.ErrLevel.SUCCESS : Cst.ErrLevel.SQL_ERROR;
                    break;
                case CommandType.TableDirect:
                    //insert into "commandText" value (P1, P2, ...)
                    string query = SQLCst.INSERT_INTO_DBO + commandText + SQLCst.VALUES + "(";
                    if (parametersSpecified && parameters.parameterSpecified)
                    {
                        foreach (Parameter p in parameters.parameter)
                        {
                            string param = (p.nameSpecified ? p.name : p.id);
                            query += $"@{param},";
                        }
                    }
                    else
                    {
                        throw new NullReferenceException($"{nameof(parameters)} are not specified. Parameters needed for insert values on table:{commandText}");
                    }

                    query = query.Substring(0, query.Length - 1) + ")";
                    qryParameters = new QueryParameters(connectionString, query, pa);
                    DataHelper.ExecuteNonQuery(connectionString, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processAck"></param>
        protected override void ReplaceKeyword(IProcessAcknowledgment processAck)
        {
            if (ExistKeyword)
            {
                if (connectionStringSpecified)
                {
                    connectionString = ReplaceKeyword(connectionString, processAck);
                }

                if (commandTextSpecified)
                {
                    commandText = ReplaceKeyword(commandText, processAck);
                    commandText = ReplaceContents(commandText, processAck);
                }

                if (parametersSpecified && parameters.parameterSpecified)
                {
                    foreach (Parameter p in parameters.parameter)
                    {
                        p.Value = ReplaceKeyword(p.Value, processAck);
                        p.Value = ReplaceContents(p.Value, processAck);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCommandType"></param>
        /// <returns></returns>
        /// EG 20220908 [XXXXX][WI418] Suppression de la classe obsolète EFSParameter
        /// FI 20230130 [26512] Modify
        private DataParameter[] GetDataParameters(CommandType pCommandType)
        {
            DataParameter[] param = null;

            if (parametersSpecified && parameters.parameterSpecified)
            {
                param = new DataParameter[parameters.parameter.Length];
                int i = 0;
                foreach (Parameter p in parameters.parameter)
                {
                    //Warning: On ne gère que des paramètres "Input" de type "varchar(4000)
                    param[i] = new DataParameter(connectionString, pCommandType, (p.nameSpecified ? p.name : p.id), DbType.AnsiString, 4000)
                    {
                        Direction = ParameterDirection.Input,
                        Value = p.Value
                    };
                    i++;
                }
            }

            return param;
        }
        #endregion Methods
    }


    /// <summary>
    /// Accusé de traitement (Acknowledge) de type: MessageQueue (Postage d'un message dans une queue)
    /// </summary>
    [Serializable]
    public class AckMessageQueueSchedule : AckScheduleBase
    {
        #region Members
        [XmlElementAttribute()]
        public string platform;
        [XmlIgnoreAttribute()]
        public bool platformSpecified;
        [XmlElementAttribute()]
        public string queueName;
        [XmlIgnoreAttribute()]
        public bool queueNameSpecified;
        [XmlElementAttribute()]
        public string messageName;
        [XmlIgnoreAttribute()]
        public bool messageNameSpecified;
        [XmlIgnoreAttribute()]
        public bool messagePrioritySpecified;
        [XmlIgnoreAttribute()]
        public MessagePriority messagePriority;
        [XmlIgnoreAttribute()]
        public bool recoverableSpecified;
        [XmlIgnoreAttribute()]
        public Boolean recoverable;
        [XmlElementAttribute()]
        public string messageContents;
        [XmlIgnoreAttribute()]
        public bool messageContentsSpecified;
        
        #endregion Members
        #region Accessors
        private bool ExistKeyword
        {
            get
            {
                return (queueNameSpecified && queueName.Contains("%%"))
                || (messageNameSpecified && messageName.Contains("%%"))
                || (messageContentsSpecified && messageContents.Contains("%%"));
            }
        }
        public override bool IsValid
        {
            get { return platformSpecified && queueNameSpecified && messageNameSpecified; }
        }
        #endregion Accessors
        #region Constructors
        public AckMessageQueueSchedule() {
            messagePriority = MessagePriority.Normal;
        }
        #endregion Constructors
        #region Methods

        /// <summary>
        /// Génération du messageQueue faisant office d'accusé de traitement.
        /// </summary>
        /// <param name="processAck"></param>
        /// <returns></returns>
        public override void Generate(IProcessAcknowledgment processAck)
        {
            ackErrLevelSpecified = true;
            ackErrLevel = Cst.ErrLevel.SUCCESS;

            switch (platform)
            {
                case "MSMQ":
                    MSMQGenerate();
                    break;
                default:
                    throw new NotImplementedException($"platform: {platform} is not implemented");
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processAck"></param>
        protected override void ReplaceKeyword(IProcessAcknowledgment processAck)
        {
            if (ExistKeyword)
            {
                queueName = ReplaceKeyword(queueName, processAck);
                messageName = ReplaceKeyword(messageName, processAck);
                if (messageContentsSpecified)
                    messageContents = ReplaceContents(messageContents, processAck);
            }
        }

        /// <summary>
        /// Envoie de Message Queue de type MSMQ
        /// </summary>
        private void MSMQGenerate()
        {
            //generation de message
            using (Message msg = new Message(this.messageContentsSpecified ? this.messageContents : string.Empty))
            {
                msg.Label = messageName;

                if (messagePrioritySpecified)
                    msg.Priority = messagePriority;

                if (recoverableSpecified)
                    msg.Recoverable = recoverable;

                //Timeout: 0.1 --> Permet de n'effectuer qu'une seule tentative, car les tentatives ont lieu toutes les 0.5sec
                double timeout = 0.1;
                using (MessageQueue mq = MQueueTools.GetMsMQueue(queueName, timeout, out int attemps))
                {
                    mq.Send(msg);
                }
            }
        }
        #endregion Methods
    }



    /// <summary>
    /// Accusé de traitement (Acknowledge) (Ecrirure dans d'un message queue dans la queue Response)
    /// </summary>
    [Serializable]
    public class AckWebSessionSchedule : AckScheduleBase
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool responseSpecified;
        /// <summary>
        ///  Destination où le message ResponseRequestMQueue est envoyé
        /// </summary>
        [XmlElementAttribute()]
        public MOMSettings response;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public override bool IsValid
        {
            get { return responseSpecified; }
        }
        #endregion Accessors

        #region Constructors
        public AckWebSessionSchedule() { }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Génération du fichier/message faisant office d'accusé de traitement.
        /// </summary>
        /// <param name="processAck"></param>
        /// <returns></returns>
        public override void Generate(IProcessAcknowledgment processAck)
        {
            ackErrLevelSpecified = true;
            ackErrLevel = Cst.ErrLevel.SUCCESS;

            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = processAck.CS,
                id = (null != processAck.IdData) ? processAck.IdData.id : 0,
                identifier = (null != processAck.IdData) ? processAck.IdData.idIdentifier : string.Empty,
                idInfo = processAck.IdInfo,
                requester = new MQueueRequester(processAck.RequestId,
                                                processAck.Request.Session, processAck.Request.DtRequest)
            };

            ResponseRequestMQueue responseMQueue = new ResponseRequestMQueue(mQueueAttributes)
            {
                requestProcess = processAck.ProcessRequested,
                idTRK_L = processAck.RequestId,
                idProcess = processAck.IdLogProcess,
                readyState = processAck.ReadyState,
                nbMessage = processAck.Request.PostedMsg,
                nbNone = processAck.Request.NoneMsg,
                nbSucces = processAck.Request.SuccessMsg,
                nbWarning = processAck.Request.WarningMsg,
                nbError = processAck.Request.ErrorMsg


            };
            MQueueSendInfo sendInfo = new MQueueSendInfo
            {
                MOMSetting = this.response
            };
            if (sendInfo.IsInfoValid)
            {
                int nbAttemps = 0;
                MQueueTools.Send(responseMQueue, sendInfo, 60, ref nbAttemps);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processAck"></param>
        protected override void ReplaceKeyword(IProcessAcknowledgment processAck)
        {
            base.ReplaceKeyword(processAck);
        }
        #endregion Methods
    }
}

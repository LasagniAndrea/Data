#region using
using System;
using System.Collections.Generic;
using System.Globalization;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.EfsSend;
#endregion


namespace EFS.Common
{
    /// <summary>
    /// Représente le message envoyé par Spheres® après l'exécution d'une tâche quelconque
    /// <para>NB: Cette objet est à déplacer dans un projet common, pour utilisation en dehors de Spheres IO.</para>
    /// </summary>
    public class SMTPNotification
    {
        CultureInfo m_Culture;

        string m_SrvName;
        string m_DBName;
        DateTime m_StartDate;
        TimeSpan m_ElapsedTime;

        /// <summary>
        /// Les paramètres du serveur SMTP
        /// </summary>
        readonly SQL_SMTPServer m_SQL_SMTPServer;

        /// <summary>
        /// La liste d'éléments exécutés avant envoi de la notification
        /// </summary>
        List<Pair<string, Cst.IOReturnCodeEnum>> m_ExecutedElement;

        /// <summary>
        /// La liste des destinataires principaux
        /// </summary>
        List<EfsSendSMTPContact> m_SendTo;

        /// <summary>
        /// La liste des destinataires de copie carbone
        /// </summary>
        List<EfsSendSMTPContact> m_SendCc;

        /// <summary>
        /// La liste des destinataires de copie carbone invisibles
        /// </summary>
        List<EfsSendSMTPContact> m_SendBcc;

        EfsSendSMTP m_SendSmtp;

        /// <summary>
        /// 
        /// </summary>
        public EfsSendSMTP SendSmtp
        {
            get { return m_SendSmtp; }
        }

        /// <summary>
        /// La liste des éléments exécutés avant l'envoi de la notification
        /// </summary>
        public List<Pair<string, Cst.IOReturnCodeEnum>> ExecutedElement
        {
            get { return m_ExecutedElement; }
        }

        /// <summary>
        /// La liste des destinataires principaux
        /// </summary>
        public List<EfsSendSMTPContact> SendTo
        {
            get { return m_SendTo; }
        }

        /// <summary>
        /// La liste des destinataires de copie carbone
        /// </summary>
        public List<EfsSendSMTPContact> SendCc
        {
            get { return m_SendCc; }
        }

        /// <summary>
        /// La liste des destinataires de copie carbone invisibles
        /// </summary>
        public List<EfsSendSMTPContact> SendBcc
        {
            get { return m_SendBcc; }
        }

        /// <summary>
        /// Obtient la culture de notification de la tâche
        /// </summary>
        public CultureInfo Culture
        {
            get { return m_Culture; }
        }

        /// <summary>
        /// Affecte le temps ecoulé depuis l'initilisation de la Notification
        /// </summary>
        /// <returns></returns>
        public void SetElapsedTime()
        {
            SetElapsedTime(DateTime.Now);
        }

        /// <summary>
        /// Affecte le temps ecoulé entre l'initilisation de la Notification et la date {pEndDate}
        /// </summary>
        /// <returns></returns>
        public void SetElapsedTime(DateTime pEndDate)
        {
            m_ElapsedTime = pEndDate - m_StartDate;
        }

        /// <summary>
        /// Remplace une chaine d'entrée {pStringWithKeyWords} avec les keywords autorisés. 
        /// </summary>
        /// <param name="pStringWithKeyWords"></param>
        /// <returns></returns>
        public string ReplaceKeyWords(string pStringWithKeyWords)
        {
            if (pStringWithKeyWords.Contains("{SVRNAME}"))
                pStringWithKeyWords = pStringWithKeyWords.Replace("{SVRNAME}", m_SrvName);

            if (pStringWithKeyWords.Contains("{DBNAME}"))
                pStringWithKeyWords = pStringWithKeyWords.Replace("{DBNAME}", m_DBName);

            if (pStringWithKeyWords.Contains("{STARTDATE}"))
                pStringWithKeyWords = pStringWithKeyWords.Replace("{STARTDATE}", DtFunc.DateTimeToString(m_StartDate, DtFunc.FmtShortDate, m_Culture));

            if (pStringWithKeyWords.Contains("{STARTTIME}"))
                pStringWithKeyWords = pStringWithKeyWords.Replace("{STARTTIME}", DtFunc.DateTimeToString(m_StartDate, DtFunc.FmtLongTime, m_Culture));

            if (pStringWithKeyWords.Contains("{ELAPSEDHOUR}"))
                pStringWithKeyWords = pStringWithKeyWords.Replace("{ELAPSEDHOUR}", m_ElapsedTime.Hours.ToString());

            if (pStringWithKeyWords.Contains("{ELAPSEDMINITE}"))
                pStringWithKeyWords = pStringWithKeyWords.Replace("{ELAPSEDMINITE}", m_ElapsedTime.Minutes.ToString());

            if (pStringWithKeyWords.Contains("{ELAPSEDSECOND}"))
                pStringWithKeyWords = pStringWithKeyWords.Replace("{ELAPSEDSECOND}", m_ElapsedTime.Seconds.ToString());

            if (pStringWithKeyWords.Contains("{EXECUTEDELEMENT}") || pStringWithKeyWords.Contains("{EXECUTEDELEMENTWITHSTATUS}"))
            {
                string executedElement = string.Empty;
                foreach (Pair<string, Cst.IOReturnCodeEnum> element in m_ExecutedElement)
                {
                    executedElement += "                - " + element.First;
                    if (pStringWithKeyWords.Contains("{EXECUTEDELEMENTWITHSTATUS}"))
                        executedElement += " (" + element.Second + ")";

                    executedElement += Cst.CrLf;
                }
                //
                if (pStringWithKeyWords.Contains("{EXECUTEDELEMENTWITHSTATUS}"))
                    pStringWithKeyWords = pStringWithKeyWords.Replace("{EXECUTEDELEMENTWITHSTATUS}", executedElement);
                else
                    pStringWithKeyWords = pStringWithKeyWords.Replace("{EXECUTEDELEMENT}", executedElement);
            }

            return pStringWithKeyWords;
        }

        #region Constructors
        /// <summary>
        /// Initialisation des informations nécéssaires à l'envoi de la notification
        /// <para>Chargement du référentiel SMTPSERVER</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIDSMTPServer"></param>
        public SMTPNotification(string pCs, string pSMTPServer_Identifier)
            : this(pCs, DateTime.Now, pSMTPServer_Identifier, CultureInfo.CreateSpecificCulture("en-GB")) { }

        /// <summary>
        /// Initialisation des informations nécéssaires à l'envoi de la notification
        /// <para>Chargement du référentiel SMTPSERVER</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIDSMTPServer"></param>
        public SMTPNotification(string pCs, int pIDSMTPServer)
            : this(pCs, DateTime.Now, pIDSMTPServer, CultureInfo.CreateSpecificCulture("en-GB")) { }

        /// <summary>
        /// Initialisation des informations nécéssaires à l'envoi de la notification
        /// <para>Chargement du référentiel SMTPSERVER</para>
        /// <para>Initialisation de la date de début</para>
        /// <para>NB: Ce constructeur effectu tous les chargements de données à partir de la DB, et doit être appelé au début du traitement à notifier.</para>
        /// <para>Ainsi, si le traitement est arrété à cause d'un accès à la base de données (Timeout, ...), la notification sera tout de même envoyée</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pStartDate"></param>
        /// <param name="pIDSMTPServer"></param>
        /// <param name="pCulture"></param>
        public SMTPNotification(string pCs, DateTime pStartDate, string pSMTPServer_Identifier, CultureInfo pCulture)
        {
            Initialize(pCs, pStartDate, pCulture);

            if (pSMTPServer_Identifier.Length > 0)
            {
                m_SQL_SMTPServer = new SQL_SMTPServer(pCs, SQL_TableWithID.IDType.Identifier, pSMTPServer_Identifier, SQL_Table.ScanDataDtEnabledEnum.Yes);
                m_SQL_SMTPServer.LoadTable();
            }
        }
        /// <summary>
        /// Initialisation des informations nécéssaires à l'envoi de la notification
        /// <para>Chargement du référentiel SMTPSERVER</para>
        /// <para>Initialisation de la date de début</para>
        /// <para>NB: Ce constructeur effectu tous les chargements de données à partir de la DB, et doit être appelé au début du traitement à notifier.</para>
        /// <para>Ainsi, si le traitement est arrété à cause d'un accès à la base de données (Timeout, ...), la notification sera tout de même envoyée</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pStartDate"></param>
        /// <param name="pIDSMTPServer"></param>
        /// <param name="pCulture"></param>
        public SMTPNotification(string pCs, DateTime pStartDate, int pIDSMTPServer, CultureInfo pCulture)
        {
            Initialize(pCs, pStartDate, pCulture);

            if (pIDSMTPServer > 0)
            {
                m_SQL_SMTPServer = new SQL_SMTPServer(pCs, pIDSMTPServer);
                m_SQL_SMTPServer.LoadTable();
            }
        }
        private void Initialize(string pCs, DateTime pStartDate, CultureInfo pCulture)
        {
            m_Culture = pCulture;
            m_StartDate = pStartDate;

            CSManager csManager = new CSManager(pCs);
            m_SrvName = csManager.GetSvrName();
            m_DBName = csManager.GetDbName();

            m_SendTo = new List<EfsSendSMTPContact>();
            m_SendCc = new List<EfsSendSMTPContact>();
            m_SendBcc = new List<EfsSendSMTPContact>();

            m_ExecutedElement = new List<Pair<string, Cst.IOReturnCodeEnum>>();
        }
        #endregion Constructors

        /// <summary>
        /// Instancié un objet EfsSendSMTP et envoi le message
        /// </summary>
        /// <param name="pFrom"></param>
        /// <param name="pSubject"></param>
        /// <param name="pBody"></param>
        /// <param name="pPriority"></param>
        public Cst.ErrLevel Send(string pFrom, string pSubject, string pBody, EfsSendSMTPPriorityEnum pPriority)
        {
            if (m_ElapsedTime == TimeSpan.Zero)
                SetElapsedTime();

            // Objet d'envoi de message
            m_SendSmtp = new EfsSendSMTP(m_SQL_SMTPServer.Host_Primary);
            
            // Initialisation du serveur SMTP
            if (IntFunc.IsFilledAndNoZero(m_SQL_SMTPServer.Port_Primary))
                m_SendSmtp.SmtpClient.Port = m_SQL_SMTPServer.Port_Primary;

            /// FI 20211119 [XXXX] Alimentation de EnableSsl
            m_SendSmtp.SmtpClient.EnableSsl = m_SQL_SMTPServer.IsSSL_Primary;

            if (StrFunc.IsFilled(m_SQL_SMTPServer.UserID_Primary))
                m_SendSmtp.SmtpServerUser = m_SQL_SMTPServer.UserID_Primary;
            if (StrFunc.IsFilled(m_SQL_SMTPServer.PWD_Primary))
                m_SendSmtp.SmtpServerUserPwd = m_SQL_SMTPServer.PWD_Primary;

            // Auteur
            if (StrFunc.IsEmpty(pFrom))
                pFrom = m_SQL_SMTPServer.OriginAdress;
            if (StrFunc.IsEmpty(pFrom))
                pFrom = "no-reply@" + m_SQL_SMTPServer.Identifier + ".net";

            m_SendSmtp.MailMessage.From = new EfsSendSMTPContact(pFrom);

            // Destinataires principaux
            m_SendSmtp.MailMessage.To = m_SendTo.ToArray();

            // Autres destinataires
            m_SendSmtp.MailMessage.Cc = m_SendCc.ToArray();

            // Autres destinataires cachés
            m_SendSmtp.MailMessage.Bcc = m_SendBcc.ToArray();

            // Sujet
            m_SendSmtp.MailMessage.Subject = new EfsSendSubject
            {
                Value = ReplaceKeyWords(pSubject)
            };

            // Corps du message
            m_SendSmtp.MailMessage.Body = new EfsSendBody
            {
                Value = ReplaceKeyWords(pBody),
                format = EfsSendFormatEnum.Text
            };

            m_SendSmtp.MailMessage.DeliveryNotificationOptions = EfsSendSMTPDeliveryNotificationEnum.None;
            m_SendSmtp.MailMessage.Priority = pPriority;

            // Envoi de la notification en utilisant EfsSendSMTP
            return m_SendSmtp.SendEmail();
        }

        /// <summary>
        /// Instancié un objet EfsSendSMTP et envoi le message
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIDSMTPServer"></param>
        /// <param name="pSendTo"></param>
        /// <param name="pSubject"></param>
        public static void Send(string pCs, int pIDSMTPServer, List<EfsSendSMTPContact> pSendTo, string pSubject)
        {
            Send(pCs, pIDSMTPServer, pSendTo, null, null, pSubject, string.Empty, EfsSendSMTPPriorityEnum.Normal);
        }


        public static void Send(string pCs, int pIDSMTPServer, string pSendTo, string pSubject)
        {
            Send(pCs, pIDSMTPServer, new List<EfsSendSMTPContact>() { new EfsSendSMTPContact(pSendTo, pSendTo) }, null, null, pSubject, string.Empty, EfsSendSMTPPriorityEnum.Normal);
        }


        /// <summary>
        /// Instancie un objet EfsSendSMTP et envoi le message
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIDSMTPServer"></param>
        /// <param name="pSendTo"></param>
        /// <param name="pSendCc"></param>
        /// <param name="pSendBcc"></param>
        /// <param name="pSubject"></param>
        /// <param name="pBody"></param>
        /// <param name="pPriority"></param>
        public static Cst.ErrLevel Send(string pCs, int pIDSMTPServer,
            List<EfsSendSMTPContact> pSendTo, List<EfsSendSMTPContact> pSendCc, List<EfsSendSMTPContact> pSendBcc,
            string pSubject, string pBody, EfsSendSMTPPriorityEnum pPriority)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.INCORRECTPARAMETER;
            
            if (pIDSMTPServer != 0)
            {
                // Initialisation des informations nécéssaires à l'envoi d'une notification
                SMTPNotification notification = new SMTPNotification(pCs, pIDSMTPServer);

                if (pSendTo != null)
                    notification.SendTo.AddRange(pSendTo);
                if (pSendCc != null)
                    notification.SendCc.AddRange(pSendCc);
                if (pSendBcc != null)
                    notification.SendBcc.AddRange(pSendBcc);

                // Envoyer la notification en utilisant EfsSendSMTP
                ret = notification.Send(string.Empty, pSubject, pBody, pPriority);
            }

            return ret;
        }
    }
}


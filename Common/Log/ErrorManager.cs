#region Using Directives
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Text;
using System.Web;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Common.EfsSend;
#endregion Using Directives

namespace EFS.Common.Log
{

    /// <summary>
    /// 
    /// </summary>
    public class WebErrorWriter
    {
        #region Method
        /// <summary>
        /// Writes Error Info to successfully initialized output devices (for web Application only) 
        /// </summary>
        /// <param name="pErrorBlock"></param>
        public static void Write(ErrorBlock pErrorBlock)
        {
            if (null == pErrorBlock)
                throw new ArgumentNullException("pErrorBlock", "pErrorBlock is null");

            HttpContext.Current.Application.Lock();
            ErrorManager errorManager = (ErrorManager)System.Web.HttpContext.Current.Application["LOG"];
            errorManager.Write(pErrorBlock);
            HttpContext.Current.Application.UnLock();

        }
        #endregion Methods
    }

    /// <summary>
    /// Class used to send error information to specified outputs
    /// </summary>
    /// FI 20210111 [XXXXX] ErrorManager est IDisposable
    public class ErrorManager : IDisposable
    {
        #region Enums



        [Flags]
        public enum OutputDevicesEnum
        {
            None = 0,
            Database = 0x0001,
            XMLLogFile = 0x0002,
            EventLog = 0x0004,
            HTMLText = 0x0008,
            LogFile = 0x0010,
            Email = 0x0020,
            All = Database | XMLLogFile | EventLog | HTMLText | LogFile | Email
        };
        #endregion

        #region Members
        // General
        private OutputDevicesEnum _OutputDevicesEnum = OutputDevicesEnum.None;

        // Database
        //private ErrorDBLog _dbLog;
        private ErrorDBLog2 _dbLog;

        // HTML
        private string _HTMLText;

        // XML
        private string _XMLFilename;
        private FileStream _XMLLogWriter;

        // Ordinary Log File
        private string _LogFilename;
        private StreamWriter _LogWriter;

        // Event Log
        private string _EventLogSourceName;
        private string _EventLogName;

        // Email
        private string _SMTPServer;

        private bool disposedValue;
        #endregion

        #region Public Properties
        public string ErrDescriptionHTML
        {
            get { return _HTMLText; }
        }
        #endregion

        #region Constructor/Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        ///<remarks>
        ///Initializes variables
        ///</remarks>
        ///<return>
        ///Nothing
        ///</return>
        public ErrorManager()
        {
            InitHTMLOutput();
            _XMLFilename = string.Empty;
            _OutputDevicesEnum = OutputDevicesEnum.None;
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Constructs a date string to be attached to ordinary log file and XML log file.
        /// </summary>
        /// <param name="pLogFileName">Original log file name</param>
        /// <returns>new log file name or null if error</returns>
        public static string BuildLogFileName(string pLogFileName)
        {

            int indexOfPeriod = pLogFileName.LastIndexOf('.');
            if (-1 != indexOfPeriod)
            {
                DateTime dtNow = SystemTools.GetOSDateSys();
                string tmpFileName = pLogFileName.Substring(0, indexOfPeriod);
                string extension = pLogFileName.Substring(indexOfPeriod);
                return String.Format("{0}_{1}{2}", tmpFileName, dtNow.ToString("yyyy-MM-dd"), extension);
            }
            else
            {
                throw new ArgumentException(StrFunc.AppendFormat("Invalid FileName : {0}", pLogFileName));
            }

        }


        //private static void OutputToEmergencyDevice()
        //{
        //    // A faire : Write to log file or something
        //}
        #endregion

        #region Public Functions
        ///<summary>
        ///Initialize E-mail Output
        ///</summary>
        ///<return>
        ///Returns nothing.
        ///</return>
        ///<param name="pSmtpServer">String value of SMTP server to be used to send e-mail</param>
        public void InitEmail(string pSmtpServer)
        {
            _SMTPServer = pSmtpServer;
            //("smtp.provider.com" => valeur présente dans le web.config par défaut et qui n'a pas de sens)
            if (StrFunc.IsFilled(_SMTPServer) && (_SMTPServer != "smtp.provider.com"))
                _OutputDevicesEnum |= ErrorManager.OutputDevicesEnum.Email;
        }

        ///<summary>
        ///Initialize Event Log.
        ///</summary>
        ///<return>
        ///Returns Nothing.
        ///</return>
        public void InitEventLog(string pEventLogName, string pEventLogSourceName)
        {
            try
            {
                if (!(EventLog.SourceExists(pEventLogSourceName)))
                {
                    EventLog.CreateEventSource(pEventLogSourceName, pEventLogName);
                }

                if (EventLog.SourceExists(pEventLogSourceName))
                {
                    // Event Log exists... Validated
                    _OutputDevicesEnum |= ErrorManager.OutputDevicesEnum.EventLog;
                    _EventLogName = pEventLogName;
                    _EventLogSourceName = pEventLogSourceName;
                }
            }
            catch (Exception ex)
            {
                AppInstance.TraceManager.TraceError(this, $"Error on initialize Windows® Event Log: {Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(ex)}");
            }
        }

        ///<summary>
        ///Initialize XML Log File.
        ///</summary>
        ///<param name="pXMLLogFilename">Complete path of XML log file.</param>
        // EG 20180423 Analyse du code Correction [CA2202]
        public void InitXMLLogFile(string pXMLLogFilename)
        {
            _XMLFilename = BuildLogFileName(pXMLLogFilename);

            bool bXMLLogFileExists = File.Exists(_XMLFilename);
            if (false == bXMLLogFileExists)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(_XMLFilename, false))
                    {
                        writer.Write(String.Format("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>{0}<exceptions>{1}</exceptions>{2}", Environment.NewLine, Environment.NewLine, Environment.NewLine));
                        writer.Flush();
                    }
                }
                catch (Exception ex)
                {
                    AppInstance.TraceManager.TraceError(this, $"Error on initialize XML Log: {Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(ex)}");
                }
            }

            try
            {
                _XMLLogWriter = File.Open(_XMLFilename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                _OutputDevicesEnum |= ErrorManager.OutputDevicesEnum.XMLLogFile;
            }
            catch (Exception ex)
            {
                AppInstance.TraceManager.TraceError(this, $"Error on initialize XML Log: {Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(ex)}");
            }
        }


        /// <summary>
        /// Initialize Ordinary Log File.
        /// </summary>
        /// <param name="pLogFilename">Complete path of log file</param>
        public void InitLogFile(string pLogFilename)
        {
            _LogFilename = BuildLogFileName(pLogFilename);

            try
            {
                bool isLogFileExists = File.Exists(_LogFilename);
                if (true != isLogFileExists)
                {
                    File.Create(_LogFilename);
                }
            }
            catch (Exception ex)
            {
                AppInstance.TraceManager.TraceError(this, $"Error on initialize File Log: {Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(ex)}");
            }

            try
            {
                _LogWriter = File.AppendText(_LogFilename); //, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite) ;
                _OutputDevicesEnum |= ErrorManager.OutputDevicesEnum.LogFile;
            }
            catch (Exception ex)
            {
                AppInstance.TraceManager.TraceError(this, $"Error on initialize File Log: {Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(ex)}");
            }
        }

        ///<summary>
        ///Initialize Database Connection.
        ///</summary>
        ///<param name="pConnectionString"></param>
        public void InitDatabase2(string pConnectionString)
        {
            try
            {
                _dbLog = new ErrorDBLog2(pConnectionString);
                _OutputDevicesEnum |= ErrorManager.OutputDevicesEnum.Database;
            }
            catch (Exception ex)
            {
                AppInstance.TraceManager.TraceError(this, $"Error on initialize Database Log: {Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(ex)}");
            }
        }

        ///<summary>
        ///Initialize HTML Variable
        ///</summary>
        ///<return>
        ///Returns nothing.
        ///</return>
        public void InitHTMLOutput()
        {
            _HTMLText = string.Empty;
            _OutputDevicesEnum |= ErrorManager.OutputDevicesEnum.HTMLText;
        }



        ///<summary>
        ///Writes Error Info to successfully initialzed output devices
        ///</summary>
        ///<remarks>
        ///Overloaded function with public void Write( ErrorBlock oErrorBlock )
        ///Should be called when specifying new output devices
        ///</remarks>
        ///<return>
        ///Returns nothing.
        ///</return>
        ///<param name=lOutputDevices>
        ///Flags specifying output devices for error information
        ///</param>
        ///<param name=oErrorBlock>
        ///Object containing all pertinant information to send to output devices
        ///</param>
        public void Write(ErrorBlock pErrorBlock)
        {
            ErrorFormatter ef = new ErrorFormatter(pErrorBlock);

            if (ErrorManager.OutputDevicesEnum.EventLog == (_OutputDevicesEnum & ErrorManager.OutputDevicesEnum.EventLog))
                WriteEventLog(ef, pErrorBlock);

            if (ErrorManager.OutputDevicesEnum.XMLLogFile == (_OutputDevicesEnum & ErrorManager.OutputDevicesEnum.XMLLogFile))
                WriteXMLLog(ef);

            if (ErrorManager.OutputDevicesEnum.HTMLText == (_OutputDevicesEnum & ErrorManager.OutputDevicesEnum.HTMLText))
                WriteHTMLLog(ef);

           if (ErrorManager.OutputDevicesEnum.LogFile == (_OutputDevicesEnum & ErrorManager.OutputDevicesEnum.LogFile))
                WriteTXTLog(ef);

            if (ErrorManager.OutputDevicesEnum.Database == (_OutputDevicesEnum & ErrorManager.OutputDevicesEnum.Database))
                WriteRDBMSLog(pErrorBlock);

            if (ErrorManager.OutputDevicesEnum.Email == (_OutputDevicesEnum & ErrorManager.OutputDevicesEnum.Email))
                SendEmailLog(ef);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: supprimer l'état managé (objets managés)

                    if (_LogWriter != null)
                    {
                        _LogWriter.Close();
                        _LogWriter.Dispose();
                    }
                    if (_XMLLogWriter != null)
                    {
                        _XMLLogWriter.Close();
                        _XMLLogWriter.Dispose();
                    }
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~ErrorManager()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pErrorFormatter"></param>
        /// <param name="pErrorBlock"></param>
        /// FI 20210111 [XXXXX] Add
        private void WriteEventLog(ErrorFormatter pErrorFormatter, ErrorBlock pErrorBlock)
        {
            EventLogEntryType logEntryType;
            switch (pErrorBlock.Severity)
            {
                case ErrorLogSeverityEnum.HIGH:
                case ErrorLogSeverityEnum.VERY_HIGH:
                    logEntryType = EventLogEntryType.Error;
                    break;
                case ErrorLogSeverityEnum.MEDIUM:
                    logEntryType = EventLogEntryType.Warning;
                    break;
                case ErrorLogSeverityEnum.VERY_LOW:
                case ErrorLogSeverityEnum.LOW:
                default:
                    logEntryType = EventLogEntryType.Information;
                    break;
            }

            using (EventLog elog = new EventLog(_EventLogName))
            {
                elog.Source = _EventLogSourceName;
                try
                {
                    elog.WriteEntry(pErrorFormatter.EventLogData, logEntryType);
                }
                catch (Exception ex)
                {
                    AppInstance.TraceManager.TraceError(this, $"Error on writing Windows® Event Log: {Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(ex)}");
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pErrorFormatter"></param>
        /// <param name="pErrorBlock"></param>
        /// FI 20210111 [XXXXX] Add
        private void WriteXMLLog(ErrorFormatter pErrorFormatter)
        {
            _XMLLogWriter.Seek(-("</exceptions>".Length + (1 * Environment.NewLine.Length)), SeekOrigin.End);
            ASCIIEncoding encoding = new ASCIIEncoding();

            try
            {
                _XMLLogWriter.Write(encoding.GetBytes(pErrorFormatter.XMLData), 0, pErrorFormatter.XMLData.Length);
                _XMLLogWriter.Flush();
            }
            catch (Exception ex)
            {
                AppInstance.TraceManager.TraceError(this, $"Error on writing XML Log: {Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(ex)}");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pErrorFormatter"></param>
        /// FI 20210111 [XXXXX] Add
        private void WriteHTMLLog(ErrorFormatter pErrorFormatter)
        {
            _HTMLText = pErrorFormatter.HTMLData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pErrorFormatter"></param>
        /// FI 20210111 [XXXXX] Add
        private void WriteTXTLog(ErrorFormatter pErrorFormatter)
        {
            try
            {
                _LogWriter.WriteLine(pErrorFormatter.LogFileData);
                _LogWriter.Flush();
            }
            catch (Exception ex)
            {
                AppInstance.TraceManager.TraceError(this, $"Error on writing TXT Log: {Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(ex)}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pErrorFormatter"></param>
        /// <param name="pErrorBlock"></param>
        /// FI 20210111 [XXXXX] Add
        private void WriteRDBMSLog(ErrorBlock pErrorBlock)
        {
            try
            {
                _dbLog.Write(pErrorBlock);
            }
            catch (Exception ex)
            {
                AppInstance.TraceManager.TraceError(this, $"Error on writing RBBMS Log: {Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(ex)}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pErrorFormatter"></param>
        /// FI 20210111 [XXXXX] Add
        /// FI 20211119 [XXXXX] Gestion de EnableSsl 
        private void SendEmailLog(ErrorFormatter pErrorFormatter)
        {
            try
            {

                string smtpUser = ConfigurationManager.AppSettings["Spheres_ErrorEmailSmtpUserName"];

                string pwd = ConfigurationManager.AppSettings["Spheres_ErrorEmailSmtpUserNamePwd"];
                bool isPwdCrypted = Convert.ToBoolean(ConfigurationManager.AppSettings["Spheres_ErrorEmailSmtpIsPwdCrypted"]);
                if (isPwdCrypted)
                    pwd = Cryptography.Decrypt(pwd);
                
                string strEnabledssl = ConfigurationManager.AppSettings["Spheres_ErrorEmailSmtpEnableSsl"];
                Nullable<Boolean> enabledssl = null;
                if (StrFunc.IsFilled(strEnabledssl))
                    enabledssl = Convert.ToBoolean(strEnabledssl);
                
                string strPort = ConfigurationManager.AppSettings["Spheres_ErrorEmailSmtpPort"];
                int port = (StrFunc.IsFilled(strPort) ? Convert.ToInt32(strPort) : (enabledssl.HasValue && enabledssl == true) ? 587: 25);

                EfsSendSMTP sendSmtp = new EfsSendSMTP(_SMTPServer, port, smtpUser, pwd);
                if (enabledssl.HasValue)
                    sendSmtp.SmtpClient.EnableSsl = enabledssl.Value;

                sendSmtp.MailMessage.From = new EfsSendSMTPContact(ConfigurationManager.AppSettings["Spheres_ErrorEmailSender"]);
                sendSmtp.MailMessage.To = new EfsSendSMTPContact[1] { new EfsSendSMTPContact(ConfigurationManager.AppSettings["Spheres_ErrorEmailReceiver"]) };
                sendSmtp.MailMessage.Priority = EfsSendSMTPPriorityEnum.High;
                sendSmtp.MailMessage.Subject.Value = ConfigurationManager.AppSettings["Spheres_ErrorEmailSubject"];
                sendSmtp.MailMessage.Body.Value = pErrorFormatter.EmailData;

                sendSmtp.SendEmail();
            }
            catch (Exception ex)
            {
                AppInstance.TraceManager.TraceError(this, $"Error on writing Email Log: {Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(ex)}");
            }

        }
        #endregion
    }
}

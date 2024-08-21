using EFS.ACommon;
using System;
using System.Text;

namespace EFS.Common.Log
{
    /// <summary>
    /// Classe ErrorFormatter used to format text to be sent to various output devices
    /// </summary>
    /// PM 20200601 [XXXXX] Classe créé à partir de la classe ErrorFormatter (Common/Log/ErrorFormatter.cs) qui maintenant hérite de ErrorFileFormatter
    public class ErrorLogFormatter
    {
        #region Members
        protected ErrorLogInfo m_ErrorLogInfo;
        private string m_XmlData = default;
        private string m_HtmlData = default;
        private string m_LogFileData = default;
        private string m_EventLogData = default;
        private string m_EmailData = default;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Log pour sortie XML
        /// </summary>
        public string XMLData
        {
            get
            {
                if (m_XmlData == default)
                {
                    FormatXML();
                }
                return m_XmlData;
            }
        }

        /// <summary>
        /// Log pour sortie fichier
        /// </summary>
        public string LogFileData
        {
            get
            {
                if (m_LogFileData == default)
                {
                    FormatLogFile();
                }
                return m_LogFileData;
            }
        }

        /// <summary>
        /// Log pour sortie Html
        /// </summary>
        public string HTMLData
        {
            get
            {
                if (m_HtmlData == default)
                {
                    FormatHTML();
                }
                return m_HtmlData;
            }
        }

        /// <summary>
        /// Log pour sortie Journal des événements
        /// </summary>
        public string EventLogData
        {
            get
            {
                if (m_EventLogData == default)
                {
                    FormatEventLog();
                }
                return m_EventLogData;
            }
        }

        /// <summary>
        /// Log pour sortie Email
        /// </summary>
        public string EmailData
        {
            get
            {
                if (m_EmailData == default)
                {
                    FormatEmail();
                }
                return m_EmailData;
            }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pErrorLogInfo"></param>
        public ErrorLogFormatter(ErrorLogInfo pErrorLogInfo)
        {
            m_ErrorLogInfo = pErrorLogInfo;
        }
        /// <summary>
        /// Constructeur
        /// </summary>
        protected ErrorLogFormatter() { }
        #endregion Constructors

        #region Private formatting functions
        #region eMail
        private void FormatEmail()
        {
            string mailData = string.Empty;
            if (m_ErrorLogInfo != default(ErrorLogInfo))
            {
                mailData += String.Format("Date:            {0}{1}", m_ErrorLogInfo.DtError.ToString("yyyy-MM-dd"), Environment.NewLine);
                mailData += String.Format("Time:            {0}{1}", m_ErrorLogInfo.DtError.ToString("T"), Environment.NewLine);
                mailData += String.Format("Severity:        {0}{1}", m_ErrorLogInfo.Severity.ToString(), Environment.NewLine);
                mailData += String.Format("Method:          {0}{1}", m_ErrorLogInfo.Method, Environment.NewLine);
                mailData += String.Format("Source:          {0}{1}", m_ErrorLogInfo.Source, Environment.NewLine);
                mailData += String.Format("Messsage:        {0}{1}", m_ErrorLogInfo.Message, Environment.NewLine);
                mailData += String.Format("AppName:         {0}{1}", m_ErrorLogInfo.AppName, Environment.NewLine);
                mailData += String.Format("AppVersion:      {0}{1}", m_ErrorLogInfo.AppVersion, Environment.NewLine);
                mailData += String.Format("Hostname:        {0}{1}", m_ErrorLogInfo.HostName, Environment.NewLine);
                mailData += String.Format("Username:        {0}{1}", m_ErrorLogInfo.IdA_Identifier, Environment.NewLine);
                mailData += String.Format("URL:             {0}{1}", m_ErrorLogInfo.URL, Environment.NewLine);
                mailData += String.Format("BrowserInfo:     {0}{1}", m_ErrorLogInfo.BrowserInfo, Environment.NewLine);
            }
            m_EmailData = mailData;
        }
        #endregion eMail
        #region XML
        private string UnspecifiedIfEmpty(string pData)
        {
            return StrFunc.IsFilled(pData) ? pData : "Unspecified";
        }
        private void FormatXML()
        {
            StringBuilder xmlData = new StringBuilder();
            if (m_ErrorLogInfo != default(ErrorLogInfo))
            {
                xmlData.Append(String.Format("  <exception>{0}", Environment.NewLine));
                xmlData.Append(String.Format("    <date>{0}</date>{1}", m_ErrorLogInfo.DtError.ToString("yyyy-MM-dd"), Environment.NewLine));
                xmlData.Append(String.Format("    <time>{0}</time>{1}", m_ErrorLogInfo.DtError.ToString("T"), Environment.NewLine));

                xmlData.Append(String.Format("    <severity>{0}</severity>{1}", UnspecifiedIfEmpty(m_ErrorLogInfo.Severity.ToString()), Environment.NewLine));
                //
                xmlData.Append(String.Format("    <method>{0}</method>{1}", UnspecifiedIfEmpty(m_ErrorLogInfo.Method), Environment.NewLine));
                //
                xmlData.Append(String.Format("    <source>{0}</source>{1}", UnspecifiedIfEmpty(m_ErrorLogInfo.Source), Environment.NewLine));
                //
                xmlData.Append(String.Format("    <message>{0}</message>{1}", UnspecifiedIfEmpty(m_ErrorLogInfo.Message), Environment.NewLine));
                //
                xmlData.Append(String.Format("    <appName>{0}</appName>{1}", UnspecifiedIfEmpty(m_ErrorLogInfo.AppName), Environment.NewLine));
                //
                xmlData.Append(String.Format("    <appVersion>{0}</appVersion>{1}", UnspecifiedIfEmpty(m_ErrorLogInfo.AppVersion), Environment.NewLine));
                //
                xmlData.Append(String.Format("    <hostname>{0}</hostname>{1}", UnspecifiedIfEmpty(m_ErrorLogInfo.HostName), Environment.NewLine));
                //
                xmlData.Append(String.Format("    <username>{0}</username>{1}", UnspecifiedIfEmpty(m_ErrorLogInfo.IdA_Identifier), Environment.NewLine));
                //
                xmlData.Append(String.Format("    <browser>{0}</browser>{1}", UnspecifiedIfEmpty(m_ErrorLogInfo.BrowserInfo), Environment.NewLine));
                //
                xmlData.Append(String.Format("    <url>{0}</url>{1}", UnspecifiedIfEmpty(m_ErrorLogInfo.URL), Environment.NewLine));
                //
                xmlData.Append(String.Format("  </exception>{0}", Environment.NewLine));
                xmlData.Append(String.Format("</exceptions>{0}", Environment.NewLine));
            }
            m_XmlData = xmlData.ToString();
        }
        #endregion XML
        #region HTML
        private void FormatHTML()
        {
            string htmlData = "<table border=\"1\" width=\"80%\"><tr><td cspan=\"3\">";
            htmlData += "</td></tr></table>";
            m_HtmlData = htmlData;
        }
        #endregion HTML
        #region EventLog
        private void FormatEventLog()
        {
            string eventLogData = string.Empty;
            if (m_ErrorLogInfo != default(ErrorLogInfo))
            {
                if (StrFunc.IsFilled(m_ErrorLogInfo.Severity.ToString()))
                {
                    eventLogData += String.Format("{0}Severity: {1}", Environment.NewLine, m_ErrorLogInfo.Severity.ToString());
                }
                if (StrFunc.IsFilled(m_ErrorLogInfo.Method))
                {
                    eventLogData += String.Format("{0}Method: {1}", Environment.NewLine, m_ErrorLogInfo.Method.ToString());
                }
                if (StrFunc.IsFilled(m_ErrorLogInfo.Source))
                {
                    eventLogData += String.Format("{0}Source: {1}", Environment.NewLine, m_ErrorLogInfo.Source);
                }
                if (StrFunc.IsFilled(m_ErrorLogInfo.Message))
                {
                    eventLogData += String.Format("{0}Message: {1}", Environment.NewLine, m_ErrorLogInfo.Message);
                }
                if (StrFunc.IsFilled(m_ErrorLogInfo.AppName))
                {
                    eventLogData += String.Format("{0}App: {1} - {2}", Environment.NewLine, m_ErrorLogInfo.AppName, m_ErrorLogInfo.AppVersion);
                }
                if (StrFunc.IsFilled(m_ErrorLogInfo.HostName))
                {
                    eventLogData += String.Format("{0}Hostname: {1}", Environment.NewLine, m_ErrorLogInfo.HostName);
                }
                if (StrFunc.IsFilled(m_ErrorLogInfo.IdA_Identifier))
                {
                    eventLogData += String.Format("{0}UserName: {1}", Environment.NewLine, m_ErrorLogInfo.IdA_Identifier);
                }
                if (StrFunc.IsFilled(m_ErrorLogInfo.BrowserInfo))
                {
                    eventLogData += String.Format("{0}Browser: {1}", Environment.NewLine, m_ErrorLogInfo.BrowserInfo);
                }
                if (StrFunc.IsFilled(m_ErrorLogInfo.URL))
                {
                    eventLogData += String.Format("{0}URL: {1}", Environment.NewLine, m_ErrorLogInfo.URL);
                }
            }
            m_EventLogData = eventLogData;
        }
        #endregion EventLog
        #region LogFile
        private void FormatLogFile()
        {
            string sLogData = string.Empty;
            if (m_ErrorLogInfo != default(ErrorLogInfo))
            {
                sLogData = String.Format("{0,10} | {1,8} | {2,9} | {3,15} | {4,15} | {5,100} | {6,16} | {7,16} | {8,16} | {9,16} | {10,16} | {11,100}",
                m_ErrorLogInfo.DtError.ToString("yyyy-MM-dd"),
                m_ErrorLogInfo.DtError.ToString("T"),
                m_ErrorLogInfo.Severity.ToString(),
                m_ErrorLogInfo.Method,
                m_ErrorLogInfo.Source,
                m_ErrorLogInfo.Message,
                m_ErrorLogInfo.AppName,
                m_ErrorLogInfo.AppVersion,
                m_ErrorLogInfo.HostName,
                m_ErrorLogInfo.IdA_Identifier,
                m_ErrorLogInfo.BrowserInfo,
                m_ErrorLogInfo.URL);
            }
            m_LogFileData = sLogData;
        }
        #endregion LogFile
        #endregion Private formatting functions
    }
}

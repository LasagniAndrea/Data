using EFS.ACommon;
using System.Collections.Generic;
using System.Diagnostics;

namespace EFS.Common.Log
{
    /// <summary>
    /// 
    /// </summary>
    public static class EventLogTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInstanceName"></param>
        /// <param name="pData"></param>
        public static void WriteEventLogSystemError(string pInstanceName, params string[] pData)
        {
            WriteEventLogSystemError(pInstanceName, default, pData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInstanceName"></param>
        /// <param name="pSpheresException"></param>
        /// <param name="pData"></param>
        public static void WriteEventLogSystemError(string pInstanceName, SpheresException2 pSpheresException, params string[] pData)
        {
            WriteEventLogError(pInstanceName, EventLog_EventId.SpheresServices_Error_System, pSpheresException, pData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInstanceName"></param>
        /// <param name="pData"></param>
        public static void WriteEventLogSQLError(string pInstanceName, params string[] pData)
        {
            WriteEventLogSQLError(pInstanceName, default, pData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInstanceName"></param>
        /// <param name="pSpheresException"></param>
        /// <param name="pData"></param>
        public static void WriteEventLogSQLError(string pInstanceName, SpheresException2 pSpheresException, params string[] pData)
        {
            WriteEventLogError(pInstanceName, EventLog_EventId.SpheresServices_Error_SQL, pSpheresException, pData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInstanceName"></param>
        /// <param name="pErrorEventId"></param>
        /// <param name="pSpheresException"></param>
        /// <param name="pData"></param>
        public static void WriteEventLogError(string pInstanceName, EventLog_EventId pErrorEventId, SpheresException2 pSpheresException, params string[] pData)
        {
            List<string> message = new List<string>();
            if (pData != default)
            {
                message.AddRange(pData);
            }
            if (pSpheresException != default(SpheresException2))
            {
                if (StrFunc.IsFilled(pSpheresException.Method))
                {
                    message.Add("Method: " + pSpheresException.Method);
                }
                // FI 20200910 [XXXXX] usage de MessageExtended
                if (StrFunc.IsFilled(pSpheresException.MessageExtended))
                {
                    message.Add(pSpheresException.MessageExtended);
                }
            }
            WriteEventLog(pInstanceName, EventLogEntryType.Error, pErrorEventId, message.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInstanceName"></param>
        /// <param name="pLogLevel"></param>
        /// <param name="pEventId"></param>
        /// <param name="pData"></param>
        public static void WriteEventLog(string pInstanceName, EventLogEntryType pLogLevel, EventLog_EventId pEventId, params string[] pData)
        {
            // eventLog  => journal des évènements de windows® utilisé par le service 
            string eventLog = RegistryTools.GetEventLog(pInstanceName);
            // source  => source de l'évènement qui sera inscrit dans le journal
            string source = pInstanceName + Cst.EventLogSourceExtension;
            //
            WriteEventLog(eventLog, source, pLogLevel, pEventId, pData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEventLog"></param>
        /// <param name="pSource"></param>
        /// <param name="pLogLevel"></param>
        /// <param name="pEventId"></param>
        /// <param name="pData"></param>
        public static void WriteEventLog(string pEventLog, string pSource, EventLogEntryType pLogLevel, EventLog_EventId pEventId, params string[] pData)
        {
            EventLogCharateristics elc = new EventLogCharateristics(System.Environment.MachineName, pEventLog, pSource, pLogLevel, pEventId, pData);
            // FI 20240624 [WI981] add using instruction
            using (EventLogEx eventLogEx = new EventLogEx(elc))
            {
                eventLogEx.ReportEvent();
            }
        }
    }
}

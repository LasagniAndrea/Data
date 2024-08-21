using EFS.ACommon;
using EFS.Common.Log;
using EFS.LoggerClient.LoggerService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EFS.LoggerClient
{
    /// <summary>
    ///  Gestion d'un ensemble de log (ouverture, fermeture, écriture, etc..)
    /// </summary>
    /// FI 20240111 [WI793] Add
    public class LoggerScope
    {
        #region
        private LogScope m_LogScope;
        private int m_SequenceNumber;
        private LogLevelEnum m_LogLevel;
        private string m_ProcessType;
        private GetDateSysUTC m_GetDateSysUTC;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public LogScope LogScope { get => m_LogScope; }

        /// <summary>
        /// 
        /// </summary>
        public LogLevelEnum LogLevel { get => m_LogLevel; }

        /// <summary>
        /// Démarre l'ensemble de log
        /// </summary>
        /// <param name="pGetDateSysUTC"></param>
        /// <param name="pLogScope">Identification de l'ensemble de log</param>
        /// <returns></returns>
        public LogScope BeginScope(GetDateSysUTC pGetDateSysUTC, LogScope pLogScope)
        {
            if ((LoggerManager.IsInitialized) && (pLogScope != default))
            {
                m_LogScope = pLogScope;
                if (m_LogScope.ProcessType == m_LogScope.InitialProcessType)
                {
                    // -1 parce que le 1er log ajouté alimente PROCESS_L
                    m_SequenceNumber = -1;
                }
                else
                {
                    m_SequenceNumber = 0;
                }
                try
                {
                    LoggerManager.LoggerClient.BeginScope(pLogScope);
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError( new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }

                m_LogLevel = LogLevelEnum.Trace;
                m_ProcessType = (pLogScope != default) ? pLogScope.ProcessType : Cst.ProcessTypeEnum.NA.ToString();
                m_GetDateSysUTC = pGetDateSysUTC;

                // FI 2020822 [XXXXX] utilisation GetDateSysUTC
                m_LogScope.DtStProcess = GetDateSysUTC();
            }
            return pLogScope;
        }

        /// <summary>
        /// Termine l'ensemble de log
        /// </summary>
        /// <param name="pStatutProcess">Statut du process pour compatibilité PROCESS_L</param>
        public void EndScope(string pStatutProcess = default)
        {
            if (LoggerManager.IsInitialized && (m_LogScope != default))
            {
                try
                {
                    m_LogScope.IdStProcess = pStatutProcess;
                    if (StrFunc.IsFilled(pStatutProcess))
                    {
                        // FI 2020822 [XXXXX] Utilisation de GetDateSysUTC()
                        m_LogScope.DtStProcess = GetDateSysUTC();
                        m_LogScope.DtProcessEnd = m_LogScope.DtStProcess;
                    }
                    Task taskEndScope = LoggerManager.LoggerClient.EndScopeAsync(m_LogScope);
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError( new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
                finally
                {
                    m_LogScope = default;
                }
            }
        }

        /// <summary>
        /// Ajoute un log à l'ensemble de log et l'envoie au service de log
        /// </summary>
        /// <param name="pLogData">Données ajoutée</param>
        /// <param name="pIsSync">true = envoie en synchrone (par défaut en asynchrone)</param>
        /// <returns></returns>
        public bool Log(LoggerData pLogData, bool pIsSync = false)
        {
            bool ret = false;
            if (LoggerManager.IsInitialized && (pLogData != default(LoggerData)) && ((pLogData.LogLevel >= m_LogLevel) || pLogData.LogLevel >= LogLevelEnum.Warning))
            {
                // Ajout de la méthode appelante
                // FI 20240801 [WI1011] Spheres® retourne la 1er méthode en dehors de l'espace de nom EFS.LoggerClient.
                string methodFullName = string.Empty;
                StackTrace st = new StackTrace();
                if (st.FrameCount >= 1)
                {
                    string currentNamsSpace = st.GetFrame(0).GetMethod().DeclaringType.Namespace; //currentNamsSpace vaut EFS.LoggerClient
                    for (int i = 1; i < st.FrameCount; i++)
                    {
                        MethodBase method = st.GetFrame(i).GetMethod();
                        if (method.DeclaringType.Namespace != currentNamsSpace)
                        {
                            pLogData.CallingMethod = method.Name;
                            pLogData.CallingClassType = method.DeclaringType.Name;
                            methodFullName = pLogData.CallingClassType + "." + pLogData.CallingMethod;
                            break;
                        };
                    }
                }


                // PM 20210125 [XXXXX] Ajout des messages de log de niveau None, Critical, Error et Warning dans le fichier de Trace
                TraceLog(pLogData, methodFullName);

                if (pLogData.LogLevel >= m_LogLevel)
                {
                    // Ajout des infos utiles au log
                    ExtendLoggerData(pLogData);
                    try
                    {
                        if (pIsSync)
                        {
                            LoggerManager.LoggerClient.Log(pLogData);
                        }
                        else
                        {
                            // Création de la tâche d'envoie de log
                            Task taskLog = LoggerManager.LoggerClient.LogAsync(pLogData);
                        }
                        ret = true;
                    }
                    catch (Exception e)
                    {
                        LoggerManager.WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), e));
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne le dernier Log reçu par le service de log 
        /// </summary>
        /// <returns></returns>
        public ResultLoggerData GetLastLog()
        {
            ResultLoggerData logData = default;
            if (LoggerManager.IsInitialized)
            {
                try
                {
                    logData = LoggerManager.LoggerClient.GetLastLog(m_LogScope);
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return logData;
        }

        /// <summary>
        /// Obtient le pire LogLevel du scope parmi Info, Warning, Error et Critical présent après la réception du journal pLogDataFirst (pour compatibilité avec IO)
        /// </summary>
        /// <param name="pLogDataFirst"></param>
        /// <returns></returns>
        public LogLevelEnum GetWorstLogLevel(LoggerData pLogDataFirst)
        {
            LogLevelEnum retLogLevel = LogLevelEnum.Info;
            if (LoggerManager.IsInitialized && (pLogDataFirst != default(LoggerData)))
            {
                try
                {
                    retLogLevel = LoggerManager.LoggerClient.GetWorstLogLevel(m_LogScope, pLogDataFirst);
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return retLogLevel;
        }

        /// <summary>
        /// Demande d'écriture de tous les logs
        /// </summary>
        /// <param name="pIsForced">Indique si l'écriture doit être forcée</param>
        /// PM 20210507 [XXXXX] Ajout paramétre pIsForced pour permettre de forcer l'écriture
        public void Write(bool pIsForced = false)
        {

            if (LoggerManager.IsInitialized && (m_LogScope != default))
            {
                try
                {
                    LoggerManager.LoggerClient.Write(m_LogScope, pIsForced);
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }

                // Pour compatibilité PROCESS_L
                if (0 == m_LogScope.IdPROCESS_L)
                {
                    GetIdProcess_L();
                }
            }
        }

        /// <summary>
        /// Demande d'écriture asynchrone de tous les logs
        /// </summary>
        public async Task WriteAsync()
        {
            if (LoggerManager.IsInitialized && (m_LogScope != default))
            {
                try
                {
                    await LoggerManager.LoggerClient.WriteAsync(m_LogScope, false);
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
                // Pour compatibilité PROCESS_L
                if (0 == m_LogScope.IdPROCESS_L)
                {
                    GetIdProcess_L();
                }
            }
        }

        /// <summary>
        /// Obtient l'IDProcess_L du journal 
        /// </summary>
        /// <returns></returns>
        public int GetIdProcess_L()
        {
            int id = 0;
            if (LoggerManager.IsInitialized)
            {
                try
                {
                    id = LoggerManager.LoggerClient.GetIdProcess_L(m_LogScope);
                    m_LogScope.IdPROCESS_L = id;
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return id;
        }

        /// <summary>
        /// Obtient l'IDProcess_L du journal en faisant plusieurs tentatives si besoin
        /// </summary>
        /// <param name="pNbLoop"></param>
        /// <param name="pMilliSec"></param>
        /// <returns></returns>
        public int WaitIdProcess_L(int pNbLoop, int pMilliSec)
        {
            int id = 0;
            if (LoggerManager.IsInitialized)
            {
                int nbTry = 1;
                id = GetIdProcess_L();
                while ((id == 0) && (nbTry <= pNbLoop))
                {
                    Thread.Sleep(pMilliSec);
                    id = GetIdProcess_L();
                    nbTry += 1;
                }
                if (id == 0)
                {
                    LoggerManager.WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), string.Format("Unable to get IDPROCESS_L after {0} retries", nbTry)));
                }
                else
                {
                    LoggerManager.TraceManager.TraceVerbose(ErrorLogTools.GetMethodName(), string.Format("IDPROCESS_L ({0}) obtained after {1} retries", id, nbTry));
                }
            }
            return id;
        }

        /// <summary>
        /// Définie le niveau de log
        /// </summary>
        /// <param name="pLogLevel"></param>
        public void SetLogLevel(LogLevelEnum pLogLevel)
        {
            m_LogLevel = pLogLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLogData"></param>
        private void ExtendLoggerData(LoggerData pLogData)
        {
            if (pLogData != default(LoggerData))
            {
                m_SequenceNumber += 1;

                pLogData.ProcessType = m_ProcessType;
                pLogData.SequenceNumber = m_SequenceNumber;
                pLogData.LogScope = m_LogScope;
                pLogData.DtUtcLog = GetDateSysUTC();


                pLogData.InstanceName = LoggerManager.AppInstance.Name;
                pLogData.InstanceVersion = LoggerManager.AppInstance.Version;
                pLogData.InstanceIda = LoggerManager.AppInstance.Ida;

                pLogData.ProcessName = LoggerManager.CurrentProcess.Name;
                pLogData.OS_PID = LoggerManager.CurrentProcess.Id;

                pLogData.HostName = System.Environment.MachineName;
                pLogData.ThreadID = System.Environment.CurrentManagedThreadId;
            }
        }

        /// <summary>
        /// Retourne l'horodatage courant (fuseau horaire UTC)
        /// <para>A partir du serveur SQL (si m_GetDateSysUTC est différent de null) ou à partir du serveur applicatif </para>
        /// </summary>
        /// <returns></returns>
        /// FI 20200812 [XXXXX] Add
        /// PM 20200818 [XXXXX] Ajout trace et log
        private DateTime GetDateSysUTC()
        {
            DateTime ret;
            if (m_GetDateSysUTC == default)
            {
                ret = DateTime.UtcNow;
                LoggerManager.TraceManager.TraceWarning(ErrorLogTools.GetMethodName(), "Delegate method GetDateSysUTC not available");
            }
            else
            {
                try
                {
                    ret = m_GetDateSysUTC.Invoke(m_LogScope.ConnectionString);
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError( new SpheresException2(ErrorLogTools.GetMethodName(), e));
                    ret = DateTime.UtcNow;
                }
            }
            return ret;
        }

        /// <summary>
        /// Ajout des logs de niveau NONE, CRITICAL, ERROR et WARNING dans le fichier de trace
        /// </summary>
        /// <param name="pLogData"></param>
        /// <param name="pMethodFullName"></param>
        /// PM 20210125 [XXXXX] Ajout
        private static void TraceLog(LoggerData pLogData, string pMethodFullName)
        {
            string message;

            bool isExistSysMsgCode = (pLogData.SysMsg != default(SysMsgCode));
            if (isExistSysMsgCode)
            {
                message = $"Code: {pLogData.SysMsg.MessageCode} Msg: {Ressource.GetSystemMsg(pLogData.SysMsg.SysCode.ToString(), pLogData.SysMsg.SysNumber.ToString())}";
            }
            else
            {
                message = $"Msg: {pLogData.Message}";
            }

            if (pLogData.Parameters != default(List<LogParam>))
            {
                // Alimentation des paramétres du message
                IEnumerable<string> msgParameters = pLogData.Parameters.Where(p => p.DataSpecified).Select(p => p.Data);
                int max = System.Math.Min(msgParameters.Count(), 10);
                if (max > 0)
                {
                    for (int i = 1; i <= max; i += 1)
                    {
                        string data = msgParameters.ElementAt(i - 1);
                        if (data != default)
                        {
                            message = message.Replace(StrFunc.AppendFormat("{{{0}}}", i), data);
                        }
                    }
                }
            }


            // FI 20220719 s'il existe un code (ie "SYS-03469"), le log de Spheres® n'affiche que le message associé au code
            // Dans la trace, Spheres ajoute donc l'eventuel message lorsqu'il est renseigné (ce message est renseigné en cas d'exception notamment. Il contient le message de l'exception et toute la stack) 
            // Ces informations pourront être utile au support pour effectuer un diagnostique            
            string msgTrace = message;
            if (isExistSysMsgCode && StrFunc.IsFilled(pLogData.Message))
                msgTrace += Cst.CrLf + $"Msg complete: {pLogData.Message}";

            switch (pLogData.LogLevel)
            {
                case LogLevelEnum.None:
                    LoggerManager.TraceManager.TraceInformation(pMethodFullName, msgTrace);
                    break;
                case LogLevelEnum.Critical:
                case LogLevelEnum.Error:
                    LoggerManager.TraceManager.TraceError(pMethodFullName, msgTrace);
                    break;
                case LogLevelEnum.Warning:
                    LoggerManager.TraceManager.TraceWarning(pMethodFullName, msgTrace);
                    break;
            }
        }

    }
}

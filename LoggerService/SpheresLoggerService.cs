using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
//
namespace EFS.LoggerService
{
    /// <summary>
    /// Classe contenant la gestion du TraceManager
    /// </summary>
    public static class LoggerServiceAppTool
    {
        #region Members
        private const string m_InstanceName = "SpheresLogger";

        private static SpheresTraceManager m_TraceManager;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Nom de l'instance SpheresLogger
        /// </summary>
        public static string InstanceName
        {
            get { return m_InstanceName;  }
        }

        /// <summary>
        /// Gestionnaire de trace fichier
        /// </summary>
        public static SpheresTraceManager TraceManager
        {
            get { return m_TraceManager; }
        }
        #endregion Accessors

        #region Methods
        /// <summary>
        /// Initialisation de Diagnostics Trace
        /// </summary>
        public static void InitilizeTraceManager()
        {
            if (default(SpheresTraceManager) == m_TraceManager)
            {
                if (default(AppInstance) != EFS.Common.AppInstance.MasterAppInstance)
                {
                    // Prendre le SpheresTraceManager de l'instance si il existe (cas du LoggerService hébergé dans Service SpheresLogger)
                    m_TraceManager = EFS.Common.AppInstance.TraceManager;
                }
                else
                {
                    m_TraceManager = new SpheresTraceManager(InstanceName);
                    m_TraceManager.NewTrace();

                    // Pour lécriture dans la trace en cas de d'erreur de requête SQL
                    if (null != m_TraceManager.SpheresTrace)
                    {
                        DataHelper.traceQueryError = m_TraceManager.TraceError;
                        DataHelper.traceQueryWarning = m_TraceManager.TraceWarning;
                        DataHelper.sqlDurationLimit = m_TraceManager.SpheresTrace.SqlDurationLimit;
                    }
                }
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de paramètres
    /// </summary>
    public class LoggerSettings
    {
        #region Members
        /// <summary>
        /// Nombre maximum de journaux gardés en mémoire
        /// </summary>
        private readonly int m_MaxLogStorage = 500000;
        /// <summary>
        /// Temps minimum avant pouvoir supprimer les journaux de la mémoire
        /// </summary>
        private TimeSpan m_MinLogKeepingTime = new TimeSpan(12, 0, 0);

        private readonly int m_MaxWriteInterval = 60;
        private readonly int m_MinWriteInterval = 10;

        /// <summary>
        /// Nombre de messages reçus avant écriture automatique lors de reception de messages
        /// </summary>
        private readonly int m_NbLogBeforeWrite = 50;
        /// <summary>
        /// Nombre de secondes (int) entre 2 écritures automatique lors de reception de messages
        /// </summary>
        private readonly int m_WriteTimeInterval = 10;
        /// <summary>
        /// Nombre de secondes (TimeSpan) entre 2 écritures automatique lors de reception de messages
        /// </summary>
        private TimeSpan m_WriteTimeSpan;

        /// <summary>
        /// Nombre de millisecondes entre 2 écritures périodiques par le Timer
        /// </summary>
        private readonly int m_TimerWriteDelay = 10000;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Nombre maximum de journaux gardés en mémoire
        /// </summary>
        public int MaxLogStorage
        {
            get { return m_MaxLogStorage; }
        }
        /// <summary>
        /// Temps minimum avant pouvoir supprimer les journaux de la mémoire
        /// </summary>
        public TimeSpan MinLogKeepingTime
        {
            get { return m_MinLogKeepingTime; }
        }
        public int MaxWriteInterval
        {
            get { return m_MaxWriteInterval; }
        }
        public int MinWriteInterval
        {
            get { return m_MinWriteInterval; }
        }

        /// <summary>
        /// Nombre de messages reçus avant écriture automatique lors de reception de messages
        /// </summary>
        public int NbLogBeforeWrite
        {
            get { return m_NbLogBeforeWrite; }
        }
        /// <summary>
        /// Nombre de secondes (int) entre 2 écritures automatique lors de reception de messages
        /// </summary>
        public int WriteTimeInterval
        {
            get { return m_WriteTimeInterval; }
        }
        /// <summary>
        /// Nombre de secondes (TimeSpan) entre 2 écritures automatique lors de reception de messages
        /// </summary>
        public TimeSpan WriteTimeSpan
        {
            get { return m_WriteTimeSpan; }
        }

        /// <summary>
        /// Nombre de milliseconde entre 2 écritures périodiques
        /// </summary>
        public int TimerWriteDelay
        {
            get { return m_TimerWriteDelay; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public LoggerSettings()
        {
            m_WriteTimeSpan = new TimeSpan(0, 0, m_WriteTimeInterval);
            if (int.TryParse(SystemSettings.GetAppSettings("MaxLogStorage", m_MaxLogStorage.ToString()), out int maxLogStorage))
            {
                m_MaxLogStorage = maxLogStorage;
            }
            if (TimeSpan.TryParse(SystemSettings.GetAppSettings("MinLogKeepingTime", m_MinLogKeepingTime.ToString()), out TimeSpan minLogKeepingTime))
            {
                m_MinLogKeepingTime = minLogKeepingTime;
            }
        }
        #endregion Constructor
    }

    /// <summary>
    /// Gestion du service de journal
    /// </summary>
    public class SpheresLoggerService : ISpheresLoggerService
    {
        #region Members
        private readonly static LogStorage m_LoggerStore = new LogStorage();
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public SpheresLoggerService()
        {
            LoggerServiceAppTool.InitilizeTraceManager();
        }
        #endregion Constructor

        #region Methods ISpheresLoggerService
        /// <summary>
        /// Indique que le service est actif
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            return true;
        }

        /// <summary>
        /// Commence un nouvel ensemble de journaux
        /// </summary>
        /// <returns></returns>
        public void BeginScope(LogScope pScope)
        {
            if (pScope != default(LogScope))
            {
                try
                {
                    m_LoggerStore.BeginScope(pScope);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
        }

        /// <summary>
        /// Termine un ensemble de journaux
        /// </summary>
        /// <returns></returns>
        public void EndScope(LogScope pScope)
        {
            if (pScope != default(LogScope))
            {
                try
                {
                    m_LoggerStore.EndScope(pScope);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
        }

        /// <summary>
        /// Obtient les infos d'un ensemble de journaux à partir d'un IpProcess (de PROCESS_L) s'il existe déjà
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pIdProcess"></param>
        /// <returns></returns>
        public LogScope GetLogScope(string pConnectionString, int pIdProcess)
        {
            LogScope scope = default;
            if ((StrFunc.IsFilled(pConnectionString)) && (pIdProcess != 0))
            {
                try
                { 
                    scope = m_LoggerStore.GetLogScope(pConnectionString, pIdProcess);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return scope;
        }

        /// <summary>
        /// Ajout un élément de journal
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public void Log(LoggerData pData)
        {
            if (pData != default(LoggerData))
            {
                try
                {
                    if (pData.DtUtcLog == default)
                    {
                        pData.DtUtcLog = DateTime.UtcNow;
                    }
                    m_LoggerStore.Add(pData);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
        }

        /// <summary>
        /// Mettre à jour le LogLevel d'un élément de journal
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pNewLogLevel"></param>
        /// <returns></returns>
        public bool UpdateLogLevel(LoggerData pData, LogLevelEnum pNewLogLevel)
        {
            bool isUpdated = false;
            if (pData != default(LoggerData))
            {
                try
                {
                    isUpdated = m_LoggerStore.UpdateLogLevel(pData, pNewLogLevel);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return isUpdated;
        }

        /// Mettre à jour le LogLevel du premier et dernier élément de journal avec le pire LogLevel des autres éléments
        /// </summary>
        /// <param name="pDataFirst"></param>
        /// <param name="pDataLast"></param>
        /// <returns></returns>
        public bool UpdateWithWorstLogLevel(LoggerData pDataFirst, LoggerData pDataLast)
        {
            bool isUpdated = false;
            if ((pDataFirst != default(LoggerData)) && (pDataFirst.LogScope != default(LogScope)))
            {
                try
                {
                    isUpdated = m_LoggerStore.UpdateWithWorstLogLevel(pDataFirst, pDataLast);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return isUpdated;
        }

        /// <summary>
        /// Obtient le statut de l'écriture automatique du journal
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public bool GetAutoWrite(LogScope pScope)
        {
            bool isAutoWrite = false;
            if (pScope != default(LogScope))
            {
                try
                {
                    isAutoWrite = m_LoggerStore.GetAutoWrite(pScope);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return isAutoWrite;
        }

        /// <summary>
        /// Activation ou désactivation de l'écriture automatique du journal
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pAutoWrite"></param>
        /// <returns></returns>
        public bool SetAutoWrite(LogScope pScope, bool pAutoWrite)
        {
            bool isSet = false;
            if (pScope != default(LogScope))
            {
                try
                {
                    isSet = m_LoggerStore.SetAutoWrite(pScope, pAutoWrite);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return isSet;
        }

        /// <summary>
        /// Ecrire tous les journaux d'un LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pIsForced">Indique si l'écriture doit être forcée</param>
        /// <returns></returns>
        // PM 20210507 [XXXXX] Ajout paramétre pIsForced pour permettre de forcer l'écriture
        public void Write(LogScope pScope, bool pIsForced = false)
        {
            if (pScope != default(LogScope))
            {
                try
                {
                    m_LoggerStore.Write(pScope, pIsForced);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
        }

        /// <summary>
        /// Obtient tous les journaux en mémoire pour un LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public ResultLoggerData GetBufferLog(LogScope pScope)
        {
            ResultLoggerData allLog = default;
            if (pScope != default)
            {
                try
                {
                    IEnumerable<LoggerData> bufferLog = m_LoggerStore.GetBufferLog(pScope);
                    allLog = new ResultLoggerData(bufferLog);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return allLog;
        }

        /// <summary>
        /// Obtient le dernier journal reçu pour un LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public ResultLoggerData GetLastLog(LogScope pScope)
        {
            ResultLoggerData resLastLog = default;
            if (pScope != default)
            {
                try
                {
                    LoggerData last = m_LoggerStore.GetLast(pScope);
                    resLastLog = new ResultLoggerData(last);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return resLastLog;
        }

        /// <summary>
        /// Obtient les journaux en fonction de critères
        /// </summary>
        /// <param name="pRequest"></param>
        /// <returns></returns>
        public ResultLoggerData GetLog(LoggerRequest pRequest)
        {
            ResultLoggerData resLastLog = default;
            if (pRequest != default)
            {
                try
                {
                    resLastLog = new ResultLoggerData(m_LoggerStore.GetLog(pRequest));
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return resLastLog;
        }

        /// <summary>
        /// Obtient l'Id interne de lors d'une écriture dans PROCESS_L en mode compatibilité ou 0 si non présent dans PROCESS_L
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public int GetIdProcess_L(LogScope pScope)
        {
            int id = 0;
            if (pScope != default(LogScope))
            {
                try
                {
                    id = m_LoggerStore.GetIdProcess_L(pScope);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return id;
        }

        /// <summary>
        /// Obtient le pire LogLevel du scope parmi Info, Warning, Error et Critical présent aprés la réception du journal pData (pour compatibilité avec IO)
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        public LogLevelEnum GetWorstLogLevel(LogScope pScope, LoggerData pData)
        {
            LogLevelEnum retLogLevel = LogLevelEnum.Info;
            if ((pScope != default(LogScope)) && (pData != default(LoggerData)) && (pData.LogScope != default(LogScope)) && (pData.LogScope.LogScopeId == pScope.LogScopeId))
            {
                try
                {
                    retLogLevel = m_LoggerStore.GetWorstLogLevel(pScope, pData);
                }
                catch (Exception e)
                {
                    WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return retLogLevel;
        }
        #endregion Methods ISpheresLoggerService

        #region Private Methods
        /// <summary>
        /// Ecriture des erreurs dans fichier trace et observateur d'événement
        /// </summary>
        /// <param name="pInstanceName"></param>
        /// <param name="pSpheresException"></param>
        /// <param name="pData"></param>
        private static void WriteEventLogSystemError(string pInstanceName, SpheresException2 pSpheresException, params string[] pData)
        {
            LoggerServiceAppTool.TraceManager.TraceError(pSpheresException.Method, ExceptionTools.GetMessageAndStackExtended(pSpheresException));
            //
            EventLogTools.WriteEventLogSystemError(pInstanceName, pSpheresException, pData);
        }
        #endregion Private Methods
    }

    /// <summary>
    /// Class de stockage et manipulation des journaux
    /// </summary>
    internal class LogStorage
    {
        #region Members
        //
        // Stockage des journaux par process principal
        private readonly ConcurrentDictionary<Guid, LogArea> m_LogAreaBank = new ConcurrentDictionary<Guid, LogArea>();
        //
        // Nombre de journaux en mémoire
        private int m_NbAllLogInMemory = 0;
        //
        // Ensemble des SQL writers (1 par ConnectionString)
        private readonly ConcurrentDictionary<string,LoggerSQLWriter> m_SQLWriterBank = new ConcurrentDictionary<string, LoggerSQLWriter>();
        //
        // Timer d'écriture périodique
        private readonly LoggerTimer m_WriteTimer = default;
        //
        // Paramétrage
        private readonly LoggerSettings m_Settings;
        //
        // Service ayant déjà envoyé un journal
        // PM 20240624 [WI967] New
        private readonly ConcurrentBag<LogInstance> m_InstanceConnected = new ConcurrentBag<LogInstance>();
        #endregion Members

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        public LogStorage()
        {
            m_Settings = new LoggerSettings();
            m_WriteTimer = new LoggerTimer(WriteByTimer, m_Settings.TimerWriteDelay, m_Settings.TimerWriteDelay);
            m_WriteTimer.StartTimer();
        }
        #endregion Constructors

        #region Destructor
        /// <summary>
        /// Destructeur
        /// </summary>
        ~LogStorage()
        {
            if (m_WriteTimer != default(LoggerTimer))
            {
                m_WriteTimer.StopTimer();
            }
        }
        #endregion Destructor

        #region Methods
        /// <summary>
        /// Commence un nouvel ensemble de journaux (si celui-ci n'existe pas déjà)
        /// </summary>
        /// <returns></returns>
        public void BeginScope(LogScope pScope)
        {
            if (pScope != default(LogScope))
            {
                LogArea logArea = GetLogArea(pScope);
                if (logArea == default(LogArea))
                {
                    AddScope(pScope);
                }
                else
                {
                    logArea.AddSubScope(pScope);
                }
            }
        }

        /// <summary>
        /// Ajout du LogScope dans l'ensemble des LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        private LogArea AddScope(LogScope pScope)
        {
            LoggerSQLWriter sqlWritter = default;
            RDBMSConnectionInfo csInfo = default;
            if (pScope != default(LogScope))
            {
                // Rechercher ou créer un LoggerSQLWriter
                csInfo = new RDBMSConnectionInfo(pScope.ConnectionString);
                string connectionStringKey = csInfo.ShortCSWithoutPwd;
                if (StrFunc.IsFilled(connectionStringKey) && (false == m_SQLWriterBank.TryGetValue(connectionStringKey, out sqlWritter)))
                {
                    sqlWritter = new LoggerSQLWriter(csInfo);
                    m_SQLWriterBank.TryAdd(connectionStringKey, sqlWritter);
                }
                // Garder le processType d'origine
                pScope.InitialProcessType = pScope.ProcessType;
            }
            LogArea logArea = new LogArea(pScope, csInfo, sqlWritter, m_Settings);
            if (false == m_LogAreaBank.TryAdd(logArea.LogScopeId, logArea))
            {
                m_LogAreaBank.TryGetValue(logArea.LogScopeId, out logArea);
            }
            return logArea;
        }

        /// <summary>
        /// Termine un ensemble de journaux
        /// </summary>
        /// <returns></returns>
        public void EndScope(LogScope pScope)
        {
            if (pScope != default(LogScope))
            {
                LogArea logArea = GetLogArea(pScope);
                if (logArea != default(LogArea))
                {
                    logArea.Terminate(pScope);
                }
            }
        }

        /// <summary>
        /// Obtient les infos d'un ensemble de journaux à partir d'un IpProcess (de PROCESS_L) s'il existe déjà
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pIdProcess"></param>
        /// <returns></returns>
        public LogScope GetLogScope(string pConnectionString, int pIdProcess)
        {
            LogScope scope = default;
            if ((StrFunc.IsFilled(pConnectionString)) && (pIdProcess != 0))
            {
                RDBMSConnectionInfo csInfo = new RDBMSConnectionInfo(pConnectionString);
                IEnumerable<LogArea> foundLogArea = m_LogAreaBank.Values.Where(s => (s.ConnectionStringKey == csInfo.ShortCSWithoutPwd) && (s.IdPROCESS_L == pIdProcess)).OrderByDescending(l => l.DtLastReceived);
                LogArea logArea = foundLogArea.FirstOrDefault();
                int nbLoop = 0;
                while ((logArea == default) && (nbLoop < 20))
                {
                    Thread.Sleep(100);
                    //logArea = m_LogAreaBank.Values.Where(s => (s.ConnectionStringKey == csInfo.ShortCSWithoutPwd) && (s.IdPROCESS_L == pIdProcess)).FirstOrDefault();
                    // PM 20221202 [XXXXX] Bien que cela ne devrais pas arriver, au cas où il y aurait plusieurs LogArea ayant l'IdProcess recherché : prendre le plus récent
                    IEnumerable<LogArea> logAreaValues = m_LogAreaBank.Values.Where(s => (s.ConnectionStringKey == csInfo.ShortCSWithoutPwd) && (s.IdPROCESS_L == pIdProcess));
                    logArea = logAreaValues.OrderByDescending(e => e.DtLastReceived).FirstOrDefault();
                    //
                    nbLoop += 1;
                }
                if (logArea != default(LogArea))
                {
                    scope = logArea.LogScope;

                    LoggerServiceAppTool.TraceManager.TraceVerbose(ErrorLogTools.GetMethodName(), string.Format("LogArea for IdProcess {0} obtained after {1} retries", pIdProcess, nbLoop));
                }
                else
                {
                    LoggerServiceAppTool.TraceManager.TraceError(ErrorLogTools.GetMethodName(), string.Format("Unable to get LogArea for IdProcess {0} after {1} retries", pIdProcess, nbLoop));
                }
            }
            return scope;
        }

        /// <summary>
        /// Ajout un élément de log dans la collection de journaux
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public bool Add(LoggerData pData)
        {
            bool isAdded = false;
            if ((pData != default(LoggerData)) && (pData.LogScope != default(LogScope)))
            {
                LogArea logArea = GetLogArea(pData.LogScope);
                if (logArea == default(LogArea))
                {
                    // LogScope inexistant : le créer
                    logArea = AddScope(pData.LogScope);
                }
                if (logArea != default(LogArea))
                {
                    bool isIdProcessEmpty = (logArea.IdPROCESS_L == 0);
                    isAdded = logArea.Add(pData);
                    if (isAdded)
                    {
                        // PM 20240624 [WI967] Ajout trace de nouvelle instance connectée lors de la réception du premier log
                        if (logArea.Count == 1)
                        {
                            TraceInstance(pData);
                        }
                        //
                        // PM 20221202 [XXXXX] Si nouveau IdProcess:
                        // Rechercher si des LogArea ont la même clé mais avec un LogScopeId différent et les supprimer
                        // Cas pouvant survenir lors de rechargement de la base de données
                        if (isIdProcessEmpty && (logArea.IdPROCESS_L != 0))
                        {
                            IEnumerable<LogArea> logAreaValues = m_LogAreaBank.Values.Where(s => (s.ConnectionStringKey == logArea.ConnectionStringKey) && (s.IdPROCESS_L == logArea.IdPROCESS_L) && (s.LogScopeId != logArea.LogScopeId));
                            foreach (LogArea log in logAreaValues)
                            {
                                m_LogAreaBank.TryRemove(log.LogScopeId, out LogArea removedLogArea);
                            }
                        }

                        m_NbAllLogInMemory += 1;
                        if (m_NbAllLogInMemory > m_Settings.MaxLogStorage)
                        {
                            FreeOlderLog();
                        }
                    }
                }
            }
            return isAdded;
        }

        /// <summary>
        /// Mettre à jour le LogLevel d'un élément de journal
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pNewLogLevel"></param>
        /// <returns></returns>
        public bool UpdateLogLevel(LoggerData pData, LogLevelEnum pNewLogLevel)
        {
            bool isUpdated = false;
            if ((pData != default(LoggerData)) && (pData.LogScope != default(LogScope)))
            {
                LogArea logArea = GetLogArea(pData.LogScope);
                if (logArea != default(LogArea))
                {
                    isUpdated = logArea.UpdateLogLevel(pData, pNewLogLevel);
                }
            }
            return isUpdated;
        }

        /// Mettre à jour le LogLevel du premier et dernier élément de journal avec le pire LogLevel des autres éléments
        /// </summary>
        /// <param name="pDataFirst"></param>
        /// <param name="pDataLast"></param>
        /// <returns></returns>
        public bool UpdateWithWorstLogLevel(LoggerData pDataFirst, LoggerData pDataLast)
        {
            bool isUpdated = false;
            if ((pDataFirst != default(LoggerData)) && (pDataFirst.LogScope != default(LogScope))
                && ((pDataLast == default(LoggerData))
                || ((pDataLast.LogScope != default(LogScope)) && (pDataLast.LogScope.LogScopeId == pDataFirst.LogScope.LogScopeId))))
            {
                LogArea logArea = GetLogArea(pDataFirst.LogScope);
                if (logArea != default(LogArea))
                {
                    isUpdated = logArea.UpdateWithWorstLogLevel(pDataFirst, pDataLast);
                }
            }
            return isUpdated;
        }

        /// <summary>
        /// Obtient tous les journaux en mémoire pour un LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public IEnumerable<LoggerData> GetBufferLog(LogScope pScope)
        {
            IEnumerable<LoggerData> bufferLog = default;
            LogArea logArea = GetLogArea(pScope);
            if (logArea != default)
            {
                bufferLog = logArea.GetBufferLog(pScope);
            }
            return bufferLog;
        }

        /// <summary>
        /// Obtient le dernier journal reçu pour un LogScope
        /// </summary>
        /// <returns></returns>
        public LoggerData GetLast(LogScope pScope)
        {
            LoggerData last = default;
            LogArea logArea = GetLogArea(pScope);
            if (logArea != default)
            {
                last = logArea.GetLast(pScope);
            }
            return last;
        }

        /// <summary>
        /// Obtient les journaux en fonction de critères
        /// </summary>
        /// <param name="pRequest"></param>
        /// <returns></returns>
        public IEnumerable<LoggerData> GetLog(LoggerRequest pRequest)
        {
            IEnumerable<LoggerData> requestedLog = default;
            if (pRequest != default(LoggerRequest))
            {
                // TO DO
                //IEnumerable<LogUnit> result =
                //    m_LogBank.Where(u => ((pRequest.HostName == default) || (u.LogData.HostName == pRequest.HostName))
                //        && ((pRequest.ProcessName == default) || (u.LogData.ProcessName == pRequest.ProcessName))
                //        && ((pRequest.ProcessType == default) || (u.LogData.ProcessType == pRequest.ProcessType))
                //        && ((pRequest.DtUtcLogStart == default) || (u.LogData.DtUtcLog >= pRequest.DtUtcLogStart))
                //        && ((pRequest.DtUtcLogEnd == default) || (u.LogData.DtUtcLog <= pRequest.DtUtcLogEnd))
                //        && (u.LogData.LogLevel >= pRequest.LogLevel)
                //    );
                //requestedLog = result.Select(u => u.LogData);
            }
            return requestedLog;
        }

        /// <summary>
        /// Obtient l'Id interne de lors d'une écriture dans PROCESS_L en mode compatibilité ou 0 si non présent dans PROCESS_L
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public int GetIdProcess_L(LogScope pScope)
        {
            int id = 0;
            if (pScope != default(LogScope))
            {
                LogArea logArea = GetLogArea(pScope);
                if (logArea != default(LogArea))
                {
                    id = logArea.GetIdProcess_L(pScope);
                }
            }
            return id;
        }

        /// <summary>
        /// Obtient le pire LogLevel du scope parmi Info, Warning, Error et Critical présent aprés la réception du journal pData (pour compatibilité avec IO)
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        public LogLevelEnum GetWorstLogLevel(LogScope pScope, LoggerData pData)
        {
            LogLevelEnum retLogLevel = LogLevelEnum.Info;
            if ((pScope != default(LogScope)) && (pData != default(LoggerData)) && (pData.LogScope != default(LogScope)) && (pData.LogScope.LogScopeId == pScope.LogScopeId))
            {
                LogArea logArea = GetLogArea(pScope);
                if (logArea != default(LogArea))
                {
                    retLogLevel = logArea.GetWorstLogLevel(pScope, pData);
                }
            }
            return retLogLevel;
        }

        /// <summary>
        /// Obtient le statut de l'écriture automatique du journal
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public bool GetAutoWrite(LogScope pScope)
        {
            bool isAutoWrite = false;
            LogArea logArea = GetLogArea(pScope);
            if (logArea != default(LogArea))
            {
                isAutoWrite = logArea.GetAutoWrite(pScope);
            }
            return isAutoWrite;
        }

        /// <summary>
        /// Activation ou désactivation de l'écriture automatique du journal
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pAutoWrite"></param>
        /// <returns></returns>
        public bool SetAutoWrite(LogScope pScope, bool pAutoWrite)
        {
            bool isSet = false;
            LogArea logArea = GetLogArea(pScope);
            if (logArea != default(LogArea))
            {
                isSet = logArea.SetAutoWrite(pScope, pAutoWrite);
            }
            return isSet;
        }

        /// <summary>
        /// Ecrire tous les journaux d'un LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pIsForced">Indique si l'écriture doit être forcée</param>
        /// <returns></returns>
        // PM 20210507 [XXXXX] Ajout paramétre pIsForced pour permettre de forcer l'écriture
        public int Write(LogScope pScope, bool pIsForced = false)
        {
            int nbWrite = 0;
            LogArea logArea = GetLogArea(pScope);
            if (logArea != default(LogArea))
            {
                nbWrite = logArea.Write(pScope, pIsForced);
            }
            return nbWrite;
        }

        /// <summary>
        /// Ecriture des messages déclenché par le Timer
        /// </summary>
        /// <param name="state"></param>
        internal void WriteByTimer(object state)
        {
            foreach (LogArea area in m_LogAreaBank.Values)
            {
                if (area.CountNotWritten > 0)
                {
                    area.WriteByTimer(state);
                }
            }
        }

        /// <summary>
        /// Recherche du LogArea en fonction du LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        private LogArea GetLogArea(LogScope pScope)
        {
            LogArea logArea = default;
            if ((pScope != default(LogScope)) && (m_LogAreaBank != default(ConcurrentDictionary<Guid, LogArea>)))
            {
                if (pScope.LogScopeId == default)
                {
                    // Dans le cas où il n'y aurais pas de LogScopeId (ne doit pas arriver): on est sur un process enfant, rechercher le LogScope du process principal
                    LogScope mainScope = GetLogScope(pScope.ConnectionString, pScope.IdPROCESS_L);
                    pScope.LogScopeId = mainScope.LogScopeId;
                    pScope.InitialProcessType = mainScope.ProcessType;
                }
                m_LogAreaBank.TryGetValue(pScope.LogScopeId, out logArea);
            }
            return logArea;
        }

        /// <summary>
        /// Efface les journaux du LogScope n'ayant pas reçu de journal depuis le plus longtemps
        /// </summary>
        /// <returns></returns>
        private int FreeOlderLog()
        {
            int nbFree = 0;
            if ((m_LogAreaBank != default(ConcurrentDictionary<Guid, LogArea>)) && (m_LogAreaBank.Count > 0))
            {
                // Essayer de reduire au moins de moitié le nombre de message stocker en mémoire
                int nbTofree = m_Settings.MaxLogStorage / 2;
                //
                LoggerServiceAppTool.TraceManager.TraceVerbose(ErrorLogTools.GetMethodName(), string.Format("Attempt to free {0} messages out of {1} messages in memory", nbTofree, m_NbAllLogInMemory));
                //
                // Prendre les LogArea terminés par ordre chronologique du dernier message reçu
                //var scope = m_LogAreaBank.Where(a => a.Value.IsEnded).OrderBy(a => a.Value.DtLastReceived);
                // PM 20201020 Ajout contrôle sur DtLastReceived afin de ne pas supprimer les Area terminés mais qui pourraient recevoir encore des messages
                DateTime dtLastReceivedMax = DateTime.UtcNow.Subtract(m_Settings.MinLogKeepingTime);
                var scope = m_LogAreaBank.Where(a => (a.Value.IsEnded) && (a.Value.DtLastReceived < dtLastReceivedMax))
                    .OrderBy(a => a.Value.DtLastReceived);
                IEnumerator<KeyValuePair < Guid, LogArea >> scopeNumerator = scope.GetEnumerator();
                while (scopeNumerator.MoveNext() && (nbFree < nbTofree))
                {
                    KeyValuePair<Guid, LogArea> scopeToFree = scopeNumerator.Current;
                    LogArea area = scopeToFree.Value;
                    int msgCount = area.Count;
                    // Retirer le LogArea de l'ensemble en mémoire
                    if (m_LogAreaBank.TryRemove(scopeToFree.Key, out area))
                    {
                        // Incrémenter le nombre de message supprimé
                        nbFree += msgCount;
                        // Décrémenter le nombre de message en mémoire
                        m_NbAllLogInMemory -= msgCount;
                    }
                }
            }
            //
            LoggerServiceAppTool.TraceManager.TraceVerbose(ErrorLogTools.GetMethodName(), string.Format("{0} messages deleted, {1} messages still stored in memory", nbFree, m_NbAllLogInMemory));
            //
            return nbFree;
        }

        /// <summary>
        /// Ajout dans la trace toute nouvelle instance ayant envoyée un journal
        /// </summary>
        /// <param name="pData"></param>
        // PM 20240624 [WI967] New
        private void TraceInstance(LoggerData pData)
        {
            if (pData != default(LoggerData))
            {
                if (false == m_InstanceConnected.Any(i => (i.HostName == pData.HostName) && (i.InstanceName == pData.InstanceName) && (i.InstanceVersion == pData.InstanceVersion)))
                {
                    m_InstanceConnected.Add(new LogInstance(pData));
                    LoggerServiceAppTool.TraceManager.TraceInformation(ErrorLogTools.GetMethodName(), $"Receiving a log from a new instance: {pData.InstanceName} {pData.InstanceVersion} on {pData.HostName}");
                }
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Class de stockage et manipulation des journaux pour un LogScope
    /// </summary>
    internal class LogArea
    {
        #region Members
        //
        private readonly List<LogUnit> m_LogBank;
        private readonly SemaphoreSlim m_LogBankLock = new SemaphoreSlim(1, 1); // Semaphore d'accés à l'ensemble des logs
        private readonly LogScope m_LogScope;
        private readonly ConcurrentBag<Guid> m_SubLogScopeId = new ConcurrentBag<Guid>();
        private bool m_IsMainProcessEnded = false;
        private int m_NbProcessEnded = 0;
        private bool m_IsEnded = false;
        //
        #region Paramétrage
        /// <summary>
        /// Ecriture automatique
        /// </summary>
        private bool m_IsAutoWriteSQL = false;
        /// <summary>
        /// Paramètres généraux
        /// </summary>
        private readonly LoggerSettings m_Settings;
        #endregion Paramétrage
        //
        private readonly LoggerSQLWriter m_SQLWriter;
        private readonly RDBMSConnectionInfo m_ConnectionInfo;
        //
        private DateTime m_DtLastWrite = DateTime.MinValue; // Horaire local de la dernière écriture
        //
        private DateTime m_DtFirstUnsaved = DateTime.MinValue; // Horaire dans le message du premier message non sauvegardé
        private DateTime m_DtLastReceived = DateTime.MinValue; // Horaire local du dernier message reçu
        private int m_NBLogNotWritten = 0; // Nombre de message non encore écrit
        #endregion Members

        #region Accessors
        /// <summary>
        /// Identifiant du LogScope
        /// </summary>
        public Guid LogScopeId
        {
            get { return (m_LogScope != default) ? m_LogScope.LogScopeId : default; }
        }

        /// <summary>
        /// Identifiant du LogScope
        /// </summary>
        public LogScope LogScope
        {
            get { return m_LogScope; }
        }

        /// <summary>
        /// Id interne de PROCESS_L pour compatibilité PROCESS_L
        /// </summary>
        public int IdPROCESS_L
        {
            get { return ((m_LogScope != default(LogScope)) ? m_LogScope.IdPROCESS_L : 0); }
        }

        /// <summary>
        /// ConnextionString courte sans Password servant comme identifiant du RDBMS
        /// </summary>
        public string ConnectionStringKey
        {
            get { return ((m_ConnectionInfo != default(RDBMSConnectionInfo)) ? m_ConnectionInfo.ShortCSWithoutPwd : string.Empty); }
        }

        /// <summary>
        /// Heure du dernier journal reçu
        /// </summary>
        public DateTime DtLastReceived
        {
            get { return m_DtLastReceived; }
        }

        /// <summary>
        /// Nombre de log présent
        /// </summary>
        public int Count
        {
            get { return m_LogBank.Count; }
        }

        /// <summary>
        /// Nombre de log non encore écrit
        /// </summary>
        public int CountNotWritten
        {
            get { return m_NBLogNotWritten; }
        }

        /// <summary>
        /// Indique si l'écriture automatique est activée
        /// </summary>
        public bool IsAutoWrite
        {
            get { return m_IsAutoWriteSQL; }
        }
        
        /// <summary>
        /// Indique si cet ensemble de journaux est terminé et ne reçoit plus de nouveau journal
        /// </summary>
        public bool IsEnded
        {
            get { return m_IsEnded; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pLogScope"></param>
        public LogArea(LogScope pLogScope, RDBMSConnectionInfo pConnectionInfo, LoggerSQLWriter pSQLWriter, LoggerSettings pSettings)
        {
            m_IsEnded = false;
            m_LogBank = new List<LogUnit>();
            m_LogScope = pLogScope;
            m_ConnectionInfo = pConnectionInfo;
            m_SQLWriter = pSQLWriter;
            m_Settings = pSettings;
            m_DtLastWrite = DateTime.MinValue;
            m_IsAutoWriteSQL = true;
            // Sauvegarder le LogScope du process utilisant ce journal 
            AddSubScope(m_LogScope);
            //
            if (default(LogScope) != pLogScope)
            {
                string msg = string.Format("New log area for process {0} (Guid: {1})", pLogScope.InitialProcessType, pLogScope.LogScopeId);
                LoggerServiceAppTool.TraceManager.TraceVerbose(ErrorLogTools.GetMethodName(), msg);
            }
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Ajoute un sous scope au LogScope de ce LogArea
        /// </summary>
        /// <param name="pLogScope"></param>
        public void AddSubScope(LogScope pLogScope)
        {
            if ((pLogScope != default) && (pLogScope.SubLogScopeId != default))
            {
                // Sauvegarder chaque SubLogScopeId des process utilisant ce journal 
                if (false == m_SubLogScopeId.Contains(pLogScope.SubLogScopeId))
                {
                    m_SubLogScopeId.Add(pLogScope.SubLogScopeId);
                }
            }
        }

        /// <summary>
        /// Termine l'utilisation de cet ensemble de journaux
        /// </summary>
        /// <param name="pScope"></param>
        public void Terminate(LogScope pScope)
        {
            if ((pScope != default(LogScope)) && (pScope.LogScopeId == LogScopeId))
            {
                // Si on est sur le process de même type que le process d'origine
                if ((pScope.IdPROCESS_L == LogScope.IdPROCESS_L) && (pScope.ProcessType == LogScope.InitialProcessType))
                {
                    // Garder le statut du process d'origine
                    m_IsMainProcessEnded = true;
                    m_LogScope.IdStProcess = pScope.IdStProcess;
                    m_LogScope.DtStProcess = pScope.DtStProcess;
                    m_LogScope.DtProcessEnd = pScope.DtProcessEnd;

                    // Mise à jour du Statut, de son horaire et de l'horaire final
                    WriteSQLScope(pScope);

                    LoggerServiceAppTool.TraceManager.TraceVerbose(ErrorLogTools.GetMethodName(), string.Format("Main process {0} (Guid: {1} / Id: {2}) terminated", LogScope.InitialProcessType, LogScope.LogScopeId, LogScope.IdPROCESS_L));
                }

                // Regarder si le process d'origine est déjà terminé
                if (m_IsMainProcessEnded)
                {
                    // Regarder si l'horaire de fin est postérieur à celui déjà présent
                    if (pScope.DtProcessEnd > m_LogScope.DtProcessEnd)
                    {
                        // Affectation de l'horaire final
                        m_LogScope.DtProcessEnd = pScope.DtProcessEnd;

                        // Mise à jour de l'horaire final
                        WriteSQLScope(m_LogScope);
                    }
                }

                m_NbProcessEnded += 1;

                // Regarder si plus aucun process pour ce scope
                if (m_IsMainProcessEnded && (m_SubLogScopeId.Count == m_NbProcessEnded))
                {
                    // Ecriture forcée des derniers messages au cas ou il en resterait certains non écrit
                    Write(true);
                    //
                    m_IsEnded = true;
                }
            }
        }

        /// <summary>
        /// Ajout un élément de log dans la collection de journaux
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public bool Add(LoggerData pData)
        {
            bool isAdded = false;
            if ((pData != default(LoggerData)) && (pData.LogScope != default(LogScope)) && (LogScopeId == pData.LogScope.LogScopeId))
            {
                m_LogBankLock.Wait();
                try
                {
                    // Affectation éventuelle du sequence number apparié
                    AddGroupSeqNumber(pData);
                    // Ajout du journal dans l'ensemble des journaux reçus
                    m_LogBank.Add(new LogUnit(pData));
                    isAdded = true;
                    // Mise à jour de l'horaire du plus ancien message non écrit
                    if (m_DtFirstUnsaved == DateTime.MinValue)
                    {
                        m_DtFirstUnsaved = pData.DtUtcLog;
                    }
                    // Mise à jour de l'horaire du dernier message reçu
                    m_DtLastReceived = DateTime.UtcNow;
                    m_NBLogNotWritten += 1;
                }
                finally
                {
                    m_LogBankLock.Release();
                }
                //
                if (m_SQLWriter != default(LoggerSQLWriter))
                {
                    // Ecriture forcée s'il s'agit des 2 premiers messages
                    if (m_LogBank.Count < 3)
                    {
                        // Ecriture forcée
                        Write(true);

                        // Pour compatibilité : récupération de l'IdPROCESS_L
                        if (m_LogScope.IdPROCESS_L == 0)
                        {
                            m_LogScope.IdPROCESS_L = pData.LogScope.IdPROCESS_L;
                        }
                    }
                    // Sinon si le message n'est pas le premier non écrit
                    else if ((m_IsAutoWriteSQL) && (m_DtFirstUnsaved != pData.DtUtcLog))
                    {
                        // Tester si le dernier message non écrit est plus ancien que la durée d'écriture entre 2 messages
                        bool isToWrite = (DateTime.Compare(m_DtFirstUnsaved.AddSeconds(m_Settings.WriteTimeInterval), pData.DtUtcLog) <= 0);
                        //
                        if (false == isToWrite)
                        {
                            // Tester si nombre de message reçus avant écriture est atteint
                            isToWrite = m_NBLogNotWritten >= m_Settings.NbLogBeforeWrite;
                        }
                        if (isToWrite)
                        {
                            Write();
                        }
                    }
                }
            }
            return isAdded;
        }

        /// <summary>
        /// Mettre à jour le LogLevel d'un élément de journal
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pNewLogLevel"></param>
        /// <returns></returns>
        public bool UpdateLogLevel(LoggerData pData, LogLevelEnum pNewLogLevel)
        {
            bool isUpdated = false;
            if ((pData != default(LoggerData)) && (pData.LogScope != default(LogScope)) && (LogScopeId == pData.LogScope.LogScopeId))
            {
                bool isToWriteUpdate = false;
                // Recherche du journal le plus récent correpondant à pDataFirst
                LogUnit last = FindLast(pData);
                if ((last != default(LogUnit)) && (last.LogData.LogLevel != pNewLogLevel))
                {
                    last.LogData.LogLevel = pNewLogLevel;
                    last.IsToUpdate = last.IsSaved;
                    isToWriteUpdate = last.IsToUpdate;
                }
                if (isToWriteUpdate)
                {
                    // Des journaux déjà sauvegardés ont été modifiés et doivent être de nouveaux écrits
                    Update(pData.LogScope);
                }
            }
            return isUpdated;
        }

        /// <summary>
        /// Mettre à jour le LogLevel du premier et dernier élément de journal avec le pire LogLevel des autres éléments
        /// </summary>
        /// <param name="pDataFirst"></param>
        /// <param name="pDataLast"></param>
        /// <returns></returns>
        public bool UpdateWithWorstLogLevel(LoggerData pDataFirst, LoggerData pDataLast)
        {
            bool isUpdated = false;
            if ((m_LogBank.Count > 0) && (pDataFirst != default(LoggerData)) && (pDataFirst.LogScope != default(LogScope)) && (LogScopeId == pDataFirst.LogScope.LogScopeId)
                && ((pDataLast == default(LoggerData)) || ((pDataLast.LogScope != default(LogScope)) && (LogScopeId == pDataLast.LogScope.LogScopeId))))
            {
                bool isToWriteUpdate = false;

                // Recherche du journal le plus récent correpondant à pDataFirst
                LogUnit first = FindLast(pDataFirst);
                if (first != default(LogUnit))
                {
                    // Recherhe du pire LogLevel
                    LogLevelEnum worstLogLevel = WorstLogLevelInBank(first);

                    if (worstLogLevel != LogLevelEnum.Info)
                    {
                        LogUnit last = default;

                        first.LogData.LogLevel = worstLogLevel;
                        first.IsToUpdate = first.IsSaved;
                        isToWriteUpdate = first.IsToUpdate;

                        if (pDataLast == default)
                        {
                            m_LogBankLock.Wait();
                            try
                            {
                                last = m_LogBank.Last();
                            }
                            finally
                            {
                                m_LogBankLock.Release();
                            }
                        }
                        else
                        {
                            last = FindLast(pDataLast);
                        }
                        if (last != default(LogUnit))
                        {
                            last.LogData.LogLevel = worstLogLevel;
                            last.IsToUpdate = last.IsSaved;
                            isToWriteUpdate |= last.IsToUpdate;
                        }
                    }
                }

                if (isToWriteUpdate)
                {
                    // Des journaux déjà sauvegardés ont été modifiés et doivent être de nouveaux écrits
                    Update(pDataFirst.LogScope);
                }
            }
            return isUpdated;
        }

        /// <summary>
        /// Obtient tous les journaux en mémoire pour un LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public IEnumerable<LoggerData> GetBufferLog(LogScope pScope)
        {
            IEnumerable<LoggerData> bufferLog = default;
            if ((pScope != default) && (LogScopeId == pScope.LogScopeId))
            {
                bufferLog = m_LogBank.Select(u => u.LogData);
            }
            return bufferLog;
        }

        /// <summary>
        /// Obtient le dernier journal reçu
        /// </summary>
        /// <returns></returns>
        public LoggerData GetLast(LogScope pScope)
        {
            LoggerData last = default;
            if ((pScope != default) && (LogScopeId == pScope.LogScopeId))
            {
                LogUnit element = m_LogBank.LastOrDefault();
                if (element != default)
                {
                    last = element.LogData;
                }
            }
            return last;
        }

        /// <summary>
        /// Obtient l'Id interne de lors d'une écriture dans PROCESS_L en mode compatibilité ou 0 si non présent dans PROCESS_L
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public int GetIdProcess_L(LogScope pScope)
        {
            int id = 0;
            if ((pScope != default(LogScope)) && (LogScopeId == pScope.LogScopeId))
            {
                if (m_LogScope.IdPROCESS_L != 0)
                {
                    id = m_LogScope.IdPROCESS_L;

                    LoggerServiceAppTool.TraceManager.TraceVerbose(ErrorLogTools.GetMethodName(), string.Format("Process {0} (Guid: {1}), GetId: {2}", LogScope.InitialProcessType, LogScope.LogScopeId, id));
                }
                else if ((m_SQLWriter != default(LoggerSQLWriter)) && (m_LogBank.Count > 0))
                {
                    int nbTry = 0;
                    // Attendre qu'il y est un premier log présent
                    while ((m_LogBank.Count == 0) && (nbTry < 10))
                    {
                        Thread.Sleep(10);
                        nbTry += 1;
                    }
                    if (m_LogScope.IdPROCESS_L != 0)
                    {
                        id = m_LogScope.IdPROCESS_L;

                        LoggerServiceAppTool.TraceManager.TraceVerbose(ErrorLogTools.GetMethodName(), string.Format("Process {0} (Guid: {1}), GetId: {2} after {3} retries", LogScope.InitialProcessType, LogScope.LogScopeId, id, nbTry));
                    }
                    else
                    {
                        m_LogBankLock.Wait();
                        try
                        {
                            // Prendre le premier journal
                            LogUnit first = m_LogBank.OrderBy(u => u.LogData.DtUtcLog).OrderBy(u => u.LogData.SequenceNumber).FirstOrDefault();
                            if (first != default(LogUnit))
                            {
                                if (false == first.IsSaved)
                                {
                                    id = WriteSQLFirstLog(first);
                                }
                                else if ((first.LogData != default(LoggerData)) && (first.LogData.LogScope != default(LogScope)))
                                {
                                    id = first.LogData.LogScope.IdPROCESS_L;
                                    m_LogScope.IdPROCESS_L = id;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                        }
                        finally
                        {
                            m_LogBankLock.Release();

                            LoggerServiceAppTool.TraceManager.TraceVerbose(ErrorLogTools.GetMethodName(), string.Format("Process {0} (Guid: {1}), GetId: {2} after {3} retries and writing first log", LogScope.InitialProcessType, LogScope.LogScopeId, id, nbTry));
                        }
                    }
                }
            }
            return id;
        }

        /// <summary>
        /// Obtient le pire LogLevel du scope parmi Info, Warning, Error et Critical présent aprés la réception du journal pData (pour compatibilité avec IO)
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        public LogLevelEnum GetWorstLogLevel(LogScope pScope, LoggerData pData)
        {
            LogLevelEnum retLogLevel = LogLevelEnum.Info;
            if ((pScope != default(LogScope)) && (LogScopeId == pScope.LogScopeId)
                && (pData != default(LoggerData)) && (pData.LogScope != default(LogScope)) && (LogScopeId == pData.LogScope.LogScopeId))
            {
                // Recherche du journal le plus récent correpondant à pDataFirst
                LogUnit first = FindLast(pData);
                if (first != default(LogUnit))
                {
                    // Recherhe du pire LogLevel
                    retLogLevel = WorstLogLevelInBank(first);
                    //
                    if (retLogLevel == LogLevelEnum.Info)
                    {
                        // Pas d'erreur ni warning, il faut alors vérifier dans la base de données pour ce qui est batch ou externe
                        retLogLevel = WorstLogLevelInDB(first);
                    }
                }
            }
            return retLogLevel;
        }

        /// <summary>
        /// Obtient le statut de l'écriture automatique du journal
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public bool GetAutoWrite(LogScope pScope)
        {
            bool isAutoWrite = false;
            if ((pScope != default(LogScope)) && (LogScopeId == pScope.LogScopeId))
            {
                isAutoWrite = m_IsAutoWriteSQL;
            }
            return isAutoWrite;
        }

        /// <summary>
        /// Activation ou désactivation de l'écriture automatique du journal
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pAutoWrite"></param>
        /// <returns></returns>
        public bool SetAutoWrite(LogScope pScope, bool pAutoWrite)
        {
            bool isSet = false;
            if ((pScope != default(LogScope)) && (LogScopeId == pScope.LogScopeId))
            {
                m_IsAutoWriteSQL = pAutoWrite;
                isSet = m_IsAutoWriteSQL;
            }
            return isSet;
        }

        /// <summary>
        /// Ecrire tous les journaux
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pIsForced">Indique si l'écriture doit être forcée</param>
        /// <returns></returns>
        // PM 20210507 [XXXXX] Ajout paramétre pIsForced pour permettre de forcer l'écriture
        public int Write(LogScope pScope, bool pIsForced = false)
        {
            int nbWrite = 0;
            if ((pScope != default(LogScope)) && (LogScopeId == pScope.LogScopeId))
            {
                nbWrite = Write(pIsForced);
            }
            return nbWrite;
        }

        /// <summary>
        /// Mettre à jour les journaux
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        public int Update(LogScope pScope)
        {
            int nbWrite = 0;
            if ((pScope != default(LogScope)) && (LogScopeId == pScope.LogScopeId))
            {
                if (m_SQLWriter != default(LoggerSQLWriter))
                {
                    nbWrite = UpdateSQL();
                }
            }
            return nbWrite;
        }

        #region private Methods
        /// <summary>
        /// Ecrire tous les journaux
        /// (Méthode principale d'écriture des messages)
        /// </summary>
        /// <param name="pIsForced">Indique si l'écriture doit être forcée</param>
        /// <returns></returns>
        internal int Write(bool pIsForced = false)
        {
            int nbWrite = 0;
            if (m_SQLWriter != default(LoggerSQLWriter))
            {
                // Si des messages ont été reçus depuis la dernière écriture ou si de messages ne sont pas encore écrit
                if (pIsForced || (m_DtLastReceived >= m_DtLastWrite) || (m_NBLogNotWritten > 0))
                {
                    // Si la dernière écriture n'est pas trop récente
                    TimeSpan span = DateTime.UtcNow - m_DtLastWrite;
                    if (pIsForced || (span > m_Settings.WriteTimeSpan))
                    {
                        nbWrite = WriteSQL();
                    }
                }
            }
            return nbWrite;
        }

        /// <summary>
        /// Ecriture des messages déclenché par le Timer
        /// </summary>
        /// <param name="state"></param>
        internal void WriteByTimer(object _)
        {
            Write();
        }

        /// <summary>
        /// Alimentation du Sequence number apparié
        /// </summary>
        /// <param name="pData"></param>
        private void AddGroupSeqNumber(LoggerData pData)
        {
            if ((pData != default) && (pData.SysMsg != default))
            {
                if (pData.SysMsg.SysCode == SysCodeEnum.LOG)
                {
                    int grpSysNumber = 0;
                    // Prendre le code de début de groupe en fonction du code de fin de groupe
                    switch (pData.SysMsg.SysNumber)
                    {
                        case 6002: //Fin du traitement de la tâche
                            grpSysNumber = 6001; //Début du traitement de la tâche
                            break;
                        case 6004: //Fin du traitement de l'élément
                            grpSysNumber = 6003; //Début du traitement de l'élément
                            break;
                    }
                    if (grpSysNumber != 0)
                    {
                        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        // PL 20210422 Add for a Log with multiple tasks
                        // Rechercher plus récent LOG-6001 - Début du traitement de la tâche 
                        // Afin de ne rechercher ci-dessous que les logs supérieurs ou égals à l'horodatage de celui-ci.
                        // ----------------------------------------------------------------------------------------------
                        var log6001 = m_LogBank.Where(u => ((u.LogData.SysMsg != default)
                            && (u.LogData.SysMsg.SysCode == SysCodeEnum.LOG)
                            && (u.LogData.SysMsg.SysNumber == 6001)
                            && (u.LogData.DtUtcLog < pData.DtUtcLog)));
                        
                        DateTime dtUtcLatestLog6001 = DateTime.MinValue;
                        if (log6001.Count() > 0)
                        {
                            dtUtcLatestLog6001 = log6001.Max(u => u.LogData.DtUtcLog);
                        }
                        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

                        // Rechercher l'ensemble des journaux correspondant à des débuts de groupe
                        // PM 20210507 [XXXXX] La recherche du journal de début de groupe ne se fait plus avec < mais avec <= pour le cas où le début et la fin de groupe ont le même horaire
                        var pairedLog = m_LogBank.Where(u => ((u.LogData.SysMsg != default)
                            && (u.LogData.SysMsg.SysCode == SysCodeEnum.LOG)
                            && (u.LogData.SysMsg.SysNumber == grpSysNumber)
                            && (u.LogData.DtUtcLog <= pData.DtUtcLog)
                            // PL 20210422 Add for a Log with multiple tasks
                            && (u.LogData.DtUtcLog >= dtUtcLatestLog6001)));

                        if (pairedLog.Count() > 0)
                        {
                            // Affecter le plus récent début de groupe
                            pData.GroupSequenceNumber = pairedLog.Max(u => u.LogData.SequenceNumber);

                            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            // PL 20210422 TIP A REVOIR AU RETOUR DE PM
                            // Usage de la valeur -999 pour insérer 0 en BdD (chercher ailleurs par 2 fois ce même commentaire)
                            // ----------------------------------------------------------------------------------------------
                            if ((log6001.Count() > 1) && (pData.GroupSequenceNumber == 0))
                            {
                                pData.GroupSequenceNumber = -999;
                            }
                            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        }
                        else
                        {
                            pData.GroupSequenceNumber = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recherche le dernier journal reçu correspondant à celui en paramètre
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        private LogUnit FindLast(LoggerData pData)
        {
            LogUnit last = default;
            if (pData != default)
            {
                m_LogBankLock.Wait();
                try
                {
                    IEnumerable<LogUnit> matchingLog =
                    m_LogBank.Where( l => (l.LogData.LogCategory == pData.LogCategory)
                                        && (l.LogData.LogLevel == pData.LogLevel)
                                        && (l.LogData.Message == pData.Message)
                                        && (l.LogData.RankOrder == pData.RankOrder)
                                        && (l.LogData.SysMsg == pData.SysMsg)
                                        && (l.LogData.ThreadID == pData.ThreadID)).OrderBy(l => l.LogData.DtUtcLog);
                    //
                    last = matchingLog.LastOrDefault();
                }
                finally
                {
                    m_LogBankLock.Release();
                }
            }
            return last;
        }

        /// <summary>
        /// Retourne le journal non sauvegardé le plus ancien
        /// </summary>
        /// <returns></returns>
        private LogUnit FirstUnsaved()
        {
            LogUnit first = default;
            m_LogBankLock.Wait();
            try
            {
                // Prendre tous les journaux non sauvegardés du LogScope triés par date et SequenceNumber
                IEnumerable<LogUnit> unsavedLog = m_LogBank.Where(u => (u.IsSaved == false)).OrderBy(u => u.LogData.DtUtcLog).OrderBy(u => u.LogData.SequenceNumber);
                // Premier journal non sauvegardé
                first = unsavedLog.FirstOrDefault();
            }
            finally
            {
                m_LogBankLock.Release();
            }
            return first;
        }

        /// <summary>
        /// Recherche en mémoire le pire LogLevel (entre Info, Warning, Error et Critical) reçu après le journal pFirstLog
        /// </summary>
        /// <param name="pFirstLog"></param>
        /// <returns></returns>
        private LogLevelEnum WorstLogLevelInBank(LogUnit pFirstLog)
        {
            LogLevelEnum worstLogLevel = LogLevelEnum.Info;
            if (pFirstLog != default(LogUnit))
            {
                // Prendre les différents LogLevel existant dans le journal reçu après pFirstLog
                List<LogLevelEnum> distinctLogLevel;
                m_LogBankLock.Wait();
                try
                {
                    // PM 20201009 Ajout critère sur SequenceNumber
                    distinctLogLevel = m_LogBank.Where(
                        u => (u.LogData.DtUtcLog >= pFirstLog.LogData.DtUtcLog)
                        && (u.LogData.SequenceNumber > pFirstLog.LogData.SequenceNumber)).Select(u => u.LogData.LogLevel).Distinct().ToList();
                }
                finally
                {
                    m_LogBankLock.Release();
                }
                if (distinctLogLevel != default(List<LogLevelEnum>))
                {
                    if (distinctLogLevel.Contains(LogLevelEnum.Critical))
                    {
                        worstLogLevel = LogLevelEnum.Critical;
                    }
                    else if (distinctLogLevel.Contains(LogLevelEnum.Error))
                    {
                        worstLogLevel = LogLevelEnum.Error;
                    }
                    else if (distinctLogLevel.Contains(LogLevelEnum.Warning))
                    {
                        worstLogLevel = LogLevelEnum.Warning;
                    }
                }
            }
            return worstLogLevel;
        }

        /// <summary>
        /// Recherche dans la base données le pire LogLevel (entre Info, Warning, Error et Critical) reçu après le journal pFirstLog
        /// </summary>
        /// <param name="pFirstLog"></param>
        /// <returns></returns>
        private LogLevelEnum WorstLogLevelInDB(LogUnit pFirstLog)
        {
            LogLevelEnum worstLogLevel = LogLevelEnum.Info;
            if ((pFirstLog != default(LogUnit)) && (m_SQLWriter != default(LoggerSQLWriter)))
            {
                worstLogLevel = m_SQLWriter.WorstLogLevel(pFirstLog);
            }
            return worstLogLevel;
        }

        #region Methods d'accés SQL
        /// <summary>
        /// Ecriture du premier log dans la base et prendre son IdProcess
        /// </summary>
        /// <param name="pFirstLog"></param>
        /// <returns></returns>
        private int WriteSQLFirstLog(LogUnit pFirstLog)
        {
            int id = 0;
            if ((m_SQLWriter != default(LoggerSQLWriter)) && (pFirstLog != default(LogUnit)))
            {
                try
                {
                    // Ecrire le premier journal dans PROCESS_L
                    if (m_SQLWriter.SQLWriteProcessL(pFirstLog))
                    {
                        m_NBLogNotWritten -= 1;
                        if (pFirstLog.LogData.LogScope.IdPROCESS_L > 0)
                        {
                            id = pFirstLog.LogData.LogScope.IdPROCESS_L;
                            m_LogScope.IdPROCESS_L = id;
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteEventLogSQLError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e), "Error during insert into table PROCESS_L");
                }
            }
            return id;
        }

        /// <summary>
        /// Mise à jour de la table PROCESSL avec le statut et/ou l'horaire de fin
        /// </summary>
        /// <param name="pLogScope"></param>
        /// <returns></returns>
        private int WriteSQLScope(LogScope pLogScope)
        {
            int nbWrite = 0;
            if ((m_SQLWriter != default(LoggerSQLWriter)) && (pLogScope != default(LogScope)) && StrFunc.IsFilled(pLogScope.IdStProcess))
            {
                try
                {
                    if (pLogScope.DtStProcess < pLogScope.DtProcessEnd)
                    {
                        nbWrite = m_SQLWriter.SQLUpdateProcessL(pLogScope.IdPROCESS_L, pLogScope.DtProcessEnd);
                    }
                    else
                    {
                        nbWrite = m_SQLWriter.SQLUpdateProcessL(pLogScope.IdPROCESS_L, pLogScope.IdStProcess, pLogScope.DtStProcess, pLogScope.DtProcessEnd);
                    }
                }
                catch (Exception e)
                {
                    WriteEventLogSQLError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e), "Error during update of table PROCESS_L");
                }
            }
            return nbWrite;
        }

        /// <summary>
        /// Ecrire tous journaux en base de données
        /// </summary>
        /// <returns></returns>
        private int WriteSQL()
        {
            int nbWrite = 0;
            if (m_SQLWriter != default(LoggerSQLWriter))
            {
                List<LogUnit> unsavedLog = default;
                m_LogBankLock.Wait();
                try
                {
                    // Prendre tous les journaux non sauvegardés du LogScope
                    unsavedLog = m_LogBank.Where(u => (u.IsSaved == false)).OrderBy(u => u.LogData.DtUtcLog).OrderBy(u => u.LogData.SequenceNumber).ToList();

                    if ((this.m_LogScope.IdPROCESS_L == 0) && (unsavedLog.Count() > 0))
                    {
                        // PROCESS_L pas encore créé et il existe des journaux non sauvegardés
                        //
                        // Prendre le premier journal non sauvegardé
                        LogUnit first = unsavedLog.FirstOrDefault();
                        //
                        // Vérifier que des journaux n'ont pas déjà été écrit
                        if ((first != default(LogUnit)) && (false == m_LogBank.Any(u => u.IsSaved)))
                        {
                            // Aucun journal du scope n'est sauvegardé
                            //
                            // Ecrire le premier journal dans PROCESS_L
                            WriteSQLFirstLog(first);
                            //
                            // Reconstruire l'ensemble des journaux non sauvegardé
                            unsavedLog = unsavedLog.Where(u => (u.IsSaved == false)).ToList();
                        }
                    }
                    //
                    // Messages marqués comme sauvegardé à l'intérieur de la section critique même si erreur, sinon boucle car n'arrive jamais à sauvegarder
                    foreach (LogUnit log in unsavedLog)
                    {
                        log.IsSaved = true;
                    }
                    // Plus aucun journaux non sauvegardé
                    m_DtFirstUnsaved = DateTime.MinValue;
                    m_DtLastWrite = DateTime.UtcNow;
                    m_NBLogNotWritten -= unsavedLog.Count;
                }
                finally
                {
                    m_LogBankLock.Release();
                }
                //
                if ((unsavedLog != default(List<LogUnit>)) && (unsavedLog.Count() > 0))
                {
                    try
                    {
                        // Ecrire tous les journaux non encore écrit
                        nbWrite = m_SQLWriter.SQLWriteProcessDetL(m_LogScope, unsavedLog);
                    }
                    catch (Exception e)
                    {
                        WriteEventLogSystemError(LoggerServiceAppTool.InstanceName, new SpheresException2(ErrorLogTools.GetMethodName(), e));
                    }
                }
            }
            return nbWrite;
        }

        /// <summary>
        /// Modifier dans la base de données les journaux devant l'être 
        /// </summary>
        /// <returns></returns>
        private int UpdateSQL()
        {
            int nbWrite = 0;
            if (m_SQLWriter != default(LoggerSQLWriter))
            {
                List<LogUnit> toUpdateLog = default;
                m_LogBankLock.Wait();
                try
                {
                    // Prendre tous les journaux déjà sauvegardés du LogScope et devant être mis à jour
                    toUpdateLog = m_LogBank.Where(u => (u.IsSaved == true) && (u.IsToUpdate == true)).ToList();
                }
                finally
                {
                    m_LogBankLock.Release();
                }
                if ((toUpdateLog != default(List<LogUnit>)) &(toUpdateLog.Count() > 0))
                {
                    // Ecrire tous les journaux non encore écrit
                    nbWrite += m_SQLWriter.SQLUpdateProcessDetL(toUpdateLog);
                }
            }
            return nbWrite;
        }
        #endregion Methods d'accés SQL

        #region Methods d'écriture des erreurs
        /// <summary>
        /// Ecriture des erreurs systèmes dans fichier trace et observateur d'événement
        /// </summary>
        /// <param name="pInstanceName"></param>
        /// <param name="pSpheresException"></param>
        /// <param name="pData"></param>
        private static void WriteEventLogSystemError(string pInstanceName, SpheresException2 pSpheresException, params string[] pData)
        {
            LoggerServiceAppTool.TraceManager.TraceError(pSpheresException.Method, ExceptionTools.GetMessageAndStackExtended(pSpheresException));
            //
            EventLogTools.WriteEventLogSystemError(pInstanceName, pSpheresException, pData);
        }

        /// <summary>
        /// Ecriture des erreurs SQL dans fichier trace et observateur d'événement
        /// </summary>
        /// <param name="pInstanceName"></param>
        /// <param name="pSpheresException"></param>
        /// <param name="pData"></param>
        private static void WriteEventLogSQLError(string pInstanceName, SpheresException2 pSpheresException, params string[] pData)
        {
            LoggerServiceAppTool.TraceManager.TraceError(pSpheresException.Method, ExceptionTools.GetMessageAndStackExtended(pSpheresException));
            //
            EventLogTools.WriteEventLogSQLError(pInstanceName, pSpheresException, pData);
        }
        #endregion Methods d'écriture des erreurs
        #endregion private Methods
        #endregion Methods
    }

    /// <summary>
    /// Class représentant un élément de stockage d'un Log
    /// </summary>
    internal class LogUnit
    {
        #region Members
        private bool m_IsSaved = false;
        private bool m_IsToUpdate = false;
        private readonly LoggerData m_RecLogData = default;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Données du Log
        /// </summary>
        public LoggerData LogData
        {
            get { return m_RecLogData; }
        }

        /// <summary>
        /// Indicateur de sauvegarde du Log
        /// </summary>
        public bool IsSaved
        {
            get { return m_IsSaved; }
            internal set { m_IsSaved = value; }
        }

        /// <summary>
        /// Indicateur de nécessité de mise à jour du Log
        /// </summary>
        public bool IsToUpdate
        {
            get { return m_IsToUpdate; }
            internal set { m_IsToUpdate = value; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pData"></param>
        public LogUnit(LoggerData pData)
        {
            m_RecLogData = pData;
        }
        #endregion Constructors
    }

    /// <summary>
    /// Comparer de LogUnit
    /// </summary>
    internal class LogUnitDateComparer : Comparer<LogUnit>
    {
        #region override Comparer Methods
        public override int Compare(LogUnit x, LogUnit y)
        {
            int ret = 0;
            if (x != y)
            {
                if ((x != default(LogUnit)) && (y != default(LogUnit)))
                {
                    LoggerData lx = x.LogData;
                    LoggerData ly = y.LogData;
                    if (lx != ly)
                    {
                        if ((lx != default) && (ly != default))
                        {
                            ret = lx.DtUtcLog.CompareTo(ly.DtUtcLog);
                        }
                        else if (lx != default)
                        {
                            ret = lx.DtUtcLog.CompareTo(default);
                        }
                        else if (ly != default)
                        {
                            ret = ly.DtUtcLog.CompareTo(default);
                        }
                    }
                }
                else if (x != default(LogUnit))
                {
                    LoggerData lx = x.LogData;
                    if (lx != default(LoggerData))
                    {
                        ret = lx.DtUtcLog.CompareTo(default);
                    }
                }
                else if (y != default)
                {
                    LoggerData ly = y.LogData;
                    if (ly != default)
                    {
                        ret = ly.DtUtcLog.CompareTo(default);
                    }
                }
            }
            return ret;
        }
        #endregion override Comparer Methods
    }

    /// <summary>
    /// 
    /// </summary>
    // PM 20240624 [WI967] Ajout
    internal class LogInstance
    {
        #region Members
        private string m_HostName = string.Empty;
        private string m_InstanceName = string.Empty;
        private string m_InstanceVersion = string.Empty;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Nom de la machine
        /// </summary>
        public string HostName
        {
            get { return m_HostName; }
            set { m_HostName = value; }
        }

        /// <summary>
        /// Nom de l'instance
        /// </summary>
        public string InstanceName
        {
            get { return m_InstanceName; }
            set { m_InstanceName = value; }
        }

        /// <summary>
        /// Version de l'instance
        /// </summary>
        public string InstanceVersion
        {
            get { return m_InstanceVersion; }
            set { m_InstanceVersion = value; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pHostName"></param>
        /// <param name="pInstanceName"></param>
        /// <param name="pInstanceVersion"></param>
        public LogInstance(string pHostName, string pInstanceName, string pInstanceVersion)
        {
            m_HostName = pHostName;
            m_InstanceName = pInstanceName;
            m_InstanceVersion = pInstanceVersion;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pData"></param>
        public LogInstance(LoggerData pData)
        {
            if (pData != default(LoggerData))
            {
                m_HostName = pData.HostName;
                m_InstanceName = pData.InstanceName;
                m_InstanceVersion = pData.InstanceVersion;
            }
        }
        #endregion Constructors
    }
}

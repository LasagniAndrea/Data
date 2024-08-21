using EFS.ACommon;
using EFS.Common.Log;
using EFS.LoggerClient.LoggerService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace EFS.LoggerClient
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20240111 [WI793] Add
    public sealed class LoggerHelper
    {

        /// <summary>
        /// Obtient les infos d'un ensemble de journaux à partir d'un IpProcess (de PROCESS_L) s'il existe déjà
        /// </summary>
        /// <param name="pProcessType"></param>
        /// <param name="pCS"></param>
        /// <param name="pIdProcess"></param>
        /// <returns></returns>
        public static LogScope GetScope(string pProcessType, string pCS, int pIdProcess)
        {
            LogScope scope = default;
            if (LoggerManager.IsInitialized && (StrFunc.IsFilled(pCS)) && (pIdProcess != 0))
            {
                try
                {
                    scope = LoggerManager.LoggerClient.GetLogScope(pCS, pIdProcess);
                    if (scope != default)
                    {
                        scope.ProcessType = pProcessType;
                        scope.SubLogScopeId = Guid.NewGuid();
                    }
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return scope;
        }

        /// <summary>
        /// Obtient le LogScope en faisant plusieurs tentatives si besoin
        /// </summary>
        /// <param name="pMilliSec"></param>
        /// <returns></returns>
        public static LogScope WaitScope(string pProcessType, string pCS, int pIdProcess, int pNbLoop, int pMilliSec)
        {
            LogScope scope = default;
            if (LoggerManager.IsInitialized && (StrFunc.IsFilled(pCS)) && (pIdProcess != 0))
            {
                int nbTry = 0;
                scope = GetScope(pProcessType, pCS, pIdProcess);
                while ((scope == default) && (nbTry < pNbLoop))
                {
                    Thread.Sleep(pMilliSec);
                    scope = GetScope(pProcessType, pCS, pIdProcess);
                    nbTry += 1;
                }
                if (scope == default)
                {
                    LoggerManager.TraceManager.TraceError(ErrorLogTools.GetMethodName(), string.Format("Unable to get LogScope for IDPROCESS_L ({0}) after {1} tries", pIdProcess, nbTry));
                }
                else
                {
                    LoggerManager.TraceManager.TraceVerbose(ErrorLogTools.GetMethodName(), string.Format("LogScope for IDPROCESS_L ({0}) obtained after {1} try", pIdProcess, nbTry));
                }
            }
            return scope;
        }

        /// <summary>
        /// Mise à jour du niveau de log d'un log
        /// </summary>
        /// <param name="pLogData"></param>
        /// <param name="pNewLogLevel"></param>
        /// <returns></returns>
        public static bool UpdateLogLevel(LoggerData pLogData, LogLevelEnum pNewLogLevel)
        {
            bool ret = false;
            if (LoggerManager.IsInitialized && (pLogData != default(LoggerData)))
            {
                try
                {
                    // Création de la tâche de mise à jour de log
                    Task<bool> taskLog = LoggerManager.LoggerClient.UpdateLogLevelAsync(pLogData, pNewLogLevel);
                    ret = true;
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return ret;
        }

        /// <summary>
        /// Mise à jour des niveaux de log des premiers et derniers éléments de log avec le pire niveau entre ces deux logs
        /// </summary>
        /// <param name="pLogDataFirst"></param>
        /// <param name="pLogDataLast"></param>
        /// <returns></returns>
        public static bool UpdateWithWorstLogLevel(LoggerData pLogDataFirst, LoggerData pLogDataLast)
        {
            bool ret = false;
            if (LoggerManager.IsInitialized && (pLogDataFirst != default(LoggerData)))
            {
                try
                {
                    // Création de la tâche de mise à jour de log
                    Task<bool> taskLog = LoggerManager.LoggerClient.UpdateWithWorstLogLevelAsync(pLogDataFirst, pLogDataLast);
                    ret = true;
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne un ensemble de log répondant à la requête en paramètre
        /// </summary>
        /// <param name="pRequest"></param>
        /// <returns></returns>
        public static ResultLoggerData GetLog(LoggerRequest pRequest)
        {
            ResultLoggerData logData = default;
            if (LoggerManager.IsInitialized && (pRequest != default(LoggerRequest)))
            {
                try
                {
                    logData = LoggerManager.LoggerClient.GetLog(pRequest);
                }
                catch (Exception e)
                {
                    LoggerManager.WriteEventLogSystemError(new SpheresException2(ErrorLogTools.GetMethodName(), e));
                }
            }
            return logData;
        }
    }
}

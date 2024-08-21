using EFS.ACommon;
using EFS.Common.Log;
using EFS.LoggerClient.LoggerService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFS.LoggerClient
{
    /// <summary>
    /// Gestion de <see cref="CurrentScope"/>. 
    /// </summary>
    public sealed class Logger
    {

        /// <summary>
        ///  Gestion de l'ensemble de log courant
        /// </summary>
        public static LoggerScope CurrentScope { get; private set; }


        /// <summary>
        /// Démarre le log courant. Le log courant est arrêté s'il n'a pas été terminé <seealso cref="EndScope(string)"/>.  
        /// </summary>
        /// <param name="pGetDateSysUTC">null accepté, dans ce cas l'heure UTC est obtenu à partir du serveur applicatif</param>
        /// <param name="pLogScope">Identification de l'ensemble de log</param>
        public static LogScope BeginScope(GetDateSysUTC pGetDateSysUTC, LogScope pLogScope)
        {
            if ((LoggerManager.IsInitialized) && (pLogScope != default))
            {
                if (CurrentScope != default)
                {
                    EndScope("NONE");
                }

                CurrentScope = new LoggerScope();
                CurrentScope.BeginScope(pGetDateSysUTC, pLogScope);

            }
            return CurrentScope.LogScope;
        }

        /// <summary>
        /// Termine le log courant
        /// </summary>
        /// <param name="pStatutProcess">Statut du process pour compatibilité PROCESS_L</param>
        public static void EndScope(string pStatutProcess = default)
        {
            if (LoggerManager.IsInitialized && (CurrentScope != default))
            {
                try
                {
                    CurrentScope.EndScope(pStatutProcess);
                }
                finally
                {
                    CurrentScope = default;
                }
            }
        }

        /// <summary>
        /// Ajoute un log au log courant et l'envoie au service de log
        /// </summary>
        /// <param name="pLogData">Données à envoyer</param>
        /// <param name="pIsSync">true = envoie en synchrone (par défaut en asynchrone)</param>
        /// <returns></returns>
        public static bool Log(LoggerData pLogData, bool pIsSync = false)
        {
            Boolean isOK = true;
            if (LoggerManager.IsInitialized && (CurrentScope != default))
            {
                CurrentScope.Log(pLogData);
            }
            return isOK;
        }

        /// <summary>
        /// Envoie d'une collection de log au log courant et l'envoie au service de log
        /// </summary>
        /// <param name="pLogData"></param>
        /// <returns></returns>
        public static bool Log(IEnumerable<LoggerData> pLogData)
        {
            bool ret = false;
            if (pLogData != default(IEnumerable<LoggerData>))
            {
                foreach (LoggerData data in pLogData)
                {
                    ret &= Log(data);
                }
            }
            return ret;
        }

        /// <summary>
        /// Demande d'écriture de tous les log du log courant
        /// </summary>
        /// <param name="pIsForced">Indique si l'écriture doit être forcée</param>
        public static void Write(bool pIsForced = false)
        {
            if (LoggerManager.IsInitialized && (CurrentScope != default))
            {
                CurrentScope.Write(pIsForced);
            }
        }
    }

}

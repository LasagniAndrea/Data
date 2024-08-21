using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Acknowledgment;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Restriction;
using EFS.SpheresService;
using EFS.Status;
using EFS.Tuning;

using EfsML.Enum.Tools;

namespace EFS.Process
{
    /// <summary>
    /// 
    /// </summary>
    public abstract partial class ProcessBase
    {
        /// <summary>
        /// Retourne true si le niveau de log {pLogLevel} est suffisant pour être inséré dans le log 
        /// </summary>
        /// <param name="pLogLevel"></param>
        /// <returns></returns>
        public bool IsLevelToLog(LogLevelDetail pLogLevel)
        {
            return (LogDetailEnum >= pLogLevel);
        }

        /// <summary>
        ///  Obtient ou définit le niveau de log 
        /// </summary>
        public LogLevelDetail LogDetailEnum
        {
            get;
            set;
        }


        /// <summary>
        /// Identifiant du log du process (issu de PROCESS_L)
        /// </summary>

        public int IdProcess
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient true si le Logger est actif et qu'il existe un idLog  
        /// </summary>
        /// FI 20201013 [XXXXX] Add
        public Boolean IsLogAvailable
        {
            get
            {
                return (IdProcess > 0 && LoggerManager.IsInitialized);
            }
        }


        /// <summary>
        /// Création d'un nouvel LogScope compatible avec PROCESS_L. Alimentation de <see cref="IdProcess"/>
        /// 
        /// </summary>

        private void NewLogScope()
        {
            if (MQueue.header.requesterSpecified && MQueue.header.requester.idPROCESSSpecified)
            {
                // Obtenir le scope existant
                LogScope scope = LoggerHelper.WaitScope(MQueue.ProcessType.ToString(), MQueue.ConnectionString, MQueue.header.requester.idPROCESS, 10, 50);
                if (scope == default)
                {
                    scope = new LogScope(MQueue.ProcessType.ToString(), MQueue.ConnectionString, Tracker.IdTRK_L, MQueue.header.requester.idPROCESS)
                    {
                        // PL 20210421 Usage de la valeur NA pour éviter plantage lors du Parse en ProcessTypeEnum dans ProcesBase.ProcessInitialize()
                        //scope.InitialProcessType = "Not Found";
                        InitialProcessType = Cst.ProcessTypeEnum.NA.ToString()
                    };
                }
                //
                // Démarrer le scope pour ce process
                // FI 20200812 [XXXXX] usage de GetDateSysUTC 
                Logger.BeginScope(OTCmlHelper.GetDateSysUTC, scope);
                
                IdProcess = MQueue.header.requester.idPROCESS;
            }
            else
            {
                // Création et démarrage d'un nouveau scope
                LogScope scope = new LogScope(MQueue.ProcessType.ToString(), MQueue.ConnectionString, Tracker.IdTRK_L);
                
                Logger.BeginScope(OTCmlHelper.GetDateSysUTC, scope);
                
                List<LogParam> dataList = new List<LogParam>(); // Ensemble des paramètres du 1er log (alimente PROCESS_L)
                SysMsgCode sysMsg = default;
                //
                // Création et ajout des paramètres du log
                if ((null != Tracker.IdData) && ((Tracker.IdData.id != 0) || (Tracker.IdData.idIdentifier != default)))
                {
                    dataList.Add(new LogParam(Tracker.IdData.id, Tracker.IdData.idIdentifier, Tracker.IdData.idIdent, Cst.LoggerParameterLink.IDDATA));
                }
                else
                {
                    dataList.Add(new LogParam(MQueue.id, default, DataIdent, Cst.LoggerParameterLink.IDDATA));
                }
                dataList.Add(new LogParam(0, MQueue.header.messageQueueName, default, Cst.LoggerParameterLink.QUEUEMSG));
                // Paramétre LODATATXT
                string loDataTxt = MQueue.GetLogInfoDet();
                if (loDataTxt != default)
                {
                    dataList.Add(new LogParam(0, loDataTxt, default, Cst.LoggerParameterLink.LODATATXT));
                }
                if (null != Tracker.Data)
                {
                    if (StrFunc.IsFilled(Tracker.Data.data1))
                    {
                        dataList.Add(new LogParam(Tracker.Data.data1));
                    }
                    if (StrFunc.IsFilled(Tracker.Data.data2))
                    {
                        dataList.Add(new LogParam(Tracker.Data.data2));
                    }
                    if (StrFunc.IsFilled(Tracker.Data.data3))
                    {
                        dataList.Add(new LogParam(Tracker.Data.data3));
                    }
                    if (StrFunc.IsFilled(Tracker.Data.data4))
                    {
                        dataList.Add(new LogParam(Tracker.Data.data4));
                    }
                    if (StrFunc.IsFilled(Tracker.Data.data5))
                    {
                        dataList.Add(new LogParam(Tracker.Data.data5));
                    }
                }
                if (null != Tracker.Data)
                {
                    SysCodeEnum sysCode = (SysCodeEnum)StringToEnum.Parse(Tracker.Data.sysCode, SysCodeEnum.TRK);
                    sysMsg = new SysMsgCode(sysCode, Tracker.Data.sysNumber);
                }
                
                // Envoie message pour PROCESS_L (En synchrone)
                Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(ProcessState.Status), sysMsg, 0, dataList), true);
                
                // Attendre l'obtention de IdProcess_L
                IdProcess = Logger.CurrentScope.WaitIdProcess_L(20, 20);
                if (IdProcess == 0) // IsLogAvailable retourne false si IdProcess == 0
                {
                    // IDPROCESS_L n'est pas alimenté, inutile de continuer le traitement
                    SpheresException2 sEx = new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Unable to get IDPROCESS_L from SpheresLogger",
                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND));
                    throw sEx;
                }
                MQueue.header.requester.idPROCESS = IdProcess;
                MQueue.header.requester.idPROCESSSpecified = (0 < MQueue.header.requester.idPROCESS);
            }
            
            Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(LogDetailEnum));
        }

        /// <summary>
        /// Ecriture final dans le log
        /// </summary>
        private void LoggerFinalize()
        {
            if (IsLogAvailable)
            {
                if (ProcessStateTools.IsStatusPending(ProcessState.Status))
                {
                    // FI 20201014 [XXXXX] Ce message est désormais de niveau LogLevelEnum.Info
                    Logger.Log(new LoggerData(LogLevelEnum.Info, "The message is placed again in the queue"));
                }

                // PM 20210121 [XXXXX] Passage du message au niveau de log None systématiquement
                Logger.Log(new LoggerData(LogLevelEnum.None, "[Code Return:" + Cst.Space + ProcessState.CodeReturn + Cst.Space + "]", 0,
                    new LogParam(ProcessState.Status.ToString()),
                    new LogParam(Logger.CurrentScope.LogScope.InitialProcessType),
                    new LogParam(Logger.CurrentScope.LogScope.ProcessType),
                    new LogParam(Logger.CurrentScope.LogScope.LogScopeId.ToString()),
                    new LogParam(Logger.CurrentScope.LogScope.SubLogScopeId.ToString())));

                Logger.EndScope(ProcessState.Status.ToString());
            }
        }
    }
}

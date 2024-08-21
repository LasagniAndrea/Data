#region using
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.EfsSend;
using EFS.Common.IO;
using EFS.Common.IO.Interface;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresIO.Compare;
#endregion

namespace EFS.SpheresIO
{
    /// <summary>
    /// Classe qui se charge de l'éxécution d'une tâche IO
    /// </summary>
    // PM 20180219 [23824] Ajout interface IIOTaskLaunching
    public class Task : IIOTaskLaunching
    {
        #region Members
        /// <summary>
        /// Message queue 
        /// </summary>
        public IOMQueue m_IOMQueue;
        /// <summary>
        /// 
        /// </summary>
        protected IOTask m_IOTask;

        /// <summary>
        /// Id non significatif de la tâche
        /// </summary>
        protected int m_CurrentId;
        
        
        protected SpheresIOProcess m_process;
        protected SQL_IOTask m_Sql_IOTask;

        protected SMTPNotification m_TaskNotification;
        /// <summary>
        /// Date début exécution de la tâche
        /// </summary>
        protected DateTime m_StartDate;
        #endregion Members

        #region Accessors
        public MQueueRequester Requester
        {
            get
            {
                return m_IOMQueue.header.requester;
            }
        }

        /// <summary>
        /// Obtient la l'instance Spheres®I/O qui exécute la tâche
        /// </summary>
        public AppInstanceService AppInstance
        {
            get { return m_process.AppInstance; }
        }

        public AppSession Session
        {
            get { return m_process.Session; }
        }

        /// <summary>
        /// Obtient le process qui execute la tâche IO
        /// </summary>
        public ProcessBase Process
        {
            get { return (ProcessBase)m_process; }
        }
        /// <summary>
        /// L'envoi de notification est activé sur la tâche
        /// </summary>
        public bool IsNotifToSend
        {
            get { return m_Sql_IOTask.IsNotif_SMTP && (null != m_TaskNotification); }
        }

        /// <summary>
        /// statut de la tâche
        /// </summary>
        public ProcessStateTools.StatusEnum TaskStatus
        {
            set;
            get;
        }

        /// <summary>
        ///  Obtient les éléments de la tâche
        /// </summary>
        /// FI 20140618 [19911] Add propery
        public DataRow[] Element
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne true si la tâche contient l'élément {pElement} 
        /// </summary>
        /// FI 20140618 [19911] Add propery
        public Boolean ExistElement(string pElement)
        {
            Boolean ret = false;
            if (ArrFunc.IsFilled(Element))
            {
                DataRow[] row = Element.Where(e => e["IDIOELEMENT"].ToString() == pElement).ToArray();
                ret = ArrFunc.IsFilled(row);
            }
            return ret;
        }

        /// <summary>
        /// Retourne true si la tâche contient un élément dont le nom contient {pValue} 
        /// </summary>
        /// PM 20170524 [22834][23078] Nouvelle méthode
        public Boolean ExistElementContains(string pValue)
        {
            Boolean ret = false;
            if (ArrFunc.IsFilled(Element))
            {
                ret = Element.Any(e => e["IDIOELEMENT"].ToString().Contains(pValue));
            }
            return ret;
        }
        #endregion

        #region Accessors interface IIOTaskLaunching
        /// <summary>
        /// Connection string
        /// </summary>
        public string Cs
        {
            get { return m_process.Cs; }
        }
        /// <summary>
        /// Tâche IO
        /// </summary>
        public IOTask IoTask
        {
            get { return m_IOTask; }
            set { m_IOTask = value; }
        }
        
        /// <summary>
        /// Obtient l'identifiant non significatif de l'acteur requester ou de l'acteur qui lance l'instance
        /// </summary>
        // PM 20180219 [23824] Ajout
        public int UserId { get { return Process.UserId; } }
        /// <summary>
        /// Requester Hostname
        /// </summary>
        // PM 20180219 [23824] Ajout
        public string RequesterHostName { get { return Requester.hostName; } }
        /// <summary>
        /// Requester IdA
        /// </summary>
        // PM 20180219 [23824] Ajout
        public int RequesterIdA { get { return Requester.idA; } }
        /// <summary>
        /// Requester Date
        /// </summary>
        // PM 20180219 [23824] Ajout
        public DateTime RequesterDate { get { return Requester.date; } }
        /// <summary>
        /// Requester Session ID
        /// </summary>
        // PM 20180219 [23824] Ajout
        public string RequesterSessionId { get { return Requester.sessionId; } }
        /// <summary>
        /// Process Id
        /// </summary>
        // PM 20180219 [23824] Ajout
        
        //public int IdProcess { get { return process.processLog.header.IdProcess; } }
        public int IdProcess { get { return Process.IdProcess; } }
        #endregion Accessors interface IIOTaskLaunching

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcess"></param>
        public Task(SpheresIOProcess pProcess)
        {
            m_process = pProcess;
            m_IOMQueue = (IOMQueue)m_process.MQueue;
            m_CurrentId = m_process.CurrentId;
            TaskStatus = ProcessStateTools.StatusEnum.NONE;
        }
        #endregion Constructor

        #region Methods interface IIOTaskLaunching
        /// <summary>
        /// Contrôle l'existence du folder associé à un path de fichier
        /// <para>Si plusieurs tentatives ont été nécessaires pour accéder au Folder, Spheres® injecte des lignes dans le log pour le notifier</para>
        /// <para>Attention ServiceBase s'appuie sur l'écriture dans le log pour restituer des informations dans le journal des évènements de windows</para>
        /// </summary>
        /// <exception cref="SpheresException2[FOLDERNOTFOUND] si folder non accessible, l'exception donne les informations suivantes: folder,nbr de tentatives,timeout en secondes "/>
        /// <param name="pPathFileName"></param>
        /// <param name="pTimeout"></param>
        /// <param name="pLevelOrder"></param>
        public void CheckFolderFromFilePath(string pPathFileName, double pTimeout, int pLevelOrder)
        {
            Process.CheckFolderFromFilePath(pPathFileName, pTimeout, pLevelOrder);
        }
        /// <summary>
        /// Méthode qui permet d'interpréter le chemin lorsqu'il existe un "."  ou "~"
        /// <para>. est emplacé par  {AppRootFolder}</para>
        /// <para>~ est remplacé par {AppWorkingFolder}</para>
        /// <para>
        /// </para>
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Ajout
        public string GetFilepath(string pFilePath)
        {
            return AppInstance.GetFilepath(pFilePath);
        }
        /// <summary>
        /// Retoure Le chemin {AppWorkingFolder}\{Temporary} ou {AppWorkingFolder}\{Temporary}\{sessionId}
        /// Le Folder est crée lorsque qu'il nexiste pas 
        /// </summary>
        /// <param name="pAddFolderSession"></param>
        /// <returns></returns>
        // PM 20180219 [23824] Ajout
        public string GetTemporaryDirectory(AppSession.AddFolderSessionId pAddFolderSession)
        {
            return Session.GetTemporaryDirectory(pAddFolderSession);
        }
        
        

        /// <summary>
        /// Ajoute l'exception si elle est reconnue comme critique
        /// </summary>
        /// <param name="pEx"></param>
        /// FI 20200623 [XXXXX] Add
        public void AddCriticalException(Exception pEx)
        {
            Process.ProcessState.AddCriticalException(pEx);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// FI 20200623 [XXXXX] Add
        public void SetErrorWarning(ProcessStateTools.StatusEnum pStatusEnum)
        {
            Process.ProcessState.SetErrorWarning(pStatusEnum);
        }

        #endregion Methods interface IIOTaskLaunching

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20211110 [XXXXX] Ajout des paramètres d'une tâche IO dans le LOG
        public Cst.ErrLevel ProcessTask()
        {
            m_StartDate = DateTime.Now;
            Cst.IOReturnCodeEnum taskRetCode = Cst.IOReturnCodeEnum.NA;

            m_Sql_IOTask = IOTaskTools.CheckTaskEnabledExists(Cs, m_CurrentId); ;

            // Initialisation des infos pour l'envoi de la notification
            if (m_Sql_IOTask.IsNotif_SMTP)
                InitializeTaskNotification();

            try
            {
                // Initialiser le niveau de Log du Process en fonction du Niveau de Log de la tâche
                if (StrFunc.IsFilled(m_Sql_IOTask.LogLevel))
                {
                    m_process.LogDetailEnum = (LogLevelDetail)Enum.Parse(typeof(LogLevelDetail), m_Sql_IOTask.LogLevel, true);
                    // PM 20200910 [XXXXX] New Log: gestion niveau de log
                    Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(m_process.LogDetailEnum));

                    if ((Common.AppInstance.TraceManager.IsSpheresTraceAvailable) &&
                        Common.AppInstance.TraceManager.SpheresTrace.Switch.Level == System.Diagnostics.SourceLevels.Verbose)
                    {
                        Common.AppInstance.TraceManager.TraceVerbose(this, StrFunc.AppendFormat("Server info:{0}{1}", Cst.CrLf, DataHelper.GetServerVersion(Cs)));
                        string Assemblies = AssemblyTools.GetDomainAssemblies<string>();
                        Common.AppInstance.TraceManager.TraceVerbose(this, StrFunc.AppendFormat("Loaded Assemblies info:{0}{1}", Cst.CrLf, Assemblies));
                    }
                }

                // Charger les infos de la tâche dans le flux PostParsing (m_TaskPostParsing)
                m_IOTask = new IOTask(m_CurrentId.ToString(), Session.SessionId, m_Sql_IOTask);

                // Charger les paramètres de la tâche dans le flux PostParsing (m_TaskPostParsing)
                if (null != m_IOMQueue.parameters && ArrFunc.Count(m_IOMQueue.parameters.parameter) >= 1)
                {
                    int paramCount = ArrFunc.Count(m_IOMQueue.parameters.parameter);
                    if (0 < paramCount)
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6040), 0));

                        m_IOTask.parameters = new IOTaskParams
                        {
                            param = new IOTaskParamsParam[paramCount]
                        };

                        for (int i = 0; i < paramCount; i++)
                        {
                            m_IOTask.parameters.param[i] = new IOTaskParamsParam
                            {
                                id = m_IOMQueue.parameters.parameter[i].id,
                                name = m_IOMQueue.parameters.parameter[i].name,
                                displayname = m_IOMQueue.parameters.parameter[i].displayName,
                                direction = m_IOMQueue.parameters.parameter[i].direction,
                                datatype = m_IOMQueue.parameters.parameter[i].dataType.ToString(),
                                returntype = m_IOMQueue.parameters.parameter[i].ReturnType.ToString(),
                                Value = m_IOMQueue.parameters.parameter[i].Value
                            };
                            //GP 20070124. Les paramètres du task doivent pouvoir être dynamic
                            // PM 20180219 [23824] IOTools => IOCommonTools
                            m_IOTask.parameters.param[i].Value = IOCommonTools.CheckDynamicString(m_IOTask.parameters.param[i].Value, this, true);

                            // EG 20211110 [XXXXX] Ajout des paramètres d'une tâche IO dans le LOG (PARAMETRE et SA VALEUR)
                            string name = m_IOTask.parameters.param[i].id;
                            if (StrFunc.IsFilled(m_IOTask.parameters.param[i].name))
                                name = LogTools.GetCurrentCultureString(Cs, m_IOTask.parameters.param[i].name, null);

                            LoggerData loggerData = new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6041), 2, new LogParam(name), new LogParam(m_IOTask.parameters.param[i].Value));
                            Logger.Log(loggerData);
                        }
                    }
                }
                else if (m_IOMQueue.IsGatewayMqueue) 
                {
                    // FI 20240118 [WI814] add parameter PARAM_ISCREATEDBY_GATEWAY
                    // L'ajout de ce paramètre pourra être supprimé lorsque les versions des gateways en prod seront >= v14
                    m_IOTask.parameters = new IOTaskParams
                    {
                        param = new IOTaskParamsParam[]{
                                        new IOTaskParamsParam{
                                            id = IOMQueue.PARAM_ISCREATEDBY_GATEWAY,
                                            name = IOMQueue.PARAM_ISCREATEDBY_GATEWAY,
                                            datatype = TypeData.TypeDataEnum.boolean.ToString(),
                                            Value = ObjFunc.FmtToISo(true, TypeData.TypeDataEnum.boolean)
                                        }
                            }
                    };
                    
                    LoggerData loggerData = new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6041), 2, new LogParam(m_IOTask.parameters.param[0].name), new LogParam(m_IOTask.parameters.param[0].Value));
                    Logger.Log(loggerData);
                }

                // Traiter tous les éléments de la tâche
                return ProcessTaskElement();
            }
            catch (Exception)
            {
                taskRetCode = Cst.IOReturnCodeEnum.ERROR;
                throw;
            }
            finally
            {
                if ((m_Sql_IOTask.IsNotif_SMTP) && (null != m_TaskNotification))
                    // Envoi de la notification            
                    SendTaskNotification(taskRetCode);
            }
        }

        /// <summary>
        ///  Exécute toutes les éléments de la tâche
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Lorsqu'il n'existe pas d'élément sur la tâche"></exception>
        /// FI 20140618 [19911] Appel à SetTaskElementData 
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20220221 [XXXXX] Gestion IRQ
        private Cst.ErrLevel ProcessTaskElement()
        {

            Cst.ErrLevel task_RetCode = Cst.ErrLevel.SUCCESS;
            Cst.IOReturnCodeEnum previousElementRetCode = Cst.IOReturnCodeEnum.NA;
            
            // PM 20180219 [23824] SetTaskElementData() remplacé par IOTaskTools.ReadTaskElementDataRow
            //SetTaskElementData();
            Element = IOTaskTools.TaskElementDataRow(Cs, m_CurrentId);
            
            CheckElement();
            
            // DR-GS 20100420: Insert point
            if (m_IOTask.name == "RReport-Puma2")
            {
                RegulatoryReportProcess regulatoryReport = new RegulatoryReportProcess(Cs);
                regulatoryReport.Start(m_IOTask, Process, "Puma2", Process.UserId);
            }

            // GS 20121603: entry point for AUREL-BGC specific dev  
            if (m_IOTask.name == "ExportBR_Daily")
            {
                EurosysTradesAudit tradeAudit = new EurosysTradesAudit(Cs, m_IOTask, Process, Process.UserId);
                tradeAudit.TradesAuditProcess();
            }

            

            
            
            Logger.Write();
            LoggerData startElementLog = default;
            bool abortTaskProcess = false;
            int elementNumber;
            for (elementNumber = 1; elementNumber <= ArrFunc.Count(Element); elementNumber++)
            {
                DataRow elementRow = Element[elementNumber - 1];
                var element = new
                {
                    Type = (Cst.IOElementType)Enum.Parse(typeof(Cst.IOElementType), elementRow["ELEMENTTYPE"].ToString(), true),
                    Id = elementRow["IDIOELEMENT"].ToString(),
                    TaskDetId = Convert.ToInt32(elementRow["IDIOTASKDET"]),
                    Log = LogTools.IdentifierAndId(elementRow["IDIOELEMENT"].ToString(), Convert.ToInt32(elementRow["IDIOTASKDET"])),
                    RetCodeOnNoData = (Cst.IOReturnCodeEnum)Enum.Parse(typeof(Cst.IOReturnCodeEnum), elementRow["RETCODEONNODATA"].ToString(), true),
                    RetCodeOnNoDataM = (Cst.IOReturnCodeEnum)Enum.Parse(typeof(Cst.IOReturnCodeEnum), elementRow["RETCODEONNODATAM"].ToString(), true),
                    RuleOnError = Convert.IsDBNull(elementRow["RULEONERROR"]) ? null : (Nullable<Cst.RuleOnError>)Enum.Parse(typeof(Cst.RuleOnError), elementRow["RULEONERROR"].ToString(), true)
                };
                
                #region Process element by element
                Cst.ErrLevel currentElementRetCode = Cst.ErrLevel.SUCCESS;
                Boolean isCurrentElementToRun = true;

                
                ProcessStateTools.StatusEnum currentElementStatus = ProcessStateTools.StatusEnum.NA;

                try
                {
                    // Log: Début de traitement de l'élément
                    // PM 20201009 Attendre 10 millisecondes avant de générer le Log afin que son horaire ne soit pas identique à l'horaire de fin de l'élément précédant
                    Thread.Sleep(10);

                    startElementLog = new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6003), 1,
                        new LogParam(element.Type),
                        new LogParam(element.Log));

                    Logger.Log(startElementLog);

                    isCurrentElementToRun = CheckLastElementRetCode(elementRow, previousElementRetCode, out string elementRetCodeRuleLog);
                    if (false == isCurrentElementToRun)
                    {
                        // Log: Elément non exécuté car les conditions d'enchainement ne sont pas vérifiées

                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6028), 2,
                            new LogParam(Ressource.GetString(previousElementRetCode.ToString())),
                            new LogParam(elementRetCodeRuleLog),
                            new LogParam(element.Type.ToString()),
                            new LogParam(element.Log)));
                    }
                    else
                    {
                        m_IOTask.taskDet = new IOTaskDet()
                        {
                            id = element.TaskDetId.ToString(),
                            loglevel = elementRow["LOGLEVEL"].ToString(),
                            commitmode = elementRow["COMMITMODE"].ToString()
                        };

                        switch (element.Type)
                        {
                            case Cst.IOElementType.INPUT:
                                ProcessInput processInput = new ProcessInput(this, element.TaskDetId, element.Id, element.RetCodeOnNoData, element.RetCodeOnNoDataM);
                                // EG 20220221 [XXXXX] Activation IRQManaged on IO
                                if (IRQTools.IsIRQRequestedWithLog(Process, Process.IRQNamedSystemSemaphore, ref currentElementRetCode))
                                {
                                    currentElementStatus = ProcessStateTools.StatusEnum.IRQ;
                                }
                                else
                                {
                                    currentElementRetCode = processInput.Process();

                                    currentElementStatus = processInput.ProcessState.Status;
                                }
                                break;
                            case Cst.IOElementType.OUTPUT:
                                ProcessOutput processOutput = new ProcessOutput(this, element.TaskDetId, element.Id, element.RetCodeOnNoData, element.RetCodeOnNoDataM);
                                // EG 20220221 [XXXXX] Activation IRQManaged on IO
                                if (IRQTools.IsIRQRequestedWithLog(Process, Process.IRQNamedSystemSemaphore, ref currentElementRetCode))
                                {
                                    currentElementStatus = ProcessStateTools.StatusEnum.IRQ;
                                }
                                else
                                {
                                    currentElementRetCode = processOutput.Process();

                                    currentElementStatus = processOutput.ProcessState.Status;
                                }
                                break;
                            case Cst.IOElementType.SHELL:
                                // PM 20210507 [XXXXX] Ajout paramétre pIsForced de Write à True pour permettre de forcer l'écriture
                                Logger.Write(true);

                                ProcessShell ioShell = new ProcessShell(this, element.Id);
                                // EG 20220221 [XXXXX] Activation IRQManaged on IO
                                if (IRQTools.IsIRQRequestedWithLog(Process, Process.IRQNamedSystemSemaphore, ref currentElementRetCode))
                                {
                                    currentElementStatus = ProcessStateTools.StatusEnum.IRQ;
                                }
                                else
                                {
                                    currentElementRetCode = ioShell.Process();

                                    // PM 20210507 [XXXXX] Ajout temporisation de 0,1 sec afin que l'horaire du début de traitement de l'élément ne soit pas identique à l'horaire de fin de traitement de l'élément
                                    Thread.Sleep(100);

                                    if (Cst.ErrLevel.TIMEOUT == currentElementRetCode)
                                    {

                                        Logger.Log(new LoggerData(LogLevelEnum.Info, " Process aborted after timeout !"));
                                    }

                                    // Le contenu de la commande Shell est inconnu. 
                                    //Spheres® fait donc une lecture du log afin de vérifier s'il existe des erreurs
                                    LogLevelEnum worstLogLevel = Logger.CurrentScope.GetWorstLogLevel(startElementLog);
                                    switch (worstLogLevel)
                                    {
                                        case LogLevelEnum.Critical:
                                        case LogLevelEnum.Error:
                                            currentElementStatus = ProcessStateTools.StatusErrorEnum;
                                            currentElementRetCode = Cst.ErrLevel.FAILURE;
                                            break;
                                        case LogLevelEnum.Warning:
                                            currentElementStatus = ProcessStateTools.StatusWarningEnum;
                                            currentElementRetCode = Cst.ErrLevel.FAILUREWARNING;
                                            break;
                                        default:
                                            currentElementStatus = ProcessStateTools.StatusSuccessEnum;
                                            break;
                                    }
                                }
                                break;
                            case Cst.IOElementType.COMPARE:
                                ProcessCompare processCompare = new ProcessCompare(this, element.TaskDetId, element.Id);
                                // EG 20220221 [XXXXX] Activation IRQManaged on IO
                                if (IRQTools.IsIRQRequestedWithLog(Process, Process.IRQNamedSystemSemaphore, ref currentElementRetCode))
                                {
                                    currentElementStatus = ProcessStateTools.StatusEnum.IRQ;
                                }
                                else
                                {
                                    currentElementRetCode = processCompare.Compare();

                                    currentElementStatus = processCompare.ProcessState.Status;
                                }
                                break;
                            default:
                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6004), 2, new LogParam(element.Type), new LogParam(element.Log)));
                                throw new Exception(StrFunc.AppendFormat("elementType: {0} is not managed", element.Type.ToString()));
                        }
                    }
                }
                catch (Exception ex)
                {

                    // EG 20130722
                    //currentElement_RetCode = exLog.ProcessState.CodeReturn;
                    Process.ProcessState.AddException(SpheresExceptionParser.GetSpheresException(null, ex));
                    Process.ProcessState.SetCodeReturnFromLastException2();


                    currentElementStatus = ProcessStateTools.StatusEnum.ERROR;
                    currentElementRetCode = Process.ProcessState.CodeReturn;

                    // FI 20180829 [XXXXX]  Appel à ExceptionTools.GetMessageExtended(ex)
                    // ex ne dvrait pas être de type SpheresException car dans une SpheresException le message d'erreur un bien lourd puisqu'il comporte la stack des Appels

                    currentElementStatus = ProcessStateTools.StatusEnum.ERROR;
                    Logger.Log(new LoggerData(LogLevelEnum.Error, ExceptionTools.GetMessageExtended(ex), 1));

                }
                finally
                {
                    // On retient la 1er erreur en code retour et on passe à l'élément suivant
                    if ((currentElementRetCode != Cst.ErrLevel.SUCCESS) && (task_RetCode == Cst.ErrLevel.SUCCESS))
                        task_RetCode = currentElementRetCode;

                    // EG 20220221 [XXXXX] Activation IRQManaged on IO
                    LogLevelEnum worstLogLevel;
                    if ((ProcessStateTools.StatusErrorEnum != currentElementStatus) &&
                        (ProcessStateTools.StatusWarningEnum != currentElementStatus) &&
                        (ProcessStateTools.StatusInterruptEnum != currentElementStatus))
                    {
                        // Recherche du pire LogLevel depuis le début du traitement de l'élément
                        worstLogLevel = Logger.CurrentScope.GetWorstLogLevel(startElementLog);
                        // PM 20200909 [XXXXX] Passage du Level en None si Info pour affichage systèmatique
                        worstLogLevel = (worstLogLevel == LogLevelEnum.Info ? LogLevelEnum.None : worstLogLevel);
                        if ((worstLogLevel == LogLevelEnum.Error) || (worstLogLevel == LogLevelEnum.Critical))
                        {
                            currentElementStatus = ProcessStateTools.StatusErrorEnum;
                        }
                        else if (worstLogLevel == LogLevelEnum.Warning)
                        {
                            currentElementStatus = ProcessStateTools.StatusWarningEnum;
                        }
                    }
                    else
                    {
                        worstLogLevel = LoggerTools.StatusToLogLevelEnum(currentElementStatus, LogLevelEnum.None);
                    }

                    LoggerData endElementLog = new LoggerData(worstLogLevel, new SysMsgCode(SysCodeEnum.LOG, 6004), 1,
                                                    new LogParam(element.Type),
                                                    new LogParam(element.Log));
                    Logger.Log(endElementLog);
                    // Gestion GRPNO : La mise à jour du message de début de traitement de l'élément n'est plus nécessaire
                    //// Mise à jour du message de début de traitement de l'élément
                    //Logger.UpdateLogLevel(startElementLog, LoggerTools.StatusToLogLevelEnum(currentElementStatus));
                    //Logger.Write();

                    // PM 20200102 [XXXXX] New Log : Mise à jour du statut de la tâche
                    // EG 20220221 [XXXXX] Activation IRQManaged on IO
                    if (currentElementStatus == ProcessStateTools.StatusErrorEnum)
                    {
                        // Lorsqu'un élément est en erreur => la tâche est en erreur
                        // Lorsqu'un élément est interrompu => la tâche est marquée comme Interrompue
                        TaskStatus = currentElementStatus;
                    }
                    else if ((currentElementStatus == ProcessStateTools.StatusWarningEnum) && (TaskStatus != ProcessStateTools.StatusErrorEnum))
                    {
                        TaskStatus = currentElementStatus;
                    }
                    else if ((currentElementStatus == ProcessStateTools.StatusInterruptEnum) && (TaskStatus != ProcessStateTools.StatusWarningEnum))
                    {
                        TaskStatus = currentElementStatus;
                    }

                    // Gestion du code retour de l'élément:
                    // - s'il y'a plusieurs éléments sur la tâche
                    // - ou bien l'envoi de notification est activé sur la tâche
                    if (ArrFunc.Count(Element) > 1 || this.IsNotifToSend)
                    {
                        // Par rapport à un "élément" donné, son "élément précédent" est toujours le "dernier élément exécuté", et ce n'est pas forcément le précédent élément.
                        if (isCurrentElementToRun)
                        {

                            previousElementRetCode = GetCurrentElementReturnCode(currentElementRetCode, currentElementStatus);

                            if (this.IsNotifToSend)
                            {
                                m_TaskNotification.ExecutedElement.Add(new Pair<string, Cst.IOReturnCodeEnum>(element.Id, previousElementRetCode));
                            }

                            // RD 20120919 [18131]
                            // Gérer la règle en cas d'anomalie:
                            // - s'il y'a plusieurs éléments sur la tâche 
                            // - ET on n'est pas sur le dérnier élément
                            if (ArrFunc.Count(Element) > 1 && ArrFunc.Count(Element) != elementNumber)
                            {
                                // EG 20130722 Add DEADLOCK
                                // RD 20210308  Utiliser element.RuleOnError
                                if (element.RuleOnError == Cst.RuleOnError.ABORT &&
                                   (previousElementRetCode == Cst.IOReturnCodeEnum.ERROR ||
                                    previousElementRetCode == Cst.IOReturnCodeEnum.DEADLOCK ||
                                    previousElementRetCode == Cst.IOReturnCodeEnum.TIMEOUT ||
                                    previousElementRetCode == Cst.IOReturnCodeEnum.IRQ))
                                {

                                    // Log: Fin de traitement de la tâche car sur cet élément "Règle en cas d'anomalie" = Arrêter
                                    // RD 20210308  Utiliser element.RuleOnError
                                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 6033), 1,
                                        new LogParam(Ressource.GetString(previousElementRetCode.ToString())),
                                        new LogParam(Ressource.GetString(element.RuleOnError.ToString())),
                                        new LogParam(element.Type),
                                        new LogParam(element.Log)));

                                    // Arrêter le traitement des éléments suivants
                                    abortTaskProcess = true;
                                }
                                else if (currentElementRetCode == Cst.ErrLevel.IRQ_EXECUTED)
                                {
                                    abortTaskProcess = true;
                                }
                            }
                        }
                    }
                }

                // RD 20120919 [18131]
                // Arrêter le traitement des éléments suivants
                if (abortTaskProcess)
                    break;

                #endregion
            }

            return task_RetCode;
        }





        /// <summary>
        /// Renvoi le nouveau code de retour en fonction:
        /// <para>- du code retour de l'élément lui même après traitement</para>
        /// <para>- du niveau du Log (Level, Status) de l'élément lui même après traitement</para>
        /// <para>- et dans le cas de SUCCESS, vérification s'il existe des érreurs, dans les détails du log précédents produits par l'élément.</para>
        /// </summary>
        /// <param name="pElementCodeReturn"></param>
        /// <param name="pCurrentElementStatus"></param>
        /// <returns></returns>
        /// RD 20150922 [21275] Modify
        private Cst.IOReturnCodeEnum GetCurrentElementReturnCode(Cst.ErrLevel pElementCodeReturn, ProcessStateTools.StatusEnum pCurrentElementStatus)
        {
            Cst.IOReturnCodeEnum ret;
            switch (pElementCodeReturn)
            {
                case Cst.ErrLevel.TIMEOUT:
                    ret = Cst.IOReturnCodeEnum.TIMEOUT;
                    break;
                // EG 20130722
                case Cst.ErrLevel.DEADLOCK:
                    ret = Cst.IOReturnCodeEnum.DEADLOCK;
                    break;
                case Cst.ErrLevel.ABORTED:             //Exception 
                case Cst.ErrLevel.BREAK:               //Exception 
                case Cst.ErrLevel.FAILURE:             //Exception 
                    ret = Cst.IOReturnCodeEnum.ERROR;
                    break;
                case Cst.ErrLevel.IRQ_EXECUTED:             //Exception 
                    ret = Cst.IOReturnCodeEnum.IRQ;
                    break;
                default:
                    // EG 20130724
                    //ProcessStateTools.StatusEnum logStatus;
                    //process.processLog.GetProcessState(pElementCodeReturn, out logStatus);
                    ProcessStateTools.StatusEnum logStatus = Process.GetProcessStatus(pElementCodeReturn);
                    if (logStatus == ProcessStateTools.StatusErrorEnum)
                        ret = Cst.IOReturnCodeEnum.ERROR;
                    else if (logStatus == ProcessStateTools.StatusWarningEnum)
                        ret = Cst.IOReturnCodeEnum.WARNING;
                    else
                        ret = Cst.IOReturnCodeEnum.SUCCESS;
                    break;
            }

            if ((Cst.IOReturnCodeEnum.SUCCESS == ret) && (ProcessStateTools.StatusSuccessEnum != pCurrentElementStatus))
            {
                if (ProcessStateTools.StatusEnum.ERROR == pCurrentElementStatus)
                {
                    ret = Cst.IOReturnCodeEnum.ERROR;
                }
                else
                {
                    ret = Cst.IOReturnCodeEnum.WARNING;
                }
            }
            return ret;
        }

        /// <summary>
        /// Vérification de la compatibilité:
        /// <para>- des conditions de lancement de l'élément en cours</para>
        /// <para>- avec le code retourné par l'élément précédent</para>
        /// </summary>
        /// <param name="pTaskDet"></param>
        /// <param name="pLastElementReturnCode"></param>
        /// <returns></returns>
        private static bool CheckLastElementRetCode(DataRow pTaskDet, Cst.IOReturnCodeEnum pLastElementReturnCode, out string pElementRetCodeRule)
        {
            pElementRetCodeRule = string.Empty;

            // Ok: L'élément en cours est le premier élément de la liste
            if (pLastElementReturnCode == Cst.IOReturnCodeEnum.NA)
            {
                return true;
            }
            else
            {
                bool isRunSuccess = BoolFunc.IsTrue(pTaskDet["ISRUNONSUCCESS"]);
                bool isRunWarning = BoolFunc.IsTrue(pTaskDet["ISRUNONWARNING"]);
                bool isRunError = BoolFunc.IsTrue(pTaskDet["ISRUNONERROR"]);
                bool isRunTimeOut = BoolFunc.IsTrue(pTaskDet["ISRUNONTIMEOUT"]);
                bool isRunDeadLock = BoolFunc.IsTrue(pTaskDet["ISRUNONDEADLOCK"]);

                // Ok: Tous les codes sont à true ou bien Tous les codes sont à false
                if ((isRunSuccess && isRunWarning && isRunError && isRunDeadLock && isRunTimeOut) ||
                    (false == (isRunSuccess || isRunWarning || isRunError || isRunDeadLock || isRunTimeOut)))
                {
                    return true;
                }
                else
                {
                    // Ok: Le code retourné par l'élément précédent est compatible
                    if ((isRunSuccess && pLastElementReturnCode == Cst.IOReturnCodeEnum.SUCCESS) ||
                        (isRunWarning && pLastElementReturnCode == Cst.IOReturnCodeEnum.WARNING) ||
                        (isRunError && pLastElementReturnCode == Cst.IOReturnCodeEnum.ERROR) ||
                        (isRunDeadLock && pLastElementReturnCode == Cst.IOReturnCodeEnum.DEADLOCK) ||
                        (isRunTimeOut && pLastElementReturnCode == Cst.IOReturnCodeEnum.TIMEOUT))
                    {
                        return true;
                    }
                }

                List<string> elementRetCodeRule = new List<string>();
                if (isRunSuccess)
                    elementRetCodeRule.Add(Ressource.GetString(Cst.IOReturnCodeEnum.SUCCESS.ToString()));
                if (isRunWarning)
                    elementRetCodeRule.Add(Ressource.GetString(Cst.IOReturnCodeEnum.WARNING.ToString()));
                if (isRunError)
                    elementRetCodeRule.Add(Ressource.GetString(Cst.IOReturnCodeEnum.ERROR.ToString()));
                if (isRunDeadLock)
                    elementRetCodeRule.Add(Ressource.GetString(Cst.IOReturnCodeEnum.DEADLOCK.ToString()));
                if (isRunTimeOut)
                    elementRetCodeRule.Add(Ressource.GetString(Cst.IOReturnCodeEnum.TIMEOUT.ToString()));
                //
                pElementRetCodeRule = StrFunc.StringArrayList.StringArrayToStringList(elementRetCodeRule.ToArray(), false);
                pElementRetCodeRule = pElementRetCodeRule.Replace(StrFunc.StringArrayList.LIST_SEPARATOR, ',');
            }

            // Nok: Le code retourné par l'élément précédent n'est pas compatible
            return false;
        }

            
        

        /// <summary>
        /// Initialisation des informations nécéssaires à l'envoi d'une notification à l'issu du traitement
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void InitializeTaskNotification()
        {

            if (m_Sql_IOTask.IDSMTPServer == 0)
            {
                // FI 20200623 [XXXXX] Call SetErrorWarning
                Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // Log: Erreur, Serveur SMTP manquant
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6006), 0, new LogParam(m_IOMQueue.identifier)));
            }
            else
            {
                CultureInfo taskCulture = GetNotificationTaskCultureInfo();
                m_TaskNotification = new SMTPNotification(Cs, m_StartDate, m_Sql_IOTask.IDSMTPServer, taskCulture);

                if (IntFunc.IsFilledAndNoZero(m_Sql_IOTask.IDA_SMTP))
                {
                    SQL_Actor sql_SendTo = new SQL_Actor(CSTools.SetCacheOn(Cs), m_Sql_IOTask.IDA_SMTP);
                    if (sql_SendTo.LoadTable(new string[] { "IDA, IDENTIFIER, MAIL" }))
                    {
                        if (StrFunc.IsEmpty(m_Sql_IOTask.AddressIdentDENT_SMTP))
                        {
                            m_TaskNotification.SendTo.Add(new EfsSendSMTPContact(sql_SendTo.Identifier, sql_SendTo.Mail));
                        }
                        else
                        {
                            DataParameters parameters = new DataParameters();
                            parameters.Add(new DataParameter(Cs, "IDA", DbType.Int32), m_Sql_IOTask.IDA_SMTP);
                            parameters.Add(new DataParameter(Cs, "ADDRESSIDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), m_Sql_IOTask.AddressIdentDENT_SMTP);
                            //
                            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
                            sql += "ac.MAIL" + Cst.CrLf;
                            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ADDRESSCOMPL.ToString() + " ac";
                            sql += SQLCst.WHERE + "ac.IDA=@IDA and ac.ADDRESSIDENT=@ADDRESSIDENT" + Cst.CrLf;
                            //
                            using (DataSet ds = DataHelper.ExecuteDataset(Cs, CommandType.Text, sql.ToString(), parameters.GetArrayDbParameter()))
                            {
                                DataTable dtResult = ds.Tables[0];
                                DataRow[] drResult = dtResult.Select();

                                string mail = string.Empty;
                                if (ArrFunc.Count(drResult) > 0)
                                    mail = (Convert.IsDBNull(drResult[0]["MAIL"]) ? null : drResult[0]["MAIL"].ToString());

                                if (StrFunc.IsFilled(mail))
                                    m_TaskNotification.SendTo.Add(new EfsSendSMTPContact(sql_SendTo.Identifier, mail));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Envoyer la notification en utilisant EfsSendSMTP
        /// <para>Dans cette méthode, il ne devrait y avoir aucun accès à la base de données,</para>
        /// <para>car on pourrait vouloir envoyer une notification suite à un problème d'accès à la base de donnes elle même</para>
        /// </summary>
        /// <param name="pRetCode"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void SendTaskNotification(Cst.IOReturnCodeEnum pRetCode)
        {
            try
            {
                #region Ressource pour la notification
                const string Default_Subject = @"Spheres I/O®: Task «{TASKID}» ended with {STATUS}";
                const string Default_Subject_fr = @"Spheres I/O®: Tâche «{TASKID}» terminée avec {STATUS}";
                const string Default_Subject_it = @"Spheres I/O®: Attività «{TASKID}» completata con {STATUS}";

                const string Default_Body_header = @"EXECUTED TASK:  «{TASKID}» executed on {STARTDATE} at {STARTTIME}
DURATION:       {ELAPSEDHOUR} hours, {ELAPSEDMINITE} minites, {ELAPSEDSECOND} seconds
STATUS:         {STATUS}";
                const string Default_Body_header_fr = @"TACHE EXÉCUTÉE: «{TASKID}» exécutée le {STARTDATE} à {STARTTIME}
DURÉE:          {ELAPSEDHOUR} heures, {ELAPSEDMINITE} minutes, {ELAPSEDSECOND} secondes
STATUT:         {STATUS}";
                const string Default_Body_header_it = @"ATTIVITA ESEGUITA: «{TASKID}» eseguita il {STARTDATE} alle {STARTTIME}
DURATA:            {ELAPSEDHOUR} ore, {ELAPSEDMINITE} minuti, {ELAPSEDSECOND} secondi
STATO:             {STATUS}";

                const string Default_Body_Success = @"MESSAGES:       The task ended successfully.(Spheres® log n°{LOGID}).
                The following elements were executed:
{EXECUTEDELEMENT}";
                const string Default_Body_Success_fr = @"MESSAGES:       La tâche s'est terminée avec succès.(Journal Spheres® n°{LOGID}).
                Les éléments suivants ont été exécutés :
{EXECUTEDELEMENT}";
                const string Default_Body_Success_it = @"MESSAGGI:          Attività completata con successo.(Log Spheres® n°{LOGID}).
                   Gli elementi seguenti sono stati eseguiti:
{EXECUTEDELEMENT}";

                const string Default_Body_NotSucces = @"MESSAGES:       Warning, the task did not end correctly. For more détails, Consult Spheres® log n°{LOGID}.
                The following elements were executed:
{EXECUTEDELEMENTWITHSTATUS}";
                const string Default_Body_NotSucces_fr = @"MESSAGES:       Attention, la tâche ne s’est pas terminée correctement. Consultez le journal Spheres® n°{LOGID} pour plus de détails.
                Les éléments suivants ont été exécutés:
{EXECUTEDELEMENTWITHSTATUS}";
                const string Default_Body_NotSucces_it = @"MESSAGGI:          Attenzione, attività completata non correttamente. Per maggiori informazioni, consultare il log Spheres® n°{LOGID}.
                   Gli elementi seguenti sono stati eseguiti:
{EXECUTEDELEMENTWITHSTATUS}";
                #endregion

                // Calculer le code retour de la tâche en fonction du code retour de l'ensemble des éléments composant la tâche
                if (pRetCode == Cst.IOReturnCodeEnum.NA)
                {
                    if (m_TaskNotification.ExecutedElement.Exists(element => element.Second == Cst.IOReturnCodeEnum.ERROR) ||
                        m_TaskNotification.ExecutedElement.Exists(element => element.Second == Cst.IOReturnCodeEnum.TIMEOUT))
                        pRetCode = Cst.IOReturnCodeEnum.ERROR;
                    else if (m_TaskNotification.ExecutedElement.Exists(element => element.Second == Cst.IOReturnCodeEnum.WARNING))
                        pRetCode = Cst.IOReturnCodeEnum.WARNING;
                    else
                        pRetCode = Cst.IOReturnCodeEnum.SUCCESS;
                }

                // Vérifier si le paramètrage de la tâche permet d'envoyer la notification en fonction du code retour de la tâche
                bool isToSend = (m_Sql_IOTask.SendON_SMTP == Cst.StatusTask.ONTERMINATED) ||
                                 ((m_Sql_IOTask.SendON_SMTP == Cst.StatusTask.ONSUCCESS) && (pRetCode == Cst.IOReturnCodeEnum.SUCCESS)) ||
                                 ((m_Sql_IOTask.SendON_SMTP == Cst.StatusTask.ONERROR) && (pRetCode == Cst.IOReturnCodeEnum.ERROR)) ||
                                 ((m_Sql_IOTask.SendON_SMTP == Cst.StatusTask.ONWARNING) && (pRetCode == Cst.IOReturnCodeEnum.WARNING)) ||
                                 ((m_Sql_IOTask.SendON_SMTP == Cst.StatusTask.ONUNSUCCESS) && (pRetCode != Cst.IOReturnCodeEnum.SUCCESS));

                if (isToSend)
                {
                    // Expéditeur
                    string from = m_Sql_IOTask.OriginAdress;
                    if (StrFunc.IsFilled(m_Sql_IOTask.MailAdress))
                        m_TaskNotification.SendTo.Add(new EfsSendSMTPContact(m_Sql_IOTask.MailAdress));

                    // Objet du message
                    string subject = m_Sql_IOTask.Object_SMTP;
                    if (StrFunc.IsEmpty(subject))
                    {
                        switch (m_TaskNotification.Culture.Name)
                        {
                            case "fr-FR":
                                subject = Default_Subject_fr;
                                break;
                            case "it-IT":
                                subject = Default_Subject_it;
                                break;
                            default:
                                subject = Default_Subject;
                                break;
                        }
                    }
                    subject = ReplaceKeyWordsForNotification(subject, pRetCode);

                    // Corps du message
                    string body = string.Empty;
                    switch (m_TaskNotification.Culture.Name)
                    {
                        case "fr-FR":
                            body = Default_Body_header_fr + Cst.CrLf + (pRetCode == Cst.IOReturnCodeEnum.SUCCESS ? Default_Body_Success_fr : Default_Body_NotSucces_fr);
                            break;
                        case "it-IT":
                            body = Default_Body_header_it + Cst.CrLf + (pRetCode == Cst.IOReturnCodeEnum.SUCCESS ? Default_Body_Success_it : Default_Body_NotSucces_it);
                            break;
                        default:
                            body = Default_Body_header + Cst.CrLf + (pRetCode == Cst.IOReturnCodeEnum.SUCCESS ? Default_Body_Success : Default_Body_NotSucces);
                            break;
                    }
                    if (StrFunc.IsFilled(m_Sql_IOTask.Signature_SMTP))
                        body += Cst.CrLf2 + m_Sql_IOTask.Signature_SMTP;

                    body = ReplaceKeyWordsForNotification(body, pRetCode);

                    // Importance
                    EfsSendSMTPPriorityEnum priority = EfsSendSMTPPriorityEnum.Normal;
                    if (pRetCode == Cst.IOReturnCodeEnum.SUCCESS)
                        priority = GetEfsSendSMTPPriority(m_Sql_IOTask.PriorityOnSUCCESS);
                    else
                        priority = GetEfsSendSMTPPriority(m_Sql_IOTask.PriorityOnERROR);

                    // Calculer le temps écoulé entre le début de l'exécution de la tâche et l'heure d'attenite de cette ligne.                    
                    m_TaskNotification.SetElapsedTime(DateTime.Now);

                    // Envoi de la notification en utilisant EfsSendSMTP
                    m_TaskNotification.Send(from, subject, body, priority);

                    
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6029), 0,
                        new LogParam(Ressource.GetString(pRetCode.ToString())),
                        new LogParam(m_TaskNotification.SendSmtp.SmtpClient.Host)));
                }
                else
                {
                    // Log: Message de notification non envoyé pour non adéquation du code retour avec la condition d'envoi
                    
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6030), 0,
                        new LogParam(Ressource.GetString(pRetCode.ToString())),
                        new LogParam(Ressource.GetString(m_Sql_IOTask.SendON_SMTP.ToString()))));
                }
            }
            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] Call SetErrorWarning
                Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                Process.ProcessState.AddCriticalException(ex);

                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));

                string fileName = m_Sql_IOTask.Identifier + "_EfsSendSMTP.xml";
                if (Cst.ErrLevel.SUCCESS == this.AddXmlFileInLog(fileName,
                    typeof(EfsSendSMTP), m_TaskNotification.SendSmtp, null, null))
                {
                    // Ecrire dans le log uniquement si l'insertion du fichier XML dans AttachDoc s'est bien passée
                    
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6031), 0, new LogParam(fileName)));
                }

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6008), 0,
                    new LogParam(m_TaskNotification.SendSmtp.SmtpClient.Host),
                    new LogParam(m_IOMQueue.identifier)));

                //
                //Génération d'un exception pour stopper le traitement
                //Cette exception ne contient pas l'exception source puisque l'exeption source est déjà insérée dans le log
                throw new Exception("Error on sending email", ex);
            }
        }

        /// <summary>
        /// Remplace une chaine d'entrée {pStringWithKeyWords} avec les keywords autorisés. 
        /// </summary>
        /// <param name="pStringWithKeyWords"></param>
        /// <param name="pRetCode"></param>
        /// <returns></returns>
        public string ReplaceKeyWordsForNotification(string pStringWithKeyWords, Cst.IOReturnCodeEnum pRetCode)
        {
            if (pStringWithKeyWords.Contains("{TASKID}"))
                pStringWithKeyWords = pStringWithKeyWords.Replace("{TASKID}", m_Sql_IOTask.Identifier);

            if (pStringWithKeyWords.Contains("{TASKDN}"))
                pStringWithKeyWords = pStringWithKeyWords.Replace("{TASKDN}", m_Sql_IOTask.DisplayName);

            if (pStringWithKeyWords.Contains("{STATUS}"))
                pStringWithKeyWords = pStringWithKeyWords.Replace("{STATUS}", Ressource.GetString(pRetCode.ToString(), pRetCode.ToString(), m_TaskNotification.Culture));

            // Remplacer {LOGURL} avant {LOGID}, car {LOGURL} contient {LOGID}
            if (pStringWithKeyWords.Contains("{LOGURL}"))
                pStringWithKeyWords = pStringWithKeyWords.Replace("{LOGURL}", "List.aspx?Log=PROCESSDET_L&InputMode=5&IDMenu=OTC_VIEW_LOGPROCESS_DET&FK={LOGID}");

            if (pStringWithKeyWords.Contains("{LOGID}"))
            {
                
                //    pStringWithKeyWords = pStringWithKeyWords.Replace("{LOGID}", process.processLog.header.IdProcess.ToString());
                pStringWithKeyWords = pStringWithKeyWords.Replace("{LOGID}", Process.IdProcess.ToString());
            }
            return pStringWithKeyWords;
        }

        /// <summary>
        /// Retourne la culture de notification à utiliser pour la tâche
        /// <para>- Utiliser la langue de notification de l'acteur propriétaire de la tâche, si  elle est définie</para>
        /// <para>- Sinon, utiliser la langue de l'acteur propriétaire de la tâche, si  elle est définie</para>
        /// <para>- Utiliser l'anglais (en-GB)</para>
        /// </summary>
        public CultureInfo GetNotificationTaskCultureInfo()
        {
            string taskCulture = string.Empty;

            SQL_Actor sqlActor = new SQL_Actor(CSTools.SetCacheOn(Cs), m_Sql_IOTask.IDA);
            sqlActor.LoadTable(new string[] { "CULTURE", "CULTURE_CNF" });
            if ((sqlActor.IsLoaded))
            {
                if (StrFunc.IsFilled(sqlActor.Culture_Cnf))
                    taskCulture = sqlActor.Culture_Cnf;
                else if (StrFunc.IsFilled(sqlActor.Culture))
                    taskCulture = sqlActor.Culture;
            }

            if (StrFunc.IsEmpty(taskCulture))
                taskCulture = "en-GB";

            return CultureInfo.CreateSpecificCulture(taskCulture);
        }

        /// <summary>
        /// Retourne la priorité d'envoi du message
        /// </summary>
        /// <param name="pStatusPriority"></param>
        /// <returns></returns>
        public EfsSendSMTPPriorityEnum GetEfsSendSMTPPriority(Cst.StatusPriority pStatusPriority)
        {
            switch (pStatusPriority)
            {
                case Cst.StatusPriority.HIGH:
                    return EfsSendSMTPPriorityEnum.High;
                case Cst.StatusPriority.REGULAR:
                    return EfsSendSMTPPriorityEnum.Normal;
                case Cst.StatusPriority.LOW:
                    return EfsSendSMTPPriorityEnum.Low;
            }
            return EfsSendSMTPPriorityEnum.High;
        }

        /// <summary>
        ///  Ajoute le fichier postParsing ou le fichier postMapping dans ATTACHED DOC 
        ///  Génère le fichier dans le répertoire temporaire
        /// </summary>
        /// <param name="pKeyFile"></param>
        public Cst.ErrLevel AddXmlFileInLog(string pFileName, Type pType, object pDocument, XmlDocument pDomXml)
        {
            return AddXmlFileInLog(pFileName, pType, pDocument, pDomXml, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileName"></param>
        /// <param name="pType"></param>
        /// <param name="pDocument"></param>
        /// <param name="pDomXml"></param>
        /// <param name="pXmlSerializer"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel AddXmlFileInLog(string pFileName, Type pType, object pDocument, XmlDocument pDomXml, XmlSerializer pXmlSerializer)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                string fileNameFull = m_process.Session.MapTemporaryPath(pFileName, AppSession.AddFolderSessionId.True);
                SaveInTemporaryFile(fileNameFull, pType, pDocument, pDomXml, pXmlSerializer);
                AddLogAttachedDoc(pFileName, fileNameFull);
            }
            // RD 20120314 
            // Pour ne pas arrêter le traitement, on écrit l'exception tout de suite et on continu
            catch (Exception ex)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.None, ex.Message));

                ret = Cst.ErrLevel.FAILURE;
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileNameFull"></param>
        /// <param name="pType"></param>
        /// <param name="pDocument"></param>
        /// <param name="pDomXml"></param>
        /// <param name="pXmlSerializer"></param>
        public void SaveInTemporaryFile(string pFileNameFull, Type pType, object pDocument, XmlDocument pDomXml, XmlSerializer pXmlSerializer)
        {
            try
            {
                if (null == pDomXml)
                {
                    if (null == pXmlSerializer)
                        IOTools.XmlSerialize(pType, pDocument, pFileNameFull);
                    else
                        IOTools.XmlSerialize(pXmlSerializer, pDocument, pFileNameFull);
                }
                else
                    pDomXml.Save(pFileNameFull);
            }
            catch (Exception ex)
            {
                string logMessage;
                // RD 20120314 
                // Ecrire un message de Log clair, notamment s'il s'agit d'une erreur OutOfMemory
                if (ex.Message.Contains("OutOfMemory"))
                    logMessage = "The XML file was not written in the Temporary directory, because of its big volume" + Cst.CrLf + "[File:" + pFileNameFull + "]";
                else
                    logMessage = "The XML file was not written in the Temporary directory" + Cst.CrLf + "[File:" + pFileNameFull + "]";

                throw new Exception(logMessage, ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileName"></param>
        /// <param name="pFileNameFull"></param>
        // AL 20240709 [WI999] Added parameter docType (default is xml)
        public void AddLogAttachedDoc(string pFileName, string pFileNameFull, string pDocType = "xml")
        {
            try
            {
                byte[] data = FileTools.ReadFileToBytes(pFileNameFull);
                LogTools.AddAttachedDoc(this.Cs, this.Process.IdProcess, this.Process.Session.IdA, data, pFileName, pDocType);
            }
            catch (Exception ex)
            {
                string logMessage;
                if (ex.Message.Contains("OutOfMemory"))
                    logMessage = "The XML file was not written in the attachments of the Log, because of its big volume" + Cst.CrLf + "[File:" + pFileNameFull + "]";
                else
                    logMessage = "The XML file was not written in the attachments of the Log" + Cst.CrLf + "[File:" + pFileNameFull + "]";

                throw new Exception(logMessage, ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileName"></param>
        /// <param name="pDomXml"></param>
        public void AddLogAttachedDoc(string pFileName, XmlDocument pDomXml)
        {
            try
            {
                byte[] data = IOTools.ReadDocToBytes(pDomXml);
                LogTools.AddAttachedDoc(this.Cs, this.Process.IdProcess, this.Process.Session.IdA, data, pFileName, "xml");
            }
            catch (Exception ex)
            {
                string logMessage;
                if (ex.Message.Contains("OutOfMemory"))
                    logMessage = "The XML file was not written in the attachments of the Log, because of its big volume";
                else
                    logMessage = "The XML file was not written in the attachments of the Log" + Cst.CrLf2 + ex.Message;

                throw new Exception(logMessage, ex);
            }
        }

        #region AddIOTrackLog
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMessage"></param>
        /// <param name="pIdDataSource"></param>
        /// <param name="pDataSourceIdent"></param>
        /// <param name="pData"></param>
        /// <param name="pDataIdent"></param>
        /// <param name="pStatusReturn"></param>
        public void AddIOTrackLog(string pMessage,
            Nullable<int> pIdDataSource, string pDataSourceIdent,
            string[] pData, string[] pDataIdent,
            string pStatusReturn)
        {
            AddIOTrackLog(pMessage,
                pIdDataSource, pDataSourceIdent,
                pData, pDataIdent,
                pStatusReturn,
                null, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMessage"></param>
        /// <param name="pIdDataSource"></param>
        /// <param name="pDataSourceIdent"></param>
        /// <param name="pData"></param>
        /// <param name="pDataIdent"></param>
        /// <param name="pStatusReturn"></param>
        /// <param name="pDescriptionReturn"></param>
        public void AddIOTrackLog(string pMessage,
            Nullable<int> pIdDataSource, string pDataSourceIdent,
            string[] pData, string[] pDataIdent,
            string pStatusReturn,
            string pDescriptionReturn)
        {
            AddIOTrackLog(pMessage,
                pIdDataSource, pDataSourceIdent,
                pData, pDataIdent,
                pStatusReturn,
                null, null,
                pDescriptionReturn);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMessage"></param>
        /// <param name="pIdDataSource"></param>
        /// <param name="pDataSourceIdent"></param>
        /// <param name="pData"></param>
        /// <param name="pDataIdent"></param>
        /// <param name="pStatusReturn"></param>
        /// <param name="pIdDataTarget"></param>
        /// <param name="pDataTargetIdent"></param>
        public void AddIOTrackLog(string pMessage,
            Nullable<int> pIdDataSource, string pDataSourceIdent,
            string[] pData, string[] pDataIdent,
            string pStatusReturn,
            Nullable<int> pIdDataTarget, string pDataTargetIdent)
        {
            AddIOTrackLog(pMessage,
                pIdDataSource, pDataSourceIdent,
                pData, pDataIdent,
                pStatusReturn,
                pIdDataTarget, pDataTargetIdent,
                null, null, null, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMessage"></param>
        /// <param name="pIdDataSource"></param>
        /// <param name="pDataSourceIdent"></param>
        /// <param name="pData"></param>
        /// <param name="pDataIdent"></param>
        /// <param name="pStatusReturn"></param>
        /// <param name="pIdDataTarget"></param>
        /// <param name="pDataTargetIdent"></param>
        /// <param name="pDescriptionReturn"></param>
        public void AddIOTrackLog(string pMessage,
            Nullable<int> pIdDataSource, string pDataSourceIdent,
            string[] pData, string[] pDataIdent,
            string pStatusReturn,
            Nullable<int> pIdDataTarget, string pDataTargetIdent,
            string pDescriptionReturn)
        {
            AddIOTrackLog(pMessage,
                pIdDataSource, pDataSourceIdent,
                pData, pDataIdent, pStatusReturn,
                pIdDataTarget, pDataTargetIdent,
                pDescriptionReturn, null);
        }

        /// <summary>
        /// Add a row element to the table IOTRACK
        /// </summary>
        /// <param name="pMessage">value feeding the IOTRACK.MESSAGE column, may be NULL</param>
        /// <param name="pIdDataSource">value feeding the IOTRACK.IDDATASOURCE column, mya be NULL</param>
        /// <param name="pDataSourceIdent">value feeding the IOTRACK.DATASOURCEIDENT column, may be NULL</param>
        /// <param name="pData">value feeding the IOTRACK.DATAx (DATA1, DATA2, .., DATA5) columns, may be NULL</param>
        /// <param name="pDataIdent">value feeding the IOTRACK.DATAIDENTx (DATA1IDENT, DATA2IDENT, .., DATA5IDENT) columns, may be NULL</param>
        /// <param name="pStatusReturn">value feeding the IOTRACK.STATUSRETURN column, should NOT be NULL</param>
        /// <param name="pIdDataTarget">value feeding the IOTRACK.IDDATATARGET column, may be NULL</param>
        /// <param name="pDataTargetIdent">value feeding the IOTRACK.DATATARGETIDENT column, may be NULL</param>
        /// <param name="pDescriptionReturn">value feeding the IOTRACK.DESCRIPTIONRETURN column, may be NULL</param>
        /// <param name="pLOTxt">description des tables SQL destinataires</param>
        public void AddIOTrackLog(string pMessage,
            Nullable<int> pIdDataSource, string pDataSourceIdent,
            string[] pData, string[] pDataIdent,
            string pStatusReturn,
            Nullable<int> pIdDataTarget, string pDataTargetIdent,
            string pDescriptionReturn, string pLOTxt)
        {
            AddIOTrackLog(pMessage,
                pIdDataSource, pDataSourceIdent,
                pData, pDataIdent,
                pStatusReturn,
                pIdDataTarget, pDataTargetIdent,
                null, pDescriptionReturn, null, null, null, null,
                pLOTxt);
        }

        /// <summary>
        /// Add a row element to the table IOTRACK
        /// </summary>
        /// <param name="pMessage">value feeding the IOTRACK.MESSAGE column, may be NULL</param>
        /// <param name="pData">value feeding the IOTRACK.DATAx (DATA1, DATA2, .., DATA5) columns, may be NULL</param>
        /// <param name="pDataIdent">value feeding the IOTRACK.DATAIDENTx (DATA1IDENT, DATA2IDENT, .., DATA5IDENT) columns, may be NULL</param>
        /// <param name="pStatusReturn">value feeding the IOTRACK.STATUSRETURN column, should NOT be NULL</param>
        /// <param name="pDateTime">value feeding the IOTRACK.DTDATA column, may be NULL</param>
        /// <param name="pDateTimeIdentifier">value feeding the IOTRACK.DTDATAIDENT column, may be NULL</param>
        /// <param name="pElementSubType">value feeding the IOTRACK.ELEMENTSUBTYPE column, may be NULL</param>
        public void AddIOTrackLog(
            string pMessage,
            string[] pData,
            string[] pDataIdent,
            string pStatusReturn,
            DateTime pDateTime, string pDateTimeIdentifier,
            string pElementSubType)
        {
            AddIOTrackLog(pMessage, null, null, pData, pDataIdent, pStatusReturn,
                null, null, null, null, null, pDateTime, pDateTimeIdentifier, pElementSubType);
        }

        public void AddIOTrackLog(string pMessage,
            Nullable<int> pIdDataSource, string pDataSourceIdent,
            string[] pData, string[] pDataIdent,
            string pStatusReturn,
            Nullable<int> pIdDataTarget, string pDataTargetIdent,
            string pCodeReturn, string pDescriptionReturn,
            string pUrl,
            Nullable<DateTime> pDateTime, string pDateTimeIdentifier)
        {
            AddIOTrackLog(pMessage,
                pIdDataSource, pDataSourceIdent,
                pData, pDataIdent,
                pStatusReturn,
                pIdDataTarget, pDataTargetIdent,
                pCodeReturn, pDescriptionReturn,
                pUrl,
                pDateTime, pDateTimeIdentifier,
                null, null);
        }

        public void AddIOTrackLog(
            string pMessage,
            Nullable<int> pIdDataSource, string pDataSourceIdent,
            string[] pData, string[] pDataIdent,
            string pStatusReturn,
            Nullable<int> pIdDataTarget, string pDataTargetIdent,
            string pCodeReturn, string pDescriptionReturn,
            string pUrl,
            Nullable<DateTime> pDateTime, string pDateTimeIdentifier,
            string pElementSubType)
        {
            AddIOTrackLog(pMessage,
                pIdDataSource, pDataSourceIdent,
                pData, pDataIdent,
                pStatusReturn,
                pIdDataTarget, pDataTargetIdent,
                pCodeReturn, pDescriptionReturn,
                pUrl,
                pDateTime, pDateTimeIdentifier,
                // 20120911 Ticket 18117 - IOTRACK.ELEMENTSUBTYPE column was not inserted anymore
                //null, null);
                pElementSubType, null);
        }

        /// <summary>
        /// Add a row element to the table IOTRACK
        /// </summary>
        /// <param name="pMessage">value feeding the IOTRACK.MESSAGE column, may be NULL</param>
        /// <param name="pIdDataSource">value feeding the IOTRACK.IDDATASOURCE column, mya be NULL</param>
        /// <param name="pDataSourceIdent">value feeding the IOTRACK.DATASOURCEIDENT column, may be NULL</param>
        /// <param name="pData">value feeding the IOTRACK.DATAx (DATA1, DATA2, .., DATA5) columns, may be NULL</param>
        /// <param name="pDataIdent">value feeding the IOTRACK.DATAIDENTx (DATA1IDENT, DATA2IDENT, .., DATA5IDENT) columns, may be NULL</param>
        /// <param name="pStatusReturn">value feeding the IOTRACK.STATUSRETURN column, should NOT be NULL</param>
        /// <param name="pIdDataTarget">value feeding the IOTRACK.IDDATATARGET column, may be NULL</param>
        /// <param name="pDataTargetIdent">value feeding the IOTRACK.DATATARGETIDENT column, may be NULL</param>
        /// <param name="pCodeReturn">value feeding the IOTRACK.CODERETURN column, may be NULL</param>
        /// <param name="pDescriptionReturn">value feeding the IOTRACK.DESCRIPTIONRETURN column, may be NULL</param>
        /// <param name="pUrl">value feeding the IOTRACK.URL column, may be NULL</param>
        /// <param name="pDateTime">value feeding the IOTRACK.DTDATA column, may be NULL</param>
        /// <param name="pDateTimeIdentifier">value feeding the IOTRACK.DTDATAIDENT column, may be NULL</param>
        /// <param name="pElementSubType">value feeding the IOTRACK.ELEMENTSUBTYPE column, may be NULL</param>
        /// <param name="pLOTxt">description des tables SQL destinataires</param>
        public void AddIOTrackLog(
            string pMessage,
            Nullable<int> pIdDataSource, string pDataSourceIdent,
            string[] pData, string[] pDataIdent,
            string pStatusReturn,
            Nullable<int> pIdDataTarget, string pDataTargetIdent,
            string pCodeReturn, string pDescriptionReturn,
            string pUrl,
            Nullable<DateTime> pDateTime, string pDateTimeIdentifier,
            string pElementSubType,
            string pLOTxt)
        {
            DataParameters parameters;

            #region Query Insert
            string SQLInsert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.IOTRACK + Cst.CrLf;
            SQLInsert += @"(IDPROCESS_L, IDIOTASKDET, MESSAGE, IDDATASOURCE, DATASOURCEIDENT, IDDATATARGET, DATATARGETIDENT, 
                                DATA1, DATA2, DATA3, DATA4, DATA5, DATA1IDENT, DATA2IDENT, DATA3IDENT, DATA4IDENT, DATA5IDENT,
                                LOTXT, LOBIN, STATUSRETURN, CODERETURN, DESCRIPTIONRETURN, URL,
                                DTUPD, IDAUPD, IDAINS, DTINS, EXTLLINK, DTDATA, DTDATAIDENT, ELEMENTSUBTYPE)
                          values
                               (@IDPROCESS_L, @IDIOTASKDET, @MESSAGE, @IDDATASOURCE, @DATASOURCEIDENT, @IDDATATARGET, @DATATARGETIDENT, 
                                @DATA1, @DATA2, @DATA3, @DATA4, @DATA5, @DATA1IDENT, @DATA2IDENT, @DATA3IDENT, @DATA4IDENT, @DATA5IDENT,
                                @LOTXT, null, @STATUSRETURN, @CODERETURN, @DESCRIPTIONRETURN, @URL,
                                @DTUPD, @IDAUPD,@IDAINS, @DTINS, @EXTLLINK, @DTDATA, @DTDATAIDENT, @ELEMENTSUBTYPE);";
            #endregion Query Insert

            #region Parameters setting
            parameters = new DataParameters();
            
            //parameters.Add(new DataParameter(Cs, "IDPROCESS_L", DbType.Int32), process.processLog.header.IdProcess);
            parameters.Add(new DataParameter(Cs, "IDPROCESS_L", DbType.Int32), Process.IdProcess);
            parameters.Add(new DataParameter(Cs, "IDIOTASKDET", DbType.Int32), m_IOTask.taskDet.id);
            //
            parameters.Add(new DataParameter(Cs, "MESSAGE", DbType.AnsiString, SQLCst.UT_MESSAGE_LEN), pMessage);
            parameters.Add(new DataParameter(Cs, "IDDATASOURCE", DbType.Int32), (pIdDataSource >= 0 ? pIdDataSource : Convert.DBNull));
            parameters.Add(new DataParameter(Cs, "DATASOURCEIDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pDataSourceIdent);
            parameters.Add(new DataParameter(Cs, "IDDATATARGET", DbType.Int32), (pIdDataTarget >= 0 ? pIdDataTarget : Convert.DBNull));
            parameters.Add(new DataParameter(Cs, "DATATARGETIDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pDataTargetIdent);
            //                    
            for (int i = 0; i < ArrFunc.Count(pData); i++)
            {
                parameters.Add(new DataParameter(Cs, "DATA" + (i + 1).ToString(), DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pData[i]);
                parameters.Add(new DataParameter(Cs, "DATA" + (i + 1).ToString() + "IDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pDataIdent[i]);
            }
            //
            for (int i = ArrFunc.Count(pData); i < 5; i++)
            {
                parameters.Add(new DataParameter(Cs, "DATA" + (i + 1).ToString(), DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(Cs, "DATA" + (i + 1).ToString() + "IDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
            }
            //
            parameters.Add(new DataParameter(Cs, "STATUSRETURN", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pStatusReturn);
            parameters.Add(new DataParameter(Cs, "CODERETURN", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pCodeReturn);
            parameters.Add(new DataParameter(Cs, "DESCRIPTIONRETURN", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), pDescriptionReturn);
            parameters.Add(new DataParameter(Cs, "URL", DbType.AnsiString, SQLCst.UT_UNC_LEN), pUrl);
            //
            parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDAUPD), Convert.DBNull);
            parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTUPD), Convert.DBNull);
            parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDAINS), Process.UserId);
            // FI 20200820 [25468] Dates systemes en UTC
            parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTINS), OTCmlHelper.GetDateSysUTC(Cs));
            parameters.Add(new DataParameter(Cs, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), Convert.DBNull);
            parameters.Add(new DataParameter(Cs, "DTDATA", DbType.DateTime), pDateTime);
            parameters.Add(new DataParameter(Cs, "DTDATAIDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pDateTimeIdentifier);
            parameters.Add(new DataParameter(Cs, "ELEMENTSUBTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pElementSubType);
            parameters.Add(new DataParameter(Cs, "LOTXT", DbType.AnsiString), pLOTxt);
            #endregion Parameters setting

            _ = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, SQLInsert, parameters.GetArrayDbParameter());
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200706 [XXXXX] Add
        private void CheckElement()
        {
            int elementCount = ArrFunc.Count(Element);
            if (elementCount == 0)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6007), 0, new LogParam(m_IOMQueue.identifier)));
                throw new Exception("Task without element");
            }

            foreach (DataRow elementRow in Element)
            {
                string elementId = elementRow["IDIOELEMENT"].ToString();
                int elementTaskDetId = Convert.ToInt32(elementRow["IDIOTASKDET"]);
                string elementLog = LogTools.IdentifierAndId(elementId, elementTaskDetId);

                string retCodeOnNoData = elementRow["RETCODEONNODATA"].ToString();
                if (false == Enum.IsDefined(typeof(Cst.IOReturnCodeEnum), retCodeOnNoData))
                    throw new Exception($"Element:{elementLog}. Return code on no data: {retCodeOnNoData} is not managed");

                string retCodeOnNoDataModif = elementRow["RETCODEONNODATAM"].ToString();
                if (false == Enum.IsDefined(typeof(Cst.IOReturnCodeEnum), retCodeOnNoDataModif))
                    throw new Exception($"Element:{elementLog}. Return code on no data modification: {retCodeOnNoDataModif} is not managed");

                string ruleOnError = (Convert.IsDBNull(elementRow["RULEONERROR"]) ? string.Empty : elementRow["RULEONERROR"].ToString());
                if (StrFunc.IsFilled(ruleOnError))
                {
                    if (false == Enum.IsDefined(typeof(Cst.RuleOnError), ruleOnError))
                        throw new Exception($"Element:{elementLog}. Rule in case of anomaly: {ruleOnError} is not managed");
                }

            }
        }
        #endregion Methods
    }
}

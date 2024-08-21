#region Using Directives
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion Using Directives

namespace EFS.Process.EventsGen
{
    public static partial class New_EventsGenAPI
    {
        #region GENERATION DES EVENEMENTS EN MODE ASYNCHRONE
        #region CreateEventsAsync
        /// <summary>
        /// Traitement de génération des événements en mode ASYNCHRONE
        /// </summary>
        /// <param name="pCallProcess">Process appelant</param>
        /// <param name="pTrades">Enumerateur des trades : Pair(idT,identifier)</param>
        /// <param name="pGroupProduct">Groupe de produits</param>
        /// <returns></returns>
        // EG 20180205 [23769] New  
        // EG 20180326 [23769] Test trade.First != null sur getReturnTasksQuery
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20210706 [XXXXX] Test (null != trade) sur construction de getReturnTaskQuery
        public static async Task<Cst.ErrLevel> CreateEventsAsync(ProcessBase pCallProcess, IEnumerable<Pair<int, string>> pTrades, 
            ProductTools.GroupProductEnum pGroupProduct)
        {
            AppInstance.TraceManager.TraceInformation(pCallProcess, "START CreateEventsAsync");

            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            bool isAddLog = (1 < pCallProcess.SemaphoreAsync.CurrentCount) || (1 < pTrades.Count());
            if (isAddLog)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4061), 2));
                Logger.Write();
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            List<Task<ProcessState>> getReturnTasks = null;
            try
            {
                IEnumerable<Task<ProcessState>> getReturnTasksQuery =
                    from trade in pTrades 
                    where (null != trade) && (trade.First > 0)
                    select CreateEventAsync(pCallProcess, trade, pGroupProduct, cts.Token);

                getReturnTasks = getReturnTasksQuery.ToList();

                // Boucle sur l'ensemble des tâches du Pool
                // On s'arrête à la 1ère tâche terminée pour :
                // 1. La supprimer de la liste
                // 2. Récupérer sa valeur de retour
                while (0 < getReturnTasks.Count)
                {
                    // On s'arrête sur la première tâche complétée.  
                    Task<ProcessState> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    // On supprime la tâche de la liste des tâches pour éviter que le process ne la traite une nouvelle fois.  
                    getReturnTasks.Remove(firstFinishedTask);
                    // EG 20230927 [26506] Ajout du message d'erreur dans la trace
                    //if (firstFinishedTask.IsFaulted)
                    //    // FI 20200817 [XXXXX] usage de firstFinishedTask.Exception.Flatten() pour faire comme partout ailleurs dans le produit
                    //    //throw new AggregateException(MethodInfo.GetCurrentMethod().Name, firstFinishedTask.Exception);
                    //    throw firstFinishedTask.Exception.Flatten();
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(pCallProcess, firstFinishedTask, "CreateEventsAsync", null, true);
                }
                if (isAddLog)
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4060), 2));
                    Logger.Write();
                }
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw ex;
            }
            finally
            {
                AppInstance.TraceManager.TraceInformation(pCallProcess, "STOP CreateEventsAsync");
                AppInstance.TraceManager.TraceTimeSummary("SlaveCallEventsGenAsync"); 
            }
            return ret;
        }
        #endregion CreateEventsAsync
        #region CreateEventAsync
        /// <summary>
        /// Génération des événements d'un trade (RISK, TRADE de MARCHE, INVOICE)
        /// </summary>
        /// <param name="pCallProcess">Process appelant</param>
        /// <param name="pTrades">Trade : Pair(idT,identifier)</param>
        /// <param name="pGroupProduct">Groupe de produits</param>
        /// <param name="pCt">Token pour notification d'annulation</param>
        /// <returns></returns>
        // EG 20180205 [23769] New 
        // EG 20180525 [23979] IRQ Processing
        private static async Task<ProcessState> CreateEventAsync(ProcessBase pCallProcess, Pair<int, string> pTrade,
            ProductTools.GroupProductEnum pGroupProduct, CancellationToken pCt)
        {

            string key = String.Format("(Trade: {0}({1}))", pTrade.Second, pTrade.First);
            string wait = "START CreateEventAsync Wait   : {0} " + key;
            string release = "STOP  CreateEventAsync Release: {0} " + key;

            bool isSemaphoreSpecified = (null != pCallProcess.SemaphoreAsync);

            await Task.Run(() =>
            {
                try
                {
                    AppInstance.TraceManager.TraceTimeBegin("SlaveCallEventsGenAsync", key);
                    if (isSemaphoreSpecified)
                    {
                        pCallProcess.SemaphoreAsync.Wait();
                        AppInstance.TraceManager.TraceVerbose(pCallProcess, String.Format(wait, pCallProcess.SemaphoreAsync.CurrentCount));
                    }
                    EventsGenCalculation(pCallProcess, pTrade.First, pTrade.Second,pGroupProduct);
                }
                catch (Exception) { throw; }
                finally
                {
                    if (isSemaphoreSpecified)
                    {
                        pCallProcess.SemaphoreAsync.Release();
                        AppInstance.TraceManager.TraceVerbose(pCallProcess, String.Format(release, pCallProcess.SemaphoreAsync.CurrentCount));
                    }
                    AppInstance.TraceManager.TraceTimeEnd("SlaveCallEventsGenAsync", key);
                }
            }, pCt);
            return pCallProcess.ProcessState;
        }
        #endregion CreateEventAsync
        #region EventsGenCalculation
        /// <summary>
        /// Calcul et génération des événements appelé mode ASYNCHRONE via SLAVE CALL SpheresEventsGen service
        /// </summary>
        /// <param name="pCallProcess">Process appelant</param>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pIdentifier">Identifier du trade</param>
        /// <param name="pXslFileNameMain">Fichier maitre XSLT pour les événements</param>
        /// <param name="pGProduct">Groupe de produit</param>
        /// <param name="pDetailLog">Niveau de log pour les messages de sorties</param>
        /// <returns></returns>
        // EG 20180205 [23769] New 
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Use DbTransaction
        public static ProcessState EventsGenCalculation(ProcessBase pCallProcess, int pIdT, string pIdentifier, ProductTools.GroupProductEnum pGProduct)
        {
            return EventsGenCalculation(pCallProcess, null, pIdT, pIdentifier,  pGProduct);
        }
        public static ProcessState EventsGenCalculation(ProcessBase pCallProcess, IDbTransaction pDbTransaction, int pIdT, string pIdentifier,
            ProductTools.GroupProductEnum pGProduct)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusUnknownEnum, Cst.ErrLevel.SUCCESS);
            try
            {
                if (0 < pIdT)
                {
                    string gProduct = ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(pGProduct);

                    AppInstance.TraceManager.TraceVerbose(pCallProcess, String.Format("START EventsGenCalculation {0}({1})", pIdentifier, pIdT));

                    IdInfo idInfo = new IdInfo()
                    {
                        id = pIdT,
                        idInfos = new DictionaryEntry[3]{
                                                new DictionaryEntry("ident", "TRADE"),
                                                new DictionaryEntry("identifier", pIdentifier),
                                                new DictionaryEntry("GPRODUCT", gProduct)}
                    };
                    MQueueAttributes mQueueAttributes = new MQueueAttributes()
                    {
                        connectionString = pCallProcess.Cs,
                        id = idInfo.id,
                        identifier = pIdentifier,
                        idInfo = idInfo,
                        requester = pCallProcess.MQueue.header.requester
                    };

                    EventsGenMQueue _queue = new EventsGenMQueue(mQueueAttributes);
                    processState = ExecuteSlaveCall(_queue, pDbTransaction, pCallProcess, false);
                }
            }
            catch (Exception ex)
            {
                SpheresException2 otcmlEx = SpheresExceptionParser.GetSpheresException(ex.GetType().ToString(), ex);

                // FI 20200623 [XXXXX] AddCriticalException
                pCallProcess.ProcessState.AddCriticalException(otcmlEx);
                
                
                
                Logger.Log(new LoggerData(otcmlEx));
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 1018), 2));
                Logger.Write();

                pCallProcess.ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;
            }
            AppInstance.TraceManager.TraceVerbose(pCallProcess, String.Format("FINISHED EventsGenCalculation {0}({1})", pIdentifier, pIdT));
            return processState;
        }
        #endregion EventsGenCalculation

        #region CreateEventsGenMQueue
        // EG 20190613 [24683] New
        public static EventsGenMQueue CreateEventsGenMQueue(ProcessBase pCallProcess, int pIdT, string pIdentifier, ProductTools.GroupProductEnum pGProduct)
        {
            string gProduct = ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(pGProduct);
            IdInfo idInfo = new IdInfo()
            {
                id = pIdT,
                idInfos = new DictionaryEntry[]{
                                        new DictionaryEntry("ident", "TRADE"),
                                        new DictionaryEntry("identifier", pIdentifier),
                                        new DictionaryEntry("GPRODUCT", gProduct)}
            };

            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = pCallProcess.Cs,
                id = idInfo.id,
                identifier = pIdentifier,
                idInfo = idInfo,
                requester = pCallProcess.MQueue.header.requester
            };
            return new EventsGenMQueue(mQueueAttributes);
        }
        #endregion CreateEventsGenMQueue
        #endregion GENERATION DES EVENEMENTS EN MODE ASYNCHRONE
    }
}

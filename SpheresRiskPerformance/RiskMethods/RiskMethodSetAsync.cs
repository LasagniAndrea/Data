using EFS.ACommon;
using EFS.Common;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.Hierarchies;
using FpML.v44.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Extension de la classe RiskMethodSet pour la gestion ASYNCHRONE
    /// </summary>
    public sealed partial class RiskMethodSet
    {
        private bool m_IsCalculationAsync;

        // Utilisé pour limiter le nombre de threads dans le calcul du déposit 
        private static SemaphoreSlim m_SemaphoreDeposit;


        /// <summary>
        /// Calcul des déposits (Net Margining)
        /// ASYNCHRONE MODE
        /// </summary>
        /// <param name="pProcessBase">Process appelant</param>
        /// <returns></returns>
        // EG 20180418 [23769] New
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void EvaluateNetMarginingThreading(RiskCommonProcessBase pProcessBase, CalculationSheet.CalculationSheetRepository pLogsRepository)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IEnumerable<KeyValuePair<Pair<int, int>, Deposit>> netMarginingDeposits =
                Deposits.Where(deposit => false == deposit.Value.IsGrossMargining);


            m_IsCalculationAsync = pProcessBase.IsParallelProcess(ParallelProcess.InitialMarginCalculation);
            // Initialisation de la sémaphore
            m_SemaphoreDeposit = pProcessBase.SemaphoreAsync;

            // Découpage du traitement de calcul des déposits par tronçon de n éléments (.config)
            int heapSize = pProcessBase.GetHeapSize(ParallelProcess.InitialMarginCalculation);
            List<List<KeyValuePair<Pair<int, int>, Deposit>>> subLstNetMarginingDeposits = ListExtensionsTools.ChunkBy(netMarginingDeposits.ToList(), heapSize);

            int counter = 1;
            int totSubList = subLstNetMarginingDeposits.Count();
            subLstNetMarginingDeposits.ForEach(lstNetMarginingDeposits =>
            {
                if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                    (false == IRQTools.IsIRQRequested(pProcessBase, pProcessBase.IRQNamedSystemSemaphore, ref codeReturn)))
                {
                    if ((1 < heapSize) || (1 == totSubList))
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.Info, String.Format("Parallel Evaluate Net Margining {0}/{1}", counter, totSubList), 2));
                        Logger.Write();
                    }
                    EvaluateNetMarginingAsync(pProcessBase, lstNetMarginingDeposits).Wait();
                    counter++;
                }
            });

            if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                pProcessBase.ProcessState.CodeReturn = codeReturn;
        }

        /// <summary>
        /// Calcul des déposits (Net Margining)
        /// ASYNCHRONE MODE
        /// </summary>
        /// <param name="pProcessBase">Process appelant</param>
        /// <returns></returns>
        // EG 20180418 [23769] New
        public async Task EvaluateNetMarginingAsync(RiskCommonProcessBase pProcessBase , List<KeyValuePair<Pair<int, int>, Deposit>> pLstNetMarginingDeposits)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            // Construction de la liste des tâches de calcul des déposits (Net margining)
            IEnumerable<Task<DepositStatus>> getReturnTasksQuery =
                from netMarginingDeposit in pLstNetMarginingDeposits
                select EvaluateDepositNetMarginingAsync(netMarginingDeposit.Key.First, netMarginingDeposit.Key.Second, netMarginingDeposit.Value, cts.Token);

            m_IsCalculationAsync = pProcessBase.IsParallelProcess(ParallelProcess.InitialMarginCalculation);
            // Initialisation de la sémaphore
            m_SemaphoreDeposit = pProcessBase.SemaphoreAsync;

            // Mise en attente des tâches de calcul des déposits (Net margining)
            List<Task<DepositStatus>> getReturnTasks = getReturnTasksQuery.ToList();
            try
            {
                // Boucle de traitement des tâches asynchrones de la liste "getReturnTasks" : 
                // après chaque tâche terminée, contrôle si Erreur = Cancellation des autres tâches mise en attente (cts.Cancel())
                while (0 < getReturnTasks.Count)
                {
                    Task<DepositStatus> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    getReturnTasks.Remove(firstFinishedTask);
                    // EG 20230927 [26506] Ajout du message d'erreur dans la trace
                    //if (firstFinishedTask.IsFaulted)
                    //    throw firstFinishedTask.Exception.Flatten();
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(this, firstFinishedTask, "EvaluateNetMarginingAsync", null, true);
                }
            }
            catch (Exception)
            {
                cts.Cancel();
                throw;
            }
        }
        /// <summary>
        /// Mise en attente d'un tâche de calcul des déposit (Net Margining)
        /// </summary>
        /// <param name="pActorId">Acteur</param>
        /// <param name="pBookId">Book</param>
        /// <param name="pDepositToEvaluate">Déposit à évaluer</param>
        /// <param name="pCt">Notification d'annulation</param>
        /// <returns></returns>
        // EG 20180205 [23769] New
        private async Task<DepositStatus> EvaluateDepositNetMarginingAsync(int pActorId, int pBookId, Deposit pDepositToEvaluate, CancellationToken pCt)
        {
            string key = String.Format("(ActorId: {0} BookId: {1})", pActorId, pBookId);
            string wait = "START EvaluateDepositNetMarginingAsync Wait   : {0} " + key;
            string release = "STOP  EvaluateDepositNetMarginingAsync Release: {0} " + key;

            bool isSemaphoreSpecified = (null != m_SemaphoreDeposit);
            await Task.Run(() =>
                {
                    try
                    {
                        // FI 20200818 [XXXXX] Usage de ImRequestDiagnostics.GetDate()
                        DateTime Start = ImRequestDiagnostics.GetDate();
                        if (isSemaphoreSpecified)
                        {
                            m_SemaphoreDeposit.Wait();
                            AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, m_SemaphoreDeposit.CurrentCount));
                        }

                        EvaluateDepositNetMargining(pActorId, pBookId, pDepositToEvaluate);
                        // FI 20200818 [XXXXX] Usage de ImRequestDiagnostics.GetDate()
                        DateTime End = ImRequestDiagnostics.GetDate();
                        this.ImRequestDiagnostics.SetStartEndTime(this.IdCSS, pActorId, pBookId, Start, End);

                    }
                    catch (Exception) { throw; }
                    finally
                    {
                        if (isSemaphoreSpecified)
                        {
                            m_SemaphoreDeposit.Release();
                            AppInstance.TraceManager.TraceVerbose(this, String.Format(release, m_SemaphoreDeposit.CurrentCount));
                        }
                    }
                }, pCt);

            return pDepositToEvaluate.Status;
        }

        /// <summary>
        /// Calcul des déposit (Gross Margining)
        /// SYNCHRONE MODE
        /// </summary>
        // EG 20180205 [23769] New 
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void EvaluateGrossMargining(CalculationSheet.CalculationSheetRepository pLogsRepository)
        {
           
            Logger.Log(new LoggerData(LogLevelEnum.Info, "Evaluate Gross Margining", 2));

            IEnumerable<KeyValuePair<Pair<int, int>, Deposit>> grossMarginingDeposits =
                this.Deposits.Where(deposit => deposit.Value.IsGrossMargining);

            foreach (KeyValuePair<Pair<int, int>, Deposit> keyValue in grossMarginingDeposits)
            {
                // FI 20200818 [XXXXX] Usage de ImRequestDiagnostics.GetDate()
                DateTime Start = ImRequestDiagnostics.GetDate();
                AppInstance.TraceManager.TraceInformation(this, String.Format("SYNC RiskMethodSet.EvaluateDepositGrossMargining(ActorId: {0},BookId : {1})", keyValue.Key.First, keyValue.Key.Second));
                EvaluateDepositGrossMargining(keyValue.Key.First, keyValue.Key.Second, keyValue.Value);
                // FI 20200818 [XXXXX] Usage de ImRequestDiagnostics.GetDate()
                DateTime End = ImRequestDiagnostics.GetDate();
                this.ImRequestDiagnostics.SetStartEndTime(this.IdCSS, keyValue.Key.First, keyValue.Key.Second, Start, End);
            }
        }

        /// <summary>
        /// Evaluation d'un Deposit (Gestion Nette)
        /// </summary>
        /// <param name="pActorId">Acteur</param>
        /// <param name="pBookId">Book</param>
        /// <param name="pDepositToEvaluate">Deposit à évaluer</param>
        // EG 20180205 [23769] New 
        private List<Money> EvaluateDepositNetMargining(int pActorId, int pBookId, Deposit pDepositToEvaluate)
        {
            // Si Deposit déjà évalué, ne pas réévaluer et retourner directement les montants
            if (pDepositToEvaluate.Status != DepositStatus.EVALUATED)
            {
                pDepositToEvaluate.Status = DepositStatus.EVALUATING;
                List<Money> amounts = null;

                // Lire toute la position
                RiskData riskData = RiskElement.AggregateRiskData(pDepositToEvaluate.Factors);

                // Lancer l'évaluation
                amounts = EvaluateRiskElement(pActorId, pBookId, pDepositToEvaluate.HierarchyClass, riskData, out IMarginCalculationMethodCommunicationObject[] calcMethodComObjs);

                // Ajout d'un montant à 0 au cas ou aucun montant n'aurait été calculé
                AddZeroAmount(amounts);

                // Le Deposit est évalué, affecter et retourner les montants
                pDepositToEvaluate.Status = DepositStatus.EVALUATED;
                pDepositToEvaluate.Amounts = amounts;
                pDepositToEvaluate.MarginCalculationMethods = calcMethodComObjs;

                // PM 20220202 Ajout gestion IsIncomplete
                if (calcMethodComObjs.Where(c => c.IsIncomplete).Count() > 0)
                {
                    pDepositToEvaluate.IsIncomplete = true;
                }
                else
                {
                    pDepositToEvaluate.IsIncomplete = false;
                }

            }
            return pDepositToEvaluate.Amounts;
        }


        /// <summary>
        /// Evaluation d'un Deposit (Gestion Brutte)
        /// </summary>
        /// <param name="pActorId">Acteur</param>
        /// <param name="pBookId">Book</param>
        /// <param name="pDepositToEvaluate">Déposit à évaluer</param>
        // EG 20180205 [23769] New 
        private List<Money> EvaluateDepositGrossMargining(int pActorId, int pBookId, Deposit pDepositToEvaluate)
        {
            // Si Deposit déjà évalué, ne pas réévaluer et retourner directement les montants
            if (pDepositToEvaluate.Status != DepositStatus.EVALUATED)
            {
                pDepositToEvaluate.Status = DepositStatus.EVALUATING;
                IMarginCalculationMethodCommunicationObject[] calcMethodComObjs = null;
                List<Money> amounts = null;

                // Evaluer chaque élément séparement
                foreach (RiskElement factor in pDepositToEvaluate.Factors)
                {
                    List<Money> subamounts = new List<Money>();
                    Pair<int, int> keyDeposit = new Pair<int, int>(factor.ActorId, factor.AffectedBookId);

                    // L'élément est-il un Deposit d'un autre acteur
                    bool isGetAmountsSubDeposit =
                        ((factor.ActorId != pActorId) && (factor.AffectedBookId != pBookId) && Deposits.ContainsKey(keyDeposit));

                    if (isGetAmountsSubDeposit)
                    {
                        #region L'élément est le Deposit d'un autre acteur => un appel récursif
                        Deposit deposit = Deposits[keyDeposit];
                        if (deposit.Status == DepositStatus.NOTEVALUATED)
                        {

                            subamounts = EvaluateDepositGrossMargining(keyDeposit.First, keyDeposit.Second, deposit);
                        }
                        else
                        {
                            subamounts = deposit.Amounts;
                        }
                        #endregion L'élément est le Deposit d'un autre acteur => un appel récursif
                    }
                    else
                    {
                        #region L'élément est un élément du Déposit courant, alors l'évaluer en utilisant la gestion Brute (book par book)
                        // Lire la position par book
                        // PM 20170313 [22833] Changement de type et RiskElement.Positions remplacé par RiskElement.RiskDataActorBook
                        //IEnumerable<IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>>> riskDataByBook = PositionsExtractor.GetPositionsByBook(factor);
                        IEnumerable<RiskData> riskDataByBook = factor.GetRiskData();

                        // PM 20170313 [22833] Changement de type
                        //foreach (IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> bookRiskData in riskDataByBook)
                        foreach (RiskData bookRiskData in riskDataByBook)
                        {
                            // FI 20160613 [22256]  Mise en place des variables idA, idB
                            int idA = 0;
                            int idB = 0;
                            RiskActorBook riskActorBookKey = default;
                            if (pDepositToEvaluate.HierarchyClass == DepositHierarchyClass.ENTITY)
                            {
                                riskActorBookKey = bookRiskData.GetFirstDealerActorBook();
                            }
                            else if (pDepositToEvaluate.HierarchyClass == DepositHierarchyClass.CLEARER)
                            {
                                riskActorBookKey = bookRiskData.GetFirstClearerActorBook();
                            }
                            //else
                            if (riskActorBookKey == default)
                            {
                                throw new NotImplementedException(StrFunc.AppendFormat("DepositHierarchyClass: {0} is not implemented", pDepositToEvaluate.HierarchyClass));
                            }
                            idA = riskActorBookKey.IdA;
                            idB = riskActorBookKey.IdB;

                            List<Money> bookSubamounts = EvaluateRiskElement(idA, idB, pDepositToEvaluate.HierarchyClass, bookRiskData, out IMarginCalculationMethodCommunicationObject[] itemCalcMethodComObjs);

                            // Concaténation des différents résultats afin d'obtenir les réductions de positions 
                            // Remarque : seuls les éléments UnderlyingStock seront exploités
                            if (calcMethodComObjs == null)
                            {
                                calcMethodComObjs = itemCalcMethodComObjs;
                            }
                            else
                            {
                                calcMethodComObjs = (calcMethodComObjs.Concat(itemCalcMethodComObjs)).ToArray();
                            }

                            // Mettre à 0 les montants négatifs avant de les ajouter (deposit < 0 -> deposit := 0)
                            bookSubamounts = RiskTools.SetNegativeToZero(bookSubamounts);
                            BaseMethod.SumAmounts(bookSubamounts, ref subamounts);
                        }
                        #endregion L'élément est un élément du Déposit courant, alors l'évaluer en utilisant la gestion Brute (book par book)
                    }
                    // Mettre à 0 les montants négatifs avant de les ajouter (deposit < 0 -> deposit := 0)
                    subamounts = RiskTools.SetNegativeToZero(subamounts);

                    // Ajouter les sous-montants aux montants totaux
                    BaseMethod.SumAmounts(subamounts, ref amounts);
                }

                // Ajout d'un montant à 0 au cas ou aucun montant n'aurait été calculé
                AddZeroAmount(amounts);

                // Le Deposit est évalué, affecter et retourner les montants
                pDepositToEvaluate.Status = DepositStatus.EVALUATED;
                pDepositToEvaluate.Amounts = amounts;
                pDepositToEvaluate.MarginCalculationMethods = calcMethodComObjs;
            }
            return pDepositToEvaluate.Amounts;
        }

        /// <summary>
        // Ajout d'un montant à 0 au cas ou aucun montant n'aurait été calculé
        /// </summary>
        /// <param name="pLstAmounts">Liste des montants</param>
        // EG 20180205 [23769] New 
        private void AddZeroAmount(List<Money> pLstAmounts)
        {
            if (pLstAmounts == default)
                pLstAmounts = new List<Money>();

            if (pLstAmounts.Count == 0)
            {
                Money zeroAmount = new Money(0, (StrFunc.IsFilled(m_CssCurrency) ? m_CssCurrency : "EUR"));
                pLstAmounts.Add(zeroAmount);
            }
        }


    }
}
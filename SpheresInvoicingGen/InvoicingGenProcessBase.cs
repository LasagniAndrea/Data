#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.Administrative.Invoicing;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process.EventsGen;
using EFS.SpheresService;
using EFS.TradeInformation;
using EFS.TradeLink;
using EFS.Tuning;
//
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;
//
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion Using Directives

namespace EFS.Process
{

    public abstract class InvoicingGenProcessBase
    {
        #region Members
        protected InvoicingGenProcess m_InvoicingGenProcess;
        protected EventProcess m_EventProcess;
        protected Invoicing m_Invoicing;
        #endregion Members
        #region Accessors
        #region ActionDate
        public virtual DateTime ActionDate
        {
            get { return DateTime.MinValue; }
        }
        #endregion ActionDate
        #region Criteria
        public virtual SQL_Criteria Criteria
        {
            get { return null; }
        }
        #endregion Criteria
        #region CurrentInstrument
        public AdministrativeInstrumentBase CurrentInstrument
        {
            get { return m_Invoicing.entity.CurrentInstrument; }
        }
        #endregion CurrentInstrument
        #region CorrectionInvoiceDates
        protected string CorrectionInvoiceDates
        {
            get
            {
                string eventCode = string.Empty;
                switch (m_Invoicing.entity.defaultInstrumentType)
                {
                    case InvoicingInstrumentTypeEnum.AdditionalInvoicing:
                        eventCode = EventCodeFunc.AdditionalInvoiceDates;
                        break;
                    case InvoicingInstrumentTypeEnum.CreditNote:
                        eventCode = EventCodeFunc.CreditNoteDates;
                        break;
                }
                return eventCode;
            }
        }
        #endregion CorrectionInvoiceDates
        #region EntityValue
        // EG 20150706 [21021] new
        public virtual int EntityValue
        {
            get { return Entity ?? 0; }
        }
        #endregion EntityValue

        #region Entity
        // EG 20150706 [21021] Nullable<int>
        public virtual Nullable<int> Entity
        {
            get { return null; }
        }
        #endregion Entity
        #region IdA_Entity
        protected int IdA_Entity
        {
            get { return m_Invoicing.parameter.idA_Entity; }
        }
        #endregion IdA_Entity
        #region IdA_PayerSpecified
        protected bool IdA_PayerSpecified
        {
            get { return m_Invoicing.parameter.IdA_PayerSpecified; }
        }
        #endregion IdA_PayerSpecified
        #region IdC_FeeSpecified
        protected bool IdC_FeeSpecified
        {
            get { return m_Invoicing.parameter.IdC_FeeSpecified; }
        }
        #endregion IdC_FeeSpecified
        #region InvoiceDate
        protected DateTime InvoiceDate
        {
            get { return m_Invoicing.parameter.invoiceDate; }
        }
        #endregion InvoiceDate
        #region IsSimulation
        public virtual bool IsSimulation
        {
            get { return true; }
        }
        #endregion IsSimulation
        #region MasterDate
        public virtual DateTime MasterDate
        {
            get { return DateTime.MinValue; }
        }
        #endregion MasterDate
        #region ProcessBase
        public ProcessBase ProcessBase
        {
            get { return (ProcessBase)m_InvoicingGenProcess; }
        }
        #endregion ProcessBase
        #region Queue
        public MQueueBase Queue
        {
            get { return m_InvoicingGenProcess.MQueue; }
        }
        #endregion Queue
        #region StEnvironment
        protected Cst.StatusEnvironment StEnvironment
        {
            get
            {
                Cst.StatusEnvironment stEnvironnement = Cst.StatusEnvironment.REGULAR;
                if (m_Invoicing.parameter.isSimul)
                    stEnvironnement = Cst.StatusEnvironment.SIMUL;
                return stEnvironnement;
            }
        }
        #endregion StEnvironment
        public virtual string TradeAdminTitle(string pValue, InvoicingScope pScope)
        {
            string title;
            switch (m_Invoicing.entity.defaultInstrumentType)
            {
                case InvoicingInstrumentTypeEnum.AdditionalInvoicing:
                    title = Ressource.GetString2("AdditionalInvoiceDisplayName", pValue);
                    break;
                case InvoicingInstrumentTypeEnum.CreditNote:
                    title = Ressource.GetString2("CreditNoteDisplayName", pValue);
                    break;
                default:
                    InvoicingDates period = pScope.invoicingPeriodDates;
                    string startPeriod = DtFunc.DateTimeToString(period.startPeriodPlusOneDay, DtFunc.FmtShortDate);
                    string endPeriod = DtFunc.DateTimeToString(period.endPeriod, DtFunc.FmtShortDate);
                    title = Ressource.GetString2(pValue, startPeriod, endPeriod);
                    break;
            }
            return title;
        }
        #endregion Accessors
        #region Constructors
        public InvoicingGenProcessBase(InvoicingGenProcess pInvoicingGenProcess)
        {
            m_InvoicingGenProcess = pInvoicingGenProcess;
        }
        #endregion Constructors
        #region Methods
        #region Generate
        public virtual Cst.ErrLevel Generate()
        {
            return Cst.ErrLevel.UNDEFINED;
        }
        #endregion Generate
        #region Save
        /// <summary>
        /// Sauvegarde de la facture
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        /// FI 20170404 [23039] Modify
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected Cst.ErrLevel Save(InvoicingScope pScope)
        {
            TradeCommonCaptureGen.ErrorLevel codeReturn = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
            ProcessState processState = null;
            IDbTransaction dbTransaction = null;
            try
            {
                AdministrativeInstrumentBase currentInstrument = CurrentInstrument;
                
                #region START Transaction (Begin Tran)
                //Begin Tran  doit être la 1er instruction Car si Error un  roolback est fait de manière systematique
                try { dbTransaction = DataHelper.BeginTran(ProcessBase.Cs); }
                catch (Exception ex) { throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, TradeCommonCaptureGen.ErrorLevel.BEGINTRANSACTION_ERROR); }
                #endregion START Transaction (Begin Tran)
                //
                TradeAdminCaptureGen captureGen = new TradeAdminCaptureGen();
                TradeAdminInput input = captureGen.Input;

                input.Identification = new SpheresIdentification
                {
                    Identifier = currentInstrument.SQLTrade.Identifier,
                    OTCmlId = currentInstrument.SQLTrade.Id
                };
                input.SQLTrade = currentInstrument.SQLTrade;
                input.SQLProduct = currentInstrument.SQLProduct;
                input.SQLInstrument = currentInstrument.SQLInstrument;
                input.DataDocument = pScope.DataDocument;
                //
                // EG 20100217 
                input.TradeStatus.Initialize(ProcessBase.Cs, currentInstrument.SQLTrade.IdT);
                //
                input.TradeStatus.stEnvironment.CurrentSt = StEnvironment.ToString();
                input.TradeStatus.stActivation.CurrentSt = Cst.StatusActivation.REGULAR.ToString();
                input.TradeStatus.stPriority.CurrentSt = Cst.StatusPriority.REGULAR.ToString();
                input.InitStUserFromPartiesRole(CSTools.SetCacheOn(ProcessBase.Cs), null);
                //FI 20110112 Alimentation des partyNotification
                input.SetTradeNotification(true);
                //
                #region Ecriture dans la base de données
                CaptureSessionInfo sessionInfo = new CaptureSessionInfo
                {
                    user = new User(ProcessBase.Session.IdA, null, RoleActor.SYSADMIN),
                    session = ProcessBase.Session,
                    licence = ProcessBase.License,
                    idProcess_L = ProcessBase.IdProcess,
                    idTracker_L = ProcessBase.Tracker.IdTRK_L
                };

                TradeRecordSettings recordSettings = new TradeRecordSettings
                {
                    displayName = TradeAdminTitle("InvoiceDisplayName", pScope),
                    description = TradeAdminTitle("InvoiceDescription", pScope), 
                    extLink = string.Empty,
                    idScreen = currentInstrument.ScreenName,
                    isGetNewIdForIdentifier = true,
                    isCheckValidationRules = true,
                    isCheckValidationXSD = false,
                    isCheckLicense = false
                };

                Pair<int, string>[] underlying = null;
                Pair<int, string>[] trader = null;

                TradeCommonCaptureGenException errExc = null;
                try
                {
                    captureGen.CheckAndRecord(ProcessBase.Cs, dbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin), Cst.Capture.ModeEnum.New, sessionInfo, recordSettings,
                                                            ref pScope.tradeIdentifier, ref pScope.idT,
                                                            out underlying, out trader);
                }
                catch (TradeCommonCaptureGenException ex)
                {
                    //Erreur reconnue
                    errExc = ex;
                    codeReturn = errExc.ErrLevel;
                }
                catch (Exception) { throw; }//Error non gérée
                #endregion Ecriture dans la base de données
                //
                //Si une exception se produit après l'enregistrement du trade=> cela veut dire que le trade est correctement rentré en base, on est en succès
                if (null != errExc)
                {
                    if (TradeCommonCaptureGen.IsRecordInSuccess(errExc.ErrLevel))
                        codeReturn = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
                    else
                        throw errExc;
                }
                //
                if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == codeReturn)
                {
                    ProductContainer productcontainer = captureGen.Input.DataDocument.CurrentProduct;
                    if (productcontainer.IsCreditNote ||
                        productcontainer.IsAdditionalInvoice)
                    {
                        #region TradeLink
                        /* 20090227 EG : Identifiants liens changés */
                        int invoiceId = 0;
                        string invoiceIdentifier = string.Empty;
                        TradeLinkType tradeLinkType = TradeLinkType.NA;
                        TradeLinkDataIdentification tradeLinkData = TradeLinkDataIdentification.NA;
                        if (productcontainer.IsAdditionalInvoice)
                        {
                            tradeLinkType = TradeLinkType.AddInvoice;
                            tradeLinkData = TradeLinkDataIdentification.AddInvoiceIdentifier;
                            IAdditionalInvoice additionalInvoice = (IAdditionalInvoice)productcontainer.Product;
                            invoiceId = additionalInvoice.InitialInvoiceAmount.OTCmlId;
                            invoiceIdentifier = additionalInvoice.InitialInvoiceAmount.Identifier.Value;
                        }
                        else if (productcontainer.IsCreditNote)
                        {
                            tradeLinkType = TradeLinkType.CreditNote;
                            tradeLinkData = TradeLinkDataIdentification.CreditNoteIdentifier;
                            ICreditNote creditNote = (ICreditNote)productcontainer.Product;
                            invoiceId = creditNote.InitialInvoiceAmount.OTCmlId;
                            invoiceIdentifier = creditNote.InitialInvoiceAmount.Identifier.Value;
                        }
                        /* 20090227 EG : Identifiants liens changés */
                        TradeLink.TradeLink tradeLink = new TradeLink.TradeLink( pScope.idT, invoiceId, tradeLinkType,
                            null, null,
                            new string[2] { pScope.tradeIdentifier, invoiceIdentifier },
                            new string[2] { tradeLinkData.ToString(), TradeLinkDataIdentification.InvoiceIdentifier.ToString() });
                        tradeLink.Insert(ProcessBase.Cs, dbTransaction);
                        #endregion TradeLink
                    }
                    else if (productcontainer.IsInvoice)
                    {
                        #region TradeLink
                        TradeLinkType tradeLinkType = TradeLinkType.Invoice;
                        TradeLinkDataIdentification tradeLinkData = TradeLinkDataIdentification.InvoiceIdentifier;
                        foreach (InvoicingTrade trade in pScope.trades)
                        {
                            if (ArrFunc.IsFilled(trade.events))
                            {
                                TradeLink.TradeLink tradeLink = new TradeLink.TradeLink( pScope.idT, trade.idT, tradeLinkType,
                                    null, null,
                                    new string[2] { pScope.tradeIdentifier, trade.tradeIdentifier },
                                    new string[2] { tradeLinkData.ToString(), TradeLinkDataIdentification.TradeIdentifier.ToString() });
                                tradeLink.Insert(ProcessBase.Cs, dbTransaction);
                            }
                        }
                        #endregion TradeLink
                    }
                    //
                    #region Suppression des trades générés précédemment en mode SIMUL pour le même contexte
                    codeReturn = pScope.DeleteTradeSimul(dbTransaction);
                    #endregion Suppression des trades générés précédemment en mode SIMUL pour le même contexte
                    //
                    #region Mise à jour statut IDSTUSEDBY (Action sur facture) / Evénements / TradeLink
                    if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == codeReturn)
                        codeReturn = Update(dbTransaction);
                    #endregion Mise à jour statut IDSTUSEDBY (Action sur facture) / Evénements / TradeLink
                    //
                    if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == codeReturn)
                    {
                        #region END Transaction (Commit)
                        try { DataHelper.CommitTran(dbTransaction); dbTransaction = null; }
                        catch (Exception ex) { throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, TradeCommonCaptureGen.ErrorLevel.COMMIT_ERROR); }
                        #endregion END Transaction (Commit)

                        // Insertion LOG Trade invoice / additionalInvoice / CreditNote
                        
                        //string msg = "LOG-05230";
                        SysMsgCode msg = new SysMsgCode(SysCodeEnum.LOG, 5230);
                        if (productcontainer.IsCreditNote)
                        {
                            //msg = "LOG-05233";
                            msg = new SysMsgCode(SysCodeEnum.LOG, 5233);
                        }
                        else if (productcontainer.IsAdditionalInvoice)
                        {
                            //msg = "LOG-05232";
                            msg = new SysMsgCode(SysCodeEnum.LOG, 5232);
                        }
                        Logger.Log(new LoggerData(LogLevelEnum.Info, msg, 1,
                            new LogParam(LogTools.IdentifierAndId(pScope.tradeIdentifier, pScope.idT)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(InvoiceDate))));

                        // Insertion LOG Détail génération des événements
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 500), 2,
                            new LogParam(LogTools.IdentifierAndId(pScope.tradeIdentifier, pScope.idT)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(InvoiceDate))));

                        MQueueAttributes mQueueAttributes = new MQueueAttributes()
                        {
                            connectionString = ProcessBase.Cs,
                            id = pScope.idT,
                            idInfo = new IdInfo()
                            {
                                id = pScope.idT,
                                idInfos = new DictionaryEntry[]{
                                                new DictionaryEntry("ident", "TRADEADMIN"),
                                                new DictionaryEntry("identifier", pScope.tradeIdentifier),
                                                new DictionaryEntry("GPRODUCT", Cst.ProductGProduct_ADM)}
                            },
                            requester = Queue.header.requester
                        };
                        // EG 20220519 [WI637] SEul EventsGen est en mode SLAVE
                        processState = New_EventsGenAPI.ExecuteSlaveCall(new EventsGenMQueue(mQueueAttributes), null, ProcessBase, true);
                        if (Cst.ErrLevel.SUCCESS == processState.CodeReturn)
                        {
                            // Insertion LOG Detail Valorisation des événements
                            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 600), 2,
                                new LogParam(LogTools.IdentifierAndId(pScope.tradeIdentifier, pScope.idT)),
                                new LogParam(DtFunc.DateTimeToStringDateISO(InvoiceDate))));

                            // EG 20220519 [WI637] SEul EventsGen est en mode SLAVE
                            //processState = New_EventsValAPI.ExecuteSlaveCall(new EventsValMQueue(mQueueAttributes), 
                            //    ProcessBase, false, false, true);
                        }
                    }
                }
                if (processState.CodeReturn != Cst.ErrLevel.SUCCESS)
                    codeReturn = TradeCommonCaptureGen.ErrorLevel.FAILURE;
                return processState.CodeReturn;
            }
            catch (TradeCommonCaptureGenException ex)
            {
                codeReturn = ex.ErrLevel;
                throw;
            }
            catch (SpheresException2)
            { 
                codeReturn = TradeCommonCaptureGen.ErrorLevel.FAILURE; 
                throw; 
            }
            catch (Exception ex) 
            {
                codeReturn = TradeCommonCaptureGen.ErrorLevel.FAILURE;
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); 
            }
            finally
            {
                if ((TradeCommonCaptureGen.ErrorLevel.SUCCESS != codeReturn) && (null != dbTransaction))
                    DataHelper.RollbackTran(dbTransaction);
            }
        }
        #endregion Save
        #region SetDataDocument
        /// <summary>
        /// Alimentation du DataDocument de facturation (Facture, Facture additionnelle, Avoir)
        /// - Début et fin de période de facturation
        /// - Conditions (Remise et Tranches)
        /// - Taxes
        /// +
        /// - Détermination du type de document (Facture, Facture additionnelle ou Avoir)
        /// - Alimentation des parties
        /// - Alimentation du tradeHeader
        /// - Alimentation des données de facturation
        /// - Alimentation des données "Détails" de facturation
        /// - Alimentation des paramètres de Remise
        /// - Calcul des montants de facturation
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        protected Cst.ErrLevel SetDataDocument(InvoicingScope pScope)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            AdministrativeInstrumentBase currentInstrument = CurrentInstrument;

            if (null == currentInstrument)
                codeReturn = Cst.ErrLevel.DATANOTFOUND;

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                // Début et Fin de période de facturation
                InvoicingRules rules = pScope.rules;
                ICalculationPeriodFrequency applicationPeriod = currentInstrument.CreateFrequency(rules.period_Invoicing, rules.periodMltp_Invoicing, rules.rollConvention_Invoicing);
                pScope.invoicingPeriodDates = new InvoicingDates(this, InvoicingApplicationPeriodTypeEnum.InvoicingDate, InvoiceDate, applicationPeriod, pScope.context.idC_Fee);
                InvoicingConditions conditions = pScope.conditions;
                if (null != conditions)
                {
                    if (conditions.maxValueSpecified)
                    {
                        // Début et Fin de période d'application du plafond
                        applicationPeriod = currentInstrument.CreateFrequency(conditions.maxPeriod, conditions.maxPeriodMltp, rules.rollConvention_Invoicing);
                        pScope.conditions.capPeriodDates = new InvoicingDates(this, InvoicingApplicationPeriodTypeEnum.RebateCap, InvoiceDate, applicationPeriod, pScope.context.idC_Fee);
                    }
                    if (conditions.bracketApplicationSpecified)
                    {
                        // Début et Fin de période d'application des remises par tranche
                        applicationPeriod = currentInstrument.CreateFrequency(conditions.discountPeriod, conditions.discountPeriodMltp, rules.rollConvention_Invoicing);
                        pScope.conditions.bracketPeriodDates = new InvoicingDates(this, InvoicingApplicationPeriodTypeEnum.RebateBracket, InvoiceDate, applicationPeriod, pScope.context.idC_Fee);
                    }
                }
                // Tax
                InvoicingTax tax = pScope.tax;
                if (null != tax)
                    codeReturn = tax.GetTax(InvoiceDate, m_Invoicing.entity);
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = pScope.SetDataDocument(InvoiceDate, m_Invoicing.entity);
            return codeReturn;
        }
        #endregion SetDataDocument
        #region SetDataDocumentAndSave
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel SetDataDocumentAndSave()
        {
            Cst.ErrLevel codeReturnMaster = Cst.ErrLevel.SUCCESS;
            // Insertion LOG Détail 
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5236), 0,
                new LogParam(LogTools.IdentifierAndId(m_Invoicing.EntityIdentifier, EntityValue)),
                new LogParam(DtFunc.DateTimeToStringDateISO(InvoiceDate))));

            AdministrativeInstrumentBase currentInstrument = CurrentInstrument;
            if (null != currentInstrument)
            {
                _ = currentInstrument.CreateDataDocument();

                if (ProcessBase.IsParallelProcess(ParallelProcess.InvoicingWriting))
                    codeReturnMaster = InvoicingWritingThreading();
                else
                {
                    foreach (InvoicingScope scope in m_Invoicing.scopes)
                    {
                        Cst.ErrLevel codeReturn = CreateResult(scope);
                        if (Cst.ErrLevel.SUCCESS != codeReturn)
                            codeReturnMaster = Cst.ErrLevel.FAILURE;
                    }
                }
            }
            else
            {
                codeReturnMaster = Cst.ErrLevel.DATANOTFOUND;
            }
            return codeReturnMaster;
        }
        #endregion SetDataDocumentAndSave
        #region SetRowStatus
        public void SetRowStatus(DataRow pRow, DataSetEventTrade pDsEventTrade, TuningOutputTypeEnum pTypeEnum)
        {
            if (ProcessBase.ProcessTuningSpecified)
            {
                if ((TuningOutputTypeEnum.OES == pTypeEnum) || (TuningOutputTypeEnum.OEE == pTypeEnum))
                {
                    int idE = Convert.ToInt32(pRow["IDE"]);
                    ProcessTuningOutput processTuning = ProcessBase.ProcessTuning.GetProcessTuningOutput(pTypeEnum);
                    // FI 20200820 [25468] dates systemes en UTC
                    pDsEventTrade.SetEventStatus(idE, processTuning, ProcessBase.Session.IdA, OTCmlHelper.GetDateSysUTC(ProcessBase.Cs));
                }
            }
        }
        #endregion SetRowStatus

        #region Update
        protected virtual TradeCommonCaptureGen.ErrorLevel Update(IDbTransaction pDbTransaction)
        {
            return TradeCommonCaptureGen.ErrorLevel.SUCCESS;
        }
        #endregion Update
        #region UpdateTradeStSys
        public virtual Cst.ErrLevel UpdateTradeStSys(IDbTransaction pDbTransaction)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UpdateTradeStSys

        /// <summary>
        /// Calcul et Sauvegarde des factures en multi-threading
        /// Découpage de la liste des périmètres de facturation avec facture à générer en sous-liste
        /// - en fonction du paramètre heapSize
        /// </summary>
        /// <returns></returns>
        private Cst.ErrLevel InvoicingWritingThreading()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (ArrFunc.IsFilled(m_Invoicing.scopes))
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                int heapSize = ProcessBase.GetHeapSize(ParallelProcess.InvoicingWriting);
                List<List<InvoicingScope>> subScopes = ListExtensionsTools.ChunkBy(m_Invoicing.scopes.ToList(), heapSize);

                int counter = 1;
                int totSubList = subScopes.Count();
                int prevSubList = 0;
                subScopes.ForEach(lstScopes =>
                {
                    if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                        (false == IRQTools.IsIRQRequested(ProcessBase, ProcessBase.IRQNamedSystemSemaphore, ref ret)))
                    {
                        ProcessBase.InitializeMaxThreshold(ParallelProcess.InvoicingWriting);
                        Task<ProcessState> task = CreateResultsAsync(lstScopes, cts);
                        task.Wait();
                        ProcessBase.ProcessState.SetErrorWarning(task.Result.Status, task.Result.CodeReturn);

                        prevSubList += lstScopes.Count();

                        AppInstance.TraceManager.TraceTimeSummary("InvoicingWritingThreading", StrFunc.AppendFormat("{0}/{1}", counter, totSubList));
                        counter++;
                    }
                });
                // EG 20220519 [WI637] IRQ
                if (Cst.ErrLevel.IRQ_EXECUTED != ret) 
                    ret = ProcessBase.ProcessState.CodeReturn;

            }
            return ret;
        }
        /// <summary>
        /// Classe asynchrone de génération des factures
        /// - Traitement multi-tâche des périmètres à facturer
        /// </summary>
        /// <param name="pLstScopes"></param>
        /// <param name="pPreviousCounter"></param>
        /// <param name="pCts"></param>
        /// <returns></returns>
        private async Task<ProcessState> CreateResultsAsync(List<InvoicingScope> pLstScopes, CancellationTokenSource pCts)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            ProcessState processState = new ProcessState(ProcessStateTools.StatusEnum.SUCCESS, ret);
            try
            {
                #region Génération des trades Facturations
                List<Task<Cst.ErrLevel>> getReturnTasks = null;
                Common.AppInstance.TraceManager.TraceInformation(this, "START CreateResultsAsync INV");
                IEnumerable<Task<Cst.ErrLevel>> getReturnTasksQuery =
                    from scope in pLstScopes select CreateResultAsync(scope, pCts.Token);

                getReturnTasks = getReturnTasksQuery.ToList();
                int counter = 0;
                while (0 < getReturnTasks.Count)
                {
                    // On s'arrête sur la première tâche complétée.  
                    Task<Cst.ErrLevel> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    // On supprime la tâche de la liste des tâches pour éviter que le process ne la traite une nouvelle fois.  
                    getReturnTasks.Remove(firstFinishedTask);
                    counter++;

                    // On rentre ici exceptionnellement (si exception non gérée) ou interruption de traitement
                    // Dans la méthode CreateResult quasi toute exception rencontrée lors de la création du trade est trappée pour produire un log 
                    // EG 20230927 [26506] Ajout du message d'erreur dans la trace
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(this, firstFinishedTask, "CreateResultsAsync", processState);

                    if (Cst.ErrLevel.SUCCESS != firstFinishedTask.Result)
                        processState.SetErrorWarning(ProcessStateTools.StatusEnum.ERROR, firstFinishedTask.Result);

                    Logger.Write();

                    if (IRQTools.IsIRQRequested(ProcessBase, ProcessBase.IRQNamedSystemSemaphore, ref processState))
                        pCts.Cancel(true);
                }
                #endregion Génération des trades Facturations
            }
            catch (AggregateException)
            {
                if (processState.CodeReturn != Cst.ErrLevel.IRQ_EXECUTED)
                {
                    pCts.Cancel(true);
                    throw;
                }

            }
            catch (Exception)
            {
                pCts.Cancel(true);
                throw;
            }
            finally
            {
                Common.AppInstance.TraceManager.TraceInformation(this, "STOP CreateResultsAsync INV");
                Logger.Write();
            }
            return processState;
        }
        /// <summary>
        /// Tâche asynchrone de génération d'une facture
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pCt"></param>
        /// <returns></returns>
        // EG 20220523 [WI639] Gestion des Deadlocks
        private async Task<Cst.ErrLevel> CreateResultAsync(InvoicingScope pScope, CancellationToken pCt)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            string invoiced = LogTools.IdentifierAndId(pScope.actorInvoiced.Identifier, pScope.actorInvoiced.Id);
            string payer = LogTools.IdentifierAndId(pScope.actorPayer.Identifier, pScope.actorPayer.Id);

            string key = $"(Actor invoiced: {invoiced} Payer: {payer})";
            if (null != pScope.actorBeneficiary)
            {
                string beneficiary = LogTools.IdentifierAndId(pScope.actorBeneficiary.actor.Identifier, pScope.actorBeneficiary.actor.Id);
                string bookBeneficiary = LogTools.IdentifierAndId(pScope.actorBeneficiary.book.SQLBook.Identifier, pScope.actorBeneficiary.book.SQLBook.Id);
                key += $" (Beneficiary: {beneficiary} Book beneficiary: {bookBeneficiary})";
            }
            string wait = "START CreateResultAsync Wait   : {0} " + key;
            string release = "STOP  CreateResultAsync Release: {0} " + key;

            bool isSemaphoreSpecified = (null != ProcessBase.SemaphoreAsync);
            await Task.Run(() =>
            {
                try
                {
                    Common.AppInstance.TraceManager.TraceTimeBegin("InvoicingGenAsync", key);
                    if (isSemaphoreSpecified)
                    {
                        ProcessBase.SemaphoreAsync.Wait();
                        Common.AppInstance.TraceManager.TraceVerbose(this, string.Format(wait, ProcessBase.SemaphoreAsync.CurrentCount));
                    }
                    // EG 20220519 [WI637] En attente, à tester plus tard
                    TryMultiple tryMultiple = new TryMultiple(this.ProcessBase.Cs, "CreateResult", "CreateResult")
                    {
                        SetErrorWarning = ProcessBase.ProcessState.SetErrorWarning,
                        IsModeTransactional = false,
                        ThreadSleep = 5 //blocage de 5 secondes entre chaque tentative
                    };
                    ret = tryMultiple.Exec<InvoicingScope, Cst.ErrLevel>(CreateResult, pScope as InvoicingScope);

                    //CreateResult(pCS, pTradeInfo, pInputUser, pSessionInfo, key);
                    //tryMultiple.Exec<string, MRTradeInfo, InputUser, CaptureSessionInfo, string, MRTradeInfo>(
                    //    delegate (String arg1, MRTradeInfo arg2, InputUser arg3, CaptureSessionInfo arg4, String arg5)
                    //    { return CreateResult(arg1, arg2, arg3, arg4, arg5); }, pCS, pTradeInfo, pInputUser, pSessionInfo, key);

                }
                catch (Exception) { throw; }
                finally
                {
                    if (isSemaphoreSpecified)
                    {
                        ProcessBase.SemaphoreAsync.Release();
                        Common.AppInstance.TraceManager.TraceVerbose(this, string.Format(release, ProcessBase.SemaphoreAsync.CurrentCount));
                    }
                    Common.AppInstance.TraceManager.TraceTimeEnd("InvoicingGenAsync", key);
                }
            }, pCt);

            return ret;
        }

        /// <summary>
        /// Génération de la facture
        /// - Appelée en mode mono-threading ou multi-threading
        /// - Alimentation du DataDocument
        /// - Sauvegarde
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        private Cst.ErrLevel CreateResult(InvoicingScope pScope)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if (ArrFunc.IsFilled(pScope.trades))
            {
                codeReturn = SetDataDocument(pScope);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = Save(pScope);
            }
            return codeReturn;
        }


        #endregion Methods
    }

}
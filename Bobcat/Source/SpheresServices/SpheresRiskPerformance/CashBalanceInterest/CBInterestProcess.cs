using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Process.EventsGen;
using EFS.TradeInformation;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.CashBalanceInterest;
using EfsML.v30.Shared;
//
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Ird;
using FpML.v44.Shared;
//
using Tz = EFS.TimeZone;

namespace EFS.SpheresRiskPerformance.CashBalanceInterest
{
    /// <summary>
    /// 
    /// </summary>
    public class CashBalanceInterestProcess : RiskCommonProcessBase
    {
        #region members
        internal InterestProcessParameters m_ProcessParameter = null;
        #endregion members

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQueue"></param>
        /// <param name="pAppInstance"></param>
        public CashBalanceInterestProcess(CashBalanceInterestMQueue pQueue, AppInstanceService pAppInstance)
            : base(pQueue, pAppInstance)
        {
            m_ProcessParameter = new InterestProcessParameters();
        }
        #endregion constructors

        #region methods
        /// <summary>
        /// 
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void ProcessInitialize()
        {
            try
            {
                base.ProcessInitialize();

                if (false == IsProcessObserver)
                {
                    CashBalanceInterestMQueue interestQueue = (CashBalanceInterestMQueue)base.MQueue;
                    if (interestQueue.idSpecified)
                    {
                        m_ProcessParameter.IdA_Cbo = interestQueue.id;
                    }
                    if (interestQueue.identifierSpecified)
                    {
                        m_ProcessParameter.Identifier_Cbo = interestQueue.identifier;
                    }
                    if (interestQueue.parametersSpecified
                        && interestQueue.IsExistParameter(CashBalanceInterestMQueue.PARAM_DATE1)
                        && interestQueue.IsExistParameter(CashBalanceInterestMQueue.PARAM_AMOUNTTYPE)
                        && interestQueue.IsExistParameter(CashBalanceInterestMQueue.PARAM_IDC))
                    {
                        try
                        {
                            m_ProcessParameter.ProcessDate = interestQueue.GetDateTimeValueParameterById(CashBalanceInterestMQueue.PARAM_DATE1);
                            m_ProcessParameter.AmountType = interestQueue.GetStringValueParameterById(CashBalanceInterestMQueue.PARAM_AMOUNTTYPE);
                            m_ProcessParameter.Currency = interestQueue.GetStringValueParameterById(CashBalanceInterestMQueue.PARAM_IDC);
                            m_ProcessParameter.Period = interestQueue.GetStringValueParameterById(CashBalanceInterestMQueue.PARAM_PERIOD);
                            m_ProcessParameter.PeriodMultiplier = interestQueue.GetIntValueParameterById(CashBalanceInterestMQueue.PARAM_PERIODMLTP);
                        }
                        catch (SystemException ex)
                        {
                            // FI 20200623 [XXXXX] AddCriticalException
                            ProcessState.AddCriticalException(ex);

                            // FI 20200623 [XXXXX] SetErrorWarning
                            ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                            
                            Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1014), 0));

                            ProcessState.CodeReturn = Cst.ErrLevel.MISSINGPARAMETER;
                        }
                    }
                    else
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.None, Ressource.GetString("RiskPerformance_WARNINGParametersNotExist")));

                        ProcessState.CodeReturn = Cst.ErrLevel.MISSINGPARAMETER;
                    }

                // Initialise les requêtes
                CBInterestDataContractHelper.InitCashBalanceInterest(this.Cs);

                    /////////////////////////////////////////////////////////////////////////////////////////////
                    SerializationHelper.SerializationDirectory = this.Session.GetTemporaryDirectory(AppSession.AddFolderSessionId.True);
                    /////////////////////////////////////////////////////////////////////////////////////////////
                }
            }
            catch (Exception ex)
            {
                ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;
                // FI 20200623 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(ex);

                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4500), 0,
                    new LogParam(m_ProcessParameter.ProcessDateISO),
                    new LogParam(m_ProcessParameter.Identifier_Cbo),
                    new LogParam(m_ProcessParameter.Currency),
                    new LogParam(m_ProcessParameter.AmountType)));
            }
        }
        /// <summary>
        /// Calcul des intérêts sur solde ou sur couverture espèce du deposit
        /// </summary>
        /// <returns></returns>
        /// EG 20140204 [19586] Add ProcessBase parameter (gestion TimeOut/DeadLock)
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            m_Timers.CreateTimer("PROCESSEXECUTESPECIFIC");
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4500), 0,
                new LogParam(m_ProcessParameter.ProcessDateISO),
                new LogParam(m_ProcessParameter.Identifier_Cbo),
                new LogParam(m_ProcessParameter.Currency),
                new LogParam(m_ProcessParameter.AmountType)));

            codeReturn = base.ProcessExecuteSpecific();
            try
            {
                // Rechercher et calculer les montants sur lesquels portent les intérêts
                CashInterestCalculation CBICalc = new CashInterestCalculation(this, Cs, m_ProcessParameter);
                // Charger les règles de calcul et les flux
                codeReturn = CBICalc.LoadData();
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    Cst.ErrLevel createCodeReturn = Cst.ErrLevel.SUCCESS;
                    // Calcul des flux
                    CBICalc.CalcAllFlows();
                    // Génération des informations sur le(s) trade(s) à créer
                    List<CashBalanceInterestTradeInfo> tradeInfo = CBICalc.GetTradeInfo();
                    // Génèrer le(s) trade(s) et ses évènements
                    // La valorisation des évènements se fera classique=> Le service de génération des évènements poste un message à EventsVal
                    foreach (CashBalanceInterestTradeInfo ti in tradeInfo)
                    {
                        createCodeReturn = CheckTradeExist(Cs, ti.IdA_Interest, ti.IdB_Interest, ti.IdA_Entity, ti.IdB_Entity, m_ProcessParameter.AmountTypeEnum, out int idT);
                        if (Cst.ErrLevel.SUCCESS == createCodeReturn)
                        {
                            // Créer le trade si le trade existe déjà ou s'il possède au moins un montant != 0
                            if ((idT != 0) || ti.Amounts.Exists(a => a.Amount.DecValue != 0))
                            {
                                createCodeReturn = CreateTradeCashBalanceInterest(Cs, ti, idT);
                            }
                            else
                            {
                                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4501), 0));
                            }
                        }
                        //codeReturn du CheckTradeExist ou du CreateAndRecordTrade
                        if (Cst.ErrLevel.SUCCESS != createCodeReturn)
                        {
                            codeReturn = createCodeReturn;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // FI 20220803 [XXXXX] Trace déjà alimentée par le logger
                if (false == LoggerManager.IsEnabled)
                {
                    // FI 20200918 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                    Common.AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));
                }

                ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;

                ProcessState.AddCriticalException(ex);
                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
            }
            finally
            {
                if (false == ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4500), 0,
                        new LogParam(m_ProcessParameter.ProcessDateISO),
                        new LogParam(m_ProcessParameter.Identifier_Cbo),
                        new LogParam(m_ProcessParameter.Currency),
                        new LogParam(m_ProcessParameter.AmountType)));
                }
            }
            return codeReturn;
        }

        /// <summary>
        /// Vérifie s'il existe un Trade cash balance interest pour la date de traitement demandée.
        /// <para>Pour le couple ({pIdA},{pIdB})</para>
        /// <para>et l'Entity {pIdA_Entity})</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// <param name="pIdA_Entity"></param>
        /// <param name="pIdB_Entity"></param>
        /// <param name="pInterestTypeEnum"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        private Cst.ErrLevel CheckTradeExist(string pCS, int pIdA, int pIdB, int pIdA_Entity, int pIdB_Entity, InterestAmountTypeEnum pInterestTypeEnum, out int pIdT)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            pIdT = 0;
            //
            try
            {
                Dictionary<string, object> dbParametersValue = new Dictionary<string, object>();
                List<InterestTrade> previousTrade = null;
                using (IDbConnection connection = DataHelper.OpenConnection(pCS))
                {
                    /*int nbstream = -1;
                    switch (pInterestTypeEnum)
                    {
                        case InterestAmountTypeEnum.CashBalance:
                            nbstream = 2;
                            break;
                        case InterestAmountTypeEnum.CashCoveredInitialMargin:
                            nbstream = 1;
                            break;
                    }
                    */
                    // Chargement des trades issus d'un précédant calcul
                    dbParametersValue.Add(DataParameter.ParameterEnum.PRODUCT.ToString(), Cst.ProductCashBalanceInterest);
                    dbParametersValue.Add(DataParameter.ParameterEnum.IDA.ToString(), pIdA);
                    dbParametersValue.Add(DataParameter.ParameterEnum.IDB.ToString(), pIdB);
                    dbParametersValue.Add(DataParameter.ParameterEnum.IDA_ENTITY.ToString(), pIdA_Entity);
                    dbParametersValue.Add(DataParameter.ParameterEnum.IDB_ENTITY.ToString(), pIdB_Entity);
                    dbParametersValue.Add(DataParameter.ParameterEnum.IDC.ToString(), m_ProcessParameter.Currency);
                    dbParametersValue.Add(DataParameter.ParameterEnum.DT.ToString(), m_ProcessParameter.ProcessDate);
                    dbParametersValue.Add(DataParameter.ParameterEnum.AMOUNTTYPE.ToString(), pInterestTypeEnum.ToString());
                    //dbParametersValue.Add(DataParameter.ParameterEnum.STREAMNO.ToString(), nbstream);
                    previousTrade = DataContractLoad<InterestTrade>.LoadData(connection, dbParametersValue, DataContractResultSets.PREVIOUSTRADE_CBI);
                    dbParametersValue.Clear();
                    if (previousTrade.Count > 0)
                    {
                        pIdT = Convert.ToInt32(previousTrade.First().IdT);
                    }
                }
            }
            catch (Exception ex)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                // Une erreur s'est produite lors de la recherche d'un calcul précédent
                SpheresException2 sEx = new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-04503", ex,
                    m_ProcessParameter.ProcessDateISO, m_ProcessParameter.Identifier_Cbo, m_ProcessParameter.Currency, m_ProcessParameter.AmountType);

                // FI 20200623 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(sEx);
                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4503), 0,
                    new LogParam(m_ProcessParameter.ProcessDateISO),
                    new LogParam(m_ProcessParameter.Identifier_Cbo),
                    new LogParam(m_ProcessParameter.Currency),
                    new LogParam(m_ProcessParameter.AmountType)));
            }
            return codeReturn;
        }
        /// <summary>
        /// Génère le trade
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pIdT_Source"></param>
        /// <returns></returns>
        /// EG 20140310 Add pProcessBase parameter
        /// FI 20140930 [XXXXX] Modify 
        // EG 20180205 [23769]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private Cst.ErrLevel CreateTradeCashBalanceInterest(string pCS, CashBalanceInterestTradeInfo pTradeInfo, int pIdT_Source)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                // RD 20170421 [23094] 
                if (pTradeInfo.SqlEntity.Id != pTradeInfo.SqlInterestBook.IdA_Entity)
                {
                    SQL_Actor sqlBookEntity = new SQL_Actor(CSTools.SetCacheOn(pCS), pTradeInfo.SqlInterestBook.IdA_Entity);
                    sqlBookEntity.LoadTable();

                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                        StrFunc.AppendFormat(
@"<b>Calculation of cash interest not made.</b>
<b>Cause:</b> The processing entity is different from the Book accounting entity.
<b>Action:</b> Please specify the correct accounting entity on the Book and re-launch the process.
<b>Details:</b>
- Office: <b>{0}</b>
- Book: <b>{1}</b>
- Book accounting entity: <b>{2}</b>
- Processing entity: <b>{3}</b>
- Trade date: <b>{4}</b>",
                     LogTools.IdentifierAndId(pTradeInfo.SqlInterestActor.Identifier, pTradeInfo.SqlInterestActor.Id),
                     LogTools.IdentifierAndId(pTradeInfo.SqlInterestBook.Identifier, pTradeInfo.SqlInterestBook.Id),
                     LogTools.IdentifierAndId(sqlBookEntity.Identifier, sqlBookEntity.Id),
                     LogTools.IdentifierAndId(pTradeInfo.SqlEntity.Identifier, pTradeInfo.SqlEntity.Id),
                     DtFunc.DateTimeToStringDateISO(pTradeInfo.TradeDate)));
                }

                
                User user = new User(this.Session.IdA, null, RoleActor.SYSADMIN);
                CaptureSessionInfo sessionInfo = new CaptureSessionInfo
                {
                    user = user,
                    session = Session,
                    licence = License,
                    idProcess_L = IdProcess,
                    idTracker_L = Tracker.IdTRK_L
                };

                //FI 20140930 [XXXXX] utilisation du menu InputTradeRisk_CashInterest
                string idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeRisk_CashInterest);
                InputUser inputUser = new InputUser(idMenu, user);
                inputUser.InitializeFromMenu(CSTools.SetCacheOn(pCS));
                inputUser.InitCaptureMode();

                SQL_Instrument sqlInstrument =
                new SQL_Instrument(
                    CSTools.SetCacheOn(pCS), Cst.ProductCashBalanceInterest, SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.No, null, string.Empty);

                bool findInstr = sqlInstrument.LoadTable(new string[] { "IDI,IDENTIFIER" });
                if (false == findInstr)
                    throw new NotSupportedException(StrFunc.AppendFormat("Instrument {0} not found", Cst.ProductCashBalanceInterest));

                string screenName = string.Empty;

                if (pIdT_Source > 0)
                {
                    inputUser.CaptureMode = Cst.Capture.ModeEnum.Update;
                }
                else
                {
                    SearchInstrumentGUI searchInstrumentGUI = new SearchInstrumentGUI(sqlInstrument.Id);
                    StringData[] data = searchInstrumentGUI.GetDefault(CSTools.SetCacheOn(pCS), false);

                    if (ArrFunc.IsEmpty(data))
                        throw new NotSupportedException(StrFunc.AppendFormat("Screen or template not found for Instrument {0}", sqlInstrument.Identifier));

                    screenName = ((StringData)ArrFunc.GetFirstItem(data, "SCREENNAME")).value;
                    string templateIdentifier = ((StringData)ArrFunc.GetFirstItem(data, "TEMPLATENAME")).value;

                    pIdT_Source = TradeRDBMSTools.GetTradeIdT(CSTools.SetCacheOn(pCS), templateIdentifier);
                }

                if (pIdT_Source == 0)
                    throw new NotSupportedException("Trade Source not found");

                #region Create new trade ou update existing trade
                TradeRiskCaptureGen captureGen = new TradeRiskCaptureGen();
                string csLoad = pCS;
                if (inputUser.CaptureMode == Cst.Capture.ModeEnum.New)
                    csLoad = CSTools.SetCacheOn(csLoad);

                bool isFound =
                    captureGen.Load(csLoad, null,
                    pIdT_Source.ToString(), SQL_TableWithID.IDType.Id, inputUser.CaptureMode,
                    user, this.Session.SessionId, false);

                if (false == isFound)
                    throw new NotSupportedException(StrFunc.AppendFormat("<b>trade [idT:{0}] not found</b>", pIdT_Source));

                if (inputUser.CaptureMode == Cst.Capture.ModeEnum.Update)
                    screenName = captureGen.TradeCommonInput.SQLLastTradeLog.ScreenName;
                //
                captureGen.InitBeforeCaptureMode(pCS, null, inputUser, sessionInfo);

                // DataDocument issu du template
                DataDocumentContainer dataDoc = captureGen.TradeCommonInput.DataDocument;
                SetDataDocument(pCS, captureGen.TradeCommonInput.DataDocument, pTradeInfo);

                if (inputUser.CaptureMode == Cst.Capture.ModeEnum.New)
                {
                    //En création: Spheres® ecrase systématiquement le StatusEnvironment issu du template par REGULAR
                    captureGen.TradeCommonInput.TradeStatus.stEnvironment.CurrentSt = Cst.StatusEnvironment.REGULAR.ToString();
                }
                //
                //Alimentation des partyNotification (avec messagerie)
                captureGen.TradeCommonInput.SetTradeNotification(true);

                TradeRecordSettings recordSettings = new TradeRecordSettings
                {
                    displayName = $"Cash-Balance-Interest [{DtFunc.DateTimeToStringDateISO(pTradeInfo.TradeDate)}/{m_ProcessParameter.Identifier_Cbo}]",
                    description = $"Cash-Balance-Interest [{DtFunc.DateTimeToStringDateISO(pTradeInfo.EffectiveDate)}/{DtFunc.DateTimeToStringDateISO(pTradeInfo.TradeDate)}/{m_ProcessParameter.Identifier_Cbo}]",
                    extLink = string.Empty,
                    idScreen = screenName,

                    isGetNewIdForIdentifier = true,
                    //FI 20121031 [] isCheckValidationRules et isCheckValidationXSD est désormais à false pour gagner en performance
                    isCheckValidationRules = false,
                    isCheckValidationXSD = false,
                    // RD 20121031 ne pas vérifier la license pour les services pour des raisons de performances
                    isCheckLicense = false
                };


                //FI 20140930 [XXXXX] add inputUser.IdMenu

                TradeCommonCaptureGen.ErrorLevel lRet = RiskPerformanceTools.RecordTradeRisk(
                    pCS, null, captureGen, recordSettings, sessionInfo, inputUser.CaptureMode, inputUser.IdMenu,
                    ProcessState.SetErrorWarning, ProcessState.AddCriticalException,
                    LogLevelDetail.LEVEL4, LogTools.AddAttachedDoc);

                if (lRet == TradeCommonCaptureGen.ErrorLevel.SUCCESS)
                {
                    
                    //string message;
                    SysMsgCode message;
                    if (inputUser.CaptureMode == Cst.Capture.ModeEnum.New)
                    {
                        //message = "LOG-04540";
                        message = new SysMsgCode(SysCodeEnum.LOG, 4540);
                    }
                    else if (inputUser.CaptureMode == Cst.Capture.ModeEnum.Update)
                    {
                        //message = "LOG-04541";
                        message = new SysMsgCode(SysCodeEnum.LOG, 4541);
                    }
                    else
                        throw new NotImplementedException(StrFunc.AppendFormat("CaptureMode {0} is not implemented", inputUser.CaptureMode.ToString()));

                    
                    Logger.Log(new LoggerData(LogLevelEnum.None, message, 0,
                        new LogParam(LogTools.IdentifierAndId(captureGen.TradeCommonInput.Identification.Identifier, captureGen.TradeCommonInput.Identification.OTCmlId))));
                }
                else
                {
                    ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.ParseCodeReturn(lRet.ToString()));
                }
                pIdT_Source = captureGen.TradeCommonInput.Identification.OTCmlId;
                #endregion
                #region  Events Generation
                if (EventGenModeEnum.Internal == this.m_EventGenMode)
                {
                    //Generation of Events
                    string tradeIdentifier = TradeRDBMSTools.GetTradeIdentifier(pCS, pIdT_Source);

                    MQueueAttributes mQueueAttributes = new MQueueAttributes()
                    {
                        connectionString = pCS,
                        id = pIdT_Source,
                        idInfo = new IdInfo()
                        {
                            id = pIdT_Source,
                            idInfos = new DictionaryEntry[]{
                                                    new DictionaryEntry("ident", "TRADE"),
                                                    new DictionaryEntry("identifier", tradeIdentifier),
                                                    new DictionaryEntry("GPRODUCT", Cst.ProductGProduct_RISK)}
                        },
                        requester = MQueue.header.requester
                    };

                    EventsGenMQueue eventsGenMQueue = new EventsGenMQueue(mQueueAttributes);
                    ProcessState processState = New_EventsGenAPI.ExecuteSlaveCall(eventsGenMQueue, null, this, true);
                    if (ProcessStateTools.IsCodeReturnSuccess(processState.CodeReturn))
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4060), 0));
                    }
                    else
                    {
                        codeReturn = processState.CodeReturn;
                    }
                }
                else if (EventGenModeEnum.External == this.m_EventGenMode)
                {

                    //Send message queue
                    Boolean isDelEvent = false; // FI CheckAndRecord se charge déjà de faire le delete si Update
                    int idt = pIdT_Source;
                    EventsGenMQueue eventsGenMQueue = CaptureTools.GetMQueueForEventProcess(pCS, idt, isDelEvent, MQueue.header.requester);
                    if (null != eventsGenMQueue)
                    {
                        MQueueSendInfo sendInfo = EFS.SpheresService.ServiceTools.GetMqueueSendInfo(Cst.ProcessTypeEnum.EVENTSGEN, AppInstance);
                        if (false == sendInfo.IsInfoValid)
                            throw new Exception("MOM information unavailable for Spheres® EventsGen service");
                        MQueueTools.Send(eventsGenMQueue, sendInfo);

                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4061), 0));
                    }
                }
                else
                {
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", m_EventGenMode.ToString()));
                }
                #endregion  Event Gen

            }
            catch (Exception ex)
            {
                codeReturn = Cst.ErrLevel.FAILURE;

                // FI 20200623 [XXXXX] AddException
                ProcessState.AddException(ex);
                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
            }
            return codeReturn;
        }

        /// <summary>
        /// Construction du DataDocument
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataDoc"></param>
        /// <param name="pTradeInfo"></param>
        private void SetDataDocument(string pCS, DataDocumentContainer pDataDoc, CashBalanceInterestTradeInfo pTradeInfo)
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Ajout des parties
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            //Suppression des parties existantes
            pDataDoc.RemoveParty();
            //
            IParty partyActor = pDataDoc.AddParty(pTradeInfo.SqlInterestActor);
            IPartyTradeIdentifier partyActorTradeIdentifier = pDataDoc.AddPartyTradeIndentifier(partyActor.Id);
            if ((pTradeInfo.IdB_Interest != 0) && pTradeInfo.SqlInterestBook != default)
            {
                Tools.SetBookId(partyActorTradeIdentifier.BookId, pTradeInfo.SqlInterestBook);
                partyActorTradeIdentifier.BookIdSpecified = true;
            }
            //                    
            IParty partyEntity = pDataDoc.AddParty(pTradeInfo.SqlEntity);
            if ((pTradeInfo.IdB_Entity != 0) && pTradeInfo.SqlEntityBook != default)
            {
                IPartyTradeIdentifier partyEntityTradeIdentifier = pDataDoc.AddPartyTradeIndentifier(partyEntity.Id);
                Tools.SetBookId(partyEntityTradeIdentifier.BookId, pTradeInfo.SqlEntityBook);
                partyEntityTradeIdentifier.BookIdSpecified = true;
                partyEntityTradeIdentifier.TradeIdSpecified = false;
            }
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Trade Date
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            ITradeHeader tradeHeader = pDataDoc.TradeHeader;
            tradeHeader.TradeDate.Value = pDataDoc.TradeHeader.ClearedDate.Value;
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cash Balance Interest
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            EfsML.v30.CashBalanceInterest.CashBalanceInterest cashBalanceInterest = (EfsML.v30.CashBalanceInterest.CashBalanceInterest)pDataDoc.CurrentProduct.Product;
            cashBalanceInterest.interestAmountType = pTradeInfo.InterestTypeEnum;
            cashBalanceInterest.entityPartyReference = new PartyReference(pTradeInfo.SqlEntity.XmlId);


            if (InterestAmountTypeEnum.CashCoveredInitialMargin == cashBalanceInterest.interestAmountType)
            {
                CashBalanceInterestStream stream = cashBalanceInterest.cashBalanceInterestStream[0];
                stream.Id = InterestAmountTypeEnum.CashCoveredInitialMargin.ToString();
                SetDataDocumentStream(pCS, stream, pTradeInfo, InterestAmountTypeEnum.CashCoveredInitialMargin);
            }
            else
            {
                bool isExistCredit = pTradeInfo.Amounts.Exists(a => a.Amount.DecValue > 0);
                bool isExistDebit = pTradeInfo.Amounts.Exists(a => a.Amount.DecValue < 0);
                List<CashBalanceInterestStream> streamList = new List<CashBalanceInterestStream>();
                if (isExistCredit)
                {
                    CashBalanceInterestStream creditStream = cashBalanceInterest.cashBalanceInterestStream[0];
                    creditStream.Id = InterestAmountTypeEnum.CreditCashBalance.ToString();
                    SetDataDocumentStream(pCS, creditStream, pTradeInfo, InterestAmountTypeEnum.CreditCashBalance);
                    streamList.Add(creditStream);
                }
                if (isExistDebit)
                {
                    CashBalanceInterestStream debitStream = ((streamList.Count == 0) ? cashBalanceInterest.cashBalanceInterestStream[0] : new CashBalanceInterestStream());
                    debitStream.Id = InterestAmountTypeEnum.DebitCashBalance.ToString();
                    SetDataDocumentStream(pCS, debitStream, pTradeInfo, InterestAmountTypeEnum.DebitCashBalance);
                    streamList.Add(debitStream);
                }
                cashBalanceInterest.cashBalanceInterestStream = streamList.ToArray();
            }
        }

        /// <summary>
        /// Construction d'un Stream du DataDocument
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pStream"></param>
        /// <param name="pTradeInfo"></param>
        /// <param name="pInterestTypeEnum"></param>
        private void SetDataDocumentStream(
            string pCS,
            CashBalanceInterestStream pStream,
            CashBalanceInterestTradeInfo pTradeInfo,
            InterestAmountTypeEnum pInterestTypeEnum)
        {
            string businessCenter = null;
            // Lire les règles de calcul des intérêts pour le type de montant courant.
            InterestRule rule = pTradeInfo.GetInterestRule(pInterestTypeEnum);
            if (rule != default)
            {
                rule.SetSqlRateIndex(pCS);
                businessCenter = rule.BusinessCenter;
                // PM 20130819 [18582] Gestion du seuil minimum
                if (rule.MinimumThreshold.HasValue)
                {
                    Money thresholdAmount = new Money(rule.MinimumThreshold.Value, rule.Currency);
                    pStream.minimumThresholdSpecified = true;
                    pStream.minimumThreshold = thresholdAmount;
                }
                else
                {
                    pStream.minimumThresholdSpecified = false;
                }
            }
            BusinessCenter[] bc = new BusinessCenter[1] { new BusinessCenter(businessCenter) };
            BusinessCenters bcs = new BusinessCenters(bc);

            if (pInterestTypeEnum == InterestAmountTypeEnum.DebitCashBalance)
            {
                pStream.payerPartyReference = new PartyOrAccountReference(pTradeInfo.SqlInterestActor.XmlId);
                pStream.receiverPartyReference = new PartyOrAccountReference(pTradeInfo.SqlEntity.XmlId);
            }
            else
            {
                pStream.payerPartyReference = new PartyOrAccountReference(pTradeInfo.SqlEntity.XmlId);
                pStream.receiverPartyReference = new PartyOrAccountReference(pTradeInfo.SqlInterestActor.XmlId);
            }

            // Calculation Period Dates
            string calcPeriodDatesId = "calculationPeriodDates" + pInterestTypeEnum.ToString();
            CalculationPeriodDates calculationPeriodDates = pStream.calculationPeriodDates;
            calculationPeriodDates.Id = calcPeriodDatesId;
            calculationPeriodDates.effectiveDateAdjustable.unadjustedDate.DateValue = pTradeInfo.EffectiveDate;
            calculationPeriodDates.effectiveDateAdjustableSpecified = true;
            calculationPeriodDates.effectiveDateAdjustable.dateAdjustments.businessDayConvention = BusinessDayConventionEnum.NONE;
            calculationPeriodDates.terminationDateAdjustable.unadjustedDate.DateValue = pTradeInfo.TerminationDate;
            calculationPeriodDates.terminationDateAdjustableSpecified = true;
            calculationPeriodDates.terminationDateAdjustable.dateAdjustments.businessDayConvention = BusinessDayConventionEnum.NONE;
            calculationPeriodDates.calculationPeriodDatesAdjustments = new BusinessDayAdjustments(BusinessDayConventionEnum.NONE, null);
            calculationPeriodDates.calculationPeriodFrequency = new CalculationPeriodFrequency(PeriodEnum.D, 1, RollConventionEnum.NONE);

            // Payment Dates
            PaymentDates paymentDates = pStream.paymentDates;
            paymentDates.Id = "paymentDates" + pInterestTypeEnum.ToString();
            paymentDates.paymentDatesDateReferenceCalculationPeriodDatesReference.href = calcPeriodDatesId;
            paymentDates.paymentDatesDateReferenceCalculationPeriodDatesReferenceSpecified = true;
            paymentDates.paymentFrequency = new Interval(PeriodEnum.T, 1);
            paymentDates.payRelativeTo = PayRelativeToEnum.CalculationPeriodEndDate;
            paymentDates.paymentDaysOffset = new Offset(PeriodEnum.D, 0, DayTypeEnum.Business);
            paymentDates.paymentDaysOffsetSpecified = true;
            paymentDates.paymentDatesAdjustments = new BusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING, bcs);

            // Calculation Period Amount
            CalculationPeriodAmountBase calculationPeriodAmount = pStream.calculationPeriodAmount;
            // Notional Steps Schedule
            CashInterestAmount firstAmount = pTradeInfo.Amounts.FirstOrDefault(a => a.ValueDate == pTradeInfo.EffectiveDate);
            if (firstAmount != default(CashInterestAmount))
            {
                // PM 20151109 / RD 20151109 [21186] Prendre le notional initial en fonction du signe et du stream
                // - Pour les DebitCashBalance ne prendre que les flux négatifs
                // - Pour les autres types de flux, prendre les flux positifs
                //calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional.notionalStepSchedule.initialValue = firstAmount.amount;
                EFS_Decimal firstNotional =
                    new EFS_Decimal((((pInterestTypeEnum == InterestAmountTypeEnum.DebitCashBalance) && (firstAmount.amount.DecValue < 0))
                                  || ((pInterestTypeEnum != InterestAmountTypeEnum.DebitCashBalance) && (firstAmount.amount.DecValue >= 0)))
                                  ? System.Math.Abs(firstAmount.amount.DecValue) : 0);
                //
                calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional.notionalStepSchedule.initialValue = firstNotional;
                calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional.notionalStepSchedule.currency = firstAmount.currency;

            }
            // Création des notional steps :
            // - Pour les DebitCashBalance ne prendre que les flux négatifs
            // - Pour les autres types de flux, prendre les flux positifs

            // RD 20151106 [21186] Considérer une seule ligne par Date                 
            //Step[] notionalSteps = (from amount in pTradeInfo.Amounts
            //                        where (amount.ValueDate != pTradeInfo.EffectiveDate)                                    
            //                        select new Step
            //                        {
            //                            stepDate = new EFS_Date(DtFunc.DateTimeToStringDateISO(amount.ValueDate)),
            //                            stepValue = new EFS_Decimal((((pInterestTypeEnum == InterestAmountTypeEnum.DebitCashBalance) && (amount.amount.DecValue < 0))
            //                                                      || ((pInterestTypeEnum != InterestAmountTypeEnum.DebitCashBalance) && (amount.amount.DecValue >= 0)))
            //                                                      ? System.Math.Abs(amount.amount.DecValue) : 0),
            //                        }
            //                       ).ToArray();
            Step[] notionalSteps =
                (from amountByDate in
                     (from amount in pTradeInfo.Amounts
                      where (amount.ValueDate != pTradeInfo.EffectiveDate)
                      select amount).GroupBy(item => item.ValueDate)
                 select new Step
                 {
                     stepDate = new EFS_Date(DtFunc.DateTimeToStringDateISO(amountByDate.Key)),
                     stepValue = new EFS_Decimal((((pInterestTypeEnum == InterestAmountTypeEnum.DebitCashBalance) && (amountByDate.First().amount.DecValue < 0))
                         || ((pInterestTypeEnum != InterestAmountTypeEnum.DebitCashBalance) && (amountByDate.First().amount.DecValue >= 0)))
                         ? System.Math.Abs(amountByDate.First().amount.DecValue) : 0),
                 }
                 ).ToArray();
            // S'il existe au moins un nominal != 0
            if ((firstAmount.Amount.DecValue != 0) || notionalSteps.Any(ns => ns.stepValue.DecValue != 0))
            {
                // Ajout des notional steps
                calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional.notionalStepSchedule.step = notionalSteps;
                calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional.notionalStepSchedule.stepSpecified = true;
            }
            // Rate
            if (rule != default)
            {
                calculationPeriodAmount.calculationPeriodAmountCalculation.dayCountFraction = rule.DayCountFractionEnum;
                if (rule.Fixedrate != null)
                {
                    calculationPeriodAmount.calculationPeriodAmountCalculation.rateFixedRate.initialValue = new EFS_Decimal((decimal)rule.Fixedrate);
                    calculationPeriodAmount.calculationPeriodAmountCalculation.rateFixedRateSpecified = true;
                    calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRateSpecified = false;
                    pStream.resetDatesSpecified = false;
                }
                else if (rule.IdAsset != null)
                {
                    // Reset Dates
                    string resetDatesId = "resetDates" + pInterestTypeEnum.ToString();
                    ResetDates resetDates = pStream.resetDates;
                    resetDates.Id = resetDatesId;
                    resetDates.calculationPeriodDatesReference.href = calcPeriodDatesId;
                    resetDates.resetRelativeTo = ResetRelativeToEnum.CalculationPeriodStartDate;
                    resetDates.resetRelativeToSpecified = true;
                    resetDates.fixingDates = new RelativeDateOffset();
                    if ((rule.SqlInterestRateIndex != default) && (rule.SqlInterestRateIndex.IsLoaded))
                    {
                        SQL_RateIndex rateIndex = rule.SqlInterestRateIndex;
                        resetDates.fixingDates.periodMultiplier = new EFS_Integer(rateIndex.PeriodMlptFixingOffset);
                        resetDates.fixingDates.period = (PeriodEnum)StringToEnum.Parse(rateIndex.PeriodFixingOffset, PeriodEnum.D);
                        resetDates.fixingDates.dayType = rateIndex.FpML_Enum_DayTypeFixingOffset;
                        resetDates.fixingDates.businessDayConvention = rateIndex.FpML_Enum_CalcPeriodBusinessDayConvention;
                    }
                    else
                    {
                        resetDates.fixingDates.periodMultiplier = new EFS_Integer(0);
                        resetDates.fixingDates.period = PeriodEnum.D;
                        resetDates.fixingDates.dayType = DayTypeEnum.Business;
                        resetDates.fixingDates.businessDayConvention = BusinessDayConventionEnum.NONE;
                    }
                    resetDates.fixingDates.dayTypeSpecified = true;
                    resetDates.fixingDates.businessCentersDefine = bcs;
                    resetDates.fixingDates.businessCentersDefineSpecified = true;
                    resetDates.fixingDates.dateRelativeTo.href = resetDatesId;
                    //
                    resetDates.resetFrequency = new ResetFrequency
                    {
                        periodMultiplier = new EFS_Integer(1),
                        period = PeriodEnum.D
                    };
                    resetDates.resetDatesAdjustments = new BusinessDayAdjustments(BusinessDayConventionEnum.NONE, null);
                    pStream.resetDatesSpecified = true;
                    // Floating rate
                    calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.floatingRateIndex.OTCmlId = (int)rule.IdAsset;
                    calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.floatingRateIndex.Value = rule.AssetIdentifier;
                    calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.indexTenor = new Interval(PeriodEnum.D, 1);
                    calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.indexTenorSpecified = true;
                    calculationPeriodAmount.calculationPeriodAmountCalculation.rateFixedRateSpecified = false;
                    calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRateSpecified = true;
                    // CC 20161012 [22532] Ajout ISAPPLYNEGATIVEINT - Gestion des taux négatifs
                    //calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.negativeInterestRateTreatment = NegativeInterestRateTreatmentEnum.ZeroInterestRateMethod;
                    if (rule.IsApplyNegativeInt == true)
                    {   
                        calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.negativeInterestRateTreatment = NegativeInterestRateTreatmentEnum.NegativeInterestRateMethod;
                    }
                    else
                    {
                        calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.negativeInterestRateTreatment = NegativeInterestRateTreatmentEnum.ZeroInterestRateMethod;
                    }
                    calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.negativeInterestRateTreatmentSpecified = true;
                    if (rule.Spread != null)
                    {
                        calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.spreadSchedule[0].initialValue = new EFS_Decimal((decimal)rule.Spread);
                        calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.spreadScheduleSpecified = true;
                    }
                    if (rule.Multiplier != null)
                    {
                        calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.floatingRateMultiplierSchedule.initialValue = new EFS_Decimal((decimal)rule.Multiplier);
                        calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.floatingRateMultiplierScheduleSpecified = true;
                    }
                }
            }
            calculationPeriodAmount.calculationPeriodAmountCalculationSpecified = true;
        }
        #endregion methods
    }
}

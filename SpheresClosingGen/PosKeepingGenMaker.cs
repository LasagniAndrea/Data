#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML.Business;
using EfsML.Interface;
//
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
#endregion Using Directives

namespace EFS.Process.PosKeeping
{
    /// <summary>
    /// Utilisée exclusivement pour les traitements EOD|CLOSINGDAY
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>► Pour un couple ENTITE/CSS|CUSTODIAN donné (IDA_ENTITY et IDA_CSSCUSTODIAN de POSREQUEST)</para>
    ///<para>  ● Chargement des lignes de la table ENTITYMARKET correspondante</para>
    ///<para>  ● Pour chaque ligne ENTITYMARKET rencontrée</para>
    ///<para>    1 ● Instanciation de la bonne classe de traitement fonction du groupe de produitContrôle traitement de fin de journée correctement effectué et complet</para>
    ///<para>        PosKeepingGen_COM, PosKeepingGen_ETD, PosKeepingGen_SEC, etc.</para>
    ///<para>        via Lecture GPRODUCT  (TRADEINSTRUMENT|INSTRUMENT)</para>
    ///<para>        => Exemple : une chambre (ECC) peut traiter plusieurs groupe de produits (ETD et COM)</para>
    ///<para>    2 ● Traitement EOD|CLOSINGDAY</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    /// </summary>
    /// FI 20170406 [23053] Modify
    public class PosKeepingGenMaker : PosKeepingGenProcessBase
    {
        #region Constructors
        public PosKeepingGenMaker(PosKeepingGenProcess pPosKeepingGenProcess)
            : base(pPosKeepingGenProcess)
		{
		}
        #endregion Constructors

        #region Generate
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without margin Requirement(Cst.PosRequestTypeEnum.EndOfDayWithoutMR)
        public override Cst.ErrLevel Generate()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;
            PosKeepingRequestMQueue requestQueue = Queue as PosKeepingRequestMQueue;
            try
            {
                IProductBase product = Tools.GetNewProductBase();

                if (m_PKGenProcess.IsCreatedByNormalizedMessageFactory)
                {
                    m_MasterPosRequest = PosKeepingTools.CreateScheduledPosRequest(CS, null, requestQueue, product, m_PKGenProcess.Session);
                    m_PKGenProcess.Tracker.UpdateIdData("POSREQUEST", m_MasterPosRequest.IdPR);
                    m_PKGenProcess.CurrentId = m_MasterPosRequest.IdPR;
                    m_PKGenProcess.LockObjectId(m_PKGenProcess.TypeLock, m_MasterPosRequest.IdPR.ToString(), requestQueue.identifier, requestQueue.ProcessType.ToString(), LockTools.Exclusive);
                }
                else
                {
                    m_MasterPosRequest = PosKeepingTools.GetPosRequest(CS, product, requestQueue.id);
                    string tradeIdentifier = requestQueue.GetStringValueIdInfoByKey("TRADE");
                    if (StrFunc.IsFilled(tradeIdentifier))
                        m_MasterPosRequest.SetIdentifiers(tradeIdentifier);
                }

                if (null != m_MasterPosRequest)
                {
                    m_PosRequest = RestoreMasterRequest;
                    codeReturn = LockProcessByRequest();

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        // STEP 0 : INITIALIZE / IN PROGRESS
                        m_MasterPosRequest.StatusSpecified = true;
                        m_MasterPosRequest.Status = ProcessStateTools.StatusProgressEnum;
                        
                        //PosKeepingTools.UpdatePosRequest(CS, null, m_MasterPosRequest.idPR, m_MasterPosRequest, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, null);
                        PosKeepingTools.UpdatePosRequest(CS, null, m_MasterPosRequest.IdPR, m_MasterPosRequest, m_PKGenProcess.Session.IdA, IdProcess, null);

                        switch (m_MasterPosRequest.RequestType)
                        {
                            case Cst.PosRequestTypeEnum.EndOfDay:
                            case Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin:
                                codeReturn = EOD_Gen();
                                break;
                            case Cst.PosRequestTypeEnum.ClosingDay:
                                codeReturn = CLOSINGDAY_Gen();
                                break;
                            default:
                                codeReturn = Cst.ErrLevel.DATANOTFOUND;
                                break;
                        }

                        if (m_MasterPosRequest.IdEMSpecified)
                        {
                            m_PKGenProcess.UnLockObjectId(m_MasterPosRequest.IdEM);
                        }
                        else
                        {
                            if (null != m_LockIdEM)
                            {
                                m_LockIdEM.ForEach
                                    (
                                        _lock =>
                                        {
                                            foreach (LockObject item in m_PKGenProcess.LockObject)
                                            {
                                                // FI 20170406 [23053] Add restriction ENTITYMARKET
                                                if (item.ObjectId == _lock.ToString() && (item.ObjectType == TypeLockEnum.ENTITYMARKET))
                                                    LockTools.UnLock(CS, item, m_PKGenProcess.Session.SessionId);
                                            }
                                            // FI 20170406 [23053] Mise en commentaire => Trop tôt pour faire un lockObject.Clear()
                                            //m_PKGenProcess.lockObject.Clear();
                                        }
                                    );
                            }
                            // FI 20170406 [23053] Appel ici
                            m_PKGenProcess.LockObject.Clear();
                        }
                    }
                }
                else
                {
                    codeReturn = Cst.ErrLevel.DATANOTFOUND;

                    ProcessStateTools.StatusEnum status = ProcessStateTools.StatusErrorEnum;
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_PKGenProcess.ProcessState.SetErrorWarning(status);

                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5102), 0,
                        new LogParam(LogTools.IdentifierAndId(GetPosRequestLogValue(m_PosRequest.RequestType), requestQueue.id)),
                        new LogParam(requestQueue.GetStringValueIdInfoByKey("DTBUSINESS"))));
                }
            }
            catch (Exception ex)
            {
                // FI 20200918 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                Common.AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));

                codeReturn = Cst.ErrLevel.FAILURE;
                /* FI 20200623 [XXXXX] 
                  m_PKGenProcess.ProcessState.LastSpheresException = SpheresExceptionParser.GetSpheresException(string.Empty, ex);
                  m_PKGenProcess.CodeReturn = m_PKGenProcess.SetCodeReturnFromLastException();
                */

                // FI 20200623 [XXXXX] call AddException
                m_PKGenProcess.ProcessState.AddException(ex);

                // FI 20200623 [XXXXX] call SetCodeReturnFromLastException2
                m_PKGenProcess.ProcessState.SetCodeReturnFromLastException2();

                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                    new LogParam(LogTools.IdentifierAndId(GetPosRequestLogValue(m_PosRequest.RequestType), requestQueue.id)),
                    new LogParam(requestQueue.GetStringValueIdInfoByKey("DTBUSINESS"))));

                // FI 20200812 pas de throw pour eviter double ajout dans le log  de l'exception
                //throw;
            }
            finally
            {
                if (ProcessStateTools.IsCodeReturnIRQExecuted(codeReturn))
                {
                    m_MasterPosRequest.Status = ProcessStateTools.StatusInterruptEnum;
                }
                else if ((null != m_LstMarketSubPosRequest) && (0 < m_LstMarketSubPosRequest.Count))
                {
                    m_MasterPosRequest.Status = PosKeepingTools.GetStatusGroupLevel(m_LstMarketSubPosRequest, m_MasterPosRequest.IdPR, ProcessStateTools.StatusUnknownEnum);
                    m_PKGenProcess.ProcessState.SetErrorWarning(m_MasterPosRequest.Status);
                }
                else if (false == ProcessStateTools.IsStatusProgress(m_PKGenProcess.ProcessState.Status))
                {
                    m_MasterPosRequest.Status = m_PKGenProcess.ProcessState.Status;
                }
                else
                {
                    m_MasterPosRequest.Status = (ProcessStateTools.IsCodeReturnSuccess(codeReturn) ? ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusErrorEnum);
                }
                m_MasterPosRequest.StatusSpecified = true;
                // FI 20170406 [23053] ne pas écraser codeReturn
                //codeReturn = UpdatePosRequestTerminated(m_MasterPosRequest.idPR, m_MasterPosRequest.status);
                UpdatePosRequestTerminated(m_MasterPosRequest.IdPR, m_MasterPosRequest.Status);
            }
            return codeReturn;
        }
        #endregion Generate

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // EOD
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region EOD_Gen
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180319 [23769] Async config
        // EG 20180413 [23769] Gestion customParallelConfigSource
        // EG 20180525 [23979] IRQ Processing
        // EG 20180620 Gestion Date de demande de traitement  différente de celle d'ENTITYMARKET (Message + Traitement en WARNING)
        // EG 20190308 New ClosingReopeningPosition
        // EG 20190812 [24825] Test Aucune activité sur CSS/CUSTODIAN
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without margin Requirement(Cst.PosRequestTypeEnum.EndOfDayWithoutMR)
        // EG 20240520 [WI930] Update InitializePosRequestByMarket
        private Cst.ErrLevel EOD_Gen()
        {
            Cst.ErrLevel mainCodeReturn = Cst.ErrLevel.SUCCESS;

            // LECTURE DES MARCHES et CONTROLE DTMARKET
            DataRowCollection rowEntityMarkets = ControlAndGetEntityMarkets(ref mainCodeReturn);

            if ((null != rowEntityMarkets) && (Cst.ErrLevel.SUCCESS == mainCodeReturn))
            {
                // Multiplier (4 =>  isCommoditySpot / IsTradedDerivative / isEquityMarket / isOverTheCounter)
                int nbRow = rowEntityMarkets.Count * 4;
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, nbRow + 1);
                if (0 < nbRow)
                {
                    // Chargement des contextes de merge (1 seule fois avant traitement par marché)
                    mainCodeReturn = LoadTradeMergeRules();

                    bool isInitialMargin = false;
                    // EG 20170510 [21153] Return SUCCESS si NoActivity
                    bool isNoActivity = false;

                    Cst.ErrLevel codeReturn;
                    if (Cst.ErrLevel.SUCCESS == mainCodeReturn)
                    {
                        m_LstMarketSubPosRequest = new List<IPosRequest>();
                        foreach (DataRow rowEntityMarket in rowEntityMarkets)
                        {
                            if ((Cst.ErrLevel.IRQ_EXECUTED == mainCodeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref mainCodeReturn))
                                break;

                            PosKeepingEntityMarket entityMarket = new PosKeepingEntityMarket(CS, rowEntityMarket);

                            // Calcul du déposit a lancer ?
                            isInitialMargin |= entityMarket.isInitialMargin;

                            if ((Cst.ErrLevel.IRQ_EXECUTED == mainCodeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref mainCodeReturn))
                                break;

                            // TradedDerivative : ETD
                            if (entityMarket.isExchangeTradedDerivative)
                            {

                                PosKeepingGen_ETD posKeepingGen_ETD = new PosKeepingGen_ETD(m_PKGenProcess);
                                posKeepingGen_ETD.SetListeSourceToTarget(this);
                                posKeepingGen_ETD.InitializePosRequestByMarket(m_MasterPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                                posKeepingGen_ETD.SetCurrentParallelSettings(entityMarket.entityIdentifier, entityMarket.cssCustodianIdentifier, entityMarket.market);

                                entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.ExchangeTradedDerivative;
                                codeReturn = EOD_MakerGen(posKeepingGen_ETD, entityMarket, newIdPR);
                                if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                                    mainCodeReturn = Cst.ErrLevel.IRQ_EXECUTED;
                                else if (Cst.ErrLevel.SUCCESS != codeReturn)
                                    mainCodeReturn = Cst.ErrLevel.FAILURE;
                                newIdPR++;
                            }

                            if ((Cst.ErrLevel.IRQ_EXECUTED == mainCodeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref mainCodeReturn))
                                break;

                            // CommoditySpot : COM
                            if (entityMarket.isCommodity)
                            {
                                PosKeepingGen_COM posKeepingGen_COM = new PosKeepingGen_COM(m_PKGenProcess);
                                posKeepingGen_COM.SetListeSourceToTarget(this);
                                posKeepingGen_COM.InitializePosRequestByMarket(m_MasterPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                                posKeepingGen_COM.SetCurrentParallelSettings(entityMarket.entityIdentifier, entityMarket.cssCustodianIdentifier, entityMarket.market);
                                entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.Commodity;
                                codeReturn = EOD_MakerGen(posKeepingGen_COM, entityMarket, newIdPR);
                                if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                                    mainCodeReturn = Cst.ErrLevel.IRQ_EXECUTED;
                                else if (Cst.ErrLevel.SUCCESS != codeReturn)
                                    mainCodeReturn = Cst.ErrLevel.FAILURE;
                                newIdPR++;
                            }

                            if ((Cst.ErrLevel.IRQ_EXECUTED == mainCodeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref mainCodeReturn))
                                break;

                            // MarkToMarket : MTM
                            if (entityMarket.isMarkToMarket)
                            {
                                PosKeepingGen_MTM posKeepingGen_MTM = new PosKeepingGen_MTM(m_PKGenProcess);
                                posKeepingGen_MTM.SetListeSourceToTarget(this);
                                posKeepingGen_MTM.InitializePosRequestByMarket(m_MasterPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                                posKeepingGen_MTM.SetCurrentParallelSettings(entityMarket.entityIdentifier, entityMarket.cssCustodianIdentifier, entityMarket.market);
                                entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.MarkToMarket;
                                codeReturn = EOD_MakerGen(posKeepingGen_MTM, entityMarket, newIdPR);
                                if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                                    mainCodeReturn = Cst.ErrLevel.IRQ_EXECUTED;
                                else if (Cst.ErrLevel.SUCCESS != codeReturn)
                                    mainCodeReturn = Cst.ErrLevel.FAILURE;
                                newIdPR++;
                            }

                            if ((Cst.ErrLevel.IRQ_EXECUTED == mainCodeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref mainCodeReturn))
                                break;

                            // OverTheCounter : OTC
                            if (entityMarket.isOverTheCounter)
                            {
                                PosKeepingGen_OTC posKeepingGen_OTC = new PosKeepingGen_OTC(m_PKGenProcess);
                                posKeepingGen_OTC.SetListeSourceToTarget(this);
                                posKeepingGen_OTC.InitializePosRequestByMarket(m_MasterPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                                posKeepingGen_OTC.SetCurrentParallelSettings(entityMarket.entityIdentifier, entityMarket.cssCustodianIdentifier, entityMarket.market);
                                entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.OverTheCounter;
                                codeReturn = EOD_MakerGen(posKeepingGen_OTC, entityMarket, newIdPR);
                                if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                                    mainCodeReturn = Cst.ErrLevel.IRQ_EXECUTED;
                                else if (Cst.ErrLevel.SUCCESS != codeReturn)
                                    mainCodeReturn = Cst.ErrLevel.FAILURE;
                                newIdPR++;
                            }

                            if ((Cst.ErrLevel.IRQ_EXECUTED == mainCodeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref mainCodeReturn))
                                break;

                            // EquityMarket : SEC
                            if (entityMarket.isSecurity)
                            {
                                PosKeepingGen_SEC posKeepingGen_SEC = new PosKeepingGen_SEC(m_PKGenProcess);
                                posKeepingGen_SEC.SetListeSourceToTarget(this);
                                posKeepingGen_SEC.InitializePosRequestByMarket(m_MasterPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                                posKeepingGen_SEC.SetCurrentParallelSettings(entityMarket.entityIdentifier, entityMarket.cssCustodianIdentifier, entityMarket.market);
                                entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.Security;
                                codeReturn = EOD_MakerGen(posKeepingGen_SEC, entityMarket, newIdPR);
                                if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                                    mainCodeReturn = Cst.ErrLevel.IRQ_EXECUTED;
                                else if (Cst.ErrLevel.SUCCESS != codeReturn)
                                    mainCodeReturn = Cst.ErrLevel.FAILURE;
                                newIdPR++;
                            }
                            if (entityMarket.IsNoActivity)
                            {
                                NoActivityOnMarket(Cst.PosRequestTypeEnum.EOD_MarketGroupLevel, entityMarket, newIdPR, m_MasterPosRequest.IdPR);
                                newIdPR++;
                            }
                            isNoActivity |= entityMarket.IsNoActivity;
                        }
                    }
                    // Calcul de deposit uniquement si les traitements de fin de journé des marchés est ok
                    m_PosRequest = PosKeepingTools.ClonePosRequest(m_MasterPosRequest);
                    // FI 20190717 [24785][24752] Cacul de deposit effectué même si des erreurs (y compris des exceptions) se sont produites aus étapes précédentes 
                    //if ((Cst.ErrLevel.SUCCESS == mainCodeReturn) && isInitialMargin)
                    // EG 20231129 [WI762] End of Day processing : Possibility to request processing without margin Requirement
                    if (isInitialMargin && m_PKGenProcess.IsRequestEndOfDayWithMR)
                    {
                        if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref mainCodeReturn))
                        {
                            codeReturn = EOD_InitialMarginGen();
                            if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                                mainCodeReturn = Cst.ErrLevel.IRQ_EXECUTED;
                            else if (Cst.ErrLevel.SUCCESS != codeReturn)
                                mainCodeReturn = Cst.ErrLevel.FAILURE;
                        }
                    }
                }
            }
            return mainCodeReturn;
        }
        #endregion EOD_Gen
        #region NoActivityOnMarket
        // EG 20190812 [24825] New - Test Aucune activité sur CSS/CUSTODIAN
        private void NoActivityOnMarket(Cst.PosRequestTypeEnum pPosRequestType, PosKeepingEntityMarket pEntityMarket, int pIdPR, int pIdPR_Parent)
        {
            IPosRequest _posRequest = InsertPosRequestGroupLevel(pPosRequestType, pIdPR, m_MasterPosRequest, pEntityMarket.idEM,
                ProcessStateTools.StatusProgressEnum, pIdPR_Parent, null, pEntityMarket.CurrentGroupProduct);
            _posRequest.IdentifiersSpecified = true;
            _posRequest.Identifiers = m_Product.CreatePosRequestKeyIdentifier();
            _posRequest.Identifiers.Entity = Queue.GetStringValueIdInfoByKey("ENTITY");
            _posRequest.Identifiers.CssCustodian = Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN");
            _posRequest.Identifiers.Market = pEntityMarket.marketShortAcronym;
            _posRequest.IdMSpecified = true;
            _posRequest.IdM = pEntityMarket.idM;

            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5035), (Cst.PosRequestTypeEnum.ClosingDay_EOD_MarketGroupLevel == pPosRequestType ? 1 : 0),
                new LogParam(LogTools.IdentifierAndId(_posRequest.Identifiers.Entity, _posRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(_posRequest.Identifiers.CssCustodian, _posRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(_posRequest.Identifiers.Market, _posRequest.IdM)),
                new LogParam(DtFunc.DateTimeToStringDateISO(_posRequest.DtBusiness))));
            Logger.Write();

            UpdatePosRequestGroupLevel(_posRequest, ProcessStateTools.StatusNoneEnum);
        }
        #endregion NoActivityOnMarket
        #region EOD_MakerGen
        /// EG 20170222 [22717] Move EOD_PriceControl to EOD_ControlGen (by Market)
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
        // EG 20240520 [WI930] Update InitializePosRequestByMarket
        private Cst.ErrLevel EOD_MakerGen(PosKeepingGenProcessBase pPosKeepingGenProcessBase, PosKeepingEntityMarket pEntityMarket, int pIdPR)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            m_LstSubPosRequest = new List<IPosRequest>();
            pPosKeepingGenProcessBase.InitializePosRequestByMarket(m_MasterPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
            // FI 20210802 [XXXXX] implémentation d'un try catch finally, dans finally appel à AddRangeSubPosRequest
            try
            {
                // TRAITEMENT DU MARCHE
                codeReturn = pPosKeepingGenProcessBase.EOD_MarketGen(pIdPR, pEntityMarket.idEM, pEntityMarket.marketShortAcronym, pEntityMarket.idM, pEntityMarket.CurrentGroupProduct);
                if ((Cst.ErrLevel.SUCCESS != codeReturn) && (Cst.ErrLevel.IRQ_EXECUTED != codeReturn))
                    codeReturn = Cst.ErrLevel.FAILURE;
            }
            catch { throw; }
            finally
            {
                AddRangeSubPosRequest(m_LstMarketSubPosRequest, pPosKeepingGenProcessBase.LstMarketSubPosRequest);
                AddRangeSubPosRequest(m_LstSubPosRequest, pPosKeepingGenProcessBase.LstSubPosRequest);
            }

            return codeReturn;
        }
        #endregion EOD_MakerGen

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // CLOSINGDAY 
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region CLOSINGDAY_Gen
        /// <summary>
        /// Méthode principale de traitement de clôture de journée
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Message de type PosKeepingRequestMQueue avec REQUESTYPE = CLOSINGDAY</para>
        ///<para>  ● Pour un couple ENTITE/CSS donné (IDA_ENTITY et IDA_CSS de POSREQUEST)</para>
        ///    
        ///<para>► Traitement séquentiel</para>
        ///<para>  1 ● Contrôle traitement de fin de journée correctement effectué et complet</para>
        ///<para>      pour chaque marché du couple ENTITE/CSS</para>
        ///<para>  2 ● Contrôle traitement de calcul de déposit correctement effectué</para>
        ///<para>  3 ● Clôture de journée (Changement des date dans la table ENTITYMARKET)</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
        ///</summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20170222 [22717] Delete AssetControl_EOD
        // EG 20171128 [23331] Upd Gestion CodeReturn
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
        // EG 20180620 Gestion Date de demande de traitement  différente de celle d'ENTITYMARKET (Message + Traitement en WARNING)
        // EG 20180803 PERF (ASSETCTRL_{buildTableId}_W sur la base de ASSETCTRL_MODEL à la place de ASSETCONTROL_EOD)
        // EG 20180803 PERF (TRADEASSETCTRL_{buildTableId}_W sur la base de TRADEASSETCTRL_MODEL -> SQLServer Table pour une unique évaluation dans la clause WITH)
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] DeadLock
        // EG 20240520 [WI930] m_LstEntityMarketInfoChangedAfterCRP + CLOSINGDAY_ValidationAfterCRP (Gestion de la validation sur la nouvelle CSS
        private Cst.ErrLevel CLOSINGDAY_Gen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            // INSERTION LOG
            
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5071), 0,
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));

            // STEP 0 : INITIALIZE / IN PROGRESS
            m_MasterPosRequest.StatusSpecified = true;
            m_MasterPosRequest.Status = ProcessStateTools.StatusProgressEnum;
            
            //PosKeepingTools.UpdatePosRequest(CS, null, m_MasterPosRequest.idPR, m_MasterPosRequest, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, null);
            PosKeepingTools.UpdatePosRequest(CS, null, m_MasterPosRequest.IdPR, m_MasterPosRequest, m_PKGenProcess.Session.IdA, IdProcess, null);

            DataRowCollection rowEntityMarkets = ControlAndGetEntityMarkets(ref codeReturn);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                ProcessStateTools.StatusEnum statusControl = ProcessStateTools.StatusEnum.NA;
                if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                {
                    if (null != rowEntityMarkets)
                    {
                        m_LstMarketSubPosRequest = new List<IPosRequest>();
                        m_LstEntityMarketInfoChangedAfterCRP = new List<IPosKeepingMarket>();
                        statusControl = CLOSINGDAY_Control(rowEntityMarkets);

                        if (false == ProcessStateTools.IsStatusSuccess(statusControl))
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(statusControl);

                            
                            Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(statusControl), new SysMsgCode(SysCodeEnum.LOG, 5084), 1,
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));
                        }
                    }
                }

                // Traitement de début de journée
                if (ProcessStateTools.IsStatusSuccess(statusControl) || (ProcessStateTools.IsStatusWarning(statusControl)))
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                    {
                        m_LstMarketSubPosRequest = new List<IPosRequest>();
                        codeReturn = CLOSINGDAY_EODFinalGen(rowEntityMarkets);
                        CLOSINGDAY_ValidationAfterCRP();
                    }

                    // VALIDATION
                    if ((Cst.ErrLevel.FAILURE != codeReturn) && (Cst.ErrLevel.IRQ_EXECUTED != codeReturn))
                        codeReturn = CLOSINGDAY_Validation();

                    if (codeReturn == Cst.ErrLevel.SUCCESS)
                    {
                        // EG 20190613 [24683] Test DEADLOCK
                        int guard = 0;
                        while (++guard < 5)
                        {
                            if (1 < guard)
                            {
                                ProcessStateTools.StatusEnum status = ProcessStateTools.StatusWarningEnum;
                                // FI 20200623 [XXXXX] Call SetErrorWarning
                                m_PKGenProcess.ProcessState.SetErrorWarning(status);

                                
                                Logger.Log(new LoggerData(LogLevelEnum.Warning, "AddCBOMARKET2 is reexecuted"));
                            }

                            Nullable<SQLErrorEnum> err = AddCBOMARKET2(CS, m_MasterPosRequest.IdA_Entity, m_MasterPosRequest.DtBusiness, m_PKGenProcess.Session);
                            if ((false == err.HasValue) || SQLErrorEnum.DuplicateKey == err)
                                break;
                        }
                    }
                }
                else if (ProcessStateTools.IsStatusInterrupt(statusControl))
                {
                    codeReturn = Cst.ErrLevel.IRQ_EXECUTED;
                }
                else if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
                {
                    // EG 20171128 [23331]
                    codeReturn = Cst.ErrLevel.FAILURE;
                }
            }

            return codeReturn;
        }
        #endregion CLOSINGDAY_Gen
        #region CLOSINGDAY_Control
        /// <summary>
        /// CONTROLE DE CLOTURE DE JOURNEE
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Message de type PosKeepingRequestMQueue avec REQUESTYPE = CLOSINGDAY</para>
        ///<para>  ● Pour un couple ENTITE/CSS donné (IDA_ENTITY et IDA_CSS de POSREQUEST)</para>
        ///    
        ///<para>► Traitement séquentiel</para>
        ///<para>  1 ● Contrôle traitement de fin de journée correctement effectué et complet</para>
        ///<para>      pour chaque marché du couple ENTITE/CSS</para>
        ///<para>  2 ● Contrôle traitement de calcul de déposit (si besoin) correctement effectué</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
        ///</summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20170223 New 
        /// EG 20170510 [21153] Return SUCCESS si NoActivity
        // EG 20171128 [23331] New signature to posKeepingGenProcessBase.EOD_MarketControl : add entityMarket.marketShortAcronym, entityMarket.idM
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190812 [24825] Test Aucune activité sur CSS/CUSTODIAN
        // EG 20240520 [WI930] Update InitializePosRequestByMarket
        private ProcessStateTools.StatusEnum CLOSINGDAY_Control(DataRowCollection pRowEntityMarkets)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            m_LstMarketSubPosRequest = new List<IPosRequest>();
            m_LstSubPosRequest = new List<IPosRequest>();
            SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
            // STEP 0 : INITIALISATION POSREQUEST REGROUPEMENT (CONTROLE)
            IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.ClosingDay_ControlGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                ProcessStateTools.StatusEnum.PROGRESS, m_PosRequest.IdPR, m_LstSubPosRequest, null);

            bool isInitialMargin = false;
            // EG 20170510 [21153] Return SUCCESS si NoActivity
            bool isNoActivity = false;

            // STEP 1 : CONTROLE EOD PAR MARCHE
            foreach (DataRow rowEntityMarket in pRowEntityMarkets)
            {
                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                    break;

                PosKeepingEntityMarket entityMarket = new PosKeepingEntityMarket(CS, rowEntityMarket);

                // Calcul du déposit a lancer ?
                isInitialMargin |= entityMarket.isInitialMargin;

                if (entityMarket.IsNoActivity)
                {
                    SQLUP.GetId(out newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                    NoActivityOnMarket(Cst.PosRequestTypeEnum.ClosingDayMarketGroupLevel, entityMarket, newIdPR, posRequestGroupLevel.IdPR);
                }
                else
                {
                    // INSERTION LOG
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5072), 1,
                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                        new LogParam(LogTools.IdentifierAndId(entityMarket.marketShortAcronym, entityMarket.idM)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));

                    SQLUP.GetId(out newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, entityMarket.GetNbToken_IDPR(Cst.PosRequestTypeEnum.ClosingDay_ControlGroupLevel));

                    if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        break;


                    PosKeepingGenProcessBase posKeepingGenProcessBase;
                    // TradedDerivative : ETD
                    if (entityMarket.isExchangeTradedDerivative)
                    {
                        posKeepingGenProcessBase = new PosKeepingGen_ETD(m_PKGenProcess);
                        posKeepingGenProcessBase.InitializePosRequestByMarket(m_MasterPosRequest, m_LstMarketSubPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                        entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.ExchangeTradedDerivative;
                        codeReturn = posKeepingGenProcessBase.EOD_MarketControl(newIdPR, entityMarket.idEM, posRequestGroupLevel.IdPR, entityMarket.CurrentGroupProduct, entityMarket.marketShortAcronym, entityMarket.idM);
                        newIdPR += entityMarket.Increment_IDPR(Cst.PosRequestTypeEnum.ClosingDay_ControlGroupLevel, entityMarket.CurrentGroupProduct);
                    }

                    if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        break;

                    // CommoditySpot : COMD
                    if (entityMarket.isCommodity)
                    {
                        posKeepingGenProcessBase = new PosKeepingGen_COM(m_PKGenProcess);
                        posKeepingGenProcessBase.InitializePosRequestByMarket(m_MasterPosRequest, m_LstMarketSubPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                        entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.Commodity;
                        codeReturn = posKeepingGenProcessBase.EOD_MarketControl(newIdPR, entityMarket.idEM, posRequestGroupLevel.IdPR, entityMarket.CurrentGroupProduct, entityMarket.marketShortAcronym, entityMarket.idM);
                        newIdPR += entityMarket.Increment_IDPR(Cst.PosRequestTypeEnum.ClosingDay_ControlGroupLevel, entityMarket.CurrentGroupProduct);
                    }

                    if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        break;

                    // MarkToMarket : MTM
                    if (entityMarket.isMarkToMarket)
                    {
                        posKeepingGenProcessBase = new PosKeepingGen_MTM(m_PKGenProcess);
                        posKeepingGenProcessBase.InitializePosRequestByMarket(m_MasterPosRequest, m_LstMarketSubPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                        entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.MarkToMarket;
                        codeReturn = posKeepingGenProcessBase.EOD_MarketControl(newIdPR, entityMarket.idEM, posRequestGroupLevel.IdPR, entityMarket.CurrentGroupProduct, entityMarket.marketShortAcronym, entityMarket.idM);
                        newIdPR += entityMarket.Increment_IDPR(Cst.PosRequestTypeEnum.ClosingDay_ControlGroupLevel, entityMarket.CurrentGroupProduct);
                    }

                    if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        break;

                    // OverTheCounter : OTC
                    if (entityMarket.isOverTheCounter)
                    {
                        posKeepingGenProcessBase = new PosKeepingGen_OTC(m_PKGenProcess);
                        posKeepingGenProcessBase.InitializePosRequestByMarket(m_MasterPosRequest, m_LstMarketSubPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                        entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.OverTheCounter;
                        codeReturn = posKeepingGenProcessBase.EOD_MarketControl(newIdPR, entityMarket.idEM, posRequestGroupLevel.IdPR, entityMarket.CurrentGroupProduct, entityMarket.marketShortAcronym, entityMarket.idM);
                        newIdPR += entityMarket.Increment_IDPR(Cst.PosRequestTypeEnum.ClosingDay_ControlGroupLevel, entityMarket.CurrentGroupProduct);
                    }

                    if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        break;

                    // EquityMarket : SEC
                    if (entityMarket.isSecurity)
                    {
                        posKeepingGenProcessBase = new PosKeepingGen_SEC(m_PKGenProcess);
                        posKeepingGenProcessBase.InitializePosRequestByMarket(m_MasterPosRequest, m_LstMarketSubPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                        entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.Security;
                        codeReturn = posKeepingGenProcessBase.EOD_MarketControl(newIdPR, entityMarket.idEM, posRequestGroupLevel.IdPR, entityMarket.CurrentGroupProduct, entityMarket.marketShortAcronym, entityMarket.idM);
                        newIdPR += entityMarket.Increment_IDPR(Cst.PosRequestTypeEnum.ClosingDay_ControlGroupLevel, entityMarket.CurrentGroupProduct);
                    }
                }
                isNoActivity |= entityMarket.IsNoActivity;
            }
            // STEP 2 : INITIAL MARGIN
            if (isInitialMargin)
            {
                if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                    (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn)))
                    codeReturn = EOD_InitialMarginControl(posRequestGroupLevel.IdPR);
            }
            // EG 20170510 [21153] Return SUCCESS si NoActivity
            // EG [WI762] End of Day processing : Possibility to request processing without initial margin
            // Alimentation du posRequest dans la liste pour contrôle final du statut)
            //UpdatePosRequestGroupLevel(posRequestGroupLevel, m_LstMarketSubPosRequest,
            //    isNoActivity ? ProcessStateTools.StatusEnum.SUCCESS : (Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ? ProcessStateTools.StatusEnum.IRQ : ProcessStateTools.StatusEnum.NONE);
            UpdatePosRequestGroupLevel(posRequestGroupLevel, 
                isNoActivity ? ProcessStateTools.StatusEnum.SUCCESS : (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)? ProcessStateTools.StatusEnum.IRQ:ProcessStateTools.StatusEnum.NONE);
            return posRequestGroupLevel.Status;
        }
        #endregion CLOSINGDAY_Control
        #region CLOSINGDAY_EODFinalGenAfterCRP
        /// <summary>
        /// Euronext Clearing migration for Commodities & Financial Derivatives
        ///  - Mise à jour des dates sur ENTITYMARKET (cas spécial changement de Clearing House - CRP)
        ///  - Il est possible de clôturer définitivement une journée sur un marché en SUCCESS qui a changé de CSS
        ///    même si le traitement de clôture global est en erreur sur les autres marchés
        /// </summary>
        // EG 20240520 [WI930] New
        private void CLOSINGDAY_ValidationAfterCRP()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if ((null != m_LstEntityMarketInfoChangedAfterCRP) && (0 < m_LstEntityMarketInfoChangedAfterCRP.Count))
            {
                IPosRequest posRequestGroupLevel = null;
                try
                {
                    using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS))
                    {
                        try
                        {
                            m_LstEntityMarketInfoChangedAfterCRP.ForEach(entityMarketInfo =>
                            {
                            // INSERTION LOG
                            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5085), 1,
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), entityMarketInfo.IdA_Entity)),
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                                new LogParam(LogTools.IdentifierAndId(entityMarketInfo.IdA_CSS_Identifier, entityMarketInfo.IdA_CSS)),
                                new LogParam(LogTools.IdentifierAndId(entityMarketInfo.IdM_Identifier, entityMarketInfo.IdM)),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));

                                // Insert POSREQUEST GROUP
                                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                                posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.ClosingDay_ClosingReopeningValidationGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                                    ProcessStateTools.StatusEnum.PROGRESS, m_PosRequest.IdPR, m_LstSubPosRequest, null);

                                posRequestGroupLevel.IdPR_PosRequest = m_MasterPosRequest.IdPR;
                                posRequestGroupLevel.IdPR_PosRequestSpecified = true;

                                OpenNewDtMarket(dbTransaction, entityMarketInfo, newIdPR);
                            });
                            if (null != dbTransaction)
                                DataHelper.CommitTran(dbTransaction);

                            codeReturn = Cst.ErrLevel.SUCCESS;
                        }
                        catch (Exception)
                        {
                            DataHelper.RollbackTran(dbTransaction);
                            throw;
                        }
                    }
                }
                catch (Exception)
                {
                    codeReturn = Cst.ErrLevel.FAILURE;
                    throw;
                }
                finally
                {
                    if (null != posRequestGroupLevel)
                    {
                        UpdatePosRequest(codeReturn, posRequestGroupLevel, m_MasterPosRequest.IdPR);
                        m_PKGenProcess.ProcessState.Status = posRequestGroupLevel.Status;
                    }
                    else
                    {
                        m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    }
                }
            }
        }
        #endregion CLOSINGDAY_EODFinalGenAfterCRP
        #region CLOSINGDAY_EODFinalGen
        /// <summary>
        /// Méthode principale de traitement de tenue de position final de cloture de journée
        ///</summary>
        /// <param name="pRowEntityMarkets">Collection d'ENTITE/MARCHE</param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Upd
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20240520 [WI930] Update InitializePosRequestByMarket
        private Cst.ErrLevel CLOSINGDAY_EODFinalGen(DataRowCollection pRowEntityMarkets)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                if (null != pRowEntityMarkets)
                {
                    if (0 < pRowEntityMarkets.Count)
                    {

                        // INSERTION LOG
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5082), 1,
                            new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                            new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));

                       //codeReturn = LoadClosingReopeningAction();

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            m_LstMarketSubPosRequest = new List<IPosRequest>();
                            foreach (DataRow rowEntityMarket in pRowEntityMarkets)
                            {
                                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                    break;

                                PosKeepingGenProcessBase posKeepingGenProcessBase = null;
                                PosKeepingEntityMarket entityMarket = new PosKeepingEntityMarket(CS, rowEntityMarket);

                                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, entityMarket.GetNbToken_IDPR(Cst.PosRequestTypeEnum.ClosingDay_EOD_MarketGroupLevel));

                                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                    break;

                                // TradedDerivative : ETD
                                if (entityMarket.isExchangeTradedDerivative)
                                {
                                    posKeepingGenProcessBase = new PosKeepingGen_ETD(m_PKGenProcess);
                                    posKeepingGenProcessBase.SetListeSourceToTarget(this);
                                    posKeepingGenProcessBase.InitializePosRequestByMarket(m_MasterPosRequest, m_LstMarketSubPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                                    posKeepingGenProcessBase.SetCurrentParallelSettings(entityMarket.entityIdentifier, entityMarket.cssCustodianIdentifier, entityMarket.market);

                                    entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.ExchangeTradedDerivative;
                                    if (Cst.ErrLevel.SUCCESS != posKeepingGenProcessBase.CLOSINGDAY_EODMarketGen(newIdPR, entityMarket.idEM, entityMarket.marketShortAcronym, entityMarket.idM,
                                        entityMarket.idBC, entityMarket.CurrentGroupProduct))
                                        codeReturn = Cst.ErrLevel.FAILURE;
                                    newIdPR++;
                                }

                                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                    break;

                                // CommoditySpot : COM
                                if (entityMarket.isCommodity)
                                {
                                    posKeepingGenProcessBase = new PosKeepingGen_COM(m_PKGenProcess);
                                    posKeepingGenProcessBase.SetListeSourceToTarget(this);
                                    posKeepingGenProcessBase.InitializePosRequestByMarket(m_MasterPosRequest, m_LstMarketSubPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                                    posKeepingGenProcessBase.SetCurrentParallelSettings(entityMarket.entityIdentifier, entityMarket.cssCustodianIdentifier, entityMarket.market);

                                    entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.Commodity;
                                    if (Cst.ErrLevel.SUCCESS != posKeepingGenProcessBase.CLOSINGDAY_EODMarketGen(newIdPR, entityMarket.idEM, entityMarket.marketShortAcronym, entityMarket.idM,
                                        entityMarket.idBC, entityMarket.CurrentGroupProduct))
                                        codeReturn = Cst.ErrLevel.FAILURE;
                                    newIdPR++;
                                }

                                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                    break;

                                // EquityMarket  : SEC
                                if (entityMarket.isSecurity)
                                {
                                    posKeepingGenProcessBase = new PosKeepingGen_OTC(m_PKGenProcess);
                                    posKeepingGenProcessBase.SetListeSourceToTarget(this);
                                    posKeepingGenProcessBase.InitializePosRequestByMarket(m_MasterPosRequest, m_LstMarketSubPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                                    posKeepingGenProcessBase.SetCurrentParallelSettings(entityMarket.entityIdentifier, entityMarket.cssCustodianIdentifier, entityMarket.market);

                                    entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.Security;
                                    if (Cst.ErrLevel.SUCCESS != posKeepingGenProcessBase.CLOSINGDAY_EODMarketGen(newIdPR, entityMarket.idEM, entityMarket.marketShortAcronym, entityMarket.idM,
                                        entityMarket.idBC, entityMarket.CurrentGroupProduct))
                                        codeReturn = Cst.ErrLevel.FAILURE;
                                    newIdPR++;
                                }

                                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                    break;

                                // OverTheCounter : OTC
                                if (entityMarket.isOverTheCounter)
                                {
                                    posKeepingGenProcessBase = new PosKeepingGen_OTC(m_PKGenProcess);
                                    posKeepingGenProcessBase.SetListeSourceToTarget(this);
                                    posKeepingGenProcessBase.InitializePosRequestByMarket(m_MasterPosRequest, m_LstMarketSubPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                                    posKeepingGenProcessBase.SetCurrentParallelSettings(entityMarket.entityIdentifier, entityMarket.cssCustodianIdentifier, entityMarket.market);

                                    entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.OverTheCounter;
                                    if (Cst.ErrLevel.SUCCESS != posKeepingGenProcessBase.CLOSINGDAY_EODMarketGen(newIdPR, entityMarket.idEM, entityMarket.marketShortAcronym, entityMarket.idM,
                                        entityMarket.idBC, entityMarket.CurrentGroupProduct))
                                        codeReturn = Cst.ErrLevel.FAILURE;
                                    newIdPR++;
                                }

                                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                    break;

                                // MarkToMarket : MTM
                                if (entityMarket.isMarkToMarket)
                                {
                                    posKeepingGenProcessBase = new PosKeepingGen_MTM(m_PKGenProcess);
                                    posKeepingGenProcessBase.SetListeSourceToTarget(this);
                                    posKeepingGenProcessBase.InitializePosRequestByMarket(m_MasterPosRequest, m_LstMarketSubPosRequest, m_LstSubPosRequest, m_LstEntityMarketInfoChangedAfterCRP);
                                    posKeepingGenProcessBase.SetCurrentParallelSettings(entityMarket.entityIdentifier, entityMarket.cssCustodianIdentifier, entityMarket.market);

                                    entityMarket.CurrentGroupProduct = ProductTools.GroupProductEnum.MarkToMarket;
                                    if (Cst.ErrLevel.SUCCESS != posKeepingGenProcessBase.CLOSINGDAY_EODMarketGen(newIdPR, entityMarket.idEM, entityMarket.marketShortAcronym, entityMarket.idM,
                                        entityMarket.idBC, entityMarket.CurrentGroupProduct))
                                        codeReturn = Cst.ErrLevel.FAILURE;
                                    newIdPR++;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                throw;
            }
            return codeReturn;
        }
        #endregion CLOSINGDAY_EODFinalGen
        #region CLOSINGDAY_Validation
        /// <summary>
        /// Validation de la clôture de journée pour chaque marché de ENTITY/CSSCUSTODIAN
        ///</summary>
        /// <param name="pRowEntityMarket"></param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20131004 [19027]
        //PM 20150422 [20575] Ajout gestion DtEntity
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180620 Gestion Date de demande de traitement  différente de celle d'ENTITYMARKET (Message + Traitement en WARNING)
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20240520 [WI930] Refactoring avec appel à la méthode : OpenNewDtMarket
        private Cst.ErrLevel CLOSINGDAY_Validation()
        {

            IPosRequest posRequestGroupLevel = null;
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                // INSERTION LOG
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5083), 1,
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));

                // Insert POSREQUEST GROUP
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.ClosingDay_ValidationGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    ProcessStateTools.StatusEnum.PROGRESS, m_PosRequest.IdPR, m_LstSubPosRequest, null);

                posRequestGroupLevel.IdPR_PosRequest = m_MasterPosRequest.IdPR;
                posRequestGroupLevel.IdPR_PosRequestSpecified = true;

                DataRowCollection rowEntityMarkets = ControlAndGetEntityMarkets(ref codeReturn);

                if ((null != rowEntityMarkets) && (Cst.ErrLevel.SUCCESS == codeReturn))
                {
                    using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS))
                    {
                        try
                        {
                            foreach (DataRow rowEntityMarket in rowEntityMarkets)
                            {
                                m_EntityMarketInfo = m_PKGenProcess.ProcessCacheContainer.GetEntityMarketLock(Convert.ToInt32(rowEntityMarket["IDEM"]));
                                OpenNewDtMarket(dbTransaction, m_EntityMarketInfo, newIdPR);
                            }
                            if (null != dbTransaction)
                                DataHelper.CommitTran(dbTransaction);

                            codeReturn = Cst.ErrLevel.SUCCESS;
                        }
                        catch (Exception)
                        {
                            DataHelper.RollbackTran(dbTransaction);
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                throw;
            }
            finally
            {
                if (null != posRequestGroupLevel)
                {
                    UpdatePosRequest(codeReturn, posRequestGroupLevel, m_MasterPosRequest.IdPR);
                    m_PKGenProcess.ProcessState.Status = posRequestGroupLevel.Status;
                }
                else
                {
                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                }
            }
            return codeReturn;
        }
        /// <summary>
        /// Validation final 
        /// - Alimentation de ENTITYMARKETTRAIL
        /// - Mise à jour de ENTITYMARKET
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pPosKeepingMarket"></param>
        /// <param name="pNewIdPR"></param>
        // EG 20240520 [WI930] New
        private void OpenNewDtMarket(IDbTransaction pDbTransaction, IPosKeepingMarket pPosKeepingMarket, int pNewIdPR)
        {
            IOffset offset = m_Product.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.ExchangeBusiness);
            IBusinessDayAdjustments bda = m_Product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING, pPosKeepingMarket.IdBC);
            IBusinessDayAdjustments bdaEntity = m_Product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING,
                String.IsNullOrEmpty(pPosKeepingMarket.IdBCEntity) ? pPosKeepingMarket.IdBCEntity : pPosKeepingMarket.IdBC);

            DateTime newDtMarket = Tools.ApplyOffset(CS, pPosKeepingMarket.DtMarket, offset, bda, null);
            DateTime newDtMarketNext = Tools.ApplyOffset(CS, newDtMarket, offset, bda, null);
            DateTime newDtEntity = Tools.ApplyOffset(CS, m_MasterPosRequest.DtBusiness, offset, bdaEntity, null);
            DateTime newDtEntityNext = Tools.ApplyOffset(CS, newDtEntity, offset, bdaEntity, null);

            // Gestion du cas de jours férié entity, mais pas sur le marché
            DateTime tryDtMarket = newDtMarket;
            while ((tryDtMarket < newDtEntity) && (newDtMarket < newDtEntity) && (newDtMarketNext <= newDtEntity))
            {
                tryDtMarket = newDtMarketNext;
                if (tryDtMarket <= newDtEntity)
                {
                    newDtMarket = newDtMarketNext;
                    newDtMarketNext = Tools.ApplyOffset(CS, newDtMarket, offset, bda, null);
                }
            }

            string sqlEntityMarketTrail;
            string sqlEntityMarket;
            int rowAffected = 0;
            DataParameters dpEntityMarketTrail = new DataParameters();
            DataParameters dpEntityMarket = new DataParameters();

            DateTime dtsys = OTCmlHelper.GetDateSysUTC(CS);

            // La date marché ne doit pas être supérieure à la date entité
            if (newDtMarket > newDtEntity)
            {
                // La date marché reste inchangée, seule la date Entity change : on revient en arrière par rapport aux newDtMarket
                newDtMarketNext = newDtMarket;
                newDtMarket = pPosKeepingMarket.DtMarket;

                // Insert ENTITYMARKETTRAIL (PL 20140414 Newness)
                sqlEntityMarketTrail = @"insert into dbo.ENTITYMARKETTRAIL 
                (IDEM, IDA_CUSTODIAN, DTMARKETCLOSED, DTMARKETOPEN, DTENTITYCLOSED, DTENTITYOPEN, IDA, DTSYS, HOSTNAME, APPNAME, APPVERSION, APPBROWSER, IDPROCESS_L, IDTRK_L,IDPR) 
                select IDEM, IDA_CUSTODIAN, DTMARKETPREV, @DTMARKET, DTENTITY, @DTENTITY, @IDA, @DTSYS, @HOSTNAME, @APPNAME, @APPVERSION, @APPBROWSER, @IDPROCESS_L, @IDTRK_L, @IDPR
                from dbo.ENTITYMARKET
                where (IDEM=@IDEM)";

                sqlEntityMarket = @"update dbo.ENTITYMARKET
                set DTENTITYPREV = DTENTITY, DTENTITY = @DTENTITY, DTENTITYNEXT = @DTENTITYNEXT, DTMARKETNEXT = @DTMARKETNEXT
                where (IDEM=@IDEM)";
            }
            else
            {
                dpEntityMarket.Add(new DataParameter(CS, "DTMARKET", DbType.Date), newDtMarket); // FI 20201006 [XXXXX] DbType.Date

                // Insert ENTITYMARKETTRAIL (PL 20140414 Newness)
                sqlEntityMarketTrail = @"insert into dbo.ENTITYMARKETTRAIL 
                (IDEM, IDA_CUSTODIAN, DTMARKETCLOSED, DTMARKETOPEN, DTENTITYCLOSED, DTENTITYOPEN, IDA, DTSYS, HOSTNAME, APPNAME, APPVERSION, APPBROWSER, IDPROCESS_L, IDTRK_L,IDPR) 
                select IDEM, IDA_CUSTODIAN, DTMARKET, @DTMARKET, DTENTITY, @DTENTITY, @IDA, @DTSYS, @HOSTNAME, @APPNAME, @APPVERSION, @APPBROWSER, @IDPROCESS_L, @IDTRK_L, @IDPR
                from dbo.ENTITYMARKET
                where (IDEM=@IDEM)";

                sqlEntityMarket = @"update dbo.ENTITYMARKET
                set DTENTITYPREV = DTENTITY, DTENTITY = @DTENTITY, DTENTITYNEXT = @DTENTITYNEXT,  DTMARKETPREV = DTMARKET, DTMARKET = @DTMARKET, DTMARKETNEXT = @DTMARKETNEXT
                where (IDEM=@IDEM)";
            }

            dpEntityMarketTrail.Add(new DataParameter(CS, "IDEM", DbType.Int32), pPosKeepingMarket.IdEM);
            dpEntityMarketTrail.Add(new DataParameter(CS, "DTENTITY", DbType.Date), newDtEntity); // FI 20201006 [XXXXX] DbType.Date
            dpEntityMarketTrail.Add(new DataParameter(CS, "DTMARKET", DbType.Date), newDtMarket); // FI 20201006 [XXXXX] DbType.Date

            dpEntityMarketTrail.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDA), m_PKGenProcess.Session.IdA);
            dpEntityMarketTrail.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTSYS), dtsys);

            dpEntityMarketTrail.Add(new DataParameter(CS, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN), m_PKGenProcess.AppInstance.HostName);
            dpEntityMarketTrail.Add(new DataParameter(CS, "APPNAME", DbType.AnsiString, SQLCst.UT_APPNAME_LEN), m_PKGenProcess.AppInstance.AppName);
            dpEntityMarketTrail.Add(new DataParameter(CS, "APPVERSION", DbType.AnsiString, SQLCst.UT_APPVERSION_LEN), m_PKGenProcess.AppInstance.AppNameVersion);
            dpEntityMarketTrail.Add(new DataParameter(CS, "APPBROWSER", DbType.AnsiString, 64), Convert.DBNull);

            dpEntityMarketTrail.Add(new DataParameter(CS, "IDPROCESS_L", DbType.Int32), IdProcess);
            dpEntityMarketTrail.Add(new DataParameter(CS, "IDTRK_L", DbType.Int32), IdTracker);
            dpEntityMarketTrail.Add(new DataParameter(CS, "IDPR", DbType.Int32), pNewIdPR);

            // EG 20240628 [OTRS1000275] New Contrôle DUPKEY if multi CRP instructions for a market
            QueryParameters qryParameters = new QueryParameters(CS, sqlEntityMarketTrail, dpEntityMarketTrail);
            try
            {
                rowAffected += DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }
            catch (Exception ex)
            {
                if (!DataHelper.IsDuplicateKeyError(CS, ex))
                    throw;
            }

            dpEntityMarket.Add(new DataParameter(CS, "IDEM", DbType.Int32), pPosKeepingMarket.IdEM);
            dpEntityMarket.Add(new DataParameter(CS, "DTENTITY", DbType.Date), newDtEntity);         // FI 20201006 [XXXXX] DbType.Date
            dpEntityMarket.Add(new DataParameter(CS, "DTENTITYNEXT", DbType.Date), newDtEntityNext); // FI 20201006 [XXXXX] DbType.Date
            dpEntityMarket.Add(new DataParameter(CS, "DTMARKETNEXT", DbType.Date), newDtMarketNext); // FI 20201006 [XXXXX] DbType.Date


            qryParameters = new QueryParameters(CS, sqlEntityMarket, dpEntityMarket);
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            // Alimentation de la table des jours fériés
            HolidayResultTools.AddHoliday(CS, pDbTransaction, pPosKeepingMarket.IdEM, m_PKGenProcess.Session.IdA, dtsys);
        }
        #endregion CLOSINGDAY_Validation
    }
}

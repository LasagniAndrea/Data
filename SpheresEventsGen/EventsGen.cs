#region Debug Directives
//      ---------------------------------------------------------------------------------------------------------------------
//      SpheresEventGen : Service de génération des événement hérite du projet SpheresCommonEventsGenBase
//      ---------------------------------------------------------------------------------------------------------------------
//      class EventsGenProcess : ProcessTradeBase
//          Cette classe contient entre autre les membres liés au service et au trade (Queue, Tracker, Trade, ...)

//          method ProcessExecuteSpecific()
//          -------------------------------
//          Cette méthode générique du service est le point d'entrée de la génération des événements.

//          Pour debugguer pas à pas ce module mettre un point d'arrêt sur chacune des 2 méthodes suivantes:
//          method ExecuteProduct() pour le calcul des classes de travail
//          method ExecuteTrade() pour la génération des événements

//
//          Le traitement se décompose en 2 étapes :
//
//      ---------------------------------------------------------------------------------------------------------------------
//           1./ Instanciation et alimentation des classes de calculs, sources de données utilisées pour la construction 
//               et la génération des évènements (EFS_CalculationPeriodDates, EFS_PaymentDates ...)
//      ---------------------------------------------------------------------------------------------------------------------
        #region Step 1

//          METHOD ExecuteProduct() :
//          -----------------------
//          Cette méthode récursive redirige les calculs vers les classes adéquates en fonction de la nature de(s) produit(s) présent sur le trade;
//          à savoir :
//
            #region class EventsGenProductCalculationIRD : EventsGenProductCalculationBase
//          method Calculation() : 
//            . BULLET PAYMENT
//            . CAP/FLOOR
//            . FRA
//            . LOAN DEPOSIT
//            . SWAP
//            . SWAPTION
            #endregion
            #region class EventsGenProductCalculationFX : EventsGenProductCalculationBase
//          method Calculation() : 
//            . FX LEG
//            . FX SWAP
//            . FX TERM DEPOSIT
//            . FX AVERAGE OPTION
//            . FX BARRIER OPTION
//            . FX DIGITAL OPTION
//            . FX SIMPLE  OPTION
            #endregion
            #region class EventsGenProductCalculationEQD : EventsGenProductCalculationBase
//          method Calculation() : 
//            . RETURN SWAP
//            . EQUITY OPTION
            #endregion
            #region class EventsGenProductCalculationADM : EventsGenProductCalculationBase
//          method Calculation() : 
//            . INVOICE
//            . COMPLEMENTARY INVOICE
//            . CREDIT NOTE
            #endregion
            #region class EventsGenProductCalculationLSD : EventsGenProductCalculationBase
//          method Calculation() : 
//            . EXCHANGETRADEDERIVATIVEFUTURE
//            . EXCHANGETRADEDERIVATIVEOPTION
            #endregion
//
            #region class EventsGenProductCalculationBase : CommonEventsGen
//          contient les membres communs au traitement
//
//            . ProcessBase m_Process;
//            . string m_ConnectionString;
//            . EFS_TradeLibrary m_tradeLibrary;
//            . DataSetTrade m_DsTrade;
//            . EventProcess m_eventProcess;
            #endregion
        #endregion Step 1
//      ---------------------------------------------------------------------------------------------------------------------
//           2./ Génération et Insertion des événemements
//      ---------------------------------------------------------------------------------------------------------------------
        #region Step 2
//          Cette étape concerne le trade quels que soient les produits qui le constitue
//
//          METHOD ExecuteTrade() :
//          -----------------------
//
            #region class EventsGenTrade : CommonEventsGen
//          method Calculation() : 
//            . OTHER PARTY PAYMENTS
//          method Generation() : 
//            . Génération des événements (Alimentation de la class EFS_Events)
//            . Insertion des événements dans les tables
            #endregion

        #endregion Step 2
//      ---------------------------------------------------------------------------------------------------------------------
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.TradeInformation;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
using EfsML.Interface;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.EventsGen
{
    /// <summary>
    /// 
    /// </summary>
    public class EventsGenProcess : ProcessTradeBase
    {
        #region Members
        private readonly EventsGenMQueue m_EventsGenMQueue;
        private TradeInfo _tradeInfo;
        
        private Dictionary<int, string> m_AssetCacheWithoutMaturityDate;
        // FI 20180919 [23976] add eventsGenTrade
        private readonly EventsGenTrade eventsGenTrade;

        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient le message Queue qui active le process
        /// </summary>
        public EventsGenMQueue EventsGenMQueue
        {
            get { return m_EventsGenMQueue; }
        }
        
        // EG 20190613 [24683] New
        public override int IdEParent_ForOffsettingPosition
        {
            get
            {
                int idEParent = 0;
                try
                {
                    idEParent = (from item in _tradeInfo.tradeLibrary.CurrentTrade.ProductEvents.events
                                 where EventCodeFunc.IsLegGroup(item.eventKey.eventCode)
                                 select item.eventKey.idE).FirstOrDefault();
                }
                catch { }
                return idEParent;
            }
        }

        #endregion Accessors
        #region Constructor
        // EG 20181127 PERF Post RC (Step 3)
        public EventsGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            m_EventsGenMQueue = (EventsGenMQueue)pMQueue;
            NoLockCurrentId = m_EventsGenMQueue.GetBoolValueParameterById(EventsGenMQueue.PARAM_NOLOCKCURRENTID);
            // FI 20180919 [23976] new EventsGenTrade => Only one instance
            eventsGenTrade = new EventsGenTrade(this);
        }
        #endregion Constructor
        #region Methods

        /// <summary>
        /// Alimente les classes de calcul nécessaires à la génération des évènements
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        protected override Cst.ErrLevel ProcessExecuteSimpleProduct(IProduct pProduct)
        {
            SimpleProductProductCalculation(pProduct);
            // RD 20100524 [17004] / Use of MaxDate
            // EG 20120224 Refactoring : Ecriture directe dans la table LOG des Asset sans date de maturité renseignée
            if (pProduct.ProductBase.IsExchangeTradedDerivative)
                LogAssetWithMaxDate(pProduct);
            return Cst.ErrLevel.SUCCESS;
        }

        /// <summary>
        /// Alimente les classes de calcul nécessaires à la génération des évènements
        /// </summary>
        /// <param name="pProduct"></param>
        /// EG 20161122 New Commodity Derivative
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void SimpleProductProductCalculation(IProduct pProduct)
        {
            // Calcul des données de bases en fonction du produit
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 502), 4,
                new LogParam(LogTools.IdentifierAndId(m_EventsGenMQueue.Identifier, CurrentId)),
                new LogParam(pProduct.ProductBase.ProductName)));

            EventsGenProductCalculationBase eventsGenProductCalculation;
            if (pProduct.ProductBase.IsIRD)
                eventsGenProductCalculation = new EventsGenProductCalculationIRD(Cs, _tradeInfo);
            else if ((pProduct.ProductBase.IsFx) || (pProduct.ProductBase.IsFxTermDeposit))
                eventsGenProductCalculation = new EventsGenProductCalculationFX(Cs, _tradeInfo);
            else if (pProduct.ProductBase.IsEQD || pProduct.ProductBase.IsReturnSwap)
                eventsGenProductCalculation = new EventsGenProductCalculationEQD(Cs, _tradeInfo);
            else if (pProduct.ProductBase.IsESE)
                eventsGenProductCalculation = new EventsGenProductCalculationESE(Cs, _tradeInfo);
            else if (pProduct.ProductBase.IsADM)
                eventsGenProductCalculation = new EventsGenProductCalculationADM(Cs, _tradeInfo);
            else if (pProduct.ProductBase.IsSEC)
                eventsGenProductCalculation = new EventsGenProductCalculationSEC(Cs, _tradeInfo);
            else if (pProduct.ProductBase.IsLSD)
                eventsGenProductCalculation = new EventsGenProductCalculationLSD(Cs, _tradeInfo);
            else if (pProduct.ProductBase.IsRISK)
                eventsGenProductCalculation = new EventsGenProductCalculationRISK(Cs, _tradeInfo);
            else if (pProduct.ProductBase.IsBondOption)
                eventsGenProductCalculation = new EventsGenProductCalculationBO(Cs, _tradeInfo);
            else if (pProduct.ProductBase.IsCommodityDerivative)
                eventsGenProductCalculation = new EventsGenProductCalculationCOMD(Cs, _tradeInfo);
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 11), 0,
                    new LogParam(pProduct.ProductBase.ProductName)));

                throw new NotImplementedException("SYS-00004");
            }
            eventsGenProductCalculation.Calculation(this, pProduct);
        }

        /// <summary>
        /// Génère les évènements
        /// </summary>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Use slaveDbTransaction
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            // Chargement du trade
            if (ProcessCall == ProcessCallEnum.Master)
            {
                
                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 500), 0,
                    new LogParam(LogTools.IdentifierAndId(m_EventsGenMQueue.Identifier, CurrentId))));
            }
            
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 501), 4,
                new LogParam(LogTools.IdentifierAndId(m_EventsGenMQueue.Identifier, CurrentId)),
                new LogParam(MQueue.GProduct())));

            _tradeInfo = TradeInfo.LoadTradeInfo(Cs, SlaveDbTransaction, CurrentId, MQueue.GProduct());
            // FI 20180919 [23976] InitLogAttachDelegates
            if (LogDetailEnum >= LogLevelDetail.LEVEL4)
            {
                
                //_tradeInfo.tradeLibrary.InitLogAttachDelegates(processLog.AddAttachedDoc);
                _tradeInfo.tradeLibrary.InitLogAttachDelegates(LogTools.AddAttachedDoc, IdProcess, Session.IdA);
            }

            #region Calculation (EFS_xxx)
            if (ProcessStateTools.IsCodeReturnSuccess(ret))
            {
                ret = ProcessExecuteProduct(_tradeInfo.tradeLibrary.Product);
            }

            if (ProcessStateTools.IsCodeReturnSuccess(ret))
            {
                eventsGenTrade.IsDelEvents = m_EventsGenMQueue.GetBoolValueParameterById(EventsGenMQueue.PARAM_DELEVENTS); 
                
                if (_tradeInfo.tradeLibrary.CurrentTrade.OtherPartyPaymentSpecified)
                {
                    ret = PaymentTools.CalcPayments(Cs, SlaveDbTransaction, _tradeInfo.tradeLibrary.Product,
                        _tradeInfo.tradeLibrary.CurrentTrade.OtherPartyPayment, _tradeInfo.tradeLibrary.DataDocument);

                    bool existsPayment_OnFeeScopeOrderId = false;
                    bool existsPayment_OnFeeScopeFolderId = false;

                    // PL 20191218 [25099] New
                    if (ProcessStateTools.IsCodeReturnSuccess(ret) 
                        && _tradeInfo.tradeLibrary.CurrentTrade.OtherPartyPaymentSpecified
                        && PaymentTools.IsExistsPayment_OnFeeScopeOrderIdOrFolderId_WithMinMax(_tradeInfo.tradeLibrary.CurrentTrade.OtherPartyPayment, 
                                                                                             ref existsPayment_OnFeeScopeOrderId, ref existsPayment_OnFeeScopeFolderId))
                    {
                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        #region Lock de l'ordre et/ou du dossier
                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        string orderId = string.Empty;
                        string folderId = string.Empty;
                        if (existsPayment_OnFeeScopeOrderId)
                        {
                            orderId = _tradeInfo.tradeLibrary.DataDocument.GetOrderId();
                            ret = LockObjectId(TypeLockEnum.TRADE, Cst.FeeScopeEnum.OrderId.ToString() + ":" + orderId, orderId, MQueue.ProcessType.ToString(), LockTools.Exclusive.ToString());
                        }
                        if (existsPayment_OnFeeScopeFolderId && ProcessStateTools.IsCodeReturnSuccess(ret))
                        {
                            folderId = ""; // _tradeInfo.tradeLibrary.dataDocument.GetFolderId(); //GLOP25099 TODO GetFolderId()
                            ret = LockObjectId(TypeLockEnum.TRADE, Cst.FeeScopeEnum.FolderId.ToString() + ":" + folderId, folderId, MQueue.ProcessType.ToString(), LockTools.Exclusive.ToString());
                            ret = Cst.ErrLevel.LOCKUNSUCCESSFUL;
                        }
                        #endregion Lock de l'ordre et/ou du dossier

                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        #region Recalcul des Frais relatifs à Ordre/Dossier avec Min/max
                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        if (ProcessStateTools.IsCodeReturnSuccess(ret))
                        {
                            string CS = this.Cs;

                            //Le flag "isDelEvents=false" est ici utilisé pour déduire qu'il s'agit d'une création de trade
                            //bool isNewTrade = !eventsGenTrade.isDelEvents; --> PL Non fiable !!!
                            //bool isNewTrade = (null == DataHelper.ExecuteScalar(CS, CommandType.Text,
                            //                   String.Format("select count(IDT) from dbo.TRADETRAIL where IDT={0} and ACTION!='{1}'", _tradeInfo.idT, EFS.Permission.PermissionEnum.Create.ToString())));
                            //if (isNewTrade)
                            //***************************************************************************************************************************************
                            //PL 20200117 [25099] IMPORTANT
                            //***************************************************************************************************************************************
                            //La modification d'un trade comportant des frais sur un Scope OrderId/FolderID avec un Min/Max est interdite depuis la saisie des Trades
                            //See: Msg_TradeActionDisabledReason_OPPOnScopeOrderOrFolderWithMinMax in Web Application
                            //***************************************************************************************************************************************
                            {
                                //Pour chaque payment OnFeeScopeOrderIdOrFolderId_WithMinMax opérer un recalcul 
                                //- si le résultat est identique --> RAS
                                //- si le résultat est différent --> MAJ du payment et MAJ du TRADEXML
                                bool isTodoUpdateTradeXML = false;
                                if (existsPayment_OnFeeScopeOrderId)
                                {
                                    ret = New_EventsGenAPI.FeeCalculation_OrderId(CS, this, _tradeInfo.tradeLibrary.DataDocument, new KeyValuePair<int, string>(m_EventsGenMQueue.id, m_EventsGenMQueue.identifier),
                                                                  ref isTodoUpdateTradeXML);
                                    if (ProcessStateTools.IsCodeReturnSuccess(ret) && isTodoUpdateTradeXML)
                                        eventsGenTrade.IsUpdTradeXMLForFees = isTodoUpdateTradeXML;
                                }
                            }
                        }
                        #endregion Recalcul des Frais relatifs à Ordre/Dossier avec Min/max
                    }
                }
            }
            #endregion
            #region Event GENERATION
            // EG 20180503 EventMatrixConstruction (_eventMatrix) in parameter
            if (ProcessStateTools.IsCodeReturnSuccess(ret))
            {
                // FI 20180919 [23976] Set eventsGenTrade.TradeInfo
                eventsGenTrade.TradeInfo = _tradeInfo;
                //PL 20191217 Ligne remontée un peu plus haut... eventsGenTrade.isDelEvents = m_EventsGenMQueue.GetBoolValueParameterById(EventsGenMQueue.PARAM_DELEVENTS);
                ret = eventsGenTrade.Generation();
            }
            #endregion
            return ret;
        }

        /// <summary>
        /// Affiche un message dans le log pour les assets ETD qui n'ont pas de date d'échéance
        /// </summary>
        // RD 20100524 [17004] / Use of MaxDate
        // EG 20120224 Refactoring : Ecriture directe dans la table LOG des Asset sans date de maturité renseignée
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void LogAssetWithMaxDate(IProduct pProduct)
        {
            IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)pProduct;
            EFS_Asset asset = exchangeTradedDerivative.Efs_ExchangeTradedDerivative.AssetETD;
            if (asset.maturityDate == DateTime.MaxValue.Date)
            {
                if (null == m_AssetCacheWithoutMaturityDate)
                    m_AssetCacheWithoutMaturityDate = new Dictionary<int, string>();

                if (!m_AssetCacheWithoutMaturityDate.ContainsKey(asset.idAsset))
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.SYS, 541), 1,
                        new LogParam(LogTools.IdentifierAndId(asset.contractIdentifier, asset.idAsset))));

                    m_AssetCacheWithoutMaturityDate.Add(asset.idAsset, asset.contractIdentifier);
                }
            }
        }
        #endregion Methods
    }
    
    /// <summary>
    /// 
    /// </summary>
    // EG 20180205 [23769] Add isAutoWriteLogDetail
    // EG 20190613 [24683] Use DbTransaction
    public static partial class New_EventsGenAPI
    {
        /// <summary>
        /// Execution of a EventGen process from calling Process
        /// </summary>
        /// <param name="pQueue">Execution parameters</param>
        /// <param name="pCallProcess">Reference at calling Process</param>
        /// <returns></returns>
        // EG 20180503 DisposeFlags initialize
        // EG 20190613 [24683] Set Tracker to Slave + IdEParent_ForOffsettingPosition
        public static ProcessState ExecuteSlaveCall(EventsGenMQueue pQueue, IDbTransaction pDbTransaction, ProcessBase pCallProcess, bool pIsSendMessage_PostProcess)
        {
            //
            EventsGenProcess process = new EventsGenProcess(pQueue, pCallProcess.AppInstance)
            {
                SlaveDbTransaction = pDbTransaction,
                
                ProcessCall = ProcessBase.ProcessCallEnum.Slave,
                
                
                IdProcess = pCallProcess.IdProcess,
                LogDetailEnum = pCallProcess.LogDetailEnum,
                IsSendMessage_PostProcess = pIsSendMessage_PostProcess,
                NoLockCurrentId = true,
                DisposeFlags = new ProcessBaseDisposeFlags(),
                // FI 20190605 [XXXXX] valorisation du tracker
                Tracker = pCallProcess.Tracker,
            };
            process.ProcessStart();
            pQueue.IdEParent_ForOffsettingPosition = process.IdEParent_ForOffsettingPosition;
            return process.ProcessState;
        }

        /// <summary>
        /// Recalcul de certains frais sur un trade (cf. FeesCalculationSettingsMode2)
        /// <para>- Possibilité de conserver les frais forcés manuellement</para>
        /// </summary>
        /// <param name="pFeesCalculationSetting">Définition du périmètre des frais à recalculer (Liste de barèmes)</param>
        /// <param name="pMQueue"></param>
        /// <param name="pProcess"></param>
        /// <param name="pIsPreservedForcedPayments"></param>
        /// <param name="opTradeInput"></param>
        /// <param name="newOpp"></param>
        /// <param name="opModifiedOpp"></param>
        /// <returns></returns>
        // FI 20170323 [XXXXX] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        // PL 20191218 [25099] Refactoring
        public static Cst.ErrLevel FeeCalculation(FeesCalculationSettingsMode2 pFeesCalculationSetting,
            ProcessTradeBase pProcess, bool pIsPreserved_ForcedPayments,
            ref TradeInput opTradeInput, ref List<Pair<IPayment, string>> opAllPreservedAndModifiedPayments, ref IEnumerable<IPayment> opModifiedPayments)
        {
            // Pour l'instant une seule fonctionalité => Recalcul des frais sur certains barêmes
            if ((!pFeesCalculationSetting.feeSheduleSpecified) || (null == pFeesCalculationSetting.feeShedule))
                throw new NotSupportedException("feeShedule is not specified");

            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            opAllPreservedAndModifiedPayments.Clear();
            opModifiedPayments = null;
            string CS = pProcess.Cs;

            // FI 20170323 [XXXXX] lecture Dtsys // PL 20191218 Replaced by new DtSys accessor on EventQuery (see below)
            //DateTime dtSys = OTCmlHelper.GetDateSys(CS); 

            string[] logIdentFeeShedule = (from item in pFeesCalculationSetting.feeShedule 
                                           select LogTools.IdentifierAndId(item.identifier, item.OTCmlId)).ToArray();

            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG,7010),0, 
                new LogParam(LogTools.IdentifierAndId(pFeesCalculationSetting.trade)),
                new LogParam(StrFunc.StringArrayList.StringArrayToStringList(logIdentFeeShedule)),
                new LogParam(pIsPreserved_ForcedPayments.ToString())));


            // FI 20170306 [22225] Modify l'appel à workingTradeInput.SearchAndDeserializeShortForm est-il vraiment nécessaire ?
            Actor.User user = new Actor.User(pProcess.Session.IdA, null, Actor.RoleActor.SYSADMIN);
            TradeInput workingTradeInput = new TradeInput();
            workingTradeInput.SearchAndDeserializeShortForm(CS, null, pFeesCalculationSetting.trade.Key.ToString(), SQL_TableWithID.IDType.Id, user, pProcess.Session.SessionId);

            int[] idFeeShedule = (from item in pFeesCalculationSetting.feeShedule
                                  select item.OTCmlId).ToArray();

            if (workingTradeInput.CurrentTrade.OtherPartyPaymentSpecified)
            {
                //Liste des paiements préservés (Paiements relatifs à un barème qui n'est pas à recalculer et Paiements manuels) 
                List<IPayment> lstPreservedPayment = (from IPayment item in workingTradeInput.CurrentTrade.OtherPartyPayment.
                                                      Where(x => (PaymentTools.GetIdFeeShedule(x).HasValue && !ArrFunc.ExistInArray(idFeeShedule.Cast<IComparable>().ToArray(), (IComparable)PaymentTools.GetIdFeeShedule(x).Value))
                                                                 || PaymentTools.IsManualOpp(x))
                                                      select item).ToList();


                //Ajout dans la liste globale des paiements, de ces paiements préservés avec l'étiquette "PreservedOpp"
                opAllPreservedAndModifiedPayments.AddRange(from item in lstPreservedPayment
                                                           select new Pair<IPayment, string>(item, "PreservedOpp"));

                //Liste des paiements impactés (Paiements relatifs à un barème qui est à recalculer) 
                List<IPayment> lstImpactedPayment = (from IPayment item in workingTradeInput.CurrentTrade.OtherPartyPayment.
                                                     Where(x => (PaymentTools.GetIdFeeShedule(x).HasValue && ArrFunc.ExistInArray(idFeeShedule.Cast<IComparable>().ToArray(), (IComparable)PaymentTools.GetIdFeeShedule(x).Value)))
                                                     select item).ToList();

                if (pIsPreserved_ForcedPayments)
                {
                    List<IPayment> lstForced_ImpactedPayment = (from item in lstImpactedPayment.Where(x => PaymentTools.GetFeeStatus(x).Value == SpheresSourceStatusEnum.Forced)
                                                                select item).ToList();

                    if (lstForced_ImpactedPayment.Count() > 0)
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7018), 0,
                                    new LogParam(LogTools.IdentifierAndId(pFeesCalculationSetting.trade))));

                        pProcess.AddLogFeeInformation(workingTradeInput.DataDocument, lstForced_ImpactedPayment.ToArray());

                        //Ajout complémentaire dans la liste globale des paiements, de ces autres paiements préservés avec l'étiquette "PreservedOpp"
                        opAllPreservedAndModifiedPayments.AddRange(from item in lstForced_ImpactedPayment
                                                                   select new Pair<IPayment, string>(item, "PreservedOpp"));
                    }

                    //Suppression dans la liste des paiements impactés de ces paiements forcés qui sont à préserver.
                    lstImpactedPayment = (from item in lstImpactedPayment.Where(x => PaymentTools.GetFeeStatus(x).Value != SpheresSourceStatusEnum.Forced)
                                          select item).ToList();
                }

                if (lstImpactedPayment.Count > 0)
                {
                    workingTradeInput.CurrentTrade.OtherPartyPayment = null;
                    workingTradeInput.CurrentTrade.OtherPartyPaymentSpecified = false;

                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    //Recalcul Fee And Tax -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    workingTradeInput.RecalculFeeAndTax(CS, null);
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

                    if (workingTradeInput.CurrentTrade.OtherPartyPaymentSpecified)
                    {
                        opTradeInput = workingTradeInput;

                        //Suppression des Id générés via l'appel à RecalculFeeAndTax(...). Ils sont recalculés ultérieurement par une appel à SetPaymentId(...) 
                        foreach (IPayment payment in workingTradeInput.CurrentTrade.OtherPartyPayment)
                            payment.Id = string.Empty;

                        // Liste des nouveaux paiements calculés à partir du paramétrage en vigueur et, le cas échéant, des autres trades exécutés dans le cas de scope OrderId/FolderId 
                        List<IPayment> lstNew_ImpactedPayment = (from IPayment item in workingTradeInput.CurrentTrade.OtherPartyPayment.
                                                                 Where(x => (PaymentTools.GetIdFeeShedule(x).HasValue && ArrFunc.ExistInArray(idFeeShedule.Cast<IComparable>().ToArray(), (IComparable)PaymentTools.GetIdFeeShedule(x).Value)))
                                                                 select item).ToList();

                        foreach (IPayment originalPayment in lstImpactedPayment)
                        {
                            //Recherche d'un paiement de remplacement correspondant à un paiement impacté via la clé: couple {barème/condition}
                            IPayment newPayment = lstNew_ImpactedPayment.FirstOrDefault(x => PaymentTools.GetIdFeeShedule(x) == PaymentTools.GetIdFeeShedule(originalPayment)
                                                                                          && PaymentTools.GetIdFeeMatrix(x) == PaymentTools.GetIdFeeMatrix(originalPayment));

                            //IMPORTANT - Règles en vigueur:
                            // 1/ on ne considère pas un nouveau paiement établi via le paramétrage en vigueur lorsqu'il n'existe pas d'équivalence dans les paiments impactés (ex. cas d'un nouveau barème) 
                            // 2/ on conserve le paiement d'origine lorsqu'il n'existe pas d'équivalence dans les nouveaux paiments établis via le paramétrage en vigueur (ex. cas d'un barème supprimé)
                            // 3/ on conserve le paiement d'origine lorsqu'il existe une équivalence dans les nouveaux paiments établis, mais que le résultat est identique (20200106 [25099] nouvelle règle)
                            bool isKeepOriginalPayment = false;
                            if ((null == newPayment))
                            {
                                //Paiement impacté non trouvé --> ajout complémentaire dans la liste globale des paiements du paiment initial avec l'étiquette "PreservedOpp"
                                isKeepOriginalPayment = true;
                            }
                            else
                            {
                                //Paiement impacté trouvé --> si le paiement impacté est identique au paiement d'origine, on conserve ce dernier afin d'éviter toute modification inutile
                                isKeepOriginalPayment = PaymentTools.IsIdenticalPayment(originalPayment, newPayment);
                            }
                            if (isKeepOriginalPayment)
                            {
                                //WARNING: 20200106 PL Correction d'une anomalie par rapport à la règle 2/ ci-dessus. On ajoutait "newPayment" (donc NULL) plutôt que "originalPayment". Cette correction va donc changer le comportement de Spheres®...
                                opAllPreservedAndModifiedPayments.Add(new Pair<IPayment, string>(originalPayment, "PreservedOpp"));
                            }
                            else
                            {
                                //Ajout complémentaire dans la liste globale des paiements du paiment de remplacement avec l'étiquette "ModifiedOpp"
                                opAllPreservedAndModifiedPayments.Add(new Pair<IPayment, string>(newPayment, "ModifiedOpp"));
                            }
                        }

                        //Constitution d'une liste avec les seuls paiements impactés, remplacés par un nouveau calcul.
                        opModifiedPayments = from item in opAllPreservedAndModifiedPayments.Where(x => x.Second == "ModifiedOpp") select item.First;
                        if (opModifiedPayments.Count() == 0)
                        {
                            //Aucune modification de frais observée
                            // PM 20200701 [XXXXX] New Log
                            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 7014), 0, new LogParam(LogTools.IdentifierAndId(pFeesCalculationSetting.trade))));
                        }
                    }
                    else
                    {
                        // Aucun frais en vigueur ne s'applique au trade => Les frais sont conservés 
                        // PM 20200701 [XXXXX] New Log
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 7013), 0, new LogParam(LogTools.IdentifierAndId(pFeesCalculationSetting.trade))));
                    }
                }
                else
                {
                    codeReturn = Cst.ErrLevel.NOTHINGTODO;

                    // Il n'existe pas de frais portant sur les barèmes {pFeesCalculationSetting.feeShedule} ou tous sont forcés
                    // PM 20200701 [XXXXX] New Log
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7012), 0, new LogParam(LogTools.IdentifierAndId(pFeesCalculationSetting.trade))));
                }
            }
            else
            {
                codeReturn = Cst.ErrLevel.NOTHINGTODO;
                // Il n'existe pas de frais sur le trade => pas de recalcul
                // PM 20200701 [XXXXX] New Log
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7011), 0, new LogParam(LogTools.IdentifierAndId(pFeesCalculationSetting.trade))));
            }

            return codeReturn;
        }

        /// <summary>
        /// Pour chaque payment relatif à un FeeScope "OrderId" avec un "Min/Max" opérer un recalcul 
        /// <para>- si le résultat est identique --> RAS</para>
        /// <para>- si le résultat est différent --> MAJ du payment et MAJ du TRADEXML</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProcess"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pTradeId"></param>
        /// <param name="opIsTodoUpdateTradeXML"></param>
        /// <returns></returns>
        public static Cst.ErrLevel FeeCalculation_OrderId(string pCS, ProcessTradeBase pProcess, DataDocumentContainer pDataDocument, KeyValuePair<int, string> pTradeId, 
                                                  ref bool opIsTodoUpdateTradeXML)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS; 
            
            List<Pair<IPayment, string>> allPayments = new List<Pair<IPayment, string>>();
            IEnumerable<IPayment> modifiedPayments = null;

            bool isForcedFeesPreserved = true;
            List<FeeSheduleId> feeSheduleId = new List<FeeSheduleId>();
            foreach (IPayment payment in pDataDocument.OtherPartyPayment)
            {
                if (payment.Efs_Payment.paymentSource.feeScopeSpecified
                    && (payment.Efs_Payment.paymentSource.feeScope == Cst.FeeScopeEnum.OrderId)
                    && (
                         (payment.Efs_Payment.paymentSource.formulaMinSpecified && payment.Efs_Payment.paymentSource.formulaMin != Cst.NotAvailable)
                         ||
                         (payment.Efs_Payment.paymentSource.formulaMaxSpecified && payment.Efs_Payment.paymentSource.formulaMax != Cst.NotAvailable)
                       )
                    )
                    feeSheduleId.Add(new FeeSheduleId(new KeyValuePair<int, string>(payment.Efs_Payment.paymentSource.idFeeSchedule, payment.Efs_Payment.paymentSource.feeScheduleIdentifier)));
            }
            FeesCalculationSettingsMode2 feesCalculationSetting = new FeesCalculationSettingsMode2
            {
                feeSheduleSpecified = true,
                feeShedule = feeSheduleId.ToArray(),
                trade = pTradeId
            };

            //PL 20191218 [25099] Add reference to TradeCaptureGen, CommonCaptureGen and EfsMLGUI for use TradeInput 
            TradeInput workingTradeInput = new TradeInput();
            codeReturn = New_EventsGenAPI.FeeCalculation(feesCalculationSetting,
                                                                      pProcess, isForcedFeesPreserved,
                                                                      ref workingTradeInput, ref allPayments, ref modifiedPayments);
            if (ProcessStateTools.IsCodeReturnNothingToDo(codeReturn))
                codeReturn = Cst.ErrLevel.SUCCESS;
            
            if (ProcessStateTools.IsCodeReturnSuccess(codeReturn) && (modifiedPayments.Count() > 0))
            {
                IEnumerable<IPayment> allPayments_Sorted = ((from item in allPayments.Where(x => x.Second == "PreservedOpp")
                                                             select item.First).Concat(from item in modifiedPayments
                                                                                       select item)).ToArray();
                // Mise en place des Id uniquement sur les frais modifiés
                // Il ne faut pas mettre des Id sur les frais préservés (ceux-ci conservent leur Id initial qui par ailleurs peut être renseigné ou pas)  
                // => il ne faut pas mettre un Id sur un frais préservé sur lequel l'Id est non renseigné. Pour ce frais EVENTFEE.PAYMENTID est non renseigné.  
                Tools.SetPaymentId(modifiedPayments.ToArray(), "OPP", Tools.GetMaxIndex(allPayments_Sorted.ToArray(), "OPP") + 1);

                opIsTodoUpdateTradeXML = true;
                int nbEvent_Filler = 0;
                IPayment[] allPayments_Prepared = EventQuery.PrepareFeeEvents(pCS, workingTradeInput.Product.Product, workingTradeInput.DataDocument, pTradeId.Key, allPayments_Sorted.ToArray(), ref nbEvent_Filler);
                pDataDocument.SetOtherPartyPayment(allPayments_Prepared);
            }

            return codeReturn;
        }
    }
}